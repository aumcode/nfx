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
