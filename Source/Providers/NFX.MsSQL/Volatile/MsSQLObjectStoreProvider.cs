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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 0.8  2010.09.20
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using NFX.Log;
using NFX.Environment;
using NFX.ServiceModel;

using NFX.MsSQL;

namespace NFX.ApplicationModel.Volatile
{
  /// <summary>
  /// Provider that stores objects in Microsoft SQL Server database
  /// </summary>
  public class MsSQLObjectStoreProvider : ObjectStoreProvider
  {
    #region CONSTS

    public const string CONFIG_CONNECT_STRING_ATTR = "connect-string";

    public const string CONFIG_LOAD_LIMIT_ATTR = "load-limit";
    public const string CONFIG_PURGE_AFTER_DAYS_ATTR = "purge-after-days";

    public const long MAX_LOAD_LIMIT = 1/*gb*/ * 1024/*mb*/ * 1024/*k bytes */ * 1024/*bytes*/;
    public const long DEFAULT_LOAD_LIMIT = 128/*mb*/ * 1024/*k bytes */ * 1024/*bytes*/;

    public const int MIN_PURGE_AFTER_DAYS = 1;
    public const int DEFAULT_PURGE_AFTER_DAYS = 5;

    public const string TBL_EXISTS_SQL = "SELECT TOP 1  * FROM _tbl_ObjectStoreProvider";
 
    public const string TABLE_CREATE_SQL =
@"
 CREATE TABLE _tbl_ObjectStoreProvider
 (
    STORE_ID        UNIQUEIDENTIFIER    NOT NULL,
    OBJECT_ID       UNIQUEIDENTIFIER    NOT NULL,
    TIME_STAMP      DATETIME            NOT NULL,
    OBJECT_CONTENT  VARBINARY(max),
    PRIMARY KEY CLUSTERED(STORE_ID, OBJECT_ID)
 )";

    public const string SELECT_SQL =
@"
 SELECT * FROM _tbl_ObjectStoreProvider
 WHERE
  STORE_ID = @STORE_ID
 ORDER BY
  TIME_STAMP DESC
";


    public const string INSERT_SQL =
@"
 DELETE FROM _tbl_ObjectStoreProvider
 WHERE
  (STORE_ID = @STORE_ID)  AND (OBJECT_ID = @OBJECT_ID);
 
 INSERT INTO _tbl_MsSQLObjectStoreProvider
  (STORE_ID, OBJECT_ID, TIME_STAMP, OBJECT_CONTENT)
 VALUES
  (@STORE_ID, @OBJECT_ID, GETDATE(), @CONTENT);
";

    public const string DELETE_SQL =
@"
 DELETE FROM _tbl_ObjectStoreProvider
 WHERE 
   (STORE_ID = @STORE_ID) AND
   (
     (OBJECT_ID = @OBJECT_ID) OR (TIME_STAMP < DATEADD(day,- @PURGE_AFTER_DAYS, GETDATE()))
   )
";


    #endregion


    #region .ctor
      public MsSQLObjectStoreProvider() : base(null)
      {

      }

      public MsSQLObjectStoreProvider(ObjectStoreService director) : base(director)
      {

      }
    #endregion

    #region Private Fields

      private long m_LoadLimit;
      private string m_ConnectString;

      private long m_LoadSize;

      private int m_PurgeAfterDays;

    #endregion


    #region Properties

    /// <summary>
    /// Imposes the limit on number of bytes that can be read from database on load all.
    /// Once limit is exceeded the rest of objects will not load.
    /// Provider loads most recent objects first
    /// </summary>
    public long LoadLimit
    {
      get { return m_LoadLimit; }
      set
      {
        if (value > MAX_LOAD_LIMIT)
          value = MAX_LOAD_LIMIT;
        m_LoadLimit = value;
      }
    }

    /// <summary>
    /// Specifies after how many days objects get deleted from SQL database
    /// </summary>
    public int PurgeAfterDays
    {
      get { return m_PurgeAfterDays; }
      set
      {
        if (value < MIN_PURGE_AFTER_DAYS)
          value = MIN_PURGE_AFTER_DAYS;
        m_PurgeAfterDays = value;
      }
    }

    /// <summary>
    /// Returns how many bytes have been loaded from the database
    /// </summary>
    public long LoadSize
    {
      get { return m_LoadSize; }
    }



    /// <summary>
    /// Gets/sets the connect string for database where objecs are stored
    /// </summary>
    public string ConnectString
    {
      get { return m_ConnectString ?? string.Empty; }
      set
      {
        CheckServiceInactive();

        m_ConnectString = value;

        testConnection();
      }
    }

    #endregion



    #region Public

