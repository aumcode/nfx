/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
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
using System.Threading.Tasks;

using NFX;
using NFX.DataAccess.CRUD;
using NFX.Security;
using NFX.Serialization.JSON;
using NFX.Wave.MVC;
using System.IO;

namespace WaveTestSite.Controllers
{
  public class IntegrationTester: Controller
  {
    #region Nested classes

      public enum TestStatus { Ok, Err};

      public class SpanRow: TypedRow
      {
        [Field] public TimeSpan Span { get; set; }
      }

      public class TestRow: TypedRow
      {
        [Field] public int ID { get; set; }
        [Field] public string Name { get; set; }

        [Field] public DateTime Date { get; set; }
        [Field] public DateTime? DateNullable { get; set; }

        [Field] public TimeSpan Span { get; set; }
        [Field] public TimeSpan? SpanNullable { get; set; }

        [Field] public TestStatus Status { get; set; }
        [Field] public TestStatus? StatusNullable { get; set; }

        [Field] public bool Is { get; set; }
        [Field] public bool? IsNullable { get; set; }

        [Field] public decimal Money { get; set; }
        [Field] public decimal? MoneyNullable { get; set; }

        [Field] public float Float { get; set; }
        [Field] public float? FloatNullable { get; set; }

        [Field] public double Double { get; set; }
        [Field] public double? DoubleNullable { get; set; }
      }

      public class TestComplexRow: TypedRow
      {
        [Field] public int ID { get; set; }
        [Field] public TestRow Row1 { get; set; }
        [Field] public TestRow Row2 { get; set; }
        [Field] public TestRow[] ErrorRows { get; set; }
      }
        
    #endregion

    [Action]
    public string Empty()
    {
      return string.Empty;
    }

    [Action("ActionName1", 0)]
    public string ActionName0()
    {
      return "ActionName1";
    }

    [Action("ActionGet", 0, "match{methods=GET}")]
    public string ActionGet()
    {
      return "ActionGet";
    }

    [Action("ActionPost", 0, "match{methods=POST}")]
    public string ActionPost()
    {
      return "ActionPost";
    }

    [Action("IsLocalAction", 0, "match{is-local=true}")]
    public string IsLocalAction()
    {
      return "IsLocalAction";
    }

    [Action("IsNotLocalAction", 0, "match{is-local=false}")]
    public string IsNotLocalAction()
    {
      return "IsNotLocalAction";
    }

    [Action]
    public string Add(int a, int b)
    {
      return (a + b).ToString();
    }

    [Action]
    public string AddDefault(int a = 5, int b = 7)
    {
      return (a + b).ToString();
    }

    [Action]
    public object GetList()
    {
      return new List<int>(new int[] {1, 2, 3});
    }

    [Action]
    public object GetArray()
    {
      return new int[] {1, 2, 3};
    }

    [Action]
    public string GetSetTimeSpan(TimeSpan ts)
    {
      return ts.Add(TimeSpan.FromDays(1)).ToString();
    }

    [Action]
    [AdHocPermission("TestPath", "TestPermission", 10)]
    public object GetWithPermission()
    {
      return string.Empty;
    }

    [Action]
    public object InboundJSONMapEcho(JSONDataMap data)
    {
      return data;
    }

    [Action]
    public object GetAnonymousObject()
    {
      return new { ID=55, Name="test"};
    }

    [Action("RowGet", 0, "match{methods=GET}")]
    public object RowGet()
    {
      var row = new TestRow(){
        ID = 777,
        Name = "Test Name",
        Date = DateTime.Now,
        Status = TestStatus.Ok,
        Span = TimeSpan.FromSeconds(1)
      };

      return row;
    }

    [Action("RowSet", 0, "match{methods=POST}")]
    public object RowSet(TestRow row)
    {
      row.Date = DateTime.Now;
      return row;
    }

    [Action("ComplexRowSet", 0, "match{methods=POST}")]
    public object ComplexRowSet(TestComplexRow row)
    {
      row.ID += 1;
      row.Row1.ID += 2;
      row.ErrorRows[2].Date = row.ErrorRows[2].Date.AddDays(-2);
      return row;//new JSONResult(row, JSONWritingOptions.CompactRowsAsMap);
    }

    [Action]
    public object RowAndPrimitive_RowFirst(TestRow row, int n, string s)
    {
      row.ID = n;
      row.Name = s;
      return row;//new JSONResult(row, JSONWritingOptions.CompactRowsAsMap);
    }

    [Action]
    public object RowAndPrimitive_RowLast(int n, string s, TestRow row)
    {
      row.ID = n;
      row.Name = s;
      return row;//new JSONResult(row, JSONWritingOptions.CompactRowsAsMap);
    }

    [Action]
    public object RowAndPrimitive_RowMiddle(int n, TestRow row, string s)
    {
      row.ID = n;
      row.Name = s;
      return row;//new JSONResult(row, JSONWritingOptions.CompactRowsAsMap);
    }

