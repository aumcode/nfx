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
using System.IO;

using NUnit.Framework;

using NFX.Glue.Native;
using NFX.Glue;
using NFX.IO;

namespace NFX.NUnit.Glue
{
    [TestFixture]
    public class NodeTests
    {
        [TestCase]
        public void Node1()
        {
            var n = new Node("http://server:9045");
            Assert.AreEqual("server", n.Host);
            Assert.AreEqual("http", n.Binding);
            Assert.AreEqual("9045", n.Service);
        }

        [TestCase]
        public void Node2()
        {
            var n = new Node("http://server=127.0.0.1;interface=eth0:hgov");
            Assert.AreEqual("server=127.0.0.1;interface=eth0", n.Host);
            Assert.AreEqual("http", n.Binding);
            Assert.AreEqual("hgov", n.Service);
        }


        [TestCase]
        public void Node3()
        {
            var n = new Node("server:1891");
            Assert.AreEqual("server", n.Host);
            Assert.AreEqual(string.Empty, n.Binding);
            Assert.AreEqual("1891", n.Service);
        }

        [TestCase]
        public void Node4()
        {
            var n = new Node("http://server");
            Assert.AreEqual("server", n.Host);
            Assert.AreEqual("http", n.Binding);
            Assert.AreEqual(string.Empty, n.Service);
        }

        [TestCase]
        public void Node5()
        {
            var n = new Node("server");
            Assert.AreEqual("server", n.Host);
            Assert.AreEqual(string.Empty, n.Binding);
            Assert.AreEqual(string.Empty, n.Service);
        }

    }
}
