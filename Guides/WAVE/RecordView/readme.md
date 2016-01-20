# RecordView
Binds Record model (an instance of Record class) with UI builder/rendering library which dynamically builds the DOM in the attached view components (div placeholders). Thus, changes made to view model/data model in record instance will get automatically represented in the attached UI.

## RecordView
Constructor. Initializes a new instance using string field definition and value.
```js
new WAVE.RecordModel.Record(string ID, object record, object gui, bool manualViews)
```
| Parameter   | Requirement | Description                                                           |
| ----------- |:-----------:| --------------------------------------------------------------------- |
| ID          | required    | id - unique in page id of the view                                    |
| record      | required    | data record instance                                                  |
| gui         | optional    | GUI library, if null then default script "wv.gui.js" must be included |
| manualViews | optional    | if true then view controls will not be auto-constructed               |
### Examples
```js
var REC =  new WAVE.RecordModel.Record({ID: 'R1', 
            fields: [
                {def: {Name: 'FirstName', Type: 'string', Required: true}, val: 'John'},
                {def: {Name: 'LastName', Type: 'string', Required: true}, val: 'Smith'},
                {def: {Name: 'Age', Type: 'int', MinValue: 10}, val: 33},
                {def: {Name: 'MusicType', Type: 'string', LookupDict: {HRK: "Hard Rock", CRK: "Classic Rock", RAP: "Rap", CMU: "Classical music"}}},
            ]}
        );
var RVIEW = new WAVE.RecordModel.RecordView("V1", REC);
```
