/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
    private const string DEF_LOG_TIME_FORMAT = "HH:mm:ss";


    public ConsoleDestination() : this(null) { }
    public ConsoleDestination(string name) : base (name) { }

    [Config]
    public bool Colored { get; set;}

    /// <summary>
    /// Time format for log line entries
    /// </summary>
    [Config]
    public string LogTimeFormat{ get; set;}


    protected internal override void DoSend(Message msg)
    {
      var txt = fmt(msg);

      if (Colored)
      {
        if (msg.Type<MessageType.Warning) ConsoleUtils.Info(txt);
        else
        if (msg.Type<MessageType.Error) ConsoleUtils.Warning(txt);
        else
          ConsoleUtils.Error(txt);
      }
      else
        Console.WriteLine(txt);
    }

    private string fmt(Message msg)
    {
      var tf = LogTimeFormat;
      if (tf.IsNullOrWhiteSpace()) tf = DEF_LOG_TIME_FORMAT;

      return string.Format("{0}|{1}|{2}|{3}| {4}",
                    msg.TimeStamp.ToString(tf),
                    msg.Type,
                    msg.Source,
                    msg.From,
                    msg.Text);
    }

  }
}
