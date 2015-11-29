using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                                 Mailbox recipient) : base(store, name, query, recipient)
    {
       m_ErlBox = store.m_ErlNode.CreateMbox(name);

       m_ErlBox.MailboxMessage += (_, msg) => 
                                  {
                                    Task.Run( () => process(msg) );
                                    return true;
                                  };

       var handler = store.QueryResolver.Resolve(query);
       var result = handler.ExecuteWithoutFetch( new ErlCRUDQueryExecutionContext(store, m_ErlBox), query);
    }

    

    protected override void Destructor()
    {
       DisposableObject.DisposeAndNull(ref m_ErlBox);
 	     base.Destructor();
    }

    internal ErlMbox m_ErlBox;


    private static readonly IErlObject SUBSCRIPTION_MSG_PATTERN = 
                   "{Schema::atom(), Type, Rows::list()}".ToErlObject();

    private static readonly ErlAtom SCHEMA = new ErlAtom("Schema");
    private static readonly ErlAtom TYPE   = new ErlAtom("Type");
    private static readonly ErlAtom ROWS   = new ErlAtom("Rows");

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

        //{Schema, ChangeType :: $d | $w, [{schema, Flds...}]}
        if (erlMsg.Msg==null ||
           !erlMsg.Msg.Match(SUBSCRIPTION_MSG_PATTERN, binding))
          return;

        var schemaName = ((ErlAtom)binding[SCHEMA]).Value;
        var op         = ((ErlByte)binding[TYPE]).Value;
        var rows       = ((ErlList)binding[ROWS]).Value;

        var schema = map.GetCRUDSchemaForName(schemaName);

        foreach(var rowTuple in rows.Cast<ErlTuple>())
        {
          var row  = map.ErlTupleToRow(schemaName, rowTuple, schema);
          var ctp  = op == 'd' ? RowChangeType.Delete : RowChangeType.Upsert;
          var data = new RowChange(ctp, row, null);
          this.Mailbox.Deliver(this, data);
        }
      }
      catch(Exception err)
      {
        App.Log.Write(
          new Log.Message
          {
            Type = Log.MessageType.Error,
            Topic = CoreConsts.ERLANG_TOPIC,
            From = "{0}.process()".Args(GetType().Name),
            Text = err.ToMessageWithType(),
            Exception = err 
          }
        );
      }
    }
  }
}
