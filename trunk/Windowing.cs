// TODO: implement mouse cursor
// TODO: add way to cancel key repeat?
// TODO: add 'other control' to focus events?
// TODO: examine Layout further (implemented properly?)
using System;
using System.Collections;
using System.Drawing;
using GameLib.Events;
using GameLib.Video;

namespace GameLib.Forms
{

#region Event arguments and delegates
public class ControlEventArgs : EventArgs
{ public ControlEventArgs(Control control) { Control = control; }
  public Control Control;
}
public delegate void ControlEventHandler(object sender, ControlEventArgs e);

public class DragEventArgs : EventArgs
{ public bool Pressed(byte button) { return (Buttons&(1<<button))!=0; }
  public void SetPressed(byte button, bool down)
  { if(down) Buttons|=(byte)(1<<button); else Buttons&=(byte)~(1<<button);
  }
  public Point Start, End;
  public byte  Buttons;
  public bool  Cancel;
}
public delegate void DragEventHandler(object sender, DragEventArgs e);

public class PaintEventArgs : EventArgs
{ public PaintEventArgs(Control control, Rectangle rect, Surface surface)
  { Surface=surface; WindowRect=rect; DisplayRect=control.WindowToDisplay(rect);
  }
  public Surface   Surface;
  public Rectangle WindowRect, DisplayRect;
}
public delegate void PaintEventHandler(object sender, PaintEventArgs e);

public class KeyEventArgs
{ public KeyEventArgs(KeyboardEvent ke) { KE=ke; }
  public KeyboardEvent KE;
  public bool Handled;
}
public delegate void KeyEventHandler(object sender, KeyEventArgs e);

public class ClickEventArgs
{ public ClickEventArgs() { CE=new MouseClickEvent(); }
  public ClickEventArgs(MouseClickEvent ce) { CE=ce; }
  public MouseClickEvent CE;
  public bool Handled;
}
public delegate void ClickEventHandler(object sender, ClickEventArgs e);
public delegate void MouseMoveEventHandler(object sender, MouseMoveEvent e);
#endregion

#region Control class
[Flags]
public enum ControlStyle
{ None=0, Clickable=1, DoubleClickable=2, Draggable=4, CanFocus=8,
  NormalClick=Clickable|DoubleClickable, Anyclick=NormalClick|Draggable,
}

[Flags]
public enum AnchorStyle // TODO: support more than just corners
{ Left=1, Top=2, Right=4, Bottom=8,
  TopLeft=Top|Left, TopRight=Top|Right, BottomLeft=Bottom|Left, BottomRight=Bottom|Right
}

public enum DockStyle
{ None, Left, Top, Right, Bottom
}

public class Control
{ public Control() { controls = new ControlCollection(this); }

  #region ControlCollection
  public class ControlCollection : CollectionBase
  { internal ControlCollection(Control parent) { this.parent=parent; }
    
    public Control this[int index] { get { return (Control)List[index]; } }
    public int Add(Control control)
    { if(control.Parent!=null) throw new ArgumentException("Already belongs to a control!");
      control.SetParent(parent);
      return List.Add(control);
    }
    public void AddRange(Control[] controls) { foreach(Control c in controls) Add(c); }
    public void AddRange(params object[] controls) { foreach(Control c in controls) Add(c); }
    public int IndexOf(Control control) { return List.IndexOf(control); }
    public int IndexOf(string name)
    { for(int i=0; i<Count; i++) if(this[i].Name==name) return i;
      return -1;
    }
    public Control Find(string name) { return Find(name, true); }
    public Control Find(string name, bool deepSearch)
    { int index = IndexOf(name);
      if(index==-1)
      { if(deepSearch)
          foreach(Control c in this)
          { Control f = c.Controls.Find(name, true);
            if(f!=null) return f;
          }
        return null;
      }
      else return this[index];
    }
    public new void Clear()
    { foreach(Control c in this) c.SetParent(null);
      List.Clear();
    }
    public void Insert(int index, Control control)
    { if(control.Parent!=null) throw new ArgumentException("Already belongs to a control!");
      List.Insert(index, control);
    }
    public void Remove(Control control)
    { if(!Contains(control)) throw new ArgumentException("Control does not exist in collection");
      control.SetParent(null);
      List.Remove(control);
    }
    public new void RemoveAt(int index)
    { this[index].SetParent(null);
      List.RemoveAt(index);
    }
    public bool Contains(Control control) { return control.parent==parent; }
    public bool Contains(Control control, bool deepSearch)
    { if(deepSearch)
      { while(control!=null) { if(control==parent) return true; control=control.parent; }
        return false;
      }
      else return control.parent==parent;
    }
    public bool Contains(string name) { return Find(name, false)!=null; }
    public bool Contains(string name, bool deepSearch) { return Find(name, deepSearch)!=null; }
    
    internal IList Array { get { return List; } }
    
    Control parent;
  }
  #endregion

  #region Properties
  public bool AcceptsTab { get { return acceptsTab; } set { acceptsTab=value; } }
  
  public AnchorStyle Anchor
  { get { return anchor; }
    set
    { if(anchor!=value)
      { if(value!=AnchorStyle.TopLeft) dock=DockStyle.None;
        anchor=value;
        if(parent!=null) UpdateAnchor();
      }
    }
  }
    

  public Color BackColor
  { get
    { Control c = this;
      while(c!=null) { if(c.back!=Color.Transparent) return c.back; c=c.parent; }
      return Color.Transparent;
    }
    set
    { if(value!=back)
      { ValueChangedEventArgs e = new ValueChangedEventArgs(back);
        back = value;
        OnBackColorChanged(e);
      }
    }
  }

  public IBlittable BackImage
  { get { return backimg; }
    set
    { if(value!=backimg)
      { ValueChangedEventArgs e = new ValueChangedEventArgs(backimg);
        backimg = value;
        OnBackImageChanged(e);
      }
    }
  }

  public int Bottom
  { get { return bounds.Bottom; }
    set
    { if(value!=bounds.Bottom)
      { Point p = bounds.Location;
        p.Y = value-bounds.Height;
        Location = p;
      }
    }
  }

