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
using System.Reflection;


using NFX.Glue;
using NFX.Environment;
using NFX.IO;
using NFX;


namespace NFX.Glue.Tools
{
    /// <summary>
    /// CS Gluec compiler
    /// </summary>
    public class CSharpGluecCompiler : GluecCompiler
    {
       public CSharpGluecCompiler(Assembly asm) : base (asm)
       {

       }


       private string m_RootNamespace;
       private string m_NamespaceSuffix = "GluedClients";
       private string m_ClassSuffix = "Client";


       [Config("$ns-root")]
       public string RootNamespace
       {
         get
         {
           return m_RootNamespace ?? string.Empty;
         }
         set
         {
           if (value!=null) value = value.TrimEnd('.',' ');
           m_RootNamespace = value;
         }
       }

       [Config("$ns-suffix")]
       public string NamespaceSuffix
       {
         get
         {
           return m_NamespaceSuffix ?? string.Empty;
         }
         set
         {
           if (value!=null) value = value.TrimStart('.',' ');
           m_NamespaceSuffix = value;
         }
       }

       [Config("$cl-suffix")]
       public string ClassSuffix
       {
         get
         {
          return m_ClassSuffix ?? string.Empty;
         }
         set
         {
           if (value!=null && value.StartsWith(".")) value = value.Remove(0,1);
           m_ClassSuffix = value;
         }
       }


       public override void Compile()
       {
           var sb = new StringBuilder();
           var fpath = Path.Combine(OutPath, this.ClassSuffix+"s.cs");



           foreach(var ns in Namespaces)
           {
             var types = m_Assembly.GetTypes().Where(t => t.Namespace == ns && t.IsInterface && Attribute.IsDefined(t, typeof(GluedAttribute)));

             foreach(var tp in types)
             {
                var fn = compileContract(sb, tp);
                if (FilePerContract)
                {
                  fpath = Path.Combine(OutPath, fn);
                  writeFile(fpath, sb);
                  sb = new StringBuilder();
                }
             }

             if (!FilePerContract)
                writeFile(fpath, sb);
           }
       }

       protected virtual void BeforeClientClass(StringBuilder sb, Type tc, string cname, string iname) {}

       protected virtual string GetClientInterfaceName(Type tc)
       {
         return TypeToStr(tc);
       }

       private void writeFile(string fpath, StringBuilder sb)
       {
         if (sb.Length>0)
          File.WriteAllText(fpath, sb.ToString());
       }

