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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 1.0  2011.01.31
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SqlClient;

using NFX.Environment;
using NFX.DataAccess.MsSQL;

using NFX.MsSQL;

namespace NFX.Log.Destinations
{
  /// <summary>
  /// Provides SQL Server storage log destination functionality
  /// </summary>
  public class SQLServerDestination : Destination
  {
      #region CONSTS
      
      public const string CONFIG_CONNECTSTRING_ATTR = "connect-string";
      public const string CONFIG_TABLENAME_ATTR = "table-name";
      
      #endregion

      #region .ctor
      public SQLServerDestination() : base(null)
      {
          
      }
       
      public SQLServerDestination(string name, string connectString, string tableName = null) : base(name)
      {
        m_ConnectString = connectString;
        m_TableName = tableName;
      }

      protected override void Destructor()
      {
        base.Destructor();
      }
      #endregion


      #region Pvt Fields
      private string m_ConnectString;
        
      private SqlConnection  m_Connection;
      private SqlCommand  m_Command;

      private string m_TableName;
      #endregion 
     
      #region Public
      
      /// <summary>
      /// Sql Connection String
      /// </summary>
      [Config("$" + CONFIG_CONNECTSTRING_ATTR)]
      public string ConnectString
      {
        get { return string.IsNullOrWhiteSpace(m_ConnectString) ? string.Empty : m_ConnectString; }
        set
        {
          if (m_ConnectString != value)
          {
            m_ConnectString = value;
            resetConnection();
          }
        }
      }

      [Config("$" + CONFIG_TABLENAME_ATTR)]
      public string TableName
      {
        get { return string.IsNullOrWhiteSpace( m_TableName) ? DEFAULT_TABLE_NAME : m_TableName; }
        set
        {
          if (m_TableName != value)
          {
            m_TableName = value;
            resetConnection();
          }
        }
      }

      public override void Open()
      {
        base.Open();
        if (TestOnStart) 
          testConnection();
      }
        
      public override void  Close()
      {
 	      base.Close();
        closeConnection();
      }
        
      #endregion

      #region Protected

      protected override void DoSend(Message msg)
      {
        if (m_Command==null)
          prepareCommand();
           
        DateTime ts = msg.TimeStamp;
        if (ts.ToBinary() == 0) ts = App.LocalizedTime;
                 
        m_Command.Parameters[MSG_GUID].Value = msg.Guid;
        m_Command.Parameters[RELATED_TO_GUID].Value = msg.RelatedTo;
        m_Command.Parameters[MSG_TYPE].Value = msg.Type;
        m_Command.Parameters[MSG_SOURCE].Value = msg.Source;
        m_Command.Parameters[LOCAL_TIME].Value = ts;
        m_Command.Parameters[MSG_HOST].Value = msg.Host;
        m_Command.Parameters[MSG_FROM].Value = msg.From;
        m_Command.Parameters[MSG_TOPIC].Value = msg.Topic;
        m_Command.Parameters[MSG_TEXT].Value = msg.Text;
        m_Command.Parameters[MSG_PARAMETERS].Value = msg.Parameters;
        m_Command.Parameters[MSG_EXCEPTION].Value = getExceptionStr( msg.Exception);
            
        m_Command.ExecuteNonQuery();
      }
     
      #endregion
      
      
      #region Private

