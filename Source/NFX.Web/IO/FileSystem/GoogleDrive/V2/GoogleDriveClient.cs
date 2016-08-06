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
 * Author: Andrey Kolbasov <andrey@kolbasov.com>
 */
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using NFX.Web;
using NFX.Serialization.JSON;
using NFX.IO.FileSystem.GoogleDrive.Auth;

namespace NFX.IO.FileSystem.GoogleDrive.V2
{
    /// <summary>
    /// Google Drive REST API wrapper.
    /// </summary>
    public class GoogleDriveClient
    {
      #region CONST

        private const string ROOT = "root";
        private const string MY_DRIVE = "My Drive";
        private const string GRANT_TYPE = "refresh_token";
        private const string USER_AGENT = "NFX";

        private const string HEADER_AUTHORIZATION = "Authorization";
        private const string HEADER_LOCATION = "Location";

      #endregion

      #region Fields

        private int m_Timeout;
        private int m_Attempts = 5;

        private string m_AccessToken;
        private JsonWebToken m_JsonWebToken;

      #endregion

      #region .ctor

        public GoogleDriveClient(string email, string certPath)
        {
          m_JsonWebToken = new JsonWebToken(email, certPath);
        }

        public GoogleDriveClient(string email, string certPath, int timeout, int attempts)
          : this(email, certPath)
        {
          m_Timeout = timeout;

          if (attempts > 0)
            m_Attempts = attempts;
        }

      #endregion

      #region Public

        /// <summary>
        /// Returns directory names contained in the spicifed directory
        /// </summary>
        public IEnumerable<string> GetDirectories(string path, bool recursive)
        {
          Ensure.NotNull(path, "path");

          var parent = GetHandle(path);

          if (parent == null || !parent.IsFolder)
          {
            yield break;
          }

          var items = GetSubfolders(parent.Id, recursive);

          foreach (var item in items)
          {
            yield return item[Metadata.TITLE];
          }
        }

        /// <summary>
        /// Returns files contained in the spicifed directory
        /// </summary>
        public IEnumerable<GoogleDriveHandle> GetFiles(string parentId, bool recursive)
        {
          Ensure.NotNull(parentId, "parentId");

          var items = (IEnumerable<dynamic>)Get(ApiUrls.Files(parentId)).Data[Metadata.ITEMS];

          foreach (var item in items)
          {
            yield return new GoogleDriveHandle(item);
          }

          if (!recursive)
          {
            yield break;
          }

          // Get all subfolders recursivly.
          var subfolders = GetSubfolders(parentId, recursive:true);

          foreach (var subfolder in subfolders)
          {
            var subfolderId = (string)subfolder[Metadata.ID];

            var files = GetFiles(subfolderId, recursive:false);

            foreach (var file in files)
            {
              yield return file;
            }
          }
        }

        /// <summary>
        /// Creates a directory
        /// </summary>
        public GoogleDriveHandle CreateDirectory(string parentId, string name)
        {
          Ensure.NotNull(parentId, "parentId");

          var body = new GoogleDriveRequestBody();
          body.SetTitle(name);
          body.SetParent(parentId);
          body.SetMimeType(GoogleDriveMimeType.FOLDER);

          var res = Post(ApiUrls.Files(), body);

          var handle = new GoogleDriveHandle(res.Data);

          return handle;
        }

        /// <summary>
        /// Creates a file
        /// </summary>
        public GoogleDriveHandle CreateFile(string parentId, string name)
        {
          Ensure.NotNull(parentId, "parentId");

          var body = new GoogleDriveRequestBody();
          body.SetTitle(name);
          body.SetParent(parentId);

          var res = Post(ApiUrls.Files(), body);

          var handle = new GoogleDriveHandle(res.Data);

          return handle;
        }

        /// <summary>
        /// Creates a file
        /// </summary>
        public GoogleDriveHandle CreateFile(string parentId, string name, Stream stream)
        {
          Ensure.NotNull(parentId, "parentId");
          Ensure.NotNull(name, "name");

          // 1. Start a resumable session.
          var body = new GoogleDriveRequestBody();
          body.SetTitle(name);
          body.SetParent(parentId);

          var reqStream = body.ToJSON().ToStream();

          var res = TrySend(HTTPRequestMethod.POST, ContentType.JSON, ApiUrls.Upload(), reqStream);

          // 2. Save the resumable session URI.
          var location = res.Headers[HEADER_LOCATION].FormatUri();

          // 3. Upload the file.
          var info = Post(location, stream);

          return new GoogleDriveHandle(info.Data);
        }

