/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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
using System.Runtime.Serialization;

namespace NFX.Health
{

    /// <summary>
    /// Represents general ancestor for exceptions thrown by health checks
    /// </summary>
    [Serializable]
    public class HealthException : NFXException
    {
        public HealthException()
        {
        }

        public HealthException(string message)
            : base(message)
        {
        }

        public HealthException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected HealthException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

    }


    /// <summary>
    /// Thrown by Health.CheckList
    /// </summary>
    [Serializable]
    public class HealthCheckListException : NFXException
    {
        public HealthCheckListException()
        {
        }

        public HealthCheckListException(string message)
            : base(message)
        {
        }

        public HealthCheckListException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected HealthCheckListException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

    }
}