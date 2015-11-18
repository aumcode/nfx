using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Environment;
using NFX.DataAccess.CRUD;
using NFX.Erlang;

namespace NFX.DataAccess.Erlang
{

  /// <summary>
  /// Maps Erlang schemas -> CRUD and CRUD -> Erlang
  /// </summary>
  public class SchemaMap
  {
    public const string CONFIG_SCHEMA_SECTION = "schema";
    public const string CONFIG_FIELD_SECTION = "field";
    public const string CONFIG_VALUE_SECTION = "value";

     public const string CONFIG_TITLE_ATTR = "title";
     public const string CONFIG_TYPE_ATTR = "type";
     public const string CONFIG_REQUIRED_ATTR = "required";
     public const string CONFIG_KEY_ATTR = "key";
     public const string CONFIG_DESCR_ATTR = "descr";
     public const string CONFIG_DEFAULT_ATTR = "default";
     public const string CONFIG_LEN_ATTR = "len";
     public const string CONFIG_VISIBLE_ATTR = "visible";
     public const string CONFIG_DISPLAY_FORMAT_ATTR = "display-format";

     public const string CONFIG_CODE_ATTR = "code";
     public const string CONFIG_DISPLAY_ATTR = "display";

     private const string SCHEMA_KEY_COUNT = "KEY_COUNT";

    public SchemaMap(ErlDataStore store, string xmlContent)
    {
      m_Store = store;
      m_ErlSchema = XMLConfiguration.CreateFromXML(xmlContent);
    }

    private ErlDataStore m_Store;
    private XMLConfiguration m_ErlSchema;
    private Registry<Schema> m_CRUDSchemas = new Registry<Schema>();



    public ErlDataStore Store{ get{ return m_Store;} }

    /// <summary>
    /// Enumerates all Erl schemas
    /// </summary>
    public IEnumerable<IConfigSectionNode> ErlSchemaSections
    {
      get { return m_ErlSchema.Root.Children.Where( c => c.IsSameName(CONFIG_SCHEMA_SECTION)); }
    }


    /// <summary>
    /// Returns config section for named erl schema or null
    /// </summary>
    public IConfigSectionNode GetErlSchemaSection(string name)
    {
      return ErlSchemaSections.FirstOrDefault( c => c.IsSameNameAttr(name));
    }

    public Schema GetCRUDSchemaForName(string name)
    {
      return m_CRUDSchemas.GetOrRegister(name, (nm) => erlSchemaToCRUDSchema(nm), name);
    }

    /// <summary>
    /// Converts ErlCRUD response to CLR CRUD rowset
    /// </summary>
    /// <remarks>
    /// An Example data packet is field defs as speced in schema:
    /// "tca_jaba": has two field in PK
    /// [
    ///    {tca_jaba, {1234, tav}, "User is cool", true},
    ///    {tca_jaba, {2344, zap}, "A bird wants to drink", false}, 
    ///    {tca_jaba, {8944, tav}, "Have you seen this?", false} 
    /// ]
    /// 
    /// "aaa": has one field in PK - notice no tuple in key
    /// [
    ///    {aaa, 1234, tav, "User is cool", true},
    ///    {aaa, 2344, zap, "A bird wants to drink", false}, 
    ///    {aaa, 8944, tav, "Have you seen this?", false} 
    /// ]
    /// </remarks>
    public RowsetBase ErlCRUDResponseToRowset(string schemaName, ErlList erlData)
    {
      var crudSchema = GetCRUDSchemaForName(schemaName);
      var result = new Rowset(crudSchema);

      foreach(var elm in erlData)
      {
        var tuple = elm as ErlTuple;
        if (tuple==null)
          throw new ErlDataAccessException(StringConsts.ERL_DS_INVALID_RESPONSE_PROTOCOL_ERROR+"ErlCRUDResponseToRowset(list element is not tuple)");

        var row = ErlTupleToRow(schemaName, tuple, crudSchema);
        result.Add( row );
      }

      return result;
    }

