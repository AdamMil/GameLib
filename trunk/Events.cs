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
using GameLib.Input;
using GameLib.Interop.SDL;

namespace GameLib.Events
{

#region Enums
/// <summary>This enum holds values specifying various types of events.</summary>
[Flags]
public enum EventType
{ 
  /// <summary>A <see cref="FocusEvent"/> event.</summary>
  Focus,
  /// <summary>A <see cref="KeyboardEvent"/> event.</summary>
  Keyboard,
  /// <summary>A <see cref="MouseMoveEvent"/> event.</summary>
  MouseMove,
  /// <summary>A <see cref="MouseClickEvent"/> event.</summary>
  MouseClick,
  /// <summary>A <see cref="JoyMoveEvent"/> event.</summary>
  JoyMove,
  /// <summary>A <see cref="JoyBallEvent"/> event.</summary>
  JoyBall,
  /// <summary>A <see cref="JoyHatEvent"/> event.</summary>
  JoyHat,
  /// <summary>A <see cref="JoyButtonEvent"/> event.</summary>
  JoyButton,
  /// <summary>A <see cref="QuitEvent"/> event.</summary>
  Quit,
  /// <summary>A <see cref="ResizeEvent"/> event.</summary>
  Resize,
  /// <summary>A <see cref="RepaintEvent"/> event.</summary>
  Repaint,
  /// <summary>An <see cref="ExceptionEvent"/> event.</summary>
  Exception,
  /// <summary>An event derived from <see cref="WindowEvent"/>.</summary>
  Window,
  /// <summary>An event derived from <see cref="UserEvent"/>.</summary>
  UserDefined
}

/// <summary>This enum holds values which are ORed together to specify the types of focus events that occurred.
/// Multiple focus events can occur simultaneously, so multiple flags flags may be specified at once.
/// </summary>
[Flags]
public enum FocusType
{ 
  /// <summary>Mouse focus was gained or lost. Typically this occurs when the mouse is moved into or out of the
  /// window.
  /// </summary>
  Mouse=SDL.FocusType.MouseFocus,
  /// <summary>Input focus was gained or lost. Typically this occurs when the application moves into the foreground or
  /// background.
  /// </summary>
  Input=SDL.FocusType.InputFocus,
  /// <summary>The application was minimized (iconified) (<see cref="FocusEvent.Focused"/>=false) or restored
  /// (<see cref="FocusEvent.Focused"/>=true).
  /// </summary>
  Application=SDL.FocusType.AppActive
};

/// <summary>This enum holds values specifying the position of a joystick POV hat.</summary>
[Flags]
public enum HatPosition : byte
{ 
  /// <summary>The hat is centered (not pushed in any position).</summary>
  Center=0,
  /// <summary>The hat is pushed upwards.</summary>
  Up=SDL.HatPos.Up,
  /// <summary>The hat is pushed downwards.</summary>
  Down=SDL.HatPos.Down,
  /// <summary>The hat is pushed to the left.</summary>
  Left=SDL.HatPos.Left,
  /// <summary>The hat is pushed to the right.</summary>
  Right=SDL.HatPos.Right,
  /// <summary>The hat is pushed to the upper left.</summary>
  UpLeft=Up|Left,
  /// <summary>The hat is pushed to the upper right.</summary>
  UpRight=Up|Right,
  /// <summary>The hat is pushed to the lower left.</summary>
  DownLeft=Down|Left,
  /// <summary>The hat is pushed to the lower right.</summary>
  DownRight=Down|Right
}

/// <summary>This enum contains values specifying from where an exception was thrown.</summary>
public enum ExceptionLocation
{ 
  /// <summary>An unknown or unspecified location.</summary>
  Unknown,
  /// <summary>The exception was thrown from the audio thread.</summary>
  AudioThread,
  /// <summary>The exception was thrown from a network thread.</summary>
  NetworkThread,
  /// <summary>The exception occurred in a user thread.</summary>
  UserThread
};
/// <summary>This enum contains values that can be returned from an event filter to tell the <see cref="Events"/>
/// class what to do with the event.
/// </summary>
public enum FilterAction
{ 
  /// <summary>The event should be passed onto the next filter, or queued (accepted) if there are no other filters.</summary>
  Continue,
  /// <summary>The event should be dropped (rejected) immediately.</summary>
  Drop,
  /// <summary>The event should be queued (accepted) immediately.</summary>
  Queue
}
#endregion

#region System, base, and miscellaneous event classes
/// <summary>This abstract class is the base of all GameLib events.</summary>
/// <remarks>User-defined events should be derived from <see cref="UserEvent"/>, not this class.</remarks>
public abstract class Event
{ 
  /// <summary>Initializes this event.</summary>
  /// <param name="type">The <see cref="EventType"/> of this event.</param>
  protected Event(EventType type) { this.type=type; }

  /// <summary>Gets the type of this event.</summary>
  /// <value>The <see cref="EventType"/> of this event.</value>
  public EventType Type { get { return type; } }

  EventType type;
}

/// <summary>This event occurs when the application gains or loses focus.</summary>
/// <remarks>See the <see cref="FocusType"/> enumeration to see under what circumstances this event is triggered.
/// This event will not be triggered when the application is first started.
/// </remarks>
public class FocusEvent : Event
{ 
  /// <summary>Initializes this event.</summary>
  public FocusEvent() : base(EventType.Focus) { }

  internal FocusEvent(ref SDL.ActiveEvent evt) : base(EventType.Focus)
  { FocusType=(FocusType)evt.State; Focused=(evt.Focused!=0);
  }

  /// <summary>The type of focus event.</summary>
  /// <value>The <see cref="FocusType"/> of this event.</value>
  public FocusType FocusType;

  /// <summary>Specifies whether focus was gained or lost.</summary>
  /// <value>If true, focus was gained. If false, focus was lost.</value>
  public bool Focused;
}

/// <summary>This event occurs when the system reports that the application should close.</summary>
/// <remarks>Typically this event event occurs when the user closes the main application window, or when the system
/// is shutting down. This event can also be pushed onto the event queue manually. This allows the quitting code
/// to be centralized in the event handler. The event can be ignored to prevent the user from closing the window
/// normally.
/// </remarks>
public class QuitEvent : Event
{ 
  /// <summary>Initializes this event.</summary>
  public QuitEvent() : base(EventType.Quit) { }
}

/// <summary>This event occurs when the application window is resized.</summary>
/// <remarks>This event signifies that the user has resized the application window. When this happens, you should
/// call <see cref="Video.Video.SetMode"/> or <see cref="Video.Video.SetGLMode"/> to set the video mode to the
/// new size. This event is not triggered when the video mode is set.
/// </remarks>
public class ResizeEvent : Event
{ 
  /// <summary>Initializes this event.</summary>
  public ResizeEvent() : base(EventType.Resize) { }

