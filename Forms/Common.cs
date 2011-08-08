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

#warning Document everything!

using System;
using System.Drawing;
using GameLib.Forms;
using GameLib.Events;
using GameLib.Video;

namespace GameLib.Forms
{

#region AnchorStyle
/// <summary>
/// This enum specifies where the control will be anchored in relation to its parent control. See the
/// <see cref="Control.Anchor"/> property for more information. The members can be ORed together to combine their
/// effects.
/// </summary>
[Flags]
public enum AnchorStyle
{
  /// <summary>The control will not be anchored. This value is typically only used when <see cref="Control.Dock"/>
  /// is used. If <see cref="Control.Dock"/> is set to <see cref="DockStyle.None"/>, this value will be treated the
  /// same as <see cref="TopLeft"/>.
  /// </summary>
  None = 0,
  /// <summary>The control will be anchored to its parent's left edge.</summary>
  Left = 1,
  /// <summary>The control will be anchored to its parent's top edge.</summary>
  Top = 2,
  /// <summary>The control will be anchored to its parent's right edge.</summary>
  Right = 4,
  /// <summary>The control will be anchored to its parent's bottom edge.</summary>
  Bottom = 8,
  /// <summary>The control will be anchored to its parent's top left corner.</summary>
  TopLeft = Top | Left,
  /// <summary>The control will be anchored to its parent's top right corner.</summary>
  TopRight = Top | Right,
  /// <summary>The control will be anchored to its parent's bottom left corner.</summary>
  BottomLeft = Bottom | Left,
  /// <summary>The control will be anchored to its parent's bottom right corner.</summary>
  BottomRight = Bottom | Right,
  /// <summary>The control will be anchored to its parent's left and right sides.</summary>
  LeftRight = Left | Right,
  /// <summary>The control will be anchored to its parent's top and bottom edges.</summary>
  TopBottom = Top | Bottom,
  /// <summary>The control will be anchored to all four corners of its parent.</summary>
  All = TopLeft | BottomRight
}
#endregion

#region BorderStyle
/// <summary>Common border styles.</summary>
[Flags]
public enum BorderStyle
{
  /// <summary>No border.</summary>
  None = 0,
  /// <summary>A solid-color border.</summary>
  FixedFlat = 1,
  /// <summary>A border composed of two colors, used to give the appearance of light hitting a 3D object at an angle.
  /// </summary>
  Fixed3D = 2,
  /// <summary>A thick border with a 3D appearance.</summary>
  FixedThick = 3,
  /// <summary>A border that signifies that the control can be resized by dragging its edges.</summary>
  Resizeable = 4,
  /// <summary>A mask that selects the type of the border (eg, flat, 3d, thick, etc).</summary>
  TypeMask = 7,

  /// <summary>A flag that indicates that the border is depressed rather than raised.</summary>
  Depressed = 8,
};
#endregion

#region ControlStyle
/// <summary>
/// This enum is generally used by derived controls to control how they will be treated by the
/// <see cref="Desktop" />. The enumeration members can be ORed together to combine their effects.
/// </summary>
[Flags]
public enum ControlStyle
{
  /// <summary>The control may not receive click, double-click, or drag events, and cannot receive focus.</summary>
  None = 0,
  /// <summary>The control may receive click events.</summary>
  Clickable = 0x1,
  /// <summary>The control may receive double-click events. Without this flag, if the user double-clicks, the
  ///   control will receive two click events.
  /// </summary>
  DoubleClickable = 0x2,
  /// <summary>The control may receive drag events. Without this flag, a drag may be interpreted as a click.</summary>
  Draggable = 0x4,
  /// <summary>The control may receive keyboard focus.</summary>
  CanReceiveFocus = 0x8,
  /// <summary>This flag is the same as specifying <c>Clickable</c> and <c>DoubleClickable</c>.</summary>
  NormalClick = Clickable | DoubleClickable,
  /// <summary>This flag is the same as specifying <c>Clickable</c>, <c>DoubleClickable</c>, and <c>Draggable</c>.
  /// </summary>
  Anyclick = NormalClick | Draggable,
  /// <summary>This flag indicates that the control should be skipped by layout code.</summary>
  DontLayout = 0x10,
  /// <summary>This flag indicates that the control needs its own draw target.</summary>
  CustomDrawTarget = 0x20,
  // NOTE: if new control styles are added, Control.Flags should be updated
}
#endregion

#region DockStyle
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
  Bottom,
  /// <summary>The control will fill its parent's control area.</summary>
  Fill
}
#endregion

