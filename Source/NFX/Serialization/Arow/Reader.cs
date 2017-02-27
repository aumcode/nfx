using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using NFX.IO;
using NFX.DataAccess.CRUD;

namespace NFX.Serialization.Arow
{
  /// <summary>
  /// Provides low-level Arow format reading
  /// </summary>
  public static class Reader
  {

    public static readonly Dictionary<Type, string> DESER_TYPE_MAP = new Dictionary<Type, string>
    {
      {typeof(byte?),       null},
      {typeof(byte),        null},
      {typeof(byte[]),@"
         if (dt==DataType.Null) row.{0} = null;
         else if (dt==DataType.ByteArray) row.{0} = streamer.ReadByteArray();
         else if (dt!=DataType.Array) break;
         else
         {
           atp = Reader.ReadDataType(streamer);
           if (atp!=DataType.Byte) break;
           row.{0} = Reader.ReadByteArray(streamer);
         }
         continue;"
      },

      {typeof(List<byte>),@"
         if (dt==DataType.Null) row.{0} = null;
         else if (dt==DataType.ByteArray) row.{0} = new List<byte>(streamer.ReadByteArray());
         else if (dt!=DataType.Array) break;
         else
         {
           atp = Reader.ReadDataType(streamer);
           if (atp!=DataType.Byte) break;
           row.{0} = new List<byte>(Reader.ReadByteArray(streamer));
         }
         continue;"
      },

      //-------------------------------------------------------------------------------------------

      {typeof(bool?),       null},
      {typeof(bool),       null},
      {typeof(bool[]),     null},
      {typeof(List<bool>), null},

      {typeof(sbyte?),       null},
      {typeof(sbyte),       null},
      {typeof(sbyte[]),     null},
      {typeof(List<sbyte>), null},

      {typeof(short?),         null},
      {typeof(short),         null},
      {typeof(short[]),       null},
      {typeof(List<short>),   null},

      {typeof(ushort?),         null},
      {typeof(ushort),         null},
      {typeof(ushort[]),       null},
      {typeof(List<ushort>),   null},

      {typeof(int?),         null},
      {typeof(int),         null},
      {typeof(int[]),       null},
      {typeof(List<int>),   null},

      {typeof(uint?),       null},
      {typeof(uint),       null},
      {typeof(uint[]),     null},
      {typeof(List<uint>), null},

      {typeof(long?),         null},
      {typeof(long),         null},
      {typeof(long[]),       null},
      {typeof(List<long>),   null},

      {typeof(ulong?),       null},
      {typeof(ulong),       null},
      {typeof(ulong[]),     null},
      {typeof(List<ulong>), null},

      {typeof(float?),         null},
      {typeof(float),         null},
      {typeof(float[]),       null},
      {typeof(List<float>),   null},

      {typeof(double?),         null},
      {typeof(double),         null},
      {typeof(double[]),       null},
      {typeof(List<double>),   null},

      {typeof(decimal?),         null},
      {typeof(decimal),         null},
      {typeof(decimal[]),       null},
      {typeof(List<decimal>),   null},

      {typeof(char?),         null},
      {typeof(char),         null},
      {typeof(char[]),       null},
      {typeof(List<char>),   null},

      {typeof(string),         null},
      {typeof(string[]),       null},
      {typeof(List<string>),   null},

      {typeof(Financial.Amount?),         null},
      {typeof(Financial.Amount),         null},
      {typeof(Financial.Amount[]),       null},
      {typeof(List<Financial.Amount>),   null},

      {typeof(DateTime?),       null},
      {typeof(DateTime),       null},
      {typeof(DateTime[]),     null},
      {typeof(List<DateTime>), null},

      {typeof(TimeSpan?),       null},
      {typeof(TimeSpan),       null},
      {typeof(TimeSpan[]),     null},
      {typeof(List<TimeSpan>), null},

      {typeof(Guid?),       null},
      {typeof(Guid),       null},
      {typeof(Guid[]),     null},
      {typeof(List<Guid>), null},

      {typeof(NFX.DataAccess.Distributed.GDID?),       null},
      {typeof(NFX.DataAccess.Distributed.GDID),       null},
      {typeof(NFX.DataAccess.Distributed.GDID[]),     null},
      {typeof(List<NFX.DataAccess.Distributed.GDID>), null},


      {typeof(NFX.FID?),       null},
      {typeof(NFX.FID),       null},
      {typeof(NFX.FID[]),     null},
      {typeof(List<NFX.FID>), null},

      {typeof(NFX.ApplicationModel.Pile.PilePointer?),       null},
      {typeof(NFX.ApplicationModel.Pile.PilePointer),       null},
      {typeof(NFX.ApplicationModel.Pile.PilePointer[]),     null},
      {typeof(List<NFX.ApplicationModel.Pile.PilePointer>), null},

      {typeof(NFX.Serialization.JSON.NLSMap?),       null},
      {typeof(NFX.Serialization.JSON.NLSMap),       null},
      {typeof(NFX.Serialization.JSON.NLSMap[]),     null},
      {typeof(List<NFX.Serialization.JSON.NLSMap>), null},
    };



    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void ReadHeader(ReadingStreamer streamer)
    {
      if (streamer.ReadByte()==0xC0 && streamer.ReadByte()==0xFE) return;

      throw new ArowException(StringConsts.AROW_HEADER_CORRUPT_ERROR);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static ulong ReadName(ReadingStreamer streamer)
    {
      return streamer.ReadULong();
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static DataType ReadDataType(ReadingStreamer streamer)
    {
      return (DataType)streamer.ReadByte();
    }


    public static TRow[] ReadRowArray<TRow>(TypedRow rowScope, ReadingStreamer streamer, string name) where TRow : TypedRow, new()
    {
       var len = Reader.ReadArrayLength(streamer);
       var arr = new TRow[len];
       for(var i=0; i<len; i++)
       {
         var has = streamer.ReadBool();
         if (!has) continue;
         var vrow = new TRow();
         if (Reader.TryReadRow(rowScope, vrow, streamer, name+'_'+i.ToString()))
           arr[i] = vrow;
       }
       return arr;
    }

    public static List<TRow> ReadRowList<TRow>(TypedRow rowScope, ReadingStreamer streamer, string name) where TRow : TypedRow, new()
    {
       var len = Reader.ReadArrayLength(streamer);
       var lst = new List<TRow>(len);
       for(var i=0; i<len; i++)
       {
         var has = streamer.ReadBool();
         if (!has)
         {
           lst.Add(null);
           continue;
         }
         var vrow = new TRow();
         if (Reader.TryReadRow(rowScope, vrow, streamer, name+'_'+i.ToString()))
           lst.Add( vrow );
       }
       return lst;
    }


    public static bool TryReadRow(TypedRow rowScope, TypedRow newRow, ReadingStreamer streamer, string name)
    {
       var ok = ArowSerializer.TryDeserialize(newRow, streamer, false);
       if (ok) return true;

       var map = readRowAsMap(streamer);//unconditionaly to advance stream

       var arow = rowScope as IAmorphousData;
       if (arow==null) return false;
       if (!arow.AmorphousDataEnabled) return false;
       arow.AmorphousData[name] = map;
       return false;
    }

    public static object ConsumeUnmatched(TypedRow row, ReadingStreamer streamer, string name, DataType dt, DataType? atp)
    {
       object value = null;

       if (atp.HasValue)
       {
         var len = ReadArrayLength(streamer);
         var arr = new object[len];
         for(var i=0; i<arr.Length; i++)
          arr[i] = readOneAsObject(streamer, dt);
         value = arr;
       }
       else
       {
         value = readOneAsObject(streamer, dt);
       }

       var arow = row as IAmorphousData;
       if (arow==null) return value;
       if (!arow.AmorphousDataEnabled) return value;
       arow.AmorphousData[name] = value;
       return value;
    }

    private static JSON.JSONDataMap readRowAsMap(ReadingStreamer streamer)
    {
      var result = new JSON.JSONDataMap();
      while(true)
      {
        var name = ReadName(streamer);
        if (name==0) return result;
        var dt = ReadDataType(streamer);
        DataType? atp = null;
        if (dt==DataType.Array)
         atp = ReadDataType(streamer);
        var val = ConsumeUnmatched(null, streamer, null, dt, atp);
        result[CodeGenerator.GetName(name)] = val;
      }
    }

    private static object readOneAsObject(ReadingStreamer streamer, DataType dt)
    {
      switch(dt)
      {
        case DataType.Null: return null;
        case DataType.Row:  return readRowAsMap(streamer);

        case DataType.Boolean     :  return ReadBoolean     (streamer);
        case DataType.Char        :  return ReadChar        (streamer);
        case DataType.String      :  return ReadString      (streamer);
        case DataType.Single      :  return ReadSingle      (streamer);
        case DataType.Double      :  return ReadDouble      (streamer);
        case DataType.Decimal     :  return ReadDecimal     (streamer);
        case DataType.Amount      :  return ReadAmount      (streamer);
        case DataType.Byte        :  return ReadByte        (streamer);
        case DataType.ByteArray   :  return streamer.ReadByteArray();
        case DataType.SByte       :  return ReadSByte       (streamer);
        case DataType.Int16       :  return ReadInt16       (streamer);
        case DataType.Int32       :  return ReadInt32       (streamer);
        case DataType.Int64       :  return ReadInt64       (streamer);
        case DataType.UInt16      :  return ReadUInt16      (streamer);
        case DataType.UInt32      :  return ReadUInt32      (streamer);
        case DataType.UInt64      :  return ReadUInt64      (streamer);
        case DataType.DateTime    :  return ReadDateTime    (streamer);
        case DataType.TimeSpan    :  return ReadTimeSpan    (streamer);
        case DataType.Guid        :  return ReadGuid        (streamer);
        case DataType.GDID        :  return ReadGDID        (streamer);
        case DataType.FID         :  return ReadFID         (streamer);
        case DataType.PilePointer :  return ReadPilePointer (streamer);
        case DataType.NLSMap      :  return ReadNLSMap      (streamer);
        default: throw new ArowException(StringConsts.AROW_DESER_CORRUPT_ERROR);
      }
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static int ReadArrayLength(ReadingStreamer streamer)
    {
      var len = streamer.ReadInt();
      if (len > Writer.MAX_ARRAY_LENGTH) throw new ArowException(StringConsts.AROW_MAX_ARRAY_LEN_ERROR.Args(len));
      return len;
    }



    public static Boolean                                ReadBoolean      (ReadingStreamer streamer){ return streamer.ReadBool(); }
    public static Char                                   ReadChar         (ReadingStreamer streamer){ return streamer.ReadChar(); }
    public static String                                 ReadString       (ReadingStreamer streamer){ return streamer.ReadString (); }
    public static Single                                 ReadSingle       (ReadingStreamer streamer){ return streamer.ReadFloat  (); }
    public static Double                                 ReadDouble       (ReadingStreamer streamer){ return streamer.ReadDouble (); }
    public static Decimal                                ReadDecimal      (ReadingStreamer streamer){ return streamer.ReadDecimal(); }
    public static NFX.Financial.Amount                   ReadAmount       (ReadingStreamer streamer){ return streamer.ReadAmount(); }
    public static Byte                                   ReadByte         (ReadingStreamer streamer){ return streamer.ReadByte     (); }
    public static SByte                                  ReadSByte        (ReadingStreamer streamer){ return streamer.ReadSByte    (); }
    public static Int16                                  ReadInt16        (ReadingStreamer streamer){ return streamer.ReadShort    (); }
    public static Int32                                  ReadInt32        (ReadingStreamer streamer){ return streamer.ReadInt      (); }
    public static Int64                                  ReadInt64        (ReadingStreamer streamer){ return streamer.ReadLong     (); }
    public static UInt16                                 ReadUInt16       (ReadingStreamer streamer){ return streamer.ReadUShort   (); }
    public static UInt32                                 ReadUInt32       (ReadingStreamer streamer){ return streamer.ReadUInt     (); }
    public static UInt64                                 ReadUInt64       (ReadingStreamer streamer){ return streamer.ReadULong    (); }
    public static DateTime                               ReadDateTime     (ReadingStreamer streamer){ return streamer.ReadDateTime (); }
    public static TimeSpan                               ReadTimeSpan     (ReadingStreamer streamer){ return streamer.ReadTimeSpan (); }
    public static Guid                                   ReadGuid         (ReadingStreamer streamer){ return streamer.ReadGuid     (); }
    public static NFX.DataAccess.Distributed.GDID        ReadGDID         (ReadingStreamer streamer){ return streamer.ReadGDID(); }
    public static FID                                    ReadFID          (ReadingStreamer streamer){ return streamer.ReadFID();  }
    public static NFX.ApplicationModel.Pile.PilePointer  ReadPilePointer  (ReadingStreamer streamer){ return streamer.ReadPilePointer(); }
    public static NFX.Serialization.JSON.NLSMap          ReadNLSMap       (ReadingStreamer streamer){ return streamer.ReadNLSMap(); }


    public static byte[] ReadByteArray(ReadingStreamer streamer)
    {
      var len = ReadArrayLength(streamer);
      var arr = new byte[len];
      for(var i=0; i<len; i++)
       arr[i] = streamer.ReadByte();
      return arr;
    }



  }
}
