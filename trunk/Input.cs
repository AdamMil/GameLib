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
using GameLib.Interop.SDL;
using GameLib.Events;

namespace GameLib.Input
{

#region Types
/// <summary>An enum representing mouse buttons in the system.</summary>
public enum MouseButton : byte
{ 
  /// <summary>The left mouse button.</summary>
  Left=0,
  /// <summary>The middle mouse button.</summary>
  Middle=1,
  /// <summary>The right mouse button.</summary>
  Right=2,
  /// <summary>Used when the mousewheel is rolled upwards/forwards (away from the user).</summary>
  WheelUp=3,
  /// <summary>Used when the mousewheel is rolled downwards/backwards (towards the user).</summary>
  WheelDown=4
}

#region Key enum
/// <summary>This enum contains virtual key values for the operating system's virtual keyboard.</summary>
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
  /// <summary>Left Windows key.</summary>
  LSuper          =SDL.Key.LSuper, // left Windows key
  /// <summary>Right Windows key.</summary>
  RSuper          =SDL.Key.RSuper, // right Windows key
  /// <summary>"Alt Gr" key.</summary>
  Mode            =SDL.Key.Mode,
  /// <summary>Multi-key compose key.</summary>
  Compose         =SDL.Key.Compose,

  /* Additional function keys */
  Help            =SDL.Key.Help,
  Print           =SDL.Key.Print,
  Sysreq          =SDL.Key.Sysreq,
  Break           =SDL.Key.Break,
  /// <summary>Key to open menus.</summary>
  Menu            =SDL.Key.Menu,
  /// <summary>Macintosh (etc) power key.</summary>
  Power           =SDL.Key.Power,
  /// <summary>Some european keyboards have a euro key.</summary>
  Euro            =SDL.Key.Euro,
  /// <summary>Some keyboards have an undo key.</summary>
  Undo            =SDL.Key.Undo,

  NumKeys
}
#endregion

/// <summary>This enum contains keyboard modifier bitmask values.</summary>
[Flags]
public enum KeyMod : uint
{ 
  /// <summary>No keyboard modifiers.</summary>
  None=0,
  /// <summary>Left shift key.</summary>
  LShift=SDL.KeyMod.LShift,
  /// <summary>Right shift key.</summary>
  RShift=SDL.KeyMod.RShift,
  /// <summary>Left ctrl key.</summary>
  LCtrl=SDL.KeyMod.LCtrl,
  /// <summary>Right ctrl key.</summary>
  RCtrl=SDL.KeyMod.RCtrl,
  /// <summary>Left alt key.</summary>
  LAlt=SDL.KeyMod.LAlt,
  /// <summary>Right alt key.</summary>
  RAlt=SDL.KeyMod.RAlt,
  /// <summary>Left meta key.</summary>
  LMeta=SDL.KeyMod.LMeta,
  /// <summary>Right meta key.</summary>
  RMeta=SDL.KeyMod.RMeta,
  /// <summary>A mask containing the left and right shift keys.</summary>
  Shift=LShift|RShift,
  /// <summary>A mask containing the left and right ctrl keys.</summary>
  Ctrl=LCtrl|RCtrl,
  /// <summary>A mask containing the left and right alt keys.</summary>
  Alt=LAlt|RAlt,
  /// <summary>A mask containing the left and right meta keys.</summary>
  Meta=LMeta|RMeta,
  /// <summary>Num lock status.</summary>
  NumLock=SDL.KeyMod.NumLock,
  /// <summary>Caps lock status.</summary>
  CapsLock=SDL.KeyMod.CapsLock,
  /// <summary>Keyboard mode.</summary>
  Mode=SDL.KeyMod.Mode,

  /// <summary>A mask containing the num lock, caps lock, and keyboard mode status modifiers ("status modifiers").</summary>
  StatusMask=NumLock|CapsLock|Mode,
  /// <summary>A mask containing the alt, shift, ctrl, and meta key modifiers ("key modifiers").</summary>
  KeyMask=Alt|Shift|Ctrl|Meta
}

/// <summary>This class represents a key combination and provides methods for matching it against keyboard events.</summary>
public struct KeyCombo
{ 
  /// <summary>Initializes this key combination from a keyboard event.</summary>
  /// <param name="e">The <see cref="KeyboardEvent"/> from which this key combination will be initialized.</param>
  /// <remarks>This constructor sets the <see cref="Key"/>, <see cref="Char"/>, and <see cref="KeyMod"/> fields from
  /// the corresponding fields in the event.
  /// </remarks>
  public KeyCombo(KeyboardEvent e) { KeyMods=e.KeyMods; Key=e.Key; Char=e.Char; }
  /// <summary>Initializes this key combination from a virtual key code.</summary>
  /// <param name="key">The virtual key code to use.</param>
  /// <remarks>This constructor sets the <see cref="Key"/> field.</remarks>
  public KeyCombo(Key key) { KeyMods=KeyMod.None; Key=key; Char=(char)0; }
  /// <summary>Initializes this key combination from a virtual key and a set of key modifiers.</summary>
  /// <param name="keyMods">The key modifiers to use.</param>
  /// <param name="key">The virtual key code to use.</param>
  /// <remarks>This constructor sets the <see cref="KeyMods"/> and <see cref="Key"/> fields.</remarks>
  public KeyCombo(KeyMod keyMods, Key key) { KeyMods=keyMods; Key=key; Char=(char)0; }
  /// <summary>Initailizes this key combination from a character and a set of key modifiers.</summary>
  /// <param name="keyMods">The key modifiers to use.</param>
  /// <param name="character">The character to use.</param>
  /// <remarks>This constructor sets the <see cref="KeyMods"/> and <see cref="Char"/> fields.</remarks>
  public KeyCombo(KeyMod keyMods, char character) { KeyMods=keyMods; Key=Key.None; Char=char.ToUpper(character); }
  /// <summary>Initializes this key combination from a character, a virtual key code, and a set of key modifiers.</summary>
  /// <param name="keyMods">The key modifiers to use.</param>
  /// <param name="key">The virtual key code to use.</param>
  /// <param name="character">The character to use.</param>
  /// <remarks>This constructor sets the <see cref="KeyMods"/>, <see cref="Key"/>, and <see cref="Char"/> fields.</remarks>
  public KeyCombo(KeyMod keyMods, Key key, char character) { KeyMods=keyMods; Key=key; Char=char.ToUpper(character); }