    /// <summary>
    /// Maps ErlRow to CLR row supplied by schema, either Dynamic or TypedRow 
    /// </summary>
    public Row ErlTupleToRow(string schemaName, ErlTuple tuple, Schema schema)
    {
      var singleKey = schema.ExtraData[SCHEMA_KEY_COUNT].AsInt(0) < 2;

      var row = Row.MakeRow(schema, schema.TypedRowType);

      var i = -1;
      foreach(var elm in enumErlResponseTuple(tuple, singleKey))
      {
        i++;
        if (i==0)
        {
          var thisname = tuple[0].ValueAsString;
          if (!schemaName.EqualsOrdSenseCase(thisname))
            throw new ErlDataAccessException(StringConsts.ERL_DS_CRUD_RESPONSE_SCHEMA_MISMATCH_ERROR.Args(thisname, schemaName));
          continue;
        }

        //map fields now
        if (i-1 >= schema.FieldCount)
          throw new ErlDataAccessException(StringConsts.ERL_DS_CRUD_RESPONSE_SCHEMA_FLD_COUNT_MISMATCH_ERROR.Args(schemaName, schema.Name));
        var fdef = schema[i-1];
        var atr = fdef[m_Store.TargetName];

        var erlType = atr.BackendType;
        if (erlType.IsNullOrWhiteSpace())
          throw new ErlDataAccessException(StringConsts.ERL_DS_INTERNAL_MAPPING_ERROR+"fielddef('{0}') has no backend type".Args(fdef.Name));


        if (elm.IsNull())
        {
          row.SetFieldValue(fdef, null);
          continue;
        }

        Tuple<Type, Func<IErlObject, object>> mapping;
        if (ERL_TO_CLR_TYPEMAP.TryGetValue(erlType, out mapping))
        {
          var clrValue = mapping.Item2(elm);
          row.SetFieldValue(fdef, clrValue);
        }
        else throw new ErlDataAccessException(StringConsts.ERL_DS_INTERNAL_MAPPING_ERROR+"erltype'{0}' not matched in the dict".Args(erlType));
      }
  
      return row;
    }

    /// <summary>
    /// Converts CLR row data into ErlTuple
    /// </summary>
    public ErlTuple RowToErlTuple(Row row, bool keysOnly = false)
    {
      var result = new ErlTuple();
      
      result.Add(new ErlAtom(row.Schema.Name));

      var keys = new List<IErlObject>();

      foreach(var def in row.Schema)
      {
        var atr = def[m_Store.TargetName];

        if (keysOnly && !atr.Key) break;

        if (keys!=null && !atr.Key)
        {
          if (keys.Count>1)
            result.Add( new ErlTuple(keys, false) );
          else
            foreach(var key in keys) result.Add( key ); 
          keys = null;
        }


        IErlObject erlValue = ErlAtom.Undefined;

        var clrValue = row.GetFieldValue(def);
        if (clrValue!=null)
        {
           Func<object, IErlObject> fconv;
           if (!CLR_TO_ERL_TYPEMAP.TryGetValue(atr.BackendType, out fconv))
            throw new ErlDataAccessException(StringConsts.ERL_DS_INTERNAL_MAPPING_ERROR+"erltype'{0}' not matched in the dict".Args(atr.BackendType));

           erlValue = fconv(clrValue);
        }
        
        if (keys!=null)
          keys.Add( erlValue );
        else
          result.Add( erlValue );
      }

      if (keys!=null)
      {
        if (keys.Count>1)
         result.Add( new ErlTuple(keys, false) );
        else
         foreach(var key in keys) result.Add( key ); 
      }

      return result;
    }




    #region .pvt

