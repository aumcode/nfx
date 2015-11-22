using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using NFX.Environment;
using NFX.ApplicationModel;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.CRUD.Subscriptions;
using NFX.ServiceModel;
using NFX.Erlang;


namespace NFX.DataAccess.Erlang
{
  /// <summary>
  /// Represents a CRUD data store that uses Erlang backend
  /// </summary>
  public class ErlDataStore : ServiceWithInstrumentationBase<object>, ICRUDDataStoreImplementation, ICRUDSubscriptionStoreImplementation  
  {
    #region CONSTS
      public const string ERL_FILE_SUFFIX = ".erl.qry";
      public const string DEFAULT_TARGET_NAME = "ERLANG";

      public static readonly ErlAtom NFX_CRUD_MOD      = new ErlAtom("nfx_crud");
      public static readonly ErlAtom NFX_RPC_FUN       = new ErlAtom("rpc");
      public static readonly ErlAtom NFX_SUBSCRIBE_FUN = new ErlAtom("subscribe");
      public static readonly ErlAtom NFX_WRITE_FUN     = new ErlAtom("write");
      public static readonly ErlAtom NFX_DELETE_FUN    = new ErlAtom("delete");
      public static readonly ErlAtom NFX_BONJOUR_FUN   = new ErlAtom("bonjour");

      public static readonly IErlObject BONJOUR_OK_PATTERN =
           ErlObject.Parse("{bonjour, InstanceID::int(), {ok, SchemaContent::binary()}}");
      
      public static readonly IErlObject CRUD_WRITE_OK_PATTERN  = ErlObject.Parse("{ok, Affected::int()}");

      public static readonly ErlAtom AFFECTED     = new ErlAtom("Affected");
    #endregion

    #region .ctor
      public ErlDataStore() : this(null) {}
      
      public ErlDataStore(object director) : base(director) 
      {
        m_QueryResolver = new QueryResolver(this);

        m_InstanceID = NFX.ExternalRandomGenerator.Instance.NextRandomUnsignedInteger;
      }

      protected override void Destructor()
      {
        base.Destructor();
      }
    #endregion


    #region Fields
    
      private uint m_InstanceID;

      private bool m_InstrumentationEnabled;
      private string m_TargetName = DEFAULT_TARGET_NAME;

      private QueryResolver m_QueryResolver;

      internal ErlLocalNode m_ErlNode;
      
      
      private object m_MapSync = new object();
      private volatile SchemaMap m_Map;

      private ErlAtom m_RemoteName;
      private ErlAtom m_RemoteCookie;


      private Registry<Subscription> m_Subscriptions = new Registry<Subscription>();
      private Registry<Mailbox> m_Mailboxes = new Registry<Mailbox>();

    #endregion


    #region Properties
    
       public string ScriptFileSuffix     { get{ return ERL_FILE_SUFFIX; }}
       public CRUDDataStoreType StoreType { get{ return CRUDDataStoreType.Hybrid; }}
       public bool SupportsTrueAsynchrony { get{ return false; }}
       public bool SupportsTransactions   { get{ return true; }}


      [Config(Default=false)]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
      public override bool InstrumentationEnabled{ get{return m_InstrumentationEnabled;} set{m_InstrumentationEnabled = value;}}


      [Config]
      public string TargetName
      {
        get { return m_TargetName;}
        set
        {
          CheckServiceInactive();
         
          if (value.IsNullOrWhiteSpace())
            value = DEFAULT_TARGET_NAME;

          m_TargetName = value;
        }
      }

      [Config]
      public StoreLogLevel LogLevel{ get; set;}


      [Config]
      public string RemoteName
      {
        get { return m_RemoteName!=null ? m_RemoteName.ToString() : string.Empty;}
        set
        {
          CheckServiceInactive();
          m_RemoteName = value.IsNullOrWhiteSpace() ? null : new ErlAtom(value);
        }
      }

      [Config]
      public string RemoteCookie
      {
        get { return m_RemoteCookie!=null ? m_RemoteCookie.ToString() : string.Empty;}
        set
        {
          CheckServiceInactive();
          m_RemoteCookie = value.IsNullOrWhiteSpace() ? null : new ErlAtom(value);
        }
      }

      public IRegistry<Subscription> Subscriptions
      {
        get { return m_Subscriptions; }
      }

      public IRegistry<Mailbox> Mailboxes
      {
        get { return m_Mailboxes; }
      }

      public ICRUDQueryResolver QueryResolver
      {
        get { return m_QueryResolver; }
      }

    #endregion



    #region Public 
    
      public void TestConnection()
      {
        CheckServiceActive();
        throw new NotImplementedException();
      }

      public Subscription Subscribe(string name, Query query, Mailbox recipient)
      {
        CheckServiceActive();
        return new ErlCRUDSubscription(this, name, query, recipient);
      }

      public Mailbox OpenMailbox(string name)
      {
        CheckServiceActive();
        return new ErlCRUDMailbox(this, name);
      }