        public override IEnumerable<ObjectStoreEntry> LoadAll()
        {
          var now = App.LocalizedTime;

          using (var cnn = getConnection())
          using (var cmd = cnn.CreateCommand())
          {
            cmd.CommandText = SELECT_SQL;
            cmd.Parameters.Add(new SqlParameter() { ParameterName = "@STORE_ID", Value = ComponentDirector.StoreGUID });
            using (var reader = cmd.ExecuteReader())
              while (reader.Read())
              {
                if (m_LoadLimit > 0 && m_LoadSize > m_LoadLimit)
                {
                  WriteLog(MessageType.Info, "Load limit imposed", m_LoadLimit.ToString() + " bytes");
                  yield break;
                }


                Guid oid = (Guid)reader["OBJECT_ID"];
                byte[] buf = reader["OBJECT_CONTENT"] as byte[];
                BinaryFormatter formatter = new BinaryFormatter();
                var entry = new ObjectStoreEntry();

                try
                {
                  entry.Value = formatter.Deserialize(new MemoryStream(buf));
                }
                catch (Exception error)
                {
                  WriteLog(MessageType.Error, "Deserialization error: " + error.Message, oid.ToString());
                  continue;
                }
                m_LoadSize += buf.Length;

                entry.Key = oid;
                entry.LastTime = now;
                entry.Status = ObjectStoreEntryStatus.Normal;

                yield return entry;

              }//while reader.read()
          }
        }


        public override void Write(ObjectStoreEntry entry)
        {
          using (var ms = new MemoryStream())
          {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, entry.Value);

            using (var cnn = getConnection())
              using (var cmd = cnn.CreateCommand())
              {
                cmd.CommandText = INSERT_SQL;
                cmd.Parameters.Add(new SqlParameter() { ParameterName = "@STORE_ID", Value = ComponentDirector.StoreGUID });
                cmd.Parameters.Add(new SqlParameter() { ParameterName = "@OBJECT_ID", Value = entry.Key });

                cmd.Parameters.Add(new SqlParameter()
                {
                  ParameterName = "@CONTENT",
                  SqlDbType = SqlDbType.VarBinary,
                  Value = ms.GetBuffer()
                }
                                  );

                cmd.ExecuteNonQuery();
              }
          }
        }

        public override void Delete(ObjectStoreEntry entry)
        {
          using (var cnn = getConnection())
            using (var cmd = cnn.CreateCommand())
            {
              cmd.CommandText = DELETE_SQL;
              cmd.Parameters.Add(new SqlParameter() { ParameterName = "@STORE_ID", Value = ComponentDirector.StoreGUID });
              cmd.Parameters.Add(new SqlParameter() { ParameterName = "@OBJECT_ID", Value = entry.Key });
              cmd.Parameters.Add(new SqlParameter() { ParameterName = "@PURGE_AFTER_DAYS", Value = m_PurgeAfterDays });
              cmd.ExecuteNonQuery();
            }
        }


    #endregion


    #region Protected

        protected override void DoConfigure(IConfigSectionNode node)
        {
          base.DoConfigure(node);
          LoadLimit = node.AttrByName(CONFIG_LOAD_LIMIT_ATTR).ValueAsLong(DEFAULT_LOAD_LIMIT);
          ConnectString = node.AttrByName(CONFIG_CONNECT_STRING_ATTR).ValueAsString();
          PurgeAfterDays = node.AttrByName(CONFIG_PURGE_AFTER_DAYS_ATTR).ValueAsInt(DEFAULT_PURGE_AFTER_DAYS);
        }


        protected override void DoStart()
        {
          testConnection();

          base.DoStart();

          m_LoadSize = 0;
        }



    #endregion


    #region .pvt .utils

        private SqlConnection getConnection()
        {
          var cnn = new SqlConnection(this.ConnectString);
          cnn.Open();
          return cnn;
        }


        private void testConnection()
        {
          try
          {
            using (var cnn = getConnection())
            {
              var cmd = cnn.CreateCommand();
              cmd.CommandType = System.Data.CommandType.Text;
              cmd.CommandText = "SELECT 1+1";
              if (cmd.ExecuteScalar().ToString() != "2")
                throw new NFXException(StringConsts.SQL_STATEMENT_FAILED_ERROR);

              cmd.CommandText = TBL_EXISTS_SQL;
              try
              {
                cmd.ExecuteReader().Dispose();
              }
              catch
              {
                cmd.CommandText = TABLE_CREATE_SQL;
                cmd.ExecuteNonQuery();
              }

            }
          }
          catch (Exception error)
          {
            throw new NFXException(string.Format(StringConsts.CONNECTION_TEST_FAILED_ERROR, error.Message), error);
          }
        }

    #endregion


  }


}
