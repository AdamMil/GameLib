/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2004 Adam Milazzo

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
*/

using System;
using System.Collections;

// TODO: use generics when they become available

namespace GameLib.Collections
{

#region RedBlackBase
/// <summary>This class implements a red-black tree and serves as the base for <see cref="Map"/> and
/// <see cref="BinaryTree"/>.
/// </summary>
public abstract class RedBlackBase
{ internal RedBlackBase(IComparer comparer) { this.comparer = comparer; }

  /// <summary>This class represents a node in a red-black tree.</summary>
  protected internal class Node
  { public Node() { }
    public Node(object value) { Parent=Left=Right=nullNode; Value=value; Red=true; }
    public Node Parent, Left, Right;
    public object   Value;
    public bool     Red;
    public static Node Null { get { return nullNode; } }
    static readonly Node nullNode = new Node();
  }

  #region EnumeratorBase
  /// <summary>This class serves as a base for enumerators that want to iterate through a red-black tree.</summary>
  public class EnumeratorBase
  { internal EnumeratorBase(RedBlackBase tree)
    { root = tree.Root;
      if(root==Node.Null) GC.SuppressFinalize(this);
      else
      { handler = new TreeChangeHandler(OnTreeChanged);
        tree.TreeChanged += handler;
        this.tree=tree;
      }
      nodes = new Stack();
      reset = true;
    }
    ~EnumeratorBase() { tree.TreeChanged -= handler; }

    /// <summary>Advances the enumerator to the next element of the collection.</summary>
    /// <remarks>See <see cref="IEnumerator.MoveNext"/> for more information. <seealso cref="IEnumerator.MoveNext"/></remarks>
    public bool MoveNext()
    { AssertNotChanged();
      if(root==Node.Null) return false;
      Node n;
      if(reset)
      { n = root;
        do { nodes.Push(n); n=n.Left; } while(n!=Node.Null);
        reset = false;
        return true;
      }

      n = ((Node)nodes.Pop()).Right;
      while(n!=Node.Null) { nodes.Push(n); n=n.Left; }
      return nodes.Count>0;
    }

    /// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
    /// <remarks>See <see cref="IEnumerator.Reset"/> for more information. <seealso cref="IEnumerator.Reset"/></remarks>
    public void Reset()
    { AssertNotChanged();
      nodes.Clear();
      reset = true;
    }

    internal Node Current
    { get
      { if(nodes.Count==0) throw new InvalidOperationException("Bad position.");
        return (Node)nodes.Peek();
      }
    }

    protected void AssertNotChanged()
    { if(changed) throw new InvalidOperationException("The collection has changed");
    }

    void OnTreeChanged()
    { changed=true;
      tree.TreeChanged -= handler;
      GC.SuppressFinalize(this);
    }

    RedBlackBase tree;
    TreeChangeHandler handler;
    Node root;
    Stack nodes;
    bool  reset, changed;
  }
  #endregion

  /// <summary>Gets the number of elements contained in the collection.</summary>
  public int Count { get { return count; } }

  /// <summary>Removes all elements from the collection.</summary>
  public void Clear() { root=Node.Null; }

  /// <summary>Gets the root node of the binary tree.</summary>
  protected Node Root { get { return root; } }

