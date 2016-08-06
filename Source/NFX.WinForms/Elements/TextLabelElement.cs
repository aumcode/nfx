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

namespace NFX.WinForms.Elements
{
  public class TextLabelElement : TextElement
  {
    #region .ctor

    public TextLabelElement(ElementHostControl host)
      : base(host)
    {
    }

    #endregion


    #region Private Fields
      private StringAlignment m_Alignment = StringAlignment.Near;
      private bool m_IsHyperlink;
    #endregion


    #region Properties

     public StringAlignment Alignment
     {
      get { return m_Alignment;}
      set
      {
        m_Alignment = value;
        Invalidate();
      }
     }

     /// <summary>
     /// Indicates whether this label element is a hyperlink and should raise OnHyperlink event
     /// </summary>
     public bool IsHyperlink
     {
       get { return m_IsHyperlink; }
       set
       {
         m_IsHyperlink = value;
       }
     }

    #endregion


    #region Events
      public EventHandler Hyperlink;
    #endregion

    #region Public


    #endregion


    #region Protected

      protected internal override void Paint(System.Drawing.Graphics gr)
      {
        base.Paint(gr);

        //BaseApplication.Theme.PartRenderer.LabelText(gr,
        //                          Region,
        //                          m_IsHyperlink ? MouseIsOver : false,
        //                          Host.Font,
        //                          FieldControlContext,
        //                          Text,
        //                          m_Alignment);
      }

      protected internal override void OnMouseEnter(EventArgs e)
      {
        base.OnMouseEnter(e);

        if (m_IsHyperlink)
          Invalidate();
      }

      protected internal override void OnMouseLeave(EventArgs e)
      {
        base.OnMouseLeave(e);

        if (m_IsHyperlink)
          Invalidate();
      }

      protected internal override void OnMouseClick(System.Windows.Forms.MouseEventArgs e)
      {
        base.OnMouseClick(e);

        if (m_IsHyperlink)
             OnHyperlink(EventArgs.Empty);
      }

      protected virtual void OnHyperlink(EventArgs e)
      {
        if (Hyperlink!=null)
                 Hyperlink(this, e);
      }


    #endregion


    #region Private Utils


    #endregion

  }

}
