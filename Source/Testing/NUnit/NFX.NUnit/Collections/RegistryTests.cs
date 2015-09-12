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

using System.Threading;
using System.Threading.Tasks;

using NFX.Collections;
using System.Collections;

namespace NFX.NUnit.Collections
{
  
                       public class NamedClazz : INamed
                       {
                         public NamedClazz(string name, int data) 
                         {
                           m_Name = name;
                           m_Data = data;
                         }
                         private string m_Name;
                         private int m_Data;
                         public string Name { get { return m_Name; } }
                         public int Data { get { return m_Data; } }
                       }

                       public class OrderedClazz : NamedClazz, IOrdered
                       {
                         public OrderedClazz(string name, int order, int data) : base (name, data) 
                         {
                           m_Order = order;
                         }
                         private int m_Order;
                         public int Order{ get { return m_Order; } }
                       }

  
  
  [TestFixture]
  public class RegistryTests
  {
    [TestCase]
    public void Registry()
    {
       var reg = new Registry<NamedClazz>();
       Assert.IsTrue(  reg.Register( new NamedClazz("Apple", 1) ) );
       Assert.IsTrue(  reg.Register( new NamedClazz("Banana", 2) ) );
       Assert.IsFalse(  reg.Register( new NamedClazz("Apple", 3) ) );

       Assert.AreEqual(2, reg.Count);

       Assert.AreEqual(1, reg["Apple"].Data);
       Assert.AreEqual(2, reg["Banana"].Data);
       Assert.AreEqual(null, reg["Grapes"]);

       Assert.IsFalse( reg.Unregister(new NamedClazz("I was never added before", 1)) );
       Assert.AreEqual(2, reg.Count);

       Assert.IsTrue( reg.Unregister(new NamedClazz("Apple", 1)) );
       Assert.AreEqual(1, reg.Count);
       Assert.AreEqual(null, reg["Apple"]);
       Assert.AreEqual(2, reg["Banana"].Data);
    }


    [TestCase]
    public void CaseInsensitiv()
    {
       var reg = new Registry<NamedClazz>();//INSENSITIVE
       Assert.IsTrue(  reg.Register( new NamedClazz("Apple", 1) ) );
       Assert.IsTrue(  reg.Register( new NamedClazz("Banana", 2) ) );
       Assert.IsFalse(  reg.Register( new NamedClazz("APPLE", 3) ) );

       Assert.AreEqual(2, reg.Count);

       Assert.AreEqual(1, reg["Apple"].Data);
       Assert.AreEqual(2, reg["Banana"].Data);
       Assert.AreEqual(1, reg["APPLE"].Data);

       Assert.IsFalse( reg.Unregister(new NamedClazz("I was never added before", 1)) );
       Assert.AreEqual(2, reg.Count);

       Assert.IsTrue( reg.Unregister(new NamedClazz("ApPle", 1)) );
       Assert.AreEqual(1, reg.Count);
       Assert.AreEqual(null, reg["Apple"]);
       Assert.AreEqual(2, reg["Banana"].Data);
    }

    [TestCase]
    public void CaseSensitive()
    {
       var reg = new Registry<NamedClazz>(true);//SENSITIVE!!!!!!!!!!!!!!!!!!!!!!!!!!!
       Assert.IsTrue(  reg.Register( new NamedClazz("Apple", 1) ) );
       Assert.IsTrue(  reg.Register( new NamedClazz("Banana", 2) ) );
       Assert.IsTrue(  reg.Register( new NamedClazz("APPLE", 3) ) );

       Assert.AreEqual(3, reg.Count);

       Assert.AreEqual(1, reg["Apple"].Data);
       Assert.AreEqual(2, reg["Banana"].Data);
       Assert.AreEqual(3, reg["APPLE"].Data);

       Assert.IsFalse( reg.Unregister(new NamedClazz("I was never added before", 1)) );
       Assert.AreEqual(3, reg.Count);

       Assert.IsFalse( reg.Unregister(new NamedClazz("AppLE", 1)) );
       Assert.AreEqual(3, reg.Count);
       Assert.IsTrue( reg.Unregister(new NamedClazz("APPLE", 3)) );
       Assert.AreEqual(2, reg.Count);
       Assert.AreEqual(1, reg["Apple"].Data);
       Assert.AreEqual(2, reg["Banana"].Data);
       Assert.AreEqual(null, reg["APPLE"]);
    }


