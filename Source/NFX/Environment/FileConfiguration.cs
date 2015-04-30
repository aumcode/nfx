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
 * Revision: NFX 1.0  2011.01.31
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX.Environment
{
  /// <summary>
  /// Provides file-based configuration base object used for concrete implementations such as XML or INI file base configurations
  /// </summary>
  [Serializable]
  public abstract class FileConfiguration : Configuration
  {
    #region .ctor

        /// <summary>
        /// Creates an instance of a new configuration not bound to any file
        /// </summary>
        protected FileConfiguration()
          : base()
        {

        }

        /// <summary>
        /// Creates an isntance of configuration and reads contents from the file
        /// </summary>
        protected FileConfiguration(string filename)
          : base()
        {
          m_FileName = filename;
        }

    #endregion

    #region Private Fields
        protected string m_FileName;

        private bool m_IsReadOnly;
    #endregion

    #region Properties
        public string FileName
        {
          get { return m_FileName; }
        }

        /// <summary>
        /// Indicates whether configuration is readonly or may be modified and saved
        /// </summary>
        public override bool IsReadOnly
        {
          get { return m_IsReadOnly; }
        }
    #endregion

    #region Public

        /// <summary>
        /// Saves configuration into specified file
        /// </summary>
        public virtual void SaveAs(string filename)
        {
          m_FileName = filename;

          if (m_Root != null)
            m_Root.ResetModified();
        }

        public void SetReadOnly(bool val)
        {
          m_IsReadOnly = val;
        }

    #endregion

    #region Protected

    #endregion

  }
}
