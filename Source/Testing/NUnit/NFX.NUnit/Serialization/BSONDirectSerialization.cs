/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.Log;
using NFX.DataAccess.Distributed;
using NFX.Serialization.BSON;
using NFX.Serialization.JSON;
using NFX.Financial;


namespace NFX.NUnit.Serialization
{
  [TestFixture]
  public class BSONDirectSerialization
  {
    [BSONSerializable("25C155E8-59DC-4E5B-A9AD-6A94CB4381BD")]
    public class TypicalData : IArchiveLoggable
    {
      public Guid     Guid         { get; set; } // PKFieldName
      public bool     True         { get; set; } public const string BSON_FLD_TRUE         = "True";
      public bool     False        { get; set; } public const string BSON_FLD_FALSE        = "False";
      public char     CharMin      { get; set; } public const string BSON_FLD_CHARMIN      = "CharMin";
      public char     CharMax      { get; set; } public const string BSON_FLD_CHARMAX      = "CharMax";
      public sbyte    SByteMin     { get; set; } public const string BSON_FLD_SBYTEMIN     = "SByteMin";
      public sbyte    SByteMax     { get; set; } public const string BSON_FLD_SBYTEMAX     = "SByteMax";
      public byte     ByteMin      { get; set; } public const string BSON_FLD_BYTEMIN      = "ByteMin";
      public byte     ByteMax      { get; set; } public const string BSON_FLD_BYTEMAX      = "ByteMax";
      public short    Int16Min     { get; set; } public const string BSON_FLD_INT16MIN     = "Int16Min";
      public short    Int16Max     { get; set; } public const string BSON_FLD_INT16MAX     = "Int16Max";
      public ushort   UInt16Min    { get; set; } public const string BSON_FLD_UINT16MIN    = "UInt16Min";
      public ushort   UInt16Max    { get; set; } public const string BSON_FLD_UINT16MAX    = "UInt16Max";
      public int      Int32Min     { get; set; } public const string BSON_FLD_INT32MIN     = "Int32Min";
      public int      Int32Max     { get; set; } public const string BSON_FLD_INT32MAX     = "Int32Max";
      public uint     UInt32Min    { get; set; } public const string BSON_FLD_UINT32MIN    = "UInt32Min";
      public uint     UInt32Max    { get; set; } public const string BSON_FLD_UINT32MAX    = "UInt32Max";
      public long     Int64Min     { get; set; } public const string BSON_FLD_INT64MIN     = "Int64Min";
      public long     Int64Max     { get; set; } public const string BSON_FLD_INT64MAX     = "Int64Max";
      public ulong    UInt64Min    { get; set; } public const string BSON_FLD_UINT64MIN    = "UInt64Min";
      public ulong    UInt64Max    { get; set; } public const string BSON_FLD_UINT64MAX    = "UInt64Max";
      public float    SingleEps    { get; set; } public const string BSON_FLD_SINGLEEPS    = "SingleEps";
      public float    SingleMin    { get; set; } public const string BSON_FLD_SINGLEMIN    = "SingleMin";
      public float    SingleMax    { get; set; } public const string BSON_FLD_SINGLEMAX    = "SingleMax";
      public float    SingleNaN    { get; set; } public const string BSON_FLD_SINGLENAN    = "SingleNaN";
      public float    SinglePosInf { get; set; } public const string BSON_FLD_SINGLEPOSINF = "SinglePosInf";
      public float    SingleNegInf { get; set; } public const string BSON_FLD_SINGLENEGINF = "SingleNegInf";
      public double   DoubleEps    { get; set; } public const string BSON_FLD_DOUBLEEPS    = "DoubleEps";
      public double   DoubleMin    { get; set; } public const string BSON_FLD_DOUBLEMIN    = "DoubleMin";
      public double   DoubleMax    { get; set; } public const string BSON_FLD_DOUBLEMAX    = "DoubleMax";
      public double   DoubleNaN    { get; set; } public const string BSON_FLD_DOUBLENAN    = "DoubleNaN";
      public double   DoublePosInf { get; set; } public const string BSON_FLD_DOUBLEPOSINF = "DoublePosInf";
      public double   DoubleNegInf { get; set; } public const string BSON_FLD_DOUBLENEGINF = "DoubleNegInf";
      public decimal  DecimalMin   { get; set; } public const string BSON_FLD_DECIMALMIN   = "DecimalMin";
      public decimal  DecimalMax   { get; set; } public const string BSON_FLD_DECIMALMAX   = "DecimalMax";
      public decimal  DecimalZero  { get; set; } public const string BSON_FLD_DECIMALZERO  = "DecimalZero";
      public decimal  DecimalOne   { get; set; } public const string BSON_FLD_DECIMALONE   = "DecimalOne";
      public decimal  DecimalMOne  { get; set; } public const string BSON_FLD_DECIMALMONE  = "DecimalMOne";
      public DateTime DateTimeMin  { get; set; } public const string BSON_FLD_DATETIMEMIN  = "DateTimeMin";
      public DateTime DateTimeMax  { get; set; } public const string BSON_FLD_DATETIMEMAX  = "DateTimeMax";
      public DateTime DateTimeNow  { get; set; } public const string BSON_FLD_DATETIMENOW  = "DateTimeNow";
      public DateTime DateTimeUtc  { get; set; } public const string BSON_FLD_DATETIMEUTC  = "DateTimeUtc";
      public TimeSpan TimeSpanMin  { get; set; } public const string BSON_FLD_TIMESPANMIN  = "TimeSpanMin";
      public TimeSpan TimeSpanMax  { get; set; } public const string BSON_FLD_TIMESPANMAX  = "TimeSpanMax";
      public string   StringEmpty  { get; set; } public const string BSON_FLD_STRINGEMPTY  = "StringEmpty";
      public string   StringNull   { get; set; } public const string BSON_FLD_STRINGNULL   = "StringNull";
      public string   String       { get; set; } public const string BSON_FLD_STRING       = "String";

