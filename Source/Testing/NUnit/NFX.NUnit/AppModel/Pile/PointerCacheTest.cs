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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
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
using NFX.Environment;


namespace NFX.NUnit.AppModel.Pile
{
  [TestFixture]
  public class PointerCacheTest
  {
      [TestCase(    100)]
      [TestCase( 250111)]
      public void BasicDurable(int COUNT)
      {
        using (var cache = PileCacheTestCore.MakeDurableCache())
        {
          var tA = cache.GetOrCreateTable<Guid>("A");

          Aver.AreObjectsEqual( CollisionMode.Durable, tA.CollisionMode );

          var dict = new Dictionary<Guid, PilePointer>();

          for(var i=0; i<COUNT; i++)
          {
            var key = Guid.NewGuid();
            var ptr = cache.Pile.Put(key.ToString(), preallocateBlockSize: 1024);
            dict.Add(key, ptr);

            Aver.IsTrue( PutResult.Inserted == tA.PutPointer(key, ptr) );
          }

          Aver.AreEqual( COUNT, tA.Count );

          foreach(var kvp in dict)
          {
            var gotPointer = tA.GetPointer(kvp.Key);
            Aver.IsTrue( gotPointer.Valid );

            Aver.AreEqual(gotPointer, kvp.Value);

            var gotObject = tA.Get(kvp.Key) as string;
            Aver.IsNotNull( gotObject );

            Aver.AreEqual( kvp.Key.ToString(), gotObject);
          }

          //because there is durable chaining, object count in pile is greater than in table
          Aver.IsTrue(COUNT <= cache.Pile.ObjectCount);

          foreach(var kvp in dict)
          {
            Aver.IsTrue( tA.Remove(kvp.Key) );
          }

          Aver.AreEqual(0, tA.Count );
          Aver.AreEqual(0, cache.Pile.ObjectCount);
        }
      }


      [TestCase(    100)]
      [TestCase( 250111)]
      public void BasicSpeculative(int COUNT)
      {
        using (var cache = PileCacheTestCore.MakeCache())
        {
          var tA = cache.GetOrCreateTable<Guid>("A");

          Aver.AreObjectsEqual( CollisionMode.Speculative, tA.CollisionMode );

          var dict = new Dictionary<Guid, PilePointer>();

          for(var i=0; i<COUNT; i++)
          {
            var key = Guid.NewGuid();
            var ptr = cache.Pile.Put(key.ToString(), preallocateBlockSize: 1024);
            dict.Add(key, ptr);

            var pr = tA.PutPointer(key, ptr);
            Aver.IsTrue( PutResult.Inserted == pr || PutResult.Overwritten == pr );
          }

          var ratio = tA.Count / (double)COUNT;
          Console.WriteLine(ratio);
          Aver.IsTrue( ratio > 0.85d );


          foreach(var kvp in dict)
          {
            var gotPointer = tA.GetPointer(kvp.Key);
            if (!gotPointer.Valid) continue;

            Aver.AreEqual(gotPointer, kvp.Value);

            var gotObject = tA.Get(kvp.Key) as string;
            Aver.IsNotNull( gotObject );

            Aver.AreEqual( kvp.Key.ToString(), gotObject);
          }

          foreach(var kvp in dict)
          {
            tA.Remove(kvp.Key);
          }

          Aver.AreEqual(0, tA.Count );
          Aver.AreEqual(0, cache.Pile.ObjectCount);
        }
      }



  }
}
