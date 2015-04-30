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
using System.Linq;
using StreamHelpers = NFX.Web.MultiPartContent.StreamHelpers;
using NUnit.Framework;

namespace NFX.NUnit.Web.MultiPart
{
  [TestFixture]
  public class FindSplitTests
  {
    #region Consts

      private static int[] BUF_SIZES = {1, 2, 3, 5, 30};

    #endregion

    #region FindIndex

      [Test]
      public void FindIndex_IdxStart()
      {
        byte[] where = { 0, 1, 2, 3, 4, 5 };

        int whatIdx;

        whatIdx = StreamHelpers.FindIndex(where, 0, where.Length, new byte[] { 0 });
        Assert.AreEqual(0, whatIdx);

        whatIdx = StreamHelpers.FindIndex(where, 0, where.Length, new byte[] { 0, 1 });
        Assert.AreEqual(0, whatIdx);

        whatIdx = StreamHelpers.FindIndex(where, 0, where.Length, new byte[] { 0, 1, 2, 3, 4, 5 });
        Assert.AreEqual(0, whatIdx);
      }

      [Test]
      public void FindIndex_IdxMiddle()
      {
        byte[] where = { 0, 1, 2, 3, 4, 5 };

        int whatIdx;

        whatIdx = StreamHelpers.FindIndex(where, 0, where.Length, new byte[] { 1 });
        Assert.AreEqual(1, whatIdx);

        whatIdx = StreamHelpers.FindIndex(where, 0, where.Length, new byte[] { 2, 3 });
        Assert.AreEqual(2, whatIdx);

        whatIdx = StreamHelpers.FindIndex(where, 0, where.Length, new byte[] { 2, 3, 4 });
        Assert.AreEqual(2, whatIdx);
      }

      [Test]
      public void FindIndex_IdxFinish()
      {
        byte[] where = { 0, 1, 2, 3, 4, 5 };

        int whatIdx;

        whatIdx = StreamHelpers.FindIndex(where, 0, where.Length, new byte[] { 5 });
        Assert.AreEqual(5, whatIdx);

        whatIdx = StreamHelpers.FindIndex(where, 0, where.Length, new byte[] { 4, 5 });
        Assert.AreEqual(4, whatIdx);

        whatIdx = StreamHelpers.FindIndex(where, 0, where.Length, new byte[] { 3, 4, 5 });
        Assert.AreEqual(3, whatIdx);
      }

      [Test]
      public void FindIndex_IdxFinishBy1()
      {
        byte[] where = { 0, 1, 2, 3, 4, 5, 0 };

        int whatIdx;

        whatIdx = StreamHelpers.FindIndex(where, 1, where.Length-1, new byte[] { 0 });
        Assert.AreEqual(6, whatIdx);
      }

      [Test]
      public void FindIndex_IdxNotFound()
      {
        byte[] where = { 0, 1, 2, 3, 4, 5 };

        int whatIdx;

        whatIdx = StreamHelpers.FindIndex(where, 0, where.Length, new byte[] { 6 });
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, whatIdx);

        whatIdx = StreamHelpers.FindIndex(where, 0, where.Length, new byte[] { 0, 0 });
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, whatIdx);

