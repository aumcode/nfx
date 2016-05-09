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

using NFX.Environment;

namespace NFX.NUnit.Config
{
    [TestFixture]
    public class OverridesAndMerges
    {

static string xml1 = @"
 <root>
    
    <section-a>
        <sub1>Sub Value 1</sub1>
        <sub2>Sub Value 2</sub2>
        <sub3>Sub Value 3</sub3>
        <destination name='A' type='CSVFile'> </destination>
        <destination name='B' type='SMTPMail'> </destination>
    </section-a>

    <section-b _override='attributes' age='32'>  
    
    </section-b>

    <section-c _override='fail'>
       This can not be overridden and exception will be thrown
    </section-c>

    <section-d _override='stop'>This can not be overridden and no exception will be thrown</section-d>

    <section-e _override='replace' some-attr='123'>  
       <a> </a>
       <b> </b>
    </section-e>

    <section-f _override='sections' some-attr='423'>  
       <a> </a>
       <b> </b>
    </section-f>

 </root>
";


static string xml2 = @"
 <root>
    
    <section-a newattr='new val'>
        <sub2>Sub Value 2 ammended</sub2>
        <sub3>Sub Value 3</sub3>
        <destination name='B' type='Clock'> </destination>
    </section-a>

    <section-b _override='attributes' age='89' pension='true'>  
    
    </section-b>

    <section-d all='yes'>
       This will be ignored
    </section-d>

    <section-e _override='replace' some-attr='992'>  
       <demo>Demo!</demo>
    </section-e>

    <section-f _override='sections' some-attr='324'>  
       <_clear/>
       <bim good='yes'> </bim>
    </section-f>

 </root>
";

static string xml3 = @"
 <root>
    <section-c>  
       Will cause failure if merged with XML1
    </section-c>
 </root>
";
        [TestCase]
        public void BasicMerge()
        {
          var conf1 = NFX.Environment.XMLConfiguration.CreateFromXML(xml1);
          var conf2 = NFX.Environment.XMLConfiguration.CreateFromXML(xml2);
          
          var conf = new NFX.Environment.MemoryConfiguration();
          conf.CreateFromMerge(conf1.Root, conf2.Root);
                                                              

          Assert.AreEqual("Sub Value 1", conf.Root["section-a"]["sub1"].Value);
          Assert.AreEqual("Sub Value 2 ammended", conf.Root["section-a"]["sub2"].Value);
          Assert.AreEqual("Sub Value 3", conf.Root["section-a"]["sub3"].Value);

          Assert.AreEqual("CSVFile", conf.Root["section-a"].Children.FirstOrDefault(n=>n.IsSameName("destination") && n.AttrByName("name").Value=="A").AttrByName("type").Value);
          Assert.AreEqual("Clock", conf.Root["section-a"].Children.FirstOrDefault(n=>n.IsSameName("destination") && n.AttrByName("name").Value=="B").AttrByName("type").Value);
          Assert.AreEqual("SMTPMail", conf1.Root["section-a"].Children.FirstOrDefault(n=>n.IsSameName("destination") && n.AttrByName("name").Value=="B").AttrByName("type").Value);

        }

        [TestCase]
        [ExpectedException(typeof(ConfigException))]
        public void ExpectOverrideException()
        {
          var conf1 = NFX.Environment.XMLConfiguration.CreateFromXML(xml1);
          var conf2 = NFX.Environment.XMLConfiguration.CreateFromXML(xml3);
          
          var conf = new NFX.Environment.MemoryConfiguration();
          
          try
          {
            conf.CreateFromMerge(conf1.Root, conf2.Root);
          }
          catch(Exception error)
          {
            Console.WriteLine("Expected and got: "+error.Message);
            throw error;
          }

        }


        [TestCase]
        public void MergeStop()
        {
          var conf1 = NFX.Environment.XMLConfiguration.CreateFromXML(xml1);
          var conf2 = NFX.Environment.XMLConfiguration.CreateFromXML(xml2);
          
          var conf = new NFX.Environment.MemoryConfiguration();
          conf.CreateFromMerge(conf1.Root, conf2.Root);
                                                              

          Assert.AreEqual("This can not be overridden and no exception will be thrown", conf.Root["section-d"].Value);
          Assert.AreEqual(1, conf.Root["section-d"].Attributes.Count());
          Assert.AreEqual(OverrideSpec.Stop, NodeOverrideRules.Default.StringToOverrideSpec(conf.Root["section-d"].AttrByName("_override").Value));
        }

