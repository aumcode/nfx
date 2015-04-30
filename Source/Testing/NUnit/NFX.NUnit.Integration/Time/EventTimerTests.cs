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
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;

using NUnit.Framework;

using NFX;
using NFX.ApplicationModel;
using NFX.Time;

namespace NFX.NUnit.Integration.Time
{
                           public class TeztHandler : IEventHandler
                           {
                             public static List<string> s_List = new List<string>();

                             public TeztHandler(string name, bool podonok)
                             {
                               Name = "{0}.{1}".Args(name, podonok);
                             }

                             public readonly string Name;

                             public void EventHandlerBody(Event sender)
                             {
                                var who = "{0}::{1}".Args(sender.Context, Name);
                                lock(s_List)
                                 s_List.Add(who);
                             }

                             public void EventStatusChange(Event sender, EventStatus priorStatus)
                             {
                             }

                             public void EventDefinitionChange(Event sender, string parameterName)
                             {
                             }
                           }



  /// <summary>
  /// To perform tests below MySQL server instance is needed.
  /// Look at CONNECT_STRING constant
  /// </summary>
  [TestFixture]
  public class EventTimerTests
  {
        
        public const string CONFIG1=@"
nfx
{
  event-timer
  {
    resolution-ms=150 
  }
}";

        public const string CONFIG2_ARZAMAS=@"
nfx
{
  time-location {utc-offset='04:45:10' description='Arzamas-7'}
  
  event-timer
  {
    resolution-ms=150 
  }
}";

    public const string CONFIG3_HANDLERS=@"
nfx
{
  event-timer
  {
    resolution-ms=150
    event{ name='A' interval='0:0:1' Context='ItonTV' handler{type='NFX.NUnit.Integration.Time.TeztHandler, NFX.NUnit.Integration' arg0='Gorin' arg1='true'} } 
    event{ name='B' interval='0:0:3' Context='Nativ'  handler{type='NFX.NUnit.Integration.Time.TeztHandler, NFX.NUnit.Integration' arg0='Kedmi' arg1='false'} } 
  }
}";

       public static readonly TimeSpan ARZAMAS_OFFSET = TimeSpan.Parse("04:45:10");


        [Test]
        public void T1()
        {
            using(var app = new ServiceBaseApplication(null, CONFIG1.AsLaconicConfig()))
            {
              Assert.AreEqual(150, app.EventTimer.ResolutionMs);

              var lst = new List<string>();

              new Event(app.EventTimer, "A", (e) => {lock(lst) lst.Add("a");}) { Interval = new TimeSpan(0,0,1), Context = 123 };
              new Event(app.EventTimer, "B", (e) => {lock(lst) lst.Add("b");}) { Interval = new TimeSpan(0,0,3), Context = 234 };

              Assert.AreEqual(2, app.EventTimer.Events.Count);
              Assert.AreEqual(123, app.EventTimer.Events["A"].Context);
              Assert.AreEqual(234, app.EventTimer.Events["B"].Context);


              Thread.Sleep(10000);       
              
              app.EventTimer.Events["A"].Dispose();
              app.EventTimer.Events["B"].Dispose();

              Assert.AreEqual(0, app.EventTimer.Events.Count);

              Console.WriteLine(string.Join(" , ", lst));
              Assert.AreEqual(10, lst.Count(s=>s=="a"));
              Assert.AreEqual(4, lst.Count(s=>s=="b"));
            }   
        }

        [Test]
        public void T2()
        {
            using(var app = new ServiceBaseApplication(null, CONFIG1.AsLaconicConfig()))
            {
              Assert.AreEqual(150, app.EventTimer.ResolutionMs);


              var lst = new List<string>();

              new Event(app.EventTimer, "A", (e) => {lock(lst) lst.Add("a");}) { Interval = new TimeSpan(0,0,1)};
              new Event(app.EventTimer, "B", (e) => {lock(lst) lst.Add("b");}) { Interval = new TimeSpan(0,0,3), Enabled = false};

              Thread.Sleep(10000);       
              
              app.EventTimer.Events["A"].Dispose();
              app.EventTimer.Events["B"].Dispose();

              Assert.AreEqual(0, app.EventTimer.Events.Count);

              Console.WriteLine(string.Join(" , ", lst));
              Assert.AreEqual(10, lst.Count(s=>s=="a"));
              Assert.AreEqual(0, lst.Count(s=>s=="b"));
            }   
        }


