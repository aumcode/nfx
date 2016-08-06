/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.NetworkInformation;


namespace NFX.OS
{
  /// <summary>
  /// Provides network-related utilities
  /// </summary>
  public static class NetworkUtils
  {

      /// <summary>
      /// Gets a string which represents a unique signature of this machine based on MAC addresses of interfaces.
      /// The signature has a form of: [intf.count]-[CRC32 of all MACs]-[convoluted MD5 of all MACs]
      /// </summary>
      public static string GetMachineUniqueMACSignature()
      {
          var nics = NetworkInterface.GetAllNetworkInterfaces();
          var buf = new byte[6 * 32];
          var ibuf = 0;
          var cnt=0;
          var csum = new NFX.IO.ErrorHandling.CRC32();
          foreach(var nic in nics.Where(a => a.NetworkInterfaceType!=NetworkInterfaceType.Loopback))
          {
            var mac = nic.GetPhysicalAddress().GetAddressBytes();
            csum.Add( mac );
            for(var i=0; i<mac.Length; i++)
            {
              buf[ibuf] = mac[i];
              ibuf++;
              if(ibuf==buf.Length) ibuf = 0;
            }
            cnt++;
          }

          var md5s = new StringBuilder();
          using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
          {
               var hash = md5.ComputeHash(buf);
               for(var i=0 ; i<hash.Length ; i+=2)
                md5s.Append( (hash[i]^hash[i+1]).ToString("X2"));
          }

          return "{0}-{1}-{2}".Args( cnt, csum.Value.ToString("X8"),  md5s );
      }




  }







        /// <summary>
        /// Provides information about IP network host
        /// </summary>
        [Serializable]
        public sealed class HostNetInfo : INamed
        {
           #region Inner Types
                              /// <summary>
                              /// Describes an address supported by net adapter
                              /// </summary>
                              [Serializable]
                              public sealed class NetAddrInfo : INamed, IOrdered
                              {
                                  internal NetAddrInfo(IPAddressInformation addr, int ord)
                                  {
                                    m_Name = addr.Address.ToString();
                                    m_Order = ord;
                                    m_Bytes = addr.Address.GetAddressBytes();
                                    m_Transient = addr.IsTransient;
                                    m_Family = addr.Address.AddressFamily.ToString();
                                    m_IPv4 = addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
                                    m_IPv6 = addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
                                    m_Unicast = addr is UnicastIPAddressInformation;

                                    if (m_Unicast)
                                    {
                                       var uaddr = (UnicastIPAddressInformation)addr;
                                       m_UnicastIPv4Mask = uaddr.IPv4Mask.GetAddressBytes();
                                       m_UnicastPreferredLifetimeSec = uaddr.AddressPreferredLifetime;
                                       m_UnicastValidLifetimeSec = uaddr.AddressValidLifetime;
                                       m_UnicastDHCPLeaseLifetimeSec = uaddr.DhcpLeaseLifetime;
                                    }

                                  }

                                  private string m_Name;
                                  private int m_Order;
                                  private byte[] m_Bytes;
                                  private bool m_Transient;
                                  private string m_Family;
                                  private bool m_IPv4;
                                  private bool m_IPv6;
                                  private bool m_Unicast;
                                  private byte[] m_UnicastIPv4Mask;
                                  private long m_UnicastPreferredLifetimeSec;
                                  private long m_UnicastValidLifetimeSec;
                                  private long m_UnicastDHCPLeaseLifetimeSec;

                                  public string Name        { get { return m_Name;}}
                                  public int    Order       { get { return m_Order;}}
                                  public byte[] Bytes       { get { return m_Bytes;}}
                                  public bool   Transient   { get { return m_Transient;}}
                                  public string Family      { get { return m_Family;}}
                                  public bool   IPv4        { get { return m_IPv4;}}
                                  public bool   IPv6        { get { return m_IPv6;}}
                                  public bool   Unicast     { get { return m_Unicast;}}
                                  public byte[] UnicastIPv4Mask  { get { return m_UnicastIPv4Mask;}}
                                  public long UnicastPreferredLifetimeSec { get{ return m_UnicastPreferredLifetimeSec;}}
                                  public long UnicastValidLifetimeSec     { get{ return m_UnicastValidLifetimeSec;}}
                                  public long UnicastDHCPLeaseLifetimeSec { get{ return m_UnicastDHCPLeaseLifetimeSec;}}

                                  public override int GetHashCode()
                                  {
                                    return m_Name.GetHashCode();
                                  }

                                  public override bool Equals(object obj)
                                  {
                                    var other = obj as NetAddrInfo;
                                    if (other==null) return false;
                                    return m_Bytes.SequenceEqual(other.m_Bytes);
                                  }

                                  public override string ToString()
                                  {
                                    return m_Name;
                                  }
                              }