        [TestCase]
        public void Performance()
        {
          const int CNT = 10000;

          var conf1 = NFX.Environment.XMLConfiguration.CreateFromXML(largexml1);
          var conf2 = NFX.Environment.XMLConfiguration.CreateFromXML(largexml2);

          var clock = System.Diagnostics.Stopwatch.StartNew();
          for(var i=0; i<CNT; i++)
          {
            var conf = new NFX.Environment.MemoryConfiguration();
            conf.CreateFromMerge(conf1.Root, conf2.Root);
          } 
          clock.Stop();

          Console.WriteLine("Config merge performance. Merged {0} times in {1} ms", CNT, clock.ElapsedMilliseconds);

        }



static string largexml1 = @"
 <r>
    <a>  
       <a-a a1='1' a2='2' a3='3' a4='4' a5='5'>
       </a-a>
    </a>

    <b>  
       <a-a>
       </a-a>
    </b>

    <c _override='all'>  
       <providers when='now'>
          <provider name='1' a1='1' a2='2' a3='3' a4='4' a5='5'>

          </provider>
          <provider name='2' a1='1' a2='2' a3='3' a4='4' a5='5'>

          </provider>
          <provider name='3' a1='1' a2='2' a3='3' a4='4' a5='5'>
                  <M> 
                    <K a1='1' a2='2' a3='3' a4='4' a5='5' 
                              b1='1' b2='2' b3='3' b4='4' b5='5'
                              c1='1' c2='2' c3='3' c4='4' c5='5'
                              >
                     
                    </K>
                  </M>
          </provider>
          <provider name='4' a1='1' a2='2' a3='3' a4='4' a5='5'>

          </provider>
          <provider name='5' a1='1' a2='2' a3='3' a4='4' a5='5'>

          </provider>
       </providers>
    </c>

    <d>  
       <a-a a1='$(/c/providers/$when)' a2='2' a3='3' a4='4' a5='5' 
                              b1='1' b2='2' b3='3' b4='4' b5='5'
                              c1='1' c2='2' c3='3' c4='4' c5='5'>
       </a-a>
    </d>

    <e>  
       <a-a a1='$(/c/providers/$when)' a2='2' a3='3' a4='4' a5='5' 
                              b1='1' b2='2' b3='3' b4='4' b5='5'
                              c1='1' c2='2' c3='3' c4='4' c5='5'>
       </a-a>
    </e>


    <f>  
       <a-a a1='$(/c/providers/$when)' a2='2' a3='3' a4='4' a5='5' 
                              b1='1' b2='2' b3='3' b4='4' b5='5'
                              c1='1' c2='2' c3='3' c4='4' c5='5'>
       </a-a>
    </f>

 </r>
";

static string largexml2 = @"
 <r>    
    <a>  
       <a-a za1='1' za2='2' za3='3' za4='4' za5='5'>
       </a-a>
    </a>

    <b>  
       <a-a>
       </a-a>
    </b>

    <c>  
       <providers when='now'>
          <provider name='1' za1='1' za2='2' a3='3' a4='4' a5='5'>

          </provider>
          <provider name='2' a1='lkhsfd; adshf; lsahf ha f' za2='2' a3='3' a4='sadfsdafn.asn' a5='qwerwqer5'>

          </provider>
          <provider name='tri' a1='1' a2='2' a3='3' a4='4' a5='5'>
                  <M a='jrtglkdjfglk jdflkjg djsg jd;flkjg ;dlsfjg ;ldsfjg ldkgjd;lfgl;k jwerojgoirjg wj;erigj ;ewrigj ;weirjg ;wiergiwe;rj; ow; gjw rgjjgiuefu'> 
                    <K a1='1' a2='2' a3='3' a4='4' a5='5' 
                              b1='1' b2='2' >

                              hahaha!
                     
                    </K>
                  </M>
          </provider>
          <provider name='aza' za1='1' za2='2' za3='3' za4='4' za5='5'>

          </provider>
          <provider name='zasaz' za1='1' za2='2' za3='3' za4='4' za5='5'>

          </provider>
       </providers>
    </c>

    <d>  
       <a-a a1='$(/c/providers/$when)' a2='23' a3='435343' a4='ertert4' a5='ert5' 
                              b1='ert1' b2='ewr2' b3='uiuyi3' b4='rettwe4' b5='ewrter5'
                              c1='1ert' c2='wer2' c3='ewrt3' c4='ert4' c5='erterter5'>
       </a-a>
    </d>

    <g>  
       <mail name='$(/c/providers/$when)' a2='2' a3='3' a4='4' a5='5'>
       </mail>

       <mail name='2' a2='2' a3='3' a4='4' a5='5'>
       </mail>

       <mail name='3' a2='2' a3='3' a4='4' a5='5'>

           <demo this='will have garbage nodes'>
              <sun bright='oh yeah' animals='various'>
                <gun pattern='alalal' flexible='true' retriever='233'>
                   Some text that may take multiple lines that the way
                   we wanted to put this text in here 
                </gun>
              </sun>
           </demo>

