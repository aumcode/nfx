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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using NUnit.Framework;

using NFX.Environment;

using NFX.Log;
using LSVC = NFX.Log.LogService;
using TSLS = NFX.NUnit.TestSyncLog;
using NFX.Log.Destinations;
using NFX.ApplicationModel;
using System.Threading;
using System.Reflection;

namespace NFX.NUnit.Logging
{
    [TestFixture]   
    public class BasicLogServiceTests
    {
        public const string TEST_DIR = @"c:\NFX"; // TODO: Don't hard-code - get from environment

        [Test]
        public void CSVFileDestinationStartByCode()
        {
         string TNAME = "UnitTest-" + MethodBase.GetCurrentMethod().Name;
         string FNAME = TNAME + ".csv.log";

         var svc = new LSVC(null);

         var fname = Path.Combine(TEST_DIR, FNAME);

         if (File.Exists(fname)) File.Delete(fname);  

         using (Scope.OnExit(() => File.Delete(fname)))
         {
           svc.RegisterDestination(
               new CSVFileDestination(TNAME, TEST_DIR){
                    CreateDir = true
               });
  
           svc.Start();      
  
               svc.Write(new Message{Text = "1 message"});
               svc.Write(new Message{Text = "2 message"});
               svc.Write(new Message{Text = "3 message"});
               svc.Write(new Message{Text = "4 message"});
  
  
           svc.WaitForCompleteStop();
  
           Assert.AreEqual(true, File.Exists(fname));
           Assert.AreEqual(4, File.ReadAllLines(fname).Length);
         }
        }

        [Test]
        public void CSVFileDestinationStartByConfig1()
        {
         string TNAME = "UnitTest-" + MethodBase.GetCurrentMethod().Name;
         string FNAME = TNAME + ".csv.log";
         const string DATE = "20131012";

         var xml= @"<log>
                        <destination type='NFX.Log.Destinations.CSVFileDestination, NFX'
                                     name='{0}'
                                     filename='$(@~path)$(::now fmt=yyyyMMdd value={1})-$($name).csv.log'
                                     create-dir='true'
                        />
                 </log>".Args(TNAME, DATE);

         var svc = new LSVC(null);

         var fname = Path.Combine(TEST_DIR, DATE + "-" + FNAME);

         if (File.Exists(fname)) File.Delete(fname);  

         using (Scope.OnExit(() => File.Delete(fname)))
         {
           var cfg = XMLConfiguration.CreateFromXML(xml);
           cfg.EnvironmentVarResolver = new Vars { { "path", TEST_DIR } };
           svc.Configure(cfg.Root);

           svc.Start();

               svc.Write(new Message{Text = "1 message"});
               svc.Write(new Message{Text = "2 message"});
               svc.Write(new Message{Text = "3 message"});


           svc.WaitForCompleteStop();

           Assert.IsTrue(File.Exists(fname));
           Assert.AreEqual(3, File.ReadAllLines(fname).Length);
         }
        }

        [Test]
        public void CSVFileDestinationStartByConfig2()
        {
         string TNAME = "UnitTest-" + MethodBase.GetCurrentMethod().Name;
         string FNAME = TNAME + ".csv.log";
         var xml= @"<log>
                        <destination  type='NFX.Log.Destinations.CSVFileDestination, NFX'
                                      name='{0}' path='{1}' create-dir='true' name-time-format=''
                        />
                 </log>".Args(TNAME, TEST_DIR);

         var svc = new LSVC(null);

         var fname = Path.Combine(TEST_DIR, FNAME);

         if (File.Exists(fname)) File.Delete(fname);  

         using (Scope.OnExit(() => File.Delete(fname)))
         {
           svc.Configure(XMLConfiguration.CreateFromXML(xml).Root);

           svc.Start();


           svc.Start();

               svc.Write(new Message{Text = "1 message"});
               svc.Write(new Message{Text = "2 message"});
               svc.Write(new Message{Text = "3 message"});


           svc.WaitForCompleteStop();

           Assert.AreEqual(true, File.Exists(fname));
           Assert.AreEqual(3, File.ReadAllLines(fname).Length);
         }
        }

        [Test]
        public void CSVFileDestinationStartByLaConfig()
        {
          string TNAME = "UnitTest-" + MethodInfo.GetCurrentMethod().Name;
          string FNAME = TNAME + ".csv.log";
          const string DATE = "20131012";

          var laStr = @"log 
                        {{ 
                          destination 
                          {{ 
                            type='NFX.Log.Destinations.CSVFileDestination, NFX' 
                            name='{0}'
                            filename='$(@~path)$(::now fmt=yyyyMMdd value={1})-$($name).csv.log'
                          }} 
                        }}".Args(TNAME, DATE);

          var cnf = LaconicConfiguration.CreateFromString(laStr);
          cnf.EnvironmentVarResolver = new Vars() { { "path", TEST_DIR}};

          string fname = Path.Combine(TEST_DIR, DATE + "-" + FNAME);
          IOMiscUtils.EnsureFileEventuallyDeleted(fname);

          var logService = new LSVC(null);

          using (Scope.OnExit(() => File.Delete(fname)))
          {
            logService.Configure(cnf.Root);

            logService.Start();

            logService.Write(new Message() { Text = "Msg 1"});
            logService.Write(new Message() { Text = "Msg 2" });
            logService.Write(new Message() { Text = "Msg 3" });

            logService.WaitForCompleteStop();

            Assert.IsTrue(File.Exists(fname));
            Assert.AreEqual(3, File.ReadAllLines(fname).Length);
          }
        }

