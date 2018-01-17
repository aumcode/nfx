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

using NFX;
using NFX.Environment;
using NFX.Templatization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest.Templatization.NHT
{
  public class NHTTemplate
  {
    public static string CONFIG = @"
    compiler
    {
      jsopt
      {
        dom-gen
        {
          pretty=true
        }
      }
    }
    ";

    public static void Run()
    {
      var tplSource = typeof(NHTTemplate).GetText("Template.htm");
      var config = CONFIG.AsLaconicConfig();
      var type = typeof(NHTCompiler);
      var args = new object[] {new TemplateStringContentSource(tplSource)};

      var cmp = FactoryUtils.MakeAndConfigure<NHTCompiler>(config, type, args);
      cmp.Compile();

      var unit = cmp.First();
      if (unit.CompilationException != null)
        Console.WriteLine(unit.CompilationException.ToMessageWithType());
      else
        Console.Write( unit.CompiledSource );
    }
  }
}
