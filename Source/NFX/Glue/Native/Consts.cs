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

using NFX.Serialization.Slim;

namespace NFX.Glue.Native
{
    /// <summary>
    /// Constants common to Native/Socket-based family of technologies
    /// </summary>
    internal class Consts
    {
        public const string SLIM_FORMAT = "slim";

        /// <summary>
        /// Size of the packet delimiting field
        /// </summary>
        public const int PACKET_DELIMITER_LENGTH    = sizeof(int);

        public const int DEFAULT_MAX_MSG_SIZE       = 512 * 1024;
        public const int DEFAULT_RCV_BUFFER_SIZE    = 16 * 1024;
        public const int DEFAULT_SND_BUFFER_SIZE    = 16 * 1024;


        public const int MAX_MSG_SIZE_LOW_BOUND     = 0xff;

        public const int DEFAULT_SERIALIZER_STREAM_CAPACITY = 32 * 1024;

        public const string CONFIG_MAX_MSG_SIZE_ATTR = "max-msg-size";

        public const string CONFIG_RCV_BUF_SIZE_ATTR = "rcv-buf-size";
        public const string CONFIG_SND_BUF_SIZE_ATTR = "snd-buf-size";

        public const string CONFIG_RCV_TIMEOUT_ATTR = "rcv-timeout";
        public const string CONFIG_SND_TIMEOUT_ATTR = "snd-timeout";

    }
}
