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

using NUnit.Framework;

using NFX;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;

namespace NFX.NUnit.Integration.CRUD
{
    internal static class TestLogic
    {
        public static void QueryInsertQuery(ICRUDDataStore store)
        {
            var query = new Query("CRUD.Patient.List") { new Query.Param("LN", "%loff") };
            var result = store.Load( query );  

            Assert.AreEqual(1, result.Count);
            var rowset = result[0];
            Assert.AreEqual(0, rowset.Count);
                
            var row = new DynamicRow(rowset.Schema);

            row["ssn"] = "999-88-9012";
            row["First_Name"] = "Jack";
            row["Last_Name"] = "Kozloff";
            row["DOB"] = new DateTime(1980, 1, 12);

            Assert.IsNull( row.Validate());

            store.Insert(row);   

                      
            result = store.Load( query );  

            Assert.AreEqual(1, result.Count);
            rowset = result[0];

            Assert.AreEqual(1, rowset.Count);
            Assert.AreEqual("Jack", rowset[0]["First_Name"]);
            
        }

        public static void QueryInsertQuery_TypedRow(ICRUDDataStore store)
        {
            var query = new Query("CRUD.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff") };
            var result = store.Load( query );  

            Assert.AreEqual(1, result.Count);
            var rowset = result[0];
            Assert.AreEqual(0, rowset.Count);
                
            var row = new Patient();

            row.SSN = "999-88-9012";
            row.First_Name = "Jack";
            row.Last_Name = "Kozloff";
            row.DOB = new DateTime(1980, 1, 12);

            Assert.IsNull( row.Validate());

            store.Insert(row);   

                      
            result = store.Load( query );  

            Assert.AreEqual(1, result.Count);
            rowset = result[0];

            Assert.AreEqual(1, rowset.Count);
            Assert.IsInstanceOf<Patient>( rowset[0] );
            Assert.AreEqual("Jack", rowset[0]["First_Name"]);
            
        }

        public static void QueryInsertQuery_TypedRowDerived(ICRUDDataStore store)
        {
            var query = new Query("CRUD.Patient.List", typeof(SuperPatient) ) { new Query.Param("LN", "%loff") };
            var result = store.Load( query );  

            Assert.AreEqual(1, result.Count);
            var rowset = result[0];
            Assert.AreEqual(0, rowset.Count);
                
            var row = new SuperPatient();

            row.SSN = "999-88-9012";
            row.First_Name = "Jack";
            row.Last_Name = "Kozloff";
            row.DOB = new DateTime(1980, 1, 12);
            row.Superman = true;

            Assert.IsNull( row.Validate());

            store.Insert(row);   

                      
            result = store.Load( query );  

            Assert.AreEqual(1, result.Count);
            rowset = result[0];

            Assert.AreEqual(1, rowset.Count);
            Assert.IsInstanceOf<SuperPatient>( rowset[0] );
            Assert.AreEqual("Jack", rowset[0]["First_Name"]);
            
        }


        public static void InsertManyUsingLogChanges_TypedRow(ICRUDDataStore store)
        {
            var rowset = new Rowset( Schema.GetForTypedRow(typeof(Patient)));
            rowset.LogChanges = true;
       
            for(var i=0; i<1000; i++)
            {
                rowset.Insert( new Patient
                               {
                                 SSN = "999-88-9012",
                                 First_Name = "Jack",
                                 Last_Name = "Kozloff"+i,
                                 DOB = new DateTime(1980, 1, 12)
                               });
            }
            
            for(var i=0; i<327; i++)
            {
                rowset.Insert( new Patient 
                               {
                                 SSN = "999-88-9012",
                                 First_Name = "Jack",
                                 Last_Name = "Abramovich"+i,
                                 DOB = new DateTime(2001, 1, 12)
                               });
            }

            store.Save( rowset );

            var result = store.Load( new Query("CRUD.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff%") } )[0];
            
            Assert.AreEqual(1000, result.Count);

