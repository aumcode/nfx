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
 * Revision: NFX 1.0  2011.01.31
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Parsing;
using NFX.IO.FileSystem;
using NFX.Serialization.JSON;
using NFX.DataAccess.Distributed;

namespace NFX.Environment
{

  /// <summary>
  /// Provides configuration node abstraction for section and attribute nodes. This class is thread-safe
  /// </summary>
  [Serializable]
  public abstract class ConfigNode : IConfigNode
  {
    #region .ctor

        private ConfigNode()
        {
        }

        protected ConfigNode(Configuration conf, ConfigSectionNode parent)
        {
          m_Configuration = conf;
          m_Parent = parent;
        }
        protected ConfigNode(Configuration conf, ConfigSectionNode parent, string name, string value)
        {
          m_Configuration = conf;
          m_Parent = parent;
          m_Name = conf.CheckAndAdjustNodeName(name);
          m_Value = value;
        }

        /// <summary>
        /// Creates new node by cloning other node from this or another configuration
        /// </summary>
        protected ConfigNode(Configuration conf, ConfigSectionNode parent, IConfigNode clone)
        {
          if (!clone.Exists)
            throw new ConfigException(StringConsts.CONFIGURATION_CLONE_EMPTY_NODE_ERROR);

          m_Configuration = conf;
          m_Parent = parent;
          m_Name = conf.CheckAndAdjustNodeName(clone.Name);
          m_Value = clone.VerbatimValue;
          m_Modified = false; //modified does not get cloned
        }


    #endregion



    #region Private Fields
      internal bool __Empty;

      private ConfigSectionNode m_Parent;
      protected Configuration m_Configuration;
      private string m_Name;
      private string m_Value;
      protected internal bool m_Modified;
    #endregion

    #region Properties


            /// <summary>
            /// References configuration this node is under
            /// </summary>
            public Configuration Configuration
            {
               get { return m_Configuration;}
            }


            /// <summary>
            /// Determines whether this node really exists in configuration or is just a sentinel empty node
            /// </summary>
            public bool Exists
            {
              get { return !__Empty; }
            }



            /// <summary>
            /// Retrieves node name
            /// </summary>
            public string Name
            {
              get { return m_Name ?? string.Empty; }
              set
              {
                checkCanModify();
                var val = m_Configuration.CheckAndAdjustNodeName(value);
                if (m_Name != value)
                {
                  m_Name = value ?? string.Empty;
                  m_Modified = true;
                }
              }
            }


            /// <summary>
            /// Returns verbatim (without variable evaluation) node value or null
            /// </summary>
            public string VerbatimValue
            {
              get { return m_Value; }
            }

            /// <summary>
            /// Returns null or value of this node with all variables evaluated
            /// </summary>
            public string EvaluatedValue
            {
              get
              {
                if (string.IsNullOrEmpty(m_Value)) return null;

                var section = this as ConfigSectionNode;
                if (section != null)
                  return section.EvaluateValueVariables(m_Value);
                else
                  return m_Parent.EvaluateValueVariables(m_Value);
              }
            }


            /// <summary>
            /// Retrieves node value or null. The value getter performs evaluation of variables, while setter sets the value verbatim
            /// </summary>
            public string Value
            {
              get { return EvaluatedValue; }
              set
              {
                checkCanModify();
                if (m_Value != value)
                {
                  m_Value = value;
                  m_Modified = true;
                }
              }
            }




            /// <summary>
            /// References parent node or Empty if this node has no parent
            /// </summary>
            public ConfigSectionNode Parent
            {
              get { return m_Parent ?? m_Configuration.m_EmptySectionNode; }
            }

            /// <summary>
            /// References parent node
            /// </summary>
            IConfigSectionNode IConfigNode.Parent
            {
              get { return this.Parent; }
            }


            /// <summary>
            /// Indicates whether a node was modified
            /// </summary>
            public virtual bool Modified
            {
              get { return m_Modified; }
            }


            string IConfigNode.RootPath
            {
              get { return this.RootPath; }
            }

            /// <summary>
            /// Returns path from root to this node
            /// </summary>
            public string RootPath
            {
              get
              {
                 if (!Parent.Exists) return "/";

                 var children = Parent.Children.Where(cn=> cn.IsSameName(Name)).ToList();

                 var idx = -1;
                 if (children.Count>1)
                 {
                    for(var i=0; i< children.Count; i++)
                     if ( object.ReferenceEquals(children[i], this))
                     {
                       idx = i;
                       break;
                     }
                 }

                 string path = object.ReferenceEquals(Parent, m_Configuration.Root) ? string.Empty : Parent.RootPath;



                 path += "/{0}{1}".Args(
                                        (this is ConfigAttrNode) ? "$" : string.Empty,
                                        (idx>=0) ? "[{0}]".Args(idx) : Name
                                       );

                 return path;
              }
            }

    #endregion

    #region Public

    /// <summary>
    /// Resets modification flag
    /// </summary>
    public virtual void ResetModified()
    {
      m_Modified = false;
    }

    public abstract void Delete();


    #region Value Accessors

            public string ValueAsString(string dflt = null, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;
              return val.AsStringWhenNullOrEmpty(dflt);
            }

            public byte[] ValueAsByteArray(byte[] dflt = null, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;
              return val.AsByteArray(dflt);
            }


            public short ValueAsShort(short dflt = 0, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;
              return val.AsShort(dflt);
            }


            public short? ValueAsNullableShort(short? dflt = 0, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;
              return val.AsNullableShort(dflt);
            }


            public ushort ValueAsUShort(ushort dflt = 0, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;
              return val.AsUShort(dflt);
            }


            public ushort? ValueAsNullableUShort(ushort? dflt = 0, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;
              return val.AsNullableUShort(dflt);
            }





            public byte ValueAsByte(byte dflt = 0, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;
              return val.AsByte(dflt);
            }


            public byte? ValueAsNullableByte(byte? dflt = 0, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;
              return val.AsNullableByte(dflt);
            }



            public sbyte ValueAsSByte(sbyte dflt = 0, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;
              return val.AsSByte(dflt);
            }


            public sbyte? ValueAsNullableSByte(sbyte? dflt = 0, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;
              return val.AsNullableSByte(dflt);
            }


