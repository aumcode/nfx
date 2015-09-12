using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

using NFX.Serialization.BSON;

namespace NFX.DataAccess.MongoDB.Connector
{
  //see http://docs.mongodb.org/meta-driver/latest/legacy/mongodb-wire-protocol/
  //
  //Byte Ordering
  //Like BSON documents, all data in the MongoDB wire protocol is little-endian.
  //https://github.com/mongodb/mongo-csharp-driver/tree/373d7595aa17203ed955f7d76f5dcaf102eb67af/src/MongoDB.Driver.Core/Core/WireProtocol


 //Changed in version 2.6: A new protocol for write operations integrates write concerns with the write operations, eliminating the need for a separate getLastError.
 // Most write methods now return the status of the write operation, including error information. In previous versions, clients typically used the getLastError in 
 //combination with a write operation to verify that the write succeeded.
// http://docs.mongodb.org/manual/reference/command/getLastError/

//http://docs.mongodb.org/meta-driver/latest/legacy/mongodb-driver-requirements/
//http://docs.mongodb.org/meta-driver/latest/


//http://stackoverflow.com/questions/17385679/where-is-the-latest-mongodb-wire-protocol
//https://github.com/mongodb/mongo/blob/master/src/mongo/db/wire_version.h#L46

//See this: <--- latest Spec!
//https://github.com/mongodb/specifications/blob/master/source/server_write_commands.rst
// INSERT/UPDATE/DELETE are handled via 'database'.$cmd.OP_QUERY now

/*
 Craig Wilson:
 * OP_QUERY
 * [5:46:09 PM] Craig Wilson (MongoDB): nToReturn should be -1
 * yes, send to <database>.$cmd, and put the command into the query: {insert: <collection_name>, documents: [...] }
 * OP_REPLY will always come back from that
 */

  /// <summary>
  /// This class uses static methods on purpose to avoid allocations
  /// </summary>
  internal static class Protocol
  {
      private const string NOT_AVAIL = "<n/a>";
      
      public const string COMMAND_COLLECTION = "$cmd";

      public const string _ID = "_id";

      public const int BSON_SIZE_LIMIT = 16 * 1024 * 1024;

      public const int MAX_DOC_COUNT_LIMIT = BSON_SIZE_LIMIT / 5;//suppose that every doc is at least 5 bytes

      public const Int32 STD_HDR_LEN = 4 * 4;//4 fields 
      
      /* struct MsgHeader {
          int32   messageLength; // total message size, including this
          int32   requestID;     // identifier for this message             //This ID is unique per connection
          int32   responseTo;    // requestID from the original request
                                 //   (used in reponses from db)
          int32   opCode;        // request type - see table below
     }
       */

      public const Int32 OP_REPLY        = 1;//Reply to a client request. responseTo is set
      public const Int32 OP_MSG          = 1000;// generic msg command followed by a string
      public const Int32 OP_UPDATE       = 2001;// update document
      public const Int32 OP_INSERT       = 2002;// insert new document
      public const Int32 RESERVED        = 2003;// formerly used for OP_GET_BY_OID
      public const Int32 OP_QUERY        = 2004;// query a collection
      public const Int32 OP_GET_MORE     = 2005;// Get more data from a query. See Cursors
      public const Int32 OP_DELETE       = 2006;// Delete documents
      public const Int32 OP_KILL_CURSORS = 2007;// Tell database client is done with a cursor


      private static void writeStandardHeader(Stream stream, Int32 msgLen, Int32 reqID,  Int32 respTo, Int32 opCode)
      {
        BinUtils.WriteInt32(stream, msgLen);
        BinUtils.WriteInt32(stream, reqID);
        BinUtils.WriteInt32(stream, respTo);
        BinUtils.WriteInt32(stream, opCode);
      }

