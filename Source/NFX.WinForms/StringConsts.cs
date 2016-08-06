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

namespace NFX.WinForms
{
  internal static class StringConsts
  {
    public const string CONTROLLER_BINDING_CATEGORY = "Controller Binding";
    public const string INTERACTIONS_CATEGORY = "Interactions";
    public const string VIEW_CATEGORY = "View Properties";

     public const string ARGUMENT_ERROR = "Argument error: ";


    public const string SURROGATE_CREATE_ERROR = "Could not create record surrogate type \"{0}\" \n Exception: \n\n {1}";

    public const string THEME_CREATION_ERROR =
        "Theme of supplied type could not be created. Type: \"{0}\"";


    public const string END_UPDATE_MISMATCH_ERROR =
        "A call to EndUpdate() was mismatched";

    public const string ERROR_BALLOON_TEXT =
     "Error in field: \"{0}\" \n {1}";


   public const string APP_INSTANCE_NULL_ERROR =
         "Application instance has not been created";

    public const string APP_INSTANCE_ALREADY_CREATED_ERROR =
         "Application instance has already been created";

    public const string APP_LOG_INIT_ERROR =
          "Application log initApplication error: ";

    public const string APP_DATA_STORE_INIT_ERROR =
          "Application data store initApplication error: ";

    public const string APP_INSTRUMENTATION_INIT_ERROR =
          "Application instrumentation initApplication error: ";

    public const string APP_OBJECT_STORE_INIT_ERROR =
          "Application object store initApplication error: ";

    public const string APP_FORMS_FACTORY_INIT_ERROR =
          "Application forms factory initApplication error: ";


    public const string COLUMN_ID_EXISTS_GRID_ERROR =
          "Column already exists with ID: ";

    public const string COLUMN_ARE_REPOSITIONING_GRID_ERROR =
          "Column are repositioning and column list can not be changed now";

  }
}
