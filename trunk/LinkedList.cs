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

namespace GameLib.Collections
{

public class LinkedList : ICollection, IEnumerable
{ public LinkedList() { cmp=Comparer.Default; }
  public LinkedList(IComparer comparer) { cmp=comparer; }
  public LinkedList(ICollection col) { foreach(object o in col) Append(o); }
  public LinkedList(IComparer comparer, ICollection col)
  { cmp=comparer;
    foreach(object o in col) Append(o);
  }

  public sealed class Node
  { internal Node(object data) { this.data=data; }
    internal Node(object data, Node prev, Node next) { this.data=data; Prev=prev; Next=next; }
    public Node   PrevNode { get { return Prev; } }
    public Node   NextNode { get { return Next; } }
    public object Data { get { return data; } set { data=value; } }
    internal Node Prev, Next;
    object data;
  }

  #region ICollection
  public int    Count          { get { return count; } }
  public bool   IsSynchronized { get { return false; } }
  public object SyncRoot       { get { return this; } }
  
  public void CopyTo(Array array, int startIndex)
  { Node n = head;
    while(head!=null) array.SetValue(head.Data, startIndex++);
  }
  #endregion

  #region IEnumerable
  public class Enumerator : IEnumerator
  { internal Enumerator(LinkedList list, Node head)
    { if(head==null) GC.SuppressFinalize(this);
      else
      { handler = new ListChangeHandler(OnListChanged);
        list.ListChanged += handler;
        this.list=list;
      }
      this.head=head;
      Reset();
    }
    ~Enumerator() { list.ListChanged -= handler; }

    public object Current
    { get
      { if(cur==null) throw new InvalidOperationException("Invalid position");
        return cur.Data;
      }
    }
    
    public bool MoveNext()
    { if(changed) throw new InvalidOperationException("The collection has changed");
      if(cur==null)
      { if(!reset || head==null) return false;
        cur   = head;
        reset = false;
        return true;
      }
      cur = cur.Next;
      return cur!=null;
    }

    public void Reset()
    { cur=null; reset=true;
    }
    
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
  
  IEnumerator IEnumerable.GetEnumerator() { return new Enumerator(this, head); }

  internal delegate void ListChangeHandler();
  internal event ListChangeHandler ListChanged;
  #endregion
  
  public Node Head  { get { return head; } }
  public Node Tail  { get { return tail; } }

  public Node Append(object o)
  { if(tail==null) { AssertEmpty(); count=1; return head=tail=new Node(o); }
    else return InsertAfter(tail, new Node(o));
  }
  public Node Prepend(object o)
  { if(head==null) { AssertEmpty(); count=1; return head=tail=new Node(o); }
    else return InsertBefore(head, new Node(o));
  }
  public Node InsertAfter(object at, object o)
  { Node n = Find(at);
    if(n==null) throw new ArgumentException("Object not found in list", "at");
    return InsertAfter(n, o);
  }
  public Node InsertBefore(object at, object o)
  { Node n = Find(at);
    if(n==null) throw new ArgumentException("Object not found in list", "at");
    return InsertBefore(n, o);
  }
  public void Remove(object o) { Remove(Find(o)); }
  public bool Contains(object o) { return Find(o)!=null; }

  public Node Append(Node newNode)
  { if(tail==null)
    { AssertEmpty();
      count=1;
      newNode.Next=newNode.Prev=null;
      return head=tail=newNode;
    }
    else return InsertAfter(tail, newNode);
  }
  public Node Prepend(Node newNode)
  { if(head==null)
    { AssertEmpty();
      count=1;
      newNode.Next=newNode.Prev=null;
      return head=tail=newNode;
    }
    else return InsertBefore(head, newNode);
  }
  public Node Find(object o) { return Find(o, head); }
  public Node Find(object o, Node start)
  { while(start!=null && cmp.Compare(start.Data, o)!=0) start=start.Next;
    return start;
  }
  public Node FindLast(object o) { return Find(o, tail); }
  public Node FindLast(object o, Node end)
  { while(end!=null && cmp.Compare(end.Data, o)!=0) end=end.Prev;
    return end;
  }
  public bool Contains(Node node)
  { for(Node test=head; test!=null; test=test.Next) if(test==node) return true;
    return false;
  }
  public Node InsertAfter(Node node, object o) { return InsertAfter(node, new Node(o)); }
  public Node InsertAfter(Node node, Node newNode)
  { if(node==null || newNode==null) throw new ArgumentNullException();
    AssertIn(node);
    AssertOut(newNode);
    Validate();
    newNode.Prev = node;
    newNode.Next = node.Next;
    node.Next    = newNode;
    if(node==tail) tail=newNode;
    else node.Next.Prev=newNode;
    count++;
    if(ListChanged!=null) ListChanged();
    AssertIn(node);
    AssertIn(newNode);
    Validate();
    return newNode;
  }
  public Node InsertBefore(Node node, object o) { AssertIn(node); return InsertBefore(node, new Node(o)); }
  public Node InsertBefore(Node node, Node newNode)
  { if(node==null || newNode==null) throw new ArgumentNullException();
    AssertIn(node);
    AssertOut(newNode);
    Validate();
    newNode.Prev = node.Prev;
    newNode.Next = node;
    node.Prev    = newNode;
    if(node==head) head=newNode;
    else node.Prev.Next=newNode;
    count++;
    if(ListChanged!=null) ListChanged();
    AssertIn(node);
    AssertIn(newNode);
    Validate();
    return newNode;
  }
  public void Remove(Node node)
  { if(node==null) return;
    AssertIn(node);
    Validate();
    if(node==head) head=node.Next;
    else node.Prev.Next=node.Next;
    if(node==tail) tail=node.Prev;
    else node.Next.Prev=node.Prev;
    count--;
    if(ListChanged!=null) ListChanged();
    AssertOut(node);
    Validate();
  }
  public void Clear() { head=tail=null; count=0; }
  
  public void AssertEmpty()
  { if(count!=0 || head!=null || tail!=null) throw new Exception("test5");
  }

  public void AssertIn(Node node)
  { /*Node tn = head;
    while(tn!=null) { if(tn==node) return; tn=tn.Next; }*/
    if(!Contains(node)) throw new Exception("test1");
  }

  public void AssertOut(Node node)
  { Node tn = head;
    while(tn!=null) { if(tn==node) throw new Exception("test2"); tn=tn.Next; }
  }
  
  public void Validate()
  { if(head!=null && head.Prev!=null || tail!=null && tail.Next!=null) throw new Exception("test6");
    Hashtable hash = new Hashtable();
    Node tn = head;
    int c=0;
    while(tn!=null)
    { if(hash.Contains(tn) || hash.Contains(tn.Data)) throw new Exception("test3");
      hash[tn]=true;
      hash[tn.Data]=true;
      c++;
      tn=tn.Next;
    }
    if(c!=count) throw new Exception("test4");
  }

  protected IComparer cmp;
  protected Node head, tail;
  protected int count;
}

} // namespace GameLib.Collections
