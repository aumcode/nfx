/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using NFX.Parsing;

namespace NFX.Environment
{
    /// <summary>
    /// Executes configuration scripts which are embedded in configuration
    /// </summary>
    public class ScriptRunner : IConfigurable
    {
        #region CONSTS
            public const string CONFIG_SCRIPT_RUNNER_SECTION = "script-runner";
            public const string CONFIG_SCRIPT_RUNNER_PATH = "/" + CONFIG_SCRIPT_RUNNER_SECTION;

            public const string DEFAULT_KEYWORD_BLOCK   = "_BLOCK";
            public const string DEFAULT_KEYWORD_IF   = "_IF";
            public const string DEFAULT_KEYWORD_ELSE = "_ELSE";
            public const string DEFAULT_KEYWORD_LOOP = "_LOOP";
            public const string DEFAULT_KEYWORD_SET  = "_SET";
            public const string DEFAULT_KEYWORD_CALL  = "_CALL";

            public const string DEFAULT_SCRIPT_ONLY_ATTR  = "script-only";

            public const string CONFIG_TIMEOUT_ATTR  = "timeout-ms";

            public const int DEFAULT_TIMEOUT_MS  = 250;
        #endregion

        #region .ctor
        #endregion

        #region Fields

            [Config("$" + DEFAULT_KEYWORD_BLOCK, DEFAULT_KEYWORD_BLOCK)]
            private string m_KeywordBLOCK;

            [Config("$" + DEFAULT_KEYWORD_IF, DEFAULT_KEYWORD_IF)]
            private string m_KeywordIF;

            [Config("$" + DEFAULT_KEYWORD_ELSE, DEFAULT_KEYWORD_ELSE)]
            private string m_KeywordELSE;

            [Config("$" + DEFAULT_KEYWORD_LOOP, DEFAULT_KEYWORD_LOOP)]
            private string m_KeywordLOOP;

            [Config("$" + DEFAULT_KEYWORD_SET, DEFAULT_KEYWORD_SET)]
            private string m_KeywordSET;

            [Config("$" + DEFAULT_KEYWORD_CALL, DEFAULT_KEYWORD_CALL)]
            private string m_KeywordCALL;

            [Config("$" + DEFAULT_SCRIPT_ONLY_ATTR, DEFAULT_SCRIPT_ONLY_ATTR)]
            private string m_AttributeScriptOnly;

            [Config("$" + CONFIG_TIMEOUT_ATTR, DEFAULT_TIMEOUT_MS)]
            private int m_TimeoutMs = DEFAULT_TIMEOUT_MS;

        #endregion

        #region Properties

            /// <summary>
            /// Gets/sets BLOCK keyword - used for unconditional script evaluation block
            /// </summary>
            public string KeywordBLOCK
            {
                get { return m_KeywordBLOCK.IsNullOrWhiteSpace() ? DEFAULT_KEYWORD_BLOCK : m_KeywordBLOCK; }
                set { m_KeywordBLOCK = value; }
            }


            /// <summary>
            /// Gets/sets IF keyword - used for conditional block
            /// </summary>
            public string KeywordIF
            {
                get { return m_KeywordIF.IsNullOrWhiteSpace() ? DEFAULT_KEYWORD_IF : m_KeywordIF; }
                set { m_KeywordIF = value; }
            }

            /// <summary>
            /// Gets/sets ELSE keyword - used for IF-complementary conditional block
            /// </summary>
            public string KeywordELSE
            {
                get { return m_KeywordELSE.IsNullOrWhiteSpace() ? DEFAULT_KEYWORD_ELSE : m_KeywordELSE; }
                set { m_KeywordELSE = value; }
            }

            /// <summary>
            /// Gets/sets LOOP keyword - used for repetition block
            /// </summary>
            public string KeywordLOOP
            {
                get { return m_KeywordLOOP.IsNullOrWhiteSpace() ? DEFAULT_KEYWORD_LOOP : m_KeywordLOOP; }
                set { m_KeywordLOOP = value; }
            }



            /// <summary>
            /// Gets/sets SET keyword - used for variable assignment statement
            /// </summary>
            public string KeywordSET
            {
                get { return m_KeywordSET.IsNullOrWhiteSpace() ? DEFAULT_KEYWORD_SET : m_KeywordSET; }
                set { m_KeywordSET = value; }
            }

            /// <summary>
            /// Gets/sets CALL keyword - used for sub-routine invocation
            /// </summary>
            public string KeywordCALL
            {
                get { return m_KeywordCALL.IsNullOrWhiteSpace() ? DEFAULT_KEYWORD_CALL : m_KeywordCALL; }
                set { m_KeywordCALL = value; }
            }