  public Rectangle Bounds
  { get { return bounds; }
    set
    { Location = value.Location;
      Size     = value.Size;
    }
  }

  public bool CanFocus { get { return (style&ControlStyle.CanFocus)!=ControlStyle.None && visible && enabled; } }
  public bool Capture
  { get
    { DesktopControl desktop = Desktop;
      return desktop != null && desktop.capturing==this;
    }
    set
    { DesktopControl desktop = Desktop;
      if(value)
      { if(desktop==null) throw new InvalidOperationException("This control has no desktop");
        desktop.capturing = this;
      }
      else if(desktop!=null && desktop.capturing==this) desktop.capturing=null;
    }
  }

  public bool Modal
  { get
    { DesktopControl desktop = Desktop;
      return desktop != null && desktop.modal==this;
    }
    set
    { DesktopControl desktop = Desktop;
      if(value)
      { if(desktop==null) throw new InvalidOperationException("This control has no desktop");
        desktop.SetModal(this);
      }
      else if(desktop!=null && desktop.modal==this) desktop.SetModal(null);
    }
  }

  public Rectangle WindowRect { get { return new Rectangle(0, 0, bounds.Width, bounds.Height); } }

  public ControlCollection Controls { get { return controls; } }
  
  public IBlittable Cursor
  { get
    { Control c = this;
      while(c!=null) { if(c.cursor!=null) return c.cursor; c=c.parent; }
      return null;
    }
    set { cursor=value; }
  }
  
  public DesktopControl Desktop
  { get
    { Control p = this;
      while(p.parent!=null) p=p.parent;
      return p as DesktopControl;
    }
  }

  public Rectangle DisplayRect { get { return WindowToDisplay(WindowRect); } }

  public DockStyle Dock
  { get { return dock; }
    set { if(value!=dock) { throw new NotImplementedException(); } }
  }

  public bool Enabled
  { get
    { Control c = this;
      while(c!=null) { if(c.enabled==false) return false; c=c.parent; }
      return true;
    }
    set
    { if(value!=enabled)
      { enabled = value;
        OnEnabledChanged(new ValueChangedEventArgs(!value));
      }
    }
  }

  public bool Focused
  { get { return parent==null ? false : parent.focused==this; }
    set { if(value) Focus(); else Blur(); }
  }
  
  public GameLib.Fonts.Font Font
  { get
    { Control c = this;
      while(c!=null) { if(c.font!=null) return c.font; c=c.parent; }
      return null;
    }
    set
    { if(value!=font)
      { ValueChangedEventArgs e = new ValueChangedEventArgs(font);
        font = value;
        OnFontChanged(e);
      }
    }
  }

  public Color ForeColor
  { get
    { Control c = this;
      while(c!=null) { if(c.fore!=Color.Transparent) return c.fore; c=c.parent; }
      return Color.Transparent;
    }
    set
    { if(value!=fore)
      { ValueChangedEventArgs e = new ValueChangedEventArgs(fore);
        fore = value;
        OnForeColorChanged(e);
      }
    }
  }

  public bool HasChildren { get { return controls.Count>0; } }

  public int Height
  { get { return bounds.Height; }
    set
    { if(value!=bounds.Height)
      { Size s = bounds.Size;
        s.Height = value;
        Size = s;
      }
    }
  }

  public Rectangle InvalidRect
  { get { return invalid; }
    set
    { invalid.Width=0;
      Invalidate(value);
    }
  }
  
  public bool KeyPreview { get { return keyPreview; } set { keyPreview=value; } }
  
  public int Left
  { get { return bounds.X; }
    set
    { if(value!=bounds.X)
      { Point p = bounds.Location;
        p.X = value;
        Location = p;
      }
    }
  }

  public Point Location
  { get { return bounds.Location; }
    set
    { if(value!=bounds.Location)
      { ValueChangedEventArgs e = new ValueChangedEventArgs(bounds.Location);
        bounds.Location = value;
        OnLocationChanged(e);
      }
    }
  }

  public string Name
  { get { return name; }
    set
    { if(value==null) throw new ArgumentNullException("Name");
      name=value;
    }
  }

  public Control Parent
  { get { return parent; }
    set
    { if(value!=parent)
      { ValueChangedEventArgs e = new ValueChangedEventArgs(parent);
        // 'mychange' is used so OnParentChanged gets called properly
        // (otherwise it'd get called twice with intermediate values)
        mychange=true;
        if(parent!=null) parent.Controls.Remove(this);
        if(value!=null) value.Controls.Add(this);
        mychange=false;
        OnParentChanged(e);
      }
    }
  }

  public Rectangle ParentRect { get { return WindowToParent(WindowRect); } }

  public Color RawBackColor   { get { return back; } }
  public IBlittable RawCursor { get { return cursor; } }
  public bool RawEnabled      { get { return enabled; } }
  public GameLib.Fonts.Font RawFont { get { return font; } }
  public Color RawForeColor   { get { return fore; } }
  public bool RawVisible      { get { return visible; } }

  public int Right
  { get { return bounds.Right; }
    set
    { if(value!=bounds.Right)
      { Point p = bounds.Location;
        p.X = value-bounds.Width;
        Location = p;
      }
    }
  }

  public Size Size
  { get { return bounds.Size; }
    set
    { if(value!=bounds.Size)
      { ValueChangedEventArgs e = new ValueChangedEventArgs(bounds.Size);
        bounds.Size = value;
        OnSizeChanged(e);
      }
    }
  }

  public ControlStyle Style { get { return style; } set { style=value; } }

  public int TabIndex
  { get { return tabIndex; }
    set
    { if(value<-1) throw new ArgumentOutOfRangeException("TabIndex", "must be >= -1");
      tabIndex=value;
    }
  }

  public object Tag { get { return tag; } set { tag=value; } }
  
  public virtual string Text
  { get { return text; }
    set
    { if(value!=text)
      { ValueChangedEventArgs e = new ValueChangedEventArgs(text);
        text = value;
        OnTextChanged(e);
      }
    }
  }

