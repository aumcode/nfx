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

// Configure Windows 8.1 Undefined Cable Network to be Private (PowerShell/PSH)
// 1. Get-NetConnectionProfile
// 2. Set-NetConnectionProfile -InterfaceIndex [INDEX_FROM_1.] -NetworkCategory Private

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

using System.Threading.Tasks;


using BusinessLogic;
using NFX.WinForms;
using NFX.Glue;
using NFX.Glue.Protocol;
using NFX.Security;
using NFX;
using System.Collections.Concurrent;
using System.Threading;

namespace WinFormsTest
{     
  #pragma warning disable 0649,0169,0067
 
    public partial class GlueForm : Form
    {
        public class BadContract : ClientEndPoint
        {
           public BadContract(string node, NFX.Glue.Binding binding = null) : base(node, binding) {}
     
           public override Type Contract
           {
               get { return typeof(BadContract); }
           }

          #region Contract Methods
       
            public string Echo(string text)
            {            
              return Async_Echo(text).GetValue<string>();
            }

            public CallSlot Async_Echo(string text)
            {
              var request = new RequestAnyMsg(GetType().GetMethod("Echo"), RemoteInstance, new object[]{ text }); 
              return DispatchCall(request);  
            }

            public void Notify(string text)
            {
              var call = Async_Notify(text);
              if (call.CallStatus!= CallStatus.Dispatched)
                throw new ClientCallException(call.CallStatus);
            }

            public CallSlot Async_Notify(string text)
            {
              var request = new RequestAnyMsg(GetType().GetMethod("Notify"), RemoteInstance, new object[]{ text }); 
                                         
              return DispatchCall(request);  
            }
      
          #endregion
        }

       
        public GlueForm()
        {
            InitializeComponent();
            cbo.SelectedIndex = 0;
        }

        private void warmup()
        {
          var lst = new List<Task>();
          for(var ti=0; ti<Environment.ProcessorCount*8; ti++)
           lst.Add( Task.Factory.StartNew( 
             () =>
             {
               long sum = 0;
               for(var k=0; k<500000000; k++)
                sum +=k;
              return sum;
             }
             ));

          Task.WaitAll(lst.ToArray());
          GC.Collect(2);
        }


        int ECHO_COUNT = 0;

        private void button1_Click(object sender, EventArgs ea)
        {
          ECHO_COUNT++;

          var client = new JokeContractClient(cbo.Text);
          
          client.Headers.Add( new AuthenticationHeader( new IDPasswordCredentials(tbID.Text, tbPwd.Text)) );

          
          try
          {
            var echoed = chkUnsecureEcho.Checked ? client.UnsecureEcho("Hello!") : client.Echo("Hello!");
            Text = echoed + "  " + ECHO_COUNT.ToString() + " times";
          }
          catch (Exception e)
          {
            Text = e.ToMessageWithType();
          }

          client.Dispose();
        }

        private void button2_Click(object sender, EventArgs ea)
        {
          var CNT = edRepeat.Text.AsInt();
         
          var client = new JokeContractClient(cbo.Text);

          client.Headers.Add( new AuthenticationHeader( new IDPasswordCredentials(tbID.Text, tbPwd.Text)) );
          
        //  client.ReserveTransport = true;
          var w = Stopwatch.StartNew();

          try
          {
            if (chkUnsecureEcho.Checked)
            {
                for(int i=0; i<CNT; i++)
                 client.UnsecureEcho("Hello!");
            }
            else
            {
               for(int i=0; i<CNT; i++)
                 client.Echo("Hello!");
            }

            w.Stop();
            Text = "Echoed  "+CNT.ToString()+" in " + w.ElapsedMilliseconds + " ms";
          }
          catch (Exception e)
          {
            Text = e.ToMessageWithType();
          }
          
          client.Dispose();
        }

        int NOTIFY_COUNT = 0;

        private void button3_Click(object sender, EventArgs ea)
        {
          NOTIFY_COUNT++;

          var client = new JokeContractClient(cbo.Text);

          try
          {
            client.Notify("Notify!");
            Text = "Notified " + NOTIFY_COUNT.ToString() + " times";
          }
          catch (Exception e)
          {
            Text = e.ToMessageWithType();
          }          
         
          client.Dispose();
        }

