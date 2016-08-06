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
      /// Represents source code stored in a stream
      /// </summary>
      public class StreamSource : StreamReader, ISourceText
      {
            /// <summary>
            /// Constructs stream source with specified language and default encoding
            /// </summary>
            public StreamSource(Stream stream, Language language, string name = null)
              : base(stream)
            {
              m_Language = language;
              m_Name = name;
            }

            /// <summary>
            /// Constructs stream source with specified language and encoding
            /// </summary>
            public StreamSource(Stream stream, Encoding encoding, Language language, string name = null)
              : base(stream, encoding)
            {
              m_Language = language;
              m_Name = name;
            }


            private Language m_Language;
            private string m_Name;


            public void Reset()
            {
              BaseStream.Position = 0;
              DiscardBufferedData();
            }


            /// <summary>
            /// Returns source's name
            /// </summary>
            public string Name
            {
              get { return m_Name ?? string.Empty; }
            }

            public bool EOF
            {
              get
              {
                return EndOfStream;
              }
            }

            public char ReadChar()
            {

              return (char)Read();
            }

            public char PeekChar()
            {
              return (char)Peek();
            }

            public Language Language
            {
              get { return m_Language ?? UnspecifiedLanguage.Instance; }
            }

      }


}
