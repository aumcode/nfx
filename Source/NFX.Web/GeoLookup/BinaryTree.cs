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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Web.GeoLookup
{
  public class BinaryIndexedTree<K, V> where K : IEnumerable<bool>
  {
    public struct Node
    {
      public int Index;
      public uint Left;
      public uint Right;
      public override string ToString()
      {
        return "{0}:{1}|{2}".Args(Index, Left, Right);
      }
    }

    public readonly Node[] Nodes;
    public readonly V[] Values;

    public BinaryIndexedTree(Node[] nodes, V[] values)
    {
      Nodes = nodes;
      Values = values;
    }

    public virtual V this[K key]
    {
      get
      {
        return Find(key, 0);
      }
    }

    protected int FindIndex(K key, uint offset)
    {
      var index = Nodes[offset].Index;
      foreach (var bit in key)
      {
        offset = bit ? Nodes[offset].Right : Nodes[offset].Left;
        if (offset == 0)
          break;
        if (Nodes[offset].Index >= 0)
          index = Nodes[offset].Index;
      }
      return index;
    }

    protected bool FindOffset(K key, ref uint offset)
    {
      var initial = offset;
      foreach (var bit in key)
      {
        offset = bit ? Nodes[offset].Right : Nodes[offset].Left;
        if (offset == 0)
        {
          offset = initial;
          return false;
        }
      }
      return true;
    }

    protected V Find(K key, uint offset)
    {
      var index = FindIndex(key, offset);
      if (index < 0) return default(V);
      return Values[index];
    }
  }

  public class BinaryTree<K, V> where K : IEnumerable<bool>
  {
    public class Node<T> where T : IEnumerable<bool>
    {
      public Node<T> Left { get; set; }
      public Node<T> Right { get; set; }
      public int Index { get; set; }
      public uint Offset { get; set; }
      public Node() { Index = -1; }

      public int this[T key]
      {
        get { return Find(key).Index; }
        set { Find(key).Index = value; }
      }

      public int FindIndex(T key)
      {
        var node = this;
        var index = node.Index;
        foreach (var bit in key)
        {
          node = bit ? node.Right : node.Left;
          if (node == null) break;
          if (node.Index >= 0)
            index = node.Index;
        }
        return index;
      }

      public Node<T> Find(T key)
      {
        var node = this;

        foreach (var bit in key)
        {
          var next = bit ? node.Right : node.Left;
          if (next == null)
          {
            next = new Node<T>();
            if (bit) node.Right = next;
            else node.Left = next;
          }
          node = next;
        }

        return node;
      }
    }

    public readonly Node<K> Root;
    public readonly List<V> Values;

    public V this[K key]
    {
      get
      {
        var index = Root.FindIndex(key);
        if (index < 0) return default(V);
        return Values[index];
      }
      set
      {
        var node = Root.Find(key);
        if (node.Index < 0)
        {
          node.Index = Values.Count;
          Values.Add(value);
        }
        else
          Values[node.Index] = value;
      }
    }

    public BinaryTree()
    {
      Root = new Node<K>();
      Values = new List<V>();
    }

    public BinaryIndexedTree<K, V> BuildIndex()
    {
      var count = 0u;
      iterate(Root, node =>
      {
        node.Offset = count++;
      });
      var nodes = new BinaryIndexedTree<K, V>.Node[count];
      iterate(Root, node =>
      {
        nodes[node.Offset].Index = node.Index;
        var left = node.Left;
        nodes[node.Offset].Left = left != null ? left.Offset : 0;
        var right = node.Right;
        nodes[node.Offset].Right = right != null ? right.Offset : 0;
      });
      return new BinaryIndexedTree<K, V>(nodes, Values.ToArray());
    }

    private void iterate(Node<K> node, Action<Node<K>> callback)
    {
      if (node != null)
      {
        callback(node);
        iterate(node.Left, callback);
        iterate(node.Right, callback);
      }
    }
  }
}
