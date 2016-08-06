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
using System.Reflection;

using NFX.Environment;

namespace NFX.Wave.Handlers
{

    /// <summary>
    /// Represents a location used for dynamic type searches
    /// </summary>
    public sealed class TypeLocation : INamed, IOrdered
    {
       public const string CONFIG_TYPE_LOCATION_SECTION = "type-location";
       public const string CONFIG_ASSEMBLY_NAME_ATTR = "assembly";
       public const string CONFIG_NAMESPACE_SECTION = "ns";

       public const string CONFIG_PORTAL_ATTR = "portal";

       private string m_Name;
       private int m_Order;

       /// <summary>
       /// Location name
       /// </summary>
       public string Name
       {
          get { return m_Name;}
       }

       /// <summary>
       /// Location order
       /// </summary>
       public int Order
       {
          get { return m_Order;}
       }

       /// <summary>
       /// Name of assembly. When this property is set then Assembly==null
       /// </summary>
       public readonly string AssemblyName;

       /// <summary>
       /// Assembly reference. When this property is set then AssemblyName==null
       /// </summary>
       public readonly Assembly Assembly;

       /// <summary>
       /// Name of portal. When this property is set then this location will only be matched if
       ///  WorkContext.Portal.Name matches for given request. NUll by default
       /// </summary>
       public readonly string Portal;

       /// <summary>
       /// A list of namespaces
       /// </summary>
       public readonly IEnumerable<string> Namespaces;


       public TypeLocation(string name, int order, string portal, string assemblyName, params string[] namespaces)
       {
         if (assemblyName.IsNullOrWhiteSpace())
          throw new WaveException(StringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(assemblyName==null|empty)");

         if (name.IsNullOrWhiteSpace()) name = Guid.NewGuid().ToString();
         m_Name = name;
         m_Order = order;
         Portal = portal;
         AssemblyName = assemblyName;
         Namespaces = namespaces;
       }

       public TypeLocation(string name, int order, string portal, Assembly assembly, params string[] namespaces)
       {
         if (assembly==null)
          throw new WaveException(StringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(assembly==null|empty)");

         if (name.IsNullOrWhiteSpace()) name = Guid.NewGuid().ToString();
         m_Name = name;
         m_Order = order;
         Portal = portal;
         Assembly = assembly;
         Namespaces = namespaces;
       }

       public TypeLocation(IConfigSectionNode confNode)
       {
         if (confNode==null)
          throw new WaveException(StringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(confNode==null)");

         m_Name = confNode.AttrByName(Configuration.CONFIG_NAME_ATTR).ValueAsString( Guid.NewGuid().ToString() );
         m_Order = confNode.AttrByName(Configuration.CONFIG_ORDER_ATTR).ValueAsInt();
         Portal = confNode.AttrByName(CONFIG_PORTAL_ATTR).Value;
         AssemblyName = confNode.AttrByName(CONFIG_ASSEMBLY_NAME_ATTR).Value;

         if (AssemblyName.IsNullOrWhiteSpace())
          throw new WaveException(StringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(config{$assembly==null|empty})");

         List<string> nsList = null;
         foreach(var ns in confNode.Children
                                   .Where(cn=>cn.IsSameName(CONFIG_NAMESPACE_SECTION))
                                   .Select(cn=>cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value))
            if (ns.IsNotNullOrWhiteSpace())
            {
              if (nsList==null) nsList = new List<string>();
              nsList.Add(ns);
            }

         Namespaces = nsList;
       }
    }



    /// <summary>
    /// A list of type search locations used for dynamic type searches
    /// </summary>
    public sealed class TypeLocations : OrderedRegistry<TypeLocation>
    {
       public TypeLocations(): base() { }
    }


    /// <summary>
    /// Lookup table string->Type
    /// </summary>
    internal sealed class TypeLookup : Dictionary<string, Type>
    {
      public TypeLookup() : base(StringComparer.Ordinal)
      {

      }

      public TypeLookup(TypeLookup clone) : base(clone, StringComparer.Ordinal)
      {

      }
    }
}
