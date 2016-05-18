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
using System.Text;

using NFX.Collections;

using NUnit.Framework;

namespace NFX.NUnit.Collections
{
  [TestFixture]
  public class LookAheadEnumerableTest
  {
    [Test]
    public void EmptyEnumerable()
    {
      var enumerator = string.Empty.AsLookAheadEnumerable().GetLookAheadEnumerator();
      Assert.False(enumerator.HasNext);
      Assert.Throws<NFXException>(() => {
        var next = enumerator.Next;
      }, StringConsts.OPERATION_NOT_SUPPORTED_ERROR + "{0}.Next(!HasNext)".Args(typeof(LookAheadEnumerator<char>).FullName));
      Assert.Throws<InvalidOperationException>(() => {
        var current = enumerator.Current;
      });
      Assert.False(enumerator.MoveNext());      
    }

    [Test]
    public void SingleEnumerable()
    {
      var enumerator = " ".AsLookAheadEnumerable().GetLookAheadEnumerator();
      Assert.True(enumerator.HasNext);
      Assert.AreEqual(enumerator.Next, ' ');
      Assert.Throws<InvalidOperationException>(() =>
      {
        var current = enumerator.Current;
      });
      Assert.True(enumerator.MoveNext());
      Assert.False(enumerator.HasNext);
      Assert.Throws<NFXException>(() =>
      {
        var next = enumerator.Next;
      }, StringConsts.OPERATION_NOT_SUPPORTED_ERROR + "{0}.Next(!HasNext)".Args(typeof(LookAheadEnumerator<char>).FullName));
      Assert.AreEqual(enumerator.Current, ' ');
      Assert.False(enumerator.MoveNext());
      enumerator.Reset();
      Assert.AreEqual(enumerator.HasNext, true);
      Assert.AreEqual(enumerator.Next, ' ');
      Assert.Throws<InvalidOperationException>(() =>
      {
        var current = enumerator.Current;
      });
      Assert.True(enumerator.MoveNext());
    }

    [Test]
    public void MulripleEnumerable()
    {
      var enumerator = "+-".AsLookAheadEnumerable().GetLookAheadEnumerator();
      Assert.True(enumerator.HasNext);
      Assert.AreEqual(enumerator.Next, '+');
      Assert.Throws<InvalidOperationException>(() =>
      {
        var current = enumerator.Current;
      });
      Assert.True(enumerator.MoveNext());
      Assert.True(enumerator.HasNext);
      Assert.AreEqual(enumerator.Next, '-');
      Assert.AreEqual(enumerator.Current, '+');
      Assert.True(enumerator.MoveNext());
      Assert.False(enumerator.HasNext);
      Assert.Throws<NFXException>(() =>
      {
        var next = enumerator.Next;
      }, StringConsts.OPERATION_NOT_SUPPORTED_ERROR + "{0}.Next(!HasNext)".Args(typeof(LookAheadEnumerator<char>).FullName));
      Assert.AreEqual(enumerator.Current, '-');
      Assert.False(enumerator.MoveNext());
      enumerator.Reset();
      Assert.True(enumerator.HasNext);
      Assert.AreEqual(enumerator.Next, '+');
      Assert.Throws<InvalidOperationException>(() =>
      {
        var current = enumerator.Current;
      });
      Assert.True(enumerator.MoveNext());
    }

    [Test]
    public void EmptyEnumerable_AsEnumerable()
    {
      var enumerator = string.Empty.AsLookAheadEnumerable().GetEnumerator();
      Assert.Throws<InvalidOperationException>(() =>
      {
        var current = enumerator.Current;
      });
      Assert.False(enumerator.MoveNext());
    }

    [Test]
    public void SingleEnumerable_AsEnumerable()
    {
      var enumerator = " ".AsLookAheadEnumerable().GetEnumerator();
      Assert.Throws<InvalidOperationException>(() =>
      {
        var current = enumerator.Current;
      });
      Assert.True(enumerator.MoveNext());
      Assert.AreEqual(enumerator.Current, ' ');
      Assert.False(enumerator.MoveNext());
      enumerator.Reset();
      Assert.Throws<InvalidOperationException>(() =>
      {
        var current = enumerator.Current;
      });
      Assert.True(enumerator.MoveNext());
    }
    
    [Test]
    public void MultipleEnumerable_AsEnumerable()
    {
      var enumerator = "+-".AsLookAheadEnumerable().GetEnumerator();
      Assert.Throws<InvalidOperationException>(() =>
      {
        var current = enumerator.Current;
      });
      Assert.True(enumerator.MoveNext());
      Assert.AreEqual(enumerator.Current, '+');
      Assert.True(enumerator.MoveNext());
      Assert.AreEqual(enumerator.Current, '-');
      Assert.False(enumerator.MoveNext());
      enumerator.Reset();
      Assert.Throws<InvalidOperationException>(() =>
      {
        var current = enumerator.Current;
      });
      Assert.True(enumerator.MoveNext());
    }

    [Test]
    public void ForEach()
    {
      var sb = new StringBuilder();
      foreach (var c in string.Empty.AsLookAheadEnumerable()) sb.Append(c);
      Assert.That(sb.ToString(), 
        Is.EqualTo(string.Empty));
      sb.Clear();
      foreach (var c in "+".AsLookAheadEnumerable()) sb.Append(c);
      Assert.That(sb.ToString(), 
        Is.EqualTo("+"));
      sb.Clear();
      foreach (var c in "+-".AsLookAheadEnumerable()) sb.Append(c);
      Assert.That(sb.ToString(), 
        Is.EqualTo("+-"));
    }

    [Test]
    public void DetectSimbolPair()
    {
      var enumerator = @" """" ".AsLookAheadEnumerable().GetLookAheadEnumerator();
      var detect = false;
      while (!detect && enumerator.MoveNext())
      {
        if ('\"' == enumerator.Current && enumerator.HasNext && '\"' == enumerator.Next)
          detect = true;
      }
      Assert.True(detect);
      enumerator = @"""  """.AsLookAheadEnumerable().GetLookAheadEnumerator();
      detect = false;
      while (!detect && enumerator.MoveNext())
      {
        if ('\"' == enumerator.Current && enumerator.HasNext && '\"' == enumerator.Next)
          detect = true;
      }
      Assert.False(detect);
    }
  }
}