  public int Top
  { get { return bounds.Top; }
    set
    { if(value!=bounds.Top)
      { Point p = bounds.Location;
        p.Y = value;
        Location = p;
      }
    }
  }

  public bool Visible
  { get
    { Control c = this;
      while(c!=null) { if(c.visible==false) return false; c=c.parent; }
      return true;
    }
    set
    { if(value!=visible)
      { visible = value;
        OnVisibleChanged(new ValueChangedEventArgs(!value));
      }
    }
  }

  public int Width
  { get { return bounds.Width; }
    set
    { if(value!=bounds.Width)
      { Size s = bounds.Size;
        s.Width = value;
        Size = s;
      }
    }
  }

  public int BottomAnchorOffset { get { return bottomAnchor; } }
  public int RightAnchorOffset  { get { return rightAnchor;  } }
  #endregion
  
  #region Public methods
  public void AddInvalidRect(Rectangle rect)
  { rect.Intersect(WindowRect);
    if(rect.Width==0) return;
    if(invalid.Width==0) invalid = rect;
    else invalid = Rectangle.Union(rect, invalid);
  }

  public void BringToFront()
  { AssertParent();
    IList list = parent.controls.Array;
    if(list[list.Count-1]!=this)
    { list.Remove(this);
      list.Insert(list.Count-1, this);
      Invalidate();
    }
  }

  public void Blur() { if(parent!=null && parent.FocusedControl==this) parent.FocusedControl=null; }

  public void Focus()
  { AssertParent();
    if(CanFocus) parent.FocusedControl = this;
  }

  public Control GetChildAtPoint(Point point)
  { for(int i=controls.Count-1; i>=0; i--)
    { Control c = controls[i];
      if(c.visible && c.bounds.Contains(point)) return c;
    }
    return null;
  }

  public Control GetNextControl() { return GetNextControl(false); }
  public Control GetNextControl(bool reverse)
  { Control next=null, ext=null;
    if(reverse)
    { int tabv=0, maxv=int.MinValue, index = focused==null ? int.MinValue : focused.tabIndex;
      foreach(Control c in controls)
      { if(!c.enabled || !c.visible) continue;
        if(c.tabIndex<index && c.tabIndex>=tabv) { tabv=c.tabIndex; next=c; }
        if(c.tabIndex>-1 && c.tabIndex>maxv) { maxv=c.tabIndex; ext=c; }
      }
    }
    else
    { int tabv=int.MaxValue, minv=int.MaxValue, index = focused==null ? int.MaxValue : focused.tabIndex;
      foreach(Control c in controls)
      { if(!c.enabled || !c.visible) continue;
        if(c.tabIndex>index && c.tabIndex<=tabv) { tabv=c.tabIndex; next=c; }
        if(c.tabIndex>-1 && c.tabIndex<minv) { minv=c.tabIndex; ext=c; }
      }
    }
    return next==null ? ext : next;
  }

  public void Invalidate() { Invalidate(WindowRect); }
  public void Invalidate(Rectangle area)
  { if(back==Color.Transparent && parent!=null) parent.Invalidate(WindowToParent(area));
    else
    { AddInvalidRect(area);
      if(!pendingPaint && visible && invalid.Width>0 &&
         (parent!=null || this is DesktopControl) && Events.Events.Initialized)
      { pendingPaint=true;
        Events.Events.PushEvent(new WindowPaintEvent(this));
      }
    }
  }

  public bool IsOrHas(Control control) { return this==control || Controls.Contains(control); }

  public void Update()
  { DesktopControl desktop = Desktop;
    if(desktop!=null) desktop.DoPaint(this);
  }
  
  public Point DisplayToWindow(Point displayPoint)
  { Control c = this;
    while(c!=null) { displayPoint.X-=c.bounds.X; displayPoint.Y-=c.bounds.Y; c=c.parent; }
    return displayPoint;
  }
  public Point WindowToChild(Point windowPoint, Control child)
  { windowPoint.X -= child.bounds.X; windowPoint.Y -= child.bounds.Y;
    return windowPoint;
  }
  public Point WindowToParent(Point windowPoint)
  { windowPoint.X+=bounds.X; windowPoint.Y+=bounds.Y; return windowPoint;
  }
  public Point WindowToDisplay(Point windowPoint) { return WindowToAncestor(windowPoint, null); }
  public Point WindowToAncestor(Point windowPoint, Control ancestor)
  { if(ancestor==this) return windowPoint;
    Control c = this;
    do { windowPoint.X+=c.bounds.X; windowPoint.Y+=c.bounds.Y; c=c.parent; } while(c!=ancestor);
    return windowPoint;
  }

  public Rectangle DisplayToWindow(Rectangle displayRect)
  { return new Rectangle(DisplayToWindow(displayRect.Location), displayRect.Size);
  }
  public Rectangle WindowToChild(Rectangle windowRect, Control child)
  { windowRect.X -= child.bounds.X; windowRect.Y -= child.bounds.Y;
    return windowRect;
  }
  public Rectangle WindowToParent(Rectangle windowRect)
  { return new Rectangle(WindowToParent(windowRect.Location), windowRect.Size);
  }
  public Rectangle WindowToDisplay(Rectangle windowRect)
  { return new Rectangle(WindowToAncestor(windowRect.Location, null), windowRect.Size);
  }
  public Rectangle WindowToAncestor(Rectangle windowRect, Control ancestor)
  { return new Rectangle(WindowToAncestor(windowRect.Location, ancestor), windowRect.Size);
  }

  public void Refresh() { Refresh(WindowRect); }
  public void Refresh(Rectangle area)
  { if(back==Color.Transparent && parent!=null) parent.Refresh(WindowToParent(area));
    else
    { AddInvalidRect(area);
      Update();
    }
  }

  public void ResumeLayout() { ResumeLayout(true); }
  public void ResumeLayout(bool updateNow)
  { if(layoutSuspended && pendingLayout)
    { if(updateNow) OnLayout(new EventArgs());
      else if(Events.Events.Initialized) Events.Events.PushEvent(new WindowLayoutEvent(this));
    }
    layoutSuspended=false;
  }

