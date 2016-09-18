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
using System.Collections;

namespace NFX.Erlang.Internal
{
  /// <summary>
  /// This class implements a registry of Mailboxes indexed by Name and Pid
  /// </summary>
  internal class MboxRegistry : IEnumerable<KeyValuePair<ErlPid, ErlMbox>>
  {
    public MboxRegistry(ErlLocalNode enclosingInstance)
    {
      m_Node = enclosingInstance;
      m_ByPid = new ConcurrentDictionary<ErlPid, ErlMbox>();
      m_ByName = new ConcurrentDictionary<ErlAtom, ErlMbox>();
    }

  #region Fields

    private ErlLocalNode m_Node;
    private ConcurrentDictionary<ErlPid, ErlMbox>  m_ByPid  = null; // mbox pids here
    private ConcurrentDictionary<ErlAtom, ErlMbox> m_ByName = null; // mbox names here

  #endregion

  #region Props

    /// <summary>
    /// Return a list of all registered mailboxes
    /// </summary>
    public IEnumerable<ErlAtom> RegNames { get { return m_ByName.Keys.ToList(); } }

    public IEnumerable<ErlPid> Pids { get { return m_ByPid.Keys.ToList(); } }

    /// <summary>
    /// Look up a mailbox based on its name
    /// </summary>
    /// <returns>Mailbox or null if the name is not registered</returns>
    public ErlMbox this[ErlAtom name]
    {
      get
      {
        ErlMbox m;
        return m_ByName.TryGetValue(name, out m) ? m : null;
      }
    }

    /// <summary>
    /// Look up a mailbox based on its pid
    /// </summary>
    /// <returns>Mailbox reference or null if the pid is not present on this node</returns>
    public ErlMbox this[ErlPid pid]
    {
      get
      {
        ErlMbox m;
        return m_ByPid.TryGetValue(pid, out m) ? m : null;
      }
    }

  #endregion

  #region Public

    /// <summary>
    /// Create a named mailbox if one is not already registered, otherwise
    /// return registered mailbox
    /// </summary>
    public ErlMbox Create(ErlAtom name)
    {
      if (name == ErlAtom.Null)
        throw new NFXException(StringConsts.ERL_VALUE_MUST_NOT_BE_NULL_ERROR);

      var mbox = m_ByName.GetOrAdd(name, n =>
      {
        var pid = m_Node.CreatePid();
        var m = new ErlMbox(m_Node, pid, n);
        m_ByPid[pid] = m;
        return m;
      });
      return mbox;
    }

    /// <summary>
    /// Return a new or unused mailbox from freelist
    /// </summary>
    public ErlMbox Create(bool useCache = true)
    {
      ErlMbox m = null;
      ErlPid  pid = ErlPid.Null;

      if (useCache && m_Node.MboxFreelist.TryPeek(out m) &&
          (DateTime.UtcNow - m.LastUsed).TotalSeconds > 60)
        while (m_Node.MboxFreelist.TryDequeue(out m))
        {
          if (m.Disposed || m.DisposeStarted)
            continue;

          pid = m.Self;
          break;
        }

      if (pid == ErlPid.Null)
      {
        pid = m_Node.CreatePid();
        m   = new ErlMbox(m_Node, pid);
      }

      m_ByPid[pid] = m;
      return m;
    }

    /// <summary>
    /// Register an unnamed mailbox with the registry
    /// </summary>
    public bool Register(ErlMbox mbox)
    {
      return register(ErlAtom.Null, mbox);
    }

    /// <summary>
    /// Register a named mailbox with the registry. Name must not be empty
    /// </summary>
    public bool Register(string name, ErlMbox mbox)
    {
      return Register(new ErlAtom(name ?? string.Empty), mbox);
    }

    /// <summary>
    /// Register a named mailbox with the registry. Name must not be empty
    /// </summary>
    public bool Register(ErlAtom name, ErlMbox mbox)
    {
      if (name == ErlAtom.Null)
        throw new ErlException(StringConsts.ERL_INVALID_MBOX_NAME_ERROR);
      return register(name, mbox);
    }

    /// <summary>
    /// Unregister a mailbox and move it to free-list
    /// </summary>
    public void Unregister(ErlMbox mbox)
    {
      ErlMbox m;
      if (mbox.Name != ErlAtom.Null)
        m_ByName.TryRemove(mbox.Name, out m);
      if (m_ByPid.TryRemove(mbox.Self, out m))
      {
        mbox.LastUsed = DateTime.UtcNow;
        if (!m.Disposed && !m.DisposeStarted)
          m_Node.MboxFreelist.Enqueue(m);
      }
    }

  #endregion

  #region .pvt / Internal

    /// <summary>
    /// Clear the registry
    /// </summary>
    internal void Clear()
    {
      m_ByName.Clear();
      m_ByPid.Clear();
    }

    private bool register(ErlAtom name, ErlMbox mbox)
    {
      bool added = false;
      ErlMbox m = m_ByName.GetOrAdd(name, n => { added = true; return mbox; });
      return added;
    }

    public IEnumerator<KeyValuePair<ErlPid, ErlMbox>> GetEnumerator()
    {
      return m_ByPid.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return m_ByPid.GetEnumerator();
    }

  #endregion
  }
}
