using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NFX.Media.PDF.DocumentModel;
using NFX.Media.PDF.Elements;
using NFX.Media.PDF.Text;

namespace NFX.Media.PDF
{
  /// <summary>
  /// Class that aggregates PDF format-specific writing logic
  /// </summary>
  public sealed class PdfWriter : IDisposable
  {
    #region CONSTS

      private const string DATE_PDF_FORMAT = "(D:{0:yyyyMMddhhmmss})";
      private const string ARRAY_PDF_FORMAT = "[{0}]";
      private const string FONTS_REFERENCE_FORMAT = "{0} {1}";
      private const string DICTIONARY_PDF_FORMAT = "<<{0}>>";
      private const string BOX_PDF_FORMAT = "[0 0 {0} {1}]";
      private const string PROC_SET_STRING = "[/PDF /Text /ImageC]";
      private const string PATH_START_FORMAT = "{0} {1} m";
      private const string BEGIN_OBJ_FORMAT = "{0} 0 obj";
      private const string BASE_FONT_FORMAT = "/{0}";

    #endregion CONSTS

    public PdfWriter(Stream stream)
    {
      m_Stream = stream;
      m_Writer = new BinaryWriter(stream, Encoding.ASCII, true);

#if DEBUG
      PrettyFormatting = true;
#endif
    }

    private readonly Stream m_Stream;

    private readonly BinaryWriter m_Writer;

    /// <summary>
    /// Insert nonnecessary tabs and returns for a pretty output look file
    /// </summary>
    public bool PrettyFormatting { get; set; }

    #region Public

    /// <summary>
    /// Writes PDF document into file stream
    /// </summary>
    /// <param name="document">PDF document</param>
    public void Write(PdfDocument document)
    {
      // document meta data
      Write(document.Meta);

      // header object
      document.Trailer.AddObjectOffset(m_Stream.Position);
      Write(document.Root);

      // info object
      document.Trailer.AddObjectOffset(m_Stream.Position);
      Write(document.Info);

      // outlines object
      document.Trailer.AddObjectOffset(m_Stream.Position);
      Write(document.Outlines);

      // fonts
      foreach (var font in document.Fonts)
      {
        document.Trailer.AddObjectOffset(m_Stream.Position);
        Write(font);
      }

      // page tree
      document.Trailer.AddObjectOffset(m_Stream.Position);
      Write(document.PageTree);

      // pages
      foreach (var page in document.Pages)
      {
        document.Trailer.AddObjectOffset(m_Stream.Position);
        Write(page);

        // elements
        foreach (var element in page.Elements)
        {
          // all
          document.Trailer.AddObjectOffset(m_Stream.Position);
          element.Write(this);

          // images
          var image = element as ImageElement;
          if (image != null)
          {
            document.Trailer.AddObjectOffset(m_Stream.Position);
            WriteXObject(image);
          }
        }
      }

      // trailer
      document.Trailer.XRefOffset = m_Stream.Position;
      Write(document.Trailer);
    }

    internal void Write(PdfMeta meta)
    {
      writeLineRaw("%PDF-{0}", meta.Version);
      writeLineRaw("%\u00b5\u00b5\u00b5\u00b5");
    }

    /// <summary>
    /// Writes PDF font into file stream
    /// </summary>
    /// <param name="font">PDF font</param>
    internal void Write(PdfFont font)
    {
      writeBeginObject(font.ObjectId);
      writeBeginDictionary();
      writeDictionaryEntry("/Type", "/Font");
      writeDictionaryEntry("/Subtype", "/TrueType");
      writeDictionaryEntry("/Name", font.GetResourceReference());
      writeDictionaryEntry("/BaseFont", BASE_FONT_FORMAT.Args(font.Name));
      writeDictionaryEntry("/Encoding", "/WinAnsiEncoding");
      writeEndDictionary();
      writeEndObject();
    }

    /// <summary>
    /// Writes PDF header into file stream
    /// </summary>
    /// <param name="root">PDF document root</param>
    internal void Write(PdfRoot root)
    {
      writeBeginObject(root.ObjectId);
      writeBeginDictionary();
      writeDictionaryEntry("/Type", "/Catalog");
      writeDictionaryEntry("/Pages", root.PageTree.GetReference());
      writeDictionaryEntry("/Outlines", root.Outlines.GetReference());
      writeEndDictionary();
      writeEndObject();
    }