#region Orientation
public enum Orientation
{
  Horizontal, Vertical
}
#endregion

#region RectOffset
/// <summary>This structure represents a rectangular set of offsets, one offset for each of the four sides. It
/// can be used to easily grow and shrink <see cref="Rectangle"/> objects.
/// </summary>
public struct RectOffset
{
  /// <summary>This constructor sets all four offsets to a single value.</summary>
  /// <param name="size">The value to use for all four offsets.</param>
  public RectOffset(int size) { Left = Top = Right = Bottom = size; }
  /// <summary>This constructor sets the sets the left and right offsets to one value, and the top and bottom
  /// offsets to another value.
  /// </summary>
  /// <param name="width">The value to use for the <see cref="Left"/> and <see cref="Right"/> offsets.</param>
  /// <param name="height">The value to use for the <see cref="Top"/> and <see cref="Bottom"/> offsets.</param>
  public RectOffset(int width, int height) { Left = Right = width; Top = Bottom = height; }
  /// <summary>This constructor sets all four offsets individually.</summary>
  /// <param name="left">The value to use for the <see cref="Left"/> offset.</param>
  /// <param name="top">The value to use for the <see cref="Top"/> offset.</param>
  /// <param name="right">The value to use for the <see cref="Right"/> offset.</param>
  /// <param name="bottom">The value to use for the <see cref="Bottom"/> offset.</param>
  public RectOffset(int left, int top, int right, int bottom) { Left = left; Top = top; Right = right; Bottom = bottom; }
  /// <summary>This constructor sets the offsets based on the difference between two rectangles.</summary>
  /// <param name="outer">The outer <see cref="Rectangle"/>.</param>
  /// <param name="inner">The inner <see cref="Rectangle"/>.</param>
  /// <remarks>
  /// The offsets will be calculated in such a way that if this object is used to <see cref="Shrink(Rectangle)"/> the outer
  /// rectangle, the result will be the inner rectangle, and if you <see cref="Grow(Rectangle)"/> the inner rectangle, the
  /// result will be the outer rectangle.
  /// </remarks>
  public RectOffset(Rectangle outer, Rectangle inner)
  {
    Left = inner.X - outer.X;
    Top = inner.Y - outer.Y;
    Right = outer.Right - inner.Right;
    Bottom = outer.Bottom - inner.Bottom;
  }

  /// <summary>Gets the total horizontal offset.</summary>
  /// <value>Returns the total horizontal offset, which is equal to <see cref="Right"/> + <see cref="Left"/>.</value>
  /// <remarks>The total horizontal offset is the amount a given rectangle will be changed in the horizontal
  /// direction if this object is applied to it.
  /// </remarks>
  public int Horizontal { get { return Right + Left; } }
  /// <summary>Gets the total vertical offset.</summary>
  /// <value>Returns the total vertical offset, which is equal to <see cref="Top"/> + <see cref="Bottom"/>.</value>
  /// <remarks>The total vertical offset is the amount a given rectangle will be changed in the vertical
  /// direction if this object is applied to it.
  /// </remarks>
  public int Vertical { get { return Top + Bottom; } }
  /// <summary>Gets the top and left offsets as a <see cref="Size"/> object.</summary>
  /// <value>A <see cref="Size"/> object containing the <see cref="Left"/> and <see cref="Top"/> offsets as
  /// its <see cref="Size.Width"/> and <see cref="Size.Height"/> respectively.
  /// </value>
  public Size TopLeft { get { return new Size(Left, Top); } }
  /// <summary>Gets the bottom and right offsets as a <see cref="Size"/> object.</summary>
  /// <value>A <see cref="Size"/> object containing the <see cref="Right"/> and <see cref="Bottom"/> offsets as
  /// its <see cref="Size.Width"/> and <see cref="Size.Height"/> respectively.
  /// </value>
  public Size BottomRight { get { return new Size(Right, Bottom); } }

  public override bool Equals(object obj)
  {
    return obj is RectOffset ? this == (RectOffset)obj : false;
  }

  public override int GetHashCode()
  {
    return Left ^ Top ^ Right ^ Bottom;
  }

