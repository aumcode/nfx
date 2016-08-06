namespace NFX.Media.PDF.DocumentModel
{
  /// <summary>
  /// PDF document header
  /// </summary>
  public abstract class PdfObject : IPdfObject
  {
    private const string REFERENCE_FORMAT = "{0} 0 R";

    /// <summary>
    /// Document-wide unique object Id
    /// </summary>
    public int ObjectId { get; set; }

    /// <summary>
    /// Returns PDF object indirect reference
    /// </summary>
    public string GetReference()
    {
      return REFERENCE_FORMAT.Args(ObjectId);
    }
  }
}
