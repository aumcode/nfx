using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.DataAccess.CRUD.Subscriptions
{
  /// <summary>
  /// Represents a mapping of the external channel type token to CLR Row type
  /// </summary>
  public sealed class RowTypeMapping : INamed, IOrdered
  {
    public string Name
    {
      get { throw new NotImplementedException(); }
    }

    public int Order
    {
      get { throw new NotImplementedException(); }
    }
  }
}
