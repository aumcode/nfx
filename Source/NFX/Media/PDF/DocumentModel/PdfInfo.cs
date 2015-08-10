using System;

namespace NFX.Media.PDF.DocumentModel
{
  /// <summary>
  /// PDF document info
  /// </summary>
  public class PdfInfo : PdfObject
  {
    /// <summary>
    /// Document's title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Document's subject
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// Document's keywords
    /// </summary>
    public string Keywords { get; set; }

    /// <summary>
    /// Document's author
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// Document's creation date
    /// </summary>
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// Document's modification date
    /// </summary>
    public DateTime ModificationDate { get; set; }

    /// <summary>
    /// Document's creator program
    /// </summary>
    public string Creator { get; set; }

    /// <summary>
    /// Document's producer (if it started as another program)
    /// </summary>
    public string Producer { get; set; }
  }
}
