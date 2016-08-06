using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Globalization;
using NFX.Serialization.JSON;

namespace NFX.Serialization.BSON
{

  /// <summary>
  /// Represents a BSON array element with BSONElement[] value
  /// </summary>
  public sealed class BSONArrayElement : BSONElement<BSONElement[]>
  {
    /// <summary>
    /// Creates an array element
    /// </summary>
    public BSONArrayElement(BSONElement[] value) : base(null, value.NonNull(text: "value"))
    {
    }

    public BSONArrayElement(string name, BSONElement[] value)
      : base(name.NonNull(text: "name"), value.NonNull(text: "value"))
    {
    }

    internal BSONArrayElement(Stream stream) : base(stream)
    {
    }

    private int m_ValueByteSize;

    public override BSONElementType ElementType
    {
      get { return BSONElementType.Array; }
    }

    protected internal override int GetValueByteSize()
    {
      return getValueByteSize(true);
    }

    protected override void ReadValueFromStream(Stream stream)
    {
      var elements = new List<BSONElement>();
      long start = stream.Position;
      var totalSize = BinUtils.ReadInt32(stream);
      long read = 4;
      while(read<totalSize-1)
      {
        var et = BinUtils.ReadElementType(stream);
        var factory = BSONElement.GetElementFactory(et);
        var element = factory(stream);//element made
        element.MarkAsArrayItem();
        elements.Add(element);
        read = stream.Position - start;
      }
      Value = elements.ToArray();

      var terminator = BinUtils.ReadByte(stream);
      if (terminator != BinUtils.TERMINATOR || stream.Position - start != totalSize)
        throw new BSONException(StringConsts.BSON_EOD_ERROR);
    }

    protected internal override void WriteValueToStream(Stream stream)
    {
      //Arrays are writen as surrogated docuements
      var valueSize = getValueByteSize(false);
      BinUtils.WriteInt32(stream, valueSize);
      for (int i = 0; i < Value.Length; i++)
      {
        var element = Value[i];
        BinUtils.WriteByte(stream, (byte)element.ElementType);
        BinUtils.WriteCString(stream, i.ToString(CultureInfo.InvariantCulture));
        element.WriteValueToStream(stream);
      }
      BinUtils.WriteTerminator(stream);
    }

    private int getValueByteSize(bool recalc)
    {
      if (recalc)
      {
        m_ValueByteSize = 4 + //underlying document int32 size
                          1;  //underlying document terminator
        for (int i = 0; i < Value.Length; i++)
        {
          m_ValueByteSize += 1 + //type
                             BinUtils.GetIntDigitCount(i) + //name as UTF8 string size
                             1 + //string terminator
                             Value[i].GetValueByteSize();
        }

      }

      return m_ValueByteSize;
    }
  }

  /// <summary>
  /// Represents a BSON element with a BSON document value
  /// </summary>
  public sealed class BSONDocumentElement : BSONElement<BSONDocument>
  {
    /// <summary>
    /// Creates an array element
    /// </summary>
    public BSONDocumentElement(BSONDocument value) : base(null, value.NonNull(text: "value"))
    {
    }

    public BSONDocumentElement(string name, BSONDocument value) : base(name.NonNull(text: "name"), value.NonNull(text: "value"))
    {
    }

    internal BSONDocumentElement(Stream stream) : base(stream)
    {
    }

    public override BSONElementType ElementType { get { return BSONElementType.Document; } }

    protected internal override int GetValueByteSize()
    {
      return Value.GetByteSize(true);
    }

    protected override void ReadValueFromStream(Stream stream)
    {
      Value = new BSONDocument(stream);
    }

    protected internal override void WriteValueToStream(Stream stream)
    {
      Value.WriteAsBSON(stream);
    }
  }

  /// <summary>
  /// Represents a BSON element with a string value
  /// </summary>
  public sealed class BSONStringElement : BSONElement<string>
  {
    /// <summary>
    /// Creates an array element
    /// </summary>
    public BSONStringElement(string value) : base(null, value.NonNull(text: "value"))
    {
    }

    public BSONStringElement(string name, string value) : base(name.NonNull(text: "name"), value.NonNull(text: "value"))
    {
    }

    internal BSONStringElement(Stream stream) : base(stream)
    {
    }

    public override BSONElementType ElementType { get { return BSONElementType.String; } }

