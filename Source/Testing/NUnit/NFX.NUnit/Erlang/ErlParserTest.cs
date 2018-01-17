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
using NUnit.Framework;
using NFX.Erlang;

namespace NFX.NUnit.Erlang
{
  [TestFixture]
  public class ErlParserFixture
  {
    [Test]
    public void ErlParserTest()
    {
      var tests = new Dictionary<string, IErlObject>
      {
        {"<<1,2,3>>",         new ErlBinary(new byte[] {1,2,3})},
        {"<<>>",              new ErlBinary(new byte[] {})},
        {"<<\"abc\">>",       new ErlBinary(new byte[] {(byte)'a', (byte)'b', (byte)'c' })},
        {"<<\"\">>",          new ErlBinary(new byte[] {})},
      };

      foreach (var t in tests)
      {
        Assert.DoesNotThrow(() => ErlObject.Parse(t.Key, t.Value),
                            "Error parsing: {0} (test: {1})".Args(t.Key, t.ToString()));

        var res = ErlObject.Parse(t.Key, t.Value);
        Assert.AreEqual(t.Value, res,
                        "Unexpected value: {0} (expected: {1})".Args(res, t.Value));
      }
    }

    [Test]
    public void ErlParserMFATest()
    {
      var tests = new Dictionary<string, Tuple<string, string, ErlList, object[]>>
      {
        {"a:b()",           Tuple.Create("a", "b", new ErlList(),   (object[])null)},
        {"a:b().",          Tuple.Create("a", "b", new ErlList(),   (object[])null)},
        {"a:b()\t      .",  Tuple.Create("a", "b", new ErlList(),   (object[])null)},
        {"a:b()       ",    Tuple.Create("a", "b", new ErlList(),   (object[])null)},
        {"a:b()\n.",        Tuple.Create("a", "b", new ErlList(),   (object[])null)},
        {"a:b(%comment\n).",Tuple.Create("a", "b", new ErlList(),   (object[])null)},
        {"a:b().%comment",  Tuple.Create("a", "b", new ErlList(),   (object[])null)},
        {"a:b()\t",         Tuple.Create("a", "b", new ErlList(),   (object[])null)},
        {"a:b(10)",         Tuple.Create("a", "b", new ErlList(10), (object[])null)},
        {"a:b(10).",        Tuple.Create("a", "b", new ErlList(10), (object[])null)},
        {"aa:bb(10)",       Tuple.Create("aa","bb",new ErlList(10), (object[])null)},
        {"a:b(10,20)",      Tuple.Create("a", "b", new ErlList(10,20), (object[])null)},
        {"a:b(~w)",         Tuple.Create("a", "b", new ErlList(10), new object[] {10})},
        {"a:b(~w).",        Tuple.Create("a", "b", new ErlList(10), new object[] {10})},
        {
          "a:b(~f,~d).",
          Tuple.Create("a", "b", new ErlList(10d, 20), new object[] {10d, 20})
        },
        {
          "a:b([~w,~w],30)",
          Tuple.Create("a", "b", new ErlList(new ErlList(10, 20), 30),
                       new object[] {10, 20})
        },
        {
          "a:b([~w,~w],~w)",
          Tuple.Create("a", "b", new ErlList(new ErlList(10, 20), 30),
                       new object[] {10, 20, 30})
        },
      };

      foreach (var t in tests)
      {
        Assert.DoesNotThrow(() => ErlObject.ParseMFA(t.Key, t.Value.Item4),
                            "Error parsing: {0} (test: {1})".Args(t.Key, t.ToString()));

        var res = ErlObject.ParseMFA(t.Key, t.Value.Item4);
        Assert.AreEqual(t.Value.Item1, res.Item1.Value,
                        "Unexpected module value: {0} (expected: {1})".Args(
                          res.Item1.Value, t.Value.Item1));
        Assert.AreEqual(t.Value.Item2, res.Item2.Value,
                        "Unexpected function value: {0} (expected: {1})".Args(
                          res.Item2.Value, t.Value.Item2));
        Assert.AreEqual(t.Value.Item3, res.Item3,
                        "Unexpected args value: {0} (expected: {1})".Args(res.Item3,
                                                                          t.Value.Item3));
      }
    }

    [Test]
    public void ErlParserMFAFailTest()
    {
      var tests = new Dictionary<string, object[]>
      {
        {"a:b(1,%comment\n",   (object[])null},
        {"a:b(1,%comment 2).", (object[])null},
        {"(",                  (object[])null},
        {")",                  (object[])null},
        {".",                  (object[])null},
        {"aa",                 (object[])null},
        {"a(",                 (object[])null},
        {"a:b(",               (object[])null},
        {"a.b()",              (object[])null},
        {"a:b(10         20)", (object[])null},
        {"a:b(10.        20)", (object[])null},
        {"a:b(10.(20)",        (object[])null},
        {"a:b(~w,~w)",         new object[] {10}},
        {"a:b([~w,20],~w)",    new object[] {10}},
      };

      foreach (var t in tests)
        Assert.Throws<ErlException>(
          () => ErlObject.ParseMFA(t.Key, t.Value),
          "Errorneously parsing term: {0}".Args(t.Key));
    }

    [Test]
    public void ErlFormatTest()
    {
      var tests = new Dictionary<string, ErlList>
      {
        {"abc 10.5",  new ErlList("abc ~w.~w", 10, 5)},
        {"xx 8",      new ErlList("xx ~i~w", 12, 8)},
        {"~z",        new ErlList("~~z", 16)},
        {"a 16",      new ErlList("~c ~w", (byte)'a', 16)},
        {"xyz 12\n",  new ErlList("xyz ~10.6.B~n", 12)},
        {"x~y21",     new ErlList("x~~y~w1", 2)},
        {"{ok, A}",   new ErlList("{ok, ~v}", "A")},
        {"{ok, A}.",  new ErlList("{ok, ~v}.", new ErlAtom("A"))},
        {"{ok, A} ",  new ErlList("{ok, ~v} ", new ErlString("A"))},
        {"{ok, A}  ", new ErlList("{ok, ~v}  ", new ErlVar("A"))},
        {
          "{ok, A::a()}",
          new ErlList("{ok, ~v::a()}", new ErlVar("A", ErlTypeOrder.ErlLong))
        },
        {"{ok, A::int()}", new ErlList("{ok, ~v}", new ErlVar("A", ErlTypeOrder.ErlLong))},
      };

      foreach (var t in tests)
        Assert.AreEqual(t.Key, ErlObject.Format(t.Value),
                        "Error in test: {0} <- format({1})".Args(t.Key, t.Value.ToString()));

      var failTests = new List<ErlList>
      {
        new ErlList("abc ~w.~w"),
        new ErlList("xx ~i~w", 12),
        new ErlList("~y",      12),
      };

      foreach (var t in failTests)
        Assert.Throws<ErlException>(
          () => ErlObject.Format(t), "Errorneously formatted term: {0}".Args(t));

      var V = new ErlVar("V", ErlTypeOrder.ErlLong);
      var expected = new ErlTuple(new ErlAtom("ok"), V).ToString();

      Assert.AreEqual(expected, "{ok, V::int()}".ToErlObject().ToString());
      Assert.AreEqual(expected, "{ok, ~w}".ToErlObject(V).ToString());
    }
  }
}
