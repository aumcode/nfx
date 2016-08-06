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

using NFX.Serialization.JSON;

namespace NFX.Security.CAPTCHA
{
  /// <summary>
  /// Provides methods for generation, storing, and interpretation of user actions with a keypad of random layout.
  /// This .ctor is supplied some code that user has to punch-in(touch/click) on a randomly laid-out keypad which is usually rendered as
  ///  an image. Use DecipherCoordinates() methods to convert user click/touch coordinates into characters
  /// </summary>
  [Serializable]
  public sealed class PuzzleKeypad
  {
      public const int DEFAULT_RENDER_OFFSET_X = 16;
      public const int DEFAULT_RENDER_OFFSET_Y = 16;


      public PuzzleKeypad(string code,
                          string extra = null,
                          int puzzleBoxWidth = 5,
                          int boxWidth = 35,
                          int boxHeight = 50,
                          double boxSizeVariance = 0.3d,
                          int minBoxWidth = 10,
                          int minBoxHeight = 16
                          )
      {
        if (code.IsNullOrWhiteSpace())
          throw new SecurityException(StringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(code==null|empty)");
        m_Code = code;

        if (puzzleBoxWidth<1) puzzleBoxWidth = 1;
        if (minBoxWidth<8) minBoxWidth = 8;
        if (minBoxHeight<8) minBoxHeight = 8;

        if (boxWidth<minBoxWidth) boxWidth = minBoxWidth;
        if (boxHeight<minBoxHeight) boxHeight = minBoxHeight;

        boxSizeVariance =  Math.Abs(boxSizeVariance);
        if (boxSizeVariance>=0.5d) boxSizeVariance = 0.5d;

        makePuzzle(Code.Union(extra ?? string.Empty).Distinct().ToArray(),
                    puzzleBoxWidth,
                    boxWidth,
                    boxHeight,
                    boxSizeVariance,
                    minBoxWidth,
                    minBoxHeight );
      }

      private string m_Code;
      private List<CharBox> m_Boxes = new List<CharBox>();

           [Serializable]
           public struct CharBox
           {
              public char Char;
              public Rectangle Rect;
           }


      /// <summary>
      /// Returns the original secret code that this keypad was created for
      /// </summary>
      public string Code {get{return m_Code;}}

      /// <summary>
      /// Returns char boxes
      /// </summary>
      public IEnumerable<CharBox> Boxes { get{return m_Boxes;} }

      /// <summary>
      /// Returns the size of rectangle that covers all boxes
      /// </summary>
      public Size Size
      {
        get { return new Size( m_Boxes.Max(b=>b.Rect.Right+1), m_Boxes.Max(b=>b.Rect.Bottom+1));}
      }


      /// <summary>
      /// Translates user action coordinates (i.e. screen touches or mouse clicks) into a string.
      /// The coordinates must be supplied as a JSON array of json objects that have '{x: [int], y: [int]}' structure
      /// </summary>
      public string DecipherCoordinates(JSONDataArray coords, int? offsetX = null, int? offsetY = null)
      {
        return DecipherCoordinates(coords.Where(o=>o is JSONDataMap)
                                         .Cast<JSONDataMap>()
                                         .Select(jp=>new Point(jp["x"].AsInt(), jp["y"].AsInt())),
                                   offsetX, offsetY
                                  );
      }

      /// <summary>
      /// Translates user action coordinates (i.e. screen touches or mouse clicks) into a string.
      /// The coordinates are supplied as IEnumerable(Point)
      /// </summary>
      public string DecipherCoordinates(IEnumerable<Point> coords, int? offsetX = null, int? offsetY = null)
      {
        var result = new StringBuilder();

        var ox = offsetX.HasValue? offsetX.Value : DEFAULT_RENDER_OFFSET_X;
        var oy = offsetY.HasValue? offsetY.Value : DEFAULT_RENDER_OFFSET_Y;

        foreach(var pnt in coords)
        {
          var op = pnt;
          op.Offset(-ox, -oy);
          var box = m_Boxes.Reverse<CharBox>().FirstOrDefault(b=>b.Rect.Contains(op));
          if (box.Char>0)
           result.Append(box.Char);
        }

        return result.ToString();
      }


      /// <summary>
      /// Renders default image of the keypad suitable for user entry (i.e. touch or mouse clicks)
      /// </summary>
      public Image DefaultRender(Color? bgColor = null, bool showRects = false)
      {
        var sz = Size;
        var result = new Bitmap(sz.Width + DEFAULT_RENDER_OFFSET_X*2 , sz.Height + DEFAULT_RENDER_OFFSET_Y*2);
        using(var gr = Graphics.FromImage(result))
        {
          if (bgColor.HasValue)
            gr.Clear(bgColor.Value);

          gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

          var boxPen = new Pen(Color.FromArgb(240, 240, 240), 1.0f);

          var boxPen2 = new Pen(Color.FromArgb(200, 200, 200), 1.3f);
          boxPen2.DashStyle = DashStyle.Dot;

          var shadowPen = new Pen(Color.FromArgb(255, 220,220,220), 1.0f);

          LinearGradientBrush paperBrush = null;
          Font font = null;
          LinearGradientBrush fntBrush = null;
          LinearGradientBrush fntBadBrush = null;

          try
          {
            foreach(var box in m_Boxes)
            {
              if (paperBrush==null)
              {
                 paperBrush = new LinearGradientBrush(box.Rect, Color.FromArgb(240,240,240), Color.FromArgb(255,255, 255), LinearGradientMode.Vertical);
                 paperBrush.WrapMode = WrapMode.TileFlipXY;
              }

              if (fntBrush==null)
              {
                 fntBrush = new LinearGradientBrush(box.Rect, Color.FromArgb(240,240,250), Color.FromArgb(100,80, 150), LinearGradientMode.ForwardDiagonal);
                 fntBrush.WrapMode = WrapMode.TileFlipXY;

                 fntBadBrush = new LinearGradientBrush(box.Rect, Color.FromArgb(240,240,250), Color.FromArgb(200,180, 255), LinearGradientMode.Vertical);
                 fntBadBrush.WrapMode = WrapMode.TileFlipXY;
              }

              if (font==null)
              {
                 font = new Font("Courier New", box.Rect.Height * 0.65f, FontStyle.Bold, GraphicsUnit.Pixel);
              }


              gr.ResetTransform();
              gr.TranslateTransform(DEFAULT_RENDER_OFFSET_X, DEFAULT_RENDER_OFFSET_Y);

              if (showRects)
                gr.DrawRectangle(Pens.Red, box.Rect);

              gr.RotateTransform( (float)(-3d + (6d*ExternalRandomGenerator.Instance.NextRandomDouble)));

              var pTL =  new Point(box.Rect.Left + ExternalRandomGenerator.Instance.NextScaledRandomInteger(-6, +6),
                                   box.Rect.Top + ExternalRandomGenerator.Instance.NextScaledRandomInteger(-6, +6));

              var pTR =  new Point(box.Rect.Right + ExternalRandomGenerator.Instance.NextScaledRandomInteger(-6, +6),
                                   box.Rect.Top + ExternalRandomGenerator.Instance.NextScaledRandomInteger(-6, +6));

              var pBL =  new Point(box.Rect.Left + ExternalRandomGenerator.Instance.NextScaledRandomInteger(-6, +6),
                                   box.Rect.Bottom + ExternalRandomGenerator.Instance.NextScaledRandomInteger(-6, +6));

              var pBR =  new Point(box.Rect.Right + ExternalRandomGenerator.Instance.NextScaledRandomInteger(-6, +6),
                                   box.Rect.Bottom + ExternalRandomGenerator.Instance.NextScaledRandomInteger(-6, +6));

              var pa = new []{ pTL, pTR, pBR, pBL};
              gr.FillPolygon(paperBrush, pa);
              gr.DrawPolygon(boxPen, pa);

              //gr.FillRectangle(paperBrush, box.Rect);
              //gr.DrawRectangle(boxPen, box.Rect);

              //var distortedBRX = box.Rect.Right + ExternalRandomGenerator.Instance.NextScaledRandomInteger(-10, +4);
              //var distortedBRY = box.Rect.Bottom + ExternalRandomGenerator.Instance.NextScaledRandomInteger(-10, +8);

              gr.DrawLine(shadowPen,pTR, pBR);

              gr.DrawLine(boxPen2, pBL.X+1, pBL.Y, pBR.X-1, pBR.Y);
              gr.DrawLine(boxPen2, pBL.X, pBL.Y+1, pBR.X-2, pBR.Y+1);


              var tsz = gr.MeasureString(box.Char.ToString(), font);
              var pnt = new PointF((box.Rect.Left + box.Rect.Width / 2f) - tsz.Width / 2f,
                                    (box.Rect.Top + box.Rect.Height / 2f) - tsz.Height / 2f);


              if (ExternalRandomGenerator.Instance.NextScaledRandomInteger(0,100)>40)
              {
                var bpnt = pnt;
                bpnt.X +=  ExternalRandomGenerator.Instance.NextScaledRandomInteger(-2, 4);
                bpnt.Y +=  ExternalRandomGenerator.Instance.NextScaledRandomInteger(-2, 4);
                gr.DrawString(box.Char.ToString(), font, fntBadBrush, bpnt);

                for(var i=0; i<ExternalRandomGenerator.Instance.NextScaledRandomInteger(8,75);i++)
                {
                  gr.FillRectangle(fntBadBrush, new Rectangle(box.Rect.Left+ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, box.Rect.Width-4),
                                                              box.Rect.Top +ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, box.Rect.Height-4),
                                                              ExternalRandomGenerator.Instance.NextScaledRandomInteger(1, 4),
                                                              ExternalRandomGenerator.Instance.NextScaledRandomInteger(1, 4)
                                                              ));

                }
              }

              gr.DrawString(box.Char.ToString(), font, fntBrush, pnt);
            }
          }
          finally
          {
            if (boxPen!=null) boxPen.Dispose();
            if (boxPen2!=null) boxPen2.Dispose();
            if (shadowPen!=null) shadowPen.Dispose();
            if (paperBrush!=null) paperBrush.Dispose();
            if (font!=null) font.Dispose();
            if (fntBrush!=null)
            {
              fntBrush.Dispose();
              fntBadBrush.Dispose();
            }
          }

        }
        return result;
      }





