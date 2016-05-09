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
using NUnit.Framework;

using NFX.Erlang;

namespace NFX.NUnit.Erlang
{
  [TestFixture]
  public class ErlTermFixture
  {
    private static readonly ErlAtom A = new ErlAtom("A");
    private static readonly ErlAtom B = new ErlAtom("B");
    private static readonly ErlAtom M = new ErlAtom("M");
    private static readonly ErlAtom N = new ErlAtom("N");
    private static readonly ErlAtom X = new ErlAtom("X");

    [Test]
    public void AtomTableTest()
    {
      Assert.AreEqual(0, AtomTable.Instance[string.Empty]);

      Assert.AreEqual(1, AtomTable.Instance["true"]);

      Assert.AreEqual(2, AtomTable.Instance["false"]);

      Assert.AreEqual(1, ErlAtom.True.Index);
      Assert.AreEqual(2, ErlAtom.False.Index);

      bool found = AtomTable.Instance.IndexOf("ok") != -1;
      int count = AtomTable.Instance.Count;

      var am_ok = new ErlAtom("ok");

      Assert.AreEqual(found ? am_ok.Index : AtomTable.Instance.Count - 1, am_ok.Index);
      Assert.AreEqual(found ? count : count + 1, AtomTable.Instance.Count);
    }

    [Test]
    public void ErlAtomTest()
    {
      var am_test = new ErlAtom("test");
      Assert.IsTrue(am_test.Equals(new ErlAtom("test")));
      Assert.AreEqual(am_test, new ErlAtom("test"));
      Assert.AreEqual("test", am_test.Value);
      Assert.AreEqual("test", am_test.ToString());
      Assert.IsTrue(am_test.IsScalar);
      Assert.AreEqual(ErlTypeOrder.ErlAtom, am_test.TypeOrder);

      Assert.IsTrue(am_test.Matches(new ErlAtom("test")));
      Assert.AreEqual(new ErlVarBind(), am_test.Match(new ErlAtom("test")));

      var am_Test = new ErlAtom("Test");
      Assert.AreEqual("'Test'", am_Test.ToString());
      Assert.AreEqual(4, am_Test.Length);
      Assert.AreNotEqual(am_test, am_Test);

      IErlObject temp = null;
      Assert.IsFalse(am_test.Subst(ref temp, new ErlVarBind()));

      Assert.IsTrue(am_Test.Visit(true, (acc, o) => acc));

      Assert.DoesNotThrow(() => { var x = am_test.ValueAsObject; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = am_test.ValueAsInt; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = am_test.ValueAsLong; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = am_test.ValueAsDecimal; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = am_test.ValueAsDateTime; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = am_test.ValueAsTimeSpan; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = am_test.ValueAsDouble; });
      Assert.DoesNotThrow(() => { var x = am_test.ValueAsString; });
      Assert.DoesNotThrow(() => { var x = am_test.ValueAsBool; });
      Assert.AreEqual('a', new ErlAtom("a").ValueAsChar);
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = am_test.ValueAsByteArray; });

      string s = am_test;  // Implicit conversion
      Assert.AreEqual("test", s);

