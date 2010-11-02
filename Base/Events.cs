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
  /// <summary>An event derived from <c>ControlEvent</c>.</summary>
  Control,
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
/// call <c>Video.SetMode</c> or <c>Video.SetGLMode</c> to set the video mode to the
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
/// <c>Video.Flip</c> is sufficient to accomplish the redraw.
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
  protected UserEvent() : base(EventType.UserDefined) { }
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
  public Exception Exception
  {
    get { return exception; }
    protected set { exception = value; }
  }

  /// <summary>The exception that occurred.</summary>
  Exception exception;
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
  public bool HasAny(KeyMod mod) { return (Mods&mod)!=KeyMod.None; }
  /// <summary>Returns true if all of the given modifiers were in effect when the event occurred.</summary>
  /// <param name="mod">A set of <see cref="KeyMod"/> modifiers, ORed together.</param>
  /// <returns>True if all of the given modifiers were in effect when the event occurred, and false
  /// otherwise.
  /// </returns>
  public bool HasAll(KeyMod mod) { return (Mods&mod)==mod; }
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
  public bool HasOnly(KeyMod mod)
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
  public bool IsMouseWheel
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

#region Events class
/// <summary>This delegate is used to filter incoming events.</summary>
/// <remarks>The function should return a <see cref="FilterAction"/> value to determine how the incoming event will
/// be treated.
/// </remarks>
public delegate FilterAction EventFilter(Event e);
/// <summary>This delegate is used to handle a given event.</summary>
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
/// <summary>This delegate is used to do something when no events are pending.</summary>
/// <remarks>This function can be used to do something when no events are pending, such as painting the screen or
/// working on a low-priority background task. The function should not wait too long, however, as it will hold up the
/// event queue if it does. If any idle procedure returns true, the event loop will check for events and if none are
/// available yet, call the idle procedures again immediately. If all idle procedures return false, the event loop will
/// wait for the next event. Thus, returning true regularly during a long background task ensures that the event loop
/// is not held up for too long. False should be returned when there is no more work to be done to prevent the event
/// loop from calling the idle procedure in a tight loop, consuming CPU resources unnecessarily.
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
public static class Events
{ 
  /// <summary>A value representing an infinite amount of time.</summary>
  /// <remarks>This value can be passed to <see cref="NextEvent"/> and <see cref="PeekEvent"/> to request that
  /// they block until an event becomes available.
  /// </remarks>
  public const int Infinite = -1;

  /// <summary>Returns true if the event subsystem has been initialized.</summary>
  public static bool Initialized 
  {
    get { return initCount > 0; } 
  }

  /// <summary>Gets or sets the maximum size of the event queue.</summary>
  /// <value>The maximum size of the event queue, in number of events.</value>
  /// <remarks>If set to a size smaller than the number of events in the queue, the oldest events in the queue
  /// will be dropped to reduce it to the maximum.
  /// </remarks>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is less than or equal to zero.</exception>
  public static int MaxQueueSize
  { 
    get { return maxEvents; }
    set
    {
      if(value <= 0) throw new ArgumentOutOfRangeException("value", value, "must be greater than zero");

      lock(queue)
      {
        if(value < maxEvents)
        {
          while(queue.Count > value) queue.Dequeue();
        }
        maxEvents = value;
      }
    }
  }

  /// <summary>Gets the number of events waiting in the queue.</summary>
  /// <remarks>This property can be queried even while the queue is locked.</remarks>
  public static int Count
  {
    get { return queue.Count; }
  }

  /// <summary>Gets an object that can be locked to synchronize access to the event queue.</summary>
  /// <remarks>While it's strongly recommended that only one thread read events from the queue, it is possible
  /// possible to have multiple threads reading the queue if each thread locks this property with a
  /// <see cref="System.Threading.Monitor"/> or the C# <c>lock</c> statement. Note that methods that alter the event
  /// queue, like <see cref="PushEvent"/>, automatically take this lock.
  /// </remarks>
  public static object SyncRoot 
  {
    get { return queue; }
  }

