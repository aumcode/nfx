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
using System.IO;
using NUnit.Framework;


using NFX;
using NFX.ApplicationModel;
using NFX.Environment;
using NFX.Glue.Native;
using NFX.Glue;
using NFX.IO;

namespace NFX.NUnit.Glue
{
    [TestFixture]
    public class MpxTests
    {                              
   const string CONF_SRC_MPX =@"
 nfx
 {
  cs='mpx://127.0.0.1:5701'
  cs2='mpx://127.0.0.1:5702'
  

  log
  {
    destination{ type = 'NFX.Log.Destinations.ConsoleDestination, NFX'}
  }

  object-store
  {
    guid='B05D3038-A821-4BE0-96AA-E6D24DFA746F'
    provider {name='nop' type='NFX.ApplicationModel.Volatile.NOPObjectStoreProvider, NFX'}
  }

  glue
  {
     bindings
     {
        binding { name=mpx type='NFX.Glue.Native.MpxBinding, NFX' max-msg-size=900000}
     }

     servers
     {
        server { name='test1' node='mpx://*:5701' contract-servers='NFX.NUnit.Glue.TestServerA, NFX.NUnit;NFX.NUnit.Glue.TestServerB_ThreadSafe, NFX.NUnit'}
        server { name='test2' node='mpx://*:5702' contract-servers='NFX.NUnit.Glue.TestServerB_NotThreadSafe, NFX.NUnit'}
     }
  }
 }
 "; 
       

        const string CONF_SRC_MPX_TRANSPORTS_A =@"
 nfx
 {
  cs='mpx://127.0.0.1:5701'
  cs2='mpx://127.0.0.1:5702'
  
  object-store
  {
    guid='B05D3038-A821-4BE0-96AA-E6D24DFA746F'
    provider {name='nop' type='NFX.ApplicationModel.Volatile.NOPObjectStoreProvider, NFX'}
  }

  glue
  {
     bindings
     {
        binding 
        {
          name=mpx type='NFX.Glue.Native.MpxBinding, NFX'
          max-msg-size=900000
          client-transport {  max-count=1 }
        }
     }

     servers
     {
        server { name='test1' node='mpx://*:5701' contract-servers='NFX.NUnit.Glue.TestServerA, NFX.NUnit;NFX.NUnit.Glue.TestServerB_ThreadSafe, NFX.NUnit'}
        server { name='test2' node='mpx://*:5702' contract-servers='NFX.NUnit.Glue.TestServerB_NotThreadSafe, NFX.NUnit'}
     }
  }
 }
 "; 
       
       
       
       
       
        [TestCase]
        public void Mpx_A_TwoWayCall()
        {
            TestLogic.TestContractA_TwoWayCall(CONF_SRC_MPX);
        }

        [TestCase]
        public void Mpx_TASK_A_TwoWayCall()
        {
            TestLogic.TASK_TestContractA_TwoWayCall(CONF_SRC_MPX);
        }

        [TestCase]
        public void Mpx_TASKReturning_A_TwoWayCall()
        {
            TestLogic.TASKReturning_TestContractA_TwoWayCall(CONF_SRC_MPX);
        }

        [TestCase]
        public void Mpx_A_TwoWayCall_Timeout()
        {
            TestLogic.TestContractA_TwoWayCall_Timeout(CONF_SRC_MPX);
        }

        [TestCase]
        public void Mpx_TASK_A_TwoWayCall_Timeout()
        {
            TestLogic.TASK_TestContractA_TwoWayCall_Timeout(CONF_SRC_MPX);
        }

        [TestCase]
        public void Mpx_A_OneWayCall()
        {
            TestLogic.TestContractA_OneWayCall(CONF_SRC_MPX);
        }

        [TestCase]
        public void Mpx_B_1()
        {
            TestLogic.TestContractB_1(CONF_SRC_MPX);
        }

        [TestCase]
        public void Mpx_B_1_Async()
        {
            TestLogic.TestContractB_1_Async(CONF_SRC_MPX);
        }

        [TestCase]
        public void Mpx_B_2()
        {
            TestLogic.TestContractB_2(CONF_SRC_MPX);
        }

