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
using System;
using System.Collections.Generic;
using NFX.Media.PDF.Elements;
using NFX.Media.PDF.Styling;

namespace NFX.Media.PDF.DocumentModel
{
  /// <summary>
  /// PDF Page
  /// </summary>
  public class PdfPage : PdfObject
  {
    internal PdfPage(PdfPageTree parent, PdfSize size)
    {
      m_Size = size;
      m_Unit = m_Size.Unit;
      m_Parent = parent;
      m_Fonts = new List<PdfFont>();
      m_Elements = new List<PdfElement>();
    }

    private readonly PdfSize m_Size;

    private readonly PdfUnit m_Unit;

    private readonly List<PdfElement> m_Elements;

    private readonly PdfPageTree m_Parent;

    private readonly List<PdfFont> m_Fonts;

    #region Properties

    /// <summary>
    /// Page's height
    /// </summary>
    public float Height
    {
      get { return m_Size.Height; }
    }

    /// <summary>
    /// Page's width
    /// </summary>
    public float Width
    {
      get { return m_Size.Width; }
    }

    /// <summary>
    /// Page elements
    /// </summary>
    public List<PdfElement> Elements
    {
      get { return m_Elements; }
    }

    /// <summary>
    /// User space units
    /// (the default user space unit is 1/72 inch)
    /// </summary>
    public float UserUnit
    {
      get { return m_Unit.Points; }
    }

    /// <summary>
    /// Page tree
    /// </summary>
    internal PdfPageTree Parent
    {
      get { return m_Parent; }
    }

    /// <summary>
    /// Fonts used on the page
    /// </summary>
    internal List<PdfFont> Fonts
    {
      get { return m_Fonts; }
    }

    #endregion Properties

    /// <summary>
    /// Adds PDF element to page's elements collection
    /// </summary>
    public void Add(PdfElement element)
    {
      if (m_Elements.Contains(element))
        throw new InvalidOperationException("The element has already been added");

      m_Elements.Add(element);
    }

    #region Add text

    /// <summary>
    /// Add raw text to the page
    /// </summary>
    public TextElement AddText(string text)
    {
      return this.AddText(text, Constants.DEFAULT_FONT_SIZE, PdfFont.Courier);
    }

    /// <summary>
    /// Add raw text to the page
    /// </summary>
    public TextElement AddText(string text, float fontSize, PdfFont font)
    {
      return this.AddText(text, fontSize, font, PdfColor.Black);
    }

    /// <summary>
    /// Add raw text to the page
    /// </summary>
    public TextElement AddText(string text, float fontSize, PdfFont font, PdfColor foreground)
    {
      var element = new TextElement(text, fontSize, font, foreground);
      Add(element);

      return element;
    }

    #endregion Add text

    #region Add path

    /// <summary>
    /// Add path to the page
    /// </summary>
    public PathElement AddPath(float x, float y)
    {
      return AddPath(x, y, new PdfDrawStyle());
    }

    /// <summary>
    /// Add path to the page
    /// </summary>
    public PathElement AddPath(float x, float y, float thickness)
    {
      return AddPath(x, y, new PdfDrawStyle(thickness));
    }

    /// <summary>
    /// Add path to the page
    /// </summary>
    public PathElement AddPath(float x, float y, float thickness, PdfColor color)
    {
      return AddPath(x, y, new PdfDrawStyle(thickness, color));
    }

    /// <summary>
    /// Add path to the page
    /// </summary>
    public PathElement AddPath(float x, float y, float thickness, PdfColor color, PdfLineType type)
    {
      return AddPath(x, y, new PdfDrawStyle(thickness, color, type));
    }

    /// <summary>
    /// Add path to the page
    /// </summary>
    public PathElement AddPath(float x, float y, PdfDrawStyle style)
    {
      var path = new PathElement(x, y, style);
      Add(path);

      return path;
    }

    #endregion Add path

    #region Add line

    /// <summary>
    /// Add line primitive to the page
    /// </summary>
    public PathElement AddLine(float x1, float y1, float x2, float y2)
    {
      return AddLine(x1, y1, x2, y2, new PdfDrawStyle());
    }

    /// <summary>
    /// Add line primitive to the page
    /// </summary>
    public PathElement AddLine(float x1, float y1, float x2, float y2, float thickness)
    {
      return AddLine(x1, y1, x2, y2, new PdfDrawStyle(thickness));
    }

    /// <summary>
    /// Add line primitive to the page
    /// </summary>
    public PathElement AddLine(float x1, float y1, float x2, float y2, float thickness, PdfColor color)
    {
      return AddLine(x1, y1, x2, y2, new PdfDrawStyle(thickness, color));
    }

