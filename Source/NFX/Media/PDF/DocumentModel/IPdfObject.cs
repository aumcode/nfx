namespace NFX.Media.PDF.DocumentModel
{
  /// <summary>
  /// Object that can be placed in PDF document
  /// </summary>
  internal interface IPdfObject
  {
    /// <summary>
    /// Document-wide unique object Id
    /// </summary>
    int ObjectId { get; set; }

    /// <summary>
    /// Returns PDF object indirect reference
    /// </summary>
    string GetReference();
  }
}