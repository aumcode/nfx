
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
 * Revision: NFX 1.0  3/25/2014 1:10:19 PM
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
using NFX.Security;

namespace NFX.IO.FileSystem.S3
{
  /// <summary>
  /// Represents Amazon S3 credentials (access key and secret key)
  /// </summary>
  [Serializable]
  public class S3Credentials : Credentials
  {
    public S3Credentials(string accessKey, string secretKey)
    {
      m_AccessKey = accessKey;
      m_SecretKey = secretKey;
    }

    private string m_AccessKey;
    private string m_SecretKey;

    public string AccessKey { get { return m_AccessKey; }}
    public string SecretKey { get { return m_SecretKey; }}

    public override string ToString()
    {
      return m_AccessKey;
    }

  } //S3Credentials

}
