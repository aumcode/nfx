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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using System.Threading;
using System.Threading.Tasks;

using NFX.Collections;
using System.Collections;

namespace NFX.NUnit.Collections
{
  
  [TestFixture]
  public class StringMapTests
  {
    [TestCase]
    public void CaseSensitive()
    {
      var m = new StringMap(true);
      m["a"] = "Albert";
      m["A"] = "Albert Capital";

      Assert.AreEqual(2, m.Count);
      Assert.AreEqual("Albert", m["a"]);
      Assert.AreEqual("Albert Capital", m["A"]);
    }

    [TestCase]
    public void CaseSensitive_dfltCtor()
    {
      var m = new StringMap();
      m["a"] = "Albert";
      m["A"] = "Albert Capital";

      Assert.AreEqual(2, m.Count);
      Assert.AreEqual("Albert", m["a"]);
      Assert.AreEqual("Albert Capital", m["A"]);
    }

    [TestCase]
    public void CaseInsensitive()
    {
      var m = new StringMap(false);
      m["a"] = "Albert";
      m["A"] = "Albert Capital";

      Assert.AreEqual(1, m.Count);
      Assert.AreEqual("Albert Capital", m["a"]);
      Assert.AreEqual("Albert Capital", m["A"]);
    }

    [TestCase]
    public void KeyExistence()
    {
      var m = new StringMap();
      m["a"] = "Albert";
      m["b"] = "Benedict";

      Assert.AreEqual(2, m.Count);
      Assert.AreEqual("Albert", m["a"]);
      Assert.AreEqual("Benedict", m["b"]);
      Assert.IsNull(  m["c"] );
      Assert.IsTrue( m.ContainsKey("a"));
      Assert.IsFalse( m.ContainsKey("c"));
    }


  }
}