      public bool IsKnownTypeForBSONDeserialization(Type type)
      {
        return false;
      }

      public void SerializeToBSON(BSONSerializer serializer, BSONDocument doc, IBSONSerializable parent, ref object context)
      {
        serializer.AddTypeIDField(doc, parent, this, context);

        doc.Add(serializer.PKFieldName, Guid, required: true);
        doc.Add(BSON_FLD_TRUE,         True);
        doc.Add(BSON_FLD_FALSE,        False);
        doc.Add(BSON_FLD_CHARMIN,      CharMin);
        doc.Add(BSON_FLD_CHARMAX,      CharMax);
        doc.Add(BSON_FLD_SBYTEMIN,     SByteMin);
        doc.Add(BSON_FLD_SBYTEMAX,     SByteMax);
        doc.Add(BSON_FLD_BYTEMIN,      ByteMin);
        doc.Add(BSON_FLD_BYTEMAX,      ByteMax);
        doc.Add(BSON_FLD_INT16MIN,     Int16Min);
        doc.Add(BSON_FLD_INT16MAX,     Int16Max);
        doc.Add(BSON_FLD_UINT16MIN,    UInt16Min);
        doc.Add(BSON_FLD_UINT16MAX,    UInt16Max);
        doc.Add(BSON_FLD_INT32MIN,     Int32Min);
        doc.Add(BSON_FLD_INT32MAX,     Int32Max);
        doc.Add(BSON_FLD_UINT32MIN,    UInt32Min);
        doc.Add(BSON_FLD_UINT32MAX,    UInt32Max);
        doc.Add(BSON_FLD_INT64MIN,     Int64Min);
        doc.Add(BSON_FLD_INT64MAX,     Int64Max);
        doc.Add(BSON_FLD_UINT64MIN,    UInt64Min);
        doc.Add(BSON_FLD_UINT64MAX,    UInt64Max);
        doc.Add(BSON_FLD_SINGLEEPS,    SingleEps);
        doc.Add(BSON_FLD_SINGLEMIN,    SingleMin);
        doc.Add(BSON_FLD_SINGLEMAX,    SingleMax);
        doc.Add(BSON_FLD_SINGLENAN,    SingleNaN);
        doc.Add(BSON_FLD_SINGLEPOSINF, SinglePosInf);
        doc.Add(BSON_FLD_SINGLENEGINF, SingleNegInf);
        doc.Add(BSON_FLD_DOUBLEEPS,    DoubleEps);
        doc.Add(BSON_FLD_DOUBLEMIN,    DoubleMin);
        doc.Add(BSON_FLD_DOUBLEMAX,    DoubleMax);
        doc.Add(BSON_FLD_DOUBLENAN,    DoubleNaN);
        doc.Add(BSON_FLD_DOUBLEPOSINF, DoublePosInf);
        doc.Add(BSON_FLD_DOUBLENEGINF, DoubleNegInf);
        doc.Add(BSON_FLD_DECIMALMIN,   DecimalMin);
        doc.Add(BSON_FLD_DECIMALMAX,   DecimalMax);
        doc.Add(BSON_FLD_DECIMALZERO,  DecimalZero);
        doc.Add(BSON_FLD_DECIMALONE,   DecimalOne);
        doc.Add(BSON_FLD_DECIMALMONE,  DecimalMOne);
        doc.Add(BSON_FLD_DATETIMEMIN,  DateTimeMin);
        doc.Add(BSON_FLD_DATETIMEMAX,  DateTimeMax);
        doc.Add(BSON_FLD_DATETIMENOW,  DateTimeNow);
        doc.Add(BSON_FLD_DATETIMEUTC,  DateTimeUtc);
        doc.Add(BSON_FLD_TIMESPANMIN,  TimeSpanMin);
        doc.Add(BSON_FLD_TIMESPANMAX,  TimeSpanMax);
        doc.Add(BSON_FLD_STRINGEMPTY,  StringEmpty);
        doc.Add(BSON_FLD_STRINGNULL,   StringNull);
        doc.Add(BSON_FLD_STRING,       String);
      }

