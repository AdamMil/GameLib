/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2010 Adam Milazzo

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
using System.Collections.ObjectModel;
using System.Drawing;
using AdamMil.Utilities;
using GameLib.Events;
using GameLib.Video;
using Font = GameLib.Fonts.Font;

// TODO: fix focusing/selecting to support the scenario that a form that is reselected after being deselected has the
// control that previously had keyboard focus focused again

// TODO: try to think of a way to make the layout logic in the base Control class not such a special case (for instance, there
// are assumptions in various places that if the .Dock property is set, then a layout needs to be triggered, but this isn't true
// for layout panels that don't use docking.) i think docking and anchoring should be just another layout style, similar to
// how stacking is a style. this should include removing the Dock and Anchor properties from the Control class and placing them
// somewhere else, because it would be ugly to add the union of all possible layout properties to the control, and it's ugly to
// have layout properties on the base class that are ignored by some layout styles. it'd also be nice to separate the layout
// logic from the Control class, since it's been very confusing at times

// Clicking (autofocusing) a control attempts to select all the controls down to it. then, if it's active and focusable, it is
// focused. (it should perhaps use a variant of the Select() method that doesn't perform focusing, in order to prevent controls
// from gaining input focus and then losing it immediately.) the Select() call should set the parents SelectedControl to 'this'
// if the current control is visible (and enabled?). if that results in the control becoming active, an attempt is made to focus
// it. the Focus() method attempts to select the current control and all its ancestors. then, if it's active and focusable, it is
// focused

namespace GameLib.Forms
{

/* Coordinate systems:
 *   Control coordinates - pixels relative to the control (used for most purposes, such as layout)
 *   Screen coordinates - pixels relative to the display device (used primarily for handling mouse input)
 *   Draw coordinates - pixels relative to the drawing surface (used for drawing)
 */

/// <summary>Implements the basic logic shared between all user interface controls.</summary>
public class Control
{
  /// <summary>Initializes a new <see cref="Control"/>.</summary>
  public Control()
  {
    Controls = new ControlCollection(this);
  }

  #region ControlCollection
  /// <summary>This class provides a strongly-typed collection of <see cref="Control"/> objects.</summary>
  public class ControlCollection : Collection<Control>
  {
    internal ControlCollection(Control parent) { this.parent = parent; }

    /// <summary>Adds several new child controls at once.</summary>
    /// <param name="controls">An array containing the controls to add.</param>
    public void AddRange(params Control[] controls)
    {
      foreach(Control c in controls) Add(c);
    }

    /// <summary>Returns a value indicating whether the given control exists in this collection.</summary>
    /// <param name="control">The control to search for.</param>
    /// <returns>Returns true if the control was found and false otherwise.</returns>
    public new bool Contains(Control control)
    {
      return Contains(control, false);
    }

    /// <summary>Returns a value indicating whether the given control is a child or descendant of this one.</summary>
    /// <param name="control">The control to search for.</param>
    /// <param name="deepSearch">If true, a recursive search is performed, searching descendants if the
    /// control cannot be found here. If false, the search stops after searching the immediate children.
    /// </param>
    /// <returns>Returns true if the control was found and false if not.</returns>
    public bool Contains(Control control, bool deepSearch)
    {
      if(control == null) throw new ArgumentNullException();

      if(deepSearch)
      {
        do
        {
          if(control.Parent == parent) return true;
          control = control.Parent;
        } while(control != null && control != parent);
        return false;
      }
      else
      {
        return control.Parent == parent;
      }
    }

    /// <summary>Returns a value indicating whether the named control a child or descendant of this one.</summary>
    /// <returns>Returns true if the control was found and false if not.</returns>
    /// <remarks>Calling this method is equivalent to calling <see cref="Contains(string, bool)"/> with <c>deepSearch</c> set to false.</remarks>
    public bool Contains(string name)
    {
      return Find(name, false) != null;
    }

    /// <summary>Returns a value indicating whether the named control a child or descendant of this one.</summary>
    /// <returns>Returns true if the control was found and false if not.</returns>
    /// <param name="name">The name of the control.</param>
    /// <param name="deepSearch">If true, a recursive search is performed, searching descendants if the
    /// named control cannot be found here. If false, the search stops after searching the immediate children.
    /// </param>
    public bool Contains(string name, bool deepSearch)
    {
      return Find(name, deepSearch) != null;
    }

    /// <summary>Finds a control by name and returns a reference to it, or null if the control could not be found.</summary>
    /// <remarks>Calling this method is equivalent to calling <see cref="Find(string, bool)"/> with <c>deepSearch</c> set to false.</remarks>
    public Control Find(string name)
    {
      return Find(name, false);
    }

    /// <summary>Finds a control by name and returns a reference to it, or null if the control could not be found.</summary>
    /// <param name="name">The name of the control.</param>
    /// <param name="deepSearch">If true, a recursive search is performed, searching descendants if the
    /// named control cannot be found here. If false, the search stops after searching the immediate children.
    /// </param>
    public Control Find(string name, bool deepSearch)
    {
      int index = IndexOf(name);
      if(index != -1) return this[index];

      if(deepSearch)
      {
        foreach(Control child in this)
        {
          Control descendant = child.Controls.Find(name, true);
          if(descendant != null) return descendant;
        }
      }
      return null;
    }

    /// <summary>Returns the index of the specified child control within the collection, or -1 if the control
    /// could not be found.
    /// </summary>
    public int IndexOf(string name)
    {
      for(int i=0; i < Count; i++)
      {
        if(string.Equals(this[i].Name, name, StringComparison.Ordinal)) return i;
      }
      return -1;
    }

    protected sealed override void ClearItems()
    {
      foreach(Control control in this) control.SetParent(null);
      base.ClearItems();
    }

    protected sealed override void InsertItem(int index, Control control)
    {
      if(control == null) throw new ArgumentNullException();
      if(control.Parent != null) throw new ArgumentException("This control already has a parent.");
      parent.ValidateNewChild(control);
      control.ValidateNewParent(parent);
      base.InsertItem(index, control);
      control.SetParent(parent);
    }

    protected sealed override void RemoveItem(int index)
    {
      Control control = this[index];
      base.RemoveItem(index);
      control.SetParent(null);
    }

    protected sealed override void SetItem(int index, Control control)
    {
      if(control == null) throw new ArgumentNullException();
      Control oldControl = this[index];
      if(oldControl != control)
      {
        if(control.Parent != null) throw new ArgumentException("This control already has a parent.");
        parent.ValidateNewChild(control);
        control.ValidateNewParent(parent);
        base.SetItem(index, control);
        oldControl.SetParent(null);
        control.SetParent(parent);
      }
    }

    Control parent;
  }
  #endregion

  /// <summary>Gets whether this control and all of its ancestors are selected. This does not necessarily indicate that the
  /// control has input focus. To check that, use the <see cref="Focused"/> property.
  /// </summary>
  public bool Active
  {
    get { return HasFlag(Flag.Active); }
  }

  /// <summary>Gets or sets the control's background color. If set to a translucent color (A != 255), the parent
  /// control will be visible through this control. The effective background color can be retrieved by calling
  /// <see cref="GetEffectiveBackColor"/>.
  /// </summary>
  public Color BackColor
  {
    get { return _backColor; }
    set
    {
      if(value != BackColor)
      {
        _backColor = value;
        Invalidate();
      }
    }
  }

  /// <summary>Gets or sets the control's foreground color. If set to <see cref="Color.Empty"/>, the default, the
  /// control will use its parent's foreground color.
  /// </summary>
  public Color ForeColor
  {
    get { return _color; }
    set
    {
      if(value != ForeColor)
      {
        _color = value;
        Invalidate();
      }
    }
  }

  /// <summary>Gets or sets the background image for this control.</summary>
  /// <remarks>Setting the background image causes the image to be displayed in the background of the control.
  /// The alignment of the background image can be controlled using the <see cref="BackImageAlign"/> property.
  /// </remarks>
  public IGuiImage BackImage
  {
    get { return _backImage; }
    set
    {
      if(value != BackImage)
      {
        _backImage = value;
        Invalidate();
      }
    }
  }

  /// <summary>Gets or sets the alignment of the background image.</summary>
  /// <remarks>This setting controls where the <see cref="BackImage"/> will be displayed relative to the control.
  /// </remarks>
  public ContentAlignment BackImageAlign
  {
    get { return _backImageAlign; }
    set
    {
      if(value != BackImageAlign)
      {
        _backImageAlign = value;
        if(BackImage != null) Invalidate();
      }
    }
  }

  /// <summary>Gets or sets the color of the control's border. If set to <see cref="Color.Empty"/>, the default, a
  /// system-defined border color will be used. You can use <see cref="GetEffectiveBorderColor"/> to get the
  /// effective border color.
  /// </summary>
  public Color BorderColor
  {
    get { return _borderColor; }
    set
    {
      Color old = BorderColor;
      _borderColor = value;
      if(value != old && BorderThickness != Size.Empty) Invalidate();
    }
  }

  /// <summary>Gets or sets the control's <see cref="BorderStyle">border style</see>.</summary>
  public BorderStyle BorderStyle
  {
    get { return _borderStyle; }
    set
    {
      if(value != BorderStyle)
      {
        if(Renderer != null && BorderThickness != Renderer.GetBorderThickness(value)) OnContentOffsetChanged();
        _borderStyle = value;
      }
    }
  }

  /// <summary>Gets the width of the control's border.</summary>
  /// <value>The width of the border, in pixels.</value>
  /// <remarks>The border width is determined by the <see cref="BorderStyle"/> property.</remarks>
  public Size BorderThickness
  {
    get { return Renderer == null ? Size.Empty : Renderer.GetBorderThickness(BorderStyle); }
  }

  /// <summary>Determines where the control will be anchored in relation to its parent.</summary>
  /// <remarks>When a control is anchored, it will be automatically moved and/or resized when its parent is resized
  /// in order to maintain the same spacing to the anchored edges. Setting this property will automatically set
  /// the <see cref="Dock"/> property to <see cref="DockStyle.None"/> because the two properties are incompatible.
  /// The default value is <see cref="AnchorStyle.TopLeft"/>.
  /// </remarks>
  public AnchorStyle Anchor
  {
    get { return _anchor; }
    set
    {
      if(value != Anchor)
      {
        if(value == AnchorStyle.None) throw new ArgumentException("Anchor cannot be directly set to None.");
        _dock   = DockStyle.None;
        _anchor = value;
      }
    }
  }

