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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 0.3  2009.10.12
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NFX.Environment;

namespace NFX.Log.Destinations
{
  /// <summary>
  /// Provides a CSV file storage log destination implementation
  /// </summary>
  public class CSVFileDestination : TextFileDestination
  {
      #region CONSTS
          public const string NAME_TIME_FORMAT              = "yyyyMMdd";
          public const string DEFAULT_EXTENSION             = ".csv.log";
          public const string DEFAULT_LOG_TIME_FORMAT       = "yyyyMMdd-HHmmss";

          public const string CONFIG_NAMETIMEFORMAT_ATTR    = "name-time-format";
          public const string CONFIG_FILEEXTENSION_ATTR     = "file-extension";
      #endregion

      #region .ctor

          /// <summary>
          /// Creates a new instance of destination that stores log in CSV files
          /// </summary>
          public CSVFileDestination() {}

          /// <summary>
          /// Creates a new instance of destination that stores log in CSV files
          /// </summary>
          public CSVFileDestination(string name, string path) : base(name, null, path) {}

      #endregion

      #region Fields

          private string m_Extension = DEFAULT_EXTENSION;

      #endregion

      #region Public/Props

          [Config("$" + CONFIG_NAMETIMEFORMAT_ATTR)]
          public string NameTimeFormat { get; set; }

          [Config("$" + CONFIG_FILEEXTENSION_ATTR, DEFAULT_EXTENSION)]
          public string FileExtension { get { return m_Extension; } set { m_Extension = value ?? DEFAULT_EXTENSION; } }

          public static string MessageToCSVLine(Message msg, string logTimeFormat = null)
          {
              StringBuilder line = new StringBuilder();

              line.Append(msg.Guid); line.Append(',');
              line.Append(msg.RelatedTo == Guid.Empty ? string.Empty : msg.RelatedTo.ToString()); line.Append(',');
              line.Append(msg.Type.ToString()); line.Append(',');
              line.Append(msg.Source.ToString()); line.Append(',');
              line.Append(msg.TimeStamp.ToString(logTimeFormat ?? DEFAULT_LOG_TIME_FORMAT)); line.Append(',');

              line.Append(escape(msg.Host)); line.Append(',');
              line.Append(escape(msg.From)); line.Append(',');
              line.Append(escape(msg.Topic)); line.Append(',');
              line.Append(escape(msg.Text)); line.Append(',');
              line.Append(escape(msg.Parameters)); line.Append(',');
              if (msg.Exception != null)
                  line.Append(escape(msg.Exception.GetType().FullName + "::" + msg.Exception.Message));

              return line.ToString();
          }

      #endregion

      #region Protected

          protected override void DoConfigure(IConfigSectionNode node)
          {
              // Set up the default for backward compatibility
              LogTimeFormat = DEFAULT_LOG_TIME_FORMAT;

              base.DoConfigure(node);
              ConfigAttribute.Apply(this, node);

              // Code below this line is added for backward compatibility

              if (!string.IsNullOrEmpty(Filename))
                  return;

              Filename = System.IO.Path.Combine(m_Path, DefaultFileName);
          }

          /// <summary>
          /// This is for backward compatibility
          /// </summary>
          protected override string DefaultFileName
          {
              get
              {
                  return "$(~name){0}$(~extension)".Args(
                      NameTimeFormat.IsNullOrEmpty() ? string.Empty : "-$(::now fmt=" + NameTimeFormat + ")");
              }
          }

          protected override string DefaultExtension { get { return m_Extension; } }

          /// <summary>
          /// This is for backward compatibility
          /// </summary>
          protected override string DefaultLogTimeFormat { get { return DEFAULT_LOG_TIME_FORMAT; } }

          /// <summary>
          /// Spools instance data in CSV format for storage in a file destination
          /// </summary>
          protected override string DoFormatMessage(Message msg)
          {
              return MessageToCSVLine(msg, LogTimeFormat);
          }

      #endregion


      #region Private

          private static string escape(string str)
          {
              bool needsQuotes = str.IndexOfAny(new char[] { ' ', ',', '\n', '\r', '"' }) >= 0;

              str = str.Replace("\n", @"\n");
              str = str.Replace("\r", @"\r");
              str = str.Replace("\"", "\"\"");

              return needsQuotes ? "\"" + str + "\"" : str;
          }

      #endregion

  }
}
