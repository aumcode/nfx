
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
 * Revision: NFX 1.0  2013.12.25
 * Author: Denis Latushkin<dxwizard@gmail.com>
 * Based on zXing / Apache 2.0; See NOTICE and CHANGES for attribution
 */
using System.Text;

using NFX.Collections;

namespace NFX.Media.TagCodes.QR
{
  public partial class QRMatrix: Matrix2D<byte>
  {
    #region CONSTS

      private static readonly byte[][] POSITION_DETECTION_PATTERN = new byte[][] {
        new byte[] { 1, 1, 1, 1, 1, 1, 1 },
        new byte[] { 1, 0, 0, 0, 0, 0, 1 },
        new byte[] { 1, 0, 1, 1, 1, 0, 1 },
        new byte[] { 1, 0, 1, 1, 1, 0, 1 },
        new byte[] { 1, 0, 1, 1, 1, 0, 1 },
        new byte[] { 1, 0, 0, 0, 0, 0, 1 },
        new byte[] { 1, 1, 1, 1, 1, 1, 1 }
      };

      private static readonly byte[][] POSITION_ADJUSTMENT_PATTERN = new byte[][] {
        new byte[] { 1, 1, 1, 1, 1 },
        new byte[] { 1, 0, 0, 0, 1 },
        new byte[] { 1, 0, 1, 0, 1 },
        new byte[] { 1, 0, 0, 0, 1 },
        new byte[] { 1, 1, 1, 1, 1 }
      };

      // From Appendix E. Table 1, JIS0510X:2004 (p 71). The table was double-checked by komatsu.
      private static readonly int[][] POSITION_ADJUSTMENT_PATTERN_COORDINATE_TABLE = new int[][] {
        new int[] { -1, -1, -1, -1, -1, -1, -1 },
        new int[] { 6, 18, -1, -1, -1, -1, -1 },
        new int[] { 6, 22, -1, -1, -1, -1, -1 },
        new int[] { 6, 26, -1, -1, -1, -1, -1 },
        new int[] { 6, 30, -1, -1, -1, -1, -1 },
        new int[] { 6, 34, -1, -1, -1, -1, -1 },
        new int[] { 6, 22, 38, -1, -1, -1, -1 },
        new int[] { 6, 24, 42, -1, -1, -1, -1 },
        new int[] { 6, 26, 46, -1, -1, -1, -1 },
        new int[] { 6, 28, 50, -1, -1, -1, -1 },
        new int[] { 6, 30, 54, -1, -1, -1, -1 },
        new int[] { 6, 32, 58, -1, -1, -1, -1 },
        new int[] { 6, 34, 62, -1, -1, -1, -1 },
        new int[] { 6, 26, 46, 66, -1, -1, -1 },
        new int[] { 6, 26, 48, 70, -1, -1, -1 },
        new int[] { 6, 26, 50, 74, -1, -1, -1 },
        new int[] { 6, 30, 54, 78, -1, -1, -1 },
        new int[] { 6, 30, 56, 82, -1, -1, -1 },
        new int[] { 6, 30, 58, 86, -1, -1, -1 },
        new int[] { 6, 34, 62, 90, -1, -1, -1 },
        new int[] { 6, 28, 50, 72, 94, -1, -1 },
        new int[] { 6, 26, 50, 74, 98, -1, -1 },
        new int[] { 6, 30, 54, 78, 102, -1, -1 },
        new int[] { 6, 28, 54, 80, 106, -1, -1 },
        new int[] { 6, 32, 58, 84, 110, -1, -1 },
        new int[] { 6, 30, 58, 86, 114, -1, -1 },
        new int[] { 6, 34, 62, 90, 118, -1, -1 },
        new int[] { 6, 26, 50, 74, 98, 122, -1 },
        new int[] { 6, 30, 54, 78, 102, 126, -1 },
        new int[] { 6, 26, 52, 78, 104, 130, -1 },
        new int[] { 6, 30, 56, 82, 108, 134, -1 },
        new int[] { 6, 34, 60, 86, 112, 138, -1 },
        new int[] { 6, 30, 58, 86, 114, 142, -1 },
        new int[] { 6, 34, 62, 90, 118, 146, -1 },
        new int[] { 6, 30, 54, 78, 102, 126, 150 },
        new int[] { 6, 24, 50, 76, 102, 128, 154 },
        new int[] { 6, 28, 54, 80, 106, 132, 158 },
        new int[] { 6, 32, 58, 84, 110, 136, 162 },
        new int[] { 6, 26, 54, 82, 110, 138, 166 },
        new int[] { 6, 30, 58, 86, 114, 142, 170 }
      };

