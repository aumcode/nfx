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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.IO.FileSystem.S3.V4.S3V4Sign
{
  public static class S3V4URLHelpers
  {
    public const string US_EAST_1 = "us-east-1";

    public static Uri CreateURI(string region = null, string bucketName = null, string itemRelativePath = null, IDictionary<string, string> parameters = null)
    {
      return new Uri(CreateURIString(region, bucketName, itemRelativePath, parameters));
    }

    public static string CreateURIString(string region = null, string bucketName = null, string itemRelativePath = null, IDictionary<string, string> parameters = null)
    {
      string regionURLPart = string.Empty;
      if (!string.IsNullOrEmpty(region))
        if (!region.Equals(US_EAST_1, StringComparison.OrdinalIgnoreCase))
          regionURLPart = string.Format("-{0}", region);

      string bucketURLPart = string.IsNullOrWhiteSpace(bucketName) ? string.Empty : bucketName + ".";

      string queryParametersStr = string.Empty;
      if (parameters != null && parameters.Count > 0)
      {
        queryParametersStr = string.Format("?{0}",
          string.Join("&", parameters.Select(p => string.Format("{0}={1}", Uri.EscapeDataString(p.Key), Uri.EscapeDataString(p.Value)))));
      }

      string endpointURL = string.Format("https://{0}s3{1}.amazonaws.com/{2}{3}", bucketURLPart, regionURLPart,
        itemRelativePath != null ? itemRelativePath.TrimStart('/') : null,
        queryParametersStr);

      return endpointURL;
    }

    public static void Parse(string path, out string bucket, out string region, out string itemLocalPath)
    {
      IDictionary<string, string> queryParams;
      Parse( path, out bucket, out region, out itemLocalPath, out queryParams);
    }

    public static void Parse(string path, out string bucket, out string region, out string itemLocalPath, out IDictionary<string, string> queryParams)
    {
      Uri uri;
      try
      {
        uri = new Uri(path);
      }
      catch (UriFormatException)
      {
        throw new NFXException(NFX.Web.StringConsts.ARGUMENT_ERROR + typeof(S3V4URLHelpers) + ".Parse(path should be a valid Uri)");
      }

      Parse( uri, out bucket, out region, out itemLocalPath, out queryParams);
    }

    public static void Parse(Uri uri, out string bucket, out string region, out string itemLocalPath, out IDictionary<string, string> queryParams)
    {
      bucket = region = itemLocalPath = null;

      if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        throw new NFXException(NFX.Web.StringConsts.ARGUMENT_ERROR + typeof(S3V4URLHelpers) + ".Parse(path should be a http or https Uri)");

      itemLocalPath = uri.LocalPath.TrimStart('/');

      queryParams = null;
      if (!string.IsNullOrWhiteSpace(uri.Query))
      {
        foreach(var paramStr in uri.Query.TrimStart('?').Split('&'))
        {
          if (queryParams == null)
            queryParams = new Dictionary<string, string>();

          string[] keyNValuePair = paramStr.Split('=');
          string key = Uri.UnescapeDataString(keyNValuePair[0]);
          string value = keyNValuePair.Length > 1 ? Uri.UnescapeDataString(keyNValuePair[1]) : null;
          queryParams.Add(key, value);
        }
      }

      string[] hostFragments = uri.Host.Split('.');
      if (hostFragments.Length != 4)
        throw new NFXException(NFX.Web.StringConsts.ARGUMENT_ERROR + typeof(S3V4URLHelpers) + ".Parse(path host should be [BUCKET].s3.amazonaws.com or [BUCKET].s3-[REGION].amazonaws.com)");

      string zoneFragment = hostFragments[hostFragments.Length - 1];
      string domainFragment = hostFragments[hostFragments.Length - 2];
      if (zoneFragment != "com" || domainFragment != "amazonaws")
        throw new NFXException(NFX.Web.StringConsts.ARGUMENT_ERROR + typeof(S3V4URLHelpers) + ".Parse(path should have domain amazonaws.com)");

      bucket = hostFragments[0];

      string s3Fragment = hostFragments[1];
      if (!s3Fragment.StartsWith("s3"))
        throw new NFXException(NFX.Web.StringConsts.ARGUMENT_ERROR + typeof(S3V4URLHelpers) + ".Parse(path host should be [BUCKET].s3.amazonaws.com or [BUCKET].s3-[REGION].amazonaws.com)");

      if (s3Fragment == "s3")
      {
        region = US_EAST_1;
      }
      else
      {
        if (!s3Fragment.StartsWith("s3-"))
          throw new NFXException(NFX.Web.StringConsts.ARGUMENT_ERROR + typeof(S3V4URLHelpers) + ".Parse(path host should be [BUCKET].s3.amazonaws.com or [BUCKET].s3-[REGION].amazonaws.com)");

        region = s3Fragment.Substring("s3-".Length);
      }
    }

    public static string GetDomainURL(this Uri uri)
    {
      return "{0}://{1}".Args(uri.Scheme, uri.Host);
    }

    public static string GetParentURL(this Uri uri)
    {
      if (uri.Segments.Count() < 2)
        return string.Empty;
      else
        return uri.GetDomainURL() + string.Join(string.Empty, uri.Segments.Take(uri.Segments.Count() - 1)).TrimEnd('/');
    }

    public static string GetLocalName(this Uri uri)
    {
      if (uri.Segments.Count() < 2)
        return string.Empty;
      else
        return uri.Segments.Last().TrimEnd('/');
    }

    public static string ToDirectoryPath(this string itemLocalPath)
    {
      if (!string.IsNullOrWhiteSpace(itemLocalPath) && !itemLocalPath.EndsWith("/"))
        return itemLocalPath + "/";
      else
        return itemLocalPath;
    }

  } //S3URL
}
