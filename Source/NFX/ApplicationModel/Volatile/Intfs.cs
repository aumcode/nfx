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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Time;
using NFX.Environment;
using NFX.Instrumentation;


namespace NFX.ApplicationModel.Volatile
{
  /// <summary>
  /// Outlines interface for object store
  /// </summary>
  public interface IObjectStore : IApplicationComponent, ILocalizedTimeProvider
  {

    /// <summary>
    /// Retrieves an object reference from the store identified by the "key" or returns null if such object does not exist.
    /// Object is not going to be persisted as this method provides logical read-only access. If touch=true then object timestamp is updated
    /// </summary>
    object Fetch(Guid key, bool touch = false);


    /// <summary>
    /// Retrieves an object reference from the store identified by the "key" or returns null if such object does not exist.
    /// Object is not going to be persisted until it is checked back in the store.
    /// </summary>
    object CheckOut(Guid key);


    /// <summary>
    /// Reverts object state to Normal after the call to Checkout. This way the changes (if any) are not going to be persisted.
    /// Returns true if object was found and checkout canceled. Keep in mind: this method CAN NOT revert inner object state
    ///  to its original state if it was changed, it only unmarks object as changed.
    /// This method is reentrant just like the Checkout is
    /// </summary>
    bool UndoCheckout(Guid key);

    /// <summary>
    /// Puts an object into store identified by the "key"
    /// </summary>
    bool CheckIn(Guid key, int msTimeout = 0);


    /// <summary>
    /// Puts an object reference "value" into store identified by the "key"
    /// </summary>
    void CheckIn(Guid key, object value, int msTimeout = 0);

    /// <summary>
    /// Puts an object reference "value" into store identified by the "oldKey" under the "newKey".
    /// If oldKey was not checked in, then checks-in under new key as normally would
    /// </summary>
    void CheckInUnderNewKey(Guid oldKey, Guid newKey, object value, int msTimeout = 0);


    /// <summary>
    /// Deletes object identified by key. Returns true when object was found and marked for deletion
    /// </summary>
    bool Delete(Guid key);

    /// <summary>
    /// Specifies how long objects live without being touched before becoming evicted from the list
    /// </summary>
    int ObjectLifeSpanMS{ get; }

  }


  public interface IObjectStoreImplementation : IObjectStore, IDisposable, IConfigurable, IInstrumentable
  {

  }


}
