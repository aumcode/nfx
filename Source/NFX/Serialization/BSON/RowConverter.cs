/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.Environment;
using NFX.Financial;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;
using NFX.Serialization.JSON;
using NFX.DataAccess;

namespace NFX.Serialization.BSON
{
  /// <summary>
  /// Provides methods for Row to/from BSONDocument conversion
  /// </summary>
  public class RowConverter: IConfigurable
  {
    #region CONST

        public const decimal DECIMAL_LONG_MUL = 1000M;//multiplier for dec->long conversion

        public const decimal MAX_DECIMAL = 9223372036854775.807M; // +9,223,372,036,854,775.807M;
        public const decimal MIN_DECIMAL = -9223372036854775.808M;// -9,223,372,036,854,775.808M;

        /// <summary>
        /// Maximum size of a byte[] field.
        /// Large filed (> 1Mbyte) should not be stored as fields in the database
        /// </summary>
        public const int MAX_BYTE_BUFFER_SIZE = 1/*mb*/ * 1024 * 1024;

        protected static Dictionary<Type, Func<string, object, BSONElement>> s_CLRtoBSON = new Dictionary<Type,Func<string, object, BSONElement>>
        {
          { typeof(string),   (n, v) => n != null ? new BSONStringElement(n, (string)v) : new BSONStringElement((string)v) },
          { typeof(bool),     (n, v) => n != null ? new BSONBooleanElement(n, (bool)v) : new BSONBooleanElement((bool)v) },
          { typeof(int),      (n, v) => n != null ? new BSONInt32Element(n, (int)v) : new BSONInt32Element((int)v) },
          { typeof(uint),     (n, v) => n != null ? new BSONInt64Element(n, (uint)v) : new BSONInt64Element((uint)v) },
          { typeof(byte),     (n, v) => n != null ? new BSONInt32Element(n, (byte)v) : new BSONInt32Element((byte)v) },
          { typeof(sbyte),    (n, v) => n != null ? new BSONInt32Element(n, (sbyte)v) : new BSONInt32Element((sbyte)v) },
          { typeof(short),    (n, v) => n != null ? new BSONInt32Element(n, (short)v) : new BSONInt32Element((short)v) },
          { typeof(ushort),   (n, v) => n != null ? new BSONInt32Element(n, (ushort)v) : new BSONInt32Element((ushort)v) },
          { typeof(long),     (n, v) => n != null ? new BSONInt64Element(n, (long)v) : new BSONInt64Element((long)v) },
          { typeof(ulong),    (n, v) => n != null ? new BSONInt64Element(n, (long)((ulong)v)) : new BSONInt64Element((long)((ulong)v)) },
          { typeof(float),    (n, v) => n != null ? new BSONDoubleElement(n, (float)v) : new BSONDoubleElement((float)v) },
          { typeof(double),   (n, v) => n != null ? new BSONDoubleElement(n, (double)v) : new BSONDoubleElement((double)v) },
          { typeof(decimal),  (n, v) => Decimal_CLRtoBSON(n, (decimal)v) },
          { typeof(Amount),   (n, v) => Amount_CLRtoBSON(n, (Amount)v) },
          { typeof(GDID),     (n, v) => GDID_CLRtoBSON(n, (GDID)v) },
          { typeof(DateTime), (n, v) => n != null ? new BSONDateTimeElement(n, (DateTime)v) : new BSONDateTimeElement((DateTime)v) },
          { typeof(TimeSpan), (n, v) => n != null ? new BSONInt64Element(n, ((TimeSpan)v).Ticks) : new BSONInt64Element(((TimeSpan)v).Ticks) },
          { typeof(Guid),     (n, v) => n != null ? new BSONStringElement(n, ((Guid)v).ToString("N")) : new BSONStringElement(((Guid)v).ToString("N")) },
          { typeof(byte[]),   (n, v) => ByteBuffer_CLRtoBSON(n, (byte[])v ) },

          //nullable not needed here since they are not boxed(only actual value is boxed if it is not null)
        };