  /// <summary>Gets or sets the quit flag, which indicates that the application has shown an intention to quit.</summary>
  /// <remarks>If the application returns false from an <see cref="GameLib.Events.EventProcedure"/>, the quit flag will
  /// be set to true. The quit flag will cause some methods to do no work and return immediately, with the assumption
  /// being that the application is about to quit. See <see cref="GameLib.Events.EventProcedure"/> for more information
  /// on this.
  /// </remarks>
  public static bool QuitFlag 
  {
    get { return quit; } 
    set { quit = value; } 
  }

  /// <summary>Initializes the event subsystem.</summary>
  /// <remarks>This method can be called multiple times, but <see cref="Deinitialize"/> should be called the
  /// same number of times to finally deinitialize the system.
  /// </remarks>
  public static void Initialize()
  { 
    if(initCount++ == 0)
    { 
      mods = (KeyMod)SDL.GetModState();
      SDL.Initialize(SDL.InitFlag.Video);
      SDL.Initialize(SDL.InitFlag.EventThread); // SDL quirk: this must be done AFTER video
      SDL.EnableUNICODE(1); // SDL quirk: must be done AFTER Initialize(EventThread)
      quit = false;
    }
  }

  /// <summary>Deinitializes the event subsystem.</summary>
  /// <remarks>This method should be called the same number of times that <see cref="Initialize"/> has been called
  /// in order to deinitialize the event subsystem. All messages still waiting in the queue will be lost.
  /// </remarks>
  public static void Deinitialize()
  { 
    if(initCount == 0) throw new InvalidOperationException("Deinitialize called too many times!");
    if(--initCount == 0)
    {
      lock(queue)
      {
        SDL.Deinitialize(SDL.InitFlag.Video | SDL.InitFlag.EventThread);
        queue.Clear();
      }
    }
  }

  /// <summary>Adds an event filter to the end of the list of filters, so that it will be executed after the rest of
  /// the filters in the list.
  /// </summary>
  /// <seealso cref="GameLib.Events.EventFilter"/>
  public static void AppendEventFilter(EventFilter filter)
  {
    if(filter == null) throw new ArgumentNullException();
    filters.Add(filter);
  }

  /// <summary>Adds an event filter to the beginning of the list of filters, so that it will be executed before the
  /// rest of the filters in the list.
  /// </summary>
  /// <seealso cref="GameLib.Events.EventFilter"/>
  public static void PrependEventFilter(EventFilter filter)
  {
    if(filter == null) throw new ArgumentNullException();
    filters.Insert(0, filter);
  }

  /// <summary>Removes an event filter from the list of filters.</summary>
  /// <seealso cref="GameLib.Events.EventFilter"/>
  public static void RemoveEventFilter(EventFilter filter)
  {
    if(filter == null) throw new ArgumentNullException();
    filters.Remove(filter);
  }

  /// <summary>Adds an event handler to the end of the list of handlers, so that it will be executed after the rest of
  /// the handlers in the list.
  /// </summary>
  /// <seealso cref="GameLib.Events.EventProcedure"/>
  public static void AppendEventHandler(EventProcedure handler)
  {
    if(handler == null) throw new ArgumentNullException();
    eventProcs.Add(handler);
  }

  /// <summary>Adds an event handler to the beginning of the list of handlers, so that it will be executed before the
  /// rest of the handlers in the list.
  /// </summary>
  /// <seealso cref="GameLib.Events.EventProcedure"/>
  public static void PrependEventHandler(EventProcedure handler)
  {
    if(handler == null) throw new ArgumentNullException();
    eventProcs.Insert(0, handler);
  }

  /// <summary>Removes an event handler from the list of handlers.</summary>
  /// <seealso cref="GameLib.Events.EventProcedure"/>
  public static void RemoveEventHandler(EventProcedure handler)
  {
    if(handler == null) throw new ArgumentNullException();
    eventProcs.Remove(handler);
  }

  /// <summary>Adds an idle procedure to the end of the list, so that it will be executed after the rest of the idle
  /// procedures in the list.
  /// </summary>
  /// <seealso cref="GameLib.Events.IdleProcedure"/>
  public static void AppendIdleProcedure(IdleProcedure idleProc)
  {
    if(idleProc == null) throw new ArgumentNullException();
    idleProcs.Add(idleProc);
  }

