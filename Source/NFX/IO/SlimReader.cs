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
using System.IO;
using System.Reflection;

using NFX.Inventorization;
using NFX.Serialization.JSON;

namespace NFX.IO
{
    /// <summary>
    /// Reads primitives and other supported types from Slim-format stream. Use factory method of SlimFormat intance to create a new instance of SlimReader class
    /// </summary>
    [Inventory(Concerns=SystemConcerns.Testing | SystemConcerns.MissionCriticality)]
    public class SlimReader : ReadingStreamer
    {
        #region .ctor

            protected internal SlimReader(Encoding encoding=null) : base(encoding)
            {
            }

        #endregion

        #region Fields


        #endregion


        #region Properties

            /// <summary>
            /// Returns SlimFormat that this reader implements
            /// </summary>
            public override StreamerFormat Format
            {
                get { return SlimFormat.Instance; }
            }


        #endregion


        #region Public







          public override bool ReadBool()
          {
            var b = m_Stream.ReadByte();
            if (b<0) throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadBool(): eof");

            return b!=0;
          }

              public override bool? ReadNullableBool()
              {
                var has = this.ReadBool();

                if (has) return this.ReadBool();

                return null;
              }


          public override byte ReadByte()
          {
            var b = m_Stream.ReadByte();
            if (b<0) throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadByte(): eof");

            return (byte)b;
          }

              public override byte? ReadNullableByte()
              {
                var has = this.ReadBool();

                if (has) return this.ReadByte();

                return null;
              }


          public override byte[] ReadByteArray()
          {
            var has = this.ReadBool();
            if (!has) return null;

            var len = this.ReadInt();
            if (len>SlimFormat.MAX_BYTE_ARRAY_LEN)
              throw new NFXIOException(StringConsts.SLIM_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "bytes", SlimFormat.MAX_BYTE_ARRAY_LEN));

            var buf = new byte[len];

            ReadFromStream(buf, len);

            return buf;
          }



          public override int[] ReadIntArray()
          {
            var has = this.ReadBool();
            if (!has) return null;

            var len = this.ReadInt();
            if (len>SlimFormat.MAX_INT_ARRAY_LEN)
              throw new NFXIOException(StringConsts.SLIM_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "ints", SlimFormat.MAX_INT_ARRAY_LEN));

            var result = new int[len];


            for(int i=0; i<len; i++)
              result[i] = this.ReadInt();

