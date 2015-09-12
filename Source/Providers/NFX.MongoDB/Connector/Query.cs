using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NFX.DataAccess.Distributed;
using NFX.Serialization.BSON;

namespace NFX.DataAccess.MongoDB.Connector
{
  /// <summary>
  /// Represents a query document sent to MongoDB
  /// </summary>
  public class Query : BSONDocument
  {
    public const string _ID = Protocol._ID;


    public static Query ID_EQ_Int32(Int32 id)
    {
      var result = new Query();
      result.Set( new BSONInt32Element(_ID, id) );
      return result;
    }

    public static Query ID_EQ_Int32(Int64 id)
    {
      var result = new Query();
      result.Set( new BSONInt64Element(_ID, id) );
      return result;
    }

    public static Query ID_EQ_String(string id)
    {
      if (id==null)
       throw new MongoDBConnectorException(StringConsts.ARGUMENT_ERROR+"ID_EQ_String(id==null)");

      var result = new Query();
      result.Set( new BSONStringElement(_ID, id) );
      return result;
    }

    public static Query ID_EQ_GDID(GDID id)
    {
      var result = new Query();
      result.Set( NFX.Serialization.BSON.RowConverter.GDID_CLRtoBSON(_ID, id) );
      return result;
    }


    public Query() : base() { }
    public Query(Stream stream) : base(stream) { }
   
    /// <summary>
    /// Creates an instance of the query from JSON template with parameters populated from args optionally caching the template internal
    /// representation. Do not cache templates that change often
    /// </summary>
    public Query(string template, bool cacheTemplate, params TemplateArg[] args):base(template, cacheTemplate, args)
    {

    }

  }

}
