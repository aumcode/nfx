# allValidationErrorStrings()
Returns all record and field-level validation errors.

## Examples
```js
var rec = new WAVE.RecordModel.Record("1", function(){
            new this.Field({Name: "A", Type: "string"});
            new this.Field({Name: "B", Type: "string", Required: true});
          });
          
rec.validate();
var allErr = rec.allValidationErrorStrings();
// allErr contains: 'B' must have a value
```

data()
Returns a map of {<fieldName>: <fieldValue>...}.
```
