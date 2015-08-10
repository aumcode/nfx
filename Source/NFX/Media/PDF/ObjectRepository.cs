using System.Collections.Generic;
using NFX.Media.PDF.DocumentModel;

namespace NFX.Media.PDF
{
  /// <summary>
  /// Class that generates document-wide unique object Id-s
  /// (the class is not thread-safe)
  /// </summary>
  internal class ObjectRepository
  {
    public ObjectRepository()
    {
      m_CurrentId = 0;
      m_Objects = new Dictionary<int, IPdfObject>();
    }

    private readonly Dictionary<int, IPdfObject> m_Objects;

    private int m_CurrentId;

    public int CurrentId
    {
      get { return m_CurrentId; }
    }

    public IPdfObject GetObject(int id)
    {
      IPdfObject result;
      m_Objects.TryGetValue(id, out result);
      return result;
    }

    internal void Register(IPdfObject pdfObject)
    {
      pdfObject.ObjectId = ++m_CurrentId;
      m_Objects[m_CurrentId] = pdfObject;

      if (pdfObject is IPdfXObject)
      {
        ((IPdfXObject)pdfObject).XObjectId = ++m_CurrentId;
        m_Objects[m_CurrentId] = pdfObject;
      }
    }
  }
}