// TODO: implement mouse cursor
// TODO: add 'other control' to focus events?
// TODO: examine Layout further (implemented properly?)
// TODO: implement optional backing surface
using System;
using System.Collections;
using System.Drawing;
using GameLib.Events;
using GameLib.Video;

namespace GameLib.Forms
{

#region Event arguments and delegates
/// <summary>This class is used in events that refer to a control.</summary>
public class ControlEventArgs : EventArgs
{ public ControlEventArgs(Control control) { Control = control; }
  /// <summary>This member holds the control associated with this event.</summary>
  public Control Control;
}
public delegate void ControlEventHandler(object sender, ControlEventArgs e);

/// <summary>This class is used in events that refer to the mouse cursor being dragged over a control.</summary>
public class DragEventArgs : EventArgs
{
  /// <summary>This method checks whether the specified button was depressed at the time the drag was started.
  /// </summary>
  /// <param name="button">The mouse button to check for (0-7; 0=left, 1=middle, 2=right)</param>
  /// <returns>Returns true if the specified button was depressed at the time the drag was started.</returns>
  public bool Pressed(byte button) { return (Buttons&(1<<button))!=0; }
  /// <summary>
  /// This method sets or clears a bit in the <see cref="Buttons"/> field corresponding to the specified button.
  /// </summary>
  /// <param name="button">The mouse button to set (0-7; 0=left, 1=middle, 2=right)</param>
  /// <param name="down">The button bit will be set if this is true and cleared if false.</param>
  public void SetPressed(byte button, bool down)
  { if(down) Buttons|=(byte)(1<<button); else Buttons&=(byte)~(1<<button);
  }
  /// <summary>
  /// This field holds the beginning of the drag, in window coordinates. It is valid during all of the drag events.
  /// </summary>
  public Point Start;
  /// <summary>This field holds the beginning of the drag, in window coordinates. It is valid during the
  /// <see cref="Control.DragMove"/> and <see cref="Control.DragEnd"/> events.
  /// </summary>
  public Point End;
  /// <summary>
  /// This field holds the mouse buttons that were depressed at the time the drag started. Use the
  /// <see cref="Pressed"/> and <see cref="SetPressed"/> events, which reference this field.
  /// </summary>
  public byte Buttons;
  /// <summary>If this field is set to true in an event handler, the drag will be aborted, and
  /// <see cref="Control.DragEnd"/> will not be called.
  /// </summary>
  public bool Cancel;
}
public delegate void DragEventHandler(object sender, DragEventArgs e);

/// <summary>
/// This class is used in events that draw the control to the screen.
/// </summary>
public class PaintEventArgs : EventArgs
{ 
  /// <param name="control">The control that is to be painted.</param>
  /// <param name="rect">The area within the control to be painted, in window coordinates.
  /// This rectangle is used to generated the <see cref="DisplayRect"/> field.
  /// </param>
  /// <param name="surface">The surface onto which the control should be drawn. This is expected to be the
  /// same surface that's associated with the control's <see cref="Control.Desktop"/> property. If the child
  /// has a backing surface, the surface passed will be ignored, and the child's backing surface will be used
  /// instead. Also, the rectangle passed in will be clipped against the surface's bounds by the constructor.
  /// </param>
  public PaintEventArgs(Control control, Rectangle windowRect, Surface surface)
  { WindowRect = windowRect;
    if(control.backingSurface==null)
    { Surface = surface;
      DisplayRect = control.WindowToDisplay(windowRect);
      if(!Surface.Bounds.Contains(DisplayRect))
      { DisplayRect.Intersect(Surface.Bounds);
        WindowRect = control.DisplayToWindow(DisplayRect);
      }
    }
    else
    { Surface = control.backingSurface;
      WindowRect.Intersect(Surface.Bounds);
      DisplayRect = WindowRect;
    }
  }
  /// <summary>This field holds a reference to the surface upon which the control should be painted.</summary>
  public Surface Surface;
  /// <summary>This rectangle holds the area to be painted, in window coordinates.</summary>
  public Rectangle WindowRect;
  /// <summary>This rectangle holds the area to be painted, in screen coordinates within <see cref="Surface"/>.
  /// </summary>
  public Rectangle DisplayRect;
}
public delegate void PaintEventHandler(object sender, PaintEventArgs e);

/// <summary>This class is used in keyboard events.</summary>
public class KeyEventArgs : EventArgs
{ 
  /// <param name="ke">The internal GameLib event which will be stored into <see cref="KE"/>.</param>
  public KeyEventArgs(KeyboardEvent ke) { KE=ke; }
  /// <summary>The internal GameLib event which contains information about the key.</summary>
  public KeyboardEvent KE;
  /// <summary>This is set to true within an event handler to indicate that the event has been handled and should
  /// not be propogated to other event handling code.
  /// </summary>
  public bool Handled;
}
public delegate void KeyEventHandler(object sender, KeyEventArgs e);

/// <summary>This class is used in mouse button events.</summary>
public class ClickEventArgs : EventArgs
{ 
  /// <summary>This constructor creates an instance with all click information initialized to zero. You should
  /// fill in the <see cref="CE"/> field yourself before using this instance.
  /// </summary>
  public ClickEventArgs() { CE=new MouseClickEvent(); }
  /// <summary>This constructor creates an instance with click information populated from <c>ce</c>.</summary>
  /// <param name="ce">This reference is stored in the <see cref="CE"/> field.</param>
  public ClickEventArgs(MouseClickEvent ce) { CE=ce; }
  /// <summary>This field holds the mouse click information. Fields referring to the location where the
  /// click occurred will be automatically converted to window coordinates when the event handler is called.
  /// </summary>
  public MouseClickEvent CE;
  /// <summary>This is set to true within an event handler to indicate that the event has been handled and should
  /// not be propogated to other event handling code.
  /// </summary>
  public bool Handled;
}
/// <summary>This delegate is used along with <see cref="ClickEventArgs"/> to service mouse click events.</summary>
public delegate void ClickEventHandler(object sender, ClickEventArgs e);
/// <summary>This delegate is used along with <see cref="MouseMoveEvent"/> to service mouse move events.
/// Information in <c>e</c> relating to the location of the mouse movement will be automatically converted
/// to window coordinates when the event handler is called.
/// </summary>
public delegate void MouseMoveEventHandler(object sender, MouseMoveEvent e);
#endregion

#region Control class
/// <summary>
/// This enum is generally used by derived controls to control how they will be treated by the
/// <see cref="DesktopControl">Desktop</see>. The enumeration members can be ORed together to combine their effects.
/// </summary>
[Flags]
public enum ControlStyle
{ 
  /// <summary>The control may not receive click, double-click, or drag events, and cannot receive focus.</summary>
  None=0,
  /// <summary>The control may receive click events.</summary>
  Clickable=1,
  /// <summary>The control may receive double-click events. Without this flag, if the user double-clicks, the
  ///   control will receive two click events.
  /// </summary>
  DoubleClickable=2,
  /// <summary>The control may receive drag events. Without this flag, a drag may be interpreted as a click.</summary>
  Draggable=4,
  /// <summary>The control may receive focus.</summary>
  CanFocus=8,
  /// <summary>This flag is the same as specifying <c>Clickable</c> and <c>DoubleClickable</c>.</summary>
  NormalClick=Clickable|DoubleClickable,
  /// <summary>This flag is the same as specifying <c>Clickable</c>, <c>DoubleClickable</c>, and <c>Draggable</c>.
  /// </summary>
  Anyclick=NormalClick|Draggable,
  /// <summary>Instead of drawing to the desktop directly, the control will have a backing surface to which all
  /// drawing will be done. This is especially useful if it's difficult for the control to keep its drawing within
  /// its window. Transparent background colors for controls with backing surfaces is not supported.
  /// </summary>
  BackingSurface=16
}

/// <summary>
/// This enum specifies where the control will be anchored in relation to its parent control. See the
/// <see cref="Control.Anchor"/> property for more information. The members can be ORed together to combine their
/// effects.
/// </summary>
[Flags]
public enum AnchorStyle // TODO: support more than just corners
{ 
  /// <summary>The control will be anchored to its parent's left edge.</summary>
  Left=1,
  /// <summary>The control will be anchored to its parent's top edge.</summary>
  Top=2,
  /// <summary>The control will be anchored to its parent's right edge.</summary>
  Right=4,
  /// <summary>The control will be anchored to its parent's bottom edge.</summary>
  Bottom=8,
  /// <summary>The control will be anchored to its parent's top left corner.</summary>
  TopLeft=Top|Left,
  /// <summary>The control will be anchored to its parent's top right corner.</summary>
  TopRight=Top|Right,
  /// <summary>The control will be anchored to its parent's bottom left corner.</summary>
  BottomLeft=Bottom|Left,
  /// <summary>The control will be anchored to its parent's bottom right corner.</summary>
  BottomRight=Bottom|Right
}

/// <summary>
/// This enum specifies where the control will be docked. See the <see cref="Control.Dock"/> property for more
/// information.
/// </summary>
public enum DockStyle
{ 
  /// <summary>The control will not be docked.</summary>
  None,
  /// <summary>The control will be docked along its parent's left side.</summary>
  Left,
  /// <summary>The control will be docked along its parent's top side.</summary>
  Top,
  /// <summary>The control will be docked along its parent's right side.</summary>
  Right,
  /// <summary>The control will be docked along its parent's bottom side.</summary>
  Bottom
}

/// <summary>
/// This class is the base class of all controls in the windowing system. Those wanting to create new controls
/// should consider inheriting from the <see cref="Form"/> class (for dialogs), the <see cref="ContainerControl"/>
/// class (for controls that contain other controls), or one of the other controls that already exist.
/// </summary>
public class Control
{ public Control() { controls = new ControlCollection(this); }

