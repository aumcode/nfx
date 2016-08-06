using NFX.Media.PDF.Text;

namespace NFX.Media.PDF.Elements
{
  /// <summary>
  /// PDF Bezier curve primitive as a part of path
  /// </summary>
  public class BezierPrimitive : PathPrimitive
  {
    private const string BEZIER_TO_FORMAT = "{0} {1} {2} {3} {4} {5} c";

    public BezierPrimitive()
    {
    }

    public BezierPrimitive(float firstControlX, float firstControlY, float secondControlX, float secondControlY, float endX, float endY)
    {
      FirstControlX = firstControlX;
      FirstControlY = firstControlY;
      SecondControlX = secondControlX;
      SecondControlY = secondControlY;
      EndX = endX;
      EndY = endY;
    }

    /// <summary>
    /// X coordinate of Bezier's curve first control point
    /// </summary>
    public float FirstControlX { get; set; }

    /// <summary>
    /// Y coordinate of Bezier's curve first control point
    /// </summary>
    public float FirstControlY { get; set; }

    /// <summary>
    /// X coordinate of Bezier's curve second control point
    /// </summary>
    public float SecondControlX { get; set; }

    /// <summary>
    /// Y coordinate of Bezier's curve second control point
    /// </summary>
    public float SecondControlY { get; set; }

    /// <summary>
    /// X coordinate of Bezier's curve last point
    /// </summary>
    public float EndX { get; set; }

    /// <summary>
    /// Y coordinate of Bezier's curve last point
    /// </summary>
    public float EndY { get; set; }

    /// <summary>
    /// Returns PDF string representation on the line primitive
    /// </summary>
    /// <returns></returns>
    public override string ToPdfString()
    {
      return BEZIER_TO_FORMAT.Args(TextAdapter.FormatFloat(FirstControlX),
                                   TextAdapter.FormatFloat(FirstControlY),
                                   TextAdapter.FormatFloat(SecondControlX),
                                   TextAdapter.FormatFloat(SecondControlY),
                                   TextAdapter.FormatFloat(EndX),
                                   TextAdapter.FormatFloat(EndY));
    }
  }
}