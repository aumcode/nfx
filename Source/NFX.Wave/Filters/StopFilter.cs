/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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

using NFX.Serialization.JSON;
using NFX.Environment;
using NFX.Wave.Templatization;
using ErrorPage=NFX.Wave.Templatization.StockContent.Error;

namespace NFX.Wave.Filters
{
  /// <summary>
  /// Stops the processing of WorkContext by throwing exception upon match
  /// </summary>
  public class StopFilter : BeforeAfterFilterBase
  {
    #region CONSTS
      public const string VAR_CODE = "code";
      public const string VAR_ERROR = "error";
    #endregion

    #region .ctor
      public StopFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order) { }
      public StopFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode): base(dispatcher, confNode) { }
      public StopFilter(WorkHandler handler, string name, int order) : base(handler, name, order) { }
      public StopFilter(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode) { }
    #endregion

    #region Protected
      protected override void DoBeforeWork(WorkContext work, JSONDataMap matched)
      {
        var code = matched[VAR_CODE].AsInt();
        var error = matched[VAR_ERROR].AsString();
        if (code > 0)
          throw new HTTPStatusException(code, error);
        else
          work.Aborted = true;
      }

      protected override void DoAfterWork(WorkContext work, JSONDataMap matched)
      {
        var code = matched[VAR_CODE].AsInt();
        var error = matched[VAR_ERROR].AsString();
        if (code > 0)
          throw new HTTPStatusException(code, error);
        else
          work.Aborted = true;
      }
    #endregion

  }

}
