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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Serialization.JSON;

namespace NFX.DataAccess.Distributed
{
  /// <summary>
  /// Represents a Global Distributed ID key (key field) used in distributed databases that identify entities with a combination of unsigned 32 bit integer
  ///  'Era' and unsigned 64 bit integer 'ID'. The first 32 bit integer is an 'era' in which the 'ID' (64 bit) was created, consequently
  /// a GDID is a 12 byte = 96 bit integer that can hold 2^96 = 79,228,162,514,264,337,593,543,950,336 combinations.
  /// The ID consists of two segments: 4 bit authority + 60 bits counter. Authority segment occupies the most significant 4 bits of uint64, so
  ///  the system may efficiently query the data store to identify the highest stored ID value in a range.
  /// Authorities identify one of 16 possible ID generation sources in the global distributed system, therefore ID duplications are not
  /// possible between authorities.
  /// Within a single era, GDID structure may identify 2^60 = 1,152,921,504,606,846,976(per authority) * 16(authorities) = 2^64 = 18,446,744,073,709,551,616 total combinations.
  /// Because of such a large number of combinations supported by GDID.ID alone (having the same Era), some systems may always use Era=0 and only store the ID part
  /// (i.e. as UNSIGNED BIGINT in SQL datastores).
  /// Note GDID.Zero is never returned by generators as it represents the absence of a value
  /// </summary>
  [Serializable]
  public struct GDID : IDataStoreKey, IComparable<GDID>, IEquatable<GDID>, IComparable, IJSONWritable, IDistributedStableHashProvider
  {
        public const UInt64 AUTHORITY_MASK = 0xf000000000000000;
        public const UInt64 COUNTER_MASK   = 0x0fffffffffffffff;//0x 0f  ff  ff  ff  ff  ff  ff  ff
                                                                //    1   2   3   4   5   6   7   8

        /// <summary>
        /// Provides maximum value for counter segment
        /// </summary>
        public const UInt64 COUNTER_MAX    = COUNTER_MASK;

        /// <summary>
        /// Provides maximum value for authority segment
        /// </summary>
        public const int AUTHORITY_MAX    = 0x0f;

        /// <summary>
        /// Zero GDID singleton
        /// </summary>
        public static readonly GDID Zero = new GDID(0, 0ul);

        public readonly UInt32 Era;
        public readonly UInt64 ID;

        public GDID(UInt32 era, UInt64 id)
        {
          Era = era;
          ID = id;
        }

        public GDID(uint era, int authority, UInt64 counter)
        {
          if (authority>AUTHORITY_MAX || counter>COUNTER_MAX)
            throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_GDID_CTOR_ERROR.Args(authority, AUTHORITY_MAX, counter, COUNTER_MAX));

          Era = era;
          ID = (((UInt64)authority)<<60) | (counter & COUNTER_MASK);
        }

        public GDID(byte[] bytes, int startIdx = 0)
        {
          if (bytes==null || startIdx <0 || (bytes.Length-startIdx)<sizeof(uint)+sizeof(ulong))
            throw new DistributedDataAccessException(StringConsts.ARGUMENT_ERROR+"GDID.ctor(bytes==null<minsz)");

          Era = bytes.ReadBEUInt32();
          ID =  bytes.ReadBEUInt64(4);
        }


        /// <summary>
        /// Returns the 0..15 index of the authority that issued this ID
        /// </summary>
        public int Authority
        {
          get { return (int)( (ID & AUTHORITY_MASK) >> 60 );  }
        }

        /// <summary>
        /// Returns the 60 bits of counter segment of this id (without athority segment upper 4 bits)
        /// </summary>
        public UInt64 Counter
        {
          get { return ID & COUNTER_MASK; }
        }

        /// <summary>
        /// Returns the GDID buffer as BigEndian Era:ID tuple
        /// </summary>
        public byte[] Bytes
        {
          get
          {
            var result = new byte[sizeof(uint)+sizeof(ulong)];
            result.WriteBEUInt32(0, Era);
            result.WriteBEUInt64(4, ID);
            return result;
          }
        }

        /// <summary>
        /// True is this instance is invalid - represents 0:0:0
        /// </summary>
        public bool IsZero
        {
          get{ return Era==0 && ID==0;}
        }

        public override string  ToString()
        {
          return Era.ToString() + ":" + Authority.ToString() + ":" + Counter.ToString();
        }

        public override int GetHashCode()
        {
 	        return (int)Era ^ (int)ID ^ (int)(ID >> 32);
        }

        public ulong GetDistributedStableHash()
        {
          return  ((ulong)Era << 32) ^ ID;
        }

        public override bool  Equals(object obj)
        {
 	        if(obj==null || !(obj is GDID)) return false;
          return this.Equals(((GDID)obj));
        }

        public int CompareTo(GDID other)
        {
          var result = this.Era.CompareTo(other.Era);
          if (result!=0) return result;
          return this.ID.CompareTo(other.ID);
        }

        public int CompareTo(object obj)
        {
          if (obj==null) return -1;
          return this.CompareTo((GDID)obj);
        }

        public bool Equals(GDID other)
        {
          return (this.Era == other.Era) && (this.ID == other.ID);
        }

        public static bool operator ==(GDID x, GDID y)
        {
          return x.Equals(y);
        }

        public static bool operator !=(GDID x, GDID y)
        {
          return !x.Equals(y);
        }

