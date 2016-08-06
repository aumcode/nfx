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

using NFX;

using NFX.Serialization.JSON;

namespace NFX.Financial
{
  /// <summary>
  /// Represents monetary amount with currency
  /// </summary>
  [Serializable]
  public struct Amount : IEquatable<Amount>, IComparable<Amount>, IJSONWritable
  {
    #region Const

      private const decimal EMPTY_AMOUNT = 0;
      private const string EMPTY_CURRENCY_ISO = "";

    #endregion

    public Amount(string currencyISO, decimal value)
    {
      m_CurrencyISO = (currencyISO ?? string.Empty).Trim();
      if (m_CurrencyISO.Length != 3)
        throw new FinancialException(StringConsts.ARGUMENT_ERROR + typeof(Amount).FullName + ".ctor(currencyISO '{0}' length is not equal to 3)".Args(currencyISO));
      m_Value = value;
    }

    private string m_CurrencyISO;
    private decimal m_Value;

    public string CurrencyISO { get{ return m_CurrencyISO ?? EMPTY_CURRENCY_ISO;} }
    public decimal Value { get{ return m_Value;} }

    public bool IsEmpty { get { return m_CurrencyISO == EMPTY_CURRENCY_ISO && m_Value == EMPTY_AMOUNT; } }

    /// <summary>
    /// Perfoms case-insensitive currency equality comparison
    /// </summary>
    public bool IsSameCurrencyAs(Amount other)
    {
      return CurrencyISO.EqualsOrdIgnoreCase(other.CurrencyISO);
    }


    public static Amount Parse(string val)
    {
      if (val==null) throw new FinancialException(StringConsts.ARGUMENT_ERROR + typeof(Amount).FullName + ".Parse(null)");

      try
      {
        var i = val.IndexOf(':');
        if (i<0)
        {
          var dv = decimal.Parse(val);
          return new Amount(null, dv);
        }
        else
        {
          var iso = i<val.Length-1 ? val.Substring(i+1) : string.Empty;
          var dv = decimal.Parse( val.Substring(0, i) );
          return new Amount(iso, dv);
        }
      }
      catch
      {
        throw new FinancialException(StringConsts.FINANCIAL_AMOUNT_PARSE_ERROR.Args(val));
      }
    }

    public static bool TryParse(string val, out Amount result)
    {
      result = new Amount();

      if (val==null) return false;

      var i = val.IndexOf(':');
      if (i<0)
      {
        decimal dv;
        if (!decimal.TryParse(val, out dv)) return false;
        result = new Amount(null, dv);
        return true;
      }
      else
      {
        var iso = i<val.Length-1 ? val.Substring(i+1) : string.Empty;
        if (i==0) return false;
        decimal dv;
        if (!decimal.TryParse( val.Substring(0, i), out dv )) return false;
        result = new Amount(iso, dv);
        return true;
      }
    }

    #region Object overrides and intfs

        public override string ToString()
        {
          return "{0}:{1}".Args(m_Value.ToString("G"), m_CurrencyISO);
        }

        public override int GetHashCode()
        {
          return m_CurrencyISO.GetHashCode() ^ m_Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
          if (!(obj is Amount)) return false;
          return Equals((Amount)obj);
        }

        public bool Equals(Amount other)
        {
          return IsSameCurrencyAs(other) && m_Value == other.m_Value;
        }


        public int CompareTo(Amount other)
        {
          return this.Equals(other) ? 0 : this < other ? -1 : +1;
        }

        public void WriteAsJSON(System.IO.TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
        {
          wri.Write('{');
            wri.Write("\"iso\":"); JSONWriter.EncodeString(wri, m_CurrencyISO, options);
            wri.Write(',');
            wri.Write("\"v\":"); wri.Write( m_Value );
          wri.Write('}');
        }


    #endregion

    #region Operators

        public static bool operator ==(Amount left, Amount right)
        {
          return Equals(left, right);
        }

        public static bool operator !=(Amount left, Amount right)
        {
          return !Equals(left, right);
        }

        public static bool operator <(Amount left, Amount right)
        {
          return left.IsSameCurrencyAs(right) && (left.Value < right.Value);
        }

        public static bool operator >(Amount left, Amount right)
        {
          return left.IsSameCurrencyAs(right) && (left.Value > right.Value);
        }

        public static bool operator <=(Amount left, Amount right)
        {
          return left.IsSameCurrencyAs(right) && (left.Value <= right.Value);
        }

        public static bool operator >=(Amount left, Amount right)
        {
          return left.IsSameCurrencyAs(right) && (left.Value >= right.Value);
        }

        public static Amount operator +(Amount left, Amount right)
        {
          if (! left.IsSameCurrencyAs(right)) throw new FinancialException(StringConsts.FINANCIAL_AMOUNT_DIFFERENT_CURRENCIES_ERROR.Args('+', left, right));

          return new Amount(left.m_CurrencyISO, left.m_Value + right.m_Value);
        }

        public static Amount operator -(Amount left, Amount right)
        {
          if (! left.IsSameCurrencyAs(right)) throw new FinancialException(StringConsts.FINANCIAL_AMOUNT_DIFFERENT_CURRENCIES_ERROR.Args('-', left, right));

          return new Amount(left.m_CurrencyISO, left.m_Value - right.m_Value);
        }

        public static Amount operator *(Amount left, int right)
        {
          return new Amount(left.m_CurrencyISO, left.m_Value * right);
        }

        public static Amount operator *(int left, Amount right)
        {
          return new Amount(right.m_CurrencyISO, right.m_Value * left);
        }

        public static Amount operator *(Amount left, decimal right)
        {
          return new Amount(left.m_CurrencyISO, left.m_Value * right);
        }

        public static Amount operator *(decimal left, Amount right)
        {
          return new Amount(right.m_CurrencyISO, right.m_Value * left);
        }

        public static Amount operator *(Amount left, double right)
        {
          return new Amount(left.m_CurrencyISO, left.m_Value * (decimal)right);
        }

        public static Amount operator *(double left, Amount right)
        {
          return new Amount(right.m_CurrencyISO, right.m_Value * (decimal)left);
        }

        public static Amount operator /(Amount left, int right)
        {
          return new Amount(left.m_CurrencyISO, left.m_Value / right);
        }

        public static Amount operator /(Amount left, decimal right)
        {
          return new Amount(left.m_CurrencyISO, left.m_Value / right);
        }

        public static Amount operator /(Amount left, double right)
        {
          return new Amount(left.m_CurrencyISO, left.m_Value / (decimal)right);
        }

    #endregion





  }
}