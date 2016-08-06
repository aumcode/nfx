using System;
using System.Globalization;
using System.Text;

namespace NFX.Media.PDF.Text
{
  /// <summary>
  /// Utility class for operations with text
  /// </summary>
  public static class TextAdapter
  {
    #region CONSTS

      private const char HEX_OPEN = '<';
      private const char HEX_CLOSE = '>';
      private const string HEX_STRING_PAIR = "{0:X2}{1:X2}";
      private const string FLOAT_FORMAT = "{0:0.####}";

    #endregion CONSTS

    private static Encoding s_UnicodeEncoding;

    private static Encoding s_TrivialEncoding;

    public static Encoding UnicodeEncoding
    {
      get
      {
        if (s_UnicodeEncoding == null)
          s_UnicodeEncoding = new PdfUnicodeEncoding();

        return s_UnicodeEncoding;
      }
    }

    public static Encoding TrivialEncoding
    {
      get
      {
        if (s_TrivialEncoding == null)
          s_TrivialEncoding = new PdfTrivialEncoding();

        return s_TrivialEncoding;
      }
    }

    public static string FixEscapes(string text)
    {
      text = text.Replace(@"\", @"\\");
      text = text.Replace("(", @"\(");
      text = text.Replace(")", @"\)");

      return text;
    }

    /// <summary>
    /// Converts the specified byte array into a byte array representing a unicode hex string literal.
    /// </summary>
    /// <param name="bytes">The bytes of the string.</param>
    /// <returns>The PDF bytes.</returns>
    public static byte[] FormatHexStringLiteral(byte[] bytes)
    {
      var builder = new StringBuilder();
      builder.Append(HEX_OPEN);
      for (int i = 0; i < bytes.Length; i += 2)
      {
        builder.AppendFormat(HEX_STRING_PAIR, bytes[i], bytes[i + 1]);
        if (i != 0 && (i % 48) == 0)
          builder.Append(Constants.CARRIAGE_RETURN);
      }
      builder.Append(HEX_CLOSE);
      var content = builder.ToString();

      // todo: remove when unicode support will be added
      content = content.Replace("00", "");

      return TrivialEncoding.GetBytes(content);
    }

    /// <summary>
    /// Formats given float with a dot as fraction part delimeter and foors it to 4 digits after dot
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static string FormatFloat(double number)
    {
      return string.Format(CultureInfo.InvariantCulture, FLOAT_FORMAT, number);
    }
  }
}