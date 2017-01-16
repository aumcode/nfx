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
namespace NFX.Web.Pay.Braintree
{
  public class BraintreeSession : PaySession
  {
    public BraintreeSession(BraintreeSystem system, BraintreeConnectionParameters cParams, IPaySessionContext context = null)
      : base(system, cParams, context)
    {
    }

    public object ClientToken { get { return PaySystem.GenerateClientToken(this); } }

    protected new BraintreeSystem PaySystem { get { return base.PaySystem as BraintreeSystem; } }

    public string MerchantID
    {
      get
      {
        if (!IsValid) return string.Empty;
        var credentials = User.Credentials as BraintreeCredentials;
        if (credentials == null) return string.Empty;
        return credentials.MerchantID;
      }
    }
  }
}