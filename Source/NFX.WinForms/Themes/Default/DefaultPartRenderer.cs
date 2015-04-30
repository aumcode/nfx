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

using NFX.Geometry;

namespace NFX.WinForms.Themes.Default
{
  
  public class DefaultPartRenderer : DisposableObject, IPartRenderer
  {

    protected override void Destructor()
    {
      base.Destructor();
    }


    private const int MIN_RB_SIZE = 12;
    private const float SHADOW_ARC_START = -45f;
    private const float SHADOW_ARC_SWEEP = 188f;
    private const float BRUSH_GRADIENT_ANGLE = 90f;

    #region IPartRenderer Members

    public void RadioButton(Graphics gr,
                            Rectangle rect,
                            bool isHot,
                            IFieldControlContext context,
                            bool isChecked)
    {
      if (rect.Width < MIN_RB_SIZE) rect.Width=MIN_RB_SIZE;
      if (rect.Height < MIN_RB_SIZE) rect.Height=MIN_RB_SIZE;
      
      gr.SmoothingMode = SmoothingMode.HighQuality;

      bool focused, enabled, applicable;
      
      if (context != null)
      {
        focused = context.Focused;
        enabled = context.IsEnabled;
        applicable = context.IsApplicable;
      }
      else
      {
        focused = false;
        enabled = true;
        applicable = true;
      }
            
      Color grStart = (focused)?  Color.FromArgb(0xff, 0xf0, 0x60) : Color.FromArgb(0xb0, 0xb0, 0xff);
      
      if (!enabled) grStart = Color.Snow;
       else 
         if (!applicable) grStart = Color.Silver;
            
      //Draw radio button base
      using (LinearGradientBrush gb = new LinearGradientBrush(rect,
                                                              grStart, 
                                                              Color.White,
                                                              BRUSH_GRADIENT_ANGLE))
      {
        gr.FillEllipse(gb, rect);
      }

      //outer rim
      gr.DrawEllipse(Pens.SlateBlue, rect); 
      
      if (enabled && applicable)
      {
        // outer rim black shadow
        gr.DrawArc(Pens.Black, rect, SHADOW_ARC_START, SHADOW_ARC_SWEEP);
        rect.Offset(1,1);
        gr.DrawArc(Pens.SlateBlue, rect, SHADOW_ARC_START, SHADOW_ARC_SWEEP);
        rect.Offset(-1, -1);
      }
      
      if (isChecked)   //draw radio button circle
      {
        int deltaX = -(int)(rect.Width * 0.25);
        int deltaY = -(int)(rect.Height * 0.25);
              
        rect.Inflate(deltaX, deltaY);
        using (LinearGradientBrush gb = new LinearGradientBrush(rect, Color.Navy, Color.Gray, BRUSH_GRADIENT_ANGLE))
        {
          gr.FillEllipse(gb, rect);
        }
      }//is checked
      
    }



    public void RadioGroup(Graphics gr,
                            Rectangle rect,
                            bool isHot,
                            IFieldControlContext context)
    {
      bool focused, enabled, applicable;
      bool error;
      bool modified;

      if (context != null)
      {
        focused = context.Focused;
        error = context.Validated && !context.Valid;
        modified = context.GUIModified;
        enabled = context.IsEnabled;
        applicable = context.IsApplicable;
      }
      else
      {
        focused = false;
        error = false;
        modified = false;
        enabled = false;
        applicable = true;
      }

      if (focused)
        System.Windows.Forms.ControlPaint.DrawFocusRectangle(gr, rect);
        

      if (enabled)
      {
        if (error)
          Utils.DrawUnderlineRectangle(gr, rect, Color.Red);
        else
          if (modified)
            Utils.DrawUnderlineRectangle(gr, rect, Color.FromArgb(128, Color.Green));
      }   
        
    }
    

    public Padding CheckBoxMetrics
    {
      get
      {
        return new Padding(2, 2, 2, 2);
      }
    }

