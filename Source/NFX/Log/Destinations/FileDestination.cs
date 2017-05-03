/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
        private  const string DEF_FILENAME = "{0:yyyyMMdd}.log";
      #endregion

      #region .ctor
        protected FileDestination(string name) : base(name)
        {
        }
      #endregion


      #region Pvt Fields

        protected string        m_Path;
        protected string        m_FileName;

        protected FileStream    m_Stream;
        private bool            m_Recreate;

      #endregion

      #region Properties
        /// <summary>
        /// The name of the file without path may use {0} for date: {0:yyyyMMdd}-$($name).csv.log
        /// </summary>
        [Config]
        public virtual string FileName
        {
            get { return m_FileName; }
            set
            {
              if (m_FileName==value) return;
              m_FileName = value;
              m_Recreate = true;
            }
        }

        /// <summary>
        /// Directory where file should be created. Will create the directory chane if it doesn't exist
        /// </summary>
        [Config]
        public virtual string Path
        {
            get { return m_Path; }
            set
            {
              if (m_Path==value) return;
              m_Path = value;
              m_Recreate = true;
            }
        }

      #endregion

      #region Public

        public override void Open()
        {
            base.Open();
            ensureStream();
        }

        public override void Close()
        {
            base.Close();
            closeStream();
        }

      #endregion


      #region Protected

        private DateTime m_PrevDate;
        protected internal override void DoPulse()
        {
          base.DoPulse();

          if (m_Stream==null) return;

          var utcNow = DateTime.UtcNow;
          if ((utcNow - m_PrevDate).TotalSeconds < 10) return;
          m_PrevDate = utcNow;

          if (m_Stream.Name != GetDestinationFileName())
          {
            m_Recreate = true;
            ensureStream();
          }
        }


        /// <summary>
        /// Called after output stream has been opened
        /// </summary>
        protected abstract void DoOpenStream();

        /// <summary>
        /// Called just before output stream is closed
        /// </summary>
        protected abstract void DoCloseStream();

        /// <summary>
        /// Called when message is to be written to stream
        /// </summary>
        protected abstract void DoWriteMessage(Message msg);

        /// <summary>
        /// Override DoFormatMessage() instead
        /// </summary>
        protected internal sealed override void DoSend(Message msg)
        {
          ensureStream();
          DoWriteMessage(msg);
        }

        protected virtual string DefaultFileName  { get { return DEF_FILENAME;  } }


        protected virtual string GetDestinationFileName()
        {
          var path = m_Path;
          if (path.IsNotNullOrWhiteSpace())
            try
            {
              path = path.Args( this.Service.LocalizedTime );
            }
            catch(Exception error)
            {
              throw new NFXException(StringConsts.LOGSVC_FILE_DESTINATION_PATH_ERROR.Args(Name, path, error.ToMessageWithType()), error);
            }

          var fn = m_FileName;

          if (fn.IsNullOrWhiteSpace())
             fn = Name;

          if (fn.IsNullOrWhiteSpace())
             fn = DefaultFileName;

          try
          {
            fn = fn.Args( this.Service.LocalizedTime );
          }
          catch(Exception error)
          {
            throw new NFXException(StringConsts.LOGSVC_FILE_DESTINATION_FILENAME_ERROR.Args(Name, fn, error.ToMessageWithType()), error);
          }

          if (path.IsNotNullOrWhiteSpace() && !Directory.Exists(path))
             IOMiscUtils.EnsureAccessibleDirectory(path);

          var result = path.IsNullOrWhiteSpace() ? fn : System.IO.Path.Combine(path, fn);
          return result;
        }

      #endregion


      #region Private

        private void ensureStream()
        {
          if (m_Stream!=null && !m_Recreate) return;
          closeStream();
          m_Recreate = false;
          var fn     = GetDestinationFileName();

          m_Stream = new FileStream(fn, FileMode.Append, FileAccess.Write, FileShare.Read);
          DoOpenStream();
        }

        private void closeStream()
        {
          DoCloseStream();
          DisposableObject.DisposeAndNull(ref m_Stream);
        }

      #endregion

    }
}
