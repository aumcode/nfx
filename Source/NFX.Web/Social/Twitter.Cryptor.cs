
/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 Dmitriy Khmaladze, IT Adapter Inc / 2015-2016 Aum Code LLC
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
 * Revision: NFX 1.0  1/14/2014 2:59:35 PM
 * Author: Denis Latushkin<dxwizard@gmail.com>
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using NFX;
using NFX.Environment;
using System.Security.Cryptography;
using NFX.Web.IO;

namespace NFX.Web.Social
{
  public partial class Twitter : SocialNetwork
  {

    #region Inner Types

      public class TwitterCryptor
      {
        private HMACSHA1 m_Crypto;

        public TwitterCryptor(string consumerSecret, string oauthTokenSecret = null)
        {
          string signingKey = RFC3986.Encode(consumerSecret) + '&' + RFC3986.Encode(oauthTokenSecret);
          m_Crypto = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey));
          m_Crypto.Initialize();
        }

        public string GetHashString(string src)
        {
          byte[] srcBytes = Encoding.UTF8.GetBytes(src);
          byte[] hashBytes = m_Crypto.ComputeHash(srcBytes);
          string hashStr = Convert.ToBase64String(hashBytes);
          return hashStr;
        }
      }

    #endregion

  } //TwitterCryptor

}
