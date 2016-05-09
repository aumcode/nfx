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
using NFX.Serialization.JSON;

namespace NFX.NUnit.Config
{
    [TestFixture]   
    public class JSONConfigurationTest
    {
        [TestCase]
        [ExpectedException(typeof(ConfigException))]
        public void ReadOnlyErrorOnNodeCreate()
        {
           var conf = new NFX.Environment.JSONConfiguration();
           conf.Create(); 
           
           conf.SetReadOnly(true);

           conf.Root.AddChildNode("A", null);
        }

        [TestCase]
        public void NodeCreate()
        {
           var conf = new NFX.Environment.JSONConfiguration();
           conf.Create(); 
           conf.Root.AddChildNode("A", null);

           Assert.AreEqual("A", conf.Root["A"].Name);
        }


        [TestCase]
        public void EmptySectionAndAttributeNodes()
        {
           var conf = new NFX.Environment.JSONConfiguration();
           conf.Create(); 
           conf.Root.AddChildNode("A", null).AddChildNode("A.A", "haha!").AddAttributeNode("good", true);

           Assert.AreEqual("haha!", conf.Root["A"]["A.A"].Value);
           Assert.AreEqual(true, conf.Root["A"]["A.A"].Exists);
           Assert.AreEqual(true, conf.Root["A"]["A.A"].AttrByName("good").Exists);
           Assert.AreEqual(true, conf.Root["A"]["A.A"].AttrByIndex(0).Exists);

           Assert.AreEqual(false, conf.Root["A1"]["A.A"].Exists);
           Assert.AreEqual(false, conf.Root["A"]["A.A1"].Exists);

           Assert.AreEqual(false, conf.Root["A"]["A.A"].AttrByName("bad").Exists);
           Assert.AreEqual(false, conf.Root["A"]["A.A"].AttrByIndex(100).Exists);
        }


        [TestCase]
        [ExpectedException(typeof(ConfigException))]
        public void ReadOnlyErrorOnNodeRename()
        {
           var conf = new NFX.Environment.JSONConfiguration();
           conf.Create(); 
           conf.Root.AddChildNode("A", null);
           conf.SetReadOnly(true);
           conf.Root.Name = "changed-name";
        }
        
        [TestCase]
        [ExpectedException(typeof(ConfigException))]
        public void ReadOnlyErrorOnNodeDelete()
        {
           var conf = new NFX.Environment.JSONConfiguration();
           conf.Create(); 
           conf.Root.AddChildNode("A", null);
           conf.SetReadOnly(true);
           conf.Root["A"].Delete();
        }
        
        [TestCase]
        public void NodeDelete()
        {
           var conf = new NFX.Environment.JSONConfiguration();
           conf.Create(); 
           conf.Root.AddChildNode("A", null);
           conf.Root.AddChildNode("B", null).AddChildNode("B1");
           conf.Root["A"].Delete();
           Assert.AreEqual(false, conf.Root["A"].Exists);
           Assert.AreEqual(true, conf.Root["B"].Exists);
           
           conf.Root.ResetModified();
           Assert.AreEqual(false, conf.Root["B"].Modified);
           conf.Root["B"]["B1"].Delete();
           Assert.AreEqual(true, conf.Root["B"].Modified);
        }
        
        [TestCase]
        public void RootDelete()
        {
           var conf = new NFX.Environment.JSONConfiguration();
           conf.Create(); 
           conf.Root.AddChildNode("A", null);
           
           Assert.AreEqual(true, conf.Root.Exists);
           conf.Root.Delete();
           Assert.AreEqual(false, conf.Root.Exists);
        }
        

        [TestCase]
        public void NodeRename()
        {
           var conf = new NFX.Environment.JSONConfiguration();
           conf.Create(); 
           conf.Root.AddChildNode("A", null);
           conf.Root["A"].Name = "B";
           Assert.AreEqual("B", conf.Root["B"].Name);
        }

        [TestCase]
        public void NavigationAndValueAccessors()
        {
           var conf = new NFX.Environment.JSONConfiguration();
           conf.Create(); 
           conf.Root.AddChildNode("A", 10).AddChildNode("A.A", 20);
           conf.Root.AddChildNode("B", 789);
           conf.Root.AddChildNode("URI", UriKind.RelativeOrAbsolute);

           conf.Root["A"]["A.A"].AddChildNode("MARS", -1000);
           Assert.AreEqual(-1000, conf.Root["A"]["A.A"]["MARS"].ValueAsInt());
           Assert.AreEqual(-1000, conf.Root[0][0][0].ValueAsInt());
           Assert.AreEqual(789, conf.Root[1].ValueAsInt());
           Assert.AreEqual(UriKind.RelativeOrAbsolute, conf.Root["URI"].ValueAsEnum<UriKind>());
           Assert.AreEqual(UriKind.RelativeOrAbsolute, conf.Root["URI"].ValueAsEnum<UriKind>(UriKind.Absolute));
           Assert.AreEqual(UriKind.RelativeOrAbsolute, conf.Root["NONENTITY"].ValueAsEnum<UriKind>(UriKind.RelativeOrAbsolute));
        }