    protected internal override int GetValueByteSize()
    {
      return 4 + // int32 string length
             (Value == null ? 0 : BinUtils.UTF8Encoding.GetByteCount(Value)) +
             1; //string terminator
    }

    protected override void ReadValueFromStream(Stream stream)
    {
      var length = BinUtils.ReadInt32(stream);
      Value = BinUtils.ReadCString(stream, length);
    }

    protected internal override void WriteValueToStream(Stream stream)
    {
      var length = 1 + // string terminator
                   (Value == null ? 0 : BinUtils.UTF8Encoding.GetByteCount(Value)); // string UTF8 length
      BinUtils.WriteInt32(stream, length);
      BinUtils.WriteCString(stream, Value ?? string.Empty);
    }
  }

  /// <summary>
  /// Represents a BSON element with an int32 value
  /// </summary>
  public sealed class BSONInt32Element : BSONElement<Int32>
  {
    /// <summary>
    /// Creates an array element
    /// </summary>
    public BSONInt32Element(Int32 value) : base(null, value)
    {
    }

    public BSONInt32Element(string name, Int32 value) : base(name.NonNull(text: "name"), value)
    {
    }

    internal BSONInt32Element(Stream stream) : base(stream)
    {
    }

    public override BSONElementType ElementType { get { return BSONElementType.Int32; } }

    protected internal override int GetValueByteSize() { return 4; }

    protected override void ReadValueFromStream(Stream stream)
    {
      Value = BinUtils.ReadInt32(stream);
    }

    protected internal override void WriteValueToStream(Stream stream)
    {
      BinUtils.WriteInt32(stream, Value);
    }
  }

  /// <summary>
  /// Represents a BSON element with an bool value
  /// </summary>
  public sealed class BSONBooleanElement : BSONElement<bool>
  {
    /// <summary>
    /// Creates an array element
    /// </summary>
    public BSONBooleanElement(bool value) : base(null, value)
    {
    }

    public BSONBooleanElement(string name, bool value) : base(name, value)
    {
    }

    internal BSONBooleanElement(Stream stream) : base(stream)
    {
    }

    public override BSONElementType ElementType { get { return BSONElementType.Boolean; } }

    protected internal override int GetValueByteSize() { return 1; }

    protected override void ReadValueFromStream(Stream stream)
    {
      var flag = BinUtils.ReadByte(stream);
      switch (flag)
      {
        case (byte) BSONBoolean.True:
          Value = true; break;
        case (byte) BSONBoolean.False:
          Value = false; break;
        default:
          throw new BSONException(StringConsts.BSON_READ_BOOLEAN_ERROR.Args(flag));
      }
    }

    protected internal override void WriteValueToStream(Stream stream)
    {
      var b = Value ? (byte)BSONBoolean.True : (byte)BSONBoolean.False;
      BinUtils.WriteByte(stream, b);
    }
  }

  /// <summary>
  /// Represents a BSON element with an UTC DateTime value
  /// </summary>
  public sealed class BSONDateTimeElement : BSONElement<DateTime>
  {
    /// <summary>
    /// Creates an array element
    /// </summary>
    public BSONDateTimeElement(DateTime value) : base(null, value)
    {
    }

    public BSONDateTimeElement(string name, DateTime value) : base(name, value)
    {
    }

    internal BSONDateTimeElement(Stream stream) : base(stream)
    {
    }

    public override BSONElementType ElementType { get { return BSONElementType.DateTime; } }

    protected internal override int GetValueByteSize() { return 8; }

    protected override void ReadValueFromStream(Stream stream)
    {
      Value = BinUtils.ReadInt64(stream).FromMillisecondsSinceUnixEpochStart();
    }

    protected internal override void WriteValueToStream(Stream stream)
    {
      var dateValue = Value.ToMillisecondsSinceUnixEpochStart();
      BinUtils.WriteInt64(stream, dateValue);
    }

    public override void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
    {
      if (Value.ToUniversalTime() >= MiscUtils.UNIX_EPOCH_START_DATE)
      {
        JSONWriter.WriteMap(wri,
          nestingLevel + 1,
          options,
          new DictionaryEntry("$date", Value.ToMillisecondsSinceUnixEpochStart())
          );
      }
      else
      {
        JSONWriter.WriteMap(wri,
          nestingLevel + 1,
          options,
          new DictionaryEntry("$date", "{{ \"$numberLong\" : {0} }}".Args(Value.Ticks / TimeSpan.TicksPerMillisecond))
          );
      }
    }
  }