  /// <summary>Gets the size of the new window.</summary>
  public System.Drawing.Size Size { get { return new System.Drawing.Size(Width, Height); } }
  /// <summary>The width of the new window.</summary>
  public int Width;
  /// <summary>The height of the new window.</summary>
  public int Height;

  internal ResizeEvent(ref SDL.ResizeEvent evt) : base(EventType.Resize)
  { Width=evt.Width; Height=evt.Height;
  }
}

/// <summary>This event occurs when the application window needs to be redrawn.</summary>
/// <remarks>This is normally caused by interactions with overlapping windows in the window manager, or by
/// the application window being minimized (iconified) and restored. Generally, a simple
/// <see cref="Video.Video.Flip"/> is sufficient to accomplish the redraw.
/// </remarks>
public class RepaintEvent : Event
{ 
  /// <summary>Initializes this event.</summary>
  public RepaintEvent() : base(EventType.Repaint) { }
}

/// <summary>This event acts as a base class for custom user events.</summary>
/// <remarks>Applications wanting to create custom events should derive classes from this, not <see cref="Event"/>.
/// </remarks>
public abstract class UserEvent : Event
{ 
  /// <summary>Initializes this event.</summary>
  /// <remarks>This constructor sets <see cref="Event.Type"/> to <see cref="EventType.UserDefined"/>.</remarks>
  public UserEvent() : base(EventType.UserDefined) { }
}

/// <summary>This event occurs when an unhandled exception is thrown in a worker thread.</summary>
/// <remarks>The affected subsystem will be left in an undefined state and should be deinitialized after
/// receiving one of these events. For exceptions occurring in user threads, this event will not be generated
/// automatically. You must raise these events manually if you want them in your own threads. For more complicated
/// scenarios, you can derive from this class to allow it to provide more information.
/// </remarks>
public class ExceptionEvent : Event
{ 
  /// <summary>Initializes this event.</summary>
  /// <param name="e">The exception that occurred.</param>
  /// <remarks>This constructor sets <see cref="Location"/> to <see cref="ExceptionLocation.UserThread"/>.</remarks>
  public ExceptionEvent(Exception e) : this(ExceptionLocation.UserThread, e) { }

  internal ExceptionEvent(ExceptionLocation loc, Exception e) : base(EventType.Exception)
  { location=loc; exception=e;
  }

  /// <summary>Gets the location where the exception was thrown.</summary>
  /// <remarks>The <see cref="ExceptionLocation"/> that determines where the exception was thrown.</remarks>
  public ExceptionLocation Location { get { return location; } }

  /// <summary>Gets the exception that occurred.</summary>
  /// <remarks>The exception that occurred.</remarks>
  public Exception Exception { get { return exception; } }

  /// <summary>The exception that occurred.</summary>
  protected Exception exception;
  ExceptionLocation location;
}
#endregion

#region Keyboard event
/// <summary>This event occurs when a key is pressed or released.</summary>
public class KeyboardEvent : Event
{ 
  /// <summary>Initializes this event.</summary>
  public KeyboardEvent() : base(EventType.Keyboard) { }

  internal KeyboardEvent(ref SDL.KeyboardEvent evt) : base(EventType.Keyboard)
  { Key  = (Key)evt.Key.Sym;
    Mods = (KeyMod)evt.Key.Mod;
    Char = evt.Key.Unicode;
    Scan = evt.Key.Scan;
    Down = evt.Down!=0;
  }

  /// <summary>Evaluates to true if the key pressed or released was a modifier key (Ctrl, Alt, etc).</summary>
  public bool IsModKey { get { return Keyboard.IsModKey(Key); } }
  /// <summary>Gets the key modifiers that were in effect when the event occurred.</summary>
  /// <value>A set of <see cref="KeyMod"/> values, ORed together, that were in effect when the event occurred.
  /// </value>
  /// <remarks>Key modifiers only include modifier keys such as Ctrl, Alt, etc., which were depressed when the
  /// event occurred.
  /// </remarks>
  public KeyMod KeyMods { get { return Mods&KeyMod.KeyMask; } }
  /// <summary>Gets the status modifiers that were in effect when the event occurred.</summary>
  /// <value>A set of <see cref="KeyMod"/> values, ORed together, that were in effect when the event occurred.
  /// </value>
  /// <remarks>Status modifiers only include modifiers such as NumLock, CapsLock, etc., which were enabled when the
  /// event occurred.
  /// </remarks>
  public KeyMod StatusMods { get { return Mods&KeyMod.StatusMask; } }

  /// <summary>Returns true if any of the given modifiers were in effect when the event occurred.</summary>
  /// <param name="mod">A set of <see cref="KeyMod"/> modifiers, ORed together.</param>
  /// <returns>True if any of the given modifiers were in effect when the event occurred, and false
  /// otherwise.
  /// </returns>
  public bool HasAnyMod(KeyMod mod) { return (Mods&mod)!=KeyMod.None; }
  /// <summary>Returns true if all of the given modifiers were in effect when the event occurred.</summary>
  /// <param name="mod">A set of <see cref="KeyMod"/> modifiers, ORed together.</param>
  /// <returns>True if all of the given modifiers were in effect when the event occurred, and false
  /// otherwise.
  /// </returns>
  public bool HasAllMods(KeyMod mod) { return (Mods&mod)==mod; }
  /// <summary>Returns true if any of the given key modifiers were in effect when the event occurred, and no
  /// others.
  /// </summary>
  /// <param name="mod">A set of <see cref="KeyMod"/> modifiers, ORed together.</param>
  /// <returns>True if any of the given key modifiers were in effect when the event occurred, and no
  /// others.
  /// </returns>
  /// <remarks>Key modifiers only include modifier keys such as Ctrl, Alt, etc., which were depressed when the
  /// event occurred. To check whether all (instead of any) of the given key modifiers were in effect, and no others,
  /// simply compare the desired set of flags to see whether it equals <see cref="KeyMods"/>.
  /// </remarks>
  public bool HasOnlyKeys(KeyMod mod)
  { KeyMod mods = KeyMods;
    return (mods&mod)!=KeyMod.None && (mods&~mod)==KeyMod.None;
  }

