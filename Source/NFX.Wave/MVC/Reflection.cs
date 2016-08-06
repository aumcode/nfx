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

namespace NFX.Wave.MVC
{
    /// <summary>
    /// Provides reflection information about controller type.
    /// This is a framework internal method which is not intended to be used by business logic developers
    /// </summary>
    public sealed class ControllerInfo : INamed
    {
      public static string TypeToKeyName(Type type)
      {
        return type.AssemblyQualifiedName;
      }

      public static string GetInvocationName(MethodInfo mi)
      {
        var result = mi.Name;
        var attr = mi.GetCustomAttribute(typeof(ActionAttribute), false) as ActionAttribute;
        if (attr!=null)
          if (attr.Name.IsNotNullOrWhiteSpace()) result = attr.Name;

        return result;
      }

      public ControllerInfo(Type type)
      {
        string aname = null;
        try
        {
          Type = type;
          m_Name = TypeToKeyName(type);
          var groups = new Registry<ActionGroupInfo>();
          Groups = groups;

          var allmi = GetAllActionMethods();

          foreach(var mi in allmi)
          {
            var iname = GetInvocationName(mi);
            aname = iname;
            var agi = groups[iname];
            if (agi==null)
            {
              agi = new ActionGroupInfo(this, iname);
              groups.Register(agi);
            }
            aname = null;
          }
        }
        catch(Exception error)
        {
          throw new WaveException(StringConsts.MVC_CONTROLLER_REFLECTION_ERROR.Args(type.FullName, aname, error.ToMessageWithType()), error);
        }
      }

      private string m_Name;

      public readonly Type Type;
      public readonly IRegistry<ActionGroupInfo> Groups = new Registry<ActionGroupInfo>();
      public string Name { get{return m_Name;} }

      internal IEnumerable<MethodInfo> GetAllActionMethods()
      {
        return Type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                   .Where(mi => Attribute.IsDefined(mi, typeof(ActionAttribute), false) &&
                                !mi.ContainsGenericParameters &&
                                !mi.GetParameters().Any(mp=>mp.IsOut || mp.ParameterType.IsByRef)
                                 );
      }
    }


    /// <summary>
    /// Provides reflection information about a group of action methods which all share the same action name(invocation name) within controller type.
    /// Invocation names are mapped to actual method names, as ActionAttribute may override the name of actual method that it decorates.
    /// This is a framework internal method which is not intended to be used by business logic developers
    /// </summary>
    public sealed class ActionGroupInfo : INamed
    {
      internal ActionGroupInfo(ControllerInfo controller, string actionInvocationName)
      {
        Controller = controller;
        m_Name = actionInvocationName;

        var allmi = controller.GetAllActionMethods()
                              .Where(mi => ControllerInfo.GetInvocationName(mi).Equals(actionInvocationName, StringComparison.InvariantCultureIgnoreCase));

        var actions = new List<ActionInfo>();

        foreach(var mi in allmi)
          actions.Add(new ActionInfo(this, mi));

        Actions = actions.OrderBy( ai => ai.Attribute.Order );

        //warm-up for possible errors
        foreach(var ai in actions)
        {
          var matches = ai.Attribute.Matches;//cause matches script to load, and bubble exceptions if it contains any
        }

      }

      private string m_Name;

      /// <summary>
      /// Action invocation name- may be diffrent from method name
      /// </summary>
      public string Name { get{return m_Name;} }

      public readonly ControllerInfo Controller;

      /// <summary>
      /// Returns the actions in the order suitable for match making
      /// </summary>
      public IEnumerable<ActionInfo> Actions;
    }


    /// <summary>
    /// Provides reflection information about a particular action method of a controller type.
    /// This is a framework internal method which is not intended to be used by business logic developers
    /// </summary>
    public sealed class ActionInfo
    {
      internal ActionInfo(ActionGroupInfo group, MethodInfo method)
      {
        Group = group;
        Method = method;
        Attribute = method.GetCustomAttribute(typeof(ActionAttribute), false) as ActionAttribute;
      }

      public readonly ActionGroupInfo Group;
      public readonly MethodInfo      Method;
      public readonly ActionAttribute Attribute;
    }
}
