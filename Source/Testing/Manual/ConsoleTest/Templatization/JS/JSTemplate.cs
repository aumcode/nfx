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

using NFX;
using NFX.Environment;
using NFX.Templatization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest.Templatization.JS
{
  public class JSTemplate
  {
    public static string CONFIG = @"
    compiler
    {
      dom-gen
      {
        pretty=true
      }
    }
    ";

    public static void Run()
    {
      var tplSource = typeof(JSTemplate).GetText("_source.js");

      var tpl =  new FileTemplateStringContentSource(@"C:\Users\opana\Desktop\nfx\Source\Testing\Manual\ConsoleTest\Templatization\JS\_source.js");
      var config = CONFIG.AsLaconicConfig();
      var type = typeof(TextJSTemplateCompiler);
      var args = new object[] {tpl};//new TemplateStringContentSource(tplSource)};

      var cmp = FactoryUtils.MakeAndConfigure<TextJSTemplateCompiler>(config, type, args);
      cmp.Compile();

      var unit = cmp.First();
      if (unit.CompilationException != null)
        Console.WriteLine(unit.CompilationException.ToMessageWithType());
      else
      {
        Console.WriteLine( unit.CompiledSource );
        File.WriteAllText(@"..\..\Source\Testing\Manual\ConsoleTest\Templatization\JS\compiled.js", unit.CompiledSource);
        Console.WriteLine("Saved to file");
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
      }
    }
  }
}
