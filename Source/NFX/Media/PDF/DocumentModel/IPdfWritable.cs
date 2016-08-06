namespace NFX.Media.PDF.DocumentModel
{
  /// <summary>
  /// Represents entities that has string representation in PDF
  /// </summary>
  public interface IPdfWritable
  {
    /// <summary>
    /// Returns PDF string representation
    /// </summary>
    string ToPdfString();
  }
}