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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

using NFX.Time;
using NFX.Log.Destinations;
using NFX.Serialization.JSON;

namespace NFX.Log
{
  /// <summary>
  /// Represents a Log message
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
        /// Gets/Sets message source line number/tracepoint#, this is used in conjunction with From
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
        /// Gets/Sets logical component ID, such as: class name, method name, process instance, that generated the message.
        /// This field is used in the scope of Topic
        /// </summary>
        public string From
        {
          get { return m_From ?? string.Empty; }
          set { m_From = value; }
        }

        /// <summary>
        /// Gets/Sets a message topic/relation - the name of software concern within the big app, i.e. "Database" or "Security"
        /// </summary>
        public string Topic
        {
          get { return m_Topic ?? string.Empty; }
          set { m_Topic = value; }
        }

        /// <summary>
        /// Gets/Sets an unstructured message text, the emitting component name must be in From field, not in text.
        /// Note about logging errors. Use caught exception.ToMessageWithType() method, then attach the caught exception as Exception property
        /// </summary>
        public string Text
        {
          get { return m_Text ?? string.Empty; }
          set { m_Text = value; }
        }

        /// <summary>
        /// Gets/Sets a structured parameter bag, this may be used for additional debug info like source file name, additional context etc.
        /// </summary>
        public string Parameters
        {
          get { return m_Parameters ?? string.Empty; }
          set { m_Parameters = value; }
        }

        /// <summary>
        /// Gets/Sets exception associated with message.
        /// Set this property EVEN IF the name/text of exception is already included in Text as log destinations may elect to dump the whole stack trace
        /// </summary>
        public Exception Exception
        {
          get { return m_Exception; }
          set { m_Exception = value; }
        }

    #endregion


    public Message()
    {
      m_Guid = Guid.NewGuid();
      m_Host = Message.DefaultHostName ?? System.Environment.MachineName;
      m_TimeStamp = App.Instance.Log.LocalizedTime;
    }

    /// <summary>
    /// Creates message with Parameters supplanted with caller file name and line #
    /// </summary>
    public Message(object pars, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) : this()
    {
      SetParamsAsObject( FormatCallerParams(pars, file, line) );
      Source = line;
    }



    public override string ToString()
    {
      return "{0}/{1}, {2}, {3}, {4}, {5}, {6}, {7}".Args(
                           m_Type,
                           m_Source,
                           m_TimeStamp,
                           Host,
                           From,
                           Topic,
                           Text,
                           Exception!=null ? Exception.ToString() : string.Empty);
    }

    /// <summary>
    /// Supplants the from string with caller as JSON string
    /// </summary>
    public static object FormatCallerParams(object pars,
                                            [CallerFilePath]  string file = null,
                                            [CallerLineNumber]int line = 0)
    {
      if (pars==null)
        return new
          {
            clrF = file.IsNotNullOrWhiteSpace() ? Path.GetFileName(file) : null,
            clrL = line,
          };
      else
        return new
          {
            clrF = file.IsNotNullOrWhiteSpace() ? Path.GetFileName(file) : null,
            clrL = line,
            p = pars
          };
    }

    public Message SetParamsAsObject(object p)
    {
      if (p==null)
        m_Parameters = null;
      else
        m_Parameters = p.ToJSON(JSONWritingOptions.CompactASCII);

      return this;
    }

  }

  internal class MessageList : List<Message>
  {
        public MessageList() {}
        public MessageList(IEnumerable<Message> other) : base(other) {}
  }
}
