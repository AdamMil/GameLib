// TODO: implement mouse cursor
// TODO: add way to cancel key repeat?

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
  public Point  Start, End;
  public byte   Buttons;
}
public delegate void DragEventHandler(object sender, DragEventArgs e);

public class PaintEventArgs : EventArgs
{ public PaintEventArgs(Control control, Rectangle rect, Surface surface)
  { Surface=surface; ClientRect=rect; DisplayRect=control.RectToDisplay(rect);
  }
  public Surface   Surface;
  public Rectangle ClientRect, DisplayRect;
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

[Flags]
public enum ControlStyle
{ None=0, Clickable=1, DoubleClickable=2, Draggable=4, CanFocus=8,
  NormalClick=Clickable|DoubleClickable, Anyclick=NormalClick|Draggable,
}

#region Control class
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
    public bool Contains(Control control) { return List.Contains(control); }
    public bool Contains(Control child, bool deepSearch)
    { if(Contains(child)) return true;
      if(deepSearch) foreach(Control c in this) if(c.Controls.Contains(child)) return true;
      return false;
    }
    public bool Contains(string name) { return Find(name, false)!=null; }
    public bool Contains(string name, bool deepSearch) { return Find(name, deepSearch)!=null; }
    
    internal IList Array { get { return List; } }
    
