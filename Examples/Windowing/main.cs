/*
This is an example application for the GameLib multimedia/gaming library.
http://www.adammil.net/
Copyright (C) 2004-2005 Adam Milazzo

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
  class CustomDesktop : Desktop
  {
    public CustomDesktop()
    {
      AutoFocusing = AutoFocus.Click;
      BackColor    = SystemColors.AppWorkspace;
      Renderer     = new SurfaceControlRenderer();

      menu.Dock = DockStyle.Top;

      Line line = new Line(SystemColors.ControlDarkDark);
      line.Dock = DockStyle.Top;

      Controls.AddRange(menu, line);
    }

    public MenuBarBase Menu { get { return menu; } }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      if(menu != null && !e.Handled) e.Handled = menu.HandleKey(e.KE);
      base.OnKeyDown(e);
    }

    protected override void OnEffectiveFontChanged(GameLib.ValueChangedEventArgs e)
    { 
      // we want things to adjust to the font size
      if(EffectiveFont != null) menu.Height = EffectiveFont.LineSkip*3/2;
      base.OnEffectiveFontChanged(e);
    }

    MenuBar menu = new MenuBar();
  }
  #endregion

  #region SampleForm
  class SampleForm : Form
  {
    public SampleForm()
    {
      Text = "Sample Form";
      MinimumSize = Size = new Size(336, 192);
      BorderStyle = BorderStyle.Resizeable; // allow the form to be resized

      #region Add Controls
      bar.Minimum = 64; // allow alpha values from 64-255
      bar.Value   = bar.Maximum = 255;
      bar.Dock    = DockStyle.Right; // dock the bar on the right
      bar.ValueChanged += new GameLib.ValueChangedEventHandler(bar_ValueChanged);

      label1.Dock = DockStyle.Top;
      label1.TextAlign = ContentAlignment.MiddleCenter;

      btn.Click += btn_Click;
      // have these controls resize as the form resizes
      edit.Anchor = chk.Anchor = btn.Anchor = AnchorStyle.LeftRight;

      edit.TabIndex = 0; // let the user tab between them
      chk.TabIndex  = 1;
      btn.TabIndex  = 2;

      Controls.AddRange(bar, label1, edit, chk, btn);
      #endregion

      UpdateAlpha();
    }

    void UpdateAlpha() // called to change the window alpha
    {
      if(bar.Value < 255)
      {
        ControlStyle |= ControlStyle.CustomDrawTarget;
        ((Surface)GetDrawTarget()).SetSurfaceAlpha((byte)bar.Value);
      }
      else ControlStyle &= ~ControlStyle.CustomDrawTarget;
      Invalidate();
      edit.Text = "Alpha = " + bar.Value;
    }

    #region Event handlers
    protected override void OnDrawTargetChanged()
    {
      base.OnDrawTargetChanged();

      if(GetDrawTarget() != Desktop.DrawTarget)
      {
        ((Surface)GetDrawTarget()).SetSurfaceAlpha((byte)bar.Value);
      }
    }

    // make everything scale to the font size. normally not necessary if you
    // know the font, but want we everything to work properly regardless
    protected override void OnEffectiveFontChanged(GameLib.ValueChangedEventArgs e)
    {
      if(EffectiveFont != null)
      {
        int height    = EffectiveFont.LineSkip*3/2, contentWidth = ContentRect.Width - bar.Width;
        label1.Height = height;
        edit.Bounds   = new Rectangle(5, 0, contentWidth-10, height);
        chk.Bounds    = new Rectangle(5, edit.Bottom, edit.Width, height);

        int wid = EffectiveFont.CalculateSize(btn.Text).Width*2;
        btn.Bounds = new Rectangle((contentWidth-wid)/2, chk.Bottom, wid, height);

        Height = Math.Max(192, height*7);
      }

      base.OnEffectiveFontChanged(e);
    }

    void bar_ValueChanged(object sender, GameLib.ValueChangedEventArgs e)
    {
      UpdateAlpha(); // update the alpha value when the scrollbar changes
    }

    void btn_Click(object sender, ClickEventArgs e)
    {
      Random rand = new Random(); // change everything to a random color
      BackColor = bar.ForeColor = Color.FromArgb(rand.Next(255), rand.Next(255),
                                                 rand.Next(255));
      bar.BackColor = UIHelper.GetDarkColor(bar.ForeColor);
    }
    #endregion

    #region Controls
    ScrollBar bar = new ScrollBar();
    TextBox edit = new TextBox();
    Label label1 = new Label("Use the scrollbar to change transparency.");
    CheckBox chk  = new CheckBox("Check, please.");
    Button btn  = new Button("Hit me!");
    #endregion
  }
  #endregion

  #region ControlsForm
  public class ControlsForm : Form
  {
    public ControlsForm()
    {
      Text = "Controls Form";
      Size = new Size(320, 240);
      Padding = new RectOffset(4);

      #region Add Controls
      list = new ListBox();
      list.Items.AddRange("Apple", "Beet", "Cabbage", "Carrot", "Coconut",
                          "Date", "Grape", "Kiwifruit", "Limes", "Mango",
                          "Orange", "Papaya");
      for(int i=1; i<=50; i++) list.Items.Add("Item "+i);
      list.Width = 75;

      combo = new ComboBox(list.Items);
      combo.Left  = list.Right + 4;
      combo.Width = list.Width + 20;
      combo.SelectedIndex = 0;
      combo.DropDownStyle = ComboBoxStyle.DropDownList;
      Controls.AddRange(list, combo);
      #endregion
    }

    protected override void LayOutChildren()
    {
      base.LayOutChildren();
      combo.ListBoxHeight = list.Height = list.GetPreferredSize(10).Height;
    }

    ListBox list;
    ComboBox combo;
  }
  #endregion

  class App
  {
#if DEBUG
    static string dataPath = "../../../"; // set to something correct
#else
    static string dataPath = "data/"; // set to something correct
#endif

    [STAThread] // set this so we can use the clipboard
    static void Main()
    {
      Video.Initialize();
      Keyboard.EnableKeyRepeat();
      SetMode(640, 480, false);

      #region Setup controls
      {
        TrueTypeFont font = new TrueTypeFont(dataPath+"vera.ttf", 11);
        font.RenderStyle  = RenderStyle.Shaded; // make the font look pretty
        desktop.Font = font;          // set the default font

        Menu menu = new Menu("Menu", new KeyCombo(KeyMod.Alt, 'M'));
        menu.Add(new MenuItem("Toggle font size")).Click += ToggleFont_Click;
        menu.Add(new MenuItem("Toggle fullscreen", 'T', new KeyCombo(KeyMod.Alt, Key.Enter)))
            .Click += ToggleFS_Click;
        menu.Add(new MenuItem("Message box", 'M', new KeyCombo(KeyMod.Ctrl, 'M')))
            .Click += MessageBox_Click;
        menu.Add(new MenuItem("Form 1", 'F', new KeyCombo(KeyMod.Ctrl, 'F')))
            .Click += Form1_Click;
        menu.Add(new MenuItem("Form 2", 'C', new KeyCombo(KeyMod.Ctrl, 'C')))
            .Click += Form2_Click;
        menu.Add(new MenuItem("Exit", 'Q', new KeyCombo(KeyMod.Ctrl, 'Q')))
            .Click += Exit_Click;
        desktop.Menu.Add(menu);

        desktop.Menu.Add(new Menu("Disabled")).Enabled=false;

        menu = new Menu("Dummy", new KeyCombo(KeyMod.Alt, 'D'));
        menu.Add(new MenuItem("Dummy 1", '1', new KeyCombo(KeyMod.Alt, Key.F1)));
        menu.Add(new MenuItem("Dummy 2", '2', new KeyCombo(KeyMod.Alt, Key.F2))).Enabled=false;
        menu.Add(new MenuItem("Dummy 3", '3', new KeyCombo(KeyMod.Alt, Key.F3)));
        desktop.Menu.Add(menu);
      }
      #endregion

      Events.Initialize();
      Events.PumpEvents(new EventProcedure(EventProc));

      Video.Deinitialize();
    }

    static void SetMode(int width, int height)
    {
      SetMode(width, height,
              Video.DisplaySurface.HasFlag(SurfaceFlag.Fullscreen));
    }

    static void SetMode(int width, int height, bool fullScreen)
    {
      Video.SetMode(width, height, 32, fullScreen ? SurfaceFlag.Fullscreen : SurfaceFlag.Resizeable);
      WM.WindowTitle = "Windowing Example";
      desktop.Bounds     = Video.DisplaySurface.Bounds;
      desktop.DrawTarget = Video.DisplaySurface;
    }

    static bool EventProc(Event e) // fairly standard event handler
    {
      if(desktop.ProcessEvent(e))
      {
        if(desktop.Updated)
        {
          Video.Flip();
          desktop.Updated = false;
        }
      }
      else
      {
        if(e is RepaintEvent) Video.Flip();
        else if(e is ResizeEvent)
        {
          ResizeEvent re = (ResizeEvent)e;
          SetMode(re.Width, re.Height);
        }
        else if(e is ExceptionEvent) throw ((ExceptionEvent)e).Exception;
        else if(e is QuitEvent) return false;
      }
      return true;
    }

    static CustomDesktop desktop = new CustomDesktop();
    static ControlsForm conForm;

    #region Event handlers
    static void ToggleFont_Click(object sender, EventArgs e)
    {
      TrueTypeFont oldFont = (TrueTypeFont)desktop.Font;
      TrueTypeFont newFont = new TrueTypeFont(dataPath+"vera.ttf", oldFont.PointSize == 11 ? 14 : 11);
      newFont.RenderStyle = oldFont.RenderStyle;
      desktop.Font = newFont;
      GameLib.Utility.Dispose(oldFont);
    }

    static void ToggleFS_Click(object sender, EventArgs e)
    {
      SetMode(640, 480, !Video.DisplaySurface.HasFlag(SurfaceFlag.Fullscreen));
    }

    // uses simple and complex message boxes
    static void MessageBox_Click(object sender, EventArgs e)
    {
      string text = "This is a message box. It works much like the ones you "+
                    "may be used to. Would you like to blow up the monitor?";
      if(MessageBox.Show(desktop, "Hello", text,
                         new string[] { "Blow it up!", "I think not." }, 1) == 0)
      {
        MessageBox.Show(desktop, "Boom?", "Boom!!! Wait... no, that didn't quite work.");
      }
      else
      {
        MessageBox.Show(desktop, "Boom?", "Okay, fine. I guess I'll just format the hard drive.");
      }
    }

    static void Form1_Click(object sender, EventArgs e)
    {
      SampleForm form = new SampleForm();
      form.Parent = desktop;

      Random rand = new Random(); // place it randomly on the desktop
      form.Location = new Point(rand.Next(desktop.ContentRect.Width-form.Width),
                                rand.Next(desktop.ContentRect.Height-form.Height));
      form.Focus(); // make it the active form
    }

    static void Form2_Click(object sender, EventArgs e)
    {
      if(conForm==null || conForm.Parent==null) // if closed or never opened,
      {
        conForm = new ControlsForm();           // create a new one
        conForm.Parent   = desktop;
        conForm.Location = new Point(160, 130); // place it at a fixed point
      }
      conForm.Focus(); // bring it to the front
    }

    static void Exit_Click(object sender, EventArgs e)
    {
      Events.PushEvent(new QuitEvent());
    }
    #endregion
  }

} // namespace WindowingTest