      //http://docs.mongodb.org/manual/reference/write-concern/
      private static BSONDocument getWriteConcern(Collection collection)
      {
        var concern = collection.Server.WriteConcern;

        if (concern<WriteConcern.Acknowledged) return null;//none
        
        var result = new BSONDocument();

        result.Set( new BSONInt32Element("w", concern >= WriteConcern.Acknowledged ? 1 : 0));

        if (concern == WriteConcern.Journaled)
         result.Set( new BSONBooleanElement("j", true));

        return result;
      }

           #region RESPONSES

                 #region Read_OP_REPLY

                        [Flags]
                        public enum ResponseFlags
                        {
                           None = 0,
                           /// <summary>
                           ///Set when getMore is called but the cursor id is not valid at the server. Returned with zero results. 
                           /// </summary>
                           CursorNotFound = 1,

                           /// <summary>
                           ///Set when query failed. Results consist of one document containing an “$err” field describing the failure.
                           /// </summary>
                           QueryFailure = 1 << 1,

                           /// <summary>
                           ///Drivers should ignore this. Only mongos will ever see this set, in which case, it needs to update config from the server. 
                           /// </summary>
                           ShardConfigStale = 1 << 2,

                           /// <summary>
                           /// Set when the server supports the AwaitData Query option. If it doesn’t, a client should sleep a little between getMore’s of a Tailable cursor. Mongod version 1.6 supports AwaitData and thus always sets AwaitCapable.
                           /// </summary>
                           AwaitCapable = 1 << 3
                        }

                        public struct ReplyData
                        {
                           public ReplyData(Int32 hdrReqID, Int32 hdrRespTo, ResponseFlags flags, Int64 cursorID, Int32 startingFrom, BSONDocument[] docs)
                           {
                              HDR_RequestID = hdrReqID;
                              HDR_ResponseTo = hdrRespTo;
                              Flags = flags;
                              CursorID = cursorID;
                              StartingFrom = startingFrom;
                              Documents = docs; 
                           }

                           public readonly Int32 HDR_RequestID;
                           public readonly Int32 HDR_ResponseTo;

                           public readonly ResponseFlags Flags; 
                           public readonly Int64 CursorID;
                           public readonly Int32 StartingFrom;
                           public readonly BSONDocument[] Documents;
                        }
                        
                        /*
                        struct {
                            MsgHeader header;         // standard message header
                            int32     responseFlags;  // bit vector - see details below
                            int64     cursorID;       // cursor id if client needs to do get more's
                            int32     startingFrom;   // where in the cursor this reply is starting
                            int32     numberReturned; // number of documents in the reply
                            document* documents;      // documents
                        }
                        */
                        /// <summary>
                        /// Reads OP_REPLY but does not interpret it
                        /// </summary>
                        public static ReplyData Read_REPLY(Stream stream)
                        {
                           stream.Position = sizeof(Int32);//skip Total size
                           var hdrReqID  = BinUtils.ReadInt32(stream);
                           var hdrRespTo = BinUtils.ReadInt32(stream);
                           var hdrOpCode = BinUtils.ReadInt32(stream);
                           if (hdrOpCode!=OP_REPLY)
                             throw new MongoDBConnectorProtocolException(StringConsts.PROTO_READ_OP_REPLY_ERROR+" not OP_REPLY"); 

                           var flags = (ResponseFlags)BinUtils.ReadInt32(stream);
                           var cursorID = BinUtils.ReadInt64(stream);
                           var startingFrom = BinUtils.ReadInt32(stream);
                           var numReturned = BinUtils.ReadInt32(stream);

                           if (numReturned>MAX_DOC_COUNT_LIMIT)
                             throw new MongoDBConnectorProtocolException(StringConsts.PROTO_REPLY_DOC_COUNT_EXCEED_LIMIT_ERROR.Args(numReturned, MAX_DOC_COUNT_LIMIT));

                           var docs = new BSONDocument[numReturned];

                           for(var i=0; i < numReturned; i++)
                             docs[i] = new BSONDocument(stream);
                             
                           return new ReplyData(hdrReqID, hdrRespTo, flags, cursorID, startingFrom, docs);
                        }

