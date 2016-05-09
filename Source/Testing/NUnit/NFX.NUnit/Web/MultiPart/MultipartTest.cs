using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

using NFX.Web;

namespace NFX.NUnit.Web.MultiPart
{
  [TestFixture]
  public class MultipartTest
  {
    private const string POSTFIX_CONTENT_TYPE = "_contenttype";
    private const string POSTFIX_FILENAME = "_filename";
    private static readonly byte[] BYTES =
      { 0x12, 0x34, 0xaa, 0xfe, 0x10, 0x24, 0x1a, 0xfd,
          0x34, 0x00, 0x00, 0x2d, 0x2d, 0x27, 0x2a, 0xff,
          0x7f, 0x8a, 0x2d, 0x2d, 0x0d, 0x0a, 0x55, 0x49,
          0x0d, 0x0a, 0x75, 0xdb, 0x1e, 0x84, 0xf0, 0x63 };

    [Test]
    public void EncodeDecode_OneField()
    {
      var part = getDefaultField();

      var mpE = new Multipart(new Multipart.Part[] { part });
      var encCont = mpE.Encode();

      string boundary = null;
      var mpD = Multipart.ReadFromBytes(encCont.Buffer, ref boundary);

      Assert.AreEqual(encCont.Boundary, boundary);
      Assert.AreEqual(1, mpD.Parts.Count);
      Assert.AreEqual(part.Content.AsString(), mpD.Parts[part.Name].Content.AsString());
      Assert.IsNull(mpD.Parts[part.Name].ContentType);
      Assert.IsNull(mpD.Parts[part.Name].FileName);
    }

    [Test]
    public void EncodeDecode_OneField_Enc1251()
    {
      var encoding = Encoding.GetEncoding(1251);
      var part = new Multipart.Part("Field");
      part.Content = "Значение";

      var mpE = new Multipart(new Multipart.Part[] { part });
      var encCont = mpE.Encode(encoding);

      string boundary = null;
      var mpD = Multipart.ReadFromBytes(encCont.Buffer, ref boundary, encoding);

      Assert.AreEqual(encCont.Boundary, boundary);
      Assert.AreEqual(encCont.Encoding, encoding);
      Assert.AreEqual(1, mpD.Parts.Count);
      Assert.AreEqual(part.Content.AsString(), mpD.Parts[part.Name].Content.AsString());
      Assert.IsNull(mpD.Parts[part.Name].ContentType);
      Assert.IsNull(mpD.Parts[part.Name].FileName);
    }

    [Test]
    public void EncodeDecode_OneFile()
    {
      var part = getDefaultFile();

      var mpE = new Multipart(new Multipart.Part[] { part });
      var encCont = mpE.Encode();

      string boundary = null;
      var mpD = Multipart.ReadFromBytes(encCont.Buffer, ref boundary);

      Assert.AreEqual(encCont.Boundary, boundary);
      Assert.AreEqual(1, mpD.Parts.Count);
      Assert.IsTrue(IOMiscUtils.MemBufferEquals(part.Content as byte[], mpD.Parts[part.Name].Content as byte[]));
      Assert.AreEqual(part.FileName, mpD.Parts[part.Name].FileName);
      Assert.AreEqual(part.ContentType, mpD.Parts[part.Name].ContentType);
    }

    [Test]
    public void EncodeDecode_FieldAndFile()
    {
      var partField = getDefaultField();
      var partFile = getDefaultFile();

      var mpE = new Multipart(new Multipart.Part[] { partField, partFile });
      var encCont = mpE.Encode();

      string boundary = null;
      var mpD = Multipart.ReadFromBytes(encCont.Buffer, ref boundary);

      Assert.AreEqual(encCont.Boundary, boundary);
      Assert.AreEqual(2, mpD.Parts.Count);

      Assert.AreEqual(partField.Content.AsString(), mpD.Parts[partField.Name].Content.AsString());
      Assert.IsNull(mpD.Parts[partField.Name].ContentType);
      Assert.IsNull(mpD.Parts[partField.Name].FileName);
      Assert.AreEqual(partFile.FileName, mpD.Parts[partFile.Name].FileName);
      Assert.AreEqual(partFile.ContentType, mpD.Parts[partFile.Name].ContentType);
      Assert.IsTrue(IOMiscUtils.MemBufferEquals(partFile.Content as byte[], mpD.Parts[partFile.Name].Content as byte[]));
    }

