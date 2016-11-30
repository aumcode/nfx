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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;

using NFX;
using NFX.Wave.MVC;
using NFX.Security.CAPTCHA;
using NFX.Serialization.JSON;
using NFX.DataAccess;
using NFX.DataAccess.CRUD;
using WaveTestSite.Pages;

namespace WaveTestSite.Controllers
{

  public class CookieTest : Controller
  {
    [Action]
    public string SetCookies()
    {
      var c1 = new Cookie
      {
        Name="cookieA",
        Value="valueA"
      };

      var c2 = new Cookie
      {
        Name = "cookieB",
        Value = "valueB"
      };

      //WorkContext.Response.AppendCookie(c1);
      //WorkContext.Response.AppendCookie(c2);

      WorkContext.Response.AddHeader("Set-Cookie", "{0}={1}; Expires=Wed, 09 Jun 2021 10:18:14 GMT; Path=/;".Args(c1.Name, c1.Value));
      WorkContext.Response.AddHeader("Set-Cookie-New", "{0}={1}; Expires=Wed, 09 Jun 2021 10:18:14 GMT; Path=/;".Args(c2.Name, c2.Value));
      return "OK!";
    }
  }
}
