using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.DataAccess.Distributed;

namespace NFX.ApplicationModel.Pile
{
  /// <summary>
  /// Represents a value-type GDID+3 char ISO code tuple, suitable for serving as a cache table key.
  /// The point of this structure is to avoid creation of references for Pile cache so keys alone do not stall the GC.
  /// The ISO code is case-insensitive
  /// </summary>
  public struct GDIDWithISOKey : IDistributedStableHashProvider, IEquatable<GDIDWithISOKey>
  {
    internal static int ISOtoInt(string iso)
    {
      if (iso==null)
       throw new NFXException(StringConsts.ARGUMENT_ERROR+"GDIDWithISOKey(iso==null)");
      
      var l = iso.Length;
      
      if (l==0 || l>3)
        throw new NFXException(StringConsts.ARGUMENT_ERROR+"GDIDWithISOKey(iso==0|>3)");


      //note: ISO codes are in ASCII plane
      var isoChar0 = (byte)iso[0];
      var isoChar1 = l>1 ? (byte)iso[1] : (byte)0x00;
      var isoChar2 = l>2 ? (byte)iso[2] : (byte)0x00;

      if (isoChar0>0x60) isoChar0 -= 0x20;//convert to upper case
      if (isoChar1>0x60) isoChar1 -= 0x20;//convert to upper case
      if (isoChar2>0x60) isoChar2 -= 0x20;//convert to upper case

      var result = (isoChar2 << 16) + (isoChar1 << 8 ) + isoChar0;
      return result;
    }

    internal static string INTtoISO(int iso)
    {
      var result = string.Empty;

        var c =(char)(iso & 0xff);
        if (c!=0) result += c;

        c = (char)((iso>>8) & 0xff);
        if (c!=0) result += c;
        
        c = (char)((iso>>16) & 0xff);
        if (c!=0) result += c;
         
        return result;
    }


    public GDIDWithISOKey(GDID gdid, string iso)
    {
      GDID = gdid;
      ISO = ISOtoInt(iso);
    }

    public readonly GDID GDID;
    public readonly int ISO;


    public string ISOCode { get{ return INTtoISO(ISO);} }


    public ulong GetDistributedStableHash()
    {
      return GDID.GetDistributedStableHash() ^ (ulong)ISO;
    }

    public override int GetHashCode()
    {
      return GDID.GetHashCode() ^ ISO;
    }

    public bool Equals(GDIDWithISOKey other)
    {
      return this.ISO  == other.ISO &&
             this.GDID == other.GDID;
    }

    public override bool Equals(object obj)
    {
      if (!(obj is GDIDWithISOKey)) return false;
      return this.Equals((GDIDWithISOKey)obj);
    }

    public override string ToString()
    {
      return "Key[GDID: '{0}' ISO: '{1}']".Args(GDID, ISOCode);
    }
  }


  /// <summary>
  /// Represents a date (not time)-sensitive value-type GDID+3 char ISO code tuple, suitable for serving as a cache table key.
  /// The point of this structure is to avoid creation of references for Pile cache so keys alone do not stall the GC.
  /// The ISO code is case-insensitive
  /// </summary>
  public struct DatedGDIDWithISOKey : IDistributedStableHashProvider, IEquatable<DatedGDIDWithISOKey>
  {
    
    internal static int DateToYMD(DateTime dt)
    {
      return ((dt.Year  & 0xffff) << 16) +
             ((dt.Month & 0xff)  << 8) +
             ((dt.Day   & 0xff));
    }

    internal static DateTime YMDtoDate(int ymd)
    {
      var result = new DateTime(((ymd >> 16) & 0xffff), ((ymd >> 8) & 0xff), (ymd & 0xff), 0, 0, 0, DateTimeKind.Utc);
      return result;
    }
    
    
    public DatedGDIDWithISOKey(DateTime date, GDID gdid, string iso)
    {
      GDID = gdid;
      ISO = GDIDWithISOKey.ISOtoInt(iso);
      YMD = DatedGDIDWithISOKey.DateToYMD(date);
    }

    public readonly int  YMD;
    public readonly GDID GDID;
    public readonly int  ISO;


    public string ISOCode { get{ return GDIDWithISOKey.INTtoISO(ISO);} }

    public DateTime DateTime { get{ return YMDtoDate(YMD);} }


    public ulong GetDistributedStableHash()
    {
      return GDID.GetDistributedStableHash() ^ (((ulong)ISO)<<32) ^ (ulong)YMD;
    }

    public override int GetHashCode()
    {
      return GDID.GetHashCode() ^ ISO ^ YMD;
    }

    public bool Equals(DatedGDIDWithISOKey other)
    {
      return this.YMD  == other.YMD &&
             this.ISO  == other.ISO &&
             this.GDID == other.GDID;
    }

    public override bool Equals(object obj)
    {
      if (!(obj is DatedGDIDWithISOKey)) return false;
      return this.Equals((DatedGDIDWithISOKey)obj);
    }

    public override string ToString()
    {
      return "Key[GDID: '{0}' ISO: '{1}' Date: '{2:yyyy-MM-dd}']".Args(GDID, ISOCode, DateTime);
    }
  }


  /// <summary>
  /// Represents a date (not time)-sensitive value-type 2 GDIDs+3 char ISO code tuple, suitable for serving as a cache table key.
  /// The point of this structure is to avoid creation of references for Pile cache so keys alone do not stall the GC.
  /// The ISO code is case-insensitive
  /// </summary>
  public struct Dated2GDIDWithISOKey : IDistributedStableHashProvider, IEquatable<Dated2GDIDWithISOKey>
  {
    
    public Dated2GDIDWithISOKey(DateTime date, GDID gdid1, GDID gdid2, string iso)
    {
      GDID1 = gdid1;
      GDID2 = gdid2;
      ISO = GDIDWithISOKey.ISOtoInt(iso);
      YMD = DatedGDIDWithISOKey.DateToYMD(date);
    }

    public readonly int  YMD;
    public readonly GDID GDID1;
    public readonly GDID GDID2;
    public readonly int  ISO;


    public string ISOCode { get{ return GDIDWithISOKey.INTtoISO(ISO);} }

    public DateTime DateTime { get{ return DatedGDIDWithISOKey.YMDtoDate(YMD);} }


    public ulong GetDistributedStableHash()
    {
      return GDID1.GetDistributedStableHash() ^ 
             GDID2.GetDistributedStableHash() ^ 
             (((ulong)ISO)<<32) ^ (ulong)YMD;
    }

    public override int GetHashCode()
    {
      return GDID1.GetHashCode() ^ GDID2.GetHashCode() ^ ISO ^ YMD;
    }

    public bool Equals(Dated2GDIDWithISOKey other)
    {
      return this.YMD  == other.YMD &&
             this.ISO  == other.ISO &&
             this.GDID1 == other.GDID1 &&
             this.GDID2 == other.GDID2 ;
    }

    public override bool Equals(object obj)
    {
      if (!(obj is Dated2GDIDWithISOKey)) return false;
      return this.Equals((Dated2GDIDWithISOKey)obj);
    }

    public override string ToString()
    {
      return "Key[GDID1: '{0}' GDID2: '{1}' ISO: '{2}' Date: '{3:yyyy-MM-dd}']".Args(GDID1, GDID2, ISOCode, DateTime);
    }
  }


}
