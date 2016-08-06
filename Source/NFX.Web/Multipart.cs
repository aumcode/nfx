using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NFX;
using NFX.Serialization.JSON;

namespace NFX.Web
{
  /// <summary>
  /// Faciliates working with multipart encoding
  /// </summary>
  public sealed class Multipart
  {
    #region Consts

      private const byte CR = 0x0D;
      private const byte LF = 0x0A;
      private const byte HYPHEN = 0x2D;
      private static readonly byte[] EOL = new byte[] {CR, LF};
      private static readonly byte[] DOUBLE_EOL = new byte[] {CR, LF, CR, LF};
      private static readonly byte[] DOUBLE_HYPHEN = {HYPHEN, HYPHEN};
      private const string DOUBLE_HYPHEN_STR = "--";

      private const string POSTFIX_CONTENT_TYPE = "_contenttype";
      private const string POSTFIX_FILENAME = "_filename";
      private const string BOUNDARY_PREFIX = "----------";
      private const string BOUNDARY_HEADER_START = "multipart/form-data; boundary=";

      private static readonly string[] NEW_LINE_SPLIT = { "\r\n" };
      private static readonly char[] SEMICOLON_SPLIT = new char[] { ';' };
      private static readonly char[] EQUALS_SPLIT = new char[] { '=' };
      private static readonly char[] KVP_TRIM = new char[] { ' ', '"' };
      private static readonly char[] COLON_SPLIT = new char[] { ':' };

      private const string CONTENT_DISPOSITION = "Content-Disposition:";
      private const string CONTENT_TYPE = "Content-Type:";
      private const string PARAM_NAME = "name";
      private const string PARAM_CHARSET = "charset";
      private const string PARAM_FILENAME = "filename";
      private const string PARAMS_NAME_PART = CONTENT_DISPOSITION + " form-data; " + PARAM_NAME + "=\"{0}\"";
      private const string PARAMS_FILENAME_PART = "; " + PARAM_FILENAME + "=\"{0}\"";
      private const string PARAMS_CONTENT_TYPE_PART = CONTENT_TYPE + " {0}";
      private const string TEXT = "text";
      private const string JSON = "json";

      private const int BUF_SIZE = 4096;
      private static readonly byte[] EOF = new byte[] {HYPHEN, HYPHEN, CR, LF};
      public const int NOT_FOUND_POS = -1;

    #endregion

    #region Nested

      /// <summary>
      /// Represents a part of multipart encoding
      /// </summary>
      public sealed class Part : INamed
      {
        internal Part() {}

        public Part(string name)
        {
          if (name.IsNullOrWhiteSpace())
            throw new NFXException(StringConsts.MULTIPART_PART_EMPTY_NAME_ERROR);

          m_Name = name;
        }

        private string m_Name;

        public string Name { get { return m_Name; } }
            internal void ____SetName(string name) { m_Name = name; }

        public string FileName { get; set; }
        public string ContentType { get; set; }
        public object Content { get; set; }

      }

      /// <summary>
      /// Represents result of multipart encoded content
      /// </summary>
      public struct EncodedContent
      {
        internal EncodedContent(string boundary, byte[] buf, long idx, long len, Encoding enc)
        {
          Boundary = boundary;
          Buffer = buf;
          StartIdx = idx;
          Length = len;
          Encoding = enc;
        }

        public readonly string Boundary;
        public readonly byte[] Buffer;
        public readonly long StartIdx;
        public readonly long Length;
        public readonly Encoding Encoding;
      }

    #endregion

    #region Static

      public static Multipart ReadFromStream(Stream stream, ref string boundary, Encoding encoding = null)
      {
        if (stream == null || !stream.CanRead)
          throw new NFXException(StringConsts.MULTIPART_STREAM_NOT_NULL_MUST_SUPPORT_READ_ERROR);

        encoding = encoding ?? Encoding.UTF8;

        // find boundary
        var boundaryBytes = boundary.IsNullOrWhiteSpace() ?
                              findBoundary(stream, encoding, out boundary) :
                              locateBoundary(stream, encoding, boundary);

        var result = new Multipart();
        // fill parts from stream
        foreach (var segment in streamSplitNew(stream, boundaryBytes))
        {
          var part = partParse(segment.Array, segment.Count, encoding);
          var success = result.Parts.Register(part);
          if (!success)
            throw new NFXException(StringConsts.MULTIPART_PART_IS_ALREADY_REGISTERED_ERROR.Args(part.Name));
        }

        return result;
      }

      public static Multipart ReadFromBytes(byte[] buffer, ref string boundary, Encoding encoding = null)
      {
        var stream = new MemoryStream(buffer);
        return Multipart.ReadFromStream(stream, ref boundary, encoding);
      }

