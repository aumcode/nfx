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
 * Revision: NFX 1.0  2011.02.03
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using NFX.ServiceModel;
using NFX.Log;
using NFX.Environment;

namespace NFX.ApplicationModel.Volatile
{

  /// <summary>
  /// Implements ObjectStore service that does nothing
  /// </summary>
  public sealed class NOPObjectStore : ApplicationComponent, IObjectStore
  {

    public NOPObjectStore(): base(){}


    /// <summary>
    /// Retrieves an object reference from the store identified by the "key" or returns null if such object does not exist.
    /// Object is not going to be persisted until it is checked back in the store.
    /// </summary>
    public object CheckOut(Guid key)
    {
      return null;
    }

    /// <summary>
    /// Reverts object state to Normal after the call to Checkout. This way the changes (if any) are not going to be persisted.
    /// Returns true if object was found and checkout canceled. Keep in mind: this method CAN NOT revert inner object state
    ///  to its original state if it was changed, it only unmarks object as changed.
    /// This method is reentrant just like the Checkout is
    /// </summary>
    public bool UndoCheckout(Guid key)
    {
      return false;
    }

    /// <summary>
    /// Puts an object reference "value" into store identified by the "key"
    /// </summary>
    public void CheckIn(Guid key, object value, int msTimeout = 0)
    {

    }

    /// <summary>
    /// Puts an object reference "value" into store identified by the "oldKey" under the "newKey".
    /// If oldKey was not checked in, then checks-in under new key as normally would
    /// </summary>
    public void CheckInUnderNewKey(Guid oldKey, Guid newKey, object value, int msTimeout = 0)
    {

    }

    /// <summary>
    /// Puts an object reference "value" into store identified by the "key"
    /// </summary>
    public bool CheckIn(Guid key, int msTimeout = 0)
    {
       return true;
    }




    /// <summary>
    /// Deletes object identified by key. Returns true when object was found and marked for deletion
    /// </summary>
    public bool Delete(Guid key)
    {
      return false;
    }

    private static NOPObjectStore s_Instance = new NOPObjectStore();

    /// <summary>
    /// Returns a singlelton instance of the objectstore that does not do anything
    /// </summary>
    public static NOPObjectStore Instance
    {
      get { return s_Instance; }
    }



    public object Fetch(Guid key, bool touch = false)
    {
        return null;
    }

    public int ObjectLifeSpanMS
    {
        get { return 0; }
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
  }

}