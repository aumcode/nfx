namespace NFX.Media.PDF.DocumentModel
{
  /// <summary>
  /// XObject that can be placed in PDF document
  /// </summary>
  internal interface IPdfXObject
  {
    /// <summary>
    /// Document-wide unique x-object Id
    /// </summary>
    int XObjectId { get; set; }

    /// <summary>
    /// Returns PDF x-object indirect reference
    /// </summary>
    string GetXReference();
  }
}