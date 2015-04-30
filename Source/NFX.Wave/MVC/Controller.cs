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
  /// Represents controller that all MVC controllers must inherit from 
  /// </summary>
  public abstract class Controller : DisposableObject
  {
      #region CONSTS
      
         public const string DEFAULT_VAR_MVC_ACTION = "mvc-action";
         public const string DEFAULT_MVC_ACTION = "index";

      #endregion

      [NonSerialized]
      internal WorkContext m_WorkContext;

      public virtual string ActionVarName{ get{ return DEFAULT_VAR_MVC_ACTION; }}
      public virtual string DefaultActionName{ get{ return DEFAULT_MVC_ACTION; }}

      /// <summary>
      /// Returns current work context
      /// </summary>
      public WorkContext WorkContext { get{ return m_WorkContext;}}

      /// <summary>
      /// Override to perform custom action name + params -> MethodInfo, param array resolution.
      /// This method rarely needs to be overridden because the framework does the resolution that suites most cases
      /// </summary>
      protected internal virtual MethodInfo FindMatchingAction(WorkContext work, string action, out object[] args)
      {
        args = null;
        return null;
      }


         private static Dictionary<MethodInfo, ActionFilterAttribute[]> m_Filters;


         /// <summary>
         /// Returns cached (for performance)ordered array of ActionFilterAttributes 
         /// </summary>
         protected static ActionFilterAttribute[] GetActionFilters(MethodInfo mi)
         {
            var dict = m_Filters;//thread safe copy
            ActionFilterAttribute[] actions = null;

            if (dict!=null && dict.TryGetValue(mi, out actions)) return actions;

            actions = mi.GetCustomAttributes().Where(atr => typeof(ActionFilterAttribute).IsAssignableFrom(atr.GetType()))
                                                              .Cast<ActionFilterAttribute>()
                                                              .OrderBy(a => a.Order)
                                                              .ToArray();

            var newDict = dict!=null ?  new Dictionary<MethodInfo, ActionFilterAttribute[]>( dict ) : new Dictionary<MethodInfo, ActionFilterAttribute[]>();
            newDict[mi] = actions;

            m_Filters = newDict; //thread safe swap

            return actions;
         }



      /// <summary>
      /// Override to add logic/filtering right before the invocation of action method.
      /// Return TRUE to indicate that request has already been handled and no need to call method body and AfterActionInvocation in which case
      ///  return result via 'out result' paremeter.
      ///  The default implementation calls ActionFilters 
      /// </summary>
      protected internal virtual bool BeforeActionInvocation(WorkContext work, string action, MethodInfo method, object[] args, ref object result)
      {
          var filters = GetActionFilters(method);
          if (filters!=null)
           for(var i=0; i<filters.Length; i++)
            if (filters[i].BeforeActionInvocation(this, work, action, method, args, ref result)) return true;
          
          result = null;
          return false;
      }
      
      /// <summary>
      /// Override to add logic/filtering right after the invocation of action method. Must return the result (which can be altered/filtered).
      /// The default implementation calls ActionFilters
      /// </summary>
      protected internal virtual object AfterActionInvocation(WorkContext work, string action, MethodInfo method, object[] args, object result)
      {
         var filters = GetActionFilters(method);
          if (filters!=null)
           for(var i=filters.Length-1; i>=0; i--)
            if (filters[i].AfterActionInvocation(this, work, action, method, args, ref result)) return result;

         return result;
      }

  }
}
