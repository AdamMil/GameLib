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

/// <summary>This class represents a doubly-linked list of objects.</summary>
public sealed class LinkedList : ICollection, IEnumerable
{ 
  /// <summary>Initializes a new, empty instance of the <see cref="LinkedList"/> class, using
  /// <see cref="Comparer.Default"/> to compare elements.
  /// </summary>
  public LinkedList() { cmp=Comparer.Default; }
  /// <summary>Initializes a new, empty instance of the <see cref="LinkedList"/> class, using the specified
  /// <see cref="IComparer"/> to compare elements.
  /// </summary>
  /// <param name="comparer">The <see cref="IComparer"/> that will be used to compare elements.</param>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="comparer"/> is null.</exception>
  public LinkedList(IComparer comparer)
  { if(comparer==null) throw new ArgumentNullException();
    cmp=comparer;
  }
  /// <summary>Initializes a new instance of the <see cref="LinkedList"/> class containing the objects from the
  /// given collection and using <see cref="Comparer.Default"/> to compare elements.
  /// </summary>
  /// <param name="items">A <see cref="ICollection"/> of objects that will be inserted into the linked list.</param>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="items"/> is null.</exception>
  public LinkedList(ICollection items) : this(Comparer.Default, items) { }
  /// <summary>Initializes a new instance of the <see cref="LinkedList"/> class containing the objects from the
  /// given collection and using the specified <see cref="IComparer"/> to compare elements.
  /// </summary>
  /// <param name="comparer">The <see cref="IComparer"/> that will be used to compare elements.</param>
  /// <param name="items">A <see cref="ICollection"/> of objects that will be inserted into the linked list.</param>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="comparer"/> or <paramref name="items"/> is
  /// null.
  /// </exception>
  public LinkedList(IComparer comparer, ICollection items)
  { cmp = comparer;
    if(comparer==null || items==null) throw new ArgumentNullException();
    foreach(object o in items) Append(o);
  }

  /// <summary>This class represents a node in the linked list.</summary>
  public sealed class Node
  { internal Node(object data) { this.data=data; }
    internal Node(object data, Node prev, Node next) { this.data=data; Prev=prev; Next=next; }
    /// <summary>Returns the previous node in the linked list or null if there is no previous node.</summary>
    public Node PrevNode { get { return Prev; } }
    /// <summary>Returns the next node in the linked list or null if there is no next node.</summary>
    public Node NextNode { get { return Next; } }
    /// <summary>Gets the data associated with this node.</summary>
    public object Data { get { return data; } set { data=value; } }
    internal Node Prev, Next;
    object data;
  }

  #region ICollection
  /// <summary>Gets the number of elements contained in the list.</summary>
  public int Count { get { return count; } }
  /// <summary>Gets a value indicating whether access to the list is synchronized (thread-safe).</summary>
  /// <remarks>See the <see cref="ICollection.IsSynchronized"/> property for more information.
  /// <seealso cref="ICollection.IsSynchronized"/>
  /// </remarks>
  public bool IsSynchronized { get { return false; } }
  /// <summary>Gets an object that can be used to synchronize access to the list.</summary>
  /// <remarks>See the <see cref="ICollection.SyncRoot"/> property for more information.
  /// <seealso cref="ICollection.SyncRoot"/>
  /// </remarks>
  public object SyncRoot { get { return this; } }
  /// <summary>Copies the list elements to an existing one-dimensional array, starting at the specified array index.</summary>
  /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied
  /// from the list.
  /// </param>
  /// <param name="startIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
  /// <remarks>See <see cref="ICollection.CopyTo"/> for more information. <seealso cref="ICollection.CopyTo"/></remarks>
  public void CopyTo(Array array, int startIndex)
  { Node n = head;
    while(head!=null) array.SetValue(head.Data, startIndex++);
  }
  #endregion

