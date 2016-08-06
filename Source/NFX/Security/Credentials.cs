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

using NFX.Serialization.JSON;

namespace NFX.Security
{
  /// <summary>
  /// Denotes credentials that can be represented as a string that can be used for example in Authorization header
  /// </summary>
  public interface IStringRepresentableCredentials
  {
    string RepresentAsString();

    bool Forgotten { get; }

    void Forget();
  }

  /// <summary>
  /// User credentials base class. A credentials may be as simple as user+password, access card codes, door key, Twitter account token etc...
  /// </summary>
  [Serializable]
  public abstract class Credentials
  {

    private bool m_Forgotten;

    /// <summary>
    /// Indicates whether Forget() was called on this instance
    /// </summary>
    public bool Forgotten
    {
      get { return m_Forgotten; }
    }


     /// <summary>
     /// Deletes sensitive information (such as password).
     /// This method is mostly used on client (vs. server) to prevent process memory-inspection attack.
     /// Its is usually called right after Login() was called.
     /// Implementers may consider forcing post-factum GC.Collect() on all generations to make sure that orphaned
     /// memory buff with sensitive information, that remains in RAM even after all references are killed, gets
     /// compacted; consequently, this method may take considerable time to execute.
     /// </summary>
     public virtual void Forget()
     {
       m_Forgotten = true;
     }



  }



}
