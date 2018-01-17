/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
