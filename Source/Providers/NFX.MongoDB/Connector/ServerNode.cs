using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NFX.Glue;
using NFX.Environment;
using NFX.ApplicationModel;


namespace NFX.DataAccess.MongoDB.Connector
{
  /// <summary>
  /// Manages connections to the same server
  /// </summary>
  public sealed class ServerNode : ApplicationComponent, INamed
  {
      #region CONSTS

        public const string CONFIG_SERVER_SECTION = "server";

        public const int DEFAULT_RCV_TIMEOUT = 10000;
        public const int DEFAULT_SND_TIMEOUT = 10000;

        public const int DEFAULT_RCV_BUFFER_SIZE    = 128 * 1024;
        public const int DEFAULT_SND_BUFFER_SIZE    = 128 * 1024;

        public const int MIN_IDLE_TIMEOUT_SEC  = 5;
        public const int DEFAULT_IDLE_TIMEOUT_SEC  = 2 * 60;

        public const int MAX_EXISTING_ACQUISITION_TIMEOUT_MS_MIN = 1000;
        public const int MAX_EXISTING_ACQUISITION_TIMEOUT_MS_MAX = 90 * 1000;
        public const int MAX_EXISTING_ACQUISITION_TIMEOUT_MS_DEFAULT = 15 * 1000;
      #endregion
      
      #region .ctor
        internal ServerNode(MongoClient client, Node node) : base(client)
        {
          m_Client = client;
          m_Node = node;

          var cfg = client.ConfigRoot//1. Try to find the SERVER section with name equals this node
                          .Children
                          .FirstOrDefault(c => c.IsSameName(CONFIG_SERVER_SECTION) && c.IsSameNameAttr(node.ConnectString));
          if (cfg==null)
           cfg = client.ConfigRoot //2. If not found, try to find SERVER section without name attr
                       .Children
                       .FirstOrDefault(c => c.IsSameName(CONFIG_SERVER_SECTION) && !c.AttrByName(Configuration.CONFIG_NAME_ATTR).Exists);

          if (cfg!=null)
            ConfigAttribute.Apply(this, client.ConfigRoot);
        }

        protected override void Destructor()
        {
          if (!m_Client.DisposeStarted)
            m_Client.m_Servers.Unregister( this );
         
          killCursors(true);

          CloseAllConnections(true);//must be the last thing
          base.Destructor();
        }
      #endregion
      
      #region Fields
        private int m_ID_SEED;
        private MongoClient m_Client;
        private Node m_Node;
        private object m_NewConnectionSync = new object();
        private object m_ListSync = new object();
        private volatile List<Connection> m_List = new List<Connection>();
      
        internal Registry<Database> m_Databases = new Registry<Database>(true);

        private WriteConcern m_WriteConcern;

        private List<Cursor> m_Cursors = new List<Cursor>(256);


        private int m_MaxConnections;
        private int m_MaxExistingAcquisitionTimeoutMs = MAX_EXISTING_ACQUISITION_TIMEOUT_MS_DEFAULT;
        //Timeouts and socket buffers setup
        private int m_IdleConnectionTimeoutSec = DEFAULT_IDLE_TIMEOUT_SEC;
            
        private int m_SocketReceiveBufferSize = DEFAULT_RCV_BUFFER_SIZE;
        private int m_SocketSendBufferSize    = DEFAULT_SND_BUFFER_SIZE;

        private int m_SocketReceiveTimeout = DEFAULT_RCV_TIMEOUT;
        private int m_SocketSendTimeout = DEFAULT_SND_TIMEOUT;
      #endregion


      #region Properties


        /// <summary>
        /// References client that this node is under
        /// </summary>
        public MongoClient Client{ get{ return m_Client;} }

        public Node Node { get{ return m_Node;} }
        
        public string Name { get{ return m_Node.ConnectString;} }


        /// <summary>
        /// Generates request ID unique per server node
        /// </summary>
        internal int NextRequestID
        {
          get { return System.Threading.Interlocked.Increment(ref m_ID_SEED); } //overflows are ok
        }