  /// <summary>Returns true if this key combination is valid.</summary>
  /// <remarks>A key combination is considered valid if either the <see cref="Char"/> field is nonzero or the
  /// <see cref="Key"/> doesn't equal <see cref="GameLib.Input.Key.None"/>. An invalid key combination can't match
  /// anything.
  /// </remarks>
  public bool Valid { get { return Char!=0 || Key!=Key.None; } }

  /// <summary>Checks whether this key combination matches the given keyboard event.</summary>
  /// <param name="e">The keyboard event to check against.</param>
  /// <returns>True if the event matches this key combination and false otherwise.</returns>
  /// <remarks>First, the if the <see cref="Char"/> field is non-zero, it is checked to see if it matches
  /// the character passed (case-insensitive, and the Ctrl-A character [ASCII 1] is considered to match 'A' if the
  /// Ctrl key is depressed according to the event). Otherwise, if the <see cref="Key"/> field is not
  /// <see cref="GameLib.Input.Key.None"/>, it is checked against the virtual key in the event. If neither of those
  /// tests succeed,
  /// false is returned. Otherwise, the flags are checked. Each set of modifier keys (Ctrl, Shift, Alt, Meta) must
  /// match <see cref="KeyMod"/>, but if <see cref="KeyMod"/> specifies both keys in a set (left and right), only
  /// one must be present for a match to be successful.
  /// </remarks>
  public bool Matches(KeyboardEvent e) { return Matches(e.KeyMods, e.Key, e.Char); }
  /// <summary>Checks whether this key combination matches the given virtual key and modifiers.</summary>
  /// <param name="keyMods">The key modifiers to check against.</param>
  /// <param name="key">The virtual key to check against.</param>
  /// <returns>True if the given key and modifiers match this key combination and false otherwise.</returns>
  /// <remarks>If the <see cref="Key"/> field is not <see cref="GameLib.Input.Key.None"/>, it is checked against the
  /// virtual key
  /// given. If it doesn't match, false is returned. Otherwise, the flags are checked. Each set of modifier keys
  /// (Ctrl, Shift, Alt, Meta) must match <see cref="KeyMod"/>, but if <see cref="KeyMod"/> specifies both keys in
  /// a set (left and right), only one must be present for a match to be successful.
  /// </remarks>
  public bool Matches(KeyMod keyMods, Key key) { return Matches(keyMods, key, (char)0); }
  /// <summary>Checks whether this key combination matches the given character and modifiers.</summary>
  /// <param name="keyMods">The key modifiers to check against.</param>
  /// <param name="character">The character to check against.</param>
  /// <returns>True if the given character and modifiers match this key combination and false otherwise.</returns>
  /// <remarks>First, the <see cref="Char"/> field is checked to see if it matches the character passed
  /// (case-insensitive, and the Ctrl-A character [ASCII 1] is considered to match 'A' if the Ctrl key is depressed
  /// according to <paramref name="keyMods"/>). If it doesn't match, false is returned. Otherwise, the flags are
  /// checked. Each set of modifier keys (Ctrl, Shift, Alt, Meta) must match <see cref="KeyMod"/>, but if
  /// <see cref="KeyMod"/> specifies both keys in a set (left and right), only one must be present for a match to
  /// be successful.
  /// </remarks>
  public bool Matches(KeyMod keyMods, char character) { return Matches(keyMods, Key.None, character); }
  /// <summary>Checks whether this key combination matches the given character or virtual key, and modifiers.</summary>
  /// <param name="keyMods">The key modifiers to check against.</param>
  /// <param name="key">The virtual key to check against.</param>
  /// <param name="character">The character to check against.</param>
  /// <returns>True if the event matches this key combination and false otherwise.</returns>
  /// <remarks>First, the if the <see cref="Char"/> field is non-zero, it is checked to see if it matches
  /// the character passed (case-insensitive, and the Ctrl-A character [ASCII 1] is considered to match 'A' if the
  /// Ctrl key is depressed according to <paramref name="keyMods"/>). Otherwise, if <see cref="Char"/> is zero and
  /// the <see cref="Key"/> field is not <see cref="GameLib.Input.Key.None"/>, it is checked against the virtual key
  /// passed. If neither of those tests succeed,
  /// false is returned. Otherwise, the flags are checked. Each set of modifier keys (Ctrl, Shift, Alt, Meta) must
  /// match <see cref="KeyMod"/>, but if <see cref="KeyMod"/> specifies both keys in a set (left and right), only
  /// one must be present for a match to be successful.
  /// </remarks>
  public bool Matches(KeyMod keyMods, Key key, char character)
  { if(Char!=0 && character!=0)
    { character = (keyMods&KeyMod.Ctrl)!=0 && character<32 ? (char)(character+64) : char.ToUpper(character);
      if(character!=Char) return false;
    }
    else if(Key==Key.None || key!=Key) return false;
    for(int i=0; i<masks.Length; i++)
    { KeyMod mask = KeyMods&masks[i];
      if(mask!=0 && (keyMods&mask)==0 || mask==0 && (keyMods&mask)!=0) return false;
    }
    return true;
  }

