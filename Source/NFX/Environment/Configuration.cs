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
 * Revision: NFX 0.3  2009.10.12
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NFX.Serialization.JSON;

namespace NFX.Environment
{
  /// <summary>
  /// Provides top-level configuration abstraction
  /// </summary>
  [Serializable]
  public abstract class Configuration : ICloneable
  {
    #region CONSTS

          public const  string  DEFAULT_CONFIG_INCLUDE_PRAGMA = "_include";

          public const  string  CONFIG_INCLUDE_PRAGMA_FS_SECTION = "fs";
          public const  string  CONFIG_INCLUDE_PRAGMA_SESSION_SECTION = "session";
          public const  string  CONFIG_INCLUDE_PRAGMA_FILE_ATTR = "file";
          public const  string  CONFIG_INCLUDE_PRAGMA_REQUIRED_ATTR = "required";

          public const  string  DEFAULT_VAR_ESCAPE = "$(###)";
          public const  string  DEFAULT_VAR_START = "$(";
          public const  string  DEFAULT_VAR_END = ")";
          public const  string  DEFAULT_VAR_PATH_MOD = "@";
          public const  string  DEFAULT_VAR_ENV_MOD = "~";

          public const  string  DEFAULT_VAR_MACRO_START = "::";

          public const  string  CONFIG_NAME_ATTR = "name";
          public const  string  CONFIG_ORDER_ATTR = "order";


          public const  string  CONFIG_LACONIC_FORMAT = "laconf";

    #endregion

    #region Static

        /// <summary>
        /// Creates a new empty config root based on laconic format
        /// </summary>
        public static ConfigSectionNode NewEmptyRoot(string name = null)
        {
          var cfg = new LaconicConfiguration();
          cfg.Create(name);
          return cfg.Root;
        }

        /// <summary>
        /// Returns all configuration file formats (file extensions without '.') supported
        /// by ProviderLoadFromFile/ProviderLoadFromAnySupportedFormatFile/ProviderLoadFromString
        /// </summary>
        public static IEnumerable<string> AllSupportedFormats
        {
          get
          {
            return NFX.CodeAnalysis.Laconfig.LaconfigLanguage.Instance.FileExtensions
                                            .Concat(NFX.CodeAnalysis.XML.XMLLanguage.Instance.FileExtensions)
                                            .Concat(NFX.CodeAnalysis.JSON.JSONLanguage.Instance.FileExtensions);
          }
        }

        /// <summary>
        /// Loads the contents of the supplied file name in an appropriate configuration provider implementation for the supplied extension format
        /// </summary>
        public static Configuration ProviderLoadFromFile(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();

            if (ext.StartsWith(".")) ext=ext.Remove(0, 1);

            //since C# does not support first-class types, these if statements below must handle what AllSupportedFormat returns
            //in future Aum conversion replace with Dictionary<format, configType> lookup

            if (NFX.CodeAnalysis.Laconfig.LaconfigLanguage.Instance.FileExtensions.Any(e => string.Equals(e, ext, StringComparison.InvariantCultureIgnoreCase) ))
              return new LaconicConfiguration(fileName);

            if (NFX.CodeAnalysis.XML.XMLLanguage.Instance.FileExtensions.Any(e => string.Equals(e, ext, StringComparison.InvariantCultureIgnoreCase) ))
              return new XMLConfiguration(fileName);

            if (NFX.CodeAnalysis.JSON.JSONLanguage.Instance.FileExtensions.Any(e => string.Equals(e, ext, StringComparison.InvariantCultureIgnoreCase) ))
              return new JSONConfiguration(fileName);

            throw new ConfigException(StringConsts.CONFIG_NO_PROVIDER_LOAD_FILE_ERROR + fileName);
        }

