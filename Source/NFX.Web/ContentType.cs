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
    /// Declares web-related/mime content types
    /// </summary>
    public static class ContentType
    {
       public const string TEXT = "text/plain";
       public const string HTML = "text/html";
       public const string CSS = "text/css";
       public const string JS = "application/x-javascript";


       public const string XML_TEXT = "text/xml";
       public const string XML_APP = "application/xml";

       public const string PDF = "application/pdf";
       public const string BINARY = "application/octet-stream";
       public const string EXE = BINARY;

       public const string GIF = "image/gif";
       public const string JPEG = "image/jpeg";
       public const string PNG = "image/png";
       public const string ICO = "image/x-icon";

       public const string JSON = "application/json";


       public const string TTF  = "application/font-sfnt";
       public const string EOT  = "application/vnd.ms-fontobject";
       public const string OTF  = "application/font-sfnt";
       public const string WOFF = "application/font-woff";




       public const string FORM_URL_ENCODED = "application/x-www-form-urlencoded";

       public const string FORM_MULTIPART_ENCODED = "multipart/form-data";


       public static string ExtensionToContentType(string ext, string dflt = ContentType.HTML)
       {
         //todo Make this list configurable and use dictionary of default values instead of switch/case which is slower than dict lookup
          if (ext==null) return dflt;

          ext = ext.ToLowerInvariant().Trim();

          if (ext.StartsWith(".")) ext = ext.Remove(0,1);

              switch(ext)
              {
                case "htm":
                case "html": return ContentType.HTML;

                case "json": return ContentType.JSON;

                case "xml": return ContentType.XML_TEXT;

                case "css": return ContentType.CSS;

                case "js": return ContentType.JS;

                case "gif": return ContentType.GIF;

                case "jpe":
                case "jpg":
                case "jpeg": return ContentType.JPEG;

                case "png": return ContentType.PNG;

                case "ico": return ContentType.ICO;

                case "pdf": return ContentType.PDF;

                case "exe": return ContentType.EXE;

                case "txt": return ContentType.TEXT;

                case "ttf": return ContentType.TTF;
                case "eot": return ContentType.EOT;
                case "otf": return ContentType.OTF;
                case "woff": return ContentType.WOFF;


                default: return dflt;
              }
       }
    }
}