            return result;
          }


          public override long[] ReadLongArray()
          {
            var has = this.ReadBool();
            if (!has) return null;

            var len = this.ReadInt();
            if (len>SlimFormat.MAX_LONG_ARRAY_LEN)
              throw new NFXIOException(StringConsts.SLIM_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "longs", SlimFormat.MAX_LONG_ARRAY_LEN));

            var result = new long[len];


            for(int i=0; i<len; i++)
              result[i] = this.ReadLong();

            return result;
          }


          public override double[] ReadDoubleArray()
          {
            var has = this.ReadBool();
            if (!has) return null;

            var len = this.ReadInt();
            if (len>SlimFormat.MAX_DOUBLE_ARRAY_LEN)
              throw new NFXIOException(StringConsts.SLIM_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "doubles", SlimFormat.MAX_DOUBLE_ARRAY_LEN));

            var result = new double[len];


            for(int i=0; i<len; i++)
              result[i] = this.ReadDouble();

            return result;
          }





          public override char ReadChar()
          {
            return (char)this.ReadShort();
          }
              public override char? ReadNullableChar()
              {
                var has = this.ReadBool();

                if (has) return this.ReadChar();

                return null;
              }



          public override char[] ReadCharArray()
          {
            byte[] buf = this.ReadByteArray();
            if (buf==null) return null;

            return m_Encoding.GetChars(buf);
          }


          public override string[] ReadStringArray()
          {
            var has = this.ReadBool();
            if (!has) return null;
            var len = this.ReadInt();

            if (len>SlimFormat.MAX_STRING_ARRAY_CNT)
              throw new NFXIOException(StringConsts.SLIM_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "strings", SlimFormat.MAX_STRING_ARRAY_CNT));


            var result = new string[len];

            for(int i=0; i<len; i++)
             result[i] = this.ReadString();

            return result;
          }

          //DKh 20160418
          //public override decimal ReadDecimal()
          //{
          //  var bits = new int[4];
          //  bits[0] = this.ReadInt();
          //  bits[1] = this.ReadInt();
          //  bits[2] = this.ReadInt();
          //  bits[3] = this.ReadInt();
          //  return new Decimal(bits);
          //}

          public override decimal ReadDecimal()
          {
            var bits_0 = this.ReadInt();
            var bits_1 = this.ReadInt();
            var bits_2 = this.ReadInt();
            var bits_3 = this.ReadByte();
            return new Decimal(bits_0,
                               bits_1,
                               bits_2,
                               (bits_3 & 0x80) != 0,
                               (byte)(bits_3 & 0x7F));
          }

              public override decimal? ReadNullableDecimal()
              {
                var has = this.ReadBool();

                if (has) return this.ReadDecimal();

                return null;
              }


          public unsafe override double ReadDouble()
          {
            ReadFromStream(m_Buff32, 8);

            uint seg1 = (uint)((int)m_Buff32[0] |
                               (int)m_Buff32[1] << 8 |
                               (int)m_Buff32[2] << 16 |
                               (int)m_Buff32[3] << 24);

	          uint seg2 = (uint)((int)m_Buff32[4] |
                               (int)m_Buff32[5] << 8 |
                               (int)m_Buff32[6] << 16 |
                               (int)m_Buff32[7] << 24);

	          ulong core = (ulong)seg2 << 32 | (ulong)seg1;

	          return *(double*)(&core);
          }

              public override double? ReadNullableDouble()
              {
                var has = this.ReadBool();

                if (has) return this.ReadDouble();

                return null;
              }


          public unsafe override float ReadFloat()
          {
            ReadFromStream(m_Buff32, 4);

            uint core = (uint)((int)m_Buff32[0] |
                               (int)m_Buff32[1] << 8 |
                               (int)m_Buff32[2] << 16 |
                               (int)m_Buff32[3] << 24);
	          return *(float*)(&core);
          }
              public override float? ReadNullableFloat()
              {
                var has = this.ReadBool();

                if (has) return this.ReadFloat();

                return null;
              }


          public override int ReadInt()
          {
            int result = 0;
            var b = m_Stream.ReadByte();
            if (b<0) throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadInt(): eof");

            var neg = ((b & 1) != 0);


            var has = (b & 0x80) > 0;
            result |= ((b & 0x7f) >> 1);
            var bitcnt = 6;

            while(has)
            {
               if (bitcnt>31)
                throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadInt()");

               b = m_Stream.ReadByte();
               if (b<0) throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadInt(): eof");
               has = (b & 0x80) > 0;
               result |= (b & 0x7f) << bitcnt;
               bitcnt += 7;
            }

            return neg ? ~result : result;
          }


              public override int? ReadNullableInt()
              {
                var has = this.ReadBool();

                if (has) return this.ReadInt();

                return null;
              }

          public override long ReadLong()
          {
            long result = 0;
            var b = m_Stream.ReadByte();
            if (b<0) throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadLong(): eof");

            var neg = ((b & 1) != 0);


            var has = (b & 0x80) > 0;
            result |= ((long)(b & 0x7f) >> 1);
            var bitcnt = 6;

            while(has)
            {
               if (bitcnt>63)
                throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadLong()");

               b = m_Stream.ReadByte();
               if (b<0) throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadLong(): eof");
               has = (b & 0x80) > 0;
               result |= (long)(b & 0x7f) << bitcnt;
               bitcnt += 7;
            }

            return neg ? ~result : result;
          }


              public override long? ReadNullableLong()
              {
                var has = this.ReadBool();

                if (has) return this.ReadLong();

                return null;
              }


          public override sbyte ReadSByte()
          {
            var b = m_Stream.ReadByte();
            if (b<0) throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadSByte(): eof");
            return (sbyte)b;
          }
              public override sbyte? ReadNullableSByte()
              {
                var has = this.ReadBool();

                if (has) return this.ReadSByte();

                return null;
              }


          public override short ReadShort()
          {
            short result = 0;
            var b = m_Stream.ReadByte();
            if (b<0) throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadShort(): eof");

            var neg = ((b & 1) != 0);


            var has = (b & 0x80) > 0;
            result |= (short)((b & 0x7f) >> 1);
            var bitcnt = 6;

            while(has)
            {
               if (bitcnt>15)
                throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadShort()");

               b = m_Stream.ReadByte();
               if (b<0) throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadShort(): eof");
               has = (b & 0x80) > 0;
               result |= (short)((b & 0x7f) << bitcnt);
               bitcnt += 7;
            }

            return (short)(neg ? ~result : result);
          }
              public override short? ReadNullableShort()
              {
                var has = this.ReadBool();

                if (has) return ReadShort();

                return null;
              }

          private const int STR_BUF_SZ = 32 * 1024;

          [ThreadStatic]
          private static byte[] ts_StrBuff;


          public override string ReadString()
          {
            //20150626 DKh
            //var buf = this.ReadByteArray();
            //if (buf==null) return null;

            //return m_Encoding.GetString(buf);

            var has = this.ReadBool();
            if (!has) return null;

            var bsz = this.ReadInt();
            if (bsz<STR_BUF_SZ)
            {
              if (ts_StrBuff==null) ts_StrBuff = new byte[STR_BUF_SZ];
              ReadFromStream(ts_StrBuff, bsz);
              return m_Encoding.GetString(ts_StrBuff, 0, bsz);
            }


            if (bsz>SlimFormat.MAX_BYTE_ARRAY_LEN)
              throw new NFXIOException(StringConsts.SLIM_READ_X_ARRAY_MAX_SIZE_ERROR.Args(bsz, "string bytes", SlimFormat.MAX_BYTE_ARRAY_LEN));

            var buf = new byte[bsz];

            ReadFromStream(buf, bsz);

            return m_Encoding.GetString(buf);
          }




          public override uint ReadUInt()
          {
            uint result = 0;
            var bitcnt = 0;
            var has = true;

            while(has)
            {
               if (bitcnt>31)
                throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadUInt()");

               var b = m_Stream.ReadByte();
               if (b<0) throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadUInt(): eof");
               has = (b & 0x80) != 0;
               result |= (uint)(b & 0x7f) << bitcnt;
               bitcnt += 7;
            }

            return result;
          }

              public override uint? ReadNullableUInt()
              {
                var has = this.ReadBool();

                if (has) return this.ReadUInt();

                return null;
              }



          public override ulong ReadULong()
          {
            ulong result = 0;
            var bitcnt = 0;
            var has = true;

            while(has)
            {
               if (bitcnt>63)
                throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadULong()");

               var b = m_Stream.ReadByte();
               if (b<0) throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadULong(): eof");
               has = (b & 0x80) > 0;
               result |= (ulong)(b & 0x7f) << bitcnt;
               bitcnt += 7;
            }

            return result;
          }

              public override ulong? ReadNullableULong()
              {
                var has = this.ReadBool();

                if (has) return this.ReadULong();

                return null;
              }

          public override ushort ReadUShort()
          {
            ushort result = 0;
            var bitcnt = 0;
            var has = true;

            while(has)
            {
               if (bitcnt>31)
                throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadUShort()");

               var b = m_Stream.ReadByte();
               if (b<0) throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadUShort(): eof");
               has = (b & 0x80) > 0;
               result |= (ushort)((b & 0x7f) << bitcnt);
               bitcnt += 7;
            }

            return result;
          }

              public override ushort? ReadNullableUShort()
              {
                var has = this.ReadBool();

                if (has) return this.ReadUShort();

                return null;
              }


          public override MetaHandle ReadMetaHandle()
          {
            uint handle = 0;
            var b = m_Stream.ReadByte();
            if (b<0) throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadMetaHandle(): eof");

            var meta = ((b & 1) != 0);


            var has = (b & 0x80) > 0;
            handle |= ((uint)(b & 0x7f) >> 1);
            var bitcnt = 6;

            while(has)
            {
               if (bitcnt>31)
                throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadMetaHandle()");

               b = m_Stream.ReadByte();
               if (b<0) throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadMetaHandle(): eof");
               has = (b & 0x80) > 0;
               handle |= (uint)(b & 0x7f) << bitcnt;
               bitcnt += 7;
            }

            if (meta)
            {
               var sv = ReadString();
               if (sv!=null)
                return new MetaHandle(true, handle, new VarIntStr( sv ));
               else
                return new MetaHandle(true, handle, new VarIntStr( ReadUInt() ));
            }

            return new MetaHandle(true, handle);
          }


              public override MetaHandle? ReadNullableMetaHandle()
              {
                var has = this.ReadBool();

                if (has) return this.ReadMetaHandle();

                return null;
              }



          public override DateTime ReadDateTime()
          {
            //prior to 20150625 DKh
            //var bin = this.ReadLong();
            //return DateTime.FromBinary(bin);

            var ticks = (long) m_Stream.ReadBEUInt64();
            var kind = (DateTimeKind) m_Stream.ReadByte();
            return new DateTime(ticks, kind);
          }

              public override DateTime? ReadNullableDateTime()
              {
                var has = this.ReadBool();

                if (has) return this.ReadDateTime();

                return null;
              }


          public override TimeSpan ReadTimeSpan()
          {
            var ticks = this.ReadLong();
            return TimeSpan.FromTicks(ticks);
          }

              public override TimeSpan? ReadNullableTimeSpan()
              {
                var has = this.ReadBool();

                if (has) return this.ReadTimeSpan();

                return null;
              }


          public override Guid ReadGuid()
          {
            var arr = this.ReadByteArray();
            return new Guid(arr);
          }

              public override Guid? ReadNullableGuid()
              {
                var has = this.ReadBool();

                if (has) return this.ReadGuid();

                return null;
              }

          public override NFX.DataAccess.Distributed.GDID ReadGDID()
          {
            var era = this.ReadUInt();
            var id = this.ReadULong();
            return new NFX.DataAccess.Distributed.GDID(era, id);
          }

              public override NFX.DataAccess.Distributed.GDID? ReadNullableGDID()
              {
                var has = this.ReadBool();

                if (has) return this.ReadGDID();

                return null;
              }


         public override NFX.Glue.Protocol.TypeSpec ReadTypeSpec()
         {
            var result = new NFX.Glue.Protocol.TypeSpec();
            result.m_Name = this.ReadString();
            result.m_Hash = m_Stream.ReadBEUInt64();
            return result;
         }

         public override NFX.Glue.Protocol.MethodSpec ReadMethodSpec()
         {
            var result = new NFX.Glue.Protocol.MethodSpec();
            result.m_MethodName = this.ReadString();
            result.m_ReturnType = m_Stream.ReadBEUInt64();
            result.m_Signature = this.ReadByteArray();
            result.m_Hash = m_Stream.ReadBEUInt64();
            return result;
         }


         public override FID ReadFID()
          {
            var id = m_Stream.ReadBEUInt64();
            return new FID(id);
          }

              public override FID? ReadNullableFID()
              {
                var has = this.ReadBool();

                if (has) return this.ReadFID();

                return null;
              }

         public override NFX.ApplicationModel.Pile.PilePointer ReadPilePointer()
          {
            var node = this.ReadInt();
            var seg  = this.ReadInt();
            var adr  = this.ReadInt();
            return new NFX.ApplicationModel.Pile.PilePointer(node, seg, adr);
          }

              public override NFX.ApplicationModel.Pile.PilePointer? ReadNullablePilePointer()
              {
                var has = this.ReadBool();

                if (has) return this.ReadPilePointer();

                return null;
              }



          public override VarIntStr ReadVarIntStr()
          {
            var str = this.ReadString();
            if (str!=null) return new VarIntStr(str);

            return new VarIntStr( this.ReadUInt() );
          }

              public override VarIntStr? ReadNullableVarIntStr()
              {
                var has = this.ReadBool();

                if (has) return this.ReadVarIntStr();

                return null;
              }

          public override NLSMap ReadNLSMap()
          {
            var cnt = this.ReadInt();
            var result = new NLSMap(cnt>0);
            if (cnt<=0) return result;
            for(var i=0; i<cnt; i++)
            {
              var key = this.ReadString();
              if (key==null)
                throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadNLSMap(): key==null");
              var name = this.ReadString();
              var descr = this.ReadString();
              result.m_Data[key] = new NLSMap.NDPair(name, descr);
            }

            return result;
          }

              public override NLSMap? ReadNullableNLSMap()
              {
                var has = this.ReadBool();

                if (has) return this.ReadNLSMap();

                return null;
              }

          public override Collections.StringMap ReadStringMap()
          {
            var has = this.ReadBool();
            if (!has) return null;

            var senseCase = this.ReadBool();

            var dict = Collections.StringMap.MakeDictionary(senseCase);

            var count = this.ReadInt();
            for(var i=0; i<count; i++)
            {
              var key = this.ReadString();
              var value = this.ReadString();
              dict[key] = value;
            }

            return new Collections.StringMap(senseCase, dict);
          }

        #endregion



    }
}
