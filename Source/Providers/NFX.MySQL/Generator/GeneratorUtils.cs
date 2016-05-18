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

using NFX;
using NFX.Log;
using NFX.DataAccess;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;


namespace NFX.DataAccess.MySQL
{

  /// <summary>
  /// Facilitates various SQL-construction and logging tasks
  /// </summary>
  public static class GeneratorUtils
  {

        public static string KeyToWhere(IDataStoreKey key, MySqlParameterCollection parameters)
        {
          string where = null;

          if (key is CounterDataStoreKey)
          {
            where = "T1.COUNTER = ?CTR";
            var par = new MySqlParameter();
            par.ParameterName = "?CTR";
            par.Value = ((CounterDataStoreKey)key).Counter;

            parameters.Add(par);
          }
          else
          if (key is GDID)
          {
            where = "T1.GDID = ?CTR";
            var par = new MySqlParameter();
            par.ParameterName = "?CTR";
            par.Value = key;

            parameters.Add(par);
          }
          else
            if (key is NameValueDataStoreKey)
            {
              var dict = key as NameValueDataStoreKey;
              var s = new StringBuilder();
              var idx = 0;

              foreach (var e in dict)
              {
                s.AppendFormat(" (T1.`{0}` = ?P{1}) AND", e.Key, idx);
                var par = new MySqlParameter();
                par.ParameterName = "?P" + idx.ToString();
                par.Value = e.Value;
                parameters.Add(par);

                idx++;
              }

              if (s.Length > 0) s.Remove(s.Length - 3, 3);//cut "AND"

              where = s.ToString();
            }
            else
              throw new MySQLDataAccessException(StringConsts.INVALID_KEY_TYPE_ERROR);

          return where;
        }


        public static bool HasFieldInNamedKey(string fieldName, IDataStoreKey key)
        {
          var nvk = key as NameValueDataStoreKey;
          if (nvk==null || fieldName.IsNullOrWhiteSpace()) return false;
          return nvk.ContainsKey(fieldName);
        }

        public static void LogCommand(StoreLogLevel level, string from, MySqlCommand cmd, Exception error)
        {
            if (level==StoreLogLevel.None) return;
            if (!App.Available) return;
            
            MessageType mt = level==StoreLogLevel.Debug ? MessageType.DebugSQL : MessageType.TraceSQL;

            var descr = new StringBuilder(512);
            descr.Append("Transaction: ");
            if (cmd.Transaction==null)
                descr.AppendLine("null");
            else
                descr.AppendLine(cmd.Transaction.IsolationLevel.ToString());
            foreach(var p in cmd.Parameters.Cast<MySqlParameter>())
            {
                descr.AppendFormat("Parameter {0} = {1}", p.ParameterName, p.Value!=null?p.Value.ToString():"null");
            }

            var msg = new Message
            {             
                Type = mt,
                From = from,
                Topic = "DataStore",
                Exception = error,
                Text = cmd.CommandText,
                Parameters = descr.ToString()
            };


            App.Log.Write( msg );
        }


  }
}
