using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using NFX.DataAccess;
using NFX.DataAccess.CRUD;
using NFX.Serialization.JSON;
using NFX.Wave;
using NFX.Wave.Client;

namespace NFX.NUnit.Web.Client
{
  [TestFixture]
  public class RecordTest
  {
    [Test]
    public void Record_ValidInitJSON()
    {
      var json = 
@"{
  'OK': true,
  'ID': '39a833dd-b48f-46c1-83a6-96603cc962a6',
  'ISOLang': 'eng',
  '__FormMode': 'Edit',
  '__CSRFToken': '1kk_qzXPLyScAa2A5y5GLTo9IlCqjuP',
  '__Roundtrip': '{\'GDID\':\'0:1:5\'}',
  'fields': [
      {
        'def': {
          'Name': 'Mnemonic',
          'Type': 'string',
          'Description': 'Mnemonic',
          'Required': true,
          'MinSize': 1,
          'Size': 25,
          'Placeholder': 'Mnemonic',
          'Stored': false
        },
        'val': 'Dns'
      },
      {
        'def': {
          'Name': 'Vertical_ID',
          'Type': 'string',
          'Description': 'Vertical',
          'Required': false,
          'Visible': false,
          'Size': 15,
          'DefaultValue': 'HAB',
          'Key': true,
          'Case': 'Upper',
          'LookupDict': {
            'HAB': 'HAB.rs Health and Beauty',
            'READRS': 'READ.rs Book Business',
            'SLRS': 'SL.RS General Business'
          }
        },
        'val': 'HAB'
      },
      {
        'def': {
          'Name': 'Table_ID',
          'Type': 'int',
          'Key': true,
          'Description': 'Table',
          'Required': true,
          'Visible': false,
          'MinValue': 1,
          'MaxValue': 123,
          'DefaultValue': 15,
          'Kind': 'Number',
          'Stored': true
        },
        'val': 2
      }
    ]
}";

      var init = JSONReader.DeserializeDataObject(json) as JSONDataMap;
      var rec = new Record(init);
      
      Assert.AreEqual(0, rec.ServerErrors.Count());

      Assert.AreEqual(true, rec.OK);
      Assert.AreEqual("eng", rec.ISOLang);
      Assert.AreEqual(new Guid("39a833dd-b48f-46c1-83a6-96603cc962a6"), rec.ID);
      Assert.AreEqual("Edit", rec.FormMode);
      Assert.AreEqual("1kk_qzXPLyScAa2A5y5GLTo9IlCqjuP", rec.CSRFToken);

      var roundtrip = rec.Roundtrip;
      Assert.IsNotNull(roundtrip);
      Assert.AreEqual(roundtrip.Count, 1);
      Assert.AreEqual("0:1:5", roundtrip["GDID"]);

      Assert.AreEqual(3, rec.Schema.FieldCount);

      var fdef = rec.Schema["Mnemonic"];
      Assert.IsNotNull(fdef);
      Assert.AreEqual("Mnemonic", fdef.Name);
      Assert.AreEqual(typeof(string), fdef.Type);
      Assert.AreEqual(false, fdef.AnyTargetKey);
      Assert.IsNotNull(fdef.Attrs);
      Assert.AreEqual(1, fdef.Attrs.Count());
      var attr = fdef.Attrs.First();
      Assert.AreEqual("Mnemonic", attr.Description);
      Assert.AreEqual(true, attr.Required);
      Assert.AreEqual(true, attr.Visible);
      Assert.AreEqual(null, attr.Min);
      Assert.AreEqual(null, attr.Max);
      Assert.AreEqual(1, attr.MinLength);
      Assert.AreEqual(25, attr.MaxLength);
      Assert.AreEqual(null, attr.Default);
      Assert.AreEqual(0, attr.ParseValueList().Count);
      Assert.AreEqual(StoreFlag.OnlyLoad, attr.StoreFlag);
      Assert.AreEqual(@"''{Name=Mnemonic Type=string Description=Mnemonic Required=True MinSize=1 Size=25 Placeholder=Mnemonic Stored=False}", attr.MetadataContent);
      Assert.AreEqual("Dns", rec["Mnemonic"]);

