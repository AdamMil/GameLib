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
using GameLib.Events;

namespace GameLib.Input
{

#region Types
public enum MouseButton : byte { Left=0, Middle=1, Right=2, WheelUp=3, WheelDown=4 }

#region Key enum
public enum Key : int
{ Unknown         =SDL.Key.Unknown, None=Unknown,
  
  /* These are mapped to ascii -- BEGIN ASCII MAPPED SECTION */
  Backspace       =SDL.Key.Backspace,
  Tab             =SDL.Key.Tab,
  Clear           =SDL.Key.Clear,
  Enter           =SDL.Key.Return,
  Pause           =SDL.Key.Pause,
  Escape          =SDL.Key.Escape,
  Space           =SDL.Key.Space,
  Exclaim         =SDL.Key.Exclaim,
  DoubleQuote     =SDL.Key.Quotedbl,
  Hash            =SDL.Key.Hash,
  Dollar          =SDL.Key.Dollar,
  Ampersand       =SDL.Key.Ampersand,
  Quote           =SDL.Key.Quote,
  LeftParen       =SDL.Key.LeftParen,
  RightParen      =SDL.Key.RightParen,
  Asterisk        =SDL.Key.Asterisk,
  Plus            =SDL.Key.Plus,
  Comma           =SDL.Key.Comma,
  Minus           =SDL.Key.Minus,
  Period          =SDL.Key.Period,
  Slash           =SDL.Key.Slash,
  Num0            =SDL.Key.Num0,
  Num1            =SDL.Key.Num1,
  Num2            =SDL.Key.Num2,
  Num3            =SDL.Key.Num3,
  Num4            =SDL.Key.Num4,
  Num5            =SDL.Key.Num5,
  Num6            =SDL.Key.Num6,
  Num7            =SDL.Key.Num7,
  Num8            =SDL.Key.Num8,
  Num9            =SDL.Key.Num9,
  Colon           =SDL.Key.Colon,
  Semicolon       =SDL.Key.Semicolon,
  Less            =SDL.Key.Less,
  Equals          =SDL.Key.Equals,
  Greater         =SDL.Key.Greater,
  Question        =SDL.Key.Question,
  At              =SDL.Key.At,

  /* The alphabetical keys reference the lowercase ASCII values letters. Subtract 32 to get the uppercase values */
  A               =SDL.Key.A,
  B               =SDL.Key.B,
  C               =SDL.Key.C,
  D               =SDL.Key.D,
  E               =SDL.Key.E,
  F               =SDL.Key.F,
  G               =SDL.Key.G,
  H               =SDL.Key.H,
  I               =SDL.Key.I,
  J               =SDL.Key.J,
  K               =SDL.Key.K,
  L               =SDL.Key.L,
  M               =SDL.Key.M,
  N               =SDL.Key.N,
  O               =SDL.Key.O,
  P               =SDL.Key.P,
  Q               =SDL.Key.Q,
  R               =SDL.Key.R,
  S               =SDL.Key.S,
  T               =SDL.Key.T,
  U               =SDL.Key.U,
  V               =SDL.Key.V,
  W               =SDL.Key.W,
  X               =SDL.Key.X,
  Y               =SDL.Key.Y,
  Z               =SDL.Key.Z,

  LeftBracket     =SDL.Key.LeftBracket,
  Backslash       =SDL.Key.Backslash,
  RightBracket    =SDL.Key.RightBracket,
  Caret           =SDL.Key.Caret,
  Underscore      =SDL.Key.Underscore,
  Backquote       =SDL.Key.Backquote,
  
  Delete          =SDL.Key.Delete,
  /* END ASCII MAPPED SECTION */

