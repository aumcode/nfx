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
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NFX.Erlang.Internal
{
  /// <summary>
  /// Server that handles inbound RPC calls
  /// </summary>
  internal class ErlRpcServer : DisposableObject
  {
    #region CONSTS

    private static readonly ErlAtom A = new ErlAtom("A");
    private static readonly ErlAtom F = new ErlAtom("F");
    private static readonly ErlAtom G = new ErlAtom("G");
    private static readonly ErlAtom M = new ErlAtom("M");
    private static readonly ErlAtom P = new ErlAtom("P");
    private static readonly ErlAtom R = new ErlAtom("R");
    private static readonly ErlAtom T = new ErlAtom("T");

    #endregion

    #region .ctor

    public ErlRpcServer(ErlLocalNode owner)
    {
      m_Active = true;
      Node = owner;
      Self = Node.CreateMbox(ConstAtoms.Rex);
      m_Thread = new Thread(start);
      m_Thread.Name = "{0} RPC".Args(owner.NodeName);
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
    //internal static readonly ErlTuple RpcReplyPattern = "{rex, T}".To<ErlTuple>();

    public readonly ErlLocalNode Node;
    public readonly ErlMbox Self;

    private bool m_Active;
    private Thread m_Thread;

    #endregion

    #region Internal

    internal static ErlTuple EncodeRPC(
        ErlPid from, string mod, string fun, ErlList args, IErlObject gleader)
    {
      return EncodeRPC(from, new ErlAtom(mod), new ErlAtom(fun), args, gleader);
    }

    internal static ErlTuple EncodeRPCcast(
        ErlPid from, string mod, string fun, ErlList args, IErlObject gleader)
    {
      return EncodeRPCcast(from, new ErlAtom(mod), new ErlAtom(fun), args, gleader);
    }

    internal static ErlTuple EncodeRPC(
        ErlPid from, ErlAtom mod, ErlAtom fun, ErlList args, IErlObject gleader)
    {
      /*{Self, {call, Mod, Fun, Args, GroupLeader}} */
      return new ErlTuple(from, new ErlTuple(ConstAtoms.Call, mod, fun, args, gleader));
    }

    internal static ErlTuple EncodeRPCcast(
        ErlPid from, ErlAtom mod, ErlAtom fun, ErlList args, IErlObject gleader)
    {
      /*{'$gen_cast', { cast, Mod, Fun, Args, GroupLeader}} */
      return new ErlTuple(
              ConstAtoms.GenCast,
              new ErlTuple(ConstAtoms.Cast, mod, fun, args, gleader));
    }

    internal static readonly ErlTuple RpcReplyPattern = "{rex, ~v}".ErlArgs(T).To<ErlTuple>();

    internal static IErlObject DecodeRPC(IErlObject msg)
    {
      var term = msg as ErlTuple;
      if (term == null)
        return null;

      var binding = term.Match(RpcReplyPattern);
      return binding != null ? binding[T] : null;
    }

    #endregion

    #region .pvt

    private void sendRpcReply(ErlPid from, ErlRef eref, IErlObject reply)
    {
      if (from.Empty) return;
      Node.Send(from, ErlTuple.Create(eref, reply));
    }

    private IErlObject rpcCall(ErlPid from, ErlRef eref,
        ErlAtom mod, ErlAtom fun, ErlList args, IErlObject groupLeader)
    {
      // We spawn a new task, so that RPC calls wouldn't block the RPC server thread
      Task.Factory.StartNew(() =>
      {
        var type = Type.GetType(mod);

        if (type == null)
        {
          sendRpcReply(from, eref,
              ErlTuple.Create(ConstAtoms.Error, "unknown type: {0}".Args(mod)));
          return;
        }

        // TODO: add LRU caching
        //var method = type.GetMethod(fun.Value, BindingFlags.Static | BindingFlags.Public);

        string methodName = fun.Value;

        if (args.Count == 0)
        {
          var pi = type.GetProperty(fun, BindingFlags.Static | BindingFlags.Public);
          if (pi != null)
          {
            try
            {
              var result = pi.GetValue(null, null);
              sendRpcReply(from, eref,
                  ErlTuple.Create(ConstAtoms.Ok, result.ToErlObject()));
            }
            catch (Exception e)
            {
              sendRpcReply(from, eref,
                  ErlTuple.Create(ConstAtoms.Error, new ErlString(e.Message)));
            };
            return;
          }
        }

        var mi = type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                     .Where(m => m.Name == methodName && m.GetParameters().Count() == args.Count)
                     .FirstOrDefault();

        if (mi == null)
        {
          sendRpcReply(from, eref,
              ErlTuple.Create(ConstAtoms.Error, "unknown method: {0}".Args(fun)));
          return;
        }

        var pars = mi.GetParameters();

        var margs = new object[pars.Length];

        for (int i = 0; i < pars.Length; i++)
        {
          var par = pars[i];
          var val = args[i];
          margs[i] = val.AsType(par.ParameterType);
        }

        try
        {
          var result = mi.Invoke(type, margs);
          sendRpcReply(from, eref, ErlTuple.Create(ConstAtoms.Ok, result.ToErlObject()));
        }
        catch (Exception e)
        {
          sendRpcReply(from, eref, ErlTuple.Create(ConstAtoms.Error, new ErlString(e.Message)));
        }
      });
      return (IErlObject)null;
    }

    private void start(object obj)
    {
      // For Erlang I/O protocol see:
      // http://erlang.org/doc/apps/stdlib/io_protocol.html

      var patterns = new ErlPatternMatcher
                {
                    {"{'$gen_call', {~v::pid(), ~v::ref()}, {call, ~v::atom(), ~v::atom(), ~v::list(), ~v}}".ErlArgs(P,R,M,F,A,G),
                        (_p, _t, b, a) => rpcCall(
                            b.Cast<ErlPid>(P), b.Cast<ErlRef>(R),
                            b.Cast<ErlAtom>(M), b.Cast<ErlAtom>(F), b.Cast<ErlList>(A), b[G]) },
                    {"{'$gen_cast', {cast, ~v::atom(), ~v::atom(), ~v::list(), ~v}}".ErlArgs(M,F,A,G),
                        (_p, _t, b, a) => rpcCall(
                            ErlPid.Null, ErlRef.Null,
                            b.Cast<ErlAtom>(M), b.Cast<ErlAtom>(F), b.Cast<ErlList>(A), b[G]) },
                    {"{_, {~v::pid(), ~v::ref()}, _Cmd}".ErlArgs(P,R),
                        (_p, _t, b, a) =>
                            ErlTuple.Create(b[P], b[R], ErlTuple.Create(ConstAtoms.Error, ConstAtoms.Unsupported))
                    },
                };

      while (m_Active)
      {

        Tuple<IErlObject, int> res = Self.ReceiveMatch(patterns, 1500);

        switch (res.Item2)
        {
          case -2: /* timeout  */
            break;
          case -1: /* no match */
            App.Log.Write(Log.MessageType.Warning,
                StringConsts.ERL_INVALID_RPC_REQUEST_ERROR, res.Item1.ToString());
            break;
          default:
            if (res.Item1 == null)
              break;
            var t = (ErlTuple)res.Item1;
            var pid = t.Cast<ErlPid>(0);
            var eref = t.Cast<ErlRef>(1);
            var reply = t[2];
            sendRpcReply(pid, eref, reply);
            break;
        }
      }

      App.Log.Write(
          Log.MessageType.Info, StringConsts.ERL_STOPPING_SERVER.Args(Node.NodeName, "RPC"));

      m_Thread = null;
    }

    #endregion
  }
}
