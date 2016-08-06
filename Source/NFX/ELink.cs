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

using NFX.DataAccess.Distributed;

namespace NFX
{
    /// <summary>
    /// Represents an Electronic Link which is an alpha-encoded identifier along with metadata information.
    /// Warning! This class MAY generate fragments of profanity, however any ID can be regenerated using a different seed passed to Encode(seed)
    /// </summary>
    public sealed class ELink
    {
        #region CONSTS

            /// <summary>
            /// Represents how many variations every link has (seeds)
            /// </summary>
            public const int VARIATIONS = 0xf;

            private const string ALPHABET_SOURCE =
@"
  AB, AC, AU, AK, AD, AS, AM, AL, AZ, AG, AN, AF, AP, AQ, AR, AX,
  AI, AJ, AT, AW, BA, BE, BI, BO, BU, BR, CA, CB, CD, CE, CI, CH,
  CO, CU, CR, CK, DA, DB, DE, DI, DO, DU, EB, ED, EF, EG, EJ, EK,
  EL, EM, EN, EP, EQ, ER, ES, ET, EV, EW, EX, EZ, FA, FE, FI, FO,
  FU, FT, FZ, FL, GA, GE, GI, GL, GM, GN, GO, GP, GR, GU, GV, GW,
  GX, GZ, HA, HE, HI, HM, HN, HO, HP, HT, HU, IB, IC, ID, IF, IG,
  IT, IK, IJ, IL, IM, IN, IP, IR, IS, IV, IW, IX, IZ, JA, JE, JI,
  JO, JZ, JU, JK, JC, JN, KA, KE, KO, KU, KI, KR, KL, KN, KY, LA,
  LE, LI, LM, LN, LO, LR, LS, LT, LU, LV, LY, LZ, MA, MB, MC, MD,
  ME, MI, MO, MU, MR, MN, MY, MZ, NA, NE, NI, NM, NO, NU, NY, NX,
  NZ, OB, OC, OD, OF, OA, OG, OH, OI, OJ, OK, OL, OM, OP, OR, OS,
  OT, OV, OW, OX, OY, OZ, PA, PE, PI, PO, PS, PU, PY, PZ, QU, QA,
  RA, RE, RI, RO, RU, RY, SA, SE, SI, SO, SU, SY, SH, SM, SN, SB,
  SD, ST, SV, SQ, TA, TE, TI, TO, TM, TN, TR, TS, TU, TV, TY, TZ,
  UB, UC, UD, UF, UG, UK, UL, UM, UN, UP, UR, UZ, VA, VE, VI, VO,
  VU, ZA, ZE, ZI, ZO, ZU, WA, WE, WI, WO, WU, WR, YA, YE, YO, XA
";

           public static readonly string[] ALPHABET;
           public static readonly Dictionary<string, int> RALPHABET;

           public const int MAX_LINK_CHAR_SIZE = 1024;

           static ELink()
           {
                ALPHABET = new string[256];
                RALPHABET = new Dictionary<string,int>(256);

                var lst = ALPHABET_SOURCE.Split(',');
                for(int i=0, j=0; j< ALPHABET.Length; i++)
                {
                    var e = lst[i].Trim(' ', '\n', '\r').ToUpperInvariant();
                    if (e.IsNullOrWhiteSpace()) continue;
                    ALPHABET[j] = e;
                    RALPHABET[e] = j;
                    j++;
                }
           }


        #endregion


        #region .ctor
            /// <summary>
            /// Creates an Elink instance initialized with GDID of 0 Era having its ID set to ulong value
            /// </summary>
            public ELink(UInt64 id, byte[] metadata)
            {
                m_GDID = new GDID(0, id);
                m_Metadata = metadata;

                if (m_Metadata!=null && ((m_Metadata.Length*2) > MAX_LINK_CHAR_SIZE))
                  throw new NFXException(StringConsts.ELINK_CHAR_LENGTH_LIMIT_ERROR.Args("metadata[{0}]".Args(m_Metadata.Length)));
            }

