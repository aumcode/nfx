using NFX.AzureClient.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFX.Serialization.JSON;

namespace NFX.AzureClient.Compute
{
  public enum VirtualMachineSizeTypes
  {
    Basic_A0,
    Basic_A1,
    Basic_A2,
    Basic_A3,
    Basic_A4,
    Standard_A0,
    Standard_A1,
    Standard_A2,
    Standard_A2_v2,
    Standard_A3,
    Standard_A4,
    Standard_A5,
    Standard_A6,
    Standard_A7,
    Standard_A8,
    Standard_A9,
    Standard_A10,
    Standard_A11,
    Standard_D1,
    Standard_D2,
    Standard_D3,
    Standard_D4,
    Standard_D11,
    Standard_D12,
    Standard_D13,
    Standard_D14,
    Standard_D1_v2,
    Standard_D2_v2,
    Standard_D3_v2,
    Standard_D4_v2,
    Standard_D5_v2,
    Standard_D11_v2,
    Standard_D12_v2,
    Standard_D13_v2,
    Standard_D14_v2,
    Standard_D15_v2,
    Standard_DS1,
    Standard_DS2,
    Standard_DS3,
    Standard_DS4,
    Standard_DS11,
    Standard_DS12,
    Standard_DS13,
    Standard_DS14,
    Standard_DS1_v2,
    Standard_DS2_v2,
    Standard_DS3_v2,
    Standard_DS4_v2,
    Standard_DS5_v2,
    Standard_DS11_v2,
    Standard_DS12_v2,
    Standard_DS13_v2,
    Standard_DS14_v2,
    Standard_DS15_v2,
    Standard_G1,
    Standard_G2,
    Standard_G3,
    Standard_G4,
    Standard_G5,
    Standard_GS1,
    Standard_GS2,
    Standard_GS3,
    Standard_GS4,
    Standard_GS5
  }

  public enum OperatingSystemTypes
  {
    Windows,
    Linux
  }

  public enum CachingTypes
  {
    None,
    ReadOnly,
    ReadWrite
  }

  public enum DiskCreateOptionTypes
  {
    fromImage,
    empty,
    attach
  }

  public enum StorageAccountTypes
  {
    Standard_LRS,
    Premium_LRS
  }

  public enum PassNames
  {
    oobeSystem
  }

  public enum ComponentNames
  {
    Microsoft_Windows_Shell_Setup
  }

  public enum SettingNames
  {
    AutoLogon,
    FirstLogonCommands
  }

  public enum ProtocolTypes
  {
    Http,
    Https
  }

  public class ImageReference : SubResource
  {
    public string publisher { get; set; }
    public string offer { get; set; }
    public string sku { get; set; }
    public string version { get; set; }
  }

  public class KeyVaultSecretReference : PropertiesFormat
  {
    public string secretUrl { get; set; }
    public SubResource sourceVault { get; set; }
  }

  public class KeyVaultKeyReference : PropertiesFormat
  {
    public string keyUrl { get; set; }
    public SubResource sourceVault { get; set; }
  }

  public class HardwareProfile : PropertiesFormat
  {
    public VirtualMachineSizeTypes? vmSize { get; set; }
  }

  public class VirtualHardDisk : PropertiesFormat
  {
    public string uri { get; set; }
  }

  public class ManagedDiskParameters : SubResource
  {
    public StorageAccountTypes? storageAccountType { get; set; }
  }

  public class BaseDisk : PropertiesFormat
  {
    public string name { get; set; }
    public VirtualHardDisk vhd { get; set; }
    public VirtualHardDisk image { get; set; }
    public CachingTypes? caching { get; set; }
    public DiskCreateOptionTypes? createOption { get; set; }
    public int diskSizeGB { get; set; }
    public ManagedDiskParameters managedDisk { get; set; }
  }

  public class DiskEncryptionSettings : PropertiesFormat
  {
    public KeyVaultSecretReference diskEncryptionKey { get; set; }
    public KeyVaultKeyReference keyEncryptionKey { get; set; }
    public bool enabled { get; set; }
  }

