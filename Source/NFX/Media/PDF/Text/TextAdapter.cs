/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 Dmitriy Khmaladze, IT Adapter Inc / 2015-2016 Aum Code LLC
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
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