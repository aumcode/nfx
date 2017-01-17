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
using System.Threading;

using NFX.Web;
using NFX.Serialization.JSON;
using NFX.Environment;
using NFX.Wave.Templatization;
using ErrorPage = NFX.Wave.Templatization.StockContent.Error;

namespace NFX.Wave
{
  /// <summary>
  /// Used for matches to bypass AuthorizationExceptions, requires EXCEPTION passed in Make(context)
  /// </summary>
  public sealed class NotAuthorizationExceptionMatch : WorkMatch
  {
    public NotAuthorizationExceptionMatch(IConfigSectionNode confNode) : base(confNode) { }

    public override JSONDataMap Make(WorkContext work, object context = null)
    {
      if (context is NFX.Security.AuthorizationException) return null;
      if (context is Exception && ((Exception)context).InnerException is NFX.Security.AuthorizationException) return null;
      return base.Make(work, context);
    }
  }

  /// <summary>
  /// Used for matching of exceptions passed to Make(context)
  /// </summary>
  public sealed class ExceptionMatch : WorkMatch
  {
    public ExceptionMatch(string name, int order) : base(name, order) { }
    public ExceptionMatch(IConfigSectionNode confNode) : base(confNode) { }

    private Type m_ExceptionType;
    private bool m_IsNot;

    [Config]
    public string ExceptionType
    {
      get { return m_ExceptionType==null ? string.Empty : m_ExceptionType.AssemblyQualifiedName; }
      set { m_ExceptionType = value.IsNullOrWhiteSpace() ? null : Type.GetType(value, true, true); }
    }

    [Config]
    public bool IsNot
    {
      get { return m_IsNot; }
      set { m_IsNot = value; }
    }

    public override JSONDataMap Make(WorkContext work, object context = null)
    {
      var error = context as Exception;
      while (error != null && m_ExceptionType != null)
      {
        var got = error.GetType();

        if (m_IsNot && got == m_ExceptionType) return null;
        if (!m_IsNot && got != m_ExceptionType) return null;

        error = error.InnerException;
      }
      return base.Make(work, context);
    }
  }


}
