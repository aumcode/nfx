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
using System.IO;
using System.Reflection;

namespace NFX.IO
{
    /// <summary>
    /// Represents a base for stream readers and writers.
    /// Streamer object instances ARE NOT THREAD-safe
    /// </summary>
    public abstract class Streamer
    {
            public static readonly UTF8Encoding UTF8Encoding = new UTF8Encoding(false, false);

            protected Streamer(Encoding encoding = null)
            {
              m_Encoding = encoding ?? UTF8Encoding;

              m_Buff32 = ts_Buff32;
              if (m_Buff32==null)
              {
                var buf = new byte[32];
                m_Buff32 = buf;
                ts_Buff32 = buf;
              }

            }

            [ThreadStatic]
            private static byte[] ts_Buff32;

            protected byte[] m_Buff32;

            protected Stream m_Stream;
            protected Encoding m_Encoding;



            /// <summary>
            /// Returns format that this streamer implements
            /// </summary>
            public abstract StreamerFormat Format
            {
              get;
            }


            /// <summary>
            /// Returns underlying stream if it is bound or null
            /// </summary>
            public Stream Stream
            {
              get { return m_Stream; }
            }

            /// <summary>
            /// Returns stream string encoding
            /// </summary>
            public Encoding Encoding
            {
              get { return m_Encoding; }
            }



            /// <summary>
            /// Sets the stream as the target for output/input.
            /// This call must be coupled with UnbindStream()
            /// </summary>
            public void BindStream(Stream stream)
            {
              if (stream==null)
               throw new NFXIOException(StringConsts.ARGUMENT_ERROR+GetType().FullName+".BindStream(stream==null)");

              if (m_Stream!=null && m_Stream!=stream)
               throw new NFXIOException(StringConsts.ARGUMENT_ERROR+GetType().FullName+" must unbind prior stream first");

              m_Stream = stream;
            }

            /// <summary>
            /// Unbinds the current stream. This call is coupled with BindStream(stream)
            /// </summary>
            public void UndindStream()
            {
              if (m_Stream==null) return;

              if (this is WritingStreamer) m_Stream.Flush();
              m_Stream = null;
            }

    }


}
