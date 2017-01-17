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

// dlatushkin 2015022: to allow lietaning to port w/o administrative rights the following command should be executed:
// netsh http add urlacl url=http://+:8080/ user=[USER_NAME]

using System;

using NFX;
using NFX.Wave;
using NFX.ApplicationModel;

namespace WaveTestSite
{
  class Program
  {
    static void Main(string[] args)
    {
       // System.Threading.ThreadPool.SetMaxThreads(3000, 3000);
       // System.Threading.ThreadPool.SetMinThreads(256, 256);

       int wt, ct;
       System.Threading.ThreadPool.GetAvailableThreads(out wt, out ct);
       Console.WriteLine("Worker: {0}  Completion: {1}", wt, ct);

        try
        {
           using(new ServiceBaseApplication(args, null))
             using(var ws = new WaveServer())
             {
                ws.Configure(null);
                ws.Start();
                Console.WriteLine("Web server started. Press <ENTER> to terminate...");
                Console.ReadLine();
             }
        }
        catch(Exception error)
        {
          Console.WriteLine("Exception leaked");
          Console.WriteLine("----------------");
          Console.WriteLine(error.ToMessageWithType());
          System.Environment.ExitCode = -1;
        }
    }
  }
}
