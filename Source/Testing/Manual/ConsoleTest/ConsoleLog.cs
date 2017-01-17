/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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

using NFX.Log;
using NFX.Log.Destinations;


namespace ConsoleTest
{
    static class ConsoleLog
    {
      
      public static void Run()
      {
        var svc = new LogService(null);
        var dest = new ConsoleDestination("konzol");
        dest.Colored = true;

        var filter = new FloodFilter(dest) { Interval = TimeSpan.FromSeconds(3d) };

        //filter.MinLevel = MessageType.Error;
        //filter.MaxLevel = MessageType.Info;

        svc.RegisterDestination(filter);

        svc.Start();


        svc.Write(new Message{ Type = MessageType.Info, Text = "This is info", From = "Tukhis" });
        
     System.Threading.Thread.Sleep(4000);
       
        svc.Write(new Message{ Type = MessageType.Warning, Text = "Warning text goes here", From = "Beitzhen" });
     
     System.Threading.Thread.Sleep(3500);   
        svc.Write(new Message{ Type = MessageType.Error, Text = "This is error line", From = "Moisha" });

        for(var i=0; i<1000000; i++)
         svc.Write(new Message{ Type = MessageType.Info, Text = "Loop trakhen" + i.ToString(), From = "Loop" });


        svc.WaitForCompleteStop();
      }

    }
}
