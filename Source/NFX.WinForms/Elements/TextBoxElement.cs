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
using NFX.WinForms.Views;
using NFX.WinForms.Themes;

namespace NFX.WinForms.Elements
{
  public class TextBoxElement : TextElement
  {
    #region .ctor

    public TextBoxElement(ElementHostControl host,InternalTextBox textBox)
      : base(host)
    {
      m_InternalTextBox = textBox;
    }

    #endregion


    #region Private Fields
      private InternalTextBox m_InternalTextBox;
    #endregion


    #region Properties

    public InternalTextBox InternalTextBox
    {
      get { return m_InternalTextBox; }
    }

    #endregion


    #region Events

    #endregion

    #region Public


    #endregion


    #region Protected

    protected internal override void Paint(System.Drawing.Graphics gr)
    {
      base.Paint(gr);
      BaseApplication.Theme.PartRenderer.TextBox(gr, 
                                                Region, 
                                                MouseIsOver,
                                                FieldControlContext,
                                                m_InternalTextBox.ControlImage);
    }

    #endregion


    #region Private Utils


    #endregion

  }

}
