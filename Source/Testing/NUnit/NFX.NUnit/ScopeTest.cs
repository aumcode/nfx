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
using NFX;
using NFX.ApplicationModel;
using NFX.Log;
using NUnit.Framework;

namespace NFX.NUnit
{
    [TestFixture]
    public class ScopeTestFixture
    {
        [TestCase]
        public void ScopeExitTest()
        {
            int  n = 1;
            bool b = true;
            string s0 = "ok";

            {
                using (Scope.OnExit(() => n = 1))
                    n = 2;

                Assert.AreEqual(1, n);
            }

            {
                using (Scope.OnExit<int>(n, (x) => n = x))
                    n = 2;

                Assert.AreEqual(1, n);
            }

            {
                using (Scope.OnExit<int,bool>(n, b, (x,y) => {n=x; b=y;}))
                {
                    n = 5;
                    b = false;
                }

                Assert.AreEqual(1, n);
                Assert.AreEqual(true, b);
            }

            {
                using (Scope.OnExit<int,bool,string>(n, b, s0, (x,y,s) => {n=x; b=y; s0=s;}))
                {
                    n = 5;
                    b = false;
                    s0 = "bad";
                }

                Assert.AreEqual(1, n);
                Assert.AreEqual(true, b);
                Assert.AreEqual("ok", s0);
            }


            {
                var text = "Cool scopper!";
                using (var log = new TestMemoryLog())
                using (var app = new TestApplication { Log = log })
                {
                    log.Start();

                    using (new Scope(logText: text, logMsgType: MessageType.Warning, measureTime: true))
                    {
                        for (int i = 0; i < 1000000; i++) ;
                    }

                    Assert.AreEqual(1,      log.Messages.Count);
                    Assert.AreEqual(text,   log.Messages[0].Text);
                    Assert.AreEqual("NFX.NUnit.ScopeTestFixture.ScopeExitTest[ScopeTest.cs",
                                    log.Messages[0].From.TakeWhile(c => c != ':'));
                    Assert.AreEqual("time", log.Messages[0].Parameters.TakeWhile(c => c != '='));
                    Assert.AreEqual(MessageType.Warning, log.Messages[0].Type);
                }
            }

        }
    }
}
