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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Drawing;

using NFX.WinForms.Views;

namespace NFX.WinForms.Elements
{
  /// <summary>
  /// Represents a combo box element
  /// </summary>
  public class ComboBoxElement : TextElement
  {
    #region .ctor

      public ComboBoxElement(ElementHostControl host,InternalTextBox textBox)
        : base(host)
      {
        m_InternalTextBox = textBox;
        m_Button = new ComboButtonElement(host);
        m_Button.ZOrder = 2;
        
        m_Button.MouseClick += delegate(object sender, MouseEventArgs e)
                               {
                                 OnButtonClick(EventArgs.Empty);
                               };  
        
      }

      protected override void Destructor()
      {
        m_Button.Dispose();
        base.Destructor();
      }

    #endregion


    #region Private Fields
      private InternalTextBox m_InternalTextBox;
      private ComboButtonElement m_Button;
      
    #endregion


    #region Properties

      public InternalTextBox InternalTextBox
      {
        get { return m_InternalTextBox; }
      }
    
      public Rectangle ButtonRegion
      {
        get { return m_Button.Region; }
      }
   

      /// <summary>
      /// Provides access to drop-down button inner element
      /// </summary>
      public ComboButtonElement Button
      {
        get { return m_Button; }
      }


    #endregion


    #region Events
      public EventHandler ButtonClick;
   
    #endregion


    #region Public

   

    #endregion


    #region Protected


      protected override void FieldControlContextChanged()
      {
        m_Button.FieldControlContext = this.FieldControlContext;
      }
    
    

    protected internal override void Paint(System.Drawing.Graphics gr)
    {
      base.Paint(gr);


      int txtWidth =
       m_Button.Visible ? (int)(Region.Width * BaseApplication.Theme.PartRenderer.ComboTextBoxWidthPercent) : Region.Width; 
      
      Rectangle rgn = new Rectangle(Region.Left, Region.Top, txtWidth, Region.Height);

      BaseApplication.Theme.PartRenderer.TextBox(gr,
                                                rgn,
                                                MouseIsOver,
                                                FieldControlContext,
                                                m_InternalTextBox.ControlImage);
    }


    protected override void ZOrderChanged()
    { 
      base.ZOrderChanged();
      m_Button.ZOrder = ZOrder + 1;
    }

    protected override void RegionChanged()
    {
      base.RegionChanged();

      Themes.IPartRenderer renderer = BaseApplication.Theme.PartRenderer;
      
      int minDim = (Region.Width<Region.Height) ? Region.Width : Region.Height;
      
      int btnWidth = (int)(minDim * renderer.ComboButtonSizePercent);
      Padding pd = renderer.ComboButtonPadding;
      
      m_Button.Region = new Rectangle(Region.Right - btnWidth - pd.Horizontal,
                                      Region.Top + pd.Top,
                                      btnWidth, 
                                      Region.Height - pd.Vertical);
    }

   
   
    protected virtual void OnButtonClick(EventArgs e)
    {
      if (ButtonClick!=null)
           ButtonClick(this, e);
    }
   

    #endregion


    #region Private Utils

    

    #endregion

  }

}
