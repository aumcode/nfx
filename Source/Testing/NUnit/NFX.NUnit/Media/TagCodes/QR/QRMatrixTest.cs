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
using NFX.Collections;
using NFX.Media.TagCodes.QR;
using NUnit.Framework;

namespace NFX.NUnit.Media.TagCodes.QR
{
  [TestFixture]
  public class QRMatrixTest
  {
    [Test]
    public void Pattern_01()
    {
      const int WIDTH = 21, HEIGHT = 21;

      QRMatrix matrix = new QRMatrix(WIDTH, HEIGHT);
      matrix.Fill(2);
      matrix.AddBasicPatterns(QRVersion.GetVersionByNumber(1));

      string expected =
          "1 1 1 1 1 1 1 0           0 1 1 1 1 1 1 1\r\n" +
          "1 0 0 0 0 0 1 0           0 1 0 0 0 0 0 1\r\n" +
          "1 0 1 1 1 0 1 0           0 1 0 1 1 1 0 1\r\n" +
          "1 0 1 1 1 0 1 0           0 1 0 1 1 1 0 1\r\n" +
          "1 0 1 1 1 0 1 0           0 1 0 1 1 1 0 1\r\n" +
          "1 0 0 0 0 0 1 0           0 1 0 0 0 0 0 1\r\n" +
          "1 1 1 1 1 1 1 0 1 0 1 0 1 0 1 1 1 1 1 1 1\r\n" +
          "0 0 0 0 0 0 0 0           0 0 0 0 0 0 0 0\r\n" +
          "            1                            \r\n" +
          "            0                            \r\n" +
          "            1                            \r\n" +
          "            0                            \r\n" +
          "            1                            \r\n" +
          "0 0 0 0 0 0 0 0 1                        \r\n" +
          "1 1 1 1 1 1 1 0                          \r\n" +
          "1 0 0 0 0 0 1 0                          \r\n" +
          "1 0 1 1 1 0 1 0                          \r\n" +
          "1 0 1 1 1 0 1 0                          \r\n" +
          "1 0 1 1 1 0 1 0                          \r\n" +
          "1 0 0 0 0 0 1 0                          \r\n" +
          "1 1 1 1 1 1 1 0                          ";

      Assert.AreEqual( expected, matrix.ToString());
    }

    [Test]
    public void Pattern_02()
    {
      const int WIDTH = 25, HEIGHT = 25;

      QRMatrix matrix = new QRMatrix(WIDTH, HEIGHT);
      matrix.Fill(2);
      matrix.AddBasicPatterns(QRVersion.GetVersionByNumber(2));

      string expected =
        "1 1 1 1 1 1 1 0                   0 1 1 1 1 1 1 1\r\n" +
        "1 0 0 0 0 0 1 0                   0 1 0 0 0 0 0 1\r\n" +
        "1 0 1 1 1 0 1 0                   0 1 0 1 1 1 0 1\r\n" +
        "1 0 1 1 1 0 1 0                   0 1 0 1 1 1 0 1\r\n" +
        "1 0 1 1 1 0 1 0                   0 1 0 1 1 1 0 1\r\n" +
        "1 0 0 0 0 0 1 0                   0 1 0 0 0 0 0 1\r\n" +
        "1 1 1 1 1 1 1 0 1 0 1 0 1 0 1 0 1 0 1 1 1 1 1 1 1\r\n" +
        "0 0 0 0 0 0 0 0                   0 0 0 0 0 0 0 0\r\n" +
        "            1                                    \r\n" +
        "            0                                    \r\n" +
        "            1                                    \r\n" +
        "            0                                    \r\n" +
        "            1                                    \r\n" +
        "            0                                    \r\n" +
        "            1                                    \r\n" +
        "            0                                    \r\n" +
        "            1                   1 1 1 1 1        \r\n" +
        "0 0 0 0 0 0 0 0 1               1 0 0 0 1        \r\n" +
        "1 1 1 1 1 1 1 0                 1 0 1 0 1        \r\n" +
        "1 0 0 0 0 0 1 0                 1 0 0 0 1        \r\n" +
        "1 0 1 1 1 0 1 0                 1 1 1 1 1        \r\n" +
        "1 0 1 1 1 0 1 0                                  \r\n" +
        "1 0 1 1 1 0 1 0                                  \r\n" +
        "1 0 0 0 0 0 1 0                                  \r\n" +
        "1 1 1 1 1 1 1 0                                  ";

      Assert.AreEqual( expected, matrix.ToString());
    }

