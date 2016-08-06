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
using NFX.IO;

namespace NFX.Log.Destinations
{
    /// <summary>
    /// Logs messages in stdio.console
    /// </summary>
    public class ConsoleDestination : Destination
    {
        #region CONSTS
            private const string DEF_LOG_TIME_FORMAT = "HH:mm:ss";
        #endregion

        #region .ctor

        public ConsoleDestination ()
            {
            }

            public ConsoleDestination (string name) : base (name)
            {

            }

            protected override void Destructor()
            {
              base.Destructor();
            }

       #endregion

       #region Pvt/Protected Fields

            private bool m_Colored;
            private string m_LogTimeFormat = DEF_LOG_TIME_FORMAT;

       #endregion


       #region Properties

           [Config("$colored")]
           public bool Colored
           {
               get { return m_Colored; }
               set { m_Colored = value; }
           }

           /// <summary>
           /// Time format for log line entries
           /// </summary>
           [Config("$" + FileDestination.CONFIG_LOGTIMEFORMAT_ATTR)]
           public string LogTimeFormat
           {
               get { return m_LogTimeFormat; }
               set
               {
                   m_LogTimeFormat = string.IsNullOrWhiteSpace(value) ? DEF_LOG_TIME_FORMAT : value;
               }
           }

       #endregion

      #region Protected /.pvt

            protected internal override void DoSend(Message msg)
            {
                var txt = fmt(msg);

                if (m_Colored)
                {
                    if (msg.Type<MessageType.Warning) ConsoleUtils.Info(txt);
                    else
                    if (msg.Type<MessageType.Error) ConsoleUtils.Warning(txt);
                    else
                      ConsoleUtils.Error(txt);
                }
                else
                {
                    Console.WriteLine(txt);
                }
            }

            private string fmt(Message msg)
            {
               return string.Format("{0}|{1}|{2}|{3}| {4}",
                              msg.TimeStamp.ToString(m_LogTimeFormat),
                              msg.Type,
                              msg.Source,
                              msg.From,
                              msg.Text);
            }
      #endregion

    }
}
