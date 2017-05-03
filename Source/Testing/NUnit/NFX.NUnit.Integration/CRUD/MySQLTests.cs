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



using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;

using NUnit.Framework;

using NFX;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.MySQL;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace NFX.NUnit.Integration.CRUD
{
  /// <summary>
  /// To perform tests below MySQL server instance is needed.
  /// Look at CONNECT_STRING constant
  /// </summary>
  [TestFixture]
  public class MySQLTests
  {


        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(500)]
        public void Parallel_Fiasco(int cnt)
        {
          var cstr = getConnectString();

          Console.WriteLine(cstr);

          Parallel.For(0, cnt, new ParallelOptions{ MaxDegreeOfParallelism=36}, (i) =>
          {
            using(var cnn = new MySqlConnection(cstr))
            {
             cnn.Open();
            // using(var tx = cnn.BeginTransaction())
               using(var cmd = cnn.CreateCommand())
               {
                 var a = i;
                 var b = i+980;
                 cmd.CommandText = "select SQL_NO_CACHE {0}+{1} as ZZZ, t.* from mysql.help_keyword t".Args(a, b);
               //  Thread.Sleep(NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(1,50));
                 using(var reader = cmd.ExecuteReader())
                 {
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual(a+b, reader[0]);
                    var rc = 0;
                    while(reader.Read()) rc++;
                    Console.WriteLine("Count: "+rc);
                 }

              //   tx.Commit();
               }
            }
          });
        }



        [Test]
        public void ManualDS_QueryInsertQuery()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.QueryInsertQuery( store );
            }
        }

        [Test]
        public void ManualDS_ASYNC_QueryInsertQuery()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.ASYNC_QueryInsertQuery( store );
            }
        }

        [Test]
        public void ManualDS_QueryInsertQuery_TypedRow()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.QueryInsertQuery_TypedRow( store );
            }
        }

        [Test]
        public void ManualDS_QueryInsertQuery_TypedRowDerived()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.QueryInsertQuery_TypedRowDerived( store );
            }
        }


        [Test]
        public void ManualDS_QueryInsertQuery_DynamicRow()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.QueryInsertQuery_DynamicRow( store );
            }
        }


        [Test]
        public void ManualDS_InsertManyUsingLogChanges_TypedRow()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.InsertManyUsingLogChanges_TypedRow( store );
            }
        }

        [Test]
        public void ManualDS_ASYNC_InsertManyUsingLogChanges_TypedRow()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.ASYNC_InsertManyUsingLogChanges_TypedRow( store );
            }
        }


        [Test]
        public void ManualDS_InsertInTransaction_Commit_TypedRow()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.InsertInTransaction_Commit_TypedRow( store );
            }
        }

        [Test]
        public void ManualDS_ASYNC_InsertInTransaction_Commit_TypedRow()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.ASYNC_InsertInTransaction_Commit_TypedRow( store );
            }
        }

        [Test]
        public void ManualDS_InsertInTransaction_Rollback_TypedRow()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.InsertInTransaction_Rollback_TypedRow( store );
            }
        }

        [Test]
        public void ManualDS_InsertThenUpdate_TypedRow()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.InsertThenUpdate_TypedRow( store );
            }
        }

        [Test]
        public void ManualDS_ASYNC_InsertThenUpdate_TypedRow()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.ASYNC_InsertThenUpdate_TypedRow( store );
            }
        }


        [Test]
        public void ManualDS_InsertThenDelete_TypedRow()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.InsertThenDelete_TypedRow( store );
            }
        }

        [Test]
        public void ManualDS_ASYNC_InsertThenDelete_TypedRow()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.ASYNC_InsertThenDelete_TypedRow( store );
            }
        }

        [Test]
        public void ManualDS_InsertThenUpsert_TypedRow()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.InsertThenUpsert_TypedRow( store );
            }
        }

        [Test]
        public void ManualDS_ASYNC_InsertThenUpsert_TypedRow()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.ASYNC_InsertThenUpsert_TypedRow( store );
            }
        }

        [Test]
        public void ManualDS_GetSchemaAndTestVariousTypes()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.StringBool = false;
                store.FullGDIDS = false;
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.GetSchemaAndTestVariousTypes( store );
            }
        }

        [Test]
        public void ManualDS_ASYNC_GetSchemaAndTestVariousTypes()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.StringBool = false;
                store.FullGDIDS = false;
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.ASYNC_GetSchemaAndTestVariousTypes( store );
            }
        }


        [Test]
        public void ManualDS_TypedRowTestVariousTypes()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.StringBool = false;
                store.FullGDIDS = false;
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.TypedRowTestVariousTypes( store );
            }
        }

        [Test]
        public void ManualDS_TypedRowTestVariousTypes_StrBool()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.StringBool = true; //<-------- NOTICE
                store.StringForTrue = "1";
                store.StringForFalse = "0";

                store.FullGDIDS = false;
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.TypedRowTestVariousTypes( store );
            }
        }



        [Test]
        public void ManualDS_TypedRowTest_FullGDID()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.StringBool = false;
                store.FullGDIDS = true;
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.TypedRowTest_FullGDID( store );
            }
        }


        [Test]
        public void ManualDS_GetSchemaAndTestFullGDID()
        {
            using(var store = new MySQLDataStore(getConnectString()))
            {
                store.StringBool = false;
                store.FullGDIDS = true;
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.GetSchemaAndTestFullGDID( store );
            }
        }

        [Test]
        public void ManualDS_InsertWithPredicate()
        {
            using (var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.InsertWithPredicate(store);
                clearAllTables();
            }
        }

        [Test]
        public void ManualDS_UpdateWithPredicate()
        {
            using (var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.InsertWithPredicate(store);
                clearAllTables();
            }
        }

        [Test]
        public void ManualDS_UpsertWithPredicate()
        {
            using (var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.UpsertWithPredicate(store);
                clearAllTables();
            }
        }


        [Test]
        public void ManualDS_Populate_OpenCursor()
        {
            using (var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.Populate_OpenCursor(store);
                clearAllTables();
            }
        }

        [Test]
        public void ManualDS_ASYNC_Populate_OpenCursor()
        {
            using (var store = new MySQLDataStore(getConnectString()))
            {
                store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
                clearAllTables();
                TestLogic.Populate_ASYNC_OpenCursor(store);
                clearAllTables();
            }
        }


        //===============================================================================================================================

        private const string CONNECT_STRING = "Server=localhost;Database=NFXTest;Uid=root;Pwd=thejake;";
       // private const string CONNECT_STRING = "Server=127.0.0.1;Database=NFXTest;Uid=root;Pwd=thejake;";

        private const string SCRIPT_ASM = "NFX.NUnit.Integration";


                                                private string getConnectString()  //todo read form ENV var in future
                                                {
                                                 return CONNECT_STRING;
                                                }

                                                private void clearAllTables()
                                                {
                                                    using(var cnn = new MySqlConnection(CONNECT_STRING))
                                                    {
                                                        cnn.Open();
                                                        using(var cmd = cnn.CreateCommand())
                                                        {
                                                          cmd.CommandText = "TRUNCATE TBL_TUPLE; TRUNCATE TBL_PATIENT; TRUNCATE TBL_DOCTOR; TRUNCATE TBL_TYPES; TRUNCATE TBL_FULLGDID;";
                                                          cmd.ExecuteNonQuery();
                                                        }
                                                    }

                                                }

  }
}
