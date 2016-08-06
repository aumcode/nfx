using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using NFX.Serialization.Slim;

namespace NFX.Log.Bin
{
  /// <summary>
  /// Writes bin log in Slim format. The format does not support object versioning however it is
  /// very efficient in both space and speed
  /// </summary>
  public class SlimLogWriter : LogWriter
  {
    public SlimLogWriter(Stream stream) : base(stream)
    {

    }


  }
}