  /// <summary>The virtual key that was pressed/released.</summary>
  /// <value>The <see cref="Key"/> that was pressed/released.</value>
  /// <remarks>This is a virtual key, which means that it may not map to a key actually present on the keyboard.
  /// The operating system can use combinations of keys on the keyboard or other criteria to allow more key values
  /// than are present on the physical keyboard. This is comparatively rare, however. The values
  /// in the <see cref="Key"/> enumeration are named assuming a QWERTY layout, but depending on the operating
  /// system, may only denote the given position on the keyboard, so you should not use this field to get character
  ///  Use <see cref="Char"/> for that. This field only presents the key on the virtual keyboard as
  /// recognized by the operating system.
  /// </remarks>
  public Key Key;

  /// <summary>The modifiers that were in effect when the event occurred.</summary>
  /// <value>A set of <see cref="KeyMod"/> values, ORed together, representing the modifiers that were
  /// in effect when the event occurred.
  /// </value>
  /// <remarks>This field combines both key modifiers and status modifiers. Use the <see cref="KeyMods"/> and
  /// <see cref="StatusMods"/> properties to separate them.
  /// </remarks>
  public KeyMod Mods;

  /// <summary>The character that was generated by the keypress.</summary>
  /// <value>The actual character that was generated, taking into account keyboard mapping and modifier keys.</value>
  /// <remarks>This field does not always contain a valid character, in which case it will be set to zero (The NUL
  /// character. For instance, on the event associated with the press of a shift key, it's unlikely that a character
  /// will be generated. Also, no characters are generated when a key is released. For reading text input, this is the
  /// field that should be used.
  /// </remarks>
  public char Char;

  /// <summary>The scancode of the key that was pressed.</summary>
  /// <remarks>The scancode of the key is the location of the key on the physical keyboard as reported by the
  /// hardware. Generally, this field should not be used as most systems have keys mapped to a virtual keyboard.
  /// The <see cref="Key"/> field should be used to get the virtual key code and the <see cref="Char"/> field should
  /// be used to get character 
  /// </remarks>
  public byte Scan;

  /// <summary>Determines whether the key was pressed or released.</summary>
  /// <value>True if the key was pressed and false if it was released.</value>
  public bool Down;
}
#endregion

#region Mouse events
/// <summary>This event occurs when the mouse is moved while the application has mouse focus.</summary>
public class MouseMoveEvent : Event
{ 
  /// <summary>Initializes this event.</summary>
  public MouseMoveEvent() : base(EventType.MouseMove) { }

  internal MouseMoveEvent(ref SDL.MouseMoveEvent evt) : base(EventType.MouseMove)
  { X=(int)evt.X; Y=(int)evt.Y; Xrel=(int)evt.Xrel; Yrel=(int)evt.Yrel; Buttons=evt.State;
  }

  /// <summary>Returns true if only the specified mouse button was depressed when the event occurred.</summary>
  /// <param name="button">The <see cref="MouseButton"/> to check for.</param>
  /// <returns>True if only the specified button was depressed when the event occurred.</returns>
  public bool OnlyPressed(MouseButton button) { return Buttons==(1<<(byte)button); }

  /// <summary>Returns true if the specified mouse button was depressed when the event occurred.</summary>
  /// <param name="button">The <see cref="MouseButton"/> to check for.</param>
  /// <returns>True if the specified mouse button was depressed when the event occurred.</returns>
  public bool Pressed(MouseButton button) { return (Buttons&(1<<(byte)button))!=0; }

  /// <summary>Alters the <see cref="Buttons"/> field, setting the specified button to be marked as pressed or not.</summary>
  /// <param name="button">The <see cref="MouseButton"/> to mark as pressed/released.</param>
  /// <param name="down">If true, the specified button is marked as pressed. Otherwise, it's marked as released.</param>
  /// <remarks>This method alters the <see cref="Buttons"/> field, which has the effect of altering the return values
  /// of the <see cref="OnlyPressed"/> and <see cref="Pressed"/> methods as well.
  /// </remarks>
  public void SetPressed(MouseButton button, bool down)
  { if(down) Buttons|=(byte)(1<<(byte)button); else Buttons&=(byte)~(1<<(byte)button);
  }

  /// <summary>Gets the point to which the mouse was moved.</summary>
  public System.Drawing.Point Point
  { get { return new System.Drawing.Point(X, Y); }
    set { X=value.X; Y=value.Y; }
  }

  /// <summary>Gets the distance the mouse was moved since the last event.</summary>
  public System.Drawing.Size Offset
  { get { return new System.Drawing.Size(Xrel, Yrel); }
    set { Xrel=value.Width; Yrel=value.Height; }
  }

  /// <summary>This field holds the X coordinate of the point to which the mouse was moved.</summary>
  public int X;
  /// <summary>This field holds the Y coordinate of the point to which the mouse was moved.</summary>
  public int Y;
  /// <summary>This field holds the distance the mouse was moved on the X axis since the last event.</summary>
  public int Xrel;
  /// <summary>This field holds the distance the mouse was moved on the Y axis since the last event.</summary>
  public int Yrel;
  /// <summary>This field is a bitfield specifying which buttons were depressed at the time the event occurred.</summary>
  /// <remarks>The bitfield is packed so the first mouse button is in the low bit, the second mouse button is in the
  /// next bit, etc.
  /// </remarks>
  public byte Buttons;
}

/// <summary>Occurs when a mouse button is pressed or released or the mouse wheel is moved.</summary>
public class MouseClickEvent : Event
{ 
  /// <summary>Initializes this event.</summary>
  public MouseClickEvent() : base(EventType.MouseClick) { }
  internal MouseClickEvent(ref SDL.MouseButtonEvent evt) : base(EventType.MouseClick)
  { Button=(MouseButton)(evt.Button-1); Down=(evt.Down!=0); X=(int)evt.X; Y=(int)evt.Y;
  }

