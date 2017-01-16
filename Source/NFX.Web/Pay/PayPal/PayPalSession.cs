/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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
using NFX.Security;

namespace NFX.Web.Pay.PayPal
{
  /// <summary>
  /// Represents PayPal pay session
  /// </summary>
  public class PayPalSession : PaySession
  {
    public PayPalSession(PayPalSystem paySystem, PayPalConnectionParameters cParams, IPaySessionContext context = null)
        : base(paySystem, cParams, context)
    {
    }

    public PayPalOAuthToken AuthorizationToken
    {
      get
      {
        if (!IsValid) return null;

        var token = User.AuthToken.Data as PayPalOAuthToken;

        if (token == null || token.IsCloseToExpire())
        {
          token = ((PayPalSystem)PaySystem).generateOAuthToken((PayPalCredentials)User.Credentials);
          User = new User(User.Credentials, new AuthenticationToken(PayPalSystem.PAYPAL_REALM, token), User.Name, User.Rights);
        }

        return token;
      }
    }

    public void ResetAuthorizationToken()
    {
      User = new User(User.Credentials, new AuthenticationToken(PayPalSystem.PAYPAL_REALM, null), User.Name, User.Rights);
    }
  }
}
