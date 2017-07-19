/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using System.Threading.Tasks;

using NFX.ApplicationModel;
using NFX.DataAccess.CRUD;
using NFX.Environment;
using NFX.Serialization.Arow;
using NFX.ServiceModel;

namespace NFX.Web.Messaging
{
  /// <summary>
  /// Facilitates the conversion of config into stream of Addressee entries
  /// </summary>
  public sealed class MessageAddressBuilder
  {
    #region CONSTS
      public const string CONFIG_A_SECT = "a";
      public const string ATTR_NAME = "name";
      public const string ATTR_CHANNEL_NAME = "channel-name";
      public const string ATTR_CHANNEL_ADDRESS = "channel-address";
    #endregion

    /// <summary>
    /// Holds data about an adressee: {Name, Channel, Address (per channel)}, example {"Frank Borland", "UrgentSMTP", "frankb@xyz.com"}
    /// </summary>
    public struct Addressee
    {
      public string Name { get; set; }
      public string ChannelName { get; set; }
      public string ChannelAddress { get; set; }
    }

    public MessageAddressBuilder(string config)
    {
      m_Config = (config ?? string.Empty).AsLaconicConfig(handling: ConvertErrorHandling.Throw);
    }

    private ConfigSectionNode m_Config;

    /// <summary>
    /// Enumerates all Addressee instances
    /// </summary>
    public IEnumerable<Addressee> All
    {
      get
      {
        return  m_Config.Children
                        .Where(c => c.IsSameName(CONFIG_A_SECT))
                        .Select(c => new Addressee
                                     {
                                       Name = c.AttrByName(ATTR_NAME).ValueAsString(),
                                       ChannelName = c.AttrByName(ATTR_CHANNEL_NAME).ValueAsString(),
                                       ChannelAddress = c.AttrByName(ATTR_CHANNEL_ADDRESS).ValueAsString()
                                     });
      }
    }

    #region Public

      public override string ToString()
      {
        return m_Config.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.Compact);
      }

      public bool MatchNamedChannel(IEnumerable<string> names)
      {
        if (names == null || !names.Any()) return false;
        var channelNames = All.Select(a => a.ChannelName);
        return channelNames.Any(c => names.Any(n => n.EqualsOrdIgnoreCase(c)));
      }

      public void AddAddressee(Addressee addressee)
      {
        var aSection = m_Config.AddChildNode(CONFIG_A_SECT);
        aSection.AddAttributeNode(ATTR_NAME, addressee.Name);
        aSection.AddAttributeNode(ATTR_CHANNEL_NAME, addressee.ChannelName);
        aSection.AddAttributeNode(ATTR_CHANNEL_ADDRESS, addressee.ChannelAddress);
      }

    #endregion
  }
}
