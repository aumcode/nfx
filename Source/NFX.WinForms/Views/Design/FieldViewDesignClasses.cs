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

using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using NFX.RecordModel;

using NFX.WinForms.Elements;


namespace NFX.WinForms.Views.Design
{

  /// <summary>
  /// Custom designer for FieldView control and derived classes
  /// </summary>
  public class FieldViewDesigner : FieldContextControlDesigner
  {

    public override SelectionRules SelectionRules
    {
      get
      {
        return base.SelectionRules ^ SelectionRules.TopSizeable ^ SelectionRules.BottomSizeable;
      }
    }



    protected override void OnPaintAdornments(PaintEventArgs pe)
    {
      base.OnPaintAdornments(pe);
      
      FieldView ctl = Control as FieldView;
      
      if (ctl!=null)
      {
              
          Rectangle r = ctl.ClientRectangle;
          r.Inflate(-1, -1);

          using (Pen p = new Pen(Color.FromArgb(190, 255,170, 40), 2f))
          {
            p.DashStyle = DashStyle.Dot;
            pe.Graphics.DrawRectangle(p, r);
            
            pe.Graphics.DrawLine(p, ctl.Width /2, 0, ctl.Width/2, ctl.Height);


            if ((ctl.CaptionPlacement==CaptionPlacement.Left) || (ctl.CaptionPlacement==CaptionPlacement.Right))
            {
              pe.Graphics.DrawLine(p, ctl.CaptionHIndent, 0, ctl.CaptionHIndent, ctl.Height);
              pe.Graphics.DrawLine(p, ctl.Width-ctl.CaptionHIndent, 0, ctl.Width-ctl.CaptionHIndent, ctl.Height);
            }
          }
      
      }
      
    }

   

  } //designer class






}