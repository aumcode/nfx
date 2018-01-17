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
using NFX.Media.PDF.Styling;

namespace NFX.Media.PDF.Elements
{
  public class RectangleElement : PdfElement
  {
    #region .ctor

    public RectangleElement(float x1, float y1, float x2, float y2)
      : this(x1, y1, x2, y2, new PdfDrawStyle())
    {
    }

    public RectangleElement(float x1, float y1, float x2, float y2, PdfDrawStyle borderStyle)
    {
      X = x1;
      Y = y1;
      X1 = x2;
      Y1 = y2;
      Style = borderStyle;
    }

    #endregion .ctor

    public float X1 { get; set; }

    public float Y1 { get; set; }

    public PdfDrawStyle Style { get; set; }

    public override void Write(PdfWriter writer)
    {
      writer.Write(this);
    }
  }
}