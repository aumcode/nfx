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
using System.Threading.Tasks;

using NFX.Security;

namespace NFX.Web.Shipping
{
  /// <summary>
  /// Represents session of ShippingSystem.
  /// All PaySystem operation requires session as mandatory parameter
  /// </summary>
  public abstract class ShippingSession : DisposableObject, INamed
  {
    #region .ctor

      protected ShippingSession(ShippingSystem shipSystem, ShippingConnectionParameters cParams)
      {
        if (shipSystem == null || cParams == null)
          throw new ShippingException(StringConsts.ARGUMENT_ERROR + this.GetType().Name + ".ctor(shipSystem != null and cParams != null)");

        m_ShippingSystem = shipSystem;
        m_Name = cParams.Name;
        m_User = cParams.User;

        lock (m_ShippingSystem.Sessions)
          m_ShippingSystem.Sessions.Add(this);
      }

      protected override void Destructor()
      {
        lock (m_ShippingSystem.Sessions)
          m_ShippingSystem.Sessions.Remove(this);

        base.Destructor();
      }

    #endregion

    #region Fields

      private readonly ShippingSystem m_ShippingSystem;
      private readonly string m_Name;
      private readonly User m_User;

    #endregion

    #region Properties

      public ShippingSystem ShippingSystem { get { return m_ShippingSystem; } }
      public string Name { get { return m_Name; } }
      public User User { get { return m_User; } }

      public bool IsValid { get { return m_User != null || m_User != User.Fake; } }

    #endregion

    /// <summary>
    /// Creates shipping label
    /// </summary>
    public Label CreateLabel(IShippingContext context, Shipment shipment)
    {
      if (!m_ShippingSystem.Capabilities.SupportsLabelCreation)
        throw new ShippingException(StringConsts.SHIPPING_SYSTEM_UNSUPPORTED_ACTION.Args("CreateLabel"));

      return m_ShippingSystem.CreateLabel(this, context, shipment);
    }

    /// <summary>
    /// Retrieves shipment tracking info
    /// </summary>
    public TrackInfo TrackShipment(IShippingContext context, string carrierID, string trackingNumber)
    {
      if (!m_ShippingSystem.Capabilities.SupportsShipmentTracking)
        throw new ShippingException(StringConsts.SHIPPING_SYSTEM_UNSUPPORTED_ACTION.Args("TrackShipment"));

      return m_ShippingSystem.TrackShipment(this, context, carrierID, trackingNumber);
    }

    /// <summary>
    /// Retrieves tracking URL by carrier and number
    /// </summary>
    public string GetTrackingURL(IShippingContext context, string carrierID, string trackingNumber)
    {
      if (!m_ShippingSystem.Capabilities.SupportsShipmentTracking)
        throw new ShippingException(StringConsts.SHIPPING_SYSTEM_UNSUPPORTED_ACTION.Args("GetTrackingURL"));

      return m_ShippingSystem.GetTrackingURL(this, context, carrierID, trackingNumber);
    }

    /// <summary>
    /// Validates shipping address.
    /// Returns new Address instance which may contain corrected address fields ('New Yourk' -> 'New York')
    /// </summary>
    public Address ValidateAddress(IShippingContext context, Address address, out ValidateShippingAddressException error)
    {
      if (!m_ShippingSystem.Capabilities.SupportsAddressValidation)
        throw new ShippingException(StringConsts.SHIPPING_SYSTEM_UNSUPPORTED_ACTION.Args("ValidateAddress"));

      return m_ShippingSystem.ValidateAddress(this, context, address, out error);
    }

    /// <summary>
    /// Returns all the carriers allowed for the system
    /// </summary>
    public IEnumerable<ShippingCarrier> GetShippingCarriers(IShippingContext context)
    {
      if (!m_ShippingSystem.Capabilities.SupportsCarrierServices)
        throw new ShippingException(StringConsts.SHIPPING_SYSTEM_UNSUPPORTED_ACTION.Args("GetShippingCarriers"));

      return m_ShippingSystem.GetShippingCarriers(this, context);
    }

    /// <summary>
    /// Estimates shipping label cost
    /// </summary>
    public ShippingRate EstimateShippingCost(IShippingContext context, Shipment shipment)
    {
      if (!m_ShippingSystem.Capabilities.SupportsShippingCostEstimation)
        throw new ShippingException(StringConsts.SHIPPING_SYSTEM_UNSUPPORTED_ACTION.Args("EstimateShippingCost"));

      return m_ShippingSystem.EstimateShippingCost(this, context, shipment);
    }

  }
}