  /// <summary>Adds a node to the binary tree.</summary>
  /// <param name="node">The <see cref="Node"/> to add.</param>
  protected void Add(Node node)
  { if(TreeChanged!=null) TreeChanged();
    Node x=root, y=Node.Null, gp;

    while(x!=Node.Null) { y=x; x=comparer.Compare(node.Value, x.Value)<0 ? x.Left : x.Right; }
    (x=node).Parent = y;

    if(y==Node.Null) root=x;
    else if(comparer.Compare(x.Value, y.Value)<0) y.Left=x;
    else y.Right=x; // equal values go to the right

    while(x!=root && x.Parent.Red)
      if(x.Parent==(gp=x.Parent.Parent).Left)
        if((y=gp.Right).Red)
        { x.Parent.Red = y.Red = false;
          gp.Red = true;
          x = gp;
        }
        else
        { if(x==x.Parent.Right) LeftRotate(x=x.Parent);
          x.Parent.Red = false;
          (gp=x.Parent.Parent).Red = true;
          RightRotate(gp);
        }
      else
        if((y=gp.Left).Red)
        { x.Parent.Red = y.Red = false;
          gp.Red = true;
          x = gp;
        }
        else
        { if(x==x.Parent.Left) RightRotate(x=x.Parent);
          x.Parent.Red = false;
          (gp=x.Parent.Parent).Red = true;
          LeftRotate(gp);
        }
    root.Red=false;
    count++;
  }

  /// <summary>Finds the node with the given value.</summary>
  /// <param name="value">The value to search for.</param>
  /// <returns>The <see cref="Node"/> containing the value or <see cref="Node.Null"/> if the value cannot be found.</returns>
  protected Node Find(object value)
  { Node n=root;
    int  c;
    while(n!=Node.Null)
    { c=comparer.Compare(value, n.Value);
      if(c<0) n=n.Left;
      else if(c>0) n=n.Right;
      else break;
    }
    return n;
  }

  /// <summary>Removes a node from the binary tree.</summary>
  /// <param name="node">The <see cref="Node"/> to remove. If this parameter is <see cref="Node.Null"/>, this method
  /// will return immediately.
  /// </param>
  protected void Remove(Node node)
  { if(node==Node.Null) return;
    if(TreeChanged!=null) TreeChanged();
    Node w, x, y, p;
    y = node.Left==Node.Null || node.Right==Node.Null ? node : Successor(node);
    x = y.Left==Node.Null ? y.Right : y.Left;

    if((x.Parent=y.Parent)==Node.Null) root=x;
    else if(y==y.Parent.Left) y.Parent.Left=x;
    else y.Parent.Right=x;

    if(y!=node) node.Value=y.Value;
    if(!y.Red)
      while(x!=root && !x.Red)
        if(x==(p=x.Parent).Left)
        { w=p.Right;
          if(w.Red)
          { w.Red=false;
            p.Red=true;
            LeftRotate(p);
            w=(p=x.Parent).Right;
          }
          if(!w.Left.Red && !w.Right.Red)
          { w.Red=true;
            x=p;
          }
          else
          { if(!w.Right.Red)
            { w.Left.Red=false;
              w.Red=true;
              RightRotate(w);
              w=(p=x.Parent).Right;
            }
            w.Red=p.Red;
            p.Red=w.Right.Red=false;
            LeftRotate(p);
            break;
          }
        }
        else
        { if((w=p.Left).Red)
          { w.Red=false;
            p.Red=true;
            RightRotate(p);
            w=(p=x.Parent).Left;
          }
          if(!w.Left.Red && !w.Right.Red)
          { w.Red=true;
            x=p;
          }
          else
          { if(!w.Left.Red)
            { w.Right.Red=false;
              w.Red=true;
              LeftRotate(w);
              w=(p=x.Parent).Left;
            }
            w.Red=p.Red;
            p.Red=w.Left.Red=false;
            RightRotate(p);
            break;
          }
        }
    x.Red=false;
    count--;
  }

  delegate void TreeChangeHandler();
  event TreeChangeHandler TreeChanged;

  void LeftRotate(Node x)
  { Node y=x.Right;
    if((x.Right=y.Left)!=Node.Null) y.Left.Parent=x;
    if((y.Parent=x.Parent)!=Node.Null)
    { if(x==x.Parent.Left) x.Parent.Left=y;
      else x.Parent.Right=y;
    }
    else root=y;
    y.Left=x; x.Parent=y;
  }

