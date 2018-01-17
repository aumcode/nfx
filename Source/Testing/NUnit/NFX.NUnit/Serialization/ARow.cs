/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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


      [Test]
      public void SerDeser_FamilyRow_1_NoReferences()
      {
        var row1 = new FamilyRow
        {
         ID = new GDID(1,345), Name = "Lalala", RegisteredToVote = true,

         //Father = new SimplePersonRow
         //{
         //  Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten", Date = new DateTime(1980, 08, 12, 13, 45, 11)
         //}
        };

        var writer = SlimFormat.Instance.GetWritingStreamer();
        var reader = SlimFormat.Instance.GetReadingStreamer();
        using(var ms = new MemoryStream())
        {
            writer.BindStream(ms);
            ArowSerializer.Serialize(row1, writer);
            writer.UnbindStream();

            ms.Position = 0;

            var row2 = new FamilyRow();
            reader.BindStream(ms);
            ArowSerializer.Deserialize(row2, reader);
            reader.UnbindStream();

            Aver.AreEqual(row1.ID,    row2.ID);
            Aver.AreEqual(row1.Name, row2.Name);
        }
      }

      [Test]
      public void SerDeser_FamilyRow_2_OneFieldRef()
      {
        var row1 = new FamilyRow
        {
         ID = new GDID(1,345), Name = "Lalala", RegisteredToVote = true,

         Father = new SimplePersonRow
         {
           Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten", Date = new DateTime(1980, 08, 12, 13, 45, 11)
         }
        };

        var writer = SlimFormat.Instance.GetWritingStreamer();
        var reader = SlimFormat.Instance.GetReadingStreamer();
        using(var ms = new MemoryStream())
        {
            writer.BindStream(ms);
            ArowSerializer.Serialize(row1, writer);
            writer.UnbindStream();

            ms.Position = 0;

            var row2 = new FamilyRow();
            reader.BindStream(ms);
            ArowSerializer.Deserialize(row2, reader);
            reader.UnbindStream();

            Aver.AreNotSameRef(row1, row2);
            Aver.AreEqual(row1.ID,    row2.ID);
            Aver.AreEqual(row1.Name, row2.Name);
            Aver.AreEqual(row1.RegisteredToVote, row2.RegisteredToVote);
            Aver.IsNotNull( row2.Father );
            Aver.AreEqual(row1.Father.ID, row2.Father.ID);
            Aver.AreEqual(row1.Father.Age, row2.Father.Age);
            Aver.AreEqual(row1.Father.Str1, row2.Father.Str1);

            Aver.AreEqual(row1.Father.Date, row2.Father.Date);
            Aver.IsNull(row2.Father.Str2);

            Aver.IsNull(row2.Mother);
        }
      }


      [Test]
      public void SerDeser_FamilyRow_3_TwoFieldRef()
      {
        var row1 = new FamilyRow
        {
         ID = new GDID(1,345), Name = "Lalala", RegisteredToVote = true,

         Father = new SimplePersonRow
         {
           Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten", Date = new DateTime(1980, 08, 12, 13, 45, 11)
         },
         Mother = new SimplePersonRow
         {
           Age = 245, Bool1 =false, ID = new GDID(2,12), Name = "Katya Zhaba", Salary=180000, Str1="Snake", Str2="Borra", Date = new DateTime(1911, 01, 01, 14, 11, 07)
         }
        };

        var writer = SlimFormat.Instance.GetWritingStreamer();
        var reader = SlimFormat.Instance.GetReadingStreamer();
        using(var ms = new MemoryStream())
        {
            writer.BindStream(ms);
            ArowSerializer.Serialize(row1, writer);
            writer.UnbindStream();

            ms.Position = 0;

            var row2 = new FamilyRow();
            reader.BindStream(ms);
            ArowSerializer.Deserialize(row2, reader);
            reader.UnbindStream();

            Aver.AreNotSameRef(row1, row2);
            Aver.AreEqual(row1.ID,    row2.ID);
            Aver.AreEqual(row1.Name, row2.Name);
            Aver.AreEqual(row1.RegisteredToVote, row2.RegisteredToVote);
            Aver.IsNotNull( row2.Father );
            Aver.AreEqual(row1.Father.ID, row2.Father.ID);
            Aver.AreEqual(row1.Father.Age, row2.Father.Age);
            Aver.AreEqual(row1.Father.Str1, row2.Father.Str1);

            Aver.AreEqual(row1.Father.Date, row2.Father.Date);
            Aver.IsNull(row2.Father.Str2);

            Aver.IsNotNull(row2.Mother);
            Aver.AreEqual(row1.Mother.ID, row2.Mother.ID);
            Aver.AreEqual(row1.Mother.Age, row2.Mother.Age);
            Aver.AreEqual(row1.Mother.Str1, row2.Mother.Str1);
            Aver.IsNotNull(row2.Mother.Str2);
            Aver.AreEqual(row1.Mother.Str2, row2.Mother.Str2);
            Aver.AreEqual(row1.Mother.Date, row2.Mother.Date);
        }
      }

      [Test]
      public void SerDeser_FamilyRow_4_EmptyArray()
      {
        var row1 = new FamilyRow
        {
         ID = new GDID(1,345), Name = "Lalala", RegisteredToVote = true,

         Father = new SimplePersonRow
         {
           Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten", Date = new DateTime(1980, 08, 12, 13, 45, 11)
         },
         Mother = new SimplePersonRow
         {
           Age = 245, Bool1 =false, ID = new GDID(2,12), Name = "Katya Zhaba", Salary=180000, Str1="Snake", Str2="Borra", Date = new DateTime(1911, 01, 01, 14, 11, 07)
         },
         Brothers = new SimplePersonRow[0],
        };

        var writer = SlimFormat.Instance.GetWritingStreamer();
        var reader = SlimFormat.Instance.GetReadingStreamer();
        using(var ms = new MemoryStream())
        {
            writer.BindStream(ms);
            ArowSerializer.Serialize(row1, writer);
            writer.UnbindStream();

            ms.Position = 0;

            var row2 = new FamilyRow();
            reader.BindStream(ms);
            ArowSerializer.Deserialize(row2, reader);
            reader.UnbindStream();

            Aver.AreNotSameRef(row1, row2);
            Aver.AreEqual(row1.ID,    row2.ID);
            Aver.AreEqual(row1.Name, row2.Name);
            Aver.AreEqual(row1.RegisteredToVote, row2.RegisteredToVote);
            Aver.IsNotNull( row2.Father );
            Aver.AreEqual(row1.Father.ID, row2.Father.ID);
            Aver.AreEqual(row1.Father.Age, row2.Father.Age);
            Aver.AreEqual(row1.Father.Str1, row2.Father.Str1);

            Aver.AreEqual(row1.Father.Date, row2.Father.Date);
            Aver.IsNull(row2.Father.Str2);

            Aver.IsNotNull(row2.Mother);
            Aver.AreEqual(row1.Mother.ID, row2.Mother.ID);
            Aver.AreEqual(row1.Mother.Age, row2.Mother.Age);
            Aver.AreEqual(row1.Mother.Str1, row2.Mother.Str1);
            Aver.IsNotNull(row2.Mother.Str2);
            Aver.AreEqual(row1.Mother.Str2, row2.Mother.Str2);
            Aver.AreEqual(row1.Mother.Date, row2.Mother.Date);

            Aver.IsNotNull(row2.Brothers);
            Aver.AreEqual(0, row2.Brothers.Length);
            Aver.IsNull(row2.Sisters);
            Aver.IsNull(row2.Advisers);
        }
      }

      [Test]
      public void SerDeser_FamilyRow_5_OneArrayFilled()
      {
        var row1 = new FamilyRow
        {
         ID = new GDID(1,345), Name = "Lalala", RegisteredToVote = true,

         Father = new SimplePersonRow
         {
           Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten", Date = new DateTime(1980, 08, 12, 13, 45, 11)
         },
         Mother = new SimplePersonRow
         {
           Age = 245, Bool1 =false, ID = new GDID(2,12), Name = "Katya Zhaba", Salary=180000, Str1="Snake", Str2="Borra", Date = new DateTime(1911, 01, 01, 14, 11, 07)
         },
         Brothers = new []{ new SimplePersonRow{Age=111}, new SimplePersonRow{Age=222}, new SimplePersonRow{Age=333}}
        };

        var writer = SlimFormat.Instance.GetWritingStreamer();
        var reader = SlimFormat.Instance.GetReadingStreamer();
        using(var ms = new MemoryStream())
        {
            writer.BindStream(ms);
            ArowSerializer.Serialize(row1, writer);
            writer.UnbindStream();

            ms.Position = 0;

            var row2 = new FamilyRow();
            reader.BindStream(ms);
            ArowSerializer.Deserialize(row2, reader);
            reader.UnbindStream();

            Aver.AreNotSameRef(row1, row2);
            Aver.AreEqual(row1.ID,    row2.ID);
            Aver.AreEqual(row1.Name, row2.Name);
            Aver.AreEqual(row1.RegisteredToVote, row2.RegisteredToVote);
            Aver.IsNotNull( row2.Father );
            Aver.AreEqual(row1.Father.ID, row2.Father.ID);
            Aver.AreEqual(row1.Father.Age, row2.Father.Age);
            Aver.AreEqual(row1.Father.Str1, row2.Father.Str1);

            Aver.AreEqual(row1.Father.Date, row2.Father.Date);
            Aver.IsNull(row2.Father.Str2);

            Aver.IsNotNull(row2.Mother);
            Aver.AreEqual(row1.Mother.ID, row2.Mother.ID);
            Aver.AreEqual(row1.Mother.Age, row2.Mother.Age);
            Aver.AreEqual(row1.Mother.Str1, row2.Mother.Str1);
            Aver.IsNotNull(row2.Mother.Str2);
            Aver.AreEqual(row1.Mother.Str2, row2.Mother.Str2);
            Aver.AreEqual(row1.Mother.Date, row2.Mother.Date);

            Aver.IsNotNull(row2.Brothers);
            Aver.AreEqual(3, row2.Brothers.Length);
            Aver.AreEqual(111, row2.Brothers[0].Age);
            Aver.AreEqual(222, row2.Brothers[1].Age);
            Aver.AreEqual(333, row2.Brothers[2].Age);
            Aver.IsNull( row2.Sisters);
            Aver.IsNull(row2.Advisers);
        }
      }

      [Test]
      public void SerDeser_FamilyRow_6_TwoArrayFilled()
      {
        var row1 = new FamilyRow
        {
         ID = new GDID(1,345), Name = "Lalala", RegisteredToVote = true,

         Father = new SimplePersonRow
         {
           Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten", Date = new DateTime(1980, 08, 12, 13, 45, 11)
         },
         Mother = new SimplePersonRow
         {
           Age = 245, Bool1 =false, ID = new GDID(2,12), Name = "Katya Zhaba", Salary=180000, Str1="Snake", Str2="Borra", Date = new DateTime(1911, 01, 01, 14, 11, 07)
         },
         Brothers = new []{ new SimplePersonRow{Age=111}, new SimplePersonRow{Age=222}, new SimplePersonRow{Age=333}},
         Sisters = new []{ new SimplePersonRow{Age=12}, new SimplePersonRow{Age=13}}
        };

        var writer = SlimFormat.Instance.GetWritingStreamer();
        var reader = SlimFormat.Instance.GetReadingStreamer();
        using(var ms = new MemoryStream())
        {
            writer.BindStream(ms);
            ArowSerializer.Serialize(row1, writer);
            writer.UnbindStream();

            ms.Position = 0;

            var row2 = new FamilyRow();
            reader.BindStream(ms);
            ArowSerializer.Deserialize(row2, reader);
            reader.UnbindStream();

            Aver.AreNotSameRef(row1, row2);
            Aver.AreEqual(row1.ID,    row2.ID);
            Aver.AreEqual(row1.Name, row2.Name);
            Aver.AreEqual(row1.RegisteredToVote, row2.RegisteredToVote);
            Aver.IsNotNull( row2.Father );
            Aver.AreEqual(row1.Father.ID, row2.Father.ID);
            Aver.AreEqual(row1.Father.Age, row2.Father.Age);
            Aver.AreEqual(row1.Father.Str1, row2.Father.Str1);

            Aver.AreEqual(row1.Father.Date, row2.Father.Date);
            Aver.IsNull(row2.Father.Str2);

            Aver.IsNotNull(row2.Mother);
            Aver.AreEqual(row1.Mother.ID, row2.Mother.ID);
            Aver.AreEqual(row1.Mother.Age, row2.Mother.Age);
            Aver.AreEqual(row1.Mother.Str1, row2.Mother.Str1);
            Aver.IsNotNull(row2.Mother.Str2);
            Aver.AreEqual(row1.Mother.Str2, row2.Mother.Str2);
            Aver.AreEqual(row1.Mother.Date, row2.Mother.Date);

            Aver.IsNotNull(row2.Brothers);
            Aver.AreEqual(3, row2.Brothers.Length);
            Aver.AreEqual(111, row2.Brothers[0].Age);
            Aver.AreEqual(222, row2.Brothers[1].Age);
            Aver.AreEqual(333, row2.Brothers[2].Age);
            Aver.IsNotNull(row2.Sisters);
            Aver.AreEqual(2, row2.Sisters.Length);
            Aver.AreEqual(12, row2.Sisters[0].Age);
            Aver.AreEqual(13, row2.Sisters[1].Age);
            Aver.IsNull(row2.Advisers);
        }
      }


      [Test]
      public void SerDeser_FamilyRow_7_ArraysAndList()
      {
        var row1 = new FamilyRow
        {
         ID = new GDID(1,345), Name = "Lalala", RegisteredToVote = true,

         Father = new SimplePersonRow
         {
           Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten", Date = new DateTime(1980, 08, 12, 13, 45, 11)
         },
         Mother = new SimplePersonRow
         {
           Age = 245, Bool1 =false, ID = new GDID(2,12), Name = "Katya Zhaba", Salary=180000, Str1="Snake", Str2="Borra", Date = new DateTime(1911, 01, 01, 14, 11, 07)
         },
         Brothers = new []{ new SimplePersonRow{Age=111}, new SimplePersonRow{Age=222}, new SimplePersonRow{Age=333}},
         Sisters = new []{ new SimplePersonRow{Age=12}, new SimplePersonRow{Age=13}},
         Advisers = new List<SimplePersonRow>{ new SimplePersonRow{Age=101, Name="Kaznatchei"}, new SimplePersonRow{Age=102}}
        };

        var writer = SlimFormat.Instance.GetWritingStreamer();
        var reader = SlimFormat.Instance.GetReadingStreamer();
        using(var ms = new MemoryStream())
        {
            writer.BindStream(ms);
            ArowSerializer.Serialize(row1, writer);
            writer.UnbindStream();

            ms.Position = 0;

            var row2 = new FamilyRow();
            reader.BindStream(ms);
            ArowSerializer.Deserialize(row2, reader);
            reader.UnbindStream();

            Aver.AreNotSameRef(row1, row2);
            Aver.AreEqual(row1.ID,    row2.ID);
            Aver.AreEqual(row1.Name, row2.Name);
            Aver.AreEqual(row1.RegisteredToVote, row2.RegisteredToVote);
            Aver.IsNotNull( row2.Father );
            Aver.AreEqual(row1.Father.ID, row2.Father.ID);
            Aver.AreEqual(row1.Father.Age, row2.Father.Age);
            Aver.AreEqual(row1.Father.Str1, row2.Father.Str1);

            Aver.AreEqual(row1.Father.Date, row2.Father.Date);
            Aver.IsNull(row2.Father.Str2);

            Aver.IsNotNull(row2.Mother);
            Aver.AreEqual(row1.Mother.ID, row2.Mother.ID);
            Aver.AreEqual(row1.Mother.Age, row2.Mother.Age);
            Aver.AreEqual(row1.Mother.Str1, row2.Mother.Str1);
            Aver.IsNotNull(row2.Mother.Str2);
            Aver.AreEqual(row1.Mother.Str2, row2.Mother.Str2);
            Aver.AreEqual(row1.Mother.Date, row2.Mother.Date);

            Aver.IsNotNull(row2.Brothers);
            Aver.AreEqual(3, row2.Brothers.Length);
            Aver.AreEqual(111, row2.Brothers[0].Age);
            Aver.AreEqual(222, row2.Brothers[1].Age);
            Aver.AreEqual(333, row2.Brothers[2].Age);
            Aver.IsNotNull(row2.Sisters);
            Aver.AreEqual(2, row2.Sisters.Length);
            Aver.AreEqual(12, row2.Sisters[0].Age);
            Aver.AreEqual(13, row2.Sisters[1].Age);
            Aver.IsNotNull(row2.Advisers);

            Aver.AreEqual(2, row2.Advisers.Count);
            Aver.AreEqual(101, row2.Advisers[0].Age);
            Aver.AreEqual("Kaznatchei", row2.Advisers[0].Name);
            Aver.AreEqual(102, row2.Advisers[1].Age);
        }
      }


    }
}
