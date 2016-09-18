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
  public enum LabelFormat
  {
    PDF = 0,
    PDF_4X6 = 1,
    PNG = 2,
    ZPLII = 3
  }

  public enum Carrier
  {
    Unknown = 0,
    USPS = 1
  }

  public struct Label
  {
    public Label(object id,
                 string url,
                 byte[] data,
                 LabelFormat format,
                 string trackingNumber,
                 Carrier carrier,
                 NFX.Financial.Amount rate) : this()
    {
      ID = id;
      CreateDate = App.TimeSource.UTCNow;
      URL = url;
      Data = data;
      Format = format;
      TrackingNumber = trackingNumber;
      Carrier = carrier;
      Rate = rate;
    }

    public object ID { get; private set; } // system-inner label ID
    public DateTime CreateDate { get; private set; }

    public string URL { get; private set; }
    public byte[] Data { get; private set; }
    public LabelFormat Format { get; private set; }

    public string TrackingNumber { get; private set; }
    public Carrier Carrier { get; private set; }
    public NFX.Financial.Amount Rate { get; private set; }
  }
}
