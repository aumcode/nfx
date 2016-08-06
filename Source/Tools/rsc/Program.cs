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
using NFX.RelationalModel;
using NFX.Environment;
using NFX.Templatization;
using NFX.IO;

namespace rsc
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
            throw new Exception("Schema file missing");

          var schemaFileName = config.Root.AttrByIndex(0).Value;
          if (string.IsNullOrWhiteSpace(schemaFileName))
            throw new Exception("Schema empty path");

          schemaFileName = Path.GetFullPath(schemaFileName);

          ConsoleUtils.Info("Trying to load schema: " + schemaFileName);

          var schema = new Schema(schemaFileName, new string[] { Path.GetDirectoryName(schemaFileName) });

          ConsoleUtils.Info("Schema file loaded OK");

          var tcompiler = typeof(MySQLCompiler);
          var tcname = config.Root["c", "compiler"].AttrByIndex(0).Value;

          if (!string.IsNullOrWhiteSpace(tcname))
            tcompiler = Type.GetType(tcname, true);

          var compiler = Activator.CreateInstance(tcompiler, new object[] { schema }) as Compiler;

          if (compiler==null) throw new Exception("Could not create compiler type");



          compiler.OutputPath = Path.GetDirectoryName(schemaFileName);

          var options = config.Root["o","opt","options"];
          if (options.Exists)
            compiler.Configure(options);



          ConsoleUtils.Info("Compiler information:");
          Console.WriteLine("   Type={0}\n   Name={1}\n   Target={2}".Args(compiler.GetType().FullName, compiler.Name, compiler.Target) );
          if (compiler is RDBMSCompiler)
            Console.WriteLine("   DomainsSearchPath={0}".Args(((RDBMSCompiler)compiler).DomainSearchPaths) );
          Console.WriteLine("   OutPath={0}".Args(compiler.OutputPath) );
          Console.WriteLine("   OutPrefix={0}".Args(compiler.OutputPrefix) );
          Console.WriteLine("   CaseSensitive={0}".Args(compiler.CaseSensitiveNames) );

          compiler.Compile();



          foreach(var error in compiler.CompileErrors)
            ConsoleUtils.Error(error.ToMessageWithType());


          if (compiler.CompileException!=null)
          {
            ConsoleUtils.Warning("Compile exception thrown");
            throw compiler.CompileException;
          }
        }

    }
}
