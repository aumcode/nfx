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

namespace NFX.CodeAnalysis.CSharp
{
  /// <summary>
  /// Provides C# number parsing
  /// </summary>
  public static class CSNumbers
  {
        //Documentation:
        //http://msdn.microsoft.com/en-us/library/aa664674(VS.71).aspx
        public const string S_UINT = "U";
        public const string S_LONG = "L";
        public const string S_ULONG1 = "UL";
        public const string S_ULONG2 = "LU";
        public const string S_FLOAT = "F";
        public const string S_DOUBLE = "D";
        public const string S_DECIMAL = "M";




    /// <summary>
    /// Tries to convert string to number, returns null if conversion could not be made.
    /// Raises exception if operand is wider than specifier allows
    /// </summary>
    public static object Convert(string str, out CSTokenType type)
    {
      ulong inum = 0;
      double fnum = 0.0;
      decimal dnum = 0.0M;


      if (strToInt(str, out inum))
      {
        if (inum > UInt32.MaxValue)
          type = CSTokenType.tLongIntLiteral;
        else
          type = CSTokenType.tIntLiteral;
        return inum;
      }

      if (strToFloat(str, out fnum))
      {
        if (inum > Single.MaxValue)
          type = CSTokenType.tDoubleLiteral;
        else
          type = CSTokenType.tFloatLiteral;

        return fnum;
      }


      type = CSTokenType.tUnknown;

      str = str.ToUpperInvariant();

      //No XLAT Hashtable on purpose , speed and simplicity
      //order of IFs is important because some types have the same endings
      if (str.EndsWith(S_ULONG1))
      {
        str = str.Replace(S_ULONG1, string.Empty);
        type = CSTokenType.tULongIntLiteral;
      }
      else if (str.EndsWith(S_ULONG2))
      {
        str = str.Replace(S_ULONG2, string.Empty);
        type = CSTokenType.tULongIntLiteral;
      }
      else if (str.EndsWith(S_UINT))
      {
        str = str.Replace(S_UINT, string.Empty);
        type = CSTokenType.tUIntLiteral;
      }
      else if (str.EndsWith(S_LONG))
      {
        str = str.Replace(S_LONG, string.Empty);
        type = CSTokenType.tLongIntLiteral;
      }
      else if (str.EndsWith(S_FLOAT))
      {
        str = str.Replace(S_FLOAT, string.Empty);
        type = CSTokenType.tFloatLiteral;
      }
       else if (str.EndsWith(S_DOUBLE))
      {
        str = str.Replace(S_DOUBLE, string.Empty);
        type = CSTokenType.tDoubleLiteral;
      }
      else if (str.EndsWith(S_DECIMAL))
      {
        str = str.Replace(S_DECIMAL, string.Empty);
        type = CSTokenType.tDecimalLiteral;
      }
      else
        return null;



      if ((type != CSTokenType.tDecimalLiteral) &&
          (type != CSTokenType.tFloatLiteral) &&
          (type != CSTokenType.tDoubleLiteral))
      {
        if (strToInt(str, out inum))
        {
          //safeguard - this will never happen unless system supports 128bit arithmetic
          if (inum > UInt32.MaxValue)
            throw new ArgumentException(S_UINT + ": " + inum);//caught by lexer
          return inum;
        }
      }
      else if (type != CSTokenType.tDecimalLiteral)
      {
        if (strToFloat(str, out fnum))
        {
          if (
             (type == CSTokenType.tFloatLiteral) &&
             (fnum > float.MaxValue)
             ) throw new ArgumentException(S_FLOAT + ": " + fnum);//caught by lexer

          return fnum;
        }
      }
      else if (strToDecimal(str, out dnum))
      {
        return dnum;
      }


      return null;
    }//Convert


    /// <summary>
    ///  Converts number string into int respectibg prefixes
    /// </summary>
    private static bool strToInt(string str, out ulong num)
    {

      if (UInt64.TryParse(str, out num))
        return true;

      try
      {

        if (str.StartsWith("0x"))   //HEX
        {
          num = System.Convert.ToUInt64(str.Substring(2), 16);
          return true;
        }
        //C# does not support BIN now, but nice to have
        //else if (str.StartsWith("0b")) //BIN
        //{
        //  num = System.Convert.ToUInt64(str.Substring(2), 2);
        //  return true;
        //}
      }
      catch
      {
      }

      return false;
    }//strToInt

    private static bool strToFloat(string str, out double num)
    {
      if (Double.TryParse(str, out num))
        return true;
      else
        return false;
    }

    private static bool strToDecimal(string str, out decimal num)
    {

      if (Decimal.TryParse(str, out num))
        return true;
      else
        return false;
    }



  }
}