  /* International keysyms */
  World0=SDL.Key.World_0,   World1=SDL.Key.World_1,   World2=SDL.Key.World_2,   World3=SDL.Key.World_3,
  World4=SDL.Key.World_4,   World5=SDL.Key.World_5,   World6=SDL.Key.World_6,   World7=SDL.Key.World_7,
  World8=SDL.Key.World_8,   World9=SDL.Key.World_9,   World10=SDL.Key.World_10, World11=SDL.Key.World_11,
  World12=SDL.Key.World_12, World13=SDL.Key.World_13, World14=SDL.Key.World_14, World15=SDL.Key.World_15,
  World16=SDL.Key.World_16, World17=SDL.Key.World_17, World18=SDL.Key.World_18, World19=SDL.Key.World_19,
  World20=SDL.Key.World_20, World21=SDL.Key.World_21, World22=SDL.Key.World_22, World23=SDL.Key.World_23,
  World24=SDL.Key.World_24, World25=SDL.Key.World_25, World26=SDL.Key.World_26, World27=SDL.Key.World_27,
  World28=SDL.Key.World_28, World29=SDL.Key.World_29, World30=SDL.Key.World_30, World31=SDL.Key.World_31,
  World32=SDL.Key.World_32, World33=SDL.Key.World_33, World34=SDL.Key.World_34, World35=SDL.Key.World_35,
  World36=SDL.Key.World_36, World37=SDL.Key.World_37, World38=SDL.Key.World_38, World39=SDL.Key.World_39,
  World40=SDL.Key.World_40, World41=SDL.Key.World_41, World42=SDL.Key.World_42, World43=SDL.Key.World_43,
  World44=SDL.Key.World_44, World45=SDL.Key.World_45, World46=SDL.Key.World_46, World47=SDL.Key.World_47,
  World48=SDL.Key.World_48, World49=SDL.Key.World_49, World50=SDL.Key.World_50, World51=SDL.Key.World_51,
  World52=SDL.Key.World_52, World53=SDL.Key.World_53, World54=SDL.Key.World_54, World55=SDL.Key.World_55,
  World56=SDL.Key.World_56, World57=SDL.Key.World_57, World58=SDL.Key.World_58, World59=SDL.Key.World_59,
  World60=SDL.Key.World_60, World61=SDL.Key.World_61, World62=SDL.Key.World_62, World63=SDL.Key.World_63,
  World64=SDL.Key.World_64, World65=SDL.Key.World_65, World66=SDL.Key.World_66, World67=SDL.Key.World_67,
  World68=SDL.Key.World_68, World69=SDL.Key.World_69, World70=SDL.Key.World_70, World71=SDL.Key.World_71,
  World72=SDL.Key.World_72, World73=SDL.Key.World_73, World74=SDL.Key.World_74, World75=SDL.Key.World_75,
  World76=SDL.Key.World_76, World77=SDL.Key.World_77, World78=SDL.Key.World_78, World79=SDL.Key.World_79,
  World80=SDL.Key.World_80, World81=SDL.Key.World_81, World82=SDL.Key.World_82, World83=SDL.Key.World_83,
  World84=SDL.Key.World_84, World85=SDL.Key.World_85, World86=SDL.Key.World_86, World87=SDL.Key.World_87,
  World88=SDL.Key.World_88, World89=SDL.Key.World_89, World90=SDL.Key.World_90, World91=SDL.Key.World_91,
  World92=SDL.Key.World_92, World93=SDL.Key.World_93, World94=SDL.Key.World_94, World95=SDL.Key.World_95,

  /* The keypad */
  Kp0             =SDL.Key.KP0,
  Kp1             =SDL.Key.KP1,
  Kp2             =SDL.Key.KP2,
  Kp3             =SDL.Key.KP3,
  Kp4             =SDL.Key.KP4,
  Kp5             =SDL.Key.KP5,
  Kp6             =SDL.Key.KP6,
  Kp7             =SDL.Key.KP7,
  Kp8             =SDL.Key.KP8,
  Kp9             =SDL.Key.KP9,
  KpPeriod        =SDL.Key.KP_Period,
  KpDivide        =SDL.Key.KP_Divide,
  KpMultiply      =SDL.Key.KP_Multiply,
  KpMinus         =SDL.Key.KP_Minus,
  KpPlus          =SDL.Key.KP_Plus,
  KpEnter         =SDL.Key.KP_Enter,
  KpEquals        =SDL.Key.KP_Equals,

  /* The navigation blocks */
  Up              =SDL.Key.Up,
  Down            =SDL.Key.Down,
  Right           =SDL.Key.Right,
  Left            =SDL.Key.Left,
  Insert          =SDL.Key.Insert,
  Home            =SDL.Key.Home,
  End             =SDL.Key.End,
  PageUp          =SDL.Key.PageUp,
  PageDown        =SDL.Key.PageDown,

