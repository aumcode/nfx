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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using NFX.Collections;
using NFX.Log;

namespace NFX.Parsing
{
  public static class CSVParser
  {
    public sealed class CharIteratorWithStats : ILookAheadEnumerator<char>
    {
      private readonly ILookAheadEnumerator<char> m_Enumerator;
      public int Line { get; private set; }
      public int Column { get; private set; }
      public int Count { get; set; }
      public int RequiredCount { get; set; }
      public bool DetectCount { get; set; }

      public CharIteratorWithStats(ILookAheadEnumerator<char> enumerator, int count = -1)
      {
        m_Enumerator = enumerator;
        Line = 0;
        Column = 0;
        Count = 0;
        RequiredCount = count;
        DetectCount = false;
      }
      public bool HasNext { get { return m_Enumerator.HasNext; } }
      public char Next { get { return m_Enumerator.Next; } }
      public char Current { get { return m_Enumerator.Current; } }
      public void Dispose()
      {
        Line = 0;
        Column = 0;
        m_Enumerator.Dispose();
      }
      object IEnumerator.Current { get { return Current; } }
      public bool MoveNext()
      {
        var result = m_Enumerator.MoveNext();
        if (result && '\n' == m_Enumerator.Current)
        {
          Line++;
          Column = 0;
        }
        else
        {
          Column++;
        }
        return result;
      }
      public void Reset()
      {
        Line = 0;
        Column = 0;
        m_Enumerator.Reset();
      }
    }

    public static IEnumerable<IEnumerable<string>> ParseCSV(this IEnumerable<char> stream,
      bool trim = false,
      bool skipHeader = false,
      int columns = -1,
      bool skipIfMore = false,
      bool addIfLess = false)
    {
      var enumerator = new CharIteratorWithStats(stream.AsLookAheadEnumerable().GetLookAheadEnumerator(), columns);
      var sb = new StringBuilder();
      var ws = new StringBuilder();
      if (skipHeader && enumerator.HasNext)
      {
        enumerator.DetectCount = columns < 0;
        foreach (var value in parseCSVRow(enumerator, trim, skipIfMore, addIfLess, sb, ws)) ;
      }
      enumerator.DetectCount = columns < 0;
      while (enumerator.HasNext)
      {
        yield return parseCSVRow(enumerator, trim, skipIfMore, addIfLess, sb, ws);
      }
    }

    public static IEnumerable<string> ParseCSVRow(this IEnumerable<char> row,
      bool trim = false,
      int columns = -1,
      bool skipIfMore = false,
      bool addIfLess = false)
    {
      return parseCSVRow(new CharIteratorWithStats(row.AsLookAheadEnumerable().GetLookAheadEnumerator(), columns), trim, skipIfMore, addIfLess, new StringBuilder(), new StringBuilder());
    }

    private enum State
    {
      None,
      Value,
      Quote,
      AfterQuote
    };