        /// <summary>
        /// Loads the contents of the supplied file name without format extension trying to match any of the supported format extensions.
        /// When match is found the file is loaded via an appropriate configuration provider
        /// </summary>
        /// <example>
        /// Given "c:\conf\users" as an input:
        ///   if "c:\conf\users.xml" exists then it will be opened as XMLConfiguration
        ///   if "c:\conf\users.laconf" exists then it will be opened as LaconicConfiguration
        ///   if "c:\conf\users.json" exists then it will be opened as JSONConfiguration
        ///   and so on... for the rest of supported formats
        /// </example>
        public static Configuration ProviderLoadFromAnySupportedFormatFile(string fileName)
        {
            if (fileName.IsNotNullOrWhiteSpace())
            {
              if (fileName.EndsWith(".")) fileName=fileName.Remove(fileName.Length-1);

              foreach(var fmt in AllSupportedFormats)
              {
                var fn =  "{0}.{1}".Args(fileName, fmt);
                if (File.Exists(fn)) return ProviderLoadFromFile(fn);
              }
            }

            throw new ConfigException(StringConsts.CONFIG_NO_PROVIDER_LOAD_FILE_ERROR + fileName);
        }


        /// <summary>
        /// Loads the supplied string content in the specified format, which may be format name like "xml" or "laconfig" with or without extension period
        /// </summary>
        public static Configuration ProviderLoadFromString(string content, string format)
        {
            if (format.StartsWith(".")) format=format.Remove(0, 1);

            //since C# does not support first-class types, these if statements below must handle what AllSupportedFormat returns
            //in future Aum conversion replace with Dictionary<format, configType> lookup

            if (NFX.CodeAnalysis.Laconfig.LaconfigLanguage.Instance.FileExtensions.Any(e => string.Equals(e, format, StringComparison.InvariantCultureIgnoreCase) ))
              return LaconicConfiguration.CreateFromString(content);

            if (NFX.CodeAnalysis.XML.XMLLanguage.Instance.FileExtensions.Any(e => string.Equals(e, format, StringComparison.InvariantCultureIgnoreCase) ))
              return XMLConfiguration.CreateFromXML(content);

            if (NFX.CodeAnalysis.JSON.JSONLanguage.Instance.FileExtensions.Any(e => string.Equals(e, format, StringComparison.InvariantCultureIgnoreCase) ))
              return JSONConfiguration.CreateFromJSON(content);

            throw new ConfigException(StringConsts.CONFIG_NO_PROVIDER_LOAD_FORMAT_ERROR + format);
        }


        /// <summary>
        /// Gets/sets global Environment variable resolver that is used by all configurations in this process instance
        /// </summary>
        public static IEnvironmentVariableResolver ProcesswideEnvironmentVarResolver;

    #endregion


    #region .ctor
        protected Configuration()
        {
          m_EmptySectionNode = new ConfigSectionNode(this, null);
          m_EmptyAttrNode = new ConfigAttrNode(this, null);

          m_EmptySectionNode.__Empty = true;
          m_EmptyAttrNode.__Empty = true;

          m_Root = m_EmptySectionNode;
        }


    #endregion

    #region Private/Protected Fields

      private bool m_StrictNames = true;

      protected ConfigSectionNode m_Root;

      //not static because nodes retain Configuration ownership, hence every config has its own set of sentinel nodes
      protected internal ConfigSectionNode m_EmptySectionNode;
      protected internal ConfigAttrNode m_EmptyAttrNode;


      private IEnvironmentVariableResolver m_EnvironmentVarResolver;
      private IMacroRunner m_MacroRunner;
      private object m_MacroRunnerContext;

      private string m_Variable_ESCAPE = DEFAULT_VAR_ESCAPE;
      private string m_Variable_START = DEFAULT_VAR_START;
      private string m_Variable_END   = DEFAULT_VAR_END;
      private string m_Variable_PATH_MOD = DEFAULT_VAR_PATH_MOD;
      private string m_Variable_ENV_MOD  = DEFAULT_VAR_ENV_MOD;