  #region ControlCollection
  /// <summary>This class provides a strongly-typed collection of <see cref="Control"/> objects.</summary>
  public class ControlCollection : CollectionBase
  { internal ControlCollection(Control parent) { this.parent=parent; }
    /// <summary>
    /// Gets the control specified by the index given.
    /// </summary>
    /// <param name="index">The zero-based index of the control to return.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <para><paramref name="index"/> is less than zero.</para>
    /// <para>-or-</para>
    /// <para><paramref name="index"/> is equal to or greater than <see cref="ICollection.Count"/>.</para>
    /// </exception>
    public Control this[int index] { get { return (Control)List[index]; } }
    /// <summary>Adds a control as a new child of this control.</summary>
    /// <param name="control">The control to add.</param>
    /// <returns>The index at which the control was added to the collection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="control"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="control"/> already belongs to another control (ie,
    /// its <see cref="Control.Parent"/> property is not null).
    /// </exception>
    /// <event cref="Control.ControlAdded">Raised on this control to notify the application that a new control
    /// was added.
    /// </event>
    /// <event cref="Control.ParentChanged">Raised on the child control to notify it that its parent has changed.
    /// </event>
    public int Add(Control control) { return List.Add(control); }
    /// <summary>Adds several new child controls at once.</summary>
    /// <remarks>This method effectively calls <see cref="Add"/> on each member of <paramref name="controls"/>.
    /// See <see cref="Add"/> for more information on what occurs when this method is called.
    /// </remarks>
    /// <param name="controls">An array containing the controls to add.</param>
    public void AddRange(Control[] controls) { foreach(Control c in controls) List.Add(c); }
    /// <summary>Adds several new child controls at once.</summary>
    /// <remarks>This method effectively calls <see cref="Add"/> on each member of <paramref name="controls"/>.
    /// See <see cref="Add"/> for more information on what occurs when this method is called.
    /// </remarks>
    /// <param name="controls">An array containing the controls to add.</param>
    public void AddRange(params object[] controls) { foreach(Control c in controls) List.Add(c); }
    /// <summary>Returns the index of the specified child control within the collection.</summary>
    /// <returns>If the control is found, the index of the control is returned. Otherwise, -1 is returned.</returns>
    /// <param name="control">A reference to a control to search for.</param>
    public int IndexOf(Control control) { return List.IndexOf(control); }
    /// <summary>Returns the index of the specified child control within the collection.</summary>
    /// <returns>If the control is found, the index of the control is returned. Otherwise, -1 is returned.</returns>
    /// <param name="name">The name of a control to search for. This does a case-sensitive comparison against the
    /// <see cref="Control.Name"/> property.
    /// </param>
    public int IndexOf(string name)
    { for(int i=0; i<Count; i++) if(this[i].Name==name) return i;
      return -1;
    }
    /// <summary>Finds a control by name and returns a reference to it.</summary>
    /// <returns>If the control is found, a reference to it is returned. Otherwise, null is returned.</returns>
    /// <remarks>Calling this method is equivalent to calling <see cref="Find(string, bool)"/> with
    /// <paramref name="deepSearch"/> set to false.
    /// </remarks>
    /// <param name="name">The name of the control. A case-insensitive comparison against the
    /// <see cref="Control.Name"/> property is done.
    /// </param>
    /// <returns>If the control is found, a reference to it will be returned. Otherwise, null will be returned.</returns>
    public Control Find(string name) { return Find(name, false); }
    /// <summary>Finds a control by name and returns a reference to it.</summary>
    /// <returns>If the control is found, a reference to it is returned. Otherwise, null is returned.</returns>
    /// <param name="name">The name of the control. A case-insensitive comparison against the
    /// <see cref="Control.Name"/> property is done.
    /// </param>
    /// <param name="deepSearch">If true, a recursive search is performed, searching descendants if the
    /// named control cannot be found here. If false, the search stops after searching the immediate children.
    /// </param>
    /// <returns>If the control is found, a reference to it will be returned. Otherwise, null will be returned.</returns>
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
    /// <summary>Adds a control as a new child of this control, inserting it at the specified index.</summary>
    /// <param name="index">The index at which to add the control.</param>
    /// <param name="control">The control to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="control"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="control"/> already belongs to another control (ie,
    /// its <see cref="Control.Parent"/> property is not null).
    /// </exception>
    /// <event cref="Control.ControlAdded">Raised on this control to notify the application that a new control
    /// was added.
    /// </event>
    /// <event cref="Control.ParentChanged">Raised on the child control to notify it that its parent has changed.
    /// </event>
    public void Insert(int index, Control control) { List.Insert(index, control); }
    /// <summary>Removes a child control.</summary>
    /// <param name="control">A reference to the control to remove.</param>
    /// <exception cref="ArgumentException"><paramref name="control"/> cannot be found in this collection.</exception>
    /// <event cref="Control.ControlRemoved">Raised on this control to notify the application that a control
    /// was removed.
    /// </event>
    /// <event cref="Control.ParentChanged">Raised on the child control to notify it that its parent has changed.
    /// </event>
    public void Remove(Control control) { List.Remove(control); }
    /// <summary>Returns a value indicating whether the given control exists in this collection.</summary>
    /// <param name="control">The control to search for.</param>
    /// <returns>Returns true if the control was found and false otherwise.</returns>
    public bool Contains(Control control) { return control.parent==parent; }
    /// <summary>Returns a value indicating whether the given control a child or descendant of this one.</summary>
    /// <param name="control">The control to search for.</param>
    /// <param name="deepSearch">If true, a recursive search is performed, searching descendants if the
    /// control cannot be found here. If false, the search stops after searching the immediate children.
    /// </param>
    /// <returns>Returns true if the control was found and false if not.</returns>
    public bool Contains(Control control, bool deepSearch)
    { if(deepSearch)
      { while(control!=null) { if(control==parent) return true; control=control.parent; }
        return false;
      }
      else return control.parent==parent;
    }
    /// <summary>Returns a value indicating whether the named control a child or descendant of this one.</summary>
    /// <returns>Returns true if the control was found and false if not.</returns>
    /// <remarks>Calling this method is equivalent to calling <see cref="Contains(string, bool)"/> with
    /// <paramref name="deepSearch"/> set to false.
    /// </remarks>
    /// <param name="name">The name of the control. A case-insensitive comparison against the
    /// <see cref="Control.Name"/> property is done.
    /// </param>
    public bool Contains(string name) { return Find(name, false)!=null; }
    /// <summary>Returns a value indicating whether the named control a child or descendant of this one.</summary>
    /// <returns>Returns true if the control was found and false if not.</returns>
    /// <remarks>Calling this method is equivalent to calling <see cref="Contains(string, bool)"/> with
    /// <paramref name="deepSearch"/> set to false.
    /// </remarks>
    /// <param name="name">The name of the control. A case-insensitive comparison against the
    /// <see cref="Control.Name"/> property is done.
    /// </param>
    /// <param name="deepSearch">If true, a recursive search is performed, searching descendants if the
    /// named control cannot be found here. If false, the search stops after searching the immediate children.
    /// </param>
    public bool Contains(string name, bool deepSearch) { return Find(name, deepSearch)!=null; }
    
