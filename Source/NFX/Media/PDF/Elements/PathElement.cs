using System.Collections.Generic;
using NFX.Media.PDF.Styling;

namespace NFX.Media.PDF.Elements
{
  /// <summary>
  /// PDF path element as a collection of path primitives (lines, Bezier curves etc.)
  /// </summary>
  public class PathElement : PdfElement
  {
    public PathElement(float x, float y)
    {
      X = x;
      Y = y;
      m_Primitives = new List<PathPrimitive>();
      Style = new PdfDrawStyle();
    }

    public PathElement(float x, float y, PdfDrawStyle style)
      : this(x, y)
    {
      Style = style;
    }

    private readonly List<PathPrimitive> m_Primitives;

    public List<PathPrimitive> Primitives
    {
      get { return m_Primitives; }
    }

    public bool IsClosed { get; set; }

    public PdfDrawStyle Style { get; set; }

    public void AddLine(float endX, float endY)
    {
      var line = new LinePrimitive(endX, endY);
      m_Primitives.Add(line);
    }

    public void AddBezier(float firstControlX, float firstControlY, float secondControlX, float secondControlY, float endX, float endY)
    {
      var bezier = new BezierPrimitive(firstControlX, firstControlY, secondControlX, secondControlY, endX, endY);
      m_Primitives.Add(bezier);
    }

    public override void Write(PdfWriter writer)
    {
      writer.Write(this);
    }
  }
}