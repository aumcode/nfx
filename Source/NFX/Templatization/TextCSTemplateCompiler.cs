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
using Microsoft.CSharp;
using System.CodeDom.Compiler;

using NFX.ApplicationModel;
using NFX.Environment;

namespace NFX.Templatization
{
    /// <summary>
    /// Compiles templates based of text files that use C# language syntax
    /// </summary>
    /// <example>
     /// <code>
     ///  #&lt;conf&gt;
     ///   &lt;compiler base-class-name="NFX.Web.Templatization.SimpleWebTemplate"
     ///             namespace="TestWebApp.Templates"
     ///             abstract="true"
     ///             summary="Test master page"
     ///    /&gt;
     /// #&lt;/conf&gt;
     /// #[class]
     ///
     ///     public string Title { get {return "aaaaa"; } }
     ///
     ///
     ///     protected abstract void renderHeader();
     ///     protected abstract void renderBody(bool showDetails);
     ///     protected abstract void renderFooter();
     ///
     ///
     /// #[render]
     /// &lt;html&gt;
     ///  &lt;head&gt;
     ///    &lt;title&gt;?[Title]&lt;/title&gt;
     ///  &lt;/head&gt;
     ///  &lt;body&gt;
     ///
     ///   &lt;h1&gt;This is Header&lt;/h1&gt;
     ///    @[renderHeader();]
     ///
     ///   &lt;h1&gt;This is Body&lt;/h1&gt;
     ///    @[renderBody(true);]
     ///   &lt;p&gt;This is in master page&lt;/p&gt;
     ///
     ///   &lt;h1&gt;This is Footer&lt;/h1&gt;
     ///    @[renderFooter();]
     ///
     ///  &lt;/body&gt;
     /// &lt;/html&gt;
     /// </code>
    /// </example>
    public class TextCSTemplateCompiler : TemplateCompiler
    {
       #region CONSTS

         public const string DEFAULT_BASE_TEMPLATE_CLASS_NAME = "NFX.Templatization.Template<object, NFX.Templatization.IRenderingTarget, object>";

         public const string CONFIG_START = "#<conf>";
         public const string CONFIG_END = "#</conf>";

         public const string LACONFIG_START = "#<laconf>";
         public const string LACONFIG_END = "#</laconf>";

         public const string CONFIG_COMPILER_SECTION = "compiler";
         public const string CONFIG_NAMESPACE_ATTR = "namespace";
         public const string CONFIG_ABSTRACT_ATTR = "abstract";
         public const string CONFIG_SUMMARY_ATTR = "summary";
         public const string CONFIG_BASE_RENDER_ATTR = "base-render";
         public const string CONFIG_CLASS_NAME_ATTR = "class-name";
         public const string CONFIG_CLASS_DECLARATION_ATTR = "class-declaration";
         public const string CONFIG_CLASS_CONSTRAINT_ATTR = "class-constraint";
         public const string CONFIG_BASE_CLASS_NAME_ATTR = "base-class-name";
         public const string CONFIG_REF_ASSEMBLY_SECTION = "ref-asm";
         public const string CONFIG_REF_ASSEMBLY_NAME_ATTR = "name";
         public const string CONFIG_USING_SECTION = "using";
         public const string CONFIG_USING_NS_ATTR = "ns";

         public const string CONFIG_ATTRIBUTE_SECTION = "attribute";
         public const string CONFIG_ATTRIBUTE_DECL_ATTR = "decl";


         public const string AREA = "#[";
         public const string CLASS_AREA = "class";
         public const string RENDER_AREA = "render";

         public const string STATEMENT = "@[";
         public const string EXPRESSION = "?[";



         public const string AREA_ESCAPE = "##";
         public const string STATEMENT_ESCAPE = "@@";
         public const string EXPRESSION_ESCAPE = "??";



         public const char SPAN_OPEN = '[';
         public const char SPAN_CLOSE = ']';

         public const string VERBATIM = ":";

         public const string OVERRIDE = "override";
         public const string VIRTUAL = "virtual";

