using System;

namespace NFX.Media.PDF.DocumentModel
{
  /// <summary>
  /// PDF Font
  /// </summary>
  public sealed class PdfFont : PdfObject, IPdfResource, INamed
  {
    #region CONSTS

      private const string FONT_REFERENCE_FORMAT = "/F{0}";

      private const string HELVETICA = "Helvetica";
      private const string HELVETICA_BOLD = "Helvetica-Bold";
      private const string HELVETICA_OBLIQUE = "Helvetica-Oblique";
      private const string HELVETICA_BOLDOBLIQUE = "Helvetica-BoldOblique";
      private const string COURIER = "Courier";
      private const string COURIER_BOLD = "Courier-Bold";
      private const string COURIER_OBLIQUE = "Courier-Oblique";
      private const string COURIER_BOLDOBLIQUE = "Courier-BoldOblique";
      private const string TIMES = "Times-Roman";
      private const string TIMES_BOLD = "Times-Bold";
      private const string TIMES_ITALIC = "Times-Italic";
      private const string TIMES_BOLDITALIC = "Times-BoldItalic";

    #endregion

    #region STATIC

    private static readonly Registry<PdfFont> s_Fonts = new Registry<PdfFont>();

      /// <summary>
      /// Returns existing font by name or creates and registers a new one
      /// </summary>
      public static PdfFont ByName(string name)
      {
        if (name.IsNullOrWhiteSpace())
         throw new PdfException(StringConsts.ARGUMENT_ERROR + typeof(PdfFont).Name + ".ByName(name==null|empty)");

        var result = s_Fonts.GetOrRegister(name, pts => new PdfFont(name), name);

        return result;
      }

        #region Predefined

          private static readonly PdfFont s_Helvetica = ByName(HELVETICA);
          public static PdfFont Helvetica
          {
            get { return s_Helvetica; }
          }

          private static readonly PdfFont s_HelveticaBold = ByName(HELVETICA_BOLD);
          public static PdfFont HelveticaBold
          {
            get { return s_HelveticaBold; }
          }

          private static readonly PdfFont s_HelveticaOblique = ByName(HELVETICA_OBLIQUE);
          public static PdfFont HelveticaOblique
          {
            get { return s_HelveticaOblique; }
          }

          private static readonly PdfFont s_HelveticaBoldOblique = ByName(HELVETICA_BOLDOBLIQUE);
          public static PdfFont HelveticaBoldOblique
          {
            get { return s_HelveticaBoldOblique; }
          }

          private static readonly PdfFont s_Courier = ByName(COURIER);
          public static PdfFont Courier
          {
            get { return s_Courier; }
          }

          private static readonly PdfFont s_CourierBold = ByName(COURIER_BOLD);
          public static PdfFont CourierBold
          {
            get { return s_CourierBold; }
          }

          private static readonly PdfFont s_CourierOblique = ByName(COURIER_OBLIQUE);
          public static PdfFont CourierOblique
          {
            get { return s_CourierOblique; }
          }

          private static readonly PdfFont s_CourierBoldOblique = ByName(COURIER_BOLDOBLIQUE);
          public static PdfFont CourierBoldOblique
          {
            get { return s_CourierBoldOblique; }
          }

          private static readonly PdfFont s_Times = ByName(TIMES);
          public static PdfFont Times
          {
            get { return s_Times; }
          }

          private static readonly PdfFont s_TimesBold = ByName(TIMES_BOLD);
          public static PdfFont TimesBold
          {
            get { return s_TimesBold; }
          }

          private static readonly PdfFont s_TimesItalic = ByName(TIMES_ITALIC);
          public static PdfFont TimesItalic
          {
            get { return s_TimesItalic; }
          }

          private static readonly PdfFont s_TimesBoldItalic = ByName(TIMES_BOLDITALIC);
          public static PdfFont TimesBoldItalic
          {
            get { return s_TimesBoldItalic; }
          }

        #endregion

    #endregion

    private PdfFont(string name)
    {
      m_Name = name;
    }

    private readonly string m_Name;

    /// <summary>
    /// Font name
    /// </summary>
    public string Name
    {
      get { return m_Name; }
    }

    /// <summary>
    /// Document-wide unique resource Id
    /// </summary>
    public int ResourceId { get; set; }

    /// <summary>
    /// Returns PDF object indirect reference
    /// </summary>
    public string GetResourceReference()
    {
      return FONT_REFERENCE_FORMAT.Args(ResourceId);
    }

    public override string ToString()
    {
      return m_Name;
    }

    public override int GetHashCode()
    {
      return m_Name.GetHashCodeOrdSenseCase();
    }

    public override bool Equals(object obj)
    {
      return this.Equals(obj as PdfFont);
    }

    public bool Equals(PdfFont other)
    {
      if (other == null) return false;
      return m_Name.EqualsOrdSenseCase(other.m_Name);
    }
  }
}
