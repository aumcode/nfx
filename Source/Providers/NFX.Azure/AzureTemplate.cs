using System;
using System.Collections.Generic;
using System.Linq;

using NFX;
using NFX.ServiceModel;
using NFX.Environment;
using NFX.Log;
using NFX.ApplicationModel;
using NFX.AzureClient.Compute;

namespace NFX.Web.Cloud.Azure
{
  public class AzureNetworkInterface
  {
    public AzureNetworkInterface(IConfigSectionNode node) { ConfigAttribute.Apply(this, node); }

    [Config("$vnet")]
    public string VNet { get; set; }
    [Config]
    public string Subnet { get; set; }
    [Config("$public-ip")]
    public string PublicIP { get; set; }
    [Config]
    public string SecurityGroup { get; set; }
  }

  public class AzureTemplate : CloudTemplate
  {
    public const string CONFIG_NETWORK_INTERFACE_SECTION = "network-interface";

    public AzureTemplate(CloudSystem system) : base(system) { }
    public AzureTemplate(CloudSystem system, IConfigSectionNode node) : base(system, node) { }

    [Config]
    public string Location { get; set; }
    [Config]
    public VirtualMachineSizeTypes HardwareProfile { get; set; }
    [Config]
    public string Group{ get; set; }
    [Config]
    public string StorageAccount { get; set; }
    [Config]
    public string ImageUri { get; set; }
    [Config("$os-disk-uri")]
    public string OSDiskUri { get; set; }
    [Config]
    public string AdminUsername { get; set; }
    [Config]
    public bool AdminPasswordRandom { get; set; }
    [Config]
    public string AdminPassword { get; set; }

    private List<AzureNetworkInterface> m_NetworkInterfaces = new List<AzureNetworkInterface>();
    public List<AzureNetworkInterface> NetworkInterfaces { get { return m_NetworkInterfaces; } }

    public override void Configure(IConfigSectionNode node)
    {
      base.Configure(node);

      if (AdminPasswordRandom)
        AdminPassword = ExternalRandomGenerator.Instance.NextRandomWebSafeString();

      foreach (var niNode in node.Children.Where(cn => cn.IsSameName(CONFIG_NETWORK_INTERFACE_SECTION)))
        m_NetworkInterfaces.Add(new AzureNetworkInterface(niNode));
    }
  }
}
