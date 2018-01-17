/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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

using NFX.Environment;

namespace NFX.Web.Messaging
{
  /// <summary>
  /// Represents events fired on MessageBuilder change
  /// </summary>
  public delegate void MessageBuilderChangeEventHandler(MessageAddressBuilder builder);

  /// <summary>
  /// Facilitates the conversion of config into stream of Addressee entries
  /// </summary>
  public sealed class MessageAddressBuilder
  {
    #region CONSTS
      public const string CONFIG_ROOT_SECT     = "as";
      public const string CONFIG_A_SECT        = "a";
      public const string ATTR_NAME            = "nm";
      public const string ATTR_CHANNEL_NAME    = "cn";
      public const string ATTR_CHANNEL_ADDRESS = "ca";
    #endregion

    /// <summary>
    /// Holds data about an addressee: {Name, Channel, Address (per channel)}, example {"Frank Borland", "UrgentSMTP", "frankb@xyz.com"}
    /// </summary>
    public struct Addressee
    {
      public Addressee(string name, string channelName, string channelAddress)
      {
        Name = name;
        ChannelName = channelName;
        ChannelAddress = channelAddress;
      }

      public readonly string Name;
      public readonly string ChannelName;
      public readonly string ChannelAddress;

      public bool Assigned{ get{ return this.ChannelAddress.IsNotNullOrWhiteSpace();} }
    }

    public static string OneAddressee(string name, string channelName, string channelAddress)
    {
      var b = new MessageAddressBuilder(name, channelName, channelAddress);
      return b.ToString();
    }

    public MessageAddressBuilder(string name, string channelName, string channelAddress)
      : this(new Addressee(name, channelName, channelAddress))
    {
    }

    public MessageAddressBuilder(Addressee addressee) : this(null)
    {
      AddAddressee(addressee);
    }

    public MessageAddressBuilder(string config, MessageBuilderChangeEventHandler onChange = null)
    {
      if (config.IsNullOrWhiteSpace())
      {
        var c = new MemoryConfiguration();
        c.Create(CONFIG_ROOT_SECT);
        m_Config = c.Root;
      }
      else
       m_Config = config.AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      if (onChange!=null)
       MessageBuilderChange+=onChange;
    }


    private ConfigSectionNode m_Config;


    /// <summary>
    /// Subscribe to get change notifications
    /// </summary>
    public event MessageBuilderChangeEventHandler MessageBuilderChange;

    /// <summary>
    /// Enumerates all Addressee instances
    /// </summary>
    public IEnumerable<Addressee> All
    {
      get
      {
        return  m_Config.Children
                        .Where(c => c.IsSameName(CONFIG_A_SECT))
                        .Select(c => new Addressee(
                                         c.AttrByName(ATTR_NAME).ValueAsString(),
                                         c.AttrByName(ATTR_CHANNEL_NAME).ValueAsString(),
                                         c.AttrByName(ATTR_CHANNEL_ADDRESS).ValueAsString()
                                       )
                               );
      }
    }

    #region Public

      public override string ToString()
      {
        return m_Config.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.Compact);
      }

      public bool MatchNamedChannel(IEnumerable<string> channelNames)
      {
        if (channelNames == null || !channelNames.Any()) return false;
        var adrChannelNames = All.Select(a => a.ChannelName);
        return adrChannelNames.Any(c => channelNames.Any(n => n.EqualsOrdIgnoreCase(c)));
      }

      public IEnumerable<Addressee> GetMatchesForChannels(IEnumerable<string> channelNames)
      {
        if (channelNames == null || !channelNames.Any()) return Enumerable.Empty<Addressee>();
        return All.Where(a => channelNames.Any(n => n.EqualsOrdIgnoreCase(a.ChannelName)));
      }

      public Addressee GetFirstOrDefaultMatchForChannels(IEnumerable<string> channelNames)
      {
        return GetMatchesForChannels(channelNames).FirstOrDefault();
      }

      public void AddAddressee(string name, string channelName, string channelAddress)
      {
        AddAddressee(new Addressee(name, channelName, channelAddress));
      }

      public void AddAddressee(Addressee addressee)
      {
        var aSection = m_Config.AddChildNode(CONFIG_A_SECT);
        aSection.AddAttributeNode(ATTR_NAME, addressee.Name);
        aSection.AddAttributeNode(ATTR_CHANNEL_NAME, addressee.ChannelName);
        aSection.AddAttributeNode(ATTR_CHANNEL_ADDRESS, addressee.ChannelAddress);

        if (MessageBuilderChange!=null) MessageBuilderChange(this);
      }

    #endregion
  }
}
