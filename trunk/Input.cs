using System;
using GameLib.Interop.SDL;
using GameLib.Events;

namespace GameLib.Input
{

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

public sealed class Keyboard
{ private Keyboard() { }
  
  public static event KeyPressHandler KeyPress;

  public static KeyMod Modifiers { get { return mods; } }

  public static void Initialize()
  { if(initCount++==0)
    { Events.Events.Initialize();
      Events.Events.EventFilter += new EventFilter(OnEvent);
      mods = (KeyMod)SDL.GetModState();
      unsafe
      { int   i=0, num;
        byte *keys = SDL.GetKeyState(&num);
        if(num>(int)Key.NumKeys) num = (int)Key.NumKeys;
        for(; i<num; i++) state[i]=(keys[i]!=0);
        for(; i<(int)Key.NumKeys; i++) state[i]=false;
      }
    }
  }

  public static void Deinitialize()
  { if(initCount==0) throw new InvalidOperationException("Deinitialize called too many times!");
    if(--initCount==0)
    { Events.Events.EventFilter -= new EventFilter(OnEvent);
      Events.Events.Deinitialize();
    }
  }

  public static string KeyName(Key key) { return Enum.GetName(typeof(Key), key); }

  public static bool Pressed(Key key) { return state[(int)key]; }

  public void EnableKeyRepeat() { EnableKeyRepeat(500, 30); }
  public void EnableKeyRepeat(int delayMs, int intervalMs)
  { if(SDL.EnableKeyRepeat(delayMs, intervalMs)!=0) SDL.RaiseError();
  }
  public void DisableKeyRepeat() { EnableKeyRepeat(0, 0); }
  
  static FilterAction OnEvent(Event evt)
  { if(evt.Type==EventType.Keyboard)
    { KeyboardEvent ke = (KeyboardEvent)evt;
      mods = (KeyMod)ke.Mods;
      state[(int)ke.Key] = ke.Down;
      if(KeyPress!=null) KeyPress(ke);
    }
    return FilterAction.Continue;
  }

  static bool[] state = new bool[(int)Key.NumKeys];
  static uint   initCount;
  static KeyMod mods;
}

}