  public class OSDisk : BaseDisk
  {
    public OperatingSystemTypes? osType { get; set; }
    public DiskEncryptionSettings encryptionSettings { get; set; }
  }

  public class DataDisk : BaseDisk
  {
    public int lun { get; set; }
  }

  public class StorageProfile : PropertiesFormat
  {
    public ImageReference imageReference { get; set; }
    public OSDisk osDisk { get; set; }
    public List<DataDisk> dataDisks { get; set; }
  }

  public class AdditionalUnattendContent : PropertiesFormat
  {
    public PassNames? passName { get; set; }
    public ComponentNames? componentName { get; set; }
    public SettingNames? settingName { get; set; }
    public string content { get; set; }
  }

  public class WinRMListener : PropertiesFormat
  {
    public ProtocolTypes? protocol { get; set; }
    public string certificateUrl { get; set; }
  }

  public class WinRMConfiguration : PropertiesFormat
  {
    public List<WinRMListener> listeners { get; set; }
  }

  public class WindowsConfiguration : PropertiesFormat
  {
    public bool? provisionVMAgent { get; set; }
    public bool? enableAutomaticUpdates { get; set; }
    public string timeZone { get; set; }
    public List<AdditionalUnattendContent> additionalUnattendContent { get; set; }
    public WinRMConfiguration winRM { get; set; }
  }

  public class SshPublicKey : PropertiesFormat
  {
    public string path { get; set; }
    public string keyData { get; set; }
  }

  public class SshConfiguration : PropertiesFormat
  {
    public List<SshPublicKey> publicKeys { get; set; }
  }

  public class LinuxConfiguration : PropertiesFormat
  {
    public bool? disablePasswordAuthentication { get; set; }
    public SshConfiguration ssh { get; set; }
  }

  public class VaultCertificate : PropertiesFormat
  {
    public string certificateUrl { get; set; }
    public string certificateStore { get; set; }
  }

  public class VaultSecretGroup : PropertiesFormat
  {
    public SubResource sourceVault { get; set; }
    public VaultCertificate vaultCertificate { get; set; }
  }

  public class OSProfile : PropertiesFormat
  {
    public string computerName { get; set; }
    public string adminUserName { get; set; }
    public string adminPassword { get; set; }
    public string customData { get; set; }
    public WindowsConfiguration windowsConfiguration { get; set; }
    public LinuxConfiguration linuxConfiguration { get; set; }
    public List<VaultSecretGroup> secrets { get; set; }
  }

  public class NetworkInterfaceReferencePropertiesFormat : PropertiesFormat
  {
    public bool? primary { get; set; }
  }

  public class NetworkInterfaceReference : SubResourceWithProperties<NetworkInterfaceReferencePropertiesFormat>
  {
    public NetworkInterfaceReference(NetworkInterface networkInterface)
    {
    }
  }

  public class NetworkProfile : PropertiesFormat
  {
    public List<NetworkInterfaceReference> networkInterfaces { get; set;}
  }

  public class VirtualMachineInstanceView : PropertiesFormat
  {

  }


  public class Plan : PropertiesFormat
  {

  }

  public class VirtualMachineExtension : PropertiesFormat
  {

  }

  public class VirtualMachineIdentity : PropertiesFormat
  {

  }

  public class VirtualMachinePropertiesFormat : PropertiesFormat
  {
    public HardwareProfile hardwareProfile { get; set; }
    public StorageProfile storageProfile { get; set; }
    public OSProfile osProfile { get; set; }
    public NetworkProfile networkProfile { get; set; }
    public string ProvisioningState { get; private set; }
    public VirtualMachineInstanceView instanceView { get; private set; }
    public string licenseType { get; set; }
    public string vmId { get; private set; }
  }

  public class VirtualMachine : NamedResourceWithProperties<VirtualMachinePropertiesFormat>
  {
    public Plan plan { get; set; }
    public List<VirtualMachineExtension> resources { get; set; }
    public VirtualMachineIdentity identity { get; set; }
  }
}
