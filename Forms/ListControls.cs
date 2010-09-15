/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2007 Adam Milazzo

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

// TODO: add documentation
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using GameLib.Video;
using GameLib.Input;
using Font = GameLib.Fonts.Font;
using Timer = System.Windows.Forms.Timer;

namespace GameLib.Forms
{

#region ListControl
// TODO: implement multi-column list boxes
public abstract class ListControl : ScrollableControl
{
  protected ListControl()
  {
    items = new ItemCollection(this);
  }

  protected ListControl(System.Collections.IEnumerable items)
  {
    this.items = new ItemCollection(this, items);
    OnListChanged();
  }

  #region ItemCollection
  public sealed class ItemCollection : System.Collections.IList
  {
    public ItemCollection(ListControl parent)
    {
      this.parent = parent;
      list = new List<Item>();
    }

    public ItemCollection(ListControl parent, System.Collections.IEnumerable items)
    {
      this.parent = parent;
      list = new List<Item>();
      foreach(object o in items) list.Add(new Item(o));
    }

    #region Enumerator
    sealed class Enumerator : System.Collections.IEnumerator
    {
      public Enumerator(ItemCollection parent)
      {
        this.parent = parent;
        Reset();
      }

      public object Current
      {
        get
        {
          if(!haveItem) throw new InvalidOperationException();
          return current;
        }
      }

      public bool MoveNext()
      {
        if(version != parent.version) throw new InvalidOperationException();

        if(index >= parent.Count || ++index == parent.Count)
        {
          haveItem = false;
          return false;
        }

        current = parent[index];
        haveItem = true;
        return true;
      }

      public void Reset()
      {
        index = -1;
        version = parent.version;
        haveItem = false;
      }

      readonly ItemCollection parent;
      object current;
      int index, version;
      bool haveItem;
    }
    #endregion

    public object this[int index]
    {
      get { return list[index].Object; }
      set
      {
        if(sorted) throw new InvalidOperationException("You can't set items in a sorted list. Use Add() instead.");
        Item item = list[index];
        item.Object = value;
        list[index] = item;
        OnUpdated();
      }
    }

    public int Count { get { return list.Count; } }

    public bool IsFixedSize { get { return false; } }
    public bool IsReadOnly { get { return false; } }
    public bool IsSynchronized { get { return false; } }
    public object SyncRoot { get { return parent; } }

    public int Add(object item)
    {
      int index;
      if(sorted)
      {
        index = FindInsertionPoint(item);
        list.Insert(index, new Item(item));
      }
      else
      {
        list.Add(new Item(item));
        index = list.Count - 1;
      }
      OnUpdated();
      return index;
    }

    public void AddRange(params object[] items)
    {
      if(!sorted || items.Length > 3)
      {
        for(int i = 0; i < items.Length; i++) list.Add(new Item(items[i]));
        if(sorted) list.Sort(Comparer);
      }
      else
      {
        for(int i = 0; i < items.Length; i++)
        {
          Item item = new Item(items[i]);
          list.Insert(FindInsertionPoint(item), item);
        }
      }
      OnUpdated();
    }

    public void AddRange(System.Collections.IEnumerable items)
    {
      foreach(object o in items) list.Add(new Item(o));
      if(sorted) list.Sort(Comparer);
      OnUpdated();
    }

    public void Clear()
    {
      list.Clear();
      OnUpdated();
    }

    public bool Contains(object item) { return IndexOf(item) != -1; }

    public void CopyTo(object[] array, int index)
    {
      if(index < 0 || index + Count > array.Length) throw new ArgumentOutOfRangeException();
      foreach(Item item in list) array[index++] = item.Object;
    }

    public System.Collections.IEnumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    public void Insert(int index, object item)
    {
      if(sorted) throw new InvalidOperationException("Can't insert into a sorted list. Use Add()");
      else list.Insert(index, new Item(item));
      OnUpdated();
    }

    public int IndexOf(object item)
    {
      if(sorted)
      {
        int index = list.BinarySearch(new Item(item), Comparer);
        if(index < 0) return -1;
        string text = parent.GetObjectText(item);
        bool down = true, up = true;
        for(int i = 0, j, c = 0, len = list.Count; i < len; i++)
        {
          if(++c == 4) c = 0;
          if(down)
          {
            j = index - i;
            if(j >= 0)
            {
              object o = list[j].Object;
              if(o.Equals(item)) return j;
              if(c == 0 && parent.GetObjectText(o) != text)
              {
                down = false;
                if(!up) break;
              }
            }
          }

          if(up)
          {
            j = index + i + 1;
            if(j < len)
            {
              object o = list[j].Object;
              if(o.Equals(item)) return j;
              if(c == 0 && parent.GetObjectText(o) != text)
              {
                up = false;
                if(!down) break;
              }
            }
          }
        }
      }
      else for(int i = 0; i < list.Count; i++) if(list[i].Object.Equals(item)) return i;
      return -1;
    }

    public void Remove(object item)
    {
      int index = IndexOf(item);
      if(index != -1) list.RemoveAt(index);
    }

    public void RemoveAt(int index)
    {
      list.RemoveAt(index);
      OnUpdated();
    }

    internal int StateVersion
    {
      get { return stateVersion; }
    }
    
    internal int Version
    {
      get { return version; }
    }

    internal bool Sorted
    {
      get { return sorted; }
      set
      {
        if(value && !sorted)
        {
          list.Sort(Comparer);
          OnUpdated();
        }
        sorted = value;
      }
    }

    internal void StateUpdated()
    {
      stateVersion++;
      parent.OnListChanged();
      parent.Invalidate(parent.ContentRect);
    }

    void System.Collections.ICollection.CopyTo(Array array, int index)
    {
      if(index < 0 || index + Count > array.Length) throw new ArgumentOutOfRangeException();
      foreach(Item item in list) array.SetValue(item.Object, index++);
    }

