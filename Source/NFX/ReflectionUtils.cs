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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 1.0  2011.01.31
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Threading.Tasks;

using NFX.Environment;
using NFX.DataAccess.CRUD;

namespace NFX
{
  /// <summary>
  /// Provides a collection of frequently-used reflection extension methods
  /// </summary>
  public static class ReflectionUtils
  {
    public static bool OfSignature(this MethodInfo method, params Type[] args)
    {
      if (method==null) return false;

      var pars = method.GetParameters();

      return (args==null || args.Length==0) ? 
              pars.Length==0 :
              pars.Select(i => i.ParameterType).SequenceEqual(args);
    }

    public static int IndexOfArg(this MethodInfo method, Type type, string name)
    {
      if (method==null || type==null) return -1;

      var pars = method.GetParameters();
      for (int i=0; i<pars.Length; i++)
      {
        var par = pars[i];
        if (par.ParameterType==type && (name==null || name.EqualsSenseCase(par.Name)))
          return i;
      }

      return -1;
    }

  }
}