        /// <summary>
        /// Updates content of a file by file ID
        /// </summary>
        public GoogleDriveHandle UpdateFile(string id, Stream stream)
        {
          Ensure.NotNull(id, "id");

          var res = Put(ApiUrls.Update(id), stream);
          return new GoogleDriveHandle(res.Data);
        }

        /// <summary>
        /// Gets a file by ID
        /// </summary>
        public void GetFile(string id, Stream stream)
        {
          Ensure.NotNull(id, "id");

          var res = TrySend(HTTPRequestMethod.GET, null, ApiUrls.Download(id));

          using(var data = res.GetResponseStream())
          {
            data.CopyTo(stream);
          }
        }

        /// <summary>
        /// Deletes an item
        /// </summary>
        public void Delete(string id)
        {
          Ensure.NotNull(id, "id");

          Delete(ApiUrls.FileById(id));
        }

        /// <summary>
        /// Sets modification timestamp
        /// </summary>
        public void SetModifiedDate(string id, DateTime date)
        {
          Ensure.NotNull(id, "id");

          var body = new GoogleDriveRequestBody();
          body.SetModifiedDate(date);

          Patch(ApiUrls.SetModifiedDate(id), body);
        }

        /// <summary>
        /// Renames a file/folder by its ID
        /// </summary>
        public void Rename(string id, string name)
        {
          Ensure.NotNull(id, "id");
          Ensure.NotNull(name, "name");

          var body = new GoogleDriveRequestBody();
          body.SetTitle(name);

          Patch(ApiUrls.Rename(id), body);
        }

        /// <summary>
        /// Checks whether the specified file exists
        /// </summary>
        public bool FileExists(string path)
        {
          Ensure.NotNull(path, "path");

          var info = GetHandle(path);

          return info != null && !info.IsFolder;
        }

        /// <summary>
        /// Checks whether the specified folder exists
        /// </summary>
        public bool FolderExists(string path)
        {
          Ensure.NotNull(path, "path");

          var info = GetHandle(path);

          return info != null && info.IsFolder;
        }

        /// <summary>
        /// Gets a handle for the specified file or folder.
        /// </summary>
        public GoogleDriveHandle GetHandle(string path)
        {
          Ensure.NotNull(path, "path");

          var segments = GoogleDrivePath.Split(path);

          if (segments.Length == 0 || path.EqualsOrdIgnoreCase(ROOT))
          {
            var root = GetItemInfoById(ROOT);
            return root;
          }

          var queue = new Queue<string>(segments);

          if (IsRoot(segments[0]))
          {
            queue.Dequeue();
          }

          var info = GetItemInfo(ROOT, queue);

          if (info == null)
          {
            return null;
          }

          var handle = new GoogleDriveHandle(info);

          return handle;
        }

        public GoogleDriveHandle GetItemInfoById(string id)
        {
          Ensure.NotNull(id, "id");

          var res = Get(ApiUrls.FileById(id));

          return new GoogleDriveHandle(res.Data);
        }

      #endregion

      #region Private

        private bool IsRoot(string segment)
        {
          return segment.EqualsOrdIgnoreCase(ROOT) || segment.EqualsOrdIgnoreCase(MY_DRIVE);
        }

        private IEnumerable<dynamic> GetSubfolders(string parentId, bool recursive)
        {
          var res = Get(ApiUrls.Subfolders(parentId));

          var subfolders = (IEnumerable<dynamic>)res.Data[Metadata.ITEMS];

          foreach (var subfolder in subfolders)
          {
            yield return subfolder;

            if (recursive)
            {
              var subfolderId = (string)subfolder[Metadata.ID];

              foreach(var folder in GetSubfolders(subfolderId, recursive))
              {
                yield return folder;
              }
            }
          }
        }

        private IEnumerable<dynamic> GetSubfolders(string parentId)
        {
          var res = Get(ApiUrls.Subfolders(parentId));

          var subfolders = (IEnumerable<dynamic>)res.Data[Metadata.ITEMS];

          foreach (var subfolder in subfolders)
          {
            yield return subfolder;
          }
        }

        private dynamic Get(Uri uri)
        {
          return Send(HTTPRequestMethod.GET, null, uri);
        }

        private dynamic Post(Uri uri, Stream body)
        {
          return Send(HTTPRequestMethod.POST, ContentType.BINARY, uri, body);
        }

        private dynamic Post(Uri uri, object body)
        {
          var stream = body.ToJSON().ToStream();
          return Send(HTTPRequestMethod.POST, ContentType.JSON, uri, stream);
        }

