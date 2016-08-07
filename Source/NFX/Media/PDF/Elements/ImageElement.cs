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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NFX.Media.PDF.DocumentModel;

namespace NFX.Media.PDF.Elements
{
  /// <summary>
  /// PDF image element
  /// </summary>
  public class ImageElement : PdfElement, IPdfXObject
  {
    private const string XREFERENCE_FORMAT = "/I{0}";
    private const string FULL_IMAGE_REFERENCE = "/I{0} {0} 0 R";

    public ImageElement(string filePath)
    {
      loadImage(filePath);
    }

    public ImageElement(string filePath, float width, float height)
    {
      loadImage(filePath);

      Width = width;
      Height = height;
    }

    /// <summary>
    /// Image unique object Id
    /// </summary>
    public int XObjectId { get; set; }

    /// <summary>
    /// PDF displayed image width
    /// </summary>
    public float Width { get; private set; }

    /// <summary>
    /// PDF displayed image height
    /// </summary>
    public float Height { get; private set; }

    /// <summary>
    /// Image bytes
    /// </summary>
    public byte[] Content { get; private set; }

    /// <summary>
    /// Image's own widht
    /// </summary>
    public float OwnWidth { get; private set; }

    /// <summary>
    /// Image's own height
    /// </summary>
    public float OwnHeight { get; private set; }

    /// <summary>
    /// Returns image's bits per pixel
    /// </summary>
    public int BitsPerPixel { get; private set; }

    /// <summary>
    /// Returns PDF x-object indirect reference
    /// </summary>
    public string GetXReference()
    {
      return XREFERENCE_FORMAT.Args(XObjectId);
    }

    public string GetCoupledReference()
    {
      return FULL_IMAGE_REFERENCE.Args(XObjectId);
    }

    /// <summary>
    /// Writes element into file stream
    /// </summary>
    /// <param name="writer">PDF writer</param>
    /// <returns>Written bytes count</returns>
    public override void Write(PdfWriter writer)
    {
      writer.Write(this);
    }

    private void loadImage(string filePath)
    {
      using (var image = Image.FromFile(filePath))
      using (var stream = new MemoryStream())
      {
        image.Save(stream, ImageFormat.Jpeg);
        Content = stream.ToArray();
        Width = image.Width;
        OwnWidth = image.Width;
        Height = image.Height;
        OwnHeight = image.Height;
        BitsPerPixel = Image.GetPixelFormatSize(image.PixelFormat);
      }
    }
  }
}
