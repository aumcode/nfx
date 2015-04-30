
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
 * Revision: NFX 1.0  5/23/2014 3:45:17 PM
 * Author: Denis Latushkin<dxwizard@gmail.com>
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using NFX;
using NFX.Environment;
using NFX.Serialization.JSON;


namespace NFX.Web
{
  /// <summary>
  /// Faciliatets working with multipart encoding
  /// </summary>
  public sealed class MultiPartContent
  {
    #region CONSTS

      private const byte CR = 0x0D;
      private const byte LF = 0x0A;
      private static byte[] EOL_LF_BYTES = new byte[] { LF};
      private static byte[] EOL_CRLF_BYTES = new byte[] { CR, LF};
      private static char HYPHEN_CHAR = '-';
      private static byte HYPHEN_BYTE = (byte)HYPHEN_CHAR;

      private const string BOUNDARY_HEADER_START = "multipart/form-data; boundary=";

      private const string POSTFIX_CONTENT_TYPE = "_contenttype";
      private const string POSTFIX_FILENAME = "_filename";

    #endregion

    #region Inner Types

      #region StreamHelpers

        public static class StreamHelpers
        {
          private const int BUF_SIZE = 4096;
          public const int NOT_FOUND_POS = -1;

          public static int FindIndex(byte[] where, int whereStart, int whereLength, byte[] what)
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

          public static IEnumerable<Tuple<int, int>> BytesSplitNew(byte[] bytes, int bytesLength, byte[] by, bool returnOuter = false)
          {
            int byLength = by.Length;
            int curSegmentStart = NOT_FOUND_POS;
            while (true)
            {
              int findFrom = curSegmentStart == NOT_FOUND_POS ? 0 : curSegmentStart;
              int findLength = curSegmentStart == NOT_FOUND_POS ? bytesLength : bytesLength - curSegmentStart;
              int byIdx = FindIndex(bytes, findFrom, findLength, by);

              if (byIdx != NOT_FOUND_POS)
              {
                if ((returnOuter || curSegmentStart != NOT_FOUND_POS) && byIdx > 0 && byIdx > curSegmentStart)
                {
                  int returnSegmentStart = curSegmentStart >= 0 ? curSegmentStart : 0;
                  int returnSegmentLength = curSegmentStart >= 0 ? byIdx - curSegmentStart : byIdx;
                  yield return new Tuple<int, int>(returnSegmentStart, returnSegmentLength);
                }

                curSegmentStart = byIdx + byLength;
              }
              else
              {
                if (returnOuter)
                {
                  int returnSegmentStart = curSegmentStart >= 0 ? curSegmentStart : 0;
                  int returnSegmentLength = curSegmentStart >= 0 ? bytesLength - curSegmentStart : bytesLength;
                  if (returnSegmentLength > 0)
                    yield return new Tuple<int, int>(returnSegmentStart, returnSegmentLength); 
                }
                break;
              }
            }
          }

          public static IEnumerable<Tuple<byte[], int>> StreamSplitNew(Stream stream, byte[] by, int bufSize = BUF_SIZE)
          {
            var segments = new List<Tuple<byte[], int>>();
            int byLength = by.Length;

            byte[] seekBuf = new byte[bufSize];
            int seekBufCapacity = bufSize;
            int seekBufLength = 0;
            bool firstFound = false;

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
                break;

              seekBufLength += bytesRead;

              while (true)
              {
                int byIdx = FindIndex(seekBuf, 0, seekBufLength, by);

                if (byIdx != NOT_FOUND_POS)
                {
                  if (firstFound && byIdx>0)
                  {
                    yield return new Tuple<byte[], int>(seekBuf, byIdx);
                  }
                  else
                  {
                    firstFound = true;
                  }

                  int copyFrom = byIdx + byLength;
                  int copyLength = seekBufLength - copyFrom;
                  Array.Copy(seekBuf, copyFrom, seekBuf, 0, copyLength);
                  seekBufLength = copyLength;
                }
                else
                  break;
              }
            }

            yield break;
          }