        protected static Dictionary<Type, Func<BSONElement, object>> s_BSONtoCLR = new Dictionary<Type,Func<BSONElement, object>>
        {
          { typeof(string),   (v) => ((BSONStringElement)v).Value },
          { typeof(bool),     (v) => ((BSONBooleanElement)v).Value },
          { typeof(int),      (v) => ((BSONInt32Element)v).Value },
          { typeof(uint),     (v) => (uint)((BSONInt64Element)v).Value },
          { typeof(byte),     (v) => (byte)((BSONInt32Element)v).Value },
          { typeof(sbyte),    (v) => (sbyte)((BSONInt32Element)v).Value },
          { typeof(short),    (v) => (short)((BSONInt32Element)v).Value },
          { typeof(ushort),   (v) => ((BSONInt32Element)v).Value },
          { typeof(long),     (v) => ((BSONInt64Element)v).Value },
          { typeof(ulong),    (v) => (ulong)((BSONInt64Element)v).Value },
          { typeof(float),    (v) => (float)((BSONDoubleElement)v).Value },
          { typeof(double),   (v) => ((BSONDoubleElement)v).Value },
          { typeof(decimal),  (v) => Decimal_BSONtoCLR((BSONInt64Element)v) },
          { typeof(Amount),   (v) => {  if (v is BSONDocumentElement) return Amount_BSONtoCLR((BSONDocumentElement)v); return Amount.Parse(Convert.ToString(v.ObjectValue)); }},
          { typeof(GDID),     (v) => GDID_BSONtoCLR((BSONBinaryElement)v) },
          { typeof(DateTime), (v) => ((BSONDateTimeElement)v).Value },
          { typeof(TimeSpan), (v) => TimeSpan.FromTicks( ((BSONInt64Element)v).Value) },
          { typeof(Guid),     (v) => ((BSONStringElement)v).Value.AsGUID(Guid.Empty, ConvertErrorHandling.Throw) },
          { typeof(byte[]),   (v) => ((BSONBinaryElement)v).Value.Data },
          ////nullable are not needed, as BsonNull is handled already
        };

    #endregion

    #region .ctor

        public RowConverter()
        {
          m_CLRtoBSON = new Dictionary<Type,Func<string, object,BSONElement>>(s_CLRtoBSON);
          m_BSONtoCLR = new Dictionary<Type,Func<BSONElement,object>>(s_BSONtoCLR);
        }

      #endregion

    #region Fields

        protected Dictionary<Type, Func<string, object, BSONElement>> m_CLRtoBSON;
        protected Dictionary<Type, Func<BSONElement, object>> m_BSONtoCLR;
        private static Dictionary<Schema, Dictionary<string, Schema.FieldDef>> s_TypedRowSchemaCache = new Dictionary<Schema,Dictionary<string, Schema.FieldDef>>();
        [ThreadStatic] private static HashSet<object> ts_References;

    #endregion




    public virtual void Configure(IConfigSectionNode node) {}


    /// <summary>
    /// Makes CRUD Schema out of BSON document. The types of all fields are object as documents do not have
    ///  a predictable type of every field (they are dynamic and can change form doc to doc)
    /// </summary>
    public virtual Schema InferSchemaFromBSONDocument(BSONDocument doc, string schemaName = null)
    {
      var defs = new List<Schema.FieldDef>();

      foreach(var elm in doc)
      {
          var clrv = elm.ObjectValue;
          var tv = typeof(object);
          var def = new Schema.FieldDef(elm.Name, tv, new FieldAttribute[]{ new FieldAttribute(backendName: elm.Name) });
          defs.Add( def );
      }

      return  new Schema(schemaName.IsNotNullOrWhiteSpace() ? schemaName : Guid.NewGuid().ToString(),
                         true,
                         defs.ToArray());
    }



    #region BSONDocumentToRow

    /// <summary>
    /// Converts BSON document into Row by filling the supplied row instance making necessary type transforms to
    ///  suit Row.Schema field definitions per target name. If the passed row supports IAmorphousData, then
    /// the fields either not found in row, or the fields that could not be type-converted to CLR type will be
    /// stowed in amorphous data dictionary
    /// </summary>
    public virtual void BSONDocumentToRow(BSONDocument doc, Row row, string targetName, bool useAmorphousData = true, Func<BSONDocument, BSONElement, bool> filter = null)
    {
      if (doc==null || row==null) throw new BSONException(StringConsts.ARGUMENT_ERROR+"BSONDocumentToRow(doc|row=null)");

      var amrow = row as IAmorphousData;

      foreach(var elm in doc)
      {
        if (filter!=null)
          if (!filter(doc, elm)) continue;

        // 2015.03.01 Introduced caching
        var fld = MapBSONFieldNameToSchemaFieldDef(row.Schema, targetName, elm.Name);


        if (fld==null)
        {
            if (amrow!=null && useAmorphousData && amrow.AmorphousDataEnabled)
              SetAmorphousFieldAsCLR(amrow, elm, targetName, filter);
            continue;
        }

        var wasSet = TrySetFieldAsCLR(row, fld, elm, targetName, filter);
        if (!wasSet)//again dump it in amorphous
        {
          if (amrow!=null && useAmorphousData && amrow.AmorphousDataEnabled)
              SetAmorphousFieldAsCLR(amrow, elm, targetName, filter);
        }
      }


      if (amrow!=null && useAmorphousData && amrow.AmorphousDataEnabled)
      {
          amrow.AfterLoad(targetName);
      }
    }

