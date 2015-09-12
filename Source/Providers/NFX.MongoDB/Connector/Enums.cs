using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.DataAccess.MongoDB.Connector
{
  /// <summary>
  /// Defines data safety modes http://docs.mongodb.org/manual/core/write-concern/
  /// </summary>
  public enum WriteConcern
  {
    Unacknowledged = -1,
    Acknowledged   = 0,
    Journaled = +1
  }
}
