using System;
using GameLib.Interop.SDL;
using GameLib.Events;

namespace GameLib.Input
{

#region Types
public class MouseButton
{ private MouseButton() { }
  public const byte Left      =0;
  public const byte Middle    =1;
  public const byte Right     =2;
  public const byte WheelUp   =3;
  public const byte WheelDown =4;
}

#region Key enum
public enum Key : int
{ Unknown         =SDL.Key.Unknown,
  
  /* These are mapped to ascii -- BEGIN ASCII MAPPED SECTION */
  Backspace       =SDL.Key.Backspace,
  Tab             =SDL.Key.Tab,
  Clear           =SDL.Key.Clear,
  Return          =SDL.Key.Return,
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
  NumLock=SDL.KeyMod.NumLock, CapsLock=SDL.KeyMod.CapsLock, Mode=SDL.KeyMod.Mode,
  Shift=LShift|RShift, Ctrl=LCtrl|RCtrl, Alt=LAlt|RAlt, Meta=LMeta|RMeta
}

public delegate void KeyPressHandler(KeyboardEvent evt);
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
  
  public static event KeyPressHandler KeyPress;

  public static KeyMod Modifiers { get { return mods; } }

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

  internal static void OnKeyPress(KeyboardEvent e)
  { mods = (KeyMod)e.Mods;
    state[(int)e.Key] = e.Down;
    if(KeyPress!=null) KeyPress(e);
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

  public static int X { get { return x; } }
  public static int Y { get { return y; } }
  public static System.Drawing.Point Point { get { return new System.Drawing.Point(x, y); } }
  
  public static bool CursorVisible
  { get { return cursorVisible; }
    set { SDL.ShowCursor(value?1:0); cursorVisible=(SDL.ShowCursor(-1)!=0); }
  }

  public static byte Buttons { get { return buttons; } }
  public static bool Pressed(int button) { return (buttons&(1<<button))!=0; }

  internal static void Initialize()
  { unsafe
    { int x, y;
      Mouse.buttons = SDL.GetMouseState(&x, &y);
      Mouse.x=x; Mouse.y=y;
      SDL.ShowCursor(1);
    }
  }

  internal static void OnMouseMove(MouseMoveEvent e)
  { x=e.X; y=e.Y; buttons=e.Buttons; // TODO: mouse moves 100x faster than normal in fullscreen mode. why?
    if(MouseMove!=null) MouseMove(e);
  }
  internal static void OnMouseClick(MouseClickEvent e)
  { if(e.Down) buttons |= (byte)(1<<e.Button);
    else buttons &= (byte)~(1<<e.Button);
    if(MouseClick!=null) MouseClick(e);
  }

  static int  x, y;
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

  public static Joystick[] Joysticks { get { return joysticks; } }

  public static void Initialize(bool useJoysticks)
  { if(initCount++==0)
    { Events.Events.Initialize();
      Events.Events.EventFilter += new EventFilter(OnEvent);
      Keyboard.Initialize();
      Mouse.Initialize();
      SDL.ShowCursor(0);
      
      if(useJoysticks)
      { SDL.Initialize(SDL.InitFlag.Joystick);
        joysticks = new Joystick[SDL.NumJoysticks()];
        for(int i=0; i<joysticks.Length; i++) joysticks[i] = new Joystick(i);
      }
    }
  }

  public static void Deinitialize()
  { if(initCount==0) throw new InvalidOperationException("Deinitialize called too many times!");
    if(--initCount==0)
    { if(joysticks!=null)
      { for(int i=0; i<joysticks.Length; i++) joysticks[i].Dispose();
        SDL.Deinitialize(SDL.InitFlag.Joystick);
        joysticks = null;
      }
      Events.Events.EventFilter -= new EventFilter(OnEvent);
      Events.Events.Deinitialize();
    }
  }

  static FilterAction OnEvent(Event e)
  { switch(e.Type)
    { case EventType.MouseMove:  Mouse.OnMouseMove((MouseMoveEvent)e); break;
      case EventType.JoyMove:
      { JoyMoveEvent je = (JoyMoveEvent)e;
        joysticks[je.Device].OnJoyMove(je);
        break;
      }
      case EventType.Keyboard:   Keyboard.OnKeyPress((KeyboardEvent)e); break;
      case EventType.MouseClick: Mouse.OnMouseClick((MouseClickEvent)e); break;
      case EventType.JoyBall:
      { JoyBallEvent je = (JoyBallEvent)e;
        joysticks[je.Device].OnJoyBall(je);
        break;
      }
      case EventType.JoyButton:
      { JoyButtonEvent je = (JoyButtonEvent)e;
        joysticks[je.Device].OnJoyButton(je);
        break;
      }
      case EventType.JoyHat:
      { JoyHatEvent je = (JoyHatEvent)e;
        joysticks[je.Device].OnJoyHat(je);
        break;
      }
    }
    return FilterAction.Continue;
  }

  static Joystick[] joysticks;
  static uint initCount;
}
#endregion

}