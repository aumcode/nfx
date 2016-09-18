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

using NFX.IO;

namespace ConsoleTest
{
  class Markup
  {
    public static void Run()
    {
      ConsoleUtils.WriteMarkupContent(
       @"Hello nash drug!<push><f color=""red""><b color=""yellow"">KOZEL!<br><pop>normalno<br><br>
namesnik zarya v Goroxovke! <br>
Table:<br>
A kto ya?<space>pyaffka!<br>
<push>
<f color=""green""><b color=""gray"">
<j width=""10"" dir=""right"" text=""Name""> <j width=""20"" dir=""center"" text=""Value""><br>
<pop>
<j width=""10"" dir=""right"" text=""Jaba""> <j width=""20"" dir=""center"" text=""71-983619786238""><br>
<j width=""10"" dir=""right"" text=""Kathy""> <j width=""20"" dir=""center"" text=""Yes""><br>
<j width=""10"" dir=""right"" text=""Kith""> <j width=""20"" dir=""center"" text=""71238""><br>
<j width=""10"" dir=""right"" text=""Laima""> <j width=""20"" dir=""center"" text=""Yes""><br>
<j width=""10"" dir=""right"" text=""Alex""> <j width=""20"" dir=""center"" text=""71269838""><br>
<j width=""10"" dir=""right"" text=""Kollab""> <j width=""20"" dir=""center"" text=""Yes""><br>
<j width=""10"" dir=""right"" text=""Xoxma""> <j width=""20"" dir=""center"" text=""7126983619786238""><br>
<j width=""10"" dir=""right"" text=""San-Bolla""> <j width=""20"" dir=""center"" text=""Yes""><br>
        "
       );
       
      

      Console.ReadLine();
    }
  }
}
