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


// Author: Serge Aleynikov
// Date: 2013-07-01
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NFX.Environment;
using System.Text.RegularExpressions;

namespace NFX.Log.Destinations
{
    /// <summary>
    /// Provides a file storage destination implementation for debug and trace logging
    /// </summary>
    public class DebugDestination : TextFileDestination
    {
        #region CONSTS

            private const char SEPARATOR = '|';

        #endregion

        #region .ctor

            public DebugDestination() : base(null, null, null) {}

            /// <summary>
            /// Creates a new instance of destination
            /// </summary>
            public DebugDestination(string name = null, string filename = null, string path = null)
                : base(name, filename, path)
            {
            }

        #endregion

        #region Protected

            protected override string DoFormatMessage(Message msg)
            {
                StringBuilder line = new StringBuilder();

                DateTime now = msg.TimeStamp;
                line.Append(now.ToString(LogTimeFormat)); line.Append(SEPARATOR);
                line.Append(msg.RelatedTo == Guid.Empty ? string.Empty : msg.RelatedTo.ToString());
                line.Append(SEPARATOR);
                line.Append(msg.Type.ToString()); line.Append(SEPARATOR);
                line.Append(msg.Text); line.Append(SEPARATOR);
                if (!msg.Topic.Equals(msg.Type)) line.Append(msg.Topic);
                line.Append(SEPARATOR);
                line.Append(msg.Source.ToString()); line.Append(SEPARATOR);
                line.Append(msg.From);

                if (msg.Exception != null)
                {
                    line.Append(SEPARATOR);
                    line.AppendFormat("\n  --- Exception in {0} ---\n", msg.Exception.TargetSite);
                    line.Append(msg.Exception.StackTrace);
                }

                return line.ToString();
            }

        #endregion
    }
}
