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
using System.Threading;

using NFX.ApplicationModel;
using NFX.Glue.Protocol;
using NFX.ServiceModel;

namespace NFX.Glue
{
    /// <summary>
    /// Facilititates execution of asynchronous client calls and their corresponding callback functions.
    /// This class introduces callback execution latency and is handy for cases where eventual event flow need to execute regardless of timing.
    /// The latency depends on other callback implementation as the reactor services all calls from a single thread
    /// </summary>
    public sealed class CallReactor
    {
       public const int THREAD_SLEEP_MSEC = 25;
       public const string THREAD_NAME = "Glue Call Reactor";


       public CallReactor(IEnumerable<Call> calls) : this(calls, false, null, null)
       {
       }

       public CallReactor(params Call[] calls) : this(false, null, null, calls)
       {
       }

       public CallReactor(IEnumerable<Call> calls, Action<CallReactor> finishAction, object context) : this(calls, false, finishAction, context)
       {
       }

       public CallReactor(Action<CallReactor> finishAction, object context, params Call[] calls) : this(false, finishAction, context, calls)
       {
       }

       public CallReactor(IEnumerable<Call> calls, bool isBackground) : this(calls, isBackground, null, null)
       {

       }

       public CallReactor(bool isBackground, params Call[] calls) : this(isBackground, null, null, calls)
       {
       }

       public CallReactor(IEnumerable<Call> calls, bool isBackground, Action<CallReactor> finishAction, object context)
         : this(isBackground, finishAction, context, calls.ToArray())
       {

       }

       public CallReactor(bool isBackground, Action<CallReactor> finishAction, object context, params Call[] calls)
       {
             if (calls==null || calls.Length<1)
              throw new InvalidGlueOperationException(StringConsts.ARGUMENT_ERROR + "CallReactor.ctor(null | empty)");


             foreach(var call in calls)
               if (call.m_Reactor!=null)
                throw new InvalidGlueOperationException(StringConsts.GLUE_CALL_SERVICED_BY_DIFFERENT_REACTOR_ERROR);
               else
                call.m_Reactor = this;

             m_Calls = calls;
             m_Context = context;
             m_FinishAction = finishAction;
             m_Thread = new Thread( workBody );
             m_Thread.Name = THREAD_NAME;
             m_Thread.IsBackground = isBackground;
             m_Thread.Start();
       }


       private bool m_Finished;
       private Thread m_Thread;
       private Call[] m_Calls;

       private object m_Context;
       private Action<CallReactor> m_FinishAction;

       private Exception m_Exception;

       /// <summary>
       /// Enumerates all calls that this reactor instance services
       /// </summary>
       public IEnumerable<Call> Calls
       {
         get { return m_Calls; }
       }

       /// <summary>
       /// Gets/sets reactor-wide context which is originally supplied in .ctor or null
       /// </summary>
       public object Context
       {
         get { return m_Context; }
         set { m_Context = value;}
       }

       /// <summary>
       /// Retunrs true when all calls have finished processing
       /// </summary>
       public bool Finished
       {
         get { return m_Finished; }
       }

       /// <summary>
       /// Returns an exception surfaced during reactor operation or null
       /// </summary>
       public Exception Exception
       {
         get { return m_Exception;}
       }

       /// <summary>
       /// Blocks until all calls managed by this reactor instance have ended
       /// </summary>
       public void Wait()
       {
         if (!Finished)
          m_Thread.Join();
       }

       /// <summary>
       /// Waits for all reactors to finish
       /// </summary>
       public static void WaitAll(IEnumerable<CallReactor> reactors)
       {
         foreach(var rtor in reactors) rtor.Wait();
       }


       private void workBody()
       {
         try
         {
             while(!m_Thread.IsBackground || NFX.ApplicationModel.ExecutionContext.Application.Active)
             {
                 bool pending = false;
                 foreach(var call in m_Calls)
                 {
                   if (call.Ended) continue;
                   pending = true;

                   if (call.CallSlot.Available || call.CallSlot.CallStatus!=CallStatus.Dispatched)
                    call.MakeCallbackAndFinish();
                 }
                 if (!pending) break;

                 Thread.Sleep(THREAD_SLEEP_MSEC); //<--- granularity
             }

             if (m_FinishAction!=null)
              m_FinishAction(this);
         }
         catch(Exception error)
         {
            m_Exception = error;
         }

         m_Finished = true;
       }
    }


    /// <summary>
    /// Describes a flow of events originating from a Glue client call
    /// </summary>
    public class Call
    {
       public Call(CallSlot call)
       {
         if (call==null)
          throw new InvalidGlueOperationException(StringConsts.ARGUMENT_ERROR + "Call.ctor(null)");
         m_CallSlot=call;
       }


       public Call(CallSlot call, Action<CallReactor, Call> callback)
       {
         if (call==null || callback==null)
          throw new InvalidGlueOperationException(StringConsts.ARGUMENT_ERROR + "Call.ctor(null)");

         m_CallSlot = call;
         m_Callback = callback;
       }

       public Call(CallSlot call, object context, Action<CallReactor, Call, object> callback)
       {
         if (call==null || callback==null)
          throw new InvalidGlueOperationException(StringConsts.ARGUMENT_ERROR + "Call.ctor(null)");

         m_CallSlot = call;
         m_Context = context;
         m_CallbackInContext = callback;
       }

       internal CallReactor m_Reactor;
       private CallSlot m_CallSlot;
       private object m_Context;
       private Action<CallReactor, Call> m_Callback;
       private Action<CallReactor, Call, object> m_CallbackInContext;
       private bool m_Ended;

       private Exception m_CallbackException;



       /// <summary>
       /// Returns the reactor that services this call
       /// </summary>
       public CallReactor Reactor
       {
         get { return m_Reactor; }
       }


       /// <summary>
       /// Returns CallSlot that represents this call
       /// </summary>
       public CallSlot CallSlot
       {
         get { return m_CallSlot; }
       }

       /// <summary>
       /// Retruns call-dependent context object if one was supplied in call .ctor, or null
       /// </summary>
       public object Context
       {
         get { return m_Context; }
       }

       /// <summary>
       /// Returns true when call has ended with all attached events/callbacks
       /// </summary>
       public bool Ended
       {
         get{ return m_Ended; }
       }

       /// <summary>
       /// Returns exception that was thrown from callback or null if no exception happened
       /// </summary>
       public Exception CallbackException
       {
         get { return m_CallbackException; }
       }


       internal void MakeCallbackAndFinish()
       {
         try
         {
             if (m_CallbackInContext!=null)
                m_CallbackInContext(m_Reactor, this, m_Context);
             else
             {
                if (m_Callback!=null)
                    m_Callback(m_Reactor, this);
             }
         }
         catch(Exception error)
         {
           m_CallbackException = error;
         }

         m_Ended = true;
       }

    }


}
