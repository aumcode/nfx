
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
 * Revision: NFX 1.0  3/19/2014 6:13:35 PM
 * Author: Denis Latushkin<dxwizard@gmail.com>
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace NFX.IO.FileSystem.SVN
{
  /// <summary>
  /// Facilitates read-only tasks with WebDAV compliant SVN server. This class is NOT thread-safe
  /// </summary>
  public sealed class WebDAV
  {
    #region CONSTS

      private const string LIST_CONTENT_BODY =
        "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
        "<propfind xmlns=\"DAV:\"> xmlns:in=\"http://sapportals.com/xmlns/cm/webdav\"" +
        //"<allprop/>" +
        "  <prop>" +
        "    <getlastmodified/>" +
        "    <creationdate/>" +
        "    <version-name/>" +
        "    <getcontentlength/>" +
        "    <getcontenttype/>" +
        "  </prop>" +
        "</propfind>";

      private static byte[] LIST_CONTENT_BODY_BYTES = Encoding.UTF8.GetBytes(LIST_CONTENT_BODY);

      private const string LIST_CONTENT_METHOD = "PROPFIND";
      private const int HTTP_STATUS_CODE_MULTISTATUS = 207;

      private const string REPORT_CONTENT_BODY =
        "<?xml version=\"1.0\"?>" +
        "<S:log-report xmlns:S=\"svn:\">" +
        "<S:start-revision>1</S:start-revision>" +
        "<S:date/>" +
        //"<S:discover-changed-paths/>"
        "</S:log-report>";

      private static byte[] REPORT_CONTENT_BODY_BYTES = Encoding.UTF8.GetBytes(REPORT_CONTENT_BODY);

      private const string REPORT_METHOD = "REPORT";

    #endregion                                                                                          i

    #region Inner Types

      /// <summary>
      /// General ancestor for items in remote Dav catalog, such as files and directories
      /// </summary>
      public abstract class Item: INamed, IEquatable<Item>
      {
        internal static char[] SEPARATORS = new [] {'/', '\\'};

        internal Item(WebDAV client, string name, string version, DateTime creationDate, DateTime lastModificationDate,
          string contentType, Directory parent)
        {
          m_Client = client;
          m_Name = name ?? CoreConsts.UNKNOWN;
          m_Version = version ?? CoreConsts.UNKNOWN;
          m_CreationDate = creationDate;
          m_LastModificationDate = lastModificationDate;
          m_ContentType = contentType ?? CoreConsts.UNKNOWN;

          m_Parent = parent;
        }

        private WebDAV m_Client;
        private string m_Name;
        private Directory m_Parent;

        private string m_Version;
        private DateTime m_CreationDate;
        private DateTime m_LastModificationDate;

        private string m_ContentType;

        public WebDAV Client { get { return m_Client;}}
        public string Name { get { return m_Name;}}

        /// <summary>
        /// Returns path relative to root directory
        /// </summary>
        public string Path { get { return m_Parent == null ? "/" : "{0}/{1}".Args(m_Parent.Path.TrimEnd('/'), m_Name.TrimStart('/'));} }

        /// <summary>
        /// Returns absolute path for this item
        /// </summary>
        public string AbsolutePath { get { return m_Client.GetAbsoluteItemPath(this); } }

        /// <summary>
        /// Returns parent directory that contains this item or NULL for very root directory
        /// </summary>
        public Directory Parent { get { return m_Parent;} }

        public string Version { get { return m_Version;} }

        public DateTime CreationDate { get { return m_CreationDate;} }

        public DateTime LastModificationDate { get { return m_LastModificationDate;} }

        /// <summary>
        /// Returns content type (e.g. 'text/plainutf-8')
        /// </summary>
        public string ContentType { get { return m_ContentType; } }

        /// <summary>
        /// Performs case-sensitive name equality test
        /// </summary>
        public bool IsSameName(string name)
        {
          return string.Equals(m_Name, name);
        }

        public override string ToString()
        {
          return Path;
        }

        public override int GetHashCode()
        {
          return Path.GetHashCode();
        }

        public override bool Equals(object obj)
        {
          Item other = obj as Item;
          if (other == null) return false;

          return Equals(other);
        }

        public bool Equals(Item other)
        {
          if (other == null) return false;

          return Path.Equals(other.Path);
        }
      }

      /// <summary>
      /// Represents a directory in remote Dav catalog
      /// </summary>
      public sealed class Directory: Item
      {
        internal Directory(WebDAV client, string name, string version, DateTime creationDate, DateTime lastModificationDate, string contentType,
          Directory parent)
          : base(client, name, version, creationDate, lastModificationDate, contentType, parent) {}

        private List<Item> m_Children;

        /// <summary>
        /// Lists subitems contained in this remote Dav directory. Items are fetched lazily and then cached until Refresh() is called
        /// </summary>
        public IEnumerable<Item> Children
        {
          get
          {
            if (m_Children == null)
            {
              m_Children = Client.listDirectoryContent(this);
            }
            return m_Children;
          }
        }

        /// <summary>
        /// Lists subdirectories of this directory
        /// </summary>
        public IEnumerable<Item> Directories
        {
          get
          {
            return Children.Where(c => c is Directory);
          }
        }

        /// <summary>
        /// Lists files contained in this directory
        /// </summary>
        public IEnumerable<Item> Files
        {
          get
          {
            return Children.Where(c => c is File);
          }
        }

        /// <summary>
        /// Gets an item contained in this directory by name be it file or directory. Returns NULL if item with such name does not exist
        /// </summary>
        public Item this[string name] { get { return Children.FirstOrDefault(c => c.IsSameName(name));} }

        /// <summary>
        /// Tries to navigate path and returns destination directory, file or null if its not found
        /// </summary>
        public Item NavigatePath(string path)
        {
          if (path.IsNullOrWhiteSpace()) return this;

          var segs = path.Split( SEPARATORS, StringSplitOptions.RemoveEmptyEntries);
          Directory item = this;
          for (int i = 0; i < segs.Length; i++)
          {
            string seg = segs[i];

            Directory dir = item[seg] as Directory;
            if (dir != null)
            {
              item = dir;
              continue;
            }

            if (i == segs.Length-1) // last segment
            {
              File file = item[seg] as File;
              return file;
            }
            else
              return null;
          }

          return item;
        }

        /// <summary>
        /// Refreshes the internal status of the directory by purging all cached objects
        /// </summary>
        public void Refresh()
        {
          m_Children = null;
        }
      }

    //todo: 20140723 dlat + dkh: does WebDav return file size in xml?

      /// <summary>
      /// Represents a file in the remote Dav catalog
      /// </summary>
      public sealed class File: Item
      {
        internal File(WebDAV client, string name, ulong size, string version, DateTime creationDate, DateTime lastModificationDate, string contentType,
          Directory parent)
          : base(client, name, version, creationDate, lastModificationDate, contentType, parent)
        {
          m_Size = size;
        }

        private ulong m_Size;

        /// <summary>
        /// Returns 'ContentLength' of this file
        /// </summary>
        public ulong Size { get { return m_Size; } }

        /// <summary>
        /// Writes contents of the remote file into a stream. No caching is done
        /// </summary>
        public void GetContent(Stream stream)
        {
          Client.getFileContent(this, stream);
        }
      }

      /// <summary>
      /// Represents a version of items in the remote Dav catalog
      /// </summary>
      public sealed class Version: IFileSystemVersion
      {
        public Version(string name, string comment, string creator, DateTime date)
	      {
          m_Name = name ?? CoreConsts.UNKNOWN;
          m_Comment = comment;
          m_Creator = creator;
          m_Date = date;
	      }

        private string m_Name;
        private string m_Comment;
        private string m_Creator;
        private DateTime m_Date;

        public string Name { get { return m_Name; } }
        public string Comment { get { return m_Comment ?? string.Empty; } }
        public string Creator { get { return m_Creator ?? string.Empty; } }
        public DateTime Date { get { return m_Date; } }

        public override string ToString()
        {
 	         return "{0} from {1}".Args(m_Name, m_Date);
        }

        public override int GetHashCode()
        {
 	         return m_Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
          Version other = obj as Version;
          if (other == null)
            return false;
          else
            return m_Name == other.m_Name;
        }
      }

    #endregion

    #region static

      /// <summary>
      /// Retrieves a list of versions contained in the remote Dav catalog
      /// </summary>
      public static IEnumerable<Version> GetVersions(string rootURL, string uName, string uPwd)
      {
        if (rootURL.IsNullOrWhiteSpace())
          throw new NFXException(NFX.Web.StringConsts.ARGUMENT_ERROR + typeof(WebDAV).Name + ".GetVersions(rootURL == null|empty)");

        NFX.Web.WebSettings.RequireInitializedSettings();

        return doListVersions(rootURL, uName, uPwd);
      }

    #endregion

    #region .ctor

      public WebDAV(string rootURL, int timeoutMs = 0, string uName = null, string uPwd = null, Version version = null)
      {
        NFX.Web.WebSettings.RequireInitializedSettings();

        if (rootURL.IsNullOrWhiteSpace())
          throw new NFXException(NFX.Web.StringConsts.ARGUMENT_ERROR + this.GetType().Name + ".ctor(path == null|empty)");

        m_RootUri = new Uri(rootURL);

        m_TimeoutMs = timeoutMs;

        m_UName = uName;
        m_UPwd = uPwd;
        m_CurrentVersion = version;
      }

    #endregion

    #region Pvt/Prot/Int Fields

      private Uri m_RootUri;

      private Directory m_Root;

      private string m_UName;
      private string m_UPwd;

      private int m_TimeoutMs;

      private Version m_CurrentVersion;

    #endregion

    #region Properties

      /// <summary>
      /// The top-most URL of the remote repository
      /// </summary>
      public Uri RootUri { get { return m_RootUri; } }

      /// <summary>
      /// Returns top-level directory as of root Uri mount point
      /// </summary>
      public Directory Root
      {
        get
        {
          if (m_Root == null)
            m_Root = (Directory)listDirectoryContent(depth: 0).First();

          return m_Root;
        }
      }

      /// <summary>
      /// Gets current user name or empty when user is not set
      /// </summary>
      public string UName { get { return m_UName ?? string.Empty; } }

      /// <summary>
      /// Gets current user password or empty when user is not set
      /// </summary>
      public string UPwd { get { return m_UPwd ?? string.Empty; } }

      /// <summary>
      /// Gets timeout for call, if zero then actual timeout is taken from WebSettings
      /// </summary>
      public int TimeoutMs { get { return m_TimeoutMs; } }

      /// <summary>
      /// Get or sets current version
      /// </summary>
      public Version CurrentVersion
      {
        get { return m_CurrentVersion; }
        set
        {
          if (m_CurrentVersion != null && value != null && m_CurrentVersion.Equals(value)) return;

          if (m_CurrentVersion == null && value == null) return;

          m_CurrentVersion = value;
          Refresh();
        }
      }

    #endregion


    #region Public

      /// <summary>
      /// Returns the absolute path of the item
      /// </summary>
      public string GetAbsoluteItemPath(Item item)
      {
        return m_RootUri.AbsolutePath.TrimEnd('/') + "/" + item.Path.Trim('/');
      }

      /// <summary>
      /// Lists the specified number of versions contained in remote Dav catalog
      /// </summary>
      public List<Version> ListVersions(string startVersion = "1", string endVersion = null, int? limit = null)
      {
        var versions = doListVersions(m_RootUri.AbsoluteUri, UName, UPwd, startVersion, endVersion, limit);
        return versions;
      }

      /// <summary>
      /// Retrieves the latest head version
      /// </summary>
      public Version GetHeadRootVersion()
      {
        var root = (Directory)listDirectoryContent(depth: 0).First();

        var version = ListVersions(root.Version, root.Version, 1).First();

        return version;
      }

      /// <summary>
      /// Refreshes the state of the object by purging cached items (forgets root dir)
      /// </summary>
      public void Refresh()
      {
        m_Root = null;
      }

      public override string ToString()
      {
        return m_RootUri.AbsoluteUri;
      }

    #endregion

    #region .pvt. impl.

      private static List<Version> doListVersions(string rootURL, string uName, string uPwd,
        string startVersion = "1", string endVersion = null, int? limit = null)
      {
        var sb = new StringBuilder();

        sb.AppendLine("<?xml version=\"1.0\"?>");
        sb.AppendLine("<S:log-report xmlns:S=\"svn:\">");

        sb.AppendLine("<S:start-revision>{0}</S:start-revision>".Args(startVersion));

        if (endVersion.IsNotNullOrWhiteSpace())
          sb.AppendLine("<S:end-revision>{0}</S:end-revision>".Args(endVersion));

        if (limit.HasValue)
          sb.AppendLine("<S:limit>{0}</S:limit>".Args(limit.Value));

        sb.AppendLine("<S:date/>");
        sb.AppendLine("</S:log-report>");

        byte[] contentBytes = Encoding.UTF8.GetBytes(sb.ToString());

        Uri uri = new Uri(rootURL);

        HttpWebRequest request = makeRequest(uri, uName, uPwd, 0, REPORT_METHOD);

        request.Headers.Set("Depth", "0");

        request.ContentLength = contentBytes.Length;
        request.ContentType = "text/xml";

        using (var requestStream = request.GetRequestStream())
          requestStream.Write(contentBytes, 0, contentBytes.Length);

        using (var response = (HttpWebResponse)request.GetResponse())
        {
          if (response.StatusCode == HttpStatusCode.OK)
          {
            using (Stream responseStream = response.GetResponseStream())
            {
              using (StreamReader responseReader = new StreamReader(responseStream))
              {
                string responseStr = responseReader.ReadToEnd();
                IEnumerable<Version> items = createVersionsFromXML(responseStr);

                log(uri.ToString(), "URI: {0}  Count: {1}".Args(uri, items.Count()));

                return items.ToList();
              }
            }
          }
          else
            throw new NFXException(NFX.Web.StringConsts.HTTP_OPERATION_ERROR + typeof(WebDAV).Name +
              ".listVersions: response.StatusCode=\"{0}\"".Args(response.StatusCode));
        }
      }

      private static IEnumerable<Version> createVersionsFromXML(string xml)
      {
        XDocument doc = XDocument.Parse(xml);

        var q = doc
          .Elements().Where(e => e.Name.LocalName == "log-report")
          .Elements().Where(e => e.Name.LocalName == "log-item");

        foreach (var e in q)
        {
          var versionEl = e.Elements().First(e1 => e1.Name.LocalName == "version-name");
          var commentEl = e.Elements().First(e1 => e1.Name.LocalName == "comment");
          var creatorEl = e.Elements().First(e1 => e1.Name.LocalName == "creator-displayname");
          var dateEl = e.Elements().First(e1 => e1.Name.LocalName == "date");

          DateTime date = DateTime.Parse(dateEl.Value);

          yield return new Version(versionEl.Value, commentEl.Value, creatorEl.Value, date);
        }
      }

      internal List<Item> listDirectoryContent(Directory dir = null, int depth = 1)
      {
        string relativeUrl = dir != null ? dir.Path : string.Empty;
        Uri uri = new Uri(vercificatePathIfNeeded(m_RootUri.AbsoluteUri.TrimEnd('/') + "/" + relativeUrl.Trim('/')));

        log(uri.ToString(), "listDirectoryContent(dir='{0}',depth='{1}')".Args(dir, depth));

        HttpWebRequest request = makeRequest(uri, LIST_CONTENT_METHOD);

        request.Headers.Set("Depth", "1");

        request.ContentLength = LIST_CONTENT_BODY_BYTES.Length;
        request.ContentType = "text/xml";

        using (Stream requestStream = request.GetRequestStream())
          requestStream.Write(LIST_CONTENT_BODY_BYTES, 0, LIST_CONTENT_BODY_BYTES.Length);

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
          if ((int)response.StatusCode == HTTP_STATUS_CODE_MULTISTATUS)
          {
            using (Stream responseStream = response.GetResponseStream())
            {
              using (StreamReader responseReader = new StreamReader(responseStream))
              {
                string responseStr = responseReader.ReadToEnd();
                IEnumerable<Item> items = createItemsFromXML(responseStr, dir);
                if (depth != 0)
                  items = items.Skip(1);

                return items.ToList();
              }
            }
          }
          else
            throw new NFXException(NFX.Web.StringConsts.HTTP_OPERATION_ERROR + this.GetType().Name +
              ".getFileContent: response.StatusCode=\"{0}\"".Args(response.StatusCode));
        }
      }

      private IEnumerable<Item> createItemsFromXML(string xml, Directory dir)
      {
        XDocument doc = XDocument.Parse(xml);

        var q = doc
          .Elements().Where(e => e.Name.LocalName == "multistatus")
          .Elements().Where(e => e.Name.LocalName == "response");

        foreach (var e in q)
        {
          var hrefEl = e.Elements().First(e1 => e1.Name.LocalName == "href");
          string name = hrefEl.Value.Substring(m_RootUri.AbsolutePath.Length);
          if (name.IsNotNullOrWhiteSpace())
            name = Uri.UnescapeDataString( name.Split(Item.SEPARATORS, StringSplitOptions.RemoveEmptyEntries).Last());

          var propEl = e.Elements().First(e1 => e1.Name.LocalName == "propstat").Elements().First(e1 => e1.Name.LocalName == "prop");

          string lastModificationStr = propEl.Elements().First(e1 => e1.Name.LocalName == "getlastmodified").Value;
          string creationDateStr = propEl.Elements().First(e1 => e1.Name.LocalName == "creationdate").Value;
          string versionName = propEl.Elements().First(e1 => e1.Name.LocalName == "version-name").Value;
          string contentType = propEl.Elements().First(e1 => e1.Name.LocalName == "getcontenttype").Value;

          DateTime lastModificationDate = DateTime.Parse(lastModificationStr);
          DateTime creationDate = DateTime.Parse(creationDateStr);

          Item item;

          if (name.IsNullOrWhiteSpace() || hrefEl.Value.EndsWith("/"))
            item = new Directory(this, name.TrimEnd('/'), versionName, creationDate, lastModificationDate, contentType, dir);
          else
          {
            string sizeStr = propEl.Elements().First(e1 => e1.Name.LocalName == "getcontentlength").Value;
            ulong size = (ulong)sizeStr.AsLong();

            item = new File(this, name, size, versionName, creationDate, lastModificationDate, contentType, dir);
          }

          yield return item;
        }
      }

      internal void getFileContent(File file, Stream stream)
      {
        Uri uri = new Uri(vercificatePathIfNeeded(m_RootUri.AbsoluteUri.TrimEnd('/') + "/" + file.Path.Trim('/')));

        log(uri.ToString(), "getFileContent()");

        HttpWebRequest request = makeRequest(uri);

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
          if (response.StatusCode == HttpStatusCode.OK)
          {
            using (Stream responseStream = response.GetResponseStream())
            {
              responseStream.CopyTo(stream);
            }
          }
          else
            throw new NFXException(NFX.Web.StringConsts.HTTP_OPERATION_ERROR + this.GetType().Name +
              "getFileContent: response.StatusCode=\"{0}\"".Args(response.StatusCode));
        }
      }

      private string vercificatePathIfNeeded(string path)
      {
        if (m_CurrentVersion == null) return path;

        return "{0}?p={1}".Args(path, m_CurrentVersion.Name);
      }

      private static void log(string text, string from)
      {
        var ltp = NFX.Web.WebSettings.WebDavLogType;

        if (ltp.HasValue)
          App.Log.Write(
            new Log.Message
            {
              Type = ltp.Value,
              Topic = NFX.Web.StringConsts.WEB_LOG_TOPIC,
              From = "{0}.{1}".Args(typeof(WebDAV).Name, from),
              Text = text
            }
          );
      }

      private HttpWebRequest makeRequest(Uri uri, string method = "GET")
      {
        return makeRequest(uri, m_UName, m_UPwd, m_TimeoutMs, method);
      }

      private static HttpWebRequest makeRequest(Uri uri, string uName, string uPwd, int timeout = 0, string method = "GET")
      {
        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
        request.Method = method;

        var t = timeout == 0 ? NFX.Web.WebSettings.WebDavDefaultTimeoutMs : timeout;
        if (t > 0) request.Timeout = t;

        NetworkCredential credentials = new NetworkCredential(uName, uPwd);

        request.Credentials = credentials;
        request.PreAuthenticate = true;

        request.Pipelined = false;

        return request;
      }

    #endregion

  } //WDav

}
