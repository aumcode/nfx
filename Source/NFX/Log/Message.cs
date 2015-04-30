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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 0.3  2009.10.12
 */
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using NFX.Time;
using NFX.Log.Destinations;

namespace NFX.Log
{
  /// <summary>
  /// Represents an Log message 
  /// </summary>
  [Serializable]
  public sealed class Message
  {
    
      public static string DefaultHostName;
    
    #region Private Fields
            private Guid m_Guid;
            private Guid m_RelatedTo;
            private MessageType m_Type;
            private int m_Source;
            private DateTime m_TimeStamp;
            private string m_Host;
            private string m_From;
            private string m_Topic;
            private string m_Text;
            private string m_Parameters;
            private Exception m_Exception;
            private int m_ThreadID;
    #endregion


    #region Properties

        /// <summary>
        /// Returns global unique identifier for this particular message
        /// </summary>
        public Guid Guid
        {
          get { return m_Guid; }
        }

        /// <summary>
        /// Gets/Sets global unique identifier of a message that this message is related to.
        /// No referential integrity check is performed
        /// </summary>
        public Guid RelatedTo
        {
          get { return m_RelatedTo; }
          set { m_RelatedTo = value; }
        }

        /// <summary>
        /// Gets/Sets message type, such as: Info/Warning/Error etc...
        /// </summary>
        public MessageType Type
        {
          get { return m_Type; }
          set { m_Type = value; }
        }

        /// <summary>
        /// Gets/Sets message source, particular applications may elect to cast to their enums
        /// </summary>
        public int Source
        {
          get { return m_Source; }
          set { m_Source = value; }
        }




        /// <summary>
        /// Gets/Sets timestamp when message was generated
        /// </summary>
        public DateTime TimeStamp
        {
          get { return m_TimeStamp; }
          set { m_TimeStamp = value; }
        }

        
        /// <summary>
        /// Gets/Sets host name that generated the message
        /// </summary>
        public string Host
        {
          get { return m_Host ?? string.Empty; }
          set { m_Host = value; }
        }
        
        
        /// <summary>
        /// Gets/Sets instance name/ID/type, such as: class name, method name, process instance, that generated the message
        /// </summary>
        public string From
        {
          get { return m_From ?? string.Empty; }
          set { m_From = value; }
        }

        /// <summary>
        /// Gets/Sets a message type in unstructured textual form - message topic
        /// </summary>
        public string Topic
        {
          get { return m_Topic ?? string.Empty; }
          set { m_Topic = value; }
        }

        /// <summary>
        /// Gets/Sets an unstructured message text
        /// </summary>
        public string Text
        {
          get { return m_Text ?? string.Empty; }
          set { m_Text = value; }
        }

        /// <summary>
        /// Gets/Sets a structured parameter bag 
        /// </summary>
        public string Parameters 
        {
          get { return m_Parameters ?? string.Empty; }
          set { m_Parameters = value; }
        }

        /// <summary>
        /// Returns the ID of the thread that created the message
        /// </summary>
        public int ThreadID { get { return m_ThreadID; } }

        /// <summary>
        /// Gets/Sets exception
        /// </summary>
        public Exception Exception 
        {
          get { return m_Exception; }
          set { m_Exception = value; }
        }

    #endregion

    private Message(DateTime timestamp)
    {
      m_Guid = Guid.NewGuid();
      m_Host = Message.DefaultHostName ?? System.Environment.MachineName;
      m_TimeStamp = timestamp;
      m_ThreadID = Thread.CurrentThread.ManagedThreadId;
    }

    public Message() : this(App.Instance.Log.LocalizedTime)
    {}

    public Message(Guid relatedTo,
                    MessageType type,
                    int source,
                    DateTime timeStamp,
                    string from,
                    string topic,
                    string text,
                    Exception exception)
      : this(timeStamp)
    {
      m_RelatedTo = relatedTo;
      m_Type = type;
      m_Source = source;
      m_From = from;
      m_Topic = topic;
      m_Text = text;
      m_Exception = exception;
    }


    public override string ToString()
    {
      return string.Format("{0}/{1}, {2}, {3}, {4}, {5}, {6}, {7}",
                           m_Type,
                           m_Source,
                           m_TimeStamp,
                           Host,
                           From,
                           Topic,
                           Text,
                           Exception!=null ? Exception.ToString() : string.Empty);
    }
  }

  internal class MessageList : List<Message>
  {
        public MessageList() {}
        public MessageList(IEnumerable<Message> other) : base(other) {}
  }

}
