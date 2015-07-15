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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;


using NFX.Financial;
using NFX.Serialization.JSON;



namespace NFX.NUnit.Financial
{
    [TestFixture]
    public class AmountTests
    {
       

       [TestCase]
        public void ToString_1()
        {
           var amt = new Amount("usd", 12.45M);
                                 
           Console.WriteLine(amt);    
           Assert.AreEqual("12.45:usd", amt.ToString());    
        }

        [TestCase]
        public void ToString_2()
        {
           var amt = new Amount("usd", -12.45M);
                                 
           Console.WriteLine(amt);    
           Assert.AreEqual("-12.45:usd", amt.ToString());    
        }

        [TestCase]
        public void ToString_3()
        {
           var amt = new Amount("usd", -122M);
                                 
           Console.WriteLine(amt);    
           Assert.AreEqual("-122:usd", amt.ToString());    
        }


        [TestCase]
        public void Equal()
        {
           Assert.IsTrue( new Amount("usd", 10M)  ==  new Amount("usd", 10M) );
        }

        [TestCase]
        public void NotEqual()
        {
           Assert.IsTrue( new Amount("usd", 102.11M)  !=  new Amount("usd", 100.12M) );
        }

        [TestCase]
        public void Less()
        {
           Assert.IsTrue( new Amount("usd", 10M)  <  new Amount("usd", 22.12M) );
        }

        [TestCase]
        public void Greater()
        {
           Assert.IsTrue( new Amount("usd", 102.11M)  >  new Amount("usd", 100.12M) );
        }

        [TestCase]
        public void LessOrEqual()
        {
           Assert.IsTrue( new Amount("usd", 22.12M)  <=  new Amount("usd", 22.12M) );
        }

        [TestCase]
        public void GreaterOrEqual()
        {
           Assert.IsTrue( new Amount("usd", 102.11M)  >=  new Amount("usd", 102.11M) );
        }



        [TestCase]
        public void Add()
        {
           Assert.AreEqual( new Amount("usd", 111.12M),   new Amount("usd", 10M) + new Amount("usd", 101.12M) );
        }

        [TestCase]
        public void Subtract()
        {
           Assert.AreEqual( new Amount("usd", -91.12M),   new Amount("usd", 10M) - new Amount("usd", 101.12M) );
        }

        [TestCase]
        [ExpectedException(typeof(FinancialException), ExpectedMessage="different currencies", MatchType = MessageMatch.Contains)]
        public void Add_2()
        {
           var r = new Amount("sas", 10M) + new Amount("usd", 101.12M);
        }

        [TestCase]
        [ExpectedException(typeof(FinancialException), ExpectedMessage="different currencies", MatchType = MessageMatch.Contains)]
        public void Subtract_2()
        {
           var r = new Amount("sas", 10M) - new Amount("usd", 101.12M);
        }



        [TestCase]
        public void Mul()
        {
           Assert.AreEqual( new Amount("usd", 100.10M),   new Amount("usd", 20.02M) * 5 );
           Assert.AreEqual( new Amount("usd", 100.10M),   5 * new Amount("usd", 20.02M) );

           Assert.AreEqual( new Amount("usd", 100.10M),   new Amount("usd", 20.02M) * 5d );
           Assert.AreEqual( new Amount("usd", 100.10M),   5d * new Amount("usd", 20.02M) );

           Assert.AreEqual( new Amount("usd", 100.10M),   new Amount("usd", 20.02M) * 5M );
           Assert.AreEqual( new Amount("usd", 100.10M),   5M * new Amount("usd", 20.02M) );

        }


        [TestCase]
        public void Div()
        {
           Assert.AreEqual( new Amount("usd", 20M),   new Amount("usd", 100M) / 5 );

           Assert.AreEqual( new Amount("usd", 20M),   new Amount("usd", 100M) / 5d );

           Assert.AreEqual( new Amount("usd", 20M),   new Amount("usd", 100M) / 5M );

        }


        [TestCase]
        public void Compare()
        {
           Assert.IsTrue( new Amount("usd", 100.12M).CompareTo( new Amount("usd", 200m)) < 0);
           Assert.IsTrue( new Amount("usd", 200.12M).CompareTo( new Amount("usd", 100m)) > 0);
           Assert.IsTrue( new Amount("usd", 200.12M).CompareTo( new Amount("usd", 200.12m)) == 0);

        }

