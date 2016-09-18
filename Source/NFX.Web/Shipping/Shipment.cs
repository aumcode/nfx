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

namespace NFX.Web.Shipping
{
  public enum MassUnit { Lb = 0, Kg }
  public enum DistanceUnit { In = 0, Cm }
  public enum ParcelTemplate { USPS_FlatRateEnvelope = 0 }

  public class Shipment
  {
    public ParcelTemplate? Template { get; set; }

    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public DistanceUnit DistanceUnit { get; set; }

    public decimal Weight { get; set; }
    public MassUnit MassUnit { get; set; }

    public string CarrierName { get; set; } // system-inner carrier name
    public object CarrierID { get; set; }   // system-inner carrier ID
    public object ServiceID { get; set; }   // system-inner carrier service ID
    public LabelFormat LabelFormat { get; set; }

    public Address FromAddress { get; set; }
    public Address ToAddress { get; set; }
    public Address? ReturnAddress { get; set; }
  }
}
