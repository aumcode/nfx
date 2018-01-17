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
