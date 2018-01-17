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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NFX.Erlang.Internal
{
  internal class ErlMonitors : IEnumerable<KeyValuePair<ErlRef, ErlPid>>
  {
    public ErlMonitors(ErlMbox owner)
    {
      m_Mbox = owner;
      m_Monitors = new ConcurrentDictionary<ErlRef, ErlPid>();
    }

    private ErlMbox m_Mbox;
    private ConcurrentDictionary<ErlRef, ErlPid> m_Monitors;

    public ErlRef Add(ErlPid pid)
    {
      var r = m_Mbox.Node.CreateRef();
      Add(r, pid);
      return r;
    }

    public bool Add(ErlRef eref, ErlPid pid)
    {
      bool added = true;
      m_Monitors.AddOrUpdate(eref, pid, (_r, p) => { added = false; return pid; });
      return added;
    }

    public ErlRef Add(ErlAtom node)
    {
      return Add(new ErlPid(node, 0, 0));
    }

    public bool Remove(ErlRef eref)
    {
      ErlPid p;
      return m_Monitors.TryRemove(eref, out p);
    }

    public bool Remove(ErlRef eref, out ErlPid pid)
    {
      return m_Monitors.TryRemove(eref, out pid);
    }




    public IEnumerator<KeyValuePair<ErlRef, ErlPid>> GetEnumerator()
    {
      return m_Monitors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return m_Monitors.GetEnumerator();
    }
  }
}
