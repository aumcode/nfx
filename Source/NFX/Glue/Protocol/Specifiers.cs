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
using System.Reflection;
using System.Text;

namespace NFX.Glue.Protocol
{
    /// <summary>
    /// Hash helpers used by Glue protocol for type resolution
    /// </summary>
    public static class HashUtils
    {

        /// <summary>
        /// Converts string identifier into uint64 stable hash that does not depend on a platform.
        /// This functions optimized for hashing identifiers/type names
        /// </summary>
        public static ulong StringIDHash(string id)
        {
            if (id==null) return 0;
            var sl = id.Length;
            if (sl==0) return 0;
            ulong result = (ulong)(sl % 251) << 56;//64-8

            ulong hash2 = 0;
            for(int i=id.Length-1, cnt=0; cnt<sizeof(ulong)-1 && i>=0; i--,cnt++)
            {
              if (cnt>0) hash2 <<= 8;
              hash2 |= (byte)id[i];
            }

            result |= (ulong)hash2;
            return result;
        }

        /// <summary>
        /// Returns stable ulong hash for a type that does not depend on a platform
        /// </summary>
        public static ulong TypeHash(Type type)
        {
          if (type==null) return 0;
          if (type==typeof(void)) return 0;

          var aqn = type.AssemblyQualifiedName;
          var crc32 = NFX.IO.ErrorHandling.CRC32.ForString(aqn);

          var tp  = StringIDHash(type.FullName);

          return tp ^ ( ((ulong)crc32) << 24);//64-8-32
          //[tp.len%251][crc1][crc2][crc3][crc4][char-3][char-2][char-1]
        }
    }










    /// <summary>
    /// Type specification for marshalling contract types between glued peers
    /// </summary>
    [Serializable]
    public struct TypeSpec
    {
        public TypeSpec(Type type)
        {
           m_Name = type.AssemblyQualifiedName;
           m_Hash = HashUtils.TypeHash(type);
        }

        internal string m_Name;
        internal ulong  m_Hash;

        public string Name{get{return m_Name;}}
        public ulong Hash{get{return m_Hash;}}



                   private static Dictionary<string, Type> s_Types = new Dictionary<string,Type>(StringComparer.Ordinal);

        /// <summary>
        /// Returns the type or throws if it can't be found
        /// </summary>
        /// <returns>The type or throws exception if actual type could not be gotten</returns>
        public Type GetSpecifiedType()
        {
          Type result;

          if (m_Name==null) throw new ServerContractException(StringConsts.GLUE_TYPE_SPEC_ERROR + StringConsts.NULL_STRING);

          if (s_Types.TryGetValue(m_Name, out result)) return result;

          result = Type.GetType(m_Name, false, false);
          if (result==null)
           throw new ServerContractException(StringConsts.GLUE_TYPE_SPEC_ERROR + m_Name);

          var dict = new Dictionary<string,Type>(s_Types, StringComparer.Ordinal);
          dict[m_Name] = result;
          s_Types = dict;//atomic

          return result;
        }

        public override int GetHashCode()
        {
          return (int)(m_Hash >> 32) ^ (int)m_Hash;
        }

        public override bool Equals(object obj)
        {
          if (!(obj is TypeSpec)) return false;
          var other = (TypeSpec)obj;
          return this.m_Name.EqualsOrdSenseCase(other.m_Name);
        }

        public override string ToString()
        {
          return "TypeSpec('{0}', {1})".Args(m_Name, m_Hash);
        }

    }


    /// <summary>
    /// Method specification for marshalling method information between glued peers
    /// </summary>
    [Serializable]
    public struct MethodSpec
    {
        public MethodSpec(MethodInfo mi)
        {
            m_MethodName = mi.Name;

            var rtp = mi.ReturnType;
            m_ReturnType = HashUtils.TypeHash( rtp );

            var pars = mi.GetParameters();
            m_Signature = new byte[pars.Length*sizeof(ulong)];
            for(var i=0; i<pars.Length; i++)
            {
               var par = pars[i];
               if (par.ParameterType.IsByRef || par.IsOut || par.ParameterType.IsGenericParameter)
                  throw new ServerContractException(StringConsts.GLUE_METHOD_SPEC_UNSUPPORTED_ERROR.Args(mi.DeclaringType.FullName, mi.Name, par.Name));

               m_Signature.WriteBEUInt64( i *sizeof(ulong), HashUtils.TypeHash(pars[i].ParameterType) );
            }

            m_Hash =  HashUtils.StringIDHash(m_MethodName) ^ m_ReturnType ^ ((ulong)m_Signature.Length * 17);
        }


        internal string m_MethodName;
        internal ulong  m_ReturnType;
        internal byte[] m_Signature ; //this is byte[] vs ulong[] for serialization speed
        internal ulong  m_Hash      ;


        public string MethodName{get{ return m_MethodName;}}
        public ulong  ReturnType{get{ return m_ReturnType;}}
        public byte[] Signature {get{ return m_Signature;}} //this is byte[] vs ulong[] for serialization speed
        public ulong  Hash      {get{ return m_Hash;}}


        public static bool operator ==(MethodSpec left, MethodSpec right)
        {
          if (!left.m_MethodName.EqualsOrdSenseCase(right.m_MethodName)) return false;

          if (left.m_ReturnType != right.m_ReturnType ) return false;

          //20140625 DKh+DLat
          if (!left.m_Signature.MemBufferEquals(right.m_Signature)) return false;

          return true;
        }

        public static bool operator !=(MethodSpec left, MethodSpec right)
        {
          return !(left==right);
        }

        public override bool Equals(object obj)
        {
            if (obj==null) return false;
            return this == (MethodSpec)obj;
        }

        public override int GetHashCode()
        {
            return (int)(m_Hash >> 32) ^ (int)m_Hash;
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1}({2})", m_ReturnType, m_MethodName, m_Signature.Length / sizeof(ulong));
        }
    }
}