       private string compileContract(StringBuilder sb, Type tc)
       {
         if (tc.IsGenericTypeDefinition)
          throw new NotSupportedException("Glue does not support contracts that have generic arguments. Contract: '{0}'".Args(tc.FullName));

         m_ContractMethods = new List<MethodInfo>();
         var newFile = sb.Length==0;

         if (newFile) sb.AppendLine( FileHeader() );

         var nsname = (m_RootNamespace.IsNullOrWhiteSpace()? tc.Namespace : m_RootNamespace) + (NamespaceSuffix.IsNullOrWhiteSpace() ? string.Empty : ("." + NamespaceSuffix));
         var cname = tc.Name;
         if (cname.StartsWith("I")) cname = cname.Remove(0,1);

         cname += ClassSuffix;

         var iname = GetClientInterfaceName(tc);

         Console.WriteLine(string.Format(" {0} -> {1}.{2}", tc.FullName, nsname, cname));

         sb.AppendLine();
         sb.AppendLine("namespace " + nsname);
         sb.AppendLine("{");

         BeforeClientClass(sb, tc, cname, iname);

         sb.AppendLine("  ///<summary>");
         sb.AppendLine("  /// Client for glued contract "+ tc.FullName + " server.");
         sb.AppendLine("  /// Each contract method has synchronous and asynchronous versions, the later denoted by 'Async_' prefix.");
         sb.AppendLine("  /// May inject client-level inspectors here like so:");
         sb.AppendLine("  ///   client.MsgInspectors.Register( new YOUR_CLIENT_INSPECTOR_TYPE());");
         sb.AppendLine("  ///</summary>");
         sb.AppendLine("  public class " + cname + " : ClientEndPoint, " + iname);
         sb.AppendLine("  {");
         sb.AppendLine();
         sb.AppendLine("  #region Static Members");
         sb.AppendLine();
         sb.AppendLine("     private static TypeSpec s_ts_CONTRACT;");
         foreach(var mi in tc.GetMethods())
           sb.AppendLine("     private static MethodSpec " + msName(mi) + ";");
         sb.AppendLine();
         sb.AppendLine("     //static .ctor");
         sb.AppendLine("     static "+cname+"()");
         sb.AppendLine("     {");
         sb.AppendLine("         var t = typeof("+TypeToStr( tc )+");");
         sb.AppendLine("         s_ts_CONTRACT = new TypeSpec(t);");
         foreach(var mi in tc.GetMethods())
          sb.AppendLine("         "+ msName(mi) + " = new MethodSpec(t.GetMethod("+ argsGetMethod(mi)+"));");
         sb.AppendLine("     }");
         sb.AppendLine("  #endregion");

         sb.AppendLine();
         sb.AppendLine("  #region .ctor");

         sb.AppendLine("     public "+cname+"(string node, Binding binding = null) : base(node, binding) { ctor(); }");
         sb.AppendLine("     public "+cname+"(Node node, Binding binding = null) : base(node, binding) { ctor(); }");
         sb.AppendLine("     public "+cname+"(IGlue glue, string node, Binding binding = null) : base(glue, node, binding) { ctor(); }");
         sb.AppendLine("     public "+cname+"(IGlue glue, Node node, Binding binding = null) : base(glue, node, binding) { ctor(); }");

         sb.AppendLine();

         sb.AppendLine("     //common instance .ctor body");
         sb.AppendLine("     private void ctor()");
         sb.AppendLine("     {");
         sb.AppendLine();
         sb.AppendLine("     }");

         sb.AppendLine();

         sb.AppendLine("  #endregion");

         sb.AppendLine();


         sb.AppendLine("     public override Type Contract");
         sb.AppendLine("     {");
         sb.AppendLine("       get { return typeof("+TypeToStr(tc)+"); }");
         sb.AppendLine("     }");

         sb.AppendLine();
         sb.AppendLine();
         sb.AppendLine();

         sb.AppendLine("  #region Contract Methods");

         foreach(var mi in tc.GetMethods())
         {
            var oneWay = Attribute.IsDefined(mi, typeof(OneWayAttribute));
            sb.AppendLine();
            if (oneWay)
            {
              sb.AppendLine("         ///<summary>");
              sb.AppendLine("         /// Synchronous invoker for  '"+ tc.FullName + "."+mi.Name+"'.");
              sb.AppendLine("         /// This is a one-way call per contract specification, meaning - the server sends no acknowledgement of this call receipt and");
              sb.AppendLine("         /// there is no result that server could return back to the caller.");
              sb.AppendLine("         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.");
              sb.AppendLine("         ///</summary>");
              sb.AppendLine("         public void @"+mi.Name + mSig(mi, true));
              sb.AppendLine("         {");
              sb.AppendLine("            var call = Async_"+mi.Name + mSig(mi, false)+";");
              sb.AppendLine("            if (call.CallStatus != CallStatus.Dispatched)");
              sb.AppendLine("                throw new ClientCallException(call.CallStatus, \"Call failed: '"+cname+"."+mi.Name+"'\");");
              sb.AppendLine("         }");
            }
            else
            {
              if (mi.ReturnType!=typeof(void) && mi.ReturnType!=null)
              {
                  sb.AppendLine("         ///<summary>");
                  sb.AppendLine("         /// Synchronous invoker for  '"+ tc.FullName + "."+mi.Name+"'.");
                  sb.AppendLine("         /// This is a two-way call per contract specification, meaning - the server sends the result back either");
                  sb.AppendLine("         ///  returning '"+TypeToStr(mi.ReturnType)+"' or RemoteExceptionData instance.");
                  sb.AppendLine("         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.");
                  sb.AppendLine("         /// RemoteException is thrown if the server generated exception during method execution.");
                  sb.AppendLine("         ///</summary>");
                  sb.AppendLine("         public "+TypeToStr(mi.ReturnType)+" @"+mi.Name + mSig(mi, true));
                  sb.AppendLine("         {");
                  sb.AppendLine("            var call = Async_"+mi.Name + mSig(mi, false)+";");
                  sb.AppendLine("            return call.GetValue<"+TypeToStr(mi.ReturnType)+">();");
                  sb.AppendLine("         }");
              }
              else
              {
                  sb.AppendLine("         ///<summary>");
                  sb.AppendLine("         /// Synchronous invoker for  '"+ tc.FullName + "."+mi.Name+"'.");
                  sb.AppendLine("         /// This is a two-way call per contract specification, meaning - the server sends the result back either");
                  sb.AppendLine("         ///  returning no exception or RemoteExceptionData instance.");
                  sb.AppendLine("         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.");
                  sb.AppendLine("         /// RemoteException is thrown if the server generated exception during method execution.");
                  sb.AppendLine("         ///</summary>");
                  sb.AppendLine("         public void @"+mi.Name + mSig(mi, true));
                  sb.AppendLine("         {");
                  sb.AppendLine("            var call = Async_"+mi.Name + mSig(mi, false)+";");
                  sb.AppendLine("            call.CheckVoidValue();");
                  sb.AppendLine("         }");
              }
            }

            sb.AppendLine();
            sb.AppendLine("         ///<summary>");
            sb.AppendLine("         /// Asynchronous invoker for  '"+ tc.FullName + "."+mi.Name+"'.");
            if (oneWay)
            {
              sb.AppendLine("         /// This is a one-way call per contract specification, meaning - the server sends no acknowledgement of this call receipt and");
              sb.AppendLine("         /// there is no result that server could return back to the caller.");
            }
            else
            {
              sb.AppendLine("         /// This is a two-way call per contract specification, meaning - the server sends the result back either");
              sb.AppendLine("         ///  returning no exception or RemoteExceptionData instance.");
            }

            if (oneWay)
              sb.AppendLine("         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg.");
            else
              sb.AppendLine("         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.");

            sb.AppendLine("         ///</summary>");
            sb.AppendLine("         public CallSlot Async_"+mi.Name + mSig(mi, true));
            sb.AppendLine("         {");

            //-------------
            var argmatr = mi.GetCustomAttribute(typeof(ArgsMarshallingAttribute)) as ArgsMarshallingAttribute;
            if (argmatr==null)
            {
              sb.AppendLine("            var request = new RequestAnyMsg(s_ts_CONTRACT, {0}, {1}, RemoteInstance, new object[]{2});".Args(msName(mi), oneWay.ToString().ToLower(), mSig(mi, false, true)));
            }
            else//use arg marsgalling attribute
            {
              sb.AppendLine("            var request = new {0}(s_ts_CONTRACT, {1}, {2}, RemoteInstance)".Args(TypeToStr(argmatr.RequestMsgType),msName(mi), oneWay.ToString().ToLower()));
              sb.AppendLine("            {");
              var pars = mi.GetParameters();
              for(var i=0; i<pars.Length; i++)
              {
                var fName = "MethodArg_{0}_{1}".Args(i, pars[i].Name);
                sb.AppendLine("               {0} = @{1},".Args(fName, pars[i].Name));
              }
              sb.AppendLine("            };");
            }
            //-------------
            sb.AppendLine("            return DispatchCall(request);");
            sb.AppendLine("         }");

            sb.AppendLine();
            sb.AppendLine();
         }
         sb.AppendLine("  #endregion");

         sb.AppendLine();


         sb.AppendLine("  }//class");

         sb.AppendLine("}//namespace");

         return cname+".cs";
       }



