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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using NFX.Templatization;
using NUnit.Framework;

namespace NFX.NUnit.Templatization
{

  [TestFixture]
  public class TextCSTemplateCompilerTests
  {
    public const string NFX_NUNIT_DLL = "NFX.NUnit.dll";


    #region Public

      [Test]
      public void InitialState()
      {
        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler();

        Assert.False(compiler.Compiled);
        Assert.False(compiler.CompileCode);
        Assert.IsTrue(string.IsNullOrEmpty(compiler.ReferencedAssembliesSearchPath));
        Assert.AreEqual(0, compiler.Count());
        Assert.AreEqual(0, compiler.CompileUnitsWithErrors.Count());
        Assert.AreEqual(0, compiler.CodeCompilerErrors.Count());
        Assert.AreEqual(6, compiler.ReferencedAssemblies.Count());
      }

      [Test]
      public void CompilationProperties()
      {
        const string srcStr = @"#<conf>
          <compiler base-class-name=""NFX.NUnit.Templatization.TeztTemplate""
                    namespace=""NFX.NUnit.Templatization""
                    abstract=""true""
                    summary=""Test master page""
           />
        #</conf>
        #[class]
        
            public string Title { get {return ""aaaaa""; } }
     
     
            protected abstract void renderHeader();
            protected abstract void renderBody(bool showDetails);
            protected abstract void renderFooter();
     
     
        #[render]   
        <html>
         <head>   
           <title>?[Title]</title>
         </head>
         <body>
     
          <h1>This is Header</h1>
           @[renderHeader();]
       
          <h1>This is Body</h1>
           @[renderBody(true);]
          <p>This is in master page</p>
     
          <h1>This is Footer</h1>
           @[renderFooter();]
       
         </body>
        </html> ";

        TemplateStringContentSource src = new TemplateStringContentSource(srcStr);

        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler(src);

        compiler.Compile();

        Assert.AreEqual(1, compiler.Count());

        CompileUnit unit = compiler.First();

        Assert.IsNull(unit.CompilationException);
        Assert.IsNull(unit.CompiledTemplateType);
        Assert.IsNotNullOrEmpty(unit.CompiledTemplateTypeName);
        Assert.AreSame(src, unit.TemplateSource);
        Assert.AreEqual(srcStr, src.Content);
      }

