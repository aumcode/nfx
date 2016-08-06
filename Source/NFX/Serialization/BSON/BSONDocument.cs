using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using NFX.Serialization.JSON;
using System.Collections;
using System.Runtime.CompilerServices;

namespace NFX.Serialization.BSON
{

  /// <summary>
  /// Represents template argument
  /// </summary>
  public struct TemplateArg
  {
    public TemplateArg(string name, BSONElementType btype, object value)
    {
      Name = name;
      BSONType = btype;
      Value = value;
    }

    public readonly string Name;
    public readonly BSONElementType BSONType;
    public readonly object Value;
  }


  /// <summary>
  /// Represents a BSON document
  /// </summary>
  public class BSONDocument : IJSONWritable, IEnumerable<BSONElement>
  {
    #region CONSTS
      public const int IDX_THRESHOLD = 6;
      public const string ARG_TPL_PREFIX = "$$";
    #endregion

    #region .ctor
      /// <summary>
      /// Create an empty document
      /// </summary>
      public BSONDocument()
      {
      }

      /// <summary>
      /// Deserializes BSON document from stream containing BSON-encoded data
      /// </summary>
      public BSONDocument(Stream stream)
      {
        if (stream==null)
           throw new BSONException(StringConsts.ARGUMENT_ERROR+"BSONDocument.ctor(stream==null)");

        long start = stream.Position;
        var totalSize = BinUtils.ReadInt32(stream);
        long read = 4;
        while(read<totalSize-1)
        {
          var et = BinUtils.ReadElementType(stream);
          var factory = BSONElement.GetElementFactory(et);
          var element = factory(stream);//element made
          Set(element);
          read = stream.Position - start;
        }
        var terminator = BinUtils.ReadByte(stream);
        if (terminator != BinUtils.TERMINATOR || stream.Position - start != totalSize)
          throw new BSONException(StringConsts.BSON_EOD_ERROR);
      }


      /// <summary>
      /// Creates an instance of the document from JSON template with parameters populated from args optionally caching the template internal
      /// representation. Do not cache templates that change often
      /// </summary>
      public BSONDocument(string template, bool cacheTemplate, params TemplateArg[] args)
      {
        if (template.IsNullOrWhiteSpace())
           throw new BSONException(StringConsts.ARGUMENT_ERROR+"BSONDocument.ctor(template==null|empty)");

        var map = getTemplateJSONDataMap(template, cacheTemplate);
        buildFromTemplateArgs(this, map, args);
      }
    #endregion

    #region Fields
      private int m_ByteSize;
      private readonly List<BSONElement> m_List = new List<BSONElement>();
      private Dictionary<string, int> m_Dict;
      [ThreadStatic] private static HashSet<BSONDocument> ts_DocGraph;
    #endregion

    #region Properties
      /// <summary>
      /// Returns BSONElement by name or null if not found
      /// </summary>
      public BSONElement this[string name]
      {
        get
        {
          if (name==null)
             throw new BSONException(StringConsts.ARGUMENT_ERROR+"BSONDocument[name: null]");

          var idx = IndexOfName(name);
          return  idx >= 0 ? m_List[idx] : null;
        }
      }

      /// <summary>
      /// Returns BSONElement by index or null if index is out of bounds
      /// </summary>
      public BSONElement this[int idx]
      {
        get
        {
          return (idx<0 || idx>=m_List.Count) ? null : m_List[idx];
        }
      }

      /// <summary>
      /// Returns the number of BSONElements in this document
      /// </summary>
      public int Count { get{ return m_List.Count; } }

      /// <summary>
      /// Recalculates the BSON binary size of this document expressed in bytes
      /// </summary>
      public int ByteSize{ get{ return GetByteSize(true); } }


      /// <summary>
      /// Calculates the BSON binary size of this document expressed in bytes
      /// </summary>
      internal int GetByteSize(bool recalc)
      {
        if (recalc)
        {
          var graph = ts_DocGraph;
          if (graph==null)
          {
            graph = new HashSet<BSONDocument>();
            ts_DocGraph = graph;
          }

          if (!graph.Add(this))
            throw new BSONException(StringConsts.BSON_DOCUMENT_RECURSION_ERROR);

          try
          {
            var total = 4 + //int32 size
                        1;  //zero terminator at the very end of the document

            var cnt = m_List.Count;
            for(var i=0; i<cnt; i++)
              total += m_List[i].GetByteSize(true);

            m_ByteSize = total;
          }
          finally
          {
            graph.Remove(this);
          }
        }

        return m_ByteSize;
      }

