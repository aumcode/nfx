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

using E = System.Net.WebUtility;

using NFX.Environment;



namespace NFX.Health
{
    /// <summary>
    /// Reports health check results as HTML
    /// </summary>
    public class HTMLReporter : Reporter
    {

        public HTMLReporter(CheckList list) : base(list)
        {

        }


        public override void Report(System.IO.TextWriter writer)
        {
            writer.WriteLine("<html>");
            writer.WriteLine("<head>");
            writer.WriteLine("<title>Health Check List</title>");
            writer.WriteLine("<style>");
            writer.WriteLine("  body{ background-color: black; color: lime; font-family: Tahoma, Verdana; font-size: 9pt; }");
            writer.WriteLine("  table{ border: 1px dotted #407020; font-size: 8pt; }");
            writer.WriteLine("  td{ border: 1px dotted #508030; padding: 4px; }");
            writer.WriteLine("</style>");
            writer.WriteLine("</head>");

            writer.WriteLine("<body>");

            writer.WriteLine("<h1>Health Check List</h1>");

            writer.WriteLine("<table>");
            writer.WriteLine("<tr><td {0}>successful</td><td>{1}</td></tr>",
                                     CheckList.Successful?
                                        "style='background-color: lime; color: black'" :
                                        "style='background-color: red; color: white'"  ,
                                         CheckList.Successful);


            if (CheckList.Status == CheckListStatus.Run)
            {
                writer.WriteLine("<tr><td>started</td><td>{0}</td></tr>", CheckList.RunStart);
                writer.WriteLine("<tr><td>finished</td><td>{0}</td></tr>", CheckList.RunFinish);
                writer.WriteLine("<tr><td>duration</td><td>{0}</td></tr>", CheckList.RunFinish - CheckList.RunStart);
            }
            else
                writer.WriteLine("<tr><td>started</td><td>{0}</td></tr>", "never ran");

            writer.WriteLine("</table>");

            writer.WriteLine("<hr />");

            foreach(var check in CheckList.Checks)
             reportCheck(writer, check);


            writer.WriteLine("</body>");
            writer.WriteLine("</html>");
        }


        private void reportCheck(System.IO.TextWriter writer, BaseCheck check)
        {
          writer.WriteLine("<table>");
           writer.WriteLine("<tr><td>name</td><td>{0}</td></tr>", E.HtmlEncode(check.Name));
           writer.WriteLine("<tr><td>description</td><td>{0}</td></tr>", E.HtmlEncode(check.Description));

           if(check.Result.Skipped)
             writer.WriteLine("<tr><td style='background-color: yellow; color: black'>skipped</td><td>was not run (CanRun = false)</td></tr>");
           else
           {
               writer.WriteLine("<tr><td {0}>successful</td><td>{1}</td></tr>",
                                     check.Result.Successful?
                                        "style='background-color: lime; color: black'" :
                                        "style='background-color: red; color: white'"  ,
                                         check.Result.Successful);


               writer.WriteLine("<tr><td>error</td><td>{0}</td></tr>", check.Result.Exception!=null? E.HtmlEncode(check.Result.Exception.ToString()) : string.Empty);
               writer.WriteLine("<tr><td>results</td><td>");

               writer.WriteLine("<br /> <table>");

               foreach(var kv in check.Result)
                    writer.WriteLine("<tr><td>{0}</td><td>{1}</td></tr>", E.HtmlEncode(kv.Key), E.HtmlEncode(kv.Value.ToString()));

               writer.WriteLine(" </table>");

               writer.WriteLine(" </td></tr>");
           }

          writer.WriteLine("</table>");
          writer.WriteLine("<br />");
        }
    }
}
