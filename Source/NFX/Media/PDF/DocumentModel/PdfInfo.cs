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
