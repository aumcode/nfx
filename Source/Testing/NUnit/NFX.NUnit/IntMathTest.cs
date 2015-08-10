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

namespace NFX.NUnit
{
    [TestFixture]
    public class IntMathTest
    {
        [TestCase]
        public void IntMathPower()
        {
            Assert.AreEqual(1024,   IntMath.Pow(2, 10));
            Assert.AreEqual(2187,   IntMath.Pow(3, 7));
            Assert.AreEqual(390625, IntMath.Pow(5, 8));
            Assert.AreEqual(0,      IntMath.Pow(0, 0));
            Assert.AreEqual(0,      IntMath.Pow(0, 1));
            Assert.AreEqual(1,      IntMath.Pow(1, 0));
            Assert.AreEqual(1,      IntMath.Pow(100, 0));
            Assert.AreEqual(100,    IntMath.Pow(100, 1));
        }

        [TestCase]
        public void IntMathLog()
        {
            Assert.AreEqual(10, IntMath.Log(1024, 2));
            Assert.AreEqual(2, IntMath.Log(9, 3));
            Assert.AreEqual(2, IntMath.Log(11, 3));
            Assert.AreEqual(1, IntMath.Log(2, 2));
            Assert.AreEqual(0, IntMath.Log(1, 2));
            Assert.Throws<NFXException>(delegate { IntMath.Log(0, 2); });

            Assert.AreEqual(62, IntMath.Log(1L << 62, 2));
            Assert.AreEqual(32, IntMath.Log(1L << 32, 2));
            Assert.AreEqual(10, IntMath.Log(1024, 2));
            Assert.AreEqual(4,  IntMath.Log(16, 2));
            Assert.AreEqual(3,  IntMath.Log(8, 2));
            Assert.AreEqual(1,  IntMath.Log(2, 2));
            Assert.AreEqual(0,  IntMath.Log(1, 2));
        }

        [TestCase]
        public void IntMathUpperPower()
        {
            Assert.AreEqual(1024,   IntMath.UpperPow(1024, 2));
            Assert.AreEqual(9,      IntMath.UpperPow(9, 3));
            Assert.AreEqual(27,     IntMath.UpperPow(11, 3));
            Assert.AreEqual(16,     IntMath.UpperPow(14, 2));
            Assert.AreEqual(16,     IntMath.UpperPow(15, 2));
            Assert.AreEqual(16,     IntMath.UpperPow(16, 2));
            Assert.AreEqual(4,      IntMath.UpperPow(3, 2));
        }

        [TestCase]
        public void MinMaxTest()
        {
            Assert.AreEqual(1,  IntMath.MinMax(-1, 1, 3));
            Assert.AreEqual(2,  IntMath.MinMax(2, 2, 3));
            Assert.AreEqual(3,  IntMath.MinMax(2, 3, 3));
            Assert.AreEqual(-1, IntMath.MinMax(-1, -10, 3));
            Assert.AreEqual(3,  IntMath.MinMax(-1, 5, 3));
            Assert.Throws<NFXException>(() => IntMath.MinMax(10, 5, 3));
        }


        [TestCase]
        public void Align8_int()
        {
            Assert.AreEqual(8,    IntMath.Align8(3));
            Assert.AreEqual(40,   IntMath.Align8(33));
            Assert.AreEqual(0,    IntMath.Align8(0));
            Assert.AreEqual(8,    IntMath.Align8(8));
            Assert.AreEqual(16,   IntMath.Align8(16));
            Assert.AreEqual(24,   IntMath.Align8(17));
            Assert.AreEqual(128,  IntMath.Align8(127));
            Assert.AreEqual(128,  IntMath.Align8(128));
            Assert.AreEqual(136,  IntMath.Align8(129));

            Assert.AreEqual(0,   IntMath.Align8(-5));
            Assert.AreEqual(-8,  IntMath.Align8(-10));
        }

        [TestCase]
        public void Align8_long()
        {
            Assert.AreEqual(8,    IntMath.Align8(3L));
            Assert.AreEqual(40,   IntMath.Align8(33L));
            Assert.AreEqual(0,    IntMath.Align8(0L));
            Assert.AreEqual(8,    IntMath.Align8(8L));
            Assert.AreEqual(16,   IntMath.Align8(16L));
            Assert.AreEqual(24,   IntMath.Align8(17L));
            Assert.AreEqual(128,  IntMath.Align8(127L));
            Assert.AreEqual(128,  IntMath.Align8(128L));
            Assert.AreEqual(136,  IntMath.Align8(129L));

            Assert.AreEqual(0,   IntMath.Align8(-5L));
            Assert.AreEqual(-8,  IntMath.Align8(-10L));
        }

