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

using System;
using GameLib.Interop.SDL;

namespace GameLib.Events
{

#region Event definitions
[Flags]
public enum EventType
{ Focus, Keyboard, MouseMove, MouseClick, JoyMove, JoyBall, JoyHat, JoyButton, Quit,
  Resize, Repaint, Exception, Window, UserDefined
}

public enum FocusType
{ Mouse=SDL.FocusType.MouseFocus, Input=SDL.FocusType.InputFocus, Application=SDL.FocusType.AppActive
};

public enum HatPosition : byte
{ Center=SDL.HatPos.Centered, Up=SDL.HatPos.Up, Down=SDL.HatPos.Down, Left=SDL.HatPos.Left, Right=SDL.HatPos.Right,
  UpLeft=SDL.HatPos.UpLeft, UpRight=SDL.HatPos.UpRight, DownLeft=SDL.HatPos.DownLeft, DownRight=SDL.HatPos.DownRight
}

public abstract class Event
{ protected Event(EventType type) { this.type=type; }
  public EventType Type { get { return type; } }
  public EventType type;
}

public class FocusEvent : Event
{ public FocusEvent() : base(EventType.Focus) { }
  internal FocusEvent(ref SDL.ActiveEvent evt) : base(EventType.Focus)
  { FocusType=(FocusType)evt.State; Focused=(evt.Focused!=0);
  }
  public FocusType FocusType;
  public bool      Focused;
}

public class KeyboardEvent : Event
{ public KeyboardEvent() : base(EventType.Keyboard) { }
  internal KeyboardEvent(ref SDL.KeyboardEvent evt) : base(EventType.Keyboard)
  { Key  = (Input.Key)evt.Key.Sym;
    Mods = (Input.KeyMod)evt.Key.Mod;
    Char = evt.Key.Unicode;
    Scan = evt.Key.Scan;
    Down = evt.Down!=0;
  }
  public bool IsModKey { get { return Input.Keyboard.IsModKey(Key); } }
  public bool HasAnyMod  (Input.KeyMod mod) { return (Mods&mod)!=Input.KeyMod.None; }
  public bool HasAllMods (Input.KeyMod mod) { return (Mods&mod)==mod; }
  public bool HasAllKeys (Input.KeyMod mod) { return (KeyMods&mod)==mod; }
  public bool HasOnlyKeys(Input.KeyMod mod)
  { Input.KeyMod mods = KeyMods;
    return (mods&mod)!=Input.KeyMod.None && (mods&~mod)==Input.KeyMod.None;
  }
  public Input.KeyMod KeyMods    { get { return Mods&Input.KeyMod.KeyMask; } }
  public Input.KeyMod StatusMods { get { return Mods&Input.KeyMod.StatusMask; } }

  public Input.Key    Key;
  public Input.KeyMod Mods;
  public char Char;
  public byte Scan;
  public bool Down;
}

public class MouseMoveEvent : Event
{ public MouseMoveEvent() : base(EventType.MouseMove) { }
  internal MouseMoveEvent(ref SDL.MouseMoveEvent evt) : base(EventType.MouseMove)
  { X=(int)evt.X; Y=(int)evt.Y; Xrel=(int)evt.Xrel; Yrel=(int)evt.Yrel; Buttons=evt.State;
  }
  public bool OnlyPressed(Input.MouseButton button) { return Buttons==(1<<(byte)button); }
  public bool Pressed(Input.MouseButton button) { return (Buttons&(1<<(byte)button))!=0; }
  public void SetPressed(Input.MouseButton button, bool down)
  { if(down) Buttons|=(byte)(1<<(byte)button); else Buttons&=(byte)~(1<<(byte)button);
  }
  public System.Drawing.Point Point
  { get { return new System.Drawing.Point(X, Y); }
    set { X=value.X; Y=value.Y; }
  }
  public System.Drawing.Size Offset
  { get { return new System.Drawing.Size(Xrel, Yrel); }
    set { Xrel=value.Width; Yrel=value.Height; }
  }

