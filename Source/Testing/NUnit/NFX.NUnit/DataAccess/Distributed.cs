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

using NFX.Serialization.JSON;
using NFX.DataAccess.Distributed;
using NFX.Serialization.Slim;


namespace NFX.NUnit.DataAccess
{
    [TestFixture]
    public class Distributed
    {
        [TestCase]
        public void Create_Seal()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Assert.AreEqual(ParcelState.Creating, parcel.State);
            Assert.AreEqual(true, parcel.PayloadUnwrapped);
            parcel.Seal(FakeNOPBank.Instance);

            Assert.AreEqual(true, parcel.PayloadUnwrapped);
        }

            [TestCase]
            public void Create_Seal_NotWrapped()
            {
                var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

                var parcel = new PeopleNamesParcel_NotWrapped(new GDID(0, 123), names);

                Assert.AreEqual(ParcelState.Creating, parcel.State);
                Assert.AreEqual(true, parcel.PayloadUnwrapped);
                parcel.Seal(FakeNOPBank.Instance);

                Assert.AreEqual(true, parcel.PayloadUnwrapped);
            }


        [TestCase]
        public void ForgetPayloadCopy()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Assert.AreEqual(ParcelState.Creating, parcel.State);
            Assert.IsTrue(parcel.PayloadUnwrapped);
            Assert.IsFalse(parcel.HasWrappedPayload);
            
            parcel.Seal(FakeNOPBank.Instance);//SEAL!!!!!!!!!!!!!!!!

            Assert.IsTrue(parcel.PayloadUnwrapped);
            Assert.IsFalse(parcel.HasWrappedPayload);

            parcel.ForgetPayloadCopy();//FORGET!!!!!!!!!!!!!!!!!!!!

            Assert.IsFalse(parcel.PayloadUnwrapped);
            Assert.IsTrue(parcel.HasWrappedPayload);

            //even after forget payload, i can get my payload back
            var payload = parcel.Payload;
            Assert.IsTrue(parcel.PayloadUnwrapped);
            Assert.IsTrue(parcel.HasWrappedPayload);

