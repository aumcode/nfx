using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Serialization.BSON;

namespace NFX.DataAccess.MongoDB.Connector
{

  /// <summary>
  /// Pairs necessary information for updates: query, update documents along with upsert and multi flags
  /// </summary>
  public struct UpdateEntry
  {
    public UpdateEntry(BSONDocument query, BSONDocument update, bool multi, bool upsert)
    {
      Query  = query;
      Update = update;
      Multi  = multi;
      Upsert = upsert;
    }

    public readonly BSONDocument Query;
    public readonly BSONDocument Update;
    public readonly bool Multi;
    public readonly bool Upsert;
  }

  /// <summary>
  /// Denotes limits for deletion: None=Everything or One
  /// </summary>
  public enum DeleteLimit
  {
    /// <summary>
    /// Everything that matches will be deleted
    /// </summary>
    None = 0,

    /// <summary>
    /// Only the first matching document will be deleted
    /// </summary>
    OnlyFirstMatch = 1
  }

  /// <summary>
  /// Pairs necessary information for deletes: query + limit flag
  /// </summary>
  public struct DeleteEntry
  {
    public DeleteEntry(BSONDocument query, DeleteLimit limit)
    {
      Query  = query;
      Limit  = limit;
    }

    public readonly BSONDocument Query;
    public readonly DeleteLimit Limit;
  }


  /// <summary>
  /// Returned by CRUD operations from the server, contains information about total docs affected and write errors.
  /// The caller needs to inspect the TotalDocumentsAffected and WriteErrors to handle error conditions
  /// </summary>
  public struct CRUDResult
  {
      internal CRUDResult(int n, int nModified, CRUDWriteError[] wErrors, CRUDUpsertInfo[] upserted)
      {
        TotalDocumentsAffected = n;
        TotalDocumentsUpdatedAffected = nModified;
        WriteErrors = wErrors;
        Upserted = upserted;
      }
      
      /// <summary>
      /// This field contains the aggregated number of documents successfully matched (n) by the entire write command. 
      /// This includes the number of documents inserted, upserted, updated, and deleted. We do not report on the 
      /// individual number of documents affected by each batch item. If the application would wish so, then the 
      /// application should issue one-item batches.
      /// </summary>
      public readonly int TotalDocumentsAffected;


      /// <summary>
      /// Optional field, with a positive numeric type or zero. 
      /// Zero is the default value. This field is only and always present for batch updates. 
      /// nModified is the physical number of documents affected by an update, while TotalDocumentsMatched is the logical number of documents matched by 
      /// the update's query.
      /// </summary>
      public readonly int TotalDocumentsUpdatedAffected;

      /// <summary>
      /// NULL or For every batch write that had an error, there is one entry in the array describing the error
      /// </summary>
      public readonly CRUDWriteError[] WriteErrors;

      /// <summary>
      /// NULL or for every batch document that was upserted
      /// </summary>
      public readonly CRUDUpsertInfo[] Upserted;
  }

  /// <summary>
  /// Provides information about document write error
  /// </summary>
  public struct CRUDWriteError
  {
      internal CRUDWriteError(int idx, int code, string msg, object info)
      {
        Index = idx;
        Code = code;
        Message = msg;
        Info = info;
      }
      
      /// <summary>
      /// WRITE ERROR ONLY, The index of the erroneous batch item relative to request batch order. Batch items indexes start with 0
      /// </summary>
      public readonly int Index;

      /// <summary>
      /// Mandatory field with integer format. Contains a numeric code corresponding to a certain type of error
      /// </summary>
      public readonly int Code;

      /// <summary>
      /// Mandatory field, containing a human-readable version of the error.
      /// </summary>
      public readonly string Message;

      /// <summary>
      /// Optional field, with a BSONObj format. This field contains structured information about an error that can be processed programmatically.
      /// For example, if a request returns with a shard version error, we may report the proper shard version as a sub-field here.
      /// For another example, if a write concern timeout occurred, the information previously reported on wtimeout would be reported here. 
      /// The format of this field depends on the code above
      /// </summary>
      public readonly object Info;
  }

  /// <summary>
  /// Provides information about an upserted document
  /// </summary>
  public struct CRUDUpsertInfo
  {
      internal CRUDUpsertInfo(int idx, object id)
      {
        Index = idx;
        ID = id;
      }
      
      /// <summary>
      /// Index of the document in the batch
      /// </summary>
      public readonly int Index;

      /// <summary>
      /// _ID value assigned by an upsert
      /// </summary>
      public readonly object ID;
  }

}
