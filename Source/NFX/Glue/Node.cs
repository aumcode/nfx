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

namespace NFX.Glue
{
    /// <summary>
    /// Represents a network node. It is a binding, logical address of a host and a service that host provides
    /// Nodes are not contract-dependent. The componets of address are not case-sensitive.
    /// The form of the address is: <code>binding://host:service</code>. The "host" and "service" segment syntaxes depend on binding and may not contain the ':' char.
    /// An example of some 'mytest' binding: 'mytest://adr=1.1.1.1,nic=eth001:job,chat,backup'
    /// </summary>
    [Serializable]
    public struct Node : INamed
    {
      public const string BINDING_SEPARATOR = "://";
      public const string SERVICE_SEPARATOR = ":";


      //the string is stored as a whole for serialization efficiency
      private string m_ConnectString;

      /// <summary>
      /// Inits a node struct. It is a binding, logical address of a host and a service that host provides
      /// Nodes are not contract-dependent. The componets of address are not case-sensitive.
      /// The form of the address is: <code>binding://host:service</code>. The "host" and "service" segment syntaxes depend on binding and may not contain the ':' char.
      /// An example of some 'mytest' binding: 'mytest://adr=1.1.1.1,nic=eth001:job,chat,backup'
      /// </summary>
      public Node(string connectString)
      {
        if (string.IsNullOrWhiteSpace(connectString))
         throw new GlueException(StringConsts.ARGUMENT_ERROR + "Node.ctor(null)");

        m_ConnectString = connectString;
      }


      /// <summary>
      /// Gets a connection string - a structured URL-like connection descriptor that identifies a host
      ///  along with binding and service. The componets of address are not case-sensitive.
      /// The form of the address is: <code>binding://host:service</code>. The "host" and "service" segment syntaxes depend on binding and may not contain the ':' char.
      /// An example of some 'mytest' binding: 'mytest://adr=1.1.1.1,nic=eth001:job,chat,backup'
      /// </summary>
      public string ConnectString { get { return m_ConnectString ?? StringConsts.NULL_STRING; }  }


      /// <summary>
      /// INamed shortcut to ConnectString
      /// </summary>
      public string Name { get{ return ConnectString;} }

      /// <summary>
      /// Returns true when struct has some data assigned i.e. connect string is specified
      /// </summary>
      public bool Assigned
      {
        get { return !string.IsNullOrEmpty(m_ConnectString);}
      }


      /// <summary>
      /// Gets binding portion of ConnectString. This value selects binding adapter
      /// </summary>
      public string Binding
      {
        get
        {
          var i = m_ConnectString.IndexOf(BINDING_SEPARATOR);
          if (i<=0) return string.Empty;
          return m_ConnectString.Substring(0, i);
        }
      }

      /// <summary>
      /// Gets host portion of ConnectString. This value may have a structure of its own which is understood by binding adapter
      /// </summary>
      public string Host
      {
       get
       {
         var i = m_ConnectString.IndexOf(BINDING_SEPARATOR);
         if (i<0)//binding spec missing
         {
           i = m_ConnectString.IndexOf(SERVICE_SEPARATOR);
           if (i<=0) return m_ConnectString;
           return m_ConnectString.Substring(0, i);
         }
         else
         {
           i+=BINDING_SEPARATOR.Length;
           var j= m_ConnectString.IndexOf(SERVICE_SEPARATOR, i);
           if (j<0) return m_ConnectString.Substring(i);
           return m_ConnectString.Substring(i, j-i);
         }
       }
      }

      /// <summary>
      /// Gets service/port portion of ConnectString. This value may have a structure of its own which is understood by binding adapter
      /// </summary>
      public string Service
      {
       get
       {
         var i = m_ConnectString.IndexOf(BINDING_SEPARATOR);
         var j = m_ConnectString.LastIndexOf(SERVICE_SEPARATOR);

         if (j<0 || j<=i || j+1>=m_ConnectString.Length) return string.Empty;

         return m_ConnectString.Substring(j+1);

       }
      }


      public override string ToString()
      {
          return m_ConnectString ?? CoreConsts.UNKNOWN;
      }

      public override int GetHashCode()
      {
          return  m_ConnectString==null ? 0 :  this.m_ConnectString.GetHashCodeOrdIgnoreCase();
      }

      public override bool Equals(object obj)
      {
          if (obj==null) return false;
          if (! (obj is Node)) return false;

          var other = (Node)obj;

          return this.Assigned &&
                 other.Assigned &&
                 this.m_ConnectString.EqualsOrdIgnoreCase(other.m_ConnectString);
      }


    }

}
