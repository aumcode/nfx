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
  /// Represents entity that can perform shipping fuctions like labels creation, tracking etc.
  /// </summary>
  public interface IShippingSystem
  {
    /// <summary>
    /// Starts shipping session with given or default connection parameters
    /// </summary>
    ShippingSession StartSession(ShippingConnectionParameters cParams = null);

    /// <summary>
    /// Creates shipping label
    /// </summary>
    Label CreateLabel(ShippingSession session, IShippingContext context, Shipment shipment);

    /// <summary>
    /// Creates return label for existed shipping label
    /// </summary>
    Label CreateReturnLabel(ShippingSession session, IShippingContext context, Shipment shipment, object labelID);

    /// <summary>
    /// Retrieves shipment tracking info
    /// </summary>
    TrackInfo TrackShipment(ShippingSession session, IShippingContext context, string trackingNumber);

    // CalculateRates
    // TrackShipment
    // RefundLabel
    // ValidateAddress
  }

  /// <summary>
  /// Denotes an implementation of IShippingSystem
  /// </summary>
  public interface IShippingSystemImplementation : IShippingSystem, IConfigurable, IInstrumentable
  {
    NFX.Log.MessageType LogLevel { get; set; }
  }

}