  /// <summary>Adds a given amount to each offset.</summary>
  /// <param name="amount">The amount to add to the <see cref="Left"/>, <see cref="Top"/>, <see cref="Right"/>,
  /// and <see cref="Bottom"/> offsets.
  /// </param>
  /// <remarks><paramref name="amount"/> can be negative if you want to decrease the offsets.</remarks>
  public void Offset(int amount)
  {
    Left += amount; Top += amount; Right += amount; Bottom += amount;
  }

  /// <summary>Adds a given amount to each offset.</summary>
  public void Offset(Size amount)
  {
    Left += amount.Width; Top += amount.Height; Right += amount.Width; Bottom += amount.Height;
  }

  /// <summary>Adds a given amount to each offset.</summary>
  /// <param name="horizontal">The amount to add to the <see cref="Left"/> and <see cref="Right"/> offsets.</param>
  /// <param name="vertical">The amount to add to the <see cref="Top"/> and <see cref="Bottom"/> offsets.</param>
  /// <remarks><paramref name="horizontal"/> and <paramref name="vertical"/> can be negative if you want to
  /// decrease the offsets.
  /// </remarks>
  public void Offset(int horizontal, int vertical)
  {
    Left += horizontal; Right += horizontal; Top += vertical; Bottom += vertical;
  }
  /// <summary>Adds a given amount to each offset.</summary>
  /// <param name="left">The amount to add to the <see cref="Left"/> offset.</param>
  /// <param name="top">The amount to add to the <see cref="Top"/> offset.</param>
  /// <param name="right">The amount to add to the <see cref="Right"/> offset.</param>
  /// <param name="bottom">The amount to add to the <see cref="Bottom"/> offset.</param>
  /// <remarks>The parameters can be negative if you want to decrease the offsets.</remarks>
  public void Offset(int left, int top, int right, int bottom)
  {
    Left += left; Right += right; Top += top; Bottom += bottom;
  }

  /// <summary>Enlarges a rectangle.</summary>
  /// <param name="rect">The <see cref="Rectangle"/> to enlarge.</param>
  /// <returns>A <see cref="Rectangle"/> that has been enlarged by the offsets in this object. Note that if
  /// the offsets are negative, the resulting rectangle may actually shrink.
  /// </returns>
  public Rectangle Grow(Rectangle rect)
  {
    rect.X -= Left;
    rect.Width += Right + Left;
    rect.Y -= Top;
    rect.Height -= Top + Bottom;
    return rect;
  }
  /// <summary>Enlarges a <see cref="Size"/> object.</summary>
  /// <param name="size">The <see cref="Size"/> to enlarge.</param>
  /// <returns>A <see cref="Size"/> that has been enlarged by the <see cref="Horizontal"/> and <see cref="Vertical"/>
  /// offsets in this object. Note that if the offsets are negative, the resulting size may actually shink.
  /// </returns>
  public Size Grow(Size size)
  {
    size.Width += Left + Right;
    size.Height += Top + Bottom;
    return size;
  }

  /// <summary>Shrinks a rectangle.</summary>
  /// <param name="rect">The <see cref="Rectangle"/> to shrink.</param>
  /// <returns>A <see cref="Rectangle"/> that has been shrunk by the offsets in this object. Note that if
  /// the offsets are negative, the resulting rectangle may actually grow.
  /// </returns>
  public Rectangle Shrink(Rectangle rect)
  {
    rect.X += Left;
    rect.Width -= Right + Left;
    rect.Y += Top;
    rect.Height -= Top + Bottom;
    return rect;
  }
  /// <summary>Shrinks a <see cref="Size"/> object.</summary>
  /// <param name="size">The <see cref="Size"/> to shrink.</param>
  /// <returns>A <see cref="Size"/> that has been shrunk by the <see cref="Horizontal"/> and <see cref="Vertical"/>
  /// offsets in this object. Note that if the offsets are negative, the resulting size may actually grow.
  /// </returns>
  public Size Shrink(Size size)
  {
    size.Width -= Left + Right;
    size.Height -= Top + Bottom;
    return size;
  }

  public override string ToString()
  {
    return string.Format("{{Left={0} Top={1} Right={2} Bottom={3}}}", Left, Top, Right, Bottom);
  }

  /// <summary>The offset that will be applied to the left side of a rectangle.</summary>
  public int Left;
  /// <summary>The offset that will be applied to the top side of a rectangle.</summary>
  public int Top;
  /// <summary>The offset that will be applied to the right side of a rectangle.</summary>
  public int Right;
  /// <summary>The offset that will be applied to the bottom side of a rectangle.</summary>
  public int Bottom;

