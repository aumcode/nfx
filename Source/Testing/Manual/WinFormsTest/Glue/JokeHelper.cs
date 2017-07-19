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
using NFX.ApplicationModel;
using NFX.Environment;

namespace WinFormsTest.Glue
{
  public class JokeHelper
  {
    const string CONFIG_STR = @"
nfx
{
	disk-root=$(~NFX_TEST_ROOT)\
	log-root=$(\$disk-root)
	log-csv='NFX.Log.Destinations.CSVFileDestination, NFX'
	debug-default-action='Log,Throw'
	
	glue
	{
		bindings
		{
			binding
			{
				name='sync'
				type='NFX.Glue.Native.SyncBinding'
				max-msg-size='65535'
				
				client-inspectors
				{
					inspector { type='BusinessLogic.TextInfoReporter, BusinessLogic' }
				}
				
				client-transport
				{
          rcv-buf-size='8192'
          snd-buf-size='8192'
          rcv-timeout='10000'
          snd-timeout='10000'
				}
			}			
      
      binding
      {
        name='async'
        type='NFX.Glue.Native.AsyncSlimBinding'
        io-buf-size='16384'
        max-async-io-ops='10000'
        
        client-transport
        {
            rcv-buf-size='131072'
            snd-buf-size='131072'
            Zno-delay='false'
        }
      }
      
      binding
      {
        name='inproc'
        type='NFX.Glue.Native.InProcBinding, NFX'
      }
		}
    
    servers
    {
      server
      {
        name='local'
        node='inproc://localhost'
        contract-servers='TestServer.Glue.JokeServer, TestServer; TestServer.Glue.JokeCalculatorServer, TestServer'
      }

    }
	}
}
";

    public static ServiceBaseApplication MakeApp()
    {
      var configuration = LaconicConfiguration.CreateFromString(CONFIG_STR);

      return new ServiceBaseApplication(new string[] { }, configuration.Root);
    }
  }
}
