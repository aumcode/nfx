published.strEmpty = function (str) { return (!str || 0 === str.length || /^\s*$/.test(str)); };

published.strAsBool = function (str, dflt) {
  if (typeof (str) === tUNDEFINED || str === null) return ((typeof (dflt) === tUNDEFINED) ? false : dflt);
  str = str.toString();
  return published.strOneOf(str, ["true", "t", "yes", "1"]);
};

published.strDefault = function (str, dflt) {
  return typeof (str) === tUNDEFINED || str === null ? (typeof (dflt) === tUNDEFINED || dflt === null ? '' : dflt) : str.toString();
};

published.strEmptyDefault = function (str, dflt) {
  return published.strEmpty(str) ? (typeof (dflt) === tUNDEFINED || dflt === null ? '' : dflt) : str.toString();
};

published.nlsNameDefault = function (nls, dflt) {
  var v = (typeof (nls) === tUNDEFINED || nls === null) ? null : nls.n;
  return published.strEmptyDefault(v, dflt);
};

published.nlsDescrDefault = function (nls, dflt) {
  var v = (typeof (nls) === tUNDEFINED || nls === null) ? null : nls.d;
  return published.strEmptyDefault(v, dflt);
};

published.nlsNameOrDescrDefault = function (nls, dflt) {
  var v = null;
  if (typeof (nls) !== tUNDEFINED && nls !== null) {
    v = nls.n;
    if (published.strEmpty(v)) v = nls.d;
  }
  return published.strEmptyDefault(v, dflt);
};

published.nlsDescrOrNameDefault = function (nls, dflt) {
  var v = null;
  if (typeof (nls) !== tUNDEFINED && nls !== null) {
    v = nls.d;
    if (published.strEmpty(v)) v = nls.n;
  }
  return published.strEmptyDefault(v, dflt);
};


published.strTrim = function (str) { return str.replace(/^\s+|\s+$/g, ''); };
published.strLTrim = function (str) { return str.replace(/^\s+/, ''); };
published.strRTrim = function (str) { return str.replace(/\s+$/, ''); };

// Truncates str if its length exceeds maxLen and adds endWith string to result end.
published.strTrunc = function (str, maxLen, endWith) {
  if (!str) return str;
  var len = str.length;
  if (len <= maxLen) return str;
  endWith = endWith || "...";
  return str.substr(0, maxLen - endWith.length) + endWith;
};

//Capitalizes first chars after spaces or dots, otionally converting chars in between to lower case
published.strCaps = function (str, norm) {
  //this does not use Regexp because regexp does not detect words correctly for non-english languages
  if (published.strEmpty(str)) return str;
  var c;
  var result = "";
  var sp = false;
  for (var i = 0; i < str.length; i++) {
    c = str[i];
    if (c === ' ' || c === '.') {
      sp = true;
      result += c;
      continue;
    }

    if (sp || i === 0)
      result += c.toUpperCase();
    else
      result += norm ? c.toLowerCase() : c;
    sp = false;
  }
  return result;
};

published.joinPathSegs = function () {
  if (arguments.length === 0) return '';
  var s = '';
  var first = true;
  for (var i = 0; i < arguments.length; i++) {
    var seg = arguments[i];
    if (!seg) continue;

    seg = (first ? WAVE.strLTrim(seg) : seg.replace(/^[\/\\\s]+/, '')).replace(/[\/\\\s]+$/, '');
    if (WAVE.strEmpty(seg)) continue;

    if (!first) s += '/';
    s += seg;
    first = false;
  }
  return s;
};

published.mapCurrencyISOToSymbol = function (ciso, dflt) {
  ciso = published.strDefault(ciso, "");
  ciso = ciso.toUpperCase();
  switch (ciso) {
    case "USD":
      return "$";
    case "RUB":
      return "₽";
    case "EUR":
      return "€";
    case "GBP":
      return "£";
    default:
      return published.strDefault("?", dflt);
  }
};

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
published.charIsAZLetterOrDigit = function (c) {
  if (c === null) return false;
  return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9');
};

published.strStartsWith = function (str, s, scase) {
  return scase ? str.slice(0, s.length) === s : str.slice(0, s.length).toLowerCase() === s.toLowerCase();
};

