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

  public class Node
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

  public Node Append(object o)  { return tail==null ? head=tail=new Node(o) : InsertAfter(tail, o); }
  public Node Prepend(object o) { return head==null ? head=tail=new Node(o) : InsertBefore(head, o); }
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
  public Node InsertAfter(Node node, object o)
  { if(node==null) throw new ArgumentNullException("node");
    node.Next = new Node(o, node, node.Next);
    if(node==tail) tail=node.Next;
    count++;
    if(ListChanged!=null) ListChanged();
    return node.Next;
  }
  public Node InsertBefore(Node node, object o)
  { if(node==null) throw new ArgumentNullException("node");
    node.Prev = new Node(o, node.Prev, node);
    if(node==head) head=node.Prev;
    count++;
    if(ListChanged!=null) ListChanged();
    return node.Prev;
  }
  public void Remove(Node node)
  { if(node==null) return;
    if(node.Prev!=null) node.Prev.Next=node.Next;
    if(node.Next!=null) node.Next.Prev=node.Prev;
    if(node==head) head=node.Next;
    if(node==tail) tail=node.Prev;
    count--;
    if(ListChanged!=null) ListChanged();
  }
  public void Clear() { head=tail=null; count=0; }
  
  protected IComparer cmp;
  protected Node head, tail;
  protected int count;
}

} // namespace GameLib.Collections