    //public static Amount Amount_BSONtoCLR(BSONDocument bson)
    //{
    //  var iso = bson.GetValue("c", string.Empty).ToString();
    //  var value = Decimal_BSONtoCLR(bson.GetValue("v", 0L).ToString());
    //  return new Amount(iso, value);
    //}


        protected virtual bool TrySetFieldAsCLR(Row row, Schema.FieldDef field, BSONElement value, string targetName, Func<BSONDocument, BSONElement, bool> filter)
        {
          object clrValue;
          if (!TryConvertBSONtoCLR(field.NonNullableType, value, targetName, out clrValue, filter)) return false;
          row.SetFieldValue(field, clrValue);
          return true;
        }


        /// <summary>
        /// Maps bsonFieldName to targeted fieldDef from specified schema using cache for TypedRows
        /// </summary>
        protected static Schema.FieldDef MapBSONFieldNameToSchemaFieldDef(Schema schema, string targetName, string bsonFieldName)
        {
          if (schema.TypedRowType==null)
            return mapBSONFieldNameToSchemaFieldDef(schema, targetName, bsonFieldName);

          if (targetName==null) targetName = FieldAttribute.ANY_TARGET;

          Dictionary<string, Schema.FieldDef> byName;
          if (!s_TypedRowSchemaCache.TryGetValue(schema, out byName)) byName = null;//safeguard

          Schema.FieldDef result;
          var key = targetName+"::"+bsonFieldName;
          if (byName==null || !byName.TryGetValue(key, out result))
          {
            result = mapBSONFieldNameToSchemaFieldDef(schema, targetName, bsonFieldName);
            var dict1 = new Dictionary<Schema,Dictionary<string,Schema.FieldDef>>(s_TypedRowSchemaCache);
            var dict2 = byName==null ? new Dictionary<string, Schema.FieldDef>() : new Dictionary<string, Schema.FieldDef>(byName);
            dict2[key] = result;
            dict1[schema] = dict2;
            s_TypedRowSchemaCache = dict1;//atomic
          }
          return result;
        }

                private static Schema.FieldDef mapBSONFieldNameToSchemaFieldDef(Schema schema, string targetName, string bsonFieldName)
                {
                    return schema.FirstOrDefault( fd =>
                                                 {
                                                   var match = fd.GetBackendNameForTarget(targetName).EqualsOrdIgnoreCase( bsonFieldName );
                                                   if (!match) return false;
                                                   var attr = fd[targetName];
                                                   if (attr==null) return true;
                                                   return attr.StoreFlag==StoreFlag.LoadAndStore || attr.StoreFlag==StoreFlag.OnlyLoad;
                                                 });
                }

                protected virtual bool SetAmorphousFieldAsCLR(IAmorphousData amorph, BSONElement bsonElement, string targetName, Func<BSONDocument, BSONElement, bool> filter)
                {
                  object clrValue;
                  if (!TryConvertBSONtoCLR(typeof(object), bsonElement, targetName, out clrValue, filter)) return false;
                  amorph.AmorphousData[bsonElement.Name] = clrValue;
                  return true;
                }


