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
using System;
using System.Reflection;
using System.Text;

using NFX.Environment;


namespace NFX.Web
{
  /// <summary>
  /// Decorates controller classes or actions that set NoCache headers in response. By default Force = true
  /// </summary>
  public struct CacheControl
  {
    public const int DEFAULT_CACHE_MAX_AGE_SEC = 24 * //hrs
                                                 60 * //min
                                                 60;   //sec

    public enum Type
    {
      Public = 0,
      Private,
      NoCache
    }

    public static CacheControl FromConfig(IConfigSectionNode cfg)
    {
      return ConfigAttribute.Apply(new CacheControl(), cfg);
    }

    public static CacheControl PublicMaxAgeSec(int maxAgeSec = DEFAULT_CACHE_MAX_AGE_SEC)
    {
      return new CacheControl { Cacheability = Type.Public, MaxAgeSec = maxAgeSec };
    }

    public static CacheControl PrivateMaxAgeSec(int maxAgeSec = DEFAULT_CACHE_MAX_AGE_SEC)
    {
      return new CacheControl { Cacheability = Type.Private, MaxAgeSec = maxAgeSec };
    }

    public static CacheControl NoCache { get { return new CacheControl { Cacheability = Type.NoCache, NoStore = true }; } }

    [Config] public Type Cacheability { get; set; }
    [Config] public int? MaxAgeSec { get; set; }
    [Config] public int? SharedMaxAgeSec { get; set; }

    [Config] public bool NoStore { get; set; }
    [Config] public bool NoTransform { get; set; }
    [Config] public bool MustRevalidate { get; set; }
    [Config] public bool ProxyRevalidate { get; set; }

    /// <summary>
    /// Build standard cache control header from fields
    /// </summary>
    public string HTTPCacheControl
    {
      get
      {
        var sb = new StringBuilder();

        switch (Cacheability)
        {
          case Type.Private: sb.Append("private, "); break;
          case Type.NoCache: sb.Append("no-cache, "); break;
          default: sb.Append("public, "); break;
        }
        if (Cacheability == Type.NoCache && NoStore) sb.Append("no-store, ");
        if (NoTransform) sb.Append("no-transform, ");
        if (Cacheability != Type.NoCache && MaxAgeSec.HasValue) sb.AppendFormat("max-age={0}, ", MaxAgeSec);
        if (Cacheability != Type.NoCache && SharedMaxAgeSec.HasValue) sb.AppendFormat("s-maxage={0}, ", SharedMaxAgeSec);
        if (MustRevalidate) sb.Append("must-revalidate, ");
        if (ProxyRevalidate) sb.Append("proxy-revalidate, ");
        return sb.ToString(0, sb.Length - 2);
      }
    }
  }
}
