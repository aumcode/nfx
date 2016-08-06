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
 * Originated: 2006.01
 * Revision: NFX 0.8  2010.09.20
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;


using NFX.Log;
using NFX.Environment;
using NFX.ServiceModel;
using NFX.Serialization;
using NFX.Serialization.Slim;

namespace NFX.ApplicationModel.Volatile
{
  /// <summary>
  /// Defines a provider that does not do anything - does not store object anywhere but memory
  /// </summary>
  public class NOPObjectStoreProvider : ObjectStoreProvider
  {
    #region .ctor
        public NOPObjectStoreProvider() : base(null)
        {

        }

        public NOPObjectStoreProvider(ObjectStoreService director) : base(director)
        {

        }
    #endregion


    #region Public

        public override IEnumerable<ObjectStoreEntry> LoadAll()
        {
            return Enumerable.Empty<ObjectStoreEntry>();
        }

        public override void Write(ObjectStoreEntry entry)
        {

        }

        public override void Delete(ObjectStoreEntry entry)
        {

        }


    #endregion

  }


}