    public void CheckBox(Graphics gr,
                            Rectangle rect,
                            bool isHot,
                            IFieldControlContext context,
                            bool isChecked)
    {
      if (rect.Width < MIN_RB_SIZE) rect.Width = MIN_RB_SIZE;
      if (rect.Height < MIN_RB_SIZE) rect.Height = MIN_RB_SIZE;

      bool focused, enabled, applicable;
      bool error;
      bool modified;
      
      if (context!=null)
      {
         focused = context.Focused;
         enabled = context.IsEnabled;
         applicable = context.IsApplicable;
         error = context.Validated && !context.Valid;
         modified = context.GUIModified;
      }
      else
      {
         focused = false;
         enabled = true;
         applicable = true;
         error = false;
         modified = false;
      }

      Color grStart = (focused)? Color.FromArgb(0xff, 0xf0, 0x20) : Color.FromArgb(0xb0, 0xb0, 0xff);

      if (!enabled) grStart = Color.Snow;
      else
        if (!applicable) grStart = Color.Silver; 
       
      //Draw check button base
      using (LinearGradientBrush br = new LinearGradientBrush(rect,
                                                              Color.White, 
                                                               grStart, 
                                                               BRUSH_GRADIENT_ANGLE))
      {
        gr.FillRectangle(br, rect);
      }
      
      
      
      gr.DrawRectangle(Pens.SlateBlue, pinnedInflate(rect, -1, -1));

      if (focused)
      {
        Rectangle fr = rect;
        fr.Inflate(1,1);
        fr = pinnedInflate(fr, 1,1);
        System.Windows.Forms.ControlPaint.DrawFocusRectangle(gr, fr);
      }   
      
      if (enabled && applicable)
      {
        //bottom shadow
        gr.DrawLine(Pens.Navy, rect.Left + 1, rect.Bottom, rect.Right , rect.Bottom);

        //Right shadow
        gr.DrawLine(Pens.Navy, rect.Right, rect.Bottom, rect.Right, rect.Top+1);
      }


      if (isChecked)   //draw checkmark
      {
        using (Pen p = new Pen(Color.FromArgb(180, 0,0,255), 2f))
        {
          gr.DrawLine(p,
             (int)(rect.Left + rect.Width * 0.25), (int)(rect.Top + rect.Height * 0.25),
             (int)(rect.Right - rect.Width * 0.25), (int)(rect.Bottom - rect.Height * 0.25)
            );
          gr.DrawLine(p,
             (int)(rect.Right - rect.Width * 0.18), (int)(rect.Top + rect.Height * 0.18),
             (int)(rect.Left + rect.Width * 0.18), (int)(rect.Bottom - rect.Height * 0.18)
            );

        }
      }//is checked

      if (enabled)
      {
      
        rect.Inflate(2,2);
        if (error)
          Utils.DrawUnderlineRectangle(gr, rect, Color.Red);
        else
          if (modified)
            Utils.DrawUnderlineRectangle(gr, rect, Color.FromArgb(128, Color.Green));
      }

    }




    public void Text(Graphics gr, 
                     Rectangle rect,
                     bool isHot,
                     Font font,
                     Brush brush,
                     IFieldControlContext context, 
                     string text, 
                     StringAlignment align)
    {
      using (StringFormat fmt = new StringFormat())
      {
       fmt.Alignment = align;
       gr.DrawString(text, font, brush, rect, fmt);
      } 
    }

    public void LabelText(Graphics gr,
                     Rectangle rect,
                     bool isHot,
                     Font font,
                     IFieldControlContext context,
                     string text,
                     StringAlignment align)
    {
      Font fnt = isHot ? new Font(font.FontFamily, font.Size, font.Style | FontStyle.Underline): font;
      
      bool enabled, marked;
      Color markerColor;
      float markerIntensity;

      if (context!=null)
      {
       enabled = context.IsEnabled && context.IsApplicable;
       marked = context.Marked;
       markerColor = context.MarkerColor;
       markerIntensity = context.MarkerIntensity;
      }
      else
      {
       enabled = true; 
       marked = false;
       markerColor = Color.Yellow;
       markerIntensity = 0.85f;
      } 
   
      if (enabled)
      {
        rect.Offset(1, 1);  
        Text(gr, rect, isHot, fnt, Brushes.White, context, text, align);
        rect.Offset(-1,-1);
      }

      Text(gr, rect, isHot, fnt, enabled ? (isHot ? Brushes.Blue : Brushes.Black) : Brushes.Gray, context, text, align);
      
      
      if (enabled && marked)
        Utils.DrawTextHighlight(gr, rect, fnt, text, align, Color.FromArgb((int)((float)255 * markerIntensity), markerColor));

      if (isHot) fnt.Dispose();
    }


               



