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

using NFX;
using NFX.DataAccess;
using NFX.RecordModel;
using MySql.Data.MySqlClient;
using NFX.DataAccess.CRUD;


namespace NFX.DataAccess.MySQL
{
   
  internal static class RecordModelGenerator
  {

    /// <summary>
    /// Auto generates select sql and params. If sqlStatement!=null then params are added to that statement
    /// </summary>
    public static void Load(MySQLDataStoreBase store, MySqlConnection cnn, string sqlStatement, ModelBase instance, IDataStoreKey key, object[] extra)
    {
        var autoSql = string.IsNullOrEmpty(sqlStatement);

        var record = GeneratorUtils.AsSuitableRecordInstance(instance, autoSql);

            var select = new StringBuilder();

      if (autoSql)
      {
        foreach (var fld in record.Fields)
          if (fld.StoreFlag == StoreFlag.LoadAndStore || fld.StoreFlag == StoreFlag.OnlyLoad)
            select.AppendFormat(" T1.`{0}`,", fld.FieldName);

        if (select.Length > 0)
          select.Remove(select.Length - 1, 1);// remove ","
        else throw new MySQLDataAccessException(StringConsts.LOAD_NO_SELECT_COLUMNS_ERROR);

      }

      var pk = key ?? record.DataStoreKey;

      if (pk == null)
        throw new MySQLDataAccessException(StringConsts.KEY_UNAVAILABLE_ERROR);

      using (var cmd = cnn.CreateCommand())
      {

        var where = GeneratorUtils.KeyToWhere(pk, cmd.Parameters);

        if (autoSql)
          cmd.CommandText = string.Format("SELECT {0} FROM `{1}` T1 WHERE {2}", select, record.TableName, where);
        else
          cmd.CommandText = string.Format(sqlStatement, where);


        MySqlDataReader reader = null;
        try
        {
            reader = cmd.ExecuteReader();
            GeneratorUtils.LogCommand(store.LogLevel, "rmload-ok", cmd, null);
        }
        catch(Exception error)
        {
            GeneratorUtils.LogCommand(store.LogLevel, "rmload-error", cmd, error);
            throw;
        }


        using (reader)
        {
          if (reader.Read())
            reader.CopyFieldsToRecordFields(record);
          else
            throw new MySQLDataAccessException(string.Format(StringConsts.LOADING_ENTITY_NOT_FOUND_ERROR, pk));
        }//using reader

      }//using command
    }

    public static void Save(MySQLDataStoreBase store, MySqlConnection cnn, ModelBase instance, IDataStoreKey key, object[] extra)
    {
        
      var record = GeneratorUtils.AsSuitableRecordInstance(instance, true);


      if (record.LastPostedChange != ChangeType.Created && record.LastPostedChange != ChangeType.Edited)
        throw new MySQLDataAccessException(string.Format(StringConsts.MODEL_INVALID_STATE_ERROR, "Created || Edited", instance.LastPostedChange));

      bool insert = instance.LastPostedChange == ChangeType.Created;


      var cnames = new StringBuilder();
      var values = new StringBuilder();
      var vparams = new List<MySqlParameter>();
      var vpidx = 0;
      foreach (var fld in record.Fields)
        if (fld.StoreFlag == StoreFlag.LoadAndStore || fld.StoreFlag == StoreFlag.OnlyStore)
          if (
               insert || fld.Modified
             )
          {
            cnames.AppendFormat(" `{0}`,", fld.FieldName);
            if (fld.HasValue)
            {
              var pname = string.Format("?VAL{0}", vpidx);

              if (insert)
                values.AppendFormat(" {0},", pname);
              else
                values.AppendFormat(" `{0}` = {1},", fld.FieldName, pname);

              var par = new MySqlParameter();
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
                values.AppendFormat(" `{0}` = NULL,", fld.FieldName);
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
               string.Format("INSERT INTO `{0}` ({1}) VALUES ({2})", record.TableName, cnames, values);
        }
        else //UPDATE
        {
          var pk = key ?? record.DataStoreKey;

          if (pk == null)
            throw new MySQLDataAccessException(StringConsts.KEY_UNAVAILABLE_ERROR);

          var where = GeneratorUtils.KeyToWhere(pk, cmd.Parameters);

          if (!string.IsNullOrEmpty(where))
           sql = string.Format("UPDATE `{0}` T1  SET {1} WHERE {2}", record.TableName, values, where);
          else
           sql = string.Format("UPDATE `{0}` T1  SET {1}", record.TableName, values); 
        }


        cmd.CommandText = sql;
        cmd.Parameters.AddRange(vparams.ToArray());


        var affected = 0;
        try
        {
            affected = cmd.ExecuteNonQuery();
            GeneratorUtils.LogCommand(store.LogLevel, "rmsave-ok", cmd, null);
        }
        catch(Exception error)
        {
            GeneratorUtils.LogCommand(store.LogLevel, "rmsave-error", cmd, error);
            throw;
        }

        if (affected == 0)
          throw new MySQLDataAccessException(string.Format(StringConsts.NO_ROWS_AFFECTED_ERROR, instance.LastPostedChange == ChangeType.Created ? "Insert" : "Update"));


      }//using command
    }



    public static void Delete(MySQLDataStoreBase store, MySqlConnection cnn, ModelBase instance, IDataStoreKey key, object[] extra)
    {
      var record = GeneratorUtils.AsSuitableRecordInstance(instance, true);

   
      using (var cmd = cnn.CreateCommand())
      {
        var pk = key ?? record.DataStoreKey;

        if (pk == null)
          throw new MySQLDataAccessException(StringConsts.KEY_UNAVAILABLE_ERROR);

        var where = GeneratorUtils.KeyToWhere(pk, cmd.Parameters);

          if (!string.IsNullOrEmpty(where))
            cmd.CommandText = string.Format("DELETE T1 FROM `{0}` T1 WHERE {1}", record.TableName, where);
          else
            cmd.CommandText = string.Format("DELETE T1 FROM `{0}` T1", record.TableName);

        var affected = 0;
        try
        {
            affected = cmd.ExecuteNonQuery();
            GeneratorUtils.LogCommand(store.LogLevel, "rmdelete-ok", cmd, null);
        }
        catch(Exception error)
        {
            GeneratorUtils.LogCommand(store.LogLevel, "rmdelete-error", cmd, error);
            throw;
        }

        if (affected == 0)
          throw new MySQLDataAccessException(string.Format(StringConsts.NO_ROWS_AFFECTED_ERROR, "Delete"));


      }//using command
    }




  }
}
