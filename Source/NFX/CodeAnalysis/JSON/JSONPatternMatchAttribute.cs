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

namespace NFX.CodeAnalysis.JSON
{
    /// <summary>
    /// Base class for JSON pattern matching
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited=true)]
    public abstract class JSONPatternMatchAttribute : Attribute
    {

        /// <summary>
        /// Checks all pattern match attributes against specified member info until first match found
        /// </summary>
        public static bool Check(MemberInfo info, JSONLexer content)
        {
            var attrs = info.GetCustomAttributes(typeof(JSONPatternMatchAttribute), true).Cast<JSONPatternMatchAttribute>();
            foreach(var attr in attrs)
                if (attr.Match(content)) return true;

            return false;
        }


        /// <summary>
        /// Override to perform actual pattern matching, i.e. the one that uses FSM
        /// </summary>
        public abstract bool Match(JSONLexer content);
    }
}
