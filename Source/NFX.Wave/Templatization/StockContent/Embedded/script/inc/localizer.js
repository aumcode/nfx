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