      public void DeserializeFromBSON(BSONSerializer serializer, BSONDocument doc, ref object context)
      {
        Guid         = doc.TryGetObjectValueOf(serializer.PKFieldName).AsGUID(Guid.Empty);
        True         = doc.TryGetObjectValueOf(BSON_FLD_TRUE).AsBool();
        False        = doc.TryGetObjectValueOf(BSON_FLD_FALSE).AsBool();
        CharMin      = doc.TryGetObjectValueOf(BSON_FLD_CHARMIN).AsChar();
        CharMax      = doc.TryGetObjectValueOf(BSON_FLD_CHARMAX).AsChar();
        SByteMin     = doc.TryGetObjectValueOf(BSON_FLD_SBYTEMIN).AsSByte();
        SByteMax     = doc.TryGetObjectValueOf(BSON_FLD_SBYTEMAX).AsSByte();
        ByteMin      = doc.TryGetObjectValueOf(BSON_FLD_BYTEMIN).AsByte();
        ByteMax      = doc.TryGetObjectValueOf(BSON_FLD_BYTEMAX).AsByte();
        Int16Min     = doc.TryGetObjectValueOf(BSON_FLD_INT16MIN).AsShort();
        Int16Max     = doc.TryGetObjectValueOf(BSON_FLD_INT16MAX).AsShort();
        UInt16Min    = doc.TryGetObjectValueOf(BSON_FLD_UINT16MIN).AsUShort();
        UInt16Max    = doc.TryGetObjectValueOf(BSON_FLD_UINT16MAX).AsUShort();
        Int32Min     = doc.TryGetObjectValueOf(BSON_FLD_INT32MIN).AsInt();
        Int32Max     = doc.TryGetObjectValueOf(BSON_FLD_INT32MAX).AsInt();
        UInt32Min    = doc.TryGetObjectValueOf(BSON_FLD_UINT32MIN).AsUInt();
        UInt32Max    = doc.TryGetObjectValueOf(BSON_FLD_UINT32MAX).AsUInt();
        Int64Min     = doc.TryGetObjectValueOf(BSON_FLD_INT64MIN).AsLong();
        Int64Max     = doc.TryGetObjectValueOf(BSON_FLD_INT64MAX).AsLong();
        UInt64Min    = doc.TryGetObjectValueOf(BSON_FLD_UINT64MIN).AsULong();
        UInt64Max    = doc.TryGetObjectValueOf(BSON_FLD_UINT64MAX).AsULong();
        SingleEps    = doc.TryGetObjectValueOf(BSON_FLD_SINGLEEPS).AsFloat();
        SingleMin    = doc.TryGetObjectValueOf(BSON_FLD_SINGLEMIN).AsFloat();
        SingleMax    = doc.TryGetObjectValueOf(BSON_FLD_SINGLEMAX).AsFloat();
        SingleNaN    = doc.TryGetObjectValueOf(BSON_FLD_SINGLENAN).AsFloat();
        SinglePosInf = doc.TryGetObjectValueOf(BSON_FLD_SINGLEPOSINF).AsFloat();
        SingleNegInf = doc.TryGetObjectValueOf(BSON_FLD_SINGLENEGINF).AsFloat();
        DoubleEps    = doc.TryGetObjectValueOf(BSON_FLD_DOUBLEEPS).AsDouble();
        DoubleMin    = doc.TryGetObjectValueOf(BSON_FLD_DOUBLEMIN).AsDouble();
        DoubleMax    = doc.TryGetObjectValueOf(BSON_FLD_DOUBLEMAX).AsDouble();
        DoubleNaN    = doc.TryGetObjectValueOf(BSON_FLD_DOUBLENAN).AsDouble();
        DoublePosInf = doc.TryGetObjectValueOf(BSON_FLD_DOUBLEPOSINF).AsDouble();
        DoubleNegInf = doc.TryGetObjectValueOf(BSON_FLD_DOUBLENEGINF).AsDouble();
        DecimalMin   = RowConverter.Decimal_BSONtoCLR(doc[BSON_FLD_DECIMALMIN]);
        DecimalMax   = RowConverter.Decimal_BSONtoCLR(doc[BSON_FLD_DECIMALMAX]);
        DecimalZero  = RowConverter.Decimal_BSONtoCLR(doc[BSON_FLD_DECIMALZERO]);
        DecimalOne   = RowConverter.Decimal_BSONtoCLR(doc[BSON_FLD_DECIMALONE]);
        DecimalMOne  = RowConverter.Decimal_BSONtoCLR(doc[BSON_FLD_DECIMALMONE]);
        DateTimeMin  = doc.TryGetObjectValueOf(BSON_FLD_DATETIMEMIN).AsDateTime();
        DateTimeMax  = doc.TryGetObjectValueOf(BSON_FLD_DATETIMEMAX).AsDateTime();
        DateTimeNow  = doc.TryGetObjectValueOf(BSON_FLD_DATETIMENOW).AsDateTime();
        DateTimeUtc  = doc.TryGetObjectValueOf(BSON_FLD_DATETIMEUTC).AsDateTime();
        TimeSpanMin  = doc.TryGetObjectValueOf(BSON_FLD_TIMESPANMIN).AsTimeSpan();
        TimeSpanMax  = doc.TryGetObjectValueOf(BSON_FLD_TIMESPANMAX).AsTimeSpan();
        StringEmpty  = doc.TryGetObjectValueOf(BSON_FLD_STRINGEMPTY).AsString();
        StringNull   = doc.TryGetObjectValueOf(BSON_FLD_STRINGNULL).AsString();
        String       = doc.TryGetObjectValueOf(BSON_FLD_STRING).AsString();
      }
    }