    void OnUpdated()
    {
      version++;
      StateUpdated();
    }

    sealed class TextComparer : IComparer<Item>
    {
      public TextComparer(ListControl list) { this.list = list; }

      public int Compare(Item a, Item b)
      {
        return string.Compare(list.GetObjectText(a.Object), list.GetObjectText(b.Object),
                              StringComparison.CurrentCulture);
      }
      
      ListControl list;
    }

    TextComparer Comparer
    {
      get
      {
        if(comparer == null) comparer = new TextComparer(parent);
        return comparer;
      }
    }

    int FindInsertionPoint(object item)
    {
      int index = list.BinarySearch(new Item(item), Comparer);
      return index < 0 ? ~index : index;
    }

    internal List<Item> list;
    ListControl parent;
    TextComparer comparer;
    int version, stateVersion;
    bool sorted;
  }
  #endregion

  #region SelectedIndexCollection
  public sealed class SelectedIndexCollection : ICollection<int>
  {
    internal SelectedIndexCollection(ListControl list)
    {
      items   = list.Items;
      version = items.StateVersion - 1; // subtract one to force a reload the first time the collection is used
    }

    #region IndexEnumerator
    sealed class IndexEnumerator : IEnumerator<int>
    {
      public IndexEnumerator(SelectedIndexCollection indices)
      {
        this.indices = indices;
        count = indices.Count;
        version = indices.items.StateVersion;
        index = -1;
      }

      public void Dispose() { indices = null; }

      public int Current
      {
        get
        {
          if(index < 0 || index >= count) throw new InvalidOperationException();
          return current;
        }
      }

      object System.Collections.IEnumerator.Current
      {
        get
        {
          if(index < 0 || index >= count) throw new InvalidOperationException();
          return current;
        }
      }

      public bool MoveNext()
      {
        if(version != indices.items.StateVersion) throw new InvalidOperationException();
        if(index >= count - 1) return false;
        current = indices[++index];
        return true;
      }

      public void Reset()
      {
        if(version != indices.items.StateVersion) throw new InvalidOperationException();
        index = -1;
      }

      SelectedIndexCollection indices;
      int index, count, current, version;
    }
    #endregion

    public int this[int index] { get { return Indices[index]; } }
    public int Count { get { return Indices.Length; } }
    public bool IsReadOnly { get { return true; } }

    public void Add(int index) { throw new InvalidOperationException("SelectedIndexCollection is read only"); }
    public void Clear() { throw new InvalidOperationException("SelectedIndexCollection is read only"); }
    public bool Remove(int index) { throw new InvalidOperationException("SelectedIndexCollection is read only"); }

    public bool Contains(int index) { return IndexOf(index) != -1; }
    public void CopyTo(int[] array, int index) { Indices.CopyTo(array, index); }
    public IEnumerator<int> GetEnumerator() { return new IndexEnumerator(this); }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return new IndexEnumerator(this); }

    public int IndexOf(int index)
    {
      int[] array = Indices;
      for(int i = 0; i < array.Length; i++) if(array[i] == index) return i;
      return -1;
    }

    internal int[] Indices
    {
      get
      {
        if(version != items.StateVersion)
        {
          List<Item> list = items.list;
          List<int> sel = new List<int>();
          for(int i = 0; i < list.Count; i++) if(list[i].Is(ItemState.Selected)) sel.Add(i);
          indices = sel.ToArray();
          version = items.StateVersion;
        }
        return indices;
      }
    }

    ItemCollection items;
    int[] indices;
    int version;
  }
  #endregion

  #region SelectedObjectCollection
  public sealed class SelectedObjectCollection : System.Collections.ICollection
  {
    internal SelectedObjectCollection(ListControl list)
    {
      this.list = list;
      indices = list.SelectedIndices;
    }

    #region ObjectEnumerator
    sealed class ObjectEnumerator : System.Collections.IEnumerator
    {
      public ObjectEnumerator(SelectedObjectCollection objs) { this.objs = objs; indices = objs.indices.GetEnumerator(); }
      public object Current { get { return objs[indices.Current]; } }
      public bool MoveNext() { return indices.MoveNext(); }
      public void Reset() { indices.Reset(); }

      SelectedObjectCollection objs;
      IEnumerator<int> indices;
    }
    #endregion

    public object this[int index] { get { return list.items[indices.Indices[index]]; } }
    public int Count { get { return indices.Indices.Length; } }
    public bool IsSynchronized { get { return false; } }
    public object SyncRoot { get { return list; } }

    public bool Contains(object item) { return IndexOf(item) != -1; }
    public int IndexOf(object item)
    {
      int[] indices = this.indices.Indices;
      for(int i = 0; i < indices.Length; i++)
      {
        if(list.items[indices[i]] == item) return i;
      }
      return -1;
    }

    public void CopyTo(object[] array, int index)
    {
      int[] indices = this.indices.Indices;
      for(int i = 0; i < indices.Length; i++) array[i + index] = list.items[indices[i]];
    }

    public System.Collections.IEnumerator GetEnumerator() { return new ObjectEnumerator(this); }

    void System.Collections.ICollection.CopyTo(Array array, int index)
    {
      int[] indices = this.indices.Indices;
      for(int i = 0; i < indices.Length; i++) array.SetValue(list.items[indices[i]], i + index);
    }

    ListControl list;
    SelectedIndexCollection indices;
  }
  #endregion

  public event EventHandler SelectedIndexChanged;

  public ItemCollection Items
  {
    get { return items; }
  }
  
  public abstract int SelectedIndex { get; set; }

  public SelectedIndexCollection SelectedIndices
  {
    get
    {
      if(selectedIndices == null) selectedIndices = new SelectedIndexCollection(this);
      return selectedIndices;
    }
  }