        [Test]
        public void FloodFilter()
        {
         string TNAME = "UnitTest-" + MethodBase.GetCurrentMethod().Name;
         string FNAME = TNAME + ".csv.log";
         var svc = new LSVC(null);

         var fname = Path.Combine(TEST_DIR, FNAME);

         if (File.Exists(fname)) File.Delete(fname);  

         try
         {
           svc.RegisterDestination(
               new FloodFilter(new CSVFileDestination(TNAME, TEST_DIR){ CreateDir = true })
                   { 
                       Interval = TimeSpan.FromSeconds(10),
                       MaxCount = 1000,
                       MaxTextLength = 1024
                   }
           );
  
           svc.Start();
  
               for (var i=0; i < 100000; i++)
                 svc.Write(new Message{Text = i.ToString() +" message"});
  
  
           svc.WaitForCompleteStop();
  
           Assert.AreEqual(true, File.Exists(fname));
           Assert.AreEqual(1, File.ReadAllLines(fname).Length);
           Assert.IsTrue( new FileInfo(fname).Length < 1500);
         }
         finally
         {
           File.Delete(fname);
         }
        }

        [Test]
        public void CSVDestinationFilenameDefaultTest()
        {
            string TNAME = "TestDest-" + MethodBase.GetCurrentMethod().Name;
            string FNAME = Path.Combine(TEST_DIR, TNAME + CSVFileDestination.DEFAULT_EXTENSION);

            File.Delete(FNAME);

            //var log = new TestMemoryLog();
            //using (var app = new TestApplication { Log = log })
            using (var svc = new TSLS())
            using (Scope.OnExit(() => File.Delete(FNAME)))
            {
                svc.RegisterDestination(new CSVFileDestination(TNAME, TEST_DIR));
                svc.Start();

                Assert.IsTrue(File.Exists(FNAME));

                svc.WaitForCompleteStop();
            }
        }

        [Test]
        public void CSVDestinationFilenameConfigTest()
        {
            DateTime now = App.LocalizedTime;

            string TNAME = "TestDest" + MethodBase.GetCurrentMethod().Name;
            string FNAME = Path.Combine(TEST_DIR, "{0}-{1:yyyyMMdd}{2}".Args(TNAME, now, CSVFileDestination.DEFAULT_EXTENSION));

            File.Delete(FNAME);

            var xml= @"<log>
                            <destination type='NFX.Log.Destinations.CSVFileDestination, NFX'
                             name='{0}' path='{1}' create-dir='true' name-time-format='yyyyMMdd'/>
                       </log>".Args(TNAME, TEST_DIR);

            //var log = new TestMemoryLog();
            //using (var app = new TestApplication { Log = log })
            using (var svc = new TSLS())
            using (Scope.OnExit(() => File.Delete(FNAME)))
            {
                svc.Configure(XMLConfiguration.CreateFromXML(xml).Root);
                svc.Start();

                Assert.IsTrue(File.Exists(FNAME));

                svc.WaitForCompleteStop();
            }
        }

        [Test]
        public void DebugDestinationFilenameDefaultTest()
        {
            string TNAME = "TestDest-" + MethodBase.GetCurrentMethod().Name;
            string FNAME = Path.Combine(TEST_DIR, TNAME + ".log");

            File.Delete(FNAME);

            //var log = new TestMemoryLog();
            //using (var app = new TestApplication { Log = log })
            using (var svc = new TSLS())
            using (Scope.OnExit(() => File.Delete(FNAME)))
            {
                svc.RegisterDestination(new DebugDestination(TNAME, FNAME));
                svc.Start();

                Assert.IsTrue(File.Exists(FNAME));

                svc.WaitForCompleteStop();
            }
        }

        [Test]
        public void FileDestinationFilenameConfigTest()
        {
            DateTime now = App.LocalizedTime;

            string TNAME = "TestDest-" + MethodBase.GetCurrentMethod().Name;
            string FNAME = Path.Combine(TEST_DIR, "{0:yyyyMMdd}-{1}.log".Args(now, TNAME));

            File.Delete(FNAME);

            var xml= @"<log>
                            <destination type='NFX.Log.Destinations.DebugDestination, NFX' name='{0}' path='{1}'/>
                       </log>".Args(TNAME, TEST_DIR);

            //var log = new TestMemoryLog();
            //using (var app = new TestApplication { Log = log })
            using (var svc = new TSLS())
            using (Scope.OnExit(() => File.Delete(FNAME)))
            {
                svc.Configure(XMLConfiguration.CreateFromXML(xml).Root);
                svc.Start();

                Assert.IsTrue(File.Exists(FNAME));

                svc.WaitForCompleteStop();
            }
        }

