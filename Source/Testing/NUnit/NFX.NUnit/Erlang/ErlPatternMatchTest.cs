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
using NUnit.Framework;

using NFX.Erlang;
using KVP = System.Collections.Generic.KeyValuePair<
                NFX.Erlang.ErlPatternMatcher.Pattern, NFX.Erlang.ErlVarBind>;

namespace NFX.NUnit.Erlang
{
    [TestFixture]
    class ErlPatternMatchTest
    {
        private static readonly ErlAtom A = new ErlAtom("A");
        private static readonly ErlAtom B = new ErlAtom("B");
        private static readonly ErlAtom C = new ErlAtom("C");
        private static readonly ErlAtom D = new ErlAtom("D");
        private static readonly ErlAtom E = new ErlAtom("E");
        private static readonly ErlAtom F = new ErlAtom("F");
        private static readonly ErlAtom L = new ErlAtom("L");
        private static readonly ErlAtom N = new ErlAtom("N");
        private static readonly ErlAtom T = new ErlAtom("T");

        [Test]
        public void ErlTestPatternMatch()
        {
            {
                var binding = new ErlVarBind();
                IErlObject obj = ErlObject.Parse("{snapshot, x12, []}");
                IErlObject pat = ErlObject.Parse("{snapshot, N, L}");

                Assert.IsTrue(pat.Match(obj, binding));
                ErlAtom n = binding.Cast<ErlAtom>(N);
                ErlList l = binding.Cast<ErlList>(L);
                Assert.IsNotNull(n);
                Assert.IsNotNull(l);
                Assert.IsTrue(l.Count == 0);
            }
            {
                IErlObject pat = ErlObject.Parse("{test, A, B, C}");
                IErlObject obj = ErlObject.Parse("{test, 10, a, [1,2,3]}");

                var binding = new ErlVarBind();
                Assert.IsTrue(pat.Match(obj, binding));
                Assert.AreEqual(3, binding.Count);
                Assert.AreEqual(10, binding.Cast<ErlLong>(A));
                Assert.AreEqual("a", binding.Cast<ErlAtom>(B).ValueAsString);
                Assert.AreEqual("[1,2,3]", binding["C"].ToString());
            }

            {
                var binding = new ErlVarBind();
                IErlObject obj = ErlObject.Parse("[1,a,$b,\"xyz\",{1,10.0},[]]");
                IErlObject pat = ErlObject.Parse("[A,B,C,D,E,F]");

                Assert.IsTrue(pat.Match(obj, binding));
                Assert.IsNotNull(binding.Cast<ErlLong>(A));
                Assert.IsNotNull(binding.Cast<ErlAtom>(B));
                Assert.IsNotNull(binding.Cast<ErlByte>(C));
                Assert.IsNotNull(binding.Cast<ErlString>(D));
                Assert.IsNotNull(binding.Cast<ErlTuple>(E));
                Assert.IsNotNull(binding.Cast<ErlList>(F));

                Assert.IsTrue(binding.Cast<ErlTuple>(E).Count == 2);
                Assert.IsTrue(binding.Cast<ErlList>(F).Count == 0);
            }

            IErlObject pattern = ErlObject.Parse("{test, T}");
            string exp = "{test, ~w}";
            {
                var binding = new ErlVarBind();
                IErlObject obj = ErlObject.Parse(exp, (int)3);
                Assert.IsTrue(pattern.Match(obj, binding));
                Assert.AreEqual(3, binding.Cast<ErlLong>(T));
            }
            {
                var binding = new ErlVarBind();
                IErlObject obj = ErlObject.Parse(exp, (long)100);
                Assert.IsTrue(pattern.Match(obj, binding));
                Assert.AreEqual(100, binding.Cast<ErlLong>(T));
            }
            {
                var binding = new ErlVarBind();
                IErlObject obj = ErlObject.Parse(exp, 100.0);
                Assert.IsTrue(pattern.Match(obj, binding));
                Assert.AreEqual(100.0, binding.Cast<ErlDouble>(T).ValueAsDouble);
            }
            {
                var binding = new ErlVarBind();
                IErlObject obj = ErlObject.Parse(exp, "test");
                Assert.IsTrue(pattern.Match(obj, binding));
                Assert.AreEqual("test", binding.Cast<ErlString>(T).ValueAsString);
            }
            {
                var binding = new ErlVarBind();
                IErlObject obj = ErlObject.Parse(exp, true);
                Assert.IsTrue(pattern.Match(obj, binding));
                Assert.AreEqual(true, binding.Cast<ErlBoolean>(T).ValueAsBool);
            }
            {
                var binding = new ErlVarBind();
                IErlObject obj = ErlObject.Parse(exp, 'c');
                Assert.IsTrue(pattern.Match(obj, binding));
                Assert.AreEqual((byte)'c', binding.Cast<ErlByte>(T).ValueAsInt);
            }
            {
                var binding = new ErlVarBind();
                var pid = new ErlPid("tmp", 1, 2, 3);
                IErlObject obj = ErlObject.Parse(exp, pid as IErlObject);
                Assert.IsTrue(pattern.Match(obj, binding));
                Assert.AreEqual(pid, binding.Cast<ErlPid>(T));
                Assert.AreEqual(pid, binding.Cast<ErlPid>(T).Value);

                obj = ErlObject.Parse(exp, pid);
                Assert.IsTrue(pattern.Match(obj, binding));
                Assert.AreEqual(pid, binding.Cast<ErlPid>(T).Value);
            }
            {
                var binding = new ErlVarBind();
                var port = new ErlPort("tmp", 1, 2);
                IErlObject obj = ErlObject.Parse(exp, port);
                Assert.IsTrue(pattern.Match(obj, binding));
                Assert.AreEqual(port, binding.Cast<ErlPort>(T));
                Assert.AreEqual(port, binding.Cast<ErlPort>(T).Value);
            }
            {
                var binding = new ErlVarBind();
                var reference = new ErlRef("tmp", 1, 0, 0, 2);
                IErlObject obj = ErlObject.Parse(exp, reference);
                Assert.IsTrue(pattern.Match(obj, binding));
                Assert.AreEqual(reference, binding.Cast<ErlRef>(T));
                Assert.AreEqual(reference, binding.Cast<ErlRef>(T).Value);
            }
            {
                var binding = new ErlVarBind();
                ErlList obj = new ErlList(new ErlLong(10), new ErlDouble(30.0),
                    new ErlString("abc"), new ErlAtom("a"),
                    new ErlBinary(new byte[] { 1, 2, 3 }), false, new ErlBoolean(true));
                IErlObject pat = ErlObject.Parse("T");
                Assert.IsTrue(pat.Match(obj, binding));
                IErlObject expected = ErlObject.Parse("[10, 30.0, \"abc\", 'a', ~w, \'false\', true]",
                    new ErlBinary(new byte[] { 1, 2, 3 }));
                IErlObject result = binding[T];
                Assert.IsTrue(expected.Equals(result));
            }
        }