        [TestCase]
        public void LoadReadSaveReadAgainCompound()
        {
           var json = @"
           {
            root:
            { 'kind': 'Absolute',
              a: 
              {
                 b:{'cool': true, c: 75 }
              },
            'web.world': 'who knows?'
           }}";
                                   
           var conf = NFX.Environment.JSONConfiguration.CreateFromJSON(json);
           
           Assert.AreEqual(UriKind.Absolute, conf.Root.AttrByName("kind").ValueAsEnum<UriKind>(UriKind.Relative));
           Assert.AreEqual(true, conf.Root["a"]["b"].AttrByName("cool").ValueAsBool(false));
           Assert.AreEqual(75, conf.Root["a"]["b"].AttrByName("c").ValueAsInt());
           Assert.AreEqual(-123, conf.Root["a"]["dont exist"].AttrByName("c").ValueAsInt(-123));

           Assert.AreEqual("who knows?", conf.Root.AttrByName("web.world").ValueAsString());

           var savedJSON = conf.SaveToString(JSONWritingOptions.PrettyPrint);

           Console.WriteLine(savedJSON);

           //retest after configuration was saved and then reloaded from string
           conf =  NFX.Environment.JSONConfiguration.CreateFromJSON(savedJSON);
           Assert.AreEqual(UriKind.Absolute, conf.Root.AttrByName("kind").ValueAsEnum<UriKind>(UriKind.Relative));
           Assert.AreEqual(true, conf.Root["a"]["b"].AttrByName("cool").ValueAsBool(false));
           Assert.AreEqual(75, conf.Root["a"]["b"].AttrByName("c").ValueAsInt());
           Assert.AreEqual("who knows?", conf.Root.AttrByName("web.world").ValueAsString());
        }


        [TestCase]
        public void SectionValue()
        {
          var json = @"{
           root: {
                  '-section-value': 123,
                  a: {'-section-value': 237}

           }}";
                                   
           var conf = NFX.Environment.JSONConfiguration.CreateFromJSON(json);

           Console.WriteLine(conf.SaveToString(JSONWritingOptions.PrettyPrint));



           Assert.AreEqual(123, conf.Root.ValueAsInt());
           Assert.AreEqual(237, conf.Root["a"].ValueAsInt());
        }


        [TestCase]
        public void ModifiedFlag()
        {
          var json = @"{
           root: {
           
            a: {
               b: {cool: true, snake: false, c: {'-section-value': 75}}
            }

           }}";
                                   
           var conf = NFX.Environment.JSONConfiguration.CreateFromJSON(json);

           Assert.IsFalse(conf.Root.Modified);

           Assert.AreEqual("75", conf.Root.Navigate("a/b/c").Value);
           conf.Root.Navigate("a/b/c").Value = "a";
           Assert.AreEqual("a", conf.Root.Navigate("a/b/c").Value);

           Assert.IsTrue(conf.Root.Modified);
           conf.Root.ResetModified();
           Assert.IsFalse(conf.Root.Modified);

           conf.Root.NavigateSection("a/b").AddAttributeNode("a", true);
           Assert.IsTrue(conf.Root.Modified);
           conf.Root.Navigate("a/b").ResetModified();
           Assert.IsFalse(conf.Root.Modified);
        }

        [TestCase]
        public void LaconicJSONRoundtrip()
        {
          var conf1 = @"
            tezt-root='Hahaha\nha!'
            {
              types
              {
                _override=stop
                t1='Type1'
                t2='Type2'
              }
              providers
              {
                provider1
                {
                  type=$(/types/$t1)
                  name='Zhaba'
                }
                provider2
                {
                  type=$(/types/$t2)
                  name='Koshka'

                  //notice sub-sections with the same name
                  set{ a=123 b=true c=11{da=net}}
                  set{ a=5623 b=false}
                  set{ a=78 b=true}
                }
              }
            }".AsLaconicConfig();

           assert(conf1);
           Console.WriteLine(conf1.ToLaconicString());

           var map = conf1.Configuration.ToConfigurationJSONDataMap();
           var json = map.ToJSON();

           var cjson = JSONConfiguration.CreateFromJSON(json);
           assert(cjson.Root);

           json = cjson.SaveToString(JSONWritingOptions.PrettyPrint);
           Console.WriteLine(json);
           cjson = JSONConfiguration.CreateFromJSON(json);
           assert(cjson.Root);
        }

        private void assert(ConfigSectionNode root)
        {
          Assert.AreEqual("tezt-root", root.Name);
          Assert.AreEqual("Hahaha\nha!", root.Value);

          Assert.AreEqual("Type1", root.Navigate("types/$t1").Value);
          Assert.AreEqual("Type2", root.Navigate("types/$t2").Value);

          Assert.AreEqual("Type1", root.Navigate("providers/provider1/$type").Value);
          Assert.AreEqual("Type2", root.Navigate("providers/provider2/$type").Value);
          Assert.IsFalse(root.Navigate("providers/provider3").Exists);
          
          Assert.AreEqual(3, root["providers"]["provider2"].ChildCount);
          Assert.AreEqual("Koshka", root.Navigate("providers/provider2/$name").Value);
          
          Assert.AreEqual("net", root.Navigate("providers/provider2/[0]/c/$da").Value);
          Assert.AreEqual(5623, root.Navigate("providers/provider2/[1]/$a").ValueAsInt());
          Assert.IsFalse(root.Navigate("providers/provider2/[1]/$b").ValueAsBool());
          Assert.AreEqual(78, root.Navigate("providers/provider2/[2]/$a").ValueAsInt());
          Assert.IsTrue(root.Navigate("providers/provider2/[2]/$b").ValueAsBool());
        }


    }
}