  /// <summary>Returns true if this event was caused by the mouse wheel being moved.</summary>
  /// <remarks>This works by checking whether <see cref="Button"/> is equal to <see cref="MouseButton.WheelUp"/>
  /// or <see cref="MouseButton.WheelDown"/>.
  /// </remarks>
  public bool MouseWheel
  { get { return Button==MouseButton.WheelDown || Button==MouseButton.WheelUp; }
  }
  /// <summary>Gets the position of the mouse cursor at the time the button event was recorded.</summary>
  public System.Drawing.Point Point
  { get { return new System.Drawing.Point(X, Y); }
    set { X=value.X; Y=value.Y; }
  }

  /// <summary>The X coordinate of the point the mouse cursor was at when the button event was recorded.</summary>
  public int X;
  /// <summary>The Y coordinate of the point the mouse cursor was at when the button event was recorded.</summary>
  public int Y;
  /// <summary>The mouse button which was pressed or released.</summary>
  public MouseButton Button;
  /// <summary>True if the mouse button was pressed and false if it was released.</summary>
  public bool Down;
}
#endregion

#region Joystick events
/// <summary>Occurs when a joystick is moved along an axis.</summary>
public class JoyMoveEvent : Event
{ 
  /// <summary>Initializes this event.</summary>
  public JoyMoveEvent() : base(EventType.JoyMove) { }
  internal JoyMoveEvent(ref SDL.JoyAxisEvent evt) : base(EventType.JoyMove)
  { Position=evt.Value; Device=evt.Device; Axis=evt.Axis;
  }

  /// <summary>The new position along the axis.</summary>
  public int Position;
  /// <summary>The index of the axis that was moved.</summary>
  /// <remarks>While the meanings of the axes are not defined anywhere, it's a safe bet that 0 is the X axis and
  /// 1 is the Y axis. 2 and 3 may be the Z axis and throttle, but not necessarily in that order. This can be used
  /// to index into <see cref="Joystick.Axes"/>.
  /// </remarks>
  public byte Axis;
  /// <summary>The joystick that caused this event.</summary>
  /// <remarks> This field can be used to index into <see cref="Input.Input.Joysticks"/>.</remarks>
  public byte Device;
}

/// <summary>Occurs when a joystick ball is moved.</summary>
/// <remarks>Some joysticks have trackball-like devices embedded in them. This event occurs when one of those is moved.
/// </remarks>
public class JoyBallEvent : Event
{ 
  /// <summary>Initializes this event.</summary>
  public JoyBallEvent() : base(EventType.JoyBall) { }
  internal JoyBallEvent(ref SDL.JoyBallEvent evt) : base(EventType.JoyBall)
  { Device=evt.Device; Ball=evt.Ball; Xrel=(int)evt.Xrel; Yrel=(int)evt.Yrel;
  }

  /// <summary>Gets the distance the ball was moved since the last event regarding this ball.</summary>
  public System.Drawing.Size Offset { get { return new System.Drawing.Size(Xrel, Yrel); } }

  /// <summary>The distance the ball was moved along the X axis since the last event regarding this ball.</summary>
  public int Xrel;
  /// <summary>The distance the ball was moved along the Y axis since the last event regarding this ball.</summary>
  public int Yrel;
  /// <summary>The index of the ball that was moved.</summary>
  /// <remarks>This can be used to index into <see cref="Joystick.Balls"/>.</remarks>
  public byte Ball;
  /// <summary>The joystick that caused this event.</summary>
  /// <remarks> This field can be used to index into <see cref="Input.Input.Joysticks"/>.</remarks>
  public byte Device;
}

/// <summary>Occurs when a joystick POV hat is moved.</summary>
public class JoyHatEvent : Event
{ 
  /// <summary>Initializes this event.</summary>
  public JoyHatEvent() : base(EventType.JoyHat) { }
  internal JoyHatEvent(ref SDL.JoyHatEvent evt) : base(EventType.JoyHat)
  { Device=evt.Device; Hat=evt.Hat; Position=(HatPosition)evt.Position;
  }

  /// <summary>The new position of the POV hat.</summary>
  public HatPosition Position;
  /// <summary>The index of the hat that was moved.</summary>
  /// <remarks>This can be used to index into <see cref="Joystick.Hats"/>.</remarks>
  public byte Hat;
  /// <summary>The joystick that caused this event.</summary>
  /// <remarks> This field can be used to index into <see cref="Input.Input.Joysticks"/>.</remarks>
  public byte Device;
}

/// <summary>Occurs when a joystick button is pressed or released.</summary>
public class JoyButtonEvent : Event
{ 
  /// <summary>Initializes this event.</summary>
  public JoyButtonEvent() : base(EventType.JoyButton) { }
  internal JoyButtonEvent(ref SDL.JoyButtonEvent evt) : base(EventType.JoyButton)
  { Device=evt.Device; Button=evt.Button; Down=(evt.Down!=0);
  }

  /// <summary>The joystick that caused this event.</summary>
  /// <remarks> This field can be used to index into <see cref="Input.Input.Joysticks"/>.</remarks>
  public byte Device;
  /// <summary>The index of the button that caused this event.</summary>
  /// <remarks> This field can be used to index into <see cref="Joystick.Buttons"/>.</remarks>
  public byte Button;
  /// <summary>True if the button was pressed and false if it was released.</summary>
  public bool Down;
}
#endregion

#region Windowing system events
/// <summary>This event class serves as a base for all messages specific to the windowing system.</summary>
/// <remarks>These messages serve to implement the internals of the windowing system and do not normally need to be
/// handled by user code. However, implementors of custom windowing controls may want to derive from this class to
/// implement custom events. Since the UI handling code and the event handling code should be on the same thread,
/// controls that use other threads (or use objects such as <see cref="System.Threading.Timer"/>, which use other
/// threads) are good candidates for using events derived from this class. See <see cref="Forms.Control"/> for more
/// information.
/// </remarks>
public abstract class WindowEvent : Event
{ 
  /// <summary>Initializes this object as a custom windowing event.</summary>
  /// <param name="control">The control associated with this event. The event will be passed to that control's
  /// <see cref="Forms.Control.OnCustomEvent"/> handler.
  /// </param>
  /// <remarks>This constructor sets <see cref="SubType"/> to <see cref="MessageType.Custom"/>.</remarks>
  public WindowEvent(GameLib.Forms.Control control) : this(control, MessageType.Custom) { }
  internal WindowEvent(GameLib.Forms.Control control, MessageType subType) : base(EventType.Window)
  { if(control==null) throw new ArgumentNullException("control");
    Control=control; this.subType=subType;
  }
  /// <summary>This enum contains values denoting the various types of window events.</summary>
  public enum MessageType
  { 
    /// <summary>A custom event.</summary>
    /// <remarks>All windowing events that are not used to implement the core windowing logic use this type.</remarks>
    Custom,
    /// <summary>An event which signals that a control needs to repaint itself.</summary>
    Paint,
    /// <summary>An event which signals that a control needs to arrange its children.</summary>
    Layout,
    /// <summary>An event which signals that a key should repeat due to having been held down.</summary>
    KeyRepeat,
    /// <summary>An event which serves to wake up the desktop so it'll redraw itself, in the event that it's been
    /// updated manually by a custom control.
    /// </summary>
    DesktopUpdated
  };
  /// <summary>Gets the type of windowing event this object represents.</summary>
  public MessageType SubType { get { return subType; } }
  /// <summary>The control associated with this event.</summary>
  public GameLib.Forms.Control Control;