            /// <summary>
            /// Gets/sets attribute name that indicates that marked entity should not be brough over into script output target
            /// </summary>
            public string AttributeScriptOnly
            {
                get { return m_AttributeScriptOnly.IsNullOrWhiteSpace() ? DEFAULT_SCRIPT_ONLY_ATTR : m_AttributeScriptOnly; }
                set { m_AttributeScriptOnly = value; }
            }


            /// <summary>
            /// Gets/sets script execution timeout
            /// </summary>
            public int TimeoutMs
            {
                get { return m_TimeoutMs; }
                set { m_TimeoutMs = value>=0 ? value : 0;}
            }

        #endregion

        #region Public

            /// <summary>
            /// Runs script on the configuration
            /// </summary>
            public virtual void Execute(Configuration source, Configuration target)
            {
               try
               {
                    var sw = Stopwatch.StartNew();

                    if (!target.Root.Exists) target.Create();
                    if (target.Root.HasAttributes ||target.Root.HasChildren)
                        throw new ConfigException(StringConsts.CONFIGURATION_SCRIPT_TARGET_CONFIGURATION_MUST_BE_EMPTY_ERROR);

                    target.Root.Name = source.Root.Name;
                    CloneAttributes(source.Root, target.Root);
                    DoNode(sw, source.Root, target.Root);
               }
               catch(Exception error)
               {
                    throw new ConfigException(StringConsts.CONFIGURATION_SCRIPT_EXECUTION_ERROR + error.ToMessageWithType(), error);
               }
            }


            public virtual void Configure(IConfigSectionNode node)
            {
                if (node == null || !node.Exists)
                    node = App.ConfigRoot[CONFIG_SCRIPT_RUNNER_PATH];
                ConfigAttribute.Apply(this, node);
            }

        #endregion

        #region Protected

            protected virtual void DoNode(Stopwatch sw, ConfigSectionNode source, ConfigSectionNode target)
            {
                if (source==null || !source.Exists) return;
                if (target==null || !target.Exists) return;


                if (m_TimeoutMs>0 && sw.ElapsedMilliseconds > m_TimeoutMs)
                    throw new ConfigException(StringConsts.CONFIGURATION_SCRIPT_TIMEOUT_ERROR.Args(m_TimeoutMs, source.RootPath));


                ConfigSectionNode priorStatement = null;
                foreach(var subSource in source.Children)
                {
                    if      (subSource.IsSameName(KeywordBLOCK)) DoBLOCK(sw, subSource, target);
                    else if (subSource.IsSameName(KeywordIF))    DoIF  (sw, subSource, target);
                    else if (subSource.IsSameName(KeywordELSE))  DoELSE(sw, subSource, priorStatement, target);
                    else if (subSource.IsSameName(KeywordLOOP))  DoLOOP(sw, subSource, target);
                    else if (subSource.IsSameName(KeywordSET))   DoSET (sw, subSource);
                    else if (subSource.IsSameName(KeywordCALL))  DoCALL(sw, subSource, target);
                    else
                    {
                        var scriptOnly = false;
                        var scriptOnlyAttr = subSource.AttrByName(AttributeScriptOnly);
                        if (scriptOnlyAttr.Exists)
                         scriptOnly = scriptOnlyAttr.ValueAsBool(false);

                        if (!scriptOnly)
                        {
                            var underStatement = false;
                            var p = subSource;
                            while(p!=null && p.Exists)
                            {
                                if (p.m_Script_Statement)
                                {
                                    underStatement = true;
                                    break;
                                }
                                p = p.Parent;
                            }
                            var newTarget = target.AddChildNode( subSource.EvaluateValueVariables( subSource.Name ), underStatement ? subSource.Value : subSource.VerbatimValue);
                            CloneAttributes(subSource, newTarget, underStatement);

                            DoNode(sw, subSource, newTarget);
                        }

                        priorStatement = null;
                        continue;
                    }
                    priorStatement = subSource;
                }
            }



                            protected virtual void DoBLOCK(Stopwatch sw, ConfigSectionNode blockStatement, ConfigSectionNode target)
                            {
                                InitStatement(blockStatement);
                                DoNode(sw, blockStatement, target);
                            }

                            protected virtual void DoIF(Stopwatch sw, ConfigSectionNode ifStatement, ConfigSectionNode target)
                            {
                                InitStatement(ifStatement);

                                var condition = EvaluateBooleanConditionExpression(ifStatement);
                                if (!condition) return;

                                DoNode(sw, ifStatement, target);
                            }

