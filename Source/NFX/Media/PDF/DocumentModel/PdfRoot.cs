namespace NFX.Media.PDF.DocumentModel
{
  /// <summary>
  /// PDF document header
  /// </summary>
  internal class PdfRoot : PdfObject
  {
    /// <summary>
    /// Document outlines' object Id
    /// </summary>
    public PdfOutlines Outlines { get; set; }

    /// <summary>
    /// Document info's object Id
    /// </summary>
    public PdfInfo Info { get; set; }

    /// <summary>
    /// Document page tree's object Id
    /// </summary>
    public PdfPageTree PageTree { get; set; }
  }
}
