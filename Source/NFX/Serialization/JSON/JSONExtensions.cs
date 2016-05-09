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

using NFX.CodeAnalysis.Source;


namespace NFX.Serialization.JSON
{
    /// <summary>
    /// Provides JSON extension methods
    /// </summary>
    public static class JSONExtensions
    {
        /// <summary>
        ///  Deserializes JSON content into dynamic JSON object
        /// </summary>
        public static dynamic JSONToDynamic(this string json, bool caseSensitiveMaps = true)
        {
            return JSONReader.DeserializeDynamic(json, caseSensitiveMaps);
        }

        /// <summary>
        ///  Deserializes JSON content into dynamic JSON object
        /// </summary>
        public static dynamic JSONToDynamic(this Stream json, Encoding encoding = null, bool caseSensitiveMaps = true)
        {
            return JSONReader.DeserializeDynamic(json, encoding, caseSensitiveMaps);
        }


        /// <summary>
        ///  Deserializes JSON content into dynamic JSON object
        /// </summary>
        public static dynamic JSONToDynamic(this ISourceText json, bool caseSensitiveMaps = true)
        {
            return JSONReader.DeserializeDynamic(json, caseSensitiveMaps);
        }


        /// <summary>
        ///  Deserializes JSON content into IJSONDataObject
        /// </summary>
        public static IJSONDataObject JSONToDataObject(this string json, bool caseSensitiveMaps = true)
        {
            return JSONReader.DeserializeDataObject(json, caseSensitiveMaps);
        }

        /// <summary>
        ///  Deserializes JSON content into IJSONDataObject
        /// </summary>
        public static IJSONDataObject JSONToDataObject(this Stream json, Encoding encoding = null, bool caseSensitiveMaps = true)
        {
            return JSONReader.DeserializeDataObject(json, encoding, caseSensitiveMaps);
        }


        /// <summary>
        ///  Deserializes JSON content into IJSONDataObject
        /// </summary>
        public static IJSONDataObject JSONToDataObject(this ISourceText json, bool caseSensitiveMaps = true)
        {
            return JSONReader.DeserializeDataObject(json, caseSensitiveMaps);
        }



        /// <summary>
        ///  Serializes object into JSON string
        /// </summary>
        public static string ToJSON(this object root, Serialization.JSON.JSONWritingOptions options = null)
        {
            return JSONWriter.Write(root, options);
        }

        /// <summary>
        ///  Serializes object into JSON format using provided TextWriter
        /// </summary>
        public static void ToJSON(this object root, TextWriter wri, Serialization.JSON.JSONWritingOptions options = null)
        {
            JSONWriter.Write(root, wri, options);
        }

        /// <summary>
        ///  Serializes object into JSON format using provided stream and optional encoding
        /// </summary>
        public static void ToJSON(this object root, Stream stream, Serialization.JSON.JSONWritingOptions options = null, Encoding encoding = null)
        {
            JSONWriter.Write(root, stream, options, encoding);
        }
    }
}
