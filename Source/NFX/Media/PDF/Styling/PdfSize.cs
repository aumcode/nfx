using System;

namespace NFX.Media.PDF.Styling
{
  /// <summary>
  /// PDF size with its units
  /// </summary>
  public sealed class PdfSize
  {
    public PdfSize(PdfUnit unit, float width, float height)
    {
      if (unit == null)
        throw new PdfException(StringConsts.ARGUMENT_ERROR+GetType().Name+".ctor(unit==null)");

      m_Unit = unit;
      Width = width;
      Height = height;
    }

    private readonly PdfUnit m_Unit;

    /// <summary>
    /// Pdf unit measured in default points (1 pt = 1/72 inch)
    /// </summary>
    public PdfUnit Unit
    {
      get { return m_Unit; }
    }

    /// <summary>
    /// Height measured in values calculated according to Unit property
    /// (by default: 1 pt = 1/72 inch)
    /// </summary>
    public float Height { get; set; }

    /// <summary>
    /// Width measured in values calculated according to Unit property
    /// (by default: 1 pt = 1/72 inch)
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// Change unit of the size and recalculate height and width
    /// </summary>
    /// <param name="unit">New unit</param>
    public PdfSize ChangeUnits(PdfUnit unit)
    {
      var ratio = Unit.Points / unit.Points;
      var result = new PdfSize(unit, Height * ratio, Width * ratio);

      return result;
    }

    public override bool Equals(object obj)
    {
      return this.Equals(obj as PdfSize);
    }

    public override int GetHashCode()
    {
      var result = this.Width.GetHashCode() ^ this.Height.GetHashCode();
      if (m_Unit != null)
        result ^= m_Unit.GetHashCode();

      return result;
    }

    public override string ToString()
    {
      return "height: {0} width: {1} units: {2}".Args(Height, Width, Unit);
    }

    public bool Equals(PdfSize other)
    {
      if (other == null) return false;
      return Math.Abs(this.Height - other.Height) < double.Epsilon &&
             Math.Abs(this.Width - other.Width) < double.Epsilon &&
             this.Unit == null ? (other.Unit == null) : this.Unit.Equals(other.Unit);
    }
  }
}