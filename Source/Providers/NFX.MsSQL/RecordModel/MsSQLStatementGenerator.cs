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


/* NFX by ITAdapter
 * Originated: 2008.03
 * Revision: NFX 1.0  2011.02.06
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.SqlClient;

using NFX.DataAccess;
using NFX.DataAccess.MsSQL;

using NFX.MsSQL;

namespace NFX.RecordModel.DataAccess.MsSQL
{
  
  /// <summary>
  /// This helper class generates table SELECT/INSERT/UPDATE/DELETE statements for MS SQL server (2000 or higher)
  /// depending on model state (last posted operation , key, etc).
  /// All values are emitted as parameters.
  /// This class respects two key types: CounterDataStoreKey and NameValueDataStoreKey
  ///  depending on what key type was passed corresponding WHERE clause is generated 
  /// </summary>
  public static class MsSQLStatementGenerator
  {
   #region Public 
   
    /// <summary>
    /// Auto generates select sql and params. If sqlStatement!=null then params are added to that statement
    /// </summary>
    public static void Load(SqlConnection cnn, string sqlStatement ,ModelBase instance, IDataStoreKey key, object[] extra)
    {
        var autoSql = string.IsNullOrEmpty(sqlStatement);

        var record = asSuitableRecordInstance(instance, autoSql);

        var select = new StringBuilder();

      if (autoSql)
      {
        foreach (var fld in record.Fields)
          if (fld.StoreFlag == StoreFlag.LoadAndStore || fld.StoreFlag == StoreFlag.OnlyLoad)
            select.AppendFormat(" T1.[{0}],", fld.FieldName);

        if (select.Length > 0) 
         select.Remove(select.Length - 1, 1);// remove ","
        else throw new MsSQLDataAccessException(StringConsts.LOAD_NO_SELECT_COLUMNS_ERROR);
         
      }

      var pk = key ?? record.DataStoreKey;

      if (pk == null)
        throw new MsSQLDataAccessException(StringConsts.KEY_UNAVAILABLE_ERROR);

      using (var cmd = cnn.CreateCommand())
      {

        var where = keyToWhere(pk, cmd.Parameters);

        if (autoSql)
          cmd.CommandText = string.Format("SELECT {0} FROM [{1}] T1 WHERE {2}", select, record.TableName, where);
        else
          cmd.CommandText =  string.Format(sqlStatement, where);

        using (var reader = cmd.ExecuteReader())
        {
          if (reader.Read())
            reader.CopyFieldsToRecordFields(record);
          else
            throw new MsSQLDataAccessException(string.Format(StringConsts.LOADING_ENTITY_NOT_FOUND_ERROR, pk));
        }//using reader

      }//using command
    }

    public static void Save(SqlConnection cnn, ModelBase instance, IDataStoreKey key, object[] extra)
    {
        
      var record = asSuitableRecordInstance(instance, true);

      if (record.LastPostedChange!=ChangeType.Created && record.LastPostedChange!=ChangeType.Edited)
        throw new MsSQLDataAccessException(string.Format(StringConsts.MODEL_INVALID_STATE_ERROR, "Created || Edited", instance.LastPostedChange)); 

      bool insert = instance.LastPostedChange==ChangeType.Created;

      var cnames = new StringBuilder();
      var values = new StringBuilder();
      var vparams = new List<SqlParameter>();
      var vpidx = 0;
      foreach (var fld in record.Fields)
        if (fld.StoreFlag == StoreFlag.LoadAndStore || fld.StoreFlag == StoreFlag.OnlyStore)
         if (
              insert || fld.Modified
            )
         {
           cnames.AppendFormat(" [{0}],", fld.FieldName);
           if (fld.HasValue)
           {
             var pname = string.Format("@VAL{0}", vpidx);
             
             if (insert)
               values.AppendFormat(" {0},", pname);
             else
               values.AppendFormat(" [{0}] = {1},", fld.FieldName, pname);
             
             var par = new SqlParameter();
             par.ParameterName = pname;
             par.Value = fld.ValueAsObject;
             vparams.Add(par);
             
             vpidx++;
           }  
           else
           {
             if (insert)
               values.Append(" NULL,");
             else
               values.AppendFormat(" [{0}] = NULL,", fld.FieldName);  
           }
         }


      if (cnames.Length > 0)
      {
        cnames.Remove(cnames.Length - 1, 1);// remove ","
        if (values.Length > 0) values.Remove(values.Length - 1, 1);// remove ","
      }
      else
        return;//nothing has been modified



      
      using (var cmd = cnn.CreateCommand())
      {
        var sql = string.Empty;
        
        if (insert) //INSERT
        {
            sql = 
                 string.Format("INSERT INTO [{0}] ({1}) VALUES ({2})", record.TableName, cnames, values);
        }
        else //UPDATE
        {
          var pk = key ?? record.DataStoreKey;

          if (pk == null)
            throw new MsSQLDataAccessException(StringConsts.KEY_UNAVAILABLE_ERROR);
          
          var where = keyToWhere(pk, cmd.Parameters);

          if (!string.IsNullOrEmpty(where))
           sql = string.Format("UPDATE [{0}] T1  SET {1} WHERE {2}", record.TableName, values, where);
          else
           sql = string.Format("UPDATE [{0}] T1  SET {1}", record.TableName, values); 
        }


        cmd.CommandText = sql;
        cmd.Parameters.AddRange(vparams.ToArray());


        if (cmd.ExecuteNonQuery()==0)
          throw new MsSQLDataAccessException(string.Format(StringConsts.NO_ROWS_AFFECTED_ERROR, instance.LastPostedChange == ChangeType.Created ? "Insert":"Update"));
       

      }//using command
    }



    public static void Delete(SqlConnection cnn, ModelBase instance, IDataStoreKey key, object[] extra)
    {
      var record = asSuitableRecordInstance(instance, true);


      using (var cmd = cnn.CreateCommand())
      {
          var pk = key ?? record.DataStoreKey;

          if (pk == null)
            throw new MsSQLDataAccessException(StringConsts.KEY_UNAVAILABLE_ERROR);

          var where = keyToWhere(pk, cmd.Parameters);

          if (!string.IsNullOrEmpty(where))
            cmd.CommandText = string.Format("DELETE T1 FROM [{0}] T1 WHERE {1}", record.TableName, where);
          else
            cmd.CommandText = string.Format("DELETE T1 FROM [{0}] T1", record.TableName);


         if (cmd.ExecuteNonQuery() == 0)
           throw new MsSQLDataAccessException(string.Format(StringConsts.NO_ROWS_AFFECTED_ERROR, "Delete"));


      }//using command
    }

   #endregion


   #region .pvt impl.

    private static Record asSuitableRecordInstance(ModelBase model, bool autoSql)
    {
      var record = model as Record;

      if (record == null)
        throw new MsSQLDataAccessException(string.Format(StringConsts.MODEL_TYPE_NOT_RECORD_ERROR, model.GetType().Name));

      if (autoSql)
      {
          if (string.IsNullOrEmpty(record.TableName))
            throw new MsSQLDataAccessException(string.Format(StringConsts.RECORD_TABLE_NAME_ERROR, model.GetType().Name));
      }
      
      return record;
    }



    private static string keyToWhere(IDataStoreKey key, SqlParameterCollection parameters)
    {
      string where = null;

      if (key is CounterDataStoreKey)
      {
        where = "T1.COUNTER = @CTR";
        var par = new SqlParameter();
        par.ParameterName = "@CTR";
        par.Value = ((CounterDataStoreKey)key).Counter;

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
            s.AppendFormat(" (T1.[{0}] = @P{1}) AND", e.Key, idx);
            var par = new SqlParameter();
            par.ParameterName = "@P" + idx.ToString();
            par.Value = e.Value;
            parameters.Add(par);

            idx++;
          }

          if (s.Length > 0) s.Remove(s.Length - 3, 3);//cut "AND"

          where = s.ToString();
        }
        else
          throw new MsSQLDataAccessException(StringConsts.INVALID_KEY_TYPE_ERROR);

      return where;
    }
    
    #endregion
    
    
  }
}
