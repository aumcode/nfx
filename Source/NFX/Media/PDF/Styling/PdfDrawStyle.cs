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
using System.Text;
using NFX.Media.PDF.DocumentModel;
using NFX.Media.PDF.Text;

namespace NFX.Media.PDF.Styling
{
  /// <summary>
  /// PDF line's style
  /// </summary>
  public class PdfDrawStyle : IPdfWritable
  {
    public PdfDrawStyle(PdfColor fillColor)
    {
      FillColor = fillColor;
    }

    public PdfDrawStyle()
      : this(Constants.DEFAULT_LINE_THICKNESS, PdfColor.Black, PdfLineType.Normal)
    {
    }

    public PdfDrawStyle(float strokeThickness)
      : this(strokeThickness, PdfColor.Black, PdfLineType.Normal)
    {
    }

    public PdfDrawStyle(float strokeThickness, PdfColor strokeColor)
      : this(strokeThickness, strokeColor, PdfLineType.Normal)
    {
    }

    public PdfDrawStyle(float strokeThickness, PdfColor strokeColor, PdfLineType strokeType)
    {
      StrokeThickness = strokeThickness;
      StrokeColor = strokeColor;
      StrokeType = strokeType;
    }

    public float? StrokeThickness { get; set; }

    public PdfColor StrokeColor { get; set; }

    public PdfColor FillColor { get; set; }

    public PdfLineType? StrokeType { get; set; }

    /// <summary>
    /// Returns PDF string representation
    /// </summary>
    public string ToPdfString()
    {
      var styleBuilder = new StringBuilder();

      if (StrokeColor != null)
        styleBuilder.AppendFormatLine("{0} RG", StrokeColor.ToPdfString());

      if (FillColor != null)
        styleBuilder.AppendFormatLine("{0} rg", FillColor.ToPdfString());

      if (StrokeThickness != null)
        styleBuilder.AppendFormatLine("{0} w", TextAdapter.FormatFloat(StrokeThickness.Value));

      if (StrokeType != null)
      {
        switch (StrokeType)
        {
          case PdfLineType.OutlinedThin:
            styleBuilder.AppendLine("[2 2] 0 d");
            break;
          case PdfLineType.Outlined:
            styleBuilder.AppendLine("[4 4] 0 d");
            break;
          case PdfLineType.OutlinedBold:
            styleBuilder.AppendLine("[6 6] 0 d");
            break;
        }
      }

      return styleBuilder.ToString();
    }
  }
}