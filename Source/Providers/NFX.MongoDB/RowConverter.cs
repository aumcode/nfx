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

using MongoDB.Bson;

namespace NFX.DataAccess.MongoDB
{
  /// <summary>
  /// Provides methods for Row to/from BSONDocument conversion
  /// </summary>
  public class RowConverter: IConfigurable
  {
      #region CONSTS
        public const decimal DECIMAL_LONG_MUL = 1000M;//multiplier for dec->long conversion
                                                                  //    TTT BBB MMM        .cents
        public const decimal MAX_DECIMAL = 9223372036854775.807M; // +9,223,372,036,854,775.807M;
        public const decimal MIN_DECIMAL = -9223372036854775.808M;// -9,223,372,036,854,775.808M;

        /// <summary>
        /// Maximum size of a byte[] field.
        /// Large filed (> 1Mbyte) should not be stored as fields in the database
        /// </summary>
        public const int MAX_BYTE_BUFFER_SIZE = 1/*mb*/ * 1024 * 1024;


         protected static Dictionary<Type, Func<object, BsonValue>> s_CLRtoBSON = new Dictionary<Type,Func<object, BsonValue>>
         {
            { typeof(string),   (v) => { var str = (string)v;  return str==string.Empty ? BsonString.Empty : new BsonString(str); } },
            { typeof(bool),     (v) => ((bool)v) ? BsonBoolean.True : BsonBoolean.False },
            { typeof(int),      (v) => new BsonInt32((int)v) },
            { typeof(uint),     (v) => new BsonInt64((uint)v) },
            { typeof(byte),     (v) => new BsonInt32((byte)v) },
            { typeof(sbyte),    (v) => new BsonInt32((sbyte)v) },
            { typeof(short),    (v) => new BsonInt32((short)v) },
            { typeof(ushort),   (v) => new BsonInt32((ushort)v) },
            { typeof(long),     (v) => new BsonInt64((long)v) },
            { typeof(ulong),    (v) => new BsonInt64((long)((ulong)v)) },
            { typeof(float),    (v) => new BsonDouble((float)v) },
            { typeof(double),   (v) => new BsonDouble((double)v) },
            { typeof(decimal),  (v) => Decimal_CLRtoBSON((decimal)v) },
            { typeof(Amount),   (v) => Amount_CLRtoBSON((Amount)v) },
            { typeof(GDID),     (v) => GDID_CLRtoBSON((GDID)v) },
            { typeof(DateTime), (v) => new BsonDateTime( (DateTime)v ) },
            { typeof(TimeSpan), (v) => new BsonInt64( ((TimeSpan)v).Ticks ) },
            { typeof(Guid),     (v) => new BsonString( ((Guid)v).ToString("N")) }, //no slashes in between
            { typeof(byte[]),   (v) => ByteBuffer_CLRtoBSON( (byte[])v ) },

            //nullable not needed here since they are not boxed(only actual value is boxed if it is not null)
          };

          protected static Dictionary<Type, Func<BsonValue, object>> s_BSONtoCLR = new Dictionary<Type,Func<BsonValue, object>>
          {
            { typeof(string),   (v) =>  v.ToString() },
            { typeof(bool),     (v) =>  v.ToString().AsBool(handling: ConvertErrorHandling.Throw) },
            { typeof(int),      (v) =>  v.ToString().AsInt(handling: ConvertErrorHandling.Throw) },
            { typeof(uint),     (v) =>  v.ToString().AsUInt(handling: ConvertErrorHandling.Throw) },
            { typeof(byte),     (v) =>  v.ToString().AsByte(handling: ConvertErrorHandling.Throw) },
            { typeof(sbyte),    (v) =>  v.ToString().AsSByte(handling: ConvertErrorHandling.Throw) },
            { typeof(short),    (v) =>  v.ToString().AsShort(handling: ConvertErrorHandling.Throw) },
            { typeof(ushort),   (v) =>  v.ToString().AsUShort(handling: ConvertErrorHandling.Throw) },
            { typeof(long),     (v) =>  v.ToString().AsLong(handling: ConvertErrorHandling.Throw) },
            { typeof(ulong),    (v) =>  v.ToString().AsULong(handling: ConvertErrorHandling.Throw) },
            { typeof(float),    (v) =>  v.ToString().AsFloat(handling: ConvertErrorHandling.Throw) },
            { typeof(double),   (v) =>  v.ToString().AsDouble(handling: ConvertErrorHandling.Throw) },
            { typeof(decimal),  (v) => Decimal_BSONtoCLR(v.ToString()) },
            { typeof(Amount),   (v) => {  if (v is BsonDocument) return Amount_BSONtoCLR((BsonDocument)v); return Amount.Parse(v.ToString()); }},
            { typeof(GDID),     (v) => GDID_BSONtoCLR( v ) },
            { typeof(DateTime), (v) => v.ToString().AsDateTime(/* it throws anyway handling: ConvertErrorHandling.Throw*/) },
            { typeof(TimeSpan), (v) => TimeSpan.FromTicks( v.ToString().AsLong(handling: ConvertErrorHandling.Throw)) },
            { typeof(Guid),     (v) => v.ToString().AsGUID(Guid.Empty, ConvertErrorHandling.Throw) },
            { typeof(byte[]),   (v) => ByteBuffer_BSONtoCLR(v) },
            //nullable are not needed, as BsonNull is handled already
          };

