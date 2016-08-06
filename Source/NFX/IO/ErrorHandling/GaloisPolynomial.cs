
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
 * Revision: NFX 1.0  2013.12.12
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX.IO.ErrorHandling
{
  public class GaloisPolynomial
  {
    #region .ctor

      public GaloisPolynomial(GaloisField field, int[] coefficients)
      {
        if (field == null)
          throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".ctor(field=null)");

        if (coefficients == null || coefficients.Length == 0)
          throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".ctor(coeff=null|empty)");

        m_Field = field;

        if (coefficients.Length > 1 && coefficients[0] == 0)
        {
          int firstNonZero = 1;
          while (firstNonZero < coefficients.Length && coefficients[firstNonZero] == 0)
            firstNonZero++;

          if (firstNonZero == coefficients.Length)
          {
            m_Coefficients = m_Field.Polynomial0.Coefficients;
          }
          else
          {
            m_Coefficients = new int[coefficients.Length - firstNonZero];
            Array.Copy(coefficients, firstNonZero, m_Coefficients, 0, m_Coefficients.Length);
          }
        }
        else
        {
          m_Coefficients = coefficients;
        }
      }

    #endregion

    #region Pvt/Prot/Int Fields

      private readonly GaloisField m_Field;
      private readonly int[] m_Coefficients;

    #endregion

    #region Properties

      public int[] Coefficients
      {
          get { return m_Coefficients; }
      }

      public int Degree
      {
         get  { return m_Coefficients.Length - 1; }
      }

      public bool IsPolynomial0
      {
        get { return m_Coefficients[0] == 0; }
      }

    #endregion

    #region Public

      public GaloisPolynomial Add(GaloisPolynomial summand)
      {
        if (!m_Field.Equals(summand.m_Field))
          throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".Add(summand: different field)");

        if (IsPolynomial0)
          return summand;

        if (summand.IsPolynomial0)
          return this;

        int[] shorterPolynomialCoefficients;
        int[] longerPolynomialCoefficients;

        if (m_Coefficients.Length < summand.m_Coefficients.Length)
        {
          shorterPolynomialCoefficients = m_Coefficients;
          longerPolynomialCoefficients = summand.m_Coefficients;
        }
        else
        {
          shorterPolynomialCoefficients = summand.m_Coefficients;
          longerPolynomialCoefficients = m_Coefficients;
        }

        int[] resultCoefficients = new int[longerPolynomialCoefficients.Length];
        int deltaLength = (int)(longerPolynomialCoefficients.Length - shorterPolynomialCoefficients.Length);

        for (int i = 0; i < longerPolynomialCoefficients.Length; i++)
          if (i < deltaLength)
            resultCoefficients[i] = longerPolynomialCoefficients[i];
          else
            resultCoefficients[i] = GaloisField.Add( longerPolynomialCoefficients[i], shorterPolynomialCoefficients[i-deltaLength]);

        return new GaloisPolynomial(m_Field, resultCoefficients);
      }

      public GaloisPolynomial Multiply(GaloisPolynomial factorPolynomial)
      {
        if (!m_Field.Equals(factorPolynomial.m_Field))
          throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".Mult(factor: different field)");

        if (IsPolynomial0 || factorPolynomial.IsPolynomial0)
          return m_Field.Polynomial0;

        int[] multiplicand = m_Coefficients;
        int[] factor = factorPolynomial.m_Coefficients;
        int[] product = new int[m_Coefficients.Length + factorPolynomial.m_Coefficients.Length - 1];
        for (int i = 0; i < multiplicand.Length; i++)
        {
          int multiplicandCoefficient = multiplicand[i];
          for (int j = 0; j < factor.Length; j++)
          {
            product[i+j] = GaloisField.Add( product[i+j], m_Field.Multiply(multiplicandCoefficient, factor[j]));
          }
        }

        return new GaloisPolynomial( m_Field, product);
      }

      public GaloisPolynomial Multiply(int degree, int coefficient)
      {
        if (coefficient == 0)
          return m_Field.Polynomial0;

        int[] product = new int[m_Coefficients.Length + degree];
        for (int i = 0; i < m_Coefficients.Length; i++)
          product[i] = m_Field.Multiply(m_Coefficients[i], coefficient);

        return new GaloisPolynomial(m_Field, product);
      }

      public void Divide(GaloisPolynomial divider, out GaloisPolynomial quotient, out GaloisPolynomial remainder)
      {
        if (!m_Field.Equals(divider.m_Field))
          throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".Div(divider: different field)");

        if (divider.IsPolynomial0)
          throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".Div(divider: div zero is undefined)");

        quotient = m_Field.Polynomial0;
        remainder = this;

        int dividerLeadingCoefficient = divider.coefficientAt(divider.Degree);//divider.Coefficients.Last();
        int inversedDividerLeadingCoefficient = m_Field.Inverse(dividerLeadingCoefficient);

        while (remainder.Degree >= divider.Degree && !remainder.IsPolynomial0)
        {
          int deltaDegree = remainder.Degree - divider.Degree;
          int scale = m_Field.Multiply( remainder.coefficientAt(remainder.Degree), inversedDividerLeadingCoefficient);
          GaloisPolynomial term = divider.Multiply( deltaDegree, scale);
          GaloisPolynomial iterationQuotient = m_Field.GenerateMonomial(deltaDegree, scale);
          quotient = quotient.Add(iterationQuotient);
          remainder = remainder.Add(term);
        }
      }

    #endregion

    #region .pvt. impl.

      private int coefficientAt(int degree)
      {
        return m_Coefficients[m_Coefficients.Length - degree - 1];
      }

    #endregion
  }
}