      // Type info cells at the left top corner.
      private static readonly int[][] TYPE_INFO_COORDINATES = new int[][] {
        new int[] { 8, 0 },
        new int[] { 8, 1 },
        new int[] { 8, 2 },
        new int[] { 8, 3 },
        new int[] { 8, 4 },
        new int[] { 8, 5 },
        new int[] { 8, 7 },
        new int[] { 8, 8 },
        new int[] { 7, 8 },
        new int[] { 5, 8 },
        new int[] { 4, 8 },
        new int[] { 3, 8 },
        new int[] { 2, 8 },
        new int[] { 1, 8 },
        new int[] { 0, 8 }
      };

      // From Appendix D in JISX0510:2004 (p. 67)
      private const int VERSION_INFO_POLY = 0x1f25; // 1 1111 0010 0101

      // From Appendix C in JISX0510:2004 (p.65).
      private const int TYPE_INFO_POLY = 0x537;
      private const int TYPE_INFO_MASK_PATTERN = 0x5412;

      private const int hspWidth = 8;
      private const int vspSize = 7;
      private const byte EMPTY_BYTE = 0x02;

      protected const int MASK_PATTERNS_QTY = 8;

    #endregion

    #region Static

      public static void GenerateTypeInfoBits(BitList bits, QRCorrectionLevel correctionLevel, int maskPattern)
      {
        if (!IsValidMaskPattern(maskPattern))
          throw new NFXException(StringConsts.ARGUMENT_ERROR + typeof(QRMatrix).Name
            + ".GenerateTypeInfoBits(!QRCode.IsValidMaskPattern(maskPattern))");

        int typeInfo = (correctionLevel.MarkerBits << 3) | maskPattern;
        bits.AppendBits(typeInfo, 5);

        int bchCode = CalculateBCHCode(typeInfo, TYPE_INFO_POLY);
        bits.AppendBits(bchCode, 10);

        BitList maskBits = new BitList();
        maskBits.AppendBits(TYPE_INFO_MASK_PATTERN, 15);
        bits.Xor(maskBits);

        if (bits.Size != 15)
          throw new NFXException(StringConsts.CODE_LOGIC_ERROR + typeof(QRMatrix).Name + ".makeTypeInfoBits: bits.Size != 15");
      }

      public static void GenerateVersionInfoBits( BitList bits, QRVersion version)
      {
        bits.AppendBits(version.Number, 6);
        int bchCode = CalculateBCHCode(version.Number, VERSION_INFO_POLY);
        bits.AppendBits(bchCode, 12);

        if (bits.Size != 18)
          throw new NFXException(StringConsts.CODE_LOGIC_ERROR + typeof(QRMatrix).Name + ".makeVersionInfoBits: bits.Size != 18");
      }

      public static int CalculateMSBSet(int valueRenamed)
      {
        int numDigits = 0;
        while (valueRenamed != 0)
        {
          valueRenamed = (int)((uint)valueRenamed >> 1);
          numDigits++;
        }
        return numDigits;
      }

      public static int CalculateBCHCode(int value, int poly)
      {
        // MSB is 13 for poly "1 1111 0010 0101" (version info) so 1 should be subtracted to get 12
        int msbSetInPoly = CalculateMSBSet(poly);
        value <<= msbSetInPoly - 1;
        // Division by XOR
        while (CalculateMSBSet(value) >= msbSetInPoly)
          value ^= poly << (CalculateMSBSet(value) - msbSetInPoly);

        // Return remainder/BCH code
        return value;
      }

    #endregion

    #region .ctor

      public QRMatrix(int width, int height): base( width, height)
      {
        Clear();
      }

    #endregion

    #region Public

      public void FormMatrix( BitList dataBits, QRCorrectionLevel correctionLevel, QRVersion version, int maskPattern)
      {
        Fill(EMPTY_BYTE);

        AddBasicPatterns(version);
        // Type
        AddTypeInfo(correctionLevel, maskPattern);
        // Version (when >= 6).
        AddVersionInfoIfRequired(version);
        // Data
        InsertDataBits(dataBits, maskPattern);
      }