      public static string ParseContentType(string contentType)
      {
        if (contentType.IsNotNullOrWhiteSpace() && contentType.StartsWith(BOUNDARY_HEADER_START, StringComparison.InvariantCultureIgnoreCase))
          return contentType.Substring(BOUNDARY_HEADER_START.Length);
        else
          return null;
      }

    #endregion

    #region .ctor

      public Multipart()
      {
        m_Parts = new Registry<Part>(caseSensitive: true);
      }

      public Multipart(IEnumerable<Part> parts)
      {
        if (parts == null || parts.Count() == 0)
          throw new NFXException(StringConsts.MULTIPART_PARTS_COULDNT_BE_EMPTY_ERROR);

        m_Parts = new Registry<Part>(caseSensitive: true);
        foreach (var part in parts)
        {
          var success = m_Parts.Register(part);
          if (!success)
            throw new NFXException(StringConsts.MULTIPART_PART_IS_ALREADY_REGISTERED_ERROR.Args(part.Name));
        }
      }

    #endregion

    #region Fields

      private readonly Registry<Part> m_Parts;

    #endregion

    public Registry<Part> Parts { get { return m_Parts; } }

    #region Public

      public JSONDataMap ToJSONDataMap()
      {
        var result = new JSONDataMap(false);

        foreach (var part in m_Parts)
        {
          if (part.FileName != null)
          {
            result[part.Name] = part.Content;
            result[part.Name + POSTFIX_FILENAME] = part.FileName;
            result[part.Name + POSTFIX_CONTENT_TYPE] = part.ContentType;
          }
          else
          {
            result[part.Name] = part.Content;
          }
        }

        return result;
      }

      public EncodedContent Encode(Encoding encoding = null)
      {
        var stream = new MemoryStream();
        return this.Encode(stream, encoding);
      }

      public EncodedContent Encode(MemoryStream stream, Encoding encoding = null)
      {
        if (stream == null || !stream.CanWrite)
          throw new NFXException(StringConsts.MULTIPART_STREAM_NOT_NULL_MUST_SUPPORT_WRITE_ERROR);

        encoding = encoding ?? Encoding.UTF8;
        long startIdx = stream.Position;

        // calculate boundary
        var boundary = makeUniqueBoundary(encoding);
        var boundaryBytes = encoding.GetBytes(boundary);

        if (m_Parts.Count > 0)
        {
          foreach (var part in m_Parts)
          {
            stream.Write(DOUBLE_HYPHEN, 0, 2);
            stream.Write(boundaryBytes, 0, boundaryBytes.Length);
            stream.Write(EOL, 0, 2);
            partEncode(part, stream, encoding);
          }

          stream.Write(DOUBLE_HYPHEN, 0, 2);
          stream.Write(boundaryBytes, 0, boundaryBytes.Length);
          stream.Write(DOUBLE_HYPHEN, 0, 2);
          stream.Write(EOL, 0, 2);
        }

        return new EncodedContent(boundary,
                                  stream.GetBuffer(),
                                  startIdx,
                                  stream.Position - startIdx,
                                  encoding);
      }

    #endregion

    #region .pvt

      private static byte[] locateBoundary(Stream stream, Encoding encoding, string boundary)
      {
        var fullBoundary = DOUBLE_HYPHEN_STR + boundary;
        var boundaryBytes = encoding.GetBytes(fullBoundary);

        bool found = false;
        int current;
        int idx = 0;
        while ((current = stream.ReadByte()) > -1)
        {
          if (boundaryBytes[idx] == current) idx += 1;
          else idx = 0;

          if (idx == boundaryBytes.Length)
          {
            found = true;
            break;
          }
        }

        if (!found)
          throw new NFXException(StringConsts.MULTIPART_SPECIFIED_BOUNDARY_ISNT_FOUND_ERROR);

        return boundaryBytes;
      }

      private static byte[] findBoundary(Stream stream, Encoding encoding, out string boundary)
      {
        int current;
        var foundEOL = false;
        var prevIsCR = false;
        var boundaryBytesList = new List<byte>();

        // find EOL
        while (!foundEOL && (current = stream.ReadByte()) > -1)
        {
          var b = (byte)current;
          if (b == LF)
          {
            if (prevIsCR) foundEOL = true;
          }
          else
          {
            if (b == CR)
            {
              prevIsCR = true;
              continue;
            }
            boundaryBytesList.Add(b);
          }
          prevIsCR = false;
        }

        if (!foundEOL)
          throw new NFXException(StringConsts.MULTIPART_NO_LF_NOR_CRLF_ISNT_FOUND_ERROR);

        // set boundary
        byte[] boundaryBytes = boundaryBytesList.ToArray();
        var fullBoundary = encoding.GetString(boundaryBytes);
        if (fullBoundary.Length < 3)
          throw new NFXException(StringConsts.MULTIPART_BOUNDARY_IS_TOO_SHORT);
        if (fullBoundary[0] != HYPHEN || fullBoundary[1] != HYPHEN)
          throw new NFXException(StringConsts.MULTIPART_BOUNDARY_SHOULD_START_WITH_HYPHENS);

        boundary = fullBoundary.Substring(2); // remove two leading hyphens

        return boundaryBytes;
      }