  /// <summary>Adds an idle procedure to the beginning of the list, so that it will be executed before the rest of the
  /// idle procedures in the list.
  /// </summary>
  /// <seealso cref="GameLib.Events.IdleProcedure"/>
  public static void PrependIdleProcedure(IdleProcedure idleProc)
  {
    if(idleProc == null) throw new ArgumentNullException();
    idleProcs.Insert(0, idleProc);
  }

  /// <summary>Removes an idle procedure from the list of idle procedures.</summary>
  /// <seealso cref="GameLib.Events.IdleProcedure"/>
  public static void RemoveIdleProcedure(IdleProcedure idleProc)
  {
    if(idleProc == null) throw new ArgumentNullException();
    idleProcs.Remove(idleProc);
  }

  /// <summary>Returns the next event from the queue, optionally removing it as well.</summary>
  /// <param name="timeout">The amount of time to wait for an event. <see cref="Infinite"/> can be passed to
  /// specify an infinite timeout.
  /// </param>
  /// <param name="remove">Set to true if the event is to be removed, and false if not.</param>
  /// <returns>The next event in the queue, or null if no events arrive in the specified timeout period.</returns>
  /// <exception cref="InvalidOperationException">Thrown if the event subsystem has not been initialized.</exception>
  public static Event GetEvent(int timeout, bool remove)
  {
    AssertInit();
    if(timeout != Infinite && timeout < 0) throw new ArgumentOutOfRangeException();

    lock(queue)
    {
      if(queue.Count != 0) return remove ? queue.Dequeue() : queue.Peek();
    }

    Event ret = null;

    if(timeout == Infinite)
    {
      do
      {
        ret = NextSDLEvent();
        lock(queue)
        {
          if(ret == null) // the only time NextSDLEvent() returns null is after an event was pushed using PushEvent()
          {
            if(queue.Count != 0) ret = remove ? queue.Dequeue() : queue.Peek();
          }
          else if(!remove) QueueEvent(ret);
        }
      } while(ret == null);
    }
    else
    {
      uint start = SDL.GetTicks();
      while(true)
      {
        ret = PollSDLEvent();
        if(ret != null)
        {
          lock(queue)
          {
            if(!remove) QueueEvent(ret);
            break;
          }
        }

        if(timeout == 0) break;

        System.Threading.Thread.Sleep(Math.Min(5, timeout)); // i don't like this, but SDL doesn't provide a
        timeout -= (int)(SDL.GetTicks()-start);              // version that waits for an arbitrary timeout

        if(timeout <= 0) break;

        if(queue.Count != 0) // this may happen if an event is pushed using PushEvent()
        {
          lock(queue)
          {
            if(queue.Count != 0) return remove ? queue.Dequeue() : queue.Peek();
          }
        }
      }
    }

    return ret;
  }

  /// <summary>Returns the next event without removing it from the queue.</summary>
  /// <returns>The next event in the queue, or null if the queue is empty.</returns>
  /// <remarks>Calling this method is equivalent to calling <see cref="GetEvent(int,bool)"/> with a timeout of
  /// zero and passing false to signal that the event is not to be removed from the queue.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if the event subsystem has not been initialized.</exception>
  public static Event PeekEvent() 
  {
    return GetEvent(0, false); 
  }

  /// <summary>Removes and returns the next event from the queue.</summary>
  /// <returns>The next event in the queue.</returns>
  /// <remarks>Calling this method is equivalent to calling <see cref="GetEvent(int,bool)"/> with a timeout of
  /// <see cref="Infinite"/> and passing true to signal that the event should be removed from the queue.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if the event subsystem has not been initialized.</exception>
  public static Event NextEvent()
  {
    return GetEvent(Infinite, true);
  }

