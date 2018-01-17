/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
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
