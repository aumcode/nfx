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
/*
 *  Refer to Copyright.txt, Originated by Dmitriy Khmaladze, 1997 - 2008
 *  No party shall claim exclusive commercial rights to this software
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace NFX.CodeAnalysis.JSON
{
  /// <summary>
  /// Identifier validation and other utilities in context of JSON grammar
  /// </summary>
  public static class JSONIdentifiers
  {


    /// <summary>
    /// Checks whether supplied char is suitable for a part of JSON identifier
    /// </summary>
    public static bool ValidateChar(char c)
    {
      return Char.IsLetter(c) || (c == '_');
    }

    /// <summary>
    /// Checks to see whether supplied char is a digit
    /// </summary>
    public static bool ValidateDigit(char c)
    {
      return (c >= '0' && c <= '9');
    }

    /// <summary>
    /// Checks whether supplied string is a valid JSON grammar identifier
    /// </summary>
    public static bool Validate(string id)
    {
      if (id==null) return false;
      if (id.Length < 1) return false;
      if (!ValidateChar(id[0])) return false;

      for (int i = 1; i < id.Length; i++)
      {
        if (! (ValidateChar(id[i]) || ValidateDigit(id[i]))) return false;
      }

      return true;
    }
  }
}

