/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 Dmitriy Khmaladze, IT Adapter Inc / 2015-2016 Aum Code LLC
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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 1.0  2011.02.03
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX.Parsing.JavaScript
{
  /// <summary>
  /// Strips JavaScript content off comments and whitespaces
  /// </summary>
  public class Stripper
  {
    #region .ctor
      private Stripper()
      {

      }
      public Stripper(string source)
      {
        m_Source = source;
        m_Buffer = new StringBuilder();
        parse();
      }
    #endregion

    #region Private Prop
      private string m_Source;
      private StringBuilder m_Buffer;
    #endregion

    #region Properties

      public string Source
      {
        get { return m_Source; }
      }

      public string StrippedSource
      {
        get { return m_Buffer.ToString(); }
      }
    #endregion



    #region Private Impl.

        private int idx = 0;

        private char pchr;
        private char chr;
        private char nchr;

        private bool isSingleString = false;
        private bool isDoubleString = false;

        private void move()
        {
          if (isEof)
          {
            chr = (char)0;
            nchr = (char)0;
            return;
          }

          chr = m_Source[idx];
          idx++;
          if (isEof)
            nchr = (char)0;
          else
            nchr = m_Source[idx];
        }

        private bool isEof { get { return idx >= m_Source.Length; } }
        private bool isString { get { return isSingleString || isDoubleString; } }


        private void parse()
        {

          while (!isEof)
          {
            move();
            if (!isString)
            {
              if (chr == '/' && nchr == '/')
              {
                move();
                while (!isEof)
                {
                  move();
                  if (chr == '\n' || chr == '\r') { break; }
                }
                if (isEof) break;
              }
              else
                if (chr == '/' && nchr == '*')
                {
                  move();
                  while (!isEof)
                  {
                    move();
                    if (chr == '*' && nchr == '/') { move(); move(); break; }
                  }
                  if (isEof) break;
                }


              if (chr == 0 || chr == 9 || chr == 10 || chr == 13) continue;

              if (chr == ' ' && pchr == ' ') continue;

              if (chr == '\'')
              {
                isSingleString = true;
              }
              else
                if (chr == '"')
                {
                  isDoubleString = true;
                }

            }
            else //UNDER STRING
            {
              if (isSingleString)
                if (chr == '\'' && pchr != '\\')
                {
                  isSingleString = false;
                }
              if (isDoubleString)
                if (chr == '"' && pchr != '\\')
                {
                  isDoubleString = false;
                }

            }

            m_Buffer.Append(chr);
            pchr = chr;
          }//while !eof
        }

    #endregion


  }
}