    public Padding TextBoxMetrics
    {
      get
      {
       return new Padding(4, 4, 4, 6);
      }
    }

    public Padding FocusedTextBoxMetrics
    {
      get
      {
        return new Padding(4, 4, 4, 3);
      }
    }


    public Color TextBoxBackgroundColor
    { 
     get
     {
       return Color.FromArgb(0xff, 0xf0, 0x80);
     }
    
    }



    public void TextBox(Graphics gr, 
                        Rectangle rect,
                        bool isHot,
                        IFieldControlContext context, 
                        Bitmap textImage)
    {
      bool multiline;
      bool focused, enabled, applicable;
      bool modified;
      bool error;
      
      if (context!=null)
      {                 
        multiline = context.Multiline;
        focused = context.Focused;
        enabled = context.IsEnabled;
        applicable = context.IsApplicable;
        modified = context.GUIModified;
        error = context.Validated && !context.Valid;
      }
      else
      {
        multiline = false;
        focused = false;
        modified = false;
        error = false;  
        enabled = false;
        applicable = true;   
      }

      
      if (enabled)
      {
        using (LinearGradientBrush br =
              new LinearGradientBrush
                (
                     rect,
                     (focused) ? TextBoxBackgroundColor : Color.White,
                     (focused) ? TextBoxBackgroundColor : Color.FromArgb(0xe0, 0xe0, 0xff),
                     90
                )
            )
        {
            if (!focused)
            {
              ColorBlend blend = new ColorBlend(3);

              blend.Colors[0] = Color.FromArgb(0xff, 0xff, 0xff);
              blend.Colors[1] = Color.FromArgb(0xb0, 0xb0, 0xff);
              blend.Colors[2] = Color.FromArgb(0xf0, 0xf0, 0xff);

              blend.Positions[0] = 0;
              blend.Positions[1] = 0.5f;
              blend.Positions[2] = 1;

              br.InterpolationColors = blend;
            }
            gr.FillRectangle(br, rect);
        }
        
        gr.DrawRectangle(Pens.SlateBlue, rect);
        //Right line
        gr.DrawLine(Pens.Navy, rect.Right, rect.Top + 1, rect.Right, rect.Bottom);

        //Bottom Shelf
        // gr.DrawLine(Pens.White, rect.Left + 1, rect.Bottom - 1, rect.Right - 2, rect.Bottom - 1);
        gr.DrawLine(Pens.Navy, rect.Left + 1, rect.Bottom, rect.Right, rect.Bottom);
      }  
      else
      {
        gr.FillRectangle(Brushes.Snow, rect);
        gr.DrawRectangle(Pens.Silver, rect);
      }  
      
      if (focused)
         System.Windows.Forms.ControlPaint.DrawFocusRectangle(gr, rect);
           
      
      
      
      if (textImage!=null)
        gr.DrawImage(textImage, rect.Left + TextBoxMetrics.Left, rect.Top + TextBoxMetrics.Top);
      
      
      if (enabled)
      {
          if (error)
           Utils.DrawUnderline(gr, new Point(rect.Left+4, rect.Bottom -4), rect.Width - 8, Color.FromArgb(150, Color.Red));
          else
           if (modified)
             Utils.DrawUnderline(gr, new Point(rect.Left + 4, rect.Bottom - 4), rect.Width - 8, Color.FromArgb(128, Color.Green));
      }
    }


    public Padding ComboButtonPadding 
    {
     get { return new Padding(1,2,1,2); }
    }

    public double ComboTextBoxWidthPercent 
    { 
     get { return 1.0; }
    }

