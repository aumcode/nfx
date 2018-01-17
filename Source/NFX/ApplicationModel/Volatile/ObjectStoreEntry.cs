/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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

namespace NFX.ApplicationModel.Volatile
{

  /// <summary>
  /// Represents status of ObjectStoreEntry
  /// </summary>
  public enum ObjectStoreEntryStatus { Normal, CheckedOut, ChekedIn, Deleted }

  /// <summary>
  /// Internal framework class that stores data in ObjectStoreService
  /// </summary>
  public class ObjectStoreEntry
  {
    public ObjectStoreEntryStatus Status;

    public DateTime LastTime;

    public Guid Key;
    public object Value;

    public int CheckoutCount;

    public int MsTimeout;


    public override string ToString()
    {
      return Key.ToString();
    }

    public override int GetHashCode()
    {
      return Key.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      var o = obj as ObjectStoreEntry;

      if (o == null) return false;

      return Key.Equals(o.Key);
    }

  }

  internal class Bucket : Dictionary<Guid, ObjectStoreEntry>
  {
     public Bucket() : base(0xff) {} //capacity

     public DateTime LastAcquire = DateTime.MinValue;
  }


}