  /// <summary>Returns the next event without removing it from the queue.</summary>
  /// <param name="timeout">The amount of time to wait for an event. <see cref="Infinite"/> can be passed to
  /// specify an infinite timeout.
  /// </param>
  /// <returns>The next event in the queue, or null if no events arrive in the specified timeout period.</returns>
  /// <remarks>Calling this method is equivalent to calling <see cref="GetEvent(int,bool)"/> and passing false to
  /// signal that the event is not to be removed from the queue.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if the event subsystem has not been initialized.</exception>
  public static Event PeekEvent(int timeout) 
  { 
    return GetEvent(timeout, false); 
  }

  /// <summary>Removes and returns the next event from the queue.</summary>
  /// <param name="timeout">The amount of time to wait for an event. <see cref="Infinite"/> can be passed to
  /// specify an infinite timeout.
  /// </param>
  /// <returns>The next event in the queue, or null if no events arrive in the specified timeout period.</returns>
  /// <remarks>Calling this method is equivalent to calling <see cref="GetEvent(int,bool)"/> and passing true to
  /// signal that the event should be removed from the queue.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if the event subsystem has not been initialized.</exception>
  public static Event NextEvent(int timeout)
  { 
    return GetEvent(timeout, true);
  }

  /// <summary>This method adds an event to the queue without passing it through any event filters.</summary>
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
  { 
    if(evt == null) throw new ArgumentNullException();
    AssertInit();
    if(filter && !ShouldAllowEvent(evt)) return false;

    lock(queue)
    {
      // first add any pending SDL events to the queue, because they belong there first
      Event sdlEvent;
      while((sdlEvent=PollSDLEvent()) != null) QueueEvent(sdlEvent);

      // then add the new event
      QueueEvent(evt);

      // this is done so that if the code is stuck in the NextSDLEvent() call in NextEvent(), it will wake up
      SDL.Event e = new SDL.Event();
      e.Type = SDL.EventType.UserEvent0;
      SDL.PushEvent(ref e);
    }

    return true;
  }

  /// <summary>Processes all events that are waiting in the queue, and returns the complement of the quit flag.</summary>
  /// <remarks>This method essentially calls <see cref="PumpEvent"/> repeatedly until it returns false.</remarks>
  public static bool PumpAvailableEvents()
  {
    while(PumpEvent(false)) { }
    return !QuitFlag;
  }

  /// <summary>Handles the next event.</summary>
  /// <param name="waitForEvent">Whether the method should wait for an event to process if the event queue is currently
  /// empty.
  /// </param>
  /// <returns>Returns true if an event was handled and false if not.</returns>
  /// <remarks>
  /// <para>
  /// If <see cref="EventProcedure">EventProcedures</see> have been registered, they will be called to process events.
  /// Otherwise, the default event processing will occur, which ignores all messages except quit messages, which set
  /// the <see cref="QuitFlag"/> to true. If <paramref name="waitForEvent"/> and the <see cref="QuitFlag"/> are true,
  /// this method will return immediately without doing anything. Otherwise, if <paramref name="waitForEvent"/> is
  /// true, this method will wait until an event arrives, calling the <see cref="IdleProcedure"/> methods, if any,
  /// until all return false as long as no events are waiting in the queue.
  /// After an event arrives, it is processed with the registered <see cref="EventProcedure"/> methods or the default
  /// behavior described above. If any event procedure returns false, the <see cref="QuitFlag"/> is set to true and
  /// execution of further event procedures is not done.
  /// </para>
  /// See <see cref="GameLib.Events.EventProcedure"/> for more information.
  /// </remarks>
  public static bool PumpEvent(bool waitForEvent)
  { 
    if(waitForEvent && QuitFlag) return false;

    Event e = NextEvent(0);
    if(e == null)
    { 
      if(idleProcs.Count != 0)
      { 
        bool morework;
        do
        { 
          morework = false;
          foreach(IdleProcedure idleProc in idleProcs)
          {
            if(idleProc()) morework = true;
          }
        } while(morework && !QuitFlag && PeekEvent(0) == null);
      }
      if(waitForEvent) e = NextEvent();
    }

    if(e != null)
    {
      if(eventProcs.Count != 0)
      {
        foreach(EventProcedure eventProc in eventProcs)
        {
          if(!eventProc(e))
          {
            QuitFlag = true;
            break;
          }
        }
      }
      else if(e.Type == EventType.Quit) QuitFlag = true;
    }

    return e != null;
  }

