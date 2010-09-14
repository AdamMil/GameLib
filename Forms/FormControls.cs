/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2007 Adam Milazzo

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

// TODO: add documentation
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading;
using GameLib.Video;
using GameLib.Input;
using Font = GameLib.Fonts.Font;

namespace GameLib.Forms
{

#region FormBase
// TODO: make TAB start tabbing through child controls
public enum ButtonClicked
{
  None, Abort, Cancel, Ignore, No, Ok, Retry, Yes
}

public abstract class FormBase : Control
{
  protected FormBase()
  {
    ControlStyle |= ControlStyle.CanReceiveFocus | ControlStyle.Draggable;
    ForeColor     = SystemColors.ControlText;
    BackColor     = SystemColors.Control;
    DragThreshold = 3;
  }

  #region TitleBarBase
  public abstract class TitleBarBase : Control
  {
    protected TitleBarBase(FormBase parent)
    {
      if(parent == null) throw new ArgumentNullException();
      ControlStyle |= ControlStyle.Draggable | ControlStyle.DontLayout;
      BackColor     = SystemColors.InactiveCaption;
      ForeColor     = SystemColors.InactiveCaptionText;
      DragThreshold = 3;
      parent.Controls.Add(this);
    }

    public abstract bool CloseBox { get; set; }

    protected internal override void OnDragStart(DragEventArgs e)
    {
      if(!e.OnlyPressed(MouseButton.Left)) e.Cancel = true;
      base.OnDragStart(e);
    }

    protected internal override void OnDragMove(DragEventArgs e)
    {
      DragParent(e);
      base.OnDragMove(e);
    }

    protected internal override void OnDragEnd(DragEventArgs e)
    {
      DragParent(e);
      base.OnDragEnd(e);
    }

    protected override void OnResize()
    {
      FormBase parent = (FormBase)Parent;
      if(parent.MinimumHeight < Height) parent.MinimumHeight = Height;
      base.OnResize();
      parent.OnContentSizeChanged();
    }

    void DragParent(DragEventArgs e)
    {
      Point location = Parent.Location;
      location.Offset(e.End.X - e.Start.X, e.End.Y - e.Start.Y);
      Parent.Location = location;
    }
  }
  #endregion

  public ButtonClicked Button
  {
    get { return button; }
    set { button = value; }
  }

  public override RectOffset ContentOffset
  {
    get
    {
      RectOffset ret = base.ContentOffset;
      ret.Top += TitleBar.Height;
      return ret;
    }
  }

  public int MinimumHeight
  {
    get { return minSize.Height; }
    set { MinimumSize = new Size(value, MinimumHeight); }
  }

  public Size MinimumSize
  {
    get { return minSize; }
    set
    {
      if(value.Width < 0 || value.Height < 0)
      {
        throw new ArgumentOutOfRangeException("MinimumSize", value, "must not be negative");
      }

      minSize  = value;
      Size = new Size(Math.Max(Width, value.Width), Math.Max(Height, value.Height));
    }
  }

  public int MinimumWidth
  {
    get { return minSize.Width; }
    set { MinimumSize = new Size(value, MinimumHeight); }
  }

  public int MaximumHeight
  {
    get { return maxSize.Height; }
    set { MaximumSize = new Size(MaximumWidth, value); }
  }

  public Size MaximumSize
  {
    get { return maxSize; }
    set
    {
      if(value.Width < -1 || value.Height < -1)
      {
        throw new ArgumentOutOfRangeException("MaximumSize", value, "must be greater than or equal to -1");
      }
      maxSize = value;
      Size = new Size(value.Width  == -1 ? Width  : Math.Min(Width,  value.Width),
                      value.Height == -1 ? Height : Math.Min(Height, value.Height));
    }
  }

  public int MaximumWidth
  {
    get { return maxSize.Width; }
    set { MaximumSize = new Size(value, MaximumHeight); }
  }

  public abstract TitleBarBase TitleBar { get; }

  public object DialogResult
  {
    get; set;
  }

  public void Close()
  {
    if(Parent == null) return;

    System.ComponentModel.CancelEventArgs e = new System.ComponentModel.CancelEventArgs();
    OnClosing(e);
    if(!e.Cancel)
    {
      OnClosed();
      Parent = null;
    }
  }

  public object ShowDialog(Desktop desktop)
  {
    if(desktop == null) throw new ArgumentNullException();
    Visible = true;
    Parent = desktop;
    Focus();
    SetModal(true);
    while(Events.Events.PumpEvent(true) && Parent != null) ;
    return DialogResult;
  }