    [TestCase]
    public void Registry_UnregisterByName()
    {
       var reg = new Registry<NamedClazz>();
       Assert.IsTrue(  reg.Register( new NamedClazz("Apple", 1) ) );
       Assert.IsTrue(  reg.Register( new NamedClazz("Banana", 2) ) );
       Assert.IsFalse(  reg.Register( new NamedClazz("Apple", 3) ) );

       Assert.AreEqual(2, reg.Count);

       Assert.AreEqual(1, reg["Apple"].Data);
       Assert.AreEqual(2, reg["Banana"].Data);
       Assert.AreEqual(null, reg["Grapes"]);

       Assert.IsFalse( reg.Unregister("I was never added before") );
       Assert.AreEqual(2, reg.Count);

       Assert.IsTrue( reg.Unregister("Apple") );
       Assert.AreEqual(1, reg.Count);
       Assert.AreEqual(null, reg["Apple"]);
       Assert.AreEqual(2, reg["Banana"].Data);
    }

    [TestCase]
    public void Registry_Clear()
    {
       var reg = new Registry<NamedClazz>();
       Assert.IsTrue(  reg.Register( new NamedClazz("Apple", 1) ) );
       Assert.IsTrue(  reg.Register( new NamedClazz("Banana", 2) ) );
       Assert.IsFalse(  reg.Register( new NamedClazz("Apple", 3) ) );

       Assert.AreEqual(2, reg.Count);

       Assert.AreEqual(1, reg["Apple"].Data);
       Assert.AreEqual(2, reg["Banana"].Data);
       Assert.AreEqual(null, reg["Grapes"]);

       reg.Clear();

       Assert.AreEqual(0, reg.Count);
       Assert.AreEqual(null, reg["Apple"]);
       Assert.AreEqual(null, reg["Banana"]);
    }



    [TestCase]
    public void OrderedRegistry()
    {
       var reg = new OrderedRegistry<OrderedClazz>();
       Assert.IsTrue(  reg.Register( new OrderedClazz("Apple",  8,  1) ) );
       Assert.IsTrue(  reg.Register( new OrderedClazz("Banana", -2,  2) ) );
       Assert.IsFalse(  reg.Register( new OrderedClazz("Apple", 22, 3) ) );

       Assert.AreEqual(2, reg.Count);

       Assert.AreEqual(1, reg["Apple"].Data);
       Assert.AreEqual(2, reg["Banana"].Data);
       Assert.AreEqual(null, reg["Grapes"]);

       var ordered = reg.OrderedValues.ToArray();
       Assert.AreEqual(2, ordered.Length);
       Assert.AreEqual("Banana", ordered[0].Name);
       Assert.AreEqual("Apple", ordered[1].Name);

       Assert.IsTrue( reg.Register( new OrderedClazz("Zukini", 0, 180) )  );
       
       ordered = reg.OrderedValues.ToArray();
       Assert.AreEqual(3, ordered.Length);
       Assert.AreEqual("Banana", ordered[0].Name);
       Assert.AreEqual("Zukini", ordered[1].Name);
       Assert.AreEqual("Apple", ordered[2].Name);


       Assert.IsFalse( reg.Unregister(new OrderedClazz("I was never added before", 1, 1)) );
       Assert.AreEqual(3, reg.Count);

       Assert.IsTrue( reg.Unregister(new OrderedClazz("Apple", 2, 1)) );
       Assert.AreEqual(2, reg.Count);
       Assert.AreEqual(null, reg["Apple"]);
       Assert.AreEqual(2, reg["Banana"].Data);
       Assert.AreEqual(180, reg["Zukini"].Data);
    }

    [TestCase]
    public void OrderedRegistry_Clear()
    {
       var reg = new OrderedRegistry<OrderedClazz>();
       Assert.IsTrue(  reg.Register( new OrderedClazz("Apple",  8,  1) ) );
       Assert.IsTrue(  reg.Register( new OrderedClazz("Banana", -2,  2) ) );
       Assert.IsFalse(  reg.Register( new OrderedClazz("Apple", 22, 3) ) );

       var ordered = reg.OrderedValues.ToArray();
       Assert.AreEqual(2, ordered.Length);
       Assert.AreEqual("Banana", ordered[0].Name);
       Assert.AreEqual("Apple", ordered[1].Name);

       reg.Clear();
       Assert.AreEqual(0, reg.Count);
       Assert.AreEqual(0, reg.OrderedValues.Count());


    }


