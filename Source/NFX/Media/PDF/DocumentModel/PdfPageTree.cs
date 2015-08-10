using System.Collections.Generic;
using NFX.Media.PDF.Styling;

namespace NFX.Media.PDF.DocumentModel
{
  /// <summary>
  /// PDF Page Tree document object
  /// </summary>
  internal class PdfPageTree : PdfObject
  {
    public PdfPageTree()
    {
      m_Pages = new List<PdfPage>();
    }

    private readonly List<PdfPage> m_Pages;

    public List<PdfPage> Pages
    {
      get { return m_Pages; }
    }

    /// <summary>
    /// Creates new page and adds it to the page tree
    /// </summary>
    /// <returns></returns>
    public PdfPage CreatePage(PdfSize size)
    {
      var page = new PdfPage(this, size);
      m_Pages.Add(page);

      return page;
    }
  }
}
