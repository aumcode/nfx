/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
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
using System.Threading.Tasks;

using NFX.Log;
using NFX.Environment;
using NFX.Serialization.JSON;

namespace NFX.Wave.Filters
{
  /// <summary>
  /// Logs information extracted from WorkContext
  /// </summary>
  public class LoggingFilter : BeforeAfterFilterBase
  {
    #region CONSTS
      public const string VAR_TYPE = "type";
      public const string VAR_TEXT = "text";
      public const string VAR_FROM = "from";
    #endregion

    #region .ctor
      public LoggingFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order){ }
      public LoggingFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode): base(dispatcher, confNode){ }
      public LoggingFilter(WorkHandler handler, string name, int order) : base(handler, name, order){ }
      public LoggingFilter(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode){ }
    #endregion

    #region Protected

      protected override void DoBeforeWork(WorkContext work, JSONDataMap matched)
      {
        work.Log(
           matched[VAR_TYPE].AsEnum<MessageType>(MessageType.Info),
           matched[VAR_TEXT].AsString(work.About),
           matched[VAR_FROM].AsString("{0}.Before".Args(GetType().FullName)),
           pars: matched.ToJSON(JSONWritingOptions.CompactASCII)
           );
      }

      protected override void DoAfterWork(WorkContext work, JSONDataMap matched)
      {
        work.Log(
           matched[VAR_TYPE].AsEnum<MessageType>(MessageType.Info),
           matched[VAR_TEXT].AsString(work.About),
           matched[VAR_FROM].AsString("{0}.After".Args(GetType().FullName)),
           pars: matched.ToJSON(JSONWritingOptions.CompactASCII)
           );
      }

    #endregion
  }

}
