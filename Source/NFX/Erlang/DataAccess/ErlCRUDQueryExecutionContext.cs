/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 Dmitriy Khmaladze, IT Adapter Inc / 2015-2016 Aum Code LLC
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

using NFX.DataAccess.CRUD;
using NFX.DataAccess.CRUD.Subscriptions;
using NFX.Erlang;

namespace NFX.DataAccess.Erlang
{
  /// <summary>
    /// Provides query execution environment in Erlang context
    /// </summary>
    public struct ErlCRUDQueryExecutionContext : ICRUDQueryExecutionContext
    {
        public ErlCRUDQueryExecutionContext(ErlDataStore store, ErlMbox erlMBox = null, DataTimeStamp? ts = null)
        {
          this.DataStore = store;
          ErlMailBox = erlMBox;
          SubscriptionTimestamp = ts;
        }

        public readonly ErlDataStore  DataStore;
        public readonly ErlMbox ErlMailBox;
        public readonly DataTimeStamp? SubscriptionTimestamp;
    }
}
