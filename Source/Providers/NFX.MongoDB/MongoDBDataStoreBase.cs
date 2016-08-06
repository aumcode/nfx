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
using NFX.DataAccess.CRUD;
using NFX.Environment;
using NFX.ApplicationModel;
using NFX.DataAccess.MongoDB.Connector;

namespace NFX.DataAccess.MongoDB
{
  /// <summary>
  /// Implements MongoDB store base functionality
  /// Connect string takes form of:
  /// <code>
  ///  mongodb://[username:password@]hostname[:port][/[database][?options]]
  /// </code>
  /// </summary>
  public abstract class MongoDBDataStoreBase : ApplicationComponent, IDataStoreImplementation
  {
    #region CONST



    #endregion

    #region .ctor/.dctor

      protected MongoDBDataStoreBase():base()
      {
      }

      protected MongoDBDataStoreBase(string connectString, string dbName):base()
      {
        ConnectString = connectString;
        DatabaseName = dbName;
      }

    #endregion


    #region Private Fields

      private string m_ConnectString;
      private string m_DatabaseName;

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
      /// Get/Sets MongoDB database connection string
      /// </summary>
      [Config("$connect-string")]
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

      /// <summary>
      /// Get/Sets MongoDB database name
      /// </summary>
      [Config("$db-name")]
      public string DatabaseName
      {
        get
        {
           return m_DatabaseName ?? string.Empty;
        }
        set
        {
             if (m_DatabaseName != value)
             {
               m_DatabaseName = value;
             }
        }
      }

      [Config]
      public StoreLogLevel LogLevel { get; set;}

      [Config]
      public string TargetName
      {
         get{ return m_TargetName.IsNullOrWhiteSpace() ? "MongoDB" : m_TargetName;}
         set{ m_TargetName = value;}
      }

    #endregion

    #region Public
      public void TestConnection()
      {
        if (string.IsNullOrEmpty(m_ConnectString) || string.IsNullOrEmpty(m_DatabaseName)) return;

        try
        {
          var db = GetDatabase();
          db.Ping();
        }
        catch (Exception error)
        {
          throw new MongoDBDataAccessException(StringConsts.CONNECTION_TEST_FAILED_ERROR.Args(error.ToMessageWithType()), error);
        }
      }

    #endregion

    #region IConfigurable Members

      public virtual void Configure(IConfigSectionNode node)
      {
        ConfigAttribute.Apply(this, node);
      }

    #endregion

    #region Protected

      /// <summary>
      /// Gets appropriate database. It does not need to be disposed
      /// </summary>
      protected Database GetDatabase()
      {
        //cstring may either have server cs, or contain laconic config  mongo{server='' database=''}
        var cstring = m_ConnectString;
        var dbn = m_DatabaseName;

        //Try to override from the context
        var ctx = CRUDOperationCallContext.Current;
        if (ctx!=null)
        {
          if (ctx.ConnectString.IsNotNullOrWhiteSpace()) cstring = ctx.ConnectString;
          if (ctx.DatabaseName.IsNotNullOrWhiteSpace()) dbn = ctx.DatabaseName;
        }

        //2. try to parse as laconic
        var lactxt = cstring;
        if (lactxt!=null)
        {
          lactxt = lactxt.Trim();
          if (lactxt.StartsWith("mongo{"))//quick reject filter
          {
            var root = lactxt.AsLaconicConfig();
            if (root!=null)
            {
              cstring = root.AttrByName("server").Value;
              if (dbn.IsNullOrWhiteSpace())
                dbn = root.AttrByName("db").Value;
            }
          }
        }

        var server = MongoClient.Instance[ new NFX.Glue.Node(cstring)];
        var database = server[dbn];
        return database;
      }


    #endregion
  }
}