        whatIdx = StreamHelpers.FindIndex(where, 0, where.Length, new byte[] { 1, 2, 3, 3 });
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, whatIdx);
      }

      [Test]
      public void FindIndex_WhereBoundary()
      {
        byte[] where = { 0, 1, 2, 3, 4, 5 };

        int whatIdx;

        whatIdx = StreamHelpers.FindIndex(where, 1, where.Length - 1, new byte[] { 0, 1 });
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, whatIdx);

        whatIdx = StreamHelpers.FindIndex(where, 1, where.Length - 1, new byte[] { 1, 2, 3 });
        Assert.AreEqual(1, whatIdx);

        whatIdx = StreamHelpers.FindIndex(where, 2, where.Length - 2, new byte[] { 1, 2, 3 });
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, whatIdx);

        whatIdx = StreamHelpers.FindIndex(where, 0, where.Length - 2, new byte[] { 1, 2, 3 });
        Assert.AreEqual(1, whatIdx);

        whatIdx = StreamHelpers.FindIndex(where, 0, where.Length - 3, new byte[] { 1, 2, 3 });
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, whatIdx);

        whatIdx = StreamHelpers.FindIndex(where, 2, where.Length - 1, new byte[] { 2, 3, 4 });
        Assert.AreEqual(2, whatIdx);
      }

      [Test]
      public void FindIndex_WhatIsLonger()
      {
        byte[] where = { 0, 1, 2, 3, 4, 5 };

        int whatIdx;

        whatIdx = StreamHelpers.FindIndex(where, 1, where.Length - 1, new byte[] { 0, 1, 2, 3, 4, 5, 6 });
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, whatIdx);
      }

      [Test]
      public void FindIndex_EmptyBoth()
      {
        byte[] where = { };

        int whatIdx;

        whatIdx = StreamHelpers.FindIndex(where, 0, where.Length, new byte[] { });
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, whatIdx);
      }

      [Test]
      public void FindIndex_EmptyWhat()
      {
        byte[] where = { 0 };

        int whatIdx;

        whatIdx = StreamHelpers.FindIndex(where, 0, where.Length, new byte[] { });
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, whatIdx);
      }

      [Test]
      public void FindIndex_ErrorNullWhat()
      {
        int whatIdx = StreamHelpers.FindIndex((byte[])null, 0, 0, new byte[] { });
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, whatIdx);
      }

      [Test]
      [ExpectedException(typeof(NullReferenceException))]
      public void FindIndex_ErrorNullWhere()
      {
        StreamHelpers.FindIndex(new byte[] { }, 0, 0, null);
      } 

    #endregion

    #region BytesSplitNew

      [Test]  
      [ExpectedException(typeof(NullReferenceException))]
      public void BytesSplitNew_NullBy()
      {
        byte[] what = {0, 1, 2, 3, 4, 5};

        var segments = StreamHelpers.BytesSplitNew( what, what.Length, null).ToArray();
      }

      [Test]  
      public void BytesSplitNew_NullWhat()
      {
        var segments = StreamHelpers.BytesSplitNew((byte[])null, 0, new byte[] {}).ToArray();
        Assert.AreEqual(0, segments.Length);
      }

      [Test]
      public void BytesSplitNew_NoMatch()
      {
        byte[] what = {0, 1, 2, 3, 4, 5};
        var segments = StreamHelpers.BytesSplitNew( what, what.Length, new byte[] {6}).ToArray();
        Assert.AreEqual(0, segments.Length);
      }

      [Test]
      public void BytesSplitNew_MatchStart()
      {
        byte[] what = {0, 1, 2, 3, 4, 5};
        var segments = StreamHelpers.BytesSplitNew( what, what.Length, new byte[] {0}).ToArray();
        Assert.AreEqual(0, segments.Length);
      }

      [Test]
      public void BytesSplitNew_MatchFinish()
      {
        byte[] what = {0, 1, 2, 3, 4, 5};
        var segments = StreamHelpers.BytesSplitNew( what, what.Length, new byte[] {5}).ToArray();
        Assert.AreEqual(0, segments.Length);
      }

      [Test]
      public void BytesSplitNew_MatchMiddle()
      {
        byte[] what = {0, 1, 2, 3, 4, 5};
        var segments = StreamHelpers.BytesSplitNew( what, what.Length, new byte[] {3}).ToArray();
        Assert.AreEqual(0, segments.Length);
      }

      [Test]
      public void BytesSplitNew_MatchStart_ReturnOuter()
      {
        byte[] what = {0, 1, 2, 3, 4, 5};
        var segments = StreamHelpers.BytesSplitNew( what, what.Length, new byte[] {0}, true).ToArray();
        Assert.AreEqual(1, segments.Length);
        Assert.IsTrue((new byte[] {1, 2, 3, 4, 5}).SequenceEqual(what.Skip(segments[0].Item1).Take(segments[0].Item2)));
      }

      [Test]
      public void BytesSplitNew_MatchFinish_ReturnOuter()
      {
        byte[] what = {0, 1, 2, 3, 4, 5};
        var segments = StreamHelpers.BytesSplitNew( what, what.Length, new byte[] {5}, true).ToArray();
        Assert.AreEqual(1, segments.Length);
        Assert.IsTrue((new byte[] {0, 1, 2, 3, 4}).SequenceEqual(what.Skip(segments[0].Item1).Take(segments[0].Item2)));
      }

      [Test]
      public void BytesSplitNew_MatchMiddle_ReturnOuter()
      {
        byte[] what = {0, 1, 2, 3, 4, 5};
        var segments = StreamHelpers.BytesSplitNew( what, what.Length, new byte[] {3}, true).ToArray();
        Assert.AreEqual(2, segments.Length);
        Assert.IsTrue((new byte[] {0, 1, 2}).SequenceEqual(what.Skip(segments[0].Item1).Take(segments[0].Item2)));
        Assert.IsTrue((new byte[] {4, 5}).SequenceEqual(what.Skip(segments[1].Item1).Take(segments[1].Item2)));
      }

      [Test]
      public void BytesSplitNew_MatchSingle()
      {
        byte[] what = {0, 1, 2, 3, 4, 5, 0};
        var segments = StreamHelpers.BytesSplitNew( what, what.Length, new byte[] {0}).ToArray();
        Assert.AreEqual(1, segments.Length);
        Assert.IsTrue((new byte[] {1, 2, 3, 4, 5}).SequenceEqual(what.Skip(segments[0].Item1).Take(segments[0].Item2)));
      }

      [Test]
      public void BytesSplitNew_MatchSingle1()
      {
        byte[] what = {0, 1, 2, 3, 4, 1, 5, 0};
        var segments = StreamHelpers.BytesSplitNew( what, what.Length, new byte[] {1}).ToArray();
        Assert.AreEqual(1, segments.Length);
        Assert.IsTrue((new byte[] { 2, 3, 4}).SequenceEqual(what.Skip(segments[0].Item1).Take(segments[0].Item2)));
      }

      [Test]
      public void BytesSplitNew_MatchSingle2()
      {
        byte[] what = {1, 2, 3, 4, 1, 0, 5, 0};
        var segments = StreamHelpers.BytesSplitNew( what, what.Length, new byte[] {0}).ToArray();
        Assert.AreEqual(1, segments.Length);
        Assert.IsTrue((new byte[] { 5}).SequenceEqual(what.Skip(segments[0].Item1).Take(segments[0].Item2)));
      }

      [Test]
      public void BytesSplitNew_DoubleBy()
      {
        byte[] what = {1, 2, 3, 4, 1, 2};
        var segments = StreamHelpers.BytesSplitNew( what, what.Length, new byte[] {1, 2}).ToArray();
        Assert.AreEqual(1, segments.Length);
        Assert.IsTrue((new byte[] { 3, 4}).SequenceEqual(what.Skip(segments[0].Item1).Take(segments[0].Item2)));
      }

      [Test]
      public void BytesSplitNew_DoubleByDoubleWhat()
      {
        byte[] what = {1, 2, 3, 4, 1, 2, 0, 1, 2};
        var segments = StreamHelpers.BytesSplitNew( what, what.Length, new byte[] {1, 2}).ToArray();
        Assert.AreEqual(2, segments.Length);
        Assert.IsTrue((new byte[] { 3, 4}).SequenceEqual(what.Skip(segments[0].Item1).Take(segments[0].Item2)));
        Assert.IsTrue((new byte[] { 0}).SequenceEqual(what.Skip(segments[1].Item1).Take(segments[1].Item2)));
      }

      [Test]
      public void BytesSplitNew_DoubleByDoubleWhat1()
      {
        byte[] what = {0, 1, 2, 3, 4, 1, 2, 0, 1, 1, 1, 2};
        var segments = StreamHelpers.BytesSplitNew( what, what.Length, new byte[] {1, 2}).ToArray();
        Assert.AreEqual(2, segments.Length);
        Assert.IsTrue((new byte[] { 3, 4}).SequenceEqual(what.Skip(segments[0].Item1).Take(segments[0].Item2)));
        Assert.IsTrue((new byte[] { 0, 1, 1}).SequenceEqual(what.Skip(segments[1].Item1).Take(segments[1].Item2)));
      }

      [Test]
      public void BytesSplitNew_DoubleByDoubleWhat2()
      {
        byte[] what = {0, 1, 2, 1, 2, 3, 4, 1, 2, 0, 1, 1, 1, 2};
        var segments = StreamHelpers.BytesSplitNew( what, what.Length, new byte[] {1, 2}).ToArray();
        Assert.AreEqual(2, segments.Length);
        Assert.IsTrue((new byte[] { 3, 4}).SequenceEqual(what.Skip(segments[0].Item1).Take(segments[0].Item2)));
        Assert.IsTrue((new byte[] { 0, 1, 1}).SequenceEqual(what.Skip(segments[1].Item1).Take(segments[1].Item2)));
      }

    #endregion

    #region StreamSplitNew

      [Test]
      public void Split_EmptyStartBy()
      {
        byte[] what = {0, 1, 2, 3, 4, 5};

        foreach (var bufSize in BUF_SIZES)
        {
          using (var ms = new MemoryStream(what))
          {
            var segments = StreamHelpers.StreamSplitNew(ms, new byte[] { 0 }, bufSize).ToArray();
            Assert.AreEqual(0, segments.Length);
          } 
        }
      }

      [Test]
      public void Split_EmptyFinishBy()
      {
        byte[] what = {0, 1, 2, 3, 4, 5};

        foreach (var bufSize in BUF_SIZES)
        {
          using (var ms = new MemoryStream(what))
          {
            var segments = StreamHelpers.StreamSplitNew(ms, new byte[] { 5 }, bufSize).ToArray();
            Assert.AreEqual(0, segments.Length);
          } 
        }
      }

      [Test]
      public void Split_EmptyMiddleBy()
      {
        byte[] what = {0, 1, 2, 3, 4, 5};

        foreach (var bufSize in BUF_SIZES)
        {
          using (var ms = new MemoryStream(what))
          {
            var segments = StreamHelpers.StreamSplitNew(ms, new byte[] { 2 }, bufSize).ToArray();
            Assert.AreEqual(0, segments.Length);
          } 
        }
      }

      [Test]
      public void Split_Single1()
      {
        byte[] what = {0, 1, 0, 3, 4, 5};

        foreach (var bufSize in BUF_SIZES)
        {
          using (var ms = new MemoryStream(what))
          {
            int i = 0;
            foreach (var segment in StreamHelpers.StreamSplitNew(ms, new byte[] { 0 }, 30))
            {
              if (i == 0)
                Assert.IsTrue((new byte[] {1}).SequenceEqual(segment.Item1.Take(segment.Item2)));
              i++;
            }
            Assert.AreEqual(1, i);
          } 
        }
      }

      [Test]
      public void Split_Single2()
      {
        byte[] what = {0, 1, 2, 3, 4, 5, 0};

        foreach (var bufSize in BUF_SIZES)
        {
          using (var ms = new MemoryStream(what))
          {
            var segments = StreamHelpers.StreamSplitNew(ms, new byte[] { 0 }, bufSize).ToArray();
            Assert.AreEqual(1, segments.Length);
            Assert.IsTrue((new byte[] {1, 2, 3, 4, 5}).SequenceEqual(segments[0].Item1.Take(segments[0].Item2)));
          } 
        }
      }

      [Test]
      public void Split_Single3()
      {
        byte[] what = {1, 0, 2, 3, 4, 5, 0};

        foreach (var bufSize in BUF_SIZES)
        {
          using (var ms = new MemoryStream(what))
          {
            var segments = StreamHelpers.StreamSplitNew(ms, new byte[] { 0 }, bufSize).ToArray();
            Assert.AreEqual(1, segments.Length);
            Assert.IsTrue((new byte[] {2, 3, 4, 5}).SequenceEqual(segments[0].Item1.Take(segments[0].Item2)));
          } 
        }
      }

      [Test]
      public void Split_Double1()
      {
        byte[] what = {0, 1, 2, 0, 3, 4, 5, 0};

        foreach (var bufSize in BUF_SIZES)
        {
          using (var ms = new MemoryStream(what))
          {
            int i = 0;
            foreach (var segment in StreamHelpers.StreamSplitNew(ms, new byte[] { 0 }, 30))
            {
              if (i == 0)
                Assert.IsTrue((new byte[] {1, 2}).SequenceEqual(segment.Item1.Take(segment.Item2)));
              else if (i == 1)
                Assert.IsTrue((new byte[] {3, 4, 5}).SequenceEqual(segment.Item1.Take(segment.Item2)));

              i++;
            }
            Assert.AreEqual(2, i);
          } 
        }
      }

      [Test]
      public void Split_Double2()
      {
        byte[] what = {0, 0, 0, 0, 1, 2, 3, 4, 5, 0, 0, 0, 0, 1, 2};

        foreach (var bufSize in BUF_SIZES)
        {
          using (var ms = new MemoryStream(what))
          {
            var segments = StreamHelpers.StreamSplitNew(ms, new byte[] { 0, 1, 2 }, bufSize).ToArray();
            Assert.AreEqual(1, segments.Length);
            Assert.IsTrue((new byte[] {3, 4, 5, 0, 0, 0}).SequenceEqual(segments[0].Item1.Take(segments[0].Item2)));
          } 
        }
      }

      [Test]
      public void Split_Tripple()
      {
        byte[] what = {0, 0, 1, 2, 3, 5, 6, 7, 1, 2, 5, 0, 0, 1, 2, 4, 5};

        foreach (var bufSize in BUF_SIZES)
        {
          using (var ms = new MemoryStream(what))
          {
            int i = 0;
            foreach(var segment in StreamHelpers.StreamSplitNew(ms, new byte[] { 1, 2 }, bufSize))
            {
              if (i == 0)
                Assert.IsTrue((new byte[] {3, 5, 6, 7}).SequenceEqual(segment.Item1.Take(segment.Item2)));
              else if (i == 0)
                Assert.IsTrue((new byte[] {5, 0, 0}).SequenceEqual(segment.Item1.Take(segment.Item2)));
              i++;
            }
            Assert.AreEqual(2, i);
          } 
        }
      }

      [Test]
      public void Split_RepeatingByPrefix()
      {
        byte[] what = { 0, 0, 0, 0, 1, 2, (byte)'a', (byte)'b', (byte)'c', (byte)'a', 0, 0, 0, 0, 1, 2};

        foreach (var bufSize in BUF_SIZES)
        {
          using (var ms = new MemoryStream(what))
          {
            int i = 0;
            foreach(var segment in StreamHelpers.StreamSplitNew(ms, new byte[] { 0, 0, 0, 1, 2}, bufSize))
            {
              if (i == 0)
                Assert.IsTrue((new byte[] {(byte)'a', (byte)'b', (byte)'c', (byte)'a', 0}).SequenceEqual(segment.Item1.Take(segment.Item2)));

              i++;
            }
            Assert.AreEqual(1, i);
          } 
        }
      }

      [Test]
      public void Split_EmptyInARow()
      {
        byte[] what = {0, 1, 2, 0, 1, 2};

        foreach (var bufSize in BUF_SIZES)
        {
          using (var ms = new MemoryStream(what))
          {
            var segments = StreamHelpers.StreamSplitNew(ms, new byte[] { 0, 1, 2 }, bufSize).ToArray();
            Assert.AreEqual(0, segments.Length);
          } 
        }
      }

    #endregion
  }
}
