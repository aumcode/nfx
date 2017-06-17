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
  public class MMFPileLifecycleTests : MMFPileTestBase
  {

      [TestCase(true, 1 * 1024L * 1024L * 1024L,       64 * 1024 * 1024,  72, 72, 4)]

      [TestCase(true, 1 * 1024L * 1024L * 1024L,       64 * 1024 * 1024,  0, 16, 8)]//test small payloads

      //Various sizes
      [TestCase(true, 4 * 1024L * 1024L * 1024L,       64 * 1024 * 1024,  1, 1024, 4)]

      //Various sizes
      [TestCase(true, 4 * 1024L * 1024L * 1024L,       64 * 1024 * 1024,  1, 1024, 8)]

      //------------------------------------------------------------------------------

      [TestCase(false, 1 * 1024L * 1024L * 1024L,       64 * 1024 * 1024,  72, 72, 4)]
      [TestCase(false, 1 * 1024L * 1024L * 1024L,      128 * 1024 * 1024,  72, 72, 4)]
      [TestCase(false, 1 * 1024L * 1024L * 1024L, 1 * 1024 * 1024 * 1024,  72, 72, 4)]

      [TestCase(false, 1 * 1024L * 1024L * 1024L,       64 * 1024 * 1024,  0, 16, 8)]//test small payloads
      [TestCase(false, 1 * 1024L * 1024L * 1024L,       64 * 1024 * 1024,  0, 128, 8)]//test small payloads
      [TestCase(false, 1 * 1024L * 1024L * 1024L,       64 * 1024 * 1024,  0, 256, 8)]//test small payloads

      //Various sizes
      [TestCase(false, 4 * 1024L * 1024L * 1024L,       64 * 1024 * 1024,  1, 1024, 4)]
      [TestCase(false, 4 * 1024L * 1024L * 1024L,      128 * 1024 * 1024,  1, 1024, 4)]
      [TestCase(false, 4 * 1024L * 1024L * 1024L, 1 * 1024 * 1024 * 1024,  1, 1024, 4)]

      //Various sizes
      [TestCase(false, 4 * 1024L * 1024L * 1024L,       64 * 1024 * 1024,  1, 1024, 8)]
      [TestCase(false, 4 * 1024L * 1024L * 1024L,      128 * 1024 * 1024,  1, 1024, 8)]
      [TestCase(false, 4 * 1024L * 1024L * 1024L, 1 * 1024 * 1024 * 1024,  1, 1024, 8)]
      public void StartStopStart(bool speed, long totalSize, int segmentSize, int fromSize, int toSize, int workers)
      {
        using (var pile = MakeMMFPile())
        {
          pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
          pile.SegmentSize = segmentSize;
          pile.Start();

          var tasks = new Task[workers];

          for(var i=0; i<workers; i++)
           tasks[i] = Task.Factory.StartNew( () =>
           {
              while(pile.UtilizedBytes < totalSize)
              {
                var sz = fromSize==toSize ? toSize : ExternalRandomGenerator.Instance.NextScaledRandomInteger(fromSize, toSize);
                var load = new byte[sz];
                pile.Put(load);
              }
           });

          Task.WaitAll(tasks);

          var objectCount = pile.ObjectCount;
          var totalBytes = pile.UtilizedBytes;
          var overheadBytes = pile.OverheadBytes;
          var segmentCount = pile.SegmentCount;
          //pile.ObjectLinkCount

          Console.WriteLine("Inserted Objects: {0:n0} Utilized: {1:n0} Overhead: {2:n0} Segments: {3:n0}".Args(objectCount, totalBytes, overheadBytes, segmentCount));
          Console.WriteLine("Waiting for complete stop");
          //--------------------------------------
          pile.WaitForCompleteStop();//stop it

          Console.WriteLine("Stopped. Starting...");

          pile.Start();//read the objects back in RAM

          Console.WriteLine("Started. Waiting for completely loaded...");
          while(!pile.CompletelyLoaded)
          {
            Thread.Sleep(1000);
            Console.WriteLine("{0} - Loaded {1:n0} objects, {2:n0} bytes,  = {3:n0}%".Args(
                                                                                  DateTime.UtcNow,
                                                                                  pile.ObjectCount,
                                                                                  pile.UtilizedBytes,
                                                                                  (int)(100 * (pile.ObjectCount / (float)objectCount))
                                                                                  ));
          }
          Console.WriteLine("Pile is completely loaded");

          Console.WriteLine("Loaded Objects: {0:n0} Utilized: {1:n0} Overhead: {2:n0} Segments: {3:n0}".Args(pile.ObjectCount,
                                                                                                             pile.UtilizedBytes,
                                                                                                             pile.OverheadBytes,
                                                                                                             pile.SegmentCount));
          Aver.AreEqual( objectCount   , pile.ObjectCount );
          Aver.AreEqual( totalBytes    , pile.UtilizedBytes );
          Aver.AreEqual( overheadBytes , pile.OverheadBytes );
          Aver.AreEqual( segmentCount  , pile.SegmentCount );
        }
      }


      [Test]
      public void MutateAfterRestart()
      {
        using (var pile = MakeMMFPile())
        {
        }
      }

  }
}