        [TestCase]
        public void IsSameCurrency()
        {
           Assert.IsTrue( new Amount("usd", 100.12M).IsSameCurrencyAs( new Amount("usd", 200m)));
           Assert.IsTrue( new Amount("USd", 100.12M).IsSameCurrencyAs( new Amount("usD", 200m)));
           Assert.IsTrue( new Amount("USd ", 100.12M).IsSameCurrencyAs( new Amount("   usD ", 200m)));
           Assert.IsFalse( new Amount("usd", 100.12M).IsSameCurrencyAs( new Amount("eur", 200m)));

        }


        [TestCase]
        public void JSON()
        {
           var data = new {name="aaa", amount=new Amount("usd", 1234.12M)};
           var json = data.ToJSON();

           Console.WriteLine(json);

           Assert.AreEqual(@"{""amount"":{""iso"":""usd"",""v"":1234.12},""name"":""aaa""}", json);

        }


        [TestCase]
        public void Parse_1()
        {
           var a = Amount.Parse("123:usd");
           Assert.AreEqual(new Amount("usd", 123m), a);
        }

        [TestCase]
        public void Parse_2()
        {
           var a = Amount.Parse("-123.12:usd");
           Assert.AreEqual(new Amount("usd", -123.12m), a);
        }

        [TestCase]
        [ExpectedException(typeof(FinancialException), ExpectedMessage="parse", MatchType=MessageMatch.Contains)]
        public void Parse_3()
        {
           Amount.Parse("-123.12");
        }

        [TestCase]
        [ExpectedException(typeof(FinancialException), ExpectedMessage="parse", MatchType = MessageMatch.Contains)]
        public void Parse_4()
        {
           var a = Amount.Parse("-1 23.12");
        }

        [TestCase]
        [ExpectedException(typeof(FinancialException), ExpectedMessage="parse", MatchType = MessageMatch.Contains)]
        public void Parse_5()
        {
           var a = Amount.Parse(":-123.12");
        }


        [TestCase]
        [ExpectedException(typeof(FinancialException), ExpectedMessage="parse", MatchType = MessageMatch.Contains)]
        public void Parse_6()
        {
           var a = Amount.Parse(":123");
        }

        [TestCase]
        public void Parse_7()
        {
           var a = Amount.Parse("-123.12 : uah");
           Assert.AreEqual(new Amount("UAH", -123.12m), a);
        }

        [TestCase]
        [ExpectedException(typeof(FinancialException), ExpectedMessage="parse", MatchType = MessageMatch.Contains)]
        public void Parse_8()
        {
           Amount.Parse("-123.12 :");
        }


        [TestCase]
        public void TryParse_1()
        {
           Amount a;
           var parsed = Amount.TryParse("123:usd", out a);
           Assert.IsTrue( parsed );
           Assert.AreEqual( new Amount("usd", 123), a);
        }

        [TestCase]
        public void TryParse_2()
        {
           Amount a;
           var parsed = Amount.TryParse("-1123:usd", out a);
           Assert.IsTrue( parsed );
           Assert.AreEqual( new Amount("usd", -1123M), a);
        }

        [TestCase]
        public void TryParse_3()
        {
           Amount a;
           var parsed = Amount.TryParse("-1123:eur", out a);
           Assert.IsTrue( parsed );
           Assert.AreEqual( new Amount("eur", -1123M), a);
        }

        [TestCase]
        public void TryParse_4()
        {
           Amount a;
           var parsed = Amount.TryParse("-1123:rub", out a);
           Assert.IsTrue( parsed );
           Assert.AreEqual( new Amount("rub", -1123M), a);
        }

        [TestCase]
        public void TryParse_5()
        {
           Amount a;
           var parsed = Amount.TryParse("-11 23", out a);
           Assert.IsFalse( parsed );
        }

        [TestCase]
        public void TryParse_6()
        {
           Amount a;
           var parsed = Amount.TryParse(":1123", out a);
           Assert.IsFalse( parsed );
        }

        [TestCase]
        public void TryParse_7()
        {
           Amount a;
           var parsed = Amount.TryParse("", out a);
           Assert.IsFalse( parsed );
        }

        [TestCase]
        public void TryParse_8()
        {
           Amount a;
           var parsed = Amount.TryParse("aaa:bbb", out a);
           Assert.IsFalse( parsed );
        }

        [TestCase]
        public void TryParse_9()
        {
           Amount a;
           var parsed = Amount.TryParse("-1123 :gbp", out a);
           Assert.IsTrue( parsed );
           Assert.AreEqual( new Amount("gbp", -1123M), a);
        }

        [TestCase]
        public void TryParse_10()
        {
           Amount a;
           var parsed = Amount.TryParse("-1123 :  uah", out a);
           Assert.IsTrue( parsed );
           Assert.AreEqual( new Amount("UAH", -1123M), a);
        }


    }



}