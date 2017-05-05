published.isString = function (obj) {
  if (typeof (obj) === tUNDEFINED) return false;
  return Object.prototype.toString.call(obj) === '[object String]';
};

//Returns true when the passed parameter is a map, not an array or function
published.isObject = function (obj) {
  if (typeof (obj) === tUNDEFINED) return false;
  return obj === Object(obj) && !published.isArray(obj) && !published.isFunction(obj);
};

//Returns true when the passed parameter is an array, not a map or function
published.isArray = function (obj) {
  if (typeof (obj) === tUNDEFINED) return false;
  return Object.prototype.toString.call(obj) === '[object Array]';
};

//Returns true when poassed parameter is a function, not a map object or an array
published.isFunction = function (obj) {
  return typeof (obj) === "function";
};

//Returns true when the passed parameter is an array, or map but not a function
published.isMapOrArray = function (obj) {
  return obj === Object(obj) && !published.isFunction(obj);
};

//Shortcut for not null and not undefined
published.exists = function (obj) {
  return typeof (obj) !== tUNDEFINED && obj !== null;
};

//Overrides existing function by wrapping in new one. May call base like so:
//  object.about = WAVE.overrideFun(object.about, function(){ return this.baseFunction() + "overridden" });
published.overrideFunction = function (original, fn) {
  var superFunction = original;
  return function () {
    this.baseFunction = superFunction;
    return fn.apply(this, arguments);
  };
};

//Mixin behavior - extend obj with properties of ext. keepExisting=true preserves existing object key, even if it is null
published.extend = function (obj, ext, keepExisting) {
  var prop;
  if (!keepExisting) {
    for (prop in ext)
      if (ext.hasOwnProperty(prop))
        obj[prop] = ext[prop];
  } else {
    for (prop in ext)
      if (ext.hasOwnProperty(prop) && !obj.hasOwnProperty(prop))
        obj[prop] = ext[prop];
  }
  return obj;
};

// deep clones data object (not functions)
published.clone = function (obj) {
  return JSON.parse(JSON.stringify(obj));
};

// deep clones data object, optionally setting all keys to lower case
published.memberClone = function (obj, lowerCaseKeys) {
  var result;
  if (!published.isObject(obj)) {
    if (!published.isArray(obj)) return obj;
    result = [];
    var l = obj.length;
    for (var i = 0; i < l; i++)
      result.push(published.memberClone(obj[i], lowerCaseKeys));
    return result;
  }

  result = {};
  for (var n in obj) result[lowerCaseKeys ? n.toLowerCase() : n] = published.memberClone(obj[n], lowerCaseKeys);
  return result;
};

// returns true if both objects represent the same scalar value or complex structure/map
// that is keys/values of maps/arrays. Nulls are considered equivalent
published.isSame = function (obj1, obj2) {
  if (arguments.length < 2) return false;
  if (obj1 === null && obj2 === null) return true;
  if (obj1 === null || obj2 === null) return false;
  if (typeof (obj1) !== typeof (obj2)) return false;

  if (typeof (obj1.getTime) === "function")//Date requires special handling
    return obj1.getTime() === obj2.getTime();

  if (published.isMapOrArray(obj1)) {
    if (obj1.length !== obj2.length ||
      Object.keys(obj1).length !== Object.keys(obj2).length) return false;
    for (var i in obj1)
      if (!published.isSame(obj1[i], obj2[i])) return false;
    return true;
  }

  return obj1 === obj2;
};

//Checks object property for string value and if it is converts it to object (map)
//Does nothing if prop does not exist, is null or not a string value
published.propStrAsObject = function (obj, prop) {
  if (obj === null) return;
  if (published.strEmpty(prop)) return;
  if (!obj.hasOwnProperty(prop)) return;
  var val = obj[prop];
  if (val === null) return;
  if (typeof (val) === "string") {
    try { obj[prop] = JSON.parse(val); }
    catch (e) {
      console.error("WV.propStrAsObject, error parsing property '" + prop + "' string as JSON: " + val);
      throw e;
    }
  }
};

//Tries to parse string as json, passing through objects and arrays
published.tryParseJSON = function (content, dflt) {
  if (typeof (content) !== tUNDEFINED && content !== null) {
    if (published.isMapOrArray(content))
      return { ok: true, obj: content };
    try {
      return { ok: true, obj: JSON.parse(content) };
    } catch (e) { }
  }

  return { ok: false, obj: typeof (dflt) !== tUNDEFINED ? dflt : {} };
};

//returns true if object has no duplicated keys
published.checkKeysUnique = function (obj) {
  if (!published.isObject(obj)) return obj;

  var keys = Object.keys(obj);
  for (var i = 0; i < keys.length; ++i)
    for (var j = i + 1; j < keys.length; ++j)
      if (keys[i].toLowerCase() === keys[j].toLowerCase()) return true;

  return false;
};

//true if object has no keys
published.empty = function (obj) {
  if (typeof (obj) === tUNDEFINED || obj === null) return true;
  for (var n in obj) return false;
  return true;
};

//Test if object has its own property
published.has = function (obj, prop) {
  return obj ? hasOwnProperty.call(obj, prop) : false;
};

//Reads obj prop OR it doesnt exist return default or null, but never undefined
published.get = function (obj, prop, dflt) {
  if (typeof (obj) !== tUNDEFINED &&
    obj !== null &&
    typeof (prop) !== tUNDEFINED &&
    prop !== null &&
    published.has(obj, prop) &&
    typeof (obj[prop]) !== tUNDEFINED) return obj[prop];

  return (typeof (dflt) === tUNDEFINED) ? null : dflt;
};