  /* Function keys */
  F1              =SDL.Key.F1,
  F2              =SDL.Key.F2,
  F3              =SDL.Key.F3,
  F4              =SDL.Key.F4,
  F5              =SDL.Key.F5,
  F6              =SDL.Key.F6,
  F7              =SDL.Key.F7,
  F8              =SDL.Key.F8,
  F9              =SDL.Key.F9,
  F10             =SDL.Key.F10,
  F11             =SDL.Key.F11,
  F12             =SDL.Key.F12,
  F13             =SDL.Key.F13,
  F14             =SDL.Key.F14,
  F15             =SDL.Key.F15,

  /* Modifier keys */
  NumLock         =SDL.Key.NumLock,
  CapsLock        =SDL.Key.CapsLock,
  ScrollLock      =SDL.Key.ScrollLock,
  RShift          =SDL.Key.RShift,
  LShift          =SDL.Key.LShift,
  RCtrl           =SDL.Key.RCtrl,
  LCtrl           =SDL.Key.LCtrl,
  RAlt            =SDL.Key.RAlt,
  LAlt            =SDL.Key.LAlt,
  RMeta           =SDL.Key.RMeta,
  LMeta           =SDL.Key.LMeta,
  LSuper          =SDL.Key.LSuper, // left Windows key
  RSuper          =SDL.Key.RSuper, // right Windows key
  Mode            =SDL.Key.Mode,
  Compose         =SDL.Key.Compose,

  /* Additional function keys */
  Help            =SDL.Key.Help,
  Print           =SDL.Key.Print,
  Sysreq          =SDL.Key.Sysreq,
  Break           =SDL.Key.Break,
  Menu            =SDL.Key.Menu,
  Power           =SDL.Key.Power,
  Euro            =SDL.Key.Euro,
  Undo            =SDL.Key.Undo,
  
  NumKeys
}
#endregion

[Flags]
public enum KeyMod : uint
{ None=0, LShift=SDL.KeyMod.LShift, RShift=SDL.KeyMod.RShift, LCtrl=SDL.KeyMod.LCtrl, RCtrl=SDL.KeyMod.RCtrl,
  LAlt=SDL.KeyMod.LAlt, RAlt=SDL.KeyMod.RAlt, LMeta=SDL.KeyMod.LMeta, RMeta=SDL.KeyMod.RMeta,
  Shift=LShift|RShift, Ctrl=LCtrl|RCtrl, Alt=LAlt|RAlt, Meta=LMeta|RMeta,
  NumLock=SDL.KeyMod.NumLock, CapsLock=SDL.KeyMod.CapsLock, Mode=SDL.KeyMod.Mode,

  StatusMask=NumLock|CapsLock|Mode, KeyMask=Alt|Shift|Ctrl|Meta
}

public struct KeyCombo
{ public KeyCombo(KeyboardEvent e) { KeyMods=e.KeyMods; Key=e.Key; Char=e.Char; }
  public KeyCombo(Key key) { KeyMods=KeyMod.None; Key=key; Char=(char)0; }
  public KeyCombo(KeyMod keyMods, Key key) { KeyMods=keyMods; Key=key; Char=(char)0; }
  public KeyCombo(KeyMod keyMods, char character) { KeyMods=keyMods; Key=Key.None; Char=char.ToUpper(character); }
  public KeyCombo(KeyMod keyMods, Key key, char character) { KeyMods=keyMods; Key=key; Char=char.ToUpper(character); }

  public bool Valid { get { return Char!=0 || Key!=Key.None; } }

  public bool Matches(KeyboardEvent e) { return Matches(e.KeyMods, e.Key, e.Char); }
  public bool Matches(KeyMod keyMods, Key key) { return Matches(keyMods, key, (char)0); }
  public bool Matches(KeyMod keyMods, char character) { return Matches(keyMods, Key.None, character); }
  public bool Matches(KeyMod keyMods, Key key, char character)
  { if(Char!=0)
    { character = (keyMods&KeyMod.Ctrl)!=0 && character<32 ? (char)(character+64) : char.ToUpper(character);
      if(character!=Char) return false;
    }
    else if(key!=Key) return false;
    for(int i=0; i<masks.Length; i++)
    { KeyMod mask = KeyMods&masks[i];
      if(mask!=0 && (keyMods&mask)==0) return false;
    }
    return true;
  }