  /// <summary>Gets or sets how this control will be docked to its parent.</summary>
  /// <remarks>When a control is docked, it will be automatically moved and/or resized when its parent is resized
  /// in order to fill the entire edge that it's docked to. Multiple controls can be docked at once, even to the
  /// same edge. The docking calculations for the controls are performed in the same order that the controls are
  /// in the <see cref="Controls"/> collections. Setting this property to anything but <see cref="DockStyle.None"/>
  /// will cause the <see cref="Anchor"/> property to be ignored, because the two properties are incompatible.
  /// </remarks>
  public DockStyle Dock
  {
    get { return _dock; }
    set
    {
      if(value != Dock)
      {
        _anchor = value != DockStyle.None ? AnchorStyle.None : AnchorStyle.TopLeft;
        _dock   = value;
        if(Parent != null) Parent.TriggerLayout(); // changing the dock style requires a relayout within the parent
      }
    }
  }

  /// <summary>Gets or sets the location and size of this control, in pixels, relative to the top-left corner of its
  /// parent control.
  /// </summary>
  public Rectangle Bounds
  {
    get { return _bounds; }
    set { SetBounds(value, false); }
  }

  /// <summary>Gets or sets the control's location, in pixels, relative to the top-left corner of its parent control.</summary>
  public Point Location
  {
    get { return Bounds.Location; }
    set { SetBounds(value, Size, false); }
  }

  /// <summary>Gets or sets the control's X coordinate, in pixels, relative to the left of its parent control.</summary>
  public int Left
  {
    get { return Bounds.X; }
    set { Location = new Point(value, Bounds.Y); }
  }

  /// <summary>Gets or sets the control's Y coordinate, in pixels, relative to the top of its parent control.</summary>
  public int Top
  {
    get { return Bounds.Y; }
    set { Location = new Point(Bounds.X, value); }
  }

  /// <summary>Gets or sets the control's X coordinate, in pixels, relative to the left of its parent control.</summary>
  public int Right
  {
    get { return Bounds.Right; }
    set { Location = new Point(value-Width, Bounds.Y); }
  }

  /// <summary>Gets or sets the position of the bottom edge of the control relative to its parent.</summary>
  /// <remarks>Changing this property will move the control.</remarks>
  public int Bottom
  {
    get { return Bounds.Bottom; }
    set { Location = new Point(Bounds.X, value-Height); }
  }

  /// <summary>Gets or sets the size of the control, in pixels.</summary>
  public Size Size
  {
    get { return Bounds.Size; }
    set { SetBounds(Location, value, false); }
  }

  /// <summary>Gets or sets the width of the control, in pixels.</summary>
  public int Width
  {
    get { return Bounds.Width; }
    set { Size = new Size(value, Bounds.Height); }
  }

  /// <summary>Gets or sets the height of the control, in pixels.</summary>
  public int Height
  {
    get { return Bounds.Height; }
    set { Size = new Size(Bounds.Width, value); }
  }

  /// <summary>Gets the control's bounds, in control coordinates. This is a rectangle of size <see cref="Size"/>,
  /// located at position 0,0.
  /// </summary>
  public Rectangle ControlRect
  {
    get { return new Rectangle(0, 0, Bounds.Width, Bounds.Height); }
  }

  /// <summary>Gets the offset that defines the content area.</summary>
  /// <value>A <see cref="RectOffset"/> that will be used to shrink the <see cref="ControlRect"/> and obtain
  /// the <see cref="ContentRect"/>.
  /// </value>
  /// <remarks>By default, this is defined as the offset created by <see cref="Padding"/> and
  /// <see cref="BorderThickness"/>. It can be overridden to provide a new definition of the content area, but for
  /// compatibility, you should define your new offset as a modification of the base class's version. Also,
  /// make sure to call <see cref="OnContentOffsetChanged"/> if your criteria for evaluating the offset change.
  /// </remarks>
  public virtual RectOffset ContentOffset
  {
    get
    {
      RectOffset offset = Padding;
      offset.Offset(BorderThickness);
      return offset;
    }
  }

  /// <summary>Gets the rectangle representing the content area, in control coordinates. This is the area of the
  /// control in which child controls and other content should be placed.
  /// </summary>
  public Rectangle ContentRect
  {
    get { return Rectangle.Intersect(ContentOffset.Shrink(ControlRect), ControlRect); }
  }

  /// <summary>Gets or sets the control's padding.</summary>
  /// <value>A <see cref="RectOffset"/> object representing the control's padding.</value>
  /// <remarks>The control's padding is a buffer area inside the border. By default, controls will not be laid out
  /// in the padding area, and controls will not paint inside the padding (except to fill their background color).
  /// You can use the padding to set the area in which controls will be laid out, as well as to simply provide buffer
  /// room around a control's content.
  /// </remarks>
  public RectOffset Padding
  {
    get { return _padding; }
    set
    {
      if(value != Padding)
      {
        if(value.Left < 0 || value.Top < 0 || value.Right < 0 || value.Bottom < 0)
        {
          throw new ArgumentOutOfRangeException("Padding", value, "offset cannot be negative");
        }
        _padding = value;
        OnContentOffsetChanged();
      }
    }
  }

  /// <summary>Gets a collection containing this control's children.</summary>
  public ControlCollection Controls
  {
    get; private set;
  }

  /// <summary>Gets the control that this this control belongs to, or null if this control is not owned by any other.</summary>
  public Control Parent
  {
    get { return _parent; }
    set
    {
      if(value != Parent)
      {
        ValueChangedEventArgs<Control> e = new ValueChangedEventArgs<Control>(_parent);
        SetFlags(Flag.MyChange, true); // set this flag so we don't get two OnParentChanged() notifications
        if(Parent != null) Parent.Controls.Remove(this);
        if(value != null) value.Controls.Add(this);
        SetFlags(Flag.MyChange, false);
        OnParentChanged(e);
      }
    }
  }

  /// <summary>Enables or disables mouse capture for this control.</summary>
  /// <remarks>Mouse capture allows a control to receive mouse events even if the mouse is not within the bounds
  /// of the control. Capturing applies to the descendants of the control as well, so they will receive mouse events as normal,
  /// if the mouse is over them. Because mouse capture requires an active desktop, this property cannot be set unless the
  /// control is associated with a desktop. For information on associating a control with a desktop, see the
  /// <see cref="Desktop"/> property. Since only one control can capture the mouse at a time, setting this to true
  /// will take the mouse capture away from any other control that has mouse capture.
  /// </remarks>
  public bool Capture
  {
    get { return Desktop != null && Desktop.IsCapturing(this); }
    set
    {
      if(value)
      {
        if(Desktop == null) throw new InvalidOperationException("This control has no desktop");
        Desktop.SetCapture(this);
      }
      else if(Desktop != null && Desktop.IsCapturing(this))
      {
        Desktop.SetCapture(null);
      }
    }
  }

  /// <summary>Gets the desktop to which this control belongs, or null if it is not part of any desktop.</summary>
  public Desktop Desktop
  {
    get; internal set;
  }

  /// <summary>Gets or sets whether the control and all of its ancestors are enabled.</summary>
  public bool EffectivelyEnabled
  {
    get { return HasFlag(Flag.EffectivelyEnabled); }
  }

  /// <summary>Gets or sets whether the control and all of its ancestors are visible.</summary>
  public bool EffectivelyVisible
  {
    get { return HasFlag(Flag.EffectivelyVisible); }
  }

  /// <summary>Gets or sets whether the control will respond to user input. The default is true. Note that the
  /// control will still be disabled if one of its ancestors is disabled. To check whether the control and all its
  /// ancestors are enabled, use the <see cref="EffectivelyEnabled"/> property.
  /// </summary>
  public bool Enabled
  {
    get { return HasFlag(Flag.Enabled); }
    set
    {
      if(value != Enabled)
      {
        SetFlags(Flag.Enabled, value);
        RecursivelySetEnabled();
        OnEnabledChanged(new ValueChangedEventArgs<bool>(!value));
      }
    }
  }

  /// <summary>Returns true if this control has input focus.</summary>
  /// <remarks>The control that has input focus will receive keyboard events. Many controls can be selected (see
  /// <see cref="Selected"/>) or active (see <see cref="Active"/>) but only one control can have input focus -- the control whose
  /// ancestors are all selected, which has the <see cref="Forms.ControlStyle.CanReceiveFocus"/> style, is effectively visible
  /// and effectively enabled, and which doesn't have a selected descendant with those same qualities. To set input focus,
  /// use the <see cref="Focus"/> method.
  /// </remarks>
  public bool Focused
  {
    get { return HasFlag(Flag.Focused); }
  }

  /// <summary>Gets or sets the font that this control should use.</summary>
  /// <remarks>Setting this property to <c>null</c> will cause it to inherit its parent's font. To get the effective
  /// font, use <see cref="EffectiveFont"/>.
  /// </remarks>
  public Font Font
  {
    get { return _font; }
    set
    {
      if(value != Font)
      {
        Font old = Font;
        _font = value;
        RecursivelySetFont();
      }
    }
  }

  /// <summary>Gets or sets whether this control receives keyboard events before its children.</summary>
  /// <remarks>If a control has key preview, it will receive keyboard events before its children, and have
  /// a chance to process and/or cancel them.
  /// </remarks>
  public bool KeyPreview
  {
    get { return HasFlag(Flag.KeyPreview); }
    set { SetFlags(Flag.KeyPreview, value); }
  }

  /// <summary>Gets or sets whether this is a modal control.</summary>
  /// <remarks>A modal control will receive input focus and all keyboard events, and cannot lose focus unless a new
  /// modal control is opened after it.
  /// </remarks>
  public bool Modal
  {
    get { return HasFlag(Flag.Modal); }
  }

  /// <summary>Gets or sets name of this control.</summary>
  /// <remarks>The name can be used to locate a control within its parent easily, using the
  /// <see cref="ControlCollection.Find"/> methods, but is otherwise not used by the windowing system.
  /// For proper functioning, the name should be unique among all its ancestors and their descendants.
  /// </remarks>
  public string Name
  {
    get { return _name; }
    set
    {
      if(value == null) throw new ArgumentNullException();
      _name = value;
    }
  }

  /// <summary>Gets whether this control can be selected.</summary>
  /// <remarks>A control can be selected if it has a parent and is effectively enabled and effectively visible.</remarks>
  public bool Selectable
  {
    get { return Parent != null && HasFlags(Flag.EffectivelyEnabled | Flag.EffectivelyVisible); }
  }