      private IEnumerable<IErlObject> enumErlResponseTuple(ErlTuple tuple, bool singleKey)
      {
         if (singleKey)
         {
           foreach(var elm in tuple)
            yield return elm;

           yield break;
         }
         
         for(var i=0; i<tuple.Count; i++)
         {
            var elm = tuple[i];
            if (i==1)
            {
              var ktuple = elm as ErlTuple;
              if (ktuple==null)
               throw new ErlDataAccessException(StringConsts.ERL_DS_INTERNAL_MAPPING_ERROR+"key tuple is missing");

              foreach(var keye in ktuple)
               yield return keye;
              continue;
            }
            yield return elm;
         }
      }


      private Schema erlSchemaToCRUDSchema(string name)
      {
        var erlSect = GetErlSchemaSection(name);
        if (erlSect==null) 
         throw new ErlDataAccessException(StringConsts.ERL_DS_SCHEMA_MAP_NOT_KNOWN_ERROR.Args(name));

        var defs = new List<Schema.FieldDef>();

        var keyCount = 0;
        var nodeFields = erlSect.Children.Where(c => c.IsSameName(CONFIG_FIELD_SECTION));
        foreach(var nodeField in nodeFields)
        {
          var fname   = nodeField.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;
          var ftitle  = nodeField.AttrByName(CONFIG_TITLE_ATTR).Value;
          var isKey   = nodeField.AttrByName(CONFIG_KEY_ATTR).ValueAsBool();
          var required= nodeField.AttrByName(CONFIG_REQUIRED_ATTR).ValueAsBool(false);
          var type = nodeField.AttrByName(CONFIG_TYPE_ATTR).Value;
          var clrType = mapErlTypeToCLR(type);

          var strDfltValue = nodeField.AttrByName(CONFIG_DEFAULT_ATTR).ValueAsString(string.Empty);
          object dfltValue = null;
          
          if (clrType!=typeof(string))
          {
            if (strDfltValue.IsNotNullOrWhiteSpace())
            {
               if (clrType==typeof(DateTime?))
                dfltValue = ((long)strDfltValue.AsType(typeof(long), false)).FromMicrosecondsSinceUnixEpochStart();
               else
                dfltValue = strDfltValue.AsType(clrType, false);
            }
          }
          else
          {
            dfltValue = strDfltValue;
          }

          if (isKey) keyCount++;

          List<string> vList = null;
                   
          var values = nodeField.Children.Where( c => c.IsSameName(CONFIG_VALUE_SECTION));
          foreach(var vnode in values)
          {
            var code = vnode.AttrByName(CONFIG_CODE_ATTR).Value;
            var disp = vnode.AttrByName(CONFIG_DISPLAY_ATTR).Value;
            if (code.IsNullOrWhiteSpace()) continue;

            if (vList==null) vList = new List<string>();
            if (disp.IsNullOrWhiteSpace())
              vList.Add(code);
            else
              vList.Add("{0}: {1}".Args(code, disp));
          }


          var atr = new FieldAttribute(
                         targetName: m_Store.TargetName, 
                         backendName: fname,
                         backendType: type,
                         storeFlag: StoreFlag.LoadAndStore,
                         key: isKey,
                         required: required,
                         dflt: dfltValue,

                         visible:   nodeField.AttrByName(CONFIG_VISIBLE_ATTR).ValueAsBool(true),
                         maxLength: nodeField.AttrByName(CONFIG_LEN_ATTR).ValueAsInt(0),
                         description: nodeField.AttrByName(CONFIG_DESCR_ATTR).Value,
                         displayFormat: nodeField.AttrByName(CONFIG_DISPLAY_FORMAT_ATTR).Value,
                         valueList: vList==null ? null : string.Join(",", vList),//"CAR: Car Driver,SMK: Smoker, REL: Religious, CNT: Country music lover, GLD: Gold collector"
                         metadata: nodeField.ToLaconicString());

          var def = new Schema.FieldDef(ftitle ?? fname, clrType, new FieldAttribute[]{atr});
          defs.Add( def );
        }//for fields 

        var result = new Schema(name, false, defs.ToArray());

        result.ExtraData[SCHEMA_KEY_COUNT] = keyCount; 

        return result;
      }

