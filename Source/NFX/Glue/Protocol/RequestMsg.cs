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
using System.Reflection;
using System.Text;

namespace NFX.Glue.Protocol
{
    /// <summary>
    /// This message is sent from client to server and contains contract type, method specification and invocation arguments
    /// which are either included as object[] if RequestAnyMsg is used or inlined in RequestMsg- typed derivative for speed to avoid boxing
    /// </summary>
    [Serializable]
    public abstract class RequestMsg : Msg
    {

        /// <summary>
        /// Constructs request message from method information and arguments for call invocation.
        /// This constructor is slower as it uses reflection
        /// </summary>
        protected RequestMsg(MethodInfo method, Guid? instance) : base()
        {
            m_Contract = new TypeSpec(method.DeclaringType);
            m_Method = new MethodSpec(method);
            m_OneWay = Attribute.IsDefined(method, typeof(OneWayAttribute));

            m_RemoteInstance = instance;
        }


        /// <summary>
        /// Constructs request message from pre-computed  specs obtained by reflection.
        /// This constructor is the fastest as it does not use reflection
        /// </summary>
        protected RequestMsg(TypeSpec contract, MethodSpec method, bool oneWay, Guid? instance) : base()
        {
            m_Contract = contract;
            m_Method = method;
            m_OneWay = oneWay;
            m_RemoteInstance = instance;
        }


        /// <summary>
        /// This .ctor is handy for message inspectors.
        /// Creates a substitute message for the original one with new values.
        /// Binding-specific context is cloned and headers/correlation data are cloned conditionaly
        /// </summary>
        protected RequestMsg(RequestMsg inspectedOriginal,
                          MethodInfo method, Guid? instance,
                          bool cloneHeaders = true, bool cloneCorrelation = true) : this(method, instance)
        {
           m_ServerTransport = inspectedOriginal.m_ServerTransport;
           CloneState(inspectedOriginal, cloneHeaders, cloneCorrelation);
        }


        /// <summary>
        /// This .ctor is handy for message inspectors.
        /// Creates a substitute message for the original one with new values.
        /// Binding-specific context is cloned and headers/correlation data are cloned conditionaly
        /// </summary>
        protected RequestMsg(RequestMsg inspectedOriginal,
                          TypeSpec contract, MethodSpec method, bool oneWay, Guid? instance,
                          bool cloneHeaders = true, bool cloneCorrelation = true) : this(contract, method, oneWay, instance)
        {
          m_ServerTransport = inspectedOriginal.m_ServerTransport;
          CloneState(inspectedOriginal, cloneHeaders, cloneCorrelation);
        }





        [NonSerialized]
        protected ServerTransport m_ServerTransport;
        /// <summary>
        /// Implementation-specific internal method not to be used by developers
        /// </summary>
        public void __setServerTransport(ServerTransport t) { m_ServerTransport = t; }




        private TypeSpec m_Contract;

        private Guid? m_RemoteInstance;

        private MethodSpec m_Method;

        private bool m_OneWay;



        [NonSerialized]
        private ApplicationModel.ISession m_Session;


           /// <summary>
           /// Returns request ID for this instance. Every request is uniquely identified
           /// </summary>
           public override FID RequestID
           {
             get { return ID;}
           }


           /// <summary>
           /// If server is stateful then this property contains remote instance ID
           /// </summary>
           public Guid? RemoteInstance
           {
             get { return m_RemoteInstance; }
           }


           /// <summary>
           /// Returns contract type that this call belongs to. This property throws if actual specified type can not be found
           /// </summary>
           public Type Contract
           {
             get { return m_Contract.GetSpecifiedType();}
           }

           /// <summary>
           /// Returns contract type spec that this call belongs to
           /// </summary>
           public TypeSpec ContractSpec
           {
             get { return m_Contract;}
           }

           /// <summary>
           /// Gets a name of method to be invoked on the other side
           /// </summary>
           public string MethodName
           {
             get {return m_Method.MethodName; }
           }

           /// <summary>
           /// Gets a method specification to be invoked on the other side
           /// </summary>
           public MethodSpec Method
           {
             get {return m_Method; }
           }

           /// <summary>
           /// Indicates that no response is expected
           /// </summary>
           public bool OneWay
           {
             get { return m_OneWay; }
           }

           /// <summary>
           /// Returns transport that this request arrived through. Property is to be used only on server
           /// </summary>
           public ServerTransport ServerTransport { get { return m_ServerTransport; } }

           /// <summary>
           /// Session reference that can be used to pass session object that was already determined by transport (i.e. HttpServerTransport)
           ///  into Glue server handler. This property is NOT transmitted/serialized over wire
           /// </summary>
           public ApplicationModel.ISession Session { get{ return m_Session; }  set{ m_Session = value;} }

    }


    /// <summary>
    /// Represents request msg that marshalls arguments as object[].
    /// Although the most convenient and simple, this way of working with glue is slower than
    /// using RequestTypedMsg which needs to be derived-from for every method
    /// </summary>
    public sealed class RequestAnyMsg : RequestMsg
    {

        /// <summary>
        /// Constructs request message from method information and arguments for call invocation.
        /// This constructor is slower as it uses reflection
        /// </summary>
        public RequestAnyMsg(MethodInfo method, Guid? instance, object[] args) : base(method, instance)
        {
           m_Arguments = args;
        }

        /// <summary>
        /// Constructs request message from pre-computed  specs obtained by reflection.
        /// This constructor is the fastest as it does not use reflection
        /// </summary>
        public RequestAnyMsg(TypeSpec contract, MethodSpec method, bool oneWay, Guid? instance, object[] args) : base(contract, method, oneWay, instance)
        {
            m_Arguments = args;
        }

        /// <summary>
        /// This .ctor is handy for message inspectors.
        /// Creates a substitute message for the original one with new values.
        /// Binding-specific context is cloned and headers/correlation data are cloned conditionaly
        /// </summary>
        public RequestAnyMsg(RequestMsg inspectedOriginal,
                          MethodInfo method, Guid? instance, object[] args,
                          bool cloneHeaders = true, bool cloneCorrelation = true) : base(inspectedOriginal, method, instance, cloneHeaders, cloneCorrelation)
        {
          m_Arguments = args;
        }


        /// <summary>
        /// This .ctor is handy for message inspectors.
        /// Creates a substitute message for the original one with new values.
        /// Binding-specific context is cloned and headers/correlation data are cloned conditionaly
        /// </summary>
        public RequestAnyMsg(RequestMsg inspectedOriginal,
                          TypeSpec contract, MethodSpec method, bool oneWay, Guid? instance, object[] args,
                          bool cloneHeaders = true, bool cloneCorrelation = true) : base(inspectedOriginal, contract, method, oneWay, instance, cloneHeaders, cloneCorrelation)
        {
         m_Arguments = args;
        }



           private object[] m_Arguments;


           /// <summary>
           /// Returns call arguments
           /// </summary>
           public object[] Arguments
           {
             get { return m_Arguments;}
           }
    }


}
