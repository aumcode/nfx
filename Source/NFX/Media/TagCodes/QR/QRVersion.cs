
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
using System.Linq;


namespace NFX.Media.TagCodes.QR
{
  public class QRVersion
  {
    #region CONSTS

      private const int VERSION_QTY = 40;

    #endregion

    #region Inner Types

      public sealed class CorrectionBlock
      {
        #region .ctor

          public CorrectionBlock(int qty, int codewords)
          {
            Qty = qty;
            Codewords = codewords;
          }

        #endregion

        #region Properties

          public readonly int Qty;
          public readonly int Codewords;

        #endregion
      }

      public sealed class CorrectionBlockSet
      {
        #region .ctor

          public CorrectionBlockSet(int codewordsPerBlock, params CorrectionBlock[] blocks)
          {
            CodewordsPerBlock = codewordsPerBlock;
            Blocks = blocks;

            TotalQty = blocks.Sum(b => b.Qty);
            TotalCodewords = codewordsPerBlock * TotalQty;
          }

        #endregion

        #region Properties

         public readonly int CodewordsPerBlock;
         public readonly CorrectionBlock[] Blocks;

         public int TotalQty;
         public int TotalCodewords;

        #endregion

        #region .pvt. impl.

        #endregion
      }

    #endregion

    #region Static

      public static QRVersion GetVersionByNumber(int number)
      {
        if (number < 1 || number > VERSION_QTY)
          throw new NFXException(StringConsts.ARGUMENT_ERROR + typeof(QRVersion).Name + ".GetVersionByNumber(number: any supported version)");

        return Versions[number-1];
      }