                        public static void CheckReplyDataForErrors(ReplyData reply)
                        {
                          if (reply.Flags.HasFlag(ResponseFlags.QueryFailure))
                          {
                           var err = (reply.Documents!=null && reply.Documents.Length>0) ? reply.Documents[0].ToString() : NOT_AVAIL;
                           throw new MongoDBConnectorServerException(StringConsts.SERVER_QUERY_FAILURE_ERROR + err);
                          }

                          if (reply.Flags.HasFlag(ResponseFlags.CursorNotFound))
                           throw new MongoDBConnectorServerException(StringConsts.SERVER_CURSOR_NOT_FOUND_ERROR);
                        }

                        public static bool IsOKReplyDoc(ReplyData reply)
                        {
                             return (reply.Documents!=null) && 
                                    (reply.Documents.Length>=0) &&
                                    (reply.Documents[0]["ok"].AsInt()==1);
                        }

                 #endregion

                 #region Read_CRUD_Response

                        //see:
                        //https://github.com/mongodb/specifications/blob/master/source/server_write_commands.rst#error-document
                        public static CRUDResult Read_CRUD_Response(string operation, Stream stream)
                        {
                          var reply = Read_REPLY(stream);
                          CheckReplyDataForErrors(reply);

                          if (reply.Documents==null || reply.Documents.Length==0)
                           throw new MongoDBConnectorServerException(StringConsts.PROTO_SERVER_REPLIED_WITH_NO_DOCUMENTS_ERROR);

                          var body = reply.Documents[0];

                          var ok = body["ok"];
                          if (ok==null)
                            throw new MongoDBConnectorProtocolException(StringConsts.PROTO_READ_CRUD_RESPONSE_ERROR+" missing 'ok' element");

                          if (ok.AsInt()==0)
                          {
                            var code = body["code"];
                            var errmsg = body["errmsg"];
                            
                            throw new MongoDBConnectorServerException(StringConsts.SERVER_OPERATION_RETURNED_ERROR
                                                                     .Args(operation,
                                                                           code!=null ? code.ObjectValue : NOT_AVAIL,
                                                                           errmsg!=null ? errmsg.ObjectValue : NOT_AVAIL
                                                                          )
                                                                     );
                          }

                          var _n = body["n"].AsInt();
                          var _nModified = body["nModified"].AsInt();

                          //write errors
                          CRUDWriteError[] _wErrors = null;
                          var we = body["writeErrors"] as BSONArrayElement;
                          if (we!=null)
                           _wErrors = we.Value.Cast<BSONDocumentElement>()
                                              .Select( ed => ed.Value)
                                              .Select( doc => 
                                                        {
                                                          var ei = doc["errInfo"];

                                                          return new CRUDWriteError(
                                                            doc["index"].AsInt(),
                                                            doc["code"].AsInt(),
                                                            doc["errmsg"].AsString(),
                                                            ei!=null ? ei.ObjectValue : null
                                                          );
                                                        }).ToArray();
                          
                          //upserted
                          CRUDUpsertInfo[] _upserted = null;
                          var ue = body["upserted"] as BSONArrayElement;
                          if (ue!=null)
                           _upserted = ue.Value.Cast<BSONDocumentElement>()
                                               .Select( ud => ud.Value)
                                               .Select( doc => 
                                                        {
                                                          var oi = doc[_ID];

                                                          return new CRUDUpsertInfo(
                                                            doc["index"].AsInt(),
                                                            oi!=null ? oi.ObjectValue : null
                                                          );
                                                        }).ToArray();

                          //Form result
                          return new CRUDResult(_n, _nModified, _wErrors, _upserted);
                        }

                 #endregion

           #endregion


           #region REQUESTS

                 #region INSERT