      #endregion


      #region .ctor
        public RowConverter()
        {
           m_CLRtoBSON = new Dictionary<Type,Func<object,BsonValue>>(s_CLRtoBSON);
           m_BSONtoCLR = new Dictionary<Type,Func<BsonValue,object>>(s_BSONtoCLR);
        }

      #endregion
      
      #region Fields
      
         protected Dictionary<Type, Func<object, BsonValue>> m_CLRtoBSON;
         protected Dictionary<Type, Func<BsonValue, object>> m_BSONtoCLR;
       
      #endregion

      
      #region Public Methods 

        public virtual void Configure(IConfigSectionNode node)
        {

        }
      

        /// <summary>
        /// Converts BSON document to JSON data map by directly mapping 
        ///  BSON types into corresponding CLR types. The sub-documents get mapped into JSONDataObjects,
        ///   and BSON arrays get mapped into CLR object[]
        /// </summary>
        public virtual JSONDataMap BSONDocumentToJSONMap(BsonDocument doc, Func<BsonDocument, BsonElement, bool> filter = null)
        {
          if (doc==null) return null;

          var result = new JSONDataMap(true);
          foreach(var elm in doc)
          {
             if (filter!=null)
              if (!filter(doc, elm)) continue;

             var clrValue = DirectConvertBSONValue(elm.Value, filter);
             result[elm.Name] = clrValue;
          }

          return result;
        }


                private static Dictionary<Schema, Dictionary<string, Schema.FieldDef>> s_TypedRowSchemaCache = new Dictionary<Schema,Dictionary<string, Schema.FieldDef>>();


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


