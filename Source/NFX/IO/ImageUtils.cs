/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace NFX.IO
{
  public static class ImageUtils
  {
    /// <summary>
    /// Extracts three main colors and background color from source image.
    /// Does the same as <see cref="ExtractMainColors"/> method but performs the second extraction attempt if the first attempt returns almost the same colors
    /// </summary>
    public static Color[] ExtractMainColors2Iter(Bitmap srcBmp,
                                                 int dwnFactor1=128, int dwnFactor2=24,
                                                 float borderPct=0.9F,
                                                 float imgDistEps=0.2F)
    {
      var topColors = ExtractMainColors(srcBmp, dwnFactor1, dwnFactor2, borderPct);
      var d12 = imagesAbsDist(topColors[0], topColors[1]);
      var d23 = imagesAbsDist(topColors[1], topColors[2]);
      if (d12<imgDistEps && d23<imgDistEps)
        topColors = ExtractMainColors(srcBmp, dwnFactor1/2, dwnFactor2, borderPct);

      return topColors;
    }

    /// <summary>
    /// Extracts three main colors and background color from source image by the following algorithm:
    /// 1. source image color quality reduced down to 256/<paramref name="dwnFactor1"/> color per each of RGB channel
    ///    (2*2*2=8 base colors used as default)
    /// 2. four areas (three main and one background) with the biggest color frequencies are taken
    /// 3. color frequency analisys is performed in each area which gives area main color
    ///    (image color quality reduced down to 256/<paramref name="dwnFactor2"/> color per each of RGB channel)
    ///
    /// Background area search is limited to [1-<paramref name="interiorPct"/>, <paramref name="interiorPct"/>] portion of image interior
    /// </summary>
    public static unsafe Color[] ExtractMainColors(Bitmap srcBmp,
                                                   int dwnFactor1=128, int dwnFactor2=24,
                                                   float interiorPct=0.9F)
    {
      var height   = srcBmp.Height;
      var width    = srcBmp.Width;
      var mainHist = new Dictionary<Color, int>();
      var backHist = new Dictionary<Color, int>();

      using (var dwnBmp = new Bitmap(width, height))
      {
        // extract downgraded (very few colors) color histogramm
        var srcData = srcBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, srcBmp.PixelFormat);
        int srcBytesPerPixel  = Bitmap.GetPixelFormatSize(srcBmp.PixelFormat) / 8;
        int srcHeightInPixels = srcData.Height;
        int srcWidthInBytes   = srcData.Width * srcBytesPerPixel;
        byte* srcFirstPixel   = (byte*)srcData.Scan0;

        var dwnData = dwnBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, dwnBmp.PixelFormat);
        int dwnBytesPerPixel  = Bitmap.GetPixelFormatSize(dwnBmp.PixelFormat) / 8;
        int dwnHeightInPixels = dwnData.Height;
        int dwnWidthInBytes   = dwnData.Width * dwnBytesPerPixel;
        byte* dwnFirstPixel   = (byte*)dwnData.Scan0;

        for (int y=0; y<height; y++)
        {
          byte* srcLine = srcFirstPixel + (y*srcData.Stride);
          byte* dwnLine = dwnFirstPixel + (y*dwnData.Stride);
          for (int x=0; x<srcWidthInBytes; x += srcBytesPerPixel)
          {
            var b = dwnFactor1*(srcLine[x]/dwnFactor1);
            var g = dwnFactor1*(srcLine[x+1]/dwnFactor1);
            var r = dwnFactor1*(srcLine[x+2]/dwnFactor1);
            var color = Color.FromArgb(r, g, b);

            // histogramm for main color
            if (!mainHist.ContainsKey(color)) mainHist[color] = 1;
            else mainHist[color] += 1;

            // histogramm for background color
            if (Math.Abs(2*x-srcWidthInBytes)>=interiorPct*srcWidthInBytes || Math.Abs(2*y-height)>=interiorPct*height)
            {
              if (!backHist.ContainsKey(color)) backHist[color] = 1;
              else backHist[color] += 1;
            }

            dwnLine[x]   = (byte)b;
            dwnLine[x+1] = (byte)g;
            dwnLine[x+2] = (byte)r;
          }
        }

        // take background area color and the first three colors (i.e. main image areas) except background
        var backArea = backHist.OrderByDescending(h => h.Value).First().Key;
        var areas = mainHist.Where(h => h.Key != backArea).OrderByDescending(h => h.Value).Take(3).ToList();
        var firstArea  = (areas.Count > 0) ? areas[0].Key : backArea;
        var secondArea = (areas.Count > 1) ? areas[1].Key : firstArea;
        var thirdArea  = (areas.Count > 2) ? areas[2].Key : secondArea;

        // get histogramm for background area each of three main areas
        var firstHist  = new Dictionary<Color, int>();
        var secondHist = new Dictionary<Color, int>();
        var thirdHist  = new Dictionary<Color, int>();
        var bckHist    = new Dictionary<Color, int>();
        for (int y=0; y<height; y++)
        {
          byte* srcLine = srcFirstPixel + (y*srcData.Stride);
          byte* dwnLine = dwnFirstPixel + (y*dwnData.Stride);
          for (int x=0; x<srcWidthInBytes; x += srcBytesPerPixel)
          {
            var b = dwnLine[x];
            var g = dwnLine[x+1];
            var r = dwnLine[x+2];
            var color = Color.FromArgb(r, g, b);
            Dictionary<Color, int> h;
            if      (color==firstArea)  h = firstHist;
            else if (color==secondArea) h = secondHist;
            else if (color==thirdArea)  h = thirdHist;
            else if (color==backArea)   h = bckHist;
            else continue;

            b = (byte)(dwnFactor2*(srcLine[x]/dwnFactor2));
            g = (byte)(dwnFactor2*(srcLine[x+1]/dwnFactor2));
            r = (byte)(dwnFactor2*(srcLine[x+2]/dwnFactor2));
            color = Color.FromArgb(r, g, b);
            if (!h.ContainsKey(color)) h[color] = 1;
            else h[color] += 1;
          }
        }

        srcBmp.UnlockBits(srcData);
        dwnBmp.UnlockBits(dwnData);

        // extract color for each histogram
        if (secondHist.Count==0) secondHist = firstHist;
        if (thirdHist.Count==0)  thirdHist  = secondHist;
        if (bckHist.Count==0)    bckHist    = thirdHist;
        var topColors = new[]
        {
          colorFromHist(firstHist),
          colorFromHist(secondHist),
          colorFromHist(thirdHist),
          colorFromHist(bckHist)
        };

        return topColors;
      }
    }


    /// <summary>
    /// Scales source image so it fits in the desired image size preserving aspect ratio.
    /// This function is usable for profile picture size/aspect normalization
    /// </summary>
    public static Image NormalizeCenteredImage(this Image srcImage, int targetWidth = 128, int targetHeight = 128, int xDpi = 96, int yDpi = 96)
    {
      if (srcImage==null || targetWidth<1 ||targetHeight<1 || xDpi<1 || yDpi<1)
       throw new NFXException(StringConsts.ARGUMENT_ERROR + "NormalizeCenteredImage(...)");

      var result = new Bitmap(targetWidth, targetHeight);
      result.SetResolution(xDpi, yDpi);

      using (var gr = Graphics.FromImage(result))
      {
        var scx = srcImage.Width / 2;
        var scy = srcImage.Height / 2;
        var sar = srcImage.Width / (double)srcImage.Height;
        int sx,sy,sw,sh;

        if (targetHeight>targetWidth)
        {
          var ky = srcImage.Height / (double)targetHeight;
          sw = (int)(ky * targetWidth);
          sh = srcImage.Height;
        }
        else
        {
          var kx = srcImage.Width / (double)targetWidth;
          sw = srcImage.Width;
          sh = (int)(kx * targetHeight);
        }

        if (sw>srcImage.Width)
        {
          var k = (sw-srcImage.Width) / (double)srcImage.Width;
          sw = srcImage.Width;
          sh = (int)(sh * (1.0-k*sar));
        }
        if (sh>srcImage.Height)
        {
          var k = (sh-srcImage.Height) / (double)srcImage.Height;
          sh = srcImage.Height;
          sw = (int)(sw * (1.0-k/sar));
        }

        sx = scx - sw / 2;
        sy = scy - sh / 2;

        gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        gr.DrawImage(srcImage,
                     new Rectangle(0, 0, targetWidth, targetHeight),
                     sx, sy, sw, sh, GraphicsUnit.Pixel);
      }

      return result;
    }

    #region .pvt

    private static Color colorFromHist(Dictionary<Color, int> hist)
    {
      var topColors   = hist.OrderByDescending(h => h.Value).Take(3).ToList();
      var firstColor  = topColors[0];
      var secondColor = topColors.Count > 1 ? topColors[1] : firstColor;
      var thirdColor  = topColors.Count > 2 ? topColors[2] : secondColor;
      var cnt = firstColor.Value + secondColor.Value + thirdColor.Value;

      var r = (firstColor.Key.R * firstColor.Value + secondColor.Key.R * secondColor.Value + thirdColor.Key.R * thirdColor.Value) / cnt;
      var g = (firstColor.Key.G * firstColor.Value + secondColor.Key.G * secondColor.Value + thirdColor.Key.G * thirdColor.Value) / cnt;
      var b = (firstColor.Key.B * firstColor.Value + secondColor.Key.B * secondColor.Value + thirdColor.Key.B * thirdColor.Value) / cnt;

      return Color.FromArgb(r, g, b);
    }

    private static float imagesAbsDist(Color c1, Color c2)
    {
      var d = Math.Abs(c1.R-c2.R) + Math.Abs(c1.G-c2.G) + Math.Abs(c1.B-c2.B);
      return d / 256.0F;
    }

    #endregion
  }
}