                        public static Int32 Write_INSERT(Stream stream,
                                                         Int32 requestID,
                                                         Collection collection,
                                                         BSONDocument[] data)
                        {
                         
                          var body = new BSONDocument();
                          body.Set( new BSONStringElement("insert", collection.Name) );

                          var writeConcern = getWriteConcern(collection);
                          if (writeConcern!=null)
                            body.Set( new BSONDocumentElement("writeConcern", writeConcern) );

                          var arr = data.Select( elm => new BSONDocumentElement(elm) ).ToArray();
                          body.Set( new BSONArrayElement("documents", arr) );

                          return Write_QUERY(stream, requestID, collection.Database, null, QueryFlags.None, 0, -1, body, null);
                        }

                 #endregion

                 #region UPDATE

                        public static Int32 Write_UPDATE(Stream stream,
                                                         Int32 requestID,
                                                         Collection collection,
                                                         UpdateEntry[] updates)
                        {
                          var body = new BSONDocument();
                          body.Set( new BSONStringElement("update", collection.Name) );

                          var writeConcern = getWriteConcern(collection);
                          if (writeConcern!=null)
                            body.Set( new BSONDocumentElement("writeConcern", writeConcern) );

                          var arr = updates.Select( one =>
                                                    {
                                                      var doc = new BSONDocument();
                                                      doc.Set( new BSONDocumentElement("q", one.Query) );
                                                      doc.Set( new BSONDocumentElement("u", one.Update) );

                                                      if (one.Multi)
                                                        doc.Set( new BSONBooleanElement("multi", true) );

                                                      if (one.Upsert)
                                                        doc.Set( new BSONBooleanElement("upsert", true) );

                                                      return new BSONDocumentElement(doc);
                                                    }
                          ).ToArray();

                          body.Set( new BSONArrayElement("updates", arr) );

                          return Write_QUERY(stream, requestID, collection.Database, null, QueryFlags.None, 0, -1, body, null);
                        }

                 #endregion

                 #region DELETE


                        public static Int32 Write_DELETE(Stream stream,
                                                         Int32 requestID,
                                                         Collection collection,
                                                         DeleteEntry[] deletes)
                        {
                          var body = new BSONDocument();
                          body.Set( new BSONStringElement("delete", collection.Name) );

                          var writeConcern = getWriteConcern(collection);
                          if (writeConcern!=null)
                            body.Set( new BSONDocumentElement("writeConcern", writeConcern) );

                          var arr = deletes.Select( one =>
                                                    {
                                                      var doc = new BSONDocument();
                                                      doc.Set( new BSONDocumentElement("q", one.Query) );
                                                      doc.Set( new BSONInt32Element("limit", one.Limit==DeleteLimit.None ? 0 : 1) );
                                                      return new BSONDocumentElement(doc);
                                                    }
                          ).ToArray();

                          body.Set( new BSONArrayElement("deletes", arr) );

                          return Write_QUERY(stream, requestID, collection.Database, null, QueryFlags.None, 0, -1, body, null);
                        }

                 #endregion

                 #region QUERY
                        /*
                           struct OP_QUERY {
                              MsgHeader header;                 // standard message header
                              int32     flags;                  // bit vector of query options.  See below for details.
                              cstring   fullCollectionName ;    // "dbname.collectionname"
                              int32     numberToSkip;           // number of documents to skip
                              int32     numberToReturn;         // number of documents to return
                                                                //  in the first OP_REPLY batch
                              document  query;                  // query object.  See below for details.
                            [ document  returnFieldsSelector; ] // Optional. Selector indicating the fields
                                                                //  to return.  See below for details.
                          }
                        }*/

                        [Flags]
                        public enum QueryFlags
                        {
                          None = 0,

                          TailableCursor = 1,//Tailable means cursor is not closed when the last data is retrieved. Rather, the cursor marks the final object’s position. 
                                             //You can resume using the cursor later, from where it was located, if more data were received. Like any “latent cursor”, 
                                             //the cursor may become invalid at some point (CursorNotFound) – for example if the final object it references were deleted.
                        