    #endregion

    #region Pub Methods
      /// <summary>
      /// Returns index of named BSONElement or -1 if it does not exist
      /// </summary>
      public int IndexOfName(string name)
      {
        if (name==null) return -1;

        if (m_Dict==null) //linear search
        {
          var cnt = m_List.Count;
          for(var i=0; i<cnt; i++)
          {
            var elm = m_List[i];
            if (isSameName(name, elm.Name)) return i;
          }
          return -1;
        }

        //else use index
        int result;
        if (m_Dict.TryGetValue(name, out result)) return result;
        return -1;
      }

      /// <summary>
      /// Inserts BSONElement into document
      /// </summary>
      public BSONDocument Set(BSONElement value, int atIndex = -1)
      {
        bool added;
        this.Set(value, out added, atIndex);
        return this;
      }

      /// <summary>
      /// Inserts BSONElement into document
      /// </summary>
      public BSONDocument Set(BSONElement value, out bool added, int atIndex = -1)
      {
        if (value==null)
           throw new BSONException(StringConsts.ARGUMENT_ERROR+"Set(value==null)");

        if (value.IsArrayElement)
         throw new BSONException(StringConsts.BSON_ARRAY_ELM_DOC_ERROR.Args(value));

        added = setCore(value, atIndex);
        return this;
      }

      /// <summary>
      /// Deletes an element by name returning true if it was found and deleted
      /// </summary>
      public bool Delete(string name)
      {
        if (name==null) return false;

        var idx = IndexOfName(name);
        if (idx<0) return false;

        m_List.RemoveAt(idx);
        if (m_Dict!=null)
          m_Dict.Remove(name);

        return true;
      }

      /// <summary>
      /// Serializes this dosument into a TextWriter - this method is used by JSON serializer(JSONWriter)
      /// </summary>
      public void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
      {
        //this will throw nesting exception in case of cyclical graph
        JSONWriter.WriteMap(wri, this.Select(elm => new DictionaryEntry(elm.Name, elm)), nestingLevel+1, options);
      }


      /// <summary>
      /// Serializes this document into BSON-format stream
      /// </summary>
      public void WriteAsBSON(Stream stream)
      {
        GetByteSize(true);//this will throw in case of cyclical document graph starting from root level
        WriteAsBSONCore(stream);
      }

      internal void WriteAsBSONCore(Stream stream)
      {
        var docSize = this.GetByteSize(false);//gets cached value
        BinUtils.WriteInt32(stream, docSize);
        var cnt = m_List.Count;
        for(var i=0; i<cnt; i++)
        {
           var elm = m_List[i];
           elm.WriteToStream(stream);
        }
        BinUtils.WriteTerminator(stream); //x00
      }

      public IEnumerator<BSONElement> GetEnumerator()
      {
        return m_List.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return m_List.GetEnumerator();
      }


      public override string ToString()
      {
        return this.ToJSON();
      }

    #endregion

    #region .pvt
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      private static bool isSameName(string n1, string n2)
      {
        return string.Equals(n1, n2, StringComparison.Ordinal);
      }

      /// <summary>
      /// Returns true when added, false - replaced
      /// </summary>
      private bool setCore(BSONElement elm, int atIndex)
      {
        var idxExisting = IndexOfName(elm.Name);
        if (idxExisting>=0)
        {
          m_List[idxExisting] = elm;
          return false;
        }

        if (atIndex>=0 && atIndex<m_List.Count)
        {
          m_List.Insert(atIndex, elm);
          //rebuild dict
          var cnt = m_List.Count;
          if (cnt>=IDX_THRESHOLD)
          {
             makeDict();//rebuild dict as indexes have shifted
          }
        }
        else
        {
          m_List.Add(elm);
          var cnt = m_List.Count;
          var idxNew = cnt-1;
          if (cnt>=IDX_THRESHOLD)
          {
             if (m_Dict==null)
               makeDict();
             else
               m_Dict.Add(elm.Name, idxNew);
          }
        }
        return true;
      }

