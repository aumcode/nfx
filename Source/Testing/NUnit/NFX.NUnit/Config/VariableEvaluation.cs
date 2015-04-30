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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;


using NFX.Environment;

namespace NFX.NUnit.Config
{

    [TestFixture]   
    public class VariableEvaluation
    {
        static string xml = 
@"<root>
   
     <varEscaped>$(###)This is not var: $(../AAA)</varEscaped>
     <varIncomplete1>$()</varIncomplete1>
     <varIncomplete2>$(</varIncomplete2>
   
   <vars>
     
     <var1>val1</var1>
     <var2>$(../var1)</var2>

     <path1 value='c:\logs\' />
     <path2 value='\critical' />

     <many>
        <a value='1' age='18'>1</a>
        <a value='2' age='25'>2</a>
     </many>

     <var3>$(../var4)</var3>
     <var4>$(../var3)</var4>

     <var5>$(../var6)</var5>
     <var6>$(../var7)</var6>
     <var7>$(../var1)$(../var3)$(../var2)</var7>
   </vars>
   
   
   <MyClass> 
    <data pvt-name='private'
          prot-name='protected'
          pub-name='public'
          age='99'>

          <extra
            enum='B'
            when='05/12/1982'
            cycle='$(/vars/var5)'
            >

            <fuzzy>true</fuzzy>
            <jazzy></jazzy>
          
          </extra>
    </data>
  </MyClass>

  <this name='$(/vars/var1)' text='This happened on $(../MyClass/data/extra/$when) date' />

  <logger location='$(/vars/path1/$value)$(@/vars/path2/$value)'/>

  <optional>$(/non-existent)</optional>
  <required>$(!/non-existent)</required>

  <env1>$(~A)</env1>
  <env2>$(~A)+$(~B)</env2>
  <env3>$(~A)$(@~B)</env3>


 </root>
";

       
        [TestCase]
        public void EscapedVar()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Assert.AreEqual("This is not var: $(../AAA)", conf.Root.Navigate("/varEscaped").Value );
        }

        [TestCase]
        public void IncompleteVars()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Assert.AreEqual("", conf.Root.Navigate("/varIncomplete1").Value );
          Assert.AreEqual("$(", conf.Root.Navigate("/varIncomplete2").Value );
        }


        [ExpectedException(typeof(ConfigException), ExpectedMessage="not a section node", MatchType=MessageMatch.Contains)]
        [TestCase]
        public void BadPathWithAttrAttr()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Assert.IsFalse( conf.Root.Navigate("/vars/path1/$value/$kaka").Exists );
        }