          public static long Find(Stream where, byte[] what, 
            long whereFirst = 0, long? whereLast = null, long whatFirst = 0, long? whatLast = null, int seekBuffSize = BUF_SIZE)
          {
            if (!whereLast.HasValue) whereLast = where.Length-1;
            if (!whatLast.HasValue) whatLast = what.Length-1;

            if (whereFirst > whereLast.Value) return NOT_FOUND_POS;

            long matchPos = whatFirst-1;

            byte[] currBuf = new byte[seekBuffSize];

            long whereInitialPos = where.Position;
            try
            {
              where.Position = whereFirst;

              long whatLength = whatLast.Value - whatFirst + 1;

              long streamBasePos = 0;
              int bytesRead;
              while ((bytesRead = where.Read(currBuf, 0, seekBuffSize <= whereLast - streamBasePos + 1 ? seekBuffSize : (int)(whereLast - streamBasePos + 1))) > 0)
              {
                for (long i = 0; i < bytesRead; i++)
                {
                  if (what[matchPos + 1] == currBuf[i])
                  {
                    if (++matchPos >= whatLast)
                      return (whereFirst + streamBasePos + i - whatLength + 1);
                  }
                  else
                  {
                    matchPos = whatFirst - 1;
                    if (what[matchPos + 1] == currBuf[i])
                    {
                      if (++matchPos >= whatLast)
                        return (whereFirst + streamBasePos + i - whatLength + 1);
                    }
                  }
                }

                streamBasePos += bytesRead;
              }

              return NOT_FOUND_POS;
            }
            finally
            {
              where.Position = whereInitialPos;
            }
          }

          public static bool EndsWith(Stream what, byte[] with, 
            long whatFirst = 0, long? whatLast = null, long withFirst = 0, long? withLast = null, int seekBuffSize = BUF_SIZE)
          {
            if (!whatLast.HasValue) whatLast = what.Length-1;
            if (!withLast.HasValue) withLast = with.Length-1;

            long pos = Find(what, with, whatLast.Value-with.Length, whatLast, withFirst, withLast, seekBuffSize);
            return pos != NOT_FOUND_POS;
          }

          public static long CopyTo(Stream src, Stream dst, long srcFirst, long srcLast, long copyBufSize = BUF_SIZE)
          {
            long initialSrcPos = src.Position;
            try 
	          {	        
              src.Position = srcFirst;

              long srcCopyLength = srcLast - srcFirst + 1;

              var buf = new byte[copyBufSize];
              int bytesRead;
              while ((bytesRead = src.Read(buf, 0, (int)Math.Min(srcCopyLength, srcLast - src.Position + 1))) > 0)
                dst.Write(buf, 0, bytesRead);

              return dst.Length;
	          }
	          finally
	          {
              src.Position = initialSrcPos;
	          }
          }

          public static byte[] GetToTerminator(Stream src, byte[] terminator, 
            long whereFirst = 0, long? whereLast = null, long whatFirst = 0, long? whatLast = null, int seekBuffSize = BUF_SIZE)
          {
            long initialSrcPos = src.Position;

            try 
	          {	        
		          long terminatorPos = Find(src, terminator, whereFirst, whereLast, whatFirst, whatLast, seekBuffSize);
              if (terminatorPos == NOT_FOUND_POS)
                return null;

              int fragmentSize = (int)(terminatorPos-initialSrcPos);
              byte[] res = new byte[fragmentSize];

              src.Read(res, 0, fragmentSize);

              return res;
	          }
	          finally
	          {
              src.Position = initialSrcPos;
	          }
          }

          public static IEnumerable<Tuple<long, long>> Split(Stream what, byte[] by, 
            long whatFirst = 0, long? whatLast = null, long byFirst = 0, long? byLast = null, int seekBuffSize = BUF_SIZE)
          {
            long initialSrcPos = what.Position;

            try 
	          {
              if (!whatLast.HasValue) whatLast = what.Length-1;

              if (what.Length == 0) yield break;

              if (by.Length == 0)
              {
                yield return new Tuple<long, long>(whatFirst, whatLast.Value);
                yield break;
              }

              long bySignificantLength = byLast.HasValue ? byLast.Value - byFirst + 1 : by.Length - byFirst;
              long prevMatch = NOT_FOUND_POS;
              long curMatch = NOT_FOUND_POS;

              while (true)
              {
                curMatch = StreamHelpers.Find(what, by, whatFirst, whatLast, byFirst, byLast, seekBuffSize);

                if (curMatch == NOT_FOUND_POS)
                {
                  if (prevMatch == NOT_FOUND_POS)
                    yield break;

                  long curSegmentFirst = prevMatch + bySignificantLength;

                  if (curSegmentFirst <= whatLast)
                  {
                    yield return new Tuple<long, long>(curSegmentFirst, whatLast.Value);
                  }

                  yield break;
                }
                else
                {
                  if (prevMatch == NOT_FOUND_POS)
                  {
                    if (whatFirst < curMatch)
	                  {
                      yield return new Tuple<long, long>(whatFirst, curMatch-1);
                    }
                  }
                  else
                  {
                    long curSegmentFirst = prevMatch + bySignificantLength;
                    if (curSegmentFirst < curMatch)
                      yield return new Tuple<long, long>(curSegmentFirst, curMatch-1);
                  }
                }

                prevMatch = curMatch;

                whatFirst = curMatch + by.Length;
              }
	          }
	          finally
	          {
              what.Position = initialSrcPos;
	          }
          }

