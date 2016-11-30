/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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
using System.IO;
using System.Linq;
using System.Text;
using NFX.Serialization.JSON;
using System.Globalization;

namespace NFX.Standards
{
  /// <summary>
  /// Represents length distance with unit type.
  /// All operations are done with presision of 1 micrometer (10^(-3) mm)
  /// </summary>
  public struct Distance : IEquatable<Distance>, IComparable<Distance>, IJSONWritable, IFormattable
  {
    #region Inner

    public enum UnitType
    {
      Cm = 0,
      In,
      Ft,
      Mm,
      M,
      Yd
    }

    #endregion

    #region CONSTS

    public const decimal MM_IN_CM = 10.0m;
    public const decimal MM_IN_IN = 25.4m;
    public const decimal MM_IN_FT = 304.8m;
    public const decimal MM_IN_M = 1000.0m;
    public const decimal MM_IN_YD = 914.4m;
    public const int VALUE_PRECISION = 3;

    #endregion

    public readonly decimal ValueInMm;

    public readonly decimal Value;

    public readonly UnitType Unit;

    public string UnitName
    {
      get
      {
        return Enum.GetName(typeof(UnitType), Unit).ToLower();
      }
    }

    public Distance(decimal value, UnitType unit)
    {
      Unit = unit;
      Value = value;
      switch (Unit)
      {
        case UnitType.Cm: ValueInMm = Math.Round(value * MM_IN_CM, VALUE_PRECISION); break;
        case UnitType.In: ValueInMm = Math.Round(value * MM_IN_IN, VALUE_PRECISION); break;
        case UnitType.Ft: ValueInMm = Math.Round(value * MM_IN_FT, VALUE_PRECISION); break;
        case UnitType.Mm: ValueInMm = Math.Round(value, VALUE_PRECISION); break;
        case UnitType.M: ValueInMm = Math.Round(value * MM_IN_M, VALUE_PRECISION); break;
        case UnitType.Yd: ValueInMm = Math.Round(value * MM_IN_YD, VALUE_PRECISION); break;
        default: throw new NFXException("Unknown distance type:" + Unit);
      }
    }

    public decimal ValueIn(UnitType toUnit)
    {
      switch (toUnit)
      {
        case UnitType.Mm: return ValueInMm;
        case UnitType.Cm: return ValueInMm / MM_IN_CM;
        case UnitType.In: return ValueInMm / MM_IN_IN;
        case UnitType.Ft: return ValueInMm / MM_IN_FT;
        case UnitType.M:  return ValueInMm / MM_IN_M;
        case UnitType.Yd: return ValueInMm / MM_IN_YD;
        default: throw new NFXException("Unknown distance type:" + Unit);
      }
    }

    public Distance Convert(UnitType toUnit)
    {
      if (Unit == toUnit) return this;
      return new Distance(ValueIn(toUnit), toUnit);
    }

    public static Distance Parse(string val)
    {
      if (val == null || val.Length < 2)
        throw new NFXException(StringConsts.ARGUMENT_ERROR + typeof(Distance).FullName + ".Parse({0})".Args(val));

      var nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };
      string valueString;
      string unitString;
      getPair(val, out valueString, out unitString);

      if (valueString == null || unitString == null)
        throw new NFXException(StringConsts.ARGUMENT_ERROR + typeof(Distance).FullName + ".Parse({0})".Args(val));

      return new Distance(decimal.Parse(valueString, nfi), (UnitType)Enum.Parse(typeof(UnitType), unitString));
    }

    public static bool TryParse(string val, out Distance? result)
    {
      var nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };
      string valueString;
      string unitString;
      decimal value;
      UnitType unit;

      getPair(val, out valueString, out unitString);
      if (decimal.TryParse(valueString, NumberStyles.Number, nfi, out value) && Enum.TryParse<UnitType>(unitString, out unit))
      {
        result = new Distance(value, unit);
        return true;
      }
      result = null;
      return false;
    }

    public override bool Equals(Object obj)
    {
      if (obj == null || GetType() != obj.GetType())
        return false;

      Distance d = (Distance)obj;
      return (ValueInMm == d.ValueInMm);
    }

    public override int GetHashCode()
    {
      return ValueInMm.GetHashCode();
    }

    public override string ToString()
    {
      return "{0} {1}".Args(Value.ToString("#,#.###"), UnitName);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
      throw new NotImplementedException();
    }

    #region INTERFACES

    public bool Equals(Distance other)
    {
      return (ValueInMm == other.ValueInMm);
    }

    public int CompareTo(Distance other)
    {
      return ValueInMm.CompareTo(other.ValueInMm);
    }

    public void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
    {
      JSONDataMap map = new JSONDataMap { { "unit", UnitName }, { "value", Value } };
      map.ToJSON(wri, options);
    }

    #endregion

    #region OPERATORS

    public static Distance operator +(Distance obj1, Distance obj2)
    {
      decimal v = obj1.ValueInMm + obj2.ValueInMm;
      return new Distance(v, UnitType.Mm).Convert(obj1.Unit);
    }

    public static Distance operator -(Distance obj1, Distance obj2)
    {
      decimal v = obj1.ValueInMm - obj2.ValueInMm;
      return new Distance(v, UnitType.Mm).Convert(obj1.Unit);
    }

    public static Distance operator *(Distance obj1, decimal obj2)
    {
      decimal v = obj1.ValueInMm * obj2;
      return new Distance(v, UnitType.Mm).Convert(obj1.Unit);
    }

    public static Distance operator /(Distance obj1, decimal obj2)
    {
      decimal v = obj1.ValueInMm / obj2;
      return new Distance(v, UnitType.Mm).Convert(obj1.Unit);
    }

    public static bool operator ==(Distance obj1, Distance obj2)
    {
      return obj1.ValueInMm == obj2.ValueInMm;
    }

    public static bool operator !=(Distance obj1, Distance obj2)
    {
      return obj1.ValueInMm != obj2.ValueInMm;
    }

    public static bool operator >=(Distance obj1, Distance obj2)
    {
      return obj1.ValueInMm >= obj2.ValueInMm;
    }

    public static bool operator <=(Distance obj1, Distance obj2)
    {
      return obj1.ValueInMm <= obj2.ValueInMm;
    }

    public static bool operator >(Distance obj1, Distance obj2)
    {
      return obj1.ValueInMm > obj2.ValueInMm;
    }

    public static bool operator <(Distance obj1, Distance obj2)
    {
      return obj1.ValueInMm < obj2.ValueInMm;
    }

    #endregion

    private static void getPair(string val, out string valueString, out string unitString)
    {
      valueString = null;
      unitString = null;
      const string VALUE_ENDS = "0123456789 ";
      string normString = val.Trim().ToUpper();

      foreach (string unitName in Enum.GetNames(typeof(UnitType)))
      {
        int idx = normString.IndexOf(unitName.ToUpper()); // we search case-insensitive
        if (idx == normString.Length - unitName.Length    // search unit name at the end of string
            && VALUE_ENDS.IndexOf(normString[idx - 1]) >= 0) // left part (value) should ends with VALUE_ENDS char
        {
          valueString = normString.Substring(0, idx).Trim();
          unitString = unitName;
          return;
        }
      }
    }
  }

}