    public double ComboButtonSizePercent 
    {
     get { return 1.0; }
    }




    public void ComboButton(Graphics gr,
                      Rectangle rect,
                      bool isHot,
                      bool isPressed,
                      IFieldControlContext context)
    {
      if (rect.Width < MIN_RB_SIZE) rect.Width = MIN_RB_SIZE;
      if (rect.Height < MIN_RB_SIZE) rect.Height = MIN_RB_SIZE;

      bool enabled;
      
      if (context!=null)
      {
         enabled = context.IsEnabled && context.IsApplicable;
      }
      else
      {
         enabled = true;
      }


      if (enabled)
      {
        //Draw  button base
        using (LinearGradientBrush br = 
                    isHot? new LinearGradientBrush(rect,Color.White, Color.FromArgb(210, 0xff, 0xbf, 0x70), BRUSH_GRADIENT_ANGLE) :
                           new LinearGradientBrush(rect,Color.White, Color.FromArgb(128, 0xb0, 0xb0, 0xff), BRUSH_GRADIENT_ANGLE) 
                    )
        {
          gr.FillRectangle(br, rect);
        }
          
        
        if (isHot)      
         gr.DrawRectangle(Pens.SlateBlue,rect);
        else
         gr.DrawRectangle(Pens.Silver, rect);
      }
      
      if (isPressed)
      {
        //left shadow
        gr.DrawLine(Pens.Navy, rect.Left, rect.Bottom, rect.Left, rect.Top);

        //Top shadow
        gr.DrawLine(Pens.Navy, rect.Left, rect.Top, rect.Right, rect.Top);
      }
      
      //Draw chevron
      
      if (isPressed) 
         rect.Offset(0,1); //make chevron look like pushed
      
      using (Pen pen = new Pen(enabled? Color.Black : Color.Silver, 1))
      {
        int minDim = rect.Height<rect.Width ? rect.Height : rect.Width;
        
        Point midPoint = new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
        
        midPoint.Offset(0, (int)(minDim * 0.19)); //move midPoint down
        
        int delta = (int)(minDim * 0.25);// chevron branch proportions
        
        Point lPoint = midPoint;
        lPoint.Offset(-delta, -delta);

        Point rPoint = midPoint;
        rPoint.Offset(+delta, -delta);
        
        gr.DrawLine(pen, midPoint, lPoint);
        gr.DrawLine(pen, midPoint, rPoint);      
      }

    }




    public void ComboBoxLookupForm(Form form, Graphics gr)
    {
      Rectangle rect = form.ClientRectangle;
      rect.Width -=2;
      rect.Height -=2;
      
      gr.DrawRectangle(Pens.SlateBlue, rect);
      
      //bottom
      gr.DrawLine(Pens.Black, rect.Left+1, rect.Bottom+1, rect.Right+1, rect.Bottom+1);

      //right
      gr.DrawLine(Pens.Black, rect.Right+1, rect.Bottom, rect.Right+1, rect.Top+1);
      
    }



    public void Balloon(Graphics gr,
                  Rectangle rect,
                  Point target,
                  Color color,
                  IFieldControlContext context)
    {
      Point[] balloon = VectorUtils.VectorizeBalloon(rect, target, Math.PI / 8);
      
      gr.SmoothingMode = SmoothingMode.AntiAlias;
            
      using (LinearGradientBrush br =
              new LinearGradientBrush(rect, Color.White, color, LinearGradientMode.ForwardDiagonal)
             )
      {
        br.WrapMode = WrapMode.TileFlipXY;
        
        gr.FillClosedCurve(br, balloon, FillMode.Alternate, 0.08f);
      }

      gr.DrawClosedCurve(Pens.Black, balloon, 0.08f, FillMode.Alternate);
    }
    
    
    

    #endregion
    
    
    #region Private Utils
      private Rectangle pinnedInflate(Rectangle rect, int dx, int dy)
      {
        return new Rectangle(rect.Left, rect.Top, rect.Width + dx, rect.Height + dy);
      }
    #endregion
  }
  
  
}
