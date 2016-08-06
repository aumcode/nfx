
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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 1.0  2013.12.26
 * Author: Denis Latushkin<dxwizard@gmail.com>
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using NFX;
using NFX.Environment;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace NFX.Media.Graphics
{
  public class DrawingOutput: DisposableObject
  {
    #region .ctor

      public DrawingOutput()
      {
      }

      public DrawingOutput(int width, int height)
      {
        CreateBitmapNGraphics(width, height);
      }

      public DrawingOutput(int width, int height, Brush fillBrush): this(width, height)
      {
        FillRect(0, 0, width, height, fillBrush);
      }

    #endregion

    #region Pvt/Prot/Int Fields

      private Bitmap m_Bitmap;
      private System.Drawing.Graphics m_Graphics;

    #endregion

    #region Properties

      public Bitmap Bitmap { get { return m_Bitmap; } }

    #endregion

    #region Public

      public Color GetPixel(int x, int y)
      {
        return m_Bitmap.GetPixel( x, y);
      }

      public void SetPixel(int x, int y, Color color)
      {
        m_Bitmap.SetPixel(x, y, color);
      }

      public void SetPixelScaled(int x, int y, Brush brush, int scale = 1)
      {
        int bitmapX = x * scale;
        int bitmapY = y * scale;

        FillRect(bitmapX, bitmapY, scale, scale, brush);
      }

      public void FillRect(int x, int y, int width, int height, Brush brush)
      {
        m_Graphics.FillRectangle(brush, x, y, width, height);
      }

      public void SetDimensions(int width, int height)
      {
        bool dimensionChanged = m_Bitmap.Width != width || m_Bitmap.Height != height;

        if (dimensionChanged)
        {
          if (width > 0 && height > 0)
          {
            CreateBitmapNGraphics(width, height);
          }
          else
            PurgeBitmapNGraphics();
        }
      }

      public void ToImage(Stream s, ImageFormat format)
      {
        m_Bitmap.Save(s, format);
      }

    #endregion

    #region Protected

      protected override void Destructor()
      {
        PurgeBitmapNGraphics();
      }

    #endregion

    #region .pvt. impl.

      private void CreateBitmapNGraphics(int width, int height)
      {
        m_Bitmap = new Bitmap(width, height);
        m_Graphics = System.Drawing.Graphics.FromImage(m_Bitmap);
      }

      private void PurgeBitmapNGraphics()
      {
        if (m_Graphics != null)
        {
          m_Graphics.Dispose();
          m_Graphics = null;
        }

        if (m_Bitmap != null)
        {
          m_Bitmap.Dispose();
          m_Bitmap = null;
        }
      }

    #endregion

  }//class

}
