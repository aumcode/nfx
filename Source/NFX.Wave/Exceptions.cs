/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using System.Runtime.Serialization;
using System.Collections.Generic;

using NFX.Web;

namespace NFX.Wave
{
  /// <summary>
  /// Base exception thrown by the WAVE framework
  /// </summary>
  [Serializable]
  public class WaveException : NFXException
  {
    public WaveException() { }
    public WaveException(string message) : base(message) { }
    public WaveException(string message, Exception inner) : base(message, inner) { }
    protected WaveException(SerializationInfo info, StreamingContext context): base(info, context) { }
  }

  /// <summary>
  /// Wraps inner exceptions capturing stack trace in inner implementing blocks
  /// </summary>
  [Serializable]
  public class MVCActionException : WaveException
  {
    public const string CONTROLLER_FLD_NAME = "MVCAE-C";
    public const string ACTION_FLD_NAME = "MVCAE-A";

    public static MVCActionException WrapActionBodyError(string controller, string action, Exception src)
    {
      if (src==null) throw new WaveException(StringConsts.ARGUMENT_ERROR+typeof(MVCActionException).Name+"Wrap(src=null)");

      var trace = src.StackTrace;
      return new MVCActionException(controller,
                                    action,
                                    "Controller action body: '{0}'.'{1}'. Exception: {2} Trace: \r\n {3}".Args(controller, action, src.ToMessageWithType(), trace),
                                    src);
    }

    public static MVCActionException WrapActionResultError(string controller, string action, object result, Exception src)
    {
      if (src==null) throw new WaveException(StringConsts.ARGUMENT_ERROR+typeof(MVCActionException).Name+"Wrap(src=null)");

      var trace = src.StackTrace;
      return new MVCActionException(controller,
                                    action,
                                    "Controller action result processing: '{0}'.'{1}' -> {2}. Exception: {3} Trace: \r\n {4}".Args(controller,
                                                                                                                                 action,
                                                                                                                                 result==null ? "<null>" : result.GetType().FullName,
                                                                                                                                 src.ToMessageWithType(),
                                                                                                                                 trace),
                                    src);
    }

    protected MVCActionException(string controller, string action, string msg, Exception inner): base(msg, inner)
    {
      Controller = controller;
      Action = action;
    }

    protected MVCActionException(SerializationInfo info, StreamingContext context): base(info, context)
    {
      Controller = info.GetString(CONTROLLER_FLD_NAME);
      Action = info.GetString(ACTION_FLD_NAME);
    }