  /// <summary>Handle events until the application decides to quit.</summary>
  /// <remarks>Calling this method is equivalent to calling <see cref="PumpEvents(EventProcedure,IdleProcedure)"/>
  /// and passing null for both parameters.
  /// </remarks>
  public static void PumpEvents()
  {
    PumpEvents(null, null); 
  }

  /// <summary>Handle events until the application decides to quit.</summary>
  /// <param name="proc">A <see cref="GameLib.Events.EventProcedure"/> that will be prepended to the list of event
  /// handlers, or null to not add any.
  /// </param>
  /// <remarks>Calling this method is equivalent to calling <see cref="PumpEvents(EventProcedure,IdleProcedure)"/>
  /// and passing null for the <see cref="IdleProcedure"/> parameter.
  /// </remarks>
  public static void PumpEvents(EventProcedure proc) 
  {
    PumpEvents(proc, null); 
  }

  /// <summary>Handle events until the application decides to quit.</summary>
  /// <param name="proc">A <see cref="GameLib.Events.EventProcedure"/> that will be prepended to the list of event
  /// handlers, or null to not add any.
  /// </param>
  /// <param name="idle">A <see cref="GameLib.Events.IdleProcedure"/> that will be prepended to the list of idle
  /// procedures, or null to not add any.
  /// </param>
  /// <remarks>
  /// <para>
  /// If <see cref="EventProcedure">EventProcedures</see> have been registered, they will be called to process events.
  /// Otherwise, the default event processing will occur, which ignores all messages except quit messages, which set
  /// the <see cref="QuitFlag"/> to true.
  /// </para>
  /// <para>
  /// If the <see cref="QuitFlag"/> is true, this method will return immediately without doing anything. Otherwise,
  /// this method will wait until an event arrives, calling the <see cref="IdleProcedure"/> methods until all return
  /// false as long as no events are waiting in the queue. After an event arrives, it is processed with the registered
  /// <see cref="EventProcedure"/> methods or the default event processing described above. If any event procedure
  /// returns false, the <see cref="QuitFlag"/> is set to true and processing of further event procedures will not be
  /// done. If the <see cref="QuitFlag"/> is false after that, the whole process is repeated. If it's true, the
  /// method will return. See <see cref="EventProcedure"/> for more information.
  /// </para>
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if no event procedure has been registered.</exception>
  public static void PumpEvents(EventProcedure proc, IdleProcedure idle)
  {
    try
    {
      if(proc != null) PrependEventHandler(proc);
      if(idle != null) PrependIdleProcedure(idle);
      while(PumpEvent(true)) { }
    }
    finally
    {
      if(idle != null) RemoveIdleProcedure(idle);
      if(proc != null) RemoveEventHandler(proc);
    }
  }

  /// <summary>Retrieves the next SDL event that passes the event filters if there's one waiting, or null if there is
  /// not.
  /// </summary>
  static unsafe Event PollSDLEvent()
  { 
    Event ret = null;
    SDL.Event evt;
    do
    {
      if(SDL.PollEvent(out evt) != 0) // if there's an SDL event waiting
      {
        ret = ConvertEvent(ref evt);
        if(!ShouldAllowEvent(ret)) ret = null;
      }
      else break;
    } while(ret == null);
    return ret;
  }

  /// <summary>Waits for and retrieves the next SDL event that passes the event filters.</summary>
  static unsafe Event NextSDLEvent()
  { 
    Event ret;
    SDL.Event evt;
    while(true)
    { 
      if(SDL.WaitEvent(out evt) != 0)
      {
        // the UserEvent0 event is used to wake up this method when an event is pushed into the queue with PushEvent()
        if(evt.Type == SDL.EventType.UserEvent0) return null;

        ret = ConvertEvent(ref evt);
        if(ShouldAllowEvent(ret)) return ret;
      }
    }
  }