  public object SelectedItem
  {
    get
    {
      int index = SelectedIndex;
      return index == -1 ? null : items[index];
    }
    set { SelectedIndex = items.IndexOf(value); }
  }

  public SelectedObjectCollection SelectedItems
  {
    get
    {
      if(selectedItems == null) selectedItems = new SelectedObjectCollection(this);
      return selectedItems;
    }
  }

  public bool Sorted
  {
    get { return items.Sorted; }
    set { items.Sorted = value; }
  }

  public override string Text
  {
    get { return SelectedIndex < 0 ? "" : GetItemText(SelectedIndex); }
    set { SelectedIndex = FindStringExact(value); }
  }

  public void ClearSelected()
  {
    int[] indices = SelectedIndices.Indices;
    if(indices.Length > 0)
    {
      for(int i = 0; i < indices.Length; i++)
      {
        Item it = items.list[indices[i]];
        it.State &= ~ItemState.Selected;
        items.list[indices[i]] = it;
      }
      items.StateUpdated();
      Invalidate(ContentRect);
    }
  }

  public int FindString(string startsWith)
  {
    return FindString(startsWith, 0);
  }
  
  public int FindString(string startsWith, int from)
  {
    for(; from < items.Count; from++) if(GetItemText(from).StartsWith(startsWith)) return from;
    return -1;
  }

  public int FindStringExact(string text)
  {
    return FindStringExact(text, 0);
  }
  
  public int FindStringExact(string text, int from)
  {
    for(; from < items.Count; from++) if(GetItemText(from) == text) return from;
    return -1;
  }

  public string GetItemText(int index)
  {
    return GetObjectText(items[index]);
  }

  public bool IsSelected(int index)
  {
    return Items.list[index].Is(ItemState.Selected);
  }
  
  public void SetSelected(int index, bool selected)
  {
    SetSelected(index, selected, false);
  }
  
  public void SetSelected(int index, bool selected, bool deselectOthers)
  {
    Item rit = items.list[index];
    bool was = rit.Is(ItemState.Selected);

    if(deselectOthers) ClearSelected();

    if(selected) rit.State |= ItemState.Selected;
    else rit.State &= ~ItemState.Selected;
    items.list[index] = rit;

    if(deselectOthers) Invalidate(ContentRect);
    else if(was != selected) InvalidateItem(index);
    else return;
    items.StateUpdated();
  }

  public void ToggleSelected(int index)
  {
    Item it = items.list[index];
    it.State ^= ItemState.Selected;
    items.list[index] = it;
    items.StateUpdated();
    InvalidateItem(index);
  }

  protected virtual string GetObjectText(object item)
  {
    return item.ToString();
  }
  
  protected virtual void InvalidateItem(int index) { }
  protected virtual void OnListChanged() { }
  
  protected virtual void OnSelectedIndexChanged()
  {
    if(SelectedIndexChanged != null) SelectedIndexChanged(this, EventArgs.Empty);
  }

  [Flags]
  protected internal enum ItemState // TODO: this Custom1-4 stuff is not very clean...
  {
    None = 0, Selected = 1, Disabled = 2, Checked = 4, Expanded = 8,
    Custom1 = 16, Custom2 = 32, Custom3 = 64, Custom4 = 128
  }

  protected internal struct Item
  {
    public Item(object item) { Object = item; State = ItemState.None; }
    public bool Is(ItemState state) { return (State & state) != 0; }
    public override string ToString() { return Object == null ? "" : Object.ToString(); }

    public object Object;
    public ItemState State;
  }

  ItemCollection items;
  SelectedIndexCollection selectedIndices;
  SelectedObjectCollection selectedItems;
}
#endregion

#region ListBox
public enum SelectionMode
{
  None, One, MultiSimple, MultiExtended
}

// TODO: optimize the scrolling so it doesn't redraw the entire box, but only the newly-uncovered portion
public class ListBox : ListControl
{
  public ListBox() { Init(); }
  public ListBox(System.Collections.IEnumerable items) : base(items) { Init(); }
  public ListBox(params object[] items) : base((System.Collections.IEnumerable)items) { Init(); }

  static ListBox()
  {
    scrollTimer = new Timer();
    scrollTimer.Interval = 25;
    scrollTimer.Tick += ScrollIt;
  }

  void Init()
  {
    ControlStyle     |= ControlStyle.CanReceiveFocus;
    BackColor         = Color.White;
    BorderColor       = SystemColors.ControlDarkDark;
    BorderStyle       = BorderStyle.FixedFlat;
    Padding           = new RectOffset(1);
    SelectedBackColor = SystemColors.Highlight;
    SelectedForeColor = SystemColors.HighlightText;
    FixedHeight       = true;

    cursor  = bottom = -1;
    selMode = SelectionMode.One;
  }

  public Color SelectedBackColor
  {
    get; set;
  }
  
  public Color SelectedForeColor
  {
    get; set;
  }

  public int CursorPosition
  {
    get { return cursor; }
    set
    {
      int newValue = value < -1 || value >= Items.Count ? -1 : value;
      if(newValue != cursor)
      {
        if(cursor >= 0) Invalidate(GetItemRectangle(cursor, true));
        if(newValue >= 0) Invalidate(GetItemRectangle(newValue, true));
        cursor = newValue;
      }
    }
  }

  protected bool FixedHeight
  {
    get; set;
  }

  public override int SelectedIndex
  {
    get
    {
      SelectedIndexCollection coll = SelectedIndices;
      return coll.Count > 0 ? coll[0] : -1;
    }
    set
    {
      int newValue = value < 0 || value >= Items.Count ? -1 : value;
      if(newValue != SelectedIndex || SelectedIndices.Count > 1)
      {
        string text = Text;

        if(newValue == -1) ClearSelected();
        else if(selMode == SelectionMode.One)
        {
          int old = SelectedIndex;
          if(old >= 0) SetSelected(old, false);
          if(newValue >= 0) SetSelected(newValue, true);
        }
        else SetSelected(newValue, true, true);

        OnSelectedIndexChanged();
        if(Text != text) OnTextChanged(new ValueChangedEventArgs(text));
      }
    }
  }

