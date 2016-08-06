
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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 1.0  2013.12.18
 * Author: Denis Latushkin<dxwizard@gmail.com>
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using NFX;
using NFX.Environment;
using System.Collections;

namespace NFX.Collections
{
  /// <summary>
  /// Provides bit array with automatic resizing
  /// </summary>
  public class BitList: IEquatable<BitList>
  {
    #region .ctor

      public BitList(int initSize = 0)
      {
        m_BitArray = new BitArray(initSize);
        m_Size = initSize;
      }

    #endregion

    #region Pvt/Prot/Int Fields

      private BitArray m_BitArray;
      private int m_Size;

    #endregion

    #region Properties

      public int Size { get { return m_Size; } }

      public int ByteSize { get { return  (Size + 7) >> 3; } }

      public bool this[int i]
      {
         get { return m_BitArray[i]; }
         set { m_BitArray[i] = value; }
      }

    #endregion

    #region Public

      public void AppendBit(bool bit)
      {
        ensureCapacity(++m_Size);
        m_BitArray[m_Size-1] = bit;
      }

      public void AppendBits(int value, int numBits)
      {
        if (numBits < 0 || numBits >= 8 * sizeof(int))
          throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".AppendBits(numBits>=0|<8*sizeof(int))");

        ensureCapacity(m_Size + numBits);
        for (int leftBitIndex = numBits; leftBitIndex > 0; leftBitIndex--)
        {
          int bitValue = (value >> (leftBitIndex - 1)) & 0x01;
          AppendBit(bitValue == 1);
        }
      }

      public void AppendBitList(BitList other)
      {
        ensureCapacity( m_Size + other.Size);
        for (int i = 0; i < other.m_Size; i++)
          AppendBit(other.m_BitArray[i]);
      }

      public void Xor(BitList other)
      {
        if (m_BitArray.Length != other.m_BitArray.Length)
          throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".Xor(other.Length!=this.Length)");

        m_BitArray.Xor(other.m_BitArray);
      }

      public void GetBytes(byte[] buf, int bitOffset, int offset, int numBytes)
      {
        for (int i = 0; i < numBytes; i++)
         {
            int theByte = 0;
            for (int j = 0; j < 8; j++)
            {
               if (this[bitOffset])
               {
                  theByte |= 1 << (7 - j);
               }
               bitOffset++;
            }
            buf[offset + i] = (byte)theByte;
         }
      }

    #endregion

    #region Protected

      public override string ToString()
      {
        StringBuilder b = new StringBuilder();

        for (int i = 0; i < m_Size; i++)
        {
          if (i > 0 && i % 8 == 0)
            b.Append(' ');

          bool elem = m_BitArray[i];

          b.Append(elem ? '1' : '0');
        }

        return b.ToString();
      }

      public override int GetHashCode()
      {
        return m_BitArray.GetHashCode();
      }

      public bool Equals(BitList otherObj)
      {
        return m_BitArray.Equals( otherObj.m_BitArray);
      }

    #endregion

    #region .pvt. impl.

      private void ensureCapacity(int size)
      {
        if (size >= m_BitArray.Length)
        {
          int newLength = 1 << (int)Math.Ceiling( Math.Log(size, 2));
          m_BitArray.Length = newLength;
        }
      }

    #endregion

  }//class

}
