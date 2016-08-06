using NFX.Media.PDF.Styling;

namespace NFX.Media.PDF.Elements
{
  public class RectangleElement : PdfElement
  {
    #region .ctor

    public RectangleElement(float x1, float y1, float x2, float y2)
      : this(x1, y1, x2, y2, new PdfDrawStyle())
    {
    }

    public RectangleElement(float x1, float y1, float x2, float y2, PdfDrawStyle borderStyle)
    {
      X = x1;
      Y = y1;
      X1 = x2;
      Y1 = y2;
      Style = borderStyle;
    }

    #endregion .ctor

    public float X1 { get; set; }

    public float Y1 { get; set; }

    public PdfDrawStyle Style { get; set; }

    public override void Write(PdfWriter writer)
    {
      writer.Write(this);
    }
  }
}