
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
 * Revision: NFX 1.0  3/13/2014 4:42:58 PM
 * Author: Denis Latushkin<dxwizard@gmail.com>
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Xml.Linq;

using NFX;
using NFX.Environment;


namespace NFX.IO.FileSystem.S3.V4
{
  internal class S3V4ListBucketResult
  {
    #region Static

      public static S3V4ListBucketResult FromXML(string xml)
      {
        S3V4ListBucketResult result = new S3V4ListBucketResult();

        result.AddXML(xml);

        return result;
      }

    #endregion

    #region .ctor

      private S3V4ListBucketResult() {}

    #endregion

    #region Pvt/Prot/Int Fields

      private bool m_IsTruncated;
      private List<S3V4ListBucketItem> m_Items = new List<S3V4ListBucketItem>();

    #endregion

    #region Properties

      public bool IsTruncated { get { return m_IsTruncated; } }

      public List<S3V4ListBucketItem> Items { get { return m_Items; } }

    #endregion

    #region Public

      public void AddXML(string xml)
      {
        XDocument xdoc = XDocument.Parse(xml);

        XElement resultRoot = xdoc.Elements().First(e => e.Name.LocalName == "ListBucketResult");

        string strIsTruncated = resultRoot.Elements().First(e => e.Name.LocalName == "IsTruncated").Value;
        m_IsTruncated = bool.Parse(strIsTruncated);

        string prefix = resultRoot.Elements().First(e => e.Name.LocalName == "Prefix").Value;

        List<S3V4ListBucketItem> items = xdoc.Descendants().Where(x => x.Name.LocalName == "Contents")
          .Select(c => S3V4ListBucketItem.FromXElement(c, prefix))
          .Where(i => !string.IsNullOrWhiteSpace(i.Key) && i.Key != prefix) // skip containig folder
          .ToList();

        m_Items.AddRange(items);
      }

    #endregion

  } //S3V4ListBucketResult

  internal class S3V4ListBucketItem
  {
    #region Static

      public static S3V4ListBucketItem FromXElement(XElement contentElement, string prefix)
      {
        string key = null;
        DateTime lastModified = DateTime.MinValue;
        string eTag = null;
        ulong size = 0;
        bool isFolder = false;
        string itemName = null;
        bool isNested = false;

        foreach (XElement e in contentElement.Elements())
        {
          if (e.Name.LocalName == "Key")
          {
            key = e.Value.Substring(prefix.Length);
            isFolder = key.EndsWith("/");
            isNested = isKeyNested( key);
            itemName = key.TrimEnd('/').Split('/').Last();
          }
          else if (e.Name.LocalName == "ETag")
            eTag = e.Value;
          else if (e.Name.LocalName == "LastModified")
            lastModified = DateTime.Parse(e.Value);
          else if (e.Name.LocalName == "Size")
            size = ulong.Parse(e.Value);
        }

        S3V4ListBucketItem item = new S3V4ListBucketItem()
        {
          Key = key,
          ItemName = itemName,
          LastModified = lastModified,
          ETag = eTag,
          Size = size,
          IsFolder = isFolder,
          IsNested = isNested
        };

        return item;
      }

    #endregion

    #region ctor

      private S3V4ListBucketItem() { }

    #endregion

    #region Protected

      public override string ToString()
      {
        if (IsFolder)
          return string.Format("Folder name=\"{0}\", key=\"{1}\", date=\"{2}\"", ItemName, Key, LastModified);
        else
          return string.Format("File name=\"{0}\", key=\"{1}\", date=\"{2}\", size=\"{3}\"", ItemName, Key, LastModified, Size);
      }

    #endregion

    #region Properties

      public string Key { get; private set; }
      public DateTime LastModified { get; private set; }
      public string ETag { get; private set; }
      public ulong Size { get; private set; }
      public bool IsFolder { get; private set; }

      public string ItemName { get; private set; }

      public bool IsNested { get; private set; }

    #endregion

    #region .pvt. impl.

      private static bool isKeyNested(string key)
      {
        if (string.IsNullOrWhiteSpace(key))
          return false;

        int separatorPos = key.IndexOf('/');

        bool isNested = separatorPos != -1 && separatorPos != key.Length-1;

        return isNested;
      }

    #endregion

  } //S3V4ListBucketItem

}
