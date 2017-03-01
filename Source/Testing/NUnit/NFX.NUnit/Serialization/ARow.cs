/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using System.IO;
using System.Diagnostics;
using System.Reflection;

using NUnit.Framework;

using NFX.IO;
using NFX.Serialization.Arow;
using NFX.ApplicationModel;
using NFX.Financial;
using NFX.DataAccess.Distributed;

namespace NFX.NUnit.Serialization
{
    [TestFixture]
    public class ARow
    {
      [TestFixtureSetUp]
      public void Setup()
      {
        ArowSerializer.RegisterTypeSerializationCores( Assembly.GetExecutingAssembly() );
      }


      [Test]
      public void SerDeser_OneSimplePerson()
      {
        var row1 = new SimplePersonRow
        {
           Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten", Date = new DateTime(1980, 08, 12, 13, 45, 11)
        };
        var writer = SlimFormat.Instance.GetWritingStreamer();
        var reader = SlimFormat.Instance.GetReadingStreamer();
        using(var ms = new MemoryStream())
        {
            writer.BindStream(ms);
            ArowSerializer.Serialize(row1, writer);
            writer.UnbindStream();

            ms.Position = 0;

            var row2 = new SimplePersonRow();
            reader.BindStream(ms);
            ArowSerializer.Deserialize(row2, reader);
            reader.UnbindStream();

            Aver.AreEqual(row1.ID,    row2.ID);
            Aver.AreEqual(row1.Bool1, row2.Bool1);
            Aver.AreEqual(row1.Name,  row2.Name);
            Aver.AreEqual(row1.Age,   row2.Age);
            Aver.AreEqual(row1.Salary,row2.Salary);
            Aver.AreEqual(row1.Str1,  row2.Str1);
            Aver.AreEqual(row1.Date,  row2.Date);
        }
      }

      [Test]
      public void SerDeser_OneSimplePersonWithEnum()
      {
        var row1 = new SimplePersonWithEnumRow
        {
           Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten", Date = new DateTime(1980, 08, 12, 13, 45, 11),
           Married = SimplePersonWithEnumRow.MaritalStatus.Alien
        };
        var writer = SlimFormat.Instance.GetWritingStreamer();
        var reader = SlimFormat.Instance.GetReadingStreamer();
        using(var ms = new MemoryStream())
        {
            writer.BindStream(ms);
            ArowSerializer.Serialize(row1, writer);
            writer.UnbindStream();

            ms.Position = 0;

            var row2 = new SimplePersonWithEnumRow();
            reader.BindStream(ms);
            ArowSerializer.Deserialize(row2, reader);
            reader.UnbindStream();

            Aver.AreEqual(row1.ID,    row2.ID);
            Aver.AreEqual(row1.Bool1, row2.Bool1);
            Aver.AreEqual(row1.Name,  row2.Name);
            Aver.AreEqual(row1.Age,   row2.Age);
            Aver.AreEqual(row1.Salary,row2.Salary);
            Aver.AreEqual(row1.Str1,  row2.Str1);
            Aver.AreEqual(row1.Date,  row2.Date);
            Aver.IsTrue(row1.Married == row2.Married);
        }
      }


    }
}
