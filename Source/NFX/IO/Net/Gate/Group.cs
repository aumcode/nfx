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

using NFX.Environment;

namespace NFX.IO.Net.Gate
{
  public class Group : INamed, IOrdered
  {

            /// <summary>
            /// Represents the address node of the group
            /// </summary>
            public class Address : INamed, IOrdered
            {
              public Address(string name, int order, string patterns)
              {
                m_Name = name.IsNullOrWhiteSpace() ? Guid.NewGuid().ToString() : name;
                m_Order = order;
                Patterns = patterns ?? string.Empty;
              }

              public Address(IConfigSectionNode node)
              {
                ConfigAttribute.Apply(this, node);
                if (m_Name.IsNullOrWhiteSpace())
                  m_Name = Guid.NewGuid().ToString();
              }


              [Config]private string m_Name;
              [Config]private int    m_Order;
              private string[] m_Patterns;


              public string Name { get{return m_Name;}}
              public int    Order  {get{return m_Order;}}

              [Config]
              public string Patterns
              {
                get {return m_Patterns==null ? NetGate.PATTERN_CAPTURE_WC : string.Join(",",m_Patterns);}
                set {m_Patterns = value.IsNullOrWhiteSpace() ? null : value.Split(Rule.LIST_DELIMITERS, StringSplitOptions.RemoveEmptyEntries);}
              }

              public virtual bool Match(string address)
              {
                if (m_Patterns==null || address.IsNullOrWhiteSpace()) return false;
                return m_Patterns.Any(pat => NFX.Parsing.Utils.MatchPattern(address, pat));
              }
            }


    public Group(string name, int order)
    {
      m_Name = name.IsNullOrWhiteSpace() ? Guid.NewGuid().ToString() : name;
      m_Order = order;
    }

    public Group(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
      if (m_Name.IsNullOrWhiteSpace())
        m_Name = Guid.NewGuid().ToString();

      if (node!=null)
       foreach(var cn in node.Children.Where(cn=>cn.IsSameName(NetGate.CONFIG_ADDRESS_SECTION)))
         if(!m_Addresses.Register( FactoryUtils.Make<Address>(cn, typeof(Address), args: new object[]{ cn })) )
             throw new NetGateException(StringConsts.NETGATE_CONFIG_DUPLICATE_ENTITY_ERROR.Args(cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value, "Group."+NetGate.CONFIG_ADDRESS_SECTION));
    }


    [Config]private string m_Name;
    [Config]private int m_Order;

    private OrderedRegistry<Address> m_Addresses = new OrderedRegistry<Address>();


    public string Name { get{ return m_Name;}}

    /// <summary>
    /// Returns group key for lookup in State.NetState object
    /// </summary>
    public string Key { get { return @"grp,:;|\/"+Name; } }

    public int Order { get { return m_Order; } }

    /// <summary>
    /// Addresses that are part of the group
    /// </summary>
    public OrderedRegistry<Address> Addresses { get {return m_Addresses;}}

    /// <summary>
    /// Tries to find an address in group and returns it or null
    /// </summary>
    public Address Match(string address)
    {
      return m_Addresses.FirstOrDefault(adr=>adr.Match(address));
    }
  }
}
