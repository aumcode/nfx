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
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;


using NFX.Log;
using NFX.Log.Destinations;
using NFX.Environment;
using NFX.ApplicationModel;



namespace NFX.NUnit.Logging
{
    [TestFixture]
    public class VariousDestinations
    {
 private const string CONF_SRC1 =@"
 nfx
 {
  log
  {
    destination{ name='mem1' type='NFX.Log.Destinations.MemoryBufferDestination, NFX'}
  }
 }
 ";  
       
        [TestCase]
        public void Configed_MemoryBufferDestination()
        {
       
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC1);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var mbd = ((LogService)app.Log).Destinations.First() as MemoryBufferDestination;

                System.Threading.Thread.Sleep( 3000 );
                mbd.ClearBuffer();
                
                
                app.Log.Write( new Message{ Type = Log.MessageType.Info, From = "test", Text = "Hello1"});
                System.Threading.Thread.Sleep( 1000 );
                app.Log.Write( new Message{ Type = Log.MessageType.Info, From = "test", Text = "Hello2"});

                System.Threading.Thread.Sleep( 3000 );

                Assert.AreEqual(2, mbd.Buffered.Count());

                Assert.AreEqual("Hello1", mbd.BufferedTimeAscending.First().Text);
                Assert.AreEqual("Hello2", mbd.BufferedTimeAscending.Last().Text);

                Assert.AreEqual("Hello2", mbd.BufferedTimeDescending.First().Text);
                Assert.AreEqual("Hello1", mbd.BufferedTimeDescending.Last().Text);
            }
        }

        [TestCase]
        public void Configed_MemoryBufferDestinationCapacity()
        {
       
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC1);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {
                var mbd = ((LogService)app.Log).Destinations.First() as MemoryBufferDestination;

                System.Threading.Thread.Sleep( 3000 );
                mbd.BufferSize = 10;
                
                for(int i=0; i<100; i++)
                    app.Log.Write( new Message{Type = Log.MessageType.Info, From = "test", Text = "i={0}".Args(i)} );
                System.Threading.Thread.Sleep( 3000 );

                Assert.AreEqual(10, mbd.Buffered.Count());

                Assert.AreEqual("i=99", mbd.BufferedTimeDescending.First().Text);
                Assert.AreEqual("i=90", mbd.BufferedTimeDescending.Last().Text);
            }
        }


    }
}