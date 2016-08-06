using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Log.Bin
{

  /// <summary>
  /// Denotes entities that represent metadata - extra information about data such as timestamps and corruptions
  /// </summary>
  public interface ILogMetadata
  {

  }


  /// <summary>
  /// Denotes a special surrogate structure that wraps DatTime in log.
  /// These structures get inserted in log automatically by binary log writers to indicate te point in time
  /// </summary>
  public sealed class LogUTCTimeStamp : ILogMetadata
  {
    private LogUTCTimeStamp(){ }//for faster Slim ser
    internal LogUTCTimeStamp(DateTime utcValue)
    {
      Value = utcValue;
    }

    public readonly DateTime Value;
  }

  /// <summary>
  /// Denotes a special surrogate structure that indicates log corruption
  /// </summary>
  public sealed class LogCorruption : ILogMetadata
  {
    /// <summary>
    /// Singleton instance of LogCorruption
    /// </summary>
    public static readonly LogCorruption Instance = new LogCorruption();

    private LogCorruption(){ }//for faster Slim ser
  }
}