      fdef = rec.Schema["Vertical_ID"];
      Assert.IsNotNull(fdef);
      Assert.AreEqual("Vertical_ID", fdef.Name);
      Assert.AreEqual(typeof(string), fdef.Type);
      Assert.AreEqual(true, fdef.AnyTargetKey);
      Assert.IsNotNull(fdef.Attrs);
      Assert.AreEqual(1, fdef.Attrs.Count());
      attr = fdef.Attrs.First();
      Assert.AreEqual("Vertical", attr.Description);
      Assert.AreEqual(false, attr.Required);
      Assert.AreEqual(false, attr.Visible);
      Assert.AreEqual(null, attr.Min);
      Assert.AreEqual(null, attr.Max);
      Assert.AreEqual(0, attr.MinLength);
      Assert.AreEqual(15, attr.MaxLength);
      Assert.AreEqual(CharCase.Upper, attr.CharCase);
      Assert.AreEqual("HAB", attr.Default);
      Assert.AreEqual(FieldAttribute.ParseValueListString("HAB:HAB.rs Health and Beauty,READRS:READ.rs Book Business,SLRS:SL.RS General Business", true), attr.ParseValueList(true));
      Assert.AreEqual("''{Name=Vertical_ID Type=string Description=Vertical Required=False Visible=False Size=15 DefaultValue=HAB Key=True Case=Upper LookupDict=\"{\\\"HAB\\\":\\\"HAB.rs Health and Beauty\\\",\\\"READRS\\\":\\\"READ.rs Book Business\\\",\\\"SLRS\\\":\\\"SL.RS General Business\\\"}\"}", attr.MetadataContent);
      Assert.AreEqual("HAB", rec["Vertical_ID"]);
      
      fdef = rec.Schema["Table_ID"];
      Assert.IsNotNull(fdef);
      Assert.AreEqual("Table_ID", fdef.Name);
      Assert.AreEqual(typeof(long), fdef.Type);
      Assert.AreEqual(true, fdef.AnyTargetKey);
      Assert.IsNotNull(fdef.Attrs);
      Assert.AreEqual(1, fdef.Attrs.Count());
      attr = fdef.Attrs.First();
      Assert.AreEqual("Table", attr.Description);
      Assert.AreEqual(true, attr.Required);
      Assert.AreEqual(false, attr.Visible);
      Assert.AreEqual(1, attr.Min);
      Assert.AreEqual(123, attr.Max);
      Assert.AreEqual(15, attr.Default);
      Assert.AreEqual(DataKind.Number, attr.Kind);
      Assert.AreEqual(StoreFlag.LoadAndStore, attr.StoreFlag);
      Assert.AreEqual(0, attr.ParseValueList(true).Count);
      Assert.AreEqual("''{Name=Table_ID Type=int Key=True Description=Table Required=True Visible=False MinValue=1 MaxValue=123 DefaultValue=15 Kind=Number Stored=True}", attr.MetadataContent);
      Assert.AreEqual(2, rec["Table_ID"]);
    }
    
    [Test]
    public void Record_InitJSONWithErrors()
    {
      var json = 
@"{
  'error': 'Error message',
  'errorText': 'Error details',
  'fields': 
   [
     {
       'def': {
         'Name': 'ID',
         'Type': 'string'
       },
       'val': 'ABBA',
       'error': 'ID Error message',
       'errorText': 'ID Error details'
     },
     {
       'def': {
         'Name': 'Name',
         'Type': 'string'
       },
       'val': 'SUP',
       'error': 'Name Error message'
     },
     {
       'def': {
         'Name': 'Value',
         'Type': 'string'
       },
       'val': 'ASP',
       'errorText': 'Value Error details'
     },
     {
       'def': {
         'Name': 'Mess',
         'Type': 'string'
       },
       'val': 'NoError'
     }
   ]
}";

      var rec = new Record(json);
      var errors = rec.ServerErrors.ToList();
      
