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
    /// Represents a response message sent by called party
    /// </summary>
    [Serializable]
    public sealed class ResponseMsg : Msg
    {

        public ResponseMsg(FID requestID, object returnValue) : this(requestID, null, returnValue)
        {}


        public ResponseMsg(FID requestID, Guid? instance,  object returnValue) : base()
        {
          m_RequestID = requestID;
          m_RemoteInstance = instance;
          m_ReturnValue = returnValue;
        }


        /// <summary>
        /// This .ctor is handy for message inspectors.
        /// Creates a substitute message for the original one with new value.
        /// Binding-specific context is cloned and headers/correlation data are cloned conditionaly
        /// </summary>
        public ResponseMsg(ResponseMsg inspectedOriginal, object newReturnValue, bool cloneHeaders = true, bool cloneCorrelation = true) : base()
        {
          m_RequestID = inspectedOriginal.m_RequestID;
          m_RemoteInstance = inspectedOriginal.m_RemoteInstance;
          m_ReturnValue = newReturnValue;

          CloneState(inspectedOriginal, cloneHeaders, cloneCorrelation);
        }


       private FID m_RequestID;

       //note: out and ref parameters ar not supported
       //may contain remote exception data
       private object m_ReturnValue;

       private Guid? m_RemoteInstance;


       /// <summary>
       /// Returns request ID this response is for
       /// </summary>
       public override FID RequestID
       {
         get { return m_RequestID;}
       }

       /// <summary>
       /// Returns return value of the called method. Note: out and ref params are not supported
       /// </summary>
       public object ReturnValue
       {
         get { return m_ReturnValue; }
       }


       /// <summary>
       /// For stateful servers returns instance ID
       /// </summary>
       public Guid? RemoteInstance
       {
         get { return m_RemoteInstance; }
       }

       /// <summary>
       /// Returns remote exception data if any
       /// </summary>
       public RemoteExceptionData ExceptionData { get {return m_ReturnValue as RemoteExceptionData; } }

       /// <summary>
       /// Returns true when reponse does not contain remote server exception which is represented by RemoteExceptionData
       /// </summary>
       public bool OK { get {return !(m_ReturnValue is RemoteExceptionData); }  }
    }




}
