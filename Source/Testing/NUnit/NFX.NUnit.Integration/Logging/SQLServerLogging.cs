/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using NUnit.Framework;

using NFX.Log;
using NFX.Environment;
using NFX.Log.Destinations;
using LSVC = NFX.Log.LogService;

namespace NFX.NUnit.Integration.Logging
{
  /// <summary>
  /// To perform tests below SQL server instance is needed.
  /// Database is hardcoded in DB_NAME constant. 
  /// Table name is hardcoded in TBL_NFXLOG constant (is created and dropped automatically).
  /// Connection string is hardcoded in CONN_STR constant.
  /// </summary>
  [TestFixture]
  public class SQLServerLogging
  {
    #region CONSTS

    const string DEST_NAME = "SQL_LOGGER";
    public const string DB_NAME = "NFX_DB_TEST";
    public const string TBL_NFXLOG = "TBL_NFXLOG";
    public const string TBL_NFXLOG_CUSTOM = "TBL_NFX_LOG_CUSTOM";

    public readonly string CONN_STR = @"Server=localhost;Database={0};Trusted_Connection=True;".Args(DB_NAME);

    public readonly string SQL_DROP_TABLE_PATTERN = @"
      IF OBJECT_ID (N'{0}', N'U') IS NOT NULL
	      DROP TABLE {0}";

    public const string SQL_TABLE_EXISTS_PATTERN = @"SELECT OBJECT_ID (N'{0}', N'U')";

    public readonly string SQL_CREATE_TABLE_PATTERN = @"CREATE TABLE {0}
      (
          MSG_GUID              UNIQUEIDENTIFIER NOT NULL, 
          RELATED_TO_GUID       UNIQUEIDENTIFIER,
          MSG_TYPE              CHAR(15) NOT NULL,
          MSG_SOURCE            CHAR(15) NOT NULL,
          LOCAL_TIME            DATETIME NOT NULL,
          SYSTEM_TIME           DATETIME NOT NULL,
          MSG_HOST              VARCHAR(256) NOT NULL,
          MSG_FROM              VARCHAR(256) NOT NULL,
          MSG_TOPIC             VARCHAR(256) NOT NULL,
          MSG_TEXT              VARCHAR(1024) NOT NULL,
          MSG_PARAMETERS        XML,
          MSG_EXCEPTION         VARCHAR(1024) NOT NULL
      );".Args(TBL_NFXLOG);

    public readonly string SQL_SELECT_LOG_TABLE_PATTERN = @"SELECT * FROM {0}";

    #endregion


    #region SetUp/TearDown

        [TestFixtureSetUp]
        public void SetUp()
        {
          m_Conn = new SqlConnection(CONN_STR);
          m_Conn.Open();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
          m_Conn.Close();
        }
    #endregion


    #region Fields

        private SqlConnection m_Conn;

    #endregion


    #region Test methods

    [Test]
    public void TestDestination_Autocreation_DefaultTableName()
    {
      dropTable(TBL_NFXLOG);

      var logService = new LSVC(null);

      using (Scope.OnExit(() => dropTable(TBL_NFXLOG)))
      {
        logService.RegisterDestination(new SQLServerDestination(DEST_NAME, CONN_STR));

        logService.Start();
        logService.Write(new Message() { Text = "Msg 1" });
        logService.WaitForCompleteStop();

        Assert.IsTrue(tableExists(TBL_NFXLOG));
      }
    }

    [Test]
    public void TestDestination_Autocreation_CustomTableName()
    {
      dropTable(TBL_NFXLOG_CUSTOM);

      var logService = new LSVC(null);

      using (Scope.OnExit(() => dropTable(TBL_NFXLOG_CUSTOM)))
      {
        logService.RegisterDestination(new SQLServerDestination(DEST_NAME, CONN_STR, TBL_NFXLOG_CUSTOM));

        logService.Start();
        logService.Write(new Message() { Text = "Msg 1" });
        logService.WaitForCompleteStop();

        Assert.IsTrue(tableExists(TBL_NFXLOG_CUSTOM)); 
      }
    }

    [Test]
    public void TestDestination_Write()
    {
      createTable(TBL_NFXLOG);

      var logService = new LSVC(null);

      using (Scope.OnExit(() => dropTable(TBL_NFXLOG)))
      {
        logService.RegisterDestination(new SQLServerDestination(DEST_NAME, CONN_STR));

        logService.Start();
        for (int i = 0; i < 3; i++)
          logService.Write(new Message() { Text = "Msg " + i });
        logService.WaitForCompleteStop();

        Assert.AreEqual(3, getLogTableRecords(TBL_NFXLOG).Rows.Count);
      }
    }

    [Test]
    public void TestDestination_CreateByLaConfig()
    {
      string la = @"
        log
        {{
          destination
          {{
            type='NFX.Log.Destinations.SQLServerDestination, NFX'
            name='{0}'
            connect-string='{1}'
            table-name='{2}'
          }}
        }}".Args(DEST_NAME, CONN_STR, TBL_NFXLOG_CUSTOM);
      var cfg = LaconicConfiguration.CreateFromString(la);

      dropTable(TBL_NFXLOG_CUSTOM);

      var logService = new LSVC(null);
      using (Scope.OnExit(() => dropTable(TBL_NFXLOG_CUSTOM)))
      {
        logService.Configure(cfg.Root);

        logService.Start();

        logService.Write(new Message() { Text = "Msg 1" });
        logService.Write(new Message() { Text = "Msg 2" });
        logService.Write(new Message() { Text = "Msg 3" });

        logService.WaitForCompleteStop();

        DataTable tbl = getLogTableRecords(TBL_NFXLOG_CUSTOM);
        Assert.AreEqual(3, tbl.Rows.Count); 
      }
    }

    #endregion

    #region .pvt / Helpers

    private DataTable getLogTableRecords(string tableName)
    {
      DataTable tbl = new DataTable();

      using (SqlDataAdapter a = new SqlDataAdapter(SQL_SELECT_LOG_TABLE_PATTERN.Args(tableName), m_Conn))
      {
        a.Fill(tbl);
      }

      return tbl;
    }

    private void dropTable(string tableName)
    {
      using (SqlCommand cmd = new SqlCommand(SQL_DROP_TABLE_PATTERN.Args(tableName), m_Conn))
      {
        cmd.ExecuteNonQuery();
      }
    }

    /// <summary>
    /// Methos is safe if required tabe already exists.
    /// </summary>
    private void createTable(string tableName)
    {
      string initSql = SQL_DROP_TABLE_PATTERN.Args(tableName) + "\r\n\r\n" + SQL_CREATE_TABLE_PATTERN.Args(tableName);
      using (SqlCommand cmd = new SqlCommand(initSql))
      {
        cmd.Connection = m_Conn;
        cmd.ExecuteNonQuery();
      }
    }

    private bool tableExists(string tableName)
    {
      using (SqlCommand cmd = new SqlCommand(SQL_TABLE_EXISTS_PATTERN.Args(tableName), m_Conn))
      {
        var exists = cmd.ExecuteScalar();
        return !Convert.IsDBNull(exists);
      }
    }

    #endregion
    
  }
}