                          SlaveOk = 1 << 1, //	Allow query of replica slave. Normally these return an error except for namespace “local”.
                        
                          OplogReplay = 1 << 2,//	Internal replication use only - driver should not set
                        
                          NoCursorTimeout = 1 << 3,// The server normally times out idle cursors after an inactivity period (10 minutes) to prevent excess memory use. 
                                                   //Set this option to prevent that.
                          AwaitData = 1 << 4, // 	Use with TailableCursor. If we are at the end of the data, block for a while rather than returning no data.
                                             // After a timeout period, we do return as normal.
                          Exhaust  = 1 << 5, //	Stream the data down full blast in multiple “more” packages, on the assumption that the client will fully read all data 
                                             // queried. Faster when you are pulling a lot of data and know you want to pull it all down. Note: the client is not allowed 
                                             // to not read all the data unless it closes the connection.
                         	Partial = 1 << 6  //	Get partial results from a mongos if some shards are down (instead of throwing an error)
                        }

                        public static Int32 Write_QUERY(Stream stream,
                                            Int32 requestID,
                                            Database db, 
                                            Collection collection, //may be null for $CMD
                                            QueryFlags flags,
                                            Int32 numberToSkip,
                                            Int32 numberToReturn,
                                            BSONDocument query,
                                            BSONDocument selector//may be null
                                            )
                        {
                          stream.Position = STD_HDR_LEN;//skip the header

                          BinUtils.WriteInt32(stream, (Int32)flags);

                          //if collection==null then query the $CMD collection
                          var fullNameBuffer = collection!=null ? collection.m_FullNameCStringBuffer : db.m_CMD_NameCStringBuffer;
                          stream.Write(fullNameBuffer, 0, fullNameBuffer.Length);
                          

                          BinUtils.WriteInt32(stream, numberToSkip);
                          BinUtils.WriteInt32(stream, numberToReturn);

                          query.WriteAsBSON(stream);
                          
                          if (selector!=null)
                           selector.WriteAsBSON(stream);

                          var total = (Int32)stream.Position;
                          stream.Position = 0;
                          writeStandardHeader(stream, total, requestID, 0, OP_QUERY);
                          return total; 
                        }
                 #endregion


                 #region KILL_CURSORS
                        /*
                          struct {
                              MsgHeader header;            // standard message header
                              int32     ZERO;              // 0 - reserved for future use
                              int32     numberOfCursorIDs; // number of cursorIDs in message
                              int64*    cursorIDs;         // sequence of cursorIDs to close
                          }
                        }*/

                        public static Int32 Write_KILL_CURSORS(Stream stream,
                                            Int32 requestID,
                                            Int64[] cursorIDs
                                            )
                        {
                          stream.Position = STD_HDR_LEN;//skip the header

                          BinUtils.WriteInt32(stream, 0);//ZERO

                          BinUtils.WriteInt32(stream, cursorIDs.Length);

                          for(var i=0; i<cursorIDs.Length; i++)
                            BinUtils.WriteInt64(stream, cursorIDs[i]);

                          var total = (Int32)stream.Position;
                          stream.Position = 0;
                          writeStandardHeader(stream, total, requestID, 0, OP_KILL_CURSORS);
                          return total; 
                        }
                 #endregion


                 #region DROP


                        public static Int32 Write_DROP(Stream stream,
                                                         Int32 requestID,
                                                         Collection collection)
                        {
                          var body = new BSONDocument();
                          body.Set( new BSONStringElement("drop", collection.Name) );

                          return Write_QUERY(stream, requestID, collection.Database, null, QueryFlags.None, 0, -1, body, null);
                        }
                #endregion

                #region PING

                        public static Int32 Write_PING(Stream stream, Int32 requestID, Database db)
                        {
                          var body = new BSONDocument();
                          body.Set( new BSONInt32Element("ping", 1) );

                          return Write_QUERY(stream, requestID, db, null, QueryFlags.None, 0, -1, body, null);
                        }

