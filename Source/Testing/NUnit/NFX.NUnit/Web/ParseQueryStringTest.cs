/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
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
using NUnit.Framework;
using System;

using NFX.Web;

namespace NFX.NUnit.Web
{
  [TestFixture]
  public class ParseQueryStringTest
  {
    [Test]
    public void Empty()
    {
      Assert.AreEqual(0, Utils.ParseQueryString(null).Count);
      Assert.AreEqual(0, Utils.ParseQueryString(string.Empty).Count);
      Assert.AreEqual(0, Utils.ParseQueryString(" ").Count);
      Assert.AreEqual(0, Utils.ParseQueryString("\r \n").Count);
      Assert.AreEqual(0, Utils.ParseQueryString("\t \t ").Count);
    }

    [Test]
    public void WOAmKey()
    {
      var dict = Utils.ParseQueryString("a");

      Assert.AreEqual(1, dict.Count);
      Assert.AreEqual(null, dict["a"]);
    }

    [Test]
    public void WOAmpKeyVal()
    {
      var dict = Utils.ParseQueryString("a=1");

      Assert.AreEqual(1, dict.Count);
      Assert.AreEqual("1", dict["a"]);
    }

    [Test]
    public void WOAmpVal()
    {
      var dict = Utils.ParseQueryString("=1");

      Assert.AreEqual(0, dict.Count);
    }

    [Test]
    public void DoubleEq()
    {
      var dict = Utils.ParseQueryString("a==1");

      Assert.AreEqual(1, dict.Count);
      Assert.AreEqual("=1", dict["a"]);
    }

    [Test]
    public void KeyVal()
    {
      Action<string> check = _ =>
      {
        var dict = Utils.ParseQueryString(_);

        Assert.AreEqual(2, dict.Count);
        Assert.AreEqual("1", dict["a"]);
        Assert.AreEqual("rrt", dict["b"]);
      };

      check("a=1&b=rrt");
      check("a=1&b=rrt&");
      check("&a=1&b=rrt&");
      check("&&a=1&&b=rrt&&&");
    }

    [Test]
    public void KeyEmptyEqNormal()
    {
      var dict = Utils.ParseQueryString("a=&b&&=&=14&c=3459");

      Assert.AreEqual(3, dict.Count);
      Assert.AreEqual(null, dict["a"]);
      Assert.AreEqual(null, dict["b"]);
      Assert.AreEqual("3459", dict["c"]);
    }

    [Test]
    public void Esc()
    {
      string[] strs = { " ", "!", "=", "&", "\"zele/m\\h()an\"" };

      foreach (var str in strs)
      {
        var query = "a=" + Uri.EscapeDataString(str);

        var dict = Utils.ParseQueryString(query);

        Assert.AreEqual(1, dict.Count);
        Assert.AreEqual(str, dict["a"]);
      }
    }
  }
}
