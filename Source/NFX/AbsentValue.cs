using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX
{
  /// <summary>
  /// Represents a special value that signifies the absence of any entity/state/value/data.
  /// The instances may be stored in a cache to indicate that the key exists, but points to a non-existing "absent" entity.
  /// This is usefull for DB lookups, not to touch the backend for values that dont exist.
  /// Use AbsentValue.Instance singleton
  /// </summary>
  [Serializable]
  public sealed class AbsentValue
  {
    public static readonly AbsentValue Instance = new AbsentValue();

    private AbsentValue(){}

    public override int GetHashCode() { return 0; }
    public override bool Equals(object obj) { return obj is AbsentValue;}
    public override string ToString(){ return "[Absent]";}
  }
}
