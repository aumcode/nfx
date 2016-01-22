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


## deferValidation(bool dfrvld)
Defer or NOT validation event while changing of field’s value.
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

## about()
Returns field's description from field definition.

## displayValue()
Returns string value of field for display in attached controls/views.

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

## applicable(bool aplcbl)
Sets or returns applicable flag.

## applyDefaultValue(bool force)
Applies default value to field and returns it.

## case(string cs)
Sets or returns string case for texboxes.

## controlType(string tp)
Sets or returns field's control type in attached view.

## defaultValue(val)
Sets or returns field's default value.

## description(string desc)
Sets or returns field's description.

## enabled(bool enbld)
Sets or returns enabled flag.

## getOrInferControlType()
Returns control type for the view from schema, or if not available then infers it from field definition.

## hint(val)
Sets or returns hint for field.

## inferControlType()
Infers control type for the view from field definition disregarding ControlType set in schema.

## isEnabled()
Returns true if field is enabled and applicable.

## isGUIModified()
Returns true when this field was modified from an attached control.

## isLookupDict()
Returns true if field has lookup dictionary constraint.

## isModified()
Returns true when this field has been modified.

## isNull()
Returns true if value is null.

## isTypeLogical()
Returns true if field contains boolean data.

## key()
Returns true if field is key.

## kind(string knd)
Sets or returns data kind for textboxes.

## loaded()
Returns true when this field has finished loading.

## lookupDict(object val)
Sets or returns lookup dictionary for field.

## marked(bool mrk)
Sets or returns marked flag.

## minValue/maxValue(val)
Sets or returns minValue/maxValue of field.

## name()
Returns immutable field name.

## password(bool pswrd)
Sets or returns password flag.

## placeholder(string plc)
Sets or returns field's placeholder property.

## preProcessValueFromGUI(val)
Pre-processes value when it is set from GUI, i.e. trim() string field or adjust phone numbers.

## record()
Returns owner record.

## readonly(bool rdnl)
Sets or returns readonly flag.

## required()
Returns true if field is required.

## resetGUIModified()
Resets field's modification from GUI flag.

## resetModified()
Resets field's modification flag.

## resetSchema()
Reverts the field instance to it's original schema definition, firing events.

## setGUIValue(val)
Sets textual value from control into the field value performing necessary adjustments, i.e. may adjust the format of the phone number etc. 

## size/minSize(int val)
Sets or returns field's size/minimum size.

## toString()
Returns string like `[fieldType]Field(fieldName = 'fieldValue')`.

## type()
Returns immutable field data type.

## schemaDef()
Returns the the original schema field def. Do not modify its values, use WAVE.clone() if copy is needed.

## stored()
Returns true if field must be stored back in the server (db).

## Validation functions
* setExternalValidationError(error) - sets external validation error (i.e. from the server side),
* valid()     - returns true if field has not validation error,
* validate()  - validates field and returns true if it is valid,
* validated() - returns true if field has been validated,
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

## visible(bool vsbl)
Sets or returns visible flag.

