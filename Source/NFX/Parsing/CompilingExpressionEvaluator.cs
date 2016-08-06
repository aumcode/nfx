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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CSharp;

namespace NFX.Parsing
{
    /// <summary>
    /// Implements an evaluator that compiles all expressions represented by instances of this class in a certain scope into dynamic assemblies.
    /// Every unique scope name creates a separate assembly.
    /// The compilation of scope is triggered either by a call to Compile() or first attempt to call Evaluate() on any instance within a scope.
    /// Once a scope has been compiled, no further allocations in this scoped are allowed, this is because CLR does not allow to unload assemblies dynamically.
    /// Within an expression context is passed as "ctx" and argument as "arg".
    /// This class is thread-safe.
    /// </summary>
    public class CompilingExpressionEvaluator<TContext, TResult, TArg>
    {
       #region inner
            private class _ScopeData
            {
              internal Assembly Assembly;
              internal NFXException CompileException;
              internal List<CompilingExpressionEvaluator<TContext, TResult, TArg>> m_Expressions = new List<CompilingExpressionEvaluator<TContext, TResult, TArg>>();

              internal List<string> m_ReferencedAssemblies = new List<string>();
              internal List<string> m_Usings = new List<string>();

              public _ScopeData()
              {
                 m_ReferencedAssemblies.Add("System.dll");
                 m_ReferencedAssemblies.Add("System.Core.dll");
                 m_ReferencedAssemblies.Add("System.Data.dll");
                 m_ReferencedAssemblies.Add("System.Xml.dll");
                 m_ReferencedAssemblies.Add("System.Xml.Linq.dll");
                 m_ReferencedAssemblies.Add("NFX.dll");

                 m_Usings.Add("System");
                 m_Usings.Add("System.Text");
                 m_Usings.Add("System.IO");
                 m_Usings.Add("System.Collections.Generic");
                 m_Usings.Add("System.Linq");
                 m_Usings.Add("NFX");
              }


            }

            private class _Scopes : Dictionary<string, _ScopeData>{ }

            private static class _G
            {
              internal static _Scopes s_Scopes = new _Scopes();
            }

       #endregion


       #region Static

        /// <summary>
        /// Indicates whether this scope has already been compiled and no more epressions can be allocated in it
        /// </summary>
        public static bool IsScopeAlreadyCompiled(string scope)
        {
            _ScopeData sdata = null;
            lock(_G.s_Scopes)
            {
              if (_G.s_Scopes.TryGetValue(scope, out sdata))
                  if (sdata.Assembly!=null || sdata.CompileException!=null) return true;
            }

            return false;
        }


       #endregion

       #region .ctor
         /// <summary>
         /// Allocates a new expression. This call fails if the scope was already compiled
         /// </summary>
         /// <param name="scope">A valid identifier for namespace sub-path like "mycode.test" no leading or trailing "."</param>
         /// <param name="expression">A C# expression to compile</param>
         /// <param name="referencedAssemblies"> An enumerable of assemblies that compiler should reference while building scope assembly i.e. "MyCompany.dll"</param>
         /// <param name="usings">Extra usings i.e. "System.IO", "MyCode.Routines" etc.</param>
         public CompilingExpressionEvaluator(string scope,
                                             string expression,
                                             IEnumerable<string> referencedAssemblies = null,
                                             IEnumerable<string> usings = null)
         {
            if (string.IsNullOrWhiteSpace(scope)||
                string.IsNullOrWhiteSpace(expression)) throw new ArgumentException(StringConsts.ARGUMENT_ERROR + "CompilingExpressionEvaluator(scope,expression)");

            m_Scope = scope.Trim();
            m_Expression = expression;

            _ScopeData sdata = null;
            lock(_G.s_Scopes)
            {
              if (!_G.s_Scopes.TryGetValue(scope, out sdata))
              {
                sdata = new _ScopeData();
                _G.s_Scopes.Add(scope, sdata);
              }

              if (sdata.Assembly!=null)
                 throw new NFXException(StringConsts.CAN_NOT_CREATE_MORE_SCOPE_EXPRESSIONS_ERROR + scope);

              if (sdata.CompileException!=null)
                 throw new NFXException(StringConsts.CAN_NOT_ADD_FAILED_SCOPE_COMPILE_ERROR + scope, sdata.CompileException);

              sdata.m_Expressions.Add(this);

              if (referencedAssemblies!=null)
                 foreach(var ass in referencedAssemblies)
                   if (!sdata.m_ReferencedAssemblies.Contains(ass))
                     sdata.m_ReferencedAssemblies.Add(ass);

              if (usings!=null)
                 foreach(var use in usings)
                   if (!sdata.m_Usings.Contains(use))
                     sdata.m_Usings.Add(use);
            }
         }

