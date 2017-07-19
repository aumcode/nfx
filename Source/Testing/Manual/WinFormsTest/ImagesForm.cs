using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Diagnostics;

namespace WinFormsTest
{
  public partial class ImagesForm : Form
  {
    public ImagesForm()
    {
      InitializeComponent();
    }

    #region Load Image

    private void button1_Click(object sender, EventArgs e)
    {
      var dialog = new OpenFileDialog();
      dialog.Filter = "Image Files (*.jpg; *.jpeg; *.gif; *.bmp; *.png)|*.jpg; *.jpeg; *.gif; *.bmp; *.png";
      if (dialog.ShowDialog() != DialogResult.OK) return;

      processImage(dialog.FileName, null);
    }

    private void onImageDrop(object sender, DragEventArgs e)
    {
      processImage(null, e.Data);
    }

    private Bitmap loadImage(string path, IDataObject data)
    {
      Bitmap tempBmp = null;
      MemoryStream stream = null;
      if (!string.IsNullOrWhiteSpace(path))
      {
        tempBmp = new Bitmap(path);
      }
      else if (data.GetDataPresent(DataFormats.FileDrop))
      {
        var files = (string[])data.GetData(DataFormats.FileDrop);
        if (files.Any())
          tempBmp = new Bitmap(files[0]);
      }
      else
      {
        var html = (string)data.GetData(DataFormats.Html);
        var anchor = "src=\"";
        int idx1 = html.IndexOf(anchor) + anchor.Length;
        int idx2 = html.IndexOf("\"", idx1);
        var url = html.Substring(idx1, idx2 - idx1);

        if (url.StartsWith("http"))
        {
          using (var client = new WebClient())
          {
            var bytes = client.DownloadData(url);
            stream = new MemoryStream(bytes);
            tempBmp = new Bitmap(stream);
          }
        }
        else if (url.StartsWith("data:image"))
        {
          anchor = "base64,";
          idx1 = url.IndexOf(anchor) + anchor.Length;
          var base64Data = url.Substring(idx1);
          var bytes = Convert.FromBase64String(base64Data);
          stream = new MemoryStream(bytes);
          tempBmp = new Bitmap(stream);
        }
      }

      if (tempBmp == null)
      {
        throw new Exception("Cannot load image");
      }

      var ms = new MemoryStream();
      tempBmp.Save(ms, ImageFormat.Png);
      tempBmp.Dispose();
      if (stream != null) stream.Dispose();
      var result = new Bitmap(ms); // OMG
      m_ImgInitial.Image = Image.FromStream(ms);

      return result;
    }

    #endregion

    #region Proessing

    private void processImage(string path, IDataObject data)
    {
      const int CNT = 1000;
      try
      {
        var sw = Stopwatch.StartNew();

        using (var bitmap = loadImage(path, data))
        {
          var normBitmap = (Bitmap)NFX.IO.ImageUtils.NormalizeCenteredImage(bitmap, 32, 32);

          var topColors = NFX.IO.ImageUtils.ExtractMainColors2Iter(normBitmap, 128, 24, 0.9F);
          var start = sw.ElapsedMilliseconds;
          for(var i=0; i<CNT; i++)
           NFX.IO.ImageUtils.ExtractMainColors2Iter(normBitmap, 128, 24, 0.9F);
          var end = sw.ElapsedMilliseconds;


          m_ImgNormalized.Image = normBitmap;

          var tcolor = topColors[0];
          m_Color1.BackColor = Color.FromArgb(tcolor.R, tcolor.G, tcolor.B);
          tcolor = topColors[1];
          m_Color2.BackColor = Color.FromArgb(tcolor.R, tcolor.G, tcolor.B);
          tcolor = topColors[2];
          m_Color3.BackColor = Color.FromArgb(tcolor.R, tcolor.G, tcolor.B);
          tcolor = topColors[3];
          m_Color4.BackColor = Color.FromArgb(tcolor.R, tcolor.G, tcolor.B);

          m_TxtElapsed.Text = string.Format("1 call: {0:n3} microseconds", (1000f * (end - start)) / (float)CNT);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error: " + ex.Message);
      }
    }

    #endregion

    private void ImagesForm_DragEnter(object sender, DragEventArgs e)
    {
      e.Effect = DragDropEffects.Copy;
    }
  }
}
