/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using NFX;
using NFX.Environment;
using NFX.Security;

namespace NFX.Web.Pay.Stripe
{
  /// <summary>
  /// Represents stripe credentials (test and publihsable keys)
  /// </summary>
  public class StripeCredentials : Credentials
  {
    public StripeCredentials(string email, string secretKey, string publishableKey)
    {
      m_Email = email;
      m_SecretKey = secretKey;
      m_PublishableKey = publishableKey;
    }

    private string m_Email;
    private string m_SecretKey;
    private string m_PublishableKey;

    public string Email { get { return m_Email; }}
    public string SecretKey { get { return m_SecretKey; }}
    public string PublishableKey { get { return m_PublishableKey; }}

    public override string ToString()
    {
      return m_Email;
    }

  } //StripeCredentials
}