        [Test]
        public void ErlTestFormat()
        {
            {
                IErlObject obj1 = ErlObject.Parse("a");
                Assert.IsInstanceOf(typeof(ErlAtom), obj1);
                Assert.AreEqual("a", obj1.ValueAsString);
            }
            {
                IErlObject obj1 = ErlObject.Parse("$a");
                Assert.IsInstanceOf(typeof(ErlByte), obj1);
                Assert.AreEqual('a', (char)obj1.ValueAsInt);
            }
            {
                IErlObject obj1 = ErlObject.Parse("'Abc'");
                Assert.IsInstanceOf(typeof(ErlAtom), obj1);
                Assert.AreEqual("Abc", obj1.ValueAsString);
            }
            {
                IErlObject obj1 = ErlObject.Parse("{'true', 'false', true, false}");
                Assert.IsInstanceOf(typeof(ErlTuple), obj1);
                var t = (ErlTuple)obj1;
                Assert.AreEqual(4, t.Count);
                t.Select(o => { Assert.IsInstanceOf(typeof(ErlBoolean), o); return 0; });
                Assert.AreEqual(true, t[0].ValueAsBool);
                Assert.AreEqual(false,t[1].ValueAsBool);
                Assert.AreEqual(true, t[2].ValueAsBool);
                Assert.AreEqual(false,t[3].ValueAsBool);
            }
            {
                IErlObject obj1 = ErlObject.Parse("\"Abc\"");
                Assert.IsInstanceOf(typeof(ErlString), obj1);
                Assert.AreEqual("Abc", obj1.ValueAsString);
            }
            {
                IErlObject obj1 = ErlObject.Parse("Abc");
                Assert.IsInstanceOf(typeof(ErlVar), obj1);
                Assert.AreEqual("Abc", ((ErlVar)obj1).Name.Value);

                IErlObject obj2 = ErlObject.Parse("V");
                Assert.IsInstanceOf(typeof(ErlVar), obj2);
                Assert.AreEqual("V", ((ErlVar)obj2).Name.Value);
            }
            {
                IErlObject obj1 = ErlObject.Parse("1");
                Assert.IsInstanceOf(typeof(ErlLong), obj1);
                Assert.AreEqual(1, obj1.ValueAsInt);
            }
            {
                IErlObject obj1 = ErlObject.Parse("1.23");
                Assert.IsInstanceOf(typeof(ErlDouble), obj1);
                Assert.AreEqual(1.23, obj1.ValueAsDouble);
            }
            {
                IErlObject obj1 = ErlObject.Parse("$a");
                Assert.IsInstanceOf(typeof(ErlByte), obj1);
                Assert.AreEqual('a', (char)obj1.ValueAsInt);
            }
            {
                IErlObject obj1 = ErlObject.Parse("{1}");
                Assert.IsInstanceOf(typeof(ErlTuple), obj1);
                Assert.AreEqual(1, ((ErlTuple)obj1).Count);
                Assert.IsInstanceOf(typeof(ErlLong), ((ErlTuple)obj1)[0]);
                Assert.AreEqual(1, (obj1 as ErlTuple)[0].ValueAsInt);
            }
            {
                IErlObject obj0 = ErlObject.Parse("[]");
                Assert.IsInstanceOf(typeof(ErlList), obj0);
                Assert.AreEqual(0, ((ErlList)obj0).Count);
                IErlObject obj1 = ErlObject.Parse("[1]");
                Assert.IsInstanceOf(typeof(ErlList), obj1);
                Assert.AreEqual(1, ((ErlList)obj1).Count);
                Assert.IsInstanceOf(typeof(ErlLong), (obj1 as ErlList)[0]);
                Assert.AreEqual(1, (obj1 as ErlList)[0].ValueAsInt);
            }
            {
                IErlObject obj1 = ErlObject.Parse("[{1,2}, []]");
                Assert.IsInstanceOf(typeof(ErlList), obj1);
                Assert.AreEqual(2, (obj1 as ErlList).Count);
                Assert.IsInstanceOf(typeof(ErlTuple), (obj1 as ErlList)[0]);
                Assert.AreEqual(2, ((obj1 as ErlList)[0] as ErlTuple).Count);
                Assert.AreEqual(0, ((obj1 as ErlList)[1] as ErlList).Count);
            }
            {
                IErlObject obj1 = ErlObject.Parse("{a, [b, 1, 2.0, \"abc\"], {1, 2}}");
                Assert.IsInstanceOf(typeof(ErlTuple), obj1);
                Assert.AreEqual(3, (obj1 as ErlTuple).Count);
            }
            {
                IErlObject obj1 = ErlObject.Parse("~w", 1);
                Assert.IsInstanceOf(typeof(ErlLong), obj1);
                Assert.AreEqual(1, (((ErlLong)obj1)).ValueAsInt);
                IErlObject obj2 = ErlObject.Parse("{~w, ~w,~w}", 1, 2, 3);
                Assert.IsInstanceOf(typeof(ErlTuple), obj2);
                Assert.AreEqual(3, (obj2 as ErlTuple).Count);
                Assert.IsInstanceOf(typeof(ErlLong), (obj2 as ErlTuple)[0]);
                Assert.AreEqual(1, (obj2 as ErlTuple)[0].ValueAsInt);
                Assert.IsInstanceOf(typeof(ErlLong), (obj2 as ErlTuple)[1]);
                Assert.AreEqual(2, (obj2 as ErlTuple)[1].ValueAsInt);
                Assert.IsInstanceOf(typeof(ErlLong), (obj2 as ErlTuple)[2]);
                Assert.AreEqual(3, (obj2 as ErlTuple)[2].ValueAsInt);
            }
            {
                IErlObject obj2 = ErlObject.Parse("{~w, ~w,~w,~w, ~w}", 1.0, 'a', "abc", 2, true);
                Assert.IsInstanceOf(typeof(ErlTuple), obj2);
                Assert.AreEqual(5, (obj2 as ErlTuple).Count);
                Assert.IsInstanceOf(typeof(ErlDouble), (obj2 as ErlTuple)[0]);
                Assert.AreEqual(1.0, (obj2 as ErlTuple)[0].ValueAsDouble);
                Assert.IsInstanceOf(typeof(ErlByte), (obj2 as ErlTuple)[1]);
                Assert.AreEqual((byte)'a', (obj2 as ErlTuple)[1].ValueAsInt);
                Assert.IsInstanceOf(typeof(ErlString), (obj2 as ErlTuple)[2]);
                Assert.AreEqual("abc", (obj2 as ErlTuple)[2].ValueAsString);
                Assert.IsInstanceOf(typeof(ErlLong), (obj2 as ErlTuple)[3]);
                Assert.AreEqual(2, (obj2 as ErlTuple)[3].ValueAsInt);
                Assert.IsInstanceOf(typeof(ErlBoolean), (obj2 as ErlTuple)[4]);
                Assert.AreEqual(true, (obj2 as ErlTuple)[4].ValueAsBool);
            }
        }

