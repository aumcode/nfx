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