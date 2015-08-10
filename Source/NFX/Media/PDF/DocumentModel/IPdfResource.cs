namespace NFX.Media.PDF.DocumentModel
{
  /// <summary>
  /// Object that can be placed in PDF document as a resource
  /// </summary>
  internal interface IPdfResource
  {
    /// <summary>
    /// Document-wide unique resource Id
    /// </summary>
    int ResourceId { get; set; }

    /// <summary>
    /// Returns PDF object indirect reference
    /// </summary>
    string GetResourceReference();
  }
}