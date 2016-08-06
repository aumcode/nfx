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
using System.Threading.Tasks;

using NFX.Environment;
using NFX.ApplicationModel;
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
    Letter = 1 << 6
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
  [Serializable]
  public class Message
  {
    public struct Attachment
    {
       public Attachment(string name, byte[] content, string contentType)
       {
         Name = name;
         Content = content;
         ContentType = contentType ?? NFX.Web.ContentType.BINARY;
       }
       public readonly string Name;
       public readonly byte[] Content;
       public readonly string ContentType;
    }

    private Message(){ }

    public Message(Guid? guid)
    {
      GUID = guid ?? Guid.NewGuid();
      Priority = MsgPriority.Normal;
    }

    public Guid GUID { get; private set;}

    public MsgPriority   Priority   { get; set;}
    public MsgImportance Importance { get; set;}
    public MsgChannels   Channels   { get; set;}
    public string FROMAddress{get;set;}
    public string FROMName{get;set;}

    public string TOAddress{get;set;}
    public string TOName{get;set;}
    public string CC{get;set;}
    public string BCC{get;set;}

    public string Subject{get;set;}

    /// <summary>
    /// Plain/text body
    /// </summary>
    public string Body{get;set;}

    /// <summary>
    /// HTML-formatted body
    /// </summary>
    public string HTMLBody{get; set;}

    public Attachment[] Attachments;
  }

}
