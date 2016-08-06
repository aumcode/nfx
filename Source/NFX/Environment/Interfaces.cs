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
 * Originated: 2010.03.20
 * Revision: NFX 0.8  2010.03.20
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Serialization.JSON;
using NFX.DataAccess.Distributed;

namespace NFX.Environment
{
      /// <summary>
      /// Designates entities that may be configured
      /// </summary>
      public interface IConfigurable
      {
        /// <summary>
        /// Configures an entity from supplied config node
        /// </summary>
        void Configure(IConfigSectionNode node);
      }


      /// <summary>
      /// Designates entities that may persist their parameters/state in configuration
      /// </summary>
      public interface IConfigurationPersistent
      {
        /// <summary>
        /// Persists relevant entities parameters/state into configuration
        /// </summary>
        void PersistConfiguration(ConfigSectionNode node);
      }


      /// <summary>
      /// Provides read-only configuration node abstraction for section and attribute nodes
      /// </summary>
      public interface IConfigNode : INamed
      {
            /// <summary>
            /// References configuration this node is under
            /// </summary>
            Configuration Configuration { get; }

            /// <summary>
            /// Determines whether this node really exists in configuration or is just a sentinel empty node
            /// </summary>
            bool Exists { get; }

            /// <summary>
            /// Returns varbatim (without variable evaluation) node value or null
            /// </summary>
            string VerbatimValue { get; }

            /// <summary>
            /// Returns null or value of this node with all variables evaluated
            /// </summary>
            string EvaluatedValue { get; }

            /// <summary>
            /// Returns null or value of this node with all variables evaluated
            /// </summary>
            string Value { get; }

            /// <summary>
            /// References parent node or empty node if this is the top-most node with no parent
            /// </summary>
            IConfigSectionNode Parent { get; }


            /// <summary>
            /// Returns a path from root to this node
            /// </summary>
            string RootPath { get; }



            string     ValueAsString(string dflt = null, bool verbatim = false);
            byte[]     ValueAsByteArray(byte[] dflt = null, bool verbatim = false);
            short      ValueAsShort(short dflt = 0, bool verbatim = false);
            short?     ValueAsNullableShort(short? dflt = 0, bool verbatim = false);
            ushort     ValueAsUShort(ushort dflt = 0, bool verbatim = false);
            ushort?    ValueAsNullableUShort(ushort? dflt = 0, bool verbatim = false);
            byte       ValueAsByte(byte dflt = 0, bool verbatim = false);
            byte?      ValueAsNullableByte(byte? dflt = 0, bool verbatim = false);
            sbyte      ValueAsSByte(sbyte dflt = 0, bool verbatim = false);
            sbyte?     ValueAsNullableSByte(sbyte? dflt = 0, bool verbatim = false);
            int        ValueAsInt(int dflt = 0, bool verbatim = false);
            int?       ValueAsNullableInt(int? dflt = 0, bool verbatim = false);
            uint       ValueAsUInt(uint dflt = 0, bool verbatim = false);
            uint?      ValueAsNullableUInt(uint? dflt = 0, bool verbatim = false);
            long       ValueAsLong(long dflt = 0, bool verbatim = false);
            long?      ValueAsNullableLong(long? dflt = 0, bool verbatim = false);
            ulong      ValueAsULong(ulong dflt = 0, bool verbatim = false);
            ulong?     ValueAsNullableULong(ulong? dflt = 0, bool verbatim = false);
            double     ValueAsDouble(double dflt = 0d, bool verbatim = false);
            double?    ValueAsNullableDouble(double? dflt = 0d, bool verbatim = false);
            float      ValueAsFloat(float dflt = 0f, bool verbatim = false);
            float?     ValueAsNullableFloat(float? dflt = 0f, bool verbatim = false);
            decimal    ValueAsDecimal(decimal dflt = 0m, bool verbatim = false);
            decimal?   ValueAsNullableDecimal(decimal? dflt = 0m, bool verbatim = false);
            bool       ValueAsBool(bool dflt = false, bool verbatim = false);
            bool?      ValueAsNullableBool(bool? dflt = false, bool verbatim = false);
            Guid       ValueAsGUID(Guid dflt, bool verbatim = false);
            Guid?      ValueAsNullableGUID(Guid? dflt = null, bool verbatim = false);
            GDID       ValueAsGDID(GDID dflt, bool verbatim = false);
            GDID?      ValueAsNullableGDID(GDID? dflt = null, bool verbatim = false);
            DateTime   ValueAsDateTime(DateTime dflt, bool verbatim = false);
            DateTime?  ValueAsNullableDateTime(DateTime? dflt = null, bool verbatim = false);
            TimeSpan   ValueAsTimeSpan(TimeSpan dflt, bool verbatim = false);
            TimeSpan?  ValueAsNullableTimeSpan(TimeSpan? dflt = null, bool verbatim = false);
            TEnum      ValueAsEnum<TEnum>(TEnum dflt = default(TEnum), bool verbatim = false) where TEnum : struct;
            TEnum?     ValueAsNullableEnum<TEnum>(TEnum? dflt = null, bool verbatim = false) where TEnum : struct;

