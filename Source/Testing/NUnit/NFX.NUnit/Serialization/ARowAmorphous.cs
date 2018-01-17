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
using NFX.Serialization.JSON;
using NFX.ApplicationModel;
using NFX.Financial;
using NFX.DataAccess.Distributed;
using NFX.ApplicationModel.Pile;

namespace NFX.NUnit.Serialization
{
    [TestFixture]
    public class ARowAmorphous
    {
      [TestFixtureSetUp]
      public void Setup()
      {
        ArowSerializer.RegisterTypeSerializationCores( Assembly.GetExecutingAssembly() );
      }



      [Test]
      public void ReadIntoSame()
      {
        var v1r1 = new Ver1Row
        {
           A = "A string",
           B = 156,
           C = null,
           D = true,
           E = new byte[]{1,5,98},
           F = new DateTime(1980, 02, 12),
           G = new List<Ver1Row>{ new Ver1Row{ C = -998, D = null }}
        };

        var writer = SlimFormat.Instance.GetWritingStreamer();
        var reader = SlimFormat.Instance.GetReadingStreamer();
        using(var ms = new MemoryStream())
        {
          writer.BindStream(ms);
          ArowSerializer.Serialize(v1r1, writer);
          writer.UnbindStream();

          ms.Position = 0;

          var v1r2 = new Ver1Row();
          reader.BindStream(ms);
          ArowSerializer.Deserialize(v1r2, reader);

          Aver.AreEqual("A string", v1r2.A);
          Aver.AreEqual(156, v1r2.B);
          Aver.IsNull( v1r2.C );
          Aver.IsTrue( v1r2.D.Value );
          Aver.AreEqual(3,  v1r2.E.Length );
          Aver.AreEqual(98,  v1r2.E[2] );
          Aver.AreEqual(new DateTime(1980, 02, 12),  v1r2.F );
          Aver.IsNotNull(v1r2.G);
          Aver.AreEqual(1,  v1r2.G.Count );
          Aver.AreEqual(-998,  v1r2.G[0].C );
          Aver.IsNull( v1r2.G[0].D );
        }
      }

      [Test]
      public void ReadIntoAnother()
      {
        var v1r1 = new Ver1Row
        {
           A = "A string",
           B = 156,
           C = null,
           D = true,
           E = new byte[]{1,5,98},
           F = new DateTime(1980, 02, 12),
           G = new List<Ver1Row>{ new Ver1Row{ C = -998, D = null }}
        };

        var writer = SlimFormat.Instance.GetWritingStreamer();
        var reader = SlimFormat.Instance.GetReadingStreamer();
        using(var ms = new MemoryStream())
        {
          writer.BindStream(ms);
          ArowSerializer.Serialize(v1r1, writer);
          writer.UnbindStream();

          ms.Position = 0;

          var v2r2 = new Ver2Row();
          reader.BindStream(ms);
          ArowSerializer.Deserialize(v2r2, reader);

          Console.WriteLine(v2r2.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap));

          Aver.AreEqual("A string", v2r2.A);

          Aver.AreEqual(156, v2r2.AmorphousData["b"].AsInt());
          Aver.IsNull(v2r2.AmorphousData["c"]);
          Aver.IsTrue(v2r2.AmorphousData["e"] is byte[]);
          Aver.IsNotNull(v2r2.AmorphousData["g"]);
          Aver.IsTrue(v2r2.AmorphousData["g"] is Array);
          Aver.AreEqual(-998, ((JSONDataMap)((object[])v2r2.AmorphousData["g"])[0])["c"].AsInt());

          Aver.AreEqual(0, v2r2.B);
          Aver.AreEqual(null, v2r2.C);
          Aver.AreEqual(true, v2r2.D);
          Aver.IsNull(v2r2.E);
          Aver.AreEqual(new DateTime(1980, 02, 12), v2r2.F);
          Aver.IsNull(v2r2.G);
        }
      }


    }
}
