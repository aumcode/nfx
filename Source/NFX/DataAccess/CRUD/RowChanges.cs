/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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

namespace NFX.DataAccess.CRUD
{
    /// <summary>
    /// Describes what kind of modification was done
    /// </summary>
    public enum RowChangeType { Insert, Upsert, Update, Delete }


    /// <summary>
    /// Describes row modification
    /// </summary>
    public struct RowChange
    {
        public RowChange(RowChangeType type, Row row, IDataStoreKey key)
        {
            ChangeType = type;
            Row = row;
            Key = key;
        }


        public readonly RowChangeType ChangeType;
        public readonly Row Row;
        public readonly IDataStoreKey Key;
    }
}
