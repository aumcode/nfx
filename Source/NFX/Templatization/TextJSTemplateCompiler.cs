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

using NFX.Environment;
using System.Text.RegularExpressions;

namespace NFX.Templatization
{
  public class TextJSTemplateCompiler : TemplateCompiler
  {
    public const string CONFIG_DOM_GENERATOR_SECT = "dom-gen";

    #region .ctor
    public TextJSTemplateCompiler() : base()
    {
    }

    public TextJSTemplateCompiler(params ITemplateSource<string>[] sources) : base(sources)
    {
    }

    public TextJSTemplateCompiler(IEnumerable<ITemplateSource<string>> sources) : base(sources)
    {
    }
    #endregion

    #region Private/Prot Fields
    private DOMGenerator m_DOMGenerator;
    #endregion

    #region Properties
    public override string LanguageName
    {
      get { return CoreConsts.JS_LANGUAGE; }
    }

    public override string LanguageSourceFileExtension
    {
      get { return CoreConsts.JS_EXTENSION; }
    }

    public DOMGenerator DOMGenerator
    {
      get { return m_DOMGenerator ?? DOMGenerator.Default; }
      set
      {
        EnsureNotCompiled();
        m_DOMGenerator = value;
      }
    }
    #endregion

    #region Protected
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      if (node == null || !node.Exists) return;

      var n = node[CONFIG_DOM_GENERATOR_SECT];
      if (!n.Exists) return;

      m_DOMGenerator = FactoryUtils.MakeAndConfigure<DOMGenerator>(n, typeof(DOMGenerator));
    }

    protected override void DoCompileTemplateSource(CompileUnit unit)
    {
      var text = unit.TemplateSource.GetSourceContent().ToString().Trim();
      var icname = unit.TemplateSource.InferClassName();

      var r = new Regex(@"\/\*{3}(.*?)\*{3}\/", RegexOptions.Singleline);

      unit.CompiledSource = r.Replace(text, delegate(Match m) {
        var c = m.Groups[1].AsLaconicConfig(handling: ConvertErrorHandling.Throw);
        return DOMGenerator.Generate(c);
      });
    }

    protected override void DoCompileCode()
    {
      throw new NotSupportedException("TextJSTemplateCompiler does not support compilation");
    }
    #endregion
  }
}
