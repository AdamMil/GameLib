/*
This is an example application for the GameLib multimedia/gaming library.
http://www.adammil.net/
Copyright (C) 2004 Adam Milazzo

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
using GameLib.Events;
using GameLib.Fonts;
using GameLib.Input;
using GameLib.Video;

namespace InputTest
{

class App
{ 
  #if DEBUG
  static string dataPath = "../../../"; // set to something correct
  #else
  static string dataPath = "data/"; // set to something correct
  #endif

  static void Main()
  { Events.Initialize();
    Input.Initialize(true);
    Video.Initialize();
    
    Video.SetMode(640, 480, 32);
    WM.WindowTitle = "Input Test";

    font = new GameLib.Fonts.TrueTypeFont(dataPath+"mstrr.ttf", 24);
    font.RenderStyle = RenderStyle.Blended;

    Events.PumpEvents(new EventProcedure(EventProc), new IdleProcedure(IdleProc));
    
    Video.Deinitialize();
    Input.Deinitialize();
    Events.Deinitialize();
  }
  
  static bool EventProc(Event e)
  { if(Input.ProcessEvent(e))
    { int ypos = 0;
      if(Keyboard.Pressed(Key.Escape)) return false;
      Video.DisplaySurface.Fill(0);
      font.Render(Video.DisplaySurface,
                  string.Format("Cursor: {0} Wheel: {1} Buttons: {2}",
                                Mouse.Point, Mouse.Z, Mouse.Buttons),
                  0, ypos);
      foreach(Joystick joy in Input.Joysticks)
        font.Render(Video.DisplaySurface,
                    string.Format("Joystick {0}: {1}", joy.Number, joy.Name),
                    0, ypos += font.LineSkip);
      string text = string.Empty;
      switch(e.Type)
      { case EventType.Keyboard:
        { KeyboardEvent ke = (KeyboardEvent)e;
          text = string.Format("Key {0} (char {1}) {2}", ke.Key,
                                ke.Char==0 ? ' ' : ke.Char,
                                ke.Down ? "pressed" : "release");
          break;
        }
        case EventType.MouseMove:
        { MouseMoveEvent mme = (MouseMoveEvent)e;
          text = string.Format("Mouse moved {0},{1} to {2}",
                               mme.Xrel, mme.Yrel, mme.Point);
          break;
        }
        case EventType.MouseClick:
        { MouseClickEvent mce = (MouseClickEvent)e;
          text = string.Format("Mouse button {0} {1}", mce.Button,
                                mce.Down ? "pressed" : "released");
          break;
        }
        case EventType.JoyMove:
        { JoyMoveEvent jme = (JoyMoveEvent)e;
          text = string.Format("Joystick {0} axis {1} moved to {2}",
                               jme.Device, jme.Axis, jme.Position);
          break;
        }
        case EventType.JoyButton:
        { JoyButtonEvent jbe = (JoyButtonEvent)e;
          text = string.Format("Joystick {0} button {1} {2}",
                               jbe.Device, jbe.Button,
                               jbe.Down ? "pressed" : "released");
          break;
        }
        case EventType.JoyHat:
        { JoyHatEvent jhe = (JoyHatEvent)e;
          text = string.Format("Joystick {0} hat {1} moved to {2}",
                               jhe.Device, jhe.Hat, jhe.Position);
          break;
        }
        case EventType.JoyBall:
        { JoyBallEvent jbe = (JoyBallEvent)e;
          text = string.Format("Joystick {0} ball {1} moved by {2}",
                               jbe.Device, jbe.Ball, jbe.Offset);
          break;
        }
      }
      font.Render(Video.DisplaySurface, text, 0, ypos+font.LineSkip);
      updated = true;
    }
    else if(e.Type==EventType.Quit) return false;
    return true;
  }
  
  static bool IdleProc()
  { if(updated)
    { Video.Flip();
      updated = false;
    }
    return false;
  }
  
  static GameLib.Fonts.TrueTypeFont font;
  static bool updated;
}

} // namespace InputTest