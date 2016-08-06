using System;
using NFX.Media.PDF.DocumentModel;
using NFX.Media.PDF.Text;

namespace NFX.Media.PDF.Styling
{
  /// <summary>
  /// PDF Color
  /// </summary>
  public class PdfColor : IPdfWritable, INamed
  {
    private const string COLOR_PDF_FORMAT = "{0} {1} {2}";

    #region STATIC

      private static readonly Registry<PdfColor> s_Colors = new Registry<PdfColor>();

      /// <summary>
      /// Returns existing color by name or throws if it is not found
      /// </summary>
      public static PdfColor ByName(string name)
      {
        if (name.IsNullOrWhiteSpace())
          throw new PdfException(StringConsts.ARGUMENT_ERROR + typeof(PdfColor).Name + ".ByName(name==null|empty)");

        var result = s_Colors[name];
        if (result == null)
          throw new PdfException(StringConsts.PDF_COLOR_DOESNOTEXIST_ERROR.Args(name));

        return result;
      }

      /// <summary>
      /// Returns existing color by name or creates and registers a new one
      /// </summary>
      public static PdfColor ByName(string name, byte r, byte g, byte b)
      {
        if (name.IsNullOrWhiteSpace())
         throw new PdfException(StringConsts.ARGUMENT_ERROR + typeof(PdfColor).Name + ".ByName(name==null|empty)");

        var result = s_Colors.GetOrRegister(name, pts => new PdfColor(name, pts.Item1, pts.Item2, pts.Item3), new Tuple<byte, byte, byte>(r, g, b));
        if (r != result.R || g != result.G || b != result.B)
         throw new PdfException(StringConsts.PDF_COLOR_INCONSISTENCY_ERROR.Args(name));

        return result;
      }

      #region Predefined

        private static readonly PdfColor s_Black = ByName("Black", 0, 0, 0);
        public static PdfColor Black
        {
          get { return s_Black; }
        }

        private static readonly PdfColor s_White = ByName("White", 255, 255, 255);
        public static PdfColor White
        {
          get { return s_White; }
        }

        private static readonly PdfColor s_Red = ByName("Red", 255, 0, 0);
        public static PdfColor Red
        {
          get { return s_Red; }
        }

        private static readonly PdfColor s_LightRed = ByName("LightRed", 255, 192, 192);
        public static PdfColor LightRed
        {
          get { return s_LightRed; }
        }

        private static readonly PdfColor s_DarkRed = ByName("DarkRed", 128, 0, 0);
        public static PdfColor DarkRed
        {
          get { return s_DarkRed; }
        }

        private static readonly PdfColor s_Orange = ByName("Orange", 255, 128, 0);
        public static PdfColor Orange
        {
          get { return s_Orange; }
        }

        private static readonly PdfColor s_LightOrange = ByName("LightOrange", 255, 192, 0);
        public static PdfColor LightOrange
        {
          get { return s_LightOrange; }
        }

        private static readonly PdfColor s_DarkOrange = ByName("DarkOrange", 255, 64, 0);
        public static PdfColor DarkOrange
        {
          get { return s_DarkOrange; }
        }

        private static readonly PdfColor s_Yellow = ByName("Yellow", 255, 255, 64);
        public static PdfColor Yellow
        {
          get { return s_Yellow; }
        }

        private static readonly PdfColor s_LightYellow = ByName("LightYellow", 255, 255, 192);
        public static PdfColor LightYellow
        {
          get { return s_LightYellow; }
        }

        private static readonly PdfColor s_DarkYellow = ByName("DarkYellow", 255, 255, 0);
        public static PdfColor DarkYellow
        {
          get { return s_DarkYellow; }
        }

        private static readonly PdfColor s_Blue = ByName("Blue", 0, 0, 255);
        public static PdfColor Blue
        {
          get { return s_Blue; }
        }

        private static readonly PdfColor s_LightBlue = ByName("LightBlue", 26, 77, 192);
        public static PdfColor LightBlue
        {
          get { return s_LightBlue; }
        }

