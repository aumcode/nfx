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
    /// Reports XML health check results
    /// </summary>
    public class XMLReporter : Reporter
    {

        public XMLReporter(CheckList list) : base(list)
        {

        }


        public override void Report(System.IO.TextWriter writer)
        {
            var conf = new XMLConfiguration();
            conf.StrictNames = false;

            conf.Create();
            conf.Root.Name = "health-check-list";

            conf.Root.AddAttributeNode("successful", CheckList.Successful);

            var runNode = conf.Root.AddChildNode("run", null);
            if (CheckList.Status == CheckListStatus.Run)
            {
                runNode.AddAttributeNode("started", CheckList.RunStart);
                runNode.AddAttributeNode("finished", CheckList.RunFinish);
                runNode.AddAttributeNode("duration", CheckList.RunFinish - CheckList.RunStart);
            }
            else
                runNode.AddAttributeNode("started", "never ran");

            var checksNode = conf.Root.AddChildNode("checks", null);

            foreach(var check in CheckList.Checks)
             reportCheck(checksNode, check);



            writer.Write(conf.ToString());
        }


                private void reportCheck(ConfigSectionNode parent, BaseCheck check)
                {
                  var node = parent.AddChildNode("check", null);
                  node.AddAttributeNode("name", check.Name);
                  node.AddAttributeNode("description", check.Description);
                  node.AddAttributeNode("skipped", check.Result.Skipped);
                  node.AddAttributeNode("successful", check.Result.Successful);
                  node.AddAttributeNode("error", check.Result.Exception!=null? check.Result.Exception.ToString() : string.Empty);
                  var keys = node.AddChildNode("results", null);

                  foreach(var kv in check.Result)
                    keys.AddChildNode(kv.Key, kv.Value);
                }



    }
}
