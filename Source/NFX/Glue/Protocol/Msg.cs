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

namespace NFX.Glue.Protocol
{

    /// <summary>
    /// Represents a message base that requests and responses derive from.
    /// Messages are exchanged between glued tiers
    /// </summary>
    [Serializable]
    public abstract class Msg
    {
        /// <summary>
        /// Constructs new message and allocates unique ID
        /// </summary>
        protected Msg()
        {
          m_ID = FID.Generate();
        }


        internal FID m_ID;
        internal Headers m_Headers;


        [NonSerialized]
        private object m_BindingSpecificContext;

        [NonSerialized]
        private Dictionary<string, object> m_CorrelationData;

        [NonSerialized]
        private long m_ArrivalTimeStamp;



        /// <summary>
        /// Returns a unique ID for this message
        /// </summary>
        public FID ID
        {
          get { return m_ID; }
        }

        /// <summary>
        /// Returns arrival timestamp expressed in ticks, that is - when message just arrived before its processing started by Glue (before deserialization).
        /// This property allows for glue-internal latency assessment
        /// </summary>
        public long ArrivalTimeStamp
        {
          get { return m_ArrivalTimeStamp;}
        }

        /// <summary>
        /// Framework method not for developers. Sets arrival ticks stamp. Usually called from transport
        /// </summary>
        public void __SetArrivalTimeStampTicks(long value)
        {
           m_ArrivalTimeStamp = value;
        }


        /// <summary>
        /// Returns request ID that relates to this message instance. It is the message ID for RequestMsg and related message ID for ResponseMsg
        /// </summary>
        public abstract FID RequestID { get; }


        /// <summary>
        /// Returns true when this message contains header data
        /// </summary>
        public bool HasHeaders
        {
          get { return m_Headers!=null && m_Headers.Count > 0;}
        }


        /// <summary>
        /// Returns a list of headers that this message contains
        /// </summary>
        public Headers Headers
        {
           get
           {
             if (m_Headers==null) m_Headers = new Headers();
             return m_Headers;
           }
           set
           {
             m_Headers = value;
           }
        }


        /// <summary>
        /// Returns true when this message contains header data
        /// </summary>
        public bool HasCorrelationData
        {
          get { return m_CorrelationData!=null && m_CorrelationData.Count > 0;}
        }

        /// <summary>
        /// Arbitrary named object collection that can be used to correlate messages and/or attach extra data.
        /// This field is NOT transmitted over wire nor is it used by the framework. Message inspectors may use this property
        /// at their own discretion, for example in cases when the same inspector type is registered on multiple levels (endpoint, binding, glue),
        /// inspectors may check this property to execute only once per message.
        /// The collection is lazily allocated at the first reference attempt
        /// </summary>
        public Dictionary<string, object> CorrelationData
        {
          get
          {
            if (m_CorrelationData==null)
             m_CorrelationData = new Dictionary<string,object>();

            return m_CorrelationData;
          }
        }

        /// <summary>
        /// Returns a binding-specific context object which is used for message processing/routing by binding.
        /// This property value is not serialized and used internally by Glue framework. Not all bindings use/need this property
        /// </summary>
        public object BindingSpecificContext
        {
            get { return m_BindingSpecificContext; }
        }

        /// <summary>
        /// Internal method not to be called by app developers.
        /// Called from custom bindings to inject binding-specific context
        /// </summary>
        public void __SetBindingSpecificContext(object context)
        {
          m_BindingSpecificContext = context;
        }

        /// <summary>
        /// Internal method not to be called by app developers.
        /// Called from custom bindings to inject binding-specific context
        /// </summary>
        public void __SetBindingSpecificContext(Msg other)
        {
          m_BindingSpecificContext = other == null ? null : other.m_BindingSpecificContext;
        }


        /// <summary>
        /// Clones message state from another message instance.
        /// State includes binding-specific context and optionally headers and correlation data
        /// </summary>
        public void CloneState(Msg from, bool cloneHeaders = true, bool cloneCorrelation = true)
        {
              if (cloneHeaders)
                  if (from.HasHeaders)
                    this.Headers.AddRange(from.Headers);

              if (cloneCorrelation)
                  if (from.HasCorrelationData)
                    this.m_CorrelationData = new Dictionary<string,object>( from.CorrelationData );

              __SetBindingSpecificContext(from);
        }

        public override string ToString()
        {
            return string.Format("{0}(id: {1}, rid: {2})", GetType().Name, m_ID, RequestID);
        }


    }
}
