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

using NFX.Environment;

namespace NFX.RelationalModel
{
    /// <summary>
    /// Specifies a type of target that compiler produces output for
    /// </summary>
    public enum TargetType
    {
        Other = 0,
        GenericSQL,
        Oracle,
        MsSQLServer,
        DB2,
        PostgreSQL,
        MySQL,
        Interbase,
        Firebird,
        Mnesia,
        MongoDB,
        JSON,
        JavaScript,
        RecordModel,
        ErlScript
    }



    /// <summary>
    /// Represents a compiler that can turn source schema into particular target script/schema, i.e. into database creation script for particular technology
    ///  (i.e. Oracle, MySQL, PostgreSQL, etc..) or some other code/script (i.e. RecordModel classes, JavaScript classes)
    /// </summary>
    public abstract class Compiler : IConfigurable, INamed
    {
                       #region inner classes
                            public sealed class Outputs : IEnumerable<KeyValuePair<string, StringBuilder>>
                            {
                                private Dictionary<string, StringBuilder> m_Dict = new Dictionary<string,StringBuilder>();

                                private StringBuilder m_Current;

                                public StringBuilder this[string key]
                                {
                                    get
                                    {
                                        StringBuilder result = null;
                                        if (!m_Dict.TryGetValue(key, out result))
                                        {
                                            result = new StringBuilder();
                                            m_Dict[key] = result;
                                        }
                                        m_Current = result;
                                        return result;
                                    }
                                }

                                /// <summary>
                                /// Returns last output used or Unspecified output
                                /// </summary>
                                public StringBuilder CurrentOrUnspecified { get{ return m_Current ?? this["Unspecified"];}}


                                public IEnumerator<KeyValuePair<string,StringBuilder>> GetEnumerator()
                                {
 	                                return m_Dict.GetEnumerator();
                                }

                                System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                                {
 	                                return m_Dict.GetEnumerator();
                                }
                            }

                        #endregion


        #region CONSTS

            /// <summary>
            /// Specifies the name of the file to be included in the output verbatim, i.e.:  script-include="mytypes.txt"{}
            /// </summary>
            public const string SCRIPT_INCLUDE_SECTION = "script-include";

            /// <summary>
            /// Specifies the comment that will be output for decorated entity in the script
            /// </summary>
            public const string SCRIPT_COMMENT_ATTR = "script-comment";

            /// <summary>
            /// Specifies the text to be included in the output verbatim, i.e.:  script-text="INSERT INTO TBL_ABC VALUES(1, true, now())"{}
            /// </summary>
            public const string SCRIPT_TEXT_SECTION = "script-text";

            /// <summary>
            /// Specifies the name of the output that script text has to be placed in
            /// </summary>
            public const string SCRIPT_OUTPUT_NAME_ATTR = "output-name";

        #endregion


        #region .ctor
            protected Compiler(Schema schema)
            {
                m_Schema = schema;
            }
        #endregion

        #region Fields
            private Schema m_Schema;
            private bool m_HasCompiled;
            private Exception m_CompileException;
            protected List<SchemaCompilationException> m_CompileErrors = new List<SchemaCompilationException>();


            private bool m_CaseSensitiveNames;
            private string m_OutputPath;

            private string m_OutputPrefix;

        #endregion


        #region Properties

            /// <summary>
            /// Returns the name of the technology that this compiler targets. i.e. "ORACLE", "RecordModel"
            /// </summary>
            public abstract string Name { get; }


            /// <summary>
            /// Returns type of target that this instance produces output for
            /// </summary>
            public abstract TargetType Target { get; }


            /// <summary>
            /// Returns source schema
            /// </summary>
            public Schema Schema { get { return m_Schema;} }

            /// <summary>
            /// Returns true to indicate that compiler has already processed the source schema
            /// </summary>
            public bool HasCompiled { get{ return m_HasCompiled;}}

            /// <summary>
            /// Returns exception that surfaced during compilation, or null if source has not compiled yet or no exception happened.
            /// This exception is different form CompileErrors because it indicates some un=planned condition that broke the compilation process
            /// </summary>
            public Exception CompileException { get{ return m_CompileException;} }

            /// <summary>
            /// Returns exception errors that were generated during compilation. This property returns "planned" errors that were caused by input,
            ///  whereas CompileException returns exception that indicates some more drastic abnormality that broke the compilation
            /// </summary>
            public IEnumerable<SchemaCompilationException> CompileErrors { get{ return m_CompileErrors;} }


            /// <summary>
            /// Returns true when this instance did not compile properly
            /// </summary>
            public bool HasErrors { get { return m_CompileException!=null || m_CompileErrors.Count>0; }}


            /// <summary>
            /// Determines whether output script is case sensitive
            /// </summary>
            [Config("$case-sensitive-names|$case-sensitive")]
            public virtual bool CaseSensitiveNames
            {
                get{ return m_CaseSensitiveNames; }
                set
                {
                    EnsureNotCompiled();
                    m_CaseSensitiveNames = value;
                }

            }

            /// <summary>
            /// Returns string comparison options for names that depend on target case sensitivity
            /// </summary>
            public StringComparison NameComparison
            {
                get { return CaseSensitiveNames ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase; }
            }

