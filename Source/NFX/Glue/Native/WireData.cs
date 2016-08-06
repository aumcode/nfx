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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using NFX.IO;
using NFX.Environment;
using NFX.Serialization;

namespace NFX.Glue.Native
{

    /// <summary>
    /// Represents a stream/writable chunk of memory backed by an array buffer
    /// </summary>
    public class MemChunk : MemoryStream
    {
       public MemChunk(int capacity) : base(capacity)
       {
         SetLength(capacity);
       }

       /// <summary>
       /// Whan was the chunk released to the pool for the last time
       /// </summary>
       internal DateTime _LastReleaseUtc;

       /// <summary>
       /// True if chunk is taken out of pool by some socket
       /// </summary>
       internal bool Acquired;

       /// <summary>
       /// Used for statistics/tracking
       /// </summary>
       internal int stat_MaxUsedPosition;

       /// <summary>
       /// Used for statistics/tracking
       /// </summary>
       internal int stat_MaxLength;
    }


    /// <summary>
    /// Represents a message that is transmitted over the wire:
    /// </summary>
    /// <remarks>
    ///   Wire Msg gets serialized like so:
    ///     +----+ | +----------------------------------------------+---------+ | +-------+
    ///     |Size| | |                 Frame Content                | Hdrs... | | | GlueM |
    ///     +----+ | +----------------------------------------------+---------+ | +-------+
    ///        ^     \----------------------+---------------------------------/       ^
    ///        |                            |                                         |
    ///        |                            |                                         +-- Msg data (payload) , i.e. Glue message body
    ///        |                            +----------------------------- Transport Frame with optional var length headers
    ///        +-------------------------------------------------------- Total msg size 4 bytes
    /// </remarks>
    public struct WireMsg
    {
      /// <summary>
      /// Creates WireMsg around pre-filled memory chunk that includes msg size - first 4 bytes
      /// </summary>
      public WireMsg(MemChunk data) : this()
      {
        Data = data;
      }

      /// <summary>
      /// Returns the data chunk received/sent from/to socket INCLUDING the total size (first 4 bytes BEint32)
      /// </summary>
      public readonly MemChunk Data;

      private bool m_FrameRead;
      private WireFrame m_Frame;

      /// <summary>
      /// Returns frame serialized in data
      /// </summary>
      public WireFrame Frame
      {
       get
       {
          if (!m_FrameRead)
          {
            m_Frame = new WireFrame( new MemoryStream(Data.GetBuffer(), sizeof(int), (int)Data.Length-sizeof(int)));
            m_FrameRead =true;
          }
          return m_Frame;
       }
       internal set
       {
          m_Frame = value;
          m_FrameRead = true;
       }
      }

      /// <summary>
      /// Returns data byte buffer
      /// </summary>
      public byte[] Buffer { get{ return Data.GetBuffer();}}

       /// <summary>
      /// Returns data byte buffer use count, i.w. the buffer may be physically larger, however only BufferUsedSize must be transmitted
      /// </summary>
      public int BufferUsedSize { get{ return (int)Data.Position;}}

      /// <summary>
      /// Update buffers statistics
      /// </summary>
      public void UpdateBufferStats()
      {
        var used = BufferUsedSize;
        if (used>Data.stat_MaxUsedPosition) Data.stat_MaxUsedPosition = used;

        var len = (int)Data.Length;
        if (len>Data.stat_MaxLength) Data.stat_MaxLength = len;
      }
    }


    /// <summary>
    /// Type of transport frame message
    /// </summary>
    public enum FrameType
    {
        /// <summary>
        /// Indicator that following payload is a standard Glue message
        /// </summary>
        GlueMsg       = 'G',

        /// <summary>
        /// Indicator of the oneway hartbeat message
        /// </summary>
        Heartbeat     = 'H',

        /// <summary>
        /// Indicator that this is an echo request message and sending party awaits the EchoResponse message
        /// </summary>
        Echo          = 'E',

        /// <summary>
        /// Indicator that this is an echo response message that sending party awaits after E was sent
        /// </summary>
        EchoResponse  = 'R',

        /// <summary>
        /// Indicator that the payload is a one way dummy and should be ignored.
        /// This may be needed for stress testing the network throughput
        /// </summary>
        Dummy         = 'D'
    }

