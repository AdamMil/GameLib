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

// TODO: implement mouse cursor
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
  /// <summary>This method checks whether only the specified button was depressed at the time the drag was started.
  /// </summary>
  /// <param name="button">The mouse button to check for</param>
  /// <returns>Returns true if only the specified button was depressed at the time the drag was started.</returns>
  public bool OnlyPressed(Input.MouseButton button) { return Buttons==(1<<(byte)button); }
  /// <summary>This method checks whether the specified button was depressed at the time the drag was started.
  /// </summary>
  /// <param name="button">The mouse button to check for</param>
  /// <returns>Returns true if the specified button was depressed at the time the drag was started.</returns>
  public bool Pressed(Input.MouseButton button) { return (Buttons&(1<<(byte)button))!=0; }
  /// <summary>
  /// This method sets or clears a bit in the <see cref="Buttons"/> field corresponding to the specified button.
  /// </summary>
  /// <param name="button">The mouse button to set</param>
  /// <param name="down">The button bit will be set if this is true and cleared if false.</param>
  public void SetPressed(Input.MouseButton button, bool down)
  { if(down) Buttons|=(byte)(1<<(byte)button); else Buttons&=(byte)~(1<<(byte)button);
  }
  /// <summary>This property returns a rectangle that represents the area through which the mouse was dragged.
  /// </summary>
  public Rectangle Rectangle
  { get
    { int x=Math.Min(Start.X, End.X), x2=Math.Max(Start.X, End.X);
      int y=Math.Min(Start.Y, End.Y), y2=Math.Max(Start.Y, End.Y);
      return new Rectangle(x, y, x2-x+1, y2-y+1);
    }
  }
  /// <summary>
  /// This field holds the beginning of the drag, in control coordinates. It is valid during all of the drag events.
  /// </summary>
  public Point Start;
  /// <summary>This field holds the beginning of the drag, in control coordinates. It is valid during the
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
  /// <param name="windowRect">The area within the control to be painted, in control coordinates.
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
  /// <summary>This rectangle holds the area to be painted, in control coordinates.</summary>
  public Rectangle WindowRect;
  /// <summary>This rectangle holds the area to be painted, in display coordinates within <see cref="Surface"/>.
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
  /// click occurred will be automatically converted to control coordinates when the event handler is called.
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
/// to control coordinates when the event handler is called.
/// </summary>
public delegate void MouseMoveEventHandler(object sender, MouseMoveEvent e);
#endregion

