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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NFX.WinForms.Controls
{

  /// <summary>
  /// A ComboBox that allows to specify the color of the control.
  /// Note that the ForeColor of the parent ComboBox only paints the drop-down list
  /// but not the face color of the ComboBox itself.
  /// </summary>
  public class ComboBoxEx : ComboBox
  {

    public ComboBoxEx()
    {
      base.DrawMode = DrawMode.OwnerDrawFixed;
    }


    private Color m_HighlightColor = Color.Gray;


    new public DrawMode DrawMode 
    { 
      get { return DrawMode.OwnerDrawFixed; }
      set {} 
    }
      
    [Browsable(true)]
    public Color HighlightColor 
    { 
      get { return m_HighlightColor;} 
      set
      {
        m_HighlightColor = value;
        Refresh();
      } 
    }


    protected override void OnDrawItem(DrawItemEventArgs e)
    {
      if (e.Index < 0) return;

      e.Graphics.FillRectangle(
          (e.State & DrawItemState.Selected) == DrawItemState.Selected
              ? new SolidBrush(HighlightColor)
              : new SolidBrush(this.BackColor),
          e.Bounds);

      e.Graphics.DrawString(Items[e.Index].ToString(), e.Font,
                            new SolidBrush(ForeColor),
                            new Point(e.Bounds.X, e.Bounds.Y));

      e.DrawFocusRectangle();

      base.OnDrawItem(e);
    }

  }
}