  MessageType subType;
}

internal class WindowPaintEvent : WindowEvent
{ public WindowPaintEvent(GameLib.Forms.Control control) : base(control, WindowEvent.MessageType.Paint) { }
}

internal class WindowLayoutEvent : WindowEvent
{ public WindowLayoutEvent(GameLib.Forms.Control control, bool recursive)
    : base(control, WindowEvent.MessageType.Layout) { Recursive=recursive; }
  public bool Recursive;
}

internal class KeyRepeatEvent : WindowEvent
{ public KeyRepeatEvent(GameLib.Forms.DesktopControl desktop) : base(desktop, WindowEvent.MessageType.KeyRepeat) { }
}

/// <summary>This event serves to wake up the desktop so it'll redraw itself, in the event that it's been
/// updated manually by a custom control.
/// </summary>
/// <remarks>This event helps serve to implement the internals of the windowing system and does not need to be
/// handled by ordinary user code. The desktop will normally update itself automatically, but if it's waiting for
/// the next event (it may do this, depending on the type of event loop you use in the application), it won't paint
/// itself until the next event wakes it up. A custom control that manually updates the desktop and wants to be sure
/// the change will be immediately visible may post this message to wake up the desktop so it'll see that it's been
/// updated.
/// </remarks>
public class DesktopUpdatedEvent : WindowEvent
{ public DesktopUpdatedEvent(GameLib.Forms.DesktopControl desktop)
    : base(desktop, WindowEvent.MessageType.DesktopUpdated) { }
}
#endregion

#region Events class
/// <summary>This delegate is used in conjunction with <see cref="Events.EventFilter"/> to filter events.</summary>
/// <remarks>The function should return a <see cref="FilterAction"/> value to determine how the incoming event will
/// be treated.
/// </remarks>
public delegate FilterAction EventFilter(Event e);
/// <summary>This delegate is used by <see cref="Events.EventProcedure"/>, <see cref="Events.PumpEvent"/>, and
/// <see cref="Events.PumpEvents"/> to handle a given event.
/// </summary>
/// <remarks>The function should handle the event passed to it in a way meaningful to the application.
/// The function should return true if the event loop should continue working, and false if it should stop. False
/// should generally be returned only when the application is quitting, but it can be returned in order to break out
/// of <see cref="Events.PumpEvents"/> (though this is considered poor style, and potentially confusing). Returning
/// false will set <see cref="Events.QuitFlag"/> to true, which will cause many functions to do nothing and return
/// immediately, because the assumption is that the application will be quitting. <see cref="Events.QuitFlag"/> can
/// be reset to false to counter this (though again, this is only supported for flexibility -- using a false return
/// to indicate anything besides that the application should quit is considered bad style).
/// </remarks>
public delegate bool EventProcedure(Event e);
/// <summary>This delegate is used by <see cref="Events.IdleProcedure"/>, <see cref="Events.PumpEvent"/>, and
/// <see cref="Events.PumpEvents"/> to do something when no other events are executing.
/// </summary>
/// <remarks>This function can be used to do something when no other events are pending, such as paint the screen
/// or work on a low-priority background task. The function should not wait too long, however, as it will hold up
/// the event queue if it does. If the function returns true, the event loop will check for events and if none
/// are available yet, call the idle procedure again immediately. If the function returns false, the event loop will
/// wait for the next event. Thus, returning true regularly during a long background task ensures that the event
/// loop is not held up for too long. False should be returned when there is no more work to be done to prevent the
/// event loop from calling the idle procedure in a tight loop, consuming all the CPU resources.
/// </remarks>
public delegate bool IdleProcedure();

/// <summary>This class handles events from the operating system, from GameLib, and from user code, presenting a
/// single event queue and a variety of ways to interact with it.
/// </summary>
/// <remarks>Multiple threads can add events, but only one thread should read events. If multiple threads must read
/// events, lock the <see cref="Events.SyncRoot"/> property with <see cref="System.Threading.Monitor"/> or
/// C#'s <c>lock</c> statement to prevent concurrent access. Events cannot be added with <see cref="PushEvent"/>
/// from other threads while those locks are held, though!
/// The event subsystem must be initialized by calling <see cref="Events.Initialize"/> before use.
/// </remarks>
public sealed class Events
{ private Events() { }

  /// <summary>A value representing an infinite amount of time.</summary>
  /// <remarks>This value can be passed to <see cref="NextEvent"/> and <see cref="PeekEvent"/> to request that
  /// they block until an event becomes available.
  /// </remarks>
  public const int Infinite=-1;

  /// <summary>Occurs when an event is received, but before the event is added to the queue. This can be used to
  /// filter incoming events. See <see cref="EventFilter"/> for more information.
  /// </summary>
  public static event EventFilter EventFilter;
  /// <summary>Called to handle an event removed from the queue by <see cref="PumpEvent"/> or
  /// <see cref="PumpEvents"/>. See <see cref="GameLib.Events.EventProcedure"/> for more information.
  /// </summary>
  public static event EventProcedure EventProcedure;
  /// <summary>Called by <see cref="PumpEvent"/> and <see cref="PumpEvents"/> when the queue is empty. See
  /// <see cref="GameLib.Events.IdleProcedure"/> for more information.
  /// </summary>
  public static event IdleProcedure IdleProcedure;

