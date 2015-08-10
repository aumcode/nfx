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