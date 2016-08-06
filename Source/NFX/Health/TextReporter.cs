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

using NFX.Environment;

namespace NFX.Health
{
    /// <summary>
    /// Reports health check results in an plain text format
    /// </summary>
    public class TextReporter : Reporter
    {

        public TextReporter(CheckList list) : base(list)
        {

        }


        public override void Report(System.IO.TextWriter writer)
        {
            writer.WriteLine("Health Check List");
            writer.WriteLine("-----------------");

            writer.WriteLine("successful = {0}", CheckList.Successful);


            if (CheckList.Status == CheckListStatus.Run)
            {
                writer.WriteLine("started = {0}", CheckList.RunStart);
                writer.WriteLine("finished = {0}", CheckList.RunFinish);
                writer.WriteLine("duration = {0}", CheckList.RunFinish - CheckList.RunStart);
            }
            else
                writer.WriteLine("started = {0}", "never ran");

            writer.WriteLine("--------------------------------------------------------------");

            foreach(var check in CheckList.Checks)
             reportCheck(writer, check);

        }


        private void reportCheck(System.IO.TextWriter writer, BaseCheck check)
        {
          writer.WriteLine("* Check");
           writer.WriteLine("    name = {0}", check.Name);
           writer.WriteLine("    description = {0}", check.Description);
           writer.WriteLine("    skipped = {0}", check.Result.Skipped);
           writer.WriteLine("    successful = {0}", check.Result.Successful);
           writer.WriteLine("    error = {0}", check.Result.Exception!=null? check.Result.Exception.ToString() : string.Empty);
           writer.WriteLine("    results = {");
           foreach(var kv in check.Result)
                writer.WriteLine("                     [{0}] = [{1}]", kv.Key, kv.Value);
           writer.WriteLine("              }");
        }
    }
}