    /// <summary>
    /// This struct defines a frame transmitted as a part of WireMsg
    /// which can be used for transport or session management needs
    /// </summary>
    /// <remarks>
    /// Frame Content:
    ///   -----------------------------------------
    ///   MAGIC       ushort               2  byte   -  0xABBA
    ///   VESRION     byte                 1  byte
    ///   TYPE        {G|H|E|R|D}          1  byte
    ///   FORMAT      int32BE              4  bytes
    ///   ONEWAY      {0|!0}               1  byte
    ///   REQID       FID                  8  bytes
    ///   HDRSLEN     int32BE              4  bytes
    ///                                ------------
    ///                                   21  bytes
    ///
    ///   HDRSCONTENT  utf8_char[x]  HDRSLEN  bytes
    ///   -----------------------------------------
    ///                         21 + HDRSLEN  bytes total size
    /// </remarks>
    public struct WireFrame
    {
        #region CONSTS

            public const ushort MAGIC = 0xABBA;

            public const int SLIM_FORMAT = 0x534C494D;//'SLIM'

            public const byte VERSION = 1;

            public const byte FRAME_LENGTH = sizeof(ushort) + //MAGIC
                                             1 +              //VERSION
                                             1 +              //TYPE
                                             sizeof(int) +    //FORMAT
                                             1 +              //ONEWAY
                                             FID_LENGTH +     //REQID,
                                             sizeof(int);     //HDRSLEN

            public const byte FID_LENGTH = 8;

            public static readonly Encoding HEADERS_ENCODING = Encoding.UTF8;

            public const int MAX_HEADERS_BYTE_LENGTH = 4 * 1024 * 1024;

        #endregion

        #region .ctor


            public WireFrame(int format, bool oneWay, FID reqID, string headersContent = null) :
                             this(FrameType.GlueMsg, format, oneWay, reqID, headersContent)
            {

            }

            public WireFrame(FrameType type, int format, bool oneWay, FID reqID, string headersContent = null) : this()
            {
                m_Type           = type;
                m_Format         = format;
                m_OneWay         = type==FrameType.GlueMsg ? oneWay : type!=FrameType.Echo;
                m_RequestID      = reqID;
                m_HeadersContent = headersContent;
                m_HeadersContentLength = string.IsNullOrWhiteSpace(headersContent)? 0 : HEADERS_ENCODING.GetByteCount( headersContent );
                m_Length         =  FRAME_LENGTH + m_HeadersContentLength;

                if (m_HeadersContentLength>MAX_HEADERS_BYTE_LENGTH)
                       throw new ProtocolException(
                           StringConsts.GLUE_BAD_PROTOCOL_FRAME_ERROR + "created headers length of {0} bytes exceeds the limit of {1} bytes"
                                                                        .Args(m_HeadersContentLength, MAX_HEADERS_BYTE_LENGTH));
            }

            /// <summary>
            /// Reconstruct (deserializes) frame from the stream. May throw on error
            /// </summary>
            public WireFrame(Stream stream) : this()
            {
                //MAGIC
                var magic = stream.ReadBEUShort();
                if (magic != MAGIC)
                    throw new ProtocolException(
                                StringConsts.GLUE_BAD_PROTOCOL_FRAME_ERROR + "bag magic: expected '{0}' but got '{1}'".Args(MAGIC, magic));

                //VERSION
                var ver = stream.ReadByte();
                if (ver != VERSION)
                    throw new ProtocolException(
                                StringConsts.GLUE_BAD_PROTOCOL_FRAME_ERROR + "version mismatch: expected '{0}' but got '{1}'".Args(VERSION, ver));
                //TYPE
                m_Type  = (FrameType)stream.ReadByte();

                //FORMAT
                m_Format = stream.ReadBEInt32();

                //ONEWAY
                m_OneWay = stream.ReadByte() != 0;

                //REQUESTID
                m_RequestID = new FID( stream.ReadBEUInt64() );

                //HEADERSCONTENT
                var len = stream.ReadBEInt32();
                if (len>0)
                {
                    if (len>MAX_HEADERS_BYTE_LENGTH)
                       throw new ProtocolException(
                                StringConsts.GLUE_BAD_PROTOCOL_FRAME_ERROR + "arrived headers length of {0} bytes exceeds the limit of {1} bytes".Args(len, MAX_HEADERS_BYTE_LENGTH));

                    m_HeadersContentLength = len;

                    var hbuf = new byte[len];
                    stream.Read(hbuf, 0, len);
                    m_HeadersContent = HEADERS_ENCODING.GetString( hbuf );
                }

                m_Length = FRAME_LENGTH + m_HeadersContentLength;
            }