  /// <summary>Gets or sets whether this control is its parent's selected control.</summary>
  /// <remarks>This property only checks whether the control is selected by its immediate parent. To check whether all of the
  /// control's ancestors are selected, use the <see cref="Active"/> property.
  /// </remarks>
  public bool Selected
  {
    get { return Parent != null && Parent.SelectedControl == this; }
    set
    {
      if(value)
      {
        if(Visible) Parent.SelectedControl = this;
      }
      else
      {
        if(Parent.SelectedControl == this) Parent.SelectedControl = null;
      }
    }
  }

  /// <summary>Gets the child control that is selected, or null if no children have are selected. Note that this does not
  /// indicate that the child actually is active or will receive keyboard events. See <see cref="Active"/> and
  /// <see cref="Focused"/> for more details.
  /// </summary>
  public Control SelectedControl
  {
    get { return _selected; }
    set
    {
      if(value != SelectedControl)
      {
        if(value != null && !Controls.Contains(value)) throw new ArgumentException("Not a child of this control.");

        Control previouslySelected = SelectedControl;
        if(previouslySelected != null) RecursivelyDeactivate(previouslySelected);
        _selected = value;
        if(previouslySelected != null) previouslySelected.OnDeselected();
        if(value != null) RecursivelyActivate(value);
      }
    }
  }

  /// <summary>Gets or sets this control's <see cref="ControlStyle"/>.</summary>
  /// <remarks>This property is generally designed to be set by the implementors of controls. Altering this
  /// property could invalidate assumptions made by the control and cause problems. See the
  /// <see cref="ControlStyle"/> enum for more information about control styles.
  /// </remarks>
  public ControlStyle ControlStyle
  {
    get { return (ControlStyle)(flags & Flag.ControlStyleMask); }
    set
    {
      bool updateDrawTarget =
        ((ControlStyle)flags & ControlStyle.CustomDrawTarget) != (value & ControlStyle.CustomDrawTarget);
      flags = flags & ~Flag.ControlStyleMask | ((Flag)value & Flag.ControlStyleMask);
      if(updateDrawTarget) UpdateDrawTarget(false, false);
    }
  }

  /// <summary>Gets or sets this control's position within the tab order.</summary>
  /// <remarks>Controls are often meant to be used in a certain order. By setting this property, you can alter
  /// the logical order of controls. <see cref="TabToNextControl"/> and <see cref="Desktop"/> both use
  /// this property to determine in what order controls should be focused. The default value is -1, which means
  /// that this control is not part of the tab ordering.
  /// </remarks>
  public int TabIndex
  {
    get { return _tabIndex; }
    set
    {
      if(value < -1) throw new ArgumentOutOfRangeException("TabIndex", "must be >= -1");
      _tabIndex = value;
    }
  }

  /// <summary>Gets or sets a bit of user data associated with this control.</summary>
  /// <remarks>This property is not altered or used by the windowing system in any way. It is meant to be used
  /// to associate any context with controls that might be helpful.
  /// </remarks>
  public object Tag { get; set; }

  /// <summary>Gets or sets this control's text.</summary>
  public virtual string Text
  {
    get { return _text; }
    set
    {
      if(!string.Equals(value, _text, StringComparison.Ordinal))
      {
        if(value == null) throw new ArgumentNullException();
        ValueChangedEventArgs<string> e = new ValueChangedEventArgs<string>(_text);
        _text = value;
        OnTextChanged(e);
      }
    }
  }

  /// <summary>Gets or sets whether the control will render itself or its children. The default is true. Note that the
  /// control will still be hidden if one of its ancestors is not visible. To check whether the control and all its
  /// ancestors are visible, use the <see cref="EffectivelyVisible"/> property.
  /// </summary>
  public bool Visible
  {
    get { return HasFlag(Flag.Visible); }
    set
    {
      if(value != Visible)
      {
        SetFlags(Flag.Visible, value);
        RecursivelySetVisible();
        OnVisibleChanged(new ValueChangedEventArgs<bool>(!value));
      }
    }
  }

  #region Events
  /// <summary>Occurs after a child is added to this control.</summary>
  public event EventHandler<ControlEventArgs> ControlAdded;

  /// <summary>Occurs after a child is removed from this control.</summary>
  public event EventHandler<ControlEventArgs> ControlRemoved;

  /// <summary>Occurs when the mouse button is double-clicked inside a control's area.</summary>
  /// <remarks>If another control has captured mouse input or is the topmost modal contral, this event will only
  /// be raised for that control. The control must have the <see cref="Forms.ControlStyle.DoubleClickable"/> style to
  /// receive this event.
  /// <seealso cref="Forms.Desktop.DoubleClickDelay"/>
  /// </remarks>
  public event EventHandler<ClickEventArgs> DoubleClick;

  /// <summary>Occurs when the mouse is clicked and dragged inside the control's area.</summary>
  /// <remarks>If another control has captured mouse input or is the topmost modal contral, this event will only
  /// be raised for that control. The control must have the <see cref="Forms.ControlStyle.Draggable"/> style to
  /// receive this event.
  /// <seealso cref="Control.DragThreshold"/>
  /// <seealso cref="Forms.Desktop.DefaultDragThreshold"/>
  /// </remarks>
  public event EventHandler<DragEventArgs> DragStart;

  /// <summary>Occurs when the mouse is moved after a drag has started. <seealso cref="DragStart"/></summary>
  public event EventHandler<DragEventArgs> DragMove;

  /// <summary>Occurs when the mouse button is released, ending a drag. <seealso cref="DragStart"/></summary>
  public event EventHandler<DragEventArgs> DragEnd;

  /// <summary>Occurs when the control receives keyboard focus.</summary>
  public event EventHandler GotFocus;

  /// <summary>Occurs when the control loses keyboard focus.</summary>
  public event EventHandler LostFocus;

  /// <summary>Occurs when a keyboard key is pressed and this control has input focus.</summary>
  public event EventHandler<KeyEventArgs> KeyDown;

  /// <summary>Occurs when a keyboard key is released and this control has input focus.</summary>
  public event EventHandler<KeyEventArgs> KeyUp;

  /// <summary>Occurs when a key having an associated character is pressed and this control has input focus.</summary>
  /// <remarks>Some keys do not have associated characters, such as the shift keys, the arrow keys, etc., and these
  /// will not cause the KeyPress event to be raised. Some key presses depend on the state of other keys. For instance,
  /// the character associated with the A key depends on the state of the Caps Lock state and modifier keys such as
  /// Shift and Ctrl. These key combinations will nonetheless result in only one KeyPress event being raised
  /// (per character).
  /// </remarks>
  public event EventHandler<KeyEventArgs> KeyPress;

  /// <summary>Occurs when the control is about to lay out its children.</summary>
  /// <remarks>This event should be raised before the actual layout code executes, so the event handler can
  /// make modifications to control positions and expect the changes to be taken into account.
  /// </remarks>
  public event EventHandler<LayoutEventArgs> Layout;

  /// <summary>Occurs when the mouse button is both pressed and released inside a control's area.</summary>
  /// <remarks>If another control has captured mouse input or is the topmost modal contral, this event will only
  /// be raised for that control. The control must have the <see cref="Forms.ControlStyle.Clickable"/> style to receive
  /// this event.
  /// </remarks>
  public event EventHandler<ClickEventArgs> MouseClick;

  /// <summary>Occurs when the mouse wheel is moved while the cursor is inside the control.</summary>
  /// <remarks>If another control has captured mouse input or is the topmost modal contral, this event will only
  /// be raised for that control.
  /// </remarks>
  public event EventHandler<ClickEventArgs> MouseWheel;

  /// <summary>Occurs when the mouse is positioned over the control or one of its ancestors.</summary>
  /// <remarks>If at any level in the control hierarchy, there are multiple overlapping sibling controls under the
  /// cursor, the one with the highest Z-order will be given precedence. This event will not be raised for
  /// controls that are not visible.
  /// </remarks>
  public event EventHandler MouseEnter;

  /// <summary>Occurs when the mouse is moved over a control's surface.</summary>
  /// <remarks>If another control has captured mouse input or is the topmost modal contral, this event will only
  /// be raised for that control.
  /// </remarks>
  public event MouseMoveEventHandler MouseMove;

  /// <summary>Occurs when the mouse button is pressed inside a control's area.</summary>
  /// <remarks>If another control has captured mouse input or is the topmost modal contral, this event will only
  /// be raised for that control. The control must have the <see cref="Forms.ControlStyle.Clickable"/> style to receive
  /// this event.
  /// </remarks>
  public event EventHandler<ClickEventArgs> MouseDown;

  /// <summary>Occurs when the mouse button is released inside a control's area.</summary>
  /// <remarks>If another control has captured mouse input or is the topmost modal contral, this event will only
  /// be raised for that control. The control must have the <see cref="Forms.ControlStyle.Clickable"/> style to receive
  /// this event.
  /// </remarks>
  public event EventHandler<ClickEventArgs> MouseUp;

  /// <summary>Occurs when the mouse is leaves the control's area.</summary>
  /// <remarks>If the <see cref="MouseEnter"/> event was not raised for this control, the <see cref="MouseLeave"/>
  /// event will not be raised either. This event will not be raised for controls that are not visible.
  /// </remarks>
  public event EventHandler MouseLeave;

  /// <summary>Occurs after the value of the <see cref="Location"/> property changes.</summary>
  public event EventHandler Move;

  /// <summary>Occurs after a control has been painted, but before its children have been.</summary>
  public event EventHandler<PaintEventArgs> Paint;

  /// <summary>Occurs after the background of a control has been painted.</summary>
  public event EventHandler<PaintEventArgs> PaintBackground;

  /// <summary>Occurs after the value of the <see cref="Size"/> property changes.</summary>
  public event EventHandler Resize;

  /// <summary>Occurs when the value of the <see cref="Text"/> property changes.</summary>
  public event ValueChangedEventHandler<string> TextChanged;

  /// <summary>Occurs when the control is activated (i.e. <see cref="Active"/> became true).</summary>
  protected virtual void OnActivated() { }

  /// <summary>Occurs when the control is deactivated (i.e. <see cref="Active"/> became false).</summary>
  protected virtual void OnDeactivated() { }

  /// <summary>Occurs when the control is deselected by its parent.</summary>
  /// <remarks>This event only signifies that the control was deselected by its parent. The control may not have have actually
  /// been an active control (i.e. it doesn't mean <see cref="Active"/> was true) before this method was called.
  /// </remarks>
  protected virtual void OnDeselected() { }

