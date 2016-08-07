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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Drawing;

using NFX.WinForms;

namespace NFX.WinForms.Elements
{
  /// <summary>
  /// Represents an elemnt that may be pressed down by the mouse. Mostly used as a button base
  /// </summary>
  public abstract class PressableElement : Element
  {
    #region .ctor

    public PressableElement(ElementHostControl host)
      : base(host)
    {
    }

    #endregion


    #region Private Fields
       private bool m_Pressed;
    #endregion


    #region Properties
      /// <summary>
      /// Indicates whether an element is pressed
      /// </summary>
      public bool Pressed
      {
        get { return m_Pressed; }
      }
    #endregion


    #region Events


    #endregion


    #region Public

    #endregion


    #region Protected

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
      if (!m_Pressed)
      {
        m_Pressed = true;
        Invalidate();
      }
      base.OnMouseDown(e);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
      if (m_Pressed)
      {
        m_Pressed = false;
        Invalidate();
      }
      base.OnMouseUp(e);
    }


    protected internal override void OnMouseEnter(EventArgs e)
    {
      base.OnMouseEnter(e);
      Invalidate();
    }


    protected internal override void OnMouseLeave(EventArgs e)
    {
      m_Pressed = false;
      base.OnMouseLeave(e);
      Invalidate();
    }


    #endregion


    #region Private Utils



    #endregion

  }

}
