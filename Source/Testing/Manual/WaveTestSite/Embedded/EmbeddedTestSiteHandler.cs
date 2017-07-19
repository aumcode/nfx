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

using NFX;
using NFX.Web;
using NFX.Wave;
using NFX.Wave.Handlers;
using NFX.Environment;

namespace WaveTestSite.Embedded
{
  /// <summary>
  /// This handler serves the particular embedded site
  /// </summary>
  public class EmbeddedTestSiteHandler : EmbeddedSiteHandler
  {
    public EmbeddedTestSiteHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match)
                          : base(dispatcher, name, order, match){}


    public EmbeddedTestSiteHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode)
                          : base(dispatcher, confNode) {}


    public override string RootResourcePath
    {
      get { return "WaveTestSite.Embedded"; }
    }

    protected override IEnumerable<EmbeddedSiteHandler.IAction> GetActions()
    {
      yield return new CountAction();
    }
  }

  /// <summary>
  /// Counts from one int to another
  /// </summary>
  public class CountAction : EmbeddedSiteHandler.IAction
  {
    public string Name{ get { return "Count"; } }

    public void Perform(WorkContext context)
    {
      var from = context.Request.QueryString["from"].AsInt(1);
      var to   = context.Request.QueryString["to"].AsInt(10);

      if (to-from>1000) to = from + 1000;//limit so no infinite loop possible

      context.Response.ContentType = ContentType.TEXT;
      for(var i=from;i<=to;i++)
        context.Response.WriteLine("{0} times and counting".Args(i));
    }
  }


}