  /// <summary>Called when the control is selected by its parent, becoming the <see cref="SelectedControl"/>.</summary>
  /// <remarks>This event only signifies that the control was selected by its parent. It doesn't mean that the entire chain of
  /// is ancestors is selected. To check that, use the <see cref="Active"/> property.
  /// </remarks>
  protected virtual void OnSelected() { }

  /// <summary>Called when the background color has been changed.</summary>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class' version to ensure that
  /// the default processing gets performed. The proper place to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnBackColorChanged(ValueChangedEventArgs<Color> e)
  {
    Invalidate();
  }

  /// <summary>Called when the size of the content offset changes.</summary>
  /// <remarks>This method raises the When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that default processing gets performed.
  /// </remarks>
  protected virtual void OnContentOffsetChanged()
  {
    TriggerLayout();
    Invalidate();
  }

  /// <summary>Called when this control's draw target changes. This method is not called when an ancestor's draw target
  /// changes.
  /// </summary>
  protected virtual void OnDrawTargetChanged()
  {
    Invalidate();
  }

  /// <summary>Called when the <see cref="EffectiveFont"/> property has been changed.</summary>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class' version to ensure that
  /// the default processing gets performed. The proper place to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnEffectiveFontChanged(ValueChangedEventArgs<Font> e)
  {
    Invalidate();
  }

  /// <summary>Called when the <see cref="Enabled"/> property has been changed.</summary>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class' version to ensure that
  /// the default processing gets performed. The proper place to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnEnabledChanged(ValueChangedEventArgs<bool> e)
  {
    if(!Enabled) Blur();
    Invalidate();
  }

  /// <summary>Called when the <see cref="ForeColor"/> property has been changed.</summary>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class' version to ensure that
  /// the default processing gets performed. The proper place to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnForeColorChanged(ValueChangedEventArgs<Color> e)
  {
    if(e.OldValue != GetEffectiveForeColor()) Invalidate();
  }

  /// <summary>Called when the <see cref="Location"/> property has changed.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs{T}"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class' version to ensure that
  /// the default processing gets performed. The proper place to do this is at the end of the derived version.
  /// </remarks>
  protected virtual void OnLocationChanged(ValueChangedEventArgs<Point> e)
  {
    if(EffectivelyVisible && Parent != null)
    {
      // invalidate the area of the parent where we used to be
      Parent.Invalidate(new Rectangle(e.OldValue, Size));
      Invalidate(); // and invalidate our new area so that we get redrawn
    }

    OnMove();
  }

  /// <summary>Called when the <see cref="Parent"/> property changes.</summary>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class' version to ensure that
  /// the default processing gets performed. The proper place for this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnParentChanged(ValueChangedEventArgs<Control> e)
  {
    if(Parent != null) Invalidate();
  }

  /// <summary>Called when the <see cref="Size"/> property changes.</summary>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class' version to ensure that
  /// the default processing gets performed. The proper place to do this is at the end of the derived version.
  /// </remarks>
  protected virtual void OnSizeChanged(ValueChangedEventArgs<Size> e)
  {
    if(invalid.Width != 0) invalid.Intersect(ControlRect); // if we have an invalid rectangle, clip it to the new size

    UpdateDrawTarget(false, false);

    // if we have a parent and got smaller, we'll need to invalidate a portion of the parent
    if(Parent != null && EffectivelyVisible)
    {
      Size oldSize = e.OldValue;
      if(Width < oldSize.Width) // if the width shrunk...
      {
        // if both dimensions shrunk, invalidate the area of the parent containing our old location. it may seem
        // wasteful to invalidate the entire area rather than the two slivers, but due to the way Invalidate() works,
        // the effect would be the same, because the rectangular union of the two slivers would be the entire area
        if(Height < oldSize.Height)
        {
          Parent.Invalidate(new Rectangle(Location, oldSize));
        }
        else // only the width shrunk, so invalidate just the sliver on the right side that we used to occupy
        {
          Parent.Invalidate(new Rectangle(Right, Top, oldSize.Width-Width, oldSize.Height));
        }
      }
      // if only the height shrunk, invalidate the sliver on the bottom that we used to occupy
      else if(Height < oldSize.Height)
      {
        Parent.Invalidate(new Rectangle(Left, Bottom, oldSize.Width, oldSize.Height-Height));
      }
    }

    TriggerLayout();
    Invalidate();
    OnResize();
  }

  /// <summary>Called when the <see cref="Text"/> property changes.</summary>
  /// <param name="e">A <see cref="ValueChangedEventArgs{T}"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected virtual void OnTextChanged(ValueChangedEventArgs<string> e)
  {
    if(TextChanged!=null) TextChanged(this, e);
  }

  /// <summary>Called when the <see cref="Visible"/> property changes.</summary>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class' version to ensure that
  /// the default processing gets performed. The proper place to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnVisibleChanged(ValueChangedEventArgs<bool> e)
  {
    if(!Visible)
    {
      Blur();
      if(Parent != null) Parent.Invalidate(Bounds);
    }
    else
    {
      Invalidate();
    }
  }

  /// <summary>Raises the <see cref="GotFocus"/> event.</summary>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected virtual void OnGotFocus()
  {
    if(GotFocus != null) GotFocus(this, EventArgs.Empty);
  }

  /// <summary>Raises the <see cref="LostFocus"/> event.</summary>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected virtual void OnLostFocus()
  {
    if(LostFocus != null) LostFocus(this, EventArgs.Empty);
  }

  /// <summary>Raises the <see cref="Layout"/> event and then performs a layout.</summary>
  /// <param name="e">A <see cref="LayoutEventArgs"/> that contains the event data.</param>
  /// <remarks>It is usually better to override the <see cref="LayOutChildren"/> method than this one, because then you
  /// won't have to duplicate the other logic that surrounds layout like propogating a layout recursively.
  /// When overriding this method in a derived class, be sure to call the base class' version to ensure that
  /// the default processing gets performed. The proper place to do this is at the beginning of the derived version.
  /// </remarks>
  protected internal virtual void OnLayout(LayoutEventArgs e)
  {
    if(Layout != null) Layout(this, e);

    SetFlags(Flag.PendingLayout | Flag.RecursiveLayout, false);
    anchorSpace = ContentRect;
    LayOutChildren();

    if(e.Recursive)
    {
      foreach(Control child in Controls) child.OnLayout(e);
    }
  }

  /// <summary>Raises the <see cref="MouseEnter"/> event.</summary>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnMouseEnter() { if(MouseEnter != null) MouseEnter(this, EventArgs.Empty); }

  /// <summary>Raises the <see cref="MouseLeave"/> event.</summary>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnMouseLeave() { if(MouseLeave != null) MouseLeave(this, EventArgs.Empty); }

  /// <summary>Raises the <see cref="Move"/> event.</summary>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected virtual void OnMove() { if(Move != null) Move(this, EventArgs.Empty); }

  /// <summary>Raises the <see cref="Resize"/> event.</summary>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected virtual void OnResize() { if(Resize != null) Resize(this, EventArgs.Empty); }

  /// <summary>Raises the <see cref="ControlAdded"/> event and performs default handling.</summary>
  /// <param name="e">A <see cref="ControlEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed. The proper place to do this is at the
  /// end of the derived version.
  /// </remarks>
  protected virtual void OnControlAdded(ControlEventArgs e)
  {
    if(e.Control.Dock == DockStyle.None)
    {
      e.Control.SetBounds(e.Control.Bounds, false); // set the same bounds again to set up layout logic
    }
    else
    {
      switch(e.Control.Dock)
      {
        case DockStyle.Top: case DockStyle.Bottom:
          e.Control.SetBounds(e.Control.Location, new Size(ContentRect.Width, e.Control.Height), false);
          break;
        case DockStyle.Left: case DockStyle.Right:
          e.Control.SetBounds(e.Control.Location, new Size(e.Control.Width, ContentRect.Height), false);
          break;
        case DockStyle.Fill:
          e.Control.SetBounds(Bounds, false);
          break;
      }
    }

    if(ControlAdded != null) ControlAdded(this, e);
  }

  /// <summary>Raises the <see cref="ControlRemoved"/> event and performs default handling.</summary>
  /// <param name="e">A <see cref="ControlEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed. The proper place to do this is at the end
  /// of the derived version.
  /// </remarks>
  protected virtual void OnControlRemoved(ControlEventArgs e)
  {
    if(SelectedControl == e.Control) e.Control.Deselect();
    if(e.Control.Dock != DockStyle.None) TriggerLayout();
    if(e.Control.EffectivelyVisible) Invalidate(e.Control.Bounds);
    if(ControlRemoved != null) ControlRemoved(this, e);
  }

  /// <summary>Called when the control receives a custom window event.</summary>
  /// <param name="e">A <see cref="ControlEvent"/> that contains the event.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class' version if you
  /// don't completely handle the event, or the event could possibly be handled by another class.
  /// </remarks>
  protected internal virtual void OnCustomEvent(ControlEvent e) { }

  /// <summary>Raises the <see cref="KeyDown"/> event.</summary>
  /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnKeyDown(KeyEventArgs e) { if(KeyDown != null) KeyDown(this, e); }

  /// <summary>Raises the <see cref="KeyUp"/> event.</summary>
  /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnKeyUp(KeyEventArgs e) { if(KeyUp != null) KeyUp(this, e); }

  /// <summary>Raises the <see cref="KeyPress"/> event.</summary>
  /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
  /// <remarks>This method will only be called for keystrokes that produce characters. When overriding this
  /// method in a derived class, be sure to call the base class' version to ensure that the default processing
  /// gets performed.
  /// </remarks>
  protected internal virtual void OnKeyPress(KeyEventArgs e) { if(KeyPress != null) KeyPress(this, e); }

  /// <summary>Raises the <see cref="MouseMove"/> event.</summary>
  /// <param name="e">A <see cref="MouseMoveEvent"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnMouseMove(MouseMoveEvent e) { if(MouseMove != null) MouseMove(this, e); }

  /// <summary>Raises the <see cref="MouseDown"/> event.</summary>
  /// <param name="e">A <see cref="ClickEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnMouseDown(ClickEventArgs e) { if(MouseDown != null) MouseDown(this, e); }

  /// <summary>Raises the <see cref="MouseUp"/> event.</summary>
  /// <param name="e">A <see cref="ClickEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnMouseUp(ClickEventArgs e) { if(MouseUp != null) MouseUp(this, e); }

