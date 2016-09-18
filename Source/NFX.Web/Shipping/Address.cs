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
  public struct Address
  {
    public string Name { get; set; }
    public string Company { get; set; }
    public string Street1 { get; set; }
    public string Street2 { get; set; }
    public string Country { get; set; }
    public string State { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string Phone { get; set; }
    public string EMail { get; set; }

    public bool IsResidential { get; set; }
    public bool Validate { get; set; }

    public override string ToString()
    {
      return "[name: {0}, country: {1}, state: {2}, city: {3}, street1: {4}, zip: {5}]"
              .Args(Name ?? "-",
                    Country ?? "-",
                    State ?? "-",
                    City ?? "-",
                    Street1 ?? "-",
                    PostalCode ?? "-");
    }

    // SHIPPO

    // R  "object_purpose":"PURCHASE",     <-- only "PURCHASE" for labels
    // R  name
    //    company
    // R  street1
    //    street_no
    //    street2
    // R  city
    // R  zip
    // R  state
    // R  country
    // R  phone
    // R  email
    //    is_residential
    //    validate
    //    metadata


    // SHIPSTATION

    // "name": "The President",
    // "company": "US Govt",
    // "street1": "1600 Pennsylvania Ave",
    // "street2": "Oval Office",
    // "street3": null,
    // "city": "Washington",
    // "state": "DC",
    // "postalCode": "20500",
    // "country": "US",
    // "phone": null,
    // "residential": false


    // EASYPOST

    // street1
    // street2
    // city
    // state
    // zip
    // country
    // residential
    // carrier_facility
    // name
    // company
    // phone
    // email
    // federal_tax_id
    // state_tax_id
    // verifications
  }
}