            /// <summary>
            /// Create ELink instance from GDID (with era).
            /// </summary>
            public ELink(GDID gdid, byte[] metadata = null)
            {
                m_GDID = gdid;
                m_Metadata = metadata;

                if (m_Metadata!=null && ((m_Metadata.Length*2) > MAX_LINK_CHAR_SIZE))
                  throw new NFXException(StringConsts.ELINK_CHAR_LENGTH_LIMIT_ERROR.Args("metadata[{0}]".Args(m_Metadata.Length)));
            }

            public ELink(string link)
            {
                if (link.IsNullOrWhiteSpace()) throw new NFXException(StringConsts.ARGUMENT_ERROR + "ELink.ctor(link=null|empty)");

                if (link.Length > MAX_LINK_CHAR_SIZE)
                  throw new NFXException(StringConsts.ELINK_CHAR_LENGTH_LIMIT_ERROR.Args(link.Substring(0, 20)));

                m_Link = link;
                decode();
            }

        #endregion

        #region Fields

            private GDID m_GDID;
            private byte[] m_Metadata;

            private string m_Link;

        #endregion

        #region Properies

            /// <summary>
            /// Returns the ID portion of GDID represented by this instance
            /// </summary>
            public UInt64 ID { get{ return m_GDID.ID; }}

            /// <summary>
            /// Returns metadata attached to this instance, or null if there is no metadata specified
            /// </summary>
            public byte[] Metadata { get { return m_Metadata;}}

            /// <summary>
            /// Returns a link encoded as a string using whatever randomization seed was passed to the last Encode(seed) call.
            /// If Encode() was not called, then the link will get encoded using system rnd for a seed value
            /// </summary>
            public string Link
            {
                get
                {
                    if (m_Link==null) Encode();
                    return m_Link;
                }
            }

            /// <summary>
            /// Returns the GDID that this link represents
            /// </summary>
            public GDID GDID { get{ return m_GDID;}}

            public GDIDSymbol AsGDIDSymbol { get{ return new GDIDSymbol(m_GDID, Link); }}

        #endregion

        #region Methods

            /// <summary>
            /// Encodes a link into a textual form, using the supplied randomization seed, otherwise the system rnd is used.
            /// A seed has 4 effective bits, yielding 16 possible variations for every link
            /// </summary>
            public string Encode(byte? seed = null) //props -> link
            {

                /* Format
                 * [lead1][lead2][csum] - [... era ...] - [... id ...]{-[...metadata...]}
                 *  1bt     1bt    1bt       0..4bts        1..8bts         md-len bts
                 *
                 * [lead1] = [rnd][authority]  rnd 0..f
                 *            4bi     4bi
                 * [lead2] = [eraLen][idLen]  idLen is w/o authority
                 *            4bi     4bi

                 * [csum] = CRC32(era, id, md_len, metadata_bytes) % 0xff
                 *
                 * [era] = LitEndian uint32 0..4 bytes
                 * [id] = LitEndian 1..8 bytes LitEndian uint64 w/o authority
                 *
                 *
                 * [metadata] = 0..limited by MAX_LINK_CHAR_SIZE/2
                 *
                 */



                int rnd = (!seed.HasValue ? (byte)(ExternalRandomGenerator.Instance.NextRandomInteger & 0xff) : seed.Value) & VARIATIONS;
                var era = m_GDID.Era;
                var id = m_GDID.ID;

                var eraLength =
                     (era & 0xff000000) > 0 ? 4 :
                     (era &   0xff0000) > 0 ? 3 :
                     (era &     0xff00) > 0 ? 2 :
                     (era &       0xff) > 0 ? 1 : 0;

                var idLength =
                      (id & 0x0f00000000000000) > 0 ? 8 :  //notice Authority exclusion in mask
                      (id &   0xff000000000000) > 0 ? 7 :
                      (id &     0xff0000000000) > 0 ? 6 :
                      (id &       0xff00000000) > 0 ? 5 :
                      (id &         0xff000000) > 0 ? 4 :
                      (id &           0xff0000) > 0 ? 3 :
                      (id &             0xff00) > 0 ? 2 : 1;

               var lead1 = (rnd << 4) | m_GDID.Authority;
               var lead2 = (eraLength << 4) | idLength;

               var crcValue = crc(era, id, m_Metadata);

                rnd |= rnd << 4;

                var result = new StringBuilder();
                result.Append( ALPHABET[ lead1 ] );
                result.Append( ALPHABET[ lead2 ^ rnd ]);
                result.Append( ALPHABET[ crcValue ^ rnd ]);

                result.Append( '-' );

                ulong seg = era;
                if (seg>0)
                {
                  for(var i=0; i<eraLength; i++)
                  {
                      result.Append( ALPHABET[ (int)(seg & 0xff)  ^ rnd ] );
                      seg >>= 8;
                  }
                }

                seg = id;
                for(var i=0; i<idLength; i++)
                {
                    if (i==4 && idLength>5)
                     result.Append( '-' );

                    result.Append( ALPHABET[ (int)(seg & 0xff)  ^ rnd ] );
                    seg >>= 8;
                }

                if (m_Metadata!=null)
                {
                   result.Append( '-' );
                   for(var i=0; i<m_Metadata.Length; i++)
                        result.Append( ALPHABET[ m_Metadata[i] ^ rnd ] );
                }

                var link = result.ToString();

                if (link.Length > MAX_LINK_CHAR_SIZE)
                  throw new NFXException(StringConsts.ELINK_CHAR_LENGTH_LIMIT_ERROR.Args(link.Substring(0, 20)));

                m_Link = link;

                return link;
            }

