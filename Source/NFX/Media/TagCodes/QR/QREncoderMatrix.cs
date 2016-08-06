
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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 1.0  2013.12.18
 * Author: Denis Latushkin<dxwizard@gmail.com>
 * Based on zXing / Apache 2.0; See NOTICE and CHANGES for attribution
 */
using System;
using System.Collections.Generic;
using System.Text;

using NFX.Collections;
using NFX.IO.ErrorHandling;

namespace NFX.Media.TagCodes.QR
{
  public class QREncoderMatrix: QRMatrix
  {
    #region CONSTS

      public const string DEFAULT_ENCODING = "ISO-8859-1";

      private static readonly int[] ALPHANUMERIC_TABLE =
      {
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  // 0x00-0x0f
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  // 0x10-0x1f
        36, -1, -1, -1, 37, 38, -1, -1, -1, -1, 39, 40, -1, 41, 42, 43,  // 0x20-0x2f
        0,   1,  2,  3,  4,  5,  6,  7,  8,  9, 44, -1, -1, -1, -1, -1,  // 0x30-0x3f
        -1, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,  // 0x40-0x4f
        25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, -1, -1, -1, -1, -1,  // 0x50-0x5f
      };

      private const int QUIET_ZONE = 4;

    #endregion

    #region Static

      public static QREncoderMatrix CreateMatrix(string content, QRCorrectionLevel correctionLevel)
      {
        QREncoderMatrix matrix = Encode(content, correctionLevel);
        return matrix;
      }

      public static QREncoderMatrix Encode(string content, QRCorrectionLevel correctionLevel)
      {
        string encoding = DEFAULT_ENCODING;

        QRMode mode = chooseMode(content, encoding);

        BitList header = new BitList();
        header.AppendBits(mode.ModeSignature, 4);

        BitList data = new BitList();
        AppendBytes(content, mode, data);

        int provisionalBitsNeeded = header.Size + mode.GetVersionCharacterCount(QRVersion.GetVersionByNumber(1)) + data.Size;
        QRVersion provisionalVersion = chooseVersion(provisionalBitsNeeded, correctionLevel);

        int bitsNeeded = header.Size + mode.GetVersionCharacterCount(provisionalVersion) + data.Size;
        QRVersion version = chooseVersion(bitsNeeded, correctionLevel);

        BitList headerNData = new BitList();

        headerNData.AppendBitList(header);

        int numLetters = mode == QRMode.BYTE ? data.ByteSize : content.Length;

        AppendLengthInfo(numLetters, version, mode, headerNData);

        headerNData.AppendBitList(data);

        QRVersion.CorrectionBlockSet correctionBlockSet = version.GetBlockSetByLevel(correctionLevel);
        int dataBytesQty = version.TotalCodewords - correctionBlockSet.TotalCodewords;

        WriteTerminationSection(dataBytesQty, headerNData);

        BitList finalBits = MixWithCorrectionBytes(headerNData, version.TotalCodewords, dataBytesQty, correctionBlockSet.TotalQty);

        int dimension = version.Dimension;
        QREncoderMatrix matrix = new QREncoderMatrix( dimension, content, correctionLevel, mode, version);

        int maskPattern = chooseMaskPattern(finalBits, correctionLevel, version, matrix);

        matrix.MaskPattern = maskPattern;

        matrix.FormMatrix(finalBits, correctionLevel, version, maskPattern);

        return matrix;
      }

      public static void AppendLengthInfo(int numLetters, QRVersion version, QRMode mode, BitList bits)
      {
         int numBits = mode.GetVersionCharacterCount(version);
         if (numLetters >= (1 << numBits))
           throw new NFXException(StringConsts.CODE_LOGIC_ERROR + typeof(QREncoderMatrix).Name + ".appendLengthInfo(numLetters >= (1 << numBits))");

         bits.AppendBits(numLetters, numBits);
      }

