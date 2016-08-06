using NFX.Environment;
using NFX.Security;

namespace NFX.Web.Pay.Braintree
{
  public class BraintreeConnectionParameters : PayConnectionParameters
  {
    public const string BRAINTREE_REALM = "braintree";

    public BraintreeConnectionParameters(): base() { }
    public BraintreeConnectionParameters(IConfigSectionNode node): base(node) { }
    public BraintreeConnectionParameters(string connectString, string format = Configuration.CONFIG_LACONIC_FORMAT)
      : base(connectString, format) { }

    public override void Configure(IConfigSectionNode node)
    {
      base.Configure(node);

      var merchantId = node.AttrByName("merchant-id").ValueAsString();
      if (merchantId.IsNullOrWhiteSpace())
        User = User.Fake; //throw new PaymentException("Braintree: " + StringConsts.PAYMENT_BRAINTREE_MERCHANT_ID_REQUIRED.Args(this.GetType().FullName));

      var accessToken = node.AttrByName("access-token").ValueAsString();
      if (accessToken.IsNotNullOrWhiteSpace())
        User = new User(new BraintreeAuthCredentials(merchantId, accessToken), new AuthenticationToken(BRAINTREE_REALM, accessToken), merchantId, Rights.None);
      else
      {
        var publicKey = node.AttrByName("public-key").ValueAsString();
        var privateKey = node.AttrByName("private-key").ValueAsString();
        if (publicKey.IsNullOrWhiteSpace() || privateKey.IsNullOrWhiteSpace())
          User = User.Fake; //throw new PaymentException("Braintree: " + StringConsts.PAYMENT_BRAINTREE_CREDENTIALS_REQUIRED.Args(this.GetType().FullName));

        User = new User(new BraintreeCredentials(merchantId, publicKey, privateKey), new AuthenticationToken(BRAINTREE_REALM, publicKey), merchantId, Rights.None);
      }
    }
  }
}