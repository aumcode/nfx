using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Serialization.BSON;

namespace NFX.Log
{
  /// <summary>
  /// Marker interface for entities that can be stored in archives, such as access/telemetry logs
  /// </summary>
  public interface IArchiveLoggable : IBSONSerializable, IBSONDeserializable
  {

  }



}
