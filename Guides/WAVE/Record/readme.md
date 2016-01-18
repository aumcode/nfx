# Record Class
The purpose of this class is to represent server-side NFX.DataAccess.CRUD.Schema on the client side along with view model/controller.
Record instances are initialized using server NFX.Wave.Client.RecordModelGenerator class that turns Schema into JSON object suitable for record initialization on client.

## Record()
Constructor. Initializes a new instance using record id with optional field initialization function
or complex initialization vector:
```js
new WAVE.RecordModel.Record(recID[, fieldFunc])
```
### Parameters
| Parameter | Requirement | Description                                                       |
| --------- |:-----------:| ----------------------------------------------------------------- |
| fieldFunc | optional    | callback-function which contains fields initialization statements |
| recID     | required    | record id                                                         |

or

```js
new WAVE.RecordModel.Record(initVector)
```
### Parameters
| Parameter  | Requirement | Description                                            |
| -----------|:---------- :| ------------------------------------------------------ |
| initVector | required    | Contains record id and fields' definitions with values |

**Notes**  
In both cases fields will be named as concatenation string 'fld' and field name from definition.

### Examples
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

## allValidationErrorStrings()
Returns all record and field-level validation errors.

### Examples
```js
var rec = new WAVE.RecordModel.Record("1", function(){
            new this.Field({Name: "A", Type: "string"});
            new this.Field({Name: "B", Type: "string", Required: true});
          });
          
rec.validate();
var allErr = rec.allValidationErrorStrings();
// allErr contains: 'B' must have a value
```

## data()
Returns a map of fields: `{fieldName:fieldValue,...}`.

```js
data(modifiedOnly, includeNonStored)
```

## Parameters
| Parameter        |  Necessity | Description                         |
| ---------------- |:----------:| ----------------------------------- |
| modifiedOnly     | optional   | only get fields that have changed   |
| includeNonStored | optional   |  include fields that are not stored |

## Examples
```js
var rec = new WAVE.RecordModel.Record({ID: 'REC-1', 
            fields: [
              {def: {Name: 'FirstName', Type: 'string'}, val: 'John'},
              {def: {Name: 'LastName', Type: 'string'}, val: 'Smith'},
              {def: {Name: 'Age', Type: 'int'}, val: 33},
              {def: {Name: 'Helper', Type: 'string', Stored: false}}
            ]});

var d = JSON.stringify(rec.data(false, true));
// d = {"FirstName":"John","LastName":"Smith","Age":33,"Helper":null}

rec.fldLastName.value("Brown");
var d = JSON.stringify(rec.data(true));
// d = {"LastName":"Brown"}
```


## eventBind()
Binds a function to the named event handler.
```js
eventBind(evtName, handler)
```
### Parameters
| Parameter | Requirement | Description                                                       |
| --------- |:-----------:| ----------------------------------------------------------------- |
| evtName   | optional    | callback-function which contains fields initialization statements |
| recID     | required    | record id                                                         |