        /// <summary>
        /// Returns mounted databases
        /// </summary>
        public IRegistry<Database> Databases { get {return m_Databases;} }

        [Config]
        public WriteConcern WriteConcern
        {
            get { return m_WriteConcern; }
            set { m_WriteConcern = value;}
        }

        /// <summary>
        /// When greater than zero, imposes a limit on the open connection count
        /// </summary>
        [Config]
        public int MaxConnections 
        {
            get { return m_MaxConnections; }
            set { m_MaxConnections = value <0 ? 0 : value; }
        }

        /// <summary>
        /// Imposes a timeout for system trying to get an existing connection instance per remote address.
        /// </summary>
        [Config]
        public int MaxExistingAcquisitionTimeoutMs
        {
            get { return m_MaxExistingAcquisitionTimeoutMs; }
            set
            { 
              m_MaxExistingAcquisitionTimeoutMs = 
                   value <MAX_EXISTING_ACQUISITION_TIMEOUT_MS_MIN ? MAX_EXISTING_ACQUISITION_TIMEOUT_MS_MIN :
                   value >MAX_EXISTING_ACQUISITION_TIMEOUT_MS_MAX ? MAX_EXISTING_ACQUISITION_TIMEOUT_MS_MAX :
                   value;
            }         
        }

        [Config]
        public int IdleConnectionTimeoutSec 
        {
            get { return m_IdleConnectionTimeoutSec; }
            set { m_IdleConnectionTimeoutSec = value <MIN_IDLE_TIMEOUT_SEC ? MIN_IDLE_TIMEOUT_SEC : value;}
        }

        [Config]
        public int SocketReceiveBufferSize 
        {
            get { return m_SocketReceiveBufferSize; }
            set { m_SocketReceiveBufferSize = value <=0 ? DEFAULT_RCV_BUFFER_SIZE : value;}
        }

        [Config]
        public int SocketSendBufferSize 
        {
            get { return m_SocketSendBufferSize; }
            set { m_SocketSendBufferSize = value <=0 ? DEFAULT_SND_BUFFER_SIZE : value;}
        }

        [Config]
        public int SocketReceiveTimeout 
        {
            get { return m_SocketReceiveTimeout; }
            set { m_SocketReceiveTimeout = value <=0 ? DEFAULT_RCV_TIMEOUT : value;}
        }

        [Config]
        public int SocketSendTimeout 
        {
            get { return m_SocketSendTimeout; }
            set { m_SocketSendTimeout = value <=0 ? DEFAULT_SND_TIMEOUT : value;}
        }

        /// <summary>
        /// Returns an existing database or creates a new one
        /// </summary>
        public Database this[string name]
        {
          get
          {
             EnsureObjectNotDisposed();

             if (name.IsNullOrWhiteSpace())
               throw new MongoDBConnectorException(StringConsts.ARGUMENT_ERROR+"ServerNode[name==null|empty]");
             
             return m_Databases.GetOrRegister(name, (n) => new Database(this, n), name);
          }
        }

      #endregion

      #region Public

        /// <summary>
        /// Closes all connections. Waits untill all closed if wait==true, otherwise tries to close what it can
        /// </summary>
        public void CloseAllConnections(bool wait)
        {
          bool allClosed = true;
          do
          {
            allClosed = true;
            var lst = m_List;
             
            foreach(var cnn in lst)
              if (cnn.TryAcquire())
                cnn.Dispose();
              else
                allClosed = false;  

          } while(wait && !allClosed);
        }

      #endregion

      #region Protected/Int


        internal Connection AcquireConnection()
        {
          return acquireConnection(true);
        }
              private Connection acquireConnection(bool checkDisposed)
              {
                 if (checkDisposed) EnsureObjectNotDisposed();

                 var result = tryAcquireExistingConnection();
                 if (result!=null) return result;

                 //open new connection
                 lock(m_NewConnectionSync)
                 {
                   result = tryAcquireExistingConnection();
                   if (result!=null) return result;

                   result = openNewConnection();
                   return result;
                 }
              }