  /// <summary>
  /// Represents a BSON element with an regular expression value
  /// </summary>
  public sealed class BSONRegularExpressionElement : BSONElement<BSONRegularExpression>
  {
    /// <summary>
    /// Creates an array element
    /// </summary>
    public BSONRegularExpressionElement(BSONRegularExpression value) : base(null, value)
    {
    }

    public BSONRegularExpressionElement(string name, BSONRegularExpression value) : base(name, value)
    {
    }

    internal BSONRegularExpressionElement(Stream stream) : base(stream)
    {
    }

    public override BSONElementType ElementType { get { return BSONElementType.RegularExpression; } }

    protected internal override int GetValueByteSize()
    {
      return BinUtils.UTF8Encoding.GetByteCount(Value.Pattern) +
             Value.Options.Count() +
             2; //string terminator
    }

    protected override void ReadValueFromStream(Stream stream)
    {
      var pattern = BinUtils.ReadCString(stream);
      var options = BinUtils.ReadCString(stream).ToBSONOptions();
      Value = new BSONRegularExpression(pattern, options);
    }

    protected internal override void WriteValueToStream(Stream stream)
    {
      BinUtils.WriteCString(stream, Value.Pattern ?? string.Empty);
      BinUtils.WriteCString(stream, Value.Options.ToBSONString());
    }
  }

  /// <summary>
  /// Represents a BSON element with an int32 value
  /// </summary>
  public sealed class BSONDoubleElement : BSONElement<double>
  {
    /// <summary>
    /// Creates an array element
    /// </summary>
    public BSONDoubleElement(double value) : base(null, value)
    {
    }

    public BSONDoubleElement(string name, double value) : base(name.NonNull(text: "name"), value)
    {
    }

    internal BSONDoubleElement(Stream stream) : base(stream)
    {
    }

    public override BSONElementType ElementType { get { return BSONElementType.Double; } }

    protected internal override int GetValueByteSize() { return 8; }

    protected override void ReadValueFromStream(Stream stream)
    {
      Value = BinUtils.ReadDouble(stream);
    }

    protected internal override void WriteValueToStream(Stream stream)
    {
      BinUtils.WriteDouble(stream, Value);
    }
  }

  /// <summary>
  /// Represents a BSON element with an int64 value
  /// </summary>
  public sealed class BSONInt64Element : BSONElement<Int64>
  {
    /// <summary>
    /// Creates an array element
    /// </summary>
    public BSONInt64Element(Int64 value) : base(null, value)
    {
    }

    public BSONInt64Element(string name, Int64 value) : base(name.NonNull(text: "name"), value)
    {
    }

    internal BSONInt64Element(Stream stream) : base(stream)
    {
    }

    public override BSONElementType ElementType { get { return BSONElementType.Int64; } }

    protected internal override int GetValueByteSize() { return 8; }

    protected override void ReadValueFromStream(Stream stream)
    {
      Value = BinUtils.ReadInt64(stream);
    }

    protected internal override void WriteValueToStream(Stream stream)
    {
      BinUtils.WriteInt64(stream, Value);
    }

    public override void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
    {
      JSONWriter.WriteMap(wri,
          nestingLevel + 1,
          options,
          new DictionaryEntry("$numberLong", Value)
          );
    }
  }

  /// <summary>
  /// Represents a BSON element with an BSONObjectID value
  /// </summary>
  public sealed class BSONObjectIDElement : BSONElement<BSONObjectID>
  {
    /// <summary>
    /// Creates an array element
    /// </summary>
    public BSONObjectIDElement(BSONObjectID value) : base(null, value)
    {
    }

    public BSONObjectIDElement(string name, BSONObjectID value) : base(name.NonNull(text: "name"), value)
    {
    }

    internal BSONObjectIDElement(Stream stream) : base(stream)
    {
    }

    public override BSONElementType ElementType { get { return BSONElementType.ObjectID; } }

    protected internal override int GetValueByteSize() { return 12; }

    protected override void ReadValueFromStream(Stream stream)
    {
      var buffer = new byte[12];
      stream.Read(buffer, 0, 12);
      Value = new BSONObjectID(buffer);
    }

