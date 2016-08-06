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

using NUnit.Framework;

using NFX.IO;
using NFX.Serialization.JSON;
using NFX.Collections;

namespace NFX.NUnit.IO
{
    [TestFixture]  
    public class SlimFormatReadWrite
    {
        
        [TestCase]
        public void PositiveInt()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();

            r.BindStream(ms);
            w.BindStream(ms);
             
            w.Write(0);  //1 byte
            w.Write(127); //2 byte
            w.Write(128); //2 bytes
            w.Write(int.MaxValue);//5 bytes

            ms.Seek(0, SeekOrigin.Begin);

             Assert.AreEqual(0, r.ReadInt());
             Assert.AreEqual(127, r.ReadInt());
             Assert.AreEqual(128, r.ReadInt());
             Assert.AreEqual(int.MaxValue, r.ReadInt());

             Assert.AreEqual(10, ms.Length);  
          }
        }

        [TestCase]
        public void NegativeInt()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();
             
            r.BindStream(ms);
            w.BindStream(ms);

            w.Write(0);  //1 byte
            w.Write(-127); //2 byte
            w.Write(-128); //2 bytes
            w.Write(int.MinValue);//5 bytes

            ms.Seek(0, SeekOrigin.Begin);

             Assert.AreEqual(0, r.ReadInt());
             Assert.AreEqual(-127, r.ReadInt());
             Assert.AreEqual(-128, r.ReadInt());
             Assert.AreEqual(int.MinValue, r.ReadInt());

             Assert.AreEqual(10, ms.Length);  
          }
        }

        [TestCase]
        public void NullableInt()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();
             
             r.BindStream(ms);
            w.BindStream(ms);

            w.Write((int?)18);  //2 byte
            w.Write((int?)-127); //3 byte
            w.Write((int?)null); //1 bytes
            w.Write((int?)255);//3 bytes

            ms.Seek(0, SeekOrigin.Begin);

             Assert.AreEqual(18, r.ReadNullableInt().Value);
             Assert.AreEqual(-127, r.ReadNullableInt().Value);
             Assert.AreEqual(false, r.ReadNullableInt().HasValue);
             Assert.AreEqual(255, r.ReadNullableInt().Value);

             Assert.AreEqual(9, ms.Length);  
          }
        }




                                    [TestCase]
                                    public void PositiveLong()
                                    {
                                      using(var ms = new MemoryStream())
                                      {
                                        var r = SlimFormat.Instance.MakeReadingStreamer();
                                        var w = SlimFormat.Instance.MakeWritingStreamer();
             
                                          r.BindStream(ms);
                                        w.BindStream(ms);

                                        w.Write(0L);  //1 byte
                                        w.Write(127L); //2 byte
                                        w.Write(128L); //2 bytes
                                        w.Write(long.MaxValue);//10 bytes

                                        ms.Seek(0, SeekOrigin.Begin);

                                         Assert.AreEqual(0L, r.ReadLong());
                                         Assert.AreEqual(127L, r.ReadLong());
                                         Assert.AreEqual(128L, r.ReadLong());
                                         Assert.AreEqual(long.MaxValue, r.ReadLong());

                                         Assert.AreEqual(15, ms.Length);  
                                      }
                                    }

                                    [TestCase]
                                    public void NegativeLong()
                                    {
                                      using(var ms = new MemoryStream())
                                      {
                                        var r = SlimFormat.Instance.MakeReadingStreamer();
                                        var w = SlimFormat.Instance.MakeWritingStreamer();

                                         r.BindStream(ms);
                                        w.BindStream(ms);
             
                                        w.Write(0L);  //1 byte
                                        w.Write(-127L); //2 byte
                                        w.Write(-128L); //2 bytes
                                        w.Write(long.MinValue);//10 bytes

                                        ms.Seek(0, SeekOrigin.Begin);

                                         Assert.AreEqual(0L, r.ReadLong());
                                         Assert.AreEqual(-127L, r.ReadLong());
                                         Assert.AreEqual(-128L, r.ReadLong());
                                         Assert.AreEqual(long.MinValue, r.ReadLong());

                                         Assert.AreEqual(15, ms.Length);  
                                      }
                                    }

                                    [TestCase]
                                    public void NullableLong()
                                    {
                                      using(var ms = new MemoryStream())
                                      {
                                        var r = SlimFormat.Instance.MakeReadingStreamer();
                                        var w = SlimFormat.Instance.MakeWritingStreamer();
             
                                          r.BindStream(ms);
                                        w.BindStream(ms);

                                        w.Write((long?)18);  //2 byte
                                        w.Write((long?)-127); //3 byte
                                        w.Write((long?)null); //1 bytes
                                        w.Write((long?)long.MaxValue);//11 bytes

                                        ms.Seek(0, SeekOrigin.Begin);

                                         Assert.AreEqual(18, r.ReadNullableLong().Value);
                                         Assert.AreEqual(-127, r.ReadNullableLong().Value);
                                         Assert.AreEqual(false, r.ReadNullableLong().HasValue);
                                         Assert.AreEqual(long.MaxValue, r.ReadNullableLong().Value);

                                         Assert.AreEqual(17, ms.Length);  
                                      }
                                    }


        [TestCase]
        public void UInt()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

            w.Write((uint)0);  //1 byte
            w.Write((uint)127); //1 byte
            w.Write((uint)128); //2 bytes
            w.Write(uint.MaxValue);//5 bytes

            ms.Seek(0, SeekOrigin.Begin);

             Assert.AreEqual(0, r.ReadUInt());
             Assert.AreEqual(127, r.ReadUInt());
             Assert.AreEqual(128, r.ReadUInt());
             Assert.AreEqual(uint.MaxValue, r.ReadUInt());

             Assert.AreEqual(9, ms.Length);  
          }
        }

        [TestCase]
        public void NullableUInt()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

            w.Write((uint?)18);  //2 byte
            w.Write((uint?)127); //2 byte
            w.Write((uint?)null); //1 bytes
            w.Write((uint?)255);//3 bytes

            ms.Seek(0, SeekOrigin.Begin);

             Assert.AreEqual(18, r.ReadNullableUInt().Value);
             Assert.AreEqual(127, r.ReadNullableUInt().Value);
             Assert.AreEqual(false, r.ReadNullableUInt().HasValue);
             Assert.AreEqual(255, r.ReadNullableUInt().Value);

             Assert.AreEqual(8, ms.Length);  
          }
        }

           
                        [TestCase]
                        public void ULong()
                        {
                          using(var ms = new MemoryStream())
                          {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();
             
                              r.BindStream(ms);
                            w.BindStream(ms);

                            w.Write((ulong)0);  //1 byte
                            w.Write((ulong)127); //1 byte
                            w.Write((ulong)128); //2 bytes
                            w.Write(ulong.MaxValue);//10 bytes

                            ms.Seek(0, SeekOrigin.Begin);

                             Assert.AreEqual(0, r.ReadULong());
                             Assert.AreEqual(127, r.ReadULong());
                             Assert.AreEqual(128, r.ReadULong());
                             Assert.AreEqual(ulong.MaxValue, r.ReadULong());

                             Assert.AreEqual(14, ms.Length);  
                          }
                        }

                        [TestCase]
                        public void NullableULong()
                        {
                          using(var ms = new MemoryStream())
                          {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();
             
                              r.BindStream(ms);
                            w.BindStream(ms);

                            w.Write((ulong?)18);  //2 byte
                            w.Write((ulong?)127); //2 byte
                            w.Write((ulong?)null); //1 bytes
                            w.Write((ulong?)255);//3 bytes

                            ms.Seek(0, SeekOrigin.Begin);

                             Assert.AreEqual(18, r.ReadNullableULong().Value);
                             Assert.AreEqual(127, r.ReadNullableULong().Value);
                             Assert.AreEqual(false, r.ReadNullableULong().HasValue);
                             Assert.AreEqual(255, r.ReadNullableULong().Value);

                             Assert.AreEqual(8, ms.Length);  
                          }
                        }
        
        [TestCase]
        public void UShort()
        {
            using(var ms = new MemoryStream())
            {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

            w.Write((ushort)0);  //1 byte
            w.Write((ushort)127); //1 byte
            w.Write((ushort)128); //2 bytes
            w.Write(ushort.MaxValue);//3 bytes

            ms.Seek(0, SeekOrigin.Begin);

                Assert.AreEqual(0, r.ReadUShort());
                Assert.AreEqual(127, r.ReadUShort());
                Assert.AreEqual(128, r.ReadUShort());
                Assert.AreEqual(ushort.MaxValue, r.ReadUShort());

                Assert.AreEqual(7, ms.Length);  
            }
        }

        [TestCase]
        public void NullableUShort()
        {
            using(var ms = new MemoryStream())
            {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

            w.Write((ushort?)18);  //2 byte
            w.Write((ushort?)127); //2 byte
            w.Write((ushort?)null); //1 bytes
            w.Write((ushort?)255);//3 bytes

            ms.Seek(0, SeekOrigin.Begin);

                Assert.AreEqual(18, r.ReadNullableUShort().Value);
                Assert.AreEqual(127, r.ReadNullableUShort().Value);
                Assert.AreEqual(false, r.ReadNullableUShort().HasValue);
                Assert.AreEqual(255, r.ReadNullableUShort().Value);

                Assert.AreEqual(8, ms.Length);  
            }
        }



                                                                    [TestCase]
                                                                    public void PositiveShort()
                                                                    {
                                                                      using(var ms = new MemoryStream())
                                                                      {
                                                                        var r = SlimFormat.Instance.MakeReadingStreamer();
                                                                        var w = SlimFormat.Instance.MakeWritingStreamer();
             
                                                                        r.BindStream(ms);
                                                                      w.BindStream(ms);

                                                                        w.Write((short)0);  //1 byte
                                                                        w.Write((short)127); //2 byte
                                                                        w.Write((short)128); //2 bytes
                                                                        w.Write(short.MaxValue);//3 bytes

                                                                        ms.Seek(0, SeekOrigin.Begin);

                                                                         Assert.AreEqual(0, r.ReadShort());
                                                                         Assert.AreEqual(127, r.ReadShort());
                                                                         Assert.AreEqual(128, r.ReadShort());
                                                                         Assert.AreEqual(short.MaxValue, r.ReadShort());

                                                                         Assert.AreEqual(8, ms.Length);  
                                                                      }
                                                                    }

                                                                    [TestCase]
                                                                    public void NegativeShort()
                                                                    {
                                                                      using(var ms = new MemoryStream())
                                                                      {
                                                                        var r = SlimFormat.Instance.MakeReadingStreamer();
                                                                        var w = SlimFormat.Instance.MakeWritingStreamer();
             
                                                                          r.BindStream(ms);
                                                                        w.BindStream(ms);

                                                                        w.Write((short)0);  //1 byte
                                                                        w.Write((short)-127); //2 byte
                                                                        w.Write((short)-128); //2 bytes
                                                                        w.Write(short.MinValue);//3 bytes

                                                                        ms.Seek(0, SeekOrigin.Begin);

                                                                         Assert.AreEqual(0, r.ReadShort());
                                                                         Assert.AreEqual(-127, r.ReadShort());
                                                                         Assert.AreEqual(-128, r.ReadShort());
                                                                         Assert.AreEqual(short.MinValue, r.ReadShort());

                                                                         Assert.AreEqual(8, ms.Length);  
                                                                      }
                                                                    }

                                                                    [TestCase]
                                                                    public void NullableShort()
                                                                    {
                                                                      using(var ms = new MemoryStream())
                                                                      {
                                                                        var r = SlimFormat.Instance.MakeReadingStreamer();
                                                                        var w = SlimFormat.Instance.MakeWritingStreamer();
             
                                                                            r.BindStream(ms);
                                                                          w.BindStream(ms);

                                                                        w.Write((short?)18);  //2 byte
                                                                        w.Write((short?)-127); //3 byte
                                                                        w.Write((short?)null); //1 bytes
                                                                        w.Write((short?)255);//3 bytes

                                                                        ms.Seek(0, SeekOrigin.Begin);

                                                                         Assert.AreEqual(18, r.ReadNullableShort().Value);
                                                                         Assert.AreEqual(-127, r.ReadNullableShort().Value);
                                                                         Assert.AreEqual(false, r.ReadNullableShort().HasValue);
                                                                         Assert.AreEqual(255, r.ReadNullableShort().Value);

                                                                         Assert.AreEqual(9, ms.Length);  
                                                                      }
                                                                    }






        [TestCase]
        public void _DateTime()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();

                 r.BindStream(ms);
            w.BindStream(ms);

                var now = App.LocalizedTime;

                w.Write(now);
                
                ms.Seek(0, SeekOrigin.Begin);
                
                Assert.AreEqual(now, r.ReadDateTime());
            }
        }

        [TestCase]
        public void NullableDateTime()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();

                 r.BindStream(ms);
            w.BindStream(ms);

                var now = App.LocalizedTime;

                w.Write((DateTime?)null);
                w.Write((DateTime?)now);
                
                ms.Seek(0, SeekOrigin.Begin);
                
                Assert.AreEqual(false, r.ReadNullableDateTime().HasValue); 
                Assert.AreEqual(now, r.ReadNullableDateTime().Value); 
            }
        }



        [TestCase]
        public void _TimeSpan()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();

                 r.BindStream(ms);
            w.BindStream(ms);
             
                var ts = TimeSpan.FromMilliseconds(25467);

                w.Write(ts);
                
                ms.Seek(0, SeekOrigin.Begin);
                
                Assert.AreEqual(ts, r.ReadTimeSpan());
            }
        }

        [TestCase]
        public void NullableTimeSpan()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();

                 r.BindStream(ms);
            w.BindStream(ms);
             
                var ts = TimeSpan.FromMilliseconds(25467);

                w.Write((TimeSpan?)null);
                w.Write((TimeSpan?)ts);
                
                ms.Seek(0, SeekOrigin.Begin);
                
                Assert.AreEqual(false, r.ReadNullableTimeSpan().HasValue); 
                Assert.AreEqual(ts, r.ReadNullableTimeSpan().Value); 
            }
        }



                    [TestCase]
                    public void _Guid()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                            var guid = Guid.NewGuid();

                            w.Write(guid);
                
                            ms.Seek(0, SeekOrigin.Begin);
                
                            Assert.AreEqual(guid, r.ReadGuid());
                        }
                    }

                    [TestCase]
                    public void NullableGuid()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                            var guid = Guid.NewGuid();

                            w.Write((Guid?)null);
                            w.Write((Guid?)guid);
                
                            ms.Seek(0, SeekOrigin.Begin);
                
                            Assert.AreEqual(false, r.ReadNullableGuid().HasValue); 
                            Assert.AreEqual(guid, r.ReadNullableGuid().Value); 
                        }
                    }
                    

                    [TestCase]
                    public void _GDID_1()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                            var gdid = new NFX.DataAccess.Distributed.GDID(5, 123);

                            w.Write(gdid);
                
                            ms.Seek(0, SeekOrigin.Begin);
                
                            Assert.AreEqual(gdid, r.ReadGDID());
                        }
                    }

                    [TestCase]
                    public void _GDID_2()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                            var gdid = new NFX.DataAccess.Distributed.GDID(11, 0xffffffffffffffe0);

                            w.Write(gdid);
                
                            ms.Seek(0, SeekOrigin.Begin);
                
                            Assert.AreEqual(gdid, r.ReadGDID());
                        }
                    }

                    [TestCase]
                    public void NullableGDID_1()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                            var gdid = new NFX.DataAccess.Distributed.GDID(0, 123);

                            w.Write((NFX.DataAccess.Distributed.GDID?)null);
                            w.Write((NFX.DataAccess.Distributed.GDID?)gdid);
                
                            ms.Seek(0, SeekOrigin.Begin);
                
                            Assert.AreEqual(false, r.ReadNullableGDID().HasValue); 
                            Assert.AreEqual(gdid, r.ReadNullableGDID().Value); 
                        }
                    }

                    [TestCase]
                    public void NullableGDID_2()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                            var gdid = new NFX.DataAccess.Distributed.GDID(12, 0xffffffffffffffe0);

                            w.Write((NFX.DataAccess.Distributed.GDID?)null);
                            w.Write((NFX.DataAccess.Distributed.GDID?)gdid);
                
                            ms.Seek(0, SeekOrigin.Begin);
                
                            Assert.AreEqual(false, r.ReadNullableGDID().HasValue); 
                            Assert.AreEqual(gdid, r.ReadNullableGDID().Value); 
                        }
                    }





        [TestCase]
        public void _Bool()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                
                w.Write(true);
                w.Write(false);

                ms.Seek(0, SeekOrigin.Begin);
                
                Assert.AreEqual(true, r.ReadBool());
                Assert.AreEqual(false, r.ReadBool());
            }
        }

        [TestCase]
        public void NullableBool()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                w.Write((bool?)null);
                w.Write((bool?)true);
                
                ms.Seek(0, SeekOrigin.Begin);
                
                Assert.AreEqual(false, r.ReadNullableBool().HasValue); 
                Assert.AreEqual(true, r.ReadNullableBool().Value); 
            }
        }


        [TestCase]
        public void _Byte()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();
             

              r.BindStream(ms);
            w.BindStream(ms);
                
                w.Write((byte)2);
                w.Write((byte)0xff);

                ms.Seek(0, SeekOrigin.Begin);
                
                Assert.AreEqual(2, r.ReadByte());
                Assert.AreEqual(0xff, r.ReadByte());
            }
        }

        [TestCase]
        public void NullableByte()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                w.Write((byte?)null);
                w.Write((byte?)19);
                
                ms.Seek(0, SeekOrigin.Begin);
                
                Assert.AreEqual(false, r.ReadNullableByte().HasValue); 
                Assert.AreEqual(19, r.ReadNullableByte().Value); 
            }
        }



                    [TestCase]
                    public void _SByte()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                
                            w.Write((sbyte)-2);
                            w.Write((sbyte)0x4e);

                            ms.Seek(0, SeekOrigin.Begin);
                
                            Assert.AreEqual(-2, r.ReadSByte());
                            Assert.AreEqual(0x4e, r.ReadSByte());
                        }
                    }

                    [TestCase]
                    public void NullableSByte()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();
             

              r.BindStream(ms);
            w.BindStream(ms);

                            w.Write((sbyte?)null);
                            w.Write((sbyte?)-19);
                
                            ms.Seek(0, SeekOrigin.Begin);
                
                            Assert.AreEqual(false, r.ReadNullableSByte().HasValue); 
                            Assert.AreEqual(-19, r.ReadNullableSByte().Value); 
                        }
                    }


            [TestCase]
            public void _Float()
            {
                using(var ms = new MemoryStream())
                {
                    var r = SlimFormat.Instance.MakeReadingStreamer();
                    var w = SlimFormat.Instance.MakeWritingStreamer();
             

              r.BindStream(ms);
            w.BindStream(ms);
                
                    w.Write((float)-2);
                    w.Write((float)0.1234);

                    ms.Seek(0, SeekOrigin.Begin);
                
                    Assert.AreEqual(-2f, r.ReadFloat());
                    Assert.AreEqual(0.1234f, r.ReadFloat());
                }
            }

            [TestCase]
            public void NullableFloat()
            {
                using(var ms = new MemoryStream())
                {
                    var r = SlimFormat.Instance.MakeReadingStreamer();
                    var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                    w.Write((float?)null);
                    w.Write((float?)0.6789);
                
                    ms.Seek(0, SeekOrigin.Begin);
                
                    Assert.AreEqual(false, r.ReadNullableFloat().HasValue); 
                    Assert.AreEqual(0.6789f, r.ReadNullableFloat().Value); 
                }
            }


            [TestCase]
            public void _Double()
            {
                using(var ms = new MemoryStream())
                {
                    var r = SlimFormat.Instance.MakeReadingStreamer();
                    var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                
                    w.Write((double)-2);
                    w.Write((double)0.1234);

                    ms.Seek(0, SeekOrigin.Begin);
                
                    Assert.AreEqual(-2, r.ReadDouble());
                    Assert.AreEqual(0.1234, r.ReadDouble());
                }
            }

            [TestCase]
            public void NullableDouble()
            {
                using(var ms = new MemoryStream())
                {
                    var r = SlimFormat.Instance.MakeReadingStreamer();
                    var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                    w.Write((double?)null);
                    w.Write((double?)0.6789);
                
                    ms.Seek(0, SeekOrigin.Begin);
                
                    Assert.AreEqual(false, r.ReadNullableDouble().HasValue); 
                    Assert.AreEqual(0.6789, r.ReadNullableDouble().Value); 
                }
            }



                [TestCase]
                public void _Decimal()
                {
                    using(var ms = new MemoryStream())
                    {
                        var r = SlimFormat.Instance.MakeReadingStreamer();
                        var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                
                        w.Write(-2m);
                        w.Write(0.1234m);

                        w.Write(789123612637621332.2390m);
                        w.Write(-789123612637621332.2390m);
                        
                        w.Write(23123123213789123612637621332.001237182738m);
                        w.Write(-23123123213789123612637621332.001237182738m);

                        w.Write(0.123123213789123612637621332001237182738m);
                        w.Write(-0.123123213789123612637621332001237182738m);

                        w.Write(0.0000000000000000000000000073m);
                        w.Write(-0.0000000000000000000000000073m);

                        ms.Seek(0, SeekOrigin.Begin);

                        Assert.AreEqual(-2m, r.ReadDecimal());
                        Assert.AreEqual(0.1234m, r.ReadDecimal());

                        Assert.AreEqual(789123612637621332.2390m, r.ReadDecimal());
                        Assert.AreEqual(-789123612637621332.2390m, r.ReadDecimal());

                        Assert.AreEqual(23123123213789123612637621332.001237182738m, r.ReadDecimal());
                        Assert.AreEqual(-23123123213789123612637621332.001237182738m, r.ReadDecimal());

                        Assert.AreEqual(0.123123213789123612637621332001237182738m, r.ReadDecimal());
                        Assert.AreEqual(-0.123123213789123612637621332001237182738m, r.ReadDecimal());

                        Assert.AreEqual(0.0000000000000000000000000073m, r.ReadDecimal());
                        Assert.AreEqual(-0.0000000000000000000000000073m, r.ReadDecimal());
                    }
                }

                [TestCase]
                public void NullableDecimal()
                {
                    using(var ms = new MemoryStream())
                    {
                        var r = SlimFormat.Instance.MakeReadingStreamer();
                        var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                        w.Write((decimal?)null);
                        w.Write((decimal?)0.6789);
                
                        ms.Seek(0, SeekOrigin.Begin);
                
                        Assert.AreEqual(false, r.ReadNullableDecimal().HasValue); 
                        Assert.AreEqual(0.6789, r.ReadNullableDecimal().Value); 
                    }
                }



                           [TestCase]
                            public void _Char()
                            {
                                using(var ms = new MemoryStream())
                                {
                                    var r = SlimFormat.Instance.MakeReadingStreamer();
                                    var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                
                                    w.Write('a');
                                    w.Write('b');

                                    ms.Seek(0, SeekOrigin.Begin);
                
                                    Assert.AreEqual('a', r.ReadChar());
                                    Assert.AreEqual('b', r.ReadChar());
                                }
                            }

                            [TestCase]
                            public void NullableChar()
                            {
                                using(var ms = new MemoryStream())
                                {
                                    var r = SlimFormat.Instance.MakeReadingStreamer();
                                    var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                                    w.Write((char?)null);
                                    w.Write((char?)'Z');
                
                                    ms.Seek(0, SeekOrigin.Begin);
                
                                    Assert.AreEqual(false, r.ReadNullableChar().HasValue); 
                                    Assert.AreEqual('Z', r.ReadNullableChar().Value); 
                                }
                            }


                [TestCase]
                public void MetaHandle()
                {
                    using(var ms = new MemoryStream())
                    {
                        var r = SlimFormat.Instance.MakeReadingStreamer();
                        var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                
                        w.Write(new MetaHandle(1));// 1 byte
                        w.Write(new MetaHandle(0xffff));// 3 byte
                        w.Write(new MetaHandle(0xffff, new VarIntStr("0123456789")));// 3 byte + 2 len + 10 byte  = 19
                        ms.Seek(0, SeekOrigin.Begin);
                
                        Assert.AreEqual(1, r.ReadMetaHandle().Handle);
                        Assert.AreEqual(0xffff, r.ReadMetaHandle().Handle);

                        Assert.AreEqual(19,  ms.Length);
                    }
                }

                [TestCase]
                public void NullableMetaHandle()
                {
                    using(var ms = new MemoryStream())
                    {
                        var r = SlimFormat.Instance.MakeReadingStreamer();
                        var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

                        w.Write((MetaHandle?)null); //1 byte
                        w.Write((MetaHandle?)new MetaHandle(12, new VarIntStr("It works")));// 1(nnul) + 1(12) + 2(strlen) + 8(It works) = 12 bytes
                        ms.Seek(0, SeekOrigin.Begin);
                
                        Assert.AreEqual(false, r.ReadNullableMetaHandle().HasValue); 
                       
                        var mh = r.ReadNullableMetaHandle().Value;
                        Assert.AreEqual(12, mh.Handle);
                        Assert.AreEqual("It works", mh.Metadata.Value.StringValue);

                        Assert.AreEqual(13,  ms.Length); 
                    }
                }



        [TestCase]
        public void StringArray()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

            var arr1 = new string[10];
            var arr2 = new string[3];
            string[] arr3 = null;

            arr1[0] = "AAA";
            arr2[2] = "zzz";

            w.Write(arr1);
            w.Write(arr2);
            w.Write(arr3);

            ms.Seek(0, SeekOrigin.Begin);

            var r1 = r.ReadStringArray();
            var r2 = r.ReadStringArray();
            var r3 = r.ReadStringArray();

             Assert.AreEqual("AAA", r1[0]);
             Assert.AreEqual("zzz", r2[2]);
             Assert.AreEqual(10, r1.Length);
             Assert.AreEqual(3, r2.Length);
             Assert.IsNull(r3);
          }
        }


        [TestCase]
        public void ByteArray()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();
             
              r.BindStream(ms);
            w.BindStream(ms);

            var arr1 = new byte[]{12,56,11,254};
            var arr2 = new byte[]{};
            byte[] arr3 = null;


            w.Write(arr1);
            w.Write(arr2);
            w.Write(arr3);

            ms.Seek(0, SeekOrigin.Begin);

            var r1 = r.ReadByteArray();
            var r2 = r.ReadByteArray();
            var r3 = r.ReadByteArray();

             Assert.AreEqual(56, r1[1]);
             Assert.AreEqual(254, r1[3]);
             Assert.AreEqual(arr1.Length, r1.Length);
             Assert.AreEqual(arr2.Length, r2.Length);
             Assert.IsNull(r3);
          }
        }

        [TestCase]
        public void CharArray()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();
          
            r.BindStream(ms);
            w.BindStream(ms); 


            var arr1 = new char[]{'a','b','c','z'};
            var arr2 = new char[]{};
            char[] arr3 = null;


            w.Write(arr1);
            w.Write(arr2);
            w.Write(arr3);

            ms.Seek(0, SeekOrigin.Begin);

            var r1 = r.ReadByteArray();
            var r2 = r.ReadByteArray();
            var r3 = r.ReadByteArray();

             Assert.AreEqual('b', r1[1]);
             Assert.AreEqual('z', r1[3]);
             Assert.AreEqual(arr1.Length, r1.Length);
             Assert.AreEqual(arr2.Length, r2.Length);
             Assert.IsNull(r3);
          }
        }


                    [TestCase]
                    public void TypeSpec()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

                            r.BindStream(ms);
                            w.BindStream(ms); 
             
                            var spec = new NFX.Glue.Protocol.TypeSpec(typeof(System.Collections.Generic.List<int>));

                            w.Write(spec);
                
                            ms.Seek(0, SeekOrigin.Begin);
                
                            Assert.AreEqual(spec, r.ReadTypeSpec());
                        }
                    }

                    [TestCase]
                    public void MethodSpec()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

                            r.BindStream(ms);
                            w.BindStream(ms); 
             
                            var spec = new NFX.Glue.Protocol.MethodSpec(typeof(System.Collections.Generic.List<int>).GetMethod("Clear"));

                            w.Write(spec);
                
                            ms.Seek(0, SeekOrigin.Begin);
                
                            Assert.AreEqual(spec, r.ReadMethodSpec());
                        }
                    }

                    [TestCase]
                    public void FID()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

                            r.BindStream(ms);
                            w.BindStream(ms); 
             
                            var fid = NFX.FID.Generate();

                            w.Write(fid);
                
                            ms.Seek(0, SeekOrigin.Begin);
                
                            Assert.AreEqual(fid, r.ReadFID());
                        }
                    }

                    [TestCase]
                    public void _FID()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

                            r.BindStream(ms);
                            w.BindStream(ms); 
             
                            var fid = NFX.FID.Generate();

                            w.Write((NFX.FID?)null);
                            w.Write(fid);
                
                            ms.Seek(0, SeekOrigin.Begin);
                
                            Assert.AreEqual(false, r.ReadNullableFID().HasValue);
                            Assert.AreEqual(fid, r.ReadFID());
                        }
                    }


                    [TestCase]
                    public void PilePointer()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

                            r.BindStream(ms);
                            w.BindStream(ms); 
             
                            var pp = new NFX.ApplicationModel.Pile.PilePointer(10,20,30);

                            w.Write(pp);
                
                            ms.Seek(0, SeekOrigin.Begin);
                
                            Assert.AreEqual(pp, r.ReadPilePointer());
                        }
                    }

                    [TestCase]
                    public void _PilePointer()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

                            r.BindStream(ms);
                            w.BindStream(ms); 
             
                            var pp = new NFX.ApplicationModel.Pile.PilePointer(10,20,30);

                            w.Write((NFX.ApplicationModel.Pile.PilePointer?)null);
                            w.Write(pp);
                
                            ms.Seek(0, SeekOrigin.Begin);
                
                            Assert.AreEqual(false, r.ReadNullablePilePointer().HasValue);
                            Assert.AreEqual(pp, r.ReadPilePointer());
                        }
                    }


        [TestCase]
        public void NLSMap()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();

                r.BindStream(ms);
                w.BindStream(ms); 
             
                var map = new NLSMap(
        @"nls{
            eng{n='Cream>Serum>Day' d='Daily Serum Care'}
            rus{n='Крем>Серум>Дневной' d='Дневной Уход Серум'}
            deu{n='Ein Drek'}
          }".AsLaconicConfig(handling: ConvertErrorHandling.Throw));

                w.Write(map);
                
                ms.Position = 0;
                
                var map2 = r.ReadNLSMap();

                Assert.IsNotNull(map2);
                Assert.AreEqual(3, map2.Count);
                Assert.AreEqual("Cream>Serum>Day", map2["ENG"].Name);
                Assert.AreEqual("Крем>Серум>Дневной", map2["rus"].Name);
                Assert.AreEqual("Ein Drek", map2["dEu"].Name);

                Assert.AreEqual("Daily Serum Care", map2["ENG"].Description);
                Assert.AreEqual("Дневной Уход Серум", map2["rus"].Description);
                Assert.AreEqual(null, map2["dEu"].Description);

                ms.Position = 0;
                NLSMap nullmap = new NLSMap();
                w.Write(nullmap);
                ms.Position = 0;

                var map3 = r.ReadNLSMap();
                Assert.IsNull( map3.m_Data);
                Assert.AreEqual(0 , map3.Count);


            }
        }

                [TestCase]
                public void _NLSMap()
                {
                    using(var ms = new MemoryStream())
                    {
                        var r = SlimFormat.Instance.MakeReadingStreamer();
                        var w = SlimFormat.Instance.MakeWritingStreamer();

                        r.BindStream(ms);
                        w.BindStream(ms); 
             
                        NLSMap? map = new NLSMap(
                @"nls{
                    eng{n='Cream>Serum>Day' d='Daily Serum Care'}
                    rus{n='Крем>Серум>Дневной' d='Дневной Уход Серум'}
                    deu{n='Ein Drek'}
                  }".AsLaconicConfig(handling: ConvertErrorHandling.Throw));

                        w.Write(map);
                
                        ms.Position = 0;
                
                        var map2 = r.ReadNullableNLSMap();

                        Assert.IsTrue(map2.HasValue);
                        Assert.AreEqual(3, map2.Value.Count);
                        Assert.AreEqual("Cream>Serum>Day", map2.Value["ENG"].Name);
                        Assert.AreEqual("Крем>Серум>Дневной", map2.Value["rus"].Name);
                        Assert.AreEqual("Ein Drek", map2.Value["dEu"].Name);

                       
                        ms.Position = 0;
                        NLSMap? nullmap = null;
                        w.Write(nullmap);
                        ms.Position = 0;

                        var map3 = r.ReadNullableNLSMap();
                        Assert.IsFalse( map3.HasValue );
                    }
                }


                [TestCase]
                public void StringMap()
                {
                    using(var ms = new MemoryStream())
                    {
                        var r = SlimFormat.Instance.MakeReadingStreamer();
                        var w = SlimFormat.Instance.MakeWritingStreamer();

                        r.BindStream(ms);
                        w.BindStream(ms); 
             
                        var mapS = new StringMap(true)
                        {
                           {"a", "Alex"},
                           {"b","Boris"}
                        };

                        var mapI = new StringMap(false)
                        {
                           {"a", "Alex"},
                           {"b","Boris"},
                           {"c","Chuck"}
                        };



                        w.Write(mapS);
                        w.Write(mapI);
                
                        ms.Seek(0, SeekOrigin.Begin);
                
                        
                        var mapS2 = r.ReadStringMap();
                        Assert.IsTrue(mapS2.CaseSensitive);
                        Assert.AreEqual(2, mapS2.Count);
                        Assert.AreEqual("Alex", mapS2["a"]);
                        Assert.AreEqual("Boris", mapS2["b"]);

                        var mapI2 = r.ReadStringMap();
                        Assert.IsFalse(mapI2.CaseSensitive);
                        Assert.AreEqual(3, mapI2.Count);
                        Assert.AreEqual("Alex", mapI2["a"]);
                        Assert.AreEqual("Boris", mapI2["b"]);
                        Assert.AreEqual("Chuck", mapI2["c"]);
                    }
                }

    }
}
