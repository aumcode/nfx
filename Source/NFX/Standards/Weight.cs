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
using NFX.Serialization.JSON;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Standards
{
  /// <summary>
  /// Represents weight with unit type.
  /// All operations are done with presision of 1 milligramm
  /// </summary>
  public struct Weight : IEquatable<Weight>, IComparable<Weight>, IJSONWritable, IFormattable
  {
    #region Inner

    public enum UnitType
    {
      G = 0,
      Oz,
      Lb,
      Kg
    }//add aliases

    #endregion

    #region CONSTS

    public const decimal G_IN_OZ = 28.3495m;
    public const decimal G_IN_LB = 453.592m;
    public const decimal G_IN_KG = 1000.0m;
    public const int VALUE_PRECISION = 3;

    #endregion

    public readonly decimal ValueInGrams;

    public readonly decimal Value;

    public readonly UnitType Unit;

    public string UnitName
    {
      get
      {
        return Enum.GetName(typeof(UnitType), Unit).ToLower();
      }
    }

    public Weight(decimal value, UnitType unit)
    {
      Unit = unit;
      //Value = value;
      switch (Unit)
      {
        case UnitType.G:
          ValueInGrams = Math.Round(value, VALUE_PRECISION);
          Value = ValueInGrams;
          break;
        case UnitType.Oz:
          ValueInGrams = Math.Round(value * G_IN_OZ, VALUE_PRECISION);
          Value = ValueInGrams / G_IN_OZ;
          break;
        case UnitType.Lb:
          ValueInGrams = Math.Round(value * G_IN_LB, VALUE_PRECISION);
          Value = ValueInGrams / G_IN_LB;
          break;
        case UnitType.Kg:
          ValueInGrams = Math.Round(value * G_IN_KG, VALUE_PRECISION);
          Value = ValueInGrams / G_IN_KG;
          break;
        default: throw new NFXException("Unknown weight type:" + Unit);
      }
    }

    public decimal ValueIn(UnitType toUnit)
    {
      switch (toUnit)
      {
        case UnitType.G:  return ValueInGrams;
        case UnitType.Oz: return ValueInGrams / G_IN_OZ;
        case UnitType.Lb: return ValueInGrams / G_IN_LB;
        case UnitType.Kg: return ValueInGrams / G_IN_KG;
        default: throw new NFXException("Unknown weight type:" + Unit);
      }
    }

    public Weight Convert(UnitType toUnit)
    {
      if (Unit == toUnit) return this;
      return new Weight(ValueIn(toUnit), toUnit);
    }

    public static Weight Parse(string val)
    {
      if (val == null || val.Length < 2)
        throw new NFXException(StringConsts.ARGUMENT_ERROR + typeof(Weight).FullName + ".Parse({0})".Args(val));

      var nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };
      string valueString;
      string unitString;
      getPair(val, out valueString, out unitString);

      if (valueString == null || unitString == null)
        throw new NFXException(StringConsts.ARGUMENT_ERROR + typeof(Weight).FullName + ".Parse({0})".Args(val));

      return new Weight(decimal.Parse(valueString, nfi), (UnitType)Enum.Parse(typeof(UnitType), unitString));
    }

    public static bool TryParse(string val, out Weight? result)
    {
      var nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };
      string valueString;
      string unitString;
      decimal value;
      UnitType unit;

      getPair(val, out valueString, out unitString);
      if (decimal.TryParse(valueString, NumberStyles.Number, nfi, out value) && Enum.TryParse<UnitType>(unitString, out unit))
      {
        result = new Weight(value, unit);
        return true;
      }
      result = null;
      return false;
    }

    public override bool Equals(Object obj)
    {
      if (obj == null || GetType() != obj.GetType())
        return false;

      var d = (Weight)obj;
      return (ValueInGrams == d.ValueInGrams);
    }

    public override int GetHashCode()
    {
      return ValueInGrams.GetHashCode();
    }

    public override string ToString()
    {
      return "{0} {1}".Args(Value.ToString("#,#.###"), UnitName);
    }

    public String ToString(String format, IFormatProvider formatProvider)
    {
      throw new NotImplementedException();
    }

    #region INTERFACES

    public bool Equals(Weight other)
    {
      return (ValueInGrams == other.ValueInGrams);
    }

    public int CompareTo(Weight other)
    {
      return ValueInGrams.CompareTo(other.ValueInGrams);
    }

    public void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
    {
      JSONDataMap map = new JSONDataMap { { "unit", UnitName }, { "value", Value } };
      map.ToJSON(wri, options);
    }

    #endregion

    #region OPERATORS

    public static Weight operator +(Weight obj1, Weight obj2)
    {
      decimal v = obj1.ValueInGrams + obj2.ValueInGrams;
      return new Weight(v, UnitType.G).Convert(obj1.Unit);
    }

    public static Weight operator -(Weight obj1, Weight obj2)
    {
      decimal v = obj1.ValueInGrams - obj2.ValueInGrams;
      return new Weight(v, UnitType.G).Convert(obj1.Unit);
    }

    public static Weight operator *(Weight obj1, decimal obj2)
    {
      decimal v = obj1.ValueInGrams * obj2;
      return new Weight(v, UnitType.G).Convert(obj1.Unit);
    }

    public static Weight operator /(Weight obj1, decimal obj2)
    {
      decimal v = obj1.ValueInGrams / obj2;
      return new Weight(v, UnitType.G).Convert(obj1.Unit);
    }

    public static bool operator ==(Weight obj1, Weight obj2)
    {
      return obj1.ValueInGrams == obj2.ValueInGrams;
    }

    public static bool operator !=(Weight obj1, Weight obj2)
    {
      return obj1.ValueInGrams != obj2.ValueInGrams;
    }

    public static bool operator >=(Weight obj1, Weight obj2)
    {
      return obj1.ValueInGrams >= obj2.ValueInGrams;
    }

    public static bool operator <=(Weight obj1, Weight obj2)
    {
      return obj1.ValueInGrams <= obj2.ValueInGrams;
    }

    public static bool operator >(Weight obj1, Weight obj2)
    {
      return obj1.ValueInGrams > obj2.ValueInGrams;
    }

    public static bool operator <(Weight obj1, Weight obj2)
    {
      return obj1.ValueInGrams < obj2.ValueInGrams;
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
