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

using NUnit.Framework;
using NFX.Erlang;

namespace NFX.NUnit.Erlang
{
  [TestFixture]
  public class ErlEncodingTest
  {
    [Test]
    public void ErlTermSerializeTest()
    {
      {
        var b = new byte[] { 131, 100, 0, 3, 97, 98, 99 };
        var t = new ErlAtom("abc");
        var os = new ErlOutputStream(t);
        Assert.AreEqual(b, os.GetBuffer().TakeWhile((_, i) => i < b.Length).ToArray());
        var es = new ErlInputStream(b);
        Assert.AreEqual(t, es.Read());
      }
      {
        var b = new byte[] { 131, 109, 0, 0, 0, 3, 1, 2, 3 };
        var t = new ErlBinary(new byte[] { 1, 2, 3 });
        var os = new ErlOutputStream(t);
        Assert.AreEqual(b, os.GetBuffer().TakeWhile((_, i) => i < b.Length).ToArray());
        var es = new ErlInputStream(b);
        Assert.AreEqual(t, es.Read());
      }
      {
        var b1 = new byte[] { 131, 100, 0, 4, 116, 114, 117, 101 };
        var t1 = new ErlBoolean(true);
        var os1 = new ErlOutputStream(t1);
        Assert.AreEqual(b1, os1.GetBuffer().TakeWhile((_, i) => i < b1.Length).ToArray());
        var es1 = new ErlInputStream(b1);
        Assert.AreEqual(t1, es1.Read());

        var b2 = new byte[] { 131, 100, 0, 5, 102, 97, 108, 115, 101 };
        var t2 = new ErlBoolean(false);
        var os2 = new ErlOutputStream(t2);
        Assert.AreEqual(b2, os2.GetBuffer().TakeWhile((_, i) => i < b2.Length).ToArray());
        var es2 = new ErlInputStream(b2);
        Assert.AreEqual(t2, es2.Read());
      }
      {
        var b = new byte[] { 131, 97, 127 };
        var t = new ErlByte(127);
        var os = new ErlOutputStream(t);
        Assert.AreEqual(b, os.GetBuffer().TakeWhile((_, i) => i < b.Length).ToArray());
        var es = new ErlInputStream(b);
        Assert.AreEqual(t, es.Read());
      }
      {
        var b = new byte[] { 131, 70, 64, 36, 62, 249, 219, 34, 208, 229 };
        var t = new ErlDouble(10.123);
        var os = new ErlOutputStream(t);
        Assert.AreEqual(b, os.GetBuffer().TakeWhile((_, i) => i < b.Length).ToArray());
        var es = new ErlInputStream(b);
        Assert.AreEqual(t, es.Read());
      }
      {
        var b = new byte[] { 131, 108, 0, 0, 0, 2, 107, 0, 1, 1, 107, 0, 1, 2, 106 };
        var t = new ErlList(new ErlList(1), new ErlList(2));
        var os = new ErlOutputStream(t);
        Assert.AreEqual(b, os.GetBuffer().TakeWhile((_, i) => i < b.Length).ToArray());
        var es = new ErlInputStream(b);
        Assert.AreEqual(t, es.Read());
      }
      {
        var b = new byte[] { 131,108,0,0,0,2,108,0,0,0,2,97,1,107,0,1,2,106,107,0,1,3,106 };
        var t = new ErlList(new ErlList(1, new ErlList(2)), new ErlList(3));
        var os = new ErlOutputStream(t);
        Assert.AreEqual(b, os.GetBuffer().TakeWhile((_, i) => i < b.Length).ToArray());
        var es = new ErlInputStream(b);
        Assert.AreEqual(t, es.Read());
      }
      {
        var b = new byte[] { 131,108,0,0,0,3,97,1,70,64,36,61,112,163,215,10,61,108,0,0,0,2,
                             100,0,4,116,114,117,101,107,0,1,97,106,106 };
        var t = new ErlList(1, 10.12, new ErlList(true, "a"));
        var os = new ErlOutputStream(t);
        Assert.AreEqual(b, os.GetBuffer().TakeWhile((_, i) => i < b.Length).ToArray());
        var es = new ErlInputStream(b);
        Assert.AreEqual(t, es.Read());
      }
      {
        var b = new byte[] {
          131,108,0,0,0,3,97,23,97,4,104,1,108,0,0,0,1,104,2,109,0,0,0,5,101,118,101,
          110,116,104,1,108,0,0,0,2,104,2,109,0,0,0,10,102,108,101,101,116,95,104,97,
          115,104,109,0,0,0,36,97,54,97,50,50,100,49,52,45,56,52,56,51,45,52,49,102,99,
          45,97,52,54,98,45,50,56,51,98,57,55,55,55,99,50,97,50,104,2,109,0,0,0,4,116,
          121,112,101,109,0,0,0,13,102,108,101,101,116,95,99,104,97,110,103,101,100,
          106,106,106 };
        var t = ErlObject.Parse("[23,4,{[{<<\"event\">>,"+
                                "{[{<<\"fleet_hash\">>,<<\"a6a22d14-8483-41fc-a46b-283b9777c2a2\">>},"+
                                "{<<\"type\">>,<<\"fleet_changed\">>}]}}]}]");
        var os = new ErlOutputStream(t);
        Assert.AreEqual(b, os.GetBuffer().TakeWhile((_, i) => i < b.Length).ToArray());
        var es = new ErlInputStream(b);
        Assert.AreEqual(t, es.Read());
      }
      {
        var b1 = new byte[] { 131, 98, 255, 255, 255, 251 };
        var t1 = new ErlLong(-5);
        var os1 = new ErlOutputStream(t1);
        Assert.AreEqual(b1, os1.GetBuffer().TakeWhile((_, i) => i < b1.Length).ToArray());
        var es1 = new ErlInputStream(b1);
        Assert.AreEqual(t1, es1.Read());

        var b2 = new byte[] { 131, 97, 5 };
        var t2 = new ErlLong(5);
        var os2 = new ErlOutputStream(t2);
        Assert.AreEqual(b2, os2.GetBuffer().TakeWhile((_, i) => i < b2.Length).ToArray());
        var es2 = new ErlInputStream(b2);
        Assert.AreEqual(t2, es2.Read());

        var b3 = new byte[] { 131, 98, 0, 16, 0, 0 };
        var t3 = new ErlLong(1024 * 1024);
        var os3 = new ErlOutputStream(t3);
        Assert.AreEqual(b3, os3.GetBuffer().TakeWhile((_, i) => i < b3.Length).ToArray());
        var es3 = new ErlInputStream(b3);
        Assert.AreEqual(t3, es3.Read());

        var b4 = new byte[] { 131, 110, 6, 0, 0, 0, 0, 0, 0, 4 };
        var t4 = new ErlLong(1024L * 1024 * 1024 * 1024 * 4);
        var os4 = new ErlOutputStream(t4);
        Assert.AreEqual(b4, os4.GetBuffer().TakeWhile((_, i) => i < b4.Length).ToArray());
        var es4 = new ErlInputStream(b4);
        Assert.AreEqual(t4, es4.Read());

        var b5 = new byte[] { 131, 110, 8, 1, 0, 0, 0, 0, 0, 0, 0, 128 };
        var t5 = new ErlLong(1L << 63);
        var os5 = new ErlOutputStream(t5);
        Assert.AreEqual(b5, os5.GetBuffer().TakeWhile((_, i) => i < b5.Length).ToArray());
        var es5 = new ErlInputStream(b5);
        Assert.AreEqual(t5, es5.Read());

        var b6 = new byte[] { 131, 110, 8, 1, 0, 0, 0, 0, 0, 0, 0, 128 };
        var t6 = new ErlLong(-1L << 63);
        var os6 = new ErlOutputStream(t6);
        Assert.AreEqual(b6, os6.GetBuffer().TakeWhile((_, i) => i < b6.Length).ToArray());
        var es6 = new ErlInputStream(b6);
        Assert.AreEqual(t6, es6.Read());

        var b7 = new byte[] { 131, 110, 8, 0, 255, 255, 255, 255, 255, 255, 255, 255 };
        var es7 = new ErlInputStream(b7);
        var t7 = new ErlLong(-1);
        Assert.AreEqual(t7, es7.Read());
        var bi7 = new byte[] {131, 98, 255, 255, 255, 255};
        var os7 = new ErlOutputStream(t7);
        Assert.AreEqual(bi7, os7.GetBuffer().TakeWhile((_, i) => i < bi7.Length).ToArray());
      }
      {
        var b = new byte[] { 131, 103, 100, 0, 7, 98, 64, 112, 105, 112, 105, 116, 0, 0, 0, 38, 0, 0, 0, 0, 1 };
        var t = new ErlPid("b@pipit", 38, 0, 1);
        var os = new ErlOutputStream(t);
        Assert.AreEqual(b, os.GetBuffer().TakeWhile((_, i) => i < b.Length).ToArray());
        var es = new ErlInputStream(b);
        Assert.AreEqual(t, es.Read());
      }
      {
        var b = new byte[] { 131, 102, 100, 0, 7, 98, 64, 112, 105, 112, 105, 116, 0, 0, 0, 38, 1 };
        var t = new ErlPort("b@pipit", 38, 1);
        var os = new ErlOutputStream(t);
        Assert.AreEqual(b, os.GetBuffer().TakeWhile((_, i) => i < b.Length).ToArray());
        var es = new ErlInputStream(b);
        Assert.AreEqual(t, es.Read());
      }
      {
        var b = new byte[] { 131, 114, 0, 3, 100, 0, 7, 98, 64, 112, 105, 112, 105, 116, 1, 0, 0, 0, 181, 0, 0, 0, 0, 0, 0, 0, 0 };
        var t = new ErlRef("b@pipit", 181, 0, 0, 1);
        var os = new ErlOutputStream(t);
        Assert.AreEqual(b, os.GetBuffer().TakeWhile((_, i) => i < b.Length).ToArray());
        var es = new ErlInputStream(b);
        Assert.AreEqual(t, es.Read());
      }
      {
        var b = new byte[] { 131, 107, 0, 3, 115, 116, 114 };
        var t = new ErlString("str");
        var os = new ErlOutputStream(t);
        Assert.AreEqual(b, os.GetBuffer().TakeWhile((_, i) => i < b.Length).ToArray());
        var es = new ErlInputStream(b);
        Assert.AreEqual(t, es.Read());
      }
      {
        var b = new byte[] { 131, 104, 3, 97, 1, 100, 0, 1, 97, 104, 2, 97, 10, 70, 63, 241, 247, 206, 217, 22, 135, 43 };
        var t = new ErlTuple(1, new ErlAtom("a"), new ErlTuple(10, 1.123));
        var os = new ErlOutputStream(t);
        Assert.AreEqual(b, os.GetBuffer().TakeWhile((_, i) => i < b.Length).ToArray());
        var es = new ErlInputStream(b);
        Assert.AreEqual(t, es.Read());
      }
      {
        var b = new byte[] { 131, 116, 0, 0, 0, 2, 100, 0, 1, 97, 97, 1, 107, 0, 3, 115, 116, 114, 70, 64, 0, 0, 0, 0, 0, 0, 0 };
        var t = new ErlMap {
          { new ErlAtom("a"), 1.ToErlObject() },
          { new ErlString("str"), new ErlDouble(2.0) }
        };
        var os = new ErlOutputStream(t);
        Assert.AreEqual(b, os.GetBuffer().TakeWhile((_, i) => i < b.Length).ToArray());
        var es = new ErlInputStream(b);
        Assert.AreEqual(t, es.Read());
      }
    }
  }
}
