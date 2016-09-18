/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace NFX.IO.Net.Gate
{
  /// <summary>
  /// Represents the state of the metwrk site - it can be a particular address or group
  /// </summary>
  public sealed class NetSiteState
  {
      internal NetSiteState(Group group)
      {
        Group = group;
        m_LastTouch = DateTime.UtcNow;
      }

      internal NetSiteState(string addr)
      {
        Address = addr;
        m_LastTouch = DateTime.UtcNow;
      }


      internal Dictionary<string, _value> m_Variables = new Dictionary<string, _value>(StringComparer.OrdinalIgnoreCase);
      internal DateTime m_LastTouch;



      public readonly string Address;
      public readonly Group Group;

      public string Key { get{ return Group==null ? Address : Group.Key;}}


        internal class _value
        {
          public long Value;
        }

  }


}
