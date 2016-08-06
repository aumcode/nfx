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

using NFX.ApplicationModel;

namespace NFX.Instrumentation
{
    /// <summary>
    /// Represents an exception event recorded by instrumentation
    /// </summary>
    [Serializable]
    public class ExceptionEvent : Event, IErrorInstrument
    {

        /// <summary>
        /// Create event from exception instance
        /// </summary>
        public static void Record(Exception error)
        {
          var inst = ExecutionContext.Application.Instrumentation;
          if (inst.Enabled)
            inst.Record( new ExceptionEvent(error) );
        }

        /// <summary>
        /// Create event from exception instance and source
        /// </summary>
        public static void Record(string source, Exception error)
        {
          var inst = ExecutionContext.Application.Instrumentation;
          if (inst.Enabled)
            inst.Record(  new ExceptionEvent(source, error) );
        }

        /// <summary>
        /// Create event from exception instance as of utcTime
        /// </summary>
        public static void Record(string source, Exception error, DateTime utcTime)
        {
          var inst = ExecutionContext.Application.Instrumentation;
          if (inst.Enabled)
            inst.Record( new ExceptionEvent(source, error, utcTime) );
        }

        private ExceptionEvent()
        {

        }

        protected ExceptionEvent(Exception error) : base()
        {
          m_ExceptionType = error.GetType().FullName;
        }

        protected ExceptionEvent(string source, Exception error) : base(source)
        {
          m_ExceptionType = error.GetType().FullName;
        }

        protected ExceptionEvent(string source, Exception error, DateTime utcTime) : base(source, utcTime)
        {
          m_ExceptionType = error.GetType().FullName;
        }

        private string m_ExceptionType;


        public string ExceptionType
        {
          get { return m_ExceptionType; }
        }


         [NonSerialized]
         private Dictionary<string, int> m_Errors;

        protected override Datum MakeAggregateInstance()
        {
            return new ExceptionEvent(){ m_Errors = new Dictionary<string, int>() };
        }

        protected override void AggregateEvent(Datum evt)
        {
            var eevt = evt as ExceptionEvent;
            if (eevt==null) return;

            if (m_Errors.ContainsKey(eevt.m_ExceptionType))
              m_Errors[eevt.m_ExceptionType] += 1;
            else
              m_Errors.Add(eevt.m_ExceptionType, 1);
        }

        protected override void SummarizeAggregation()
        {
          var sb = new StringBuilder();

          foreach( var s in m_Errors.OrderBy( p => -p.Value).Take(10).Select(p => p.Key))
          {
           sb.Append(s);
           sb.Append(", ");
          }

          m_ExceptionType = sb.ToString();

        }


        public override string ToString()
        {
            return base.ToString() + " " + m_ExceptionType ?? string.Empty;
        }

    }

}
