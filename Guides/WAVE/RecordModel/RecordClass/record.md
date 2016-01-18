# Record()
Constructor. Initializes a new instance using record id with optional field initialization function
or complex initialization vector:
```js
new WAVE.RecordModel.Record(recID, fieldFunc)
```
## Parameters
| Parameter     |  Necessity    | Description                                    |
| ------------- |:-------------:| ---------------------------------------------- |
| fieldFunc     | optional      | callback-function which contains     |
| recID         | required      | record id                                      |

or

```js
new WAVE.RecordModel.Record(initVector)
```
## Parameters
| Parameter     |  Necessity    | Description                                                |
| ------------- |:-------------:| ----------------------------------------------             |
| initVector    | required      | Contains record id and fields' definitions with values     |

**Notes**  
In both cases fields will be named as concatenation string 'fld' and field name from definition.

## Examples
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
