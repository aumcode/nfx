using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Serialization.BSON;

namespace NFX.DataAccess.MongoDB.Connector
{
  /// <summary>
  /// Represents a UNIDIRECTIONAL SINGLE-pass (no buffering) cursor returned by the Find(query) command.
  /// The cursor needs to be closed by calling Dispose() if the eof has not been reached OR
  /// it will auto-close on EOF. The cursor may be enumerated only once. It is NOT thread-safe
  /// </summary>
  public sealed class Cursor : DisposableObject, IEnumerable<BSONDocument>, IEnumerator<BSONDocument>
  {
    public const int DEFAULT_FETCH_BY = 32; 

    internal Cursor(Int64 id, Collection collection, Query query, BSONDocument selector, BSONDocument[] initialData)
    {
      m_ID = id;
      m_Collection = collection;
      m_Query = query;
      m_Selector = selector;

      m_Buffered = initialData;
      m_Index = -1;
      m_EOF = initialData==null || initialData.Length==0;
      
      //register cursor so it can get auto-closed on dispose
      if (!m_EOF)
        collection.Server.RegisterCursor(this);
    }

    protected override void Destructor()
    {
      //the Server perriodically auto-closes EOF/Disposed cursors in batches
    }

    private Int64 m_ID;
    private Collection m_Collection;
    private Query m_Query;
    private BSONDocument m_Selector;

    private int m_Index;
    private BSONDocument[] m_Buffered;
    private bool m_EOF;
    private BSONDocument m_Current;

    private int m_FetchBy = DEFAULT_FETCH_BY;


    /// <summary>
    /// Server-supplied cursor ID
    /// </summary>
    public Int64 ID { get{ return m_ID;}}
    
    /// <summary>
    /// Collection that cursor is open against
    /// </summary>
    public Collection Collection{ get{ return m_Collection;} }

    /// <summary>
    /// Query that was sent to the server and resulted in this cursor
    /// </summary>
    public Query Query{ get{ return m_Query;} }

    /// <summary>
    /// Optional selector that was issued with Query or NULL
    /// </summary>
    public BSONDocument Selector{ get{ return m_Selector;} }

    /// <summary>
    /// True if EOF has been reached
    /// </summary>
    public bool EOF{ get{return m_EOF;} }


    /// <summary>
    /// Gets/sets the size of GET_MORE fetch
    /// </summary>
    public int FetchBy
    {
      get{ return m_FetchBy;}
      set{ m_FetchBy = value<=0?DEFAULT_FETCH_BY : value;}
    }



    public IEnumerator<BSONDocument> GetEnumerator(){ return this; }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return this; }

                #region IEnumerator<BSONDocument>
                  public BSONDocument Current
                  {
                    get { return m_Current; }
                  }

                  object System.Collections.IEnumerator.Current
                  {
                    get { return this.Current; }
                  }

                  public bool MoveNext()
                  {
                    if (EOF) return false;
                    m_Index++;
                    if (m_Index==m_Buffered.Length)
                    {
                       fetchNextChunk();
                       if (EOF)
                         return false;

                       m_Index=0;
                    }
                    m_Current = m_Buffered[m_Index];
                    return true;
                  }

                  public void Reset()
                  {
                    throw new MongoDBConnectorException(StringConsts.CURSOR_ENUM_ALREADY_STARTED_ERROR);
                  }
                #endregion
     
     #region .pvt
      
      private void fetchNextChunk()
      {
          EnsureObjectNotDisposed();
          var connection = m_Collection.Server.AcquireConnection();
          try
          {
            var reqId = m_Collection.Database.NextRequestID;
            m_Buffered = connection.GetMore(reqId, this);
            m_EOF = m_Buffered==null;
          }
          finally
          {
            connection.Release();
          }
      }

     #endregion


  }
}
