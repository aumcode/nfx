/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using System.Diagnostics;
using System.Threading;
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
  public class PileEnumerationTest
  {
      [SetUp]
      public void SetUp()
      {
        GC.Collect();
      }

      [TearDown]
      public void TearDown()
      {
        GC.Collect();
      }


      [TestCase(723, 64 * 1024 * 1024)]//count < 1024
      [TestCase(1500, 64 * 1024 * 1024)]//1 segment
      [TestCase(250000, 64 * 1024 * 1024)]
      [TestCase(750000, 64 * 1024 * 1024)]
      public void Buffers(int count, int segmentSize)
      {
        using (var pile = new DefaultPile())
        {
          pile.SegmentSize = segmentSize;
          pile.Start();

          var hs = new HashSet<int>();

          for(var i=0; i<count; i++)
          {
            var buf = new byte[4 + (i%512)];
            buf.WriteBEInt32(i);
            pile.Put(buf);
            hs.Add(i);
          }

          Console.WriteLine("Created {0} segments".Args(pile.SegmentCount));

          var j = 0;
          foreach(var entry in pile)
          {
            var buf = pile.Get(entry.Pointer) as byte[];
            Aver.IsNotNull(buf);
            Aver.IsTrue(buf.Length>=4);
            var i = buf.ReadBEInt32();
            Aver.IsTrue(hs.Remove(i));
            Aver.IsTrue( entry.Type == PileEntry.DataType.Buffer );
            j++;
          }
          Aver.AreEqual(j, count);
          Aver.AreEqual(0, hs.Count);
        }
      }
  }
}
