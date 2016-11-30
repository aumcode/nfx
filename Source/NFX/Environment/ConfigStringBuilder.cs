using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Environment
{

  /// <summary>
  /// Denotes objects that can build a string (i.e. a database connection string) from the configured
  /// state/properties. This is used for example to build DB connect strings from host names that need
  /// to be resolved via an external system
  /// </summary>
  public interface IConfigStringBuilder : IConfigurable
  {
    /// <summary>
    /// Builds the string based on the configured state
    /// </summary>
    string BuildString();
  }


  /// <summary>
  /// Facilitates the creation/building of strings from existing strings or configuration vectors.
  /// This is used for example to build DB connect strings from host names that need
  /// to be resolved via an external system
  /// </summary>
  public static class ConfigStringBuilder
  {
    /// <summary>
    /// Builds the string based on the configured state supplied as a config vector, or
    /// passes the supplied string through if it is not a loaconic vector with CONFIG_BUILDER_ROOT
    /// </summary>
    /// <param name="source">The original source which may be IConfigStringBuilder injector or attribute value</param>
    /// <returns>The original attribute string value or the string returnd by IConfigStringBuilder.BuildString() method if IConfigStringBuilder was specified</returns>
    public static string Build(IConfigNode source)
    {
      if (source == null || !source.Exists) return string.Empty;

      if (source is IConfigAttrNode) return source.Value;

      var root = (IConfigSectionNode)source;

      var builder = FactoryUtils.MakeAndConfigure<IConfigStringBuilder>(root);
      return builder.BuildString();
    }

    /// <summary>
    /// Builds the string based on the configured state supplied as a config vector, or
    /// passes the supplied string through if it is not a loaconic vector with CONFIG_BUILDER_ROOT
    /// </summary>
    /// <param name="level">The configuration section level</param>
    /// <param name="sectionOrAttributeName">The name of section or attribute at the specified level</param>
    /// <returns>The original attribute string value or the string returnd by IConfigStringBuilder.BuildString() method if IConfigStringBuilder was specified</returns>
    public static string Build(IConfigSectionNode level, string sectionOrAttributeName)
    {
      if (level == null || !level.Exists) return string.Empty;

      IConfigNode source = level[sectionOrAttributeName];
      if (!source.Exists) source = level.AttrByName(sectionOrAttributeName);
      return Build(source);
    }
  }
}
