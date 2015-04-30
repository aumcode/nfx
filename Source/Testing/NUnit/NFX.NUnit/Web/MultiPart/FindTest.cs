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
  public class FindTest
  {
    #region Consts

      private static byte[] SRC_BYTES_10 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

      private byte[] SEQ_EXISTING_BYTES_1 = new byte[] { 1};
      private byte[] SEQ_NONEXISTING_BYTES_1 = new byte[] { 0};

      private byte[] SEQ_EXISTING_BYTES_3 = new byte[] { 1, 2, 3};
      private byte[] SEQ_EXISTING_BYTES_3_MIDDLE = new byte[] { 5, 6, 7};
      private byte[] SEQ_NONEXISTING_BYTES_3 = new byte[] { 1, 2, 2};

    #endregion

    #region Single subsequence

      [Test]
      public void NoSingle_FindBuf1()
      {
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, find(SRC_BYTES_10, SEQ_NONEXISTING_BYTES_1));
      }

      [Test]
      public void NoSingle_FindBuf2()
      {
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, find(SRC_BYTES_10, SEQ_NONEXISTING_BYTES_1, seekBuffSize: 2));
      }

      [Test]
      public void NoSeveral_FindBuf1()
      {
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, find(SRC_BYTES_10, SEQ_NONEXISTING_BYTES_3));
      }

      [Test]
      public void NoSeveral_FindBuf2()
      {
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, find(SRC_BYTES_10, SEQ_NONEXISTING_BYTES_3, seekBuffSize: 2));
      }

      [Test]
      public void FindSingeFirst_FindBuf1()
      {
        Assert.AreEqual(0, find(SRC_BYTES_10, SEQ_EXISTING_BYTES_1));
      }

      [Test]
      public void FindSingeFirst_FindBuf2()
      {
        Assert.AreEqual(0, find(SRC_BYTES_10, SEQ_EXISTING_BYTES_1, seekBuffSize: 2));
      }

      [Test]
      public void FindSeveralFirst_FindBuf1()
      {
        Assert.AreEqual(0, find(SRC_BYTES_10, SEQ_EXISTING_BYTES_3));
      }

      [Test]
      public void FindSeveralFirst_FindBuf2()
      {
        Assert.AreEqual(0, find(SRC_BYTES_10, SEQ_EXISTING_BYTES_3, seekBuffSize: 2));
      }

      [Test]
      public void FindSeveralFirst_FindBuf4()
      {
        Assert.AreEqual(0, find(SRC_BYTES_10, SEQ_EXISTING_BYTES_3, seekBuffSize: 4));
      }

      [Test]
      public void FindSeveralFirst_FindBuf10()
      {
        Assert.AreEqual(0, find(SRC_BYTES_10, SEQ_EXISTING_BYTES_3, seekBuffSize: 10));
      }

      [Test]
      public void FindSeveralFirst_FindBuf12()
      {
        Assert.AreEqual(0, find(SRC_BYTES_10, SEQ_EXISTING_BYTES_3, seekBuffSize: 12));
      }

      [Test]
      public void FindSeveralMiddle_FindBuf1()
      {
        Assert.AreEqual(4, find(SRC_BYTES_10, SEQ_EXISTING_BYTES_3_MIDDLE));
      }

      [Test]
      public void FindSeveralMiddle_FindBuf3()
      {
        Assert.AreEqual(4, find(SRC_BYTES_10, SEQ_EXISTING_BYTES_3_MIDDLE, seekBuffSize: 3));
      }

      [Test]
      public void FindSeveralMiddle_FindBuf5()
      {
        Assert.AreEqual(4, find(SRC_BYTES_10, SEQ_EXISTING_BYTES_3_MIDDLE, seekBuffSize: 5));
      }

      [Test]
      public void NoMoreInLess_FindBuf1()
      {
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, find(SEQ_NONEXISTING_BYTES_3, SRC_BYTES_10));
      }

      [Test]
      public void NoMoreInLess_FindBuf2()
      {
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, find(SEQ_NONEXISTING_BYTES_3, SRC_BYTES_10, seekBuffSize: 2));
      }

      [Test]
      public void NoMoreInLess_FindBuf5()
      {
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, find(SEQ_NONEXISTING_BYTES_3, SRC_BYTES_10, seekBuffSize: 5));
      }

      [Test]
      public void NoMoreInLess_FindBuf12()
      {
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, find(SEQ_NONEXISTING_BYTES_3, SRC_BYTES_10, seekBuffSize: 12));
      }

      [Test]
      public void FindInTheSame_FindBuf1()
      {
        Assert.AreEqual(0, find(SRC_BYTES_10, SRC_BYTES_10));
        Assert.AreEqual(0, find(SEQ_EXISTING_BYTES_3, SEQ_EXISTING_BYTES_3));
      }

      [Test]
      public void FindInTheSame_FindBuf2()
      {
        Assert.AreEqual(0, find(SRC_BYTES_10, SRC_BYTES_10, seekBuffSize: 2));
        Assert.AreEqual(0, find(SEQ_EXISTING_BYTES_3, SEQ_EXISTING_BYTES_3, seekBuffSize: 2));
      }

      [Test]
      public void FindInTheSame_FindBuf5()
      {
        Assert.AreEqual(0, find(SRC_BYTES_10, SRC_BYTES_10, seekBuffSize: 5));
        Assert.AreEqual(0, find(SEQ_EXISTING_BYTES_3, SEQ_EXISTING_BYTES_3, seekBuffSize: 5));
      }

      [Test]
      public void FindInTheSame_FindBuf12()
      {
        Assert.AreEqual(0, find(SRC_BYTES_10, SRC_BYTES_10, seekBuffSize: 12));
        Assert.AreEqual(0, find(SEQ_EXISTING_BYTES_3, SEQ_EXISTING_BYTES_3, seekBuffSize: 12));
      }

    #endregion

    #region Seek first/last

      #region Consts

        private static byte[] SRC_BYTES_WHERE = new byte[] { 1, 2, 3, 4, 5, 1, 2, 3, 4, 5 };
        private static byte[] SRC_BYTES_WHERE_12 = new byte[] { 1, 2, 1, 2, 1, 2, 1, 2, 1, 2 };

        //private byte[] SEQ_EXISTING_BYTES_1 = new byte[] { 1};
        //private byte[] SEQ_NONEXISTING_BYTES_1 = new byte[] { 0};

        //private byte[] SEQ_EXISTING_BYTES_3 = new byte[] { 1, 2, 3};
        //private byte[] SEQ_EXISTING_BYTES_3_MIDDLE = new byte[] { 5, 6, 7};
        //private byte[] SEQ_NONEXISTING_BYTES_3 = new byte[] { 1, 2, 2};

      #endregion

      [Test]
      public void NoSeveral_WhereFirst()
      {
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, find(SRC_BYTES_10, SEQ_EXISTING_BYTES_3, whereFirst: 1));
      }

      [Test]
      public void NoSeveral_WhereLast()
      {
        Assert.AreEqual(StreamHelpers.NOT_FOUND_POS, find(SRC_BYTES_10, SEQ_EXISTING_BYTES_3, whereLast: 1));
      }

      [Test]
      public void FindFirst_WhereFirst()
      {
        Assert.AreEqual(5, find(SRC_BYTES_WHERE, SEQ_EXISTING_BYTES_3, whereFirst: 1));
      }

      [Test]
      public void FindFirst_WhatFirst()
      {
        Assert.AreEqual(1, find(SRC_BYTES_WHERE, SEQ_EXISTING_BYTES_3, whatFirst: 1));
        Assert.AreEqual(2, find(SRC_BYTES_WHERE, SEQ_EXISTING_BYTES_3, whatFirst: 2));
      }

      [Test]
      public void FindFirst_WhatLast()
      {
        Assert.AreEqual(0, find(SRC_BYTES_WHERE_12, SEQ_EXISTING_BYTES_3, whatLast: 1));
      }

    #endregion

    #region In a row

      [Test]
      public void FindInARow_Single()
      {
        Assert.AreEqual(0, find(new byte[] {1, 1}, new byte[] {1}));
        Assert.AreEqual(1, find(new byte[] {1, 1}, new byte[] {1}, whereFirst: 1));
      }
      
    #endregion

    #region Exceptions

      [Test]
      [ExpectedException(typeof(ArgumentNullException))]
      public void FindInNullSource()
      {
        find(null, SEQ_EXISTING_BYTES_1);
      }

      [Test]
      [ExpectedException(typeof(NullReferenceException))]
      public void FindNullSequence()
      {
        find(SEQ_EXISTING_BYTES_1, null);
      }

    #endregion

    #region Helpers

      public long find(byte[] where, byte[] what, 
        long whereFirst = 0, long? whereLast = null, long whatFirst = 0, long? whatLast = null, int seekBuffSize = 1)
      {
        using (var stream = new MemoryStream(where))
        {
          long pos = StreamHelpers.Find(stream, what, whereFirst, whereLast, whatFirst, whatLast, seekBuffSize);
          return pos;
        }
      }

    #endregion

  }
}
