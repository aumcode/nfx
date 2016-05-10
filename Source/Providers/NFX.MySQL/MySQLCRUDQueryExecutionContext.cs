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

using MySql.Data.MySqlClient;

using NFX.DataAccess.CRUD;

namespace NFX.DataAccess.MySQL
{
    /// <summary>
    /// Provides query execution environment in MySql context 
    /// </summary>
    public struct MySQLCRUDQueryExecutionContext : ICRUDQueryExecutionContext
    {
       public readonly MySQLDataStoreBase  DataStore;
       public readonly MySqlConnection  Connection;
       public readonly MySqlTransaction Transaction;

       public MySQLCRUDQueryExecutionContext(MySQLDataStoreBase  store, MySqlConnection cnn, MySqlTransaction trans)
       {
            DataStore = store;
            Connection = cnn;
            Transaction = trans;
       }


       /// <summary>
       /// Based on store settings, converts CLR value to MySQL-acceptable value, i.e. GDID -> BYTE[].
       /// </summary>
       public object CLRValueToDB(MySQLDataStoreBase store, object value, out MySqlDbType? convertedDbType)
       {
          return CRUDGenerator.CLRValueToDB(DataStore, value, out convertedDbType);
       }

       /// <summary>
       /// Based on store settings, converts query parameters into MySQL-acceptable values, i.e. GDID -> BYTe[].
       /// This function is not idempotent
       /// </summary>
       public void ConvertParameters(MySqlParameterCollection pars)
       {
          CRUDGenerator.ConvertParameters(DataStore, pars);
       }

    }
}
