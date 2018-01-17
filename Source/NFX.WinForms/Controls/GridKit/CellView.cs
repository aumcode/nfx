/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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

using NFX.WinForms.Elements;

namespace NFX.WinForms.Controls.GridKit
{
  /// <summary>
  /// Provides a viewport for grid cells
  /// </summary>
  public class CellView : ElementHostControl
  {
    
     internal CellView():base()
     {
       //for(var x=0; x<1600; x+= 48)
       //  for(var y=0; y<1800; y+= 16)
       //  {
       //  var elm =  new CheckBoxElement(this);// new TextLabelElement(this);
       //  elm.Region = new Rectangle(x,y, 46, 14);
       // // elm.Text = "Cell " + x+"x"+y;
       //  elm.Visible = true;
       //  }
     }

     /// <summary>
     /// Points to parent Grid that hosts this cell view
     /// </summary>
     public Grid Grid{ get { return (Grid)Parent; } }
  }
}
