/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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

using NUnit.Framework;

using NFX.ApplicationModel;
using NFX.Web.Shipping;

namespace NFX.NUnit.Integration.Web.Shipping
{
  [TestFixture]
  public class ShippoTest
  {
    public const string LACONF = @"
    nfx
    {
      starters
      {
        starter
        {
          name='Shipping Processing 1'
          type='NFX.Web.Shipping.ShippingSystemStarter, NFX.Web'
          application-start-break-on-exception=true
        }
      }

      web-settings
      {
        shipping-processing
        {
          shipping-system-host
          {
            name='PrimaryShippingSystemHost'
            type='NFX.NUnit.Integration.Web.Shipping.FakeShippingSystemHost, NFX.NUnit.Integration'
          }

          shipping-system
          {
            name='Shippo'
            type='NFX.Web.Shipping.Shippo.ShippoSystem, NFX.Web'
            auto-start=true

            default-session-connect-params
            {
              name='ShippoDefaultConnection'
              type='NFX.Web.Shipping.Shippo.ShippoConnectionParameters, NFX.Web'

              private-token=$(~SHIPPO_PRIVATE_TOKEN)
              public-token=$(~SHIPPO_PUBLIC_TOKEN)
              carrier-id=$(~SHIPPO_USPS_CARRIER_ID)
            }
          }
        }
      }
    }";

    private ServiceBaseApplication m_App;
    private ShippingSystem m_ShippingSystem;

    public ShippingSystem ShippingSystem
    {
      get
      {
        if (m_ShippingSystem == null)
          m_ShippingSystem = NFX.Web.Shipping.ShippingSystem.Instances["Shippo"];

        return m_ShippingSystem;
      }
    }


    [TestFixtureSetUp]
    public void SetUp()
    {
      var config = LACONF.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
      m_App = new ServiceBaseApplication(null, config);
    }

    [TestFixtureTearDown]
    public void TearDown()
    {
      DisposableObject.DisposeAndNull(ref m_App);
    }


    [Test]
    public void CreateLabel()
    {
      using (var session = ShippingSystem.StartSession())
      {
        var shipment = getDefaultShipment();
        var label = session.CreateLabel(null, shipment);

        Assert.IsNotNull(label);
        Assert.AreEqual(Carrier.USPS, label.Carrier);
        Assert.AreEqual(LabelFormat.PDF, label.Format);
        Assert.AreEqual("USD", label.Rate.CurrencyISO);
        Assert.IsNotNullOrEmpty(label.TrackingNumber);
        Assert.IsNotNullOrEmpty(label.URL);
        Assert.IsTrue(label.Data.Length > 0);
      }
    }

    [Test]
    public void CreateReturnLabel()
    {
      using (var session = ShippingSystem.StartSession())
      {
        var shipment = getDefaultShipment();
        shipment.ReturnAddress = getDefaultAddress();
        var label = session.CreateLabel(null, shipment);

        var retLabel = session.CreateReturnLabel(null, shipment, label.ID);

        Assert.IsNotNull(retLabel);
        Assert.AreEqual(Carrier.USPS, retLabel.Carrier);
        Assert.AreEqual(LabelFormat.PDF, retLabel.Format);
        Assert.AreEqual("USD", retLabel.Rate.CurrencyISO);
        Assert.IsNotNullOrEmpty(retLabel.URL);
        Assert.IsNotNullOrEmpty(retLabel.TrackingNumber);
        Assert.IsTrue(retLabel.Data.Length > 0);
      }
    }


    private Address getDefaultAddress()
    {
      var result = new Address
      {
        Name = "Stan Ulam",
        Country = "US",
        State = "NM",
        City = "Los Alamos",
        EMail = "s-ulam@itadapter.com",
        Street1 = "587 Kiva Street",
        PostalCode = "87544",
        Phone = "(333) 777-77-77"
      };

      return result;
    }

    private Shipment getDefaultShipment()
    {
      var result = new Shipment
      {
        Length = 10,
        Width = 5.5M,
        Height = 3,
        DistanceUnit = DistanceUnit.In,
        Weight = 2.3M,
        MassUnit = MassUnit.Lb,
        FromAddress = new Address
        {
          Name = "J. London",
          Country = "US",
          State = "NY",
          City = "New York",
          EMail = "jlondon@itadapter.com",
          Street1 = "183 Canal Street",
          PostalCode = "10013",
          Phone = "(111) 222-33-44"
        },
        ToAddress = new Address
        {
          Name = "A. Einstein",
          Country = "US",
          State = "CA",
          City = "Los Angeles",
          EMail = "aeinstein@itadapter.com",
          Street1 = "1782 West 25th Street",
          PostalCode = "90018",
          Phone = "(111) 333-44-55"
        }
      };

      return result;
    }
  }
}
