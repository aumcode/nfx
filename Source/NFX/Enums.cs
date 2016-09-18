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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX
{
    /// <summary>
    /// Format of the String Dump
    /// </summary>
    public enum DumpFormat
    {
        /// <summary>
        /// Perform no conversion - data copied as is
        /// </summary>
        Binary,

        /// <summary>
        /// Decimal string representation. E.g. "&lt;&lt;39, 16, 25, ...>>"
        /// </summary>
        Decimal,

        /// <summary>
        /// Hex string representation. E.g. "A1 B9 16 ..."
        /// </summary>
        Hex,

        /// <summary>
        /// Human readable string representation. E.g. "...Test 123\n..."
        /// </summary>
        Printable
    }
}
