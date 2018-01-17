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


// Author: Serge Aleynikov
// Date: 2013-07-01
using System;
using System.Text;

using NFX.Environment;

namespace NFX.Log.Destinations
{
    /// <summary>
    /// Provides a file storage destination implementation for debug and trace logging
    /// </summary>
    public sealed class DebugDestination : TextFileDestination
    {
      private const char   SEPARATOR = '|';
      private const string LOG_TIME_FORMAT_DFLT = "yyyyMMdd-HHmmss";

      public DebugDestination() : this(null) { }
      public DebugDestination(string name = null) : base(name)
      {
      }

      /// <summary>
      /// Specifies time format to be used for message logging
      /// </summary>
      [Config]
      public string LogTimeFormat { get; set; }

      protected override string DoFormatMessage(Message msg)
      {
        var output = new StringBuilder();
        var fmt  = LogTimeFormat;

        if (fmt.IsNullOrWhiteSpace())
          fmt = LOG_TIME_FORMAT_DFLT;

        string time;

        var now = msg.TimeStamp;
        try { time = now.ToString(fmt); }
        catch (Exception e)
        {
          time = now.ToString(LOG_TIME_FORMAT_DFLT) + " " + e.ToMessageWithType();
        }

        output.Append('@');
        output.Append(time);        output.Append(SEPARATOR);
        output.Append(msg.Guid);    output.Append(SEPARATOR);
        output.Append(msg.Host);    output.Append(SEPARATOR);
        output.Append(msg.Channel); output.Append(SEPARATOR);
        output.AppendLine(msg.RelatedTo == Guid.Empty ? string.Empty : msg.RelatedTo.ToString());

        output.Append(msg.Type); output.Append(SEPARATOR); output.Append(msg.Topic); output.Append(SEPARATOR); output.Append(msg.From); output.Append(SEPARATOR); output.AppendLine(msg.Source.ToString());
        output.Append(msg.Text); output.AppendLine();

        if (msg.Parameters.IsNotNullOrWhiteSpace()) output.AppendLine(msg.Parameters);

        dumpException(output, msg.Exception, 1);

        output.AppendLine();
        output.AppendLine();

        return output.ToString();
      }

      private void dumpException(StringBuilder output, Exception error, int level)
      {
        if (error==null) return;
        output.AppendLine();

        var sp = new string(' ', level * 2);
        output.Append(sp); output.AppendLine("+-Exception ");
        output.Append(sp); output.Append    ("| Type      "); output.AppendLine(error.GetType().FullName);
        output.Append(sp); output.Append    ("| Source    "); output.AppendLine(error.Source);
        output.Append(sp); output.Append    ("| Target    "); output.AppendLine(error.TargetSite.Name);
        output.Append(sp); output.Append    ("| Message   "); output.AppendLine(error.Message.Replace("\n", "\n       .    "+sp));
        output.Append(sp); output.AppendLine("| Stack     ");
                           output.AppendLine(sp+error.StackTrace.Replace("\n", "\n"+sp));

        dumpException(output, error.InnerException, level+1);
      }

    }
}
