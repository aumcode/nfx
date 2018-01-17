/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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

using NUnit.Framework;

using NFX.Web.Shipping;

namespace NFX.NUnit.Web.Shipping
{
  [TestFixture]
  public class AddressComparatorTest
  {
    [Test]
    public void Validate_Line1()
    {
      var address = getDefaultAddress();

      var other1 = getDefaultAddress(line1: "587 Kiva Str.");
      var other2 = getDefaultAddress(line1: "587 Kiva St.");
      var other3 = getDefaultAddress(line1: "587 Kiva St");

      Assert.IsTrue(AddressComparator.AreSimilar(address, other1));
      Assert.IsTrue(AddressComparator.AreSimilar(address, other2));
      Assert.IsTrue(AddressComparator.AreSimilar(address, other3));
      Assert.IsTrue(AddressComparator.AreSimilar(other1, other2));
      Assert.IsTrue(AddressComparator.AreSimilar(other1, other3));
      Assert.IsTrue(AddressComparator.AreSimilar(other2, other3));
    }
    [Test]
    public void Validate_Line2()
    {
      var address = getDefaultAddress(line2: "Apt 23");

      var other1 = getDefaultAddress(line2: "apartMent 23");
      var other2 = getDefaultAddress(line2: "ap.23");
      var other3 = getDefaultAddress(line2: "#23   ");

      Assert.IsTrue(AddressComparator.AreSimilar(address, other1));
      Assert.IsTrue(AddressComparator.AreSimilar(address, other2));
      Assert.IsTrue(AddressComparator.AreSimilar(address, other3));
      Assert.IsTrue(AddressComparator.AreSimilar(other1, other2));
      Assert.IsTrue(AddressComparator.AreSimilar(other1, other3));
      Assert.IsTrue(AddressComparator.AreSimilar(other2, other3));
    }

    [Test]
    public void Validate_Country()
    {
      var address = getDefaultAddress();
      var other = getDefaultAddress(country: "US");

      Assert.IsTrue(AddressComparator.AreSimilar(address, other));
    }



    private Address getDefaultAddress(string country = null, string line1 = null, string line2 = null, string zip = null)
    {
      country = country ?? "USA";
      line1 = line1 ?? "587 Kiva Street";
      line2 = line2 ?? string.Empty;
      zip = zip ?? "87544";

      var result = new Address
      {
        PersonName = "Stan Ulam",
        Country = country,
        Region = "NM",
        City = "Los Alamos",
        EMail = "s-ulam@myime.com",
        Line1 = line1,
        Line2 = line2,
        Postal = zip,
        Phone = "(333) 777-77-77"
      };

      return result;
    }
  }
}
