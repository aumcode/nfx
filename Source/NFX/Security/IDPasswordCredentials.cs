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
  /// Represents simple ID/password textual credentials
  /// </summary>
  [Serializable]
  public class IDPasswordCredentials : Credentials
  {
     public IDPasswordCredentials(string id, string pwd)
     {
       m_ID = id; 
       m_Password = pwd;
     }
     
     
     private string m_ID;
     private string m_Password;
     

     public string ID
     {
       get { return m_ID ?? string.Empty; }
     }

     public string Password
     {
       get { return m_Password ?? string.Empty; }
     }
     
        


     /// <summary>
     /// Deletes sensitive password information.
     /// This method is mostly used on client (vs. server) to prevent process memory-inspection attack.
     /// Its is usually called right after Login() was called.
     /// Implementers may consider forcing post-factum GC.Collect() on all generations to make sure that orphaned 
     /// memory buff with sensitive information, that remains in RAM even after all references are killed, gets
     /// compacted. This class implementation DOES NOT call Gc.Collect();
     /// </summary>
     public override void Forget() //todo  Use SecureString to keep password
     {
       m_Password = string.Empty;
       base.Forget();
     }



     public override string ToString()
     {
       return "{0}({1})".Args(GetType().Name, ID);
     }
  
  
  }
}
