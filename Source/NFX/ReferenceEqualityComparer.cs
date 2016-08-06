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
using System.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NFX
{
    /// <summary>
    /// Checks for reference equality. Use ReferenceEqualityComparer(T).Instance
    /// </summary>
    public sealed class ReferenceEqualityComparer<T> : EqualityComparer<T>
    {
        private static ReferenceEqualityComparer<T> s_Instance = new ReferenceEqualityComparer<T>();

        public static ReferenceEqualityComparer<T> Instance { get { return s_Instance;}}

        private ReferenceEqualityComparer() {}


        public override bool Equals(T x, T y)
        {
            return object.ReferenceEquals(x, y);
        }

        public override int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}

