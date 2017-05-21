using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Environment;

namespace NFX.Serialization.BSON
{

  /// <summary>
  /// Resolves types decorated with BSONSerializableAttribute(guid) attribute
  /// </summary>
  public sealed class BSONTypeResolver : GuidTypeResolver<object, BSONSerializableAttribute>
  {
    public BSONTypeResolver(params Type[] types): base(types){}
    public BSONTypeResolver(IDictionary<Guid, Type> mappings): base(mappings){}
    public BSONTypeResolver(IConfigSectionNode conf) : base(conf){}
  }


  /// <summary>
  /// Provides a scope of fake BSOnWritable parent which indicates what types are known
  /// this way the serilizer does not have to emit the TypeIDFieldName element.
  /// Used with root serializations i.e. to database, when storing log messages in the table,
  /// it is not necessary to store Log.Message type ID in every record.
  /// Pass the instance to serializer.Serialize(parent: instance).
  /// Pre-allocate known types before calling serializer.Deserialize(result: instance)
  /// </summary>
  public sealed class BSONParentKnownTypes : IBSONSerializable
  {
    /// <summary>
    /// Specifies the types which are knwon to the parent calling scope, consequently the
    /// TypeIDFieldNamewill not be emitted to doc.
    /// Use in cases like writing to database table, where all root types are known by design
    /// </summary>
    public BSONParentKnownTypes(params Type[] known) {  KnownTypes = known; }

    private IEnumerable<Type> KnownTypes;

    public void SerializeToBSON(BSONSerializer serializer, BSONDocument doc, IBSONSerializable parent, ref object context)
    {
      throw new NotSupportedException();
    }

    public bool IsKnownTypeForBSONDeserialization(Type type)
    {
      return KnownTypes.Contains( type );
    }
  }


  /// <summary>
  /// Provides serialization/deserialization of types that support direct BSON serialization/deserialization -
  ///  implement IBSONSerializable/IBSONDeserializable
  /// </summary>
  public class BSONSerializer
  {
    public const string CONFIG_RESOLVER_SECTON = "type-resolver";
    public const string DEFAULT_TYPE_ID_FIELD = "__t";


    public BSONSerializer()
    {
      m_Resolver = new BSONTypeResolver((IConfigSectionNode) null);
    }

    public BSONSerializer(IConfigSectionNode node)
    {
      if (node == null || !node.Exists) throw new BSONException(StringConsts.ARGUMENT_ERROR + "BSONSerializer.ctor(node==null|!Exists)");
      ConfigAttribute.Apply(this, node);

      m_Resolver = new BSONTypeResolver(node[CONFIG_RESOLVER_SECTON]);
    }

    public BSONSerializer(BSONTypeResolver resolver)
    {
      if (resolver==null)
        throw new BSONException(StringConsts.ARGUMENT_ERROR + "BSONSerializer.ctor(resolver==null)");

      m_Resolver = resolver;
    }


    private string m_TypeIDFieldName;
    protected BSONTypeResolver m_Resolver;

    /// <summary>
    /// Defines the purpose/level of detail of serialization
    /// </summary>
    [Config]
    public BSONSerializationPurpose Purpose { get; set;}

    /// <summary>
    /// Returns the name of the field which is used to store type ID, by default DEFAULT_TYPE_ID_FIELD/"__t" is assumed
    /// </summary>
    [Config]
    public string TypeIDFieldName
    {
      get { return m_TypeIDFieldName.IsNullOrWhiteSpace() ? DEFAULT_TYPE_ID_FIELD : m_TypeIDFieldName; }
      set { m_TypeIDFieldName = value;}
    }

    /// <summary>
    /// Adds a field named TypeIDFieldName with the type id of the object instance, force=true to emit the field even if it is a known type.
    /// Returns true if type id element was added
    /// </summary>
    public virtual bool AddTypeIDField(BSONDocument doc, IBSONSerializable parent, IBSONSerializable self, object ctx, bool force = false)
    {
      if (doc==null) return false;
      if (self==null) return false;

      var add = force || parent==null;
      var t = self.GetType();
      if (!add)
        add = !parent.IsKnownTypeForBSONDeserialization(t);

      if (add)
      {
        var id = BSONSerializableAttribute.GetGuidTypeAttribute<object, BSONSerializableAttribute>(t)
                                          .TypeGuid
                                          .ToString("N");

        var telm = new BSONStringElement(TypeIDFieldName, id);
        doc.Set( telm );
        return true;
      }

      return false;
    }

    /// <summary>
    /// Serializes an instance directly into BSONDocument
    /// </summary>
    public BSONDocument Serialize(IBSONSerializable payload, IBSONSerializable parent = null)
    {
      if (payload==null) return null;

      object ctx = null;
      var result = new BSONDocument();
      try
      {
        payload.SerializeToBSON(this, result, parent, ref ctx);
      }
      catch(Exception error)
      {
        throw new BSONException(StringConsts.BSON_SERIALIZER_STB_ERROR.Args(error.ToMessageWithType()), error);
      }
      return result;
    }

    /// <summary>
    /// Deserilizes an instance from BSONDocument.
    /// If instance is not pre-allocated (result=null) tries to create a type
    /// decorated with BSONSerializableAttribute(guid)
    /// </summary>
    public IBSONDeserializable Deserialize(BSONDocument doc, IBSONDeserializable result = null)
    {
      object ctx = null;

      if (result==null)
      {
        var telm = doc[TypeIDFieldName] as BSONStringElement;

        if (telm==null)
          throw new BSONException(StringConsts.BSON_DESERIALIZER_DOC_MISSING_TID_ERROR.Args(TypeIDFieldName));

        var tid = telm.Value.AsGUID(Guid.Empty);

        if (tid==Guid.Empty)
          throw new BSONException(StringConsts.BSON_DESERIALIZER_DOC_TID_GUID_ERROR.Args(TypeIDFieldName, telm.Value));

        try
        {
          var tresult = m_Resolver.Resolve(tid);
          result = SerializationUtils.MakeNewObjectInstance(tresult) as IBSONDeserializable;
          if (result==null) throw new BSONException("result isnot IBSONDeserializable");
        }
        catch(Exception error)
        {
          throw new BSONException(StringConsts.BSON_DESERIALIZER_MAKE_TYPE_ERROR.Args(tid, error.ToMessageWithType()), error);
        }
      }

      try
      {
        result.DeserializeFromBSON(this, doc, ref ctx);
      }
      catch(Exception error)
      {
        throw new BSONException(StringConsts.BSON_DESERIALIZER_DFB_ERROR.Args(error.ToMessageWithType()), error);
      }

      return result;
    }
  }
}
