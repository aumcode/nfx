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
using System.Windows.Forms;

//using NFX.WinApi;


namespace NFX.WinForms.Views
{
  /// <summary>
  /// Represents internal popup form displayed when cmbo-box drop-down is activated
  /// </summary>
  internal class ComboBoxLookupForm : Form
  {
      #region .ctor
       
        public ComboBoxLookupForm() : base()
        {
           FormBorderStyle = FormBorderStyle.None;
           ShowInTaskbar = false;
           BackColor = Color.White;
           KeyPreview = true;
           StartPosition = FormStartPosition.Manual;
           Size = new Size(400, 256);
           
        }
      
      #endregion
  
  
      #region Private Fields
        
      
      #endregion
  
  
      #region Public
      
        public bool ShowDropdown(FieldView view)
        {
          
          //find location
          //assumption: form size (width and height) already set and they can not be greate than 50% of screen size
          Point scrPoint = view.Parent.PointToScreen(view.comboLookupFormAnchorPoint());
                
          Rectangle scrRect = Screen.GetWorkingArea(scrPoint);
          
          //vertical
            int h = view.comboLookupFormElementHeight();
            if ((scrPoint.Y+h+ Height)>scrRect.Bottom) 
              scrPoint.Y = scrPoint.Y - 1 - Height;
            else
              scrPoint.Y = scrPoint.Y + h + 1;

           //horizontal
            if ((scrPoint.X + Width) > scrRect.Right)
              scrPoint.X = scrPoint.X - ((scrPoint.X + Width) - scrRect.Right);
            if (scrPoint.X < scrRect.Left)
              scrPoint.X = scrRect.Left;  
              
           
           Location = scrPoint;



           using(Timer tmr = new Timer())
           {
             tmr.Interval = 10;//ms
             tmr.Tick += tmrTick;
             try
             {
              tmr.Enabled = true;
              ShowDialog();
             }
             finally
             {
               tmr.Enabled = false;
               tmr.Tick -= tmrTick; //timer bug fix
             } 
           }
           
          return false;
        }
  
  
      #endregion
  
  
  
  
      #region Protected

        
        protected override void OnMouseDown(MouseEventArgs e)
        {
          base.OnMouseDown(e);
          
          if (!ClientRectangle.Contains(e.Location))
                 Close();
        }
        
     
       
        
        protected override void OnPaint(PaintEventArgs e)
        {
          base.OnPaint(e);
          BaseApplication.Theme.PartRenderer.ComboBoxLookupForm(this, e.Graphics);
        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
          base.OnKeyDown(e);
          
          if (e.KeyCode == Keys.Escape) 
            Close();
        }
         
      
      #endregion
      
      
      #region Private Utils
         private void tmrTick(object sender, EventArgs e)
         {
           Point pos = Cursor.Position;
           
           if (!ClientRectangle.Contains(PointToClient(pos)))
             Capture = true;
           else
             Capture = false;  
         }
      
      #endregion
      
  }
  
  
}