  public event System.ComponentModel.CancelEventHandler Closing;
  public event EventHandler Closed;

  protected override void OnGotFocus()
  {
    BringToFront();
    base.OnGotFocus();
  }

  protected internal override void OnDragStart(DragEventArgs e)
  {
    if(!e.Cancel && BorderStyle == BorderStyle.Resizeable && e.OnlyPressed(MouseButton.Left))
    {
      int bwidth = BorderWidth;
      drag = DragEdge.None;

      if(e.Start.X >= Width - bwidth) drag |= DragEdge.Right;
      else if(e.Start.X < bwidth) drag |= DragEdge.Left;

      if(drag != DragEdge.None)
      {
        if(e.Start.Y < 16) drag |= DragEdge.Top;
        else if(e.Start.Y >= Height - 16) drag |= DragEdge.Bottom;
      }

      if(e.Start.Y < bwidth || e.Start.Y >= Height - bwidth)
      {
        if(e.Start.X < 16) drag |= DragEdge.Left;
        else if(e.Start.X >= Width - 16) drag |= DragEdge.Right;
        if(e.Start.Y >= Height - bwidth) drag |= DragEdge.Bottom;
        else drag |= DragEdge.Top;
      }

      e.Cancel = drag == DragEdge.None;
    }
    else
    {
      e.Cancel = true;
    }

    base.OnDragStart(e);
  }

  protected internal override void OnDragMove(DragEventArgs e)
  {
    DragResize(e);
    base.OnDragMove(e);
  }

  protected internal override void OnDragEnd(DragEventArgs e)
  {
    DragResize(e);
    base.OnDragEnd(e);
  }

  protected virtual void OnClosing(System.ComponentModel.CancelEventArgs e)
  {
    if(Closing != null) Closing(this, e);
  }

  protected virtual void OnClosed()
  {
    if(Closed != null) Closed(this, EventArgs.Empty);
  }

  [Flags]
  enum DragEdge
  {
    None = 0, Left = 1, Right = 2, Top = 4, Bottom = 8
  };

  void DragResize(DragEventArgs e)
  {
    int xd = e.End.X - e.Start.X, yd = e.End.Y - e.Start.Y;
    Rectangle bounds = Bounds;
    if((drag & DragEdge.Right) != 0) { bounds.Width += xd; e.Start.X = e.End.X; }
    else if((drag & DragEdge.Left) != 0) { bounds.X += xd; bounds.Width -= xd; }
    if((drag & DragEdge.Bottom) != 0) { bounds.Height += yd; e.Start.Y = e.End.Y; }
    else if((drag & DragEdge.Top) != 0) { bounds.Y += yd; bounds.Height -= yd; }

    if(bounds.Width < minSize.Width)
    {
      if((drag & DragEdge.Left) != 0) bounds.X += bounds.Width - minSize.Width;
      else e.Start.X -= bounds.Width - minSize.Width;
      bounds.Width = minSize.Width;
    }
    else if(maxSize.Width != -1 && bounds.Width > maxSize.Width)
    {
      if((drag & DragEdge.Left) != 0) bounds.X += bounds.Width - maxSize.Width;
      else e.Start.X -= bounds.Width - maxSize.Width;
      bounds.Width = maxSize.Width;
    }

    if(bounds.Height < minSize.Height)
    {
      if((drag & DragEdge.Top) != 0) bounds.Y += bounds.Height - minSize.Height;
      else e.Start.Y -= bounds.Height - minSize.Height;
      bounds.Height = minSize.Height;
    }
    else if(maxSize.Height != -1 && bounds.Height > maxSize.Height)
    {
      if((drag & DragEdge.Top) != 0) bounds.Y += bounds.Height - maxSize.Height;
      else e.Start.Y -= bounds.Height - maxSize.Height;
      bounds.Height = maxSize.Height;
    }

    if(Bounds != bounds)
    {
      TriggerLayout(true); Bounds = bounds;
    }
  }

  protected override void LayOutChildren()
  {
    base.LayOutChildren();

    Rectangle rect = Rectangle.Inflate(ControlRect, -BorderWidth, -BorderWidth);
    rect.Height = TitleBar.Height;
    TitleBar.SetBounds(rect, true);
  }

  Size minSize = new Size(100, 24), maxSize = new Size(-1, -1);
  ButtonClicked button;
  DragEdge drag;
}
#endregion

#region Form
// TODO: add menu bar?
public class Form : FormBase
{
  public Form()
  {
    titleBar    = new MyTitleBar(this);
    BorderStyle = BorderStyle.FixedThick;
    KeyPreview  = true;
  }