      private string m_Variable_MACRO_START = DEFAULT_VAR_MACRO_START;



    #endregion

    #region Public properties

        /// <summary>
        /// Accesses root section configuration node
        /// </summary>
        public ConfigSectionNode Root
        {
          get
          {
            if (m_Root != null)
              return m_Root;
            else
              return m_EmptySectionNode;
          }
        }


        /// <summary>
        /// Determines whether exception is thrown when configuration node name contains
        /// inappropriate chars for particular configuration type. For example,
        ///  for XMLConfiguration node names may not have spaces and other separator chars.
        /// When StrictNames is false then particular configurations may replace incompatible
        ///  chars in node names with neutral ones (i.e. "my value"->"my-value" in case of XMLConfiguration).
        /// </summary>
        public bool StrictNames
        {
          get { return m_StrictNames;}
          set {m_StrictNames = value; }
        }


        /// <summary>
        /// Indicates whether configuration is read-only
        /// </summary>
        public abstract bool IsReadOnly { get; }


        /// <summary>
        /// References variable resolver. If this property is not set then default Windows environment var resolver is used
        /// </summary>
        public IEnvironmentVariableResolver EnvironmentVarResolver
        {
          get { return m_EnvironmentVarResolver; }
          set { m_EnvironmentVarResolver = value; }
        }

        /// <summary>
        /// References macro runner. If this property is not set then default macro runner is used
        /// </summary>
        public IMacroRunner MacroRunner
        {
          get { return m_MacroRunner; }
          set { m_MacroRunner = value; }
        }


        /// <summary>
        /// Gets/sets an object passed by the framework into MacroRunner.Run() method.
        /// This property is auto-set for classes decorated with [ConfigMacroContext] attribute
        /// </summary>
        public object MacroRunnerContext
        {
          get { return m_MacroRunnerContext; }
          set { m_MacroRunnerContext = value; }
        }

        /// <summary>
        /// References a special instance of an empty section node (one per configuration).
        /// Empty nodes are returned by indexers when a real node with specified name does not exist
        /// </summary>
        public ConfigSectionNode EmptySection
        {
          get { return m_EmptySectionNode; }
        }

        /// <summary>
        /// References a special instance of an empty attribute node (one per configuration).
        /// Empty nodes are returned by indexers when a real node with specified name does not exist
        /// </summary>
        public ConfigAttrNode EmptyAttr
        {
          get { return m_EmptyAttrNode; }
        }


        /// <summary>
        /// Variable escape tag
        /// </summary>
        public string Variable_ESCAPE
        {
          get { return m_Variable_ESCAPE ?? DEFAULT_VAR_ESCAPE; }
          set { m_Variable_ESCAPE = value;}
        }


        /// <summary>
        /// Variable start tag
        /// </summary>
        public string Variable_START
        {
          get { return m_Variable_START ?? DEFAULT_VAR_START; }
          set { m_Variable_START = value;}
        }

        /// <summary>
        /// Variable end tag
        /// </summary>
        public string Variable_END
        {
          get { return m_Variable_END ?? DEFAULT_VAR_END; }
          set { m_Variable_END = value;}
        }

        /// <summary>
        /// Variable path modifier
        /// </summary>
        public string Variable_PATH_MOD
        {
          get { return m_Variable_PATH_MOD ?? DEFAULT_VAR_PATH_MOD; }
          set { m_Variable_PATH_MOD = value;}
        }

        /// <summary>
        /// Variable environment modifier
        /// </summary>
        public string Variable_ENV_MOD
        {
          get { return m_Variable_ENV_MOD ?? DEFAULT_VAR_ENV_MOD; }
          set { m_Variable_ENV_MOD = value;}
        }

        /// <summary>
        /// Variable get clause modifier
        /// </summary>
        public string Variable_MACRO_START
        {
          get { return m_Variable_MACRO_START ?? DEFAULT_VAR_MACRO_START; }
        }