  static void AssertInit()
  { 
    if(initCount == 0) throw new InvalidOperationException("Events not initialized yet"); 
  }

  /// <summary>Converts an SDL event into a GameLib event.</summary>
  static unsafe Event ConvertEvent(ref SDL.Event evt)
  {
    switch(evt.Type)
    { 
      case SDL.EventType.Active:
      { 
        FocusEvent e = new FocusEvent(ref evt.Active);

        if((e.FocusType&FocusType.Application) != 0) minimized = e.Focused;
        if((e.FocusType&FocusType.Input) != 0) inputFocus = e.Focused;
        if((e.FocusType&FocusType.Mouse) != 0) mouseFocus = e.Focused;

        // unset keyboard mods when regaining application focus, to prevent mod keys from seeming stuck
        if(e.Focused && (e.FocusType&(FocusType.Application|FocusType.Input))!=0) mods &= ~KeyMod.KeyMask;
        return e;
      }

      case SDL.EventType.KeyDown: case SDL.EventType.KeyUp:
      { 
        KeyboardEvent e = new KeyboardEvent(ref evt.Keyboard);
        mods = mods & ~KeyMod.StatusMask | e.StatusMods;
        if(e.Down)
        {
          switch(e.Key) // SDL's mod handling is quirky, so i'll do it myself
          { 
            case Key.LeftShift: mods |= KeyMod.LeftShift; break;
            case Key.RightShift: mods |= KeyMod.RightShift; break;
            case Key.LeftCtrl:  mods |= KeyMod.LeftCtrl;  break;
            case Key.RightCtrl:  mods |= KeyMod.RightCtrl;  break;
            case Key.LeftAlt:   mods |= KeyMod.LeftAlt;   break;
            case Key.RightAlt:   mods |= KeyMod.RightAlt;   break;
            case Key.LeftMeta:  mods |= KeyMod.LeftMeta;  break;
            case Key.RightMeta:  mods |= KeyMod.RightMeta;  break;
          }
        }
        else
        {
          switch(e.Key)
          { 
            case Key.LeftShift: mods &= ~KeyMod.LeftShift; break;
            case Key.RightShift: mods &= ~KeyMod.RightShift; break;
            case Key.LeftCtrl:  mods &= ~KeyMod.LeftCtrl;  break;
            case Key.RightCtrl:  mods &= ~KeyMod.RightCtrl;  break;
            case Key.LeftAlt:   mods &= ~KeyMod.LeftAlt;   break;
            case Key.RightAlt:   mods &= ~KeyMod.RightAlt;   break;
            case Key.LeftMeta:  mods &= ~KeyMod.LeftMeta;  break;
            case Key.RightMeta:  mods &= ~KeyMod.RightMeta;  break;
          }
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
      case SDL.EventType.UserEvent0: return null;
      default: return null;
    }
  }

  /// <summary>Adds an event to the queue, first dequeueing the oldest event if the queue is full.</summary>
  static void QueueEvent(Event evt)
  { 
    if(queue.Count >= maxEvents) queue.Dequeue();
    queue.Enqueue(evt);
  }

  /// <summary>Returns true if the event should be allowed into the queue and false if not.</summary>
  static bool ShouldAllowEvent(Event evt)
  {
    if(evt == null) return false;

    if(filters.Count != 0)
    {
      foreach(EventFilter ef in filters)
      {
        switch(ef(evt))
        {
          case FilterAction.Drop: return false;
          case FilterAction.Queue: break;
        }
      }
    }

    return true;
  }

  internal static KeyMod mods;
  internal static bool minimized=true, inputFocus=true, mouseFocus=true;

  static System.Collections.Generic.Queue<Event> queue = new System.Collections.Generic.Queue<Event>();
  static readonly List<EventFilter> filters = new List<EventFilter>(4);
  static readonly List<EventProcedure> eventProcs = new List<EventProcedure>(4);
  static readonly List<IdleProcedure> idleProcs = new List<IdleProcedure>(4);
  static uint initCount;
  static int  maxEvents = 128;
  static bool quit;
}
#endregion

} // namespace GameLib.Events