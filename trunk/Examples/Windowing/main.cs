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

/* I hate being limited to ~80 columns */

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
  { // we want things to adjust to the font size
    if(Font!=null) menu.Height = Font.LineSkip*3/2;
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
    MinimumSize = Size = new Size(256, 192);
    BorderStyle = BorderStyle.Resizeable; // allow the form to be resized

    #region Add Controls
    bar.Minimum = 64; // allow alpha values from 64-255
    bar.Value   = bar.Maximum = 255;
    bar.Dock    = DockStyle.Right; // dock the bar on the right
    bar.ValueChanged += new GameLib.ValueChangedEventHandler(bar_ValueChanged);
    
    label1.Dock = DockStyle.Top;
    label1.TextAlign = ContentAlignment.MiddleCenter;
    
    btn.Click += new ClickEventHandler(btn_Click);
    // have these controls resize as the form resizes
    edit.Anchor = chk.Anchor = btn.Anchor = AnchorStyle.LeftRight;

    edit.TabIndex = 0; // let the user tab between them
    chk.TabIndex  = 1;
    btn.TabIndex  = 2;

    Controls.AddRange(bar, label1, edit, chk, btn);
    #endregion
  }

  void UpdateAlpha() // called to change the window alpha
  { if(BackingSurface!=null)
    { BackingSurface.SetSurfaceAlpha((byte)bar.Value);
      Invalidate();
    }
    edit.Text = "Alpha = "+bar.Value;
  }

  #region Event handlers
  protected override void OnBackingSurfaceChanged(EventArgs e)
  { UpdateAlpha();
    base.OnBackingSurfaceChanged(e);
  }

  // make everything scale to the font size. normally not necessary if you
  // know the font, but want we everything to work properly regardless
  protected override void OnFontChanged(GameLib.ValueChangedEventArgs e)
  { if(Font!=null)
    { int height = Font.LineSkip*3/2;
      label1.Height = height;
      edit.Bounds = new Rectangle(5, 0, ContentWidth-10, height);
      chk.Bounds = new Rectangle(5, edit.Bottom, edit.Width, height);
      int wid = Font.CalculateSize(btn.Text).Width*2;
      btn.Bounds = new Rectangle((ContentWidth-wid)/2, chk.Bottom, wid, height);
      Height = Math.Max(192, height*7);
    }
    base.OnFontChanged(e);
  }

  void bar_ValueChanged(object sender, GameLib.ValueChangedEventArgs e)
  { UpdateAlpha(); // update the alpha value when the scrollbar changes
  }

  void btn_Click(object sender, ClickEventArgs e)
  { Random rand = new Random(); // change everything to a random color
    BackColor = bar.ForeColor = Color.FromArgb(rand.Next(255), rand.Next(255),
                                               rand.Next(255));
    bar.BackColor = Helpers.GetDarkColor(bar.ForeColor);
  }
  #endregion

  #region Controls
  ScrollBar bar = new ScrollBar();
  TextBox  edit = new TextBox();
  Label  label1 = new Label("Use the scrollbar to change transparency.");
  CheckBox chk  = new CheckBox("Check, please.");
  Button   btn  = new Button("Hit me!");
  #endregion
}
#endregion

#region ControlsForm
public class ControlsForm : Form
{ public ControlsForm()
  { Text = "Controls Form";
    Size = new Size(320, 240);
    Padding = new RectOffset(4);

    #region Add Controls
    ListBox list = new ListBox();
    for(int i=1; i<=100; i++) list.Items.Add("Item "+i);
    list.Size = new Size(75, 100);

    Controls.AddRange(list);
    TriggerLayout(true);
    #endregion
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
    { TrueTypeFont font = new TrueTypeFont(dataPath+"vera.ttf", 11);
      font.RenderStyle = RenderStyle.Shaded; // make the font look pretty
      desktop.Font = font; // set the default font
      desktop.KeyRepeatDelay = 350; // 350 ms delay before key repeat
      
      Menu menu = new Menu("Menu", new KeyCombo(KeyMod.Alt, 'M'));
      menu.Add(new MenuItem("MessageBox", 'M', new KeyCombo(KeyMod.Ctrl, 'M')))
        .Click += new EventHandler(MessageBox_Click);
      menu.Add(new MenuItem("Form 1", 'F', new KeyCombo(KeyMod.Ctrl, 'F')))
        .Click += new EventHandler(Form1_Click);
      menu.Add(new MenuItem("Form 2", 'C', new KeyCombo(KeyMod.Ctrl, 'C')))
        .Click += new EventHandler(Form2_Click);
      menu.Add(new MenuItem("Exit", 'Q', new KeyCombo(KeyMod.Ctrl, 'Q')))
        .Click += new EventHandler(Exit_Click);
      desktop.Menu.Add(menu);
      
      menu = new Menu("Dummy", new KeyCombo(KeyMod.Alt, 'D'));
      menu.Add(new MenuItem("Dummy 1", '1', new KeyCombo(KeyMod.Alt, Key.F1)));
      menu.Add(new MenuItem("Dummy 2", '2', new KeyCombo(KeyMod.Alt, Key.F2)));
      menu.Add(new MenuItem("Dummy 3", '3', new KeyCombo(KeyMod.Alt, Key.F3)));
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

  static bool EventProc(Event e) // fairly standard event handler
  { if(desktop.ProcessEvent(e)) desktop.UpdateDisplay();
    else
    { if(e is RepaintEvent) Video.Flip();
      else if(e is ResizeEvent)
      { ResizeEvent re = (ResizeEvent)e;
        SetMode(re.Width, re.Height);
      }
      else if(e is ExceptionEvent) throw ((ExceptionEvent)e).Exception;
      else if(e is QuitEvent) return false;
    }
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
                                        "quite work.");
    else MessageBox.Show(desktop, "Boom?", "Okay, fine. I guess I'll just "+
                         "format the hard drive.");
  }

  private static void Form1_Click(object sender, EventArgs e)
  { SampleForm form = new SampleForm();
    form.Parent = desktop;

    Random rand = new Random(); // place it randomly on the desktop
    form.Location = new Point(rand.Next(desktop.ContentWidth-form.Width),
                              rand.Next(desktop.ContentHeight-form.Height));
  }

  private static void Form2_Click(object sender, EventArgs e)
  { ControlsForm form = new ControlsForm();
    form.Parent   = desktop;
    form.Location = new Point(10, 10);
  }

  static void Exit_Click(object sender, EventArgs e)
  { Events.PushEvent(new QuitEvent());
  }
  #endregion
}

} // namespace WindowingTest
