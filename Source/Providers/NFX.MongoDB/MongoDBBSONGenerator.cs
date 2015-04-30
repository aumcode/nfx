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
using NFX.DataAccess.MongoDB;

using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Driver;


namespace NFX.RecordModel.DataAccess.MongoDB
{

  /// <summary>
  /// This helper class generates BSON documents and CRUD commands depending on model state (last posted operation , key, etc)
  /// </summary>
  public static class MongoDBBSONGenerator
  {
    #region Public

    
    
    public static BsonValue ToBsonValue(this object value)
    {
      return BsonValue.Create(value);
    }
    
    public static IMongoQuery ToMongoQuery(this NameValueDataStoreKey key)
    {
      //return new QueryDocument(key);

      var keys = new List<IMongoQuery>();
      foreach(var pair in key)
       keys.Add(Query.EQ(pair.Key, pair.Value.ToBsonValue()));

      return Query.And(keys);
    } 


    /// <summary>
    /// Inits record feld values from BSON document
    /// </summary>
    public static void LoadDataFromBSON(this Record rec, BsonDocument doc)
    {
      foreach(var fld in rec.Fields.Where(f => f.StoreFlag == StoreFlag.LoadAndStore || f.StoreFlag == StoreFlag.OnlyLoad))
      {
        BsonValue val;
        if (!doc.TryGetValue(fld.FieldName, out val)) continue;
        
        if (val is BsonDateTime)
         fld.ValueAsDateTime = val.ToUniversalTime();// .AsDateTime;
        else
         fld.ValueAsObject = val.AsBsonValue;//.RawValue;
      }
    }

    /// <summary>
    /// NFX Record -> BSON Update
    /// </summary>
    public static UpdateBuilder ToBSONUpdate(this Record rec)
    {
      var result = new UpdateBuilder();

      foreach(var fld in rec.Fields.Where(f => f.StoreFlag == StoreFlag.LoadAndStore || f.StoreFlag == StoreFlag.OnlyStore))
      {                              
         BsonValue bval = null;
         if (fld is DecimalField)
          bval =  BsonValue.Create(fld.ValueAsString);
         else
          bval =  BsonValue.Create(fld.ValueAsObject);   
        
         result.Set(fld.FieldName, bval);      
      }

      return result;
    }

    /// <summary>
    /// Auto generates select sql and params. If sqlStatement!=null then params are added to that statement
    /// </summary>
    public static void Load(MongoDatabase db, ModelBase instance, IDataStoreKey key, object[] extra)
    {
      var record = asSuitableRecordInstance(instance);

      key = key ?? record.DataStoreKey;
      
      var nvkey = key as NameValueDataStoreKey;

      if (nvkey==null)
       throw new MongoDBDataAccessException(string.Format(StringConsts.KEY_SUPPORT_ERROR));

     
      var col = db.GetCollection<BsonDocument>(record.TableName);     

      if (!col.Exists()) 
        throw new MongoDBDataAccessException(string.Format(StringConsts.COLLECTION_DOES_NOT_EXIST_ERROR, record.TableName));

      var query = nvkey.ToMongoQuery();

      var cursor = col.Find(query); 

      var dataBSON = cursor.FirstOrDefault();

      if (dataBSON==null)
        throw new MongoDBDataAccessException(string.Format(StringConsts.LOADING_ENTITY_NOT_FOUND_ERROR, key));

      record.LoadDataFromBSON(dataBSON);
    }

    public static void Save(MongoDatabase db, ModelBase instance, IDataStoreKey key, object[] extra)
    {
      var record = asSuitableRecordInstance(instance);
     
      key = key ?? record.DataStoreKey;
      
      var nvkey = key as NameValueDataStoreKey;

      if (key!=null && nvkey==null)
       throw new MongoDBDataAccessException(string.Format(StringConsts.KEY_SUPPORT_ERROR));

      if (nvkey==null)
      {
        nvkey = new NameValueDataStoreKey();
        foreach(var fld in record.Fields)
         if(fld.KeyField)
          nvkey.Add(fld.FieldName, fld.ValueAsObject);
      }

     
      var col = db.GetCollection<BsonDocument>(record.TableName);     

      if (!col.Exists()) 
        throw new MongoDBDataAccessException(string.Format(StringConsts.COLLECTION_DOES_NOT_EXIST_ERROR, record.TableName));

      
      
      var query = nvkey!=null ? nvkey.ToMongoQuery() : Query.Null;
      var update = record.ToBSONUpdate();
      col.Update(query, update, UpdateFlags.Upsert);              
    }



    public static void Delete(MongoDatabase db, ModelBase instance, IDataStoreKey key, object[] extra)
    {
      var record = asSuitableRecordInstance(instance);
     
      key = key ?? record.DataStoreKey;
      
      var nvkey = key as NameValueDataStoreKey;

      if (key!=null && nvkey==null)
       throw new MongoDBDataAccessException(string.Format(StringConsts.KEY_SUPPORT_ERROR));

      if (nvkey==null)
      {
        nvkey = new NameValueDataStoreKey();
        foreach(var fld in record.Fields)
         if(fld.KeyField)
          nvkey.Add(fld.FieldName, fld.ValueAsObject);
      }

     
      var col = db.GetCollection<BsonDocument>(record.TableName);     

      if (!col.Exists()) 
        throw new MongoDBDataAccessException(string.Format(StringConsts.COLLECTION_DOES_NOT_EXIST_ERROR, record.TableName));

      
      
      var query = nvkey!=null ? nvkey.ToMongoQuery() : Query.Null;
      var update = record.ToBSONUpdate();
      col.Remove(query);       
    }

    #endregion


   #region .pvt impl.

        private static Record asSuitableRecordInstance(ModelBase model)
        {
          var record = model as Record;

          if (record == null)
            throw new MongoDBDataAccessException(string.Format(StringConsts.MODEL_TYPE_NOT_RECORD_ERROR, model.GetType().Name));

          if (string.IsNullOrEmpty(record.TableName))
                throw new MongoDBDataAccessException(string.Format(StringConsts.RECORD_TABLE_NAME_ERROR, model.GetType().Name));
      
          return record;
        }

  #endregion


  }
}
