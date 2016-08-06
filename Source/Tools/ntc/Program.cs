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

using NFX;
using NFX.Environment;
using NFX.Templatization;
using NFX.IO;

namespace ntc
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

          if (config.Root["?"].Exists ||
              config.Root["h"].Exists ||
              config.Root["help"].Exists)
          {
             ConsoleUtils.WriteMarkupContent( typeof(Program).GetText("Help.txt") );
             return;
          }



          if (!config.Root.AttrByIndex(0).Exists)
          {
            ConsoleUtils.Error("Template source path is missing");
            return;
          }


          var files =  getFiles(config.Root);

          var ctypename = config.Root["c"].AttrByIndex(0).Value ?? config.Root["compiler"].AttrByIndex(0).Value;

          var ctype = string.IsNullOrWhiteSpace(ctypename)? typeof(NFX.Templatization.TextCSTemplateCompiler) : Type.GetType(ctypename);
          if (ctype == null)
               throw new NFXException("Can not create compiler type: " + (ctypename??"<none>"));

          var compiler = Activator.CreateInstance(ctype) as TemplateCompiler;

          var onode = config.Root["options"];
          if (!onode.Exists) onode = config.Root["o"];
          if (onode.Exists)
          {
            ConfigAttribute.Apply(compiler, onode);
            var asms = onode.AttrByName("ref").ValueAsString(string.Empty).Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            foreach(var asm in asms)
             compiler.ReferenceAssembly(asm);
          }

          showOptions(compiler);

          foreach(var f in files)
          {
             var src = new FileTemplateStringContentSource(f);
             ConsoleUtils.Info("Included: " + src.GetName(50));
             compiler.IncludeTemplateSource(src);
          }

          compiler.Compile();

          if (compiler.HasErrors)
          {
            showErrors(compiler, config.Root);
            return;
          }

          if (config.Root["src"].Exists)
            writeToDiskCompiledSourceFiles(compiler, config.Root);


        }

                                  static IEnumerable<string> getFiles(IConfigSectionNode configRoot)
                                  {
                                      var pathArg = configRoot.AttrByIndex(0).Value;
                                      if (!Path.IsPathRooted(pathArg)) pathArg = "."+Path.DirectorySeparatorChar + pathArg;
                                      var rootPath = Path.GetDirectoryName(pathArg);
                                      var mask = Path.GetFileName(pathArg);
                                      if (mask.Length == 0) mask = "*";

                                      return rootPath.AllFileNamesThatMatch(mask, configRoot["r"].Exists || configRoot["recurse"].Exists);
                                  }


                                  static void showErrors(TemplateCompiler compiler, IConfigSectionNode configRoot)
                                  {
                                    ConsoleUtils.Warning("Compilation finished with errors");
                                    Console.WriteLine();
                                    ConsoleUtils.Info("Compile unit errors:");
                                    foreach(var erru in compiler.CompileUnitsWithErrors)
                                    {
                                     ConsoleUtils.Error(string.Format("Unit '{0}'. Type '{1}' Text: '{2}'",
                                                                         erru.TemplateSource.GetName(16),
                                                                         erru.CompilationException.GetType().Name,
                                                                         erru.CompilationException.Message));
                                    }
                                    Console.WriteLine();
                                    ConsoleUtils.Info(string.Format("{0} Code compilation errors:", compiler.LanguageName));
                                    foreach(var cerr in compiler.CodeCompilerErrors)
                                    {
                                     ConsoleUtils.Error( cerr.ToString() );
                                    }
                                  }

                                  static void writeToDiskCompiledSourceFiles(TemplateCompiler compiler, IConfigSectionNode configRoot)
                                  {
                                     var ext = configRoot["ext"].AttrByIndex(0).ValueAsString(compiler.LanguageSourceFileExtension);

                                      foreach(var cu in compiler)
                                      {
                                        if (cu.CompilationException!=null) continue;
                                        var fs = cu.TemplateSource as FileTemplateStringContentSource;
                                        if (fs==null) continue;
                                        File.WriteAllText(fs.FileName + ext , cu.CompiledSource);
                                      }
                                  }


                                  static void showOptions(TemplateCompiler compiler)
                                  {
                                     const string NONE = "<none>";
                                     Console.WriteLine();
                                     ConsoleUtils.Info("Compiler type: "+compiler.GetType().FullName);
                                     ConsoleUtils.Info("Assembly file: "+compiler.AssemblyFileName ?? NONE);
                                     ConsoleUtils.Info("Base template type name: "+compiler.BaseTypeName ?? NONE);
                                     ConsoleUtils.Info("Compile code: "+compiler.CompileCode ?? NONE);
                                     ConsoleUtils.Info("Language name: "+compiler.LanguageName ?? NONE);
                                     ConsoleUtils.Info("Namespace: "+compiler.Namespace ?? NONE);
                                     foreach(var asm in compiler.ReferencedAssemblies)
                                       ConsoleUtils.Info("Referenced assembly: "+asm);
                                     ConsoleUtils.Info("Ref assembly search path: "+compiler.ReferencedAssembliesSearchPath ?? NONE );
                                  }




    }
}
