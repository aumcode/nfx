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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;


using E = NFX.Parsing.Evaluator;

namespace NFX.NUnit.Parsing
{
    [TestFixture]   
    public class Evaluator
    {
        [TestCase]
        public void BasicArithmetic()
        {
           var e = new E("2+2-4");
           Assert.AreEqual("0",  e.Evaluate());
        }

        [TestCase]
        public void Precedence()
        {
           var e = new E("2+2*10");
           Assert.AreEqual("22",  e.Evaluate());
        }

        [TestCase]
        public void Precedence2()
        {
           var e = new E("(2+2)*10");
           Assert.AreEqual("40",  e.Evaluate());
        }

        [TestCase]
        public void Unary()
        {
           var e = new E("-(2+2)*10");
           Assert.AreEqual("-40",  e.Evaluate());
        }

        [TestCase]
        public void StringArithmetic()
        {
           var e = new E("'cold='+(-(2+2)*10)");
           Assert.AreEqual("cold=-40",  e.Evaluate());
        }

        [TestCase]
        public void StringArithmetic2()
        {
           var e = new E("'cold='+'hot'");
           Assert.AreEqual("cold=hot",  e.Evaluate());
        }

        [TestCase]
        public void StringArithmeticWithVars()
        {
           var e = new E("'cold='+(-(2+2)*x)");
           e.OnIdentifierLookup += new NFX.Parsing.IdentifierLookup((ident)=>ident=="x"?"100":ident);
           Assert.AreEqual("cold=-400",  e.Evaluate());
        }

        [TestCase]
        public void StringArithmeticWithVars_Lambda()
        {
           var e = new E("'cold='+(-(2+2)*x)");
           Assert.AreEqual("cold=-400",  e.Evaluate((ident)=>ident=="x"?"100":ident));
        }

        [TestCase]
        public void Constants()
        {
           var e = new E("Pi*2");
           Assert.AreEqual("6.2831",  e.Evaluate().Substring(0, 6));
        }

        [TestCase]
        public void Conditional()
        {
           var e = new E("'less:'+(?10<20;'yes';'no')");
           Assert.AreEqual("less:yes",  e.Evaluate());
        }

        [TestCase]
        public void ConditionalWithVars()
        {
           var e = new E("'less:'+(?x<y;'yes';'no')");
           
           var x = "10";
           var y = "20";
           e.OnIdentifierLookup += new NFX.Parsing.IdentifierLookup((ident)=>ident=="x"?x:ident=="y"?y:ident);
          
           Assert.AreEqual("less:yes",  e.Evaluate());

           y = "0";
           Assert.AreEqual("less:no",  e.Evaluate());
        }

        [TestCase]
        public void ConditionalWithVars_Lambda()
        {
           var e = new E("'less:'+(?x<y;'yes';'no')");
           
           var x = "10";
           var y = "20";
          
           Assert.AreEqual("less:yes",  e.Evaluate((ident)=>ident=="x"?x:ident=="y"?y:ident));

           y = "0";
           Assert.AreEqual("less:no",  e.Evaluate((ident)=>ident=="x"?x:ident=="y"?y:ident));
        }


    }
}


