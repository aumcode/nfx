/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
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

    public const string PROJECTION_ROOT = "$NFX-QUERY-PROJECTION";


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

    public static Query ID_EQ_BYTE_ARRAY(byte[] id)
    {
      var result = new Query();
      result.Set( NFX.Serialization.BSON.RowConverter.ByteBufferID_CLRtoBSON(_ID, id) );
      return result;
    }


    public Query() : base() { }
    public Query(Stream stream) : base(stream) { }

    /// <summary>
    /// Creates an instance of the query from JSON template with parameters populated from args optionally caching the template internal
    /// representation. Do not cache templates that change often
    /// </summary>
    public Query(string template, bool cacheTemplate, params TemplateArg[] args) : base(template, cacheTemplate, args)
    {
      var projNode = this[PROJECTION_ROOT] as BSONDocumentElement;
      if (projNode!=null)
      {
        this.ProjectionSelector = projNode.Value;
        this.Delete(PROJECTION_ROOT);
      }
    }

    /// <summary>
    /// Gets/sets projection document which should be embedded in query with '$NFX-QUERY-PROJECTION' see the PROJECTION_ROOT constant
    /// https://docs.mongodb.com/manual/tutorial/project-fields-from-query-results
    /// </summary>
    public BSONDocument ProjectionSelector{ get; set;}
  }

}