  /// <summary>Raises the <see cref="MouseWheel"/> event.</summary>
  /// <param name="e">A <see cref="ClickEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnMouseWheel(ClickEventArgs e) { if(MouseWheel != null) MouseWheel(this, e); }

  /// <summary>Raises the <see cref="MouseClick"/> event.</summary>
  /// <param name="e">A <see cref="ClickEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnMouseClick(ClickEventArgs e) { if(MouseClick != null) MouseClick(this, e); }

  /// <summary>Raises the <see cref="DoubleClick"/> event.</summary>
  /// <param name="e">A <see cref="ClickEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnDoubleClick(ClickEventArgs e) { if(DoubleClick != null) DoubleClick(this, e); }

  /// <summary>Raises the <see cref="DragStart"/> event.</summary>
  /// <param name="e">A <see cref="DragEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnDragStart(DragEventArgs e) { if(DragStart != null) DragStart(this, e); }

  /// <summary>Raises the <see cref="DragMove"/> event.</summary>
  /// <param name="e">A <see cref="DragEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnDragMove(DragEventArgs e) { if(DragMove != null) DragMove(this, e); }

  /// <summary>Raises the <see cref="DragEnd"/> event.</summary>
  /// <param name="e">A <see cref="DragEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed.
  /// </remarks>
  protected internal virtual void OnDragEnd(DragEventArgs e) { if(DragEnd != null) DragEnd(this, e); }

  /// <summary>Raises the <see cref="Paint"/> event and performs default painting.</summary>
  /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
  /// <remarks>When overriding this method in a derived class, be sure to call the base class'
  /// version to ensure that the default processing gets performed. The proper place to do this is at the start
  /// of the derived version. The draw target's <see cref="IGuiRenderTarget.ClipRect" /> property will be set equal
  /// to the <see cref="PaintEventArgs.DrawRect"/> property.
  /// </remarks>
  protected virtual void OnPaint(PaintEventArgs e)
  {
    if(e.ControlRect.Contains(invalid)) invalid.Width = 0;
    SetFlags(Flag.PendingRepaint, false);
  }

  /// <summary>Paints the background of the control.</summary>
  protected virtual void OnPaintBackground(PaintEventArgs e)
  {
    PaintBackgroundColor(e);
    PaintBackgroundImage(e);
    PaintBorder(e);
  }
  #endregion

  /// <summary>Removes input focus from this control, but does not deselect it.</summary>
  public void Blur()
  {
    if(Focused)
    {
      SetFlags(Flag.Focused, false);
      OnLostFocus();
    }
  }

  /// <summary>Deselects the current control, if it's selected.</summary>
  public void Deselect()
  {
    if(Selected) Parent.SelectedControl = null;
  }

  /// <summary>Attempts to give this control input focus.</summary>
  /// <remarks>Input focus will not actually be given to a control if it does not have the
  /// <see cref="Forms.ControlStyle.CanReceiveFocus"/> style or cannot be made active.
  /// </remarks>
  public void Focus()
  {
    if(!Focused)
    {
      // before the control can be focused, it needs to be active. we'll try to make it active by selecting it and its ancestors
      for(Control control = this; !control.Active && control.Selectable; control = control.Parent)
      {
        control.Select(false);
      }

      // if it's active now, and can receive focus, then try to focus it
      if(HasFlags(Flag.Active | (Flag)ControlStyle.CanReceiveFocus))
      {
        // search both up and down the control tree to find a control that may have been focused previously. controls in other
        // areas of the control tree will already have been blurred during the act of activating this control
        Control controlToBlur = null;
        for(Control ancestor = Parent; ancestor != null; ancestor = ancestor.Parent)
        {
          if(ancestor.Focused)
          {
            controlToBlur = ancestor;
            break;
          }
        }

        if(controlToBlur == null)
        {
          for(Control descendant = SelectedControl; descendant != null; descendant = descendant.SelectedControl)
          {
            if(descendant.Focused)
            {
              controlToBlur = descendant;
              break;
            }
          }
        }

        // if we found another control that's focused, blur it
        if(controlToBlur != null) controlToBlur.Blur();

        // then focus this control
        SetFlags(Flag.Focused, true);
        OnGotFocus();
      }
    }
  }

  /// <summary>Attempts to select the current control and give it input focus. This method does not select the control's
  /// ancestors.
  /// </summary>
  public void Select()
  {
    Select(true);
  }

  /// <summary>Attempts to select the current control and optionally give it input focus. This method does not select the
  /// control's ancestors.
  /// </summary>
  /// <param name="focus">If true, the method will attempt to give the control input focus.</param>
  public void Select(bool focus)
  {
    if(Selectable) Parent.SelectedControl = this;
    if(focus && Active) Focus();
  }

  /// <summary>Brings this control to the front of the Z order, ensuring that it's drawn above its other siblings.</summary>
  public void BringToFront()
  {
    AssertParent();

    Control parent = this.Parent;
    if(parent.Controls[parent.Controls.Count - 1] != this)
    {
      SetFlags(Flag.JustSetParent, true);
      parent.Controls.Remove(this);
      parent.Controls.Add(this);
      SetFlags(Flag.JustSetParent, false);
      Invalidate();
    }
  }

  /// <summary>Sends this control to the back of the Z order, ensuring that it's drawn below its other siblings.</summary>
  public void SendToBack()
  {
    AssertParent();

    Control parent = this.Parent;
    if(parent.Controls[0] != this)
    {
      SetFlags(Flag.JustSetParent, true);
      parent.Controls.Remove(this);
      parent.Controls.Insert(0, this);
      SetFlags(Flag.JustSetParent, false);
    }
  }

  /// <summary>Converts a point from control coordinates to an ancestor's control coordinates. If
  /// <paramref name="ancestor"/> is null, the point will be converted to screen coordinates. Otherwise, if the control
  /// does not belong to the given ancestor, an exception will be thrown.
  /// </summary>
  public Point ControlToAncestor(Point controlPoint, Control ancestor)
  {
    Control control = this;
    do
    {
      controlPoint = control.ControlToParent(controlPoint);
      control      = control.Parent;
    } while(control != ancestor && control != null);

    if(control == null && ancestor != null)
    {
      throw new ArgumentException("The control does not belong to the given ancestor.");
    }

    return controlPoint;
  }

  /// <summary>Converts a rectangle from control coordinates to an ancestor's control coordinates. If
  /// <paramref name="ancestor"/> is null, the rectangle will be converted to screen coordinates. Otherwise, if the
  /// control does not belong to the given ancestor, an exception will be thrown.
  /// </summary>
  public Rectangle ControlToAncestor(Rectangle controlRect, Control ancestor)
  {
    return new Rectangle(ControlToAncestor(controlRect.Location, ancestor), controlRect.Size);
  }

  /// <summary>Converts a point from control coordinates to the coordinate space of the draw target.</summary>
  public Point ControlToDraw(Point controlPt)
  {
    for(Control control = this; control != null && control.DrawTarget == null; control = control.Parent)
    {
      controlPt = control.ControlToParent(controlPt);
    }
    return controlPt;
  }

  /// <summary>Converts a rectangle from control coordinates to draw coordinates.</summary>
  public Rectangle ControlToDraw(Rectangle windowRect)
  {
    return new Rectangle(ControlToDraw(windowRect.Location), windowRect.Size);
  }

  /// <summary>Converts a point from control coordinates to the parent's control coordinates.</summary>
  public Point ControlToParent(Point controlPoint)
  {
    controlPoint.X += Left;
    controlPoint.Y += Top;
    return controlPoint;
  }

  /// <summary>Converts a rectangle from control coordinates to the parent's control coordinates.</summary>
  public Rectangle ControlToParent(Rectangle windowRect)
  {
    return new Rectangle(ControlToParent(windowRect.Location), windowRect.Size);
  }

  /// <summary>Maps a point from control coordinates to screen coordinates, assuming the root control is a desktop.</summary>
  public Point ControlToScreen(Point controlPoint)
  {
    return ControlToAncestor(controlPoint, null);
  }

  /// <summary>Maps a rectangle from control coordinates to screen coordinates, assuming the root control is a desktop.</summary>
  public Rectangle ControlToScreen(Rectangle controlRect)
  {
    return new Rectangle(ControlToScreen(controlRect.Location), controlRect.Size);
  }

  /// <summary>Converts a point from the coordinate space of the backing surface to control coordinates.</summary>
  public Point DrawToControl(Point backingPt)
  {
    for(Control control = this; control != null && control.DrawTarget == null; control = control.Parent)
    {
      backingPt = control.ParentToControl(backingPt);
    }
    return backingPt;
  }

  /// <summary>Converts a rectangle from draw coordinates to control coordinates.</summary>
  public Rectangle DrawToControl(Rectangle drawRect)
  {
    return new Rectangle(DrawToControl(drawRect.Location), drawRect.Size);
  }

  /// <summary>Converts a point from the parent's control coordinates to this control's coordinates.</summary>
  public Point ParentToControl(Point parentPoint)
  {
    parentPoint.X -= Left;
    parentPoint.Y -= Top;
    return parentPoint;
  }

  /// <summary>Converts a rectangle from the parent's control coordinates to this control's coordinates.</summary>
  public Rectangle ParentToControl(Rectangle parentRect)
  {
    return new Rectangle(ParentToControl(parentRect.Location), parentRect.Size);
  }

  /// <summary>Maps a point from screen coordinates to control coordinates, assuming the root control is a desktop.</summary>
  public Point ScreenToControl(Point screenPoint)
  {
    Control control = this;
    do
    {
      screenPoint = control.ParentToControl(screenPoint);
      control     = control.Parent;
    } while(control != null);
    return screenPoint;
  }

  /// <summary>Maps a rectangle from screen coordinates to control coordinates, assuming the root control is a desktop.</summary>
  public Rectangle ScreenToControl(Rectangle screenRect)
  {
    return new Rectangle(ScreenToControl(screenRect.Location), screenRect.Size);
  }

  /// <summary>Disposes this child and all of its descendants, if they implement <see cref="IDisposable"/>, and clears
  /// the <see cref="Controls"/> collection.
  /// </summary>
  public void DisposeAll()
  {
    DisposeAll(true);
  }

  /// <summary>Disposes this child and all of its descendants, if they implement <see cref="IDisposable"/>, and clears
  /// the <see cref="Controls"/> collection.
  /// </summary>
  /// <param name="disposeThis">If true, this control will be disposed. If false, only the descendants will be. This
  /// should be false when called from a Dispose() method, to avoid an infinite recursion.
  /// </param>
  public void DisposeAll(bool disposeThis)
  {
    foreach(Control child in Controls) child.DisposeAll(true);
    Controls.Clear();

    if(disposeThis)
    {
      IDisposable disposable = this as IDisposable;
      if(disposable != null) disposable.Dispose();
    }
  }

