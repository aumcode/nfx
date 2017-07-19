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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;


using NFX.Environment;
using NFX.ApplicationModel;



namespace NFX.NUnit.AppModel
{
    [TestFixture]
    public class StarterTests
    {

        public static string RESULT;


        [TestCase]
        public void Starter1()
        {
 var confSource=@"
 nfx{
    starters{
        starter{ name='Alex'  type='NFX.NUnit.AppModel.MyStarter, NFX.NUnit'}
        starter{ name='Boris' type='NFX.NUnit.AppModel.MyStarter, NFX.NUnit'}
    }

 }
 ";
            RESULT = "";
            var conf = LaconicConfiguration.CreateFromString(confSource);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {

            }

            Assert.AreEqual("Alex Before;Boris Before;Alex After;Boris After;", RESULT);

        }

        [TestCase]
        public void Starter2_WithException_NoBreak()
        {
 var confSource=@"
 nfx{
    starters{
        starter{ name='Alex'  type='NFX.NUnit.AppModel.MyStarter, NFX.NUnit'}
        starter{ name='Boris' type='NFX.NUnit.AppModel.MyStarter, NFX.NUnit'}
    }

 }
 ";
            RESULT = null;//NULL REFERENCE should happen
            var conf = LaconicConfiguration.CreateFromString(confSource);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {

            }

            Assert.IsNull( RESULT); //no exception

        }

        [TestCase]
        [ExpectedException(typeof(NFXException),
                           ExpectedMessage="Error calling Starter.ApplicationStartBeforeInit() 'Alex'. Exception: [System.NullReferenceException]",
                           MatchType=MessageMatch.Contains)]
        public void Starter2_WithException_Break()
        {
 var confSource=@"
 nfx{
    starters{
        starter{ name='Alex'  type='NFX.NUnit.AppModel.MyStarter, NFX.NUnit' application-start-break-on-exception='true'}
        starter{ name='Boris' type='NFX.NUnit.AppModel.MyStarter, NFX.NUnit'}
    }

 }
 ";
            RESULT = null;//NULL REFERENCE should happen
            var conf = LaconicConfiguration.CreateFromString(confSource);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {

            }

        }


    }


            public class MyStarter: IApplicationStarter
            {

                [Config]
                public bool ApplicationStartBreakOnException
                {
                    get;
                    set;
                }

                public void ApplicationStartBeforeInit(IApplication application)
                {
                   StarterTests.RESULT.Trim();//causes null reference
                   StarterTests.RESULT += "{0} Before;".Args(Name);
                }

                public void ApplicationStartAfterInit(IApplication application)
                {
                   StarterTests.RESULT.Trim();//causes null reference
                   StarterTests.RESULT += "{0} After;".Args(Name);
                }

                public void Configure(IConfigSectionNode node)
                {
                    ConfigAttribute.Apply(this, node);
                }

                [Config]
                public string Name
                {
                    get;
                    set;
                }
            }


}