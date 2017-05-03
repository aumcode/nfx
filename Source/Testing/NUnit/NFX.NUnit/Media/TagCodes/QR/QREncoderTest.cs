/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
  public class QREncoderTest
  {
    [TestCase]
    public void Encode()
    {
      QREncoderMatrix qrCode = QREncoderMatrix.CreateMatrix("ABCDEF", QRCorrectionLevel.H);
      string expected =
        "Mode: ALPHANUMERIC\r\n" +
        "CorrectionLevel: H\r\n" +
        "Version: 1\r\n" +
        "Mask: 4\r\n" +
        "Matrix:\r\n" +
        "1 1 1 1 1 1 1 0 0 1 0 1 0 0 1 1 1 1 1 1 1\r\n" +
        "1 0 0 0 0 0 1 0 1 0 1 0 1 0 1 0 0 0 0 0 1\r\n" +
        "1 0 1 1 1 0 1 0 0 0 0 0 0 0 1 0 1 1 1 0 1\r\n" +
        "1 0 1 1 1 0 1 0 0 1 0 0 1 0 1 0 1 1 1 0 1\r\n" +
        "1 0 1 1 1 0 1 0 0 1 0 1 0 0 1 0 1 1 1 0 1\r\n" +
        "1 0 0 0 0 0 1 0 1 0 0 1 1 0 1 0 0 0 0 0 1\r\n" +
        "1 1 1 1 1 1 1 0 1 0 1 0 1 0 1 1 1 1 1 1 1\r\n" +
        "0 0 0 0 0 0 0 0 1 0 0 0 1 0 0 0 0 0 0 0 0\r\n" +
        "0 0 0 0 1 1 1 1 0 1 1 0 1 0 1 1 0 0 0 1 0\r\n" +
        "0 0 0 0 1 1 0 1 1 1 0 0 1 1 1 1 0 1 1 0 1\r\n" +
        "1 0 0 0 0 1 1 0 0 1 0 1 0 0 0 1 1 1 0 1 1\r\n" +
        "1 0 0 1 1 1 0 0 1 1 1 1 0 0 0 0 1 0 0 0 0\r\n" +
        "0 1 1 1 1 1 1 0 1 0 1 0 1 1 1 0 0 1 1 0 0\r\n" +
        "0 0 0 0 0 0 0 0 1 1 0 0 0 1 1 0 0 0 1 0 1\r\n" +
        "1 1 1 1 1 1 1 0 1 1 1 1 0 0 0 0 0 1 1 0 0\r\n" +
        "1 0 0 0 0 0 1 0 1 1 0 1 0 0 0 1 0 1 1 1 1\r\n" +
        "1 0 1 1 1 0 1 0 1 0 0 1 0 0 0 1 1 0 0 1 1\r\n" +
        "1 0 1 1 1 0 1 0 0 0 1 1 0 1 0 0 0 0 1 1 1\r\n" +
        "1 0 1 1 1 0 1 0 0 1 0 1 0 0 0 1 1 0 0 0 0\r\n" +
        "1 0 0 0 0 0 1 0 0 1 0 0 1 0 0 1 1 0 0 0 1\r\n" +
        "1 1 1 1 1 1 1 0 0 0 1 0 0 1 0 0 0 0 1 1 1";

      Assert.AreEqual(expected, qrCode.ToString());
    }

    [Test]
    public void AppendLengthInfo()
    {
      BitList bits = new BitList();
      QREncoderMatrix.AppendLengthInfo(1,  /* 1 letter (1/1).*/ QRVersion.GetVersionByNumber(1),  QRMode.NUMERIC, bits);
      Assert.AreEqual("00000000 01", bits.ToString());  // 10 bits.

      bits = new BitList();
      QREncoderMatrix.AppendLengthInfo(2,  /* 2 letters (2/1).*/  QRVersion.GetVersionByNumber(10), QRMode.ALPHANUMERIC, bits);
      Assert.AreEqual("00000000 010", bits.ToString());  // 11 bits.

      bits = new BitList();
      QREncoderMatrix.AppendLengthInfo(255,  /* 255 letter (255/1).*/ QRVersion.GetVersionByNumber(27), QRMode.BYTE, bits);
      Assert.AreEqual("00000000 11111111", bits.ToString());  // 16 bits.

      bits = new BitList();
      QREncoderMatrix.AppendLengthInfo(512,  /* 512 letters (1024/2).*/ QRVersion.GetVersionByNumber(40), QRMode.KANJI, bits);
      Assert.AreEqual("00100000 0000", bits.ToString());  // 12 bits.
    }

    [Test]
    public void AppendBytes()
    {
      // Should use appendNumericBytes.
      // 1 = 01 = 0001 in 4 bits.
      BitList bits = new BitList();
      QREncoderMatrix.AppendBytes("1", QRMode.NUMERIC, bits);
      Assert.AreEqual("0001" , bits.ToString());
      // Should use appendAlphanumericBytes.
      // A = 10 = 0xa = 001010 in 6 bits
      bits = new BitList();
      QREncoderMatrix.AppendBytes("A", QRMode.ALPHANUMERIC, bits);
      Assert.AreEqual("001010" , bits.ToString());

      bits = new BitList();
      QREncoderMatrix.AppendBytes("abc", QRMode.BYTE, bits);
      Assert.AreEqual("01100001 01100010 01100011", bits.ToString());
      // Anything can be encoded in QRCode.MODE_8BIT_BYTE.
      QREncoderMatrix.AppendBytes("\0", QRMode.BYTE, bits);
    }

    [Test]
    [ExpectedException(typeof(NFXException))]
    public void AppendBytesException()
    {
      BitList bits = new BitList();
      QREncoderMatrix.AppendBytes("A", QRMode.ALPHANUMERIC, bits);
      Assert.AreEqual("001010" , bits.ToString());
      // Lower letters such as 'a' cannot be encoded in MODE_ALPHANUMERIC.
      QREncoderMatrix.AppendBytes("a", QRMode.ALPHANUMERIC, bits);
    }

    [Test]
    public void TerminateBits()
    {
      BitList v = new BitList();
      QREncoderMatrix.WriteTerminationSection(0, v);
      Assert.AreEqual("", v.ToString());
      v = new BitList();
      QREncoderMatrix.WriteTerminationSection(1, v);
      Assert.AreEqual("00000000", v.ToString());
      v = new BitList();
      v.AppendBits(0, 3);  // Append 000
      QREncoderMatrix.WriteTerminationSection(1, v);
      Assert.AreEqual("00000000", v.ToString());
      v = new BitList();
      v.AppendBits(0, 5);  // Append 00000
      QREncoderMatrix.WriteTerminationSection(1, v);
      Assert.AreEqual("00000000", v.ToString());
      v = new BitList();
      v.AppendBits(0, 8);  // Append 00000000
      QREncoderMatrix.WriteTerminationSection(1, v);
      Assert.AreEqual("00000000", v.ToString());
      v = new BitList();
      QREncoderMatrix.WriteTerminationSection(2, v);
      Assert.AreEqual("00000000 11101100", v.ToString());
      v = new BitList();
      v.AppendBits(0, 1);  // Append 0
      QREncoderMatrix.WriteTerminationSection(3, v);
      Assert.AreEqual("00000000 11101100 00010001", v.ToString());
    }

    [Test]
    public void GetNumDataBytesAndNumECBytesForBlockID()
    {
      int[] numDataBytes = new int[1];
      int[] numEcBytes = new int[1];
      // Version 1-H.
      QREncoderMatrix.GetNumDataBytesAndNumCorrectionBytesByBlockID(26, 9, 1, 0, numDataBytes, numEcBytes);
      Assert.AreEqual(9, numDataBytes[0]);
      Assert.AreEqual(17, numEcBytes[0]);

      // Version 3-H.  2 blocks.
      QREncoderMatrix.GetNumDataBytesAndNumCorrectionBytesByBlockID(70, 26, 2, 0, numDataBytes, numEcBytes);
      Assert.AreEqual(13, numDataBytes[0]);
      Assert.AreEqual(22, numEcBytes[0]);
      QREncoderMatrix.GetNumDataBytesAndNumCorrectionBytesByBlockID(70, 26, 2, 1, numDataBytes, numEcBytes);
      Assert.AreEqual(13, numDataBytes[0]);
      Assert.AreEqual(22, numEcBytes[0]);

      // Version 7-H. (4 + 1) blocks.
      QREncoderMatrix.GetNumDataBytesAndNumCorrectionBytesByBlockID(196, 66, 5, 0, numDataBytes, numEcBytes);
      Assert.AreEqual(13, numDataBytes[0]);
      Assert.AreEqual(26, numEcBytes[0]);
      QREncoderMatrix.GetNumDataBytesAndNumCorrectionBytesByBlockID(196, 66, 5, 4, numDataBytes, numEcBytes);
      Assert.AreEqual(14, numDataBytes[0]);
      Assert.AreEqual(26, numEcBytes[0]);

      // Version 40-H. (20 + 61) blocks.
      QREncoderMatrix.GetNumDataBytesAndNumCorrectionBytesByBlockID(3706, 1276, 81, 0, numDataBytes, numEcBytes);
      Assert.AreEqual(15, numDataBytes[0]);
      Assert.AreEqual(30, numEcBytes[0]);
      QREncoderMatrix.GetNumDataBytesAndNumCorrectionBytesByBlockID(3706, 1276, 81, 20, numDataBytes, numEcBytes);
      Assert.AreEqual(16, numDataBytes[0]);
      Assert.AreEqual(30, numEcBytes[0]);
      QREncoderMatrix.GetNumDataBytesAndNumCorrectionBytesByBlockID(3706, 1276, 81, 80, numDataBytes, numEcBytes);
      Assert.AreEqual(16, numDataBytes[0]);
      Assert.AreEqual(30, numEcBytes[0]);
    }

    [Test]
    public void InterleaveWithECBytes()
    {
      byte[] dataBytes = {32, 65, (byte)205, 69, 41, (byte)220, 46, (byte)128, (byte)236};
      BitList inList = new BitList();
      foreach (byte dataByte in dataBytes)
        inList.AppendBits(dataByte, 8);

      BitList outList = QREncoderMatrix.MixWithCorrectionBytes(inList, 26, 9, 1);
      byte[] expected = {
          // Data bytes.
          32, 65, (byte)205, 69, 41, (byte)220, 46, (byte)128, (byte)236,
          // Error correction bytes.
          42, (byte)159, 74, (byte)221, (byte)244, (byte)169, (byte)239, (byte)150, (byte)138, 70,
          (byte)237, 85, (byte)224, 96, 74, (byte)219, 61,
      };
      Assert.AreEqual(expected.Length, outList.ByteSize);
      byte[] outArray = new byte[expected.Length];
      outList.GetBytes(outArray, 0, 0, expected.Length);
      // Can't use Arrays.equals(), because outArray may be longer than out.sizeInBytes()
      for (int x = 0; x < expected.Length; x++) {
        Assert.AreEqual(expected[x], outArray[x]);
      }
      // Numbers are from http://www.swetake.com/qr/qr8.html
      dataBytes = new byte[] {
          67, 70, 22, 38, 54, 70, 86, 102, 118, (byte)134, (byte)150, (byte)166, (byte)182,
          (byte)198, (byte)214, (byte)230, (byte)247, 7, 23, 39, 55, 71, 87, 103, 119, (byte)135,
          (byte)151, (byte)166, 22, 38, 54, 70, 86, 102, 118, (byte)134, (byte)150, (byte)166,
          (byte)182, (byte)198, (byte)214, (byte)230, (byte)247, 7, 23, 39, 55, 71, 87, 103, 119,
          (byte)135, (byte)151, (byte)160, (byte)236, 17, (byte)236, 17, (byte)236, 17, (byte)236,
          17
      };
      inList = new BitList();
      foreach (byte dataByte in dataBytes)
        inList.AppendBits(dataByte, 8);

      outList = QREncoderMatrix.MixWithCorrectionBytes(inList, 134, 62, 4);
      expected = new byte[] {
          // Data bytes.
          67, (byte)230, 54, 55, 70, (byte)247, 70, 71, 22, 7, 86, 87, 38, 23, 102, 103, 54, 39,
          118, 119, 70, 55, (byte)134, (byte)135, 86, 71, (byte)150, (byte)151, 102, 87, (byte)166,
          (byte)160, 118, 103, (byte)182, (byte)236, (byte)134, 119, (byte)198, 17, (byte)150,
          (byte)135, (byte)214, (byte)236, (byte)166, (byte)151, (byte)230, 17, (byte)182,
          (byte)166, (byte)247, (byte)236, (byte)198, 22, 7, 17, (byte)214, 38, 23, (byte)236, 39,
          17,
          // Error correction bytes.
          (byte)175, (byte)155, (byte)245, (byte)236, 80, (byte)146, 56, 74, (byte)155, (byte)165,
          (byte)133, (byte)142, 64, (byte)183, (byte)132, 13, (byte)178, 54, (byte)132, 108, 45,
          113, 53, 50, (byte)214, 98, (byte)193, (byte)152, (byte)233, (byte)147, 50, 71, 65,
          (byte)190, 82, 51, (byte)209, (byte)199, (byte)171, 54, 12, 112, 57, 113, (byte)155, 117,
          (byte)211, (byte)164, 117, 30, (byte)158, (byte)225, 31, (byte)190, (byte)242, 38,
          (byte)140, 61, (byte)179, (byte)154, (byte)214, (byte)138, (byte)147, 87, 27, 96, 77, 47,
          (byte)187, 49, (byte)156, (byte)214,
      };
      Assert.AreEqual(expected.Length, outList.ByteSize);
      outArray = new byte[expected.Length];
      outList.GetBytes(outArray, 0, 0, expected.Length);
      for (int x = 0; x < expected.Length; x++) {
        Assert.AreEqual(expected[x], outArray[x]);
      }
    }

    [Test]
    public void AppendNumericBytes() 
    {
      // 1 = 01 = 0001 in 4 bits.
      BitList bits = new BitList();
      QREncoderMatrix.AppendNumericBytes("1", bits);
      Assert.AreEqual("0001" , bits.ToString());
      // 12 = 0xc = 0001100 in 7 bits.
      bits = new BitList();
      QREncoderMatrix.AppendNumericBytes("12", bits);
      Assert.AreEqual("0001100" , bits.ToString());
      // 123 = 0x7b = 0001111011 in 10 bits.
      bits = new BitList();
      QREncoderMatrix.AppendNumericBytes("123", bits);
      Assert.AreEqual("00011110 11" , bits.ToString());
      // 1234 = "123" + "4" = 0001111011 + 0100
      bits = new BitList();
      QREncoderMatrix.AppendNumericBytes("1234", bits);
      Assert.AreEqual("00011110 110100" , bits.ToString());
      // Empty.
      bits = new BitList();
      QREncoderMatrix.AppendNumericBytes("", bits);
      Assert.AreEqual("" , bits.ToString());
    }

    [Test]
    public void AppendAlphanumericBytes()
    {
      // A = 10 = 0xa = 001010 in 6 bits
      BitList bits = new BitList();
      QREncoderMatrix.AppendAlphanumericBytes("A", bits);
      Assert.AreEqual("001010" , bits.ToString());
      // AB = 10 * 45 + 11 = 461 = 0x1cd = 00111001101 in 11 bits
      bits = new BitList();
      QREncoderMatrix.AppendAlphanumericBytes("AB", bits);
      Assert.AreEqual("00111001 101", bits.ToString());
      // ABC = "AB" + "C" = 00111001101 + 001100
      bits = new BitList();
      QREncoderMatrix.AppendAlphanumericBytes("ABC", bits);
      Assert.AreEqual("00111001 10100110 0" , bits.ToString());
      // Empty.
      bits = new BitList();
      QREncoderMatrix.AppendAlphanumericBytes("", bits);
      Assert.AreEqual("" , bits.ToString());
    }

    [Test]
    [ExpectedException(typeof(NFXException))]
    public void AppendAlphanumericBytesException()
    {
      QREncoderMatrix.AppendAlphanumericBytes("abc", new BitList());
    }

    [Test]
    public void Append8BitBytes()
    {
      // 0x61, 0x62, 0x63
      BitList bits = new BitList();
      QREncoderMatrix.Append8BitBytes("abc", bits);
      Assert.AreEqual("01100001 01100010 01100011", bits.ToString());
      // Empty.
      bits = new BitList();
      QREncoderMatrix.Append8BitBytes("", bits);
      Assert.AreEqual("", bits.ToString());
    }

    [Test]
    public void GenerateECBytes() 
    {
      byte[] dataBytes = {32, 65, (byte)205, 69, 41, (byte)220, 46, (byte)128, (byte)236};
      byte[] ecBytes = QREncoderMatrix.GetCorrectionBytes(dataBytes, 17);
      int[] expected = { 42, 159, 74, 221, 244, 169, 239, 150, 138, 70, 237, 85, 224, 96, 74, 219, 61};
      Assert.AreEqual(expected.Length, ecBytes.Length);
      for (int x = 0; x < expected.Length; x++) {
        Assert.AreEqual(expected[x], ecBytes[x] & 0xFF);
      }
      dataBytes = new byte[] {67, 70, 22, 38, 54, 70, 86, 102, 118,
          (byte)134, (byte)150, (byte)166, (byte)182, (byte)198, (byte)214};
      ecBytes = QREncoderMatrix.GetCorrectionBytes(dataBytes, 18);
      expected = new int[] { 175, 80, 155, 64, 178, 45, 214, 233, 65, 209, 12, 155, 117, 31, 140, 214, 27, 187};
      Assert.AreEqual(expected.Length, ecBytes.Length);
      for (int x = 0; x < expected.Length; x++) {
        Assert.AreEqual(expected[x], ecBytes[x] & 0xFF);
      }
      // High-order zero coefficient case.
      dataBytes = new byte[] {32, 49, (byte)205, 69, 42, 20, 0, (byte)236, 17};
      ecBytes = QREncoderMatrix.GetCorrectionBytes(dataBytes, 17);
      expected = new int[] { 0, 3, 130, 179, 194, 0, 55, 211, 110, 79, 98, 72, 170, 96, 211, 137, 213};
      Assert.AreEqual(expected.Length, ecBytes.Length);
      for (int x = 0; x < expected.Length; x++) {
        Assert.AreEqual(expected[x], ecBytes[x] & 0xFF);
      }
    }
  }
}