  public void SendToBack()
  { AssertParent();
    IList list = parent.controls.Array;
    if(list[0]!=this)
    { list.Remove(this);
      list.Insert(0, this);
      parent.Invalidate(bounds);
    }
  }

  public void SuspendLayout() { layoutSuspended=true; }
  
  public void TabToNextControl() { TabToNextControl(false); }
  public void TabToNextControl(bool reverse) { FocusedControl = GetNextControl(reverse); }
  #endregion
  
  #region Events
  public event ValueChangedEventHandler BackColorChanged, BackImageChanged, EnabledChanged, FontChanged,
    ForeColorChanged, LocationChanged, ParentChanged, SizeChanged, TabIndexChanged, TextChanged, VisibleChanged;
  public event EventHandler GotFocus, LostFocus, Layout, MouseEnter, MouseLeave, Move, Resize;
  public event ControlEventHandler ControlAdded, ControlRemoved;
  public event KeyEventHandler KeyDown, KeyUp, KeyPress;
  public event MouseMoveEventHandler MouseMove;
  public event ClickEventHandler MouseDown, MouseUp, MouseClick, DoubleClick;
  public event DragEventHandler DragStart, DragMove, DragEnd;
  public event PaintEventHandler PaintBackground, Paint;
  
  // TODO: should these be triggered if our back-color is transparent and an ancestor's is changed?
  protected virtual void OnBackColorChanged(ValueChangedEventArgs e)
  { if(BackColorChanged!=null) BackColorChanged(this, e);
    Invalidate();
    foreach(Control c in controls) c.OnParentBackColorChanged(e);
  }

  protected virtual void OnBackImageChanged(ValueChangedEventArgs e)
  { if(BackImageChanged!=null) BackImageChanged(this, e);
    Invalidate();
    foreach(Control c in controls) c.OnParentBackImageChanged(e);
  }

  protected virtual void OnEnabledChanged(ValueChangedEventArgs e)
  { if(EnabledChanged!=null) EnabledChanged(this, e);
    if(Enabled != (bool)e.OldValue)
    { if(!Enabled) Blur();
      Invalidate();
      foreach(Control c in controls) c.OnParentEnabledChanged(e);
    }
  }

  protected virtual void OnFontChanged(ValueChangedEventArgs e)
  { if(FontChanged!=null) FontChanged(this, e);
    Invalidate();
    foreach(Control c in controls) c.OnParentFontChanged(e);
  }

  protected virtual void OnForeColorChanged(ValueChangedEventArgs e)
  { if(ForeColorChanged!=null) ForeColorChanged(this, e);
    Invalidate();
    foreach(Control c in controls) c.OnParentForeColorChanged(e);
  }

  protected virtual void OnLocationChanged(ValueChangedEventArgs e)
  { if(LocationChanged!=null) LocationChanged(this, e);
    if(parent!=null)
    { parent.Invalidate(new Rectangle((Point)e.OldValue, bounds.Size));
      Invalidate();
      UpdateAnchor();
    }
    OnMove(e);
  }

  protected virtual void OnParentChanged(ValueChangedEventArgs e)
  { if(ParentChanged!=null) ParentChanged(this, e);
    if(parent!=null)
    { UpdateAnchor();
      UpdateDock();
      Invalidate();
    }
  }

  protected virtual void OnSizeChanged(ValueChangedEventArgs e)
  { if(SizeChanged!=null) SizeChanged(this, e);
    if(invalid.Right>bounds.Width) invalid.Width-=invalid.Right-bounds.Width;
    if(invalid.Bottom>bounds.Height) invalid.Height-=invalid.Bottom-bounds.Height;
    if(!pendingLayout && !mychange) // 'mychange' is true when docking
    { if(!layoutSuspended && Events.Events.Initialized) Events.Events.PushEvent(new WindowLayoutEvent(this));
      pendingLayout=true;
    }
    Size old = (Size)e.OldValue;
    if(parent!=null && (bounds.Width<old.Width || bounds.Height<old.Height))
    { if(bounds.Width<old.Width && bounds.Height<old.Height) // invalidate the smallest rectangle necessary
        parent.Invalidate(new Rectangle(WindowToParent(bounds.Location), old));
      if(bounds.Width<old.Width)
        parent.Invalidate(new Rectangle(WindowToParent(new Point(bounds.Width, 0)),
                                        new Size(old.Width-bounds.Width, old.Height)));
      else parent.Invalidate(new Rectangle(WindowToParent(new Point(0, bounds.Height)),
                                           new Size(old.Width, old.Height-bounds.Height)));
    }
    Invalidate();
    OnResize(e);
    foreach(Control c in controls) c.OnParentResized(e);
  }

  protected virtual void OnTabIndexChanged(ValueChangedEventArgs e)
  { if(TabIndexChanged!=null) TabIndexChanged(this, e);
  }

  protected virtual void OnTextChanged(ValueChangedEventArgs e) { if(TextChanged!=null) TextChanged(this, e); }

  protected virtual void OnVisibleChanged(ValueChangedEventArgs e)
  { if(VisibleChanged!=null) VisibleChanged(this, e);
    if(Visible!=(bool)e.OldValue)
    { if(!Visible) Blur();
      foreach(Control c in controls) c.OnParentVisibleChanged(e);
      Invalidate();
    }
  }

  protected virtual void OnGotFocus(EventArgs e)  { if(GotFocus!=null) GotFocus(this, e); }
  protected virtual void OnLostFocus(EventArgs e) { if(LostFocus!=null) LostFocus(this, e); }

  protected internal virtual void OnLayout(EventArgs e)
  { if(Layout!=null) Layout(this, e);
    if(dock!=DockStyle.None && parent!=null) UpdateDock();
    pendingLayout=false;
  }

  protected internal virtual void OnMouseEnter(EventArgs e) { if(MouseEnter!=null) MouseEnter(this, e); }
  protected internal virtual void OnMouseLeave(EventArgs e) { if(MouseLeave!=null) MouseLeave(this, e); }