    [Test]
    public void SerializeTypicalData()
    {
      var ser = new BSONSerializer(new BSONTypeResolver(typeof(TypicalData)));

      var data = new TypicalData
      {
        Guid = Guid.NewGuid(),
        True = true,
        False = false,
        CharMin = Char.MinValue,
        CharMax = Char.MaxValue,
        SByteMin = SByte.MinValue,
        SByteMax = SByte.MaxValue,
        ByteMin = Byte.MinValue,
        ByteMax = Byte.MaxValue,
        Int16Min = Int16.MinValue,
        Int16Max = Int16.MaxValue,
        UInt16Min = UInt16.MinValue,
        UInt16Max = UInt16.MaxValue,
        Int32Min = Int32.MinValue,
        Int32Max = Int32.MaxValue,
        UInt32Min = UInt32.MinValue,
        UInt32Max = UInt32.MaxValue,
        Int64Min = Int64.MinValue,
        Int64Max = Int64.MaxValue,
        UInt64Min = UInt64.MinValue,
        UInt64Max = UInt64.MaxValue,
        SingleEps = Single.Epsilon,
        SingleMin = Single.MinValue,
        SingleMax = Single.MaxValue,
        SingleNaN = Single.NaN,
        SinglePosInf = Single.PositiveInfinity,
        SingleNegInf = Single.NegativeInfinity,
        DoubleEps = Double.Epsilon,
        DoubleMin = Double.MinValue,
        DoubleMax = Double.MaxValue,
        DoubleNaN = Double.NaN,
        DoublePosInf = Double.PositiveInfinity,
        DoubleNegInf = Double.NegativeInfinity,
        DecimalMin = RowConverter.MIN_DECIMAL,
        DecimalMax = RowConverter.MAX_DECIMAL,
        DecimalZero = Decimal.Zero,
        DecimalOne = Decimal.One,
        DecimalMOne = Decimal.MinusOne,
        DateTimeMin = DateTime.MinValue,
        DateTimeMax = DateTime.MaxValue,
        DateTimeNow = DateTime.Now,
        DateTimeUtc = DateTime.UtcNow,
        TimeSpanMin = TimeSpan.MinValue,
        TimeSpanMax = TimeSpan.MaxValue,
        StringEmpty = String.Empty,
        StringNull = null,
        String = "TypicalString",
      };

      var doc = ser.Serialize(data);

      Console.WriteLine(doc.ToJSON(JSONWritingOptions.PrettyPrint));

      Aver.IsTrue(doc.IndexOfName(ser.TypeIDFieldName) >= 0);

      var got = ser.Deserialize(doc) as TypicalData;

      averDataAreEqual(data, got);
    }

