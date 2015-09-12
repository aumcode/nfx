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

using NFX.Log;
using NFX.Environment;
using NFX.Serialization.BSON;

namespace NFX.DataAccess.MongoDB
{
    /// <summary>
    /// Implements a store that sends log messages into MongoDB
    /// </summary>
    public class MongoDBLogMessageDataStore : MongoDBDataStoreBase
    {
        #region CONSTS

          public const string CONFIG_COLLECTION_NAME_DEFAULT = "nfx_log"; 

        #endregion
        
        
        #region .ctor/.dctor
     
          public MongoDBLogMessageDataStore()
          {      
          }

          public MongoDBLogMessageDataStore(string connectString, string dbName, string collectionName)
          {  
            ConnectString = connectString;    
            DatabaseName = dbName;
            CollectionName = collectionName;
          }
      
        #endregion

        #region PrivateFields

          private string m_CollectionName;

        #endregion

        #region Properties

          /// <summary>
          /// Gets/sets collection name used for logging
          /// </summary>
          [Config("$collection")]
          public string CollectionName
          {
            get { return m_CollectionName ?? CONFIG_COLLECTION_NAME_DEFAULT; }
            set { m_CollectionName = value; }
          }

        #endregion


        #region Public

           /// <summary>
           /// Inserts log message into MongoDB
           /// </summary>
           public void SendMessage(Message msg)
           {
              var db = GetDatabase();
              var col = db[CollectionName];
              col.Insert(docFromMessage(msg));
           }

        #endregion

          #region .pvt
           
                  private BSONDocument docFromMessage(Message msg)
                  {
                    var doc = new BSONDocument();

                    var rc = new RowConverter();

                    doc.Set(new BSONStringElement("Guid", msg.Guid.ToString("N")));
                    doc.Set(new BSONStringElement("RelatedTo", msg.RelatedTo.ToString("N")));
                    doc.Set(new BSONStringElement("Type", msg.Type.ToString()));
                    doc.Set(new BSONInt32Element("Source", msg.Source));
                    doc.Set(new BSONInt64Element("TimeStamp", msg.TimeStamp.Ticks));
                    doc.Set(new BSONStringElement("Host", msg.Host));
                    doc.Set(new BSONStringElement("From", msg.From));
                    doc.Set(new BSONStringElement("Topic", msg.Topic));
                    doc.Set(new BSONStringElement("Text", msg.Text));
                    doc.Set(new BSONStringElement("Parameters", msg.Parameters));
                    doc.Set(new BSONStringElement("Exception", msg.Exception.ToMessageWithType()));
                    doc.Set(new BSONInt32Element("ThreadID", msg.ThreadID));

                    return doc;
                  }

          #endregion

    }
}
