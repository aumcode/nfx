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
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.IO;
using NFX.Serialization.Arow;
using NFX.ApplicationModel;
using NFX.Financial;
using NFX.DataAccess.Distributed;

namespace NFX.NUnit.Serialization
{
    [TestFixture]
    public class ARowBenchmarkingParallel
    {
      [TestFixtureSetUp]
      public void Setup()
      {
        ArowSerializer.RegisterTypeSerializationCores( Assembly.GetExecutingAssembly() );
      }

      private SimplePersonRow getSimplePerson()
      {
        return new SimplePersonRow
        {
           Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten"
        };
      }

      private FamilyRow getFamily()
      {
        return new FamilyRow
        {
           ID = new GDID(12,3214232333),
           Name = "The Familians",
           Advisers = new List<SimplePersonRow>{ new SimplePersonRow{ Name="Jabitzkiy"}, new SimplePersonRow{ Name="Fixman", Age=4827384}},
           Father = getSimplePerson(),
           Mother = getSimplePerson(),
           Brothers = new []{ getSimplePerson() },
           Sisters = new []{ getSimplePerson(), getSimplePerson(), getSimplePerson() },
        };
      }


      [TestCase(250000, 12)]
      [TestCase(250000, 10)]
      [TestCase(250000, 8)]
      [TestCase(250000, 4)]
      public void Serialize_SimplePerson_Arow(int CNT, int tcnt)
      {
        var row = getSimplePerson();

        var tasks = new List<Task>();

        var sw = Stopwatch.StartNew();
        for(var t=0; t<tcnt; t++)
          tasks.Add( Task.Factory.StartNew( ()=>
          {
                var streamer = SlimFormat.Instance.GetWritingStreamer();

                using(var ms = new MemoryStream())
                {

                  for(var i=0; i<CNT; i++)
                  {
                    ms.Position = 0;
                    streamer.BindStream(ms);
                    ArowSerializer.Serialize(row, streamer);
                    streamer.UnbindStream();
                  }
                }
          }));

        Task.WaitAll(tasks.ToArray());

        var el = sw.ElapsedMilliseconds;
        var total = CNT * tcnt;
        Console.WriteLine("Arow did {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args( total, el, total / (el/1000d)));
      }

      [TestCase(250000, 12)]
      [TestCase(250000, 10)]
      [TestCase(250000, 8)]
      [TestCase(250000, 4)]
      public void Deserialize_SimplePerson_Arow(int CNT, int tcnt)
      {

        var row = getSimplePerson();


        var tasks = new List<Task>();
        var sw = Stopwatch.StartNew();
         tasks.Add( Task.Factory.StartNew( ()=>
          {

              var writer = SlimFormat.Instance.GetWritingStreamer();
              var reader = SlimFormat.Instance.GetReadingStreamer();

              using(var ms = new MemoryStream())
              {
                writer.BindStream(ms);
                ArowSerializer.Serialize(row, writer);
                writer.UnbindStream();

                reader.BindStream(ms);

                for(var i=0; i<CNT; i++)
                {
                  ms.Position = 0;
                  var row2 = new SimplePersonRow();
                  ArowSerializer.Deserialize(row2, reader);
                  Aver.AreEqual(row.ID, row2.ID);
                }

              }
          }));

          Task.WaitAll(tasks.ToArray());
          var el = sw.ElapsedMilliseconds;
          var total = CNT * tcnt;
          Console.WriteLine("Arow did {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args( total, el, total / (el/1000d) ));
      }

      [TestCase(250000, 12)]
      [TestCase(250000, 10)]
      [TestCase(250000, 8)]
      [TestCase(250000, 4)]
      public void Serialize_SimplePerson_Slim(int CNT, int tcnt)
      {
        var row = getSimplePerson();

        var tasks = new List<Task>();

        var sw = Stopwatch.StartNew();
        for(var t=0; t<tcnt; t++)
          tasks.Add( Task.Factory.StartNew( ()=>
          {
            var slim = new NFX.Serialization.Slim.SlimSerializer( NFX.Serialization.Slim.TypeRegistry.BoxedCommonNullableTypes,
                                                                  NFX.Serialization.Slim.TypeRegistry.BoxedCommonTypes,
                                                                  new []{ typeof(SimplePersonRow) });

            slim.TypeMode = NFX.Serialization.Slim.TypeRegistryMode.Batch;//give slim all possible preferences

            using(var ms = new MemoryStream())
            {
              slim.Serialize(ms, row);//warmup

              for(var i=0; i<CNT; i++)
              {
                ms.Position = 0;
                slim.Serialize(ms, row);
              }

            }
        }));

        Task.WaitAll(tasks.ToArray());

        var el = sw.ElapsedMilliseconds;
        var total = CNT * tcnt;
        Console.WriteLine("Slim did {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args( total, el, total / (el/1000d)));
      }


      [TestCase(250000, 12)]
      [TestCase(250000, 10)]
      [TestCase(250000, 8)]
      [TestCase(250000, 4)]
      public void Deserialize_SimplePerson_Slim(int CNT, int tcnt)
      {
        var row = getSimplePerson();

        var tasks = new List<Task>();
        var sw = Stopwatch.StartNew();
        tasks.Add( Task.Factory.StartNew( ()=>
        {
            var slim = new NFX.Serialization.Slim.SlimSerializer( NFX.Serialization.Slim.TypeRegistry.BoxedCommonNullableTypes,
                                                                  NFX.Serialization.Slim.TypeRegistry.BoxedCommonTypes,
                                                                  new []{ typeof(SimplePersonRow) });

            slim.TypeMode = NFX.Serialization.Slim.TypeRegistryMode.Batch;//give slim all possible preferences

            using(var ms = new MemoryStream())
            {
              slim.Serialize(ms, row);//warmup
              for(var i=0; i<CNT; i++)
              {
                ms.Position = 0;
                var row2 = slim.Deserialize(ms) as SimplePersonRow;
                Aver.AreEqual(row.ID, row2.ID);
              }
            }
        }));

        Task.WaitAll(tasks.ToArray());

        var el = sw.ElapsedMilliseconds;
        var total = CNT * tcnt;
        Console.WriteLine("Slim did {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args( total, el, total / (el/1000d)));
      }

      [TestCase(250000, 12)]
      [TestCase(250000, 10)]
      [TestCase(250000, 8)]
      [TestCase(250000, 4)]
      public void Serialize_Family_Arow(int CNT, int tcnt)
      {
        var row = getFamily();

        var tasks = new List<Task>();

        var sw = Stopwatch.StartNew();
        for(var t=0; t<tcnt; t++)
          tasks.Add( Task.Factory.StartNew( ()=>
          {

            var streamer = SlimFormat.Instance.GetWritingStreamer();

            using(var ms = new MemoryStream())
            {
              for(var i=0; i<CNT; i++)
              {
                ms.Position = 0;
                streamer.BindStream(ms);
                ArowSerializer.Serialize(row, streamer);
                streamer.UnbindStream();
              }

            }
        }));

        Task.WaitAll(tasks.ToArray());

        var el = sw.ElapsedMilliseconds;
        var total = CNT * tcnt;
        Console.WriteLine("Arow did {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args( total, el, total / (el/1000d)));
      }

      [TestCase(250000, 12)]
      [TestCase(250000, 10)]
      [TestCase(250000, 8)]
      [TestCase(250000, 4)]
      public void Deserialize_Family_Arow(int CNT, int tcnt)
      {

        var row = getFamily();

        var tasks = new List<Task>();
        var sw = Stopwatch.StartNew();
        tasks.Add( Task.Factory.StartNew( ()=>
        {
            var writer = SlimFormat.Instance.GetWritingStreamer();
            var reader = SlimFormat.Instance.GetReadingStreamer();

            using(var ms = new MemoryStream())
            {
              writer.BindStream(ms);
              ArowSerializer.Serialize(row, writer);
              writer.UnbindStream();
              reader.BindStream(ms);

              for(var i=0; i<CNT; i++)
              {
                ms.Position = 0;
                var row2 = new FamilyRow();
                ArowSerializer.Deserialize(row2, reader);
                Aver.AreEqual(row.ID, row2.ID);
              }

            }
        }));

        Task.WaitAll(tasks.ToArray());
        var el = sw.ElapsedMilliseconds;
        var total = CNT * tcnt;
        Console.WriteLine("Arow did {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args( total, el, total / (el/1000d)));
      }

      [TestCase(250000, 12)]
      [TestCase(250000, 10)]
      [TestCase(250000, 8)]
      [TestCase(250000, 4)]
      public void Serialize_Family_Slim(int CNT, int tcnt)
      {
        var row = getFamily();

        var tasks = new List<Task>();

        var sw = Stopwatch.StartNew();
        for(var t=0; t<tcnt; t++)
          tasks.Add( Task.Factory.StartNew( ()=>
          {
            var slim = new NFX.Serialization.Slim.SlimSerializer( NFX.Serialization.Slim.TypeRegistry.BoxedCommonNullableTypes,
                                                                  NFX.Serialization.Slim.TypeRegistry.BoxedCommonTypes,
                                                                  new []{ typeof(SimplePersonRow),typeof(SimplePersonRow[]), typeof(List<SimplePersonRow>), typeof(FamilyRow) });

            slim.TypeMode = NFX.Serialization.Slim.TypeRegistryMode.Batch;//give slim all possible preferences

            using(var ms = new MemoryStream())
            {
              slim.Serialize(ms, row);//warmup
              for(var i=0; i<CNT; i++)
              {
                ms.Position = 0;
                slim.Serialize(ms, row);
              }

            }
        }));

        Task.WaitAll(tasks.ToArray());

        var el = sw.ElapsedMilliseconds;
        var total = CNT * tcnt;

        Console.WriteLine("Slim did {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args( total, el, total / (el/1000d)));
      }


      [TestCase(250000, 12)]
      [TestCase(250000, 10)]
      [TestCase(250000, 8)]
      [TestCase(250000, 4)]
      public void Deserialize_Family_Slim(int CNT, int tcnt)
      {
        var row = getFamily();

        var tasks = new List<Task>();
        var sw = Stopwatch.StartNew();
        tasks.Add( Task.Factory.StartNew( ()=>
        {

            var slim = new NFX.Serialization.Slim.SlimSerializer( NFX.Serialization.Slim.TypeRegistry.BoxedCommonNullableTypes,
                                                                  NFX.Serialization.Slim.TypeRegistry.BoxedCommonTypes,
                                                                  new []{ typeof(SimplePersonRow),typeof(SimplePersonRow[]), typeof(List<SimplePersonRow>), typeof(FamilyRow) });

            slim.TypeMode = NFX.Serialization.Slim.TypeRegistryMode.Batch;//give slim all possible preferences

            using(var ms = new MemoryStream())
            {
              slim.Serialize(ms, row);//warmup
              for(var i=0; i<CNT; i++)
              {
                ms.Position = 0;
                var row2 = slim.Deserialize(ms) as FamilyRow;
                Aver.AreEqual(row.ID, row2.ID);
              }

            }
        }));

        Task.WaitAll(tasks.ToArray());

        var el = sw.ElapsedMilliseconds;
        var total = CNT * tcnt;
        Console.WriteLine("Slim did {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args( total, el, total / (el/1000d)));
      }



    }
}