    [TestCase]
    public void Registry_Parallel()
    {
       var reg = new Registry<NamedClazz>();

       var CNT = 250000;

       Parallel.For(0, CNT, (i)=>
       {
           reg.Register(new NamedClazz("Name_{0}".Args(i % 128), i));
           var item = reg["Name_{0}".Args(i % 128)];//it may be null
           reg.Unregister("Name_{0}".Args((i-2) %128));
       });

       Assert.Pass("No exceptions thrown during multithreaded parallel work");
    }



    [TestCase]
    public void OrderedRegistry_Parallel()
    {
       var reg = new OrderedRegistry<OrderedClazz>();

       var CNT = 250000;

       Parallel.For(0, CNT, (i)=>
       {
           reg.Register(new OrderedClazz("Name_{0}".Args(i % 128), i % 789, i));
           var item = reg["Name_{0}".Args(i % 128)];//it may be null
           reg.Unregister("Name_{0}".Args((i-2) %128));
       });

       Assert.Pass("No exceptions thrown during multithreaded parallel work");
    }



    
    [TestCase]
    public void OrderedRegistry_RegisterOrReplace()
    {
       var reg = new OrderedRegistry<OrderedClazz>();
       Assert.IsTrue(  reg.Register( new OrderedClazz("Apple",  8,  1) ) );
       Assert.IsTrue(  reg.Register( new OrderedClazz("Banana", -2,  2) ) );
       Assert.IsTrue(  reg.Register( new OrderedClazz("Grapes", 0,  3) ) );
       Assert.IsFalse(  reg.Register( new OrderedClazz("Apple", 22, 12345) ) );

       var ordered = reg.OrderedValues.ToArray();
       Assert.AreEqual(3, ordered.Length);
       Assert.AreEqual("Banana", ordered[0].Name);
       Assert.AreEqual("Grapes", ordered[1].Name);
       Assert.AreEqual("Apple", ordered[2].Name);

       Assert.AreEqual( 1, reg["Apple"].Data);

       Assert.IsFalse(  reg.RegisterOrReplace( new OrderedClazz("Apple", 22, 12345) ) );

       ordered = reg.OrderedValues.ToArray();
       Assert.AreEqual(3, ordered.Length);
       Assert.AreEqual("Banana", ordered[0].Name);
       Assert.AreEqual("Grapes", ordered[1].Name);
       Assert.AreEqual("Apple", ordered[2].Name);

       Assert.AreEqual( 12345, reg["Apple"].Data);//got replaced


       Assert.IsTrue(  reg.RegisterOrReplace( new OrderedClazz("Peach", 99, -234) ) );

       ordered = reg.OrderedValues.ToArray();
       Assert.AreEqual(4, ordered.Length);
       Assert.AreEqual("Banana", ordered[0].Name);
       Assert.AreEqual("Grapes", ordered[1].Name);
       Assert.AreEqual("Apple", ordered[2].Name);
       Assert.AreEqual("Peach", ordered[3].Name);

       Assert.AreEqual( 12345, reg["Apple"].Data);//got replaced before
       Assert.AreEqual( -234, reg["Peach"].Data);

    }


    [TestCase]
    public void OrderedRegistry_GetOrRegister()
    {
       var reg = new OrderedRegistry<OrderedClazz>();
       
       bool wasAdded;
       var obj1 = reg.GetOrRegister<object>("Apple", (_) => new OrderedClazz("Apple",  8,  1), null, out wasAdded);
       Assert.AreEqual( 8, obj1.Order );
       Assert.IsTrue( wasAdded );

       var obj2 = reg.GetOrRegister<object>("Yabloko", (_) => new OrderedClazz("Yabloko",  3,  2), null, out wasAdded);
       Assert.AreEqual( 3, obj2.Order );
       Assert.IsTrue( wasAdded );

       Assert.IsFalse( object.ReferenceEquals( obj1, obj2 ) );

       var obj3 = reg.GetOrRegister<object>("Apple", (_) => new OrderedClazz("Apple",  123,  111), null, out wasAdded);
       Assert.AreEqual( 8, obj3.Order );
       Assert.IsFalse( wasAdded );

       Assert.IsTrue( object.ReferenceEquals( obj1, obj3 ) );


       var ordered = reg.OrderedValues.ToArray();
       Assert.AreEqual(2, ordered.Length);
       Assert.AreEqual("Yabloko", ordered[0].Name);
       Assert.AreEqual("Apple", ordered[1].Name);
    }


  }
}
