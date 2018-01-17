/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
using System.Text;

using NFX.Environment;
using NFX.Serialization.BSON;
using NFX.Serialization.JSON;

namespace NFX.Log.Destinations
{
    /// <summary>
    /// Provides detailed JSON-based logging format
    /// </summary>
    public sealed class JSONDestination : TextFileDestination
    {
      private const string CONFIG_JSON_OPT_SECTION = "json-options";

      public JSONDestination() : this(null) { }
      public JSONDestination(string name = null) : base(name)
      {
         m_Options = JSONWritingOptions.CompactASCII;
      }

      private BSONSerializer m_Serializer = new BSONSerializer();
      private IBSONSerializable m_Known = new BSONParentKnownTypes(typeof(Message));

      private JSONWritingOptions m_Options;

      protected override void DoConfigure(IConfigSectionNode node)
      {
        base.DoConfigure(node);
        if (node==null) return;

        var on = node[CONFIG_JSON_OPT_SECTION];
        if (on.Exists) m_Options = FactoryUtils.MakeAndConfigure<JSONWritingOptions>(on, typeof(JSONWritingOptions));
      }

      protected override string DoFormatMessage(Message msg)
      {
        var doc = m_Serializer.Serialize(msg, m_Known);
        return doc.ToJSON(m_Options)+"\n\n";
      }

    }
}
