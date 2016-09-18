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
using System.Globalization;
using System.Linq;
using System.Text;

using NFX;
using NFX.Environment;
using NFX.Log;
using NFX.Serialization.JSON;

namespace NFX.Web.Shipping.Shippo
{
  public class ShippoSystem : ShippingSystem
  {
    #region CONSTS

      public const string SHIPPO_REALM = "shippo";

      public const string PURCHASE_LABEL_PURPOSE = "PURCHASE";
      public const string STATUS_SUCCESS = "SUCCESS";
      public const string STATUS_ERROR = "ERROR";

      public const string USPS_PRIORITY_SERVICE = "usps_priority";

      public const string HDR_AUTHORIZATION = "Authorization";
      public const string HDR_AUTHORIZATION_TOKEN = "ShippoToken {0}";

      public const string URI_API_BASE = "https://api.goshippo.com";
      public const string URI_TRANSACTIONS = "/v1/transactions";
      public const string URI_RATES = "/v1/rates/{0}";
      public const string URI_TRACKING = "v1/tracks/";

      public static readonly Dictionary<LabelFormat, string> FORMATS = new Dictionary<LabelFormat, string>
      {
        { LabelFormat.PDF, "PDF" },
        { LabelFormat.PDF_4X6 , "PDF_4X6" },
        { LabelFormat.PNG, "PNG" },
        { LabelFormat.ZPLII, "ZPLII" }
      };

      public static readonly Dictionary<Carrier, string> CARRIERS = new Dictionary<Carrier, string>
      {
        { Carrier.USPS, "USPS" }
      };

      public static readonly Dictionary<DistanceUnit, string> DIST_UNITS = new Dictionary<DistanceUnit, string>
      {
        { DistanceUnit.In, "in" }
      };

      public static readonly Dictionary<MassUnit, string> MASS_UNITS = new Dictionary<MassUnit, string>
      {
        { MassUnit.Lb, "lb" }
      };

      public static readonly Dictionary<ParcelTemplate, string> PARCEL_TEMPLATES = new Dictionary<ParcelTemplate, string>
      {
        { ParcelTemplate.USPS_FlatRateEnvelope, "USPS_FlatRateEnvelope" }
      };

    #endregion

    #region .ctor

      public ShippoSystem(string name, IConfigSectionNode node) : base(name, node)
      {
      }

      public ShippoSystem(string name, IConfigSectionNode node, object director) : base(name, node, director)
      {
      }

    #endregion

    #region IShippingSystem impl

      protected override ShippingSession DoStartSession(ShippingConnectionParameters cParams = null)
      {
        cParams = cParams ?? DefaultSessionConnectParams;
        return new ShippoSession(this, (ShippoConnectionParameters)cParams);
      }

      protected override ShippingConnectionParameters MakeDefaultSessionConnectParams(IConfigSectionNode paramsSection)
      {
        return ShippingConnectionParameters.Make<ShippoConnectionParameters>(paramsSection);
      }

      public override Label CreateLabel(ShippingSession session, IShippingContext context, Shipment shipment)
      {
        return CreateLabel((ShippoSession)session, context, shipment);
      }

      public Label CreateLabel(ShippoSession session, IShippingContext context, Shipment shipment)
      {
        var logID = Log(MessageType.Info, "CreateLabel()", StringConsts.SHIPPO_CREATE_LABEL_MESSAGE.Args(shipment.FromAddress, shipment.ToAddress));

        try
        {
          return doCreateLabel(session, context, shipment, null, logID);
        }
        catch (Exception ex)
        {
          StatCreateLabelError();

          var header = StringConsts.SHIPPO_CREATE_LABEL_ERROR.Args(shipment.FromAddress, shipment.ToAddress, ex.ToMessageWithType());
          Log(MessageType.Error, "CreateLabel()", header, ex, logID);
          var error = ShippingException.ComposeError(ex.Message, ex);

          throw error;
        }
      }

      public override Label CreateReturnLabel(ShippingSession session, IShippingContext context, Shipment shipment, object labelID)
      {
        return CreateReturnLabel((ShippoSession)session, context, shipment, labelID);
      }

