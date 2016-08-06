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
            public const string CONFIG_SCRIPT_RUNNER_PATH = "/" + CONFIG_SCRIPT_RUNNER_SECTION + "/";

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

            [Config(CONFIG_SCRIPT_RUNNER_PATH + "$" + DEFAULT_KEYWORD_BLOCK, DEFAULT_KEYWORD_BLOCK)]
            private string m_KeywordBLOCK;

            [Config(CONFIG_SCRIPT_RUNNER_PATH + "$" + DEFAULT_KEYWORD_IF, DEFAULT_KEYWORD_IF)]
            private string m_KeywordIF;

            [Config(CONFIG_SCRIPT_RUNNER_PATH + "$" + DEFAULT_KEYWORD_ELSE, DEFAULT_KEYWORD_ELSE)]
            private string m_KeywordELSE;

            [Config(CONFIG_SCRIPT_RUNNER_PATH + "$" + DEFAULT_KEYWORD_LOOP, DEFAULT_KEYWORD_LOOP)]
            private string m_KeywordLOOP;

            [Config(CONFIG_SCRIPT_RUNNER_PATH + "$" + DEFAULT_KEYWORD_SET, DEFAULT_KEYWORD_SET)]
            private string m_KeywordSET;

            [Config(CONFIG_SCRIPT_RUNNER_PATH + "$" + DEFAULT_KEYWORD_CALL, DEFAULT_KEYWORD_CALL)]
            private string m_KeywordCALL;

            [Config(CONFIG_SCRIPT_RUNNER_PATH + "$" + DEFAULT_SCRIPT_ONLY_ATTR, DEFAULT_SCRIPT_ONLY_ATTR)]
            private string m_AttributeScriptOnly;

            [Config(CONFIG_SCRIPT_RUNNER_PATH + "$" + CONFIG_TIMEOUT_ATTR, DEFAULT_TIMEOUT_MS)]
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
            public void Execute(Configuration source, Configuration target)
            {
               try
               {
                    var sw = Stopwatch.StartNew();

                    if (!target.Root.Exists) target.Create();
                    if (target.Root.HasAttributes ||target.Root.HasChildren)
                        throw new ConfigException(StringConsts.CONFIGURATION_SCRIPT_TARGET_CONFIGURATION_MUST_BE_EMPTY_ERROR);

                    target.Root.Name = source.Root.Name;
                    cloneAttributes(source.Root, target.Root);
                    doNode(sw, source.Root, target.Root);
               }
               catch(Exception error)
               {
                    throw new ConfigException(StringConsts.CONFIGURATION_SCRIPT_EXECUTION_ERROR + error.ToMessageWithType(), error);
               }
            }


            public void Configure(IConfigSectionNode node)
            {
                ConfigAttribute.Apply(this, node);
            }

        #endregion

        #region .pvt

            private void doNode(Stopwatch sw, ConfigSectionNode source, ConfigSectionNode target)
            {
                if (source==null || !source.Exists) return;
                if (target==null || !target.Exists) return;


                if (m_TimeoutMs>0 && sw.ElapsedMilliseconds > m_TimeoutMs)
                    throw new ConfigException(StringConsts.CONFIGURATION_SCRIPT_TIMEOUT_ERROR.Args(m_TimeoutMs, source.RootPath));


                ConfigSectionNode priorStatement = null;
                foreach(var subSource in source.Children)
                {
                    if      (subSource.IsSameName(KeywordBLOCK)) doBLOCK(sw, subSource, target);
                    else if (subSource.IsSameName(KeywordIF))    doIF  (sw, subSource, target);
                    else if (subSource.IsSameName(KeywordELSE))  doELSE(sw, subSource, priorStatement, target);
                    else if (subSource.IsSameName(KeywordLOOP))  doLOOP(sw, subSource, target);
                    else if (subSource.IsSameName(KeywordSET))   doSET (sw, subSource);
                    else if (subSource.IsSameName(KeywordCALL))  doCALL(sw, subSource, target);
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
                            cloneAttributes(subSource, newTarget, underStatement);

                            doNode(sw, subSource, newTarget);
                        }

                        priorStatement = null;
                        continue;
                    }
                    priorStatement = subSource;
                }
            }



                            private void doBLOCK(Stopwatch sw, ConfigSectionNode blockStatement, ConfigSectionNode target)
                            {
                                initStatement(blockStatement);
                                doNode(sw, blockStatement, target);
                            }

                            private void doIF(Stopwatch sw, ConfigSectionNode ifStatement, ConfigSectionNode target)
                            {
                                initStatement(ifStatement);

                                var condition = evaluateBooleanConditionExpression(ifStatement);
                                if (!condition) return;

                                doNode(sw, ifStatement, target);
                            }

                            private void doELSE(Stopwatch sw, ConfigSectionNode elseStatement, ConfigSectionNode priorStatement, ConfigSectionNode target)
                            {

                                if (priorStatement==null || !priorStatement.IsSameName(DEFAULT_KEYWORD_IF))
                                    throw new ConfigException(StringConsts.CONFIGURATION_SCRIPT_ELSE_NOT_AFTER_IF_ERROR.Args(elseStatement.RootPath));

                                initStatement(elseStatement);

                                var condition =  priorStatement.m_Script_Bool_Condition_Result;
                                if (condition) return;

                                doNode(sw, elseStatement, target);
                            }

                            private void doLOOP(Stopwatch sw, ConfigSectionNode loopStatement, ConfigSectionNode target)
                            {
                                initStatement(loopStatement);

                                while(true)
                                {
                                    var condition = evaluateBooleanConditionExpression(loopStatement);
                                    if (!condition) break;

                                    doNode(sw, loopStatement, target);
                                }
                            }

                            private void doSET(Stopwatch sw, ConfigSectionNode setStatement)
                            {
                                initStatement(setStatement);

                                var path = setStatement.AttrByName("path").Value ?? StringConsts.NULL_STRING;
                                var to =  setStatement.AttrByName("to").Value;

                                to = evaluateAnyExpression(setStatement, to);

                                var target = setStatement.Navigate(path);
                                if (!target.Exists)
                                    throw new ConfigException(StringConsts.CONFIGURATION_SCRIPT_SET_PATH_ERROR.Args(setStatement.RootPath, path) );

                                target.Value = to;
                            }

                            private void doCALL(Stopwatch sw, ConfigSectionNode callStatement, ConfigSectionNode target)
                            {
                                initStatement(callStatement);

                                var path = callStatement.Value;
                                var callTarget = callStatement.NavigateSection(path);
                                if (!callTarget.Exists)
                                    throw new ConfigException(StringConsts.CONFIGURATION_SCRIPT_CALL_TARGET_PATH_ERROR.Args(callStatement.RootPath, path) );

                                doNode(sw, callTarget, target);
                            }


                            private void initStatement(ConfigSectionNode statement)
                            {
                                statement.m_Script_Statement = true;
                            }


                            private bool evaluateBooleanConditionExpression(ConfigSectionNode exprContainer)
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

                            private string evaluateAnyExpression(ConfigSectionNode exprContainer, string expression)
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

                            private void cloneAttributes(ConfigSectionNode from, ConfigSectionNode to, bool evaluate = false)
                            {
                                if (evaluate)
                                   foreach(var atr in from.Attributes) to.AddAttributeNode(atr.Name, atr.Value);
                                else
                                   foreach(var atr in from.Attributes) to.AddAttributeNode(atr.Name, atr.VerbatimValue);
                            }

        #endregion


    }
}
