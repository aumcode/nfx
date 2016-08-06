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
    /// Writes primitives to stream
    /// </summary>
    [Inventory(Concerns=SystemConcerns.Testing | SystemConcerns.MissionCriticality)]
    public abstract class WritingStreamer : Streamer
    {
       #region .ctor

            protected WritingStreamer(Encoding encoding=null) : base(encoding)
            {
            }

       #endregion


        #region Public

         public abstract void Flush();

          public abstract void Write(bool value);


              public abstract void Write(bool? value);



          public abstract void Write(byte value);


              public abstract void Write(byte? value);



          public abstract void Write(byte[] buffer);

          public abstract void Write(int[] value);

          public abstract void Write(long[] value);

          public abstract void Write(double[] value);


          public abstract void Write(char ch);

              public abstract void Write(char? value);




          public abstract void Write(char[] buffer);



          public abstract void Write(string[] array);



          public abstract void Write(decimal value);

              public abstract void Write(decimal? value);



          public abstract void Write(double value);


              public abstract void Write(double? value);



          public abstract void Write(float value);


              public abstract void Write(float? value);



          public abstract void Write(int value);


              public abstract void Write(int? value);


          public abstract void Write(long value);

              public abstract void Write(long? value);



          public abstract void Write(sbyte value);


              public abstract void Write(sbyte? value);



          public abstract void Write(short value);


              public abstract void Write(short? value);


          public abstract void Write(string value);


          public abstract void Write(uint value);


              public abstract void Write(uint? value);


          public abstract void Write(ulong value);


              public abstract void Write(ulong? value);


          public abstract void Write(ushort value);

              public abstract void Write(ushort? value);




          public abstract void Write(MetaHandle value);



              public abstract void Write(MetaHandle? value);





          //public abstract void Write(byte[] buffer, int index, int count);

          //public abstract void Write(char[] chars, int index, int count);


          public abstract void Write(DateTime value);


              public abstract void Write(DateTime? value);


          public abstract void Write(TimeSpan value);

              public abstract void Write(TimeSpan? value);

          public abstract void Write(Guid value);

              public abstract void Write(Guid? value);

          public abstract void Write(NFX.DataAccess.Distributed.GDID value);

              public abstract void Write(NFX.DataAccess.Distributed.GDID? value);


          public abstract void Write(NFX.Glue.Protocol.TypeSpec spec);
          public abstract void Write(NFX.Glue.Protocol.MethodSpec spec);


           public abstract void Write(FID value);

              public abstract void Write(FID? value);

           public abstract void Write(NFX.ApplicationModel.Pile.PilePointer value);

              public abstract void Write(NFX.ApplicationModel.Pile.PilePointer? value);



           public abstract void Write(VarIntStr value);

              public abstract void Write(VarIntStr? value);

           public abstract void Write(NLSMap map);

              public abstract void Write(NLSMap? map);

           public abstract void Write(Collections.StringMap map);

        #endregion

    }
}
