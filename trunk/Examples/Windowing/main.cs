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
{ public CustomDesktop()
  { AutoFocusing=AutoFocus.Click; BackColor=SystemColors.AppWorkspace;
    KeyPreview=true;
    menu.Dock=DockStyle.Top;

    Line line = new Line(SystemColors.ControlDarkDark);
    line.Dock = DockStyle.Top;

    Controls.AddRange(menu, line);
  }

  public MenuBarBase Menu { get { return menu; } }
  
  protected override void OnKeyDown(KeyEventArgs e)
  { if(menu!=null) menu.HandleKey(e.KE);
    base.OnKeyDown(e);
  }

  protected override void OnFontChanged(GameLib.ValueChangedEventArgs e)
  { if(Font!=null) menu.Height = Font.LineSkip*2;
    base.OnFontChanged(e);
  }

  MenuBar menu = new MenuBar();
}
#endregion

#region SampleForm
class SampleForm : Form
{ public SampleForm()
  { Style |= ControlStyle.BackingSurface;
    Text = "Sample Form";
    Size = new Size(256, 192);

    #region Add Controls
    bar.Minimum = 64;
    bar.Value   = bar.Maximum = 255;
    bar.Dock    = DockStyle.Right;
    bar.ValueChanged += new GameLib.ValueChangedEventHandler(bar_ValueChanged);
    
    label1.Dock = DockStyle.Top;
    label1.TextAlign = ContentAlignment.MiddleCenter;
    
    btn.Click += new ClickEventHandler(btn_Click);

    edit.Anchor = chk.Anchor = btn.Anchor = AnchorStyle.LeftRight;

    edit.TabIndex = 0;
    chk.TabIndex  = 1;
    btn.TabIndex  = 2;

    Controls.AddRange(bar, label1, edit, chk, btn);
    #endregion
  }

  void UpdateAlpha()
  { if(BackingSurface!=null)
    { BackingSurface.SetSurfaceAlpha((byte)bar.Value);
      Parent.Invalidate(WindowToParent(WindowRect));
    }
    edit.Text = "Alpha = "+bar.Value;
  }

  #region Event handlers
  protected override void OnBackingSurfaceChanged(EventArgs e)
  { UpdateAlpha();
    base.OnBackingSurfaceChanged(e);
  }

  protected override void OnFontChanged(GameLib.ValueChangedEventArgs e)
  { if(Font!=null)
    { int height = Font.LineSkip*3/2;
      label1.Height = height;
      edit.Bounds = new Rectangle(5, 0, LayoutWidth-10, height);
      chk.Bounds = new Rectangle(5, edit.Bottom, edit.Width, height);
      int wid = Font.CalculateSize(btn.Text).Width*2;
      btn.Bounds = new Rectangle((LayoutWidth-wid)/2, chk.Bottom, wid, height);
      Height = Math.Max(192, height*7);
    }
    base.OnFontChanged(e);
  }

  void bar_ValueChanged(object sender, GameLib.ValueChangedEventArgs e)
  { UpdateAlpha();
  }
  #endregion

  #region Controls
  ScrollBar bar = new ScrollBar();
  TextBox  edit = new TextBox();
  Label  label1 = new Label("Use the scrollbar to change transparency.");
  CheckBox chk  = new CheckBox("Check, please.");
  Button   btn  = new Button("Hit me!");
  #endregion

  void btn_Click(object sender, ClickEventArgs e)
  { Random rand = new Random();
    BackColor = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
  }
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
      font.RenderStyle = RenderStyle.Shaded;
      desktop.Font = font; // set the default font
      desktop.KeyRepeatDelay = 350; // 350 ms delay before key repeat
      
      Menu menu = new Menu("Menu", new KeyCombo(KeyMod.Alt, 'M'));

      menu.Add(new MenuItem("MessageBox", 'M', new KeyCombo(KeyMod.Ctrl, 'M')))
        .Click += new EventHandler(MessageBox_Click);
      menu.Add(new MenuItem("Form", 'F', new KeyCombo(KeyMod.Ctrl, 'F')))
        .Click += new EventHandler(Form_Click);
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
    else desktop.UpdateDisplay();
    return true;
  }
  
  static CustomDesktop desktop = new CustomDesktop();

  #region Event handlers
  static void MessageBox_Click(object sender, EventArgs e)
  { string text = "This is a message box. It works much like the message boxes "+
                  "you may be used to. Would you like to blow up the monitor?";
    if(MessageBox.Show(desktop, "Hello", text,
                       new string[] { "Blow it up!", "I think not." }, 1) == 0)
      MessageBox.Show(desktop, "Boom?", "Boom!!! Wait... no, that didn't "+
                                        "work. Lemme try again.");
    else MessageBox.Show(desktop, "Boom?",
                         "Okay, fine. I'll format the hard drive instead!");
  }

  private static void Form_Click(object sender, EventArgs e)
  { Random rand = new Random();
    SampleForm form = new SampleForm();
    form.Parent = desktop;
    form.SetBounds(new Point(rand.Next(desktop.Width-form.Width),
                             rand.Next(desktop.Height-form.Height)),
                   form.Size, BoundsType.Absolute);
  }

  static void Exit_Click(object sender, EventArgs e)
  { Events.PushEvent(new QuitEvent());
  }
  #endregion
}

} // namespace WindowingTest