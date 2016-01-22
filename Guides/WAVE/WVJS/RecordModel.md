#WAVE.RecordModel

## Constants
```js
FIELDDEF_DEFAULTS: {
            Description: '',
            Placeholder: '',
            Type:        'string',
            Key:         false,
            Kind:        'text',
            Case:        'asis', 
            Stored:     true,
            Required:   false,
            Applicable: true,
            Enabled:    true,
            ReadOnly:   false,
            Visible:    true,
            Password:   false,
            MinValue:   null,
            MaxValue:   null,
            MinSize:    0,
            Size:       0,
            ControlType:  'auto',
            DefaultValue: null,
            Hint:       null,
            Marked:     false,
            LookupDict: {}, //{key:"description",...}]
            Lookup:     {}, //complex lookup 
            DeferValidation: false
        },
EVT_VALIDATION_DEFINITION_CHANGE: "validation-definition-change",
EVT_INTERACTION_CHANGE: "interaction-change",
EVT_DATA_CHANGE: "data-change",

EVT_RECORD_LOAD: "record-load",
EVT_VALIDATE:    "validate",
EVT_VALIDATED:   "validated",
EVT_FIELD_LOAD:  "field-load",
EVT_FIELD_RESET: "field-reset",
EVT_FIELD_DROP:  "field-drop",

EVT_PHASE_BEFORE: "before",
EVT_PHASE_AFTER:  "after",

CTL_TP_AUTO:    "auto",
CTL_TP_CHECK:   "check",
CTL_TP_RADIO:   "radio",
CTL_TP_COMBO:   "combo",
CTL_TP_TEXT:    "text",
CTL_TP_TEXTAREA:"textarea",
CTL_TP_HIDDEN:  "hidden",
CTL_TP_PUZZLE:  "puzzle",

KIND_TEXT:  'text',
KIND_SCREENNAME:  'screenname',
KIND_COLOR: 'color',
KIND_DATE:  'date',
KIND_DATETIME: 'datetime',
KIND_DATETIMELOCAL: 'datetime-local',
KIND_EMAIL: 'email',
KIND_MONTH: 'month',
KIND_NUMBER: 'number',
KIND_RANGE:  'range',
KIND_SEARCH: 'search',
KIND_TEL:    'tel',
KIND_TIME:   'time',
KIND_URL:    'url',
KIND_WEEK:   'week',

CASE_ASIS:  'asis',
CASE_UPPER: 'upper',
CASE_LOWER: 'lower',
CASE_CAPS:  'caps',
CASE_CAPSNORM:  'capsnorm',

DATA_RECVIEW_ID_ATTR = "data-wv-rid",
DATA_FIELD_NAME_ATTR = "data-wv-fname",
DATA_CTL_TP_ATTR = "data-wv-ctl"
```


## Functions
##### records()
Returns the copy of list of record instances.

##### isDirty()
Returns true when there is at least one record instance with user-made modifications.


## Classes
##### [Record](Record.md)
##### [Field](Field.md)
##### [RecordView](RecordView.md)
