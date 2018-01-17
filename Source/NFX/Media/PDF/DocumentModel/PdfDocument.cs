/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
using System.Collections.Generic;
using System.IO;
using NFX.Media.PDF.Styling;

namespace NFX.Media.PDF.DocumentModel
{
  /// <summary>
  /// Model for PDF document
  /// </summary>
  public class PdfDocument
  {
    public PdfDocument()
    {
      m_Fonts = new List<PdfFont>();
      m_Meta = new PdfMeta();
      m_Info = new PdfInfo();
      m_OutLines = new PdfOutlines();
      m_Root = new PdfRoot();
      m_PageTree = new PdfPageTree();
      m_Trailer = new PdfTrailer();
      m_ObjectRepository = new ObjectRepository();
      m_ResourceRepository = new ResourceRepository();

      m_Root.Info = m_Info;
      m_Root.Outlines = m_OutLines;
      m_Root.PageTree = m_PageTree;
      m_Trailer.Root = m_Root;

      m_PageSize = PdfPageSize.Default();
    }

    #region Fields

    private readonly List<PdfFont> m_Fonts;

    private readonly PdfMeta m_Meta;

    private readonly PdfRoot m_Root;

    private readonly PdfInfo m_Info;

    private readonly PdfOutlines m_OutLines;

    private readonly PdfPageTree m_PageTree;

    private readonly PdfTrailer m_Trailer;

    private readonly ObjectRepository m_ObjectRepository;

    private readonly ResourceRepository m_ResourceRepository;

    private readonly PdfSize m_PageSize;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Used fonts
    /// </summary>
    public List<PdfFont> Fonts
    {
      get { return m_Fonts; }
    }

    /// <summary>
    /// PDF document meta
    /// </summary>
    public PdfMeta Meta
    {
      get { return m_Meta; }
    }

    /// <summary>
    /// Document info
    /// </summary>
    public PdfInfo Info
    {
      get { return m_Info; }
    }

    /// <summary>
    /// Document outlines
    /// </summary>
    public PdfOutlines Outlines
    {
      get { return m_OutLines; }
    }

    /// <summary>
    /// Document header
    /// </summary>
    internal PdfRoot Root
    {
      get { return m_Root; }
    }

    /// <summary>
    /// Document pages
    /// </summary>
    public List<PdfPage> Pages
    {
      get { return m_PageTree.Pages; }
    }

    /// <summary>
    /// Document trailer
    /// </summary>
    internal PdfTrailer Trailer
    {
      get { return m_Trailer; }
    }

    /// <summary>
    /// Document page tree
    /// </summary>
    internal PdfPageTree PageTree
    {
      get { return m_PageTree; }
    }

    /// <summary>
    /// Size for all pages created after it's setting
    /// </summary>
    public PdfSize PageSize { get; set; }

    /// <summary>
    /// User units for all pages created after it's setting
    /// (the default user space unit is 1/72 inch)
    /// </summary>
    public PdfUnit Unit
    {
      get { return m_PageSize == null ? null : m_PageSize.Unit; }
    }

    #endregion Properties

    #region Public

    /// <summary>
    /// Adds new page to document
    /// </summary>
    /// <returns>Page</returns>
    public PdfPage AddPage(PdfUnit unit)
    {
      unit = unit ?? Unit ?? PdfUnit.Default;
      var size = PageSize != null ? PageSize.ChangeUnits(unit) : PdfPageSize.Default(unit);
      return AddPage(size);
    }

    /// <summary>
    /// Adds new page to document
    /// </summary>
    /// <returns>Page</returns>
    public PdfPage AddPage(PdfSize size = null)
    {
      size = size ?? PageSize ?? PdfPageSize.Default();
      return m_PageTree.CreatePage(size);
    }

    /// <summary>
    /// Save document to file
    /// </summary>
    /// <param name="filePath">File path</param>
    public void Save(string filePath)
    {
      using (var file = new FileStream(filePath, FileMode.Create))
      using (var writer = new PdfWriter(file))
      {
        prepare();

        writer.Write(this);
      }
    }

    #endregion Public

    #region .pvt

    /// <summary>
    /// Supplies document objects with unique sequential Ids
    /// </summary>
    private void prepare()
    {
      m_ObjectRepository.Register(m_Root);
      m_ObjectRepository.Register(m_Info);
      m_ObjectRepository.Register(m_OutLines);

      foreach (var font in Fonts)
      {
        m_ObjectRepository.Register(font);
        m_ResourceRepository.Register(font);
      }

      m_ObjectRepository.Register(m_PageTree);

      foreach (var page in Pages)
      {
        m_ObjectRepository.Register(page);
        page.Fonts.AddRange(Fonts);

        foreach (var element in page.Elements)
        {
          m_ObjectRepository.Register(element);
        }
      }

      m_Trailer.LastObjectId = m_ObjectRepository.CurrentId;
    }

    #endregion .pvt
  }
}