#region Control class
/// <summary>
/// This enum is generally used by derived controls to control how they will be treated by the
/// <see cref="DesktopControl">Desktop</see>. The enumeration members can be ORed together to combine their effects.
/// </summary>
/// <remarks>
/// THREADING CONSIDERATIONS
/// <para>This class, and classes derived from it, are not designed to be used from multiple threads simultaneously.
/// The thread that reads the events (see <see cref="GameLib.Events.Events">Events</see>) should be the only thread
/// to use this control. If you want to trigger an event from another thread, the recommended implementation is to
/// push a custom event, derived from <see cref="WindowEvent"/>, with the <see cref="WindowEvent.Control"/> field
/// set to the control you want to notify. The control can then use <see cref="Control.OnCustomEvent"/> to handle
/// the event.
/// </para>
/// </remarks>
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
  /// its window. Transparent background colors for controls with backing surfaces are not supported.
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
  BottomRight=Bottom|Right,
  /// <summary>The control will be anchored to its parent's left and right sides.</summary>
  LeftRight=Left|Right,
  /// <summary>The control will be anchored to its parent's top and bottom edges.</summary>
  TopBottom=Top|Bottom,
  /// <summary>The control will be anchored to all four corners of its parent.</summary>
  All=TopLeft|BottomRight,
  /// <summary>The default anchoring mode, which performs no anchoring (equal to TopLeft).</summary>
  None=TopLeft
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
    public void Insert(int index, Control control) { List.Insert(index, control); }
    /// <summary>Removes a child control.</summary>
    /// <param name="control">A reference to the control to remove.</param>
    /// <exception cref="ArgumentException"><paramref name="control"/> cannot be found in this collection.</exception>
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
      base.OnRemove(index, value);
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
  /// <summary>Determines where the control will be anchored in relation to its parent.</summary>
  /// <remarks>When a control is anchored, it will be automatically moved and/or resized when its parent is resized
  ///   in order to maintain the same spacing to the anchored edges. Setting this property will automatically set
  ///   the <see cref="Dock"/> property to <see cref="DockStyle.None"/> because the two properties are incompatible.
  /// </remarks>
  public AnchorStyle Anchor
  { get { return anchor; }
    set
    { if(anchor!=value)
      { if(value!=AnchorStyle.None) dock=DockStyle.None;
        anchor=value;
        if(parent!=null) UpdateAnchor();
      }
    }
  }

  /// <summary>Gets or sets the effective background color for this control.</summary>
  /// <remarks>Setting this property will alter the <see cref="RawBackColor"/> property of this control. Setting
  /// it to <see cref="System.Drawing.Color.Transparent">Color.Transparent</see> will cause it to inherit the
  /// background color of its parent. Reading this property will return the effective background color, after
  /// taking inheritance into account.
  /// </remarks>
  /// <value>The effective background color of this control.</value>
  public Color BackColor
  { get
    { Control c = this;
      while(c!=null) { if(c.back!=Color.Transparent) return c.back; c=c.parent; }
      return Color.Transparent;
    }
    set
    { if(value!=back)
      { Color old = BackColor;
        back = value;
        if(BackColor!=old) OnBackColorChanged(new ValueChangedEventArgs(old));
      }
    }
  }

  /// <summary>Gets or sets the background image for this control.</summary>
  /// <remarks>Setting the background image causes the image to be displayed in the background of the control.
  /// The alignment of the background image can be controlled using the <see cref="BackImageAlign"/> property.
  /// </remarks>
  public IBlittable BackImage
  { get { return backImage; }
    set
    { if(value!=backImage)
      { ValueChangedEventArgs e = new ValueChangedEventArgs(backImage);
        backImage = value;
        OnBackImageChanged(e);
      }
    }
  }

  /// <summary>Gets or sets the alignment of the background image.</summary>
  /// <remarks>This setting controls where the <see cref="BackImage"/> will be displayed relative to the control.
  /// </remarks>
  public ContentAlignment BackImageAlign
  { get { return backImageAlign; }
    set
    { if(value!=backImageAlign)
      { ValueChangedEventArgs e = new ValueChangedEventArgs(backImageAlign);
        backImageAlign = value;
        OnBackImageAlignChanged(e);
      }
    }
  }

  /// <summary>Gets or sets the position of the bottom edge of the control relative to its parent.</summary>
  /// <remarks>Changing this property will move the control.</remarks>
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

  /// <summary>Gets or sets the location and size of the control relative to its parent.</summary>
  /// <remarks>Changing this property will both move and resize the control.</remarks>
  public Rectangle Bounds
  { get { return bounds; }
    set
    { Location = value.Location;
      Size     = value.Size;
    }
  }

  /// <summary>This property is true if the control can receive focus.</summary>
  /// <remarks>This property will return true if the <see cref="ControlStyle"/> property has the
  /// <see cref="ControlStyle.CanFocus"/> flag, and the control is visible and enabled. Otherwise, it will return
  /// false.
  /// </remarks>
  public bool CanFocus { get { return (style&ControlStyle.CanFocus)!=ControlStyle.None && visible && enabled; } }

  /// <summary>Enables or disables mouse capture for this control.</summary>
  /// <remarks>Mouse capture allows a control to receive mouse events even if the mouse is not within the bounds
  /// of the control. Because mouse capture requires an active desktop, this property cannot be set unless the
  /// control is associated with a desktop. For information on associating a control with a desktop, see the
  /// <see cref="Desktop"/> property. Since only one control can capture the mouse at a time, setting this to true
  /// will take the mouse capture away from any other control that has mouse capture.
  /// </remarks>
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

  /// <summary>Gets the <see cref="ControlCollection"/> containing this control's children.</summary>
  public ControlCollection Controls { get { return controls; } }
  
  /// <summary>Gets or sets the effective mouse cursor associated with this control.</summary>
  /// <remarks>Setting this property sets the <see cref="RawCursor"/> property. Setting this property to null will
  /// cause the mouse cursor to be inherited from the control's parent. Getting this property takes inheritance into
  /// account and returns the effective mouse cursor.
  /// </remarks>
  public IBlittable Cursor
  { get
    { Control c = this;
      while(c!=null) { if(c.cursor!=null) return c.cursor; c=c.parent; }
      return null;
    }
    set { cursor=value; }
  }
  
  /// <summary>Gets the desktop with which this control is associated.</summary>
  /// <remarks>Controls are associated with other controls through their <see cref="Parent"/> and
  /// <see cref="Controls"/> properties. If an ancestor of a control is derived from <see cref="DesktopControl"/>,
  /// the control is associated with that desktop.
  /// </remarks>
  public DesktopControl Desktop
  { get
    { Control p = this;
      while(p.parent!=null) p=p.parent;
      return p as DesktopControl;
    }
  }

  /// <summary>Gets the that rectangle this control occupies on the display surface.</summary>
  /// <remarks>The display surface is the <see cref="GameLib.Video.Surface"/> that's associated with this control's
  /// desktop (see <see cref="Desktop"/>). This property returns the rectangle that this control occupies on the
  /// display surface. This is not necessarily the rectangle that is safe to draw into during a paint event!
  /// More specifically, if the control has a backing surface (that is to say, if the <see cref="ControlStyle"/>
  /// property has the <see cref="ControlStyle.BackingSurface"/> flag), this property is not the same as the
  /// rectangle in which you can draw. For controls that have backing surface, the rectangle in which you can draw
  /// is the entire backing surface. For controls that don't have a backing surface, this is the rectangle in which
  /// it's safe to draw during a paint operation.
  /// </remarks>
  public Rectangle DisplayRect { get { return WindowToDisplay(WindowRect); } }

  /// <summary>Gets or sets how this control will be docked to its parent.</summary>
  /// <remarks>When a control is docked, it will be automatically moved and/or resized when its parent is resized
  /// in order to fill the entire edge that it's docked to. Multiple controls can be docked at once, even to the
  /// same edge. The docking calculations for the controls are performed in the same order that the controls are
  /// in the <see cref="Controls"/> collections. Setting this property will automatically set the
  /// <see cref="Anchor"/> property to <see cref="AnchorStyle.None"/> because the two properties are incompatible.
  /// </remarks>
  public DockStyle Dock
  { get { return dock; }
    set { if(value!=dock) { throw new NotImplementedException(); } }
  }

  /// <summary>Gets or sets whether this control can receive input.</summary>
  /// <remarks>Setting this property will alter the <see cref="RawEnabled"/> property of this control. 
  /// Setting it to true will cause it to inherit the enabled status of its parent. Reading this property will
  /// return whether this control is effectively enabled, taking inheritance into account. A control that is not
  /// enabled will not be able to receive user input events.
  /// </remarks>
  public bool Enabled
  { get
    { Control c = this;
      while(c!=null) { if(c.enabled==false) return false; c=c.parent; }
      return true;
    }
    set
    { if(value!=enabled)
      { bool old = Enabled;
        enabled = value;
        if(Enabled!=old) OnEnabledChanged(new ValueChangedEventArgs(old));
      }
    }
  }

  /// <summary>Returns true if this control has input focus.</summary>
  /// <remarks>The control that has input focus will receive keyboard events. Many controls can be focused
  /// (relative to their parent) but only one control can have input focus -- the control whose ancestors are
  /// all focused as well. This property returns true if this control has input focus. To set input focus, use
  /// the <see cref="Focus"/> method.
  /// </remarks>
  public bool Focused
  { get
    { Control p = this;
      while(p.parent!=null) { if(p.parent.focused!=p) return false; p=p.parent; }
      return p is DesktopControl ? true : false;
    }
  }

  /// <summary>Gets or sets the font that this control should use.</summary>
  /// <remarks>Setting this property will alter the <see cref="RawFont"/> property of this control. 
  /// Setting it to <c>null</c> will cause it to inherit the its parent's font. Reading this property will
  /// take inheritance into account and return the effective font for this control. Since the font is likely
  /// shared with other controls, you should set the font's properties (such as the color) before using it.
  /// </remarks>
  public GameLib.Fonts.Font Font
  { get
    { Control c = this;
      while(c!=null) { if(c.font!=null) return c.font; c=c.parent; }
      return null;
    }
    set
    { if(value!=font)
      { GameLib.Fonts.Font old = Font;
        font = value;
        if(Font!=old) OnFontChanged(new ValueChangedEventArgs(old));
      }
    }
  }

  /// <summary>Gets or sets the foreground color control should use.</summary>
  /// <remarks>Setting this property will alter the <see cref="RawForeColor"/> property of this control. 
  /// Setting it to <see cref="System.Drawing.Color.Transparent">Color.Transparent</see> will cause it to
  /// inherit its parent's foreground color. Reading this property will take inheritance into account and
  /// return the effective foreground color for this control.
  /// </remarks>
  public Color ForeColor
  { get
    { Control c = this;
      while(c!=null) { if(c.fore!=Color.Transparent) return c.fore; c=c.parent; }
      return Color.Transparent;
    }
    set
    { if(value!=fore)
      { Color old=ForeColor;
        fore = value;
        if(ForeColor!=old) OnForeColorChanged(new ValueChangedEventArgs(old));
      }
    }
  }

  /// <summary>Returns true if this control has any child controls.</summary>
  public bool HasChildren { get { return controls.Count>0; } }

  /// <summary>Gets or sets the height of this control, in pixels.</summary>
  /// <remarks>Changing this property will resize the control.</remarks>
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

  /// <summary>Gets or sets the area of the control that must be painted.</summary>
  /// <remarks>This property holds the rectangle, relative to the <see cref="WindowRect"/>, of the bounding box
  /// of the area that
  /// is invalid and should be painted on the next call to <see cref="OnPaint"/>. Generally, this property will
  /// not be used directly. Use the <see cref="Invalidate"/> and <see cref="Refresh"/> methods to mark areas of
  /// the control as invalid.
  /// </remarks>
  public Rectangle InvalidRect
  { get { return invalid; }
    set
    { invalid.Width=0;
      Invalidate(value);
    }
  }
  
  /// <summary>Gets or sets whether this control receives keyboard events before its children.</summary>
  /// <remarks>If a control has key preview, it will receive keyboard events before its children, and have
  /// a chance to process and/or cancel them.
  /// </remarks>
  public bool KeyPreview { get { return keyPreview; } set { keyPreview=value; } }
  
  /// <summary>Gets or sets the position of the left edge of the control relative to its parent.</summary>
  /// <remarks>Changing this property will move the control.</remarks>
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

  /// <summary>Gets or sets the location of this control's top-left point, relative to its parent.</summary>
  /// <remarks>Changing this property will move the control.</remarks>
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

  /// <summary>Returns true if this is a modal control.</summary>
  /// <remarks>A modal control will receive input focus and all keyboard events, and cannot lose focus unless a new
  /// modal control is opened after it. You can use the <see cref="TopModal"/> property to check whether this
  /// control is the topmost modal control (the one with focus).
  /// </remarks>
  public bool Modal
  { get
    { DesktopControl desktop = Desktop;
      return desktop != null && desktop.modal.Contains(this);
    }
  }

  /// <summary>Gets or sets name of this control.</summary>
  /// <remarks>The name can be used to locate a control within its parent easily, using the
  /// <see cref="ControlCollection.Find"/> methods, but is otherwise not used by the windowing system.
  /// For proper functioning, the name should be unique among all its ancestors and their descendants, and
  /// should follow this naming convention: <c>[a-zA-Z_][a-zA-Z0-9_]*</c>. Essentially, a letter or underscore
  /// followed by possibly more letters, digits, and/or underscores.
  /// </remarks>
  public string Name
  { get { return name; }
    set
    { if(value==null) throw new ArgumentNullException("Name");
      name=value;
    }
  }

  /// <summary>Gets or sets this control's parent control.</summary>
  /// <remarks>This property will return null if the control has no parent. Setting this property will
  /// remove this control from its current parent, if any, and add it to the new parent, if any.
  /// </remarks>
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

  /// <summary>Gets this control's raw background color.</summary>
  /// <remarks>Generally, the <see cref="BackColor"/> property should be used to get the background color,
  /// but if you want to get it without taking inheritance into account, use this one.
  /// </remarks>
  public Color RawBackColor   { get { return back; } }

  /// <summary>Gets this control's raw cursor.</summary>
  /// <remarks>Generally, the <see cref="Cursor"/> property should be used to get the cursor, but if you want
  /// to get it without taking inheritance into account, use this one.
  /// </remarks>
  public IBlittable RawCursor { get { return cursor; } }

  /// <summary>Gets this control's raw enabled value.</summary>
  /// <remarks>Generally, the <see cref="Enabled"/> property should be used to determine if the control is enabled,
  /// but if you don't want inheritance to be taken into account, use this one.
  /// </remarks>
  public bool RawEnabled      { get { return enabled; } }

  /// <summary>Gets this control's raw font.</summary>
  /// <remarks>Generally, the <see cref="Font"/> property should be used to get the font, but if you want
  /// to get it without taking inheritance into account, use this one.
  /// </remarks>
  public GameLib.Fonts.Font RawFont { get { return font; } }

  /// <summary>Gets this control's raw foreground color.</summary>
  /// <remarks>Generally, the <see cref="ForeColor"/> property should be used to get the foreground color,
  /// but if you want to get it without taking inheritance into account, use this one.
  /// </remarks>
  public Color RawForeColor   { get { return fore; } }

  /// <summary>Gets this control's raw visible value.</summary>
  /// <remarks>Generally, the <see cref="Visible"/> property should be used to determine if the control is visible,
  /// but if you don't want inheritance to be taken into account, use this one.
  /// </remarks>
  public bool RawVisible      { get { return visible; } }

  /// <summary>Gets or sets the position of the right edge of the control relative to its parent.</summary>
  /// <remarks>Changing this property will move the control.</remarks>
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

  /// <summary>Gets or sets whether this control is its parent's focused control.</summary>
  /// <remarks>See the <see cref="Focused"/> property for more information on input focusing. Unlike the
  /// <see cref="Focused"/> property, which will return true only this control and all of its ancestors are
  /// focused, this property only considers whether or not this control is focused. Thus, this property is not
  /// for determining whether this control actual input focus. Setting this property is equivalent to calling
  /// <see cref="Focus"/> or <see cref="Blur"/>, depending on whether the value is true or false, respectively.
  /// </remarks>
  public bool Selected
  { get { return parent==null ? false : parent.focused==this; }
    set { if(value) Focus(); else Blur(); }
  }
  
  /// <summary>Gets or sets the size of this control.</summary>
  /// <remarks>Changing this control will resize the control.</remarks>
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

  /// <summary>Gets or sets this control's <see cref="ControlStyle"/>.</summary>
  /// <remarks>This property is generally designed to be set by the implementors of controls. Altering this
  /// property could invalidate assumptions made by the control and cause problems. See the
  /// <see cref="ControlStyle"/> enum for more information about control styles.
  /// </remarks>
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

  /// <summary>Gets or sets this control's position within the tab order.</summary>
  /// <remarks>Controls are often meant to be used in a certain order. By setting this property, you can alter
  /// the logical order of controls. <see cref="TabToNextControl"/> and <see cref="DesktopControl"/> both use
  /// this property to determine in what order controls should be focused. The default value is -1, which means
  /// that this control is not part of the tab ordering.
  /// </remarks>
  public int TabIndex
  { get { return tabIndex; }
    set
    { if(value<-1) throw new ArgumentOutOfRangeException("TabIndex", "must be >= -1");
      tabIndex=value;
    }
  }

  /// <summary>Gets or sets a bit of user data associated with this control.</summary>
  /// <remarks>This property is not altered or used by the windowing system in any way. It is meant to be used
  /// to associate any context with controls that might be helpful. The <see cref="Name"/> property can be used
  /// similarly, but with some restrictions.
  /// </remarks>
  public object Tag { get { return tag; } set { tag=value; } }
  
  /// <summary>Gets or sets this control's text.</summary>
  /// <remarks>Different types of controls use this property differently. For <see cref="Form"/>, it's the window
  /// caption. For <see cref="ButtonBase"/>, it's the text displayed along with the button. For
  /// <see cref="MenuBase"/> and <see cref="MenuItemBase"/>, it's the text displayed for the menu. For
  /// <see cref="TextBoxBase"/>, it's the text entered by the user. Other controls may use this differently.
  /// See the documentation for derived classes to see how they treat this property.
  /// </remarks>
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

  /// <summary>Gets or sets the position of the top edge of the control relative to its parent.</summary>
  /// <remarks>Changing this property will move the control.</remarks>
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

  /// <summary>Returns true if this is the topmost modal control.</summary>
  /// <remarks>A modal control will receive input focus and all keyboard events, and cannot lose focus unless a new
  /// modal control is opened after it. You can use this property to check whether this control is the topmost
  /// modal control (the one with focus).
  /// </remarks>
  public bool TopModal
  { get
    { DesktopControl desktop = Desktop;
      return desktop != null && desktop.modal.Count>0 && desktop.modal[desktop.modal.Count-1]==this;
    }
  }

  /// <summary>Gets or sets whether this control will be rendered.</summary>
  /// <remarks>Setting this property will alter the <see cref="RawVisible"/> property of this control. 
  /// Setting it to true will cause it to inherit the visibility status of its parent. Reading this property will
  /// return whether this control is effectively visible, taking inheritance into account. A control that is not
  /// visible will not be rendered.
  /// </remarks>
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

  /// <summary>Gets or sets the width of this control, in pixels.</summary>
  /// <remarks>Changing this property will resize the control.</remarks>
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

  /// <summary>Gets a rectangle representing the client area of this control.</summary>
  /// <remarks>The client area of the control is the area of the control, relative to the control itself.
  /// The <see cref="Rectangle.X"/> and <see cref="Rectangle.Y"/> of the returned rectangle will both be 0,
  /// and the <see cref="Rectangle.Width"/> and <see cref="Rectangle.Height"/> of the returned rectangle will
  /// be equal to the <see cref="Width"/> and <see cref="Height"/> of this control.
  /// </remarks>
  public Rectangle WindowRect { get { return new Rectangle(0, 0, bounds.Width, bounds.Height); } }

  /// <summary>Gets this control's offset from its parent's bottom edge.</summary>
  /// <remarks>This property is meant to be used by controls handling <see cref="OnLayout"/> and doing custom
  /// layout. It holds the number of pixels that should be between this control's bottom edge and its parent's.
  /// The <see cref="ContainerControl"/> class implements the standard <see cref="OnLayout"/> handler, and it's
  /// recommended that you consider deriving from <see cref="ContainerControl"/> instead of attempting to
  /// implement anchoring and docking yourself.
  /// </remarks>
  public int BottomAnchorOffset { get { return bottomAnchor; } }

  /// <summary>Gets this control's offset from its parent's right edge.</summary>
  /// <remarks>This property is meant to be used by controls handling <see cref="OnLayout"/> and doing custom
  /// layout. It holds the number of pixels that should be between this control's right edge and its parent's.
  /// The <see cref="ContainerControl"/> class implements the standard <see cref="OnLayout"/> handler, and it's
  /// recommended that you consider deriving from <see cref="ContainerControl"/> instead of attempting to
  /// implement anchoring and docking yourself.
  /// </remarks>
  public int RightAnchorOffset  { get { return rightAnchor;  } }
  #endregion
  
  #region Public methods
  /// <summary>Marks a region of the control as invalid, without triggering a repaint event.</summary>
  /// <param name="rect">The rectangle, in control coordinates, to mark as invalid.</param>
  /// <remarks>Generally this method should not be used to mark regions invalid because it does not cause a
  /// repaint event to be triggered. Instead, consider using the <see cref="Invalidate"/> and <see cref="Refresh"/>
  /// methods. This method may be useful in conjunction with the <see cref="Update"/> method, however.
  /// </remarks>
  public void AddInvalidRect(Rectangle rect)
  { rect.Intersect(WindowRect);
    if(rect.Width==0) return;
    if(invalid.Width==0) invalid = rect;
    else invalid = Rectangle.Union(rect, invalid);
  }

  /// <summary>Brings this control to the front of the Z-order.</summary>
  /// <remarks>This method will bring this control to the front, ensuring that it's drawn above its other siblings.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if this control has no parent.</exception>
  public void BringToFront()
  { AssertParent();
    ArrayList list = parent.controls.Array;
    if(list[list.Count-1]!=this)
    { list.Remove(this);
      list.Insert(list.Count-1, this);
      Invalidate();
    }
  }

  /// <summary>Removes input focus from this control.</summary>
  /// <remarks>After calling this method, the control will not have input focus.</remarks>
  public void Blur() { if(parent!=null && parent.FocusedControl==this) parent.FocusedControl=null; }

  /// <summary>Converts a point from display coordinates to control coordinates.</summary>
  /// <param name="displayPoint">The point to convert, in display coordinates.</param>
  /// <returns>The converted point, in control coordinates.</returns>
  /// <remarks>This method converts a point relative to this control's display surface (the <see cref="Surface"/>
  /// property of this control's <see cref="Desktop"/>) into control coordinates. This method does not return a
  /// correct value for controls that use a backing surface (see <see cref="DisplayRect"/> for more information).
  /// </remarks>
  public Point DisplayToWindow(Point displayPoint)
  { Control c = this;
    while(c!=null) { displayPoint.X-=c.bounds.X; displayPoint.Y-=c.bounds.Y; c=c.parent; }
    return displayPoint;
  }

  /// <summary>Converts a rectangle from display coordinates to control coordinates.</summary>
  /// <param name="displayRect">The rectangle to convert, in display coordinates.</param>
  /// <returns>The converted rectangle, in control coordinates.</returns>
  /// <remarks>This method converts a rectangle relative to this control's display surface (the <see cref="Surface"/>
  /// property of this control's <see cref="Desktop"/>) into control coordinates. This method does not return a
  /// correct value for controls that use a backing surface (see <see cref="DisplayRect"/> for more information).
  /// </remarks>
  public Rectangle DisplayToWindow(Rectangle displayRect)
  { return new Rectangle(DisplayToWindow(displayRect.Location), displayRect.Size);
  }

  /// <summary>Selects this control.</summary>
  /// <remarks>Calling this is equivalent to calling <see cref="Focus(bool)"/> and passing true.</remarks>
  public void Focus() { Focus(false); }

  /// <summary>Attempts to give this control input focus.</summary>
  /// <param name="focusAncestors">If true, an attempt will be made to give all the ancestors input focus as well.
  /// </param>
  /// <remarks>This method will select this control (if <see cref="CanFocus"/> is true), and possibly try the same
  /// with all its ancestors, depending on the value of <paramref name="focusAncestors"/>.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if this control has no parent.</exception>
  public void Focus(bool focusAncestors)
  { AssertParent();
    if(CanFocus) parent.FocusedControl = this;
    if(focusAncestors)
    { Control anc = parent;
      while(anc.parent!=null)
      { if(anc.CanFocus) anc.parent.FocusedControl=anc;
        anc=anc.parent;
      }
    }
  }

  /// <summary>Returns the topmost control at a given point in control coordinates.</summary>
  /// <param name="point">The point to consider, in control coordinates.</param>
  /// <returns>The child control at the point specified, or null if none are.</returns>
  /// <remarks>If multiple children overlap at the given point, the one highest in the Z-order will be returned.
  /// </remarks>
  public Control GetChildAtPoint(Point point)
  { for(int i=controls.Count-1; i>=0; i--)
    { Control c = controls[i];
      if(c.visible && c.bounds.Contains(point)) return c;
    }
    return null;
  }

  /// <summary>Returns the next control in the tab order.</summary>
  /// <returns>The next control in the tab order, or null if none were found.</returns>
  /// <remarks>Calling this is equivalent to calling <see cref="GetNextControl(bool)"/> and passing false.</remarks>
  public Control GetNextControl() { return GetNextControl(false); }

  /// <summary>Returns the next or previous control in the tab order.</summary>
  /// <param name="reverse">If true, the previous control in the tab order will be returned.</param>
  /// <returns>The next or previous control in the tab order, or null if none were found.</returns>
  /// <remarks>This method uses the <see cref="TabIndex"/> property to order the controls. This method treats the
  /// set of controls as a circular list, so the control after the last one in the tab order is the first again.
  /// </remarks>
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

  /// <summary>Invalidates the entire control area and triggers a repaint.</summary>
  /// <remarks>Calling this is equivalent to calling <see cref="Invalidate(Rectangle)"/> and passing
  /// <see cref="WindowRect"/>.
  /// </remarks>
  public void Invalidate() { Invalidate(WindowRect); }

  /// <summary>Invalidates an area in control coordinates and triggers a repaint.</summary>
  /// <param name="area">The area to invalidate, in control coordinates.</param>
  /// <remarks>Calling this may also invalidate areas of this control's parent. For instance, if this control has
  /// a transparent background, then its parent will need to be invalidated as well to redraw the background.
  /// This method can be called multiple times in quick succession, and only one paint event will be generated
  /// because if a paint event for this control is already queued, no new one will be added.
  /// </remarks>
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

  /// <summary>Returns true if this control or one of its children is the control specified.</summary>
  /// <param name="control">The control to compare to.</param>
  /// <returns>Returns true if this control or one of its children is the control specified, and false
  /// otherwise.
  /// </returns>
  public bool IsOrHas(Control control) { return this==control || Controls.Contains(control); }

  /// <summary>Forces an immediate repaint.</summary>
  public void Update()
  { DesktopControl desktop = Desktop;
    if(desktop!=null) desktop.DoPaint(this);
  }
  
  /// <summary>Converts a point from control coordinates to an ancestor's control coordinates.</summary>
  /// <param name="windowPoint">The point to convert, in control coordinates.</param>
  /// <param name="ancestor">The ancestor whose control coordinates the point will be converted to.</param>
  /// <returns>The converted point, in the control coordinates of the specified ancestor.</returns>
  /// <remarks>If <paramref name="ancestor"/> is not an ancestor of this control, the results are undefined.</remarks>
  public Point WindowToAncestor(Point windowPoint, Control ancestor)
  { if(ancestor==this) return windowPoint;
    Control c = this;
    do { windowPoint.X+=c.bounds.X; windowPoint.Y+=c.bounds.Y; c=c.parent; } while(c!=ancestor);
    return windowPoint;
  }

  /// <summary>Converts a rectangle from control coordinates to an ancestor's control coordinates.</summary>
  /// <param name="windowRect">The rectangle to convert, in control coordinates.</param>
  /// <param name="ancestor">The ancestor whose control coordinates the rectangle will be converted to.</param>
  /// <returns>The converted rectangle, in the control coordinates of the specified ancestor.</returns>
  /// <remarks>If <paramref name="ancestor"/> is not an ancestor of this control, the results are undefined.</remarks>
  public Rectangle WindowToAncestor(Rectangle windowRect, Control ancestor)
  { return new Rectangle(WindowToAncestor(windowRect.Location, ancestor), windowRect.Size);
  }

  /// <summary>Converts a point from control coordinates to a child's control coordinates.</summary>
  /// <param name="windowPoint">The point to convert, in control coordinates.</param>
  /// <param name="child">The child whose control coordinates the point will be converted to.</param>
  /// <returns>The converted point, in the control coordinates of the specified child.</returns>
  /// <remarks>If <paramref name="child"/> is not a child of this control, the results are undefined.</remarks>
  public Point WindowToChild(Point windowPoint, Control child)
  { windowPoint.X -= child.bounds.X; windowPoint.Y -= child.bounds.Y;
    return windowPoint;
  }

  /// <summary>Converts a rectangle from control coordinates to a child's control coordinates.</summary>
  /// <param name="windowRect">The rectangle to convert, in control coordinates.</param>
  /// <param name="child">The child whose control coordinates the rectangle will be converted to.</param>
  /// <returns>The converted rectangle, in the control coordinates of the specified child.</returns>
  /// <remarks>If <paramref name="child"/> is not a child of this control, the results are undefined.</remarks>
  public Rectangle WindowToChild(Rectangle windowRect, Control child)
  { windowRect.X -= child.bounds.X; windowRect.Y -= child.bounds.Y;
    return windowRect;
  }

  /// <summary>Converts a point from control coordinates to display coordinates.</summary>
  /// <param name="windowPoint">The point to convert, in control coordinates.</param>
  /// <returns>The converted point, in display coordinates.</returns>
  /// <remarks>This method converts a point relative to this control into display coordinates (coordinates relative
  /// to the display surface [the <see cref="Surface"/> property of this control's <see cref="Desktop"/>]). This
  /// method does not return a correct value for controls that use a backing surface
  /// (see <see cref="DisplayRect"/> for more information).
  /// </remarks>
  public Point WindowToDisplay(Point windowPoint) { return WindowToAncestor(windowPoint, null); }

  /// <summary>Converts a rectangle from control coordinates to display coordinates.</summary>
  /// <param name="windowRect">The rectangle to convert, in control coordinates.</param>
  /// <returns>The converted rectangle, in display coordinates.</returns>
  /// <remarks>This method converts a rectangle relative to this control into display coordinates (coordinates
  /// relative to the display surface [the <see cref="Surface"/> property of this control's <see cref="Desktop"/>]).
  /// This method does not return a correct value for controls that use a backing surface
  /// (see <see cref="DisplayRect"/> for more information).
  /// </remarks>
  public Rectangle WindowToDisplay(Rectangle windowRect)
  { return new Rectangle(WindowToAncestor(windowRect.Location, null), windowRect.Size);
  }

  /// <summary>Converts a point from control coordinates to the parent's control coordinates.</summary>
  /// <param name="windowPoint">The point to convert, in control coordinates.</param>
  /// <returns>The converted point, in the control coordinates this control's parent.</returns>
  public Point WindowToParent(Point windowPoint)
  { windowPoint.X+=bounds.X; windowPoint.Y+=bounds.Y; return windowPoint;
  }

  /// <summary>Converts a rectangle from control coordinates to the parent's control coordinates.</summary>
  /// <param name="windowRect">The rectangle to convert, in control coordinates.</param>
  /// <returns>The converted rectangle, in the control coordinates this control's parent.</returns>
  public Rectangle WindowToParent(Rectangle windowRect)
  { return new Rectangle(WindowToParent(windowRect.Location), windowRect.Size);
  }

  /// <summary>Immediately repaints the entire display surface.</summary>
  /// <remarks>Calling this is equivalent to calling <see cref="Refresh(Rectangle)"/> and passing
  /// <see cref="WindowRect"/>.
  /// </remarks>
  public void Refresh() { Refresh(WindowRect); }

  /// <summary>Invalidates the given area and immediately repaints the control.</summary>
  /// <param name="area">The area to invalidate.</param>
  /// <remarks>This method adds the given area to the invalid rectangle (see <see cref="InvalidRect"/>) and
  /// then forces an immediate repaint.
  /// </remarks>
  public void Refresh(Rectangle area)
  { if(back==Color.Transparent && parent!=null) parent.Refresh(WindowToParent(area));
    else
    { AddInvalidRect(area);
      Update();
    }
  }

  /// <summary>Sends this control to the back of the Z-order.</summary>
  /// <remarks>This method will send this control to the back, ensuring that it's drawn below its other siblings.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if this control has no parent.</exception>
  public void SendToBack()
  { AssertParent();
    ArrayList list = parent.controls.Array;
    if(list[0]!=this)
    { list.Remove(this);
      list.Insert(0, this);
      parent.Invalidate(bounds);
    }
  }

  /// <summary>This method selects the next control in the tab order.</summary>
  /// <remarks>Calling this method is equivalent to calling <see cref="TabToNextControl(bool)"/> and passing false.
  /// </remarks>
  public void TabToNextControl() { TabToNextControl(false); }

  /// <summary>This method selects the next or previous control in the tab order.
  /// <seealso cref="Selected"/> <seealso cref="GetNextControl(bool)"/>
  /// </summary>
  /// <param name="reverse">If true, selects the previous control. Otherwise, selects the next control.</param>
  public void TabToNextControl(bool reverse) { FocusedControl = GetNextControl(reverse); }
  #endregion
  
  #region Events
  /// <summary>Occurs when the value of the <see cref="BackColor"/> property changes.</summary>
  public event ValueChangedEventHandler BackColorChanged;
  /// <summary>Occurs when the value of the <see cref="BackImage"/> property changes.</summary>
  public event ValueChangedEventHandler BackImageChanged;
  /// <summary>Occurs when the value of the <see cref="BackImageAlign"/> property changes.</summary>
  public event ValueChangedEventHandler BackImageAlignChanged;
  /// <summary>Occurs when the value of the <see cref="Enabled"/> property changes.</summary>
  public event ValueChangedEventHandler EnabledChanged;
  /// <summary>Occurs when the value of the <see cref="Font"/> property changes.</summary>
  public event ValueChangedEventHandler FontChanged;
  /// <summary>Occurs when the value of the <see cref="ForeColor"/> property changes.</summary>
  public event ValueChangedEventHandler ForeColorChanged;
  /// <summary>Occurs when the value of the <see cref="Location"/> property changes.</summary>
  public event ValueChangedEventHandler LocationChanged;
  /// <summary>Occurs when the value of the <see cref="Parent"/> property changes.</summary>
  public event ValueChangedEventHandler ParentChanged;
  /// <summary>Occurs when the value of the <see cref="Size"/> property changes.</summary>
  public event ValueChangedEventHandler SizeChanged;
  /// <summary>Occurs when the value of the <see cref="TabIndex"/> property changes.</summary>
  public event ValueChangedEventHandler TabIndexChanged;
  /// <summary>Occurs when the value of the <see cref="Text"/> property changes.</summary>
  public event ValueChangedEventHandler TextChanged;
  /// <summary>Occurs when the value of the <see cref="Visible"/> property changes.</summary>
  public event ValueChangedEventHandler VisibleChanged;

  /// <summary>Occurs when the control is selected.</summary>
  /// <remarks>This event only signifies that the control was focused by its parent. The event handler should
  /// consider using the <see cref="Focused"/> property to check that this control has actual input focus.
  /// </remarks>
  public event EventHandler GotFocus;

  /// <summary>Occurs when the control is unselected.</summary>
  /// <remarks>This event only signifies that the control was unfocused by its parent. The control may not have
  /// had actual input focus before this event was raised.
  /// </remarks>
  public event EventHandler LostFocus;

  /// <summary>Occurs when the control is to lay out its children.</summary>
  /// <remarks>This event should be raised before the actual layout code executes, so the event handler can
  /// make modifications to control positions and expect the changes to be taken into account.
  /// </remarks>
  public event EventHandler Layout;

  /// <summary>Occurs when the mouse is positioned over the control or one of its ancestors.</summary>
  /// <remarks>If at any level in the control hierarchy, there are multiple overlapping sibling controls under the
  /// cursor, the one with the highest Z-order will be given precedence.  This event will not be raised for
  /// controls that are not visible.
  /// </remarks>
  public event EventHandler MouseEnter;

  /// <summary>Occurs when the mouse is leaves the control's area.</summary>
  /// <remarks>If the <see cref="MouseEnter"/> event was not raised for this control, the <see cref="MouseLeave"/>
  /// event will not be raised either. This event will not be raised for controls that are not visible.
  /// </remarks>
  public event EventHandler MouseLeave;

  /// <summary>Occurs after the value of the <see cref="Location"/> property changes.</summary>
  /// <remarks>This event occurs after the <see cref="LocationChanged"/> event. Generally, it's better to hook this
  /// event rather han the <see cref="LocationChanged"/> event.
  /// </remarks>
  public event EventHandler Move;

  /// <summary>Occurs after the value of the <see cref="Size"/> property changes.</summary>
  /// <remarks>This event occurs after the <see cref="SizeChanged"/> event. Generally, it's better to hook this
  /// event rather han the <see cref="SizeChanged"/> event.
  /// </remarks>
  public event EventHandler Resize;

  /// <summary>Occurs after a child is added to this control.</summary>
  public event ControlEventHandler ControlAdded;
  /// <summary>Occurs after a child is removed from this control.</summary>
  public event ControlEventHandler ControlRemoved;

  /// <summary>Occurs when a keyboard key is pressed and this control has input focus.</summary>
  public event KeyEventHandler KeyDown;
  /// <summary>Occurs when a keyboard key is released and this control has input focus.</summary>
  public event KeyEventHandler KeyUp;

  /// <summary>Occurs when a key having an associated character is pressed and this control has input focus.</summary>
  /// <remarks>Some keys do not have associated characters, such as the shift keys, the arrow keys, etc.
  /// Some key presses depend on the state of other keys. For instance, the character associated with the A key
  /// depends on the state of the Caps Lock state and modifier keys such as Shift and Ctrl.
  /// </remarks>
  public event KeyEventHandler KeyPress;

  /// <summary>Occurs when the mouse is moved over a control's surface.</summary>
  /// <remarks>If another control has captured mouse input or is the topmost modal contral, this event will only
  /// be raised for that control.
  /// </remarks>
  public event MouseMoveEventHandler MouseMove;

  /// <summary>Occurs when the mouse button is pressed inside a control's area.</summary>
  /// <remarks>If another control has captured mouse input or is the topmost modal contral, this event will only
  /// be raised for that control. The control must have the <see cref="ControlStyle.Clickable"/> style to receive
  /// this event.
  /// </remarks>
  public event ClickEventHandler MouseDown;

  /// <summary>Occurs when the mouse button is released inside a control's area.</summary>
  /// <remarks>If another control has captured mouse input or is the topmost modal contral, this event will only
  /// be raised for that control. The control must have the <see cref="ControlStyle.Clickable"/> style to receive
  /// this event.
  /// </remarks>
  public event ClickEventHandler MouseUp;

  /// <summary>Occurs when the mouse button is both pressed and released inside a control's area.</summary>
  /// <remarks>If another control has captured mouse input or is the topmost modal contral, this event will only
  /// be raised for that control. The control must have the <see cref="ControlStyle.Clickable"/> style to receive
  /// this event.
  /// </remarks>
  public event ClickEventHandler MouseClick;

  /// <summary>Occurs when the mouse button is double-clicked inside a control's area.</summary>
  /// <remarks>If another control has captured mouse input or is the topmost modal contral, this event will only
  /// be raised for that control. The control must have the <see cref="ControlStyle.DoubleClickable"/> style to
  /// receive this event.
  /// <seealso cref="DesktopControl.DoubleClickDelay"/>
  /// </remarks>
  public event ClickEventHandler DoubleClick;

  /// <summary>Occurs when the mouse is clicked and dragged inside the control's area.</summary>
  /// <remarks>If another control has captured mouse input or is the topmost modal contral, this event will only
  /// be raised for that control. The control must have the <see cref="ControlStyle.Draggable"/> style to
  /// receive this event.
  /// <see cref="Control.DragThreshold"/> <see cref="DesktopControl.DragThreshold"/>
  /// </remarks>
  public event DragEventHandler DragStart;

  /// <summary>Occurs when the mouse is moved after a drag has started. <seealso cref="DragStart"/></summary>
  public event DragEventHandler DragMove;
  /// <summary>Occurs when the mouse button is released, ending a drag. <seealso cref="DragStart"/></summary>
  public event DragEventHandler DragEnd;

  /// <summary>Occurs immediately before the background of a control is to be repainted.</summary>
  public event PaintEventHandler PaintBackground;
  /// <summary>Occurs immediately before a control is to be repainted.</summary>
  public event PaintEventHandler Paint;

  /// <summary>Raises the <see cref="BackColorChanged"/> event and performs default handing.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs"/> that contains the event data.</param>
  /// <remarks>This method raises the <see cref="BackColorChanged"/> event, invalidates the control (using
  /// <see cref="Invalidate"/>), and calls <see cref="OnParentBackColorChanged"/> on each child.
  /// When overriding this method in a derived class, be sure to call the base class' version to ensure that the
  /// default processing gets performed.
  /// </remarks>
  protected virtual void OnBackColorChanged(ValueChangedEventArgs e)
  { if(BackColorChanged!=null) BackColorChanged(this, e);
    Invalidate();
    foreach(Control c in controls) c.OnParentBackColorChanged(e);
  }

  /// <summary>Raises the <see cref="BackImageChanged"/> event and performs default handing.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs"/> that contains the event data.</param>
  /// <remarks>This method raises the <see cref="BackImageChanged"/> event and invalidates the control (using
  /// <see cref="Invalidate"/>).
  /// When overriding this method in a derived class, be sure to call the base class' version to ensure that the
  /// default processing gets performed.
  /// </remarks>
  protected virtual void OnBackImageChanged(ValueChangedEventArgs e)
  { if(BackImageChanged!=null) BackImageChanged(this, e);
    Invalidate();
  }

  /// <summary>Raises the <see cref="BackImageAlignChanged"/> event and performs default handing.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs"/> that contains the event data.</param>
  /// <remarks>This method raises the <see cref="BackImageAlignChanged"/> event and invalidates the control (using
  /// <see cref="Invalidate"/>) if a background image is defined.
  /// When overriding this method in a derived class, be sure to call the base class' version to ensure that the
  /// default processing gets performed.
  /// </remarks>
  protected virtual void OnBackImageAlignChanged(ValueChangedEventArgs e)
  { if(BackImageAlignChanged!=null) BackImageAlignChanged(this, e);
    if(backImage!=null) Invalidate();
  }

  /// <summary>Raises the <see cref="EnabledChanged"/> event and performs default handing.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs"/> that contains the event data.</param>
  /// <remarks>This method raises the <see cref="EnabledChanged"/> event, and invalidates and possibly blurs the
  /// control, and calls <see cref="OnParentEnabledChanged"/> on each child.
  /// When overriding this method in a derived class, be sure to call the base class' version to ensure that the
  /// default processing gets performed. The proper place to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnEnabledChanged(ValueChangedEventArgs e)
  { if(EnabledChanged!=null) EnabledChanged(this, e);
    if(Enabled != (bool)e.OldValue)
    { if(!Enabled) Blur();
      Invalidate();
      foreach(Control c in controls) c.OnParentEnabledChanged(e);
    }
  }

  /// <summary>Raises the <see cref="FontChanged"/> event and performs default handing.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs"/> that contains the event data.</param>
  /// <remarks>This method raises the <see cref="FontChanged"/> event, and invalidates the control (using
  /// <see cref="Invalidate"/>), and calls <see cref="OnParentFontChanged"/> on each child.
  /// When overriding this method in a derived class, be sure to call the base class' version to ensure that the
  /// default processing gets performed.
  /// </remarks>
  protected virtual void OnFontChanged(ValueChangedEventArgs e)
  { if(FontChanged!=null) FontChanged(this, e);
    Invalidate();
    foreach(Control c in controls) c.OnParentFontChanged(e);
  }

  /// <summary>Raises the <see cref="ForeColorChanged"/> event and performs default handing.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs"/> that contains the event data.</param>
  /// <remarks>This method raises the <see cref="ForeColorChanged"/> event, and invalidates the control (using
  /// <see cref="Invalidate"/>), and calls <see cref="OnParentForeColorChanged"/> on each child.
  /// When overriding this method in a derived class, be sure to call the base class' version to ensure that the
  /// default processing gets performed.
  /// </remarks>
  protected virtual void OnForeColorChanged(ValueChangedEventArgs e)
  { if(ForeColorChanged!=null) ForeColorChanged(this, e);
    Invalidate();
    foreach(Control c in controls) c.OnParentForeColorChanged(e);
  }

  /// <summary>Raises the <see cref="LocationChanged"/> event and performs default handing.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs"/> that contains the event data.</param>
  /// <remarks>This method raises the <see cref="LocationChanged"/> event, and invalidates the control and
  /// its parent (using <see cref="Invalidate"/>), updates anchor information, and calls <see cref="OnMove"/>.
  /// When overriding this method in a derived class, be sure to call the base class' version to ensure that the
  /// default processing gets performed. The proper place to do this is at the end of the derived version.
  /// </remarks>
  protected virtual void OnLocationChanged(ValueChangedEventArgs e)
  { if(LocationChanged!=null) LocationChanged(this, e);
    if(parent!=null)
    { parent.Invalidate(new Rectangle((Point)e.OldValue, bounds.Size));
      Invalidate();
      UpdateAnchor();
    }
    OnMove(e);
  }

  /// <summary>Raises the <see cref="ParentChanged"/> event and performs default handing.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs"/> that contains the event data.</param>
  /// <remarks>This method raises the <see cref="ParentChanged"/> event, and performs important processing.
  /// When overriding this method in a derived class, be sure to call the base class' version to ensure that the
  /// default processing gets performed. The proper place for this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnParentChanged(ValueChangedEventArgs e)
  { if(ParentChanged!=null) ParentChanged(this, e);
    UpdateBackingSurface(false);

    if(parent!=null)
    { UpdateAnchor();
      Invalidate();
      parent.TriggerLayout();
    }

    if(e.OldValue==null)
    { if(back!=BackColor) OnBackColorChanged(new ValueChangedEventArgs(back));
      if(enabled!=Enabled) OnEnabledChanged(new ValueChangedEventArgs(enabled));
      if(font!=Font) OnFontChanged(new ValueChangedEventArgs(font));
      if(fore!=ForeColor) OnForeColorChanged(new ValueChangedEventArgs(fore));
      if(visible!=Visible) OnVisibleChanged(new ValueChangedEventArgs(visible));
    }
    else
    { Control oldPar = (Control)e.OldValue;

      if(back==Color.Transparent)
      { Color old = oldPar.BackColor; 
        if(old!=back) OnBackColorChanged(new ValueChangedEventArgs(old));
      }
      if(enabled && !oldPar.Enabled) OnEnabledChanged(new ValueChangedEventArgs(false));
      if(font==null)
      { GameLib.Fonts.Font old = oldPar.Font;
        if(old!=font) OnFontChanged(new ValueChangedEventArgs(old));
      }
      if(fore==Color.Transparent)
      { Color old = oldPar.ForeColor; 
        if(old!=fore) OnBackColorChanged(new ValueChangedEventArgs(old));
      }
      if(visible && !oldPar.Visible) OnEnabledChanged(new ValueChangedEventArgs(false));
      
      ((Control)e.OldValue).TriggerLayout();
    }
  }

  /// <summary>Raises the <see cref="SizeChanged"/> event and performs default handing.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs"/> that contains the event data.</param>
  /// <remarks>This method raises the <see cref="SizeChanged"/> event, performs important processing, and calls
  /// <see cref="OnResize"/>. When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed. The proper place to do this is at the end
  /// of the derived version.
  /// </remarks>
  protected virtual void OnSizeChanged(ValueChangedEventArgs e)
  { if(SizeChanged!=null) SizeChanged(this, e);
    if(invalid.Right>bounds.Width) invalid.Width-=invalid.Right-bounds.Width;
    if(invalid.Bottom>bounds.Height) invalid.Height-=invalid.Bottom-bounds.Height;
    if(!mychange) TriggerLayout(); // 'mychange' is true when docking
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

  /// <summary>Raises the <see cref="TabIndexChanged"/> event.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected virtual void OnTabIndexChanged(ValueChangedEventArgs e)
  { if(TabIndexChanged!=null) TabIndexChanged(this, e);
  }

  /// <summary>Raises the <see cref="TextChanged"/> event.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected virtual void OnTextChanged(ValueChangedEventArgs e) { if(TextChanged!=null) TextChanged(this, e); }

  /// <summary>Raises the <see cref="VisibleChanged"/> event and performs default handing.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs"/> that contains the event data.</param>
  /// <remarks>This method raises the <see cref="VisibleChanged"/> event, invalidates and possibly blurs the
  /// control, and calls <see cref="OnParentVisibleChanged"/> on each child.
  /// When overriding this method in a derived class, be sure to call the base class' version to ensure that the
  /// default processing gets performed. The proper place to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnVisibleChanged(ValueChangedEventArgs e)
  { if(VisibleChanged!=null) VisibleChanged(this, e);
    if(Visible!=(bool)e.OldValue)
    { if(!Visible) Blur();
      foreach(Control c in controls) c.OnParentVisibleChanged(e);
      Invalidate();
    }
  }

  /// <summary>Raises the <see cref="GotFocus"/> event.</summary>
  /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected virtual void OnGotFocus(EventArgs e)  { if(GotFocus!=null) GotFocus(this, e); }

  /// <summary>Raises the <see cref="LostFocus"/> event.</summary>
  /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected virtual void OnLostFocus(EventArgs e) { if(LostFocus!=null) LostFocus(this, e); }

  /// <summary>Raises the <see cref="Layout"/> event and performs default handling.</summary>
  /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed. The proper place to do this is at
  /// the beginning of the derived version.
  /// </remarks>
  protected internal virtual void OnLayout(EventArgs e)
  { if(Layout!=null) Layout(this, e);
    pendingLayout=false;
  }

  /// <summary>Raises the <see cref="MouseEnter"/> event.</summary>
  /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnMouseEnter(EventArgs e) { if(MouseEnter!=null) MouseEnter(this, e); }

  /// <summary>Raises the <see cref="MouseLeave"/> event.</summary>
  /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnMouseLeave(EventArgs e) { if(MouseLeave!=null) MouseLeave(this, e); }

  /// <summary>Raises the <see cref="Move"/> event.</summary>
  /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected virtual void OnMove(EventArgs e)   { if(Move!=null) Move(this, e); }

  /// <summary>Raises the <see cref="Resize"/> event.</summary>
  /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected virtual void OnResize(EventArgs e) { if(Resize!=null) Resize(this, e); }

  /// <summary>Raises the <see cref="ControlAdded"/> event and performs default handling.</summary>
  /// <param name="e">A <see cref="ControlEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed. The proper place to do this is at the
  /// end of the derived version.
  /// </remarks>
  protected virtual void OnControlAdded(ControlEventArgs e)
  { if(e.Control.Dock != DockStyle.None) TriggerLayout();
    if(ControlAdded!=null) ControlAdded(this, e);
  }

  /// <summary>Raises the <see cref="ControlRemoved"/> event and performs default handling.</summary>
  /// <param name="e">A <see cref="ControlEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed. The proper place to do this is at the end
  /// of the derived version.
  /// </remarks>
  protected virtual void OnControlRemoved(ControlEventArgs e)
  { if(focused==e.Control) { focused.OnLostFocus(e); focused=null; }
    if(e.Control.Dock != DockStyle.None) TriggerLayout();
    if(e.Control.Visible) Invalidate(e.Control.Bounds);
    if(ControlRemoved!=null) ControlRemoved(this, e);
  }

  /// <summary>Raises the <see cref="KeyDown"/> event.</summary>
  /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnKeyDown(KeyEventArgs e) { if(KeyDown!=null) KeyDown(this, e); }

  /// <summary>Raises the <see cref="KeyUp"/> event.</summary>
  /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnKeyUp(KeyEventArgs e) { if(KeyUp!=null) KeyUp(this, e); }

  /// <summary>Raises the <see cref="KeyPress"/> event.</summary>
  /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnKeyPress(KeyEventArgs e) { if(KeyPress!=null) KeyPress(this, e); }

  /// <summary>Raises the <see cref="MouseMove"/> event.</summary>
  /// <param name="e">A <see cref="MouseMoveEvent"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnMouseMove(MouseMoveEvent e) { if(MouseMove!=null) MouseMove(this, e); }

  /// <summary>Raises the <see cref="MouseDown"/> event.</summary>
  /// <param name="e">A <see cref="ClickEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnMouseDown(ClickEventArgs e) { if(MouseDown!=null) MouseDown(this, e); }

  /// <summary>Raises the <see cref="MouseUp"/> event.</summary>
  /// <param name="e">A <see cref="ClickEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnMouseUp(ClickEventArgs e) { if(MouseUp!=null) MouseUp(this, e); }

  /// <summary>Raises the <see cref="MouseClick"/> event.</summary>
  /// <param name="e">A <see cref="ClickEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnMouseClick(ClickEventArgs e) { if(MouseClick!=null) MouseClick(this, e); }

  /// <summary>Raises the <see cref="DoubleClick"/> event.</summary>
  /// <param name="e">A <see cref="ClickEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnDoubleClick(ClickEventArgs e) { if(DoubleClick!=null) DoubleClick(this, e); }

  /// <summary>Raises the <see cref="DragStart"/> event.</summary>
  /// <param name="e">A <see cref="DragEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnDragStart(DragEventArgs e) { if(DragStart!=null) DragStart(this, e); }

  /// <summary>Raises the <see cref="DragMove"/> event.</summary>
  /// <param name="e">A <see cref="DragEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnDragMove(DragEventArgs e) { if(DragMove!=null) DragMove(this, e); }

  /// <summary>Raises the <see cref="DragEnd"/> event.</summary>
  /// <param name="e">A <see cref="DragEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnDragEnd(DragEventArgs e) { if(DragEnd!=null) DragEnd(this, e); }

  /// <summary>Raises the <see cref="PaintBackground"/> event and performs default painting.</summary>
  /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed. The proper place to do this is at the start
  /// of the derived version.
  /// </remarks>
  protected internal virtual void OnPaintBackground(PaintEventArgs e)
  { if(PaintBackground!=null) PaintBackground(this, e);
    if(back!=Color.Transparent) e.Surface.Fill(e.DisplayRect, back);
    if(backImage!=null)
    { Point at = Helpers.CalculateAlignment(DisplayRect, new Size(backImage.Width, backImage.Height), backImageAlign);
      backImage.Blit(e.Surface, at.X, at.Y);
    }
  }

  /// <summary>Raises the <see cref="Paint"/> event and performs default painting.</summary>
  /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed. The proper place to do this is at the start
  /// of the derived version.
  /// </remarks>
  protected internal virtual void OnPaint(PaintEventArgs e)
  { if(Paint!=null) Paint(this, e);
    invalid.Width = 0;
    pendingPaint  = false;
  }
  
  /// <summary>Called when the parent's <see cref="BackColor"/> property changes.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs"/> that contains the event data.</param>
  /// <remarks>This method will call <see cref="OnBackColorChanged"/> if necessary.
  /// When overriding this method in a derived class, be sure to call the base class' version to ensure that the
  /// default processing gets performed.
  /// </remarks>
  protected virtual void OnParentBackColorChanged(ValueChangedEventArgs e)
  { if(back==Color.Transparent) OnBackColorChanged(e);
  }

  /// <summary>Called when the parent's <see cref="Enabled"/> property changes.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs"/> that contains the event data.</param>
  /// <remarks>This method will call <see cref="OnEnabledChanged"/> if necessary.
  /// When overriding this method in a derived class, be sure to call the base class' version to ensure that the
  /// default processing gets performed.
  /// </remarks>
  protected virtual void OnParentEnabledChanged(ValueChangedEventArgs e) { if(enabled) OnEnabledChanged(e); }

  /// <summary>Called when the parent's <see cref="Font"/> property changes.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs"/> that contains the event data.</param>
  /// <remarks>This method will call <see cref="OnFontChanged"/> if necessary.
  /// When overriding this method in a derived class, be sure to call the base class' version to ensure that the
  /// default processing gets performed.
  /// </remarks>
  protected virtual void OnParentFontChanged(ValueChangedEventArgs e) { if(font==null) OnFontChanged(e); }

  /// <summary>Called when the parent's <see cref="ForeColor"/> property changes.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs"/> that contains the event data.</param>
  /// <remarks>This method will call <see cref="OnForeColorChanged"/> if necessary.
  /// When overriding this method in a derived class, be sure to call the base class' version to ensure that the
  /// default processing gets performed.
  /// </remarks>
  protected virtual void OnParentForeColorChanged(ValueChangedEventArgs e)
  { if(fore==Color.Transparent) OnForeColorChanged(e);
  }

  /// <summary>Called when the parent's <see cref="Size"/> property changes.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class' version to ensure
  /// that the default processing gets performed.
  /// </remarks>
  protected virtual void OnParentResized(EventArgs e) { }

  /// <summary>Called when the parent's <see cref="Visible"/> property changes.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs"/> that contains the event data.</param>
  /// <remarks>This method will call <see cref="OnVisibleChanged"/> if necessary.
  /// When overriding this method in a derived class, be sure to call the base class' version to ensure that the
  /// default processing gets performed.
  /// </remarks>
  protected virtual void OnParentVisibleChanged(ValueChangedEventArgs e) { if(visible) OnVisibleChanged(e); }
  
  /// <summary>Called when the control receives a custom window event.</summary>
  /// <param name="e">A <see cref="WindowEvent"/> that contains the event.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class' version to
  /// ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnCustomEvent(WindowEvent e) { }
  #endregion
  
  /// <summary>Gets or sets this control's focused control.</summary>
  /// <remarks>Changing this property will call <see cref="OnLostFocus"/> and <see cref="OnGotFocus"/> to raise
  /// the appropriate events.
  /// </remarks>
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

  /// <summary>Tests whether the control has a certain <see cref="ControlStyle"/> flag.</summary>
  /// <param name="test">The <see cref="ControlStyle"/> flag to test for.</param>
  /// <returns>Returns true if the <see cref="Style"/> property contains the specified flag.</returns>
  protected internal bool HasStyle(ControlStyle test) { return (style & test) != ControlStyle.None; }

  /// <summary>Gets or sets the drag threshold for this control.</summary>
  /// <remarks>The drag threshold controls how far the mouse has to be dragged for it to register as a drag event.
  /// The value is stored as the distance in pixels, squared, so if a movement of 4 pixels is required to signify
  /// a drag, this property should be set to 16, which is 4 squared. As a special case, setting it to -1 causes it
  /// to inherit the desktop's <see cref="DesktopControl.DragThreshold"/> property value. Reading this property does
  /// not take inheritance into account and may return -1. The default value is -1.
  /// </remarks>
  /// <value>The drag threshold, as the distance squared, in pixels, or -1 to inherit the desktop's
  /// <see cref="DesktopControl.DragThreshold"/> property.
  /// </value>
  protected internal int DragThreshold
  { get { return dragThreshold; }
    set
    { if(value<-1) throw new ArgumentOutOfRangeException("DragThreshold", value, "must be >=0 or -1");
      dragThreshold=value;
    }
  }

  internal void SetParent(Control control)
  { ValueChangedEventArgs ve = new ValueChangedEventArgs(parent);
    ControlEventArgs ce = new ControlEventArgs(this);
    if(parent!=null)
    { DesktopControl desktop = parent is DesktopControl ? (DesktopControl)parent : parent.Desktop;
      if(desktop!=null)
      { if(desktop.capturing!=null && ce.Control.IsOrHas(desktop.capturing)) desktop.capturing=null;
        desktop.UnsetModal(this);
      }
      parent.OnControlRemoved(ce);
    }
    parent = control;
    if(parent!=null) parent.OnControlAdded(ce);
    if(!mychange) OnParentChanged(ve);
  }

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

  /// <summary>Gets the <see cref="Surface"/> used as the backing surface for this control.</summary>
  /// <remarks>This property will return null if there is no backing surface being used.</remarks>
  protected Surface BackingSurface { get { return backingSurface; } }
  
  /// <summary>Throws an exception if the control has no parent.</summary>
  /// <remarks>This method will raise an <see cref="InvalidOperationException"/> if the control has no parent.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if the control has no parent.</exception>
  protected void AssertParent()
  { if(parent==null) throw new InvalidOperationException("This control has no parent");
  }

  /// <summary>Changes the control's modal status.</summary>
  /// <param name="modal">Makes the control modal if true, and nonmodal if false.</param>
  /// <remarks>After calling this with <paramref name="modal"/> set to true, the control will be the topmost modal
  /// control, even if it was previously modal.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if the control has no associated desktop.</exception>
  protected void SetModal(bool modal)
  { DesktopControl desktop = Desktop;
    if(desktop==null) throw new InvalidOperationException("This control has no desktop");
    if(modal) desktop.SetModal(this);
    else desktop.UnsetModal(this);
  }

  /// <summary>Triggers a relayout of this control's children.</summary>
  /// <remarks>Calling this method pushes a new <see cref="WindowLayoutEvent"/> onto the event queue for this
  /// control, if one is not already there.
  /// </remarks>
  protected void TriggerLayout()
  { if(!pendingLayout && Events.Events.Initialized)
    { Events.Events.PushEvent(new WindowLayoutEvent(this));
      pendingLayout=true;
    }
  }

  /// <summary>Updates information needed to anchor this control.</summary>
  void UpdateAnchor()
  { if((anchor&AnchorStyle.Right) != 0) rightAnchor=parent.Width-bounds.Right;
    if((anchor&AnchorStyle.Bottom) != 0) bottomAnchor=parent.Height-bounds.Bottom;
  }
  
  ControlCollection controls;
  Control parent, focused;
  GameLib.Fonts.Font font;
  Color back=Color.Transparent, fore=Color.Transparent;
  IBlittable backImage, cursor;
  string name=string.Empty, text=string.Empty;
  object tag;
  Rectangle bounds = new Rectangle(0, 0, 100, 100), invalid;
  int tabIndex=-1, bottomAnchor, rightAnchor, dragThreshold=-1;
  ControlStyle style;
  AnchorStyle  anchor=AnchorStyle.None;
  DockStyle    dock;
  ContentAlignment backImageAlign=ContentAlignment.TopLeft;
  bool enabled=true, visible=true, mychange, keyPreview, pendingPaint, pendingLayout;
}
#endregion

