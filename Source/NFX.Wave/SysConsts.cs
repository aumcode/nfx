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
  /// Non-localizable constants
  /// </summary>
  public static class SysConsts
  {
      /// <summary>
      /// Returns object {OK = true}
      /// </summary>
      public static readonly object JSON_RESULT_OK = new {OK = true};

      /// <summary>
      /// Returns object {OK = false}
      /// </summary>
      public static readonly object JSON_RESULT_ERROR = new {OK = false};


      public const string HEADER_API_VERSION = "wv-api-ver";
      public const string HEADER_API_SESSION = "wv-api-session";

      public const string WAVE_LOG_TOPIC = "WAVE";
      public const string NULL_STRING = "<null>";

      public const string UNSPECIFIED = "<unspecified>";

      public const string CONFIG_WAVE_SECTION = "wave";


  }
}
