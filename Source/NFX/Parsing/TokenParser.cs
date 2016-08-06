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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 1.0  2011.02.03
 */
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NFX.Parsing
{
  /// <summary>
  /// TokenParser class translates tokenized content string to an array list of parsed tokens with attributes
  /// </summary>
  public class TokenParser : List<TokenParser.Token>
  {

    #region Private Fields

    private string m_Content = string.Empty;
    private char m_OpenChar = '[';
    private char m_CloseChar = ']';

    #endregion



    #region Properties
    /// <summary>
    /// Retrieves raw unparsed content
    /// </summary>
    public string Content
    {
      get
      {
        return m_Content;
      }
    }

    #endregion

    public TokenParser(string content)
    {
      m_Content = content;
      Parse();
    }//.ctor

    public TokenParser(string content, char open, char close)
    {
      m_OpenChar = open;
      m_CloseChar = close;

      m_Content = content;
      Parse();
    }//.ctor



    #region Inner Classes


    /// <summary> Token class holds parsed token info along with an array of attributes </summary>
    public class Token : Hashtable
    {
      private string m_Content = string.Empty;
      private string m_Name = string.Empty;
      private bool m_IsSimpleText;







      public string this[string key]
      {
        get
        {
          object val = base[key];

          if (val == null)
            return string.Empty;
          else
          {
            Attribute attr = val as Attribute;

            if (attr == null)
              return string.Empty;
            else
              return attr.Value;
          }
        }
      }







      /// <summary>
      /// A string representing whole token body, in between open and close chars
      /// </summary>
      public string Content
      {
        get
        {
          return m_Content;
        }
      }

      /// <summary>
      /// A string representing token name - first identifier within token content
      /// </summary>
      public string Name
      {
        get
        {
          return m_Name;
        }
      }

      /// <summary>
      /// True if a token represents literal text that gets embedded in response as is
      /// </summary>
      public bool IsSimpleText
      {
        get
        {
          return m_IsSimpleText;
        }
      }

      public Token(bool simple, string content)
      {
        m_Content = content;
        m_IsSimpleText = simple;
        Parse();
      }//Token.ctor

      //Splits Content in attributes from i.e.:
      // SENSOR Name="Speed" Formula="x*1.62" output="{x} mph"
      private void Parse()
      {
        if (m_IsSimpleText)
          return;

        string content = " " + m_Content.Trim() + " ";
        StringBuilder buf = new StringBuilder();
        int i;

        for (i = 1; i < content.Length - 1; i++)
        {
          if (content[i] == ' ')
            break;
          buf.Append(content[i]);
        }//for 1

        m_Name = buf.ToString();
        buf.Length = 0;


        string nm;
        string vl;
        bool str;
        int idx = 0;
        int cidx;

        for (; i < content.Length - 1; i++)
        {
          nm = string.Empty;
          vl = string.Empty;
          str = false;

          //look for attribute name
          cidx = i;
          buf.Length = 0;
          for (; i < content.Length - 1; i++)
          {
            if (content[i] == '=')
            {
              i++;
              while ((i < content.Length - 1) && (content[i] == ' '))
                i++;
              break;
            };
            buf.Append(content[i]);
          }//for atr name
          nm = buf.ToString().Trim();

          //look for attribute value
          buf.Length = 0;
          str = false;
          for (; i < content.Length - 1; i++)
          {
            if ((buf.Length == 0) && (content[i] == '"'))
            {
              str = true;
              continue;
            }

            if (
                ((!str) && (content[i] == ' ')) ||
                ((str) && (content[i] == '"') && (content[i - 1] != '\\'))
               )
              break;

            if ((str) &&
                (content[i] == '"') &&
                (content[i - 1] == '\\') &&
                (buf.Length > 0))
            {
              buf.Length--;
            }//eat escape char

            buf.Append(content[i]);
          }//for atr value
          vl = buf.ToString();


          Add(nm.ToUpper(), new Attribute(nm, vl, idx, cidx, i - cidx));
          idx++;
        }//for attr

      }//Token.Parse

      /// <summary>
      /// Attribute class represents token's attribute
      /// </summary>
      public class Attribute
      {
        private string m_Name = string.Empty;
        private string m_Value = string.Empty;
        private int m_Index;
        private int m_ContentIndex;
        private int m_ContentLength;

        /// <summary>
        /// Represents token's attribute name
        /// </summary>
        public string Name
        {
          get
          {
            return m_Name;
          }
        }

        /// <summary>
        /// Represents token's attribute value
        /// </summary>
        public string Value
        {
          get
          {
            return m_Value;
          }
        }

        /// <summary>
        /// Attribute index (0-based position) within token tag parse order
        /// </summary>
        public int Index
        {
          get
          {
            return m_Index;
          }
        }


        /// <summary>
        /// Attribute body index in content string
        /// </summary>
        public int ContentIndex
        {
          get
          {
            return m_ContentIndex;
          }
        }

        /// <summary>
        /// Attribute body length in content string
        /// </summary>
        public int ContentLength
        {
          get
          {
            return m_ContentLength;
          }
        }



        public Attribute(string name, string val, int idx, int cidx, int clen)
        {
          m_Name = name;
          m_Value = val;
          m_Index = idx;
          m_ContentIndex = cidx;
          m_ContentLength = clen;
        }//.ctor
      }//Attribute class

    }//Token class

    #endregion


    private void Parse()
    {
      StringBuilder buf = new StringBuilder();
      string content = " " + m_Content + " ";


      bool span = false;
      bool str = false;

      for (int i = 1; i < content.Length - 1; i++)
      {
        if (span && (content[i] == '"') && (content[i - 1] != '\\'))
        {
          str = !str;
        }

        if ((content[i] == m_OpenChar) && (!span))
        {
          if (buf.Length > 0)
            Add(new Token(true, buf.ToString()));
          buf.Length = 0;
          span = true;
          continue;
        }
        else if ((content[i] == m_CloseChar) && (span) && (!str))
        {
          if (buf.Length > 0)
            Add(new Token(false, buf.ToString()));
          buf.Length = 0;
          span = false;
          continue;
        }

        buf.Append(content[i]);

      }//for

      if (buf.Length > 0)
        Add(new Token(true, (span ? m_OpenChar.ToString() : "") + buf.ToString()));

    }//TokenParser.Parse



  }//TokenParser class
}
