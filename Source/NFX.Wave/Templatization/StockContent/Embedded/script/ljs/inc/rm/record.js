//Record class, either pass just string ID with optional field init func:
// var rec = new WAVE.RecordModel.Record("r1",function(rec){ new this.Field()... )
//or complex init vector:
// {ID: string, fields: [{def: fieldDef1,val: value1}...}]}
published.Record = function(init, fieldFunc)
{
  if (!init) throw "Record.ctor(need id+fieldFunc or initObject)";
  var record = this;
  var fRecordLoaded = false;

  WAVE.extend(record, WAVE.EventManager);

  this.eventInvoke(published.EVT_RECORD_LOAD, published.EVT_PHASE_BEFORE);//todo How to subscribe to this event?

  var id = null;
  var lang = null;
  var fFormMode = "unspecified";
  var fCSRFToken = "";
  var fRoundtrip = "";

  if (WAVE.isObject(init) &&  init.fields)
  {
    id = init.ID;
    lang = init.ISOLang;

    if (init.__FormMode)
      fFormMode = init.__FormMode;

    if (init.__CSRFToken)
      fCSRFToken = init.__CSRFToken;

    if (init.__Roundtrip)
      fRoundtrip = init.__Roundtrip;
  }
  else
  {
    id = init.toString();
    init = null;
  }

  var fID = WAVE.strEmpty(id)? WAVE.genRndKey(16) : id;
  var fISOLang = WAVE.strEmpty(lang) ? "eng" : lang;

  var fFields = [];
  var fRecValidated = false;
  var fRecValidationError = null;

  function resetRecValidation(){
    fRecValidated = false;
    fRecValidationError = null;
  }


  //Returns record instance ID
  this.ID = function(){ return fID;};

  //Returns record ISO language/culture
  this.ISOLang = function(){ return fISOLang;};

  //Returns form mode on the server if one was supplied (i.e. insert|edit). This property can not be set on client
  this.formMode = function(){ return fFormMode;};

  //Returns CSRF token supplied by server. This property can not be set on client
  this.csrfToken = function(){ return fCSRFToken;};

  //Returns Roundtrip bag content supplied by server. This property can not be set on client
  this.roundtrip = function(){ return fRoundtrip;};

  //Returns true when record has finished loading data and constructing fields
  //Field event consumers may reference this flag to exit out of unnecessary event processing
  this.loaded = function() {return fRecordLoaded;};

  //Returns copy of fields list
  this.fields = function() {
    return WAVE.arrayShallowCopy(fFields);
  };

  //Returns a map of {fieldName: fieldValue...}
  //modifiedOnly - will only get fields that have changed
  //includeNonStored - will include fields that are not Stored
  this.data = function(modifiedOnly, includeNonStored){
    var result = {};

    for(var i in fFields){
      var fld = fFields[i];
      if ((!modifiedOnly ||
            fld.isModified() ||
            fld.isGUIModified()) &&
          (includeNonStored || fld.stored())
          )  result[fld.name()] = fld.value();
    }

    if (!WAVE.strEmpty(fFormMode) && fFormMode!=="unspecified")
      result["__FormMode"] = fFormMode;

    if (!WAVE.strEmpty(fCSRFToken))
      result["__CSRFToken"] = fCSRFToken;

    if (!WAVE.strEmpty(fRoundtrip))
      result["__Roundtrip"] = fRoundtrip;

    return result;
  };

  //Returns a field by its case-insensitive name or null
  this.fieldByName = function(name){
    if (WAVE.strEmpty(name)) return null;
    for(var i in fFields){
      var fld = fFields[i];
      if (WAVE.strSame(fld.name(), name)) return fld;
    }
    return null;
  };


  //Returns array of all recrod and field-level validation errors
  this.allValidationErrors = function(){
    var errors = [];
    if (fRecValidationError!==null) errors.push(fRecValidationError);
    for(var i in fFields){
      var fld = fFields[i];
      var fe = fld.validationError();
      if (fe!==null) errors.push(fe);
    }
    return errors;
  };

  //Returns all record and field-level validation errors
  this.allValidationErrorStrings = function(){
    var errors = "";
    var all = this.allValidationErrors();
    for(var i in all)
      errors += all[i].toString() + "\n";
    return errors;
  };


  //Validates record and returns true is everything is valid
  this.validate = function() {
    resetRecValidation();
    fRecValidated = true;

    var result = true;
    for(var i in fFields){
      var f = fFields[i];
      if (!f.validate()) result = false;
    }

    if (result)
      try
      {
        record.eventInvoke(published.EVT_VALIDATE);//this throws errors
      }
      catch(error)
      {
        fRecValidationError = error;
        result = false;
      }

    record.eventInvoke(published.EVT_VALIDATED);
    return result;
  };

  this.validated = function(){ return fRecValidated;};

  //Returns rec-level validation eror if any
  this.validationError = function(){ return fRecValidationError;};

  //Returns true if record and all its fields have been validated and valid
  this.valid = function() {
    return fRecValidated &&  fRecValidationError===null && this.fieldsValid();
  };


  //Sets external validation error (i,e, from the server side)
  this.setExternalValidationError = function(error, errorText){//for now error is ignored
    if (WAVE.strEmpty(errorText)) return;
    fRecValidationError = errorText;
    fRecValidated = false;
  };

  //Returns true if all field have been validated (but some may be invalid)
  this.fieldsValidated = function() {
    for(var i in fFields)
      if (!fFields[i].validated()) return false;
    return true;
  };

  //Returns true if all field have been validated and valid
  this.fieldsValid = function() {
    for(var i in fFields)
      if (!fFields[i].valid()) return false;
    return true;
  };


  //Returns true is some field have been modified
  this.isModified = function() {
      for(var i in fFields)
        if (fFields[i].isModified()) return true;
      return false;
  };

  //Returns true is some field have been modified through GUI-attached views
  this.isGUIModified = function() {
    for(var i in fFields)
      if (fFields[i].isGUIModified()) return true;
    return false;
  };

  //Resets all field modification flags
  this.resetModified = function() {
    for(var i in fFields){
      var f = fFields[i];
      f.resetModified();
      f.resetGUIModified();
    }
    return false;
  };

  //Resets all field schemas
  this.resetSchema = function() {
    for(var i in fFields) fFields[i].resetSchema();
  };

  //Applies default values to all fields
  this.applyDefaultValue = function(force) {
    for(var i in fFields) fFields[i].applyDefaultValue(force);
  };

  //Changes all eventInvocationSuspendCount on all fields and this record
  this.latchAllEvents = function(delta) {
    this.eventInvocationSuspendCount+=delta;
    for(var i in fFields) fFields[i].eventInvocationSuspendCount+=delta;
  };

  //Re-inits the record and all of its fields anew - resetting all flags.
  this.resetRecord = function(disableTransitiveEvents){
    if (disableTransitiveEvents) record.latchAllEvents(+1);
    try
    {
      resetRecValidation();
      this.resetSchema();
      this.applyDefaultValue(true);
      this.resetModified();
    }
    finally
    {
      if (disableTransitiveEvents) record.latchAllEvents(-1);
    }

    for(var i in fFields){
      var f = fFields[i];
      f.eventInvoke(published.EVT_FIELD_RESET, f);
    }
  };


  //Field class
  this.Field = function(fieldDef)
  {
    var fFieldLoaded = false;

    if (!WAVE.isObject(fieldDef)) throw "Field.ctor(fieldDef must be a map)";
    if (WAVE.strEmpty(fieldDef.Name)) throw "Field def must have a name";
    if (WAVE.strEmpty(fieldDef.Type)) throw "Field def must have a type";

    //Coerce strbools to bools
    fieldDef.Key      = WAVE.strAsBool(fieldDef.Key, published.FIELDDEF_DEFAULTS.Key);
    fieldDef.Stored   = WAVE.strAsBool(fieldDef.Stored, published.FIELDDEF_DEFAULTS.Stored);
    fieldDef.Required = WAVE.strAsBool(fieldDef.Required, published.FIELDDEF_DEFAULTS.Required);
    fieldDef.Applicable = WAVE.strAsBool(fieldDef.Applicable, published.FIELDDEF_DEFAULTS.Applicable);
    fieldDef.Enabled    = WAVE.strAsBool(fieldDef.Enabled, published.FIELDDEF_DEFAULTS.Enabled);
    fieldDef.ReadOnly   = WAVE.strAsBool(fieldDef.ReadOnly, published.FIELDDEF_DEFAULTS.ReadOnly);
    fieldDef.Visible    = WAVE.strAsBool(fieldDef.Visible, published.FIELDDEF_DEFAULTS.Visible);
    fieldDef.Password   = WAVE.strAsBool(fieldDef.Password, published.FIELDDEF_DEFAULTS.Password);
    fieldDef.Marked     = WAVE.strAsBool(fieldDef.Marked, published.FIELDDEF_DEFAULTS.Marked);
    fieldDef.DeferValidation = WAVE.strAsBool(fieldDef.DeferValidation, published.FIELDDEF_DEFAULTS.DeferValidation);


    var field = this;
    WAVE.extend(field, WAVE.EventManager);
    fFields.push( field );//register with parent
    var recPropertyName = "fld"+fieldDef.Name;
    record[recPropertyName] = field;//make rec.fldLastName shortcut

    record.eventInvoke(published.EVT_FIELD_LOAD, field, published.EVT_PHASE_BEFORE);

    //convert LookupDict to object if it is not an object
    WAVE.propStrAsObject(fieldDef, "LookupDict");

    //convert Lookup to object if it is not an object
    WAVE.propStrAsObject(fieldDef, "Lookup");

    var fSchemaDef = fieldDef;
    var fDef = WAVE.clone(fieldDef);//make copy
    WAVE.extend(fDef, published.FIELDDEF_DEFAULTS, true);

    var fValue = null;
    var fModified = false;
    var fGUIModified = false;
    var fValidated = false;
    var fValidationError = null;


    //Owner record
    this.record = function(){return record; };

    //Returns true when this field has finished loading
    this.loaded = function() {return fFieldLoaded;};

    //Deletes this field from the record
    this.drop = function() {
      if (!record[recPropertyName]) return false;
      this.eventInvoke(published.EVT_FIELD_DROP, published.EVT_PHASE_BEFORE);
      record.eventInvoke(published.EVT_FIELD_DROP, field, published.EVT_PHASE_BEFORE);
      WAVE.arrayDelete(fFields, this);
      delete record[recPropertyName];
      record.eventInvoke(published.EVT_FIELD_DROP, field, published.EVT_PHASE_AFTER);
      this.eventInvoke(published.EVT_FIELD_DROP, published.EVT_PHASE_AFTER);
      return true;
    };

    //Returns the the original schema field def.
    //DO NOT modify its values, use WAVE.clone() if copy is needed
    this.schemaDef  = function(){return fSchemaDef;};

    //Immutable field name
    this.name = function(){return fDef.Name;};

    //Immutable field data type
    this.type = function(){return fDef.Type;};

    //True if field contains boolean data
    this.isTypeLogical = function() { return WAVE.strOneOf(fDef.Type, ["bool", "boolean", "logical"]);};

    //True if field has lookup dictionary constraint
    this.isLookupDict = function(){
      var ld = fDef.LookupDict;
      return Object.keys(ld).length > 0;
    };

    //True if field is key
    this.key = function(){return fDef.Key;};

    //True if field must be stored back in the server (db)
    this.stored = function(){return fDef.Stored;};

    this.about = function(){
      var result = fDef.Description;
      if (WAVE.strEmpty(result)) result = fDef.Name;
      return result;
    };

    //Infers control type for the view from field definition disregarding ControlType set in schema
    this.inferControlType = function(){
      if (this.isTypeLogical()) return published.CTL_TP_CHECK;
      if (this.isLookupDict())
      {
        var cnt = Object.keys(fDef.LookupDict).length;
        if (cnt>4) return published.CTL_TP_COMBO;
        return published.CTL_TP_RADIO;
      }

      if (fDef.Size>64)
        return published.CTL_TP_TEXTAREA;
      else
        return published.CTL_TP_TEXT;
    };

    //Returns control type for the view from schema, or if not avalable then infers it from field def
    this.getOrInferControlType = function(){
      var ct = fDef.ControlType;
      if (WAVE.strEmpty(ct) || WAVE.strOneOf(ct, [published.CTL_TP_AUTO, "automatic", "infer"])) ct = this.inferControlType();
      return ct;
    };



    function resetValidation(){
      resetRecValidation();
      fValidated = false;
      fValidationError = null;
    }

    function fireValidationDefChange(what, val){
      record.eventInvoke(published.EVT_VALIDATION_DEFINITION_CHANGE, field, what, val);
      field.eventInvoke(published.EVT_VALIDATION_DEFINITION_CHANGE, what, val);
    }

    function fireInteractionChange(what, val){
      record.eventInvoke(published.EVT_INTERACTION_CHANGE, field, what, val);
      field.eventInvoke(published.EVT_INTERACTION_CHANGE, what, val);
    }


    //Reverts the field instance to it's original schema def, firing events
    this.resetSchema = function(){
      fDef = WAVE.clone(fSchemaDef);//make copy
      WAVE.extend(fDef, published.FIELDDEF_DEFAULTS, true);

      resetValidation();

      fireValidationDefChange(null, null);
      fireInteractionChange(null, null);
    };


    //Validates field and returns true if it is valid
    this.validate = function(){

      function valError(txt, args){       return WAVE.strTemplate( WAVE.strLocalize(record.ISOLang(), "RMField", "error", txt), args);        }

      fValidated = true;
      fValidationError = null;
      try
      {
        if (fDef.Required){
          if (fValue===null || WAVE.strEmpty(fValue.toString())) throw valError("Field '@f@' must have a value", {f: this.about()});
        }

        if (fValue!==null &&
            !WAVE.strEmpty(fValue.toString()))
        {
          var min = fDef.MinValue;
          if (min!==null){
            try { if (fValue<min) throw 1; }
            catch(e) { throw valError("Field '@f@' value can not be less than '@b@'", {f: this.about(), b: min}); }
          }

          var max = fDef.MaxValue;
          if (max!==null){
            try { if (fValue>max) throw 1; }
            catch(e) { throw valError("Field '@f@' value can not be greater than '@b@'", {f: this.about(), b: max}); }
          }

          var sz = fDef.Size;
          if (sz!==null && sz>0){
            if (fValue.toString().length>sz) throw valError("Field '@f@' value can not be longer than @b@ characters", {f: this.about(), b: sz});
          }

          sz = fDef.MinSize;
          if (sz!==null && sz>0){
            if (fValue.toString().length<sz) throw valError("Field '@f@' value can not be shorter than @b@ characters", {f: this.about(), b: sz});
          }

          var keys = Object.keys(fDef.LookupDict);
          var isSimple = WAVE.isSimpleKeyStringMap(fDef.LookupDict);
          if (keys.length > 0 && isSimple) {
            var sval = WAVE.convertScalarType(false, fValue, "string", "<unconvertible>");
            if (!WAVE.strOneOf(sval, keys)) throw valError("Field '@f@' value '@v@' is not allowed", {f: this.about(), v: sval});
          }

          if (fDef.Kind===published.KIND_EMAIL){
            var evalid = WAVE.strIsEMail(fValue);
            if (!evalid) throw valError("Field '@f@' must be a valid e-mail address", {f: this.about()});
          }

          if (fDef.Kind===published.KIND_SCREENNAME){
            var svalid = WAVE.strIsScreenName(fValue);
            if (!svalid) throw valError("Field '@f@' must start from letter and contain only letters or digits separated by single '.' or '-' or '_'", {f: this.about()});
          }
        }

        field.eventInvoke(published.EVT_VALIDATE);//this trows errors
      }
      catch(error)
      {
        fValidationError = error;
      }
      field.eventInvoke(published.EVT_VALIDATED);
      return fValidationError===null;
    };

    //Error thrown during validation
    this.validationError = function() { return fValidationError;};

    //Sets external validation error (i,e, from the server side)
    this.setExternalValidationError = function(error, errorText){//for now error is ignored
      if (WAVE.strEmpty(errorText)) return;
      fValidationError = errorText;
      fValidated = false;
    };

    this.required = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.Required) return fDef.Required;
      fDef.Required = val;
      resetValidation();
      fireValidationDefChange("required", val);
    };


    //Sets value and modified, validated flags if new value is different from an existing one.
    //Pass fromGUI to indicate that field is being altered from an attached control
    this.value = function(val, fromGUI){
      if (typeof(val)===tUNDEFINED) return fValue;
      var old = fValue;
      //convert value to field's data type
      var cval = WAVE.convertScalarType(true, val, fDef.Type, fDef.DefaultValue);

      var same = WAVE.isSame(old, cval);

      if (fromGUI){
        var prior = cval;
        cval = this.preProcessValueFromGUI(cval);
        same = same && WAVE.isSame(prior, cval);
      }

      if (same) return fValue;//value did not change

      record.eventInvoke(published.EVT_DATA_CHANGE, published.EVT_PHASE_BEFORE, field, old, cval);
      this.eventInvoke(published.EVT_DATA_CHANGE, published.EVT_PHASE_BEFORE, old, cval);
      fValue = cval;
      fModified = true;
      if (fromGUI) fGUIModified = true;
      if (fDef.DeferValidation)
        resetValidation();
      else
        this.validate();
      this.eventInvoke(published.EVT_DATA_CHANGE, published.EVT_PHASE_AFTER, old, cval);
      record.eventInvoke(published.EVT_DATA_CHANGE, published.EVT_PHASE_AFTER, field, old, cval);

      return fValue;
    };

    //Pre-processes value when it is set from GUI, i.e. trim() string field or adjust phone numbers
    this.preProcessValueFromGUI = function(val){
      if (val===null) return null;

      if (typeof(val)==='string')
      {
        val = WAVE.strTrim(val);

        if (fDef.Kind===published.KIND_TEL)
          val = WAVE.strNormalizeUSPhone(val);

        if (fDef.Case===published.CASE_UPPER) val = val.toUpperCase();
        else if (fDef.Case===published.CASE_LOWER) val = val.toLowerCase();
        else if (fDef.Case===published.CASE_CAPS) val = WAVE.strCaps(val, false);
        else if (fDef.Case===published.CASE_CAPSNORM) val = WAVE.strCaps(val, true);
      }

      return val;
    };

    this.isNull = function(){ return fValue===null;};

    //returns string value of field for display in attached controls/views
    this.displayValue = function(){
      if (fValue===null) return "";

      if (WAVE.isDateType(fDef.Type))
      {
        //todo Add culture
        if (fDef.Kind===published.KIND_DATE) return WAVE.toUSDateString(fValue);
        return WAVE.toUSDateTimeString(fValue);
      }
      return fValue.toString();
    };

    //sets textual value from control into the field value performing necessary adjustements, i.e.
    //may adjust the format of the phone number etc.
    this.setGUIValue = function(val){
      //todo adjust phones, etc..
      this.value(val, true);
    };


    this.applyDefaultValue = function(force){
      if (force || this.isNull())
      this.value(fDef.DefaultValue);

      return fValue;
    };



    this.valid = function() { return fValidationError===null; };
    this.validated = function() { return fValidated;};
    this.validationError = function() { return fValidationError;};

    this.isModified = function() { return fModified; };
    this.resetModified = function() {
      var was = fModified;
      fModified = false;
      fireInteractionChange("modified", false);
      return was;
    };

    this.isGUIModified = function() { return fGUIModified; };
    this.resetGUIModified = function()
    {
      var was = fGUIModified;
      fGUIModified = false;
      fireInteractionChange("guimodified", false);
      return was;
    };

    //Returns true if field is enabled and applicable
    this.isEnabled = function(){
      return this.enabled() && this.applicable();
    };


    //Data kind for texboxes
    this.kind = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.Kind) return fDef.Kind;
      fDef.Kind = val;
      fireInteractionChange("kind", val);
    };

    //String case  for texboxes
    this.case = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.Case) return fDef.Case;
      fDef.Case = val;
      if (WAVE.isStringType(fDef.Type))
        this.value(fValue, false);
    };

    this.applicable = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.Applicable) return fDef.Applicable;
      fDef.Applicable = val;
      fireInteractionChange("applicable", val);
    };

    this.enabled = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.Enabled) return fDef.Enabled;
      fDef.Enabled = val;
      fireInteractionChange("enabled", val);
    };

    this.readonly = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.ReadOnly) return fDef.ReadOnly;
      fDef.ReadOnly = val;
      fireInteractionChange("readonly", val);
    };

    this.visible = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.Visible) return fDef.Visible;
      fDef.Visible = val;
      fireInteractionChange("visible", val);
    };

    this.password = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.Password) return fDef.Password;
      fDef.Password = val;
      fireInteractionChange("password", val);
    };

    this.deferValidation = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.DeferValidation) return fDef.DeferValidation;
      fDef.DeferValidation = val;
      resetValidation();
      fireValidationDefChange("defervalidation", val);
    };

    this.minValue = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.MinValue) return fDef.MinValue;
      fDef.MinValue = val;
      resetValidation();
      fireValidationDefChange("minvalue", val);
    };

    this.maxValue = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.MaxValue) return fDef.MaxValue;
      fDef.MaxValue = val;
      resetValidation();
      fireValidationDefChange("maxvalue", val);
    };


    this.minSize = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.MinSize) return fDef.MinSize;
      fDef.MinSize = val;
      resetValidation();
      fireValidationDefChange("minsize", val);
    };

    this.size = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.Size) return fDef.Size;
      fDef.Size = val;
      resetValidation();
      fireValidationDefChange("size", val);
    };

    this.controlType = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.ControlType) return fDef.ControlType;
      fDef.ControlType = val;
      fireInteractionChange("controltype", val);
    };

    this.scriptType = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.ScriptType) return fDef.ScriptType;
      fDef.ScriptType = val;
      fireInteractionChange("scripttype", val);
    };

    this.defaultValue = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.DefaultValue) return fDef.DefaultValue;
      fDef.DefaultValue = val;
      resetValidation();
      fireValidationDefChange("defaultvalue", val);
    };

    this.lookupDict = function(val){
      if (typeof(val)===tUNDEFINED) return fDef.LookupDict;
      if (val===null) val = {};
      if (WAVE.isSame(fDef.DefaultValue, val)) return fDef.LookupDict;
      fDef.LookupDict = val;
      resetValidation();
      fireValidationDefChange("lookupDict", val);
    };

    this.hint = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.Hint) return fDef.Hint;
      fDef.Hint = val;
      fireInteractionChange("hint", val);
    };

    this.marked = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.Marked) return fDef.Marked;
      fDef.Marked = val;
      fireInteractionChange("marked", val);
    };

    this.description = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.Description) return fDef.Description;
      fDef.Description = val;
      fireInteractionChange("description", val);
    };

    this.placeholder = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.Placeholder) return fDef.Placeholder;
      fDef.Placeholder = val;
      fireInteractionChange("placeholder", val);
    };

    this.config = function(val){
      if (typeof(val)===tUNDEFINED || val===fDef.Config) return fDef.Config;

      if(!WAVE.isObject(val))
        throw "RecordView.config(val: must be object)";
      fDef.Config = val;

      resetValidation();
      fireValidationDefChange("config", null);
      fireInteractionChange("config", null);
    };

    this.toString = function(){
      return "["+this.type()+"]Field("+this.name()+" = '"+this.value()+"')";
    };


    fFieldLoaded = true;
    record.eventInvoke(published.EVT_FIELD_LOAD, field, published.EVT_PHASE_AFTER);
  };//Field

  //Complex object with schema was passed, init fields from it
  if (init){
    for(var i in init.fields){
      var finit = init.fields[i];
      var fld = new this.Field(finit.def);
      if (finit.hasOwnProperty('val')) fld.value(finit.val);
      if (finit.hasOwnProperty('errorText')) fld.setExternalValidationError(WAVE.strDefault(finit.error), finit.errorText);
      fld.resetModified();
    }

    //Record-level validation
    if (init.hasOwnProperty('errorText')) this.setExternalValidationError(WAVE.strDefault(init.error), init.errorText);

  }else if (WAVE.isFunction(fieldFunc)) fieldFunc.apply(record);

  fRecordLoaded = true;
  fRecords.push(this);
  this.eventInvoke(published.EVT_RECORD_LOAD, published.EVT_PHASE_AFTER);
};//Record

published.Record.prototype.toString = function(){ return "Record["+this.ID()+"]"; };