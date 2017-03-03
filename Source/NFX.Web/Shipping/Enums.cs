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

namespace NFX.Web.Shipping
{
  /// <summary>
  /// Denotes type of shipping packages i.e. Envelope, Box etc.
  /// </summary>
  public enum PackageType
  {
    Unknown   = 0,
    Card      = 10,
    Envelope  = 20,
    Package   = 30, // thick envelope
    Box       = 40,
    Box_Small = 41,
    Box_Med   = 42,
    Box_Large = 43,
    Pak       = 50,
    Tube      = 60,

    Crate  = 70,
    Drum   = 80,
    Pallet = 90
  }

  /// <summary>
  /// Denotes price category: cheap, expedited etc.
  /// </summary>
  public enum PriceCategory
  {
    Unknown = 0,
    Saver = 1,
    Standard = 2,
    Expedited = 3
  }

  /// <summary>
  /// Denotes shipping carrier i.e. USPS,FedEx etc.
  /// </summary>
  public enum CarrierType
  {
    Unknown = 0,
    USPS = 1,
    FedEx = 2,
    UPS = 3,
    DHLExpress = 4
  }

  /// <summary>
  /// Shipping label print format i.e. PDF, PNG etc.
  /// </summary>
  public enum LabelFormat
  {
    PDF = 0,
    PDF_4X6 = 1,
    PNG = 2,
    ZPLII = 3
  }

  /// <summary>
  /// Tracking status
  /// </summary>
  public enum TrackStatus
  {
    Unknown = 0,
    Transit = 1,
    Delivered = 2,
    Failure = 3,
    Returned = 4,
    Cancelled = 5,
    Error = 4
  }

}
