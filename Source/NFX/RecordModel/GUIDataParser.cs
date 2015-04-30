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
 * Originated: 2008.03
 * Revision: NFX 1.0  2011.02.06
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using NFX.Parsing;

namespace NFX.RecordModel
{
  /// <summary>
  /// Provides data-entry parsing routines
  /// </summary>
  public static class GUIDataParser
  {
    
     /// <summary>
     /// Normalizes entered string according to supplied format and culture. Normalization is done by
     ///  adding characters so data looks more standard, i.e. adding dashes and hyphens to phone numbers etc.
     /// May throw exception if normalization is not possible i.e. normalize "uiug" as currency 
     /// </summary>
     public static string NormalizeEnteredString(string value, DataEntryFormat format, CultureInfo culture)
     {
       //so far this is only for US culture, add logic in future to support different cultures supplied in "culture" parameter
       switch (format)
       {
          case DataEntryFormat.Phone:
          {
             return DataEntryUtils.NormalizeUSPhone(value);
          }
       
       
       
          default:   
            return value;
       
       }//switch
     }
    
  
  }
}
