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
namespace NFX.Media.PDF.DocumentModel
{
  /// <summary>
  /// PDF document header
  /// </summary>
  internal class PdfHeader : PdfObject
  {
    /// <summary>
    /// Document outlines' object Id
    /// </summary>
    public int OutlinesId { get; set; }

    /// <summary>
    /// Document info's object Id
    /// </summary>
    public int InfoId { get; set; }

    /// <summary>
    /// Document page tree's object Id
    /// </summary>
    public int PageTreeId { get; set; }
  }
}
