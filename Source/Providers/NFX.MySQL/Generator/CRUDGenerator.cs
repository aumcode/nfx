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
using MySql.Data.MySqlClient;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;

namespace NFX.DataAccess.MySQL
{

  internal static class CRUDGenerator
  {

      public static int CRUDInsert(MySQLDataStoreBase store, MySqlConnection cnn, MySqlTransaction trans, Row row, FieldFilterFunc filter)
      {
        try
        {           
            return crudInsert(store, cnn, trans, row, filter);
        }
        catch(Exception error)
        {          
           throw new MySQLDataAccessException(
                          StringConsts.CRUD_STATEMENT_EXECUTION_ERROR.Args("insert", error.ToMessageWithType(), error),
                          error, 
                          KeyViolationKind.Unspecified, 
                          keyViolationName(error));
        }
      }

      public static int CRUDUpdate(MySQLDataStoreBase store, MySqlConnection cnn, MySqlTransaction trans, Row row, IDataStoreKey key, FieldFilterFunc filter)
      {
        try
        {
            return crudUpdate(store, cnn, trans, row, key, filter);
        }
        catch(Exception error)
        {
           throw new MySQLDataAccessException(
                         StringConsts.CRUD_STATEMENT_EXECUTION_ERROR.Args("update",
                         error.ToMessageWithType(), error),
                         error,
                         KeyViolationKind.Unspecified, 
                         keyViolationName(error)
                         );
        }
      }

      public static int CRUDUpsert(MySQLDataStoreBase store, MySqlConnection cnn, MySqlTransaction trans, Row row, FieldFilterFunc filter)
      {
        try
        {
            return crudUpsert(store, cnn, trans, row, filter);
        }
        catch(Exception error)
        {
           throw new MySQLDataAccessException(
                        StringConsts.CRUD_STATEMENT_EXECUTION_ERROR.Args("upsert", error.ToMessageWithType(), error),
                        error,
                        KeyViolationKind.Unspecified, 
                        keyViolationName(error));
        }
      }

      public static int CRUDDelete(MySQLDataStoreBase store, MySqlConnection cnn, MySqlTransaction trans, Row row, IDataStoreKey key)
      {
        try
        {
            return crudDelete(store, cnn, trans, row, key);
        }
        catch(Exception error)
        {
           throw new MySQLDataAccessException(StringConsts.CRUD_STATEMENT_EXECUTION_ERROR.Args("delete", error.ToMessageWithType(), error), error);
        }
      }


   #region .pvt impl.

    private static string keyViolationName(Exception error)
    {
      if (error==null) return null;
      var msg = error.Message;

      var i = msg.IndexOf("Duplicate", StringComparison.InvariantCultureIgnoreCase);
      if (i<0) return null;
      var j = msg.IndexOf("for key", StringComparison.InvariantCultureIgnoreCase);
      if (j>i && j<msg.Length)
       return msg.Substring(j);
      return null;
    }


    private static string getTableName(Schema schema, string target)
    {
      string tableName = schema.Name;

      if (schema.TypedRowType!=null)
        tableName = "tbl_" + schema.TypedRowType.Name;//without namespace

      var tableAttr = schema.GetTableAttrForTarget(target);
      if (tableAttr!=null && tableAttr.Name.IsNotNullOrWhiteSpace()) tableName = tableAttr.Name;
      return tableName;
    }


    private static int crudInsert(MySQLDataStoreBase store, MySqlConnection cnn, MySqlTransaction trans, Row row, FieldFilterFunc filter)
    {
      var target = store.TargetName;
      var cnames = new StringBuilder();
      var values = new StringBuilder();
      var vparams = new List<MySqlParameter>();
      var vpidx = 0;
      foreach (var fld in row.Schema.FieldDefs)
      {
        var fattr = fld[target];
        if (fattr==null) continue;

        if (fattr.StoreFlag != StoreFlag.LoadAndStore && fattr.StoreFlag != StoreFlag.OnlyStore) continue;
        
        if (filter!=null)//20160210 Dkh+SPol
        {
          if (!filter(row, null, fld)) continue;
        }

        var fname = fld.GetBackendNameForTarget(target);
         
        var fvalue = getFieldValue(row, fld.Order, store);
         
          
        cnames.AppendFormat(" `{0}`,", fname);

        if ( fvalue != null)
        {
                var pname = string.Format("?VAL{0}", vpidx);

                values.AppendFormat(" {0},", pname);

                var par = new MySqlParameter();
                par.ParameterName = pname;
                par.Value = fvalue;
                vparams.Add(par);

                vpidx++;
        }
        else
        {
                values.Append(" NULL,");
        }
      }//foreach

      if (cnames.Length > 0)
      {
        cnames.Remove(cnames.Length - 1, 1);// remove ","
        values.Remove(values.Length - 1, 1);// remove ","
      }
      else
        return 0;//nothing to do


      string tableName = getTableName(row.Schema, target);

      using(var cmd = cnn.CreateCommand())
      {   
        var sql = "INSERT INTO `{0}` ({1}) VALUES ({2})".Args( tableName, cnames, values);

        cmd.Transaction = trans;
        cmd.CommandText = sql;
        cmd.Parameters.AddRange(vparams.ToArray());
        ConvertParameters(store, cmd.Parameters);
        try
        {
            var affected = cmd.ExecuteNonQuery();
            GeneratorUtils.LogCommand(store.LogLevel, "insert-ok", cmd, null);
            return affected;
        }
        catch(Exception error)
        {
            GeneratorUtils.LogCommand(store.LogLevel, "insert-error", cmd, error);
            throw;
        }
      }//using command
    }