  void RightRotate(Node y)
  { Node x=y.Left;
    if((y.Left=x.Right)!=Node.Null) x.Right.Parent=y;
    if((x.Parent=y.Parent)!=Node.Null)
    { if(y==y.Parent.Left) y.Parent.Left=x;
      else y.Parent.Right=x;
    }
    else root=x;
    x.Right=y; y.Parent=x;
  }

  Node Successor(Node x)
  { Node y;
    if(x.Right!=Node.Null) for(y=x.Right; y.Left!=Node.Null; y=y.Left);
    else for(y=x.Parent; y!=Node.Null && x==y.Right; x=y, y=y.Parent);
    return y;
  }

  Node root=Node.Null;
  IComparer comparer;
  int count;
}
#endregion

#region Map
/// <summary>This class represents a map.</summary>
/// <remarks>
/// <para>This class works similarly to <see cref="SortedList"/>, but is more scalable. A <see cref="SortedList"/> is
/// O(n) for insertion and removal, while a map is O(log2 n). The <see cref="SortedList"/> generally runs faster for
/// small sets of elements because it doesn't have the overhead of managing a balanced binary tree, however, but
/// slows down quickly with larger numbers of elements. For lookups, both the map and the <see cref="SortedList"/>
/// run in O(log2 n) time, but the <see cref="SortedList"/> is a bit faster in practice because it uses an array
/// internally, and array operations are faster than tree operations. Both are maintained in sorted order by key, so
/// iterating over either returns items in order.
/// </para>
/// <para>A hash table may be preferable to a map if you have a good <see cref="Object.GetHashCode"/>
/// implementation for your keys and don't need items maintained in sorted order.
/// </para>
/// </remarks>
public sealed class Map : RedBlackBase, IDictionary
{ 
  /// <summary>Initializes a new, empty instance of the <see cref="Map"/> class, using <see cref="Comparer.Default"/>
  /// to compare keys.
  /// </summary>
  public Map() : this(Comparer.Default) { }
  /// <summary>Initializes a new, empty instance of the <see cref="Map"/> class, using the specified
  /// <see cref="IComparer"/> to compare keys.
  /// </summary>
  public Map(IComparer comparer) : base(new EntryComparer(comparer)) { }
  /// <summary>Initializes a new instance of the <see cref="Map"/> class containing the items from the given
  /// <see cref="IDictionary"/> and using <see cref="Comparer.Default"/> to compare keys.
  /// </summary>
  public Map(IDictionary dict) : this(dict, Comparer.Default) { }
  /// <summary>Initializes a new instance of the <see cref="Map"/> class containing the items from the given
  /// <see cref="IDictionary"/> and using the specified <see cref="IComparer"/> to compare keys.
  /// </summary>
  public Map(IDictionary dict, IComparer comparer)
    : base(comparer==Comparer.Default ? EntryComparer.Default : new EntryComparer(comparer))
  { foreach(DictionaryEntry de in dict) Add(de.Key, de.Value);
  }

  #region IDictionary
  /// <summary>Gets a value indicating whether the map has a fixed size.</summary>
  /// <remarks>See <see cref="IDictionary.IsFixedSize"/> for more information.
  /// <seealso cref="IDictionary.IsFixedSize"/>
  /// </remarks>
  public bool IsFixedSize { get { return false; } }
  /// <summary>Gets a value indicating whether the map is read-only.</summary>
  /// <remarks>See <see cref="IDictionary.IsReadOnly"/> for more information.<seealso cref="IDictionary.IsReadOnly"/></remarks>
  public bool IsReadOnly  { get { return false; } }
  /// <summary>Gets or sets the element with the specified key. In C#, this property is the indexer for the map.</summary>
  /// <remarks>See <see cref="IDictionary.this"/> for more information.<seealso cref="IDictionary.this"/></remarks>
  public object this[object key]
  { get { Node n=FindKey(key); return n==Node.Null ? null : ((Entry)n.Value).Value; }
    set
    { Node n=FindKey(key);
      if(n==Node.Null) Add(key, value);
      else
      { Entry e = (Entry)n.Value;
        n.Value = new Entry(e.Key, value);
      }
    }
  }
  /// <summary>Gets an <see cref="ICollection"/> containing the keys in the map.</summary>
  /// <remarks>See <see cref="IDictionary.Keys"/> for more information.<seealso cref="IDictionary.Keys"/></remarks>
  public ICollection Keys
  { get
    { object[] array = new object[Count];
      int i=0;
      foreach(DictionaryEntry de in this) array[i++] = de.Key;
      return array;
    }
  }
  /// <summary>Gets an <see cref="ICollection"/> containing the values in the map.</summary>
  /// <remarks>See <see cref="IDictionary.Values"/> for more information.<seealso cref="IDictionary.Values"/></remarks>
  public ICollection Values
  { get
    { object[] array = new object[Count];
      int i=0;
      foreach(DictionaryEntry de in this) array[i++] = de.Value;
      return array;
    }
  }

