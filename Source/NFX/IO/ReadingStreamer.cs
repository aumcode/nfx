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
    /// Reads primitives from stream
    /// </summary>
    [Inventory(Concerns=SystemConcerns.Testing | SystemConcerns.MissionCriticality)]
    public abstract class ReadingStreamer : Streamer
    {
       #region .ctor
            protected ReadingStreamer(Encoding encoding=null) : base(encoding)
            {
            }

       #endregion

       #region Protected

         protected int ReadFromStream(byte[] buffer, int count)
         {
           if (count<=0) return 0;
           var total = 0;
           do
           {
             var got = m_Stream.Read(buffer, total, count-total);
             if (got==0) //EOF
              throw new NFXIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadFromStream(Need: {0}; Got: {1})".Args(count, total));
             total += got;
           }while(total<count);

           return total;
         }


       #endregion

       #region Public

          public abstract bool ReadBool();


              public abstract bool? ReadNullableBool();



          public abstract byte ReadByte();


              public abstract byte? ReadNullableByte();



          public abstract byte[] ReadByteArray();

          public abstract int[] ReadIntArray();

          public abstract long[] ReadLongArray();

          public abstract double[] ReadDoubleArray();


          public abstract char ReadChar();

              public abstract char? ReadNullableChar();




          public abstract char[] ReadCharArray();



          public abstract string[] ReadStringArray();



          public abstract decimal ReadDecimal();

              public abstract decimal? ReadNullableDecimal();



          public abstract double ReadDouble();

              public abstract double? ReadNullableDouble();



          public abstract float ReadFloat();

              public abstract float? ReadNullableFloat();


          public abstract int ReadInt();


              public abstract int? ReadNullableInt();


          public abstract long ReadLong();



              public abstract long? ReadNullableLong();


          public abstract sbyte ReadSByte();

              public abstract sbyte? ReadNullableSByte();


          public abstract short ReadShort();

              public abstract short? ReadNullableShort();



          public abstract string ReadString();




          public abstract uint ReadUInt();


              public abstract uint? ReadNullableUInt();




          public abstract ulong ReadULong();


              public abstract ulong? ReadNullableULong();


          public abstract ushort ReadUShort();


              public abstract ushort? ReadNullableUShort();



          public abstract MetaHandle ReadMetaHandle();



              public abstract MetaHandle? ReadNullableMetaHandle();




          public abstract DateTime ReadDateTime();


              public abstract DateTime? ReadNullableDateTime();



          public abstract TimeSpan ReadTimeSpan();


              public abstract TimeSpan? ReadNullableTimeSpan();



          public abstract Guid ReadGuid();


              public abstract Guid? ReadNullableGuid();


          public abstract NFX.DataAccess.Distributed.GDID ReadGDID();


              public abstract NFX.DataAccess.Distributed.GDID? ReadNullableGDID();

         public abstract NFX.Glue.Protocol.TypeSpec ReadTypeSpec();
         public abstract NFX.Glue.Protocol.MethodSpec ReadMethodSpec();


             public abstract FID ReadFID();


              public abstract FID? ReadNullableFID();

            public abstract NFX.ApplicationModel.Pile.PilePointer ReadPilePointer();


              public abstract NFX.ApplicationModel.Pile.PilePointer? ReadNullablePilePointer();


             public abstract VarIntStr ReadVarIntStr();


              public abstract VarIntStr? ReadNullableVarIntStr();

         public abstract NLSMap ReadNLSMap();

             public abstract NLSMap? ReadNullableNLSMap();


         public abstract Collections.StringMap  ReadStringMap();

        #endregion


    }
}
