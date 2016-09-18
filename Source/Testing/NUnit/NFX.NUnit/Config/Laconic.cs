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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using NFX.Environment;
using NFX.CodeAnalysis.Laconfig;

namespace NFX.NUnit.Config
{
    [TestFixture]   
    public class Laconic
    {

    static string src1 = 
@"
root
{
   vars
   {
     var1=val1{}
     var2=$(../var1){}

     path1{ value=c:\logs\ }
     path2{ value=\critical }

     many
     {
        a{ value=1}
        a{ value=2}
     }

     var3=$(../var4){}
     var4=$(../var3){}

     var5=$(../var6){}
     var6=$(../var7){}
     var7=$(../var1)$(../var3)$(../var2){}
   }
   
   
   MyClass
   { 
    data
    {  pvt-name='private'
       prot-name='protected'
       pub-name='public'
       age='99'

          extra
          {
            enum='B'
            when='05/12/1982'
            cycle='$(/vars/var5)'
           

            fuzzy=true{}
            jazzy=''{}
          
          }
    }//data
   }//MyClass

  this { name=$(/vars/var1) text='This happened on $(../MyClass/data/extra/$when) date'}

  logger{ location=$(/vars/path1/$value)$(@/vars/path2/$value)}

  optional=$(/non-existent){}
  required=$(!/non-existent){}

  env1=$(~A){}
  env2=$(~A)+$(~B){}
  env3=$(~A)$(@~B){}

  bytes='3d,12,ff'

 }//root
";



        [TestCase]
        public void JustRoot()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString("root{}");
          
          Assert.AreEqual("root", conf.Root.Name);
        }

        [TestCase]
        public void RootAndSub()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString("root{sub{}}");
          