          public static string GetString(Stream stream, long first, long last, Encoding encoding = null)
          {
            using (var ms = new MemoryStream())
            {
              CopyTo(stream, ms, first, last);
              string str = (encoding ?? Encoding.UTF8).GetString(ms.GetBuffer(), 0, (int)ms.Length);
              return str;
            }
          }
        }
    
      #endregion

      #region MultiPartParameters

        public class MultiPartParameters
        {
          public static char[] SEMICOLON_SPLIT = new char[] { ';'};
          public static char[] EQUALS_SPLIT = new char[] { '='};
          public static char[] KVP_TRIM = new char[] { ' ', '"'};
          public static char[] COLON_SPLIT = new char[] { ':'};

          public const string CONTENT_DISPOSITION = "Content-Disposition:";
          public const string CONTENT_TYPE = "Content-Type:";

          public const string PARAM_NAME = "name";
          public const string PARAM_FILENAME = "filename";

          public static MultiPartParameters CreateField(string name, byte[] eol)
          {
            return new MultiPartParameters(eol) { m_Name = name};
          }

          public static MultiPartParameters CreateFile(string name, string fileName, string contentType, byte[] eol)
          {
            return new MultiPartParameters(eol) { m_Name = name, m_FileName = fileName, m_ContentType = contentType};
          }

          public static MultiPartParameters Parse(string str, byte[] eol)
          {
            return new MultiPartParameters(str, eol);
          }

          public static MultiPartParameters Parse(IEnumerable<string> prmStrs, byte[] eol)
          {
            return new MultiPartParameters(prmStrs, eol);
          }

          private MultiPartParameters(byte[] eol) 
          {
            m_EOL = eol;
          }

          private MultiPartParameters(string str, byte[] eol): this(eol)
          {
            string[] eolSplit = new string[] { Encoding.UTF8.GetString(eol) };

            foreach (var s in str.Split(eolSplit, StringSplitOptions.RemoveEmptyEntries))
            {
              if (s.StartsWith(CONTENT_DISPOSITION))
                parseContentDisposition(s);
              else if (s.StartsWith(CONTENT_TYPE))
                parseContentType(s);
            }
          }

          private MultiPartParameters(IEnumerable<string> prmStrs, byte[] eol): this(eol)
          {
            foreach (var s in prmStrs)
            {
              if (s.StartsWith(CONTENT_DISPOSITION))
                parseContentDisposition(s);
              else if (s.StartsWith(CONTENT_TYPE))
                parseContentType(s);
            }
          }

          private byte[] m_EOL;
          private string m_Name;
          private string m_FileName;
          private string m_ContentType;

          public string Name { get { return m_Name; }}
          public string FileName { get { return m_FileName; }}
          public bool IsField { get { return m_FileName.IsNullOrWhiteSpace(); }}
          public string ContentType { get { return m_ContentType; }}

          public void Write(Stream stream)
          {
            byte[] buf = Encoding.UTF8.GetBytes("Content-Disposition: form-data; name=\"{0}\"".Args(m_Name));
            stream.Write(buf, 0, buf.Length);

            if (!IsField)
            {
              buf = Encoding.UTF8.GetBytes("; filename=\"{0}\"".Args(m_FileName));
              stream.Write(buf, 0, buf.Length);
              stream.Write(m_EOL, 0, m_EOL.Length);

              buf = Encoding.UTF8.GetBytes("Content-Type: {0}".Args(m_ContentType));
              stream.Write(buf, 0, buf.Length);
            }

            stream.Write(m_EOL, 0, m_EOL.Length);
          }

