using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Serialization.BSON
{

  /// <summary>
  /// Specifies the purpose of BSON serialization so the level of detail may be dynamically adjusted.
  /// Depending on this parameter IBSONWritable implementors may include additional details
  /// that are otherwise not needed
  /// </summary>
  public enum BSONSerializationPurpose
  {
      Unspecified = 0,

      /// <summary>
      /// UI Interface feeding - include only the data needed to show to the user
      /// </summary>
      UIFeed,

      /// <summary>
      /// Include as much data as possible for remote object reconstruction
      /// </summary>
      Marshalling
  }


  /// <summary>
  /// Denotes entities which can write/serialize their state directly into BSONDocument
  /// </summary>
  public interface IBSONSerializable
  {
    void SerializeToBSON(BSONSerializer serializer, BSONDocument doc, IBSONSerializable parent, ref object context);

    /// <summary>
    /// Return true to state that the supplied type is known by IBSONDeserializable implementation
    /// and its type id should not be added to the document
    /// </summary>
    bool IsKnownTypeForBSONDeserialization(Type type);
  }


  /// <summary>
  /// Denotes entities which can read/deserialize their state directly from BSONDocument
  /// </summary>
  public interface IBSONDeserializable
  {
    void DeserializeFromBSON(BSONSerializer serializer, BSONDocument doc, ref object context);
  }

}