        [Test]
        public void T3()
        {
            using(var app = new ServiceBaseApplication(null, CONFIG1.AsLaconicConfig()))
            {
              Assert.AreEqual(150, app.EventTimer.ResolutionMs);


              var lst = new List<string>();

              var appNow = app.LocalizedTime;

              new Event(app.EventTimer, "A", (e) => {lock(lst) lst.Add("a");}) { Interval = new TimeSpan(0,0,1), StartDate = appNow.AddSeconds(3.0)};
              new Event(app.EventTimer, "B", (e) => {lock(lst) lst.Add("b");}) { Interval = new TimeSpan(0,0,3), StartDate = appNow.AddSeconds(-100.0)};

              Thread.Sleep(10000);       
              
              app.EventTimer.Events["A"].Dispose();
              app.EventTimer.Events["B"].Dispose();

              Assert.AreEqual(0, app.EventTimer.Events.Count);

              Console.WriteLine(string.Join(" , ", lst));
              Assert.AreEqual(7, lst.Count(s=>s=="a"));
              Assert.AreEqual(4, lst.Count(s=>s=="b"));
            }   
        }

        
        [Test]
        public void T4()
        {
            using(var app = new ServiceBaseApplication(null, CONFIG1.AsLaconicConfig()))
            {
              Assert.AreEqual(150, app.EventTimer.ResolutionMs);


              var lst = new List<string>();

              var limpopoTime = new TimeLocation(new TimeSpan(0, 0, 2), "Limpopo Time is 2 seconds ahead of UTC");

              var utcNow = app.TimeSource.UTCNow;

              Console.WriteLine( utcNow.Kind );

              new Event(app.EventTimer, "A", (e) => {lock(lst) lst.Add("a");}, new TimeSpan(0,0,1)) { TimeLocation = limpopoTime, StartDate = utcNow.AddSeconds(-2) };
              new Event(app.EventTimer, "B", (e) => {lock(lst) lst.Add("b");}, new TimeSpan(0,0,3)) { TimeLocation = TimeLocation.UTC, StartDate = utcNow };

              Thread.Sleep(10000);       
              
              app.EventTimer.Events["A"].Dispose();
              app.EventTimer.Events["B"].Dispose();

              Assert.AreEqual(0, app.EventTimer.Events.Count);

              Console.WriteLine(string.Join(" , ", lst));
              Assert.AreEqual(10, lst.Count(s=>s=="a"));
              Assert.AreEqual(4, lst.Count(s=>s=="b"));
            }   
        } 


        [Test]
        public void T5()
        {
            using(var app = new ServiceBaseApplication(null, CONFIG2_ARZAMAS.AsLaconicConfig()))
            {
              Assert.AreEqual(150, app.EventTimer.ResolutionMs);


              var lst = new List<string>();          

              var utcNow = app.TimeSource.UTCNow;

              new Event(app.EventTimer, "A", (e) => {lock(lst) lst.Add("a");}) { Interval = new TimeSpan(0,0,1), StartDate = DateTime.SpecifyKind(utcNow + ARZAMAS_OFFSET, DateTimeKind.Local) };
              new Event(app.EventTimer, "B", (e) => {lock(lst) lst.Add("b");}) { Interval = new TimeSpan(0,0,3), StartDate = utcNow };

              Thread.Sleep(10000);       
              
              app.EventTimer.Events["A"].Dispose();
              app.EventTimer.Events["B"].Dispose();

              Assert.AreEqual(0, app.EventTimer.Events.Count);

              Console.WriteLine(string.Join(" , ", lst));
              Assert.AreEqual(10, lst.Count(s=>s=="a"));
              Assert.AreEqual(4, lst.Count(s=>s=="b"));
            }   
        } 


        [Test]
        public void T6()
        {
            var lst = TeztHandler.s_List;
            lst.Clear();
            
            using(var app = new ServiceBaseApplication(null, CONFIG3_HANDLERS.AsLaconicConfig()))
            {
              Thread.Sleep(10000);
            }

            Console.WriteLine(string.Join(" , ", lst));
            Assert.AreEqual(14, lst.Count);
            Assert.AreEqual(10, lst.Count(s=>s=="ItonTV::Gorin.True"));
            Assert.AreEqual(4, lst.Count(s=>s=="Nativ::Kedmi.False"));   
        } 



        [Test]
        public void T7()
        {
            using(var app = new ServiceBaseApplication(null, CONFIG1.AsLaconicConfig()))
            {

              new Event(app.EventTimer, "A")
              { 
                Interval = new TimeSpan(0,0,1), 
                Context = 123,
                StartDate = new DateTime(2079, 12, 12),
                EndDate = new DateTime(1980, 1, 1) 
              };

              Assert.AreEqual(EventStatus.NotStarted, app.EventTimer.Events["A"].Status);
              Thread.Sleep(2000);
              Assert.AreEqual(EventStatus.Invalid, app.EventTimer.Events["A"].Status);

              app.EventTimer.Events["A"].StartDate = new DateTime(1979, 1,1);
              Thread.Sleep(2000);
              Assert.AreEqual(EventStatus.Expired, app.EventTimer.Events["A"].Status);
              
              app.EventTimer.Events["A"].EndDate = new DateTime(2979, 1,1);
              Thread.Sleep(2000);
              Assert.AreEqual(EventStatus.Started, app.EventTimer.Events["A"].Status);
            }   
        }



         [Test]
        public void T8()
        {
            using(var app = new ServiceBaseApplication(null, CONFIG1.AsLaconicConfig()))
            {
              var lst = new List<string>();

              new Event(app.EventTimer, "A", (e) => {lock(lst) lst.Add("a");}) { Interval = new TimeSpan(0,0,1), Context = 123, MaxCount=2 };
              new Event(app.EventTimer, "B", (e) => {lock(lst) lst.Add("b");}) { Interval = new TimeSpan(0,0,3), Context = 234 };

              Thread.Sleep(10000);       
              
              app.EventTimer.Events["A"].Dispose();
              app.EventTimer.Events["B"].Dispose();

              Console.WriteLine(string.Join(" , ", lst));
              Assert.AreEqual(2, lst.Count(s=>s=="a"));
              Assert.AreEqual(4, lst.Count(s=>s=="b"));
            }   
        }


  }
}
