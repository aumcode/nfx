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
using NFX.Serialization.Slim;
using NFX.ApplicationModel;

namespace NFX.NUnit.Serialization
{
    [TestFixture]
    public class ArrayWalking
    { 

        [TestCase]
        public void Utils_WalkArrayWrite_1D()
        {
             
            var arr1 = new object[25];
                        
            var cnt = 0;
            NFX.Serialization.SerializationUtils.WalkArrayWrite(arr1, elm => cnt++);
            

            Assert.AreEqual(arr1.Length, cnt);
        }

         [TestCase]
        public void Utils_WalkArrayRead_1D()
        {
             
            var arr1 = new object[25];
                        
            var cnt = 0;
            NFX.Serialization.SerializationUtils.WalkArrayRead(arr1, () => cnt++);
            

            Assert.AreEqual(arr1.Length, cnt);
        }

        [TestCase]
        public void Utils_WalkArrayWrite_2D_1()
        {
             
            var arr1 = new object[2,25];
                        
            var cnt = 0;
            NFX.Serialization.SerializationUtils.WalkArrayWrite(arr1, elm => cnt++);
            
            Assert.AreEqual(50, cnt);
            Assert.AreEqual(arr1.Length, cnt);
        }

        [TestCase]
        public void Utils_WalkArrayRead_2D_1()
        {
             
            var arr1 = new object[2,25];
                        
            var cnt = 0;
            NFX.Serialization.SerializationUtils.WalkArrayRead(arr1, () => cnt++);
            
            Assert.AreEqual(50, cnt);
            Assert.AreEqual(arr1.Length, cnt);
        }


        [TestCase]
        public void Utils_WalkArrayWrite_2D_2()
        {
             
            var arr1 = new object[25, 2];
                        
            var cnt = 0;
            NFX.Serialization.SerializationUtils.WalkArrayWrite(arr1, elm => cnt++);
            

            Assert.AreEqual(50, cnt);
            Assert.AreEqual(arr1.Length, cnt);
        }


        [TestCase]
        public void Utils_WalkArrayRead_2D_2()
        {
             
            var arr1 = new object[25, 2];
                        
            var cnt = 0;
            NFX.Serialization.SerializationUtils.WalkArrayRead(arr1, () => cnt++);
            

            Assert.AreEqual(50, cnt);
            Assert.AreEqual(arr1.Length, cnt);
        }





        [TestCase]
        public void Utils_WalkArrayWrite_3D()
        {
             
            var arr1 = new object[8,2,4];
                        
            var cnt = 0;
            NFX.Serialization.SerializationUtils.WalkArrayWrite(arr1, elm => cnt++);
            
            
            Assert.AreEqual(arr1.Length, cnt);
            Assert.AreEqual(64, cnt);
        }

        [TestCase]
        public void Utils_WalkArrayRead_3D()
        {
             
            var arr1 = new object[8,2,4];
                        
            var cnt = 0;
            NFX.Serialization.SerializationUtils.WalkArrayRead<object>(arr1, () => cnt++ );
            

            Assert.AreEqual(arr1.Length, cnt);
            Assert.AreEqual(64, cnt);
        }


         [TestCase]
        public void Utils_WalkArrayWrite_4D()
        {
             
            var arr1 = new object[8,2,4,10];
                        
            var cnt = 0;
            NFX.Serialization.SerializationUtils.WalkArrayWrite(arr1, elm => cnt++);
            
            
            Assert.AreEqual(arr1.Length, cnt);
            Assert.AreEqual(640, cnt);
        }

        [TestCase]
        public void Utils_WalkArrayRead_4D()
        {
             
            var arr1 = new object[8,2,4,10];
                        
            var cnt = 0;
            NFX.Serialization.SerializationUtils.WalkArrayRead<object>(arr1, () => cnt++ );
            

            Assert.AreEqual(arr1.Length, cnt);
            Assert.AreEqual(640, cnt);
        }




    }
}