      public void AddBasicPatterns( QRVersion version)
      {
         // Corner squares
         addPositionDetectionPatternsAndSeparators();
         // Left bottom black dot
         addDarkDotAtLeftBottomCorner();

         // Position adjustment (if version > 1)
         possiblyAddPositionAdjustmentPatterns(version);
         // Then timing patterns
         addTimingPatterns();
      }

      public void Clear()
      {
        Fill(EMPTY_BYTE);
      }

      public void AddTypeInfo(QRCorrectionLevel ecLevel, int maskPattern)
      {
        BitList typeInfoBits = new BitList();
        GenerateTypeInfoBits(typeInfoBits, ecLevel, maskPattern);

        for (int i = 0; i < typeInfoBits.Size; ++i)
        {
          // "typeInfoBits": LSB to MSB
          byte bit = typeInfoBits[typeInfoBits.Size - 1 - i] ? (byte)1 : (byte)0;

          // TypeInfoBits -> left top ( 8.9 of JISX0510:2004 (p.46))
          int x1 = TYPE_INFO_COORDINATES[i][0];
          int y1 = TYPE_INFO_COORDINATES[i][1];
          this[x1, y1] = bit;

          if (i < 8)
          {
            // Right Top
            int x2 = Width - i - 1;
            int y2 = 8;
            this[x2, y2] = bit;
          }
          else
          {
            // Left Bottom
            int x2 = 8;
            int y2 = Height - 7 + (i - 8);
            this[x2, y2] = bit;
          }
        }
      }

      public void AddVersionInfoIfRequired(QRVersion version)
      {
        if (version.Number < 7)
          return;

        BitList versionInfoBits = new BitList();
        GenerateVersionInfoBits(versionInfoBits, version);

        int bitIndex = 6 * 3 - 1; // 17 decrease to 0
        for (int i = 0; i < 6; ++i)
        {
          for (int j = 0; j < 3; ++j)
          {
            // LSB->MSB
            byte bit = versionInfoBits[bitIndex] ? (byte)1 : (byte)0;
            bitIndex--;
            // Left Bottom
            this[i, Height - 11 + j] = bit;
            // Right Bottom
            this[Height - 11 + j, i] = bit;
          }
        }
      }

      public void InsertDataBits(BitList dataBits, int maskPattern)
      {
        int bitIndex = 0;
        int direction = -1;
        //Initial is right bottom
        int x = Width - 1;
        int y = Height - 1;
        while (x > 0)
        {
          // No vertical timing pattern.
          if (x == 6)
            x -= 1;

          while (y >= 0 && y < Height)
          {
            for (int i = 0; i < 2; ++i)
            {
              int xx = x - i;
              // Skip filled cells
              if (this[xx, y] != EMPTY_BYTE)
                continue;

              byte bit;
              if (bitIndex < dataBits.Size)
              {
                bit = dataBits[bitIndex] ? (byte)1 : (byte)0;
                ++bitIndex;
              }
              else
              {
                // Padding. According to 8.4.9 of JISX0510:2004 (p. 24): when no bit left left cells are filled with 0
                bit = 0;
              }

              // No mask for pattern==-1
              if (maskPattern != -1)
              {
                if (GetDataMaskBit(maskPattern, xx, y))
                  bit ^= 0x1;

              }
              this[xx, y] = bit;
            }
            y += direction;
          }
          direction = -direction; // Inverse direction.
          y += direction;
          x -= 2; // go left
        }

        // Check if all bits are used
        if (bitIndex != dataBits.Size)
          throw new NFXException(StringConsts.CODE_LOGIC_ERROR + GetType().Name + ".makeVersionInfoBits: bitIndex != dataBits.Size");
      }

    #endregion

    #region Protected

      protected static bool IsValidMaskPattern(int maskPattern)
      {
         return maskPattern >= 0 && maskPattern < MASK_PATTERNS_QTY;
      }

      public override string ToString()
      {
        StringBuilder b = new StringBuilder();

        for (int y = 0; y < Height; y++)
        {
          if (y != 0)
            b.AppendLine();

          for (int x = 0; x < Width; x++)
          {
            if (x != 0)
              b.Append(' ');

            char ch;

            switch (this[x, y])
            {
              case 0:
                ch = '0';
                break;
              case 1:
                ch = '1';
                break;
              default:
                ch = ' ';
                break;
            }

            b.Append(ch);
          }
        }

        return b.ToString();
      }