        /// <summary>
        /// Converts BSON document into Row by filling the supplied row instance making necessary type transforms to
        ///  suit Row.Schema field definitions per target name. If the passed row supports IAmorphousData, then
        /// the fields either not found in row, or the fields that could not be type-converted to CLR type will be 
        /// stowed in amorphous data dictionary
        /// </summary>
        public virtual void BSONDocumentToRow(BsonDocument doc, Row row, string targetName, bool useAmorphousData = true, Func<BsonDocument, BsonElement, bool> filter = null)
        {                
          if (doc==null || row==null) throw new MongoDBDataAccessException(StringConsts.ARGUMENT_ERROR+"BSONDocumentToRow(doc|row=null)");
         
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

            var wasSet = TrySetFieldAsCLR(row, fld, elm.Value, targetName, filter);
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

        /// <summary>
        /// Converts row to BSON document suitable for storage in MONGO.DB.
        /// Pass target name (name of particular store/epoch/implementation) to get targeted field metadata.
        /// Note: the supplied row MAY NOT CONTAIN REFERENCE CYCLES - either direct or transitive
        /// </summary>
        public virtual BsonDocument RowToBSONDocument(Row row, string targetName, bool useAmorphousData = true)
        {
          if (row==null) return null;

          var amrow = row as IAmorphousData;
          if (amrow!=null && useAmorphousData && amrow.AmorphousDataEnabled)
          {
              amrow.BeforeSave(targetName);
          }

          var result = new BsonDocument();
          foreach(var field in row.Schema)
          {
             var attr = field[targetName];
             if (attr!=null && attr.StoreFlag!=StoreFlag.OnlyStore && attr.StoreFlag!=StoreFlag.LoadAndStore) continue;
             var bson = GetFieldAsBSON(row, field, targetName);
             result.Add( bson ); 
          }

          if (amrow!=null && useAmorphousData && amrow.AmorphousDataEnabled)
           foreach(var kvp in amrow.AmorphousData)
           {
             result.Add( GetAmorphousFieldAsBSON(kvp, targetName) );
           }

          return result;
        } 


                [ThreadStatic]
                private static HashSet<object> ts_References;
        /// <summary>
        /// Converts CLR value to BSON. The following values are supported:
        ///   primitives+scaled decimal, enums, TypedRows, GDID, Guid, Amount, IDictionary, IEnumerable (including arrays).
        /// If conversion could not be made then exception is thrown
        /// </summary>
        public BsonValue ConvertCLRtoBSON(object data, string targetName)
        {
          if (data==null) return BsonNull.Value;

          var tp = data.GetType();
          if (tp.IsValueType)
            return DoConvertCLRtoBSON(data, tp, targetName);

          if (ts_References==null) 
             ts_References = new HashSet<object>(NFX.ReferenceEqualityComparer<object>.Instance);
          
          if (ts_References.Contains(data))
             throw new MongoDBDataAccessException(StringConsts.CLR_BSON_CONVERSION_REFERENCE_CYCLE_ERROR.Args(tp.FullName));
          
          ts_References.Add( data );
          try
          {
            return DoConvertCLRtoBSON(data, tp, targetName);
          }
          finally
          {
            ts_References.Remove( data );
          }

        }

      #endregion


      #region Protected

        protected virtual BsonElement GetFieldAsBSON(Row row, Schema.FieldDef field, string targetName)
        {
          var name = field.GetBackendNameForTarget(targetName);
          var fvalue = row.GetFieldValue(field);
          var value = ConvertCLRtoBSON( fvalue , targetName);
          return new BsonElement(name, value);  
        }

        protected virtual bool TrySetFieldAsCLR(Row row, Schema.FieldDef field, BsonValue value, string targetName, Func<BsonDocument, BsonElement, bool> filter)
        {
          object clrValue;
          if (!TryConvertBSONtoCLR(field.NonNullableType, value, targetName, out clrValue, filter)) return false;
          row.SetFieldValue(field, clrValue);
          return true;
        }

        protected virtual BsonElement GetAmorphousFieldAsBSON(KeyValuePair<string, object> field, string targetName)
        {
          var value = ConvertCLRtoBSON( field.Value , targetName );
          return new BsonElement( field.Key, value );  
        }

        protected virtual bool SetAmorphousFieldAsCLR(IAmorphousData amorph, BsonElement bsonElement, string targetName, Func<BsonDocument, BsonElement, bool> filter)
        {
          object clrValue;
          if (!TryConvertBSONtoCLR(typeof(object), bsonElement.Value, targetName, out clrValue, filter)) return false;
          amorph.AmorphousData[bsonElement.Name] = clrValue;   
          return true;
        }

        

        /// <summary>
        /// override to perform the conversion. the data is never null here, and ref cycles a ruled out
        /// </summary>
        protected virtual BsonValue DoConvertCLRtoBSON(object data, Type dataType, string targetName)
        {
          //1 Primitive/direct types
          Func<object, BsonValue> func;
          if (m_CLRtoBSON.TryGetValue(dataType, out func)) return func(data);

          //2 Enums
          if (dataType.IsEnum)
           return new BsonString(data.ToString());

          //3 Complex Types
          if (data is Row)
          {
            return this.RowToBSONDocument((Row)data, targetName);
          }

          //IDictionary //must be before IEnumerable
          if (data is IDictionary)
          {
            var dict = (IDictionary)data;
            var result = new BsonDocument();
            foreach( var key in dict.Keys)
            {
             var name = key.ToString();
             var value = ConvertCLRtoBSON( dict[key] , targetName );
             result.Add( name, value );
            }
            return result;
          }

          //IEnumerable
          if (data is IEnumerable)
          {
            var list = (IEnumerable)data;
            var result = new BsonArray();
            foreach( var obj in list)
            {
             var value = ConvertCLRtoBSON( obj , targetName );
             result.Add( value );
            }
            return result;
          }


          throw new MongoDBDataAccessException(StringConsts.CLR_BSON_CONVERSION_TYPE_NOT_SUPPORTED_ERROR.Args(dataType.FullName));
        }

        /// <summary>
        /// Tries to convert the BSON value into target CLR type. Returns true if conversion was successfull
        /// </summary>
        protected virtual bool TryConvertBSONtoCLR(Type target, BsonValue value, string targetName, out object clrValue, Func<BsonDocument, BsonElement, bool> filter)
        {
          if (value==null || value is BsonNull) 
          {
            clrValue = null;
            return true;
          }

          if (target == typeof(object))
          {
            //just unwrap Bson:CLR = 1:1, without type conversion
            clrValue = DirectConvertBSONValue( value, filter );
            return true;
          }

          clrValue = null;

          if (target.IsSubclassOf(typeof(TypedRow)))
          {
            var doc = value as BsonDocument;
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
            var arr = value as BsonArray;
            if (arr==null) return false;//not array
            var telm = target.GetElementType();
            var clrArray = Array.CreateInstance(telm, arr.Count);
            for(var i=0; i<arr.Count; i++)
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
            var arr = value as BsonArray;
            if (arr==null) return false;//not array
            var gargs = target.GetGenericArguments();
            var telm = gargs[0];
            var clrList = Activator.CreateInstance(target) as System.Collections.IList;
            for(var i=0; i<arr.Count; i++)
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
            var doc = value as BsonDocument;
            if (doc==null) return false;//not document
            clrValue = BSONDocumentToJSONMap(doc, filter);
            return true;
          }
                       


          //Primitive type-targeted value
          Func<BsonValue, object> func;
          if (m_BSONtoCLR.TryGetValue(target, out func)) 
          {
            try
            {
              clrValue = func(value);
            }
            catch
            {
              return false;//functor could not convert
            }
            return true;
          }

          return false;//could not convert
        }


        /// <summary>
        /// Converts BSON to CLR value 1:1, without type change
        /// </summary>
        protected virtual object DirectConvertBSONValue(BsonValue value, Func<BsonDocument, BsonElement, bool> filter = null)
        {
          if (value==null || value is BsonNull) return null;
          
          if (value is BsonDocument) return BSONDocumentToJSONMap((BsonDocument)value, filter);
         
          if (value is BsonArray) 
          {
            var arr = (BsonArray)value;
            var lst = new List<object>();
            foreach(var elm in arr)
             lst.Add( DirectConvertBSONValue(elm, filter) );
            return lst.ToArray();
          }

          if (value is BsonString)   return ((BsonString)value).Value;
          if (value is BsonInt32)    return ((BsonInt32)value).Value;
          if (value is BsonInt64)    return ((BsonInt64)value).Value;
          if (value is BsonDouble)   return ((BsonDouble)value).Value;
          if (value is BsonBoolean)   return ((BsonBoolean)value).Value;
          if (value is BsonDateTime)   return ((BsonDateTime)value).ToUniversalTime();
          if (value is BsonBinaryData)   return ((BsonBinaryData)value).Bytes;

          return value.ToString(); 
        }

      
        protected static BsonValue Decimal_CLRtoBSON(decimal v)
        {
          if (v<MIN_DECIMAL || v>MAX_DECIMAL) 
          throw new MongoDBDataAccessException(StringConsts.DECIMAL_OUT_OF_RANGE_ERROR.Args(v, MIN_DECIMAL, MAX_DECIMAL));

          var lv = (long)decimal.Truncate(v * DECIMAL_LONG_MUL);
          return new BsonInt64(lv); 
        }

        protected static decimal Decimal_BSONtoCLR(string bson)
        {
          var unscaled = bson.AsDecimal(handling: ConvertErrorHandling.Throw);
          return unscaled / DECIMAL_LONG_MUL; 
        }

        protected static BsonValue Amount_CLRtoBSON(Amount amount)
        {
          return new BsonDocument
          {
            {"c", amount.CurrencyISO},
            {"v", Decimal_CLRtoBSON(amount.Value)}
          }; 
        }

        protected static BsonValue GDID_CLRtoBSON(GDID gdid)
        {
          //As tested on Feb 27, 2015
          //BinData works faster than string 8% and stores 40%-60% less data in index and data segment
          //Also, SEQUENTIAL keys (big endian) yield 30% smaller indexes (vs non-sequential)
          //ObjectId() is very similar if not identical to BinData(UserDefined)
          return new BsonBinaryData(gdid.Bytes, BsonBinarySubType.UserDefined);
        }

        protected static Amount Amount_BSONtoCLR(BsonDocument bson)
        {
          var iso = bson.GetValue("c", string.Empty).ToString();
          var value = Decimal_BSONtoCLR(bson.GetValue("v", 0L).ToString());
          return new Amount(iso, value);
        }

        protected static GDID GDID_BSONtoCLR(BsonValue bson)
        {
          try
          {
              var bv = bson as BsonBinaryData;
              if (bv==null) 
                throw new MongoDBDataAccessException("not BsonBinaryData in doc field");
              var buf = bv.Bytes;
              return new GDID(buf);
          }
          catch(Exception e)
          {
            throw new MongoDBDataAccessException(StringConsts.GDID_BUFFER_ERROR.Args(e.ToMessageWithType()), e);
          }
        }

        protected static BsonBinaryData ByteBuffer_CLRtoBSON(byte[] buf)
        {
          if (buf.Length > MAX_BYTE_BUFFER_SIZE)
          throw new MongoDBDataAccessException(StringConsts.BUFFER_LONGER_THAN_ALLOWED_ERROR.Args(buf.Length, MAX_BYTE_BUFFER_SIZE));
           
          return new BsonBinaryData( (byte[])buf );
        }

        protected static byte[] ByteBuffer_BSONtoCLR(BsonValue bson)
        {
          if (bson==null || bson is BsonNull) return null;
          return (byte[])bson;//BsonBinaryData explicit cast
        }


      #endregion

  }

}
