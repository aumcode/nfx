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

using NFX.DataAccess.CRUD;
using NFX.Serialization.Arow;


namespace NFX.Web.Messaging
{
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
      [Field(backendName: "nm", isArow: true)]   public string Name { get; set; }
      [Field(backendName: "ct", isArow: true)]   public byte[] Content { get; set; }
      [Field(backendName: "curl", isArow: true)] public string ContentURL { get; set; }
      [Field(backendName: "tp", isArow: true)]   public string ContentType { get; set; }

      /// <summary>
      /// Returns true to indicate that the content has fetched either as byte[] or URL (that yet needs to be fetched).
      /// This is used in fetching messages back from the store where their attachemnts must be
      /// fetched using a separate call due to their sheer size
      /// </summary>
      public bool HasContent{ get{ return Content!= null || ContentURL.IsNotNullOrWhiteSpace();} }
    }

    protected Message(){ }

    public Message(Guid? id, DateTime? utcCreateDate = null)
    {
      ID = id ?? Guid.NewGuid();
      Priority = MsgPriority.Normal;
      CreateDateUTC = utcCreateDate ?? App.TimeSource.UTCNow;
    }

    /// <summary>
    /// Every message has an ID of type GUID generated upon the creation, it is used for unique identification
    /// in small systems and message co-relation into conversation threads
    /// </summary>
    [Field(backendName: "id", isArow: true)] public Guid  ID { get; private set;}

    /// <summary>
    /// When set, identifies the message in a thread which this one relates to
    /// </summary>
    [Field(backendName: "rel", isArow: true)] public Guid?  RelatedID { get; set;}

    [Field(backendName: "cdt", isArow: true)] public DateTime CreateDateUTC { get; set;}

    [Field(backendName: "pr", isArow: true)] public MsgPriority   Priority   { get; set;}
    [Field(backendName: "im", isArow: true)] public MsgImportance Importance { get; set;}
    [Field(backendName: "fa", isArow: true)] public string FROMAddress{get;set;}
    [Field(backendName: "fn", isArow: true)] public string FROMName{get;set;}


    private string m_ToAddress;
    [Field(backendName: "ta", isArow: true)] public string TOAddress
    {
      get { return m_ToAddress; }
      set { m_ToAddress = value; m_MessageAddress = null; }
    }

    [NonSerialized] private MessageAddressBuilder m_MessageAddress;

    /// <summary>
    /// Returns message address accessor
    /// </summary>
    public MessageAddressBuilder MessageAddress
    {
      get
      {
        if (m_MessageAddress == null) m_MessageAddress = new MessageAddressBuilder(m_ToAddress);
        return m_MessageAddress;
      }
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

    /// <summary>
    /// Collection of Attachments
    /// </summary>
    [Field(backendName: "ats", isArow: true)] public Attachment[] Attachments { get; set; }


    /// <summary>
    /// Adds a single Addressee to the TOAddress collection
    /// </summary>
    public void AddAddressee(MessageAddressBuilder.Addressee addressee)
    {
      MessageAddress.AddAddressee(addressee);
      m_ToAddress = MessageAddress.ToString();
    }
  }
}