      Assert.AreEqual(4, errors.Count);
      Assert.AreEqual(null, errors[0].FieldName);
      Assert.AreEqual("Error message", errors[0].Error);
      Assert.AreEqual("Error details", errors[0].Text);
      
      Assert.AreEqual(4, rec.Schema.FieldCount);

      var fdef = rec.Schema["ID"];
      Assert.IsNotNull(fdef);
      Assert.AreEqual("ID", fdef.Name);
      Assert.AreEqual(typeof(string), fdef.Type);
      Assert.AreEqual("ID", errors[1].FieldName);
      Assert.AreEqual("ID Error message", errors[1].Error);
      Assert.AreEqual("ID Error details", errors[1].Text);
      Assert.AreEqual("ABBA", rec["ID"]);
      
      fdef = rec.Schema["Name"];
      Assert.IsNotNull(fdef);
      Assert.AreEqual("Name", fdef.Name);
      Assert.AreEqual(typeof(string), fdef.Type);
      Assert.AreEqual("Name", errors[2].FieldName);
      Assert.AreEqual("Name Error message", errors[2].Error);
      Assert.AreEqual(null, errors[2].Text);
      Assert.AreEqual("SUP", rec["Name"]);
      
      fdef = rec.Schema["Value"];
      Assert.IsNotNull(fdef);
      Assert.AreEqual("Value", fdef.Name);
      Assert.AreEqual(typeof(string), fdef.Type);
      Assert.AreEqual("Value", errors[3].FieldName);
      Assert.AreEqual(null, errors[3].Error);
      Assert.AreEqual("Value Error details", errors[3].Text);
      Assert.AreEqual("ASP", rec["Value"]);
      
