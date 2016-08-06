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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NFX.IO.FileSystem.S3.V4.S3V4Sign
{
  /// <summary>
  /// Handles S3V4 signing routines
  /// </summary>
  public class S3V4Signer
  {
    #region CONST

      public const string EMPTY_BODY_SHA256 = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

      public const string X_AMZ_CONTENT_SHA256 = "X-Amz-Content-SHA256";
      public const string X_AMZ_DATE = "X-Amz-Date";

      public const string SCHEME = "AWS4";
      public const string AWS4_ALGORITHM = "AWS4-HMAC-SHA256";
      public const string S3SERVICE = "s3";
      public const string TERMINATOR = "aws4_request";

      public const string COLON = ":";
      public const string SEMICOLON = ";";
      public const string AND = "&";
      public const string NEWLINE = "\n";

      public const string SHA256 = "SHA-256";
      public const string HMACSHA256 = "HMACSHA256";

    #endregion

    #region Static

      private HashAlgorithm m_SHA256Algo = HashAlgorithm.Create(SHA256);

      /// <summary>
      /// Returns string representing S3V4 call in canonized form
      /// </summary>
      public static string GetCanonicalRequest(string method, Uri uri,
        IDictionary<string, string> headers, IDictionary<string, string> queryParameters = null,
        string hashedPayload = EMPTY_BODY_SHA256)
      {
        var sortedLoweredHeaders = (headers ?? new Dictionary<string, string>()).OrderBy(kvp => kvp.Key);

        string canonicalQueryString = string.Join(AND, (queryParameters ?? new Dictionary<string, string>())
          .OrderBy(kvp => Uri.EscapeDataString(kvp.Key))
          .Select(kvp => string.Format("{0}={1}", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value))));

        string canonicalHeaderString = string.Concat(
          sortedLoweredHeaders.Select(kvp => string.Format("{0}:{1}\n", kvp.Key.ToLower(), kvp.Value.Trim())));

        string signedHeaders = string.Join(SEMICOLON, sortedLoweredHeaders.Select(kvp => kvp.Key.ToLower()));

        string r = string.Join(NEWLINE, method, uri.GetS3AbsoolutePath(),
          canonicalQueryString, canonicalHeaderString, signedHeaders, hashedPayload);

        return r;
      }

      /// <summary>
      /// Returns string which must be signed
      /// </summary>
      public string GetStringToSign(DateTime date, string region, string canonicalRequestStr, string scope)
      {
        string canonicalRequestHashHexStr = ComputeHash(canonicalRequestStr);

        string r = string.Join(NEWLINE, AWS4_ALGORITHM, date.S3DateTimeString(), scope, canonicalRequestHashHexStr);

        return r;
      }

      /// <summary>
      /// Calculates SHA256 hash of given string.
      /// Returns haxadecimal string representation of hash
      /// </summary>
      public string ComputeHash(string str)
      {
        byte[] hashBytes = m_SHA256Algo.ComputeHash(Encoding.UTF8.GetBytes(str));
        return hashBytes.ToHexString();
      }

      /// <summary>
      /// Returns "scope" (S3 V4 term) of given region
      /// </summary>
      public string GetScope(DateTime date, string region)
      {
        string s = string.Join("/", date.S3DateString(), region, S3SERVICE, TERMINATOR);
        return s;
      }

      /// <summary>
      /// Calculates signing key for given secret, timestamp and region
      /// </summary>
      public byte[] GetScopedSigningKey(string secretKey, DateTime date, string region)
      {
        byte[] initialKey = (SCHEME + secretKey).S3Bytes();

        byte[] hashDate = ComputeKeyedHash(initialKey, date.S3DateString().S3Bytes());
        byte[] dateRegionKey = ComputeKeyedHash(hashDate, region.S3Bytes());
        byte[] dateRegionServiceKey = ComputeKeyedHash(dateRegionKey, S3SERVICE.S3Bytes());
        byte[] signingKey = ComputeKeyedHash(dateRegionServiceKey, TERMINATOR.S3Bytes());

        return signingKey;
      }

      /// <summary>
      /// Calculates hash of data by key
      /// </summary>
      public byte[] ComputeKeyedHash(byte[] key, byte[] data)
      {
        KeyedHashAlgorithm algorithm = KeyedHashAlgorithm.Create(HMACSHA256);
        algorithm.Key = key;
        return algorithm.ComputeHash(data);
      }

      /// <summary>
      /// Returns authorization header content
      /// </summary>
      public string GetAutorization(string accessKey, string scope, string signedHeaders, string signature)
      {
        StringBuilder b = new StringBuilder();

        b.AppendFormat("{0} ", AWS4_ALGORITHM);
        b.AppendFormat("Credential={0}/{1}, ", accessKey, scope);
        b.AppendFormat("SignedHeaders={0}, ", signedHeaders);
        b.AppendFormat("Signature={0}", signature);

        return b.ToString();
      }

    #endregion

    #region ctor

      public S3V4Signer() {}

    #endregion

    #region Properties

      /// <summary>
      /// Call access key
      /// </summary>
      public string AccessKey
      {
        set
        {
          m_Calculated = false;
          m_AccessKey = value;
        }
      }

      /// <summary>
      /// Call secret key
      /// </summary>
      public string SecretKey
      {
        set
        {
          m_Calculated = false;
          m_SecretKey = value;
        }
      }

      /// <summary>
      /// Call region
      /// </summary>
      public string Region
      {
        set
        {
          m_Calculated = false;
          m_Region = value;
        }
      }

      /// <summary>
      /// Call bucket
      /// </summary>
      public string Bucket
      {
        set
        {
          m_Calculated = false;
          m_Bucket = value;
        }
      }

      /// <summary>
      /// Call item local path
      /// </summary>
      public string ItemLocalPath
      {
        set
        {
          m_Calculated = false;
          m_ItemLocalPath = value;
        }
      }

      /// <summary>
      /// Call method (i.e. GET)
      /// </summary>
      public string Method
      {
        set
        {
          m_Calculated = false;
          m_Method = value;
        }
      }

      /// <summary>
      /// Call timestamp
      /// </summary>
      public DateTime? RequestDateTime
      {
        set
        {
          m_Calculated = false;
          m_RequestDateTime = value;
        }
      }

      /// <summary>
      /// Call query params
      /// </summary>
      public IDictionary<string, string> QueryParams
      {
        set
        {
          m_Calculated = false;
          m_QueryParams = value;
        }
      }

      /// <summary>
      /// Call content stream
      /// </summary>
      public Stream ContentStream
      {
        set
        {
          m_Calculated = false;
          m_ContentStream = value;
        }
      }

      /// <summary>
      /// Call Uri (readonly)
      /// </summary>
      public Uri Uri
      {
        get
        {
          if (!m_Calculated) Calculate();
          return m_Uri;
        }
      }

      /// <summary>
      /// Call http headers (readonly)
      /// </summary>
      public IDictionary<string, string> Headers
      {
        get
        {
          if (!m_Calculated) Calculate();
          return m_Headers;
        }
      }

    #endregion

    #region Public

      public IDictionary<string, string> Calculate()
      {
        m_Uri = S3V4URLHelpers.CreateURI(m_Region, m_Bucket, m_ItemLocalPath, m_QueryParams);

        DateTime dateTime = m_RequestDateTime ?? DateTime.UtcNow;

        int contentLength = m_ContentStream != null ? (int)m_ContentStream.Length : 0;

        string contentHash = m_ContentStream == null || m_ContentStream.Length == 0 ? EMPTY_BODY_SHA256 : computeHash(m_ContentStream);

        IDictionary<string, string> headers = new Dictionary<string, string>() {
          {"content-type", "text/plain"},
          {"Host", m_Uri.Host},
          {X_AMZ_CONTENT_SHA256, contentHash},
          {X_AMZ_DATE, dateTime.S3DateTimeString()}
        };

        if (m_Method == "PUT")
          headers.Add("content-length", contentLength.ToString());

        var sortedLoweredHeaders = (headers ?? new Dictionary<string, string>()).OrderBy(kvp => kvp.Key);
        string signedHeaders = string.Join(SEMICOLON, sortedLoweredHeaders.Select(kvp => kvp.Key.ToLower()));

        string canonicalRequest = GetCanonicalRequest(m_Method, m_Uri, headers, m_QueryParams, contentHash);

        string scope = GetScope(dateTime, m_Region);

        string stringToSign = GetStringToSign(dateTime, m_Region, canonicalRequest, scope);

        byte[] scopedSigningKey = GetScopedSigningKey(m_SecretKey, dateTime, m_Region);

        byte[] signature = ComputeKeyedHash(scopedSigningKey, stringToSign.S3Bytes());
        string signatureStr = signature.ToHexString();

        string authorization = GetAutorization(m_AccessKey, scope, signedHeaders, signatureStr);

        headers.Add("Authorization", authorization);

        m_Headers = headers;

        m_Calculated = true;

        return headers;
      }

    #endregion

    #region prt. impl.

      private string computeHash(Stream stream)
      {
        stream.Position = 0;
        byte[] hashBytes = m_SHA256Algo.ComputeHash(stream);
        return hashBytes.ToHexString();
      }

    #endregion

    #region pvt. properties

      private bool m_Calculated;

      private string m_AccessKey;
      private string m_SecretKey;

      private string m_Region;
      private string m_Bucket;
      private string m_ItemLocalPath;

      private string m_Method;
      private DateTime? m_RequestDateTime;
      private IDictionary<string, string> m_QueryParams;
      //private byte[] m_ContentBytes;
      private Stream m_ContentStream;

      // Outbound
      private Uri m_Uri;
      private IDictionary<string, string> m_Headers;

    #endregion

  }
}