  /// <summary>Returns the first ancestor matching the given type parameter, or null if no matching ancestor is found.</summary>
  public T GetAncestor<T>() where T : Control
  {
    for(Control control = Parent; control != null; control = control.Parent)
    {
      T value = control as T;
      if(value != null) return value;
    }
    return null;
  }

  /// <summary>Returns the topmost control at a given point in control coordinates.</summary>
  /// <param name="controlPoint">The point to consider, in control coordinates.</param>
  /// <returns>The child control at the point specified, or null if none are.</returns>
  /// <remarks>If multiple children overlap at the given point, the one highest in the Z-order will be returned.</remarks>
  public Control GetChildAtPoint(Point controlPoint)
  {
    for(int i=Controls.Count-1; i >= 0; i--)
    {
      Control child = Controls[i];
      if(child.Visible && child.Bounds.Contains(controlPoint)) return child;
    }
    return null;
  }

  /// <summary>Returns the control's content area, in draw coordinates.</summary>
  public Rectangle GetContentDrawRect()
  {
    return ContentOffset.Shrink(GetDrawRect());
  }

  /// <summary>Returns the control's bounds, in draw coordinates.</summary>
  public Rectangle GetDrawRect()
  {
    return new Rectangle(ControlToDraw(Point.Empty), Size);
  }

  /// <summary>Returns the control's bounds, in screen coordinates.</summary>
  public Rectangle GetScreenRect()
  {
    return new Rectangle(ControlToScreen(Point.Empty), Size);
  }

  /// <summary>Sets the <see cref="Bounds"/> property and performs layout logic.</summary>
  /// <param name="x">The new X coordinate of the control.</param>
  /// <param name="y">The new Y coordinate of the control.</param>
  /// <param name="width">The new width of the control.</param>
  /// <param name="height">The new height of the control.</param>
  /// <param name="absolute">Determines whether the coordinates will be affected by layout logic or not.</param>
  /// <remarks>Calling this method is equivalent to calling <see cref="SetBounds(Rectangle,bool)"/> and
  /// using the location and size parameters to create the new rectangle.
  /// See <see cref="SetBounds(Rectangle,bool)"/> for more information on this method.
  /// </remarks>
  public void SetBounds(int x, int y, int width, int height, bool absolute)
  {
    SetBounds(new Rectangle(x, y, width, height), absolute);
  }

  /// <summary>Sets the <see cref="Bounds"/> property and performs layout logic.</summary>
  /// <param name="location">The new location of the control.</param>
  /// <param name="size">The new size of the control.</param>
  /// <param name="absolute">Determines whether the coordinates will be affected by layout logic or not.</param>
  /// <remarks>Calling this method is equivalent to calling <see cref="SetBounds(Rectangle,bool)"/> and
  /// using the <paramref name="location"/> and <paramref name="size"/> parameters to create the new rectangle.
  /// See <see cref="SetBounds(Rectangle,bool)"/> for more information on this method.
  /// </remarks>
  public void SetBounds(Point location, Size size, bool absolute)
  {
    SetBounds(new Rectangle(location, size), absolute);
  }

  /// <summary>Sets the <see cref="Bounds"/> property and performs layout logic.</summary>
  /// <param name="newBounds">The new control boundaries.</param>
  /// <param name="absolute">Determines whether the coordinates will be affected by layout logic or not.</param>
  /// <remarks><para>This method is used to set the bounds of a control and perform operations that need to execute
  /// whenever the bounds change.</para>
  /// <para>If <paramref name="absolute"/> is true, <paramref name="newBounds"/> will be used as-is and no layout
  /// logic will be taken into account. Otherwise, <paramref name="newBounds"/> may be modified by layout code, and
  /// the anchor will be updated to reflect the new position. Note that using absolute coordinates does not prevent
  /// later layout logic from altering the control's position. If you want to place a control without having it
  /// affected by layout logic in the future, give it the <see cref="Forms.ControlStyle.DontLayout"/> control style.</para>
  /// <para>This method is also responsible for calling <see cref="OnLocationChanged"/> and
  /// <see cref="OnSizeChanged"/> as necessary. Overriders should implement their version by calling this base
  /// method.</para>
  /// </remarks>
  public void SetBounds(Rectangle newBounds, bool absolute)
  {
    if(newBounds.Size.Width < 0 || newBounds.Size.Height < 0)
    {
      throw new ArgumentOutOfRangeException("Control size cannot be negative.");
    }

    if(!absolute)
    {
      preLayoutBounds = newBounds;
      anchorSpace.Width  = Math.Max(0, anchorSpace.Width  + newBounds.Width  - Width);
      anchorSpace.Height = Math.Max(0, anchorSpace.Height + newBounds.Height - Height);

      if(Parent != null && !HasStyle(ControlStyle.DontLayout))
      {
        if(Dock != DockStyle.None)
        {
          Parent.TriggerLayout(); // TODO: there's probably a more efficient way to update the layout
        }
        else
        {
          anchorOffsets = new RectOffset(Math.Max(newBounds.Left - Parent.anchorSpace.Left, 0),
                                         Math.Max(newBounds.Top  - Parent.anchorSpace.Top,  0),
                                         Math.Max(Parent.anchorSpace.Right  - newBounds.Right,  0),
                                         Math.Max(Parent.anchorSpace.Bottom - newBounds.Bottom, 0));
          DoAnchor();
          return; // DoAnchor() always calls SetBounds(rect, true), so we can return immediately
        }
      }
    }

    if(newBounds.Location != Location)
    {
      ValueChangedEventArgs<Point> e = new ValueChangedEventArgs<Point>(Location);
      _bounds.Location = newBounds.Location;
      OnLocationChanged(e);
    }

    if(newBounds.Size != Size)
    {
      ValueChangedEventArgs<Size> e = new ValueChangedEventArgs<Size>(Size);
      _bounds.Size = newBounds.Size;
      OnSizeChanged(e);
    }
  }

  /// <summary>Returns the next or previous control in the tab order.</summary>
  /// <param name="reverse">If true, the previous control in the tab order will be returned.</param>
  /// <returns>The next or previous control in the tab order, or null if none were found.</returns>
  /// <remarks>This method uses the <see cref="TabIndex"/> property to order the controls. This method treats the
  /// set of controls as a circular list, so the control after the last one in the tab order is the first again.
  /// </remarks>
  public Control GetNextControl(bool reverse)
  {
    Control next=null, ext=null;
    if(reverse)
    {
      int tabv=0, maxv=int.MinValue, index = SelectedControl == null ? int.MinValue : SelectedControl.TabIndex;
      foreach(Control c in Controls)
      {
        if(!c.Enabled || !c.Visible) continue;
        if(c.TabIndex<index && c.TabIndex>=tabv) { tabv=c.TabIndex; next=c; }
        if(c.TabIndex>-1 && c.TabIndex>maxv) { maxv=c.TabIndex; ext=c; }
      }
    }
    else
    {
      int tabv=int.MaxValue, minv=int.MaxValue, index = SelectedControl == null ? int.MaxValue : SelectedControl.TabIndex;
      foreach(Control c in Controls)
      {
        if(!c.Enabled || !c.Visible) continue;
        if(c.TabIndex>index && c.TabIndex<=tabv) { tabv=c.TabIndex; next=c; }
        if(c.TabIndex>-1 && c.TabIndex<minv) { minv=c.TabIndex; ext=c; }
      }
    }
    return next==null ? ext : next;
  }

  /// <summary>Invalidates the entire control, so that it will be redrawn on the next rendering of the desktop.</summary>
  public void Invalidate() { Invalidate(ControlRect); }

  /// <summary>Invalidates a portion of the control, in control coordinates, so that it will be redrawn on the next
  /// rendering of the desktop.
  /// </summary>
  public void Invalidate(Rectangle dirtyRect)
  {
    dirtyRect.Intersect(ControlRect); // clip the dirty rect to the control surface
    if(dirtyRect.Width == 0) return;

    // if the parent control is visible through the child control, then invalidate that portion of the parent instead
    if(Parent != null && IsTranslucent)
    {
      dirtyRect.Location = ControlToParent(dirtyRect.Location);
      Parent.Invalidate(dirtyRect);
    }
    else if(EffectivelyVisible) // otherwise, no part of the parent should be visible
    {
      AddInvalidatedRegion(dirtyRect);

      // finally, if we need a repaint and don't have one pending, notify the desktop that we need one
      if(invalid.Width != 0 && !HasFlag(Flag.PendingRepaint) && EffectivelyVisible && Desktop != null)
      {
        // if the parent is not going to repaint this area anytime soon, add ourselves to the paint list
        if(Parent == null || !Parent.HasFlag(Flag.PendingRepaint) ||
           !Parent.InvalidRect.Contains(ControlToParent(dirtyRect)))
        {
          SetFlags(Flag.PendingRepaint, true);
          Desktop.NeedsRepaint(this);
        }
      }
    }
  }

  /// <summary>Determines whether this control is an ancestor of the given control. Only strict ancestry is checked, so false
  /// will be returned if the controls are identical.
  /// </summary>
  /// <param name="control">The control whose ancestry will be checked.</param>
  public bool IsAncestorOf(Control control)
  {
    return IsAncestorOf(control, false);
  }

  /// <summary>Determines whether this control is an ancestor of (or optionally identical to) the given control.</summary>
  /// <param name="control">The control whose ancestry will be checked.</param>
  /// <param name="matchIdentity">If true, true will be returned if <paramref name="control"/> is identical to this control.
  /// Otherwise, true will only be returned if this control is strictly an ancestor of <paramref name="control"/>.
  /// </param>
  public bool IsAncestorOf(Control control, bool matchIdentity)
  {
    if(control == null) throw new ArgumentNullException();
    if(!matchIdentity) control = control.Parent;
    for(; control != null; control = control.Parent)
    {
      if(control == this) return true;
    }
    return false;
  }

  /// <summary>Forces this control to lay out its children.</summary>
  /// <param name="recursive">Determines whether the layout will recurse and lay out all descendents.
  /// See <see cref="TriggerLayout(bool)"/> for more information.
  /// </param>
  public void PerformLayout(bool recursive)
  {
    OnLayout(new LayoutEventArgs(recursive));
  }

