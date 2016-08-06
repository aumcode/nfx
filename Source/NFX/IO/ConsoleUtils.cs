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
 * Revision: NFX 0.3  2009.10.12
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Parsing;
using System.Net;

namespace NFX.IO
{
  /// <summary>
  /// Provides various console-helper utilities
  /// </summary>
  public static class ConsoleUtils
  {
    /// <summary>
    /// Reads password from console displaying substitute characters instead of real ones
    /// </summary>
    public static string ReadPassword(char substitute)
    {
      string buff = string.Empty;

      while (true)
      {
        char c = Console.ReadKey(true).KeyChar;
        if (Char.IsControl(c)) return buff;
        buff += c;

        if (substitute != (char)0)
          Console.Write(substitute);

      }

    }


    /// <summary>
    /// Outputs colored text from content supplied in an HTML-like grammar:
    /// </summary>
    public static void WriteMarkupContent(string content)
    {
      WriteMarkupContent(content, '<', '>');
    }


    private enum direction { Left, Center, Right };


    public static string WriteMarkupContentAsHTML(
                                                string content,
                                                Encoding encoding = null,  // UTF8 if null
                                                char open = '<', char close = '>',
                                                string mkpPRE = "pre",
                                                string mkpCssForeColorPrefix = "conForeColor_",
                                                string mkpCssBackColorPrefix = "conBackColor_",
                                                ConsoleColor defaultForeColor = ConsoleColor.White,
                                                ConsoleColor defaultBackColor = ConsoleColor.Black)
    {
      using( var str = new System.IO.StringWriter())
      {
        WriteMarkupContentAsHTML(str, content, encoding, open, close, mkpPRE, mkpCssForeColorPrefix, mkpCssBackColorPrefix, defaultForeColor, defaultBackColor);
        return str.ToString();
      }
    }

    public static void WriteMarkupContentAsHTML(System.IO.Stream output,
                                                string content,
                                                Encoding encoding = null,  // UTF8 if null
                                                char open = '<', char close = '>',
                                                string mkpPRE = "pre",
                                                string mkpCssForeColorPrefix = "conForeColor_",
                                                string mkpCssBackColorPrefix = "conBackColor_",
                                                ConsoleColor defaultForeColor = ConsoleColor.White,
                                                ConsoleColor defaultBackColor = ConsoleColor.Black)
    {
      using(var wri = new System.IO.StreamWriter(output, encoding ?? Encoding.UTF8, 1024, true))
	    {
        WriteMarkupContentAsHTML(wri, content, encoding, open, close, mkpPRE, mkpCssForeColorPrefix, mkpCssBackColorPrefix, defaultForeColor, defaultBackColor);
      }
    }


