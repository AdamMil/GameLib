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
{ public DragEventArgs(Point start) { Start=start; }
  public Point  Start, End;
  public object Data;
  public byte   Button;
}
public delegate void DragEventHandler(object sender, DragEventArgs e);

public class PaintEventArgs : EventArgs
{ public PaintEventArgs(Control control, Rectangle rect, Surface surface)
  { Surface=surface; ClientRect=rect; DesktopRect=control.RectToDesktop(rect);
  }
  public Surface   Surface;
  public Rectangle ClientRect, DesktopRect;
}
public delegate void PaintEventHandler(object sender, PaintEventArgs e);

public class KeyEventArgs
{ public KeyEventArgs(KeyboardEvent ke) { KE=ke; }
  public KeyboardEvent KE;
  public bool Handled;
}
public delegate void KeyEventHandler(object sender, KeyEventArgs e);

public class ClickEventArgs
{ public ClickEventArgs(MouseClickEvent ce) { CE=ce; }
  public MouseClickEvent CE;
  public bool Handled;
}
public delegate void ClickEventHandler(object sender, ClickEventArgs e);
public delegate void MouseMoveEventHandler(object sender, MouseMoveEvent e);
#endregion

[Flags]
public enum ControlStyle
{ None=0, AcceptClick=1, AcceptDoubleClick=2, Draggable=4
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

  public bool CanFocus { get { return canFocus && visible && enabled; } }
  public bool Capture
  { get { return capturing; }
    set
    { // TODO: do something
    }
  }

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

  public Rectangle ParentRect  { get { return RectToParent(bounds); } }
  public Rectangle DesktopRect { get { return RectToDesktop(bounds); } }

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
      while(p!=null) { c=p; p=p.parent; }
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

  public void Focus()
  { CheckParent();
    if(CanFocus) parent.FocusedControl = this;
  }

  public void Blur()
  { CheckParent();
    if(parent.FocusedControl==this) parent.FocusedControl=null;
  }

  public Control GetChildAtPoint(Point point)
  { Control front=null;
    int maxz=int.MinValue;
    foreach(Control c in controls) if(c.bounds.Contains(point) && c.zindex>=maxz) { front=c; maxz=c.zindex; }
    return front;
  }

  public Control GetNextControl()
  { Control next=null, min=null;
    int tabv=int.MaxValue, minv=int.MaxValue;
    foreach(Control c in controls)
    { if(c.tabIndex>tabIndex && c.tabIndex<=tabv) { tabv=c.tabIndex; next=c; }
      if(c.tabIndex>-1 && c.tabIndex<=minv) { minv=c.tabIndex; min=c; }
    }
    return next==null ? min : next;
  }

  public void Invalidate() { Invalidate(bounds); }
  public void Invalidate(Rectangle area)
  { area.Intersect(new Rectangle(0, 0, bounds.Width, bounds.Height));
    if(area.Width==0) return;
    if(!pendingPaint)
    { pendingPaint=true;
      // TODO: post paint message?
    }
    if(invalid.Width==0) invalid=area;
    else
    { if(area.X<invalid.X) invalid.X=area.X;
      if(area.Y<invalid.Y) invalid.Y=area.Y;
      if(area.Right>invalid.Right) invalid.Width+=area.Right-invalid.Right;
      if(area.Bottom>invalid.Bottom) invalid.Width+=area.Bottom-invalid.Bottom;
    }
  }

  public Point PointToClient(Point point)
  { Control c = this;
    while(c.parent!=null) { point.X-=c.bounds.X; point.Y-=c.bounds.Y; c=c.parent; }
    return point;
  }

  public Point PointToParent(Point point) { point.X+=bounds.X; point.Y+=bounds.Y; return point; }
  public Point PointToDesktop(Point point) { return PointToAncestor(point, null); }
  public Point PointToAncestor(Point point, Control ancestor)
  { Control c = this;
    if(ancestor==null) while(c.parent!=null) { point.X+=c.bounds.X; point.Y+=c.bounds.Y; c=c.parent; }
    else do { point.X+=c.bounds.X; point.Y+=c.bounds.Y; c=c.parent; } while(c!=ancestor);
    return point;
  }

  public Rectangle RectToClient(Rectangle rect)
  { return new Rectangle(PointToClient(rect.Location), rect.Size);
  }

  public Rectangle RectToParent(Rectangle rect) { return new Rectangle(PointToParent(rect.Location), rect.Size); }
  public Rectangle RectToDesktop(Rectangle rect) { return RectToAncestor(rect, null); }
  public Rectangle RectToAncestor(Rectangle rect, Control ancestor)
  { return new Rectangle(PointToAncestor(rect.Location, ancestor), rect.Size);
  }

  public void Refresh() { Refresh(bounds); }
  public void Refresh(Rectangle area)
  { pendingPaint=true;
    Invalidate(area);
    DesktopControl desktop = Desktop;
    if(desktop!=null) desktop.DoPaint(this);
  }