            public int ValueAsInt(int dflt = 0, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsInt(dflt);
            }


            public int? ValueAsNullableInt(int? dflt = 0, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsNullableInt(dflt);
            }



            public uint ValueAsUInt(uint dflt = 0, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsUInt(dflt);
            }


            public uint? ValueAsNullableUInt(uint? dflt = 0, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsNullableUInt(dflt);
            }



            public long ValueAsLong(long dflt = 0, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsLong(dflt);
            }


            public long? ValueAsNullableLong(long? dflt = 0, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsNullableLong(dflt);
            }



            public ulong ValueAsULong(ulong dflt = 0, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsULong(dflt);
            }


            public ulong? ValueAsNullableULong(ulong? dflt = 0, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsNullableULong(dflt);
            }



            public double ValueAsDouble(double dflt = 0d, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsDouble(dflt);
            }


            public double? ValueAsNullableDouble(double? dflt = 0d, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsNullableDouble(dflt);
            }


            public float ValueAsFloat(float dflt = 0f, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsFloat(dflt);
            }


            public float? ValueAsNullableFloat(float? dflt = 0f, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsNullableFloat(dflt);
            }


            public decimal ValueAsDecimal(decimal dflt = 0m, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsDecimal(dflt);
            }


            public decimal? ValueAsNullableDecimal(decimal? dflt = 0m, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsNullableDecimal(dflt);
            }



            public bool ValueAsBool(bool dflt = false, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsBool(dflt);
            }


            public bool? ValueAsNullableBool(bool? dflt = false, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsNullableBool(dflt);
            }



            public Guid ValueAsGUID(Guid dflt, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsGUID(dflt);
            }


            public Guid? ValueAsNullableGUID(Guid? dflt = null, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsNullableGUID(dflt);
            }

            public GDID ValueAsGDID(GDID dflt, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsGDID(dflt);
            }


            public GDID? ValueAsNullableGDID(GDID? dflt = null, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsNullableGDID(dflt);
            }



            public DateTime ValueAsDateTime(DateTime dflt, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsDateTime(dflt);
            }


            public DateTime? ValueAsNullableDateTime(DateTime? dflt = null, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsNullableDateTime(dflt);
            }




            public TimeSpan ValueAsTimeSpan(TimeSpan dflt, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsTimeSpan(dflt);
            }


            public TimeSpan? ValueAsNullableTimeSpan(TimeSpan? dflt = null, bool verbatim = false)
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsNullableTimeSpan(dflt);
            }





