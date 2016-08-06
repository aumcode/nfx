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

using NFX.Environment;
using NFX.Serialization.JSON;

namespace NFX.Security
{
  /// <summary>
  /// Represents simple ID/password textual credentials
  /// </summary>
  [Serializable]
  public class IDPasswordCredentials : Credentials, IStringRepresentableCredentials
  {

     /// <summary>
     /// Creates IDPass credentials from base64 encoded auth header content as provided by RepresentAsString() method.
     /// Returns null if the content is unparsable
     /// </summary>
     public static IDPasswordCredentials FromBasicAuth(string basicAuth)
     {
        if (basicAuth.IsNullOrWhiteSpace()) return null;

        var bin = Convert.FromBase64String(basicAuth);
        var concat = Encoding.UTF8.GetString(bin).Trim();

        if (concat.IsNullOrWhiteSpace()) return null;

        var i = concat.IndexOf(':');
        if (i<0) return new IDPasswordCredentials(concat, null);

        var id = i==0 ? null : concat.Substring(0, i);
        var pwd = i==concat.Length-1 ? null : concat.Substring(i+1);

        return new IDPasswordCredentials(id, pwd);
     }


     public IDPasswordCredentials(string id, string pwd)
     {
       m_ID = id;
       m_Password = pwd;
     }

     /// <summary>
     /// Warning: storing plain credentials in config file is not secure. Use this method for the most simplistic cases
     /// like unit testing
     /// </summary>
     public IDPasswordCredentials(IConfigSectionNode cfg)
     {
       if (cfg == null || !cfg.Exists)
         throw new SecurityException(StringConsts.ARGUMENT_ERROR + "IDPasswordCredentials.ctor(cfg=null|!exists)");

       ConfigAttribute.Apply(this, cfg);
     }

     [Config] private string m_ID;
     [Config] private string m_Password;


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

     public string RepresentAsString()
     {
       if (Forgotten)
         throw new SecurityException(StringConsts.SECURITY_REPRESENT_CREDENTIALS_FORGOTTEN);

       var concat = "{0}:{1}".Args(m_ID, m_Password);
       var encoded = Encoding.UTF8.GetBytes(concat);

       return Convert.ToBase64String(encoded, Base64FormattingOptions.None);
     }



     public override string ToString()
     {
       return "{0}({1})".Args(GetType().Name, ID);
     }
  }
}
