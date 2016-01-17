# NFX Data Access
Accessing/Working with Data

## Overview of NFX Data Access
NFX data access approach is a hybrid one. It is not a strict ORM or strict CRUD, rather a 
combination of different approaches that are most beneficial to a particular application.

NFX data access was designed with the following data-store types in mind:

* Distributed data (i.e. web services/API sources)
* BigData (Hadoop, Hive etc.)
* Relational Data (SQL: MsSQL, MySQL, ORACLE etc.)
* NoSQL Data: Document and others (MongoDB, Riak, tuple spaces etc.)
* Unstructured data accessed via custom APIs(parse CSV files etc.)
* Non-homogenous data: all/some of the aforementioned sources may be needed in the same system

The data access is facilitated via the `NFX.DataAccess.IDataStore` interface which is just a 
marker interface for the application container (accessible via `NFX.App.DataStore` shortcut).

Every system may select a combination of the following strategies that fit the particular case the best:

* Calling 3rd party services (i.e. via REST) - pulling data via some API calls
* Read/Write some data via app-specific APIs (classes/props/methods) - similar to ORM
* Work with data via CRUD facade (i.e. DataStore.Insert(new Row{......}) - similar to Active Record pattern/Entity framework
* Work with higher-level facade to any of the aforementioned ways

## Building Blocks: Rows, Schema, FieldDefs

Any POCO (Plain CLR) class instance may be used to access data, as data stores are just interfaces, 
for example:  `MyCar car = MyApp.Data.GetCarsByDriver("Frank-123");`, as the function in the proceeding 
example may return a domain object "MyCar".

NFX.DataAccess.CRUD namespace provides a very convenient base for business logic/domain models
 building blocks, which are typically used in a CRUD scenario:

* NFX.DataAccess.CRUD.Schema
* NFX.DataAccess.CRUD.Row
* NFX.DataAccess.CRUD.RowsetBase/Rowset/Table
* NFX.DataAccess.CRUD.Query

### CRUD.Schema
Schema defines the structure of the "table", it consists of FieldDef instances that define attributes 
for every field. Fields may be of complex types (i.e. TypedRow). So Schema basically shapes the data
contained in Rows.

### CRUD.Row
A row is a string of data, it consists of fields where every field is assigned a FieldDef from Schema.
A Schema is the property of a Row. FieldDef is a property of a field within the row. There are two 
types of rows:

* Dynamic Rows
* Typed Rows

Dynamic rows are instances of `DynamicRow` class, they keep data internally in the `object[]`.
Typed rows are instances of sub-types of a `TypedRow`. The fields of typed row must be explicitly 
declared in code and tagged with a `[Field]` attribute which defines field's detailed FieldDef.

This design is very flexible, as both rows stem from `Row` abstract class, which has the following key
features:
    Row person = new DynamicRow(Schema.GetForTypedRow(PersonRow));
    person[0] = 123;
    Assert.AreEqual(123, person["id"]);
    person["name"] = "Frank Drebin";
    var error = person.Validate();
    Assert.IsNull(error);
    
    var person2 = new PersonRow();//no schema need to be passed as it is a typed row
    person.CopyTo(person2);
    ...
    
### CRUD.Rowset

Rowsets are what their name implies. There two types both inheriting from RowsetBase:
* Rowset
* Table

The difference between the two is the presence of primary key in the `NFX.DataAccess.CRUD.Table`
which allows for quick in-memory merges/findKey() calls, consequently table is not for sorting. It is
a pk-organized list of rows of the same schema.

`NFX.DataAccess.CRUD.Rowset` does not have this limit - it allows to sort the data, however the 
findkey() calls do linear search.

An interesting feature of rowsets is the ability to mix Dynamic and Typed rows instances in one list
as long as their schemas are the same.

Rowsets can track changes, if `RowsetBase.LogChanges=true`, then RowChange enumerable can be obtained 
via `Rowset.Changes` property. The concept is somewhat simiar to .NET's DataSet, BUT there is a 
**key difference** in the approach: **NFX Data Access is for accessing any data, not only relational**.

### CRUD Virtual Query

Queries are command objects that group parameters under some name. The queries are polymorphic (virtual),
that is: the backend provider (DataStore-implementor) is responsible for query to actual handler resolution.

There are two types of handlers:
* Script QueryHandler
* Code Query Handler

This design leads to infinite flexibility, as script queries may be written in backend-specific 
scripting technology, i.e.:

    var qry = Query("GetUserById"){ new Query.Param("UID", 1234)};
    
    //for My SQL, will get resolved into    
    SELECT T1.* FROM TBL_USER T1 WHERE T1.ID = ?UID
    
    //For MongoDB
    #pragma
    modify=user
    
    {"_id": "$$UID"}}
    
    //For Erlang MFA(module function arg)
    nfx_test:exec_qry(get_user_bid, Uid:long())
    
        
See NFX.NUnit.Integrations for more use-cases.


### CRUD Data Store

`NFX.DataAccess.CRUD.Intfs.cs` contains the definitions of `ICRUDOPerations` which stipulate the contract 
for working in a CRUD style:

    /// <summary>
    /// Describes an entity that performs single (not in transaction/batch)CRUD operations
    /// </summary>
    public interface ICRUDOperations
    {
        /// <summary>
        /// Returns true when backend supports true asynchronous operations, such as the ones that do
        /// not create extra threads/empty tasks
        /// </summary>
        bool SupportsTrueAsynchrony { get;}
        Schema GetSchema(Query query);
        Task<Schema> GetSchemaAsync(Query query);
        List<RowsetBase> Load(params Query[] queries);
        Task<List<RowsetBase>> LoadAsync(params Query[] queries);
        RowsetBase LoadOneRowset(Query query);
        Task<RowsetBase> LoadOneRowsetAsync(Query query);
        Row        LoadOneRow(Query query);
        Task<Row>  LoadOneRowAsync(Query query);
        int Save(params RowsetBase[] rowsets);
        Task<int> SaveAsync(params RowsetBase[] rowsets);
        int ExecuteWithoutFetch(params Query[] queries);
        Task<int> ExecuteWithoutFetchAsync(params Query[] queries);
        int Insert(Row row);
        Task<int> InsertAsync(Row row);
        int Upsert(Row row);
        Task<int> UpsertAsync(Row row);
        int Update(Row row, IDataStoreKey key = null);
        Task<int> UpdateAsync(Row row, IDataStoreKey key = null);
        int Delete(Row row, IDataStoreKey key = null);
        Task<int> DeleteAsync(Row row, IDataStoreKey key = null);
    }

This way of working with data backend is similar to the **"Active Record"** pattern.

An example use case:
    
    var person = new PersonRow
    {
        ID = MyApp.Data.IDGenerator.GetNext(typeof(PersonRow)),
        Name = "Jon Lord",
        IsCertified = true
    };
    
    MyApp.Data.Upsert(person);
    
Or a typical case of use with NFX.WAVE.MVC Web API:

    [Action("person", 1, "match{ methods='GET' accept-json='true'}"]
    public object GetPerson(string id)
    {
        return MyApp.Data.LoadOneRow(Queries.PersonById(id));
    }
    
    [Action("person", 1, "match{ methods='POST' accept-json='true'}"]
    public object PostPerson(Person person)
    {
        var err = person.Validate();
        if (err!=null)
          return new {OK=false, Err = err.Message};//Or throw HttpStatus code exception
          
        MyApp.Data.Upsert(person);
        return new {OK=true};
    }

As illustrated above, the NFX.WAVE framework understands row injection into the MVC actions, 
which is very convenient.