  /// <summary>Returns true if the event subsystem has been initialized.</summary>
  public static bool Initialized { get { return initCount>0; } }

  /// <summary>Gets/sets the maximum size of the event queue.</summary>
  /// <value>The maximum size of the event queue, in number of events.</value>
  /// <remarks>If set to a size smaller than the number of events in the queue, the oldest events in the queue
  /// will be dropped to reduce it to the maximum. This can be very destructive, so set the maximum queue size
  /// before receiving events with this class -- ideally before calling <see cref="Initialize"/>, even.
  /// </remarks>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is less than or equal to zero.</exception>
  public static int MaxQueueSize
  { get { return max; }
    set
    { if(value<=0) throw new ArgumentOutOfRangeException("value", value, "must be greater than zero");
      lock(queue)
      { if(value<max) while(queue.Count>value) queue.Dequeue();
        max=value;
      }
    }
  }

  /// <summary>Gets an object that can be locked to synchronize access to the event queue.</summary>
  /// <remarks>While it's strongly recommended that only one thread read events from the queue, it should be
  /// possible to have multiple threads reading the queue if each thread locks this property with a
  /// <see cref="System.Threading.Monitor"/> or the C# <c>lock</c> statement. Note that it is not possible to
  /// add events from other threads using <see cref="PushEvent"/> while this is locked.
  /// </remarks>
  public static object SyncRoot { get { return queue; } }

  /// <summary>Gets the number of events waiting in the queue.</summary>
  public static int Count { get { return queue.Count; } }

  /// <summary>Gets/sets the quit flag, which indicates that the application has shown an intention to quit.</summary>
  /// <remarks>If the application returns false from an <see cref="GameLib.Events.EventProcedure"/>, the quit flag
  /// will be set to true. The quit flag will cause some methods to do no work and
  /// return immediately, with the assumption being that the application is about to quit. See
  /// <see cref="GameLib.Events.EventProcedure"/> for more information on this.
  /// </remarks>
  public static bool QuitFlag { get { return quit; } set { quit=value; } }

  /// <summary>Initializes the event subsystem.</summary>
  /// <remarks>This method can be called multiple times. The <see cref="Deinitialize"/> method should be called the
  /// same number of times to finally deinitialize the system.
  /// </remarks>
  public static void Initialize()
  { if(initCount++==0)
    { mods = (KeyMod)SDL.GetModState();
      SDL.Initialize(SDL.InitFlag.Video);
      SDL.Initialize(SDL.InitFlag.EventThread); // SDL quirk: this must be done AFTER video
      SDL.EnableUNICODE(1); // SDL quirk: must be AFTER Initialize(EventThread)
    }
  }

  /// <summary>Deinitializes the event subsystem.</summary>
  /// <remarks>This method should be called the same number of times that <see cref="Initialize"/> has been called
  /// in order to deinitialize the event subsystem. All messages still waiting in the queue will be lost.
  /// </remarks>
  public static void Deinitialize()
  { if(initCount==0) throw new InvalidOperationException("Deinitialize called too many times!");
    if(--initCount==0)
      lock(queue)
      { SDL.Deinitialize(SDL.InitFlag.Video|SDL.InitFlag.EventThread);
        queue.Clear();
        waiting = false;
      }
  }

  /// <summary>Returns the next event without removing it from the queue.</summary>
  /// <returns>The next event in the queue, or null if the queue is empty.</returns>
  /// <remarks>Calling this method is equivalent to calling <see cref="NextEvent(int,bool)"/> with a timeout of
  /// zero and passing false to signal that the event is not to be removed from the queue.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if the event subsystem has not been initialized.</exception>
  public static Event PeekEvent() { return NextEvent(0, false); }
  /// <summary>Removes and returns the next event from the queue.</summary>
  /// <returns>The next event in the queue.</returns>
  /// <remarks>Calling this method is equivalent to calling <see cref="NextEvent(int,bool)"/> with a timeout of
  /// <see cref="Infinite"/> and passing true to signal that the event should be removed from the queue.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if the event subsystem has not been initialized.</exception>
  public static Event NextEvent() { return NextEvent(Infinite, true);  }
  /// <summary>Returns the next event without removing it from the queue.</summary>
  /// <param name="timeout">The amount of time to wait for an event. <see cref="Infinite"/> can be passed to
  /// specify an infinite timeout.
  /// </param>
  /// <returns>The next event in the queue, or null if no events arrive in the specified timeout period.</returns>
  /// <remarks>Calling this method is equivalent to calling <see cref="NextEvent(int,bool)"/> and passing false to
  /// signal that the event is not to be removed from the queue.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if the event subsystem has not been initialized.</exception>
  public static Event PeekEvent(int timeout) { return NextEvent(timeout, false); }
  /// <summary>Removes and returns the next event from the queue.</summary>
  /// <param name="timeout">The amount of time to wait for an event. <see cref="Infinite"/> can be passed to
  /// specify an infinite timeout.
  /// </param>
  /// <returns>The next event in the queue, or null if no events arrive in the specified timeout period.</returns>
  /// <remarks>Calling this method is equivalent to calling <see cref="NextEvent(int,bool)"/> and passing true to
  /// signal that the event should be removed from the queue.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if the event subsystem has not been initialized.</exception>
  public static Event NextEvent(int timeout) { return NextEvent(timeout, true);  }
  /// <summary>Returns the next event from the queue, optionally removing it as well.</summary>
  /// <param name="timeout">The amount of time to wait for an event. <see cref="Infinite"/> can be passed to
  /// specify an infinite timeout.
  /// </param>
  /// <param name="remove">Set to true if the event is to be removed, and false if not.</param>
  /// <returns>The next event in the queue, or null if no events arrive in the specified timeout period.</returns>
  /// <exception cref="InvalidOperationException">Thrown if the event subsystem has not been initialized.</exception>
  public static Event NextEvent(int timeout, bool remove)
  { AssertInit();

    lock(queue)
    { if(waiting) waiting=false;
      if(queue.Count>0) return remove ? queue.Dequeue() : queue.Peek();
    }

    Event ret;

    if(timeout==Infinite)
    { ret = NextSDLEvent();
      lock(queue)
      { if(ret==null) { waiting=false; return remove ? queue.Dequeue() : queue.Peek(); }
        else if(!remove) QueueEvent(ret);
        return ret;
      }
    }

    uint start = SDL.GetTicks();
    do
    { ret = PeekSDLEvent();
      lock(queue)
      { if(ret!=null)
        { if(!remove) QueueEvent(ret);
          if(waiting) { waiting=false; return remove ? queue.Dequeue() : queue.Peek(); }
          return ret;
        }
      }
      System.Threading.Thread.Sleep(10); // i don't like this...
      timeout -= (int)(SDL.GetTicks()-start);
    } while(timeout>0);
    return null;
  }