        /// <summary>
        /// Tries to convert the BSON value into target CLR type. Returns true if conversion was successfull
        /// </summary>
        protected virtual bool TryConvertBSONtoCLR(Type target, BSONElement element, string targetName, out object clrValue, Func<BSONDocument, BSONElement, bool> filter)
        {
          if (element==null || element is BSONNullElement)
          {
            clrValue = null;
            return true;
          }

          if (target == typeof(object))
          {
            //just unwrap Bson:CLR = 1:1, without type conversion
            clrValue = DirectConvertBSONValue( element, filter );
            return true;
          }

          clrValue = null;

          if (target.IsSubclassOf(typeof(TypedRow)))
          {
            var bsonDocumentElement = element as BSONDocumentElement;
            var doc = bsonDocumentElement != null ? bsonDocumentElement.Value : null;
            if (doc==null) return false;//not document
            var tr = (TypedRow)Activator.CreateInstance(target);
            BSONDocumentToRow(doc, tr, targetName, filter: filter);
            clrValue = tr;
            return true;
          }

          //ARRAY
          if (target.IsArray &&
              target.GetArrayRank()==1 &&
              target!=typeof(byte[]))//exclude byte[] as it is treated with m_BSONtoCLR
          {
            var bsonArrayElement = element as BSONArrayElement;
            var arr = bsonArrayElement != null ? bsonArrayElement.Value : null;
            if (arr==null) return false;//not array
            var telm = target.GetElementType();
            var clrArray = Array.CreateInstance(telm, arr.Length);
            for(var i=0; i<arr.Length; i++)
            {
              object clrElement;
              if (!TryConvertBSONtoCLR(telm, arr[i], targetName, out clrElement, filter))
              {
                return false;//could not convert some element of array
              }
              clrArray.SetValue(clrElement, i);
            }

            clrValue = clrArray;
            return true;
          }

          //LIST<T>
          if (target.IsGenericType && target.GetGenericTypeDefinition() == typeof(List<>))
          {
            var bsonArrayElement = element as BSONArrayElement;
            var arr = bsonArrayElement != null ? bsonArrayElement.Value : null;
            if (arr==null) return false;//not array
            var gargs = target.GetGenericArguments();
            var telm = gargs[0];
            var clrList = Activator.CreateInstance(target) as System.Collections.IList;
            for(var i=0; i<arr.Length; i++)
            {
              object clrElement;
              if (!TryConvertBSONtoCLR(telm, arr[i], targetName, out clrElement, filter))
              {
                return false;//could not convert some element of array into element of List<t>
              }
              clrList.Add( clrElement );
            }

            clrValue = clrList;
            return true;
          }

          //JSONDataMap
          if (target==typeof(JSONDataMap))
          {
            var bsonDocumentElement = element as BSONDocumentElement;
            var doc = bsonDocumentElement != null ? bsonDocumentElement.Value : null;
            clrValue = BSONDocumentToJSONMap(doc, filter);
            return true;
          }

          if (target.IsEnum)
          {
            try
            {
              clrValue = Enum.Parse(target, ((BSONStringElement)element).Value, true);
              return true;
            }
            catch
            {
              return false;
            }
          }

          //Primitive type-targeted value
          Func<BSONElement, object> func;
          if (m_BSONtoCLR.TryGetValue(target, out func))
          {
            try
            {
              clrValue = func(element);
            }
            catch(Exception error)
            {
              Debug.Fail("Error in BSONRowConverter.TryConvertBSONtoCLR(): " + error.ToMessageWithType());
              return false;//functor could not convert
            }
            return true;
          }

          return false;//could not convert
        }

        /// <summary>
        /// Converts BSON to CLR value 1:1, without type change
        /// </summary>
        protected virtual object DirectConvertBSONValue(BSONElement element, Func<BSONDocument, BSONElement, bool> filter = null)
        {
          if (element==null || element is BSONNullElement) return null;

          if (element.ElementType == BSONElementType.Document) return BSONDocumentToJSONMap(((BSONDocumentElement)element).Value, filter);

          if (element.ElementType == BSONElementType.Array)
          {
            var bsonArr = (BSONArrayElement)element;
            var lst = new List<object>();
            foreach(var elm in bsonArr.Value)
              lst.Add( DirectConvertBSONValue(elm, filter) );
            return lst.ToArray();
          }

          switch (element.ElementType)
          {
            case BSONElementType.ObjectID: return ((BSONObjectIDElement)element).Value.AsGDID;
            case BSONElementType.Binary: return ((BSONBinaryElement)element).Value.Data;
          }

          return element.ObjectValue;
        }

        /// <summary>
        /// Converts BSON document to JSON data map by directly mapping
        ///  BSON types into corresponding CLR types. The sub-documents get mapped into JSONDataObjects,
        ///   and BSON arrays get mapped into CLR object[]
        /// </summary>
        public virtual JSONDataMap BSONDocumentToJSONMap(BSONDocument doc, Func<BSONDocument, BSONElement, bool> filter = null)
        {
          if (doc==null) return null;

          var result = new JSONDataMap(true);
          foreach(var elm in doc)
          {
             if (filter!=null)
              if (!filter(doc, elm)) continue;

             var clrValue = DirectConvertBSONValue(elm, filter);
             result[elm.Name] = clrValue;
          }

          return result;
        }