                              /// <summary>
                              /// Describes a network adapter on subordinate host
                              /// </summary>
                              [Serializable]
                              public sealed class NetAdapterInfo : INamed, IOrdered
                              {

                                  internal NetAdapterInfo(NetworkInterface nic, int order)
                                  {
                                    m_Name = "{0}::{1}".Args(nic.Id, nic.Name);
                                    m_Order = order;
                                    m_Description = nic.Description;
                                    m_ReceiveOnly = nic.IsReceiveOnly;
                                    m_AdapterType =  nic.NetworkInterfaceType.ToString();
                                    m_Status = nic.OperationalStatus.ToString();
                                    m_Speed = nic.Speed;
                                    m_IPv4Support = nic.Supports( NetworkInterfaceComponent.IPv4 );
                                    m_IPv6Support = nic.Supports( NetworkInterfaceComponent.IPv6 );

                                    var addrs = nic.GetIPProperties().UnicastAddresses;
                                    m_Addresses = new Registry<NetAddrInfo>();
                                    var ord = 0;
                                    foreach(var addr in addrs)
                                    {
                                      m_Addresses.Register( new NetAddrInfo(addr, ord) );
                                      ord++;
                                    }
                                  }

                                  private string m_Name;
                                  private int m_Order;
                                  private string m_Description;
                                  private bool m_ReceiveOnly;
                                  private string m_AdapterType;
                                  private string m_Status;
                                  private long   m_Speed;
                                  private bool   m_IPv4Support;
                                  private bool   m_IPv6Support;
                                  private Registry<NetAddrInfo> m_Addresses;

                                  public string Name        { get { return m_Name;}}
                                  public int    Order       { get { return m_Order;}}
                                  public string Description { get { return m_Description;}}
                                  public bool   ReceiveOnly { get { return m_ReceiveOnly;}}
                                  public string AdapterType { get { return m_AdapterType;}}
                                  public string Status      { get { return m_Status;}}
                                  public long   Speed       { get { return m_Speed;}}
                                  public bool   IPV4Support { get { return m_IPv4Support;}}
                                  public bool   IPV6Support { get { return m_IPv6Support;}}
                                  public IRegistry<NetAddrInfo> Addresses { get{ return m_Addresses;}}

                                  public override int GetHashCode()
                                  {
                                    return m_Name.GetHashCodeIgnoreCase();
                                  }

                                  public override bool Equals(object obj)
                                  {
                                    var other = obj as NetAdapterInfo;
                                    if (other==null) return false;
                                    return this.m_Name.EqualsIgnoreCase(other.Name) && m_Addresses.SequenceEqual(other.m_Addresses);
                                  }

                                  public override string ToString()
                                  {
                                    return "Adapter[{0} {1}]".Args(m_Name, m_Description);
                                  }
                              }
           #endregion


            public static HostNetInfo ForThisHost()
            {
              var computerProps = IPGlobalProperties.GetIPGlobalProperties();

              var result = new HostNetInfo
              {
                m_Name = "{0}.{1}".Args(computerProps.HostName, computerProps.DomainName),
                m_HostNameSegment = computerProps.HostName,
                m_DomainNameSegment = computerProps.DomainName
              };

              var adapters = new OrderedRegistry<NetAdapterInfo>();
              var nics = NetworkInterface.GetAllNetworkInterfaces();
              var ord = 0;
              foreach(var nic in nics)
              {
                adapters.Register( new NetAdapterInfo(nic, ord) );
                ord++;
              }

              result.m_Adapters = adapters;
              return result;
            }


            private HostNetInfo() {}

            private string m_Name;
            private string m_HostNameSegment;
            private string m_DomainNameSegment;
            private OrderedRegistry<NetAdapterInfo> m_Adapters;

            /// <summary>
            /// Full host name including domain
            /// </summary>
            public string Name { get { return m_Name;}}

            /// <summary>
            /// Host name segment of Name
            /// </summary>
            public string HostNameSegment   { get { return m_HostNameSegment;}}

            /// <summary>
            /// Domain name segment of Name
            /// </summary>
            public string DomainNameSegment { get { return m_DomainNameSegment;}}

            /// <summary>
            /// Network adapters on the host
            /// </summary>
            public IRegistry<NetAdapterInfo> Adapters { get { return m_Adapters;}}

            public override int GetHashCode()
            {
              return m_Name.GetHashCode();
            }

            public override bool Equals(object obj)
            {
              var other = obj as HostNetInfo;
              if (other==null) return false;
              return this.m_Name.EqualsIgnoreCase(other.Name) && m_Adapters.SequenceEqual(other.m_Adapters);
            }

            public override string ToString()
            {
              return "{0} ({1})".Args(m_Name, m_Adapters.Count);
            }

        }





}
