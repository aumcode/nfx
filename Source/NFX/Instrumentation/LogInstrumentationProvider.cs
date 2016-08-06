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

using NFX.Log;

namespace NFX.Instrumentation
{
    /// <summary>
    /// Represents a provider that writes aggregated datums to log
    /// </summary>
    public class LogInstrumentationProvider : InstrumentationProvider
    {
       #region .ctor

        public LogInstrumentationProvider() : base(null)
        {

        }
       #endregion



        public override void Write(Datum aggregatedDatum)
        {
           App.Log.Write( toMsg(aggregatedDatum) );
        }


        private Message toMsg(Datum datum)
        {
          var msg = new Message
          {
            Type = MessageType.PerformanceInstrumentation,
            Topic = CoreConsts.INSTRUMENTATIONSVC_TOPIC,
            From = datum.GetType().FullName,
            Text = datum.ToString()
          };

          return msg;
        }


    }
}
