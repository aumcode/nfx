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

namespace NFX.Environment
{

    /// <summary>
    /// Override specifications that dictate what to do when another node supplies entity with the same name
    /// </summary>
    public enum OverrideSpec
    {
      /// <summary>
      /// Override everything: attributes, sections, and value
      /// </summary>
      All=0,

      /// <summary>
      /// Ovveride attributes only
      /// </summary>
      Attributes,

      /// <summary>
      /// Override sections only
      /// </summary>
      Sections,

      /// <summary>
      /// Completely replace node
      /// </summary>
      Replace,

      /// <summary>
      /// Stop override at this level
      /// </summary>
      Stop,

      /// <summary>
      /// Fail the process
      /// </summary>
      Fail
    }


    /// <summary>
    /// Contains node override rule definitions such as override specifier names and values.
    /// This class is used for merging/override of configurations/nodes
    /// </summary>
    public sealed class NodeOverrideRules
    {
       #region CONST

        public const string DEFAULT_OVERRIDE_ATTR_NAME = "_override";
        public const string DEFAULT_OVERRIDE_VALUE_ATTRIBUTES = "attributes";
        public const string DEFAULT_OVERRIDE_VALUE_SECTIONS = "sections";
        public const string DEFAULT_OVERRIDE_VALUE_ALL = "all";
        public const string DEFAULT_OVERRIDE_VALUE_REPLACE = "replace";
        public const string DEFAULT_OVERRIDE_VALUE_STOP = "stop";
        public const string DEFAULT_OVERRIDE_VALUE_FAIL = "fail";

        public const string DEFAULT_SECTION_MATCH_ATTR_NAME = "name";

        public const string DEFAULT_SECTION_CLEAR_NAME = "_clear";
       #endregion


       #region Static

         private static NodeOverrideRules s_Instance = new NodeOverrideRules();

         /// <summary>
         /// Default instance that uses default names
         /// </summary>
         public static NodeOverrideRules Default
         {
           get { return s_Instance; }
         }


       #endregion


       #region .ctor

          public NodeOverrideRules() {}

       #endregion

       #region Fields
            private string m_OverrideAttrName;
              private string m_OverrideValue_Attributes;
              private string m_OverrideValue_Sections;
              private string m_OverrideValue_All;
              private string m_OverrideValue_Replace;
              private string m_OverrideValue_Stop;
              private string m_OverrideValue_Fail;

            private string m_SectionMatchAttrName;
            private string m_SectionClearName;
       #endregion


       #region Properties

        /// <summary>
        /// Provides name for override attribute
        /// </summary>
        public string OverrideAttrName
        {
          get { return m_OverrideAttrName ?? DEFAULT_OVERRIDE_ATTR_NAME;}
          set { m_OverrideAttrName = value; }
        }

        /// <summary>
        /// Provides value for attributes-only override
        /// </summary>
        public string OverrideValue_Attributes
        {
          get { return m_OverrideValue_Attributes ?? DEFAULT_OVERRIDE_VALUE_ATTRIBUTES;}
          set { m_OverrideValue_Attributes = value!=null ? value.ToLower().Trim() : null; }
        }

        /// <summary>
        /// Provides value for sections-only override
        /// </summary>
        public string OverrideValue_Sections
        {
          get { return m_OverrideValue_Sections ?? DEFAULT_OVERRIDE_VALUE_SECTIONS;}
          set { m_OverrideValue_Sections = value!=null ? value.ToLower().Trim() : null; }
        }


        /// <summary>
        /// Provides value for all(sections and attributes) override
        /// </summary>
        public string OverrideValue_All
        {
          get { return m_OverrideValue_All ?? DEFAULT_OVERRIDE_VALUE_ALL;}
          set { m_OverrideValue_All = value!=null ? value.ToLower().Trim() : null; }
        }

        /// <summary>
        /// Provides value for replace override - when overriding section replaces base completely
        /// </summary>
        public string OverrideValue_Replace
        {
          get { return m_OverrideValue_Replace ?? DEFAULT_OVERRIDE_VALUE_REPLACE;}
          set { m_OverrideValue_Replace = value!=null ? value.ToLower().Trim() : null; }
        }


        /// <summary>
        /// Provides value for stop override - so no section can modify anything in this one
        /// </summary>
        public string OverrideValue_Stop
        {
          get { return m_OverrideValue_Stop ?? DEFAULT_OVERRIDE_VALUE_STOP;}
          set { m_OverrideValue_Stop = value!=null ? value.ToLower().Trim() : null; }
        }

        /// <summary>
        /// Provides value for fail override - an exception is thrown when a child tries to override this section
        /// </summary>
        public string OverrideValue_Fail
        {
          get { return m_OverrideValue_Fail ?? DEFAULT_OVERRIDE_VALUE_FAIL;}
          set { m_OverrideValue_Fail = value!=null ? value.ToLower().Trim() : null; }
        }



        /// <summary>
        /// Provides attribute name for matching of multiple sections with the same name, i.e. a logger may have many 'destinations'
        /// subnodes each with different 'name' attribute
        /// </summary>
        public string SectionMatchAttrName
        {
          get { return m_SectionMatchAttrName ?? DEFAULT_SECTION_MATCH_ATTR_NAME;}
          set { m_SectionMatchAttrName = value; }
        }

        /// <summary>
        /// When true will APPEND sections not having SectionMatchAttrName defined,
        /// otherwise matches section names by SectionMatchAttrName
        /// </summary>
        public bool AppendSectionsWithoutMatchAttr
        {
          get;
          set;
        }

        /// <summary>
        /// Provides a name for clear section - when present it deletes all existing subsections
        /// </summary>
        public string SectionClearName
        {
          get { return m_SectionClearName ?? DEFAULT_SECTION_CLEAR_NAME;}
          set { m_SectionClearName = value; }
        }

       #endregion

       #region Public

          /// <summary>
          /// Tries to convert a string to OverrideSpec enum. If string is null or empty then "All" is returned, otherwise exception is thrown if
          ///  the value does not match any of the expected values. The comparison is case-insensitive
          /// </summary>
          public OverrideSpec StringToOverrideSpec(string str)
          {
            if (string.IsNullOrEmpty(str)) return OverrideSpec.All;

            str = str.ToLower().Trim();

            if (str==OverrideValue_All) return OverrideSpec.All;
            if (str==OverrideValue_Attributes) return OverrideSpec.Attributes;
            if (str==OverrideValue_Sections) return OverrideSpec.Sections;
            if (str==OverrideValue_Replace) return OverrideSpec.Replace;
            if (str==OverrideValue_Stop) return OverrideSpec.Stop;
            if (str==OverrideValue_Fail) return OverrideSpec.Fail;

            throw new ConfigException(StringConsts.CONFIGURATION_OVERRIDE_SPEC_ERROR + str);
          }


       #endregion

    }
}