  public SelectionMode SelectionMode
  {
    get { return selMode; }
    set
    {
      if(value == SelectionMode.None) ClearSelected();
      else if(value == SelectionMode.One && SelectedIndex >= 0) SetSelected(SelectedIndex, true, true);
      selMode = value;
    }
  }

  public override string Text
  {
    get { return SelectedIndex < 0 ? "" : GetItemText(SelectedIndex); }
    set { SelectedIndex = FindStringExact(value); }
  }

  public int TopIndex
  {
    get { return top; }
    set
    {
      int newValue = Math.Max(Math.Min(value, lastTop), 0);
      if(newValue != top)
      {
        top = newValue;
        if(VerticalScrollBar != null) VerticalScrollBar.Value = newValue;
        Invalidate(ContentRect);
        bottom = Math.Max(Math.Min(value, Items.Count - 1), 0) == lastTop ? lastTop : -1;
      }
    }
  }

  public Size GetPreferredSize()
  {
    return GetPreferredSize(Items.Count);
  }
  
  public Size GetPreferredSize(int numItems)
  {
    if(numItems < 0 || numItems > Items.Count) throw new ArgumentOutOfRangeException();

    Size totalSize = Size.Empty;
    for(int i = 0; i < numItems; i++)
    {
      Size itemSize = MeasureItem(i);
      totalSize.Height += itemSize.Height;
      if(itemSize.Width > totalSize.Width) totalSize.Width = itemSize.Width;
    }

    return ContentOffset.Grow(totalSize);
  }

  public int PointToItem(Point controlPoint)
  {
    if(Items.Count != 0)
    {
      Rectangle bounds = ContentRect;
      if(FixedHeight)
      {
        if(!bounds.Contains(controlPoint)) return -1;
        int index = TopIndex + (controlPoint.Y - bounds.Y) / MeasureItem(0).Height;
        return index >= Items.Count ? -1 : index;
      }
      else
      {
        for(int i = TopIndex; i < Items.Count && bounds.Height > 0; i++)
        {
          Rectangle itemRect = new Rectangle(bounds.X, bounds.Y, bounds.Width, MeasureItem(i).Height);
          if(itemRect.Contains(controlPoint)) return i;
          bounds.Y += itemRect.Height; bounds.Height -= itemRect.Height;
        }
      }
    }
    return -1;
  }

  public void ScrollTo(int index)
  {
    if(index < TopIndex) TopIndex = index;
    else if(index > GetBottomIndex()) TopIndex = GetTopIndex(index);
  }

  protected virtual void DrawItem(int index, PaintEventArgs e, Rectangle bounds)
  {
    Color fore, back;

    if(IsSelected(index))
    {
      back = SelectedBackColor;
      fore = SelectedForeColor;
      if(back.A != 0) e.Target.FillArea(bounds, back);
    }
    else
    {
      back = GetEffectiveBackColor();
      fore = GetEffectiveForeColor();
    }
    if(!EffectivelyEnabled) fore = SystemColors.GrayText;

    if(EffectiveFont != null)
    {
      EffectiveFont.Color     = fore;
      EffectiveFont.BackColor = back;
      e.Target.DrawText(EffectiveFont, GetItemText(index), bounds.Location);
      if(index == CursorPosition) e.Renderer.DrawBox(e.Target, bounds, Color.FromArgb(128, fore));
    }
  }

  protected int FindChar(char c)
  {
    return FindChar(c, 0);
  }
  
  protected int FindChar(char c, int start)
  {
    c = char.ToUpper(c);
    for(; start < Items.Count; start++)
    {
      string s = GetItemText(start);
      if(s.Length > 0 && char.ToUpper(s[0]) == c) return start;
    }
    return -1;
  }

  /// <summary>Returns the index of the last item to be displayed, given the current value of <see cref="TopIndex"/>.</summary>
  protected int GetBottomIndex()
  {
    if(bottom == -1)
    {
      if(FixedHeight)
      {
        bottom = TopIndex - 1;
        if(TopIndex < Items.Count)
        {
          int height = MeasureItem(TopIndex).Height;
          bottom = Math.Min(bottom + (Height - ContentOffset.Vertical + height - 1) / height, Items.Count-1);
        }
      }
      else
      {
        Rectangle bounds = ContentRect;
        int i;
        for(i = TopIndex; i < Items.Count && bounds.Height > 0; i++) bounds.Height -= MeasureItem(i).Height;
        bottom = i - 1;
      }
    }
    return bottom;
  }

  protected Rectangle GetItemRectangle(int index, bool onlySeen)
  {
    if(index < 0 || index >= Items.Count) throw new ArgumentOutOfRangeException();
    if(onlySeen && index < TopIndex) return new Rectangle(-1, -1, 0, 0);

    Rectangle bounds = ContentRect;
    if(FixedHeight)
    {
      bounds.Height = MeasureItem(index).Height;
      bounds.Y += (index - TopIndex) * bounds.Height;
    }
    else if(index < TopIndex)
    {
      for(int i = TopIndex; i >= index; i--)
      {
        int height = MeasureItem(i).Height;
        bounds.Y -= height; bounds.Height = height;
      }
    }
    else
    {
      int i = TopIndex, end = bounds.Bottom;
      while(true)
      {
        int height = MeasureItem(i).Height;
        bounds.Height = height;
        if(i++ == index) return bounds;
        if(bounds.Y >= end) return new Rectangle(-1, -1, 0, 0);
        bounds.Y += height;
      }
    }
    return bounds;
  }

