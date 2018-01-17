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
  /// <summary>
  /// Adds numbers
  /// </summary>
  public class InheritBase : Controller
  {

    [Action]
    public string Echo1(string msg)
    {
      return "Base:Echo1 "+msg;
    }

    [Action]
    public string EchoNew(string msg)
    {
      return "Base:EchoNew "+msg;
    }

    [Action]
    public virtual string EchoVirtual(string msg)
    {
      return "Base:EchoVirtual "+msg;
    }
  }


}