      const string MSG_GUID = "MSG_GUID";
      const string RELATED_TO_GUID = "RELATED_TO_GUID";
      const string MSG_TYPE = "MSG_TYPE";
      const string MSG_SOURCE = "MSG_SOURCE";
      const string LOCAL_TIME = "LOCAL_TIME";            
      const string SYSTEM_TIME = "SYSTEM_TIME";
      const string MSG_HOST = "MSG_HOST";        
      const string MSG_FROM = "MSG_FROM";            
      const string MSG_TOPIC = "MSG_TOPIC";
      const string MSG_TEXT = "MSG_TEXT";
      const string MSG_PARAMETERS = "MSG_PARAMETERS";
      const string MSG_EXCEPTION = "MSG_EXCEPTION";
      const string DEFAULT_TABLE_NAME = "TBL_NFXLOG";
      const string SQL_TBL_EXISTS_PATTERN = "SELECT TOP 1  * FROM {0}";
      const string SQL_CREATE_TABLE_PATTERN = @"CREATE TABLE {0}
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
                                        );";

       
        private void prepareCommand()
        { //todo  SET SERVER OUTPUT OFF???
          string sql = string.Format(
            @"INSERT INTO {0}
              ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})
              VALUES
              ( @{1}, @{2}, @{3}, @{4}, @{5}, SYSDATETIME(), @{7}, @{8}, @{9}, @{10}, @{11}, @{12})",
                TableName,
                MSG_GUID, //1
                RELATED_TO_GUID,//2
                MSG_TYPE, //3
                MSG_SOURCE,//4
                LOCAL_TIME,//5
                SYSTEM_TIME,//6
                MSG_HOST,//7
                MSG_FROM,//8
                MSG_TOPIC,//9
                MSG_TEXT, //10
                MSG_PARAMETERS,//11
                MSG_EXCEPTION);//12

          testConnection();

          m_Connection = new SqlConnection(m_ConnectString);
          m_Command = new SqlCommand(sql, m_Connection);
            
          m_Command.Parameters.Add(MSG_GUID, System.Data.SqlDbType.UniqueIdentifier);
          m_Command.Parameters.Add(RELATED_TO_GUID, System.Data.SqlDbType.UniqueIdentifier);
          m_Command.Parameters.Add(MSG_TYPE, System.Data.SqlDbType.VarChar, Int32.MaxValue);
          m_Command.Parameters.Add(MSG_SOURCE, System.Data.SqlDbType.VarChar, Int32.MaxValue);
          m_Command.Parameters.Add(LOCAL_TIME, System.Data.SqlDbType.DateTime, Int32.MaxValue);
          m_Command.Parameters.Add(MSG_HOST, System.Data.SqlDbType.VarChar, Int32.MaxValue);
          m_Command.Parameters.Add(MSG_FROM, System.Data.SqlDbType.VarChar, Int32.MaxValue);
          m_Command.Parameters.Add(MSG_TOPIC, System.Data.SqlDbType.VarChar, Int32.MaxValue);
          m_Command.Parameters.Add(MSG_TEXT, System.Data.SqlDbType.VarChar, Int32.MaxValue);
          m_Command.Parameters.Add(MSG_PARAMETERS, System.Data.SqlDbType.VarChar, Int32.MaxValue);
          m_Command.Parameters.Add(MSG_EXCEPTION, System.Data.SqlDbType.VarChar, Int32.MaxValue);
           
          m_Connection.Open();
          m_Command.Prepare(); 
        }

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

              cmd.CommandText = SQL_TBL_EXISTS_PATTERN.Args(TableName);
              try
              {
                cmd.ExecuteReader().Dispose();
              }
              catch
              {
                cmd.CommandText = SQL_CREATE_TABLE_PATTERN.Args(TableName);
                cmd.ExecuteNonQuery();
              }
            }
          }
          catch (Exception error)
          {
            throw new NFXException(string.Format(StringConsts.CONNECTION_TEST_FAILED_ERROR, error.Message), error);
          }
        }
        
        private void closeConnection()
        {
          if (m_Command!=null)
          {
            m_Command.Dispose();
            m_Command = null;
          }
              
          if (m_Connection!=null)
          {
            m_Connection.Dispose();
            m_Connection = null;
          }
        }

        private void resetConnection()
        {
          closeConnection();
          testConnection();
        }

      private static string getExceptionStr(Exception ex)
      {
        if (ex != null)
          return ex.GetType().FullName + "::" + ex.Message;
        else
          return string.Empty;
      }
      
      #endregion
  
  }
}