       #endregion

       #region .ctor

           public TextCSTemplateCompiler() : base ()
           {
           }

           public TextCSTemplateCompiler(params ITemplateSource<string>[] sources) : base (sources)
           {
           }

           public TextCSTemplateCompiler(IEnumerable<ITemplateSource<string>> sources) : base (sources)
           {
           }
       #endregion


       #region Fields


       #endregion


       #region Props

           public override string LanguageName
           {
               get { return CoreConsts.CS_LANGUAGE; }
           }

           public override string LanguageSourceFileExtension
           {
               get { return CoreConsts.CS_EXTENSION; }
           }
       #endregion

       #region Protected

           protected override void DoCompileTemplateSource(CompileUnit unit)
           {
             var text = unit.TemplateSource.GetSourceContent().ToString().Trim();
             var icname = unit.TemplateSource.InferClassName();


             Configuration conf = new MemoryConfiguration();

             var confLineCount = 0;
             if (text.StartsWith(CONFIG_START))
             {
               var i = text.IndexOf(CONFIG_END);
               if (i<CONFIG_START.Length) throw new TemplateParseException(StringConsts.TEMPLATE_CS_COMPILER_CONFIG_CLOSE_TAG_ERROR);

               var confText = text.Substring(CONFIG_START.Length, i - CONFIG_START.Length);

               confLineCount = confText.Count(c=>c=='\n');

               //cut configuration out of template
               text = text.Substring(i + CONFIG_END.Length);

               try
               {
                 conf = XMLConfiguration.CreateFromXML("<config>"+confText+"</config>");
               }
               catch(Exception error)
               {
                 throw new TemplateParseException(StringConsts.TEMPLATE_CS_COMPILER_CONFIG_ERROR + error.Message, error);
               }
             }else//20140103 DKh add Laconic support
             if (text.StartsWith(LACONFIG_START))
             {
               var i = text.IndexOf(LACONFIG_END);
               if (i<LACONFIG_START.Length) throw new TemplateParseException(StringConsts.TEMPLATE_CS_COMPILER_CONFIG_CLOSE_TAG_ERROR);

               var confText = text.Substring(LACONFIG_START.Length, i - LACONFIG_START.Length);

               confLineCount = confText.Count(c=>c=='\n');

               //cut configuration out of template
               text = text.Substring(i + LACONFIG_END.Length);

               try
               {
                 conf = LaconicConfiguration.CreateFromString("config{"+confText+"}");
               }
               catch(Exception error)
               {
                 throw new TemplateParseException(StringConsts.TEMPLATE_CS_COMPILER_CONFIG_ERROR + error.Message, error);
               }
             }



             var compilerNode = conf.Root[CONFIG_COMPILER_SECTION];

             //add referenced assemblies
             foreach(var anode in compilerNode.Children.Where(cn=> cn.IsSameName(CONFIG_REF_ASSEMBLY_SECTION)))
               this.ReferenceAssembly(anode.AttrByName(CONFIG_REF_ASSEMBLY_NAME_ATTR).Value);

             //add usings
             var usings = new HashSet<string>();

             RegisterDefaultUsings(usings);

             foreach(var unode in compilerNode.Children.Where(cn=> cn.IsSameName(CONFIG_USING_SECTION)))
               usings.Add(unode.AttrByName(CONFIG_USING_NS_ATTR).Value);

             //add attributes
             var attributes = new List<string>();
              foreach(var anode in compilerNode.Children.Where(cn=> cn.IsSameName(CONFIG_ATTRIBUTE_SECTION)))
               attributes.Add(anode.AttrByName(CONFIG_ATTRIBUTE_DECL_ATTR).Value);

             unit.CompiledSource = new FSM(){Compiler = this,
                                             Unit = unit,
                                             InferredClassName = icname,
                                             ConfigNode = conf.Root,
                                             Source = text,
                                             Usings = usings,
                                             Attributes = attributes,
                                             LineNo = confLineCount+1}.Build().ToString();
           }