    private void averDataAreEqual(TypicalData data, TypicalData got)
    {
      Aver.IsNotNull(got);
      Aver.AreNotSameRef(data, got);

      Aver.AreEqual(data.Guid,         got.Guid);
      Aver.AreEqual(data.True,         got.True);
      Aver.AreEqual(data.False,        got.False);
      Aver.AreEqual(data.CharMin,      got.CharMin);
      Aver.AreEqual(data.CharMax,      got.CharMax);
      Aver.AreEqual(data.SByteMin,     got.SByteMin);
      Aver.AreEqual(data.SByteMax,     got.SByteMax);
      Aver.AreEqual(data.ByteMin,      got.ByteMin);
      Aver.AreEqual(data.ByteMax,      got.ByteMax);
      Aver.AreEqual(data.Int16Min,     got.Int16Min);
      Aver.AreEqual(data.Int16Max,     got.Int16Max);
      Aver.AreEqual(data.UInt16Min,    got.UInt16Min);
      Aver.AreEqual(data.UInt16Max,    got.UInt16Max);
      Aver.AreEqual(data.Int32Min,     got.Int32Min);
      Aver.AreEqual(data.Int32Max,     got.Int32Max);
      Aver.AreEqual(data.UInt32Min,    got.UInt32Min);
      Aver.AreEqual(data.UInt32Max,    got.UInt32Max);
      Aver.AreEqual(data.Int64Min,     got.Int64Min);
      Aver.AreEqual(data.Int64Max,     got.Int64Max);
      Aver.AreEqual(data.UInt64Min,    got.UInt64Min);
      Aver.AreEqual(data.UInt64Max,    got.UInt64Max);
      Aver.AreEqual(data.SingleEps,    got.SingleEps);
      Aver.AreEqual(data.SingleMin,    got.SingleMin);
      Aver.AreEqual(data.SingleMax,    got.SingleMax);
      Aver.AreEqual(data.SingleNaN,    got.SingleNaN);
      Aver.AreEqual(data.SinglePosInf, got.SinglePosInf);
      Aver.AreEqual(data.SingleNegInf, got.SingleNegInf);
      Aver.AreEqual(data.DoubleEps,    got.DoubleEps);
      Aver.AreEqual(data.DoubleMin,    got.DoubleMin);
      Aver.AreEqual(data.DoubleMax,    got.DoubleMax);
      Aver.AreEqual(data.DoubleNaN,    got.DoubleNaN);
      Aver.AreEqual(data.DoublePosInf, got.DoublePosInf);
      Aver.AreEqual(data.DoubleNegInf, got.DoubleNegInf);
      Aver.AreEqual(data.DecimalMin,   got.DecimalMin);
      Aver.AreEqual(data.DecimalMax,   got.DecimalMax);
      Aver.AreEqual(data.DecimalZero,  got.DecimalZero);
      Aver.AreEqual(data.DecimalOne,   got.DecimalOne);
      Aver.AreEqual(data.DecimalMOne,  got.DecimalMOne);
      Aver.AreEqual(data.DateTimeMin,  got.DateTimeMin);
      Aver.AreEqual(data.DateTimeMax,  got.DateTimeMax);
      Aver.AreEqual(data.DateTimeNow,  got.DateTimeNow);
      Aver.AreEqual(data.DateTimeUtc,  got.DateTimeUtc);
      Aver.AreEqual(data.TimeSpanMin,  got.TimeSpanMin);
      Aver.AreEqual(data.TimeSpanMax,  got.TimeSpanMax);
      Aver.AreEqual(data.StringEmpty,  got.StringEmpty);
      Aver.AreEqual(data.StringNull,   got.StringNull);
      Aver.AreEqual(data.String,       got.String);
    }

