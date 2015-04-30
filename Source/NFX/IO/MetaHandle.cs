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

namespace NFX.IO
{
    /// <summary>
    /// Represents tuple of an unsigned integer with optional string metadata. If metadata is null then integer is stored by itself in an efficient way.
    /// The type is useful for storage of handles/indexes (such as pointer surrogates) with optional description of pointed-to data (such as type information).
    /// A special case is reserved for strings which are immutable yet reference types, in which case a special handle INLINED_STRING_HANDLE is set to indicate that 
    ///  "Metadata" really contains string data that this handle should resolve into. Check "IsInlinedString" property to see if string was inlined.
    /// Check "IsInlinedValueType" is set to true when a struct/valuetype is inlined and "Metadata" contains type spec 
    /// </summary>
    [Serializable]
    public struct MetaHandle     
    {                               
      internal const uint INLINED_STRING_HANDLE = 0;
      internal const uint INLINED_VALUETYPE_HANDLE = 1;
      internal const uint INLINED_REFTYPE_HANDLE = 2;
      internal const uint INLINED_TYPEVAL_HANDLE = 3;
      internal const uint HANDLE_OFFSET = 4;

      
      internal uint m_Handle;     
      private string m_Metadata;   


      /// <summary>
      /// Returns handle value. This value is invalid if special conditions such as inlining are true
      /// </summary>
      public uint Handle { get { unchecked {return m_Handle - HANDLE_OFFSET;}}}

      /// <summary>
      /// Indicates whether a string instance is inlined in Metadata property
      /// </summary>
      public bool IsInlinedString { get { return m_Handle == INLINED_STRING_HANDLE;}}

      /// <summary>
      /// Indicates whether a struct (value type) instance is inlined right after this handle and Metadata property contains type
      /// </summary>
      public bool IsInlinedValueType { get { return m_Handle == INLINED_VALUETYPE_HANDLE;}}

      /// <summary>
      /// Indicates whether a reference (reference type) instance is inlined right after this handle and Metadata property contains type.
      /// This is used for handling of ref types that are natively supported by streamers
      /// </summary>
      public bool IsInlinedRefType { get { return m_Handle == INLINED_REFTYPE_HANDLE;}}

      /// <summary>
      /// Indicates whether a reference to TYPE is inlined - that is a Metadata parameter points to the value of type (reference to Type)
      /// </summary>
      public bool IsInlinedTypeValue { get { return m_Handle == INLINED_TYPEVAL_HANDLE;}}

      
      public string Metadata { get { return m_Metadata;}}


      
      public MetaHandle(uint handle)
      {
        m_Handle = handle + HANDLE_OFFSET;
        m_Metadata = null;
      }

       public MetaHandle(bool serializer, uint handle)
      {
        m_Handle = handle + (serializer ? 0 : HANDLE_OFFSET);
        m_Metadata = null;
      }

      public MetaHandle(uint handle, string metadata)
      {
        m_Handle = handle + HANDLE_OFFSET;
        m_Metadata = metadata;
      }

      internal MetaHandle(bool serializer, uint handle, string metadata)
      {
        m_Handle = handle + (serializer ? 0 : HANDLE_OFFSET);
        m_Metadata = metadata;
      }

      /// <summary>
      /// Inlines string instance instead of pointer handle
      /// </summary>
      public static MetaHandle InlineString(string inlinedString)
      {
         var result = new MetaHandle();
         result.m_Handle = INLINED_STRING_HANDLE;
         result.m_Metadata = inlinedString;
         return result;
      }


      /// <summary>
      /// Inlines value type instead of pointer handle
      /// </summary>
      public static MetaHandle InlineValueType(string inlinedValueType)
      {
         var result = new MetaHandle();
         result.m_Handle = INLINED_VALUETYPE_HANDLE;
         result.m_Metadata = inlinedValueType;
         return result;
      }

      /// <summary>
      /// Inlines ref type instead of pointer handle
      /// </summary>
      public static MetaHandle InlineRefType(string inlinedRefType)
      {
         var result = new MetaHandle();
         result.m_Handle = INLINED_REFTYPE_HANDLE;
         result.m_Metadata = inlinedRefType;
         return result;
      }

      /// <summary>
      /// Inlines type value instead of pointer handle
      /// </summary>
      public static MetaHandle InlineTypeValue(string inlinedTypeValue)
      {
         var result = new MetaHandle();
         result.m_Handle = INLINED_TYPEVAL_HANDLE;
         result.m_Metadata = inlinedTypeValue;
         return result;
      }

      public override string ToString()
      {
          return string.Format("[{0}] {1}",
               IsInlinedString ? "string" : IsInlinedValueType ? "struct" : IsInlinedRefType ? "refobj" : Handle.ToString(),
               Metadata ?? StringConsts.NULL_STRING);
      }

      public override int GetHashCode()
      {
          return m_Handle.GetHashCode();
      }

      public override bool Equals(object obj)
      {
          if (obj==null) return false;
          var other = (MetaHandle)obj;
          return m_Handle==other.m_Handle && ((m_Metadata==null && other.m_Metadata==null) || (m_Metadata == other.m_Metadata));
      }
    }
}
