using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using NFX.Serialization.JSON;

namespace NFX.Serialization.BSON
{
  /// <summary>
  /// Very base class for BSON elements
  /// </summary>
  public abstract class BSONElement : INamed, IConvertible, IJSONWritable
  {

    private static readonly Dictionary<BSONElementType, Func<Stream, BSONElement>> s_ELEMENTSTREAMCTORS = new Dictionary<BSONElementType, Func<Stream, BSONElement>>
    {
      { BSONElementType.Document,  (str) => new BSONDocumentElement(str) },
      { BSONElementType.Int32,     (str) => new BSONInt32Element(str) },
      { BSONElementType.Double,    (str) => new BSONDoubleElement(str) },
      { BSONElementType.String,    (str) => new BSONStringElement(str) },
      { BSONElementType.Array,     (str) => new BSONArrayElement(str) },
      { BSONElementType.Binary,    (str) => new BSONBinaryElement(str) },
      { BSONElementType.ObjectID,  (str) => new BSONObjectIDElement(str) },
      { BSONElementType.Boolean,   (str) => new BSONBooleanElement(str) },
      { BSONElementType.DateTime,  (str) => new BSONDateTimeElement(str) },
      { BSONElementType.Null,      (str) => new BSONNullElement(str) },
      { BSONElementType.RegularExpression,   (str) => new BSONRegularExpressionElement(str) },
      { BSONElementType.JavaScript,          (str) => new BSONJavaScriptElement(str) },
      { BSONElementType.JavaScriptWithScope, (str) => new BSONJavaScriptWithScopeElement(str) },
      { BSONElementType.TimeStamp, (str) => new BSONTimestampElement(str) },
      { BSONElementType.Int64,     (str) => new BSONInt64Element(str) },
      { BSONElementType.MinKey,    (str) => new BSONMinKeyElement(str) },
      { BSONElementType.MaxKey,    (str) => new BSONMaxKeyElement(str) }
    };

    private static readonly Dictionary<BSONElementType, Func<string, object, BSONElement>> s_ELEMENTVALUECTORS = new Dictionary<BSONElementType, Func<string, object, BSONElement>>
    {
      { BSONElementType.Document,            (n, v) => n!=null ? (BSONElement)new BSONDocumentElement(n, (BSONDocument)v)                    :  (BSONElement)new BSONDocumentElement((BSONDocument)v)                      },
      { BSONElementType.Int32,               (n, v) => n!=null ? (BSONElement)new BSONInt32Element(n, v.AsInt())                             :  (BSONElement)new BSONInt32Element(v.AsInt())                               },
      { BSONElementType.Double,              (n, v) => n!=null ? (BSONElement)new BSONDoubleElement(n, v.AsDouble())                         :  (BSONElement)new BSONDoubleElement(v.AsDouble())                           },
      { BSONElementType.String,              (n, v) => n!=null ? (BSONElement)new BSONStringElement(n, v.AsString())                         :  (BSONElement)new BSONStringElement(v.AsString())                           },
      { BSONElementType.Array,               (n, v) => n!=null ? (BSONElement)new BSONArrayElement(n, (BSONElement[])v)                      :  (BSONElement)new BSONArrayElement((BSONElement[])v)                        },
      { BSONElementType.Binary,              (n, v) => n!=null ? (BSONElement)new BSONBinaryElement(n, (BSONBinary)v)                        :  (BSONElement)new BSONBinaryElement((BSONBinary)v)                          },
      { BSONElementType.ObjectID,            (n, v) => n!=null ? (BSONElement)new BSONObjectIDElement(n, (BSONObjectID)v)                    :  (BSONElement)new BSONObjectIDElement((BSONObjectID)v)                      },
      { BSONElementType.Boolean,             (n, v) => n!=null ? (BSONElement)new BSONBooleanElement(n, v.AsBool())                          :  (BSONElement)new BSONBooleanElement(v.AsBool())                            },
      { BSONElementType.DateTime,            (n, v) => n!=null ? (BSONElement)new BSONDateTimeElement(n, v.AsDateTime())                     :  (BSONElement)new BSONDateTimeElement(v.AsDateTime())                       },
      { BSONElementType.Null,                (n, v) => n!=null ? (BSONElement)new BSONNullElement(n)                                         :  (BSONElement)new BSONNullElement()                                         },
      { BSONElementType.RegularExpression,   (n, v) => n!=null ? (BSONElement)new BSONRegularExpressionElement(n, (BSONRegularExpression)v)  :  (BSONElement)new BSONRegularExpressionElement((BSONRegularExpression)v)    },
      { BSONElementType.JavaScript,          (n, v) => n!=null ? (BSONElement)new BSONJavaScriptElement(n, v.AsString())                     :  (BSONElement)new BSONJavaScriptElement(v.AsString())                       },
      { BSONElementType.JavaScriptWithScope, (n, v) => n!=null ? (BSONElement)new BSONJavaScriptWithScopeElement(n, (BSONCodeWithScope)v)    :  (BSONElement)new BSONJavaScriptWithScopeElement((BSONCodeWithScope)v)      },
      { BSONElementType.TimeStamp,           (n, v) => n!=null ? (BSONElement)new BSONTimestampElement(n, (BSONTimestamp)v)                  :  (BSONElement)new BSONTimestampElement((BSONTimestamp)v)                    },
      { BSONElementType.Int64,               (n, v) => n!=null ? (BSONElement)new BSONInt64Element(n, v.AsLong())                            :  (BSONElement)new BSONInt64Element(v.AsLong())                              },
      { BSONElementType.MinKey,              (n, v) => n!=null ? (BSONElement)new BSONMinKeyElement(n)                                       :  (BSONElement)new BSONMinKeyElement()                                       },
      { BSONElementType.MaxKey,              (n, v) => n!=null ? (BSONElement)new BSONMaxKeyElement(n)                                       :  (BSONElement)new BSONMaxKeyElement()                                       },
    };