    [Test]
    public void SerializeDeserializeLogMessage()
    {
      var ser = new BSONSerializer( new BSONTypeResolver(typeof(Message)) );

      var msg = new NFX.Log.Message
      {
         Type = MessageType.DebugGlue,
         TimeStamp = App.TimeSource.UTCNow,
         Channel = "MTV",
          From = "Zhaba",
           Topic = "App",
            Text = "Hello text",
             Source = 12345,
              Parameters = "aaaaa",
               ArchiveDimensions = "a=1 b=2"
      };

      var doc = ser.Serialize( msg );

      Console.WriteLine( doc.ToJSON(JSONWritingOptions.PrettyPrint) );


      Aver.IsTrue( doc.IndexOfName(ser.TypeIDFieldName)>=0);//field was added

      var got = ser.Deserialize(doc) as NFX.Log.Message;

      testMsgEquWoError( msg, got);
    }


    [Test]
    public void SerializeDeserializeLogMessage_KnownTypes()
    {
      var ser = new BSONSerializer();//notice no resolver

      var msg = new NFX.Log.Message
      {
         Type = MessageType.DebugGlue,
         TimeStamp = App.TimeSource.UTCNow,
         Channel = "MTV",
          From = "Zhaba",
           Topic = "App",
            Text = "Hello text",
             Source = 12345,
              Parameters = "aaaaa",
               ArchiveDimensions = "a=1 b=2"
      };

      var doc = ser.Serialize( msg , new BSONParentKnownTypes(typeof(Message)));

      Console.WriteLine( doc.ToJSON(JSONWritingOptions.PrettyPrint) );

      Aver.IsFalse( doc.IndexOfName(ser.TypeIDFieldName)>=0);//field was NOT added as the root type is known

      var got = new NFX.Log.Message();//pre-allocate before deserialize, as __t was not emitted
      Aver.IsNotNull( ser.Deserialize(doc, result: got) as NFX.Log.Message );
      testMsgEquWoError( msg, got);
    }


