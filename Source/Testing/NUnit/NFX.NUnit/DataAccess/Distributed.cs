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
            parcel.Seal(FakeNOPBank.Instance);
            Assert.AreEqual(ParcelState.Sealed, parcel.State);
        }



        [TestCase]
        [ExpectedException(typeof(ParcelSealValidationException), ExpectedMessage="Aroyan", MatchType=MessageMatch.Contains) ]
        public void Create_Seal_ValidationError()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "NoAroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Assert.AreEqual(ParcelState.Creating, parcel.State);
            parcel.Seal(FakeNOPBank.Instance);
        }


        [TestCase]
        public void Create_Seal_SerializeDeserialize_Read()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Assert.AreEqual(ParcelState.Creating, parcel.State);
            parcel.Seal(FakeNOPBank.Instance);

            

            var ser = new SlimSerializer( Parcel.STANDARD_KNOWN_SERIALIZER_TYPES );
            
            var ms = new MemoryStream();

            ser.Serialize(ms, parcel);
            ms.Position = 0;
            var parcel2 = ser.Deserialize( ms ) as PeopleNamesParcel;
            
            Assert.IsNotNull(parcel2);
            var payload2 = parcel2.Payload;
            Assert.IsNotNull(payload2);

                                   
            Assert.AreEqual(payload2.Count, names.Count);
            Assert.IsTrue( payload2.SequenceEqual( names) );

        }



        [TestCase]
        public void FullLifecycle()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Assert.AreEqual(ParcelState.Creating, parcel.State);
            parcel.Seal(FakeNOPBank.Instance);

            var ser = new SlimSerializer( Parcel.STANDARD_KNOWN_SERIALIZER_TYPES );
            
            var ms = new MemoryStream();

            ser.Serialize(ms, parcel);
            ms.Position = 0;
            var parcel2 = ser.Deserialize( ms ) as PeopleNamesParcel;
            
            Assert.IsNotNull(parcel2);

            Assert.AreEqual(ParcelState.Sealed, parcel2.State);
            parcel2.Open();
            Assert.AreEqual(ParcelState.Modifying, parcel2.State);

            parcel2.Payload[1]="Boyarskiy";
            parcel2.Seal(FakeNOPBank.Instance);
            
            ms.Position = 0;
            ser.Serialize(ms, parcel2);
            ms.Position = 0;
            var parcel3 = ser.Deserialize( ms ) as PeopleNamesParcel;

            Assert.IsTrue( new List<string>{"Kozloff", "Boyarskiy", "Aroyan", "Gurevich"}.SequenceEqual( parcel3.Payload) );
        }


        [TestCase]
        [ExpectedException(typeof(DistributedDataAccessException), ExpectedMessage="Validate", MatchType=MessageMatch.Contains) ]
        public void StateError_2()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Assert.AreEqual(ParcelState.Creating, parcel.State);
            parcel.Seal(FakeNOPBank.Instance);
            parcel.Validate(FakeNOPBank.Instance);
        }

        [TestCase]
        [ExpectedException(typeof(DistributedDataAccessException), ExpectedMessage="Argument", MatchType=MessageMatch.Contains) ]
        public void StateError_3()
        {
            var parcel = new PeopleNamesParcel(new GDID(0, 123), null);
        }


        [TestCase]
        [ExpectedException(typeof(SlimSerializationException), ExpectedMessage="OnSerializing", MatchType=MessageMatch.Contains) ]
        public void StateError_4()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Assert.AreEqual(ParcelState.Creating, parcel.State);
            //not sealed
            var ser = new SlimSerializer( Parcel.STANDARD_KNOWN_SERIALIZER_TYPES );
            
            var ms = new MemoryStream();

            ser.Serialize(ms, parcel);//can not serialize open parcel

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




//============================================================================================

        [TestCase]
        public void ShardingPointer_IsAssigned()
        {
          ShardingPointer p1 = new ShardingPointer();
          ShardingPointer p2 = new ShardingPointer(typeof(HumanParcel), 124);

          Assert.IsFalse( p1.IsAssigned );
          Assert.IsTrue( p2.IsAssigned );
        }

    }

}