        [Test]
        public void ErlTestFormatVariable()
        {
            var cases = new Dictionary<string, ErlTypeOrder> {
                { "B",              ErlTypeOrder.ErlObject },
                { "B::int()",       ErlTypeOrder.ErlLong },
                { "B::integer()",   ErlTypeOrder.ErlLong },
                { "B::string()",    ErlTypeOrder.ErlString },
                { "B::atom()",      ErlTypeOrder.ErlAtom },
                { "B::float()",     ErlTypeOrder.ErlDouble },
                { "B::double()",    ErlTypeOrder.ErlDouble },
                { "B::binary()",    ErlTypeOrder.ErlBinary },
                { "B::bool()",      ErlTypeOrder.ErlBoolean },
                { "B::boolean()",   ErlTypeOrder.ErlBoolean },
                { "B::byte()",      ErlTypeOrder.ErlByte },
                { "B::char()",      ErlTypeOrder.ErlByte },
                { "B::list()",      ErlTypeOrder.ErlList },
                { "B::tuple()",     ErlTypeOrder.ErlTuple },
                { "B::pid()",       ErlTypeOrder.ErlPid },
                { "B::ref()",       ErlTypeOrder.ErlRef },
                { "B::reference()", ErlTypeOrder.ErlRef },
                { "B::port()",      ErlTypeOrder.ErlPort }
            };

            foreach (var p in cases)
            {
                IErlObject o = ErlObject.Parse(p.Key);
                Assert.IsInstanceOf(typeof(ErlVar), o);
                Assert.AreEqual(p.Value, ((ErlVar)o).ValueType);
            }

            var pat1 = ErlObject.Parse("{A::char(), B::tuple(), C::float(), D::list(), [E::string(), F::int()], G::bool()}");
            var obj1 = ErlObject.Parse("{$a, {1,2,3}, 10.0, [5,6], [\"abc\", 190], true}");

            var binding = new ErlVarBind();

            Assert.IsTrue(pat1.Match(obj1, binding)); // Match unbound variables
            Assert.IsTrue(pat1.Match(obj1, binding)); // Match bound variables

            var obj2 = ErlObject.Parse("{$a, {1,2,3}, 20.0, [5,6], [\"abc\", 190], true}");

            Assert.IsFalse(pat1.Match(obj2, binding)); // Match bound variables

            binding.Clear();

            var obj3 = ErlObject.Parse("{$a, {1,2,3}, 10.0, [5,6], [\"abc\", bad], false}");

            Assert.IsFalse(pat1.Match(obj3, binding));
        }

