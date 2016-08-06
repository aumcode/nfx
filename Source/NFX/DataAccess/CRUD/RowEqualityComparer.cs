using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.DataAccess.CRUD
{
  /// <summary>
  /// Checks for reference equality. Use RowEqualityComparer.Instance
  /// </summary>
  public sealed class RowEqualityComparer : EqualityComparer<Row>
  {
      private static RowEqualityComparer s_Instance = new RowEqualityComparer();

      public static RowEqualityComparer Instance { get { return s_Instance;}}

      private RowEqualityComparer() {}


      public override bool Equals(Row x, Row y)
      {
        if (x==null && y==null) return true;
        if (x==null) return false;

        return x.Equals(y);
      }

      public override int GetHashCode(Row row)
      {
        if (row==null) return 0;
        return row.GetHashCode();
      }
  }
}