  public override string ToString()
  { if(Char==0 && Key==Key.None) return string.Empty;
    string ret=string.Empty;
    if((KeyMods&KeyMod.Shift) != 0)
    { if((KeyMods&KeyMod.LShift)==0) ret += 'R';
      else if((KeyMods&KeyMod.RShift)==0) ret += 'L';
      ret += "Shift-";
    }
    if((KeyMods&KeyMod.Ctrl) != 0)
    { if((KeyMods&KeyMod.LCtrl)==0) ret += 'R';
      else if((KeyMods&KeyMod.RCtrl)==0) ret += 'L';
      ret += "Ctrl-";
    }
    if((KeyMods&KeyMod.Alt) != 0)
    { if((KeyMods&KeyMod.LAlt)==0) ret += 'R';
      else if((KeyMods&KeyMod.RAlt)==0) ret += 'L';
      ret += "Alt-";
    }
    if((KeyMods&KeyMod.Meta) != 0)
    { if((KeyMods&KeyMod.LMeta)==0) ret += 'R';
      else if((KeyMods&KeyMod.RMeta)==0) ret += 'L';
      ret += "Meta-";
    }
    return ret += Char==0 ? Key.ToString() : Char.ToString();
  }

  public KeyMod KeyMods;
  public Key    Key;
  public char   Char;
  
  static KeyMod[] masks = new KeyMod[] { KeyMod.Shift, KeyMod.Ctrl, KeyMod.Alt, KeyMod.Meta };
}

public delegate void KeyEventHandler(KeyboardEvent evt);
public delegate void MouseMoveHandler(MouseMoveEvent evt);
public delegate void MouseClickHandler(MouseClickEvent evt);
public delegate void JoyMoveHandler(Joystick js, JoyMoveEvent evt);
public delegate void JoyBallHandler(Joystick js, JoyBallEvent evt);
public delegate void JoyHatHandler(Joystick js, JoyHatEvent evt);
public delegate void JoyButtonHandler(Joystick js, JoyButtonEvent evt);
#endregion

#region Keyboard
public sealed class Keyboard
{ private Keyboard() { }
  
  public static KeyEventHandler KeyEvent;

  public static KeyMod KeyMods { get { return mods&KeyMod.KeyMask; } }
  public static KeyMod StatusMods { get { return mods&KeyMod.StatusMask; } }
  public static KeyMod Mods { get { return mods; } }

  public static bool IsModKey(Key key)
  { return key==Key.LShift || key==Key.LCtrl || key==Key.LAlt   ||
           key==Key.RShift || key==Key.RCtrl || key==Key.RAlt   ||
           key==Key.LMeta  || key==Key.RMeta || key==Key.LSuper || key==Key.RSuper;
  }
  public static bool HasAnyMod  (KeyMod mod) { return (mods&mod)!=KeyMod.None; }
  public static bool HasAllMods (KeyMod mod) { return (mods&mod)==mod; }
  public static bool HasAllKeys (KeyMod mod) { return (KeyMods&mod)==mod; }
  public static bool HasOnlyKeys(KeyMod mod)
  { KeyMod mods = KeyMods;
    return (mods&mod)!=KeyMod.None && (mods&~mod)==KeyMod.None;
  }

  public static string KeyName(Key key) { return Enum.GetName(typeof(Key), key); }

  public static bool Pressed(Key key)          { return state[(int)key]; }
  public static bool PressedRel(Key key)       { bool ret=Pressed(key); if(ret) Release(key); return ret; }
  public static void Press(Key key)            { state[(int)key]=true; }
  public static void Release(Key key)          { state[(int)key]=false; }
  public static void Press(Key key, bool down) { state[(int)key]=down; }

  public static void EnableKeyRepeat() { EnableKeyRepeat(500, 30); }
  public static void EnableKeyRepeat(int delayMs, int intervalMs)
  { if(SDL.EnableKeyRepeat(delayMs, intervalMs)!=0) SDL.RaiseError();
  }
  public static void DisableKeyRepeat() { EnableKeyRepeat(0, 0); }
  
  internal static void Initialize()
  { mods = (KeyMod)SDL.GetModState();
    unsafe
    { int   i=0, num;
      byte *keys = SDL.GetKeyState(&num);
      if(num>(int)Key.NumKeys) num = (int)Key.NumKeys;
      for(; i<num; i++) state[i]=(keys[i]!=0);
      for(; i<(int)Key.NumKeys; i++) state[i]=false;
    }
  }

