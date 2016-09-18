/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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


using NFX.Time;
using NFX.Environment;
using System.Threading;
using System.Globalization;

namespace NFX.NUnit.Config
{
    #pragma warning disable 0649,0169                    



        public enum MyEnum {A,B,C,D}

        [Config("MyClass/data")]
        public class MyClass
        {
           
           [Config("$pvt-int")]
           private int m_PrivateInt;         public int getPrivateInt(){ return m_PrivateInt;}
           
           
           private int m_privateProperty;
           
           [Config("$pvt-property")]
           private int privateProperty
           {
             get { return m_privateProperty;}
             set { m_privateProperty = value; }
           }   public int getPrivateProperty(){ return privateProperty; }


           
           [Config("$pvt-name")]
           private string m_PrivateName;         public string getPrivateName(){ return m_PrivateName;}
           
           [Config("$prot-name")]
           protected string m_ProtectedName;     public string getProtectedName(){ return m_ProtectedName;}
           
           [Config("$pub-name")]
           public string m_PublicName;

           [Config("$pub-format|$pub-def-format")]
           public string Format;

           [Config("$age")]
           public int Age;

           [Config]
           public int Age2;

           [Config]
           public int TheNewAge;

           [Config("extra/$enum")]
           public MyEnum MyEnumField;

           [Config("extra/$when")]
           public DateTime When{ get; set; }
           
           [Config("extra/fuzzy")]
           public bool? Fuzzy{ get; set; }

           [Config("extra/jazzy")]
           public bool? Jazzy{ get; set; }


           [Config("extra/$none1", 155)]
           public int NoneInt{ get; set; }

           [Config("extra/$none2", true)]
           public bool NoneBool{ get; set; }

           [Config("extra/$none3", "This is default")]
           public string NoneString{ get; set; }


           [Config("extra/options")]
           public IConfigSectionNode OptionsProp { get; set;}

           [Config("extra/options")]
           public IConfigSectionNode OptionsField;

           [Config]
           public byte[] Bytes;
        }


        public class MyBadClass : MyClass
        {
           [Config("$pub-format|!$NON-EXISTENT-format")]
           public string FormatThatIsRequired;
        }
       
        public class MyClassExtended : MyClass
        {
           
           [Config("$pvt-int-extended")]
           private int m_PrivateInt;         public int getPrivateIntExtended(){ return m_PrivateInt;}
           
           
           private int m_privateProperty;
           
           [Config("$pvt-property-extended")]
           private int privateProperty
           {
             get { return m_privateProperty;}
             set { m_privateProperty = value; }
           }   public int getPrivatePropertyExtended(){ return privateProperty; }
           
           
           //notice no attributes on this level, they will get inherited here
           [Config("abrakabadra", "So what?")]
           public string NoneAnotherString{ get; set; }
        }
       
        [Config("MyClassExtended2/data")]
        public class MyClassExtended2 : MyClass
        {
         //notice no attributes on this level, they will get inherited here
        }



                        [ConfigMacroContext]
                        public class SomeFatClassWithContext : ILocalizedTimeProvider, IConfigurable
                        {

                            public SomeFatClassWithContext()
                            {

                            }

                            [Config]
                            public string Text; 


                            public TimeLocation TimeLocation
                            {
                                get { return new TimeLocation(new TimeSpan(2, 15, 0), "In Europe"); }
                            }

                            public DateTime LocalizedTime
                            {
                                get { return new DateTime(2000, 01, 02, 18, 30, 00); }
                            }

                            public DateTime UniversalTimeToLocalizedTime(DateTime utc)
                            {
                                return new DateTime(2000, 01, 02, 16, 15, 00);
                            }

                            public DateTime LocalizedTimeToUniversalTime(DateTime local)
                            {
                              throw new NotImplementedException();
                            }


                            public void Configure(IConfigSectionNode node)
                            {
                                Text = node.AttrByName("text").Value;
                            }

                                                        
                        }




    [TestFixture]   
    public class Attributes
    {
        static string xml = 
@"<root>

   <injected type='NFX.NUnit.Config.SomeFatClassWithContext, NFX.NUnit' 
             text='$(::now fmt=yyyyMMdd-HHmmss)'
   />



   <MyClass> 
    <data pvt-name='private'
          prot-name='protected'
          pub-name='public'
          pub-def-format='xxx'
          age='99'
          age2='890'
          the-new-age='1890'
          bytes='0,1 ,2 ,3 ,  4,5,6,7,8,9,a,b,c,d,e,f'
          
          pvt-int='-892'
          pvt-property='23567'
          
          
          pvt-int-extended='892'
          pvt-property-extended='-23567'
          
          >

          <extra
            enum='B'
            when='05/12/1982'>

            <fuzzy>true</fuzzy>
            <jazzy></jazzy>
          
            <options>
                <hello a='1'>YES!</hello>
            </options>

          </extra>


    </data>
  </MyClass>

  <MyClassExtended2> 
    <data prot-name='protected'
          pub-name='public'
          age='199'>

          <extra
            enum='C'
            when='01/1/1944'>

            <fuzzy>false</fuzzy>
            <jazzy></jazzy>
          
          </extra>


    </data>
  </MyClassExtended2>
 </root>
";
        
        
        
        [TestCase]
        public void ConfigAttributeApply()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          var cl = new MyClass();
          ConfigAttribute.Apply(cl, conf.Root);

          Assert.AreEqual("private", cl.getPrivateName());
          Assert.AreEqual("protected", cl.getProtectedName());
          Assert.AreEqual("public", cl.m_PublicName);


           Assert.AreEqual(-892, cl.getPrivateInt());
           Assert.AreEqual(23567, cl.getPrivateProperty());