        public static bool operator <(GDID x, GDID y)
        {
          return x.CompareTo(y) < 0;
        }

        public static bool operator >(GDID x, GDID y)
        {
          return x.CompareTo(y) > 0;
        }

         public static bool operator <=(GDID x, GDID y)
        {
          return x.CompareTo(y)<=0;
        }

        public static bool operator >=(GDID x, GDID y)
        {
          return x.CompareTo(y)>=0;
        }

        public void WriteAsJSON(System.IO.TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
        {
          wri.Write('"');
          wri.Write(Era);
          wri.Write(':');
          wri.Write(Authority);
          wri.Write(':');
          wri.Write(Counter);
          wri.Write('"');
        }

        public static GDID Parse(string str)
        {
          GDID result;
          if (!TryParse(str, out result))
            throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_GDID_PARSE_ERROR.Args(str));

          return result;
        }

        private static int hexDigit(char c)
        {
          var d = c - '0';
          if (d>=0 && d<=9) return d;

          d = c - 'A';
          if (d>=0 && d<=5) return 10 + d;

          d = c - 'a';
          if (d>=0 && d<=5) return 10 + d;

          return -1;
        }


        public static bool TryParse(string str, out GDID? gdid)
        {
          GDID parsed;
          if (TryParse(str, out parsed))
          {
            gdid = parsed;
            return true;
          }

          gdid = null;
          return false;
        }

        public static bool TryParse(string str, out GDID gdid)
        {
          gdid = GDID.Zero;

          var ix = str.IndexOf("0x", StringComparison.OrdinalIgnoreCase);
          if (ix>-1)//HEX format
          {
            ix+=2;//skip 0x
            var buf = new byte[sizeof(uint) + sizeof(ulong)];
            var j = 0;
            for(var i=ix; i<str.Length;)
            {
              var dh = hexDigit(str[i]); i++;
              if (dh<0 || i==str.Length) return false;
              var dl = hexDigit(str[i]); i++;
              if (dl<0) return false;

              if (j==buf.Length) return false;
              buf[j] = (byte)((dh << 4) + dl);
              j++;
            }
            if (j<buf.Length) return false;

            gdid = new GDID(buf);
            return true;
          }//HEX format

          //regular Era:Auth:Counter format
          const int MIN_LEN = 5;// "0:0:0"
          if (str.IsNullOrWhiteSpace() || str.Length<MIN_LEN) return false;

          string sera, sau, sctr;
          var i1 = str.IndexOf(':', 0);
          if (i1<=0 || i1==str.Length-1) return false;

          sera = str.Substring(0, i1);

          var i2 = str.IndexOf(':', i1+1);
          if (i2<0 || i2==str.Length-1 || i2==i1+1) return false;

          sau = str.Substring(i1+1, i2-i1-1);

          sctr = str.Substring(i2+1);



          uint era=0;
          if (!uint.TryParse(sera, out era)) return false;

          byte au=0;
          if (!byte.TryParse(sau, out au)) return false;

          ulong ctr;
          if (!ulong.TryParse(sctr, out ctr)) return false;

          if (au>AUTHORITY_MAX || ctr>COUNTER_MAX) return false;

          gdid = new GDID(era, au, ctr);
          return true;
        }

  }


  /// <summary>
  /// Represents a tuple of GDID and its symbolic representation (framework usually uses an ELink as symbolic representation).
  /// This struct is needed to pass GDID along with its ELink representation together.
  /// Keep in mind that string poses a GC load, so this stuct is not suitable for beiing used as a pile cache key
  /// </summary>
  public struct GDIDSymbol : IEquatable<GDIDSymbol>
  {
     public GDIDSymbol(GDID gdid, string symbol)
     {
       GDID = gdid;
       Symbol = symbol;
     }

     public readonly GDID GDID;
     public readonly string Symbol;

     public bool IsZero{ get{ return GDID.IsZero;}}

     public override string ToString()
     {
       return "{0}::'{1}'".Args(GDID, Symbol);
     }

     public override int GetHashCode()
     {
       return GDID.GetHashCode() ^ (Symbol!=null ? Symbol.GetHashCode() : 0);
     }

     public override bool Equals(object obj)
     {
       if (!(obj is GDIDSymbol)) return false;
       return this.Equals((GDIDSymbol)obj);
     }

     public bool Equals(GDIDSymbol other)
     {
       return this.GDID.Equals(other.GDID) && this.Symbol.EqualsOrdSenseCase(other.Symbol);
     }
  }


  /// <summary>
  /// Compares GDID regardless of authority. This is useful for range checking, when authorities generating GDIDs in the same
  ///  range should be disregarded. Use GDIDRangeComparer.Instance.
  ///  Only relative range comparison can be made.
  ///  The Equality returned by this comparer can not be relied upon for GDID comparison as it disregards authority.
  ///  Equality can only be tested for range comparison.
  /// </summary>
  public class GDIDRangeComparer : IComparer<GDID>
  {
    public static readonly GDIDRangeComparer Instance = new GDIDRangeComparer();

    private GDIDRangeComparer() {}


    public int Compare(GDID x, GDID y)
    {
      var result = x.Era.CompareTo(y.Era);
      if (result!=0) return result;
      return x.Counter.CompareTo(y.Counter);//Authority is disregarded
    }
  }


}