published.strEndsWith = function (str, s, scase) {
  return scase ? str.slice(-s.length) === s : str.slice(-s.length).toLowerCase() === s.toLowerCase();
};

// Ensures that string ends with the specified string: strEnsureEnding("path",'/')
published.strEnsureEnding = function (str, ending) {
  return str + (str.slice(-ending.length) === ending ? '' : ending);
};

//Returns true when str contains a seg optionally respecting case
published.strContains = function (str, seg, scase) {
  return scase ? str.indexOf(seg) > -1 : str.toLowerCase().indexOf(seg.toLowerCase()) > -1;
};

//Returns true if both string contain the same trimmed case-insensitive value.
//This method is usefull for tasks like searches of components by name
published.strSame = function (str1, str2) {
  if (typeof (str1) === tUNDEFINED || typeof (str2) === tUNDEFINED) return false;
  if (str1 === null || str2 === null) return false;
  return published.strTrim(str1).toLowerCase() === published.strTrim(str2).toLowerCase();
};

//Returns true if the case-insensitive trimmed string is in the set of values
//Neither string nor set value may contain delimiter which is '|' by default:
//   strOneOf("car",["car","house","tax"],';')
published.strOneOf = function (str1, set, del) {
  if (str1 === null || set === null || !published.isArray(set)) return false;
  if (!del) del = "|";
  str1 = del + published.strTrim(str1).toLowerCase() + del;
  var vset = (del + set.join(del) + del).toLowerCase();
  return vset.indexOf(str1) >= 0;
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

published.DATE_TIME_FORMATS = {
      LONG_DATE: "LongDate",
      SHORT_DATE: "ShortDate",
      LONG_DATE_TIME: "LongDateTime",
      SHORT_DATE_TIME: "ShortDateTime",
      TRAN_DATE_TIME: "TranDateTime",
      SHORT_DAY_MONTH: "ShortDayMonth"
    };

    var monthNames = ["January",
                      "February",
                      "March",
                      "April",
                      "May",
                      "June",
                      "July",
                      "August",
                      "September",
                      "October",
                      "November",
                      "December"];

    var dayNames = [ 'Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

    published.dateTimeToString = function(date, format, langIso) {
      if (typeof(date) === tUNDEFINED || date === null) return "";

      function addLeadingZero(num) {
        return ('0' + num.toString()).slice(-2);
      }

      function getMN(idx) {
        return published.strLocalize(langIso, "date", "", monthNames[idx]);
      }

      function gM() { return addLeadingZero(fDate.getMonth() + 1); }
      function gD() { return addLeadingZero(fDate.getDate()); }
      function gY() { return fDate.getFullYear(); }
      function gH() { return addLeadingZero(fDate.getHours()); }
      function gMin() { return addLeadingZero(fDate.getMinutes()); }
      function gS() { return addLeadingZero(fDate.getSeconds()); }

      var fFormat = WAVE.strDefault(format, published.DATE_TIME_FORMATS.LONG_DATE);
      var fDate = WAVE.isString(date) ? new Date(date) : date;
      switch (fFormat)
      {
        case published.DATE_TIME_FORMATS.LONG_DATE:
          return getMN(fDate.getMonth()) + " " + gD() + ", " + gY();
        case published.DATE_TIME_FORMATS.SHORT_DATE:
          return gM() + "/" + gD() + "/" + gY();
        case published.DATE_TIME_FORMATS.LONG_DATE_TIME:
          return getMN(fDate.getMonth()) + " " + gD() + ", " + gY() + " " + gH() + ":" + gMin() + ":" + gS();
        case published.DATE_TIME_FORMATS.SHORT_DATE_TIME:
          return gM() + "/" + gD() + "/" + gY() + " " + gH() + ":" + gMin();
        case published.DATE_TIME_FORMATS.TRAN_DATE_TIME:
          return gM() + "/" + gD() + "/" + gY() + " " + gH() + ":" + gMin() + ":" + gS();
        case published.DATE_TIME_FORMATS.SHORT_DAY_MONTH:
          return gM() + "/" + gD();
      }
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