      public Label CreateReturnLabel(ShippoSession session, IShippingContext context, Shipment shipment, object labelID)
      {
        var logID = Log(MessageType.Info, "CreateReturnLabel()", StringConsts.SHIPPO_CREATE_LABEL_MESSAGE.Args(shipment.FromAddress, shipment.ToAddress));

        try
        {
          return doCreateLabel(session, context, shipment, labelID, logID);
        }
        catch (Exception ex)
        {
          StatCreateReturnLabelError();

          var header = StringConsts.SHIPPO_CREATE_RETURN_LABEL_ERROR
                                   .Args(shipment.FromAddress,
                                         shipment.ToAddress,
                                         shipment.ReturnAddress ?? shipment.FromAddress,
                                         ex.ToMessageWithType());
          Log(MessageType.Error, "CreateReturnLabel()", header, ex, logID);
          var error = ShippingException.ComposeError(ex.Message, ex);

          throw error;
        }
      }

      public override TrackInfo TrackShipment(ShippingSession session, IShippingContext context, string trackingNumber)
      {
        return TrackShipment((ShippoSession)session, context, trackingNumber);
      }

      public TrackInfo TrackShipment(ShippoSession session, IShippingContext context, string trackingNumber) // carrier ID???
      {
        var logID = Log(MessageType.Info, "TrackShipment()", StringConsts.SHIPPO_TRACK_SHIPMENT_MESSAGE.Args(trackingNumber));

        try
        {
          return doTrackShipment(session, context, trackingNumber, logID);
        }
        catch (Exception ex)
        {
          StatCreateLabelError();

          var header = StringConsts.SHIPPO_TRACK_SHIPMENT_ERROR.Args(trackingNumber,ex.ToMessageWithType());
          Log(MessageType.Error, "TrackShipment()", header, ex, logID);
          var error = ShippingException.ComposeError(ex.Message, ex);

          throw error;
        }
      }

    #endregion

    #region .pvt

      private Label doCreateLabel(ShippoSession session, IShippingContext context, Shipment shipment, object labelID, Guid logID)
      {
        var cred = (ShippoCredentials)session.User.Credentials;

        // label request
        var request = new WebClient.RequestParams
        {
          Caller = this,
          Uri = new Uri(URI_API_BASE + URI_TRANSACTIONS),
          Method = HTTPRequestMethod.POST,
          ContentType = ContentType.JSON,
          Headers = new Dictionary<string, string>
            {
              { HDR_AUTHORIZATION, HDR_AUTHORIZATION_TOKEN.Args(cred.PrivateToken) }
            },
          Body = getCreateLabelRequestBody(session, shipment, labelID)
        };

        var response = WebClient.GetJson(request);

        Log(MessageType.Info, "doCreateLabel()", response.ToJSON(), relatedMessageID: logID);

        checkResponse(response);

        // get label bin data from URL in response
        string labelURL = response["label_url"].AsString().EscapeURIStringWithPlus();
        string contentType;
        var data = WebClient.GetData(new Uri(labelURL), this, out contentType);

        // get used rate, fill label's data
        var id = response["object_id"];
        var trackingNumber = response["tracking_number"].AsString();
        var rate = getRate(session, response["rate"].AsString(), logID);
        var carrier = rate != null ?
                      CARRIERS.First(c => c.Value.EqualsIgnoreCase(rate["provider"].AsString())).Key :
                      Carrier.Unknown;
        var amount = rate != null ?
                     new NFX.Financial.Amount(rate["currency"].AsString(), rate["amount"].AsDecimal()) :
                     new NFX.Financial.Amount(string.Empty, 0);

        var label = new Label(id,
                              labelURL,
                              data,
                              shipment.LabelFormat,
                              trackingNumber,
                              carrier,
                              amount);

        if (labelID == null) StatCreateLabel();
        else StatCreateReturnLabel();

        return label;
      }

      private TrackInfo doTrackShipment(ShippoSession session, IShippingContext context, string trackingNumber, Guid logID)
      {
        throw new NotImplementedException();
      }

