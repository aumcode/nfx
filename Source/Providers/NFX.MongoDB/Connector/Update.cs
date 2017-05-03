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