    internal ArrayList Array { get { return InnerList; } }
    
    protected override void OnClear()
    { foreach(Control c in this) c.SetParent(null);
      base.OnClear();
    }
    protected override void OnInsert(int index, object value)
    { Control control = (Control)value;
      if(control==null) throw new ArgumentNullException("control");
      if(control.Parent!=null) throw new ArgumentException("Already belongs to a control!");
      control.SetParent(parent);
      base.OnInsert(index, value);
    }
    protected override void OnRemove(int index, object value)
    { ((Control)value).SetParent(null);
      base.OnRemoveComplete(index, value);
    }
    protected override void OnSet(int index, object oldValue, object newValue)
    { Control control = (Control)newValue;
      if(control==null) throw new ArgumentNullException("control");
      if(control.Parent!=null) throw new ArgumentException("Already belongs to a control!");
      ((Control)oldValue).SetParent(null);
      control.SetParent(parent);
      base.OnSet(index, oldValue, newValue);
    }

    Control parent;
  }
  #endregion

  #region Properties
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
  { get
    { Control p = this;
      while(p.parent!=null) { if(p.parent.focused!=p) return false; p=p.parent; }
      return p is DesktopControl ? true : false;
    }
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

  public bool Modal
  { get
    { DesktopControl desktop = Desktop;
      return desktop != null && desktop.modal.Contains(this);
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

  public bool Selected
  { get { return parent==null ? false : parent.focused==this; }
    set { if(value) Focus(); else Blur(); }
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

  public ControlStyle Style
  { get { return style; }
    set
    { if(value!=style)
      { style=value;
        if((value&ControlStyle.BackingSurface)==0) backingSurface=null;
        else if(backingSurface==null) UpdateBackingSurface(false);
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

  public Rectangle WindowRect { get { return new Rectangle(0, 0, bounds.Width, bounds.Height); } }

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
    ArrayList list = parent.controls.Array;
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
    ArrayList list = parent.controls.Array;
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
    UpdateBackingSurface(false);
  }

  protected virtual void OnSizeChanged(ValueChangedEventArgs e)
  { if(SizeChanged!=null) SizeChanged(this, e);
    if(invalid.Right>bounds.Width) invalid.Width-=invalid.Right-bounds.Width;
    if(invalid.Bottom>bounds.Height) invalid.Height-=invalid.Bottom-bounds.Height;
    if(!pendingLayout && !mychange) // 'mychange' is true when docking
    { if(!layoutSuspended && Events.Events.Initialized) Events.Events.PushEvent(new WindowLayoutEvent(this));
      pendingLayout=true;
    }
    UpdateBackingSurface(false);
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
      desktop.UnsetModal(this);
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

  internal void UpdateBackingSurface(bool forceNew)
  { DesktopControl desktop = Desktop;
    bool hasStyle = (style&ControlStyle.BackingSurface)!=0;
    if(forceNew || ((!hasStyle || desktop==null) && backingSurface!=null) ||
       ((hasStyle || desktop!=null) && (backingSurface==null || Size!=backingSurface.Size)))
    { if(hasStyle && desktop!=null && desktop.Surface!=null)
        backingSurface = desktop.Surface.CreateCompatible(Width, Height, SurfaceFlag.None);
      else backingSurface = null;
    }
  }

  internal Surface backingSurface;
  internal uint lastClickTime = int.MaxValue;

  protected void AssertParent()
  { if(parent==null) throw new InvalidOperationException("This control has no parent");
  }

  protected void SetModal(bool modal)
  { DesktopControl desktop = Desktop;
    if(desktop==null) throw new InvalidOperationException("This control has no desktop");
    if(modal) desktop.SetModal(this);
    else desktop.UnsetModal(this);
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
  bool enabled=true, visible=true, mychange, keyPreview;
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
    { if(value!=surface)
      { surface = value;
        if(surface!=null) Invalidate();
        UpdateBackingSurfaces();
      }
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
      child.OnPaintBackground(pe);
      child.OnPaint(pe);
      if(pe.Surface==child.backingSurface)
      { Point pt = new Point(pe.DisplayRect.X+child.Left, pe.DisplayRect.Y+child.Top);
        pe.Surface.Blit(surface, pe.DisplayRect, pt);
        pe.DisplayRect.Location = pt;
      }

      // TODO: combine rectangles more efficiently
      if(trackUpdates) AddUpdatedArea(pe.DisplayRect);
    }
  }

  public void AddUpdatedArea(Rectangle area)
  { int i;
    for(i=0; i<updatedLen; i++)
    { if(updated[i].Contains(area)) return;
      retest:
      if(area.Contains(updated[i]) && --updatedLen != i)
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
      updated[updatedLen++] = area;
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
      bool passModal = modal.Count==0;
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
        if(!passModal && c==modal[modal.Count-1]) passModal=true;
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
        keyProcessing=true;
        if(heldKey!=null) heldKey.Mods = ea.KE.Mods;
        if(ea.KE.Down)
        { if(krTimer!=null && !ea.KE.IsModKey)
          { krTimer.Change(krDelay, krRate);
            heldKey = ea.KE;
          }
        }
        else if(heldKey!=null && heldKey.Key==ea.KE.Key) StopKeyRepeat();
        DispatchKeyToFocused(ea);
        keyProcessing=false;
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
      bool passModal = modal.Count==0;
      
      at.X -= bounds.X; at.Y -= bounds.Y; // at is the cursor point local to 'p'
      while(p.Enabled && p.Visible)
      { c = p.GetChildAtPoint(at);
        if(c==null) break;
        if(!passModal && c==modal[modal.Count-1]) passModal=true;
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
          if(heldKey!=null)
          { keyProcessing=true;
            DispatchKeyToFocused(new KeyEventArgs(heldKey));
            keyProcessing=false;
          }
          break;
        case WindowEvent.MessageType.Paint: DoPaint(we.Control); break;
        case WindowEvent.MessageType.Layout: we.Control.OnLayout(new EventArgs()); break;
        case WindowEvent.MessageType.DesktopUpdated: if(we.Control!=this) return FilterAction.Continue; break;
        default: we.Control.OnCustomEvent(we); break;
      }
      return FilterAction.Drop;
    }
    #endregion
    return FilterAction.Continue;
  }
  #endregion
  
  public void StopKeyRepeat()
  { if(krTimer!=null)
    { krTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
      heldKey = null;
    }
  }

  protected void Dispose(bool destructor)
  { if(init)
    { Video.Video.ModeChanged -= modeChanged;
      modeChanged = null;
      Events.Events.Deinitialize();
      init = false;
    }
    if(krTimer!=null)
    { krTimer.Dispose();
      krTimer=null;
    }
  }

  internal void SetModal(Control control)
  { if(control.Desktop!=this) throw new InvalidOperationException("The control is not associated with this desktop!");
    if(modal.Contains(control)) UnsetModal(control);
    modal.Add(control);
    if(capturing!=control) capturing=null;
    if(dragging!=null && dragging!=control) EndDrag();
    while(control!=this) { control.Focus(); control=control.Parent; }
  }

  internal void UnsetModal(Control control)
  { if(control.Desktop!=this) throw new InvalidOperationException("The control is not associated with this desktop!");
    modal.Remove(control);
    if(modal.Count>0)
    { control = (Control)modal[modal.Count-1];
      if(capturing!=control) capturing=null;
      if(dragging!=null && dragging!=control) EndDrag();
      while(control!=this) { control.Focus(); control=control.Parent; }
    }
  }

  internal Control capturing;
  internal ArrayList modal = new ArrayList(4);

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
    if(!e.Handled && e.KE.Down && e.KE.Key==tab &&
       (e.KE.KeyMods==Input.KeyMod.None || e.KE.HasOnlyKeys(Input.KeyMod.Shift)))
      TabToNext(e.KE.HasAnyMod(Input.KeyMod.Shift));
    done:
    return !e.Handled;
  }
  #endregion

  void EndDrag()
  { dragging=null;
    dragStarted=false;
    drag.Buttons=0;
  }

  void Init()
  { Events.Events.Initialize();
    init = true;
    drag = new DragEventArgs();
    modeChanged = new ModeChangedHandler(UpdateBackingSurfaces);
    Video.Video.ModeChanged += modeChanged;
  }

  void RepeatKey(object dummy) { if(!keyProcessing) Events.Events.PushEvent(new KeyRepeatEvent()); }

  void TabToNext(bool reverse)
  { Control fc = this;
    while(fc.FocusedControl!=null) fc=fc.FocusedControl;
    (fc==this ? this : fc.Parent).TabToNextControl(reverse);
  }

  void UpdateBackingSurfaces() { UpdateBackingSurfaces(this); }
  void UpdateBackingSurfaces(Control control)
  { control.UpdateBackingSurface(true);
    foreach(Control child in control.Controls) UpdateBackingSurfaces(child);
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
  ModeChangedHandler modeChanged;
  Input.Key tab=Input.Key.Tab;
  ClickStatus clickStatus;
  int   dragThresh=16, enteredLen, updatedLen;
  uint  dcDelay=350, krDelay, krRate=40;
  bool  keys=true, clicks=true, moves=true, init, dragStarted, trackUpdates=true, keyProcessing;
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