  /// <summary>Returns a human-readable string representing this key combination.</summary>
  /// <returns>A human-readable string representing this key combination.</returns>
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

  /// <summary>This field holds the set of key modifiers to match against.</summary>
  /// <remarks>This field holds the set of key modifiers to match against, except that if it contains both key
  /// modifiers in a set (eg, both left and right shift keys), only one of them will need to match.
  /// </remarks>
  public KeyMod KeyMods;
  /// <summary>This field holds the virtual key code to check against.</summary>
  /// <remarks>To disable the check, set this field to <see cref="GameLib.Input.Key.None"/>. Note that if this field
  /// and <see cref="Char"/> are both set, <see cref="Char"/> will take precedence and this field will be ignored.
  /// </remarks>
  public Key Key;
  /// <summary>This field holds the character to check against.</summary>
  /// <remarks>To disable the check, set this field to 0. Note that if this field and <see cref="Key"/> are both set,
  /// this field will take precedence and <see cref="Key"/> will be ignored.
  /// </remarks>
  public char Char;

  static KeyMod[] masks = new KeyMod[] { KeyMod.Shift, KeyMod.Ctrl, KeyMod.Alt, KeyMod.Meta };
}

/// <summary>This delegate is used in conjunction with <see cref="Keyboard.KeyEvent"/> to notify the application of
/// keyboard events.
/// </summary>
public delegate void KeyEventHandler(KeyboardEvent e);
/// <summary>This delegate is used in conjunction with <see cref="Mouse.MouseMove"/> to notify the application of
/// mouse movement.
/// </summary>
public delegate void MouseMoveHandler(MouseMoveEvent e);
/// <summary>This delegate is used in conjunction with <see cref="Mouse.MouseClick"/> to notify the application of
/// mouse clicks and mouse wheel motion.
/// </summary>
public delegate void MouseClickHandler(MouseClickEvent e);
/// <summary>This delegate is used in conjunction with <see cref="Joystick.JoyMove"/> to notify the application of
/// joystick axis motion.
/// </summary>
public delegate void JoyMoveHandler(Joystick js, JoyMoveEvent e);
/// <summary>This delegate is used in conjunction with <see cref="Joystick.JoyBall"/> to notify the application of
/// joystick ball motion.
/// </summary>
public delegate void JoyBallHandler(Joystick js, JoyBallEvent e);
/// <summary>This delegate is used in conjunction with <see cref="Joystick.JoyHat"/> to notify the application of
/// joystick POV hat motion.
/// </summary>
public delegate void JoyHatHandler(Joystick js, JoyHatEvent e);
/// <summary>This delegate is used in conjunction with <see cref="Joystick.JoyButton"/> to notify the application of
/// joystick button presses and releases.
/// </summary>
public delegate void JoyButtonHandler(Joystick js, JoyButtonEvent e);
#endregion

#region Keyboard
/// <summary>This class represents the keyboard.</summary>
/// <remarks>This class is updated by the <see cref="Input.ProcessEvent"/> method.</remarks>
public sealed class Keyboard
{ private Keyboard() { }

  /// <summary>Occurs when a keyboard key is pressed or released.</summary>
  /// <remarks>This event is raised by the <see cref="Input.ProcessEvent"/> method.</remarks>
  public static KeyEventHandler KeyEvent;

  /// <summary>Gets the current set of key modifiers that are depressed.</summary>
  /// <remarks>This property is updated by the <see cref="Input.ProcessEvent"/> method.</remarks>
  public static KeyMod KeyMods { get { return mods&KeyMod.KeyMask; } }
  /// <summary>Gets the current set of status modifiers that are enabled.</summary>
  /// <remarks>This property is updated by the <see cref="Input.ProcessEvent"/> method.</remarks>
  public static KeyMod StatusMods { get { return mods&KeyMod.StatusMask; } }
  /// <summary>Gets the current set of modifiers.</summary>
  /// <remarks>This property is updated by the <see cref="Input.ProcessEvent"/> method.</remarks>
  public static KeyMod Mods { get { return mods; } }

