/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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

using NFX.ApplicationModel;
using NFX.Environment;

using NFX.MsSQL;

namespace NFX.DataAccess.MsSQL
{
  /// <summary>
  /// Implements Ms SQL server store base functionality
  /// </summary>
  public abstract class MsSQLDataStoreBase : ApplicationComponent, IDataStore
  {
    #region CONSTS
      public const string CONFIG_CONNECTSTRING_ATTR = "connect-string";
    #endregion


    #region .ctor/.dctor
     
      protected MsSQLDataStoreBase()
      {      
      }

      protected MsSQLDataStoreBase(string connectString)
      {  
        ConnectString = connectString;    
      }
      
    #endregion


    #region Private Fields
      
      private string m_ConnectString;  
      private string m_TargetName; 
      
      private bool m_InstrumentationEnabled;  
    #endregion

    #region IInstrumentation

      public string Name { get{return GetType().FullName;}}

      [Config(Default=false)]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
      public bool InstrumentationEnabled{ get{return m_InstrumentationEnabled;} set{m_InstrumentationEnabled = value;}}

      /// <summary>
      /// Returns named parameters that can be used to control this component
      /// </summary>
      public IEnumerable<KeyValuePair<string, Type>> ExternalParameters{ get { return ExternalParameterAttribute.GetParameters(this); } }

      /// <summary>
      /// Returns named parameters that can be used to control this component
      /// </summary>
      public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
      { 
        return ExternalParameterAttribute.GetParameters(this, groups); 
      }

      /// <summary>
      /// Gets external parameter value returning true if parameter was found
      /// </summary>
      public bool ExternalGetParameter(string name, out object value, params string[] groups)
      {
          return ExternalParameterAttribute.GetParameter(this, name, out value, groups);
      }
          
      /// <summary>
      /// Sets external parameter value returning true if parameter was found and set
      /// </summary>
      public bool ExternalSetParameter(string name, object value, params string[] groups)
      {
        return ExternalParameterAttribute.SetParameter(this, name, value, groups);
      }

    #endregion


    #region Properties
    
      /// <summary>
      /// Get/Sets MsSql database connection string
      /// </summary>
      public string ConnectString
      {
        get
        {
           return m_ConnectString ?? string.Empty;  
        }
        set
        {
             if (m_ConnectString != value)
             {
               m_ConnectString = value;
             }
        }
      }

      [Config]
      public StoreLogLevel LogLevel { get; set;}
      
      [Config]
      public string TargetName
      {
         get{ return m_TargetName.IsNullOrWhiteSpace() ? "MsSQL" : m_TargetName;}
         set{ m_TargetName = value;}
      }
    
    #endregion


    #region Public

       public void TestConnection()
       {
         try
         {
           using(var cnn = GetConnection())
           {
             var cmd = cnn.CreateCommand();
             cmd.CommandType = System.Data.CommandType.Text;
             cmd.CommandText = "SELECT 1+1";
             if (cmd.ExecuteScalar().ToString() != "2")
               throw new MsSQLDataAccessException(StringConsts.SQL_STATEMENT_FAILED_ERROR);
           }
         }
         catch(Exception error)
         {
           throw new MsSQLDataAccessException(string.Format(StringConsts.CONNECTION_TEST_FAILED_ERROR, error.Message), error);
         }
       }


    #endregion



    #region IConfigurable Members

      public void Configure(IConfigSectionNode node)
      {
        ConnectString = node.AttrByName(CONFIG_CONNECTSTRING_ATTR).Value;
      }

    #endregion
    
    #region Protected
      
      /// <summary>
      /// Allocates Ms SQL Server connection
      /// </summary>
      protected SqlConnection GetConnection()
      {
        var cnn = new SqlConnection(this.ConnectString);
        cnn.Open();
        return cnn;
      }

    #endregion


    #region .pvt impl.
      
    #endregion
  
  }
}
