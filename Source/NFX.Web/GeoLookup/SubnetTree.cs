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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Web.GeoLookup
{
  public struct Subnet : IEnumerable<bool>
  {
    public const int CIDRv4Subnet = 96;

    public static bool IsIPv6(IPAddress original)
    {
      switch (original.AddressFamily)
      {
        case AddressFamily.InterNetworkV6:
          return true;
        case AddressFamily.InterNetwork:
          return false;
        default:
          throw new GeoException("{0}.IsIPv6(net!v4|v6)".Args(typeof(Subnet).Name));
      }
    }

    public static IEnumerable<bool> Enumerate(IPAddress address, int CIDR = -1, bool mapToIPv6 = false)
    {
      if (CIDR >= 0 && mapToIPv6 && !IsIPv6(address)) CIDR += CIDRv4Subnet;
      if (mapToIPv6) address = address.MapToIPv6();
      var bytes = address.GetAddressBytes();
      var count = CIDR < 0 ? bytes.Length * 8 : (int)CIDR;
      for (var i = 0; i < count; i++)
        yield return (bytes[i / 8] & (1u << (7 - (i % 8)))) != 0;
    }

    public readonly IPAddress Address;
    public readonly int CIDR;
    public bool MapToIPv6;

    public Subnet(IPAddress address, int cidr = -1, bool mapToIPv6 = false)
    {
      Address = address;
      CIDR = cidr;
      MapToIPv6 = mapToIPv6;
    }
    public Subnet(string subnet, bool mapToIPv6 = false)
    {
      var parts = subnet.Split('/');
      Address = IPAddress.Parse(parts[0]);
      CIDR = parts[1].AsInt();
      MapToIPv6 = mapToIPv6;
    }

    public IEnumerator<bool> GetEnumerator()
    {
      return Enumerate(Address, CIDR, MapToIPv6).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }

  public class SubnetTree<V> : BinaryIndexedTree<Subnet, V>
  {
    public readonly uint OffsetIPv4;
    public SubnetTree(BinaryIndexedTree<Subnet, V> tree)
      : base(tree.Nodes, tree.Values)
    {
      OffsetIPv4 = 0;
      FindOffset(new Subnet(IPAddress.Any, 0, mapToIPv6: true), ref OffsetIPv4);
    }
    public override V this[Subnet key]
    {
      get
      {
        key.MapToIPv6 = OffsetIPv4 == 0;
        return Find(key, Subnet.IsIPv6(key.Address) ? 0u : OffsetIPv4);
      }
    }
  }
}
