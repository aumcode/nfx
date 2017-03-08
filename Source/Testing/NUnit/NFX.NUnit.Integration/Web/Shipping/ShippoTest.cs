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
            }

            carriers
            {
              carrier
              {
                carrier-type='USPS'
                inner-id=USPS
                outer-id=$(~SHIPPO_USPS_CARRIER_ID)
                nls-name { eng{n='USPS' d='United States Postal Service'} }

                methods
                {
                  method
                  {
                    inner-id=USPS_PRIORITY
                    outer-id=usps_priority
                    nls-name { eng{n='Priority Mail' d='USPS Priority Mail'} }
                    price-category=Standard
                  }

                  method
                  {
                    inner-id=USPS_PRIORITY_EXPRESS
                    outer-id=usps_priority_express
                    nls-name { eng{n='Priority Mail Express' d='USPS Priority Mail Express'} }
                    price-category=Expedited
                  }
                  
                  method
                  {
                    inner-id=USPS_PARCEL_SELECT
                    outer-id=usps_parcel_select
                    nls-name { eng{n='Parcel Select' d='USPS Parcel Select'} }
                    price-category=Standard
                  }
                  
                  method
                  {
                    inner-id=USPS_FIRST
                    outer-id=usps_first
                    nls-name { eng{n='First-Class Package/Mail Parcel' d='USPS First-Class Package/Mail Parcel'} }
                    price-category=Saver
                  }
                }

                templates
                {
                  template
                  {
                    inner-id=USPS_FLAT_RATE_ENVELOPE_TEMPLATE
                    outer-id=USPS_FlatRateEnvelope
                    nls-name { eng{n='Flat Rate Envelope' d='USPS Flat Rate Envelope'} }
                  }
                }
              }// carrier USPS

              carrier
              {
                carrier-type='DHLExpress'
                inner-id=DHL_EXPRESS
                outer-id=$(~SHIPPO_DHL_EXPRESS_CARRIER_ID)
                nls-name { eng{n='DHL Express' d='DHL Express'} }

                methods
                {
                  method
                  {
                    inner-id=DHL_EXPRESS_DOMESTIC_EXPRESS_DOC
                    outer-id=dhl_express_domestic_express_doc
                    nls-name { eng{n='Domestic Express Doc' d='DHL Express Domestic Express Doc'} }
                    price-category=Standard
                  }
                }
              }// carrier DHL EXPRESS

            }// carriers

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
    public void CreateLabel_USPS()
    {
      using (var session = ShippingSystem.StartSession())
      {
        var usps = session.GetShippingCarriers(null).First(c => c.Type==CarrierType.USPS);
        var method = usps.Services["USPS_PRIORITY"];
        var template = usps.Packages["USPS_FLAT_RATE_ENVELOPE_TEMPLATE"];

        var shipment = getDefaultShipment(usps, method, template);
        var label = session.CreateLabel(null, shipment);

        Assert.IsNotNull(label);
        Assert.AreEqual(CarrierType.USPS, label.Carrier);
        Assert.AreEqual(LabelFormat.PDF, label.Format);
        Assert.AreEqual("USD", label.Rate.CurrencyISO);
        Assert.IsNotNullOrEmpty(label.TrackingNumber);
        Assert.IsNotNullOrEmpty(label.URL);
      }
    }

    [Test]
    public void CreateLabel_DHLExpress()
    {
      using (var session = ShippingSystem.StartSession())
      {
        var dhl = session.GetShippingCarriers(null).First(c => c.Type==CarrierType.DHLExpress);
        var method = dhl.Services["DHL_EXPRESS_DOMESTIC_EXPRESS_DOC"];

        var shipment = getDefaultShipment(dhl, method);
        var label = session.CreateLabel(null, shipment);

        Assert.IsNotNull(label);
        Assert.AreEqual(CarrierType.DHLExpress, label.Carrier);
        Assert.AreEqual(LabelFormat.PDF, label.Format);
        Assert.AreEqual("USD", label.Rate.CurrencyISO);
        Assert.IsNotNullOrEmpty(label.TrackingNumber);
        Assert.IsNotNullOrEmpty(label.URL);
      }
    }

    [Test]
    public void CreateReturnLabel_USPS()
    {
      using (var session = ShippingSystem.StartSession())
      {
        var usps = session.GetShippingCarriers(null).First(c => c.Type==CarrierType.USPS);
        var method = usps.Services["USPS_PRIORITY"];
        var template = usps.Packages["USPS_FLAT_RATE_ENVELOPE_TEMPLATE"];

        var shipment = getDefaultShipment(usps, method, template);
        shipment.ReturnAddress = getDefaultAddress();
        var label = session.CreateLabel(null, shipment);

        shipment.LabelIDForReturn = label.ID;
        var retLabel = session.CreateLabel(null, shipment);

        Assert.IsNotNull(retLabel);
        Assert.AreEqual(CarrierType.USPS, retLabel.Carrier);
        Assert.AreEqual(LabelFormat.PDF, retLabel.Format);
        Assert.AreEqual("USD", retLabel.Rate.CurrencyISO);
        Assert.IsNotNullOrEmpty(retLabel.URL);
        Assert.IsNotNullOrEmpty(retLabel.TrackingNumber);
      }
    }

    [Test]
    public void CreateReturnLabel_DHLExpress()
    {
      using (var session = ShippingSystem.StartSession())
      {
        var dhl = session.GetShippingCarriers(null).First(c => c.Type==CarrierType.DHLExpress);
        var method = dhl.Services["DHL_EXPRESS_DOMESTIC_EXPRESS_DOC"];

        var shipment = getDefaultShipment(dhl, method);
        shipment.ReturnAddress = getDefaultAddress();
        var label = session.CreateLabel(null, shipment);

        shipment.LabelIDForReturn = label.ID;
        var retLabel = session.CreateLabel(null, shipment);

        Assert.IsNotNull(retLabel);
        Assert.AreEqual(CarrierType.DHLExpress, retLabel.Carrier);
        Assert.AreEqual(LabelFormat.PDF, retLabel.Format);
        Assert.AreEqual("USD", retLabel.Rate.CurrencyISO);
        Assert.IsNotNullOrEmpty(retLabel.URL);
        Assert.IsNotNullOrEmpty(retLabel.TrackingNumber);
      }
    }

    [Test]
    public void EstimateShippingCost_USPS_FlatRateTemplate()
    {
      using (var session = ShippingSystem.StartSession())
      {
        var usps = session.GetShippingCarriers(null).First(c => c.Type==CarrierType.USPS);
        var method = usps.Services["USPS_PRIORITY"];
        var template = usps.Packages["USPS_FLAT_RATE_ENVELOPE_TEMPLATE"];

        var shipment = getDefaultShipment(usps, method, template);
        var amount = session.EstimateShippingCost(null, shipment);

        Assert.IsNotNull(amount);
        Assert.AreEqual(amount.Value.CurrencyISO, "USD");
        Assert.AreEqual(amount.Value.Value, 5.60M);
      }
    }

    [Test]
    public void EstimateShippingCost_USPS_NoTemplate()
    {
      using (var session = ShippingSystem.StartSession())
      {
        var usps = session.GetShippingCarriers(null).First(c => c.Type==CarrierType.USPS);

        var method = usps.Services["USPS_PRIORITY"];
        var shipment = getDefaultShipment(usps, method);
        var amount = session.EstimateShippingCost(null, shipment);

        Assert.IsNotNull(amount);
        Assert.AreEqual(amount.Value.CurrencyISO, "USD");
        Assert.AreEqual(amount.Value.Value, 6.95M);

        method = usps.Services["USPS_PRIORITY_EXPRESS"];
        shipment = getDefaultShipment(usps, method);
        amount = session.EstimateShippingCost(null, shipment);

        Assert.IsNotNull(amount);
        Assert.AreEqual(amount.Value.CurrencyISO, "USD");
        Assert.AreEqual(amount.Value.Value, 28.08M);

        method = usps.Services["USPS_PARCEL_SELECT"];
        shipment = getDefaultShipment(usps, method);
        amount = session.EstimateShippingCost(null, shipment);

        Assert.IsNotNull(amount);
        Assert.AreEqual(amount.Value.CurrencyISO, "USD");
        Assert.AreEqual(amount.Value.Value, 7.11M);

        method = usps.Services["USPS_FIRST"];
        shipment = getDefaultShipment(usps, method);
        amount = session.EstimateShippingCost(null, shipment);

        Assert.IsNotNull(amount);
        Assert.AreEqual(amount.Value.CurrencyISO, "USD");
        Assert.AreEqual(amount.Value.Value, 2.60M);
      }
    }

    [Test]
    public void EstimateShippingCost_DHLExpress()
    {
      using (var session = ShippingSystem.StartSession())
      {
        var dhl = session.GetShippingCarriers(null).First(c => c.Type==CarrierType.DHLExpress);
        var method = dhl.Services["DHL_EXPRESS_DOMESTIC_EXPRESS_DOC"];

        var shipment = getDefaultShipment(dhl, method);
        var amount = session.EstimateShippingCost(null, shipment);

        Assert.IsNull(amount);
      }
    }

    [Test]
    public void ValidateAddress_Valid()
    {
      using (var session = ShippingSystem.StartSession())
      {
        ValidateShippingAddressException error;

        var address = getDefaultAddress();
        var corrAddress = session.ValidateAddress(null, address, out error);

        Assert.IsNull(error);
      }
    }

    [Test]
    public void ValidateAddress_Valid_Corrected()
    {
      using (var session = ShippingSystem.StartSession())
      {
        ValidateShippingAddressException error;

        var address = getDefaultAddress();
        address.City = "Los Angeles";
        var corrAddress = session.ValidateAddress(null, address, out error);

        Assert.IsNull(error);
        Assert.AreEqual("los alamos", corrAddress.City.ToLower());
      }
    }

    [Test]
    public void ValidateAddress_Invalid()
    {
      using (var session = ShippingSystem.StartSession())
      {
        ValidateShippingAddressException error;

        var address = getDefaultAddress();
        address.Line1 = "Starokubanskaya 33";
        var corrAddress = session.ValidateAddress(null, address, out error);
        Assert.IsNotNull(error);
        Assert.IsNull(corrAddress);
        Assert.IsTrue(error.Message.Contains("The address as submitted could not be found"));

        address = getDefaultAddress();
        address.Line1 = "587000 Kiva Street";
        corrAddress = session.ValidateAddress(null, address, out error);
        Assert.IsNotNull(error);
        Assert.IsNull(corrAddress);
        Assert.IsTrue(error.Message.Contains("The address as submitted could not be found"));

        address = getDefaultAddress();
        address.City = "New York";
        address.Postal = "350011";
        corrAddress = session.ValidateAddress(null, address, out error);
        Assert.IsNotNull(error);
        Assert.IsNull(corrAddress);
        Assert.IsTrue(error.Message.Contains("The City in the address submitted is invalid"));
      }
    }

    [Test]
    public void ValidateAddress_Crazy()
    {
      // Shippo API can pass STATUS_INVALID or
      // (!!!!!) STATUS_VALID with error in 'messages' with 'code'="Invalid"
      var crazy = new Address
      {
        PersonName = "Stan Ulam",
        Country = "USA",
        Region = "NM",
        City = "NEW YORK",
        EMail = "s-ulam@myime.com",
        Line1 = "2500TH ST AVE",
        Postal = "10017000000",
        Phone = "(333) 777-77-77"
      };

      using (var session = ShippingSystem.StartSession())
      {
        ValidateShippingAddressException error;
        var corrAddress = session.ValidateAddress(null, crazy, out error);

        Assert.IsNotNull(error);
        Assert.IsNull(corrAddress);
      }
    }

    [Test]
    public void Test_ShippoFixedBugs()
    {
      using (var session = ShippingSystem.StartSession())
      {
        var address = new Address
        {
          PersonName = "Stan Ulam",
          Country = "USA",
          Region = "IN",
          City = "INDIANAPOLIS",
          EMail = "s-ulam@myime.com",
          Line1 = "1004 HOSBROOK ST",
          Postal = "46203-1012",
          Phone = "(333) 777-77-77"
        };

        ValidateShippingAddressException error;
        var corrAddress = session.ValidateAddress(null, address, out error);
        Assert.IsNull(error);
        Assert.IsNotNull(corrAddress);

        address.City = "INDIANAPOLIs";
        corrAddress = session.ValidateAddress(null, address, out error);
        Assert.IsNull(error);
        Assert.IsNotNull(corrAddress);

        corrAddress = session.ValidateAddress(null, corrAddress, out error);
        Assert.IsNull(error);
        Assert.IsNotNull(corrAddress);

        address = new Address
        {
          PersonName = "Stan Ulam",
          Country = "USA",
          Region = "NM",
          City = "LOS ALAMOS",
          EMail = "s-ulam@myime.com",
          Line1 = "587 KIVA ST",
          Postal = "87544",
          Phone = "(333) 777-77-77"
        };

        corrAddress = session.ValidateAddress(null, address, out error);
        Assert.IsNull(error);
        Assert.IsNotNull(corrAddress);

        corrAddress = session.ValidateAddress(null, corrAddress, out error);
        Assert.IsNull(error);
        Assert.IsNotNull(corrAddress);
      }
    }

    [Test]
    [ExpectedException(typeof(ShippingException), ExpectedMessage ="Operation failed", MatchType = MessageMatch.Contains)]
    public void Test_TrackingNumber()
    {
      // just a stub
      // we can not track sandbox shipments
      using (var session = ShippingSystem.StartSession())
      {
        var track = session.TrackShipment(null, "USPS", "12334fsfsdfw3r32");
      }
    }


    private Address getDefaultAddress()
    {
      var result = new Address
      {
        PersonName = "Stan Ulam",
        Country = "USA",
        Region = "NM",
        City = "LOS ALAMOS",
        EMail = "s-ulam@myime.com",
        Line1 = "587 KIVA ST",
        Postal = "87544",
        Phone = "(333) 777-77-77"
      };

      return result;
    }

    private Shipment getDefaultShipment(ShippingCarrier carrier, ShippingCarrier.Service method, ShippingCarrier.Package template = null)
    {
      var result = new Shipment
      {
        Length = 0.5M,
        Width = 0.5M,
        Height = 0.5M,
        DistanceUnit = NFX.Standards.Distance.UnitType.In,
        Weight = 0.3M,
        WeightUnit = NFX.Standards.Weight.UnitType.Lb,
        Carrier = carrier,
        Service = method,
        Package = template,
        FromAddress = new Address
        {
          PersonName = "J. London",
          Country = "US",
          Region = "NY",
          City = "New York",
          EMail = "jlondon@myime.com",
          Line1 = "183 Canal Street",
          Postal = "10013",
          Phone = "(111) 222-33-44"
        },
        ToAddress = new Address
        {
          PersonName = "A. Einstein",
          Country = "US",
          Region = "CA",
          City = "Los Angeles",
          EMail = "aeinstein@myime.com",
          Line1 = "1782 West 25th Street",
          Postal = "90018",
          Phone = "(111) 333-44-55"
        }
      };

      return result;
    }
  }
}