            /// <summary>
            /// Tries to get value as specified type or throws if it can not be converted
            /// </summary>
            object ValueAsType(Type tp, bool verbatim = false, bool strict = true);


            /// <summary>
            /// Returns true when another node has the same name as this one
            /// </summary>
            bool IsSameName(IConfigNode other);

            /// <summary>
            /// Returns true when another name is the same as this node's name
            /// </summary>
            bool IsSameName(string other);

      }

      /// <summary>
      /// Provides read-only configuration section node abstraction
      /// </summary>
      public interface IConfigSectionNode : IConfigNode
      {
            /// <summary>
            /// Indicates whether this node has any child section nodes
            /// </summary>
            bool HasChildren { get; }

            /// <summary>
            /// Returns number of child section nodes
            /// </summary>
            int ChildCount { get; }

            /// <summary>
            /// Indicates whether this node has any associated attributes
            /// </summary>
            bool HasAttributes { get; }

            /// <summary>
            /// Returns number of child attribute nodes
            /// </summary>
            int AttrCount { get; }

            /// <summary>
            /// Enumerates all child nodes
            /// </summary>
            IEnumerable<IConfigSectionNode> Children { get; }

            /// <summary>
            /// Enumerates all attribute nodes
            /// </summary>
            IEnumerable<IConfigAttrNode> Attributes { get; }

            /// <summary>
            /// Retrieves section node by names, from left to right until existing node is found.
            /// If no existing node could be found then empty node instance is returned
            /// </summary>
            IConfigSectionNode this[params string[] names] { get; }

            /// <summary>
            /// Retrieves section node by index or empty node instance if section node with such index could not be found
            /// </summary>
            IConfigSectionNode this[int idx] { get; }



            /// <summary>
            /// Returns attribute node by its name or empty attribute if real attribute with such name does not exist
            /// </summary>
            IConfigAttrNode AttrByName(string name, bool autoCreate = false);

            /// <summary>
            /// Returns attribute node by its index or empty attribute if real attribute with such index does not exist
            /// </summary>
            IConfigAttrNode AttrByIndex(int idx);

            /// <summary>
            /// Navigates the path and return the appropriate node. Example '!/nfx/logger/destination/$file-name'
            /// </summary>
            /// <param name="path">If path starts from '!' then exception will be thrown if such a node does not exist;
            ///  Use '/' as leading char for root,
            ///  '..' for step up,
            ///  '$' for attribute name. Multiple paths may be coalesced using '|' or ';'
            /// </param>
            IConfigNode Navigate(string path);


