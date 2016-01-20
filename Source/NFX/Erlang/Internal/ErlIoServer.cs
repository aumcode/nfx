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
using System;
using System.Threading;

namespace NFX.Erlang.Internal
{
  /// <summary>
  /// I/O server processing distributed I/O operations from remote
  /// Erlang nodes
  /// </summary>
  /// <remarks>
  /// <see href="http://erlang.org/doc/apps/stdlib/io_protocol.html"/>
  /// </remarks>
  internal class ErlIoServer : DisposableObject
  {
  #region CONSTS

    private static readonly ErlAtom A = new ErlAtom("A");
    private static readonly ErlAtom C = new ErlAtom("C");
    private static readonly ErlAtom E = new ErlAtom("E");
    private static readonly ErlAtom F = new ErlAtom("F");
    private static readonly ErlAtom M = new ErlAtom("M");
    private static readonly ErlAtom N = new ErlAtom("N");
    private static readonly ErlAtom P = new ErlAtom("P");
    private static readonly ErlAtom R = new ErlAtom("R");
    private static readonly ErlAtom RA = new ErlAtom("RA");

  #endregion

  #region .ctor

    public ErlIoServer(ErlLocalNode owner)
    {
      m_Active = true;
      Node = owner;
      Self = Node.CreateMbox(ConstAtoms.User);
      m_Thread = new Thread(threadSpin);
      m_Thread.Name = "{0} I/O".Args(owner.NodeName);
      m_Thread.IsBackground = true;
      m_Thread.Start();
    }

    protected override void Destructor()
    {
      base.Destructor();
      if (m_Thread != null)
      {
        m_Active = false;
        m_Thread.Join();
        m_Thread = null;
      }
      Node.CloseMbox(Self);
    }

  #endregion

  #region Fields

    public readonly ErlLocalNode   Node;
    public  readonly ErlMbox       Self;

    private bool                   m_Active;
    private Thread                 m_Thread;

  #endregion

  #region .pvt

    private static readonly IErlObject s_RequestPattern =
        ErlObject.Parse("{io_request, F::pid(), RA, R::tuple()}");
    private static readonly IErlObject s_ReplyPattern =
        ErlObject.Parse("{io_reply, RA, R}");

    private IErlObject ioProcessPutChars(ErlAtom encoding,
        ErlString str, IErlObject replyAs)
    {
      Node.OnIoOutput(encoding, str);
      return s_ReplyPattern.Subst(
          new ErlVarBind { { RA, replyAs }, { R, ConstAtoms.Ok } });
    }

    private IErlObject ioProcessPutChars(ErlAtom encoding,
        ErlAtom mod, ErlAtom fun, ErlList args, IErlObject replyAs)
    {
      string term;
      if (mod == ConstAtoms.Io_Lib && fun == ConstAtoms.Format && args.Count == 2)
        try { term = ErlObject.Format(args); }
        catch { term = "{0}:{1}({2})".Args(mod, fun, args.ToString(true)); }
      else
        term = "{0}:{1}({2})".Args(mod, fun, args.ToString(true));
      Node.OnIoOutput(encoding, new ErlString(term));
      return s_ReplyPattern.Subst(
          new ErlVarBind { { RA, replyAs }, { R, ConstAtoms.Ok } });
    }

    private IErlObject ioProcessGetUntil(ErlAtom encoding,
        IErlObject prompt, ErlAtom mod, ErlAtom fun, ErlList args,
        IErlObject replyAs)
    {
      return s_ReplyPattern.Subst(
          new ErlVarBind{
                        {RA, replyAs},
                        {R, (IErlObject)Tuple.Create(ConstAtoms.Error, ConstAtoms.Request)}
          });
    }

    private IErlObject ioProcessGetChars(
        ErlAtom encoding, IErlObject prompt, IErlObject n, IErlObject replyAs)
    {
      return s_ReplyPattern.Subst(
          new ErlVarBind{
                        {RA, replyAs},
                        {R, (IErlObject)Tuple.Create(ConstAtoms.Error, ConstAtoms.Request)}
          });
    }

    private IErlObject ioProcessGetLine(
        ErlAtom encoding, IErlObject prompt, IErlObject replyAs)
    {
      return s_ReplyPattern.Subst(
          new ErlVarBind{
                        {RA, replyAs},
                        {R, (IErlObject)Tuple.Create(ConstAtoms.Error, ConstAtoms.Request)}
          });
    }

    private IErlObject ioProcessRequests(
        ErlPatternMatcher pm, ErlList requests, IErlObject replyAs)
    {

      foreach (var r in requests)
      {
        IErlObject term = r;
        if (pm.Match(ref term, replyAs) < 0)
          return term;
      }
      return s_ReplyPattern.Subst(
          new ErlVarBind{
                        {RA, replyAs},
                        {R, (IErlObject)Tuple.Create(ConstAtoms.Error, ConstAtoms.Request)
                    }});
    }