    public static Func<Stream, BSONElement> GetElementFactory(BSONElementType bsonType)
    {
      Func<Stream, BSONElement> result;
      if (s_ELEMENTSTREAMCTORS.TryGetValue(bsonType, out result)) return result;

      throw new BSONException(StringConsts.BSON_TYPE_NOT_SUPORTED_ERROR.Args(bsonType));
    }

    private static Func<string, object, BSONElement> GetValueSet(BSONElementType bsonType)
    {
      Func<string, object, BSONElement> result;
      if (s_ELEMENTVALUECTORS.TryGetValue(bsonType, out result)) return result;

      throw new BSONException(StringConsts.BSON_TYPE_NOT_SUPORTED_ERROR.Args(bsonType));
    }

    public static BSONElement MakeOfType(BSONElementType bsonType, string name, object value)
    {
      var f = GetValueSet(bsonType);
      return f(name, value);
    }



    #region .ctor

      protected BSONElement(string name)
      {
        m_Name = name;//null=array
      }

      protected BSONElement(Stream stream)
      {
        if (stream==null)
         throw new BSONException(StringConsts.ARGUMENT_ERROR+"BSONElement.ctor(stream==null)");

        m_Name = BinUtils.ReadCString(stream);
        ReadValueFromStream(stream);
      }

    #endregion .ctor

    private string m_Name;
    private int m_ByteSize;

    #region Properties
      /// <summary>
      /// Return the name of this element. The name is immutable.
      /// Check IaArrayElelemnt first, as this property can not be gotten for array elements
      /// </summary>
      public string Name
      {
        get
        {
          if (m_Name==null) throw new BSONException(StringConsts.BSON_ARRAY_ELM_NAME_ERROR);
          return m_Name;
        }
      }

      /// <summary>
      /// Returns true when this element does not have a name - it is a part of the array
      /// </summary>
      public bool IsArrayElement { get { return m_Name==null; } }

      /// <summary>
      /// Provides BSON classification of data type
      /// </summary>
      public abstract BSONElementType ElementType { get; }

      /// <summary>
      /// Gets/sets the value of this element polymorphically
      /// </summary>
      public abstract object ObjectValue{ get; set; }

      /// <summary>
      /// Recalculates the BSON binary size of this document expressed in bytes
      /// </summary>
      public int ByteSize{ get{ return GetByteSize(true); } }

      /// <summary>
      /// Returns the BSON binary size of this element expressed in bytes
      /// </summary>
      internal int GetByteSize(bool recalc)
      {
        if (recalc)
        {
          m_ByteSize = 1 + //type
                       BinUtils.UTF8Encoding.GetByteCount(Name) +
                       1 + //string terminator
                       GetValueByteSize();
        }

        return m_ByteSize;
      }
    #endregion Properties

    #region Pub Methods

      internal void MarkAsArrayItem()
      {
        m_Name = null;
      }

      public override string ToString()
      {
        return "{0}{1}={2}".Args(GetType().Name, Name, ObjectValue ?? "<null>");
      }

      public virtual void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
      {
        JSONWriter.Write(this.ObjectValue, wri, options);
      }

    #endregion

    #region Protected
      /// <summary>
      /// Returns the size of this element in bytes
      /// </summary>
      protected internal abstract int GetValueByteSize();

      /// <summary>
      /// Write element to stream
      /// </summary>
      protected internal void WriteToStream(Stream stream)
      {
        BinUtils.WriteByte(stream, (byte)ElementType);
        BinUtils.WriteCString(stream, Name);
        WriteValueToStream(stream);
      }