            result = store.Load( new Query("CRUD.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%ovich%") } )[0];
            
            Assert.AreEqual(327, result.Count);
        }


        public static void InsertInTransaction_Commit_TypedRow(ICRUDDataStore store)
        {
            var transaction = store.BeginTransaction();

            for(var i=0; i<25; i++)
            {
                transaction.Insert( new Patient
                               {
                                 SSN = "999-88-9012",
                                 First_Name = "Jack",
                                 Last_Name = "Kozloff"+i,
                                 DOB = new DateTime(1980, 1, 12)
                               });
            }

            
            var result = store.Load( new Query("CRUD.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff%") } )[0];
            
            Assert.AreEqual(0, result.Count);

            transaction.Commit();

            result = store.Load( new Query("CRUD.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff%") } )[0];
            
            Assert.AreEqual(25, result.Count);
        }

        public static void InsertInTransaction_Rollback_TypedRow(ICRUDDataStore store)
        {
            var transaction = store.BeginTransaction();

            for(var i=0; i<25; i++)
            {
                transaction.Insert( new Patient
                               {
                                 SSN = "999-88-9012",
                                 First_Name = "Jack",
                                 Last_Name = "Kozloff"+i,
                                 DOB = new DateTime(1980, 1, 12)
                               });
            }

            
            var result = store.Load( new Query("CRUD.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff%") } )[0];
            
            Assert.AreEqual(0, result.Count);

            transaction.Rollback();

            result = store.Load( new Query("CRUD.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff%") } )[0];
            
            Assert.AreEqual(0, result.Count);
        }


        public static void InsertThenUpdate_TypedRow(ICRUDDataStore store)
        {
            for(var i=0; i<10; i++)
            {
                store.Insert( new Patient
                               {
                                 SSN = i.ToString(),
                                 First_Name = "Jack",
                                 Last_Name = "Kozloff_"+i,
                                 DOB = new DateTime(1980, 1, 12)
                               });
            }

            
            var result = store.Load( new Query("CRUD.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_5") } )[0];
            
            Assert.AreEqual(1, result.Count);
            var row = result[0] as Patient;
            Assert.AreEqual("5", row.SSN);

            row.Last_Name = "Gagarin";
            store.Update( row );

            result = store.Load( new Query("CRUD.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_5") } )[0];
            Assert.AreEqual(0, result.Count);

            result = store.Load( new Query("CRUD.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%garin") } )[0];
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("5", result[0]["SSN"]);
            Assert.AreEqual("Gagarin", result[0]["Last_Name"]);


        }

        public static void InsertThenDelete_TypedRow(ICRUDDataStore store)
        {
            for(var i=0; i<10; i++)
            {
                store.Insert( new Patient
                               {
                                 SSN = i.ToString(),
                                 First_Name = "Jack",
                                 Last_Name = "Kozloff_"+i,
                                 DOB = new DateTime(1980, 1, 12)
                               });
            }

            
            var result = store.Load( new Query("CRUD.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_5") } )[0];
            
            Assert.AreEqual(1, result.Count);
            var row = result[0] as Patient;
            Assert.AreEqual("5", row.SSN);

            store.Delete( row );

            result = store.Load( new Query("CRUD.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_5") } )[0];
            Assert.AreEqual(0, result.Count);

            result = store.Load( new Query("CRUD.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_%") } )[0];
            Assert.AreEqual(9, result.Count);

        }


        public static void InsertThenUpsert_TypedRow(ICRUDDataStore store)
        {
            for(var i=0; i<10; i++)
            {
                store.Insert( new Patient
                               {
                                 SSN = i.ToString(),
                                 First_Name = "Jack",
                                 Last_Name = "Kozloff_"+i,
                                 DOB = new DateTime(1980, 1, 12)
                               });
            }

            
            var result = store.Load( new Query("CRUD.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_5") } )[0];
            
            Assert.AreEqual(1, result.Count);
            var row = result[0] as Patient;
            Assert.AreEqual("5", row.SSN);
            Assert.AreEqual(null, row.Phone);

            row.Phone = "22-94-92";
            store.Upsert( row );
            
            result = store.Load( new Query("CRUD.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_5") } )[0];
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("22-94-92", result[0]["Phone"]);

            result = store.Load( new Query("CRUD.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_%") } )[0];
            Assert.AreEqual(10, result.Count);

            row = new Patient
                               {
                                 SSN = "-100",
                                 First_Name = "Vlad",
                                 Last_Name = "Lenin",
                                 DOB = new DateTime(1871, 4, 20)
                               };

            store.Upsert(row);

            result = store.Load( new Query("CRUD.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%") } )[0];
            Assert.AreEqual(11, result.Count);

            result = store.Load( new Query("CRUD.Patient.List", typeof(Patient) ) { new Query.Param("LN", "Lenin") } )[0];
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Vlad", result[0]["First_Name"]);

        }
        


        public static void GetSchemaAndTestVariousTypes(ICRUDDataStore store)
        {
            var schema = store.GetSchema(new Query("CRUD.Types.Load"));
                
            var row = new DynamicRow(schema);
            row["GDID"] = new GDID(0, 145);
            row["SCREEN_NAME"] = "User1";
            row["STRING_NAME"] = "Some user 1";
            row["CHAR_NAME"] = "Some user 2";
            row["BOOL_CHAR"] = 'T'; 
            row["BOOL_BOOL"] = true;

            row["AMOUNT"] = 145670.23m;

            row["DOB"] = new DateTime(1980,12,1);

            store.Insert( row );

            var row2 = store.LoadOneRow(new Query("CRUD.Types.Load", new GDID(0, 145)));

            Assert.NotNull(row2);
            Assert.AreEqual(145, row2["GDID"]);
            Assert.AreEqual("User1", row2["Screen_Name"]);
            Assert.AreEqual("Some user 1", row2["String_Name"]);
            Assert.AreEqual("Some user 2", row2["Char_Name"]);

            Assert.AreEqual(true, row2["BOOL_Char"].AsBool());
            Assert.AreEqual(true, row2["BOOL_BOOL"].AsBool());

            Assert.AreEqual(145670.23m, row2["Amount"]);

            Assert.AreEqual(1980, row2["DOB"].AsDateTime().Year);

            
        }

        public static void TypedRowTestVariousTypes(ICRUDDataStore store)
        {
                
            var row = new Types();
            row["GDID"] = new GDID(0, 234);
            row["SCREEN_NAME"] = "User1";
            row["STRING_NAME"] = "Some user 1";
            row["CHAR_NAME"] = "Some user 2";
            row["BOOL_CHAR"] = true; //notice TRUE for both char and bool columns below 
            row["BOOL_BOOL"] = true;

            row["AMOUNT"] = 145670.23m;

            row["DOB"] = new DateTime(1980,12,1);

            store.Insert( row );

            var row2 = store.LoadOneRow(new Query("CRUD.Types.Load", new GDID(0, 234)));

            Assert.NotNull(row2);
            Assert.AreEqual(234, row2["GDID"]);
            Assert.AreEqual("User1", row2["Screen_Name"]);
            Assert.AreEqual("Some user 1", row2["String_Name"]);
            Assert.AreEqual("Some user 2", row2["Char_Name"]);

            Assert.AreEqual(true, row2["BOOL_Char"].AsBool());
            Assert.AreEqual(true, row2["BOOL_BOOL"].AsBool());

            Assert.AreEqual(145670.23m, row2["Amount"]);

            Assert.AreEqual(1980, row2["DOB"].AsDateTime().Year);

            
        }


    }
}
