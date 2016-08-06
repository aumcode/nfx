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

namespace NFX.Wave
{
  /// <summary>
  /// A dictionary of framework text messages.
  /// Localization may be done in this class in future
  /// </summary>
  internal static class StringConsts
  {
      public const string ARGUMENT_ERROR =
        "Error in call arguments: ";

      public const string DISPATCHER_NOT_THIS_SERVER_ERROR =
        "Error assigning WaveServer.Dispatcher. The supplied dispatcher instance was not created to be directed by this server instance";

      public const string SERVER_NO_PREFIXES_ERROR =
        "WaveServer '{0}' can not start as there are no prefixes defined";

      public const string SERVER_COULD_NOT_GET_REGISTERED_ERROR =
        "WaveServer '{0}' can not start as there is already a global instance with such name";

      public const string WRONG_DISPATCHER_FILTER_REGISTRATION_ERROR =
        "Can not register a filter '{0}' because it does not belong to this dispatcher";

      public const string WRONG_DISPATCHER_FILTER_UNREGISTRATION_ERROR =
        "Can not unregister a filter '{0}' because it does not belong to this dispatcher";

      public const string WRONG_DISPATCHER_HANDLER_REGISTRATION_ERROR =
        "Can not register a handler '{0}' because it does not belong to this dispatcher";

      public const string WRONG_DISPATCHER_HANDLER_UNREGISTRATION_ERROR =
        "Can not unregister a handler '{0}' because it does not belong to this dispatcher";

      public const string NO_HANDLER_FOR_WORK_ERROR =
        "No suitable handler for work '{0}' could be matched";

      public const string URI_PATTERN_PARSE_ERROR =
        "Uri pattern '{0}' could not be parsed: {1}";

      public const string URI_WILDCARD_PARSE_ERROR =
        "Pattern may contain only one {{*var_name}} wildcard capture variable as the very last pattern segment";

      public const string RESPONSE_WAS_WRITTEN_TO_ERROR =
        "Response object was already written to and can not perform operation: ";

      public const string RESPONSE_CANCEL_NON_BUFFERED_ERROR =
        "Response can not be canceled because it was already written to and Buffered=false";

      public const string RESPONSE_WRITE_FILE_DOES_NOT_EXIST_ERROR =
        "Can not write non-existing local file '{0}' to response";

      public const string RESPONSE_WRITE_FILE_OVER_MAX_SIZE_ERROR =
        "Can not write local file '{0}' to response as it exceeds the maximum size limit of {1} bytes and Buffered = true.\n"+
        "If client allows, set Buffered=false to support chunked response encoding";


      public const string FILE_DL_HANDLER_NOT_FOUND_INFO =
        "File not found '{0}'";

      public const string WORK_NO_DEFAULT_AUTO_CLOSE_ERROR =
        "WorkContext.NoDefaultAutoClose may be set to true only when Response.Buffered=false (chunked transfer) and Response has been written to";

      public const string DONT_KNOW_ACTION_ERROR =
        "Dont know how to handle site action: ";

      public const string NOT_FOUND_ERROR =
        "404 Not Found: ";

      public const string ERROR_PAGE_TEMPLATE_TYPE_ERROR =
        "Custom error page could not be created. Supplied type '{0}'. Error: {1}";

      public const string CONFIG_DUPLICATE_HANDLER_NAME_ERROR =
        "Handler '{0}' is specified more than once in config";

      public const string CONFIG_DUPLICATE_FILTER_NAME_ERROR =
        "Filter '{0}' is specified more than once in config";

      public const string CONFIG_HANDLER_DUPLICATE_MATCH_NAME_ERROR =
        "Match '{0}' is specified more than once in handler config";

     public const string CONFIG_HANDLER_DUPLICATE_FILTER_NAME_ERROR =
        "Filter '{0}' is specified more than once in handler config";

      public const string CONFIG_OTHER_DUPLICATE_MATCH_NAME_ERROR =
        "Match '{0}' is specified more than once in {1} config";

      public const string CONFIG_OTHER_DUPLICATE_PORTAL_NAME_ERROR =
        "Portal '{0}' is specified more than once in {1} config";

      public const string SERVER_DEFAULT_ERROR_WC_NULL_ERROR =
        "Server exception could not be responded to as WorkContext is null. Error: ";

      public const string SERVER_DEFAULT_ERROR_HANDLER_ERROR =
        "Server default exception handler threw error: ";

      public const string SESSION_NOT_AVAILABLE_ERROR =
        "Session context is not available for '{0}'. No SessionFilter (or derivative) injected in the processing chain";

      public const string MVCCONTROLLER_ACTION_UNKNOWN_ERROR =
         "MVC Controller '{0}' does not have action '{1}'";

      public const string MVCCONTROLLER_ACTION_PARAM_BINDER_ERROR =
         "MVC Controller '{0}' {1} action '{2}' parameter '{3}' of type '{4}' could not be assigned a value of '{5}'";

      public const string MVCCONTROLLER_ACTION_UNMATCHED_HANDLER_ERROR =
         "MVC Controller '{0}' could not match any action '{1}' handler";


      public const string MVC_ACTION_ATTR_MATCH_PARSING_ERROR =
        "Action attribute matches parsing error. Script '{0}'. Error: {1}";

      public const string MVC_CONTROLLER_REFLECTION_ERROR =
        "Error reflecting on MVC controller type '{0}' action '{1}'. Error: {2}";

      public const string CLIENT_VARS_LENGTH_OVER_LIMIT_ERROR =
        "The total length of client vars with cookie name is {0} chars, which is over the limit of {1} chars. Reduce the amount of information stored in client vars";


      public const string CONFIG_PORTAL_ROOT_URI_ERROR =
        "Portal '{0}' has invalid root uri attribute: {1}";

      public const string CONFIG_PORTAL_DUPLICATE_THEME_NAME_ERROR =
        "Theme '{0}' is specified more than once in portal '{1}' config";

      public const string CONFIG_PORTAL_NO_THEMES_ERROR =  "Portal '{0}' does not have any themes specified";
      public const string CONFIG_PORTAL_NO_DEFAULT_THEME_ERROR =  "Portal '{0}' does not have any theme marked as DEFAULT. At least one theme should be a default one";
      public const string CONFIG_PORTAL_THEME_NO_NAME_ERROR =  "Portal '{0}' configures a theme that has no name defined";


      public const string PORTAL_HUB_INSTANCE_IS_NOT_AVAILABLE_ERROR = "Portal hub instance is not available";
      public const string PORTAL_HUB_INSTANCE_IS_ALREADY_AVAILABLE_ERROR = "Portal hub instance is already available";

      public const string CONFIG_PORTAL_HUB_NODE_ERROR = "Portal hub config node is null or !exists";

      public const string CONFIG_PORTAL_HUB_FS_ROOT_PATH_ERROR = "Portal hub content file system '{0}' section root path attribute '{1}' is required";

      public const string PORTAL_HUB_INSTANCE_ALREADY_CONTAINS_PORTAL_ERROR = "Portal hub instance already has portal '{0}' registered";

      public const string PORTAL_PARENT_DEPTH_ERROR = "Portal '{0}' point to parent portal '{1}' which exceeds max depths of {2}";

      public const string PORTAL_PARENT_INVALID_ERROR = "Portal '{0}' point to parent portal '{1}' which does not exist";

      public const string CONFIG_PORTAL_LOCALIZATION_FILE_ERROR =  "Portal '{0}' points to localization message file '{1}' which could not be read: {2}";
  }
}
