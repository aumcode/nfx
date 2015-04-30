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

using NFX;
using NFX.Web;

namespace ConsoleTest.Social
{
  class SocialTest
  {
    public static void Test01()
    {
      var conf = @"nfx { web-settings { social {  
        provider { type='NFX.Web.Social.GooglePlus, NFX.Web' client-code='396843619799' client-secret='0p8bbUnpmpH-q8APtbJVy0Oo' web-service-call-timeout-ms='20000' keep-alive='false' pipelined='false'}
} } }".AsLaconicConfig();
      using (var app = new NFX.ApplicationModel.ServiceBaseApplication(new string[] {}, conf))
      {
        var social = WebSettings.SocialNetworks;
      }
    }
  }
}
