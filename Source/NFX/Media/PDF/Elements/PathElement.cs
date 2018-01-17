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