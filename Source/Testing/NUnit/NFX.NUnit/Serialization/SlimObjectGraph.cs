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
using System.IO;

using NUnit.Framework;

using NFX.IO;
using NFX.Environment;
using NFX.Serialization.Slim;
using NFX.ApplicationModel;

namespace NFX.NUnit.Serialization
{
    [TestFixture]
    public class SlimObjectGraph
    { 
        
        
        public class ObjectA
        {
           public ObjectA Another1;
           public ObjectA Another2;
           public ObjectA Another3;
           public ObjectA Another4;
           public ObjectA Another5;
           public ObjectA Another6;
           public ObjectA Another7;
           public ObjectA Another8;
           public ObjectA Another9;
           public ObjectA Another10;

           public int AField;
        }

        public class ObjectB : ObjectA
        {
           public int BField;
        }

        
        
        
        [TestCase]
        public void T01()
        {
          using(var ms = new MemoryStream())
          {           
            var s = new SlimSerializer(SlimFormat.Instance);
            
            var root = new ObjectA(){ AField = -890};
             
            s.Serialize(ms, root);

            ms.Position = 0;


            var deser = s.Deserialize(ms) as ObjectA;

            Assert.IsNotNull( deser );
            Assert.IsTrue( deser.GetType() == typeof(ObjectA));

            Assert.AreEqual(-890, deser.AField);
            Assert.IsNull( deser.Another1 );
            Assert.IsNull( deser.Another2 );
            Assert.IsNull( deser.Another3 );
            Assert.IsNull( deser.Another4 );
            Assert.IsNull( deser.Another5 );
            Assert.IsNull( deser.Another6 );
            Assert.IsNull( deser.Another7 );
            Assert.IsNull( deser.Another8 );
            Assert.IsNull( deser.Another9 );
            Assert.IsNull( deser.Another10 );
          }
        }


        [TestCase]
        public void T02()
        {
          using(var ms = new MemoryStream())
          {           
            var s = new SlimSerializer(SlimFormat.Instance);
            
            var root = new ObjectA
            {
              AField = 2345,
              Another1 = new ObjectA{ AField = 7892}
            };
             
            s.Serialize(ms, root);

            ms.Position = 0;


            var deser = s.Deserialize(ms) as ObjectA;

            Assert.IsNotNull( deser );
            Assert.IsTrue( deser.GetType() == typeof(ObjectA));
            Assert.AreEqual(2345,  deser.AField );

            Assert.IsNotNull( deser.Another1 );
            Assert.AreEqual(7892,  deser.Another1.AField );
            Assert.IsNull( deser.Another2 );
            Assert.IsNull( deser.Another3 );
            Assert.IsNull( deser.Another4 );
            Assert.IsNull( deser.Another5 );
            Assert.IsNull( deser.Another6 );
            Assert.IsNull( deser.Another7 );
            Assert.IsNull( deser.Another8 );
            Assert.IsNull( deser.Another9 );
            Assert.IsNull( deser.Another10 );

          }
        }


        [TestCase]
        public void T03()
        {
          using(var ms = new MemoryStream())
          {           
            var s = new SlimSerializer(SlimFormat.Instance);
            
            var root = new ObjectA
            {
              AField = 2345,
              Another1 = new ObjectA{ AField = 9001},
              Another2 = new ObjectA{ AField = 9002},
              Another3 = new ObjectA{ AField = 9003},
              Another4 = new ObjectA{ AField = 9004},
              Another5 = new ObjectA{ AField = 9005},
              Another6 = new ObjectA{ AField = 9006},
              Another7 = new ObjectA{ AField = 9007},
              Another8 = new ObjectA{ AField = 9008},
              Another9 = new ObjectA{ AField = 9009},
              Another10 = new ObjectA{ AField = 9010},
            };
             
            s.Serialize(ms, root);

            ms.Position = 0;


            var deser = s.Deserialize(ms) as ObjectA;

            Assert.IsNotNull( deser );
            Assert.IsTrue( deser.GetType() == typeof(ObjectA));
            Assert.AreEqual(2345,  deser.AField );

            Assert.IsNotNull( deser.Another1 );
            Assert.IsNotNull( deser.Another1 );
            Assert.IsNotNull( deser.Another2 );
            Assert.IsNotNull( deser.Another3 );
            Assert.IsNotNull( deser.Another4 );
            Assert.IsNotNull( deser.Another5 );
            Assert.IsNotNull( deser.Another6 );
            Assert.IsNotNull( deser.Another7 );
            Assert.IsNotNull( deser.Another8 );
            Assert.IsNotNull( deser.Another9 );
            Assert.IsNotNull( deser.Another10 );

             Assert.AreEqual(9001,  deser.Another1.AField );
             Assert.AreEqual(9002,  deser.Another2.AField );
             Assert.AreEqual(9003,  deser.Another3.AField );
             Assert.AreEqual(9004,  deser.Another4.AField );
             Assert.AreEqual(9005,  deser.Another5.AField );
             Assert.AreEqual(9006,  deser.Another6.AField );
             Assert.AreEqual(9007,  deser.Another7.AField );
             Assert.AreEqual(9008,  deser.Another8.AField );
             Assert.AreEqual(9009,  deser.Another9.AField );
             Assert.AreEqual(9010,  deser.Another10.AField );
          }
        }


