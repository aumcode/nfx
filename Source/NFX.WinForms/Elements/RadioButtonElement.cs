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

using System.Windows.Forms;
using System.Drawing;

namespace NFX.WinForms.Elements
{
  public class RadioButtonElement : CheckableElement
  {
    #region .ctor

          public RadioButtonElement(ElementHostControl host) : base(host)
          {
          }

    #endregion


    #region Private Fields

      private object m_Key;

    #endregion


    #region Properties

      public object Key
      {
        get { return m_Key; }
        set { m_Key = value;}
      }

    #endregion


    #region Public


    #endregion


    #region Protected

      protected internal override void Paint(Graphics gr)
      {
        //BaseApplication.Theme.PartRenderer.RadioButton(gr, Region, MouseIsOver, FieldControlContext, Checked);
      }

    #endregion


    #region Private Utils


    #endregion

  }

}