      public static void AppendBytes(String content, QRMode mode, BitList bits)
      {
        if (mode.Equals(QRMode.NUMERIC))
          AppendNumericBytes(content, bits);
        else if (mode.Equals(QRMode.ALPHANUMERIC))
          AppendAlphanumericBytes(content, bits);
        else if (mode.Equals(QRMode.BYTE))
          Append8BitBytes(content, bits);
        else
          throw new NFXException(StringConsts.ARGUMENT_ERROR + typeof(QREncoderMatrix).Name + ".appendBytes(mode=NUMERIC|ALPHANUMERIC|BYTE)");
      }

      /// <summary>
      /// Write termination section according 8.4.8 and 8.4.9 of JISX0510:2004 (p.24).
      /// </summary>
      public static void WriteTerminationSection(int dataBytesQty, BitList bits)
      {
        int capacity = dataBytesQty << 3;
        if (bits.Size > capacity)
          throw new NFXException(StringConsts.ARGUMENT_ERROR + typeof(QREncoderMatrix).Name + ".writeTerminationSection(bits.Size>(dataBytesQty<<3))");

        for (int i = 0; i < 4 && bits.Size < capacity; ++i)
          bits.AppendBit(false);

        // Termination bits (JISX0510:2004 p.24 8.4.8)
        // Add padding bits when last byte isn't 8bit aligned
        int numBitsInLastByte = bits.Size & 0x07;
        if (numBitsInLastByte > 0)
          for (int i = numBitsInLastByte; i < 8; i++)
              bits.AppendBit(false);

        // According to JISX0510:2004 8.4.9 p.24 fill possible free space with pre-defined pattern
        int numPaddingBytes = dataBytesQty - bits.ByteSize;
        for (int i = 0; i < numPaddingBytes; ++i)
          bits.AppendBits((i & 0x01) == 0 ? 0xEC : 0x11, 8);

        if (bits.Size != capacity)
          throw new NFXException(StringConsts.CODE_LOGIC_ERROR + typeof(QREncoderMatrix).Name + ".writeTerminationSection: bits.Size!=capacity");
      }

      /// <summary>
      /// According to JISX0510:2004 (p.30) tbl 12 in 8.5.1
      /// Calculate data and correction bytes for block
      /// </summary>
      /// <param name="numTotalBytes">Total bytes count</param>
      /// <param name="numDataBytes">Data bytes count</param>
      /// <param name="numRSBlocks">Reed/Solomon blocks count</param>
      /// <param name="blockID">Block Id</param>
      /// <param name="numDataBytesInBlock">Data bytes count in this block</param>
      /// <param name="numECBytesInBlock">Correction bytes count in this block</param>
      public static void GetNumDataBytesAndNumCorrectionBytesByBlockID(int numTotalBytes, int numDataBytes, int numRSBlocks,
                                                         int blockID, int[] numDataBytesInBlock, int[] numECBytesInBlock)
      {
        if (blockID >= numRSBlocks)
          throw new NFXException(StringConsts.ARGUMENT_ERROR + typeof(QREncoderMatrix).Name + ".getNumDataBytesAndNumECBytesForBlockID(blockID<numRSBlocks)");

        int numRsBlocksInGroup2 = numTotalBytes % numRSBlocks;
        int numRsBlocksInGroup1 = numRSBlocks - numRsBlocksInGroup2;
        int numTotalBytesInGroup1 = numTotalBytes / numRSBlocks;
        int numTotalBytesInGroup2 = numTotalBytesInGroup1 + 1;
        int numDataBytesInGroup1 = numDataBytes / numRSBlocks;
        int numDataBytesInGroup2 = numDataBytesInGroup1 + 1;
        int numEcBytesInGroup1 = numTotalBytesInGroup1 - numDataBytesInGroup1;
        int numEcBytesInGroup2 = numTotalBytesInGroup2 - numDataBytesInGroup2;

        if (numEcBytesInGroup1 != numEcBytesInGroup2)
          throw new NFXException(StringConsts.CODE_LOGIC_ERROR + typeof(QREncoderMatrix).Name +
            ".getNumDataBytesAndNumECBytesForBlockID: numEcBytesInGroup1!=numEcBytesInGroup2");

        if (numRSBlocks != numRsBlocksInGroup1 + numRsBlocksInGroup2)
          throw new NFXException(StringConsts.CODE_LOGIC_ERROR + typeof(QREncoderMatrix).Name +
            ".getNumDataBytesAndNumECBytesForBlockID: numRSBlocks != numRsBlocksInGroup1 + numRsBlocksInGroup2");

        if (numTotalBytes !=
            ((numDataBytesInGroup1 + numEcBytesInGroup1) *
                numRsBlocksInGroup1) +
                ((numDataBytesInGroup2 + numEcBytesInGroup2) *
                    numRsBlocksInGroup2))
          throw new NFXException(StringConsts.CODE_LOGIC_ERROR + typeof(QREncoderMatrix).Name +
            ".getNumDataBytesAndNumECBytesForBlockID: numTotalBytes!=((numDataBytesInGroup1+numEcBytesInGroup1)*numRsBlocksInGroup1)+((numDataBytesInGroup2+numEcBytesInGroup2)*numRsBlocksInGroup2)");

        if (blockID < numRsBlocksInGroup1)
        {
          numDataBytesInBlock[0] = numDataBytesInGroup1;
          numECBytesInBlock[0] = numEcBytesInGroup1;
        }
        else
        {
          numDataBytesInBlock[0] = numDataBytesInGroup2;
          numECBytesInBlock[0] = numEcBytesInGroup2;
        }
      }

