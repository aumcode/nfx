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
  /// (i.e. as UNSIGNED BIGINT in SQL datastores)
  /// </summary>
  [Serializable]
  public struct GDID : IDataStoreKey, IComparable<GDID>, IEquatable<GDID>, IComparable, IJSONWritable, IDistributedStableHashProvider
  {
        public const UInt64 AUTHORITY_MASK = 0xf000000000000000;
        public const UInt64 COUNTER_MASK   = 0x0fffffffffffffff;

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
        
        
        public override string  ToString()
        {
 	       return "GDID[{0}:{1}({2},{3})]".Args(Era, ID, Authority, Counter);
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
          wri.Write(ID);
          wri.Write('"');
        }

        public static GDID Parse(string str)
        {
          GDID result;
          if (!TryParse(str, out result))
            throw new DistributedDataAccessException(StringConsts.DISTRIBUTED_DATA_GDID_PARSE_ERROR.Args(str));

          return result;
        }

        public static bool TryParse(string str, out GDID gdid)
        {
          gdid = GDID.Zero;
          if (str.IsNullOrWhiteSpace()) return false;

          string sera, sid;
          var i = str.IndexOf(':');
          if (i==0 || i==str.Length-1) return false;
          if (i<0)
          {
            sera = null;
            sid = str;
          } 
          else
          {
            sera = str.Substring(0, i);
            sid = str.Substring(i+1);
          }

          uint era=0;
          if (sera!=null)
           if (!uint.TryParse(sera, out era)) return false;

          ulong id;
           if (!ulong.TryParse(sid, out id)) return false;

          gdid = new GDID(era, id);
          return true;
        }

  }
}
