using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Reflection;

using NFX;
using NFX.IO;
using NFX.Environment;
using NFX.Serialization.Arow;

namespace arow
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


          var asmFileName = config.Root.AttrByIndex(0).Value;
          var path = config.Root.AttrByIndex(1).Value;

          if (asmFileName.IsNullOrWhiteSpace()) throw new Exception("Assembly missing");
          if (path.IsNullOrWhiteSpace()) throw new Exception("Output path is missing");

          if (!File.Exists(asmFileName)) throw new Exception("Assembly file does not exist");
          if (!Directory.Exists(path)) throw new Exception("Output path does not exist");

          var assembly = Assembly.LoadFrom(asmFileName);

          using(var generator = new CodeGenerator())
          {
            generator.RootPath = path;
            generator.CodeSegregation = config.Root["c", "code"].ValueAsEnum<CodeGenerator.GeneratedCodeSegregation>(CodeGenerator.GeneratedCodeSegregation.FilePerNamespace);
            generator.Generate( assembly );
          }
        }
  }
}
