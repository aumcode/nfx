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
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFX.IO.FileSystem.S3.V4.S3V4Sign;
using System.IO;

namespace NFX.IO.FileSystem.S3.V4
{
  public class S3V4
  {
    #region Public

      public static bool FileExists(S3V4URI uri, string accessKey, string secretKey, int timeoutMs)
      {
        return FileExists(uri.LocalPath, accessKey, secretKey, uri.Bucket, uri.Region, timeoutMs);
      }

      public static bool FileExists(string itemLocalPath, string accessKey, string secretKey, string bucket, string region, int timeoutMs)
      {
        return ItemExists(itemLocalPath, accessKey, secretKey, bucket, region, timeoutMs);
      }

      public static bool FolderExists(S3V4URI uri, string accessKey, string secretKey, int timeoutMs)
      {
        return FileExists(uri.LocalPath, accessKey, secretKey, uri.Bucket, uri.Region, timeoutMs);
      }

      public static bool FolderExists(string itemLocalPath, string accessKey, string secretKey, string bucket, string region, int timeoutMs)
      {
        string s3FolderPath = itemLocalPath.ToDirectoryPath();
        return ItemExists( s3FolderPath, accessKey, secretKey, bucket, region, timeoutMs);
      }

      public static bool ItemExists(S3V4URI uri, string accessKey, string secretKey, int timeoutMs)
      {
        return ItemExists(uri.LocalPath, accessKey, secretKey, uri.Bucket, uri.Region, timeoutMs);
      }

      public static bool ItemExists(string itemLocalPath, string accessKey, string secretKey, string bucket, string region, int timeoutMs)
      {
        try
        {
          GetItemMetadata(itemLocalPath, accessKey, secretKey, bucket, region, timeoutMs);
        }
        catch (WebException ex)
        {
          if (ex.Message.Contains("404"))
            return false;

          throw;
        }

        return true;
      }

      public static IDictionary<string, string> GetItemMetadata(S3V4URI uri, string accessKey, string secretKey, int timeoutMs)
      {
        return GetItemMetadata(uri.LocalPath, accessKey, secretKey, uri.Bucket, uri.Region, timeoutMs);
      }

      public static IDictionary<string, string> GetItemMetadata(string itemLocalPath, string accessKey, string secretKey,
        string bucket, string region, int timeoutMs)
      {
        Uri uri = S3V4URLHelpers.CreateURI(region, bucket, itemLocalPath);

        S3V4Signer signer = new S3V4Signer()
        {
          AccessKey = accessKey, SecretKey = secretKey,
          Bucket = bucket, Region = region,
          Method = "HEAD",
          ItemLocalPath = itemLocalPath
        };

        var headers = signer.Headers;

        var request = S3V4HttpHelpers.ConstructWebRequest(uri, "HEAD", new MemoryStream(), headers, timeoutMs);

        var resultHeaders = S3V4HttpHelpers.GetHeaders(request);
        return resultHeaders;
      }

      public static void GetFile(S3V4URI uri, string accessKey, string secretKey, Stream stream, int timeoutMs)
      {
        GetFile(uri.LocalPath, accessKey, secretKey, uri.Bucket, uri.Region, stream, timeoutMs);
      }

      public static void GetFile(string itemLocalPath, string accessKey, string secretKey, string bucket, string region, Stream stream,
        int timeoutMs)
      {
        Uri uri = S3V4URLHelpers.CreateURI(region, bucket, itemLocalPath);

        S3V4Signer signer = new S3V4Signer()
        {
          AccessKey = accessKey,
          SecretKey = secretKey,
          Bucket = bucket,
          Region = region,
          Method = "GET",
          ItemLocalPath = itemLocalPath
        };

        var headers = signer.Headers;

        var request = S3V4HttpHelpers.ConstructWebRequest(uri, "GET", new MemoryStream(), headers, timeoutMs);

        request.GetResponseBytes(stream);
      }

      public static string PutFolder(S3V4URI uri, string accessKey, string secretKey, int timeoutMs)
      {
        return PutFolder( uri.LocalPath, accessKey, secretKey, uri.Bucket, uri.Region, timeoutMs);
      }

      public static string PutFolder(string itemLocalPath, string accessKey, string secretKey, string bucket, string region, int timeoutMs)
      {
        string s3FolderPath = itemLocalPath.ToDirectoryPath();
        return PutItem(s3FolderPath, accessKey, secretKey, bucket, region, EMPTY_CONTENT_STREAM, timeoutMs);
      }

      public static string PutFile(S3V4URI uri, string accessKey, string secretKey, Stream contentStream, int timeoutMs)
      {
        return PutItem(uri.LocalPath, accessKey, secretKey, uri.Bucket, uri.Region, contentStream, timeoutMs);
      }

      public static string PutFile(string itemLocalPath, string accessKey, string secretKey, string bucket, string region, Stream contentStream,
        int timeoutMs)
      {
        return PutItem(itemLocalPath, accessKey, secretKey, bucket, region, contentStream, timeoutMs);
      }