  /// <summary>Immediately repaints the entire display surface.</summary>
  /// <remarks>Calling this is equivalent to calling <see cref="Refresh(Rectangle)"/> and passing
  /// <see cref="ControlRect"/>.
  /// </remarks>
  public void Refresh() { Refresh(ControlRect); }

  /// <summary>Invalidates the given area and immediately repaints the control.</summary>
  /// <param name="area">The area to invalidate.</param>
  /// <remarks>This method adds the given area to the invalid rectangle (see <see cref="InvalidRect"/>) and
  /// then forces an immediate repaint.
  /// </remarks>
  public void Refresh(Rectangle area)
  {
    if(Parent != null && IsTranslucent)
    {
      Parent.Refresh(ControlToParent(area));
    }
    else
    {
      AddInvalidatedRegion(area);
      Update();
    }
  }

  /// <summary>Triggers a relayout of this control's children.</summary>
  /// <remarks>Calling this method is equivalent to calling <see cref="TriggerLayout(bool)"/> and passing
  /// false to signify a non-recursive layout. See <see cref="TriggerLayout(bool)"/> for more information.
  /// </remarks>
  public void TriggerLayout()
  {
    TriggerLayout(false);
  }

  /// <summary>Triggers a relayout of this control's children or descendants.</summary>
  /// <param name="recursive">If true, a recursive layout should be performed.</param>
  /// <remarks>Generally, recursive layouts are not necessary because the default layout implementation will trickle down to
  /// descendants automatically. However, it can be used to force layouts of all descendants to happen at the same time, if
  /// that's necessary, though doing so can use substantially more CPU time than the default non-recursive, trickle-down
  /// implementation.
  /// </remarks>
  public void TriggerLayout(bool recursive)
  {
    if(!HasFlag(Flag.PendingLayout) && Controls.Count != 0)
    {
      if(Desktop == null)
      {
        SetFlags(Flag.PendingLayout, true);
        SetFlags(Flag.RecursiveLayout, recursive || HasFlag(Flag.RecursiveLayout));
      }
      else if(Parent == null || !Parent.HasFlags(Flag.PendingLayout | Flag.RecursiveLayout))
      {
        Desktop.NeedsLayout(this, recursive);
        SetFlags(Flag.PendingLayout, true);
      }
    }
  }

  /// <summary>Forces an immediate repaint.</summary>
  public void Update()
  {
    if(Desktop != null) Desktop.DoPaint(this);
  }

  /// <summary>This method selects the next control in the tab order.</summary>
  /// <remarks>Calling this method is equivalent to calling <see cref="TabToNextControl(bool)"/> and passing false.</remarks>
  public void TabToNextControl()
  {
    TabToNextControl(false);
  }

  /// <summary>This method selects the next or previous control in the tab order.
  /// <seealso cref="GetNextControl(bool)"/>
  /// </summary>
  /// <param name="reverse">If true, selects the previous control. Otherwise, selects the next control.</param>
  public void TabToNextControl(bool reverse)
  {
    SelectedControl = GetNextControl(reverse);
  }

  /// <summary>Gets or sets the drag threshold for this control.</summary>
  /// <remarks>The drag threshold controls how far the mouse has to be dragged for it to register as a drag event. The
  /// value is stored as the distance in pixels squared, so if a movement of 4 pixels is required to signify a drag,
  /// this property should be set to 16, which is 4 squared. As a special case, the value -1, which is the default,
  /// causes it to use the desktop's <see cref="Forms.Desktop.DefaultDragThreshold"/> property value.
  /// </remarks>
  protected internal int DragThreshold
  {
    get { return _dragThreshold; }
    set
    {
      if(value < -1) throw new ArgumentOutOfRangeException("DragThreshold", value, "must be >=0 or -1");
      _dragThreshold = value;
    }
  }

  protected internal IControlRenderer Renderer
  {
    get; internal set;
  }

  /// <summary>Gets the control's effective font by returning the first font of the control or of an ancestor that is
  /// not null. Since the font is likely shared with other controls, you should set the font's properties (such as the
  /// color) before using it.
  /// </summary>
  protected Font EffectiveFont
  {
    get { return _effectiveFont; }
  }

  protected void AssertParent()
  {
    if(Parent == null) throw new InvalidOperationException("This control has no parent.");
  }

  /// <summary>Gets the rendering target that this control should draw into, or null if the control is not associated
  /// with any render target.
  /// </summary>
  protected internal IGuiRenderTarget GetDrawTarget()
  {
    for(Control control = this; control != null; control = control.Parent)
    {
      if(control.DrawTarget != null) return control.DrawTarget;
    }
    return null;
  }

  /// <summary>Gets the control's effective border color by returning a default color if the border color is not set.</summary>
  protected Color GetEffectiveBorderColor()
  {
    return BorderColor.IsEmpty ? (Color)(Focused ? SystemColors.ActiveBorder : SystemColors.InactiveBorder) : BorderColor;
  }

  /// <summary>Gets the control's effective background color by returning the first color of the control or of an
  /// ancestor that is not fully transparent.
  /// </summary>
  protected Color GetEffectiveBackColor()
  {
    for(Control control = this; control != null; control = control.Parent)
    {
      if(!control.BackColor.IsTransparent) return control.BackColor;
    }
    return Color.Empty;
  }

  /// <summary>Gets the control's effective foreground color by returning the first color of the control or of an
  /// ancestor that is not fully transparent.
  /// </summary>
  protected Color GetEffectiveForeColor()
  {
    for(Control control = this; control != null; control = control.Parent)
    {
      if(!control.ForeColor.IsTransparent) return control.ForeColor;
    }
    return Color.Empty;
  }

  protected virtual void LayOutChildren()
  {
    foreach(Control child in Controls)
    {
      if(!child.HasStyle(ControlStyle.DontLayout))
      {
        switch(child.Dock)
        {
          case DockStyle.Left:
            child.SetBounds(anchorSpace.X, anchorSpace.Y, child.Width, anchorSpace.Height, true);
            anchorSpace.X    += child.Width;
            anchorSpace.Width = Math.Max(0, anchorSpace.Width - child.Width);
            break;

          case DockStyle.Top:
            child.SetBounds(anchorSpace.X, anchorSpace.Y, anchorSpace.Width, child.Height, true);
            anchorSpace.Y     += child.Height;
            anchorSpace.Height = Math.Max(0, anchorSpace.Height - child.Height);
            break;

          case DockStyle.Right:
            child.SetBounds(anchorSpace.Right-child.Width, anchorSpace.Y, child.Width, anchorSpace.Height, true);
            anchorSpace.Width = Math.Max(0, anchorSpace.Width - child.Width);
            break;

          case DockStyle.Bottom:
            child.SetBounds(anchorSpace.X, anchorSpace.Bottom-child.Height, anchorSpace.Width, child.Height, true);
            anchorSpace.Height = Math.Max(0, anchorSpace.Height - child.Height);
            break;

          case DockStyle.Fill:
            child.SetBounds(anchorSpace, true);
            anchorSpace = new Rectangle();
            break;
        }
      }
    }

    foreach(Control child in Controls)
    {
      if(child.Dock == DockStyle.None && !child.HasStyle(ControlStyle.DontLayout)) child.DoAnchor();
    }
  }

  /// <summary>Fills the background with the background color if it is not fully transparent.</summary>
  protected virtual void PaintBackgroundColor(PaintEventArgs e)
  {
    e.Renderer.DrawBackgroundColor(this, e, BackColor);
  }

  /// <summary>Paints the background image if it has been set.</summary>
  protected virtual void PaintBackgroundImage(PaintEventArgs e)
  {
    e.Renderer.DrawBackgroundImage(this, e, BackImage, BackImageAlign);
  }

  /// <summary>Paints the border if the control has one.</summary>
  protected virtual void PaintBorder(PaintEventArgs e)
  {
    e.Renderer.DrawBorder(this, e, BorderStyle, GetEffectiveBorderColor());
  }

  /// <summary>Sets the control's modal status.</summary>
  /// <param name="modal">Makes the control modal if true, and nonmodal if false.</param>
  /// <remarks>After calling this with <paramref name="modal"/> set to true, the control will be the topmost modal
  /// control, even if it was previously modal.
  /// </remarks>
  protected void SetModal(bool modal)
  {
    if(Desktop == null) throw new InvalidOperationException("This control has no desktop");
    if(modal) Desktop.SetModal(this);
    else Desktop.UnsetModal(this);
  }

  /// <summary>Determines whether the given child can be placed inside this control.</summary>
  protected virtual void ValidateNewChild(Control newChild) { }

  /// <summary>Determines whether this control can be placed inside the given parent control.</summary>
  protected virtual void ValidateNewParent(Control newParent) { }

  #region Flag
  internal enum Flag
  {
    /// <summary>The mask that should be applied to <see cref="flags"/> to retrieve the control style.</summary>
    ControlStyleMask = 0xFF,
    /// <summary>Determines whether the control will respond to user input.</summary>
    Enabled = 0x100,
    /// <summary>Determines whether the control and its children will be rendered.</summary>
    Visible = 0x200,
    /// <summary>Determines whether we've requested a repaint already.</summary>
    PendingRepaint = 0x400,
    /// <summary>Determines whether the control will have a chance to process keyboard events before its children.</summary>
    KeyPreview = 0x800,
    /// <summary>Indicates whether the control and all of its ancestors are enabled.</summary>
    EffectivelyEnabled = 0x1000,
    /// <summary>Indicates whether the control and all of its ancestors are visible.</summary>
    EffectivelyVisible = 0x2000,
    /// <summary>Indicates whether the control is modal, and so will receive all input events.</summary>
    Modal = 0x4000,
    /// <summary>Indicates whether the control and all of its ancestors are selected.</summary>
    Active = 0x8000,
    /// <summary>Indicates whether the control has keyboard focus.</summary>
    Focused=0x10000,
    /// <summary>Indicates that the change currently occurring was triggered by the control itself and not external code.</summary>
    MyChange = 0x20000,
    /// <summary>Indicates that when the parent is changed, the only processing that should occur is the processing
    /// to set the the parent field. This is used while rearranging controls, to prevent the processing from occurring
    /// more than we want.
    /// </summary>
    JustSetParent = 0x40000,
    /// <summary>Indicates that the control should be laid out.</summary>
    PendingLayout = 0x80000,
    /// <summary>Indicates that the next layout will be recursive.</summary>
    RecursiveLayout = 0x100000,
  }
  #endregion

