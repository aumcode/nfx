/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
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
using System.Text;
using System.IO;

namespace NFX.CodeAnalysis.Source
{
  /// <summary>
  /// Provides source code from string
  /// </summary>
  public class StringSource : ISourceText
  {
    private Language m_Language;
    private string m_Name;
    private string m_Source;
    private int m_Position;


    private StringSource()
    {
    }

    public StringSource(string source, Language language = null, string name = null)
    {
      m_Language = language;
      m_Source = source;
      m_Name = name;
    }


    #region ISourceText Members

        public void Reset()
        {
          m_Position = 0;
        }

        public bool EOF
        {
          get { return m_Position >= m_Source.Length; }
        }

        public char ReadChar()
        {
          if (EOF) return (char)0;

          return m_Source[m_Position++];
        }

        public char PeekChar()
        {
          if (EOF) return (char)0;

          return m_Source[m_Position];
        }


        public Language Language
        {
          get { return m_Language ?? UnspecifiedLanguage.Instance; }
        }

        /// <summary>
        /// Provides a handy way to name an otherwise-anonymous string source code,
        /// This property is like a "file name" only data is kept in a string
        /// </summary>
        public string Name
        {
          get { return m_Name ?? string.Empty;}
        }


    #endregion
  }


  /// <summary>
  /// Represents a list of strings used as source text
  /// </summary>
  public class StringSourceList : List<StringSource>
  {

  }


}