            /// <summary>
            /// Determines where compiled output is placed
            /// </summary>
            [Config("$out-path")]
            public string OutputPath
            {
                get{ return m_OutputPath ?? string.Empty; }
                set
                {
                    EnsureNotCompiled();
                    m_OutputPath = value;
                }

            }

            /// <summary>
            /// Determines the prefix for output names
            /// </summary>
            [Config("$out-name-prefix")]
            public string OutputPrefix
            {
                get{ return m_OutputPrefix ?? string.Empty; }
                set
                {
                    EnsureNotCompiled();
                    m_OutputPrefix = value;
                }

            }

        #endregion

        #region Public

            public void Configure(IConfigSectionNode node)
            {
                EnsureNotCompiled();
                ConfigAttribute.Apply(this, node);
            }


            public void Compile()
            {
                if (m_HasCompiled) return;
                try
                {
                    DoCompile();
                }
                catch(Exception error)
                {
                    m_CompileException = error;
                }
                finally
                {
                    m_HasCompiled = true;
                }
            }
        #endregion

        #region Protected

            protected void EnsureNotCompiled()
            {
                if (m_HasCompiled)
                 throw new CompilerException(StringConsts.SCHEMA_HAS_ALREADY_COMPILED_OPTION_CHANGE_ERROR.Args(GetType().FullName));
            }


            /// <summary>
            /// Override to provide meaningful extension for particular technology, i.e. SQL
            /// </summary>
            public virtual string GetOutputFileSuffix(string outputName)
            {
                return ".{0}.out".Args(Name);
            }


            /// <summary>
            /// Performs a compilation - this is a root  override-able method where compilation starts
            /// </summary>
            protected virtual void DoCompile()
            {
                if (!Directory.Exists(OutputPath))
                    throw new CompilerException(StringConsts.RELATIONAL_COMPILER_OUTPUT_PATH_DOSNT_EXIST_ERROR + OutputPath);

                var outputs = new Outputs();
                BuildOutputs(outputs);

                foreach(var pair in outputs)
                    File.WriteAllText( Path.Combine(OutputPath, OutputPrefix + EscapeFileName(pair.Key) + GetOutputFileSuffix(pair.Key)), pair.Value.ToString());
            }


            /// <summary>
            /// Override to perform compilation into output, the base implementation loops over all nodes and interprets
            ///  script includes
            /// </summary>
            protected virtual void BuildOutputs(Outputs outputs)
            {
                foreach(var node in Schema.Source.Children)
                {
                    if      (node.IsSameName(SCRIPT_INCLUDE_SECTION)) IncludeScriptFile(node, outputs);
                    else if (node.IsSameName(SCRIPT_TEXT_SECTION)) IncludeScriptText(node, outputs);
                    else
                         BuildNodeOutput(node, outputs);
                }
            }

            /// <summary>
            /// Override to perform custom interpretation per particular compiler target
            /// </summary>
            protected abstract void BuildNodeOutput(IConfigSectionNode node, Outputs outputs);


            /// <summary>
            /// Performs a script file include
            /// </summary>
            protected virtual void IncludeScriptFile(IConfigSectionNode node, Outputs outputs)
            {
                var fn = node.Value;
                if (fn!=null)
                {
                    if (!File.Exists(fn))
                        foreach(var path in Schema.IncludePaths)
                        {
                            fn = Path.Combine(path, node.Value);
                            if (File.Exists(fn)) break;
                        }
                }
                if (fn==null || !File.Exists(fn))
                    throw new SchemaCompilationException(node.RootPath,
                                                         StringConsts.RELATIONAL_COMPILER_INCLUDE_SCRIPT_NOT_FOUND_ERROR
                                                                     .Args(node.ValueAsString(StringConsts.NULL_STRING), node.RootPath));

                var content = File.ReadAllText(fn);

                var outputName = node.AttrByName(SCRIPT_OUTPUT_NAME_ATTR).Value;

                var output = outputName.IsNullOrWhiteSpace() ? outputs.CurrentOrUnspecified : outputs[outputName];

                output.Append(content);
            }

            /// <summary>
            /// Performs a verbatim script text include
            /// </summary>
            public virtual void IncludeScriptText(IConfigSectionNode node, Outputs outputs)
            {
                var text = node.Value;

                var outputName = node.AttrByName(SCRIPT_OUTPUT_NAME_ATTR).Value;

                var output = outputName.IsNullOrWhiteSpace() ? outputs.CurrentOrUnspecified : outputs[outputName];

                output.Append(text);
            }



            /// <summary>
            /// Replaces incompatible characters for file names with "_"
            /// </summary>
            public string EscapeFileName(string fn)
            {
                var result = new StringBuilder();

                foreach(var c in fn)
                {
                    if ( char.IsLetterOrDigit(c) || c=='-' || c=='_' || c=='.') result.Append(c);
                    else
                      result.Append('_');
                }

                return result.ToString();
            }

            public virtual string EscapeString(string str)
            {
                str = str.Replace("'", "\\'");
                return "'{0}'".Args(str);
            }

        #endregion

    }
}