  /// <summary>Determines whether a virtual key code represents a modifier key.</summary>
  /// <param name="key">The virtual key to check.</param>
  /// <returns>True if the virtual key is a modifier key (left or right shift/ctrl/alt/meta/super) and false
  /// otherwise.
  /// </returns>
  public static bool IsModKey(Key key)
  { return key==Key.LShift || key==Key.LCtrl || key==Key.LAlt   ||
           key==Key.RShift || key==Key.RCtrl || key==Key.RAlt   ||
           key==Key.LMeta  || key==Key.RMeta || key==Key.LSuper || key==Key.RSuper;
  }
  /// <summary>Returns true if any of the given modifiers are in effect.</summary>
  /// <param name="mod">A set of <see cref="KeyMod"/> modifiers, ORed together.</param>
  /// <returns>True if any of the given modifiers are in effect, and false otherwise.</returns>
  /// <remarks>This methods requires <see cref="Input.ProcessEvent"/> to be called in order to work properly.</remarks>
  public static bool HasAnyMod(KeyMod mod) { return (mods&mod)!=KeyMod.None; }
  /// <summary>Returns true if all of the given modifiers are in effect.</summary>
  /// <param name="mod">A set of <see cref="KeyMod"/> modifiers, ORed together.</param>
  /// <returns>True if all of the given modifiers are in effect, and false otherwise.</returns>
  /// <remarks>This methods requires <see cref="Input.ProcessEvent"/> to be called in order to work properly.</remarks>
  public static bool HasAllMods(KeyMod mod) { return (mods&mod)==mod; }
  /// <summary>Returns true if any of the given key modifiers are in effect, and no others.</summary>
  /// <param name="mod">A set of <see cref="KeyMod"/> modifiers, ORed together.</param>
  /// <returns>True if any of the given key modifiers are in effect, and no others.</returns>
  /// <remarks>Key modifiers only include modifier keys such as Ctrl, Alt, etc. To check whether all (instead of any)
  /// of the given key modifiers are in effect, and no others, simply compare the desired set of flags to see whether
  /// it equals <see cref="KeyMods"/>.
  /// </remarks>
  public static bool HasOnlyKeys(KeyMod mod)
  { KeyMod mods = KeyMods;
    return (mods&mod)!=KeyMod.None && (mods&~mod)==KeyMod.None;
  }

  /// <summary>Returns a human-readable name for a virtual key code.</summary>
  /// <param name="key">The virtual key code.</param>
  /// <returns>A human-readable name for the key.</returns>
  public static string KeyName(Key key) { return Enum.GetName(typeof(Key), key); }

  /// <summary>Returns true if the given virtual key is depressed.</summary>
  /// <param name="key">The virtual key to check for.</param>
  /// <returns>True if the given virtual key is depressed and false otherwise.</returns>
  /// <remarks>This method is updated by <see cref="Input.ProcessEvent"/>.</remarks>
  public static bool Pressed(Key key) { return state[(int)key]; }
  /// <summary>Returns true if the given virtual key is depressed, and then marks it as unpressed.</summary>
  /// <param name="key">The virtual key to check for.</param>
  /// <returns>True if the given virtual key is depressed and false otherwise.</returns>
  /// <remarks>After checking whether the key is pressed, this method marks the key as not pressed. This allows you
  /// to easily check for unique key presses. This method is updated by <see cref="Input.ProcessEvent"/>.
  /// </remarks>
  public static bool PressedRel(Key key) { bool ret=Pressed(key); if(ret) state[(int)key]=false; return ret; }
  /// <summary>Marks the specified key as depressed or not.</summary>
  /// <param name="key">The virtual key to mark as depressed or not.</param>
  /// <param name="down">If true, the given virtual key is marked as pressed. Otherwise, it's marked as released.</param>
  public static void Press(Key key, bool down) { state[(int)key]=down; }

  /// <summary>Enables key repeat with default delay values.</summary>
  /// <remarks>This method enables key repeating at a low level, using default delay and interval values (a 500
  /// millisecond delay and 30 millisecond intervals between repeats). WARNING: Using this key repeat is incompatible
  /// with the <see cref="GameLib.Forms.DesktopControl.KeyRepeatDelay"/> and
  /// <see cref="GameLib.Forms.DesktopControl.KeyRepeatRate"/> properties, as the
  /// <see cref="GameLib.Forms.DesktopControl"/> has its own implementation of key repeat.
  /// </remarks>
  public static void EnableKeyRepeat() { EnableKeyRepeat(500, 30); }
  /// <summary>Enables key repeat with the specified delay values, or optionally disables it.</summary>
  /// <param name="delayMs">The number of milliseconds a key has to be held down before it starts repeating.</param>
  /// <param name="intervalMs">The number of milliseconds between key repeats.</param>
  /// <remarks>This method enables key repeating at a low level with the specified delays, or if
  /// <paramref name="delayMs"/> and <paramref name="intervalMs"/> are zero, disables it.
  /// WARNING: Using this key repeat is incompatible
  /// with the <see cref="GameLib.Forms.DesktopControl.KeyRepeatDelay"/> and
  /// <see cref="GameLib.Forms.DesktopControl.KeyRepeatRate"/> properties, as the
  /// <see cref="GameLib.Forms.DesktopControl"/> has its own implementation of key repeat.
  /// </remarks>
  public static void EnableKeyRepeat(int delayMs, int intervalMs)
  { if(SDL.EnableKeyRepeat(delayMs, intervalMs)!=0) SDL.RaiseError();
  }
  /// <summary>Disables key repeat.</summary>
  public static void DisableKeyRepeat() { EnableKeyRepeat(0, 0); }