  /// <summary>Gets the render target directly associated with this control, or null if the control is inheriting the
  /// draw target of its parent. To get the actual render target that will be used for drawing, use the
  /// <see cref="GetDrawTarget"/> method.
  /// </summary>
  internal IGuiRenderTarget DrawTarget
  {
    get; set;
  }

  /// <summary>Gets the portion of this control that needs to be redrawn.</summary>
  internal Rectangle InvalidRect
  {
    get { return invalid; }
  }

  /// <summary>Determines whether this control is translucent, or is drawn onto a translucent draw target.</summary>
  internal bool IsTranslucent
  {
    get
    {
      if(!BackColor.IsOpaque) return true;

      if(Renderer != null)
      {
        IGuiRenderTarget drawTarget = GetDrawTarget();
        return drawTarget != null && Renderer.IsTranslucent(drawTarget);
      }

      return false;
    }
  }

  /// <summary>Paints this control and its children, and raises paint-related events.</summary>
  internal void DoPaint(PaintEventArgs e)
  {
    if(e.Target != null)
    {
      Rectangle oldClipRect = e.Target.ClipRect;
      try
      {
        e.Target.ClipRect = e.DrawRect;
        OnPaintBackground(e);
        if(PaintBackground != null) PaintBackground(this, e);
        OnPaint(e);
        if(Paint != null) Paint(this, e);
      }
      finally { e.Target.ClipRect = oldClipRect; }

      foreach(Control child in Controls)
      {
        if(child.Visible)
        {
          Rectangle paint = Rectangle.Intersect(child.Bounds, e.ControlRect);
          if(paint.Width != 0)
          {
            paint.Location = child.ParentToControl(paint.Location);
            child.AddInvalidatedRegion(paint);
            Desktop.DoPaint(child, paint);
          }
        }
      }
    }
  }

  internal bool HasFlag(Flag flag)
  {
    return (flags & flag) != 0;
  }

  internal bool HasFlags(Flag flags)
  {
    return (this.flags & flags) == flags;
  }

  internal void SetFlags(Flag flags, bool on)
  {
    if(on) this.flags |= flags;
    else this.flags &= ~flags;
  }

  internal bool HasStyle(ControlStyle style)
  {
    return HasFlag((Flag)style);
  }

  internal void SetRenderer(IControlRenderer renderer)
  {
    if(renderer != this.Renderer)
    {
      this.Renderer = renderer;
      if(renderer != null) OnContentOffsetChanged(); // the renderer may affect border sizes, etc
    }
  }

  /// <summary>Called when the control's <see cref="DrawTarget"/> may need to be updated. The default implementation
  /// just propogates the call if <paramref name="recursive"/> is true. The actual logic to update the draw target will
  /// have to be implemented in a derived class, which should call the base implementation at the end of the derived
  /// version.
  /// </summary>
  internal void UpdateDrawTarget(bool forceRecreate, bool recursive)
  {
    if(this != Desktop &&
       (forceRecreate || // if we're being forced to update it
        !HasStyle(ControlStyle.CustomDrawTarget) && DrawTarget != null || // or we don't need our backing surface anymore
        HasStyle(ControlStyle.CustomDrawTarget) && (DrawTarget == null || Size != DrawTarget.Size))) // or we need one and ours is invalid
    {
      IGuiRenderTarget oldTarget = DrawTarget;
      Utility.Dispose(DrawTarget);
      DrawTarget = null;

      if(HasStyle(ControlStyle.CustomDrawTarget) && Renderer != null)
      {
        DrawTarget = Renderer.CreateDrawTarget(this);
        forceRecreate = recursive = true;
      }

      if(oldTarget != DrawTarget) OnDrawTargetChanged();
    }

    if(recursive)
    {
      foreach(Control child in Controls) child.UpdateDrawTarget(forceRecreate, true);
    }
  }

  void AddInvalidatedRegion(Rectangle dirtyRect)
  {
    if(invalid.Width == 0) invalid = dirtyRect; // set the invalid rect if it's empty
    else invalid = Rectangle.Union(invalid, dirtyRect); // otherwise union it with the dirty rect
  }

  void DoAnchor()
  {
    Rectangle newBounds = preLayoutBounds, available = Parent.anchorSpace;

    if((Anchor & AnchorStyle.LeftRight) == AnchorStyle.LeftRight) // LeftRight
    {
      newBounds.X     = available.X     + anchorOffsets.Left;
      newBounds.Width = available.Width - anchorOffsets.Horizontal;
    }
    else if((Anchor & AnchorStyle.Right) != 0) // Right
    {
      newBounds.X = available.Right - anchorOffsets.Right - newBounds.Width;
    }
    else
    {
      newBounds.X = available.X + anchorOffsets.Left; // Left (default)
    }

    if((Anchor & AnchorStyle.TopBottom) == AnchorStyle.TopBottom) // TopBottom
    {
      newBounds.Y      = available.Y      + anchorOffsets.Top;
      newBounds.Height = available.Height - anchorOffsets.Vertical;
    }
    else if((Anchor & AnchorStyle.Bottom) != 0) // Bottom
    {
      newBounds.Y = available.Bottom - anchorOffsets.Bottom - newBounds.Height;
    }
    else
    {
      newBounds.Y = available.Y + anchorOffsets.Top; // Top (default)
    }

    SetBounds(newBounds, true);
  }

  void RecursivelyActivate(Control control)
  {
    Control parent = this;
    do
    {
      bool active = parent.HasFlag(Flag.Active);
      control.SetFlags(Flag.Active, active);
      control.OnSelected();
      if(active) control.OnActivated();
      parent  = control;
      control = control.SelectedControl;
    } while(control != null);
  }

  void RecursivelyDeactivate(Control control)
  {
    if(control.SelectedControl != null) RecursivelyDeactivate(control.SelectedControl);

    control.Blur();
    if(control.HasFlag(Flag.Active))
    {
      control.SetFlags(Flag.Active, false);
      control.OnDeactivated();
    }
  }

  void RecursivelySetEffectiveValues()
  {
    SetFlags(Flag.EffectivelyEnabled, Enabled && (Parent == null || Parent.EffectivelyEnabled));
    SetFlags(Flag.EffectivelyVisible, Visible && (Parent == null || Parent.EffectivelyVisible));

    if(Parent != null)
    {
      bool hadNoDesktop = Desktop == null;
      Desktop = Parent.Desktop;
      SetRenderer(Parent.Renderer);

      Font newFont = Font ?? Parent.EffectiveFont;
      if(newFont != EffectiveFont)
      {
        ValueChangedEventArgs<Font> e = new ValueChangedEventArgs<Font>(EffectiveFont);
        _effectiveFont = newFont;
        OnEffectiveFontChanged(e);
      }

      if(hadNoDesktop && Desktop != null && HasFlag(Flag.PendingLayout))
      {
        Desktop.NeedsLayout(this, HasFlag(Flag.RecursiveLayout));
      }
    }
    else
    {
      Desktop        = null;
      _effectiveFont = Font;
      SetRenderer(null);
    }

    foreach(Control child in Controls) child.RecursivelySetEffectiveValues();
  }

  void RecursivelySetEnabled()
  {
    bool enabled = Enabled && (Parent == null || Parent.EffectivelyEnabled);
    SetFlags(Flag.EffectivelyEnabled, enabled);
    if(!enabled) Blur();
    foreach(Control child in Controls) child.RecursivelySetEnabled();
  }

  void RecursivelySetFont()
  {
    Font newFont = Font ?? (Parent == null ? null : Parent.EffectiveFont);
    if(newFont != EffectiveFont)
    {
      ValueChangedEventArgs<Font> e = new ValueChangedEventArgs<Font>(EffectiveFont);
      _effectiveFont = newFont;
      OnEffectiveFontChanged(e);

      foreach(Control child in Controls) child.RecursivelySetFont();
    }
  }

  void RecursivelySetVisible()
  {
    bool visible = Visible && (Parent == null || Parent.EffectivelyVisible);
    SetFlags(Flag.EffectivelyVisible, visible);
    if(!visible) Blur();
    foreach(Control child in Controls) child.RecursivelySetVisible();
  }

  void SetParent(Control control)
  {
    if(HasFlag(Flag.JustSetParent)) // if we're manipulating the ControlCollection
    {                               // and don't want it performing all these actions...
      _parent = control; // just set the parent
    }
    else // otherwise, do everything
    {
      Control oldParent = Parent;
      ControlEventArgs ce = new ControlEventArgs(this);

      if(oldParent != null)
      {
        if(Desktop != null)
        {
          if(Desktop.IsCapturing(this) || Desktop.CapturingControl != null && Controls.Contains(Desktop.CapturingControl, true))
          {
            Desktop.SetCapture(null);
          }
          Desktop.UnsetModal(this);
        }
        oldParent.OnControlRemoved(ce);
      }

      _parent = control;
      RecursivelySetEffectiveValues();
      if(Parent != null)
      {
        Parent.OnControlAdded(ce);
        if(oldParent == null) TriggerLayout();
      }
      if(!HasFlag(Flag.MyChange)) OnParentChanged(new ValueChangedEventArgs<Control>(oldParent));
    }
  }

  /// <summary>The object in which this GUI control is contained, or NULL if it is the root of a control hierarchy.</summary>
  Control _parent;

  /// <summary>The selected child.</summary>
  Control _selected;

  Font _font, _effectiveFont;

  string _name = string.Empty, _text = string.Empty;

  /// <summary>The position and size of the control within its parent, in pixels.</summary>
  Rectangle _bounds = new Rectangle(0, 0, 100, 100);

  /// <summary>The area of this control that needs repainting.</summary>
  Rectangle invalid;

  /// <summary>The area of the control available for anchored controls.</summary>
  Rectangle anchorSpace = new Rectangle(0, 0, 100, 100);

  /// <summary>The position and size of the control within its parent, before considering layout logic.</summary>
  Rectangle preLayoutBounds;

  /// <summary>The distances from the edges of the parent control, at which this control is anchored.</summary>
  RectOffset anchorOffsets;

  RectOffset _padding;

  Color _backColor, _color, _borderColor;

  IGuiImage _backImage;

  int _dragThreshold = -1, _tabIndex = -1;
  internal uint lastClickTime;
  Flag flags = Flag.Enabled | Flag.Visible | Flag.EffectivelyEnabled | Flag.EffectivelyVisible;

  AnchorStyle _anchor = AnchorStyle.TopLeft;
  DockStyle _dock;
  ContentAlignment _backImageAlign = ContentAlignment.TopLeft;
  BorderStyle _borderStyle;
}

} // namespace GameLib.Forms