      /// <summary>
      /// According to JISX0510:2004 8.6 p.37 bits are mixed mixes with their correction bytes.
      /// </summary>
      /// <param name="bits">Data bits</param>
      /// <param name="numTotalBytes">Total bytes count</param>
      /// <param name="numDataBytes">Data bytes count</param>
      /// <param name="rsBlocksQty">Reed/Solomon blocks count</param>
      /// <returns>Mixed bits</returns>
      public static BitList MixWithCorrectionBytes(BitList bits, int numTotalBytes, int numDataBytes, int rsBlocksQty)
      {
        if (bits.ByteSize != numDataBytes)
          throw new NFXException(StringConsts.ARGUMENT_ERROR + typeof(QREncoderMatrix).Name + ".interleaveWithECBytes(bits.ByteSize != numDataBytes)");

        // Split data bytes into blocks. Generate error correction bytes.
        int dataBytesOffset = 0;
        int maxNumDataBytes = 0;
        int maxNumEcBytes = 0;

        var blocks = new List<QRDataNCorrection>(rsBlocksQty);

        for (int i = 0; i < rsBlocksQty; ++i)
        {
          int[] numDataBytesInBlock = new int[1];
          int[] numEcBytesInBlock = new int[1];
          GetNumDataBytesAndNumCorrectionBytesByBlockID( numTotalBytes, numDataBytes, rsBlocksQty, i, numDataBytesInBlock, numEcBytesInBlock);

          int size = numDataBytesInBlock[0];
          byte[] dataBytes = new byte[size];
          bits.GetBytes(dataBytes, 8 * dataBytesOffset, 0, size);
          byte[] ecBytes = GetCorrectionBytes(dataBytes, numEcBytesInBlock[0]);
          blocks.Add(new QRDataNCorrection(dataBytes, ecBytes));

          maxNumDataBytes = Math.Max(maxNumDataBytes, size);
          maxNumEcBytes = Math.Max(maxNumEcBytes, ecBytes.Length);
          dataBytesOffset += numDataBytesInBlock[0];
        }

        if (numDataBytes != dataBytesOffset)
          throw new NFXException(StringConsts.CODE_LOGIC_ERROR + typeof(QREncoderMatrix).Name + ".interleaveWithECBytes: numDataBytes!=dataBytesOffset");

        BitList result = new BitList();

        for (int i = 0; i < maxNumDataBytes; ++i)
          foreach (QRDataNCorrection block in blocks)
          {
            byte[] dataBytes = block.Data;
            if (i < dataBytes.Length)
              result.AppendBits(dataBytes[i], 8);
          }

        for (int i = 0; i < maxNumEcBytes; ++i)
          foreach (QRDataNCorrection block in blocks)
          {
            byte[] ecBytes = block.Correction;
            if (i < ecBytes.Length)
              result.AppendBits(ecBytes[i], 8);
          }

        if (numTotalBytes != result.ByteSize)
          throw new NFXException(StringConsts.CODE_LOGIC_ERROR + typeof(QREncoderMatrix).Name + ".interleaveWithECBytes: numTotalBytes!=result.ByteSize");

        return result;
      }

