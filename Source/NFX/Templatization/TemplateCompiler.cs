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
using System.Reflection;

using NFX.Environment;

namespace NFX.Templatization
{
    /// <summary>
    /// Represents a compilation unit which is compiled by TemplateCompiler.
    /// Use TemplateCompiler.IncludeTemplateSource() method or pass sources to .ctor
    /// </summary>
    public class CompileUnit
    {
      internal CompileUnit(ITemplateSource source)
      {
        m_TemplateSource = source;
      }

      private ITemplateSource m_TemplateSource;
      private string m_CompiledSource;
      private Type m_CompiledType;
      private string m_CompiledTypeName;
      private Exception m_CompilationException;

      /// <summary>
      /// References original template source such as a string for text-based templates or an image
      /// </summary>
      public ITemplateSource TemplateSource { get { return m_TemplateSource;} }

      /// <summary>
      /// Returns source code in a language that particular TemplateCompiler derivative supports.
      /// Use TemplateCompiler.LanguageName to determine language polymorphicaly
      /// </summary>
      public string CompiledSource { get { return m_CompiledSource ?? string.Empty;} internal set { m_CompiledSource = value; }}


      /// <summary>
      /// Returns CLR type that compiler produced IF language source compilation was performed
      /// </summary>
      public Type CompiledTemplateType{ get { return m_CompiledType;} internal set { m_CompiledType = value; }}

      /// <summary>
      /// Returns fully-qualified CLR type name that compiler produced
      /// </summary>
      public string CompiledTemplateTypeName{ get { return m_CompiledTypeName;} internal set { m_CompiledTypeName = value; }}

      /// <summary>
      /// Returns an exception that was thrown during compilation
      /// </summary>
      public Exception CompilationException{ get { return m_CompilationException;} internal set { m_CompilationException = value; }}
    }



    /// <summary>
    /// Represents abstraction of template compilers. This class is not thread-safe
    /// </summary>
    public abstract class TemplateCompiler : DisposableObject, IConfigurable, IEnumerable<CompileUnit>
    {
       #region CONSTS


       #endregion

       #region .ctor

           protected TemplateCompiler()
           {
              m_ReferencedAssemblies.Add("System.dll");
              m_ReferencedAssemblies.Add("System.Core.dll");
              m_ReferencedAssemblies.Add("System.Data.dll");
              m_ReferencedAssemblies.Add("System.Xml.dll");
              m_ReferencedAssemblies.Add("System.Xml.Linq.dll");
              m_ReferencedAssemblies.Add("NFX.dll");
           }

           protected TemplateCompiler(params ITemplateSource[] sources) : this()
           {
             if (sources!=null)
              foreach(var src in sources)
                 IncludeTemplateSource(src);
           }

           protected TemplateCompiler(IEnumerable<ITemplateSource> sources) : this()
           {
             if (sources!=null)
              foreach(var src in sources)
                 IncludeTemplateSource(src);
           }

           protected override void Destructor()
           {
               base.Destructor();
           }
       #endregion

       #region Private/Prot Fields
            private bool m_Compiled;
            private bool m_CompileCode;

            private string m_BaseTypeName;
            private string m_Namespace;


            protected IConfigSectionNode m_Options;



            protected Assembly m_Assembly;
            protected string m_AssemblyFileName;

            protected CompileUnits m_Units = new CompileUnits();
            protected List<string> m_ReferencedAssemblies = new List<string>();

            protected string m_ReferencedAssembliesSearchPath;

            protected List<Exception> m_CodeCompilerErrors = new List<Exception>();
       #endregion


       #region Properties


           /// <summary>
           /// Gets the name of the language that this compiler supports, i.e. "C#"
           /// </summary>
           public abstract string LanguageName
           {
             get;
           }

           /// <summary>
           /// Gets the name of the language source code file extension that this compiler supports, i.e. ".cs"
           /// </summary>
           public abstract string LanguageSourceFileExtension
           {
             get;
           }


