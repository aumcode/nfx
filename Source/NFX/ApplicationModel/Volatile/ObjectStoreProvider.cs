/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
 * Revision: NFX 1.0  2011.02.03
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Log;
using NFX.Environment;
using NFX.ServiceModel;

namespace NFX.ApplicationModel.Volatile
{
  /// <summary>
  /// Defines a base provider that stores objects for ObjectStoreService class
  /// </summary>
  public abstract class ObjectStoreProvider : Service<ObjectStoreService>
  {
    #region CONSTS

    #endregion

    #region .ctor

        protected ObjectStoreProvider(ObjectStoreService director) : base(director)
        {

        }
    #endregion


    #region Private Fields

    #endregion


    #region Public

        public abstract IEnumerable<ObjectStoreEntry> LoadAll();

        public abstract void Write(ObjectStoreEntry entry);

        public abstract void Delete(ObjectStoreEntry entry);

    #endregion


    #region Protected
        protected override void DoConfigure(IConfigSectionNode node)
        {
          base.DoConfigure(node);
        }

        protected void WriteLog(MessageType type, string message, string parameters, string from = null)
        {
          App.Log.Write(
                                    new Log.Message
                                    {
                                      Text = message ?? string.Empty,
                                      Type = type,
                                      Topic = CoreConsts.OBJSTORESVC_PROVIDER_TOPIC,
                                      From = from,
                                      Parameters = parameters ?? string.Empty
                                    });
        }


    #endregion


  }


}