      public ICRUDQueryHandler MakeScriptQueryHandler(QuerySource querySource)
      {
        CheckServiceActive();
        return new ErlCRUDScriptQueryHandler(this, querySource);
      }

      public Schema GetSchema(Query query)
      {
        CheckServiceActive();
        var handler = QueryResolver.Resolve(query);
        return handler.GetSchema(new ErlCRUDQueryExecutionContext(this), query);
      }

      public Task<Schema> GetSchemaAsync(Query query)
      {
        CheckServiceActive();
        var handler = QueryResolver.Resolve(query);
        return handler.GetSchemaAsync(new ErlCRUDQueryExecutionContext(this), query);
      }

      public virtual List<RowsetBase> Load(params Query[] queries)
      {
        CheckServiceActive();
        var result = new List<RowsetBase>();
        if (queries==null) return result;

        foreach(var query in queries)
        {
            var handler = QueryResolver.Resolve(query);
            var rowset = handler.Execute( new ErlCRUDQueryExecutionContext(this), query, false);
            result.Add( rowset );
        }

        return result;
      }

      public virtual Task<List<RowsetBase>> LoadAsync(params Query[] queries)
      {
        CheckServiceActive();
        return TaskUtils.AsCompletedTask( () => this.Load(queries) );
      }

      public virtual RowsetBase LoadOneRowset(Query query)
      {
        CheckServiceActive();
        return Load(query).FirstOrDefault();
      }

      public virtual Task<RowsetBase> LoadOneRowsetAsync(Query query)
      {
        CheckServiceActive();
        return this.LoadAsync(query)
                   .ContinueWith( antecedent => antecedent.Result.FirstOrDefault());
      }

      public virtual Row LoadOneRow(Query query)
      {
        CheckServiceActive();
        return LoadOneRowset(query).FirstOrDefault();
      }

      public virtual Task<Row> LoadOneRowAsync(Query query)
      {
        CheckServiceActive();
        return TaskUtils.AsCompletedTask( () => this.LoadOneRow(query) );
      }

      public virtual int Save(params RowsetBase[] rowsets)
      {
        CheckServiceActive();
        if (rowsets==null) return 0;

        var affected = 0;

        foreach(var rset in rowsets)
        {
            foreach(var change in rset.Changes)
            {
                switch(change.ChangeType)
                {
                    case RowChangeType.Insert: affected += Insert(change.Row); break;
                    case RowChangeType.Update: affected += Update(change.Row, change.Key); break;
                    case RowChangeType.Upsert: affected += Upsert(change.Row); break;
                    case RowChangeType.Delete: affected += Delete(change.Row, change.Key); break;
                }
            }
        }

        return affected;
      }

      public virtual Task<int> SaveAsync(params RowsetBase[] rowsets)
      {
        CheckServiceActive();
        return TaskUtils.AsCompletedTask( () => this.Save(rowsets) );
      }

      public virtual int ExecuteWithoutFetch(params Query[] queries)
      {
        CheckServiceActive();
        throw new NotImplementedException();
      }

      public virtual Task<int> ExecuteWithoutFetchAsync(params Query[] queries)
      {
        CheckServiceActive();
        throw new NotImplementedException();
      }

      public virtual int Insert(Row row)
      {
        CheckServiceActive();
        return CRUDWrite(row);
      }

      public virtual Task<int> InsertAsync(Row row)
      {
        CheckServiceActive();
        return TaskUtils.AsCompletedTask( () => Insert(row) );
      }

      public virtual int Upsert(Row row)
      {
        CheckServiceActive();
        return CRUDWrite(row);
      }

      public virtual Task<int> UpsertAsync(Row row)
      {
        CheckServiceActive();
        return TaskUtils.AsCompletedTask( () => Upsert(row) );
      }

      public virtual int Update(Row row, IDataStoreKey key = null)
      {
        CheckServiceActive();
        return CRUDWrite(row);
      }

      public virtual Task<int> UpdateAsync(Row row, IDataStoreKey key = null)
      {
        CheckServiceActive();
        return TaskUtils.AsCompletedTask( () => Update(row) );
      }

      public virtual int Delete(Row row, IDataStoreKey key = null)
      {
        CheckServiceActive();
        return CRUDWrite(row, true);
      }

      public virtual Task<int> DeleteAsync(Row row, IDataStoreKey key = null)
      {
        CheckServiceActive();
        return TaskUtils.AsCompletedTask( () => Delete(row) );
      }

      public virtual CRUDTransaction BeginTransaction(IsolationLevel iso = IsolationLevel.ReadCommitted, TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose)
      {
        CheckServiceActive();
        throw new NotImplementedException();
      }

      public virtual Task<CRUDTransaction> BeginTransactionAsync(IsolationLevel iso = IsolationLevel.ReadCommitted, TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose)
      {
        CheckServiceActive();
        throw new NotImplementedException();
      }
    #endregion


