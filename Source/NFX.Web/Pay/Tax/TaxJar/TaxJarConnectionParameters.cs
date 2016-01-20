using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Environment;
using NFX.Security;

namespace NFX.Web.Pay.Tax.TaxJar
{
  public class TaxJarConnectionParameters: TaxConnectionParameters
  {
    public const string TAXJAR_REALM = "stripe";

    public const string CONFIG_EMAIL_ATTR = "email";
    public const string CONFIG_APIKEY_ATTR = "api-key";

    public TaxJarConnectionParameters(): base() {}
    public TaxJarConnectionParameters(IConfigSectionNode node): base(node) {}
    public TaxJarConnectionParameters(string connectString, string format = Configuration.CONFIG_LACONIC_FORMAT)
      : base(connectString, format) {}

    private string m_ApiKey;

    public string ApiKey { get { return m_ApiKey; } set { m_ApiKey = value; } }

    public override void Configure(IConfigSectionNode node)
    {
      base.Configure(node);

      var email = node.AttrByName(CONFIG_EMAIL_ATTR).Value;
      var apiKey = node.AttrByName(CONFIG_APIKEY_ATTR).Value;

      var cred = new TaxJarCredentials(email, apiKey);
      var at = new AuthenticationToken(TAXJAR_REALM, email);

      User = new User(cred, at, UserStatus.User, email, email, Rights.None);
    }
  }
}
