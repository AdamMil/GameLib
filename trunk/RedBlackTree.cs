using System;
using System.Collections;

namespace GameLib.Collections
{

public class RedBlackTree : IDictionary, ICollection, IEnumerable
{ public RedBlackTree() { comparer=Comparer.Default; }
  public RedBlackTree(IComparer comparer) { this.comparer=comparer; }
  public RedBlackTree(IDictionary dict) { foreach(DictionaryEntry de in dict) Add(de.Key, de.Value); }
  public RedBlackTree(IDictionary dict, IComparer comparer)
  { this.comparer=comparer;
    foreach(DictionaryEntry de in dict) Add(de.Key, de.Value);
  }

  #region IDictionary
  public bool IsFixedSize { get { return false; } }
  public bool IsReadOnly  { get { return false; } }
  public object this[object key]
  { get { Node n=Find(key); return n==Node.Null ? null : n.Value; }
    set { CheckWriteable(); Node n=Find(key); if(n==Node.Null) Add(key, value); else n.Value=value; }
  }
  public ICollection Keys
  { get
    { object[] array = new object[Count];
      int i=0;
      foreach(DictionaryEntry de in this) array[i++] = de.Key;
      return array;
    }
  }
  public ICollection Values
  { get
    { object[] array = new object[Count];
      int i=0;
      foreach(DictionaryEntry de in this) array[i++] = de.Value;
      return array;
    }
  }
  
  public void Add(object key, object value)
  { if(key==null) throw new ArgumentNullException("key");
    CheckWriteable();

    Node x=root, y=Node.Null, gp;
    int  c=1;

    while(x!=Node.Null)
    { y=x;
      c=comparer.Compare(key, x.Key);
      if(c<0) x=x.Left;
      else if(c>0) x=x.Right;
      else break;
    }
    if(c==0) throw new ArgumentException("Key already exists in collection");
    
    x=new Node(y, key, value);
    if(y!=Node.Null)
    { c=comparer.Compare(key, y.Key);
      if(c<0) y.Left=x;
      else y.Right=x;
    }
    else root=x;
    
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
    if(TreeChanged!=null) TreeChanged();
  }

  public void Clear() { root=Node.Null; }
  public bool Contains(object key) { return Find(key)!=Node.Null; }
  IDictionaryEnumerator IDictionary.GetEnumerator() { return new Enumerator(this, root); }
  public void Remove(object key) { CheckWriteable(); Remove(Find(key)); }
  #endregion

  #region ICollection
  public int    Count          { get { return count; } }
  public bool   IsSynchronized { get { return false; } }
  public object SyncRoot       { get { return this; } }
  
  public void CopyTo(Array array, int startIndex)
  { foreach(DictionaryEntry de in this) array.SetValue(de, startIndex++);
  }
  #endregion

  #region IEnumerable
  public class Enumerator : IDictionaryEnumerator
  { internal Enumerator(RedBlackTree tree, Node root)
    { if(root==Node.Null) GC.SuppressFinalize(this);
      else
      { handler = new TreeChangeHandler(OnTreeChanged);
        tree.TreeChanged += handler;
        this.tree=tree;
      }
      this.root=root;
      Reset();
    }
    ~Enumerator() { tree.TreeChanged -= handler; }

    public object Current
    { get
      { if(states.Count==0) throw new InvalidOperationException("Invalid position");
        State state = (State)states.Peek();
        return new DictionaryEntry(state.node.Key, state.node.Value);
      }
    }
    
    public DictionaryEntry Entry
    { get
      { if(changed) throw new InvalidOperationException("The collection has changed");
        if(states.Count==0) throw new InvalidOperationException("Invalid position");
        State state = (State)states.Peek();
        return new DictionaryEntry(state.node.Key, state.node.Value);
      }
    }
    
    public object Key
    { get
      { if(changed) throw new InvalidOperationException("The collection has changed");
        if(states.Count==0) throw new InvalidOperationException("Invalid position");
        return ((State)states.Peek()).node.Key;
      }
    }
    
    public object Value
    { get
      { if(changed) throw new InvalidOperationException("The collection has changed");
        if(states.Count==0) throw new InvalidOperationException("Invalid position");
        return ((State)states.Peek()).node.Value;
      }
    }