  #region MyTitleBar
  class MyTitleBar : TitleBarBase
  {
    public MyTitleBar(FormBase parent) : base(parent)
    {
      UpdateSize();
      CloseBox = true;
    }

    public override bool CloseBox
    {
      get { return close != null; }
      set
      {
        if(value != CloseBox)
        {
          if(value)
          {
            close = new CloseButton();
            close.Anchor = AnchorStyle.TopBottom | AnchorStyle.Right;
            close.Bounds = new Rectangle(Right - Height + 2, 2, Height - 4, Height - 4);
            Controls.Add(close);
          }
          else
          {
            Controls.Remove(close);
            close = null;
          }
        }
      }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      base.OnPaint(e);

      if(EffectiveFont != null)
      {
        Rectangle rect = GetDrawRect();
        rect.X += (EffectiveFont.LineHeight + 3) / 6;
        EffectiveFont.Color     = GetEffectiveForeColor();
        EffectiveFont.BackColor = GetEffectiveBackColor();
        Point renderPt = new Point(rect.X, rect.Y + (rect.Height - EffectiveFont.Height) / 2);
        e.Target.DrawText(EffectiveFont, Parent.Text, renderPt);
      }
    }

    protected override void OnEffectiveFontChanged(ValueChangedEventArgs e)
    {
      base.OnEffectiveFontChanged(e);
      UpdateSize();
      Invalidate();
    }

    protected override void OnVisibleChanged(ValueChangedEventArgs e)
    {
      UpdateSize();
      base.OnVisibleChanged(e);
    }

    protected override void OnSizeChanged(ValueChangedEventArgs e)
    {
      base.OnSizeChanged(e);

      if(Parent != null)
      {
        int oldHeight = ((Size)e.OldValue).Height;
        if(oldHeight != Height) Parent.TriggerLayout();
      }
    }

    void UpdateSize()
    {
      if(!Visible) Height = 0;
      else if(EffectiveFont != null) Height = EffectiveFont.LineHeight * 4 / 3;
    }

    #region CloseButton
    class CloseButton : Button
    {
      public CloseButton()
      {
        BackColor = SystemColors.Control;
        ForeColor = SystemColors.ControlText;
      }

      protected override void OnResize()
      {
        Rectangle rect = Bounds;
        int xd = Width - Height;
        if(xd != 0) // if it's not square, make it square
        {
          rect.X     += xd;
          rect.Width -= xd;
          Bounds      = rect;
        }

        base.OnResize();
      }

      protected override void OnClick(ClickEventArgs e)
      {
        ((FormBase)Parent.Parent).Close();
        base.OnClick(e);
      }

      protected override void OnPaint(PaintEventArgs e)
      {
        Rectangle rect = GetContentDrawRect();
        float size = (float)rect.Width * 3 / 5;
        int pad = (int)Math.Round((rect.Width - size) / 2), isize = (int)Math.Round(size);
        rect.X      += pad;
        rect.Width  -= isize - (rect.Width & 1) - 1;
        rect.Y      += pad;
        rect.Height -= isize - (rect.Height & 1);

        int right = rect.Right - 1, bottom = rect.Bottom - 1;

        Color c = EffectivelyEnabled ? GetEffectiveForeColor() : SystemColors.GrayText;
        e.Renderer.DrawLine(e.Target, new Point(rect.X, rect.Y), new Point(right - 1, bottom), c, false);
        e.Renderer.DrawLine(e.Target, new Point(rect.X + 1, rect.Y), new Point(right, bottom), c, false);
        e.Renderer.DrawLine(e.Target, new Point(rect.X, bottom), new Point(right - 1, rect.Y), c, false);
        e.Renderer.DrawLine(e.Target, new Point(rect.X + 1, bottom), new Point(right, rect.Y), c, false);

        base.OnPaint(e);
      }
    }
    #endregion

    CloseButton close;
  }
  #endregion

  public override TitleBarBase TitleBar
  {
    get { return titleBar; }
  }

  protected override void OnGotFocus()
  {
    base.OnGotFocus();
    BorderColor = SystemColors.ActiveBorder;
    TitleBar.ForeColor = SystemColors.ActiveCaptionText;
    TitleBar.BackColor = SystemColors.ActiveCaption;
  }

  protected override void OnLostFocus()
  {
    base.OnLostFocus();
    BorderColor = SystemColors.InactiveBorder;
    TitleBar.ForeColor = SystemColors.InactiveCaptionText;
    TitleBar.BackColor = SystemColors.InactiveCaption;
  }

