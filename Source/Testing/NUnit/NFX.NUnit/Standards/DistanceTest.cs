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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NFX.Standards;
using NFX.Serialization.JSON;

namespace NFX.NUnit.Standards
{
  [TestFixture]
  class DistanceTest
  {
    [TestCase]
    public void Convert()
    {
      Distance d1 = new Distance(3.12m, Distance.UnitType.M);
      Assert.AreEqual(d1.Convert(Distance.UnitType.Cm).Value, 312);
    }

    [TestCase]
    public void Parse()
    {
      Assert.AreEqual(Distance.Parse("15.8 Cm").Unit, Distance.UnitType.Cm);
      Assert.AreEqual(Distance.Parse("15.8 Cm").Value, 15.8m);
      Assert.AreEqual(Distance.Parse("  15.8     Cm   ").Unit, Distance.UnitType.Cm);
      Assert.AreEqual(Distance.Parse("  15.8     Cm   ").Value, 15.8m);
      Assert.AreEqual(Distance.Parse("15.8 MM").Unit, Distance.UnitType.Mm);
      Assert.AreEqual(Distance.Parse("15.8 mM").Value, 15.8m);
    }

    [TestCase]
    [ExpectedException()]
    public void ParseFail()
    {
      Assert.AreEqual(Distance.Parse("a 15.8 cm").Value, 15.8m);
    }

    [TestCase]
    [ExpectedException(typeof(NFXException))]
    public void ParseIncorrect()
    {
      Assert.AreEqual(Distance.Parse("15.8 mdm").Value, 15.8m);
    }

    [TestCase]
    [ExpectedException(typeof(NFXException))]
    public void ParseEmpty()
    {
      Assert.AreEqual(Distance.Parse("").Value, 15.8m);
    }

    [TestCase]
    [ExpectedException(typeof(NFXException))]
    public void ParseNull()
    {
      Assert.AreEqual(Distance.Parse(null).Value, 15.8m);
    }

    [TestCase]
    public void TryParse()
    {
      Distance? result;
      Assert.AreEqual(Distance.TryParse("15.8 Cm", out result), true);
      Assert.AreEqual(Distance.TryParse("not a 16.8 kg", out result), false);

      Distance.TryParse(" 15.8   Cm ", out result);
      Assert.AreEqual(result.Value.Value, 15.8m);
    }

    [TestCase]
    public void TestEquals()
    {
      Distance d1 = new Distance(33, Distance.UnitType.M);
      Distance d2 = new Distance(3300, Distance.UnitType.Cm);
      Assert.IsTrue(d1.Equals(d2));
    }

    [TestCase]
    public void TestNotEquals()
    {
      Distance d1 = new Distance(3.25m, Distance.UnitType.Mm);
      Weight w1 = new Weight(15, Weight.UnitType.Kg);
      Assert.IsFalse(d1.Equals(w1));
    }

    [TestCase]
    public void TestHashCode()
    {
      Distance d1 = new Distance(3, Distance.UnitType.Mm);
      Assert.AreEqual(d1.GetHashCode(), d1.Value.GetHashCode());
    }

    [TestCase]
    public void TestToString()
    {
      Distance d1 = new Distance(3.25m, Distance.UnitType.Mm);
      Assert.AreEqual(d1.ToString(), "3.25 mm");
    }


    [TestCase]
    public void CompareTo()
    {
      Distance d1 = new Distance(35, Distance.UnitType.M);
      Distance d2 = new Distance(3300, Distance.UnitType.Cm);
      Assert.AreEqual(d1.CompareTo(d2), 1);
    }

    [TestCase]
    public void JSON()
    {
      var data = new { dist = new Distance(3.25m, Distance.UnitType.Mm) };
      var json = data.ToJSON();
      Console.WriteLine(json);
      Assert.AreEqual(@"{""dist"":{""unit"":""mm"",""value"":3.25}}", json);
    }

    [TestCase]
    public void Operators()
    {
      Distance d1 = new Distance(35, Distance.UnitType.M);
      Distance d2 = new Distance(1200, Distance.UnitType.Mm);
      Distance d3 = d1 + d2;
      Assert.AreEqual(d3.ToString(), "36.2 m");
      d3 = d1 - d2;
      Assert.AreEqual(d3.ToString(), "33.8 m");
      d3 = d1 * 2;
      Assert.AreEqual(d3.ToString(), "70 m");
      d3 = d1 / 2;
      Assert.AreEqual(d3.ToString(), "17.5 m");
      Assert.IsTrue(d1 == new Distance(35000, Distance.UnitType.Mm));
      Assert.IsTrue(d1 != d2);
      Assert.IsTrue(d1 >= d2);
      Assert.IsTrue(d1 > d2);
    }

  }
}
