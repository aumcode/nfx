# Data()
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