            /// <summary>
            /// Navigates the path and return the appropriate section node. Example '!/nfx/logger/destination'
            /// </summary>
            /// <param name="path">If path starts from '!' then exception will be thrown if such a section node does not exist;
            ///  Use '/' as leading char for root,
            ///  '..' for step up. Multiple paths may be coalesced using '|' or ';'
            /// </param>
            IConfigSectionNode NavigateSection(string path);

            /// <summary>
            /// Evaluates a value string expanding all variables with var-paths relative to this node.
            /// Evaluates configuration variables such as "$(varname)" or "$(@varname)". Varnames are paths
            /// to other config nodes from the same configuration or variable names when prefixed with "~". If varname starts with "@" then it gets combined
            ///  with input as path string. "~" is used to qualify environment vars that get resolved through Configuration.EnvironmentVarResolver
            ///  Example: `....add key="Schema.$(/A/B/C/$attr)" value="$(@~HOME)bin\Transforms\"...`
            /// </summary>
            string EvaluateValueVariables(string value);

            /// <summary>
            /// Returns true when this and another nodes both have attribute "name" and their values are equal per case-insensitive culture-neutral comparison
            /// </summary>
            bool IsSameNameAttr(IConfigSectionNode other);

            /// <summary>
            /// Returns true when this node has an attribute called "name" and its value is euqal to the supplied value per case-insensitive culture-neutral comparison
            /// </summary>
            bool IsSameNameAttr(string other);

            /// <summary>
            /// Serializes configuration tree rooted at this node into Laconic format and returns it as a string
            /// </summary>
            string ToLaconicString(NFX.CodeAnalysis.Laconfig.LaconfigWritingOptions options = null);

            /// <summary>
            /// Converts this ConfigSectionNode to JSONDataMap. Contrast with ToConfigurationJSONDataMap
            /// Be carefull: that this operation can "loose" data from ConfigSectionNode.
            /// In other words some ConfigSectionNode information can not be reflected in corresponding JSONDataMap, for example
            ///  this method overwrites duplicate key names and does not support section values
            /// </summary>
            JSONDataMap ToJSONDataMap();

            /// <summary>
            /// Returns the contents of this node per JSONConfiguration specification. Contrast with ToJSONDataMap
            /// </summary>
            JSONDataMap ToConfigurationJSONDataMap();

            /// <summary>
            /// Serializes configuration tree rooted at this node into JSON configuration format and returns it as a string
            /// </summary>
            string ToJSONString(NFX.Serialization.JSON.JSONWritingOptions options = null);

            /// <summary>
            /// Returns attribute values as string map
            /// </summary>
            Collections.StringMap AttrsToStringMap(bool verbatim = false);
      }

      /// <summary>
      /// Represents a read-only attribute of a attribute node
      /// </summary>
      public interface IConfigAttrNode : IConfigNode
      {

      }


      /// <summary>
      /// Represents an entity that provides a type-safe access to configuration settings that come from Configuration nodes.
      /// This class obviates the need for navigation between config nodes on every property get and facilitates faster access to some config parameters
      /// that need to be gotten efficiently, as they are now kept cached in RAM in native format (i.e. DateTime vs. string) as fields.
      /// Usually classes that implement this interface are singleton and they get registered with the application using IApplication.RegisterConfigSettings()
      /// method. Warning: the implementation must be thread-safe and allow property getters to keep reading while ConfigChanged() notification happens
      /// </summary>
      public interface IConfigSettings
      {
          /// <summary>
          /// Notifies the implementer that underlying source configuration has changed and memory-resident
          /// fields need to be re-read from config nodes. Usually this method is called by application container
          ///  when instance of this class has been registered with the application using IApplication.RegisterConfigSettings().
          /// Warning: the implementation must be thread-safe and allow getters to keep reading while notification happens
          /// </summary>
          /// <param name="atNode">
          /// Passes the most top-level node that covers all of the changes that happened in the source config system.
          /// Usually this is a root config node. The capability of source config change detection on node level is not supported by all providers
          /// </param>
          void ConfigChanged(IConfigSectionNode atNode);
      }



}