        private void button4_Click(object sender, EventArgs ea)
        {
          var CNT = edRepeat.Text.AsInt();
         
          var client = new JokeContractClient(cbo.Text);
          
         // client.ReserveTransport = true;
          var w = Stopwatch.StartNew();
          int i = 0;
          try
          {
            for(; i<CNT; i++)
              client.Notify("Notify!");

            w.Stop();
            Text = "Notified  "+CNT.ToString()+" in " + w.ElapsedMilliseconds + " ms";
          }
          catch (Exception e)
          {
            Text = string.Format("After {0} times: {1}", i, e.ToMessageWithType());
          }          

          client.Dispose();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private JokeCalculatorClient calc = null;

        private void btnInit_Click(object sender, EventArgs e)
        {
            calc = new JokeCalculatorClient(cbo.Text);
            calc.Init(tbCalc.Text.AsInt());
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            Text = calc.Add(tbCalc.Text.AsInt()).ToString();
        }

        private void btnSub_Click(object sender, EventArgs e)
        {
            Text = calc.Sub(tbCalc.Text.AsInt()).ToString();
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            Text = calc.Done().ToString();
            calc.Dispose();
        }

        private void edRepeat_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void btnReactor_Click(object sender, EventArgs e)
        {
            using(var client1 = new JokeContractClient(cbo.Text))
              using (var client2 = new JokeContractClient(cbo.Text))
                {

                    var result = "";

                    var reactor = new CallReactor(false,
                         new Call( client1.Async_Echo("One."),  (r, call) => result += call.CallSlot.GetValue<string>()),
                         new Call( client2.Async_Echo("Two."),  (r, call) => result += call.CallSlot.GetValue<string>()),
                         new Call( client1.Async_Echo("Three."),(r, call) => result += call.CallSlot.GetValue<string>())
                    );

                    reactor.Wait();
                    
                    MessageBox.Show( result );
               }
        }

        private void btnReactor2_Click(object sender, EventArgs e)
        {
              var client1 = new JokeContractClient(cbo.Text);
              var client2 = new JokeContractClient(cbo.Text);

                
                    new CallReactor(false,
                        finishedReactor => 
                        {
                           client1.Dispose(); 
                           client2.Dispose();
                           Invoke( (Action)( () => MessageBox.Show(finishedReactor.Context.ToString()) ) );
                        },
                        string.Empty,
                        new Call( client1.Async_Echo("One."),  (reactor, call) => reactor.Context = ((string)reactor.Context) + call.CallSlot.GetValue<string>()),
                        new Call( client2.Async_Echo("Two."),  (reactor, call) => reactor.Context = ((string)reactor.Context) + call.CallSlot.GetValue<string>()),
                        new Call( client1.Async_Echo("Three."),(reactor, call) => reactor.Context = ((string)reactor.Context) + call.CallSlot.GetValue<string>())
                    );

        }

        private void btnBadMsg_Click(object sender, EventArgs ea)
        {
            var client = new BadContract(cbo.Text);

            try
            {
                var echoed = client.Echo("Hello!");
                Text = echoed + "  " + ECHO_COUNT.ToString() + " times";
            }
            catch (Exception e)
            {
                Text = e.ToMessageWithType();
            }

            client.Dispose();
        }


            private class BadClass
            {
                public event EventHandler Click;

                public BadClass()
                {
                }
            }

        private void btnBadPayload_Click(object sender, EventArgs ea)
        {
          var client = new JokeContractClient(cbo.Text);

          try
          {
            var bad = new BadClass();
            bad.Click += btnBadMsg_Click;
            var echoed = client.ObjectWork(bad);
            Text = echoed.ToString();
          }
          catch (Exception e)
          {
            Text = e.ToMessageWithType();
          }

          client.Dispose();
        }

        private void btnManyParallel_Click(object sender, EventArgs e)
        {
            Text = "Working...";
           
            var w = Stopwatch.StartNew();
            
                var client = new JokeContractClient(cbo.Text);

                if (chkImpersonate.Checked)
                    client.Headers.Add( new AuthenticationHeader( new IDPasswordCredentials(tbID.Text, tbPwd.Text)) );

                var totalCalls = 0;
                var reactors = new List<CallReactor>();
                
                for(int rCnt=0, n=tbReactors.Text.AsInt(); rCnt<n; rCnt++)
                {
                    var calls = new List<Call>();
                    for(int cnt=0, m=tbCallsPerReactor.Text.AsInt(); cnt<m; cnt++)
                        try { calls.Add( new Call( client.Async_Echo("Call number {0} from {1} reactor ".Args(cnt, rCnt) ))); }
                        catch (Exception err) { tbNote.Text += "{0:000}:{1:00000}: {2}".Args(rCnt, cnt, err.Message); }
                
                    totalCalls+=calls.Count; 
                    reactors.Add(new CallReactor(calls));
                }

            var callsPlaced = w.ElapsedMilliseconds;

                CallReactor.WaitAll(reactors);


                var stats = new ConcurrentDictionary<CallStatus, int>();
                foreach(var rtor in reactors)
                {
                     foreach(var call in rtor.Calls)
                     {
                        if (call.CallSlot.CallStatus==CallStatus.ResponseError)
                        {
                            var msg = call.CallSlot.ResponseMsg;
                            Text = msg.ExceptionData.Message;
                        }
                        stats.AddOrUpdate(call.CallSlot.CallStatus, 1, (_,k)=> k+1); 
                     }
                }

                var sb = new StringBuilder();
                foreach(var k in stats.Keys)
                 sb.AppendLine( "{0} = {1} times".Args(k, stats[k]) );

                tbNote.Text = sb.ToString();

            var allFinished = w.ElapsedMilliseconds;

            Text = "Placed {0} calls in {1} ms, then waited {2} ms for finish, total time {3} ms @ {4} calls/sec "
                   .Args
                   (
                     totalCalls,
                     callsPlaced, 
                     allFinished - callsPlaced,
                     allFinished,
                     (1000* totalCalls) / allFinished
                   );
        }

        private void btnParallelDispatch_Click(object sender, EventArgs e)
        {
            warmup();
            var unsecure = chkUnsecureEcho.Checked;
            var argsMarshal = chkArgsMarshalling.Checked;
            Text = "Working...";
            tbNote.Text = "Started...";
            var w = Stopwatch.StartNew();
            
                var client = new JokeContractClient(cbo.Text);
 client.DispatchTimeoutMs = 5 * 1000;
 client.TimeoutMs = 40 * 1000;             
                if (!unsecure && chkImpersonate.Checked)
                    client.Headers.Add( new AuthenticationHeader( new IDPasswordCredentials(tbID.Text, tbPwd.Text)) );

                var totalCalls = tbCallsPerReactor.Text.AsInt();
                
                
                    var calls = new ConcurrentQueue<Call>();
                    var derrors = new ConcurrentQueue<string>();
                    System.Threading.Tasks.Parallel.For(0, totalCalls,
                        (i) =>
                        {
                            try 
                            {
                               calls.Enqueue( new Call( 
                                         argsMarshal ?
                                          client.Async_UnsecEchoMar("Call number {0} ".Args(i))
                                         :
                                         (unsecure ? client.Async_UnsecureEcho("Call number {0} ".Args(i)) 
                                                  : client.Async_Echo("Call number {0} ".Args(i)) ))); 
                            }
                            catch (Exception err) 
                            {
                              derrors.Enqueue( "{0}: {1}\r\n".Args(1, err.ToMessageWithType()) ); 
                            }
                        });

                
   
                string estr = null;
                while(derrors.TryDequeue(out estr)) tbNote.Text += estr;


                var reactor = new CallReactor( calls );

            var callsPlaced = w.ElapsedMilliseconds;

                reactor.Wait();


                var stats = new ConcurrentDictionary<CallStatus, int>();
                
                     foreach(var call in reactor.Calls)
                     {
                        if (call.CallSlot.CallStatus==CallStatus.ResponseError)
                        {
                            var msg = call.CallSlot.ResponseMsg;
                            Text = msg.ExceptionData.Message;
                        }
                        stats.AddOrUpdate(call.CallSlot.CallStatus, 1, (_,k)=> k+1); 
                     }

                var sb = new StringBuilder();
                foreach(var k in stats.Keys)
                 sb.AppendLine( "{0} = {1} times".Args(k, stats[k]) );

                tbNote.Text += sb.ToString();

            var allFinished = w.ElapsedMilliseconds;

           Text = "Placed {0:n2} calls in {1:n2} ms, then waited {2:n2} ms for finish, total time {3:n2} ms @ {4:n2} calls/sec "
                   .Args
                   (
                     totalCalls,
                     callsPlaced, 
                     allFinished - callsPlaced,
                     allFinished,
                     totalCalls / (allFinished / 1000d)
                   );
        }

        private void btnManyDBWorks_Click(object sender, EventArgs e)
        {
            warmup();
            var rCount = tbRecCount.Text.AsInt();
            var waitFrom = tbWaitFrom.Text.AsInt();
            var waitTo = tbWaitTo.Text.AsInt();

            Text = "Working...";
            tbNote.Text = "Started...";
            var w = Stopwatch.StartNew();
            
                var client = new JokeContractClient(cbo.Text);
 client.DispatchTimeoutMs = 5 * 1000;
 client.TimeoutMs = 40 * 1000;             

                var totalCalls = tbCallsPerReactor.Text.AsInt();
                
                
                    var calls = new ConcurrentQueue<Call>();
                    var derrors = new ConcurrentQueue<string>();
                    System.Threading.Tasks.Parallel.For(0, totalCalls, new ParallelOptions{ MaxDegreeOfParallelism = tbReactors.Text.AsInt(16)},
                        (i) =>
                        {
                            try
                            { 
                              calls.Enqueue( new Call(client.Async_DBWork(i.ToString(), rCount, 
                                                                                        waitFrom > 0 ? ExternalRandomGenerator.Instance.NextScaledRandomInteger(waitFrom,waitTo) : 0))); 
                                                       //ExternalRandomGenerator.Instance.NextScaledRandomInteger(1,100),
                                                       //ExternalRandomGenerator.Instance.NextScaledRandomInteger(0,50))) ); 
                            }
                            catch (Exception err) 
                            {
                              derrors.Enqueue( "{0}: {1}\r\n".Args(1, err.ToMessageWithType()) ); 
                            }
                        });

                
   
                string estr = null;
                while(derrors.TryDequeue(out estr)) tbNote.Text += estr;


                var reactor = new CallReactor( calls );

            var callsPlaced = w.ElapsedMilliseconds;

                reactor.Wait();


                var stats = new ConcurrentDictionary<CallStatus, int>();
                
                     foreach(var call in reactor.Calls)
                     {
                        if (call.CallSlot.CallStatus==CallStatus.ResponseError)
                        {
                            var msg = call.CallSlot.ResponseMsg;
                            Text = msg.ExceptionData.Message;
                        }
                        stats.AddOrUpdate(call.CallSlot.CallStatus, 1, (_,k)=> k+1); 
                     }

                var sb = new StringBuilder();
                foreach(var k in stats.Keys)
                 sb.AppendLine( "{0} = {1} times".Args(k, stats[k]) );

                tbNote.Text += sb.ToString();

            var allFinished = w.ElapsedMilliseconds;

            Text = "Placed {0} DB calls in {1} ms, then waited {2} ms for finish, total time {3} ms @ {4} calls/sec "
                   .Args
                   (
                     totalCalls,
                     callsPlaced, 
                     allFinished - callsPlaced,
                     allFinished,
                     (1000* totalCalls) / allFinished
                   );


            if (chkAutoDispatch.Checked)
            {
              tmrAuto.Interval = 1000 + ExternalRandomGenerator.Instance.NextScaledRandomInteger(100, 10000);
              tmrAuto.Enabled = true;
                            
            }
        }

        private void tmrAuto_Tick(object sender, EventArgs e)
        {
          tmrAuto.Enabled = false;

          btnManyDBWorks_Click(null, null);
        }

        private void button6_Click(object sender, EventArgs e)
        {
           warmup();
           var unsecure = chkUnsecureEcho.Checked;
            Text = "Working...";
            tbNote.Text = "Started...";
            var w = Stopwatch.StartNew();
            
                var client = new JokeContractClient(cbo.Text);
 client.DispatchTimeoutMs = 5 * 1000;
 client.TimeoutMs = 40 * 1000;             
                if (!unsecure && chkImpersonate.Checked)
                    client.Headers.Add( new AuthenticationHeader( new IDPasswordCredentials(tbID.Text, tbPwd.Text)) );

                var totalCalls = tbCallsPerReactor.Text.AsInt();
                
                    System.Threading.Tasks.Parallel.For(0, totalCalls,
                        (i) =>
                        {
                           client.Async_Notify("aa");
                        });
              var allFinished = w.ElapsedMilliseconds;           
              Text = "Placed {0:n2} One Way calls in {1:n2} ms,  @ {2:n2} calls/sec "
                   .Args
                   (
                     totalCalls,
                     allFinished,
                     totalCalls / (allFinished/1000d)
                   );
        }

        private void button7_Click(object sender, EventArgs e)
        {
          warmup();
          var unsecure = chkUnsecureEcho.Checked;
            Text = "Working...";
            tbNote.Text = "Started...";
            var w = Stopwatch.StartNew();
            
               var node = cbo.Text;
               //var totalCalls = tbCallsPerReactor.Text.AsInt();

               var rcount = tbReactors.Text.AsInt(10);
               var prc = tbCallsPerReactor.Text.AsInt();//totalCalls / rcount;
               var auth = new AuthenticationHeader( new IDPasswordCredentials(tbID.Text, tbPwd.Text));

               var totalCalls = rcount * prc; 
                
                    var calls = new ConcurrentQueue<Call>();
                    var derrors = new ConcurrentQueue<string>();
                    var tasks = new List<Task>();

                    for(var i=0; i<rcount; i++)
                    {
                      tasks.Add(Task.Factory.StartNew((idx)=>
                      {
                            var lcl = new JokeContractClient(node);
                            lcl.DispatchTimeoutMs = 5 * 1000;
                            lcl.TimeoutMs = 40 * 1000;           
                            lcl.ReserveTransport = true;  
                            if (!unsecure && chkImpersonate.Checked)
                                lcl.Headers.Add( auth );

                            for(var j=0; j<prc;j++)
                            {
                              try { calls.Enqueue( new Call( unsecure ? lcl.Async_UnsecureEcho("Call number {0} ".Args(j)) 
                                                                      : lcl.Async_Echo("Call number {0} ".Args(j)) )); }
                              catch (Exception err) 
                              {
                                derrors.Enqueue( "{0}: {1}\r\n".Args(j, err.ToMessageWithType()) ); 
                              }
                            }//for

                            lcl.Dispose();

                      }, i, TaskCreationOptions.LongRunning));

                    }

                Task.WaitAll(tasks.ToArray());
   
                string estr = null;
                while(derrors.TryDequeue(out estr)) tbNote.Text += estr;


                var reactor = new CallReactor( calls );

            var callsPlaced = w.ElapsedMilliseconds;

                reactor.Wait();


                var stats = new ConcurrentDictionary<CallStatus, int>();
                
                     foreach(var call in reactor.Calls)
                     {
                        if (call.CallSlot.CallStatus==CallStatus.ResponseError)
                        {
                            var msg = call.CallSlot.ResponseMsg;
                            Text = msg.ExceptionData.Message;
                        }
                        stats.AddOrUpdate(call.CallSlot.CallStatus, 1, (_,k)=> k+1); 
                     }

                var sb = new StringBuilder();
                foreach(var k in stats.Keys)
                 sb.AppendLine( "{0} = {1} times".Args(k, stats[k]) );

                tbNote.Text += sb.ToString();

            var allFinished = w.ElapsedMilliseconds;

             Text = "Placed {0:n2} calls in {1:n2} ms, then waited {2:n2} ms for finish, total time {3:n2} ms @ {4:n2} calls/sec "
                   .Args
                   (
                     totalCalls,
                     callsPlaced, 
                     allFinished - callsPlaced,
                     allFinished,
                     totalCalls / (allFinished / 1000d)
                   );
        }

        private void btnSimple_Click(object sender, EventArgs e)
        {
            warmup();
            var unsecure = chkUnsecureEcho.Checked;
            Text = "Working...";
            tbNote.Text = "Started...";
            var w = Stopwatch.StartNew();
            
                var client = new JokeContractClient(cbo.Text);
 client.DispatchTimeoutMs = 5 * 1000;
 client.TimeoutMs = 40 * 1000;             
                if (!unsecure && chkImpersonate.Checked)
                    client.Headers.Add( new AuthenticationHeader( new IDPasswordCredentials(tbID.Text, tbPwd.Text)) );

                var totalCalls = tbCallsPerReactor.Text.AsInt();

                var totalErrors = 0;
                
                
                    System.Threading.Tasks.Parallel.For(0, totalCalls,
                        (i) =>
                        {
                            try 
                            { 
                              if (unsecure)
                                client.UnsecureEcho("Call number {0} ".Args(i));
                              else
                                client.Echo("Call number {0} ".Args(i));
                            }
                            catch (Exception) 
                            {
                              Interlocked.Increment(ref totalErrors);
                            }
                        });


            var allFinished = w.ElapsedMilliseconds;

            Text = "Placed {0:n2} calls in {1:n2} ms total time {2:n2} ms @ {3:n2} calls/sec; totalErrors={4:n2} "
                   .Args
                   (
                     totalCalls,
                     allFinished,
                     allFinished,
                     totalCalls / (allFinished/1000d),
                     totalErrors
                   );
        }

        private void btGCCollect_Click(object sender, EventArgs e)
        {
          var m1 = GC.GetTotalMemory(false);
          GC.Collect();
          var m2 = GC.GetTotalMemory(false);
          Text = "Collected {0} bytes".Args(m1 - m2);
        }

        private void btnSimpleWork_Click(object sender, EventArgs e)
        {
            warmup();
            var marshal = chkArgsMarshalling.Checked;            
            Text = "Working...";
            tbNote.Text = "Started...";
            var w = Stopwatch.StartNew();
            
               var node = cbo.Text;
               //var totalCalls = tbCallsPerReactor.Text.AsInt();

               var rcount = tbReactors.Text.AsInt(10);
               var prc = tbCallsPerReactor.Text.AsInt();//totalCalls / rcount;
               var auth = new AuthenticationHeader( new IDPasswordCredentials(tbID.Text, tbPwd.Text));

               var totalCalls = rcount * prc; 
                
                    var calls = new ConcurrentQueue<Call>();
                    var derrors = new ConcurrentQueue<string>();
                    var tasks = new List<Task>();

                    for(var i=0; i<rcount; i++)
                    {
                      tasks.Add(Task.Factory.StartNew((idx)=>
                      {
                            var lcl = new JokeContractClient(node);
                            lcl.DispatchTimeoutMs = 5 * 1000;
                            lcl.TimeoutMs = 40 * 1000;           
                            lcl.ReserveTransport = true;  

                            for(var j=0; j<prc;j++)
                            {
                              try { calls.Enqueue( new Call( marshal ? lcl.Async_SimpleWorkMar("Call number {0} ".Args(j), j, i, true, 123.12) 
                                                                      : lcl.Async_SimpleWorkAny("Call number {0} ".Args(j), j, i, true, 123.12)) ); }
                              catch (Exception err) 
                              {
                                derrors.Enqueue( "{0}: {1}\r\n".Args(j, err.ToMessageWithType()) ); 
                              }
                            }//for

                            lcl.Dispose();

                      }, i, TaskCreationOptions.LongRunning));

                    }

                Task.WaitAll(tasks.ToArray());
   
                string estr = null;
                while(derrors.TryDequeue(out estr)) tbNote.Text += estr;


                var reactor = new CallReactor( calls );

            var callsPlaced = w.ElapsedMilliseconds;

                reactor.Wait();


                var stats = new ConcurrentDictionary<CallStatus, int>();
                
                     foreach(var call in reactor.Calls)
                     {
                        if (call.CallSlot.CallStatus==CallStatus.ResponseError)
                        {
                            var msg = call.CallSlot.ResponseMsg;
                            Text = msg.ExceptionData.Message;
                        }
                        stats.AddOrUpdate(call.CallSlot.CallStatus, 1, (_,k)=> k+1); 
                     }

                var sb = new StringBuilder();
                foreach(var k in stats.Keys)
                 sb.AppendLine( "{0} = {1} times".Args(k, stats[k]) );

                tbNote.Text += sb.ToString();

            var allFinished = w.ElapsedMilliseconds;

            Text = "Placed {0:n2} calls in {1:n2} ms, then waited {2:n2} ms for finish, total time {3:n2} ms @ {4:n2} calls/sec "
                   .Args
                   (
                     totalCalls,
                     callsPlaced, 
                     allFinished - callsPlaced,
                     allFinished,
                     totalCalls / (allFinished / 1000d)
                   );
        }

        private void chkDumpMessages_CheckedChanged(object sender, EventArgs e)
        {
          var en = chkDumpMessages.Checked;
          
          App.Glue.Bindings["sync"].ClientDump = en ? DumpDetail.Message : DumpDetail.None;
          App.Glue.Bindings["mpx"].ClientDump = en ? DumpDetail.Message : DumpDetail.None;
        }



    }
}
