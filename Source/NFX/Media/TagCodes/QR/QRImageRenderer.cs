
/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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
 * Based on zXing / Apache 2.0; See NOTICE and CHANGES for attribution
 */

using System.Drawing;
using System.Drawing.Imaging;

using NFX.Media.Graphics;

namespace NFX.Media.TagCodes.QR
{
  public static class QRImageRenderer
  {
    #region CONSTS

      private static Color DEFAULT_TRUE_COLOR = Color.FromArgb(0, 0, 0);
      private static Color DEFAULT_FALSE_COLOR = Color.FromArgb(0, 0, 0);

    #endregion

    #region Inner Types

      public enum ImageScale { Scale1x = 1, Scale2x = 2, Scale3x = 3, Scale4x = 4, Scale5x = 5, Scale6x = 6, Scale7x = 7, Scale8x = 8};

    #endregion

    #region Public

      public static void ToBMP(this QRMatrix matrix,
                               System.IO.Stream stream,
                               Color? trueColor = null,
                               Color? falseColor = null,
                               ImageScale? scale = ImageScale.Scale1x)
      {
        var output = createDrawingOutput(matrix, trueColor, falseColor, scale);
        output.ToImage(stream, ImageFormat.Bmp);
      }

      public static void ToPNG(this QRMatrix matrix,
                               System.IO.Stream stream,
                               Color? trueColor = null,
                               Color? falseColor = null,
                               ImageScale? scale = ImageScale.Scale1x)
      {
        var output = createDrawingOutput(matrix, trueColor, falseColor, scale);
        output.ToImage(stream, ImageFormat.Png);
      }

      public static void ToJPG(this QRMatrix matrix,
                               System.IO.Stream stream,
                               Color? trueColor = null,
                               Color? falseColor = null,
                               ImageScale? scale = ImageScale.Scale1x)
      {
        var output = createDrawingOutput(matrix, trueColor, falseColor, scale);
        output.ToImage(stream, ImageFormat.Jpeg);
      }

      public static void ToGIF(this QRMatrix matrix,
                               System.IO.Stream stream,
                               Color? trueColor = null,
                               Color? falseColor = null,
                               ImageScale? scale = ImageScale.Scale1x)
      {
        var output = createDrawingOutput(matrix, trueColor, falseColor, scale);
        output.ToImage(stream, ImageFormat.Gif);
      }

      public static DrawingOutput CreateDrawingOutput(this QRMatrix matrix,
                                                       Color? trueColor = null,
                                                       Color? falseColor = null,
                                                       ImageScale? scale = ImageScale.Scale1x)
      {
        return createDrawingOutput(matrix, trueColor, falseColor, scale);
      }

    #endregion

    #region .pvt. impl.

      private static DrawingOutput createDrawingOutput(QRMatrix matrix,
                                                        Color? trueColor = null,
                                                        Color? falseColor = null,
                                                        ImageScale? scale = ImageScale.Scale1x)
      {
        var black = trueColor ?? Color.Black;
        var white = falseColor ?? Color.White;

        if (black == white)
          throw new NFXException(StringConsts.ARGUMENT_ERROR + typeof(QRImageRenderer).Name + ".ToBitmap(trueColor!=falseColor)");

        int scaleFactor = (int)scale;

        int canvasWidth = matrix.Width * scaleFactor;
        int canvasHeight = matrix.Height * scaleFactor;

    //TODO: Can we not cache brush instances on color in static dictionary? i.e. var blackbrush = BRUSHES[black]; Is brush thread safe for reading only?
        using(var blackBrush = new SolidBrush(black))
        using(var whiteBrush = new SolidBrush(white))
        {
          var drawingOutput = new DrawingOutput(canvasWidth, canvasHeight, blackBrush);
          for (int yMatrix = 0, yCanvasStart = 0, yCanvasStop = scaleFactor;
            yMatrix < matrix.Height;
            yMatrix++, yCanvasStart+=scaleFactor, yCanvasStop+=scaleFactor)
          {
            for (int xMatrix = 0, xCanvasStart = 0, xCanvasStop = scaleFactor;
              xMatrix < matrix.Width;
              xMatrix++, xCanvasStart+=scaleFactor, xCanvasStop+=scaleFactor)
            {
              if (matrix[xMatrix, yMatrix] == 0)
                drawingOutput.SetPixelScaled(xMatrix, yMatrix, whiteBrush, scaleFactor);
            }
          }

          return drawingOutput;
        }
      }

    #endregion

  }//class

}
