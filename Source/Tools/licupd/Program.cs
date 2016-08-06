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
using System.IO;

using System.Text.RegularExpressions;

using NFX;
using NFX.Environment;

namespace licupd
{
  class Program
  {
    static void Main(string[] str_args)
    {
      Console.WriteLine("NFX License Block Updater");
      Console.WriteLine(" Usage:");
      Console.WriteLine("   licupd  path lfile [-pat pattern] [-insert]");
      Console.WriteLine("    path - path to code base");
      Console.WriteLine("    lfile - path to license file");
      Console.WriteLine("    [-pat pattern] - optional pattern search such as '-pat *.txt'. Assumes '*.cs' if omitted");
      Console.WriteLine("    [-insert] - optional switch that causes license block insertion when missing");

      var args = new CommandArgsConfiguration(str_args);

      var path =  args.Root.AttrByIndex(0).ValueAsString(string.Empty);

      var license = File.ReadAllText(args.Root.AttrByIndex(1).ValueAsString(string.Empty));

      var fpat = args.Root["pat"].AttrByIndex(0).ValueAsString("*.cs");

      var insert = args.Root["insert"].Exists;


      var regexp = new Regex(
      @"/\*<FILE_LICENSE>[\s*|\S*]{0,}</FILE_LICENSE>\*/"

      );

      var replaced = 0;
      var inserted = 0;

      foreach(var fn in path.AllFileNamesThatMatch(fpat, true))
      {
       var content = File.ReadAllText(fn);

       if (regexp.Match(content).Success)
       {
          Console.Write("Matched:  " + fn);
          File.WriteAllText(fn, regexp.Replace(content, license));
          Console.WriteLine("   Replaced.");
          replaced++;
       }
       else
        if (insert)
        {
          content = license + Environment.NewLine + content;
          File.WriteAllText(fn, content);
          Console.WriteLine("Inserted:  " + fn);
          inserted++;
        }
      }//freach file

     Console.WriteLine(string.Format("Total Replaced: {0}  Inserted: {1}", replaced, inserted));
     Console.WriteLine("Strike <enter>");
     Console.ReadLine();
    }
  }
}
