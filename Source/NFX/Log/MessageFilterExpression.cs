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

using NFX.Parsing;
using NFX.Log.Destinations;

namespace NFX.Log
{
    /// <summary>
    /// Defines an expression used for log message filtering.
    /// Important: it is not a good practice to create many different scopes as it leads to creation of many assemblies dynamically
    /// </summary>
    public class MessageFilterExpression : CompilingExpressionEvaluator<Destination, bool, Message>
    {
       public const string SCOPE = "_NFX._Log._Filtering";

       /// <summary>
       /// Creates a new expression in a default logging filter scope.
       /// This .ctor will fail if at least one expression from this scope has already been compiled
       /// </summary>
       public MessageFilterExpression(string expr) : base(SCOPE, expr, null, new string[]{ "NFX.Log", "NFX.Log.Destinations"})
       {

       }

       /// <summary>
       /// Use this .ctor to specify a different scope name. Every unique scope name gets compiled into a new assembly,
       /// consequently it is not a good practice to create many different scopes.
       /// This .ctor will fail if at least one expression from this scope has already been compiled
       /// </summary>
       public MessageFilterExpression(string scope,
                                      string expr,
                                      IEnumerable<string> referencedAssemblies = null,
                                      IEnumerable<string> usings = null) : base(scope, expr, referencedAssemblies, usings)
       {

       }

    }
}