            private bool m_Managing = false;
        /// <summary>
        /// Periodically invoked by the client to do management work, like close expired connections
        /// </summary>
        internal void ManagerVisit()
        {
          if (m_Managing || DisposeStarted) return;
          try
          {
            m_Managing = true;
            closeInactiveConnections();
            killCursors(false);
          }
          finally
          {
            m_Managing = false;
          }
        }


        internal void RegisterCursor(Cursor cursor)
        {
          if (DisposeStarted) return;

          lock(m_Cursors)
           m_Cursors.Add(cursor);
        }

      #endregion

      #region .pvt
        private Connection openNewConnection()
        {
           var result = new Connection( this );
           lock(m_ListSync)
           {
             var lst = new List<Connection>(m_List);
             lst.Add(result);
             m_List = lst;//atomic
           }
           return result;
        }


        internal void closeExistingConnection(Connection cnn)
        {
          if (!this.DisposeStarted)
            lock(m_ListSync)
            {
              var lst = new List<Connection>(m_List);
              lst.Remove(cnn);
              m_List = lst;//atomic
            }
        }

        private Connection tryAcquireExistingConnection()
        {
          int GRANULARITY_MS = 5 + ((System.Threading.Thread.CurrentThread.GetHashCode() & CoreConsts.ABS_HASH_MASK) % 15);
          
          var wasActive = App.Active;//remember whether app was active during start
          var elapsed = 0;
          while ((App.Active | !wasActive) && !DisposeStarted)
          {
            var lst = m_List;//atomic
            var got = lst.FirstOrDefault( c => c.TryAcquire() );
            if (got!=null) return got;

            if (m_MaxConnections<=0) return null;//could not acquire, lets allocate
            if (lst.Count<m_MaxConnections) return null;//limit has not been reached

            System.Threading.Thread.Sleep(GRANULARITY_MS);
            elapsed+=GRANULARITY_MS;

            if (elapsed>m_MaxExistingAcquisitionTimeoutMs)
              throw new MongoDBConnectorException(
                                            StringConsts.CONNECTION_EXISTING_ACQUISITION_ERROR
                                            .Args(this.Node, m_MaxConnections, m_MaxExistingAcquisitionTimeoutMs)
                                            );
          }

          return null; //the new connection may need to opened EVEN when the dispose is in progress so cursors may be closed etc.
        }

        private void closeInactiveConnections()
        {
          var now = App.TimeSource.UTCNow;
          var lst = m_List;
          for(var i=0; i<lst.Count; i++)
          {
             var cnn = lst[i];
             if (cnn.TryAcquire())
              try
              {
                if (cnn.m_ExpirationStartUTC.HasValue)
                {
                  if ((now - cnn.m_ExpirationStartUTC.Value).TotalSeconds > m_IdleConnectionTimeoutSec)
                  {
                   cnn.Dispose();
                   continue;
                  }
                }
                else cnn.m_ExpirationStartUTC = now;
              }
              finally
              {
                cnn.Release(true);//not to reset use date
              }

          }
        }

        private void killCursors(bool killAll)
        {
          const int BATCH = 256;
          var toKill = new List<Cursor>();
          
          do
          {
              lock(m_Cursors)
              {
                for(var i=m_Cursors.Count-1; i>=0 && m_Cursors.Count>0 && toKill.Count<=BATCH; i--)
                {
                  var cursor = m_Cursors[i];
                  if (killAll || cursor.Disposed)
                  {
                    toKill.Add(cursor);
                    m_Cursors.RemoveAt(i);
                  }
                }
              }

              if (toKill.Count>0)
              {
                var connection = acquireConnection(false);
                try
                {
                  var reqId = NextRequestID;
                  connection.KillCursor(reqId, toKill.ToArray());
                }
                finally
                {
                  connection.Release();
                }

                toKill.Clear();
              }
              else break;
              
          }
          while(killAll);
        }

      #endregion

  }

}
