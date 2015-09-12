using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NFX.Serialization.BSON;

namespace WinFormsTest
{
  public partial class BSONTestForm : Form
  {
    public BSONTestForm()
    {
      m_Generators = new Dictionary<string, Action<Stream>>
      {
        { "EmptyDocument", WriteEmptyDocument },
        { "SingleDouble", WriteSingleDouble },
        { "SingleString", WriteSingleString },
        { "SingleInt32", WriteSingleInt32 },
        { "SingleInt64", WriteSingleInt64 },
        { "StringArray", WriteStringArray },
        { "Int32Array", WriteInt32Array },
        { "StringInt32DoubleMixedArray", WriteStringInt32DoubleMixedArray },
        { "StringAndInt32Pair", WriteStringAndInt32Pair },
        { "NestedDocument", WriteNestedDocument },
        { "SingleBinaryData", WriteSingleBinaryData },
        { "SingleObjectId", WriteSingleObjectId },
        { "SingleBooleanTrue", WriteSingleBooleanTrue },
        { "SingleBooleanFalse", WriteSingleBooleanFalse },
        { "SingleNull", WriteSingleNull },
        { "SingleDateTime", WriteSingleDateTime },
        { "SingleRegularExpression", WriteSingleRegularExpression },
        { "SingleJavaScript", WriteSingleJavaScript },
        { "SingleJavaScriptWithScope", WriteSingleJavaScriptWithScope },
        { "SingleTimestamp", WriteSingleTimestamp },
        { "SingleMinKey", WriteSingleMinKey },
        { "SingleMaxKey", WriteSingleMaxKey },  
        { "Int32Pair", WriteInt32Pair },  
        { "UnicodeStrings", WriteUnicodeStrings }, 
        { "BigIntegers", WriteBigIntegers },
      };
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      var toolTip = new ToolTip();
      toolTip.SetToolTip(sender as Button, "{} (empty document)");

      using (var stream = File.Create("test.bson"))
      {
        var root = new BSONDocument();
        root.WriteAsBSON(stream);
      }

      Process.Start(AppDomain.CurrentDomain.BaseDirectory);
    }

    private void button2_Click(object sender, EventArgs e)
    {
      var toolTip = new ToolTip();
      toolTip.SetToolTip(sender as Button, "{ greetings: \"Hello World!\" }");

      using (var stream = File.Create("test.bson"))
      {
        var root = new BSONDocument();
        root.Set(new BSONDoubleElement("pi", Math.PI));
        root.WriteAsBSON(stream);
      }
      
      Process.Start(AppDomain.CurrentDomain.BaseDirectory);
    }

    private void button3_Click(object sender, EventArgs e)
    {
      var toolTip = new ToolTip();
      toolTip.SetToolTip(sender as Button, "{ pi: 3.14159265358979 }");

      using (var stream = File.Create("test.bson"))
      {
        var root = new BSONDocument();
        root.Set(new BSONStringElement("greetings", "Hello World!"));
        root.WriteAsBSON(stream);
      }
      
      Process.Start(AppDomain.CurrentDomain.BaseDirectory);
    }

    private void button4_Click(object sender, EventArgs e)
    {
      var toolTip = new ToolTip();
      toolTip.SetToolTip(sender as Button, "{ name: \"Gagarin\", birth: 1934  }");

      using (var stream = File.Create("test.bson"))
      {
        var root = new BSONDocument();
        root.Set(new BSONStringElement("name", "Gagarin"));
        root.Set(new BSONInt32Element("birth", 1934));
        root.WriteAsBSON(stream);
      }
      
      Process.Start(AppDomain.CurrentDomain.BaseDirectory);
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("http://docs.mongodb.org/v2.6/reference/program/bsondump/");
    }

