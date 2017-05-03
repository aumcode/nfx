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

using NFX.Environment;

namespace NFX.Wave.Filters
{
  /// <summary>
  /// Upon match, redirects client to the specified URL resource. Specify matches with 'redirect-url' var
  /// </summary>
  public class RedirectFilter : WorkFilter
  {
    #region CONSTS
      public const string VAR_REDIRECT_URL = "redirect-url";
      public const string VAR_REDIRECT_TARGET = "redirect-target";

    #endregion

    #region .ctor
      public RedirectFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order) {}
      public RedirectFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode): base(dispatcher, confNode) { configureMatches(confNode); }
      public RedirectFilter(WorkHandler handler, string name, int order) : base(handler, name, order) {}
      public RedirectFilter(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode){ configureMatches(confNode); }

    #endregion

    #region Fields

      private OrderedRegistry<WorkMatch> m_RedirectMatches = new OrderedRegistry<WorkMatch>();

    #endregion

    #region Properties

      /// <summary>
      /// Returns matches used by the filter to determine whether redirect should be issued
      /// </summary>
      public OrderedRegistry<WorkMatch> RedirectMatches { get{ return m_RedirectMatches;}}

    #endregion

    #region Protected

      protected override void DoFilterWork(WorkContext work, IList<WorkFilter> filters, int thisFilterIndex)
      {
        foreach(var match in m_RedirectMatches.OrderedValues)
        {
          var matched = match.Make(work);
          if (matched==null) continue;

          var url = matched[VAR_REDIRECT_URL].AsString();
          if (url.IsNotNullOrWhiteSpace())
          {
            var target = matched[VAR_REDIRECT_TARGET].AsString();
            if (target.IsNotNullOrWhiteSpace())
            {
              var partsA = url.Split('#');
              var parts = partsA[0].Split('?');
              var query = parts.Length > 1 ? parts[0] + "&" : string.Empty;
              url = "{0}?{1}{2}={3}{4}".Args(parts[0], query,
                target, Uri.EscapeDataString(work.Request.Url.PathAndQuery),
                partsA.Length > 1 ? "#" + partsA[1] : string.Empty);
            }

            work.Response.RedirectAndAbort(url);
            return;
          }
        }

        this.InvokeNextWorker(work, filters, thisFilterIndex);
      }

    #endregion

    #region .pvt
     private void configureMatches(IConfigSectionNode confNode)
      {
        foreach(var cn in confNode.Children.Where(cn=>cn.IsSameName(WorkMatch.CONFIG_MATCH_SECTION)))
          if(!m_RedirectMatches.Register( FactoryUtils.Make<WorkMatch>(cn, typeof(WorkMatch), args: new object[]{ cn })) )
            throw new WaveException(StringConsts.CONFIG_OTHER_DUPLICATE_MATCH_NAME_ERROR.Args(cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value, "{0}".Args(GetType().FullName)));
      }

    #endregion
  }

}
