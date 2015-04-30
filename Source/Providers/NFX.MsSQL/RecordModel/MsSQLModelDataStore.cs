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

using NFX.Environment;

using NFX.DataAccess;
using NFX.DataAccess.MsSQL;

namespace NFX.RecordModel.DataAccess.MsSQL
{
  /// <summary>
  /// Implements MsSQL general data store that always generates SQL statements automatically regardless of model type.
  /// This class IS thread-safe load/save/delete operations
  /// </summary>
  public class MsSQLModelDataStore : MsSQLDataStoreBase, IModelDataStoreImplementation
  {

    #region .ctor/.dctor
     
      /// <summary>
      /// Allocates MsSQL general data store that always generates SQL statements automatically regardless of model type.
      /// This class IS thread-safe load/save/delete operations
      /// </summary>
      public MsSQLModelDataStore()  : base()
      {      
      }

      /// <summary>
      /// Allocates MsSQL general data store that always generates SQL statements automatically regardless of model type.
      /// This class IS thread-safe load/save/delete operations
      /// </summary>
      public MsSQLModelDataStore(string connectString)  : base (connectString)
      {  
      }
      
    #endregion


    #region IModelDataStore Members

      public void Load(ModelBase instance, IDataStoreKey key, params object[] extra)
      {
        using(var cnn = GetConnection())
          DoLoad(cnn, instance, key, extra);
      }

      public void Save(ModelBase instance, IDataStoreKey key, params object[] extra)
      {
        using (var cnn = GetConnection())
          DoSave(cnn, instance, key, extra);
      }

      public void Delete(ModelBase instance, IDataStoreKey key, params object[] extra)
      {
        using (var cnn = GetConnection())
          DoDelete(cnn, instance, key, extra);
      }

    #endregion


    #region Protected

      /// <summary>
      /// Performs model load from SQL server by generating appropriate SQL statement automatically
      /// </summary>
      protected virtual void DoLoad(SqlConnection cnn, ModelBase instance, IDataStoreKey key, params object[] extra)
      {
         MsSQLStatementGenerator.Load(cnn, null, instance, key, extra);
      }

      /// <summary>
      /// Performs model save into SQL server by generating appropriate SQL statement automatically
      /// </summary>
      protected virtual void DoSave(SqlConnection cnn, ModelBase instance, IDataStoreKey key, params object[] extra)
      {
         MsSQLStatementGenerator.Save(cnn, instance, key, extra);
      }

      /// <summary>
      /// Performs delete per model instance  from SQL server by generating appropriate SQL statement automatically
      /// </summary>
      protected virtual void DoDelete(SqlConnection cnn, ModelBase instance, IDataStoreKey key, params object[] extra)
      {
         MsSQLStatementGenerator.Delete(cnn, instance, key, extra);
      }

    #endregion
  
  }
}
