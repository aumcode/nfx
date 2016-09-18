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
using NFX;
using NFX.ApplicationModel;
using NFX.Environment;
using NFX.Erlang;

namespace ErlangNode
{
    class Program
    {
        static void Main(string[] args)
        {
            Configuration argsConfig = new CommandArgsConfiguration(args);

            if (argsConfig.Root[CommonApplicationLogic.CONFIG_SWITCH].Exists)
                using(new ServiceBaseApplication(args, null))
                    run(App.ConfigRoot, true);
            else
                run(argsConfig.Root, false);

        }

        private static void run(IConfigSectionNode argsConfig, bool hasConfigFile)
        {
            //try to read from  /config file
            var localNodeName  = argsConfig["local"].AttrByIndex(0).Value;
            var remoteNodeName = argsConfig["remote"].AttrByIndex(0).Value;
            var cookie = new ErlAtom(argsConfig["cookie"].AttrByIndex(0).ValueAsString(string.Empty));
            var trace = (ErlTraceLevel)Enum.Parse(typeof(ErlTraceLevel), argsConfig["trace"].AttrByIndex(0).ValueAsString("Off"), true);
            var timeout = argsConfig["timeout"].AttrByIndex(0).ValueAsInt(120);

            if (!hasConfigFile && (localNodeName == null || remoteNodeName == null))
            {
                Console.WriteLine("Usage: {0} [-config ConfigFile.config] [-local NodeName -remote NodeName]\n" +
                                  "          [-cookie Cookie] [-trace Level] [-timeout Timeout]\n\n" +
                                  "     -config ConfFile    - Provide configuration file\n" +
                                  "     -trace Level        - Turn on trace for level:\n" +
                                  "                             Off (default) | Send | Ctrl | Handshake | Wire\n" +
                                  "     -timeout Timeout    - Wait for messages for this number of seconds before\n" +
                                  "                              exiting (default: 15)\n" +
                                  "Example:\n" +
                                  "========\n" +
                                  "  [Shell A] $ erl -sname a\n" +
                                  "  [Shell B] $ {0} -local b -remote a -trace Send -timeout 60\n\n" +
                                  "  In the Erlang shell send messages to the C# node:\n" +
                                  "  [Shell A] (a@localhost)1> {{test, b@localhost}} ! \"Hello World!\".\n"
                                  , MiscUtils.EntryExeName(false));
                Environment.Exit(1);
            }

            // Create an local Erlang node that will process all communications with other nodes

            var node = hasConfigFile ? ErlApp.Node : new ErlLocalNode(localNodeName, cookie, true);

            node.Trace += (_, t, l, m) =>
                Console.WriteLine("[TRACE {0}]   {1} {2}", t, l == Direction.Inbound ? "<-" : "->", m);

            node.NodeStatusChange += (_, n, up, info) =>
                Console.WriteLine("<NodeStatus>  Node {0} {1} ({2})", n.Value, up ? "up" : "down", info);

            node.ConnectAttempt += (_, n, dir, info) =>
                Console.WriteLine("<ConnAttempt> Node {0}: {1} connection {2}", n, dir.ToString().ToLower(), info);

            node.EpmdFailedConnectAttempt += (_, n, info) =>
                Console.WriteLine("<EmpdFailure> Node {0} Epmd connectivity failure: {1}", n, info);
            
            node.UnhandledMsg += (_, c, msg) =>
                Console.WriteLine("<UnhandMsg>   Node {0} unhandled message from node {1}: {2}", c.LocalNode.NodeName, c.RemoteNode.NodeName, msg);

            node.ReadWrite += (_, c, d, n, tn, tm) =>
                Console.WriteLine("<ReadWrite>   {0} {1} bytes (total: {2} bytes, {3} msgs)", d == Direction.Inbound ? "Read" : "Written", n, tn, tm);

            node.IoOutput += (_, _encoding, output) =>
                Console.WriteLine("<I/O output>  ==> Received output: {0}", output);

            // Create a named mailbox "test"
            ErlMbox mbox = null;

            if (hasConfigFile)
                mbox = node.CreateMbox("test");
            else
            {
                node.TraceLevel = trace;
                Console.WriteLine("Node = {0}, cookie = {1}", node.NodeName, node.Cookie);

                // Start the node

                try
                {
                    node.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                    goto exit;
                }

                mbox = node.CreateMbox("test");

                // Connect to remote node

                var remote = node.Connection(remoteNodeName);

                Console.WriteLine("{0} to remote node {1}".Args(
                    remote == null ? "Couldn't connect" : "Connected",
                    remoteNodeName));

                if (remote == null)
                {
                    Console.WriteLine("Couldn't connect to {0}", remoteNodeName);
                    goto exit;
                }

                // Synchronous RPC call of erlang:now() at the remote node
                /* new ErlAtom("erlang") */
                /* new ErlList() */
                var result = mbox.RPC(remote.Name, ConstAtoms.Erlang, new ErlAtom("now"), ErlList.Empty);

                Console.WriteLine("RPC call to erlang:now() resulted in response: {0}", result.ValueAsDateTime.ToLocalTime());

                // Asynchronous RPC call of erlang:now() at the remote node

                mbox.AsyncRPC(remote.Name, ConstAtoms.Erlang, new ErlAtom("now"), ErlList.Empty);

                int i = node.WaitAny(mbox);

                if (i < 0)
                {
                    Console.WriteLine("Timeout waiting for RPC result");
                    goto exit;
                }

                result = mbox.ReceiveRPC();

                Console.WriteLine("AsyncRPC call to erlang:now() resulted in response: {0}", result.ValueAsDateTime.ToLocalTime());

                // I/O output call on the remote node of io:format(...)
                // that will return the output to this local node (because by default RPC
                // passes node.GroupLeader as the mailbox to receive the output

                var mfa = ErlObject.ParseMFA("io:format(\"output: 1, 10.0, abc\n\", [])");

                result = mbox.RPC(remote.Name, mfa.Item1, mfa.Item2, mfa.Item3);

                Console.WriteLine("io:format() -> {0}", result.ToString());

                // Poll for incoming messages destined to the 'test' mailbox
            }

            var deadline = DateTime.UtcNow.AddSeconds(timeout);

            do
            {
                var result = mbox.Receive(1000);
                if (result != null)
                    Console.WriteLine("Mailbox {0} got message: {1}", mbox.Self, result);
            }
            while (DateTime.UtcNow < deadline);

        exit:
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }
}