      private string makeUniqueBoundary(Encoding encoding)
      {
        while(true)
        {
          var boundary = BOUNDARY_PREFIX + Guid.NewGuid().ToString("N");
          var sequence = encoding.GetBytes(boundary);

          if (!m_Parts.Any(p =>
                            {
                              var content = getPartByteContent(p, encoding);
                              return findIndex(content, 0, content.Length, sequence) != NOT_FOUND_POS;
                            })
             ) return boundary;
        }
      }

      private void partEncode(Part part, Stream stream, Encoding encoding)
      {
        partWriteParameters(part, stream, encoding);
        stream.Write(EOL, 0, 2);
        var content = getPartByteContent(part, encoding);
        stream.Write(content, 0, content.Length);
        stream.Write(EOL, 0, 2);
      }

      private byte[] getPartByteContent(Part part, Encoding encoding)
      {
        byte[] content;
        if (part.Content is byte[])
          content = (byte[])part.Content;
        else
        {
          var csEncoding = getEncodingFromCharset(part.ContentType) ?? encoding;
          content = csEncoding.GetBytes(part.Content.AsString());
        }

        return content;
      }

      private static Part partParse(byte[] buf, int length, Encoding encoding)
      {
        if (buf == null || length == 0)
          throw new NFXException(StringConsts.MULTIPART_PART_COULDNT_BE_EMPTY_ERROR);

        if (buf.Length < 2 || buf[length - 2] != CR || buf[length - 1] != LF)
            throw new NFXException(StringConsts.MULTIPART_PART_MUST_BE_ENDED_WITH_EOL_ERROR);

        int separatorPos = findIndex(buf, 0, length, DOUBLE_EOL);
        if (separatorPos == NOT_FOUND_POS)
          throw new NFXException(StringConsts.MULTIPART_DOUBLE_EOL_ISNT_FOUND_AFTER_HEADER_ERROR);

        var result = new Part();
        // The multipart delimiters and header fields are always 7-bit ASCII in any case, and data within the body parts can be encoded on a part-by-part basis,
        // with Content-Transfer-Encoding fields for each appropriate body part. (https://www.w3.org/Protocols/rfc1341/7_2_Multipart.html)
        // todo: what to do with Content-Transfer-Encoding?
        var prmsStr = Encoding.ASCII.GetString(buf, 0, separatorPos);
        partParseParameters(result, prmsStr);

        int contentLength = length - separatorPos - 6; // 6 = EOL.Length * 3
        var rawContent = new byte[contentLength];
        Array.Copy(buf, separatorPos + 4, rawContent, 0, contentLength);  // 4 = EOL.Length * 2

        if (result.ContentType.IsNullOrWhiteSpace())
        {
          if (result.FileName.IsNotNullOrEmpty())
            throw new NFXException();
          result.Content = encoding.GetString(rawContent);
        }
        else if (isTextContent(result.ContentType))
        {
          var csEncoding = getEncodingFromCharset(result.ContentType) ?? encoding;
          result.Content = csEncoding.GetString(rawContent);
        }
        else
          result.Content = rawContent;

        return result;
      }

      private static void partParseParameters(Part part, string str)
      {
        foreach (var s in str.Split(NEW_LINE_SPLIT, StringSplitOptions.RemoveEmptyEntries))
        {
          if (s.StartsWith(CONTENT_DISPOSITION, StringComparison.InvariantCultureIgnoreCase))
            partParseContentDisposition(part, s);
          else if (s.StartsWith(CONTENT_TYPE, StringComparison.InvariantCultureIgnoreCase))
            partParseContentType(part, s);
        }

        if (part.FileName.IsNotNullOrEmpty() && part.ContentType.IsNullOrEmpty())
          part.ContentType = ContentType.BINARY;
      }

      private static void partParseContentDisposition(Part part, string str)
      {
        var valStr = str.Substring(CONTENT_DISPOSITION.Length);
        foreach (var s in str.Split(SEMICOLON_SPLIT, StringSplitOptions.RemoveEmptyEntries))
        {
          var kvps = s.Split(EQUALS_SPLIT, StringSplitOptions.RemoveEmptyEntries);
          if (kvps.Length <= 1)
            continue;

          var key = kvps[0].Trim(KVP_TRIM);
          var val = kvps[1].Trim(KVP_TRIM);

          if (key == PARAM_NAME)
          {
            if (val.IsNullOrEmpty())
              throw new NFXException(StringConsts.MULTIPART_PART_EMPTY_NAME_ERROR);
            part.____SetName(val);
          }
          else if (key == PARAM_FILENAME)
            part.FileName = val;
        }
      }

