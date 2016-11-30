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
using System.Threading.Tasks;

namespace NFX.Web.Shipping
{
  public enum PriceCategory
  {
    Saver = 0,
    Standard = 1,
    Expedited = 2
  }

  public enum CarrierType
  {
    Unknown = 0,
    USPS = 1,
    FedEx = 2,
    UPS = 3,
    DHLExpress = 4,
    Other = 10000
  }

  public enum LabelFormat
  {
    PDF = 0,
    PDF_4X6 = 1,
    PNG = 2,
    ZPLII = 3
  }

  public enum MassUnit { G = 0, Oz = 1, Lb = 2, Kg = 3 }
  public enum DistanceUnit { Cm = 0, In = 1, Ft = 2, Mm = 3, M = 4, Yd = 5 }


}