      fdef = rec.Schema["Mess"];
      Assert.IsNotNull(fdef);
      Assert.AreEqual("Mess", fdef.Name);
      Assert.AreEqual(typeof(string), fdef.Type);
      Assert.AreEqual("NoError", rec["Mess"]);
    }

    [Test]
    [ExpectedException(typeof(WaveException), ExpectedMessage = "Record.ctor(init is bad)", MatchType = MessageMatch.Contains)]
    public void Record_BadInit()
    {
      var json = 
@"{
  'error': 'Error message',
  'errorText': 'Error details'
  'fields': 
   [
     {
       'def': {
         'Name': 'ID',
         'Type': 'string'
       },
       'val': 'ABBA',
       'error': 'ID Error message',
       'errorText': 'ID Error details'
     }
   ]
}";
      var rec = new Record(json);
    }

    [Test]
    [ExpectedException(typeof(WaveException), ExpectedMessage = "Record.ctor(init==null|empty)", MatchType = MessageMatch.Contains)]
    public void Record_EmptyInit()
    {
      var json = "";
      var rec = new Record(json);
    }

    [Test]
    public void Record_MapJSToCLRTypes()
    {
      var json = 
@"{
  'fields': 
   [
     {
       'def': {
         'Name': 'STR',
         'Type': 'string'
       },
       'val': 'ABBA'
     },
     {
       'def': {
         'Name': 'INT',
         'Type': 'int'
       },
       'val': -23
     },
     {
       'def': {
         'Name': 'NUM',
         'Type': 'real'
       },
       'val': -123.456
     },
     {
       'def': {
         'Name': 'BOOL',
         'Type': 'bool'
       },
       'val': true
     },
     {
       'def': {
         'Name': 'DATE',
         'Type': 'datetime'
       },
       'val': '2016-03-23 12:23:59'
     },
     {
       'def': {
         'Name': 'OBJ',
         'Type': 'object'
       },
       'val': { 'n': 'name', 'age': 23 }
     },
     {
       'def': {
         'Name': 'DFT'
       },
       'val': 'Default type is string'
     }
   ]
}";

      var rec = new Record(json);
      var errors = rec.ServerErrors.ToList();
      
      Assert.AreEqual(0, errors.Count);
      
      Assert.AreEqual(7, rec.Schema.FieldCount);

      var fdef = rec.Schema["STR"];
      Assert.IsNotNull(fdef);
      Assert.AreEqual("STR", fdef.Name);
      Assert.AreEqual(typeof(string), fdef.Type);
      Assert.AreEqual("ABBA", rec["STR"]);
      
      fdef = rec.Schema["INT"];
      Assert.IsNotNull(fdef);
      Assert.AreEqual("INT", fdef.Name);
      Assert.AreEqual(typeof(long), fdef.Type);
      Assert.AreEqual(-23, rec["INT"]);
      
      fdef = rec.Schema["NUM"];
      Assert.IsNotNull(fdef);
      Assert.AreEqual("NUM", fdef.Name);
      Assert.AreEqual(typeof(double), fdef.Type);
      Assert.AreEqual(-123.456, rec["NUM"]);
      
      fdef = rec.Schema["BOOL"];
      Assert.IsNotNull(fdef);
      Assert.AreEqual("BOOL", fdef.Name);
      Assert.AreEqual(typeof(bool), fdef.Type);
      Assert.AreEqual(true, rec["BOOL"]);
      
      fdef = rec.Schema["DATE"];
      Assert.IsNotNull(fdef);
      Assert.AreEqual("DATE", fdef.Name);
      Assert.AreEqual(typeof(DateTime), fdef.Type);
      Assert.AreEqual("2016-03-23 12:23:59".AsDateTime(), rec["DATE"]);
      
      fdef = rec.Schema["OBJ"];
      Assert.IsNotNull(fdef);
      Assert.AreEqual("OBJ", fdef.Name);
      Assert.AreEqual(typeof(object), fdef.Type);
      var value = JSONReader.DeserializeDataObject("{ 'n': 'name', 'age': 23 }") as JSONDataMap;
      Assert.AreEqual(value, rec["OBJ"]);
      
      fdef = rec.Schema["DFT"];
      Assert.IsNotNull(fdef);
      Assert.AreEqual("DFT", fdef.Name);
      Assert.AreEqual(typeof(string), fdef.Type);
      Assert.AreEqual("Default type is string", rec["DFT"]);
    }
    
    [Test]
    public void Record_MapJSToCLRKinds()
    {
      var json = 
@"{
  'fields': 
   [
     {
       'def': {
         'Name': 'LOCAL',
         'Type': 'datetime',
         'Kind': 'datetime-local'
       },
       'val': '2016-03-23 12:23:59'
     },
     {
       'def': {
         'Name': 'TEL',
         'Type': 'string',
         'Kind': 'tel'
       },
       'val': '(111) 222-33-44'
     },
     {
       'def': {
         'Name': 'DFT'
       },
       'val': 'Default kind is Text'
     }
   ]
}";

      var rec = new Record(json);
      var errors = rec.ServerErrors.ToList();
      
      Assert.AreEqual(0, errors.Count);
      
      Assert.AreEqual(3, rec.Schema.FieldCount);

      var fdef = rec.Schema["LOCAL"];
      Assert.IsNotNull(fdef);
      Assert.AreEqual("LOCAL", fdef.Name);
      Assert.AreEqual(typeof(DateTime), fdef.Type);
      var attr = fdef.Attrs.First();
      Assert.AreEqual(DataKind.DateTimeLocal, attr.Kind);
      Assert.AreEqual("2016-03-23 12:23:59".AsDateTime(), rec["LOCAL"]);
      
      fdef = rec.Schema["TEL"];
      Assert.IsNotNull(fdef);
      Assert.AreEqual("TEL", fdef.Name);
      Assert.AreEqual(typeof(string), fdef.Type);
      attr = fdef.Attrs.First();
      Assert.AreEqual(DataKind.Telephone, attr.Kind);
      Assert.AreEqual("(111) 222-33-44", rec["TEL"]);
      
      fdef = rec.Schema["DFT"];
      Assert.IsNotNull(fdef);
      Assert.AreEqual("DFT", fdef.Name);
      Assert.AreEqual(typeof(string), fdef.Type);
      attr = fdef.Attrs.First();
      Assert.AreEqual(DataKind.Text, attr.Kind);
      Assert.AreEqual("Default kind is Text", rec["DFT"]);
    }
  }
}