  public void ResumeLayout() { ResumeLayout(true); }
  public void ResumeLayout(bool updateLayout)
  { layoutSuspended=false;
    if(updateLayout)
    { // TODO: update layout if necessary
    }
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
  #endregion
  
  #region Events
  public event ValueChangedEventHandler BackColorChanged, BackImageChanged, EnabledChanged, FontChanged,
    ForeColorChanged, LocationChanged, ParentChanged, SizeChanged, TabIndexChanged, TextChanged, VisibleChanged;
  public event EventHandler GotFocus, LostFocus, Layout, MouseEnter, MouseLeave, Move, Resize;
  public event ControlEventHandler ControlAdded, ControlRemoved;
  public event KeyEventHandler KeyDown, KeyUp, KeyPress;
  public event MouseMoveEventHandler MouseMove;
  public event ClickEventHandler MouseDown, MouseUp, Click, DoubleClick;
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
    // TODO: handle this
    if(Move!=null) Move(this, new EventArgs());
  }

  protected virtual void OnParentChanged(ValueChangedEventArgs e)
  { if(ParentChanged!=null) ParentChanged(this, e);
  }

  protected virtual void OnSizeChanged(ValueChangedEventArgs e)
  { if(SizeChanged!=null) SizeChanged(this, e);
    if(invalid.Right>bounds.Width) invalid.Width-=invalid.Right-bounds.Width;
    if(invalid.Bottom>bounds.Height) invalid.Height-=invalid.Bottom-bounds.Height;
    // TODO: mark as needing layout
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
    { foreach(Control c in controls) c.OnParentVisibleChanged(e);
      // TODO: repaint desktop
    }
  }

  protected virtual void OnGotFocus(EventArgs e)  { if(GotFocus!=null) GotFocus(this, e); }
  protected virtual void OnLostFocus(EventArgs e) { if(LostFocus!=null) LostFocus(this, e); }

  protected virtual void OnLayout(EventArgs e) { if(Layout!=null) Layout(this, e); }

  protected internal virtual void OnMouseEnter(EventArgs e) { if(MouseEnter!=null) MouseEnter(this, e); }
  protected internal virtual void OnMouseLeave(EventArgs e) { if(MouseLeave!=null) MouseLeave(this, e); }

  protected virtual void OnMove(EventArgs e)   { if(Move!=null) Move(this, e); }
  protected virtual void OnResize(EventArgs e) { if(Resize!=null) Resize(this, e); }

  protected virtual void OnControlAdded(ControlEventArgs e)   { if(ControlAdded!=null) ControlAdded(this, e); }
  protected virtual void OnControlRemoved(ControlEventArgs e) { if(ControlRemoved!=null) ControlRemoved(this, e); }

  protected internal virtual void OnKeyDown(KeyEventArgs e)  { if(KeyDown!=null) KeyDown(this, e); }
  protected internal virtual void OnKeyUp(KeyEventArgs e)    { if(KeyUp!=null) KeyUp(this, e); }
  protected internal virtual void OnKeyPress(KeyEventArgs e) { if(KeyPress!=null) KeyPress(this, e); }

  protected internal virtual void OnMouseMove(MouseMoveEvent e) { if(MouseMove!=null) MouseMove(this, e); }

  protected internal virtual void OnMouseDown(ClickEventArgs e)   { if(MouseDown!=null) MouseDown(this, e); }
  protected internal virtual void OnMouseUp(ClickEventArgs e)     { if(MouseUp!=null) MouseUp(this, e); }
  protected internal virtual void OnClick(ClickEventArgs e)       { if(Click!=null) Click(this, e); }
  protected internal virtual void OnDoubleClick(ClickEventArgs e) { if(DoubleClick!=null) DoubleClick(this, e); }
  
  protected internal virtual void OnDragStart(DragEventArgs e) { if(DragStart!=null) DragStart(this, e); }
  protected internal virtual void OnDragMove(DragEventArgs e)  { if(DragMove!=null) DragMove(this, e); }
  protected internal virtual void OnDragEnd(DragEventArgs e)   { if(DragEnd!=null) DragEnd(this, e); }

  protected virtual void OnPaintBackground(PaintEventArgs e)
  { if(back!=Color.Transparent) e.Surface.Fill(e.DesktopRect, back);
    if(backimg!=null) backimg.Blit(e.Surface, e.ClientRect, e.DesktopRect.X, e.DesktopRect.Y);
    if(PaintBackground!=null) PaintBackground(this, e);
  }
  protected virtual void OnPaint(PaintEventArgs e)
  { if(Paint!=null) Paint(this, e);
    pendingPaint=false;
  }
  
