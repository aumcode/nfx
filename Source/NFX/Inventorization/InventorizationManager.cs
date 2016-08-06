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
    /// Defines a list of inventorization strategies
    /// </summary>
    public sealed class Strategies : List<IInventorization> {}


    /// <summary>
    /// Performs an inventory scan of supplied assemblies using specified options.
    /// Items to be included in result must be tagged with Inventory attribute, otherwise then will be omitted unless OnlyAttributed == false.
    /// The scan is performed using options and inventorization strategies.
    /// This class is NOT THREAD SAFE
    /// </summary>
    public class InventorizationManager
    {
      #region CONSTS

        public const string ATTRIBUTES_NODE = "attributes";

      #endregion




      #region .ctor
       public InventorizationManager(IEnumerable<Assembly> assemblies)
       {
         m_Assemblies.AddRange(assemblies);
       }

       public InventorizationManager(IEnumerable<string> assemblies)
       {
         loadAssemblies(assemblies);
       }

       /// <summary>
       /// Delimiter assembly names with ';'
       /// </summary>
       public InventorizationManager(string assemblies, string path = null)
       {
         loadAssemblies(assemblies.Split(';').Where(s => !string.IsNullOrWhiteSpace(s)), path);
       }

       private void loadAssemblies(IEnumerable<string> assemblies, string path = null)
       {
         foreach(var name in assemblies)
             try
             {
                    var n = name;
                    if (!string.IsNullOrWhiteSpace(path))
                     n = System.IO.Path.Combine(path, name);
                    var asm = Assembly.LoadFrom(n);
                    m_Assemblies.Add(asm);
             }
             catch(Exception error)
             {
               throw new InventorizationException(StringConsts.INVENTORIZATION_ASSEMBLY_LOAD_ERROR + name, error);
             }
       }

      #endregion


      #region .pvt .fields

        private List<Assembly> m_Assemblies = new List<Assembly>();

        private IConfigSectionNode m_Options;

        private bool m_OnlyAttributed = true;
        private SystemTiers?    m_Tiers;
        private SystemConcerns? m_Concerns;
        private string m_Technology;
        private string m_Schema;
        private string m_Tool;

        private DateTime? m_StartDate;
        private DateTime? m_EndDate;


        private Strategies m_Startegies = new Strategies();
      #endregion


      #region Properties


        /// <summary>
        /// References inventorization strategies
        /// </summary>
        public Strategies Strategies
        {
          get { return m_Startegies;}
        }


        /// <summary>
        /// Options for inventorization strategies
        /// </summary>
        public IConfigSectionNode Options
        {
          get { return m_Options; }
          set { m_Options = value; }
        }


        /// <summary>
        /// When true, inventorizes ONLY items that have have Inventory attribute applied.
        /// Even if this property is false but some other inventory filter specified, then an item must be Inventory-tagged to be included
        /// </summary>
        [Config("$attributed", Default=true)]
        public bool OnlyAttributed
        {
          get { return m_OnlyAttributed;}
          set { m_OnlyAttributed = value;}
        }


        /// <summary>
        /// Imposes a filter on system tiers parameter of Inventory attribute
        /// </summary>
        [Config("$tiers")]
        public SystemTiers?   Tiers
        {
          get { return m_Tiers;}
          set { m_Tiers = value;}
        }

        /// <summary>
        /// Imposes a filter on system concerns parameter of Inventory attribute
        /// </summary>
        [Config("$concerns")]
        public SystemConcerns? Concerns
        {
          get { return m_Concerns; }
          set { m_Concerns = value;}
        }

        /// <summary>
        /// Imposes a filter on technology parameter of Inventory attribute
        /// </summary>
        [Config("$technology")]
        public string Technology
        {
          get{ return m_Technology;}
          set { m_Technology = value;}
        }

        /// <summary>
        /// Imposes a filter on schema parameter of Inventory attribute
        /// </summary>
        [Config("$schema")]
        public string Schema
        {
          get{ return m_Schema;}
          set { m_Schema = value;}
        }


        /// <summary>
        /// Imposes a filter on tool parameter of Inventory attribute
        /// </summary>
        [Config("$tool")]
        public string Tool
        {
          get{ return m_Tool;}
          set { m_Tool = value;}
        }


        /// <summary>
        /// Imposes a filter on StartDate parameter of Inventory attribute
        /// </summary>
        [Config("$startdate")]
        public DateTime? StartDate
        {
          get { return m_StartDate; }
          set { m_StartDate = value; }
        }

        /// <summary>
        /// Imposes a filter on EndDate parameter of Inventory attribute
        /// </summary>
        [Config("$enddate")]
        public DateTime? EndDate
        {
          get { return m_EndDate; }
          set { m_EndDate = value; }
        }




      #endregion




       /// <summary>
       /// Runs inventorization routine dumping result into config node
       /// </summary>
       public void Run(ConfigSectionNode root)
       {
          if (m_Startegies.Count==0)
           throw new InventorizationException(StringConsts.INVENTORIZATION_NEED_STRATEGY_ERROR);

          foreach(var asm in m_Assemblies.Where(a => filter(a)))
          {
            var asmnode = root.AddChildNode("assembly");
            asmnode.AddAttributeNode("name", asm.FullName);
            WriteInventoryAttributes(asm.GetCustomAttributes(typeof(InventoryAttribute), false).Cast<InventoryAttribute>(),
                                     asmnode.AddChildNode(ATTRIBUTES_NODE));


            var types = asm.GetTypes();
            foreach(var type in types.Where(t => filter(t)).OrderBy(t => t.FullName))
            {
             var tnode = asmnode.AddChildNode("type");
             tnode.AddAttributeNode("ns", type.Namespace);
             tnode.AddAttributeNode("name", type.Name);

             foreach(var strategy in m_Startegies)
                strategy.Inventorize(type, tnode);
            }
          }
       }


       /// <summary>
       /// Adds nodes for InventoryAttributes
       /// </summary>
       public static void WriteInventoryAttributes(IEnumerable<InventoryAttribute> attrs, ConfigSectionNode root)
       {
         foreach(var attr in attrs)
         {
           var node = root.AddChildNode("item");
           node.AddAttributeNode("tiers", attr.Tiers.ToString());
           node.AddAttributeNode("concerns", attr.Concerns.ToString());
           node.AddAttributeNode("schema", attr.Schema);
           node.AddAttributeNode("tech", attr.Technology);
           node.AddAttributeNode("tool", attr.Tool);

           if (attr.StartDate.HasValue)
             node.AddAttributeNode("sdate", attr.StartDate.Value);

           if (attr.EndDate.HasValue)
             node.AddAttributeNode("edate", attr.EndDate.Value);

           if (attr.Parameters!=null)
             node.AddChildNode("params", attr.Parameters);
         }
       }

       /// <summary>
       /// Describes type including generic arguments
       /// </summary>
       public static void WriteType(Type t, ConfigSectionNode node)
        {
           node.AddAttributeNode("name-in-code", tname(t));
           node.AddAttributeNode("name", t.Name);
           if (t==typeof(void))
           {
            node.AddAttributeNode("void", true);
            return;
           }
           node.AddAttributeNode("ns", t.Namespace);
           node.AddAttributeNode("asm-q-name", t.AssemblyQualifiedName);
           node.AddAttributeNode("asm-name", t.Assembly.FullName);
           node.AddAttributeNode("intf", t.IsInterface);
           node.AddAttributeNode("valuetype", t.IsValueType);

           if (t.IsEnum)
           {
             var enode = node.AddChildNode("enum");
             foreach(var name in Enum.GetNames(t))
             {
               var inode = enode.AddChildNode("item");
               inode.AddAttributeNode("key", name);
               inode.AddAttributeNode("value", (int)Enum.Parse(t, name));
             }
           }

           if (t.IsGenericType)
           {
             var ganode = node.AddChildNode("generic-type-args");
             foreach(var garg in t.GetGenericArguments())
               WriteType(garg, ganode.AddChildNode("type-arg"));
           }
        }


        private static string tname(Type type)
         {
           var result =  type.Namespace + "." + type.Name;

           if (type.IsGenericType)
           {
             result = result.Remove(result.LastIndexOf('`'));

             var gal = string.Empty;
             foreach(var at in type.GetGenericArguments())
             {
               gal += tname(at) + ",";
             }
             gal = gal.Remove(gal.Length-1);
             result = result + "<"+gal+">";
           }
           return result;
         }



       private bool filter(Assembly asm)
       {
          var atrs = asm.GetCustomAttributes(typeof(InventoryAttribute), false).Cast<InventoryAttribute>();

          return (!m_OnlyAttributed  || atrs.Count()>0);
       }

       private bool filter(Type type)
       {
          var atrs = type.GetCustomAttributes(typeof(InventoryAttribute), true).Cast<InventoryAttribute>();
          return filter(atrs);
       }

       private bool filter(IEnumerable<InventoryAttribute> atrs)
       {
         return
           (!m_OnlyAttributed     || atrs.Count()>0) &&
           (!m_Tiers.HasValue     || atrs.Any(a=>a.Tiers == m_Tiers.Value)) &&
           (!m_Concerns.HasValue  || atrs.Any(a=>a.Concerns == m_Concerns.Value)) &&
           (m_Technology==null    || atrs.Any(a=> string.Equals(a.Technology, m_Technology, StringComparison.InvariantCultureIgnoreCase))) &&
           (m_Schema==null        || atrs.Any(a=> string.Equals(a.Schema, m_Schema, StringComparison.InvariantCultureIgnoreCase))) &&
           (m_Tool==null          || atrs.Any(a=> string.Equals(a.Tool, m_Tool, StringComparison.InvariantCultureIgnoreCase))) &&
           (!m_StartDate.HasValue || atrs.Any(a=>a.StartDate.HasValue && a.StartDate.Value >= m_StartDate.Value)) &&
           (!m_EndDate.HasValue   || atrs.Any(a=>a.EndDate.HasValue && a.EndDate.Value <= m_EndDate.Value));
       }

    }


}
