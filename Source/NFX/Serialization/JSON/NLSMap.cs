/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using System.IO;

using NFX.Environment;

namespace NFX.Serialization.JSON
{
  /// <summary>
    /// Represents a JSON-serializable structure that keys [N]ame and [D]escription on lang ISO.
    /// It respects JSONWritingOptions.NLSMapLanguageISO and NLSMapLanguageISODefault
    /// </summary>
    [Serializable] //this type is directly handled by slim writer/reader
    public struct NLSMap : IEnumerable<KeyValuePair<string, NLSMap.NDPair>>, IJSONWritable
    {

      /// <summary>
      /// Facilitates the population of NLSMap from code
      /// </summary>
      public struct Builder
      {
        private Dictionary<string, NDPair> m_Data;

        public Builder Add(string langIso, string n, string d)
        {
           if (langIso.IsNullOrWhiteSpace()) return this;

           if (m_Data ==null) m_Data = makeDict();
           m_Data[langIso] = new NDPair(n, d);
           return this;
        }

        /// <summary>
        /// Returns the built map
        /// </summary>
        public NLSMap Map
        {
          get
          {
            var result = new NLSMap(false);
            result.m_Data = m_Data;
            return result;
          }
        }
      }


      /// <summary>
      /// Localized Name:Description pair
      /// </summary>
      public struct NDPair : IJSONWritable
      {
        internal NDPair(string name, string descr){ Name = name; Description = descr;}

        public bool IsAssigned{ get{ return Name.IsNotNullOrWhiteSpace() || Description.IsNotNullOrWhiteSpace(); }}

        public readonly string Name;
        public readonly string Description;

        public void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
        {
          JSONWriter.WriteMap(wri, nestingLevel, options, new System.Collections.DictionaryEntry("n", Name),
                                                          new System.Collections.DictionaryEntry("d", Description));
        }
      }

      //used by ser
      internal NLSMap(bool hasValue)
      {
        m_Data = hasValue ? makeDict() : null;
      }

      /// <summary>
      /// Makes NLSMap out of JSON config string: {eng: {n: 'Cucumber',d: 'It is green'}, deu: {n='Gurke',d='Es ist grün'}}
      /// </summary>
      public NLSMap(string nlsConf)
      {
        m_Data = makeDict();
        if (nlsConf.IsNullOrWhiteSpace()) return;
        var nlsNode = nlsConf.AsJSONConfig(handling: ConvertErrorHandling.Throw);
        ctor(nlsNode);
      }

      /// <summary>
      /// Makes NLSMap out of conf node: eng{n='Cucumber' d='It is green'} deu{n='Gurke' d='Es ist grün'}
      /// </summary>
      public NLSMap(IConfigSectionNode nlsNode)
      {
        m_Data = makeDict();
        if (nlsNode==null || !nlsNode.Exists) return;
        ctor(nlsNode);
      }

      private static Dictionary<string, NDPair> makeDict()
      {
        return new Dictionary<string, NDPair>(StringComparer.InvariantCultureIgnoreCase);
      }

      private void ctor(IConfigSectionNode nlsNode)
      {
        if (nlsNode.HasChildren)
          foreach(var ison in nlsNode.Children)
            m_Data[ison.Name] = new NDPair(ison.AttrByName("n").Value, ison.AttrByName("d").Value);
        else
          m_Data[nlsNode.Name] = new NDPair(nlsNode.AttrByName("n").Value, nlsNode.AttrByName("d").Value);
      }

      internal Dictionary<string, NDPair> m_Data;

      public NDPair this[string langIso]
      {
        get
        {
          if (m_Data!=null)
          {
            NDPair result;
            if (m_Data.TryGetValue(langIso, out result)) return result;
          }

          return new NDPair();
        }
      }


      public int Count
      {
        get { return m_Data==null ? 0 : m_Data.Count; }
      }


      /// <summary>
      /// Takes entries from this instance and overides them by ISO keys from another instance returning the new instance
      /// </summary>
      public NLSMap OverrideBy(NLSMap other)
      {
        var result = new NLSMap(true);

        foreach (var kvp in this)
          result.m_Data[kvp.Key] = kvp.Value;
        foreach (var kvp in other)
          result.m_Data[kvp.Key] = kvp.Value;

        return result;
      }

      public override string ToString()
      {
        return JSONWriter.Write(this, JSONWritingOptions.Compact);
      }

      public enum GetParts{ Name, Description, NameOrDescription, DescriptionOrName, NameAndDescription, DescriptionAndName}