  protected virtual void OnParentBackColorChanged(ValueChangedEventArgs e)
  { if(back==Color.Transparent) Invalidate();
  }
  protected virtual void OnParentBackImageChanged(ValueChangedEventArgs e)
  { if(back==Color.Transparent) Invalidate();
  }
  protected virtual void OnParentEnabledChanged(ValueChangedEventArgs e) { if(Enabled==(bool)e.OldValue) Invalidate(); }
  protected virtual void OnParentFontChanged(ValueChangedEventArgs e) { if(font==null) Invalidate(); }
  protected virtual void OnParentForeColorChanged(ValueChangedEventArgs e)
  { if(fore==Color.Transparent) Invalidate();
  }
  protected virtual void OnParentVisibleChanged(ValueChangedEventArgs e) { if(Visible) Invalidate(); }
  #endregion
  
  internal void SetParent(Control control)
  { ValueChangedEventArgs e = new ValueChangedEventArgs(parent);
    parent = control;
    OnParentChanged(e);
  }
  
  protected internal Control FocusedControl
  { get { return focused; }
    set
    { if(value!=focused)
      { if(!Controls.Contains(focused)) throw new ArgumentException("Not a child of this control", "FocusedControl");
        // FIXME: make sure controls can call .Focus() inside OnLostFocus()
        if(focused!=null) focused.OnLostFocus(new EventArgs());
        if(value!=null) value.OnLostFocus(new EventArgs());
        focused = value;
      }
    }
  }
  
  protected internal ControlStyle style;
  protected internal uint lastClickTime;

  protected void CheckParent() { if(parent==null) throw new InvalidOperationException("This control has no parent"); }
  protected ControlCollection controls;
  protected Control parent, focused;
  protected Rectangle bounds, invalid;
  protected GameLib.Fonts.Font font;
  protected Color back=Color.Transparent, fore=Color.Transparent;
  protected IBlittable backimg, cursor;
  protected string name="", text="";
  protected int tabIndex=-1, zindex;
  protected bool enabled=true, visible=true, canFocus=true, capturing=false, mychange=false, pendingPaint=false;
  protected bool keyPreview=false, layoutSuspended=false;
}
#endregion

public class ContainerControl : Control
{ protected override void OnPaint(PaintEventArgs e)
  { foreach(Control c in controls)
    { Rectangle paint = Rectangle.Intersect(c.ParentRect, e.ClientRect);
      if(paint.Width>0)
      { paint.X -= c.Left; paint.Y -= c.Top;
        c.Refresh(paint);
      }
    }
    base.OnPaint(e);
  }
}

#region DesktopControl class
public enum AutoFocus { None=0, Click=1, Hover=2 }

public class DesktopControl : ContainerControl
{ ~DesktopControl() { Dispose(true); }
  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  public AutoFocus AutoFocusing { get { return focus; } set { focus=value; } }
  public bool ProcessKeys       { get { return keys; } set { keys=value; } }
  public bool ProcessMouseMove  { get { return moves; } set { moves=value; } }
  public bool ProcessClicks     { get { return clicks; } set { clicks=value; } }

  public Surface Surface
  { get { return surface; }
    set
    { Size = surface.Size;
      Invalidate();
      surface = value;
    }
  }

  public void Activate()
  { if(filter==null)
    { filter = new EventFilter(Filter);
      Events.Events.EventFilter += filter;
    }
  }

  public void Deactivate()
  { if(filter!=null)
    { Events.Events.EventFilter -= filter;
      filter = null;
    }
  }

  public void DoPaint() { DoPaint(this); }
  public void DoPaint(Control child) { if(surface!=null) OnPaint(new PaintEventArgs(child, child.Bounds, surface)); }

  protected void Dispose(bool destructor) { Deactivate(); }

  protected virtual FilterAction Filter(Event e)
  { // TODO: implement capturing
    if(moves && e is MouseMoveEvent)
    { // TODO: handle these
    }
    else if(keys && e is KeyboardEvent) // TODO: implement key repeat
    { if(focused!=null)
      { KeyEventArgs ea = new KeyEventArgs((KeyboardEvent)e);
        Control fc = focused;
        while(fc.FocusedControl!=null)
        { if(fc.KeyPreview && !DispatchKeyEvent(fc, ea)) break;
          fc = fc.FocusedControl;
        }
        if(!ea.Handled) DispatchKeyEvent(fc, ea);
        return FilterAction.Drop;
      }
    }
    else if(clicks && e is MouseClickEvent)
    { // TODO: handle these
    }
    else if(e is WindowEvent)
    { // TODO: handle these
    }
    return FilterAction.Continue;
  }

  protected bool DispatchKeyEvent(Control target, KeyEventArgs e)
  { if(e.KE.Down)
    { target.OnKeyDown(e);
      if(e.Handled) return false;
      if(e.KE.Char!=0) target.OnKeyPress(e);
    }
    else target.OnKeyUp(e);
    return !e.Handled;
  }

  protected Surface surface;
  protected EventFilter filter;
  protected AutoFocus   focus;
  protected bool keys=true, clicks=true, moves=true;
}
#endregion

} // namespace GameLib.Forms
