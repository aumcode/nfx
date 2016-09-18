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
  public enum TrackStatus
  {
    Unknown = 0,
    InProcess = 1,
    Delivered = 2,
    Transit = 3,
    Failure = 4,
    Returned = 5,
    Cancelled = 6,
    Error = 7
  }

  public struct TrackInfo
  {
    public TrackInfo(string trackingNumber,
                     Carrier carrier,
                     TrackStatus status,
                     string details,
                     DateTime localTime,
                     Address? location) : this()
    {
      TrackingNumber = trackingNumber;
      Carrier = carrier;
      Status = status;
      Details = details;
      LocalTime = localTime;
      Location = location;
    }

    public string TrackingNumber { get; private set; }
    public Carrier Carrier { get; private set; }
    public TrackStatus Status { get; private set; }
    public string Details { get; private set; }
    public DateTime LocalTime { get; private set; }
    public Address? Location { get; private set; }
  }
}
