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
using System;

using NFX.Security;

namespace NFX.Web.Pay.Braintree
{
  public class BraintreeCredentials : Credentials
  {
    public BraintreeCredentials(string merchantId, string publicKey, string privateKey)
    {
      MerchantID = merchantId;
      PublicKey = publicKey;
      PrivateKey = privateKey;
    }

    public readonly string MerchantID;
    public readonly string PublicKey;
    public readonly string PrivateKey;

    public override string ToString() { return "[{0} {1}]".Args(MerchantID, PublicKey); }
  }

  public class BraintreeAuthCredentials : Credentials
  {
    public BraintreeAuthCredentials(string merchantId, string accessToken)
    {
      MerchantID = merchantId;
      AccessToken = accessToken;
    }
    public readonly string MerchantID;
    public readonly string AccessToken;

    public override string ToString() { return "[{0} {1}]".Args(MerchantID, AccessToken); }
  }
}