  internal static void OnKeyEvent(KeyboardEvent e)
  { mods = mods & ~KeyMod.StatusMask | e.StatusMods;
    if(e.Down) // SDL's mod handling is crap
    { switch(e.Key)
      { case Key.LShift: mods |= KeyMod.LShift; break;
        case Key.RShift: mods |= KeyMod.RShift; break;
        case Key.LCtrl:  mods |= KeyMod.LCtrl;  break;
        case Key.RCtrl:  mods |= KeyMod.RCtrl;  break;
        case Key.LAlt:   mods |= KeyMod.LAlt;   break;
        case Key.RAlt:   mods |= KeyMod.RAlt;   break;
        case Key.LMeta:  mods |= KeyMod.LMeta;  break;
        case Key.RMeta:  mods |= KeyMod.RMeta;  break;
      }
      state[(int)e.Key] = true;
    }
    else
    { switch(e.Key)
      { case Key.LShift: mods &= ~KeyMod.LShift; break;
        case Key.RShift: mods &= ~KeyMod.RShift; break;
        case Key.LCtrl:  mods &= ~KeyMod.LCtrl;  break;
        case Key.RCtrl:  mods &= ~KeyMod.RCtrl;  break;
        case Key.LAlt:   mods &= ~KeyMod.LAlt;   break;
        case Key.RAlt:   mods &= ~KeyMod.RAlt;   break;
        case Key.LMeta:  mods &= ~KeyMod.LMeta;  break;
        case Key.RMeta:  mods &= ~KeyMod.RMeta;  break;
      }
      state[(int)e.Key] = false;
    }
    if(KeyEvent!=null) KeyEvent(e);
  }

  static bool[] state = new bool[(int)Key.NumKeys];
  static KeyMod mods;
}
#endregion

#region Mouse
public sealed class Mouse
{ private Mouse() { }
  
  public static event MouseMoveHandler  MouseMove;
  public static event MouseClickHandler MouseClick;

  public static System.Drawing.Point Point
  { get { return new System.Drawing.Point(x, y); }
    set { x=value.X; y=value.Y; }
  }
  public static int X { get { return x; } set { x=value; } }
  public static int Y { get { return y; } set { y=value; } }
  public static int Z { get { return z; } set { z=value; } }
  
  public static bool SystemCursorVisible
  { get { return cursorVisible; }
    set { SDL.ShowCursor(value?1:0); cursorVisible=(SDL.ShowCursor(-1)!=0); }
  }

  public static byte Buttons { get { return buttons; } set { buttons=value; } }
  public static bool OnlyPressed(MouseButton button) { return buttons==(1<<(byte)button); }
  public static bool Pressed(MouseButton button) { return (buttons&(1<<(byte)button))!=0; }
  public static void SetPressed(MouseButton button, bool down)
  { if(down) buttons |= (byte)(1<<(byte)button);
    else buttons &= (byte)~(1<<(byte)button);
  }

  internal static void Initialize()
  { unsafe
    { int x, y;
      Mouse.buttons = SDL.GetMouseState(&x, &y);
      Mouse.x=x; Mouse.y=y;
      SDL.ShowCursor(1);
    }
  }

  internal static void OnMouseMove(MouseMoveEvent e)
  { x=e.X; y=e.Y; buttons=e.Buttons;
    if(MouseMove!=null) MouseMove(e);
  }
  internal static void OnMouseClick(MouseClickEvent e)
  { if(e.Down)
    { buttons |= (byte)(1<<(byte)e.Button);
      if(e.Button==MouseButton.WheelUp) z--;
      else if(e.Button==MouseButton.WheelDown) z++;
    }
    else buttons &= (byte)~(1<<(byte)e.Button);
    if(MouseClick!=null) MouseClick(e);
  }

  static int  x, y, z;
  static byte buttons;
  static bool cursorVisible;
}
#endregion

#region Joystick
public sealed class Joystick : IDisposable
{ public sealed class Ball
  { internal Ball() { }
    public int Xrel, Yrel;
  }

  public event JoyMoveHandler   JoyMove;
  public event JoyBallHandler   JoyBall;
  public event JoyHatHandler    JoyHat;
  public event JoyButtonHandler JoyButton;