    [Test]
    public void EncodeToStreamDecode()
    {
      var partField = getDefaultField();
      var partFile = getDefaultFile();

      var stream = new MemoryStream();
      stream.WriteByte(0xFF);
      stream.WriteByte(0xFF);

      var mpE = new Multipart(new Multipart.Part[] { partField, partFile });
      var enc = mpE.Encode(stream);
      Assert.AreEqual(enc.StartIdx, 2);

      var src = new byte[enc.Length];
      Array.Copy(enc.Buffer, enc.StartIdx, src, 0, src.Length);

      string boundary = null;
      var mpD = Multipart.ReadFromBytes(src, ref boundary);

      Assert.AreEqual(enc.Boundary, boundary);
      Assert.AreEqual(partField.Content.AsString(), mpD.Parts[partField.Name].Content.AsString());
      Assert.IsNull(mpD.Parts[partField.Name].ContentType);
      Assert.IsNull(mpD.Parts[partField.Name].FileName);
      Assert.AreEqual(partFile.FileName, mpD.Parts[partFile.Name].FileName);
      Assert.AreEqual(partFile.ContentType, mpD.Parts[partFile.Name].ContentType);
      Assert.IsTrue(IOMiscUtils.MemBufferEquals(partFile.Content as byte[], mpD.Parts[partFile.Name].Content as byte[]));
    }

    [Test]
    public void FieldAndFile_ToJSONDataMap()
    {
      var partField = getDefaultField();
      var partFile = getDefaultFile();

      var mp = new Multipart(new Multipart.Part[] { partField, partFile });
      Assert.AreEqual(2, mp.Parts.Count);

      var map = mp.ToJSONDataMap();

      Assert.AreEqual(partField.Content.AsString(), map[partField.Name].AsString());
      Assert.AreEqual(partFile.FileName, map[partFile.Name + POSTFIX_FILENAME].AsString());
      Assert.AreEqual(partFile.ContentType, map[partFile.Name + POSTFIX_CONTENT_TYPE].AsString());
      Assert.IsTrue(IOMiscUtils.MemBufferEquals(partFile.Content as byte[], map[partFile.Name] as byte[]));
    }

    [Test]
    public void ReadFromBytes()
    {
      var test = getEmbeddedFileBytes("test.dat");
      string boundary = null;
      var mp = Multipart.ReadFromBytes(test, ref boundary);
      Assert.AreEqual(3, mp.Parts.Count);
      Assert.AreEqual("---------------------------7de31e2f5200d8", boundary);

      Assert.AreEqual("value", mp.Parts["name"].Content.AsString());
      Assert.IsNull(mp.Parts["name"].ContentType);
      Assert.IsNull(mp.Parts["name"].FileName);

      var bmp = getEmbeddedFileBytes("bmp.dat");
      Assert.IsTrue(IOMiscUtils.MemBufferEquals(bmp, mp.Parts["content1"].Content as byte[]));
      Assert.AreEqual("image/bmp", mp.Parts["content1"].ContentType);
      Assert.AreEqual("bmp.dat", mp.Parts["content1"].FileName);

      var txt = getEmbeddedFileBytes("txt.dat");
      Assert.AreEqual(Encoding.UTF8.GetString(txt), mp.Parts["content2"].Content.AsString());
      Assert.AreEqual("text/plain", mp.Parts["content2"].ContentType);
      Assert.AreEqual("text.dat", mp.Parts["content2"].FileName);
    }