        #endregion

        #region Fields
            private FrameType   m_Type;
            private int         m_Format;
            private bool        m_OneWay;
            private FID         m_RequestID;
            private string      m_HeadersContent;

            private int m_Length;
            private int m_HeadersContentLength;
        #endregion

        #region Props

            /// <summary>
            /// Returns type of this frame
            /// </summary>
            public FrameType Type { get { return m_Type; } }

            /// <summary>
            /// Format used for payload encoding, use FORMAT_SLIM as a default constant
            /// </summary>
            public int Format  {   get { return m_Format; }  }

            /// <summary>
            /// RequestID of the request in following message or echo response message
            /// </summary>
            public FID RequestID { get { return m_RequestID; } private set { m_RequestID = value; } }

            /// <summary>
            /// True if the requesting party does not expect a response
            /// </summary>
            public bool OneWay { get { return m_OneWay; } private set { m_OneWay = value; } }

            /// <summary>
            /// Optional frame headers content - an unparsed XML string
            /// </summary>
            public string HeadersContent { get { return m_HeadersContent ?? string.Empty; } }

            /// <summary>
            /// Optional frame headers parsed from XML HeadersContent
            /// </summary>
            public IConfigSectionNode Headers
            {
                get { return XMLConfiguration.CreateFromXML(string.IsNullOrWhiteSpace(m_HeadersContent)? "<h></h>" : m_HeadersContent).Root; }
            }

            /// <summary>
            /// The total byte size of the frame that includes the length of headers (if any)
            /// </summary>
            public int  Length { get { return m_Length; } }

        #endregion

        #region Public

            /// <summary>
            /// Serialize the frame to the given stream returning the byte size of the frame
            /// </summary>
            public int Serialize(Stream stream)
            {
                //MAGIC
                stream.WriteBEUShort(MAGIC);

                //VERSION
                stream.WriteByte( VERSION );

                //TYPE
                stream.WriteByte( (byte)m_Type );

                //FORMAT
                stream.WriteBEInt32( m_Format );

                //ONEWAY
                stream.WriteByte( m_OneWay ? (byte)1 : (byte)0 );

                //REQUEST GUID
                stream.WriteBEUInt64( m_RequestID.ID );

                //HEADERSCONTENT
                stream.WriteBEInt32(m_HeadersContentLength);
                if (m_HeadersContentLength>0)
                {
                    var hbuf = HEADERS_ENCODING.GetBytes( m_HeadersContent );
                    stream.Write( hbuf, 0, hbuf.Length );
                }

                return Length;
            }




            public override string ToString()
            {
                string hdrs = "<none>";

                if (m_HeadersContent.IsNotNullOrWhiteSpace())
                 try { hdrs = Headers.Name; } catch { hdrs = "<invalid>";}

                return "Frame<{0},'{1}',{2},{3},[{4}]>".Args(m_Type, m_Format, m_OneWay, m_RequestID, hdrs );
            }

        #endregion

    }


  /// <summary>
  /// Represents a client call site identifier which gets generated on client and sent to server upon handshake.
  /// This struct identifies the calling client by supplying machine/host name and calling application instance ID
  /// </summary>
  [Serializable]
  public struct ClientSite : INamed
  {
      public const int MAGIC = 0x5555AAAA;

      public const int MAX_HOST_NAME_BYTE_LEN = 1024;

      public const int MAX_STREAM_BYTE_LEN =  sizeof(short) +   // TOTAL
                                              sizeof(int)   +   // MAGIC
                                              sizeof(short) +   // Host name sz
                                              MAX_HOST_NAME_BYTE_LEN + //h name byte len
                                              GUID_SIZE; //guid

