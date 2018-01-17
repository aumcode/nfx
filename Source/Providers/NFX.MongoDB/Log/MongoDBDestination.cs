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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.DataAccess.MongoDB;
using NFX.Environment;


namespace NFX.Log.Destinations
{
    /// <summary>
    /// Implements destination that sends log messages into MongoDB
    /// </summary>
    public class MongoDBDestination : Destination
    {
        
             /// <summary>
            /// Creates a new instance of destination that stores log MongoDB
            /// </summary>
            public MongoDBDestination() : base(null)
            {
            }
       
            /// <summary>
            /// Creates a new instance of destination that stores log MongoDB
            /// </summary>
            public MongoDBDestination(string name, string connectString, string dbName, string collectionName = null) : base(name)
            {
              m_DataStore.ConnectString = connectString;
              m_DataStore.DatabaseName = dbName;
              m_DataStore.CollectionName = collectionName;
            }
        
        
        
        private MongoDBLogMessageDataStore m_DataStore = new MongoDBLogMessageDataStore();


        /// <summary>
        /// Refrences an underlying data store
        /// </summary>
        public MongoDBLogMessageDataStore DataStore
        {
          get { return m_DataStore; }
        }


        public override void Open()
        {
            base.Open();
            if (TestOnStart) m_DataStore.TestConnection();
        }



        protected override void DoConfigure(Environment.IConfigSectionNode node)
        {
            base.DoConfigure(node);
            ConfigAttribute.Apply(m_DataStore, node);
        }

        protected override void DoSend(Message entry)
        {
          m_DataStore.SendMessage(entry);  
        }
    }
}