    /// <summary>
    /// Add line primitive to the page
    /// </summary>
    public PathElement AddLine(float x1, float y1, float x2, float y2, float thickness, PdfColor color, PdfLineType type)
    {
      return AddLine(x1, y1, x2, y2, new PdfDrawStyle(thickness, color, type));
    }

    /// <summary>
    /// Add line primitive to the page
    /// </summary>
    public PathElement AddLine(float x1, float y1, float x2, float y2, PdfDrawStyle style)
    {
      var path = new PathElement(x1, y1, style);
      path.AddLine(x2, y2);
      Add(path);

      return path;
    }

    #endregion Add line

    #region Add circle

    /// <summary>
    /// Add circle primitive to the page
    /// </summary>
    public PathElement AddCircle(float centerX, float centerY, float r, PdfColor fill)
    {
      return AddCircle(centerX, centerY, r, new PdfDrawStyle(fill));
    }

    /// <summary>
    /// Add circle primitive to the page
    /// </summary>
    public PathElement AddCircle(float centerX, float centerY, float r, PdfColor fill, float borderThickness)
    {
      return AddCircle(centerX, centerY, r, new PdfDrawStyle(borderThickness) { FillColor = fill });
    }

    /// <summary>
    /// Add circle primitive to the page
    /// </summary>
    public PathElement AddCircle(float centerX, float centerY, float r, PdfColor fill, float borderThickness, PdfColor borderColor)
    {
      return AddCircle(centerX, centerY, r, new PdfDrawStyle(borderThickness, borderColor) { FillColor = fill });
    }

    /// <summary>
    /// Add circle primitive to the page
    /// </summary>
    public PathElement AddCircle(float centerX, float centerY, float r, PdfColor fill, float borderThickness, PdfColor borderColor, PdfLineType borderType)
    {
      return AddCircle(centerX, centerY, r, new PdfDrawStyle(borderThickness, borderColor, borderType) { FillColor = fill });
    }

    /// <summary>
    /// Add circle primitive to the page
    /// </summary>
    public PathElement AddCircle(float centerX, float centerY, float r, PdfDrawStyle borderStyle)
    {
      var path = new PathElement(centerX - r, centerY, borderStyle);
      path.IsClosed = true;
      path.AddBezier(centerX - r, centerY + Constants.SQRT_TWO * r, centerX + r, centerY + Constants.SQRT_TWO * r, centerX + r, centerY);
      path.AddBezier(centerX + r, centerY - Constants.SQRT_TWO * r, centerX - r, centerY - Constants.SQRT_TWO * r, centerX - r, centerY);
      Add(path);

      return path;
    }

    #endregion Add circle

    #region Add rectangle

    /// <summary>
    /// Add rectangle primitive to the page
    /// </summary>
    public RectangleElement AddRectangle(float x1, float y1, float x2, float y2, PdfColor fill)
    {
      return AddRectangle(x1, y1, x2, y2, new PdfDrawStyle(fill));
    }

    /// <summary>
    /// Add rectangle primitive to the page
    /// </summary>
    public RectangleElement AddRectangle(float x1, float y1, float x2, float y2, PdfColor fill, float borderThickness)
    {
      return AddRectangle(x1, y1, x2, y2, new PdfDrawStyle(borderThickness) { FillColor = fill });
    }

    /// <summary>
    /// Add rectangle primitive to the page
    /// </summary>
    public RectangleElement AddRectangle(float x1, float y1, float x2, float y2, PdfColor fill, float borderThickness, PdfColor borderColor)
    {
      return AddRectangle(x1, y1, x2, y2, new PdfDrawStyle(borderThickness, borderColor) { FillColor = fill });
    }

    /// <summary>
    /// Add rectangle primitive to the page
    /// </summary>
    public RectangleElement AddRectangle(float x1, float y1, float x2, float y2, PdfColor fill, float borderThickness, PdfColor borderColor, PdfLineType borderType)
    {
      return AddRectangle(x1, y1, x2, y2, new PdfDrawStyle(borderThickness, borderColor, borderType) { FillColor = fill });
    }

    /// <summary>
    /// Add rectangle primitive to the page
    /// </summary>
    public RectangleElement AddRectangle(float x1, float y1, float x2, float y2, PdfDrawStyle style)
    {
      var rectangle = new RectangleElement(x1, y1, x2, y2, style);
      Add(rectangle);

      return rectangle;
    }

    #endregion Add rectangle

    #region Add image

    /// <summary>
    /// Add image to the page
    /// </summary>
    public ImageElement AddImage(string filePath)
    {
      var image = new ImageElement(filePath);
      Add(image);

      return image;
    }

    /// <summary>
    /// Add image to the page
    /// </summary>
    public ImageElement AddImage(string filePath, float width, float height)
    {
      var image = new ImageElement(filePath, width, height);
      Add(image);

      return image;
    }

    #endregion Add image
  }
}