#region DesktopControl class
/// <summary>
/// This enum is used with the <see cref="DesktopControl.AutoFocusing"/> property to determine how the
/// desktop will automatically focus controls.
/// </summary>
public enum AutoFocus
{ 
  /// <summary>The desktop will not autofocus controls. The code will have to call <see cref="Control.Focus"/> and
  /// <see cref="Control.Blur"/> manually in order to alter the focus.
  /// </summary>
  None=0,
  /// <summary>When a focusable control is clicked with any button, the desktop will focus it and all its ancestors.
  /// </summary>
  Click=1,
  /// <summary>When the mouse moves over a focusable control, it and its ancestors will be focused. When the
  /// mouse moves off of a control, it will be blurred, even if the mouse did not move onto another focusable
  /// control.
  /// </summary>
  Over=2,
  /// <summary>When the mouse moves over a focusable control, it and its ancestors will be focused.</summary>
  OverSticky=3
}

public class DesktopControl : ContainerControl, IDisposable
{ public DesktopControl() { Init(); }
  public DesktopControl(Surface surface) { Init(); Surface = surface; }
  ~DesktopControl() { Dispose(true); }
  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  #region Properties
  /// <summary>Gets or sets the auto focusing mode for this desktop.</summary>
  /// <remarks>See the <see cref="AutoFocus"/> enum for information on the types of auto focusing available.
  /// The default is <see cref="AutoFocus.None"/>.
  /// </remarks>
  public AutoFocus AutoFocusing { get { return focus; } set { focus=value; } }

