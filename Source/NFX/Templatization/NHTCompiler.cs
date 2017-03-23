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

namespace NFX.Templatization
{
  /// <summary>
  /// Compiles templates based of text files that use C# language syntax,
  /// First compiles by the TextJSTemplateCompiler then by the TextCSTemplateCompiler
  /// </summary>
  public class NHTCompiler : TextCSTemplateCompiler
  {
    #region CONSTS
    public const string CONFIG_JS_COMPILER_SECT = "jsopt";
    #endregion

    #region .ctor
    public NHTCompiler() : base () { }

    public NHTCompiler(params ITemplateSource<string>[] sources) : base (sources) { }

    public NHTCompiler(IEnumerable<ITemplateSource<string>> sources) : base (sources) { }
    #endregion

    #region Fields
    private string m_JSConfig;
    #endregion

    #region Props
    public override string LanguageName
    {
      get { return CoreConsts.CS_LANGUAGE; }
    }

    public override string LanguageSourceFileExtension
    {
      get { return CoreConsts.CS_EXTENSION; }
    }

    [Config("$jsopt")]
    public string JSConfigStr
    {
      get { return m_JSConfig; }
      set
      {
        EnsureNotCompiled();
        m_JSConfig = value;
      }
    }
    #endregion

    #region Protected

    protected override string GetCompileUnitSourceContent(CompileUnit unit, out string className)
    {
      className = unit.TemplateSource.InferClassName();

      var cmp = getJSCompiler();
      cmp.IncludeTemplateSource(unit.TemplateSource);
      cmp.Compile();
      return cmp.First().CompiledSource;
    }
    #endregion

    #region .pvt
    public TextJSTemplateCompiler getJSCompiler()
    {
      if (m_JSConfig.IsNotNullOrWhiteSpace())
      {
        var conf = m_JSConfig.AsLaconicConfig();
        if (conf != null && conf.Exists)
          return FactoryUtils.MakeAndConfigure<TextJSTemplateCompiler>(conf, typeof(TextJSTemplateCompiler));
      }
      return new TextJSTemplateCompiler();
    }
    #endregion
  }
}
