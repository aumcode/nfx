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
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace NFX.WinForms.Views
{
  /// <summary>
  /// Internal framework helper textbox used by a FieldView control. Not intended to be used by developers
  /// </summary>
  public class InternalTextBox : TextBox
  {
  
    #region .ctor
    
        public InternalTextBox(): base()
        {

          this.BorderStyle = BorderStyle.None;
        }

        protected override void Dispose(bool disposing)
        {
          base.Dispose(disposing);
          
          disposeBitmap();
        }
    
    #endregion
    
    
    #region Private Fields
    
       private Bitmap m_Bitmap;
       private bool m_BitmapDrawn;
    
    #endregion 
    
  
  
    #region Properties
    
      /// <summary>
      /// Returns an image of the TextBox control. Image is a transparent bitmap
      /// </summary>
      public Bitmap ControlImage
      {
        get
        {
           if ((m_Bitmap==null)||(Visible)||(!m_BitmapDrawn))
           {
            disposeBitmap();
           
            using(Bitmap bmp = new Bitmap(Width, Height))
            {
              if (this.PasswordChar==0)
                this.DrawToBitmap(bmp, ClientRectangle);
              else
              {
                if (this.Text.Length>0)
                  this.drawPassword(bmp, ClientRectangle);  
              }
              
              bmp.MakeTransparent(BackColor);
              m_Bitmap = Utils.CreateGrayscaleBitmap(bmp);
            }
            m_BitmapDrawn = true;
           } 
          
           return m_Bitmap;
        }
      
      } 
       
    #endregion
    
    
    #region Public
    
      /// <summary>
      /// Deletes buffered image, thus demanding repaint of buffer
      /// </summary>
      public void InvalidateControlImage()
      {
        disposeBitmap();
      }
    
    #endregion
    
    
    
    #region Protected

      protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
      {
        base.SetBoundsCore(x, y, width, height, specified);
        disposeBitmap();
      }

      protected override void OnTextChanged(EventArgs e)
      {
        base.OnTextChanged(e);
        m_BitmapDrawn = false;
      }

      //protected override bool IsInputKey(Keys key)
      //{
      //  if (key==Keys.Tab)
      //   return true;
      //  else
      //  return base.IsInputKey(key);
      //}
      
    
    #endregion
  
  
    #region Private Utils
    
            
       private void disposeBitmap()
       {
         if (m_Bitmap != null)
            m_Bitmap.Dispose();
         m_Bitmap = null;  
         m_BitmapDrawn = false;  
       }
    
       private void drawPassword(Bitmap bmp, Rectangle rect)
       {
          using (Graphics gr = Graphics.FromImage(bmp))
          {
             gr.DrawString("* PASSWORD *", Font, Brushes.Gray, rect); 
          }
       }
    
    #endregion
  
  }
}