        private static readonly PdfColor s_DarkBlue = ByName("DarkBlue", 0, 0, 128);
        public static PdfColor DarkBlue
        {
          get { return s_DarkBlue; }
        }

        private static readonly PdfColor s_Green = ByName("Green", 0, 255, 0);
        public static PdfColor Green
        {
          get { return s_Green; }
        }

        private static readonly PdfColor s_LightGreen = ByName("LightGreen", 192, 255, 192);
        public static PdfColor LightGreen
        {
          get { return s_LightGreen; }
        }

        private static readonly PdfColor s_DarkGreen = ByName("DarkGreen", 0, 128, 0);
        public static PdfColor DarkGreen
        {
          get { return s_DarkGreen; }
        }

        private static readonly PdfColor s_Cyan = ByName("Cyan", 0, 128, 255);
        public static PdfColor Cyan
        {
          get { return s_Cyan; }
        }

        private static readonly PdfColor s_LightCyan = ByName("LightCyan", 51, 205, 255);
        public static PdfColor LghtCyan
        {
          get { return s_LightCyan; }
        }

        private static readonly PdfColor s_DarkCyan = ByName("DarkCyan", 0, 102, 205);
        public static PdfColor DarkCyan
        {
          get { return s_DarkCyan; }
        }

        private static readonly PdfColor s_Purple = ByName("Purple", 128, 0, 255);
        public static PdfColor Purple
        {
          get { return s_Purple; }
        }

        private static readonly PdfColor s_LightPurple = ByName("LightPurple", 192, 115, 243);
        public static PdfColor LightPurple
        {
          get { return s_LightPurple; }
        }

        private static readonly PdfColor s_DarkPurple = ByName("DarkPurple", 102, 26, 128);
        public static PdfColor DarkPurple
        {
          get { return s_DarkPurple; }
        }

        private static readonly PdfColor s_Gray = ByName("Gray", 128, 128, 128);
        public static PdfColor Gray
        {
          get { return s_Gray; }
        }

        private static readonly PdfColor s_LightGray = ByName("LightGray", 191, 191, 191);
        public static PdfColor LghtGray
        {
          get { return s_LightGray; }
        }

        private static readonly PdfColor s_DarkGray = ByName("DarkGray", 64, 64, 64);
        public static PdfColor DarkGray
        {
          get { return s_DarkGray; }
        }

      #endregion

    #endregion

    private PdfColor(string name, byte r, byte g, byte b)
    {
      m_Name = name;
      m_R = r;
      m_G = g;
      m_B = b;
    }

    private readonly byte m_R;

    private readonly byte m_G;

    private readonly byte m_B;

    private readonly string m_Name;

    public byte R
    {
      get { return m_R; }
    }

    public byte G
    {
      get { return m_G; }
    }

    public byte B
    {
      get { return m_B; }
    }

    public string Name
    {
      get { return m_Name; }
    }

    /// <summary>
    /// Returns PDF string representation
    /// </summary>
    public string ToPdfString()
    {
      var r = TextAdapter.FormatFloat((float)R / byte.MaxValue);
      var g = TextAdapter.FormatFloat((float)G / byte.MaxValue);
      var b = TextAdapter.FormatFloat((float)B / byte.MaxValue);

      return COLOR_PDF_FORMAT.Args(r, g, b);
    }

    public override string ToString()
    {
      return ToPdfString();
    }

    public override int GetHashCode()
    {
      return m_Name.GetHashCodeOrdSenseCase() ^
             m_R.GetHashCode() ^
             m_G.GetHashCode() ^
             m_B.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      return this.Equals(obj as PdfColor);
    }

    public bool Equals(PdfColor other)
    {
      if (other == null) return false;
      return this.m_Name.EqualsOrdSenseCase(other.m_Name) &&
             this.m_R == other.m_R &&
             this.m_G == other.m_G &&
             this.m_B == other.m_B;
    }
  }
}
