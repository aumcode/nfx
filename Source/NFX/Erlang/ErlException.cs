/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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
using System.Runtime.Serialization;
using System.Text;

namespace NFX.Erlang
{
  public class ErlException : NFXException, IQueable
  {
    public ErlException(string message) : base(message)
    { }

    public ErlException(string message, Exception inner) : base(message, inner)
    { }

    public ErlException(string message, params object[] args)
        : base(string.Format(message, args))
    { }

    public ErlException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
  }

  public class ErlIncompatibleTypesException : ErlException
  {
    public ErlIncompatibleTypesException(IErlObject lhs, Type rhs)
        : base(StringConsts.ERL_CANNOT_CONVERT_TYPES_ERROR, lhs.GetType().Name, rhs.Name)
    { }

    public ErlIncompatibleTypesException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
  }

  /// <summary>
  /// Exception thrown when decoding an Erlang term if there's not enough
  /// data in the buffer to construct the term
  /// </summary>
  public class NotEnoughDataException : ErlException
  {
    public NotEnoughDataException() : base(StringConsts.ERL_CANNOT_READ_FROM_STREAM_ERROR) { }
    public NotEnoughDataException(string reason) : base(reason) { }
  }

  /// <summary>
  /// Exception thrown when the connection with a given node gets broken
  /// </summary>
  public class ErlConnectionException : ErlException
  {
    public ErlConnectionException(ErlAtom nodeName, string reason)
        : base(string.Empty)
    {
      Node = nodeName;
      Reason = new ErlString(reason);
    }

    public ErlConnectionException(ErlAtom nodeName, IErlObject reason)
        : base(string.Empty)
    {
      Node = nodeName;
      Reason = reason;
    }

    /// <summary>
    /// Name of the node that experienced connectivity loss
    /// </summary>
    public readonly ErlAtom Node;

    /// <summary>
    /// Get the reason associated with this exit signal
    /// </summary>
    public readonly IErlObject Reason;

    public override string Message { get { return Reason.ToString(); } }
  }

  public class ErlBadDataException : ErlConnectionException
  {
    public ErlBadDataException(ErlAtom nodeName, string reason)
        : base(nodeName, reason)
    { }

    public ErlBadDataException(ErlAtom nodeName, IErlObject reason)
        : base(nodeName, reason)
    { }
  }

  /// <summary>
  /// Special message sent when a linked pid dies
  /// </summary>
  internal class ErlExit : ErlConnectionException
  {
    public ErlExit(ErlPid pid, string reason) : this(pid, (IErlObject)new ErlString(reason)) { }

    public ErlExit(ErlPid pid, IErlObject reason)
        : base(pid.Node, reason)
    {
      Pid = pid;
    }

    /// <summary>
    /// The pid that sent this exit
    /// </summary>
    public readonly ErlPid Pid;
  }

  /// <summary>
  /// Message sent when the monitored Pid dies
  /// </summary>
  internal class ErlDown : ErlExit
  {
    public ErlDown(ErlRef eref, ErlPid pid, IErlObject reason)
        : base(pid, reason)
    {
      Ref = eref;
    }

    public readonly ErlRef Ref;
  }

  public class ErlAuthException : ErlConnectionException
  {
    public ErlAuthException(ErlAtom nodeName, string reason) : base(nodeName, reason) { }
    public ErlAuthException(ErlAtom nodeName, IErlObject reason) : base(nodeName, reason) { }
  }
}
