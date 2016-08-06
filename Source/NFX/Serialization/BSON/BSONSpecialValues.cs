using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.DataAccess.Distributed;
using NFX.Serialization.JSON;


namespace NFX.Serialization.BSON
{

  /// <summary>
  /// Represents an ObjectId as a 12-byte BSON type
  /// </summary>
  public struct BSONObjectID : IJSONWritable
  {
    private const int THREE_BYTES_UINT_THRESHOLD = 16777216;
    private const int DATA_LENGTH = 12;

    public BSONObjectID(GDID gdid) : this(gdid.Bytes)
    {
    }

    public BSONObjectID(byte[] data)
    {
      if (data == null)
        throw new BSONException(StringConsts.ARGUMENT_ERROR+"BSONObjectID.ctor(data==null)");

      if (data.Length < DATA_LENGTH)
        throw new BSONException(StringConsts.BSON_OBJECTID_LENGTH_ERROR + " BSONObjectID.ctor(data.Length != 12)");

      Bytes = data;
    }

    public BSONObjectID(byte[] data, int startIdx = 0)
    {
      if (data == null)
        throw new BSONException(StringConsts.ARGUMENT_ERROR+"BSONObjectID.ctor(data==null)");

      if (data.Length - startIdx < DATA_LENGTH)
        throw new BSONException(StringConsts.BSON_OBJECTID_LENGTH_ERROR + " BSONObjectID.ctor(data.Length != 12)");

      var bytes = new byte[DATA_LENGTH];
      Buffer.BlockCopy(data, startIdx, bytes, 0, DATA_LENGTH);
      Bytes = bytes;
    }

    public BSONObjectID(uint epochSeconds, uint machineId, ushort processId, uint counter)
    {
      if (machineId >= THREE_BYTES_UINT_THRESHOLD || counter >= THREE_BYTES_UINT_THRESHOLD)
        throw new BSONException(StringConsts.BSON_THREE_BYTES_UINT_ERROR.Args("machineId and counter") + "  BSONObjectID.ctor(machineId>=2^24 | counter>=2^24)");

      var bytes = new byte[DATA_LENGTH];
      BinUtils.WriteUInt32(bytes, epochSeconds);
      BinUtils.WriteUInt24(bytes, machineId, 4);
      BinUtils.WriteUInt16(bytes, processId, 7);
      BinUtils.WriteUInt24(bytes, counter, 9);
      Bytes = bytes;
    }

    public uint EpochSeconds
    {
      get { return BinUtils.ReadUInt32(Bytes); } }

    public uint MachineID
    {
      get { return BinUtils.ReadUInt24(Bytes, 4); }
    }

    public ushort ProcessID
    {
      get  { return BinUtils.ReadUInt16(Bytes, 7); }
    }

    public uint Counter
    {
      get { return BinUtils.ReadUInt24(Bytes, 9); }
    }

    public readonly byte[] Bytes;

    /// <summary>
    /// Interprets BSON/MongoDB object ID as NFX GDID
    /// </summary>
    public GDID AsGDID { get { return new GDID(Bytes); } }

    public void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
    {
      JSONWriter.WriteMap(wri,
           nestingLevel+1,
           options,
           new DictionaryEntry("$oid", Convert.ToBase64String(Bytes))
      );
    }
  }

  /// <summary>
  /// Represents a BSON bynary type
  /// </summary>
  public struct BSONBinary : IJSONWritable
  {
    public BSONBinary(BSONBinaryType type, byte[] data)
    {
      if (data==null)
         throw new BSONException(StringConsts.ARGUMENT_ERROR+"BSONBinary.ctor(data==null)");

      Type = type;
      Data = data;
    }

    public readonly BSONBinaryType Type;
    public readonly byte[] Data;

    public void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
    {
      JSONWriter.WriteMap(wri,
           nestingLevel+1,
           options,
           new DictionaryEntry("$type", Type),
           new DictionaryEntry("$binary", Convert.ToBase64String(Data))
      );
    }
  }

  /// <summary>
  /// Represents a BSON regular expression type
  /// </summary>
  public struct BSONRegularExpression : IJSONWritable
  {
    public BSONRegularExpression(string pattern, BSONRegularExpressionOptions options)
    {
      if (pattern==null)
         throw new BSONException(StringConsts.ARGUMENT_ERROR+"BSONRegularExpression.ctor(pattern==null)");

      Pattern = pattern;
      Options = options;
    }

    public readonly string Pattern;
    public readonly BSONRegularExpressionOptions Options;

    public void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
    {
      JSONWriter.WriteMap(wri,
           nestingLevel+1,
           options,
           new DictionaryEntry("$regex", Pattern),
           new DictionaryEntry("$options", Options.ToBSONString())
      );
    }
  }

  /// <summary>
  /// Represents a BSON javascript with the scope type
  /// </summary>
  public struct BSONCodeWithScope : IJSONWritable
  {
    public BSONCodeWithScope(string code, BSONDocument scope)
    {
      if (code==null || scope == null)
         throw new BSONException(StringConsts.ARGUMENT_ERROR+"BSONCodeWithScope.ctor(code==null | scope==null)");

      Code = code;
      Scope = scope;
    }

    public readonly string Code;
    public readonly BSONDocument Scope;

    public void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
    {
      var data = new {js = Code, doc = Scope};
      JSONWriter.Write(data, wri, options);
    }
  }

  /// <summary>
  /// Represents a BSON timestamp type
  /// </summary>
  public struct BSONTimestamp : IJSONWritable
  {
    public BSONTimestamp(DateTime value, uint increment)
    {
      EpochSeconds = (uint)value.ToSecondsSinceUnixEpochStart();
      Increment = increment;
    }

    public readonly uint EpochSeconds;
    public readonly uint Increment;

    public void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
    {
      JSONWriter.WriteMap(wri,
           nestingLevel+1,
           options,
           new DictionaryEntry("$timestamp", new {t = EpochSeconds, i = Increment })
      );
    }
  }

}