  /// <summary>Adds an element with the provided key and value to the map.</summary>
  /// <remarks>See <see cref="IDictionary.Add"/> for more information.<seealso cref="IDictionary.Add"/></remarks>
  public void Add(object key, object value)
  { if(key==null) throw new ArgumentNullException("key");
    if(Contains(key)) throw new ArgumentException("Key already exists in collection");
    Add(new Node(new Entry(key, value)));
  }

  // Clear() implemented in RedBlackBase

  /// <summary>Determines whether the map contains an element with the specified key.</summary>
  /// <remarks>The <see cref="IComparer"/> passed to the constructor is used to search for the key.
  /// See <see cref="IDictionary.Contains"/> for more information.<seealso cref="IDictionary.Contains"/>
  /// </remarks>
  public bool Contains(object key) { return FindKey(key)!=Node.Null; }
  /// <summary>Returns an <see cref="IDictionaryEnumerator"/> for the map.</summary>
  /// <remarks>See <see cref="IDictionary.GetEnumerator"/> for more information.
  /// <seealso cref="IDictionary.GetEnumerator"/>
  /// </remarks>
  IDictionaryEnumerator IDictionary.GetEnumerator() { return new Enumerator(this); }
  /// <summary>Removes the element with the specified key from the map.</summary>
  /// <remarks>The <see cref="IComparer"/> passed to the constructor is used to search for the key.
  /// See <see cref="IDictionary.Remove"/> for more information.<seealso cref="IDictionary.Remove"/>
  /// </remarks>
  public void Remove(object key) { base.Remove(FindKey(key)); }
  #endregion

  #region ICollection
  // Count() implemented in RedBlackBase

  /// <summary>Gets a value indicating whether access to the map is synchronized (thread-safe).</summary>
  /// <remarks>See the <see cref="ICollection.IsSynchronized"/> property for more information.
  /// <seealso cref="ICollection.IsSynchronized"/>
  /// </remarks>
  public bool IsSynchronized { get { return false; } }
  /// <summary>Gets an object that can be used to synchronize access to the map.</summary>
  /// <remarks>See the <see cref="ICollection.SyncRoot"/> property for more information.
  /// <seealso cref="ICollection.SyncRoot"/>
  /// </remarks>
  public object SyncRoot { get { return this; } }
  /// <summary>Copies the map entries to an existing one-dimensional array, starting at the specified array index.</summary>
  /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
  /// <see cref="DictionaryEntry"/> objects copied from the map.
  /// </param>
  /// <param name="startIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
  /// <remarks>See <see cref="ICollection.CopyTo"/> for more information. <seealso cref="ICollection.CopyTo"/></remarks>
  public void CopyTo(Array array, int startIndex)
  { foreach(DictionaryEntry de in this) array.SetValue(de, startIndex++);
  }
  #endregion

  #region IEnumerable
  /// <summary>Enumerates the elements of a <see cref="Map"/> in sorted order.</summary>
  public sealed class Enumerator : EnumeratorBase, IDictionaryEnumerator
  { internal Enumerator(Map map) : base(map) { }