    public static void WriteMarkupContentAsHTML(System.IO.TextWriter output,
                                                string content,
                                                Encoding encoding = null,  // UTF8 if null
                                                char open = '<', char close = '>',
                                                string mkpPRE = "pre",
                                                string mkpCssForeColorPrefix = "conForeColor_",
                                                string mkpCssBackColorPrefix = "conBackColor_",
                                                ConsoleColor defaultForeColor = ConsoleColor.White,
                                                ConsoleColor defaultBackColor = ConsoleColor.Black)
    {
		    // [mkpPRE]<span class='conColor_red'>This string will be red</span>[/mkpPRE]
        if (mkpPRE.IsNotNullOrWhiteSpace())
        {
          output.Write('<');
          output.Write(mkpPRE);
          output.Write('>');
        }

        TokenParser parser = new TokenParser(content, open, close);
        Stack<ConsoleColor> stack = new Stack<ConsoleColor>();

        bool collapsespaces = false;
        ConsoleColor foreColor = defaultForeColor, backColor = defaultBackColor;
        bool isSpanOpen = false;

        foreach (TokenParser.Token tok in parser)
        {
          if (tok.IsSimpleText)
          {
            if (collapsespaces)
              output.Write(WebUtility.HtmlEncode(tok.Content.Trim()));
            else
              output.Write(WebUtility.HtmlEncode(tok.Content));
            continue;
          }

          string name = tok.Name.ToUpperInvariant().Trim();


          if (name == "LITERAL")
          {
            collapsespaces = false;
            continue;
          }

          if (name == "HTML")
          {
            collapsespaces = true;
            continue;
          }

          if (name == "BR")
          {
            output.WriteLine();
            continue;
          }

          if ((name == "SP") || (name == "SPACE"))
          {
            string txt = " ";
            int cnt = 1;

            try { cnt = int.Parse(tok["COUNT"]); }
            catch { }

            while (txt.Length < cnt) txt += " ";

            output.Write(WebUtility.HtmlEncode(txt));
            continue;
          }

          if (name == "PUSH")
          {
            stack.Push(foreColor);
            stack.Push(backColor);
            continue;
          }

          if (name == "POP")
          {
            if (stack.Count > 1)
            {
              backColor = stack.Pop();
              foreColor = stack.Pop();
              if (isSpanOpen)
              {
                isSpanOpen = false;
                output.Write("</span>");
                if (stack.Count > 1)
                {
                  isSpanOpen = true;
                  output.Write("<span class='{0}{1} {2}{3}'>".Args(mkpCssForeColorPrefix, foreColor, mkpCssBackColorPrefix, backColor));
                }
              }
            }
            continue;
          }

          if (name == "RESET")
          {
            foreColor = defaultForeColor;
            if (isSpanOpen)
            {
              isSpanOpen = false;
              output.Write("</span>");
            }

            continue;
          }


          if ((name == "F") || (name == "FORE") || (name == "FOREGROUND"))
          {
            try
            {
              ConsoleColor clr = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), tok["COLOR"], true);
              foreColor = clr;

              if (isSpanOpen) output.Write("</span>");
              output.Write("<span class='{0}{1} {2}{3}'>".Args(mkpCssForeColorPrefix, foreColor, mkpCssBackColorPrefix, backColor));
              isSpanOpen = true;
            }
            catch { }
            continue;
          }

          if ((name == "B") || (name == "BACK") || (name == "BACKGROUND"))
          {
            try
            {
              ConsoleColor clr = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), tok["COLOR"], true);
              backColor = clr;