    [Action]
    public object JSONMapAndPrimitive_JSONFirst(JSONDataMap map, int n, string s)
    {
      map["ID"] = n;
      map["Name"] = s;
      return map; // or you could write: new JSONResult(map, JSONWritingOptions.CompactRowsAsMap);
    }

    [Action]
    public object JSONMapAndPrimitive_JSONLast(int n, string s, JSONDataMap map)
    {
      map["ID"] = n;
      map["Name"] = s;
      return map;//new JSONResult(map, JSONWritingOptions.CompactRowsAsMap);
    }

    [Action]
    public object JSONMapAndPrimitive_JSONMiddle(int n, JSONDataMap map, string s)
    {
      map["ID"] = n;
      map["Name"] = s;
      return map;//new JSONResult(map, JSONWritingOptions.CompactRowsAsMap);
    }

    [Action]
    public object RowDifferentFieldTypes(TestRow row)
    {
      return row;// new JSONResult(row, JSONWritingOptions.CompactRowsAsMap);
    }

    //[Action]
    //public object DoSomething(bool? a1, TestStatus b1)
    //{
    //  var a = WorkContext.MatchedVars["a"].AsBool();
    //  var b = WorkContext.MatchedVars["b"].AsEnum<TestStatus>(TestStatus.Ok);

    //  return new FileDownload("1.txt");
    //}

    [Action]
    public object LoginUser(string id, string pwd)
    {
      WorkContext.NeedsSession();

      WorkContext.Session.User = App.SecurityManager.Authenticate(new IDPasswordCredentials(id, pwd));

      return new {Status = WorkContext.Session.User.Status, Name = WorkContext.Session.User.Name};
    }

    [Action]
    public DateTime? RelaxedDateTime(DateTime? dt = null)
    {
      return dt;
    }

    [Action("StrictDateTime", 1, true)]
    public DateTime? StrictDateTime(DateTime? dt = null)
    {
      return dt;
    }


    [Action]
    public void MultipartByteArray(string field, string text, byte[] bin)
    {
      var fld = Encoding.UTF8.GetBytes(field);
      var txt = Encoding.UTF8.GetBytes(text);
      var output = WorkContext.Response.GetDirectOutputStreamForWriting();
      output.Write(fld, 0, fld.Length);
      output.Write(txt, 0, txt.Length);
      output.Write(bin, 0, bin.Length);
    }

    [Action]
    public void MultipartMap(JSONDataMap map)
    {
      var fld = Encoding.UTF8.GetBytes(map["field"].AsString());
      var txt = Encoding.UTF8.GetBytes(map["text"].AsString());
      var bin = map["bin"] as byte[];
      var output = WorkContext.Response.GetDirectOutputStreamForWriting();
      output.Write(fld, 0, fld.Length);
      output.Write(txt, 0, txt.Length);
      output.Write(bin, 0, bin.Length);
    }

    [Action]
    public void MultipartRow(MultipartTestRow row)
    {
      var fld = Encoding.UTF8.GetBytes(row.Field);
      var txt = Encoding.UTF8.GetBytes(row.Text);
      var bin = row.Bin;
      var output = WorkContext.Response.GetDirectOutputStreamForWriting();
      output.Write(fld, 0, fld.Length);
      output.Write(txt, 0, txt.Length);
      output.Write(bin, 0, bin.Length);
    }

    [Action]
    public void MultipartStream(string field, string text, Stream bin)
    {
      var fld = Encoding.UTF8.GetBytes(field);
      var txt = Encoding.UTF8.GetBytes(text);
      var output = WorkContext.Response.GetDirectOutputStreamForWriting();
      output.Write(fld, 0, fld.Length);
      output.Write(txt, 0, txt.Length);
      bin.CopyTo(output);
    }

    [Action]
    public void MultipartEncoding(string field)
    {
      var fld = Encoding.GetEncoding(1251).GetBytes(field);
      WorkContext.Response.GetDirectOutputStreamForWriting().Write(fld, 0, fld.Length);
    }

    //protected override System.Reflection.MethodInfo FindMatchingAction(NFX.Wave.WorkContext work, string action, out object[] args)
    //{
    //  return base.FindMatchingAction(work, action, out args);
    //}

    //multipart (byte array as well)
    //public object RowSet(different data types: decimal, bool, float, double, DateTime, TimeSpan and their Nullable versions)
    //public object RowSet(JSONDataMap row, int a, string b)
    //public object RowSet(int a, string b, JSONDataMap row)
    //public object RowSet(TestRow row, int a, string b)
    //match{is-local=false}
    
    public class MultipartTestRow : TypedRow
    {
      [Field]
      public string Field { get; set;}

      [Field]
      public string Text { get; set;}

      [Field]
      public string Text_filename { get; set;}

      [Field]
      public string Text_contenttype { get; set;}

      [Field]
      public byte[] Bin { get; set;}

      [Field]
      public string Bin_filename { get; set;}

      [Field]
      public string Bin_contenttype { get; set;}
    }
  }
}
