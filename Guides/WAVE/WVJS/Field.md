# Field Class
Record consists of fields and Field represents a FieldDef from server-side NFX.DataAccess.CRUD.Schema on the client side along with view model/controller.

## Field()
Constructor. Initializes a new instance using string field definition and value.
```js
new rec.Field(object fieldDef)
```
### Examples
```js
var rec = new WAVE.RecordModel.Record("ID-123456", function(){
            new this.Field({Name: "FirstName", Type: "string", Required: true});
            new this.Field({Name: "LastName", Type: "string"});
            new this.Field({Name: "Age", Type: "int"});
          });
```
```js
var rec = new WAVE.RecordModel.Record("ID-123456");
new rec.Field({Name: "FirstName", Type: "string"});
new rec.Field({Name: "LastName", Type: "string"});
new rec.Field({Name: "Age", Type: "int", Stored: false}); 
```


## drop()
Deletes this field from the record.


## deferValidation()
Defer or NOT validation event while changing of field’s value.
```js
deferValidation(bool flag)
```
### Examples
```js
var rec = ...
var elog = "";
rec.fldLastName.deferValidation(true);
rec.fldLastName.eventBind(WAVE.EventManager.ANY_EVENT, function(evtType, field, phase){
      elog += "|"+evtType + field.name() + phase + field.value();
    });
rec.fldLastName.value("Brown");
// elog = "|data-changeLastNamebeforeSmith|data-changeLastNameafterBrown"
```
```js
var rec = ...
var elog = "";
rec.fldLastName.deferValidation(false);
rec.fldLastName.eventBind(WAVE.EventManager.ANY_EVENT, function(evtType, field, phase){
      elog += "|"+evtType + field.name() + phase + field.value();
    });
rec.fldLastName.value("Brown");
// elog = "|data-changeLastNamebeforeSmith|validateLastNameundefinedBrown|validatedLastNameundefinedBrown|data-changeLastNameafterBrown"
```


## eventBind()
Binds a function to the named event handler on field.
```js
eventBind(string evtName, function handler)
```
| Parameter | Requirement | Description                                   |
| --------- |:-----------:| --------------------------------------------- |
| evtName   | required    | name of event from WAVE.RecordModel namespace |
| handler   | required    | callback-function which fires on event        |
### Examples
```js
var rec = ...
var elog = "";
rec.eventBind(WAVE.RecordModel.EVT_FIELD_DROP, function(field, phase){
          elog += "|" + field.name() + phase;
         });
rec.fldLastName.drop();
// elog = |LastNamebefore|LastNameafter          
```


## isGUIModified()
Returns true when this field was modified from an attached control.


## isModified()
Returns true when this field has been modified.


## loaded()
Returns true when this field has finished loading.


## toString()
Returns string like `[fieldType]Field(fieldName = 'fieldValue')`.


## stored()
Returns true if field must be stored back in the server (db).


## Validation functions
* valid()     - returns true if field has not validation error.
* validate()  - validates field and returns true if it is valid.
* validated() - returns true if field has been validated.
* validationError() - returns error thrown during validation.
### Examples
```js
var rec = new WAVE.RecordModel.Record("1", function(){
            new this.Field({Name: "A", Type: "string"});
            new this.Field({Name: "B", Type: "string", Required: true});
          });
     
rec.fldB.validated(); // false
rec.fldB.validate();
rec.fldB.validated(); //true
rec.fldB.valid();     // false
var err = rec.fldB.validationError(); // contains: 'B' must have a value
rec.fldB.value("aaaaa");
rec.fldB.valid();     // true
```


## value()
`value()` - then returns field’s value.  
`value(object value, bool fromGUI)` - Sets value and modified, validated flags if new value is different from an existing one. `fromGUI` indicates that field is being altered from an attached control.



