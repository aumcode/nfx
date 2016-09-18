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
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NFX;
using NFX.Media.TagCodes.QR;

namespace WinFormsTest
{
  public partial class QRTestForm : Form
  {
    #region Nested Types

      private class ImageFileType
      {
        public readonly static ImageFileType BMP = new ImageFileType( "bmp", "Bitmap", ImageFormat.Bmp);
        public readonly static ImageFileType PNG = new ImageFileType( "png", "Png", ImageFormat.Png);
        public readonly static ImageFileType JPG = new ImageFileType( "jpg", "Jpeg", ImageFormat.Jpeg);
        public readonly static ImageFileType GIF = new ImageFileType( "gif", "Gif", ImageFormat.Gif);

        private static readonly ImageFileType[] TYPES = new [] { BMP, PNG, JPG, GIF};

        public static IEnumerable<ImageFileType> GetTypes()
        {
          return TYPES.AsEnumerable();
        }

        private ImageFileType (string extenstion, string description, ImageFormat format)
	      {
          Extenstion = extenstion;
          Description = description;
          Mask = "*." + extenstion;
          Format = format;
	      }

        public readonly string Extenstion;
        public readonly string Description;
        public readonly string Mask;
        public readonly ImageFormat Format;
      }

    #endregion

    public QRTestForm()
    {
      InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
      m_cmbCorrection.Items.AddRange( QRCorrectionLevel.GetLevels().ToArray());
      m_cmbCorrection.SelectedIndex = 0;

      m_cmbScale.Items.AddRange( Enum.GetValues(typeof(QRImageRenderer.ImageScale)).Cast<object>().ToArray());
      m_cmbScale.SelectedItem = QRImageRenderer.ImageScale.Scale4x;

      SyncState();
      base.OnLoad(e);
    }

    private void m_btnGenerate_Click(object sender, EventArgs e)
    {
      string content = m_txtContent.Text;
      QRCorrectionLevel correctionLevel = m_cmbCorrection.SelectedItem as QRCorrectionLevel;
      QRImageRenderer.ImageScale scale = (QRImageRenderer.ImageScale)m_cmbScale.SelectedItem;

      try
      {
        Generate(content, correctionLevel, scale);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private void m_btnSave_Click(object sender, EventArgs e)
    {
      try
      {
        Save();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void Save()
    {
      SaveFileDialog dlg = new SaveFileDialog() { AddExtension = true };

      dlg.Filter = string.Join("|", ImageFileType.GetTypes().Select(t => "{0}|{1}".Args(t.Description, t.Mask)));

      if (dlg.ShowDialog() == DialogResult.OK)
      {
        string ext = Path.GetExtension(dlg.FileName).TrimStart('.');
        ImageFileType imageType = ImageFileType.GetTypes().FirstOrDefault(t => t.Extenstion == ext);
        if (imageType == null)
        {
          MessageBox.Show("Unsupported file extension/type", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }

        m_pnlImage.BackgroundImage.Save(dlg.FileName, imageType.Format);
      }
    }

    private void Generate(string content, QRCorrectionLevel correctionLevel, QRImageRenderer.ImageScale scale)
    {
      Matrix = null;
      Matrix = QREncoderMatrix.Encode(content, correctionLevel);

      using (MemoryStream stream = new MemoryStream())
      {
        Matrix.ToBMP(stream, scale: scale);
        stream.Flush();

        TypeConverter converter = TypeDescriptor.GetConverter(typeof(Bitmap));
        Bitmap bmp = converter.ConvertFrom(stream.ToArray()) as Bitmap;

        m_pnlImage.BackgroundImage = bmp;
        m_txtTrace.Text = Matrix.ToString();
      }
    }

    private void m_pnlImage_MouseDown(object sender, MouseEventArgs e)
    {
      try
      {
        DragImage(m_pnlImage);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void DragImage(Control ctrl)
    {
      if (ctrl.BackgroundImage != null)
      {
        string tmpImageName = GetBmpFileTempName();
        ctrl.BackgroundImage.Save(tmpImageName);

        StringCollection filePaths = new StringCollection();
        filePaths.Add(tmpImageName);

        DataObject data = new DataObject();
        data.SetFileDropList(filePaths);

        ctrl.DoDragDrop(data, DragDropEffects.Copy);
      }
    }

    private string GetBmpFileTempName()
    {
      return "{0}{1}qr_{2}.bmp".Args(System.IO.Path.GetTempPath(), System.IO.Path.PathSeparator, Guid.NewGuid());
    }

    private void m_txtTrace_MouseDown(object sender, MouseEventArgs e)
    {
      DataObject data = new DataObject();
      data.SetText(m_txtTrace.Text);
      m_txtTrace.DoDragDrop( data, DragDropEffects.Copy);
    }

    private void SyncState()
    {
      if (Matrix == null)
      {
        m_btnSave.Enabled = false;
      }
      else
      {
        m_btnSave.Enabled = true;
      }
    }

    private QREncoderMatrix Matrix
    {
      get { return m_matrix; }
      set 
      { 
        m_matrix = value; 
        SyncState();
      }
    }

    private QREncoderMatrix m_matrix;
  }
}
