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

using NFX.Log;
using System.IO;

namespace NFX
{
    /// <summary>
    /// Specifies how to handle Assertion and other failures
    /// </summary>
    [Flags]
    public enum DebugAction
    {
        /// <summary>Default to the value of Debugging.DefaultDebugAction</summary>
        Default = 0,
        /// <summary>Throw exception</summary>
        Throw,
        /// <summary>Write message to log</summary>
        Log,
        /// <summary>Shortcut for throwing exception and writing a message to log</summary>
        ThrowAndLog = Throw | Log
    }

    /// <summary>
    /// Facilitates debugging tasks enabled by DEBUG conditional define
    /// </summary>
    public static class Debug
    {
        public const string DEBUG = "DEBUG";

        [Conditional(DEBUG)]
        public static void Assert(
                                      bool condition,
                                      string text = null,
                                      DebugAction action = DebugAction.Default,
                                      string from = null,
                                      MessageType type = MessageType.Debug,
                                      int source = 0,
                                      string topic = null,
                                      string pars = null,
                                      Guid? correlationContext = null,
                                      int frameOffset = 2
                                 )
        {
            Debugging.Assert(condition, text, action, from, type, source, topic ?? CoreConsts.ASSERT_TOPIC, pars, correlationContext, frameOffset);
        }

        [Conditional(DEBUG)]
        public static void Fail(
                                      string text = null,
                                      DebugAction action = DebugAction.Default,
                                      string from = null,
                                      MessageType type = MessageType.Debug,
                                      int source = 0,
                                      string topic = null,
                                      string pars = null,
                                      Guid? correlationContext = null,
                                      int frameOffset = 2
                                 )
        {
            Debugging.Fail(text, action, from, type, source, topic ?? CoreConsts.ASSERT_TOPIC, pars, correlationContext, frameOffset);
        }

        [Conditional(DEBUG)]
        public static void Write(
                                      string text,
                                      string from = null,
                                      MessageType type = MessageType.Debug,
                                      int source = 0,
                                      string topic = null,
                                      string pars = null,
                                      Guid? correlationContext = null,
                                      int frameOffset = 2
                                 )
        {
            Debugging.Write(text, from, type, source, topic, pars, correlationContext, frameOffset);
        }

        /// <summary>
        /// A simplified method for tracing that doesn't evaluate text generation function
        /// if tracing is disabled by configuration
        /// </summary>
        /// <param name="textFunc">Functor to evaluate to get the text sent to logger</param>
        /// <param name="type">Message type to log</param>
        /// <param name="correlationContext">Optional correlation token to relate log entries</param>
        [Conditional(DEBUG)]
        public static void Write(
                                      Func<string> textFunc,
                                      MessageType type = MessageType.Trace,
                                      Guid? correlationContext = null
                                )
        {
            if (Debugging.TraceDisabled)
                return;

            Debugging.Write(textFunc(), null, MessageType.Trace, 0, CoreConsts.TRACE_TOPIC, null, correlationContext, 2);
        }
    }


    /// <summary>
    /// Facilitates debugging tasks enabled by TRACE conditional define
    /// </summary>
    public static class Trace
    {
        public const string TRACE = "TRACE";

        [Conditional(TRACE)]
        public static void Assert(
                                      bool condition,
                                      string text = null,
                                      DebugAction action = DebugAction.Default,
                                      string from = null,
                                      MessageType type = MessageType.Trace,
                                      int source = 0,
                                      string topic = null,
                                      string pars = null,
                                      Guid? correlationContext = null,
                                      int frameOffset = 2
                                 )
        {
            Debugging.Assert(condition, text, action, from, type, source, topic ?? CoreConsts.ASSERT_TOPIC, pars, correlationContext, frameOffset);
        }

        [Conditional(TRACE)]
        public static void Fail(
                                      string text = null,
                                      DebugAction action = DebugAction.Default,
                                      string from = null,
                                      MessageType type = MessageType.Debug,
                                      int source = 0,
                                      string topic = null,
                                      string pars = null,
                                      Guid? correlationContext = null,
                                      int frameOffset = 2
                                 )
        {
            Debugging.Fail(text, action, from, type, source, topic ?? CoreConsts.ASSERT_TOPIC, pars, correlationContext, frameOffset);
        }