      public static string PutItem(string itemLocalPath, string accessKey, string secretKey, string bucket, string region, Stream contentStream,
        int timeoutMs)
      {
        Uri uri = S3V4URLHelpers.CreateURI(region, bucket, itemLocalPath);

        S3V4Signer signer = new S3V4Signer()
        {
          AccessKey = accessKey,
          SecretKey = secretKey,
          Bucket = bucket,
          Region = region,
          Method = "PUT",
          ItemLocalPath = itemLocalPath,
          ContentStream = contentStream
        };

        var headers = signer.Headers;

        var request = S3V4HttpHelpers.ConstructWebRequest(uri, "PUT", contentStream, headers, timeoutMs);

        var resultHeaders = request.GetHeaders();

        return resultHeaders["ETag"];
      }

      public static void SetACL(string itemLocalPath, string accessKey, string secretKey, string bucket, string region, string acl,
        int timeoutMs)
      {
        var queryParams = new Dictionary<string, string>() { { "acl", string.Empty } };

        MemoryStream aclStream = new MemoryStream(Encoding.UTF8.GetBytes(acl ?? string.Empty));

        Uri uri = S3V4URLHelpers.CreateURI(region, bucket, itemLocalPath, queryParams);

        S3V4Signer signer = new S3V4Signer()
        {
          AccessKey = accessKey,
          SecretKey = secretKey,
          Bucket = bucket,
          Region = region,
          Method = "PUT",
          ItemLocalPath = itemLocalPath,
          QueryParams = queryParams,
          ContentStream = aclStream
        };

        var headers = signer.Headers;

        var request = S3V4HttpHelpers.ConstructWebRequest(uri, "PUT", aclStream, headers, timeoutMs);

        request.GetHeaders();
      }

      public static void RemoveFolder(S3V4URI uri, string accessKey, string secretKey, int timeoutMs)
      {
        RemoveFolder(uri.LocalPath, accessKey, secretKey, uri.Bucket, uri.Region, timeoutMs);
      }

      public static void RemoveFolder(string itemLocalPath, string accessKey, string secretKey, string bucket, string region, int timeoutMs)
      {
        string s3FolderPath = itemLocalPath.ToDirectoryPath();
        RemoveItem(s3FolderPath, accessKey, secretKey, bucket, region, timeoutMs);
      }

      public static void RemoveFile(S3V4URI uri, string accessKey, string secretKey, int timeoutMs)
      {
        RemoveFile(uri.LocalPath, accessKey, secretKey, uri.Bucket, uri.Region, timeoutMs);
      }

      public static void RemoveFile(string itemLocalPath, string accessKey, string secretKey, string bucket, string region, int timeoutMs)
      {
        RemoveItem(itemLocalPath, accessKey, secretKey, bucket, region, timeoutMs);
      }

      public static void RemoveItem(S3V4URI uri, string accessKey, string secretKey, int timeoutMs)
      {
        RemoveItem(uri.LocalPath, accessKey, secretKey, uri.Bucket, uri.Region, timeoutMs);
      }

      public static void RemoveItem(string itemLocalPath, string accessKey, string secretKey, string bucket, string region, int timeoutMs)
      {
        Uri uri = S3V4URLHelpers.CreateURI(region, bucket, itemLocalPath);

        S3V4Signer signer = new S3V4Signer()
        {
          AccessKey = accessKey,
          SecretKey = secretKey,
          Bucket = bucket,
          Region = region,
          Method = "DELETE",
          ItemLocalPath = itemLocalPath
        };

        var headers = signer.Headers;

        var request = S3V4HttpHelpers.ConstructWebRequest(uri, "DELETE", EMPTY_CONTENT_STREAM, headers, timeoutMs);

        request.GetHeaders(HttpStatusCode.NoContent);
      }

      public static string ListBucket(S3V4URI uri, string accessKey, string secretKey, int timeoutMs,
        string prefix = null, string marker = null, int? maxKeys = null)
      {
        return ListBucket(uri.LocalPath, accessKey, secretKey, uri.Bucket, uri.Region, timeoutMs, prefix, marker, maxKeys);
      }

      public static string ListBucket(string itemLocalPath, string accessKey, string secretKey, string bucket, string region, int timeoutMs,
        string prefix = null, string marker = null, int? maxKeys = null)
      {
        var queryParams = new Dictionary<string, string>();

        if (prefix != null)
          queryParams.Add("prefix", prefix);

        if (marker != null)
          queryParams.Add("marker", marker);

        if (maxKeys.HasValue)
          queryParams.Add("max-keys", maxKeys.Value.ToString());

        Uri uri = S3V4URLHelpers.CreateURI(region, bucket, "", queryParams);

        S3V4Signer signer = new S3V4Signer()
        {
          AccessKey = accessKey,
          SecretKey = secretKey,
          Bucket = bucket,
          Region = region,
          Method = "GET"
          ,QueryParams = queryParams
        };

        var headers = signer.Headers;

        var request = S3V4HttpHelpers.ConstructWebRequest(uri, "GET", EMPTY_CONTENT_STREAM, headers, timeoutMs);

        string responseStr = request.GetResponseStr();

        return responseStr;
      }

      private static MemoryStream EMPTY_CONTENT_STREAM = new MemoryStream();

    #endregion
  }
}