  /// <summary>Returns the index of the first item that would be displayed if the list box were scrolled all the way to the
  /// bottom.
  /// </summary>
  protected int GetLastTopIndex()
  {
    return GetTopIndex(Items.Count - 1);
  }

  /// <summary>Returns the index of the first item that would be displayed, given the index of the last item that would be
  /// displayed.
  /// </summary>
  protected int GetTopIndex(int bottom)
  {
    if(FixedHeight)
    {
      if(Items.Count == 0) return 0;
      int height = MeasureItem(0).Height;
      return Math.Max(0, bottom - (Height - ContentOffset.Vertical + height - 1) / height + 1);
    }
    else
    {
      Rectangle bounds = ContentRect;
      for(; bottom >= 0 && bounds.Height > 0; bottom--) bounds.Height -= MeasureItem(bottom).Height;
      return bottom + 1;
    }
  }

  protected override void InvalidateItem(int index) { Invalidate(GetItemRectangle(index, true)); }

  protected override ScrollBar MakeScrollBar(Orientation orientation)
  {
    ScrollBar bar = base.MakeScrollBar(orientation);
    bar.ControlStyle &= ~ControlStyle.CanReceiveFocus;
    return bar;
  }

  protected virtual Size MeasureItem(int index)
  {
    return EffectiveFont == null ? new Size(Width, 14) : EffectiveFont.CalculateSize(GetItemText(index));
  }

  protected internal override void OnCustomEvent(Events.ControlEvent e)
  {
    if(e is ScrollEvent) MouseScroll();
    else base.OnCustomEvent(e);
  }

  protected internal override void OnKeyDown(KeyEventArgs e)
  {
    if(!e.Handled)
    {
      if(e.KE.Key == Key.Up || e.KE.Key == Key.Down)
      {
        StopScrolling();
        mouseDown = false;
        if(e.KE.Key == Key.Up) ScrollUp(e);
        else ScrollDown(e);
      }
      else if(e.KE.Key == Key.PageUp)
      {
        int newIndex = CursorPosition;
        ScrollTo(newIndex);
        if(newIndex > TopIndex) newIndex = TopIndex;
        else newIndex = TopIndex = GetTopIndex(TopIndex);
        DragTo(newIndex, e);
      }
      else if(e.KE.Key == Key.PageDown)
      {
        int newIndex = CursorPosition;
        ScrollTo(newIndex);
        if(newIndex < GetBottomIndex()) newIndex = bottom;
        else { TopIndex = bottom; newIndex = GetBottomIndex(); }
        DragTo(newIndex, e);
      }
      else if(e.KE.Key == Key.Home)
      {
        DragTo(TopIndex = 0, e);
      }
      else if(e.KE.Key == Key.End)
      {
        TopIndex = lastTop;
        DragTo(Items.Count - 1, e);
      }
      else if(e.KE.Key == Key.Space)
      {
        if(selMode == SelectionMode.One) SelectedIndex = IsSelected(CursorPosition) ? -1 : CursorPosition;
        else if(selMode != SelectionMode.None) ToggleSelected(CursorPosition);
      }
      else goto done;
      e.Handled = true;
    }

    done: base.OnKeyDown(e);
  }

  protected internal override void OnKeyPress(KeyEventArgs e)
  {
    if(!e.Handled && !e.KE.HasAny(KeyMod.Alt))
    {
      char c = e.KE.Char;
      if(c <= 26) c += (char)64;
      int next = FindChar(c, CursorPosition + 1);
      if(next == -1) next = FindChar(c);
      if(next != -1)
      {
        ScrollTo(next);
        DragTo(next, e);
      }
      e.Handled = true;
    }

    base.OnKeyPress(e);
  }

  protected override void OnListChanged()
  {
    base.OnListChanged();
    CalculateIndexes();
  }

  protected internal override void OnMouseDown(ClickEventArgs e)
  {
    if(!e.Handled && e.CE.Button == MouseButton.Left && ContentRect.Contains(e.CE.Point))
    {
      int index = PointToItem(e.CE.Point);
      Capture = e.Handled = mouseDown = true;

      bool ctrl = Keyboard.HasAny(KeyMod.Ctrl), shift = Keyboard.HasAny(KeyMod.Shift);
      selecting = (!ctrl && selMode != SelectionMode.MultiSimple) || index < 0 || !IsSelected(index);

      if(selMode == SelectionMode.One)
      {
        if(!ctrl || SelectedIndex != index) SelectedIndex = index;
        else SetSelected(index, false);
      }
      else if(selMode != SelectionMode.None)
      {
        if(shift && selMode == SelectionMode.MultiExtended)
        {
          if(!ctrl) ClearSelected();
          if(index >= 0) SelectRange(Math.Max(CursorPosition, 0), index, selecting);
        }
        else
        {
          if(index >= 0 && (ctrl || selMode == SelectionMode.MultiSimple)) ToggleSelected(index);
          else SelectedIndex = index;
        }
      }
      CursorPosition = index;

      e.Handled = true;
    }

    base.OnMouseDown(e);
  }

  protected internal override void OnMouseMove(Events.MouseMoveEvent e)
  {
    if(mouseDown)
    {
      int index = PointToItem(new Point(ContentRect.X, e.Y));
      if(index == -1)
      {
        StartScrolling();
        MouseScroll();
      }
      else
      {
        StopScrolling();
        DragTo(index);
      }
    }

    base.OnMouseMove(e);
  }

  protected internal override void OnMouseUp(ClickEventArgs e)
  {
    if(!e.Handled && mouseDown && e.CE.Button == MouseButton.Left)
    {
      Capture = mouseDown = false;
      StopScrolling();
      e.Handled = true;
    }

    base.OnMouseUp(e);
  }