              if (isSpanOpen) output.Write("</span>");
              output.Write("<span class='{0}{1} {2}{3}'>".Args(mkpCssForeColorPrefix, foreColor, mkpCssBackColorPrefix, backColor));
              isSpanOpen = true;
            }
            catch { }
            continue;
          }

          if ((name == "J") || (name == "JUST") || (name == "JUSTIFY"))
          {
            try
            {
              int width = int.Parse(tok["WIDTH"]);
              direction dir = (direction)Enum.Parse(typeof(direction), tok["DIR"], true);
              string txt = tok["TEXT"];


              switch (dir)
              {
                case direction.Right:
                  {
                    while (txt.Length < width) txt = " " + txt;
                    break;
                  }
                case direction.Center:
                  {
                    while (txt.Length < width) txt = " " + txt + " ";
                    if (txt.Length > width) txt = txt.Substring(0, txt.Length - 1);
                    break;
                  }
                default:
                  {
                    while (txt.Length < width) txt = txt + " ";
                    break;
                  }
              }

              output.Write(WebUtility.HtmlEncode(txt));
            }
            catch { }
            continue;
          }
        }

        if (mkpPRE.IsNotNullOrWhiteSpace())
        {
          output.Write('<');
          output.Write('/');
          output.Write(mkpPRE);
          output.Write('>');
        }
    }

    /// <summary>
    /// Outputs colored text from content supplied in an HTML-like grammar
    /// </summary>
    public static void WriteMarkupContent(string content, char open, char close)
    {


      TokenParser parser = new TokenParser(content, open, close);
      Stack<ConsoleColor> stack = new Stack<ConsoleColor>();

      bool collapsespaces = false;


      foreach (TokenParser.Token tok in parser)
      {
        if (tok.IsSimpleText)
        {
          if (collapsespaces)
            Console.Write(tok.Content.Trim());
          else
            Console.Write(tok.Content);
          continue;
        }

        string name = tok.Name.ToUpperInvariant().Trim();


        if (name == "LITERAL")
        {
          collapsespaces = false;
          continue;
        }

        if (name == "HTML")
        {
          collapsespaces = true;
          continue;
        }



        if (name == "BR")
        {
          Console.WriteLine();
          continue;
        }

        if ((name == "SP") || (name == "SPACE"))
        {
          string txt = " ";
          int cnt = 1;

          try { cnt = int.Parse(tok["COUNT"]); }
          catch { }

          while (txt.Length < cnt) txt += " ";

          Console.Write(txt);
          continue;
        }

        if (name == "PUSH")
        {
          stack.Push(Console.ForegroundColor);
          stack.Push(Console.BackgroundColor);
          continue;
        }

        if (name == "POP")
        {
          if (stack.Count > 1)
          {
            Console.BackgroundColor = stack.Pop();
            Console.ForegroundColor = stack.Pop();
          }
          continue;
        }

        if (name == "RESET")
        {
          Console.ResetColor();
          continue;
        }


        if ((name == "F") || (name == "FORE") || (name == "FOREGROUND"))
        {
          try
          {
            ConsoleColor clr = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), tok["COLOR"], true);
            Console.ForegroundColor = clr;
          }
          catch { }
          continue;
        }

        if ((name == "B") || (name == "BACK") || (name == "BACKGROUND"))
        {
          try
          {
            ConsoleColor clr = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), tok["COLOR"], true);
            Console.BackgroundColor = clr;
          }
          catch { }
          continue;
        }


        if ((name == "J") || (name == "JUST") || (name == "JUSTIFY"))
        {
          try
          {
            int width = int.Parse(tok["WIDTH"]);
            direction dir = (direction)Enum.Parse(typeof(direction), tok["DIR"], true);
            string txt = tok["TEXT"];


            switch (dir)
            {
              case direction.Right:
                {
                  while (txt.Length < width) txt = " " + txt;
                  break;
                }
              case direction.Center:
                {
                  while (txt.Length < width) txt = " " + txt + " ";
                  if (txt.Length > width) txt = txt.Substring(0, txt.Length - 1);
                  break;
                }
              default:
                {
                  while (txt.Length < width) txt = txt + " ";
                  break;
                }
            }

            Console.Write(txt);
          }
          catch { }
          continue;
        }

      }

    }

    /// <summary>
    /// Shows message with colored error header
    /// </summary>
    public static void Error(string msg, int ln = 0)
    {
       var f = Console.ForegroundColor;
       var b = Console.BackgroundColor;


        Console.ForegroundColor = ConsoleColor.Black;
        Console.BackgroundColor = ConsoleColor.Red;
        if (ln==0)
         Console.Write("ERROR:");
        else
         Console.Write("{0:D3}-ERROR:".Args(ln));
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(" "+msg);

      Console.ForegroundColor = f;
      Console.BackgroundColor = b;
    }

    /// <summary>
    /// Shows message with colored warning header
    /// </summary>
    public static void Warning(string msg, int ln = 0)
    {
       var f = Console.ForegroundColor;
       var b = Console.BackgroundColor;

        Console.ForegroundColor = ConsoleColor.Black;
        Console.BackgroundColor = ConsoleColor.Yellow;
        if (ln==0)
         Console.Write("WARNING:");
        else
         Console.Write("{0:D3}-WARNING:".Args(ln));
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(" "+msg);

      Console.ForegroundColor = f;
      Console.BackgroundColor = b;
    }

    /// <summary>
    /// Shows message with colored info header
    /// </summary>
    public static void Info(string msg, int ln = 0)
    {
       var f = Console.ForegroundColor;
       var b = Console.BackgroundColor;

        Console.ForegroundColor = ConsoleColor.Black;
        Console.BackgroundColor = ConsoleColor.Green;
        if (ln==0)
         Console.Write("Info:");
        else
         Console.Write("{0:D3}-Info:".Args(ln));
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(" "+msg);

      Console.ForegroundColor = f;
      Console.BackgroundColor = b;
    }



  }//class
}
