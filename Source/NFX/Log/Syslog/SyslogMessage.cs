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

namespace NFX.Log.Syslog
{

    /// <summary>
    /// Represents a UNIX-standard SYSLOG message
    /// </summary>
    public sealed class SyslogMessage
    {

        public static SeverityLevel FromNFXLogMessageType(MessageType type)
        {
          if (type<MessageType.Info) return SeverityLevel.Debug;
          if (type<MessageType.Notice) return SeverityLevel.Information;
          if (type<MessageType.Warning) return SeverityLevel.Notice;
          if (type<MessageType.Error) return SeverityLevel.Warning;
          if (type<MessageType.Critical) return SeverityLevel.Error;
          if (type<MessageType.CriticalAlert) return SeverityLevel.Critical;
          if (type<MessageType.Emergency) return SeverityLevel.Alert;

          return SeverityLevel.Emergency;
        }



        public SyslogMessage()
        {

        }

        public SyslogMessage(FacilityLevel facility,
                             SeverityLevel level,
                             string text)
        {
            m_Facility = facility;
            m_Severity = level;
            m_Text = text;
        }

        public SyslogMessage(Message nfxMsg)
        {
           m_Severity = FromNFXLogMessageType(nfxMsg.Type);

           m_Facility = FacilityLevel.User;
           m_LocalTimeStamp = nfxMsg.TimeStamp;
           m_Text = string.Format("{0} - {1} - {2}", nfxMsg.Topic,  nfxMsg.From,  nfxMsg.Text);
        }


        private FacilityLevel m_Facility;
        private SeverityLevel m_Severity;
        private string m_Text;
        private DateTime m_LocalTimeStamp = App.LocalizedTime;


        public FacilityLevel Facility
        {
            get { return m_Facility;}
            set { m_Facility = value; }
        }

        public SeverityLevel Severity
        {
            get { return m_Severity;}
            set { m_Severity = value; }
        }


        public DateTime LocalTimeStamp
        {
            get { return m_LocalTimeStamp;}
            set { m_LocalTimeStamp = value; }
        }



        public string Text
        {
            get { return m_Text ?? string.Empty;}
            set { m_Text = value; }
        }


        public int Priority
        {
           get { return ((int)m_Facility * 8) + ((int)m_Severity);}
        }



    }

}
