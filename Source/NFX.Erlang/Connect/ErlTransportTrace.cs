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

namespace NFX.Erlang
{
  public enum Direction
  {
    Inbound,
    Outbound
  }

  public enum ErlTraceLevel
  {
    /// <summary>
    /// Tracing is off
    /// </summary>
    Off         = 0,

    /// <summary>
    /// Trace ordinary send and receive messages
    /// </summary>
    Send        = 1,

    /// <summary>
    /// Trace control messages (e.g. link/unlink)
    /// </summary>
    Ctrl        = 2,

    /// <summary>
    /// Trace handshaking at connection startup
    /// </summary>
    Handshake   = 3,

    /// <summary>
    /// Trace Epmd connectivity
    /// </summary>
    Epmd        = 4,

    /// <summary>
    /// Trace wire-level message content
    /// </summary>
    Wire        = 5
  }

  /// <summary>
  /// Debugging delegate called to be able to record transport-related events
  /// </summary>
  /// <param name="sender">Event sender</param>
  /// <param name="type">Type of trace event</param>
  /// <param name="dir">Event direction (in/out-bound)</param>
  /// <param name="message">Event detail</param>
  public delegate void TraceEventHandler(object sender, ErlTraceLevel type, Direction dir, string message);
}