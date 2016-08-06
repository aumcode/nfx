using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using NFX.Serialization.JSON;

namespace NFX.DataAccess.CRUD
{
  /// <summary>
  /// Facilitates mapping of CRUD/CLR types/values to JSON and back to CLR/CRUD
  /// </summary>
  public static class JSONMappings
  {
    public const string JTP_STRING = "string";
    public const string JTP_ARRAY = "array";
    public const string JTP_MAP = "map";
    public const string JTP_OBJECT = "object";

    private static readonly Dictionary<Type, string> s_CLR = new Dictionary<Type, string>()
    {
       {typeof(char), JTP_STRING},
       {typeof(bool), "bool"},
       {typeof(byte), "byte"},{typeof(sbyte), "sbyte"},
       {typeof(short), "short"},{typeof(ushort), "ushort"},
       {typeof(int), "int"},{typeof(uint), "uint"},
       {typeof(long), "long"},{typeof(ulong), "ulong"},

       {typeof(float), "float"},
       {typeof(double), "double"},
       {typeof(decimal), "decimal"},
       {typeof(DateTime), "datetime"}
    };


    private static readonly Dictionary<string, Type> s_JSON = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase)
    {
      {JTP_ARRAY, typeof(List<object>)},
      {JTP_MAP, typeof(Dictionary<string, object>)},

      {JTP_STRING, typeof(string)}, {"str", typeof(string)}, {"char", typeof(string)}, {"text", typeof(string)},

      {"bool", typeof(bool)}, {"boolean", typeof(bool)}, {"logical", typeof(bool)}, {"logic", typeof(bool)},

      {"sbyte", typeof(int)}, {"short", typeof(int)}, {"int16", typeof(int)}, {"int", typeof(int)}, {"integer", typeof(int)}, {"int32", typeof(int)},

      {"byte", typeof(uint)}, {"ushort", typeof(uint)}, {"uint16", typeof(uint)}, {"uint", typeof(uint)}, {"uinteger", typeof(uint)}, {"uint32", typeof(uint)},

      {"long", typeof(long)}, {"int64", typeof(long)},

      {"ulong", typeof(ulong)}, {"uint64", typeof(ulong)},

      {"float", typeof(double)}, {"single", typeof(double)}, {"real", typeof(double)}, {"double", typeof(double)}, {"number", typeof(double)}, {"numeric", typeof(double)},

      {"dec", typeof(decimal)}, {"decimal", typeof(decimal)}, {"money", typeof(decimal)}, {"fixed", typeof(decimal)},

      {"date", typeof(DateTime)}, {"datetime", typeof(DateTime)}, {"time", typeof(DateTime)}, {"timestamp", typeof(DateTime)}
    };


    public static string MapCLRTypeToJSON(Type type, out bool isNullable)
    {
      isNullable = false;
      if (type==null) return JTP_OBJECT;

      if (typeof(string).IsAssignableFrom(type))
      {
        return JTP_STRING;
      }

      if (typeof(Row).IsAssignableFrom(type))
      {
        return JTP_OBJECT;
      }

      if (typeof(IDictionary).IsAssignableFrom(type))
      {
        return JTP_MAP;
      }

      if (typeof(IEnumerable).IsAssignableFrom(type))
      {
        return JTP_ARRAY;
      }

      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
      {
         isNullable = true;
         type = type.GetGenericArguments()[0];
      }

      //dictionary lookup
      string name;
      if (!s_CLR.TryGetValue(type, out name))
        name = JTP_OBJECT;

      return name;
    }


    public static Type MapJSONTypeToCLR(string type, bool isNullable)
    {
      if (type.IsNullOrWhiteSpace()) return typeof(object);

      Type t;
      if (s_JSON.TryGetValue(type, out t))
      {
        if (t.IsValueType && isNullable)
         return typeof(Nullable<>).MakeGenericType(t);

        return t;
      }

      return typeof(object);
    }


  }
}