    [Test]
    public void ReadAndToMap()
    {
      var test = getEmbeddedFileBytes("test.dat");
      string boundary = null;
      var mp = Multipart.ReadFromBytes(test, ref boundary);

      var map = mp.ToJSONDataMap();

      Assert.AreEqual(7, map.Count);

      Assert.AreEqual("value", map["name"].AsString());

      var bmp = getEmbeddedFileBytes("bmp.dat");
      Assert.IsTrue(IOMiscUtils.MemBufferEquals(bmp, map["content1"] as byte[]));
      Assert.AreEqual("image/bmp", map["content1" + POSTFIX_CONTENT_TYPE].AsString());
      Assert.AreEqual("bmp.dat", map["content1" + POSTFIX_FILENAME].AsString());

      var txt = getEmbeddedFileBytes("txt.dat");
      Assert.AreEqual(Encoding.UTF8.GetString(txt), map["content2"].AsString());
      Assert.AreEqual("text/plain", map["content2" + POSTFIX_CONTENT_TYPE].AsString());
      Assert.AreEqual("text.dat", map["content2" + POSTFIX_FILENAME].AsString());
    }

    [Test]
    public void ValidBoundaryFromOutside()
    {
      var test = Encoding.UTF8.GetBytes(
@"--7de23d3b1d07fe
Content-Disposition: form-data; name=""name1""

value 1
--7de23d3b1d07fe
Content-Disposition: form-data; name=""name2""

value 2
--7de23d3b1d07fe--
");
      string boundary = "7de23d3b1d07fe";
      var mp = Multipart.ReadFromBytes(test, ref boundary);

      Assert.AreEqual(2, mp.Parts.Count);
      Assert.AreEqual("7de23d3b1d07fe", boundary);

      Assert.AreEqual("value 1", mp.Parts["name1"].Content.AsString());
      Assert.IsNull(mp.Parts["name1"].ContentType);
      Assert.IsNull(mp.Parts["name1"].FileName);

      Assert.AreEqual("value 2", mp.Parts["name2"].Content.AsString());
      Assert.IsNull(mp.Parts["name2"].ContentType);
      Assert.IsNull(mp.Parts["name2"].FileName);
    }

    [Test]
    public void ValidBoundaryFromOutside_Preamble()
    {
      var test = Encoding.UTF8.GetBytes(
@"This is a preamble
--7de23d3b1d07fe
Content-Disposition: form-data; name=""name1""

value 1
--7de23d3b1d07fe
Content-Disposition: form-data; name=""name2""

value 2
--7de23d3b1d07fe--
");
      string boundary = "7de23d3b1d07fe";
      var mp = Multipart.ReadFromBytes(test, ref boundary);

      Assert.AreEqual(2, mp.Parts.Count);
      Assert.AreEqual("7de23d3b1d07fe", boundary);

      Assert.AreEqual("value 1", mp.Parts["name1"].Content.AsString());
      Assert.IsNull(mp.Parts["name1"].ContentType);
      Assert.IsNull(mp.Parts["name1"].FileName);

      Assert.AreEqual("value 2", mp.Parts["name2"].Content.AsString());
      Assert.IsNull(mp.Parts["name2"].ContentType);
      Assert.IsNull(mp.Parts["name2"].FileName);
    }

    [Test]
    public void Charset1251()
    {
      var part = new Multipart.Part("field");
      part.FileName = "text";
      part.ContentType = "Content-type: text/plain; charset=windows-1251";
      part.Content = Encoding.GetEncoding(1251).GetBytes("Значение");

      var mpE = new Multipart(new Multipart.Part[] {part});
      var enc = mpE.Encode();

      var boundary = enc.Boundary;
      var mpD = Multipart.ReadFromBytes(enc.Buffer, ref boundary);
      Assert.AreEqual("Значение", mpD.Parts["field"].Content);
    }

    [Test]
    public void MissCharset1251()
    {
      var part = new Multipart.Part("field");
      part.FileName = "text";
      part.ContentType = "Content-type: text/plain";
      part.Content = Encoding.GetEncoding(1251).GetBytes("Значение");

      var mpE = new Multipart(new Multipart.Part[] {part});
      var enc = mpE.Encode();

      var boundary = enc.Boundary;
      var mpD = Multipart.ReadFromBytes(enc.Buffer, ref boundary);
      Assert.AreNotEqual("Значение", mpD.Parts["field"].Content);
    }

    #region Exceptions

    [Test]
    public void TryCreatePart_NullName()
    {
      try
      {
        var part = new Multipart.Part(null);
        Assert.Fail("Invalid name!");
      }
      catch (Exception e)
      {
        Assert.IsTrue(e.Message.Contains(NFX.Web.StringConsts.MULTIPART_PART_EMPTY_NAME_ERROR));
      }
    }

    [Test]
    public void TryCreateMultipart_NullParts()
    {
      try
      {
        var part = new Multipart(null);
        Assert.Fail("Invalid parts!");
      }
      catch (Exception e)
      {
        Assert.IsTrue(e.Message.Contains(NFX.Web.StringConsts.MULTIPART_PARTS_COULDNT_BE_EMPTY_ERROR));
      }
    }

    [Test]
    public void EmptyPart()
    {
      var test = Encoding.UTF8.GetBytes(
@"-----------------------------7de23d3b1d07fe
-----------------------------7de23d3b1d07fe--
");
      string boundary = null;
      try
      {
        var mp = Multipart.ReadFromBytes(test, ref boundary);
        Assert.Fail("Empty part!");
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        Assert.IsTrue(e.Message.Contains(NFX.Web.StringConsts.MULTIPART_PART_COULDNT_BE_EMPTY_ERROR));
      }
    }

    [Test]
    public void EmptyName()
    {
      var test = Encoding.UTF8.GetBytes(
@"-----------------------------7de23d3b1d07fe
Content-Disposition: form-data; name=""""

value
-----------------------------7de23d3b1d07fe--
");
      string boundary = null;
      try
      {
        var mp = Multipart.ReadFromBytes(test, ref boundary);
        Assert.Fail("Empty name!");
      }
      catch (Exception e)
      {
        Assert.IsTrue(e.Message.Contains(NFX.Web.StringConsts.MULTIPART_PART_EMPTY_NAME_ERROR));
      }
    }

    [Test]
    public void InvalidEOL()
    {
      var test = Encoding.UTF8.GetBytes(
@"-----------------------------7de23d3b1d07fe
Content-Disposition: form-data; name=""name""

value-----------------------------7de23d3b1d07fe--
");
      string boundary = null;
      try
      {
        var mp = Multipart.ReadFromBytes(test, ref boundary);
        Assert.Fail("Invalid end of part!");
      }
      catch (Exception e)
      {
        Assert.IsTrue(e.Message.Contains(NFX.Web.StringConsts.MULTIPART_PART_MUST_BE_ENDED_WITH_EOL_ERROR));
      }
    }

    [Test]
    public void NoDoubleEOLAfterHeader()
    {
      var test = Encoding.UTF8.GetBytes(
@"-----------------------------7de23d3b1d07fe
Content-Disposition: form-data; name=""name""
value
-----------------------------7de23d3b1d07fe--
");
      string boundary = null;
      try
      {
        var mp = Multipart.ReadFromBytes(test, ref boundary);
        Assert.Fail("No double EOL after header!");
      }
      catch (Exception e)
      {
        Assert.IsTrue(e.Message.Contains(NFX.Web.StringConsts.MULTIPART_DOUBLE_EOL_ISNT_FOUND_AFTER_HEADER_ERROR));
      }
    }

    [Test]
    public void RepeatName()
    {
      var test = Encoding.UTF8.GetBytes(
@"-----------------------------7de23d3b1d07fe
Content-Disposition: form-data; name=""name""

value 1
-----------------------------7de23d3b1d07fe
Content-Disposition: form-data; name=""name""

value 2
-----------------------------7de23d3b1d07fe--
");
      string boundary = null;
      try
      {
        var mp = Multipart.ReadFromBytes(test, ref boundary);
        Assert.Fail("Repeated name!");
      }
      catch (Exception e)
      {
        Assert.IsTrue(e.Message.Contains("is already registered."));
      }
    }

    [Test]
    public void InvalidBoundaryFromOutside()
    {
      var test = Encoding.UTF8.GetBytes(
@"--7de23d3b1d07fe
Content-Disposition: form-data; name=""name1""

value 1
--7de23d3b1d07fe
Content-Disposition: form-data; name=""name2""

value 2
--7de23d3b1d07fe--
");
      string boundary = "8asd56sge";
      try
      {
        var mp = Multipart.ReadFromBytes(test, ref boundary);
        Assert.Fail("Invalid explicit boundary");
      }
      catch (Exception ex)
      {
        Assert.IsTrue(ex.Message.Contains(NFX.Web.StringConsts.MULTIPART_SPECIFIED_BOUNDARY_ISNT_FOUND_ERROR));
      }
    }

    [Test]
    public void TooShortBoundary()
    {
      var test = Encoding.UTF8.GetBytes(
@"--
Content-Disposition: form-data; name=""name""

value 1
----
");
      string boundary = null;
      try
      {
        var mp = Multipart.ReadFromBytes(test, ref boundary);
        Assert.Fail("Invalid boundary");
      }
      catch (Exception e)
      {
        Assert.IsTrue(e.Message.Contains(NFX.Web.StringConsts.MULTIPART_BOUNDARY_IS_TOO_SHORT));
      }
    }

    [Test]
    public void FullBoundaryNotStartWithHyphens()
    {
      var test = Encoding.UTF8.GetBytes(
@"7de23d3b1d07fe
Content-Disposition: form-data; name=""name""

value 1
7de23d3b1d07fe--
");
      string boundary = null;
      try
      {
        var mp = Multipart.ReadFromBytes(test, ref boundary);
        Assert.Fail("Invalid boundary");
      }
      catch (Exception e)
      {
        Assert.IsTrue(e.Message.Contains(NFX.Web.StringConsts.MULTIPART_BOUNDARY_SHOULD_START_WITH_HYPHENS));
      }
    }

    [Test]
    public void InvalidEOF()
    {
      var test = Encoding.UTF8.GetBytes(
@"-----------------------------7de23d3b1d07fe
Content-Disposition: form-data; name=""field1""

value 1
-----------------------------7de23d3b1d07fe
Content-Disposition: form-data; name=""field2""

value 2--
");
      string boundary = null;
      try
      {
        var mp = Multipart.ReadFromBytes(test, ref boundary);
        Assert.Fail("Invalid EOF!");
      }
      catch (Exception e)
      {
        Assert.IsTrue(e.Message.Contains(NFX.Web.StringConsts.MULTIPART_TERMINATOR_ISNT_FOUND_ERROR));
      }
    }

    #endregion

    private Multipart.Part getDefaultField()
    {
      var part = new Multipart.Part("SomeField");
      part.Content = "Field's value";
      return part;
    }

    private Multipart.Part getDefaultFile()
    {
      var part = new Multipart.Part("SomeFile");
      part.Content = BYTES;
      part.FileName = "five_numbers.dat";
      part.ContentType = "application/octet-stream";
      return part;
    }

    private byte[] getEmbeddedFileBytes(string resourceName)
    {
      var resourceFullName = "Resources." + resourceName;
      using (var stream = NFX.EmbeddedResource.GetBinaryStream(typeof(MultipartTest), resourceFullName))
      {
        var buf = new byte[stream.Length];
        stream.Read(buf, 0, buf.Length);
        return buf;
      }
    }
  }
}
