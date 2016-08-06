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
using System.Linq;
using System.Text;

using NFX.CodeAnalysis.Source;
using NFX.CodeAnalysis.CSharp;

using NFX.Serialization.JSON;

namespace NFX.Wave
{
  /// <summary>
  /// Represents a URI pattern that can get matched against URI requests.
  /// The pattern is formed using regular URL grammar and captures segments denoted by "{}".
  /// Example: '/profiles/{controller}/{action="dflt value"}/{*params}'
  /// The class uses CSharp lexer, so it allows to use string escapes and unicode chars like CSharp
  /// </summary>
  public sealed class URIPattern
  {
          private enum chunkPortion{Path, Query}
          private class chunk
          {
            public chunkPortion Portion;
            public bool IsPathDiv;
            public bool IsVar;
            public bool IsWildcard;
            public string Name;
            public string DefaultValue; //i.e. city/{state='OH'} OH is default value for state variable

            public override string ToString()
            {
              return "Name:{0,-20} Dflt:{1,-15} Var:{2,-5} Wild:{3,-5} Path:{4,-10} Por:{5}".Args(Name, DefaultValue, IsVar, IsWildcard, IsPathDiv, Portion);
            }
          }


    public URIPattern(string pattern)
    {
      if (pattern==null)
        throw new WaveException(StringConsts.ARGUMENT_ERROR+"URIPattern.ctor(pattern==null)");

      m_Pattern = pattern;
      try
      {
        parse();
        m_MatchChunks = m_Chunks.Where(c=>!c.IsPathDiv).ToList();
      }
      catch(Exception error)
      {
        throw new WaveException(StringConsts.URI_PATTERN_PARSE_ERROR.Args(pattern, error.ToMessageWithType(), error));
      }
    }

    private string m_Pattern;
    private List<chunk> m_Chunks;
    private List<chunk> m_MatchChunks;

    /// <summary>
    /// Returns the original pattern
    /// </summary>
    public string Pattern { get{return m_Pattern;} }

    /// <summary>
    /// Tries to match the pattern against the URI path section and returns a JSONDataMap match object filled with pattern match or NULL if pattern could not be matched.
    /// JSONDataMap may be easily converted to dynamic by calling new JSONDynamicObject(map)
    /// </summary>
    public JSONDataMap MatchURIPath(Uri uri, bool senseCase = false)
    {
      JSONDataMap result = null;
      if (m_MatchChunks.Count==0) return new JSONDataMap(false);

      var segs = uri.LocalPath.Split('/');

      var ichunk = -1;
      chunk chunk = null;
      var wildCard = false;
      foreach(var seg in segs)
      {
           if (seg.Length==0) continue;//skip empty ////

           if (!wildCard)
           {
            ichunk++;
            if (ichunk>=m_MatchChunks.Count) return null;
            chunk = m_MatchChunks[ichunk];
           }

           if (chunk.Portion!=chunkPortion.Path) return null;

           if (chunk.IsWildcard)
           {
             wildCard = true;
             if (result==null) result = new JSONDataMap(false);
             if (!result.ContainsKey(chunk.Name))
                result[chunk.Name] = seg;
             else
                result[chunk.Name] = (string)result[chunk.Name] + '/' + seg;
           }
           else
           if (chunk.IsVar)
           {
              if (result==null) result = new JSONDataMap(false);
              result[chunk.Name] = seg;
           }
           else
           if (!chunk.Name.Equals(seg, senseCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase)) return null;
      }//foreach

      ichunk++;
      while(ichunk<m_MatchChunks.Count)
      {
        chunk=m_MatchChunks[ichunk];
        if (!chunk.IsVar) return null;//some trailing elements that are not vars and  do not match
        if (result==null)
          result = new JSONDataMap(false);
        if (!result.ContainsKey(chunk.Name))
          result[chunk.Name] = chunk.DefaultValue;
        ichunk++;
      }

      return result ?? new JSONDataMap(false);
    }

    /// <summary>
    /// Creates URI from the supplied values for this pattern
    /// </summary>
    public Uri MakeURI(IDictionary<string, object> values, Uri prefix = null)
    {
      var result = new StringBuilder();
      var portion = chunkPortion.Path;
      foreach(var chunk in m_Chunks)
      {
        if (chunk.IsPathDiv)
        {
          result.Append("/");
          continue;
        }

        if (portion!=chunk.Portion)
        {
          portion = chunk.Portion;
          result.Append('?');
        }

        if (chunk.IsVar)
        {
          object value;
          if (!values.TryGetValue(chunk.Name, out value) || value==null)
          {
            value = chunk.DefaultValue;
            if (value==null) continue;
          }
          result.Append(value.ToString());
          continue;
        }

        result.Append( chunk.Portion==chunkPortion.Query ? Uri.EscapeDataString(chunk.Name) : chunk.Name);
      }

      return prefix!=null ? new Uri(prefix, result.ToString()) :
                            new Uri(result.ToString(), UriKind.RelativeOrAbsolute);
    }

 internal string _____Chunks
 {
    get
    {
      var sb = new StringBuilder();
      foreach(var c in m_Chunks)
        sb.AppendLine(c.ToString());
      return sb.ToString();
    }
 }



    private void parse()
    {
      var source = new StringSource(m_Pattern);
      var lexer = new CSLexer(source, throwErrors: true);
      var tokens = lexer.ToList();

      m_Chunks = new List<chunk>();

      var wasWildcard = false;
      var buf = string.Empty;
      var portion = chunkPortion.Path;

      var capture = false;

      Action flushBuf = () =>
      {
         buf = buf.Trim();
         if (buf.Length==0) return;

         if (wasWildcard)
          throw new WaveException(StringConsts.URI_WILDCARD_PARSE_ERROR);

         if (capture)
         {
          //reparse buf
          var wildcard = buf.StartsWith("*");
          if (wildcard)
          {
            buf = buf.Remove(0,1).Trim();
            if (buf.Length==0) buf = "ALL";
          }

          var segs = buf.Split('=');
          if (segs.Length==2)
            m_Chunks.Add( new chunk{ Name = segs[0], DefaultValue = segs[1], Portion = portion, IsVar = true, IsWildcard = wildcard});
          else
            m_Chunks.Add( new chunk{ Name = buf, Portion = portion, IsVar = true, IsWildcard = wildcard});

          if (wildcard)
           wasWildcard = true;
         }
         else
          m_Chunks.Add( new chunk{ Name = Uri.UnescapeDataString(buf), Portion = portion});

         buf = string.Empty;
      };

         for(var i=0; i<tokens.Count; i++)
         {
            var token = tokens[i];
            if (!token.IsPrimary) continue;//skip comments etc.

            if (!capture && token.Type==CSTokenType.tBraceOpen)
            {
                flushBuf();
                capture = true;
                continue;
            }

            if (capture && token.Type==CSTokenType.tBraceClose)
            {
                flushBuf();
                capture = false;
                continue;
            }

            if (capture)
            {
              buf+=token.Text;
              continue;
            }

            if (token.Type==CSTokenType.tDiv)
            {
              flushBuf();
              m_Chunks.Add( new chunk{ IsPathDiv=true, Name = "/", Portion = portion});
              continue;
            }

            if (token.Type==CSTokenType.tTernaryIf)
            {
              flushBuf();
              portion = chunkPortion.Query;
              continue;
            }

            buf+=token.Text;
         }

         flushBuf();
    }

  }
}
