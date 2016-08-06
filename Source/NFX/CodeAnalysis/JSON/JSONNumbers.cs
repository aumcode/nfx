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
using System.Text;
using System.Globalization;

namespace NFX.CodeAnalysis.JSON
{

  public static class JSONNumbers
  {
    /// <summary>
    /// Tries to convert string to number, returns null if conversion could not be made
    /// </summary>
    public static object Convert(string str, out JSONTokenType type)
    {
      ulong inum = 0;
      double fnum = 0.0;

      if (strToInt(str, out inum))
      {
        if (inum > UInt32.MaxValue)
        {
          type = JSONTokenType.tLongIntLiteral;
        }
        else
        {
          type = JSONTokenType.tIntLiteral;
          if (inum<Int32.MaxValue)
           return (Int32)inum;
        }
        return inum;
      }

      if (strToFloat(str, out fnum))
      {
        type = JSONTokenType.tDoubleLiteral;

        return fnum;
      }


      type = JSONTokenType.tUnknown;
      return null;
    }//Convert



    private static bool strToInt(string str, out ulong num)
    {

      if (UInt64.TryParse(str, out num))
        return true;

      if (str.Length>2 && str[0]=='0' && str[1]>'a')//20131215 DKh speed improvement
        try
        {
          if (str.StartsWith("0x"))   //HEX
          {
            num = System.Convert.ToUInt64(str.Substring(2), 16);
            return true;
          }
          else if (str.StartsWith("0b")) //BIN
          {
            num = System.Convert.ToUInt64(str.Substring(2), 2);
            return true;
          }
          else if (str.StartsWith("0o")) //OCT
          {
            num = System.Convert.ToUInt64(str.Substring(2), 8);
            return true;
          }
        }
        catch
        {
        }

      return false;
    }//strToInt

    private static bool strToFloat(string str, out double num)
    {
      return Double.TryParse(str,
                             NumberStyles.Float,
                             CultureInfo.InvariantCulture,  out num);
    }

    //private static bool strToDecimal(string str, out decimal num)
    //{
    //  return Decimal.TryParse(str, out num);
    //}



  }

}