  protected internal override void OnKeyDown(KeyEventArgs e)
  {
    if(!e.Handled && e.KE.Key == Key.F4 && e.KE.HasOnly(KeyMod.Ctrl)) // close the window when Ctrl-F4 is pressed
    {
      Close();
      e.Handled = true;
    }
    base.OnKeyPress(e);
  }

  MyTitleBar titleBar;
}
#endregion

#region MessageBox
public enum MessageBoxButtons
{
  Ok, Cancel, OkCancel, YesNo, YesNoCancel
}

public sealed class MessageBox : Form
{
  internal MessageBox(string text, string[] buttons, int defaultButton)
  {
    if(defaultButton < 0 || defaultButton >= buttons.Length)
    {
      throw new ArgumentOutOfRangeException("defaultButton", defaultButton, "out of the range of 'buttons'");
    }

    DialogResult       = -1;
    message            = text;
    buttonTexts        = buttons;
    this.defaultButton = defaultButton;
    TitleBar.CloseBox  = false;
  }

  public int Show(Desktop desktop)
  {
    return Show(desktop, true);
  }

  public int Show(Desktop desktop, bool modal)
  {
    Parent = desktop;
    if(!init)
    {
      if(EffectiveFont != null)
      {
        int sidePadding = EffectiveFont.LineHeight * 2;

        Size desktopSize = desktop.ContentRect.Size;
        desktopSize.Width -= sidePadding; // we don't want to use the whole space...

        int btnWidth = 0, btnHeight = EffectiveFont.LineHeight * 3 / 2 + 2, btnSpace = 12;

        int[] btnWidths = new int[buttonTexts.Length];
        for(int i = 0; i < buttonTexts.Length; i++)
        {
          btnWidths[i] = EffectiveFont.CalculateSize(buttonTexts[i]).Width;
          btnWidth += Math.Max(btnWidths[i] * 3 / 2, 40); // add padding
        }

        #region Find button width
        // find a good width, try to degrade nicely if we can't get the width we want
        {
          int width = btnWidth + sidePadding + (btnWidths.Length - 1) * btnSpace;

          if(width <= desktopSize.Width) // it's good, accept it
          {
            for(int i = 0; i < btnWidths.Length; i++) btnWidths[i] = Math.Max(btnWidths[i] * 3 / 2, 40);
            goto foundSize;
          }
          // it doesn't fit, so try to reduce the spacing
          width = btnWidth + sidePadding + (btnWidths.Length - 1) * (btnSpace / 2);
          if(width <= desktopSize.Width)
          {
            for(int i = 0; i < btnWidths.Length; i++) btnWidths[i] = btnWidths[i] * 3 / 2;
            btnSpace /= 2;
            goto foundSize;
          }

          // okay, we need to try harder. reduce the internal space
          btnWidth = 0;
          for(int i = 0; i < btnWidths.Length; i++) btnWidth += btnWidths[i] * 5 / 4;
          width = btnWidth + sidePadding + (btnWidths.Length - 1) * btnSpace;
          if(width <= desktopSize.Width)
          {
            for(int i = 0; i < btnWidths.Length; i++) btnWidth += btnWidths[i] * 5 / 4;
            goto foundSize;
          }

          // okay, use only minimal padding and space. this is our final attempt
          btnWidth = 0;
          for(int i = 0; i < btnWidths.Length; i++) btnWidth += (btnWidths[i] += 4); // only 2 pixels of padding
          btnSpace = 4;
          width = btnWidth + sidePadding + (btnWidths.Length - 1) * btnSpace;

          foundSize:
          btnWidth = width;
        }
        #endregion

        Label label = new Label(message);
        label.Width  = Math.Max(btnWidth, Math.Min(desktopSize.Width * 3 / 4 - sidePadding,
                                EffectiveFont.CalculateSize(message).Width));
        label.Height = EffectiveFont.WordWrap(message, label.Width, int.MaxValue).Length * EffectiveFont.LineHeight;
        label.Location = new Point(sidePadding / 2, sidePadding / 2);
        label.TextAlign = ContentAlignment.TopCenter;

        Size = ContentOffset.Grow(new Size(label.Width + sidePadding,
                                           label.Height + btnHeight + sidePadding + EffectiveFont.LineHeight));
        Location = new Point(Math.Max((desktopSize.Width - Width)   / 2, 0),
                             Math.Max((desktopSize.Height - Height) / 2, 0));

        int x = (Width - btnWidth + sidePadding) / 2, y = label.Bottom + EffectiveFont.LineHeight;
        for(int i = 0; i < btnWidths.Length; i++)
        {
          Button btn = new Button(buttonTexts[i]);
          btn.Bounds = new Rectangle(x, y, btnWidths[i], btnHeight);
          btn.Click += btn_OnClick;
          btn.Tag = btn.TabIndex = i;
          Controls.Add(btn);
          if(i == defaultButton) btn.Focus();
          x += btn.Width + btnSpace;
        }
        Controls.Add(label);
        init = true;
      }
      else
      {
        int i = 0;
        foreach(Control control in Controls)
        {
          if(control is Button && i++ == defaultButton)
          {
            control.Focus();
            break;
          }
        }
      }
    }

    if(modal)
    {
      return (int)ShowDialog(desktop);
    }
    else
    {
      Visible = true;
      Focus();
      return -1;
    }
  }

