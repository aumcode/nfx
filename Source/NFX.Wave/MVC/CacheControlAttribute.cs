/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using NFX.Web;

namespace NFX.Wave.MVC
{

  /// <summary>
  /// Decorates controller classes or actions that set NoCache headers in response. By default Force = true
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
  public sealed class CacheControlAttribute : ActionFilterAttribute
  {
    public CacheControlAttribute() : base(0) {}
    public CacheControlAttribute(int order) : base(order) {}

    public CacheControl.Type Cacheability { get; set; }
    public int? MaxAgeSec { get; set; }
    public int? SharedMaxAgeSec { get; set; }

    public bool NoStore { get; set; }
    public bool NoTransform { get; set; }
    public bool MustRevalidate { get; set; }
    public bool ProxyRevalidate { get; set; }

    public bool Force{ get; set; }

    protected internal override bool BeforeActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      return false;
    }

    protected internal override bool AfterActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      work.Response.SetCacheControlHeaders(new CacheControl
      {
        Cacheability = Cacheability,
        MaxAgeSec = MaxAgeSec,
        SharedMaxAgeSec = SharedMaxAgeSec,
        NoStore = NoStore,
        NoTransform = NoTransform,
        MustRevalidate = MustRevalidate,
        ProxyRevalidate = ProxyRevalidate
      }, Force);
      return false;
    }

    protected internal override void ActionInvocationFinally(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
    }
  }
}
