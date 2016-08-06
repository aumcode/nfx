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
using System.Runtime.Serialization;

namespace NFX.Environment
{

    /// <summary>
    /// Represents an entity that can resolve variables
    /// </summary>
    public interface IEnvironmentVariableResolver
    {
        /// <summary>
        /// Turns named variable into its value or null
        /// </summary>
        string ResolveEnvironmentVariable(string name);
    }


    /// <summary>
    /// Resolves variables using Windows environment variables.
    /// NOTE: When serialized a new instance is created which will not equal by reference to static.Instance property
    /// </summary>
    [Serializable]//but there is nothing to serialize
    public sealed class WindowsEnvironmentVariableResolver : IEnvironmentVariableResolver
    {
        private static WindowsEnvironmentVariableResolver s_Instance = new WindowsEnvironmentVariableResolver();

        private WindowsEnvironmentVariableResolver()
        {

        }

        /// <summary>
        /// Returns a singleton class instance
        /// </summary>
        public static WindowsEnvironmentVariableResolver Instance
        {
          get { return s_Instance; }
        }

        public string ResolveEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name);
        }
    }

    /// <summary>
    /// Allows for simple ad-hoc environment var passing to configuration
    /// </summary>
    [Serializable]
    public sealed class Vars : Dictionary<string, string>, IEnvironmentVariableResolver
    {

        public Vars() : base(StringComparer.InvariantCultureIgnoreCase)
        {

        }

        private Vars(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }


        public string ResolveEnvironmentVariable(string name)
        {
            string val;
            if (this.TryGetValue(name, out val)) return val;

            return string.Empty;
        }
    }


}
