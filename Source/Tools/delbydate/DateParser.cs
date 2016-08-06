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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 0.2  2009.07.03
 */
using System;
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CSharp;


namespace delbydate
{
  class DateParser
  {
    public DateParser(string expr)
    {


      CSharpCodeProvider comp = new CSharpCodeProvider();

      CompilerParameters cp = new CompilerParameters();
      cp.ReferencedAssemblies.Add("system.dll");
      cp.GenerateExecutable = false;
      cp.GenerateInMemory = true;

      StringBuilder code = new StringBuilder();
      code.Append("using System; \n");
      code.Append("namespace Temp { \n");
      code.Append("  public class Evaluator { \n");

      code.Append("  public bool DoEval(DateTime file) \n");
      code.Append("  { \n");

      code.Append(" return (" + expr + ");\n");

      code.Append("  } \n");

      code.Append("} \n");//class
      code.Append("} \n");//namespace


      CompilerResults cr = comp.CompileAssemblyFromSource(cp, code.ToString());
      if (cr.Errors.HasErrors)
      {
        StringBuilder error = new StringBuilder();
        error.Append("Error Compiling Expression: ");
        foreach (CompilerError err in cr.Errors)
        {
          error.AppendFormat("{0}\n", err.ErrorText);
        }
        throw new Exception("Error Compiling Expression: " + error.ToString());
      }

      Assembly asm = cr.CompiledAssembly;
      evalObj = asm.CreateInstance("Temp.Evaluator");

    }//Eval

    private object evalObj;

    public bool Eval(DateTime fileDateTime)
    {
      MethodInfo mi = evalObj.GetType().GetMethod("DoEval");
      return (bool)mi.Invoke(evalObj, new object[] { fileDateTime });
    }//Eval


  }
};