    /// <summary>
    /// Writes PDF info into file stream
    /// </summary>
    /// <param name="info">PDF document info</param>
    internal void Write(PdfInfo info)
    {
      writeBeginObject(info.ObjectId);
      writeBeginDictionary();
      if (info.Title.IsNotNullOrWhiteSpace())
        writeDictionaryEntry("/Title", info.Title);
      if (info.Subject.IsNotNullOrWhiteSpace())
        writeDictionaryEntry("/Subject", info.Subject);
      if (info.Keywords.IsNotNullOrWhiteSpace())
        writeDictionaryEntry("/Keywords", info.Keywords);
      if (info.Author.IsNotNullOrWhiteSpace())
        writeDictionaryEntry("/Author", info.Author);
      if (info.Creator.IsNotNullOrWhiteSpace())
        writeDictionaryEntry("/Creator", info.Creator);
      if (info.Producer.IsNotNullOrWhiteSpace())
        writeDictionaryEntry("/Producer", info.Producer);
      writeDictionaryEntry("/CreationDate", DATE_PDF_FORMAT.Args(info.CreationDate == DateTime.MinValue ? DateTime.UtcNow : info.CreationDate));
      writeDictionaryEntry("/ModDate", DATE_PDF_FORMAT.Args(info.ModificationDate == DateTime.MinValue ? DateTime.UtcNow : info.ModificationDate));
      writeEndDictionary();
      writeEndObject();
    }

    /// <summary>
    /// Writes PDF document outlines into file stream
    /// </summary>
    /// <param name="outlines">PDF document outlines</param>
    internal void Write(PdfOutlines outlines)
    {
      writeBeginObject(outlines.ObjectId);
      writeBeginDictionary();
      writeDictionaryEntry("/Type", "/Outlines");
      writeDictionaryEntry("/Count", "0");
      writeEndDictionary();
      writeEndObject();
    }

    /// <summary>
    /// Writes PDF document page tree into file stream
    /// </summary>
    /// <param name="pageTree">PDF document page tree</param>
    internal void Write(PdfPageTree pageTree)
    {
      if (pageTree.Pages.Count == 0)
        throw new InvalidOperationException("PDF document has no pages");

      var pages = string.Join(Constants.SPACE, pageTree.Pages.Select(p => p.GetReference()));

      writeBeginObject(pageTree.ObjectId);
      writeBeginDictionary();
      writeDictionaryEntry("/Type", "/Pages");
      writeDictionaryEntry("/Count", pageTree.Pages.Count);
      writeDictionaryEntry("/Kids", ARRAY_PDF_FORMAT.Args(pages));
      writeEndDictionary();
      writeEndObject();
    }

    /// <summary>
    /// Writes PDF page into file stream
    /// </summary>
    /// <param name="page">PDF page</param>
    internal void Write(PdfPage page)
    {
      var resourcesBuilder = new StringBuilder();
      var elements = string.Join(Constants.SPACE.ToString(), page.Elements.Select(p => p.GetReference()));
      var images = string.Join(Constants.SPACE.ToString(), page.Elements.OfType<ImageElement>().Select(p => p.GetCoupledReference()));
      var fonts = string.Join(Constants.SPACE, page.Fonts.Select(p => FONTS_REFERENCE_FORMAT.Args(p.GetResourceReference(), p.GetReference())));

      if (fonts.Length > 0)
        resourcesBuilder.AppendFormat(" /Font <<{0}>> ", fonts);
      if (images.Length > 0)
        resourcesBuilder.AppendFormat(" /XObject <<{0}>> ", images);

      var w = TextAdapter.FormatFloat(page.Width);
      var h = TextAdapter.FormatFloat(page.Height);

      writeBeginObject(page.ObjectId);
      writeBeginDictionary();
      writeDictionaryEntry("/Type", "/Page");
      writeDictionaryEntry("/UserUnit", TextAdapter.FormatFloat(page.UserUnit));
      writeDictionaryEntry("/Parent", page.Parent.GetReference());
      writeDictionaryEntry("/Resources", DICTIONARY_PDF_FORMAT.Args(resourcesBuilder));
      writeDictionaryEntry("/MediaBox", BOX_PDF_FORMAT.Args(w, h));
      writeDictionaryEntry("/CropBox", BOX_PDF_FORMAT.Args(w, h));
      writeDictionaryEntry("/Rotate", 0);
      writeDictionaryEntry("/ProcSet", PROC_SET_STRING);
      if (elements.Length > 0)
      {
        writeDictionaryEntry("/Contents", ARRAY_PDF_FORMAT.Args(elements));
      }
      writeEndDictionary();
      writeEndObject();
    }

