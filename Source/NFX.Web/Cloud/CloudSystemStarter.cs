using System;

using NFX.Environment;
using NFX.ApplicationModel;

namespace NFX.Web.Cloud
{
  public sealed class CloudSystemStarter : IApplicationStarter
  {
    [Config] public string Name { get; set; }

    [Config] public bool ApplicationStartBreakOnException { get; set; }

    public void ApplicationStartBeforeInit(IApplication application) { }

    public void ApplicationStartAfterInit(IApplication application) { CloudSystem.AutoStart(); }

    public void Configure(Environment.IConfigSectionNode node) { ConfigAttribute.Apply(this, node); }
  }
}
