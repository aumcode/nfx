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

namespace NFX.CodeAnalysis.Laconfig
{
  /// <summary>
  /// Provides Laconfig keyword resolution services, this class is thread safe
  /// </summary>
  public static class LaconfigKeywords
  {
    /// <summary>
    /// Resolves a Laconfig keyword
    /// </summary>
    public static LaconfigTokenType Resolve(string str)
    {
      if (str.Length!=1) return LaconfigTokenType.tIdentifier;

      var c = str[0];

      switch(c)
      {
        case '{': return LaconfigTokenType.tBraceOpen;
        case '}': return LaconfigTokenType.tBraceClose;
        case '=': return LaconfigTokenType.tEQ;
      }

      return LaconfigTokenType.tIdentifier;
    }

  }



}