published.tryParseInt = function (val, allowReal) {
  var value;
  if (typeof (val) === tUNDEFINED || val === null || val.length === 0)
    value = NaN;
  else
    value = Number(val);

  var ok;
  if (allowReal) {
    ok = !isNaN(value) && isFinite(value);
    if (ok) value = value < 0 ? (-Math.floor(-value)) : Math.floor(value);
  } else {
    ok = published.isFunction(Number.isInteger) ?
      Number.isInteger(value) :
      !isNaN(value) && isFinite(value) && (Math.floor(value) === value);
  }
  return { ok: ok, value: value };
};

published.intValid = function (val) {
  return published.tryParseInt(val).ok;
};

published.intValidPositive = function (val) {
  var ival = published.tryParseInt(val);
  return ival.ok && ival.value > 0;
};

published.intValidPositiveOrZero = function (val) {
  var ival = published.tryParseInt(val);
  return ival.ok && ival.value >= 0;
};

published.formatMoney = function (amount, d, t) {
  var pObject = published.tryParseInt(amount, true);

  if (!pObject.ok)
    amount = 0;

  d = typeof (d) === tUNDEFINED ? '.' : d;
  t = typeof (t) === tUNDEFINED ? ',' : t;

  // in javascript
  // 47.87*100=4787, BUT! 37.87*100=3786.9999999999995
  var a1 = amount * 100;
  var a2 = Math.round(a1);
  amount = (Math.abs(a1 - a2) < 0.0000001) ? a2 : (amount < 0 ? -Math.floor(-a1) : Math.floor(a1));
  amount = (amount / 100).toFixed(2);

  if (d !== '.') amount = amount.replace('.', d);

  return amount.replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1" + t);
};



    published.each = function (obj, func) {
      if (typeof (obj) === tUNDEFINED || obj === null) return null;
      if (!published.isFunction(func)) return obj;
      for (var i in obj) func(obj[i], i, obj);
      return obj;
    };

    // Returns true for scalar vars and false for arrays and objects
    published.isScalar = function(value) {
      return (/boolean|number|string/).test(typeof(value));
    };

 published.isObjectType = function(tp) { return published.strOneOf(tp, ["object", "json", "map", "array"]);};
    published.isIntType = function(tp) { return published.strOneOf(tp, ["int", "integer"]);};
    published.isRealType = function(tp) { return published.strOneOf(tp, ["float", "real", "double", "money"]);};
    published.isBoolType = function(tp) { return published.strOneOf(tp, ["bool", "boolean", "logical"]);};
    published.isStringType = function(tp) { return published.strOneOf(tp, ["str", "string", "char[]", "char", "varchar", "text"]);};
    published.isDateType = function(tp) { return published.strOneOf(tp, ["date", "datetime", "time", "timestamp"]);};

    published.isSimpleKeyStringMap = function (obj) {
      if (!published.exists(obj) || !published.isObject(obj)) return false;
      var keys = Object.keys(obj);
      for (var i in keys) {
        var key = keys[i];
        var val = obj[key];
        if (typeof(val) === tUNDEFINED) return false;
        if (val !== null && !published.isString(val)) return false;
      }

      return true;
    };

    //Converts scalar value into the specified type: convertScalarType("12/14/2018", "date", true);
    published.convertScalarType = function(nullable, value, type, dflt){

         function dfltOrError(){
            if (typeof(dflt)!==tUNDEFINED && dflt!==null) return dflt;
            if (value===null) value = '<null>';
            throw "Can not convert '"+value+"' to type '"+type+"'";
         }

        if (published.strEmpty(type)) return value;

        var t;

        if (published.isObjectType(type)){
            if (value===null) return nullable ?  null : {};
            if (published.isObject(value)) return value;
            t = typeof(value);
            if (t==="boolean") return value ? {"value": true} : {"value": false};
            if (published.isFunction(value.getTime) || t==="number") return {"value": value};
            if (t==="string"){
               try { return JSON.parse(value); }
               catch(e){ return {"value": value};}
            }
            return dfltOrError();
        }
        else if (published.isIntType(type)){
            if (value===null) return nullable ?  null : 0;
            t = typeof(value);
            if (t==="boolean") return value ? 1 : 0;
            if (published.isFunction(value.getTime)) return value.getTime();

            if (t==="number" || t==="object"){
                t="string";
                value = value.toString();
            }
            if (t==="string"){
                if (published.strEmpty(value)) return nullable ? null : 0;
                var i = published.tryParseInt(value, true);
                if (i.ok) return i.value;
                return dfltOrError();
            }
            return dfltOrError();
        }
        else if (published.isStringType(type)){
            if (value===null) return nullable ?  null : "";
            t = typeof(value);
            if (t==="string") return value;
            if (t==="boolean") return value ? "true" : "false";

            if (published.isFunction(value.getTime)) return published.toUSDateTimeString(value);

            return value.toString();
        }
        else if (published.isRealType(type)){
            if (value===null) return nullable ?  null : 0.0;
            t = typeof(value);
            if (t==="boolean") return value ? 1.0 : 0.0;
            if (published.isFunction(value.getTime)) return value.getTime();

            if (t==="number" || t==="object"){
                t="string";
                value = value.toString();
            }
            if (t==="string"){
                if (published.strEmpty(value)) return nullable ? null : 0;
                var num = parseFloat(value);
                if (!isNaN(num)) return num;
                return dfltOrError();
            }

            return dfltOrError();
        }
        else if (published.isBoolType(type)){
            if (value) return true;
            return false;
        }
        else if (published.isDateType(type)){
            if (value===null) return nullable ?  null : dfltOrError();
            t = typeof(value);
            if (t==="number") return new Date(Math.round(value));

            if (published.strEmpty(value)&&nullable) return null;

            var ms = Date.parse(value);
            if (!isNaN(ms)) return new Date(ms);

            return dfltOrError();
        }


        return dfltOrError();
    };//convertType