      /// <summary>
      /// Override to write this element into a stream
      /// </summary>
      protected internal abstract void WriteValueToStream(Stream stream);

      /// <summary>
      /// Override to read element's value from stream
      /// </summary>
      protected abstract void ReadValueFromStream(Stream stream);
    #endregion Protected

    #region IConvertible
      public TypeCode GetTypeCode()
      {
        var ov = this.ObjectValue;
        return ov==null ? TypeCode.Object : Type.GetTypeCode(ov.GetType());
      }

      public bool ToBoolean(IFormatProvider provider)
      {
        return this.ObjectValue.AsBool(handling: ConvertErrorHandling.Throw);
      }

      public byte ToByte(IFormatProvider provider)
      {
        return this.ObjectValue.AsByte(handling: ConvertErrorHandling.Throw);
      }

      public char ToChar(IFormatProvider provider)
      {
        return this.ObjectValue.AsChar(handling: ConvertErrorHandling.Throw);
      }

      public DateTime ToDateTime(IFormatProvider provider)
      {
        return this.ObjectValue.AsDateTime(dflt: MiscUtils.UNIX_EPOCH_START_DATE, handling: ConvertErrorHandling.Throw);
      }

      public decimal ToDecimal(IFormatProvider provider)
      {
        return this.ObjectValue.AsDecimal(handling: ConvertErrorHandling.Throw);
      }

      public double ToDouble(IFormatProvider provider)
      {
        return this.ObjectValue.AsDouble(handling: ConvertErrorHandling.Throw);
      }

      public short ToInt16(IFormatProvider provider)
      {
        return this.ObjectValue.AsShort(handling: ConvertErrorHandling.Throw);
      }

      public int ToInt32(IFormatProvider provider)
      {
        return this.ObjectValue.AsInt(handling: ConvertErrorHandling.Throw);
      }

      public long ToInt64(IFormatProvider provider)
      {
        return this.ObjectValue.AsLong(handling: ConvertErrorHandling.Throw);
      }

      public sbyte ToSByte(IFormatProvider provider)
      {
        return this.ObjectValue.AsSByte(handling: ConvertErrorHandling.Throw);
      }

      public float ToSingle(IFormatProvider provider)
      {
        return this.ObjectValue.AsFloat(handling: ConvertErrorHandling.Throw);
      }

      public string ToString(IFormatProvider provider)
      {
        return this.ObjectValue.AsString(handling: ConvertErrorHandling.Throw);
      }

      public object ToType(Type conversionType, IFormatProvider provider)
      {
        var ov = this.ObjectValue;
        if (ov==null) return null;
        return ((IConvertible)ov).ToType(conversionType, provider);
      }

      public ushort ToUInt16(IFormatProvider provider)
      {
        return this.ObjectValue.AsUShort(handling: ConvertErrorHandling.Throw);
      }

      public uint ToUInt32(IFormatProvider provider)
      {
        return this.ObjectValue.AsUInt(handling: ConvertErrorHandling.Throw);
      }

      public ulong ToUInt64(IFormatProvider provider)
      {
        return this.ObjectValue.AsULong(handling: ConvertErrorHandling.Throw);
      }
    #endregion


  }

  /// <summary>
  /// Base class for BSON elements with typed Value property
  /// </summary>
  public abstract class BSONElement<T> : BSONElement
  {
      protected BSONElement(string name, T value) : base(name)
      {
        m_Value = value;
      }

      protected BSONElement(Stream stream) : base(stream)
      {
      }

      protected T m_Value;

      public T Value
      {
        get { return m_Value;}
        set
        {
         if (!typeof(T).IsValueType)
         {
           object obj = value;
           if (obj==null)
            throw new BSONException(StringConsts.BSON_ELEMENT_OBJECT_VALUE_SET_ERROR.Args(typeof(T).FullName, Name, "value=null"));
         }
         m_Value = value;
        }
      }

      /// <summary>
      /// Gets/sets value as object. In normal cases use typed Value to get/set instead
      /// </summary>
      public sealed override object ObjectValue
      {
        get
        {
          return m_Value;
        }
        set
        {
          try
          {
             if (value==null)
               throw new BSONException("value=null");

             Value = (T)value;
          }
          catch(Exception error)
          {
             throw new BSONException(StringConsts.BSON_ELEMENT_OBJECT_VALUE_SET_ERROR.Args(typeof(T).FullName, Name, error.ToMessageWithType()), error);
          }
        }
      }

      public override string ToString()
      {
        return IsArrayElement ? "[{0}({1})]".Args(GetType().Name, Value) :  "{0}('{1}'='{2}')".Args(GetType().Name, Name, Value);
      }
  }
}