      private void checkResponse(JSONDataMap response)
      {
        if (response == null) throw new ShippingException("checkResponse(response=null)");

        if (!response["object_status"].AsString().EqualsIgnoreCase(STATUS_SUCCESS))
        {
          string text = null;
          var messages = response["messages"] as JSONDataArray;
          if (messages != null && messages.Any())
          {
            var message = (JSONDataMap)messages.First();
            text = message["text"].AsString();
          }

          if (text.IsNullOrWhiteSpace()) text = StringConsts.SHIPPO_CREATE_LABEL_FAIL;

          throw new ShippingException(text);
        }
      }

      private string getCreateLabelRequestBody(ShippoSession session, Shipment shipment, object labelID)
      {
        shipment.CarrierID = session.ConnectionParams.CarrierID; // use USPS for the moment
        shipment.ServiceID = USPS_PRIORITY_SERVICE;

        var isReturn = labelID != null;

        var body = new JSONDataMap();
        body["carrier_account"] = shipment.CarrierID;
        body["servicelevel_token"] = shipment.ServiceID;
        body["label_file_type"] = FORMATS[shipment.LabelFormat];
        body["async"] = false;

        var shpm = new JSONDataMap();
        shpm["object_purpose"] = PURCHASE_LABEL_PURPOSE;
        shpm["address_from"] = getAddressBody(shipment.FromAddress);
        shpm["address_to"] = getAddressBody(shipment.ToAddress);
        if (!isReturn && shipment.ReturnAddress.HasValue)
          shpm["address_return"] = getAddressBody(shipment.ReturnAddress.Value);
        body["shipment"] = shpm;

        var parcel = new JSONDataMap();
        if (shipment.Template.HasValue)
          parcel["template"] = PARCEL_TEMPLATES[shipment.Template.Value];
        parcel["length"] = shipment.Length.ToString(CultureInfo.InvariantCulture);
        parcel["width"] = shipment.Width.ToString(CultureInfo.InvariantCulture);
        parcel["height"] = shipment.Height.ToString(CultureInfo.InvariantCulture);
        parcel["distance_unit"] = DIST_UNITS[shipment.DistanceUnit];
        parcel["weight"] = shipment.Weight.ToString(CultureInfo.InvariantCulture);
        parcel["mass_unit"] = MASS_UNITS[shipment.MassUnit];

        shpm["parcel"] = parcel;

        if (isReturn) shpm["return_of"] = labelID;

        return body.ToJSON(JSONWritingOptions.Compact);
      }

      private JSONDataMap getAddressBody(Address addr)
      {
        var result = new JSONDataMap();
        result["object_purpose"] = PURCHASE_LABEL_PURPOSE;
        result["name"] = addr.Name;
        result["street1"] = addr.Street1;
        result["street2"] = addr.Street2;
        result["city"] = addr.City;
        result["state"] = addr.State;
        result["zip"] = addr.PostalCode;
        result["country"] = addr.Country;
        result["phone"] = addr.Phone;
        result["email"] = addr.EMail;

        return result;
      }

      private JSONDataMap getRate(ShippoSession session, string rateID, Guid logID)
      {
        try
        {
          var cred = (ShippoCredentials)session.User.Credentials;

          var request = new WebClient.RequestParams
          {
            Caller = this,
            Uri = new Uri((URI_API_BASE + URI_RATES).Args(rateID)),
            Method = HTTPRequestMethod.GET,
            ContentType = ContentType.JSON,
            Headers = new Dictionary<string, string>
              {
                { HDR_AUTHORIZATION, HDR_AUTHORIZATION_TOKEN.Args(cred.PrivateToken) }
              }
          };

          return WebClient.GetJson(request);
        }
        catch (Exception ex)
        {
          var error = ShippingException.ComposeError(ex.Message, ex);
          Log(MessageType.Error, "getRate()", StringConsts.SHIPPO_CREATE_LABEL_ERROR, error, relatedMessageID: logID);
          return null;
        }
      }

    #endregion
  }
}
