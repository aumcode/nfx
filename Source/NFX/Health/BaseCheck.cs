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
using System.Collections.Specialized;

using NFX.Environment;

namespace NFX.Health
{
    /// <summary>
    /// Base class for all Health Checks. Particular health checks must inherit from
    ///  this class to be invoked
    /// </summary>
    [Config("/health-checks")]
    public abstract class BaseCheck : IConfigurable
    {


       /// <summary>
       /// Results property bag may be used for reporting of additional
       /// health-check parameters i.e. latency, content size etc...
       /// </summary>
       public readonly CheckResult Result = new CheckResult();


       /// <summary>
       /// Indicates whether this check will be executed
       /// </summary>
       [Config("@assembly@/@type@/$can-run")]
       public virtual bool CanRun
       {
         get ;
         set ;
       }


       /// <summary>
       /// Provides textual name for this check. Base implementation returns full type name
       /// </summary>
       public virtual string Name
       {
         get { return this.GetType().FullName; }
       }

       /// <summary>
       /// Provides additional textual description of the check
       /// </summary>
       public virtual string Description
       {
         get { return string.Empty; }
       }


       /// <summary>
       /// Runs the check
       /// </summary>
       internal void Run(NameValueCollection parameters)
       {
         try
         {
           DoRun(parameters);
         }
         catch(Exception error)
         {
           Result.Exception = error;
         }
       }

       /// <summary>
       /// Override to provide particular health check implementation
       /// </summary>
       protected abstract void DoRun(NameValueCollection parameters);



       public void Configure(IConfigSectionNode node)
       {
           ConfigAttribute.Apply(this, node);
       }
    }
}
