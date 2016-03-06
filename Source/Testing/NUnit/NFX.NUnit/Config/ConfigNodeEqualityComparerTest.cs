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
  public class ConfigNodeEqualityComparerTest
  {
    [Test]
    public void JustRoot()
    {
      var root1 = NFX.Environment.LaconicConfiguration.CreateFromString("root{}").Root;
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(null, null));
      Assert.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(null, root1));
      Assert.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root1, null));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root1));

      var root2 = NFX.Environment.LaconicConfiguration.CreateFromString("rOOt { }").Root;
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root2));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root2));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root1));

      root2.Name = "new_root";
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root2));
      Assert.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root1, root2));
      Assert.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root2, root1));
    }

    [Test]
    public void RootWithValue()
    {
      var root1 = NFX.Environment.LaconicConfiguration.CreateFromString("root='people' {}").Root;
      var root2 = NFX.Environment.LaconicConfiguration.CreateFromString("root=people {}").Root;
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root1));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root2));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root2));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root1));
     
      var root3 = NFX.Environment.LaconicConfiguration.CreateFromString("root='PeoPle' {}").Root;
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root3, root3));
      Assert.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root1, root3));
      Assert.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root3, root1));
    }

    [Test]
    public void WithoutName()
    {
      var root1 = NFX.Environment.LaconicConfiguration.CreateFromString("root{}").Root;
      var root2 = NFX.Environment.LaconicConfiguration.CreateFromString("root{}").Root;

      var n1 = root1.AddChildNode("");
      n1.AddChildNode("details");
      var n2 = root2.AddChildNode("");
      n2.AddChildNode("details");
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root2));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root1));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root1));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root2));
    }

    [Test]
    public void DifferentTypes()
    {
      var root1 = NFX.Environment.LaconicConfiguration.CreateFromString("details { name = 'John Smith'}").Root as IConfigNode;
      var root2 = NFX.Environment.LaconicConfiguration.CreateFromString("details { name = 'John Smith'}").Root as IConfigSectionNode;
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root1));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root2));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root2));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root1));

      var root3 = NFX.Environment.LaconicConfiguration.CreateFromString("details { name = 'John Smith'}").Root as IConfigSectionNode;
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root3, root3));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root3));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root3, root2));
    }

    [Test]
    public void Exists()
    {
      var root1 = NFX.Environment.LaconicConfiguration.CreateFromString("details { name = 'John Smith'}").Root;
      var root2 = NFX.Environment.LaconicConfiguration.CreateFromString("details {}").Root;

      var node1 = root1.AttrByName("name");
      var node2 = root2.AttrByName("name");
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(node1, node1));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(node2, node2));
      Assert.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(node1, node2));
      Assert.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(node2, node1));

      var node3 = root2.AttrByName("name");
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(node3, node3));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(node2, node3));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(node3, node2));
    }

    [Test]
    public void AttributesCountAndOrder()
    {
      string conf1 = 
        @"root
          {
            type = 'person'
            has_drive_license = 0
            details {}
          }";
      string conf2 = 
        @"root
          {
            type = 'person'
            details {}
            has_drive_license = 0
          }";
      string conf3 = 
        @"root
          {
            type = 'person'
            details {}
          }";
      string conf4 = 
        @"root
          {
            has_drive_license = 0
            type = 'person'
            details {}
          }";

      var root1 = NFX.Environment.LaconicConfiguration.CreateFromString(conf1).Root;
      var root2 = NFX.Environment.LaconicConfiguration.CreateFromString(conf2).Root;
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root1));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root2));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root2));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root1));

      // count
      var root3 = NFX.Environment.LaconicConfiguration.CreateFromString(conf3).Root;
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root3, root3));
      Assert.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root1, root3));
      Assert.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root3, root1));

      // order
      var root4 = NFX.Environment.LaconicConfiguration.CreateFromString(conf4).Root;
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root4, root4));
      Assert.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root1, root4));
      Assert.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root4, root1));
    }

    [Test]
    public void SectionsCountAndOrder()
    {
      string conf1 = 
        @"root
          {
            type = 'person'
            details {}
            contacts {}
          }";
      string conf2 = 
        @"root
          {
            details {}
            type = 'person'
            contacts {}
          }";
      string conf3 = 
        @"root
          {
            details {}
            type = 'person'
          }";
      string conf4 = 
        @"root
          {
            contacts {}
            type = 'person'
            details {}
          }";

      var root1 = NFX.Environment.LaconicConfiguration.CreateFromString(conf1).Root;
      var root2 = NFX.Environment.LaconicConfiguration.CreateFromString(conf2).Root;
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root1));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root2));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root2));
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root1));

      // count
      var root3 = NFX.Environment.LaconicConfiguration.CreateFromString(conf3).Root;
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root3, root3));
      Assert.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root1, root3));
      Assert.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root3, root1));

      // order
      var root4 = NFX.Environment.LaconicConfiguration.CreateFromString(conf4).Root;
      Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root4, root4));
      Assert.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root1, root4));
      Assert.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root4, root1));
    }

    [Test]
    public void Overall()
    {
      string conf1 = 
        @"root
        {
          type = person
          details 
          {
            name = 'John Smith'
            age = 22
          }
          contacts
          {
            phone=1111 { mobile='' }
            address {}
          }
          has-drive-license = 1
        }";
      string conf2 = 
        @"Root
        {
          Details 
          {
            name = 'John Smith'
            AGE = 22
          }
          Contacts
          {
            phone=1111 { mobile='' }
            address {}
          }
          Type = person
          has-drive-license = 1
        }";
        var root1 = NFX.Environment.LaconicConfiguration.CreateFromString(conf1).Root;
        var root2 = NFX.Environment.LaconicConfiguration.CreateFromString(conf2).Root;
        Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root1));
        Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root2));
        Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root2));
        Assert.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root1));
    }

    [Test]
    public void HashCode()
    {
      string conf1 = 
        @"root
        {
          type = person
          details 
          {
            name = 'John Smith'
            age = 22
          }
          contacts
          {
            phone=1111 { mobile='' }
            address {}
          }
          has-drive-license = 1
        }";
      string conf2 = 
        @"Root
        {
          Details 
          {
            name = 'John Smith'
            AGE = '22'
          }
          Contacts
          {
            phone=1111 { mobile='' }
            address {}
          }
          Type = person
          has-drive-license = 1
        }";
        var root1 = NFX.Environment.LaconicConfiguration.CreateFromString(conf1).Root;
        var root2 = NFX.Environment.LaconicConfiguration.CreateFromString(conf2).Root;
        var hash1 = ConfigNodeEqualityComparer.Instance.GetHashCode(root1);
        var hash2 = ConfigNodeEqualityComparer.Instance.GetHashCode(root2);
        Assert.AreEqual(hash1, hash2);

        var hashOfNull = ConfigNodeEqualityComparer.Instance.GetHashCode(null); 
        Assert.AreEqual(hashOfNull, hashOfNull);

        root1 = NFX.Environment.LaconicConfiguration.CreateFromString("root { }").Root;
        root2 = NFX.Environment.LaconicConfiguration.CreateFromString("rOOt { }").Root;
        hash1 = ConfigNodeEqualityComparer.Instance.GetHashCode(root1);
        hash2 = ConfigNodeEqualityComparer.Instance.GetHashCode(root2);
        Assert.AreEqual(hash1, hash2);

        root2 = NFX.Environment.LaconicConfiguration.CreateFromString("root=people { }").Root;
        hash2 = ConfigNodeEqualityComparer.Instance.GetHashCode(root2);
        Assert.AreNotEqual(hash1, hash2);

        var sectionNode = root1["details"];
        hash1 = ConfigNodeEqualityComparer.Instance.GetHashCode(sectionNode);
        Assert.AreEqual(hash1, hashOfNull);

        var attrNode = root1.AttrByIndex(0);
        hash1 = ConfigNodeEqualityComparer.Instance.GetHashCode(attrNode);
        Assert.AreEqual(hash1, hashOfNull);

        attrNode = root1.AttrByName("type");
        hash1 = ConfigNodeEqualityComparer.Instance.GetHashCode(attrNode);
        Assert.AreEqual(hash1, hashOfNull);
    }
  }
}
