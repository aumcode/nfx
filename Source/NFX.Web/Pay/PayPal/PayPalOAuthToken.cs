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
using NFX.Serialization.JSON;


namespace NFX.Web.Pay.PayPal
{
  /// <summary>
  /// Represents PayPal OAuth token
  /// </summary>
  public class PayPalOAuthToken
  {
    public PayPalOAuthToken(string applicationID, int expiresInSec, string tokenType, string accessToken, string scope, string nonce, int expirationMarginSec)
    {
      ObtainTime = App.TimeSource.Now;
      ApplicationID = applicationID;
      ExpiresInSec = expiresInSec;
      TokenType = tokenType;
      AccessToken = accessToken;
      Scope = scope;
      Nonce = nonce;
      ExpirationMarginSec = expirationMarginSec;
    }

    public readonly int ExpirationMarginSec;
    public readonly DateTime ObtainTime;
    public readonly string ApplicationID;
    public readonly int ExpiresInSec;
    public readonly string TokenType;
    public readonly string AccessToken;
    public readonly string Scope;
    public readonly string Nonce;

    public string AuthorizationHeader { get { return "{0} {1}".Args(TokenType, AccessToken); } }

    public bool IsCloseToExpire() { return (App.TimeSource.Now - ObtainTime).TotalSeconds >= ExpiresInSec - ExpirationMarginSec; }

    public override string ToString() { return AccessToken; }
  }
}