  /// <summary>This method adds an event to the queue.</summary>
  /// <param name="evt">The event to add.</param>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="evt"/> is null.</exception>
  public static void PushEvent(Event evt) { PushEvent(evt, false); }
  /// <summary>This method adds an event to the queue.</summary>
  /// <param name="evt">The event to add.</param>
  /// <param name="filter">If true, the event will be passed through the registered event filters. Otherwise, it
  /// will not.
  /// </param>
  /// <returns>Returns true if the event was added to the queue, and false if not. The event will not be added if
  /// it was filtered out.
  /// </returns>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="evt"/> is null.</exception>
  public static bool PushEvent(Event evt, bool filter)
  { if(evt==null) throw new ArgumentNullException("evt");
    AssertInit();
    if(filter && !FilterEvent(evt)) return false;
    lock(queue)
    { QueueEvent(evt);
      waiting = true;

      SDL.Event e = new SDL.Event();
      e.Type = SDL.EventType.UserEvent0;
      SDL.PushEvent(ref e);
    }

    return true;
  }

  /// <summary>Handles the next event.</summary>
  /// <returns>Returns true if the application should continue working or false if it should quit.</returns>
  /// <remarks>
  /// <para>
  /// This method requires that at least one <see cref="GameLib.Events.EventProcedure"/> has been registered
  /// with the <see cref="EventProcedure"/> event. If the <see cref="QuitFlag"/> is true, this method will return
  /// immediately without doing anything. Otherwise, this method will wait until an event arrives, calling the
  /// <see cref="IdleProcedure"/> event until all return false as long as no events are waiting in the queue. After
  /// an event arrives, it is processed with <see cref="EventProcedure"/>. If any event procedure returns false, the
  /// <see cref="QuitFlag"/> is set to true and processing of further event procedures is not done.
  /// The logical negation of the <see cref="QuitFlag"/> is then returned.
  /// </para>
  /// See <see cref="GameLib.Events.EventProcedure"/> for more information.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if no event procedure has been registered.</exception>
  public static bool PumpEvent()
  { if(EventProcedure==null) throw new InvalidOperationException("No event procedure has been registered");
    if(quit) return false;
    Event e = NextEvent(0);
    if(e==null)
    { if(IdleProcedure!=null)
      { Delegate[] idles = IdleProcedure.GetInvocationList();
        bool morework;
        do
        { morework = false;
          for(int i=0; i<idles.Length; i++) if(((IdleProcedure)idles[i])()) morework=true;
        } while(morework && !quit && PeekEvent(0)==null);
      }
      e = NextEvent();
    }
    Delegate[] procs = EventProcedure.GetInvocationList();
    for(int i=0; i<procs.Length; i++) if(!((EventProcedure)procs[i])(e)) { quit=true; break; }
    return !quit;
  }

  /// <summary>Handle events until the application decides to quit.</summary>
  /// <remarks>Calling this method is equivalent to calling <see cref="PumpEvents(EventProcedure,IdleProcedure)"/>
  /// and passing null for both parameters.
  /// </remarks>
  public static void PumpEvents() { PumpEvents(null, null); }
  /// <summary>Handle events until the application decides to quit.</summary>
  /// <param name="proc">A <see cref="GameLib.Events.EventProcedure"/> that will be added to the
  /// <see cref="EventProcedure"/> event, or null to not register any.
  /// </param>
  /// <remarks>Calling this method is equivalent to calling <see cref="PumpEvents(EventProcedure,IdleProcedure)"/>
  /// and passing null for the <see cref="GameLib.Events.IdleProcedure"/> parameter. Beware of calling this procedure
  /// multiple times with the same value for <paramref name="proc"/>. That may cause the given event procedure to be
  /// registered multiple times and thus executed multiple times per event.
  /// </remarks>
  public static void PumpEvents(EventProcedure proc) { PumpEvents(proc, null); }
  /// <summary>Handle events until the application decides to quit.</summary>
  /// <param name="proc">A <see cref="GameLib.Events.EventProcedure"/> that will be added to the
  /// <see cref="EventProcedure"/> event, or null to not register any.
  /// </param>
  /// <param name="idle">A <see cref="GameLib.Events.IdleProcedure"/> that will be added to the
  /// <see cref="IdleProcedure"/> event, or null to not register any.
  /// </param>
  /// <remarks>
  /// <para>
  /// This method requires that at least one <see cref="GameLib.Events.EventProcedure"/> has been registered
  /// with the <see cref="EventProcedure"/> event. However, one can be passed in and it will be registered at the
  /// start.
  /// </para>
  /// <para>
  /// If the <see cref="QuitFlag"/> is true, this method will return immediately without doing anything. Otherwise,
  /// this method will wait until an event arrives, calling the <see cref="IdleProcedure"/> event until all return
  /// false as long as no events are waiting in the queue. After an event arrives, it is processed with
  /// <see cref="EventProcedure"/>. If any event procedure returns false, the <see cref="QuitFlag"/> is set to true
  /// and processing of further event procedures will not be done. If the <see cref="QuitFlag"/> is not true after
  /// that, the whole process is repeated. If it's false, the method will return.
  /// See <see cref="GameLib.Events.EventProcedure"/> for more information.
  /// </para>
  /// <para>
  /// Beware of calling this procedure multiple times with the same value for <paramref name="proc"/> or
  /// <paramref name="idle"/>. That may cause the given event or idle procedure to be registered multiple times and
  /// thus executed multiple times per event. Also, the lists of registered event and idle handlers are only
  /// retrieved once during the call to this method, so if additional handlers are added, they will not be used.
  /// You should register any handlers you want to use before calling this method or by passing them in the
  /// <paramref name="proc"/> and <paramref name="idle"/> parameters. This restriction may be lifted in the future.
  /// </para>
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if no event procedure has been registered.</exception>
  public static void PumpEvents(EventProcedure proc, IdleProcedure idle)
  { if(proc!=null) EventProcedure += proc;
    if(idle!=null) IdleProcedure += idle;
    if(EventProcedure==null) throw new InvalidOperationException("No event procedure has been registered");
    Delegate[] procs = EventProcedure.GetInvocationList();
    Delegate[] idles = IdleProcedure==null ? null : IdleProcedure.GetInvocationList();
    while(!quit)
    { Event e = NextEvent(0);
      if(e==null)
      { if(idles!=null)
        { bool morework;
          do
          { morework = false;
            for(int i=0; i<idles.Length; i++) if(((IdleProcedure)idles[i])()) morework=true;
          } while(morework && !quit && PeekEvent(0)==null);
        }
        if(quit) break;
        e = NextEvent();
      }
      for(int i=0; i<procs.Length; i++) if(!((EventProcedure)procs[i])(e)) { goto done; }
    }
    done:
    quit=true;
  }

