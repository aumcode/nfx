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
using System.Globalization;
using System.Linq;
using System.Text;

using NFX;
using NFX.Environment;

namespace NFX.Web.Shipping.Manual
{
  public class ManualSystem : ShippingSystem
  {
    #region CONSTS

      public const string MANUAL_REALM = "manual-system";

    #endregion

    #region .ctor

      public ManualSystem(string name, IConfigSectionNode node) : base(name, node)
      {
      }

      public ManualSystem(string name, IConfigSectionNode node, object director) : base(name, node, director)
      {
      }

    #endregion

    #region IShippingSystem impl

      public override IShippingSystemCapabilities Capabilities
      {
        get { return ManualCapabilities.Instance; }
      }

      protected override ShippingSession DoStartSession(ShippingConnectionParameters cParams = null)
      {
        cParams = cParams ?? DefaultSessionConnectParams;
        return new ManualSession(this, (ManualConnectionParameters)cParams);
      }

      protected override ShippingConnectionParameters MakeDefaultSessionConnectParams(IConfigSectionNode paramsSection)
      {
        return ShippingConnectionParameters.Make<ManualConnectionParameters>(paramsSection);
      }

      public override Label CreateLabel(ShippingSession session, IShippingContext context, Shipment shipment)
      {
        throw new ShippingException("Not supported");
      }

      public override TrackInfo TrackShipment(ShippingSession session, IShippingContext context, string carrierID, string trackingNumber)
      {
        return TrackShipment((ManualSession)session, context, carrierID, trackingNumber);
      }

      public TrackInfo TrackShipment(ManualSession session, IShippingContext context, string carrierID, string trackingNumber)
      {
        return new TrackInfo
              {
                TrackingURL = GetTrackingURL(session, context, carrierID, trackingNumber),
                TrackingNumber = trackingNumber,
                CarrierID = carrierID
              };
      }

      public override Address ValidateAddress(ShippingSession session, IShippingContext context, Address address, out ValidateShippingAddressException error)
      {
        throw new ShippingException("Not supported");
      }

      public override Financial.Amount? EstimateShippingCost(ShippingSession session, IShippingContext context, Shipment shipment)
      {
        throw new ShippingException("Not supported");
      }

    #endregion

    #region .pvt

    #endregion
  }
}
