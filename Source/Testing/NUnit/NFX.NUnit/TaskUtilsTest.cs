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
    public void ThenTest_SimpleChain()
    {
      bool a = false, b = false;

      Task.Factory.StartNew(() => { a = true; }).Then(() => { b = true; }).Wait();

      Assert.IsTrue(a);
      Assert.IsTrue(b);
    }

    [Test]
    public void ThenTest_ThrowEx()
    {
      bool a = false, b = false;

      var tInitial = new Task(() => { a = true; throw new Exception("ex A"); });
      var tThen = tInitial.Then(() => { b = true; });
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
  }
}
