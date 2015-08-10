namespace NFX.Media.PDF.DocumentModel
{
  /// <summary>
  /// PDF document metadata
  /// </summary>
  public class PdfMeta
  {
    public PdfMeta()
    {
      Version = Constants.DEFAULT_DOCUMENT_VERSION;
    }

    /// <summary>
    /// PDF document version
    /// </summary>
    public string Version { get; set; }
  }
}