      private Type mapErlTypeToCLR(string erlType)
      {
        Tuple<Type, Func<IErlObject, object>> to;
        if (ERL_TO_CLR_TYPEMAP.TryGetValue(erlType, out to)) return to.Item1;

        throw new ErlDataAccessException(StringConsts.ERL_DS_SCHEMA_MAP_ERL_TYPE_ERROR.Args(erlType));
      }
    
      private static readonly Dictionary<string, Tuple<Type, Func<IErlObject, object>>> ERL_TO_CLR_TYPEMAP = new Dictionary<string,Tuple<Type,Func<IErlObject,object>>>
      {
        {"atom",     Tuple.Create<Type, Func<IErlObject, object>>(typeof(string),   (erl) => erl.IsNull() ? (string)null : erl.ValueAsString)},
        {"string",   Tuple.Create<Type, Func<IErlObject, object>>(typeof(string),   (erl) => erl.IsNull() ? (string)null : erl.ValueAsString)},
        {"char",     Tuple.Create<Type, Func<IErlObject, object>>(typeof(string),   (erl) => erl.IsNull() ? (string)null : erl.ValueAsString)},
        {"long",     Tuple.Create<Type, Func<IErlObject, object>>(typeof(long?),    (erl) => erl.IsNull() ? (long?)null     :erl.ValueAsLong)},
        {"double",   Tuple.Create<Type, Func<IErlObject, object>>(typeof(double?),  (erl) => erl.IsNull() ? (double?)null   :erl.ValueAsDouble)},
        {"datetime", Tuple.Create<Type, Func<IErlObject, object>>(typeof(DateTime?),(erl) => erl.IsNull() ? (DateTime?)null :erl.ValueAsDateTime)},
        {"pid",      Tuple.Create<Type, Func<IErlObject, object>>(typeof(string),   (erl) => erl.IsNull() ? (string)null :erl.ValueAsString)},
        {"bool",     Tuple.Create<Type, Func<IErlObject, object>>(typeof(bool?),    (erl) => erl.IsNull() ? (bool?)null  :erl.ValueAsBool)},
        {"bin",      Tuple.Create<Type, Func<IErlObject, object>>(typeof(byte[]),   (erl) => erl.IsNull() ? (byte[])null :erl.ValueAsByteArray)},
        {"binstr",   Tuple.Create<Type, Func<IErlObject, object>>(typeof(string),   (erl) => erl.IsNull() ? (string)null : Encoding.UTF8.GetString(erl.ValueAsByteArray))},
        {"ref",      Tuple.Create<Type, Func<IErlObject, object>>(typeof(string),   (erl) => erl.IsNull() ? (string)null :erl.ValueAsString)}
      };


      private static readonly Dictionary<string, Func<object, IErlObject>> CLR_TO_ERL_TYPEMAP = new Dictionary<string, Func<object,IErlObject>>
      {
        {"atom",     (clr) => new ErlAtom(clr.ToString())},
        {"string",   (clr) => new ErlString(clr.ToString())},
        {"char",     (clr) => new ErlByte(clr.ToString()[0])},
        {"long",     (clr) => new ErlLong(clr.AsLong(handling: ConvertErrorHandling.Throw))},
        {"double",   (clr) => new ErlDouble(clr.AsDouble(handling: ConvertErrorHandling.Throw))},
        {"datetime", (clr) => new ErlLong(clr.AsDateTime().ToMicrosecondsSinceUnixEpochStart() )},
        {"pid",      (clr) => ErlPid.Parse(clr.ToString())},
        {"bool",     (clr) => new ErlBoolean(clr.AsBool(handling: ConvertErrorHandling.Throw))},
        {"bin",      (clr) => new ErlBinary( (byte[])clr)},
        {"binstr",   (clr) => new ErlBinary( clr==null ? new byte[0] : Encoding.UTF8.GetBytes((string)clr))},
        {"ref",      (clr) => ErlRef.Parse(clr.ToString())}
      };


    #endregion

  }
}