  #region IEnumerable
  /// <summary>Represents an enumerator for a <see cref="LinkedList"/> collection.</summary>
  public sealed class Enumerator : IEnumerator
  { internal Enumerator(LinkedList list)
    { head = list.head;
      if(head==null) GC.SuppressFinalize(this);
      else
      { handler = new ListChangeHandler(OnListChanged);
        list.ListChanged += handler;
        this.list=list;
      }
      Reset();
    }
    ~Enumerator() { list.ListChanged -= handler; }

    /// <summary>Gets the current element in the collection.</summary>
    /// <remarks>See <see cref="IEnumerator.Current"/> for more information. <seealso cref="IEnumerator.Current"/></remarks>
    public object Current
    { get
      { if(cur==null) throw new InvalidOperationException("Invalid position");
        return cur.Data;
      }
    }

    /// <summary>Advances the enumerator to the next element of the collection.</summary>
    /// <remarks>See <see cref="IEnumerator.MoveNext"/> for more information. <seealso cref="IEnumerator.MoveNext"/></remarks>
    public bool MoveNext()
    { AssertNotChanged();
      if(cur==null)
      { if(!reset || head==null) return false;
        cur   = head;
        reset = false;
        return true;
      }
      cur = cur.Next;
      return cur!=null;
    }

    /// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
    /// <remarks>See <see cref="IEnumerator.Reset"/> for more information. <seealso cref="IEnumerator.Reset"/></remarks>
    public void Reset() { AssertNotChanged(); cur=null; reset=true; }

    void AssertNotChanged() { if(changed) throw new InvalidOperationException("The collection has changed"); }

    void OnListChanged()
    { changed=true;
      list.ListChanged -= handler;
      GC.SuppressFinalize(this);
    }

    ListChangeHandler handler;
    LinkedList list;
    Node  head, cur;
    bool  reset, changed;
  }

  /// <summary>Returns an enumerator that can iterate through the linked list.</summary>
  /// <returns>An <see cref="IEnumerator"/> that can be used to iterate through the list.</returns>
  IEnumerator IEnumerable.GetEnumerator() { return new Enumerator(this); }

  internal delegate void ListChangeHandler();
  internal event ListChangeHandler ListChanged;
  #endregion

  /// <summary>Gets the node at the head (beginning) of the linked list or null if the list is empty.</summary>
  public Node Head { get { return head; } }
  /// <summary>Gets the node at the tail (end) of the linked list or null if the list is empty.</summary>
  public Node Tail { get { return tail; } }

  /// <summary>Appends an object to the end of the linked list.</summary>
  /// <param name="o">The object to add to the list.</param>
  /// <returns>Returns the new <see cref="Node"/> associated with the object.</returns>
  public Node Append(object o)
  { if(tail==null) { count=1; return head=tail=new Node(o); }
    else return InsertAfter(tail, new Node(o));
  }
  /// <summary>Prepends an object to the beginning of the linked list.</summary>
  /// <param name="o">The object to add to the list.</param>
  /// <returns>Returns the new <see cref="Node"/> associated with the object.</returns>
  public Node Prepend(object o)
  { if(head==null) { count=1; return head=tail=new Node(o); }
    else return InsertBefore(head, new Node(o));
  }
  /// <summary>Inserts an object after the given object in the linked list.</summary>
  /// <param name="at">The object after which <paramref name="o"/> will be inserted.</param>
  /// <param name="o">The object to add to the list.</param>
  /// <returns>Returns the new <see cref="Node"/> associated with the object.</returns>
  /// <remarks>The <see cref="IComparer"/> given to the constructor is used to find <paramref name="at"/>.</remarks>
  public Node InsertAfter(object at, object o)
  { Node n = Find(at);
    if(n==null) throw new ArgumentException("Object not found in list", "at");
    return InsertAfter(n, o);
  }
  /// <summary>Inserts an object before the given object in the linked list.</summary>
  /// <param name="at">The object before which <paramref name="o"/> will be inserted.</param>
  /// <param name="o">The object to add to the list.</param>
  /// <returns>Returns the new <see cref="Node"/> associated with the object.</returns>
  /// <remarks>The <see cref="IComparer"/> given to the constructor is used to find <paramref name="at"/>.</remarks>
  public Node InsertBefore(object at, object o)
  { Node n = Find(at);
    if(n==null) throw new ArgumentException("Object not found in list", "at");
    return InsertBefore(n, o);
  }
  /// <summary>Returns the given object from the list.</summary>
  /// <param name="o">The object to remove from the list.</param>
  /// <remarks>The <see cref="IComparer"/> given to the constructor is used to find <paramref name="o"/>.</remarks>
  public void Remove(object o) { Remove(Find(o)); }
  /// <summary>Returns true if the list contains the specified object.</summary>
  /// <param name="o">The object to search for.</param>
  /// <returns>True if the list contains the specified object and false otherwise.</returns>
  /// <remarks>The <see cref="IComparer"/> given to the constructor is used to find <paramref name="o"/>.</remarks>
  public bool Contains(object o) { return Find(o)!=null; }

