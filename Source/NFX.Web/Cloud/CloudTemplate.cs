using System;

using NFX.Environment;

namespace NFX.Web.Cloud
{
  public abstract class CloudTemplate : INamed, IConfigurable
  {
    #region STATIC
    public static TTemplate Make<TTemplate>(CloudSystem system, IConfigSectionNode node)
      where TTemplate : CloudTemplate
    { return FactoryUtils.MakeAndConfigure<TTemplate>(node, typeof(TTemplate), args: new object[] { system }); }
    #endregion

    #region .ctor
    public CloudTemplate(CloudSystem system) { }
    public CloudTemplate(CloudSystem system, IConfigSectionNode node) { Configure(node); }
    #endregion

    #region Properties
    [Config] public string Name { get; set; }
    #endregion

    #region Public
    public virtual void Configure(IConfigSectionNode node) { ConfigAttribute.Apply(this, node); }
    #endregion
  }
}
