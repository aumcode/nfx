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
using System.IO;
using System.Linq;
using System.Text;
using NFX.Log;

namespace NFX
{
    /// <summary>
    /// Helper class to be used in the context of 'using' clause to
    /// facilitate cleanup on scope exit and performing of other functions
    /// such as status logging
    /// </summary>
    /// <remarks>
    /// Typical use case:
    /// <code>
    ///   using(Scope.OnExit(() => Bell.Ring()))
    ///   {
    ///      ... do something ...
    ///   }
    ///
    ///   using(Scope.OnExit&lt;bool>(
    ///                 ()  => { var old = Tracing.IsOn; Tracing.Off(); return old; },
    ///                 (b) => Tracing.State(b)))
    ///   {
    ///      ... do something ...
    ///   }
    /// </code>
    /// </remarks>
    public class Scope : DisposableObject
    {
        #region Local Internal Classes
            public struct _scopeExit : IDisposable
            {
                public _scopeExit(Action onExit)
                {
                    this.onExit  = onExit;
                    this.m_Disposed = false;
                }

                private readonly Action onExit;
                private bool m_Disposed;

                public void Dispose()
                {
                    if (m_Disposed) return;
                    if (onExit != null) onExit();
                    m_Disposed = true;
                }
            }

            public struct _scopeExit<TCtx1> : IDisposable
            {
                public _scopeExit(TCtx1 ctx1, Action<TCtx1> onExit)
                {
                    this.ctx1 = ctx1;
                    this.onExit = onExit;
                    this.m_Disposed = false;
                }

                private readonly TCtx1          ctx1;
                private readonly Action<TCtx1>  onExit;
                private bool                    m_Disposed;

                public void Dispose()
                {
                    if (m_Disposed) return;
                    if (onExit != null) onExit(ctx1);
                    m_Disposed = true;
                }
            }

            public struct _scopeExit<TCtx1, TCtx2> : IDisposable
            {
                public _scopeExit(TCtx1 ctx1, TCtx2 ctx2, Action<TCtx1, TCtx2> onExit)
                {
                    this.ctx = new Tuple<TCtx1, TCtx2>(ctx1, ctx2);
                    this.onExit = onExit;
                    this.m_Disposed = false;
                }

                private readonly Tuple <TCtx1,TCtx2> ctx;
                private readonly Action<TCtx1,TCtx2> onExit;
                private bool m_Disposed;

                public void Dispose()
                {
                    if (m_Disposed) return;
                    if (onExit != null) onExit(ctx.Item1, ctx.Item2);
                    m_Disposed = true;
                }
            }

            public struct _scopeExit<TCtx1, TCtx2, TCtx3> : IDisposable
            {
                public _scopeExit(TCtx1 ctx1, TCtx2 ctx2, TCtx3 ctx3, Action<TCtx1, TCtx2, TCtx3> onExit)
                {
                    this.ctx = new Tuple<TCtx1, TCtx2, TCtx3>(ctx1, ctx2, ctx3);
                    this.onExit = onExit;
                    this.m_Disposed = false;
                }

                private readonly Tuple <TCtx1,TCtx2,TCtx3> ctx;
                private readonly Action<TCtx1,TCtx2,TCtx3> onExit;
                private bool m_Disposed;

                public void Dispose()
                {
                    if (m_Disposed) return;
                    if (onExit != null) onExit(ctx.Item1, ctx.Item2, ctx.Item3);
                    m_Disposed = true;
                }
            }

            public struct _scopeExit<TCtx1, TCtx2, TCtx3, TCtx4> : IDisposable
            {
                public _scopeExit(TCtx1 c1, TCtx2 c2, TCtx3 c3, TCtx4 c4, Action<TCtx1, TCtx2, TCtx3, TCtx4> onExit)
                {
                    this.ctx = new Tuple<TCtx1, TCtx2, TCtx3, TCtx4>(c1, c2, c3, c4);
                    this.onExit = onExit;
                    this.m_Disposed = false;
                }

                private readonly Tuple <TCtx1,TCtx2,TCtx3,TCtx4> ctx;
                private readonly Action<TCtx1,TCtx2,TCtx3,TCtx4> onExit;
                private bool m_Disposed;

