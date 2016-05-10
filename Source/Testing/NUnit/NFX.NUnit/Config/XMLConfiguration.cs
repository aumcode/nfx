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
    public class XMLConfiguration
    {
        [TestCase]
        [ExpectedException(typeof(ConfigException))]
        public void BadXMLName()
        {
          var conf = new NFX.Environment.XMLConfiguration();
          conf.Create();
          conf.Root.AddChildNode("# bad name", null);
        }

        [TestCase]
        public void StrictNamesFalse()
        {
          var conf = new NFX.Environment.XMLConfiguration();
          conf.Create();
          conf.StrictNames = false;
          var node = conf.Root.AddChildNode("bad name", null);
          Assert.AreEqual("bad-name", node.Name);
        }



        [TestCase]
        [ExpectedException(typeof(ConfigException))]
        public void ReadOnlyErrorOnNodeCreate()
        {
           var conf = new NFX.Environment.XMLConfiguration();
           conf.Create(); 
           
           conf.SetReadOnly(true);

           conf.Root.AddChildNode("A", null);
        }

        [TestCase]
        public void NodeCreate()
        {
           var conf = new NFX.Environment.XMLConfiguration();
           conf.Create(); 
           conf.Root.AddChildNode("A", null);

           Assert.AreEqual("A", conf.Root["A"].Name);
        }


        [TestCase]
        public void EmptySectionAndAttributeNodes()
        {
           var conf = new NFX.Environment.XMLConfiguration();
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
           var conf = new NFX.Environment.XMLConfiguration();
           conf.Create(); 
           conf.Root.AddChildNode("A", null);
           conf.SetReadOnly(true);
           conf.Root.Name = "changed-name";
        }
        
        [TestCase]
        [ExpectedException(typeof(ConfigException))]
        public void ReadOnlyErrorOnNodeDelete()
        {
           var conf = new NFX.Environment.XMLConfiguration();
           conf.Create(); 
           conf.Root.AddChildNode("A", null);
           conf.SetReadOnly(true);
           conf.Root["A"].Delete();
        }
        
        [TestCase]
        public void NodeDelete()
        {
           var conf = new NFX.Environment.XMLConfiguration();
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
           var conf = new NFX.Environment.XMLConfiguration();
           conf.Create(); 
           conf.Root.AddChildNode("A", null);
           
           Assert.AreEqual(true, conf.Root.Exists);
           conf.Root.Delete();
           Assert.AreEqual(false, conf.Root.Exists);
        }
        

        [TestCase]
        public void NodeRename()
        {
           var conf = new NFX.Environment.XMLConfiguration();
           conf.Create(); 
           conf.Root.AddChildNode("A", null);
           conf.Root["A"].Name = "B";
           Assert.AreEqual("B", conf.Root["B"].Name);
        }

        [TestCase]
        public void NavigationAndValueAccessors()
        {
           var conf = new NFX.Environment.XMLConfiguration();
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
           var xml = @"<root kind=""Absolute"">
           <!-- comment -->
            <a>
              <b cool=""true""> <c> 75 </c> </b>
            </a>
            <web.world>who knows?</web.world>
           </root>";
                                   
           var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);
           
           Assert.AreEqual(UriKind.Absolute, conf.Root.AttrByName("kind").ValueAsEnum<UriKind>(UriKind.Relative));
           Assert.AreEqual(true, conf.Root["a"]["b"].AttrByName("cool").ValueAsBool(false));
           Assert.AreEqual(75, conf.Root["a"]["b"]["c"].ValueAsInt());
           Assert.AreEqual(-123, conf.Root["a"]["dont exist"]["c"].ValueAsInt(-123));

           Assert.AreEqual("who knows?", conf.Root["web.world"].ValueAsString());

           var savedXml = conf.ToString();
           //retest after configuration was saved and then reloaded from string
           conf =  NFX.Environment.XMLConfiguration.CreateFromXML(savedXml);
           Assert.AreEqual(UriKind.Absolute, conf.Root.AttrByName("kind").ValueAsEnum<UriKind>(UriKind.Relative));
           Assert.AreEqual(true, conf.Root["a"]["b"].AttrByName("cool").ValueAsBool(false));
           Assert.AreEqual(75, conf.Root["a"]["b"]["c"].ValueAsInt());
           Assert.AreEqual("who knows?", conf.Root["web.world"].ValueAsString());
        }


        [TestCase]
        public void PathNavigation()
        {
           var xml = @"<root kind=""Absolute"">
           <!-- comment -->
            <a>
              <b cool=""true"" snake=""false""> <c> 75 </c> </b>
            </a>
            <web.world>who knows?</web.world>
           </root>";
                                   
           var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);
           
           Assert.AreEqual(UriKind.Absolute, conf.Root.Navigate("$kind").ValueAsEnum<UriKind>(UriKind.Relative));
           Assert.AreEqual(UriKind.Absolute, conf.Root.Navigate("!$kind").ValueAsEnum<UriKind>(UriKind.Relative));

           Assert.AreEqual(75, conf.Root.Navigate("a/b/c").ValueAsInt());

           Assert.AreEqual(true, conf.Root.Navigate("a/b/c").Exists);
           Assert.AreEqual(false, conf.Root.Navigate("a/b/c/d").Exists);
           Assert.AreEqual(false, conf.Root.Navigate("GGG/b/c").Exists); //non existing section is in the beginning
           Assert.AreEqual(false, conf.Root.Navigate("a/GGG/c").Exists); //non existing section is in the middle

           Assert.AreEqual(true, conf.Root.Navigate("/a/b/$cool").ValueAsBool());
           Assert.AreEqual(false, conf.Root.Navigate("/a/b/$snake").ValueAsBool());
           Assert.AreEqual("b", conf.Root.Navigate("/a/b").Name);
           Assert.AreEqual(conf.Root, (conf.Root.Navigate("a/b") as ConfigSectionNode).Navigate("../.."));
           
        }

        [TestCase]
        [ExpectedException(typeof(ConfigException))]
        public void PathNavigationWithRequiredNodes()
        {
           var xml = @"<root>
           
            <a>
              <b cool=""true"" snake=""false""> <c> 75 </c> </b>
            </a>

           </root>";
                                   
           var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);
           
           Assert.AreEqual(75, conf.Root.Navigate("a/b/c").ValueAsInt());

           //this should throw
           Assert.AreEqual(false, conf.Root.Navigate("!a/b/c/d").Exists);
           
        }

        [TestCase]
        public void ModifiedFlag()
        {
          var xml = @"<root>
           
            <a>
              <b cool=""true"" snake=""false""> <c> 75 </c> </b>
            </a>

           </root>";
                                   
           var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

           Assert.IsFalse(conf.Root.Modified);

           conf.Root.Navigate("a/b/c").Value = "a";

           Assert.IsTrue(conf.Root.Modified);
           conf.Root.ResetModified();
           Assert.IsFalse(conf.Root.Modified);

           conf.Root.NavigateSection("a/b").AddAttributeNode("a", true);
           Assert.IsTrue(conf.Root.Modified);
           conf.Root.Navigate("a/b").ResetModified();
           Assert.IsFalse(conf.Root.Modified);

        }

        [TestCase]
        public void LoadMixedContent()
        {
           var xml = @"<root>GAGARIN<a> <!-- comment -->
              <b cool=""true""> <c> 75 </c> </b>
            </a>
            <web.world>who knows?</web.world>
           </root>";
                                   
           var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);
           
           Assert.AreEqual("GAGARIN", conf.Root.Value);
           Assert.AreEqual(true, conf.Root["a"]["b"].AttrByName("cool").ValueAsBool(false));
           Assert.AreEqual(75, conf.Root["a"]["b"]["c"].ValueAsInt());
           Assert.AreEqual(-123, conf.Root["a"]["dont exist"]["c"].ValueAsInt(-123));

           Assert.AreEqual("who knows?", conf.Root["web.world"].ValueAsString());
        }



    }
}