    [Test]
    public void SerializeDeserializeLogMessage_withException()
    {
      var ser = new BSONSerializer( new BSONTypeResolver(typeof(Message)) );

      var msg = new NFX.Log.Message
      {
         Type = MessageType.DebugGlue,
         TimeStamp = App.TimeSource.UTCNow,
         Channel = "MTV",
          From = "Zhaba",
           Topic = "App",
            Text = "Hello text",
             Source = 12345,
              Parameters = "aaaaa",
               ArchiveDimensions = "a=1 b=2",
                Exception =  WrappedException.ForException( new Exception("It is an error!!!") )
      };

      var doc = ser.Serialize( msg );

      Console.WriteLine( doc.ToJSON(JSONWritingOptions.PrettyPrint) );

      var got = ser.Deserialize(doc) as NFX.Log.Message;

      testMsgEquWoError( msg, got);

      Aver.IsTrue( got.Exception is WrappedException );
      Aver.AreEqual( msg.Exception.Message, got.Exception.Message );
      Aver.AreEqual( ((WrappedException)msg.Exception).Wrapped.Message, ((WrappedException)got.Exception).Wrapped.Message );
    }




    [Test]
    public void SerializeDeserializeLogMessage_withNestedException()
    {
      var ser = new BSONSerializer( new BSONTypeResolver(typeof(Message)) );

      var msg = new NFX.Log.Message
      {
         Type = MessageType.DebugGlue,
         TimeStamp = App.TimeSource.UTCNow,
         Channel = "MTV",
          From = "Zhaba",
           Topic = "App",
            Text = "Hello text in Chinese: 中原千军逐蒋",
             Source = 0,
              Parameters = new string('a', 128000),
               ArchiveDimensions = "a=1 b=2",
                Exception =  WrappedException.ForException( new Exception("It is an error!!!", new Exception("Inside")) )
      };

      var doc = ser.Serialize( msg );

      Console.WriteLine( doc.ToJSON(JSONWritingOptions.PrettyPrint) );

      var got = ser.Deserialize(doc) as NFX.Log.Message;

      testMsgEquWoError( msg, got);

      Aver.IsTrue( got.Exception is WrappedException );
      Aver.AreEqual( msg.Exception.Message, got.Exception.Message );
      Aver.AreEqual( ((WrappedException)msg.Exception).Wrapped.Message, ((WrappedException)got.Exception).Wrapped.Message );
      Aver.IsNotNull( ((WrappedException)msg.Exception).Wrapped.InnerException );

      Aver.AreEqual( ((WrappedException)msg.Exception).Wrapped.InnerException.Message, ((WrappedException)got.Exception).Wrapped.InnerException.Message );
    }







    private void testMsgEquWoError(NFX.Log.Message msg, NFX.Log.Message got)
    {
      Aver.IsNotNull( got );
      Aver.AreNotSameRef( msg, got);

      Aver.AreEqual( msg.Guid, got.Guid );
      Aver.AreEqual( msg.RelatedTo, got.RelatedTo );
      Aver.AreEqual( msg.Host, got.Host );
      Aver.AreEqual( msg.TimeStamp, got.TimeStamp );

      Aver.IsTrue( msg.Type == got.Type );
      Aver.AreEqual( msg.Channel, got.Channel );
      Aver.AreEqual( msg.From, got.From );
      Aver.AreEqual( msg.Topic, got.Topic );
      Aver.AreEqual( msg.Text, got.Text );
      Aver.AreEqual( msg.Source, got.Source );
      Aver.AreEqual( msg.Parameters, got.Parameters );
      Aver.AreEqual( msg.ArchiveDimensions, got.ArchiveDimensions );
    }
  }
}