    private static IEnumerable<string> parseCSVRow(CharIteratorWithStats enumerator, bool trim, bool skipIfMore, bool addIfLess, StringBuilder sb, StringBuilder ws)
    {
      if (sb.Length != 0)
        throw new CSVParserException("{0}.ParseCSVRow(!sb.empty)".Args(enumerator.GetType().Name), enumerator.Line, enumerator.Column);
      if (ws.Length != 0)
        throw new CSVParserException("{0}.ParseCSVRow(!ws.empty)".Args(enumerator.GetType().Name), enumerator.Line, enumerator.Column);
      var state = State.None;
      enumerator.Count = 0;
      while(enumerator.MoveNext())
      {
        var c = enumerator.Current;
        if (('\n' == c || '\r' == c) && state != State.Quote)
        {
          if ('\r' == c && enumerator.HasNext && '\n' == enumerator.Next)
            enumerator.MoveNext();
          break;
        }

        if (char.IsWhiteSpace(c))
        {
          ws.Append(c);
          continue;
        }
        switch (state)
        {
          case State.None:
            if ('"' == c && (ws.Length == 0 || trim))
            {
              state = State.Quote;
              ws.Clear();
            }
            else if (',' == c)
            {
              if (!trim) sb.Append(ws.ToString());
              ws.Clear();
              if (!(skipIfMore && enumerator.RequiredCount >= 0 && enumerator.Count >= enumerator.RequiredCount))
              {
                enumerator.Count++;
                yield return sb.ToString();
              }
              sb.Clear();
            }
            else
            {
              state = State.Value;
              if (!trim) sb.Append(ws.ToString());
              ws.Clear();
              sb.Append(c);
            }
            break;
          case State.Value:
            if (',' == c)
            {
              state = State.None;
              if (!trim) sb.Append(ws.ToString());
              ws.Clear();
              if (!(skipIfMore && enumerator.RequiredCount >= 0 && enumerator.Count >= enumerator.RequiredCount))
              {
                enumerator.Count++;
                yield return sb.ToString();
              }
              sb.Clear();
            }
            else
            {
              sb.Append(ws.ToString());
              ws.Clear();
              sb.Append(c);
            }
            break;
          case State.Quote:
            if ('"' == c)
            {
              sb.Append(ws.ToString());
              ws.Clear();
              if (enumerator.HasNext && '"' == enumerator.Next)
              {
                sb.Append(c);
                enumerator.MoveNext();
              }
              else
                state = State.AfterQuote;
            }
            else
            {
              sb.Append(ws.ToString());
              ws.Clear();
              sb.Append(c);
            }
            break;
          case State.AfterQuote:
            if (',' == c)
            {
              state = State.None;
              if (!trim) sb.Append(ws.ToString());
              ws.Clear();
              if (!(skipIfMore && enumerator.RequiredCount >= 0 && enumerator.Count >= enumerator.RequiredCount))
              {
                enumerator.Count++;
                yield return sb.ToString();
              }
              sb.Clear();
            }
            else
              throw new CSVParserException("{0}.parseCSVRow()".Args(enumerator.GetType().Name), enumerator.Line, enumerator.Column);
            break;
        }
      }

      if (!trim && state != State.AfterQuote) sb.Append(ws.ToString());
      else if (!trim && state == State.AfterQuote && ws.Length != 0)
        throw new CSVParserException("{0}.ParseCSVRow()".Args(enumerator.GetType().Name), enumerator.Line, enumerator.Column);
      ws.Clear();
      if (!(skipIfMore && enumerator.RequiredCount >= 0 && enumerator.Count >= enumerator.RequiredCount))
      {
        enumerator.Count++;
        yield return sb.ToString();
      }
      if (addIfLess && enumerator.RequiredCount >= 0)
        for (; enumerator.Count < enumerator.RequiredCount; enumerator.Count++)
          yield return string.Empty;
      if (enumerator.RequiredCount >= 0 && enumerator.Count != enumerator.RequiredCount)
        throw new CSVParserException("{0}.parseCSVRow(Count!=RequiredCount)({1}!={2})".Args(enumerator.GetType().Name, enumerator.Count, enumerator.RequiredCount), enumerator.Line, enumerator.Column);
      if (enumerator.RequiredCount < 0 && enumerator.DetectCount)
        enumerator.RequiredCount = enumerator.Count;
      sb.Clear();
    }
  }

  [Serializable]
  public class CSVParserException : NFXException
  {
    public readonly int Line;
    public readonly int Column;

    public CSVParserException(int line = -1, int column = -1) { Line = line; Column = column; }
    public CSVParserException(string message, int line = -1, int column = -1) : base(message) { Line = line; Column = column; }
    public CSVParserException(string message, Exception inner, int line = -1, int column = -1) : base(message, inner) { Line = line; Column = column; }
    protected CSVParserException(SerializationInfo info, StreamingContext context, int line = -1, int column = -1) : base(info, context) { Line = line; Column = column; }
  }

}
