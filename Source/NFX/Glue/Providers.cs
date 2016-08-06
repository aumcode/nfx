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
using NFX.ApplicationModel;
using NFX.Glue.Implementation;
using NFX.ServiceModel;

namespace NFX.Glue
{
    /// <summary>
    /// Represents a base type for providers - providers are facades for some
    /// low-level implementation that transports use, for example ZeroMQ.
    /// </summary>
    public abstract class Provider : GlueComponentService
    {
        private void __ctor()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new GlueException(StringConsts.CONFIGURATION_ENTITY_NAME_ERROR + this.GetType().FullName);
            Glue.RegisterProvider(this);
        }

        protected Provider(string name)
            : this((IGlueImplementation)ExecutionContext.Application.Glue, name)
        {
        }

        protected Provider(IGlueImplementation glue, string name = null) : base(glue, name)
        {
            __ctor();
        }

        protected override void Destructor()
        {
            base.Destructor();
            Glue.UnregisterProvider(this);
        }
    }

    /// <summary>
    /// A registry of Provider-derived instances
    /// </summary>
    public sealed class Providers : Registry<Provider>
    {
          public Providers()
          {

          }


    }




}
