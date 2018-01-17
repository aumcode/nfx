/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
using System.Text;

namespace NFX.Media.PDF.Text
{
  /// <summary>
  /// Unicode strings encoding.
  /// </summary>
  internal sealed class PdfUnicodeEncoding : Encoding
  {
    public override int GetByteCount(char[] chars, int index, int count)
    {
      return count * 2;
    }

    public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
      for (int count = charCount; count > 0; charIndex++, count--)
      {
        char ch = chars[charIndex];
        bytes[byteIndex++] = (byte)(ch >> 8);
        bytes[byteIndex++] = (byte)ch;
      }

      return charCount * 2;
    }

    public override int GetCharCount(byte[] bytes, int index, int count)
    {
      return count / 2;
    }

    public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
    {
      for (int count = byteCount; count > 0; byteIndex += 2, charIndex++, count--)
      {
        chars[charIndex] = (char)((int)bytes[byteIndex] << 8 + (int)bytes[byteIndex + 1]);
      }

      return byteCount;
    }

    public override int GetMaxByteCount(int charCount)
    {
      return charCount * 2;
    }

    public override int GetMaxCharCount(int byteCount)
    {
      return byteCount / 2;
    }
  }
}