  protected virtual void OnMove(EventArgs e)   { if(Move!=null) Move(this, e); }
  protected virtual void OnResize(EventArgs e) { if(Resize!=null) Resize(this, e); }

  protected virtual void OnControlAdded(ControlEventArgs e) { if(ControlAdded!=null) ControlAdded(this, e); }
  protected virtual void OnControlRemoved(ControlEventArgs e)
  { if(focused==e.Control) { focused.OnLostFocus(e); focused=null; }
    if(e.Control.Visible) Invalidate(e.Control.Bounds);
    if(ControlRemoved!=null) ControlRemoved(this, e);
  }

  protected internal virtual void OnKeyDown(KeyEventArgs e)  { if(KeyDown!=null) KeyDown(this, e); }
  protected internal virtual void OnKeyUp(KeyEventArgs e)    { if(KeyUp!=null) KeyUp(this, e); }
  protected internal virtual void OnKeyPress(KeyEventArgs e) { if(KeyPress!=null) KeyPress(this, e); }

  protected internal virtual void OnMouseMove(MouseMoveEvent e) { if(MouseMove!=null) MouseMove(this, e); }

  protected internal virtual void OnMouseDown(ClickEventArgs e)   { if(MouseDown!=null) MouseDown(this, e); }
  protected internal virtual void OnMouseUp(ClickEventArgs e)     { if(MouseUp!=null) MouseUp(this, e); }
  protected internal virtual void OnMouseClick(ClickEventArgs e)  { if(MouseClick!=null) MouseClick(this, e); }
  protected internal virtual void OnDoubleClick(ClickEventArgs e) { if(DoubleClick!=null) DoubleClick(this, e); }
  
  protected internal virtual void OnDragStart(DragEventArgs e) { if(DragStart!=null) DragStart(this, e); }
  protected internal virtual void OnDragMove(DragEventArgs e)  { if(DragMove!=null) DragMove(this, e); }
  protected internal virtual void OnDragEnd(DragEventArgs e)   { if(DragEnd!=null) DragEnd(this, e); }

  protected internal virtual void OnPaintBackground(PaintEventArgs e)
  { if(back!=Color.Transparent) e.Surface.Fill(e.DisplayRect, back);
    if(backimg!=null) backimg.Blit(e.Surface, e.WindowRect, e.DisplayRect.X, e.DisplayRect.Y);
    if(PaintBackground!=null) PaintBackground(this, e);
  }
  protected internal virtual void OnPaint(PaintEventArgs e)
  { if(Paint!=null) Paint(this, e);
    invalid.Width = 0;
    pendingPaint  = false;
  }
  
  protected virtual void OnParentBackColorChanged(ValueChangedEventArgs e)
  { if(back==Color.Transparent) Invalidate();
  }
  protected virtual void OnParentBackImageChanged(ValueChangedEventArgs e)
  { if(back==Color.Transparent) Invalidate();
  }
  protected virtual void OnParentEnabledChanged(ValueChangedEventArgs e)
  { if(Enabled==(bool)e.OldValue) Invalidate();
  }
  protected virtual void OnParentFontChanged(ValueChangedEventArgs e) { if(font==null) Invalidate(); }
  protected virtual void OnParentForeColorChanged(ValueChangedEventArgs e)
  { if(fore==Color.Transparent) Invalidate();
  }
  protected virtual void OnParentResized(EventArgs e) { }
  protected virtual void OnParentVisibleChanged(ValueChangedEventArgs e) { if(Visible) Invalidate(); }
  
  protected internal virtual void OnCustomEvent(WindowEvent e) { }
  #endregion
  
  internal void SetParent(Control control)
  { ValueChangedEventArgs ve = new ValueChangedEventArgs(parent);
    ControlEventArgs ce = new ControlEventArgs(this);
    if(parent!=null)
    { DesktopControl desktop = parent is DesktopControl ? (DesktopControl)parent : parent.Desktop;
      if(desktop.capturing!=null && ce.Control.IsOrHas(desktop.capturing)) desktop.capturing=null;
      if(desktop.modal!=null && ce.Control.IsOrHas(desktop.modal)) desktop.SetModal(null);
      parent.OnControlRemoved(ce);
    }
    parent = control;
    if(parent!=null) parent.OnControlAdded(ce);
    if(!mychange) OnParentChanged(ve);
  }
  
  protected internal Control FocusedControl
  { get { return focused; }
    set
    { if(value!=focused)
      { if(value != null && !controls.Contains(value))
          throw new ArgumentException("Not a child of this control", "FocusedControl");
        // TODO: make sure controls can call .Focus() inside OnLostFocus()
        if(focused!=null) focused.OnLostFocus(new EventArgs());
        focused = value;
        if(value!=null) value.OnGotFocus(new EventArgs());
      }
    }
  }
  
  protected internal bool HasStyle(ControlStyle test) { return (style & test) != ControlStyle.None; }
  protected internal int dragThreshold = -1;

  internal uint lastClickTime = int.MaxValue;

  protected void AssertParent()
  { if(parent==null) throw new InvalidOperationException("This control has no parent");
  }

  protected Rectangle bounds = new Rectangle(0, 0, 100, 100), invalid;
  protected bool pendingPaint, pendingLayout, layoutSuspended;
  
  void UpdateAnchor()
  { if((anchor&AnchorStyle.Right) != 0) rightAnchor=parent.Width-bounds.Right;
    if((anchor&AnchorStyle.Bottom) != 0) bottomAnchor=parent.Height-bounds.Bottom;
  }
  
  void UpdateDock()
  { mychange=true;
    // TODO: move other controls out of the way, etc
    switch(dock)
    { case DockStyle.Left:   Bounds = new Rectangle(0, 0, Width, parent.Height); break;
      case DockStyle.Top:    Bounds = new Rectangle(0, 0, parent.Width, Height); break;
      case DockStyle.Right:  Bounds = new Rectangle(parent.Width-Width, 0, Width, parent.Height); break;
      case DockStyle.Bottom: Bounds = new Rectangle(0, parent.Height-Height, parent.Width, Height); break;
    }
    mychange=false;
  }

