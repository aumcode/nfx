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
using System.Net;

namespace NFX.IO.Net.Gate
{

  public enum TrafficDirection { Incoming=0, Outgoing}

  /// <summary>
  /// Represents a traffic that passes through network gate
  /// </summary>
  public interface ITraffic
  {
    TrafficDirection Direction{get;}
    string FromAddress{get;}
    string ToAddress{get;}
    string Service{get;}
    string Method {get;}
    string RequestURL {get;}
    IDictionary<string, object> Items {get;}
  }


  /// <summary>
  /// Represents HTTP traffic that arrives via HttpListener
  /// </summary>
  public struct HTTPIncomingTraffic : ITraffic
  {
    public HTTPIncomingTraffic(HttpListenerRequest request)
    {
      m_Request = request;
      m_Items = null;
    }

    private HttpListenerRequest m_Request;
    private Dictionary<string,object> m_Items;

    public TrafficDirection Direction { get{ return TrafficDirection.Incoming;}}

    public string FromAddress{ get{ return m_Request.RemoteEndPoint.Address.ToString();} }

    public string ToAddress{ get{ return m_Request.LocalEndPoint.Address.ToString();} }

    public string Service{ get{ return m_Request.LocalEndPoint.Port.ToString();} }

    public string Method{ get{ return m_Request.HttpMethod;} }

    public string RequestURL{ get{ return m_Request.Url.ToString();}}

    public IDictionary<string, object> Items
    {
      get
      {
        if (m_Items==null)
        {
          m_Items = new Dictionary<string,object>();
          foreach(var key in m_Request.QueryString.AllKeys.Where(k=>k.IsNotNullOrWhiteSpace()))
            m_Items[key] = m_Request.QueryString.Get(key);
        }

        return m_Items;
      }
    }
  }


  /// <summary>
  /// Represents general kind of traffic not bound to any particular technology
  /// </summary>
  public struct GeneralTraffic : ITraffic
  {
    public TrafficDirection Direction { get; set;}

    public string FromAddress{ get; set; }

    public string ToAddress{ get; set; }

    public string Service{ get; set; }

    public string Method{ get; set; }

    public string RequestURL{ get; set;}

    public IDictionary<string, object> Items {get;set;}
  }


}