    private static int crudUpdate(MySQLDataStoreBase store, MySqlConnection cnn, MySqlTransaction trans, Row row, IDataStoreKey key, FieldFilterFunc filter)
    {
      var target = store.TargetName;
      var values = new StringBuilder();
      var vparams = new List<MySqlParameter>();
      var vpidx = 0;
      foreach (var fld in row.Schema.FieldDefs)
      {
        var fattr = fld[target];
        if (fattr==null) continue;

        var fname = fld.GetBackendNameForTarget(target);
        
        //20141008 DKh Skip update of key fields
        //20160124 DKh add update of keys if IDataStoreKey is present
        if (fattr.Key && !GeneratorUtils.HasFieldInNamedKey(fname, key)) continue;

        if (fattr.StoreFlag != StoreFlag.LoadAndStore && fattr.StoreFlag != StoreFlag.OnlyStore) continue;
        
        if (filter!=null)//20160210 Dkh+SPol
        {
          if (!filter(row, key, fld)) continue;
        }

         
        var fvalue = getFieldValue(row, fld.Order, store); 
        
         
        if ( fvalue != null)
        {
                var pname = string.Format("?VAL{0}", vpidx);
                
                values.AppendFormat(" `{0}` = {1},", fname, pname);

                var par = new MySqlParameter();
                par.ParameterName = pname;
                par.Value = fvalue;
                vparams.Add(par);

                vpidx++;
        }
        else
        {
                values.AppendFormat(" `{0}` = NULL,", fname);
        }
      }//foreach

      if (values.Length > 0)
      {
        values.Remove(values.Length - 1, 1);// remove ","
      }
      else
        return 0;//nothing to do


      string tableName = getTableName(row.Schema, target);

      using(var cmd = cnn.CreateCommand())
      {   
        var sql = string.Empty;
        
        var pk = key ?? row.GetDataStoreKey(target);

        if (pk == null)
            throw new MySQLDataAccessException(StringConsts.KEY_UNAVAILABLE_ERROR);

        var where = GeneratorUtils.KeyToWhere(pk, cmd.Parameters);

        if (!string.IsNullOrEmpty(where))
            sql = "UPDATE `{0}` T1  SET {1} WHERE {2}".Args( tableName, values, where);
        else
            throw new MySQLDataAccessException(StringConsts.BROAD_UPDATE_ERROR);//20141008 DKh BROAD update 

        cmd.Transaction = trans;
        cmd.CommandText = sql;
        cmd.Parameters.AddRange(vparams.ToArray());
        ConvertParameters(store, cmd.Parameters);     

        try
        {
            var affected = cmd.ExecuteNonQuery();
            GeneratorUtils.LogCommand(store.LogLevel, "update-ok", cmd, null);
            return affected;
        }
        catch(Exception error)
        {
            GeneratorUtils.LogCommand(store.LogLevel, "update-error", cmd, error);
            throw;
        }
      }//using command
    }


