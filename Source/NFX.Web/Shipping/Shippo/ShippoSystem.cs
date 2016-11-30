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

      public const string PURCHASE_PURPOSE = "PURCHASE";
      public const string QUOTE_PURPOSE = "QUOTE";
      public const string STATUS_SUCCESS = "SUCCESS";
      public const string STATUS_ERROR = "ERROR";
      public const string STATUS_VALID = "VALID";
      public const string STATUS_INVALID = "INVALID";
      public const string CODE_INVALID = "Invalid";

      public const string HDR_AUTHORIZATION = "Authorization";
      public const string HDR_AUTHORIZATION_TOKEN = "ShippoToken {0}";

      public const string URI_TRACKING_FORM = "http://tracking.goshippo.com/";
      public const string URI_API_BASE = "https://api.goshippo.com";
      public const string URI_TRANSACTIONS = "/v1/transactions";
      public const string URI_RATES = "/v1/rates/{0}";
      public const string URI_TRACKING = "/v1/tracks/{0}/{1}";
      public const string URI_ADDRESS = "/v1/addresses";
      public const string URI_SHIPMENTS = "/v1/shipments";

      public static readonly Dictionary<LabelFormat, string> FORMATS = new Dictionary<LabelFormat, string>
      {
        { LabelFormat.PDF, "PDF" },
        { LabelFormat.PDF_4X6 , "PDF_4X6" },
        { LabelFormat.PNG, "PNG" },
        { LabelFormat.ZPLII, "ZPLII" }
      };

      public static readonly Dictionary<DistanceUnit, string> DIST_UNITS = new Dictionary<DistanceUnit, string>
      {
        { DistanceUnit.Cm, "cm" },
        { DistanceUnit.In, "in" },
        { DistanceUnit.Ft, "ft" },
        { DistanceUnit.Mm, "mm" },
        { DistanceUnit.M, "m" },
        { DistanceUnit.Yd, "yd" }
      };

      public static readonly Dictionary<MassUnit, string> MASS_UNITS = new Dictionary<MassUnit, string>
      {
        { MassUnit.G, "g" },
        { MassUnit.Oz, "oz" },
        { MassUnit.Lb, "lb" },
        { MassUnit.Kg, "kg" }
      };

      public static readonly Dictionary<string, string> CARRIERS = new Dictionary<string, string>
      {
        { "USPS", "usps" },
        { "DHL_EXPRESS", "dhl_express" },
        { "FEDEX", "fedex" },
        { "UPS", "ups" }
      };

      public static readonly Dictionary<string, TrackStatus> TRACK_STATUSES = new Dictionary<string, TrackStatus>
      {
        { "UNKNOWN", TrackStatus.Unknown },
        { "DELIVERED",TrackStatus.Delivered },
        { "TRANSIT", TrackStatus.Transit },
        { "FAILURE", TrackStatus.Failure },
        { "RETURNED", TrackStatus.Returned }
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
          return doCreateLabel(session, context, shipment, logID);
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

      public override TrackInfo TrackShipment(ShippingSession session, IShippingContext context, string carrierID, string trackingNumber)
      {
        return TrackShipment((ShippoSession)session, context, carrierID, trackingNumber);
      }

      public TrackInfo TrackShipment(ShippoSession session, IShippingContext context, string carrierID, string trackingNumber)
      {
        var logID = Log(MessageType.Info, "TrackShipment()", StringConsts.SHIPPO_TRACK_SHIPMENT_MESSAGE.Args(trackingNumber));

        try
        {
          return doTrackShipment(session, context, carrierID, trackingNumber, logID);
        }
        catch (Exception ex)
        {
          StatTrackShipmentErrorCount();

          var header = StringConsts.SHIPPO_TRACK_SHIPMENT_ERROR.Args(trackingNumber,ex.ToMessageWithType());
          Log(MessageType.Error, "TrackShipment()", header, ex, logID);
          var error = ShippingException.ComposeError(ex.Message, ex);

          throw error;
        }
      }

      public override Address ValidateAddress(ShippingSession session, IShippingContext context, Address address, out ValidateShippingAddressException error)
      {
        return ValidateAddress((ShippoSession)session, context, address, out error);
      }

      public Address ValidateAddress(ShippoSession session, IShippingContext context, Address address, out ValidateShippingAddressException error)
      {
        var logID = Log(MessageType.Info, "ValidateAddress()", StringConsts.SHIPPO_VALIDATE_ADDRESS_MESSAGE);

        try
        {
          return doValidateAddress(session, context, address, logID, out error);
        }
        catch (Exception ex)
        {
          StatValidateAddressErrorCount();

          var header = StringConsts.SHIPPO_VALIDATE_ADDRESS_ERROR.Args(ex.ToMessageWithType());
          Log(MessageType.Error, "ValidateAddress()", header, ex, logID);
          throw ShippingException.ComposeError(ex.Message, ex);
        }
      }

      public override Financial.Amount? EstimateShippingCost(ShippingSession session, IShippingContext context, Shipment shipment)
      {
        return EstimateShippingCost((ShippoSession)session, context, shipment);
      }

      public Financial.Amount? EstimateShippingCost(ShippoSession session, IShippingContext context, Shipment shipment)
      {
        var logID = Log(MessageType.Info, "EstimateShippingCost()", StringConsts.SHIPPO_ESTIMATE_SHIPPING_COST_MESSAGE.Args(shipment.FromAddress, shipment.ToAddress, shipment.Method.InnerID));

        try
        {
          return doEstimateShippingCost(session, context, shipment, logID);
        }
        catch (Exception ex)
        {
          StatEstimateShippingCostErrorCount();

          var header = StringConsts.SHIPPO_ESTIMATE_SHIPPING_COST_ERROR.Args(shipment.FromAddress, shipment.ToAddress, shipment.Method.InnerID, ex.ToMessageWithType());
          Log(MessageType.Error, "EstimateShippingCost()", header, ex, logID);
          var error = ShippingException.ComposeError(ex.Message, ex);

          throw error;
        }
      }

    #endregion

    #region .pvt

      private Label doCreateLabel(ShippoSession session, IShippingContext context, Shipment shipment, Guid logID)
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
          Body = getCreateLabelRequestBody(session, shipment).ToJSON(JSONWritingOptions.Compact)
        };

        var response = WebClient.GetJson(request);

        Log(MessageType.Info, "doCreateLabel()", response.ToJSON(), relatedMessageID: logID);

        checkResponse(response);

        // get label bin data from URL in response
        string labelURL = response["label_url"].AsString().EscapeURIStringWithPlus();

        // get used rate, fill label's data
        var id = response["object_id"].AsString();
        var trackingNumber = response["tracking_number"].AsString();
        var rate = doGetRate(session, response["rate"].AsString(), logID);
        var amount = rate != null ?
                     new NFX.Financial.Amount(rate["currency"].AsString(), rate["amount"].AsDecimal()) :
                     new NFX.Financial.Amount(string.Empty, 0);

        var label = new Label(id,
                              labelURL,
                              shipment.LabelFormat,
                              trackingNumber,
                              shipment.Carrier.Type,
                              amount);

        StatCreateLabel();

        return label;
      }

      private TrackInfo doTrackShipment(ShippoSession session, IShippingContext context, string carrierID, string trackingNumber, Guid logID)
      {
        if (trackingNumber.IsNullOrWhiteSpace())
          throw new ShippingException("Tracking number is empty");

        string ccode;
        if (!CARRIERS.TryGetValue(carrierID, out ccode))
          throw new ShippingException("Unknown carrier");

        var cred = (ShippoCredentials)session.User.Credentials;

        var request = new WebClient.RequestParams
        {
          Caller = this,
          Uri = new Uri((URI_API_BASE + URI_TRACKING).Args(ccode, trackingNumber)),
          Method = HTTPRequestMethod.GET,
          ContentType = ContentType.JSON,
          Headers = new Dictionary<string, string>
            {
              { HDR_AUTHORIZATION, HDR_AUTHORIZATION_TOKEN.Args(cred.PrivateToken) }
            }
        };

        var response = WebClient.GetJson(request);

        var result = new TrackInfo();

        var status = response["tracking_status"] as JSONDataMap;
        if (status == null)
          throw new ShippingException("Tracking status is not available");

        TrackStatus ts;
        if (status["status"] != null && TRACK_STATUSES.TryGetValue(status["status"].AsString(), out ts))
          result.Status = ts;

        DateTime date;
        if (status["status_date"] != null && DateTime.TryParse(status["status_date"].AsString(), out date))
          result.Date = date;

        result.Details = status["status_details"].AsString();
        result.Location = getAddressFromJSON(status["location"] as JSONDataMap);

        var service = response["servicelevel"] as JSONDataMap;
        if (service != null) result.Service = service["name"].AsString();
        result.TrackingNumber = trackingNumber;
        result.FromAddress = getAddressFromJSON(response["address_from"] as JSONDataMap);
        result.ToAddress = getAddressFromJSON(response["address_to"] as JSONDataMap);

        var history = response["tracking_history"] as JSONDataArray;
        if (history != null)
        {
          foreach (JSONDataMap hitem in history)
          {
            var hi = new TrackInfo.HistoryItem();

            if (hitem["status"] != null && TRACK_STATUSES.TryGetValue(hitem["status"].AsString(), out ts))
              hi.Status = ts;

            hi.Details = hitem["status_details"].AsString();

            if (hitem["status_date"] != null && DateTime.TryParse(hitem["status_date"].AsString(), out date))
              hi.Date = date;

            hi.Location = getAddressFromJSON(hitem["location"] as JSONDataMap);

            result.History.Add(hi);
          }
        }

        return result;
      }

      private Address doValidateAddress(ShippoSession session, IShippingContext context, Address address, Guid logID, out ValidateShippingAddressException error)
      {
        error = null;
        var cred = (ShippoCredentials)session.User.Credentials;
        var body = getAddressBody(address);
        body["validate"] = true;

        // validate address request
        var request = new WebClient.RequestParams
        {
          Caller = this,
          Uri = new Uri(URI_API_BASE + URI_ADDRESS),
          Method = HTTPRequestMethod.POST,
          ContentType = ContentType.JSON,
          Headers = new Dictionary<string, string>
            {
              { HDR_AUTHORIZATION, HDR_AUTHORIZATION_TOKEN.Args(cred.PrivateToken) }
            },
          Body = body.ToJSON(JSONWritingOptions.Compact)
        };

        var response = WebClient.GetJson(request);

        Log(MessageType.Info, "doValidateAddress()", response.ToJSON(), relatedMessageID: logID);

        // check for validation errors:
        // Shippo API can return STATUS_INVALID or (!!!) STATUS_VALID but with 'code'="Invalid"
        var state = response["object_state"].AsString(STATUS_INVALID);
        var messages = response["messages"] as JSONDataArray;
        JSONDataMap message = null;
        var code = string.Empty;
        var text = string.Empty;
        if (messages != null) message = messages.FirstOrDefault() as JSONDataMap;
        if (message != null)
        {
          code = message["code"].AsString(string.Empty);
          text = message["text"].AsString(string.Empty);
        }

        // error found
        if (!state.EqualsIgnoreCase(STATUS_VALID) || code.EqualsIgnoreCase(CODE_INVALID))
        {
          var errMess = StringConsts.SHIPPO_VALIDATE_ADDRESS_INVALID_ERROR.Args(text);
          Log(MessageType.Error, "doValidateAddress()", errMess, relatedMessageID: logID);
          error = new ValidateShippingAddressException(errMess, text);
          return null;
        }

        // no errors
        var corrAddress = getAddressFromJSON(response);
        return corrAddress;
      }

      private Financial.Amount? doEstimateShippingCost(ShippoSession session, IShippingContext context, Shipment shipment, Guid logID)
      {
        var cred = (ShippoCredentials)session.User.Credentials;
        var sbody = getShipmentBody(shipment);

        // get shipping request
        var request = new WebClient.RequestParams
        {
          Caller = this,
          Uri = new Uri(URI_API_BASE + URI_SHIPMENTS),
          Method = HTTPRequestMethod.POST,
          ContentType = ContentType.JSON,
          Headers = new Dictionary<string, string>
            {
              { HDR_AUTHORIZATION, HDR_AUTHORIZATION_TOKEN.Args(cred.PrivateToken) }
            },
          Body = sbody.ToJSON(JSONWritingOptions.Compact)
        };

        var response = WebClient.GetJson(request);

        Log(MessageType.Info, "doEstimateShippingCost()", response.ToJSON(), relatedMessageID: logID);

        checkResponse(response);

        var rates = response["rates_list"] as JSONDataArray;
        if (rates == null) return null;

        foreach (JSONDataMap rate in rates)
        {
          var carrierID = rate["carrier_account"].AsString();
          if (carrierID.IsNotNullOrWhiteSpace() &&
              string.Equals(carrierID, shipment.Carrier.OuterID, StringComparison.InvariantCultureIgnoreCase) &&
              string.Equals(rate["servicelevel_token"].AsString(), shipment.Method.OuterID, StringComparison.InvariantCultureIgnoreCase))
            return new Financial.Amount(rate["currency_local"].AsString(), rate["amount_local"].AsDecimal());

            // todo: where is Template?! minimize cost among all mathced rates?
        }

        return null;
      }

      private JSONDataMap doGetRate(ShippoSession session, string rateID, Guid logID)
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

          var response = WebClient.GetJson(request);

          checkResponse(response);

          return response;
        }
        catch (Exception ex)
        {
          var error = ShippingException.ComposeError(ex.Message, ex);
          Log(MessageType.Error, "getRate()", StringConsts.SHIPPO_CREATE_LABEL_ERROR, error, relatedMessageID: logID);
          return null;
        }
      }


      private Address getAddressFromJSON(JSONDataMap map)
      {
        if (map==null) return null;

        var result = new Address();
        result.Name    = map["name"].AsString();
        result.Line1   = map["street1"].AsString();
        result.Line2   = map["street2"].AsString();
        result.City    = map["city"].AsString();
        result.Region  = map["state"].AsString();
        result.Postal  = AddressComparator.GetPostalMainPart(map["zip"].AsString());
        result.Country = NFX.Standards.Countries_ISO3166_1.Normalize3(map["country"].AsString());
        result.Phone   = map["phone"].AsString();
        result.EMail   = map["email"].AsString();

        return result;
      }

      private JSONDataMap getCreateLabelRequestBody(ShippoSession session, Shipment shipment)
      {
        var isReturn = shipment.LabelIDForReturn.IsNotNullOrWhiteSpace();

        var body = new JSONDataMap();
        body["carrier_account"] = shipment.Carrier.OuterID;
        body["servicelevel_token"] = shipment.Method.OuterID;
        body["label_file_type"] = FORMATS[shipment.LabelFormat];
        body["async"] = false;

        var shpm = new JSONDataMap();
        shpm["object_purpose"] = PURCHASE_PURPOSE;
        shpm["parcel"] = getParcelBody(shipment);
        shpm["address_from"] = getAddressBody(shipment.FromAddress);
        shpm["address_to"] = getAddressBody(shipment.ToAddress);
        if (!isReturn && (shipment.ReturnAddress != null))
          shpm["address_return"] = getAddressBody(shipment.ReturnAddress);

        if (isReturn) shpm["return_of"] = shipment.LabelIDForReturn;

        body["shipment"] = shpm;

        return body;
      }

      private JSONDataMap getParcelBody(Shipment shipment)
      {
        var parcel = new JSONDataMap();
        if (shipment.Template != null)
          parcel["template"] = shipment.Template.OuterID;

        parcel["distance_unit"] = DIST_UNITS[shipment.DistanceUnit];
        parcel["length"] = shipment.Length.ToString(CultureInfo.InvariantCulture);
        parcel["width"] = shipment.Width.ToString(CultureInfo.InvariantCulture);
        parcel["height"] = shipment.Height.ToString(CultureInfo.InvariantCulture);
        parcel["mass_unit"] = MASS_UNITS[shipment.MassUnit];
        parcel["weight"] = shipment.Weight.ToString(CultureInfo.InvariantCulture);

        return parcel;
      }

      private JSONDataMap getAddressBody(Address addr)
      {
        var result = new JSONDataMap();

        result["object_purpose"] = PURCHASE_PURPOSE;
        result["name"] = addr.Name;
        result["country"] = NFX.Standards.Countries_ISO3166_1.Normalize2(addr.Country);
        result["street1"] = addr.Line1;
        result["street2"] = addr.Line2;
        result["city"] = addr.City;
        result["state"] = addr.Region;
        result["zip"] = addr.Postal;
        result["phone"] = addr.Phone;
        result["email"] = addr.EMail;

        return result;
      }

      private JSONDataMap getShipmentBody(Shipment shipment)
      {
        var isReturn = shipment.LabelIDForReturn.IsNotNullOrWhiteSpace();

        var result = new JSONDataMap();
        result["object_purpose"] = PURCHASE_PURPOSE;
        result["parcel"] = getParcelBody(shipment);
        result["address_from"] = getAddressBody(shipment.FromAddress);
        result["address_to"] = getAddressBody(shipment.ToAddress);
        if (!isReturn && (shipment.ReturnAddress != null))
          result["address_return"] = getAddressBody(shipment.ReturnAddress);
        if (isReturn) result["return_of"] = shipment.LabelIDForReturn;
        result["async"] = false;

        return result;
      }


      private void checkResponse(JSONDataMap response)
      {
        if (response == null) throw new ShippingException("checkResponse(response=null)");

        if (!response["object_state"].AsString().EqualsIgnoreCase(STATUS_VALID) ||
            !response["object_status"].AsString(STATUS_SUCCESS).EqualsIgnoreCase(STATUS_SUCCESS))
        {
          string text = null;
          var messages = response["messages"] as JSONDataArray;
          if (messages != null && messages.Any())
          {
            var message = (JSONDataMap)messages.First();
            text = message["text"].AsString(string.Empty);
          }

          if (text.IsNullOrWhiteSpace()) text = StringConsts.SHIPPO_OPERATION_FAILED;

          throw new ShippingException(text);
        }
      }

    #endregion
  }
}