    protected internal override void WriteValueToStream(Stream stream)
    {
      stream.Write(Value.Bytes, 0, 12);
    }
  }

  /// <summary>
  /// Represents a BSON element with an BSONBinary value
  /// </summary>
  public sealed class BSONBinaryElement : BSONElement<BSONBinary>
  {
    /// <summary>
    /// Creates an array element
    /// </summary>
    public BSONBinaryElement(BSONBinary value) : base(null, value)
    {
    }

    public BSONBinaryElement(string name, BSONBinary value) : base(name.NonNull(text: "name"), value)
    {
    }

    internal BSONBinaryElement(Stream stream) : base(stream)
    {
    }

    public override BSONElementType ElementType { get { return BSONElementType.Binary; } }

    protected internal override int GetValueByteSize()
    {
      return 4 + // int32 size
             1 + // subtype size
             Value.Data.Length;
    }

    protected override void ReadValueFromStream(Stream stream)
    {
      var length = BinUtils.ReadInt32(stream);
      var type = BinUtils.ReadByte(stream);
      var buffer = new byte[length];
      stream.Read(buffer, 0, length);

      Value = new BSONBinary((BSONBinaryType)type, buffer);
    }

    protected internal override void WriteValueToStream(Stream stream)
    {
      BinUtils.WriteInt32(stream, Value.Data.Length);
      BinUtils.WriteByte(stream, (byte)Value.Type);
      BinUtils.WriteBinary(stream, Value.Data);
    }
  }

  /// <summary>
  /// Represents a BSON element with a javascript value
  /// </summary>
  public sealed class BSONJavaScriptElement : BSONElement<string>
  {
    /// <summary>
    /// Creates an array element
    /// </summary>
    public BSONJavaScriptElement(string value) : base(null, value.NonNull(text: "value"))
    {
    }

    public BSONJavaScriptElement(string name, string value) : base(name.NonNull(text: "name"), value.NonNull(text: "value"))
    {
    }

    internal BSONJavaScriptElement(Stream stream) : base(stream)
    {
    }

    public override BSONElementType ElementType { get { return BSONElementType.JavaScript; } }

    protected internal override int GetValueByteSize()
    {
      return 4 + // int32 script string length
             (Value == null ? 0 : BinUtils.UTF8Encoding.GetByteCount(Value)) +
             1; //string terminator
    }

    protected override void ReadValueFromStream(Stream stream)
    {
      var length = BinUtils.ReadInt32(stream);
      Value = BinUtils.ReadCString(stream, length);
    }

    protected internal override void WriteValueToStream(Stream stream)
    {
      var length = 1 + // string terminator
                   (Value == null ? 0 : BinUtils.UTF8Encoding.GetByteCount(Value)); // string UTF8 length
      BinUtils.WriteInt32(stream, length);
      BinUtils.WriteCString(stream, Value ?? string.Empty);
    }
  }

  /// <summary>
  /// Represents a BSON element with a javascript with scope value
  /// </summary>
  public sealed class BSONJavaScriptWithScopeElement : BSONElement<BSONCodeWithScope>
  {
    /// <summary>
    /// Creates an array element
    /// </summary>
    public BSONJavaScriptWithScopeElement(BSONCodeWithScope value) : base(null, value)
    {
    }

    public BSONJavaScriptWithScopeElement(string name, BSONCodeWithScope value) : base(name.NonNull(text: "name"), value)
    {
    }

    internal BSONJavaScriptWithScopeElement(Stream stream) : base(stream)
    {
    }

    private int m_ValueByteSize;

    public override BSONElementType ElementType { get { return BSONElementType.JavaScriptWithScope; } }

    protected internal override int GetValueByteSize()
    {
      return GetValueByteSize(true);
    }

    protected override void ReadValueFromStream(Stream stream)
    {
      BinUtils.ReadInt32(stream); // fullLength
      var length = BinUtils.ReadInt32(stream);
      var code = BinUtils.ReadCString(stream, length);
      var document = new BSONDocument(stream);
      Value = new BSONCodeWithScope(code, document);
    }