  internal static void Initialize()
  { unsafe
    { int   i=0, num;
      byte *keys = SDL.GetKeyState(&num);
      if(num>(int)Key.NumKeys) num = (int)Key.NumKeys;
      for(; i<num; i++) state[i]=(keys[i]!=0);
      for(; i<(int)Key.NumKeys; i++) state[i]=false;
    }
  }

  internal static void OnKeyEvent(KeyboardEvent e)
  { mods = e.Mods;
    state[(int)e.Key] = e.Down;
    if(KeyEvent!=null) KeyEvent(e);
  }

  static bool[] state = new bool[(int)Key.NumKeys];
  static KeyMod mods;
}
#endregion

#region Mouse
/// <summary>This class represents the mouse.</summary>
/// <remarks>This class is updated by the <see cref="Input.ProcessEvent"/> method.</remarks>
public sealed class Mouse
{ private Mouse() { }

  /// <summary>Occurs when the mouse is moved.</summary>
  /// <remarks>This event is raised by the <see cref="Input.ProcessEvent"/> method.</remarks>
  public static event MouseMoveHandler  MouseMove;
  /// <summary>Occurs when a mouse button is pressed or released, and when the mouse wheel is moved.</summary>
  /// <remarks>This event is raised by the <see cref="Input.ProcessEvent"/> method.</remarks>
  public static event MouseClickHandler MouseClick;

  /// <summary>Gets/sets the current position of the mouse cursor.</summary>
  /// <remarks>This property is updated by <see cref="Input.ProcessEvent"/>. When setting this property, the position
  /// given is clipped to the application window.
  /// </remarks>
  public static System.Drawing.Point Point
  { get { return new System.Drawing.Point(x, y); }
    set
    { if(value.X<0) value.X=0;
      if(value.Y<0) value.Y=0;
      SDL.WarpMouse((ushort)value.X, (ushort)value.Y);
      SDL.GetMouseState(out x, out y);
    }
  }
  /// <summary>Gets/sets the current X coordinate of the mouse cursor's position.</summary>
  /// <remarks>This property is updated by <see cref="Input.ProcessEvent"/>. When setting this property, the position
  /// given is clipped to the application window. If you want to set both the X and Y coordinates, it's more
  /// efficient to set the <see cref="Point"/> property.
  /// </remarks>
  public static int X
  { get { return x; }
    set { Point = new System.Drawing.Point(value,  y); }
  }
  /// <summary>Gets/sets the current Y coordinate of the mouse cursor's position.</summary>
  /// <remarks>This property is updated by <see cref="Input.ProcessEvent"/>. When setting this property, the position
  /// given is clipped to the application window. If you want to set both the X and Y coordinates, it's more
  /// efficient to set the <see cref="Point"/> property.
  /// </remarks>
  public static int Y
  { get { return y; }
    set { Point = new System.Drawing.Point(x, value); }
  }
  /// <summary>Gets/sets the current Z coordinate of the mouse, which represents the position of the mouse wheel.</summary>
  /// <remarks>This property is updated by <see cref="Input.ProcessEvent"/>.</remarks>
  public static int Z { get { return z; } set { z=value; } }

  /// <summary>Determines whether the system mouse cursor is visible.</summary>
  /// <remarks>If false, you will have to draw your own mouse cursor.</remarks>
  public static bool SystemCursorVisible
  { get { return cursorVisible; }
    set { SDL.ShowCursor(value?1:0); cursorVisible=(SDL.ShowCursor(-1)!=0); }
  }

  /// <summary>Gets a bitfield specifying which buttons are depressed.</summary>
  /// <remarks>The bitfield is packed so the first mouse button is in the low bit, the second mouse button is in the
  /// next bit, etc. This property is updated by <see cref="Input.ProcessEvent"/>.
  /// </remarks>
  public static byte Buttons { get { return buttons; } set { buttons=value; } }
  /// <summary>Returns true if only the specified mouse button is depressed.</summary>
  /// <param name="button">The <see cref="MouseButton"/> to check for.</param>
  /// <returns>True if only the specified button is depressed and false otherwise.</returns>
  /// <remarks>This property is updated by <see cref="Input.ProcessEvent"/>.</remarks>
  public static bool OnlyPressed(MouseButton button) { return buttons==(1<<(byte)button); }
  /// <summary>Returns true if the specified mouse button is depressed.</summary>
  /// <param name="button">The <see cref="MouseButton"/> to check for.</param>
  /// <returns>True if the specified mouse button is depressed and false otherwise.</returns>
  /// <remarks>This method is updated by <see cref="Input.ProcessEvent"/>.</remarks>
  public static bool Pressed(MouseButton button) { return (buttons&(1<<(byte)button))!=0; }
  /// <summary>Returns true if the specified mouse button is depressed, and then marks it as unpressed.</summary>
  /// <param name="button">The <see cref="MouseButton"/> to check for.</param>
  /// <returns>True if the given button is depressed and false otherwise.</returns>
  /// <remarks>After checking whether the button is depressed, this method marks it as not pressed. This allows you
  /// to easily check for unique button presses. This method is updated by <see cref="Input.ProcessEvent"/>.
  /// </remarks>
  public static bool PressedRel(MouseButton button)
  { bool ret = Pressed(button);
    if(ret) SetPressed(button, false);
    return ret;
  }
  /// <summary>Marks a mouse button as depressed or not.</summary>
  /// <param name="button">The <see cref="MouseButton"/> to mark as depressed or not</param>
  /// <param name="down">If true, the mouse button is marked as depressed. Otherwise, it's marked as unpressed.</param>
  public static void SetPressed(MouseButton button, bool down)
  { if(down) buttons |= (byte)(1<<(byte)button);
    else buttons &= (byte)~(1<<(byte)button);
  }

