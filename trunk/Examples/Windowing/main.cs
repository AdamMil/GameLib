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
using System.Drawing;
using GameLib.Events;
using GameLib.Fonts;
using GameLib.Forms;
using GameLib.Input;
using GameLib.Video;

namespace WindowingTest
{

#region CustomDesktop
class CustomDesktop : DesktopControl
{ public CustomDesktop() { KeyPreview = true; }

  public MenuBarBase Menu
  { get { return menu; }
    set
    { if(value!=menu)
      { if(menu!=null) Controls.Remove(menu);
        menu = value;
        Controls.Add(menu);
      }
    }
  }
  
  protected override void OnKeyDown(KeyEventArgs e)
  { if(menu!=null) menu.HandleKey(e.KE);
    base.OnKeyDown(e);
  }

  MenuBarBase menu;
}
#endregion

class App
{ 
  #if DEBUG
  static string dataPath = "../../../"; // set to something correct
  #else
  static string dataPath = "data/"; // set to something correct
  #endif

  static void Main()
  { Video.Initialize();
    SetMode(640, 480);

    #region Setup controls
    { TrueTypeFont font = new TrueTypeFont(dataPath+"mstrr.ttf", 12);
      font.RenderStyle = RenderStyle.Blended;
      desktop.Font = font; // set the default font
      desktop.KeyRepeatDelay = 350; // 350 ms delay before key repeat
      
      desktop.Menu = new MenuBar();
      desktop.Menu.Bounds = new Rectangle(0, 0, desktop.Width, font.LineSkip*2);
      desktop.Menu.Dock   = DockStyle.Top;

      Menu menu   = new Menu("Menu", new KeyCombo(KeyMod.Alt, 'M'));

      menu.Add(new MenuItem("MessageBox", 'M', new KeyCombo(KeyMod.Ctrl, 'M')))
        .Click += new EventHandler(MessageBox_Click);
      menu.Add(new MenuItem("Exit", 'X', new KeyCombo(KeyMod.Ctrl, 'X')))
        .Click += new EventHandler(Exit_Click);
        
      desktop.Menu.Add(menu);
    }
    #endregion

    Events.Initialize();
    Events.PumpEvents(new EventProcedure(EventProc));

    Video.Deinitialize();
  }
  
  static void SetMode(int width, int height)
  { Video.SetMode(width, height, 32, SurfaceFlag.Resizeable);
    WM.WindowTitle = "Windowing Example";
    desktop.Bounds  = Video.DisplaySurface.Bounds;
    desktop.Surface = Video.DisplaySurface;
  }

  static bool EventProc(Event e)
  { if(!desktop.ProcessEvent(e))
    { if(e is RepaintEvent) Video.Flip();
      else if(e is ResizeEvent)
      { ResizeEvent re = (ResizeEvent)e;
        SetMode(re.Width, re.Height);
      }
      else if(e is ExceptionEvent) throw ((ExceptionEvent)e).Exception;
      else if(e is QuitEvent) return false;
    }
    else if(desktop.Updated)
    { Video.UpdateRects(desktop.UpdatedAreas, desktop.NumUpdatedAreas);
      desktop.Updated = false;
    }
    return true;
  }
  
  static CustomDesktop desktop = new CustomDesktop();

  #region Event handlers
  static void MessageBox_Click(object sender, EventArgs e)
  { if(MessageBox.Show(desktop, "Hello", "This is a message box. It works much like the message boxes you may be used to. Would you like to blow up the monitor?",
                       new string[] { "I think not", "Blow it up!" }) == 0)
      MessageBox.Show(desktop, "Boom?", "Okay, fine.");
    else MessageBox.Show(desktop, "Boom?", "Boom!!!");
  }

  static void Exit_Click(object sender, EventArgs e)
  { Events.PushEvent(new QuitEvent());
  }
  #endregion
}

} // namespace WindowingTest