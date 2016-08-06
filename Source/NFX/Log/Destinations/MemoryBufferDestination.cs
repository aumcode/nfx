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

namespace NFX.Log.Destinations
{
    /// <summary>
    /// Implements a destination that caches up to specified number of latest log messages in memory
    /// </summary>
    public sealed class MemoryBufferDestination : Destination
    {
       #region
           public const int BUFFER_SIZE_DEFAULT = 1024;

       #endregion


       #region .ctor
        public MemoryBufferDestination() : base(null)
        {

        }

        public MemoryBufferDestination(string name) : base(name)
        {
        }

        protected override void Destructor()
        {
          base.Destructor();
        }
      #endregion

       #region Pvt Fields

        private Message[] m_Buffer;
        private int m_Index;
        private int m_BufferSize = BUFFER_SIZE_DEFAULT;
      #endregion


      #region Properties

        [Config]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
        public int BufferSize
        {
            get { return m_BufferSize;}
            set
            {
                if (value==m_BufferSize) return;
                if (value<1) value=1;
                m_BufferSize = value;
                m_Buffer = null; //atomic
            }
        }


        /// <summary>
        /// Returns all buffered log messages, where X = BufferSize property
        /// </summary>
        public IEnumerable<Message> Buffered {get{ return buffered(true);}}

        /// <summary>
        /// Returns all buffered log messages ordered by timestamp ascending
        /// </summary>
        public IEnumerable<Message> BufferedTimeAscending { get { return buffered(true).OrderBy( msg => msg.TimeStamp.Ticks ); } }

        /// <summary>
        /// Returns all buffered log messages ordered by timestamp descending
        /// </summary>
        public IEnumerable<Message> BufferedTimeDescending  { get { return buffered(false).OrderBy( msg => -msg.TimeStamp.Ticks ); } }

                            private IEnumerable<Message> buffered(bool asc)
                            {
                                var buffer = m_Buffer;  //atomic
                                if (buffer==null) yield break;

                                if (asc)
                                  for(var i=0; i<buffer.Length; i++)
                                  {
                                    var elm = buffer[i];
                                    if (elm!=null) yield return elm;
                                  }
                                else
                                  for(var i=buffer.Length-1; i>=0; i--)
                                  {
                                    var elm = buffer[i];
                                    if (elm!=null) yield return elm;
                                  }
                            }


      #endregion

      #region Public

           /// <summary>
           /// Deletes all buffered messages
           /// </summary>
           public void ClearBuffer()
           {
               m_Buffer = null;//atomic
           }


           public override void Open()
           {
               base.Open();
           }

           public override void Close()
           {
               base.Close();
           }

      #endregion



      #region Protected


        protected internal override void DoSend(Message entry)
        {
          if (m_Buffer==null)
          {
            m_Buffer = new Message[m_BufferSize]; //atomic
            m_Index = 0;
          }

          m_Buffer[m_Index] = entry;//atomic
          m_Index++;
          if (m_Index>=m_Buffer.Length)
            m_Index = 0;
        }

      #endregion
    }
}