  public int X, Y, Xrel, Yrel;
  public byte Buttons;
}

public class MouseClickEvent : Event
{ public MouseClickEvent() : base(EventType.MouseClick) { }
  internal MouseClickEvent(ref SDL.MouseButtonEvent evt) : base(EventType.MouseClick)
  { Button=(Input.MouseButton)(evt.Button-1); Down=(evt.Down!=0); X=(int)evt.X; Y=(int)evt.Y;
  }
  public bool MouseWheel
  { get { return Button==Input.MouseButton.WheelDown || Button==Input.MouseButton.WheelUp; }
  }
  public System.Drawing.Point Point
  { get { return new System.Drawing.Point(X, Y); }
    set { X=value.X; Y=value.Y; }
  }
  public Input.MouseButton Button;
  public bool Down;
  public int  X, Y;
}

public class JoyMoveEvent : Event
{ public JoyMoveEvent() : base(EventType.JoyMove) { }
  internal JoyMoveEvent(ref SDL.JoyAxisEvent evt) : base(EventType.JoyMove)
  { Device=evt.Device; Axis=evt.Axis; Position=(int)evt.Value;
  }
  public byte Device, Axis;
  public int  Position;
}

public class JoyBallEvent : Event
{ public JoyBallEvent() : base(EventType.JoyBall) { }
  internal JoyBallEvent(ref SDL.JoyBallEvent evt) : base(EventType.JoyBall)
  { Device=evt.Device; Ball=evt.Ball; Xrel=(int)evt.Xrel; Yrel=(int)evt.Yrel;
  }
  public byte Device, Ball;
  public int  Xrel, Yrel;
}

public class JoyHatEvent : Event
{ public JoyHatEvent() : base(EventType.JoyHat) { }
  internal JoyHatEvent(ref SDL.JoyHatEvent evt) : base(EventType.JoyHat)
  { Device=evt.Device; Hat=evt.Hat; Position=(HatPosition)evt.Position;
  }
  public byte Device, Hat;
  public HatPosition Position;
}

public class JoyButtonEvent : Event
{ public JoyButtonEvent() : base(EventType.JoyButton) { }
  internal JoyButtonEvent(ref SDL.JoyButtonEvent evt) : base(EventType.JoyButton)
  { Device=evt.Device; Button=evt.Button; Down=(evt.Down!=0);
  }
  public byte Device, Button;
  public bool Down;
}

public class QuitEvent : Event
{ public QuitEvent() : base(EventType.Quit) { }
}

public class ResizeEvent : Event
{ public ResizeEvent() : base(EventType.Resize) { }
  internal ResizeEvent(ref SDL.ResizeEvent evt) : base(EventType.Resize)
  { Width=evt.Width; Height=evt.Height;
  }
  public int Width, Height;
}

public class RepaintEvent : Event
{ public RepaintEvent() : base(EventType.Repaint) { }
}

public abstract class WindowEvent : Event
{ public WindowEvent(GameLib.Forms.Control control) : this(control, MessageType.Custom) { }
  public WindowEvent(GameLib.Forms.Control control, MessageType subType) : base(EventType.Window)
  { if(control==null) throw new ArgumentNullException("control");
    Control=control; SubType=subType;
  }
  public enum MessageType { Custom, Paint, Layout, KeyRepeat, DesktopUpdated };
  public GameLib.Forms.Control Control;
  public MessageType SubType;
}

public class WindowPaintEvent : WindowEvent
{ public WindowPaintEvent(GameLib.Forms.Control control) : base(control, WindowEvent.MessageType.Paint) { }
}

public class WindowLayoutEvent : WindowEvent
{ public WindowLayoutEvent(GameLib.Forms.Control control, bool recursive)
    : base(control, WindowEvent.MessageType.Layout) { Recursive=recursive; }
  public bool Recursive;
}

public class KeyRepeatEvent : WindowEvent
{ public KeyRepeatEvent(GameLib.Forms.DesktopControl desktop) : base(desktop, WindowEvent.MessageType.KeyRepeat) { }
}

public class DesktopUpdatedEvent : WindowEvent
{ public DesktopUpdatedEvent(GameLib.Forms.DesktopControl desktop)
    : base(desktop, WindowEvent.MessageType.DesktopUpdated) { }
}

public class UserEvent : Event
{ public UserEvent() : base(EventType.UserDefined) { }
}

public enum ExceptionLocation { Unknown, AudioThread }; // TODO: is this being used?

public class ExceptionEvent : Event
{ public ExceptionEvent(ExceptionLocation loc, Exception e) : base(EventType.Exception) { location=loc; ex=e; }
  
  public ExceptionLocation Location { get { return location; } }
  public Exception Exception { get { return ex; } }
  
  protected ExceptionLocation location;
  protected Exception ex;
}
#endregion

#region Events class
public enum FilterAction { Continue, Drop, Queue }
public delegate FilterAction EventFilter(Event evt);
public delegate bool EventProcedure(Event evt);
public delegate bool IdleProcedure();

public sealed class Events
{ private Events() { }
  
  public const int Infinite=-1;

  public static event EventFilter EventFilter;
  public static event EventProcedure EventProcedure;
  public static event IdleProcedure IdleProcedure;

  public static bool Initialized { get { return initCount>0; } }
  public static int  MaxQueueSize
  { get { return max; }
    set
    { if(value<1) throw new ArgumentOutOfRangeException("value", value, "must be greater than zero");
      if(value<max) while(queue.Count>value) queue.Dequeue();
      max=value;
    }
  }
  public static object SyncRoot { get { return queue; } }

