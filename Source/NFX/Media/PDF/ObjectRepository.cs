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