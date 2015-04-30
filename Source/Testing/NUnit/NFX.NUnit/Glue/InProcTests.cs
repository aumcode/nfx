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
    public class InProcTests
    {                              
   const string CONF_SRC_INPROC =@"
 nfx
 {
  cs='inproc://'
  
  object-store
  {
    guid='B05D3038-A821-4BE0-96AA-E6D24DFA746F'
    provider {name='nop' type='NFX.ApplicationModel.Volatile.NOPObjectStoreProvider, NFX'}
  }
  
  glue
  {
     bindings
     {
        binding { name=inproc type='NFX.Glue.Native.InProcBinding, NFX'}
     }

     servers
     {
        server { name='test' node='inproc://' contract-servers='NFX.NUnit.Glue.TestServerA, NFX.NUnit;NFX.NUnit.Glue.TestServerB_ThreadSafe, NFX.NUnit'}
     }
  }
 }
 "; 
        [TestCase]
        public void Inproc_A_TwoWayCall()
        {
            TestLogic.TestContractA_TwoWayCall(CONF_SRC_INPROC);
        }

        [TestCase]
        public void Inproc_TASK_A_TwoWayCall()
        {
            TestLogic.TASK_TestContractA_TwoWayCall(CONF_SRC_INPROC);
        }

        [TestCase]
        public void Inproc_TASKReturning_A_TwoWayCall()
        {
            TestLogic.TASKReturning_TestContractA_TwoWayCall(CONF_SRC_INPROC);
        }

        ////////this test will always fail for inproc, as there is no timeout for inproc because DipsatchRequest() blocks
        //////[TestCase]
        //////public void Inproc_TASK_A_TwoWayCall_Timeout()
        //////{
        //////    TestLogic.TASK_TestContractA_TwoWayCall_Timeout(CONF_SRC_INPROC);
        //////}

        [TestCase]
        public void Inproc_A_OneWayCall()
        {
            TestLogic.TestContractA_OneWayCall(CONF_SRC_INPROC);
        }

        [TestCase]
        public void Inproc_B_1()
        {
            TestLogic.TestContractB_1(CONF_SRC_INPROC);
        }

        [TestCase]
        public void Inproc_B_1_Async()
        {
            TestLogic.TestContractB_1_Async(CONF_SRC_INPROC);
        }

        [TestCase]
        public void Inproc_B_2()
        {
            TestLogic.TestContractB_2(CONF_SRC_INPROC);
        }

        [TestCase]
        public void Inproc_B_3()
        {
            TestLogic.TestContractB_3(CONF_SRC_INPROC);
        }


        [TestCase]
        public void Inproc_B_4()
        {
            TestLogic.TestContractB_4(CONF_SRC_INPROC);
        }

        [TestCase]
        public void Inproc_B_4_Async()
        {
            TestLogic.TestContractB_4_Async(CONF_SRC_INPROC);
        }

        [TestCase]
        public void Inproc_B_4_AsyncReactor()
        {
            TestLogic.TestContractB_4_AsyncReactor(CONF_SRC_INPROC);
        }

        [TestCase]
        public void Inproc_B_4_Parallel()
        {
            TestLogic.TestContractB_4_Parallel(CONF_SRC_INPROC, threadSafe: true);
        }

        [TestCase]
        public void Inproc_B_4_Marshalling_Parallel()
        {
            TestLogic.TestContractB_4_Marshalling_Parallel(CONF_SRC_INPROC, threadSafe: true);
        }



        [TestCase]
        public void Inproc_TASK_B_4_Parallel()
        {
            TestLogic.TASK_TestContractB_4_Parallel(CONF_SRC_INPROC, threadSafe: true);
        }

        [TestCase]
        public void Inproc_B_4_Parallel_ManyClients()
        {
            TestLogic.TestContractB_4_Parallel_ManyClients(CONF_SRC_INPROC, threadSafe: true);
        }

        [TestCase]
        public void Inproc_B_5()
        {
            TestLogic.TestContractB_5(CONF_SRC_INPROC);
        }


        [TestCase]
        public void Inproc_B_6()
        {
            TestLogic.TestContractB_6(CONF_SRC_INPROC);
        }

        [TestCase]
        public void Inproc_B_7()
        {
            TestLogic.TestContractB_7(CONF_SRC_INPROC);
        }

        [TestCase]
        public void Inproc_B_8()
        {
            TestLogic.TestContractB_8(CONF_SRC_INPROC);
        }


    }

}