  protected override void OnPaint(PaintEventArgs e)
  {
    base.OnPaint(e);

    if(Items.Count != 0)
    {
      Rectangle bounds = GetContentDrawRect();
      Rectangle oldClipRect = e.Target.ClipRect;
      try
      {
        e.Target.ClipRect = bounds; // set the clipping rect so that items that go off the edge won't erase the border
        if(FixedHeight)
        {
          bounds.Height = MeasureItem(0).Height;
          int i = Math.Max((e.DrawRect.Y - bounds.Y) / bounds.Height, 0); // calculate the index of the first item to draw
          bounds.Y += i * bounds.Height; // shift the item rectangle down to the bounds of the first item we're drawing
          for(i += TopIndex; i < Items.Count; i++)
          {
            if(bounds.IntersectsWith(e.DrawRect)) DrawItem(i, e, bounds);
            else break;
            bounds.Y += bounds.Height;
          }
        }
        else
        {
          bool drew = false;
          for(int i = TopIndex; i < Items.Count; i++)
          {
            Rectangle itemRect = new Rectangle(bounds.X, bounds.Y, bounds.Width, MeasureItem(i).Height);
            if(itemRect.Height > bounds.Height) break;
            if(itemRect.IntersectsWith(e.DrawRect))
            {
              DrawItem(i, e, itemRect);
              drew = true;
            }
            else if(drew) break;
            bounds.Y      += itemRect.Height;
            bounds.Height -= itemRect.Height;
          }
        }
      }
      finally { e.Target.ClipRect = oldClipRect; }
    }
  }

  protected override void OnResize()
  {
    base.OnResize();
    CalculateIndexes();
  }

  protected override void OnVerticalScroll(object bar, ValueChangedEventArgs e)
  {
    base.OnVerticalScroll(bar, e);
    TopIndex = VerticalScrollBar.Value;
  }

  protected void SelectRange(int from, int to, bool selected)
  {
    if(from == to) return;
    int add = to < from ? -1 : 1;
    while(true)
    {
      Item it = Items.list[from];
      if(selected) it.State |= ItemState.Selected;
      else it.State &= ~ItemState.Selected;
      Items.list[from] = it;
      if(from == to) break;
      from += add;
    }
    Items.StateUpdated();
  }

  sealed class ScrollEvent : Events.ControlEvent
  {
    public ScrollEvent(Control ctrl) : base(ctrl) { }
  }

  void CalculateIndexes()
  {
    bottom  = -1; // force the bottom index to be recalculated the next time GetBottomIndex() is called
    lastTop = GetLastTopIndex(); // recalculate and cache the last top index

    if(lastTop > 0) // if there are more items that can be shown at once (and so a scrollbar is needed)...
    {
      ShowVerticalScrollBar     = true;    // show a vertical scroll bar. the number of items the user can scroll through is the
      VerticalScrollBar.Maximum = lastTop; // number that would be hidden after scrolling all the way down

      int visibleItems = GetBottomIndex() - TopIndex + 1;
      VerticalScrollBar.ThumbSize = (float)visibleItems / Items.Count;
    }
    else // otherwise, we don't need a vertical scrollbar, so hide it
    {
      ShowVerticalScrollBar = false;
    }
  }

  void DragTo(int index)
  {
    DragTo(index, null);
  }
  
  void DragTo(int index, KeyEventArgs e)
  {
    if(index != CursorPosition)
    {
      bool shift, ctrl;
      if(e != null)
      {
        shift = e.KE.HasAny(KeyMod.Shift);
        ctrl = e.KE.HasAny(KeyMod.Ctrl);
      }
      else
      {
        shift = Keyboard.HasAny(KeyMod.Shift);
        ctrl = Keyboard.HasAny(KeyMod.Ctrl);
      }

      if(selMode != SelectionMode.None && (mouseDown || selMode != SelectionMode.MultiSimple && (!ctrl || shift)))
      {
        if(selMode == SelectionMode.One) SelectedIndex = index;
        else if(shift)
        {
          bool select = mouseDown || !ctrl || CursorPosition < 0 || IsSelected(CursorPosition);
          if(!mouseDown && !ctrl && !IsSelected(CursorPosition)) ClearSelected();
          SelectRange(Math.Max(CursorPosition, 0), index, select);
        }
        else if(mouseDown) SetSelected(index, selecting);
        else SelectedIndex = index;
      }
      CursorPosition = index;
    }
  }

  void MouseScroll()
  {
    if(mouseDown)
    {
      Point pt = ScreenToControl(Mouse.Point);
      if(pt.Y < 0) ScrollUp();
      else if(pt.Y >= Height) ScrollDown();
    }
  }

  void ScrollDown()
  {
    ScrollDown(null);
  }
  
  void ScrollDown(KeyEventArgs e)
  {
    int bi = GetBottomIndex();
    int newIndex = CursorPosition;
    if(newIndex == bi)
    {
      if(bi == -1)
      {
        newIndex = TopIndex;
      }
      else if(bi < Items.Count - 1)
      {
        TopIndex++;
        newIndex = bottom = bi + 1;
      }
    }
    else
    {
      ScrollTo(++newIndex);
    }
    DragTo(newIndex, e);
  }

  void ScrollUp()
  {
    ScrollUp(null);
  }
  
  void ScrollUp(KeyEventArgs e)
  {
    int newIndex = CursorPosition;
    if(newIndex == TopIndex)
    {
      if(TopIndex > 0) newIndex = --TopIndex;
    }
    else
    {
      ScrollTo(--newIndex);
    }
    DragTo(newIndex, e);
  }

  void StartScrolling()
  {
    if(!scrolling)
    {
      scrolling = true;
      scrollEvent = new ScrollEvent(this);
      scrollTimer.Start();
    }
  }

  static void StopScrolling()
  {
    if(scrolling)
    {
      scrollTimer.Stop();
      scrollEvent = null;
      scrolling = false;
    }
  }