        [TestCase]
        public void PathWithPipes()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Assert.AreEqual("\\critical", conf.Root.Navigate("/vars/paZZ1/$value|/vars/path2/$value").Value);
        }

        [TestCase]
        public void PathWithSectionIndexer()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Assert.AreEqual("\\critical", conf.Root.Navigate("/vars/[3]/$value").Value);
        }

        [TestCase]
        public void PathWithAttributeIndexer()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Assert.AreEqual("\\critical", conf.Root.Navigate("/vars/path2/$[0]").Value);
        }


        [TestCase]
        public void PathWithValueIndexer()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Assert.AreEqual(1, conf.Root.Navigate("/vars/many/a[1]").ValueAsInt());
          Assert.AreEqual(2, conf.Root.Navigate("/vars/many/a[2]").ValueAsInt());
        }

        [TestCase]
        public void PathWithAttributeValueIndexer()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Assert.AreEqual(1, conf.Root.Navigate("/vars/many/a[age=18]").ValueAsInt());
          Assert.AreEqual(2, conf.Root.Navigate("/vars/many/a[age=25]").ValueAsInt());
        }

            

        [ExpectedException(typeof(ConfigException), ExpectedMessage="syntax", MatchType=MessageMatch.Contains)]
        [TestCase]
        public void PathWithBadIndexerSyntax1()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          conf.Root.Navigate("/]/$value");
        }

        [ExpectedException(typeof(ConfigException), ExpectedMessage="syntax", MatchType=MessageMatch.Contains)]
        [TestCase]
        public void PathWithBadIndexerSyntax2()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          conf.Root.Navigate("/aaa]/$value");
        }


      
        [ExpectedException(typeof(ConfigException), ExpectedMessage="syntax", MatchType=MessageMatch.Contains)]
        [TestCase]
        public void PathWithBadIndexerSyntax3()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          conf.Root.Navigate("/[/$value");
        }

       
        
        
        [TestCase]
        public void TestNavigationinVarNames()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);
                                                              

          Assert.AreEqual("val1", conf.Root["vars"]["var1"].Value);
          Assert.AreEqual("val1", conf.Root["vars"]["var1"].VerbatimValue);

          Assert.AreEqual("val1", conf.Root["vars"]["var2"].Value);
          Assert.AreEqual("$(../var1)", conf.Root["vars"]["var2"].VerbatimValue);


          Assert.AreEqual("val1", conf.Root["this"].AttrByName("name").Value);
          Assert.AreEqual("$(/vars/var1)", conf.Root["this"].AttrByName("name").VerbatimValue);
          Assert.AreEqual("$(/vars/var1)", conf.Root["this"].AttrByName("name").ValueAsString(verbatim: true));

          Assert.AreEqual("This happened on 05/12/1982 date", conf.Root["this"].AttrByName("text").Value);

          Assert.AreEqual(@"c:\logs\critical", conf.Root["logger"].AttrByName("location").Value);

        }

        [TestCase]
        [ExpectedException(typeof(ConfigException))]
        public void Recursive1()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Assert.AreEqual("$(../var4)", conf.Root["vars"]["var3"].VerbatimValue);//no exception
          var v = conf.Root["vars"]["var3"].Value;
        }

        [TestCase]
        [ExpectedException(typeof(ConfigException))]
        public void Recursive2()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Assert.AreEqual("$(../var3)", conf.Root["vars"]["var4"].VerbatimValue);//no exception
          var v = conf.Root["vars"]["var4"].Value;
        }

        [TestCase]
        [ExpectedException(typeof(ConfigException))]
        public void Recursive3Transitive()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          var attr = conf.Root["MyClass"]["data"]["extra"].AttrByName("cycle");

          var v1 = attr.VerbatimValue;//no exception
          var v2 = attr.Value;//exception
        }


        [TestCase]
        public void Recursive4StackCleanup()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          var attr = conf.Root["MyClass"]["data"]["extra"].AttrByName("cycle");

          try
          {
           var v2 = attr.Value;//exception
          }
          catch(Exception error)
          {
           Console.WriteLine("Expected and got: "+error.Message);
          }

          //after exception, stack should cleanup and work again as expected
          Assert.AreEqual("val1", conf.Root["vars"]["var1"].Value);
        }


        [TestCase]
        public void Optional()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Assert.AreEqual(true, string.IsNullOrEmpty(conf.Root["optional"].Value));
        }

        [TestCase]
        [ExpectedException(typeof(ConfigException))]
        public void Required()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          var v = conf.Root["required"].Value;
        }



        [TestCase]
        public void EnvVars1()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);
          conf.EnvironmentVarResolver = new MyVars();

           Assert.AreEqual("1", conf.Root["env1"].Value);
           Assert.AreEqual("1+2", conf.Root["env2"].Value);
           Assert.AreEqual(@"1\2", conf.Root["env3"].Value);

        }


        [TestCase]
        public void EvalFromString_1()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);
          
           Assert.AreEqual("Hello val1", "Hello $(vars/var1)".EvaluateVarsInConfigScope(conf));
           Assert.AreEqual("Hello val1", "Hello $(vars/var1)".EvaluateVarsInXMLConfigScope(xml));
           Assert.AreEqual("Hello 123 suslik!", "Hello $(/$v) suslik!".EvaluateVarsInXMLConfigScope("<a v='123'> </a>"));
        }

        [TestCase]
        public void EvalFromString_2_manysame()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);
          
           Assert.AreEqual("Hello val1val1val1", "Hello $(vars/var1)$(vars/var1)$(vars/var1)".EvaluateVarsInConfigScope(conf));
        }



        [TestCase]
        public void EvalFromStringWithEnvVarResolver()
        {
           Assert.AreEqual("Time is 01/18/1901 2:03PM", "Time is $(~C)".EvaluateVars( new MyVars()));
        }


        [TestCase]
        public void EvalFromStringWithEnvVarInline()
        {
           Assert.AreEqual("Hello, your age is 123", "$(~GreEtInG), your age is $(~AGE)".EvaluateVars( new Vars{ 
                            {"Greeting", "Hello"}, 
                            {"Age", "123"}}));
        }


        [TestCase]
        public void EvalFromStringWithEnvVarAndMacro()
        {
           Assert.AreEqual("Time is 01/1901", "Time is $(~C::as-dateTime fmt=\"{0:MM/yyyy}\")".EvaluateVars( new MyVars()));
        }

        [TestCase]
        public void EvalFromStringWithEnvVarAndMacro2()
        {
           Assert.AreEqual("Time is Month=01 Year=1901", "Time is $(~C::as-dateTime fmt=\"Month={0:MM} Year={0:yyyy}\")".EvaluateVars( new MyVars()));
        }


        [TestCase]
        public void EvalFromStringMacroDefault()
        {
           Assert.AreEqual("Value is 12 OK?", "Value is $(/dont-exist::as-int dflt=\"12\") OK?".EvaluateVars());
        }

        [TestCase]
        public void EvalFromStringMacroDefault2()
        {
           Assert.AreEqual("James, the value is 12 OK?", 
                           "$(/$name::as-string dflt=\"James\"), the value is $(/dont-exist::as-int dflt=\"12\") OK?".EvaluateVars());
        }

        [TestCase]
        public void EvalFromStringMacroDefault3()
        {
           Assert.AreEqual("Mark Spenser, the value is 12 OK?", 
                           "$(~name::as-string dflt=\"James\"), the value is $(/dont-exist::as-int dflt=\"12\") OK?".EvaluateVars(
                            new Vars { {"name", "Mark Spenser"}  }
                           ));
        }

        [TestCase]
        public void EvalTestNowString()
        {
            Assert.AreEqual("20131012-06", "$(::now fmt=yyyyMMdd-HH value=20131012-06)".EvaluateVars());
        }

        [TestCase]
        public void NodePaths()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);
                                                              

          Assert.AreEqual("/vars/path2", conf.Root["vars"]["path2"].RootPath);
          Assert.AreEqual("/vars/path2/$value", conf.Root["vars"]["path2"].AttrByIndex(0).RootPath);
          Assert.AreEqual("/vars/many/[0]", conf.Root["vars"]["many"][0].RootPath);
          Assert.AreEqual("/vars/many/[1]", conf.Root["vars"]["many"][1].RootPath);
          Assert.AreEqual("/vars/many/[1]/$value", conf.Root["vars"]["many"][1].AttrByIndex(0).RootPath);
        }


        [TestCase]
        public void NavigateBackToNodePaths()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);
                                                              
          Assert.AreEqual("/vars/path2",           conf.Root.Navigate( conf.Root["vars"]["path2"].RootPath                  ).RootPath);
          Assert.AreEqual("/vars/path2/$value",    conf.Root.Navigate( conf.Root["vars"]["path2"].AttrByIndex(0).RootPath   ).RootPath);
          Assert.AreEqual("/vars/many/[0]",        conf.Root.Navigate( conf.Root["vars"]["many"][0].RootPath                ).RootPath);
          Assert.AreEqual("/vars/many/[1]",        conf.Root.Navigate( conf.Root["vars"]["many"][1].RootPath                ).RootPath);
          Assert.AreEqual("/vars/many/[1]/$value", conf.Root.Navigate( conf.Root["vars"]["many"][1].AttrByIndex(0).RootPath ).RootPath);
        }


    }



                        class MyVars : IEnvironmentVariableResolver
                        {

                            public string ResolveEnvironmentVariable(string name)
                            {
                                if (name=="A") return "1";
                                if (name=="B") return "2";
                                if (name=="C") return "01/18/1901 2:03PM";
                                return null;
                            }
                        }

                       


}