               protected virtual string FileHeader()
               {
return
@"
/* Auto generated by Glue Client Compiler tool (gluec)
on "+DateTime.Now.ToString()+" at "+System.Environment.MachineName+" by "+System.Environment.UserName+@"
Do not modify this file by hand if you plan to regenerate this file again by the tool as manual changes will be lost
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NFX.Glue;
using NFX.Glue.Protocol;
";
               }

                   private List<MethodInfo> m_ContractMethods;

               private string msName(MethodInfo mi)
               {
                 var idx = m_ContractMethods.IndexOf( mi );
                 if (idx<0)
                 {
                    m_ContractMethods.Add( mi );
                    idx = m_ContractMethods.Count-1;
                 }

                 return "@s_ms_{0}_{1}".Args( mi.Name, idx);
               }


               private string argsGetMethod(MethodInfo mi)
               {
                 var pars = mi.GetParameters();
                 var sb = new StringBuilder();

                 for(int i=0; i<pars.Length; i++)
                 {
                   var par = pars[i];

                   if (par.IsOut || par.ParameterType.IsByRef || par.ParameterType.IsGenericParameter)
                    throw new NotSupportedException(
                         "Glue does not support calls to methods that have generic, OUT or REF parameters. Contract: '{0}' Method: '{1}' Param: '{2}'"
                         .Args(mi.DeclaringType.FullName, mi.Name, par.Name));


                   if (i>0) sb.Append(", ");
                   sb.Append( "typeof({0})".Args(TypeToStr(par.ParameterType)) );
                 }

                 return "\"{0}\", new Type[]{{ {1} }}".Args(mi.Name, sb.ToString());
               }

               private string mSig(MethodInfo mi, bool isDecl, bool isArray = false)
               {
                 var pars = mi.GetParameters();

                 var sb = new StringBuilder();
                 sb.Append(isArray?"{":"(");

                 for(int i=0; i<pars.Length; i++)
                 {
                   if (i>0) sb.Append(", ");
                   if (isDecl)
                    sb.Append( TypeToStr(pars[i].ParameterType) + "  @" + pars[i].Name);
                   else
                    sb.Append( "@"+pars[i].Name);
                 }

                 sb.Append(isArray?"}":")");

                 return sb.ToString();
               }


               protected string TypeToStr(Type type)
               {
                    return type.FullNameWithExpandedGenericArgs(true);
               }

    }


}