            Assert.AreEqual(4, payload.Count);
            Assert.AreEqual("Kozloff",  payload[0]);
            Assert.AreEqual("Sergeev",  payload[1]);
            Assert.AreEqual("Aroyan",   payload[2]);
            Assert.AreEqual("Gurevich", payload[3]);
             
        }

                [TestCase]
                public void ForgetPayloadCopy_NotWrapped()
                {
                    var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

                    var parcel = new PeopleNamesParcel_NotWrapped(new GDID(0, 123), names);

                    Assert.AreEqual(ParcelState.Creating, parcel.State);
                    Assert.IsTrue(parcel.PayloadUnwrapped);
                    Assert.IsFalse(parcel.HasWrappedPayload);
            
                    parcel.Seal(FakeNOPBank.Instance);//SEAL!!!!!!!!!!!!!!!!

                    Assert.IsTrue(parcel.PayloadUnwrapped);
                    Assert.IsFalse(parcel.HasWrappedPayload);

                    parcel.ForgetPayloadCopy();//FORGET!!!!!!!!!!!!!!!!!!!!

                    Assert.IsFalse(parcel.PayloadUnwrapped);
                    Assert.IsFalse(parcel.HasWrappedPayload);//because NotWrapped mode

                    //even after forget payload, i can get my payload back
                    var payload = parcel.Payload;
                    Assert.IsTrue(parcel.PayloadUnwrapped);
                    Assert.IsFalse(parcel.HasWrappedPayload);//because NotWrapped mode

                    Assert.AreEqual(4, payload.Count);
                    Assert.AreEqual("Kozloff",  payload[0]);
                    Assert.AreEqual("Sergeev",  payload[1]);
                    Assert.AreEqual("Aroyan",   payload[2]);
                    Assert.AreEqual("Gurevich", payload[3]);
             
                }


        [TestCase]
        [ExpectedException(typeof(ParcelSealValidationException), ExpectedMessage="Aroyan", MatchType=MessageMatch.Contains) ]
        public void Create_Seal_ValidationError()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "NoAroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Assert.AreEqual(ParcelState.Creating, parcel.State);
            Assert.AreEqual(true, parcel.PayloadUnwrapped);
            parcel.Seal(FakeNOPBank.Instance);

            Assert.AreEqual(true, parcel.PayloadUnwrapped);
        }

            [TestCase]
            [ExpectedException(typeof(ParcelSealValidationException), ExpectedMessage="Aroyan", MatchType=MessageMatch.Contains) ]
            public void Create_Seal_ValidationError_NotWrapped()
            {
                var names = new List<string>{"Kozloff", "Sergeev", "NoAroyan", "Gurevich"};

                var parcel = new PeopleNamesParcel_NotWrapped(new GDID(0, 123), names);

                Assert.AreEqual(ParcelState.Creating, parcel.State);
                Assert.AreEqual(true, parcel.PayloadUnwrapped);
                parcel.Seal(FakeNOPBank.Instance);

                Assert.AreEqual(true, parcel.PayloadUnwrapped);
            }

        [TestCase]
        public void Create_Seal_Wrap()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Assert.AreEqual(ParcelState.Creating, parcel.State);
            Assert.AreEqual(true, parcel.PayloadUnwrapped);
            parcel.Seal(FakeNOPBank.Instance);

            var buf = parcel.WrappedPayload;

            Assert.IsNotNull( buf);
        }

                 [TestCase]
                public void Create_Seal_Wrap_NotWrapped()
                {
                    var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

                    var parcel = new PeopleNamesParcel_NotWrapped(new GDID(0, 123), names);

                    Assert.AreEqual(ParcelState.Creating, parcel.State);
                    Assert.AreEqual(true, parcel.PayloadUnwrapped);
                    parcel.Seal(FakeNOPBank.Instance);

                    var buf = parcel.WrappedPayload;

                    Assert.IsNull( buf);//because NotWrapped mode
                }

        [TestCase]
        public void Create_Seal_Wrap_UnwrapPayloadIntoOtherPayload()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Assert.AreEqual(ParcelState.Creating, parcel.State);
            Assert.AreEqual(true, parcel.PayloadUnwrapped);
            parcel.Seal(FakeNOPBank.Instance);

            var buf = parcel.WrappedPayload;

            Assert.IsNotNull( buf);

            var ser = new SlimSerializer( Parcel.STANDARD_KNOWN_SERIALIZER_TYPES );
            
            var names2 = ser.Deserialize( new MemoryStream( buf) ) as List<string>;
            
            Assert.IsNotNull(names2);
            Assert.AreEqual( names.Count, names2.Count);
            Assert.IsTrue( names2.SequenceEqual( names) );

        }

        [TestCase]
        public void Create_Seal_SerializeDeserialize_Read()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Assert.AreEqual(ParcelState.Creating, parcel.State);
            Assert.AreEqual(true, parcel.PayloadUnwrapped);
            parcel.Seal(FakeNOPBank.Instance);

            

            var ser = new SlimSerializer( Parcel.STANDARD_KNOWN_SERIALIZER_TYPES );
            
            var ms = new MemoryStream();

            ser.Serialize(ms, parcel);
            ms.Position = 0;
            var parcel2 = ser.Deserialize( ms ) as PeopleNamesParcel;
            
            Assert.IsNotNull(parcel2);
            Assert.IsFalse(parcel2.PayloadUnwrapped);
            Assert.IsTrue(parcel2.HasWrappedPayload);
            var payload2 = parcel2.Payload;
            Assert.IsNotNull(payload2);
            Assert.IsTrue(parcel2.PayloadUnwrapped);

            Assert.IsTrue(parcel2.HasWrappedPayload);
            parcel2.ForgetWrappedPayloadCopy();
            Assert.IsFalse(parcel2.HasWrappedPayload);

                                   
            Assert.AreEqual(payload2.Count, names.Count);
            Assert.IsTrue( payload2.SequenceEqual( names) );

        }


              [TestCase]
              public void Create_Seal_SerializeDeserialize_Read_NotWrapped()
              {
                  var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

                  var parcel = new PeopleNamesParcel_NotWrapped(new GDID(0, 123), names);

                  Assert.AreEqual(ParcelState.Creating, parcel.State);
                  Assert.AreEqual(true, parcel.PayloadUnwrapped);
                  parcel.Seal(FakeNOPBank.Instance);

            

                  var ser = new SlimSerializer( Parcel.STANDARD_KNOWN_SERIALIZER_TYPES );
            
                  var ms = new MemoryStream();

                  ser.Serialize(ms, parcel);
                  ms.Position = 0;
                  var parcel2 = ser.Deserialize( ms ) as PeopleNamesParcel_NotWrapped;
            
                  Assert.IsNotNull(parcel2);
                  Assert.IsFalse(parcel2.PayloadUnwrapped);
                  Assert.IsFalse(parcel2.HasWrappedPayload);//mode unwrapped
                  var payload2 = parcel2.Payload;
                  Assert.IsNotNull(payload2);
                  Assert.IsTrue(parcel2.PayloadUnwrapped);

                  Assert.IsFalse(parcel2.HasWrappedPayload);//mode unwrapped
                  parcel2.ForgetWrappedPayloadCopy(); //no effect in wnwrapped mode
                  Assert.IsFalse(parcel2.HasWrappedPayload);

                                   
                  Assert.AreEqual(payload2.Count, names.Count);
                  Assert.IsTrue( payload2.SequenceEqual( names) );

              }


        [TestCase]
        public void FullLifecycle()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Assert.AreEqual(ParcelState.Creating, parcel.State);
            Assert.AreEqual(true, parcel.PayloadUnwrapped);
            parcel.Seal(FakeNOPBank.Instance);

            var ser = new SlimSerializer( Parcel.STANDARD_KNOWN_SERIALIZER_TYPES );
            
            var ms = new MemoryStream();

            ser.Serialize(ms, parcel);
            ms.Position = 0;
            var parcel2 = ser.Deserialize( ms ) as PeopleNamesParcel;
            
            Assert.IsNotNull(parcel2);

            Assert.IsTrue(parcel2.HasWrappedPayload);
            parcel2.Open();
            Assert.IsFalse(parcel2.HasWrappedPayload);

            parcel2.Payload[1]="Boyarskiy";
            parcel2.Seal(FakeNOPBank.Instance);
            
            ms.Position = 0;
            ser.Serialize(ms, parcel2);
            ms.Position = 0;
            var parcel3 = ser.Deserialize( ms ) as PeopleNamesParcel;

            Assert.IsTrue( new List<string>{"Kozloff", "Boyarskiy", "Aroyan", "Gurevich"}.SequenceEqual( parcel3.Payload) );
        }

               [TestCase]
              public void FullLifecycle_NotWrapped()
              {
                  var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

                  var parcel = new PeopleNamesParcel_NotWrapped(new GDID(0, 123), names);

                  Assert.AreEqual(ParcelState.Creating, parcel.State);
                  Assert.AreEqual(true, parcel.PayloadUnwrapped);
                  parcel.Seal(FakeNOPBank.Instance);

                  var ser = new SlimSerializer( Parcel.STANDARD_KNOWN_SERIALIZER_TYPES );
            
                  var ms = new MemoryStream();

                  ser.Serialize(ms, parcel);
                  ms.Position = 0;
                  var parcel2 = ser.Deserialize( ms ) as PeopleNamesParcel_NotWrapped;
            
                  Assert.IsNotNull(parcel2);

                  Assert.IsFalse(parcel2.HasWrappedPayload);//mode unwrapped
                  parcel2.Open();                           // no effect
                  Assert.IsFalse(parcel2.HasWrappedPayload);//mode unwrapped

                  parcel2.Payload[1]="Boyarskiy";
                  parcel2.Seal(FakeNOPBank.Instance);
            
                  ms.Position = 0;
                  ser.Serialize(ms, parcel2);
                  ms.Position = 0;
                  var parcel3 = ser.Deserialize( ms ) as PeopleNamesParcel_NotWrapped;

                  Assert.IsTrue( new List<string>{"Kozloff", "Boyarskiy", "Aroyan", "Gurevich"}.SequenceEqual( parcel3.Payload) );
              }



        [TestCase]
        [ExpectedException(typeof(DistributedDataAccessException), ExpectedMessage="EnsurePayloadWrappedCopy", MatchType=MessageMatch.Contains) ]
        public void StateError_1()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "NoAroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Assert.AreEqual(ParcelState.Creating, parcel.State);
            Assert.AreEqual(true, parcel.PayloadUnwrapped);
            var buf = parcel.WrappedPayload;//throws
        }

        [TestCase]
        [ExpectedException(typeof(DistributedDataAccessException), ExpectedMessage="Validate", MatchType=MessageMatch.Contains) ]
        public void StateError_2()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Assert.AreEqual(ParcelState.Creating, parcel.State);
            Assert.AreEqual(true, parcel.PayloadUnwrapped);
            parcel.Seal(FakeNOPBank.Instance);
            parcel.Validate(FakeNOPBank.Instance);
        }

          [TestCase]
          [ExpectedException(typeof(DistributedDataAccessException), ExpectedMessage="Validate", MatchType=MessageMatch.Contains) ]
          public void StateError_2_NotWrapped()
          {
              var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

              var parcel = new PeopleNamesParcel_NotWrapped(new GDID(0, 123), names);

              Assert.AreEqual(ParcelState.Creating, parcel.State);
              Assert.AreEqual(true, parcel.PayloadUnwrapped);
              parcel.Seal(FakeNOPBank.Instance);
              parcel.Validate(FakeNOPBank.Instance);
          }

        [TestCase]
        [ExpectedException(typeof(DistributedDataAccessException), ExpectedMessage="Open", MatchType=MessageMatch.Contains) ]
        public void StateError_3()
        {
            var parcel = new PeopleNamesParcel(new GDID(0, 123), null);
            parcel.Open();
        }

              [TestCase]
              [ExpectedException(typeof(DistributedDataAccessException), ExpectedMessage="Open", MatchType=MessageMatch.Contains) ]
              public void StateError_3_NotWrapped()
              {
                  var parcel = new PeopleNamesParcel_NotWrapped(new GDID(0, 123), null);
                  parcel.Open();
              }


        [TestCase]
        [ExpectedException(typeof(SlimSerializationException), ExpectedMessage="OnSerializing", MatchType=MessageMatch.Contains) ]
        public void StateError_4()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Assert.AreEqual(ParcelState.Creating, parcel.State);
            Assert.AreEqual(true, parcel.PayloadUnwrapped);
            //not sealed
            var ser = new SlimSerializer( Parcel.STANDARD_KNOWN_SERIALIZER_TYPES );
            
            var ms = new MemoryStream();

            ser.Serialize(ms, parcel);//can not serialize open parcel

        }

            [TestCase]
            [ExpectedException(typeof(SlimSerializationException), ExpectedMessage="OnSerializing", MatchType=MessageMatch.Contains) ]
            public void StateError_4_NotWrapped()
            {
                var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

                var parcel = new PeopleNamesParcel_NotWrapped(new GDID(0, 123), names);

                Assert.AreEqual(ParcelState.Creating, parcel.State);
                Assert.AreEqual(true, parcel.PayloadUnwrapped);
                //not sealed
                var ser = new SlimSerializer( Parcel.STANDARD_KNOWN_SERIALIZER_TYPES );
            
                var ms = new MemoryStream();

                ser.Serialize(ms, parcel);//can not serialize open parcel

            }

        [TestCase]
        [ExpectedException(typeof(DistributedDataAccessException), ExpectedMessage="ForgetPayloadCopy", MatchType=MessageMatch.Contains) ]
        public void StateError_5()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "NoAroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Assert.AreEqual(ParcelState.Creating, parcel.State);
            Assert.AreEqual(true, parcel.PayloadUnwrapped);
            parcel.ForgetPayloadCopy();
        }

              [TestCase]
              [ExpectedException(typeof(DistributedDataAccessException), ExpectedMessage="ForgetPayloadCopy", MatchType=MessageMatch.Contains) ]
              public void StateError_5_NotWrapped()
              {
                  var names = new List<string>{"Kozloff", "Sergeev", "NoAroyan", "Gurevich"};

                  var parcel = new PeopleNamesParcel_NotWrapped(new GDID(0, 123), names);

                  Assert.AreEqual(ParcelState.Creating, parcel.State);
                  Assert.AreEqual(true, parcel.PayloadUnwrapped);
                  parcel.ForgetPayloadCopy();
              }

        [TestCase]
        public void Parcel_DeepClone_1()
        {
           var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};
           var p1 = new PeopleNamesParcel(new GDID(0, 123), names);

           p1.Seal(FakeNOPBank.Instance);
           var p2 = p1.DeepClone() as PeopleNamesParcel;

           Assert.IsFalse( object.ReferenceEquals(p1, p2) );
           Assert.IsFalse( object.ReferenceEquals(p1.Payload, p2.Payload) );

           Assert.IsTrue(p1.Payload.SequenceEqual(p2.Payload));
           Assert.AreEqual( p1.GDID, p2.GDID );
        }

                [TestCase]
                public void Parcel_DeepClone_1_NotWrapped()
                {
                   var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};
                   var p1 = new PeopleNamesParcel_NotWrapped(new GDID(0, 123), names);

                   p1.Seal(FakeNOPBank.Instance);
                   var p2 = p1.DeepClone() as PeopleNamesParcel_NotWrapped;

                   Assert.IsFalse( object.ReferenceEquals(p1, p2) );
                   Assert.IsFalse( object.ReferenceEquals(p1.Payload, p2.Payload) );

                   Assert.IsTrue(p1.Payload.SequenceEqual(p2.Payload));
                   Assert.AreEqual( p1.GDID, p2.GDID );
                }

        [TestCase]
        public void Parcel_DeepClone_2_Equals_ToString()
        {
           var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};
           var p1 = new PeopleNamesParcel(new GDID(0, 123), names);

           p1.Seal(FakeNOPBank.Instance);
           var p2 = p1.DeepClone() as PeopleNamesParcel;

           Assert.IsFalse( object.ReferenceEquals(p1, p2) );
           Assert.IsTrue( p1.Equals(p2) );

           Console.WriteLine(p1.ToString());
        }


                  [TestCase]
                  public void Parcel_DeepClone_2_Equals_ToString_NotWrapped()
                  {
                     var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};
                     var p1 = new PeopleNamesParcel_NotWrapped(new GDID(0, 123), names);

                     p1.Seal(FakeNOPBank.Instance);
                     var p2 = p1.DeepClone() as PeopleNamesParcel_NotWrapped;

                     Assert.IsFalse( object.ReferenceEquals(p1, p2) );
                     Assert.IsTrue( p1.Equals(p2) );

                     Console.WriteLine(p1.ToString());
                  }

        [TestCase]
        public void Parcel_DeepClone_Benchmark()
        {
           const int CNT = 25000;

           var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};
           var p1 = new PeopleNamesParcel(new GDID(0, 123), names);

           p1.Seal(FakeNOPBank.Instance);
           
           var sw = System.Diagnostics.Stopwatch.StartNew();
           for(var i=0; i<CNT; i++)
           {
             var p2 = p1.DeepClone() as PeopleNamesParcel;

             Assert.IsFalse( object.ReferenceEquals(p1, p2) );
             Assert.IsFalse( object.ReferenceEquals(p1.Payload, p2.Payload) );

             Assert.IsTrue(p1.Payload.Count == p2.Payload.Count);
             Assert.AreEqual( p1.GDID, p2.GDID );
           }
           sw.Stop();
           Console.WriteLine("NOTE: Core i7 3.2Ghz 14000 ops/sec on subsequent runs w/o optimization");
           Console.WriteLine("Cloned {0} times in {1} ms at {2} ops/sec", CNT, sw.ElapsedMilliseconds, CNT / (sw.ElapsedMilliseconds / 1000d));
        }

              [TestCase]
              public void Parcel_DeepClone_Benchmark_NotWrapped()
              {
                 const int CNT = 25000;

                 var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};
                 var p1 = new PeopleNamesParcel_NotWrapped(new GDID(0, 123), names);

                 p1.Seal(FakeNOPBank.Instance);
           
                 var sw = System.Diagnostics.Stopwatch.StartNew();
                 for(var i=0; i<CNT; i++)
                 {
                   var p2 = p1.DeepClone() as PeopleNamesParcel_NotWrapped;

                   Assert.IsFalse( object.ReferenceEquals(p1, p2) );
                   Assert.IsFalse( object.ReferenceEquals(p1.Payload, p2.Payload) );

                   Assert.IsTrue(p1.Payload.Count == p2.Payload.Count);
                   Assert.AreEqual( p1.GDID, p2.GDID );
                 }
                 sw.Stop();
                 Console.WriteLine("NOTE: Core i7 3.2Ghz 17000 ops/sec on subsequent runs w/o optimization");
                 Console.WriteLine("Cloned {0} times in {1} ms at {2} ops/sec", CNT, sw.ElapsedMilliseconds, CNT / (sw.ElapsedMilliseconds / 1000d));
              }

        [TestCase]
        public void Parcel_DeepClone_Benchmark_Parallel()
        {
           const int CNT = 100000;

           var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};
           var p1 = new PeopleNamesParcel(new GDID(0, 123), names);

           p1.Seal(FakeNOPBank.Instance);
           
           var sw = System.Diagnostics.Stopwatch.StartNew();
           System.Threading.Tasks.Parallel.For(0, CNT, 
           (i)=>
           {
             var p2 = p1.DeepClone() as PeopleNamesParcel;

             Assert.IsFalse( object.ReferenceEquals(p1, p2) );
             Assert.IsFalse( object.ReferenceEquals(p1.Payload, p2.Payload) );

             Assert.IsTrue(p1.Payload.Count == p2.Payload.Count);
             Assert.AreEqual( p1.GDID, p2.GDID );
           });
           sw.Stop();
           Console.WriteLine("NOTE: Core i7 x 6 cores 3.2Ghz 47000+ ops/sec on subsequent runs w/o optimization in non-server GC mode");
           Console.WriteLine("Cloned {0} times in {1} ms at {2} ops/sec", CNT, sw.ElapsedMilliseconds, CNT / (sw.ElapsedMilliseconds / 1000d));
        }

              [TestCase]
              public void Parcel_DeepClone_Benchmark_Parallel_NotWrapped()
              {
                 const int CNT = 100000;

                 var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};
                 var p1 = new PeopleNamesParcel_NotWrapped(new GDID(0, 123), names);

                 p1.Seal(FakeNOPBank.Instance);
           
                 var sw = System.Diagnostics.Stopwatch.StartNew();
                 System.Threading.Tasks.Parallel.For(0, CNT, 
                 (i)=>
                 {
                   var p2 = p1.DeepClone() as PeopleNamesParcel_NotWrapped;

                   Assert.IsFalse( object.ReferenceEquals(p1, p2) );
                   Assert.IsFalse( object.ReferenceEquals(p1.Payload, p2.Payload) );

                   Assert.IsTrue(p1.Payload.Count == p2.Payload.Count);
                   Assert.AreEqual( p1.GDID, p2.GDID );
                 });
                 sw.Stop();
                 Console.WriteLine("NOTE: Core i7 x 6 cores 3.2Ghz 61000+ ops/sec on subsequent runs w/o optimization in non-server GC mode");
                 Console.WriteLine("Cloned {0} times in {1} ms at {2} ops/sec", CNT, sw.ElapsedMilliseconds, CNT / (sw.ElapsedMilliseconds / 1000d));
              }


