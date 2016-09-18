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
using System.Text;

using NUnit.Framework;


using NFX.ApplicationModel;
using NFX.Log;

namespace NFX.NUnit.Config
{
    [TestFixture]
    public class Behavior
    {

    static string conf1 = @"
 <root>
    
    <log>
     <behaviors>
       <behavior type='NFX.NUnit.Config.AlwaysLogBehavior, NFX.NUnit' />
     </behaviors>
                   <!-- Notice the behavior is defined at log level where it is applied by injecting destination -->
    </log>

 </root>
";

 static string conf2 = @"
 <root>
     <behaviors>
       <behavior type='NFX.NUnit.Config.AlwaysLogBehavior, NFX.NUnit' cascade='true'/>
     </behaviors>

    <log>
                 <!-- Notice the behavior is defined at application level, but it is still applied here because it cascades down the tree -->
    </log>

 </root>
";

static string conf3 = @"
 <root>
     <behaviors>
       <behavior type='NFX.NUnit.Config.AlwaysLogBehavior, NFX.NUnit' cascade='false'/>
     </behaviors>

    <log>
             <!-- Notice the behavior is defined at application level, but it is NOT applied here because it does not cascade down the tree -->
    </log>

 </root>
";


        [TestCase]
        public void Case1_LogLevel()
        {
            var root = NFX.Environment.XMLConfiguration.CreateFromXML(conf1).Root;
            using(var app = new ServiceBaseApplication(new string[0], root ))
            {
                app.Log.Write(Log.MessageType.Info, "Khello!");

                Assert.AreEqual(1, ((LogService)app.Log).Destinations.Count());
                System.Threading.Thread.Sleep(1000);//wait for flush
                Assert.IsNotNull( ((listDestination)((LogService)app.Log).Destinations.First()).List.FirstOrDefault(m=> m.Text == "Khello!") );
            }

        }

        [TestCase]
        public void Case2_CascadeFromAppLevel()
        {
            var root = NFX.Environment.XMLConfiguration.CreateFromXML(conf2).Root;
            using(var app = new ServiceBaseApplication(new string[0],  root ))
            {
                app.Log.Write(Log.MessageType.Info, "Khello!");

                Assert.AreEqual(1, ((LogService)app.Log).Destinations.Count());
                System.Threading.Thread.Sleep(1000);//wait for flush
                Assert.IsNotNull( ((listDestination)((LogService)app.Log).Destinations.First()).List.FirstOrDefault(m=> m.Text == "Khello!") );
            }

        }

        [TestCase]
        [ExpectedException(typeof(NFXException), ExpectedMessage="No log destinations registered", MatchType=MessageMatch.Contains)]
        public void Case3_ExistsOnAppLevelButDoesNotCascade()
        {
            var root = NFX.Environment.XMLConfiguration.CreateFromXML(conf3).Root;
            using(var app = new ServiceBaseApplication(new string[0],  root ))
            {
                
            }

        }

    }



                        internal class listDestination : NFX.Log.Destinations.Destination
                        {
                            public MessageList List = new MessageList();

                            protected internal override void DoSend(Message entry)
                            {
                              List.Add(entry);  
                            }
                        }


                        public class AlwaysLogBehavior : NFX.Environment.Behavior
                        {
                            public AlwaysLogBehavior() : base() {}

                            public override void Apply(object target)
                            {
                                if (target is Log.LogService)
                                {
                                   var svc = (Log.LogService)target;

                                   svc.RegisterDestination( new listDestination());
                                }
                            }
                        }
}
