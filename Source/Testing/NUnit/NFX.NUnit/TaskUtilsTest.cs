/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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
using System.Threading.Tasks;

using NUnit.Framework;
using NFX.IO;

namespace NFX.NUnit
{
  [TestFixture]
  public class TaskUtilsTest
  {
    [Test]
    public void OnOkTest_SimpleChain()
    {
      bool a = false, b = false;

      Task.Factory.StartNew(() => { a = true; }).OnOk(() => { b = true; }).Wait();

      Assert.IsTrue(a);
      Assert.IsTrue(b);
    }

    [Test]
    public void OnOkTest_ThrowEx()
    {
      bool a = false, b = false;

      var tInitial = new Task(() => { a = true; throw new Exception("ex A"); });
      var tThen = tInitial.OnOk(() => { b = true; });
      try
      {
        tInitial.Start();
        tThen.Wait();
        Assert.Fail();
      }
      catch (AggregateException ex)
      {
        Assert.AreEqual(1, ex.InnerExceptions.Count);
        Assert.AreEqual("ex A", ex.InnerExceptions[0].Message);
        Assert.IsTrue(a);
        Assert.IsFalse(b);
      }
    }

    [Test]
    public void OnOkTest_ThrowExCascade()
    {
      bool a = false, b = false, c=false;

      var tInitial = new Task(() => { a = true; throw new Exception("ex A"); });
      var tThen1 = tInitial.OnOk(() => { b = true; });
      var tThen2 = tThen1.OnOk(() => { c = true; });
      try
      {
        tInitial.Start();
        tThen2.Wait();
        Assert.Fail();
      }
      catch (AggregateException ex)
      {
        Assert.AreEqual(1, ex.InnerExceptions.Count);
        Assert.AreEqual("ex A", ex.InnerExceptions[0].Message);
        Assert.IsTrue(a);
        Assert.IsFalse(b);
        Assert.IsFalse(c);
      }
    }

    [Test]
    public void OnError_ThrowEx()
    {
      bool a = false, b = false, c = false;

      var tInitial = new Task(() => { a = true; throw new Exception("ex A"); });
      var tThen = tInitial.OnOk(() => { b = true; });
      var tOnFailed = tThen.OnError(() => { c = true; });
      try
      {
        tInitial.Start();
        tOnFailed.Wait();
        Assert.Fail();
      }
      catch (AggregateException)
      {
        Assert.IsTrue(a);
        Assert.IsFalse(b);
        Assert.IsTrue(c);
      }
    }

    [Test]
    public void OnOkOrError_ThrowEx()
    {
      bool a = false, b = false, c = false;

      var tInitial = new Task(() => { a = true; throw new Exception("ex A"); });
      var tThen = tInitial.OnOk(() => { b = true; });
      var tOnFinally = tThen.OnOkOrError(t => { c = true; });
      try
      {
        tInitial.Start();
        tOnFinally.Wait();
        Assert.Fail();
      }
      catch (AggregateException)
      {
        Assert.IsTrue(a);
        Assert.IsFalse(b);
        Assert.IsTrue(c);
      }
    }
  }
}
