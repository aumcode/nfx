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
 * Revision: NFX 0.3  2009.10.12
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX
{
  /// <summary>
  /// Represents abstract parameters bag
  /// </summary>
  public interface IParameters
  {

    /// <summary>
    /// Enumerates all parameters
    /// </summary>
    IEnumerable<IParameter> AllParameters { get; }


    /// <summary>
    /// Returns a parameter found by name or throws exception if it could not be found
    /// </summary>
    IParameter ParamByName(string name);

    /// <summary>
    /// Tries to find parameter by name and returns null if parameter could not be found
    /// </summary>
    IParameter FindParamByName(string name);
  }


  /// <summary>
  /// Represents abstract parameter
  /// </summary>
  public interface IParameter : INamed
  {
    /// <summary>
    /// Parameter Value
    /// </summary>
    object Value { get; }

    /// <summary>
    /// Indicates whether parameter has a value, even if Value==null parameter may or
    ///  may not have a value (be assigned) in some scenarious
    /// </summary>
    bool HasValue { get; }

    /// <summary>
    /// Indicates whether parameter is purposed as an input for a target
    /// </summary>
    bool IsInput { get; }
  }


}
