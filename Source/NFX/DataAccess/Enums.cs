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
namespace NFX.DataAccess
{
  /// <summary>
  /// Defines log level for DataStores
  /// </summary>
  public enum StoreLogLevel
  {
    None = 0,
    Debug,
    Trace
  }

  /// <summary>
  /// Determines whether entity should be loaded/stored from/to storage
  /// </summary>
  public enum StoreFlag
  {
     LoadAndStore = 0,
     OnlyLoad,
     OnlyStore,
     None
  }

  /// <summary>
  /// Types of char casing
  /// </summary>
  public enum CharCase
  {
    /// <summary>
    /// The string remains as-is
    /// </summary>
    AsIs = 0,

    /// <summary>
    /// The string is converted to upper case
    /// </summary>
    Upper,

    /// <summary>
    /// The string is converted to lower case
    /// </summary>
    Lower,

    /// <summary>
    /// The first and subsequent chars after space or '.' are capitalized, the rest left intact
    /// </summary>
    Caps,

    /// <summary>
    /// The first and subsequent chars after space or '.' are capitalized, the rest is lower-cased
    /// </summary>
    CapsNorm
  }
}
