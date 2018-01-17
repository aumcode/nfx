/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
using NFX.Security;
using System.Text;

namespace NFX.Web.Pay.PayPal
{
  /// <summary>
  /// Represents basic PayPal credentials for registered application
  /// which include Business account email, client ID and client secret.
  /// </summary>
  public class PayPalCredentials : Credentials
  {
    private const string BASIC_AUTH = "Basic {0}";
    private const string BASIC_AUTH_FORMAT = "{0}:{1}";

    public PayPalCredentials(string clientID, string clientSecret)
    {
      ClientID = clientID;
      ClientSecret = clientSecret;
    }

    public readonly string ClientID;
    public readonly string ClientSecret;

    public string AuthorizationHeader
    { get { return BASIC_AUTH.Args(Convert.ToBase64String(Encoding.UTF8.GetBytes(BASIC_AUTH_FORMAT.Args(ClientID, ClientSecret)))); } }
  }
}