  /// <summary>Appends a linked list node to the end of the linked list.</summary>
  /// <param name="newNode">The <see cref="Node"/> to add to the list.</param>
  /// <returns>Returns <paramref name="newNode"/>.</returns>
  public Node Append(Node newNode)
  { 
    #if DEBUG
    if(newNode.Next!=null || newNode.Prev!=null)
      throw new ArgumentException("The given node is already part of a linked list.");
    #endif
    if(tail==null)
    { count=1;
      newNode.Next=newNode.Prev=null;
      return head=tail=newNode;
    }
    else return InsertAfter(tail, newNode);
  }
  /// <summary>Prepends a linked list node to the beginning of the linked list.</summary>
  /// <param name="newNode">The <see cref="Node"/> to add to the list.</param>
  /// <returns>Returns <paramref name="newNode"/>.</returns>
  public Node Prepend(Node newNode)
  { 
    #if DEBUG
    if(newNode.Next!=null || newNode.Prev!=null)
      throw new ArgumentException("The given node is already part of a linked list.");
    #endif
    if(head==null)
    { count=1;
      newNode.Next=newNode.Prev=null;
      return head=tail=newNode;
    }
    else return InsertBefore(head, newNode);
  }
  /// <summary>Finds the <see cref="Node"/> associated with the given object.</summary>
  /// <param name="o">The object to find.</param>
  /// <returns>The <see cref="Node"/> associated with the given object, or null if the object could not be found in
  /// the list.
  /// </returns>
  /// <remarks>The <see cref="IComparer"/> given to the constructor is used to find <paramref name="o"/>.</remarks>
  public Node Find(object o) { return Find(o, head); }
  /// <summary>Finds the <see cref="Node"/> associated with the specified object, starting from a given node.</summary>
  /// <param name="o">The object to find.</param>
  /// <param name="start">The <see cref="Node"/> at which the search will start.</param>
  /// <returns>The <see cref="Node"/> associated with the given object, or null if the object could not be found
  /// in the list.
  /// </returns>
  /// <remarks>The <see cref="IComparer"/> given to the constructor is used to find <paramref name="o"/>.</remarks>
  public Node Find(object o, Node start)
  { while(start!=null && cmp.Compare(start.Data, o)!=0) start=start.Next;
    return start;
  }
  /// <summary>Finds the <see cref="Node"/> associated with the given object, searching backwards from the end of the
  /// list.
  /// </summary>
  /// <param name="o">The object to find.</param>
  /// <returns>The <see cref="Node"/> associated with the given object, or null if the object could not be found in
  /// the list.
  /// </returns>
  /// <remarks>The <see cref="IComparer"/> given to the constructor is used to find <paramref name="o"/>. This search
  /// works backwards from the end of the list.
  /// </remarks>
  public Node FindLast(object o) { return Find(o, tail); }
  /// <summary>Finds the <see cref="Node"/> associated with the specified object, searching backwards from a given
  /// node.
  /// </summary>
  /// <param name="o">The object to find.</param>
  /// <param name="end">The <see cref="Node"/> at which the search will start.</param>
  /// <returns>The <see cref="Node"/> associated with the given object, or null if the object could not be found in
  /// the list.
  /// </returns>
  /// <remarks>The <see cref="IComparer"/> given to the constructor is used to find <paramref name="o"/>. This search
  /// works backwards through the list.
  /// </remarks>
  public Node FindLast(object o, Node end)
  { while(end!=null && cmp.Compare(end.Data, o)!=0) end=end.Prev;
    return end;
  }
  /// <summary>Returns true if the list contains the specified node.</summary>
  /// <param name="node">The <see cref="Node"/> to search for.</param>
  /// <returns>True if the list contains the given node and false otherwise.</returns>
  public bool Contains(Node node)
  { for(Node test=head; test!=null; test=test.Next) if(test==node) return true;
    return false;
  }
  /// <summary>Inserts the given object after the specified <see cref="Node"/>.</summary>
  /// <param name="node">The <see cref="Node"/> after which <paramref name="o"/> will be inserted.</param>
  /// <param name="o">The object to insert into the list.</param>
  /// <returns>The new <see cref="Node"/> associated with <paramref name="o"/>.</returns>
  public Node InsertAfter(Node node, object o) { return InsertAfter(node, new Node(o)); }
  /// <summary>Inserts the given node after the specified <see cref="Node"/>.</summary>
  /// <param name="node">The <see cref="Node"/> after which <paramref name="o"/> will be inserted.</param>
  /// <param name="newNode">The <see cref="Node"/> to insert into the linked list.</param>
  /// <returns>Returns <paramref name="newNode"/>.</returns>
  public Node InsertAfter(Node node, Node newNode)
  { 
    #if DEBUG
    if(newNode.Next!=null || newNode.Prev!=null)
      throw new ArgumentException("The given node is already part of a linked list.");
    #endif
    if(node==null || newNode==null) throw new ArgumentNullException();
    newNode.Prev = node;
    newNode.Next = node.Next;
    node.Next    = newNode;
    if(node==tail) tail=newNode;
    else newNode.Next.Prev=newNode;
    count++;
    if(ListChanged!=null) ListChanged();
    return newNode;
  }
  /// <summary>Inserts the given object before the specified <see cref="Node"/>.</summary>
  /// <param name="node">The <see cref="Node"/> before which <paramref name="o"/> will be inserted.</param>
  /// <param name="o">The object to insert into the list.</param>
  /// <returns>The new node associated with <paramref name="o"/>.</returns>
  public Node InsertBefore(Node node, object o) { return InsertBefore(node, new Node(o)); }
  /// <summary>Inserts the given node before the specified <see cref="Node"/>.</summary>
  /// <param name="node">The <see cref="Node"/> before which <paramref name="o"/> will be inserted.</param>
  /// <param name="newNode">The <see cref="Node"/> to insert into the linked list.</param>
  /// <returns>Returns <paramref name="newNode"/>.</returns>
  public Node InsertBefore(Node node, Node newNode)
  { 
    #if DEBUG
    if(newNode.Next!=null || newNode.Prev!=null)
      throw new ArgumentException("The given node is already part of a linked list.");
    #endif
    if(node==null || newNode==null) throw new ArgumentNullException();
    newNode.Prev = node.Prev;
    newNode.Next = node;
    node.Prev    = newNode;
    if(node==head) head=newNode;
    else newNode.Prev.Next=newNode;
    count++;
    if(ListChanged!=null) ListChanged();
    return newNode;
  }
  /// <summary>Removes the given <see cref="Node"/> from the linked list.</summary>
  /// <param name="node">The <see cref="Node"/> to remove from the linked list.</param>
  public void Remove(Node node)
  { if(node==null) return;
    if(node==head) head=node.Next;
    else node.Prev.Next=node.Next;
    if(node==tail) tail=node.Prev;
    else node.Next.Prev=node.Prev;
    count--;
    if(ListChanged!=null) ListChanged();
    #if DEBUG
    node.Next = node.Prev = null;
    #endif
  }
  /// <summary>Removes all elements from the linked list.</summary>
  public void Clear() { head=tail=null; count=0; }

  IComparer cmp;
  Node head, tail;
  int count;
}

} // namespace GameLib.Collections
