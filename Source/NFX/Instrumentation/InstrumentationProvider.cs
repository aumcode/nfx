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
using NFX.ServiceModel;
using NFX.Log;
using NFX.Environment;

namespace NFX.Instrumentation
{
    /// <summary>
    /// Defines a base provider for InstrumentationService
    /// </summary>
    public abstract class InstrumentationProvider : Service<InstrumentationService>, IExternallyParameterized
    {
       #region .ctor

        protected InstrumentationProvider(InstrumentationService director) : base(director)
        {

        }
       #endregion

       #region Private Fields

       #endregion


        #region Public

            public abstract void Write(Datum aggregatedDatum);


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

        #endregion

        #region Protected
            protected override void DoConfigure(IConfigSectionNode node)
            {
              base.DoConfigure(node);
            }

            protected void WriteLog(MessageType type, string message, string parameters = null, string from = null)
            {
              App.Log.Write(
                                        new Log.Message
                                        {
                                          Text = message ?? string.Empty,
                                          Type = type,
                                          Topic = CoreConsts.INSTRUMENTATIONSVC_PROVIDER_TOPIC,
                                          From = from,
                                          Parameters = parameters ?? string.Empty
                                        });
            }


        #endregion

        #region Properties

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

        #endregion


    }
}
