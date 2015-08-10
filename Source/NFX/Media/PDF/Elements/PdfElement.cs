using NFX.Media.PDF.DocumentModel;

namespace NFX.Media.PDF.Elements
{
  /// <summary>
  /// Base class for all PDF primitives
  /// </summary>
  public abstract class PdfElement : PdfObject
  {
    /// <summary>
    /// X-coordinate
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Y-coordinate
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// Writes element into file stream
    /// </summary>
    /// <param name="writer">PDF writer</param>
    public abstract void Write(PdfWriter writer);
  }
}