                public void Dispose()
                {
                    if (m_Disposed) return;
                    if (onExit != null) onExit(ctx.Item1, ctx.Item2, ctx.Item3, ctx.Item4);
                    m_Disposed = true;
                }
            }

        #endregion

        #region .ctor

            public Scope(
                string          name = null,
                bool            log = false,
                string          logText = null,
                string          logTopic = null,
                MessageType?    logMsgType = null,
                Action          onExit = null,
                bool            measureTime = false,
                int             stackFrameOffset = 2)
            {
                m_LogText   = logText;
                m_LogTopic  = logTopic;
                m_LogMsgType= logMsgType ?? MessageType.Info;
                m_Log       = log || logText != null || logTopic != null || logMsgType != null;
                m_Name      = name ?? (m_Log ? getName(stackFrameOffset) : string.Empty);
                m_OnExit   += onExit;
                if (measureTime)
                    m_Timer = Stopwatch.StartNew();
            }

        #endregion

        #region Fields

            private string      m_Name;
            private bool        m_Log;
            private string      m_LogText;
            private MessageType m_LogMsgType;
            private string      m_LogTopic;
            private Stopwatch   m_Timer;
            private Action      m_OnExit;

        #endregion

        #region Properties

        #endregion

        #region Public

            public static _scopeExit OnExit(Action onExit)
            {
                return new _scopeExit(onExit);
            }

            public static _scopeExit<T1> OnExit<T1>(T1 context, Action<T1> onExit)
            {
                return new _scopeExit<T1>(context, onExit);
            }

            public static _scopeExit<T1,T2> OnExit<T1,T2>(T1 ctx1, T2 ctx2, Action<T1,T2> onExit)
            {
                return new _scopeExit<T1,T2>(ctx1, ctx2, onExit);
            }

            public static _scopeExit<T1,T2,T3> OnExit<T1,T2,T3>(T1 ctx1, T2 ctx2, T3 ctx3, Action<T1,T2,T3> onExit)
            {
                return new _scopeExit<T1,T2,T3>(ctx1, ctx2, ctx3, onExit);
            }

            public static _scopeExit<T1,T2,T3,T4> OnExit<T1,T2,T3,T4>(T1 c1, T2 c2, T3 c3, T4 c4, Action<T1,T2,T3,T4> onExit)
            {
                return new _scopeExit<T1,T2,T3,T4>(c1, c2, c3, c4, onExit);
            }

            public Scope AtExit             (Action onExit)                                            { m_OnExit += onExit;                     return this; }
            public Scope AtExit<T1>         (T1 c1,                     Action<T1>          onExit)    { m_OnExit += () => onExit(c1);           return this; }
            public Scope AtExit<T1,T2>      (T1 c1,T2 c2,               Action<T1,T2>       onExit)    { m_OnExit += () => onExit(c1,c2);        return this; }
            public Scope AtExit<T1,T2,T3>   (T1 c1,T2 c2,T3 c3,         Action<T1,T2,T3>    onExit)    { m_OnExit += () => onExit(c1,c2,c3);     return this; }
            public Scope AtExit<T1,T2,T3,T4>(T1 c1,T2 c2,T3 c3,T4 c4,   Action<T1,T2,T3,T4> onExit)    { m_OnExit += () => onExit(c1,c2,c3,c4);  return this; }

        #endregion

        #region Protected .pvt

            protected override void Destructor()
            {
                if (m_Timer != null)
                    m_Timer.Stop();

                try
                {
                    if (m_Log)
                        App.Log.Write(
                            new Message
                            {
                                Text = m_LogText,
                                Type = m_LogMsgType,
                                From = m_Name,
                                Topic = m_LogTopic,
                                Parameters = m_Timer == null ? string.Empty : "time={0}ms".Args(m_Timer.ElapsedMilliseconds)
                            });
                }
                finally
                {
                    if (m_OnExit != null) m_OnExit();
                }
            }

            private string getName(int stackFrameOffset)
            {
                var frame = new StackFrame(stackFrameOffset, true);
                var m = frame.GetMethod();
                return "{0}.{1}[{2}:{3}]".Args(
                    m.DeclaringType.FullName, m.Name,
                    Path.GetFileName(frame.GetFileName()), frame.GetFileLineNumber());
            }

        #endregion
    }

}
