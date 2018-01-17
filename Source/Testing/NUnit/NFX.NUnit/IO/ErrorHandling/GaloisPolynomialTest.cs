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
using NFX.IO.ErrorHandling;
using NUnit.Framework;

namespace NFX.NUnit.IO.ErrorHandling
{
  [TestFixture]
  public class GaloisPolynomialTest
  {
    [Test]
    public void Divide()
    {
      //System.Diagnostics.Debugger.Launch();

      GaloisField field = new GaloisField(285, 256, 0);

      GaloisPolynomial divident = new GaloisPolynomial(field, new int[] {32, 49, 205, 69, 42, 20, 0, 236, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0});

      GaloisPolynomial divider = new GaloisPolynomial(field, new int[] {1, 119, 66, 83, 120, 119, 22, 197, 83, 249, 41, 143, 134, 85, 53, 125, 99, 79});

      GaloisPolynomial quotient, remainder;
      divident.Divide(divider, out quotient, out remainder);

      int[] expectedQuotientCoefficients = new int[] { 32, 119, 212, 254, 109, 212, 30, 95, 117};
      int[] expectedRemainderCoefficients = new int[] { 3, 130, 179, 194, 0, 55, 211, 110, 79, 98, 72, 170, 96, 211, 137, 213};

      Assert.AreEqual(expectedQuotientCoefficients.Length, quotient.Coefficients.Length);
      Assert.AreEqual(expectedRemainderCoefficients.Length, remainder.Coefficients.Length);

      for (int i = 0; i < quotient.Coefficients.Length; i++)
        Assert.AreEqual(expectedQuotientCoefficients[i], quotient.Coefficients[i]);

      for (int i = 0; i < remainder.Coefficients.Length; i++)
        Assert.AreEqual(expectedRemainderCoefficients[i], remainder.Coefficients[i]);
    }
  }
}
