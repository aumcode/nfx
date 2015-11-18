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
using System.Text;

using NUnit.Framework;

using NFX.DataAccess.CRUD;
using NFX.DataAccess.Erlang;
using NFX.Serialization.JSON;

namespace NFX.NUnit.Integration.CRUD
{
  /// <summary>
  /// Mongo CRUD tests
  /// </summary>
  [TestFixture]
  public class ErlangTests
  {
    private const string SCRIPT_ASM = "NFX.NUnit.Integration";

    private const string REMOTE_NAME = "nfx@localhost";
    private const string REMOTE_COOKIE = "klubnika";


    private ErlDataStore store;

    [SetUp]
    public void BeforeTest()
    {
      store = new ErlDataStore();
      store.RemoteName = REMOTE_NAME;
      store.RemoteCookie = REMOTE_COOKIE;
      store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
      //store.QueryResolver.RegisterHandlerLocation("NFX.NUnit.Integration.CRUD.ErlSpecific, NFX.NUnit.Integration");
      store.Start();
      clearAll();
    }

    [TearDown]
    public void AfterTest()
    {
      DisposableObject.DisposeAndNull(ref store);
    }

    private void clearAll()
    {

    }

    //==========================================================================================


    [Test]
    public void ErlConnect()
    {
      var qry = new Query("CRUD.Echo")
        {
          new Query.Param("Msg", "Lenin!")
        };

      var row = store.LoadOneRow(qry);

      Assert.IsNotNull(row);
      Assert.AreEqual("You said: Lenin!", row["echoed_msg"]);
      Assert.AreEqual(DateTime.UtcNow.Year, row["ts"].AsDateTime().Year);
    }


    [Test]
    public void ErlInsert()
    {
      var schema = store.GetSchema(new Query("CRUD.SECDEF"));
      var row = new DynamicRow(schema);

      row.ApplyDefaultFieldValues(store.TargetName);

      row["Exchange"] = "HS";
      row["Symbol"] = "EURUSD";
      row["Instr"] =  "EUR/USD";
      row["SecID"] = 1;
      row["ExchSecID"] = 1010;
      row["Ccy"] = "EUR";
      row["SettlCcy"] = "USD";

      row["ContractMult"] = 1.0d;
      row["PriceStep"] = 10e-5d;

      Assert.IsNull(row.Validate());

      var affected = store.Insert(row);
      Assert.AreEqual(1, affected);
    }


    [Test]
    public void ErlInsertManyAndQuery()
    {
      var schema = store.GetSchema(new Query("CRUD.SECDEF"));

      var pairs = new string[]
      {
        "AUD/CAD", "AUD/CHF", "AUD/HKD", "AUD/JPY", "AUD/NZD", "AUD/USD",
        "CAD/JPY", "CHF/JPY", "CHF/NOK", "CHF/SEK", "EUR/AUD", "EUR/CAD",
        "EUR/CHF", "EUR/CZK", "EUR/DKK", "EUR/GBP", "EUR/HKD", "EUR/HUF",
        "EUR/JPY", "EUR/NOK", "EUR/NZD", "EUR/PLN", "EUR/RUB", "EUR/SEK",
        "EUR/TRY", "EUR/USD", "EUR/ZAR", "GBP/AUD", "GBP/CAD", "GBP/CHF",
        "GBP/CZK", "GBP/HUF", "GBP/JPY", "GBP/NOK", "GBP/NZD", "GBP/PLN",
        "GBP/SEK", "GBP/USD", "HKD/JPY", "NOK/SEK", "NZD/JPY", "NZD/USD",
        "USD/CAD", "USD/CHF", "USD/CNH", "USD/CZK", "USD/DKK", "USD/HKD",
        "USD/HUF", "USD/ILS", "USD/JPY", "USD/MXN", "USD/NOK", "USD/PLN",
        "USD/RUB", "USD/SEK", "USD/SGD", "USD/THB", "USD/TRY", "USD/ZAR",
        "XAG/USD", "XAU/USD", "ZAR/JPY"
      };

      for (var i = 0; i < 10; i++)
      {
        var row = new DynamicRow(schema);

        row.ApplyDefaultFieldValues(store.TargetName);

        var ccy1 = pairs[i].Substring(0, 3);
        var ccy2 = pairs[i].Substring(4, 3);

        row["Exchange"]     = "CLE";
        row["Symbol"]       = ccy1 + ccy2;
        row["Instr"]        = pairs[i];
        row["SecID"]        = i;
        row["ExchSecID"]    = 1000 + i;
        row["Ccy"]          = ccy1;
        row["SettlCcy"]     = ccy2;

        row["ContractMult"] = 1.0d;
        row["PriceStep"]    = 10e-5d;

        Assert.IsNull(row.Validate());

        var affected = store.Insert(row);
        Assert.AreEqual(1, affected);
      }

      var qry = new Query("CRUD.SecDef.ByExchange")
      {
          new Query.Param("Exchange", "CLE")
      };

      var data = store.LoadOneRowset(qry);

      Assert.IsNotNull(data);

      Console.WriteLine(data.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap));

      Assert.AreEqual(11, data.Count);


    }


  }
}
