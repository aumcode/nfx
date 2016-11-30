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
using System.Threading;
using System.Net;


using NFX.Environment;
using NFX.Web.GeoLookup;


namespace NFX.Wave.Filters
{
  /// <summary>
  /// Upon match, injects real caller WorkContext.EffectiveCallerIPEndPoint
  /// </summary>
  public class EffectiveCallerIPEndPointFilter : WorkFilter
  {
    #region .ctor
      public EffectiveCallerIPEndPointFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order) {}
      public EffectiveCallerIPEndPointFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode): base(dispatcher, confNode) { ConfigAttribute.Apply(this, confNode); }
      public EffectiveCallerIPEndPointFilter(WorkHandler handler, string name, int order) : base(handler, name, order) {}
      public EffectiveCallerIPEndPointFilter(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode){ ConfigAttribute.Apply(this, confNode); }

    #endregion

    #region Properties

      [Config] public string  RealIpHdr  {  get; set; }
      [Config] public string  RealPortHdr{  get; set; }

    #endregion

    #region Protected

      protected override void DoFilterWork(WorkContext work, IList<WorkFilter> filters, int thisFilterIndex)
      {
        var ipep  = work.m_EffectiveCallerIPEndPoint;

        if (ipep==null)
        {
          var hip = RealIpHdr;
          var hprt = RealPortHdr;

          hip = hip.IsNotNullOrWhiteSpace() ? hip : "Real-IP";
          hprt  = hprt.IsNotNullOrWhiteSpace() ? hprt : "Real-Port";

          var rIP = work.Request.Headers[hip];
          var rPort = work.Request.Headers[hprt];

          if (rIP.IsNotNullOrWhiteSpace())
          {
            IPAddress address = null;
            if (IPAddress.TryParse(rIP, out address))
            {
              work.m_EffectiveCallerIPEndPoint = new IPEndPoint(address, rPort.AsInt(0));
            }
          }
        }

        this.InvokeNextWorker(work, filters, thisFilterIndex);
      }

    #endregion

  }

}