    /// <summary>Gets the current dictionary entry in the map.</summary>
    /// <value>A <see cref="DictionaryEntry"/> representing the current entry in the map.</value>
    /// <remarks>See <see cref="IEnumerator.Current"/> for more information.<seealso cref="IEnumerator.Current"/></remarks>
    object IEnumerator.Current
    { get
      { Entry entry = (Entry)base.Current.Value;
        return new DictionaryEntry(entry.Key, entry.Value);
      }
    }
    /// <summary>Gets both the key and the value of the current dictionary entry.</summary>
    /// <value>A <see cref="DictionaryEntry"/> representing the current entry in the map.</value>
    /// <remarks>See <see cref="IDictionaryEnumerator.Entry"/> for more information.
    /// <seealso cref="IDictionaryEnumerator.Entry"/>
    /// </remarks>
    public DictionaryEntry Entry
    { get
      { AssertNotChanged();
        Entry entry = (Entry)base.Current.Value;
        return new DictionaryEntry(entry.Key, entry.Value);
      }
    }
    /// <summary>Gets the key of the current dictionary entry.</summary>
    /// <remarks>See <see cref="IDictionaryEnumerator.Key"/> for more information.
    /// <seealso cref="IDictionaryEnumerator.Key"/>
    /// </remarks>
    public object Key { get { AssertNotChanged(); return ((Entry)base.Current.Value).Key; } }
    /// <summary>Gets the value of the current dictionary entry.</summary>
    /// <remarks>See <see cref="IDictionaryEnumerator.Value"/> for more information.
    /// <seealso cref="IDictionaryEnumerator.Value"/>
    /// </remarks>
    public object Value { get { AssertNotChanged(); return ((Entry)base.Current.Value).Value; } }
  }

  /// <summary>Returns an <see cref="IEnumerator"/> that can interate through the map in sorted order.</summary>
  /// <returns>An <see cref="IEnumerator"/> that can interate through the map in sorted order.</returns>
  public IEnumerator GetEnumerator() { return new Enumerator(this); }
  #endregion

  /// <summary>Determines whether the map contains a specific key.</summary>
  /// <param name="key">The key to search for.</param>
  /// <returns>True if the map contains the given key and false otherwise.</returns>
  /// <remarks>This method is identical to the <see cref="Contains"/> method. The <see cref="IComparer"/> passed to
  /// the constructor is used to compare keys.
  /// </remarks>
  public bool ContainsKey(object key) { return FindKey(key)!=Node.Null; }
  /// <summary>Determines whether the map contains a specific value.</summary>
  /// <param name="value">The value to search for.</param>
  /// <returns>True if the map contains the given value and false otherwise.</returns>
  /// <remarks><see cref="Object.Equals"/> is used to compare map values with <paramref name="value"/>. This method
  /// is much less efficient than <see cref="ContainsKey"/> because it must perform a linear search through the map,
  /// so it runs in O(n) time rather than O(log2 n) time.
  /// </remarks>
  public bool ContainsValue(object value)
  { foreach(DictionaryEntry e in this) if(e.Value.Equals(value)) return true;
    return false;
  }

  /// <summary>Gets the value of the element with the specified key.</summary>
  /// <param name="key">The key to search for.</param>
  /// <returns>The value associated with that key.</returns>
  /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> does not exist in the map.</exception>
  public object Lookup(object key)
  { Node n = FindKey(key);
    if(n==Node.Null) throw new ArgumentException("The specified key does not exist in the collection", "key");
    return ((Entry)n.Value).Value;
  }

  /// <summary>Gets the value of the element with the maximum key value.</summary>
  /// <returns>The value associated with the maximum key value.</returns>
  /// <remarks>The maximum key value is the key with the highest value as determined by the <see cref="IComparer"/>
  /// passed to the constructor.
  /// </remarks>
  public object GetMaximum() { return Maximum().Value; }