  internal Joystick(int number)
  { joystick = SDL.JoystickOpen(number);
    unsafe { if(joystick.ToPointer()==null) SDL.RaiseError(); }

    balls   = new Ball[SDL.JoystickNumBalls(joystick)];
    hats    = new HatPosition[SDL.JoystickNumHats(joystick)];
    buttons = new bool[SDL.JoystickNumButtons(joystick)];
    axes    = new int[SDL.JoystickNumAxes(joystick)];
    name    = SDL.JoystickName(number);
    for(int i=0; i<balls.Length; i++) balls[i] = new Ball();

    this.number=number;
  }
  ~Joystick() { Dispose(true); } // TODO: find out if finalizer is called if an exception is thrown from a constructor
  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  public Ball[] Balls       { get { return balls; } }
  public HatPosition[] Hats { get { return hats; } }
  public bool[] Buttons     { get { return buttons; } }
  public int[] Axes         { get { return axes; } }
  public string Name        { get { return name; } }
  public int Number         { get { return number; } }

  internal void OnJoyMove(JoyMoveEvent e)
  { axes[e.Axis] = e.Position;
    if(JoyMove!=null) JoyMove(this, e);
  }

  internal void OnJoyBall(JoyBallEvent e)
  { balls[e.Ball].Xrel=e.Xrel; balls[e.Ball].Yrel=e.Yrel;
    if(JoyBall!=null) JoyBall(this, e);
  }
  
  internal void OnJoyHat(JoyHatEvent e)
  { hats[e.Hat]=e.Position;
    if(JoyHat!=null) JoyHat(this, e);
  }
  
  internal void OnJoyButton(JoyButtonEvent e)
  { buttons[e.Button]=e.Down;
    if(JoyButton!=null) JoyButton(this, e);
  }

  void Dispose(bool deconstructor)
  { unsafe
    { if(joystick.ToPointer()!=null)
      { SDL.JoystickClose(joystick);
        joystick = new IntPtr(null);
      }
    }
  }

  IntPtr        joystick;
  Ball[]        balls;
  HatPosition[] hats;
  bool[]        buttons;
  int[]         axes;
  string        name;
  int           number;
}
#endregion

#region Input
public sealed class Input
{ private Input() { }

  public static bool Initialized { get { return initCount>0; } }

  public static Joystick[] Joysticks { get { return joysticks; } }
  
  public static bool UseJoysticks
  { get { return joysticks!=null; }
    set
    { if(value!=UseJoysticks)
      { if(initCount==0) throw new InvalidOperationException("The input system has not been initialized!");
        if(value)
        { SDL.Initialize(SDL.InitFlag.Joystick);
          SDL.JoystickEventState(SDL.JoystickMode.Events);
          joysticks = new Joystick[SDL.NumJoysticks()];
          for(int i=0; i<joysticks.Length; i++) joysticks[i] = new Joystick(i);
        }
        else
        { for(int i=0; i<joysticks.Length; i++) joysticks[i].Dispose();
          SDL.JoystickEventState(SDL.JoystickMode.Poll);
          SDL.Deinitialize(SDL.InitFlag.Joystick);
          joysticks = null;
        }
      }
    }
  }

  public static void Initialize() { Initialize(false); }
  public static void Initialize(bool useJoysticks)
  { if(initCount++==0)
    { Events.Events.Initialize();
      Keyboard.Initialize();
      Mouse.Initialize();
      UseJoysticks = useJoysticks;
    }
  }

  public static void Deinitialize()
  { if(initCount==0) throw new InvalidOperationException("Deinitialize called too many times!");
    if(--initCount==0)
    { UseJoysticks = false;
      Events.Events.Deinitialize();
    }
  }
  
  public static bool ProcessEvent(Event e)
  { switch(e.Type)
    { case EventType.MouseMove:  Mouse.OnMouseMove((MouseMoveEvent)e); return true;
      case EventType.JoyMove:
      { JoyMoveEvent je = (JoyMoveEvent)e;
        joysticks[je.Device].OnJoyMove(je);
        return true;
      }
      case EventType.Keyboard:   Keyboard.OnKeyEvent((KeyboardEvent)e); return true;
      case EventType.MouseClick: Mouse.OnMouseClick((MouseClickEvent)e); return true;
      case EventType.JoyBall:
      { JoyBallEvent je = (JoyBallEvent)e;
        joysticks[je.Device].OnJoyBall(je);
        return true;
      }
      case EventType.JoyButton:
      { JoyButtonEvent je = (JoyButtonEvent)e;
        joysticks[je.Device].OnJoyButton(je);
        return true;
      }
      case EventType.JoyHat:
      { JoyHatEvent je = (JoyHatEvent)e;
        joysticks[je.Device].OnJoyHat(je);
        return true;
      }
    }
    return false;
  }

  static Joystick[] joysticks;
  static uint initCount;
}
#endregion

}