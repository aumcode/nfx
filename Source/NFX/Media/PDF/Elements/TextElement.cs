using NFX.Media.PDF.DocumentModel;
using NFX.Media.PDF.Styling;

namespace NFX.Media.PDF.Elements
{
  /// <summary>
  /// PDF text element
  /// </summary>
  public class TextElement : PdfElement
  {
    #region .ctor

    public TextElement(string content)
      : this(content, Constants.DEFAULT_FONT_SIZE, PdfFont.Courier, PdfColor.Black)
    {
    }

    public TextElement(string content, float fontSize, PdfFont font)
      : this(content, fontSize, font, PdfColor.Black)
    {
    }

    public TextElement(string content, float fontSize, PdfFont font, PdfColor color)
    {
      Content = content;
      FontSize = fontSize;
      Font = font;
      Color = color;
    }

    #endregion .ctor

    /// <summary>
    /// Text content
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Font size
    /// </summary>
    public float FontSize { get; set; }

    /// <summary>
    /// PDF Font
    /// </summary>
    public PdfFont Font { get; set; }

    /// <summary>
    /// PDF Color
    /// </summary>
    public PdfColor Color { get; set; }

    /// <summary>
    /// Writes element into file stream
    /// </summary>
    /// <param name="writer">PDF writer</param>
    /// <returns>Written bytes count</returns>
    public override void Write(PdfWriter writer)
    {
      writer.Write(this);
    }
  }
}