      private static QRVersion[] generateVersionSet()
      {
        QRVersion[] versions = new QRVersion[VERSION_QTY];

        int i = 0;

        versions[i] = new QRVersion(++i, new int[] {},
          new CorrectionBlockSet(7, new CorrectionBlock(1, 19)),
          new CorrectionBlockSet(10, new CorrectionBlock(1, 16)),
          new CorrectionBlockSet(13, new CorrectionBlock(1, 13)),
          new CorrectionBlockSet(17, new CorrectionBlock(1, 9)));

        versions[i] = new QRVersion(++i, new int[]{6, 18},
          new CorrectionBlockSet(10, new CorrectionBlock(1, 34)),
          new CorrectionBlockSet(16, new CorrectionBlock(1, 28)),
          new CorrectionBlockSet(22, new CorrectionBlock(1, 22)),
          new CorrectionBlockSet(28, new CorrectionBlock(1, 16)));

        versions[i] = new QRVersion(++i, new int[]{6, 22},
          new CorrectionBlockSet(15, new CorrectionBlock(1, 55)),
          new CorrectionBlockSet(26, new CorrectionBlock(1, 44)),
          new CorrectionBlockSet(18, new CorrectionBlock(2, 17)),
          new CorrectionBlockSet(22, new CorrectionBlock(2, 13)));

        versions[i] = new QRVersion(++i, new int[]{6, 26},
          new CorrectionBlockSet(20, new CorrectionBlock(1, 80)),
          new CorrectionBlockSet(18, new CorrectionBlock(2, 32)),
          new CorrectionBlockSet(26, new CorrectionBlock(2, 24)),
          new CorrectionBlockSet(16, new CorrectionBlock(4, 9)));

        versions[i] = new QRVersion(++i, new int[]{6, 30},
          new CorrectionBlockSet(26, new CorrectionBlock(1, 108)),
          new CorrectionBlockSet(24, new CorrectionBlock(2, 43)),
          new CorrectionBlockSet(18, new CorrectionBlock(2, 15), new CorrectionBlock(2, 16)),
          new CorrectionBlockSet(22, new CorrectionBlock(2, 11), new CorrectionBlock(2, 12)));

        versions[i] = new QRVersion(++i, new int[]{6, 34},
          new CorrectionBlockSet(18, new CorrectionBlock(2, 68)),
          new CorrectionBlockSet(16, new CorrectionBlock(4, 27)),
          new CorrectionBlockSet(24, new CorrectionBlock(4, 19)),
          new CorrectionBlockSet(28, new CorrectionBlock(4, 15)));

        versions[i] = new QRVersion(++i, new int[]{6, 22, 38},
          new CorrectionBlockSet(20, new CorrectionBlock(2, 78)),
          new CorrectionBlockSet(18, new CorrectionBlock(4, 31)),
          new CorrectionBlockSet(18, new CorrectionBlock(2, 14), new CorrectionBlock(4, 15)),
          new CorrectionBlockSet(26, new CorrectionBlock(4, 13), new CorrectionBlock(1, 14)));

        versions[i] = new QRVersion(++i, new int[]{6, 24, 42},
          new CorrectionBlockSet(24, new CorrectionBlock(2, 97)),
          new CorrectionBlockSet(22, new CorrectionBlock(2, 38), new CorrectionBlock(2, 39)),
          new CorrectionBlockSet(22, new CorrectionBlock(4, 18), new CorrectionBlock(2, 19)),
          new CorrectionBlockSet(26, new CorrectionBlock(4, 14), new CorrectionBlock(2, 15)));

        versions[i] = new QRVersion(++i, new int[]{6, 26, 46},
          new CorrectionBlockSet(30, new CorrectionBlock(2, 116)),
          new CorrectionBlockSet(22, new CorrectionBlock(3, 36), new CorrectionBlock(2, 37)),
          new CorrectionBlockSet(20, new CorrectionBlock(4, 16), new CorrectionBlock(4, 17)),
          new CorrectionBlockSet(24, new CorrectionBlock(4, 12), new CorrectionBlock(4, 13)));

        versions[i] = new QRVersion(++i, new int[]{6, 28, 50},
          new CorrectionBlockSet(18, new CorrectionBlock(2, 68), new CorrectionBlock(2, 69)),
          new CorrectionBlockSet(26, new CorrectionBlock(4, 43), new CorrectionBlock(1, 44)),
          new CorrectionBlockSet(24, new CorrectionBlock(6, 19), new CorrectionBlock(2, 20)),
          new CorrectionBlockSet(28, new CorrectionBlock(6, 15), new CorrectionBlock(2, 16)));

        versions[i] = new QRVersion(++i, new int[]{6, 30, 54},
          new CorrectionBlockSet(20, new CorrectionBlock(4, 81)),
          new CorrectionBlockSet(30, new CorrectionBlock(1, 50), new CorrectionBlock(4, 51)),
          new CorrectionBlockSet(28, new CorrectionBlock(4, 22), new CorrectionBlock(4, 23)),
          new CorrectionBlockSet(24, new CorrectionBlock(3, 12), new CorrectionBlock(8, 13)));

        versions[i] = new QRVersion(++i, new int[]{6, 32, 58},
          new CorrectionBlockSet(24, new CorrectionBlock(2, 92), new CorrectionBlock(2, 93)),
          new CorrectionBlockSet(22, new CorrectionBlock(6, 36), new CorrectionBlock(2, 37)),
          new CorrectionBlockSet(26, new CorrectionBlock(4, 20), new CorrectionBlock(6, 21)),
          new CorrectionBlockSet(28, new CorrectionBlock(7, 14), new CorrectionBlock(4, 15)));

        versions[i] = new QRVersion(++i, new int[]{6, 34, 62},
          new CorrectionBlockSet(26, new CorrectionBlock(4, 107)),
          new CorrectionBlockSet(22, new CorrectionBlock(8, 37), new CorrectionBlock(1, 38)),
          new CorrectionBlockSet(24, new CorrectionBlock(8, 20), new CorrectionBlock(4, 21)),
          new CorrectionBlockSet(22, new CorrectionBlock(12, 11), new CorrectionBlock(4, 12)));

        versions[i] = new QRVersion(++i, new int[]{6, 26, 46, 66},
          new CorrectionBlockSet(30, new CorrectionBlock(3, 115), new CorrectionBlock(1, 116)),
          new CorrectionBlockSet(24, new CorrectionBlock(4, 40), new CorrectionBlock(5, 41)),
          new CorrectionBlockSet(20, new CorrectionBlock(11, 16), new CorrectionBlock(5, 17)),
          new CorrectionBlockSet(24, new CorrectionBlock(11, 12), new CorrectionBlock(5, 13)));

        versions[i] = new QRVersion(++i, new int[]{6, 26, 48, 70},
          new CorrectionBlockSet(22, new CorrectionBlock(5, 87), new CorrectionBlock(1, 88)),
          new CorrectionBlockSet(24, new CorrectionBlock(5, 41), new CorrectionBlock(5, 42)),
          new CorrectionBlockSet(30, new CorrectionBlock(5, 24), new CorrectionBlock(7, 25)),
          new CorrectionBlockSet(24, new CorrectionBlock(11, 12), new CorrectionBlock(7, 13)));

        versions[i] = new QRVersion(++i, new int[]{6, 26, 50, 74},
          new CorrectionBlockSet(24, new CorrectionBlock(5, 98), new CorrectionBlock(1, 99)),
          new CorrectionBlockSet(28, new CorrectionBlock(7, 45), new CorrectionBlock(3, 46)),
          new CorrectionBlockSet(24, new CorrectionBlock(15, 19), new CorrectionBlock(2, 20)),
          new CorrectionBlockSet(30, new CorrectionBlock(3, 15), new CorrectionBlock(13, 16)));

        versions[i] = new QRVersion(++i, new int[]{6, 30, 54, 78},
          new CorrectionBlockSet(28, new CorrectionBlock(1, 107), new CorrectionBlock(5, 108)),
          new CorrectionBlockSet(28, new CorrectionBlock(10, 46), new CorrectionBlock(1, 47)),
          new CorrectionBlockSet(28, new CorrectionBlock(1, 22), new CorrectionBlock(15, 23)),
          new CorrectionBlockSet(28, new CorrectionBlock(2, 14), new CorrectionBlock(17, 15)));

        versions[i] = new QRVersion(++i, new int[]{6, 30, 56, 82},
          new CorrectionBlockSet(30, new CorrectionBlock(5, 120), new CorrectionBlock(1, 121)),
          new CorrectionBlockSet(26, new CorrectionBlock(9, 43), new CorrectionBlock(4, 44)),
          new CorrectionBlockSet(28, new CorrectionBlock(17, 22), new CorrectionBlock(1, 23)),
          new CorrectionBlockSet(28, new CorrectionBlock(2, 14), new CorrectionBlock(19, 15)));

        versions[i] = new QRVersion(++i, new int[]{6, 30, 58, 86},
          new CorrectionBlockSet(28, new CorrectionBlock(3, 113), new CorrectionBlock(4, 114)),
          new CorrectionBlockSet(26, new CorrectionBlock(3, 44), new CorrectionBlock(11, 45)),
          new CorrectionBlockSet(26, new CorrectionBlock(17, 21), new CorrectionBlock(4, 22)),
          new CorrectionBlockSet(26, new CorrectionBlock(9, 13), new CorrectionBlock(16, 14)));

        versions[i] = new QRVersion(++i, new int[]{6, 34, 62, 90},
          new CorrectionBlockSet(28, new CorrectionBlock(3, 107), new CorrectionBlock(5, 108)),
          new CorrectionBlockSet(26, new CorrectionBlock(3, 41), new CorrectionBlock(13, 42)),
          new CorrectionBlockSet(30, new CorrectionBlock(15, 24), new CorrectionBlock(5, 25)),
          new CorrectionBlockSet(28, new CorrectionBlock(15, 15), new CorrectionBlock(10, 16)));

        versions[i] = new QRVersion(++i, new int[]{6, 28, 50, 72, 94},
          new CorrectionBlockSet(28, new CorrectionBlock(4, 116), new CorrectionBlock(4, 117)),
          new CorrectionBlockSet(26, new CorrectionBlock(17, 42)),
          new CorrectionBlockSet(28, new CorrectionBlock(17, 22), new CorrectionBlock(6, 23)),
          new CorrectionBlockSet(30, new CorrectionBlock(19, 16), new CorrectionBlock(6, 17)));

        versions[i] = new QRVersion(++i, new int[]{6, 26, 50, 74, 98},
          new CorrectionBlockSet(28, new CorrectionBlock(2, 111), new CorrectionBlock(7, 112)),
          new CorrectionBlockSet(28, new CorrectionBlock(17, 46)),
          new CorrectionBlockSet(30, new CorrectionBlock(7, 24), new CorrectionBlock(16, 25)),
          new CorrectionBlockSet(24, new CorrectionBlock(34, 13)));

        versions[i] = new QRVersion(++i, new int[]{6, 30, 54, 74, 102},
          new CorrectionBlockSet(30, new CorrectionBlock(4, 121), new CorrectionBlock(5, 122)),
          new CorrectionBlockSet(28, new CorrectionBlock(4, 47), new CorrectionBlock(14, 48)),
          new CorrectionBlockSet(30, new CorrectionBlock(11, 24), new CorrectionBlock(14, 25)),
          new CorrectionBlockSet(30, new CorrectionBlock(16, 15), new CorrectionBlock(14, 16)));

        versions[i] = new QRVersion(++i, new int[]{6, 28, 54, 80, 106},
          new CorrectionBlockSet(30, new CorrectionBlock(6, 117), new CorrectionBlock(4, 118)),
          new CorrectionBlockSet(28, new CorrectionBlock(6, 45), new CorrectionBlock(14, 46)),
          new CorrectionBlockSet(30, new CorrectionBlock(11, 24), new CorrectionBlock(16, 25)),
          new CorrectionBlockSet(30, new CorrectionBlock(30, 16), new CorrectionBlock(2, 17)));

        versions[i] = new QRVersion(++i, new int[]{6, 32, 58, 84, 110},
          new CorrectionBlockSet(26, new CorrectionBlock(8, 106), new CorrectionBlock(4, 107)),
          new CorrectionBlockSet(28, new CorrectionBlock(8, 47), new CorrectionBlock(13, 48)),
          new CorrectionBlockSet(30, new CorrectionBlock(7, 24), new CorrectionBlock(22, 25)),
          new CorrectionBlockSet(30, new CorrectionBlock(22, 15), new CorrectionBlock(13, 16)));

        versions[i] = new QRVersion(++i, new int[]{6, 30, 58, 86, 114},
          new CorrectionBlockSet(28, new CorrectionBlock(10, 114), new CorrectionBlock(2, 115)),
          new CorrectionBlockSet(28, new CorrectionBlock(19, 46), new CorrectionBlock(4, 47)),
          new CorrectionBlockSet(28, new CorrectionBlock(28, 22), new CorrectionBlock(6, 23)),
          new CorrectionBlockSet(30, new CorrectionBlock(33, 16), new CorrectionBlock(4, 17)));

        versions[i] = new QRVersion(++i, new int[]{6, 34, 62, 90, 118},
          new CorrectionBlockSet(30, new CorrectionBlock(8, 122), new CorrectionBlock(4, 123)),
          new CorrectionBlockSet(28, new CorrectionBlock(22, 45), new CorrectionBlock(3, 46)),
          new CorrectionBlockSet(30, new CorrectionBlock(8, 23), new CorrectionBlock(26, 24)),
          new CorrectionBlockSet(30, new CorrectionBlock(12, 15), new CorrectionBlock(28, 16)));

        versions[i] = new QRVersion(++i, new int[]{6, 26, 50, 74, 98, 122},
          new CorrectionBlockSet(30, new CorrectionBlock(3, 117), new CorrectionBlock(10, 118)),
          new CorrectionBlockSet(28, new CorrectionBlock(3, 45), new CorrectionBlock(23, 46)),
          new CorrectionBlockSet(30, new CorrectionBlock(4, 24), new CorrectionBlock(31, 25)),
          new CorrectionBlockSet(30, new CorrectionBlock(11, 15), new CorrectionBlock(31, 16)));

        versions[i] = new QRVersion(++i, new int[]{6, 30, 54, 78, 102, 126},
          new CorrectionBlockSet(30, new CorrectionBlock(7, 116), new CorrectionBlock(7, 117)),
          new CorrectionBlockSet(28, new CorrectionBlock(21, 45), new CorrectionBlock(7, 46)),
          new CorrectionBlockSet(30, new CorrectionBlock(1, 23), new CorrectionBlock(37, 24)),
          new CorrectionBlockSet(30, new CorrectionBlock(19, 15), new CorrectionBlock(26, 16)));

        versions[i] = new QRVersion(++i, new int[]{6, 26, 52, 78, 104, 130},
          new CorrectionBlockSet(30, new CorrectionBlock(5, 115), new CorrectionBlock(10, 116)),
          new CorrectionBlockSet(28, new CorrectionBlock(19, 47), new CorrectionBlock(10, 48)),
          new CorrectionBlockSet(30, new CorrectionBlock(15, 24), new CorrectionBlock(25, 25)),
          new CorrectionBlockSet(30, new CorrectionBlock(23, 15), new CorrectionBlock(25, 16)));

        versions[i] = new QRVersion(++i, new int[]{6, 30, 56, 82, 108, 134},
          new CorrectionBlockSet(30, new CorrectionBlock(13, 115), new CorrectionBlock(3, 116)),
          new CorrectionBlockSet(28, new CorrectionBlock(2, 46), new CorrectionBlock(29, 47)),
          new CorrectionBlockSet(30, new CorrectionBlock(42, 24), new CorrectionBlock(1, 25)),
          new CorrectionBlockSet(30, new CorrectionBlock(23, 15), new CorrectionBlock(28, 16)));

        versions[i] = new QRVersion(++i, new int[]{6, 34, 60, 86, 112, 138},
          new CorrectionBlockSet(30, new CorrectionBlock(17, 115)),
          new CorrectionBlockSet(28, new CorrectionBlock(10, 46), new CorrectionBlock(23, 47)),
          new CorrectionBlockSet(30, new CorrectionBlock(10, 24), new CorrectionBlock(35, 25)),
          new CorrectionBlockSet(30, new CorrectionBlock(19, 15), new CorrectionBlock(35, 16)));

        versions[i] = new QRVersion(++i, new int[]{6, 30, 58, 86, 114, 142},
          new CorrectionBlockSet(30, new CorrectionBlock(17, 115), new CorrectionBlock(1, 116)),
          new CorrectionBlockSet(28, new CorrectionBlock(14, 46), new CorrectionBlock(21, 47)),
          new CorrectionBlockSet(30, new CorrectionBlock(29, 24), new CorrectionBlock(19, 25)),
          new CorrectionBlockSet(30, new CorrectionBlock(11, 15), new CorrectionBlock(46, 16)));

        versions[i] = new QRVersion(++i, new int[]{6, 34, 62, 90, 118, 146},
          new CorrectionBlockSet(30, new CorrectionBlock(13, 115), new CorrectionBlock(6, 116)),
          new CorrectionBlockSet(28, new CorrectionBlock(14, 46), new CorrectionBlock(23, 47)),
          new CorrectionBlockSet(30, new CorrectionBlock(44, 24), new CorrectionBlock(7, 25)),
          new CorrectionBlockSet(30, new CorrectionBlock(59, 16), new CorrectionBlock(1, 17)));

        versions[i] = new QRVersion(++i, new int[]{6, 30, 54, 78, 102, 126, 150},
          new CorrectionBlockSet(30, new CorrectionBlock(12, 121), new CorrectionBlock(7, 122)),
          new CorrectionBlockSet(28, new CorrectionBlock(12, 47), new CorrectionBlock(26, 48)),
          new CorrectionBlockSet(30, new CorrectionBlock(39, 24), new CorrectionBlock(14, 25)),
          new CorrectionBlockSet(30, new CorrectionBlock(22, 15), new CorrectionBlock(41, 16)));

        versions[i] = new QRVersion(++i, new int[]{6, 24, 50, 76, 102, 128, 154},
          new CorrectionBlockSet(30, new CorrectionBlock(6, 121), new CorrectionBlock(14, 122)),
          new CorrectionBlockSet(28, new CorrectionBlock(6, 47), new CorrectionBlock(34, 48)),
          new CorrectionBlockSet(30, new CorrectionBlock(46, 24), new CorrectionBlock(10, 25)),
          new CorrectionBlockSet(30, new CorrectionBlock(2, 15), new CorrectionBlock(64, 16)));

        versions[i] = new QRVersion(++i, new int[]{6, 28, 54, 80, 106, 132, 158},
          new CorrectionBlockSet(30, new CorrectionBlock(17, 122), new CorrectionBlock(4, 123)),
          new CorrectionBlockSet(28, new CorrectionBlock(29, 46), new CorrectionBlock(14, 47)),
          new CorrectionBlockSet(30, new CorrectionBlock(49, 24), new CorrectionBlock(10, 25)),
          new CorrectionBlockSet(30, new CorrectionBlock(24, 15), new CorrectionBlock(46, 16)));

        versions[i] = new QRVersion(++i, new int[]{6, 32, 58, 84, 110, 136, 162},
          new CorrectionBlockSet(30, new CorrectionBlock(4, 122), new CorrectionBlock(18, 123)),
          new CorrectionBlockSet(28, new CorrectionBlock(13, 46), new CorrectionBlock(32, 47)),
          new CorrectionBlockSet(30, new CorrectionBlock(48, 24), new CorrectionBlock(14, 25)),
          new CorrectionBlockSet(30, new CorrectionBlock(42, 15), new CorrectionBlock(32, 16)));

        versions[i] = new QRVersion(++i, new int[]{6, 26, 54, 82, 110, 138, 166},
          new CorrectionBlockSet(30, new CorrectionBlock(20, 117), new CorrectionBlock(4, 118)),
          new CorrectionBlockSet(28, new CorrectionBlock(40, 47), new CorrectionBlock(7, 48)),
          new CorrectionBlockSet(30, new CorrectionBlock(43, 24), new CorrectionBlock(22, 25)),
          new CorrectionBlockSet(30, new CorrectionBlock(10, 15), new CorrectionBlock(67, 16)));

        versions[i] = new QRVersion(++i, new int[]{6, 30, 58, 86, 114, 142, 170},
          new CorrectionBlockSet(30, new CorrectionBlock(19, 118), new CorrectionBlock(6, 119)),
          new CorrectionBlockSet(28, new CorrectionBlock(18, 47), new CorrectionBlock(31, 48)),
          new CorrectionBlockSet(30, new CorrectionBlock(34, 24), new CorrectionBlock(34, 25)),
          new CorrectionBlockSet(30, new CorrectionBlock(20, 15), new CorrectionBlock(61, 16)));

        return versions;
      }

