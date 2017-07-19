using System;

using NFX;
using NFX.Web.Social;

namespace NFX.Web
{
  /// <summary>
  /// Provides utility functions for search and social bot detection
  /// </summary>
  public static class Crawlers
  {
    /// <summary>
    /// Returns true if the user agent represents a robot from any known social net or search crawler
    /// </summary>
    public static bool IsAnySearchCrawlerOrSocialBotUserAgent(string userAgent)
    {
      return IsSpecificSocialNetBotUserAgent(SocialNetID.UNS, userAgent) || IsAnySearchCrawler(userAgent);
    }

    /// <summary>
    /// Returns true if the user agent represents a robot from any known social net
    /// </summary>
    public static bool IsAnySocialNetBotUserAgent(string userAgent)
    {
      return IsSpecificSocialNetBotUserAgent(SocialNetID.UNS, userAgent);
    }

    /// <summary>
    /// Returns true if the user agent represents a crawler from any search engine
    /// </summary>
    public static bool IsAnySearchCrawler(string userAgent)
    {
      if (userAgent.IsNullOrWhiteSpace()) return false;

      userAgent = userAgent.TrimStart();

      if (userAgent.IndexOf("Googlebot", StringComparison.OrdinalIgnoreCase) > -1) return true;
      if (userAgent.IndexOf("Bingbot", StringComparison.OrdinalIgnoreCase) > -1) return true;
      if (userAgent.IndexOf("Slurp", StringComparison.OrdinalIgnoreCase) > -1) return true; //Yahoo
      if (userAgent.IndexOf("DuckDuckBot", StringComparison.OrdinalIgnoreCase) > -1) return true;
      if (userAgent.IndexOf("YandexBot", StringComparison.OrdinalIgnoreCase) > -1) return true;
      if (userAgent.IndexOf("Baiduspider", StringComparison.OrdinalIgnoreCase) > -1) return true;
      if (userAgent.IndexOf("Sogou", StringComparison.OrdinalIgnoreCase) > -1) return true;
      if (userAgent.IndexOf("Exabot", StringComparison.OrdinalIgnoreCase) > -1) return true;
      if (userAgent.IndexOf("facebot", StringComparison.OrdinalIgnoreCase) > -1) return true;
      if (userAgent.IndexOf("facebookexternalhit", StringComparison.OrdinalIgnoreCase) > -1) return true;
      if (userAgent.IndexOf("ia_archiver", StringComparison.OrdinalIgnoreCase) > -1) return true; //Alexa

      return false;
    }

    /// <summary>
    /// Returns true if the user agent represents a robot from the specified social net
    /// </summary>
    public static bool IsSpecificSocialNetBotUserAgent(SocialNetID net, string userAgent)
    {
      if (userAgent.IsNullOrWhiteSpace()) return false;

      userAgent = userAgent.TrimStart();

      if (net==SocialNetID.UNS || net==SocialNetID.TWT)
      {
        if (userAgent.IndexOf("Twitterbot", StringComparison.OrdinalIgnoreCase)==0) return true;
      }

      if (net == SocialNetID.UNS || net == SocialNetID.FBK)
      {
        if (userAgent.IndexOf("facebookexternalhit", StringComparison.OrdinalIgnoreCase) == 0 ||
            userAgent.IndexOf("Facebot", StringComparison.OrdinalIgnoreCase) == 0)
          return true;
      }

      if (net == SocialNetID.UNS || net == SocialNetID.GPS)
      {
        if (userAgent.IndexOf(@"Google (+https://developers.google.com/+/web/snippet/)", StringComparison.OrdinalIgnoreCase) != -1)
          return true;
      }

      if (net == SocialNetID.UNS || net == SocialNetID.LIN)
      {
        if (userAgent.IndexOf("LinkedInBot", StringComparison.OrdinalIgnoreCase) == 0) return true;
      }

      if (net == SocialNetID.UNS || net == SocialNetID.IGM)
      {
        if (userAgent.IndexOf("Instagram", StringComparison.OrdinalIgnoreCase) == 0) return true;
      }

      if (net == SocialNetID.UNS || net == SocialNetID.PIN)
      {
        if (userAgent.IndexOf(@"Pinterest/", StringComparison.OrdinalIgnoreCase) == 0 &&
            userAgent.IndexOf(@"+http://www.pinterest.com/", StringComparison.OrdinalIgnoreCase) != -1)
          return true;
      }

      if (net == SocialNetID.UNS || net == SocialNetID.VKT)
      {
        if (userAgent.IndexOf("vkShare;", StringComparison.OrdinalIgnoreCase) != -1 &&
            userAgent.IndexOf(@"vk.com/dev/Share", StringComparison.OrdinalIgnoreCase) != -1)
          return true;
      }

      if (net == SocialNetID.UNS || net == SocialNetID.ODN)
      {
        if (userAgent.IndexOf("OdklBot", StringComparison.OrdinalIgnoreCase) != -1) return true;
      }

      return false;
    }

  }
}
