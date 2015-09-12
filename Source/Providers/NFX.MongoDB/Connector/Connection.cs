using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using NFX.Glue;
using NFX.IO;
using NFX.ApplicationModel;
using NFX.Serialization.BSON;

namespace NFX.DataAccess.MongoDB.Connector
{
  /// <summary>
  /// Represent a connection to MongoDB server. Normally developers should not work with this class directly
  /// as connections are managed by the Client. 
  /// This class is not thread safe and must be Acquired first before sending data
  /// </summary>
  public sealed class Connection : ApplicationComponent
  {
      #region CONSTS
        public const string DEFAULT_MONGO_BINDING = "mongo";
        public const string DEFAULT_LOCAL_MONGO_HOST = "localhost";
        public const int DEFAULT_MONGO_PORT = 27017;
      
        public static readonly Node DEFAUL_LOCAL_NODE = new Node("{0}://{1}:{2}".Args(DEFAULT_MONGO_BINDING, DEFAULT_LOCAL_MONGO_HOST, DEFAULT_MONGO_PORT));

        private const int FREE = 0;
        private const int ACQUIRED = 1;

        private const int DEFAULT_STREAM_SIZE = 32 * 1024;
      #endregion

      #region .ctor
        internal Connection(ServerNode server) : base(server)
        {
          m_Server = server;
          connectSocket();
        }

        protected override void Destructor()
        {
          disconnectSocket();
          base.Destructor();
          m_BufferStream = null;
          m_Server.closeExistingConnection(this);
        }
      #endregion

      #region Fields
        private ServerNode m_Server;
        internal DateTime? m_ExpirationStartUTC;
        private int m_Acquired = ACQUIRED;//when first created, it is already acquired

        private TcpClient m_TcpClient;
        private MemoryStream m_BufferStream = new MemoryStream(DEFAULT_STREAM_SIZE);
        private BufferSegmentReadingStream m_Received = new BufferSegmentReadingStream();
      #endregion

      #region Properties
        public bool IsAcquired { get{ return m_Acquired == ACQUIRED;} }
        public ServerNode Server { get { return m_Server;} }

        /// <summary>
        /// Returns when the connection started to expire for its inactivity or null
        /// </summary>
        public DateTime? ExpirationStartUTC{ get{ return m_ExpirationStartUTC;}}

      #endregion

      #region Internal
        /// <summary>
        /// Returns true when the instance was able to get acquired by the calling thread for the exclusive use
        /// </summary>
        internal bool TryAcquire()
        {
           return Interlocked.CompareExchange(ref m_Acquired, ACQUIRED, FREE) == FREE;
        }

        /// <summary>
        /// Releases the connection that has been acquired before
        /// </summary>
        internal void Release(bool callFromManager = false)
        {
          if (!callFromManager)
            m_ExpirationStartUTC = null;
          
          Interlocked.Exchange(ref m_Acquired, FREE);
        }

        internal BSONDocument FindOne(int requestID, Collection collection, Query query, BSONDocument selector)
        {
          EnsureObjectNotDisposed();

          m_BufferStream.Position = 0;
          var total = Protocol.Write_QUERY(m_BufferStream, 
                                           requestID, 
                                           collection.Database,
                                           collection, 
                                           Protocol.QueryFlags.None,
                                           0,
                                           -1,// If the number is negative, then the database will return that number and close the cursor. 
                                              // No futher results for that query can be fetched. If numberToReturn is 1 the server will treat it as -1 
                                              //(closing the cursor automatically).
                                           query, 
                                           selector);
          writeSocket(total);
       
          var got = readSocket();
          var reply = Protocol.Read_REPLY(got);
          Protocol.CheckReplyDataForErrors(reply);

          var result = reply.Documents!=null && reply.Documents.Length>0 ? reply.Documents[0] : null;

          return result;
        }

        internal Cursor Find(int requestID, Collection collection, Query query, BSONDocument selector, int skipCount, int fetchBy)
        {
          EnsureObjectNotDisposed();

          if (fetchBy<=0) fetchBy = Cursor.DEFAULT_FETCH_BY;

          m_BufferStream.Position = 0;
          var total = Protocol.Write_QUERY(m_BufferStream, 
                                           requestID, 
                                           collection.Database,
                                           collection, 
                                           Protocol.QueryFlags.None,
                                           skipCount,
                                           fetchBy,
                                           query, 
                                           selector);
          writeSocket(total);
       
          var got = readSocket();
          var reply = Protocol.Read_REPLY(got);
          Protocol.CheckReplyDataForErrors(reply);

          var result = new Cursor(reply.CursorID, collection, query, selector, reply.Documents){ FetchBy = fetchBy };

          return result;
        }