    [Test]
    public void TypeInfo()
    {
      const int WIDTH = 21, HEIGHT = 21;

      // Type info bits = 100000011001110.
      QRMatrix matrix = new QRMatrix(WIDTH, HEIGHT);

      matrix.AddTypeInfo(QRCorrectionLevel.M, 5);
      string expected =
        "                0                        \r\n" +
        "                1                        \r\n" +
        "                1                        \r\n" +
        "                1                        \r\n" +
        "                0                        \r\n" +
        "                0                        \r\n" +
        "                                         \r\n" +
        "                1                        \r\n" +
        "1 0 0 0 0 0   0 1         1 1 0 0 1 1 1 0\r\n" +
        "                                         \r\n" +
        "                                         \r\n" +
        "                                         \r\n" +
        "                                         \r\n" +
        "                                         \r\n" +
        "                0                        \r\n" +
        "                0                        \r\n" +
        "                0                        \r\n" +
        "                0                        \r\n" +
        "                0                        \r\n" +
        "                0                        \r\n" +
        "                1                        ";

      Assert.AreEqual( expected, matrix.ToString());
    }

    [Test]
    public void VersionInfo()
    {
      const int WIDTH = 21, HEIGHT = 21;
      // Version info bits = 000111 110010 010100
      // Actually, version 7 QR Code has 45x45 matrix but we use 21x21 here
      // since 45x45 matrix is too big to depict.
      QRMatrix matrix = new QRMatrix(WIDTH, HEIGHT);

      matrix.AddVersionInfoIfRequired(QRVersion.GetVersionByNumber(7));

      string expected =
        "                    0 0 1                \r\n" +
        "                    0 1 0                \r\n" +
        "                    0 1 0                \r\n" +
        "                    0 1 1                \r\n" +
        "                    1 1 1                \r\n" +
        "                    0 0 0                \r\n" +
        "                                         \r\n" +
        "                                         \r\n" +
        "                                         \r\n" +
        "                                         \r\n" +
        "0 0 0 0 1 0                              \r\n" +
        "0 1 1 1 1 0                              \r\n" +
        "1 0 0 1 1 0                              \r\n" +
        "                                         \r\n" +
        "                                         \r\n" +
        "                                         \r\n" +
        "                                         \r\n" +
        "                                         \r\n" +
        "                                         \r\n" +
        "                                         \r\n" +
        "                                         ";

      Assert.AreEqual( expected, matrix.ToString());
    }

    [Test]
    public void DataBits() 
    {
      const int WIDTH = 21, HEIGHT = 21;

      //System.Diagnostics.Debugger.Launch();

      // Cells other than basic patterns should be filled with zero.
      QRMatrix matrix = new QRMatrix(WIDTH, HEIGHT);

      matrix.AddBasicPatterns(QRVersion.GetVersionByNumber(1));

      BitList bits = new BitList();

      matrix.InsertDataBits(bits, -1);

      string expected =
        "1 1 1 1 1 1 1 0 0 0 0 0 0 0 1 1 1 1 1 1 1\r\n" +
        "1 0 0 0 0 0 1 0 0 0 0 0 0 0 1 0 0 0 0 0 1\r\n" +
        "1 0 1 1 1 0 1 0 0 0 0 0 0 0 1 0 1 1 1 0 1\r\n" +
        "1 0 1 1 1 0 1 0 0 0 0 0 0 0 1 0 1 1 1 0 1\r\n" +
        "1 0 1 1 1 0 1 0 0 0 0 0 0 0 1 0 1 1 1 0 1\r\n" +
        "1 0 0 0 0 0 1 0 0 0 0 0 0 0 1 0 0 0 0 0 1\r\n" +
        "1 1 1 1 1 1 1 0 1 0 1 0 1 0 1 1 1 1 1 1 1\r\n" +
        "0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0\r\n" +
        "0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\r\n" +
        "0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0\r\n" +
        "0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\r\n" +
        "0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0\r\n" +
        "0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\r\n" +
        "0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0\r\n" +
        "1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\r\n" +
        "1 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\r\n" +
        "1 0 1 1 1 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\r\n" +
        "1 0 1 1 1 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\r\n" +
        "1 0 1 1 1 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\r\n" +
        "1 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\r\n" +
        "1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0";

      Assert.AreEqual( expected, matrix.ToString());
    }

