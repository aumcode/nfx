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

namespace NFX
{
    public static class IntMath
    {
        public static int Log(long n, int logBase = 2)
        {
            if (n == 0) throw new NFXException(StringConsts.ARGUMENT_ERROR + "n == 0");
            if (n == 1) return 0;
            int result = 0;
            for (; n > 1; n = n / logBase, result++);
            return result;
        }

        public static long Pow(long n, int power)
        {
            if (n == 0) return 0;
            if (power == 0) return 1;
            long result = 1;
            for (; power > 0; result *= n, power--);
            return result;
        }

        public static int UpperLog(long n, int logBase = 2)
        {
            int k = Log(n, logBase);
            return Pow(k, logBase) == n ? k : k + 1;
        }

        public static long UpperPow(int n, int powBase = 2)
        {
            int log = Log(n, powBase);
            long pow = Pow(powBase, log);
            return pow == n ? n : pow * powBase;
        }

        public static int MinMax(int min, int value, int max)
        {
            if (min > max) throw new NFXException(StringConsts.ARGUMENT_ERROR + "MinMax(min > max)");
            if (value < min) value = min;
            if (value > max) value = max;
            return value;
        }

        /// <summary>
        /// Returns argument increased to the nearest number divisible by 8
        /// </summary>
        public static int Align8(int i)
        {
          var r = i & 7; // 00000111
          return r==0 ? i : i + (8 - r);
        }

        /// <summary>
        /// Returns argument increased to the nearest number divisible by 16
        /// </summary>
        public static int Align16(int i)
        {
          var r = i & 15;// 00001111
          return r==0 ? i : i + (16 - r);
        }

        /// <summary>
        /// Returns argument increased to the nearest number divisible by 8
        /// </summary>
        public static long Align8(long i)
        {
          var r = i & 7; // 00000111
          return r==0 ? i : i + (8 - r);
        }

        /// <summary>
        /// Returns argument increased to the nearest number divisible by 16
        /// </summary>
        public static long Align16(long i)
        {
          var r = i & 15;// 00001111
          return r==0 ? i : i + (16 - r);
        }

        /// <summary>
        /// Tests if the number is prime
        /// </summary>
        public static bool IsPrime(int number)
        {
          if (number<2) return false;
          //1 parity case
          if ((number & 1)==0)//divisible by 2
           return number==2;//only 2 is prime, other numbers divisible by 2 are not

          for(var i=3; (i*i) <= number; i+=2)//check odd numbers starting from 3 (+2 to check only odd numbers, as even are already covered)
           if ((number % i) == 0) return false;

          return number!=1;
        }

        /// <summary>
        /// Gets adjacent prime number which is less than or equal to the specified number. Arguments less than 3 yield 2;
        /// </summary>
        public static int GetAdjacentPrimeNumberLessThanOrEqualTo(int number)
        {
          if (number<3) return 2;
          if (IsPrime(number)) return number;

          var odd = (number & 1) != 0;

          for(var i = odd ? number-2 : number-1; i>=3; i-=2)
          {
            var iprime = true;
            for(var j=3; (j*j) <= i; j+=2)//check odd numbers starting from 3 (+2 to check only odd numbers, as even are already covered)
             if ((i % j) == 0)
             {
              iprime=false;
              break;
             }

            if (iprime) return i;
          }

          return 2;
        }


        public static readonly int[] PRIME_CAPACITIES = new int[]
        {
            3, 7, 11, 17, 23, 29, 37,
            47, 59, 71, 89, 107, 131,
            163, 197, 239, 293, 353,
            431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931,
            2333, 2801, 3371, 4049,
            4861, 5839, 7013, 8419,
            10103, 12143, 14591, 17519,
            21023, 25229, 30293, 36353,
            43627, 52361, 62851, 75431,
            90523, 108631, 130363,
            156437, 187751, 225307,
            270371, 324449, 389357,
            467237, 560689, 672827,
            807403, 968897, 1162687,
            1395263, 1674319, 2009191,
            2411033, 2893249, 3471899,
            4166287, 4999559, 5999471,
            7199369, 8900029, 10700023,
            13100023, 16300007
        };

        /// <summary>
        /// Gets the capacity of at least or larger larger than the specified number. For numbers &lt;= 16300007 the function uses lookup table for speed
        /// </summary>
        public static int GetPrimeCapacityOfAtLeast(int capacity)
        {
          if (capacity<=0)
            throw new NFXException(StringConsts.ARGUMENT_ERROR+"GetPrimeCapacityOfAtLeast(capacity<=0)");

          for (var i=0; i < PRIME_CAPACITIES.Length; i++)
          {
             int prime = PRIME_CAPACITIES[i];
             if (prime >= capacity) return prime;
          }

          for (var i=capacity|1; i < int.MaxValue; i+=2)
          {
             if (IsPrime(i)) return i;
          }
          return capacity;
        }

        /// <summary>
        /// Increases a capacity to a prime number by the factor. Both numbers must be positive
        /// </summary>
        public static int GetCapacityFactoredToPrime(int capacity, double factor)
        {
          if (capacity<0 || factor<0d)
            throw new NFXException(StringConsts.ARGUMENT_ERROR+"GetCapacityFactoredToPrime(capacity|factor<0|1.0d)");

          var newCapacity = capacity * factor;
          if (newCapacity>int.MaxValue)
           capacity = 2146435069;
          else
           capacity = (int)newCapacity;

          return GetPrimeCapacityOfAtLeast(capacity);
        }


    }
}
