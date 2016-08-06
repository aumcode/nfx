using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Serialization.JSON;

namespace NFX.Collections
{
  /// <summary>
  /// Efficeintly maps string -> string for serialization.
  /// Compared to Dictionary[string,string] this class yields 20%-50% better Slim serialization speed improvement and 5%-10% space improvement
  /// </summary>
  [Serializable]
  public sealed class StringMap : IDictionary<string, string>, IJSONWritable
  {

    internal static Dictionary<string, string> MakeDictionary(bool senseCase)
    {
      return new Dictionary<string,string>(senseCase ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase);
    }


    private StringMap(){ }

    internal StringMap(bool senseCase, Dictionary<string, string> data)
    {
      m_CaseSensitive = senseCase;
      m_Data = data;
    }


    public StringMap(bool senseCase = true)
    {
      m_CaseSensitive = senseCase;
      m_Data = MakeDictionary(senseCase);
    }

    private bool m_CaseSensitive;
    private Dictionary<string, string> m_Data;


    public bool CaseSensitive{ get{ return m_CaseSensitive;} }

    public void Add(string key, string value) {  m_Data.Add(key, value);  }

    public bool ContainsKey(string key) {  return m_Data.ContainsKey(key);  }

    public ICollection<string> Keys  {  get{ return m_Data.Keys;} }

    public bool Remove(string key) { return m_Data.Remove(key); }


    public bool TryGetValue(string key, out string value)
    {
      return m_Data.TryGetValue(key, out value);
    }

    public ICollection<string> Values { get{ return m_Data.Values;} }

    public string this[string key]
    {
      get
      {
        string result;
        if (m_Data.TryGetValue(key, out result)) return result;
        return null;
      }
      set { m_Data[key] = value; }
    }

    public void Add(KeyValuePair<string, string> item)
    {
      ((IDictionary<string,string>)m_Data).Add(item);
    }

    public void Clear() {  m_Data.Clear(); }

    public bool Contains(KeyValuePair<string, string> item) { return ((IDictionary<string,string>)m_Data).Contains(item);}

    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {
      ((IDictionary<string,string>)m_Data).CopyTo(array, arrayIndex);
    }

    public int Count{ get{ return m_Data.Count;}}

    public bool IsReadOnly { get{ return((IDictionary<string,string>)m_Data).IsReadOnly;} }

    public bool Remove(KeyValuePair<string, string> item)
    {
      return ((IDictionary<string,string>)m_Data).Remove(item);
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
      return m_Data.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return m_Data.GetEnumerator();
    }

    public void WriteAsJSON(System.IO.TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
    {
      JSONWriter.WriteMap(wri, m_Data, nestingLevel, options);
    }
  }
}