                #endregion

                #region RUN_COMMAND

                        public static Int32 Write_RUN_COMMAND(Stream stream, Int32 requestID, Database db, BSONDocument command)
                        {
                          return Write_QUERY(stream, requestID, db, null, QueryFlags.None, 0, -1, command, null);
                        }

                #endregion

                #region LIST_COLLECTIONS

                        public static Int32 Write_LIST_COLLECTIONS(Stream stream, Int32 requestID, Database db)
                        {
                          var body = new BSONDocument();
                          body.Set( new BSONInt32Element("listCollections", 1) );

                          return Write_QUERY(stream, requestID, db, null, QueryFlags.None, 0, -1, body, null);
                        }

                #endregion

                #region COUNT

                        public static Int32 Write_COUNT(Stream stream, 
                                                                   Int32 requestID,
                                                                   Collection collection,
                                                                   Query query,
                                                                   Int32 limit,
                                                                   Int32 skip,
                                                                   object hint)
                        {
                          var body = new BSONDocument();
                          body.Set( new BSONStringElement("count", collection.Name) );

                          if (query!=null)  body.Set( new BSONDocumentElement("query", query) );
                          if (limit>0)      body.Set( new BSONInt32Element("limit", limit) );
                          if (skip>0)       body.Set( new BSONInt32Element("skip", limit) );

                          if (hint is string) body.Set( new BSONStringElement("hint", (string)hint) );
                          else
                          if (hint is BSONDocument) body.Set( new BSONDocumentElement("hint", (BSONDocument)hint) );

                          return Write_QUERY(stream, requestID, collection.Database, null, QueryFlags.None, 0, -1, body, null);
                        }

                #endregion

                #region KILL_CURSORS
                        /*
                        struct {
                            MsgHeader header;            // standard message header
                            int32     ZERO;              // 0 - reserved for future use
                            int32     numberOfCursorIDs; // number of cursorIDs in message
                            int64*    cursorIDs;         // sequence of cursorIDs to close
                        }*/
                        public static Int32 Write_KILL_CURSORS(Stream stream,
                                            Int32 requestID,
                                            Cursor[] cursors)
                        {
                          stream.Position = STD_HDR_LEN;//skip the header

                          BinUtils.WriteInt32(stream, 0);//ZERO

                          BinUtils.WriteInt32(stream, cursors.Length);
                          
                          for(var i=0; i<cursors.Length; i++)
                           BinUtils.WriteInt64(stream, cursors[i].ID);

                          var total = (Int32)stream.Position;
                          stream.Position = 0;
                          writeStandardHeader(stream, total, requestID, 0, OP_KILL_CURSORS);
                          return total; 
                        }

                #endregion


                #region GET_MORE
                        /*
                        struct {
                            MsgHeader header;             // standard message header
                            int32     ZERO;               // 0 - reserved for future use
                            cstring   fullCollectionName; // "dbname.collectionname"
                            int32     numberToReturn;     // number of documents to return
                            int64     cursorID;           // cursorID from the OP_REPLY
                        }

                        */

                        public static Int32 Write_GET_MORE(Stream stream,
                                            Int32 requestID,
                                            Collection collection,
                                            Cursor cursor)
                        {
                          stream.Position = STD_HDR_LEN;//skip the header

                          BinUtils.WriteInt32(stream, 0);//ZERO

                           //Collection name
                          var fullNameBuffer = collection.m_FullNameCStringBuffer;
                          stream.Write(fullNameBuffer, 0, fullNameBuffer.Length);

                          BinUtils.WriteInt32(stream, cursor.FetchBy);

                          BinUtils.WriteInt64(stream, cursor.ID);

                          var total = (Int32)stream.Position;
                          stream.Position = 0;
                          writeStandardHeader(stream, total, requestID, 0, OP_GET_MORE);
                          return total; 
                        }
                #endregion
          
          #endregion

  }

}
 
