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
using System.Text;

namespace NFX.Security
{
  /// <summary>
  /// Represents credentials that are absent. This is a singleton class
  /// </summary>
  [Serializable]
  public class BlankCredentials : Credentials
  {
     private BlankCredentials()
     {
     }

     private static BlankCredentials m_Instance;

     /// <summary>
     /// Singleton instance of blank credentials
     /// </summary>
     public static BlankCredentials Instance
     {
        get
        {
          if (m_Instance==null)
           m_Instance = new BlankCredentials();
          return m_Instance;
        }
     }

  }
}