    protected internal override void WriteValueToStream(Stream stream)
    {
      BinUtils.WriteInt32(stream, GetValueByteSize(false));
      var length = 1 + // string terminator
                   BinUtils.UTF8Encoding.GetByteCount(Value.Code); // string UTF8 length
      BinUtils.WriteInt32(stream, length);
      BinUtils.WriteCString(stream, Value.Code);
      Value.Scope.WriteAsBSONCore(stream);
    }

    private int GetValueByteSize(bool recalc)
    {
      if (recalc)
      {
        m_ValueByteSize = 4 + // int32 full size
                     4 + // int32 js string length
                     BinUtils.UTF8Encoding.GetByteCount(Value.Code) +
                     1 + //string terminator
                     Value.Scope.GetByteSize(true);
      }

      return m_ValueByteSize;
    }
  }

  /// <summary>
  /// Represents a BSON element with an Timestamp value
  /// </summary>
  public sealed class BSONTimestampElement : BSONElement<BSONTimestamp>
  {
    /// <summary>
    /// Creates an array element
    /// </summary>
    public BSONTimestampElement(BSONTimestamp value) : base(null, value)
    {
    }

    public BSONTimestampElement(string name, BSONTimestamp value) : base(name, value)
    {
    }

    internal BSONTimestampElement(Stream stream) : base(stream)
    {
    }

    public override BSONElementType ElementType { get { return BSONElementType.TimeStamp; } }

    protected internal override int GetValueByteSize() { return 8; }

    protected override void ReadValueFromStream(Stream stream)
    {
      var increment = BinUtils.ReadUInt32(stream);
      var epoch = ((long)BinUtils.ReadInt32(stream)).FromSecondsSinceUnixEpochStart();
      Value = new BSONTimestamp(epoch, increment);
    }

    protected internal override void WriteValueToStream(Stream stream)
    {
      BinUtils.WriteUInt32(stream, Value.Increment);
      BinUtils.WriteUInt32(stream, Value.EpochSeconds);
    }
  }

  /// <summary>
  /// Represents an element with Min Key value
  /// </summary>
  public sealed class BSONMinKeyElement : BSONElement
  {
    public BSONMinKeyElement(): base((string)null){}

    public BSONMinKeyElement(string name) :base(name.NonNull(text: "name")){}

    internal BSONMinKeyElement(Stream stream) : base(stream)
    {
    }

    public override BSONElementType ElementType { get { return BSONElementType.MinKey; } }

    public override object ObjectValue { get { return null; } set { } }

    protected internal override int GetValueByteSize() { return 0; }

    protected internal override void WriteValueToStream(Stream stream){ }

    protected override void ReadValueFromStream(Stream stream){ }

    public override void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
    {
      JSONWriter.WriteMap(wri,
           nestingLevel+1,
           options,
           new DictionaryEntry("$minKey", 1)
      );
    }
  }

  /// <summary>
  /// Represents an element with MaxKey value
  /// </summary>
  public sealed class BSONMaxKeyElement : BSONElement
  {
    public BSONMaxKeyElement(): base((string)null){}

    public BSONMaxKeyElement(string name) :base(name.NonNull(text: "name"))
    {
    }

    internal BSONMaxKeyElement(Stream stream) : base(stream)
    {
    }

    public override BSONElementType ElementType { get { return BSONElementType.MaxKey; } }

    public override object ObjectValue { get { return null; } set { } }

    protected internal override int GetValueByteSize() { return 0; }

    protected internal override void WriteValueToStream(Stream stream){ }

    protected override void ReadValueFromStream(Stream stream){ }

    public override void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
    {
      JSONWriter.WriteMap(wri,
           nestingLevel+1,
           options,
           new DictionaryEntry("$maxKey", 1)
      );
    }
  }

  /// <summary>
  /// Represents an element with NULL value
  /// </summary>
  public sealed class BSONNullElement : BSONElement
  {
    public BSONNullElement() : base((string)null)
    {
    }

    public BSONNullElement(string name) : base(name.NonNull(text: "name"))
    {
    }

    internal BSONNullElement(Stream stream) : base(stream)
    {
    }

    public override BSONElementType ElementType { get { return BSONElementType.Null; } }

    public override object ObjectValue { get { return null; } set { } }

    protected internal override int GetValueByteSize() { return 0; }

    protected internal override void WriteValueToStream(Stream stream){ }

    protected override void ReadValueFromStream(Stream stream){ }
  }

}
