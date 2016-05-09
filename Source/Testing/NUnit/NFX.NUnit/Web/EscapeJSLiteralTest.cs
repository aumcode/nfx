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
  public class EscapeJSLiteralTest
  {
    [Test]
    public void Empty()
    {
      Assert.AreEqual(null, Utils.EscapeJSLiteral(null));
      Assert.AreEqual("", Utils.EscapeJSLiteral(""));
      Assert.AreEqual("   ", Utils.EscapeJSLiteral("   "));
    }

    [Test]
    public void Quotes()
    {
      Assert.AreEqual(@"Mc\x27Cloud", Utils.EscapeJSLiteral("Mc'Cloud"));
      Assert.AreEqual(@"Mc\x22Cloud", Utils.EscapeJSLiteral("Mc\"Cloud"));
      Assert.AreEqual(@"Mc\x22\x27Cloud", Utils.EscapeJSLiteral("Mc\"'Cloud"));
      
    }

    [Test]
    public void Script()
    {
      Assert.AreEqual(@"not \x3C\x2Fscript\x3E the end", Utils.EscapeJSLiteral("not </script> the end"));
    }

    [Test]
    public void RN()
    {
      Assert.AreEqual(@"not \x0D \x0A the end", Utils.EscapeJSLiteral("not \r \n the end"));
    }

    [Test]
    public void Various()
    {
      Assert.AreEqual(@"not\x27s \x22\x26amp;\x22 the\x5Cs\x2F end", Utils.EscapeJSLiteral(@"not's ""&amp;"" the\s/ end"));
    }

  }
}