           protected override void DoCompileCode()
           {
               CSharpCodeProvider comp = new CSharpCodeProvider();

               CompilerParameters cp = new CompilerParameters();
               foreach(var ass in m_ReferencedAssemblies)
                 cp.ReferencedAssemblies.Add(ass);

               if (!string.IsNullOrWhiteSpace(m_ReferencedAssembliesSearchPath))
                 cp.CompilerOptions = "/lib:\"" + this.m_ReferencedAssembliesSearchPath+"\"";

               cp.GenerateExecutable = false;
               cp.GenerateInMemory = !string.IsNullOrWhiteSpace(m_AssemblyFileName);
               cp.OutputAssembly = m_AssemblyFileName;

               var sources = this.m_Units.Values.Select(u=>u.CompiledSource).ToArray();
               CompilerResults cr = comp.CompileAssemblyFromSource(cp, sources);
               if (cr.Errors.HasErrors)
               {
                 foreach (CompilerError err in cr.Errors)
                 {
                   this.m_CodeCompilerErrors.Add(new TemplateCodeCompilerException(err));
                 }
                return;
              }

              this.m_Assembly = cr.CompiledAssembly;

              foreach(var cu in m_Units.Values)
              {
                cu.CompiledTemplateType = m_Assembly.GetType(cu.CompiledTemplateTypeName);
              }
           }


       #endregion


       #region .pvt .impl

private class FSM
{
     internal TextCSTemplateCompiler Compiler;
     internal IConfigSectionNode ConfigNode;
     internal string Source;
     internal CompileUnit Unit;
     internal int LineNo;
     internal string InferredClassName;
     internal HashSet<string> Usings;
     internal List<string> Attributes;

     StringBuilder Result;

      string this[int idx, int cnt = 1]
      {
        get
        {
           if (idx<0) return string.Empty;
           if (idx>=Source.Length) return string.Empty;

           if (idx+cnt<Source.Length)
            return Source.Substring(idx, cnt);
           else
            return Source.Substring(idx);
        }
      }

      char getch()
      {
           if (Idx<0) return (char)0;
           if (Idx>=Source.Length) return (char)0;
           var c = Source[Idx];

           if (c=='\n') LineNo++;

           return c;
      }

      int Idx;
      string Namespace;
      string ClassName;
      string ClassDeclaration;
      string ClassConstraint;
      string BaseClassName;
      int ClassID;
      bool Abstract;
      bool BaseRender;
      string Summary;
      Dictionary<string, StringBuilder> Areas = new Dictionary<string,StringBuilder>();
      Dictionary<string, string> LiteralBlocks = new Dictionary<string,string>();
      StringBuilder Code = new StringBuilder();
      StringBuilder Literal = new StringBuilder();

      string readSpan()
      {
        var result = new StringBuilder();
        var cnt = 1;
        var isLineComment = false;
        var isBlockComment = false;
        var isString = false;
        var isChar = false;

        var pch = (char)0;

        var startLine = LineNo;

        while(Idx<Source.Length)
        {
            var inContext = !(isLineComment || isBlockComment ||  isString || isChar);

            var ch = getch();
            Idx++;
            if (inContext)
            {
                if (ch==SPAN_CLOSE)
                {
                 cnt--;
                 if (cnt==0)
                  return result.ToString();
                }else
                if (ch==SPAN_OPEN)
                {
                 cnt++;
                }else
                if (ch=='\'')
                {
                  isChar = true;
                }else
                if (ch=='"')
                {
                  isString = true;
                }else
                if (ch=='*' && pch=='/')
                {
                  isBlockComment = true;
                } else
                if (ch=='/' && pch=='/')
                {
                  isLineComment = true;
                }


            }
             else
            {
                if (isChar && ch=='\'' && pch!='\\')
                {
                  isChar = false;
                }else
                if (isString && ch=='"' && pch!='\\' && pch!='"')
                {
                  isString = false;
                }else
                if (isBlockComment && ch=='/' && pch=='*')
                {
                  isBlockComment = false;
                }else
                if (isLineComment && (ch=='\n' || ch=='\r'))
                {
                  isLineComment = false;
                }

            }
            result.Append(ch);

            pch = ch;
        }

        throw new TemplateParseException(string.Format(StringConsts.TEMPLATE_CS_COMPILER_UNMATCHED_SPAN_ERROR, startLine, LineNo));
      }


