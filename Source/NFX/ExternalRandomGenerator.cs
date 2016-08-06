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
using System.Threading;

namespace NFX
{
  /// <summary>
  /// Represents a random generator which is based on System.Random() yet has an ability to feed external samples into it.
  ///  Use ExtrenalRandomGenerator.Instance to use the default thread-safe instance.
  /// </summary>
  /// <remarks>
  /// Introduces external entropy into the generation sequence by adding a sample into the ring buffer.
  /// Call FeedExternalEntropySample(int sample) method from places that have true entropy values, i.e.
  ///  a network-related code may have good entropy sources in server applications.
  ///  External entropy sources may rely on user-dependent actions, i.e.:
  ///   number of bytes/requests received per second, dollar(or cent remainders) amount of purchases made (on a server),
  ///   zip codes of customers, IP addresses of site visitors, average noise level sampled on an open WAVE device(microphone),
  ///    mouse position (i.e. in GUI app) etc...
  ///  This class MAY be crypto-safe if it is fed a good entropy data at high rate, however that depends on the use pattern.
  ///  The framework implementation feeds some entropy from Glue and cache components infrequently (once every few seconds),
  ///   which is definitely not strong for cryptography
  /// </remarks>
  public sealed class ExternalRandomGenerator
  {
      private static readonly int BUFF_SIZE = 1024 + IntMath.Align8((int)DateTime.Now.Ticks & 0x0fff);

      private static ExternalRandomGenerator s_Instance;

      /// <summary>
      /// Returns the default instance of the generator. This instance is thread-safe
      /// </summary>
      public static ExternalRandomGenerator Instance
      {
        get
        {
          if (s_Instance==null)//no need to lock, 2nd copy is ok
            s_Instance = new ExternalRandomGenerator();
          return s_Instance;
        }
      }

      /// <summary>
      /// Create new instance of ExternalRandomGenerator. Create new instances only if you need to use different sample ring buffers.
      /// In majority of cases use ExternalRandomGenerator.Instance to use default instance instead of creating a new instance.
      /// Default instance is thread-safe for process-wide use
      /// </summary>
      public ExternalRandomGenerator()
      {
        m_Buffer = new int[BUFF_SIZE];
        lock(s_GlobalRandom)
          for(int i=0; i<BUFF_SIZE; i++)
            m_Buffer[i] = s_GlobalRandom.Next(Int32.MinValue, Int32.MaxValue);
      }


      private static Random s_GlobalRandom = new Random();

      [ThreadStatic] private static Random ts_Random;

      private int[] m_Buffer;

      private int m_ReadPosition;
      private int m_WritePosition;

      /// <summary>
      /// Generates next random integer in the Int32.MinValue..Int32.MaxValue diapason
      /// </summary>
      public int NextRandomInteger
      {
        get
        {
          if (ts_Random==null)
           lock(s_GlobalRandom)
            ts_Random = new Random(s_GlobalRandom.Next());

          var position = Interlocked.Increment(ref m_ReadPosition);
          if(position>=BUFF_SIZE)
          {
            position = 0;
            m_ReadPosition = 0;//no need to lock. Its ok if another thread does not see this instantly
          }
          return m_Buffer[position] ^ ts_Random.Next(Int32.MinValue, Int32.MaxValue);
        }
      }

      /// <summary>
      /// Generates next random integer in the Uint32.MinValue..Uint32.MaxValue diapason
      /// </summary>
      public uint NextRandomUnsignedInteger
      {
        get { return (uint)NextRandomInteger;}
      }

      /// <summary>
      /// Generates next random ulong in the Uint64.MinValue..Uint64.MaxValue range
      /// </summary>
      public ulong NextRandomUnsignedLong
      {
        get
        {
          return (((ulong)NextRandomUnsignedInteger) << 32) + (ulong)NextRandomUnsignedInteger;
        }
      }

