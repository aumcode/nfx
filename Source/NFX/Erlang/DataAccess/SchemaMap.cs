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
    public const string CONFIG_SCHEMA_SECTION      = "schema";
    public const string CONFIG_FIELD_SECTION       = "field";
    public const string CONFIG_VALUE_SECTION       = "value";

    public const string CONFIG_TITLE_ATTR          = "title";
    public const string CONFIG_TYPE_ATTR           = "type";
    public const string CONFIG_REQUIRED_ATTR       = "required";
    public const string CONFIG_MIN_ATTR            = "min";
    public const string CONFIG_MAX_ATTR            = "max";
    public const string CONFIG_KEY_ATTR            = "key";
    public const string CONFIG_DESCR_ATTR          = "descr";
    public const string CONFIG_DEFAULT_ATTR        = "default";
    public const string CONFIG_LEN_ATTR            = "len";
    public const string CONFIG_VISIBLE_ATTR        = "visible";

    public const string CONFIG_CASE_ATTR           = "case";

    public const string CONFIG_FORMAT_ATTR         = "format";
    public const string CONFIG_FORMAT_DESCR_ATTR   = "format-descr";
    public const string CONFIG_DISPLAY_FORMAT_ATTR = "display-format";

    public const string CONFIG_CODE_ATTR           = "code";
    public const string CONFIG_DISPLAY_ATTR        = "display";

    public const string CONFIG_INSERT_ATTR         = "insert";
    public const string CONFIG_UPDATE_ATTR         = "update";
    public const string CONFIG_DELETE_ATTR         = "delete";

    private const string SCHEMA_KEY_COUNT          = "KEY_COUNT";

    public SchemaMap(ErlDataStore store, string xmlContent)
    {
      m_Store = store;
      m_ErlSchema = XMLConfiguration.CreateFromXML(xmlContent);
      m_OriginalXMLContent = xmlContent;
    }

    private string m_OriginalXMLContent;
    private ErlDataStore m_Store;
    private XMLConfiguration m_ErlSchema;
    private Registry<Schema> m_CRUDSchemas = new Registry<Schema>();


    internal volatile bool _NeedReconnect;


    public ErlDataStore Store{ get{ return m_Store;} }


    public string OriginalXMLContent{ get{ return m_OriginalXMLContent;}}

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
    public RowsetBase ErlCRUDResponseToRowset(string schemaName, ErlList erlData, Type tRow = null)
    {
      var crudSchema = GetCRUDSchemaForName(schemaName);
      var tSchema    = tRow == null
        ? crudSchema
        : Schema.GetForTypedRow(tRow);

      var result = new Rowset(tSchema);

      foreach(var elm in erlData)
      {
        var tuple = elm as ErlTuple;
        if (tuple==null)
          throw new ErlDataAccessException(StringConsts.ERL_DS_INVALID_RESP_PROTOCOL_ERROR+"ErlCRUDResponseToRowset(list element is not tuple)");

        var row = ErlTupleToRow(schemaName, tuple, crudSchema);
        if (tRow != null)
        {
          var trow = Row.MakeRow(tSchema, tRow);
          row.CopyFields(trow);
          row = trow;
        }
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
            throw new ErlDataAccessException(StringConsts.ERL_DS_CRUD_RESP_SCH_MISMATCH_ERROR.Args(thisname, schemaName));
          continue;
        }

        //map fields now
        if (i-1 >= schema.FieldCount)
          throw new ErlDataAccessException(StringConsts.ERL_DS_CRUD_RESP_SCH_FLD_COUNT_ERROR.Args(schemaName, schema.Name));
        var fdef = schema[i-1];
        var atr = fdef[m_Store.TargetName];

        var erlType = atr.BackendType;
        if (erlType.IsNullOrWhiteSpace())
          throw new ErlDataAccessException(StringConsts.ERL_DS_INTERNAL_MAPPING_ERROR+"fielddef('{0}') has no backend type".Args(fdef.Name));

        var clrValue = ErlToClrValue(elm, schema, fdef, m_Store.TargetName, tuple);
        row.SetFieldValue(fdef, clrValue);
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
          erlValue = ClrToErlValue(atr.BackendType, clrValue);

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

    internal static IErlObject ClrToErlValue(string dataType, object clrValue)
    {
      Func<object, IErlObject> fconv;
      if (!CLR_TO_ERL_TYPEMAP.TryGetValue(dataType, out fconv))
        throw new ErlDataAccessException(StringConsts.ERL_DS_INTERNAL_MAPPING_ERROR +
                                         "ErlType '{0}' not found in the type dictionary".Args(dataType));
      return fconv(clrValue);
    }

    internal static object ErlToClrValue(IErlObject erlValue, Schema schema, Schema.FieldDef fdef, string targetName = null, IErlObject outerTerm = null)
    {
      if (erlValue.IsNull())
        return null;

      var atr = fdef[targetName];

      var dataType = atr.BackendType;
      if (dataType.IsNullOrWhiteSpace())
        throw new ErlDataAccessException(StringConsts.ERL_DS_INTERNAL_MAPPING_ERROR +
                                         "fielddef '{0}.{1}' has no backend type".Args(schema.Name, fdef.Name));

      Tuple<Type, Func<IErlObject, object>> mapping;
      if (ERL_TO_CLR_TYPEMAP.TryGetValue(dataType, out mapping))
      {
        try
        {
          return mapping.Item2(erlValue);
        }
        catch (Exception error)
        {
          var err = "Schema '{0}' field '{1}' ({2}) cannot be converted from '{3}' to '{4}' format{5}"
            .Args(schema.Name, fdef.Name, erlValue.ToString(), erlValue.GetType(), dataType,
            outerTerm == null ? "" : " in record:\n  {0}".Args(outerTerm.ToString()));
          App.Log.Write(new Log.Message
          {
            Type      = Log.MessageType.TraceErl,
            Topic     = CoreConsts.ERLANG_TOPIC,
            From      = "SchemaMap.ErlTupleToRow({0})".Args(schema.Name),
            Text      = err + error.ToMessageWithType(),
            Exception = error
          });
          throw new ErlDataAccessException(err, inner: error);
        }
      }

      var er = StringConsts.ERL_DS_INTERNAL_MAPPING_ERROR +
               "erltype'{0}' not matched in the dict".Args(dataType);
      throw new ErlDataAccessException(er);
    }

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
      if (erlSect == null)
        throw new ErlDataAccessException(StringConsts.ERL_DS_SCHEMA_MAP_NOT_KNOWN_ERROR.Args(name));

      return ErlSchemaToCRUDSchema(name, erlSect);
    }

    public static Schema ErlSchemaToCRUDSchema(string name, IConfigSectionNode erlSect)
    {
      var defs       = new List<Schema.FieldDef>();

      var isInsert   = erlSect.AttrByName(CONFIG_INSERT_ATTR).ValueAsBool(false);
      var isUpdate   = erlSect.AttrByName(CONFIG_UPDATE_ATTR).ValueAsBool(false);
      var isDelete   = erlSect.AttrByName(CONFIG_DELETE_ATTR).ValueAsBool(false);
      var schDescr   = erlSect.AttrByName(CONFIG_DESCR_ATTR).ValueAsString();

      var isReadonly = !(isInsert || isUpdate || isDelete);

      var keyCount = 0;
      var nodeFields = erlSect.Children.Where(c => c.IsSameName(CONFIG_FIELD_SECTION));
      foreach(var nodeField in nodeFields)
      {
        var cfg      = new LaconicConfiguration();
        cfg.CreateFromNode(nodeField);
        var node     = cfg.Root;

        var fname    = node.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;
        var ftitle   = node.AttrByName(CONFIG_TITLE_ATTR).Value;
        var fhint    = node.AttrByName(CONFIG_DESCR_ATTR).ValueAsString("");
        var isKey    = node.AttrByName(CONFIG_KEY_ATTR).ValueAsBool();
        var required = node.AttrByName(CONFIG_REQUIRED_ATTR).ValueAsBool(false);
        var type     = node.AttrByName(CONFIG_TYPE_ATTR).Value;
        var clrType  = mapErlTypeToCLR(type);
        var fmtdesc  = node.AttrByName(CONFIG_FORMAT_DESCR_ATTR).Value;
        var dispFmt  = node.AttrByName(CONFIG_DISPLAY_FORMAT_ATTR).Value;

        if (dispFmt != null && (dispFmt.Length < 3 || dispFmt.Substring(0, 3) != "{0:"))
          dispFmt = "{{0:{0}}}".Args(dispFmt);

        node.AddAttributeNode(CONFIG_TITLE_ATTR, ftitle);

        object minV  = null;
        object maxV  = null;

        var sv = node.AttrByName(CONFIG_MIN_ATTR).Value;
        if (sv.IsNotNullOrWhiteSpace()) minV = sv.AsType(clrType, true);

        sv = node.AttrByName(CONFIG_MAX_ATTR).Value;
        if (sv.IsNotNullOrWhiteSpace()) maxV = sv.AsType(clrType, true);


        var strDfltValue = node.AttrByName(CONFIG_DEFAULT_ATTR).ValueAsString(string.Empty);
        object dfltValue = null;

        if (clrType==typeof(string))
          dfltValue = strDfltValue;
        else if (strDfltValue.IsNotNullOrWhiteSpace())
          dfltValue = clrType==typeof(DateTime?)
                    ? ((long)strDfltValue.AsType(typeof(long), false)).FromMicrosecondsSinceUnixEpochStart()
                    : strDfltValue.AsType(clrType, false);

        if (isKey) keyCount++;

        List<string> vList = null;

        var values = node.Children.Where( c => c.IsSameName(CONFIG_VALUE_SECTION));
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

        var caze = CharCase.AsIs;

        var ca = node.AttrByName(CONFIG_CASE_ATTR).Value;
        if (ca.EqualsOrdIgnoreCase("upper")) caze = CharCase.Upper;
        else
        if (ca.EqualsOrdIgnoreCase("lower")) caze = CharCase.Lower;

        var atr = new FieldAttribute(
            targetName:    TargetedAttribute.ANY_TARGET,
            backendName:   fname,
            backendType:   type,
            storeFlag:     StoreFlag.LoadAndStore,
            key:           isKey,
            required:      required,
            dflt:          dfltValue,

            min:           minV,
            max:           maxV,

            charCase:      caze,

            visible:       node.AttrByName(CONFIG_VISIBLE_ATTR).ValueAsBool(true),
            maxLength:     node.AttrByName(CONFIG_LEN_ATTR).ValueAsInt(0),
            description:   fmtdesc.IsNullOrEmpty() ? fhint : "{0}\nFormat: {1}".Args(fhint,fmtdesc),
                            //Parsing.Utils.ParseFieldNameToDescription(ftitle, true),
            formatRegExp:  node.AttrByName(CONFIG_FORMAT_ATTR).Value,
            formatDescr:   fmtdesc,
            displayFormat: dispFmt,
            valueList:     vList==null ? null : string.Join(",", vList),//"CAR: Car Driver,SMK: Smoker, REL: Religious, CNT: Country music lover, GLD: Gold collector"
            metadata:      node.ToLaconicString());

        var def = new Schema.FieldDef(fname, clrType, new []{atr});
        defs.Add( def );
      }//for fields

      var result = new Schema(name, isReadonly, defs.ToArray());

      if (schDescr.IsNotNullOrWhiteSpace())
        result.ExtraData[CONFIG_DESCR_ATTR]               = schDescr;

      result.ExtraData[SCHEMA_KEY_COUNT]                  = keyCount;
      result.ExtraData[Schema.EXTRA_SUPPORTS_INSERT_ATTR] = isInsert;
      result.ExtraData[Schema.EXTRA_SUPPORTS_UPDATE_ATTR] = isUpdate;
      result.ExtraData[Schema.EXTRA_SUPPORTS_DELETE_ATTR] = isDelete;

      return result;
    }

    private static Type mapErlTypeToCLR(string erlType)
    {
      Tuple<Type, Func<IErlObject, object>> to;
      if (ERL_TO_CLR_TYPEMAP.TryGetValue(erlType, out to)) return to.Item1;


      throw new ErlDataAccessException(StringConsts.ERL_DS_SCHEMA_MAP_ERL_TYPE_ERROR.Args(erlType));
    }

    private static readonly    Dictionary<string,Tuple<Type,Func<IErlObject,object>>>
      ERL_TO_CLR_TYPEMAP = new Dictionary<string,Tuple<Type,Func<IErlObject,object>>>
    {
      {"atom",     Tuple.Create<Type, Func<IErlObject, object>>(typeof(string),     (erl) => erl.IsNull() ? null : erl.ValueAsString)},
      {"string",   Tuple.Create<Type, Func<IErlObject, object>>(typeof(string),     (erl) => erl.IsNull() ? null : erl.ValueAsString)},
      {"char",     Tuple.Create<Type, Func<IErlObject, object>>(typeof(string),     (erl) => erl.IsNull() ? null : erl.ValueAsChar.ToString())},
      {"int",      Tuple.Create<Type, Func<IErlObject, object>>(typeof(long?),      (erl) => erl.IsNull() ? null : (object)erl.ValueAsLong)},
      {"long",     Tuple.Create<Type, Func<IErlObject, object>>(typeof(long?),      (erl) => erl.IsNull() ? null : (object)erl.ValueAsLong)},
      {"double",   Tuple.Create<Type, Func<IErlObject, object>>(typeof(double?),    (erl) => erl.IsNull() ? null : (object)erl.ValueAsDouble)},
      {"date",     Tuple.Create<Type, Func<IErlObject, object>>(typeof(DateTime?),  (erl) => erl.IsNull() ? null : (object)erlDate2DateTime(erl))},
      {"datetime", Tuple.Create<Type, Func<IErlObject, object>>(typeof(DateTime?),  (erl) => erl.IsNull() ? null : (object)erl.ValueAsDateTime)},
      {"pid",      Tuple.Create<Type, Func<IErlObject, object>>(typeof(string),     (erl) => erl.IsNull() ? null : erl.ValueAsString)},
      {"bool",     Tuple.Create<Type, Func<IErlObject, object>>(typeof(bool?),      (erl) => erl.IsNull() ? null : (object)erl.ValueAsBool)},
      {"binary",   Tuple.Create<Type, Func<IErlObject, object>>(typeof(byte[]),     (erl) => erl.IsNull() ? null : erl.ValueAsByteArray)},
      {"binstr",   Tuple.Create<Type, Func<IErlObject, object>>(typeof(string),     (erl) => erl.IsNull() ? null : Encoding.UTF8.GetString(erl.ValueAsByteArray))},
      {"term",     Tuple.Create<Type, Func<IErlObject, object>>(typeof(IErlObject), (erl) => erl.IsNull() ? null : erl)},
      {"ip",       Tuple.Create<Type, Func<IErlObject, object>>(typeof(string),     (erl) => erl.IsNull() ? null : erlIP2String(erl))},
      {"ref",      Tuple.Create<Type, Func<IErlObject, object>>(typeof(string),     (erl) => erl.IsNull() ? null : erl.ValueAsString)}
    };


    private static readonly    Dictionary<string, Func<object,IErlObject>>
      CLR_TO_ERL_TYPEMAP = new Dictionary<string, Func<object,IErlObject>>
    {
      {"atom",     (clr) => new ErlAtom(clr.ToString())},
      {"string",   (clr) => new ErlString(clr.ToString())},
      {"char",     (clr) => new ErlByte(clr.ToString()[0])},
      {"int",      (clr) => new ErlLong(clr.AsLong(handling: ConvertErrorHandling.Throw))},
      {"long",     (clr) => new ErlLong(clr.AsLong(handling: ConvertErrorHandling.Throw))},
      {"double",   (clr) => new ErlDouble(clr.AsDouble(handling: ConvertErrorHandling.Throw))},
      {"date",     (clr) =>
                    {
                      var dt =  clr.AsDateTime();
                      dt = adjustDateToUTC(dt);
                      return new ErlTuple( dt.Year, dt.Month, dt.Day );
                    }},
      {"datetime", (clr) =>
                    {
                      var dt =  clr.AsDateTime();
                      dt = adjustDateToUTC(dt);
                      return new ErlLong(dt.ToMicrosecondsSinceUnixEpochStart() );
                    }},
      {"pid",      (clr) => ErlPid.Parse(clr.ToString())},
      {"bool",     (clr) => new ErlBoolean(clr.AsBool(handling: ConvertErrorHandling.Throw))},
      {"binary",   (clr) => new ErlBinary((byte[])clr)},
      {"binstr",   (clr) => new ErlBinary(Encoding.UTF8.GetBytes((string)clr))},
      {"term",     (clr) => (IErlObject)clr},
      {"ip",       (clr) => ipAddrToErlTuple((string)clr)},
      {"ref",      (clr) => ErlRef.Parse(clr.ToString())}
    };


    private static DateTime adjustDateToUTC(DateTime dt)
    {
      if (dt.Kind==DateTimeKind.Local)
        dt = NFX.App.LocalizedTimeToUniversalTime(dt);
      else if (dt.Kind==DateTimeKind.Unspecified)
        dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
      return dt;
    }

    private static string erlIP2String(IErlObject erl)
    {
      try
      {
        return string.Join(".", ((ErlTuple) erl).Value.Select(i => i.ValueAsInt.ToString()).Take(4));
      }
      catch (Exception e)
      {
        throw new ErlException(StringConsts.ERL_INVALID_VALUE_ERROR.Args(erl.ToString()), e);
      }
    }

    private static ErlTuple ipAddrToErlTuple(string addr)
    {
      var a = addr.AsString().Split(new[] { '.' }).Select(s => int.Parse(s)).ToArray();
      if (a.Length != 4)
        throw new ErlDataAccessException("Invalid IP address format: " + addr);
      return new ErlTuple(a[0], a[1], a[2], a[3]);
    }

    private static DateTime erlDate2DateTime(IErlObject erl)
    {
      try
      {
        var date = (ErlTuple)erl;
        var y = date[0].ValueAsInt;
        var m = date[1].ValueAsInt;
        var d = date[2].ValueAsInt;
        return new DateTime(y, m, d, 0, 0, 0, DateTimeKind.Utc);
      }
      catch (Exception e)
      {
        throw new ErlException(StringConsts.ERL_INVALID_VALUE_ERROR.Args(erl.ToString()), e);
      }
    }

    #endregion
  }

}
