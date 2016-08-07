/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 Dmitriy Khmaladze, IT Adapter Inc / 2015-2016 Aum Code LLC
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


// Author: Serge Aleynikov <saleyn@gmail.com>
// Date:   2013-07-23
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Environment;
using System.Text.RegularExpressions;
using System.IO;

namespace NFX.Log.Destinations
{
    /// <summary>
    /// Provides a file storage destination implementation
    /// </summary>
    public abstract class TextFileDestination : FileDestination
    {
        #region .ctor

            /// <summary>
            /// Creates a new instance of destination
            /// </summary>
            public TextFileDestination(string name = null) : base(name) { }

            /// <summary>
            /// Creates a new instance of destination
            /// </summary>
            public TextFileDestination(string name, string filename, string path = null)
                : base(name, filename, path)
            {}

        #endregion


        #region Pvt Fields

            private StreamWriter m_Writer;

        #endregion

        #region Public

            protected override void DoOnOpenStream()
            {
                m_Writer = new StreamWriter(Stream);
            }

            protected override void DoOnCloseStream()
            {
                if (m_Writer != null)
                {
                    m_Writer.Flush();
                    m_Writer.Close();
                    m_Writer.Dispose();
                    m_Writer = null;
                }
            }

        #endregion


        #region Protected

            protected override void DoConfigure(IConfigSectionNode node)
            {
                base.DoConfigure(node);
                ConfigAttribute.Apply(this, node);
            }

            /// <summary>
            /// Warning: don't override this method in derived destinations, use
            /// DoFormatMessage() instead!
            /// </summary>
            protected override void DoWriteMessage(Message msg)
            {
                m_Writer.WriteLine(DoFormatMessage(msg));
                m_Writer.Flush();
            }

            /// <summary>
            /// Called when message is to be written to stream
            /// </summary>
            protected abstract string DoFormatMessage(Message msg);

        #endregion


    }
}
