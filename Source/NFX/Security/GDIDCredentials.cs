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

using NFX.DataAccess.Distributed;

namespace NFX.Security
{
  /// <summary>
  /// Represents credentials based on Global Distributed ID
  /// </summary>
  [Serializable]
  public class GDIDCredentials : Credentials
  {
     public GDIDCredentials(GDID gdid)
     {
       m_GDID = gdid;
     }


     private GDID m_GDID;


     public GDID GDID
     {
       get { return m_GDID; }
     }

     public override void Forget()
     {
     }



     public override string ToString()
     {
       return "{0}({1})".Args(GetType().Name, GDID);
     }


  }
}
