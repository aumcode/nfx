# NFX Wave.js
Java Script Client Library

## General-Purpose Functions
### String Functions
#### strEmpty(string)
Returns true if string is undefined, null or empty

## Record Model
Record model is in the WAVE.RecordModel namespace.
 
### Record Class
The purpose of this class is to represent server-side NFX.DataAccess.CRUD.Schema on the client side along with view model/controller.
Record instances are initialized using server NFX.Wave.Client.RecordModelGenerator class that turns Schema into JSON object suitable for record initialization on client.

**Record(recID)**
Constructor. Initializes a new instance using just string `recID`.


**Record(recID, fieldFunc)**  
Constructor. Initializes a new instance using string recID with field initiliazation function fields will be named as `fld<fieldName>`:  
```
new WAVE.RecordModel.Record("ID-123456", function(){  
  new this.Field({Name: "FirstName", Type: "string"});    
  new this.Field({Name: "LastName", Type: "string"});
  new this.Field({Name: "Age", Type: "int"}); });
```


**Record(initVector)**  
Constructor. Initializes a new instance using initialization vector and fields will be named as `fld<fieldName>`:  
`var rec = new WAVE.RecordModel.Record({ID: <recId>, fields: [{def: <fieldDef1>, val: <value1>}...]});`




