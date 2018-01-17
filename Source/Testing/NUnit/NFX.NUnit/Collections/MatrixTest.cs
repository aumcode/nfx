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

using NFX.Collections;
using System.Collections;

namespace NFX.NUnit.Collections
{
  [TestFixture]
  public class MatrixTest
  {
    private static int[][] TEST_INTS_2D = new int[][] {
        new int[] { 1, 2, 3}, 
        new int[] { 11, 12, 13}
      };

    private static int[] TEST_INTS_FLAT = {1, 2, 3, 11, 12, 13};

    [TestCase]
    public void Rank()
    {
      Matrix2D<int> m = new Matrix2D<int>(10, 20);

      Assert.AreEqual(2, m.Rank);
      Assert.AreEqual(0, m.GetLowerBound(0));
      Assert.AreEqual(10, m.GetUpperBound(0));
      Assert.AreEqual(0, m.GetLowerBound(0));
      Assert.AreEqual(20, m.GetUpperBound(1));
    }

    [TestCase]
    public void Index()
    {
      Matrix2D<int> m = new Matrix2D<int>(10, 20);

      m[0, 0] = 10;
      m[0, 19] = 9;
      m[9, 0] = 90;
      m[9, 19] = 99;

      Assert.AreEqual(10, m[0, 0]);
      Assert.AreEqual(9, m[0, 19]);
      Assert.AreEqual(90, m[9, 0]);
      Assert.AreEqual(99, m[9, 19]);
    }

    [TestCase]
    public void Fill()
    {
      Matrix2D<int> m = new Matrix2D<int>(3, 3);
      Assert.AreEqual("0 0 0\r\n" + "0 0 0\r\n" + "0 0 0", m.ToString());

      m.Fill(2);
      Assert.AreEqual("2 2 2\r\n" + "2 2 2\r\n" + "2 2 2", m.ToString());
    }

    [TestCase]
    [ExpectedException(typeof(NFXException), ExpectedMessage=StringConsts.ARGUMENT_ERROR, MatchType=MessageMatch.StartsWith)]
    public void DimensionsBoth0()
    {
      Matrix2D<int> m = new Matrix2D<int>(0, 0);
    }

    [TestCase]
    [ExpectedException(typeof(NFXException), ExpectedMessage=StringConsts.ARGUMENT_ERROR, MatchType=MessageMatch.StartsWith)]
    public void DimensionsWidth0()
    {
      Matrix2D<int> m = new Matrix2D<int>(0, 1);
    }

    [TestCase]
    [ExpectedException(typeof(NFXException), ExpectedMessage=StringConsts.ARGUMENT_ERROR, MatchType=MessageMatch.StartsWith)]
    public void DimensionsHeight0()
    {
      Matrix2D<int> m = new Matrix2D<int>(1, 0);
    }

    [TestCase]
    public void Enumerable()
    {
      Matrix2D<int> m = new Matrix2D<int>(3, 2);

      fillMatrix2D( m, TEST_INTS_2D);

      IEnumerator ieExpected = TEST_INTS_FLAT.GetEnumerator();
      IEnumerator ieReal = m.GetEnumerator();

      while (ieExpected.MoveNext() && ieReal.MoveNext())
        Assert.AreEqual( ieExpected.Current, ieReal.Current);
    }

    [TestCase]
    public void CompareSelf()
    {
      Matrix2D<int> m0 = new Matrix2D<int>(3, 2);
      fillMatrix2D( m0, TEST_INTS_2D);

      Assert.AreNotEqual(m0, null);
      Assert.AreEqual(m0, m0);
    }

    [TestCase]
    public void CompareTypes()
    {
      Matrix2D<int> m0 = new Matrix2D<int>(3, 2);
      Matrix2D<short> m1 = new Matrix2D<short>(3, 2);

      Assert.IsFalse(object.Equals(m0, m1));
      Assert.IsFalse(m0.Equals(m1));
    }

    [TestCase]
    public void CompareRanks()
    {
      Matrix2D<int> m0 = new Matrix2D<int>(3, 2);
      Matrix2D<int> m1 = new Matrix2D<int>(3, 3);

      Assert.IsFalse(object.Equals(m0, m1));
    }

    [TestCase]
    public void CompareElements()
    {
      Matrix2D<int> m0 = new Matrix2D<int>(3, 2);
      fillMatrix2D( m0, TEST_INTS_2D);

      Matrix2D<int> m1 = new Matrix2D<int>(3, 2);
      fillMatrix2D( m1, TEST_INTS_2D);

      Assert.AreEqual(m0, m1);
      m1[1, 1] = 1;
      Assert.AreNotEqual(m0, m1);
    }

    [TestCase]
    public void MatrixToString()
    {
      Matrix2D<int> m0 = new Matrix2D<int>(3, 2);
      Assert.AreEqual("0 0 0\r\n"+"0 0 0", m0.ToString());

      Matrix2D<short> m1 = new Matrix2D<short>(2, 3);
      m1[0, 1] = 1;
      m1[1, 0] = 2;
      m1[1, 2] = 7;
      Assert.AreEqual("0 2\r\n"+"1 0\r\n"+"0 7", m1.ToString());
    }


    #region Private

      private static void fillMatrix2D<T>(Matrix2D<T> matrix, T[][] src)
      {
        for (int y = 0; y < src.Length; y++)
          for (int x = 0; x < src[y].Length; x++)
            matrix[x, y] = src[y][x];
      } 

    #endregion
  }
}
