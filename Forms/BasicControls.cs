﻿/*
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
using Clipboard = System.Windows.Forms.Clipboard;
using Font = GameLib.Fonts.Font;
using Timer = System.Windows.Forms.Timer;

namespace GameLib.Forms
{

#region ScrollableControl
public abstract class ScrollableControl : Control
{
  protected ScrollableControl() { }

  protected ScrollableControl(bool horzizontal, bool vertical)
  {
    ShowHorizontalScrollBar = horzizontal;
    ShowVerticalScrollBar   = vertical;
  }

  public override RectOffset ContentOffset
  {
    get
    {
      RectOffset ret = base.ContentOffset;
      if(horz != null) ret.Bottom += horz.Height;
      if(vert != null) ret.Right  += vert.Width;
      return ret;
    }
  }

  protected ScrollBar HorizontalScrollBar
  {
    get { return horz; }
  }

  protected ScrollBar VerticalScrollBar
  {
    get { return vert; }
  }

  protected bool ShowHorizontalScrollBar
  {
    get { return horz != null; }
    set
    {
      if(value != (horz != null))
      {
        if(horz == null)
        {
          horz = MakeScrollBar(Orientation.Horizontal);
          horz.ValueChanged += OnHorzizontalScroll;
          Controls.Add(horz);
        }
        else
        {
          Controls.Remove(horz);
          horz = null;
        }
        OnContentSizeChanged();
      }
    }
  }

  protected bool ShowVerticalScrollBar
  {
    get { return vert != null; }
    set
    {
      if(value != (vert != null))
      {
        if(vert == null)
        {
          vert = MakeScrollBar(Orientation.Vertical);
          vert.ValueChanged += OnVerticalScroll;
          Controls.Add(vert);
        }
        else
        {
          Controls.Remove(vert);
          vert = null;
        }
        OnContentSizeChanged();
      }
    }
  }

  protected override void LayOutChildren()
  {
    base.LayOutChildren();

    Rectangle rect = Rectangle.Inflate(ControlRect, -BorderWidth, -BorderWidth);
    if(horz != null)
    {
      horz.SetBounds(new Rectangle(rect.X, rect.Bottom - horz.Height, rect.Width - (vert == null ? 0 : vert.Height),
                                   horz.Height), true);
    }
    if(vert != null)
    {
      vert.SetBounds(new Rectangle(rect.Right - vert.Width, rect.X, vert.Width,
                                   rect.Height - (horz == null ? 0 : horz.Width)), true);
    }
  }

  protected virtual ScrollBar MakeScrollBar(Orientation orientation)
  {
    return new ScrollBar(orientation);
  }

  protected virtual void OnHorzizontalScroll(object bar, ValueChangedEventArgs e) { }

  protected virtual void OnVerticalScroll(object bar, ValueChangedEventArgs e) { }

  ScrollBar horz, vert;
}
#endregion

#region Line
public class Line : Control
{
  public Line()
  {
    Size = new Size(1, 1); 
  }

  public Line(Color color) 
  {
    Size      = new Size(1, 1); 
    ForeColor = color; 
  }

  public bool Antialiased
  {
    get { return antialiased; }
    set 
    {
      if(antialiased != value) 
      { 
        antialiased = value;
        Invalidate();
      } 
    }
  }

  public bool TopToBottom
  {
    get { return topToBottom; }
    set 
    {
      if(topToBottom != value) 
      {
        topToBottom = value;
        Invalidate(); 
      } 
    }
  }

  protected override void OnPaint(PaintEventArgs e)
  {
    base.OnPaint(e);

    Color color = GetEffectiveForeColor();
    if(color.A != 0)
    {
      Rectangle drawRect = GetDrawRect();
      Point p1 = topToBottom ? drawRect.Location : new Point(drawRect.X, drawRect.Bottom - 1);
      Point p2 = topToBottom ? new Point(drawRect.Right - 1, drawRect.Bottom - 1)
                             : new Point(drawRect.Right - 1, drawRect.Y);
      e.Renderer.DrawLine(e.Target, p1, p2, color, antialiased);
    }
  }

  bool topToBottom = true, antialiased;
}
#endregion

#region Label
public class Label : Control
{
  public Label() { }
  public Label(string text) { Text = text; }

  protected override void OnPaint(PaintEventArgs e)
  {
    base.OnPaint(e);

    if(Text.Length != 0)
    {
      if(EffectiveFont != null)
      {
        Rectangle rect = GetContentDrawRect();
        EffectiveFont.Color     = EffectivelyEnabled ? GetEffectiveForeColor() : SystemColors.GrayText;
        EffectiveFont.BackColor = GetEffectiveBackColor();
        e.Target.DrawText(EffectiveFont, Text, rect, TextAlign);
      }
    }
  }

  public ContentAlignment TextAlign
  {
    get { return textAlign; }
    set
    {
      if(textAlign != value)
      {
        ValueChangedEventArgs e = new ValueChangedEventArgs(textAlign);
        textAlign = value;
        OnTextAlignChanged(e);
      }
    }
  }

  protected override void OnTextChanged(ValueChangedEventArgs e)
  {
    base.OnTextChanged(e);
    Invalidate();
  }

  protected virtual void OnTextAlignChanged(ValueChangedEventArgs e)
  {
    Invalidate(); 
  }

  ContentAlignment textAlign = ContentAlignment.TopLeft;
}
#endregion

#region ButtonBase
public abstract class ButtonBase : Label
{
  protected ButtonBase()
  {
    ControlStyle |= ControlStyle.Clickable | ControlStyle.CanReceiveFocus;
    AutoPress     = true;
  }

  public event EventHandler<ClickEventArgs> Click;

  public bool AutoPress
  {
    get;
    set;
  }

  public bool Pressed
  {
    get { return pressed; }
    set
    {
      if(value != pressed)
      {
        pressed = value;
        if(mouseOver || !RequireMouseOver) OnPressedChanged();
      }
    }
  }

  public bool RequireMouseOver
  {
    get { return requireMouseOver; }
    set
    {
      if(value != RequireMouseOver)
      {
        requireMouseOver = value;
        if(Pressed && !IsMouseOver) OnPressedChanged();
      }
    }
  }

  public void PerformClick()
  {
    ClickEventArgs e = new ClickEventArgs();
    e.CE.Button = MouseButton.Left;
    e.CE.Down   = true;
    e.CE.Point  = new Point(Width / 2, Height / 2);
    OnClick(e);
  }

  protected bool IsDepressed
  {
    get { return Pressed && (mouseOver || !RequireMouseOver); }
  }

  protected bool IsMouseOver
  {
    get { return mouseOver; }
    private set
    {
      if(value != IsMouseOver)
      {
        mouseOver = value;
        if(Pressed && RequireMouseOver) OnContentSizeChanged();
      }
    }
  }

  protected internal override void OnMouseDown(ClickEventArgs e)
  {
    if(e.CE.Button == MouseButton.Left && !e.Handled)
    {
      Capture = true;
      if(AutoPress) Pressed = true;
      e.Handled = true;
    }

    base.OnMouseDown(e);
  }

  protected internal override void OnMouseUp(ClickEventArgs e)
  {
    if(e.CE.Button == MouseButton.Left && !e.Handled)
    {
      Capture = false;
      if(AutoPress) Pressed = false;
      e.Handled = true;
    }

    base.OnMouseUp(e);
  }

  protected internal override void OnMouseClick(ClickEventArgs e)
  {
    if(e.CE.Button == MouseButton.Left && !e.Handled)
    {
      if(ControlRect.Contains(e.CE.Point)) OnClick(e);
      e.Handled = true;
    }

    base.OnMouseClick(e);
  }

  protected internal override void OnMouseEnter()
  {
    IsMouseOver = true;
    base.OnMouseEnter();
  }

  protected internal override void OnMouseLeave()
  {
    IsMouseOver = false;
    base.OnMouseLeave();
  }

  protected internal override void OnKeyDown(KeyEventArgs e)
  {
    if((e.KE.Key == Key.Enter || e.KE.Key == Key.Space || e.KE.Key == Key.KpEnter) && !e.Handled)
    {
      PerformClick();
    }
  }

  protected virtual void OnClick(ClickEventArgs e)
  {
    if(Click != null) Click(this, e);
  }

  protected virtual void OnPressedChanged()
  {
    Invalidate();
  }

  bool mouseOver, pressed, requireMouseOver;
}
#endregion

#region Button
public class Button : ButtonBase
{
  public Button()
  {
    BorderStyle = BorderStyle.FixedThick;
    TextAlign   = ContentAlignment.MiddleCenter;
  }

  public Button(string text) : this()
  {
    Text = text;
  }

  public override RectOffset ContentOffset
  {
    get
    {
      RectOffset offset = base.ContentOffset;
      if(IsDepressed)
      {
        offset.Left++;
        offset.Top++;
      }
      return offset;
    }
  }

  protected override void OnGotFocus()
  {
    Invalidate();
    base.OnGotFocus();
  }

  protected override void OnLostFocus()
  {
    Invalidate();
    base.OnLostFocus();
  }

  protected override void OnPressedChanged()
  {
    base.OnPressedChanged();
    OnContentSizeChanged();
  }

  protected override void PaintBorder(PaintEventArgs e)
  {
    e.Renderer.DrawBorder(this, e, BorderStyle | (IsDepressed ? BorderStyle.Depressed : 0),
                          Focused ? GetEffectiveForeColor() : GetEffectiveBorderColor());
  }
}
#endregion

#region CheckBox
public class CheckBox : ButtonBase
{
  public CheckBox()
  {
    BorderStyle = BorderStyle.Fixed3D;
    TextAlign   = ContentAlignment.MiddleLeft;
  }

  public CheckBox(bool isChecked) : this()
  {
    Checked = isChecked; 
  }
  
  public CheckBox(string text) : this()
  {
    Text = text; 
  }
  
  public CheckBox(string text, bool isChecked) : this() 
  {
    Text    = text; 
    Checked = isChecked; 
  }

  public event EventHandler CheckedChanged;

  public bool Checked
  {
    get { return isChecked; }
    set
    {
      if(value != this.isChecked)
      {
        this.isChecked = value;
        OnCheckedChanged();
      }
    }
  }

  public override RectOffset ContentOffset
  {
    get
    {
      RectOffset offset = base.ContentOffset;
      if(renderer != null)
      {
        Size size = renderer.GetCheckBoxSize(this);
        offset.Left += size.Width + size.Width/4; // add some space between the checkbox and the content
      }
      return offset;
    }
  }

  protected virtual void OnCheckedChanged()
  {
    if(CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
    Invalidate(ContentRect);
  }

  protected override void OnClick(ClickEventArgs e)
  {
    if(e.CE.Button == MouseButton.Left && !e.Handled)
    {
      Checked = !isChecked;
      e.Handled = true;
    }

    base.OnClick(e);
  }

  protected override void OnGotFocus()
  { 
    Invalidate(); 
    base.OnGotFocus(); 
  }
  
  protected override void OnLostFocus() 
  { 
    Invalidate();
    base.OnLostFocus(); 
  }

  protected override void OnPaint(PaintEventArgs e)
  {
    base.OnPaint(e);

    Size checkboxSize = e.Renderer.GetCheckBoxSize(this);
    int checkboxWidth = checkboxSize.Width + checkboxSize.Width/4;

    Rectangle contentRect = GetContentDrawRect();
    contentRect.X     -= checkboxWidth;
    contentRect.Width += checkboxWidth;

    Point drawPoint = new Point(contentRect.X, contentRect.Y + (contentRect.Height - checkboxSize.Height)/2);
    e.Renderer.DrawCheckBox(this, e, drawPoint, Checked, IsDepressed, EffectivelyEnabled);
  }

  protected override void PaintBorder(PaintEventArgs e)
  {
    if(Focused) base.PaintBorder(e);
  }

  bool isChecked;
}
#endregion

#region ScrollBar
// TODO: make sure Minimum can be greater than Maximum
public class ScrollBar : Control, IDisposable
{
  public ScrollBar()
  {
    AutoUpdate       = true;
    BackColor        = SystemColors.ControlDark;
    ForeColor        = SystemColors.Control;
    ControlStyle     = ControlStyle.Clickable | ControlStyle.Draggable | ControlStyle.CanReceiveFocus;
    DragThreshold    = 4;
    LargeIncrement   = 10;
    Size             = Size.Empty;
    SmallIncrement   = 1;
    ThumbSize        = 0.05f;
  }

  public ScrollBar(Orientation orientation) : this()
  {
    Orientation = orientation;
  }

  static ScrollBar()
  {
    ClickRepeatDelay = 300;
  }

  ~ScrollBar() 
  {
    Dispose(true); 
  }
  
  public void Dispose()
  {
    Dispose(false); 
    GC.SuppressFinalize(this); 
  }

  public class ThumbEventArgs : EventArgs
  {
    public int Start;
    public int End;
  }

  protected enum ClickPlace { None, Down, PageDown, Thumb, PageUp, Up };

  /// <summary>Gets or sets whether the scrollbar control automatically updates the <see cref="Value"/> property upon
  /// receiving user input. The default is true, but this can be set to false if you prefer to handle user input
  /// yourself.
  /// </summary>
  public bool AutoUpdate
  {
    get; set;
  }

  /// <summary>Gets whether the user is currently dragging the thumb.</summary>
  public bool IsDraggingThumb
  {
    get { return dragOffset != -1; } 
  }

  /// <summary>Gets or sets the amount added to or subtracted from <see cref="Value"/> when the scrollbar is adjusted
  /// by a large increment, for instance by an entire page. The default is 10.
  /// </summary>
  public int LargeIncrement
  {
    get;
    set;
  }

  /// <summary>Gets or sets the maximum value to which the <see cref="Value"/> property can be set.</summary>
  public int Maximum
  {
    get { return max; }
    set
    { 
      if(value != max)
      {
        max = value;
        if(Value > max) Value = max;
        Invalidate(); 
      }
    }
  }

  /// <summary>Gets or sets the minimum value to which the <see cref="Value"/> property can be set.</summary>
  public int Minimum
  {
    get { return min; }
    set 
    {
      if(value != min) 
      {
        min = value;
        if(Value < min) Value = min;
        Invalidate();
      } 
    }
  }

  /// <summary>Gets or sets the orientation of the scrollbar. The default is <see cref="Orientation.Vertical"/>.</summary>
  public Orientation Orientation
  {
    get { return orientation; }
    set
    {
      if(value != Orientation)
      {
        orientation = value;
        Invalidate();
      }
    }
  }

  /// <summary>Gets or sets the amount added to or subtracted from <see cref="Value"/> when the scrollbar is adjusted
  /// by a small increment, for instance by a single line. The default is 1.
  /// </summary>
  public int SmallIncrement
  {
    get; set;
  }

  /// <summary>Gets or sets the size of the thumb as a fraction of the available space. This is typically set to the
  /// length of the document visible on one page divided by the entire length of the document.
  /// </summary>
  public float ThumbSize
  {
    get { return thumbSize; }
    set
    {
      if(thumbSize != value)
      {
        if(value < 0 || value > 1) throw new ArgumentOutOfRangeException();
        thumbSize = value;
        Invalidate();
      } 
    }
  }

  /// <summary>Gets or sets the current position of the scrollbar, represented as a value from <see cref="Minimum"/>
  /// to <see cref="Maximum"/>.
  /// </summary>
  public int Value
  {
    get { return value; }
    set
    {
      if(value < min) value = min;
      else if(value > max) value = max;

      if(value != this.value)
      {
        valChange.OldValue = this.value;
        this.value = value;
        OnValueChanged(valChange);
      }
    }
  }

  /// <summary>Gets or sets the number of milliseconds that the mouse button must remain depressed before the click
  /// effect will be repeated. If set to zero, mouse clicks will not be automatically repeated.
  /// </summary>
  public static int ClickRepeatDelay
  {
    get { return crDelay; }
    set
    {
      if(value != ClickRepeatDelay)
      {
        if(value < 0) throw new ArgumentOutOfRangeException();
        crDelay = value;

        if(value == 0 && crTimer != null)
        {
          crTimer.Stop();
          crTimer.Dispose();
          crTimer = null;
          repeatEvent = null;
          isRepeating = false;
        }
        else if(value != 0 && crTimer == null)
        {
          crTimer = new Timer();
          crTimer.Tick += RepeatClick;
        }
      }
    }
  }

  /// <summary>Gets or sets the delay between repeated clicks when the mouse button is clicked and held on the
  /// scrollbar.
  /// </summary>
  public static int ClickRepeatRate
  {
    get { return crRate; }
    set
    {
      if(value != ClickRepeatRate)
      {
        if(value < 0) throw new ArgumentOutOfRangeException();
        crRate = value;
        if(crTimer != null && isRepeating) crTimer.Interval = crRate;
      }
    }
  }

  #region Events
  public event EventHandler Down, Up, PageDown, PageUp;
  public event EventHandler<ThumbEventArgs> ThumbDragStart, ThumbDragMove, ThumbDragEnd;
  public event ValueChangedEventHandler ValueChanged;

  protected internal override void OnMouseDown(ClickEventArgs e)
  {
    if(!e.Handled && e.CE.Button == MouseButton.Left)
    {
      if(crTimer != null)
      {
        ClickPlace p = FindPlace(e.CE.Point);
        OnMouseDown(p);
        if(p != ClickPlace.Thumb)
        {
          repeatEvent = new ClickRepeat(this, p);
          crTimer.Interval = crDelay;
          crTimer.Start();
        }
      }

      if(!isRepeating)
      {
        switch(FindPlace(e.CE.Point))
        {
          case ClickPlace.Down: OnDown(); break;
          case ClickPlace.PageDown: OnPageDown(); break;
          case ClickPlace.PageUp: OnPageUp(); break;
          case ClickPlace.Up: OnUp(); break;
        }
      }

      e.Handled = true;
    }
  }

  protected internal override void OnMouseUp(ClickEventArgs e)
  {
    if(e.CE.Button == MouseButton.Left)
    {
      if(repeatEvent != null)
      {
        crTimer.Stop();
        repeatEvent = null;
        isRepeating = false;
      }
      OnMouseUp();
    }
  }

  protected internal override void OnMouseLeave()
  {
    // if the mouse leaves while repeating or simply depressed, consider it unpressed
    if(repeatEvent != null)
    {
      crTimer.Stop();
      repeatEvent = null;
      isRepeating = false;
      OnMouseUp();
    }
    else if(Mouse.Pressed(MouseButton.Left)) OnMouseUp();
  }

  protected internal override void OnDragStart(DragEventArgs e)
  {
    if(!isRepeating && e.Pressed(MouseButton.Left) && FindPlace(e.Start) == ClickPlace.Thumb)
    {
      dragOffset = (Orientation == Orientation.Horizontal ? e.Start.X : e.Start.Y) - ValueToThumb(value);
      thumbArgs.Start = value;
      OnThumbDragStart(thumbArgs);
    }
    else e.Cancel = true;

    base.OnDragStart(e);
  }

  protected internal override void OnDragMove(DragEventArgs e)
  {
    if(IsDraggingThumb)
    {
      thumbArgs.End = ThumbToValue((Orientation == Orientation.Horizontal ? e.End.X : e.End.Y) - dragOffset);
      OnThumbDragMove(thumbArgs);
    }

    base.OnDragMove(e);
  }

  protected internal override void OnDragEnd(DragEventArgs e)
  {
    if(IsDraggingThumb)
    {
      thumbArgs.End = ThumbToValue((Orientation == Orientation.Horizontal ? e.End.X : e.End.Y) - dragOffset);
      dragOffset = -1;
      OnThumbDragEnd(thumbArgs);
    }

    base.OnDragEnd(e);
  }

  protected override void OnContentSizeChanged()
  {
    base.OnContentSizeChanged();
    endSize = renderer == null ? 0 : renderer.GetScrollBarEndSize(this);
    if(Width == 0 || Height == 0) Size = new Size(Width == 0 ? endSize : Width, Height == 0 ? endSize : Height);
  }

  protected internal override void OnCustomEvent(Events.ControlEvent e)
  {
    ClickRepeat cr = e as ClickRepeat;
    if(cr != null)
    {
      switch(cr.Place)
      {
        case ClickPlace.Down: OnDown(); break;
        case ClickPlace.PageDown: OnPageDown(); break;
        case ClickPlace.PageUp: OnPageUp(); break;
        case ClickPlace.Up: OnUp(); break;
        default: base.OnCustomEvent(e); break;
      }
    }
  }

  protected internal override void OnKeyDown(KeyEventArgs e)
  {
    if(!e.Handled)
    {
      switch(e.KE.Key)
      {
        case Key.PageDown: OnPageUp(); break;
        case Key.PageUp: OnPageDown(); break;
        case Key.Down: case Key.Right: OnUp(); break;
        case Key.Up: case Key.Left: OnDown(); break;
      }
      e.Handled = true;
    }

    base.OnKeyDown(e);
  }

  protected override void OnPaint(PaintEventArgs e)
  {
    base.OnPaint(e);

    Rectangle rect = GetContentDrawRect();
    int thumb = ValueToThumb(Value);
    if(Orientation == Orientation.Horizontal)
    {
      int x = rect.X, w = rect.Width;
      rect.Width = EndSize;
      DrawEnd(e, rect, ClickPlace.Down);
      rect.X = x + w - EndSize;
      DrawEnd(e, rect, ClickPlace.Up);
      rect.X     = x + thumb;
      rect.Width = ThumbSizeInPixels;
    }
    else
    {
      int y = rect.Y, h = rect.Height;
      rect.Height = EndSize;
      DrawEnd(e, rect, ClickPlace.Down);
      rect.Y = y + h - EndSize;
      DrawEnd(e, rect, ClickPlace.Up);
      rect.Y      = y + thumb;
      rect.Height = ThumbSizeInPixels;
    }

    // draw the thumb
    Color color = GetEffectiveForeColor();
    e.Target.FillArea(rect, color);
    e.Renderer.DrawBorder(e.Target, rect, BorderStyle.FixedThick, color);
  }

  protected virtual void OnValueChanged(ValueChangedEventArgs e)
  {
    if(ValueChanged != null) ValueChanged(this, e);
    Refresh();
  }

  protected virtual void OnDown()
  {
    if(AutoUpdate) Value = value - SmallIncrement;
    if(Down != null) Down(this, EventArgs.Empty);
  }

  protected virtual void OnUp()
  {
    if(AutoUpdate) Value = value + SmallIncrement;
    if(Up != null) Up(this, EventArgs.Empty);
  }
  
  protected virtual void OnPageDown()
  {
    if(AutoUpdate) Value = value - LargeIncrement;
    if(PageDown != null) PageDown(this, EventArgs.Empty);
  }
  
  protected virtual void OnPageUp()
  {
    if(AutoUpdate) Value = value + LargeIncrement;
    if(PageUp != null) PageUp(this, EventArgs.Empty);
  }
  
  protected virtual void OnThumbDragStart(ThumbEventArgs e)
  {
    if(ThumbDragStart != null) ThumbDragStart(this, e);
  }
  
  protected virtual void OnThumbDragMove(ThumbEventArgs e)
  {
    if(AutoUpdate) Value = e.End;
    if(ThumbDragMove != null) ThumbDragMove(this, e);
  }
  
  protected virtual void OnThumbDragEnd(ThumbEventArgs e)
  {
    if(AutoUpdate) Value = e.End;
    if(ThumbDragEnd != null) ThumbDragEnd(this, e);
  }
  #endregion

  /// <summary>Gets the size of the scroll bar end buttons, in pixels.</summary>
  protected int EndSize
  {
    get { return endSize; }
  }

  /// <summary>Gets the size of the thumb, in pixels.</summary>
  protected int ThumbSizeInPixels
  {
    get
    {
      int space = (Orientation == Orientation.Horizontal ? Width : Height) - EndSize * 2;
      return Math.Max(10, (int)Math.Round(ThumbSize * space)); // make the thumb at least 10 pixels
    }
  }

  protected virtual void Dispose(bool finalizing)
  {
    if(crTimer != null)
    {
      crTimer.Dispose();
      crTimer = null;
    }
  }

  protected ClickPlace FindPlace(Point click)
  {
    int size = Orientation == Orientation.Horizontal ? Width : Height;
    int pos = Orientation == Orientation.Horizontal ? click.X : click.Y, thumb = ValueToThumb(value);
    if(pos < endSize) return ClickPlace.Down;
    else if(pos < thumb) return ClickPlace.PageDown;
    else if(pos < thumb + ThumbSizeInPixels) return ClickPlace.Thumb;
    else if(pos < size - endSize) return ClickPlace.PageUp;
    else return ClickPlace.Up;
  }

  protected int ThumbToValue(int position)
  {
    if(position < 0) position = 0;

    if(Orientation == Orientation.Horizontal)
    {
      if(position > Width) position = Width - 1;
    }
    else if(position > Height) position = Height - 1;
    
    return (position - endSize) * (max - min) / SpaceExcludingThumb + min;
  }

  protected int ValueToThumb(int value) 
  { 
    return (value - min) * SpaceExcludingThumb / (max - min) + endSize; 
  }

  sealed class ClickRepeat : Events.ControlEvent
  {
    public ClickRepeat(Control control, ClickPlace place) : base(control) { Place = place; }
    public ClickPlace Place;
  }

  int SpaceExcludingThumb
  {
    get
    {
      int space = (Orientation == Orientation.Horizontal ? Width : Height) - EndSize * 2;
      int thumbPixels = Math.Max(10, (int)Math.Round(ThumbSize * space)); // make the thumb at least 10 pixels
      return space - thumbPixels;
    }
  }

  void DrawEnd(PaintEventArgs e, Rectangle rect, ClickPlace place)
  {
    bool horizontal = Orientation == Orientation.Horizontal;
    Arrow arrow = place == ClickPlace.Up ? horizontal ? Arrow.Right : Arrow.Down
                                         : horizontal ? Arrow.Left  : Arrow.Up;
    renderer.DrawArrowBox(e.Target, rect, arrow, EndSize / 4, down == place, GetEffectiveForeColor(),
                          EffectivelyEnabled ? Color.Black : SystemColors.GrayText);
  }

  void OnMouseDown(ClickPlace place)
  {
    down = place;
    Invalidate();
  }

  void OnMouseUp()
  {
    down = ClickPlace.None;
    Invalidate();
  }

  ThumbEventArgs thumbArgs = new ThumbEventArgs();
  ValueChangedEventArgs valChange = new ValueChangedEventArgs(null);
  float thumbSize;
  int value, min, max = 100, endSize, dragOffset = -1;
  Orientation orientation = Orientation.Vertical;
  ClickPlace down;

  static void RepeatClick(object sender, EventArgs e)
  {
    if(repeatEvent != null)
    {
      crTimer.Interval = crRate;
      isRepeating = true;
      Events.Events.PushEvent(repeatEvent);
    }
  }

  static Timer crTimer;
  static ClickRepeat repeatEvent;
  static int crDelay, crRate = 50;
  static bool isRepeating;
}
#endregion

#region TextBox
// TODO: support multi-line editing
public class TextBox : Control
{
  public TextBox()
  {
    ControlStyle = ControlStyle.CanReceiveFocus | ControlStyle.Clickable | ControlStyle.Draggable;
    BackColor = SystemColors.Window;
    ForeColor = SystemColors.WindowText;
    BorderStyle = BorderStyle.FixedThick | BorderStyle.Depressed;
    BorderColor = SystemColors.WindowText;
    Padding     = new RectOffset(2, 0);
  }

  static TextBox() { CaretFlashRate = 600; }

  public int CaretPosition
  {
    get { return caretPosition; }
    set
    {
      if(value != caretPosition)
      {
        if(value < 0 || value > Text.Length) throw new ArgumentOutOfRangeException("CaretPosition");
        int oldlen = selectLen;
        ValueChangedEventArgs e = new ValueChangedEventArgs(caretPosition);
        caretPosition = value;
        if(selectLen > 0)
        {
          if(caretPosition + selectLen > Text.Length) selectLen = Text.Length - caretPosition;
        }
        else if(caretPosition - selectLen < 0) selectLen = -caretPosition;
        caretOn = true;
        if(oldlen != 0 && (Selected || !hideSelection)) Invalidate(ContentRect);
        OnCaretPositionChanged(e);
      }
    }
  }

  public bool HideSelection
  {
    get { return hideSelection; }
    set
    {
      hideSelection = value;
      if(value && !Selected) Invalidate(ContentRect);
    }
  }

  public string[] Lines
  {
    get { return Text.Split('\n'); }
    set { Text = string.Join("\n", value); }
  }

  public int MaxLength
  {
    get { return maxLength; }
    set
    {
      if(maxLength < -1) throw new ArgumentOutOfRangeException("MaxLength", value, "must be >= -1");
      maxLength = MaxLength;
      if(maxLength > Text.Length) Text = Text.Substring(0, maxLength);
    }
  }

  public bool Modified
  {
    get { return modified; }
    set
    {
      if(value != modified)
      {
        modified = value;
        OnModifiedChanged();
      }
    }
  }

  public bool ReadOnly
  {
    get { return readOnly; }
    set { readOnly = value; } 
  }

  public string SelectedText
  {
    get
    {
      return selectLen < 0 ? Text.Substring(caretPosition + selectLen, -selectLen)
                           : Text.Substring(caretPosition, selectLen);
    }
    set
    {
      if(value == null) throw new ArgumentNullException("SelectedText");
      int start = caretPosition, end = caretPosition;
      if(selectLen < 0) start += selectLen;
      else end += selectLen;
      Select(start, value.Length);
      Text = Text.Substring(0, start) + value + Text.Substring(end, Text.Length - end);
    }
  }

  public int SelectionEnd 
  {
    get { return selectLen < 0 ? caretPosition : caretPosition + selectLen; } 
  }

  public int SelectionLength
  {
    get { return selectLen; }
    set
    {
      if(value != selectLen)
      {
        if(value < -Text.Length || value > Text.Length) throw new ArgumentOutOfRangeException("SelectionLength");
        selectLen = value;
        if(Selected || !hideSelection) Invalidate(ContentRect);
      }
    }
  }

  public int SelectionStart 
  {
    get { return selectLen < 0 ? caretPosition + selectLen : caretPosition; } 
  }

  public bool SelectOnFocus 
  {
    get { return selectOnFocus; }
    set { selectOnFocus = value; }
  }

  public override string Text
  {
    set
    {
      if(caretPosition > value.Length)
      {
        if(selectLen < 0)
        {
          if(-selectLen > value.Length) Select(value.Length, -value.Length);
        }
        else Select(value.Length, 0);
      }
      else if(caretPosition + selectLen > value.Length) Select(caretPosition, value.Length - caretPosition);
      base.Text = value;
    }
  }

  public bool WordWrap
  {
    get { return wordWrap; }
    set { wordWrap = value; }
  }

  #region Events
  public event EventHandler ModifiedChanged;
  
  protected virtual void OnModifiedChanged()
  {
    if(ModifiedChanged != null) ModifiedChanged(this, EventArgs.Empty);
  }
  
  protected virtual void OnCaretPositionChanged(ValueChangedEventArgs e)
  {
    Invalidate(ContentRect);
  }
  
  protected virtual void OnCaretFlash()
  {
    if(Desktop != null)
    {
      Rectangle rect = ContentRect;
      rect.Y      += 2;
      rect.Height -= 4;
      rect.Width   = 1;
      rect.X      += EffectiveFont.CalculateSize(Text.Substring(visibleStart, CaretPosition - visibleStart)).Width;

      Desktop.DoPaint(this, rect, delegate(PaintEventArgs e)
      {
        e.Renderer.DrawLine(e.Target, e.DrawRect.Location, new Point(e.DrawRect.X, e.DrawRect.Bottom-1),
                            CaretOn && HasCaret ? GetEffectiveForeColor() : BackColor, false);
      });
    }
  }

  protected override void OnGotFocus()
  {
    if(selectOnFocus)
    {
      SelectAll();
//      Invalidate(ContentRect); // TODO: does this Invalidate() call need to be here?
    }
    WithCaret = this;
    base.OnGotFocus();
  }

  protected override void OnLostFocus()
  {
    if(hideSelection) Invalidate(ContentRect);
    WithCaret = null;
    base.OnLostFocus();
  }

  protected internal override void OnKeyPress(KeyEventArgs e)
  {
    if(e.KE.Char >= 32 && !readOnly && !e.Handled)
    {
      if(maxLength == -1 || Text.Length < maxLength)
      {
        Modified = true;
        InsertText(e.KE.Char.ToString());
      }
      e.Handled = true;
    }

    base.OnKeyPress(e);
  }

  protected internal override void OnKeyDown(KeyEventArgs e)
  {
    if(!e.Handled)
    {
      char c = e.KE.Char;
      if(e.KE.Key == Key.Left)
      {
        if(e.KE.KeyMods == KeyMod.None || e.KE.HasOnly(KeyMod.Shift | KeyMod.Ctrl))
        {
          if(caretPosition > 0)
          {
            if(e.KE.KeyMods == KeyMod.None) Select(caretPosition - 1, 0);
            else if(e.KE.HasOnly(KeyMod.Shift)) Select(caretPosition - 1, selectLen + 1);
            else
            {
              int pos = CtrlScan(-1);
              if(e.KE.HasOnly(KeyMod.Ctrl)) Select(pos, 0);
              else if(selectLen < 0) Select(pos, SelectionStart - pos);
              else Select(pos, SelectionEnd - pos);
            }
          }
          else if(!e.KE.HasAny(KeyMod.Shift)) SelectionLength = 0;
          e.Handled = true;
        }
      }
      else if(e.KE.Key == Key.Right)
      {
        if(e.KE.KeyMods == KeyMod.None || e.KE.HasOnly(KeyMod.Shift | KeyMod.Ctrl))
        {
          if(caretPosition < Text.Length)
          {
            if(e.KE.KeyMods == KeyMod.None) Select(caretPosition + 1, 0);
            else if(e.KE.HasOnly(KeyMod.Shift)) Select(caretPosition + 1, selectLen - 1);
            else
            {
              int pos = CtrlScan(1);
              if(e.KE.HasOnly(KeyMod.Ctrl)) Select(pos, 0);
              else if(selectLen < 0) Select(pos, SelectionStart - pos);
              else Select(pos, SelectionEnd - pos);
            }
          }
          else if(!e.KE.HasAny(KeyMod.Shift)) SelectionLength = 0;
          e.Handled = true;
        }
      }
      else if(e.KE.Key == Key.Backspace && !readOnly)
      {
        if(e.KE.KeyMods == KeyMod.None)
        {
          if(selectLen != 0) { Modified = true; SelectedText = ""; }
          else if(caretPosition > 0)
          {
            Modified = true;
            Select(caretPosition - 1, 0);
            Text = Text.Substring(0, caretPosition) +
                   Text.Substring(caretPosition + 1, Text.Length - caretPosition - 1);
          }
          e.Handled = true;
        }
        else if(e.KE.HasOnly(KeyMod.Ctrl))
        {
          if(caretPosition > 0 && selectLen == 0)
          {
            int end = caretPosition, pos = CtrlScan(-1);
            Modified = true;
            Select(pos, 0);
            Text = Text.Substring(0, pos) + Text.Substring(end, Text.Length - end);
          }
          e.Handled = true;
        }
      }
      else if(e.KE.Key == Key.Delete && !e.KE.HasAny(KeyMod.Shift) && !readOnly)
      {
        if(e.KE.KeyMods == KeyMod.None || e.KE.HasOnly(KeyMod.Ctrl))
        {
          if(selectLen != 0) { Modified = true; SelectedText = ""; }
          else if(caretPosition < Text.Length)
          {
            Modified = true;
            if(e.KE.KeyMods == KeyMod.None)
              Text = Text.Substring(0, caretPosition) +
                     Text.Substring(caretPosition + 1, Text.Length - caretPosition - 1);
            else
            {
              int pos = CtrlScan(1);
              Text = Text.Substring(0, caretPosition) + Text.Substring(pos, Text.Length - pos);
            }
          }
          e.Handled = true;
        }
      }
      else if(e.KE.Key == Key.Home)
      {
        if(e.KE.KeyMods == KeyMod.None) { Select(0, 0); e.Handled = true; }
        else if(e.KE.HasOnly(KeyMod.Shift))
        {
          if(selectLen < 0) Select(0, SelectionStart);
          else Select(0, SelectionEnd);
          e.Handled = true;
        }
      }
      else if(e.KE.Key == Key.End)
      {
        if(e.KE.KeyMods == KeyMod.None) { Select(Text.Length, 0); e.Handled = true; }
        else if(e.KE.HasOnly(KeyMod.Shift))
        {
          if(selectLen < 0) Select(Text.Length, SelectionStart - Text.Length);
          else Select(Text.Length, SelectionEnd - Text.Length);
          e.Handled = true;
        }
      }
      else if(e.KE.HasOnly(KeyMod.Ctrl) && (c == 'C' - 64 || e.KE.Key == Key.Insert))
      {
        Copy();
        e.Handled = true;
      }
      else if(c == 'X' - 64 && e.KE.HasOnly(KeyMod.Ctrl) || e.KE.Key == Key.Delete && e.KE.HasOnly(KeyMod.Shift))
      {
        if(selectLen != 0)
        {
          Modified = true;
          if(readOnly) Copy();
          else Cut();
        }
        e.Handled = true;
      }
      else if(c == 'V' - 64 && e.KE.HasOnly(KeyMod.Ctrl) || e.KE.Key == Key.Insert && e.KE.HasOnly(KeyMod.Shift) &&
              !readOnly)
      {
        if(maxLength == -1 || Text.Length < maxLength)
        {
          Modified = true;
          if(maxLength == -1) Paste();
          else
          {
            int avail = maxLength - Text.Length;
            string clipboardText = GetClipboardText();
            if(clipboardText != null)
            {
              InsertText(clipboardText.Length > avail ? clipboardText.Substring(0, avail) : clipboardText);
            }
          }
        }
        e.Handled = true;
      }
      else if(c == 'A' - 64 && e.KE.HasOnly(KeyMod.Ctrl))
      {
        SelectAll();
        e.Handled = true;
      }
    }

    base.OnKeyDown(e);
  }

  protected internal override void OnMouseClick(ClickEventArgs e)
  {
    if(e.CE.Button == MouseButton.Left)
    {
      Select(PointToPosition(e.CE.X), 0);
      e.Handled = true;
    }
  }

  protected internal override void OnDragStart(DragEventArgs e)
  {
    if(e.Pressed(0) && !e.Cancel) Select(PointToPosition(e.Start.X), 0);
    else e.Cancel = true;
    base.OnDragStart(e);
  }

  protected internal override void OnDragMove(DragEventArgs e)
  {
    int start = PointToPosition(e.Start.X), end = PointToPosition(e.End.X);
    Select(end, start - end);
    base.OnDragMove(e);
  }

  protected internal override void OnDragEnd(DragEventArgs e)
  {
    int start = PointToPosition(e.Start.X), end = PointToPosition(e.End.X);
    Select(end, start - end);
    base.OnDragEnd(e);
  }

  protected override void OnPaintBackground(PaintEventArgs e)
  {
    base.OnPaintBackground(e);

    if(EffectiveFont != null && Text.Length != 0)
    {
      Rectangle rect = GetContentDrawRect();
      int caret = CaretPosition;
      if(caret < visibleStart) visibleStart = Math.Max(caret - 10, 0);

      string text;
      while(true)
      {
        text = visibleStart == 0 ? Text : Text.Substring(visibleStart);
        visibleEnd = visibleStart + EffectiveFont.HowManyFit(text, rect.Width);
        if(visibleEnd - visibleStart < text.Length && caret >= visibleEnd)
        {
          visibleStart = Math.Min(visibleStart + 10 + caret - visibleEnd, Text.Length);
        }
        else break;
      }

      if((!HideSelection || Focused) && SelectionLength != 0 && EffectivelyEnabled)
      {
        text = Text.Substring(visibleStart, visibleEnd - visibleStart);
        if(SelectionStart > visibleStart)
        {
          rect.X += EffectiveFont.CalculateSize(text.Substring(0, SelectionStart - visibleStart)).Width;
        }

        string selectedText = text.Substring(Math.Max(0, SelectionStart - visibleStart),
                                             Math.Min(Math.Abs(SelectionLength), visibleEnd - visibleStart));
        rect.Width = EffectiveFont.CalculateSize(selectedText).Width;
        e.Target.FillArea(rect, GetEffectiveForeColor());
      }
    }
    else
    {
      visibleStart = visibleEnd = 0;
    }
  }

  protected override void OnPaint(PaintEventArgs e)
  {
    base.OnPaint(e);

    if(EffectiveFont != null && Text.Length != 0)
    {
      Rectangle rect = GetContentDrawRect();
      Point location = rect.Location;
      if(rect.Height > EffectiveFont.Height) location.Y += (rect.Height - EffectiveFont.Height) / 2;

      string text = Text.Substring(visibleStart, visibleEnd - visibleStart);

      Color fore = EffectivelyEnabled ? GetEffectiveForeColor() : SystemColors.GrayText;
      Color back = GetEffectiveBackColor();

      EffectiveFont.Color     = fore;
      EffectiveFont.BackColor = back;

      // draw the text
      if((HideSelection && !Focused) || SelectionLength == 0) // if no text is drawn as selected...
      {
        e.Target.DrawText(EffectiveFont, text, location);
      }
      else // some text is selected...
      {
        if(SelectionStart > visibleStart) // draw the text before the selection
        {
          location.X += e.Target.DrawText(EffectiveFont, text.Substring(0, SelectionStart - visibleStart), location);
        }

        // draw the text in the selection
        string selectedText = text.Substring(Math.Max(0, SelectionStart - visibleStart),
                                             Math.Min(Math.Abs(SelectionLength), visibleEnd - visibleStart));
        EffectiveFont.Color     = back;
        EffectiveFont.BackColor = fore;
        location.X += e.Target.DrawText(EffectiveFont, selectedText, location);
        
        // draw the text after the selection
        if(SelectionEnd < visibleEnd)
        {
          EffectiveFont.Color     = fore;
          EffectiveFont.BackColor = back;
          e.Target.DrawText(EffectiveFont, text.Substring(SelectionEnd - visibleStart, visibleEnd - SelectionEnd),
                            location);
        }
      }

      if(Focused) // if focused, draw the caret
      {
        int x = rect.X + EffectiveFont.CalculateSize(text.Substring(0, CaretPosition - visibleStart)).Width;
        e.Renderer.DrawLine(e.Target, new Point(x, rect.Top + 2), new Point(x, rect.Bottom - 3),
                            CaretOn && HasCaret ? fore : BackColor, false);
      }
    }
  }

  protected internal override void OnCustomEvent(Events.ControlEvent e)
  {
    if(e is CaretFlashEvent) OnCaretFlash();
    else base.OnCustomEvent(e);
  }

  protected override void OnTextChanged(ValueChangedEventArgs e)
  {
    base.OnTextChanged(e);
    if(caretPosition > Text.Length) caretPosition = Text.Length;
    Invalidate(ContentRect);
  }
  #endregion

  public void AppendText(string text) { Text += text; }
  
  // TODO: use the real clipboard. this is easy, but I don't want to link to System.Windows.Forms :-(
  public void Copy()
  {
    if(SelectionLength != 0) SetClipboardText(SelectedText);
    else if(Text.Length != 0) SetClipboardText(Text);
  }

  public void Cut()
  {
    if(SelectionLength != 0)
    {
      SetClipboardText(SelectedText);
      SelectedText = "";
    }
  }

  public void Paste()
  {
    string text = GetClipboardText();
    if(text != null) InsertText(text);
  }

  public void InsertText(string text)
  {
    if(selectLen != 0)
    {
      SelectedText = text;
      Select(caretPosition + selectLen, 0);
    }
    else if(text.Length > 0)
    {
      Text = Text.Substring(0, caretPosition) + text + Text.Substring(caretPosition, Text.Length - caretPosition);
      CaretPosition = Math.Min(Text.Length, CaretPosition + text.Length);
    }
  }

  public void Select(int start, int length)
  {
    CaretPosition = start;
    SelectionLength = length;
  }
  
  public void SelectAll()
  {
    CaretPosition = Text.Length;
    SelectionLength = -Text.Length;
  }

  protected bool HasCaret
  {
    get { return withCaret == this && Focused; }
  }

  protected int PointToPosition(int x)
  {
    Rectangle rect = ContentRect;
    if(x < rect.X) return visibleStart;
    else if(x > rect.Width) return visibleEnd;
    else if(EffectiveFont == null) return 0;
    else
    {
      return visibleStart +
             EffectiveFont.HowManyFit(Text.Substring(visibleStart, visibleEnd - visibleStart), x - rect.X);
    }
  }

  sealed class CaretFlashEvent : Events.ControlEvent
  {
    public CaretFlashEvent(TextBox tb) : base(tb) { }
  }

  int CtrlScan(int dir)
  { 
    string text = Text;
    int i = caretPosition;
    if(dir < 0)
    {
      if(--i < 0) return 0;
    }
    else if(caretPosition == Text.Length) return caretPosition;

    for(; i >= 0 && i < Text.Length; i += dir) // skip whitespace
    {
      if(!char.IsWhiteSpace(text[i])) break;
    }

    for(; i >= 0 && i < Text.Length; i += dir) // skip punctuation
    {
      if(!IsPunctuation(text[i])) break;
    }

    for(; i >= 0 && i < Text.Length; i += dir) // then scan until whitespace or punctuation
    {
      if(IsPunctuation(text[i]) || char.IsWhiteSpace(text[i]))
      {
        if(dir < 0) i++;
        break;
      }
    }

    return i < 0 ? 0 : i;
  }

  int caretPosition, selectLen, maxLength = -1, visibleStart, visibleEnd;
  bool hideSelection = true, modified, wordWrap, selectOnFocus = true, readOnly;

  public static int CaretFlashRate
  {
    get { return flashRate; }
    set
    {
      if(value != flashRate)
      {
        if(value < 0) throw new ArgumentOutOfRangeException();

        if(value == 0)
        {
          Utility.Dispose(ref caretTimer);
        }
        else
        {
          if(caretTimer == null)
          {
            caretTimer = new Timer();
            caretTimer.Tick += CaretFlash;
          }

          if(flashRate == 0 && withCaret != null)
          {
            caretTimer.Interval = value;
            caretTimer.Start();
          }
        }

        flashRate = value;
      }
    }
  }

  static bool CaretOn
  {
    get { return caretOn; }
    set { caretOn = value; }
  }

  static TextBox WithCaret
  {
    get { return withCaret; }
    set
    {
      if(withCaret != value)
      {
        bool hadValue = withCaret != null;
        if(withCaret != null) withCaret.Invalidate(withCaret.ContentRect);
        
        withCaret = value;

        if(withCaret != null)
        {
          caretOn = true;
          if(withCaret.HasCaret)
          {
            DoFlash(withCaret);
            if(!hadValue && CaretFlashRate != 0)
            {
              caretTimer.Interval = CaretFlashRate / 2;
              caretTimer.Start();
            }
          }
        }
        else if(hadValue)
        {
          if(caretTimer != null) caretTimer.Stop();
        }
      }
    }
  }

  static void CaretFlash(object sender, EventArgs e)
  {
    if(caretTimer.Interval != CaretFlashRate) caretTimer.Interval = CaretFlashRate;
    caretOn = !caretOn;
    TextBox tb = withCaret;
    if(tb != null && tb.HasCaret) DoFlash(tb);
  }

  static void DoFlash(TextBox tb)
  {
    Events.Events.PushEvent(new CaretFlashEvent(tb));
  }

  static string GetClipboardText()
  {
    string text = clipboard;

    if(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
    {
      for(int i=0; i<2 && Clipboard.ContainsText(); i++) // sometimes the clipboard is busy, so we'll try multiple times
      {
        try
        {
          text = Clipboard.GetText();
          break;
        }
        catch { Thread.Sleep(0); } // if it fails, sleep for a bit
      }
    }

    return text;
  }

  static void SetClipboardText(string text)
  {
    if(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
    {
      for(int i=0; i<2; i++) // sometimes the clipboard is busy, so we'll try multiple times
      {
        try
        {
          if(string.IsNullOrEmpty(text)) Clipboard.Clear();
          else Clipboard.SetText(text);
        }
        catch { Thread.Sleep(0); } // if it fails, sleep for a bit
      }
    }

    clipboard = text;
  }

  static bool IsPunctuation(char c)
  {
    return char.IsPunctuation(c) || char.IsSymbol(c);
  }

  static string clipboard;
  static Timer caretTimer;
  static TextBox withCaret;
  static int flashRate;
  static bool caretOn;
}
#endregion

} // namespace GameLib.Forms