using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Serialization.BSON
{
  /// <summary>
  /// Denotes types that support BSON serialization by GUID
  /// </summary>
  [AttributeUsage( AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
  public class BSONSerializableAttribute : GuidTypeAttribute
  {
    public BSONSerializableAttribute(string typeGuid) : base(typeGuid){ }
  }
}
