
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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 1.0  2013.12.10
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using NFX;
using NFX.Environment;

namespace NFX.IO.ErrorHandling
{
  public class ReedSolomonEncoder
  {
    #region .ctor

    public ReedSolomonEncoder(GaloisField field)
    {
      m_Field = field;
      m_PolySet.Add( new GaloisPolynomial(field, new int[] { 1 }) );
    }

    #endregion

    #region Pvt/Prot/Int Fields

      private GaloisField m_Field;
      private List<GaloisPolynomial> m_PolySet = new List<GaloisPolynomial>();

    #endregion

    #region Public

      public void Encode(int[] src, int errorCorrectionLength)
      {
        if (errorCorrectionLength <= 0)
          throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".Encode(errorCorrectionLength)");

         int dataLength = src.Length - errorCorrectionLength;
         if (dataLength <= 0)
           throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".Encode: No data bytes provided");

         GaloisPolynomial generationPoly = getPolySet(errorCorrectionLength);
         int[] infoCoefficients = new int[dataLength];
         Array.Copy(src, 0, infoCoefficients, 0, dataLength);

         GaloisPolynomial infoPoly = new GaloisPolynomial(m_Field, infoCoefficients);
         infoPoly = infoPoly.Multiply(errorCorrectionLength, 1);

         GaloisPolynomial remainder, quotient;
         infoPoly.Divide(generationPoly, out quotient, out remainder);
         int[] coefficients = remainder.Coefficients;
         int numZeroCoefficients = errorCorrectionLength - coefficients.Length;
         for (var i = 0; i < numZeroCoefficients; i++)
            src[dataLength + i] = 0;

         Array.Copy(coefficients, 0, src, dataLength + numZeroCoefficients, coefficients.Length);
      }

    #endregion

    #region .pvt. impl.

      private GaloisPolynomial getPolySet(int degree)
      {
        if (degree >= m_PolySet.Count)
        {
          GaloisPolynomial lastPoly = m_PolySet.Last();
          for (int i = m_PolySet.Count; i <= degree; i++)
          {
            GaloisPolynomial newPoly = lastPoly.Multiply( new GaloisPolynomial(m_Field, new int[] {1, m_Field.Exp(i - 1 + m_Field.GeneratorBase)}));
            m_PolySet.Add(newPoly);
            lastPoly = newPoly;
          }
        }
        return m_PolySet[degree];
      }

    #endregion

  }//class

}
