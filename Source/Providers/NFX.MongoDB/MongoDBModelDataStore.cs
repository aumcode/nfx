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
 * Originated: 2013.02.28
 * Revision: NFX 1.0  2013.02.28
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using NFX.Environment;
using NFX.DataAccess;
using NFX.DataAccess.MongoDB;

using MongoDB.Bson;
using MongoDB.Driver;

namespace NFX.RecordModel.DataAccess.MongoDB
{
  /// <summary>
  /// Implements MongoDB general data store that always generates BSON documents automatically regardless of model type.
  /// This class IS thread-safe load/save/delete operations
  /// </summary>
  public class MongoDBModelDataStore : MongoDBDataStoreBase, IModelDataStoreImplementation
  {

    #region .ctor/.dctor

      /// <summary>
      /// Allocates MongoDB general data store that always generates BSON documents automatically regardless of model type.
      /// This class IS thread-safe load/save/delete operations
      /// </summary>
      public MongoDBModelDataStore() : base()
      {
      }

      /// <summary>
      /// Allocates MongoDB general data store that always generates BSON documents automatically regardless of model type.
      /// This class IS thread-safe load/save/delete operations
      /// </summary>
      public MongoDBModelDataStore(string cString, string dbName) : base(cString, dbName)
      {
      }

    #endregion


    #region IModelDataStore Members

      public void Load(ModelBase instance, IDataStoreKey key, params object[] extra)
      {
        var db = GetDatabase();
        DoLoad(db, instance, key, extra);
      }

      public void Save(ModelBase instance, IDataStoreKey key, params object[] extra)
      {
        var db = GetDatabase();
        DoSave(db, instance, key, extra);
      }

      public void Delete(ModelBase instance, IDataStoreKey key, params object[] extra)
      {
        var db = GetDatabase();
        DoDelete(db, instance, key, extra);
      }

    #endregion

    #region Protected

        /// <summary>
        /// Performs model load from MongoDB server by generating appropriate BSON documents automatically
        /// </summary>
        protected virtual void DoLoad(MongoDatabase db, ModelBase instance, IDataStoreKey key, params object[] extra)
        {
          MongoDBBSONGenerator.Load(db, instance, key, extra);
        }

        /// <summary>
        /// Performs model save into MongoDB server by generating appropriate BSON documents automatically
        /// </summary>
        protected virtual void DoSave(MongoDatabase db, ModelBase instance, IDataStoreKey key, params object[] extra)
        {
          MongoDBBSONGenerator.Save(db, instance, key, extra);
        }

        /// <summary>
        /// Performs delete per model instance  from MongoDB server by generating appropriate BSON commands automatically
        /// </summary>
        protected virtual void DoDelete(MongoDatabase db, ModelBase instance, IDataStoreKey key, params object[] extra)
        {
          MongoDBBSONGenerator.Delete(db, instance, key, extra);
        }

    #endregion


  }
}
