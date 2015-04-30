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
using System.IO;
using System.Linq;
using StreamHelpers = NFX.Web.MultiPartContent.StreamHelpers;
using NUnit.Framework;

namespace NFX.NUnit.Web.MultiPart
{
  [TestFixture]
  public class SplitTest
  {
    #region Public

      [Test]
      public void SplitEmpty()
      {
        Assert.AreEqual(0, split(new byte[] { }, new byte[] { }).Length);
        Assert.AreEqual(0, split(new byte[] { }, new byte[] { 1 }).Length);
      }

      [Test]
      public void SplitWOMatch()
      {
        {
          var res = split(new byte[] { 2 }, new byte[] { });
          Assert.AreEqual(1, res.Length);
          Assert.AreEqual(0, res[0].Item1);
          Assert.AreEqual(0, res[0].Item2);
        }

        {
          var res = split(new byte[] { 1, 2 }, new byte[] { });
          Assert.AreEqual(1, res.Length);
          Assert.AreEqual(0, res[0].Item1);
          Assert.AreEqual(1, res[0].Item2);
        }
      }

      [Test]
      public void SplitWSingleEndMatch()
      {
        {
          var res = split(new byte[] {1, 2}, new byte[] {2});
          Assert.AreEqual(1, res.Length);
          Assert.AreEqual(0, res[0].Item1);
          Assert.AreEqual(0, res[0].Item2);
        }

        {
          var res = split(new byte[] {1, 2, 3, 4}, new byte[] {3, 4});
          Assert.AreEqual(1, res.Length);
          Assert.AreEqual(0, res[0].Item1);
          Assert.AreEqual(1, res[0].Item2);
        }
      }

      [Test]
      public void SplitWSingleStartMatch()
      {
        {
          var res = split(new byte[] {1, 2}, new byte[] {1});
          Assert.AreEqual(1, res.Length);
          Assert.AreEqual(1, res[0].Item1);
          Assert.AreEqual(1, res[0].Item2);
        }

        {
          var res = split(new byte[] {1, 2, 3, 4}, new byte[] {3, 4});
          Assert.AreEqual(1, res.Length);
          Assert.AreEqual(0, res[0].Item1);
          Assert.AreEqual(1, res[0].Item2);
        }
      }

      [Test]
      public void SplitWSingleMiddleMatch()
      {
        {
          var res = split(new byte[] {1, 2, 3}, new byte[] {2});
          Assert.AreEqual(2, res.Length);
          Assert.AreEqual(0, res[0].Item1);
          Assert.AreEqual(0, res[0].Item2);
          Assert.AreEqual(2, res[1].Item1);
          Assert.AreEqual(2, res[1].Item2);
        }

        {
          var res = split(new byte[] {1, 2, 3, 4}, new byte[] {2, 3});
          Assert.AreEqual(2, res.Length);
          Assert.AreEqual(0, res[0].Item1);
          Assert.AreEqual(0, res[0].Item2);
          Assert.AreEqual(3, res[1].Item1);
          Assert.AreEqual(3, res[1].Item2);
        }

        {
          var res = split(new byte[] {1, 2, 3, 4}, new byte[] {3});
          Assert.AreEqual(2, res.Length);
          Assert.AreEqual(0, res[0].Item1);
          Assert.AreEqual(1, res[0].Item2);
          Assert.AreEqual(3, res[1].Item1);
          Assert.AreEqual(3, res[1].Item2);
        }
      }

      [Test]
      public void SplitDoubleMiddleMatch()
      {
        {
          var res = split(new byte[] {1, 2, 3, 1, 2, 3}, new byte[] {2});
          Assert.AreEqual(3, res.Length);
          Assert.AreEqual(0, res[0].Item1);
          Assert.AreEqual(0, res[0].Item2);
          Assert.AreEqual(2, res[1].Item1);
          Assert.AreEqual(3, res[1].Item2);
          Assert.AreEqual(5, res[2].Item1);
          Assert.AreEqual(5, res[2].Item2);
        }

        {
          var res = split(new byte[] {1, 2, 3, 1, 2, 3}, new byte[] {1});
          Assert.AreEqual(2, res.Length);
          Assert.AreEqual(1, res[0].Item1);
          Assert.AreEqual(2, res[0].Item2);
          Assert.AreEqual(4, res[1].Item1);
          Assert.AreEqual(5, res[1].Item2);
        }

        {
          var res = split(new byte[] {1, 2, 3, 1, 2, 3}, new byte[] {3});
          Assert.AreEqual(2, res.Length);
          Assert.AreEqual(0, res[0].Item1);
          Assert.AreEqual(1, res[0].Item2);
          Assert.AreEqual(3, res[1].Item1);
          Assert.AreEqual(4, res[1].Item2);
        }
      }

      [Test]
      public void SplitInARowMatch()
      {
        {
          var res = split(new byte[] {1, 2, 1, 2, 3, 1, 2, 3}, new byte[] {1, 2, 3});
          Assert.AreEqual(1, res.Length);
          Assert.AreEqual(0, res[0].Item1);
          Assert.AreEqual(1, res[0].Item2);
        }

        {
          var res = split(new byte[] {1, 2, 3, 1, 2, 3, 1, 2}, new byte[] {1, 2, 3});
          Assert.AreEqual(1, res.Length);
          Assert.AreEqual(6, res[0].Item1);
          Assert.AreEqual(7, res[0].Item2);
        }

        {
          var res = split(new byte[] {1, 2, 3, 1, 2, 3, 1, 2, 3}, new byte[] {1, 2, 3});
          Assert.AreEqual(0, res.Length);
        }

        {
          var res = split(new byte[] {1, 2, 3, 1, 2, 3, 0, 1, 2, 3}, new byte[] {1, 2, 3});
          Assert.AreEqual(1, res.Length);
          Assert.AreEqual(6, res[0].Item1);
          Assert.AreEqual(6, res[0].Item2);
        }
      }

    #endregion

    #region Helpers

      public Tuple<long, long>[] split(byte[] what, byte[] by, 
        long whatFirst = 0, long? whatLast = null, long byFirst = 0, long? byLast = null, int seekBuffSize = 1)
      {
        using (var stream = new MemoryStream(what))
        {
          var res = StreamHelpers.Split(stream, by, whatFirst, whatLast, byFirst, byLast, seekBuffSize).ToArray();
          return res;
        }
      }

    #endregion
  }
}
