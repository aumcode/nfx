using System;
using System.Collections.Generic;

namespace NFX.Web.Shipping.Manual
{
  /// <summary>
  /// Denotes capabilities of the Manual shipping system
  /// </summary>
  public class ManualCapabilities : IShippingSystemCapabilities
  {
    #region Static

      private static ManualCapabilities s_Instance = new ManualCapabilities();

      public static ManualCapabilities Instance { get { return s_Instance; } }

    #endregion

    #region .ctor

      private ManualCapabilities() {}

    #endregion

    #region IShippingSystemCapabilities

      public bool SupportsAddressValidation      { get { return false; } }
      public bool SupportsCarrierServices        { get { return true;  } }
      public bool SupportsLabelCreation          { get { return false; } }
      public bool SupportsShipmentTracking       { get { return true;  } }
      public bool SupportsShippingCostEstimation { get { return false; } }

    #endregion

  }
}
