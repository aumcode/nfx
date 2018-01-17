/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
using NUnit.Framework;

using System.Threading;
using System.Threading.Tasks;

using NFX.Collections;
using System.Collections;

namespace NFX.NUnit.Collections
{

  [TestFixture]
  public class CappedSetTests
  {
    [TestCase]
    public void Basic_NoComparer()
    {
      using(var set = new CappedSet<string>())
      {
        Aver.IsTrue( set.Put("Gagarin") );
        Aver.IsTrue( set.Put("Titov") );
        Aver.IsTrue( set.Put("Glenn") );

        Aver.IsTrue( set.Contains("Glenn") );
        Aver.IsFalse( set.Contains("GLENN") );

        Aver.IsFalse( set.Put("Titov") );
        Aver.AreEqual(3, set.Count );
        Aver.AreEqual(3, set.ToArray().Length );

        DateTime cd;
        Aver.IsTrue( set.Get("Titov", out cd) );
        Aver.IsTrue( (App.TimeSource.UTCNow - cd).TotalSeconds < 2d);//unless machine freezes :(

        Aver.IsFalse( set.Get("Neverflew", out cd) );

        set.Clear();

        Aver.AreEqual(0, set.Count );
        Aver.AreEqual(0, set.ToArray().Length );

        Aver.IsTrue( set.Put("Gagarin") );
        Aver.IsTrue( set.Put("GAGARIN") );

        Aver.IsFalse( set.Put("Gagarin") );
        Aver.IsTrue( set.Remove("Gagarin") );
        Aver.IsTrue( set.Put("Gagarin") );
      }
    }

    [TestCase]
    public void Basic_Comparer()
    {
      using(var set = new CappedSet<string>(StringComparer.OrdinalIgnoreCase))
      {
        Aver.IsTrue( set.Put("Gagarin") );
        Aver.IsTrue( set.Put("Titov") );
        Aver.IsTrue( set.Put("Glenn") );

        Aver.IsTrue( set.Contains("Glenn") );
        Aver.IsTrue( set.Contains("GLENN") );

        Aver.IsFalse( set.Put("Titov") );
        Aver.AreEqual(3, set.Count );
        Aver.AreEqual(3, set.ToArray().Length );

        DateTime cd;
        Aver.IsTrue( set.Get("Titov", out cd) );
        Aver.IsTrue( (App.TimeSource.UTCNow - cd).TotalSeconds < 2d);//unless machine freezes :(

        Aver.IsFalse( set.Get("Neverflew", out cd) );

        set.Clear();

        Aver.AreEqual(0, set.Count );
        Aver.AreEqual(0, set.ToArray().Length );

        Aver.IsTrue( set.Put("Gagarin") );
        Aver.IsFalse( set.Put("GAGARIN") );

        Aver.IsTrue( set.Remove("Gagarin") );
        Aver.IsTrue( set.Put("Gagarin") );
      }
    }


    //The test requires    CappedSet.THREAD_GRANULARITY_MS to be around 5000ms
    [TestCase]
    public void Max_Age()
    {
      using(var set = new CappedSet<int>())
      {
        set.TimeLimitSec = 10;

        for(var i=0; i<1000; i++)
         set.Put(i);

        Assert.AreEqual(1000, set.Count);
        for(var i=0; i<40; i++)
        {
          Console.WriteLine(" count: {0}".Args(set.Count));
          Thread.Sleep(1000);
        }
        Assert.AreEqual(0, set.Count);

        Assert.IsFalse(set.Any());
      }
    }

    //The test requires    CappedSet.THREAD_GRANULARITY_MS to be around 5000ms
    [TestCase]
    public void Max_Size()
    {
      using(var set = new CappedSet<int>())
      {
        set.SizeLimit = 10000;

        for(var i=0; i<150000; i++)
         set.Put(i);

        for(var i=0; i<40; i++)
        {
          Console.WriteLine(" count: {0}".Args(set.Count));
          Thread.Sleep(1000);
        }
        Aver.IsTrue(set.Count <= (set.SizeLimit + 1024));//1024 margin of error
      }
    }

    //The test requires    CappedSet.THREAD_GRANULARITY_MS to be around 5000ms
    [TestCase]
    public void Max_SizeandTime()
    {
      using(var set = new CappedSet<int>())
      {
        set.TimeLimitSec = 30;//
        set.SizeLimit = 7000;

        for(var i=0; i<150000; i++)
         set.Put(i);

        for(var i=0; i<40; i++)
        {
          Console.WriteLine(" count: {0}".Args(set.Count));
          Thread.Sleep(1000);
        }
        Assert.AreEqual(0 , set.Count);
      }
    }



    [TestCase]
    public void Mutithreaded()
    {
      using(var set = new CappedSet<int>())
      {
        set.TimeLimitSec = 30;//
        set.SizeLimit = 7000;

        Parallel.For(0, 1500000, (i) => { set.Put(i); set.Contains(i); });

        for(var i=0; i<40; i++)
        {
          Console.WriteLine(" count: {0}".Args(set.Count));
          Thread.Sleep(1000);
        }
        Assert.AreEqual(0, set.Count);
      }
    }


  }
}