       </mail>

       <mail name='4' a2='2' a3='3' a4='4' a5='5'>
       </mail>


       <mail name='5' a2='2' a3='3' a4='4' a5='5'>
          <demo this='will have garbage nodes'>
              <sun bright='oh yeah' animals='various'>
                <gun pattern='alalal' flexible='true' retriever='233'>
                   Some text that may take multiple lines that the way
                   we wanted to put this text in here 
                </gun>
              </sun>
           </demo>
           <demoA this='will have garbage nodes'>
              <sun bright='oh yeah' animals='various'>
                <gun pattern='alalal' flexible='true' retriever='233'>
                   Some text that may take multiple lines that the way
                   we wanted to put this text in here 
                    <demoUnderGun this='will have garbage nodes'>
                      <sun bright='oh yeah' animals='various'>
                        <gun pattern='alalal' flexible='true' retriever='233'>
                           Some text that may take multiple lines that the way
                           we wanted to put this text in here 
                        </gun>

                           <demoUnderSun this='will have garbage nodes'>
                              <sun bright='oh yeah' animals='various'>
                                <gun pattern='alalal' flexible='true' retriever='233'>
                                   Some text that may take multiple lines that the way
                                   we wanted to put this text in here 
                                </gun>
                              </sun>
                           </demoUnderSun> 

                      </sun>
                   </demoUnderGun>

                </gun>
              </sun>
           </demoA>
           <demoB this='will have garbage nodes'>
              <sun bright='oh yeah' animals='various'>
                <gun pattern='alalal' flexible='true' retriever='233'>
                   Some text that may take multiple lines that the way
                   we wanted to put this text in here 
                </gun>
              </sun>
           </demoB>
       </mail>
    </g>

 </r>
";
        [Test]
        public void SectionMerge_1()
        {
          var conf1 = "r{ a{} c{}}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);
          var conf2 = "r{ b{ z = 134 }}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

          conf1.OverrideBy(conf2);

          Assert.AreEqual(3, conf1.ChildCount);
          Assert.IsTrue( conf1.Navigate("/a").Exists);
          Assert.IsTrue( conf1.Navigate("/b").Exists);
          Assert.IsTrue( conf1.Navigate("/c").Exists);
          Assert.IsTrue( conf1.Navigate("/b/$z").Exists);
          Assert.AreEqual( 134,  conf1.Navigate("/b/$z").ValueAsInt());
        }

        [Test]
        public void SectionMerge_2()
        {
          var conf1 = "r{ a{} c{}}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);
          var conf2 = "r{ b{ z = 134 } c{ y=456} c{z=789 }}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

          conf1.OverrideBy(conf2);

          Assert.AreEqual(3, conf1.ChildCount);//<---3 because all "C" get collapsed into one
          Assert.IsTrue( conf1.Navigate("/a").Exists);
          Assert.IsTrue( conf1.Navigate("/b").Exists);
          Assert.IsTrue( conf1.Navigate("/c").Exists);
          Assert.IsTrue( conf1.Navigate("/b/$z").Exists);
          Assert.AreEqual( 134,  conf1.Navigate("/b/$z").ValueAsInt());

          Assert.IsTrue( conf1.Navigate("/c/$y").Exists);
          Assert.IsTrue( conf1.Navigate("/c/$z").Exists);

          Assert.AreEqual( 456,  conf1.Navigate("/c/$y").ValueAsInt());
          Assert.AreEqual( 789,  conf1.Navigate("/c/$z").ValueAsInt());
        }

        [Test]
        public void SectionMerge_3()
        {
          var conf1 = "r{ a{} c{}}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);
          var conf2 = "r{ b{ z = 134 } c{ y=456} c{z=789 }}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

          var rules = new NodeOverrideRules{ AppendSectionsWithoutMatchAttr = true };
          conf1.OverrideBy(conf2, rules);

          Assert.AreEqual(5, conf1.ChildCount);//<---- 5 because all "C" get appended
          Assert.IsTrue( conf1.Navigate("/a").Exists);
          Assert.IsTrue( conf1.Navigate("/b").Exists);
          Assert.IsTrue( conf1.Navigate("/c").Exists);
          Assert.IsTrue( conf1.Navigate("/b/$z").Exists);
          Assert.AreEqual( 134,  conf1.Navigate("/b/$z").ValueAsInt());

          Assert.IsTrue( conf1.Navigate("/c[y=456]").Exists);
          Assert.IsTrue( conf1.Navigate("/c[z=789]").Exists);

          Assert.AreEqual( 456,  conf1.Navigate("/c[y=456]/$y").ValueAsInt());
          Assert.AreEqual( 789,  conf1.Navigate("/c[z=789]/$z").ValueAsInt());
        }

