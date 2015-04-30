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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX;
using NFX.ApplicationModel;
using NFX.ApplicationModel.Pile;
using NFX.DataAccess;
using NFX.DataAccess.Distributed;
using NFX.IO;
using NFX.Serialization.Slim;

namespace NFX.NUnit.AppModel.Pile
{
  [TestFixture]
  public class PileTest
  {
    #region Tests

      [Test]
      public void Initial()
      {
        using (var pile = new DefaultPile())
        {
          var ipile = pile as IPile;

          Assert.AreEqual(0, ipile.ObjectCount);
          Assert.AreEqual(0, ipile.AllocatedMemoryBytes);
          Assert.AreEqual(0, ipile.UtilizedBytes);
          Assert.AreEqual(0, ipile.OverheadBytes);
          Assert.AreEqual(0, ipile.SegmentCount);
        }
      }

      [Test]
      public void PutWOStart()
      {
        using (var pile = new DefaultPile())
        {
          var ipile = pile as IPile;

          var row = Charge.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(row);

          Assert.IsFalse(pp.Valid);

          Assert.AreEqual(0, ipile.ObjectCount);
          Assert.AreEqual(0, ipile.AllocatedMemoryBytes);
          Assert.AreEqual(0, ipile.UtilizedBytes);
          Assert.AreEqual(0, ipile.OverheadBytes);
          Assert.AreEqual(0, ipile.SegmentCount);
        }
      }

      [Test]
      public void GetWOStart()
      {
        using (var pile = new DefaultPile())
        {
          var ipile = pile as IPile;
          var obj = ipile.Get(PilePointer.Invalid);
          Assert.IsNull(obj);
        }
      }

      [Test]
      public void PutOne()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var ipile = pile as IPile;

          var row = Charge.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(row);

          Assert.AreEqual(1, ipile.ObjectCount);
          Assert.AreEqual(DefaultPile.SEG_SIZE_DFLT, ipile.AllocatedMemoryBytes);
          Assert.AreEqual(1, ipile.SegmentCount);
        }
      } 

      [Test]
      public void PutTwo()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var ipile = pile as IPile;

          var rowIn1 = Charge.MakeFake(new GDID(0, 1));
          var rowIn2 = Charge.MakeFake(new GDID(0, 2));

          var pp1 = ipile.Put(rowIn1);
          var pp2 = ipile.Put(rowIn2);

          Assert.AreEqual(2, ipile.ObjectCount);
          Assert.AreEqual(DefaultPile.SEG_SIZE_DFLT, ipile.AllocatedMemoryBytes);
          Assert.AreEqual(1, ipile.SegmentCount);

          var rowOut1 = pile.Get(pp1) as Charge;
          var rowOut2 = pile.Get(pp2) as Charge;

          Assert.AreEqual(rowIn1, rowOut1);
          Assert.AreEqual(rowIn2, rowOut2);
        }
      } 

      [Test]
      public void PutGetObject()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          var rowIn = Charge.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(rowIn);

          var rowOut = ipile.Get(pp) as Charge;

          Assert.AreEqual(rowIn, rowOut);
        }
      } 


      [Test]
      [ExpectedException(typeof(PileAccessViolationException))]
      public void GetNoObject()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;
          ipile.Get(PilePointer.Invalid);
        }
      } 

      [Test]
      [ExpectedException(typeof(PileAccessViolationException))]
      public void DeleteInvalid()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;
          ipile.Delete(PilePointer.Invalid);
        }
      } 

      [Test]
      [ExpectedException(typeof(PileAccessViolationException))]
      public void DeleteExisting()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          var rowIn = Charge.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(rowIn);

          ipile.Delete(pp);

          Assert.AreEqual(0, ipile.ObjectCount);
          Assert.AreEqual(0, ipile.AllocatedMemoryBytes);
          Assert.AreEqual(0, ipile.UtilizedBytes);
          Assert.AreEqual(0, ipile.OverheadBytes);
          Assert.AreEqual(0, ipile.SegmentCount);

          var rowOut = ipile.Get(pp);
        }
      } 

      [Test]
      [ExpectedException(typeof(PileAccessViolationException))]
      public void Purge()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          var rowIn = Charge.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(rowIn);

          ipile.Purge();

          Assert.AreEqual(0, ipile.ObjectCount);
          Assert.AreEqual(0, ipile.SegmentCount);

          var rowOut = ipile.Get(pp);


        }
      } 

      [Test]
      public void PutCheckerboardPattern2()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          const ulong CNT = 100;

          var ppp = new Tuple<PilePointer, Charge>[CNT];

          for (ulong i = 0; i < CNT; i++)
          {
            var ch = Charge.MakeFake(new GDID(0, i));
            ppp[i] = new Tuple<PilePointer,Charge>( ipile.Put(ch), ch);
          }

          Assert.AreEqual(CNT, ipile.ObjectCount);

          for(ulong i = 0; i < CNT; i++)
          {
            var ch = ipile.Get(ppp[i].Item1);
            Assert.AreEqual(ch, ppp[i].Item2);
          }

          for(ulong i = 0; i < CNT; i+=2)
            ipile.Delete(ppp[i].Item1);

          Assert.AreEqual(CNT/2, ipile.ObjectCount);

          for(ulong i = 0; i < CNT; i++)
          {
            if (i % 2 == 0)
            {
              try
              {
                ipile.Get(ppp[i].Item1);
                Assert.Fail("Object is deleted but its pointer doesn't throw exception!");
              }
              catch (PileAccessViolationException) {}
            }
            else
            {
              var ch = ipile.Get(ppp[i].Item1);
              Assert.AreEqual(ch, ppp[i].Item2);
            }
          }
        }  
      }

      [Test]
      public void PutCheckerboardPattern3()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          const ulong CNT = 123;

          var ppp = new Tuple<PilePointer, string>[CNT];

          for (ulong i = 0; i < CNT; i++)
          {
            var str = NFX.Parsing.NaturalTextGenerator.Generate(179);
            ppp[i] = new Tuple<PilePointer,string>( ipile.Put(str), str);
          }

          Assert.AreEqual(CNT, ipile.ObjectCount);

          for(ulong i = 0; i < CNT; i++)
          {
            if (i % 3 != 0)
              ipile.Delete(ppp[i].Item1);
          }

          Assert.AreEqual(CNT/3, ipile.ObjectCount);

          for(ulong i = 0; i < CNT; i++)
          {
            if (i % 3 != 0)
            {
              try
              {
                ipile.Get(ppp[i].Item1);
                Assert.Fail("Object is deleted but its pointer doesn't throw exception!");
              }
              catch (PileAccessViolationException) {}
            }
            else
            {
              var ch = ipile.Get(ppp[i].Item1);
              Assert.AreEqual(ch, ppp[i].Item2);
            }
          }

          ////Console.WriteLine("ObjectCount: {0}", ipile.ObjectCount);
          ////Console.WriteLine("AllocatedMemoryBytes: {0}", ipile.AllocatedMemoryBytes);
          ////Console.WriteLine("UtilizedBytes: {0}", ipile.UtilizedBytes);
          ////Console.WriteLine("OverheadBytes: {0}", ipile.OverheadBytes);
          ////Console.WriteLine("SegmentCount: {0}", ipile.SegmentCount);
        }  
      }

    #endregion
  }
}
