using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Environment;

namespace NFX.Web.Pay.Tax.TaxJar
{
  public class TaxJarConnectionParameters: TaxConnectionParameters
  {
    public const string CONFIG_APIKEY_ATTR = "api-key";

    public override void Configure(IConfigSectionNode node)
    {
      base.Configure(node);

      m_ApiKey = node.AttrByName(CONFIG_APIKEY_ATTR).Value;
    }

    public string ApiKey { get { return m_ApiKey; } }

    private string m_ApiKey;
  }
}
