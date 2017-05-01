using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using NFX.ApplicationModel;
using NFX.Web.Cloud;

namespace NFX.NUnit.Integration.Web.Cloud
{
  [TestFixture]
  public class AzureTest
  {
    public const string LACONF = @"
    nfx
    {
      azure-ad-url='https://login.windows.net'

      starters
      {
        starter
        {
          name='Cloud Systems'
          type='NFX.Web.Cloud.CloudSystemStarter, NFX.Web'
          application-start-break-on-exception=true
        }
      }

      web-settings
      {
        service-point-manager
        {
          security-protocol=4032 // Tls|Tls11|Tls12 binary flag

          /*
          service-point { uri=$(/$azure-ad-url) expect-100-continue=true }
          */

          policy
          {
            default-certificate-validation
            {
              case { uri=$(/$azure-ad-url) trusted=true}
            }
          }
        }

        cloud
        {
          system
          {
            name='Azure'
            type='NFX.Web.Cloud.Azure.AzureSystem, NFX.Azure'
            auto-start=true

            ad-uri=$(/$azure-ad-url)
            resource='https://management.azure.com/'

            default-session-connect-params
            {
              name='AzurePrimary'
              type='NFX.Web.Cloud.Azure.AzureConnectionParameters, NFX.Azure'

              tenant-id=$(~AZURE_TENANT_ID)
              client-id=$(~AZURE_CLIENT_ID)
              client-secret=$(~AZURE_CLIENT_SECRET)
              subscription-id=$(~AZURE_SUBSCRIPTION_ID)
            }

            host-template
            {
              name=Default
              type='NFX.Web.Cloud.Azure.AzureTemplate, NFX.Azure'

              location=northeurope
              hardware-profile=Standard_A2_v2

              group=CNONIM

              storage-account=cnonim
              image-uri='https://$(storage-account).blob.core.windows.net/system/Microsoft.Compute/Images/vhds/cnonim-vm-osDisk.155768bf-1adc-47b3-874b-471c4e8cb0f4.vhd'
              os-disk-uri='https://$(storage-account).blob.core.windows.net/vhds/{0}.vhd'
              admin-username='cnonim'
              admin-password-random=true

              network-interface { vnet=cnonim-vnet subnet=default public-ip=cnonim-ip security-group=cnonim-nsg }
            }
          }
        }
      }
    }";

    private ServiceBaseApplication m_App;

    [TestFixtureSetUp]
    public void SetUp()
    {
      var config = LACONF.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
      m_App = new ServiceBaseApplication(null, config);
    }

    [TestFixtureTearDown]
    public void TearDown() { DisposableObject.DisposeAndNull(ref m_App); }

    [Test]
    public void Deploy()
    {
      using (var session = Azure.StartSession())
        session.Deploy("cnonim-vm", "Default");
    }

    private ICloudSystem m_CloudSystem;
    public ICloudSystem Azure
    {
      get
      {
        if (m_CloudSystem == null) m_CloudSystem = CloudSystem.Instances["Azure"];
        return m_CloudSystem;
      }
    }
  }
}
