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
namespace NFX.Media.PDF.Styling
{
  /// <summary>
  /// Page size definitions (ISO and others)
  /// </summary>
  public static class PdfPageSize
  {
    #region CONSTS

      private const int LETTER_WIDTH_POINTS = 612;
      private const int LETTER_HEIGHT_POINTS = 792;
      private const int A0_WIDTH_POINTS = 2380;
      private const int A0_HEIGHT_POINTS = 3368;
      private const int A1_WIDTH_POINTS = 1684;
      private const int A1_HEIGHT_POINTS = 2380;
      private const int A2_WIDTH_POINTS = 1190;
      private const int A2_HEIGHT_POINTS = 1684;
      private const int A3_WIDTH_POINTS = 842;
      private const int A3_HEIGHT_POINTS = 1190;
      private const int A4_WIDTH_POINTS = 595;
      private const int A4_HEIGHT_POINTS = 842;
      private const int A5_WIDTH_POINTS = 420;
      private const int A5_HEIGHT_POINTS = 595;
      private const int B4_WIDTH_POINTS = 729;
      private const int B4_HEIGHT_POINTS = 1032;
      private const int B5_WIDTH_POINTS = 516;
      private const int B5_HEIGHT_POINTS = 729;

    #endregion CONSTS

    #region Predefined

    public static PdfSize Default(PdfUnit unit = null)
    {
      unit = unit ?? PdfUnit.Default;
      return Letter(unit);
    }

    public static PdfSize Letter(PdfUnit unit = null)
    {
      unit = unit ?? PdfUnit.Default;
      return new PdfSize(unit, LETTER_WIDTH_POINTS / unit.Points, LETTER_HEIGHT_POINTS / unit.Points);
    }

    public static PdfSize A0(PdfUnit unit = null)
    {
      unit = unit ?? PdfUnit.Default;
      return new PdfSize(unit, A0_WIDTH_POINTS / unit.Points, A0_HEIGHT_POINTS / unit.Points);
    }

    public static PdfSize A1(PdfUnit unit = null)
    {
      unit = unit ?? PdfUnit.Default;
      return new PdfSize(unit, A1_WIDTH_POINTS / unit.Points, A1_HEIGHT_POINTS / unit.Points);
    }

    public static PdfSize A2(PdfUnit unit = null)
    {
      unit = unit ?? PdfUnit.Default;
      return new PdfSize(unit, A2_WIDTH_POINTS / unit.Points, A2_HEIGHT_POINTS / unit.Points);
    }

    public static PdfSize A3(PdfUnit unit = null)
    {
      unit = unit ?? PdfUnit.Default;
      return new PdfSize(unit, A3_WIDTH_POINTS / unit.Points, A3_HEIGHT_POINTS / unit.Points);
    }

    public static PdfSize A4(PdfUnit unit = null)
    {
      unit = unit ?? PdfUnit.Default;
      return new PdfSize(unit, A4_WIDTH_POINTS / unit.Points, A4_HEIGHT_POINTS / unit.Points);
    }

    public static PdfSize A5(PdfUnit unit = null)
    {
      unit = unit ?? PdfUnit.Default;
      return new PdfSize(unit, A5_WIDTH_POINTS / unit.Points, A5_HEIGHT_POINTS / unit.Points);
    }

    public static PdfSize B4(PdfUnit unit = null)
    {
      unit = unit ?? PdfUnit.Default;
      return new PdfSize(unit, B4_WIDTH_POINTS / unit.Points, B4_HEIGHT_POINTS / unit.Points);
    }

    public static PdfSize B5(PdfUnit unit = null)
    {
      unit = unit ?? PdfUnit.Default;
      return new PdfSize(unit, B5_WIDTH_POINTS / unit.Points, B5_HEIGHT_POINTS / unit.Points);
    }

    #endregion Predefined
  }
}