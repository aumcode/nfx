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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using NFX;
using NFX.ApplicationModel;
using NFX.Environment;
using NFX.Glue.Native;
using NFX.Glue;
using NFX.IO;


namespace NFX.NUnit.Glue
{
    public static class TestLogic
    {
        private static void dumpBindingTransports(Binding binding)
        {
           Console.WriteLine("Client transport count: {0}".Args( binding.ClientTransports.Count()));
           Console.WriteLine("Server transport count: {0}".Args( binding.ServerTransports.Count()));
        }


        public static void TestContractA_TwoWayCall(string CONF_SRC)
        {
            TestServerA.s_Accumulator = 0;
            
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var cl = new TestContractAClient(App.ConfigRoot.AttrByName("cs").Value);

                var result = cl.Method1(12);
                Assert.AreEqual( "12", result);
                Assert.AreEqual(12, TestServerA.s_Accumulator);
            }
        }

        public static void TASK_TestContractA_TwoWayCall(string CONF_SRC)
        {
            TestServerA.s_Accumulator = 0;
            
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var cl = new TestContractAClient(App.ConfigRoot.AttrByName("cs").Value);

                var call = cl.Async_Method1(12);
                var task = call.AsTask;
                                         
                var result = task.Result.GetValue<string>();

                Assert.AreEqual( "12", result);
                Assert.AreEqual(12, TestServerA.s_Accumulator);
            }
        }

        public static void TASKReturning_TestContractA_TwoWayCall(string CONF_SRC)
        {
            TestServerA.s_Accumulator = 0;
            
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var cl = new TestContractAClient(App.ConfigRoot.AttrByName("cs").Value);

                var call = cl.Async_Method1(12);
                var task = call.AsTaskReturning<string>();
                                         
                var result = task.Result;

                Assert.AreEqual( "12", result);
                Assert.AreEqual(12, TestServerA.s_Accumulator);
            }
        }


        public static void TestContractA_TwoWayCall_Timeout(string CONF_SRC)
        {
            TestServerA.s_Accumulator = 0;
            
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var cl = new TestContractAClient(App.ConfigRoot.AttrByName("cs").Value);
                cl.TimeoutMs = 2000;
                
                try
                {
                  cl.Sleeper(5000);
                }
                catch(ClientCallException err)
                {
                  Assert.AreEqual(CallStatus.Timeout, err.CallStatus);
                  return;
                }
                catch(System.IO.IOException err) //sync binding throws IO exception
                {
                  Assert.IsTrue( err.Message.Contains("after a period of time") );
                  return;
                }

                Assert.Fail("Invalid Call status");
            }
        }


        public static void TASK_TestContractA_TwoWayCall_Timeout(string CONF_SRC)
        {
            TestServerA.s_Accumulator = 0;
            
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var cl = new TestContractAClient(App.ConfigRoot.AttrByName("cs").Value);
                cl.TimeoutMs = 2000;
                
                System.Threading.Tasks.Task<CallSlot> task = null;
                try
                {
                  task = cl.Async_Sleeper(10000).AsTask;
                  task.Result.GetValue<int>();
                }
                catch(ClientCallException err)
                {
                  Assert.AreEqual(CallStatus.Timeout, err.CallStatus);
                  return;
                }
                catch(System.IO.IOException err) //sync binding throws IO exception
                {
                  Assert.IsTrue( err.Message.Contains("after a period of time") );
                  return;
                }

                Assert.Fail("Invalid Call status: " + (task!=null ? task.Result.CallStatus.ToString() : "task==null"));
            }
        }



        public static void TestContractA_OneWayCall(string CONF_SRC)
        {
            TestServerA.s_Accumulator = 0;
            
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var cl = new TestContractAClient(App.ConfigRoot.AttrByName("cs").Value);

                cl.Method2(93);
                
                for(var cnt=0; cnt<10 && TestServerA.s_Accumulator!=93 ; cnt++)
                    System.Threading.Thread.Sleep(1000);

                Assert.AreEqual(93,TestServerA.s_Accumulator); 
            }
        }


        public static void TestContractB_1(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(App.ConfigRoot.AttrByName("cs").Value);

                var person = new PersonData{ID = 10, FirstName="Joe", LastName="Tester" };
                
                cl.SetPersonOfTheDay( person );
                
                var ret = cl.GetPersonOfTheDay();
                                                                 
                Assert.AreEqual( 10, ret.ID);
                Assert.AreEqual( "Joe", ret.FirstName);
                Assert.AreEqual( "Tester", ret.LastName);

                dumpBindingTransports( cl.Binding );
            }

        }


        public static void TestContractB_1_Async(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(App.ConfigRoot.AttrByName("cs").Value);

                var person = new PersonData{ID = 10, FirstName="Joe", LastName="Tester" };
                
                var slot = cl.Async_SetPersonOfTheDay( person );
                
                slot.CheckVoidValue();

                slot = cl.Async_GetPersonOfTheDay();
                    
                var ret = slot.GetValue<PersonData>();    
                                                                 
                Assert.AreEqual( 10, ret.ID);
                Assert.AreEqual( "Joe", ret.FirstName);
                Assert.AreEqual( "Tester", ret.LastName);

                dumpBindingTransports( cl.Binding );
            }

        }




        public static void TestContractB_2(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(App.ConfigRoot.AttrByName("cs").Value);

                var person = new PersonData{ID = 10, FirstName="Joe", LastName="Tester" };
                
                cl.SetPersonOfTheDay( person );
                
                var ret = cl.GetPersonOfTheDay();
                                                                 
                Assert.AreEqual( 10, ret.ID);
                Assert.AreEqual( "Joe", ret.FirstName);
                Assert.AreEqual( "Tester", ret.LastName);

                var sum = cl.SummarizeAndFinish(); //destructor

                Assert.AreEqual("That is all! for the person Tester", sum);

                cl.ForgetRemoteInstance();

                Assert.AreEqual("Felix", cl.GetName()); //this will allocate the new isntance
               
                sum = cl.SummarizeAndFinish(); // this will kill the instance again
                Assert.AreEqual("That is all! but no person of the day was set", sum);

                dumpBindingTransports( cl.Binding );
            }
        }


        public static void TestContractB_3(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(App.ConfigRoot.AttrByName("cs").Value);
                                                                 
                Assert.AreEqual( "Felix", cl.GetName());

                dumpBindingTransports( cl.Binding );
            }

        }

        public static void TestContractB_4(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(App.ConfigRoot.AttrByName("cs").Value);
                 
                //Testing overloaded calls                                                 
                Assert.AreEqual( "Felix", cl.GetName());
                Assert.AreEqual( "Felix23", cl.GetName(23));
                Assert.AreEqual( "Felix42", cl.GetName(42));
                Assert.AreEqual( "Felix", cl.GetName());

                dumpBindingTransports( cl.Binding );
            }

        }

        public static void TestContractB_4_Async(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(App.ConfigRoot.AttrByName("cs").Value);
                
                Assert.AreEqual( "Felix", cl.GetName());//alloc first
                 
                //Testing overloaded calls via CallSlot                                                
                Assert.AreEqual( "Felix",   cl.Async_GetName()  .GetValue<string>());
                Assert.AreEqual( "Felix23", cl.Async_GetName(23).GetValue<string>());
                Assert.AreEqual( "Felix42", cl.Async_GetName(42).GetValue<string>());
                Assert.AreEqual( "Felix",   cl.Async_GetName()  .GetValue<string>());
                
                dumpBindingTransports( cl.Binding );
            }

        }

        public static void TestContractB_4_AsyncReactor(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(App.ConfigRoot.AttrByName("cs").Value);
                 
                var reactor = new CallReactor(
                                new Call( cl.Async_GetName(),   (r,c)=> Assert.AreEqual( "Felix", c.CallSlot.GetValue<string>()   ) ),
                                new Call( cl.Async_GetName(23), (r,c)=> Assert.AreEqual( "Felix23", c.CallSlot.GetValue<string>() ) ),
                                new Call( cl.Async_GetName(42), (r,c)=> Assert.AreEqual( "Felix42", c.CallSlot.GetValue<string>() ) ),
                                new Call( cl.Async_GetName(2, DateTime.Now), (r,c)=> Assert.IsTrue( c.CallSlot.GetValue<string>().StartsWith("Felix2") ) )
                              );
                              
                reactor.Wait();      

                dumpBindingTransports( cl.Binding );
                Assert.IsTrue(reactor.Finished);
            }

        }


        public static void TestContractB_4_Parallel(string CONF_SRC, bool threadSafe)
        {
            const int CNT = 10000;


            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                //Use the same client.....
                var cl = new TestContractBClient(App.ConfigRoot.AttrByName(threadSafe?"cs":"cs2").Value);
                
                Assert.AreEqual( "Felix1223", cl.GetName(1223));//alloc server

                var set = new HashSet<int>();
                var watch = System.Diagnostics.Stopwatch.StartNew();
                 
                //.....for making many parallel calls
                System.Threading.Tasks.Parallel.For(0, CNT,
                                              (i, loop)=>
                                              {       
                                                var id = System.Threading.Thread.CurrentThread.ManagedThreadId;

                                                lock(set)
                                                 set.Add( id );
                                                
                                                //Testing overloaded calls 
                                                var result = cl.GetName(i);                                                
                                                Assert.AreEqual( "Felix{0}".Args(i), result);
                                              });
                var elps = watch.ElapsedMilliseconds;

                Console.WriteLine("Parallel Glue Test made {0} calls in {1} ms at {2} call/sec and was done by these threads:".Args(CNT, elps, CNT / (elps / 1000d)) );
                dumpBindingTransports( cl.Binding );
                var cnt = 0;
                foreach(var id in set)
                {
                   Console.Write( id+", " );
                   cnt++;
                }
                Console.WriteLine( cnt + " total");

            }

        }

        public static void TestContractB_4_Marshalling_Parallel(string CONF_SRC, bool threadSafe)
        {
            const int CNT = 10000;


            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                //Use the same client.....
                var cl = new TestContractBClient(App.ConfigRoot.AttrByName(threadSafe?"cs":"cs2").Value);
                
                Assert.AreEqual( "Felix1223", cl.GetNameMar(1223));//alloc server

                var set = new HashSet<int>();
                var watch = System.Diagnostics.Stopwatch.StartNew();
                 
                //.....for making many parallel calls
                System.Threading.Tasks.Parallel.For(0, CNT,
                                              (i, loop)=>
                                              {       
                                                var id = System.Threading.Thread.CurrentThread.ManagedThreadId;

                                                lock(set)
                                                 set.Add( id );
                                                
                                                //Testing overloaded calls 
                                                var result = cl.GetNameMar(i);                                                
                                                Assert.AreEqual( "Felix{0}".Args(i), result);
                                              });
                var elps = watch.ElapsedMilliseconds;

                Console.WriteLine("Parallel Glue Test made {0} calls in {1} ms at {2} call/sec and was done by these threads:".Args(CNT, elps, CNT / (elps / 1000d)) );
                dumpBindingTransports( cl.Binding );
                var cnt = 0;
                foreach(var id in set)
                {
                   Console.Write( id+", " );
                   cnt++;
                }
                Console.WriteLine( cnt + " total");

            }

        }





        public static void TASK_TestContractB_4_Parallel(string CONF_SRC, bool threadSafe)
        {
            const int CNT = 10000;


            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                //Use the same client.....
                var cl = new TestContractBClient(App.ConfigRoot.AttrByName(threadSafe?"cs":"cs2").Value);
                
                Assert.AreEqual( "Felix1223", cl.GetName(1223));//alloc server

                var set = new HashSet<int>();
                var watch = System.Diagnostics.Stopwatch.StartNew();
                 
                //.....for making many parallel calls
                System.Threading.Tasks.Parallel.For(0, CNT,
                                              (i, loop)=>
                                              {       
                                                var id = System.Threading.Thread.CurrentThread.ManagedThreadId;

                                                lock(set)
                                                 set.Add( id );
                                                
                                                //Testing overloaded calls USING asTASK property
                                                var result = cl.Async_GetName(i).AsTask.Result.GetValue<string>();                                                
                                                Assert.AreEqual( "Felix{0}".Args(i), result);
                                              });
                var elps = watch.ElapsedMilliseconds;

                Console.WriteLine("Parallel Glue Test made {0} calls in {1} ms at {2} call/sec and was done by these threads:".Args(CNT, elps, CNT / (elps / 1000d)) );
                dumpBindingTransports( cl.Binding );
                var cnt = 0;
                foreach(var id in set)
                {
                   Console.Write( id+", " );
                   cnt++;
                }
                Console.WriteLine( cnt + " total");

            }

        }






        public static void TestContractB_4_Parallel_ManyClients(string CONF_SRC, bool threadSafe)
        {
            const int CNT = 10000;
            const int CLCNT = 157;

            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var rnd = new Random();
                var rndBound = (int)(CLCNT * 1.3751d);
                var clients = new List<TestContractBClient>();

                for(var i=0; i<CLCNT; i++)
                {
                  var cl = new TestContractBClient(App.ConfigRoot.AttrByName(threadSafe?"cs":"cs2").Value);
                  Assert.AreEqual( "Felix1223", cl.GetName(1223));//alloc server
                  clients.Add(cl);
                }


                var set = new HashSet<int>();
                var watch = System.Diagnostics.Stopwatch.StartNew();
                 
                //.....for making many parallel calls
                System.Threading.Tasks.Parallel.For(0, CNT,
                                              (i, loop)=>
                                              {       
                                                var id = System.Threading.Thread.CurrentThread.ManagedThreadId;

                                                lock(set)
                                                 set.Add( id );
                                                
                                                var idx = rnd.Next( rndBound );
                                                if (idx>=clients.Count) idx = clients.Count-1;
                                                var cl = clients[idx];
                                                                      
                                                //Testing overloaded calls                                                 
                                                Assert.AreEqual( "Felix{0}".Args(i), cl.GetName(i));
                                              });
                var elps = watch.ElapsedMilliseconds;

                Console.WriteLine("Parallel Many Clients Glue test made {0} calls in {1} ms at {2} call/sec and was done by these threads:".Args(CNT, elps, CNT / (elps / 1000d)) );
                dumpBindingTransports( App.Glue.Bindings.First() );
                var cnt = 0;
                foreach(var id in set)
                {
                   Console.Write( id+", " );
                   cnt++;
                }
                Console.WriteLine( cnt + " total");

            }

        }





        public static void TestContractB_5(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(App.ConfigRoot.AttrByName("cs").Value);
                 
                var ret = cl.GetPersonalData(new int[]{1,23,97});

                Assert.AreEqual( 3, ret.Count);

                Assert.AreEqual( 1,  ret[0].ID);
                Assert.AreEqual( 23, ret[1].ID);
                Assert.AreEqual( 97, ret[2].ID);

                Assert.AreEqual( "Oleg1",  ret[0].FirstName);
                Assert.AreEqual( "Oleg23", ret[1].FirstName);
                Assert.AreEqual( "Oleg97", ret[2].FirstName);

                Assert.AreEqual( "Popov1",  ret[0].LastName);
                Assert.AreEqual( "Popov23", ret[1].LastName);
                Assert.AreEqual( "Popov97", ret[2].LastName);

                Assert.AreEqual( false, ret[0].Certified);
                Assert.AreEqual( false, ret[1].Certified);
                Assert.AreEqual( false, ret[2].Certified);

                dumpBindingTransports( cl.Binding );
            }

        }

        public static void TestContractB_6(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(App.ConfigRoot.AttrByName("cs").Value);
                 
                var ret = cl.GetPersonalData(new int[]{1,23,97}, true, 127000m);

                Assert.AreEqual( 3, ret.Count);

                Assert.AreEqual( 1,  ret[0].ID);
                Assert.AreEqual( 23, ret[1].ID);
                Assert.AreEqual( 97, ret[2].ID);

                Assert.AreEqual( "Oleg1",  ret[0].FirstName);
                Assert.AreEqual( "Oleg23", ret[1].FirstName);
                Assert.AreEqual( "Oleg97", ret[2].FirstName);

                Assert.AreEqual( "Popov1",  ret[0].LastName);
                Assert.AreEqual( "Popov23", ret[1].LastName);
                Assert.AreEqual( "Popov97", ret[2].LastName);

                Assert.AreEqual( true, ret[0].Certified);
                Assert.AreEqual( true, ret[1].Certified);
                Assert.AreEqual( true, ret[2].Certified);

                Assert.AreEqual( 127000m, ret[0].Salary);
                Assert.AreEqual( 127000m, ret[1].Salary);
                Assert.AreEqual( 127000m, ret[2].Salary);

                dumpBindingTransports( cl.Binding );
            }

        }


        public static void TestContractB_7(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(App.ConfigRoot.AttrByName("cs").Value);
                 
                var ret = cl.GetDailyStatuses(7);

                Assert.AreEqual( 7, ret.Count);
                var dt = new DateTime(1980,1,1);

                Assert.AreEqual( 100,        ret[dt].Count);
                Assert.AreEqual( "Oleg0",    ret[dt][0].FirstName);
                Assert.AreEqual( "Oleg99",   ret[dt][99].FirstName);
                Assert.AreEqual( "Popov99",  ret[dt][99].LastName);
                Assert.AreEqual( 99000m,     ret[dt][99].Salary);
               
                dt = dt.AddSeconds(ret.Count-1);

                Assert.AreEqual( 100,        ret[dt].Count);
                Assert.AreEqual( "Oleg0",    ret[dt][0].FirstName);
                Assert.AreEqual( "Oleg99",   ret[dt][99].FirstName);
                Assert.AreEqual( "Popov99",  ret[dt][99].LastName);
                Assert.AreEqual( 99000m,     ret[dt][99].Salary);
               
                dumpBindingTransports( cl.Binding );
            }

        }


        public static void TestContractB_8(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(App.ConfigRoot.AttrByName("cs").Value);
                 
                var ret = cl.GetDailyStatuses(150);

                Assert.AreEqual( 150, ret.Count);
                var dt = new DateTime(1980,1,1);

                Assert.AreEqual( 100,        ret[dt].Count);
                Assert.AreEqual( "Oleg0",    ret[dt][0].FirstName);
                Assert.AreEqual( "Oleg99",   ret[dt][99].FirstName);
                Assert.AreEqual( "Popov99",  ret[dt][99].LastName);
                Assert.AreEqual( 99000m,     ret[dt][99].Salary);
               
                dt = dt.AddSeconds(ret.Count-1);

                Assert.AreEqual( 100,        ret[dt].Count);
                Assert.AreEqual( "Oleg0",    ret[dt][0].FirstName);
                Assert.AreEqual( "Oleg99",   ret[dt][99].FirstName);
                Assert.AreEqual( "Popov99",  ret[dt][99].LastName);
                Assert.AreEqual( 99000m,     ret[dt][99].Salary);
               
                dumpBindingTransports( cl.Binding );
            }

        }


        //this will throw
        public static void TestContractB_9(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(App.ConfigRoot.AttrByName("cs").Value);
                
                Exception err = null;
                try 
                {
                  cl.GetDailyStatuses(1);//this is needed to init type registry for sync binding
                                         //because otherwise it will abort the channel instead of marshalling exception back
                  cl.GetDailyStatuses(550);
                }
                catch(Exception error)
                {
                  err = error;
                }
                Assert.IsNotNull( err );
                Assert.AreEqual( typeof(RemoteException), err.GetType());

                Assert.IsTrue( err.Message.Contains("MessageSizeException"));
                Assert.IsTrue( err.Message.Contains("exceeds limit"));
            }

        }





    }
}