        internal BSONDocument[] GetMore(int requestID, Cursor cursor)
        {
          EnsureObjectNotDisposed();

          m_BufferStream.Position = 0;
          var total = Protocol.Write_GET_MORE(m_BufferStream, 
                                           requestID, 
                                           cursor.Collection,
                                           cursor);
          writeSocket(total);
       
          var got = readSocket();
          var reply = Protocol.Read_REPLY(got);

          if (reply.Flags.HasFlag(Protocol.ResponseFlags.CursorNotFound)) return null;//EOF

          Protocol.CheckReplyDataForErrors(reply);

          return reply.Documents;
        }

        internal void KillCursor(int requestID, Cursor[] cursors)
        {
          EnsureObjectNotDisposed();

          m_BufferStream.Position = 0;
          var total = Protocol.Write_KILL_CURSORS(m_BufferStream, requestID, cursors);

          writeSocket(total);
       
          //KILL_CURSORS returns nothing!
          //////var got = readSocket();
          //////but what gets returned back? NOTHING!
        }


        internal CRUDResult Insert(int requestID, Collection collection, BSONDocument[] data)
        {
          EnsureObjectNotDisposed();

          m_BufferStream.Position = 0;
          var total = Protocol.Write_INSERT(m_BufferStream, requestID, collection, data);
          writeSocket(total);
          var got = readSocket();
          return Protocol.Read_CRUD_Response("Insert", got);
        }

        internal CRUDResult Update(int requestID, Collection collection, UpdateEntry[] updates)
        {
          EnsureObjectNotDisposed();

          m_BufferStream.Position = 0;
          var total = Protocol.Write_UPDATE(m_BufferStream, requestID, collection, updates);
          writeSocket(total);
          var got = readSocket();
          return Protocol.Read_CRUD_Response("Update", got);
        }

        internal CRUDResult Delete(int requestID, Collection collection, DeleteEntry[] deletes)
        {
          EnsureObjectNotDisposed();

          m_BufferStream.Position = 0;
          var total = Protocol.Write_DELETE(m_BufferStream, requestID, collection, deletes);
          writeSocket(total);
          var got = readSocket();
          return Protocol.Read_CRUD_Response("Delete", got);
        }

        internal void Drop(int requestID, Collection collection)
        {
          EnsureObjectNotDisposed();

          m_BufferStream.Position = 0;
          var total = Protocol.Write_DROP(m_BufferStream, requestID, collection);
          writeSocket(total);
          var got = readSocket();
          var reply = Protocol.Read_REPLY(got);
          Protocol.CheckReplyDataForErrors(reply);
        }

        internal void Ping(int requestID, Database db)
        {
          EnsureObjectNotDisposed();

          m_BufferStream.Position = 0;
          var total = Protocol.Write_PING(m_BufferStream, requestID, db);
          writeSocket(total);
          var got = readSocket();
          var reply = Protocol.Read_REPLY(got);
          Protocol.CheckReplyDataForErrors(reply);
          if (Protocol.IsOKReplyDoc(reply)) return;

          throw new MongoDBConnectorProtocolException(StringConsts.PROTO_PING_REPLY_ERROR); 
        }

        internal string[] GetCollectionNames(int requestID, Database db)
        {
          EnsureObjectNotDisposed();

          m_BufferStream.Position = 0;
          var total = Protocol.Write_LIST_COLLECTIONS(m_BufferStream, requestID, db);
          writeSocket(total);
          var got = readSocket();
          var reply = Protocol.Read_REPLY(got);
          Protocol.CheckReplyDataForErrors(reply);
          if (Protocol.IsOKReplyDoc(reply))
          {
            var cursor = reply.Documents[0]["cursor"] as BSONDocumentElement;
            if (cursor!=null && cursor.Value!=null)
            {
              var firstBatch = cursor.Value["firstBatch"] as BSONArrayElement;
              if (firstBatch!=null && firstBatch.Value!=null)
              {
                return firstBatch.Value.Where( e => e is BSONDocumentElement )
                                       .Cast<BSONDocumentElement>()
                                       .Where( de => de.Value!=null )
                                       .Select( de => de.Value["name"].AsString()).ToArray();
              }
            }
          }
          throw new MongoDBConnectorProtocolException(StringConsts.PROTO_LIST_COLLECTIONS_REPLY_ERROR); 
        }

