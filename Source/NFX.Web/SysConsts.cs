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

namespace NFX.Web
{
  /// <summary>
  /// Non-localizable constants
  /// </summary>
  public static class WebConsts
  {
    public const string HTTP_POST   = "POST";
    public const string HTTP_PUT    = "PUT";
    public const string HTTP_GET    = "GET";
    public const string HTTP_DELETE = "DELETE";
    public const string HTTP_PATCH  = "PATCH";


    public const string HTTP_HDR_AUTHORIZATION = "Authorization";
    public const string HTTP_HDR_ACCEPT = "Accept";
    public const string HTTP_HDR_CONTENT_DISPOSITION = "Content-disposition";
    public const string HTTP_SET_COOKIE = "Set-Cookie";
    public const string HTTP_WWW_AUTHENTICATE = "WWW-Authenticate";


    public const int STATUS_200 = 200;  public const string STATUS_200_DESCRIPTION = "OK";
    public const int STATUS_404 = 404;  public const string STATUS_404_DESCRIPTION = "Not found";
    public const int STATUS_403 = 403;  public const string STATUS_403_DESCRIPTION = "Forbidden";


    public const int STATUS_400 = 400;  public const string STATUS_400_DESCRIPTION = "Bad Request";

    public const int STATUS_401 = 401;  public const string STATUS_401_DESCRIPTION = "Unauthorized";

    public const int STATUS_405 = 405;  public const string STATUS_405_DESCRIPTION = "Method Not Allowed";
    public const int STATUS_406 = 406;  public const string STATUS_406_DESCRIPTION = "Not Acceptable";

    public const int STATUS_429 = 429;  public const string STATUS_429_DESCRIPTION = "Too Many Requests";

    public const int STATUS_500 = 500;  public const string STATUS_500_DESCRIPTION = "Internal error";
  }
}
