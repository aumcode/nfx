using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using NFX.Serialization.BSON;

namespace NFX.DataAccess.MongoDB.Connector
{
//see http://docs.mongodb.org/meta-driver/latest/legacy/mongodb-driver-requirements/
//see http://docs.mongodb.org/meta-driver/latest/


  /// <summary>
  /// Represents MongoDB collections which allows to execute queries and perform CRUD operations
  /// </summary>
  public sealed class Collection : DisposableObject, INamed
  {
      #region .ctor
        internal Collection(Database database, string name)
        {
          m_Database = database;
          m_Name = name;
          m_FullName = m_Database.Name + '.' + this.m_Name;

          //re-compute this in .ctor not to do this on every send
          m_FullNameCStringBuffer = BinUtils.WriteCStringToBuffer(m_FullName);
        }

        protected override void Destructor()
        {
          if (!m_Database.DisposeStarted)
            m_Database.m_Collections.Unregister(this);

          base.Destructor();
        }
      #endregion

      #region Fields
        private Database m_Database;
        private string m_Name;
        private string m_FullName;
        internal readonly byte[] m_FullNameCStringBuffer;
      #endregion

      #region Props
        public ServerNode  Server   { get{ return m_Database.Server;} }
        public Database    Database { get{ return m_Database;} }
        public string      Name     { get{ return m_Name;} }

        /// <summary>
        /// Returns full name of this collection: database.collection
        /// </summary>
        public string FullName { get{ return m_FullName;} }
      #endregion

      #region Public
        /// <summary>
        /// Finds a document that satisfied query or null
        /// </summary>
        public BSONDocument FindOne(Query query, BSONDocument selector = null)
        {
          EnsureObjectNotDisposed();

          if (query==null)
           throw new MongoDBConnectorException(StringConsts.ARGUMENT_ERROR+"Collection.FindOne(query==null)");

          var connection = Server.AcquireConnection();
          try
          {
            var reqId = Database.NextRequestID;
            return connection.FindOne(reqId, this, query, selector);
          }
          finally
          {
            connection.Release();
          }
        }

        /// <summary>
        /// Finds all documents that match the supplied query, optionally skipping some.
        /// Fetches and returns all documents as a list. Be careful not to fetch too many
        /// </summary>
        /// <param name="query">Query to match against</param>
        /// <param name="skipCount">How many documents to skip at the beginning</param>
        /// <param name="cursorFetchLimit">Impose a limit on total number of fetched docs</param>
        /// <param name="fetchBy">Size of fetch block</param>
        /// <param name="selector">Optional field mapping document like: {"field_name": 1}</param>
        /// <returns>A list of documents (may be empty)</returns>
        public List<BSONDocument> FindAndFetchAll(Query query, int skipCount = 0, int cursorFetchLimit = 0, int fetchBy = 0, BSONDocument selector = null)
        {
          var result = new List<BSONDocument>();

          using(var cursor = this.Find(query, skipCount, fetchBy, selector))
            foreach(var doc in cursor)
             if (cursorFetchLimit<=0 || result.Count<cursorFetchLimit)
              result.Add(doc);
             else
              break;

          return result;
        }

        /// <summary>
        /// Finds all documents that match the supplied query, optionally skipping some.
        /// Fetches only fetchBy at once, then lazily fetches via cursor
        /// </summary>
        /// <param name="query">Query to match against</param>
        /// <param name="skipCount">How many document sto skip at the beginning</param>
        /// <param name="fetchBy">The size of fetch block</param>
        /// <param name="selector">Optional field mapping document like: {"field_name": 1}</param>
        /// <returns>An iterable cursor</returns>
        public Cursor Find(Query query, int skipCount=0, int fetchBy=0, BSONDocument selector = null)
        {
          EnsureObjectNotDisposed();

          if (query==null)
           throw new MongoDBConnectorException(StringConsts.ARGUMENT_ERROR+"Collection.Find(query==null)");

          var connection = Server.AcquireConnection();
          try
          {
            var reqId = Database.NextRequestID;
            return connection.Find(reqId, this, query, selector, skipCount, fetchBy);
          }
          finally
          {
            connection.Release();
          }
        }


        /// <summary>
        /// Inserts documents on the server. Inspect CRUDResult for write errors and docs affected/matched
        /// </summary>
        public CRUDResult Insert(params BSONDocument[] documents)
        {
          EnsureObjectNotDisposed();

          if (documents==null || documents.Length<1)
           throw new MongoDBConnectorException(StringConsts.ARGUMENT_ERROR+"Collection.Insert(documents==null|empty)");

          var connection = Server.AcquireConnection();
          try
          {
            var reqId = Database.NextRequestID;
            return connection.Insert(reqId, this, documents);
          }
          finally
          {
            connection.Release();
          }
        }

