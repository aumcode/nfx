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

namespace NFX.Glue
{

    /// <summary>
    /// Decorates interfaces that represent glued contract points
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public class GluedAttribute : Attribute
    {
         public GluedAttribute() {}
    }

    /// <summary>
    /// Decorates methods stipulating type of RequestMsg-derivative used for method call arguments marshalling
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class ArgsMarshallingAttribute : Attribute
    {
         public ArgsMarshallingAttribute(Type requestMsgType)
         {
            if (requestMsgType==null ||
                requestMsgType==typeof(Protocol.RequestAnyMsg) ||
                !requestMsgType.IsSubclassOf(typeof(Protocol.RequestMsg)))

             throw new InvalidGlueOperationException(StringConsts.GLUE_ARGS_MARSHALLING_INVALID_REQUEST_MSG_TYPE_ERROR);

            RequestMsgType = requestMsgType;
         }

         /// <summary>
         /// Thr type of RequestMsg-derivative used to marshall the request arguments
         /// </summary>
         public readonly Type RequestMsgType;
    }


    /// <summary>
    /// Decorates methods that do not generate response message after execution.
    /// They must return void and if exception occurs it is not reported to the caller
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class OneWayAttribute : Attribute
    {
         public OneWayAttribute() {}
    }

    /// <summary>
    /// Decorates methods that initialize instance of the server class and retain class instance
    ///  in the server runtime
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class ConstructorAttribute : Attribute
    {
        public ConstructorAttribute() {}
    }

    /// <summary>
    /// Decorates methods that finalize instance of the server class and release class instance
    ///  in the server runtime
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class DestructorAttribute : Attribute
    {
       public DestructorAttribute() {}
    }


    /// <summary>
    /// Denotes server instance modes - how server instances relate to calls
    /// </summary>
    public enum ServerInstanceMode
    {
        /// <summary>
        /// Indicates that the same single process-wide instance will serve all request for particular contract.
        /// The server MUST be thread-safe
        /// </summary>
        Singleton = 0,

        /// <summary>
        /// Indicates that every request for a particular contract will create an instance which will live until the method exits
        /// </summary>
        PerCall,

        /// <summary>
        /// Indicates that the instance is stateful and will live between a call to
        ///  [Constructor]-decorated method and [Destructor]-decorated method, or until timeout interval has passed.
        /// The implementation may or may not be thread-safe, if it is, then [ThreadSafe] attribute may be used to avoid instance locking
        /// </summary>
        Stateful,

        /// <summary>
        /// Indicates that the instance is stateful and will live between a call to
        ///  either [Constructor]-decorated method or first call to any method, and [Destructor]-decorated method or until timeout interval has passed.
        /// The implementation may or may not be thread-safe, if it is then [ThreadSafe] attribute may be used to avoid instance locking.
        /// This mode is simiar to 'Stateful' but does not require the caller to explicitly call the [Constructor]-decorated method first
        /// </summary>
        AutoConstructedStateful
    }


    /// <summary>
    /// Specifies the instance mode and timeout values for server classes that implement the decorated contract.
    /// If server class does not support state then timeout is ignored
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public class LifeCycleAttribute : Attribute
    {
        public LifeCycleAttribute() {}

        public LifeCycleAttribute(ServerInstanceMode mode)
        {
          m_Mode = mode;
        }

        public LifeCycleAttribute(ServerInstanceMode mode, int timeoutMs)
        {
          m_Mode = mode;
          m_TimeoutMs = timeoutMs;
        }

        private ServerInstanceMode m_Mode;
        private int m_TimeoutMs;


        public ServerInstanceMode Mode {  get { return m_Mode;}  set{ m_Mode = value;} }
        public int TimeoutMs {  get { return m_TimeoutMs;} set{ m_TimeoutMs = value;}}
    }


    /// <summary>
    /// Indicates that server instance should not be lock()-ed by Glue handler because it is implemented in a thread-safe way.
    /// This attribute has no effect for Glue servers that are not [LifeCycle(Stateful)], because they are never locked.
    /// Singleton servers must be thread-safe by definition so this attribute is not needed for Singleton servers.
    /// If the server is implemented in a thread-safe way, then addition of this attribute may give 15-30% performance boost
    ///  from Glue runtime when many parallel requests are trying to work with the same instance, however this benefit
    ///  may be nullified by inefficient user locking code within server implementation. Leaving this attribute out is safer
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ThreadSafeAttribute : Attribute
    {
    }


    /// <summary>
    /// Indicates that contract supports authentication using AuthenticationHeader.
    /// When header is passed then Glue server will use its data to set user context through Application.SecurityManager.
    /// If this attribute not set then Glue runtime will ignore AuthenticationHeader
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class AuthenticationSupportAttribute : Attribute
    {
    }

}
