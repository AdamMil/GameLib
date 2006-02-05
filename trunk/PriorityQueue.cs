/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2005 Adam Milazzo

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
using System.Collections.Generic;

namespace GameLib.Collections
{

/// <summary>This class represents a priority queue.</summary>
/// <remarks>
/// <para>A priority queue works like a standard <see cref="Queue"/>, except that the items are ordered by a
/// predicate. The value with the highest priority will always be dequeued first. The object with the highest priority
/// is the object with the greatest value, as determined by the <see cref="IComparer"/> used to initialize the queue.
/// Multiple objects with the same priority level can be added to the queue.
/// </para>
/// <para>The priority queue is implemented using a heap, which is a very efficient array structure that makes finding
/// the highest priority item very fast (an O(1) operation), but makes finding the lowest priority item rather
/// slow (an O(n) operation).
/// </para>
/// </remarks>
[Serializable]
public sealed class PriorityQueue<T> : ICollection<T>
{ 
  /// <summary>Initializes a new, empty instance of the <see cref="PriorityQueue"/> class, with a default capacity and
  /// using <see cref="Comparer<>.Default"/> to compare elements.
  /// </summary>
  public PriorityQueue() : this(Comparer<T>.Default, 0) { }
  /// <summary>Initializes a new, empty instance of the <see cref="PriorityQueue"/> class, with the specified capacity
  /// and using <see cref="Comparer<>.Default"/> to compare elements.
  /// </summary>
  /// <param name="capacity">The initial capacity of the queue.</param>
  public PriorityQueue(int capacity) : this(Comparer<T>.Default, capacity) { }
  /// <summary>Initializes a new, empty instance of the <see cref="PriorityQueue"/> class, with a default capacity
  /// and using the specified <see cref="IComparer<>"/> to compare elements.
  /// </summary>
  /// <param name="comparer">The <see cref="IComparer<>"/> that will be used to compare elements.</param>
  public PriorityQueue(IComparer<T> comparer) : this(comparer, 0) { }
  /// <summary>Initializes a new, empty instance of the <see cref="PriorityQueue"/> class, with the specified capacity
  /// and using the given <see cref="IComparer"/> to compare elements.
  /// </summary>
  /// <param name="comparer">The <see cref="IComparer<>"/> that will be used to compare elements.</param>
  /// <param name="capacity">The initial capacity of the queue.</param>
  public PriorityQueue(IComparer<T> comparer, int capacity)
  { if(comparer==null) throw new ArgumentNullException("comparer");
    if(capacity<0) throw new ArgumentOutOfRangeException("capacity", capacity, "capacity must not be negative");
    cmp = comparer;
    array = new List<T>(capacity);
  }

  /// <summary>Gets or sets the number of elements that the internal array can contain.</summary>
  public int Capacity
  { get { return array.Capacity; }
    set { array.Capacity = value; }
  }

  /// <summary>Removes and returns the element in the queue with the highest priority.</summary>
  /// <returns>The element in the queue with the highest priority.</returns>
  /// <exception cref="InvalidOperationException">Thrown if the collection is empty.</exception>
  public T Dequeue()
  { if(Count==0) throw new InvalidOperationException("The collection is empty.");
    T max = array[0], tmp;
    int i=0, li=1, ri=2, count=Count-1, largest;

    array[0] = array[count];
    array.RemoveAt(count);

    while(true) // heapify 'array'
    { largest = li<count && cmp.Compare(array[li], array[i])>0 ? li : i;
      if(ri<count && cmp.Compare(array[ri], array[largest])>0) largest = ri;
      if(largest==i) break;
      tmp=array[i]; array[i]=array[largest]; array[largest]=tmp;
      i=largest; ri=(i+1)*2; li=ri-1; // i=largest, ri=Right(i), li=Left(i)
    }
    return max;
  }

