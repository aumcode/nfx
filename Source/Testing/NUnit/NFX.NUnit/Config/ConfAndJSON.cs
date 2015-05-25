using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using NFX.Serialization.JSON;

namespace NFX.NUnit.Config
{
  [TestFixture]
  public class ConfAndJSON
  {
    [Test]
    public void ConfigSectionNode_2_JSONDataMap()
    {
      var node = @"opt 
                  { 
                    detailed-instrumentation=true 
                    tables 
                    { 
                      master { name='tfactory' fields-qty=14} 
                      slave { name='tdoor' fields-qty=20 important=true} 
                    }
                  }".AsLaconicConfig();  
      var map = node.ToJSONDataMap();

      Assert.AreEqual(2, map.Count);
      Assert.IsTrue(map["detailed-instrumentation"].AsString() == "true");

      var tablesMap = (JSONDataMap)map["tables"];

      var master = (JSONDataMap)tablesMap["master"];
      Assert.IsTrue(master["name"].AsString() == "tfactory");
      Assert.IsTrue(master["fields-qty"].AsString() == "14");

      var slave = (JSONDataMap)tablesMap["slave"];
      Assert.IsTrue(slave["name"].AsString() == "tdoor");
      Assert.IsTrue(slave["fields-qty"].AsString() == "20");
      Assert.IsTrue(slave["important"].AsString() == "true");
    }

    [Test]
    public void JSONDataMap_2_ConfigSectionNode()
    {
      var map = (JSONDataMap)@" { 
                                  'detailed-instrumentation': true, 
                                  tables:
                                  { 
                                    master: { name: 'tfactory', 'fields-qty': 14}, 
                                    slave: { name: 'tdoor', 'fields-qty': 20, important: true} 
                                  }
                                }".JSONToDataObject();

      var cfg = map.ToConfigNode();

      Assert.AreEqual(1, cfg.Attributes.Count());
      Assert.AreEqual(1, cfg.Children.Count());

      Assert.IsTrue(cfg.AttrByName("detailed-instrumentation").ValueAsBool());

      var tablesNode = cfg.Children.Single(ch => ch.Name == "tables");

      var master = cfg.NavigateSection("tables/master");
      Assert.AreEqual(2, master.Attributes.Count());
      Assert.IsTrue(master.AttrByName("name").ValueAsString() == "tfactory");
      Assert.IsTrue(master.AttrByName("fields-qty").ValueAsInt() == 14);

      var slave = cfg.NavigateSection("tables/slave");
      Assert.AreEqual(3, slave.Attributes.Count());
      Assert.IsTrue(slave.AttrByName("name").ValueAsString() == "tdoor");
      Assert.IsTrue(slave.AttrByName("fields-qty").ValueAsInt() == 20);
      Assert.IsTrue(slave.AttrByName("important").ValueAsBool());
    }
  }
}
