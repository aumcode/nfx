using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFX.DataAccess.Distributed;
using NFX.Serialization.BSON;
using NUnit.Framework;

namespace NFX.NUnit.Serialization
{
  /// <summary>
  /// See BSON spec http://bsonspec.org/spec.html
  /// </summary>
  [TestFixture]
  public class BSON
  {
    #region Serialization

    /// <summary>
    /// {} (empty document)
    /// </summary>
    [TestCase]
    public void WriteEmptyDocument()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 5); // ensure document length is 5 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(sizeof(int)), BitConverter.GetBytes(5)); // ensure content length is 1
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00

        Assert.AreEqual(stream.Position, 5); // ensure whole document readed
      }
    }

    /// <summary>
    /// { pi: 3.14159265358979 }
    /// </summary>
    [TestCase]
    public void WriteSingleDouble()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONDoubleElement("pi", Math.PI));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 17); // ensure document length is 17 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(17)); // ensure content length is 17
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Double); // ensure element type is double 0x01
        CollectionAssert.AreEqual(reader.ReadBytes(2), Encoding.UTF8.GetBytes("pi")); // ensure element name is 'pi'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(8), BitConverter.GetBytes(Math.PI)); // ensure element value is Math.PI
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00
         
        Assert.AreEqual(stream.Position, 17); // ensure whole document readed
      }
    }

    /// <summary>
    /// { greetings: "Hello World!" }
    /// </summary>
    [TestCase]
    public void WriteSingleString()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONStringElement("greetings", "Hello World!"));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 33); // ensure document length is 33 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(sizeof(int)), BitConverter.GetBytes(33)); // ensure content length is 33
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.String); // ensure element type is string 0x02
        CollectionAssert.AreEqual(reader.ReadBytes(9), Encoding.UTF8.GetBytes("greetings")); // ensure element name is 'greetings'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(sizeof(int)), BitConverter.GetBytes(13)); // ensure string content length is 13
        CollectionAssert.AreEqual(reader.ReadBytes(12), Encoding.UTF8.GetBytes("Hello World!")); // ensure element value is 'Hello World!'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure string value terminator 0x00 is present
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00
         
        Assert.AreEqual(stream.Position, 33); // ensure whole document readed
      }
    }

    /// <summary>
    /// { lucky: 7 }
    /// </summary>
    [TestCase]
    public void WriteSingleInt32()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONInt32Element("lucky", 7));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 16); // ensure document length is 16 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(16)); // ensure content length is 16
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int32); // ensure element type is int32 0x10
        CollectionAssert.AreEqual(reader.ReadBytes(5), Encoding.UTF8.GetBytes("lucky")); // ensure element name is 'lucky'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(7)); // ensure element value is 7
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00 
         
        Assert.AreEqual(stream.Position, 16); // ensure whole document readed
      }
    }

    /// <summary>
    /// { solarSystemDiameter: 10000000000000 }
    /// </summary>
    [TestCase]
    public void WriteSingleInt64()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONInt64Element("solarSystemDiameter", 10000000000000));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 34); // ensure document length is 34 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(34)); // ensure content length is 34
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int64); // ensure element type is int64 0x12
        CollectionAssert.AreEqual(reader.ReadBytes(19), Encoding.UTF8.GetBytes("solarSystemDiameter")); // ensure element name is 'solarSystemDiameter'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(8), BitConverter.GetBytes(10000000000000)); // ensure element value is 10000000000000
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00
         
        Assert.AreEqual(stream.Position, 34); // ensure whole document readed
      }
    }

    /// <summary>
    /// Array of strings
    /// { 'fruits': ['apple, 'orange', 'plum'] } --> { 'fruits': { '0': 'apple', '1': 'orange', '2': 'plum' } }
    /// </summary>
    [TestCase]
    public void WriteStringArray()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        var array = new[] { new BSONStringElement("apple"), new BSONStringElement("orange"), new BSONStringElement("plum") };
        root.Set(new BSONArrayElement("fruits", array));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 57); // ensure document length is 57 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(57));        // document's content length is 57
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Array);                  // element type is array 0x04
        CollectionAssert.AreEqual(reader.ReadBytes(6), Encoding.UTF8.GetBytes("fruits")); // element name is 'fruits'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                   // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(44));        // array's content length is 44

        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.String);               // element type is string 0x02
        CollectionAssert.AreEqual(reader.ReadBytes(1), Encoding.UTF8.GetBytes("0"));    // element name is '0'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                 // last byte is terminator 0x00
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(6));       // string content length is 6
        CollectionAssert.AreEqual(reader.ReadBytes(5),Encoding.UTF8.GetBytes("apple")); // string content length is 'apple'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                 // last byte is terminator 0x00

        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.String);                // element type is string 0x02
        CollectionAssert.AreEqual(reader.ReadBytes(1), Encoding.UTF8.GetBytes("1"));     // element name is '1'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                  // last byte is terminator 0x00
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(7));        // string content length is 7
        CollectionAssert.AreEqual(reader.ReadBytes(6),Encoding.UTF8.GetBytes("orange")); // string content length is 'orange'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                  // last byte is terminator 0x00
         
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.String);                // element type is string 0x02 
        CollectionAssert.AreEqual(reader.ReadBytes(1), Encoding.UTF8.GetBytes("2"));     // element name is '2'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                  // last byte is terminator 0x00
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(5));        // string content length is 5
        CollectionAssert.AreEqual(reader.ReadBytes(4),Encoding.UTF8.GetBytes("plum"));   // string content length is 'plum'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                  // last byte is terminator 0x00

        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00

        Assert.AreEqual(stream.Position, 57); // ensure whole document readed
      }
    }

    /// <summary>
    /// Array of int32
    /// { 'years': [1963, 1984, 2015] } --> { 'years': { '0': 1963, '1': 1984, '2': 2015 } }
    /// </summary>
    [TestCase]
    public void WriteInt32Array()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        var array = new[] { new BSONInt32Element(1963), new BSONInt32Element(1984), new BSONInt32Element(2015) };
        root.Set(new BSONArrayElement("years", array));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 38); // ensure document length is 38 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(38));       // document's content length is 38
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Array);                 // element type is array 0x04
        CollectionAssert.AreEqual(reader.ReadBytes(5), Encoding.UTF8.GetBytes("years")); // element name is 'years'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                  // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(26));       // array's content length is 26

        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int32);             // element type is int32 0x10
        CollectionAssert.AreEqual(reader.ReadBytes(1), Encoding.UTF8.GetBytes("0")); // element name is '0'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                              // last byte is terminator 0x00
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(1963)); // value is 1963

        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int32);             // element type is int32 0x10
        CollectionAssert.AreEqual(reader.ReadBytes(1), Encoding.UTF8.GetBytes("1")); // element name is '1'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                              // last byte is terminator 0x00
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(1984)); // value is 1984

        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int32);             // element type is int32 0x10 
        CollectionAssert.AreEqual(reader.ReadBytes(1), Encoding.UTF8.GetBytes("2")); // element name is '2'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                              // last byte is terminator 0x00
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(2015)); // value is 2015

        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00
         
        Assert.AreEqual(stream.Position, 38); // ensure whole document readed
      }
    }

    /// <summary>
    /// Array of strings
    /// { 'stuff': ['apple, 3, 2.14] } --> { 'stuff': { '0': 'apple', '1': 3, '2': 2.14 } }
    /// </summary>
    [TestCase]
    public void WriteStringInt32DoubleMixedArray()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        var array = new BSONElement[] { new BSONStringElement("apple"), new BSONInt32Element(3), new BSONDoubleElement(2.14D) };
        root.Set(new BSONArrayElement("stuff", array));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 48); // ensure document length is 48 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(48));       // document's content length is 48
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Array);                 // element type is array 0x04
        CollectionAssert.AreEqual(reader.ReadBytes(5), Encoding.UTF8.GetBytes("stuff")); // element name is 'stuff'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                  // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(36));       // array's content length is 36

        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.String);               // element type is string 0x02
        CollectionAssert.AreEqual(reader.ReadBytes(1), Encoding.UTF8.GetBytes("0"));    // element name is '0'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                 // last byte is terminator 0x00
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(6));       // string content length is 6
        CollectionAssert.AreEqual(reader.ReadBytes(5),Encoding.UTF8.GetBytes("apple")); // string content length is 'apple'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                 // last byte is terminator 0x00

        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int32);             // element type is int32 0x10
        CollectionAssert.AreEqual(reader.ReadBytes(1), Encoding.UTF8.GetBytes("1")); // element name is '1'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                              // last byte is terminator 0x00
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(3));    // value is 3

        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Double);             // element type is double 0x01
        CollectionAssert.AreEqual(reader.ReadBytes(1), Encoding.UTF8.GetBytes("2")); // element name is '2'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                              // last byte is terminator 0x00
        CollectionAssert.AreEqual(reader.ReadBytes(8), BitConverter.GetBytes(2.14D)); // value is 2.14

        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00
         
        Assert.AreEqual(stream.Position, 48); // ensure whole document readed
      }
    }

    /// <summary>
    /// { name: "Gagarin", birth: 1934 }
    /// </summary>
    [TestCase]
    public void WriteStringAndInt32Pair()
    { 
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONStringElement("name", "Gagarin"));
        root.Set(new BSONInt32Element("birth", 1934));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 34); // ensure document length is 38 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(sizeof(int)), BitConverter.GetBytes(34)); // ensure content length is 34
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.String); // ensure element type is string 0x02
        CollectionAssert.AreEqual(reader.ReadBytes(4), Encoding.UTF8.GetBytes("name")); // ensure element name is 'name'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(sizeof(int)), BitConverter.GetBytes(8)); // ensure string content length is 8
        CollectionAssert.AreEqual(reader.ReadBytes(7), Encoding.UTF8.GetBytes("Gagarin")); // ensure element value is 'Gagarin'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure string value terminator 0x00 is present

        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int32); // ensure element type is int 0x10
        CollectionAssert.AreEqual(reader.ReadBytes(5), Encoding.UTF8.GetBytes("birth")); // ensure element name is 'birth'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(1934)); // ensure element value is int 1934
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00
         
        Assert.AreEqual(stream.Position, 34); // ensure whole document readed
      }
    }

    /// <summary>
    /// { nested: { capital: "Moscow" } }
    /// </summary>
    [TestCase]
    public void WriteNestedDocument()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        var nested = new BSONDocument();
        nested.Set(new BSONStringElement("capital", "Moscow"));
        root.Set(new BSONDocumentElement("nested", nested));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 38); // ensure document length is 38 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(sizeof(int)), BitConverter.GetBytes(38)); // content length is 38
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Document);                  // element type is document 0x03
        CollectionAssert.AreEqual(reader.ReadBytes(6), Encoding.UTF8.GetBytes("nested"));    // element name is 'nested'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                      // string name terminator 0x00 is present

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(25));           // nested document length is 25
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.String);                    // element type is string 0x02
        CollectionAssert.AreEqual(reader.ReadBytes(7), Encoding.UTF8.GetBytes("capital"));   // element name is 'capital'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                      // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(7));            // string length is 7
        CollectionAssert.AreEqual(reader.ReadBytes(6), Encoding.UTF8.GetBytes("Moscow"));    // element value is 'Moscow'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // last byte is terminator 0x00
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // last byte is terminator 0x00

        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // last byte is terminator 0x00
         
        Assert.AreEqual(stream.Position, 38); // ensure whole document readed
      }
    }

    /// <summary>
    ///  { binary: <bytes from 'This is binary data'> }
    /// </summary>
    [TestCase]
    public void WriteSingleBinaryData()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var data = Encoding.UTF8.GetBytes("This is binary data");
        var root = new BSONDocument();
        var binary = new BSONBinary(BSONBinaryType.BinaryOld, data);
        root.Set(new BSONBinaryElement("binary", binary));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 37); // ensure document length is 37 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(37));        // content length is 37
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Binary);                 // element type is binary 0x05
        CollectionAssert.AreEqual(reader.ReadBytes(6), Encoding.UTF8.GetBytes("binary")); // element name is 'binary'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                   // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(19));        // byte length is 19
        Assert.AreEqual(reader.ReadByte(), (byte)BSONBinaryType.BinaryOld);               // binary type is BinaryOld 0x02
        CollectionAssert.AreEqual(reader.ReadBytes(19), data);                            // byte content is correct

        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00
         
        Assert.AreEqual(stream.Position, 37); // ensure whole document readed
      }
    } 
    
    /// <summary>
    ///  { objectId: <bytes from hex '507f1f77bcf86cd799439011'> }
    /// </summary>
    [TestCase]
    public void WriteSingleObjectId()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var hex = "507f1f77bcf86cd799439011";
        var data = Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray();
        var root = new BSONDocument();
        var objectId = new BSONObjectID(data);
        root.Set(new BSONObjectIDElement("objectId", objectId));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 27); // ensure document length is 27 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(27));          // content length is 27
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.ObjectID);                 // element type is objectID 0x07
        CollectionAssert.AreEqual(reader.ReadBytes(8), Encoding.UTF8.GetBytes("objectId")); // element name is 'objectId'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                     // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(12), data);                              // byte content is correct

        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00 
         
        Assert.AreEqual(stream.Position, 27); // ensure whole document readed
      }
    }
    
    /// <summary>
    ///  { booleanTrue: true }
    /// </summary>
    [TestCase]
    public void WriteSingleBooleanTrue()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONBooleanElement("booleanTrue", true));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 19); // ensure document length is 19 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(19));              // content length is 19
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Boolean);                      // element type is boolean 0x08
        CollectionAssert.AreEqual(reader.ReadBytes(11), Encoding.UTF8.GetBytes("booleanTrue")); // element name is 'booleanTrue'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                         // string name terminator 0x00 is present
        Assert.AreEqual(reader.ReadByte(), (byte)BSONBoolean.True);                             // byte content is correct

        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00 
         
        Assert.AreEqual(stream.Position, 19); // ensure whole document readed
      }
    }
    
    /// <summary>
    ///  { booleanFalse: false }
    /// </summary>
    [TestCase]
    public void WriteSingleBooleanFalse()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONBooleanElement("booleanFalse", false));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 20); // ensure document length is 20 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(20));               // content length is 20
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Boolean);                       // element type is boolean 0x08
        CollectionAssert.AreEqual(reader.ReadBytes(12), Encoding.UTF8.GetBytes("booleanFalse")); // element name is 'booleanFalse'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                          // string name terminator 0x00 is present
        Assert.AreEqual(reader.ReadByte(), (byte)BSONBoolean.False);                             // byte content is correct

        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00
         
        Assert.AreEqual(stream.Position, 20); // ensure whole document readed
      }
    }
    
    /// <summary>
    /// { null: null }
    /// </summary>
    [TestCase]
    public void WriteSingleNull()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONNullElement("null"));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 11); // ensure document length is 11 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(11));      // content length is 11
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Null);                 // element type is null 0x0a
        CollectionAssert.AreEqual(reader.ReadBytes(4), Encoding.UTF8.GetBytes("null")); // element name is 'null'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                 // string name terminator 0x00 is present
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                 // last byte is terminator 0x00

        Assert.AreEqual(stream.Position, 11); // ensure whole document readed
      }
    }

    /// <summary>
    /// { now: <DateTime from 635000000000000000 ticks> }  ({"now":{"$date":"2013-03-27T13:53:20.000Z"}})
    /// </summary>
    [TestCase]
    public void WriteSingleDateTime()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var now = new DateTime(635000000000000000).ToUniversalTime();
        var root = new BSONDocument();
        root.Set(new BSONDateTimeElement("now", now));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 18); // ensure document length is 18 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(18));     // ensure content length is 18
        Assert.AreEqual(reader.ReadByte(), (byte) BSONElementType.DateTime);           // ensure element type is DateTime 0x09
        CollectionAssert.AreEqual(reader.ReadBytes(3), Encoding.UTF8.GetBytes("now")); // ensure element name is 'now'
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // ensure string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(8), BitConverter.GetBytes(now.ToMillisecondsSinceUnixEpochStart())); // ensure element value is correct
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // ensure last byte is terminator 0x00
                                              
        Assert.AreEqual(stream.Position, 18); // ensure whole document readed
      }
    }


                [TestCase]
                public void WriteAndReadSingleDateTime()
                {
                  using (var stream = new MemoryStream())
                  {
                    var now = new DateTime(2010, 10, 12,  11, 20, 12, DateTimeKind.Local);
                    var root = new BSONDocument();
                    root.Set(new BSONDateTimeElement("mydate", now));
                    root.WriteAsBSON(stream);

                    Assert.AreEqual(stream.Position, 21); // ensure document length is 21 bytes

                    stream.Seek(0, SeekOrigin.Begin);

                    var root2 = new BSONDocument(stream);
                    Assert.AreEqual(stream.Position, 21); // ensure whole document read

                    Assert.AreEqual(1, root2.Count); // ensure whole document read
                    Assert.AreEqual(now.ToUniversalTime(), ((BSONDateTimeElement)root2["mydate"]).Value);
                  }
                }



    /// <summary>
    /// { email: <pattern='^[-.\w]+@(?:[a-z\d]{2,}\.)+[a-z]{2,6}$' options=I,M,U> }
    /// </summary>
    [TestCase]
    public void WriteSingleRegularExpression()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        var pattern = @"^[-.\w]+@(?:[a-z\d]{2,}\.)+[a-z]{2,6}$";
        var options = BSONRegularExpressionOptions.I | BSONRegularExpressionOptions.M |BSONRegularExpressionOptions.U;
        var regex = new BSONRegularExpression(pattern, options);
        root.Set(new BSONRegularExpressionElement("email", regex));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 55); // ensure document length is 55 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(55));       // ensure content length is 55
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.RegularExpression);     // ensure element type is RegularExpression 0x0b
        CollectionAssert.AreEqual(reader.ReadBytes(5), Encoding.UTF8.GetBytes("email")); // ensure element name is 'email'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                  // ensure string name terminator 0x00 is present

        CollectionAssert.AreEqual(reader.ReadBytes(38), Encoding.UTF8.GetBytes(pattern)); // ensure element value is pattern
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                   // ensure string value terminator 0x00 is present

        CollectionAssert.AreEqual(reader.ReadBytes(3), Encoding.UTF8.GetBytes("imu")); // ensure element value is options in BSON format
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                // ensure string value terminator 0x00 is present

        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00
         
        Assert.AreEqual(stream.Position, 55); // ensure whole document readed
      }
    }   

    /// <summary>
    /// { code: "function(){var x=1;var y='abc';return 1;};" }
    /// </summary>
    [TestCase]
    public void WriteSingleJavaScript()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        var code = "function(){var x=1;var y='abc';return 1;};";
        root.Set(new BSONJavaScriptElement("code", code));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 58); // ensure document length is 58 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(58));      // content length is 58
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.JavaScript);           // element type is JavaScript 0x0d
        CollectionAssert.AreEqual(reader.ReadBytes(4), Encoding.UTF8.GetBytes("code")); // element name is 'code'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                 // string name terminator 0x00 is present

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(43));     // js code content length is 43
        CollectionAssert.AreEqual(reader.ReadBytes(42), Encoding.UTF8.GetBytes(code)); // element value is code
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                // string value terminator 0x00 is present
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                // last byte is terminator 0x00
         
        Assert.AreEqual(stream.Position, 58); // ensure whole document readed
      }
    }

    /// <summary>
    /// { codeWithScope: "function(){var x=1;var y='abc';return z;}; <with scope: z=23>" }
    /// </summary>
    [TestCase]
    public void WriteSingleJavaScriptWithScope()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        var code = "function(){var x=1;var y='abc';return z;};";
        var scope = new BSONDocument();
        scope.Set(new BSONInt32Element("z", 23));
        var jsWithScope = new BSONCodeWithScope(code, scope);
        root.Set(new BSONJavaScriptWithScopeElement("codeWithScope", jsWithScope));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 83); // ensure document length is 83 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(83));       // content length is 83
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.JavaScriptWithScope);   // element type is JavaScriptWithScope 0x0f
        CollectionAssert.AreEqual(reader.ReadBytes(13), Encoding.UTF8.GetBytes("codeWithScope")); // element name is 'codeWithScope'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                  // string name terminator 0x00 is present

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(63));     // full content length is 63
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(43));     // content length is 43
        CollectionAssert.AreEqual(reader.ReadBytes(42), Encoding.UTF8.GetBytes(code)); // element value is code
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                // string value terminator 0x00 is present

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(12));   // full scope content length is 12
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int32);             // element type is int 0x10
        CollectionAssert.AreEqual(reader.ReadBytes(1), Encoding.UTF8.GetBytes("z")); // element name is 'z'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                              // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(23));   // z variable value is 23

        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // last byte is terminator 0x00
        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // last byte is terminator 0x00
         
        Assert.AreEqual(stream.Position, 83); // ensure whole document readed
      }
    } 

    /// <summary>
    /// { stamp: <seconds since Unix epoch to DateTime from 635000000000000000 ticks with 123 increment> }
    /// </summary>
    [TestCase]
    public void WriteSingleTimestamp()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        var now = new DateTime(635000000000000000).ToUniversalTime();
        var stamp = new BSONTimestamp(now, 123);
        root.Set(new BSONTimestampElement("stamp", stamp));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 20); // ensure document length is 20 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(20));       // content length is 20
        Assert.AreEqual(reader.ReadByte(), (byte) BSONElementType.TimeStamp);            // element type is TimeStamp 0x11
        CollectionAssert.AreEqual(reader.ReadBytes(5), Encoding.UTF8.GetBytes("stamp")); // element name is 'now'
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                                 // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(123));      // increment is correct
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes((int)now.ToSecondsSinceUnixEpochStart())); // datetime is correct
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                                 // last byte is terminator 0x00
         
        Assert.AreEqual(stream.Position, 20); // ensure whole document readed
      }
    } 
    
    /// <summary>
    /// { minkey: <minkey> }
    /// </summary>
    [TestCase]
    public void WriteSingleMinKey()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONMinKeyElement("minkey"));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 13); // ensure document length is 13 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(13));        // ensure content length is 13
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.MinKey);                 // ensure element type is MinKey 0xff
        CollectionAssert.AreEqual(reader.ReadBytes(6), Encoding.UTF8.GetBytes("minkey")); // ensure element name is 'minkey'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                   // ensure string name terminator 0x00 is present
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                   // ensure last byte is terminator 0x00
         
        Assert.AreEqual(stream.Position, 13); // ensure whole document readed
      }
    } 
    
    /// <summary>
    /// { maxkey: <maxkey> }
    /// </summary>
    [TestCase]
    public void WriteSingleMaxKey()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONMaxKeyElement("maxkey"));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 13); // ensure document length is 13 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(13));        // ensure content length is 13
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.MaxKey);                 // ensure element type is MaxKey 0x7f
        CollectionAssert.AreEqual(reader.ReadBytes(6), Encoding.UTF8.GetBytes("maxkey")); // ensure element name is 'maxkey'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                   // ensure string name terminator 0x00 is present
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                   // ensure last byte is terminator 0x00
         
        Assert.AreEqual(stream.Position, 13); // ensure whole document readed
      }
    }

    /// <summary>
    /// { 
    ///   eng: "hello", 
    ///   rus: "привет", 
    ///   chi: "你好", 
    ///   jap: "こんにちは", 
    ///   gre: "γεια σας", 
    ///   alb: "përshëndetje",
    ///   arm: "բարեւ Ձեզ",
    ///   vie: "xin chào",
    ///   por: "Olá",
    ///   ukr: "Привіт",
    ///   ger: "wünsche"
    /// }
    /// </summary>
    [TestCase]
    public void WriteUnicodeStrings()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONStringElement("eng", "hello"));
        root.Set(new BSONStringElement("rus", "привет"));
        root.Set(new BSONStringElement("chi", "你好"));
        root.Set(new BSONStringElement("jap", "こんにちは"));
        root.Set(new BSONStringElement("gre", "γεια σας"));
        root.Set(new BSONStringElement("alb", "përshëndetje"));
        root.Set(new BSONStringElement("arm", "բարեւ Ձեզ"));
        root.Set(new BSONStringElement("vie", "xin chào"));
        root.Set(new BSONStringElement("por", "Olá"));
        root.Set(new BSONStringElement("ukr", "Привіт"));
        root.Set(new BSONStringElement("ger", "wünsche"));
        root.WriteAsBSON(stream);
        Assert.AreEqual(stream.Position, 232); // ensure document length is 33 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(232));    // content length is 232

        Assert.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        CollectionAssert.AreEqual(reader.ReadBytes(3), Encoding.UTF8.GetBytes("eng")); // element name is 'eng'
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(6));      // string content length is 6
        CollectionAssert.AreEqual(reader.ReadBytes(5), Encoding.UTF8.GetBytes("hello"));
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present

        Assert.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        CollectionAssert.AreEqual(reader.ReadBytes(3), Encoding.UTF8.GetBytes("rus")); // element name is 'rus'
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(13));     // string content length is 13
        CollectionAssert.AreEqual(reader.ReadBytes(12), Encoding.UTF8.GetBytes("привет"));
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present
        
        Assert.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        CollectionAssert.AreEqual(reader.ReadBytes(3), Encoding.UTF8.GetBytes("chi")); // element name is 'chi'
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(7));      // string content length is 7
        CollectionAssert.AreEqual(reader.ReadBytes(6), Encoding.UTF8.GetBytes("你好"));
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present
         
        Assert.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        CollectionAssert.AreEqual(reader.ReadBytes(3), Encoding.UTF8.GetBytes("jap")); // element name is 'jap'
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(16));     // string content length is 16
        CollectionAssert.AreEqual(reader.ReadBytes(15), Encoding.UTF8.GetBytes("こんにちは"));
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present
           
        Assert.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        CollectionAssert.AreEqual(reader.ReadBytes(3), Encoding.UTF8.GetBytes("gre")); // element name is 'gre'
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(16));     // string content length is 16
        CollectionAssert.AreEqual(reader.ReadBytes(15), Encoding.UTF8.GetBytes("γεια σας"));
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present
         
        Assert.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        CollectionAssert.AreEqual(reader.ReadBytes(3), Encoding.UTF8.GetBytes("alb")); // element name is 'alb'
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(15));     // string content length is 15
        CollectionAssert.AreEqual(reader.ReadBytes(14), Encoding.UTF8.GetBytes("përshëndetje"));
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present
          
        Assert.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        CollectionAssert.AreEqual(reader.ReadBytes(3), Encoding.UTF8.GetBytes("arm")); // element name is 'arm'
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(18));     // string content length is 18
        CollectionAssert.AreEqual(reader.ReadBytes(17), Encoding.UTF8.GetBytes("բարեւ Ձեզ"));
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present

        Assert.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        CollectionAssert.AreEqual(reader.ReadBytes(3), Encoding.UTF8.GetBytes("vie")); // element name is 'vie'
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(10));     // string content length is 10
        CollectionAssert.AreEqual(reader.ReadBytes(9), Encoding.UTF8.GetBytes("xin chào"));
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present
         
        Assert.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        CollectionAssert.AreEqual(reader.ReadBytes(3), Encoding.UTF8.GetBytes("por")); // element name is 'por'
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(5));      // string content length is 5
        CollectionAssert.AreEqual(reader.ReadBytes(4), Encoding.UTF8.GetBytes("Olá"));
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present
         
        Assert.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        CollectionAssert.AreEqual(reader.ReadBytes(3), Encoding.UTF8.GetBytes("ukr")); // element name is 'ukr'
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(13));     // string content length is 13
        CollectionAssert.AreEqual(reader.ReadBytes(12), Encoding.UTF8.GetBytes("Привіт"));
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present
                                
        Assert.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        CollectionAssert.AreEqual(reader.ReadBytes(3), Encoding.UTF8.GetBytes("ger")); // element name is 'ger'
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(9));      // string content length is 9
        CollectionAssert.AreEqual(reader.ReadBytes(8), Encoding.UTF8.GetBytes("wünsche"));
        Assert.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present

        Assert.AreEqual(reader.ReadByte(), (byte) 0x00); // ensure last byte is terminator 0x00

        Assert.AreEqual(stream.Position, 232); // ensure whole document readed
      }
    }

    /// <summary>
    /// (Longrunning!)
    /// Ensures that simple string element is correctly serialized for all string lengths
    /// in range from 0 to 10 Kb
    /// { vary: "aaaa...[n of 'a' chars]" }
    /// </summary>
    [TestCase]
    public void WriteReadStringsWithDifferentLengths()
    {
      Parallel.For(0, 10*1024, i =>
      {
        using (var stream = new MemoryStream())
        using (var reader = new BinaryReader(stream))
        {
          // Write

          var root = new BSONDocument();
          var value = new string('a', i);
          root.Set(new BSONStringElement("vary", value));
          root.WriteAsBSON(stream);

          Assert.AreEqual(stream.Position, 16 + i); // ensure document length is 16+i bytes

          stream.Seek(0, SeekOrigin.Begin);

          CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(16 + i)); // content length is 16+i
          Assert.AreEqual(reader.ReadByte(), (byte) BSONElementType.String); // element type is string 0x02
          CollectionAssert.AreEqual(reader.ReadBytes(4), Encoding.UTF8.GetBytes("vary")); // element name is 'vary'
          Assert.AreEqual(reader.ReadByte(), (byte) 0x00); // string name terminator 0x00 is present
          CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(i + 1)); // string content length is 13
          CollectionAssert.AreEqual(reader.ReadBytes(i), Encoding.UTF8.GetBytes(value)); // element value is value
          Assert.AreEqual(reader.ReadByte(), (byte) 0x00); // string value terminator 0x00 is present
          Assert.AreEqual(reader.ReadByte(), (byte) 0x00); // last byte is terminator 0x00

          Assert.AreEqual(stream.Position, 16 + i); // ensure whole document readed 
          stream.Position = 0;

          // Read

          var deser = new BSONDocument(stream);

          Assert.AreEqual(deser.ByteSize, 16 + i);
          Assert.AreEqual(deser.Count, 1);

          var element = deser["vary"] as BSONStringElement;
          Assert.IsNotNull(element);
          Assert.AreEqual(element.ElementType, BSONElementType.String);
          Assert.AreEqual(element.Name, "vary");
          Assert.AreEqual(element.Value, value);
          Assert.AreEqual(stream.Position, 16 + i); // ensure whole document readed
          stream.Position = 0;
        }
      });
    }

    /// <summary>
    /// { intMin: <int.MinValue>, intMax: <int.MaxValue>, longMin: <long.MinValue>, longMax: <long.MaxValue> }
    /// </summary>
    [TestCase]
    public void WriteBigIntegers()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONInt32Element("intMin", int.MinValue));
        root.Set(new BSONInt32Element("intMax", int.MaxValue));
        root.Set(new BSONInt64Element("longMin", long.MinValue));
        root.Set(new BSONInt64Element("longMax", long.MaxValue));
        root.WriteAsBSON(stream);

        Assert.AreEqual(stream.Position, 63); // ensure document length is 63 bytes

        stream.Seek(0, SeekOrigin.Begin);

        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(63));           // content length is 63

        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int32);                     // element type is int32 0x10
        CollectionAssert.AreEqual(reader.ReadBytes(6), Encoding.UTF8.GetBytes("intMin"));    // element name is 'intMin'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                      // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(int.MinValue)); // element value is int.MinValue

        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int32);                     // element type is int32 0x10
        CollectionAssert.AreEqual(reader.ReadBytes(6), Encoding.UTF8.GetBytes("intMax"));    // element name is 'intMax'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                      // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(4), BitConverter.GetBytes(int.MaxValue)); // element value is int.MaxValue
        
        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int64);                      // element type is int64 0x12
        CollectionAssert.AreEqual(reader.ReadBytes(7), Encoding.UTF8.GetBytes("longMin"));    // element name is 'longMin'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                       // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(8), BitConverter.GetBytes(long.MinValue)); // element value is long.MinValue

        Assert.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int64);                      // element type is int64 0x12
        CollectionAssert.AreEqual(reader.ReadBytes(7), Encoding.UTF8.GetBytes("longMax"));    // element name is 'longMax'
        Assert.AreEqual(reader.ReadByte(), (byte)0x00);                                       // string name terminator 0x00 is present
        CollectionAssert.AreEqual(reader.ReadBytes(8), BitConverter.GetBytes(long.MaxValue)); // element value is long.MaxValue


        Assert.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00 
         
        Assert.AreEqual(stream.Position, 63); // ensure whole document readed
      }
    }

    #endregion

    #region Deserialization

    /// <summary>
    /// {} (empty document)
    /// </summary>
    [TestCase]
    public void ReadEmptyDocument()
    {
      var src = Convert.FromBase64String(@"BQAAAAA=");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 5);
        Assert.AreEqual(root.Count, 0);
        Assert.AreEqual(stream.Position, 5); // ensure whole document readed
      }
    }

    /// <summary>
    /// { pi: 3.14159265358979 }
    /// </summary>
    [TestCase]
    public void ReadSingleDouble()
    {
      var src = Convert.FromBase64String(@"EQAAAAFwaQAYLURU+yEJQAA=");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 17);
        Assert.AreEqual(root.Count, 1);

        var element = root["pi"] as BSONDoubleElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.Double); 
        Assert.AreEqual(element.Name, "pi"); 
        Assert.IsTrue(Math.Abs(element.Value - Math.PI) < double.Epsilon); 
        Assert.AreEqual(stream.Position, 17); // ensure whole document readed
      }
    }
        
    /// <summary>
    /// { greetings: "Hello World!" }
    /// </summary>
    [TestCase]
    public void ReadSingleString()
    {
      var src = Convert.FromBase64String(@"IQAAAAJncmVldGluZ3MADQAAAEhlbGxvIFdvcmxkIQAA");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 33);
        Assert.AreEqual(root.Count, 1);

        var element = root["greetings"] as BSONStringElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.String); 
        Assert.AreEqual(element.Name, "greetings"); 
        Assert.AreEqual(element.Value, "Hello World!");
        Assert.AreEqual(stream.Position, 33); // ensure whole document readed
      }
    }

    /// <summary>
    /// { lucky: 7 }
    /// </summary>
    [TestCase]
    public void ReadSingleInt32()
    {
      var src = Convert.FromBase64String(@"EAAAABBsdWNreQAHAAAAAA==");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 16);
        Assert.AreEqual(root.Count, 1);

        var element = root["lucky"] as BSONInt32Element;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.Int32); 
        Assert.AreEqual(element.Name, "lucky"); 
        Assert.AreEqual(element.Value, 7);
        Assert.AreEqual(stream.Position, 16); // ensure whole document readed
      }
    }

    /// <summary>
    /// { solarSystemDiameter: 10000000000000 }
    /// </summary>
    [TestCase]
    public void ReadSingleInt64()
    {
      var src = Convert.FromBase64String(@"IgAAABJzb2xhclN5c3RlbURpYW1ldGVyAACgck4YCQAAAA==");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 34);
        Assert.AreEqual(root.Count, 1);

        var element = root["solarSystemDiameter"] as BSONInt64Element;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.Int64); 
        Assert.AreEqual(element.Name, "solarSystemDiameter"); 
        Assert.AreEqual(element.Value, 10000000000000);
        Assert.AreEqual(stream.Position, 34); // ensure whole document readed
      }
    }  
    
    /// <summary>
    /// Array of strings
    /// { 'fruits': ['apple, 'orange', 'plum'] } --> { 'fruits': { '0': 'apple', '1': 'orange', '2': 'plum' } }
    /// </summary>
    [TestCase]
    public void ReadStringArray()
    {
      var src = Convert.FromBase64String(@"OQAAAARmcnVpdHMALAAAAAIwAAYAAABhcHBsZQACMQAHAAAAb3JhbmdlAAIyAAUAAABwbHVtAAAA");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 57);
        Assert.AreEqual(root.Count, 1);

        var element = root["fruits"] as BSONArrayElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.Name, "fruits"); 
        Assert.AreEqual(element.ElementType, BSONElementType.Array); 
        Assert.IsNotNull(element.Value); 
        Assert.AreEqual(element.Value.Length, 3); 

        var item1 = element.Value[0] as BSONStringElement;
        Assert.IsNotNull(item1);
        Assert.IsTrue(item1.IsArrayElement); 
        Assert.AreEqual(item1.ElementType, BSONElementType.String); 
        Assert.AreEqual(item1.Value, "apple");

        var item2 = element.Value[1] as BSONStringElement;
        Assert.IsNotNull(item2);
        Assert.IsTrue(item2.IsArrayElement); 
        Assert.AreEqual(item2.ElementType, BSONElementType.String); 
        Assert.AreEqual(item2.Value, "orange");

        var item3 = element.Value[2] as BSONStringElement;
        Assert.IsNotNull(item3);
        Assert.AreEqual(item3.ElementType, BSONElementType.String); 
        Assert.IsTrue(item3.IsArrayElement); 
        Assert.AreEqual(item3.Value, "plum");

        Assert.AreEqual(stream.Position, 57); // ensure whole document readed  
      }
    }
                
    /// <summary>
    /// Array of int32
    /// { 'years': [1963, 1984, 2015] } --> { 'years': { '0': 1963, '1': 1984, '2': 2015 } }
    /// </summary>
    [TestCase]
    public void ReadInt32Array()
    {
      var src = Convert.FromBase64String(@"JgAAAAR5ZWFycwAaAAAAEDAAqwcAABAxAMAHAAAQMgDfBwAAAAA=");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 38);
        Assert.AreEqual(root.Count, 1);

        var element = root["years"] as BSONArrayElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.Name, "years"); 
        Assert.AreEqual(element.ElementType, BSONElementType.Array); 
        Assert.IsNotNull(element.Value); 
        Assert.AreEqual(element.Value.Length, 3); 

        var item1 = element.Value[0] as BSONInt32Element;
        Assert.IsNotNull(item1);
        Assert.AreEqual(item1.ElementType, BSONElementType.Int32); 
        Assert.IsTrue(item1.IsArrayElement); 
        Assert.AreEqual(item1.Value, 1963);
                      
        var item2 = element.Value[1] as BSONInt32Element;
        Assert.IsNotNull(item2);
        Assert.AreEqual(item2.ElementType, BSONElementType.Int32); 
        Assert.IsTrue(item2.IsArrayElement); 
        Assert.AreEqual(item2.Value, 1984);
        
        var item3 = element.Value[2] as BSONInt32Element;
        Assert.IsNotNull(item3);
        Assert.AreEqual(item3.ElementType, BSONElementType.Int32); 
        Assert.IsTrue(item3.IsArrayElement); 
        Assert.AreEqual(item3.Value, 2015);
        
        Assert.AreEqual(stream.Position, 38); // ensure whole document readed
      }
    } 

    /// <summary>
    /// Array of strings
    /// { 'stuff': ['apple, 3, 2.14] } --> { 'stuff': { '0': 'apple', '1': 3, '2': 2.14 } }
    /// </summary>
    [TestCase]
    public void ReadStringInt32DoubleMixedArray()
    {
      var src = Convert.FromBase64String(@"MAAAAARzdHVmZgAkAAAAAjAABgAAAGFwcGxlABAxAAMAAAABMgAfhetRuB4BQAAA");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 48);
        Assert.AreEqual(root.Count, 1);

        var element = root["stuff"] as BSONArrayElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.Name, "stuff"); 
        Assert.AreEqual(element.ElementType, BSONElementType.Array); 
        Assert.IsNotNull(element.Value); 
        Assert.AreEqual(element.Value.Length, 3); 

        var item1 = element.Value[0] as BSONStringElement;
        Assert.IsNotNull(item1);
        Assert.IsTrue(item1.IsArrayElement); 
        Assert.AreEqual(item1.ElementType, BSONElementType.String); 
        Assert.AreEqual(item1.Value, "apple");  

        var item2 = element.Value[1] as BSONInt32Element;
        Assert.IsNotNull(item2);
        Assert.IsTrue(item2.IsArrayElement); 
        Assert.AreEqual(item2.ElementType, BSONElementType.Int32); 
        Assert.AreEqual(item2.Value, 3);  

        var item3 = element.Value[2] as BSONDoubleElement;
        Assert.IsNotNull(item3);
        Assert.IsTrue(item3.IsArrayElement); 
        Assert.AreEqual(item3.ElementType, BSONElementType.Double); 
        Assert.AreEqual(item3.Value, 2.14D);
        
        Assert.AreEqual(stream.Position, 48); // ensure whole document readed
      }
    } 

    /// <summary>
    /// { name: "Gagarin", birth: 1934 }
    /// </summary>
    [TestCase]
    public void ReadStringAndInt32Pair()
    { 
      var src = Convert.FromBase64String(@"IgAAAAJuYW1lAAgAAABHYWdhcmluABBiaXJ0aACOBwAAAA==");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 34);
        Assert.AreEqual(root.Count, 2);

        var element1 = root["name"] as BSONStringElement;
        Assert.IsNotNull(element1);
        Assert.AreEqual(element1.ElementType, BSONElementType.String); 
        Assert.AreEqual(element1.Name, "name"); 
        Assert.AreEqual(element1.Value, "Gagarin"); 
               
        var element2 = root["birth"] as BSONInt32Element;
        Assert.IsNotNull(element2);
        Assert.AreEqual(element2.ElementType, BSONElementType.Int32); 
        Assert.AreEqual(element2.Name, "birth"); 
        Assert.AreEqual(element2.Value, 1934);
        
        Assert.AreEqual(stream.Position, 34); // ensure whole document readed 
      }
    }
    
    /// <summary>
    /// { nested: { capital: "Moscow" } }
    /// </summary>
    [TestCase]
    public void ReadNestedDocument()
    {
      var src = Convert.FromBase64String(@"JgAAAANuZXN0ZWQAGQAAAAJjYXBpdGFsAAcAAABNb3Njb3cAAAA=");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 38);
        Assert.AreEqual(root.Count, 1);

        var element = root["nested"] as BSONDocumentElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.Name, "nested");
        Assert.IsNotNull(element.Value); 
        Assert.AreEqual(element.ElementType, BSONElementType.Document); 
        Assert.AreEqual(element.Value.Count, 1);

        var nested = element.Value["capital"] as BSONStringElement;
        Assert.IsNotNull(nested);
        Assert.AreEqual(nested.ElementType, BSONElementType.String); 
        Assert.AreEqual(nested.Name, "capital");
        Assert.AreEqual(nested.Value, "Moscow");
        
        Assert.AreEqual(stream.Position, 38); // ensure whole document readed
      }
    }
    
    /// <summary>
    ///  { binary: <bytes from 'This is binary data'> }
    /// </summary>
    [TestCase]
    public void ReadSingleBinaryData()
    {
      var src = Convert.FromBase64String(@"JQAAAAViaW5hcnkAEwAAAAJUaGlzIGlzIGJpbmFyeSBkYXRhAA==");

      using (var stream = new MemoryStream(src))
      {
        var data = Encoding.UTF8.GetBytes("This is binary data");
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 37);
        Assert.AreEqual(root.Count, 1);

        var element = root["binary"] as BSONBinaryElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.Binary); 
        Assert.AreEqual(element.Name, "binary");
        Assert.AreEqual(element.Value.Data, data);
        Assert.AreEqual(element.Value.Type, BSONBinaryType.BinaryOld);
        
        Assert.AreEqual(stream.Position, 37); // ensure whole document readed
      }
    }
    
    /// <summary>
    ///  { objectId: <bytes from hex '507f1f77bcf86cd799439011'> }
    /// </summary>
    [TestCase]
    public void ReadSingleObjectId()
    {
      var src = Convert.FromBase64String(@"GwAAAAdvYmplY3RJZABQfx93vPhs15lDkBEA");

      using (var stream = new MemoryStream(src))
      {
        var hex = "507f1f77bcf86cd799439011";
        var data = Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray();
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 27);
        Assert.AreEqual(root.Count, 1);

        var element = root["objectId"] as BSONObjectIDElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.ObjectID); 
        Assert.AreEqual(element.Name, "objectId");
        Assert.AreEqual(element.Value.Bytes, data);
        
        Assert.AreEqual(stream.Position, 27); // ensure whole document readed
      }
    } 
    
    /// <summary>
    /// { booleanTrue: true }
    /// </summary>
    [TestCase]
    public void ReadSingleBooleanTrue()
    {
      var src = Convert.FromBase64String(@"EwAAAAhib29sZWFuVHJ1ZQABAA==");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 19);
        Assert.AreEqual(root.Count, 1);

        var element = root["booleanTrue"] as BSONBooleanElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.Boolean); 
        Assert.AreEqual(element.Name, "booleanTrue");
        Assert.AreEqual(element.Value, true); 
        
        Assert.AreEqual(stream.Position, 19); // ensure whole document readed 
      }
    }  
    
    /// <summary>
    /// { booleanFalse: false }
    /// </summary>
    [TestCase]
    public void ReadSingleBooleanFalse()
    {
      var src = Convert.FromBase64String(@"FAAAAAhib29sZWFuRmFsc2UAAAA=");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 20);
        Assert.AreEqual(root.Count, 1);

        var element = root["booleanFalse"] as BSONBooleanElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.Boolean); 
        Assert.AreEqual(element.Name, "booleanFalse");
        Assert.AreEqual(element.Value, false); 
        
        Assert.AreEqual(stream.Position, 20); // ensure whole document readed 
      }
    }
    
    /// <summary>
    /// { null: null }
    /// </summary>
    [TestCase]
    public void ReadSingleNull()
    {
      var src = Convert.FromBase64String(@"CwAAAApudWxsAAA=");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 11);
        Assert.AreEqual(root.Count, 1);

        var element = root["null"] as BSONNullElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.Null); 
        Assert.AreEqual(element.Name, "null");     
      }
    } 

    /// <summary>
    /// { now: <DateTime from 635000000000000000 ticks> }
    /// </summary>
    [TestCase]
    public void ReadSingleDateTime()
    {                                     
      var src = Convert.FromBase64String(@"EgAAAAlub3cAAKDErD0BAAAA");

      using (var stream = new MemoryStream(src))
      {
        var now = new DateTime(635000000000000000, DateTimeKind.Utc);
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 18); 
        Assert.AreEqual(root.Count, 1);

        var element = root["now"] as BSONDateTimeElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.DateTime); 
        Assert.AreEqual(element.Name, "now"); 
        Assert.AreEqual(element.Value, now);
        
        Assert.AreEqual(stream.Position, 18); // ensure whole document readed 
      }
    }     

    /// <summary>
    /// { email: <pattern='^[-.\w]+@(?:[a-z\d]{2,}\.)+[a-z]{2,6}$' options=I,M,U> }
    /// </summary>
    [TestCase]
    public void ReadSingleRegularExpression()
    {
      var src = Convert.FromBase64String(@"NwAAAAtlbWFpbABeWy0uXHddK0AoPzpbYS16XGRdezIsfVwuKStbYS16XXsyLDZ9JABpbXUAAA==");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);
        var pattern = @"^[-.\w]+@(?:[a-z\d]{2,}\.)+[a-z]{2,6}$";
        var options = BSONRegularExpressionOptions.I | BSONRegularExpressionOptions.M |BSONRegularExpressionOptions.U;

        Assert.AreEqual(root.ByteSize, 55); 
        Assert.AreEqual(root.Count, 1);

        var element = root["email"] as BSONRegularExpressionElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.Name, "email"); 
        Assert.AreEqual(element.ElementType, BSONElementType.RegularExpression); 
        Assert.AreEqual(element.Value.Pattern, pattern);
        Assert.AreEqual(element.Value.Options, options);
        
        Assert.AreEqual(stream.Position, 55); // ensure whole document readed
      }
    } 

    /// <summary>
    /// { code: "function(){var x=1;var y='abc';return 1;};" }
    /// </summary>
    [TestCase]
    public void ReadSingleJavaScript()
    {
      var src = Convert.FromBase64String(@"OgAAAA1jb2RlACsAAABmdW5jdGlvbigpe3ZhciB4PTE7dmFyIHk9J2FiYyc7cmV0dXJuIDE7fTsAAA==");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);
        var code = "function(){var x=1;var y='abc';return 1;};";

        Assert.AreEqual(root.ByteSize, 58);
        Assert.AreEqual(root.Count, 1);

        var element = root["code"] as BSONJavaScriptElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.JavaScript); 
        Assert.AreEqual(element.Name, "code"); 
        Assert.AreEqual(element.Value, code);
        
        Assert.AreEqual(stream.Position, 58); // ensure whole document readed
      }
    } 

    /// <summary>
    /// { codeWithScope: "function(){var x=1;var y='abc';return z;}; <with scope: z=23>" }
    /// </summary>
    [TestCase]
    public void ReadSingleJavaScriptWithScope()
    {
      var src = Convert.FromBase64String(@"UwAAAA9jb2RlV2l0aFNjb3BlAD8AAAArAAAAZnVuY3Rpb24oKXt2YXIgeD0xO3ZhciB5PSdhYmMnO3JldHVybiB6O307AAwAAAAQegAXAAAAAAA=");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);
        var code = "function(){var x=1;var y='abc';return z;};";

        Assert.AreEqual(root.ByteSize, 83);
        Assert.AreEqual(root.Count, 1);

        var element = root["codeWithScope"] as BSONJavaScriptWithScopeElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.JavaScriptWithScope); 
        Assert.AreEqual(element.Name, "codeWithScope"); 
        Assert.AreEqual(element.Value.Code, code);

        var scope = element.Value.Scope;
        Assert.IsNotNull(scope);
        Assert.AreEqual(scope.Count, 1);

        var scopeVar = scope["z"] as BSONInt32Element;
        Assert.IsNotNull(scopeVar);
        Assert.AreEqual(scopeVar.ElementType, BSONElementType.Int32); 
        Assert.AreEqual(scopeVar.Name, "z"); 
        Assert.AreEqual(scopeVar.Value, 23); 
        
        Assert.AreEqual(stream.Position, 83); // ensure whole document readed
      }
    }   

    /// <summary>
    /// { stamp: <seconds since Unix epoch to DateTime from 635000000000000000 ticks with 123 increment> }
    /// </summary>
    [TestCase]
    public void ReadSingleTimestamp()
    {
      var src = Convert.FromBase64String(@"FAAAABFzdGFtcAB7AAAAACRTUQA=");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);
        var now = new DateTime(635000000000000000, DateTimeKind.Utc);
        var increment = 123;

        Assert.AreEqual(root.ByteSize, 20);
        Assert.AreEqual(root.Count, 1);

        var element = root["stamp"] as BSONTimestampElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.Name, "stamp"); 
        Assert.AreEqual(element.ElementType, BSONElementType.TimeStamp);
        Assert.AreEqual(element.Value.EpochSeconds, now.ToSecondsSinceUnixEpochStart());
        Assert.AreEqual(element.Value.Increment, increment); 
        
        Assert.AreEqual(stream.Position, 20); // ensure whole document readed
      }
    } 

    /// <summary>
    /// { maxkey: <maxkey> }
    /// </summary>
    [TestCase]
    public void ReadSingleMaxKey()
    {
      var src = Convert.FromBase64String(@"DQAAAH9tYXhrZXkAAA==");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 13);
        Assert.AreEqual(root.Count, 1);

        var element = root["maxkey"] as BSONMaxKeyElement;
        Assert.IsNotNull(element);   
        Assert.AreEqual(element.ElementType, BSONElementType.MaxKey);
        Assert.AreEqual(element.Name, "maxkey");
        
        Assert.AreEqual(stream.Position, 13); // ensure whole document readed 
      }
    } 

    /// <summary>
    /// { minkey: <minkey> }
    /// </summary>
    [TestCase]
    public void ReadSingleMinKey()
    {
      var src = Convert.FromBase64String(@"DQAAAP9taW5rZXkAAA==");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 13);
        Assert.AreEqual(root.Count, 1);

        var element = root["minkey"] as BSONMinKeyElement;
        Assert.IsNotNull(element);      
        Assert.AreEqual(element.ElementType, BSONElementType.MinKey);
        Assert.AreEqual(element.Name, "minkey");
        
        Assert.AreEqual(stream.Position, 13); // ensure whole document readed 
      }
    }   

    [TestCase]
    public void WriteReadSingleDateTime()
    {
      using (var stream = new MemoryStream())
      {
        var now = new DateTime(2015, 08, 26, 14, 23, 56);
        var bson1 = new BSONDocument();
        bson1.Set(new BSONDateTimeElement("now", now));
        bson1.WriteAsBSON(stream);

        stream.Position = 0;

        var bson2 = new BSONDocument(stream);

        var now1 = ((BSONDateTimeElement)bson1["now"]).Value;
        var now2 = ((BSONDateTimeElement)bson2["now"]).Value;

        Console.WriteLine("{0} {1}", now1, now1.Kind);
        Console.WriteLine("{0} {1}", now2, now2.Kind);

        Assert.AreEqual(now1.ToUniversalTime(), now2);
      }
    }

    /// <summary>
    /// { 
    ///   eng: "hello", 
    ///   rus: "привет", 
    ///   chi: "你好", 
    ///   jap: "こんにちは", 
    ///   gre: "γεια σας", 
    ///   alb: "përshëndetje",
    ///   arm: "բարեւ Ձեզ",
    ///   vie: "xin chào",
    ///   por: "Olá",
    ///   ukr: "Привіт",
    ///   ger: "wünsche"
    /// }
    /// </summary>
    [TestCase]
    public void ReadUnicodeStrings()
    {
      var src = Convert.FromBase64String(@"6AAAAAJlbmcABgAAAGhlbGxvAAJydXMADQAAANC/0YDQuNCy0LXRggACY2hpAAcAAADkvaDlpb0AAmphcAAQAAAA44GT44KT44Gr44Gh44GvAAJncmUAEAAAAM6zzrXOuc6xIM+DzrHPggACYWxiAA8AAABww6tyc2jDq25kZXRqZQACYXJtABIAAADVotWh1oDVpdaCINWB1aXVpgACdmllAAoAAAB4aW4gY2jDoG8AAnBvcgAFAAAAT2zDoQACdWtyAA0AAADQn9GA0LjQstGW0YIAAmdlcgAJAAAAd8O8bnNjaGUAAA==");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 232);
        Assert.AreEqual(root.Count, 11);   

        var element = root["eng"] as BSONStringElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.String); 
        Assert.AreEqual(element.Name, "eng"); 
        Assert.AreEqual(element.Value, "hello");

        element = root["rus"] as BSONStringElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.String); 
        Assert.AreEqual(element.Name, "rus"); 
        Assert.AreEqual(element.Value, "привет");

        element = root["chi"] as BSONStringElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.String); 
        Assert.AreEqual(element.Name, "chi"); 
        Assert.AreEqual(element.Value, "你好"); 

        element = root["jap"] as BSONStringElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.String); 
        Assert.AreEqual(element.Name, "jap"); 
        Assert.AreEqual(element.Value, "こんにちは");

        element = root["gre"] as BSONStringElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.String); 
        Assert.AreEqual(element.Name, "gre"); 
        Assert.AreEqual(element.Value, "γεια σας");

        element = root["alb"] as BSONStringElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.String); 
        Assert.AreEqual(element.Name, "alb"); 
        Assert.AreEqual(element.Value, "përshëndetje"); 

        element = root["arm"] as BSONStringElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.String); 
        Assert.AreEqual(element.Name, "arm"); 
        Assert.AreEqual(element.Value, "բարեւ Ձեզ");

        element = root["vie"] as BSONStringElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.String); 
        Assert.AreEqual(element.Name, "vie"); 
        Assert.AreEqual(element.Value, "xin chào"); 

        element = root["por"] as BSONStringElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.String); 
        Assert.AreEqual(element.Name, "por"); 
        Assert.AreEqual(element.Value, "Olá");  

        element = root["ukr"] as BSONStringElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.String); 
        Assert.AreEqual(element.Name, "ukr"); 
        Assert.AreEqual(element.Value, "Привіт"); 

        element = root["ger"] as BSONStringElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(element.ElementType, BSONElementType.String); 
        Assert.AreEqual(element.Name, "ger"); 
        Assert.AreEqual(element.Value, "wünsche");

        Assert.AreEqual(stream.Position, 232); // ensure whole document readed
      }
    }

    /// <summary>
    /// { intMin: <int.MinValue>, intMax: <int.MaxValue>, longMin: <long.MinValue>, longMax: <long.MaxValue> }
    /// </summary>
    [TestCase]
    public void ReadBigIntegers()
    {
      var src = Convert.FromBase64String(@"PwAAABBpbnRNaW4AAAAAgBBpbnRNYXgA////fxJsb25nTWluAAAAAAAAAACAEmxvbmdNYXgA/////////38A");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Assert.AreEqual(root.ByteSize, 63);
        Assert.AreEqual(root.Count, 4);

        var element1 = root["intMin"] as BSONInt32Element;
        Assert.IsNotNull(element1);
        Assert.AreEqual(element1.ElementType, BSONElementType.Int32); 
        Assert.AreEqual(element1.Name, "intMin"); 
        Assert.AreEqual(element1.Value, int.MinValue); 

        var element2 = root["intMax"] as BSONInt32Element;
        Assert.IsNotNull(element2);
        Assert.AreEqual(element2.ElementType, BSONElementType.Int32); 
        Assert.AreEqual(element2.Name, "intMax"); 
        Assert.AreEqual(element2.Value, int.MaxValue);
                                                   
        var element3 = root["longMin"] as BSONInt64Element;
        Assert.IsNotNull(element3);
        Assert.AreEqual(element3.ElementType, BSONElementType.Int64); 
        Assert.AreEqual(element3.Name, "longMin"); 
        Assert.AreEqual(element3.Value, long.MinValue);
                                                   
        var element4 = root["longMax"] as BSONInt64Element;
        Assert.IsNotNull(element4);
        Assert.AreEqual(element4.ElementType, BSONElementType.Int64); 
        Assert.AreEqual(element4.Name, "longMax"); 
        Assert.AreEqual(element4.Value, long.MaxValue); 

        Assert.AreEqual(stream.Position, 63); // ensure whole document readed
      }
    }

    #endregion

    #region IConvertable

    [TestCase]
    public void TestInt32ElementIConvertable()
    {
      var element = new BSONInt32Element("name", 1256);

      var bl     = element.AsBool();
      var bt     = element.AsByte((byte)23);
      var chr    = element.AsChar();
      var date   = element.AsDateTime();
      var decim  = element.AsDecimal();
      var doubl  = element.AsDouble();
      var enm    = element.AsEnum(BSONElementType.Null);
      var gdid   = element.AsGDID();
      var guid   = element.AsGUID(Guid.Empty);
      var int16  = element.AsShort();
      var int32  = element.AsInt();
      var int64  = element.AsLong();
      var lac    = element.AsLaconicConfig(null);
      var sbt    = element.AsSByte((sbyte)23);
      var single = element.AsFloat(); 
      var str    = element.AsString();
      var uint16 = element.AsUShort();
      var uint32 = element.AsUInt();
      var uint64 = element.AsULong();

      var n_bl     = element.AsNullableBool();
      var n_bt     = element.AsNullableByte((byte)23);
      var n_chr    = element.AsNullableChar();
      var n_date   = element.AsNullableDateTime();
      var n_decim  = element.AsNullableDecimal();
      var n_doubl  = element.AsNullableDouble();
      var n_ts    = element.AsNullableTimeSpan(null);
      var n_gdid   = element.AsNullableGDID(new GDID(1,2,3));
      var n_guid   = element.AsNullableGUID(Guid.Empty);
      var n_int16  = element.AsNullableShort();
      var n_int32  = element.AsNullableInt();
      var n_int64  = element.AsNullableLong();
      var n_sbt    = element.AsNullableSByte((sbyte)23);
      var n_single = element.AsNullableFloat(); 
      var n_uint16 = element.AsNullableUShort();
      var n_uint32 = element.AsNullableUInt();
      var n_uint64 = element.AsNullableULong();

      Assert.AreEqual(bl, true); 
      Assert.AreEqual(bt, (byte)23);
      Assert.AreEqual(chr, Convert.ToChar(1256));
      Assert.AreEqual(date, new DateTime(1256));
      Assert.AreEqual(decim, 1256);
      Assert.AreEqual(doubl, 1256.0D);   
      Assert.AreEqual(enm, (BSONElementType)1256);
      Assert.AreEqual(lac, null);
      Assert.AreEqual(gdid, new GDID(0, 0, 1256));
      Assert.AreEqual(guid, Guid.Empty);
      Assert.AreEqual(int16, 1256);
      Assert.AreEqual(int32, 1256);
      Assert.AreEqual(int64, 1256);
      Assert.AreEqual(sbt, (byte)23);
      Assert.AreEqual(single, 1256.0F);
      Assert.AreEqual(str, "1256");
      Assert.AreEqual(uint16, 1256);
      Assert.AreEqual(uint32, 1256);
      Assert.AreEqual(uint64, 1256);

      Assert.AreEqual(n_bl, true); 
      Assert.AreEqual(n_bt, (byte)23);
      Assert.AreEqual(n_chr, Convert.ToChar(1256));
      Assert.AreEqual(n_date, new DateTime(1256));
      Assert.AreEqual(n_decim, 1256);
      Assert.AreEqual(n_doubl, 1256.0D);   
      Assert.AreEqual(n_ts, TimeSpan.FromTicks(1256));
      Assert.AreEqual(n_gdid, new GDID(0, 0, 1256));
      Assert.AreEqual(n_guid, Guid.Empty);
      Assert.AreEqual(n_int16, 1256);
      Assert.AreEqual(n_int32, 1256);
      Assert.AreEqual(n_int64, 1256);
      Assert.AreEqual(n_sbt, (byte)23);
      Assert.AreEqual(n_single, 1256.0F);
      Assert.AreEqual(n_uint16, 1256);
      Assert.AreEqual(n_uint32, 1256);
      Assert.AreEqual(n_uint64, 1256);
    }

    [TestCase]
    public void TestInt64ElementIConvertable()
    {
      var element = new BSONInt64Element("name", 1256000000000);

      var bl = element.AsBool();
      var date = element.AsDateTime();
      var decim = element.AsDecimal();
      var doubl = element.AsDouble();
      var int64 = element.AsLong();
      var single = element.AsFloat(); 
      var str = element.AsString();
      var uint64 = element.AsULong();

      Assert.AreEqual(bl, true); 
      Assert.AreEqual(date, new DateTime(1256000000000));
      Assert.AreEqual(decim, 1256000000000);
      Assert.AreEqual(doubl, 1256000000000.0D);
      Assert.AreEqual(int64, 1256000000000);
      Assert.AreEqual(single, 1256000000000);
      Assert.AreEqual(str, "1256000000000");
      Assert.AreEqual(uint64, 1256000000000);
    } 

    [TestCase]
    public void TestDoubleElementIConvertable()
    {
      var element = new BSONDoubleElement("name", 1256.1234D);

      var bl = element.AsBool();
      var decim = element.AsDecimal();
      var doubl = element.AsDouble();
      var int16 = element.AsShort();
      var int32 = element.AsInt();
      var int64 = element.AsLong();
      var single = element.AsFloat(); 
      var str = element.AsString();
      var uint16 = element.AsUShort();
      var uint32 = element.AsUInt();
      var uint64 = element.AsULong();

      Assert.AreEqual(bl, true); 
      Assert.AreEqual(decim, 1256.1234M);
      Assert.AreEqual(doubl, 1256.1234D);
      Assert.AreEqual(int16, 1256);
      Assert.AreEqual(int32, 1256);
      Assert.AreEqual(int64, 1256);
      Assert.AreEqual(single, 1256.1234F);
      Assert.AreEqual((1256.1234D).ToString(), str);
      Assert.AreEqual(uint16, 1256);
      Assert.AreEqual(uint32, 1256);
      Assert.AreEqual(uint64, 1256);
    } 

    [TestCase]
    public void TestStringElementConvertable()
    {
      var element = new BSONStringElement("name", "1256");

      var bl = element.AsBool();
      var chr = element.AsChar();
      var date = element.AsDateTime();
      var decim = element.AsDecimal();
      var doubl = element.AsDouble();
      var int16 = element.AsShort();
      var int32 = element.AsInt();
      var int64 = element.AsLong();
      var single = element.AsFloat(); 
      var str = element.AsString();
      var uint16 = element.AsUShort();
      var uint32 = element.AsUInt();
      var uint64 = element.AsULong();

      Assert.AreEqual(bl, true); 
      Assert.AreEqual(chr, '1');
      Assert.AreEqual(date, new DateTime(1256));
      Assert.AreEqual(decim, 1256);
      Assert.AreEqual(doubl, 1256.0D);
      Assert.AreEqual(int16, 1256);
      Assert.AreEqual(int32, 1256);
      Assert.AreEqual(int64, 1256);
      Assert.AreEqual(single, 1256.0F);
      Assert.AreEqual(str, "1256");
      Assert.AreEqual(uint16, 1256);
      Assert.AreEqual(uint32, 1256);
      Assert.AreEqual(uint64, 1256);
    }

    [TestCase]
    public void TestBooleanElementConvertable()
    {
      var element = new BSONBooleanElement("name", true);
      var bl = element.AsBool();
      var decim = element.AsDecimal();
      var doubl = element.AsDouble();
      var int16 = element.AsShort();
      var int32 = element.AsInt();
      var int64 = element.AsLong();
      var single = element.AsFloat(); 
      var str = element.AsString();
      var uint16 = element.AsUShort();
      var uint32 = element.AsUInt();
      var uint64 = element.AsULong();

      Assert.AreEqual(bl, true); 
      Assert.AreEqual(decim, 1);
      Assert.AreEqual(doubl, 1);
      Assert.AreEqual(int16, 1);
      Assert.AreEqual(int32, 1);
      Assert.AreEqual(int64, 1);
      Assert.AreEqual(single, 1);
      Assert.AreEqual(str, "True");
      Assert.AreEqual(uint16, 1);
      Assert.AreEqual(uint32, 1);
      Assert.AreEqual(uint64, 1);
    }  

    [TestCase]
    public void TestDateTimeElementIConvertable()
    {
      var value = new DateTime(2015, 1, 21, 3, 4, 5);
      var element = new BSONDateTimeElement("name", value);

      var bl = element.AsBool();
      var date = element.AsDateTime();
      var str = element.AsString(null);
      Assert.AreEqual(bl, true); 
      Assert.AreEqual(date, value);
    }

    #endregion

    #region Templatization

    [TestCase]
    public void Templatization_QuerySinglePrimitiveTypes()
    {
      var qry1 = new BSONDocument("{ age: '$$count' }", true, 
                            new TemplateArg("count", BSONElementType.Int32, 67));
      var qry2 = new BSONDocument("{ max: '$$long' }", true, 
                            new TemplateArg("long", BSONElementType.Int64, long.MaxValue));
      var qry3 = new BSONDocument("{ array: '$$items' }", true, 
                            new TemplateArg("items", BSONElementType.Array,
                              new BSONElement[]
                              {
                                new BSONStringElement("name", "value"),
                                new BSONDoubleElement("name", -1.2345D),
                                new BSONInt32Element("name", 2000000000)
                              }));
      var qry4 = new BSONDocument("{ why: '$$answer' }", true, 
                            new TemplateArg("answer", BSONElementType.Boolean, true));
      var qry5 = new BSONDocument("{ qty: '$$value' }", true, 
                            new TemplateArg("value", BSONElementType.Double, 123456.789012D));

      Assert.AreEqual(qry1.Count, 1);
      Assert.IsNotNull(qry1["age"]);
      Assert.IsInstanceOf<BSONInt32Element>(qry1["age"]);
      Assert.AreEqual(qry1["age"].ObjectValue, 67);

      Assert.AreEqual(qry2.Count, 1);
      Assert.IsNotNull(qry2["max"]);
      Assert.IsInstanceOf<BSONInt64Element>(qry2["max"]);
      Assert.AreEqual(qry2["max"].ObjectValue, long.MaxValue);

      Assert.AreEqual(qry3.Count, 1);
      Assert.IsNotNull(qry3["array"]);
      Assert.IsInstanceOf<BSONArrayElement>(qry3["array"]);
      var elements = ((BSONArrayElement)qry3["array"]).Value;
      Assert.IsNotNull(elements);
      Assert.AreEqual(elements.Length, 3);
      Assert.IsInstanceOf<BSONStringElement>(elements[0]);
      Assert.IsInstanceOf<BSONDoubleElement>(elements[1]);
      Assert.IsInstanceOf<BSONInt32Element>(elements[2]);
      Assert.AreEqual(elements[0].ObjectValue, "value");
      Assert.AreEqual(elements[1].ObjectValue, -1.2345D);
      Assert.AreEqual(elements[2].ObjectValue, 2000000000);

      Assert.AreEqual(qry4.Count, 1);
      Assert.IsNotNull(qry4["why"]);
      Assert.IsInstanceOf<BSONBooleanElement>(qry4["why"]);
      Assert.AreEqual(qry4["why"].ObjectValue, true);

      Assert.AreEqual(qry5.Count, 1);
      Assert.IsNotNull(qry5["qty"]);
      Assert.IsInstanceOf<BSONDoubleElement>(qry5["qty"]);
      Assert.AreEqual(qry5["qty"].ObjectValue, 123456.789012D);
    }  

    [TestCase]
    public void Templatization_SinglePrimitiveNames()
    {
      var qry1 = new BSONDocument("{ '$$age': 67 }", true, 
                            new TemplateArg("age", BSONElementType.String, "myage"));
      var qry2 = new BSONDocument("{ '$$max': 9223372036854775807 }", true, 
                            new TemplateArg("max", BSONElementType.String, "longMax"));
      var qry3 = new BSONDocument("{ '$$items': [1, '2', 3] }", true, 
                            new TemplateArg("items", BSONElementType.String, "array"));
      var qry4 = new BSONDocument("{ '$$why': true }", true, 
                            new TemplateArg("why", BSONElementType.String, "whyTrue"));
      var qry5 = new BSONDocument("{ '$$qty': 123456.789012 }", true, 
                            new TemplateArg("qty", BSONElementType.String, "qtyName"));

      Assert.AreEqual(qry1.Count, 1);
      Assert.IsNotNull(qry1["myage"]);
      Assert.IsInstanceOf<BSONInt32Element>(qry1["myage"]);
      Assert.AreEqual(qry1["myage"].ObjectValue, 67);

      Assert.AreEqual(qry2.Count, 1);
      Assert.IsNotNull(qry2["longMax"]);
      Assert.IsInstanceOf<BSONInt64Element>(qry2["longMax"]);
      Assert.AreEqual(qry2["longMax"].ObjectValue, long.MaxValue);

      Assert.AreEqual(qry3.Count, 1);
      Assert.IsNotNull(qry3["array"]);
      Assert.IsInstanceOf<BSONArrayElement>(qry3["array"]);
      var elements = ((BSONArrayElement)qry3["array"]).Value;
      Assert.IsNotNull(elements);
      Assert.AreEqual(elements.Length, 3);
      Assert.IsInstanceOf<BSONInt32Element>(elements[0]);
      Assert.IsInstanceOf<BSONStringElement>(elements[1]);
      Assert.IsInstanceOf<BSONInt32Element>(elements[2]);
      Assert.AreEqual(elements[0].ObjectValue, 1);
      Assert.AreEqual(elements[1].ObjectValue, "2");
      Assert.AreEqual(elements[2].ObjectValue, 3);

      Assert.AreEqual(qry4.Count, 1);
      Assert.IsNotNull(qry4["whyTrue"]);
      Assert.IsInstanceOf<BSONBooleanElement>(qry4["whyTrue"]);
      Assert.AreEqual(qry4["whyTrue"].ObjectValue, true);

      Assert.AreEqual(qry5.Count, 1);
      Assert.IsNotNull(qry5["qtyName"]);
      Assert.IsInstanceOf<BSONDoubleElement>(qry5["qtyName"]);
      Assert.AreEqual(qry5["qtyName"].ObjectValue, 123456.789012D);
    }

    [TestCase]
    public void Templatization_SinglePrimitiveNamesAndValues()
    {
      var qry1 = new BSONDocument("{ '$$age': '$$ageValue' }", true, 
                            new TemplateArg("age", BSONElementType.String, "myage"), 
                            new TemplateArg("ageValue", BSONElementType.Int32, 30));
      var qry2 = new BSONDocument("{ '$$max': '$$maxValue' }", true, 
                            new TemplateArg("max", BSONElementType.String, "longMax"), 
                            new TemplateArg("maxValue", BSONElementType.Int64, long.MaxValue));
      var qry3 = new BSONDocument("{ '$$items': '$$arrayValue' }", true, 
                            new TemplateArg("items", BSONElementType.String, "array"), 
                            new TemplateArg("arrayValue", BSONElementType.Array,
                              new BSONElement[]
                              {
                                new BSONStringElement("name", "value"),
                                new BSONDoubleElement("name", -1.2345D),
                                new BSONInt32Element("name", 2000000000)
                              }));
      var qry4 = new BSONDocument("{ '$$why': '$$whyValue' }", true, 
                            new TemplateArg("why", BSONElementType.String, "whyTrue"), 
                            new TemplateArg("whyValue", BSONElementType.Boolean, true));
      var qry5 = new BSONDocument("{ '$$qty': '$$qtyValue' }", true, 
                            new TemplateArg("qty", BSONElementType.String, "qtyName"), 
                            new TemplateArg("qtyValue", BSONElementType.Double, 123456.789012D));

      Assert.AreEqual(qry1.Count, 1);
      Assert.IsNotNull(qry1["myage"]);
      Assert.IsInstanceOf<BSONInt32Element>(qry1["myage"]);
      Assert.AreEqual(qry1["myage"].ObjectValue, 30);

      Assert.AreEqual(qry2.Count, 1);
      Assert.IsNotNull(qry2["longMax"]);
      Assert.IsInstanceOf<BSONInt64Element>(qry2["longMax"]);
      Assert.AreEqual(qry2["longMax"].ObjectValue, long.MaxValue);

      Assert.AreEqual(qry3.Count, 1);
      Assert.IsNotNull(qry3["array"]);
      Assert.IsInstanceOf<BSONArrayElement>(qry3["array"]);
      var elements = ((BSONArrayElement)qry3["array"]).Value;
      Assert.IsNotNull(elements);
      Assert.AreEqual(elements.Length, 3);
      Assert.IsInstanceOf<BSONStringElement>(elements[0]);
      Assert.IsInstanceOf<BSONDoubleElement>(elements[1]);
      Assert.IsInstanceOf<BSONInt32Element>(elements[2]);
      Assert.AreEqual(elements[0].ObjectValue, "value");
      Assert.AreEqual(elements[1].ObjectValue, -1.2345D);
      Assert.AreEqual(elements[2].ObjectValue, 2000000000);

      Assert.AreEqual(qry4.Count, 1);
      Assert.IsNotNull(qry4["whyTrue"]);
      Assert.IsInstanceOf<BSONBooleanElement>(qry4["whyTrue"]);
      Assert.AreEqual(qry4["whyTrue"].ObjectValue, true);

      Assert.AreEqual(qry5.Count, 1);
      Assert.IsNotNull(qry5["qtyName"]);
      Assert.IsInstanceOf<BSONDoubleElement>(qry5["qtyName"]);
      Assert.AreEqual(qry5["qtyName"].ObjectValue, 123456.789012D);
    }
    
    [TestCase]
    public void Templatization_QuerySingleObjects()
    {  
      var qry0 = new BSONDocument("{ '$$docName': { '$$intName': '$$intValue' } }", true, 
                            new TemplateArg("docName", BSONElementType.String, "doc0"),
                            new TemplateArg("intName", BSONElementType.String, "int"), 
                            new TemplateArg("intValue", BSONElementType.Int32, int.MinValue));
      var qry1 = new BSONDocument("{ '$$docName': { '$$longName': '$$longValue' } }", true, 
                            new TemplateArg("docName", BSONElementType.String, "doc1"),
                            new TemplateArg("longName", BSONElementType.String, "long"), 
                            new TemplateArg("longValue", BSONElementType.Int64, long.MinValue));
      var qry2 = new BSONDocument("{ '$$docName': { '$$stringName': '$$stringValue' } }", true, 
                            new TemplateArg("docName", BSONElementType.String, "doc2"),
                            new TemplateArg("stringName", BSONElementType.String, "string"), 
                            new TemplateArg("stringValue", BSONElementType.String, "Hello world!"));
      var qry3 = new BSONDocument("{ '$$docName': { '$$arrayName': '$$arrayValue' } }", true, 
                            new TemplateArg("docName", BSONElementType.String, "doc3"),
                            new TemplateArg("arrayName", BSONElementType.String, "array"), 
                            new TemplateArg("arrayValue", BSONElementType.Array,
                              new BSONElement[]
                              {
                                new BSONStringElement("value"),
                                new BSONDoubleElement(-1.2345D),
                                new BSONInt32Element(2000000000)
                              }));
      var qry4 = new BSONDocument("{ '$$docName': { '$$boolName': '$$boolValue' } }", true, 
                            new TemplateArg("docName", BSONElementType.String, "doc4"),
                            new TemplateArg("boolName", BSONElementType.String, "bool"), 
                            new TemplateArg("boolValue", BSONElementType.Boolean, true));
      var qry5 = new BSONDocument("{ '$$docName': { '$$doubleName': '$$doubleValue' } }", true, 
                            new TemplateArg("docName", BSONElementType.String, "doc5"),
                            new TemplateArg("doubleName", BSONElementType.String, "double"), 
                            new TemplateArg("doubleValue", BSONElementType.Double, double.MinValue));
          
      Assert.AreEqual(qry0.Count, 1);
      Assert.IsNotNull(qry0["doc0"]);
      Assert.IsInstanceOf<BSONDocumentElement>(qry0["doc0"]);
      var doc0 = (BSONDocumentElement)qry0["doc0"];
      Assert.IsNotNull(doc0.Value);
      Assert.AreEqual(doc0.Value.Count, 1);
      Assert.IsNotNull(doc0.Value["int"]);
      Assert.IsInstanceOf<BSONInt32Element>(doc0.Value["int"]);
      Assert.AreEqual(doc0.Value["int"].ObjectValue, int.MinValue);

      Assert.AreEqual(qry1.Count, 1);
      Assert.IsNotNull(qry1["doc1"]);
      Assert.IsInstanceOf<BSONDocumentElement>(qry1["doc1"]);
      var doc1 = (BSONDocumentElement)qry1["doc1"];
      Assert.IsNotNull(doc1.Value);
      Assert.AreEqual(doc1.Value.Count, 1);
      Assert.IsNotNull(doc1.Value["long"]);
      Assert.IsInstanceOf<BSONInt64Element>(doc1.Value["long"]);
      Assert.AreEqual(doc1.Value["long"].ObjectValue, long.MinValue);

      Assert.AreEqual(qry2.Count, 1);
      Assert.IsNotNull(qry2["doc2"]);
      Assert.IsInstanceOf<BSONDocumentElement>(qry2["doc2"]);
      var doc2 = (BSONDocumentElement)qry2["doc2"];
      Assert.IsNotNull(doc2.Value);
      Assert.AreEqual(doc2.Value.Count, 1);
      Assert.IsNotNull(doc2.Value["string"]);
      Assert.IsInstanceOf<BSONStringElement>(doc2.Value["string"]);
      Assert.AreEqual(doc2.Value["string"].ObjectValue, "Hello world!");

      Assert.AreEqual(qry3.Count, 1);
      Assert.IsNotNull(qry3["doc3"]);
      Assert.IsInstanceOf<BSONDocumentElement>(qry3["doc3"]);
      var doc3 = (BSONDocumentElement)qry3["doc3"];
      Assert.IsNotNull(doc3.Value);
      Assert.AreEqual(doc3.Value.Count, 1);
      Assert.IsNotNull(doc3.Value["array"]);
      Assert.IsInstanceOf<BSONArrayElement>(doc3.Value["array"]);
      var array = ((BSONArrayElement)doc3.Value["array"]).Value;
      Assert.AreEqual(array.Length, 3);
      Assert.IsInstanceOf<BSONStringElement>(array[0]);
      Assert.IsInstanceOf<BSONDoubleElement>(array[1]);
      Assert.IsInstanceOf<BSONInt32Element>(array[2]);
      Assert.AreEqual(array[0].ObjectValue, "value");
      Assert.AreEqual(array[1].ObjectValue, -1.2345D);
      Assert.AreEqual(array[2].ObjectValue, 2000000000);

      Assert.AreEqual(qry4.Count, 1);
      Assert.IsNotNull(qry4["doc4"]);
      Assert.IsInstanceOf<BSONDocumentElement>(qry4["doc4"]);
      var doc4 = (BSONDocumentElement)qry4["doc4"];
      Assert.IsNotNull(doc4.Value);
      Assert.AreEqual(doc4.Value.Count, 1);
      Assert.IsNotNull(doc4.Value["bool"]);
      Assert.IsInstanceOf<BSONBooleanElement>(doc4.Value["bool"]);
      Assert.AreEqual(doc4.Value["bool"].ObjectValue, true); 

      Assert.AreEqual(qry5.Count, 1);
      Assert.IsNotNull(qry5["doc5"]);
      Assert.IsInstanceOf<BSONDocumentElement>(qry5["doc5"]);
      var doc5 = (BSONDocumentElement)qry5["doc5"];
      Assert.IsNotNull(doc5.Value);
      Assert.AreEqual(doc5.Value.Count, 1);
      Assert.IsNotNull(doc5.Value["double"]);
      Assert.IsInstanceOf<BSONDoubleElement>(doc5.Value["double"]);
      Assert.AreEqual(doc5.Value["double"].ObjectValue, double.MinValue);
    }

    [TestCase]
    public void Templatization_ArrayOfUnicodeStringValues()
    {
      var qry0 = new BSONDocument("{ '$$unicode': [ '$$eng', '$$rus', '$$chi', '$$jap', '$$gre', '$$alb', '$$arm', '$$vie', '$$por', '$$ukr', '$$ger' ] }", true, 
                           new TemplateArg("unicode", BSONElementType.String, "strings"), 
                           new TemplateArg("eng", BSONElementType.String, "hello"), 
                           new TemplateArg("rus", BSONElementType.String, "привет"), 
                           new TemplateArg("chi", BSONElementType.String, "你好"), 
                           new TemplateArg("jap", BSONElementType.String, "こんにちは"), 
                           new TemplateArg("gre", BSONElementType.String, "γεια σας"), 
                           new TemplateArg("alb", BSONElementType.String, "përshëndetje"),
                           new TemplateArg("arm", BSONElementType.String, "բարեւ Ձեզ"),
                           new TemplateArg("vie", BSONElementType.String, "xin chào"),
                           new TemplateArg("por", BSONElementType.String, "Olá"),
                           new TemplateArg("ukr", BSONElementType.String, "Привіт"),
                           new TemplateArg("ger", BSONElementType.String, "wünsche"));

      Assert.AreEqual(qry0.Count, 1);
      Assert.IsNotNull(qry0["strings"]);
      Assert.IsInstanceOf<BSONArrayElement>(qry0["strings"]);
      var array = ((BSONArrayElement)qry0["strings"]).Value;
      Assert.IsNotNull(array);
      Assert.AreEqual(array.Length, 11);
      Assert.AreEqual(((BSONStringElement)array[0]).Value, "hello"); 
      Assert.AreEqual(((BSONStringElement)array[1]).Value, "привет");  
      Assert.AreEqual(((BSONStringElement)array[2]).Value, "你好"); 
      Assert.AreEqual(((BSONStringElement)array[3]).Value, "こんにちは"); 
      Assert.AreEqual(((BSONStringElement)array[4]).Value, "γεια σας"); 
      Assert.AreEqual(((BSONStringElement)array[5]).Value, "përshëndetje");
      Assert.AreEqual(((BSONStringElement)array[6]).Value, "բարեւ Ձեզ");
      Assert.AreEqual(((BSONStringElement)array[7]).Value, "xin chào");
      Assert.AreEqual(((BSONStringElement)array[8]).Value, "Olá");
      Assert.AreEqual(((BSONStringElement)array[9]).Value, "Привіт");
      Assert.AreEqual(((BSONStringElement)array[10]).Value, "wünsche"); 
    }  

    [TestCase]
    public void Templatization_ArrayOfUnicodeStringNames()
    {
      var qry0 = new BSONDocument("{ '$$eng': 'eng', '$$rus': 'rus', '$$chi': 'chi', '$$jap': 'jap', '$$gre': 'gre', '$$alb': 'alb', '$$arm': 'arm', '$$vie': 'vie', '$$por': 'por', '$$ukr': 'ukr', '$$ger': 'ger' }", true, 
                           new TemplateArg("eng", BSONElementType.String, "hello"), 
                           new TemplateArg("rus", BSONElementType.String, "привет"), 
                           new TemplateArg("chi", BSONElementType.String, "你好"), 
                           new TemplateArg("jap", BSONElementType.String, "こんにちは"), 
                           new TemplateArg("gre", BSONElementType.String, "γεια σας"), 
                           new TemplateArg("alb", BSONElementType.String, "përshëndetje"),
                           new TemplateArg("arm", BSONElementType.String, "բարեւ Ձեզ"),
                           new TemplateArg("vie", BSONElementType.String, "xin chào"),
                           new TemplateArg("por", BSONElementType.String, "Olá"),
                           new TemplateArg("ukr", BSONElementType.String, "Привіт"),
                           new TemplateArg("ger", BSONElementType.String, "wünsche"));

      Assert.AreEqual(qry0.Count, 11);
      Assert.IsNotNull(qry0["hello"]);
      Assert.IsInstanceOf<BSONStringElement>(qry0["hello"]);
      Assert.AreEqual(((BSONStringElement)qry0["hello"]).Value, "eng");
       
      Assert.IsNotNull(qry0["привет"]);
      Assert.IsInstanceOf<BSONStringElement>(qry0["привет"]);
      Assert.AreEqual(((BSONStringElement)qry0["привет"]).Value, "rus"); 
           
      Assert.IsNotNull(qry0["你好"]);
      Assert.IsInstanceOf<BSONStringElement>(qry0["你好"]);
      Assert.AreEqual(((BSONStringElement)qry0["你好"]).Value, "chi"); 
               
      Assert.IsNotNull(qry0["こんにちは"]);
      Assert.IsInstanceOf<BSONStringElement>(qry0["こんにちは"]);
      Assert.AreEqual(((BSONStringElement)qry0["こんにちは"]).Value, "jap"); 
          
      Assert.IsNotNull(qry0["γεια σας"]);
      Assert.IsInstanceOf<BSONStringElement>(qry0["γεια σας"]);
      Assert.AreEqual(((BSONStringElement)qry0["γεια σας"]).Value, "gre"); 
           
      Assert.IsNotNull(qry0["përshëndetje"]);
      Assert.IsInstanceOf<BSONStringElement>(qry0["përshëndetje"]);
      Assert.AreEqual(((BSONStringElement)qry0["përshëndetje"]).Value, "alb");
           
      Assert.IsNotNull(qry0["բարեւ Ձեզ"]);
      Assert.IsInstanceOf<BSONStringElement>(qry0["բարեւ Ձեզ"]);
      Assert.AreEqual(((BSONStringElement)qry0["բարեւ Ձեզ"]).Value, "arm");
           
      Assert.IsNotNull(qry0["xin chào"]);
      Assert.IsInstanceOf<BSONStringElement>(qry0["xin chào"]);
      Assert.AreEqual(((BSONStringElement)qry0["xin chào"]).Value, "vie");
           
      Assert.IsNotNull(qry0["Olá"]);
      Assert.IsInstanceOf<BSONStringElement>(qry0["Olá"]);
      Assert.AreEqual(((BSONStringElement)qry0["Olá"]).Value, "por");
          
      Assert.IsNotNull(qry0["Привіт"]);
      Assert.IsInstanceOf<BSONStringElement>(qry0["Привіт"]);
      Assert.AreEqual(((BSONStringElement)qry0["Привіт"]).Value, "ukr");
             
      Assert.IsNotNull(qry0["wünsche"]);
      Assert.IsInstanceOf<BSONStringElement>(qry0["wünsche"]);
      Assert.AreEqual(((BSONStringElement)qry0["wünsche"]).Value, "ger"); 
    }

    [TestCase]
    public void Templatization_ComplexObjectNoTemplate()
    {
      var qry0 = new BSONDocument(
        "{" + 
          "item1: 23," + 
          "item2: [1, 'こん好արüвіт', 123.456], " + 
          "item3: { item31: false, item32: [true, true, false], item33: {} }," + 
          "item4: {" + 
            "item41: [1, 2, 3]," +
            "item42: false," +
            "item43: -123.4567," +
            "item44: 'こんこんвапаъü'," +
            "item45: { item451: [2], item452: true, item453: {} }" +
          "} "+
        "}", true);

      Assert.AreEqual(qry0.Count, 4);

      Assert.AreEqual(((BSONInt32Element)qry0["item1"]).Value, 23);

      var item2 = ((BSONArrayElement)qry0["item2"]).Value;
      Assert.AreEqual(item2.Length, 3);
      Assert.AreEqual(((BSONInt32Element)item2[0]).Value, 1);
      Assert.AreEqual(((BSONStringElement)item2[1]).Value, "こん好արüвіт");
      Assert.AreEqual(((BSONDoubleElement)item2[2]).Value, 123.456D);

      var item3 = ((BSONDocumentElement)qry0["item3"]).Value; 
      Assert.AreEqual(item3.Count, 3);
      Assert.AreEqual(((BSONBooleanElement)item3["item31"]).Value, false);
      var arr = ((BSONArrayElement)item3["item32"]).Value;        
      Assert.AreEqual(arr.Length, 3);
      Assert.AreEqual(((BSONBooleanElement)arr[0]).Value, true);
      Assert.AreEqual(((BSONBooleanElement)arr[1]).Value, true);
      Assert.AreEqual(((BSONBooleanElement)arr[2]).Value, false);
      var item33 = ((BSONDocumentElement)item3["item33"]).Value;        
      Assert.AreEqual(item33.Count, 0);

      var item4 = ((BSONDocumentElement)qry0["item4"]).Value; 
      Assert.AreEqual(item4.Count, 5);
      var item41 = ((BSONArrayElement)item4["item41"]).Value;
      Assert.AreEqual(item41.Length, 3);
      Assert.AreEqual(((BSONInt32Element)item41[0]).Value, 1);
      Assert.AreEqual(((BSONInt32Element)item41[1]).Value, 2);
      Assert.AreEqual(((BSONInt32Element)item41[2]).Value, 3);
      Assert.AreEqual(((BSONBooleanElement)item4["item42"]).Value, false); 
      Assert.AreEqual(((BSONDoubleElement)item4["item43"]).Value, -123.4567D);  
      Assert.AreEqual(((BSONStringElement)item4["item44"]).Value, "こんこんвапаъü");

      var item45 = ((BSONDocumentElement)item4["item45"]).Value; 
      Assert.AreEqual(item45.Count, 3);
      var item451 = ((BSONArrayElement)item45["item451"]).Value;
      Assert.AreEqual(item451.Length, 1);
      Assert.AreEqual(((BSONInt32Element)item451[0]).Value, 2);
      Assert.AreEqual(((BSONBooleanElement)item45["item452"]).Value, true);

      var item453 =  ((BSONDocumentElement)item45["item453"]).Value;        
      Assert.AreEqual(item453.Count, 0);
    }

    [TestCase]
    public void Templatization_QueryComplexObject()
    {
      var qry0 = new BSONDocument(
        "{" + 
          "'$$item1': 23," + 
          "item2: [1, '$$item21', 123.456], " + 
          "'$$item3': { item31: '$$false', item32: '$$array', item33: {} }," + 
          "'$$item4': {" + 
            "'$$item41': [1, 2, 3]," +
            "'$$item42': false," +
            "item43: '$$double'," +
            "item44: 'こんこんвапаъü'," +
            "item45: { item451: '$$array2', item452: true, item453: {} }" +
          "} "+
        "}", true,
        new TemplateArg("item1", BSONElementType.String, "item1"),
        new TemplateArg("item21", BSONElementType.String, "こん好արüвіт"),
        new TemplateArg("item3", BSONElementType.String, "item3"),
        new TemplateArg("false", BSONElementType.Boolean, false),
        new TemplateArg("array", BSONElementType.Array,
          new BSONElement[]
          {
            new BSONBooleanElement(true),
            new BSONBooleanElement(true),
            new BSONBooleanElement(false)
          }),
        new TemplateArg("item4", BSONElementType.String, "item4"),
        new TemplateArg("item41", BSONElementType.String, "item41"),
        new TemplateArg("item42", BSONElementType.String, "item42"),
        new TemplateArg("double", BSONElementType.Double, -123.4567),
        new TemplateArg("array2", BSONElementType.Array,
          new BSONElement[]
          {
            new BSONInt64Element(2),
          })
        );

      Assert.AreEqual(qry0.Count, 4);

      Assert.AreEqual(((BSONInt32Element)qry0["item1"]).Value, 23);

      var item2 = ((BSONArrayElement)qry0["item2"]).Value;
      Assert.AreEqual(item2.Length, 3);
      Assert.AreEqual(((BSONInt32Element)item2[0]).Value, 1);
      Assert.AreEqual(((BSONStringElement)item2[1]).Value, "こん好արüвіт");
      Assert.AreEqual(((BSONDoubleElement)item2[2]).Value, 123.456D);

      var item3 = ((BSONDocumentElement)qry0["item3"]).Value; 
      Assert.AreEqual(item3.Count, 3);
      Assert.AreEqual(((BSONBooleanElement)item3["item31"]).Value, false);
      var arr = ((BSONArrayElement)item3["item32"]).Value;        
      Assert.AreEqual(arr.Length, 3);
      Assert.AreEqual(((BSONBooleanElement)arr[0]).Value, true);
      Assert.AreEqual(((BSONBooleanElement)arr[1]).Value, true);
      Assert.AreEqual(((BSONBooleanElement)arr[2]).Value, false);
      var item33 =  ((BSONDocumentElement)item3["item33"]).Value;        
      Assert.AreEqual(item33.Count, 0);

      var item4 = ((BSONDocumentElement)qry0["item4"]).Value; 
      Assert.AreEqual(item4.Count, 5);
      var item41 = ((BSONArrayElement)item4["item41"]).Value;
      Assert.AreEqual(item41.Length, 3);
      Assert.AreEqual(((BSONInt32Element)item41[0]).Value, 1);
      Assert.AreEqual(((BSONInt32Element)item41[1]).Value, 2);
      Assert.AreEqual(((BSONInt32Element)item41[2]).Value, 3);
      Assert.AreEqual(((BSONBooleanElement)item4["item42"]).Value, false); 
      Assert.AreEqual(((BSONDoubleElement)item4["item43"]).Value, -123.4567D);  
      Assert.AreEqual(((BSONStringElement)item4["item44"]).Value, "こんこんвапаъü");

      var item45 = ((BSONDocumentElement)item4["item45"]).Value; 
      Assert.AreEqual(item45.Count, 3);
      var item451 = ((BSONArrayElement)item45["item451"]).Value;
      Assert.AreEqual(item451.Length, 1);
      Assert.AreEqual(((BSONInt64Element)item451[0]).Value, 2);
      Assert.AreEqual(((BSONBooleanElement)item45["item452"]).Value, true);

      var item453 =  ((BSONDocumentElement)item45["item453"]).Value;        
      Assert.AreEqual(item453.Count, 0);
    }

    #endregion
  }
}