      private static void partParseContentType(Part part, string str)
      {
        var valStr = str.Substring(CONTENT_TYPE.Length);
        part.ContentType = valStr.Trim(KVP_TRIM);
      }

      private void partWriteParameters(Part part, Stream stream, Encoding encoding)
      {
        byte[] buf = encoding.GetBytes(PARAMS_NAME_PART.Args(part.Name));
        stream.Write(buf, 0, buf.Length);

        if (part.FileName.IsNotNullOrWhiteSpace())
        {
          buf = encoding.GetBytes(PARAMS_FILENAME_PART.Args(part.FileName));
          stream.Write(buf, 0, buf.Length);
          stream.Write(EOL, 0, 2);

          buf = encoding.GetBytes(PARAMS_CONTENT_TYPE_PART.Args(part.ContentType));
          stream.Write(buf, 0, buf.Length);
        }

        stream.Write(EOL, 0, 2);
      }

      private static int findIndex(byte[] where, int whereStart, int whereLength, byte[] what)
      {
        int whatLength = what.Length;
        if (whatLength == 0) return NOT_FOUND_POS;

        for (int i = whereStart; i <= whereLength - whatLength + whereStart; i++)
        {
          bool allMatch = true;
          for (int j = 0; j < whatLength; j++)
            if (where[i+j] != what[j])
            {
              allMatch = false;
              break;
            }

          if (allMatch) return i;
        }

        return NOT_FOUND_POS;
      }

      private static IEnumerable<ArraySegment<byte>> streamSplitNew(Stream stream, byte[] delimeter, int bufSize = BUF_SIZE)
      {
        int delimeterLength = delimeter.Length;
        byte[] seekBuf = new byte[bufSize];
        int seekBufCapacity = bufSize;
        int seekBufLength = 0;
        var delimeterNotFound = false;

        int bytesRead;
        while (true)
        {
          int seekBufFreeLength;
          while((seekBufFreeLength = seekBufCapacity - seekBufLength) < bufSize)
          {
            seekBufCapacity *= 2;
            Array.Resize(ref seekBuf, seekBufCapacity);
          }

          if ((bytesRead = stream.Read(seekBuf, seekBufLength, seekBufFreeLength)) == 0)
          {
            var eof = new byte[EOF.Length];
            Array.Copy(seekBuf, 0, eof, 0, EOF.Length);
            if (delimeterNotFound && !IOMiscUtils.MemBufferEquals(eof, EOF))
              throw new NFXException(StringConsts.MULTIPART_TERMINATOR_ISNT_FOUND_ERROR);
            break;
          }

          seekBufLength += bytesRead;

          while (true)
          {
            int delimeterPos = findIndex(seekBuf, 0, seekBufLength, delimeter);

            if (delimeterPos != NOT_FOUND_POS)
            {
              yield return new ArraySegment<byte>(seekBuf, 0, delimeterPos);

              int copyFrom = delimeterPos + delimeterLength;
              int copyLength = seekBufLength - copyFrom;
              Array.Copy(seekBuf, copyFrom, seekBuf, 0, copyLength);
              seekBufLength = copyLength;
              delimeterNotFound = false;
            }
            else
            {
              delimeterNotFound = true;
              break;
            }
          }
        }

        yield break;
      }

      private static bool isTextContent(string contentType)
      {
        return contentType.IndexOf(TEXT, StringComparison.InvariantCultureIgnoreCase) > -1 ||
               contentType.IndexOf(JSON, StringComparison.InvariantCultureIgnoreCase) > -1;
      }

      private static Encoding getEncodingFromCharset(string contentType)
      {
        if (contentType.IsNullOrEmpty()) return null;
        // find 'charset' value
        string charset = null;
        foreach (var s in contentType.Split(SEMICOLON_SPLIT, StringSplitOptions.RemoveEmptyEntries))
        {
          var kvps = s.Split(EQUALS_SPLIT, StringSplitOptions.RemoveEmptyEntries);
          if (kvps.Length <= 1)
            continue;

          var key = kvps[0].Trim(KVP_TRIM);
          var val = kvps[1].Trim(KVP_TRIM);

          if (key.EqualsOrdIgnoreCase(PARAM_CHARSET))
          {
            charset = val;
            break;
          }
        }

        if (charset == null) return null;
        try
        {
          return Encoding.GetEncoding(charset);
        }
        catch (ArgumentException)
        {
          return null;
        }
      }

    #endregion
  }
}