    Control parent;
  }
  #endregion

  #region Properties
  public Color BackColor
  { get { return back; }
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
      if(desktop==null) throw new InvalidOperationException("This control has no desktop");
      if(value==true) desktop.capturing = this;
      else if(desktop.capturing==this) desktop.capturing=null;
    }
  }

  public Rectangle ClientRect { get { return new Rectangle(0, 0, bounds.Width, bounds.Height); } }

  public ControlCollection Controls { get { return controls; } }
  
  public IBlittable Cursor
  { get
    { Control c = this;
      while(c!=null) { if(c.cursor!=null) return c.cursor; c=c.parent; }
      return null;
    }
    set { cursor=value; }
  }
  
  public DesktopControl Desktop { get { Control c=TopLevelControl; return c==null ? null : (DesktopControl)c.parent; } }

  public Rectangle DisplayRect { get { return RectToDisplay(ClientRect); } }

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
        mychange=true;
        if(parent!=null) parent.Controls.Remove(this);
        if(value!=null) value.Controls.Add(this);
        mychange=false;
        OnParentChanged(e);
      }
    }
  }

  public Rectangle ParentRect { get { return RectToParent(ClientRect); } }

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

  public int TabIndex
  { get { return tabIndex; }
    set
    { if(value<-1) throw new ArgumentOutOfRangeException("TabIndex", "must be >= -1");
      tabIndex=value;
    }
  }

  public string Text
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

  public Control TopLevelControl
  { get
    { Control p=parent, c=this;
      if(p==null) return null;
      while(p.parent!=null) { c=p; p=p.parent; }
      return c;
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
  #endregion
  
  #region Public methods
  public void BringToFront()
  { CheckParent();
    IList list = parent.controls.Array;
    if(list[0]!=this)
    { list.Remove(this);
      list.Insert(0, this);
    }
  }

  public void Blur()
  { CheckParent();
    if(parent.FocusedControl==this) parent.FocusedControl=null;
  }

  public void Focus()
  { CheckParent();
    if(CanFocus) parent.FocusedControl = this;
  }

  public Control GetChildAtPoint(Point point)
  { foreach(Control c in controls) if(c.bounds.Contains(point)) return c;
    return null;
  }

  public Control GetNextControl() { return GetNextControl(false); }
  public Control GetNextControl(bool reverse)
  { Control next=null, ext=null;
    if(reverse)
    { int tabv=0, maxv=int.MinValue, index = focused==null ? int.MinValue : focused.tabIndex;
      foreach(Control c in controls)
      { if(c.tabIndex<index && c.tabIndex>=tabv) { tabv=c.tabIndex; next=c; }
        if(c.tabIndex>-1 && c.tabIndex>maxv) { maxv=c.tabIndex; ext=c; }
      }
    }
    else
    { int tabv=int.MaxValue, minv=int.MaxValue, index = focused==null ? int.MaxValue : focused.tabIndex;
      foreach(Control c in controls)
      { if(c.tabIndex>index && c.tabIndex<=tabv) { tabv=c.tabIndex; next=c; }
        if(c.tabIndex>-1 && c.tabIndex<minv) { minv=c.tabIndex; ext=c; }
      }
    }
    return next==null ? ext : next;
  }

  public void Invalidate() { Invalidate(ClientRect); }
  public void Invalidate(Rectangle area)
  { area.Intersect(ClientRect);
    if(area.Width==0) return;
    if(BackColor==Color.Transparent && parent!=null) parent.Invalidate(RectToParent(area));
    else
    { if(invalid.Width==0) invalid = area;
      else invalid = Rectangle.Union(area, invalid); 
      if(!pendingPaint && Events.Events.Initialized)
      { pendingPaint=true;
        Events.Events.PushEvent(new WindowPaintEvent(this));
      }
    }
  }

  public Point PointToClient(Point screenPoint)
  { Control c = this;
    while(c!=null) { screenPoint.X-=c.bounds.X; screenPoint.Y-=c.bounds.Y; c=c.parent; }
    return screenPoint;
  }
  public Point PointToChild(Point point, Control child)
  { point.X -= child.bounds.X; point.Y -= child.bounds.Y;
    return point;
  }
  public Point PointToParent(Point point) { point.X+=bounds.X; point.Y+=bounds.Y; return point; }
  public Point PointToDisplay(Point point) { return PointToAncestor(point, null); }
  public Point PointToAncestor(Point point, Control ancestor)
  { Control c = this;
    do { point.X+=c.bounds.X; point.Y+=c.bounds.Y; c=c.parent; } while(c!=ancestor);
    return point;
  }

  public Rectangle RectToClient(Rectangle rect)
  { return new Rectangle(PointToClient(rect.Location), rect.Size);
  }
  public Rectangle RectToChild(Rectangle rect, Control child)
  { rect.X -= child.bounds.X; rect.Y -= child.bounds.Y;
    return rect;
  }
  public Rectangle RectToParent(Rectangle rect) { return new Rectangle(PointToParent(rect.Location), rect.Size); }
  public Rectangle RectToDisplay(Rectangle rect)
  { return new Rectangle(PointToAncestor(rect.Location, null), rect.Size);
  }
  public Rectangle RectToAncestor(Rectangle rect, Control ancestor)
  { return new Rectangle(PointToAncestor(rect.Location, ancestor), rect.Size);
  }

  public void Refresh() { Refresh(ClientRect); }
  public void Refresh(Rectangle area)
  { pendingPaint=true;
    Invalidate(area);
    DesktopControl desktop = Desktop;
    if(desktop!=null) desktop.DoPaint(this);
    else pendingPaint=false;
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
  { CheckParent();
    IList list = parent.controls.Array;
    if(list[list.Count-1]!=this)
    { list.Remove(this);
      list.Insert(list.Count-1, this);
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
    DesktopControl desktop = Desktop;
    if(desktop!=null)
    { Rectangle old = new Rectangle((Point)e.OldValue, bounds.Size);
      desktop.Invalidate(parent==desktop ? old : RectToAncestor(old, desktop));
      Invalidate();
    }
    if(Move!=null) Move(this, new EventArgs());
  }

  protected virtual void OnParentChanged(ValueChangedEventArgs e)
  { if(ParentChanged!=null) ParentChanged(this, e);
  }

  protected virtual void OnSizeChanged(ValueChangedEventArgs e)
  { if(SizeChanged!=null) SizeChanged(this, e);
    if(invalid.Right>bounds.Width) invalid.Width-=invalid.Right-bounds.Width;
    if(invalid.Bottom>bounds.Height) invalid.Height-=invalid.Bottom-bounds.Height;
    if(!pendingLayout)
    { if(!layoutSuspended && Events.Events.Initialized) Events.Events.PushEvent(new WindowLayoutEvent(this));
      pendingLayout=true;
    }
    Size old = (Size)e.OldValue;
    if(parent!=null && (bounds.Width<old.Width || bounds.Height<old.Height))
    { if(bounds.Width<old.Width && bounds.Height<old.Height) // invalidate the smallest rectangle necessary
        parent.Invalidate(new Rectangle(PointToAncestor(bounds.Location, parent), old));
      if(bounds.Width<old.Width)
        parent.Invalidate(new Rectangle(PointToAncestor(new Point(bounds.Width, 0), parent),
                                        new Size(old.Width-bounds.Width, old.Height)));
      else parent.Invalidate(new Rectangle(PointToAncestor(new Point(0, bounds.Height), parent),
                                           new Size(old.Width, old.Height-bounds.Height)));
    }
    if(bounds.Width>old.Width || bounds.Height>old.Height) Invalidate();
    if(Resize!=null) Resize(this, new EventArgs());
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

  protected internal virtual void OnLayout(EventArgs e) { if(Layout!=null) Layout(this, e); }

  protected internal virtual void OnMouseEnter(EventArgs e) { if(MouseEnter!=null) MouseEnter(this, e); }
  protected internal virtual void OnMouseLeave(EventArgs e) { if(MouseLeave!=null) MouseLeave(this, e); }

  protected virtual void OnMove(EventArgs e)   { if(Move!=null) Move(this, e); }
  protected virtual void OnResize(EventArgs e) { if(Resize!=null) Resize(this, e); }

  protected virtual void OnControlAdded(ControlEventArgs e)   { if(ControlAdded!=null) ControlAdded(this, e); }
  protected virtual void OnControlRemoved(ControlEventArgs e)
  { if(focused==e.Control) focused=null;
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
    if(backimg!=null) backimg.Blit(e.Surface, e.ClientRect, e.DisplayRect.X, e.DisplayRect.Y);
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
  protected virtual void OnParentVisibleChanged(ValueChangedEventArgs e) { if(Visible) Invalidate(); }
  
  protected internal virtual void OnCustomEvent(WindowEvent e)
  { throw new NotSupportedException(String.Format("Unhandled message type {0}", e.GetType()));
  }
  #endregion
  
  internal void SetParent(Control control)
  { ValueChangedEventArgs ve = new ValueChangedEventArgs(parent);
    ControlEventArgs ce = new ControlEventArgs(this);
    if(parent!=null) parent.OnControlRemoved(ce);
    parent = control;
    if(parent!=null) parent.OnControlAdded(ce);
    OnParentChanged(ve);
  }
  
  protected internal Control FocusedControl
  { get { return focused; }
    set
    { if(value!=focused)
      { if(value != null && !controls.Contains(value))
          throw new ArgumentException("Not a child of this control", "FocusedControl");
        // FIXME: make sure controls can call .Focus() inside OnLostFocus()
        if(focused!=null) focused.OnLostFocus(new EventArgs());
        focused = value;
        if(value!=null) value.OnGotFocus(new EventArgs());
      }
    }
  }
  
  protected internal bool HasStyle(ControlStyle test) { return (style & test) != ControlStyle.None; }
  protected internal ControlStyle style;
  protected internal uint lastClickTime = int.MaxValue;

  protected void CheckParent() { if(parent==null) throw new InvalidOperationException("This control has no parent"); }
  protected ControlCollection controls;
  protected Control parent, focused;
  protected Rectangle bounds = new Rectangle(0, 0, 100, 100), invalid;
  protected GameLib.Fonts.Font font;
  protected Color back=Color.Transparent, fore=Color.Transparent;
  protected IBlittable backimg, cursor;
  protected string name="", text="";
  protected int tabIndex=-1;
  protected bool enabled=true, visible=true, mychange, pendingPaint, pendingLayout, keyPreview, layoutSuspended;
}
#endregion

#region DesktopControl class
public enum AutoFocus { None=0, Click=1, Hover=2 }

public class DesktopControl : ContainerControl
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
      Size = surface.Size;
      Invalidate();
    }
  }
  public Input.Key TabCharacter { get { return tab; } set { tab=value; } }
  public bool Updated { get { return updated; } set { updated=value; } }
  #endregion

  public void DoPaint() { DoPaint(this); }
  public void DoPaint(Control child)
  { if(surface!=null)
    { PaintEventArgs pe = new PaintEventArgs(child, child.ClientRect, surface);
      child.OnPaintBackground(pe);
      child.OnPaint(pe);
      updated = true;
    }
  }

  #region ProcessEvent
  public FilterAction ProcessEvent(Event e)
  {
    #region Mouse moves
    if(moves && e is MouseMoveEvent)
    { MouseMoveEvent ea = (MouseMoveEvent)e;
      Point at = ea.Point;
      if(dragging==null && capturing==null && !Bounds.Contains(at)) return FilterAction.Continue;

      Control p=this, c;
      at.X -= bounds.X; at.Y -= bounds.Y;
      EventArgs eventArgs=null;
      int ei=0;
      while(p.Enabled && p.Visible)
      { c = p.GetChildAtPoint(at);
        if(ei<enteredLen && c!=entered[ei])
        { if(eventArgs==null) eventArgs=new EventArgs();
          for(int i=enteredLen-1; i>=ei; i--)
          { entered[i].OnMouseLeave(eventArgs);
            entered[i] = null;
          }
          enteredLen = ei;
        }
        if(c==null) break;
        if(ei==enteredLen)
        { if(eventArgs==null) eventArgs=new EventArgs();
          if(enteredLen==enteredMax)
          { Control[] na = new Control[enteredMax*=2];
            Array.Copy(entered, na, enteredLen);
            entered = na;
          }
          entered[enteredLen++] = c;
          c.OnMouseEnter(eventArgs);
        }
        at = p.PointToChild(at, c);
        if(focus==AutoFocus.Hover && c.CanFocus) c.Focus();
        ei++;
        p = c;
      }
      if(focus==AutoFocus.Hover) p.FocusedControl = null;
      
      if(dragging!=null)
      { if(dragStarted)
        { drag.End = p==dragging ? at : dragging.PointToClient(ea.Point);
          dragging.OnDragMove(drag);
        }
        else if(capturing==null || capturing==p)
        { int xd = ea.X-drag.Start.X;
          int yd = ea.Y-drag.Start.Y;
          if(xd*xd+yd*yd >= dragThresh)
          { drag.Start = p.PointToClient(drag.Start);
            drag.End = ea.Point;
            drag.Buttons = ea.Buttons;
            dragStarted = true;
            dragging.OnDragStart(drag);
          }
        }
      }

      if(capturing!=null)
      { ea.Point = at;
        p.OnMouseMove(ea);
      }
      else
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
    { if(focused!=null)
      { KeyEventArgs ea = new KeyEventArgs((KeyboardEvent)e);
        if(ea.KE.Down && ea.KE.Char!=0)
        { if(krTimer!=null)
          { heldChar = ea.KE;
            krTimer.Change(krDelay, krRate);
          }
        }
        else if(heldChar!=null && heldChar.Key==ea.KE.Key)
        { krTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
          heldChar = null;
        }
        DispatchKeyToFocused(ea, false);
        return FilterAction.Drop;
      }
      else
      { KeyboardEvent ke = (KeyboardEvent)e;
        if(ke.Down && ke.Key==tab)
        { TabToNext(ke.HasAnyMod(Input.KeyMod.Shift));
          if(krTimer!=null)
          { heldChar = ke;
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
      if(capturing==null && !dragStarted && !Bounds.Contains(at)) return FilterAction.Continue;
      Control p = this, c;
      uint time = Timing.Msecs;
      
      at.X -= bounds.X; at.Y -= bounds.Y;
      while(p.Enabled && p.Visible)
      { c = p.GetChildAtPoint(at);
        if(c==null) break;
        at = p.PointToChild(at, c);
        if(focus==AutoFocus.Click && ea.CE.Down && c.CanFocus) c.Focus();
        p = c;
      }
      if(p==this)
      { if(focus==AutoFocus.Click && ea.CE.Down && focused!=null) FocusedControl = null;
        if(!dragStarted && capturing==null) goto done;
      }

      if(ea.CE.Down)
      { if(dragging==null && p.HasStyle(ControlStyle.Draggable))
        { dragging = p;
          drag.Start = ea.CE.Point;
          drag.SetPressed(ea.CE.Button, true);
        }
      }
      else if(!dragStarted || drag.Pressed(ea.CE.Button))
      { bool skipClick = dragStarted;
        if(dragStarted)
        { drag.End = dragging.PointToClient(ea.CE.Point);
          dragging.OnDragEnd(drag);
        }
        dragging     = null;
        dragStarted  = false;
        drag.Buttons = 0;
        if(skipClick || (p==this && capturing==null)) goto done;
      }
      else if(p==this && capturing==null) goto done;

      if(capturing!=null)
      { ea.CE.Point = capturing.PointToClient(ea.CE.Point);
        DispatchClickEvent(capturing, ea, time);
      }
      else
      { ea.CE.Point = at;
        do
        { DispatchClickEvent(p, ea, time);
          ea.CE.Point = p.PointToParent(ea.CE.Point);
          p = p.Parent;
        } while(p != this && p.Enabled && p.Visible);
      }
      done:
      if(!ea.CE.Down && ea.CE.Button<8) lastClicked[ea.CE.Button] = null;
      return FilterAction.Drop;
    }
    #endregion
    #region WindowEvent
    else if(e is WindowEvent)
    { WindowEvent we = (WindowEvent)e;
      switch(we.SubType)
      { case WindowEvent.MessageType.KeyRepeat:
          if(heldChar!=null) DispatchKeyToFocused(new KeyEventArgs(heldChar), true);
          break;
        case WindowEvent.MessageType.Paint: DoPaint(we.Control); break;
        case WindowEvent.MessageType.Layout: we.Control.OnLayout(new EventArgs()); break;
        default: we.Control.OnCustomEvent(we); break;
      }
      return FilterAction.Drop;
    }
    #endregion
    else if(e is RepaintEvent) Refresh();
    return FilterAction.Continue;
  }
  #endregion

  protected void Dispose(bool destructor)
  { if(init)
    { Events.Events.Initialize();
      init = false;
    }
  }

  #region Dispatchers
  protected bool DispatchKeyEvent(Control target, KeyEventArgs e, bool repeat)
  { if(e.KE.Down)
    { if(!repeat)
      { target.OnKeyDown(e);
        if(e.Handled) return false;
      }
      if(e.KE.Char!=0) target.OnKeyPress(e);
    }
    else target.OnKeyUp(e);
    return !e.Handled;
  }

  protected bool DispatchClickEvent(Control target, ClickEventArgs e, uint time)
  { if(e.CE.Down)
    { target.OnMouseDown(e);
      if(e.Handled) return false;
    }
    if(target.HasStyle(ControlStyle.NormalClick) && e.CE.Button<8)
    { if(e.CE.Down) lastClicked[e.CE.Button] = target;
      else
      { if(lastClicked[e.CE.Button]==target)
        { if(target.HasStyle(ControlStyle.DoubleClickable) && time-target.lastClickTime<=dcDelay)
            target.OnDoubleClick(e);
          else target.OnMouseClick(e);
          target.lastClickTime = time;
          if(e.Handled) return false;
          lastClicked[e.CE.Button]=target.Parent; // TODO: make sure this is okay with captured/dragged controls
        }
      }
    }
    if(!e.CE.Down) target.OnMouseUp(e);
    return !e.Handled;
  }
  
  protected bool DispatchKeyToFocused(KeyEventArgs e, bool repeat)
  { if(e.Handled) return false;
    if(focused!=null)
    { Control fc = focused;
      while(fc.FocusedControl!=null)
      { if(fc.KeyPreview && !DispatchKeyEvent(fc, e, repeat)) goto done;
        fc = fc.FocusedControl;
      }
      if(!DispatchKeyEvent(fc, e, repeat)) return false;
    }
    done:
    if(e.KE.Down && e.KE.Key==tab) TabToNext(e.KE.HasAnyMod(Input.KeyMod.Shift));
    return !e.Handled;
  }
  #endregion
  
  protected void RepeatKey(object dummy) { Events.Events.PushEvent(new KeyRepeatEvent()); }
  protected void TabToNext(bool reverse)
  { Control fc = this;
    while(fc.FocusedControl!=null) fc=fc.FocusedControl;
    (fc==this ? this : fc.Parent).TabToNextControl(reverse);
  }
  
  protected internal Control capturing;

  protected Surface surface;
  protected AutoFocus   focus;
  protected Control[]   lastClicked=new Control[8], entered=new Control[8];
  protected Control     dragging;
  protected System.Threading.Timer krTimer;
  protected KeyboardEvent heldChar;
  protected DragEventArgs drag;
  protected Input.Key tab=Input.Key.Tab;
  protected int   dragThresh=16, enteredLen, enteredMax=8;
  protected uint  dcDelay=350, krDelay, krRate=50;
  protected bool  keys=true, clicks=true, moves=true, updated, active, init, dragStarted;
  
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
