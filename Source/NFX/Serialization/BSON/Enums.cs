using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Serialization.BSON
{

  /// <summary>
  ///  BSON element types: http://bsonspec.org/spec.html
  /// </summary>
  public enum BSONElementType
  {
    Double = 0x01,
    String = 0x02,
    Document = 0x03,
    Array = 0x04,
    Binary = 0x05,
    ObjectID = 0x07,
    Boolean = 0x08,
    DateTime = 0x09,
    Null = 0x0a,
    RegularExpression = 0x0b,
    JavaScript = 0x0d,
    JavaScriptWithScope = 0x0f,
    Int32 = 0x10,
    TimeStamp = 0x11,
    Int64 = 0x12,
    MinKey = 0xff,
    MaxKey = 0x7f
  }

  public enum BSONBinaryType
  {
    GenericBinary = 0x00,  //Generic binary subtype
    Function = 0x01,  //Function
    BinaryOld = 0x02, //Binary (Old)
    UUIDOld = 0x03,  //UUID (Old)
    UUID = 0x04,  //UUID
    MD5 = 0x05,  //MD5
    UserDefined = 0x80  //User defined
  }

  [Flags]
  public enum BSONRegularExpressionOptions
  {
     None = 0,

     /// <summary>Insensitive matching</summary>
     I = 0x000001,

     /// <summary>Multiline matching</summary>
     M = 0x000010,

     /// <summary>Verbose mode</summary>
     X = 0x000100,

     /// <summary>Make \w, \W, etc. locale dependent</summary>
     L = 0x001000,

     /// <summary>Dotall mode ('.' matches everything)</summary>
     S = 0x010000,

     /// <summary>Make \w, \W, etc. match unicode</summary>
     U = 0x100000
  }

  public enum BSONBoolean
  {
    True = 0x01,
    False = 0x00
  }

  /// <summary>
  /// Utilities methods for BSON regular expression type
  /// </summary>
  public static class RegularExpressionExtensions
  {
    private static readonly BSONRegularExpressionOptions[] _values = new[]
    {
      BSONRegularExpressionOptions.I,
      BSONRegularExpressionOptions.M,
      BSONRegularExpressionOptions.X,
      BSONRegularExpressionOptions.L,
      BSONRegularExpressionOptions.S,
      BSONRegularExpressionOptions.U
    };

    /// <summary>
    /// Returns BSON-formatted string for regeax options
    /// (The options are identified by lower-case characters, which must be stored in alphabetical order).
    /// </summary>
    public static string ToBSONString(this BSONRegularExpressionOptions options)
    {
      string result = string.Empty;
      for (int i = 0; i < _values.Length; i++)
      {
        if ((options & _values[i]) != 0 )
        result += _values[i];
      }

      return result.ToLower();
    }

    public static BSONRegularExpressionOptions ToBSONOptions(this string source)
    {
      var result = BSONRegularExpressionOptions.None;
      for (int i=0; i<source.Length; i++)
      {
        BSONRegularExpressionOptions value;
        if(Enum.TryParse(source[i].ToString(), true, out value))
          result |= value;
      }
      return result;
    }

    /// <summary>
    /// Returns regex options count
    /// </summary>
    public static byte Count(this BSONRegularExpressionOptions options)
    {
      byte result = 0;
      for (int i = 0; i < _values.Length; i++)
      {
        if ((options & _values[i]) != 0 )
        result += 1;
      }

      return result;
    }
  }

}