  /// <summary>This operator grows a <see cref="Rectangle"/> by a <see cref="RectOffset"/>.</summary>
  /// <param name="lhs">The <see cref="Rectangle"/> to enlarge.</param>
  /// <param name="rhs">The <see cref="RectOffset"/> to enlarge by.</param>
  /// <returns>Returns the result of calling <see cref="Grow(Rectangle)"/> on <paramref name="rhs"/> and
  /// passing <paramref name="lhs"/>.
  /// </returns>
  public static Rectangle operator +(Rectangle lhs, RectOffset rhs) { return rhs.Grow(lhs); }
  /// <summary>This operator shrinks a <see cref="Rectangle"/> by a <see cref="RectOffset"/>.</summary>
  /// <param name="lhs">The <see cref="Rectangle"/> to shrink.</param>
  /// <param name="rhs">The <see cref="RectOffset"/> to shrink by.</param>
  /// <returns>Returns the result of calling <see cref="Shrink(Rectangle)"/> on <paramref name="rhs"/> and
  /// passing <paramref name="lhs"/>.
  /// </returns>
  public static Rectangle operator -(Rectangle lhs, RectOffset rhs) { return rhs.Shrink(lhs); }

  /// <summary>This operator compares two <see cref="RectOffset"/> objects.</summary>
  /// <param name="a">The first <see cref="RectOffset"/> object to compare.</param>
  /// <param name="b">The second <see cref="RectOffset"/> object to compare.</param>
  /// <returns>Returns true if the two <see cref="RectOffset"/> objects are equal, and false otherwise.</returns>
  public static bool operator ==(RectOffset a, RectOffset b)
  {
    return a.Left == b.Left && a.Top == b.Top && a.Right == b.Right && a.Bottom == b.Bottom;
  }
  /// <summary>This operator compares two <see cref="RectOffset"/> objects.</summary>
  /// <param name="a">The first <see cref="RectOffset"/> object to compare.</param>
  /// <param name="b">The second <see cref="RectOffset"/> object to compare.</param>
  /// <returns>Returns true if the two <see cref="RectOffset"/> objects are unequal, and false otherwise.</returns>
  public static bool operator !=(RectOffset a, RectOffset b)
  {
    return a.Left != b.Left || a.Top != b.Top || a.Right != b.Right || a.Bottom != b.Bottom;
  }

  /// <summary>A <see cref="RectOffset"/> value with all fields set to zero.</summary>
  public static readonly RectOffset Empty = new RectOffset();
}
#endregion

#region ClickEventArgs
/// <summary>This class is used in mouse button events.</summary>
public class ClickEventArgs : EventArgs
{
  /// <summary>This constructor creates an instance with all click information initialized to zero. You should
  /// fill in the <see cref="CE"/> field yourself before using this instance.
  /// </summary>
  public ClickEventArgs() { CE = new MouseClickEvent(); }
  /// <summary>This constructor creates an instance with click information populated from <c>ce</c>.</summary>
  /// <param name="ce">This reference is stored in the <see cref="CE"/> field.</param>
  public ClickEventArgs(MouseClickEvent ce) { CE = ce; }
  /// <summary>This field holds the mouse click information. Fields referring to the location where the
  /// click occurred will be automatically converted to control coordinates when the event handler is called.
  /// </summary>
  public MouseClickEvent CE;
  /// <summary>This is set to true within an event handler to indicate that the event has been handled and should
  /// not be propogated to other event handling code.
  /// </summary>
  public bool Handled;
}
#endregion

#region ControlEventArgs
/// <summary>This class is used in events that refer to a control.</summary>
public class ControlEventArgs : EventArgs
{
  public ControlEventArgs(Control control) { Control = control; }
  /// <summary>This member holds the control associated with this event.</summary>
  public Control Control;
}
#endregion

