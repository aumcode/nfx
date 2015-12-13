using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.ApplicationModel;

namespace NFX.DataAccess.CRUD.Subscriptions
{
  
  /// <summary>
  /// Invoked when remote datastore sends an update
  /// </summary>
  public delegate void SynchronizedDatasetReceiptHandler(SynchronizedDataset sender, SynchronizedReceiptHandlerEventArgs args);

  /// <summary>
  /// Invoked when remote datastore sends an error
  /// </summary>
  public delegate void SynchronizedDatasetErrorHandler(SynchronizedDataset sender, Exception error);
  
  /// <summary>
  /// Contains parameters describibg the remnote datastore update
  /// </summary>
  public class SynchronizedReceiptHandlerEventArgs : EventArgs
  {
    public enum EventPhase{Before, After}
    
    internal SynchronizedReceiptHandlerEventArgs(
                  EventPhase phase, 
                  Table table,
                  CRUDSubscriptionEvent.EventType et,
                  Row row)
    {
      this.Phase = phase;
      this.Table = table;
      this.EventType = et;
      this.Row = row;
      this.Proceed = true;
    }

    public readonly EventPhase Phase;
    public readonly Table Table;
    public readonly CRUDSubscriptionEvent.EventType EventType;
    public readonly Row Row;

    /// <summary>
    /// Set to false in the Before phase to stop the processing
    /// </summary>
    public bool Proceed{ get; set;}
  }

  
  /// <summary>
  /// Represents a local dataset that gets synchronized (one way) with the remote dataset 
  /// via CRUD subscription. This class is thread-safe unless stated otherwise on the member level
  /// </summary>
  public class SynchronizedDataset : ApplicationComponent, INamed
  {
      private struct table : INamed
      {
        public table(Table table, Query query)
        {
          m_Table = table;
          m_Name = table.Schema.Name;
          m_Query = query;
        }

        private string m_Name;
        private Table m_Table;
        private Query m_Query;

        public Table Table{ get{ return m_Table;}}
        public string Name{ get{ return m_Name;}}
        public Query Query{ get{ return m_Query;}}
      }
    
    
    public SynchronizedDataset(ICRUDSubscriptionStore store, string name) : base(store)
    {
      if (store==null)
        throw new CRUDException(StringConsts.ARGUMENT_ERROR+GetType().Name+".ctor(store==null)");
      
      if (name.IsNullOrWhiteSpace())
       name = Guid.NewGuid().ToString();

      m_Store = store;
      m_Name = name;
      m_Mailbox = m_Store.OpenMailbox("{0}-{1}".Args(name, Guid.NewGuid().ToString()));
      m_Mailbox.Receipt += m_Mailbox_Receipt;
    }

    protected override void Destructor()
    {
      m_Mailbox.Dispose();
      base.Destructor();
    }
    
    private string m_Name;
    private ICRUDSubscriptionStore m_Store;
    private Mailbox m_Mailbox;
    private Registry<table> m_Tables = new Registry<table>();
     

    
    public string                 Name { get{ return m_Name;}}
    public ICRUDSubscriptionStore Store{ get{ return m_Store;}}

    /// <summary>
    /// The event is called under the exclusive lock on the table.
    /// It has to be efficient (do not block for long)
    /// </summary>
    public event SynchronizedDatasetReceiptHandler Receipt;

    /// <summary>
    /// Called when error occurs
    /// </summary>
    public event SynchronizedDatasetErrorHandler Error;

    /// <summary>
    /// Returns snapshot of queries and resulting shemas/tables contained in the dataset
    /// </summary>
    public IEnumerable<KeyValuePair<Query, Schema>> Data
    {
      get{ return m_Tables.Select(t => new KeyValuePair<Query, Schema>(t.Query, t.Table.Schema) ); }
    }

    /// <summary>
    /// Create a clone of the named table or null if table is not in the dataset
    /// </summary>
    public Table CloneTable(string name)
    {
      var table = m_Tables[name].Table;
      if (table!=null)
        lock(table) return new Table(table);
      return null;
    }

    /// <summary>
    /// Create a clone of the named table as rowset or null if table is not in the dataset
    /// </summary>
    public Rowset CloneRowset(string name)
    {
      var table = m_Tables[name].Table;
      if (table!=null)
        lock(table) return new Rowset(table);
      return null;
    }

    /// <summary>
    /// Adds table identified by query to synchronized dataset 
    /// </summary>
    public void AddTable(Query query)
    {
      var subs = m_Store.Subscribe("{0}-{1}".Args(m_Name, query.Name), query, m_Mailbox);
    }


    protected bool OnReceipt(SynchronizedReceiptHandlerEventArgs.EventPhase phase, Table table, CRUDSubscriptionEvent.EventType eType, Row data)
    {
      if (Receipt!=null)
      {
         var args = new SynchronizedReceiptHandlerEventArgs(phase, table, eType, data);
         Receipt(this, args);
         return args.Proceed;
      }
      return true;
    }

    protected void OnError(Exception error)
    {
      if (Error!=null)
         Error(this, error);
    }

    private void m_Mailbox_Receipt(Subscription subscription, Mailbox recipient, CRUDSubscriptionEvent data, Exception error)
    {
      if (error!=null)
      {
        OnError(error);
        return;
      }
      
      var schema = data.Schema;
      var table = m_Tables.GetOrRegister(schema.Name, (s) => new table(new Table(s), subscription.Query), schema).Table;
      lock(table)
      {
        var proceed = OnReceipt(SynchronizedReceiptHandlerEventArgs.EventPhase.Before, table, data.Type, data.Row);

        if (!proceed) return;

        switch(data.Type)
        {
          case CRUDSubscriptionEvent.EventType.TableCreate: break;
          case CRUDSubscriptionEvent.EventType.TableClear: table.DeleteAll(); break;
          case CRUDSubscriptionEvent.EventType.TableDrop: m_Tables.Unregister(schema.Name); break;
          
          case CRUDSubscriptionEvent.EventType.RowInsert: table.Insert(data.Row); break;
          case CRUDSubscriptionEvent.EventType.RowUpdate: table.Update(data.Row); break;
          case CRUDSubscriptionEvent.EventType.RowUpsert: table.Upsert(data.Row); break;
          case CRUDSubscriptionEvent.EventType.RowDelete: table.Delete(data.Row); break;
        }

        OnReceipt(SynchronizedReceiptHandlerEventArgs.EventPhase.After, table, data.Type, data.Row);
      }
    }
  }
}
