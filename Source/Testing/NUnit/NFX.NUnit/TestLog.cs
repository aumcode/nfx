/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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
using NFX.Log;

namespace NFX.NUnit
{
    /// <summary>
    /// A log class for writing messages to memory.
    /// This class is intended for testing purposes only!!!
    /// </summary>
    public class TestMemoryLog : LogServiceBase
    {
        private MessageList m_List = new MessageList();

        public override bool DestinationsAreOptional
        {
          get
          {
            return true;
          }
        }

        /// <summary>
        /// Returns a thread-safe copy of buffered messages
        /// </summary>
        public IList<Message> Messages
        {
            get { lock(m_List) { var r = new MessageList(m_List); return r; } }
        }

        public void Clear()
        {
            lock(m_List)
                m_List.Clear();
        }

        protected override void DoWrite(Message msg, bool urgent)
        {
            lock(m_List)
                m_List.Add(msg);
        }
    }

    /// <summary>
    /// A log class for writing messages synchronously to destinations.
    /// This class is intended for testing purposes only!!!
    /// </summary>
    public class TestSyncLog : LogServiceBase
    {
        protected override void DoWrite(Message msg, bool urgent)
        {
            lock (m_Destinations)
                foreach (var destination in m_Destinations)
                    destination.Send(msg);
        }
    }
}
