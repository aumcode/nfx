using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NFX.Serialization.BSON;

namespace NFX.DataAccess.MongoDB.Connector
{
  /// <summary>
  /// Represents an update document sent to MongoDB
  /// </summary>
  public class Update : BSONDocument
  {

    public Update() : base() { }
    public Update(Stream stream) : base(stream) { }
   
    /// <summary>
    /// Creates an instance of the update from JSON template with parameters populated from args optionally caching the template internal
    /// representation. Do not cache templates that change often
    /// </summary>
    public Update(string template, bool cacheTemplate, params TemplateArg[] args):base(template, cacheTemplate, args)
    {

    }

  }

}