           /// <summary>
           /// Indicates whether this instance was compiled
           /// </summary>
           public bool Compiled
           {
             get { return m_Compiled;}
           }


           /// <summary>
           /// Config options section used for compilation
           /// </summary>
           [Config("options")]
           public IConfigSectionNode Options { get { return m_Options; } set {m_Options=value;}}


           /// <summary>
           /// Returns compiled assembly or null if only source have been compiled when CompileCode=false;
           /// </summary>
           public Assembly Assembly { get { return m_Assembly; } }


           /// <summary>
           /// Returns referenced assemblies
           /// </summary>
           public IEnumerable<string> ReferencedAssemblies
           {
             get { return m_ReferencedAssemblies;}
           }


           /// <summary>
           /// Gets/sets path for referenced assemblies that do not have a path in their names
           /// </summary>
           [Config("$ref-path")]
           public string ReferencedAssembliesSearchPath
           {
              get { return m_ReferencedAssembliesSearchPath; }

              set
              {
                 EnsureNotCompiled();
                 m_ReferencedAssembliesSearchPath = value;
              }
           }


           /// <summary>
           /// Gets/sets filename for assembly, if null then assembly is created in-memory only
           /// </summary>
           [Config("$asm-file")]
           public string AssemblyFileName
           {
              get { return m_AssemblyFileName; }

              set
              {
                 EnsureNotCompiled();
                 m_AssemblyFileName = value;
              }
           }

           /// <summary>
           /// Indicates whether language compilation should be performed and Assembly be built
           /// </summary>
           [Config("$compile-code")]
           public bool CompileCode
           {
             get { return m_CompileCode; }

             set
             {
               EnsureNotCompiled();
               m_CompileCode = value;
             }
           }


           /// <summary>
           /// Sets the name of base type that templates inherit from.
           /// This type must directly or indirectly inherit from Template
           /// </summary>
           [Config("$base-type")]
           public string BaseTypeName
           {
             get { return m_BaseTypeName ?? string.Empty; }

             set
             {
               EnsureNotCompiled();
               m_BaseTypeName = value;
             }
           }

           /// <summary>
           /// Sets the name of namespace that classes compiled into
           /// </summary>
           [Config("$namespace")]
           public string Namespace
           {
             get { return m_Namespace ?? string.Empty; }

             set
             {
               EnsureNotCompiled();
               m_Namespace = value;
             }
           }



           /// <summary>
           /// Returns a compile unit by template source or throws if it does not exist
           /// </summary>
           public CompileUnit this[ITemplateSource templateSource]
           {
             get
             {
               CompileUnit unit  = null;
               if (!m_Units.TryGetValue(templateSource, out unit))
                throw new TemplateCompilerException(StringConsts.ARGUMENT_ERROR + "TemplateCompiler.[templateSource] not found");
               return unit;
             }
           }

           /// <summary>
           /// Indicates whether some units have template compilation errors or code compiler errors exist
           /// </summary>
           public bool HasErrors
           {
             get { return m_CodeCompilerErrors.Count>0 ||  m_Units.Values.Any(u=>u.CompilationException!=null); }
           }


           /// <summary>
           /// Returns code compilation errors
           /// </summary>
           public IEnumerable<Exception> CodeCompilerErrors
           {
             get { return m_CodeCompilerErrors; }
           }

            /// <summary>
           /// Returns compile units that have errors
           /// </summary>
           public IEnumerable<CompileUnit> CompileUnitsWithErrors
           {
             get { return m_Units.Values.Where(u=>u.CompilationException!=null); }
           }


       #endregion

       #region Public

         /// <summary>
         /// Throws an exception if this instance has already been compiled
         /// </summary>
         public void EnsureNotCompiled()
         {
            if (m_Compiled)
             throw new TemplateCompilerException(StringConsts.TEMPLATE_COMPILER_ALREADY_COMPILED_ERROR + GetType().FullName);
         }