    private void button5_Click(object sender, EventArgs e)
    {
      var dir = "bsons";
      if (!Directory.Exists(dir))
        Directory.CreateDirectory(dir);

      foreach (FileInfo file in new DirectoryInfo(dir).GetFiles())
      {
        file.Delete(); 
      }

      foreach (var generator in m_Generators)
      {
        using (var file = File.Create(Path.Combine(dir, generator.Key + ".bson")))
        {
          generator.Value(file);
        }
      }

      for (int i = 0; i < 7; i++)
      {
        using (var file = File.Create(Path.Combine(dir, "StringsWithDifferentLengths" + i + ".bson")))
        {
          WriteStringsWithDifferentLengths(file, i);
        }
      }

      using (var summary = File.Create(Path.Combine(dir, "all.txt")))
      using (var writer = new StreamWriter(summary))
      {
        foreach (var generator in m_Generators.Keys)
        {
          var bytes = File.ReadAllBytes(Path.Combine(dir, generator + ".bson"));
          var base64 = Convert.ToBase64String(bytes);
          writer.Write(generator);
          writer.Write(" ");
          writer.WriteLine(base64);
        }

        for (int i = 0; i < 7; i++)
        {
          var bytes = File.ReadAllBytes(Path.Combine(dir, "StringsWithDifferentLengths" + i + ".bson"));
          var base64 = Convert.ToBase64String(bytes);
          writer.Write("StringsWithDifferentLengths" + i);
          writer.Write(" ");
          writer.WriteLine(base64);
        }
      }

      Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dir));
    }

    #region BSON sample generators

    private readonly Dictionary<string, Action<Stream>> m_Generators; 
    
    /// <summary>
    /// {} (empty document)
    /// </summary>
    public void WriteEmptyDocument(Stream stream)
    {
      var root = new BSONDocument();
      root.WriteAsBSON(stream);
    }

    /// <summary>
    /// { pi: 3.14159265358979 }
    /// </summary>
    public void WriteSingleDouble(Stream stream)
    {
      var root = new BSONDocument();
      root.Set(new BSONDoubleElement("pi", Math.PI));
      root.WriteAsBSON(stream);
    }

    /// <summary>
    /// { greetings: "Hello World!" }
    /// </summary>
    public void WriteSingleString(Stream stream)
    {
      var root = new BSONDocument();
      root.Set(new BSONStringElement("greetings", "Hello World!"));
      root.WriteAsBSON(stream);
    }

    /// <summary>
    /// { lucky: 7 }
    /// </summary>
    public void WriteSingleInt32(Stream stream)
    {
      var root = new BSONDocument();
      root.Set(new BSONInt32Element("lucky", 7));
      root.WriteAsBSON(stream);
    }

    /// <summary>
    /// { solarSystemDiameter: 10000000000000 }
    /// </summary>
    public void WriteSingleInt64(Stream stream)
    {
      var root = new BSONDocument();
      root.Set(new BSONInt64Element("solarSystemDiameter", 10000000000000));
      root.WriteAsBSON(stream);
    }

    /// <summary>
    /// Array of strings
    /// { 'fruits': ['apple, 'orange', 'plum'] } --> { 'fruits': { '0': 'apple', '1': 'orange', '2': 'plum' } }
    /// </summary>
    public void WriteStringArray(Stream stream)
    {
      var root = new BSONDocument();
      var array = new[] { new BSONStringElement("apple"), new BSONStringElement("orange"), new BSONStringElement("plum") };
      root.Set(new BSONArrayElement("fruits", array));
      root.WriteAsBSON(stream);
    }

    /// <summary>
    /// Array of int32
    /// { 'years': [1963, 1984, 2015] } --> { 'years': { '0': 1963, '1': 1984, '2': 2015 } }
    /// </summary>
    public void WriteInt32Array(Stream stream)
    {
      var root = new BSONDocument();
      var array = new[] { new BSONInt32Element(1963), new BSONInt32Element(1984), new BSONInt32Element(2015) };
      root.Set(new BSONArrayElement("years", array));
      root.WriteAsBSON(stream);
    }

    /// <summary>
    /// Array of strings
    /// { 'stuff': ['apple, 3, 2.14] } --> { 'stuff': { '0': 'apple', '1': 3, '2': 2.14 } }
    /// </summary>
    public void WriteStringInt32DoubleMixedArray(Stream stream)
    {
      var root = new BSONDocument();
      var array = new BSONElement[] { new BSONStringElement("apple"), new BSONInt32Element(3), new BSONDoubleElement(2.14D) };
      root.Set(new BSONArrayElement("stuff", array));
      root.WriteAsBSON(stream);
    }

    /// <summary>
    /// { name: "Gagarin", birth: 1934 }
    /// </summary>
    public void WriteStringAndInt32Pair(Stream stream)
    { 
      var root = new BSONDocument();
      root.Set(new BSONStringElement("name", "Gagarin"));
      root.Set(new BSONInt32Element("birth", 1934));
      root.WriteAsBSON(stream);
    }
     
    /// <summary>
    /// { name: "Gagarin", birth: 1934 }
    /// </summary>
    public void WriteInt32Pair(Stream stream)
    { 
      var root = new BSONDocument();
      root.Set(new BSONInt32Element("name", 404));
      root.Set(new BSONInt32Element("birth", 1934));
      root.WriteAsBSON(stream);
    }

    /// <summary>
    /// { nested: { capital: "Moscow" } }
    /// </summary>
    public void WriteNestedDocument(Stream stream)
    {
      var root = new BSONDocument();
      var nested = new BSONDocument();
      nested.Set(new BSONStringElement("capital", "Moscow"));
      root.Set(new BSONDocumentElement("nested", nested));
      root.WriteAsBSON(stream);
    }

    /// <summary>
    ///  { binary: <bytes from 'This is binary data'> }
    /// </summary>
    public void WriteSingleBinaryData(Stream stream)
    {
      var data = Encoding.UTF8.GetBytes("This is binary data");
      var root = new BSONDocument();
      var binary = new BSONBinary(BSONBinaryType.BinaryOld, data);
      root.Set(new BSONBinaryElement("binary", binary));
      root.WriteAsBSON(stream);
    } 
    
    /// <summary>
    /// { objectId: <bytes from hex '507f1f77bcf86cd799439011'> }
    /// </summary>
    public void WriteSingleObjectId(Stream stream)
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
    }
    
    /// <summary>
    ///  { booleanTrue: true }
    /// </summary>
    public void WriteSingleBooleanTrue(Stream stream)
    {
      var root = new BSONDocument();
      root.Set(new BSONBooleanElement("booleanTrue", true));
      root.WriteAsBSON(stream);
    }
    
    /// <summary>
    ///  { booleanFalse: false }
    /// </summary>
    public void WriteSingleBooleanFalse(Stream stream)
    {
      var root = new BSONDocument();
      root.Set(new BSONBooleanElement("booleanFalse", false));
      root.WriteAsBSON(stream);
    }
    
    /// <summary>
    /// { null: null }
    /// </summary>
    public void WriteSingleNull(Stream stream)
    {
      var root = new BSONDocument();
      root.Set(new BSONNullElement("null"));
      root.WriteAsBSON(stream);
    }

    /// <summary>
    /// { now: <DateTime from 635000000000000000 ticks> }
    /// </summary>
    public void WriteSingleDateTime(Stream stream)
    {
      var now = new DateTime(635000000000000000, DateTimeKind.Utc);
      var root = new BSONDocument();
      root.Set(new BSONDateTimeElement("now", now));
      root.WriteAsBSON(stream);
    }

    /// <summary>
    /// { email: <pattern='^[-.\w]+@(?:[a-z\d]{2,}\.)+[a-z]{2,6}$' options=I,M,U> }
    /// </summary>
    public void WriteSingleRegularExpression(Stream stream)
    {
      var root = new BSONDocument();
      var pattern = @"^[-.\w]+@(?:[a-z\d]{2,}\.)+[a-z]{2,6}$";
      var options = BSONRegularExpressionOptions.I | BSONRegularExpressionOptions.M |BSONRegularExpressionOptions.U;
      var regex = new BSONRegularExpression(pattern, options);
      root.Set(new BSONRegularExpressionElement("email", regex));
      root.WriteAsBSON(stream);
    }   

    /// <summary>
    /// { code: "function(){var x=1;var y='abc';return 1;};" }
    /// </summary>
    public void WriteSingleJavaScript(Stream stream)
    {
      var root = new BSONDocument();
      var code = "function(){var x=1;var y='abc';return 1;};";
      root.Set(new BSONJavaScriptElement("code", code));
      root.WriteAsBSON(stream);
    }

    /// <summary>
    /// { codeWithScope: "function(){var x=1;var y='abc';return z;}; <with scope: z=23>" }
    /// </summary>
    public void WriteSingleJavaScriptWithScope(Stream stream)
    {
      var root = new BSONDocument();
      var code = "function(){var x=1;var y='abc';return z;};";
      var scope = new BSONDocument();
      scope.Set(new BSONInt32Element("z", 23));
      var jsWithScope = new BSONCodeWithScope(code, scope);
      root.Set(new BSONJavaScriptWithScopeElement("codeWithScope", jsWithScope));
      root.WriteAsBSON(stream);
    } 

    /// <summary>
    /// { stamp: <seconds since Unix epoch to DateTime from 635000000000000000 ticks with 123 increment> }
    /// </summary>
    public void WriteSingleTimestamp(Stream stream)
    {
      var root = new BSONDocument();
      var now = new DateTime(635000000000000000, DateTimeKind.Utc);
      var stamp = new BSONTimestamp(now, 123);
      root.Set(new BSONTimestampElement("stamp", stamp));
      root.WriteAsBSON(stream);
    } 
    
    /// <summary>
    /// { minkey: <minkey> }
    /// </summary>
    public void WriteSingleMinKey(Stream stream)
    {
      var root = new BSONDocument();
      root.Set(new BSONMinKeyElement("minkey"));
      root.WriteAsBSON(stream);
    } 
    
    /// <summary>
    /// { maxkey: <maxkey> }
    /// </summary>
    public void WriteSingleMaxKey(Stream stream)
    {
      var root = new BSONDocument();
      root.Set(new BSONMaxKeyElement("maxkey"));
      root.WriteAsBSON(stream);
    }

    public void WriteStringsWithDifferentLengths(Stream stream, int count)
    {
      stream.Position = 0;
      var root = new BSONDocument();
      var value = new string('a', count);
      root.Set(new BSONStringElement("vary", value));
      root.WriteAsBSON(stream);
    } 

    public void WriteUnicodeStrings(Stream stream)
    {
      stream.Position = 0;
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
    }   

    public void WriteBigIntegers(Stream stream)
    {
      stream.Position = 0;
      var root = new BSONDocument();
      root.Set(new BSONInt32Element("intMin", int.MinValue));
      root.Set(new BSONInt32Element("intMax", int.MaxValue));
      root.Set(new BSONInt64Element("longMin", long.MinValue));
      root.Set(new BSONInt64Element("longMax", long.MaxValue));
      root.WriteAsBSON(stream);
    }

    #endregion

    private void button6_Click(object sender, EventArgs e)
    {
      var message = string.Empty;
      var value = 1999;
      var timer = new Stopwatch();
      using (var stream = new MemoryStream())
      {
        timer.Start();
        var buffer = new byte[4];
        for (int i=0; i<10000000; i++)
        {
          buffer[0] = (byte)value;
          buffer[1] = (byte)(value >> 8); 
          buffer[2] = (byte)(value >> 16); 
          buffer[3] = (byte)(value >> 24);
          stream.Write(buffer, 0, 4);
        }  
        timer.Stop();
        message += "Writing int as byte array to stream (10000000 iters): " + timer.ElapsedMilliseconds;
      }

      timer.Reset();
      using (var stream = new MemoryStream())
      {
        timer.Start();
        for (int i=0; i<10000000; i++)
        {
          stream.WriteByte((byte)value);
          stream.WriteByte((byte)(value >> 8));
          stream.WriteByte((byte)(value >> 16));
          stream.WriteByte((byte)(value >> 34));
        }  
        timer.Stop();
        message +=  Environment.NewLine + "Writing int byte by byte to stream (10000000 iters): " + timer.ElapsedMilliseconds;
      }

      label3.Text = message;
    }
  }
}