    /// <summary>
    /// Writes PDF document trailer into file stream
    /// </summary>
    /// <param name="trailer">PDF document trailer</param>
    internal void Write(PdfTrailer trailer)
    {
      writeLineRaw("xref");
      writeLineRaw("0 {0}", trailer.LastObjectId + 1);
      writeLineRaw("0000000000 65535 f");
      foreach (var offset in trailer.ObjectOffsets)
      {
        writeLineRaw("{0} 00000 n", offset);
      }
      writeLineRaw("trailer");
      writeBeginDictionary();
      writeDictionaryEntry("/Size", trailer.LastObjectId + 1);
      writeDictionaryEntry("/Root", trailer.Root.GetReference());
      writeDictionaryEntry("/Info", trailer.Root.Info.GetReference());
      writeEndDictionary();
      writeLineRaw("startxref");
      writeLineRaw("{0}", trailer.XRefOffset);
      writeRaw(Encoding.ASCII.GetBytes("%%EOF"));
    }

    /// <summary>
    /// Writes PDF image element into file stream
    /// </summary>
    /// <param name="image">PDF image element</param>
    internal void Write(ImageElement image)
    {
      var imageContent = new StringBuilder();
      imageContent.AppendLine("q");
      imageContent.AppendFormatLine("{0} 0 0 {1} {2} {3} cm", image.Width, image.Height, image.X, image.Y);
      imageContent.AppendFormatLine("{0} Do", image.GetXReference());
      imageContent.Append("Q");

      writeStreamedObject(image.ObjectId, imageContent.ToString());
    }

    /// <summary>
    /// Writes PDF image xObject element into file stream
    /// </summary>
    /// <param name="image">PDF image xObject element</param>
    internal void WriteXObject(ImageElement image)
    {
      writeBeginObject(image.XObjectId);

      writeBeginDictionary();
      writeDictionaryEntry("/Type", "/XObject");
      writeDictionaryEntry("/Subtype", "/Image");
      writeDictionaryEntry("/Name", image.GetXReference());
      writeDictionaryEntry("/Filter", "/DCTDecode");
      writeDictionaryEntry("/Width", image.OwnWidth);
      writeDictionaryEntry("/Height", image.OwnHeight);
      writeDictionaryEntry("/BitsPerComponent", 8);
      writeDictionaryEntry("/ColorSpace", "/DeviceRGB");
      writeDictionaryEntry("/Length", image.Content.Length);
      writeEndDictionary();

      writeBeginStream();
      writeLineRaw(image.Content);
      writeEndStream();

      writeEndObject();
    }

    /// <summary>
    /// Writes PDF path into file stream
    /// </summary>
    /// <param name="path">PDF path element</param>
    internal void Write(PathElement path)
    {
      var x = TextAdapter.FormatFloat(path.X);
      var y = TextAdapter.FormatFloat(path.Y);

      var pathCoordinates = new List<string> { PATH_START_FORMAT.Args(x, y) };
      pathCoordinates.AddRange(path.Primitives.Select(p => p.ToPdfString()));

      var closeTag = path.IsClosed ? "B" : "S";

      var pathContent = new StringBuilder();
      pathContent.AppendLine("q");
      pathContent.Append(path.Style.ToPdfString());
      pathContent.AppendFormatLine(string.Join(Constants.SPACE, pathCoordinates));
      pathContent.AppendLine(closeTag);
      pathContent.Append("Q");

      writeStreamedObject(path.ObjectId, pathContent.ToString());
    }