                            protected virtual void DoELSE(Stopwatch sw, ConfigSectionNode elseStatement, ConfigSectionNode priorStatement, ConfigSectionNode target)
                            {

                                if (priorStatement==null || !priorStatement.IsSameName(DEFAULT_KEYWORD_IF))
                                    throw new ConfigException(StringConsts.CONFIGURATION_SCRIPT_ELSE_NOT_AFTER_IF_ERROR.Args(elseStatement.RootPath));

                                InitStatement(elseStatement);

                                var condition =  priorStatement.m_Script_Bool_Condition_Result;
                                if (condition) return;

                                DoNode(sw, elseStatement, target);
                            }

                            protected virtual void DoLOOP(Stopwatch sw, ConfigSectionNode loopStatement, ConfigSectionNode target)
                            {
                                InitStatement(loopStatement);

                                while(true)
                                {
                                    var condition = EvaluateBooleanConditionExpression(loopStatement);
                                    if (!condition) break;

                                    DoNode(sw, loopStatement, target);
                                }
                            }

                            protected virtual void DoSET(Stopwatch sw, ConfigSectionNode setStatement)
                            {
                                InitStatement(setStatement);

                                var path = setStatement.AttrByName("path").Value ?? StringConsts.NULL_STRING;
                                var to =  setStatement.AttrByName("to").Value;

                                to = EvaluateAnyExpression(setStatement, to);

                                var target = setStatement.Navigate(path);
                                if (!target.Exists)
                                    throw new ConfigException(StringConsts.CONFIGURATION_SCRIPT_SET_PATH_ERROR.Args(setStatement.RootPath, path) );

                                target.Value = to;
                            }

                            protected virtual void DoCALL(Stopwatch sw, ConfigSectionNode callStatement, ConfigSectionNode target)
                            {
                                InitStatement(callStatement);

                                var path = callStatement.Value;
                                var callTarget = callStatement.NavigateSection(path);
                                if (!callTarget.Exists)
                                    throw new ConfigException(StringConsts.CONFIGURATION_SCRIPT_CALL_TARGET_PATH_ERROR.Args(callStatement.RootPath, path) );

                                DoNode(sw, callTarget, target);
                            }


                            protected virtual void InitStatement(ConfigSectionNode statement)
                            {
                                statement.m_Script_Statement = true;
                            }


                            protected virtual bool EvaluateBooleanConditionExpression(ConfigSectionNode exprContainer)
                            {
                                string expression = CoreConsts.UNKNOWN;
                                try
                                {
                                    expression = exprContainer.Value;
                                    var evl = new Evaluator( expression );
                                    var evlResult = evl.Evaluate();

                                    var condition = evlResult=="1" ||
                                                    evlResult.Equals("yes", StringComparison.InvariantCultureIgnoreCase) ||
                                                    evlResult.Equals("true", StringComparison.InvariantCultureIgnoreCase) ||
                                                    evlResult.Equals("t", StringComparison.InvariantCultureIgnoreCase);

                                    exprContainer.m_Script_Bool_Condition_Result = condition;
                                    return condition;
                                }
                                catch(Exception error)
                                {
                                    throw new ConfigException(StringConsts.CONFIGURATION_SCRIPT_EXPRESSION_EVAL_ERROR.Args( expression,
                                                                                                                            exprContainer.RootPath,
                                                                                                                            error.ToMessageWithType()),
                                                                                                                            error);
                                }

                            }

                            protected virtual string EvaluateAnyExpression(ConfigSectionNode exprContainer, string expression)
                            {
                                try
                                {
                                    var evl = new Evaluator( expression );
                                    return evl.Evaluate();
                                }
                                catch(Exception error)
                                {
                                    throw new ConfigException(StringConsts.CONFIGURATION_SCRIPT_EXPRESSION_EVAL_ERROR.Args( expression,
                                                                                                                            exprContainer.RootPath,
                                                                                                                            error.ToMessageWithType()),
                                                                                                                            error);
                                }

                            }

                            protected virtual void CloneAttributes(ConfigSectionNode from, ConfigSectionNode to, bool evaluate = false)
                            {
                                if (evaluate)
                                   foreach(var atr in from.Attributes) to.AddAttributeNode(atr.Name, atr.Value);
                                else
                                   foreach(var atr in from.Attributes) to.AddAttributeNode(atr.Name, atr.VerbatimValue);
                            }

        #endregion


    }
}