          private void parseContentDisposition(string str)
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
                m_Name = val;
              else if (key == PARAM_FILENAME)
                m_FileName = val;
            }
          }

          private void parseContentType(string str)
          {
            var valStr = str.Substring(CONTENT_TYPE.Length);
            m_ContentType = valStr.Trim(KVP_TRIM);
          }
        }

      #endregion

      #region MultiPart

        public class MultiPart
        {
          public static MultiPart CreateField(string fieldName, string fieldValue, byte[] eol, Encoding encoding = null)
          {
            var parameters = MultiPartParameters.CreateField(fieldName, eol);
            var part = new MultiPart(parameters, eol, encoding);
            part.m_Content = (encoding ?? Encoding.UTF8).GetBytes(fieldValue);
            part.m_ContentString = fieldValue;
            return part;
          }

          public static MultiPart CreateFile(string name, string fieldName, string contentType, 
            byte[] content, byte[] eol, Encoding encoding = null)
          {
            var parameters = MultiPartParameters.CreateFile(name, fieldName, contentType, eol);
            var part = new MultiPart(parameters, eol, encoding);
            part.m_Content = content;
            return part;
          }

          public static MultiPart Parse(byte[] buf, int length, Encoding encoding = null)
          {
            var p = new MultiPart();

            if (buf[0] == MultiPartContent.LF)
              p.m_EOL = MultiPartContent.EOL_LF_BYTES;
            else if (buf[0] == MultiPartContent.CR && buf[1] == MultiPartContent.LF)
              p.m_EOL = MultiPartContent.EOL_CRLF_BYTES;
            else
              throw new NFXException(StringConsts.MULTIPART_NO_LF_NOR_CRLF_ISNT_FOUND_ERROR + typeof(MultiPart).Name + ".Parse");

            int eolLength = p.m_EOL.Length;
            int doubleEOLLength = eolLength << 1;
            byte[] doubleEOL = new byte[doubleEOLLength];
            Array.Copy(p.m_EOL, 0, doubleEOL, 0, eolLength); Array.Copy(p.m_EOL, 0, doubleEOL, eolLength, eolLength);

            int separatorPos = StreamHelpers.FindIndex(buf, 0, length, doubleEOL);
            if (separatorPos == StreamHelpers.NOT_FOUND_POS)
              throw new NFXException(StringConsts.MULTIPART_DOUBLE_EOL_ISNT_FOUND_AFTER_HEADER_ERROR + typeof(MultiPart).Name + ".Parse");

            string prmsStr = (encoding ?? Encoding.UTF8).GetString(buf, 0, separatorPos);
            p.m_Parameters = MultiPartParameters.Parse(prmsStr, p.m_EOL);

            for (int iEOL = 0, iBuf = length - eolLength; iEOL < eolLength; iEOL++, iBuf++)
              if (p.m_EOL[iEOL] != buf[iBuf])
                throw new NFXException("Invalid content EOL. " + typeof(MultiPart) + ".Parse()");

            int contentLength = length - separatorPos - doubleEOLLength - eolLength;
            p.m_Content = new byte[contentLength];
            Array.Copy(buf, separatorPos + doubleEOLLength, p.m_Content, 0, contentLength);

            if (p.Parameters.IsField)
              p.m_ContentString = (encoding ?? Encoding.UTF8).GetString(p.m_Content);

            return p;
          }

          private MultiPart() {}

          private MultiPart(MultiPartParameters parameters, byte[] eol, Encoding encoding = null)
          {
            m_Parameters = parameters;
            m_EOL = eol;
          }

          public MultiPart(Stream stream, long start, long finish, byte[] eol, Encoding encoding = null)
          {
            parse(stream, start, finish, eol, encoding);
          }

          private MultiPartParameters m_Parameters;
          private byte[] m_EOL;
          private byte[] m_Content;
          private string m_ContentString;

          public MultiPartParameters Parameters { get { return m_Parameters; }}

          private void parse(Stream stream, long start, long finish, byte[] eol, Encoding encoding)
          {
            byte[] eol2 = new byte[eol.Length << 1];
            Array.Copy(eol, eol2, eol.Length); Array.Copy(eol, 0, eol2, eol.Length, eol.Length);

            long eol2PosAfterHeader = StreamHelpers.Find(stream, eol2, start, finish);

            if (eol2PosAfterHeader == StreamHelpers.NOT_FOUND_POS)
              throw new NFXException(StringConsts.MULTIPART_DOUBLE_EOL_ISNT_FOUND_AFTER_HEADER_ERROR + this.GetType().Name + ".Parse");

            string str = StreamHelpers.GetString(stream, start, eol2PosAfterHeader-1);

            m_Parameters = MultiPartParameters.Parse(str, eol);

            m_Content = new byte[finish - eol2PosAfterHeader - eol2.Length + 1];
            stream.Position = eol2PosAfterHeader + eol2.Length;
            stream.Read(m_Content, 0, m_Content.Length);

            m_Content = new byte[finish - (eol2PosAfterHeader + eol2.Length) + 1];
            stream.Position = eol2PosAfterHeader + eol2.Length;
            stream.Read(m_Content, 0, m_Content.Length);

            if (m_Parameters.IsField)
              m_ContentString = encoding.GetString(m_Content);
          }

          public bool ContentContains(byte[] sequence)
          {
            using (var ms = new MemoryStream(m_Content))
            {
              return StreamHelpers.Find(ms, sequence) != StreamHelpers.NOT_FOUND_POS;
            }
          }

          public void Write(Stream stream)
          {
            m_Parameters.Write(stream);
            stream.Write(m_EOL, 0, m_EOL.Length);
            stream.Write(m_Content, 0, m_Content.Length);
            stream.Write(m_EOL, 0, m_EOL.Length);
          }
    
          public byte[] Content { get { return m_Content; }}

          public string ContentAsString { get { return m_ContentString; }}
    
        }
        
      #endregion

    #endregion

    #region Static

      public static MultiPartContent Decode(Stream stream, string boundary = null, Encoding encoding = null)
      {
        var content = new MultiPartContent();
        content.m_Boundary = boundary;
        content.m_Stream = stream;
        content.parseStream(stream, encoding);
        return content;
      }

      public static MultiPartContent Encode(IEnumerable<MultiPart> parts, string boundary = null)
      {
        if (parts.Count() == 0)
          throw new NFXException(StringConsts.MULTIPARTPARTS_COULDNT_BE_EMPTY_ERROR + typeof(MultiPartContent).Name + ".Encode");

        var content = new MultiPartContent();
        content.m_parts = parts.ToList();
        content.fillStream(parts);
        return content;
      }

      public static JSONDataMap ToJSONDataMap(Stream stream, string streamContentType = null, Encoding encoding = null)
      {
        if (stream == null || !stream.CanRead)
          throw new NFXException(StringConsts.MULTIPART_STREAM_NOT_NULL_MUST_SUPPORT_READ_ERROR + typeof(MultiPartContent) + ".ToJSONDataMap");

        string boundary = null;

        if (streamContentType.IsNotNullOrWhiteSpace() && streamContentType.StartsWith(BOUNDARY_HEADER_START))
          boundary = streamContentType.Substring(BOUNDARY_HEADER_START.Length);

        var content = new MultiPartContent();
        content.m_Boundary = boundary;
        content.m_Stream = stream;
        content.parseStream(stream, boundary, encoding);

        var result = new JSONDataMap();

        foreach (var part in content.Parts)
          if (part.Parameters.IsField)
          {
            result[part.Parameters.Name] = part.ContentAsString;
          }
          else
          {
            result[part.Parameters.Name] = part.Content;
            result[part.Parameters.Name + POSTFIX_FILENAME] = part.Parameters.FileName;
            result[part.Parameters.Name + POSTFIX_CONTENT_TYPE] = part.Parameters.ContentType;
          }

        return result;
      }

    #endregion

    #region .ctor

      private MultiPartContent() {}

    #endregion

    #region Pvt/Prot/Int Fields

      private byte[] m_EOL;
      private string m_Boundary;
      private IList<MultiPart> m_parts = new List<MultiPart>();
      private Stream m_Stream;

    #endregion

    #region Properties

      public string Boundary { get { return m_Boundary; }}
      public byte[] EOL { get { return m_EOL; }}
      public IList<MultiPart> Parts { get { return m_parts; }}
      public Stream Stream { get { return m_Stream; }}

    #endregion

    #region Protected

    #endregion

    #region .pvt. impl.

      private void parseStream(Stream stream, string boundary, Encoding encoding)
      {
        m_Stream = stream;
        string fullBoundary = new string(HYPHEN_CHAR, 2) + boundary;

        foreach (var segment in StreamHelpers.StreamSplitNew(stream, Encoding.UTF8.GetBytes(fullBoundary)))
        {
          var part = MultiPart.Parse(segment.Item1, segment.Item2);
          m_parts.Add(part);
        }
      }

      private void parseStream(Stream stream, Encoding encoding)
      {
        stream.Position = 0;

        long firstEOLPos = StreamHelpers.Find(stream, EOL_CRLF_BYTES);
        if (firstEOLPos == StreamHelpers.NOT_FOUND_POS)
        {
          firstEOLPos = StreamHelpers.Find(stream, EOL_LF_BYTES);
          if (firstEOLPos == StreamHelpers.NOT_FOUND_POS)
            throw new NFXException(StringConsts.MULTIPART_NO_LF_NOR_CRLF_ISNT_FOUND_ERROR + this.GetType().Name + ".ParseStream");

          m_EOL = EOL_LF_BYTES;
        }
        else
        {
          m_EOL = EOL_CRLF_BYTES;
        }

        byte[] boundaryBytes = new byte[firstEOLPos];
        boundaryBytes = new byte[firstEOLPos];
        stream.Read(boundaryBytes, 0, (int)firstEOLPos);

        if (m_Boundary != null)
        {
          string streamBoundaryStr = Encoding.UTF8.GetString(boundaryBytes).Substring(2);
          if (streamBoundaryStr != m_Boundary)
            throw new NFXException(StringConsts.MULTIPART_BOUNDARY_MISMATCH_ERROR.Args(m_Boundary, streamBoundaryStr) + 
              this.GetType().Name + ".ParseStream");
        }
        else
        {
          var fullBoundary = Encoding.UTF8.GetString(boundaryBytes);
          if (fullBoundary.Length < 3)
            throw new NFXException(StringConsts.MULTIPART_BOUNDARY_COULDNT_BE_SHORTER_3_ERROR + this.GetType().Name + ".ParseStream");
          m_Boundary = fullBoundary.Substring(2); // remove two leading hyphens
        }
      
        stream.Position = 0;

        int boundaryLength = boundaryBytes.Length;
        byte[] endBoundaryBytes = new byte[boundaryLength+2];
        stream.Read(endBoundaryBytes, 0, boundaryLength);
        endBoundaryBytes[boundaryLength] = HYPHEN_BYTE; endBoundaryBytes[boundaryLength+1] = HYPHEN_BYTE;

        long terminatorPos = StreamHelpers.Find(stream, endBoundaryBytes);
        if (terminatorPos == StreamHelpers.NOT_FOUND_POS)
          throw new NFXException(StringConsts.MULTIPART_TERMINATOR_ISNT_FOUND_ERROR + this.GetType().Name + ".ParseStream");

        var splitSegmentCoordinates = StreamHelpers.Split(stream, boundaryBytes, whatLast: terminatorPos).ToArray();

        foreach (var coordinate in splitSegmentCoordinates.Where(c => (c.Item2 - c.Item1) > m_EOL.Length))
        {
          if (!StreamHelpers.EndsWith(stream, m_EOL, coordinate.Item1, coordinate.Item2))
            throw new NFXException(StringConsts.MULTIPART_PART_SEGMENT_ISNT_TERMINATED_CORRECTLY_ERROR.Args(m_EOL) + this.GetType().Name + ".ParseStream");

          var part = new MultiPart( stream, coordinate.Item1, coordinate.Item2 - m_EOL.Length, m_EOL, (encoding ?? Encoding.UTF8));

          Parts.Add(part);
        }
      }

      private void fillStream(IEnumerable<MultiPart> parts)
      {
        m_Boundary = findUniqueBoundary(parts);
        var boundaryBytes = Encoding.UTF8.GetBytes(new string((char)HYPHEN_BYTE, 2) + m_Boundary);

        m_Stream = new MemoryStream();

        foreach (var part in parts)
        {
          m_Stream.Write(boundaryBytes, 0, boundaryBytes.Length);
          m_Stream.Write(EOL_CRLF_BYTES, 0, EOL_CRLF_BYTES.Length);

          part.Write(m_Stream);
        }

        m_Stream.Write(boundaryBytes, 0, boundaryBytes.Length);
        m_Stream.WriteByte(HYPHEN_BYTE); m_Stream.WriteByte(HYPHEN_BYTE);
        m_Stream.Write(EOL_CRLF_BYTES, 0, EOL_CRLF_BYTES.Length);
      }

      private string findUniqueBoundary(IEnumerable<MultiPart> parts)
      {
        string hyphenPrefix = new string((char)HYPHEN_BYTE, 10);

        while(true)
        {
          var boundary = hyphenPrefix + Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(19);
          var sequence = Encoding.UTF8.GetBytes(boundary);

          if (!parts.Any(p => p.ContentContains(sequence)))
            return boundary;
        }
      }

    #endregion

  } //MultiPartContent

}
