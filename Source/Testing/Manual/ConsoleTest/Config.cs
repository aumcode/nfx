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
using NFX;
using NFX.Environment;

namespace ConsoleTest
{

  enum FakeType {RealStuff, Fake, TotalFraud}

  class Config
  {
  
      private string m_YaDebil = App.LocalizedTime.ToString("YYmm");
  
  
  
  
      public static void Run()
      {
                        
        var conf = new XMLConfiguration();
        
        conf.Create();
        
        conf.Root.Name = "This-is-Root";
        var child1 = conf.Root.AddChildNode("Child_Node_1");
        child1.AddChildNode("Grand-Child-1", "this is my value");
        
        child1.AddAttributeNode("AttrWithoutValue");
        child1.AddAttributeNode("Important", true);
        child1.AddAttributeNode("Age", 88);
        child1.AddAttributeNode("DateOf-Enlightement", App.LocalizedTime);
        child1.AddAttributeNode("HowFakeFakeAreYou", FakeType.TotalFraud);
        
        conf.Root.AddChildNode("Child2").AddChildNode("A").AddChildNode("B").AddChildNode("C");
                
        conf.Root["Child2"]["A"]["B"]["C"].Value = "175";
        Console.WriteLine(conf.Root["Child2"]["A"]["B"]["C"].ValueAsInt());
        
        Console.WriteLine(conf.SaveToString(null));
        
             
        
        conf.SaveAs("c:\\TEST.xml");                         
        
        conf = new XMLConfiguration("c:\\TEST.xml");
        Console.WriteLine(conf.Root["Child2"]["A"]["B"]["C"].ValueAsInt());
        Console.WriteLine(conf.Root["Child_Node_1"].AttrByName("HowFakeFakeAreYou").ValueAsEnum<FakeType>(FakeType.RealStuff));        
        
        Console.ReadLine();
      }
  }
}