#region DragEventArgs
/// <summary>This class is used in events that refer to the mouse cursor being dragged over a control.</summary>
public class DragEventArgs : EventArgs
{
  /// <summary>This method checks whether only the specified button was depressed at the time the drag was started.
  /// </summary>
  /// <param name="button">The mouse button to check for</param>
  /// <returns>Returns true if only the specified button was depressed at the time the drag was started.</returns>
  public bool OnlyPressed(Input.MouseButton button) { return Buttons == (1 << (byte)button); }
  /// <summary>This method checks whether the specified button was depressed at the time the drag was started.
  /// </summary>
  /// <param name="button">The mouse button to check for</param>
  /// <returns>Returns true if the specified button was depressed at the time the drag was started.</returns>
  public bool Pressed(Input.MouseButton button) { return (Buttons & (1 << (byte)button)) != 0; }
  /// <summary>
  /// This method sets or clears a bit in the <see cref="Buttons"/> field corresponding to the specified button.
  /// </summary>
  /// <param name="button">The mouse button to set</param>
  /// <param name="down">The button bit will be set if this is true and cleared if false.</param>
  public void SetPressed(Input.MouseButton button, bool down)
  {
    if(down) Buttons |= (byte)(1 << (byte)button); else Buttons &= (byte)~(1 << (byte)button);
  }
  /// <summary>This property returns a rectangle that represents the area through which the mouse was dragged.
  /// </summary>
  public Rectangle Rectangle
  {
    get
    {
      int x = Math.Min(Start.X, End.X), x2 = Math.Max(Start.X, End.X);
      int y = Math.Min(Start.Y, End.Y), y2 = Math.Max(Start.Y, End.Y);
      return new Rectangle(x, y, x2 - x + 1, y2 - y + 1);
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
#endregion

#region KeyEventArgs
/// <summary>This class is used in keyboard events.</summary>
public class KeyEventArgs : EventArgs
{
  /// <param name="ke">The internal GameLib event which will be stored into <see cref="KE"/>.</param>
  public KeyEventArgs(KeyboardEvent ke) { KE = ke; }
  /// <summary>The internal GameLib event which contains information about the key.</summary>
  public KeyboardEvent KE;
  /// <summary>This is set to true within an event handler to indicate that the event has been handled and should
  /// not be propogated to other event handling code.
  /// </summary>
  public bool Handled;
}
#endregion

#region LayoutEventArgs
/// <summary>This class is used in layout events.</summary>
public class LayoutEventArgs : EventArgs
{
  /// <summary>This constructor sets <see cref="Recursive"/> to <paramref name="recursive"/></summary>
  /// <param name="recursive">If true, a recursive layout should be performed.</param>
  public LayoutEventArgs(bool recursive) { Recursive = recursive; }

  /// <summary>If true, a recursive layout should be performed.</summary>
  public bool Recursive;
}
#endregion

#region MouseMoveEventHandler
/// <summary>This delegate is used along with <see cref="MouseMoveEvent"/> to service mouse move events.
/// Information in <c>e</c> relating to the location of the mouse movement will be automatically converted
/// to control coordinates when the event handler is called.
/// </summary>
public delegate void MouseMoveEventHandler(object sender, MouseMoveEvent e);
#endregion

#region PaintEventArgs
/// <summary>
/// This class is used in events that draw the control to the screen.
/// </summary>
public class PaintEventArgs : EventArgs
{
  /// <summary>Initializes a new <see cref="PaintEventArgs"/> object.</summary>
  /// <param name="renderer">The renderer used to paint the control.</param>
  /// <param name="control">The control that is to be painted.</param>
  /// <param name="controlRect">The area within the control to be painted, in control coordinates.
  /// The rectangle will be clipped against the render target's bounds.
  /// </param>
  public PaintEventArgs(IControlRenderer renderer, Control control, Rectangle controlRect)
  {
    if(renderer == null || control == null) throw new ArgumentNullException();

    Renderer = renderer;
    Target   = control.GetDrawTarget();
    DrawRect = control.ControlToDraw(controlRect);

    if(Target != null)
    {
      DrawRect.Intersect(new Rectangle(Point.Empty, Target.Size));
      ControlRect = control.DrawToControl(DrawRect);
    }
    else
    {
      ControlRect = controlRect;
    }
  }

  /// <summary>The renderer that should be used to render the control.</summary>
  public IControlRenderer Renderer;
  /// <summary>The surface upon which the control should be painted.</summary>
  public IGuiRenderTarget Target;
  /// <summary>This rectangle holds the area to be painted, in control coordinates.</summary>
  public Rectangle ControlRect;
  /// <summary>This rectangle holds the area to be painted, in display coordinates within <see cref="Target"/>.</summary>
  public Rectangle DrawRect;
}
#endregion

} // namespace GameLib.Forms