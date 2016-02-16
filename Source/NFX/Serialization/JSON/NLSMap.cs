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
    [Serializable]
    public class NLSMap : IEnumerable<KeyValuePair<string, NLSMap.NDPair>>, IJSONWritable
    {
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

      //For serializer speed-up
      private NLSMap(){}

      /// <summary>
      /// Makes NLSMap out of conf string: eng{n='Cucumber' d='It is green'} deu{n='Gurke' d='Es ist grün'}
      /// </summary>
      public NLSMap(string nlsConf)
      {
        if (nlsConf.IsNullOrWhiteSpace()) return;
        var nlsNode = nlsConf.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
        ctor(nlsNode);
      }

      /// <summary>
      /// Makes NLSMap out of conf node: eng{n='Cucumber' d='It is green'} deu{n='Gurke' d='Es ist grün'}
      /// </summary>
      public NLSMap(IConfigSectionNode nlsNode)
      {
        if (nlsNode==null || !nlsNode.Exists) return;
        ctor(nlsNode);
      } 

      private void ctor(IConfigSectionNode nlsNode)
      {
        m_Data = new Dictionary<string, NDPair>(StringComparer.InvariantCultureIgnoreCase);
        
        foreach(var ison in nlsNode.Children)
          m_Data[ison.Name] = new NDPair(ison.AttrByName("n").Value, ison.AttrByName("d").Value);
      }


      protected Dictionary<string, NDPair> m_Data;
      
      public NDPair this[string languageIso]
      {
        get 
        {
          NDPair result;
          if (m_Data.TryGetValue(languageIso, out result)) return result;

          return new NDPair();
        }
      }

      /// <summary>
      /// Writes NLSMap either as a dict or as a {n:"", d: ""} pair as Options.NLSMapLanguageISO filter dictates
      /// </summary>
      public void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
      {
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
        return m_Data.GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return m_Data.GetEnumerator();
      }
    }
}
