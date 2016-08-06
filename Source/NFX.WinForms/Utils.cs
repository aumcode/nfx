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
using System.Text;
using System.Threading;
using System.Windows.Forms;

using System.Collections;
using System.Collections.Generic;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using NFX.WinApi;

namespace NFX.WinForms
{

  /// <summary>
  /// Provides various windows functionality
  /// </summary>
  public static class Utils
  {
    public static void FocusWindowByText(string text)
    {
      UserApi.EnumWindows(delegate(IntPtr hwnd, int LParam)
      {
        StringBuilder windowTextHolder = new StringBuilder(1024, 1024);

        UserApi.GetWindowText(hwnd, windowTextHolder, windowTextHolder.Capacity);

        if (windowTextHolder.ToString() == text)
        {
          UserApi.SetForegroundWindow(hwnd);
          return 0;
        }

        return 1;
      }, 0);
    }

    /// <summary>
    /// Initializes culture to US/English
    /// </summary>
    public static void SetUSCulture()
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
      Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
    }

    /// <summary>
    /// Determines if control is being used within designer
    /// </summary>
    public static bool IsControlDesignerHosted(Control ctrl)
    {
        if (ctrl != null)
        {

            if (ctrl.Site != null)
                if (ctrl.Site.DesignMode == true)
                    return true;


            if (IsControlDesignerHosted(ctrl.Parent))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Walks the whole child control tree and returns all controls as a flat enumeration
    /// </summary>
    public static IEnumerable<Control> AllChildControls(Control ctl)
    {
      if (ctl==null) yield break;

      foreach (Control c in ctl.Controls)
      {
        yield return c;

        foreach(Control subc in AllChildControls(c))
           yield return subc;
      }
    }



    /// <summary>
    /// Draws string on the background rectangle
    /// </summary>
    public static void DrawStringWithBackground(Graphics gr, string text,
                                                Font font,
                                                Brush textBrush,
                                                Brush backBrush,
                                                int x, int y)
    {
      SizeF size = gr.MeasureString(text,font);

      RectangleF rect = new RectangleF(x, y, size.Width, size.Height);

      gr.FillRectangle(backBrush, rect);
      gr.DrawString(text, font, textBrush, x, y);
    }


    /// <summary>
    /// Converts a bitmap into grayscale mode
    /// </summary>
    public static Bitmap CreateGrayscaleBitmap(Bitmap fromBitmap)
    {
      Bitmap newBitmap = new Bitmap(fromBitmap.Width, fromBitmap.Height);

      using (Graphics gr = Graphics.FromImage(newBitmap))
      {
        //create the grayscale ColorMatrix covert ramp
        ColorMatrix colorMatrix = new ColorMatrix(
           new float[][]
        {
           new float[] {.3f, .3f, .3f, 0, 0},
           new float[] {.59f, .59f, .59f, 0, 0},
           new float[] {.11f, .11f, .11f, 0, 0},
           new float[] {0, 0, 0, 1, 0},
           new float[] {0, 0, 0, 0, 1}
        });

        using (ImageAttributes attr = new ImageAttributes())
        {
            attr.SetColorMatrix(colorMatrix);

            gr.DrawImage(fromBitmap, new Rectangle(0, 0, fromBitmap.Width, fromBitmap.Height),
               0, 0, fromBitmap.Width, fromBitmap.Height, GraphicsUnit.Pixel, attr);

        }
      }

      return newBitmap;
    }


    /// <summary>
    /// Draws an underline. Similar concept to VS or MS Word
    /// </summary>
    public static void DrawUnderline(Graphics gr, Point start, int length, Color color)
    {
      using(Pen pen = new Pen(color, 2f))
      {
        pen.DashStyle = DashStyle.Dot;

        Point p1 = start;
        Point p2 = new Point(start.X+length, start.Y);

        gr.DrawLine(pen, p1, p2);

        p1.Offset(+2, 1);
        p2.Offset(+2, 1);

        gr.DrawLine(pen, p1, p2);
      }

    }

    /// <summary>
    /// Draws a rectangle that looks like underline which looks similar to VS or MS Word
    /// </summary>
    public static void DrawUnderlineRectangle(Graphics gr, Rectangle rect, Color color)
    {
      using (Pen pen = new Pen(color, 2f))
      {
        pen.DashStyle = DashStyle.Dot;

        //top
        gr.DrawLine(pen, rect.Left, rect.Top, rect.Right, rect.Top);
        gr.DrawLine(pen, rect.Left+2, rect.Top+1, rect.Right-2, rect.Top+1);

        //bottom
        gr.DrawLine(pen, rect.Left, rect.Bottom, rect.Right, rect.Bottom);
        gr.DrawLine(pen, rect.Left + 2, rect.Bottom - 1, rect.Right - 2, rect.Bottom - 1);

        //left
        gr.DrawLine(pen, rect.Left, rect.Top, rect.Left, rect.Bottom-1);
        gr.DrawLine(pen, rect.Left + 1, rect.Top + 2, rect.Left + 1, rect.Bottom -1);

        //right
        gr.DrawLine(pen, rect.Right, rect.Top, rect.Right, rect.Bottom - 1);
        gr.DrawLine(pen, rect.Right - 1, rect.Top + 2, rect.Right - 1, rect.Bottom - 1);
      }

    }



    /// <summary>
    /// Draws hand-drawn-looking highlighter color stick lines on top of text. The text itself is not drawn.
    /// Text may span multiple lines
    /// </summary>
    /// <param name="gr">Graphical context</param>
    /// <param name="rect">Bounding rectangle</param>
    /// <param name="font">Font to measure text in</param>
    /// <param name="text">Text to measure, the text is not drawn rather charecters position is determined</param>
    /// <param name="align">Text alignment</param>
    /// <param name="color">Color of highlighter including opacity</param>
    public static void DrawTextHighlight(Graphics gr,
                     Rectangle rect,
                     Font font,
                     string text,
                     StringAlignment align,
                     Color color)
    {
      Rectangle textRect;
      int lineHeight;

      using (StringFormat fmt = new StringFormat())
      {
        fmt.Alignment = align;
        SizeF size = gr.MeasureString(text, font, new SizeF(rect.Width, rect.Height), fmt);
        lineHeight = (int)gr.MeasureString("`~_W", font).Height;

        if (//RIGHT
            (align == StringAlignment.Near && Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft) ||
            (align == StringAlignment.Far)
           )
          textRect = new Rectangle(rect.Right - (int)size.Width,
                                   rect.Top,
                                   (int)size.Width, (int)size.Height);
        else
          if (align == StringAlignment.Center)
            textRect = new Rectangle(rect.Left + (rect.Width / 2) - (int)size.Width / 2,
                                    rect.Location.Y,
                                    (int)size.Width, (int)size.Height);
          else//LEFT
            textRect = new Rectangle(rect.Left, rect.Top, (int)size.Width, (int)size.Height);

      }

      textRect.Inflate(lineHeight, 0);
      if (textRect.Left < rect.Left) textRect.X = rect.Left;
      if (textRect.Right > rect.Right) textRect.Width -= textRect.Right - rect.Right;

      Rectangle brRect = textRect;//gets rid of brush wrap
      brRect.Inflate(lineHeight / 2, lineHeight / 2);

      using (LinearGradientBrush br =
                    new LinearGradientBrush(brRect,
                    color,
                    Color.FromArgb(16, Color.Silver),
                    (align == StringAlignment.Far) ? 180f : 360f))
      {
        gr.SmoothingMode = SmoothingMode.AntiAlias;
        Random rnd = new Random();
 //gr.DrawRectangle(Pens.Red, textRect);

        int y = textRect.Top + lineHeight / 2;

        Point[] pnt = new Point[4];

        while (y /*+ lineHeight / 2*/ < textRect.Bottom)
        {


          pnt[0] = new Point(textRect.Left, y - lineHeight / 2);
          pnt[1] = new Point(textRect.Right - lineHeight / 2, y - lineHeight / 2);
          pnt[2] = new Point(textRect.Right - lineHeight, y + lineHeight / 2);
          pnt[3] = new Point(textRect.Left + lineHeight / 2, y + lineHeight / 2);

          for (int i = 0; i < pnt.Length; i++)
            pnt[i].Offset(rnd.Next(0, 2), rnd.Next(-2, 2));


          gr.FillClosedCurve(br, pnt, FillMode.Alternate, 0.07f + (float)(rnd.NextDouble() / 6d));
          y += lineHeight + 2;//1 px padding top and bottom
        }
      }//using brush
    }





    /// <summary>
    /// Returns focused .NET control or null
    /// </summary>
    public static Control GetFocusedControl()
    {
      Control focusedControl = null;

      IntPtr focusedHandle = WinApi.UserApi.GetFocus();

      if (focusedHandle != IntPtr.Zero)
        focusedControl = Control.FromHandle(focusedHandle); // Fcused Control may be non-net control

      return focusedControl;
    }


  }

}