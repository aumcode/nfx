using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.DataAccess;
using NFX.DataAccess.CRUD;
using NFX.Serialization.CSV;
using System.IO;

namespace NFX.NUnit.Serialization
{
  [TestFixture]
  public class CSVWriterTest
  {
   private readonly string header =
"SimpleStr,IntValue,FloatValue,DateValue,Multiline,Nullable,Quotes,Apostr,Comma\r\n";

   private readonly string data = 
"Doctor Aibolit,66,19.66,12/31/1966 19:08:59,\"Avva\r\nChichi\",,\"\"\"Barm\"\"alei\"\"\",Mc'Farlen,\"1,2,3\"\r\n";

    private TeztRow m_Row;

    [TestFixtureSetUp]
    public void Setup()
    {
      m_Row = new TeztRow
              {
                SimpleStr = "Doctor Aibolit",
                IntValue = 66,
                FloatValue = 19.66f,
                DateValue = new DateTime(1966, 12, 31, 19, 8, 59),
                NonUIValue = "nothing",
                Multiline = "Avva\r\nChichi",
                Nullable = null,
                Quotes = "\"Barm\"alei\"",
                Apostr = "Mc'Farlen",
                Comma = "1,2,3"
              };
    }


    [Test]
    public void Row_Default()
    {
      var res = CSVWriter.Write(m_Row);
      var str = header + data;
      Assert.AreEqual(str, res);
    }

    [Test]
    public void Row_TabDelimeter()
    {
      var res = CSVWriter.Write(m_Row, new CSVWritingOptions {FieldDelimiter='\t'});
      var str =
"SimpleStr\tIntValue\tFloatValue\tDateValue\tMultiline\tNullable\tQuotes\tApostr\tComma\r\n" +
"Doctor Aibolit\t66\t19.66\t12/31/1966 19:08:59\t\"Avva\r\nChichi\"\t\t\"\"\"Barm\"\"alei\"\"\"\tMc'Farlen\t1,2,3\r\n";
      Assert.AreEqual(str, res);
    }

    [Test]
    public void Row_CustomNull()
    {
      var res = CSVWriter.Write(m_Row, new CSVWritingOptions {NullValue="\0"});
      var d = 
"Doctor Aibolit,66,19.66,12/31/1966 19:08:59,\"Avva\r\nChichi\",\0,\"\"\"Barm\"\"alei\"\"\",Mc'Farlen,\"1,2,3\"\r\n";
      Assert.AreEqual(header + d, res);
    }

    [Test]
    public void Row_AllFields()
    {
      var res = CSVWriter.Write(m_Row,  CSVWritingOptions.AllFields);
      var h = 
"SimpleStr,IntValue,FloatValue,DateValue,NonUIValue,Multiline,Nullable,Quotes,Apostr,Comma\r\n";
      var d = 
"Doctor Aibolit,66,19.66,12/31/1966 19:08:59,nothing,\"Avva\r\nChichi\",,\"\"\"Barm\"\"alei\"\"\",Mc'Farlen,\"1,2,3\"\r\n";
      Assert.AreEqual(h + d, res);
    }

    [Test]
    public void Row_NoHeader()
    {
      var res = CSVWriter.Write(m_Row, CSVWritingOptions.NoHeader);
      Assert.AreEqual(data, res);
    }

    [Test]
    public void Rowset_Default()
    {
      var rowset = new Rowset(m_Row.Schema);

      rowset.Add(m_Row);
      rowset.Add(m_Row);

      var res = CSVWriter.Write(rowset);
      var str = header + data + data;
      Assert.AreEqual(str, res);
    }

    [Test]
    public void Rowset_NoHeader()
    {
      var rowset = new Rowset(m_Row.Schema);

      rowset.Add(m_Row);
      rowset.Add(m_Row);

      var res = CSVWriter.Write(rowset, CSVWritingOptions.NoHeader);
      var str = data + data;
      Assert.AreEqual(str, res);
    }

    [Test]
    public void Row_ToBuffer()
    {
      var encoding = new UTF8Encoding(false);
      var buffer = CSVWriter.WriteToBuffer(m_Row, encoding: encoding);

      var test = encoding.GetBytes(header + data);

      Assert.IsTrue(IOMiscUtils.MemBufferEquals(test, buffer));
    }

    [Test]
    public void Row_ToFile()
    {
      var name = "data.csv";

      CSVWriter.WriteToFile(m_Row, name);
      Assert.IsTrue(File.Exists(name));

      var str = header + data;
      string res = System.IO.File.ReadAllText(name);
      Assert.AreEqual(str, res);

      File.Delete(name);
    }

    [Test]
    public void NullRowOrRowset()
    {
      var res = CSVWriter.Write((Row)null);
      Assert.IsEmpty(res);

      res = CSVWriter.Write((Rowset)null);
      Assert.IsEmpty(res);
    }

    [Test]
    public void Row_AllNonWritable()
    {
      var row = new NonWritable
                {
                  FieldA = "A",
                  FieldB = "B",
                  FieldC = "C"
                };
      
      var res = CSVWriter.Write(row);
      Assert.IsEmpty(res);

      res = CSVWriter.Write(row, CSVWritingOptions.AllFields);
      var test = "FieldA,FieldB,FieldC\r\nA,B,C\r\n";
      Assert.AreEqual(test, res);
    }

    private class TeztRow : TypedRow
    {
      [Field] public string   SimpleStr { get; set; }
      [Field] public int      IntValue       { get; set; }
      [Field] public float    FloatValue     { get; set; }
      [Field] public DateTime DateValue      { get; set; }
      [Field(nonUI:true)]                    
              public string   NonUIValue     { get; set; }
      [Field] public string   Multiline      { get; set; }
      [Field] public string   Nullable       { get; set; }
      [Field] public string   Quotes     { get; set; }
      [Field] public string   Apostr         { get; set; }
      [Field] public string   Comma          { get; set; }
    }

    private class NonWritable : TypedRow
    {
      [Field(nonUI:true)] public string FieldA { get; set; }
      [Field(storeFlag: StoreFlag.None)] public string FieldB { get; set; }
      [Field(storeFlag: StoreFlag.OnlyLoad)] public string FieldC { get; set; }
    }
  }
}