          Assert.AreEqual("xxx", cl.Format);

          Assert.AreEqual(99, cl.Age);
          Assert.AreEqual(MyEnum.B, cl.MyEnumField);

          Assert.AreEqual(5, cl.When.Month);
          Assert.AreEqual(12, cl.When.Day);
          Assert.AreEqual(1982, cl.When.Year);

          Assert.AreEqual(true, cl.Fuzzy.Value);

          Assert.AreEqual(false, cl.Jazzy.HasValue);

           Assert.AreEqual(155, cl.NoneInt);
           Assert.AreEqual(true, cl.NoneBool);
           Assert.AreEqual("This is default", cl.NoneString);
        }

        [TestCase]
        public void ConfigAttributeWithoutPath()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          var cl = new MyClass();
          ConfigAttribute.Apply(cl, conf.Root);

          Assert.AreEqual(99, cl.Age);
          Assert.AreEqual(890, cl.Age2);
          Assert.AreEqual(1890, cl.TheNewAge);
        }

         [TestCase]
        public void ConfigAttributeByteArray()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          var cl = new MyClass();
          ConfigAttribute.Apply(cl, conf.Root);

          var bytes =  cl.Bytes;
          Assert.AreEqual(16, bytes.Length);

          Assert.AreEqual(0, bytes[00]);
          Assert.AreEqual(1, bytes[01]);
          Assert.AreEqual(2, bytes[02]);
          Assert.AreEqual(3, bytes[03]);
          Assert.AreEqual(4, bytes[04]);
          Assert.AreEqual(5, bytes[05]);
          Assert.AreEqual(6, bytes[06]);
          Assert.AreEqual(7, bytes[07]);
          Assert.AreEqual(8, bytes[08]);
          Assert.AreEqual(9, bytes[09]);
          Assert.AreEqual(0xa, bytes[10]);
          Assert.AreEqual(0xb, bytes[11]);
          Assert.AreEqual(0xc, bytes[12]);
          Assert.AreEqual(0xd, bytes[13]);
          Assert.AreEqual(0xe, bytes[14]);
          Assert.AreEqual(0xf, bytes[15]);
        }


        [TestCase]
        [ExpectedException(typeof(ConfigException))]
        public void ConfigAttributeApplyToNonExistingRequiredCoalescedAttribute()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          var cl = new MyBadClass();
          ConfigAttribute.Apply(cl, conf.Root);
        }



        [TestCase]
        public void ConfigAttributeApplyToICongifSectionPropertyAndField()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          var cl = new MyClass();
          ConfigAttribute.Apply(cl, conf.Root);

         
           Assert.AreEqual("YES!", cl.OptionsProp["hello"].Value);
           Assert.AreEqual(1, cl.OptionsProp["hello"].AttrByName("a").ValueAsInt());

           Assert.AreEqual("YES!", cl.OptionsField["hello"].Value);
           Assert.AreEqual(1, cl.OptionsField["hello"].AttrByName("a").ValueAsInt());
        }


        [TestCase]
        public void ConfigAttributeApplyToExtendedClass()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          var cl = new MyClassExtended();
          ConfigAttribute.Apply(cl, conf.Root);

          Assert.AreEqual("private", cl.getPrivateName());
          Assert.AreEqual("protected", cl.getProtectedName());
          Assert.AreEqual("public", cl.m_PublicName);


          Assert.AreEqual(-892, cl.getPrivateInt());
          Assert.AreEqual(23567, cl.getPrivateProperty());

          Assert.AreEqual(+892, cl.getPrivateIntExtended());
          Assert.AreEqual(-23567, cl.getPrivatePropertyExtended());


          Assert.AreEqual(99, cl.Age);
          Assert.AreEqual(MyEnum.B, cl.MyEnumField);

          Assert.AreEqual(5, cl.When.Month);
          Assert.AreEqual(12, cl.When.Day);
          Assert.AreEqual(1982, cl.When.Year);

          Assert.AreEqual(true, cl.Fuzzy.Value);

          Assert.AreEqual(false, cl.Jazzy.HasValue);

           Assert.AreEqual(155, cl.NoneInt);
           Assert.AreEqual(true, cl.NoneBool);
           Assert.AreEqual("This is default", cl.NoneString);
           Assert.AreEqual("So what?", cl.NoneAnotherString);

        }

        [TestCase]
        public void ConfigAttributeApplyToExtendedClassWithRootOverride()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          var cl = new MyClassExtended2();
          ConfigAttribute.Apply(cl, conf.Root);

          Assert.AreEqual("protected", cl.getProtectedName());
          Assert.AreEqual("public", cl.m_PublicName);

          Assert.AreEqual(199, cl.Age);
          Assert.AreEqual(MyEnum.C, cl.MyEnumField);

          Assert.AreEqual(1, cl.When.Month);
          Assert.AreEqual(1, cl.When.Day);
          Assert.AreEqual(1944, cl.When.Year);

          Assert.AreEqual(false, cl.Fuzzy.Value);

          Assert.AreEqual(false, cl.Jazzy.HasValue);


        }

        [TestCase]
        public void MakeWithMacroContextAndLocalizedTime()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          var node = conf.Root["injected"];
          var obj = FactoryUtils.Make<SomeFatClassWithContext>(node);
           
          ConfigAttribute.Apply(obj, node); 

          Assert.AreEqual("20000102-183000", obj.Text);
        }

        [TestCase]
        public void MakeAndConfigureWithMacroContextAndLocalizedTime()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          var node = conf.Root["injected"];
          var obj = FactoryUtils.MakeAndConfigure<SomeFatClassWithContext>(node);
           
          Assert.AreEqual("20000102-183000", obj.Text);
        }


    }


}



