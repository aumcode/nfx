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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NFX.NUnit
{
    [TestFixture]
    public class SealedStringTests
    {
        [TestCase]
        public void Unassigned()
        {
          var empty = new SealedString();

          Assert.IsFalse(empty.IsAssigned);

          empty = SealedString.Unassigned;

          Assert.IsFalse(empty.IsAssigned);

          Assert.AreEqual(SealedString.Unassigned, new SealedString());
          Assert.AreEqual(0, SealedString.Unassigned.GetHashCode());

          Assert.IsTrue( null == empty.Value);
        }

        [TestCase]
        public void Create()
        {
          var s1 = new SealedString("Lenin");
          
          var original = s1.Value;

          Assert.IsTrue(s1.IsAssigned);
          Assert.AreEqual("Lenin", original);
          Console.WriteLine(s1.ToString());
        }

        [TestCase]
        public void Equals()
        {
          var s1 = new SealedString("Bird");
          var s2 = new SealedString("Cat");
          
          
          Assert.IsTrue(s1.IsAssigned);
          Assert.IsTrue(s2.IsAssigned);
          Assert.AreEqual("Bird", s1.Value);
          Assert.AreEqual("Cat", s2.Value);

          Assert.AreNotEqual(s1, s2);
          Assert.IsFalse( s1 == s2);
          Assert.IsTrue( s1 != s2);
          Assert.AreNotEqual(s1.GetHashCode(), s2.GetHashCode());

          var s3 = SealedString.Unassigned;
          Assert.AreNotEqual(s1, s3);
          Assert.IsFalse( s1 == s3);
          Assert.IsTrue( s1 != s3);
          Assert.AreNotEqual(s1.GetHashCode(), s3.GetHashCode());

          s3 = s1;
          Assert.AreEqual(s1, s3);
          Assert.IsTrue( s1 == s3);
          Assert.IsFalse( s1 != s3);
          Assert.AreEqual(s1.GetHashCode(), s3.GetHashCode());
        }

        [TestCase]
        public void Counts()
        {
          const int cnt = 512000;

          var startCount = SealedString.TotalCount;
          var startUseCount = SealedString.TotalBytesUsed;
          var startAllocCount = SealedString.TotalBytesAllocated;


          Console.WriteLine("Total: {0:n0} / used bytes: {1:n0} / allocated: {2:n0}", startCount, startUseCount, startAllocCount);

          var sw = System.Diagnostics.Stopwatch.StartNew();
            for(var i=0; i<cnt; i++) new SealedString("String content that is not very short but not very long either");
          sw.Stop();

          var endCount = SealedString.TotalCount;
          var endUseCount = SealedString.TotalBytesUsed;
          var endAllocCount = SealedString.TotalBytesAllocated;
          
          Assert.AreEqual( startCount+cnt, endCount);
          Assert.IsTrue(endUseCount > startUseCount);
          Assert.IsTrue(endAllocCount >= startAllocCount);

          Console.WriteLine("Total: {0:n0} / used bytes: {1:n0} / allocated: {2:n0}", endCount, endUseCount, endAllocCount);

          Console.WriteLine("Did {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(cnt, sw.ElapsedMilliseconds, cnt / (sw.ElapsedMilliseconds / 1000d)));

          Console.WriteLine("Total segments: {0}", SealedString.TotalSegmentCount);
        }


        [TestCase(10123456,      3,     10)]
        [TestCase(  125000,  1024,   32000)]
        public void Multithreaded(int cnt, int from, int to)
        {
        
          var startCount = SealedString.TotalCount;
          var startUseCount = SealedString.TotalBytesUsed;
          var startAllocCount = SealedString.TotalBytesAllocated;

          var data = new string[1024];
          for(var i=0; i<data.Length; i++)
           data[i] = "A".PadRight(from+NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, to));


          Console.WriteLine("Total: {0:n0} / used bytes: {1:n0} / allocated: {2:n0}", startCount, startUseCount, startAllocCount);

          var sw = System.Diagnostics.Stopwatch.StartNew();
            Parallel.For(0, cnt, (_) => 
            {
               var content = data[NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, data.Length)];
               var s = new SealedString(content); 
               var restored = s.Value;
               Assert.AreEqual(content, restored);
            });
          sw.Stop();

          var endCount = SealedString.TotalCount;
          var endUseCount = SealedString.TotalBytesUsed;
          var endAllocCount = SealedString.TotalBytesAllocated;
          
          Assert.AreEqual( startCount+cnt, endCount);
          Assert.IsTrue(endUseCount > startUseCount);
          Assert.IsTrue(endAllocCount >= startAllocCount);

          Console.WriteLine("Total: {0:n0} / used bytes: {1:n0} / allocated: {2:n0}", endCount, endUseCount, endAllocCount);

          Console.WriteLine("Did {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(cnt, sw.ElapsedMilliseconds, cnt / (sw.ElapsedMilliseconds / 1000d)));

          Console.WriteLine("Total segments: {0}", SealedString.TotalSegmentCount);
        }

        [TestCase]
        public void Scope_Insensitive()
        {
          var scope = new SealedString.Scope();

          var s1 = scope.Seal("Lenin");
          var s2 = scope.Seal("has");
          var s3 = scope.Seal("LeNIN");
          var s4 = scope.Seal("LeNeN");

          Assert.AreEqual(s1, s3);
          Assert.AreNotEqual(s1, s2);
          Assert.AreNotEqual(s1, s4);

          Assert.AreEqual(3, scope.Count);
          Assert.AreEqual(s1, scope["LENIN"]);
          Assert.AreEqual(s2, scope["HAS"]);
          Assert.AreEqual(s4, scope["LENEN"]);
        }

        [TestCase]
        public void Scope_Sensitive()
        {
          var scope = new SealedString.Scope(StringComparer.InvariantCulture);

          var s1 = scope.Seal("Lenin");
          var s2 = scope.Seal("has");
          var s3 = scope.Seal("LeNIN");
          var s4 = scope.Seal("LeNeN");

          Assert.AreNotEqual(s1, s3);
          Assert.AreNotEqual(s1, s2);
          Assert.AreNotEqual(s1, s4);

          Assert.AreEqual(4, scope.Count);
          Assert.AreEqual(s1, scope["Lenin"]);
          Assert.AreEqual(s2, scope["has"]);
          Assert.AreEqual(s3, scope["LeNIN"]);
          Assert.AreEqual(s4, scope["LeNeN"]);

          Assert.AreEqual(SealedString.Unassigned, scope["LENIN"]);
        }

    }
}
