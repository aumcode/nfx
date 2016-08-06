/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
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
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

using NFX.WinForms.Elements;

namespace NFX.WinForms.Controls.GridKit
{

  /// <summary>
  /// Represents a comment hover element for grid
  /// </summary>
  public sealed class CommentElement : TextElement
  {
      #region .ctor

       internal CommentElement(CellView host) : base(host)
       {
       }

     #endregion

     #region Protected
       protected internal override void Paint(Graphics gr)
       {
          var reg = Region;
          reg.Inflate(-2,-2);

           gr.FillRectangle(Brushes.Beige, reg);

           gr.DrawLine(Pens.Gray, reg.X, reg.Y, reg.X, reg.Bottom);
           gr.DrawLine(Pens.Gray, reg.Left, reg.Y, reg.Right, reg.Y);

           using(var pen = new Pen(Color.Black))
           {
             pen.Width = 2f;
             gr.DrawLine(pen, reg.Right, reg.Y, reg.Right, reg.Bottom);
             gr.DrawLine(pen, reg.Left, reg.Bottom, reg.Right, reg.Bottom);
           }

           using (StringFormat fmt = new StringFormat())
           {
             fmt.Alignment = StringAlignment.Near;

             var treg = reg;
             treg.Inflate(-2, -2);

             gr.DrawString(Text, m_Host.Font, Brushes.Black, treg, fmt);
           }
       }

     #endregion
  }

}
