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
    /// Represents an instance of relational schema source
    /// </summary>
    public class Schema
    {
        #region CONSTS

            public const string CONFIG_SCHEMA_SECTION = "relational-schema";

            public const string CONFIG_INCLUDE_SECTION = "include";

        #endregion

        #region .ctor

            public Schema(string fileName,  IEnumerable<string> includePaths = null, bool runScript = true, ScriptRunner runner = null)
                          :this(Configuration.ProviderLoadFromFile(fileName), includePaths, runScript, runner)
            {

            }

            public Schema(Configuration source,  IEnumerable<string> includePaths = null, bool runScript = true, ScriptRunner runner = null)
            {
                m_SourceOriginal = source;
                m_IncludePaths = includePaths ?? Enumerable.Empty<string>();

                processIncludes(source.Root, new HashSet<string>());

                if (runScript)
                {
                    m_SourceScriptRunner = runner ?? new ScriptRunner();

                    m_Source = new MemoryConfiguration();

                    m_SourceScriptRunner.Execute(m_SourceOriginal, m_Source);
                }
                else
                    m_Source = m_SourceOriginal;


            }


        #endregion

        #region Fields

            private Configuration m_SourceOriginal;
            private Configuration m_Source;

            private ScriptRunner m_SourceScriptRunner;

            private IEnumerable<string> m_IncludePaths;

        #endregion

        #region Properties




            /// <summary>
            /// Returns script runner used for schema source evaluation. Null returned when no scripts were executed
            /// </summary>
            public ScriptRunner SourceScriptRunner { get { return m_SourceScriptRunner;} }


            /// <summary>
            /// Returns the source root tree before script was run but all includes processed
            /// </summary>
            public IConfigSectionNode SourceOriginal { get { return m_SourceOriginal.Root;} }

            /// <summary>
            /// Returns the source root tree after includes were processed and script was run
            /// </summary>
            public IConfigSectionNode Source { get { return m_Source.Root;} }

            /// <summary>
            /// Returns include paths that are searched for included files
            /// </summary>
            public IEnumerable<string> IncludePaths { get { return m_IncludePaths;} }

        #endregion

        #region Public

        #endregion

        #region .pvt

            private void processIncludes(ConfigSectionNode node, HashSet<string> alreadyIncluded)
            {
                foreach(var includeNode in node.Children.Where(cn=>cn.IsSameName(CONFIG_INCLUDE_SECTION)))
                {
                    var include = includeNode.Value;

                    if (!File.Exists(include))
                    {
                        foreach(var path in m_IncludePaths)
                        {
                            var fn = Path.Combine(path, include);
                            if (File.Exists(fn))
                            {
                                include = fn;
                                break;
                            }
                        }
                    }

                    if (!File.Exists(include))
                     throw new SchemaException(StringConsts.SCHEMA_INCLUDE_FILE_DOSNT_EXIST_ERROR + include);


                    if (alreadyIncluded.Contains(include))
                     throw new SchemaException(StringConsts.SCHEMA_INCLUDE_FILE_REFERENCED_MORE_THAN_ONCE_ERROR + include);

                    alreadyIncluded.Add( include );

                    var included = Configuration.ProviderLoadFromFile( include );
                    processIncludes(included.Root, alreadyIncluded);

                    includeNode.Configuration.Include(includeNode, included.Root);
                }
            }

        #endregion
    }
}
