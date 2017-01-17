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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NFX.Standards;
using NFX.Serialization.JSON;

namespace NFX.NUnit.Standards
{
  [TestFixture]
  class WeightTest
  {
    [TestCase]
    public void Convert()
    {
      Weight d1 = new Weight(3.12m, Weight.UnitType.Kg);
      Assert.AreEqual(d1.Convert(Weight.UnitType.G).Value, 3120);
    }

    [TestCase]
    public void Parse()
    {
      Assert.AreEqual(Weight.Parse("15.8 Kg").Unit, Weight.UnitType.Kg);
      Assert.AreEqual(Weight.Parse("15.8 KG").Value, 15.8m);
      Assert.AreEqual(Weight.Parse("  15.8     kg   ").Unit, Weight.UnitType.Kg);
      Assert.AreEqual(Weight.Parse("  15.8     Kg   ").Value, 15.8m);
      Assert.AreEqual(Weight.Parse("15.8 G").Unit, Weight.UnitType.G);
      Assert.AreEqual(Weight.Parse("15.8 G").Value, 15.8m);
    }

    [TestCase]
    [ExpectedException()]
    public void ParseFail()
    {
      Assert.AreEqual(Weight.Parse("a 15.8 kg").Value, 15.8);
    }

    [TestCase]
    [ExpectedException(typeof(NFXException))]
    public void ParseIncorrect()
    {
      Assert.AreEqual(Weight.Parse("15.8 mdm").Value, 15.8);
    }

    [TestCase]
    [ExpectedException(typeof(NFXException))]
    public void ParseEmpty()
    {
      Assert.AreEqual(Weight.Parse("").Value, 15.8);
    }

    [TestCase]
    [ExpectedException(typeof(NFXException))]
    public void ParseNull()
    {
      Assert.AreEqual(Weight.Parse(null).Value, 15.8);
    }

    [TestCase]
    public void TryParse()
    {
      Weight? result;
      Assert.AreEqual(Weight.TryParse("15.8 Oz", out result), true);
      Assert.AreEqual(Weight.TryParse("not a 16.8 kg", out result), false);

      Weight.TryParse(" 15   Lb ", out result);
      Assert.AreEqual(result.Value.Value, 15);
    }

    [TestCase]
    public void TestEquals()
    {
      Weight d1 = new Weight(33, Weight.UnitType.Kg);
      Weight d2 = new Weight(33000, Weight.UnitType.G);
      Assert.IsTrue(d1.Equals(d2));
    }

    [TestCase]
    public void TestNotEquals()
    {
      Weight d1 = new Weight(3.25m, Weight.UnitType.G);
      Weight w1 = new Weight(15, Weight.UnitType.Kg);
      Assert.IsFalse(d1.Equals(w1));
    }

    [TestCase]
    public void TestHashCode()
    {
      Weight d1 = new Weight(309, Weight.UnitType.G);
      Assert.AreEqual(d1.GetHashCode(), d1.Value.GetHashCode());
    }

    [TestCase]
    public void TestToString()
    {
      Weight d1 = new Weight(3.25m, Weight.UnitType.Lb);
      Assert.AreEqual(d1.ToString(), "3.25 lb");
    }


    [TestCase]
    public void CompareTo()
    {
      Weight d1 = new Weight(35, Weight.UnitType.Kg);
      Weight d2 = new Weight(3300, Weight.UnitType.G);
      Assert.AreEqual(d1.CompareTo(d2), 1);
    }

    [TestCase]
    public void JSON()
    {
      var data = new { dist = new Weight(3.25m, Weight.UnitType.Kg) };
      var json = data.ToJSON();
      Console.WriteLine(json);
      Assert.AreEqual(@"{""dist"":{""unit"":""kg"",""value"":3.25}}", json);
    }

    [TestCase]
    public void Operators()
    {
      Weight d1 = new Weight(35, Weight.UnitType.Kg);
      Weight d2 = new Weight(1200, Weight.UnitType.G);
      Weight d3 = d1 + d2;
      Assert.AreEqual(d3.ToString(), "36.2 kg");
      d3 = d1 - d2;
      Assert.AreEqual(d3.ToString(), "33.8 kg");
      d3 = d1 * 2;
      Assert.AreEqual(d3.ToString(), "70 kg");
      d3 = d1 / 2;
      Assert.AreEqual(d3.ToString(), "17.5 kg");
      Assert.IsTrue(d1 == new Weight(35000, Weight.UnitType.G));
      Assert.IsTrue(d1 != d2);
      Assert.IsTrue(d1 >= d2);
      Assert.IsTrue(d1 > d2);
    }

  }
}