      public static void AppendNumericBytes(String content, BitList bits)
      {
        int length = content.Length;

        int i = 0;
        while (i < length)
        {
          int num1 = content[i] - '0';
          if (i + 2 < length)
          {
              // 3 num letters => 10 bits
              int num2 = content[i + 1] - '0';
              int num3 = content[i + 2] - '0';
              bits.AppendBits(num1 * 100 + num2 * 10 + num3, 10);
              i += 3;
          }
          else if (i + 1 < length)
          {
              // 2 num letters => 7 bits
              int num2 = content[i + 1] - '0';
              bits.AppendBits(num1 * 10 + num2, 7);
              i += 2;
          }
          else
          {
              // 1 num letters => 4 bits
              bits.AppendBits(num1, 4);
              i++;
          }
        }
      }

      public static void AppendAlphanumericBytes(String content, BitList bits)
      {
        int length = content.Length;

        int i = 0;
        while (i < length)
        {
          int code1 = getAlphanumericCode(content[i]);
          if (code1 == -1)
          {
              throw new NFXException(StringConsts.ARGUMENT_ERROR + typeof(QREncoderMatrix).Name + ".appendAlphanumericBytes(content[i] in ALPHANUMERIC_TABLE)");
          }
          if (i + 1 < length)
          {
              int code2 = getAlphanumericCode(content[i + 1]);
              if (code2 == -1)
              {
                throw new NFXException(StringConsts.ARGUMENT_ERROR + typeof(QREncoderMatrix).Name + ".appendAlphanumericBytes(content[i] in ALPHANUMERIC_TABLE)");
              }
              // 2 alphanum letters => 11 bits
              bits.AppendBits(code1 * 45 + code2, 11);
              i += 2;
          }
          else
          {
            // 1 alphanum letters => 6 bits
            bits.AppendBits(code1, 6);
            i++;
          }
        }
      }

      public static void Append8BitBytes(String content, BitList bits)
      {
        byte[] bytes;
        try
        {
          bytes = Encoding.GetEncoding(DEFAULT_ENCODING).GetBytes(content);
        }
        catch (Exception)
        {
          throw new NFXException(StringConsts.CODE_LOGIC_ERROR + typeof(QREncoderMatrix).Name + ".append8BitBytes: Encoding.GetEncoding(DEFAULT_BYTE_MODE_ENCODING).GetBytes(content)");
        }
        foreach (byte b in bytes)
        {
          bits.AppendBits(b, 8);
        }
      }

      public static byte[] GetCorrectionBytes(byte[] dataBytes, int numEcBytesInBlock)
      {
         int numDataBytes = dataBytes.Length;
         int[] toEncode = new int[numDataBytes + numEcBytesInBlock];
         for (int i = 0; i < numDataBytes; i++)
            toEncode[i] = dataBytes[i] & 0xFF;

         (new ReedSolomonEncoder(GaloisField.QRCODE_256)).Encode(toEncode, numEcBytesInBlock);

         byte[] correctionBytes = new byte[numEcBytesInBlock];
         for (int i = 0; i < numEcBytesInBlock; i++)
            correctionBytes[i] = (byte)toEncode[numDataBytes + i];

         return correctionBytes;
      }