//============================================================================================
      [TestCase]
        public void HumanParcel_DeepClone_Benchmark()
        {
           const int CNT = 25000;

           var human = new Human
           {
              ID = new GDID(0,123), 
              Name="Abosum Bora Sukolomokalapop", 
              DOB = DateTime.Now,
              Status = HumanStatus.Ok,
               Addr1 = "234 Babai Drive # 12345",
                Addr2 = "Suite 23",
                 AddrCity = "Zmeevo",
                  AddrState = "NY",
                   AddrZip = "90210",
                    Father = new Human{ID=new GDID(1,12312), Name = "Farukh Na Chazz"},
                    Mother = new Human{ID=new GDID(1,1342312), Name = "Fatime Suka Dodik"}

           };
           var p1 = new HumanParcel(new GDID(0, 123), human);

           p1.Seal(FakeNOPBank.Instance);
           
           var sw = System.Diagnostics.Stopwatch.StartNew();
           for(var i=0; i<CNT; i++)
           {
             var p2 = p1.DeepClone() as HumanParcel;

             Assert.IsFalse( object.ReferenceEquals(p1, p2) );
             Assert.IsFalse( object.ReferenceEquals(p1.Payload, p2.Payload) );

             Assert.IsTrue(p1.Payload.Name == p2.Payload.Name);
             Assert.AreEqual( p1.GDID, p2.GDID );
           }
           sw.Stop();
           Console.WriteLine("NOTE: Core i7 3.2Ghz 14000 ops/sec on subsequent runs w/o optimization");
           Console.WriteLine("Cloned {0} times in {1} ms at {2} ops/sec", CNT, sw.ElapsedMilliseconds, CNT / (sw.ElapsedMilliseconds / 1000d));
        }

              [TestCase]
              public void HumanParcel_DeepClone_Benchmark_NotWrapped()
              {
                 const int CNT = 25000;

                 var human = new Human
                 {
                    ID = new GDID(0,123), 
                    Name="Abosum Bora Sukolomokalapop", 
                    DOB = DateTime.Now,
                    Status = HumanStatus.Ok,
                     Addr1 = "234 Babai Drive # 12345",
                      Addr2 = "Suite 23",
                       AddrCity = "Zmeevo",
                        AddrState = "NY",
                         AddrZip = "90210",
                          Father = new Human{ID=new GDID(1,12312), Name = "Farukh Na Chazz"},
                          Mother = new Human{ID=new GDID(1,1342312), Name = "Fatime Suka Dodik"}

                 };
                 var p1 = new HumanParcel_NotWrapped(new GDID(0, 123), human);

                 p1.Seal(FakeNOPBank.Instance);
           
                 var sw = System.Diagnostics.Stopwatch.StartNew();
                 for(var i=0; i<CNT; i++)
                 {
                   var p2 = p1.DeepClone() as HumanParcel_NotWrapped;

                   Assert.IsFalse( object.ReferenceEquals(p1, p2) );
                   Assert.IsFalse( object.ReferenceEquals(p1.Payload, p2.Payload) );

                   Assert.IsTrue(p1.Payload.Name == p2.Payload.Name);
                   Assert.AreEqual( p1.GDID, p2.GDID );
                 }
                 sw.Stop();
                 Console.WriteLine("NOTE: Core i7 3.2Ghz 16000 ops/sec on subsequent runs w/o optimization");
                 Console.WriteLine("Cloned {0} times in {1} ms at {2} ops/sec", CNT, sw.ElapsedMilliseconds, CNT / (sw.ElapsedMilliseconds / 1000d));
              }                  