      /// <summary>
      /// Tries to get the specified part(s) from the map defaulting to another lang if requested lang is not found.
      /// Returns null if nothing is found
      /// </summary>
      public string Get(GetParts tp, string langIso = null, string dfltLangIso = null, string concat = null)
      {
        if (langIso.IsNullOrWhiteSpace()) langIso = CoreConsts.ISO_LANG_ENGLISH;
        if (concat.IsNullOrWhiteSpace()) concat = " - ";

        var p = this[langIso];
        string result = getSwitch(p, tp, concat);
        if (result.IsNotNullOrWhiteSpace()) return result;

        if (dfltLangIso.IsNullOrWhiteSpace()) dfltLangIso = CoreConsts.ISO_LANG_ENGLISH;
        if (langIso.EqualsIgnoreCase(dfltLangIso)) return null;

        p = this[dfltLangIso];
        result = getSwitch(p, tp, concat);
        if (result.IsNotNullOrWhiteSpace()) return result;

        return null;
      }

      /// <summary>
      /// Tries to get the specified part(s) from the JSON content that represents map defaulting to another lang if requested lang is not found.
      /// Returns null if nothing is found
      /// </summary>
      public static bool TryGet(string json, out string result, GetParts tp, string langIso = null, string dfltLangIso = null, string concat = null)
      {
        try
        {
          var nls = new NLSMap(json);
          result = nls.Get(tp, langIso, dfltLangIso, concat);
          return true;
        }
        catch
        {
          result = null;
          return false;
        }
      }


          private string getSwitch(NDPair p, GetParts tp, string concat)
          {
            switch(tp)
            {
              case GetParts.Name:               { return p.Name; }
              case GetParts.Description:        { return p.Description; }
              case GetParts.NameOrDescription:  {
                                                 if (p.Name.IsNotNullOrWhiteSpace()) return p.Name;
                                                 return p.Description;
                                                }
              case GetParts.DescriptionOrName:  {
                                                 if (p.Description.IsNotNullOrWhiteSpace()) return p.Description;
                                                 return p.Name;
                                                }
              case GetParts.NameAndDescription: {
                                                 var isName = p.Name.IsNotNullOrWhiteSpace();
                                                 var isDescr = p.Description.IsNotNullOrWhiteSpace();

                                                 if (isName && isDescr)
                                                  return p.Name+concat+p.Description;

                                                 if (isName) return p.Name;
                                                 if (isDescr) return p.Description;
                                                 return null;
                                                }
              case GetParts.DescriptionAndName: {
                                                 var isName = p.Name.IsNotNullOrWhiteSpace();
                                                 var isDescr = p.Description.IsNotNullOrWhiteSpace();

                                                 if (isName && isDescr)
                                                  return p.Description+concat+p.Name;

                                                 if (isDescr) return p.Description;
                                                 if (isName) return p.Name;
                                                 return null;
                                                }
            }

            return null;
          }


      /// <summary>
      /// Writes NLSMap either as a dict or as a {n:"", d: ""} pair as Options.NLSMapLanguageISO filter dictates
      /// </summary>
      public void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
      {
        if (m_Data==null)
        {
          wri.Write("{}");
          return;
        }

        if (options==null ||
            options.Purpose==JSONSerializationPurpose.Marshalling ||
            options.NLSMapLanguageISO.IsNullOrWhiteSpace())
        {
          JSONWriter.WriteMap(wri, m_Data, nestingLevel, options);
          return;
        }

        var pair = this[options.NLSMapLanguageISO];

        if (!pair.IsAssigned && !options.NLSMapLanguageISODefault.EqualsOrdIgnoreCase(options.NLSMapLanguageISO))
          pair = this[options.NLSMapLanguageISODefault];

        if (pair.IsAssigned)
          JSONWriter.WriteMap(wri, nestingLevel, options, new System.Collections.DictionaryEntry("n", pair.Name),
                                                          new System.Collections.DictionaryEntry("d", pair.Description));
        else
          JSONWriter.WriteMap(wri, nestingLevel, options, new System.Collections.DictionaryEntry("n", null),
                                                          new System.Collections.DictionaryEntry("d", null));
      }

      public IEnumerator<KeyValuePair<string, NLSMap.NDPair>> GetEnumerator()
      {
        return m_Data!=null ? m_Data.GetEnumerator() : Enumerable.Empty<KeyValuePair<string, NLSMap.NDPair>>().GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return m_Data!=null ? m_Data.GetEnumerator() : Enumerable.Empty<KeyValuePair<string, NLSMap.NDPair>>().GetEnumerator();
      }
    }
}
