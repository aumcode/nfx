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

#### Record()
Constructor. Initializes a new instance using record id with optional field init function
or complex init vector:
```js
new WAVE.RecordModel.Record(recID, fieldFunc)
```
##### Parameters
| Parameter     |  Necessity    | Description                                    |
| ------------- |:-------------:| ---------------------------------------------- |
| fieldFunc     | optional      | function() {new this.Field(<fieldDef>,...) }   |
| recID         | required      | record id                                      |

or

```js
new WAVE.RecordModel.Record(initVector)
```
##### Parameters
| Parameter     |  Necessity    | Description                                    |
| ------------- |:-------------:| ---------------------------------------------- |
| initVector    | required      | Contains record id and fields' definitions     |

**Notes**  
In both cases fields wil be named as concatenation string 'fld' and field name from definition.

##### Examples
```js
var rec = new WAVE.RecordModel.Record("ID-123456");
```
```js
var rec = new WAVE.RecordModel.Record("ID-123456", function(){
            new this.Field({Name: "FirstName", Type: "string"});
            new this.Field({Name: "LastName", Type: "string"});
            new this.Field({Name: "Age", Type: "int"});
          });
```
```js
var rec = new WAVE.RecordModel.Record({ID: 'REC-1', 
            fields: [
              {def: {Name: 'FirstName', Type: 'string'}, val: 'John'},
              {def: {Name: 'LastName', Type: 'string'}, val: 'Smith'},
              {def: {Name: 'Age', Type: 'int'}, val: 33},
              {def: {Name: 'Helper', Type: 'string', Stored: false}}
            ]});
```

#### allValidationErrorStrings()
Returns all record and field-level validation errors.

##### Examples
```js
var rec = new WAVE.RecordModel.Record("1", function(){
            new this.Field({Name: "A", Type: "string"});
            new this.Field({Name: "B", Type: "string", Required: true});
          });
          
rec.validate();
var allErr = rec.allValidationErrorStrings();
// allErr contains: 'B' must have a value
```