        [Test]
        public void SectionMerge_4()
        {
          var conf1 = "r{ a{} c{}}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);
          var conf2 = "r{ b{ z = 134 } c{name='id1' y=456} c{name='id2' z=789 }}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

          conf1.OverrideBy(conf2);

          Assert.AreEqual(5, conf1.ChildCount); //<-- 5 because names are different
          Assert.IsTrue( conf1.Navigate("/a").Exists);
          Assert.IsTrue( conf1.Navigate("/b").Exists);
          Assert.IsTrue( conf1.Navigate("/c").Exists);
          Assert.IsTrue( conf1.Navigate("/c[name=id1]").Exists);
          Assert.IsTrue( conf1.Navigate("/c[name=id2]").Exists);
          Assert.IsTrue( conf1.Navigate("/b/$z").Exists);
          Assert.AreEqual( 134,  conf1.Navigate("/b/$z").ValueAsInt());

          Assert.IsTrue( conf1.Navigate("/c[name=id1]/$y").Exists);
          Assert.IsTrue( conf1.Navigate("/c[name=id2]/$z").Exists);

          Assert.AreEqual( 456,  conf1.Navigate("/c[name=id1]/$y").ValueAsInt());
          Assert.AreEqual( 789,  conf1.Navigate("/c[name=id2]/$z").ValueAsInt());
        }

        [Test]
        public void SectionMerge_5()
        {
          var conf1 = "r{ a{} c{}}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);
          var conf2 = "r{ b{ z = 134 } c{name='id1' y=456} c{name='id2' z=789 } c{ gg=123}}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

          conf1.OverrideBy(conf2);

          Assert.AreEqual(5, conf1.ChildCount); //<-- 5 because names are different, but 6th collapses into the one without name
          Assert.IsTrue( conf1.Navigate("/a").Exists);
          Assert.IsTrue( conf1.Navigate("/b").Exists);
          Assert.IsTrue( conf1.Navigate("/c").Exists);
          Assert.IsTrue( conf1.Navigate("/c[name=id1]").Exists);
          Assert.IsTrue( conf1.Navigate("/c[name=id2]").Exists);
          Assert.IsTrue( conf1.Navigate("/b/$z").Exists);
          Assert.AreEqual( 134,  conf1.Navigate("/b/$z").ValueAsInt());

          Assert.IsTrue( conf1.Navigate("/c/$gg").Exists);
          Assert.IsTrue( conf1.Navigate("/c[name=id1]/$y").Exists);
          Assert.IsTrue( conf1.Navigate("/c[name=id2]/$z").Exists);

          Assert.AreEqual( 123,  conf1.Navigate("/c/$gg").ValueAsInt());
          Assert.AreEqual( 456,  conf1.Navigate("/c[name=id1]/$y").ValueAsInt());
          Assert.AreEqual( 789,  conf1.Navigate("/c[name=id2]/$z").ValueAsInt());
        }

        [Test]
        public void SectionMerge_6()
        {
          var conf1 = "r{ a{} c{}}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);
          var conf2 = "r{ b{ z = 134 } c{name='id1' y=456} c{name='id2' z=789 } c{ gg=123}}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

          var rules = new NodeOverrideRules{ AppendSectionsWithoutMatchAttr = true };
          conf1.OverrideBy(conf2, rules);

          Assert.AreEqual(6, conf1.ChildCount); //<-- 6 because names are different, but 6th gets added due to rules
          Assert.IsTrue( conf1.Navigate("/a").Exists);
          Assert.IsTrue( conf1.Navigate("/b").Exists);
          Assert.IsTrue( conf1.Navigate("/c").Exists);
          Assert.IsTrue( conf1.Navigate("/c[name=id1]").Exists);
          Assert.IsTrue( conf1.Navigate("/c[name=id2]").Exists);
          Assert.IsTrue( conf1.Navigate("/c[gg=123]").Exists);
          Assert.IsTrue( conf1.Navigate("/b/$z").Exists);
          Assert.AreEqual( 134,  conf1.Navigate("/b/$z").ValueAsInt());

          Assert.IsTrue( conf1.Navigate("/c[gg=123]/$gg").Exists);
          Assert.IsTrue( conf1.Navigate("/c[name=id1]/$y").Exists);
          Assert.IsTrue( conf1.Navigate("/c[name=id2]/$z").Exists);

          Assert.AreEqual( 123,  conf1.Navigate("/c[gg=123]/$gg").ValueAsInt());
          Assert.AreEqual( 456,  conf1.Navigate("/c[name=id1]/$y").ValueAsInt());
          Assert.AreEqual( 789,  conf1.Navigate("/c[name=id2]/$z").ValueAsInt());
        }


    }
}
