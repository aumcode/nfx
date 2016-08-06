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
using System.Reflection;
using System.IO;
using System.Globalization;

namespace NFX.Environment
{
  /// <summary>
  /// Retrieves build information encapsulated into a module in the form of an embedded resource
  /// </summary>
  [Serializable]
  public class BuildInformation
  {
    #region  CONSTS
        public const string FRAMEWORK_BUILD_INFO_PATH = "NFX."+BUILD_INFO_RESOURCE;

        public const string BUILD_INFO_RESOURCE = "BUILD_INFO.txt";

    #endregion


    #region .ctor

        /// <summary>
        /// Creates an instance of BuildInformation class for framework
        /// </summary>
        private BuildInformation() : this(null) { }


        /// <summary>
        /// Creates and instance of BuildInformation class from the specified resource path in particular assembly.
        /// If assembly is null then BuildInformation for the whole framework is returned.
        /// If Path is null then the first found BUILD info resource is used from the specified assembly
        /// </summary>
        public BuildInformation(Assembly assembly, string path = null, bool throwError = true)
        {

          if (assembly == null)
          {
            assembly = Assembly.GetExecutingAssembly();
            path = FRAMEWORK_BUILD_INFO_PATH;
          }

          if (path.IsNullOrWhiteSpace())
          {
            path = assembly.GetManifestResourceNames().FirstOrDefault(n=>n.EndsWith(BUILD_INFO_RESOURCE));
            if (path.IsNullOrWhiteSpace())
                path = FRAMEWORK_BUILD_INFO_PATH;
          }

          try
          {
            load(assembly, path);
          }
          catch (Exception error)
          {
            if (throwError)
                throw new ConfigException(StringConsts.BUILD_INFO_READ_ERROR + error.Message, error);
          }
        }


    #endregion

    #region Fields
      private static BuildInformation m_ForFramework;

      private string m_AssemblyName;
      private int m_BuildSeed;
      private string m_Computer;
      private string m_User;
      private string m_OS;
      private string m_OSVer;
      private DateTime m_DateStampUTC;
    #endregion

    #region Properties

        /// <summary>
        /// Return framework build information
        /// </summary>
        public static BuildInformation ForFramework
        {
          get
          {
              if (m_ForFramework == null)
                m_ForFramework = new BuildInformation();

            return m_ForFramework;
          }
        }



        /// <summary>
        /// Rertuns assembly name that this information was obtained from
        /// </summary>
        public string AssemblyName { get { return m_AssemblyName; } }

        /// <summary>
        /// Returns random number assigned to a build. It is NOT guaranteed to be unique
        /// </summary>
        public int BuildSeed { get { return m_BuildSeed; } }

        /// <summary>
        /// A name of the computer that performed build
        /// </summary>
        public string Computer { get { return m_Computer ?? string.Empty; } }

        /// <summary>
        /// a name of user that build session was logged under
        /// </summary>
        public string User { get { return m_User ?? string.Empty; } }

        /// <summary>
        /// OS name
        /// </summary>
        public string OS { get { return m_OS ?? string.Empty; } }

        /// <summary>
        /// OS version name
        /// </summary>
        public string OSVer { get { return m_OSVer ?? string.Empty; } }

        /// <summary>
        /// Date and time stamp when build was performed
        /// </summary>
        public DateTime DateStampUTC { get { return m_DateStampUTC; } }
    #endregion

    #region Public

        public override string ToString()
        {
            return "[{0}] by {1}@{2}:{3} on {4} UTC".Args(BuildSeed, User, Computer, OS, DateStampUTC);
        }

    #endregion


    #region .pvt .impl

        private void load(Assembly asmb, string path)
        {
           m_AssemblyName = asmb.FullName;

           List<string> lines = new List<string>();

           using (Stream strm = asmb.GetManifestResourceStream(path))
              using (StreamReader reader = new StreamReader(strm))
                while (!reader.EndOfStream)
                  lines.Add(reader.ReadLine());



           m_BuildSeed = int.Parse(val(lines[0]));
           m_Computer = val(lines[1]).Trim();
           m_User = val(lines[2]).Trim();
           m_OS = val(lines[3]).Trim();
           m_OSVer = val(lines[4]).Trim();
           m_DateStampUTC = DateTime.Parse(val(lines[5]), null, DateTimeStyles.RoundtripKind);
        }

        private string val(string line)
        {
          if (string.IsNullOrEmpty(line)) return string.Empty;

          int i = line.IndexOf('=');

          if (i < 0) return line;

          return line.Substring(i + 1);
        }

    #endregion
  }
}