  public static int  QueueSize { get { return queue.Count; } }
  public static bool QuitFlag { get { return quit; } set { quit=value; } }

  public static void Initialize()
  { if(initCount++==0)
    { SDL.Initialize(SDL.InitFlag.Video);
      SDL.Initialize(SDL.InitFlag.EventThread); // SDL quirk: this must be done AFTER video
      SDL.EnableUNICODE(1); // SDL quirk: must be AFTER Initialize(EventThread)
    }
  }

  public static void Deinitialize()
  { if(initCount==0) throw new InvalidOperationException("Deinitialize called too many times!");
    if(--initCount==0)
      lock(queue)
      { SDL.Deinitialize(SDL.InitFlag.Video|SDL.InitFlag.EventThread);
        queue.Clear();
        waiting = 0;
      }
  }

  public static bool UpdateQueue()
  { lock(queue)
    { AssertInit();
      Event evt;
      bool  ret=false;
      while(true)
      { evt = PeekSDLEvent();
        if(evt==null) return ret;
        QueueEvent(evt);
        ret = true;
      }
    }
  }

  public static Event PeekEvent() { return NextEvent(0, false); }
  public static Event NextEvent() { return NextEvent(Infinite, true);  }
  public static Event PeekEvent(int timeout) { return NextEvent(timeout, false); }
  public static Event NextEvent(int timeout) { return NextEvent(timeout, true);  }
  public static Event NextEvent(int timeout, bool remove)
  { AssertInit();

    lock(queue)
    { if(waiting>0) waiting--;
      if(queue.Count>0) return (Event)(remove ? queue.Dequeue() : queue.Peek());
    }

    Event ret;

    if(timeout==Infinite)
    { ret = NextSDLEvent();
      lock(queue)
      { if(ret==null) { waiting--; return (Event)(remove ? queue.Dequeue() : queue.Peek()); }
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
          if(waiting>0) { waiting--; return (Event)(remove ? queue.Dequeue() : queue.Peek()); }
          return ret;
        }
      }
      System.Threading.Thread.Sleep(10);
      timeout -= (int)(SDL.GetTicks()-start);
    } while(timeout>0);
    return null;
  }

  public static bool PushEvent(Event evt) { return PushEvent(evt, false); }
  public static bool PushEvent(Event evt, bool filter)
  { if(evt==null) throw new ArgumentNullException("evt");
    AssertInit();
    if(initCount==0) return false;
    if(filter && !FilterEvent(evt)) return false;
    lock(queue)
    { QueueEvent(evt);
      waiting++;
      SDL.Event e = new SDL.Event();
      e.Type = SDL.EventType.UserEvent0;
      SDL.PushEvent(ref e);
    }

    return true;
  }
  
  public static bool PumpEvent()
  { if(EventProcedure==null) throw new InvalidOperationException("No event procedure has been registered");
    if(quit) return false;
    Event e = NextEvent(0);
    if(e==null)
    { if(IdleProcedure!=null) while(IdleProcedure() && !quit && PeekEvent(0)==null);
      e = NextEvent();
    }
    quit = !EventProcedure(e) || quit;
    return !quit;
  }

  public static void PumpEvents() { PumpEvents(null); }

  public static void PumpEvents(EventProcedure proc) { PumpEvents(proc, null); }
  public static void PumpEvents(EventProcedure proc, IdleProcedure idle)
  { if(proc!=null) EventProcedure += proc;
    if(idle!=null) IdleProcedure += idle;
    if(EventProcedure==null) throw new InvalidOperationException("No event procedure has been registered");
    while(!quit)
    { Event e = NextEvent(0);
      if(e==null)
      { if(IdleProcedure!=null) while(IdleProcedure() && !quit && PeekEvent(0)==null);
        if(quit || !EventProcedure(NextEvent())) break;
      }
      else if(!EventProcedure(e)) break;
    }
    quit=true;
  }
  
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
    { Delegate[] list = EventFilter.GetInvocationList();
      foreach(EventFilter ef in list)
        switch(ef(evt))
        { case FilterAction.Drop:  return false;
          case FilterAction.Queue: break;
        }
    }
    return true;
  }
  
  static unsafe Event ConvertEvent(ref SDL.Event evt)
  { switch(evt.Type)
    { case SDL.EventType.Active: return new FocusEvent(ref evt.Active);
      case SDL.EventType.KeyDown: case SDL.EventType.KeyUp: return new KeyboardEvent(ref evt.Keyboard);
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

  static System.Collections.Queue queue = new System.Collections.Queue();
  static UserEvent userEvent = new UserEvent();
  static uint initCount, waiting;
  static int  max=512;
  static bool quit;
}
#endregion

} // namespace GameLib.Events