"use strict";
/*jshint devel: true,browser: true, sub: true */
/*global escape: true */

/*!
 * Wave Java Script Library Core v2.0.0
 *
 * Based on IT Adapter JS Library 2002-2011
 * License: Unrestricted use/modification with mandatory reference to IT Adapter Corp. as the original author
 * (c) 2002-2011, 2013-2014 IT Adapter Corp.
 * http://www.itadapter.com/
 * Authors: Dmitriy Khmaladze,
 *          Timur Shemsedinov
 * Revision Epoch:  May 1, 2014; Jan 8, 2011; 2007-2009; Mar-Apr 2006; Jan 2005...
 */
var WAVE = (function(){

    var tUNDEFINED = "undefined";

    if (!Date.now) {
      Date.now = function() { return new Date().getTime(); };
    }

    var published = {
      TUNDEFINED: tUNDEFINED
    };

    published.falseness = function() { return false; };
    published.falsefunc = function() { return false; };

    published.arrayDelete = function(array, element) {
      for (var i in array){
        if (array[i] === element){
          array.splice(i, 1);
          return true;
        }
      }
        return false;
    };

    published.arrayShallowCopy = function(source){
       var copy = [];
       if (source && source.length>0)
        for (var i=0, max=source.length; i < max; i++) copy.push(source[i]);
       return copy;
    };

    published.arrayClear = function(array) {  while(array.length > 0) array.pop(); };

    published.mergeArrays = function(left, right, matcher, transform) {
      if(!published.isArray(left) && !published.isArray(right)) return [];

      var m = published.isFunction(matcher) ? matcher : function(a, b) { return a === b; };
      var t = published.isFunction(transform) ? transform : function(a){ return a; };

      if(!published.isArray(left)) left = right;
      if(!published.isArray(right)) right = left;

      var a = left.concat(right);
      for(var i = 0; i < a.length; i++) {
        for(var j = i + 1; j < a.length; j++) {
          if (m(a[i],a[j]))
            a.splice(j--, 1);
        }
        a[i] = t(a[i]);
      }
      return a;
    };

    published.inArray = Array.prototype.indexOf ?
                          function(array, value) { return array.indexOf(value) !== -1;} :
                          function(array, value) {
                              var i = array.length;
                              while (i--) if (array[i] === value) return true;
                              return false;
                          };

    published.isString = function(obj){ return Object.prototype.toString.call(obj) === '[object String]'; };

    //Returns true when the passed parameter is a map, not an array or function
    published.isObject  = function(obj){
     if (typeof(obj)===tUNDEFINED) return false;
     return obj === Object(obj) && !published.isArray(obj) && !published.isFunction(obj);
    };

    //Returns true when the passed parameter is an array, not a map or function
    published.isArray   = function(obj){
     if (typeof(obj)===tUNDEFINED) return false;
     return Object.prototype.toString.call(obj) === '[object Array]';
    };

    //Returns true when poassed parameter is a function, not a map object or an array
    published.isFunction = function(obj){
     return typeof(obj)==="function";
    };

    //Returns true when the passed parameter is an array, or map but not a function
    published.isMapOrArray   = function(obj){
     return obj === Object(obj) && !published.isFunction(obj);
    };

    //Overrides existing function by wrapping in new one. May call base like so:
    //  object.about = WAVE.overrideFun(object.about, function(){ return this.baseFunction() + "overridden" });
    published.overrideFunction = function(original, fn){
     var superFunction = original;
    return function() {
      this.baseFunction = superFunction;
      return fn.apply(this, arguments);
      };
    };

    published.each = function (obj, func) {
      if (typeof(obj) === tUNDEFINED || obj === null) return null;
      if (!published.isFunction(func)) return obj;
      for (var i in obj) func(obj[i], i, obj);
      return obj;
    };

    //Mixin behavior - extend obj with properties of ext. keepExisting=true preserves existing object key, even if it is null
    published.extend = function(obj, ext, keepExisting) {
        var prop;
        if (!keepExisting){
            for (prop in ext)
              if (ext.hasOwnProperty(prop))
                 obj[prop] = ext[prop];
        }else{
            for (prop in ext)
              if (ext.hasOwnProperty(prop) && !obj.hasOwnProperty(prop))
                 obj[prop] = ext[prop];
        }
        return obj;
    };

    // deep clones data object (not functions)
    published.clone = function(obj){
        return JSON.parse( JSON.stringify(obj) );
    };

    // deep clones data object, optionally setting all keys to lower case
    published.memberClone = function(obj, lowerCaseKeys){
      var result;
      if (!published.isObject(obj)) {
        if(!published.isArray(obj)) return obj;
        result = [];
        var l = obj.length;
        for(var i = 0; i < l; i++)
          result.push(published.memberClone(obj[i], lowerCaseKeys));
        return result;
      }

      result = {};
      for(var n in obj) result[lowerCaseKeys ? n.toLowerCase() : n] = published.memberClone(obj[n], lowerCaseKeys);
      return result;
    };

    // returns true if both objects represent the same scalar value or complex structure/map
    // that is keys/values of maps/arrays. Nulls are considered equivalent
    published.isSame = function(obj1, obj2){
        if (arguments.length<2) return false;
        if (obj1===null && obj2===null) return true;
        if (obj1===null || obj2===null) return false;
        if (typeof(obj1)!==typeof(obj2)) return false;

        if (typeof(obj1.getTime)==="function")//Date requires special handling
            return obj1.getTime()===obj2.getTime();

        if (published.isMapOrArray(obj1)){
            if (obj1.length!==obj2.length ||
                Object.keys(obj1).length!==Object.keys(obj2).length) return false;
            for(var i in obj1)
              if (!published.isSame(obj1[i], obj2[i])) return false;
            return true;
        }

        return obj1===obj2;
    };

    //Checks object property for string value and if it is converts it to object (map)
    //Does nothing if prop does not exist, is null or not a string value
    published.propStrAsObject = function(obj, prop){
        if (obj===null) return;
        if (published.strEmpty(prop)) return;
        if (!obj.hasOwnProperty(prop)) return;
        var val = obj[prop];
        if (val===null) return;
        if (typeof(val)==="string")
        {
            try{ obj[prop] = JSON.parse(val); }
            catch(e)
            {
                console.error("WV.propStrAsObject, error parsing property '"+prop+"' string as JSON: " + val);
                throw e;
            }
        }
    };

    //Tries to parse string as json, passing through objects and arrays
    published.tryParseJSON = function(content, dflt) {
        if (typeof(content)!==tUNDEFINED && content!==null)
        {
          if (published.isMapOrArray(content))
            return {ok: true, obj: content};
          try {
            return {ok: true, obj: JSON.parse(content)};
          } catch(e){}
        }

        return {ok: false, obj: typeof(dflt)!==tUNDEFINED ? dflt : {}};
    };

    //returns true if object has no duplicated keys
    published.checkKeysUnique = function(obj){
      if (!published.isObject(obj)) return obj;

      var keys = Object.keys(obj);
      for(var i=0; i < keys.length; ++i)
        for(var j=i+1; j < keys.length; ++j)
          if (keys[i].toLowerCase() === keys[j].toLowerCase()) return true;

      return false;
    };

    //true if object has no keys
    published.empty = function(obj) {
        if (typeof(obj)===tUNDEFINED || obj===null) return true;
        for(var n in obj) return false;
        return true;
    };

    //Test if object has its own property
    published.has = function(obj, prop) {
        return obj ? hasOwnProperty.call(obj, prop) : false;
    };

    //Reads obj prop OR it doesnt exist return default or null, but never undefined
    published.get = function(obj, prop, dflt) {
        if (typeof(obj)!==tUNDEFINED &&
            obj!==null &&
            typeof(prop)!==tUNDEFINED &&
            prop!==null &&
            published.has(obj, prop) &&
            typeof(obj[prop])!==tUNDEFINED) return obj[prop];

        return(typeof(dflt)===tUNDEFINED) ? null : dflt;
    };

    published.tryParseInt = function(val, allowReal) {
      var value;
      if (typeof(val) === tUNDEFINED || val === null || val.length === 0)
        value = NaN;
      else
        value = Number(val);

      var ok;
      if (allowReal) {
        ok = !isNaN(value) && isFinite(value);
        if (ok) value = value < 0 ? (-Math.floor(-value)) : Math.floor(value);
      } else {
        ok = WAVE.isFunction(Number.isInteger) ?
          Number.isInteger(value) :
          !isNaN(value) && isFinite(value) && (Math.floor(value) === value);
      }
      return {ok: ok, value: value};
    };

    published.intValid = function(val) {
        return published.tryParseInt(val).ok;
    };

    published.intValidPositive = function(val) {
        var ival = published.tryParseInt(val);
        return ival.ok && ival.value > 0;
    };

    published.intValidPositiveOrZero = function(val) {
        var ival = published.tryParseInt(val);
        return ival.ok && ival.value >= 0;
    };

    published.formatMoney = function(amount, d, t){
      d = typeof(d)===tUNDEFINED ? '.' : d;
      t = typeof(t)===tUNDEFINED ? ',' : t;
      amount =  amount < 0 ?(-Math.floor(100 * -amount) / 100).toFixed(2)
                               :(Math.floor(100 * amount) / 100).toFixed(2);

      if (d!=='.') amount = amount.replace('.', d);

      return amount.replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1"+t);
    };

    published.strEmpty = function(str){ return ( !str  ||  0 === str.length  ||  /^\s*$/.test(str) ); };


    published.strAsBool = function(str, dflt){
      if (typeof(str)===tUNDEFINED||str===null) return ( (typeof(dflt)===tUNDEFINED) ? false : dflt  );
      str = str.toString();
      return published.strOneOf(str, ["true", "t", "yes", "1"]);
    };


    published.strDefault = function(str, dflt){
     return typeof(str)===tUNDEFINED||str===null ? (typeof(dflt)===tUNDEFINED||dflt===null?'':dflt) : str;
    };

    published.strEmptyDefault = function(str, dflt){
     return published.strEmpty(str) ? (typeof(dflt)===tUNDEFINED||dflt===null?'':dflt) : str;
    };

    published.nlsNameDefault = function(nls, dflt){
     var v = (typeof(nls)===tUNDEFINED||nls===null) ? null : nls.n;
     return published.strEmptyDefault(v, dflt);
    };

    published.nlsDescrDefault = function(nls, dflt){
     var v = (typeof(nls)===tUNDEFINED||nls===null) ? null : nls.d;
     return published.strEmptyDefault(v, dflt);
    };

    published.nlsNameOrDescrDefault = function(nls, dflt){
     var v = null;
     if (typeof(nls)!==tUNDEFINED&&nls!==null)
     {
       v = nls.n;
       if (published.strEmpty(v)) v = nls.d;
     }
     return published.strEmptyDefault(v, dflt);
    };

    published.nlsDescrOrNameDefault = function(nls, dflt){
     var v = null;
     if (typeof(nls)!==tUNDEFINED&&nls!==null)
     {
       v = nls.d;
       if (published.strEmpty(v)) v = nls.n;
     }
     return published.strEmptyDefault(v, dflt);
    };


    published.strTrim = function(str){  return str.replace(/^\s+|\s+$/g, ''); };
    published.strLTrim = function(str){  return str.replace(/^\s+/,''); };
    published.strRTrim = function(str){  return str.replace(/\s+$/,''); };

    // Truncates str if its length exceeds maxLen and adds endWith string to result end.
    published.strTrunc = function(str, maxLen, endWith) {
      if (!str) return str;
      var len = str.length;
      if (len <= maxLen) return str;
      endWith = endWith || "...";
      return str.substr(0, maxLen - endWith.length) + endWith;
    };

    //Capitalizes first chars after spaces or dots, otionally converting chars in between to lower case
    published.strCaps = function(str, norm){
        //this does not use Regexp because regexp does not detect words correctly for non-english languages
        if (published.strEmpty(str)) return str;
        var c;
        var result = "";
        var sp = false;
        for(var i=0; i<str.length; i++)
        {
           c = str[i];
           if (c===' '||c==='.') {
             sp = true;
             result+=c;
             continue;
           }

           if (sp||i===0)
             result += c.toUpperCase();
           else
             result += norm ? c.toLowerCase() : c;
           sp = false;
        }
        return result;
    };

    published.markup = (function () {
      var State = {
        NONE:           0,
        PARAGRAPH:      1,
        PARAGRAPH_NEXT: 2,
        SPAN:           3,
        SPAN_END:       4,
        CLASS:          5,
        CLASS_NEXT:     6,
        HEADING:        7,
        HEADING_BODY:   8,
        LIST:           9,
        LIST_ITEM:     10,
        LIST_NEXT:     11,
        KEY:           12,
        KEY_BODY:      13,
        VALUE:         14,
        VALUE_BODY:    15,
        KEY_NEXT:      16
      };
      var entities = {
        ' ': '&nbsp;',
        '!': '&#33;',
        '#': '&#35;',
        '$': '&#36;',
        '*': '&#42;',
        '<': '&lt;',
        '=': '&#61;',
        '>': '&gt;',
        '{': '&#123;',
        '}': '&#125;'
      };
      return function (str) {
        var out = '';
        var state = State.NONE;
        var stack = [];
        var out_stack = [];
        var level = 0;
        var type;
        var type_last;
        var list_stack = [];
        var in_kv;

        for(var i = 0, length = str.length; i < length; i++) {
          var c = str.charAt(i);
          var n = str.charAt(i+1);
          switch (c) {
            case '\r': if (n === '\n') i++;
            case '\n': c = '\n'; break;
            case '<': c = '&lt;'; break;
            case '>': c = '&gt;'; break;
            case '&':
              if (str.charAt(i+2) !== ';') break;
              var ent = entities[n];
              if (typeof(ent) !== tUNDEFINED) c = ent;
              i+=2;
              break;
          }

          switch (state) {
            case /*State.NONE*/ 0:
            case /*State.PARAGRAPH_NEXT*/ 2:
            case /*State.LIST_NEXT*/ 11:
            case /*State.KEY_NEXT*/ 16: {
              switch (c) {
                case '\n':
                  if (state === State.PARAGRAPH_NEXT) out += '</p>';
                  else {
                    if (state === State.LIST_NEXT) close_list(0);
                    if (state === State.KEY_NEXT) close_kv(false);
                  }
                  state = State.NONE;
                  break;
                case '{':
                  if (state === State.PARAGRAPH_NEXT) out += ' ';
                  else {
                    if (state === State.LIST_NEXT) close_list(0);
                    if (state === State.KEY_NEXT) close_kv(false);
                    out += '<p>';
                  }
                  stack.push(State.PARAGRAPH);
                  state = State.SPAN;
                  out_stack.push(out);
                  out = c;
                  break;
                case '!':
                  if (state === State.PARAGRAPH_NEXT) {
                    state = State.PARAGRAPH;
                    out += ' ' + c;
                  } else {
                    if (state === State.LIST_NEXT) close_list(0);
                    if (state === State.KEY_NEXT) close_kv(false);
                    state = State.HEADING;
                    out_stack.push(out);
                    out = c;
                  }
                  break;
                case '#':
                case '*':
                  if (state === State.PARAGRAPH_NEXT) {
                    state = State.PARAGRAPH;
                    out += ' ' + c;
                  } else {
                    if (state === State.KEY_NEXT) close_kv(false);
                    type = c;
                    state = State.LIST;
                    out_stack.push(out);
                    out = c;
                  }
                  break;
                case '$':
                  if (state === State.PARAGRAPH_NEXT) {
                    state = State.PARAGRAPH;
                    out += ' ' + c;
                  } else {
                    if (state === State.KEY_NEXT) close_kv(true);
                    in_kv = state === State.KEY_NEXT;
                    state = State.KEY;
                    out_stack.push(out);
                    out = c;
                  }
                  break;
                default:
                  if (state === State.PARAGRAPH_NEXT) out += ' ';
                  else {
                    if (state === State.LIST_NEXT) close_list(0);
                    if (state === State.KEY_NEXT) close_kv(false);
                    out += '<p>';
                  }
                  state = State.PARAGRAPH;
                  out += c;
                  break;
              }
            } break;
            case /*State.PARAGRAPH*/ 1: {
              switch (c) {
                case '\n':
                  state = State.PARAGRAPH_NEXT;
                  break;
                case '{':
                  stack.push(state);
                  state = State.SPAN;
                  out_stack.push(out);
                  out = c;
                  break;
                default:
                  out += c;
                  break;
              }
            } break;
            case /*State.SPAN*/ 3: {
              switch (c) {
                case '\n':
                  out += ' ';
                  break;
                case '{':
                  stack.push(state);
                  state = State.SPAN;
                  out_stack.push(out);
                  out = c;
                  break;
                case '}':
                  state = State.SPAN_END;
                  out = out.substr(1);
                  break;
                default: out += c; break;
              }
            } break;
            case /*State.SPAN_END*/ 4: {
              switch (c) {
                case '.':
                  if (class_start(n)) {
                    state = State.CLASS;
                    out_stack.push(out);
                    out = c;
                    break;
                  }
                default:
                  state = stack.pop();
                  out = out_stack.pop() + out;
                  i--;
                  break;
              }
            } break;
            case /*State.CLASS*/ 5: {
              out += c;
              if (n === '.') state = State.CLASS_NEXT;
              else if (!class_midle(n)) {
                state = stack.pop();
                out = '<span class="' + make_class(out) + '">'
                    + out_stack.pop() + '</span>';
                out = out_stack.pop() + out;
              }
            } break;
            case /*State.CLASS_NEXT*/ 6: {
              switch (c) {
                case '.':
                  if (class_start(n)) {
                    state = State.CLASS;
                    out += c;
                    break;
                  }
                default:
                  state = stack.pop();
                  out = '<span class="' + make_class(out) + '">'
                      + out_stack.pop() + '</span>.';
                  out = out_stack.pop() + out;
                  break;
              }
            } break;
            case /*State.HEADING*/ 7: {
              if (c === '\n') {
                out = out_stack.pop() + '<p>' + out;
                state = State.PARAGRAPH;
                i--;
                break;
              }
              if (c === '!' && out.length < 6) out += c;
              else {
                level = out.length;
                out = out_stack.pop() + '<h' + level + '>';
                state = State.HEADING_BODY;
                switch (c) {
                  case '{':
                    stack.push(state);
                    state = State.SPAN;
                    out_stack.push(out);
                    out = c;
                    break;
                  default:
                    out += c;
                    break;
                }
              }
            } break;
            case /*State.HEADING_BODY*/ 8: {
              switch (c) {
                case '\n':
                  state = State.NONE;
                  out += '</h' + level + '>';
                  level = 0;
                  break;
                case '{':
                  stack.push(state);
                  state = State.SPAN;
                  out_stack.push(out);
                  out = '{';
                  break;
                default:
                  out += c;
                  break;
              }
            } break;
            case /*State.LIST*/ 9: {
              if (c === '\n') {
                out = out_stack.pop() + '<p>' + out;
                state = State.PARAGRAPH;
                i--;
                break;
              }
              if (c === type ) out += c;
              else {
                state = State.LIST_ITEM;
                var new_level = out.length;
                out = out_stack.pop();
                if (level === new_level && type !== type_last)
                  close_list(0);
                if (level === new_level) {
                  out += '</li><li>';
                } else if (level < new_level) {
                  for (; level < new_level; level++) {
                    switch (type) {
                      case '#': out += '<ol><li>'; list_stack.push('ol'); break;
                      case '*': out += '<ul><li>'; list_stack.push('ul'); break;
                    }
                  }
                } else close_list(new_level);
                switch (c) {
                  case '{':
                    stack.push(state);
                    state = State.SPAN;
                    out_stack.push(out);
                    out = c;
                    break;
                  default:
                    out += c;
                    break;
                }
                break;
              }
            } break;
            case /*State.LIST_ITEM*/ 10: {
              switch (c) {
                case '\n':
                  type_last = type;
                  state = State.LIST_NEXT;
                  break;
                case '{':
                  stack.push(state);
                  state = State.SPAN;
                  out_stack.push(out);
                  out = '{';
                  break;
                default:
                  out += c;
                  break;
              }
            } break;
            case /*State.KEY*/ 12: {
              if (c === '\n' || c === '=') {
                out = out_stack.pop() + (in_kv ? '</dl>' : '') + '<p>' + out;
                state = State.PARAGRAPH;
                i--;
                break;
              }
              state = State.KEY_BODY;
              switch (c) {
                case '{':
                  stack.push(state);
                  state = State.SPAN;
                  out_stack.push(out);
                  out = c;
                  break;
                default:
                  out += c;
                  break;
              }
            } break;
            case /*State.KEY_BODY*/ 13: {
              if (c === '\n') {
                out = out_stack.pop() + (in_kv ? '</dl>' : '') + '<p>' + out;
                state = State.PARAGRAPH;
                i--;
                break;
              }
              switch (c) {
                case '{':
                  stack.push(state);
                  state = State.SPAN;
                  out_stack.push(out);
                  out = c;
                  break;
                case '=':
                  state = State.VALUE;
                  out_stack.push(out);
                  out = c;
                  break;
                default:
                  out += c;
                  break;
              }
            } break;
            case /*State.VALUE*/ 14: {
              if (c === '\n') {
                out = '<p>' + out_stack.pop() + out;
                out = out_stack.pop() + (in_kv ? '</dl>' : '') + out;
                state = State.PARAGRAPH;
                i--;
                break;
              }
              out = '<dt>' + out_stack.pop().substr(1) + '</dt><dd>';
              out = (in_kv ? out_stack.pop() : out_stack.pop() + '<dl>') + out;
              state = State.VALUE_BODY;
              switch (c) {
                case '{':
                  stack.push(state);
                  state = State.SPAN;
                  out_stack.push(out);
                  out = c;
                  break;
                default:
                  out += c;
                  break;
              }
            } break;
            case /*State.VALUE_BODY*/ 15: {
              switch (c) {
                case '\n':
                  state = State.KEY_NEXT;
                  break;
                case '{':
                  stack.push(state);
                  state = State.SPAN;
                  out_stack.push(out);
                  out = '{';
                  break;
                default:
                  out += c;
                  break;
              }
            } break;
            default: throw 'WAVE.markup(state)';
          }
        }

        stack.push(state);

        while(typeof(state = stack.pop()) !== tUNDEFINED) {
          switch(state) {
            case /*State.NONE*/ 0: break;
            case /*State.PARAGRAPH*/ 1:
            case /*State.PARAGRAPH_NEXT*/ 2: out += '</p>'; break;
            case /*State.VALUE*/ 14: out = out_stack.pop() + out;
            case /*State.KEY*/ 12:
            case /*State.KEY_BODY*/ 13: if (in_kv) out_stack.push(out_stack.pop() + '</dl>');
            case /*State.LIST*/ 9:
            case /*State.HEADING*/ 7: out = '<p>' + out + '</p>';
            case /*State.SPAN*/ 3:
            case /*State.SPAN_END*/ 4: out = out_stack.pop() + out; break;
            case /*State.HEADING_BODY*/ 8: out += '</h' + level + '>'; break;
            case /*State.LIST_ITEM*/ 10: close_list(0); break;
            case /*State.VALUE_BODY*/ 15:
            case /*State.KEY_NEXT*/ 16: close_kv(false); break;
            default: throw 'WAVE.markup(last.state)';
          }
        }

        return out;

        function close_list(new_level) {
          var lst;
          while(level > new_level && typeof(lst = list_stack.pop()) !== tUNDEFINED) {
            out += '</li></' + lst + '>';
            level--;
          }
          if (level !== 0) out += '</li><li>';
        }
        function close_kv(in_kv) {
          out += '</dd>';
          if (!in_kv) out += '</dl>';
        }
      };

      function class_start(c) {
        return '_' === c
          || ('A' <= c && c <= 'Z')
          || ('a' <= c && c <= 'z');
      }

      function class_midle(c) {
        return '_' === c
          || '-' === c
          || ('A' <= c && c <= 'Z')
          || ('a' <= c && c <= 'z')
          || ('0' <= c && c <= '9');
      }

      function make_class(str) {
        var cls = str.substr(1).split(".");
        var uni = {};
        for (var i in cls) uni[cls[i]] = 0;
        cls = [];
        for (var k in uni) cls.push('wv-markup-'+ k.toLowerCase());
        return cls.join(' ');
      }
    })();


    var intPrefixes = ["", "k", "M", "G", "T", "P", "E", "Z", "Y"];
    var floatPrefixes = ["", "m", "µ", "n", "p", "f", "a", "z", "y"];

    var siPrefixes = ["y", "z", "a", "f", "p", "n", "µ", "m", "", "k", "M", "G", "T", "P", "E", "Z", "Y"];

    // converts num to its string representation in SI (Le Système International d’Unités, SI) with precision desired
    // so 1000 = "1.00k", .1="100.00m", 23.55 = "23.55", 999.999="1.00k"
    published.siNum = function (num, decimalPlaces) {

      if (typeof (decimalPlaces) === tUNDEFINED) decimalPlaces = 2;

      if (num === 0) return num.toFixed(decimalPlaces);

      var n = num;
      if (num < 0) n = -n;

      var k = 0;
      var res = n.toFixed(decimalPlaces) + siPrefixes[k + 8];

      while (n >= 1000) { n /= 1000; k++; }
      while (n < 1) { n *= 1000; k--; }

      var roundK = Math.pow(10, decimalPlaces);

      n = Math.round(n * roundK) / roundK;

      while (n >= 1000) { n /= 1000; k++; }
      while (n < 1) { n *= 1000; k--; }

      if (num < 0) n = -n;
      res = n.toFixed(decimalPlaces) + siPrefixes[k + 8];

      return res;
    };

    //True for [a-zA-Z0-9]
    published.charIsAZLetterOrDigit = function(c){
        if (c===null) return false;
        return (c>='a' && c<='z') || (c>='A' && c<='Z') || (c>='0' && c<='9');
    };


    published.strStartsWith = function(str, s, scase){
     return scase ? str.slice(0, s.length) === s : str.slice(0, s.length).toLowerCase() === s.toLowerCase();
    };

    published.strEndsWith = function(str, s, scase){
      return scase ? str.slice(-s.length) === s : str.slice(-s.length).toLowerCase() === s.toLowerCase();
    };

    // Ensures that string ends with the specified string: strEnsureEnding("path",'/')
    published.strEnsureEnding = function(str, ending) {
      return str+(str.slice(-ending.length) === ending ? '' : ending);
    };

    //Returns true when str contains a seg optionally respecting case
    published.strContains = function(str, seg, scase) {
      return scase ? str.indexOf(seg)>-1 : str.toLowerCase().indexOf(seg.toLowerCase())>-1;
    };

    //Returns true if both string contain the same trimmed case-insensitive value.
    //This method is usefull for tasks like searches of components by name
    published.strSame = function(str1, str2){
      if (typeof(str1)===tUNDEFINED || typeof(str2)===tUNDEFINED) return false;
      if (str1===null || str2===null) return false;
      return published.strTrim(str1).toLowerCase() === published.strTrim(str2).toLowerCase();
    };

    //Returns true if the case-insensitive trimmed string is in the set of values
    //Neither string nor set value may contain delimiter which is '|' by default:
    //   strOneOf("car",["car","house","tax"],';')
    published.strOneOf = function(str1, set, del){
      if (str1===null || set===null || !published.isArray(set)) return false;
      if (!del) del = "|";
      str1 = del+published.strTrim(str1).toLowerCase()+del;
      var vset = (del+set.join(del)+del).toLowerCase();
      return vset.indexOf(str1)>=0;
    };

    //returns true if an element is a direct or indirect child of the specified parent
    published.isParentOf = function(parent, elem){
      if (WAVE.isFunction(parent.contains)) {
        try {return parent.contains(elem);}
        catch(e) {return false;}
      }

      var node = elem;
      while(true){
        node = node.parentNode;
        if(node === null) return false;
        if(node === parent) return true;
      }
    };

    //returns computed value of specified css style for given elemen
    published.styleOf = function(elem, cssStyle){
      if (typeof(elem) === tUNDEFINED || elem === null || published.strEmpty(cssStyle) || !WAVE.isFunction(window.getComputedStyle))
        return "";

      return window.getComputedStyle(elem, null).getPropertyValue(cssStyle);
    };

    //removes html element with given id
    published.removeElem = function(id) {
      var el = WAVE.id(id);
      if (el !== null){
        el.parentNode.removeChild(el);
        return true;
      }
      return false;
    };

    published.addClass = function(elem, className) {
      if (typeof(elem) === tUNDEFINED || elem === null || published.strEmpty(className)) return;

      published.removeClass(elem, className);
      elem.className += (" " + className);
    };

    published.removeClass = function(elem, className) {
      if (typeof(elem) === tUNDEFINED || elem === null || published.strEmpty(className)) return;

      elem.className = elem.className.replace(new RegExp('(?:^|\\s)' + className + '(?!\\S)', "g") , '' );
    };

    var htmlEscapes = {
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': '&quot;',
        "'": '&#39;',
        "/": '&#x2F;'
        };

    published.strEscapeHTML = function(content) {
        return String(content).replace(/[&<>"'\/]/g, function (esc) { return htmlEscapes[esc]; });
    };


    //Turns content like ' <td>@name@</td> ' -> '<td> Alex &amp; Boris </td>' provided that a = 'Alex & Boris'. Data is HTML escaped
    published.strHTMLTemplate = function(tpl, args) {
      return tpl.replace(/@([\-\.0-9a-zA-Z]+)@/g, function(s, key) { return published.strEscapeHTML(args[key]); });
  };

    //Turns content like ' {a: "@name@"} ' -> '{a: "Alex & Boris"}' provided that a = 'Alex & Boris'. Data is not HTML escaped
    published.strTemplate = function(tpl, args) {
      return tpl.replace(/@([\-\.0-9a-zA-Z]+)@/g, function(s, key) { return args[key]; });
  };


    //Turns content like ' {a: "@name@"} ' -> '{a: "Alex"}' provided that f = function(s, k){ return "Alex"}). Data is HTML escaped
    published.strHTMLTemplateFun = function(tpl, f) {
      return tpl.replace(/@([\-\.0-9a-zA-Z]+)@/g, function(s, key) {return published.strEscapeHTML(f(s, key)); });
  };

    //Turns content like ' {a: "@name@"} ' -> '{a: "Alex"}' provided that f = function(s, k){ return "Alex"}). Data is not HTML escaped
    published.strTemplateFun = function(tpl, f) {
      return tpl.replace(/@([\-\.0-9a-zA-Z]+)@/g, f);
  };

    //True if str contains valid email per: a@bc.de
    published.strIsEMail = function(str){
      if (published.strEmpty(str)) return false;
      var iat=str.indexOf("@");
      if (iat<1 || iat===str.length-1) return false;

      if (str.indexOf("@", iat+1)>=0) return false;//duplicate @

      var ldot=str.lastIndexOf(".");
      var pass =  (ldot>iat+2) && (ldot+2<=str.length);
      if (!pass) return false;

      var c;
      for(var i=0; i<str.length; i++){
        c = str[i];
        if (c==='.'||c==='@'||c==='-'||c==='_') continue;
        if (!published.isValidScreenNameLetterOrDigit(c)) return false;
      }

      return true;
    };

    published.isValidScreenNameLetter = function(c){
                      var extra = "ёЁÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿĀāĂăĄąĆćĈĉĊċČčĎďĐđĒēĔĕĖėĘęĚěĜĝĞğĠġĢģĤĥ";
                      return ((c>='A' && c<='Z') ||
                              (c>='a' && c<='z') ||
                              (c>='А' && c<='Я') ||
                              (c>='а' && c<='я') ||
                              (extra.indexOf(c)>=0));
    };

    published.isValidScreenNameLetterOrDigit = function(c){ return published.isValidScreenNameLetter(c) || (c>='0' && c<='9'); };

    published.isValidScreenNameSeparator= function(c){   return (c==='.' || c==='-' || c==='_'); };



    //True if str contains valid screen name/ID
    published.strIsScreenName = function(name){
      if (published.strEmpty(name)) return false;
      name = published.strTrim(name);
      if (name.length===0) return false;
      var wasSeparator = false;
      var c;
      for(var i=0; i<name.length; i++)
      {
        c = name[i];
        if (i===0)
        {
          if (!published.isValidScreenNameLetter(c)) return false;
          continue;
        }

        if (published.isValidScreenNameSeparator(c))
        {
          if (wasSeparator) return false;
          wasSeparator = true;
          continue;
        }
        if (!published.isValidScreenNameLetterOrDigit(c)) return false;
        wasSeparator = false;
      }
      return !wasSeparator;
    };

    // Normalizes US phone string so it looks like (999) 999-9999x9999.
    published.strNormalizeUSPhone = function(val){
        if (published.strEmpty(val)) return "";
        val = published.strTrim(val);
        if (val.length===0) return "";

        if (published.strStartsWith(val, "+", true)) return val; //international phone, just return as-is

        var isArea = false;
        var isExt = false;
        var area = "";
        var number = "";
        var ext = "";

        var chr;
        for (var i = 0; i < val.length; i++)
        {
           chr = val[i];

           if (!isArea && chr === '(' && area.length === 0)
           {
            isArea = true;
            continue;
           }

           if (isArea && chr === ')')
           {
            isArea = false;
            continue;
           }

           if (isArea && area.length === 3)
             isArea = false;


           if (number.length > 0 && !isExt)
           {      //check extention
                  if (chr === 'x' || chr === 'X' || (chr === '.' && number.length>6))
                  {
                   isExt = true;
                   continue;
                  }
                  var trailer = val.substring(i).toUpperCase();

                  if (published.strStartsWith(trailer,"EXT", false) && number.length >= 7)
                  {
                    isExt = true;
                    i += 2;
                   continue;
                  }
                 if (published.strStartsWith(trailer,"EXT.", false) && number.length >= 7)
                 {
                    isExt = true;
                    i += 3;
                    continue;
                 }
          }

          if (!published.charIsAZLetterOrDigit(chr)) continue;

          if (isArea) area += chr;
          else{
              if (isExt)
                ext += chr;
              else
                number += chr;
          }
        }//for

       while (number.length < 7)  number += '?';

       if (area.length === 0){
            if (number.length >= 10)
            {
              area = number.substring(0, 3);
              number = number.substring(3);
            }
            else
              area = "???";
       }

       if (number.length > 7 && ext.length === 0){
          ext = number.substring(7);
          number = number.substring(0, 7);
       }

       number = number.substring(0, 3) + "-" + number.substring(3);

       if (ext.length > 0) ext = "x" + ext;

       return "("+area+") " + number + ext;
    };


    published.LOCALIZER =
    {
      eng: {},
      rus: {},
      deu: {},
      fra: {},
      esp: {},

      allLanguageISOs:  function () {
         var result = [];

         for (var name in published.LOCALIZER)
           if (published.has(published.LOCALIZER, name) && name.length === 3) result.push(name);

         return result;
       }
    };


    //Localizes string per supplied lang iso code within schema/field
    published.strLocalize = function(iso, schema, fld, val){
      if (arguments.length<4) return val;
      if (published.strEmpty(iso) || published.strEmpty(val)) return val;

      var ANYSCHEMA = "--ANY-SCHEMA--";
      var ANYFIELD = "--ANY-FIELD--";

      if (published.strEmpty(schema)) schema = ANYSCHEMA;
      if (published.strEmpty(fld)) fld = ANYFIELD;

      var node = published.LOCALIZER;
      if (!node.hasOwnProperty(iso)) return val;
      node = node[iso];

      if (!node.hasOwnProperty(schema)){
        if (!node.hasOwnProperty(ANYSCHEMA)) return val;
        node = node[ANYSCHEMA];
      } else node = node[schema];

      if (!node.hasOwnProperty(fld)){
        if (!node.hasOwnProperty(ANYFIELD)) return val;
        node = node[ANYFIELD];
      } else node = node[fld];

      if (!node.hasOwnProperty(val)) return val;
      return node[val];
    };



    // Add toISOString support (i.e. "2012-01-01T12:30:15.120Z")
    published.toISODateTimeString = function(dt){
     function pad(n) { return n < 10 ? '0' + n : n; }
     return dt.getUTCFullYear() + '-' +
            pad(dt.getUTCMonth()+1)+ '-' +
            pad(dt.getUTCDate())+ 'T' +
            pad(dt.getUTCHours())+ ':' +
            pad(dt.getUTCMinutes())+ ':' +
            pad(dt.getUTCSeconds())+ 'Z';
    };

    // Add MM/DD/YYYY HH:MM:SS
    published.toUSDateTimeString = function(dt){
     function pad(n) { return n < 10 ? '0' + n : n; }

     return pad(dt.getMonth()+1)+"/"+
            pad(dt.getDate())+"/"+
            pad(dt.getFullYear())+" "+
            pad(dt.getHours())+':'+
            pad(dt.getMinutes())+':'+
            pad(dt.getSeconds());
    };

    // Add MM/DD/YYYY
    published.toUSDateString = function(dt){
     function pad(n) { return n < 10 ? '0' + n : n; }

     return pad(dt.getMonth()+1)+"/"+
            pad(dt.getDate())+"/"+
            pad(dt.getFullYear());
    };

    // Parses duration string to total seconds: duration("1d 10h 7m 13s")
    published.toSeconds = function(s) {
      var result  = 0,
        days    = s.match(/(\d+)\s*d/),
        hours   = s.match(/(\d+)\s*h/),
        minutes = s.match(/(\d+)\s*m/),
        seconds = s.match(/(\d+)\s*s/);
      if (days)    result += parseInt(days[1])*86400;
      if (hours)   result += parseInt(hours[1])*3600;
      if (minutes) result += parseInt(minutes[1])*60;
      if (seconds) result += parseInt(seconds[1]);
      return result;
    };

    // Generates random key with specified length from the alphabet of possible characters: rndKey(10,"abcdefzq2")
    published.genRndKey = function(keyLen, alphabet) {
      var key = "";
      if (!published.intValidPositive(keyLen)) keyLen = 8;
      if (published.strEmpty(alphabet)) alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
      while(key.length<keyLen)
        key += alphabet.charAt(published.rnd(alphabet.length));
      return key;
    };

    var _autoInc = {};
    //returns auto-inced named value: getAutoincKey('card', 1)
    published.genAutoincKey = function(seqName, num){
        var current = 0;
        if (published.strEmpty(seqName)) seqName = "Unspecified";
        if (!published.intValidPositive(num)) num = 1;

        if (seqName in _autoInc)
          current = _autoInc[seqName];

        var result = current;

        current += num;
        _autoInc[seqName] = current;
        return result;
    };

    // Returns true for scalar vars and false for arrays and objects
    published.isScalar = function(value) {
      return (/boolean|number|string/).test(typeof(value));
    };

    // Returns random number in the range of min/max where min=0 max =100 by default:  rnd(10,57)
    published.rnd = function() {
      var min = 0;
      var max = 100;

      if (arguments.length === 1)  max = arguments[0];
      else if (arguments.length === 2)
      {
        min = arguments[0];
        max = arguments[1];
       }

      return min+Math.floor(Math.random()*(max-min+1));
    };


    published.id = function(id){
        return document.getElementById(id);
    };

    published.getCookie = function(name) {
      var matches = document.cookie.match(new RegExp(
        "(?:^|; )" + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + "=([^;]*)"
      ));
      return matches ? decodeURIComponent(matches[1]) : false;
    };

    published.setCookie = function(name, value) {
      var cookie = name+"="+escape(value)+"; path=/";
      document.cookie = cookie;
    };

    published.deleteCookie = function(name) {
      published.setCookie(name, null, { expires: -1 });
    };

    published.isObjectType = function(tp) { return published.strOneOf(tp, ["object", "json", "map", "array"]);};
    published.isIntType = function(tp) { return published.strOneOf(tp, ["int", "integer"]);};
    published.isRealType = function(tp) { return published.strOneOf(tp, ["float", "real", "double", "money"]);};
    published.isBoolType = function(tp) { return published.strOneOf(tp, ["bool", "boolean", "logical"]);};
    published.isStringType = function(tp) { return published.strOneOf(tp, ["str", "string", "char[]", "char", "varchar", "text"]);};
    published.isDateType = function(tp) { return published.strOneOf(tp, ["date", "datetime", "time", "timestamp"]);};


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




    var any_event = "*";

    //Event Manager Mixin - keeps track of event subscriptions and invocations
    published.EventManager = {

       ANY_EVENT: any_event,

       //Binds a function to the named event handler
       eventBind: function(evtName, fun){
         if (published.strEmpty(evtName) || !fun) return false;

         var el = ensureEventList(this, evtName);
         if (published.inArray(el, fun)) return false;
         el.push(fun);
         return true;
       },

       //Un-Binds a function from the named event handler
       eventUnbind: function(evtName, fun){
         if (published.strEmpty(evtName)|| !fun) return false;

         var el = ensureEventList(this, evtName);
         return published.arrayDelete(el, fun);
       },

       //Clears all functions from the named event handler
       eventClear: function(evtName){
         if (published.strEmpty(evtName))
         {
            deleteEventList(this);
            return true;
         }

         var el = ensureEventList(this, evtName);
         published.arrayClear(el);

         return true;
       },

       //Increase to disable event firing for all events, decrease to enable, events are enabled again when value is <=0
       //This property is usefull for batch updates to suppress many event firings that are not needed
       eventInvocationSuspendCount: 0,

       //Invokes all functions bound to the named event handler
       eventInvoke: function(evtName){
         if (published.strEmpty(evtName)) return false;

         if (this.eventInvocationSuspendCount!==0) return false;

         //variadic params, remove 'evtName'
         var params = Array.prototype.slice.call(arguments, 1);

         //insert 'sender'
         params.splice(0,0,this);

         var el = ensureEventList(this, evtName);

         var i;
         //call all events for the named event
         for(i in el)
            el[i].apply(this, params);

         //insert 'evtName' for any event
         params.splice(0,0,evtName);

         el = ensureEventList(this, any_event);
         //call all "ANY" events for the named event
         for(i in el)
            el[i].apply(this, params);

         var sl = ensureSinkList(this);
         //call all "ANY" events for the named event
         for(i in sl){
           var sink = sl[i];
           var fun = sink.eventNotify;
           if (WAVE.isFunction(fun))
            fun.apply(sink, params);
         }

         return true;
       },

       //Binds a sink instance (an object) that will receive all events dispatched by this manager.
       //The sink must have a function called "eventNotify(evtName, sender,...args)" that will ge invoked
       eventSinkBind: function(sink){
         if (sink===null) return false;

         var sl = ensureSinkList(this);
         if (published.inArray(sl, sink)) return false;
         sl.push(sink);
         return true;
       },

       //Un-Binds an object that received all events from this manager
       eventSinkUnbind: function(sink){
         if (sink===null) return false;

         var sl = ensureSinkList(this);
         return published.arrayDelete(sl, sink);
       },

       //Clears all objects that act as event sinks bound to this instance
       eventSinkClear: function(){
         deleteSinkList(this);
       },

       //Returns a list of sink object that receive event notifications from this manager
       eventSinks: function(){
        return ensureSinkList(this);
       }
    };
              var eventListName = "@!WAVE EVENT FUN LIST";
              var eventSinkListName = "@!WAVE EVENT SINK LIST";

              function deleteEventList(obj){
                obj[eventListName] = {};
              }

              function deleteSinkList(obj){
                obj[eventSinkListName] = [];
              }

              function ensureEventList(obj, evtName){
                if (!(eventListName in obj)) obj[eventListName] = {};
                var el = obj[eventListName];
                if (!(evtName in el)) el[evtName] = [];
                return el[evtName];
              }

              function ensureSinkList(obj){
                if (!(eventSinkListName in obj)) obj[eventSinkListName] = [];
                return obj[eventSinkListName];
              }



    var utest = {
      CSS_CLASS_AREA:  "wvUTestArea",
      CSS_CLASS_TESTTITLE:  "wvUTestTitle",
      CSS_CLASS_TESTNUMBER: "wvUTestNumber",
      CSS_CLASS_OK:    "wvUTestOK",
      CSS_CLASS_ERROR: "wvUTestError",
      CSS_CLASS_TABLEERROR: "wvUTestTableError",
      CSS_CLASS_TABLETOTAL: "wvUTestTableTotal",
      DIV_SUMMARY: "div_wv_utest_summary",
      DIV_AREA_PREFIX: "div_wv_utest_area_",
      DIV_TEST_PREFIX: "div_wv_utest_test_"
    };




    var _testOut = [];
    var _areaIDs = {};
    var _testingStarted = false;
    var _testNumber = 0;

           function updateHeader()
           {
              var dis = published.id(utest.DIV_SUMMARY);
              if (!dis) return;
              var content = '<table style="width: 75%">';
               content += '<tr> <th>Area</th> <th>Total</th><th>Passed</th><th>Failed</th> </tr>';

               var gtAll = 0;
               var gtFailed = 0;

               for(var i in _areaIDs)
               {
                   var dia = published.id(_areaIDs[i]);
                   if (!dia) continue;
                   content +=  published.strHTMLTemplate(
                           '<tr @STL@> <td>@Area@</td> <td>@Total@</td><td>@Passed@</td><td>@Failed@</td> </tr>',
                           {
                            STL: dia.STAT_FAILED_TESTS > 0 ? "class="+utest.CSS_CLASS_TABLEERROR : "",
                            Area: dia.STAT_NAME,
                            Total: dia.STAT_TOTAL_TESTS,
                            Passed: dia.STAT_TOTAL_TESTS - dia.STAT_FAILED_TESTS,
                            Failed: dia.STAT_FAILED_TESTS});
                   gtAll += dia.STAT_TOTAL_TESTS;
                   gtFailed += dia.STAT_FAILED_TESTS;
               }

               content +=  published.strHTMLTemplate(
                           '<tr class=@CLS@> <td>TOTAL</td> <td>@Total@</td><td>@Passed@</td><td>@Failed@</td> </tr>',
                           {
                            CLS: utest.CSS_CLASS_TABLETOTAL,
                            Total: gtAll,
                            Passed: gtAll - gtFailed,
                            Failed: gtFailed});

               content += '</table>';
              dis.innerHTML = content;
           }

    utest.run = function(area, name, fun) {
        if (!_testingStarted)
        {
           _testingStarted = true;
           setTimeout( updateHeader, 1000);
        }

        if (published.strEmpty(area)) area = "Unspecified";

        var did = null;
        if (area in _areaIDs)
            did = _areaIDs[area];
        else
        {
            did = utest.DIV_AREA_PREFIX + published.genAutoincKey("UtestingArea");
           _areaIDs[area] = did;
        }

        var divArea = published.id(did);
        if (!divArea)
        {
            divArea = document.createElement("div");
            divArea.id = did;
            divArea.className = utest.CSS_CLASS_AREA;
            document.body.appendChild(divArea);
            divArea.innerHTML = "<h2>"+area+"</h2>";

            divArea.STAT_NAME = area;
            divArea.STAT_TOTAL_TESTS = 0;
            divArea.STAT_FAILED_TESTS = 0;
        }

        var error = "";
        try
        {
            _testOut = [];
            _testNumber++;
            divArea.STAT_TOTAL_TESTS += 1;
            fun();
        }
        catch(err)
        {
            divArea.STAT_FAILED_TESTS += 1;
            error = err;
        }
        var isError = !published.strEmpty(error);

        var dtid = utest.DIV_TEST_PREFIX + published.genAutoincKey("UtestingCase");
        var divTest = document.createElement("div");
        divTest.id = dtid;
        divTest.className = isError ? utest.CSS_CLASS_ERROR : utest.CSS_CLASS_OK;

        var content = "<div class='"+utest.CSS_CLASS_TESTTITLE+"'><span class='" + utest.CSS_CLASS_TESTNUMBER + "'>" + _testNumber + "</span>&nbsp;"
          + name + "&nbsp;&nbsp;:&nbsp;&nbsp;" + (isError ? "FAILED" : "PASSED")+"</div>";

        if (isError)
         content += "&nbsp;&nbsp;Error: " + error;


        if (_testOut.length>0)
        {
            content += "<pre>";

            for(var msg in _testOut)
             content += " "+_testOut[msg];

            content += "</pre>";
        }

        divTest.innerHTML = content;
        divArea.appendChild(divTest);
    };

    //Returns true to indicate that testing has been activated
    utest.testingStarted = function(){
        return _testingStarted;
    };

    //Write log message in new line
    utest.log = function(msg){
        _testOut.push("  &gt; "+msg+'\n');
    };

    //Writes log message inline
    utest.logi = function(msg){
        _testOut.push(msg);
    };

    utest.assertTrue = function(assertion, msg){
        if (published.strEmpty(msg)) msg = "Assertion not true";
        if (!assertion) throw msg;
    };

    utest.assertFalse = function(assertion, msg){
        if (published.strEmpty(msg)) msg = "Assertion not false";
        if (assertion) throw msg;
    };


    published.UTest = utest;


    var platform =
    {
        iPhone: navigator.userAgent.match(/iPhone/i),
		iPod: navigator.userAgent.match(/iPod/i),
		iPad: navigator.userAgent.match(/iPad/i),
		Android: navigator.userAgent.match(/Android/i),
		IE: navigator.appName.indexOf("Microsoft") !== -1,
		IEMobile: navigator.userAgent.match(/IEMobile/i),
		WinPhone: /windows phone/i.test(navigator.userAgent),
        Chrome: !!window.chrome, // navigator.userAgent.match(/Chrome/i),
		Safari: navigator.userAgent.match(/Safari/i) && !window.chrome,
		FireFox: navigator.userAgent.indexOf("Firefox") > -1,
		BlackBerry: navigator.userAgent.match(/BlackBerry/i),
		WebOS: navigator.userAgent.match(/webOS/i),
		Opera: window.opera, // navigator.userAgent.indexOf("Presto") > -1
		OperaMini: navigator.userAgent.match(/Opera Mini/i),
		OperaMobi: navigator.userAgent.match(/Opera Mobi/i),
        Silk: /silk/i.test(navigator.userAgent)
    };

    platform.iOS    = platform.iPhone || platform.iPod || platform.iPad;

	platform.Mobile = platform.iOS || platform.Android ||
                      platform.OperaMini || platform.OperaMobi ||
                      platform.BlackBerry ||
                      platform.WebOS ||
                      platform.IEMobile || platform.WinPhone ||
                      platform.Silk;

	platform.WebKit = platform.Chrome || platform.Safari;

    published.Platform = platform;


    var geometry = {
                     EARTH_RADIUS_KM: 6371
                   };

    var pi = 3.14159265;
	var pi2 = 6.28318531;

	geometry.PI = pi;
	geometry.PI2 = pi2;

    geometry.MapDirection = {
		North:     {Name: "North"},
		NorthEast: {Name: "NorthEast"},
		East:      {Name: "East"},
		SouthEast: {Name: "SouthEast"},
		South:     {Name: "South"},
		SouthWest: {Name: "SouthWest"},
		West:      {Name: "West"},
		NorthWest: {Name: "NorthWest"}
	};

    // Returns pixel distance between two points
	geometry.distance = function(x1, y1, x2, y2) { return Math.sqrt(Math.pow(x1 - x2, 2) + Math.pow(y1 - y2, 2)); };

    // Returns pixel distance between two points
	geometry.distancePoints = function(p1, p2) {  return geometry.distance(p1.x(), p1.y(),  p2.x(), p2.y()); };


    // Converts radians to degrees
    geometry.radToDeg = function(rad){ return (rad / pi) * 180; };

    // Converts defrees to rads
    geometry.degToRad = function(deg){ return (deg / 180) * pi; };


    // Returns azimuth angle (theta) in radians
  geometry.azimuthRad = function(xc, yc, xd, yd) {
    var angle = Math.atan2(yd - yc, xd - xc);
    if (angle < 0) angle = pi2 + angle;
    return angle;
  };

    // Returns azimuth angle (theta) in radians for two points: center and destination
  geometry.azimuthRadPoints = function(pc, pd) {  return geometry.azimuthRad(pc.x(), pc.y(),  pd.x(), pd.y()); };

    // Returns azimuth angle (theta) in degrees
  geometry.azimuthDeg = function(xc, yc, xd, yd){ return (geometry.azimuthRad(xc, yc, xd, yd) / pi2) * 360; };

    // Returns azimuth angle (theta) in degrees for twho points: center and destination
  geometry.azimuthDegPoints = function(pc, pd){ return geometry.azimuthDeg(pc.x(), pc.y(),  pd.x(), pd.y()); };

    // Returns azimuth angle (theta) in radix units
  geometry.azimuthOfRadix = function(xc, yc, xd, yd, radix) {
    if (radix < 2) radix = 2;
    var angle = geometry.azimuthRad(xc, yc, xd, yd);
    var half = pi / radix;
    angle = geometry.wrapAngle(angle, half);
    return Math.floor((angle / pi2) * radix);
  };

    // Returns azimuth angle (theta) in in radix units for twho points: center and destination
  geometry.azimuthOfRadixPoints = function(pc, pd, radix){ return geometry.azimuthOfRadix(pc.x(), pc.y(),  pd.x(), pd.y(), radix); };

    //Returns rectangle from coordinate pairs
  geometry.toRectXY = function(x1,y1, x2,y2) {
    return new geometry.Rectangle( new geometry.Point(x1, y1), new geometry.Point(x2, y2) );
  };

    // Returns rectangle from coordinats and dimensions
  geometry.toRectWH = function(x1,y1, w,h) {
    return new geometry.Rectangle( new geometry.Point(x1, y1), new geometry.Point(x1 + w, y1 + h) );
  };

    // Returns area of overlap between two rectangles
  geometry.overlapAreaRect = function(rect1, rect2) {
    var tl1 = rect1.topLeft();
    var tl2 = rect2.topLeft();
    return geometry.overlapAreaWH(
      tl1.x(), tl1.y(), rect1.width(), rect1.height(),
      tl2.x(), tl2.y(), rect2.width(), rect2.height()
    );
  };

    // Returns area of overlap between two rectangles expressed as top-left/width-height pairs
  geometry.overlapAreaWH = function(x1,y1, w1,h1, x2,y2, w2,h2) {
    var ix;
    var iy;

    if (w2>=w1) {
      if (x1<=x2) ix = (x1+w1) - x2;
      else if ((x1+w1)>=(x2+w2)) ix = (x2+w2)-x1;
      else ix = w1;
    } else {
      if (x2<=x1) ix = (x2+w2) - x1;
      else if ((x2+w2)>=(x1+w1)) ix = (x1+w1)-x2;
      else ix = w2;
    }

    if (h2>=h1) {
      if (y1<=y2) iy = (y1+h1) - y2;
      else if ((y1+h1)>=(y2+h2)) iy = (y2+h2)-y1;
      else iy = h1;
    } else {
      if (y2<=y1) iy = (y2+h2) - y1;
      else if ((y2+h2)>=(y1+h1)) iy = (y1+h1)-y2;
      else iy = h2;
    }

    if (ix<0) ix = 0;
    if (iy<0) iy = 0;

    return ix*iy;
  };

    // Returns area of overlap between two rectangles expressed as top-left/bottom-right pairs
  geometry.overlapAreaXY = function(left1, top1, right1, bott1, left2, top2, right2, bott2) {
    return geometry.overlapAreaWH(left1, top1, right1-left1, bott1-top1, left2, top2, right2-left2, bott2-top2);
  };

    // defines if line has common points with rect
    geometry.lineIntersectsRect = function(rx1, ry1, rx2, ry2, lx1, ly1, lx2, ly2) {
      var a = ly1 - ly2,
          b = lx2 - lx1,
          c = -(a * lx1 + b * ly1);

      var r = a * rx1 + b * ry1 + c;
      var sign = r === 0 ? 0 : (r > 0 ? 1 : -1);

      r = a * rx1 + b * ry2 + c;
      if ((r === 0 ? 0 : (r > 0 ? 1 : -1)) !== sign) return true;

      r = a * rx2 + b * ry1 + c;
      if ((r === 0 ? 0 : (r > 0 ? 1 : -1)) !== sign) return true;

      r = a * rx2 + b * ry2 + c;
      if ((r === 0 ? 0 : (r > 0 ? 1 : -1)) !== sign) return true;

      return false;
    };

    //Returns area of line overlap with rect. Normalizes coordinate direction
    geometry.intersectionLengthRectLineXY = function(rx1, ry1, rx2, ry2, lx1, ly1, lx2, ly2) {
        if (!geometry.lineIntersectsRect(rx1, ry1, rx2, ry2, lx1, ly1, lx2, ly2)) return 0;

        var x1, y1, x2, y2;

        if (lx1 < rx1) x1 = rx1; else if (lx1 > rx2) x1 = rx2; else x1 = lx1;
        if (ly1 < ry1) y1 = ry1; else if (ly1 > ry2) y1 = ry2; else y1 = ly1;
        if (lx2 < rx1) x2 = rx1; else if (lx2 > rx2) x2 = rx2; else x2 = lx2;
        if (ly2 < ry1) y2 = ry1; else if (ly2 > ry2) y2 = ry2; else y2 = ly2;

        return geometry.distance(x1, y1, x2, y2);
    };

    // Modifies an angle by delta value ensuring that resulting angle is always between 0 and 2pi
  geometry.wrapAngle = function(angle, delta) {
    delta = delta % pi2;
    if (delta<0) delta = pi2 + delta;
    var result = angle + delta;
    return result % pi2;
  };

    //Converts map direction to angular coordinate in radians
  geometry.mapDirectionToAngle = function(direction) {
    switch (direction) {
      case geometry.MapDirection.North: return 4/16 * pi2;
      case geometry.MapDirection.South: return 12/16 * pi2;
      case geometry.MapDirection.East:  return 0.0;
      case geometry.MapDirection.West:  return 8/16 * pi2;

      case geometry.MapDirection.NorthEast: return 2/16 * pi2;
      case geometry.MapDirection.NorthWest: return 6/16 * pi2;
      case geometry.MapDirection.SouthEast: return 14/16 * pi2;
      case geometry.MapDirection.SouthWest: return 10/16 * pi2;

      default: return 0.0;
     }
  };

    // Converts a radian angular coordinate into map direction
  geometry.angleToMapDirection = function(angle) {
    angle = geometry.wrapAngle(angle, 0);
    if ((angle >= 0.0) && (angle < pi2 * 1/16)) return geometry.MapDirection.East;
    else if ((angle >= pi2 * 1/16) && (angle < pi2 * 3/16)) return geometry.MapDirection.NorthEast;
    else if ((angle >= pi2 * 3 / 16) && (angle < pi2 * 5 / 16)) return geometry.MapDirection.North;
    else if ((angle >= pi2 * 5 / 16) && (angle < pi2 * 7 / 16)) return geometry.MapDirection.NorthWest;
    else if ((angle >= pi2 * 7 / 16) && (angle < pi2 * 9 / 16)) return geometry.MapDirection.West;
    else if ((angle >= pi2 * 9 / 16) && (angle < pi2 * 11 / 16)) return geometry.MapDirection.SouthWest;
    else if ((angle >= pi2 * 11 / 16) && (angle < pi2 * 13 / 16)) return geometry.MapDirection.South;
    else if ((angle >= pi2 * 13 / 16) && (angle < pi2 * 15 / 16)) return geometry.MapDirection.SouthEast;
    else return geometry.MapDirection.East;
  };

    // Calculates a relative area of an inner rectangle that violates outside perimeter.
    // The area is not 100% geometrically accurate - may be used for relative comparisons only
  geometry.perimeterViolationArea = function(perimeter, inner) {
    var ix = 0;
    var iy = 0;

    if (inner.left()<perimeter.left()) ix = perimeter.left() - inner.left();
    else if (inner.right()>perimeter.right()) ix = inner.right() - perimeter.right();

    if (inner.top() < perimeter.top()) iy = perimeter.top() - inner.top();
    else if (inner.bottom() > perimeter.bottom()) iy = inner.bottom() - perimeter.bottom();

    return (ix * inner.height()) + (iy * inner.width());
  };

  // Returns a point of intersection between a ray cast from the center of a rectangle
  // under certain polar coordinate angle and a rectangle side
  geometry.findRayFromRectangleCenterSideIntersection = function(rect, theta) {
    var center = new geometry.Point(rect.left() + rect.width() / 2, rect.top() + rect.height() / 2);

    //make ray "infinite" in comparison to rect
    var rayLength = rect.width() > rect.height() ? rect.width() : rect.height();

    if (rayLength < 100) rayLength = 100; //safeguard

    var rayEnd = new geometry.PolarPoint(rayLength, theta);//create ray "end" point
    var rayEndPoint = rayEnd.toPoint();

    //get line incline	aka. y = kx
    var k = (rayEndPoint.x() !== 0)? (rayEndPoint.y()) / (rayEndPoint.x()) : 0;

    var x = 0;
    var y = 0;

    var lst = [];

    //north
    x = center.x() + ((k !== 0) ? ((rect.top() - center.y()) / k) : 0);
    y = rect.top();
    if ((x >= rect.left()) && (x <= rect.right())) lst.push(new geometry.Point(x, y));

    //south
    x = center.x() + ((k !== 0) ? ((rect.bottom() - center.y()) / k) : 0);
    y = rect.bottom();
    if ((x >= rect.left()) && (x <= rect.right())) lst.push(new geometry.Point(x, y));

    //east
    x = rect.right();
    y = center.y() + k * (rect.right() - center.x());
    if ((y >= rect.top()) && (y <= rect.bottom())) lst.push(new geometry.Point(x, y));

    //west
    x = rect.left();
    y = center.y() + k * (rect.left() - center.x());
    if ((y >= rect.top()) && (y <= rect.bottom())) lst.push(new geometry.Point(x, y));

    var minPoint = new geometry.Point(1000000, 1000000);
    var minDistance = 1000000;

    var re = rayEnd.toPoint(); //rayEnd is relative to absolute 0,0
    re.offset(center.x(), center.y()); // need to make relative to rectangle center

    //find closest point
    for(var i in lst) {
      var p = lst[i];
      var dst = geometry.distancePoints(p, re);
      if (dst < minDistance) {
        minPoint = p;
        minDistance = dst;
      }
    }
    return minPoint;
  };

  // Calculates coordinates of rectangle given by width/height ('rw'/'rh')
  // which center is rotated by 'angle' (in radians) relatively to point ('cx'/'cy')
  // respecting reatangle size and 'margin'.
  // Returns coordinates of rotated rectangle (geometry.Rectangle object)
  geometry.rotateRectAroundCircle = function (cx, cy, cMargin, rw, rh, angle) {
    var halfRw = rw / 2, halfRh = rh / 2;
    var rcX = cx + Math.cos(angle) * (halfRw + cMargin);
    var rcY = cy + Math.sin(angle) * (halfRh + cMargin);
    return new geometry.toRectXY(rcX - halfRw, rcY - halfRh, rcX + halfRw, rcY + halfRh);
  };

  // Returns 2D bounding box (with  for a set of points (array of {x, y})
  geometry.getBBox = function(points) {
    if (!points) return null;
    var length = points.length;
    if (length === 0) return null;
    var minX = Number.POSITIVE_INFINITY, maxX=Number.NEGATIVE_INFINITY, minY=Number.POSITIVE_INFINITY, maxY=Number.NEGATIVE_INFINITY;
    for(var i=0; i<length; i++) {
      var p = points[i];
      var x = p.x, y = p.y;
      if(x < minX) minX = x;
      if(x > maxX) maxX = x;
      if(y < minY) minY = y;
      if(y > maxY) maxY = y;
    }
    return geometry.toRectXY(minX, minY, maxX, maxY);
  };

    // Point class represents x,y pair on a cartesian plane
  geometry.Point = function(x, y)
    {
    var fX = x;
    this.x = function(val){
            if (typeof(val)!==tUNDEFINED) fX = val;
            return fX;
        };

    var fY = y;
    this.y = function(val){
            if (typeof(val)!==tUNDEFINED) fY = val;
            return fY;
        };

    // Changes point coordinates
    this.offset = function(dx, dy) { fX += dx; fY += dy; };

  }; // Point


    // Returns point as polar point relative to the specified center
  geometry.Point.prototype.toPolarPoint = function(center) {
      var dist = geometry.distancePoints(center, this);
      var angle = Math.atan2(this.y() - center.y(), this.x() - center.x());

      if (angle < 0)	angle = pi2 + angle;

      return new geometry.PolarPoint(dist, angle);
  };


    //Determines whether the two points contain equivalent coordinates
    geometry.Point.prototype.isEqual = function(other){
      return (this.x() === other.x()) && (this.y() === other.y());
    };

    geometry.Point.prototype.toString = function()
    {
      return "(" + this.x().toString() + " , " + this.y().toString() + ")";
    };


    // Polar Point class represents point with polar coordinates
    geometry.PolarPoint = function(radius, theta)
    {
      var fRadius = radius;
      this.radius = function(val){
            if (typeof(val)!==tUNDEFINED) fRadius = val;
            return fRadius;
      };

      var fTheta = checkangle(theta);
      this.theta = function(val){
            if (typeof(val)!==tUNDEFINED) fTheta = checkangle(val);
            return fTheta;
      };

      //Returns polar point as simple x,y point
      this.toPoint = function(){
          var x = fRadius * Math.cos(fTheta);
          var y = fRadius * Math.sin(fTheta);
          return new geometry.Point(x, y);
      };

      function checkangle(angle){
       if ((angle < 0) || (angle > pi2))
            throw "Invalid polar coordinates angle";
       return angle;
      }


    };//PolarPoint

    geometry.PolarPoint.prototype.isEqual = function(other){
        return this.radius() === other.radius() && this.theta() === other.theta();
    };

    geometry.PolarPoint.prototype.toString = function(){
      return "(" + this.radius().toString() + " , " + geometry.radToDeg(this.theta()).toString() + "°)";
    };

    // Represents a rectangle
    geometry.Rectangle = function(corner1, corner2)
    {
        var self = this;

        var fCorner1 = corner1;
        var fCorner2 = corner2;

        this.corner1 = function(val){
            if (typeof(val)!==tUNDEFINED) fCorner1 = val;
            return fCorner1;
        };

        this.corner2 = function(val){
            if (typeof(val)!==tUNDEFINED) fCorner2 = val;
            return fCorner2;
        };

        // Returns top left corner point per natural axis orientation when x increases from left to right, and y increases from top to bottom
        this.topLeft = function(){
            var lx = fCorner1.x();
            var other = fCorner2.x();

            if (other < lx) lx = other;

            var ty = fCorner1.y();
            other = fCorner2.y();

            if (other < ty) ty = other;
            return new geometry.Point(lx, ty);
         };

       // Returns bottom right corner point per natural axis orientation when x increases from left to right, and y increases from top to bottom
       this.bottomRight = function(){
            var rx = fCorner1.x();
            var other = fCorner2.x();

            if (other > rx) rx = other;

            var by = fCorner1.y();
            other = fCorner2.y();

            if (other > by) by = other;

            return new geometry.Point(rx, by);
       };

       // Return rectangle width
       this.width = function(){  return Math.abs(fCorner1.x() - fCorner2.x()); };

       // Return rectangle height
       this.height = function(){  return Math.abs(fCorner1.y() - fCorner2.y()); };

       // Returns left-most edge coordinate
       this.left = function() {
         var lx = fCorner1.x();
         var other = fCorner2.x();

         if (other < lx) lx = other;

         return lx;
       };

       // Returns right-most edge coordinate
       this.right = function(){
         var rx = fCorner1.x();
         var other = fCorner2.x();

         if (other > rx) rx = other;

         return rx;
       };

       // Returns top-most edge coordinate
       this.top = function(){
         var ty = fCorner1.y();
         var other = fCorner2.y();

         if (other < ty) ty = other;

         return ty;
       };

       // Returns bottom-most edge coordinate
       this.bottom = function(){
         var by = fCorner1.y();
         var other = fCorner2.y();

         if (other > by) by = other;

         return by;
       };

       // Returns the center point
       this.centerPoint = function(){
         return new geometry.Point(self.left() + self.width() / 2, self.top() + self.height() / 2);
       };

       // Returns rectangle square
       this.square = function () { return self.width() * self.height(); };

       // Tests if point lies within this reactangle
       this.contains = function (point) {
         var topLeft = self.topLeft(), bottomRight = self.bottomRight();
         var px = point.x(), py = point.y();
         return px >= topLeft.x() && px <= bottomRight.x() && py >= topLeft.y() && py <= bottomRight.y();
       };

    };//Rectangle class

    geometry.Rectangle.prototype.isEqual = function(other){
        return this.left() === other.left() && this.top() === other.top() && this.width() === other.width() && this.height() === other.height();
    };

    geometry.Rectangle.prototype.toString = function(){
      return "(" + this.left().toString() + "," + this.top().toString() + " ; "+ this.width().toString() + "x" + this.height().toString()+")";
    };


    /**
     * Calculates callout balloon vertices suitable for contour curve drawing
     *
     * @param {Rectangle} body Balloon body coordinates
     * @param {Point} target Balloon leg attachment point
     * @param {double} legSweep Length of balloon leg attachment breach at balloon body edge, expressed in radians (arc length). A value such as PI/16 yields good results
     * @returns {array} An array of vertex points
     */
    geometry.vectorizeBalloon = function(body, target, legSweep)
    {
      var result = [];

      var center = new geometry.Point(body.left() + body.width() / 2, body.top() + body.height() / 2);
      var trg = target.toPolarPoint(center);

      var legStart = geometry.findRayFromRectangleCenterSideIntersection(
                                 body,
                                 geometry.wrapAngle(trg.theta(), -legSweep / 2));

      var legEnd = geometry.findRayFromRectangleCenterSideIntersection(
                                 body,
                                 geometry.wrapAngle(trg.theta(), +legSweep / 2));

      //build vertexes
      result.push(new geometry.Point(body.left(), body.top()));

      result.push(new geometry.Point(body.right(), body.top()));

      result.push(new geometry.Point(body.right(), body.bottom()));

      result.push(new geometry.Point(body.left(), body.bottom()));

      result.push(legStart);
      result.push(target);
      result.push(legEnd);

      //reorder points by azimuth so the curve can close and look like a balloon
      result.sort(function(p1, p2)
                      {
                        var pp1 = p1.toPolarPoint(center);
                        var pp2 = p2.toPolarPoint(center);

                        if (pp1.theta() > pp2.theta())
                          return -1;
                        else
                          return +1;
                      }
                 );

      return result;
    };


    published.Geometry = geometry;


    // Mixin that enables function chaining that facilitates lazy evaluation via lambda-funcs
    published.Walkable =
    {
        wSelect: function(selector) {
                    var srcWalkable = this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        var curr;
                        return {
                          reset: function() { walker.reset(); },
                          moveNext: function() {
                            if (!walker.moveNext()) return false;
                            curr = selector(walker.current());
                            return true;
                          },
                          current: function() { return curr; }
                        };
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wSelect

        wSelectMany: function (e2Walkable) {
                    var srcWalkable = this;
                    var walkable = {
                      getWalker: function () {
                        var walker = srcWalkable.getWalker();
                        var currWalker, curr;
                        return {
                          reset: function () {
                            walker.reset();
                            while (walker.moveNext()) {
                              var walkerCurrent = walker.current();
                              if (walkerCurrent === null) continue;
                              var eWalkable = e2Walkable ? e2Walkable(walkerCurrent) : walkerCurrent;
                              currWalker = eWalkable.getWalker();
                              break;
                            }
                          },

                          moveNext: function () {
                            if (currWalker && currWalker.moveNext()) {
                              curr = currWalker.current();
                              return true;
                            }
                            while (walker.moveNext()) {
                              var walkerCurrent = walker.current();
                              if (walkerCurrent === null) continue;
                              var eWalkable = e2Walkable ? e2Walkable(walkerCurrent) : walkerCurrent;
                              currWalker = eWalkable.getWalker();
                              if (currWalker.moveNext()) {
                                curr = currWalker.current();
                                return true;
                              }
                            }
                            return false;
                          },

                          current: function () { return curr; }
                        };
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;

        }, //wSelectMany

        wWhere: function(filter) {
                    var srcWalkable = this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        return {
                          reset: function() { walker.reset(); },
                          moveNext: function() {
                            while (walker.moveNext()) {
                              var has = filter(walker.current());
                              if (has) return true;
                            }
                            return false;
                          },
                          current: function() { return walker.current(); }
                        };
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wWhere

        wAt: function(pos) {
              var walker = this.getWalker();
              var idx = -1;
              while(walker.moveNext()) {
                idx++;
                if (idx === pos) return walker.current();
              }

              return null;
        }, //wIdx

        wFirst: function(condition) {
                  var walker = this.getWalker();
                  while(walker.moveNext())
                    if (condition) {
                      if (condition(walker.current()))
                        return walker.current();
                    } else {
                      return walker.current();
                    }

                  return null;
        }, //wFirst

        wFirstIdx: function(condition) {
                    var walker = this.getWalker();
                    var idx = -1;
                    while(walker.moveNext()) {
                      idx++;
                      if (condition(walker.current())) return idx;
                    }
                    return -1;
        }, //wFirstIdx

        wCount: function(condition) {
                var walker = this.getWalker();
                var cnt = 0;
                while(walker.moveNext())
                  if (condition){
                    if (condition(walker.current()))
                      cnt++;
                  } else {
                    cnt++;
                  }
                return cnt;
        }, //wCount

        wDistinct: function(equal) {
                    var srcWalkable = this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        function WW()
                        {
                          var existing = [], existingWalkable = WAVE.arrayWalkable(existing);
                          this.reset = function() { walker.reset(); existing.splice(0, existing.length); };

                          function filter(e){
                            if (equal)
                                return equal(e, walker.current());
                            else
                                return e === walker.current();
                          }


                          this.moveNext = function() {
                            while (walker.moveNext()) {
                              if (existingWalkable.wFirstIdx(filter) === -1)
                              {
                                existing.push(walker.current());
                                return true;
                              }
                            }
                            return false;
                          };
                          this.current = function() { return walker.current(); };
                        }
                        return new WW();
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wDistinct

        wConcat: function(other) {
          var srcWalkable = this;

          var walkable = {
            getWalker: function() {
              var walker = srcWalkable.getWalker();
              var walkerOther = null;
              return {
                reset: function() { walker.reset(); walkerOther = null; },
                moveNext: function() {
                  if (walkerOther === null) {
                    if (walker.moveNext()) {
                      return true;
                    } else {
                      walkerOther = other.getWalker();
                    }
                  }
                  return walkerOther.moveNext();
                },
                current: function() { return walkerOther === null ? walker.current() : walkerOther.current(); }
              };
            }
          }; //wConcat

          WAVE.extend(walkable, WAVE.Walkable);
          return walkable;
        },

        wExcept: function(other, equal) {
                    var srcWalkable = this;

                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();

                        function filter(e){
                            if (equal)
                                return equal(walker.current(), e);
                            else
                                return e === walker.current();
                        }

                        return {
                          reset: function() { walker.reset(); },
                          moveNext: function() {
                            while (walker.moveNext()) {
                              if (other.wFirstIdx(filter) === -1)
                              {
                                return true;
                              }
                            }
                            return false;
                          },
                          current: function() { return walker.current(); }
                        };
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        },//wExcept

        wGroup: function(keyer, valuer) {
                    var srcWalkable=this;
                    var walkable = {
                      getWalker: function() {
                        function WW() {
                          var distinct = srcWalkable.wDistinct(function (a, b) { return keyer(a) === keyer(b); });
                          var distinctWalker = distinct.getWalker();

                          this.reset = function() { distinctWalker.reset(); };
                          this.moveNext = function() { return distinctWalker.moveNext(); };
                          this.current = function() {
                                           return {
                                                    k: keyer(distinctWalker.current()),
                                                    v: srcWalkable.wWhere(function(e) { return keyer(e) === keyer(distinctWalker.current());}).wSelect(function(e) { return valuer(e); })
                                                  };
                                         };
                        }

                        return new WW();
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wGroup

        wGroupIntoObject: function() {
                var obj = {};
                this.wEach(function(e) {   obj[e.k] = e.v.wToArray();   });
                return obj;
        }, //wGroupIntoObject

        wGroupIntoArray: function(keyer, valuer) {
                var arr = [];
                this.wEach(function(e) { arr.push({k: e.k, v: e.v.wToArray()}); } );
                return arr;
        }, //wGroupIntoArray

        wGroupAggregate: function(aggregator, initVal) {
                    var srcWalkable = this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        return {
                          reset: function() { walker.reset(); },
                          moveNext: function() { return walker.moveNext(); },
                          current: function() {
                            var cur = walker.current();
                            return {
                              k: cur.k,
                              v: cur.v.wAggregate( function(r, e) { return aggregator(cur.k, r, e); })
                            };
                          }
                        };
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wGroupAggregate

        wOrder: function(order) {
                var arr = this.wToArray();
                if (!order) order = function(a, b) { return a < b ? -1 : a > b ? 1 : 0; };
                arr.sort(order);
                return WAVE.arrayWalkable(arr);
        }, //wOrder

        wTake: function(qty) {
                    var srcWalkable=this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        function WW() {
                          var taken = 0;
                          this.reset = function() { walker.reset(); taken = 0; };
                          this.moveNext = function() {
                            var has = walker.moveNext() && taken < qty;
                            if (!has) return false;
                            taken++;
                            return true;
                          };
                          this.current = function() { return walker.current(); };
                        }
                        return new WW();
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wTake

        wTakeWhile: function(cond) {
                    var srcWalkable=this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        return {
                          reset: function() { walker.reset(); },
                          moveNext: function() {
                            if (!walker.moveNext()) return false;
                            if (!cond(walker.current())) return false;
                            return true;
                          },
                          current: function() { return walker.current(); }
                        };
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wTakeWhile

        wSkip: function(qty) {
                    var srcWalkable=this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();

                        function WW() {
                          var idx = -1;
                          this.reset = function() { walker.reset(); idx = -1; };
                          this.moveNext = function() {
                            while(walker.moveNext()) {
                              idx++;
                              if (idx >= qty) return true;
                            }
                            return false;
                          };
                          this.current = function() { return walker.current(); };
                        }

                        return new WW();
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wSkip

        wAny: function(condition) {
                    var walkable = this;
                    var walker = walkable.getWalker();
                    if (!condition) return walker.moveNext();

                    while(walker.moveNext()) if (condition(walker.current())) return true;

                    return false;
        }, //wAny

        wAll: function(condition) {
                    var walkable = this;
                    var walker = walkable.getWalker();
                    if (!condition) return !walker.moveNext();

                    while(walker.moveNext()) if (!condition(walker.current())) return false;

                    return true;
        }, //wAll

        wMin: function(less) {
                    if (!less) less = function(a, b) { return a < b; };

                    var walker = this.getWalker();

                    if (!walker.moveNext()) return null;

                    var minVal = walker.current();

                    while(walker.moveNext()) if (less(walker.current(), minVal)) minVal = walker.current();

                    return minVal;
        }, //wMin

        wMax: function(greater) {
                    if (!greater) greater = function(a, b) { return a > b; };

                    var walker = this.getWalker();

                    if (!walker.moveNext()) return null;

                    var maxVal = walker.current();

                    while(walker.moveNext()) if (greater(walker.current(), maxVal)) maxVal = walker.current();

                    return maxVal;
        }, //wMax

        wAggregate: function(aggregate, initVal) {
                      var r = initVal;
                      this.wEach(function(e) { r = aggregate(r, e); });
                      return r;
        }, //wAggregate

        wSum: function(initVal) {
                      if (typeof(initVal) === tUNDEFINED) initVal = 0;
                      return this.wAggregate(function(r, e) { return r + e; }, initVal);
        }, //wSum

        wEqual: function(other, equalFunc) {
          var selfWalker = this.getWalker();
          var otherWalker = other.getWalker();
          do {
            var selfHasValue = selfWalker.moveNext();
            var otherHasValue = otherWalker.moveNext();

            if (!selfHasValue && !otherHasValue) return true;
            if (selfHasValue !== otherHasValue) return false;

            var selfEl = selfWalker.current();
            var otherEl = otherWalker.current();

            if (!equalFunc) {
              if (selfEl !== otherEl) return false;
            } else {
              if (!equalFunc(selfEl, otherEl)) return false;
            }

          } while(true);
        }, //wEqual

        wToArray: function() {
                    var walkable = this;
                    var walker = walkable.getWalker();
                    var arr = [];
                    while(walker.moveNext()) arr.push(walker.current());

                    return arr;
        }, //wToArray

        wEach: function(action) {
                    var walkable = this;
                    var walker = walkable.getWalker();

                    var i = 0;
                    while(walker.moveNext()) action(walker.current(), i++);

                    return walkable;
        },  //wEach

        // "weight"-based moving average filter (k is kind of "weight" 0..1):
        // if k == 0 - moving object has no weight (hense no momentum) and outbound signal is the same as inbound,
        // if k == 1 - moving object has infinite weight (hense infinity momentum)
        // and outbound signal will be permanent (equal to first sample amplitude)
        wWMA: function(k) {
                    var srcWalkable = this;
                    if (typeof(k) === tUNDEFINED) k = 0.5;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        var prevOutA = null; // previous oubound (processed) amplitude
                        var currOutS = {s: -1, a: 0}; // current outbound (processed) sample

                        return {
                          reset: function() {
                            prevOutA = null;
                            currOutS = {s: -1, a: 0};
                            walker.reset();
                          },

                          moveNext: function() {
                            if (!walker.moveNext()) return false;

                            currOutS.s = walker.current().s;
                            var inA = walker.current().a;

                            if (currOutS.s === 0) {
                              currOutS.a = inA;
                            } else {
                              currOutS.a = (1-k) * inA + k * prevOutA;
                            }

                            prevOutA = currOutS.a;

                            return true;
                          },

                          current: function() { return currOutS; },

                          samplingRate: function() { return walker.samplingRate ? walker.samplingRate() : 2; }
                        };
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wWMA

        // Frequency filter takes:
        //  "m"(f=ma Newtons law) -  in virtual units of mass (i.e. kilograms) - the higher the number the more attenuation for high frequencies
        //  "k"(f=-kx Hooks law) - in virtual units of spring mimbrane resistance, the higher the number the more attenuation for low frequencies
        wHarmonicFrequencyFilter: function(m,k) {
                    var srcWalkable = this;
                    if (typeof(m) === tUNDEFINED) m = 1;
                    if (typeof(k) === tUNDEFINED) k = 1;
                    if (m<1.0001) m = 1.0001;
                    if (k<0.0001) k = 0.0001;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        var prevOutA = 0; // previous oubound (processed) amplitude
                        var currOutS = {s: -1, a: 0}; // current outbound (processed) sample
                        var v = 0;
                        var prevInA = 0;

                        return {
                          reset: function() {
                            prevOutA = 0;
                            v = 0;
                            prevInA = 0;
                            currOutS = {s: -1, a: 0};
                            walker.reset();
                          },

                          moveNext: function() {
                            if (!walker.moveNext()) return false;

                            currOutS.s = walker.current().s;
                            var inA = walker.current().a;


                            var da = inA - prevInA;
                            prevInA = inA;

                            v += (da - (k * prevOutA)) / m;

                            //console.log(da, v);

                            v*=0.98;//active resistance

                            currOutS.a = prevOutA + v;
                            prevOutA = currOutS.a;

                            return true;
                          },

                          current: function() { return currOutS; },

                          samplingRate: function() { return walker.samplingRate ? walker.samplingRate() : 2; }
                        };
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        }, //wHarmonicFrequencyFilter

        //SA vector definition
        // {s: int, a: float}
        // [s]ample = monotonously increasing integer value
        // [a]mplitude = floating point, the amplitude of the signal

        //Ensures that all {s, a} vectors in incoming walkable are equally spaced in time, so {a} component linearly interpolated between
        //otherwise descret points, i.e. [ {1, -47} , {10, 150} ] => F(2) => yields vectors where
        // respected components (0 -> 5, -47 -> 150)  get linearly pro-rated
        wConstLinearSampling: function(samplingStep) {
                                var srcWalkable = this;
                                var walkable = {
                                  getWalker: function() {
                                    samplingStep = samplingStep || 1;
                                    var walker = srcWalkable.getWalker();
                                    var walkerExausted = false, inSampleLastS = null;
                                    var halfSamplingStep = samplingStep / 2;
                                    var outSampleIdx = 0; // index sample number (natural integer)
                                    var outSampleT; // t of current sample (float point)
                                    var outSampleLeftBorder, outSampleRightBorder; // left and right borders of this sample (float point)
                                    var inSamplesBuf = [];
                                    var curr = {};

                                    function WW() {
                                      this.reset = function() {
                                        inSamplesBuf.splice(0, inSamplesBuf.length);
                                        outSampleIdx = 0;
                                        walkerExausted = false; inSampleLastS = null;
                                        walker.reset();
                                      };

                                      this.moveNext = function() {

                                        function cutBufFromLeft(left, right) {
                                          while(inSamplesBuf.length > 1 && inSamplesBuf[0].s < left && inSamplesBuf[1].s < right) inSamplesBuf.shift();
                                        }

                                        function hasBufNeighbourhoodSamples(left, right) {
                                          for(var i=0; i<inSamplesBuf.length; i++) {
                                            var inSampleS = inSamplesBuf[i].s;
                                            if (inSampleS > left && inSampleS <= right) return true;
                                          }
                                          return false;
                                        }

                                        function walkerMoveNext() {
                                          if (walker.moveNext()) {
                                            inSampleLastS = walker.current().s;
                                            return true;
                                          } else {
                                            walkerExausted = true;
                                            return false;
                                          }
                                        }

                                        function avgNeighbourhoodSamples(left, right) {
                                          var sum = 0, sumQty = 0;
                                          for(var i=0; i<inSamplesBuf.length; i++) {
                                            var inSample = inSamplesBuf[i];
                                            if (inSample.s > left && inSample.s <= right) {
                                              sum += inSample.a;
                                              sumQty++;
                                            }
                                          }
                                          var avg = sum / sumQty;
                                          return avg;
                                        }

                                        function fillBuf(right) {
                                          while(walkerMoveNext()) {
                                            if (walker.current().s >= right) {
                                              inSamplesBuf.push(walker.current());
                                              break;
                                            } else {
                                              inSamplesBuf.push(walker.current());
                                            }
                                          }
                                        }

                                        if (!outSampleIdx) { // first iteration
                                          if (!walkerMoveNext()) return false;

                                          inSamplesBuf.push(walker.current());

                                          outSampleT = walker.current().s;
                                          outSampleLeftBorder = outSampleT - halfSamplingStep;
                                          outSampleRightBorder = outSampleT + halfSamplingStep;

                                          fillBuf(outSampleRightBorder);

                                          curr.s = outSampleIdx++;
                                          var avg = avgNeighbourhoodSamples(-Number.MAX_VALUE, outSampleRightBorder);
                                          curr.a = avg;

                                          return true;
                                        }

                                        outSampleT += samplingStep;
                                        outSampleLeftBorder = outSampleT - halfSamplingStep;
                                        outSampleRightBorder = outSampleT + halfSamplingStep;

                                        if (walkerExausted && inSampleLastS < outSampleLeftBorder) return false;

                                        curr.s = outSampleIdx++;

                                        cutBufFromLeft(outSampleLeftBorder, outSampleRightBorder);

                                        if (hasBufNeighbourhoodSamples(outSampleLeftBorder, outSampleRightBorder)) {
                                          curr.a = avgNeighbourhoodSamples(outSampleLeftBorder, outSampleRightBorder);
                                          return true;
                                        }

                                        fillBuf(outSampleRightBorder);

                                        if (hasBufNeighbourhoodSamples(outSampleLeftBorder, outSampleRightBorder)) {
                                          curr.a = avgNeighbourhoodSamples(outSampleLeftBorder, outSampleRightBorder);
                                          return true;
                                        }

                                        var inSampleLeft = inSamplesBuf[0];
                                        var inSampleRight = inSamplesBuf[1];

                                        var k = (inSampleRight.a - inSampleLeft.a) / (inSampleRight.s - inSampleLeft.s);
                                        var a = inSampleLeft.a - k * inSampleLeft.s;

                                        curr.a = k * outSampleT + a;

                                        return true;
                                      };

                                      this.current = function() {
                                        return curr;//{int sample, float Amplitude}=> {s,a}
                                      };
                                    }

                                    return new WW();
                                  }
                                };

                                WAVE.extend(walkable, WAVE.Walkable);
                                return walkable;
        }, //wConstLinearSampling

        //Outputs {s,a} vectors per SINE function
        wSineGen: function(cfg) {
                    var srcWalkable = this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        cfg = cfg || {};
                        var f = cfg.f || 1.0; // Frequency in Hertz assuming that sampling rate is / second
                        var a = typeof(cfg.a) === tUNDEFINED ? 1 : cfg.a; // Amplitude
                        var d = cfg.d || 0; // DC Offset
                        var r = cfg.r || (walker.samplingRate ? walker.samplingRate() : 2); // Sampling Rate in arbitrary units (i.e. seconds)
                        var p = cfg.p || 0; // Phase offset in radians

                        if (r<2) r = 2;//Nyquist frequency

                        var deltaPeriod = (pi2 / r) * f; // the radian cost of 1 period of sine at supplied sampling rate multiplied by desired frequency

                        var sampleIdx = 0; // sample idx (sequental #)
                        var period = p; // current argument
                        var curr = {}; // current sample vector {s,a}

                        walker = srcWalkable.getWalker();
                        function WW() {
                          this.reset = function() { sampleIdx = 0; period = p; walker.reset(); };
                          this.moveNext = function() {
                            if (!walker.moveNext()) return false;
                            curr.s = sampleIdx++;
                            curr.a = (a * Math.sin(period)) + d + walker.current().a;
                            period = geometry.wrapAngle(period, deltaPeriod);
                            return true;
                          };
                          this.current = function() {
                            return curr;//{int sample, float Amplitude}=> {s,a}
                          };

                          this.samplingRate = function() { return r; };
                        }

                        return new WW();
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        },

        //Outputs {s,a} vectors per "Saw" function
        wSawGen: function(cfg) {
                    var srcWalkable = this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        cfg = cfg || {};
                        var f = cfg.f || 1.0; // Frequency in Hertz assuming that sampling rate is per second
                        var a = typeof(cfg.a) === tUNDEFINED ? 1 : cfg.a; // Amplitude
                        var d = cfg.d || 0; // DC Offset
                        var r = cfg.r || (walker.samplingRate ? walker.samplingRate() : 2); // Sampling Rate in arbitrary units (i.e. seconds)
                        var p = cfg.p || 0; // Phase offset in radians
                        var s = cfg.s || 0.9; // Symmetry (attack percentage (0..1))

                        if (r<2) r = 2;//Nyquist frequency

                        var k1 = 1 / pi / s;
                        var a1 = -1;

                        var k2 = 1 / pi / (s - 1);
                        var a2 = 1 - 2 * s / (s - 1);

                        var deltaPeriod = (pi2 / r) * f; // the radian cost of 1 period of saw at supplied sampling rate multiplied by desired frequency

                        var sampleIdx = 0; // sample idx (sequental #)
                        var period = p; // current argument
                        var curr = {}; // current sample vector {s,a}

                        walker = srcWalkable.getWalker();
                        function WW() {
                          this.reset = function() { sampleIdx = 0; period = p; walker.reset(); };
                          this.moveNext = function() {
                            if (!walker.moveNext()) return false;
                            curr.s = sampleIdx++;
                            var unitV;
                            if (period < pi2 * s)
                              unitV = k1 * period + a1;
                            else
                              unitV = k2 * period + a2;
                            curr.a = (a * unitV) + d + walker.current().a;
                            period = geometry.wrapAngle(period, deltaPeriod);
                            return true;
                          };

                          this.current = function() { return curr; };

                          this.samplingRate = function() { return r; };
                        }

                        return new WW();
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;

        },

        //Outputs {s,a} vectors per "Square" function
        wSquareGen: function(cfg) {
                    var srcWalkable = this;
                    var walkable = {
                      getWalker: function() {
                        var walker = srcWalkable.getWalker();
                        cfg = cfg || {};
                        var f = cfg.f || 1.0; // Frequency in Hertz assuming that sampling rate is per second
                        var a = typeof(cfg.a) === tUNDEFINED ? 1 : cfg.a; // Amplitude
                        var d = cfg.d || 0; // DC Offset
                        var r = cfg.r || (walker.samplingRate ? walker.samplingRate() : 2); // Sampling Rate in arbitrary units (i.e. seconds)
                        var p = cfg.p || 0; // Phase offset in radians
                        var s = cfg.s || 0.9; // Symmetry (attack percentage (0..1))

                        if (r<2) r = 2;//Nyquist frequency

                        var k1 = -2 / pi / (1 - s);
                        var a1 = 1 - k1 * pi * s;

                        var k2 = 2 / pi / (1 - s);
                        var a2 = -1 - k2 * pi * (1 + s);

                        var deltaPeriod = (pi2 / r) * f; // the radian cost of 1 period of sine at supplied sampling rate multiplied by desired frequency

                        var sampleIdx = 0; // sample idx (sequental #)
                        var period = p; // current argument
                        var curr = {}; // current sample vector {s,a}

                        walker = srcWalkable.getWalker();
                        function WW() {
                          this.reset = function() { sampleIdx = 0; period = p; walker.reset(); };
                          this.moveNext = function() {
                            if (!walker.moveNext()) return false;
                            curr.s = sampleIdx++;
                            var unitV;
                            if(period < pi) {
                              if (period < pi * s) unitV = 1;
                              else unitV = k1 * period + a1;
                            } else {
                              if (period < pi + pi * s) unitV = -1;
                              else unitV = k2 * period + a2;
                            }
                            curr.a = (a * unitV) + d + walker.current().a;
                            period = geometry.wrapAngle(period, deltaPeriod);
                            return true;
                          };

                          this.current = function() { return curr; };

                          this.samplingRate = function() { return r; };
                        }

                        return new WW();
                      }
                    };
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
        },

        wRandomGen: function(cfg) {
                      var srcWalkable = this;
                      var walkable = {
                        getWalker: function() {
                          var walker = srcWalkable.getWalker();
                          cfg = cfg || {};
                          var a = typeof(cfg.a) === tUNDEFINED ? 1 : cfg.a; // Amplitude
                          var r = cfg.r || (srcWalkable.samplingRate ? srcWalkable.samplingRate() : 2); // Sampling Rate in arbitrary units (i.e. seconds)

                          if (r<2) r = 2;//Nyquist frequency

                          var sampleIdx = 0; // sample idx (sequental #)
                          var curr = {}; // current sample vector {s,a}

                          walker = srcWalkable.getWalker();

                          function WW() {
                            this.reset = function() { walker.reset(); sampleIdx = 0; };
                            this.moveNext = function() {
                              if (!walker.moveNext()) return false;
                              curr.s = sampleIdx++;
                              curr.a = walker.current().a + 2 * a * (Math.random() - 0.5);
                              return true;
                            };
                            this.current = function() { return curr; };
                            this.samplingRate = function() { return r; };
                          }

                          return new WW();
                        }
                      };

                      WAVE.extend(walkable, WAVE.Walkable);
                      return walkable;
        }
    };// Walkable

    // walkable source of constant signal
    published.signalConstSrc = function(conf) {

              function WW(cfg) {
                var a = cfg.a || 0; // Amplitude
                var qty = cfg.qty; // Values count limit (if no value infinite set of values are genereted)
                var r = cfg.r || 2; // Sampling Rate in arbitrary units (i.e. seconds)
                if (r<2) r = 2;//Nyquist frequency

                var i=0;
                var curr = {};

                this.reset = function() { i = 0; };
                this.moveNext = function() {
                  if (qty && i >= qty) return false;

                  curr.s = i;
                  curr.a = a;

                  i++;
                  return true;
                };
                this.current = function() { return curr; };
                this.samplingRate = function() { return r; };
              }

      conf = conf || {};

      var walkable = { getWalker: function() { return new WW(conf); }};
      WAVE.extend(walkable, WAVE.Walkable);

      return walkable;
    };

    // Provides walking capability for array
    published.arrayWalkable = function(arr) {
      var walker = function() {
        var idx = -1;
        this.reset = function() { idx = -1; };
        this.moveNext = function() { return (++idx < arr.length); };
        this.current = function() { return (idx < 0 || idx >= arr.length) ? null : arr[idx]; };
      };

      var walkable = { getWalker: function() { return new walker(); }, OriginalArray: arr };
      WAVE.extend(walkable, WAVE.Walkable);
      return walkable;
    };

    // Converts array with {k : {}, v: []} structure to Walkable group operation result (inverse method to Walkable.wGroupToArray)
    published.groupWalkable = function(arr) {
      var walker = function() {
        var idx = -1;
        var cur = null;
        this.reset = function() { idx = -1; cur = null; };
        this.moveNext = function() {
          if (++idx < arr.length) {
            var arrCur = arr[idx];
            cur = { k: arrCur.k, v: WAVE.arrayWalkable(arrCur.v) };
            return true;
          } else {
            return false;
          }
          //Dkh1222015  return (++idx < arr.length);
        };
        this.current = function() { return cur; };
      };

      var walkable = { getWalker: function() { return new walker(); }, OriginalArray: arr};
      WAVE.extend(walkable, WAVE.Walkable);
      return walkable;
    };


    published.SVG = (function () {
      var svg = {};

      var SVG_NS = "http://www.w3.org/2000/svg";
      var SVG_XLINK_NS = "http://www.w3.org/1999/xlink";

      svg.createCircle = function (cx, cy, r, cfg) {
        cfg = cfg || {};
        cfg.cx = cx;
        cfg.cy = cy;
        cfg.r = r;

        return svg.createShape("circle", cfg);
      };

      svg.createRect = function (x, y, width, height, cfg) {
        cfg = cfg || {};
        cfg.x = x;
        cfg.y = y;
        cfg.width = width;
        cfg.height = height;

        return svg.createShape("rect", cfg);
      };

      svg.createLine = function (x1, y1, x2, y2, cfg) {
        cfg = cfg || {};
        cfg.x1 = x1;
        cfg.y1 = y1;
        cfg.x2 = x2;
        cfg.y2 = y2;

        return svg.createShape("line", cfg);
      };

      svg.createPath = function (cfg) {
        return svg.createShape("path", cfg);
      };

      svg.createText = function (txt, x, y, cfg) {
        cfg = cfg || {};
        cfg.x = x;
        cfg.y = y;
        var textWrapper = svg.createShape("text", cfg);
        var textEl = document.createTextNode(txt);
        textWrapper.appendChild(textEl);
        return textWrapper;
      };

      svg.createGroup = function (cfg) {
        return svg.createShape("g", cfg);
      };

      svg.createMarker = function (cfg, elems) {
        var marker = this.createShape("marker", cfg);

        for (var el in elems) marker.appendChild(elems[el]);

        return marker;
      };

      svg.createDefs = function (elems) {
        var defs = this.createShape("defs");

        for (var el in elems) defs.appendChild(elems[el]);

        return defs;
      };

      svg.createUse = function (defs, x, y) {
        var use = svg.createShape("use", { x: x, y: y });
        use.setAttributeNS(SVG_XLINK_NS, 'xlink:href', '#' + defs);
        return use;
      };

      svg.createClipPath = function (id, elems, cfg) {
        cfg = cfg || {};
        cfg.id = id;
        var clipPath = svg.createShape("clipPath", cfg);

        for (var el in elems) clipPath.appendChild(elems[el]);

        return clipPath;
      };

      svg.createShape = function (shapeType, cfg) {
        var el = document.createElementNS(SVG_NS, shapeType);

        for (var attrName in cfg) {
          try {
            el.setAttribute(attrName,cfg[attrName]);
          } catch(e) {
            console.error(e);
          }
        }
        return el;
      };

      return svg;
    }());





    return published;
}());//WAVE

//==================================================================================================
//==================================================================================================


WAVE.RecordModel = (function(){

    var tUNDEFINED = "undefined";

    var published = {

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
            ScriptType: null,
            DefaultValue: null,
            Hint:       null,
            Marked:     false,
            LookupDict: {},//{key:"description",...}]
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
        CTL_TP_NLS:     "nls",
        CTL_TP_SCRIPT:  "script",
        CTL_TP_TEXTAREA:"textarea",
        CTL_TP_HIDDEN:  "hidden",
        CTL_TP_PUZZLE:  "puzzle",
        CTL_TP_ERROR_REC:   "error-rec",
        CTL_TP_ERROR_SUMMARY:  "error-summary",

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
        CASE_CAPSNORM:  'capsnorm'
    };


    var fRecords = [];

    //Returns the copy of list of record instances
    published.records = function(){
        return WAVE.arrayShallowCopy(fRecords);
    };

    //Returns true when there is at least one record instance with user-made modifications
    published.isDirty = function(){
        for(var i in fRecords)
          if (fRecords[i].isGUIModified()) return true;
        return false;
    };


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

               if (fDef.Size>50)
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
                     !WAVE.strEmpty(fValue.toString()))//20140903 DKh+DLat
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
                      if (keys.length>0){
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



    published.DATA_RECVIEW_ID_ATTR = "data-wv-rid";
    published.DATA_FIELD_NAME_ATTR = "data-wv-fname";
    published.DATA_CTL_TP_ATTR = "data-wv-ctl";


    //RecordView class
    // id - required, unique in page id of the view
    // rec - required, data record instance
    // gui - GUI library, if null then default script "wv.gui.js" must be included
    // manualViews - if true then view controls will not be auto-constructed
    published.RecordView = function(id, rec, gui, manualViews)
    {
        if (WAVE.strEmpty(id)) throw "RecordView.ctor(id: must specify)";
        if (!WAVE.isObject(rec)) throw "RecordView.ctor(rec: must be Record)";
        if (!WAVE.isObject(gui)) gui = WAVE.RecordModel.GUI;//dflt GUI lib
        var fID = id;
        var fRecord = rec;
        var fGUI = gui;
        var recview = this;
        var fViews = [];
        var fRootElement = null;


        //Returns record view instance ID used for FieldView bindings
        this.ID = function(){ return fID;};

        //Returns bound record
        this.record = function(){ return fRecord;};

        //Returns GUI backend that renders controls for the view
        this.gui = function(){ return fGUI;};

        //Returns root element that this record view is building controls under
        this.rootElement = function() { return fRootElement;};


        //Unbinds and deletes all views
        this.destroyViews = function(){
          while(fViews.length>0) fViews[0].destroy();
        };

        var divHiddenFields = null;

        //Builds and binds view controls to record fields from declarative page markup: <...dava-wv-rid='r1'...> <div data-wv-fld='LastName' ... />
        this.buildViews = function(){
          fRootElement = null;
          this.destroyViews();

          var allForms = document.getElementsByTagName("form");
          for (var i=0, imax=allForms.length; i < imax; i++) {
            var frm = allForms[i];

            var irid = frm.getAttribute(published.DATA_RECVIEW_ID_ATTR);
            if (WAVE.strSame(fID, irid))
            {
                fRootElement = frm;
                break;
            }
          }

          if (fRootElement===null){
              var allDIVS = document.getElementsByTagName("div");
              for (var j=0, jmax=allDIVS.length; j < jmax; j++) {
                var div = allDIVS[j];
                var jrid = div.getAttribute(published.DATA_RECVIEW_ID_ATTR);
                if (WAVE.strSame(fID, jrid))
                {
                    fRootElement = div;
                    break;
                }
              }
          }

          if (fRootElement===null) throw "No 'form' or 'div' element could be found with record binding '"+published.DATA_RECVIEW_ID_ATTR+"' annotation";

          //find DIVs for views, need copy as DOM indexes will shift as views get added
          var allViewDIVS = WAVE.arrayShallowCopy( fRootElement.getElementsByTagName("div") );

          for (var k=0, kmax=allViewDIVS.length; k < kmax; k++) {
                var kdiv = allViewDIVS[k];
                var fname = kdiv.getAttribute(published.DATA_FIELD_NAME_ATTR);
                if (!WAVE.strEmpty(fname))
                {
                    if (fname==="#")//whole-record-level binding
                    {
                      new this.FieldView(kdiv, null);//bind to whole record
                    }
                    else
                    {
                      var fld = fRecord.fieldByName(fname);
                      if (fld!==null) new this.FieldView(kdiv, fld);
                    }
                }
          }


          if (!WAVE.strEmpty(fRecord.formMode()) || !WAVE.strEmpty(fRecord.csrfToken()) || !WAVE.strEmpty(fRecord.roundtrip()))
          {
             if (divHiddenFields===null)
             {
                divHiddenFields = document.createElement("div");
                divHiddenFields.style.display = "none";
                fRootElement.appendChild(divHiddenFields);
             }

             var content = "";

             if (!WAVE.strEmpty(fRecord.formMode()))
                content += "<input type='hidden' name='__FormMode' value='"+fRecord.formMode()+"'>";

             if (!WAVE.strEmpty(fRecord.csrfToken()))
                content +=  "<input type='hidden' name='__CSRFToken' value='"+fRecord.csrfToken()+"'>";

             if (!WAVE.strEmpty(fRecord.roundtrip()))
                content +=  "<input type='hidden' name='__Roundtrip' value='"+fRecord.roundtrip()+"'>";

             divHiddenFields.innerHTML = content;
          }

        };

        //Returns copy of view list
        this.views = function() {
           return WAVE.arrayShallowCopy(fViews);
        };


        //Individual field view class, if fld==null binds to whole record context
        this.FieldView = function(div, fld)
        {
            if (!div) throw "FieldView.ctor(div: element must be passed)";
            if (fld!==null && !WAVE.isObject(fld)) throw "FieldView.ctor(fld: must be Record.Field)";
            var fField = fld;
            var fieldview = this;
            var fDIV = div;
            fViews.push( fieldview );//register with parent

            if (fField!==null)
              fField.eventSinkBind(this);
            else
              fRecord.eventSinkBind(this);


            var recPropertyName = "";

            if (fld!==null)
            {
              recPropertyName = "fld"+fld.name();
              recview[recPropertyName] = fieldview;//make recview.fldLastName shortcut
            }

            //Invoked by control changes
            this.eventNotify = function(evtName, sender, phase){
              if (!WAVE.strOneOf(evtName,[WAVE.RecordModel.EVT_DATA_CHANGE,
                                          WAVE.RecordModel.EVT_INTERACTION_CHANGE,
                                          WAVE.RecordModel.EVT_VALIDATION_DEFINITION_CHANGE,
                                          WAVE.RecordModel.EVT_VALIDATED])) return;

              if (typeof(phase)===tUNDEFINED) phase = "";
              if (phase === WAVE.RecordModel.EVT_PHASE_BEFORE) return;

              if (fField!==null)
                fDIV.style.visibility = fField.visible() ? "visible" : "hidden";

              fGUI.eventNotifyFieldView( fieldview, evtName, sender, phase);
            };

            //Unbinds the view and deletes all internal markup
            this.destroy = function(){
                fDIV.innerHTML = "";//destroy control content
                if (fField!==null)
                  fField.eventSinkUnbind(this);
                else
                  fRecord.eventSinkUnbind(this);

                WAVE.arrayDelete(fViews, this);

                if (fField!==null)
                  delete recview[recPropertyName];
            };

            this.record = function(){ return fRecord; };
            this.recView = function() { return recview;};
            this.field = function(){ return fField; };


            //Returns root element in which the "visual control" gets built
            this.DIV = function(){ return fDIV; };

            //Gets control type specified on this view or infersfrom field
            this.getOrInferControlType = function(){
               var ctp = fDIV.getAttribute(published.DATA_CTL_TP_ATTR);
               if (!WAVE.strEmpty(ctp)) return ctp;
               return fField===null ? WAVE.RecordModel.CTL_TP_ERROR_REC : fField.getOrInferControlType();
            };

            if (fField===null || fField.visible())
                fGUI.buildFieldViewAnew( this );
        };//FieldView


        if (!manualViews) this.buildViews();
    };//RecordView



    return published;
}());//WAVE.RecordModel


//==================================================================================================
//==================================================================================================


WAVE.Pay = (function () {

    var tUNDEFINED = "undefined";

    var published = { Providers: { } };

    // Credit Card brands
    published.Brands =
    {
        VISA: "Visa",
        MASTER_CARD: "MasterCard",
        AMERICAN_EXPRESS: "AmEx",
        DISCOVER: "Discover",
        JCB: "JCB",
        DINERS_CLUB: "Diners Club",
        UNKNOWN: "Card"
    };

    // Error types
    published.ErrorTypes =
    {
        OK: 0,                    // Everything work as expected.
        API_CONNECTION_ERROR: 1,  // Failure to connect to Stripe's API.
        API_ERROR: 2,	            // API errors cover any other type of problem (e.g., a temporary problem with Stripe's servers) and are extremely uncommon.
        AUTHENTICATION_ERROR: 3,  // Failure to properly authenticate yourself in the request.
        CARD_ERROR: 4,	          // Card errors are the most common type of error you should expect to handle. They result when the user enters a card that can't be charged for some reason.
        INVALID_REQUEST_ERROR: 5, // Invalid request errors arise when your request has invalid parameters.
        RATE_LIMIT_ERROR: 6,	    // Too many requests hit the API too quickly.
        GENERAL_ERROR: 100
    };

    // Credit Card errors
    published.CardErrors =
    {
        OK: 0,                   // Everything work as expected.
        INVALID_NUMBER: 1,	     // The card number is not a valid credit card number.
        INVALID_EXPIRY_MONTH: 2, // The card's expiration month is invalid.
        INVALID_EXPIRY_YEAR: 3,  // The card's expiration year is invalid.
        INVALID_CVC: 4,          // The card's security code is invalid.
        INCORRECT_NUMBER: 5,     // The card number is incorrect.
        EXPIRED_CARD: 6,         // The card has expired.
        INCORRECT_CVC: 7,        // The card's security code is incorrect.
        INCORRECT_ZIP: 8,        // The card's zip code failed validation.
        CARD_DECLINED: 9,        // The card was declined.
        MISSING: 10,             // There is no card on a customer that is being charged.
        PROCESSING_ERROR: 11,    // An error occurred while processing the card.
        GENERAL_ERROR: 100
    };

    // Pay Terminal Facade

    published.Facade = function (providerName, provider, init) {
        if (WAVE.strEmpty(providerName))
            throw new WAVE.Pay.RequiredArgumentError("Facade.ctor()", "providerName");
        if (WAVE.strEmpty(provider))
            throw new WAVE.Pay.RequiredArgumentError("Facade.ctor()", "provider");
        if (WAVE.strEmpty(init))
            throw new WAVE.Pay.RequiredArgumentError("Facade.ctor()", "init");

        var facade = this;
        var fProviderName = providerName;

        // returns pay provider name
        this.providerName = function(){ return fProviderName; };

        WAVE.extend(facade, WAVE.EventManager);

        try {
            // Payment provider specific setup
            provider.initialize(init);
        }
        catch (error) {
            throw new WAVE.Pay.ProviderInitError(error);
        }

        // Payment provider tokenization interface:
        // paymentData - contains CC card data (cc_number, cc_cvc, cc_exp_month, cc_exp_year, cc_holder),
        // callback - a handler function that accepts 'result' object with fields: 'error' and 'token'.
        this.tokenize = function (paymentData, callback) {
            if (WAVE.strEmpty(paymentData))
                throw new WAVE.Pay.RequiredArgumentError("tokenize()", "paymentData");
            if (WAVE.strEmpty(callback))
                throw new WAVE.Pay.RequiredArgumentError("tokenize()", "callback");

            try {
                provider.tokenize(facade, paymentData, callback);
            }
            catch (error) {
                throw new WAVE.Pay.TokenizeError(error);
            }
        };
    };

    // Errors

    published.RequiredArgumentError = function (srcFuncName, argName, inner) {
        this.name = "RequiredArgumentError";
        this.message = srcFuncName + " requires argument '" + argName + "'";
        this.inner = inner;
    };
    published.RequiredArgumentError.prototype = Error.prototype;

    published.ProviderInitError = function (inner) {
        this.name = "ProviderInitError";
        var message = "Critical error during provider setup";
        if (typeof(inner) !== tUNDEFINED && !WAVE.strEmpty(inner.message))
            message += ": " + inner.message;
        this.message = message;
        this.inner = inner;
    };
    published.ProviderInitError.prototype = Error.prototype;

    published.TokenizeError = function (inner) {
        this.name = "TokenizeError";
        var message = "Critical error during tokenization";
        if (typeof(inner) !== tUNDEFINED && !WAVE.strEmpty(inner.message))
            message += ": " + inner.message;
        this.message = message;
        this.inner = inner;
    };
    published.TokenizeError.prototype = Error.prototype;

    published.CardValidationError = function (inner) {
        this.name = "CardValidationError";
        var message = "Card is invalid";
        if (typeof(inner) !== tUNDEFINED && !WAVE.strEmpty(inner.message))
            message += ": " + inner.message;
        this.message = message;
        this.inner = inner;
    };
    published.CardValidationError.prototype = Error.prototype;

    // Utilities

    published.validateCardNumber = function (ccNum) {
      if (WAVE.strEmpty(ccNum)) return false;
      ccNum = ccNum.replace(/\s+|-/g, "");
      return ccNum.length >= 10 && ccNum.length <= 16 && published.checkLuhn(ccNum);
    };

    published.checkLuhn = function (ccNum) {
      if (WAVE.strEmpty(ccNum)) return false;

      var nCheck = 0;
      var nDigit = 0;
      var bEven = false;
      ccNum = ccNum.replace(/\D/g, "");

         for (var n = ccNum.length - 1; n >= 0; n--) {
            var cDigit = ccNum.charAt(n);
            nDigit = parseInt(cDigit, 10);

            if (bEven) {
                if ((nDigit *= 2) > 9) nDigit -= 9;
            }

            nCheck += nDigit;
            bEven = !bEven;
        }

        return (nCheck % 10) === 0;
    };

    published.validateCVC = function (cvc) {
      if (WAVE.strEmpty(cvc)) return false;
      return cvc.length >= 3 && cvc.length <= 4 && /^\d+$/.test(cvc);
    };

    published.validateExpirationDate = function (month, year) {
      if (WAVE.strEmpty(month) || WAVE.strEmpty(year)) return false;
      if (year.length === 2) year = "20"+year;

      return /^\d+$/.test(month) &&
             /^\d+$/.test(year) &&
             1 <= month && month <= 12 &&
             year.length === 4 && year > 2015;
    };

    published.getCardBrand = function (ccNum) {
        if (WAVE.strEmpty(ccNum) || !/^\d{2}/.test(ccNum))
            return published.Brands.UNKNOWN;

        if (/^4/.test(ccNum)) return published.Brands.VISA;
        if (/^5[1-5]/.test(ccNum)) return published.Brands.MASTER_CARD;
        if (/^3[47]/.test(ccNum)) return published.Brands.AMERICAN_EXPRESS;
        if (/^(6011|622(12[6-9]|1[3-9][0-9]|[2-8][0-9]{2}|9[0-1][0-9]|92[0-5]|64[4-9])|65)/.test(ccNum)) return published.Brands.DISCOVER;
        if (/^35(2[89]|[3-8][0-9])/.test(ccNum)) return published.Brands.JCB;
        if (/^3[0689]/.test(ccNum)) return published.Brands.DINERS_CLUB;

        return published.Brands.UNKNOWN;
    };

    return published;

}()); // WAVE.Pay