  /// <summary>Gets the <see cref="DictionaryEntry"/> of the element with the maximum key value.</summary>
  /// <returns>The <see cref="DictionaryEntry"/> of element with the maximum key value.</returns>
  /// <remarks>The maximum key value is the key with the highest value as determined by the <see cref="IComparer"/>
  /// passed to the constructor.
  /// </remarks>
  public DictionaryEntry GetMaximumEntry()
  { Entry e = Maximum();
    return new DictionaryEntry(e.Key, e.Value);
  }

  /// <summary>Gets the value of the element with the minimum key value.</summary>
  /// <returns>The value associated with the minimum key value.</returns>
  /// <remarks>The minimum key value is the key with the lowest value as determined by the <see cref="IComparer"/>
  /// passed to the constructor.
  /// </remarks>
  public object GetMinimum() { return Minimum().Value; }

  /// <summary>Gets the <see cref="DictionaryEntry"/> of the element with the minimum key value.</summary>
  /// <returns>The <see cref="DictionaryEntry"/> of element with the minimum key value.</returns>
  /// <remarks>The minimum key value is the key with the lowest value as determined by the <see cref="IComparer"/>
  /// passed to the constructor.
  /// </remarks>
  public DictionaryEntry GetMinimumEntry()
  { Entry e = Minimum();
    return new DictionaryEntry(e.Key, e.Value);
  }

  /// <summary>Returns the value of the element with the maximum key value and removes the element from the map.</summary>
  /// <returns>The value associated with the maximum key value.</returns>
  /// <remarks>The maximum key value is the key with the highest value as determined by the <see cref="IComparer"/>
  /// passed to the constructor.
  /// </remarks>
  public object RemoveMaximum() { return RemoveMax().Value; }

  /// <summary>Returns the <see cref="DictionaryEntry"/> of the element with the maximum key value and removes the
  /// element from the map.
  /// </summary>
  /// <returns>The <see cref="DictionaryEntry"/> of element with the maximum key value.</returns>
  /// <remarks>The maximum key value is the key with the highest value as determined by the <see cref="IComparer"/>
  /// passed to the constructor.
  /// </remarks>
  public DictionaryEntry RemoveMaximumEntry()
  { Entry e = RemoveMax();
    return new DictionaryEntry(e.Key, e.Value);
  }

  /// <summary>Returns the value of the element with the minimum key value and removes the element from the map.</summary>
  /// <returns>The value associated with the minimum key value.</returns>
  /// <remarks>The minimum key value is the key with the lowest value as determined by the <see cref="IComparer"/>
  /// passed to the constructor.
  /// </remarks>
  public object RemoveMinimum() { return RemoveMin().Value; }

  /// <summary>Returns the <see cref="DictionaryEntry"/> of the element with the minimum key value and removes the
  /// element from the map.
  /// </summary>
  /// <returns>The <see cref="DictionaryEntry"/> of element with the minimum key value.</returns>
  /// <remarks>The minimum key value is the key with the lowest value as determined by the <see cref="IComparer"/>
  /// passed to the constructor.
  /// </remarks>
  public DictionaryEntry RemoveMinimumEntry()
  { Entry e = RemoveMin();
    return new DictionaryEntry(e.Key, e.Value);
  }

  struct Entry
  { public Entry(object key, object value) { Key=key; Value=value; }
    public object Key, Value;
  }

  sealed class EntryComparer : IComparer
  { public EntryComparer(IComparer comparer) { cmp=comparer; }
    public int Compare(object x, object y) { return cmp.Compare(((Entry)x).Key, ((Entry)y).Key); }
    IComparer cmp;
    public static readonly EntryComparer Default = new EntryComparer(Comparer.Default);
  }

  void AssertNotEmpty() { if(base.Count==0) throw new InvalidOperationException("The collection is empty"); }

  Node FindKey(object key) { return base.Find(new Entry(key, null)); }

  Entry Maximum()
  { AssertNotEmpty();
    Node n=Root;
    while(n.Right!=Node.Null) n=n.Right;
    return (Entry)n.Value;
  }

