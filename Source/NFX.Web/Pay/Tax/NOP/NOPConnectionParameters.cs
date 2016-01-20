using NFX.Environment;
using NFX.Security;

namespace NFX.Web.Pay.Tax.NOP
{
  public class NOPConnectionParameters: TaxConnectionParameters
  {
    public const string NOP_REALM = "NOP";

    public const string CONFIG_EMAIL_ATTR = "email";
    public const string CONFIG_APIKEY_ATTR = "api-key";

    public NOPConnectionParameters(): base() {}
    public NOPConnectionParameters(IConfigSectionNode node): base(node) {}
    public NOPConnectionParameters(string connectString, string format = Configuration.CONFIG_LACONIC_FORMAT)
      : base(connectString, format) {}

    private string m_ApiKey;

    public string ApiKey { get { return m_ApiKey; } }

    public override void Configure(IConfigSectionNode node)
    {
      base.Configure(node);

      var email = node.AttrByName(CONFIG_EMAIL_ATTR).Value;
      var cred = new NOPCredentials(email);
      var at = new AuthenticationToken(NOP_REALM, email);

      User = new User(cred, at, email, Rights.None);
    }
  }
}