    /// <summary>
    /// Writes PDF rectangle element into file stream
    /// </summary>
    /// <param name="rectangle">PDF rectangle element</param>
    internal void Write(RectangleElement rectangle)
    {
      var x = TextAdapter.FormatFloat(rectangle.X);
      var y = TextAdapter.FormatFloat(rectangle.Y);
      var w = TextAdapter.FormatFloat(rectangle.X1 - rectangle.X);
      var h = TextAdapter.FormatFloat(rectangle.Y1 - rectangle.Y);

      var rectangleContent = new StringBuilder();
      rectangleContent.AppendLine("q");
      rectangleContent.Append(rectangle.Style.ToPdfString());
      rectangleContent.AppendFormatLine("{0} {1} {2} {3} re", x, y, w, h);
      rectangleContent.AppendLine("B");
      rectangleContent.Append("Q");

      writeStreamedObject(rectangle.ObjectId, rectangleContent.ToString());
    }

    /// <summary>
    /// Writes PDF text element into file stream
    /// </summary>
    /// <param name="text">PDF text element</param>
    internal void Write(TextElement text)
    {
      var escapedText = TextAdapter.FixEscapes(text.Content);

      var pdfStreamBuilder = new StringBuilder();
      pdfStreamBuilder.AppendLine("q");
      pdfStreamBuilder.AppendLine("BT");
      pdfStreamBuilder.AppendFormatLine("{0} {1} Tf", text.Font.GetResourceReference(), TextAdapter.FormatFloat(text.FontSize));
      pdfStreamBuilder.AppendFormatLine("{0} rg", text.Color.ToPdfString());
      pdfStreamBuilder.AppendFormatLine("{0} {1} Td", TextAdapter.FormatFloat(text.X), TextAdapter.FormatFloat(text.Y));
      pdfStreamBuilder.AppendFormatLine("({0}) Tj", escapedText);
      pdfStreamBuilder.AppendLine("ET");
      pdfStreamBuilder.Append("Q");

      writeStreamedObject(text.ObjectId, pdfStreamBuilder.ToString());
    }

    public void Dispose()
    {
      m_Writer.Flush();
      m_Writer.Close();
    }

    #endregion Public

    #region .pvt

    private void writeStreamedObject(int objectId, string stream)
    {
      writeBeginObject(objectId);

      writeBeginDictionary();
      writeDictionaryEntry("/Length", stream.Length);
      writeEndDictionary();

      writeBeginStream();
      writeLineRaw(stream);
      writeEndStream();

      writeEndObject();
    }

    private void writeRaw(byte[] bytes)
    {
      m_Writer.Write(bytes);
    }

    private void writeLineRaw(byte[] bytes)
    {
      writeRaw(bytes);
      writeRaw(Constants.RETURN);
    }

    private void writeRaw(string str, params object[] parameters)
    {
      str = str.Args(parameters);
      var bytes = Encoding.ASCII.GetBytes(str);
      writeRaw(bytes);
    }

    private void writeLineRaw(string str, params object[] parameters)
    {
      writeRaw(str, parameters);
      writeRaw(Constants.RETURN);
    }

    private void writeBeginObject(int objectId)
    {
      var str = BEGIN_OBJ_FORMAT.Args(objectId);
      writeLineRaw(str);
    }

    private void writeEndObject()
    {
      writeLineRaw("endobj");
    }

    private void writeBeginStream()
    {
      writeLineRaw("stream");
    }

    private void writeEndStream()
    {
      writeLineRaw("endstream");
    }

    private void writeBeginDictionary()
    {
      if (PrettyFormatting)
        writeLineRaw("<<");
      else
        writeRaw("<< ");
    }

    private void writeEndDictionary()
    {
      writeLineRaw(">>");
    }

    private void writeDictionaryEntry(string key, object value)
    {
      if (PrettyFormatting)
        writeLineRaw("{0}{0}{1} {2}", Constants.SPACE, key, value);
      else
        writeRaw("{0} {1} ", key, value);
    }

    #endregion .pvt
  }
}