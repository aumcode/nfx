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
using System.Reflection;
using System.Text;
using NFX.ApplicationModel;

namespace NFX.Environment
{
  /// <summary>
  /// Provides helper methods for dynamic object creation and configuration
  /// </summary>
  public static class FactoryUtils
  {
    #region CONSTS

        public const string CONFIG_TYPE_ATTR = "type";

    #endregion

    #region Public

        /// <summary>
        /// Creates and configures an instance of appropriate configurable object as specified in supplied config node.
        /// Applies configured behaviours
        /// </summary>
        public static IConfigurable MakeAndConfigure(IConfigSectionNode node, Type defaultType = null, object[] args = null)
        {
            return MakeAndConfigure<IConfigurable>(node, defaultType, args);
        }

        /// <summary>
        /// Creates and configures an instance of appropriate configurable object as specified in supplied config node.
        /// Applies configured behaviours
        /// </summary>
        public static T MakeAndConfigure<T>(IConfigSectionNode node, Type defaultType = null, object[] args = null)
            where T : IConfigurable
        {
            var result = Make<T>(node, defaultType, args);

            //20130708 DKh - support for [ConfigMacroContext] injection
            var etp = result.GetType();
            var macroAttr = etp.GetCustomAttributes(typeof(ConfigMacroContextAttribute), true).FirstOrDefault() as ConfigMacroContextAttribute;
            if (macroAttr!=null)
                 node.Configuration.MacroRunnerContext = result;
            //---

            result.Configure(node);

            Behavior.ApplyConfiguredBehaviors(result, node);

            return result;
        }

        /// <summary>
        /// Creates an instance of appropriate configurable object as specified in supplied config node.
        /// This function does not configure the instance
        /// </summary>
        public static T Make<T>(IConfigSectionNode node, Type defaultType = null, object[] args = null)
        {
            try
            {
                var tName = (node!=null && node.Exists) ? node.AttrByName(CONFIG_TYPE_ATTR).Value : null;
                return make<T>(tName, defaultType, args);
            }
            catch(Exception error)
            {
                throw new ConfigException(string.Format(StringConsts.CONFIGURATION_TYPE_CREATION_ERROR,
                                                         (node!=null) ? node.RootPath : StringConsts.NULL_STRING,
                                                         error.ToMessageWithType()
                                                       ),
                                          error);
            }
        }


        /// <summary>
        /// Invokes a constructor for type feeding it the specified args:
        ///  node{type="NS.Type, Assembly" arg0=1 arg1=true....}
        /// If the typePattern is passed, then the '*' in pattern is replaced with 'type' attr content.
        /// This is needed for security, as this method allows to inject any type with any ctor params when typePattern is null
        /// </summary>
        public static T MakeUsingCtor<T>(IConfigSectionNode node, string typePattern = null)
        {
           string tpn = CoreConsts.UNKNOWN;
           try
           {
              if (node==null || !node.Exists)
                throw new ConfigException(StringConsts.ARGUMENT_ERROR+"FactoryUtils.Make(node==null|empty)");

              tpn = node.AttrByName(CONFIG_TYPE_ATTR).Value;

              if (tpn.IsNullOrWhiteSpace())
                tpn = typeof(T).AssemblyQualifiedName;
              else
                if (typePattern.IsNotNullOrWhiteSpace())
                  tpn = typePattern.Replace("*", tpn);

              var tp = Type.GetType(tpn, true);

              var args = new List<object>();
              for(var i=0; true; i++)
              {
                var attr = node.AttrByName("arg{0}".Args(i));
                if (!attr.Exists) break;
                args.Add(attr.Value);
              }

              var cinfo = tp.GetConstructors().FirstOrDefault(ci => ci.GetParameters().Length == args.Count);
              if (cinfo==null) throw new NFXException(".ctor arg count mismatch");

              //dynamically re-cast argument types
              for(var i=0; i<args.Count; i++)
                args[i] = args[i].ToString().AsType(cinfo.GetParameters()[i].ParameterType);

              try
              {
                return (T)Activator.CreateInstance(tp, args.ToArray());
              }
              catch(TargetInvocationException tie)
              {
                throw tie.InnerException;
              }
           }
           catch(Exception error)
           {
              throw new ConfigException(StringConsts.CONFIGURATION_MAKE_USING_CTOR_ERROR.Args(tpn, error.ToMessageWithType()), error);
           }
        }


    #endregion


    #region .pvt

        private static T make<T>(string tName, Type defaultType, object[] args)
        {
          T result;

          Type t;

          if (string.IsNullOrWhiteSpace(tName))
          {
              if (defaultType == null)
                  throw new ConfigException(StringConsts.CONFIGURATION_TYPE_NOT_SUPPLIED_ERROR);
              tName = defaultType.FullName;
              t = defaultType;
          }
          else
          {
              t = Type.GetType(tName);
              if (t == null)
                throw new ConfigException(string.Format(StringConsts.CONFIGURATION_TYPE_RESOLVE_ERROR, tName));
          }

          if (!typeof(T).IsAssignableFrom(t))
              throw new ConfigException(string.Format(StringConsts.CONFIGURATION_TYPE_ASSIGNABILITY_ERROR, tName, typeof(T).FullName));

          try
          {
            if (args != null)
              result = (T)Activator.CreateInstance(
                        t,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                        null, args, null, null);
            else
              result = (T)Activator.CreateInstance(t, true);
          }
          catch(TargetInvocationException tie)
          {
            throw tie.InnerException;
          }


          return result;
        }

    #endregion


  }
}
