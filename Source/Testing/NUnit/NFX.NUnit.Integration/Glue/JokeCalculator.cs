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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using BusinessLogic;
using NFX.ApplicationModel;
using NFX.Environment;
using NFX.Glue;
using NFX.Glue.Implementation;
using NFX.Glue.Native;
using NFX.Security;
using NUnit.Framework;

namespace NFX.NUnit.Integration.Glue
{
  [TestFixture]
  public class JokeCalculator: JokeTestBase
  {
    [Test]
    [ExpectedException(typeof(RemoteException))]
    public void ExceptionBeforeInit()
    {
      using (JokeHelper.MakeApp())
      {
        var cl = new JokeCalculatorClient(TestServerSyncNode);
        cl.Headers.Add(new NFX.Glue.Protocol.AuthenticationHeader(TestCredentials));

        int result = cl.Add(1);
      }
    }

    [Test]
    [ExpectedException(typeof(RemoteException))]
    public void ExceptionAfterDestructor()
    {
      using (JokeHelper.MakeApp())
      {
        var cl = new JokeCalculatorClient(TestServerSyncNode);
        cl.Headers.Add(new NFX.Glue.Protocol.AuthenticationHeader(TestCredentials));

        cl.Init(0);
        cl.Done();

        int result = cl.Add(1);
      }
    }

    [Test]
    public void Sync_JokeCalculator_TestAdd()
    {
      using (JokeHelper.MakeApp())
      {
        var cl = new JokeCalculatorClient(TestServerSyncNode);
        cl.Headers.Add(new NFX.Glue.Protocol.AuthenticationHeader(TestCredentials));

        using (Scope.OnExit(() => cl.Done()))
        {
          cl.Init(0);
          cl.Add(10);
          int result = cl.Sub(3);

          Assert.AreEqual(7, result); 
        }
      }
    }

  }
}