        [Conditional(TRACE)]
        public static void Write(
                                      string text,
                                      string from = null,
                                      MessageType type = MessageType.Trace,
                                      int source = 0,
                                      string topic = null,
                                      string pars = null,
                                      Guid? correlationContext = null,
                                      int frameOffset = 2
                                 )
        {
            Debugging.Write(text, from, type, source, topic ?? CoreConsts.TRACE_TOPIC, pars, correlationContext, frameOffset);
        }

        /// <summary>
        /// A simplified method for tracing that doesn't evaluate text generation function
        /// if tracing is disabled by configuration
        /// </summary>
        /// <param name="textFunc">Functor to evaluate to get the text sent to logger</param>
        /// <param name="type">Message type to log</param>
        /// <param name="correlationContext">Optional correlation token to relate log entries</param>
        [Conditional(TRACE)]
        public static void Write(
                                      Func<string> textFunc,
                                      MessageType type = MessageType.Trace,
                                      Guid? correlationContext = null
                                )
        {
            if (Debugging.TraceDisabled)
                return;

            Debugging.Write(textFunc(), null, MessageType.Trace, 0, CoreConsts.TRACE_TOPIC, null, correlationContext, 2);
        }

        /// <summary>
        /// A simplified method for tracing that doesn't evaluate text generation function
        /// if tracing is disabled by configuration. It takes context argument that can be passed to the
        /// text-generating textFunc functor
        /// </summary>
        /// <param name="textFunc">Functor to evaluate to get the text sent to logger</param>
        /// <param name="ctx">Context object</param>
        /// <param name="type">Message type to log</param>
        /// <param name="correlationContext">Optional correlation token to relate log entries</param>
        [Conditional(TRACE)]
        public static void Write<TContext>(
                                      Func<TContext,string> textFunc,
                                      TContext ctx,
                                      MessageType type = MessageType.Trace,
                                      Guid? correlationContext = null
                                )
        {
            if (Debugging.TraceDisabled)
                return;

            Debugging.Write(textFunc(ctx), null, MessageType.Trace, 0, CoreConsts.TRACE_TOPIC, null, correlationContext, 2);
        }
    }


    /// <summary>
    /// Facilitates debugging tasks that do not depend on any conditional defines
    /// </summary>
    public static class Debugging
    {
        #region CONSTS

            public const string CONFIG_DEFAULT_DEBUG_ACTION_ATTR = "debug-default-action";
            public const string CONFIG_DEBUG_CONF_REFRESH_ATTR   = "debug-conf-refresh";
            public const string CONFIG_TRACE_DISABLE_ATTR        = "trace-disable";

        #endregion

        private static DebugAction s_DefaultDebugAction = DebugAction.ThrowAndLog;
        private static bool        s_TraceDisabled      = false;
        private static bool?       s_ConfRefresh        = false;

        /// <summary>
        /// Returns the global default setting, and optionally
        /// reads default debug action from global application's configuration
        /// if its value is DefaultFromConfig (default). Application can override
        /// this value at startup in order to avoid dynamic configuration lookups
        /// on every call
        /// </summary>
        public static DebugAction DefaultDebugAction
        {
            get
            {
                if (!s_ConfRefresh.HasValue)
                    s_ConfRefresh = App.ConfigRoot.AttrByName(CONFIG_DEBUG_CONF_REFRESH_ATTR).ValueAsBool(false);

                DebugAction action = (s_ConfRefresh.Value) ? ReadDefaultDebugActionFromConfig() : s_DefaultDebugAction;

                return (action == DebugAction.Default) ? DebugAction.ThrowAndLog : action;
            }
            set
            {
                s_DefaultDebugAction = (s_DefaultDebugAction == DebugAction.Default) ? DebugAction.ThrowAndLog : value;
            }
        }

        /// <summary>
        /// Controls wether to send Trace.Write() and Debug.Write() output to LogService
        /// </summary>
        public static bool TraceDisabled
        {
            get
            {
                if (!s_ConfRefresh.HasValue)
                    s_ConfRefresh = App.ConfigRoot.AttrByName(CONFIG_DEBUG_CONF_REFRESH_ATTR).ValueAsBool(false);

                return s_ConfRefresh.Value ? ReadTraceDisableFromConfig() : s_TraceDisabled;
            }
            set
            {
                s_TraceDisabled = value;
            }
        }

