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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX.Serialization.JSON
{
    /// <summary>
    /// Denotes a CLR type-safe entity (class or struct) that can directly write itself as JSON content string.
    /// This mechanism bypasses all of the reflection/dynamic code.
    /// This approach may be far more performant for some classes that need to serialize their state/data in JSON format,
    /// than relying on general-purpose JSON serializer that can serialize any type but is slower
    /// </summary>
    public interface IJSONWritable
    {
        /// <summary>
        /// Writes entitie's data/state as JSON string
        /// </summary>
        ///<param name="wri">
        ///TextWriter to write JSON content into
        ///</param>
        /// <param name="nestingLevel">
        /// A level of nesting that this instance is at, relative to the graph root.
        /// Implementations may elect to use this parameter to control indenting or ignore it
        /// </param>
        /// <param name="options">
        /// Writing options, such as indenting.
        /// Implementations may elect to use this parameter to control text output or ignore it
        /// </param>
        void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null);
    }


    /// <summary>
    /// Denotes a CLR type-safe entity (class or struct) that can directly read itself from IJSONDataObject which is supplied by JSON parser.
    /// This mechanism bypasses all of the reflection/dynamic code.
    /// This approach may be far more performant for some classes that need to de-serialize their state/data from JSON format,
    /// than relying on general-purpose JSON serializer that can deserialize any type but is slower.
    /// The particular type has to be allocated first, then it's instance can be hydrated with data/state using this method
    /// </summary>
    public interface IJSONReadable
    {
        /// <summary>
        /// Reads entitie's data/state from low-level IJSONDataObject which is supplied right by JSONParser.
        /// An implementer may elect to throw various types of esceptions to signal such conditions as:
        ///  unknown key map, or too many fields not supplied etc.
        /// </summary>
        /// <param name="data">JSONParser-supplied object</param>
        void ReadAsJSON(IJSONDataObject data);
    }
}
