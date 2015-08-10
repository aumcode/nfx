using System.Collections.Generic;

namespace NFX.Media.PDF.DocumentModel
{
  /// <summary>
  /// PDF Trailer document object
  /// </summary>
  internal class PdfTrailer : PdfObject
  {
    public PdfTrailer()
    {
      m_ObjectOffsets = new List<string>();
    }

    private readonly List<string> m_ObjectOffsets;

    /// <summary>
    /// Id of the last inserted document object
    /// </summary>
    public int LastObjectId { get; set; }

    /// <summary>
    /// PDF document's root
    /// </summary>
    public PdfRoot Root { get; set; }

    /// <summary>
    /// Inserted objects offest in PDF format
    /// </summary>
    public List<string> ObjectOffsets
    {
      get { return m_ObjectOffsets; }
    }

    /// <summary>
    /// The offset of the XREF table
    /// </summary>
    public long XRefOffset { get; set; }

    /// <summary>
    /// Add inserted object offset to offsets collection
    /// </summary>
    /// <param name="offset">Offset</param>
    public void AddObjectOffset(long offset)
    {
      m_ObjectOffsets.Add(new string('0', 10 - offset.ToString().Length) + offset);
    }
  }
}
