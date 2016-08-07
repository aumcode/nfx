/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 Dmitriy Khmaladze, IT Adapter Inc / 2015-2016 Aum Code LLC
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
/*
 * Author: Dmitriy Khmaladze, Spring 2015  dmitriy@itadapter.com
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.ApplicationModel.Pile
{
  /// <summary>
  /// Represents a pointer to the pile object (object stored in a pile).
  /// The reference may be local or distributed in which case the NodeID is>=0.
  /// Distributed pointers are very useful for organizing piles of objects distributed among many servers, for example
  ///  for "Big Memory" implementations or large neural networks where nodes may inter-connect between servers.
  /// The CLR reference to the IPile is not a part of this struct for performance and practicality reasons, as
  ///  it is highly unlikely that there are going to be more than one instance of a pile in a process, however
  ///  should more than 1 pile be allocated than this pointer would need to be wrapped in some other structure along with source IPile reference
  /// </summary>
  public struct PilePointer : IEquatable<PilePointer>
  {

    /// <summary>
    /// Returns a -1:-1 non-valid pointer (either local or distributed)
    /// </summary>
    public static PilePointer Invalid{ get{ return new PilePointer(-1,-1);} }

    /// <summary>
    /// Creates distributed pointer
    /// </summary>
    public PilePointer(int nodeId, int seg, int addr)
    {
      NodeID = nodeId;
      Segment = seg;
      Address = addr;
    }

    /// <summary>
    /// Create local pointer
    /// </summary>
    public PilePointer(int seg, int addr)
    {
      NodeID = -1;
      Segment = seg;
      Address = addr;
    }

    /// <summary>
    /// Distributed Node ID. The local pile sets this to -1 rendering this pointer as !DistributedValid
    /// </summary>
    public readonly int NodeID;

    /// <summary>
    /// Segment # within pile
    /// </summary>
    public readonly int Segment;

    /// <summary>
    /// Address within the segment
    /// </summary>
    public readonly int Address;

    /// <summary>
    /// Returns true if the pointer has positive segment and address, however this does not mean that pointed-to data exists.
    /// Even if this is a valid local pointer it may be an invalid distributed pointer
    /// </summary>
    public bool Valid{ get{ return Segment>=0 && Address>=0;}}

    /// <summary>
    /// Returns true if the pointer has positive distributed NodeID and has a valid local pointer
    /// </summary>
    public bool DistributedValid{ get{ return NodeID>=0 && Valid;}}

    public override int GetHashCode()
    {
      return Address;
    }

    public override bool Equals(object obj)
    {
      if (obj is PilePointer)
       return this.Equals((PilePointer)obj);

      return false;
    }

    public bool Equals(PilePointer other)
    {
      return (this.NodeID == other.NodeID) && (this.Segment == other.Segment) && (this.Address == other.Address);
    }

    public override string ToString()
    {
      if (NodeID<0)
       return "L:"+Segment.ToString("X4")+":"+Address.ToString("X8");
      else
       return NodeID.ToString("X4")+":"+Segment.ToString("X4")+":"+Address.ToString("X8");
    }

    public static bool operator ==(PilePointer l, PilePointer r)
    {
      return l.Equals(r);
    }

    public static bool operator !=(PilePointer l, PilePointer r)
    {
      return !l.Equals(r);
    }

  }
}
