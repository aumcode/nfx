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

namespace NFX.Inventorization
{

    /// <summary>
    /// Performs basic inventorization of Inventory-marked types and their members
    /// </summary>
    public class BasicInventorization : IInventorization
    {
        public void Inventorize(Type t, ConfigSectionNode root)
        {
           InventorizationManager.WriteInventoryAttributes(
                 t.GetCustomAttributes(typeof(InventoryAttribute), false).Cast<InventoryAttribute>(),
                 root.AddChildNode(InventorizationManager.ATTRIBUTES_NODE));

           if (t.BaseType!=null)
            root.AddAttributeNode("base", t.BaseType.FullName);

           root.AddAttributeNode("abstract",  t.IsAbstract);
           root.AddAttributeNode("class",  t.IsClass);
           root.AddAttributeNode("enum",  t.IsEnum);
           root.AddAttributeNode("intf",  t.IsInterface);
           root.AddAttributeNode("nested",  t.IsNested);
           root.AddAttributeNode("public",  t.IsPublic);
           root.AddAttributeNode("sealed",  t.IsSealed);
           root.AddAttributeNode("serializable",  t.IsSerializable);
           root.AddAttributeNode("valuetype",  t.IsValueType);
           root.AddAttributeNode("visible",  t.IsVisible);

           var members = t.GetMembers().Where(m=>m.GetCustomAttributes(typeof(InventoryAttribute), false).Count()>0);
           foreach(var mem in members)
           {
             var mnode = root.AddChildNode("member");
             mnode.AddAttributeNode("name", mem.Name);
             mnode.AddAttributeNode("kind", mem.MemberType.ToString());

             InventorizationManager.WriteInventoryAttributes(
                 mem.GetCustomAttributes(typeof(InventoryAttribute), false).Cast<InventoryAttribute>(),
                 mnode.AddChildNode(InventorizationManager.ATTRIBUTES_NODE));

             if (mem is PropertyInfo)
             {
               var pinf = (PropertyInfo)mem;
               mnode.AddAttributeNode("can-get", pinf.CanRead);
               mnode.AddAttributeNode("can-set", pinf.CanWrite);
               mnode.AddAttributeNode("type", pinf.PropertyType.FullName);
             }
             else
             if (mem is FieldInfo)
             {
               var finf = (FieldInfo)mem;
               mnode.AddAttributeNode("not-serialized", finf.IsNotSerialized);
               mnode.AddAttributeNode("public", finf.IsPublic);
               mnode.AddAttributeNode("private", finf.IsPrivate);
               mnode.AddAttributeNode("static", finf.IsStatic);
               mnode.AddAttributeNode("type", finf.FieldType.FullName);
             }
             else
             if (mem is MethodInfo)
             {
               var minf = (MethodInfo)mem;
               mnode.AddAttributeNode("virtual", minf.IsVirtual);
               mnode.AddAttributeNode("public", minf.IsPublic);
               mnode.AddAttributeNode("private", minf.IsPrivate);
               mnode.AddAttributeNode("static", minf.IsStatic);
               if (minf.ReturnType!=null)
                mnode.AddAttributeNode("return-type", minf.ReturnType.FullName);
             }
           }

        }
    }
}