  /// <summary>Gets or sets the double click delay in milliseconds for this desktop.</summary>
  /// <remarks>The double click delay is the maximum number of milliseconds allowed between mouse clicks for them
  /// to be recognized as a double click. The default value is 350 milliseconds.
  /// </remarks>
  public uint DoubleClickDelay  { get { return dcDelay; } set { dcDelay=value; } }

  /// <summary>Gets or sets the default drag threshold for this desktop.</summary>
  /// <remarks>This property provides a default drag threshold for controls that do not specify one. See
  /// <see cref="Control.DragThreshold"/> for more information about the drag threshold. The default value is 16.
  /// </remarks>
  public new int DragThreshold
  { get { return base.DragThreshold; }
    set
    { if(value<1) throw new ArgumentOutOfRangeException("DragThreshold", "must be >=1");
      base.DragThreshold = value;
    }
  }

  /// <summary>Get or sets the delay in milliseconds before a key begins repeating.</summary>
  /// <remarks>This property holds the number of milliseconds a key must remain depressed before it will begin
  /// repeating. You can set this to 0 to disable the key repeat. The default is 0.
  /// This property is incompatible with the <see cref="Input.Keyboard.EnableKeyRepeat"/> method, so
  /// enabling this property will call <see cref="Input.Keyboard.DisableKeyRepeat"/> first.
  /// </remarks>
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
      { Input.Keyboard.DisableKeyRepeat();
        krTimer = new System.Threading.Timer(new System.Threading.TimerCallback(RepeatKey), null,
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

  /// <summary>Gets the topmost modal control, or null if there are none.</summary>
  public Control ModalWindow { get { return modal.Count==0 ? null : (Control)modal[modal.Count-1]; } }

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
  public bool ProcessEvent(Event e)
  { if(Input.Input.ProcessEvent(e))
    {
      #region Mouse moves
      if(moves && e is MouseMoveEvent)
      { MouseMoveEvent ea = (MouseMoveEvent)e;
        Point at = ea.Point;
        // if the cursor is not within the desktop area, ignore it (unless dragging or capturing)
        if(dragging==null && capturing==null && !Bounds.Contains(at)) return false;

        Control p=this, c;
        // passModal is true if there's no modal window, or this movement is within the modal window
        bool passModal = modal.Count==0;
        at.X -= Bounds.X; at.Y -= Bounds.Y; // at is the cursor point local to 'p'
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
            if(xd*xd+yd*yd >= (p.DragThreshold==-1 ? DragThreshold : p.DragThreshold))
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
        return true;
      }
      #endregion
      #region Keyboard
      else if(keys && e is KeyboardEvent)
      { if(FocusedControl!=null || KeyPreview)
        { KeyEventArgs ea = new KeyEventArgs((KeyboardEvent)e);
          ea.KE.Mods = Input.Keyboard.Mods;
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
          return true;
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
        if(capturing==null && !dragStarted && !Bounds.Contains(at)) return false;
        Control p = this, c;
        uint time = Timing.Msecs;
        bool passModal = modal.Count==0;
        
        at.X -= Bounds.X; at.Y -= Bounds.Y; // at is the cursor point local to 'p'
        while(p.Enabled && p.Visible)
        { c = p.GetChildAtPoint(at);
          if(c==null) break;
          if(!passModal && c==modal[modal.Count-1]) passModal=true;
          at = p.WindowToChild(at, c);
          if(focus==AutoFocus.Click && ea.CE.Down && c.CanFocus && passModal && !ea.CE.MouseWheel) c.Focus();
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
          } while(p!=null && p!=this && p.Enabled && p.Visible);
        }
        done:
        // lastClicked is used to track if the button release occurred over the same control it was pressed over
        // this allows you to press the mouse on a control, then drag off and release to avoid the MouseClick event
        if(!ea.CE.Down && (byte)ea.CE.Button<8) lastClicked[(byte)ea.CE.Button] = null;
        return true;
      }
      #endregion
    }
    #region WindowEvent
    else if(e is WindowEvent)
    { WindowEvent we = (WindowEvent)e;
      DesktopControl desktop = we.Control.Desktop;
      if(we.SubType==WindowEvent.MessageType.Custom && (desktop==this || desktop==null))
      { we.Control.OnCustomEvent(we); return true;
      }
      if(desktop!=this) return false;

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
        case WindowEvent.MessageType.DesktopUpdated: if(we.Control!=this) return false; break;
      }
      return true;
    }
    #endregion
    return false;
  }
  #endregion
  
  public void StopKeyRepeat()
  { if(krTimer!=null)
    { krTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
      heldKey = null;
    }
  }

  protected void Dispose(bool destructing)
  { if(init)
    { Video.Video.ModeChanged -= modeChanged;
      modeChanged = null;
      Input.Input.Deinitialize();
      Events.Events.Deinitialize();
      init = false;
    }
    if(krTimer!=null)
    { krTimer.Dispose();
      krTimer=null;
    }
  }

  protected override void OnParentChanged(ValueChangedEventArgs e)
  { if(Parent!=null)
    { Parent=null;
      throw new InvalidOperationException("A desktop cannot be the child of another control!");
    }
    base.OnParentChanged(e);
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
    if(!e.CE.MouseWheel && (byte)e.CE.Button<8)
    { if(target.HasStyle(ControlStyle.NormalClick) && (clickStatus&ClickStatus.Click)!=0)
      { if(e.CE.Down) lastClicked[(byte)e.CE.Button] = target;
        else
        { if(lastClicked[(byte)e.CE.Button]==target)
          { if(target.HasStyle(ControlStyle.DoubleClickable) && time-target.lastClickTime<=dcDelay)
              target.OnDoubleClick(e);
            else target.OnMouseClick(e);
            target.lastClickTime = time;
            if(e.Handled) { clickStatus ^= ClickStatus.Click; e.Handled=false; }
            lastClicked[(byte)e.CE.Button]=target.Parent; // allow the check to be done for the parent, too // TODO: make sure this is okay with captured/dragged controls
          }
        }
      }
      else clickStatus ^= ClickStatus.Click;
    }
    if(!e.CE.Down && (clickStatus&ClickStatus.UpDown) != 0)
    { if(e.CE.MouseWheel) e.Handled=true;
      else target.OnMouseUp(e);
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
    Input.Input.Initialize(false);
    Input.Keyboard.DisableKeyRepeat();
    init = true;
    drag = new DragEventArgs();
    modeChanged = new ModeChangedHandler(UpdateBackingSurfaces);
    Video.Video.ModeChanged += modeChanged;
  }

  void RepeatKey(object dummy) { if(!keyProcessing) Events.Events.PushEvent(new KeyRepeatEvent(this)); }

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
  int   enteredLen, updatedLen;
  uint  dcDelay=350, krDelay, krRate=40;
  bool  keys=true, clicks=true, moves=true, init, dragStarted, trackUpdates=true, keyProcessing;
}
#endregion

#region Helpers
public enum BorderStyle { None, FixedFlat, Fixed3D, FixedThick, Resizeable };

public class Helpers
{ private Helpers() { }

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
  
  public static void DrawBorder(Surface surface, Rectangle rect, BorderStyle border, bool depressed)
  { switch(border)
    { case BorderStyle.FixedFlat: DrawBorder(surface, rect, border, Color.Black, depressed); break;
      case BorderStyle.Fixed3D: case BorderStyle.FixedThick: case BorderStyle.Resizeable:
        DrawBorder(surface, rect, border, Color.FromArgb(212, 208, 200), depressed);
        break;
    }
  }

  public static void DrawBorder(Surface surface, Rectangle rect, BorderStyle border, Color color, bool depressed)
  { switch(border)
    { case BorderStyle.FixedFlat: DrawBorder(surface, rect, border, color, color, depressed); break;
      case BorderStyle.Fixed3D: case BorderStyle.FixedThick: case BorderStyle.Resizeable:
        DrawBorder(surface, rect, border,
                   Color.FromArgb(color.R+(255-color.R)/2, color.G+(255-color.G)/2, color.B+(255-color.B)/2),
                   Color.FromArgb(color.R*2/3, color.G*2/3, color.B*2/3), depressed);
        break;
    }
  }

  public static void DrawBorder(Surface surface, Rectangle rect, BorderStyle border, Color c1, Color c2, bool depressed)
  { switch(border)
    { case BorderStyle.FixedFlat: Primitives.Box(surface, rect, c1); break;
      case BorderStyle.Fixed3D:
        if(depressed) { Color t=c1; c1=c2; c2=t; }
        Primitives.Line(surface, rect.X, rect.Y, rect.Right-1, rect.Y, c1);
        Primitives.Line(surface, rect.X, rect.Y, rect.X, rect.Bottom-1, c1);
        Primitives.Line(surface, rect.X, rect.Bottom-1, rect.Right-1, rect.Bottom-1, c2);
        Primitives.Line(surface, rect.Right-1, rect.Y, rect.Right-1, rect.Bottom-1, c2);
        break;
      case BorderStyle.FixedThick: case BorderStyle.Resizeable:
        Color c3, c4;
        if(depressed) { c3=c2; c4=Color.White; c2=c1; c1=Color.Black; }
        else { c4=c2; c2=Color.Black; c3=Color.White; }
        Primitives.Line(surface, rect.X, rect.Y, rect.Right-1, rect.Y, c1);
        Primitives.Line(surface, rect.X, rect.Y, rect.X, rect.Bottom-1, c1);
        Primitives.Line(surface, rect.X, rect.Bottom-1, rect.Right-1, rect.Bottom-1, c2);
        Primitives.Line(surface, rect.Right-1, rect.Y, rect.Right-1, rect.Bottom-1, c2);
        rect.Inflate(-1, -1);
        Primitives.Line(surface, rect.X, rect.Y, rect.Right-1, rect.Y, c3);
        Primitives.Line(surface, rect.X, rect.Y, rect.X, rect.Bottom-1, c3);
        Primitives.Line(surface, rect.X, rect.Bottom-1, rect.Right-1, rect.Bottom-1, c4);
        Primitives.Line(surface, rect.Right-1, rect.Y, rect.Right-1, rect.Bottom-1, c4);
        break;
    }
  }
}
#endregion

} // namespace GameLib.Forms