         /// <summary>
         /// Performs compilation if it has not already been performed
         /// </summary>
         public void Compile()
         {
           if (m_Compiled) return;

             foreach(var cu in m_Units.Values)
               try
               {
                 DoCompileTemplateSource(cu);
               }
               catch(Exception error)
               {
                 cu.CompilationException = error;
               }


             if (m_CompileCode)
              try
              {
               DoCompileCode();
              }
              catch(Exception error)
              {
                m_CodeCompilerErrors.Add(error);
              }


           m_Compiled = true;
         }

         public void Configure(IConfigSectionNode node)
         {
             EnsureNotCompiled();
             ConfigAttribute.Apply(this, node);
             DoConfigure(node);
         }


         /// <summary>
         /// Includes template source into compilation
         /// </summary>
         public void IncludeTemplateSource(ITemplateSource source)
         {
           if (source==null)
             throw new TemplateCompilerException(StringConsts.ARGUMENT_ERROR + "IncludeTemplateSource(null)");
           EnsureNotCompiled();
           m_Units.Add(source, new CompileUnit(source));
         }

         /// <summary>
         /// Exclude template source from compilation
         /// </summary>
         public bool ExcludeTemplateSource(ITemplateSource source)
         {
           if (source==null)
             throw new TemplateCompilerException(StringConsts.ARGUMENT_ERROR + "ExcludeTemplateSource(null)");
           EnsureNotCompiled();
           return m_Units.Remove(source);
         }

         /// <summary>
         /// References assembly by its name if it is already not referenced
         /// </summary>
         public void ReferenceAssembly(string aname)
         {
           if (aname==null)
             throw new TemplateCompilerException(StringConsts.ARGUMENT_ERROR + "ReferenceAssembly(null)");
           EnsureNotCompiled();

           aname = aname.Trim();
           if (!m_ReferencedAssemblies.Contains(aname))
            m_ReferencedAssemblies.Add(aname);
         }


         /// <summary>
         /// Removes assembly reference by its name if it was already referenced
         /// </summary>
         public bool UnReferenceAssembly(string aname)
         {
           if (aname==null)
             throw new TemplateCompilerException(StringConsts.ARGUMENT_ERROR + "UnReferenceAssembly(null)");
           EnsureNotCompiled();

           aname = aname.Trim();
           return m_ReferencedAssemblies.Remove(aname);
         }


              private Random m_Rnd = new Random();
              private List<string> m_UniqueNames = new List<string>();

         /// <summary>
         /// Generates unique identifier suitable for use in code per compiler instance
         /// </summary>
         public string GenerateUniqueName()
         {
           while(true)
           {
             var name = string.Format("id_{0}_template_{1}", m_Rnd.Next(100000), Math.Abs(this.GetType().FullName.GetHashCode() % 1000));
             if (!m_UniqueNames.Contains(name))
             {
               m_UniqueNames.Add(name);
               return name;
             }
           }
         }





          public IEnumerator<CompileUnit> GetEnumerator()
          {
             return m_Units.Values.GetEnumerator();
          }

          System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
          {
             return m_Units.Values.GetEnumerator();
          }

       #endregion


       #region Protected




         /// <summary>
         /// Override to perform code generation from template source into code
         /// </summary>
         protected abstract void DoCompileTemplateSource(CompileUnit unit);

         /// <summary>
         /// Override to perform compilation of code into assembly
         /// </summary>
         protected abstract void DoCompileCode();


         /// <summary>
         /// Override to perform custom configuration
         /// </summary>
         protected virtual void DoConfigure(IConfigSectionNode node)
         {

         }

         /// <summary>
         /// Override to provide default using clauses
         /// </summary>
         protected virtual void RegisterDefaultUsings(HashSet<string> usings)
         {
             usings.Add("System");
             usings.Add("System.Text");
             usings.Add("System.Linq");
             usings.Add("System.Collections.Generic");
         }

       #endregion



    }



    /// <summary>
    /// Represents a bag of compile units
    /// </summary>
    public class CompileUnits : Dictionary<ITemplateSource, CompileUnit> {}

}