  ControlCollection controls;
  Control parent, focused;
  GameLib.Fonts.Font font;
  Color back=Color.Transparent, fore=Color.Transparent;
  IBlittable backimg, cursor;
  string name=string.Empty, text=string.Empty;
  object tag;
  int tabIndex=-1, bottomAnchor, rightAnchor;
  ControlStyle style;
  AnchorStyle  anchor=AnchorStyle.TopLeft;
  DockStyle    dock;
  bool enabled=true, visible=true, mychange, keyPreview, acceptsTab;
}
#endregion

#region DesktopControl class
public enum AutoFocus { None=0, Click=1, Over=2, OverSticky=3 }

public class DesktopControl : ContainerControl, IDisposable
{ public DesktopControl() { Init(); }
  public DesktopControl(Surface surface) { Init(); Surface = surface; }
  ~DesktopControl() { Dispose(true); }
  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  #region Properties
  public AutoFocus AutoFocusing { get { return focus; } set { focus=value; } }

  public uint DoubleClickDelay  { get { return dcDelay; } set { dcDelay=value; } }

  public int DragThreshold
  { get { return dragThresh; }
    set
    { if(value<1) throw new ArgumentOutOfRangeException("DragThreshold", "must be >=1");
      dragThresh=value;
    }
  }

  public uint KeyRepeatDelay
  { get { return krDelay; }
    set
    { if(value==krDelay) return;
      krDelay=value;
      if(value==0 && krTimer!=null)
      { krTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        krTimer.Dispose();
        krTimer = null;
      }
      else if(value!=0 && krTimer==null)
      { krTimer = new System.Threading.Timer(new System.Threading.TimerCallback(RepeatKey), null,
                                             System.Threading.Timeout.Infinite, krRate);
      }
    }
  }

  public uint KeyRepeatRate
  { get { return krRate; }
    set
    { if(value==krRate) return;
      krRate=value;
      if(krTimer!=null) krTimer.Change(krRate, krRate);
    }
  }

  public bool ProcessKeys      { get { return keys; } set { keys=value; } }
  public bool ProcessMouseMove { get { return moves; } set { moves=value; } }
  public bool ProcessClicks    { get { return clicks; } set { clicks=value; } }
  public Surface Surface
  { get { return surface; }
    set
    { surface = value;
      if(surface!=null) Invalidate();
    }
  }

  public Input.Key TabCharacter { get { return tab; } set { tab=value; } }

  public bool TrackUpdates
  { get { return trackUpdates; }
    set
    { trackUpdates=value;
      if(!value) updatedLen=0;
    }
  }
  public bool Updated
  { get { return updatedLen>0; }
    set { if(value) Invalidate(); else updatedLen=0; }
  }
  public int NumUpdatedAreas { get { return updatedLen; } }
  public Rectangle[] UpdatedAreas { get { return updated; } }
  #endregion

  public void DoPaint() { DoPaint(this); }
  public void DoPaint(Control child)
  { if(surface!=null && child.InvalidRect.Width>0)
    { PaintEventArgs pe = new PaintEventArgs(child, child.InvalidRect, surface);
      if(!Surface.Bounds.Contains(pe.DisplayRect))
      { pe.DisplayRect.Intersect(Surface.Bounds);
        pe.WindowRect = child.DisplayToWindow(pe.DisplayRect);
      }
      child.OnPaintBackground(pe);
      child.OnPaint(pe);

      // TODO: combine rectangles more efficiently
      if(trackUpdates)
      { int i;
        for(i=0; i<updatedLen; i++)
        { if(updated[i].Contains(pe.DisplayRect)) return;
          retest:
          if(pe.DisplayRect.Contains(updated[i]) && --updatedLen != i)
          { updated[i] = updated[updatedLen];
            goto retest;
          }
        }
        if(i>=updatedLen)
        { if(updatedLen==updated.Length)
          { Rectangle[] narr = new Rectangle[updated.Length*2];
            Array.Copy(narr, updated, updated.Length);
            updated = narr;
          }
          updated[updatedLen++] = pe.DisplayRect;
        }
      }
    }
  }

