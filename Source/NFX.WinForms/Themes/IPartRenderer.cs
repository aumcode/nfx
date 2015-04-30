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
using System.Windows.Forms;

namespace NFX.WinForms.Themes
{
 
  /// <summary>
  /// Interfaces a class that renders parts - elements that controls are rendered from
  /// </summary>
  public interface IPartRenderer
  {
     
     void RadioButton(Graphics gr, 
                      Rectangle rect,
                      bool isHot,
                      IFieldControlContext context,
                      bool isChecked);
                      
     void RadioGroup(Graphics gr,
                      Rectangle rect,
                      bool isHot,
                      IFieldControlContext context);   
                      


     /// <summary>
     /// Returns metrics of check box adornments/padding - decorations drawn around real check box
     /// </summary>
     Padding CheckBoxMetrics { get; }
     
     void CheckBox(Graphics gr,
                      Rectangle rect,
                      bool isHot,
                      IFieldControlContext context,
                      bool isChecked);

     void Text(Graphics gr,
                      Rectangle rect,
                      bool isHot,
                      Font font,
                      Brush brush,
                      IFieldControlContext context,
                      string text,
                      StringAlignment align);
     
     
     void LabelText(Graphics gr,
                      Rectangle rect,
                      bool isHot,
                      Font font,
                      IFieldControlContext context,
                      string text,
                      StringAlignment align);

     
     /// <summary>
     /// Returns metrics of text box adornments - artificial niceties drawn around real text box
     /// </summary>
     Padding TextBoxMetrics { get;}

     /// <summary>
     /// Returns metrics of focused text box adornments - artificial niceties drawn around real text box
     /// </summary>
     Padding FocusedTextBoxMetrics { get; }
     
     
     
     Color TextBoxBackgroundColor { get;}
     
     /// <summary>
     /// Draws text box control. Due to rendering complexity of text boxes (as compared to labels), rendering is performed
     ///  by the same class that editing is performed in (usually TextBox-derivative) so here text is rendered from a pre-rendered image.
     /// Parameter "textImage" may be null in which case no text is drawn
     /// </summary>
     void TextBox(Graphics gr,
                      Rectangle rect,
                      bool isHot,
                      IFieldControlContext context,
                      Bitmap textImage);


     /// <summary>
     /// Returns padding for combo button placement inside text box
     /// </summary>
     Padding ComboButtonPadding { get; }
     
     /// <summary>
     /// Returns the width that text box shall take. 1.0 = 100% = whole control width
     /// </summary>
     double ComboTextBoxWidthPercent { get; }
     
     /// <summary>
     /// Returns the size that combo button shall take. 1.0 = 100% = all minimum control dimension
     /// </summary>
     double ComboButtonSizePercent { get; }

     

     void ComboButton(Graphics gr,
                      Rectangle rect,
                      bool isHot,
                      bool isPressed,
                      IFieldControlContext context);

     /// <summary>
     /// Draws background decorations on combo lookup form
     /// </summary>
     void ComboBoxLookupForm(Form form, Graphics gr);



     /// <summary>
     /// Draws a callout balloon with specified coordinates
     /// </summary>
     void Balloon(Graphics gr,
                  Rectangle rect,
                  Point target,
                  Color color,
                  IFieldControlContext context);

  }
  
  
}
