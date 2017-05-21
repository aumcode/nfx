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
  public enum MsgPriority { Urgent = 0, Normal=1, BelowNormal=2, Slowest = BelowNormal}

  public enum MsgImportance{ Unimportant=-100, LessImportant=-10, Normal=0, Important=10, MoreImportant=100, MostImportant=1000}

  [Flags]
  public enum MsgChannels
  {
    Unspecified = 0x00000000,
    All         = -1,

    EMail  = 1 << 0,
    SMS    = 1 << 1,
    MMS    = 1 << 2,
    Social = 1 << 3,
    Call   = 1 << 4,
    Fax    = 1 << 5,
    Letter = 1 << 6,
    Chat   = 1 << 7
  }

  public enum SendMessageErrorHandling { Ignore=0, Throw }

  public class MessageAddressBuilder
  {
    #region Consts

    public const string CONFIG_A_SECT = "a";
    public const string ATTR_NAME = "name";
    public const string ATTR_CHANNEL_NAME = "channel-name";
    public const string ATTR_CHANNEL_ADDRESS = "channel-address";

    #endregion

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
    private IEnumerable<Addressee> m_All;

    public IEnumerable<Addressee> All
    {
      get
      {
        if (m_All != null) return m_All;
        m_All = m_Config.Children
                        .Where(c => c.IsSameName(CONFIG_A_SECT))
                        .Select(c => new Addressee
                                     {
                                       Name = c.AttrByName(ATTR_NAME).ValueAsString(),
                                       ChannelName = c.AttrByName(ATTR_CHANNEL_NAME).ValueAsString(),
                                       ChannelAddress = c.AttrByName(ATTR_CHANNEL_ADDRESS).ValueAsString()
                                     });
        return m_All;
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
      if (m_All == null)
        m_All = new Addressee[] { addressee };
      else
        m_All = m_All.Concat(new Addressee[] { addressee });

      var aSection = m_Config.AddChildNode(CONFIG_A_SECT);
      aSection.AddAttributeNode(ATTR_NAME, addressee.Name);
      aSection.AddAttributeNode(ATTR_CHANNEL_NAME, addressee.ChannelName);
      aSection.AddAttributeNode(ATTR_CHANNEL_ADDRESS, addressee.ChannelAddress);
    }

    #endregion
  }

  /// <summary>
  /// Describes an entity that can send EMails
  /// </summary>
  public interface IMessenger : IApplicationComponent
  {
    void SendMsg(Message msg);
  }

  public interface IMessengerImplementation : IMessenger, IConfigurable, IService, IApplicationFinishNotifiable
  {

  }

  /// <summary>
  /// Represents an email msg that needs to be sent
  /// </summary>
  [Serializable, Arow]
  public class Message : TypedRow
  {
    [Serializable, Arow]
    public class Attachment : TypedRow
    {
      public Attachment(string name, byte[] content, string contentType)
      {
        Name = name;
        Content = content;
        ContentType = contentType ?? NFX.Web.ContentType.BINARY;
      }
      [Field(backendName: "nm", isArow: true)] public string Name { get; set; }
      [Field(backendName: "ct", isArow: true)] public byte[] Content { get; set; }
      [Field(backendName: "tp", isArow: true)] public string ContentType { get; set; }
    }

    private Message(){ }

    public Message(Guid? guid)
    {
      GUID = guid ?? Guid.NewGuid();
      Priority = MsgPriority.Normal;
    }

    [Field(backendName: "guid", isArow: true)] public Guid GUID { get; private set;}

    [Field(backendName: "pr", isArow: true)] public MsgPriority   Priority   { get; set;}
    [Field(backendName: "im", isArow: true)] public MsgImportance Importance { get; set;}
    [Field(backendName: "fa", isArow: true)] public string FROMAddress{get;set;}
    [Field(backendName: "fn", isArow: true)] public string FROMName{get;set;}

    [Field(backendName: "ta", isArow: true)] public string TOAddress
    {
      get { return m_ToAddress; }
      set { m_ToAddress = value; m_MessageAddress = null; }
    }
    [Field(backendName: "tn", isArow: true)]  public string TOName{get;set;}
    [Field(backendName: "cc", isArow: true)]  public string CC{get;set;}
    [Field(backendName: "bcc", isArow: true)] public string BCC{get;set;}

    [Field(backendName: "sb", isArow: true)] public string Subject{get;set;}
    /// <summary>
    /// Plain/text body
    /// </summary>
    [Field(backendName: "txt", isArow: true)] public string Body{get;set;}
    /// <summary>
    /// HTML-formatted body
    /// </summary>
    [Field(backendName: "html", isArow: true)] public string HTMLBody{get; set;}

    [Field(backendName: "ats", isArow: true)] public Attachment[] Attachments { get; set; }

    private string m_ToAddress;
    [NonSerialized]
    private MessageAddressBuilder m_MessageAddress;

    /// <summary>
    /// returns message address accessor
    /// </summary>
    public MessageAddressBuilder MessageAddress
    {
      get
      {
        if (m_MessageAddress == null) m_MessageAddress = new MessageAddressBuilder(m_ToAddress);
        return m_MessageAddress;
      }
    }

    public void AddAddressee(MessageAddressBuilder.Addressee addressee)
    {
      MessageAddress.AddAddressee(addressee);
      m_ToAddress = MessageAddress.ToString();
    }
  }
}