  #region ProcessEvent
  public FilterAction ProcessEvent(Event e)
  {
    #region Mouse moves
    if(moves && e is MouseMoveEvent)
    { MouseMoveEvent ea = (MouseMoveEvent)e;
      Point at = ea.Point;
      // if the cursor is not within the desktop area, ignore it (unless dragging or capturing)
      if(dragging==null && capturing==null && !Bounds.Contains(at)) return FilterAction.Continue;

      Control p=this, c;
      // passModal is true if there's no modal window, or this movement is within the modal window
      bool passModal = modal==null;
      at.X -= bounds.X; at.Y -= bounds.Y; // at is the cursor point local to 'p'
      EventArgs eventArgs=null;
      int ei=0;
      while(p.Enabled && p.Visible)
      { c = p.GetChildAtPoint(at);
        // enter/leave algorithm:
        // keep an array of the path down the control tree, from the root down
        // on mouse move, go down the tree, comparing against the stored path
        if(ei<enteredLen && c!=entered[ei])
        { if(eventArgs==null) eventArgs=new EventArgs();
          for(int i=enteredLen-1; i>=ei; i--)
          { entered[i].OnMouseLeave(eventArgs);
            entered[i] = null;
          }
          enteredLen = ei;
        }
        if(c==null) break;
        if(!passModal && c==modal) passModal=true;
        if(ei==enteredLen && passModal)
        { if(eventArgs==null) eventArgs=new EventArgs();
          if(enteredLen==entered.Length)
          { Control[] na = new Control[entered.Length*2];
            Array.Copy(entered, na, enteredLen);
            entered = na;
          }
          entered[enteredLen++] = c;
          c.OnMouseEnter(eventArgs);
        }
        at = p.WindowToChild(at, c);
        if((focus==AutoFocus.OverSticky || focus==AutoFocus.Over) && c.CanFocus && passModal) c.Focus();
        ei++;
        p = c;
      }
      // at this point, 'p' points to the control that doesn't have a child at 'at'
      // normally we'd set its FocusedControl to null to indicate this, but if there's a modal window,
      // we don't unset any focus
      if(focus==AutoFocus.Over && passModal) p.FocusedControl=null;
      
      if(dragging!=null)
      { if(dragStarted)
        { drag.End = p==dragging ? at : dragging.DisplayToWindow(ea.Point);
          dragging.OnDragMove(drag);
          if(drag.Cancel) EndDrag();
        }
        else if(capturing==null || capturing==p)
        { int xd = ea.X-drag.Start.X;
          int yd = ea.Y-drag.Start.Y;
          if(xd*xd+yd*yd >= (p.dragThreshold==-1 ? dragThresh : p.dragThreshold))
          { drag.Start = p.DisplayToWindow(drag.Start);
            drag.End = ea.Point;
            drag.Buttons = ea.Buttons;
            drag.Cancel = false;
            dragStarted = true;
            dragging.OnDragStart(drag);
            if(drag.Cancel) EndDrag();
          }
        }
      }

      if(capturing!=null)
      { ea.Point = capturing.DisplayToWindow(ea.Point);
        capturing.OnMouseMove(ea);
      }
      else if(passModal)
      { if(p != this)
        { ea.Point = at;
          p.OnMouseMove(ea);
        }
      }
      return FilterAction.Drop;
    }
    #endregion
    #region Keyboard
    else if(keys && e is KeyboardEvent)
    { if(FocusedControl!=null || KeyPreview)
      { KeyEventArgs ea = new KeyEventArgs((KeyboardEvent)e);
        if(ea.KE.Down)
        { if(krTimer!=null)
          { heldKey = ea.KE;
            krTimer.Change(krDelay, krRate);
          }
        }
        else if(heldKey!=null && heldKey.Key==ea.KE.Key)
        { krTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
          heldKey = null;
        }
        DispatchKeyToFocused(ea);
        return FilterAction.Drop;
      }
      else
      { KeyboardEvent ke = (KeyboardEvent)e;
        if(ke.Down && ke.Key==tab)
        { TabToNext(ke.HasAnyMod(Input.KeyMod.Shift));
          if(krTimer!=null)
          { heldKey = ke;
            krTimer.Change(krDelay, krRate);
          }
        }
      }
    }
    #endregion
    #region Mouse clicks
    else if(clicks && e is MouseClickEvent)
    { ClickEventArgs ea = new ClickEventArgs((MouseClickEvent)e);
      Point  at = ea.CE.Point;
      // if the click is not within the desktop area, ignore it (unless dragging or capturing)
      if(capturing==null && !dragStarted && !Bounds.Contains(at)) return FilterAction.Continue;
      Control p = this, c;
      uint time = Timing.Msecs;
      bool passModal = modal==null;
      
      at.X -= bounds.X; at.Y -= bounds.Y; // at is the cursor point local to 'p'
      while(p.Enabled && p.Visible)
      { c = p.GetChildAtPoint(at);
        if(c==null) break;
        if(!passModal && c==modal) passModal=true;
        at = p.WindowToChild(at, c);
        if(focus==AutoFocus.Click && ea.CE.Down && c.CanFocus && passModal) c.Focus();
        p = c;
      }
      if(p==this) // if p=='this', the desktop was clicked
      { if(focus==AutoFocus.Click && ea.CE.Down && FocusedControl!=null && passModal) FocusedControl = null; // blur
        if(!dragStarted && capturing==null) goto done; // if we're not dragging or capturing, then we're done
      }

      if(ea.CE.Down)
      { // only consider a drag if the click occurred within the modal window, and we're not already tracking one
        if(passModal && dragging==null && p.HasStyle(ControlStyle.Draggable))
        { dragging = p;
          drag.Start = ea.CE.Point;
          drag.SetPressed(ea.CE.Button, true);
        }
      }
      // button released. if we haven't started dragging (only considering one) or the button was one
      // involved in the drag, then end the drag/consideration
      else if(!dragStarted || drag.Pressed(ea.CE.Button))
      { bool skipClick = dragStarted;
        if(dragStarted)
        { drag.End = dragging.DisplayToWindow(ea.CE.Point);
          dragging.OnDragEnd(drag);
        }
        EndDrag();
        // if we were dragging, or the mouse was released over the desktop and we're not capturing,
        // then don't trigger any other mouse events (MouseUp or MouseClick). we're done.
        if(skipClick || (p==this && capturing==null)) goto done;
      }
      else if(p==this && capturing==null) goto done; // the mouse was released over the desktop and we're not capturing

      clickStatus = ClickStatus.All;
      if(capturing!=null)
      { ea.CE.Point = capturing.DisplayToWindow(ea.CE.Point);
        DispatchClickEvent(capturing, ea, time);
      }
      else if(passModal)
      { ea.CE.Point = at;
        do
        { if(!DispatchClickEvent(p, ea, time)) break;
          ea.CE.Point = p.WindowToParent(ea.CE.Point);
          p = p.Parent;
        } while(p != this && p.Enabled && p.Visible);
      }
      done:
      // lastClicked is used to track if the button release occurred over the same control it was pressed over
      // this allows you to press the mouse on a control, then drag off and release to avoid the MouseClick event
      if(!ea.CE.Down && ea.CE.Button<8) lastClicked[ea.CE.Button] = null;
      return FilterAction.Drop;
    }
    #endregion
    #region WindowEvent
    else if(e is WindowEvent)
    { WindowEvent we = (WindowEvent)e;
      switch(we.SubType)
      { case WindowEvent.MessageType.KeyRepeat:
          if(heldKey!=null) DispatchKeyToFocused(new KeyEventArgs(heldKey));
          break;
        case WindowEvent.MessageType.Paint: DoPaint(we.Control); break;
        case WindowEvent.MessageType.Layout: we.Control.OnLayout(new EventArgs()); break;
        default: we.Control.OnCustomEvent(we); break;
      }
      return FilterAction.Drop;
    }
    #endregion
    return FilterAction.Continue;
  }
  #endregion