        /// <summary>
        /// Primarily used for debugging - returns the content of the configuration as text in the pretty-printed Laconic format
        /// </summary>
        public string ContentView
        {
          get
          {
            if (this.Root.Exists)
            {
              return "Configuration Type: {0}\n-----------------------------------------------------\n{1}".Args(
                               this.GetType().FullName,
                               ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint)
                               );
            }
            else
              return "Configuration Type: {0} <empty>".Args( this.GetType().FullName);
          }
        }

    #endregion

    #region Public


        /// <summary>
        /// Creates new configuration - creates new configuration root with optional name parameter
        /// </summary>
        public void Create(string name = "configuration")
        {
          m_Root = new ConfigSectionNode(this, null, name, null);
        }

        /// <summary>
        /// Creates new configuration from ordered merge result of two other nodes - base and override which can be from different configurations
        /// </summary>
        /// <param name="baseNode">A base node that data is defaulted from</param>
        /// <param name="overrideNode">A node that contains overrides/additions of/to data from base node</param>
        /// <param name="rules">Rules to use or default rules will be used in null is passed</param>
        public void CreateFromMerge(ConfigSectionNode baseNode, ConfigSectionNode overrideNode, NodeOverrideRules rules = null)
        {
          m_Root = new ConfigSectionNode(this, null, baseNode);
          m_Root.OverrideBy(overrideNode, rules);
        }

        /// <summary>
        /// Creates new configuration from other node, which may belong to a different configuration instance
        /// </summary>
        /// <param name="otherNode">A base node that data is defaulted from</param>
        public void CreateFromNode(IConfigSectionNode otherNode)
        {
          m_Root = new ConfigSectionNode(this, null, otherNode);
        }


        /// <summary>
        /// Erases all config data
        /// </summary>
        public void Destroy()
        {
          m_Root = null;
        }


        /// <summary>
        /// Re-reads configuration from source
        /// </summary>
        public virtual void Refresh()
        {

        }

        /// <summary>
        /// Saves configuration to source
        /// </summary>
        public virtual void Save()
        {
          if (m_Root != null)
            m_Root.ResetModified();
        }

        /// <summary>
        /// Checks node name for aptitude for particular configuration type.
        /// For example, XML configuration does not allow nodes with spaces or separator chars.
        /// When StrictNames is set to true and value is not appropriate then exception is thrown
        /// </summary>
        public string CheckAndAdjustNodeName(string name)
        {
          name = name ?? string.Empty;

          var result = AdjustNodeName(name);

          if (m_StrictNames)
           if (!string.Equals(name, result, StringComparison.OrdinalIgnoreCase))
            throw new ConfigException(string.Format(StringConsts.CONFIGURATION_NODE_NAME_ERROR, name));

          return result;
        }

        /// <summary>
        /// Resolves variable name into its value
        /// </summary>
        public string ResolveEnvironmentVar(string name, IEnvironmentVariableResolver resolver = null)
        {
          if (resolver!=null)
           return resolver.ResolveEnvironmentVariable(name);

          if (m_EnvironmentVarResolver != null)
            return m_EnvironmentVarResolver.ResolveEnvironmentVariable(name);

          var process = ProcesswideEnvironmentVarResolver;
          if (process != null)
            return process.ResolveEnvironmentVariable(name);

          return WindowsEnvironmentVariableResolver.Instance.ResolveEnvironmentVariable(name);
        }


        /// <summary>
        /// Runs macro and returns its value
        /// </summary>
        public string RunMacro(IConfigSectionNode node, string inputValue, string macroName, IConfigSectionNode macroParams, IMacroRunner runner = null, object context = null)
        {
          if (context==null) context = m_MacroRunnerContext;

          if (runner!=null)
           return runner.Run(node, inputValue, macroName, macroParams, context);

          if (m_MacroRunner != null)
            return m_MacroRunner.Run(node, inputValue, macroName, macroParams, context);


          return DefaultMacroRunner.Instance.Run(node, inputValue, macroName, macroParams, context);
        }