      private void makePuzzle(char[] alphabet,
                              int puzzleBoxWidth,
                              int boxWidth,
                              int boxHeight,
                              double boxSizeVariance,
                              int minBoxWidth,
                              int minBoxHeight)
      {
        alphabet = toss(alphabet);

        double wvar = boxWidth * boxSizeVariance;
        double hvar = boxHeight * boxSizeVariance;
        double wvar2 = wvar / 2d;
        double hvar2 = hvar / 2d;

        var x = 1 + (int)(wvar2 * ExternalRandomGenerator.Instance.NextRandomDouble);
        int ybase = (int)hvar + 1;

        var puzzleWidth = 0;
        var maxHeight = 0;
        foreach(var ch in alphabet)
        {
           int w = boxWidth + (int)(-wvar2 + (wvar * ExternalRandomGenerator.Instance.NextRandomDouble));
           int h = boxHeight + (int)(-hvar2 + (hvar * ExternalRandomGenerator.Instance.NextRandomDouble));

           if (w<minBoxWidth) w = minBoxWidth;
           if (h<minBoxHeight) h = minBoxHeight;

           int y = ybase + (int)(-hvar2 + (hvar * ExternalRandomGenerator.Instance.NextRandomDouble));

           var cbox = new CharBox();
           cbox.Char = ch;
           cbox.Rect = new Rectangle(x, y, w, h);
           m_Boxes.Add(cbox);

           if(cbox.Rect.Bottom-ybase>maxHeight)
           {
              maxHeight = cbox.Rect.Bottom-ybase;
           }

           puzzleWidth++;
           if (puzzleWidth==puzzleBoxWidth)
           {
              puzzleWidth = 0;
              ybase += maxHeight + 2;
              x = 1 + (int)(wvar2 * ExternalRandomGenerator.Instance.NextRandomDouble);
              continue;
           }

           x+= w + 2 + (int)(wvar2 * ExternalRandomGenerator.Instance.NextRandomDouble);
        }

      }




      private char[] toss(char[] arr)
      {
        var result = new char[arr.Length];
        for(var i = 0; i<arr.Length; i++)
        {
          var c = arr[i];
          int idx;
          do
            idx = ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, result.Length * 2);
          while(idx>result.Length-1 || result[idx]>0);
          result[idx] = c;
        }
        return result;
      }

  }

}
