using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using NUnit.Framework;

using NFX;
using NFX.Serialization.Slim;

namespace NFX.NUnit.Serialization
{
  [TestFixture]
  public class SlimBatchTypeMode
  {

      [Test]
      public void T1_TypeWasAdded()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer();
          Assert.AreEqual(TypeRegistryMode.PerCall, s1.TypeMode);
          Assert.IsTrue( s1.IsThreadSafe );
          Assert.IsFalse( s1.BatchTypesAdded );

          s1.TypeMode = TypeRegistryMode.Batch;
         
          Assert.AreEqual(TypeRegistryMode.Batch, s1.TypeMode);
          Assert.IsFalse( s1.IsThreadSafe );
          Assert.IsFalse( s1.BatchTypesAdded );
          

          var o1 = new A1{ I1 = 12};

          s1.Serialize(ms, o1);
          ms.Seek(0, SeekOrigin.Begin);

          var s2 = new SlimSerializer();
          s2.TypeMode = TypeRegistryMode.Batch;
          var o2 = (A1)s2.Deserialize(ms);

          Assert.AreEqual( 12, o2.I1);

          Assert.IsTrue( s1.BatchTypesAdded );
          Assert.IsTrue( s2.BatchTypesAdded );
        }
      }

       [Test]
      public void T1_ResetCallBatch()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer();
          s1.TypeMode = TypeRegistryMode.Batch;
         
          Assert.AreEqual(TypeRegistryMode.Batch, s1.TypeMode);
          Assert.IsFalse( s1.IsThreadSafe );
          Assert.IsFalse( s1.BatchTypesAdded );

          var o1 = new A1{ I1 = 12};

          s1.Serialize(ms, o1);

          Assert.IsTrue( s1.BatchTypesAdded );

          s1.ResetCallBatch();

          Assert.IsFalse( s1.BatchTypesAdded );
        }
      }

      [Test]
      public void T1_TypeWasNotAdded()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer( new Type[]{typeof(A1)});//put it in globals
          s1.TypeMode = TypeRegistryMode.Batch;
          Assert.IsFalse( s1.BatchTypesAdded );
          

          var o1 = new A1{ I1 = 12};

          s1.Serialize(ms, o1);
          ms.Seek(0, SeekOrigin.Begin);

          var s2 = new SlimSerializer(new Type[]{typeof(A1)}); 
          s2.TypeMode = TypeRegistryMode.Batch;
          var o2 = (A1)s2.Deserialize(ms);

          Assert.AreEqual( 12, o2.I1);

          Assert.IsFalse( s1.BatchTypesAdded );
          Assert.IsFalse( s2.BatchTypesAdded );
        }
      }




      [Test]
      [ExpectedException(typeof(SlimDeserializationException), ExpectedMessage="count mismatch", MatchType = MessageMatch.Contains)]
      public void T2_PerCall_CountMismatch()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer(new Type[]{typeof(A1)});
          var o1 = new A1{ I1 = 12};

          s1.Serialize(ms, o1);
          ms.Seek(0, SeekOrigin.Begin);

          var s2 = new SlimSerializer();
          var o2 = (A1)s2.Deserialize(ms);
        }
      }

      [Test]
      [ExpectedException(typeof(SlimDeserializationException), ExpectedMessage="CSUM mismatch", MatchType = MessageMatch.Contains)]
      public void T2_PerCall_CSUM_Mismatch()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer(new Type[]{typeof(A1)});
          var o1 = new A1{ I1 = 12};

          s1.Serialize(ms, o1);
          ms.Seek(0, SeekOrigin.Begin);

          var s2 = new SlimSerializer(new Type[]{typeof(A2)});
          var o2 = (A1)s2.Deserialize(ms);
        }
      }


      [Test]
      [ExpectedException(typeof(SlimDeserializationException), ExpectedMessage="count mismatch", MatchType = MessageMatch.Contains)]
      public void T3_Batch_CountMismatch()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer(new Type[]{typeof(A1)});
          s1.TypeMode= TypeRegistryMode.Batch;
          var o1 = new A1{ I1 = 12};

          s1.Serialize(ms, o1);
          ms.Seek(0, SeekOrigin.Begin);

          var s2 = new SlimSerializer();
          s2.TypeMode= TypeRegistryMode.Batch;
          var o2 = (A1)s2.Deserialize(ms);
        }
      }

      [Test]
      [ExpectedException(typeof(SlimDeserializationException), ExpectedMessage="CSUM mismatch", MatchType = MessageMatch.Contains)]
      public void T3_Batch_CSUM_Mismatch()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer(new Type[]{typeof(A1)});
          s1.TypeMode= TypeRegistryMode.Batch;
          var o1 = new A1{ I1 = 12};

          s1.Serialize(ms, o1);
          ms.Seek(0, SeekOrigin.Begin);

          var s2 = new SlimSerializer(new Type[]{typeof(A2)});
          s2.TypeMode= TypeRegistryMode.Batch;
          var o2 = (A1)s2.Deserialize(ms);
        }
      }


      [Test]
      public void T4_Batch_WriteMany()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer();
          s1.TypeMode= TypeRegistryMode.Batch;
          var o1a = new A1{ I1 = 12};      
          var o1b = new A2{ I2 = 13};      
          var o1c = new A1{ I1 = 14};      
          var o1d = new A1{ I1 = 15};      
          var o1e = new A1{ I1 = 16};      

          s1.Serialize(ms, o1a);  Assert.IsTrue( s1.BatchTypesAdded );
          s1.Serialize(ms, o1b);  Assert.IsTrue( s1.BatchTypesAdded );
          s1.Serialize(ms, o1c);  Assert.IsFalse( s1.BatchTypesAdded );
          s1.Serialize(ms, o1d);  Assert.IsFalse( s1.BatchTypesAdded );
          s1.Serialize(ms, o1e);  Assert.IsFalse( s1.BatchTypesAdded );
          ms.Seek(0, SeekOrigin.Begin);

          var buf = ms.GetBuffer();
          Console.WriteLine( buf.ToDumpString(DumpFormat.Printable, 0,(int) ms.Length) );

          var s2 = new SlimSerializer();
          s2.TypeMode= TypeRegistryMode.Batch;
          var o2a = (A1)s2.Deserialize(ms); Assert.IsTrue( s2.BatchTypesAdded );
          var o2b = (A2)s2.Deserialize(ms); Assert.IsTrue( s2.BatchTypesAdded );
          var o2c = (A1)s2.Deserialize(ms); Assert.IsFalse( s2.BatchTypesAdded );
          var o2d = (A1)s2.Deserialize(ms); Assert.IsFalse( s2.BatchTypesAdded );
          var o2e = (A1)s2.Deserialize(ms); Assert.IsFalse( s2.BatchTypesAdded );

          Assert.AreEqual(12, o2a.I1); 
          Assert.AreEqual(13, o2b.I2); 
          Assert.AreEqual(14, o2c.I1); 
          Assert.AreEqual(15, o2d.I1); 
          Assert.AreEqual(16, o2e.I1); 
        }
      }


      [Test]
      public void T5_Batch_WriteReadMany()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer();
          s1.TypeMode= TypeRegistryMode.Batch;
          var s2 = new SlimSerializer();
          s2.TypeMode= TypeRegistryMode.Batch;
          
          var o1a = new A1{ I1 = 12};      
          var o1b = new A2{ I2 = 13};      
          var o1c = new A1{ I1 = 14};      
          var o1d = new A1{ I1 = 15};      
          var o1e = new A1{ I1 = 16};      

          s1.Serialize(ms, o1a);  Assert.IsTrue( s1.BatchTypesAdded );
          ms.Position = 0;
          var o2a = (A1)s2.Deserialize(ms); Assert.IsTrue( s2.BatchTypesAdded );

          ms.Position = 0;
          s1.Serialize(ms, o1b);  Assert.IsTrue( s1.BatchTypesAdded );
          ms.Position = 0;
          var o2b = (A2)s2.Deserialize(ms); Assert.IsTrue( s2.BatchTypesAdded );

          ms.Position = 0;
          s1.Serialize(ms, o1c);  Assert.IsFalse( s1.BatchTypesAdded );
          ms.Position = 0;
          var o2c = (A1)s2.Deserialize(ms); Assert.IsFalse( s2.BatchTypesAdded );

          ms.Position = 0;
          s1.Serialize(ms, o1d);  Assert.IsFalse( s1.BatchTypesAdded );
          ms.Position = 0;
          var o2d = (A1)s2.Deserialize(ms); Assert.IsFalse( s2.BatchTypesAdded );
          
          ms.Position = 0;
          s1.Serialize(ms, o1e);  Assert.IsFalse( s1.BatchTypesAdded );
          ms.Position = 0;
          var o2e = (A1)s2.Deserialize(ms); Assert.IsFalse( s2.BatchTypesAdded );

          Assert.AreEqual(12, o2a.I1); 
          Assert.AreEqual(13, o2b.I2); 
          Assert.AreEqual(14, o2c.I1); 
          Assert.AreEqual(15, o2d.I1); 
          Assert.AreEqual(16, o2e.I1); 
        }
      }


      [Test]
      [ExpectedException(typeof(SlimDeserializationException), ExpectedMessage="count mismatch", MatchType = MessageMatch.Contains)]
      public void T6_BatchParcelMismatch_WriteMany()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer();
          s1.TypeMode= TypeRegistryMode.Batch;
          var o1a = new A1{ I1 = 12};      
          var o1b = new A2{ I2 = 13};      
          var o1c = new A1{ I1 = 14};      
          var o1d = new A1{ I1 = 15};      
          var o1e = new A1{ I1 = 16};      

          s1.Serialize(ms, o1a);  Assert.IsTrue( s1.BatchTypesAdded );
          s1.Serialize(ms, o1b);  Assert.IsTrue( s1.BatchTypesAdded );
          s1.Serialize(ms, o1c);  Assert.IsFalse( s1.BatchTypesAdded );
          s1.Serialize(ms, o1d);  Assert.IsFalse( s1.BatchTypesAdded );
          s1.Serialize(ms, o1e);  Assert.IsFalse( s1.BatchTypesAdded );
          ms.Seek(0, SeekOrigin.Begin);

          var buf = ms.GetBuffer();
          Console.WriteLine( buf.ToDumpString(DumpFormat.Printable, 0,(int) ms.Length) );

          var s2 = new SlimSerializer();
          s2.TypeMode= TypeRegistryMode.PerCall;//MISMATCH!!!
          var o2a = (A1)s2.Deserialize(ms);
          var o2b = (A2)s2.Deserialize(ms);//exception
         
        }
      }


      [Test]
      [ExpectedException(typeof(SlimDeserializationException), ExpectedMessage="count mismatch", MatchType = MessageMatch.Contains)]
      public void T7_CountMismatchResetBatch_WriteMany()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer();
          s1.TypeMode= TypeRegistryMode.Batch;
          var o1a = new A1{ I1 = 12};      
          var o1b = new A2{ I2 = 13};      
          var o1c = new A1{ I1 = 14};      
          var o1d = new A1{ I1 = 15};      
          var o1e = new A1{ I1 = 16};      

          s1.Serialize(ms, o1a);  Assert.IsTrue( s1.BatchTypesAdded );
          s1.Serialize(ms, o1b);  Assert.IsTrue( s1.BatchTypesAdded );
          s1.Serialize(ms, o1c);  Assert.IsFalse( s1.BatchTypesAdded );
          s1.Serialize(ms, o1d);  Assert.IsFalse( s1.BatchTypesAdded );
          s1.Serialize(ms, o1e);  Assert.IsFalse( s1.BatchTypesAdded );
          ms.Seek(0, SeekOrigin.Begin);

          var buf = ms.GetBuffer();
          Console.WriteLine( buf.ToDumpString(DumpFormat.Printable, 0,(int) ms.Length) );

          var s2 = new SlimSerializer();
          s2.TypeMode= TypeRegistryMode.Batch;
          var o2a = (A1)s2.Deserialize(ms); Assert.IsTrue( s2.BatchTypesAdded );
          Assert.AreEqual(12, o2a.I1); 

          s2.ResetCallBatch();
          var o2b = (A2)s2.Deserialize(ms); //Exception
        }
      }



      [Test]
      public void TZ9999_StringInVariousLanguages()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer();
          var o1 = new A3{ Text = "Hello 久有归天愿,Աեցեհի, Не менее 100 советских самолетов поднялись в воздух, asağıda yağız yer yaratıldıkta. I Agree!"};

          Console.WriteLine(o1.Text);

          s1.Serialize(ms, o1);
          ms.Seek(0, SeekOrigin.Begin);

          var s2 = new SlimSerializer();
          var o2 = (A3)s2.Deserialize(ms);

          Console.WriteLine(o2.Text);

          Assert.AreEqual(o1.Text, o2.Text);
        }
      }




      public class A1
      {
        public int I1;
      }

      public class A2
      {
        public int I2;
      }

      public class A3
      {
        public string Text;
      }


  }
   
}