        [TestCase]
        public void T04()
        {
          using(var ms = new MemoryStream())
          {           
            var s = new SlimSerializer(SlimFormat.Instance);
            
            var root = new ObjectA
            {
              AField = 2345,
              Another1 = new ObjectA{ AField = 7892},
              Another2 = new ObjectB{ AField = 5678, BField = -12}
            };
             
            s.Serialize(ms, root);

            ms.Position = 0;


            var deser = s.Deserialize(ms) as ObjectA;

            Assert.IsNotNull( deser );
            Assert.IsTrue( deser.GetType() == typeof(ObjectA));
            Assert.AreEqual(2345,  deser.AField );

            Assert.IsNotNull( deser.Another1 );
            Assert.AreEqual(7892,  deser.Another1.AField );
           
            Assert.IsNotNull( deser.Another2 );
            Assert.AreEqual(5678,  deser.Another2.AField );

            Assert.IsTrue( deser.Another2.GetType() == typeof(ObjectB));
            Assert.AreEqual(-12,  ((ObjectB)deser.Another2).BField );

            Assert.IsNull( deser.Another3 );
            Assert.IsNull( deser.Another4 );
            Assert.IsNull( deser.Another5 );
            Assert.IsNull( deser.Another6 );
            Assert.IsNull( deser.Another7 );
            Assert.IsNull( deser.Another8 );
            Assert.IsNull( deser.Another9 );
            Assert.IsNull( deser.Another10 );

          }
        }



        [TestCase]
        public void T05()
        {
          using(var ms = new MemoryStream())
          {           
            var s = new SlimSerializer(SlimFormat.Instance);
            
            var root = new ObjectA();
            
             root.AField = 2345;
             root.Another1 = new ObjectA{ AField = 27892};
             root.Another2 = new ObjectB{ AField = -278, BField = -12, Another1 = root};
             
            s.Serialize(ms, root);

            ms.Position = 0;


            var deser = s.Deserialize(ms) as ObjectA;

            Assert.IsNotNull( deser );
            Assert.IsTrue( deser.GetType() == typeof(ObjectA));
            Assert.AreEqual(2345,  deser.AField );

            Assert.IsNotNull( deser.Another1 );
            Assert.IsTrue( deser.Another1.GetType() == typeof(ObjectA));
            Assert.AreEqual(27892,  deser.Another1.AField );
           
            Assert.IsNotNull( deser.Another2 );
            Assert.AreEqual(-278,  deser.Another2.AField );

            Assert.IsTrue( deser.Another2.GetType() == typeof(ObjectB));
            Assert.AreEqual(-12,  ((ObjectB)deser.Another2).BField );

            Assert.IsNotNull( deser.Another2.Another1 );
            Assert.IsTrue( object.ReferenceEquals(deser, deser.Another2.Another1));
            Assert.IsTrue( deser.Another2.GetType() == typeof(ObjectB));

            Assert.IsNull( deser.Another3 );
            Assert.IsNull( deser.Another4 );
            Assert.IsNull( deser.Another5 );
            Assert.IsNull( deser.Another6 );
            Assert.IsNull( deser.Another7 );
            Assert.IsNull( deser.Another8 );
            Assert.IsNull( deser.Another9 );
            Assert.IsNull( deser.Another10 );

          }
        }



