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


/* NFX by ITAdapter
 * Originated: 2008.03
 * Revision: NFX 1.0  2011.02.06
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.ApplicationModel;
using NFX.Environment;
using NFX.Instrumentation;

namespace NFX.DataAccess
{
      /// <summary>
      /// Represents a store that can save and retrieve data
      /// </summary>
      public interface IDataStore : IApplicationComponent
      {
         /// <summary>
         /// Returns the name of the underlying store technology, i.e. "ORACLE".
         /// This property is used by some metadata-based validation logic which is target-dependent
         /// </summary>
         string TargetName{get;}

         /// <summary>
         /// Tests connection and throws an exception if connection could not be established
         /// </summary>
         void TestConnection();
      }


      /// <summary>
      /// Represents a store that can save and retrieve data
      /// </summary>
      public interface IDataStoreImplementation : IDataStore, IDisposable, IConfigurable, IInstrumentable
      {
         /// <summary>
         /// Defines log level for data stores
         /// </summary>
         StoreLogLevel LogLevel { get; set; }
      }


}