    #endregion


    #region RowToBSONDocument

      /// <summary>
      /// Converts row to BSON document suitable for storage in MONGO.DB.
      /// Pass target name (name of particular store/epoch/implementation) to get targeted field metadata.
      /// Note: the supplied row MAY NOT CONTAIN REFERENCE CYCLES - either direct or transitive
      /// </summary>
      public virtual BSONDocument RowToBSONDocument(Row row, string targetName, bool useAmorphousData = true, FieldFilterFunc filter = null)
      {
        var el = RowToBSONDocumentElement(row, targetName, useAmorphousData, filter: filter);
        return el.Value;
      }

      /// <summary>
      /// Converts row to BSON document suitable for storage in MONGO.DB.
      /// Pass target name (name of particular store/epoch/implementation) to get targeted field metadata.
      /// Note: the supplied row MAY NOT CONTAIN REFERENCE CYCLES - either direct or transitive
      /// </summary>
      public virtual BSONDocumentElement RowToBSONDocumentElement(Row row, string targetName, bool useAmorphousData = true, string name= null, FieldFilterFunc filter = null)
      {
        if (row==null) return null;

        var amrow = row as IAmorphousData;
        if (amrow!=null && useAmorphousData && amrow.AmorphousDataEnabled)
        {
            amrow.BeforeSave(targetName);
        }

        var result = new BSONDocument();
        foreach(var field in row.Schema)
        {
            var attr = field[targetName];
            if (attr!=null && attr.StoreFlag!=StoreFlag.OnlyStore && attr.StoreFlag!=StoreFlag.LoadAndStore) continue;

            if (filter!=null)//20160210 Dkh+SPol
            {
              if (!filter(row, null, field)) continue;
            }

            var el = GetFieldAsBSON(row, field, targetName);
            result.Set( el );
        }

        if (amrow!=null && useAmorphousData && amrow.AmorphousDataEnabled)
          foreach(var kvp in amrow.AmorphousData)
          {
            result.Set( GetAmorphousFieldAsBSON(kvp, targetName) );
          }

        return name != null ? new BSONDocumentElement(name, result) : new BSONDocumentElement(result);
      }

      /// <summary>
      /// Converts CLR value to BSON. The following values are supported:
      ///   primitives+scaled decimal, enums, TypedRows, GDID, Guid, Amount, IDictionary, IEnumerable (including arrays).
      /// If conversion could not be made then exception is thrown
      /// </summary>
      public BSONElement ConvertCLRtoBSON(string fieldName, object data, string targetName)
      {
        if (data==null) return fieldName != null ? new BSONNullElement(fieldName) : new BSONNullElement();

        var tp = data.GetType();
        if (tp.IsValueType)
          return DoConvertCLRtoBSON(fieldName, data, tp, targetName);

        if (ts_References==null)
            ts_References = new HashSet<object>(NFX.ReferenceEqualityComparer<object>.Instance);

        if (ts_References.Contains(data))
            throw new BSONException(StringConsts.CLR_BSON_CONVERSION_REFERENCE_CYCLE_ERROR.Args(tp.FullName));

        ts_References.Add( data );
        try
        {
          return DoConvertCLRtoBSON(fieldName, data, tp, targetName);
        }
        finally
        {
          ts_References.Remove( data );
        }

      }

          protected virtual BSONElement GetAmorphousFieldAsBSON(KeyValuePair<string, object> field, string targetName)
          {
            var el = ConvertCLRtoBSON( field.Key, field.Value , targetName );
            return el;
          }

          protected virtual BSONElement GetFieldAsBSON(Row row, Schema.FieldDef field, string targetName)
          {
            var name = field.GetBackendNameForTarget(targetName);
            var fvalue = row.GetFieldValue(field);
            var el = ConvertCLRtoBSON(name, fvalue, targetName);
            return el;
          }