       #endregion


       #region Private Fields

         private string m_Scope;
         private string m_Expression;

         private MethodInfo m_EvalMethod;

       #endregion


       #region Properties

         /// <summary>
         /// Returns a scope (similar to compilation unit / assembly) that this expression is in
         /// </summary>
         public string Scope
         {
           get { return m_Scope;}
         }

         /// <summary>
         /// Returns an original expression as string that is to be evaluated
         /// </summary>
         public string Expression
         {
           get { return m_Expression;}
         }

       #endregion


       #region Public

          /// <summary>
          /// Forces the entire scope compilation now, so no delay is incurred on first call to Evaluate().
          /// </summary>
          public void Compile()
          {
             lock(_G.s_Scopes)
             {
               var sdata = _G.s_Scopes[m_Scope];//this can not be not found because otherwise .ctor would have failed
               if (sdata.Assembly!=null) return;//nothing to do
               if (sdata.CompileException!=null) throw sdata.CompileException;

               doCompile(sdata);

               if (sdata.CompileException!=null) throw sdata.CompileException;
             }
          }


          /// <summary>
          /// Evaluates expression using supplied arg in a context.
          /// Context is passed as "ctx" and argument as "arg".
          /// </summary>
          public TResult Evaluate(TContext context, TArg arg)
          {
            if (m_EvalMethod==null) Compile();

            return (TResult) m_EvalMethod.Invoke(null, new object[] {context, arg});
          }


       #endregion

       #region .pvt .impl

         private void doCompile(_ScopeData sdata)
         {
            CSharpCodeProvider comp = new CSharpCodeProvider();

            CompilerParameters cp = new CompilerParameters();
              foreach(var ass in sdata.m_ReferencedAssemblies)
                 cp.ReferencedAssemblies.Add(ass);
              cp.GenerateExecutable = false;
              cp.GenerateInMemory = true;


            var ns = "NFX.Parsing.CompilingExpressionEvaluator."+m_Scope;

            StringBuilder code = new StringBuilder();
              foreach(var use in sdata.m_Usings)
                code.AppendLine("using " + use + ";");

              code.AppendLine("namespace "+ns+" { ");
              code.AppendLine("  public static class Evaluator { ");

              for(var i=0; i<sdata.m_Expressions.Count; i++)
              {
                 var expr = sdata.m_Expressions[i];

                      code.AppendLine(
                        string.Format(" public static {0} DoEval_{1}({2} ctx, {3} arg)",
                           tname(typeof(TResult)),
                           i,
                           tname(typeof(TContext)),
                           tname(typeof(TArg))
                        )
                      );

                      code.AppendLine("  { ");

                      code.AppendLine("       return (" + expr.m_Expression + ");");

                      code.AppendLine("  } ");
                      code.AppendLine();
              }//for

              code.AppendLine("}");//class
              code.AppendLine("}");//namespace

            CompilerResults cr = comp.CompileAssemblyFromSource(cp, code.ToString());
              if (cr.Errors.HasErrors)
              {
                StringBuilder error = new StringBuilder();
                error.Append("Error Compiling Expression: ");
                foreach (CompilerError err in cr.Errors)
                {
                  error.AppendFormat("{0}\n", err.ErrorText);
                }

                sdata.CompileException =  new NFXException(StringConsts.EXPRESSION_SCOPE_COMPILE_ERROR + error.ToString());
                return;
              }

            sdata.Assembly = cr.CompiledAssembly;

            for(var i=0; i<sdata.m_Expressions.Count; i++)
            {
               var expr = sdata.m_Expressions[i];
               var type = sdata.Assembly.GetType(ns+".Evaluator");
               var method = type.GetMethod("DoEval_"+i.ToString());

               expr.m_EvalMethod = method;
            }
         }


         private string tname(Type type)
         {
           var result =  type.Namespace + "." + type.Name;

           if (type.IsGenericType)
           {
             result = result.Remove(result.LastIndexOf('`'));

             var gal = string.Empty;
             foreach(var at in type.GetGenericArguments())
             {
               gal += tname(at) + ",";
             }
             gal = gal.Remove(gal.Length-1);
             result = result + "<"+gal+">";
           }
           return result;
         }

       #endregion

    }
}
