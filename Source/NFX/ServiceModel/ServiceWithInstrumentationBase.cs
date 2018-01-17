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
using System.Text;

using NFX.Instrumentation;

namespace NFX.ServiceModel
{
  /// <summary>
  /// Provides base implementation for Service with IInstrumentable logic
  /// </summary>
  public abstract class ServiceWithInstrumentationBase<TDirector> : Service<TDirector>, IInstrumentable where TDirector : class
  {

    protected ServiceWithInstrumentationBase() : base()
    {
    }

    protected ServiceWithInstrumentationBase(TDirector director) : base(director)
    {
    }

    /// <summary>
    /// Turns instrumentation on/off
    /// </summary>
    public abstract bool InstrumentationEnabled
    {
      get;
      set;
    }

    /// <summary>
    /// Returns named parameters that can be used to control this component
    /// </summary>
    public virtual IEnumerable<KeyValuePair<string, Type>> ExternalParameters{ get { return ExternalParameterAttribute.GetParameters(this); } }

    /// <summary>
    /// Returns named parameters that can be used to control this component
    /// </summary>
    public virtual IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
    {
      return ExternalParameterAttribute.GetParameters(this, groups);
    }

    /// <summary>
    /// Gets external parameter value returning true if parameter was found
    /// </summary>
    public virtual bool ExternalGetParameter(string name, out object value, params string[] groups)
    {
        return ExternalParameterAttribute.GetParameter(this, name, out value, groups);
    }

    /// <summary>
    /// Sets external parameter value returning true if parameter was found and set
    /// </summary>
    public virtual bool ExternalSetParameter(string name, object value, params string[] groups)
    {
      return ExternalParameterAttribute.SetParameter(this, name, value, groups);
    }
  }
}