    #endregion

    #region .ctor

      private QREncoderMatrix(int dimension, string content, QRCorrectionLevel correctionLevel, QRMode mode, QRVersion version)
        : base(dimension, dimension)
      {
        Content = content;
        CorrectionLevel = correctionLevel;
        Mode = mode;
        Version = version;
      }

    #endregion

    #region Properties

      public readonly string Content;
      public readonly QRCorrectionLevel CorrectionLevel;

      public QRMode Mode { get; private set; }
      public QRVersion Version { get; private set; }
      public int MaskPattern { get; private set; }

    #endregion

    #region Protected

      public override string ToString()
      {
        StringBuilder b = new StringBuilder();

        b.AppendFormat("Mode: {0}", Mode); b.AppendLine();
        b.AppendFormat("CorrectionLevel: {0}", CorrectionLevel); b.AppendLine();
        b.AppendFormat("Version: {0}", Version != null ? Version.ToString() : "null"); b.AppendLine();
        b.AppendFormat("Mask: {0}", MaskPattern); b.AppendLine();
        b.Append("Matrix:");

        b.AppendLine();
        b.Append(base.ToString());

        return b.ToString();
      }

	  #endregion

    #region .pvt. impl.

      private static QRMode chooseMode(String content, String encoding)
      {
        bool hasNumeric = false;
        bool hasAlphanumeric = false;
        for (int i = 0; i < content.Length; ++i)
        {
          char c = content[i];
          if (c >= '0' && c <= '9')
              hasNumeric = true;
          else if (getAlphanumericCode(c) != -1)
              hasAlphanumeric = true;
          else
              return QRMode.BYTE;
        }

        if (hasAlphanumeric)
          return QRMode.ALPHANUMERIC;

        if (hasNumeric)
          return QRMode.NUMERIC;

        return QRMode.BYTE;
      }

      private static int getAlphanumericCode(int code)
      {
        if (code < ALPHANUMERIC_TABLE.Length)
          return ALPHANUMERIC_TABLE[code];
        else
          return -1;
      }

      private static QRVersion chooseVersion(int numInputBits, QRCorrectionLevel ecLevel)
      {
        for (int versionNum = 1; versionNum <= 40; versionNum++)
        {
          QRVersion version = QRVersion.GetVersionByNumber(versionNum);
          int numBytes = version.TotalCodewords;
          QRVersion.CorrectionBlockSet correctionBlocks = version.GetBlockSetByLevel(ecLevel);
          int numEcBytes = correctionBlocks.TotalCodewords;
          int numDataBytes = numBytes - numEcBytes;
          int totalInputBytes = (numInputBits + 7) / 8;
          if (numDataBytes >= totalInputBytes)
              return version;
        }

        throw new NFXException(StringConsts.CODE_LOGIC_ERROR + typeof(QREncoderMatrix).Name + ".chooseVersion(data)");
      }

      private static int chooseMaskPattern(BitList bits, QRCorrectionLevel correctionLevel, QRVersion version, QRMatrix matrix)
      {
        int minPenalty = Int32.MaxValue;  // Assume the lowest possible penalty
        int bestMaskPattern = -1;
        // Calculate all mask paterns to find the pattern with minimum possible penalty
        for (int maskPattern = 0; maskPattern < MASK_PATTERNS_QTY; maskPattern++)
        {
          matrix.FormMatrix(bits, correctionLevel, version, maskPattern);
          int penalty = matrix.GetMaskPenalty();
          if (penalty < minPenalty)
          {
            minPenalty = penalty;
            bestMaskPattern = maskPattern;
          }
        }
        return bestMaskPattern;
      }

    #endregion

  }//class

}