    private static int crudUpsert(MySQLDataStoreBase store, MySqlConnection cnn, MySqlTransaction trans, Row row, FieldFilterFunc filter)
    {
      var target = store.TargetName;
      var cnames = new StringBuilder();
      var values = new StringBuilder();
      var upserts = new StringBuilder();
      var vparams = new List<MySqlParameter>();
      var vpidx = 0;
      foreach (var fld in row.Schema.FieldDefs)
      {
        var fattr = fld[target];
        if (fattr==null) continue;

        if (fattr.StoreFlag != StoreFlag.LoadAndStore && fattr.StoreFlag != StoreFlag.OnlyStore) continue;
        

        if (filter!=null)//20160210 Dkh+SPol
        {
          if (!filter(row, null, fld)) continue;
        }

        var fname = fld.GetBackendNameForTarget(target);
         
        var fvalue = getFieldValue(row, fld.Order, store);
        
          
        cnames.AppendFormat(" `{0}`,", fname);

        if ( fvalue != null)
        {
                var pname = string.Format("?VAL{0}", vpidx);

                values.AppendFormat(" {0},", pname);
                
                if (!fattr.Key)
                    upserts.AppendFormat(" `{0}` = {1},", fname, pname);

                var par = new MySqlParameter();
                par.ParameterName = pname;
                par.Value = fvalue;
                vparams.Add(par);

                vpidx++;
        }
        else
        {
                values.Append(" NULL,");
                upserts.AppendFormat(" `{0}` = NULL,", fname);
        }
      }//foreach

      if (cnames.Length > 0 && upserts.Length > 0)
      {
        cnames.Remove(cnames.Length - 1, 1);// remove ","
        upserts.Remove(upserts.Length - 1, 1);// remove ","
        values.Remove(values.Length - 1, 1);// remove ","
      }
      else
        return 0;//nothing to do


      string tableName = getTableName(row.Schema, target);

      using(var cmd = cnn.CreateCommand())
      {   
        var sql = 
        @"INSERT INTO `{0}` ({1}) VALUES ({2}) ON DUPLICATE KEY UPDATE {3}".Args( tableName, cnames, values, upserts);

        cmd.Transaction = trans;
        cmd.CommandText = sql;
        cmd.Parameters.AddRange(vparams.ToArray());
        ConvertParameters(store, cmd.Parameters);

        try
        {
            var affected = cmd.ExecuteNonQuery();
            GeneratorUtils.LogCommand(store.LogLevel, "upsert-ok", cmd, null);
            return affected;
        }
        catch(Exception error)
        {
            GeneratorUtils.LogCommand(store.LogLevel, "upsert-error", cmd, error);
            throw;
        }
      }//using command
    }





    private static int crudDelete(MySQLDataStoreBase store, MySqlConnection cnn, MySqlTransaction trans, Row row, IDataStoreKey key)
    {
      var target = store.TargetName;
      string tableName = getTableName(row.Schema, target);

      using (var cmd = cnn.CreateCommand())
      {
        var pk = key ?? row.GetDataStoreKey(target);

        if (pk == null)
            throw new MySQLDataAccessException(StringConsts.KEY_UNAVAILABLE_ERROR);

        var where = GeneratorUtils.KeyToWhere(pk, cmd.Parameters);

        cmd.Transaction = trans;
        if (!string.IsNullOrEmpty(where))
            cmd.CommandText = string.Format("DELETE T1 FROM `{0}` T1 WHERE {1}",tableName, where);
        else
            cmd.CommandText = string.Format("DELETE T1 FROM `{0}` T1", tableName);

        ConvertParameters(store, cmd.Parameters);

        try
        {
            var affected = cmd.ExecuteNonQuery();
            GeneratorUtils.LogCommand(store.LogLevel, "delete-ok", cmd, null);
            return affected;
        }
        catch(Exception error)
        {
            GeneratorUtils.LogCommand(store.LogLevel, "delete-error", cmd, error);
            throw;
        }


      }//using command
    }

    private static object getFieldValue(Row row, int order, MySQLDataStoreBase store)
    {
      var result = row[order];

      MySqlDbType? convertedDbType;
      return CLRValueToDB(store, result, out convertedDbType);
    }

    internal static object CLRValueToDB(MySQLDataStoreBase store, object value, out MySqlDbType? convertedDbType)
    {
      convertedDbType = null;

      if (value==null) return null;

      if (value is GDID)
      {
        if (((GDID)value).IsZero)
        {
          return null;
        }
        
        if(store.FullGDIDS)
        {
          value = (object)((GDID)value).Bytes;
          convertedDbType = MySqlDbType.Binary;
        }
        else
        {
          value = (object)((GDID)value).ID;
          convertedDbType = MySqlDbType.Int64;
        }
      }
      else
      if (value is bool)
      {
        if (store.StringBool)
        {
          value = (bool)value ? store.StringForTrue : store.StringForFalse;
          convertedDbType = MySqlDbType.VarChar;
        }
      }

      return value;
    }

    internal static void ConvertParameters(MySQLDataStoreBase store, MySqlParameterCollection pars)
    {
      if (pars==null) return;
      for(var i=0; i<pars.Count; i++)
      {
        var par = pars[i];  
        MySqlDbType? convertedDbType;
        par.Value = CLRValueToDB(store, par.Value, out convertedDbType);
        if (convertedDbType.HasValue)
         par.MySqlDbType = convertedDbType.Value;
      }
    }


    #endregion


  }
}
