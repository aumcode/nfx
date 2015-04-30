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
using System.Threading.Tasks;
using NUnit.Framework;

using NFX.Serialization.JSON;
using NFX.Wave;

namespace NFX.NUnit.Wave
{
    [TestFixture]
    public class URIPatternTests
    {
        [TestCase]
        public void T1()
        {
          var uri = new Uri("http://russia.ru/2012/sep/mayor-gets-elected?bonus=true");
          var pat = new URIPattern("{year}/{month}/{title}");
          
          var match = pat.MatchURIPath(uri);   
          Assert.IsNotNull(match);
          Assert.AreEqual("2012", match["year"]);
          Assert.AreEqual("sep", match["month"]);
          Assert.AreEqual("mayor-gets-elected", match["title"]);
        }

        [TestCase]
        public void T2()
        {
          var uri = new Uri("http://russia.ru/news/2012/sep/mayor-gets-elected?bonus=true");
          var pat = new URIPattern("{year}/{month}/{title}");
          
          var match = pat.MatchURIPath(uri);   
          Assert.IsNull(match);
        }

        [TestCase]
        public void T3()
        {
          var uri = new Uri("http://russia.ru/news/2012/sep/mayor-gets-elected?bonus=true");
          var pat = new URIPattern("news/{year}/{month}/{title}");
          
          var match = pat.MatchURIPath(uri);   
          Assert.IsNotNull(match);
          Assert.AreEqual("2012", match["year"]);
          Assert.AreEqual("sep", match["month"]);
          Assert.AreEqual("mayor-gets-elected", match["title"]);
        }

        [TestCase]
        public void T3_withLeadingSlash()
        {
          var uri = new Uri("http://russia.ru/news/2012/sep/mayor-gets-elected?bonus=true");
          var pat = new URIPattern("/news/{year}/{month}/{title}");
          
          var match = pat.MatchURIPath(uri);   
          Assert.IsNotNull(match);
          Assert.AreEqual("2012", match["year"]);
          Assert.AreEqual("sep", match["month"]);
          Assert.AreEqual("mayor-gets-elected", match["title"]);
        }

        [TestCase]
        public void T4_defaults()
        {
          var uri = new Uri("http://russia.ru/news/2012/sep/mayor-gets-elected?bonus=true");
          var pat = new URIPattern("news/{year}/{month}/{title=overview}");
          
          var match = pat.MatchURIPath(uri);   
          Assert.IsNotNull(match);
          Assert.AreEqual("2012", match["year"]);
          Assert.AreEqual("sep", match["month"]);
          Assert.AreEqual("mayor-gets-elected", match["title"]);
        }

        [TestCase]
        public void T5_defaults()
        {
          var uri = new Uri("http://russia.ru/news/2012/sep/");
          var pat = new URIPattern("news/{year}/{month}/{title=overview}");
          
          var match = pat.MatchURIPath(uri);   
          Assert.IsNotNull(match);
          Assert.AreEqual("2012", match["year"]);
          Assert.AreEqual("sep", match["month"]);
          Assert.AreEqual("overview", match["title"]);
        }

        [TestCase]
        public void T6()
        {
          var uri = new Uri("http://russia.ru/news/2012/sep/mayor-gets-elected?bonus=true");
          var pat = new URIPattern("news/{year}/{month}/{title}/presidential");
          
          var match = pat.MatchURIPath(uri);   
          Assert.IsNull(match);
        }

        [TestCase]
        public void T7()
        {
          var uri = new Uri("http://russia.ru/news/2012/sep/mayor-gets-elected/presidential?bonus=true");
          var pat = new URIPattern("news/{year}/{month}/{title}/presidential");
          
          var match = pat.MatchURIPath(uri);   
          Assert.IsNotNull(match);
          Assert.AreEqual("2012", match["year"]);
          Assert.AreEqual("sep", match["month"]);
          Assert.AreEqual("mayor-gets-elected", match["title"]);
        }


        [TestCase]
        public void T8()
        {
          var uri = new Uri("http://russia.ru/news/2012/sep/mayor-gets-elected/presidential?bonus=true");
          var pat = new URIPattern("news/{*path}");
          
          var match = pat.MatchURIPath(uri);   
          Assert.IsNotNull(match);
          Assert.AreEqual("2012/sep/mayor-gets-elected/presidential", match["path"]);
        }


        [TestCase]
        [ExpectedException(typeof(WaveException), ExpectedMessage="wildcard capture variable", MatchType=MessageMatch.Contains)]
        public void T9()
        {
          var uri = new Uri("http://russia.ru/news/2012/sep/mayor-gets-elected/presidential?bonus=true");
          var pat = new URIPattern("news/{*path}/cantbe");
        }


        [TestCase]
        public void T10()
        {
          var uri = new Uri("http://russia.ru/news/2012/sep/mayor%2egets%2eelected/");
          var pat = new URIPattern("news/{year}/{month}/{title}");
          
          var match = pat.MatchURIPath(uri);   
          Assert.IsNotNull(match);
          Assert.AreEqual("2012", match["year"]);
          Assert.AreEqual("sep", match["month"]);
          Assert.AreEqual("mayor.gets.elected", match["title"]);
        }

        [TestCase]
        public void T10_notrailingslash()
        {
          var uri = new Uri("http://russia.ru/news/2012/sep/mayor%2egets%2eelected");
          var pat = new URIPattern("news/{year}/{month}/{title}");
          
          var match = pat.MatchURIPath(uri);   
          Assert.IsNotNull(match);
          Assert.AreEqual("2012", match["year"]);
          Assert.AreEqual("sep", match["month"]);
          Assert.AreEqual("mayor.gets.elected", match["title"]);
        }

        [TestCase]
        public void T11_case_insensitive()
        {
          var uri = new Uri("http://russia.ru/news/2012/sep/mayor%2egets%2eelected/");
          var pat = new URIPattern("NEWS/{year}/{month}/{title}");
          
          var match = pat.MatchURIPath(uri);   
          Assert.IsNotNull(match);
          Assert.AreEqual("2012", match["year"]);
          Assert.AreEqual("sep", match["month"]);
          Assert.AreEqual("mayor.gets.elected", match["title"]);
        }

        [TestCase]
        public void T11_case_sensitive()
        {
          var uri = new Uri("http://russia.ru/news/2012/sep/mayor%2egets%2eelected/");
          var pat = new URIPattern("NEWS/{year}/{month}/{title}");
          
          var match = pat.MatchURIPath(uri, senseCase: true);   
          Assert.IsNull(match);
        }


    }
}
