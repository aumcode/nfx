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

namespace NFX.Instrumentation
{
    /// <summary>
    /// Provides IInstrumentation implementation that does nothing
    /// </summary>
    public sealed class NOPInstrumentation : ApplicationComponent, IInstrumentationImplementation
    {
         private static NOPInstrumentation s_Instance = new NOPInstrumentation();

         private NOPInstrumentation() : base()
         {

         }

         /// <summary>
         /// Returns a singlelton instance of the NOPInstrumentation
         /// </summary>
         public static NOPInstrumentation Instance
         {
           get { return s_Instance; }
         }


         public bool InstrumentationEnabled{ get{return false;} set{}}
         public IEnumerable<KeyValuePair<string, Type>> ExternalParameters{ get{ return null;}}
         public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups){ return null;}
         public bool ExternalGetParameter(string name, out object value, params string[] groups)
         {
           value = null;
           return false;
         }
         public bool ExternalSetParameter(string name, object value, params string[] groups)
         {
           return false;
         }


         public bool Enabled
         {
           get { return false; }
         }

         public bool Overflown
         {
            get { return true;}
         }

         public int RecordCount
         {
            get { return 0;}
         }

         public int ProcessingIntervalMS
         {
           get { return 0; }
         }

         public int OSInstrumentationIntervalMS
         {
           get { return 0; }
         }

         public bool SelfInstrumented
         {
           get { return false;}
         }

         public int MaxRecordCount
         {
            get { return 0;}
            set {}
         }

         public IEnumerable<Type> DataTypes { get { return Enumerable.Empty<Type>();}}


         public void Record(Datum datum)
         {
           //does nothing
         }

         public IEnumerable<string> GetDatumTypeSources(Type datumType, out Datum defaultInstance)
         {
           defaultInstance = null;
           return Enumerable.Empty<string>();
         }

         public void Configure(Environment.IConfigSectionNode node)
         {

         }

         public Time.TimeLocation TimeLocation
         {
             get { return App.TimeLocation; }
         }

         public DateTime LocalizedTime
         {
             get { return App.LocalizedTime; }
         }

        public DateTime UniversalTimeToLocalizedTime(DateTime utc)
        {
            return App.UniversalTimeToLocalizedTime(utc);
        }

        public DateTime LocalizedTimeToUniversalTime(DateTime local)
        {
            return App.LocalizedTimeToUniversalTime(local);
        }



         public int ResultBufferSize
         {
           get { return 0; }
         }

         public IEnumerable<Datum> GetBufferedResults(int count=0)
         {
           return Enumerable.Empty<Datum>();
         }

         public IEnumerable<Datum> GetBufferedResultsSince(DateTime utcDate)
         {
           return Enumerable.Empty<Datum>();
         }
    }
}
