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
  /// Represents credentials supplied from/to Social Net site (i.e. Facebook, Twitter etc.)
  /// </summary>
  [Serializable]
  public class SocialNetTokenCredentials : Credentials
  {
     public SocialNetTokenCredentials(string netName, string token, string userName = null)
     {
       m_NetName = netName;
       m_Token = token;
       m_UserName = userName;
     }


     private string m_NetName;
     private string m_Token;
     private string m_UserName;

     /// <summary>
     /// Name of social network that returned the token
     /// </summary>
     public string NetName
     {
       get { return m_NetName ?? string.Empty; }
     }

     /// <summary>
     /// Auth token returned by the network
     /// </summary>
     public string Token
     {
       get { return m_Token ?? string.Empty; }
     }

     /// <summary>
     /// Optional user name as returned from social network (i.e. email or account screen name)
     /// </summary>
     public string UserName
     {
       get { return m_UserName ?? string.Empty; }
     }




     public override void Forget()
     {
       m_Token = string.Empty;
       base.Forget();
     }



     public override string ToString()
     {
       return "{0}({1}:{2})".Args(GetType().Name, NetName, UserName);
     }


  }
}
