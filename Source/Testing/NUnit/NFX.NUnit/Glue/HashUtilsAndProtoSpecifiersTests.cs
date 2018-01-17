/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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

using NFX.Glue.Protocol;

namespace NFX.NUnit.Glue
{
    [TestFixture]
    public class HashUtilsAndProtoSpecifiersTests
    {
        [TestCase]
        public void HashID_1()
        {
            var h1 = HashUtils.StringIDHash("Aum.Glue");
            var h2 = HashUtils.StringIDHash("Aum.Flue");

            Console.WriteLine(h1);
            Console.WriteLine(h2);

            Assert.AreNotEqual(h1, h2);
        }

        [TestCase]
        public void HashID_2()
        {
            var h1 = HashUtils.StringIDHash("A");
            var h2 = HashUtils.StringIDHash("a");

            Console.WriteLine(h1);
            Console.WriteLine(h2);

            Assert.AreNotEqual(h1, h2);
        }

        [TestCase]
        public void HashID_3()
        {
            var h1 = HashUtils.StringIDHash("Aum.Cluster.Glue");
            var h2 = HashUtils.StringIDHash("Aum.Cluster.App.Contracts");

            Console.WriteLine(h1);
            Console.WriteLine(h2);

            Assert.AreNotEqual(h1, h2);
        }

        [TestCase]
        public void HashID_4()
        {
            var h1 = HashUtils.StringIDHash("Aum.Cluster.Glue");
            var h2 = HashUtils.StringIDHash("Aum.Cluster.Glue");

            Console.WriteLine(h1);
            Console.WriteLine(h2);

            Assert.AreEqual(h1, h2);
        }


        [TestCase]
        public void TypeHash_1()
        {
            var h1 = HashUtils.TypeHash(typeof(NFX.INamed));
            var h2 = HashUtils.TypeHash(typeof(NFX.INamed));

            Console.WriteLine(h1);
            Console.WriteLine(h2);

            Assert.AreEqual(h1, h2);
        }

        [TestCase]
        public void TypeHash_2()
        {
            var h1 = HashUtils.TypeHash(typeof(NFX.INamed));
            var h2 = HashUtils.TypeHash(typeof(NFX.IOrdered));

            Console.WriteLine(h1);
            Console.WriteLine(h2);

            Assert.AreNotEqual(h1, h2);
        }

        [TestCase]
        public void TypeHash_3()
        {
            var h1 = HashUtils.TypeHash(typeof(NFX.INamed));
            var h2 = HashUtils.TypeHash(typeof(NFX.Wave.WaveServer));

            Console.WriteLine(h1);
            Console.WriteLine(h2);

            Assert.AreNotEqual(h1, h2);
        }


        [TestCase]
        public void TypeSpec_1()
        {
            var s1 = new TypeSpec(typeof(NFX.INamed));
            var s2 = new TypeSpec(typeof(NFX.INamed));

            Console.WriteLine(s1);
            Console.WriteLine(s2);

            Assert.AreEqual(s1, s2);
            Assert.AreEqual( s1.GetHashCode(), s2.GetHashCode());
        }

        [TestCase]
        public void TypeSpec_2()
        {
            var s1 = new TypeSpec(typeof(NFX.INamed));
            var s2 = new TypeSpec(typeof(NFX.IOrdered));

            Console.WriteLine(s1);
            Console.WriteLine(s2);

            Assert.AreNotEqual(s1, s2);
            Assert.AreNotEqual( s1.GetHashCode(), s2.GetHashCode());
        }


        [TestCase]
        public void TypeSpec_3()
        {
            var s1 = new TypeSpec(typeof(NFX.Registry<NFX.ServiceModel.Service>));
            var s2 = new TypeSpec(typeof(NFX.Registry<NFX.Glue.Binding>));

            Console.WriteLine(s1);
            Console.WriteLine(s2);

            Assert.AreNotEqual(s1, s2);
            Assert.AreNotEqual( s1.GetHashCode(), s2.GetHashCode());
        }

        [TestCase]
        public void MethodSpec_1()
        {
            var s1 = new MethodSpec(typeof(NFX.Registry<NFX.ServiceModel.Service>).GetMethod("Register"));
            var s2 = new MethodSpec(typeof(NFX.Registry<NFX.ServiceModel.Service>).GetMethod("Register"));

            Console.WriteLine(s1);
            Console.WriteLine(s2);

            Assert.AreEqual(s1, s2);
            Assert.AreEqual( s1.GetHashCode(), s2.GetHashCode());
        }

        [TestCase]
        public void MethodSpec_2()
        {
            var s1 = new MethodSpec(typeof(NFX.Registry<NFX.ServiceModel.Service>).GetMethod("Register"));
            var s2 = new MethodSpec(typeof(NFX.Registry<NFX.ServiceModel.Service>).GetMethod("Unregister", new Type[]{typeof(string)}));

            Console.WriteLine(s1);
            Console.WriteLine(s2);

            Assert.AreNotEqual(s1, s2);
            Assert.AreNotEqual( s1.GetHashCode(), s2.GetHashCode());
        }

        [TestCase]
        public void MethodSpec_3()
        {
            var s1 = new MethodSpec(typeof(NFX.Registry<NFX.ServiceModel.Service>).GetMethod("Register"));
            var s2 = new MethodSpec(typeof(NFX.MiscUtils).GetMethod("ToSecondsSinceUnixEpochStart"));

            Console.WriteLine(s1);
            Console.WriteLine(s2);

            Assert.AreNotEqual(s1, s2);
            Assert.AreNotEqual( s1.GetHashCode(), s2.GetHashCode());
        }

    }
}