        [TestCase]
        public void Mpx_B_3()
        {
            TestLogic.TestContractB_3(CONF_SRC_MPX);
        }

        [TestCase]
        public void Mpx_B_4()
        {
            TestLogic.TestContractB_4(CONF_SRC_MPX);
        }

        [TestCase]
        public void Mpx_B_4_Async()
        {
            TestLogic.TestContractB_4_Async(CONF_SRC_MPX);
        }

        [TestCase]
        public void Mpx_B_4_Async_TR_A()
        {
            TestLogic.TestContractB_4_Async(CONF_SRC_MPX_TRANSPORTS_A);
        }

        [TestCase]
        public void Mpx_B_4_AsyncReactor()
        {
            TestLogic.TestContractB_4_AsyncReactor(CONF_SRC_MPX);
        }

        [TestCase]
        public void Mpx_B_4_AsyncReactor_TR_A()
        {
            TestLogic.TestContractB_4_AsyncReactor(CONF_SRC_MPX_TRANSPORTS_A);
        }

        [TestCase]
        public void Mpx_B_4_Parallel_ThreadSafeServer()
        {
            TestLogic.TestContractB_4_Parallel(CONF_SRC_MPX, threadSafe: true);
        }

        [TestCase]
        public void Mpx_B_4_Parallel_Marshalling_ThreadSafeServer()
        {
            TestLogic.TestContractB_4_Marshalling_Parallel(CONF_SRC_MPX, threadSafe: true);
        }


        [TestCase]
        public void Mpx_TASK_B_4_Parallel_ThreadSafeServer()
        {
            TestLogic.TASK_TestContractB_4_Parallel(CONF_SRC_MPX, threadSafe: true);
        }

        [TestCase]
        public void Mpx_B_4_Parallel_ThreadSafeServer_TR_A()
        {
            TestLogic.TestContractB_4_Parallel(CONF_SRC_MPX_TRANSPORTS_A, threadSafe: true);
        }

        [TestCase]
        public void Mpx_B_4_Parallel_NotThreadSafeServer()
        {
            TestLogic.TestContractB_4_Parallel(CONF_SRC_MPX, threadSafe: false);
        }

        [TestCase]
        public void Mpx_B_4_Parallel_NotThreadSafeServer_TR_A()
        {
            TestLogic.TestContractB_4_Parallel(CONF_SRC_MPX_TRANSPORTS_A, threadSafe: false);
        }

        //-----

        [TestCase]
        public void Mpx_B_4_Parallel_ThreadSafeServer_ManyClients()
        {
            TestLogic.TestContractB_4_Parallel_ManyClients(CONF_SRC_MPX, threadSafe: true);
        }

        [TestCase]
        public void Mpx_B_4_Parallel_ThreadSafeServer_ManyClients_TR_A()
        {
            TestLogic.TestContractB_4_Parallel_ManyClients(CONF_SRC_MPX_TRANSPORTS_A, threadSafe: true);
        }

        [TestCase]
        public void Mpx_B_4_Parallel_NotThreadSafeServer_ManyClients()
        {
            TestLogic.TestContractB_4_Parallel_ManyClients(CONF_SRC_MPX, threadSafe: false);
        }

        [TestCase]
        public void Mpx_B_4_Parallel_NotThreadSafeServer_ManyClients_TR_A()
        {
            TestLogic.TestContractB_4_Parallel_ManyClients(CONF_SRC_MPX_TRANSPORTS_A, threadSafe: false);
        }


        [TestCase]
        public void Mpx_B_5()
        {
            TestLogic.TestContractB_5(CONF_SRC_MPX);
        }

        [TestCase]
        public void Mpx_B_6()
        {
            TestLogic.TestContractB_6(CONF_SRC_MPX);
        }

        [TestCase]
        public void Mpx_B_7()
        {
            TestLogic.TestContractB_7(CONF_SRC_MPX);
        }

         [TestCase]
        public void Mpx_B_8()
        {
            TestLogic.TestContractB_8(CONF_SRC_MPX);
        }

        [TestCase]
        public void Mpx_B_9()
        {
            TestLogic.TestContractB_9(CONF_SRC_MPX);
        }

       


    }

}
