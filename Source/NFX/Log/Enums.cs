/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 0.3  2009.10.12
 */

namespace NFX.Log
{
  /// <summary>
  /// Stipulates general logging message types like: Info/Warning/Error etc...
  /// </summary>
  public enum MessageType
  {
    /// <summary>
    /// Used in debugging temp code
    /// </summary>
    Debug = 0,

    DebugA,
    DebugB,
    DebugC,
    DebugD,

    /// <summary>
    /// Emitted by DataStore implementations
    /// </summary>
    DebugSQL,

    /// <summary>
    /// Emitted by Glue/Net code
    /// </summary>
    DebugGlue,

    /// <summary>
    /// Last debug-related message type for use in debug-related max-level config setting
    /// </summary>
    DebugZ,

    /// <summary>
    /// Tracing, no danger to system operation
    /// </summary>
    Trace = 50,

    TraceA,
    TraceB,
    TraceC,
    TraceD,

    /// <summary>
    /// Emitted by DataStore implementations
    /// </summary>
    TraceSQL,

    /// <summary>
    /// Emitted by Glue/Net code
    /// </summary>
    TraceGlue,

    /// <summary>
    /// Emitted by Erlang code
    /// </summary>
    TraceErl,

    /// <summary>
    /// Last trace-related message type for use in trace-related max-level config setting
    /// </summary>
    TraceZ,

    /// <summary>
    /// Performance/Instrumentation-related message
    /// </summary>
    PerformanceInstrumentation = 90,

    /// <summary>
    /// Informational message, no danger to system operation
    /// </summary>
    Info = 100,

    InfoA,
    InfoB,
    InfoC,
    InfoD,

    /// <summary>
    /// Last info-related message type for use in info-related max-level config setting
    /// </summary>
    InfoZ,

    /// <summary>
    /// Permission audit, usualy a result of client user action, no danger to system operation
    /// </summary>
    SecurityAudit = 200,

    /// <summary>
    /// SYSLOG.Notice Events that are unusual but not error conditions - might be summarized in an email to developers or admins to spot potential problems - no immediate action required.
    /// </summary>
    Notice = 300,

    /// <summary>
    /// Caution - inspect and take action.
    /// SYSLOG.Warning, not an error, but indication that an error will occur if action is not taken, e.g. file system 85% full - each item must be resolved within a given time.
    /// </summary>
    Warning = 400,

    /// <summary>
    /// Recoverable error, system will most-likely continue working normally.
    /// SYSLOG.Error Non-urgent failures, these should be relayed to developers or admins; each item must be resolved within a given time.
    /// </summary>
    Error = 500,

    /// <summary>
    /// SYSLOG.Critical Should be corrected immediately, but indicates failure in a primary system, an example is a loss of a backup ISP connection.
    /// </summary>
    Critical = 600,

    /// <summary>
    /// SYSLOG.Alert Should be corrected immediately, therefore notify staff who can fix the problem. An example would be the loss of a primary ISP connection.
    /// </summary>
    CriticalAlert = 700,

    /// <summary>
    /// Unrecoverable error, system  will most-likely experience major operation disruption.
    /// SYSLOG.Emergency - A "panic" condition usually affecting multiple apps/servers/sites. At this level it would usually notify all tech staff on call.
    /// </summary>
    Emergency = 1000,

    /// <summary>
    /// Unrecoverable error, system  will experience major operation disruption.
    /// SYSLOG.Emergency - A "panic" condition usually affecting multiple apps/servers/sites. At this level it would usually notify all tech staff on call.
    /// </summary>
    CatastrophicError = 2000
  }


}