    #region Protected

      protected override void DoStart()
      {
        if (m_RemoteName.ValueAsString.IsNullOrWhiteSpace())
          throw new ErlDataAccessException(StringConsts.ERL_DS_START_REQ_ERROR);
        
        if (ErlApp.Node!=null)
        {
          m_ErlNode = ErlApp.Node;
        }
        else
        {
          m_ErlNode = new ErlLocalNode(ErlLocalNode.MakeLocalNodeForThisAppOnThisHost(), new ErlAtom(m_RemoteCookie));
          m_ErlNode.Start();
        }
      }

      protected override void DoSignalStop()
      {
        if (m_ErlNode!=ErlApp.Node) 
          m_ErlNode.SignalStop();
      }

      protected override void DoWaitForCompleteStop()
      {
        foreach(var mbox in m_Mailboxes) mbox.Dispose();
        foreach(var subs in m_Subscriptions) subs.Dispose();

        m_Mailboxes.Clear();
        m_Subscriptions.Clear();
        
        if (m_ErlNode!=ErlApp.Node) 
          DisposableObject.DisposeAndNull(ref m_ErlNode);
        
        m_Map = null;
      }


      private static int s_RequestID;

      /// <summary>
      /// Generates nest sequential request ID
      /// </summary>
      protected internal int NextRequestID
      {
        get { return Interlocked.Increment(ref s_RequestID); }
      }

      /// <summary>
      /// Returns the map lazily obtaining it when needed
      /// </summary>
      protected internal SchemaMap Map
      {
        get
        {
          var result = m_Map;
          if (result!=null) return result;

          lock(m_MapSync)
          {
            result = m_Map;
            if (result!=null) return result;

            var bonjour = executeRPC(NFX_CRUD_MOD, 
                                     NFX_BONJOUR_FUN, 
                                     new ErlList()
                                     {
                                       new ErlLong(m_InstanceID), //InstanceID
                                       new ErlString(App.Name),   // Application name from app container config root
                                       new ErlString(App.Session.User.Name),//Current user name
                                     }) as ErlTuple;

            if (bonjour==null)
              throw new ErlDataAccessException(StringConsts.ERL_DS_INVALID_RESPONSE_PROTOCOL_ERROR+"Bonjour==null");
           
            var bind = bonjour.Match(BONJOUR_OK_PATTERN);
            if (bind!=null)
            {
               var instID = bind["InstanceID"].ValueAsLong;
               
               if (instID!=m_InstanceID)
                throw new ErlDataAccessException(StringConsts.ERL_DS_INVALID_RESPONSE_PROTOCOL_ERROR+"Bonjour(InstanceId mismatch)");

               var xmlContent = bind["SchemaContent"].ValueAsString;
               m_Map = new SchemaMap(this, xmlContent);
               return m_Map;
            }
            else 
             throw new ErlDataAccessException(StringConsts.ERL_DS_INVALID_RESPONSE_PROTOCOL_ERROR+"Bonjour(!ok)");
          }
        }
      }

      protected internal IErlObject ExecuteRPC(ErlAtom module, ErlAtom func, ErlList args, ErlMbox mbox = null)
      {
         var map = Map;
         return executeRPC(module, func, args, mbox);
      }
     

      protected virtual int CRUDWrite(Row row, bool delete = false)
      {
        var rowTuple = m_Map.RowToErlTuple( row, delete );
        var rowArgs = new ErlList();
        
        rowArgs.Add( rowTuple );

        // nfx_crud:write({secdef, {}, ...})
        var result = this.ExecuteRPC(NFX_CRUD_MOD, delete ? NFX_DELETE_FUN : NFX_WRITE_FUN,  rowArgs);
        
        if (result==null)
         throw new ErlDataAccessException(StringConsts.ERL_DS_INVALID_RESPONSE_PROTOCOL_ERROR+"CRUDWrite==null");

        var bind = result.Match(CRUD_WRITE_OK_PATTERN);

        if (bind==null)
         throw new ErlDataAccessException(StringConsts.ERL_DS_CRUD_WRITE_FAILED_ERROR + result.ToString());

        return bind[AFFECTED].ValueAsInt;
      }

    #endregion


    #region .pvt

      private IErlObject executeRPC(ErlAtom module, ErlAtom func, ErlList args, ErlMbox mbox = null)
      {
        var mowner = mbox==null;
        if (mowner) mbox = m_ErlNode.CreateMbox();
        try
        {
          return mbox.RPC(m_RemoteName, module, func, args);
        }
        catch(Exception error)
        {
          throw new ErlDataAccessException(StringConsts.ERL_DS_RPC_EXEC_ERROR.Args(
                                              "{0}:{1}({2})".Args(module, func, args.ToString().TakeFirstChars(20)), 
                                              error.ToMessageWithType(), error));
        }
        finally
        {
          if (mowner) mbox.Dispose();
        }
      }
    #endregion  

  

      
  }
}