      private void makeDict()
      {
        if (m_Dict==null)
         m_Dict = new Dictionary<string,int>();
        else
         m_Dict.Clear();

        var cnt = m_List.Count;
        for(var i=0; i<cnt; i++)
        {
          var elm = m_List[i];
          m_Dict.Add(elm.Name, i);
        }
      }


              private static readonly object s_TemplateCacheLock = new object();
              private static volatile Dictionary<string, JSONDataMap> s_TemplateCache = new Dictionary<string, JSONDataMap>(StringComparer.Ordinal);

              private JSONDataMap getTemplateJSONDataMap(string template, bool cache)
              {
                JSONDataMap result = null;
                if (cache)
                {
                  if (!s_TemplateCache.TryGetValue(template, out result))
                    lock(s_TemplateCacheLock)
                    {
                      if (!s_TemplateCache.TryGetValue(template, out result))
                      {
                        result = compileTemplate(template);
                        var ncache = new Dictionary<string, JSONDataMap>(s_TemplateCache, StringComparer.Ordinal);
                        ncache[template] = result;
                        s_TemplateCache = ncache;//atomic
                      }
                    }
                }
                else
                 result = compileTemplate(template);

                return result;
              }

              private JSONDataMap compileTemplate(string template)
              {
                JSONDataMap result;
                try
                {
                  result = JSONReader.DeserializeDataObject( template ) as JSONDataMap;
                  if (result==null) throw new BSONException("template is not JSONDataMap");
                }
                catch(Exception error)
                {
                  throw new BSONException(StringConsts.BSON_TEMPLATE_COMPILE_ERROR.Args(error.ToMessageWithType(), template), error);
                }

                return result;
              }

              private void buildFromTemplateArgs(BSONDocument root, JSONDataMap template, TemplateArg[] args)
              {
                foreach(var kvp in template)
                  root.Set( jToB(kvp.Key, kvp.Value, args) );
              }

              private BSONElement jToB(string name, object value, TemplateArg[] args)
              {
                if (name!=null)//array elm may be null
                  name = substName(name, args);

                var bType = BSONElementType.Null;
                if (value!=null)
                {
                    if (value is string)
                    {
                     var sv = (string)value;
                     if (sv.Length>ARG_TPL_PREFIX.Length && sv.StartsWith(ARG_TPL_PREFIX))//parameter name
                     {
                      sv = sv.Substring(ARG_TPL_PREFIX.Length);//get rid of "$$"
                      var arg = args.FirstOrDefault(a=> a.Name.EqualsOrdIgnoreCase(sv));
                      if (arg.Name!=null)
                      {
                        bType = arg.BSONType;
                        value = arg.Value;
                      }
                      else
                       throw new BSONException("Template JSON parameter '{0}' not found in args".Args(sv));
                     }
                     else
                      bType = BSONElementType.String;
                    }
                    else if (value is UInt64) bType = BSONElementType.Int64;
                    else if (value is Int32) bType = BSONElementType.Int32;
                    else if (value is Int64) bType = BSONElementType.Int64;
                    else if (value is bool) bType = BSONElementType.Boolean;
                    else if (value is double || value is float || value is decimal) bType = BSONElementType.Double;
                    else if (value is JSONDataMap)
                    {
                        var doc = value as JSONDataMap;
                        var subdoc = new BSONDocument();
                        buildFromTemplateArgs(subdoc, doc, args);
                        value = subdoc;
                        bType = BSONElementType.Document;
                    }
                    else if (value is JSONDataArray)
                    {
                        var arr = value as JSONDataArray;
                        value = arr.Select(e => jToB(null, e, args)).ToArray();
                        bType = BSONElementType.Array;
                    }
                    else throw new BSONException("Template JSON value type '{0}' not supported in BSON binding".Args(value.GetType()));
                }

                return BSONElement.MakeOfType(bType, name, value);
              }

              private string substName(string name, TemplateArg[] args)
              {
                if (name.Length>ARG_TPL_PREFIX.Length && name.StartsWith(ARG_TPL_PREFIX))//parameter name
                {
                  var aname = name.Substring(ARG_TPL_PREFIX.Length);//get rid of "$$"
                  var arg = args.FirstOrDefault(a => a.Name.EqualsOrdIgnoreCase(aname));
                  if (arg.Name!=null)
                   return arg.Value!=null ? arg.Value.ToString() : "";
                }

                return name;
              }


    #endregion

  }
}
