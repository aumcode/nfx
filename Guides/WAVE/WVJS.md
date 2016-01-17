# NFX Wave.js
Java Script Client Library

## General-Purpose Functions
### String Functions
#### strEmpty(string)
Returns true if string is undefined, null or empty

## Record Model
Record model is in the WAVE.RecordModel namespace. The purpose of this class is to represent 
the server-side NFX.DataAccess.CRUD.Schema on the client side along with view model/controller. 
Record instances are initialized using server NFX.Wave.Client.RecordModelGenerator class that turns
Schema into JSON object suitable for record initialization on client.
### Record Class

#### new Record(recID, initVector)
Constructor. Initializes a new instance using initialization vector:
 `var rec = new WAVE.RecordModel.Record({ID: <recId>, fields: [{def: <fieldDef1>, val: <value1>}...]})`.
Fields will be named as `fld<fieldName>`.

