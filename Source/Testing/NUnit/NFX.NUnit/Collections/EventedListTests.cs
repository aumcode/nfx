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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;


using NFX.Collections;



namespace NFX.NUnit.Collections
{
    [TestFixture]
    public class EventedListTests
    {
       
        [TestCase]
        public void List_SwitchContext_1()
        {
          var lst = new EventedList<string, string>("CONTEXT", false); 
          Assert.AreEqual("CONTEXT", lst.Context);
          Assert.IsFalse(lst.ContextReadOnly);
          lst.Context = "yesyes";
          Assert.AreEqual("yesyes", lst.Context);
        }

        [TestCase]
        [ExpectedException(typeof(NFXException), ExpectedMessage="Invalid operation",  MatchType=MessageMatch.Contains)]
        public void List_SwitchContext_2()
        {
          var lst = new EventedList<string, string>("CONTEXT", true); 
          Assert.AreEqual("CONTEXT", lst.Context);
          Assert.IsTrue(lst.ContextReadOnly);
          lst.Context = "yesyes";
        }

        [TestCase]
        public void List_Readonly()
        {
          var lst = new EventedList<string, string>("CONTEXT", false);
          
          var ro = false;
          
          lst.GetReadOnlyEvent = (l) => ro;
          
          lst.Add("a"); 
          lst.Add("b"); 
          lst.Add("c");
          
          Assert.AreEqual(3, lst.Count);
          ro = true;
          
          Assert.Throws<NFXException>(() =>  lst.Add("d"));          
        }

        [TestCase]
        public void List_Add()
        {
          var lst = new EventedList<string, string>("CONTEXT", false);
          
          var first = true;
          lst.GetReadOnlyEvent = (_) => false;
          
          lst.ChangeEvent = (l, ct, p, idx, v) => 
                            {
                              Assert.AreEqual( EventedList<string, string>.ChangeType.Add, ct);
                              Assert.AreEqual( first ? EventPhase.Before : EventPhase.After, p);
                              Assert.AreEqual( -1, idx);
                              Assert.AreEqual( "a", v);
                              first = false;
                            };   
          
          lst.Add("a"); 
         
        }

        [TestCase]
        public void List_Remove()
        {
          var lst = new EventedList<string, string>("CONTEXT", false);
          
          var first = true;
          lst.GetReadOnlyEvent = (_) => false;
          
          lst.Add("a");
          lst.Add("b");
          lst.Add("c");

          Assert.AreEqual(3, lst.Count);

          lst.ChangeEvent = (l, ct, p, idx, v) => 
                            {
                              Assert.AreEqual( EventedList<string, string>.ChangeType.Remove, ct);
                              Assert.AreEqual( first ? EventPhase.Before : EventPhase.After, p);
                              Assert.AreEqual( -1, idx);
                              Assert.AreEqual( "b", v);
                              first = false;
                            };   
          
          lst.Remove("b"); 
          Assert.AreEqual(2, lst.Count);
         
        }

        [TestCase]
        public void List_Set()
        {
          var lst = new EventedList<string, string>("CONTEXT", false);
          
          var first = true;
          lst.GetReadOnlyEvent = (_) => false;
          
          lst.Add("a");
          lst.Add("b");
          lst.Add("c");

          Assert.AreEqual(3, lst.Count);

          lst.ChangeEvent = (l, ct, p, idx, v) => 
                            {
                              Assert.AreEqual( EventedList<string, string>.ChangeType.Set, ct);
                              Assert.AreEqual( first ? EventPhase.Before : EventPhase.After, p);
                              Assert.AreEqual( 1, idx);
                              Assert.AreEqual( "z", v);
                              first = false;
                            };   
          
          lst[1] = "z"; 
          Assert.AreEqual("z", lst[1]);
         
        }


        [TestCase]
        public void List_Clear()
        {
          var lst = new EventedList<string, string>("CONTEXT", false);
          
          var first = true;
          lst.GetReadOnlyEvent = (_) => false;
          
          lst.Add("a");
          lst.Add("b");
          lst.Add("c");

          Assert.AreEqual(3, lst.Count);

          lst.ChangeEvent = (l, ct, p, idx, v) => 
                            {
                              Assert.AreEqual( EventedList<string, string>.ChangeType.Clear, ct);
                              Assert.AreEqual( first ? EventPhase.Before : EventPhase.After, p);
                              Assert.AreEqual( -1, idx);
                              Assert.AreEqual( null, v);
                              first = false;
                            };   
          
          lst.Clear(); 
          Assert.AreEqual( 0, lst.Count);
         
        }
       

       
  }
}
