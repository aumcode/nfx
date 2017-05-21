using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using NFX.Environment;

namespace NFX
{
  /// <summary>
  /// Describe an entity that resolve type guids into local CLR types
  /// </summary>
  public interface IGuidTypeResolver
  {
    /// <summary>
    /// Tries to resolves the GUID into type or return null
    /// </summary>
    Type TryResolve(Guid guid);


    /// <summary>
    /// Resolves the GUID into type or throws
    /// </summary>
    Type Resolve(Guid guid);
  }

  /// <summary>
  /// Provides information about the decorated type: assignes a globally-unique immutable type id
  /// </summary>
  public abstract class GuidTypeAttribute : Attribute
  {
    /// <summary>
    /// Returns TypeGuidAttribute for a type encapsulated in guid.
    /// If type is not decorated by the attribute then exception is thrown
    /// </summary>
    public static A GetGuidTypeAttribute<T, A>(Guid guid, IGuidTypeResolver resolver) where A : GuidTypeAttribute
    {
      if (resolver == null)
        throw new NFXException(StringConsts.ARGUMENT_ERROR + typeof(A).FullName + ".GetGuidTypeAttribute(resolver==null)");

      var type = resolver.Resolve(guid);
      return GetGuidTypeAttribute<T, A>(type);
    }

    /// <summary>
    /// Returns TypeGuidAttribute for a type.
    /// If type is not decorated by the attribute then exception is thrown
    /// </summary>
    public static A GetGuidTypeAttribute<T, A>(Type type) where A : GuidTypeAttribute
    {
      if (type == null || !typeof(T).IsAssignableFrom(type))
        throw new NFXException(StringConsts.ARGUMENT_ERROR + typeof(A).FullName + ".GetGuidTypeAttribute(todo=null|!" + typeof(T).FullName + ")");

      var result = getGuidTypeAttributeCore<A>(type);
      if (result == null)
        throw new NFXException(StringConsts.GUID_TYPE_RESOLVER_MISSING_ATTRIBUTE_ERROR.Args(type.FullName, typeof(A).FullName));

      return result;
    }

    private static Dictionary<Type, GuidTypeAttribute> s_Cache = new Dictionary<Type, GuidTypeAttribute>();
    private static A getGuidTypeAttributeCore<A>(Type type) where A : GuidTypeAttribute
    {
      GuidTypeAttribute result;
      if (!s_Cache.TryGetValue(type, out result))
      {
        var attrs = type.GetCustomAttributes(typeof(A), false);
        if (attrs.Length > 0) result = attrs[0] as A;
        if (result != null)
        {
          var dict = new Dictionary<Type, GuidTypeAttribute>(s_Cache);
          dict[type] = result;
          s_Cache = dict;//atomic
        }
      }
      return result as A;
    }

    public GuidTypeAttribute(string typeGuid)
    {
      Guid guid;
      if (!Guid.TryParse(typeGuid, out guid))
        throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().FullName + ".ctor(typeGuid unparsable)");

      TypeGuid = guid;
    }

    public readonly Guid TypeGuid;
  }

  /// <summary>
  /// Provides default type resolver implementation which looks for types in listed assemblies
  /// looking for types decorated with specified attribute
  /// </summary>
  public class GuidTypeResolver<T, A> : IGuidTypeResolver where T: class where A : GuidTypeAttribute
  {
    public const string CONFIG_ASSEMBLY_SECTION = "assembly";
    public const string CONFIG_NS_ATTR = "ns";


    public GuidTypeResolver(params Type[] types)
    {
       if (types==null || types.Length==0) throw new NFXException(StringConsts.ARGUMENT_ERROR + "GuidTypeResolver.ctor(types==null|Empty)");

       var mappings = types.ToDictionary( t => GuidTypeAttribute.GetGuidTypeAttribute<T, A>(t).TypeGuid, t => t );
       ctor(mappings);
    }

    public GuidTypeResolver(IDictionary<Guid, Type> mappings)
    {
      if (mappings==null || !mappings.Any()) throw new NFXException(StringConsts.ARGUMENT_ERROR + "GuidTypeResolver.ctor(mappings==null|Empty)");
      ctor(mappings);
    }

    private void ctor(IDictionary<Guid, Type> mappings)
    {
      if (mappings.Count != mappings.Distinct().Count())
        throw new NFXException(StringConsts.ARGUMENT_ERROR + "GuidTypeResolver.ctor(mappings has duplicates)");

      m_Cache = new Dictionary<Guid,Type>(mappings);
    }

    public GuidTypeResolver(IConfigSectionNode conf)
    {
      m_Cache = new Dictionary<Guid, Type>();

      if (conf == null || !conf.Exists) return;

      foreach (var nasm in conf.Children.Where(n => n.IsSameName(CONFIG_ASSEMBLY_SECTION)))
      {
        var asmName = nasm.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;
        var asmNS = nasm.AttrByName(CONFIG_NS_ATTR).Value;
        if (asmName.IsNullOrWhiteSpace()) continue;

        var asm = Assembly.LoadFrom(asmName);

        foreach (var type in asm.GetTypes().Where(t => t.IsPublic && t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(T))))
        {
          if (asmNS.IsNotNullOrWhiteSpace() && !NFX.Parsing.Utils.MatchPattern(type.FullName, asmNS)) continue;
          var atr = GuidTypeAttribute.GetGuidTypeAttribute<T, A>(type);

          Type existing;
          if (m_Cache.TryGetValue(atr.TypeGuid, out existing))
            throw new NFXException(StringConsts.GUID_TYPE_RESOLVER_DUPLICATE_ATTRIBUTE_ERROR.Args(type.FullName, atr.TypeGuid, existing.FullName));

          m_Cache.Add(atr.TypeGuid, type);
        }
      }

      if (m_Cache.Count == 0)
        App.Log.Write(new NFX.Log.Message
        {
          Type = NFX.Log.MessageType.Warning,
          Topic = CoreConsts.APPLICATION_TOPIC,
          From = "{0}.ctor()".Args(GetType().Name),
          Text = "No assemblies/types have been registered"
        });
    }

    private Dictionary<Guid, Type> m_Cache;


    /// <summary>
    /// Resolves the GUID into type object or return null
    /// </summary>
    public Type TryResolve(Guid guid)
    {
      Type result;
      if (m_Cache.TryGetValue(guid, out result)) return result;
      return null;
    }

    /// <summary>
    /// Resolves the GUID into type object or throws
    /// </summary>
    public Type Resolve(Guid guid)
    {
      Type result;
      if (m_Cache.TryGetValue(guid, out result)) return result;
      throw new NFXException(StringConsts.GUID_TYPE_RESOLVER_ERROR.Args(guid, typeof(T).Name));
    }
  }
}
