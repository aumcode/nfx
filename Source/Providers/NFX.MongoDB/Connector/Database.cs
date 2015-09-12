using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Serialization.BSON;

namespace NFX.DataAccess.MongoDB.Connector
{
  /// <summary>
  /// Represents an instance of MongoDB database
  /// </summary>
  public sealed class Database : DisposableObject, INamed
  {
    #region .ctor
      internal Database(ServerNode server, string name)
      {
        m_Server = server;
        m_Name = name;

        //re-compute this in .ctor not to do this on every send
        m_CMD_NameCStringBuffer = BinUtils.WriteCStringToBuffer(m_Name+"."+Protocol.COMMAND_COLLECTION);
      }

      protected override void Destructor()
      {
        m_Server.m_Databases.Unregister(this);
        base.Destructor();
      }
    #endregion

    #region Fields
      private ServerNode m_Server;
      private string m_Name;
      internal readonly byte[] m_CMD_NameCStringBuffer;
      internal Registry<Collection> m_Collections = new Registry<Collection>(true);
    #endregion


    #region Properties
        public ServerNode Server { get{ return m_Server;} }
        public string     Name   { get{ return m_Name;} }

        /// <summary>
        /// Returns mounted collections
        /// </summary>
        public IRegistry<Collection> Collections { get {return m_Collections;} }

        /// <summary>
        /// Returns an existing collection or creates a new one
        /// </summary>
        public Collection this[string name]
        {
          get
          {
              if (name.IsNullOrWhiteSpace())
                throw new MongoDBConnectorException(StringConsts.ARGUMENT_ERROR+"Database[name==null|empty]");
             
              return m_Collections.GetOrRegister(name, (n) => new Collection(this, n), name);
          }
        }

        /// <summary>
        /// Generates request ID unique per server node
        /// </summary>
        internal int NextRequestID
        {
          get { return Server.NextRequestID; } //overflows are ok
        }

    #endregion

    #region Public
        /// <summary>
        /// Executes a NOP command that round-trips from the server
        /// </summary>
        public void Ping()
        {
          EnsureObjectNotDisposed();

          var connection = Server.AcquireConnection();
          try
          {
            var requestID = NextRequestID;
            connection.Ping(requestID, this);
          }
          finally
          {
            connection.Release();
          }
        }

        /// <summary>
        /// Returns the names of collections in this database
        /// </summary>
        public string[] GetCollectionNames()
        {
          EnsureObjectNotDisposed();

          var connection = Server.AcquireConnection();
          try
          {
            var requestID = NextRequestID;
            return connection.GetCollectionNames(requestID, this);
          }
          finally
          {
            connection.Release();
          }
        }

        /// <summary>
        /// Runs database-level command. Does not perform any error checks beyond network traffic req/resp passing
        /// </summary>
        public BSONDocument RunCommand(BSONDocument command)
        {
          EnsureObjectNotDisposed();

          if (command==null)
            throw new MongoDBConnectorException(StringConsts.ARGUMENT_ERROR+"RunCommand(command==null)");

          var connection = Server.AcquireConnection();
          try
          {
            var requestID = NextRequestID;
            return connection.RunCommand(requestID, this, command);
          }
          finally
          {
            connection.Release();
          }
        }
    #endregion
  }
}