    #endregion

    #region .ctor

    public QRVersion(int number, int[] alignmentCenters, params CorrectionBlockSet[] correctionBlockSets)
    {
      Number = number;
      AlignmentPatternCenters = alignmentCenters;
      this.m_CorrectionBlockSets = correctionBlockSets;

      int total = 0;
      int correctionCodewords = correctionBlockSets[0].CodewordsPerBlock;
      CorrectionBlock[] correctionBlocks = m_CorrectionBlockSets[0].Blocks;
      foreach (CorrectionBlock correctionBlock in correctionBlocks)
        total = correctionBlock.Qty * (correctionBlock.Codewords + correctionCodewords);
      TotalCodewords = total;

      Dimension = 17 + 4 * number;
    }

    #endregion

    #region Pvt/Prot/Int Fields

      private readonly CorrectionBlockSet[] m_CorrectionBlockSets;


      private static QRVersion[] Versions = generateVersionSet();

    #endregion

    #region Properties

      public readonly int Number;
      public readonly int[] AlignmentPatternCenters;
      public readonly int Dimension;
      public readonly int TotalCodewords;

    #endregion

    #region Public

      public CorrectionBlockSet GetBlockSetByLevel(QRCorrectionLevel level)
      {
        return m_CorrectionBlockSets[level.Ordinal];
      }

    #endregion

    #region Protected

      public override string ToString()
      {
        return Number.ToString();
      }

    #endregion

    #region .pvt. impl.

    #endregion

  }//class

}