  protected internal override void OnKeyPress(KeyEventArgs e)
  {
    if(!e.Handled)
    {
      for(int i=0; i<Controls.Count; i++)
      {
        ButtonBase button = Controls[i] as ButtonBase;
        if(button != null && button.Text.Length != 0)
        {
          if(char.ToUpper(button.Text[0]) == char.ToUpper(e.KE.Char))
          {
            bool foundAnother = false;
            for(int j=i+1; j < Controls.Count; j++)
            {
              ButtonBase otherButton = Controls[j] as ButtonBase;
              if(otherButton != null && otherButton.Text.Length != 0 &&
                 char.ToUpper(otherButton.Text[0]) == char.ToUpper(e.KE.Char))
              {
                foundAnother = true;
                break;
              }
            }

            if(!foundAnother)
            {
              button.PerformClick();
              e.Handled = true;
            }
            break;
          }
        }
      }
    }

    base.OnKeyPress(e);
  }

  private void btn_OnClick(object sender, ClickEventArgs e)
  {
    DialogResult = ((Control)sender).Tag;
    Close();
  }

  string[] buttonTexts;
  string message;
  int defaultButton;
  bool init;

  public static MessageBox Create(string caption, string text)
  {
    return Create(caption, text, MessageBoxButtons.Ok);
  }
  
  public static MessageBox Create(string caption, string text, MessageBoxButtons buttons)
  {
    return Create(caption, text, buttons, 0);
  }
  
  public static MessageBox Create(string caption, string text, MessageBoxButtons buttons, int defaultButton)
  {
    string[] buttonTexts;
    switch(buttons)
    {
      case MessageBoxButtons.Ok: buttonTexts = okButtons; break;
      case MessageBoxButtons.Cancel: buttonTexts = cancelButtons; break;
      case MessageBoxButtons.OkCancel: buttonTexts = okCancelButtons; break;
      case MessageBoxButtons.YesNo: buttonTexts = yesNoButtons; break;
      case MessageBoxButtons.YesNoCancel: buttonTexts = yesNoCancelButtons; break;
      default: throw new ArgumentException("Unknown MessageBoxButtons value");
    }

    return Create(caption, text, buttonTexts, defaultButton);
  }
  
  public static MessageBox Create(string caption, string text, string[] buttonText)
  {
    return Create(caption, text, buttonText, 0);
  }
  
  public static MessageBox Create(string caption, string text, string[] buttonText, int defaultButton)
  {
    if(buttonText.Length == 0) throw new ArgumentException("Can't create a MessageBox with no buttons!", "buttonText");
    MessageBox box = new MessageBox(text, buttonText, defaultButton);
    box.Text = caption;
    return box;
  }

  public static void Show(Desktop desktop, string caption, string text)
  {
    Create(caption, text).Show(desktop);
  }
  
  public static int Show(Desktop desktop, string caption, string text, MessageBoxButtons buttons)
  {
    return Create(caption, text, buttons, 0).Show(desktop);
  }
  
  public static int Show(Desktop desktop, string caption, string text,
                         MessageBoxButtons buttons, int defaultButton)
  {
    return Create(caption, text, buttons, defaultButton).Show(desktop);
  }
  
  public static int Show(Desktop desktop, string caption, string text, string[] buttonText)
  {
    return Create(caption, text, buttonText).Show(desktop);
  }
  
  public static int Show(Desktop desktop, string caption, string text, string[] buttonText, int defaultButton)
  {
    return Create(caption, text, buttonText, defaultButton).Show(desktop);
  }

  static readonly string[] okButtons = new string[] { "Ok" };
  static readonly string[] cancelButtons = new string[] { "Cancel" };
  static readonly string[] okCancelButtons = new string[] { "Ok", "Cancel" };
  static readonly string[] yesNoButtons = new string[] { "Yes", "No" };
  static readonly string[] yesNoCancelButtons = new string[] { "Yes", "No", "Cancel" };
}
#endregion

} // namespace GameLib.Forms