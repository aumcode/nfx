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

using NFX.Environment;
using NFX.ServiceModel;
using NFX.Instrumentation;

using NFX.Log;

namespace NFX.Glue
{
    /// <summary>
    /// Provides base functionality for internal glue component implementations
    /// </summary>
    public abstract class GlueComponentService : ServiceWithInstrumentationBase<IGlueImplementation>
    {
        internal GlueComponentService(IGlueImplementation glue) : base(glue)
        {
        }

        protected GlueComponentService(IGlueImplementation glue, string name)
            : base(glue)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = Guid.NewGuid().ToString();

            base.Name = name;
        }

        #region Fields
          private bool m_InstrumentationEnabled;
        #endregion

        #region Properties



            public IGlueImplementation Glue { get { return ComponentDirector; } }

            /// <summary>
            /// Implements IInstrumentable
            /// </summary>
            [Config(Default=false)]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
            public override bool InstrumentationEnabled
            {
              get { return m_InstrumentationEnabled;}
              set { m_InstrumentationEnabled = value;}
            }

        #endregion


        #region Public

          public void WriteLog(
              LogSrc source, MessageType type, string msg, string from = null,
              Exception exception = null, string pars = null, Guid? relatedTo = null)
          {
            var pass = false;

            if (source== LogSrc.Server) pass= type >= Glue.ServerLogLevel;
            else
            if (source== LogSrc.Client) pass= type >= Glue.ClientLogLevel;
            else
             pass = (type >= Glue.ClientLogLevel) || (type >= Glue.ServerLogLevel);


            if ( pass )
                App.Log.Write( new Message
                {
                   Type = type,
                   From = string.Format("{0}:{1}.'{2}' {3}", source, GetType().Name, Name, from ?? string.Empty),
                   Topic = CoreConsts.GLUE_TOPIC,
                   Source = (int)source,
                   Text = msg,
                   Exception = exception,
                   RelatedTo = relatedTo.HasValue ? relatedTo.Value : Guid.Empty,
                   Parameters = pars
                });

          }

        #endregion


    }
}
