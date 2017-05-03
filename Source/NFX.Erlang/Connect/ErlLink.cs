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
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace NFX.Erlang
{
  public struct ErlLink : IEquatable<ErlLink>, IComparable<ErlLink>
  {
    public readonly ErlAtom Node;
    public readonly ErlPid Pid;

    public ErlLink(ErlAtom remoteNode)
    {
      Node = remoteNode;
      Pid  = ErlPid.Null;
    }

    public ErlLink(ErlPid remote)
    {
      Node = remote.Node;
      Pid  = remote;
    }

    /// <summary>
    /// Returns true if this link points to a pid rather than to a node
    /// </summary>
    public bool HasPid     { get { return Pid != ErlPid.Null; } }
    public bool IsNodeLink { get { return Pid == ErlPid.Null; } }

    public bool Contains(ErlPid pid) { return pid.Equals(Pid); }

    public override bool Equals(object obj)
    {
      return obj is ErlLink && Equals((ErlLink)obj);
    }

    public override int GetHashCode()
    {
      return Pid.GetHashCode();
    }

    public override string ToString()
    {
      return "#ErlLink{{{0}}}".Args(Pid.Empty ? Node.ToString() : Pid.ToString());
    }

    public bool Equals(ErlLink rhs)
    {
      return Node == rhs.Node && Equals(((ErlLink)rhs).Pid);
    }

    public bool Equals(ErlPid remote) { return remote.Equals(Pid); }

    public int CompareTo(ErlLink other)
    {
      var n = Node.CompareTo(other.Node);
      return n != 0 ? n : Pid.CompareTo(other.Pid);
    }
  }

  public class ErlLinks : IEnumerable, IEnumerable<ErlLink>
  {
    //private const int DEFAULT_CAPACITY = 10;

    public ErlLinks()
    {
      m_Links = new SortedSet<ErlLink>();
    }

    private SortedSet<ErlLink> m_Links;

    public SortedSet<ErlLink> Links { get { return m_Links; } }

    public int Count { get { return m_Links.Count; } }

    public bool Add(ErlPid to)
    {
      return Add(new ErlLink(to));
    }

    public bool Add(ErlAtom toNode)
    {
      return Add(new ErlLink(toNode));
    }

    public bool Add(ErlLink link)
    {
      lock (m_Links)
      {
        var res = !m_Links.Contains(link);
        if (res)
          m_Links.Add(link);
        return res;
      }
    }

    public List<ErlLink> Remove(ErlAtom toNode)
    {
      var links = new List<ErlLink>(Math.Min(m_Links.Count, 32));

      lock (m_Links)
          m_Links.RemoveWhere(l => { var r = l.Node == toNode; if (r) links.Add(l); return r; });

      return links;
    }

    public bool Remove(ErlPid to)
    {
      return Remove(new ErlLink(to));
    }

    public bool Remove(ErlLink link)
    {
      lock (m_Links)
          return m_Links.Remove(link);
    }

    public bool Exists(ErlLink link)
    {
      lock (m_Links)
          return m_Links.Contains(link);
    }

    public SortedSet<ErlLink> Clear()
    {
      var temp = new SortedSet<ErlLink>();
      Interlocked.Exchange(ref m_Links, temp);
      return temp;
    }

    public IEnumerator GetEnumerator()
    {
      return m_Links.GetEnumerator();
    }

    IEnumerator<ErlLink> IEnumerable<ErlLink>.GetEnumerator()
    {
      return m_Links.GetEnumerator();
    }
  }
}