  internal unsafe static void Initialize()
  { int x, y;
    Mouse.buttons = SDL.GetMouseState(&x, &y);
    Mouse.x=x; Mouse.y=y;
    SDL.ShowCursor(1);
  }

  internal static void OnMouseMove(MouseMoveEvent e)
  { x=e.X; y=e.Y; // we don't get the buttons here so that marking a button released isn't undone the next mouse movement
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
/// <summary>This class represents a joystick.</summary>
/// <remarks>This class is updated by <see cref="Input.ProcessEvent"/>.</remarks>
public sealed class Joystick : IDisposable
{ 
  /// <summary>This struct represents a joystick ball (a trackball like device embedded in some joysticks).</summary>
  public struct Ball
  { 
    /// <summary>The position of the ball. This is calculated by summing the relative movements of the ball since
    /// joysticks were enabled.
    /// </summary>
    public System.Drawing.Point Point { get { return new System.Drawing.Point(X, Y); } }
    /// <summary>The X coordinate of the ball. This is calculated by summing the relative movements of the ball since
    /// joysticks were enabled.
    /// </summary>
    public int X;
    /// <summary>The Y coordinate of the ball. This is calculated by summing the relative movements of the ball since
    /// joysticks were enabled.
    /// </summary>
    public int Y;
  }

  /// <summary>Occurs when a joystick axis is moved.</summary>
  /// <remarks>This event will not be raised for joystick movement within the dead or saturation zones (see
  /// <see cref="SetDeadZone"/> and <see cref="SetSaturationZone"/>).
  /// This event is raised by the <see cref="Input.ProcessEvent"/> method.
  /// </remarks>
  public event JoyMoveHandler JoyMove;
  /// <summary>Occurs when a joystick ball is moved.</summary>
  /// <remarks>This event is raised by the <see cref="Input.ProcessEvent"/> method.</remarks>
  public event JoyBallHandler JoyBall;
  /// <summary>Occurs when a joystick POV hat is moved.</summary>
  /// <remarks>This event is raised by the <see cref="Input.ProcessEvent"/> method.</remarks>
  public event JoyHatHandler JoyHat;
  /// <summary>Occurs when a joystick button is pressed or released.</summary>
  /// <remarks>This event is raised by the <see cref="Input.ProcessEvent"/> method.</remarks>
  public event JoyButtonHandler JoyButton;

  internal Joystick(int number)
  { joystick = SDL.JoystickOpen(number);
    unsafe { if(joystick.ToPointer()==null) SDL.RaiseError(); }

    axes    = new int[SDL.JoystickNumAxes(joystick)];
    balls   = new Ball[SDL.JoystickNumBalls(joystick)];
    buttons = new bool[SDL.JoystickNumButtons(joystick)];
    hats    = new HatPosition[SDL.JoystickNumHats(joystick)];
    name    = SDL.JoystickName(number);

    deadpoints = new int[axes.Length];
    satpoints  = new int[axes.Length];
    for(int i=0; i<satpoints.Length; i++) satpoints[i]=32768;

    SDL.JoystickUpdate();
    for(int i=0; i<axes.Length; i++) axes[i] = (short)SDL.JoystickGetAxis(joystick, i); // SDL returns out of range values (<-32k || >32k)
    for(int i=0; i<buttons.Length; i++) buttons[i] = SDL.JoystickGetButton(joystick, i)!=0;
    for(int i=0; i<hats.Length; i++) hats[i] = (HatPosition)SDL.JoystickGetHat(joystick, i);

    this.number=number;
  }
  ~Joystick() { Dispose(true); }
  /// <summary>Frees unmanaged resources held by this object.</summary>
  /// <remarks>The <see cref="Input"/> class is responsible for calling this method, and you should not call it
  /// directly.
  /// </remarks>
  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  /// <summary>Gets the size of an axis' dead zone.</summary>
  /// <param name="axis">The axis whose dead zone will be returned.</param>
  /// <returns>The size of the dead zone, as a percentage of the range on either side of the center.</returns>
  /// <remarks>A dead zone can be used to set aside a portion of the joystick range that be clipped to a dead (zero)
  /// value. This can be useful because most joysticks jitter and do not return zero even when the joystick is
  /// centered on an axis. A dead zone of 10% will set all axis values within plus or minus 10% to zero. Additionally,
  /// the <see cref="JoyMove"/> event will not be raised for joystick movement that occurs solely within the dead zone.
  /// </remarks>
  public int GetDeadZone(int axis) { return (deadpoints[axis]*100+16384)/32768; }
  /// <summary>Sets the size of an axis' dead zone, which determines the percentage of the axis' ranges of motion that
  /// is set to zero. <seealso cref="SetSaturationZone"/>
  /// </summary>
  /// <param name="axis">The axis whose dead zone will be changed.</param>
  /// <param name="value">The percentage of the axis' range of motion from the center that will be converted to zero.</param>
  /// <remarks>A dead zone can be used to set aside a portion of the joystick range that be clipped to a dead (zero)
  /// value. This can be useful because most joysticks jitter and do not return zero even when the joystick is
  /// centered on an axis. A dead zone of 10% will set all axis values within plus or minus 10% to zero. Additionally,
  /// the <see cref="JoyMove"/> event will not be raised for joystick movement that occurs solely within the dead zone.
  /// </remarks>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is less than 0 or greater than
  /// 100.
  /// </exception>
  public void SetDeadZone(int axis, int value)
  { if(value<0 || value>100) throw new ArgumentOutOfRangeException("DeadZone", value, "must be from 0-100");
    deadpoints[axis] = 32768*value/100;
    if(axes[axis]<0)
    { if(axes[axis]>-deadpoints[axis]) axes[axis]=0;
    }
    else if(axes[axis]<deadpoints[axis]) axes[axis]=0;
  }

  /// <summary>Gets the size of an axis' saturation zone.</summary>
  /// <param name="axis">The axis whose saturation zone will be returned.</param>
  /// <returns>The size of the saturation zone, as a percentage of the range from the minimum and maximum values.</returns>
  /// <remarks>A saturation zone can be used to set aside a portion of the joystick range that be set to the
  /// maximum distance from the center. This can be useful because most joysticks jitter and do not consistently
  /// return the maximum even when fully pressed to one side. A saturation zone of 5% will set all axis values within
  /// 5% of the maximum or minimum values to the maximum or minimum values, respectively. This is
  /// especially useful for throttle controls. Additionally, the <see cref="JoyMove"/> event will not be raised for
  /// joystick movement that occurs solely within the saturation zone.
  /// </remarks>
  public int GetSaturationZone(int axis) { return 100-(satpoints[axis]*100+16384)/32768; }
  /// <summary>Sets the size of an axis' saturation zone, which determines the percentage of the axis' ranges of
  /// motion that is set to the maximum value. <seealso cref="SetDeadZone"/>
  /// </summary>
  /// <param name="axis">The axis whose saturation zone will be changed.</param>
  /// <param name="value">The percentage of the axis' range of motion from the edges that will be
  /// converted to the maximum distance from the center.
  /// </param>
  /// <remarks>A saturation zone can be used to set aside a portion of the joystick range that be set to the
  /// maximum distance from the center. This can be useful because most joysticks jitter and do not consistently
  /// return the maximum even when fully pressed to one side. A saturation zone of 5% will set all axis values within
  /// 5% of the maximum or minimum values to the maximum or minimum values, respectively. This is
  /// especially useful for throttle controls. Additionally, the <see cref="JoyMove"/> event will not be raised for
  /// joystick movement that occurs solely within the saturation zone.
  /// </remarks>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is less than 0 or greater than
  /// 100.
  /// </exception>
  public void SetSaturationZone(int axis, int value)
  { if(value<0 || value>100) throw new ArgumentOutOfRangeException("SaturationZone", value, "must be from 0-100");
    satpoints[axis] = 32768*(100-value)/100;
    if(axes[axis]<0)
    { if(axes[axis]<-satpoints[axis]) axes[axis]=short.MinValue;
    }
    else if(axes[axis]>satpoints[axis]) axes[axis]=short.MaxValue;
  }

  /// <summary>Gets an array of this joystick's ball positions.</summary>
  /// <value>A <see cref="Ball"/> array holding the ball positions.</value>
  /// <remarks>A joystick ball is a trackball-like device embedded in some joysticks. Elements of the returned array
  /// can be safely modified.
  /// </remarks>
  public Ball[] Balls { get { return balls; } }
  /// <summary>Gets an array of this joystick's POV hat positions.</summary>
  /// <value>A <see cref="HatPosition"/> array holding the hat positions.</value>
  /// <remarks>Elements of the returned array can be safely modified.</remarks>
  public HatPosition[] Hats { get { return hats; } }
  /// <summary>Gets an array of this joystick's button states.</summary>
  /// <value>A boolean array holding the button states (true means the button is depressed).</value>
  /// <remarks>Elements of the returned array can be safely modified.</remarks>
  public bool[] Buttons { get { return buttons; } }
  /// <summary>Gets an array of this joystick's axis positions.</summary>
  /// <value>An integer array holding positions of the joystick axes. Each axis ranges from -32768 to 32767 in value.</value>
  /// <remarks>Elements of the returned array can be safely modified.</remarks>
  public int[] Axes { get { return axes; } }
  /// <summary>Gets the human-readable name of this joystick device.</summary>
  public string Name { get { return name; } }
  /// <summary>Gets the index of this joystick in the <see cref="Input.Joysticks"/> array.</summary>
  public int Number { get { return number; } }

  internal void OnJoyMove(JoyMoveEvent e)
  { int old = axes[e.Axis];
    if(e.Position<0)
    { if(e.Position>-deadpoints[e.Axis]) e.Position=0;
      else if(e.Position<-satpoints[e.Axis]) e.Position=short.MinValue;
    }
    else if(e.Position<deadpoints[e.Axis]) e.Position=0;
    else if(e.Position>satpoints[e.Axis]) e.Position=short.MaxValue;
    axes[e.Axis] = e.Position;
    if(old!=e.Position && JoyMove!=null) JoyMove(this, e);
  }

  internal void OnJoyBall(JoyBallEvent e)
  { balls[e.Ball].X += e.Xrel; balls[e.Ball].Y += e.Yrel;
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
  int[]         axes, deadpoints, satpoints;
  Ball[]        balls;
  bool[]        buttons;
  HatPosition[] hats;
  string        name;
  int           number;
}
#endregion

#region Input
/// <summary>This class handles input and updates the <see cref="Keyboard"/>, <see cref="Mouse"/>, and
/// <see cref="Joystick"/> classes via the <see cref="ProcessEvent"/> method.
/// </summary>
/// <remarks>It's possible for input to be handled manually without using the input classes (by using raw
/// <see cref="GameLib.Events"/> events), but using them can simplify many types of input processing.
/// </remarks>
public sealed class Input
{ private Input() { }

  /// <summary>Returns true if the input subsystem has been initialized.</summary>
  public static bool Initialized { get { return initCount>0; } }

  /// <summary>Returns an array containing the joysticks present in the system.</summary>
  /// <remarks>The input subsystem must have been initialized with joystick support before this property can be used.
  /// You can use any available methods and properties of the <see cref="Joystick"/> objects, but do not alter the
  /// returned array. This property cannot be used unless joystick support is enabled (by either
  /// <see cref="UseJoysticks"/> or <see cref="Initialize(bool)"/>).
  /// </remarks>
  public static Joystick[] Joysticks { get { return joysticks; } }

  /// <summary>Enables or disables joystick support.</summary>
  /// <remarks>Joysticks cannot be used unless joystick support is enabled. The input system must be initialized
  /// before this property can be used. Due to the fact that some joysticks constantly jitter, sending an endless
  /// stream of events through the event system, you should not enable joystick support unless it's needed.
  /// </remarks>
  public static bool UseJoysticks
  { get { return joysticks!=null; }
    set
    { if(value!=UseJoysticks)
      { if(initCount==0) throw new InvalidOperationException("The input system has not been initialized!");
        if(value)
        { SDL.Initialize(SDL.InitFlag.Joystick);
          joysticks = new Joystick[SDL.NumJoysticks()];
          for(int i=0; i<joysticks.Length; i++) joysticks[i] = new Joystick(i);
          SDL.JoystickEventState(SDL.JoystickMode.Events);
        }
        else
        { SDL.JoystickEventState(SDL.JoystickMode.Poll);
          for(int i=0; i<joysticks.Length; i++) joysticks[i].Dispose();
          joysticks = null;
          SDL.Deinitialize(SDL.InitFlag.Joystick);
        }
      }
    }
  }

  /// <summary>Initializes the input subsystem without enabling joystick support.</summary>
  /// <remarks>This method can be called multiple times. If called while already initialized, joystick support
  /// will not be disabled. The <see cref="Deinitialize"/> method should be called the same number of times to
  /// finally deinitialize the system. Joystick support can be enabled or disabled later with
  /// <see cref="UseJoysticks"/>. Alternately, <see cref="Initialize(bool)"/> can be used to initialize joystick
  /// support from the beginning.
  /// </remarks>
  public static void Initialize() { Initialize(false); }
  /// <summary>Initializes the input subsystem.</summary>
  /// <param name="useJoysticks">Determines whether joystick support will be enabled initially.</param>
  /// <remarks>This method can be called multiple times. If called while already initialized, the joystick support
  /// will not be altered. The <see cref="Deinitialize"/> method should be called the same number of times to
  /// finally deinitialize the system. Joystick support can be enabled or disabled later with
  /// <see cref="UseJoysticks"/>. Due to the fact that some joysticks constantly jitter, sending an endless
  /// stream of events through the event system, you should not enable joystick support unless it's needed.
  /// </remarks>
  public static void Initialize(bool useJoysticks)
  { if(initCount++==0)
    { Events.Events.Initialize();
      Keyboard.Initialize();
      Mouse.Initialize();
      UseJoysticks = useJoysticks;
    }
  }

  /// <summary>Deinitializes the input subsystem.</summary>
  /// <remarks>This method should be called the same number of times that <see cref="Initialize"/> has been called
  /// in order to deinitialize the input subsystem.
  /// </remarks>
  public static void Deinitialize()
  { if(initCount==0) throw new InvalidOperationException("Deinitialize called too many times!");
    if(initCount==1)
    { UseJoysticks = false;
      Events.Events.Deinitialize();
    }
    initCount--;
  }

  /// <summary>Processes input events and updates the <see cref="Keyboard"/>, <see cref="Mouse"/>, and
  /// <see cref="Joystick"/> classes as necessary.
  /// </summary>
  /// <param name="e">The <see cref="Event"/> to process. This event does not have to be related to input.</param>
  /// <returns>True if the event was related to input (and so was processed) and false otherwise.</returns>
  /// <remarks>It is important to call this method on all input events if you want the input classes to be updated
  /// accurately.
  /// </remarks>
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

} // namespace GameLib.Input