      ErlAtom a = "abc";   // Implicit conversion
      Assert.AreEqual("abc", a.Value);
    }

    [Test]
    public void ErlBinaryTest()
    {
      {
        var tt = new byte[] { 10, 11, 12 };
        var t1 = new ErlBinary(tt, false);
        tt[0] = 20;
        Assert.AreEqual(20, t1.Value[0]);

        var bb = new byte[] { 10, 11, 12 };
        var t2 = new ErlBinary(bb);
        bb[0] = 20;
        Assert.AreEqual(10, t2.Value[0]);
      }

      var tb = new byte[] { 1, 2, 3 };
      var t = new ErlBinary(tb);

      Assert.IsTrue(t.Equals(new ErlBinary(new byte[] { 1, 2, 3 })));
      Assert.AreEqual(t, new ErlBinary(tb));
      Assert.IsTrue(new ErlBinary(new byte[] { 1, 2 }).CompareTo(t) < 0);
      Assert.AreEqual(tb, t.Value);
      Assert.IsTrue(t.ValueAsBool);
      Assert.IsFalse(new ErlBinary(new byte[] { }).ValueAsBool);
      Assert.AreEqual("<<1,2,3>>", t.ToString());
      Assert.AreEqual("<<1,2,3>>", t.ToBinaryString());
      Assert.IsFalse(t.IsScalar);
      Assert.AreEqual(ErlTypeOrder.ErlBinary, t.TypeOrder);

      var bbb = new ErlBinary(new byte[] { 97, 98, 99, 10, 49, 50, 51 });
      Assert.AreEqual("<<\"abc\n123\">>", bbb.ToString());
      Assert.AreEqual("<<\"abc\n123\">>", bbb.ToPrintableString());
      Assert.AreEqual("<<\"abc...\">>", bbb.ToPrintableString(6));
      Assert.AreEqual("<<97,98,99,10,49,50,51>>", bbb.ToBinaryString());
      Assert.AreEqual("<<97,98...>>", bbb.ToBinaryString(10));

      Assert.IsTrue(t.Matches(new ErlBinary(new byte[] { 1, 2, 3 })));
      Assert.AreEqual(new ErlVarBind(), t.Match(new ErlBinary(new byte[] { 1, 2, 3 })));

      Assert.DoesNotThrow(() => { var x = t.ValueAsObject; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsInt; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsLong; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDecimal; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDateTime; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsTimeSpan; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDouble; });
      Assert.DoesNotThrow(() => { var x = t.ValueAsString; });
      Assert.DoesNotThrow(() => { var x = t.ValueAsBool; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsChar; });
      Assert.DoesNotThrow(() => { var x = t.ValueAsByteArray; });

      IErlObject temp = null;
      Assert.IsFalse(t.Subst(ref temp, new ErlVarBind()));
      Assert.AreEqual(3, t.Visit(0, (acc, o) => acc + ((ErlBinary)o).Length));

      byte[] b = t; Assert.AreEqual(tb, b);
    }

    [Test]
    public void ErlBooleanTest()
    {
      var t = new ErlBoolean(true);
      Assert.IsTrue(t.Equals(new ErlBoolean(true)));
      Assert.IsFalse(t.Equals(new ErlBoolean(false)));
      Assert.AreEqual(t, new ErlBoolean(true));
      Assert.AreEqual(-1, new ErlBoolean(false).CompareTo(t));
      Assert.AreEqual(true, t.Value);
      Assert.AreEqual(1, t.ValueAsInt);
      Assert.AreEqual(1, t.ValueAsLong);
      Assert.AreEqual("true", t.ToString());
      Assert.AreEqual("false", new ErlBoolean(false).ToString());
      Assert.IsTrue(t.IsScalar);
      Assert.AreEqual(ErlTypeOrder.ErlBoolean, t.TypeOrder);

      Assert.IsTrue(t.Matches(new ErlBoolean(true)));
      Assert.AreEqual(new ErlVarBind(), t.Match(new ErlBoolean(true)));

      Assert.DoesNotThrow(() => { var x = t.ValueAsObject; });
      Assert.AreEqual(1, t.ValueAsInt);
      Assert.AreEqual(1, t.ValueAsLong);
      Assert.AreEqual(1, t.ValueAsDecimal);
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDateTime; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsTimeSpan; });
      Assert.AreEqual(1.0, t.ValueAsDouble);
      Assert.AreEqual("True", t.ValueAsString);
      Assert.AreEqual(true, t.ValueAsBool);
      Assert.AreEqual('T', t.ValueAsChar);
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsByteArray; });

      IErlObject temp = null;
      Assert.IsFalse(t.Subst(ref temp, new ErlVarBind()));
      Assert.IsTrue(t.Visit(false, (acc, o) => o.ValueAsBool));

      bool n = t;             // Implicit conversion
      Assert.AreEqual(true, n);
      ErlBoolean a = true;    // Implicit conversion
      Assert.AreEqual(true, a.Value);
    }

    [Test]
    public void ErlByteTest()
    {
      var t = new ErlByte(10);
      Assert.IsTrue(t.Equals(new ErlByte(10)));
      Assert.AreEqual(t, new ErlByte(10));
      Assert.IsTrue(new ErlByte(1).CompareTo(t) < 0);
      Assert.AreEqual(10, t.Value);
      Assert.AreEqual(10, t.ValueAsInt);
      Assert.AreEqual(10, t.ValueAsLong);
      Assert.AreEqual("10", t.ToString());
      Assert.IsTrue(t.IsScalar);
      Assert.AreEqual(ErlTypeOrder.ErlByte, t.TypeOrder);

      Assert.IsTrue(t.Matches(new ErlByte(10)));
      Assert.AreEqual(new ErlVarBind(), t.Match(new ErlByte(10)));

      Assert.DoesNotThrow(() => { var x = t.ValueAsObject; });
      Assert.AreEqual(10, t.ValueAsInt);
      Assert.AreEqual(10, t.ValueAsLong);
      Assert.AreEqual(10, t.ValueAsDecimal);
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDateTime; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsTimeSpan; });
      Assert.AreEqual(10.0, t.ValueAsDouble);
      Assert.AreEqual("10", t.ValueAsString);
      Assert.AreEqual(true, t.ValueAsBool);
      Assert.AreEqual('\n', t.ValueAsChar);
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsByteArray; });

      IErlObject temp = null;
      Assert.IsFalse(t.Subst(ref temp, new ErlVarBind()));
      Assert.AreEqual(10, t.Visit(0, (acc, o) => o.ValueAsInt));

      char n = (char)t; Assert.AreEqual('\n', n);
      byte m = t; Assert.AreEqual(10, m);
      ErlByte b = 10; Assert.AreEqual(10, b.Value);
      ErlByte k = (ErlByte)10; Assert.AreEqual(10, k.Value);
      ErlByte z = (ErlByte)'\n'; Assert.AreEqual(10, k.Value);

      {
        var bind = new ErlVarBind();
        Assert.IsTrue(b.Match(new ErlLong(10), bind));
        Assert.IsTrue(new ErlLong(10).Match(b, bind));
        b = 111;
        Assert.IsTrue(b.Match(new ErlLong(111), bind));
        Assert.IsTrue(new ErlLong(111).Match(b, bind));
      }
    }

    [Test]
    public void ErlDoubleTest()
    {
      var t = new ErlDouble(10.128d);
      Assert.IsTrue(t.Equals(new ErlDouble(10.128d)));
      Assert.AreEqual(1, t.CompareTo(new ErlDouble(-1.1)));
      Assert.AreEqual(t, new ErlDouble(10.128d));
      Assert.AreEqual(0, t.CompareTo(new ErlDouble(10.128d)));
      Assert.IsTrue(10.128d == t);
      Assert.IsTrue(t == 10.128d);
      Assert.AreEqual(10.128d, t.Value);
      Assert.AreEqual(10, t.ValueAsInt);
      Assert.AreEqual(10, t.ValueAsLong);
      Assert.AreEqual(10.128d, t.ValueAsDouble);
      Assert.AreEqual("10.128", t.ToString());
      Assert.AreEqual("1.1", new ErlDouble(1.1).ToString());
      Assert.IsTrue(t.IsScalar);
      Assert.AreEqual(ErlTypeOrder.ErlDouble, t.TypeOrder);

      Assert.IsTrue(t.Matches(new ErlDouble(10.128d)));
      Assert.AreEqual(new ErlVarBind(), t.Match(new ErlDouble(10.128d)));

      Assert.DoesNotThrow(() => { var x = t.ValueAsObject; });
      Assert.AreEqual(10, t.ValueAsInt);
      Assert.AreEqual(10, t.ValueAsLong);
      Assert.AreEqual(10.128d, t.ValueAsDecimal);
      Assert.DoesNotThrow(() => { var x = t.ValueAsDateTime; });
      Assert.DoesNotThrow(() => { var x = t.ValueAsTimeSpan; });
      Assert.AreEqual(10.128d, t.ValueAsDouble);
      Assert.AreEqual("10.128", t.ValueAsString);
      Assert.AreEqual(true, t.ValueAsBool);
      Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 0) + new TimeSpan(10 * 10), t.ValueAsDateTime);
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsChar; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsByteArray; });

      IErlObject temp = null;
      Assert.IsFalse(t.Subst(ref temp, new ErlVarBind()));
      Assert.AreEqual(10.128d, t.Visit(0.0, (acc, o) => o.ValueAsDouble));

      double n = t;             // Implicit conversion
      Assert.AreEqual(10.128d, n);
      ErlDouble a = 10.128d;    // Implicit conversion
      Assert.AreEqual(10.128d, a.Value);
    }

    [Test]
    public void ErlListTest()
    {
      var l = new ErlList("test", 1, 1.1, true, (byte)255, 'x', new ErlAtom("a"));
      var r = new ErlList("test", 1, 1.1, true, (byte)255, 'x', new ErlAtom("a"));

      Assert.AreEqual(7, l.Count);
      Assert.AreEqual(ErlTypeOrder.ErlString, l[0].TypeOrder);
      Assert.AreEqual("test", l[0].ValueAsString);
      Assert.AreEqual(ErlTypeOrder.ErlLong, l[1].TypeOrder);
      Assert.AreEqual(1, l[1].ValueAsInt);
      Assert.AreEqual(ErlTypeOrder.ErlDouble, l[2].TypeOrder);
      Assert.AreEqual(1.1, l[2].ValueAsDouble);
      Assert.AreEqual(ErlTypeOrder.ErlBoolean, l[3].TypeOrder);
      Assert.AreEqual(true, l[3].ValueAsBool);
      Assert.AreEqual(ErlTypeOrder.ErlByte, l[4].TypeOrder);
      Assert.AreEqual(255, l[4].ValueAsInt);
      Assert.AreEqual(ErlTypeOrder.ErlByte, l[5].TypeOrder);
      Assert.AreEqual('x', l[5].ValueAsChar);
      Assert.AreEqual(ErlTypeOrder.ErlAtom, l[6].TypeOrder);
      Assert.AreEqual("a", l[6].ValueAsString);

      Assert.IsTrue(l.Matches(r));
      Assert.AreEqual(new ErlVarBind(), l.Match(r));

      Assert.AreEqual(l, r);
      Assert.IsTrue(l.Equals(r));
      Assert.AreEqual("[\"test\",1,1.1,true,255,120,a]", l.ToString());
      Assert.IsFalse(l.IsScalar);
      Assert.AreEqual(ErlTypeOrder.ErlList, l.TypeOrder);

      IErlObject temp = null;
      Assert.IsFalse(l.Subst(ref temp, new ErlVarBind()));
      Assert.IsTrue(new ErlList(new ErlVar(X), true, 1).Subst(ref temp, new ErlVarBind { { X, new ErlLong(10) } }));
      Assert.AreEqual("[10,true,1]", temp.ToString());

      Assert.AreEqual(1, l.Visit(0, (acc, o) => acc + (o is ErlAtom ? 1 : 0)));

      var d = new DateTime(2013, 1, 2);
      var ts = new TimeSpan(1, 2, 3);

      Assert.DoesNotThrow(() => { var x = l.ValueAsObject; });
      Assert.AreEqual(1, new ErlList("1")[0].ValueAsInt);
      Assert.AreEqual(1, new ErlList("1")[0].ValueAsLong);
      Assert.AreEqual(1, new ErlList("1")[0].ValueAsDecimal);
      Assert.AreEqual(d, new ErlList(d.ToString())[0].ValueAsDateTime);
      Assert.AreEqual(ts, new ErlList(ts.ToString())[0].ValueAsTimeSpan);
      Assert.AreEqual(1.0, new ErlList("1.0")[0].ValueAsDouble);
      Assert.AreEqual("a", new ErlList("a")[0].ValueAsString);
      Assert.IsTrue(new ErlList("true")[0].ValueAsBool);
      Assert.IsFalse(new ErlList("xxxx")[0].ValueAsBool);

      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsInt; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsLong; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsDecimal; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsDateTime; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsTimeSpan; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsDouble; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsString; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsBool; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsChar; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsByteArray; });

      List<IErlObject> s = l;
      Assert.AreEqual(l.Value, s);
      Assert.IsFalse(new ErlTuple(1, 1.0, "a").Equals(new ErlList(1, 1.0, "a")));
      Assert.IsFalse(new ErlTuple(1, 1.0, "a") == new ErlList(1, 1.0, "a"));
    }

    [Test]
    public void ErlLongTest()
    {
      var t = new ErlLong(10);
      Assert.IsTrue(t.Equals(new ErlLong(10)));
      Assert.AreEqual(t, new ErlLong(10));
      Assert.AreEqual(-1, new ErlAtom("ok").CompareTo(t));
      Assert.AreEqual(10, t);
      Assert.AreEqual((byte)10, t);
      Assert.AreEqual((long)10, t);
      Assert.AreEqual(10, t.Value);
      Assert.AreEqual(10, t.ValueAsInt);
      Assert.AreEqual(10, t.ValueAsLong);
      Assert.AreEqual("10", t.ToString());
      Assert.IsTrue(t.IsScalar);
      Assert.AreEqual(ErlTypeOrder.ErlLong, t.TypeOrder);

      Assert.IsTrue(t.Matches(new ErlLong(10)));
      Assert.AreEqual(new ErlVarBind(), t.Match(new ErlLong(10)));

      Assert.DoesNotThrow(() => { var x = t.ValueAsObject; });
      Assert.AreEqual(10, t.ValueAsInt);
      Assert.AreEqual(10, t.ValueAsLong);
      Assert.AreEqual(10, t.ValueAsDecimal);
      Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 0) + new TimeSpan(10 * 10), t.ValueAsDateTime);
      Assert.AreEqual(new TimeSpan(0, 0, 10), t.ValueAsTimeSpan);
      Assert.AreEqual(10.0, t.ValueAsDouble);
      Assert.AreEqual("10", t.ValueAsString);
      Assert.AreEqual(true, t.ValueAsBool);
      Assert.AreEqual('\n', t.ValueAsChar);
      Assert.Throws<ErlException>(() => { var x = new ErlLong(256).ValueAsChar; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsByteArray; });


      IErlObject temp = null;
      Assert.IsFalse(t.Subst(ref temp, new ErlVarBind()));
      Assert.AreEqual(10, t.Visit(0, (acc, o) => acc + o.ValueAsInt));

      int n = (int)t; Assert.AreEqual(10, n);
      long m = t; Assert.AreEqual(10, m);
      ErlLong a = 100;        // Implicit conversion
      Assert.AreEqual(100, a.Value);

      Assert.AreEqual(new ErlByte(127), new ErlLong(127));
      Assert.AreEqual(new ErlByte(255), new ErlLong(255));
      Assert.AreEqual(new ErlByte(0), new ErlLong(0));
    }

    [Test]
    public void ErlPidTest()
    {
      var t = new ErlPid("test", 10, 3, 1);
      Assert.AreEqual("test", t.Node.Value);
      Assert.IsTrue(t.Equals(new ErlPid("test", 10, 3, 1)));
      Assert.AreEqual(t, new ErlPid("test", 10, 3, 1));
      Assert.AreEqual(1, new ErlPid("tesu", 10, 3, 1).CompareTo(t));
      Assert.AreEqual(-1, new ErlPid("tess", 10, 3, 1).CompareTo(t));
      Assert.AreEqual(-1, new ErlPid("test", 9, 3, 1).CompareTo(t));
      Assert.AreEqual(1, new ErlPid("test", 12, 4, 1).CompareTo(t));
      Assert.AreEqual(-1, new ErlPid("test", 10, 2, 1).CompareTo(t));
      Assert.AreEqual(1, new ErlPid("test", 10, 4, 1).CompareTo(t));
      Assert.AreEqual(-1, new ErlPid("test", 10, 3, 0).CompareTo(t));
      Assert.AreEqual(1, new ErlPid("test", 10, 3, 2).CompareTo(t));
      Assert.AreEqual("#Pid<test.10.3.1>", t.ToString());
      Assert.AreEqual("#Pid<test.10.3.1>", t.ValueAsString);
      Assert.IsTrue(t.IsScalar);
      Assert.AreEqual(ErlTypeOrder.ErlPid, t.TypeOrder);

      Assert.IsTrue(t.Matches(new ErlPid("test", 10, 3, 1)));
      Assert.AreEqual(new ErlVarBind(), t.Match(new ErlPid("test", 10, 3, 1)));

      Assert.DoesNotThrow(() => { var x = t.ValueAsObject; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsInt; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsLong; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDecimal; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDateTime; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsTimeSpan; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDouble; });
      Assert.AreEqual("#Pid<test.10.3.1>", t.ValueAsString);


      var r = ErlPid.Parse("#Pid<test.10.3.1>");
      Assert.AreEqual(new ErlAtom("test"), r.Node);
      Assert.AreEqual(10, r.Id);
      Assert.AreEqual(3, r.Serial);
      Assert.AreEqual(1, r.Creation);


      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsBool; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsChar; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsByteArray; });

      Assert.IsTrue(new ErlPid("test", 0, 0, 0).Empty);

      IErlObject temp = null;
      Assert.IsFalse(t.Subst(ref temp, new ErlVarBind()));
      Assert.AreEqual(true, t.Visit(false, (acc, o) => acc |= o is ErlPid));
    }

    [Test]
    public void ErlPortTest()
    {
      var t = new ErlPort("test", 10, 1);
      Assert.IsTrue(t.Equals(new ErlPort("test", 10, 1)));
      Assert.AreEqual(t, new ErlPort("test", 10, 1));
      Assert.AreEqual(1, new ErlPort("tesu", 10, 1).CompareTo(t));
      Assert.AreEqual(-1, new ErlPort("tess", 10, 1).CompareTo(t));
      Assert.AreEqual(-1, new ErlPort("test", 9, 1).CompareTo(t));
      Assert.AreEqual(1, new ErlPort("test", 12, 1).CompareTo(t));
      Assert.AreEqual(-1, new ErlPort("test", 10, 0).CompareTo(t));
      Assert.AreEqual(1, new ErlPort("test", 10, 2).CompareTo(t));
      Assert.AreEqual("#Port<test.10>", t.ToString());
      Assert.AreEqual("#Port<test.10>", t.ValueAsString);
      Assert.IsTrue(t.IsScalar);
      Assert.AreEqual(ErlTypeOrder.ErlPort, t.TypeOrder);

      Assert.IsTrue(t.Matches(new ErlPort("test", 10, 1)));
      Assert.AreEqual(new ErlVarBind(), t.Match(new ErlPort("test", 10, 1)));

      Assert.DoesNotThrow(() => { var x = t.ValueAsObject; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsInt; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsLong; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDecimal; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDateTime; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsTimeSpan; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDouble; });
      Assert.AreEqual("#Port<test.10>", t.ValueAsString);
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsBool; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsChar; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsByteArray; });

      IErlObject temp = null;
      Assert.IsFalse(t.Subst(ref temp, new ErlVarBind()));
      Assert.IsTrue(t.Visit(false, (acc, o) => acc |= o is ErlPort));
    }

    [Test]
    public void ErlRefTest()
    {
      var ids = new int[] { 5, 6, 7 };
      var t = new ErlRef("test", 5, 6, 7, 1);
      var t1 = new ErlRef("test", ids, 1);

      Assert.AreEqual(t, t1);

      Assert.IsTrue(t.Equals(new ErlRef("test", ids, 1)));
      Assert.AreEqual(t, new ErlRef("test", ids, 1));
      Assert.AreEqual(1, new ErlRef("tesu", new int[] { 5, 6, 7 }, 1).CompareTo(t));
      Assert.AreEqual(-1, new ErlRef("tess", new int[] { 5, 6, 7 }, 1).CompareTo(t));
      Assert.AreEqual(-1, new ErlRef("test", new int[] { 4, 6, 7 }, 1).CompareTo(t));
      Assert.AreEqual(1, new ErlRef("test", new int[] { 8, 6, 7 }, 1).CompareTo(t));
      Assert.AreEqual(-1, new ErlRef("test", new int[] { 5, 4, 7 }, 1).CompareTo(t));
      Assert.AreEqual(1, new ErlRef("test", new int[] { 5, 8, 7 }, 1).CompareTo(t));
      Assert.AreEqual(-1, new ErlRef("test", new int[] { 5, 6, 4 }, 1).CompareTo(t));
      Assert.AreEqual(1, new ErlRef("test", new int[] { 5, 6, 9 }, 1).CompareTo(t));
      Assert.AreEqual(-1, new ErlRef("test", new int[] { 5, 6, 7 }, 0).CompareTo(t));
      Assert.AreEqual(1, new ErlRef("test", new int[] { 5, 6, 7 }, 2).CompareTo(t));
      Assert.AreEqual("#Ref<test.5.6.7.1>", t.ToString());
      Assert.AreEqual("#Ref<test.5.6.7.1>", t.ValueAsString);
      Assert.IsTrue(t.IsScalar);
      Assert.AreEqual(ErlTypeOrder.ErlRef, t.TypeOrder);

      Assert.IsTrue(t.Matches(t1));
      Assert.AreEqual(new ErlVarBind(), t.Match(t1));

      Assert.DoesNotThrow(() => { var x = t.ValueAsObject; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsInt; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsLong; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDecimal; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDateTime; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsTimeSpan; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDouble; });
      Assert.AreEqual("#Ref<test.5.6.7.1>", t.ValueAsString);

      var r = ErlRef.Parse("#Ref<test.5.6.7.1>");
      Assert.AreEqual(new ErlAtom("test"), r.Node);
      Assert.AreEqual(5, r.Ids[0]);
      Assert.AreEqual(6, r.Ids[1]);
      Assert.AreEqual(7, r.Ids[2]);
      Assert.AreEqual(1, r.Creation);

      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsBool; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsChar; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsByteArray; });

      IErlObject temp = null;
      Assert.IsFalse(t.Subst(ref temp, new ErlVarBind()));
      Assert.IsTrue(t.Visit(false, (acc, o) => acc |= o is ErlRef));
    }

    [Test]
    public void ErlStringTest()
    {
      var t = new ErlString("test");
      Assert.IsTrue(t.Equals(new ErlString("test")));
      Assert.AreEqual(t, new ErlString("test"));
      Assert.AreEqual("test", t.Value);
      Assert.AreEqual("\"test\"", t.ToString());
      Assert.IsTrue(t.IsScalar);
      Assert.AreEqual(ErlTypeOrder.ErlString, t.TypeOrder);

      Assert.AreNotEqual("test", new ErlString("Test").ToString());

      Assert.IsTrue(t.Matches(new ErlString("test")));
      Assert.AreEqual(new ErlVarBind(), t.Match(new ErlString("test")));

      IErlObject temp = null;
      Assert.IsFalse(t.Subst(ref temp, new ErlVarBind()));
      Assert.AreEqual(4, t.Visit(0, (acc, o) => acc + t.ValueAsString.Length));

      var d = new DateTime(2013, 1, 2);
      var ts = new TimeSpan(1, 2, 3);

      Assert.DoesNotThrow(() => { var x = t.ValueAsObject; });
      Assert.AreEqual(1, new ErlString("1").ValueAsInt);
      Assert.AreEqual(1, new ErlString("1").ValueAsLong);
      Assert.AreEqual(1, new ErlString("1").ValueAsDecimal);
      Assert.AreEqual(d, new ErlString(d.ToString()).ValueAsDateTime);
      Assert.AreEqual(ts, new ErlString(ts.ToString()).ValueAsTimeSpan);
      Assert.AreEqual(1.0, new ErlString("1.0").ValueAsDouble);
      Assert.AreEqual("a", new ErlString("a").ValueAsString);
      Assert.IsTrue(new ErlString("true").ValueAsBool);
      Assert.IsFalse(new ErlString("xxxx").ValueAsBool);
      Assert.AreEqual('a', new ErlString("a").ValueAsChar);
      Assert.Throws<ErlException>(() => { var x = t.ValueAsChar; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsByteArray; });

      string s = t;           // Implicit conversion
      Assert.AreEqual("test", s);
      ErlString a = "abc";    // Implicit conversion
      Assert.AreEqual("abc", a.Value);
    }

    [Test]
    public void ErlTupleTest()
    {
      var l = new ErlTuple("test", 1, 1.1, true, (byte)255, 'x', new ErlAtom("a"));
      var r = new ErlTuple("test", 1, 1.1, true, (byte)255, 'x', new ErlAtom("a"));

      Assert.AreEqual(7, l.Count);
      Assert.AreEqual(ErlTypeOrder.ErlString, l[0].TypeOrder);
      Assert.AreEqual("test", l[0].ValueAsString);
      Assert.AreEqual(ErlTypeOrder.ErlLong, l[1].TypeOrder);
      Assert.AreEqual(1, l[1].ValueAsInt);
      Assert.AreEqual(ErlTypeOrder.ErlDouble, l[2].TypeOrder);
      Assert.AreEqual(1.1, l[2].ValueAsDouble);
      Assert.AreEqual(ErlTypeOrder.ErlBoolean, l[3].TypeOrder);
      Assert.AreEqual(true, l[3].ValueAsBool);
      Assert.AreEqual(ErlTypeOrder.ErlByte, l[4].TypeOrder);
      Assert.AreEqual(255, l[4].ValueAsInt);
      Assert.AreEqual(ErlTypeOrder.ErlByte, l[5].TypeOrder);
      Assert.AreEqual('x', l[5].ValueAsChar);
      Assert.AreEqual(ErlTypeOrder.ErlAtom, l[6].TypeOrder);
      Assert.AreEqual("a", l[6].ValueAsString);

      Assert.IsTrue(l.Matches(r));
      Assert.AreEqual(new ErlVarBind(), l.Match(r));

      Assert.AreEqual(l, r);
      Assert.IsTrue(l.Equals(r));
      Assert.AreEqual("{\"test\",1,1.1,true,255,120,a}", l.ToString());
      Assert.IsFalse(l.IsScalar);
      Assert.AreEqual(ErlTypeOrder.ErlTuple, l.TypeOrder);

      IErlObject temp = null;
      Assert.IsFalse(l.Subst(ref temp, new ErlVarBind()));
      Assert.AreEqual(1, l.Visit(0, (acc, o) => acc + (o is ErlAtom ? 1 : 0)));
      Assert.IsTrue(new ErlTuple(new ErlVar(X), true, 1).Subst(ref temp, new ErlVarBind { { X, new ErlLong(10) } }));
      Assert.AreEqual("{10,true,1}", temp.ToString());

      var d = new DateTime(2013, 1, 2);
      var ts = new TimeSpan(1, 2, 3);

      Assert.DoesNotThrow(() => { var x = l.ValueAsObject; });
      Assert.AreEqual(1, new ErlList("1")[0].ValueAsInt);
      Assert.AreEqual(1, new ErlList("1")[0].ValueAsLong);
      Assert.AreEqual(1, new ErlList("1")[0].ValueAsDecimal);
      Assert.AreEqual(d, new ErlList(d.ToString())[0].ValueAsDateTime);
      Assert.AreEqual(ts, new ErlList(ts.ToString())[0].ValueAsTimeSpan);
      Assert.AreEqual(1.0, new ErlList("1.0")[0].ValueAsDouble);
      Assert.AreEqual("a", new ErlList("a")[0].ValueAsString);
      Assert.IsTrue(new ErlList("true")[0].ValueAsBool);
      Assert.IsFalse(new ErlList("xxxx")[0].ValueAsBool);

      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsInt; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsLong; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsDecimal; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsDateTime; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsTimeSpan; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsDouble; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsString; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsBool; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsChar; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsByteArray; });


      List<IErlObject> s = l;
      Assert.AreEqual(l.Value, s);

      Assert.IsFalse(new ErlList(1, 1.0, "a").Equals(new ErlTuple(1, 1.0, "a")));
      Assert.IsFalse(new ErlList(1, 1.0, "a") == new ErlTuple(1, 1.0, "a"));
      Assert.IsTrue(new ErlList(1, 1.0, "a") == new ErlList(1, 1.0, "a"));
    }

    [Test]
    public void ErlVarTest()
    {
      var t = ErlVar.Any;
      Assert.IsFalse(t.Equals(new ErlVar(ConstAtoms.ANY)));
      Assert.AreEqual(ConstAtoms.ANY, t.Name);
      Assert.AreEqual(ErlTypeOrder.ErlObject, t.ValueType);

      t = new ErlVar(N, ErlTypeOrder.ErlLong);
      Assert.AreEqual("N", t.Name.Value);
      Assert.AreEqual(ErlTypeOrder.ErlLong, t.ValueType);

      {
        var bind = new ErlVarBind();
        Assert.IsTrue(t.Match(new ErlByte(10), bind));
        Assert.AreEqual(10, bind["N"].ValueAsLong);
        bind.Clear();
        var q = new ErlVar("N", ErlTypeOrder.ErlByte);
        Assert.IsTrue(q.Match(new ErlLong(111), bind));
        Assert.AreEqual(111, bind["N"].ValueAsLong);
      }

      Assert.IsFalse(t.Matches(new ErlVar()));
      Assert.IsFalse(new ErlVar(A).Matches(new ErlVar(A)));
      Assert.IsFalse(new ErlVar(A).Matches(new ErlVar(B)));
      Assert.AreEqual(new ErlVarBind { { N, (ErlLong)10 } }, t.Match((ErlLong)10));
      Assert.AreEqual(new ErlVarBind { { A, (ErlLong)10 } }, new ErlVar(A).Match((ErlLong)10));

      Assert.AreEqual(-1, new ErlAtom("ok").CompareTo(t));
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsObject; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsInt; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsLong; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDecimal; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDateTime; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsTimeSpan; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDouble; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsString; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsBool; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsChar; });
      Assert.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsByteArray; });
      Assert.AreEqual("N::int()", t.ToString());
      Assert.IsTrue(t.IsScalar);
      Assert.AreEqual(ErlTypeOrder.ErlVar, t.TypeOrder);

      IErlObject temp = null;
      Assert.IsFalse(t.Subst(ref temp, new ErlVarBind { { M, new ErlLong(100) } }));
      Assert.IsTrue(t.Subst(ref temp, new ErlVarBind { { N, new ErlLong(100) } }));
      Assert.AreEqual(new ErlLong(100), temp);

      temp = new ErlVar(M, ErlTypeOrder.ErlLong);
      Assert.IsTrue(temp.Subst(ref temp, new ErlVarBind { { M, new ErlLong(100) } }));
      Assert.AreEqual(ErlTypeOrder.ErlVar, t.Visit(ErlTypeOrder.ErlByte, (acc, o) => ((ErlVar)o).TypeOrder));
      Assert.AreEqual(new ErlLong(100), temp);

      temp = new ErlVar(N, ErlTypeOrder.ErlObject);
      Assert.IsTrue(temp.Subst(ref temp, new ErlVarBind { { N, new ErlLong(100) } }));

      // Invalid variable type
      temp = new ErlVar(N, ErlTypeOrder.ErlAtom);
      Assert.Throws<ErlException>(() => temp.Subst(ref temp, new ErlVarBind { { N, new ErlLong(100) } }));
    }

    [Test]
    public void ErlVarBindTest()
    {
      var bind = new ErlVarBind
            {
                {new ErlVar("A", ErlTypeOrder.ErlLong),    10},
                {new ErlVar("B", ErlTypeOrder.ErlAtom), "abc"},
                { "C", ErlTypeOrder.ErlDouble, 5}
            };

      Assert.AreEqual((IErlObject)(new ErlLong(10)), bind["A"]);
      Assert.AreEqual((IErlObject)(new ErlAtom("abc")), bind["B"]);
      Assert.AreEqual((IErlObject)(new ErlDouble(5.0)), bind["C"]);

      var term = NFX.Erlang.ErlObject.Parse("{ok, {A::int(), [sasha, B::atom(), C::float()]}}");
      var set = term.Visit(new SortedSet<ErlVar>(), (a, o) => { if (o is ErlVar) a.Add((ErlVar)o); return a; });

      IErlObject res = null;
      Assert.IsTrue(term.Subst(ref res, bind));
      Assert.AreEqual("{ok, {10, [sasha, abc, 5.0]}}".ToErlObject(), res);
    }

    [Test]
    public void ErlTypeConversionTest()
    {
      Assert.AreEqual((IErlObject)new ErlAtom("abc"), "abc".ToErlObject(ErlTypeOrder.ErlAtom));
      Assert.AreEqual((IErlObject)new ErlString("abc"), "abc".ToErlObject(ErlTypeOrder.ErlString));
      Assert.AreEqual((IErlObject)new ErlLong(10), 10.ToErlObject(ErlTypeOrder.ErlLong));
      Assert.AreEqual((IErlObject)new ErlDouble(10), 10.ToErlObject(ErlTypeOrder.ErlDouble));
      Assert.AreEqual((IErlObject)new ErlBoolean(true), true.ToErlObject(ErlTypeOrder.ErlBoolean));
      Assert.AreEqual((IErlObject)new ErlBoolean(true), 10.ToErlObject(ErlTypeOrder.ErlBoolean));
      Assert.AreEqual((IErlObject)new ErlBinary(new byte[] { 1, 2, 3 }), (new byte[] { 1, 2, 3 }).ToErlObject(ErlTypeOrder.ErlBinary));
      Assert.AreEqual((IErlObject)new ErlList(1, 2, 3), (new List<int> { 1, 2, 3 }).ToErlObject(ErlTypeOrder.ErlList));
    }
  }
}