  protected void Dispose(bool destructor)
  { if(init)
    { Events.Events.Initialize();
      init = false;
    }
    if(krTimer!=null)
    { krTimer.Dispose();
      krTimer=null;
    }
  }

  internal void SetModal(Control control)
  { if(control==null) modal=null;
    else
    { modal = control;
      if(capturing!=control) capturing=null;
      if(dragging!=null && dragging!=control) EndDrag();
      while(control!=this) { control.Focus(); control=control.Parent; }
    }
  }

  internal Control capturing, modal;

  #region Dispatchers
  bool DispatchKeyEvent(Control target, KeyEventArgs e)
  { if(e.KE.Down)
    { target.OnKeyDown(e);
      if(e.Handled) return false;
      if(e.KE.Char!=0) target.OnKeyPress(e);
    }
    else target.OnKeyUp(e);
    return !e.Handled;
  }

  bool DispatchClickEvent(Control target, ClickEventArgs e, uint time)
  { if(e.CE.Down && (clickStatus&ClickStatus.UpDown) != 0)
    { target.OnMouseDown(e);
      if(e.Handled) { clickStatus ^= ClickStatus.UpDown; e.Handled=false; }
    }
    if(target.HasStyle(ControlStyle.NormalClick) && (clickStatus&ClickStatus.Click)!=0 && e.CE.Button<8)
    { if(e.CE.Down) lastClicked[e.CE.Button] = target;
      else
      { if(lastClicked[e.CE.Button]==target)
        { if(target.HasStyle(ControlStyle.DoubleClickable) && time-target.lastClickTime<=dcDelay)
            target.OnDoubleClick(e);
          else target.OnMouseClick(e);
          target.lastClickTime = time;
          if(e.Handled) { clickStatus ^= ClickStatus.Click; e.Handled=false; }
          lastClicked[e.CE.Button]=target.Parent; // allow the check to be done for the parent, too // TODO: make sure this is okay with captured/dragged controls
        }
      }
    }
    if(!e.CE.Down && (clickStatus&ClickStatus.UpDown) != 0)
    { target.OnMouseUp(e);
      if(e.Handled) { clickStatus ^= ClickStatus.UpDown; e.Handled=false; }
    } 
    return clickStatus!=ClickStatus.None;
  }

  bool DispatchKeyToFocused(KeyEventArgs e)
  { if(e.Handled) return false;
    Control fc=this;
    if(fc.FocusedControl!=null)
      do
      { if(fc.KeyPreview && !DispatchKeyEvent(fc, e)) goto done;
        fc = fc.FocusedControl;
      } while(fc.FocusedControl!=null);
    if(!DispatchKeyEvent(fc, e)) goto done;
    if(e.KE.Down && e.KE.Key==tab && (!e.Handled || !fc.AcceptsTab)) TabToNext(e.KE.HasAnyMod(Input.KeyMod.Shift));
    done:
    return !e.Handled;
  }
  #endregion

  void RepeatKey(object dummy) { Events.Events.PushEvent(new KeyRepeatEvent()); }
  void TabToNext(bool reverse)
  { Control fc = this;
    while(fc.FocusedControl!=null) fc=fc.FocusedControl;
    (fc==this ? this : fc.Parent).TabToNextControl(reverse);
  }
  void EndDrag()
  { dragging=null;
    dragStarted=false;
    drag.Buttons=0;
  }

  [Flags] enum ClickStatus { None=0, UpDown=1, Click=2, All=UpDown|Click };
  Surface   surface;
  AutoFocus focus=AutoFocus.Click;
  Control[] lastClicked=new Control[8], entered=new Control[8];
  Control   dragging;
  System.Threading.Timer krTimer;
  KeyboardEvent heldKey;
  DragEventArgs drag;
  Rectangle[] updated = new Rectangle[8];
  Input.Key tab=Input.Key.Tab;
  ClickStatus clickStatus;
  int   dragThresh=16, enteredLen, updatedLen;
  uint  dcDelay=350, krDelay, krRate=50;
  bool  keys=true, clicks=true, moves=true, init, dragStarted, trackUpdates=true;
  
  void Init()
  { Events.Events.Initialize();
    init = true;
    drag = new DragEventArgs();
  }
}
#endregion

#region Utility class
public class Utility
{ private Utility() { }
  
  public static Point CalculateAlignment(Rectangle container, Size item, ContentAlignment align)
  { Point ret = new Point();
    if(AlignedLeft(align)) ret.X = container.X;
    else if(AlignedCenter(align)) ret.X = container.X + (container.Width - item.Width)/2;
    else ret.X = container.Right - item.Width;
    
    if(AlignedTop(align)) ret.Y = container.Y;
    else if(AlignedMiddle(align)) ret.Y = container.Y + (container.Height - item.Height)/2;
    else ret.Y = container.Bottom - item.Height;
    return ret;
  }
  
  public static bool AlignedLeft(ContentAlignment align)
  { return align==ContentAlignment.TopLeft || align==ContentAlignment.MiddleLeft ||
           align==ContentAlignment.BottomLeft;
  }
  public static bool AlignedCenter(ContentAlignment align)
  { return align==ContentAlignment.MiddleCenter || align==ContentAlignment.TopCenter ||
           align==ContentAlignment.BottomCenter;
  }
  public static bool AlignedRight(ContentAlignment align)
  { return align==ContentAlignment.TopRight || align==ContentAlignment.MiddleRight ||
           align==ContentAlignment.BottomRight;
  }
  public static bool AlignedTop(ContentAlignment align)
  { return align==ContentAlignment.TopLeft || align==ContentAlignment.TopCenter ||
           align==ContentAlignment.TopRight;
  }
  public static bool AlignedMiddle(ContentAlignment align)
  { return align==ContentAlignment.MiddleCenter || align==ContentAlignment.MiddleLeft ||
           align==ContentAlignment.MiddleRight;
  }
  public static bool AlignedBottom(ContentAlignment align)
  { return align==ContentAlignment.BottomLeft || align==ContentAlignment.BottomCenter ||
           align==ContentAlignment.BottomRight;
  }
}
#endregion

} // namespace GameLib.Forms