        internal long Count(int requestID, Collection collection, Query query = null, int limit = -1, int skip = -1, object hint = null)
        {
          EnsureObjectNotDisposed();

          m_BufferStream.Position = 0;
          var total = Protocol.Write_COUNT(m_BufferStream, 
                                           requestID, 
                                           collection, 
                                           query,
                                           limit,
                                           skip,
                                           hint);
          writeSocket(total);
       
          var got = readSocket();
          var reply = Protocol.Read_REPLY(got);
          Protocol.CheckReplyDataForErrors(reply);

           if (Protocol.IsOKReplyDoc(reply))
          {
            return reply.Documents[0]["n"].AsLong();
          }
          throw new MongoDBConnectorProtocolException(StringConsts.PROTO_COUNT_REPLY_ERROR); 
        }

        internal BSONDocument RunCommand(int requestID, Database database, BSONDocument command)
        {
          EnsureObjectNotDisposed();

          m_BufferStream.Position = 0;
          var total = Protocol.Write_RUN_COMMAND(m_BufferStream, 
                                           requestID, 
                                           database, 
                                           command);
          writeSocket(total);
       
          var got = readSocket();
          var reply = Protocol.Read_REPLY(got);
          Protocol.CheckReplyDataForErrors(reply);

           if (Protocol.IsOKReplyDoc(reply))
          {
            return reply.Documents[0];
          }
          throw new MongoDBConnectorProtocolException(StringConsts.PROTO_RUN_COMMAND_REPLY_ERROR + command.ToString()); 
        }

      #endregion

      #region .pvt

        private void writeSocket(int total)
        {
          if (total>=Protocol.BSON_SIZE_LIMIT)
            throw new MongoDBConnectorProtocolException(StringConsts.PROTO_SOCKET_WRITE_EXCEED_LIMIT_ERROR.Args(total, Protocol.BSON_SIZE_LIMIT));

          ensureSocket();
          m_TcpClient.GetStream().Write(m_BufferStream.GetBuffer(), 0, total);
          //todo inc stats
        }

        private Stream readSocket()
        {
          var nets = m_TcpClient.GetStream();
          var total = BinUtils.ReadInt32(nets);
          
          if (total>=Protocol.BSON_SIZE_LIMIT)
            throw new MongoDBConnectorProtocolException(StringConsts.PROTO_SOCKET_READ_EXCEED_LIMIT_ERROR.Args(total, Protocol.BSON_SIZE_LIMIT));

          var leftToRead = total - sizeof(Int32);  //the total size includes the 4 bytes

          m_BufferStream.SetLength(total);
          var buffer = m_BufferStream.GetBuffer();
          BinUtils.WriteInt32(buffer, total, 0);
          socketRead(nets, buffer, sizeof(Int32), leftToRead);
          m_Received.BindBuffer(buffer, 0, total);
          return m_Received;
          //todo stats
        }


        private void socketRead(NetworkStream nets, byte[] buffer, int offset, int total)
        {
          int cnt = 0;
          while(cnt<total)
          {
            var got = nets.Read(buffer, offset + cnt, total - cnt);
            if (got<=0) throw new SocketException();
            cnt += got;
          }
        }

        private IPEndPoint getIPEndPoint()
        {
          var node = m_Server.Node;
          return (node.Host + ':' + node.Service).ToIPEndPoint(DEFAULT_MONGO_PORT);
        } 

        private void ensureSocket()
        {
          if (m_TcpClient!=null) return;
          connectSocket();
        }

        private void connectSocket()
        {
          disconnectSocket();
          var server = getIPEndPoint();
        
          m_TcpClient = new TcpClient();
        
          m_TcpClient.Connect( server );

          m_TcpClient.NoDelay = true;
          m_TcpClient.LingerState.Enabled = false;

          m_TcpClient.ReceiveBufferSize =  m_Server.SocketReceiveBufferSize;
          m_TcpClient.SendBufferSize    =  m_Server.SocketSendBufferSize; 
            
          m_TcpClient.ReceiveTimeout    =  m_Server.SocketReceiveTimeout;
          m_TcpClient.SendTimeout       =  m_Server.SocketSendTimeout;
        }

        private void disconnectSocket()
        {
          if (m_TcpClient==null) return;
          m_TcpClient.Close();
          m_TcpClient = null;
        }
      #endregion

  }

}