        [Test]
        public void DebugDestinationWriteTest()
        {
            string TNAME = "TestDest-" + MethodBase.GetCurrentMethod().Name;
            string FNAME = Path.Combine(TEST_DIR, TNAME + ".log");

            File.Delete(FNAME);

            //var log = new TestMemoryLog();
            //using (var app = new TestApplication { Log = log })
            using (var svc = new TSLS())
            using (Scope.OnExit(() => File.Delete(FNAME)))
            {
                svc.RegisterDestination(new DebugDestination(TNAME, TNAME + ".log", TEST_DIR));
                svc.Start();

                DateTime now = new DateTime(2013, 1, 2, 3, 4, 5);

                for (var i=0; i < 10; i++)
                    svc.Write(new Message { Text = i.ToString(), TimeStamp = now });

                svc.WaitForCompleteStop();

                string[] lines = File.ReadAllLines(FNAME);

                Assert.AreEqual(10, lines.Length);
                lines.Select((s, i) =>
                {
                    Assert.AreEqual(
                        "20130102-030405.000000|   {0}||Debug|{1}||0|".Args(Thread.CurrentThread.ManagedThreadId, i),
                        s);
                    return 0;
                });
            }
        }

        [Test]
        public void LogLevelsTest()
        {
            DateTime now  = App.LocalizedTime;
            DateTime time = new DateTime(now.Year, now.Month, now.Day, 3,4,5);

            string TNAME = "TestDest-" + MethodBase.GetCurrentMethod().Name;
            string FNAME = Path.Combine(TEST_DIR, "{0:yyyyMMdd}-{1}.log".Args(now, TNAME));

            File.Delete(FNAME);

            var xml= @"<log>
                            <destination type='NFX.Log.Destinations.DebugDestination, NFX'
                                name='{0}' path='{1}'
                                levels='DebugB-DebugC,InfoB-InfoD,Warning,Emergency'/>
                       </log>".Args(TNAME, TEST_DIR);

            //var log = new TestMemoryLog();
            //using (var app = new TestApplication { Log = log })
            using (var svc = new TSLS())
            using (Scope.OnExit(() => File.Delete(FNAME)))
            {
                svc.Configure(XMLConfiguration.CreateFromXML(xml).Root);
                svc.Start();

                Assert.IsTrue(File.Exists(FNAME));

                Array mts = Enum.GetValues(typeof(MessageType));

                foreach (var mt in mts)
                    svc.Write(new Message { Type = (MessageType)mt, Text = ((int)mt).ToString(), TimeStamp = now });

                svc.WaitForCompleteStop();

                string[] lines = File.ReadAllLines(FNAME);

                Assert.AreEqual(7, lines.Length);

                lines.Select((s,i) =>
                {
                    var sa = s.Split('|');
                    MessageType mt;
                    
                    Assert.IsTrue(Enum.TryParse(sa[3], out mt));
                    Assert.AreEqual(
                        "{0:yyyyMMdd}-030405.000000|   {1}||Debug|{2}||0|".Args(now, Thread.CurrentThread.ManagedThreadId, (int)mt),
                        s);
                    return 0;
                });
            }

            Assert.AreEqual(
                new Destination.LevelsList { new Tuple<MessageType, MessageType>(MessageType.DebugA, MessageType.DebugZ) },
                Destination.ParseLevels("DebugA-DebugZ"));

            Assert.AreEqual(
                new Destination.LevelsList { new Tuple<MessageType, MessageType>(MessageType.Debug, MessageType.Info) },
                Destination.ParseLevels("-Info"));

            Assert.AreEqual(
                new Destination.LevelsList { new Tuple<MessageType, MessageType>(MessageType.Info, MessageType.CatastrophicError) },
                Destination.ParseLevels("Info-"));

            Assert.AreEqual(
                new Destination.LevelsList { new Tuple<MessageType, MessageType>(MessageType.Trace, MessageType.TraceZ),
                                             new Tuple<MessageType, MessageType>(MessageType.Info, MessageType.CatastrophicError) },
                Destination.ParseLevels("Trace - TraceZ, Info-"));

            Assert.AreEqual(
                new Destination.LevelsList { new Tuple<MessageType, MessageType>(MessageType.Trace, MessageType.Trace),
                                             new Tuple<MessageType, MessageType>(MessageType.Info, MessageType.Info),
                                             new Tuple<MessageType, MessageType>(MessageType.Warning, MessageType.Warning) },
                Destination.ParseLevels("Trace | Info | Warning"));

            Assert.AreEqual(
                new Destination.LevelsList { new Tuple<MessageType, MessageType>(MessageType.Trace, MessageType.Trace),
                                             new Tuple<MessageType, MessageType>(MessageType.Info, MessageType.Info) },
                Destination.ParseLevels("Trace; Info"));
        }



    }
}
