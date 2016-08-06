using System;

namespace NFX.Media.PDF.Styling
{
  /// <summary>
  /// Represents a unit of PDF coordinates. Units are relative to standard pdf point, which is 1/72 inch by default.
  /// Use ByName() accessor to get/create units, or use one of the stock units: PdfUnit.Inch|Millimeter|Point...
  /// </summary>
  public sealed class PdfUnit : INamed, IEquatable<PdfUnit>
  {
    #region CONSTS

    private const float POINTS_IN_INCH = 72;
    private const float POINTS_IN_MILLIMETER = 72 / 25.4F;
    private const float POINTS_IN_CENTIMETER = 72 / 2.54F;

    #endregion

    #region STATIC

       private static readonly Registry<PdfUnit> s_Units = new Registry<PdfUnit>();

       /// <summary>
       /// Returns existing unit by name or throws if it is not found
       /// </summary>
       public static PdfUnit ByName(string name)
       {
         if (name.IsNullOrWhiteSpace())
          throw new PdfException(StringConsts.ARGUMENT_ERROR+typeof(PdfUnit).Name+".ByName(name==null|empty)");

         var result = s_Units[name];
         if (result==null)
          throw new PdfException(StringConsts.PDF_UNIT_DOESNOTEXIST_ERROR.Args(name));

         return result;
       }

       /// <summary>
       /// Returns existing unit by name or creates and registers a new one
       /// </summary>
       public static PdfUnit ByName(string name, float points)
       {
         if (name.IsNullOrWhiteSpace())
          throw new PdfException(StringConsts.ARGUMENT_ERROR+typeof(PdfUnit).Name+".ByName(name==null|empty)");

         var result = s_Units.GetOrRegister(name, (pts) => new PdfUnit(name, pts), points);
         if (Math.Abs(result.Points - points) > double.Epsilon)
          throw new PdfException(StringConsts.PDF_UNIT_INCONSISTENCY_ERROR.Args(name));

         return result;
       }

            #region Predefined

              public static PdfUnit Default
              {
                get { return Point; }
              }

              private static readonly PdfUnit s_Point = ByName("Default", 1);
              public static PdfUnit Point
              {
                get { return s_Point; }
              }

              private static readonly PdfUnit s_Inch = ByName("Inch", POINTS_IN_INCH);
              public static PdfUnit Inch
              {
                get { return s_Inch; }
              }

              private static readonly PdfUnit s_Millimeter = ByName("Millimeter", POINTS_IN_MILLIMETER);
              public static PdfUnit Millimeter
              {
                get { return s_Millimeter; }
              }

              private static readonly PdfUnit s_Centimeter = ByName("Centimeter", POINTS_IN_CENTIMETER);
              public static PdfUnit Centimeter
              {
                get { return s_Centimeter; }
              }

            #endregion

    #endregion

    private PdfUnit(string name, float points)
    {
      m_Name = name;
      m_Points = points;
    }

    private readonly string m_Name;

    private readonly float m_Points;

    public string Name { get { return m_Name; } }

    /// <summary>
    /// Number of default user space units in current unit
    /// (1 default user space unit is 1/72 inch)
    /// </summary>
    public float Points
    {
      get { return m_Points; }
    }

    public override string ToString()
    {
      return "{0}({1} pts.)".Args(m_Name, m_Points);
    }

    public override int GetHashCode()
    {
      return m_Name.GetHashCodeOrdSenseCase() ^ m_Points.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      return this.Equals(obj as PdfUnit);
    }

    public bool Equals(PdfUnit other)
    {
      if (other == null) return false;
      return this.m_Name.EqualsOrdSenseCase(other.m_Name) &&
             this.m_Points == other.m_Points;
    }
  }
}