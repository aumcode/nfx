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
      /// Represents source code stored in a file
      /// </summary>
      public class FileSource : StreamReader, ISourceText
      {


            /// <summary>
            /// Constructs file source infering source language from file extension
            /// </summary>
            public FileSource(string fileName)
              : base(fileName)
            {
              m_Language = Language.TryFindLanguageByFileExtension(Path.GetExtension(fileName));
              m_Name = fileName;
            }

            /// <summary>
            /// Constructs file source with specified language ignoring file extension
            /// </summary>
            public FileSource(Language language, string fileName)
              : base(fileName)
            {
              m_Language = language;
              m_Name = fileName;
            }


            private Language m_Language;
            private string m_Name;


            public void Reset()
            {
              BaseStream.Position = 0;
              DiscardBufferedData();
            }


            /// <summary>
            /// Returns source's file name
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

        /// <summary>
        /// Represents a list of file names
        /// </summary>
        public class FileNameList : List<string>
        {
            /// <summary>
            /// Checks that all files exist
            /// </summary>
            public void CheckAllNames()
            {
                foreach(string fn in this)
                    if (!File.Exists(fn))
                     throw new CodeAnalysisException(StringConsts.FILE_NOT_FOUND_ERROR + (fn ?? CoreConsts.UNKNOWN));
            }


            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                foreach (string fn in this)
                    sb.AppendLine(fn.ToString());

                return sb.ToString();
            }

        }


}
