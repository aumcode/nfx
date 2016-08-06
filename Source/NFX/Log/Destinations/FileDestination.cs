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


// Author: Serge Aleynikov <saleyn@gmail.com>
// Date:   2013-07-23
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Environment;


namespace NFX.Log.Destinations
{
    /// <summary>
    /// Provides a file storage destination implementation
    /// </summary>
    public abstract class FileDestination : Destination
    {
        #region CONSTS

            private  const string DEF_FILENAME              = "$(::now fmt=yyyyMMdd)-$(~name)$(~extension)";
            private  const string DEF_EXTENSION             = ".log";
            private  const string DEF_LOG_TIME_FORMAT       = "yyyyMMdd-HHmmss.ffffff";

            private  const string CONFIG_CREATE_DIR_ATTR    = "create-dir";
            private  const string CONFIG_FILENAME_ATTR      = "filename";
            public   const string CONFIG_PATH_ATTR          = "path";

            internal const string CONFIG_LOGTIMEFORMAT_ATTR = "log-time-format";

        #endregion

        #region .ctor

            /// <summary>
            /// Creates a new instance of destination
            /// </summary>
            public FileDestination(string name = null) : base(name)
            {
                CreateDir = true;
            }

            /// <summary>
            /// Creates a new instance of destination
            /// </summary>
            public FileDestination(string name, string filename, string path = null) : this(name)
            {
                m_Filename = filename ?? string.Empty;
                m_Path     = path ?? string.Empty;
            }

        #endregion


        #region Pvt Fields

            protected string        m_Path;
            protected string        m_Filename;
            private string          m_LogTimeFormat = DEF_LOG_TIME_FORMAT;

            private FileStream      m_Stream;

            private bool            m_Recreate = false;

        #endregion

        #region Properties

            /// <summary>
            /// File path and name. It may include environment variables (e.g. ~home),
            /// and "$(~home)$(@::now fmt=yyyyMMdd utc=false)-$($name).log" to specify current time.
            /// "utc=Bool" argument to the now function of time controls if the now time in filename
            /// should be in UTC/local time
            /// </summary>
            [Config("$" + CONFIG_FILENAME_ATTR)]
            public virtual string Filename
            {
                get { return m_Filename; }
                set
                {
                    var s = string.IsNullOrWhiteSpace(value) ? DefaultFileName : value;
                    if (m_Filename != s) { m_Filename = s; m_Recreate = true; }
                }
            }

            [Config("$" + CONFIG_PATH_ATTR)]
            public virtual string Path
            {
                get { return m_Filename; }
                set { m_Path = value ?? string.Empty; }
            }

            /// <summary>
            /// Indicates whether to create the directory to be written to if one doesn't exist
            /// </summary>
            [Config("$" + CONFIG_CREATE_DIR_ATTR, true)]
            public bool CreateDir { get; set; }

            /// <summary>
            /// Time format for log line entries
            /// </summary>
            [Config("$" + CONFIG_LOGTIMEFORMAT_ATTR)]
            public string LogTimeFormat
            {
                get { return m_LogTimeFormat; }
                set { m_LogTimeFormat = string.IsNullOrWhiteSpace(value) ? DefaultLogTimeFormat : value; }
            }

            protected FileStream Stream { get { return m_Stream; } }

        #endregion

        #region Public

            public override void TimeChanged()
            {
                m_Recreate = true;
            }

            public override void SettingsChanged()
            {
                m_Recreate = true;
            }

            public override void Open()
            {
                base.Open();
                if (m_Stream==null)
                    openStream();
            }

            public override void Close()
            {
                base.Close();
                closeStream();
            }

        #endregion


        #region Protected

            protected override void DoConfigure(IConfigSectionNode node)
            {
                base.DoConfigure(node);
                ConfigAttribute.Apply(this, node);
            }

            /// <summary>
            /// Called after output stream has been opened
            /// </summary>
            protected abstract void DoOnOpenStream();

            /// <summary>
            /// Called just before output stream is closed
            /// </summary>
            protected abstract void DoOnCloseStream();

            /// <summary>
            /// Called when message is to be written to stream
            /// </summary>
            protected abstract void DoWriteMessage(Message msg);

            /// <summary>
            /// Warning: don't override this method in derived destinations, use
            /// DoFormatMessage() instead!
            /// </summary>
            protected internal override void DoSend(Message msg)
            {
                if (m_Stream == null)
                    openStream();

                DoWriteMessage(msg);

                if (m_Recreate)
                {
                    closeStream();
                    m_Recreate = false;
                }
            }

            /// <summary>
            /// For backward compatibility
            /// </summary>
            protected virtual string DefaultFileName { get { return DEF_FILENAME; } }

            /// <summary>
            /// For backward compatibility
            /// </summary>
            protected virtual string DefaultExtension { get { return DEF_EXTENSION; } }

            /// <summary>
            /// For backward compatibility
            /// </summary>
            protected virtual string DefaultLogTimeFormat { get { return DEF_LOG_TIME_FORMAT; } }

        #endregion


        #region Private

            private void openStream()
            {
                if (m_Filename.IsNullOrEmpty())
                    Filename = string.IsNullOrEmpty(Name) ? string.Empty : DefaultFileName;

                var fn =  m_Filename.EvaluateVars(new Vars { { "name", Name }, { "extension", DefaultExtension } });

                m_Filename = (!m_Path.IsNullOrEmpty() && System.IO.Path.GetDirectoryName(fn).IsNullOrEmpty())
                           ? System.IO.Path.Combine(m_Path, fn)
                           : fn;

                if (CreateDir)
                {
                    var path = System.IO.Path.GetDirectoryName(m_Filename);
                    if (path != string.Empty && !Directory.Exists(path))
                        IOMiscUtils.EnsureAccessibleDirectory(path);
                }

                m_Stream = new FileStream(m_Filename, FileMode.Append, FileAccess.Write, FileShare.Read);
                DoOnOpenStream();
                m_Recreate = false;
            }

            private void closeStream()
            {
                if (m_Stream != null)
                {
                    DoOnCloseStream();
                    m_Stream.Dispose();

                    m_Stream = null;
                }
            }

        #endregion

    }
}