  int cursor, top, bottom, lastTop;
  SelectionMode selMode;
  bool mouseDown, selecting;

  static void ScrollIt(object sender, EventArgs e)
  {
    ScrollEvent sEvent = scrollEvent;
    if(sEvent != null)
    {
      if(sEvent.Control.Desktop == null) StopScrolling();
      else Events.Events.PushEvent(sEvent);
    }
  }

  static ScrollEvent scrollEvent;
  static Timer scrollTimer;
  static bool scrolling;
}
#endregion

#region ComboBox
public enum ComboBoxStyle
{
  /// <summary>Specifies that the list is displayed by clicking the down arrow, but the text box is also editable, allowing the
  /// user to type in a value.
  /// </summary>
  DropDown,
  /// <summary>Specifies that the list is displayed by clicking the down arrow, and the text box is not editable, so the user is
  /// restricted to selecting a value from the list.
  /// </summary>
  DropDownList,
  /// <summary>Specifies that the list is always visible, but the text box is also editable, allowing the user to type in a
  /// value.
  /// </summary>
  Simple
}

public class ComboBox : ListControl
{
  public ComboBox() { Init(); }
  public ComboBox(System.Collections.IEnumerable items) : base(items) { Init(); }

  void Init()
  {
    BackColor     = ListBox.BackColor = SystemColors.Window;
    ForeColor     = ListBox.ForeColor = SystemColors.WindowText;
    BorderStyle   = BorderStyle.FixedThick | BorderStyle.Depressed;
    DropDownStyle = ComboBoxStyle.DropDownList;
    Padding       = RectOffset.Empty;
    ControlStyle |= ControlStyle.CanReceiveFocus;
  }

  public ComboBoxStyle DropDownStyle
  {
    get { return style; }
    set
    {
      if(style != value)
      {
        TextBox.ReadOnly = value == ComboBoxStyle.DropDownList;

        if(value == ComboBoxStyle.DropDownList)
        {
          TextBox.ControlStyle &= ~(ControlStyle.CanReceiveFocus | ControlStyle.Draggable);
        }
        else
        {
          TextBox.ControlStyle |= ControlStyle.CanReceiveFocus | ControlStyle.Draggable;
        }

        bool wasSimple = style == ComboBoxStyle.Simple;
        style = value;
        if(wasSimple || value == ComboBoxStyle.Simple)
        {
          if(wasSimple)
          {
            Close();
            Height = TextBox.Height + ContentOffset.Vertical;
          }
          else
          {
            ListBox.BorderStyle = style == ComboBoxStyle.Simple ? BorderStyle.None : BorderStyle.FixedFlat;
            Open();
          }
          TriggerLayout();
        }
      }
    }
  }

  public bool IsOpen
  {
    get { return open || style == ComboBoxStyle.Simple; }
  }

  public int ListBoxHeight
  {
    get { return listHeight; }
    set
    {
      if(value != listHeight)
      {
        if(value < 0) throw new ArgumentOutOfRangeException("ListBoxHeight", value, "Height cannot be negative.");
        listHeight = value;
        if(style != ComboBoxStyle.Simple && open) ListBox.Height = value;
      }
    }
  }

  public override int SelectedIndex
  {
    get { return ListBox.SelectedIndex; }
    set { ListBox.SelectedIndex = value; }
  }

  public override string Text
  {
    get { return TextBox.Text; }
    set { TextBox.Text = value; }
  }

  public Size GetPreferredSize()
  {
    return ListBox.GetPreferredSize();
  }

  public Size GetPreferredSize(int numItems)
  {
    return ListBox.GetPreferredSize(numItems);
  }

  protected Rectangle BoxRect
  {
    get
    {
      if(style == ComboBoxStyle.Simple) return new Rectangle();
      Rectangle content = ContentRect;
      int boxSize = content.Height;
      return new Rectangle(content.Right - boxSize, content.Top, boxSize, boxSize);
    }
  }

  protected ListBox ListBox
  {
    get
    {
      if(listBox == null)
      {
        listBox = MakeListBox(Items);
        listBox.BorderStyle  = style == ComboBoxStyle.Simple ? BorderStyle.None : BorderStyle.FixedFlat;
        listBox.KeyDown              += listBox_KeyDown;
        listBox.MouseMove            += listBox_MouseMove;
        listBox.MouseUp              += listBox_MouseUp;
        listBox.SelectedIndexChanged += listBox_SelectedIndexChanged;
      }
      if(Items.Version != oldVersion)
      {
        listBox.Items.Clear();
        listBox.Items.AddRange(Items);
        oldVersion = Items.Version;
      }
      return listBox;
    }
  }

  protected TextBox TextBox
  {
    get
    {
      if(textBox == null)
      {
        textBox = MakeTextBox();
        textBox.BorderStyle  = BorderStyle.None;
        textBox.Parent       = this;
        textBox.ReadOnly     = style == ComboBoxStyle.DropDownList;
        textBox.MouseDown   += textBox_MouseDown;
        textBox.TextChanged += textBox_TextChanged;
        if(EffectiveFont != null) TextBox.Height = EffectiveFont.LineHeight + TextBox.ContentOffset.Vertical;
        if(style != ComboBoxStyle.Simple) Height = TextBox.Height + ContentOffset.Vertical;
      }
      return textBox;
    }
  }

  protected void Close()
  {
    if(style != ComboBoxStyle.Simple)
    {
      Desktop.Invalidate(ListBox.ControlToAncestor(ListBox.ControlRect, Desktop));
      ListBox.Parent = null;
    }
    ListBox.Capture = false;
    mouseDown = open = false;
    OnBoxPress(false);
  }

  protected virtual ListBox MakeListBox(ItemCollection items)
  {
    return new ListBox(items);
  }
  
