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
 * Originated: 2009.10
 * Revision: NFX 1.0  2011.01.31
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace NFX.Environment
{
  /// <summary>
  /// Provides implementation of configuration based on arguments supplied from command line
  ///  which is "string[]". Arguments start with either "/" or "-" prefix. If any argument is not
  ///  prefixed then it is written as an auto-named attribute node of the root with its value set, otherwise a section (under root) with
  ///   argument's name is created. Any argument may have options. Any option may either consist of name
  ///    or name value pair delimited by "=".
  ///  Argument options are written as attribute nodes of their corresponding sections.
  ///  If option value specified without name (without "=") then option is auto-named
  /// </summary>
  /// <example>
  ///  Given command line:
  ///   <code>
  ///   c:\>dosomething.exe "c:\input.file" "d:\output.file" -compress level=100 method=zip -shadow fast -large
  ///   </code>
  ///  The following configuration object will be created from the supplied args:
  ///  <code>
  ///    [args ?1="c:\input.file" ?2="c:\output.file"]
  ///      [compress level="100" method="zip"]
  ///      [shadow ?1="fast"]
  ///      [large]
  ///  </code>
  ///
  ///  Use args:
  ///  <code>
  ///   var conf = new CmdArgsConfiguration(args);
  ///   var inFile = conf.Root.AttrByIndex(0).ValueAsString(DEFAULT_INPUT_FILE);
  ///   var outFile = conf.Root.AttrByIndex(1).ValueAsString(DEFAULT_OUTPUT_FILE);
  ///   .....
  ///    if (conf.Root["large"].Exists) .......
  ///   .....
  ///   var level = conf.Root["compress"].AttrByName("level").ValueAsInt(DEFAULT_COMPRESSION_LEVEL);
  ///   .....
  ///  </code>
  ///
  /// </example>
  [Serializable]
  public class CommandArgsConfiguration : Configuration
  {
    #region CONSTS

        public const string ARG_PREFIX1 = "/";
        public const string ARG_PREFIX2 = "-";
        public const char OPTION_EQ = '=';
        public const string ROOT_NODE_NAME = "args";

    #endregion

    #region .ctor
      /// <summary>
      /// Creates an instance of the new configuration parsed from command line arguments
      /// </summary>
      public CommandArgsConfiguration(string[] args)
        : base()
      {
        m_Args = args;
        parseArgs();
        m_Loaded = true;
      }

    #endregion


    #region Private Fields

      private bool m_Loaded = false;
      private string[] m_Args;

    #endregion

    #region Public Properties

        /// <summary>
        /// Indicates whether configuration is readonly or may be modified and saved
        /// </summary>
        public override bool IsReadOnly
        {
            get { return m_Loaded; }
        }

        /// <summary>
        /// Returns arguments array that this configuration was parsed from
        /// </summary>
        public string[] Arguments
        {
            get { return m_Args; }
        }


    #endregion


    #region .pvt .impl


    // -arg1 -arg2 -arg3 opt1 opt2 -arg4 optA=v1 optB=v2
    private void parseArgs()
    {

      m_Root = new ConfigSectionNode(this, null, ROOT_NODE_NAME, string.Empty);

      var uargcnt = 1; //unknown arg length
      for (int i = 0; i < m_Args.Length; )
      {
        var argument = m_Args[i];

        if (argument.Length > 1 && (argument.StartsWith(ARG_PREFIX1) || argument.StartsWith(ARG_PREFIX2)))
        {
          argument = argument.Remove(0, 1);//get rid of prefix
          var argNode = m_Root.AddChildNode(argument, null);

          var uopcnt = 1;//unknown option length
          for (i++; i < m_Args.Length; )//read args's options
          {
            var option = m_Args[i];
            if (option.StartsWith(ARG_PREFIX1) || option.StartsWith(ARG_PREFIX2)) break;
            i++;

            var j = option.IndexOf(OPTION_EQ);

            if (j < 0)
            {
              argNode.AddAttributeNode(string.Format("?{0}", uopcnt), option);
              uopcnt++;
            }
            else
            {
              var name = option.Substring(0, j);
              var val = (j < option.Length - 1) ? option.Substring(j + 1) : string.Empty;

              if (string.IsNullOrEmpty(name))
              {
                name = string.Format("?{0}", uopcnt);
                uopcnt++;
              }
              argNode.AddAttributeNode(name, val);
            }
          }
        }
        else
        {
          m_Root.AddAttributeNode(string.Format("?{0}", uargcnt), argument);
          uargcnt++;
          i++;
        }
      }

      m_Root.ResetModified();
    }


    #endregion
  }
}
