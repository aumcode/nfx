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
 * Revision: NFX 1.0  2013.11.15
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX
{
  /// <summary>
  /// Some helpful extensions for standard collections
  /// </summary>
  public static class CollectionUtils
  {
    /// <summary>
    /// Runs some method over each element of src sequence
    /// </summary>
    /// <typeparam name="T">Sequence item type</typeparam>
    /// <param name="src">Source sequence</param>
    /// <param name="action">Method to run over each element</param>
    /// <returns>Source sequence (to have ability to chain similar calls)</returns>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> src, Action<T> action)
    {
      foreach (T item in src)
        action(item);

      return src;
    }

    /// <summary>
    /// Runs some method over each element of src sequence
    /// </summary>
    /// <typeparam name="T">Sequence item type</typeparam>
    /// <param name="src">Source sequence</param>
    /// <param name="action">Method to call on each element and its sequence number</param>
    /// <returns>Source sequence (to have ability to chain similar calls)</returns>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> src, Action<T, int> action)
    {
      int i = 0;
      foreach (T item in src)
        action(item, i++);

      return src;
    }

    /// <summary>
    /// Add all values from range sequence to src IDictionary. Source is actually modified.
    /// </summary>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <typeparam name="TValue">Type of value</typeparam>
    /// <param name="src">Source IDictionary (where to add range)</param>
    /// <param name="range">Sequence that should be added to source IDictionary</param>
    /// <returns>Source with added elements from range (to have ability to chain operations)</returns>
    public static IDictionary<TKey, TValue> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> src, IEnumerable<KeyValuePair<TKey, TValue>> range)
    {
      foreach (KeyValuePair<TKey, TValue> kvp in range)
        src.Add(kvp.Key, kvp.Value);

      return src;
    }


    /// <summary>
    /// Takes all elements except for last element from the given source
    /// </summary>
    public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source)
    {
      var buffer = default(T);
      var buffered = false;

      foreach(var x in source)
      {
        if (buffered)
          yield return buffer;

        buffer = x;
        buffered = true;
      }
    }

    /// <summary>
    /// Takes all but last N elements from the source
    /// </summary>
    public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source, int n)
    {
      var buffer = new Queue<T>(n + 1);

      foreach(var x in source)
      {
        buffer.Enqueue(x);

        if (buffer.Count == n + 1)
          yield return buffer.Dequeue();
      }
    }


    public static TResult FirstMin<TResult, TComparand>(this IEnumerable<TResult> source,
                                                        Func<TResult, TComparand> selector) where TComparand: IComparable
    {
      TComparand dummy;
      return source.FirstMin(selector, out dummy);
    }


    public static TResult FirstMin<TResult, TComparand>(this IEnumerable<TResult> source,
                                                        Func<TResult, TComparand> selector,
                                                        out TComparand minComparand) where TComparand: IComparable
    {
      return firstMinMax(true, source, selector, out minComparand);
    }


    public static TResult FirstMax<TResult, TComparand>(this IEnumerable<TResult> source,
                                                        Func<TResult, TComparand> selector) where TComparand: IComparable
    {
      TComparand dummy;
      return source.FirstMax(selector, out dummy);
    }


    public static TResult FirstMax<TResult, TComparand>(this IEnumerable<TResult> source,
                                                        Func<TResult, TComparand> selector,
                                                        out TComparand maxComparand) where TComparand: IComparable
    {
      return firstMinMax(false, source, selector, out maxComparand);
    }

    private static TResult firstMinMax<TResult, TComparand>(bool min,
                                                        IEnumerable<TResult> source,
                                                        Func<TResult, TComparand> selector,
                                                        out TComparand latchedComparand) where TComparand: IComparable
    {
      var latchedResult = default(TResult);
      latchedComparand = default(TComparand);

      if (source==null || selector==null) return latchedResult;

      latchedComparand = default(TComparand);
      bool was = false;
      foreach(var elm in source)
      {
        var c = selector(elm);
        if (!was || (min ? c.CompareTo(latchedComparand)<0 : c.CompareTo(latchedComparand)>0))
        {
          latchedResult = elm;
          latchedComparand = c;
          was = true;
        }
      }

      return latchedResult;
    }


    /// <summary>
    /// Tries to find the first element that matches the predicate and returns it,
    /// otherwise returns the first element found or default (i.e. null)
    /// </summary>
    public static TResult FirstOrAnyOrDefault<TResult>(this IEnumerable<TResult> source, Func<TResult, bool> predicate)
    {
      if (source==null) return default(TResult);

      if (predicate!=null)
        foreach(var elm in source) if (predicate(elm)) return elm;

      return source.FirstOrDefault();
    }



  }
}