    private void threadSpin()
    {
      try
      {
        threadSpinCore();
      }
      catch(Exception error)
      {
        var em = new NFX.Log.Message
        {
          Type = Log.MessageType.CatastrophicError,
          Topic = CoreConsts.ERLANG_TOPIC,
          From = GetType().Name + "threadSpin()",
          Text = "threadSpinCore leaked: " + error.ToMessageWithType(),
          Exception = error
        };
        App.Log.Write(em);
      }
    }

    private void threadSpinCore()
    {
      // For Erlang I/O protocol see:
      // http://erlang.org/doc/apps/stdlib/io_protocol.html

      var patterns = new ErlPatternMatcher
                {
                    {"{put_chars, ~v::atom(), ~v::string()}".ErlArgs(E, C),
                        (_p, _t, b, a) => ioProcessPutChars(
                            b.Cast<ErlAtom>(E), b.Cast<ErlString>(C), (IErlObject)a[1]) },
                    {"{put_chars, ~v::atom(), ~v::atom(), ~v::atom(), ~v::list()}".ErlArgs(E,M,F,A),
                        (_p, _t, b, a) => ioProcessPutChars(
                            b.Cast<ErlAtom>(E), b.Cast<ErlAtom>(M),
                            b.Cast<ErlAtom>(F), b.Cast<ErlList>(A),
                            (IErlObject)a[1]) },
                    {"{put_chars, ~v::string()}".ErlArgs(C),
                        (_p, _t, b, a) => ioProcessPutChars(
                            ConstAtoms.Latin1, b.Cast<ErlString>(C), (IErlObject)a[1]) },
                    {"{put_chars, ~v::atom(), ~v::atom(), ~v::list()}".ErlArgs(M,F,A),
                        (_p, _t, b, a) => ioProcessPutChars(
                            ConstAtoms.Latin1, b.Cast<ErlAtom>(M),
                            b.Cast<ErlAtom>(F), b.Cast<ErlList>(A),
                            (IErlObject)a[1]) },
                    {"{get_until, ~v::atom(), ~v, ~v::atom(), ~v::atom(), ~v::list()}".ErlArgs(E,P,M,F,A),
                        (_p, _t, b, a) => ioProcessGetUntil(
                            b.Cast<ErlAtom>(E), b[P], b.Cast<ErlAtom>(M), b.Cast<ErlAtom>(F),
                            b.Cast<ErlList>(A), (IErlObject)a[1]) },
                    {"{get_chars, ~v::atom(), ~v, ~v::integer()}".ErlArgs(E,P,N),
                        (_p, _t, b, a) => ioProcessGetChars(
                            b.Cast<ErlAtom>(E), b[P], b.Cast<ErlLong>(N), (IErlObject)a[1]) },
                    {"{get_line, ~v::atom(), ~v}".ErlArgs(E,P),
                        (_p, _t, b, a) => ioProcessGetLine(
                            b.Cast<ErlAtom>(E), b[P], (IErlObject)a[1]) },
                    {"{get_until, ~v, ~v::atom(), ~v::atom(), ~v::list()}".ErlArgs(P,M,F,A),
                        (_p, _t, b, a) => ioProcessGetUntil(
                            ConstAtoms.Latin1, b[P], b.Cast<ErlAtom>(M), b.Cast<ErlAtom>(F),
                            b.Cast<ErlList>(A), (IErlObject)a[1]) },
                    {"{get_chars, ~v, ~v::integer()}".ErlArgs(P,N),
                        (_p, _t, b, a) => ioProcessGetChars(
                            ConstAtoms.Latin1, b[P], b.Cast<ErlLong>(N), (IErlObject)a[1]) },
                    {"{get_line, ~v}".ErlArgs(P),
                        (_p, _t, b, a) => ioProcessGetLine(
                            ConstAtoms.Latin1, b[P], (IErlObject)a[1]) },
                    {"{requests, ~v::list()}".ErlArgs(R),
                        (_p, _t, b, a) => ioProcessRequests(
                            (ErlPatternMatcher)a[0], b.Cast<ErlList>(R), (IErlObject)a[1]) },
                };

      while (m_Active)
      {
        Tuple<IErlObject, ErlVarBind> res = Node.GroupLeader.ReceiveMatch(s_RequestPattern, 1500);

        if (res.Item1 == null) // Timeout
          continue;
        else if (res.Item2 == null) // No match
          App.Log.Write(Log.MessageType.Error,
              StringConsts.ERL_INVALID_IO_REQUEST + res.Item1.ToString(), from: GetType().Name);

        var from = res.Item2.Cast<ErlPid>(F);
        var replyAs = res.Item2[RA];
        var request = res.Item2.Cast<ErlTuple>(R);
        var req = (IErlObject)request;

        if (patterns.Match(ref req, patterns, replyAs) >= 0)
        {
          Node.Send(from, req);
          continue;
        }

        var reply = s_ReplyPattern.Subst(new ErlVarBind { { RA, replyAs }, { R, ConstAtoms.Request } });

        Debug.Assert(reply != null);
        Node.Send(from, reply);
      }

      App.Log.Write(Log.MessageType.Info, StringConsts.ERL_STOPPING_SERVER.Args(Node.NodeLongName, "I/O"));

      m_Thread = null;
    }

  #endregion
  }
}