      /// <summary>
      /// Generates random byte[16] buffer
      /// </summary>
      public byte[] NextRandom16Bytes
      {
        get
        {
          var arr = new byte[16];

          arr.WriteBEInt32(0,  this.NextRandomInteger);
          arr.WriteBEInt32(4,  this.NextRandomInteger);
          arr.WriteBEInt32(8,  this.NextRandomInteger);
          arr.WriteBEInt32(12, this.NextRandomInteger);

          return arr;
        }
      }



      /// <summary>
      /// Returns 0..1 random double
      /// </summary>
      public double NextRandomDouble
      {
        get { return ((uint)NextRandomInteger) / ((double)uint.MaxValue); }
      }


      /// <summary>
      /// Generates random double number in min..max range
      /// </summary>
      public double NextScaledRandomDouble(double bound1, double bound2 = 0)
      {
        var min = bound1<bound2 ? bound1 : bound2;
        var max = bound1>bound2 ? bound1 : bound2;

        var val = NextRandomInteger;

        var ratio = (UInt32)val / (double)UInt32.MaxValue;

        return min + ((max - min) * ratio);
      }

      /// <summary>
      /// Introduces external entropy into the generation sequence by adding a sample into the ring buffer.
      /// Call this method from places that have true entropy values, i.e.
      ///  a network-related code may have good entropy sources in server applications.
      ///  External entropy sources may rely on user-dependent actions, i.e.:
      ///   number of bytes/requests received per second, dollar(or cent remainders) amount of purchases made (on a server),
      ///   zip codes of customers, IP addresses of site visitors, average noise level sampled on an open WAVE device(microphone),
      ///    mouse position (i.e. in GUI app) etc...
      /// </summary>
      public void FeedExternalEntropySample(int sample)
      {
        var position = Interlocked.Increment(ref m_WritePosition);
        if(position>=BUFF_SIZE)
        {
          position = 0;
          m_WritePosition = 0;//no need to lock. Its ok if another thread does not see this instantly
        }
        m_Buffer[position] = sample;
      }

      /// <summary>
      /// Generates random number in min..max range
      /// </summary>
      public int NextScaledRandomInteger(int bound1, int bound2 = 0)
      {
        var min = bound1<bound2 ? bound1 : bound2;
        var max = bound1>bound2 ? bound1 : bound2;

        var val = NextRandomInteger;

        var ratio = (UInt32)val / (double)UInt32.MaxValue;

        return min + (int)((max - min) * ratio);
      }


                  private static readonly char[] CHAR_DICT = new char[]
                  {
                    'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z', //26
                    'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z', //26  52
                    '0','1','2','3','4','5','6','7','8','9','-','_', //12 64
                    'A','Z','T','W','7','3','9', 'q', '-', 'r', 'x', //12 76
                    'j','z','R'                                      //3  79 (prime)

                  };

                  private static readonly int CHAR_DICT_LEN = CHAR_DICT.Length;


      /// <summary>
      /// Generates a random string of chars which are safe for the use on the web -
      ///  a string that only contains "a-z"/"A-Z" and "0-9" and "-"/"_" chars, i.e.: "bo7O0EFasZe-wEty9w0__JiOKk81".
      ///  The length of the string can not be less than 4 and more than 1024 chars
      /// </summary>
      public string NextRandomWebSafeString(int minLength = 16, int maxLength = 32)
      {
        const int MIN_LEN = 4;
        const int MAX_LEN = 1024;

        if (minLength<MIN_LEN) minLength = MIN_LEN;
        if (maxLength>MAX_LEN) maxLength = MAX_LEN;

        var count = minLength;
        if (maxLength>minLength) count += this.NextScaledRandomInteger(0, maxLength - minLength);

        var result = new StringBuilder();

        for(var i=0; i<count; i++)
          result.Append( CHAR_DICT[ (this.NextRandomInteger & CoreConsts.ABS_HASH_MASK) % CHAR_DICT_LEN ]);


        return result.ToString();
      }




  }


}
