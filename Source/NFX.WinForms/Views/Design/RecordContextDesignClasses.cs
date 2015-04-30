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
  /// Custom designer for RecordContextPanel and derived classes
  /// </summary>
  public class RecordContextPanelDesigner : ParentControlDesigner
  {

    public override void Initialize(IComponent component)
    {
      base.Initialize(component);
      IComponentChangeService svc = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
      if (svc != null)
      {
        svc.ComponentRemoving += doComponentRemoving;
      }
    }

    protected override void Dispose(bool disposing)
    {       
      IComponentChangeService svc = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
      if (svc != null)
      {
        svc.ComponentRemoving -= doComponentRemoving;
      }
      base.Dispose(disposing);
    }


    private void doComponentRemoving(object sender, ComponentEventArgs e)
    {
      if (e.Component is ModelBase)
      {
        RecordContextPanel ctl = Control as RecordContextPanel;
        if (ctl != null)
        {
          if (e.Component == ctl.AttachToRecord) ctl.AttachToRecord = null;
         
        }
      }
    }


    protected override void OnPaintAdornments(PaintEventArgs pe)
    {
      base.OnPaintAdornments(pe);

      if (Control != null)
      {
        RecordContextPanel ctl = Control as RecordContextPanel;
        if (ctl != null)
        {
          string str;
          if (ctl.AttachToRecord != null)
            str = "->Record(" + ctl.AttachToRecord.RecordName + ")";
          else
          {
            str = "≈>";
          }

          if (!string.IsNullOrEmpty(ctl.AttachToRecordSurrogateTypeName))
              str += " Type(\""+ctl.AttachToRecordSurrogateTypeName+"\")";
          
           
         
          Rectangle r = ctl.ClientRectangle;
          r.Inflate(-2,-2);
          
          using (Pen p = new Pen(Color.RoyalBlue, 2f)) 
          {
           p.DashStyle = DashStyle.Dot;
           pe.Graphics.DrawRectangle(p, r);
          }
          
          
          using (Font f = new Font("Tahoma", 6, FontStyle.Bold))
            using (Brush backBrush =
                    new LinearGradientBrush(new Rectangle(0, 0, 100, 14),
                       Color.FromArgb(0xff, Color.White),
                       Color.FromArgb(0x10, Color.RoyalBlue), 90))
              Utils.DrawStringWithBackground(pe.Graphics, str, f, Brushes.Navy, backBrush, 3, 3);
          
        }//context!=null
      }
                                   
      
    }

    //protected override void PreFilterProperties(System.Collections.IDictionary properties)
    //{
    //  base.PreFilterProperties(properties);
    //}


  } //designer class



}