         [TestCase]
        public void T06()
        {
          using(var ms = new MemoryStream())
          {           
            var s = new SlimSerializer(SlimFormat.Instance);
            
            var root = new ObjectA[3];

            root[0] = null;

            root[1] = new ObjectA();
            root[2] = new ObjectB();

             root[1].AField = 2345;
             root[1].Another1 = new ObjectA{ AField = 27892};
             root[1].Another2 = new ObjectB{ AField = -278, BField = -12, Another1 = root[1]};

             root[2].AField = 2345;
             ((ObjectB)root[2]).BField = 900333;
             root[2].Another1 = new ObjectA{ AField = 8000000};
             root[2].Another2 = new ObjectB{ AField = -278, BField = -1532, Another1 = root[2]};
             root[2].Another9 = root[1];
             
            s.Serialize(ms, root);

            ms.Position = 0;


            var deser = s.Deserialize(ms) as ObjectA[];

            Assert.IsNotNull( deser );
            Assert.IsTrue( deser.GetType() == typeof(ObjectA[]));
            Assert.AreEqual( 3, deser.Length);
            Assert.IsNull(deser[0]);
            Assert.IsNotNull(deser[1]);
            Assert.IsNotNull(deser[2]);
            

            Assert.AreEqual(2345,  deser[1].AField );
            Assert.IsNotNull( deser[1].Another1 );
            Assert.IsTrue( deser[1].Another1.GetType() == typeof(ObjectA));
            Assert.AreEqual(27892,  deser[1].Another1.AField );
           
            Assert.IsNotNull( deser[1].Another2 );
            Assert.AreEqual(-278,  deser[1].Another2.AField );

            Assert.IsTrue( deser[1].Another2.GetType() == typeof(ObjectB));
            Assert.AreEqual(-12,  ((ObjectB)deser[1].Another2).BField );

            Assert.IsNotNull( deser[1].Another2.Another1 );
            Assert.IsTrue( object.ReferenceEquals(deser[1], deser[1].Another2.Another1));
            Assert.IsTrue( deser[1].Another2.GetType() == typeof(ObjectB));

            Assert.IsNull( deser[1].Another3 );
            Assert.IsNull( deser[1].Another4 );
            Assert.IsNull( deser[1].Another5 );
            Assert.IsNull( deser[1].Another6 );
            Assert.IsNull( deser[1].Another7 );
            Assert.IsNull( deser[1].Another8 );
            Assert.IsNull( deser[1].Another9 );
            Assert.IsNull( deser[1].Another10 );





            Assert.AreEqual(2345,  deser[2].AField );
            Assert.AreEqual(900333,  ((ObjectB)deser[2]).BField );
            Assert.IsNotNull( deser[2].Another1 );
            Assert.IsTrue( deser[2].Another1.GetType() == typeof(ObjectA));
            Assert.AreEqual(8000000,  deser[2].Another1.AField );
           
            Assert.IsNotNull( deser[2].Another2 );
            Assert.AreEqual(-278,  deser[2].Another2.AField );

            Assert.IsTrue( deser[2].Another2.GetType() == typeof(ObjectB));
            Assert.AreEqual(-1532,  ((ObjectB)deser[2].Another2).BField );

            Assert.IsNotNull( deser[2].Another2.Another1 );
            Assert.IsTrue( object.ReferenceEquals(deser[2], deser[2].Another2.Another1));
            Assert.IsTrue( deser[2].Another2.GetType() == typeof(ObjectB));

            Assert.IsNull( deser[2].Another3 );
            Assert.IsNull( deser[2].Another4 );
            Assert.IsNull( deser[2].Another5 );
            Assert.IsNull( deser[2].Another6 );
            Assert.IsNull( deser[2].Another7 );
            Assert.IsNull( deser[2].Another8 );
            Assert.IsNotNull( deser[2].Another9 );
            Assert.IsNull( deser[2].Another10 );

            Assert.IsTrue( object.ReferenceEquals(deser[1], deser[2].Another9) );


          }
        }
       

    }
}