      void flushLiteral()
      {
        var s = Literal.ToString();
        Literal.Clear();

        if (s.Length<1) return;


        if (Code == Areas[CLASS_AREA])
        {
          Code.Append(s);
          return;
        }

        foreach(var block in LiteralBlocks)
        {
          if (string.CompareOrdinal(s, block.Value)==0)
          {
            Code.AppendFormat("        Target.Write( {0}.{1} );\n", ClassDeclaration, block.Key);
            return;
          }
        }
        var bn = getLiteralBlockName();

        LiteralBlocks[bn] = s;
        Code.AppendFormat("        Target.Write( {0}.{1} );\n", ClassDeclaration, bn);
      }


      string getLiteralBlockName()
      {

        return string.Format("_{0}_S_LITERAL_{1}", ClassID, LiteralBlocks.Count);
      }



              public override string ToString()
              {
                return Result.ToString();
              }

              internal FSM Build()
              {
                if (Result!=null) return this;
                Result = new StringBuilder();

                var cnode = ConfigNode[CONFIG_COMPILER_SECTION];

                ClassName = cnode.AttrByName(CONFIG_CLASS_NAME_ATTR).ValueAsString(InferredClassName);
                if (string.IsNullOrWhiteSpace(ClassName)) ClassName = Compiler.BaseTypeName;
                if (string.IsNullOrWhiteSpace(ClassName)) ClassName = Compiler.GenerateUniqueName();

                ClassDeclaration = cnode.AttrByName(CONFIG_CLASS_DECLARATION_ATTR).ValueAsString(ClassName);
                if (string.IsNullOrWhiteSpace(ClassDeclaration)) ClassDeclaration = ClassName;

                ClassConstraint = cnode.AttrByName(CONFIG_CLASS_CONSTRAINT_ATTR).Value;


                ClassID = Math.Abs(ClassName.GetHashCode() % 100);

                Namespace = cnode.AttrByName(CONFIG_NAMESPACE_ATTR).Value;
                if (string.IsNullOrWhiteSpace(Namespace)) Namespace = Compiler.Namespace;
                if (string.IsNullOrWhiteSpace(Namespace)) Namespace = Compiler.GenerateUniqueName();


                Unit.CompiledTemplateTypeName = "{0}.{1}".Args( Namespace, ClassName);


                BaseClassName = cnode.AttrByName(CONFIG_BASE_CLASS_NAME_ATTR).ValueAsString(Compiler.BaseTypeName);
                if (string.IsNullOrWhiteSpace(BaseClassName)) BaseClassName = DEFAULT_BASE_TEMPLATE_CLASS_NAME;

                Abstract = cnode.AttrByName(CONFIG_ABSTRACT_ATTR).ValueAsBool(false);

                BaseRender = cnode.AttrByName(CONFIG_BASE_RENDER_ATTR).ValueAsBool(true);

                Summary = cnode.AttrByName(CONFIG_SUMMARY_ATTR).ValueAsString("Auto-generated from template");

                Areas.Add(RENDER_AREA, Code);
                Areas.Add(CLASS_AREA, new StringBuilder());


                Idx = 0;
                while(Idx<Source.Length)
                {
                    var s = this[Idx, 2];

                    if (s==AREA_ESCAPE || s==STATEMENT_ESCAPE || s==EXPRESSION_ESCAPE)
                    {
                     Idx++;
                    }else
                    if (s==STATEMENT)
                    {
                      flushLiteral();
                      Idx+=STATEMENT.Length;
                      var statement = readSpan();
                      Code.Append("      ");
                      Code.AppendLine(statement);
                    }else
                    if (s==EXPRESSION)
                    {
                      flushLiteral();
                      Idx+=EXPRESSION.Length;
                      var expression = readSpan();

                      if (expression.EndsWith(";"))
                       expression = expression.Remove(expression.Length-1);

                      if (expression.Length==0)
                       throw new TemplateParseException(string.Format(StringConsts.TEMPLATE_CS_COMPILER_EMPTY_EXPRESSION_ERROR, LineNo));

                      if (expression.StartsWith(VERBATIM))
                      {
                        expression = expression.Substring(1);
                        Code.AppendFormat("        Target.Write( {0} );\n", expression);
                      }
                      else
                        Code.AppendFormat("        Target.Write(Target.Encode( {0} ));\n", expression);
                    }else
                    if (s==AREA)
                    {
                      flushLiteral();
                      Idx+=AREA.Length;
                      var area = readSpan();
                      if (!Areas.ContainsKey(area))
                      {
                        Code = new StringBuilder();
                        Areas.Add(area, Code);
                      }
                       else
                      {
                        Code = Areas[area];
                      }
                    }

                    if (Idx<Source.Length)//20141010 DKH fixed tail span [] bug
                    {
                     Literal.Append(getch());
                     Idx++;
                    }
                }//while

                flushLiteral();


                Result.AppendFormat("//WARNING: This code was auto generated by template compiler, do not modify by hand!\n");
                Result.AppendFormat("//Generated on {0} by {1} at {2}\n", App.LocalizedTime, Compiler.GetType().FullName, System.Environment.MachineName);
                Result.AppendLine();
                foreach(var us in Usings)
                  Result.AppendFormat("using {0}; \n", us);
                Result.AppendLine();
                Result.AppendFormat("namespace {0} \n", Namespace);
                Result.AppendLine  ("{");
                Result.AppendLine();
                Result.AppendLine  (" ///<summary>");
                Result.AppendLine  (" /// "+Summary);
                Result.AppendLine  (" ///</summary>");
                if (Attributes.Count>0)
                  foreach(var attr in Attributes)
                    Result.AppendFormat(" [{0}] \n", attr);
                Result.AppendFormat(" public {0} class {1} : {2} {3}\n", Abstract ? "abstract":string.Empty, ClassDeclaration, BaseClassName, ClassConstraint);
                Result.AppendLine  (" {");
                Result.AppendLine(Areas[CLASS_AREA].ToString());

                Result.AppendLine("     protected override void DoRender()");
                Result.AppendLine("     {");

                if (BaseRender)
                 Result.AppendLine("       base.DoRender();");

                if (Areas.ContainsKey(RENDER_AREA))
                 Result.AppendLine(Areas[RENDER_AREA].ToString());

                Result.AppendLine("     }");

                foreach(var ark in Areas.Keys.Where(k=> k!=CLASS_AREA && k!=RENDER_AREA))
                {
                    var over = ark.StartsWith(OVERRIDE);
                    if (over)
                      Result.AppendLine("    protected override void " + ark.Remove(0, OVERRIDE.Length));
                    else
                    {
                      var virt = ark.StartsWith(VIRTUAL);
                      if (virt)
                        Result.AppendLine("    protected virtual void " + ark.Remove(0, VIRTUAL.Length));
                      else
                        Result.AppendLine("    protected void " + ark);
                    }
                    Result.AppendLine("    {");
                    Result.AppendLine(Areas[ark].ToString());
                    Result.AppendLine("    }");
                }

                Result.AppendLine();
                Result.AppendLine();
                Result.AppendLine("     #region Literal blocks content");
                foreach(var block in LiteralBlocks)
                {
                  Result.AppendFormat("        private const string {0} = @\"{1}\"; \n", block.Key, block.Value.Replace("\"","\"\""));
                }
                Result.AppendLine("     #endregion");
                Result.AppendLine();
                Result.AppendLine(" }//class");

                Result.AppendLine("}//namespace");

                return this;
              }
}
       #endregion

    }
}
