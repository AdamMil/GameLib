using System;
using System.Collections;

namespace GameLib.Collections
{

public class PriorityQueue : RedBlackTree
{ 
  #region IDictionary
  public new object this[object key]
  { get { CheckKey(key); return base[key]; }
    set { CheckKey(key); base[key]=value; }
  }
  
  public new void Add(object key, object value)
  { CheckKey(key);
    base.Add(key, value);
  }
  #endregion

  public void Add(int priority, object value) { base.Add(priority, value); }

  public object RemoveMinimum()
  { if(count==0) throw new InvalidOperationException("The collection is empty");
    Node n=root;
    do n=n.Left; while(n.Left!=Node.Null);
    Remove(n);
    return n.Value;
  }

  public object RemoveMaximum()
  { if(count==0) throw new InvalidOperationException("The collection is empty");
    Node n=root;
    do n=n.Right; while(n.Right!=Node.Null);
    Remove(n);
    return n.Value;
  }

  public void SetPriority(int oldPriority, int newPriority)
  { if(Contains(newPriority))
      throw new ArgumentException("Tree already contains the priority "+newPriority, "newPriority");
    Node n = Find(oldPriority);
    if(n==Node.Null) throw new ArgumentException("The old priority does not exist in the tree", "oldPriority");
    Remove(n);
    Add(newPriority, n.Value);
  }

  protected void CheckKey(object key)
  { if(!(key is int)) throw new ArgumentException("Key must be an integer");
  }
}

} // namespace GameLib.Collections