        [TestCase]
        public void Align16_int()
        {
            Assert.AreEqual(16,    IntMath.Align16(3));
            Assert.AreEqual(48,    IntMath.Align16(33));
            Assert.AreEqual(0,     IntMath.Align16(0));
            Assert.AreEqual(16,    IntMath.Align16(8));
            Assert.AreEqual(16,    IntMath.Align16(16));
            Assert.AreEqual(32,    IntMath.Align16(17));
            Assert.AreEqual(128,   IntMath.Align16(127));
            Assert.AreEqual(128,   IntMath.Align16(128));
            Assert.AreEqual(144,   IntMath.Align16(129));

            Assert.AreEqual(0,     IntMath.Align16(-5));
            Assert.AreEqual(-16,   IntMath.Align16(-17));
        }

        [TestCase]
        public void Align16_long()
        {
            Assert.AreEqual(16,    IntMath.Align16(3L));
            Assert.AreEqual(48,    IntMath.Align16(33L));
            Assert.AreEqual(0,     IntMath.Align16(0L));
            Assert.AreEqual(16,    IntMath.Align16(8L));
            Assert.AreEqual(16,    IntMath.Align16(16L));
            Assert.AreEqual(32,    IntMath.Align16(17L));
            Assert.AreEqual(128,   IntMath.Align16(127L));
            Assert.AreEqual(128,   IntMath.Align16(128L));
            Assert.AreEqual(144,   IntMath.Align16(129L));

            Assert.AreEqual(0,    IntMath.Align16(-5L));
            Assert.AreEqual(-16,  IntMath.Align16(-17L));
        }

        [TestCase]
        public void IsPrime()
        {
            Assert.IsTrue( IntMath.IsPrime( 2 ) );
            Assert.IsTrue( IntMath.IsPrime( 3 ) );
            Assert.IsTrue( IntMath.IsPrime( 7 ) );
            Assert.IsTrue( IntMath.IsPrime( 239 ) );
            Assert.IsTrue( IntMath.IsPrime( 62851 ) );
            Assert.IsTrue( IntMath.IsPrime( 7199369 ) );

            Assert.IsFalse( IntMath.IsPrime( -1 ) );
            Assert.IsFalse( IntMath.IsPrime( 0 ) );
            Assert.IsFalse( IntMath.IsPrime( 1 ) );
            Assert.IsFalse( IntMath.IsPrime( 4 ) );
            Assert.IsFalse( IntMath.IsPrime( 6 ) );
            Assert.IsFalse( IntMath.IsPrime( 8 ) );
            Assert.IsFalse( IntMath.IsPrime( 9 ) );
            Assert.IsFalse( IntMath.IsPrime( 10 ) );
            Assert.IsFalse( IntMath.IsPrime( 20 ) );
            Assert.IsFalse( IntMath.IsPrime( 120 ) );
            Assert.IsFalse( IntMath.IsPrime( 1000 ) );

        }