  Entry Minimum()
  { AssertNotEmpty();
    Node n=Root;
    while(n.Left!=Node.Null) n=n.Left;
    return (Entry)n.Value;
  }

  Entry RemoveMax()
  { AssertNotEmpty();
    Node n=Root;
    while(n.Right!=Node.Null) n=n.Right;
    Entry e = (Entry)n.Value;
    base.Remove(n);
    return e;
  }

  Entry RemoveMin()
  { AssertNotEmpty();
    Node n=Root;
    while(n.Left!=Node.Null) n=n.Left;
    Entry e = (Entry)n.Value;
    base.Remove(n);
    return e;
  }
}
#endregion

#region BinaryTree
/// <summary>This class represents a balanced binary search tree.</summary>
/// <remarks>
/// <para>This class works similarly to <see cref="SortedList"/>, but is more scalable. Also, while
/// <see cref="SortedList"/> is a dictionary (it implements <see cref="IDictionary"/>) and expects unique keys, a
/// binary tree is a simple collection of items. A <see cref="SortedList"/> is O(n) for insertion and removal, while
/// a binary tree is O(log2 n). The <see cref="SortedList"/> generally runs faster for small sets of elements because
/// it doesn't have the overhead of managing a balanced binary tree, however, but slows down quickly with
/// larger numbers of elements. For searching, both the map and the <see cref="SortedList"/> run in O(log2 n) time,
/// but the <see cref="SortedList"/> is a bit faster in practice because it uses an array internally, and array
/// operations are faster than tree operations. Both are maintained in sorted order, so iterating over either
/// returns items in order.
/// </para>
/// <para>It's possible to use the binary tree as a priority queue, but you should consider using
/// <see cref="PriorityQueue"/> instead. See <seealso cref="PriorityQueue"/> for a more detailed comparison of the
/// two classes.
/// </para>
/// <seealso cref="Map"/> <seealso cref="PriorityQueue"/>
/// </remarks>
public sealed class BinaryTree : RedBlackBase, ICollection
{ 
  /// <summary>Initializes a new, empty instance of the <see cref="BinaryTree"/> class, using
  /// <see cref="Comparer.Default"/> to compare values.
  /// </summary>
  public BinaryTree() : base(Comparer.Default) { }
  /// <summary>Initializes a new, empty instance of the <see cref="BinaryTree"/> class, using the specified
  /// <see cref="IComparer"/> to compare values.
  /// </summary>
  public BinaryTree(IComparer comparer) : base(comparer) { }
  /// <summary>Initializes a new instance of the <see cref="BinaryTree"/> class containing the items from the given
  /// <see cref="ICollection"/> and using <see cref="Comparer.Default"/> to compare values.
  /// </summary>
  public BinaryTree(ICollection items) : this(items, Comparer.Default) { }
  /// <summary>Initializes a new instance of the <see cref="BinaryTree"/> class containing the items from the given
  /// <see cref="ICollection"/> and using the specified <see cref="IComparer"/> to compare values.
  /// </summary>
  public BinaryTree(ICollection items, IComparer comparer) : base(comparer) { foreach(object o in items) Add(o); }

  #region ICollection
  // Count() implemented in RedBlackBase

  /// <summary>Gets a value indicating whether access to the tree is synchronized (thread-safe).</summary>
  /// <remarks>See the <see cref="ICollection.IsSynchronized"/> property for more information.
  /// <seealso cref="ICollection.IsSynchronized"/>
  /// </remarks>
  public bool IsSynchronized { get { return false; } }
  /// <summary>Gets an object that can be used to synchronize access to the tree.</summary>
  /// <remarks>See the <see cref="ICollection.SyncRoot"/> property for more information.
  /// <seealso cref="ICollection.SyncRoot"/>
  /// </remarks>
  public object SyncRoot { get { return this; } }
  /// <summary>Copies the tree values to an existing one-dimensional array, starting at the specified array index.</summary>
  /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the values copied from
  /// the tree.
  /// </param>
  /// <param name="startIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
  /// <remarks>See <see cref="ICollection.CopyTo"/> for more information. <seealso cref="ICollection.CopyTo"/></remarks>
  public void CopyTo(Array array, int startIndex) { foreach(object o in this) array.SetValue(o, startIndex++); }
  #endregion

