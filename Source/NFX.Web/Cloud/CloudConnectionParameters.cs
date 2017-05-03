using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX;
using NFX.Environment;

namespace NFX.Web.Cloud
{
  public class CloudConnectionParameters : INamed, IConfigurable
  {
    #region STATIC
    public static TParams Make<TParams>(IConfigSectionNode node)
      where TParams : CloudConnectionParameters
    { return FactoryUtils.MakeAndConfigure<TParams>(node, typeof(TParams), args: new object[] { node }); }

    public static TParams Make<TParams>(string connStr, string format = Configuration.CONFIG_LACONIC_FORMAT)
      where TParams : CloudConnectionParameters
    { return Make<TParams>(Configuration.ProviderLoadFromString(connStr, format).Root); }
    #endregion

    #region .ctor
    public CloudConnectionParameters() { }
    public CloudConnectionParameters(IConfigSectionNode node) { Configure(node); }
    public CloudConnectionParameters(string connStr, string format = Configuration.CONFIG_LACONIC_FORMAT)
      : this(Configuration.ProviderLoadFromString(connStr, format).Root) { }
    #endregion

    #region Properties
    [Config] public string Name { get; set; }
    #endregion

    #region Public
    public virtual void Configure(IConfigSectionNode node) { ConfigAttribute.Apply(this, node); }
    #endregion
  }
}
