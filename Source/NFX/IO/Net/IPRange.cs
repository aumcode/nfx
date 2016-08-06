using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NFX.IO.Net
{
  /// <summary>
  /// Provides a reference-free key for matching of IP/net ranges
  /// </summary>
  public struct IPRange : IEqualityComparer<IPRange>
  {
    public IPRange(string cidr) //ip/len i.e.:   2.56.34.11/24
    {

    }

    public IPRange(IPAddress ip, int len)// parsed already
    {
      if (ip.AddressFamily==System.Net.Sockets.AddressFamily.InterNetworkV6)
      {

      }
      else
      {//V4
  //      m_V4 = ip.Address;
      }


    }

    public IPRange(IPAddress ip) // for math making, al bytes are used
    {

    }

  //  private long m_V4;
  //  private long m_V6H;
  //  private long m_V6L;




    public bool Equals(IPRange x, IPRange y)
    {
      throw new NotImplementedException();
    }

    public int GetHashCode(IPRange obj)
    {
      throw new NotImplementedException();
    }
  }


}
