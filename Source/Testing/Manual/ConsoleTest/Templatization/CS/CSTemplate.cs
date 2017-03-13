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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using NFX;
using NFX.Templatization;
using NFX.ApplicationModel;

namespace ConsoleTest.Templatization.CS
{
  class CSTemplate
  {
    public static void Run()
    {
      var tplSource = typeof(CSTemplate).GetText("Template2.htm");

      var cmp = new TextCSTemplateCompiler( new TemplateStringContentSource(tplSource));


      cmp.AssemblyFileName = null;
      cmp.CompileCode = true;
      cmp.Compile();


      var unit = cmp.First();

      Console.Write( unit.CompiledSource );
    //  var template = Activator.CreateInstance(unit.CompiledTemplateType, ServiceBaseApplication.Instance, null) as SimpleTemplate;

    //  var target = new StringRenderingTarget();
    //  template.Render(target, null);

    //  Console.WriteLine(target.Value);
    }

  }
}