//============================================================================================



        [TestCase]
        public void GDID_1()
        {
          var gdid = new GDID(2, 5, 89078);
          Assert.AreEqual(2,     gdid.Era);
          Assert.AreEqual(5,     gdid.Authority);
          Assert.AreEqual(89078, gdid.Counter);
        }

        [TestCase]
        [ExpectedException(typeof(DistributedDataAccessException), ExpectedMessage="GDID can not be created from the supplied", MatchType=MessageMatch.Contains) ]
        public void GDID_2()
        {
          var gdid = new GDID(0, 16, 89078);
        }

        [TestCase]
        [ExpectedException(typeof(DistributedDataAccessException), ExpectedMessage="GDID can not be created from the supplied", MatchType=MessageMatch.Contains) ]
        public void GDID_3()
        {
          var gdid = new GDID(0, 12, GDID.COUNTER_MAX+1);
        }

        [TestCase]
        public void GDID_4()
        {
          var gdid = new GDID(0, 15, GDID.COUNTER_MAX);
          Assert.AreEqual(15,               gdid.Authority);
          Assert.AreEqual(GDID.COUNTER_MAX, gdid.Counter);
        }

        [TestCase]
        public void GDID_5()
        {
          var gdid = new GDID(0, 0, GDID.COUNTER_MAX);
          Assert.AreEqual(0,                gdid.Authority);
          Assert.AreEqual(GDID.COUNTER_MAX, gdid.Counter);
        }

        [TestCase]
        public void GDID_6()
        {
          var gdid = new GDID(0, 0, 0);
          Assert.AreEqual(0, gdid.Authority);
          Assert.AreEqual(0, gdid.Counter);
        }


        [TestCase]
        public void GDID_7()
        {
          var gdid1 = new GDID(0, 0, 12321);
          var gdid2 = new GDID(0, 1, 0);
          Assert.AreEqual(-1, gdid1.CompareTo(gdid2));
          Assert.IsFalse( gdid1.Equals(gdid2));
        }

        [TestCase]
        public void GDID_8()
        {
          var gdid1 = new GDID(0, 1, 12321);
          var gdid2 = new GDID(0, 1, 0);
          Assert.AreEqual(1, gdid1.CompareTo(gdid2));
          Assert.IsFalse( gdid1.Equals(gdid2));
        }

        [TestCase]
        public void GDID_9()
        {
          var gdid1 = new GDID(0, 3, 57);
          var gdid2 = new GDID(0, 3, 57);
          Assert.AreEqual(0, gdid1.CompareTo(gdid2));
          Assert.IsTrue( gdid1.Equals(gdid2));

          var gdid3 = new GDID(1, 3, 57);
          var gdid4 = new GDID(2, 3, 57);
          Assert.AreEqual(-1, gdid3.CompareTo(gdid4));
          Assert.IsFalse( gdid3.Equals(gdid4));
        }

        [TestCase]
        public void GDID_10()
        {
          var gdid = new GDID(1293, 3, 57);
          var s = gdid.ToString();
          Console.WriteLine(s);
          Assert.AreEqual("GDID[1293:3458764513820540985(3,57)]", s);
        }

        [TestCase]
        public void GDID_11()
        {
          var gdid = new GDID(0x01020304, 0xfacaca00aa55aa55);
          
          Assert.IsTrue( "01,02,03,04,fa,ca,ca,00,aa,55,aa,55".AsByteArray().SequenceEqual( gdid.Bytes ) );
        }


        [TestCase]
        public void GDID_JSON_1()
        {
          var gdid = new GDID(2, 3, 57);
          var s = gdid.ToJSON();
          Console.WriteLine(s);
          Assert.AreEqual("\"2:3458764513820540985\"", s);
        }

        [TestCase]
        public void GDID_JSON_2()
        {
          var obj = new{ id = new GDID(22, 3, 57), Name = "Tezter"};
          var s = obj.ToJSON();
          Console.WriteLine(s);
          Assert.AreEqual("{\"id\":\"22:3458764513820540985\",\"Name\":\"Tezter\"}", s);
        }


        [TestCase]
        public void Command_HashEquals_1()
        {
          var cmd1 = new Command("GetMeThis"){ new Command.Param("A", 1), new Command.Param("B", 2)};
          var cmd2 = new Command("GetMeThis"){ new Command.Param("A", 2), new Command.Param("B", 1)};

          Console.WriteLine("Command: {0} HashCode: {1} ULongHashCode: {2}", cmd1, cmd1.GetHashCode(), cmd1.GetULongHash());
          Console.WriteLine("Command: {0} HashCode: {1} ULongHashCode: {2}", cmd2, cmd2.GetHashCode(), cmd2.GetULongHash());

          Assert.IsFalse( cmd1.Equals(cmd2));
        }

        [TestCase]
        public void Command_HashEquals_2()
        {
          var cmd1 = new Command("GetMeThis"){ new Command.Param("A", 1), new Command.Param("B", 2)};
          var cmd2 = new Command("GetMeThis"){ new Command.Param("A", 2), new Command.Param("B", 2)};

          Console.WriteLine("Command: {0} HashCode: {1} ULongHashCode: {2}", cmd1, cmd1.GetHashCode(), cmd1.GetULongHash());
          Console.WriteLine("Command: {0} HashCode: {1} ULongHashCode: {2}", cmd2, cmd2.GetHashCode(), cmd2.GetULongHash());

          Assert.IsFalse( cmd1.Equals(cmd2));
        }

        [TestCase]
        public void Command_HashEquals_3()
        {
          var cmd1 = new Command("GetMeThis"){ new Command.Param("A", 1), new Command.Param("B", 2)};
          var cmd2 = new Command("GetMeThis"){ new Command.Param("A", 1), new Command.Param("B", 2)};

          Console.WriteLine("Command: {0} HashCode: {1} ULongHashCode: {2}", cmd1, cmd1.GetHashCode(), cmd1.GetULongHash());
          Console.WriteLine("Command: {0} HashCode: {1} ULongHashCode: {2}", cmd2, cmd2.GetHashCode(), cmd2.GetULongHash());

          Assert.IsTrue( cmd1.Equals(cmd2));
        }

        [TestCase]
        public void Command_HashEquals_4()
        {
          var cmd1 = new Command("GetMeThis"){ new Command.Param("A", 1), new Command.Param("B", 2)};
          var cmd2 = new Command("GetMeThAT"){ new Command.Param("A", 1), new Command.Param("B", 2)};

          Console.WriteLine("Command: {0} HashCode: {1} ULongHashCode: {2}", cmd1, cmd1.GetHashCode(), cmd1.GetULongHash());
          Console.WriteLine("Command: {0} HashCode: {1} ULongHashCode: {2}", cmd2, cmd2.GetHashCode(), cmd2.GetULongHash());

          Assert.IsFalse( cmd1.Equals(cmd2));
        }

        [TestCase]
        public void Command_HashEquals_5()
        {
          var cmd1 = new Command("GetMeThis"){ new Command.Param("A", 1), new Command.Param("B", 2)};
          var cmd2 = new Command("GetMeThis"){ new Command.Param("A", 1), new Command.Param("B", 2), new Command.Param("C", new int[]{1,3,76,-92})};

          Console.WriteLine("Command: {0} HashCode: {1} ULongHashCode: {2}", cmd1, cmd1.GetHashCode(), cmd1.GetULongHash());
          Console.WriteLine("Command: {0} HashCode: {1} ULongHashCode: {2}", cmd2, cmd2.GetHashCode(), cmd2.GetULongHash());

          Assert.IsFalse( cmd1.Equals(cmd2));
        }

        [TestCase]
        public void Command_HashEquals_6()
        {
          var cmd1 = new Command("GetMeThis"){ new Command.Param("A", 1), new Command.Param("B", 2), new Command.Param("C", new int[]{1,3,76,-92})};
          var cmd2 = new Command("GetMeThis"){ new Command.Param("A", 1), new Command.Param("B", 2), new Command.Param("C", new int[]{1,3,76,-92})};

          Console.WriteLine("Command: {0} HashCode: {1} ULongHashCode: {2}", cmd1, cmd1.GetHashCode(), cmd1.GetULongHash());
          Console.WriteLine("Command: {0} HashCode: {1} ULongHashCode: {2}", cmd2, cmd2.GetHashCode(), cmd2.GetULongHash());

          //Are equal because ARRAYS are equal through IStructuralEquatable check
          Assert.IsTrue( cmd1.Equals(cmd2));
        }

        [TestCase]
        public void Command_HashEquals_7()
        {
          var cmd1 = new Command("GetMeThis"){ new Command.Param("A", 1), new Command.Param("B", 2), new Command.Param("C", new int[]{1,322,76,-92})};
          var cmd2 = new Command("GetMeThis"){ new Command.Param("A", 1), new Command.Param("B", 2), new Command.Param("C", new int[]{1,3,76,-92})};

          Console.WriteLine("Command: {0} HashCode: {1} ULongHashCode: {2}", cmd1, cmd1.GetHashCode(), cmd1.GetULongHash());
          Console.WriteLine("Command: {0} HashCode: {1} ULongHashCode: {2}", cmd2, cmd2.GetHashCode(), cmd2.GetULongHash());

          Assert.IsFalse( cmd1.Equals(cmd2));
        }

        [TestCase]
        public void Command_HashEquals_8()
        {
          var cmd1 = new Command("GetMeThis"){ new Command.Param("A", 1), new Command.Param("B", 2), new Command.Param("C", new Command.PList{1,3,76,-92})};
          var cmd2 = new Command("GetMeThis"){ new Command.Param("A", 1), new Command.Param("B", 2), new Command.Param("C", new Command.PList{1,3,76,-92})};

          Console.WriteLine("Command: {0} HashCode: {1} ULongHashCode: {2}", cmd1, cmd1.GetHashCode(), cmd1.GetULongHash());
          Console.WriteLine("Command: {0} HashCode: {1} ULongHashCode: {2}", cmd2, cmd2.GetHashCode(), cmd2.GetULongHash());

          Assert.IsTrue( cmd1.Equals(cmd2));
        }

        [TestCase]
        public void Command_HashEquals_9()
        {
          var cmd1 = new Command("GetMeThis"){ new Command.Param("A", 1), new Command.Param("B", 2), new Command.Param("C", new Command.PList{1,322,76,-92})};
          var cmd2 = new Command("GetMeThis"){ new Command.Param("A", 1), new Command.Param("B", 2), new Command.Param("C", new Command.PList{1,3,76,-92})};

          Console.WriteLine("Command: {0} HashCode: {1} ULongHashCode: {2}", cmd1, cmd1.GetHashCode(), cmd1.GetULongHash());
          Console.WriteLine("Command: {0} HashCode: {1} ULongHashCode: {2}", cmd2, cmd2.GetHashCode(), cmd2.GetULongHash());

          Assert.IsFalse( cmd1.Equals(cmd2));
        }



    }


    //used only for test
    public class FakeNOPBank : IBank
    {

      public static readonly FakeNOPBank Instance = new FakeNOPBank();
      
      
      public IDistributedDataStore DataStore {get { return null;}}
     

      public ISchema Schema {get { return null;}}

      public IRegistry<IAreaInstance> Areas {get { return null;}}


      public string Description { get {return "NOP";}}

      public string GetDescription(string culture) { return "NOP";}

      public NFX.DataAccess.IGDIDProvider IDGenerator {get { return null;}}

      public IReplicationVersionInfo GenerateReplicationVersionInfo(Parcel parcel) { return null;}


      public UInt64 ObjectToShardingID(object key)
      {
        return NFX.DataAccess.Cache.ComplexKeyHashingStrategy.DefaultComplexKeyToCacheKey(key);
      }


      public T LoadByID<T>(GDID id, object shardingId = null, DataVeracity veracity = DataVeracity.Maximum, DataCaching cacheOpt = DataCaching.LatestData, int? cacheMaxAgeSec = null) where T : Parcel
      {
        throw new NotImplementedException();
      }

      public System.Threading.Tasks.Task<T> LoadByIDAsync<T>(GDID id,object shardingId = null, DataVeracity veracity = DataVeracity.Maximum, DataCaching cacheOpt = DataCaching.LatestData, int? cacheMaxAgeSec = null) where T : Parcel
      {
        throw new NotImplementedException();
      }

      public Parcel LoadByQuery(Command loadCommand, DataVeracity veracity = DataVeracity.Maximum, DataCaching cacheOpt = DataCaching.LatestData, int? cacheMaxAgeSec = null)
      {
        throw new NotImplementedException();
      }

      public System.Threading.Tasks.Task<Parcel> LoadByQueryAsync(Command loadCommand, DataVeracity veracity = DataVeracity.Maximum, DataCaching cacheOpt = DataCaching.LatestData, int? cacheMaxAgeSec = null)
      {
        throw new NotImplementedException();
      }

      public IQueryResult Query(Command command, DataVeracity veracity = DataVeracity.Maximum, DataCaching cacheOpt = DataCaching.LatestData, int? cacheMaxAgeSec = null)
      {
        throw new NotImplementedException();
      }

      public System.Threading.Tasks.Task<IQueryResult> QueryAsync(Command command, DataVeracity veracity = DataVeracity.Maximum, DataCaching cacheOpt = DataCaching.LatestData, int? cacheMaxAgeSec = null)
      {
        throw new NotImplementedException();
      }

      public void Save(Parcel parcel, DataCaching cacheOpt = DataCaching.Everywhere, int? cachePriority = null, int? cacheMaxAgeSec = null, DateTime? cacheAbsoluteExpirationUTC = null)
      {
        throw new NotImplementedException();
      }

      public System.Threading.Tasks.Task SaveAsync(Parcel parcel, DataCaching cacheOpt = DataCaching.Everywhere, int? cachePriority = null, int? cacheMaxAgeSec = null, DateTime? cacheAbsoluteExpirationUTC = null)
      {
        throw new NotImplementedException();
      }

      public bool Remove<T>(GDID id, object shardingId = null) where T : Parcel
      {
        throw new NotImplementedException();
      }

      public System.Threading.Tasks.Task<bool> RemoveAsync<T>(GDID id, object shardingId = null) where T : Parcel
      {
        throw new NotImplementedException();
      }

      public string Name
      {
        get { throw new NotImplementedException(); }
      }
    }





}
