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
// Author: Serge Aleynikov <saleyn@gmail.com>
// Created: 2013-08-26
// This code is derived from:
// https://github.com/saleyn/otp.net/blob/master/Otp/Erlang/PatternMatcher.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NFX.Erlang
{
  public delegate IErlObject PatternMatchFunc(
    ErlPatternMatcher.Pattern pattern, IErlObject term,
    ErlVarBind binding, params object[] args);

  public delegate IErlObject PatternMatchFunc<TContext>(
    TContext ctx, ErlPatternMatcher.Pattern pattern, IErlObject term,
    ErlVarBind binding, params object[] args);

  /// <summary>
  /// Pattern matcher that implements a container of patterns to be
  /// matched against a given Erlang term.  On successful match, the
  /// corresponding func registered with that pattern gets invoked
  /// </summary>
  public class ErlPatternMatcher : IEnumerable<ErlPatternMatcher.Pattern>
  {
  #region Local Classes

    public struct Pattern
    {
      public readonly int ID;
      public readonly PatternMatchFunc Func;
      public readonly IErlObject Term;

      public Pattern(int id, PatternMatchFunc b, IErlObject p)
      {
        ID = id; Func = b; Term = p;
      }

      public Pattern(int id, PatternMatchFunc b, string pattern, params object[] args)
      {
        ID = id; Func = b; Term = ErlObject.Parse(pattern, args);
      }
    }

  #endregion

  #region Fields

    List<Pattern> m_patterns = new List<Pattern>();
    int m_lastID = 0;

  #endregion

  #region Public

    /// <summary>
    /// Add a matching pattern to the collection
    /// </summary>
    /// <param name="pattern">Pattern to compile</param>
    /// <param name="func">Function to invoke on successful match</param>
    /// <returns>ID of the newly added pattern</returns>
    public int Add(string pattern, PatternMatchFunc func)
    {
      return Add(ErlObject.Parse(pattern), func);
    }

    /// <summary>
    /// Add a matching pattern to the collection
    /// </summary>
    /// <param name="pattern">Erlang term to be used as a match pattern</param>
    /// <param name="func">Function to invoke on successful match</param>
    /// <returns>ID of the newly added pattern</returns>
    public int Add(IErlObject pattern, PatternMatchFunc func)
    {
      int id = ++m_lastID;
      var pt = new Pattern(id, (p, t, b, args) => func(p, t, b, args), pattern);
      m_patterns.Add(pt);
      return id;
    }

    /// <summary>
    /// Add a matching pattern to the collection
    /// </summary>
    /// <typeparam name="TContext">Type of context passed to func</typeparam>
    /// <param name="context">Context passed to func</param>
    /// <param name="pattern">Pattern to compile</param>
    /// <param name="func">Function to invoke on successful match</param>
    /// <returns>ID of the newly added pattern</returns>
    public int Add<TContext>(TContext context, string pattern, PatternMatchFunc<TContext> func)
    {
      return Add(context, ErlObject.Parse(pattern), func);
    }

    /// <summary>
    /// Add a matching pattern to the collection
    /// </summary>
    /// <typeparam name="TContext">Type of context passed to func</typeparam>
    /// <param name="context">Context passed to func</param>
    /// <param name="pattern">Compiled pattern containing variables to match</param>
    /// <param name="func">Function to invoke on successful match</param>
    /// <returns>ID of the newly added pattern</returns>
    public int Add<TContext>(TContext context, IErlObject pattern, PatternMatchFunc<TContext> func)
    {
      int id = ++m_lastID;
      var pt = new Pattern(id, (p, t, b, args) => func(context, p, t, b, args), pattern);
      m_patterns.Add(pt);
      return id;
    }

    /// <summary>
    /// Remove pattern from collection given its ID
    /// </summary>
    public void Remove(int id)
    {
      int i = m_patterns.FindIndex(d => d.ID == id);

      if (i != -1)
        m_patterns.RemoveAt(i);
    }

    /// <summary>
    /// Match a term against the patterns in the collection.
    /// The first successful match will result in invocation of the func
    /// associated with the pattern, and storing func result in the term
    /// </summary>
    /// <param name="term">Term to match against patterns</param>
    /// <param name="args">Arguments to be passed to an func on successful pattern match</param>
    /// <returns>ID of the pattern that matched, or -1 if there were no matches</returns>
    public int Match(ref IErlObject term, params object[] args)
    {
      var binding = new ErlVarBind();

      foreach (var p in m_patterns)
      {
        if (p.Term.Match(term, binding))
        {
          term = p.Func(p, term, binding, args);
          return p.ID;
        }
        binding.Clear();
      }

      return -1;
    }

    /// <summary>
    /// Clear the collection of patterns
    /// </summary>
    public void Clear()
    {
      m_patterns.Clear();
    }

    public string PatternsToString
    {
      get
      {
        var t = m_patterns.Select(p => p.Term).ToArray<IErlObject>();
        return (new ErlList(t, false)).ToString();
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return m_patterns.GetEnumerator();
    }

    IEnumerator<Pattern> IEnumerable<ErlPatternMatcher.Pattern>.GetEnumerator()
    {
      return m_patterns.GetEnumerator();
    }

  #endregion
  }
}