        /// <summary>
        /// Updates documents on the server. Inspect CRUDResult for write errors and docs affected/matched
        /// </summary>
        public CRUDResult Update(params UpdateEntry[] updates)
        {
          EnsureObjectNotDisposed();

          if (updates==null || updates.Length<1)
           throw new MongoDBConnectorException(StringConsts.ARGUMENT_ERROR+"Collection.Update(updates==null|empty)");
          
          var connection = Server.AcquireConnection();
          try
          {
            var reqId = Database.NextRequestID;
            return connection.Update(reqId, this, updates);
          }
          finally
          {
            connection.Release();
          }
        }

        /// <summary>
        /// Updates or inserts 1 document on the server. Inspect CRUDResult for write errors and docs affected/matched
        /// </summary>
        public CRUDResult Save(BSONDocument document)
        {
          EnsureObjectNotDisposed();

          if (document==null)
            throw new MongoDBConnectorException(StringConsts.ARGUMENT_ERROR+"Collection.Save(document==null)");

          var _id = document[Protocol._ID];
          if (_id==null || _id is BSONNullElement)
            throw new MongoDBConnectorException(StringConsts.ARGUMENT_ERROR+"Collection.Save(document._id absent|null)");

          var connection = Server.AcquireConnection();
          try
          {
            var reqId = Database.NextRequestID;
            var updates = new UpdateEntry[]{ new UpdateEntry(
                   new Query("{'"+Protocol._ID+"': '$$ID'}", true, new TemplateArg("ID", _id.ElementType, _id.ObjectValue)),
                   document,
                   multi: false,
                   upsert: true
            ) };
            
            return connection.Update(reqId, this, updates);
          }
          finally
          {
            connection.Release();
          }
        }

        /// <summary>
        /// Deletes 1 document from the server. Inspect CRUDResult for write errors and docs affected/matched
        /// </summary>
        public CRUDResult DeleteOne(Query query)
        {
          if (query==null)
            throw new MongoDBConnectorException(StringConsts.ARGUMENT_ERROR+"Collection.DeleteOne(query==null)");

          return this.Delete(new DeleteEntry(query, DeleteLimit.OnlyFirstMatch));
        }

        /// <summary>
        /// Deletes documents from the server. Inspect CRUDResult for write errors and docs affected/matched
        /// </summary>
        public CRUDResult Delete(params DeleteEntry[] deletes)
        {
          EnsureObjectNotDisposed();

          if (deletes==null || deletes.Length<1)
           throw new MongoDBConnectorException(StringConsts.ARGUMENT_ERROR+"Collection.Delete(deletes==null|empty)");
          
          var connection = Server.AcquireConnection();
          try
          {
            var reqId = Database.NextRequestID;
            return connection.Delete(reqId, this, deletes);
          }
          finally
          {
            connection.Release();
          }
        }

        /// <summary>
        /// Drops the collection from the server with all of its data and disposes the Collection object
        /// </summary>
        public void Drop()
        {
          EnsureObjectNotDisposed();

          var connection = Server.AcquireConnection();
          try
          {
            var reqId = Database.NextRequestID;
            connection.Drop(reqId, this);
            this.Dispose();
          }
          finally
          {
            connection.Release();
          }
        }


        /// <summary>
        /// Performs server-side count over cursor
        /// </summary>
        /// <param name="query">Optional. A query that selects which documents to count in a collection</param>
        /// <param name="limit">Optional. The maximum number of matching documents to return</param>
        /// <param name="skip">Optional. The number of matching documents to skip before returning results</param>
        /// <param name="hint">Optional. The index to use. Specify either the index name as a string or the index specification document.</param>
        /// <returns>Count </returns>
        public long Count(Query query = null, int limit = -1, int skip = -1, object hint = null)
        { // See http://docs.mongodb.org/v3.0/reference/command/count/

          EnsureObjectNotDisposed();

          if (hint!=null && (!(hint is string)) && (!(hint is BSONDocument)))
            throw new MongoDBConnectorException(StringConsts.ARGUMENT_ERROR+"Collection.Count(hint must be string | BSONDocument)"); 

          var connection = Server.AcquireConnection();
          try
          {
            var reqId = Database.NextRequestID;
            return connection.Count(reqId, this, query, limit, skip, hint);
          }
          finally
          {
            connection.Release();
          }
        }


      #endregion

      
  }

}