    public readonly string Controller;
    public readonly string Action;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(CONTROLLER_FLD_NAME, Controller);
      info.AddValue(ACTION_FLD_NAME, Action);
      base.GetObjectData(info, context);
    }
  }

  /// <summary>
  /// Wraps WAVE template rendering execptions
  /// </summary>
  [Serializable]
  public class WaveTemplateRenderingException : WaveException
  {
    public WaveTemplateRenderingException(string message, Exception inner): base(message, inner) { }
    protected WaveTemplateRenderingException(SerializationInfo info, StreamingContext context): base(info, context) { }
  }

  /// <summary>
  /// Thrown by filter pipeline
  /// </summary>
  [Serializable]
  public class FilterPipelineException : WaveException
  {
    public const string FILTER_TYPE_FLD_NAME = "FPE-FT";
    public const string FILTER_NAME_FLD_NAME = "FPE-FN";
    public const string FILTER_ORDER_FLD_NAME = "FPE-FO";
    public const string HANDLER_TYPE_FLD_NAME = "FPE-HT";
    public const string HANDLER_NAME_FLD_NAME = "FPE-HN";

    public FilterPipelineException(WorkFilter filter, Exception inner) : base(inner.Message, inner)
    {
      FilterType = filter.GetType();
      FilterName = filter.Name;
      FilterOrder = filter.Order;
      if (filter.Handler != null)
      {
        HandlerType = filter.Handler.GetType();
        HandlerName = filter.Handler.Name;
      }
    }

    protected FilterPipelineException(SerializationInfo info, StreamingContext context): base(info, context)
    {
      FilterType = (Type)info.GetValue(FILTER_TYPE_FLD_NAME, typeof(Type));
      FilterName = info.GetString(FILTER_NAME_FLD_NAME);
      FilterOrder = info.GetInt32(FILTER_ORDER_FLD_NAME);
      HandlerType = (Type)info.GetValue(HANDLER_TYPE_FLD_NAME, typeof(Type));
      HandlerName = info.GetString(HANDLER_NAME_FLD_NAME);
    }

    public readonly Type FilterType;
    public readonly string FilterName;
    public readonly int FilterOrder;
    public readonly Type HandlerType;
    public readonly string HandlerName;

    /// <summary>
    /// Returns a mnemonic filter sequence where the root exception originated from
    /// </summary>
    public string FilterPath
    {
      get
      {
         var result = "> ";

         Exception error = this;//.InnerException;
         while(error is FilterPipelineException)
         {
            result += "{0} > ".Args(((FilterPipelineException)error).FilterName);
            error = error.InnerException;
         }

         return result;
      }
    }

    /// <summary>
    /// Returns unwound root exception - unwrapping it from FilterPipelineException
    /// </summary>
    public Exception RootException
    {
      get
      {
         if (InnerException is FilterPipelineException)
           return ((FilterPipelineException)InnerException).RootException;
         else
           return InnerException;
      }
    }

    public override string Message
    {
      get
      {
        return "{0} {1}".Args(FilterPath, RootException!=null? RootException.ToMessageWithType() : SysConsts.NULL_STRING);
      }
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(FILTER_TYPE_FLD_NAME, FilterType);
      info.AddValue(FILTER_NAME_FLD_NAME, FilterName);
      info.AddValue(FILTER_ORDER_FLD_NAME, FilterOrder);
      info.AddValue(HANDLER_TYPE_FLD_NAME, HandlerType);
      info.AddValue(HANDLER_NAME_FLD_NAME, HandlerName);
      base.GetObjectData(info, context);
    }
  }

  /// <summary>
  /// Thrown to indicate various Http status conditions
  /// </summary>
  [Serializable]
  public class HTTPStatusException : WaveException
  {
    public const string STATUS_CODE_FLD_NAME = "HTTPSE-SC";
    public const string STATUS_DESCRIPTION_FLD_NAME = "HTTPSE-SD";

    public static HTTPStatusException BadRequest_400(string descr = null)
    {
      var d = WebConsts.STATUS_400_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": "+descr);

      return new HTTPStatusException(WebConsts.STATUS_400, d);
    }

    public static HTTPStatusException Unauthorized_401(string descr = null)
    {
      var d = WebConsts.STATUS_401_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": "+descr);

      return new HTTPStatusException(WebConsts.STATUS_401, d);
    }

    public static HTTPStatusException Forbidden_403(string descr = null)
    {
      var d = WebConsts.STATUS_403_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": "+descr);

      return new HTTPStatusException(WebConsts.STATUS_403, d);
    }

    public static HTTPStatusException NotFound_404(string descr = null)
    {
      var d = WebConsts.STATUS_404_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": "+descr);

      return new HTTPStatusException(WebConsts.STATUS_404, d);
    }

    public static HTTPStatusException MethodNotAllowed_405(string descr = null)
    {
      var d = WebConsts.STATUS_405_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": "+descr);

      return new HTTPStatusException(WebConsts.STATUS_405, d);
    }

    public static HTTPStatusException NotAcceptable_406(string descr = null)
    {
      var d = WebConsts.STATUS_406_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": "+descr);

      return new HTTPStatusException(WebConsts.STATUS_406, d);
    }

    public static HTTPStatusException TooManyRequests_429(string descr = null)
    {
      var d = WebConsts.STATUS_429_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": "+descr);

      return new HTTPStatusException(WebConsts.STATUS_429, d);
    }

    public static HTTPStatusException InternalError_500(string descr = null)
    {
      var d = WebConsts.STATUS_500_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": "+descr);

      return new HTTPStatusException(WebConsts.STATUS_500, d);
    }

    public HTTPStatusException(int statusCode, string statusDescription) : base("{0} - {1}".Args(statusCode, statusDescription))
    {
      StatusCode = statusCode;
      StatusDescription = statusDescription;
    }

    public HTTPStatusException(int statusCode, string statusDescription, string message) : base("{0} - {1} : {2}".Args(statusCode, statusDescription, message))
    {
      StatusCode = statusCode;
      StatusDescription = statusDescription;
    }

    public HTTPStatusException(int statusCode, string statusDescription, string message, Exception inner) : base("{0} - {1} : {2}".Args(statusCode, statusDescription, message), inner)
    {
      StatusCode = statusCode;
      StatusDescription = statusDescription;
    }

    protected HTTPStatusException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
      StatusCode = info.GetInt32(STATUS_CODE_FLD_NAME);
      StatusDescription = info.GetString(STATUS_DESCRIPTION_FLD_NAME);
    }

    /// <summary>
    /// Http status code
    /// </summary>
    public readonly int StatusCode;

    /// <summary>
    /// Http status description
    /// </summary>
    public readonly string StatusDescription;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(STATUS_CODE_FLD_NAME, StatusCode);
      info.AddValue(STATUS_DESCRIPTION_FLD_NAME, StatusDescription);
      base.GetObjectData(info, context);
    }
  }

  /// <summary>
  /// Provides various extension methods
  /// </summary>
  public static class ExceptionExtensions
  {
     /// <summary>
     /// Describes exception for client response transmission as JSONDataMap
     /// </summary>
     public static NFX.Serialization.JSON.JSONDataMap ToClientResponseJSONMap(this Exception error, bool detailed)
     {
       var actual = error;
       if (actual is FilterPipelineException)
         actual = ((FilterPipelineException)actual).RootException;

        var result = new NFX.Serialization.JSON.JSONDataMap();
        result["OK"] = false;
        result["HttpStatusCode"] = (actual is HTTPStatusException) ? ((HTTPStatusException)actual).StatusCode : -1;
        result["HttpStatusDescription"] = (actual is HTTPStatusException) ? ((HTTPStatusException)actual).StatusDescription : string.Empty;

        if (detailed)
        {
          result["Error"] = error.Message;
          result["Type"] = error.GetType().FullName;
          result["StackTrace"] = error.StackTrace;
          if (error.InnerException!=null)
            result["Inner"] = error.InnerException.ToClientResponseJSONMap(detailed);
        }
        else
        {
          result["Error"] = actual.GetType().Name;
        }

        result["IsAuthorization"] = actual is NFX.Security.AuthorizationException || actual.InnerException is NFX.Security.AuthorizationException;

       return result;
    }
  }
}