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
using StreamHelpers = NFX.Web.MultiPartContent.StreamHelpers;
using NUnit.Framework;

namespace NFX.NUnit.Web.MultiPart
{
  [TestFixture]
  public class CopyToTest
  {
    [Test]
    public void CopyTo_Empty2Empty()
    {
      using (MemoryStream src = new MemoryStream(), dst = new MemoryStream())
      {
        StreamHelpers.CopyTo(src, dst, 0, src.Length);
        Assert.AreEqual(0, dst.Length);
      }
    }

    [Test]
    public void CopyTo_SingleWithEmptyLength()
    {
      using (MemoryStream src = new MemoryStream(new byte[] {0}), dst = new MemoryStream())
      {
        StreamHelpers.CopyTo(src, dst, 0, -1);
        Assert.AreEqual(0, dst.Length);
      }
    }

    [Test]
    public void CopyTo_Single2Single()
    {
      using (MemoryStream src = new MemoryStream(new byte[] {0}), dst = new MemoryStream())
      {
        StreamHelpers.CopyTo(src, dst, 0, 0);
        Assert.AreEqual(1, dst.Length);
      }
    }

    [Test]
    public void CopyTo_Multi2Multi()
    {
      using (MemoryStream src = new MemoryStream(new byte[] {0, 1, 2, 3}), dst = new MemoryStream())
      {
        StreamHelpers.CopyTo(src, dst, 0, 4);
        Assert.AreEqual(4, dst.Length);
      }
    }

    [Test]
    public void CopyTo_MultiWithLast()
    {
      using (MemoryStream src = new MemoryStream(new byte[] {0, 1, 2, 3, 5}), dst = new MemoryStream())
      {
        StreamHelpers.CopyTo(src, dst, 0, 2);
        Assert.AreEqual(3, dst.Length);
        Assert.AreEqual(0, dst.GetBuffer()[0]);
        Assert.AreEqual(1, dst.GetBuffer()[1]);
        Assert.AreEqual(2, dst.GetBuffer()[2]);
      }
    }

    [Test]
    public void CopyTo_MultiWithFirstLast()
    {
      using (MemoryStream src = new MemoryStream(new byte[] {0, 1, 2, 3, 5}), dst = new MemoryStream())
      {
        StreamHelpers.CopyTo(src, dst, 1, 3);
        Assert.AreEqual(3, dst.Length);
        Assert.AreEqual(1, dst.GetBuffer()[0]);
        Assert.AreEqual(2, dst.GetBuffer()[1]);
        Assert.AreEqual(3, dst.GetBuffer()[2]);
      }
    }

    [Test]
    public void CopyTo_MultiWithFirstLast2NonEmpty()
    {
      var dstBytes = new byte[] {6, 7, 8, 9, 10, 11};
      using (MemoryStream src = new MemoryStream(new byte[] {0, 1, 2, 3, 4}), dst = new MemoryStream(dstBytes))
      {
        StreamHelpers.CopyTo(src, dst, 1, 4);
        Assert.AreEqual(6, dst.Length);
        Assert.AreEqual(1, dstBytes[0]);
        Assert.AreEqual(2, dstBytes[1]);
        Assert.AreEqual(3, dstBytes[2]);
        Assert.AreEqual(4, dstBytes[3]);
        Assert.AreEqual(10, dstBytes[4]);
        Assert.AreEqual(11, dstBytes[5]);
      }
    }
  }
}