        public class KeyValueList<TKey, TValue> : List<KeyValuePair<TKey, TValue>>
        {
            public void Add(TKey key, TValue value)
            {
                Add(new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        [Test]
        public void ErlTestMatchVariable()
        {
            var cases = new KeyValueList<string, IErlObject> {
                { "B",              new ErlLong(1) },
                { "B",              new ErlAtom("abc") },
                { "B",              new ErlString("efg") },
                { "B",              new ErlDouble(10.0) },
                { "B::int()",       new ErlLong(10) },
                { "B::integer()",   new ErlLong(20) },
                { "B::string()",    new ErlString("xxx") },
                { "B::atom()",      new ErlAtom("xyz") },
                { "B::float()",     new ErlDouble(5.0) },
                { "B::double()",    new ErlDouble(3.0) },
                { "B::binary()",    new ErlBinary(new byte[] {1,2,3}) },
                { "B::bool()",      new ErlBoolean(true) },
                { "B::boolean()",   new ErlBoolean(false) },
                { "B::byte()",      new ErlByte(1) },
                { "B::char()",      new ErlByte('a') },
                { "B::list()",      new ErlList(1, 2, 3) },
                { "B::tuple()",     new ErlTuple(new ErlByte('a'), 1, "aaa") },
                { "B::pid()",       new ErlPid("xxx", 1, 2, 3) },
                { "B::ref()",       new ErlRef("xxx", 1, 0, 0, 3) },
                { "B::reference()", new ErlRef("xxx", 1, 0, 0, 3) },
                { "B::port()",      new ErlPort("xxx", 1, 3) }
            };

            foreach (var p in cases)
            {
                {
                    IErlObject pat = p.Key.ToErlObject();
                    IErlObject obj = p.Value;

                    var binding = new ErlVarBind();
                    binding[B] = obj;

                    Assert.IsTrue(pat.Match(obj, binding));
                }

                {
                    IErlObject pat = p.Key.ToErlObject();
                    IErlObject obj = p.Value;

                    var binding = new ErlVarBind();

                    Assert.IsTrue(pat.Match(obj, binding));

                    var b = binding["B"];

                    Assert.AreEqual(obj.TypeOrder, b.TypeOrder);
                    Assert.IsTrue(obj.Equals(b));
                }
            }

            var revCases = cases.Reverse<KeyValuePair<string,IErlObject>>().ToList();

            cases.Zip(revCases,
                (p1, p2) => {
                    ErlVar     pat = ErlObject.Parse<ErlVar>(p1.Key);
                    IErlObject obj = p2.Value;

                    var binding = new ErlVarBind();

                    if (pat.ValueType == ErlTypeOrder.ErlObject || pat.ValueType == obj.TypeOrder)
                        Assert.IsTrue(pat.Match(obj, binding));
                    else
                        Assert.IsFalse(pat.Match(obj, binding));

                    return false;
                }).ToList();
        }

        [Test]
        public void ErlPatternMatchCollectionTest()
        {
            var state = new KVP();

            var pm = new ErlPatternMatcher
            {
                { 0, "{A::integer(), stop}"               , (_ctx, p, t, b, _args) => { state = new KVP(p, b); return t; } },
                {    "{A::integer(), status}"             ,       (p, t, b, _args) => { state = new KVP(p, b); return t; } },
                { 1, "{A::integer(), {status, B::atom()}}", (_ctx, p, t, b, _args) => { state = new KVP(p, b); return t; } },
                {    "{A::integer(), {config, B::list()}}",       (p, t, b, _args) => { state = new KVP(p, b); return t; } }
            };

            var term = ErlObject.Parse("{10, stop}");
            Assert.AreEqual(1,  pm.Match(ref term));
            Assert.AreEqual(10, state.Value["A"].ValueAsInt);

            term = ErlObject.Parse("{11, status}");
            Assert.AreEqual(2,  pm.Match(ref term));
            Assert.AreEqual(11, state.Value["A"].ValueAsInt);

            term = ErlObject.Parse("{12, {status, ~w}}", new ErlAtom("a"));
            Assert.AreEqual(3,  pm.Match(ref term));
            Assert.AreEqual(12, state.Value["A"].ValueAsInt);
            Assert.AreEqual("a",state.Value["B"].ValueAsString);

            term = ErlObject.Parse("{13, {config, ~w}}", new ErlList());
            Assert.AreEqual(4,  pm.Match(ref term));
            Assert.AreEqual(13, state.Value["A"].ValueAsInt);
            Assert.AreEqual(0,  (state.Value["B"] as ErlList).Count);
                                
            term = ErlObject.Parse("{10, exit}");
            Assert.AreEqual(-1, pm.Match(ref term));

            var pts = pm.PatternsToString;

            Assert.AreEqual(
                "[{A::int(),stop},{A::int(),status},{A::int(),{status,B::atom()}},{A::int(),{config,B::list()}}]",
                pts);
        }
    }
}
