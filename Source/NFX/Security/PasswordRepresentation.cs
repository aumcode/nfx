/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Security
{
  /// <summary>
  /// Flags denote types of password representation: Text/Image/Audio
  /// </summary>
  [Flags]
  public enum PasswordRepresentationType
  {
    Text = 1 << 0,
    Image = 1 << 1,
    Audio = 1 << 2
  }

  /// <summary>
  /// Provides password representation content, i.e. an image with drawn password which is understandable by humans
  /// </summary>
  public class PasswordRepresentation
  {
    public PasswordRepresentation(PasswordRepresentationType type, string contentType, byte[] content)
    {
      m_Type = type;
      m_ContentType = contentType;
      m_Content = content;
    }

    private PasswordRepresentationType m_Type;
    private string m_ContentType;
    private byte[] m_Content;

    public PasswordRepresentationType Type { get { return m_Type; } }
    public string ContentType { get { return m_ContentType; } }
    public byte[] Content { get { return m_Content; } }
  }
}