            public TEnum ValueAsEnum<TEnum>(TEnum dflt = default(TEnum), bool verbatim = false) where TEnum : struct
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsEnum<TEnum>(dflt);
            }

            public TEnum? ValueAsNullableEnum<TEnum>(TEnum? dflt = null, bool verbatim = false) where TEnum : struct
            {
              var val = verbatim ? VerbatimValue : Value;

              return val.AsNullableEnum<TEnum>(dflt);
            }



              /// <summary>
              /// Tries to get value as specified type or throws if it can not be converted
              /// </summary>
              public object ValueAsType(Type tp, bool verbatim = false, bool strict = true)
              {
                try
                {
                  var val = verbatim ? VerbatimValue : Value;

                  return val.AsType(tp, strict);
                }
                catch(Exception error)
                {
                  throw new ConfigException(string.Format(StringConsts.CONFIGURATION_VALUE_COULD_NOT_BE_GOTTEN_AS_TYPE_ERROR, this.Name, tp.FullName), error);
                }
              }



    #endregion


    /// <summary>
    /// Returns true when another node has the same name as this one per case-insensitive culture-neutral comparison
    /// </summary>
    public bool IsSameName(IConfigNode other)
    {
      if (other==null) return false;
      return IsSameName(other.Name);
    }

    /// <summary>
    /// Returns true when another name is the same as this node's name per case-insensitive culture-neutral comparison
    /// </summary>
    public bool IsSameName(string other)
    {
      if (other==null) return false;
      return this.Name.EqualsIgnoreCase(other);
    }


    public override string ToString()
    {
      return string.Format("{0}=\"{1}\" {2}", m_Name, m_Value != null ? m_Value : string.Empty, m_Modified ? "(modified)" : string.Empty);
    }


    #endregion


    #region Private utils

        internal void checkCanModify()
        {
          if (m_Configuration.IsReadOnly)
            throw new ConfigException(StringConsts.CONFIGURATION_READONLY_ERROR);
          if (!Exists)
            throw new ConfigException(StringConsts.CONFIGURATION_EMPTY_NODE_MODIFY_ERROR);

        }


    #endregion


  }



  /// <summary>
  /// Represents configuration section node. This class is thread safe
  /// </summary>
  [Serializable]
  public sealed class ConfigSectionNode : ConfigNode, IConfigSectionNode
  {
    #region .ctor

        internal ConfigSectionNode(Configuration conf, ConfigSectionNode parent)
          : base(conf, parent)
        {

        }

        internal ConfigSectionNode(Configuration conf, ConfigSectionNode parent, string name, string value)
          : base(conf, parent, name, value)
        {

        }

        /// <summary>
        /// Performs deep clone copy from another node which can be in this or different configuration
        /// </summary>
        internal ConfigSectionNode(Configuration conf, ConfigSectionNode parent, IConfigSectionNode clone)
          : base(conf, parent, clone)
        {
          //there is no infinite recursion problem, because .Children and .Attributes make thread-safe copy at the moment of initial access
          var clChildren = clone.Children;//thread-safe copy
          var clAttributes = clone.Attributes;//thread-safe copy

          foreach(var clchild in clChildren)
          {
            var child = new ConfigSectionNode(conf, this, clchild);
            lock(m_Children)
              m_Children.Add(child);
          }

          foreach(var clattr in clAttributes)
          {
            var attr = new ConfigAttrNode(conf, this, clattr);
            lock(m_Attributes)
              m_Attributes.Add(attr);
          }

        }


    #endregion



    #region Private Fields
     internal ConfigSectionNodeList m_Children = new ConfigSectionNodeList();
     internal ConfigAttrNodeList m_Attributes = new ConfigAttrNodeList();


                         /// <summary>
                         /// Internal field used for attaching temporary script state. do not use
                         /// </summary>
                         internal bool m_Script_Statement;

                         /// <summary>
                         /// Internal field used for attaching temporary script state. do not use
                         /// </summary>
                         internal bool m_Script_Bool_Condition_Result;
    #endregion


    #region Properties

        /// <summary>
        /// Indicates whether this node has any child section nodes
        /// </summary>
        public bool HasChildren
        {
          get { lock (m_Children) return m_Children.Count > 0; }
        }

        /// <summary>
        /// Returns number of child section nodes
        /// </summary>
        public int ChildCount
        {
          get { lock (m_Children) return m_Children.Count; }
        }

        /// <summary>
        /// Indicates whether this node has any associated attributes
        /// </summary>
        public bool HasAttributes
        {
          get { lock (m_Attributes) return m_Attributes.Count > 0; }
        }

        /// <summary>
        /// Returns number of child attribute nodes
        /// </summary>
        public int AttrCount
        {
          get { lock (m_Attributes) return m_Attributes.Count; }
        }

        /// <summary>
        /// Indicates whether this or any child nodes or attributes were modified
        /// </summary>
        public override bool Modified
        {
          get
          {
            if (base.Modified) return true;

            lock (m_Children)
              foreach (ConfigSectionNode child in m_Children)
                if (child.Modified) return true;

            lock (m_Attributes)
              foreach (ConfigAttrNode attr in m_Attributes)
                if (attr.Modified) return true;

            return false;
          }
        }



        IEnumerable<IConfigSectionNode> IConfigSectionNode.Children
        {
          get { return this.Children; }
        }

        /// <summary>
        /// Enumerates all child nodes
        /// </summary>
        public IEnumerable<ConfigSectionNode> Children
        {
          get { lock (m_Children) return m_Children.ToList(); }
        }


        IEnumerable<IConfigAttrNode> IConfigSectionNode.Attributes
        {
          get { return this.Attributes; }
        }

        /// <summary>
        /// Enumerates all attribute nodes
        /// </summary>
        public IEnumerable<ConfigAttrNode> Attributes
        {
          get { lock (m_Attributes) return m_Attributes.ToList(); }
        }



        IConfigSectionNode IConfigSectionNode.this[params string[] names]
        {
          get { return this[names]; }
        }


        /// <summary>
        /// Retrieves section node by names, from left to right until existing node is found.
        /// If no existing node could be found then empty node instance is returned
        /// </summary>
        public ConfigSectionNode this[params string[] names]
        {
          get
          {
            if (names==null || names.Length<1)
             throw new ConfigException(StringConsts.CONFIGURATION_SECTION_INDEXER_EMPTY_ERROR);

            ConfigSectionNode result = null;

            lock (m_Children)
             foreach(var name in names)
             {
              result = m_Children.FirstOrDefault((node => node.IsSameName(name)));
              if (result!=null) break;
             }

            if (result != null)
              return result;
            else
              return m_Configuration.m_EmptySectionNode;
          }
        }

        IConfigSectionNode IConfigSectionNode.this[int idx]
        {
          get { return this[idx]; }
        }


        /// <summary>
        /// Retrieves section node by index or empty node instance if section node with such index could not be found
        /// </summary>
        public ConfigSectionNode this[int idx]
        {
          get
          {
            lock(m_Children)
              if (idx >= 0 && idx < m_Children.Count)
                return m_Children[idx];
              else
                return m_Configuration.m_EmptySectionNode;
          }
        }




    #endregion

    #region Public

        /// <summary>
        /// Deletes this section from its parent
        /// </summary>
        public override void Delete()
        {
          checkCanModify();
          if (!Parent.Exists)
           m_Configuration.Destroy();
          else
          {
            lock(Parent.m_Children)
            {
              Parent.m_Children.Remove(this);
            }
            Parent.m_Modified = true;
          }
        }

        /// <summary>
        /// Deletes all child section nodes from this node
        /// </summary>
        public void DeleteAllChildren()
        {
          checkCanModify();

            lock(m_Children)
            {
              m_Children.Clear();
              m_Modified = true;
            }
        }

        /// <summary>
        /// Deletes all attribute nodes from this node
        /// </summary>
        public void DeleteAllAttributes()
        {
          checkCanModify();

            lock(m_Attributes)
            {
              m_Attributes.Clear();
              m_Modified = true;
            }
        }


        public ConfigSectionNode AddChildNode(string name)
        {
          return AddChildNode(name, null);
        }

        public ConfigSectionNode AddChildNode(string name, object value)
        {
          return AddChildNode(name, value == null ? null : value.ToString());
        }



        /// <summary>
        /// Adds a new child section node to this node
        /// </summary>
        public ConfigSectionNode AddChildNode(string name, string value)
        {
          checkCanModify();

          lock (m_Children)
          {
            ConfigSectionNode node = new ConfigSectionNode(m_Configuration, this, name, value);
            m_Children.Add(node);
            m_Modified = true;
            return node;
          }
        }

        /// <summary>
        /// Adds a new child node into this one deeply cloning nodes data from some other node which may belong to a different conf instance
        /// </summary>
        public ConfigSectionNode AddChildNode(IConfigSectionNode clone)
        {
          checkCanModify();

          lock (m_Children)
          {
            var node = new ConfigSectionNode(m_Configuration, this, clone);
            m_Children.Add(node);
            m_Modified = true;
            return node;
          }
        }


        /// <summary>
        /// Adds a new section node to this configuration which is an ordered merge result of two other nodes - base and override.
        /// </summary>
        /// <param name="baseNode">A base node that data is defaulted from</param>
        /// <param name="overrideNode">A node that contains overrides/additions of/to data from base node</param>
        /// <param name="rules">Rules to use for this merge. Default rules will be used if null is passed</param>
        public ConfigSectionNode AddChildNodeFromMerge(IConfigSectionNode baseNode, IConfigSectionNode overrideNode, NodeOverrideRules rules = null)
        {
          checkCanModify();

          var newNode = new ConfigSectionNode(m_Configuration, this, baseNode);

          newNode.OverrideBy(overrideNode, rules);

          lock (m_Children)
          {
            m_Children.Add(newNode);
            m_Modified = true;
          }

          return newNode;
        }

        /// <summary>
        /// Merges another node data by overriding this node's value/attributes/sub nodes according to rules.
        /// </summary>
        /// <returns>True when merge match was made</returns>
        public bool OverrideBy(IConfigSectionNode other, NodeOverrideRules rules = null)
        {
          if (other==null) return false;

          checkCanModify();

          if (rules==null) rules = NodeOverrideRules.Default;

          if (!IsSameName(other)) return false;

          var ospec = rules.StringToOverrideSpec(this.AttrByName(rules.OverrideAttrName).Value);

          switch(ospec)
          {
            case OverrideSpec.Stop: return false;
            case OverrideSpec.All:
                                         MergeAttributes(other, rules);
                                         MergeSections(other, rules);
                                         Value = other.VerbatimValue;
                                         break;

            case OverrideSpec.Attributes:
                                         MergeAttributes(other, rules);
                                         break;

            case OverrideSpec.Sections:
                                         MergeSections(other, rules);
                                         break;

            case OverrideSpec.Replace:
                                         ReplaceBy(other);
                                         break;

            default: //fail
              throw new ConfigException(StringConsts.CONFIGURATION_OVERRIDE_PROHOBITED_ERROR + this.RootPath);
          }



          return true;
        }


        /// <summary>
        /// Merges attributes from another node into this one. Another node may belong to a different configuration instance
        /// </summary>
        public void MergeAttributes(IConfigSectionNode other, NodeOverrideRules rules = null)
        {
          if (other==null || !other.Exists) return;

          checkCanModify();

          if (rules==null) rules = NodeOverrideRules.Default;

          var oattrs = other.Attributes;//thread safe
          foreach(var oatr in oattrs)
          {
            var existing = this.AttrByName(oatr.Name);

            if (existing.Exists)
             existing.Value = oatr.VerbatimValue;
            else
             AddAttributeNode(oatr.Name, oatr.VerbatimValue);
          }
        }

        /// <summary>
        /// Merges child sections from another node into this one. Another node may belong to a different configuration instance.
        /// This method ignores override flags and merges nodes regardless
        /// </summary>
        public void MergeSections(IConfigSectionNode other, NodeOverrideRules rules = null)
        {
          if (other==null || !other.Exists) return;

          checkCanModify();

          if (rules==null) rules = NodeOverrideRules.Default;

          var children = this.Children;//thread safe
          var osects = other.Children;//thread safe
          foreach(var osect in osects)
          {
            if (osect.IsSameName(rules.SectionClearName))
            {
              this.DeleteAllChildren();
              continue;
            }


            //var matches = children.Where(child => child.IsSameName(osect)).ToList();
            //if (matches.Count>1)
            //{
            //  matches = matches.Where(child => child.AttrByName( rules.SectionMatchAttrName).ValueAsString()
            //                                                      .EqualsIgnoreCase(osect.AttrByName(rules.SectionMatchAttrName).ValueAsString())).ToList();
            //}

            //if (matches.Count>0)
            // matches[0].OverrideBy(osect, rules);
            //else
            // AddChildNode(osect.Name, osect.VerbatimValue).OverrideBy(osect, rules);

            //20160329 DKh
            var match = children.Where(child =>
                                 child.IsSameName(osect) &&
                                 child.AttrByName( rules.SectionMatchAttrName ).Value
                                 .EqualsIgnoreCase(osect.AttrByName(rules.SectionMatchAttrName).Value)).FirstOrDefault();

            var append = match == null;

            if (!append)
             append = rules.AppendSectionsWithoutMatchAttr &&
                      osect.AttrByName(rules.SectionMatchAttrName).Value.IsNullOrWhiteSpace();

            if (append)
              AddChildNode(osect.Name, osect.VerbatimValue).OverrideBy(osect, rules);
            else
              match.OverrideBy(osect, rules);



          }
        }


        /// <summary>
        /// Completely replaces this node's attributes, value and children with data from another node
        /// </summary>
        public void ReplaceBy(IConfigSectionNode other)
        {
          if (other==null) return;  //does not check for empty, as replace by empty basically deletes all children and attrs

          checkCanModify();

          Value = other.VerbatimValue;

          var ochildren = other.Children;//thread safe
          var oattrs = other.Attributes;//thread safe

          lock(m_Children)
          {
            m_Children.Clear();
            foreach(var ochild in ochildren)
                m_Children.Add( new ConfigSectionNode(m_Configuration, this, ochild) );
          }

          lock(m_Attributes)
          {
            m_Attributes.Clear();
            foreach(var oattr in oattrs)
                m_Attributes.Add( new ConfigAttrNode(m_Configuration, this, oattr) );
          }

          m_Modified = true;
        }


        public ConfigAttrNode AddAttributeNode(string name)
        {
          return AddAttributeNode(name, null);
        }


        public ConfigAttrNode AddAttributeNode(string name, object value)
        {
          return AddAttributeNode(name, value == null ? null : value.ToString());
        }

        public ConfigAttrNode AddAttributeNode(string name, string value)
        {
          checkCanModify();

          lock (m_Attributes)
          {
            ConfigAttrNode node = new ConfigAttrNode(m_Configuration, this, name, value);
            m_Attributes.Add(node);
            m_Modified = true;
            return node;
          }
        }


        IConfigAttrNode IConfigSectionNode.AttrByName(string name, bool autoCreate)
        {
          return this.AttrByName(name, autoCreate);
        }


        public ConfigAttrNode AttrByName(string name, bool autoCreate = false)
        {
          ConfigAttrNode result;

          lock (m_Attributes)
          {
            result = m_Attributes.FirstOrDefault((node => node.IsSameName(name)));

            if (result != null)
              return result;
            else
              return !autoCreate ? m_Configuration.m_EmptyAttrNode : AddAttributeNode(name);
          }
        }


        IConfigAttrNode IConfigSectionNode.AttrByIndex(int idx)
        {
          return this.AttrByIndex(idx);
        }


        /// <summary>
        /// Returns attribute node by its index or empty attribute if real attribute with such index does not exist
        /// </summary>
        public ConfigAttrNode AttrByIndex(int idx)
        {
          lock(m_Attributes)
            if (idx >= 0 && idx < m_Attributes.Count)
              return m_Attributes[idx];
            else
              return m_Configuration.m_EmptyAttrNode;
        }



        /// <summary>
        /// Resets modification of this an all child nodes
        /// </summary>
        public override void ResetModified()
        {
          base.ResetModified();

          lock (m_Children)
            foreach (ConfigSectionNode child in m_Children)
              child.ResetModified();

          lock (m_Attributes)
            foreach (ConfigAttrNode attr in m_Attributes)
              attr.ResetModified();
        }



        IConfigNode IConfigSectionNode.Navigate(string path)
        {
          return this.Navigate(path);
        }


        /// <summary>
        /// Navigates the path and return the appropriate node. Example: '!/nfx/logger/destination/$file-name'
        /// </summary>
        /// <param name="path">If path starts from '!' then exception will be thrown if such a node does not exist;
        ///  Use '/' as leading char for root,
        ///  '..' for step up,
        ///  '$' for attribute name,
        ///  [int] for access to subsection or attribute by index,
        ///  section[value] for access using value comparison of named section,
        ///  section[attr=value] for access using value of sections named attr
        /// Multiple paths may be coalesed using '|' or ';'
        /// </param>
        /// <example>
        ///     Navigate("/vars/[3]"); Navigate("/tables/table[resident]"); Navigate("/vars/var1/$[2]");  Navigate("/tables/table[name=patient]");
        /// </example>
        /// <remarks>
        ///   /table[patient]    -   get first section named "table" with value "patient"
        ///   /[3]               -   get 4th child section from the root
        ///   /table/$[2]        -   get 3rd attribute of first section named "table"
        ///   /table[short-name=pat] -  get first section named "table" having attribute named "short-name" equal "pat"
        /// </remarks>
        public ConfigNode Navigate(string path)
        {
          if (string.IsNullOrWhiteSpace(path))
           throw new ArgumentException(StringConsts.ARGUMENT_ERROR + "ConfigSectionNode.Navigate(path)", "path");

          try
          {
            var segments = path.Split(';', '|');
            for(var i=0; i<segments.Length; i++)
            {
                bool required;
                var node = doNavigate(segments[i], out required);
                if (node.Exists) return node;
            }

            return m_Configuration.EmptySection;
          }
          catch(Exception error)
          {
            if (error is ConfigException)
             throw error;
            else
             throw new ConfigException(StringConsts.CONFIGURATION_NAVIGATION_BAD_PATH_ERROR + path ?? StringConsts.NULL_STRING);
          }
        }


        IConfigSectionNode IConfigSectionNode.NavigateSection(string path)
        {
          return this.NavigateSection(path);
        }

        /// <summary>
        /// Navigates the path and return the appropriate section node. Example '!/nfx/logger/destination'
        /// </summary>
        /// <param name="path">If path starts from '!' then exception will be thrown if such a section node does not exist;
        ///  Use '/' as leading char for root,
        ///  '..' for step up. Multiple paths may be coalesced using '|' or ';'
        /// </param>
        public ConfigSectionNode NavigateSection(string path)
        {
          if (string.IsNullOrWhiteSpace(path))
           throw new ArgumentException(StringConsts.ARGUMENT_ERROR + "ConfigSectionNode.Navigate(path)", "path");

          try
          {
            var segments = path.Split(';', '|');
            for(var i=0; i<segments.Length; i++)
            {
                bool required;
                var node = (doNavigate(segments[i], out required) as ConfigSectionNode) ?? m_Configuration.m_EmptySectionNode;

                if (required && !node.Exists)
                 throw new ConfigException(string.Format(StringConsts.CONFIGURATION_NAVIGATION_SECTION_REQUIRED_ERROR, path));

                if (node.Exists) return node;
            }

            return m_Configuration.EmptySection;
          }
          catch(Exception error)
          {
            if (error is ConfigException)
             throw error;
            else
             throw new ConfigException(StringConsts.CONFIGURATION_NAVIGATION_BAD_PATH_ERROR + path ?? StringConsts.NULL_STRING);
          }
        }


        [ThreadStatic]
        private static HashSet<string> ts_VarNames;

        [ThreadStatic]
        private static int ts_Count;

        /// <summary>
        /// Evaluates a value string expanding all variables with var-paths relative to this node.
        /// Evaluates configuration variables such as "$(varname)" or "$(@varname)". Varnames are paths
        /// to other config nodes from the same configuration or variable names when prefixed with "~". If varname starts with "@" then it gets combined
        ///  with input as path string. "~" is used to qualify environment vars that get resolved through Configuration.EnvironmentVarResolver
        ///  Example: `....add key="Schema.$(/A/B/C/$attr)" value="$(@~HOME)bin\Transforms\"...`
        /// </summary>
        public string EvaluateValueVariables(string value)
        {
          if (value == null) return null;

          var VAR_ESCAPE = m_Configuration.Variable_ESCAPE;
          if (value.IndexOf(VAR_ESCAPE)==0)
           return value.Length > VAR_ESCAPE.Length ? value.Substring(VAR_ESCAPE.Length) : string.Empty;


          HashSet<string> vlist;

          if (ts_Count==0)
          {
            vlist = new HashSet<string>();
            ts_VarNames = vlist;
          }
          else
            vlist = ts_VarNames;

          ts_Count++;
          try
          {
                  var VAR_START = m_Configuration.Variable_START;
                  var VAR_END = m_Configuration.Variable_END;
                  var VAR_PATH_MOD = m_Configuration.Variable_PATH_MOD;

                       while(true)
                       {
                         var idxs = value.IndexOf(VAR_START);
                         if (idxs<0) break;
                         var idxe = value.IndexOf(VAR_END, idxs);
                         if (idxe<=idxs) break;

                         var vname = value.Substring(idxs + VAR_START.Length, 1 + idxe - idxs - VAR_START.Length - VAR_END.Length).Trim();

                         if (vlist.Contains(vname))
                           throw new ConfigException(string.Format(StringConsts.CONFIG_RECURSIVE_VARS_ERROR, value));

                         vlist.Add(vname);
                         try
                         {
                             if (vname.StartsWith(VAR_PATH_MOD))
                             {
                               value = replacePaths(value, VAR_START + vname + VAR_END, getValueFromMacroOrEnvVarOrNavigationWithCheck(vname.Replace(VAR_PATH_MOD, string.Empty)));
                             }
                             else
                             {
                               value = value.Replace(VAR_START + vname + VAR_END, getValueFromMacroOrEnvVarOrNavigationWithCheck(vname));
                             }
                         }
                         finally
                         {
                             vlist.Remove(vname);
                         }
                       }

                  return value;
          }
          finally
          {
            ts_Count--;
            if (ts_Count==0)
             ts_VarNames = null;
          }
        }


        /// <summary>
        /// Returns true when another node has the attribute called 'name' and its value is the same as in this one per case-insensitive culture-neutral comparison
        /// </summary>
        public bool IsSameNameAttr(IConfigSectionNode other)
        {
          if (other==null) return false;
          return IsSameNameAttr(other.AttrByName(Configuration.CONFIG_NAME_ATTR).Value);
        }

        /// <summary>
        /// Returns true when another name is the same as this section "name" attribute per case-insensitive culture-neutral comparison
        /// </summary>
        public bool IsSameNameAttr(string other)
        {
          if (other==null) return false;
          return this.AttrByName(Configuration.CONFIG_NAME_ATTR).Value.EqualsIgnoreCase( other);
        }

        /// <summary>
        /// Serializes configuration tree rooted at this node into Laconic format and returns it as a string
        /// </summary>
        public string ToLaconicString(NFX.CodeAnalysis.Laconfig.LaconfigWritingOptions options = null)
        {
          return NFX.CodeAnalysis.Laconfig.LaconfigWriter.Write(this, options);
        }

        /// <summary>
        /// Serializes configuration tree rooted at this node into JSON configuration format and returns it as a string
        /// </summary>
        public string ToJSONString(NFX.Serialization.JSON.JSONWritingOptions options = null)
        {
          return this.ToConfigurationJSONDataMap().ToJSON(options);
        }


        /// <summary>
        /// Replaces all include pragmas - sections with specified names ('_include' by default), with pointed to configuration file content
        /// as obtained via the call to file system specified in every pragma.
        /// If no FS specified then LocalFileStsrem is used. Returns true if include pragmas were found.
        /// Note: this method does not process new include pragmas that may have fetched during this call.
        /// Caution: the file system used in the operation may rely on the App container that may need to be set-up for the call to succeed,
        /// therefore calling this method before app has activated may fail, in such cases a temp app container may be set to get the config file
        ///  with processed includes, then the result may be passed to the primary app container ctor.
        /// This call is not logically thread-safe, it must be called from the main thread in the app
        /// </summary>
        /// <param name="recurse">True to process inner nodes</param>
        /// <param name="includePragma">Pragma section name, '_include' by default</param>
        /// <returns>True if pragmas were found</returns>
        /// <example>
        ///  nfx
        ///  {
        ///    sectionA{ a=2 b=3}
        ///    _include
        ///    {
        ///      name=secret // '_include' will be replaced by 'secret' with sub-nodes from referenced file
        ///      file="/etc/cluster/mysecret.laconf" //would come from local FS
        ///      required=false //if file is not found then nothing will be included instead of '_include' which will be just removed
        ///    }
        ///    _include
        ///    {
        ///      // '_include' will be replaced by whatever root section content in the referenced file
        ///      fs {type="NFX.Web.IO.FileSystem.SVNFileSystem, NFX.Web"}
        ///      session { server-url="https://myhost.com/mySvnRepo/trunk/configs" user-name="user1" user-password="******"}
        ///    }
        ///  }
        /// </example>
        public bool ProcessIncludePragmas(bool recurse, string includePragma = null)
        {
          if (includePragma.IsNullOrWhiteSpace())
            includePragma = Configuration.DEFAULT_CONFIG_INCLUDE_PRAGMA;

          var result = false;
          checkCanModify();

          foreach(var child in Children)//Children does snapshot
          {
            if (child.IsSameName(includePragma))
            {
              var included = getIncludedNode(child);
              if (included!=null)
              {
                child.include( included );
                result = true;
              }
              else
                child.Delete();

              continue;
            }

            if (recurse)
             result |= child.ProcessIncludePragmas(recurse, includePragma);
          }

          return result;
        }

        /// <summary>
        /// Returns attribute values as string map
        /// </summary>
        public Collections.StringMap AttrsToStringMap(bool verbatim = false)
        {
          var result = new Collections.StringMap();

          foreach(var atr in this.Attributes)
           result[atr.Name] = verbatim ? atr.VerbatimValue : atr.Value;

          return result;
        }


        /// <summary>
        /// Converts this ConfigSectionNode to JSONDataMap. Contrast with ToConfigurationJSONDataMap
        /// Be carefull: that this operation can "lose" data from ConfigSectionNode.
        /// In other words some ConfigSectionNode information can not be reflected in corresponding JSONDataMap, for example
        ///  this method overwrites duplicate key names and does not support section values
        /// </summary>
        public JSONDataMap ToJSONDataMap()
        {
          var map = new JSONDataMap();

          if (this.Exists)
            buildSectionMap(this, map);

          return map;
        }

                  private static void buildSectionMap(ConfigSectionNode node, JSONDataMap map)
                  {
                    foreach (var attr in node.Attributes)
                    {
                      map[attr.Name] = attr.Value;
                    }

                    foreach (var childNode in node.Children)
                    {
                      var childMap = new JSONDataMap();
                      map[childNode.Name] = childMap;
                      buildSectionMap(childNode, childMap);
                    }
                  }

         /// <summary>
         /// Returns this config node as JSON data map suitable for making JSONConfiguration.
         /// Contrast with ToJSONDataMap
         /// </summary>
         public JSONDataMap ToConfigurationJSONDataMap()
         {
           var root = new JSONDataMap(false);

           if (this.Exists)
             root[this.Name] = buildSectionConfigJSONDataMap(this);

           return root;
         }

               private static JSONDataMap buildSectionConfigJSONDataMap(ConfigSectionNode sect)
               {
                 var result = new JSONDataMap(false);

                 if (sect.VerbatimValue.IsNotNullOrWhiteSpace())
                   result[JSONConfiguration.SECTION_VALUE_ATTR] = sect.VerbatimValue;

                 foreach(var atr in sect.Attributes)
                   result[atr.Name] = atr.VerbatimValue;

                 foreach(var cs in sect.Children)
                 {
                   var subSection = buildSectionConfigJSONDataMap(cs);
                   var existing = result[cs.Name];
                   if (existing==null)
                     result[cs.Name] = subSection;
                   else
                   {
                     if (existing is JSONDataMap)
                     {
                       var lst =  new List<JSONDataMap>();
                       lst.Add((JSONDataMap)existing);
                       lst.Add(subSection);
                       result[cs.Name] = lst;
                     }
                     else
                      ((List<JSONDataMap>)existing).Add(subSection);
                   }
                 }
                 return result;
               }

    #endregion

    #region .pvt .impl


        private ConfigSectionNode getIncludedNode(ConfigSectionNode pragma)
        {
          try
          {
              var fileName = pragma.AttrByName(Configuration.CONFIG_INCLUDE_PRAGMA_FILE_ATTR).ValueAsString();
              if (fileName.IsNullOrWhiteSpace())
                throw new ConfigException("missing '{0}'".Args(Configuration.CONFIG_INCLUDE_PRAGMA_FILE_ATTR));

              var required = pragma.AttrByName(Configuration.CONFIG_INCLUDE_PRAGMA_REQUIRED_ATTR).ValueAsBool(true);

              var ndFs = pragma[Configuration.CONFIG_INCLUDE_PRAGMA_FS_SECTION];

              using(var fs = FactoryUtils.MakeAndConfigure<FileSystem>(ndFs, typeof(NFX.IO.FileSystem.Local.LocalFileSystem)))
              {
                FileSystemSessionConnectParams cParams;

                var ndSession = pragma[Configuration.CONFIG_INCLUDE_PRAGMA_SESSION_SECTION];

                if (ndSession.Exists)
                  cParams = FileSystemSessionConnectParams.Make<FileSystemSessionConnectParams>(ndSession);
                else
                  cParams = new FileSystemSessionConnectParams(){ User = NFX.Security.User.Fake};

                string source = "";
                using(var fsSession = fs.StartSession(cParams))
                {
                   var file = fsSession[fileName] as NFX.IO.FileSystem.FileSystemFile;

                   if (file==null)
                   {
                     if (required) throw new ConfigException("Referenced file '{0}' does not exist".Args(fileName));
                     return null;
                   }


                   source = file.ReadAllText();

                   string fmt = null;
                   var j = fileName.LastIndexOf('.');
                   if (j>0&&j<fileName.Length-1) fmt = fileName.Substring(j+1);

                   if (fmt.IsNullOrWhiteSpace()) fmt = Configuration.CONFIG_LACONIC_FORMAT;

                   var root = Configuration.ProviderLoadFromString(source, fmt).Root;

                   //name section wrap
                   var asname = pragma.AttrByName(Configuration.CONFIG_NAME_ATTR).ValueAsString();
                   if (asname.IsNotNullOrWhiteSpace())
                   {
                     var wrap = new MemoryConfiguration();
                     wrap.Create();
                     wrap.Root.AddChildNode( root ).Name = asname;
                     root = wrap.Root;
                   }

                   return root;
                }
              }
          }
          catch(Exception inner)
          {
            throw new ConfigException(StringConsts.CONFIGURATION_INCLUDE_PRAGMA_ERROR.Args(pragma.RootPath, inner.ToMessageWithType()), inner);
          }
        }


        internal void include(ConfigSectionNode other)
        {
          checkCanModify();

          var oattrs = other.Attributes;
          var ochildren = other.Children;

          var parentChildren = Parent.m_Children;

          lock(parentChildren)
          {
              var idx = parentChildren.IndexOf(this);
              parentChildren.Remove(this);

              foreach(var oatr in oattrs)
              {
                var existing = Parent.AttrByName(oatr.Name);

                if (existing.Exists)
                 existing.Value = oatr.VerbatimValue;
                else
                 Parent.AddAttributeNode(oatr.Name, oatr.VerbatimValue);
              }

              foreach(var ochild in ochildren)
              {
                var node = new ConfigSectionNode(m_Configuration, Parent, ochild);
                if (idx<parentChildren.Count)
                    parentChildren.Insert(idx, node);
                else
                    parentChildren.Add(node);
                idx++;
              }
          }

          Parent.m_Modified = true;
        }


       private ConfigNode doNavigate(string path, out bool required) //todo Use indexes on path and do not mutate it so no string copies are made
       {
          path = path.Trim();

          ConfigNode result = this;
          required = false;

          if (path.StartsWith("!"))
          {
            required = true;
            if (path.Length>1)
             path = path.Substring(1);
            else
             path = string.Empty;
          }


          if (path.StartsWith("/") || path.StartsWith("\\"))
          {
            if (path.Length>1)
             path = path.Substring(1);
            else
             path = string.Empty;

            result = this.Configuration.Root;
          }


          var segs = path.Split('/', '\\');

          foreach(var s in segs)
          {
               //20160319 DKh
               if (!result.Exists) break;

               var seg = s.Trim();

               if (seg=="..")
               {
                    result = result.Parent;
                    continue;
               }

               if (!(result is ConfigSectionNode))
                 throw new ConfigException(StringConsts.CONFIGURATION_PATH_SEGMENT_NOT_SECTION_ERROR.Args(seg, path));

               var section = (ConfigSectionNode)result;

               var isAttr = false;
               if (seg.StartsWith("$")) //attribute
               {
                    seg = seg.Substring(1).Trim();
                    isAttr = true;
               }

               if (seg.StartsWith("[")) //indexer like:   ../[0]  or  ../$[1] (attribute of section by index)
               {
                    var icl = seg.LastIndexOf(']');

                    if (icl<2)
                        throw new ConfigException(StringConsts.CONFIGURATION_PATH_INDEXER_SYNTAX_ERROR.Args(path));

                    var istr = seg.Substring(1, icl-1);

                    int idx;
                    if (!int.TryParse(istr, out idx))
                        throw new ConfigException(StringConsts.CONFIGURATION_PATH_INDEXER_ERROR.Args(path, istr));
                    if (isAttr)
                      result = section.AttrByIndex(idx);
                    else
                      result = section[idx];
               }
               else if (seg.EndsWith("]")) //value indexer like:   ../table[name=patient] (atr name=patient)   or  ../table[patient] (table node value=patient)
               {
                    if (isAttr)
                        throw new ConfigException(StringConsts.CONFIGURATION_PATH_VALUE_INDEXER_CAN_NOT_USE_WITH_ATTRS_ERROR.Args(path));

                    var iop = seg.IndexOf('[');

                    if (iop<1 || iop>=seg.Length-2)
                        throw new ConfigException(StringConsts.CONFIGURATION_PATH_VALUE_INDEXER_SYNTAX_ERROR.Args(path));


                    var name = seg.Substring(0, iop);
                    var vstr = seg.Substring(iop+1, seg.Length - iop - 2);

                    var ieq = vstr.IndexOf('=');

                    if (ieq<0 || ieq==0 || ieq==vstr.Length-1)
                      result = section.Children.FirstOrDefault(c =>
                                        c.IsSameName(name) &&
                                        vstr.EqualsIgnoreCase( c.Value) )
                                     ?? m_Configuration.m_EmptySectionNode;
                    else
                    {
                        var atr = vstr.Substring(0, ieq);
                        var val = vstr.Substring(ieq+1);

                        result = section.Children.FirstOrDefault(c =>
                                        c.IsSameName(name) &&
                                        val.EqualsIgnoreCase(c.AttrByName(atr).Value) )
                                     ?? m_Configuration.m_EmptySectionNode;

                    }
               }
               else
               {
                   if (isAttr)
                      result = section.AttrByName(seg);
                   else
                      result = section[seg];
               }
          }//foreach segment

          if (required && !result.Exists)
           throw new ConfigException(StringConsts.CONFIGURATION_NAVIGATION_REQUIRED_ERROR.Args(path));

          return result;
        }




        private string getValueFromMacroOrEnvVarOrNavigationWithCheck(string name)
		    {
               try
               {
                    return getValueFromMacroOrEnvVarOrNavigation(name);
               }
               catch(Exception error)
               {
                    throw new ConfigException(string.Format(StringConsts.CONFIG_VARS_EVAL_ERROR, name ?? StringConsts.NULL_STRING, error.ToMessageWithType()), error);
               }
        }

            private string getValueFromMacroOrEnvVarOrNavigation(string name)
            {
                const char CO = (char)0xab;
                const char CC = (char)0xbb;

                if (string.IsNullOrWhiteSpace(name)) return string.Empty;

                var MACRO_START = m_Configuration.Variable_MACRO_START;

                var midx = name.IndexOf(MACRO_START);
                if ((midx<0)||
                    (midx+MACRO_START.Length >= name.Length)) return getValueFromEnvVarOrNavigation(name);

                var macroSrc = CO + name.Substring(midx + MACRO_START.Length) + CC;
                name = name.Substring(0, midx);

                var value = getValueFromEnvVarOrNavigation(name);
                var macro = new TokenParser(macroSrc, CO, CC)[0];

                value = runMacro(value, macro);

                return value;
            }

        private string getValueFromEnvVarOrNavigation(string name)
		    {
			    if (string.IsNullOrWhiteSpace(name)) return string.Empty;

                var ENV_MOD = m_Configuration.Variable_ENV_MOD;

                if (name.StartsWith(ENV_MOD))
                {
                  name = name.Replace(ENV_MOD, string.Empty);
                  return m_Configuration.ResolveEnvironmentVar(name) ?? string.Empty;
                }
                else
 			     return  Navigate(name).Value ?? string.Empty;
		    }


            private string runMacro(string value, TokenParser.Token macro)
            {
               var config = new MemoryConfiguration();
               config.Create();

               foreach(var key in macro.Keys)
                if (key!=null)
                {
                 var attr = macro[key] as TokenParser.Token.Attribute;
                 if (attr!=null)
                   config.Root.AddAttributeNode( attr.Name, attr.Value );
                }
               return m_Configuration.RunMacro(this, value, macro.Name, config.Root);
            }



            private string replacePaths(string line, string oldValue, string newValue)
            {
               var start = 0;
               while(true)
               {
                 var idx = line.IndexOf(oldValue, start);
                 if (idx<0) break;
                 var path = addPath(line.Substring(0, idx), newValue);
                     start = path.Length - 1;
                     path = addPath(path, line.Substring(idx+oldValue.Length));

                 line = path;
               }

               return line;
            }

            private string addPath(string p1, string p2)
            {
              p1 = p1.Trim();
              p2 = p2.Trim();


              if (p1.Length==0) return p2;
              if (p2.Length==0) return p1;


              if (p1.EndsWith("\\", StringComparison.OrdinalIgnoreCase) ||
                  p1.EndsWith("/", StringComparison.OrdinalIgnoreCase))
              {
               if (p1.Length>1)
                p1 = p1.Remove(p1.Length-1);
               else
                p1 = string.Empty;
              }

              if (p2.StartsWith("\\", StringComparison.OrdinalIgnoreCase) ||
                  p2.StartsWith("/", StringComparison.OrdinalIgnoreCase))
              {
               if (p2.Length>1)
                p2 = p2.Remove(0, 1);
               else
                p2 = string.Empty;
              }

              return p1 + System.IO.Path.DirectorySeparatorChar + p2;
            }




    #endregion

  }

  /// <summary>
  /// Represents an attribute of a attribute node
  /// </summary>
  [Serializable]
  public sealed class ConfigAttrNode : ConfigNode, IConfigAttrNode
  {
    #region .ctor

        internal ConfigAttrNode(Configuration conf, ConfigSectionNode parent) : base(conf, parent)
        {

        }

        internal ConfigAttrNode(Configuration conf, ConfigSectionNode parent, string name, string value)
          : base(conf, parent, name, value)
        {

        }

        internal ConfigAttrNode(Configuration conf, ConfigSectionNode parent, IConfigAttrNode clone)
          : base(conf, parent, clone)
        {

        }

    #endregion

    #region Public
      /// <summary>
      /// Deletes this attribute from its parent
      /// </summary>
      public override void Delete()
      {
        checkCanModify();
        if (!Parent.Exists) return;//this is safeguard

        lock (Parent.m_Attributes)
        {
          Parent.m_Attributes.Remove(this);
        }

        Parent.m_Modified = true;
      }
    #endregion
  }



  [Serializable]internal class ConfigSectionNodeList : List<ConfigSectionNode> { }
  [Serializable]internal class ConfigAttrNodeList : List<ConfigAttrNode> { }

}
