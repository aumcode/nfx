using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using NFX;
using NFX.Log;
using NFX.DataAccess;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.CRUD.Subscriptions;
using NFX.Erlang;


namespace NFX.DataAccess.Erlang
{
  /// <summary>
  /// Implements subscriptions to Erlang CRUD data stores
  /// </summary>
  public class ErlCRUDSubscription : Subscription
  {
    internal ErlCRUDSubscription(ErlDataStore store,
                                 string name,
                                 Query query,
                                 Mailbox recipient,
                                 object correlate = null
                                 ) : base(store, name, query, recipient, correlate)
    {
       m_ErlBox = store.MakeMailbox();
       m_Queue  = new ConcurrentQueue<IQueable>();

       m_ErlBox.MailboxMessage += (_, msg) =>
                                  {
                                    m_Queue.Enqueue(msg);
                                    asyncProcess();
                                    return true;
                                  };

       m_Store = store;
       m_Query = query;
       subscribeCore();
       HasLoaded();
    }

    internal void Subscribe()
    {
      if (!IsLoaded || !IsValid)
        return;
      subscribeCore();
    }

    private int  m_TaskLock;

    private void asyncProcess()
    {
      if (Interlocked.CompareExchange(ref m_TaskLock, 1, 0) == 1)
        return;

      Task.Run
      (
        () =>
        {
          while (true)
          {
            try
            {
              IQueable msg;
              while (m_Queue.TryDequeue(out msg))
                process(msg);
            }
            finally
            {
              Thread.VolatileWrite(ref m_TaskLock, 0);
            }

            if (m_Queue.IsEmpty) return;

            if (Interlocked.CompareExchange(ref m_TaskLock, 1, 0) == 1)
              return;
          }
        }
      );
    }

    private void subscribeCore()
    {
      try
      {
         var handler = m_Store.QueryResolver.Resolve(m_Query);
         var result = handler.ExecuteWithoutFetch( new ErlCRUDQueryExecutionContext(m_Store, m_ErlBox, m_LastTimeStamp), m_Query);
      }
      catch(Exception error)
      {
         var invalid = error as InvalidSubscriptionRequestException;
         if (invalid!=null)
           Invalidate(invalid);
         else
          if (error is ErlConnectionException)
          {
            //eat as it may reconnect
          }
          else throw;
      }
    }

    protected override void Destructor()
    {
       DisposableObject.DisposeAndNull(ref m_ErlBox);
 	     base.Destructor();
    }

    private DataTimeStamp m_LastTimeStamp = new DataTimeStamp(0);
    private ErlDataStore m_Store;
    private Query m_Query;
    internal ErlMbox m_ErlBox;
    private ConcurrentQueue<IQueable> m_Queue;

    private static readonly IErlObject SUBSCRIPTION_MSG_PATTERN =
                   "{Schema::atom(), Timestamp::long(), Type, Rows::list()}".ToErlObject();

    private static readonly ErlAtom SCHEMA    = new ErlAtom("Schema");
    private static readonly ErlAtom TIMESTAMP = new ErlAtom("Timestamp");
    private static readonly ErlAtom TYPE      = new ErlAtom("Type");
    private static readonly ErlAtom ROWS      = new ErlAtom("Rows");

    private void process(IQueable msg)
    {
      try
      {
        if (msg is ErlException)
        {
          this.Mailbox.DeliverError(this, (ErlException)msg);
          return;
        }
        var erlMsg  = msg as ErlMsg;

        if (erlMsg == null)
          throw new ErlException("Invalid message type: " + msg.GetType());

        var map     = ((ErlDataStore)Store).Map;
        var binding = new ErlVarBind();

        //{Schema, TS, ChangeType :: $d | $w, [{schema, Flds...}]} %% delete, write(upsert)
        //{Schema, TS, ChangeType :: $D | $C | $c, []}             %% Drop, Create, Clear
        if (erlMsg.Msg==null ||
           !erlMsg.Msg.Match(SUBSCRIPTION_MSG_PATTERN, binding))
          return;

        var schemaName = ((ErlAtom)binding[SCHEMA]).Value;
        var ts         = binding[TIMESTAMP].ValueAsLong;
        var op         = (char)((ErlByte)binding[TYPE]).Value;
        var rows       = ((ErlList)binding[ROWS]).Value;

        var schema = map.GetCRUDSchemaForName(schemaName);


        m_LastTimeStamp = new DataTimeStamp(ts);

        CRUDSubscriptionEvent.EventType etp;

        switch(op)
        {
          case 'd': etp = CRUDSubscriptionEvent.EventType.RowDelete; break;
          case 'c': etp = CRUDSubscriptionEvent.EventType.TableClear; break;
          case 'D': etp = CRUDSubscriptionEvent.EventType.TableDrop; break;
          case 'C': etp = CRUDSubscriptionEvent.EventType.TableCreate; break;
          default:  etp = CRUDSubscriptionEvent.EventType.RowUpsert; break;
        }

        if (rows.Count>0)
        {
          int errors = 0;
          foreach(var rowTuple in rows.Cast<ErlTuple>())
          {
            try
            {
              var row  = map.ErlTupleToRow(schemaName, rowTuple, schema);
              var data = new CRUDSubscriptionEvent(etp, schema, row, m_LastTimeStamp);
              this.Mailbox.Deliver(this, data);
            }
            catch(Exception ie)
            {
              errors++;
              log(ie);
            }
          }

          // TODO: Add error reporting to user
        }
        else
        {  //used to clear data, no rows are fetched
          var data = new CRUDSubscriptionEvent(etp, schema, null, m_LastTimeStamp);
          this.Mailbox.Deliver(this, data);
        }
      }
      catch(Exception err)
      {
        log(err);
      }
    }

    private void log(Exception err)
    {
      var trace = new StackTrace(err, true);
      var frame = trace.GetFrame(0);
      var file  = Path.GetFileName(frame.GetFileName());
      var line  = frame.GetFileLineNumber();

      log(MessageType.Error, null, err.Message, err, file, line);
    }

    private void log(MessageType type, string from, string text, Exception error,
                     [CallerFilePath]  string file = null,
                     [CallerLineNumber]int    line = 0,
                     object pars = null)
    {
      App.Log.Write(
        new Message(pars, file, line)
        {
          Type      = type,
          Topic     = CoreConsts.ERLANG_TOPIC,
          From      = error == null ? "{0}.{1}".Args(GetType().Name, from) : error.ToMessageWithType(),
          Text      = text,
          Exception = error
        }
      );
    }
  }
}
