using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using NUnit.Framework;

using NFX;
using NFX.Serialization.Slim;
using System.Runtime.Serialization;

namespace NFX.NUnit.Serialization
{
  [TestFixture]
  public class Slim2
  {

    [Test]
    public void T01_NestedClass()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new C1() { FInt = 7, FC11 = new C1.C11() { FInt = -19} };

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (C1)s.Deserialize(ms);

        Assert.AreEqual( 7, dOut.FInt);
        Assert.AreEqual( -19, dOut.FC11.FInt);
      }
    }

    internal class C1
    {
      internal class C11
      {
        public int FInt { get; set;}
      }

      public int FInt { get; set; }

      public C11 FC11 { get; set; }
    }


    [Test]
    public void T02_FieldsWithSameInstance()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var c3 = new C3() { Fint1 = 137 };

        var dIn = new C2 { FObj1 = c3, FI11 = c3, FC31 = c3};

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (C2)s.Deserialize(ms);

        Assert.AreSame(dOut.FObj1, dOut.FI11);
        Assert.AreSame(dOut.FObj1, dOut.FC31);
      }
    }


    internal class C2
    {
      public object FObj1 { get; set; }
      public I1 FI11 { get; set; }
      public C3 FC31 { get; set; }
    }

    internal class C3: I1
    {
      public int Fint1 { get; set; }
    }

    internal interface I1 
    {
      int Fint1 { get; set; }
    }


    [Test]
    public void T03_CyclicSelfReferences()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new C4();
        dIn.FC4 = dIn;

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (C4)s.Deserialize(ms);

        Assert.IsTrue( object.ReferenceEquals( dOut, dOut.FC4) );
        Assert.IsTrue( object.ReferenceEquals(dOut.FC4, dOut.FC4.FC4) );
      }  
    }

    internal class C4
    {
      public C4 FC4 { get; set; }
    }


    [Test]
    public void T04_CyclicReference()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();
        
        var c6 = new C6();
        var dIn = new C5() { FC6 = c6 };
        c6.FC5 = dIn;

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (C5)s.Deserialize(ms);

        Assert.AreSame(dOut, dOut.FC6.FC5);
      }      
    }

    internal class C5
    {
      public C6 FC6 { get; set; }
    }

    internal class C6
    {
      public C5 FC5 { get; set; }
    }


    [Test]
    public void T05_CyclicArraySelfReference()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new C7();
        dIn.FObjs = new Dictionary<int,object> { {17, dIn}, {0, dIn} };
        dIn.FIC7s = new IC7[] { null, dIn };
        dIn.FC7s = (new C7[] { dIn, null }).ToList();

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (C7)s.Deserialize(ms);

        Assert.IsTrue(object.ReferenceEquals(dOut, dOut.FObjs[0]));
        Assert.IsTrue(object.ReferenceEquals(dOut, dOut.FObjs[17]));
        Assert.IsTrue(object.ReferenceEquals(dOut, dOut.FIC7s[1]));
        Assert.IsTrue(object.ReferenceEquals(dOut, dOut.FC7s[0]));
      }
    }

    internal class C7: IC7
    {
      public Dictionary<int, object> FObjs { get; set; }
      public IC7[] FIC7s { get; set; }
      public List<C7> FC7s { get; set; }
    }

    internal interface IC7 
    {
      IC7[] FIC7s { get; set; }
    }


    [Test]
    public void T06_CovariantCyclicArrays()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();
        
        var dIn = new C8();
        dIn.FObjs = new object[] { null, (object)dIn, (IC8)dIn, (C8_1)dIn, dIn };

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (C8)s.Deserialize(ms);

        Assert.IsNull(dOut.FObjs[0]);
        Assert.AreSame(dOut, dOut.FObjs[1]);
        Assert.AreSame(dOut, dOut.FObjs[2]);
        Assert.AreSame(dOut, dOut.FObjs[3]);
      }      
    }

    internal class C8: C8_1
    {
      public object[] FObjs { get; set; }
    }

    internal class C8_1: IC8 {}

    internal interface IC8 {}


    [Test]
    public void T07_CovariantArrays()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();
        
        var dIn = new C10() { FIC9s = new IC9[] { new C9_1(), new C9() } };

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (C10)s.Deserialize(ms);

        Assert.IsInstanceOf<C9_1>(dOut.FIC9s[0]);
        Assert.IsInstanceOf<C9>(dOut.FIC9s[1]);
      }
    }

    internal class C10
    {
      public IC9[] FIC9s { get; set; }
    }

    internal class C9: C9_1 {}

    internal class C9_1: IC9 {}

    internal interface IC9 {}


    [Test]
    public void T08_PreserveReferenceToTheSameArray()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();
        
        var bytes = new byte[] { 0x07, 0x12};
        var strings = new string[] { "Veingarten", "Vecherovskiy", "Zahar Gubar'"};
        var ints = new int[] { -100, 2345, 19044, 888889};
        var dIn = new C11() 
        { 
          FBytes = bytes, 
          FC11 = new C11_1() { FBytes_1 = bytes, FStrings_1 = strings, FInts_1 = ints},
          FC12 = new C11_2() { FBytes_2 = bytes, FStrings_2 = strings, FInts_2 = ints}
        };  

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (C11)s.Deserialize(ms);

        Assert.AreSame(dOut.FBytes, dOut.FC11.FBytes_1);
        Assert.AreSame(dOut.FBytes, dOut.FC12.FBytes_2);

        Assert.AreSame(dOut.FC11.FStrings_1, dOut.FC12.FStrings_2);
        Assert.AreSame(dOut.FC11.FInts_1, dOut.FC12.FInts_2);
      }  
    }

    internal class C11
    {
      public C11_1 FC11 { get; set; }
      public C11_2 FC12 { get; set; }

      public byte[] FBytes { get; set; }
    }

    internal class C11_1
    {
      public byte[] FBytes_1 { get; set; }
      public string[] FStrings_1 { get; set; }
      public int[] FInts_1 { get; set; }
    }

    internal class C11_2
    {
      public int[] FInts_2 { get; set; }
      public byte[] FBytes_2 { get; set; }
      public string[] FStrings_2 { get; set; }
    }


    //[Test]
    //public void T09_CyclicStructs()
    //{
    //  using (var ms = new MemoryStream())
    //  {
    //    var s = new SlimSerializer();

    //    object s2 = new S2();
    //    object dIn = new S1() { FS2 = s2};
    //    ((S2)s2).FS1 = dIn;
        
    //    s.Serialize(ms, dIn);
    //    ms.Seek(0, SeekOrigin.Begin);

    //    var dOut = (S1)s.Deserialize(ms);

    //    Assert.AreEqual(dOut, ((S2)dOut.FS2).FS1);
    //  }
    //}

    //internal struct S1 
    //{
    //  public object FS2 { get; set; }
    //}

    //internal struct S2
    //{
    //  public object FS1 { get; set; }
    //}


    [Test]
    public void T10_CovariantStructInterfaceArrays()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();
        
        var dIn = new C12() { FIC12s = new IC12[] { new C12_2(), new C12_1() } };

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (C12)s.Deserialize(ms);

        Assert.IsInstanceOf<C12_2>(dOut.FIC12s[0]);
        Assert.IsInstanceOf<C12_1>(dOut.FIC12s[1]);
      }
    }

    internal struct C12
    {
      public IC12[] FIC12s { get; set; }
    }

    internal struct C12_1: IC12 {}

    internal struct C12_2: IC12 {}

    internal interface IC12 {}


    [Test]
    public void T11_DictComparer()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();
        
        var dIn = new Dictionary<int, int>(new Comparer1());
        dIn[1] = 100;
        dIn[11] = 200;
        dIn[2] = 300;

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (Dictionary<int, int>)s.Deserialize(ms);

        Assert.AreEqual(2, dOut.Count);

        //foreach (var kvp in dOut)
        //{
        //  Console.WriteLine(kvp.Key + "=>" + kvp.Value);
        //}

        Assert.AreEqual(200, dOut[1]);
        Assert.AreEqual(300, dOut[2]);

        dOut[1] = 100;

        Assert.AreEqual(2, dOut.Count);
        Assert.AreEqual(100, dOut[1]);
      }
    }

    internal class Comparer1: IEqualityComparer<int>
    {
      public bool Equals(int x, int y)
      {
        return (x % 10) == (y % 10);
      }

      public int GetHashCode(int obj)
      {
        return (obj % 10).GetHashCode();
      }
    }


    [Test]
    public void T12_DictComparerComplex()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var cmpr = new Comparer1();

        var c1 = new C13();
        var c2 = new C13();

        var dict1 = new Dictionary<int, C13>(cmpr);
        dict1[1] = c1;
        dict1[2] = c1;
        dict1[3] = c2;
        var dict2 = new Dictionary<int, C13>(cmpr);
        dict2[1] = c2;
        dict2[2] = c1;

        var dIn = new List<S3>();
        dIn.Add(new S3() { FDict = dict1 });
        dIn.Add(new S3() { FDict = dict1 });
        dIn.Add(new S3() { FDict = dict2 });

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (List<S3>)s.Deserialize(ms);

        Assert.AreSame(dOut[0].FDict, dOut[1].FDict);
        Assert.AreNotSame(dOut[0].FDict, dOut[2].FDict);

        Assert.AreSame(dOut[0].FDict[1], dOut[0].FDict[2]);
        Assert.AreNotSame(dOut[0].FDict[1], dOut[0].FDict[3]);

        Assert.AreSame(dOut[0].FDict[1], dOut[2].FDict[2]);
        Assert.AreSame(dOut[0].FDict[3], dOut[2].FDict[1]);
      } 
    }

    [Test]
    public void T13_Struct1_PilePointer()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var s1 = new NFX.ApplicationModel.Pile.PilePointer(10,231,223421);

        s.Serialize(ms, s1);
        ms.Seek(0, SeekOrigin.Begin);

        var s2 = (NFX.ApplicationModel.Pile.PilePointer)s.Deserialize(ms);

        Console.WriteLine( NFX.Serialization.JSON.JSONWriter.Write(s1));
        Console.WriteLine( NFX.Serialization.JSON.JSONWriter.Write(s2));

        Assert.IsTrue( s1 == s2);
      } 
    }

        private struct MyStructFields
        {
          public int X;
          public int Y;
          public bool F;
        }

        private struct MyStructWithReadonlyField
        {
          public MyStructWithReadonlyField(int x, int y, bool f)
          {
            A = 890;
            X = x;
            Y = y;
            F = f;
          }

          public readonly int A;
          public int X;
          public int Y;
          public readonly bool F;
        }


    [Test]
    public void T14_Struct2()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var s1 = new MyStructFields{ X = 10, Y=15, F = true};

        s.Serialize(ms, s1);
        ms.Seek(0, SeekOrigin.Begin);

        var s2 = (MyStructFields)s.Deserialize(ms);

       
        Assert.AreEqual(s1.X, s2.X);
        Assert.AreEqual(s1.Y, s2.Y);
        Assert.AreEqual(s1.F, s2.F);
      } 
    }

    [Test]
    public void T15_Struct3()
    {
      //MyStructWithReadonlyField z = new MyStructWithReadonlyField(1, 2, false);
      //object oz = z;
      //typeof(MyStructWithReadonlyField).GetField("F").SetValue(oz, true);
      //z = (MyStructWithReadonlyField)oz;
      //Console.WriteLine(z.F);
      //return;
                     
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var s1 = new MyStructWithReadonlyField(10, 15, true);

        s.Serialize(ms, s1);
        ms.Seek(0, SeekOrigin.Begin);

        var s2 = (MyStructWithReadonlyField)s.Deserialize(ms);

        Console.WriteLine( NFX.Serialization.JSON.JSONWriter.Write(s1));
        Console.WriteLine( NFX.Serialization.JSON.JSONWriter.Write(s2));

        Assert.AreEqual(s1.X, s2.X);
        Assert.AreEqual(s1.Y, s2.Y);
        Assert.AreEqual(s1.F, s2.F);
      } 
    }

       private class sw{ public MyStructWithReadonlyField s;}

    [Test]
    public void T16_Struct4()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var s1 = new sw{ s =  new MyStructWithReadonlyField(10, 15, true)};

        s.Serialize(ms, s1);
        ms.Seek(0, SeekOrigin.Begin);

        var s2 = (sw)s.Deserialize(ms);

        Console.WriteLine( NFX.Serialization.JSON.JSONWriter.Write(s1.s));
        Console.WriteLine( NFX.Serialization.JSON.JSONWriter.Write(s2.s));

        Assert.AreEqual(s1.s.X, s2.s.X);
        Assert.AreEqual(s1.s.Y, s2.s.Y);
        Assert.AreEqual(s1.s.F, s2.s.F);
      } 
    }



    [Test]
    public void T1000_CustomSerializableWithNesting()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();
        var o1 = new CustomSer{ Data= new object[]{1,2,true,"ok"} };
        s.Serialize(ms, o1);

        ms.Position = 0;

        var o2 = s.Deserialize(ms) as CustomSer;

        Assert.IsNotNull( o2 );

        Assert.AreEqual(4, ((object[])o2.Data).Length);
        Assert.AreEqual("ok", ((object[])o2.Data)[3]);
       
      }
    }
      



    internal struct S3
    {
      public Dictionary<int, C13> FDict { get; set; }
    }

    internal class C13 {}


           internal class CustomSer : ISerializable
           {
             public object Data;

             public CustomSer()
             {

             }

             public CustomSer(SerializationInfo info, StreamingContext context)
             {
               var buf = (byte[]) info.GetValue("d", typeof(byte[]));
               var ms = new MemoryStream(buf);
               var s = new SlimSerializer();
               Data = s.Deserialize(ms);    
             }
             
             public void GetObjectData(SerializationInfo info, StreamingContext context)
             {
               var ms = new MemoryStream();
               var s = new SlimSerializer();
               s.Serialize(ms, Data);
               info.AddValue("d", ms.GetBuffer(), typeof(byte[]));
             }
           }


  }
}
