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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.ApplicationModel;
using NFX.Environment;
using NFX.Log;
using NFX.Parsing;
using NFX.Web.GeoLookup;

namespace NFX.NUnit.Integration.GeoLookup
{
  [TestFixture]
  public class GeoLookupTest
  {
    public const string LACONF = @"
      app
      {
        geo-lookup
        {
          data-path=$(~NFX_GEODATA)
          resolution=city
        }
      }";

    private ServiceBaseApplication m_App;

    public IGeoLookup Service { get; set; }

    public string DataPath { get { return App.ConfigRoot.Navigate("!geo-lookup/$data-path").Value; } }

    [TestFixtureSetUp]
    public void SetUp()
    {
      var config = LACONF.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
      m_App = new ServiceBaseApplication(null, config);

      var service = new GeoLookupService();
      service.Configure(config["geo-lookup"]);
      service.Start();
      Service = service;
    }

    [TestFixtureTearDown]
    public void TearDown()
    {
      DisposableObject.DisposeAndNull(ref m_App);
    }

    [Test]
    public void LookupAllIPv4()
    {
      using (var reader = new StreamReader(Path.Combine(DataPath, "GeoLite2-{0}-Blocks-IPv4.csv".Args("city"))))
      {
        foreach (var row in reader.AsCharEnumerable().ParseCSV(skipHeader: true, columns: 2, skipIfMore: true))
        {
          var record = row.ToArray();
          var subnet = record[0];
          var geoname = record[1];
          var parts = subnet.Split('/');
          var address = IPAddress.Parse(parts[0]);
          var geoentity = Service.Lookup(address);
          Assert.AreEqual(geoname, geoentity.Block.LocationID.Value);
        }
      }
    }

    [Test]
    public void LookupAllIPv6()
    {
      using (var reader = new StreamReader(Path.Combine(DataPath, "GeoLite2-{0}-Blocks-IPv6.csv".Args("city"))))
      {
        foreach (var row in reader.AsCharEnumerable().ParseCSV(skipHeader: true, columns: 2, skipIfMore: true))
        {
          var record = row.ToArray();
          var subnet = record[0];
          var geoname = record[1];
          var parts = subnet.Split('/');
          var address = IPAddress.Parse(parts[0]);
          var geoentity = Service.Lookup(address);
          Assert.AreEqual(geoname, geoentity.Block.LocationID.Value);
        }
      }
    }
  }
}
