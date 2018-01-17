using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX.Log
{
  /// <summary>
  /// Batches multiple log items into a smaller (usually single) number of log messages.
  /// The primary purpose is to preclude log flooding with repetitive error messages.
  /// Items represent log context, such as Exceptions etc.
  /// This class is not thread safe, however item addition is thread safe which makes it very convenient for use in
  /// places that use Parallel loops and the like.
  /// </summary>
  public class LogBatcher<TItem> : DisposableObject
  {
    #region Inner

      /// <summary>
      /// Provides data about the batched items, e.g. error count and a list of the first X errored Ids...
      /// Items are batched by their CLR Type
      /// </summary>
      public class ItemBatch
      {
        public ItemBatch(TItem firstItem)
        {
          First = firstItem;
          Count = 1;
        }

        public readonly TItem First;

        public Type Type { get { return First.GetType(); } }
        public int Count { get; protected set; }

        /// <summary>
        /// Override to add item-specific details, such as ids of error-causing entities.
        /// This method is called under lock and has to be efficient and not create much data, for example:
        /// only add the first 20 ids to the batch and discard the rest etc.
        /// </summary>
        protected internal virtual void Add(TItem item)
        {
          Count++;
        }

        public override string ToString()
        {
          return "{0}[{1}] First: {2}".Args(Type.Name, Count, First);
        }
      }

    #endregion

    protected LogBatcher(ILog log = null)
    {
      Log = log;
    }

    protected sealed override void Destructor()
    {
      var messages = MakeMessages();
      var log = Log ?? App.Log;

      foreach(var msg in messages) log.Write(msg);

      base.Destructor();
    }

    protected readonly Dictionary<Type, ItemBatch> m_Data = new Dictionary<Type, ItemBatch>();

    /// <summary>A log that messages will be written into when this instance is Disposed</summary>
    public readonly ILog Log;

    /// <summary>Default log message channel</summary>
    public string Channel { get; set; }

    /// <summary>Default log message type</summary>
    public MessageType Type { get; set; }

    /// <summary>Default log message topic</summary>
    public string Topic { get; set; }

    /// <summary>Default log message from</summary>
    public string From { get; set; }


    public bool HasItems { get { return m_Data.Any(); } }


    /// <summary>
    /// Add item to the logger batch
    /// </summary>
    public void Add(TItem item)
    {
      if (item==null) return;

      var t = item.GetType();

      lock (m_Data)
      {
        ItemBatch batch;
        if (!m_Data.TryGetValue(t, out batch))
        {
          batch = MakeBatch(item);
          m_Data.Add(t, batch);
          return;
        }

        batch.Add(item);
      }
    }

    /// <summary>
    /// Override to make a custom batch type
    /// </summary>
    protected virtual ItemBatch MakeBatch(TItem item)
    {
      return new ItemBatch(item);
    }

    /// <summary>
    /// Override to turn accumulated batch state into a series of log messages
    /// </summary>
    protected virtual IEnumerable<Message> MakeMessages()
    {
      var txt = new StringBuilder();

      txt.AppendLine("Batched {0} types:".Args(m_Data.Count));

      foreach(var kvp in m_Data)
      {
        txt.AppendLine("  ");
        txt.AppendLine(kvp.Value.ToString());
      }

      var msg = new Message
      {
        Type = this.Type,
        Channel = this.Channel,
        Topic = this.Topic,
        From = this.From,
        Text = txt.ToString()
      };

      yield return msg;
    }
  }

  /// <summary>
  /// Batches error logging
  /// </summary>
  public class ErrorLogBatcher : LogBatcher<Exception>
  {
    public ErrorLogBatcher(ILog log = null) : base(log)
    {
      Type = MessageType.Error;
    }

    protected override IEnumerable<Message> MakeMessages()
    {
      Guid? firstGuid = null;
      foreach(var kvp in m_Data)
      {
        var batch = kvp.Value;
        var msg = new Message
        {
          Type = this.Type,
          Channel = this.Channel,
          Topic = this.Topic,
          From = this.From,
          Text = batch.ToString(),
          Exception = batch.First
        };

        if (firstGuid.HasValue)
         msg.RelatedTo = firstGuid.Value;
        else
         firstGuid = msg.Guid;

        yield return msg;
      }
    }

  }

}