    [Test]
    public void BuildMatrix() 
    {
      const int WIDTH = 21, HEIGHT = 21;

      // From http://www.swetake.com/qr/qr7.html
      int[] ints = {
          32, 65, 205, 69, 41, 220, 46, 128, 236,
          42, 159, 74, 221, 244, 169, 239, 150, 138,
          70, 237, 85, 224, 96, 74, 219 , 61};

      BitList bits = new BitList();
      foreach (int i in ints) 
        bits.AppendBits(i, 8);

      QRMatrix matrix = new QRMatrix(WIDTH, HEIGHT);

      matrix.FormMatrix(bits, QRCorrectionLevel.H, QRVersion.GetVersionByNumber(1), 3);

      string expected =
        "1 1 1 1 1 1 1 0 0 1 1 0 0 0 1 1 1 1 1 1 1\r\n" +
        "1 0 0 0 0 0 1 0 0 0 0 0 0 0 1 0 0 0 0 0 1\r\n" +
        "1 0 1 1 1 0 1 0 0 0 0 1 0 0 1 0 1 1 1 0 1\r\n" +
        "1 0 1 1 1 0 1 0 0 1 1 0 0 0 1 0 1 1 1 0 1\r\n" +
        "1 0 1 1 1 0 1 0 1 1 0 0 1 0 1 0 1 1 1 0 1\r\n" +
        "1 0 0 0 0 0 1 0 0 0 1 1 1 0 1 0 0 0 0 0 1\r\n" +
        "1 1 1 1 1 1 1 0 1 0 1 0 1 0 1 1 1 1 1 1 1\r\n" +
        "0 0 0 0 0 0 0 0 1 1 0 1 1 0 0 0 0 0 0 0 0\r\n" +
        "0 0 1 1 0 0 1 1 1 0 0 1 1 1 1 0 1 0 0 0 0\r\n" +
        "1 0 1 0 1 0 0 0 0 0 1 1 1 0 0 1 0 1 1 1 0\r\n" +
        "1 1 1 1 0 1 1 0 1 0 1 1 1 0 0 1 1 1 0 1 0\r\n" +
        "1 0 1 0 1 1 0 1 1 1 0 0 1 1 1 0 0 1 0 1 0\r\n" +
        "0 0 1 0 0 1 1 1 0 0 0 0 0 0 1 0 1 1 1 1 1\r\n" +
        "0 0 0 0 0 0 0 0 1 1 0 1 0 0 0 0 0 1 0 1 1\r\n" +
        "1 1 1 1 1 1 1 0 1 1 1 1 0 0 0 0 1 0 1 1 0\r\n" +
        "1 0 0 0 0 0 1 0 0 0 0 1 0 1 1 1 0 0 0 0 0\r\n" +
        "1 0 1 1 1 0 1 0 0 1 0 0 1 1 0 0 1 0 0 1 1\r\n" +
        "1 0 1 1 1 0 1 0 1 1 0 1 0 0 0 0 0 1 1 1 0\r\n" +
        "1 0 1 1 1 0 1 0 1 1 1 1 0 0 0 0 1 1 1 0 0\r\n" +
        "1 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 1 0 1 0 0\r\n" +
        "1 1 1 1 1 1 1 0 0 0 1 1 1 1 1 0 1 0 0 1 0";

      Assert.AreEqual( expected, matrix.ToString());
    }

    [Test]
    public void FindMSBSet() 
    {
      Assert.AreEqual(0, QRMatrix.CalculateMSBSet(0));
      Assert.AreEqual(1, QRMatrix.CalculateMSBSet(1));
      Assert.AreEqual(8, QRMatrix.CalculateMSBSet(0x80));
      Assert.AreEqual(32, QRMatrix.CalculateMSBSet(-1)); // -1 = 0x80000000
    }

    [Test]
    public void CalculateBCHCode() 
    {
      // Encoding of type information.
      // From Appendix C in JISX0510:2004 (p 65)
      Assert.AreEqual( 0xdc, QRMatrix.CalculateBCHCode(5, 0x537));
      // From http://www.swetake.com/qr/qr6.html
      Assert.AreEqual(0x1c2, QRMatrix.CalculateBCHCode(0x13, 0x537));
      // From http://www.swetake.com/qr/qr11.html
      Assert.AreEqual(0x214, QRMatrix.CalculateBCHCode(0x1b, 0x537));

      // Encofing of version information.
      // From Appendix D in JISX0510:2004 (p 68)
      Assert.AreEqual(0xc94, QRMatrix.CalculateBCHCode(7, 0x1f25));
      Assert.AreEqual(0x5bc, QRMatrix.CalculateBCHCode(8, 0x1f25));
      Assert.AreEqual(0xa99, QRMatrix.CalculateBCHCode(9, 0x1f25));
      Assert.AreEqual(0x4d3, QRMatrix.CalculateBCHCode(10, 0x1f25));
      Assert.AreEqual(0x9a6, QRMatrix.CalculateBCHCode(20, 0x1f25));
      Assert.AreEqual(0xd75, QRMatrix.CalculateBCHCode(30, 0x1f25));
      Assert.AreEqual(0xc69, QRMatrix.CalculateBCHCode(40, 0x1f25));
    }

    [Test]
    public void MakeVersionInfoBits()
    {
      // From Appendix D in JISX0510:2004 (p 68)
      BitList bits = new BitList();
      QRMatrix.GenerateVersionInfoBits(bits, QRVersion.GetVersionByNumber(7));
      Assert.AreEqual("00011111 00100101 00", bits.ToString());
    }

    [Test]
    public void MakeTypeInfoInfoBits()
    {
      // From Appendix C in JISX0510:2004 (p 65)
      BitList bits = new BitList();
      QRMatrix.GenerateTypeInfoBits(bits, QRCorrectionLevel.M, 5);
      Assert.AreEqual("10000001 1001110", bits.ToString());
    }
  }
}
