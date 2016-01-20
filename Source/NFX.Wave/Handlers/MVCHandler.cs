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
using System.Drawing;

using NFX.Web;
using NFX.Environment;
using NFX.DataAccess.CRUD;
using NFX.Serialization.JSON;
using NFX.Wave.MVC;
using NFX.Wave.Templatization;


namespace NFX.Wave.Handlers
{
  /// <summary>
  /// Handles MVC-related requests
  /// </summary>
  public class MVCHandler : TypeLookupHandler<Controller>
  {
    #region .ctor
         protected MVCHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match) : base(dispatcher, name, order, match)
         {

         }

         protected MVCHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode)
         {

         }
    #endregion
    

    #region Fields

       private Registry<ControllerInfo> m_Controllers = new Registry<ControllerInfo>();

    #endregion

    #region Protected
      protected override void DoTargetWork(Controller target, WorkContext work)
      {
          var action = GetActionName(target, work);

          object[] args;

          //1. try controller instance to resolve action
          var mi = target.FindMatchingAction(work, action, out args);

          //2. if controller did not resolve the resolve by framework (most probable case)
          if (mi==null)
           mi = FindMatchingAction(target, work, action, out args);
         
          
          if (mi==null)
            throw new HTTPStatusException(SysConsts.STATUS_404, 
                                          SysConsts.STATUS_404_DESCRIPTION,
                                          StringConsts.MVCCONTROLLER_ACTION_UNMATCHED_HANDLER_ERROR.Args(target.GetType().FullName, action));
            
          Security.Permission.AuthorizeAndGuardAction(mi, work.Session, () => work.NeedsSession());

          object result = null;
          
          try
          {
            target.m_WorkContext = work;
            try
            {
              var handled = target.BeforeActionInvocation(work, action, mi, args, ref result);
              if (!handled)
              {
               result = mi.Invoke(target, args);
               result = target.AfterActionInvocation(work, action, mi, args, result);
              }
            }
            finally
            {
              
              target.m_WorkContext = null;
            }
          }
          catch(Exception error)
          {
            if (error is TargetInvocationException)
            {
              var cause = ((TargetInvocationException)error).InnerException;
              if (cause!=null) error = cause;
            }
            throw MVCActionException.Wrap(target.GetType().FullName, action, error);
          } 

          if (mi.ReturnType==typeof(void)) return;

          ProcessResult(target, work, result);
      }

      /// <summary>
      /// Gets name of MVC action from work and controller. Controller may override name of variable 
      /// </summary>
      protected virtual string GetActionName(Controller controller, WorkContext work)
      {
        var action = work.MatchedVars[controller.ActionVarName].AsString();
        if (action.IsNullOrWhiteSpace()) action = controller.DefaultActionName;
        return action;
      }

      /// <summary>
      /// Finds matching method that has the specified action name and best matches the supplied input
      /// </summary>
      protected virtual MethodInfo FindMatchingAction(Controller controller, WorkContext work, string action, out object[] args)
      {
        var tp = controller.GetType();

        var cInfo = m_Controllers[ControllerInfo.TypeToKeyName(tp)]; //Lock free lookup
        
        if (cInfo==null)
        {
          cInfo = new ControllerInfo(tp);
          m_Controllers.Register(cInfo);
        }


        var gInfo = cInfo.Groups[action];

        if (gInfo==null) //action unknown
        {
          throw new HTTPStatusException(SysConsts.STATUS_404, 
                                        SysConsts.STATUS_404_DESCRIPTION,
                                        StringConsts.MVCCONTROLLER_ACTION_UNKNOWN_ERROR.Args(tp.DisplayNameWithExpandedGenericArgs(), action));
        }

        foreach(var ai in gInfo.Actions)
          foreach(var match in ai.Attribute.Matches)
          {
            var matched = match.Make(work);
            if (matched!=null)
            {
              var attr = ai.Attribute;
              var result = ai.Method;

              BindParameters(controller, action, attr, result, work, out args);

              return result; 
            }
          }

        args = null;
        return null;
      }

      /// <summary>
      /// Fills method invocation param array with args doing some interpretation for widely used types like JSONDataMaps, Rows etc..
      /// </summary>
      protected virtual void BindParameters(Controller controller, string action, ActionAttribute attrAction, MethodInfo method,  WorkContext work, out object[] args)
      {
        var mpars = method.GetParameters();
        args = new object[mpars.Length];

        if (mpars.Length==0) return;

        var requested = GetRequestAsJSONDataMap(work);

        var strictParamBinding = attrAction.StrictParamBinding;

        //check for complex type
        for(var i=0; i<mpars.Length; i++)
        {
          var ctp = mpars[i].ParameterType;
          if (ctp==typeof(object) || ctp==typeof(JSONDataMap) || ctp==typeof(Dictionary<string, object>))
          {
            args[i] = requested;
            continue;  
          }
          if (typeof(TypedRow).IsAssignableFrom(ctp))
          {
            args[i] = JSONReader.ToRow(ctp, requested);
            continue;
          } 
        }
              
        for(var i=0; i<args.Length; i++)
        {
          var mp = mpars[i];
                
          var got = requested[mp.Name];

          if (got==null)
          {
            if (mp.HasDefaultValue) args[i] = mp.DefaultValue;
            continue;
          }
          
          var strVal = got.AsString();
          try
          {      
            args[i] = strVal.AsType(mp.ParameterType, strictParamBinding);
          }
          catch
          {
            const int MAX_LEN = 30;
            if (strVal.Length>MAX_LEN) strVal = strVal.Substring(0, MAX_LEN)+"...";
            throw new HTTPStatusException(SysConsts.STATUS_400, 
                                        SysConsts.STATUS_400_DESCRIPTION,
                                        StringConsts.MVCCONTROLLER_ACTION_PARAM_BINDER_ERROR
                                                    .Args(
                                                          controller.GetType().DisplayNameWithExpandedGenericArgs(),
                                                          strictParamBinding ? "strict" : "relaxed",
                                                          action,
                                                          mp.Name,
                                                          mp.ParameterType.DisplayNameWithExpandedGenericArgs(), strVal ));
          }
        }
      }

      /// <summary>
      /// Converts request into JSONDataMap
      /// </summary>
      protected JSONDataMap GetRequestAsJSONDataMap(WorkContext work)
      {
        if (!work.Request.HasEntityBody) return work.MatchedVars;

        JSONDataMap result = null;

        var ctp = work.Request.ContentType;
        
        //Multipart
        if (ctp.IndexOf(ContentType.FORM_MULTIPART_ENCODED)>=0)
          result = MultiPartContent.ToJSONDataMap(work.Request.InputStream, ctp,  work.Request.ContentEncoding);
        else //Form URL encoded
        if (ctp.IndexOf(ContentType.FORM_URL_ENCODED)>=0)
          result = JSONDataMap.FromURLEncodedStream(new NFX.IO.NonClosingStreamWrap(work.Request.InputStream),
                                                  work.Request.ContentEncoding); 
        else//JSON
        if (ctp.IndexOf(ContentType.JSON)>=0)
          result = JSONReader.DeserializeDataObject(new NFX.IO.NonClosingStreamWrap(work.Request.InputStream),
                                                  work.Request.ContentEncoding) as JSONDataMap;

        return result==null ? work.MatchedVars : result.Append(work.MatchedVars);
      }


      /// <summary>
      /// Turns result object into appropriate response
      /// </summary>
      protected virtual void ProcessResult(Controller controller, WorkContext work, object result)
      {
        if (result==null) return;
        if (result is string)
        {
          work.Response.ContentType = ContentType.TEXT;
          work.Response.Write(result);
          return;
        }

        if (result is WaveTemplate)
        {
          ((WaveTemplate)result).Render(work, null);
          return;
        }

        if (result is Image)
        {
          var img = (Image)result;
          work.Response.ContentType = ContentType.PNG;
          img.Save(work.Response.GetDirectOutputStreamForWriting(), System.Drawing.Imaging.ImageFormat.Png);
          return;
        }

        if (result is IActionResult)
        {
          ((IActionResult)result).Execute(controller, work);
          return;
        } 

        work.Response.WriteJSON(result, JSONWritingOptions.CompactRowsAsMap ); //default serialize object as JSON
      }


    #endregion

  }
}
