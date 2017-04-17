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

using NFX.Environment;
using NFX.Instrumentation;

namespace NFX.Web.Shipping
{
  /// <summary>
  /// Represents a process-global host for a shipping systems
  /// </summary>
  public interface IShippingSystemHost : INamed
  {
  }

  /// <summary>
  /// Denotes an implementation of IShippingSystemHost
  /// </summary>
  public interface IShippingSystemHostImplementation : IShippingSystemHost, IConfigurable
  {
  }

  /// <summary>
  /// Denotes context for shipping transactions. Can be used to provide additional information
  /// </summary>
  public interface IShippingContext { }

  /// <summary>
  /// Denotes capabilities of the shipping system
  /// </summary>
  public interface IShippingSystemCapabilities
  {
    /// <summary>
    /// Indicates whether a shipping system supports shipping label creation
    /// </summary>
    bool SupportsLabelCreation { get; }

    /// <summary>
    /// Indicates whethe a shipping system provides detailed tracking information about shipments
    /// </summary>
    bool SupportsShipmentTracking { get; }

    /// <summary>
    /// Indicates whether a shipping system supports shipping address validation
    /// </summary>
    bool SupportsAddressValidation { get; }

    /// <summary>
    /// Indicates whether a shipping system provides any services directly from some shipping carriers
    /// </summary>
    bool SupportsCarrierServices { get; }

    /// <summary>
    /// Indicates whether a shipping system provides any (at least approximate) shipping cost calculation
    /// </summary>
    bool SupportsShippingCostEstimation { get; }
  }

  /// <summary>
  /// Represents entity that can perform shipping fuctions like labels creation, tracking etc.
  /// </summary>
  public interface IShippingSystem
  {
    /// <summary>
    /// Returns capabilities for this shipping system
    /// </summary>
    IShippingSystemCapabilities Capabilities { get; }

    /// <summary>
    /// Starts shipping session with given or default connection parameters
    /// </summary>
    ShippingSession StartSession(ShippingConnectionParameters cParams = null);

    /// <summary>
    /// Creates shipping direct/return label
    /// </summary>
    Label CreateLabel(ShippingSession session, IShippingContext context, Shipment shipment);

    /// <summary>
    /// Retrieves shipment tracking info
    /// </summary>
    TrackInfo TrackShipment(ShippingSession session, IShippingContext context, string carrierID, string trackingNumber);

    /// <summary>
    /// Retrieves tracking URL by carrier and number
    /// </summary>
    string GetTrackingURL(ShippingSession session, IShippingContext context, string carrierID, string trackingNumber);

    /// <summary>
    /// Validates shipping address.
    /// Returns new Address instance which may contain corrected address fields ('New Yourk' -> 'New York')
    /// </summary>
    Address ValidateAddress(ShippingSession session, IShippingContext context, Address address, out ValidateShippingAddressException error);

    /// <summary>
    /// Returns all the carriers allowed for the system
    /// </summary>
    IEnumerable<ShippingCarrier> GetShippingCarriers(ShippingSession session, IShippingContext context);

    /// <summary>
    /// Estimates shipping label cost.
    /// </summary>
    /// <returns>Rate for original or alternative shipment</returns>
    ShippingRate EstimateShippingCost(ShippingSession session, IShippingContext context, Shipment shipment);

    // todo: RefundLabel
  }

  /// <summary>
  /// Denotes an implementation of IShippingSystem
  /// </summary>
  public interface IShippingSystemImplementation : IShippingSystem, IConfigurable, IInstrumentable
  {
    NFX.Log.MessageType LogLevel { get; set; }
  }

}
