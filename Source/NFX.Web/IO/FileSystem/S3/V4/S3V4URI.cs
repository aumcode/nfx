
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
 * Revision: NFX 1.0  3/11/2014 10:02:34 AM
 * Author: Denis Latushkin<dxwizard@gmail.com>
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using NFX;
using NFX.Environment;
using NFX.IO.FileSystem.S3.V4.S3V4Sign;

namespace NFX.IO.FileSystem.S3.V4
{
  public class S3V4URI
  {
    #region Static

      public static S3V4URI CreateFolder(string path)
      {
        S3V4URI uri = new S3V4URI(path.ToDirectoryPath());
        return uri;
      }

      public static S3V4URI CreateFile(string path)
      {
        S3V4URI uri = new S3V4URI(path);
        return uri;
      }

    #endregion

    #region .ctor

      public S3V4URI(string path)
      {
        m_Path = path;//.TrimEnd('/');
        m_Uri = new Uri(path);

        S3V4URLHelpers.Parse(m_Uri, out m_Bucket, out m_Region, out m_LocalPath, out m_QueryParams);

        m_LocalName = m_Uri.GetLocalName();
        m_ParentPath = m_Uri.GetParentURL();
      }

    #endregion

    #region Pvt/Prot/Int Fields

      private string m_Path;
      private Uri m_Uri;

      private string m_Bucket;
      private string m_Region;
      private string m_LocalPath;
      private string m_LocalName;
      private IDictionary<string, string> m_QueryParams;

      private string m_ParentPath;

    #endregion

    #region Properties

      public string Path { get { return m_Path; } }

      public string Bucket { get { return m_Bucket; } }

      public string Region { get { return m_Region; } }

      public string LocalPath { get { return m_LocalPath; } }

      public string LocalName { get { return m_LocalName; } }

      public IDictionary<string, string> QueryParams { get { return m_QueryParams; } }

      public string ParentPath { get { return m_ParentPath; } }

    #endregion

  } //S3V4URI

}
