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
  { if(Count==0) throw new InvalidOperationException("The collection is empty");
    Node n=Root;
    do n=n.Left; while(n.Left!=Node.Null);
    Remove(n);
    return n.Value;
  }

  public object RemoveMaximum()
  { if(Count==0) throw new InvalidOperationException("The collection is empty");
    Node n=Root;
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