  protected virtual TextBox MakeTextBox()
  {
    return new TextBox();
  }
  
  protected override void OnEffectiveFontChanged(ValueChangedEventArgs e)
  {
    base.OnEffectiveFontChanged(e);

    if(EffectiveFont != null)
    {
      int height = EffectiveFont.LineHeight + TextBox.ContentOffset.Vertical;
      if(TextBox.Height != height)
      {
        TextBox.Height = height;
        if(style != ComboBoxStyle.Simple) Height = TextBox.Height + ContentOffset.Vertical;
        TriggerLayout();
      }
    }
  }

  protected internal override void OnMouseDown(ClickEventArgs e)
  {
    if(!e.Handled && e.CE.Button == MouseButton.Left &&
       (BoxRect.Contains(e.CE.Point) || open && TextBox.Bounds.Contains(e.CE.Point)))
    {
      if(open)
      {
        Close();
        mouseDown = false;
      }
      else
      {
        Open();
        mouseDown = true;
        OnBoxPress(true);
      }
      e.Handled = true;
    }
    
    base.OnMouseDown(e);
  }

  protected internal override void OnMouseEnter()
  {
    if(mouseDown) OnBoxPress(true);
    base.OnMouseEnter();
  }

  protected internal override void OnMouseLeave()
  {
    if(mouseDown) OnBoxPress(false);
    base.OnMouseLeave();
  }

  protected internal override void OnMouseUp(ClickEventArgs e)
  {
    if(e.CE.Button == MouseButton.Left && open)
    {
      OnBoxPress(false);
      mouseDown = false;
      e.Handled = true;
    }
    base.OnMouseUp(e);
  }

  protected override void OnPaint(PaintEventArgs e)
  {
    base.OnPaint(e);
    e.Renderer.DrawArrowButton(e.Target, ControlToDraw(BoxRect), ArrowDirection.Down, BoxRect.Width / 4, depressed,
                               SystemColors.Control, EffectivelyEnabled ? Color.Black : SystemColors.GrayText);
  }

  protected override void LayOutChildren()
  {
    base.LayOutChildren();

    Rectangle content = ContentRect;
    int endWidth = style == ComboBoxStyle.Simple ? 0 : ContentRect.Height;
    TextBox.Bounds = new Rectangle(content.Left, content.Top, content.Width - endWidth, ContentRect.Height);
    ListBox.Bounds = new Rectangle(content.Left, TextBox.Bottom + 1, content.Width,
                                   style == ComboBoxStyle.Simple ? content.Bottom - TextBox.Bottom - 1 : listHeight);
  }

  protected void Open()
  {
    if(!open)
    {
      Control parent = style == ComboBoxStyle.Simple ? this : (Control)Desktop;
      if(parent == null) return;
      
      ListBox.Parent = parent;

      if(parent == this)
      {
        Rectangle content = ContentRect;
        ListBox.Bounds = new Rectangle(content.Left, TextBox.Bottom + 1,
                                       content.Width, content.Bottom - TextBox.Bottom - 1);
      }
      else
      {
        ListBox.SetBounds(ControlToAncestor(new Rectangle(0, Height, Width, listHeight), parent), true);
      }
      ListBox.Visible = true;
      ListBox.Focus();
      ListBox.Capture = style != ComboBoxStyle.Simple;
      open = true;
    }
  }

  void OnBoxPress(bool down)
  {
    if(down != this.depressed)
    {
      this.depressed = down;
      Invalidate(BoxRect);
    }
  }

  void listBox_KeyDown(object sender, KeyEventArgs e)
  {
    if(!e.Handled && DropDownStyle != ComboBoxStyle.Simple &&
       (e.KE.Key == Key.Enter || e.KE.Key == Key.KpEnter || e.KE.Key == Key.Space || e.KE.Key == Key.Tab))
    {
      Close();
      e.Handled = true;
    }
  }

  void listBox_MouseMove(object sender, GameLib.Events.MouseMoveEvent e)
  {
    if(DropDownStyle != ComboBoxStyle.Simple && listBox.ContentRect.Contains(e.Point))
    {
      listBox.CursorPosition = listBox.SelectedIndex = listBox.PointToItem(e.Point);
    }
  }

  void listBox_MouseUp(object sender, ClickEventArgs e)
  {
    if(style != ComboBoxStyle.Simple && !TextBox.Bounds.Contains(ScreenToControl(ListBox.ControlToScreen(e.CE.Point))))
    {
      Close();
      e.Handled = true;
    }
  }

  void listBox_SelectedIndexChanged(object sender, EventArgs e)
  {
    if(myChange) return;

    if(!string.Equals(TextBox.Text, ListBox.Text, StringComparison.Ordinal))
    {
      ValueChangedEventArgs ve = new ValueChangedEventArgs(TextBox.Text);
      myChange = true;
      TextBox.Text = ListBox.Text;
      myChange = false;
      OnTextChanged(ve);
    }

    OnSelectedIndexChanged();
  }

  void textBox_MouseDown(object sender, ClickEventArgs e)
  {
    if(e.CE.Button == MouseButton.Left && !TextBox.HasStyle(ControlStyle.CanReceiveFocus))
    {
      if(!open) Open();
      else Close();
      e.Handled = true;
    }
  }

  void textBox_TextChanged(object sender, ValueChangedEventArgs e)
  {
    if(!myChange)
    {
      OnTextChanged(e);
      int index = ListBox.FindStringExact(TextBox.Text);
      myChange = true;
      SelectedIndex = index;
      if(index != -1) ListBox.ScrollTo(index);
      myChange = false;
    }
  }

  ListBox listBox;
  TextBox textBox;
  int oldVersion = -1, listHeight = 100;
  ComboBoxStyle style;
  bool open, mouseDown, myChange, depressed;
}
#endregion

} // namespace GameLib.Forms