        /// <summary>
        /// Creates a deep copy of this configuration into new instance of T
        /// </summary>
        public Configuration Clone<T>() where T : Configuration, new()
        {
          return Clone(new T());
        }

        /// <summary>
        /// Creates a deep copy of this configuration into newInstance which was allocated externally
        /// </summary>
        public Configuration Clone(Configuration newInstance)
        {
          var result = newInstance;
          result.m_Root = new ConfigSectionNode(result, null, this.m_Root);

          newInstance.StrictNames = this.StrictNames;

          return result;
        }

        /// <summary>
        /// Implements IClonable by returning new MemoryConfiguration clone
        /// </summary>
        public object Clone()
        {
          return Clone<MemoryConfiguration>();
        }


        /// <summary>
        /// Completely replaces this node with another node tree, positioning the new tree in the place of local node.
        /// Existing node is deleted after this operation completes, in its place child nodes from other node are inserted
        /// preserving their existing order. Attributes of other node get merged into parent of existing node
        /// </summary>
        public void Include(ConfigSectionNode existing, ConfigSectionNode other)
        {
             if (!Root.Exists) return;

             if (IsReadOnly)
                throw new ConfigException(StringConsts.CONFIGURATION_READONLY_ERROR);

             if (existing==null || other==null)
                throw new ConfigException(StringConsts.ARGUMENT_ERROR + "Configuration.Include(null|null)");

             if (existing.Configuration!=this)
                throw new ConfigException(StringConsts.CONFIGURATION_NODE_DOES_NOT_BELONG_TO_THIS_CONFIGURATION_ERROR.Args(existing.RootPath));

             if (other.Configuration==this)
                throw new ConfigException(StringConsts.CONFIGURATION_NODE_MUST_NOT_BELONG_TO_THIS_CONFIGURATION_ERROR.Args(other.RootPath));

             if (existing == Root)
                throw new ConfigException(StringConsts.CONFIGURATION_CAN_NOT_INCLUDE_INSTEAD_OF_ROOT_ERROR.Args(other.RootPath));

             existing.include(other);
        }

        /// <summary>
        /// Serializes configuration tree into Laconic format and returns it as a string
        /// </summary>
        public string ToLaconicString(NFX.CodeAnalysis.Laconfig.LaconfigWritingOptions options = null)
        {
           return NFX.CodeAnalysis.Laconfig.LaconfigWriter.Write(this, options);
        }

        /// <summary>
        /// Serializes configuration tree into Laconic format and writes it into stream
        /// </summary>
        public void ToLaconicStream(Stream stream, NFX.CodeAnalysis.Laconfig.LaconfigWritingOptions options = null, Encoding encoding = null)
        {
           NFX.CodeAnalysis.Laconfig.LaconfigWriter.Write(this, stream, options, encoding );
        }

        /// <summary>
        /// Serializes configuration tree into Laconic format and writes it into a file
        /// </summary>
        public void ToLaconicFile(string filename, NFX.CodeAnalysis.Laconfig.LaconfigWritingOptions options = null, Encoding encoding = null)
        {
           using(var fs = new FileStream(filename, FileMode.Create))
              NFX.CodeAnalysis.Laconfig.LaconfigWriter.Write(this, fs, options, encoding);
        }

        /// <summary>
        /// Returns this config as JSON data map suitable for making JSONConfiguration
        /// </summary>
        public JSONDataMap ToConfigurationJSONDataMap()
        {
          if (m_Root==null) return new JSONDataMap(false);

          return m_Root.ToConfigurationJSONDataMap();
        }

    #endregion

    #region Protected Utils

      /// <summary>
      /// Override to perform transforms on node names so they become suitable for particular configuration type
      /// </summary>
      protected virtual string AdjustNodeName(string name)
      {
        return name;
      }

    #endregion

  }
}