          Assert.AreEqual("root", conf.Root.Name);
          Assert.AreEqual("sub", conf.Root["sub"].Name);
        }

        [TestCase]
        public void RootAttrAsByteArray()
        {
          var root = src1.AsLaconicConfig();
          
          var bytes = root.AttrByName("bytes").ValueAsByteArray();
          Assert.AreEqual(3, bytes.Length);
          Assert.AreEqual(0x3d, bytes[0]);
          Assert.AreEqual(0x12, bytes[1]);
          Assert.AreEqual(0xff, bytes[2]);
        }

        [TestCase]
        public void RootWithAttrAndSub()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString("root{ atr=val sub{}}");
          
          Assert.AreEqual("root", conf.Root.Name);
          Assert.IsTrue(conf.Root.AttrByName("atr").Exists);
          Assert.AreEqual("val", conf.Root.AttrByName("atr").Value);
        }

        [TestCase]
        public void RootWithAttrWithSpace()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString("root{ atr = val }");
          
          Assert.AreEqual("root", conf.Root.Name);
          Assert.IsTrue(conf.Root.AttrByName("atr").Exists);
          Assert.AreEqual("val", conf.Root.AttrByName("atr").Value);
        }

        [TestCase]
        public void RootWithAttrWithSpace2()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString("root{ atr = val atr2   =                                 'string   3'}");
          
          Assert.AreEqual("root", conf.Root.Name);
          Assert.IsTrue(conf.Root.AttrByName("atr").Exists);
          Assert.AreEqual("val", conf.Root.AttrByName("atr").Value);
          Assert.IsTrue(conf.Root.AttrByName("atr2").Exists);
          Assert.AreEqual("string   3", conf.Root.AttrByName("atr2").Value);
        }

       

        [TestCase]
        public void RootWithManyAttrs()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString("root{ atr1=val1 atr2=val2 'atr 3'='value{3}'}");
          
          Assert.AreEqual("root", conf.Root.Name);
          Assert.AreEqual("val1", conf.Root.AttrByName("atr1").Value);
          Assert.AreEqual("val2", conf.Root.AttrByName("atr2").Value);
          Assert.AreEqual("value{3}", conf.Root.AttrByName("atr 3").Value);
        }

        [TestCase]
        public void RootWithManyAttrsAndSectionValues()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString("root=yes{ atr1=val1 atr2=val2 'atr 3'='value{3}' log=12{}}");
          
          Assert.AreEqual("root", conf.Root.Name);
          Assert.AreEqual("yes", conf.Root.Value);
          Assert.AreEqual("val1", conf.Root.AttrByName("atr1").Value);
          Assert.AreEqual("val2", conf.Root.AttrByName("atr2").Value);
          Assert.AreEqual("value{3}", conf.Root.AttrByName("atr 3").Value);

          Assert.AreEqual("12", conf.Root["log"].Value);
        }
    
        [TestCase]
        public void TestNavigationinVarNames()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString(src1);
                                                              

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
        public void TestNavigationinVarNames_Parallel()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString(src1);
                                                              
          System.Threading.Tasks.Parallel.For(0, 100000,
            (_)=>
            {
                Assert.AreEqual("val1", conf.Root["vars"]["var1"].Value);
                Assert.AreEqual("val1", conf.Root["vars"]["var1"].VerbatimValue);

                Assert.AreEqual("val1", conf.Root["vars"]["var2"].Value);
                Assert.AreEqual("$(../var1)", conf.Root["vars"]["var2"].VerbatimValue);


                Assert.AreEqual("val1", conf.Root["this"].AttrByName("name").Value);
                Assert.AreEqual("$(/vars/var1)", conf.Root["this"].AttrByName("name").VerbatimValue);
                Assert.AreEqual("$(/vars/var1)", conf.Root["this"].AttrByName("name").ValueAsString(verbatim: true));

                Assert.AreEqual("This happened on 05/12/1982 date", conf.Root["this"].AttrByName("text").Value);

                Assert.AreEqual(@"c:\logs\critical", conf.Root["logger"].AttrByName("location").Value);
            });
        }



        [TestCase]
        [ExpectedException(typeof(ConfigException))]
        public void Recursive1()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString(src1);

          Assert.AreEqual("$(../var4)", conf.Root["vars"]["var3"].VerbatimValue);//no exception
          var v = conf.Root["vars"]["var3"].Value;
        }

        [TestCase]
        [ExpectedException(typeof(ConfigException))]
        public void Recursive2()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString(src1);

          Assert.AreEqual("$(../var3)", conf.Root["vars"]["var4"].VerbatimValue);//no exception
          var v = conf.Root["vars"]["var4"].Value;
        }

        [TestCase]
        [ExpectedException(typeof(ConfigException))]
        public void Recursive3Transitive()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString(src1);

          var attr = conf.Root["MyClass"]["data"]["extra"].AttrByName("cycle");

          var v1 = attr.VerbatimValue;//no exception
          var v2 = attr.Value;//exception
        }


        [TestCase]
        public void Recursive4StackCleanup()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString(src1);

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
        public void Recursive4StackCleanup_Parallel()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString(src1);

          System.Threading.Tasks.Parallel.For(0, 100000,
            (_)=>
            {
                var attr = conf.Root["MyClass"]["data"]["extra"].AttrByName("cycle");

                try
                {
                 var v2 = attr.Value;//exception
                }
                catch(Exception error)
                {
                 Assert.IsTrue( error.Message.Contains("recursive vars"));
                }

                //after exception, stack should cleanup and work again as expected
                Assert.AreEqual("val1", conf.Root["vars"]["var1"].Value);
          });
        }




        [TestCase]
        public void Optional()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString(src1);

          Assert.AreEqual(true, string.IsNullOrEmpty(conf.Root["optional"].Value));
        }

        [TestCase]
        [ExpectedException(typeof(ConfigException))]
        public void Required()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString(src1);

          var v = conf.Root["required"].Value;
        }



        [TestCase]
        public void EnvVars1()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString(src1);
          conf.EnvironmentVarResolver = new MyVars();

           Assert.AreEqual("1", conf.Root["env1"].Value);
           Assert.AreEqual("1+2", conf.Root["env2"].Value);
           Assert.AreEqual(@"1\2", conf.Root["env3"].Value);

        }


        [TestCase]
        public void NodePaths()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString(src1);

                                                              

          Assert.AreEqual("/vars/path2", conf.Root["vars"]["path2"].RootPath);
          Assert.AreEqual("/vars/path2/$value", conf.Root["vars"]["path2"].AttrByIndex(0).RootPath);
          Assert.AreEqual("/vars/many/[0]", conf.Root["vars"]["many"][0].RootPath);
          Assert.AreEqual("/vars/many/[1]", conf.Root["vars"]["many"][1].RootPath);
          Assert.AreEqual("/vars/many/[1]/$value", conf.Root["vars"]["many"][1].AttrByIndex(0).RootPath);
        }


        [TestCase]
        public void NodePaths_Parallel()
        {
          var conf = NFX.Environment.LaconicConfiguration.CreateFromString(src1);

           System.Threading.Tasks.Parallel.For(0, 100000,
            (_)=>
            {                                                    
                Assert.AreEqual("/vars/path2", conf.Root["vars"]["path2"].RootPath);
                Assert.AreEqual("/vars/path2/$value", conf.Root["vars"]["path2"].AttrByIndex(0).RootPath);
                Assert.AreEqual("/vars/many/[0]", conf.Root["vars"]["many"][0].RootPath);
                Assert.AreEqual("/vars/many/[1]", conf.Root["vars"]["many"][1].RootPath);
                Assert.AreEqual("/vars/many/[1]/$value", conf.Root["vars"]["many"][1].AttrByIndex(0).RootPath);
            });
        }


        [TestCase]
        public void SaveToString()
        {
          var conf = new LaconicConfiguration();
          conf.Create("very-root");
          conf.Root.AddChildNode("childSection1").AddAttributeNode("name", "Alex");
          conf.Root.AddChildNode("child2").AddAttributeNode("name", "Boris");
          conf.Root["child2"].Value = "Muxa";

          var child3 = conf.Root.AddChildNode("child3");
          child3.AddAttributeNode("atr with space", 1);
          child3.AddAttributeNode("atr2", "val with space");
          child3.AddAttributeNode("atr{3}", null);
          child3.AddAttributeNode("atr=4", null);

          child3.AddAttributeNode("atr5", "this goes on \n\r new\\next line");

          child3.AddChildNode("child3.1");
          child3.AddChildNode("child3.2").AddChildNode("child3.2.1");
          child3.AddChildNode("child3.3");

          var txt = conf.SaveToString(LaconfigWritingOptions.PrettyPrint);
          Console.WriteLine(txt);
          Assert.AreEqual(
@"very-root
{
  childSection1
  {
    name=Alex
  }
  child2=Muxa
  {
    name=Boris
  }
  child3
  {
    ""atr with space""=1
    atr2=""val with space""
    ""atr{3}""=''
    ""atr=4""=''
    atr5=""this goes on \n\r new\\next line""
    child3.1
    {
    }
    child3.2
    {
      child3.2.1
      {
      }
    }
    child3.3
    {
    }
  }
}", txt);

        txt = conf.SaveToString(LaconfigWritingOptions.Compact);

        Console.WriteLine(txt);

        Assert.AreEqual(
 @"very-root{childSection1{name=Alex}child2=Muxa{name=Boris}child3{""atr with space""=1 atr2=""val with space"" ""atr{3}""='' ""atr=4""='' atr5=""this goes on \n\r new\\next line"" child3.1{}child3.2{child3.2.1{}}child3.3{}}}",
   txt);
        }


        [TestCase]
        public void SaveToPrettyStringThenReadBack()
        {
          var conf = new LaconicConfiguration();
          conf.Create("very-root");
          conf.Root.AddChildNode("childSection1").AddAttributeNode("name", "Alex");
          conf.Root.AddChildNode("child2").AddAttributeNode("name", "Boris");
          conf.Root["child2"].Value = "Muxa";

          var child3 = conf.Root.AddChildNode("child3");
          child3.AddAttributeNode("atr with space", 1);
          child3.AddAttributeNode("atr2", "val with space");
          child3.AddAttributeNode("atr{3}", null);
          child3.AddAttributeNode("atr=4", null);

          child3.AddAttributeNode("atr5", "this goes on \n\r new\\next line");

          child3.AddChildNode("child3.1");
          child3.AddChildNode("child3.2").AddChildNode("child3.2.1");
          child3.AddChildNode("child3.3");

          var txt =  conf.SaveToString(LaconfigWritingOptions.PrettyPrint);

          var conf2 = LaconicConfiguration.CreateFromString(txt);

          Assert.IsTrue(conf2.Root["childSection1"].Exists);
          Assert.IsTrue(conf2.Root["child3"].AttrByName("atr with space").Exists);
          Assert.IsTrue(conf2.Root.Navigate("childSection1/$name").Exists);
          Assert.IsTrue(conf2.Root.Navigate("child2/$name").Exists);
          Assert.IsTrue(conf2.Root.Navigate("child3/$atr{3}").Exists);
          Assert.IsTrue(conf2.Root.Navigate("child3/child3.2/child3.2.1").Exists);
          
          Assert.AreEqual("Muxa", conf2.Root.Navigate("child2").Value);
          Assert.AreEqual("1", conf2.Root.Navigate("child3/$atr with space").Value);
          Assert.AreEqual("val with space", conf2.Root.Navigate("child3/$atr2").Value);
          Assert.IsTrue( conf2.Root.Navigate("child3/$atr=4").Value.IsNullOrWhiteSpace() );
        }

        [TestCase]
        public void SaveToCompactStringThenReadBack()
        {
          var conf = new LaconicConfiguration();
          conf.Create("very-root");
          conf.Root.AddChildNode("childSection1").AddAttributeNode("name", "Alex");
          conf.Root.AddChildNode("child2").AddAttributeNode("name", "Boris");
          conf.Root["child2"].Value = "Muxa";

          var child3 = conf.Root.AddChildNode("child3");
          child3.AddAttributeNode("atr with space", 1);
          child3.AddAttributeNode("atr2", "val with space");
          child3.AddAttributeNode("atr{3}", null);
          child3.AddAttributeNode("atr=4", null);

          child3.AddAttributeNode("atr5", "this goes on \n\r new\\next line");

          child3.AddChildNode("child3.1");
          child3.AddChildNode("child3.2").AddChildNode("child3.2.1");
          child3.AddChildNode("child3.3");

          var txt =  conf.SaveToString(LaconfigWritingOptions.Compact);
Console.WriteLine(txt);
          var conf2 = LaconicConfiguration.CreateFromString(txt);

          Assert.IsTrue(conf2.Root["childSection1"].Exists);
          Assert.IsTrue(conf2.Root["child3"].AttrByName("atr with space").Exists);
          Assert.IsTrue(conf2.Root.Navigate("childSection1/$name").Exists);
          Assert.IsTrue(conf2.Root.Navigate("child2/$name").Exists);
          Assert.IsTrue(conf2.Root.Navigate("child3/$atr{3}").Exists);
          Assert.IsTrue(conf2.Root.Navigate("child3/child3.2/child3.2.1").Exists);
          
          Assert.AreEqual("Muxa", conf2.Root.Navigate("child2").Value);
          Assert.AreEqual("1", conf2.Root.Navigate("child3/$atr with space").Value);
          Assert.AreEqual("val with space", conf2.Root.Navigate("child3/$atr2").Value);
          Assert.IsTrue( conf2.Root.Navigate("child3/$atr=4").Value.IsNullOrWhiteSpace() );
        }



        [TestCase]
        public void AddChildSectionFromClone()
        {
          var conf1 = NFX.Environment.LaconicConfiguration.CreateFromString("root{}");
          var conf2 = NFX.Environment.LaconicConfiguration.CreateFromString("root{ sect1{ a=1 b=2 subsect1{c=3 d=4}}}");
          
          conf1.Root.AddChildNode(conf2.Root["sect1"]);

          Console.WriteLine(conf1.ContentView);

          Assert.AreEqual(1, conf1.Root.Children.Count());
          Assert.AreEqual("sect1", conf1.Root["sect1"].Name);
          Assert.AreEqual(2, conf1.Root["sect1"].Attributes.Count());
          Assert.AreEqual(1, conf1.Root["sect1"].AttrByName("a").ValueAsInt());
          Assert.AreEqual(2, conf1.Root["sect1"].AttrByName("b").ValueAsInt());
          Assert.AreEqual(3, conf1.Root["sect1"]["subsect1"].AttrByName("c").ValueAsInt());
          Assert.AreEqual(4, conf1.Root["sect1"]["subsect1"].AttrByName("d").ValueAsInt());
        }


        [TestCase]
        public void Options_DontWriteRootSectionDeclaration()
        {
          var conf = "nfx=90{a=1 b=2 c{d=5}}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

          var opt = new LaconfigWritingOptions{ DontWriteRootSectionDeclaration = true };

          var saved = conf.Configuration.ToLaconicString(opt);
          Console.WriteLine(saved);

          Assert.AreEqual("a=1 b=2 c{d=5}", saved);

          opt = new LaconfigWritingOptions{ /* DontWriteRootSectionDeclaration = false - default */ };

          saved = conf.Configuration.ToLaconicString(opt).Trim();
          Console.WriteLine(saved);

          Assert.AreEqual("nfx=90{a=1 b=2 c{d=5}}", saved);
        }


    }

}