    #endregion

    #region .pvt. impl.

      private void addPositionDetectionPatternsAndSeparators()
      {
        // 3 corner squares
        int pdpWidth = POSITION_DETECTION_PATTERN[0].Length;
        // Left Top
        addPositionDetectionPattern(0, 0);
        // Right Top
        addPositionDetectionPattern(Width - pdpWidth, 0);
        // Left Bottom
        addPositionDetectionPattern(0, Width - pdpWidth);

        // Squares horizontal separation
        // Left Top
        addHorizontalSeparationPattern(0, hspWidth - 1);
        // Right Top
        addHorizontalSeparationPattern(Width - hspWidth, hspWidth - 1);
        // Left Bottom
        addHorizontalSeparationPattern(0, Width - hspWidth);

        // Square vertical separation
        // Left Top
        addVerticalSeparationPattern(vspSize, 0);
        // Right Top
        addVerticalSeparationPattern(Height - vspSize - 1, 0);
        // Left Bottom
        addVerticalSeparationPattern(vspSize, Height - vspSize);
      }

      private void addPositionDetectionPattern( int xStart, int yStart)
      {
        for (int y = 0; y < 7; ++y)
          for (int x = 0; x < 7; ++x)
            this[xStart + x, yStart + y] = POSITION_DETECTION_PATTERN[y][x];
      }

      private void addHorizontalSeparationPattern( int xStart, int yStart)
      {
        for (int x = 0; x < 8; ++x)
        {
          if (this[xStart + x, yStart] != EMPTY_BYTE)
            throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".addHorizontalSeparationPattern(this[i]!=EMPTY_BYTE)");

          this[xStart + x, yStart] = 0;
        }
      }

      private void addVerticalSeparationPattern( int xStart, int yStart)
      {
        for (int y = 0; y < 7; ++y)
        {
          if (this[xStart, yStart + y] != EMPTY_BYTE)
            throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".addVerticalSeparationPattern(this[i]!=EMPTY_BYTE)");

          this[xStart, yStart + y] = 0;
        }
      }

      private void addDarkDotAtLeftBottomCorner()
      {
         if (this[8, Height - 8] == 0)
            throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".addDarkDotAtLeftBottomCorner(this[i]!=0)");

         this[8, Height - 8] = 1;
      }

      private void possiblyAddPositionAdjustmentPatterns( QRVersion version)
      {
        if (version.Number < 2)
          return;

        int index = version.Number - 1;
        int[] coordinates = POSITION_ADJUSTMENT_PATTERN_COORDINATE_TABLE[index];
        int numCoordinates = POSITION_ADJUSTMENT_PATTERN_COORDINATE_TABLE[index].Length;
        for (int i = 0; i < numCoordinates; ++i)
        {
          for (int j = 0; j < numCoordinates; ++j)
          {
            int y = coordinates[i];
            int x = coordinates[j];
            if (x == -1 || y == -1)
            {
              continue;
            }
            // When cell is empty position adjustment is added here
            if (this[x, y] == EMPTY_BYTE)
            {
              // move position to be in center of the pattern
              this.addPositionAdjustmentPattern(x - 2, y - 2);
            }
          }
        }
      }

      private void addPositionAdjustmentPattern( int xStart, int yStart)
      {
        for (int y = 0; y < 5; ++y)
          for (int x = 0; x < 5; ++x)
            this[xStart + x, yStart + y] = POSITION_ADJUSTMENT_PATTERN[y][x];
      }

      private void addTimingPatterns()
      {
         // avoid (7 bit) patterns of position detection;
         // also (1 bit) pair of hoz/vert separation patterns;
         // which results in 7+1=8
         for (int i = 8; i < Width - 8; ++i)
         {
            byte bit = (byte)((i + 1) % 2);
            // Hor line
            if (this[i, 6] == EMPTY_BYTE)
            {
               this[i, 6] = bit;
            }
            // Vert line
            if (this[6, i] == EMPTY_BYTE)
            {
               this[6, i] = bit;
            }
         }
      }

    #endregion

  }//class

}