        public static void Assert(
                                    bool condition,
                                    string text = null,
                                    DebugAction action = DebugAction.Default,
                                    string from = null,
                                    MessageType type = MessageType.Debug,
                                    int source = 0,
                                    string topic = null,
                                    string pars = null,
                                    Guid? correlationContext = null,
                                    int frameOffset = 1
                                 )
        {
            if (condition) return;

            if (string.IsNullOrWhiteSpace(from))
            {
                var frame = new StackFrame(frameOffset, true);
                var m = frame.GetMethod();
                from = string.Format("{0}.{1} at [{2}:{3}]",
                                  m.DeclaringType.FullName, m.Name,
                                  Path.GetFileName(frame.GetFileName()), frame.GetFileLineNumber());
            }

            if (string.IsNullOrWhiteSpace(text))
                text = StringConsts.ASSERTION_ERROR; // Could be either Debug or Trace

            Exception exception = null;

            if (action == DebugAction.Default)
                action = DefaultDebugAction;

            if ((action & DebugAction.Throw) == DebugAction.Throw)
                exception = new DebugAssertionException(text + ":  " + from, from);

            if ((action & DebugAction.Log) == DebugAction.Log)
                NFX.ApplicationModel.ExecutionContext.Application.Log.Write(
                    new Message{
                        Type = type,
                        Topic = topic ?? CoreConsts.DEBUG_TOPIC,
                        Source = source,
                        From = from,
                        Text = text,
                        Parameters = pars,
                        Exception = exception,
                        RelatedTo = correlationContext.HasValue ? correlationContext.Value : Guid.Empty
                    });

            if (exception != null) throw exception;
        }

        public static void Fail(
                                    string text = null,
                                    DebugAction action = DebugAction.Default,
                                    string from = null,
                                    MessageType type = MessageType.Debug,
                                    int source = 0,
                                    string topic = null,
                                    string pars = null,
                                    Guid? correlationContext = null,
                                    int frameOffset = 1
                                 )
        {
            Assert(false, text, action, from, type, source, topic, pars, correlationContext, frameOffset);
        }

        public static void Write(
                                    string text,
                                    string from = null,
                                    MessageType type = MessageType.Debug,
                                    int source = 0,
                                    string topic = null,
                                    string pars = null,
                                    Guid? correlationContext = null,
                                    int frameOffset = 1
                                )
        {
            if (TraceDisabled)
                return;

            if (string.IsNullOrWhiteSpace(from))
            {
                StackFrame frame;
                try
                {
                    frame = new StackFrame(frameOffset, true);
                }
                catch
                {
                    try { frame = new StackFrame(1, true); }
                    catch
                    {
                        frame = null;
                    }
                }

                if (frame != null)
                {
                    var m = frame.GetMethod();
                    from = string.Format("{0}.{1}[{2}:{3}]",
                                      m.DeclaringType.FullName, m.Name,
                                      Path.GetFileName(frame.GetFileName()), frame.GetFileLineNumber());
                }
            }

            NFX.ApplicationModel.ExecutionContext.Application.Log.Write(
                new Message{
                    Type = type,
                    Topic = topic ?? CoreConsts.DEBUG_TOPIC,
                    Source = source,
                    From = from,
                    Text = text,
                    Parameters = pars,
                    RelatedTo = correlationContext.HasValue ? correlationContext.Value : Guid.Empty
                });
        }

        internal static DebugAction ReadDefaultDebugActionFromConfig()
        {
            return App.ConfigRoot
                        .AttrByName(CONFIG_DEFAULT_DEBUG_ACTION_ATTR)
                        .ValueAsEnum<DebugAction>(DebugAction.ThrowAndLog);
        }

        internal static bool ReadTraceDisableFromConfig()
        {
            return App.ConfigRoot.AttrByName(CONFIG_TRACE_DISABLE_ATTR).ValueAsBool(false);
        }
    }
}