      public const int GUID_SIZE = 16;

      public static readonly Encoding TEXT_ENCODING = Encoding.UTF8;

      /// <summary>
      /// Sets process-global machine name for client site identification.
      /// Assign on client to cluster/other machine names, by default the .ctor will use local computer name
      /// </summary>
      public static string MachineName;

      public ClientSite(string host)
      {
         if (host.IsNullOrWhiteSpace())
          host = ClientSite.MachineName;

         if (host.IsNullOrWhiteSpace()) host = System.Environment.MachineName;
         m_Host = host;
         m_AppInstanceID = App.InstanceID;
         m_Name = m_Host + '['+ m_AppInstanceID +']';
      }

      /// <summary>
      /// Deserializes ClientSite from stream. The stream position must be past total size
      /// </summary>
      public ClientSite(Stream stream)
      {
         var magic = stream.ReadBEInt32();
         if (magic != MAGIC)
           throw new ProtocolException(
                       StringConsts.GLUE_BAD_PROTOCOL_CLIENT_SITE_ERROR + "bad magic supplied '{0}' expected '{1}'".Args(magic, MAGIC));

         var hostLen = stream.ReadBEShort();
         if (hostLen<1 || hostLen>MAX_HOST_NAME_BYTE_LEN)
           throw new ProtocolException(
                           StringConsts.GLUE_BAD_PROTOCOL_CLIENT_SITE_ERROR + "host name length of {0} bytes can not be less than 1 or exceed the limit of {1} bytes"
                                                                        .Args(hostLen, MAX_HOST_NAME_BYTE_LEN));

         var buf = new byte[hostLen];
         stream.Read(buf, 0, hostLen);
         m_Host = TEXT_ENCODING.GetString(buf);


         var guid = new byte[GUID_SIZE];
         stream.Read(guid, 0, GUID_SIZE);
         m_AppInstanceID = new Guid( guid );

         m_Name = m_Host + '['+ m_AppInstanceID +']';
      }

      /// <summary>
      /// Writes ClientSite into stream including total size in bytes. Returns total size
      /// </summary>
      public int Serialize(Stream stream)
      {
         if (m_Host.IsNullOrWhiteSpace())
          throw new GlueException(StringConsts.ARGUMENT_ERROR + "CallSite.Host==null|empty");

         var buf = TEXT_ENCODING.GetBytes(m_Host);
         var blen = buf.Length;
         if (blen > MAX_HOST_NAME_BYTE_LEN)
           throw new ProtocolException(
                           StringConsts.GLUE_BAD_PROTOCOL_CLIENT_SITE_ERROR + "Requested host name '{0}' exceeds the limit of {1} bytes"
                                                                        .Args(m_Host, MAX_HOST_NAME_BYTE_LEN));

         var size = sizeof(short) + //TOTAL
                    sizeof(int)   + //MAGIC
                    sizeof(short) + //host name byte length
                    blen          + //h name (blen) bytes
                    GUID_SIZE;      //GUID

         stream.WriteBEShort((short)(size-sizeof(short)));
         stream.WriteBEInt32(MAGIC);
         stream.WriteBEShort((short)blen);
         stream.Write(buf, 0, blen);
         stream.Write( m_AppInstanceID.ToByteArray(), 0, GUID_SIZE );

         return size;
      }


      private string m_Name;
      private string m_Host;
      private Guid m_AppInstanceID;

      /// <summary>
      /// Returns host name+app instance guid suitable for registry operations
      /// </summary>
      public string Name { get{return m_Name; }}

      /// <summary>
      /// Returns host name for where calling application runs
      /// </summary>
      public string Host { get{return m_Host;}}

      /// <summary>
      /// Returns instance ID for the calling application container
      /// </summary>
      public Guid AppInstanceID { get{ return m_AppInstanceID; }}

      public override int GetHashCode() { return m_Name.GetHashCodeSenseCase();  }

      public override bool Equals(object obj)
      {
        if (!(obj is ClientSite)) return false;
        var other = (ClientSite)obj;
        return this.m_Name.EqualsSenseCase(other.m_Name);
      }

      public override string ToString() { return m_Name; }
  }


}