        private dynamic Put(Uri uri, Stream body)
        {
          return Send(HTTPRequestMethod.PUT, null, uri, body);
        }

        private dynamic Patch(Uri uri, object body)
        {
          var stream = body.ToJSON().ToStream();
          return Send(HTTPRequestMethod.PATCH, ContentType.JSON, uri, stream);
        }

        private void Delete(Uri uri)
        {
          Send(HTTPRequestMethod.DELETE, null, uri);
        }

        private HttpWebRequest CreateRequest(HTTPRequestMethod method, string contentType, Uri uri, Stream body = null)
        {
          var fullUri = new Uri(ApiUrls.GoogleApi(), uri);

          var req = WebRequest.CreateHttp(fullUri);
          req.Method = method.ToString();

          if (m_Timeout > 0)
          {
            req.Timeout = m_Timeout;
          }

          req.Headers.Add(HEADER_AUTHORIZATION, "Bearer " + m_AccessToken);
          req.UserAgent = USER_AGENT;

          var canHaveBody =
            method == HTTPRequestMethod.PUT  ||
            method == HTTPRequestMethod.POST ||
            method == HTTPRequestMethod.PATCH;

          if (canHaveBody && body != null)
          {
            req.ContentType = contentType;
            req.ContentLength = body.Length;

            using (var stream = req.GetRequestStream())
            {
              body.Position = 0;
              body.CopyTo(stream);

              stream.Flush();
              stream.Close();
            }
          }

          return req;
        }

        /// <summary>
        /// Tries to send a request m_NumberOfAttempts of times
        /// </summary>
        private HttpWebResponse TrySend(HTTPRequestMethod method, string contentType, Uri uri, Stream body = null)
        {
          for (var n = 0; n < m_Attempts; ++n)
          {
            try
            {
              var req = CreateRequest(method, contentType, uri, body);
              return SendRequest(req);
            }
            catch (System.Net.WebException ex)
            {
              var retry = CanRetryRequest(ex);

              if (!retry)
              {
                throw new FileSystemException(ex.Message, ex);
              }

              var rnd = new Random();
              Thread.Sleep((1 << n) * 1000 + rnd.Next(1001));
            }
          }

          return null;
        }

        private HttpWebResponse SendRequest(HttpWebRequest req)
        {
          return (HttpWebResponse)req.GetResponse();
        }

        private bool CanRetryRequest(System.Net.WebException ex)
        {
          if (ex.Status != WebExceptionStatus.ProtocolError)
          {
            return false;
          }

          var res = ex.Response as HttpWebResponse;

          if (res == null || res.StatusCode != HttpStatusCode.Unauthorized)
          {
            return false;
          }

          RefreshAccessToken();

          return true;
        }

        /// <summary>
        /// Refreshes the access token using the refresh token
        /// </summary>
        private void RefreshAccessToken()
        {
          var body = new GoogleDriveRequestBody();

          m_JsonWebToken.Refresh();
          body[Metadata.ASSERTION] = m_JsonWebToken.ToString();
          body[Metadata.GRANT_TYPE] = JsonWebToken.GRANT_TYPE;

          var stream = body.ToFormEncoded().ToStream();

          try
          {
            var req = CreateRequest(HTTPRequestMethod.POST, ContentType.FORM_URL_ENCODED, ApiUrls.Token(), stream);
            var res = SendRequest(req);

            m_AccessToken = res.GetJsonAsDynamic().Data[Metadata.ACCESS_TOKEN];
          }
          catch(System.Net.WebException ex)
          {
            throw new FileSystemException(ex.Message, ex);
          }
        }

        private dynamic Send(HTTPRequestMethod method, string contentType, Uri uri, Stream body = null)
        {
          if (m_AccessToken.IsNullOrEmpty())
            RefreshAccessToken();

          var res = TrySend(method, contentType, uri, body);
          return res.GetJsonAsDynamic();
        }

        private dynamic GetItemInfo(string parentId, Queue<string> queue)
        {
          var name = queue.Dequeue();

          var url = ApiUrls.FileByName(parentId, name);

          var res = Get(url);

          var items = (IList<dynamic>)res.Data[Metadata.ITEMS];

          if (items.Count == 0)
          {
            return null;
          }

          // TODO: should we throw an exception if there are more than one file?
          var child = items[0];

          if (queue.Count > 0)
          {
            return GetItemInfo(child[Metadata.ID], queue);
          }

          return child;
        }

      #endregion
    }
}