        [TestCase]
        public void GetAdjacentPrimeNumberLessThan_1()
        {
           Assert.AreEqual(2, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(-10));
           Assert.AreEqual(2, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(0));
           Assert.AreEqual(2, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(1));
           Assert.AreEqual(2, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(2));
           Assert.AreEqual(3, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(3));

           Assert.AreEqual(3, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(4));
           Assert.AreEqual(5, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(5));
           Assert.AreEqual(5, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(6));
           Assert.AreEqual(7, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(7));

           Assert.AreEqual(107, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(107));
           Assert.AreEqual(107, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(108));
           Assert.AreEqual(109, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(109));
           Assert.AreEqual(109, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(110));
           Assert.AreEqual(109, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(111));
           Assert.AreEqual(109, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(112));
           Assert.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(113));
           Assert.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(114));
           Assert.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(115));
           Assert.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(116));
           Assert.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(117));
           Assert.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(118));
           Assert.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(119));
           Assert.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(120));
           Assert.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(121));
           Assert.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(122));
           Assert.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(123));
           Assert.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(124));
           Assert.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(125));
           Assert.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(126));
           Assert.AreEqual(127, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(127));


           Assert.AreEqual(631, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(633));

           Assert.AreEqual(2459, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(2465));

           Assert.AreEqual(1148747, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(1148747));
           Assert.AreEqual(1148747, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(1148748));
           Assert.AreEqual(1148747, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(1148752));
           Assert.AreEqual(1148753, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(1148753));

           Assert.AreEqual(15485857, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(15485862));
           Assert.AreEqual(15485863, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(15485863));
        }

        [TestCase]
        public void GetAdjacentPrimeNumberLessThan_2()
        {
          for(var i=2; i<1000000; i++)
           Assert.IsTrue( IntMath.IsPrime( IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(i) ) );
        }

         [TestCase]
        public void PRIME_CAPACITIES()
        {
          foreach(var capacity in IntMath.PRIME_CAPACITIES)
           Assert.IsTrue( IntMath.IsPrime( capacity ) );
        }

        [TestCase]
        public void GetPrimeCapacityOfAtLeast_1()
        {
          Assert.AreEqual(7  , IntMath.GetPrimeCapacityOfAtLeast( 4 ));    
          Assert.AreEqual(59  , IntMath.GetPrimeCapacityOfAtLeast( 48 ));    
          Assert.AreEqual(71  , IntMath.GetPrimeCapacityOfAtLeast( 64 )); 
          
           Assert.AreEqual(131  , IntMath.GetPrimeCapacityOfAtLeast( 128 ));    
           Assert.AreEqual(131  , IntMath.GetPrimeCapacityOfAtLeast( 129 ));    
           Assert.AreEqual(131  , IntMath.GetPrimeCapacityOfAtLeast( 130 ));    
           Assert.AreEqual(131  , IntMath.GetPrimeCapacityOfAtLeast( 131 ));    
           Assert.AreEqual(163  , IntMath.GetPrimeCapacityOfAtLeast( 132 ));
           
          Assert.AreEqual(672827  , IntMath.GetPrimeCapacityOfAtLeast( 672800 ));    

          Assert.AreEqual(334231259  , IntMath.GetPrimeCapacityOfAtLeast( 334231259  ));    
          Assert.AreEqual(334231291  , IntMath.GetPrimeCapacityOfAtLeast( 334231260  ));    
        }

        [TestCase]
        public void GetPrimeCapacityOfAtLeast_2()
        {
          for(var i=2; i<1000000; i++)
          {
           var cap = IntMath.GetPrimeCapacityOfAtLeast(i);
           Assert.IsTrue( cap >= i);
           Assert.IsTrue( cap < i*2);
           Assert.IsTrue( IntMath.IsPrime( cap ) );
          }
        }

        [TestCase]
        public void GetCapacityFactoredToPrime_1()
        {
          Assert.AreEqual(11  , IntMath.GetCapacityFactoredToPrime( 4, 2d ) );
          Assert.AreEqual(37  , IntMath.GetCapacityFactoredToPrime( 16, 2d ) );
          Assert.AreEqual(59  , IntMath.GetCapacityFactoredToPrime( 16, 3d ) );

          Assert.AreEqual(521  , IntMath.GetCapacityFactoredToPrime( 256, 2d ) );

          Assert.AreEqual(2333  , IntMath.GetCapacityFactoredToPrime( 1024, 2d ) );

          Assert.AreEqual(521  , IntMath.GetCapacityFactoredToPrime( 1024, 0.5d ) );
          Assert.AreEqual(293  , IntMath.GetCapacityFactoredToPrime( 1024, 0.25d ) );

          Assert.AreEqual(2411033  , IntMath.GetCapacityFactoredToPrime( 1024 * 1024, 2d) );
          Assert.AreEqual(16777259  , IntMath.GetCapacityFactoredToPrime( 8 * 1024 * 1024, 2d) );
          Assert.AreEqual(33554467  , IntMath.GetCapacityFactoredToPrime( 16 * 1024 * 1024, 2d) );
        }

        [TestCase]
        public void GetCapacityFactoredToPrime_2()
        {
          int cap = 16;
          while(cap<2000000000)
          {
            cap = IntMath.GetCapacityFactoredToPrime(cap, 2d);
            Console.WriteLine(cap);
            Assert.IsTrue( IntMath.IsPrime( cap ) );
          }
        }
    }
}