            private byte crc(uint era, ulong id, byte[] md)
            {
              ulong sum = era * 0xaa55aa55;
              sum ^= id;
              if (md!=null)
               sum ^= ((ulong)md.Length * 0xee77);

              return (byte)(sum % 251);
            }



            private void decode() //link -> props
            {
               List<byte> data = new List<byte>(32);

               char pc = (char)0;
               for(var i=0; i<m_Link.Length; i++)
               {
                    char c = m_Link[i];
                    if (c=='-' || c==' ') continue;
                    if (pc!=(char)0)
                    {
                        var seg = string.Concat(pc, c).ToUpperInvariant();
                        pc = (char)0;
                        var sid = 0;
                        if (!RALPHABET.TryGetValue(seg, out sid))
                            throw new NFXException(StringConsts.ELINK_CHAR_COMBINATION_ERROR.Args(m_Link, seg));
                        data.Add((byte)sid);
                    }
                    else
                     pc = c;
               }

               if (data.Count<4 || pc!=(char)0)
                    throw new NFXException(StringConsts.ELINK_CHAR_LENGTH_ERROR.Args(m_Link));

               //2 control bytes
               var lead1 = data[0];
                 var rnd = (lead1 & 0xf0) >> 4;
                 rnd |= rnd << 4;
                 var authority = lead1 & 0x0f;


               var lead2 = data[1] ^ rnd;
                 var eraLength = (lead2 & 0xf0) >> 4;
                 var idLength = lead2 & 0x0f;

               var csum = data[2] ^ rnd;

               if (eraLength>4 || idLength<1 || idLength>8)
                  throw new NFXException(StringConsts.ELINK_SEGMENT_LENGTH_ERROR.Args(m_Link));


               if (data.Count-3 < eraLength + idLength)
                   throw new NFXException(StringConsts.ELINK_CHAR_LENGTH_ERROR.Args(m_Link));

                UInt32 era = 0;
                var idx = 3;
                if (eraLength>0)
                {
                  for(var i=0; i<eraLength; i++,idx++)
                      era |=  (UInt32)((byte)(data[idx] ^ rnd)) << (8 * i);
                }

                UInt64 id = 0;
                if (idLength>0)
                {
                  for(var i=0; i<idLength; i++,idx++)
                      id |=  (UInt64)((byte)(data[idx] ^ rnd)) << (8 * i);
                }

                id |= ((ulong)authority << 60);


                byte[] metadata = null;
                if (idx<data.Count)
                {
                  metadata = new byte[data.Count - idx];
                  for(var j=0; idx<data.Count; idx++, j++)
                    metadata[j] = (byte)(data[idx] ^ rnd);
                }

                var thiscsum = crc(era, id, metadata);
                if (csum!=thiscsum)
                  throw new NFXException(StringConsts.ELINK_CSUM_MISMATCH_ERROR.Args(m_Link));

                m_GDID = new GDID(era, id);
                m_Metadata = metadata;
            }

        #endregion

    }
}