  /// <summary>Adds an item to the queue.</summary>
  /// <param name="value">The item to add to the queue.</param>
  public void Enqueue(T value)
  { array.Add(value);
    int i = Count, ip;
    while(i>1) // heapify 'array'
    { ip = i/2;  // i=Parent(i)
      if(cmp.Compare(array[ip-1], value)>=0) break;
      array[i-1] = array[ip-1];
      i = ip;
    }
    array[i-1] = value;
  }

  /// <summary>Returns the element in the queue with the highest priority.</summary>
  /// <remarks>This method does not modify the queue.</remarks>
  /// <exception cref="InvalidOperationException">Thrown if the collection is empty.</exception>
  public T Peek()
  { if(Count==0) throw new InvalidOperationException("The collection is empty.");
    return array[0];
  }

  /// <summary>Shrinks the capacity to the actual number of elements in the priority queue.</summary>
  public void TrimExcess() { array.TrimExcess(); }

  #region ICollection<>
  /// <summary>Gets the number of elements contained in the priority queue.</summary>
  public int Count { get { return array.Count; } }
  /// <summary>Gets a value indicating whether access to the queue is read-only.</summary>
  /// <remarks>See the <see cref="ICollection.IsReadOnly"/> property for more information.
  /// <seealso cref="ICollection<>.IsReadOnly"/>
  /// </remarks>
  public bool IsReadOnly { get { return false; } }

  void ICollection<T>.Add(T item) { Enqueue(item); }
  /// <summary>Removes all elements from the priority queue.</summary>
  public void Clear() { array.Clear(); }
  /// <summary>Returns whether an item exists in the queue.</summary>
  /// <param name="item">The item to search for.</param>
  /// <returns>True if the item exists in the queue and false otherwise.</returns>
  public bool Contains(T item) { return array.Contains(item); }
  /// <summary>Copies the queue elements to an existing one-dimensional Array, starting at the specified array index.</summary>
  /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied
  /// from the queue.
  /// </param>
  /// <param name="startIndex">The zero-based index in array at which copying begins.</param>
  /// <remarks>See <see cref="ICollection.CopyTo"/> for more information. <seealso cref="ICollection.CopyTo"/></remarks>
  public void CopyTo(T[] array, int startIndex) { this.array.CopyTo(array, startIndex); }
  /// <summary>Removes an item from the queue.</summary>
  /// <param name="item">The item to remove.</param>
  /// <remarks>Removing an item in this fashion is not efficient.</remarks>
  public bool Remove(T item)
  { int index = array.IndexOf(item);
    if(index==-1) return false;
    throw new NotImplementedException();
    array.RemoveAt(index);
    Heapify(index); // TODO: double check this
    return true;
  }
  #endregion

  #region IEnumerable
  /// <summary>Returns an enumerator that can iterate through the queue.</summary>
  /// <returns>An <see cref="IEnumerator"/> that can be used to iterate through the queue.</returns>
  System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
  { return ((System.Collections.IEnumerable)array).GetEnumerator();
  }
  #endregion

  #region IEnumerable<T>
  /// <summary>Returns an enumerator that can iterate through the queue.</summary>
  /// <returns>An <see cref="IEnumerator"/> that can be used to iterate through the queue.</returns>
  public IEnumerator<T> GetEnumerator() { return array.GetEnumerator(); }
  #endregion

  void Heapify(int i)
  { T tmp;
    int li, ri, largest, count=Count;
    while(true)
    { ri=(i+1)*2; li=ri-1; // ri=Right(i), li=Left(i)
      largest = li<count && cmp.Compare(array[li], array[i])>0 ? li : i;
      if(ri<count && cmp.Compare(array[ri], array[largest])>0) largest=ri;
      if(largest==i) break;
      tmp=array[i]; array[i]=array[largest]; array[largest]=tmp;
      i = largest;
    }
  }

  List<T> array; // slightly less efficient than managing an array ourselves, but makes for much simpler code
  IComparer<T> cmp;
}

} // namespace GameLib.Collections