      [Test]
      public void GeneralClassAttributes()
      {
        const string CLASS_NAMESPACE = "NFX.NUnit.Templatization";
        const string BASE_CLASS_FULLNAME = "NFX.NUnit.Templatization.TeztTemplate";

        string templateStr = @"
  #<conf><compiler 
  base-class-name=""{0}"" 
  namespace=""{1}""
  abstract=""true""
  summary=""Test master page""/>
  #</conf>".Args(BASE_CLASS_FULLNAME, CLASS_NAMESPACE);

        TemplateStringContentSource src = new TemplateStringContentSource(templateStr);

        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler(src);

        compiler.Compile();

        CompileUnit unit = compiler.First();

        CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } });

        CompilerParameters compilerParams = new CompilerParameters() { GenerateInMemory = true, GenerateExecutable = false };

        foreach (var referencedAssembly in compiler.ReferencedAssemblies)
          compilerParams.ReferencedAssemblies.Add(referencedAssembly);

        compilerParams.ReferencedAssemblies.Add(NFX_NUNIT_DLL);

        CompilerResults compilerResults = provider.CompileAssemblyFromSource(compilerParams, unit.CompiledSource);

        Assembly asm = compilerResults.CompiledAssembly;
        Assert.AreEqual(1, asm.GetExportedTypes().Count());

        Type generatedType = asm.GetExportedTypes().First();

        Assert.AreEqual(CLASS_NAMESPACE, generatedType.Namespace);
        Assert.AreEqual(BASE_CLASS_FULLNAME, generatedType.BaseType.FullName);
        Assert.IsTrue(generatedType.IsAbstract);
      }

      [Test]
      public void NotAbstract()
      {
        const string CLASS_NAMESPACE = "NFX.NUnit.Templatization";
        const string BASE_CLASS_FULLNAME = "NFX.NUnit.Templatization.TeztTemplate";

        string templateStr = @"
  #<conf><compiler 
  base-class-name=""{0}"" 
  namespace=""{1}""
  abstract=""false""
  summary=""Test master page""/>
  #</conf>".Args(BASE_CLASS_FULLNAME, CLASS_NAMESPACE);

        TemplateStringContentSource src = new TemplateStringContentSource(templateStr);

        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler(src) {CompileCode = true};
        compiler.ReferenceAssembly(NFX_NUNIT_DLL);

        compiler.Compile();

        CompileUnit cu = compiler.First();

        Assert.IsFalse(cu.CompiledTemplateType.IsAbstract);
      }

      [Test]
      public void AutoGeneratedNamespace()
      {
        const string CLASS_NAMESPACE = "";
        const string BASE_CLASS_FULLNAME = "NFX.NUnit.Templatization.TeztTemplate";

        string templateSrc = @"
#<conf><compiler 
base-class-name=""{0}"" 
namespace=""{1}""
abstract=""true""
summary=""Test master page""/>
#</conf>".Args(BASE_CLASS_FULLNAME, CLASS_NAMESPACE);

        TemplateStringContentSource src = new TemplateStringContentSource(templateSrc);

        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler(src) {CompileCode = true};
        compiler.ReferenceAssembly(NFX_NUNIT_DLL);

        compiler.Compile();

        CompileUnit cu = compiler.First();

        compiler.CodeCompilerErrors.ForEach(e => Console.WriteLine(e.ToMessageWithType()));

        Assert.IsNotNullOrEmpty( cu.CompiledTemplateType.Namespace);
      }

      [Test]
      public void DefaultIfEmptyBaseClass()
      {
        const string CLASS_NAMESPACE = "NFX.NUnit.Templatization";
        const string BASE_CLASS_FULLNAME = "";

        TextCSTemplateCompiler compiler = GetCompilerForSimpleTemplateSrc(CLASS_NAMESPACE, BASE_CLASS_FULLNAME);//, NFX_WEB_DLL);

        CompileUnit cu = compiler.First();

        foreach (var err in compiler.CodeCompilerErrors)
	      {
		      Console.WriteLine(err.Message);
	      }

        Assert.AreEqual("Template<Object, IRenderingTarget, Object>", 
          cu.CompiledTemplateType.BaseType.DisplayNameWithExpandedGenericArgs());
      }

      [Test]
      public void MethodNames()
      {
        const string RENDER_HEADER = "renderHeader";
        const string RENDER_FOOTER = "renderFooter";
        const string RENDER_BODY = "renderBody";
        const string TITLE = "Title";

        string templateSrc = @"
#<conf>
  <compiler base-class-name=""NFX.NUnit.Templatization.TeztTemplate""
            namespace=""NFX.NUnit.Templatization""
            abstract=""true""
            summary=""Test master page""
   />
#</conf>
#[class]
    
    public string " + TITLE + @" { get {return ""aaaaa""; } }


    protected abstract void " + RENDER_HEADER + @"();
    protected abstract void " + RENDER_BODY + @"(bool showDetails);
    protected abstract void " + RENDER_FOOTER + @"();


#[render]   
<html>
 <head>   
   <title>?[Title]</title>
 </head>
 <body>
 
  <h1>This is Header</h1>
   @[renderHeader();]
   
  <h1>This is Body</h1>
   @[renderBody(true);]
  <p>This is in master page</p>
 
  <h1>This is Footer</h1>
   @[renderFooter();]
   
 </body>
</html> 
";

        TemplateStringContentSource src = new TemplateStringContentSource(templateSrc);

        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler(src) {CompileCode = true};
        compiler.ReferenceAssembly(NFX_NUNIT_DLL);

        compiler.Compile();

        CompileUnit cu = compiler.First();
        Type compiledType = cu.CompiledTemplateType;

        Assert.IsNotNull(compiledType.GetMethod(RENDER_HEADER, BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.IsNotNull(compiledType.GetMethod(RENDER_FOOTER, BindingFlags.NonPublic | BindingFlags.Instance));

        MethodInfo methodBody = compiledType.GetMethod(RENDER_BODY, BindingFlags.NonPublic | BindingFlags.Instance, Type.DefaultBinder, new[] {typeof (bool)}, null);

        Assert.IsNotNull(compiledType.GetProperty(TITLE, BindingFlags.Public | BindingFlags.Instance ));
      }

      [Test]
      public void La()
      {
        string templateStr = @"
#<laconf>
  compiler
  {
     base-class-name=""NFX.NUnit.Templatization.TeztTemplate""
     namespace=""TestWebApp.Templates""
     summary=""Test master page""
    
    using {ns=""NFX.Web"" }
    using {ns=""NFX.RecordModel"" }
    using {ns=""BusinessLogic"" }

    attribute {decl=""BusinessLogic.SultanPermission(4)"" }
   
   }   
#</laconf>";

        TemplateStringContentSource templateSrc = new TemplateStringContentSource(templateStr);

        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler(templateSrc);

        compiler.Compile();

        Assert.AreEqual(1, compiler.Count());

        CompileUnit unit = compiler.First();

        Assert.IsNull(unit.CompilationException);
        Assert.IsNull(unit.CompiledTemplateType);
        Assert.IsNotNullOrEmpty(unit.CompiledTemplateTypeName);
        Assert.AreSame(templateSrc, unit.TemplateSource);
        Assert.AreEqual(templateStr, templateSrc.Content);

        
      }

      [Test]
      public void Social()
      {
        string templateStr =@"
#<laconf>
  compiler {
    base-class-name=""NFX.NUnit.Templatization.TeztTemplate""
    namespace=""NFX.NUnit.Templatization""
    summary=""Social Master Page""
  }
#</laconf>

#[class]
  public string Title { get { return ""Social"";}}
#[render]
<html>
  <head>
    <title>?[Title]</title>
  </head>

  <body>

  </body>
</html>";

        TemplateStringContentSource templateSrc = new TemplateStringContentSource(templateStr);

        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler(templateSrc);

        compiler.Compile();

        Assert.AreEqual(1, compiler.Count());

        CompileUnit unit = compiler.First();

        Console.WriteLine(unit.CompiledSource);
      }

    #endregion

    #region .pvt. impl.

      private TextCSTemplateCompiler GetCompilerForSimpleTemplateSrc(string classNamespace, string baseClassFullName, params string[] additionalReferences)
      {
        string templateSrc = @"
  #<conf><compiler 
  base-class-name=""{0}"" 
  namespace=""{1}""
  abstract=""true""
  summary=""Test master page""/>
  #</conf>".Args(baseClassFullName, classNamespace);

        TemplateStringContentSource src = new TemplateStringContentSource(templateSrc);

        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler(src) { CompileCode = true};
        additionalReferences.ForEach(a => compiler.ReferenceAssembly(a));

        compiler.Compile();

        return compiler;
      }

      private TextCSTemplateCompiler GetCompiler(string templateSrc, bool compileCode = false)
      {
        TemplateStringContentSource src = new TemplateStringContentSource(templateSrc);

        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler(src) { CompileCode = compileCode};

        compiler.Compile();

        return compiler;
      }

    #endregion
  }
}
