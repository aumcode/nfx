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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NFX.RecordModel;

namespace NFX.WinForms.Views
{
  /// <summary>
  /// Defines a panel that auto populates with RecordModel's field views
  /// </summary>
  public class GeneratedRecordView : RecordContextPanel
  {
    
    public GeneratedRecordView():base()
    {  
      AutoScroll = true;
      BackColor = Color.FromArgb(0xf0, 0xf0, 0xe5);
      //panel1.AutoScrollMinSize = new Size (400, 400)
      m_ToolTip = new ToolTip();
      m_ToolTip.AutoPopDelay = 3000;
      m_ToolTip.InitialDelay = 1000;
      m_ToolTip.ReshowDelay  = 500;
      m_ToolTip.ShowAlways   = true;
      m_ToolTip.Active       = true;
    }

    private ToolTip m_ToolTip;

    public new Record Record
    {
      get { return base.Record; }
      set
      {
         base.AttachToRecord = value;
         buildFields();
      }
    }

    public bool Save(bool focusError = true)
    {
      var views = Controls.Cast<Control>().Where(ctl=>ctl is FieldView).Cast<FieldView>();
   //   foreach(var v in views.Where(v=>v.Field.IsGUIMutable))
   //    v.CommitValue();
     
      if (Record.InvalidValidatedFields.FirstOrDefault()!=null) throw new Exception();

      Record.Validate();

      if (!Record.Valid)
      {
        if (focusError)
        { 
          var errorfld = Record.InvalidValidatedFields.FirstOrDefault();
          if (errorfld==null) return false;
          var view = views.Where(v=>v.AttachToField==errorfld).FirstOrDefault();
          if (view!=null) 
          view.Focus();
        }
        return false;
      }
      return true;
    }
   
    protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
    {
      if (CanFocus) Focus(); //so scrll work with wheel
      base.OnMouseDown(e);
    }


    private void buildFields()
    {
      const int hpad = 10;
    
      var x = 10;
      var y = 10;
    
      foreach(var fld in Record.Fields)
      {
        var view = new FieldView(); 
        view.CaptionPlacement = CaptionPlacement.Left;
        view.CaptionHIndent = 170;
        view.BeginInit();
        
        view.AttachToField = fld;
        this.Controls.Add(view);
       
        var width = view.CaptionHIndent + fld.DisplayWidth * 10;
        if (fld.LookupDictionary.Count>0)
        {
          view.ControlType = ControlType.Radio;
          view.LineCount = fld.LookupDictionary.Count;
          view.ElementVSpacing = 6;
        }

        view.CharCase = (CharacterCasing)Enum.Parse(typeof(CharacterCasing), fld.CharCase.ToString());
        
        if (fld is BoolField)
         view.ControlType = ControlType.Check;
       
        view.SetBounds(x, y, width, 0);
       
        view.EndInit();
       
        //m_ToolTip.SetToolTip(view, fld.Hint);
        view.Enter += (sender, args) => { m_ToolTip.Show(fld.Hint, this, MousePosition); };

        y+=view.Height+hpad;
      }
    }//buildFields
  }
}