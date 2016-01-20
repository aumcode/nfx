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
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;

using NFX.OS;
using NFX.Serialization.JSON;

namespace NFX.NUnit.OS
{

    [TestFixture]
    public class NetUtilsTest
    {
        [TestCase]
        public void GetUniqueNetworkSignature()
        {
            var sig = NetworkUtils.GetMachineUniqueMACSignature();

            Console.WriteLine( sig );

            if (System.Environment.MachineName=="SEXTOD")
             Assert.AreEqual("9-08CC2E06-DD8F620D34473E5E", sig);
            else
             Assert.Inconclusive("This test can only run on SEXTOD");
        }

        [TestCase]
        public void Computer_UniqueNetworkSignature()
        {
            var sig = Computer.UniqueNetworkSignature;

            Console.WriteLine( sig );

            if (System.Environment.MachineName=="SEXTOD")
             Assert.AreEqual("9-08CC2E06-DD8F620D34473E5E", sig);
            else
             Assert.Inconclusive("This test can only run on SEXTOD");
        }

        [TestCase]
        public void HostNetInfo_1()
        {
            var hi = HostNetInfo.ForThisHost();

            Console.WriteLine(  hi.ToJSON(JSONWritingOptions.PrettyPrint) );
           
            if (System.Environment.MachineName=="SEXTOD")
             Assert.IsTrue( hi.Adapters["{C93B4009-15C0-46A3-8C95-91610CAEBC4F}::Local Area Connection"].Addresses.ContainsName("192.168.1.70") );
            else
             Assert.Inconclusive("This test can only run on SEXTOD");
            
        }

        

    }
}
