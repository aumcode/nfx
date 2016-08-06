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
using System.Diagnostics;
using System.IO;
using System.Reflection;

using NFX;
using NFX.Environment;
using NFX.Templatization;
using NFX.IO;
using NFX.Glue.Tools;

namespace gluec
{
    class Program
    {
        static void Main(string[] args)
        {
          try
          {
           var w = Stopwatch.StartNew();
           run(args);
           w.Stop();
           ConsoleUtils.Info("Runtime: "+w.Elapsed);
           Environment.ExitCode = 0;
          }
          catch(Exception error)
          {
           ConsoleUtils.Error(error.ToMessageWithType());
           Environment.ExitCode = -1;
          }
        }


        private static void run(string[] args)
        {
          var config = new CommandArgsConfiguration(args);


          ConsoleUtils.WriteMarkupContent( typeof(Program).GetText("Welcome.txt") );

          if (config.Root["?", "h", "help"].Exists)
          {
             ConsoleUtils.WriteMarkupContent( typeof(Program).GetText("Help.txt") );
             return;
          }

          if (!config.Root.AttrByIndex(0).Exists)
            throw new Exception("Assembly is missing");

          var afn = config.Root.AttrByIndex(0).Value;
          if (string.IsNullOrWhiteSpace(afn))
            throw new Exception("Assembly empty path");

          afn = Path.GetFullPath(afn);

          ConsoleUtils.Info("Trying to load assembly: " + afn);

          var asm = Assembly.LoadFile(afn);

          ConsoleUtils.Info("Assembly file loaded OK");

          var tcompiler = typeof(CSharpGluecCompiler);
          var tcname = config.Root["c", "compiler"].AttrByIndex(0).Value;

          if (!string.IsNullOrWhiteSpace(tcname))
            tcompiler = Type.GetType(tcname, true);

          var compiler = Activator.CreateInstance(tcompiler, new object[] { asm }) as GluecCompiler;

          if (compiler==null) throw new Exception("Could not create compiler type");

          compiler.OutPath = Path.GetDirectoryName(afn);
          compiler.FilePerContract = true;
          compiler.NamespaceFilter = config.Root["f", "flt", "filter"].AttrByIndex(0).Value;

          ConfigAttribute.Apply(compiler, config.Root["o", "opt", "options"]);

          ConsoleUtils.Info("Namespace filter: " + compiler.NamespaceFilter);
          compiler.Compile();


        }


    }
}