          /// <summary>
          /// override to perform the conversion. the data is never null here, and ref cycles a ruled out
          /// </summary>
          protected virtual BSONElement DoConvertCLRtoBSON(string name, object data, Type dataType, string targetName)
          {
            //1 Primitive/direct types
            Func<string, object, BSONElement> func;
            if (m_CLRtoBSON.TryGetValue(dataType, out func)) return func(name, data);

            //2 Enums
            if (dataType.IsEnum)
              return name != null ? new BSONStringElement(name, data.ToString()) : new BSONStringElement(data.ToString());

            //3 Complex Types
            if (data is Row)
              return this.RowToBSONDocumentElement((Row)data, targetName, name: name);

            //IDictionary //must be before IEnumerable
            if (data is IDictionary)
            {
              var dict = (IDictionary)data;
              var result = new BSONDocument();
              foreach( var key in dict.Keys)
              {
                var fldName = key.ToString();
                var el = ConvertCLRtoBSON(fldName, dict[key], targetName);
                result.Set(el);
              }
              return name != null ? new BSONDocumentElement(name, result) : new BSONDocumentElement(result);
            }

            //IEnumerable
            if (data is IEnumerable)
            {
              var list = (IEnumerable)data;
              List<BSONElement> elements = new List<BSONElement>();
              foreach( var obj in list)
              {
                var el = ConvertCLRtoBSON(null, obj, targetName);
                elements.Add(el);
              }
              var result = name != null ? new BSONArrayElement(name, elements.ToArray()) : new BSONArrayElement(elements.ToArray());
              return result;
            }


            throw new BSONException(StringConsts.CLR_BSON_CONVERSION_TYPE_NOT_SUPPORTED_ERROR.Args(dataType.FullName));
          }

    #endregion

    #region Static converters

      #region CLRtoBSON

        public static BSONInt64Element Decimal_CLRtoBSON(string name, decimal v)
        {
          if (v<MIN_DECIMAL || v>MAX_DECIMAL)
            throw new BSONException(StringConsts.DECIMAL_OUT_OF_RANGE_ERROR.Args(v, MIN_DECIMAL, MAX_DECIMAL));

          var lv = (long)decimal.Truncate(v * DECIMAL_LONG_MUL);
          return name != null ? new BSONInt64Element(name, lv) : new BSONInt64Element(lv);
        }

        public static BSONDocumentElement Amount_CLRtoBSON(string name, Amount amount)
        {
          var curEl = new BSONStringElement("c", amount.CurrencyISO);
          var valEl = Decimal_CLRtoBSON("v", amount.Value);
          var doc = new BSONDocument();
          doc.Set(curEl).Set(valEl);

          return name != null ? new BSONDocumentElement(name, doc) : new BSONDocumentElement(doc);
        }

        public static BSONElement GDID_CLRtoBSON(string name, GDID gdid)
        {
          if (gdid.IsZero) return new BSONNullElement(name);
          //As tested on Feb 27, 2015
          //BinData works faster than string 8% and stores 40%-60% less data in index and data segment
          //Also, SEQUENTIAL keys (big endian) yield 30% smaller indexes (vs non-sequential)
          //ObjectId() is very similar if not identical to BinData(UserDefined)
          var bin = new BSONBinary(BSONBinaryType.UserDefined, gdid.Bytes);
          return name != null ?  new BSONBinaryElement(name, bin) : new BSONBinaryElement( bin);
        }

        public static BSONBinaryElement ByteBuffer_CLRtoBSON(string name, byte[] buf)
        {
          if (buf.Length > MAX_BYTE_BUFFER_SIZE)
          throw new BSONException(StringConsts.BUFFER_LONGER_THAN_ALLOWED_ERROR.Args(buf.Length, MAX_BYTE_BUFFER_SIZE));

          var bsonBin = new BSONBinary(BSONBinaryType.GenericBinary, buf);
          return name != null ? new BSONBinaryElement(name, bsonBin) : new BSONBinaryElement(bsonBin);
        }

      #endregion

      #region BSONtoCLR

        public static decimal Decimal_BSONtoCLR(BSONInt64Element el)
        {
          return el.Value / DECIMAL_LONG_MUL;
        }

        public static Amount Amount_BSONtoCLR(BSONDocumentElement el)
        {
          var doc = el.Value;
          var iso = ((BSONStringElement)doc["c"]).Value;
          var val = Decimal_BSONtoCLR((BSONInt64Element)doc["v"]);
          return new Amount(iso, val);
        }

        public static GDID GDID_BSONtoCLR(BSONBinaryElement el)
        {
          try
          {
            var buf = el.Value.Data;
            return new GDID(buf);
          }
          catch(Exception e)
          {
            throw new BSONException(StringConsts.GDID_BUFFER_ERROR.Args(e.ToMessageWithType()), e);
          }
        }

      #endregion

    #endregion

  }

}
