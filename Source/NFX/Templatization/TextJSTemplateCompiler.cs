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
using System.Text.RegularExpressions;

using NFX.Environment;

namespace NFX.Templatization
{
  /// <summary>
  /// Compiles templates based of text files that use js language syntax,
  /// compiles config to inline function
  /// </summary>
  /// <example>
  ///   argument[0] - root container 'id' or 'node'
  ///   argument[1] - context for variable evaluation
  ///   <code>
  ///   function() {
  ///     /***
  ///     {
  ///       tag-name="inner text"
  ///       property-name="property value"
  ///       evaluted-property-name="@prop@ evaluated property"
  ///       on-event="function() { click(); }"
  ///     }
  ///     ***/
  ///   }
  ///   </code>
  /// </example>
  public class TextJSTemplateCompiler : TemplateCompiler
  {
    public const string CONFIG_DOM_GENERATOR_SECT = "dom-gen";

    #region .ctor
    public TextJSTemplateCompiler() : base() { }

    public TextJSTemplateCompiler(params ITemplateSource<string>[] sources) : base(sources) { }

    public TextJSTemplateCompiler(IEnumerable<ITemplateSource<string>> sources) : base(sources) { }
    #endregion

    #region Private/Prot Fields
    private DOMGenerator m_DOMGenerator;
    private string m_DomGenConfig;
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

    [Config(CONFIG_DOM_GENERATOR_SECT)]
    public DOMGenerator DOMGenerator
    {
      get { return m_DOMGenerator ?? DOMGenerator.Default; }
      set
      {
        EnsureNotCompiled();
        m_DOMGenerator = value;
      }
    }

    [Config("$"+CONFIG_DOM_GENERATOR_SECT)]
    public string DomGenConfig
    {
      get { return m_DomGenConfig; }
      set
      {
        EnsureNotCompiled();
        m_DomGenConfig = value;
        m_DOMGenerator = FactoryUtils.MakeAndConfigure<DOMGenerator>(m_DomGenConfig.AsLaconicConfig(),typeof(DOMGenerator));
      }
    }
    #endregion

    #region Protected
    protected override void DoCompileTemplateSource(CompileUnit unit)
    {
      var text = unit.TemplateSource.GetSourceContent().ToString().Trim();
      var icname = unit.TemplateSource.InferClassName();

      var r = new Regex(@"\/\*{3}(.*?)\*{3}\/", RegexOptions.Singleline);

      var counter = 0;
      unit.CompiledSource = r.Replace(text, (m) => {
        counter++;
        var conf = m.Groups[1].AsLaconicConfig(handling: ConvertErrorHandling.Throw);
        return DOMGenerator.Generate(conf, ref counter);
      });
    }

    protected override void DoCompileCode()
    {
      throw new NotSupportedException("TextJSTemplateCompiler does not support compilation");
    }
    #endregion
  }
}