    public bool MoveNext()
    { if(changed) throw new InvalidOperationException("The collection has changed");
      if(states.Count==0)
      { if(!reset || root==Node.Null) return false;
        states.Push(new State(root));
        reset = false;
        return true;
      }

      State state = (State)states.Peek();
      switch(state.did)
      { case Did.This:
          if(state.node.Left!=Node.Null)
          { states.Push(new State(state.node.Left));
            state.did = Did.Left;
            break;
          }
          else goto left;
        case Did.Left: left:
          if(state.node.Right!=Node.Null)
          { states.Push(new State(state.node.Right));
            state.did = Did.Right;
            break;
          }
          else goto right;
        case Did.Right: right:
          do
          { states.Pop();
            if(states.Count==0) break;
            state = (State)states.Peek();
          } while(state.did==Did.Right || state.node.Right==Node.Null);
          return states.Count>0;
      }
      return true;
    }

    public void Reset()
    { states = new Stack();
      reset  = true;
    }

    void OnTreeChanged()
    { changed=true;
      tree.TreeChanged -= handler;
      GC.SuppressFinalize(this);
    }

    enum Did { This, Left, Right }
    class State
    { public State(Node node) { this.node=node; did=Did.This; }
      public Node node;
      public Did  did;
    }

    RedBlackTree tree;
    TreeChangeHandler handler;
    Node  root;
    Stack states;
    bool  reset, changed;
  }
  
  IEnumerator IEnumerable.GetEnumerator() { return new Enumerator(this, root); }

  internal delegate void TreeChangeHandler();
  internal event TreeChangeHandler TreeChanged;
  #endregion

  public object Lookup(object key)
  { Node n = Find(key);
    if(n==Node.Null) throw new ArgumentException("The specified key does not exist in the collection", "key");
    return n.Value;
  }
  
  public DictionaryEntry Minimum()
  { if(count==0) throw new InvalidOperationException("The collection is empty");
    Node n=root;
    do n=n.Left; while(n.Left!=Node.Null);
    return new DictionaryEntry(n.Key, n.Value);
  }
  
  public DictionaryEntry Maximum()
  { if(count==0) throw new InvalidOperationException("The collection is empty");
    Node n=root;
    do n=n.Right; while(n.Right!=Node.Null);
    return new DictionaryEntry(n.Key, n.Value);
  }

  #region Node class
  internal protected class Node
  { public Node() { }
    public Node(Node parent, object key, object value)
    { Parent=parent; Key=key; Value=value; Red=true;
      Left=Right=nullNode;
    }
    public Node   Parent, Left, Right;
    public object Key, Value;
    public bool   Red;
    public static Node Null { get { return nullNode; } }
    static Node nullNode = new Node();
  }
  #endregion

  protected void CheckWriteable()
  { if(IsReadOnly || IsFixedSize)
      throw new NotSupportedException("Collection is "+(IsReadOnly?"read-only":"fixed-size"));
  }

  protected void LeftRotate(Node x)
  { Node y=x.Right;
    if((x.Right=y.Left)!=Node.Null) y.Left.Parent=x;
    if((y.Parent=x.Parent)!=Node.Null)
    { if(x==x.Parent.Left) x.Parent.Left=y;
      else x.Parent.Right=y;
    }
    else root=y;
    y.Left=x; x.Parent=y;
  }

  protected void RightRotate(Node y)
  { Node x=y.Left;
    if((y.Left=x.Right)!=Node.Null) x.Right.Parent=y;
    if((x.Parent=y.Parent)!=Node.Null)
    { if(y==y.Parent.Left) y.Parent.Left=x;
      else y.Parent.Right=x;
    }
    else root=x;
    x.Right=y; y.Parent=x;
  }
  
  protected Node Find(object key)
  { if(key==null) throw new ArgumentNullException("key");

    Node n=root;
    int  c;
    while(n!=Node.Null)
    { c=comparer.Compare(key, n.Key);
      if(c<0) n=n.Left;
      else if(c>0) n=n.Right;
      else break;
    }
    return n;
  }
  
  protected void Remove(Node n)
  { if(n==Node.Null) return;
    Node w, x, y, p;
    y = n.Left==Node.Null || n.Right==Node.Null ? n : Successor(n);
    x = y.Left==Node.Null ? y.Right : y.Left;
    
    if((x.Parent=y.Parent)==Node.Null) root=x;
    else if(y==y.Parent.Left) y.Parent.Left=x;
    else y.Parent.Right=x;
    
    if(y!=n) { n.Key=y.Key; n.Value=y.Value; }
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
    if(TreeChanged!=null) TreeChanged();
  }

  protected Node Successor(Node x)
  { Node y;
    if(x.Right!=Node.Null) for(y=x.Right; y.Left!=Node.Null; y=y.Left);
    else for(y=x.Parent; y!=Node.Null && x==y.Right; x=y, y=y.Parent);
    return y;
  }

  protected Node root=Node.Null;
  protected IComparer comparer;
  protected int  count;
}

} // namespace GameLib.Collections
