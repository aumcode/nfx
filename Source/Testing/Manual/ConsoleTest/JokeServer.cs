/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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

using NFX.Log;
using NFX.ApplicationModel;

using BusinessLogic;
using NFX.Instrumentation;
using NFX;

namespace ConsoleTest
{
    public class NotifyEvent : Event, IOperationClass
    {
        protected NotifyEvent() : base("JokeServer.Notify") {}
        
        public static void Happened()
        {
          var inst = ExecutionContext.Application.Instrumentation;
          if (inst.Enabled)
           inst.Record(new NotifyEvent()); 
        }


        protected override Datum MakeAggregateInstance()
        {
            return new NotifyEvent(); 
        }
    }

    public class JokeServer : IJokeContract
    {
        public string Echo(string text)
        {
          
          var sb = new StringBuilder();
          TextInfoHeader th = null;
          foreach(var h in NFX.Glue.ServerCallContext.Request.Headers)
          {
            if (h is TextInfoHeader) th = (TextInfoHeader)h;
            sb.Append(h.ToString());
            sb.Append(" ");
          }
         
         
          //ExecutionContext.Application.Log.Write(MessageType.Info, "MarazmServer.Echo. Headers: " + sb.ToString(), from: text + (th==null?string.Empty : th.Text + th.Info));
          
          
          //NFX.Glue.ServerCallContext.ResponseHeaders.Add( new MyHeader());
          return "Server echoed '" + text + "' on "+App.LocalizedTime.ToString(); 
        }

        public string UnscecureEcho(string text)
        {
          return "Server echoed '" + text;
        }

        public string UnsecEchoMar(string text)
        {
          return "Server echoed '" + text;
        }


        public void Notify(string text)
        {
            NotifyEvent.Happened();
            //ExecutionContext.Application.Log.Write(MessageType.Info, from: "MarazmServer.Notify", text: text);  
        }

        public object ObjectWork(object dummy)
        {
            if (dummy is string)
                dummy = dummy.ToString() + " server work done on " + App.LocalizedTime.ToString();
            return dummy;
        }


        public string UnsecureEcho(string text)
        {
          throw new NotImplementedException();
        }

        public object DBWork(string id, int recCount, int waitMs)
        {
          return null;
        }





        public string SimpleWorkAny(string s, int i1, int i2, bool b, double d)
        {
          return s + i1.ToString() + i2.ToString() + b.ToString() + d.ToString();
        }

        public string SimpleWorkMar(string s, int i1, int i2, bool b, double d)
        {
          return s + i1.ToString() + i2.ToString() + b.ToString() + d.ToString();
        }
    }

    [NFX.Glue.ThreadSafe]
    public class JokeCalculatorServer : IJokeCalculator
    {
        private int m_Value; //state is retained between calls


        public void Init(int value)
        {
           m_Value = value;
        }

        public int Add(int value)
        {
          m_Value += value;
          return m_Value;
        }

        public int Sub(int value)
        {
          m_Value -= value;
          return m_Value;
        }

        public int Done()
        {
          return m_Value;
        }
    }


}
