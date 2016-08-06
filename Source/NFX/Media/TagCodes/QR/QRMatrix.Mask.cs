
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
 * Revision: NFX 1.0  2013.12.31
 * Author: Denis Latushkin<dxwizard@gmail.com>
 * Based on zXing / Apache 2.0; See NOTICE and CHANGES for attribution
 */
using System;


namespace NFX.Media.TagCodes.QR
{
  public partial class QRMatrix
  {
    #region CONSTS

      // Penalty weights from section 6.8.2.1
      private const int PW1 = 3;
      private const int PW2 = 3;
      private const int PW3 = 40;
      private const int PW4 = 10;

    #endregion

	  #region Static

      // t. 21 of JISX0510:2004 (p.45).
      // Accumulate penalties.
      public int GetMaskPenalty()
      {
        int p1 = GetMaskPenaltyRule1();
        int p2 = GetMaskPenaltyRule2();
        int p3 = GetMaskPenaltyRule3();
        int p4 = GetMaskPenaltyRule4();

        return p1 + p2 + p3 + p4;
      }

      public bool GetDataMaskBit(int maskPattern, int x, int y)
      {
         int intermediate, tmp;
         switch (maskPattern)
         {
            case 0:
               intermediate = (y + x) & 0x1;
               break;
            case 1:
               intermediate = y & 0x1;
               break;
            case 2:
               intermediate = x % 3;
               break;
            case 3:
               intermediate = (y + x) % 3;
               break;
            case 4:
               intermediate = (((int)((uint)y >> 1)) + (x / 3)) & 0x1;
               break;
            case 5:
               tmp = y * x;
               intermediate = (tmp & 0x1) + (tmp % 3);
               break;
            case 6:
               tmp = y * x;
               intermediate = (((tmp & 0x1) + (tmp % 3)) & 0x1);
               break;
            case 7:
               tmp = y * x;
               intermediate = (((tmp % 3) + ((y + x) & 0x1)) & 0x1);
               break;
            default:
               throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".getDataMaskBit: Invalid mask pattern");

         }
         return intermediate == 0;
      }

      public int GetMaskPenaltyRule1()
      {
        int p0 = applyMaskPenaltyRule1Internal(true);
        int p1 = applyMaskPenaltyRule1Internal(false);

        return p0 + p1;
      }

      public int GetMaskPenaltyRule2()
      {
        int penalty = 0;
        var array = Array;
        int width = Width;
        int height = Height;
        for (int y = 0; y < height - 1; y++)
        {
          for (int x = 0; x < width - 1; x++)
          {
            int value = array[y][x];
            if (value == array[y][x + 1] && value == array[y + 1][x] && value == array[y + 1][x + 1])
            {
              penalty++;
            }
          }
        }
        return PW2 * penalty;
      }

      public int GetMaskPenaltyRule3()
      {
        int numPenalties = 0;
        byte[][] array = Array;
        int width = Width;
        int height = Height;
        for (int y = 0; y < height; y++)
        {
          for (int x = 0; x < width; x++)
          {
            byte[] arrayY = array[y]; // little optimization
            if (x + 6 < width &&
              arrayY[x] == 1 &&
              arrayY[x + 1] == 0 &&
              arrayY[x + 2] == 1 &&
              arrayY[x + 3] == 1 &&
              arrayY[x + 4] == 1 &&
              arrayY[x + 5] == 0 &&
              arrayY[x + 6] == 1 &&
              (isWhiteHorizontal(arrayY, x - 4, x) || isWhiteHorizontal(arrayY, x + 7, x + 11)))
            {
              numPenalties++;
            }
            if (y + 6 < height &&
              array[y][x] == 1 &&
              array[y + 1][x] == 0 &&
              array[y + 2][x] == 1 &&
              array[y + 3][x] == 1 &&
              array[y + 4][x] == 1 &&
              array[y + 5][x] == 0 &&
              array[y + 6][x] == 1 &&
              (isWhiteVertical(array, x, y - 4, y) || isWhiteVertical(array, x, y + 7, y + 11)))
            {
              numPenalties++;
            }
          }
        }
        return numPenalties * PW3;
      }

      public int GetMaskPenaltyRule4()
      {
        int darkCellsQty = 0;
        var array = Array;
        int width = Width;
        int height = Height;
        for (int y = 0; y < height; y++)
        {
          var arrayY = array[y];
          for (int x = 0; x < width; x++)
          {
            if (arrayY[x] == 1)
            {
              darkCellsQty++;
            }
          }
        }
        var numTotalCells = Height * Width;
        var darkRatio = (double)darkCellsQty / numTotalCells;
        var fivePercentVariances = (int)(Math.Abs(darkRatio - 0.5) * 20.0); // * 100.0 / 5.0
        return fivePercentVariances * PW4;
      }

	  #endregion

    #region .pvt. impl.

      private static bool isWhiteHorizontal(byte[] arr, int from, int to)
      {
         for (int i = from; i < to; i++)
            if (i >= 0 && i < arr.Length && arr[i] == 1)
               return false;

         return true;
      }

      private static bool isWhiteVertical(byte[][] arr, int col, int from, int to)
      {
         for (int i = from; i < to; i++)
            if (i >= 0 && i < arr.Length && arr[i][col] == 1)
               return false;

         return true;
      }

      private int applyMaskPenaltyRule1Internal(bool isHorizontal)
      {
         int penalty = 0;
         int iLimit = isHorizontal ? Height : Width;
         int jLimit = isHorizontal ? Width : Height;
         var arr = Array;
         for (int i = 0; i < iLimit; i++)
         {
            int numSameBitCells = 0;
            int prevBit = -1;
            for (int j = 0; j < jLimit; j++)
            {
               int bit = isHorizontal ? arr[i][j] : arr[j][i];
               if (bit == prevBit)
               {
                  numSameBitCells++;
               }
               else
               {
                  if (numSameBitCells >= 5)
                  {
                     penalty += PW1 + (numSameBitCells - 5);
                  }
                  numSameBitCells = 1;  // Cell itself.
                  prevBit = bit;
               }
            }
            if (numSameBitCells >= 5)
            {
               penalty += PW1 + (numSameBitCells - 5);
            }
         }
         return penalty;
      }

    #endregion

  }//class

}