  sealed class UserEventPushed : UserEvent { }

  static unsafe Event PeekSDLEvent()
  { Event ret=null;
    SDL.Event evt = new SDL.Event();
    do
    { unsafe
      { if(SDL.PollEvent(ref evt)!=0)
        { ret = ConvertEvent(ref evt);
          if(!FilterEvent(ret)) ret=null;
        }
        else break;
      }
    } while(ret==null);
    return ret;
  }

  static unsafe Event NextSDLEvent()
  { Event ret;
    SDL.Event evt = new SDL.Event();
    while(true)
    { unsafe
      { if(SDL.WaitEvent(ref evt)!=0)
        { ret = ConvertEvent(ref evt);
          if(FilterEvent(ret)) return ret;
        }
      }
    }
  }

  static void AssertInit() { if(initCount==0) throw new InvalidOperationException("Events not initialized yet"); }
  static bool FilterEvent(Event evt)
  { if(evt==null) return false;
    if(evt==userEvent) return true;
    if(EventFilter!=null)
      foreach(EventFilter ef in EventFilter.GetInvocationList())
        switch(ef(evt))
        { case FilterAction.Drop:  return false;
          case FilterAction.Queue: break;
        }
    return true;
  }

  static unsafe Event ConvertEvent(ref SDL.Event evt)
  { switch(evt.Type)
    { case SDL.EventType.Active:
      { FocusEvent e = new FocusEvent(ref evt.Active);

        if((e.FocusType&FocusType.Application) != 0) Video.WM.minimized = e.Focused;
        if((e.FocusType&FocusType.Input) != 0) Video.WM.inputFocus = e.Focused;
        if((e.FocusType&FocusType.Mouse) != 0) Video.WM.mouseFocus = e.Focused;

        // unset keyboard mods when regaining application focus, to prevent mod keys from seeming stuck
        if(e.Focused && (e.FocusType&(FocusType.Application|FocusType.Input))!=0) mods &= ~KeyMod.KeyMask;
        return e;
      }

      case SDL.EventType.KeyDown: case SDL.EventType.KeyUp:
      { KeyboardEvent e = new KeyboardEvent(ref evt.Keyboard);
        mods = mods & ~KeyMod.StatusMask | e.StatusMods;
        if(e.Down) switch(e.Key) // SDL's mod handling is quirky, so i'll do it myself
        { case Key.LShift: mods |= KeyMod.LShift; break;
          case Key.RShift: mods |= KeyMod.RShift; break;
          case Key.LCtrl:  mods |= KeyMod.LCtrl;  break;
          case Key.RCtrl:  mods |= KeyMod.RCtrl;  break;
          case Key.LAlt:   mods |= KeyMod.LAlt;   break;
          case Key.RAlt:   mods |= KeyMod.RAlt;   break;
          case Key.LMeta:  mods |= KeyMod.LMeta;  break;
          case Key.RMeta:  mods |= KeyMod.RMeta;  break;
        }
        else switch(e.Key)
        { case Key.LShift: mods &= ~KeyMod.LShift; break;
          case Key.RShift: mods &= ~KeyMod.RShift; break;
          case Key.LCtrl:  mods &= ~KeyMod.LCtrl;  break;
          case Key.RCtrl:  mods &= ~KeyMod.RCtrl;  break;
          case Key.LAlt:   mods &= ~KeyMod.LAlt;   break;
          case Key.RAlt:   mods &= ~KeyMod.RAlt;   break;
          case Key.LMeta:  mods &= ~KeyMod.LMeta;  break;
          case Key.RMeta:  mods &= ~KeyMod.RMeta;  break;
        }
        e.Mods = mods;
        return e;
      }

      case SDL.EventType.MouseMove: return new MouseMoveEvent(ref evt.MouseMove);
      case SDL.EventType.MouseDown: case SDL.EventType.MouseUp: return new MouseClickEvent(ref evt.MouseButton);
      case SDL.EventType.JoyAxis: return new JoyMoveEvent(ref evt.JoyAxis);
      case SDL.EventType.JoyBall: return new JoyBallEvent(ref evt.JoyBall);
      case SDL.EventType.JoyHat:  return new JoyHatEvent(ref evt.JoyHat);
      case SDL.EventType.JoyDown: case SDL.EventType.JoyUp: return new JoyButtonEvent(ref evt.JoyButton);
      case SDL.EventType.Quit: return new QuitEvent();
      case SDL.EventType.VideoResize: return new ResizeEvent(ref evt.Resize);
      case SDL.EventType.VideoExposed: return new RepaintEvent();
      case SDL.EventType.UserEvent0: return userEvent;
      default: return null;
    }
  }

  static void QueueEvent(Event evt)
  { if(queue.Count>=max) queue.Dequeue();
    queue.Enqueue(evt);
  }

  static System.Collections.Generic.Queue<Event> queue = new System.Collections.Generic.Queue<Event>();
  static UserEvent userEvent = new UserEventPushed();
  static uint initCount;
  static int  max=128;
  internal static KeyMod mods;
  static bool waiting, quit;
}
#endregion

} // namespace GameLib.Events