  #region IEnumerable
  /// <summary>Enumerates the elements of a <see cref="BinaryTree"/> in sorted order.</summary>
  public sealed class Enumerator : EnumeratorBase, IEnumerator
  { internal Enumerator(BinaryTree tree) : base(tree) { }
    /// <summary>Gets the current value in the collection.</summary>
    /// <remarks>See <see cref="IEnumerator.Current"/> for more information.<seealso cref="IEnumerator.Current"/></remarks>
    object IEnumerator.Current { get { return base.Current.Value; } }
  }

  /// <summary>Returns an <see cref="IEnumerator"/> that can interate through the tree in sorted order.</summary>
  /// <returns>An <see cref="IEnumerator"/> that can interate through the tree in sorted order.</returns>
  public IEnumerator GetEnumerator() { return new Enumerator(this); }
  #endregion

  /// <summary>Adds a new element to the binary tree.</summary>
  /// <param name="o">The object to add.</param>
  public void Add(object o) { base.Add(new Node(o)); }
  /// <summary>Determines whether the specified object exists in the tree.</summary>
  /// <param name="o">The object to search for.</param>
  /// <remarks>The <see cref="IComparer"/> passed to the constructor is used to compare values.</remarks>
  public bool Contains(object o) { return Find(o)!=Node.Null; }
  /// <summary>Removes an object from the tree.</summary>
  /// <param name="o">The object to remove.</param>
  /// <remarks>The <see cref="IComparer"/> passed to the constructor is used to compare values.</remarks>
  public void Remove(object o) { base.Remove(Find(o)); }

  /// <summary>Gets the item with the maximum value.</summary>
  /// <returns>The item with the maximum value.</returns>
  /// <remarks>The item with the maximum value is the item with the highest value as determined by the
  /// <see cref="IComparer"/> passed to the constructor.
  /// </remarks>
  public object GetMaximum() { return Maximum().Value; }
  /// <summary>Gets the item with the minimum value.</summary>
  /// <returns>The item with the minimum value.</returns>
  /// <remarks>The item with the minimum value is the item with the lowest value as determined by the
  /// <see cref="IComparer"/> passed to the constructor.
  /// </remarks>
  public object GetMinimum() { return Minimum().Value; }
  /// <summary>Removes and returns the item with the maximum value.</summary>
  /// <returns>The item with the maximum value.</returns>
  /// <remarks>The item with the maximum value is the item with the highest value as determined by the
  /// <see cref="IComparer"/> passed to the constructor.
  /// </remarks>
  public object RemoveMaximum()
  { Node n = Maximum();
    base.Remove(n);
    return n.Value;
  }
  /// <summary>Removes and returns the item with the minimum value.</summary>
  /// <returns>The item with the minimum value.</returns>
  /// <remarks>The item with the minimum value is the item with the lowest value as determined by the
  /// <see cref="IComparer"/> passed to the constructor.
  /// </remarks>
  public object RemoveMinimum()
  { Node n = Minimum();
    base.Remove(n);
    return n.Value;
  }

  void AssertNotEmpty() { if(base.Count==0) throw new InvalidOperationException("The collection is empty"); }

  Node Maximum()
  { AssertNotEmpty();
    Node n=Root;
    while(n.Right!=Node.Null) n=n.Right;
    return n;
  }

  Node Minimum()
  { AssertNotEmpty();
    Node n=Root;
    while(n.Left!=Node.Null) n=n.Left;
    return n;
  }
}
#endregion

} // namespace GameLib.Collections
