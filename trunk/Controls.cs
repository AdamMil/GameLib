/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2004 Adam Milazzo

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

// TODO: implement more controls (listbox, dropdown, color picker)
using System;
using System.Collections;
using System.Drawing;
using GameLib.Fonts;
using GameLib.Video;
using GameLib.Input;

namespace GameLib.Forms
{

#region Base control classes
#region ContainerControl
public class ContainerControl : Control
{ protected void DoLayout() { DoLayout(false); }
  protected void DoLayout(bool recursive)
  { Rectangle avail = ContentRect;

    foreach(Control c in Controls)
      if(!c.HasStyle(ControlStyle.DontLayout))
        switch(c.Dock)
        { case DockStyle.Left:
            c.SetBounds(new Rectangle(avail.X, avail.Y, c.Width, avail.Height), true);
            avail.X += c.Width; avail.Width -= c.Width;
            break;
          case DockStyle.Top:
            c.SetBounds(new Rectangle(avail.X, avail.Y, avail.Width, c.Height), true);
            avail.Y += c.Height; avail.Height -= c.Height;
            break;
          case DockStyle.Right:
            c.SetBounds(new Rectangle(avail.Right-c.Width, avail.Y, c.Width, avail.Height), true);
            avail.Width -= c.Width;
            break;
          case DockStyle.Bottom:
            c.SetBounds(new Rectangle(avail.X, avail.Bottom-c.Height, avail.Width, c.Height), true);
            avail.Height -= c.Height;
            break;
        }

    AnchorSpace = avail;
    LayoutEventArgs e = recursive ? new LayoutEventArgs(true) : null;
    foreach(Control c in Controls)
    { if(c.Dock==DockStyle.None && !c.HasStyle(ControlStyle.DontLayout)) c.DoAnchor();
      if(recursive) c.OnLayout(e);
    }
  }

  protected internal override void OnPaint(PaintEventArgs e)
  { foreach(Control c in Controls)
      if(c.Visible)
      { Rectangle paint = Rectangle.Intersect(c.Bounds, e.WindowRect);
        if(paint.Width>0)
        { paint.X -= c.Left; paint.Y -= c.Top;
          c.AddInvalidRect(paint);
          c.Update();
        }
      }
    base.OnPaint(e); // yes, this needs to be at the bottom, not the top! (TODO: why?)
  }

  protected internal override void OnLayout(LayoutEventArgs e)
  { base.OnLayout(e);
    DoLayout(e.Recursive);
  }
}
#endregion

#region ScrollableControl
// TODO: document
public class ScrollableControl : Control
{ protected ScrollableControl() { }
  protected ScrollableControl(bool horzizontal, bool vertical)
  { ShowHorizontalScrollBar = horzizontal;
    ShowVerticalScrollBar   = vertical;
  }

  public override RectOffset ContentOffset
  { get
    { RectOffset ret = base.ContentOffset;
      if(horz!=null) ret.Bottom += horz.Height;
      if(vert!=null) ret.Right  += vert.Width;
      return ret;
    }
  }

  protected ScrollBarBase HorizontalScrollBar { get { return horz; } }
  protected ScrollBarBase VerticalScrollBar { get { return vert; } }

  protected bool ShowHorizontalScrollBar
  { get { return horz!=null; }
    set
    { if(value != (horz!=null))
      { if(horz==null)
        { horz = MakeScrollBar(true);
          horz.ValueChanged += new ValueChangedEventHandler(OnHorzizontalScroll);
          Controls.Add(horz);
        }
        else
        { Controls.Remove(horz);
          horz = null;
        }
        OnContentSizeChanged(new EventArgs());
      }
    }
  }

  protected bool ShowVerticalScrollBar
  { get { return vert!=null; }
    set
    { if(value != (vert!=null))
      { if(vert==null)
        { vert = MakeScrollBar(false);
          vert.ValueChanged += new ValueChangedEventHandler(OnVerticalScroll);
          Controls.Add(vert);
        }
        else
        { Controls.Remove(vert);
          vert = null;
        }
        OnContentSizeChanged(new EventArgs());
      }
    }
  }

  protected internal override void OnLayout(LayoutEventArgs e)
  { Rectangle rect = PaddingRect;
    if(horz!=null)
    { horz.Top   = rect.Bottom - horz.Height;
      horz.Width = rect.Width  - (vert==null ? 0 : vert.Height);
    }
    if(vert!=null)
    { vert.Left   = rect.Right  - vert.Width;
      vert.Height = rect.Height - (horz==null ? 0 : horz.Width);
    }
    base.OnLayout(e);
  }

  protected internal override void OnPaint(PaintEventArgs e)
  { if(horz!=null) PaintBar(horz, e);
    if(vert!=null) PaintBar(vert, e);
    base.OnPaint(e); // yes, this needs to be at the bottom, not the top! (TODO: why?)
  }

  protected virtual ScrollBarBase MakeScrollBar(bool horizontal)
  { ScrollBar bar = new ScrollBar();
    bar.Horizontal = horizontal;
    return bar;
  }
  
  protected virtual void OnHorzizontalScroll(object bar, ValueChangedEventArgs e) { }
  protected virtual void OnVerticalScroll(object bar, ValueChangedEventArgs e) { }

  void PaintBar(ScrollBarBase bar, PaintEventArgs e)
  { Rectangle paint = Rectangle.Intersect(bar.Bounds, e.WindowRect);
    if(paint.Width>0)
    { paint.X -= bar.Left; paint.Y -= bar.Top;
      bar.AddInvalidRect(paint);
      bar.Update();
    }
  }

  ScrollBarBase horz, vert;
}
#endregion
#endregion

#region Basic controls
#region Line
public class Line : Control
{ public Line() { Size=new Size(1,1); }
  public Line(Color color) { Size=new Size(1,1); ForeColor=color; }

  public bool TopToBottom
  { get { return ttb; }
    set { if(ttb!=value) { ttb=value; Invalidate(); } }
  }

  public bool AntiAliased
  { get { return aa; }
    set { if(aa!=value) { aa=value; Invalidate(); } }
  }

  protected internal override void OnPaint(PaintEventArgs e)
  { base.OnPaint(e);
    Color c = ForeColor;
    if(c != Color.Transparent)
    { Point p1 = ttb ? e.DisplayRect.Location : new Point(e.DisplayRect.X, e.DisplayRect.Bottom-1);
      Point p2 = ttb ? new Point(e.DisplayRect.Right-1, e.DisplayRect.Bottom-1)
                     : new Point(e.DisplayRect.Right-1, e.DisplayRect.Y);
      if(aa) Primitives.LineAA(e.Surface, p1, p2, c);
      else Primitives.Line(e.Surface, p1, p2, c);
    }
  }

  protected bool ttb=true, aa;
}
#endregion

#region LabelBase
public abstract class LabelBase : Control
{ public IBlittable Image
  { get { return image; }
    set
    { if(image != value)
      { image = value;
        Invalidate();
      }
    }
  }

  public ContentAlignment ImageAlign
  { get { return imageAlign; }
    set
    { if(imageAlign != value)
      { imageAlign = value;
        Invalidate();
      }
    }
  }

  public ContentAlignment TextAlign
  { get { return textAlign; }
    set
    { if(textAlign != value)
      { ValueChangedEventArgs e = new ValueChangedEventArgs(textAlign);
        textAlign = value;
        OnTextAlignChanged(e);
      }
    }
  }

  protected override void OnTextChanged(ValueChangedEventArgs e)
  { Invalidate();
    base.OnTextChanged(e);
  }

  protected virtual void OnTextAlignChanged(ValueChangedEventArgs e) { Invalidate(); }

  IBlittable image;
  ContentAlignment imageAlign=ContentAlignment.TopLeft, textAlign=ContentAlignment.TopLeft;
}
#endregion

#region Label
public class Label : LabelBase
{ public Label() { }
  public Label(string text) { Text=text; }

  protected internal override void OnPaint(PaintEventArgs e)
  { base.OnPaint(e);
    Rectangle rect = ContentDrawRect;

    if(Image!=null)
    { Point at = Helpers.CalculateAlignment(rect, new Size(Image.Width, Image.Height), ImageAlign);
      Image.Blit(e.Surface, at.X, at.Y);
    }
    if(Text.Length>0)
    { GameLib.Fonts.Font f = Font;
      if(f != null)
      { f.Color     = Enabled ? ForeColor : SystemColors.GrayText;
        f.BackColor = BackColor;
        f.Render(e.Surface, Text, rect, TextAlign);
      }
    }
  }
}
#endregion

#region ButtonBase
public abstract class ButtonBase : LabelBase
{ public ButtonBase()
  { BorderStyle=BorderStyle.FixedThick; Style|=ControlStyle.Clickable|ControlStyle.CanFocus;
    autoPress=true;
  }

  public event ClickEventHandler Click;
  public event EventHandler PressedChanged;

  public bool AutoPress
  { get { return autoPress; }
    set { autoPress = value; }
  }

  public bool Pressed
  { get { return pressed; }
    set
    { if(value!=pressed)
      { pressed=value;
        OnPressedChanged(new EventArgs());
      }
    }
  }

  public void PerformClick() { PerformClick(MouseButton.Left); }
  public void PerformClick(MouseButton button)
  { ClickEventArgs e = new ClickEventArgs();
    e.CE.Button = button;
    e.CE.Down   = true;
    e.CE.Point  = new Point(Width/2, Height/2);
    OnClick(e);
  }

  protected internal override void OnMouseDown(ClickEventArgs e)
  { if(e.CE.Button==MouseButton.Left) { Capture = true; if(autoPress) Pressed = true; }
    base.OnMouseDown(e);
  }

  protected internal override void OnMouseUp(ClickEventArgs e)
  { if(e.CE.Button==MouseButton.Left) { Capture = false; if(autoPress) Pressed = false; }
    base.OnMouseUp(e);
  }

  protected internal override void OnMouseClick(ClickEventArgs e)
  { if(e.CE.Button==MouseButton.Left && !e.Handled)
    { if(WindowRect.Contains(e.CE.Point)) OnClick(e);
      e.Handled = true;
    }
    base.OnMouseClick(e);
  }

  protected internal override void OnKeyDown(KeyEventArgs e)
  { if((e.KE.Key==Key.Enter || e.KE.Key==Key.Space || e.KE.Key==Key.KpEnter) && !e.Handled)
      OnClick(new ClickEventArgs());
  }

  protected virtual void OnClick(ClickEventArgs e) { if(Click!=null) Click(this, e); }
  protected virtual void OnPressedChanged(EventArgs e) { if(PressedChanged!=null) PressedChanged(this, e); }

  bool pressed, autoPress;
}
#endregion

#region Button
public class Button : ButtonBase
{ public Button()
  { ImageAlign=TextAlign=ContentAlignment.MiddleCenter; alwaysUse=false; DontDraw|=DontDraw.Border;
  }
  public Button(string text) : this() { Text=text; }

  public bool AlwaysUsePressed { get { return alwaysUse; } set { alwaysUse=value; } }

  protected bool Over { get { return over; } }

  protected internal override void OnPaintBackground(PaintEventArgs e)
  { base.OnPaintBackground(e);
    Helpers.DrawBorder(e.Surface, DrawRect, BorderStyle, Focused ? ForeColor : BorderColor,
                       Pressed && (over || alwaysUse));
  }

  protected internal override void OnPaint(PaintEventArgs e)
  { base.OnPaint(e);

    Rectangle rect = ContentDrawRect;
    bool pressed = Pressed && (over || alwaysUse);

    if(Image!=null)
    { Point at = Helpers.CalculateAlignment(rect, new Size(Image.Width, Image.Height), ImageAlign);
      if(pressed) at.Offset(1, 1);
      Image.Blit(e.Surface, at.X, at.Y);
    }
    if(Text.Length>0)
    { GameLib.Fonts.Font f = Font;
      if(f!=null)
      { if(pressed) rect.Offset(1, 1);
        f.Color     = Enabled ? ForeColor : SystemColors.GrayText;
        f.BackColor = BackColor;
        f.Render(e.Surface, Text, rect, TextAlign);
      }
    }
  }

  protected internal override void OnMouseEnter(EventArgs e)
  { over=true;
    if(Pressed) Invalidate();
    base.OnMouseEnter(e);
  }
  protected internal override void OnMouseLeave(EventArgs e)
  { over=false;
    if(Pressed) Invalidate();
    base.OnMouseLeave(e);
  }

  protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
  protected override void OnGotFocus(EventArgs e)  { Invalidate(); base.OnGotFocus(e); }
  protected override void OnPressedChanged(EventArgs e) { Invalidate(); base.OnPressedChanged(e); }

  bool over, alwaysUse;
}
#endregion

#region CheckBoxBase
public abstract class CheckBoxBase : ButtonBase
{ public CheckBoxBase() { TextAlign=ContentAlignment.MiddleRight; }

  public bool Checked
  { get { return value; }
    set
    { if(value!=this.value)
      { this.value=value;
        OnCheckedChanged(new EventArgs());
      }
    }
  }

  public event EventHandler CheckedChanged;

  protected virtual void OnCheckedChanged(EventArgs e)
  { if(CheckedChanged!=null) CheckedChanged(this, new EventArgs());
  }

  protected override void OnClick(ClickEventArgs e)
  { if(e.CE.Button==MouseButton.Left && !e.Handled)
    { Checked   = !value;
      e.Handled = true;
    }
    base.OnClick(e);
  }

  bool value;
}
#endregion

#region CheckBox
public class CheckBox : CheckBoxBase
{ public CheckBox()
  { BorderStyle=BorderStyle.FixedThick; DontDraw|=DontDraw.Border; Padding=new RectOffset(1, 1);
  }
  public CheckBox(bool check) : this() { Checked=check; }
  public CheckBox(string text) : this() { Text=text; }
  public CheckBox(string text, bool check) : this() { Text=text; Checked=check; }

  protected internal override void OnPaintBackground(PaintEventArgs e)
  { base.OnPaintBackground(e);
    if(Focused) Helpers.DrawBorder(e.Surface, DrawRect, BorderStyle.Fixed3D, BorderColor, false);
  }

  protected internal override void OnPaint(PaintEventArgs e)
  { base.OnPaint(e);

    Rectangle rect = ContentDrawRect;

    int borderSize=BorderWidth, boxSize=11+borderSize; // constant 11 also used below
    bool right = Helpers.AlignedRight(TextAlign);
    ContentAlignment align = Helpers.AlignedTop(TextAlign)    ? ContentAlignment.TopLeft    :
                             Helpers.AlignedMiddle(TextAlign) ? ContentAlignment.MiddleLeft :
                             ContentAlignment.BottomLeft;

    GameLib.Fonts.Font font = Font;
    if(font!=null)
    { font.BackColor = BackColor;
      font.Color = Enabled ? ForeColor : SystemColors.GrayText;
      if(!right) rect.X = font.Render(e.Surface, Text, rect, align).X;
    }

    Rectangle box = new Rectangle(rect.X, rect.Y+(rect.Height-boxSize)/2, boxSize, boxSize);
    Helpers.DrawBorder(e.Surface, box, BorderStyle, BorderColor, true);
    box.Inflate(-borderSize, -borderSize);
    e.Surface.Fill(box, !down && Enabled ? SystemColors.Window : SystemColors.Control);
    if(Checked) Helpers.DrawCheck(e.Surface, box.X+1, box.Y+1, SystemColors.ControlText);

    if(font!=null && right)
    { rect.X += boxSize+3; rect.Width -= boxSize+3;
      font.Render(e.Surface, Text, rect, align);
    }
  }

  protected internal override void OnMouseDown(ClickEventArgs e)
  { down=true; Invalidate(ContentRect);
    base.OnMouseDown(e);
  }
  protected internal override void OnMouseUp(ClickEventArgs e)
  { down=false; Invalidate(ContentRect);
    base.OnMouseUp(e);
  }

  protected override void OnCheckedChanged(EventArgs e) { Invalidate(ContentRect); base.OnCheckedChanged(e); }
  
  // ensure that we have enough room for the checkbox
  // TODO: is this good idea?
  // or should we provide another mechanism for helping the user set the size of a checkbox?
  protected override void OnContentSizeChanged(EventArgs e)
  { int need = 11+BorderWidth - ContentHeight; // constant 11 also used above
    if(need>0) Height += need;
    base.OnContentSizeChanged(e);
  }

  protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
  protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

  protected override void OnTextAlignChanged(ValueChangedEventArgs e)
  { if(Helpers.AlignedCenter(TextAlign))
    { TextAlign = ContentAlignment.MiddleRight;
      throw new NotSupportedException("Middle alignment isn't supported for checkboxes.");
    }
    base.OnTextAlignChanged(e);
  }

  bool down;
}
#endregion

#region ScrollBarBase
public abstract class ScrollBarBase : Control, IDisposable
{ public ScrollBarBase()
  { Style = ControlStyle.Clickable|ControlStyle.Draggable|ControlStyle.CanFocus;
    ClickRepeatDelay = 300;
    DragThreshold    = 4;
  }
  ~ScrollBarBase() { Dispose(true); }
  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  public class ThumbEventArgs : EventArgs
  { public int Start;
    public int End;
  }
  public delegate void ThumbHandler(object sender, ThumbEventArgs e);

  protected enum Place { None, Down, PageDown, Thumb, PageUp, Up };

  #region Properties
  public bool AutoUpdate { get { return autoUpdate; } set { autoUpdate=value; } }

  public int PageIncrement { get { return pageInc; } set { pageInc=value; } }

  public uint ClickRepeatDelay
  { get { return crDelay; }
    set
    { if(value==crDelay) return;
      crDelay=value;
      if(value==0 && crTimer!=null)
      { crTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        crTimer.Dispose();
        crTimer = null;
        repeatEvent = null;
        repeated    = false;
      }
      else if(value!=0 && crTimer==null)
      { crTimer = new System.Threading.Timer(new System.Threading.TimerCallback(RepeatClick), null,
                                             System.Threading.Timeout.Infinite, crRate);
      }
    }
  }
  public uint ClickRepeatRate
  { get { return crRate; }
    set
    { if(value==crRate) return;
      crRate=value;
      if(crTimer!=null) crTimer.Change(crRate, crRate);
    }
  }

  public bool DraggingThumb { get { return dragOff!=-1; } }

  public int EndSize
  { get { return endSize; }
    set { if(endSize!=value) { endSize=value; Invalidate(); } }
  }

  public bool Horizontal
  { get { return horizontal; }
    set { if(value!=horizontal) { horizontal=value; Invalidate(); } }
  }

  public int Maximum
  { get { return max; }
    set { if(value!=max) { max=value; Invalidate(); } }
  }

  public int Minimum
  { get { return min; }
    set { if(value!=min) { min=value; Invalidate(); } }
  }

  public int Increment { get { return smallInc; } set { smallInc=value; } }

  public int ThumbSize
  { get { return thumbSize; }
    set { if(thumbSize!=value) { thumbSize=value; Invalidate(); } }
  }

  public int Value
  { get { return value; }
    set
    { if(value<min) value=min;
      else if(value>max) value=max;
      if(value!=this.value)
      { valChange.OldValue = this.value;
        this.value = value;
        OnValueChanged(valChange);
      }
    }
  }
  #endregion

  #region Events
  public event EventHandler Down, Up, PageDown, PageUp;
  public event ThumbHandler ThumbDragStart, ThumbDragMove, ThumbDragEnd;
  public event ValueChangedEventHandler ValueChanged;

  protected virtual void OnMouseDown(Place place) { }
  protected virtual void OnMouseUp() { }

  protected internal override void OnMouseDown(ClickEventArgs e)
  { if(!e.Handled && e.CE.Button==MouseButton.Left)
    { if(crTimer!=null)
      { Place p = FindPlace(e.CE.Point);
        OnMouseDown(p);
        if(p != Place.Thumb)
        { repeatEvent = new ClickRepeat(this, p);
          crTimer.Change(crDelay, crRate);
        }
      }
      if(!repeated) switch(FindPlace(e.CE.Point))
      { case Place.Down: OnDown(eventArgs); break;
        case Place.PageDown: OnPageDown(eventArgs); break;
        case Place.PageUp: OnPageUp(eventArgs); break;
        case Place.Up: OnUp(eventArgs); break;
      }
      e.Handled = true;
    }
  }
  protected internal override void OnMouseUp(ClickEventArgs e)
  { if(e.CE.Button==MouseButton.Left)
    { if(repeatEvent!=null)
      { crTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        repeatEvent = null;
        repeated    = false;
      }
      OnMouseUp();
    }
  }
  protected internal override void OnMouseLeave(EventArgs e)
  { if(repeatEvent!=null)
    { crTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
      repeatEvent = null;
      repeated    = false;
      OnMouseUp();
    } // if the mouse leaves while depressed, consider it unpressed
    else if(Mouse.Pressed(MouseButton.Left)) OnMouseUp();
  }
  protected internal override void OnDragStart(DragEventArgs e)
  { if(!repeated && e.Pressed(0) && FindPlace(e.Start)==Place.Thumb)
    { dragOff         = (horizontal ? e.Start.X : e.Start.Y) - ValueToThumb(value);
      thumbArgs.Start = value;
      OnThumbDragStart(thumbArgs);
    }
    else e.Cancel=true;
    base.OnDragStart(e);
  }
  protected internal override void OnDragMove(DragEventArgs e)
  { if(dragOff != -1)
    { thumbArgs.End = ThumbToValue((horizontal ? e.End.X : e.End.Y) - dragOff);
      OnThumbDragMove(thumbArgs);
    }
    base.OnDragMove(e);
  }
  protected internal override void OnDragEnd(DragEventArgs e)
  { if(dragOff != -1)
    { thumbArgs.End = ThumbToValue((horizontal ? e.End.X : e.End.Y) - dragOff);
      dragOff = -1;
      OnThumbDragEnd(thumbArgs);
    }
    base.OnDragEnd(e);
  }
  protected internal override void OnCustomEvent(Events.WindowEvent e)
  { if(e is ClickRepeat) switch(((ClickRepeat)e).Place)
    { case Place.Down: OnDown(eventArgs); break;
      case Place.PageDown: OnPageDown(eventArgs); break;
      case Place.PageUp: OnPageUp(eventArgs); break;
      case Place.Up: OnUp(eventArgs); break;
      default: base.OnCustomEvent(e); break;
    }
  }
  protected internal override void OnKeyDown(KeyEventArgs e)
  { if(!e.Handled) switch(e.KE.Key)
    { case Key.PageDown: OnPageUp(eventArgs); break;
      case Key.PageUp: OnPageDown(eventArgs); break;
      case Key.Down: case Key.Right: OnUp(eventArgs); break;
      case Key.Up: case Key.Left: OnDown(eventArgs); break;
    }
    base.OnKeyDown(e);
  }

  protected virtual void OnValueChanged(ValueChangedEventArgs e) { if(ValueChanged!=null) ValueChanged(this, e); }
  protected virtual void OnDown(EventArgs e)
  { if(autoUpdate) Value=value-smallInc;
    if(Down!=null) Down(this, e);
  }
  protected virtual void OnUp(EventArgs e)
  { if(autoUpdate) Value=value+smallInc;
    if(Up!=null) Up(this, e);
  }
  protected virtual void OnPageDown(EventArgs e)
  { if(autoUpdate) Value=value-pageInc;
    if(PageDown!=null) PageDown(this, e);
  }
  protected virtual void OnPageUp(EventArgs e)
  { if(autoUpdate) Value=value+pageInc;
    if(PageUp!=null) PageUp(this, e);
  }
  protected virtual void OnThumbDragStart(ThumbEventArgs e)
  { if(ThumbDragStart!=null) ThumbDragStart(this, e);
  }
  protected virtual void OnThumbDragMove(ThumbEventArgs e)
  { if(autoUpdate) Value=e.End;
    if(ThumbDragMove!=null) ThumbDragMove(this, e);
  }
  protected virtual void OnThumbDragEnd(ThumbEventArgs e)
  { if(autoUpdate) Value=e.End;
    if(ThumbDragEnd!=null) ThumbDragEnd(this, e);
  }
  #endregion

  protected void Dispose(bool finalizing)
  { if(crTimer!=null)
    { crTimer.Dispose();
      crTimer=null;
    }
  }

  protected Place FindPlace(Point click)
  { int size = horizontal ? Width : Height;
    int pos  = horizontal ? click.X : click.Y, thumb = ValueToThumb(value);
    if(pos<endSize) return Place.Down;
    else if(pos<thumb) return Place.PageDown;
    else if(pos<thumb+thumbSize) return Place.Thumb;
    else if(pos<size-endSize) return Place.PageUp;
    else return Place.Up;
  }

  protected void RepeatClick(object dummy)
  { ClickRepeat rc = repeatEvent;
    if(rc!=null) { Events.Events.PushEvent(rc); repeated=true; }
  }

  protected int ThumbToValue(int position)
  { if(position<0) position=0;
    if(horizontal)
    { if(position>Width) position=Width-1;
    }
    else if(position>Height) position=Height-1;
    return (position-endSize)*(max-min)/Space+min;
  }

  protected int ValueToThumb(int value) { return (value-min)*Space/(max-min)+endSize; }

  class ClickRepeat : Events.WindowEvent
  { public ClickRepeat(Control control, Place place) : base(control) { Place=place; }
    public Place Place;
  }

  int Space { get { return (horizontal ? Width : Height)-EndSize*2-ThumbSize; } }

  System.Threading.Timer crTimer;
  ClickRepeat repeatEvent;
  ThumbEventArgs thumbArgs = new ThumbEventArgs();
  EventArgs eventArgs = new EventArgs();
  ValueChangedEventArgs valChange = new ValueChangedEventArgs(null);
  int  value, min, max=100, smallInc=1, pageInc=10, endSize=16, thumbSize=16, dragOff=-1;
  uint crDelay, crRate=50;
  bool autoUpdate=true, horizontal, repeated;
}
#endregion

#region ScrollBar
public class ScrollBar : ScrollBarBase
{ public ScrollBar()
  { BackColor=SystemColors.ControlDark; ForeColor=SystemColors.Control; Size=new Size(EndSize, EndSize);
  }

  protected override void OnValueChanged(ValueChangedEventArgs e)
  { Refresh();
    base.OnValueChanged(e);
  }

  protected internal override void OnPaint(PaintEventArgs e)
  { base.OnPaint(e);
    Rectangle rect = ContentDrawRect;
    int thumb = ValueToThumb(Value);
    if(Horizontal)
    { int x=rect.X, w=rect.Width; rect.Width=EndSize;
      DrawEnd(e.Surface, rect, Place.Down);
      rect.X = x+w-EndSize;
      DrawEnd(e.Surface, rect, Place.Up);
      rect.X = x+thumb; rect.Width = ThumbSize;
    }
    else
    { int y=rect.Y, h=rect.Height;
      rect.Height = EndSize;
      DrawEnd(e.Surface, rect, Place.Down);
      rect.Y = y+h-EndSize;
      DrawEnd(e.Surface, rect, Place.Up);
      rect.Y = y+thumb; rect.Height = ThumbSize;
    }
    e.Surface.Fill(rect, ForeColor);
    Helpers.DrawBorder(e.Surface, rect, BorderStyle.FixedThick, ForeColor, false);
  }

  protected override void OnMouseDown(Place place) { down=place; Invalidate(); }
  protected override void OnMouseUp() { down=Place.None; Invalidate(); }

  void DrawEnd(Surface surface, Rectangle rect, Place place)
  { surface.Fill(rect, ForeColor);
    Helpers.DrawBorder(surface, rect, BorderStyle.FixedThick, ForeColor, down==place);
    if(down==place) rect.Offset(1, 1);
    Helpers.DrawArrow(surface, rect, place==Place.Up ? Horizontal ? Helpers.Arrow.Right : Helpers.Arrow.Down
                                                     : Horizontal ? Helpers.Arrow.Left  : Helpers.Arrow.Up,
                      EndSize/4, Enabled ? Color.Black : SystemColors.GrayText);
  }

  Place down;
}
#endregion

#region TextBoxBase
// TODO: support multi-line editing
public abstract class TextBoxBase : Control
{ public TextBoxBase()
  { Style=ControlStyle.CanFocus;
    BackColor=SystemColors.Window; ForeColor=SystemColors.WindowText;
  }
  static TextBoxBase() { CaretFlashRate=600; }

  #region Properties
  public int CaretPosition
  { get { return caret; }
    set
    { if(value!=caret)
      { if(value<0 || value>Text.Length) throw new ArgumentOutOfRangeException("CaretPosition");
        int oldlen = selectLen;
        ValueChangedEventArgs e = new ValueChangedEventArgs(caret);
        caret = value;
        if(selectLen>0)
        { if(caret+selectLen>Text.Length) selectLen = Text.Length-caret;
        }
        else if(caret-selectLen<0) selectLen = -caret;
        caretOn = true;
        if(oldlen!=0 && (Selected || !hideSelection)) Invalidate(ContentRect);
        OnCaretPositionChanged(e);
      }
    }
  }

  public bool HideSelection
  { get { return hideSelection; }
    set
    { hideSelection=value;
      if(value && !Selected) Invalidate(ContentRect);
    }
  }

  public string[] Lines
  { get { return Text.Split('\n'); }
    set { Text=string.Join("\n", value); }
  }

  public int MaxLength
  { get { return maxLength; }
    set
    { if(maxLength<-1) throw new ArgumentOutOfRangeException("MaxLength", value, "must be >= -1");
      maxLength = MaxLength;
      if(maxLength>Text.Length) Text = Text.Substring(0, maxLength);
    }
  }

  public bool Modified
  { get { return modified; }
    set
    { if(value!=modified)
      { modified=value;
        OnModifiedChanged(new EventArgs());
      }
    }
  }

  public bool MultiLine
  { get { return false; }
    set { if(value) throw new NotImplementedException("MultiLine text boxes not implemented"); }
  }

  public string SelectedText
  { get
    { return selectLen<0 ? Text.Substring(caret+selectLen, -selectLen) : Text.Substring(caret, selectLen);
    }
    set
    { if(value==null) throw new ArgumentNullException("SelectedText");
      int start=caret, end=caret;
      if(selectLen<0) start += selectLen;
      else end += selectLen;
      Select(start, value.Length);
      Text = Text.Substring(0, start) + value + Text.Substring(end, Text.Length-end);
    }
  }

  public int SelectionEnd { get { return selectLen<0 ? caret : caret+selectLen; } }

  public int SelectionLength
  { get { return selectLen; }
    set
    { if(value!=selectLen)
      { if(value<-Text.Length || value>Text.Length) throw new ArgumentOutOfRangeException("SelectionLength");
        selectLen = value;
        if(Selected || !hideSelection) Invalidate(ContentRect);
      }
    }
  }

  public int SelectionStart { get { return selectLen<0 ? caret+selectLen : caret; } }

  public bool SelectOnFocus { get { return selectOnFocus; } set { selectOnFocus=value; } }

  public override string Text
  { set
    { if(caret>value.Length)
      { if(selectLen<0)
        { if(-selectLen > value.Length) Select(value.Length, -value.Length);
        }
        else Select(value.Length, 0);
      }
      else if(caret+selectLen>value.Length) Select(caret, value.Length-caret);
      base.Text = value;
    }
  }

  public bool WordWrap { get { return wordWrap; } set { wordWrap=value; } }
  #endregion

  #region Events
  public event EventHandler ModifiedChanged;
  protected virtual void OnModifiedChanged(EventArgs e) { if(ModifiedChanged!=null) ModifiedChanged(this, e); }
  protected virtual void OnCaretPositionChanged(ValueChangedEventArgs e) { }
  protected virtual void OnCaretFlash() { }

  protected override void OnGotFocus (EventArgs e)
  { if(selectOnFocus) { SelectAll(); Invalidate(ContentRect); } // TODO: does this Invalidate() call need to be here?
    WithCaret=this;
    base.OnGotFocus(e);
  }
  protected override void OnLostFocus(EventArgs e)
  { if(hideSelection) Invalidate(ContentRect);
    WithCaret=null;
    base.OnLostFocus(e);
  }

  protected internal override void OnKeyPress(KeyEventArgs e)
  { if(e.KE.Char>=32)
    { if(maxLength==-1 || Text.Length<maxLength) { Modified=true; InsertText(e.KE.Char.ToString()); }
      e.Handled=true;
    }
    base.OnKeyPress(e);
  }

  protected internal override void OnKeyDown(KeyEventArgs e)
  { char c=e.KE.Char;
    if(e.KE.Key==Key.Left)
    { if(e.KE.KeyMods==KeyMod.None || e.KE.HasOnlyKeys(KeyMod.Shift|KeyMod.Ctrl))
      { if(caret>0)
        { if(e.KE.KeyMods==KeyMod.None) Select(caret-1, 0);
          else if(e.KE.HasOnlyKeys(KeyMod.Shift)) Select(caret-1, selectLen+1);
          else
          { int pos = CtrlScan(-1);
            if(e.KE.HasOnlyKeys(KeyMod.Ctrl)) Select(pos, 0);
            else if(selectLen<0) Select(pos, SelectionStart-pos);
            else Select(pos, SelectionEnd-pos);
          }
        }
        else if(!e.KE.HasAnyMod(KeyMod.Shift)) SelectionLength=0;
        e.Handled=true;
      }
    }
    else if(e.KE.Key==Key.Right)
    { if(e.KE.KeyMods==KeyMod.None || e.KE.HasOnlyKeys(KeyMod.Shift|KeyMod.Ctrl))
      { if(caret<Text.Length)
        { if(e.KE.KeyMods==KeyMod.None) Select(caret+1, 0);
          else if(e.KE.HasOnlyKeys(KeyMod.Shift)) Select(caret+1, selectLen-1);
          else
          { int pos = CtrlScan(1);
            if(e.KE.HasOnlyKeys(KeyMod.Ctrl)) Select(pos, 0);
            else if(selectLen<0) Select(pos, SelectionStart-pos);
            else Select(pos, SelectionEnd-pos);
          }
        }
        else if(!e.KE.HasAnyMod(KeyMod.Shift)) SelectionLength=0;
        e.Handled=true;
      }
    }
    else if(e.KE.Key==Key.Backspace)
    { if(e.KE.KeyMods==KeyMod.None)
      { if(selectLen!=0) { Modified=true; SelectedText=""; }
        else if(caret>0)
        { Modified=true;
          Select(caret-1, 0);
          Text = Text.Substring(0, caret) + Text.Substring(caret+1, Text.Length-caret-1);
        }
        e.Handled=true;
      }
      else if(e.KE.HasOnlyKeys(KeyMod.Ctrl))
      { if(caret>0 && selectLen==0)
        { int end=caret, pos=CtrlScan(-1);
          Modified=true;
          Select(pos, 0);
          Text = Text.Substring(0, pos) + Text.Substring(end, Text.Length-end);
        }
        e.Handled=true;
      }
    }
    else if(e.KE.Key==Key.Delete && !e.KE.HasAnyMod(KeyMod.Shift))
    { if(e.KE.KeyMods==KeyMod.None || e.KE.HasOnlyKeys(KeyMod.Ctrl))
      { if(selectLen!=0) { Modified=true; SelectedText=""; }
        else if(caret<Text.Length)
        { Modified=true;
          if(e.KE.KeyMods==KeyMod.None)
            Text = Text.Substring(0, caret) + Text.Substring(caret+1, Text.Length-caret-1);
          else
          { int pos = CtrlScan(1);
            Text = Text.Substring(0, caret) + Text.Substring(pos, Text.Length-pos);
          }
        }
        e.Handled=true;
      }
    }
    else if(e.KE.Key==Key.Home)
    { if(e.KE.KeyMods==KeyMod.None) { Select(0, 0); e.Handled=true; }
      else if(e.KE.HasOnlyKeys(KeyMod.Shift))
      { if(selectLen<0) Select(0, SelectionStart);
        else Select(0, SelectionEnd);
        e.Handled=true;
      }
    }
    else if(e.KE.Key==Key.End)
    { if(e.KE.KeyMods==KeyMod.None) { Select(Text.Length, 0); e.Handled=true; }
      else if(e.KE.HasOnlyKeys(KeyMod.Shift))
      { if(selectLen<0) Select(Text.Length, SelectionStart-Text.Length);
        else Select(Text.Length, SelectionEnd-Text.Length);
        e.Handled=true;
      }
    }
    else if(e.KE.HasOnlyKeys(KeyMod.Ctrl) && (c=='C'-64 || e.KE.Key==Key.Insert))
    { Copy();
      e.Handled=true;
    }
    else if(c=='X'-64 && e.KE.HasOnlyKeys(KeyMod.Ctrl) || e.KE.Key==Key.Delete && e.KE.HasOnlyKeys(KeyMod.Shift))
    { if(selectLen!=0)
      { Modified=true;
        Cut();
      }
      e.Handled=true;
    }
    else if(c=='V'-64 && e.KE.HasOnlyKeys(KeyMod.Ctrl) || e.KE.Key==Key.Insert && e.KE.HasOnlyKeys(KeyMod.Shift))
    { if(maxLength==-1 || Text.Length<maxLength)
      { Modified=true;
        if(maxLength==-1) Paste();
        else
        { int avail=maxLength-Text.Length;
          InsertText(clipboard.Length>avail ? clipboard.Substring(0, avail) : clipboard);
        }
      }
      e.Handled=true;
    }
    else if(c=='A'-64 && e.KE.HasOnlyKeys(KeyMod.Ctrl)) { SelectAll(); e.Handled=true; }
  }

  protected internal override void OnCustomEvent(Events.WindowEvent e)
  { if(e is CaretFlashEvent) OnCaretFlash();
    else base.OnCustomEvent(e);
  }
  #endregion

  #region Methods
  public void AppendText(string text) { Text+=text; }
  // TODO: implement a better clipboard (GameLib.Forms-wide?)
  public void Copy()  { clipboard=SelectedText; }
  public void Cut()   { clipboard=SelectedText; SelectedText=""; }
  public void Paste() { InsertText(clipboard); }

  public void InsertText(string text)
  { if(selectLen!=0) { SelectedText = text; Select(caret+selectLen, 0); }
    else if(text.Length>0)
    { Text = Text.Substring(0, caret) + text + Text.Substring(caret, Text.Length-caret);
      CaretPosition += text.Length;
    }
  }

  public void ScrollToCaret() { }

  public void Select(int start, int length) { CaretPosition=start; SelectionLength=length; }
  public void SelectAll() { CaretPosition=Text.Length; SelectionLength=-Text.Length; }
  #endregion

  protected bool HasCaret { get { return withCaret==this && Focused; } }

  class CaretFlashEvent : Events.WindowEvent { public CaretFlashEvent(TextBoxBase tb) : base(tb) { } }

  int CtrlScan(int dir)
  { // skip whitespace
    // skip punctuation
    // scan until whitespace or punctuation
    string text = Text;
    int i=caret;
    if(dir<0) { if(--i<0) return 0; }
    else if(caret==Text.Length) return caret;
    for(; i>=0 && i<Text.Length; i+=dir) if(!char.IsWhiteSpace(text[i])) break;
    for(; i>=0 && i<Text.Length; i+=dir) if(!IsPunctuation(text[i])) break;
    for(; i>=0 && i<Text.Length; i+=dir)
      if(IsPunctuation(text[i]) || char.IsWhiteSpace(text[i])) { if(dir<0) i++; break; }
    return i<0 ? 0 : i;
  }

  bool IsPunctuation(char c) { return char.IsPunctuation(c) || char.IsSymbol(c); }

  int  caret, selectLen, maxLength=-1;
  bool hideSelection=true, modified, wordWrap, selectOnFocus=true;

  #region Statics
  public static int CaretFlashRate
  { get { return flashRate; }
    set
    { if(value!=flashRate)
      { flashRate=value;
        if(value==0)
        { if(caretTimer!=null)
          { caretTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            caretTimer.Dispose();
            caretTimer=null;
          }
        }
        else if(caretTimer!=null) caretTimer.Change(flashRate/2, flashRate);
        else caretTimer = new System.Threading.Timer(new System.Threading.TimerCallback(CaretFlash), null,
                                                     flashRate/2, flashRate);
      }
    }
  }

  protected static bool CaretOn { get { return caretOn; } set { caretOn=value; } }

  static TextBoxBase WithCaret
  { get { return withCaret; }
    set
    { if(withCaret!=value)
      { if(withCaret!=null) withCaret.Invalidate(withCaret.ContentRect);
        withCaret=value;
        if(withCaret!=null) { caretOn=true; DoFlash(withCaret); }
      }
    }
  }

  static void CaretFlash(object dummy)
  { caretOn = !caretOn;
    TextBoxBase tb = withCaret;
    if(tb!=null && tb.HasCaret) DoFlash(tb);
  }

  static void DoFlash(TextBoxBase tb) { Events.Events.PushEvent(new CaretFlashEvent(tb)); }

  static System.Threading.Timer caretTimer;
  static TextBoxBase withCaret;
  static string clipboard = string.Empty;
  static int flashRate;
  static bool caretOn;
  #endregion
}
#endregion

#region TextBox
// TODO: optimize this
// FIXME: find out why the beginning of the textbox starts out blank in the windowing example
public class TextBox : TextBoxBase
{ public TextBox()
  { Style|=ControlStyle.Clickable|ControlStyle.Draggable;
    BorderStyle=BorderStyle.FixedThick|BorderStyle.Depressed;
    BorderColor=SystemColors.WindowText;
    Padding = new RectOffset(2, 0);
  }

  protected internal override void OnPaintBackground(PaintEventArgs e)
  { base.OnPaintBackground(e);

    GameLib.Fonts.Font font = Font;
    Rectangle rect = ContentDrawRect;
    if(font!=null && Text.Length>0)
    { int caret = CaretPosition;
      if(caret<start) start = Math.Max(caret-10, 0);
      string text;
      while(true)
      { text = start==0 ? Text : Text.Substring(start);
        end = start+font.HowManyFit(text, rect.Width);
        if(end-start<text.Length && caret>=end) start = Math.Min(start+10+caret-end, Text.Length);
        else break;
      }

      if((!HideSelection || Focused) && SelectionLength!=0 && Enabled)
      { text = Text.Substring(start, end-start);
        if(SelectionStart>start) rect.X += font.CalculateSize(text.Substring(0, SelectionStart-start)).Width;
        rect.Width = font.CalculateSize(text.Substring(Math.Max(0, SelectionStart-start),
                                                       Math.Min(Math.Abs(SelectionLength), end-start))).Width;
        e.Surface.Fill(rect, ForeColor);
      }
    }
    else start=end=0;
  }

  protected internal override void OnPaint(PaintEventArgs e)
  { base.OnPaint(e);

    GameLib.Fonts.Font font = Font;
    if(font!=null)
    { Rectangle rect = ContentDrawRect;
      Point location = rect.Location;
      if(rect.Height>font.Height) location.Y += (rect.Height-font.Height)/2;

      string text = Text.Substring(start, end-start);

      Color fore = Enabled ? ForeColor : SystemColors.GrayText;

      font.Color = fore;
      font.BackColor = BackColor;

      if((HideSelection && !Focused) || SelectionLength==0) font.Render(e.Surface, text, location);
      else
      { if(SelectionStart>start)
          location.X += font.Render(e.Surface, text.Substring(0, SelectionStart-start), location);

        font.Color     = BackColor;
        font.BackColor = fore;
        location.X += font.Render(e.Surface, text.Substring(Math.Max(0, SelectionStart-start),
                                                            Math.Min(Math.Abs(SelectionLength), end-start)), location);
        if(SelectionEnd<end)
        { font.Color     = fore;
          font.BackColor = BackColor;
          font.Render(e.Surface, text.Substring(SelectionEnd-start, end-SelectionEnd), location);
        }
      }

      int x = rect.X + font.CalculateSize(text.Substring(0, CaretPosition-start)).Width;
      Primitives.VLine(e.Surface, x, rect.Top+2, rect.Bottom-3, CaretOn && HasCaret ? fore : BackColor);
    }
  }

  protected internal override void OnMouseClick(ClickEventArgs e)
  { if(e.CE.Button==MouseButton.Left)
    { Select(PointToPos(e.CE.X), 0);
      e.Handled = true;
    }
  }

  protected internal override void OnDragStart(DragEventArgs e)
  { if(e.Pressed(0) && !e.Cancel) Select(PointToPos(e.Start.X), 0);
    else e.Cancel=true;
    base.OnDragStart(e);
  }

  protected internal override void OnDragMove(DragEventArgs e)
  { int start = PointToPos(e.Start.X), end = PointToPos(e.End.X);
    Select(end, start-end);
    base.OnDragMove(e);
  }

  protected internal override void OnDragEnd(DragEventArgs e)
  { int start = PointToPos(e.Start.X), end = PointToPos(e.End.X);
    Select(end, start-end);
    base.OnDragEnd(e);
  }

  protected override void OnCaretPositionChanged(ValueChangedEventArgs e) { Invalidate(ContentRect); }

  protected override void OnTextChanged(ValueChangedEventArgs e)
  { Invalidate(ContentRect);
    base.OnTextChanged(e);
  }

  protected override void OnCaretFlash()
  { if(BackingSurface!=null) Invalidate(ContentRect); // TODO: optimize this!
    else
    { DesktopControl desktop = Desktop;
      GameLib.Fonts.Font font = Font;
      if(desktop!=null && desktop.Surface!=null && font!=null)
      { Rectangle rect = ContentDrawRect;
        rect.Y += 2; rect.Height -= 4; rect.Width = 1;
        rect.X += font.CalculateSize(Text.Substring(start, CaretPosition-start)).Width;
        Primitives.VLine(desktop.Surface, rect.X, rect.Y, rect.Bottom-1, CaretOn && HasCaret ? ForeColor : BackColor);
        desktop.AddUpdatedArea(rect);
        if(Events.Events.Count==0) Events.Events.PushEvent(new Events.DesktopUpdatedEvent(desktop));
      }
    }
  }

  int PointToPos(int x)
  { Rectangle rect = ContentRect;
    if(x<rect.X) return start;
    else if(x>rect.Width) return end;
    else
    { GameLib.Fonts.Font font = Font;
      return font==null ? 0 : start + font.HowManyFit(Text.Substring(start, end-start), x-rect.X);
    }
  }

  int start, end;
}
#endregion

#endregion

#region List controls
#region ListControl
// TODO: implement multi-column list boxes
// TODO: document
public abstract class ListControl : ScrollableControl
{ protected ListControl() { items = new ItemCollection(this); }
  protected ListControl(ICollection items) { this.items=new ItemCollection(this, items); }
  protected ListControl(IEnumerable items)
  { this.items = new ItemCollection(this);
    foreach(object o in items) this.items.InnerAdd(o);
  }

  #region ItemCollection
  public sealed class ItemCollection : CollectionBase
  { public ItemCollection(ListControl list) { parent=list; }
    public ItemCollection(ListControl list, ICollection items) { parent=list; InnerList.AddRange(items); }
    
    public object this[int index] { get { return InnerList[index]; } }

    public int Add(object item) { return List.Add(item); }
    public void AddRange(params object[] items) { for(int i=0; i<items.Length; i++) List.Add(items[i]); }
    public void AddRange(ICollection items) { foreach(object o in items) List.Add(o); }

    // TODO: handle updating the selection when an item moves
    public void Insert(int index, object item)
    { if(sorted) Add(item);
      else List.Insert(index, item);
    }

    // TODO: handle updating the selection when an item is removed
    public void Remove(object item) { List.Remove(item); }

    internal bool Sorted
    { get { return sorted; }
      set
      { if(value && !sorted)
        { // TODO: updated the indexes of the parent (ie, CalcIndexes)
          throw new NotImplementedException(); // TODO: implement sorted lists (be sure to preserve selected items)
          parent.Invalidate(parent.ContentRect);
        }
        sorted=value;
      }
    }
    internal int IndexOf(object item) { return InnerList.IndexOf(item); } // TODO: use sorted list
    internal void InnerAdd(object item) { InnerList.Add(item); }

    protected override void OnClearComplete()
    { parent.OnCountChanged();
      parent.Invalidate(parent.ContentRect);
    }
    protected override void OnInsertComplete(int index, object value) { RedrawFrom(index); }
    protected override void OnRemoveComplete(int index, object value) { RedrawFrom(index); }
    protected override void OnSetComplete(int index, object oldValue, object newValue)
    { parent.OnCountChanged();
      parent.Invalidate(parent.GetItemRectangle(index, true));
    }
    
    void RedrawFrom(int index)
    { parent.OnCountChanged();
      Rectangle rect = parent.GetItemRectangle(index, true);
      rect.Height += parent.Bottom-rect.Bottom; // invalidate all the items after this one, too
      parent.Invalidate(rect);
    }

    ListControl parent;
    bool sorted;
  }
  #endregion

  #region SelectedIndexCollection
  protected class SelectedIndexCollection : CollectionBase
  { 
  }
  #endregion

  public ItemCollection Items { get { return items; } }
  public abstract int SelectedIndex { get; set; }

  public object SelectedItem
  { get { return items[SelectedIndex]; }
    set { SelectedIndex = items.IndexOf(value); }
  }

  public bool Sorted { get { return items.Sorted; } set { items.Sorted=value; } }

  public override string Text
  { get { return SelectedIndex<0 ? "" : GetItemText(SelectedIndex); }
    set { SelectedIndex = FindStringExact(value); }
  }

  public abstract int TopIndex { get; set; }

  public int FindString(string startsWith) { return FindString(startsWith, 0); }
  public int FindString(string startsWith, int from)
  { for(; from<items.Count; from++) if(GetItemText(from).StartsWith(startsWith)) return from;
    return -1;
  }

  public int FindStringExact(string text) { return FindStringExact(text, 0); }
  public int FindStringExact(string text, int from)
  { for(; from<items.Count; from++) if(GetItemText(from)==text) return from;
    return -1;
  }

  public string GetItemText(int index) { return items[index].ToString(); }
  public Size GetPreferredSize() { return GetPreferredSize(items.Count); }
  public abstract Size GetPreferredSize(int numItems);

  protected abstract void DrawItem(int index, PaintEventArgs e, Rectangle bounds);
  protected abstract Rectangle GetItemRectangle(int index, bool onlySeen);
  protected abstract bool IsSelected(int index);
  protected abstract Size MeasureItem(int index);
  protected virtual void OnCountChanged() { }
  protected abstract int PointToItem(Point clientPoint);

  ItemCollection items;
}
#endregion

#region ListBoxBase
public abstract class ListBoxBase : ListControl
{ protected ListBoxBase() { Init(); }
  protected ListBoxBase(ICollection items) : base(items) { Init(); }
  protected ListBoxBase(IEnumerable items) : base(items) { Init(); }
  void Init()
  { selected=-1; bottom=-1;
    selBack=SystemColors.Highlight;
    selFore=SystemColors.HighlightText;
    Padding=new RectOffset(1);
    Style|=ControlStyle.CanFocus;
  }

  public Color SelectedBackColor { get { return selBack; } set { selBack=value; } }
  public Color SelectedForeColor { get { return selFore; } set { selFore=value; } }

  public override int SelectedIndex
  { get { return selected; }
    set
    { int newValue = value<-1 || value>=Items.Count ? -1 : value;
      if(newValue!=selected)
      { if(selected>=0) Invalidate(GetItemRectangle(selected, true));
        if(newValue>=0) Invalidate(GetItemRectangle(newValue, true));
        selected = newValue;
      }
    }
  }

  public override int TopIndex
  { get { return top; }
    set
    { int newValue = Math.Max(Math.Min(value, lastTop), 0);
      if(newValue!=top)
      { top=newValue;
        if(VerticalScrollBar!=null) VerticalScrollBar.Value = newValue;
        Invalidate(ContentRect);
        bottom = Math.Max(Math.Min(value, Items.Count-1), 0)==lastTop ? lastTop : -1;
      }
    }
  }

  public override Size GetPreferredSize(int numItems)
  { if(numItems<0 || numItems>=Items.Count)
      throw new ArgumentOutOfRangeException("numItems", numItems, "out of range");

    Size ret = new Size(0, 0);
    for(int i=0; i<numItems; i++)
    { Size size = MeasureItem(i);
      ret.Height += size.Height;
      if(size.Width>ret.Width) ret.Width=size.Width;
    }

    return ContentOffset.Grow(ret);
  }

  protected int FindChar(char c) { return FindChar(c, 0); }
  protected int FindChar(char c, int start)
  { c = char.ToUpper(c);
    for(; start<Items.Count; start++)
    { string s = GetItemText(start);
      if(s.Length>0 && char.ToUpper(s[0])==c) return start;
    }
    return -1;
  }

  protected int GetBottomIndex()
  { if(bottom==-1)
    { Rectangle bounds = ContentRect;
      int i;
      for(i=TopIndex; i<Items.Count && bounds.Height>0; i++) bounds.Height -= MeasureItem(i).Height;
      bottom = i-1;
    }
    return bottom;
  }

  protected override Rectangle GetItemRectangle(int index, bool onlySeen)
  { if(index<0 || index>=Items.Count) throw new ArgumentOutOfRangeException("index", index, "out of range");
    if(onlySeen && index<TopIndex) return new Rectangle(-1, -1, 0, 0);

    Rectangle bounds = ContentRect;
    if(index<TopIndex)
    { for(int i=TopIndex; i>=index; i--)
      { int height = MeasureItem(i).Height;
        bounds.Y -= height; bounds.Height = height;
      }
      return bounds;
    }
    else
    { int i=TopIndex, end=bounds.Bottom;
      while(true)
      { int height = MeasureItem(i).Height;
        bounds.Height = height;
        if(i++==index) return bounds;
        if(bounds.Y>=end) return new Rectangle(-1, -1, 0, 0);
        bounds.Y += height;
      }
    }
  }

  protected override bool IsSelected(int index) { return index==selected; }

  protected override ScrollBarBase MakeScrollBar(bool horizontal)
  { ScrollBarBase bar = base.MakeScrollBar(horizontal);
    bar.Style &= ~ControlStyle.CanFocus;
    return bar;
  }

  protected override void OnCountChanged()
  { base.OnCountChanged();
    CalcIndexes();
  }

  protected internal override void OnCustomEvent(Events.WindowEvent e)
  { if(e is ScrollEvent) DoScroll();
    else base.OnCustomEvent(e);
  }

  protected internal override void OnKeyDown(KeyEventArgs e)
  { if(e.KE.KeyMods==KeyMod.None)
    { if(e.KE.Key==Key.Up || e.KE.Key==Key.Down)
      { StopScrolling();
        Capture = down = false;
        if(e.KE.Key==Key.Up) ScrollUp();
        else ScrollDown();
        e.Handled=true;
      }
      else if(e.KE.Key==Key.PageUp)
      { ScrollTo(SelectedIndex);
        if(SelectedIndex>TopIndex) SelectedIndex=TopIndex;
        else SelectedIndex=TopIndex=BottomToTop(TopIndex);
      }
      else if(e.KE.Key==Key.PageDown)
      { ScrollTo(SelectedIndex);
        if(SelectedIndex<GetBottomIndex()) SelectedIndex=bottom;
        else { TopIndex=bottom; SelectedIndex=GetBottomIndex(); }
      }
      else if(e.KE.Key==Key.Home) TopIndex=SelectedIndex=0;
      else if(e.KE.Key==Key.End)
      { TopIndex = lastTop;
        SelectedIndex = Items.Count-1;
      }
    }
    base.OnKeyDown(e);
  }

  protected internal override void OnKeyPress(KeyEventArgs e)
  { int next = FindChar(e.KE.Char, SelectedIndex+1);
    if(next==-1) next = FindChar(e.KE.Char);
    if(next!=-1) ScrollTo(SelectedIndex=next);
    base.OnKeyPress(e);
  }

  protected internal override void OnMouseDown(ClickEventArgs e)
  { if(!e.Handled && e.CE.Button==MouseButton.Left)
    { SelectedIndex = PointToItem(e.CE.Point);
      Capture = e.Handled = down = true;
    }
    base.OnMouseDown(e);
  }

  protected internal override void OnMouseMove(Events.MouseMoveEvent e)
  { if(down)
    { int index = PointToItem(new Point(Width/2, e.Y)); // assumes that Width/2 is within ContentRect
      if(index==-1)
      { StartScrolling();
        DoScroll();
      }
      else
      { StopScrolling();
        SelectedIndex = index;
      }
    }
    base.OnMouseMove(e);
  }

  protected internal override void OnMouseUp(ClickEventArgs e)
  { if(!e.Handled && down && e.CE.Button==MouseButton.Left)
    { Capture = down = false;
      StopScrolling();
      e.Handled = true;
    }
    base.OnMouseUp(e);
  }

  protected internal override void OnPaint(PaintEventArgs e)
  { base.OnPaint(e);

    Rectangle bounds = ContentDrawRect; // TODO: set e.Surface.ClipRect
    bool drew=false;
    for(int i=TopIndex; i<Items.Count; i++)
    { Rectangle itemRect = new Rectangle(bounds.X, bounds.Y, bounds.Width, MeasureItem(i).Height);
      if(itemRect.Height>bounds.Height) break;
      if(itemRect.IntersectsWith(e.DisplayRect)) { DrawItem(i, e, itemRect); drew=true; }
      else if(drew) break;
      bounds.Y += itemRect.Height; bounds.Height -= itemRect.Height;
    }
  }

  protected override void OnResize(EventArgs e)
  { base.OnResize(e);
    CalcIndexes();
  }

  protected override void OnVerticalScroll(object bar, ValueChangedEventArgs e)
  { base.OnVerticalScroll(bar, e);
    TopIndex = VerticalScrollBar.Value;
  }

  protected override int PointToItem(Point clientPoint)
  { Rectangle bounds = ContentRect;
    for(int i=TopIndex; i<Items.Count && bounds.Height>0; i++)
    { Rectangle itemRect = new Rectangle(bounds.X, bounds.Y, bounds.Width, MeasureItem(i).Height);
      if(itemRect.Contains(clientPoint)) return i;
      bounds.Y += itemRect.Height; bounds.Height -= itemRect.Height;
    }
    return -1;
  }

  class ScrollEvent : Events.WindowEvent { public ScrollEvent(Control ctrl) : base(ctrl) { } }

  int BottomToTop() { return BottomToTop(Items.Count-1); }
  int BottomToTop(int start)
  { Rectangle bounds = ContentRect;
    for(; start>=0 && bounds.Height>0; start--) bounds.Height -= MeasureItem(start).Height;
    return start+1;
  }

  void CalcIndexes()
  { bottom  = -1;
    lastTop = BottomToTop();
    if(lastTop>0)
    { ShowVerticalScrollBar = true;
      VerticalScrollBar.Maximum = lastTop;
    }
    else ShowVerticalScrollBar = false;
  }

  void DoScroll()
  { if(down)
    { Point pt = ScreenToWindow(Mouse.Point);
      if(pt.Y<0) ScrollUp();
      else if(pt.Y>=Height) ScrollDown();
    }
  }

  void ScrollDown()
  { int bi = GetBottomIndex();
    if(SelectedIndex==bi)
    { if(bi==-1) SelectedIndex=TopIndex;
      else if(bi<Items.Count-1)
      { TopIndex++;
        SelectedIndex=bottom=bi+1;
      }
    }
    else ScrollTo(++SelectedIndex);
  }

  void ScrollTo(int index)
  { if(index<TopIndex) TopIndex = index;
    else if(index>GetBottomIndex()) TopIndex = BottomToTop(index);
  }

  void ScrollUp()
  { if(SelectedIndex==TopIndex)
    { if(TopIndex>0) SelectedIndex = --TopIndex;
    }
    else ScrollTo(--SelectedIndex);
  }

  void StartScrolling()
  { if(!scrolling)
    { scrolling=true;
      if(scroll==null) scroll = new ScrollEvent(this);
      staticScroll = scroll;
      DesktopControl desktop = Desktop;
      scrollTimer.Change(25, 25);
    }
  }
  
  void StopScrolling()
  { if(scrolling)
    { scrollTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
      scrolling=false;
    }
  }

  ScrollEvent scroll;
  int   selected, top, bottom, lastTop;
  Color selBack,  selFore;
  bool  down;

  static void ScrollIt(object dummy) { Events.Events.PushEvent(staticScroll); }

  static ScrollEvent staticScroll;
  static System.Threading.Timer scrollTimer =
    new System.Threading.Timer(new System.Threading.TimerCallback(ScrollIt), null,
                               System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
  static bool scrolling;
}
#endregion

#region ListBox
public class ListBox : ListBoxBase
{ public ListBox() { Init(); }
  public ListBox(ICollection items) : base(items) { Init(); }
  public ListBox(IEnumerable items) : base(items) { Init(); }
  public ListBox(params object[] items) : base((ICollection)items) { Init(); }
  void Init() { BackColor=Color.White; BorderStyle=BorderStyle.FixedFlat; }

  public override Color BorderColor
  { get { return RawBorderColor==Color.Transparent ? ForeColor : RawBorderColor; }
  }

  protected override void DrawItem(int index, PaintEventArgs e, Rectangle bounds)
  { Color fore, back;

    if(IsSelected(index))
    { back=SelectedBackColor; fore=SelectedForeColor;
      if(back!=Color.Transparent) e.Surface.Fill(bounds, back);
    }
    else { back=BackColor; fore=ForeColor; }
    if(!Enabled) fore = SystemColors.GrayText;

    GameLib.Fonts.Font font = Font;
    if(font!=null)
    { font.Color     = fore;
      font.BackColor = back;
      font.Render(e.Surface, GetItemText(index), bounds.Location);
    }
  }

  protected override Size MeasureItem(int index)
  { GameLib.Fonts.Font font = Font;
    return font==null ? new Size(Width, 14) : font.CalculateSize(GetItemText(index)); // guess 14 for font size
  }
}
#endregion
#endregion

#region Menus
// TODO: allow the hotkey letter to be underlined
#region MenuItemBase
public abstract class MenuItemBase : Control
{ public MenuItemBase() { BackColor = SystemColors.Menu; ForeColor = SystemColors.MenuText; }

  public bool AllowKeyRepeat { get { return keyRepeat; } set { keyRepeat=value; } }
  public KeyCombo GlobalHotKey { get { return globalHotKey; } set { globalHotKey=value; } }
  public char HotKey { get { return hotKey; } set { hotKey=char.ToUpper(value); } }
  public MenuBase Menu { get { return (MenuBase)Parent; } }

  public event EventHandler Click;

  public virtual Size MeasureItem() { return Size; }

  protected internal virtual void OnClick(EventArgs e) { if(Click!=null) Click(this, e); }

  KeyCombo globalHotKey;
  char hotKey;
  bool keyRepeat;
}
#endregion

#region MenuBase
// TODO: add arrow key support
public abstract class MenuBase : ContainerControl
{ public MenuBase()
  { Style |= ControlStyle.Clickable|ControlStyle.CanFocus|ControlStyle.DontLayout;
    BackColor = SystemColors.Menu;
    ForeColor = SystemColors.MenuText;
  }

  public KeyCombo GlobalHotKey { get { return globalHotKey; } set { globalHotKey=value; } }

  public bool IsOpen { get { return source!=null; } }

  public bool PullDown
  { get { return pullDown; }
    set
    { if(value!=pullDown)
      { pullDown=value;
        if(!pullDown) Capture=true;
      }
    }
  }

  public Control SourceControl { get { return source; } }

  public event EventHandler Popup;

  public MenuItemBase Add(MenuItemBase item) { Controls.Add(item); return item; }
  public void Clear() { Controls.Clear(); }

  public void Close()
  { if(source!=null)
    { MenuBarBase bar = source as MenuBarBase;
      if(bar!=null) bar.OnMenuClosed(this);
      Parent=null;
      source=null;
    }
  }

  public void Show(Control source, Point position) { Show(source, position, false, true); }
  public void Show(Control source, Point position, bool pullDown) { Show(source, position, pullDown, true); }
  public void Show(Control source, Point position, bool pullDown, bool wait)
  { if(source==null) throw new ArgumentNullException("source");
    DesktopControl desktop = source.Desktop;
    if(desktop==null) throw new InvalidOperationException("The source control is not part of a desktop");
    this.source = source;
    this.pullDown = pullDown;
    Visible = false;
    Parent  = desktop;
    Bounds  = new Rectangle(source.WindowToAncestor(position, desktop), Size);
    OnPopup(new EventArgs());
    BringToFront();
    Visible = true;
    SetModal(true);
    Capture = true;
    if(wait) while(Events.Events.PumpEvent() && this.source!=null);
  }

  protected virtual void OnPopup(EventArgs e) { if(Popup!=null) Popup(this, e); }

  protected internal override void OnKeyPress(KeyEventArgs e)
  { if(!e.Handled)
    { char c = char.ToUpper(e.KE.Char);
      foreach(MenuItemBase item in Controls)
        if(c==item.HotKey && item.Enabled)
        { PostClickEvent(item);
          Close();
          e.Handled=true;
          break;
        }
    }
    base.OnKeyPress(e);
  }

  protected internal override void OnKeyDown(KeyEventArgs e)
  { if(!e.Handled && e.KE.Down && e.KE.Key==Key.Escape && e.KE.KeyMods==KeyMod.None)
    { Close();
      e.Handled=true;
    }
  }

  protected internal override void OnMouseUp(ClickEventArgs e)
  { if(!e.Handled && pullDown)
    { if(!WindowRect.Contains(e.CE.Point)) Close();
      else
      { pullDown = false;
        foreach(MenuItemBase item in Controls)
          if(item.Bounds.Contains(e.CE.Point))
          { if(item.Enabled) PostClickEvent(item);
            Close();
            goto done;
          }
        Capture = true;
      }
      done:
      e.Handled = true;
    }
    base.OnMouseUp(e);
  }

  protected internal override void OnMouseClick(ClickEventArgs e)
  { if(!e.Handled)
    { foreach(MenuItemBase item in Controls)
        if(item.Bounds.Contains(e.CE.Point))
        { if(item.Enabled) { PostClickEvent(item); break; }
          else goto done;
        }
      Close();
      e.Handled=true;
    }
    done: base.OnMouseClick(e);
  }

  protected internal override void OnCustomEvent(Events.WindowEvent e)
  { if(e is ItemClickedEvent) Click(((ItemClickedEvent)e).Item);
    base.OnCustomEvent(e);
  }

  protected override void OnTextChanged(ValueChangedEventArgs e)
  { MenuBarBase bar = Parent as MenuBarBase;
    if(bar!=null) bar.Relayout();
    base.OnTextChanged(e);
  }

  internal protected void PostClickEvent(MenuItemBase item)
  { if(Events.Events.Initialized) Events.Events.PushEvent(new ItemClickedEvent(this, item));
  }

  class ItemClickedEvent : Events.WindowEvent
  { public ItemClickedEvent(Control menu, MenuItemBase item) : base(menu) { Item=item; }
    public MenuItemBase Item;
  }

  void Click(MenuItemBase item)
  { item.OnClick(new EventArgs());
    Close();
  }

  Control  source;
  KeyCombo globalHotKey;
  bool     pullDown;
}
#endregion

#region MenuBarBase
public abstract class MenuBarBase : Control
{ public MenuBarBase()
  { Style |= ControlStyle.Clickable;
    BackColor = SystemColors.Menu;
    ForeColor = SystemColors.MenuText;
    menus=new MenuCollection(this);
  }

  #region MenuCollection
  /// <summary>This class provides a strongly-typed collection of <see cref="Control"/> objects.</summary>
  public class MenuCollection : CollectionBase
  { internal MenuCollection(MenuBarBase parent) { this.parent=parent; }

    public MenuBase this[int index] { get { return (MenuBase)List[index]; } }

    public int Add(MenuBase menu) { return List.Add(menu); }
    public int IndexOf(MenuBase menu) { return List.IndexOf(menu); }
    public MenuBase Find(string text)
    { foreach(MenuBase menu in List) if(menu.Text==text) return menu;
      return null;
    }

    public void Insert(int index, MenuBase menu) { List.Insert(index, menu); }
    public void Remove(MenuBase menu) { List.Remove(menu); }

    protected override void OnClearComplete()
    { parent.Relayout();
      base.OnClearComplete();
    }
    protected override void OnInsert(int index, object value)
    { if(!(value is MenuBase))
        throw new ArgumentException("Only classed derived from MenuBase are allowed in this collection");
      base.OnInsert(index, value);
    }
    protected override void OnInsertComplete(int index, object value)
    { parent.Relayout();
      base.OnInsertComplete(index, value);
    }
    protected override void OnRemoveComplete(int index, object value)
    { parent.Relayout();
      base.OnRemoveComplete(index, value);
    }
    protected override void OnSet(int index, object oldValue, object newValue)
    { if(!(newValue is MenuBase))
        throw new ArgumentException("Only classed derived from MenuBase are allowed in this collection");
      base.OnSet(index, oldValue, newValue);
    }
    protected override void OnSetComplete(int index, object oldValue, object newValue)
    { parent.Relayout();
      base.OnSetComplete(index, oldValue, newValue);
    }

    MenuBarBase parent;
  }
  #endregion

  public MenuCollection Menus { get { return menus; } }

  public int Spacing
  { get { return spacing; }
    set
    { if(value<0) throw new ArgumentOutOfRangeException("spacing", value, "must be >=0");
      spacing=value;
      Relayout();
    }
  }

  public MenuBase Add(MenuBase item) { Menus.Add(item); return item; }

  public bool HandleKey(Events.KeyboardEvent e)
  { for(int i=0; i<buttons.Length; i++) if(buttons[i].Menu.GlobalHotKey.Matches(e))
    { Desktop.StopKeyRepeat();
      Open(i, false);
      return true;
    }
    foreach(MenuBase menu in menus)
      if(menu.Enabled)
        foreach(MenuItemBase item in menu.Controls)
          if(item.Enabled && item.GlobalHotKey.Matches(e))
          { if(!item.AllowKeyRepeat) Desktop.StopKeyRepeat();
            menu.PostClickEvent(item);
            return true;
          }
    return false;
  }

  protected internal override void OnMouseDown(ClickEventArgs e)
  { if(e.CE.Button==MouseButton.Left)
    { for(int i=0; i<buttons.Length; i++)
      { if(buttons[i].Area.Contains(e.CE.Point) && buttons[i].Menu.Enabled) Open(i);
        else if(buttons[i].State!=ButtonState.Normal) Close(i);
      }
      e.Handled=true;
    }
    base.OnMouseDown(e);
  }

  protected internal override void OnMouseUp(ClickEventArgs e)
  { if(e.CE.Button==MouseButton.Left)
    { int open=-1;
      for(int i=0; i<buttons.Length; i++)
      { if(buttons[i].State==ButtonState.Open)
        { open=i;
          if(buttons[i].Area.Contains(e.CE.Point))
          { buttons[i].Menu.PullDown = false;
            goto done;
          }
        }
      }
      if(open!=-1)
      { DesktopControl desktop = Desktop;
        e.CE.Point = desktop.WindowToChild(WindowToAncestor(e.CE.Point, desktop), buttons[open].Menu);
        buttons[open].Menu.OnMouseUp(e);
      }
      done:
      e.Handled=true;
    }
    base.OnMouseDown(e);
  }

  protected internal override void OnMouseMove(Events.MouseMoveEvent e)
  { int over=-1, oldOver=-1;
    for(int i=0; i<buttons.Length; i++)
    { if(buttons[i].Over || buttons[i].State!=ButtonState.Normal) oldOver=i;
      if(buttons[i].Area.Contains(e.Point))
      { if(!buttons[i].Menu.Enabled) goto done;
        buttons[i].Over = true;
        over=i;
      }
      else buttons[i].Over = false;
    }

    if(over!=oldOver)
    { ButtonState oldState=ButtonState.Over;
      if(oldOver==-1) oldState=ButtonState.Over;
      else if(over!=-1 || buttons[oldOver].State==ButtonState.Over)
      { oldState=buttons[oldOver].State;
        buttons[oldOver].State = ButtonState.Normal;
        buttons[oldOver].Menu.Close();
        Invalidate(buttons[oldOver].Area);
      }
      if(over!=-1)
      { buttons[over].State = oldState;
        if(oldState==ButtonState.Open) Open(over);
        Invalidate(buttons[over].Area);
      }
    }
    done: base.OnMouseMove(e);
  }

  protected internal override void OnMouseLeave(EventArgs e)
  { int oldOver=-1;
    for(int i=0; i<buttons.Length; i++)
      if(buttons[i].Over || buttons[i].State!=ButtonState.Normal) { oldOver=i; break; }
    if(oldOver!=-1)
    { buttons[oldOver].Over=false;
      if(buttons[oldOver].State==ButtonState.Over)
      { buttons[oldOver].State = ButtonState.Normal;
        Invalidate(buttons[oldOver].Area);
      }
    }    
    base.OnMouseLeave(e);
  }

  protected override void OnParentChanged(ValueChangedEventArgs e) { Relayout(); base.OnParentChanged(e); }
  protected override void OnFontChanged(ValueChangedEventArgs e) { Relayout(); base.OnFontChanged(e); }
  protected override void OnResize(EventArgs e) { Relayout(); base.OnResize(e); }

  protected abstract Size MeasureItem(MenuBase menu);

  protected enum ButtonState { Normal, Over, Open };
  protected struct MenuButton
  { public MenuButton(MenuBase menu, Rectangle area) { Menu=menu; Area=area; State=ButtonState.Normal; Over=false; }
    public MenuBase  Menu;
    public Rectangle Area;
    public ButtonState State;
    public bool Over;
  }
  protected MenuButton[] Buttons { get { return buttons; } }

  protected internal void Relayout()
  { GameLib.Fonts.Font font = Font;
    if(font==null) { buttons = new MenuButton[0]; return; }

    buttons = new MenuButton[menus.Count];
    for(int i=0,x=spacing; i<menus.Count; i++)
    { MenuBase menu = (MenuBase)menus[i];
      Size size = MeasureItem(menu);
      buttons[i] = new MenuButton(menu, new Rectangle(x, (Height-size.Height)/2, size.Width, size.Height));
      x += size.Width+spacing;
    }
    Invalidate();
  }

  internal void OnMenuClosed(MenuBase menu)
  { for(int i=0; i<buttons.Length; i++)
      if(buttons[i].Menu==menu && buttons[i].State!=ButtonState.Normal) { Close(i); break; }
  }

  void Open(int menu) { Open(menu, true); }
  void Open(int menu, bool pullDown)
  { buttons[menu].State = ButtonState.Open;
    buttons[menu].Menu.Show(this, new Point(buttons[menu].Area.X, buttons[menu].Area.Bottom), pullDown, false);
    Capture = pullDown;
    Invalidate(buttons[menu].Area);
  }

  void Close(int menu)
  { buttons[menu].State = ButtonState.Normal;
    buttons[menu].Menu.Close();
    Invalidate(buttons[menu].Area);
    Capture = false;
  }

  MenuCollection menus;
  MenuButton[] buttons;
  int spacing=1;
}
#endregion

#region MenuItem
public class MenuItem : MenuItemBase
{ public MenuItem() { Init(); }
  public MenuItem(string text) { Init(); Text=text; }
  public MenuItem(string text, char hotKey) { Init(); Text=text; HotKey=hotKey; }
  public MenuItem(string text, char hotKey, KeyCombo globalHotKey)
  { Init(); Text=text; HotKey=hotKey; GlobalHotKey=globalHotKey; Padding=new RectOffset(2, 0);
  }
  void Init() { DontDraw|=DontDraw.BackImage; }

  public Color RawSelectedBackColor { get { return selBack; } }
  public Color RawSelectedForeColor { get { return selFore; } }

  public Color SelectedBackColor
  { get
    { if(selBack!=Color.Transparent) return selBack;
      Menu menu = (Menu)Parent;
      return menu==null || menu.SelectedBackColor==Color.Transparent ? ForeColor : menu.SelectedBackColor;
    }
    set { selBack=value; if(mouseOver) Invalidate(); }
  }

  public Color SelectedForeColor
  { get
    { if(selFore!=Color.Transparent) return selFore;
      Menu menu = (Menu)Parent;
      return menu==null || menu.SelectedForeColor==Color.Transparent ? BackColor : menu.SelectedForeColor;
    }
    set { selFore=value; if(mouseOver) Invalidate(); }
  }

  protected internal override void OnMouseEnter(EventArgs e) { mouseOver=true;  Invalidate(); base.OnMouseEnter(e); }
  protected internal override void OnMouseLeave(EventArgs e) { mouseOver=false; Invalidate(); base.OnMouseLeave(e); }

  // FIXME: if BackImage is used, this will overwrite it
  protected internal override void OnPaintBackground(PaintEventArgs e)
  { base.OnPaintBackground(e);
    if(mouseOver && Enabled) e.Surface.Fill(e.DisplayRect, SelectedBackColor);
    else if(RawBackColor!=Color.Transparent) e.Surface.Fill(e.DisplayRect, RawBackColor);
  }

  protected internal override void OnPaint(PaintEventArgs e)
  { base.OnPaint(e);
    if(Text.Length>0)
    { GameLib.Fonts.Font f = Font;
      if(f != null)
      { Rectangle rect = ContentDrawRect;
        if(Enabled)
        { f.Color     = mouseOver ? SelectedForeColor : ForeColor;
          f.BackColor = mouseOver ? SelectedBackColor : BackColor;
        }
        else { f.Color=SystemColors.GrayText; f.BackColor=BackColor; }

        f.Render(e.Surface, Text, rect, ContentAlignment.MiddleLeft);
        if(GlobalHotKey.Valid) f.Render(e.Surface, GlobalHotKey.ToString(), rect, ContentAlignment.MiddleRight);
      }
    }
  }

  public override Size MeasureItem()
  { GameLib.Fonts.Font f = Font;
    if(f==null) return base.MeasureItem();
    Size size = f.CalculateSize(Text);
    if(GlobalHotKey.Valid)
    { Size hotkey = f.CalculateSize(GlobalHotKey.ToString());
      size.Width += hotkey.Width + hotKeyPadding;
      if(hotkey.Height>size.Height) size.Height=hotkey.Height;
    }
    size.Width += horzPadding*2; size.Height += vertPadding*2;
    return size;
  }

  const int hotKeyPadding=20;
  int horzPadding=2, vertPadding=3;
  Color selFore=SystemColors.HighlightText, selBack=SystemColors.Highlight;
  bool mouseOver;
}
#endregion

#region Menu
// TODO: make sure this handles font/text changes, etc
// TODO: handle menus with lots of items (scrolling?)
public class Menu : MenuBase
{ public Menu() { BorderStyle=BorderStyle.FixedThick; }
  public Menu(string text) : this() { Text=text; }
  public Menu(string text, KeyCombo globalHotKey) : this() { Text=text; GlobalHotKey=globalHotKey; }

  public Color SelectedBackColor { get { return selBack; } set { selBack=value; } }
  public Color SelectedForeColor { get { return selFore; } set { selFore=value; } }

  protected override void OnPopup(EventArgs e)
  { base.OnPopup(e);

    int width=0, height=0;
    foreach(MenuItemBase item in Controls)
    { Size size = item.MeasureItem();
      if(size.Width>width) { width=size.Width; }
      else size.Width = width;
      item.Size = size;
      item.Location = new Point(2, height+2);
      height += item.Height;
    }
    foreach(MenuItemBase item in Controls) item.Width=width;
    Size = new Size(width+4, height+4);
  }

  Color selFore=Color.Transparent, selBack=Color.Transparent;
}
#endregion

#region MenuBar
public class MenuBar : MenuBarBase
{ public int HorizontalPadding
  { get { return horzPadding; }
    set
    { if(value<1) throw new ArgumentOutOfRangeException("HorizontalPadding", value, "must be >=1");
      horzPadding=value;
      Relayout();
    }
  }

  public int VerticalPadding
  { get { return vertPadding; }
    set
    { if(value<1) throw new ArgumentOutOfRangeException("VerticalPadding", value, "must be >=1");
      vertPadding=value;
      Relayout();
    }
  }

  protected internal override void OnPaintBackground(PaintEventArgs e)
  { base.OnPaintBackground(e);
    foreach(MenuButton button in Buttons)
    { if(button.Area.IntersectsWith(e.WindowRect))
      { if(button.State!=ButtonState.Normal) // TODO: make this border style/color configurable
          Helpers.DrawBorder(e.Surface, WindowToDisplay(button.Area), BorderStyle.Fixed3D, BackColor, false);
      }
    }
  }

  protected internal override void OnPaint(PaintEventArgs e)
  { base.OnPaint(e);
    GameLib.Fonts.Font font = Font;
    if(font==null) return;

    font.BackColor = BackColor;
    foreach(MenuButton button in Buttons)
      if(button.Area.IntersectsWith(e.WindowRect))
      { font.Color = button.Menu.Enabled ? ForeColor : SystemColors.GrayText;
        font.Render(e.Surface, button.Menu.Text, WindowToDisplay(button.Area), ContentAlignment.MiddleCenter);
      }
  }

  protected override Size MeasureItem(MenuBase menu)
  { Size size = Font.CalculateSize(menu.Text);
    size.Width += horzPadding*2; size.Height += vertPadding*2;
    return size;
  }

  int horzPadding=6, vertPadding=2;
}
#endregion
#endregion

#region Forms
#region FormBase
// TODO: make TAB start tabbing through child controls
public enum ButtonClicked { None, Abort, Cancel, Ignore, No, Ok, Retry, Yes }
public abstract class FormBase : ContainerControl
{ public FormBase()
  { Style |= ControlStyle.CanFocus|ControlStyle.Draggable;
    ForeColor=SystemColors.ControlText; BackColor=SystemColors.Control;
    DragThreshold=3;
  }

  #region TitleBarBase
  public abstract class TitleBarBase : ContainerControl
  { public TitleBarBase(FormBase parent)
    { Style    |= ControlStyle.Draggable|ControlStyle.DontLayout;
      BackColor = SystemColors.InactiveCaption;
      ForeColor = SystemColors.InactiveCaptionText;
      parent.Controls.Add(this);
    }

    public abstract bool CloseBox { get; set; }

    protected internal override void OnDragStart(DragEventArgs e)
    { if(!e.OnlyPressed(MouseButton.Left)) e.Cancel=true;
      base.OnDragStart(e);
    }

    protected internal override void OnDragMove(DragEventArgs e)
    { DragParent(e);
      base.OnDragMove(e);
    }

    protected internal override void OnDragEnd(DragEventArgs e)
    { DragParent(e);
      base.OnDragEnd(e);
    }

    protected override void OnResize(EventArgs e)
    { FormBase parent = (FormBase)Parent;
      if(parent.MinimumHeight<Height) parent.MinimumHeight=Height;
      base.OnResize(e);
      parent.OnContentSizeChanged(e);
    }

    void DragParent(DragEventArgs e)
    { Point location = Parent.Location;
      location.Offset(e.End.X-e.Start.X, e.End.Y-e.Start.Y);
      Parent.Location = location;
    }
  }
  #endregion

  public ButtonClicked Button { get { return button; } set { button=value; }  }

  public override RectOffset ContentOffset
  { get
    { RectOffset ret = base.ContentOffset;
      ret.Top += TitleBar.Height;
      return ret;
    }
  }

  public int MinimumHeight
  { get { return min.Height; }
    set
    { if(value<0) throw new ArgumentOutOfRangeException("MinimumHeight", value, "must not be negative");
      min.Height = value;
      if(Height<value) Height = value;
    }
  }

  public Size MinimumSize
  { get { return min; }
    set
    { if(value.Width<0 || value.Height<0)
        throw new ArgumentOutOfRangeException("MinimumSize", value, "must not be negative");
      min = value;
      if(Width<value.Width || Height<value.Height)
        Size = new Size(Math.Max(Width, value.Width), Math.Max(Height, value.Height));
    }
  }

  public int MinimumWidth
  { get { return min.Width; }
    set
    { if(value<0) throw new ArgumentOutOfRangeException("MinimumWidth", value, "must not be negative");
      min.Width = value;
      if(Width<value) Width = value;
    }
  }

  public int MaximumHeight
  { get { return max.Height; }
    set
    { if(value<-1) throw new ArgumentOutOfRangeException("MaximumHeight", value, "must be greater than or equal to -1");
      max.Height = value;
      if(value!=-1 && Height>value) Height = value;
    }
  }

  public Size MaximumSize
  { get { return max; }
    set
    { if(value.Width<-1 || value.Height<-1)
        throw new ArgumentOutOfRangeException("MaximumSize", value, "must be greater than or equal to -1");
      max = value;
      if(value.Width!=-1 && Width>value.Width || value.Height!=-1 && Height>value.Height)
        Size = new Size(value.Width ==-1 ? Width  : Math.Min(Width, value.Width),
                        value.Height==-1 ? Height : Math.Min(Height, value.Height));
    }
  }

  public int MaximumWidth
  { get { return max.Width; }
    set
    { if(value<-1) throw new ArgumentOutOfRangeException("MaximumWidth", value, "must be greater than or equal to -1");
      max.Width = value;
      if(value!=-1 && Width>value) Width = value;
    }
  }

  public abstract TitleBarBase TitleBar { get; }

  public object DialogResult { get { return returnValue; } set { returnValue=value; } }

  public void Close()
  { if(Parent==null) return;
    System.ComponentModel.CancelEventArgs e = new System.ComponentModel.CancelEventArgs();
    OnClosing(e);
    if(!e.Cancel)
    { OnClosed(new EventArgs());
      Parent = null;
    }
  }

  public object ShowDialog(DesktopControl desktop)
  { if(desktop==null) throw new ArgumentNullException("desktop");
    Visible = true;
    Parent  = desktop;
    Focus();
    SetModal(true);
    while(Events.Events.PumpEvent() && Parent!=null);
    return returnValue;
  }

  public event System.ComponentModel.CancelEventHandler Closing;
  public event EventHandler Closed;

  protected override void OnGotFocus(EventArgs e)
  { BringToFront();
    base.OnGotFocus(e);
  }

  protected internal override void OnDragStart(DragEventArgs e)
  { if(BorderStyle==BorderStyle.Resizeable && e.OnlyPressed(MouseButton.Left))
    { int bwidth = BorderWidth;
      drag = DragEdge.None;

      if(e.Start.X>=Width-bwidth) drag |= DragEdge.Right;
      else if(e.Start.X<bwidth) drag |= DragEdge.Left;
      if(drag!=DragEdge.None)
      { if(e.Start.Y<16) drag |= DragEdge.Top;
        else if(e.Start.Y>=Height-16) drag |= DragEdge.Bottom;
      }
      if(e.Start.Y<bwidth || e.Start.Y>=Height-bwidth)
      { if(e.Start.X<16) drag |= DragEdge.Left;
        else if(e.Start.X>=Width-16) drag |= DragEdge.Right;
        if(e.Start.Y>=Height-bwidth) drag |= DragEdge.Bottom;
        else drag |= DragEdge.Top;
      }
      e.Cancel = drag==DragEdge.None;
    }
    else e.Cancel=true;
    base.OnDragStart(e);
  }

  protected internal override void OnDragMove(DragEventArgs e)
  { DragResize(e);
    base.OnDragMove(e);
  }

  protected internal override void OnDragEnd(DragEventArgs e)
  { DragResize(e);
    base.OnDragEnd(e);
  }

  protected virtual void OnClosing(System.ComponentModel.CancelEventArgs e)
  { if(Closing!=null) Closing(this, e);
  }
  protected virtual void OnClosed(EventArgs e) { if(Closed!=null) Closed(this, e); }

  [Flags] enum DragEdge { None=0, Left=1, Right=2, Top=4, Bottom=8 };

  void DragResize(DragEventArgs e)
  { int xd=e.End.X-e.Start.X, yd=e.End.Y-e.Start.Y;
    Rectangle bounds = Bounds;
    if((drag&DragEdge.Right)!=0) { bounds.Width += xd; e.Start.X=e.End.X; }
    else if((drag&DragEdge.Left)!=0) { bounds.X += xd; bounds.Width -= xd; }
    if((drag&DragEdge.Bottom)!=0) { bounds.Height += yd; e.Start.Y=e.End.Y; }
    else if((drag&DragEdge.Top)!=0) { bounds.Y += yd; bounds.Height -= yd; }

    if(bounds.Width<min.Width)
    { if((drag&DragEdge.Left)!=0) bounds.X += bounds.Width-min.Width;
      else e.Start.X -= bounds.Width-min.Width;
      bounds.Width=min.Width;
    }
    else if(max.Width!=-1 && bounds.Width>max.Width)
    { if((drag&DragEdge.Left)!=0) bounds.X += bounds.Width-max.Width;
      else e.Start.X -= bounds.Width-max.Width;
      bounds.Width=max.Width;
    }

    if(bounds.Height<min.Height)
    { if((drag&DragEdge.Top)!=0) bounds.Y += bounds.Height-min.Height;
      else e.Start.Y -= bounds.Height-min.Height;
      bounds.Height=min.Height;
    }
    else if(max.Height!=-1 && bounds.Height>max.Height)
    { if((drag&DragEdge.Top)!=0) bounds.Y += bounds.Height-max.Height;
      else e.Start.Y -= bounds.Height-max.Height;
      bounds.Height=max.Height;
    }

    if(Bounds!=bounds) { TriggerLayout(true); Bounds=bounds; }
  }
  
  protected internal override void OnLayout(LayoutEventArgs e)
  { Rectangle rect = PaddingRect;
    rect.Height = TitleBar.Height;
    TitleBar.SetBounds(rect, true);
    base.OnLayout(e);
  }

  object returnValue;
  Size          min = new Size(100, 24), max = new Size(-1, -1);
  ButtonClicked button;
  DragEdge      drag;
}
#endregion

#region Form
// TODO: add menu bar?
// TODO: rewrite the titlebar stuff so it doesn't use docking
public class Form : FormBase
{ public Form() { titleBar=new OurTitleBar(this); BorderStyle=BorderStyle.FixedThick; KeyPreview=true; }

  #region OurTitleBar
  class OurTitleBar : TitleBarBase
  { public OurTitleBar(FormBase parent) : base(parent)
    { GameLib.Fonts.Font font = parent.Font;
      Height = parent.Font==null ? 24 : font.LineSkip*4/3;
      CloseBox = true;
    }

    public override bool CloseBox
    { get { return close!=null; }
      set
      { if(value!=CloseBox)
        { if(value)
          { close = new CloseButton();
            close.Anchor = AnchorStyle.TopBottom | AnchorStyle.Right;
            close.Bounds = new Rectangle(Right-Height+2, 2, Height-4, Height-4);
            Controls.Add(close);
          }
          else
          { Controls.Remove(close);
            close = null;
          }
        }
      }
    }

    protected internal override void OnPaint(PaintEventArgs e)
    { base.OnPaint(e);
      GameLib.Fonts.Font font = Font;
      if(font!=null)
      { Rectangle rect = DrawRect;
        rect.X += (font.LineSkip+3)/6;
        font.Color = ForeColor;
        font.BackColor = BackColor;
        font.Render(e.Surface, Parent.Text, rect, ContentAlignment.MiddleLeft);
      }
    }

    protected override void OnFontChanged(ValueChangedEventArgs e)
    { UpdateSize();
      Invalidate();
      base.OnFontChanged(e);
    }

    protected override void OnVisibleChanged(ValueChangedEventArgs e)
    { UpdateSize();
      base.OnVisibleChanged(e);
    }

    void UpdateSize()
    { if(!Visible) Height=0;
      else
      { GameLib.Fonts.Font font = Font;
        if(font!=null) Height = font.LineSkip*4/3;
      }
    }

    #region CloseButton
    class CloseButton : Button
    { public CloseButton() { BackColor=SystemColors.Control; ForeColor=SystemColors.ControlText; }

      protected override void OnResize(EventArgs e)
      { Rectangle rect = Bounds;
        int xd = Width-Height;
        if(xd!=0) { rect.X += xd; rect.Width -= xd; Bounds = rect; } // if it's not square, make it square
        base.OnResize(e);
      }

      protected override void OnClick(ClickEventArgs e)
      { ((FormBase)Parent.Parent).Close();
        base.OnClick(e);
      }

      protected internal override void OnPaint(PaintEventArgs e)
      { Rectangle rect = ContentDrawRect;
        float size = (float)rect.Width*3/5;
        int pad = (int)Math.Round((rect.Width-size)/2), isize = (int)Math.Round(size);
        rect.X += pad; rect.Width  -= isize-(rect.Width&1)-1;
        rect.Y += pad; rect.Height -= isize-(rect.Height&1);
        if(Pressed && Over) { rect.X++; rect.Y++; }
        int right=rect.Right-1, bottom=rect.Bottom-1;
        Color c = Enabled ? ForeColor : SystemColors.GrayText;

        Primitives.Line(e.Surface, rect.X, rect.Y, right-1, bottom, c);
        Primitives.Line(e.Surface, rect.X+1, rect.Y, right, bottom, c);
        Primitives.Line(e.Surface, rect.X, bottom, right-1, rect.Y, c);
        Primitives.Line(e.Surface, rect.X+1, bottom, right, rect.Y, c);
        base.OnPaint(e);
      }
    }
    #endregion

    CloseButton close;
  }
  #endregion

  public override TitleBarBase TitleBar { get { return titleBar; } }

  protected override void OnGotFocus(EventArgs e)
  { base.OnGotFocus(e);
    BorderColor = SystemColors.ActiveBorder;
    TitleBar.ForeColor = SystemColors.ActiveCaptionText;
    TitleBar.BackColor = SystemColors.ActiveCaption;
  }
  
  protected override void OnLostFocus(EventArgs e)
  { base.OnLostFocus(e);
    BorderColor = SystemColors.InactiveBorder;
    TitleBar.ForeColor = SystemColors.InactiveCaptionText;
    TitleBar.BackColor = SystemColors.InactiveCaption;
  }
  
  protected internal override void OnKeyDown(KeyEventArgs e)
  { if(!e.Handled && e.KE.Key==Key.F4 && e.KE.HasOnlyKeys(KeyMod.Ctrl)) // close the window when Ctrl-F4 is pressed
    { Close();
      e.Handled=true;
    }
    base.OnKeyPress(e);
  }

  OurTitleBar titleBar;
}
#endregion

#region MessageBox
public enum MessageBoxButtons { Ok, OkCancel, YesNo, YesNoCancel }
public sealed class MessageBox : Form
{ internal MessageBox(string text, string[] buttons, int defaultButton)
  { if(defaultButton<0 || defaultButton>=buttons.Length)
      throw new ArgumentOutOfRangeException("defaultButton", defaultButton, "out of the range of 'buttons'");
    DialogResult=-1; message=text; buttonTexts=buttons; this.defaultButton=defaultButton;
    TitleBar.CloseBox = false;
  }

  string[] buttonTexts;
  string   message;
  int      defaultButton;

  public int Show(DesktopControl desktop)
  { Parent = desktop;
    if(!init)
    { GameLib.Fonts.Font font = Font;
      if(font!=null)
      { int sidePadding = font.LineSkip*2;

        Size deskSize = desktop.ContentSize;
        deskSize.Width -= sidePadding; // we don't want to use the whole space...

        int btnWidth=0, btnHeight=font.LineSkip*3/2+2, btnSpace=12;

        int[] btnWidths = new int[buttonTexts.Length];
        for(int i=0; i<buttonTexts.Length; i++)
        { btnWidths[i] = font.CalculateSize(buttonTexts[i]).Width;
          btnWidth += Math.Max(btnWidths[i]*3/2, 40); // add padding
        }
        
        #region Find button width
        // find a good width, try to degrade nicely if we can't get the width we want
        { int width = btnWidth + sidePadding + (btnWidths.Length-1)*btnSpace;

          if(width<=deskSize.Width) // it's good, accept it
          { for(int i=0; i<btnWidths.Length; i++) btnWidths[i] = Math.Max(btnWidths[i]*3/2, 40);
            goto foundSize;
          }
          // it doesn't fit, so try to reduce the spacing
          width = btnWidth + sidePadding + (btnWidths.Length-1)*(btnSpace/2);
          if(width<=deskSize.Width)
          { for(int i=0; i<btnWidths.Length; i++) btnWidths[i] = btnWidths[i]*3/2;
            btnSpace /= 2;
            goto foundSize;
          }

          // okay, we need to try harder. reduce the internal space
          btnWidth = 0;
          for(int i=0; i<btnWidths.Length; i++) btnWidth += btnWidths[i]*5/4;
          width = btnWidth + sidePadding + (btnWidths.Length-1)*btnSpace;
          if(width<=deskSize.Width)
          { for(int i=0; i<btnWidths.Length; i++) btnWidth += btnWidths[i]*5/4;
            goto foundSize;
          }
          
          // okay, use only minimal padding and space. this is our final attempt
          btnWidth = 0;
          for(int i=0; i<btnWidths.Length; i++) btnWidth += (btnWidths[i] += 4); // only 2 pixels of padding
          btnSpace = 4;
          width = btnWidth + sidePadding + (btnWidths.Length-1)*btnSpace;

          foundSize:
          btnWidth = width;
        }
        #endregion
        
        Label label  = new Label(message);
        label.Width  = Math.Max(btnWidth, Math.Min(deskSize.Width*3/4-sidePadding, font.CalculateSize(message).Width));
        label.Height = font.WordWrap(message, label.Width, int.MaxValue).Length * font.LineSkip;
        label.Location  = new Point(sidePadding/2, sidePadding/2);
        label.TextAlign = ContentAlignment.TopCenter;

        Size = ContentOffset.Grow(new Size(label.Width+sidePadding, label.Height+btnHeight+sidePadding+font.LineSkip));
        Location = new Point(Math.Max((deskSize.Width-Width)/2, 0), Math.Max((deskSize.Height-Height)/2, 0));

        int x=(Width-btnWidth+sidePadding)/2, y=label.Bottom+font.LineSkip;
        for(int i=0; i<btnWidths.Length; i++)
        { Button btn = new Button(buttonTexts[i]);
          btn.Bounds = new Rectangle(x, y, btnWidths[i], btnHeight);
          btn.Click += new ClickEventHandler(btn_OnClick);
          btn.Tag    = btn.TabIndex = i;
          Controls.Add(btn);
          if(i==defaultButton) btn.Focus();
          x += btn.Width+btnSpace;
        }
        Controls.Add(label);
        init = true;
      }
      else
      { int i=0;
        foreach(Control control in Controls) if(control is Button && i++==defaultButton) { control.Focus(); break; }
      }
    }
    return (int)ShowDialog(desktop);
  }

  protected internal override void OnKeyPress(KeyEventArgs e)
  { if(!e.Handled)
      foreach(Control c in Controls)
        if(c is ButtonBase)
        { ButtonBase button = (ButtonBase)c;
          if(char.ToUpper(button.Text[0])==char.ToUpper(e.KE.Char))
          { button.PerformClick();
            e.Handled = true;
            break;
          }
        }
    base.OnKeyPress(e);
  }

  private void btn_OnClick(object sender, ClickEventArgs e)
  { DialogResult = ((Control)sender).Tag;
    Close();
  }

  bool init;

  public static MessageBox Create(string caption, string text) { return Create(caption, text, MessageBoxButtons.Ok); }
  public static MessageBox Create(string caption, string text, MessageBoxButtons buttons)
  { return Create(caption, text, buttons, 0);
  }
  public static MessageBox Create(string caption, string text, MessageBoxButtons buttons, int defaultButton)
  { switch(buttons)
    { case MessageBoxButtons.Ok: return Create(caption, text, ok, defaultButton);
      case MessageBoxButtons.OkCancel: return Create(caption, text, okCancel, defaultButton);
      case MessageBoxButtons.YesNo: return Create(caption, text, yesNo, defaultButton);
      case MessageBoxButtons.YesNoCancel: return Create(caption, text, yesNoCancel, defaultButton);
      default: throw new ArgumentException("Unknown MessageBoxButtons value");
    }
  }
  public static MessageBox Create(string caption, string text, string[] buttonText)
  { return Create(caption, text, buttonText, 0);
  }
  public static MessageBox Create(string caption, string text, string[] buttonText, int defaultButton)
  { if(buttonText.Length==0) throw new ArgumentException("Can't create a MessageBox with no buttons!", "buttonText");
    MessageBox box = new MessageBox(text, buttonText, defaultButton);
    box.Text = caption;
    return box;
  }

  public static void Show(DesktopControl desktop, string caption, string text)
  { Create(caption, text).Show(desktop);
  }
  public static int Show(DesktopControl desktop, string caption, string text, MessageBoxButtons buttons)
  { return Create(caption, text, buttons, 0).Show(desktop);
  }
  public static int Show(DesktopControl desktop, string caption, string text,
                         MessageBoxButtons buttons, int defaultButton)
  { return Create(caption, text, buttons, defaultButton).Show(desktop);
  }
  public static int Show(DesktopControl desktop, string caption, string text, string[] buttonText)
  { return Create(caption, text, buttonText).Show(desktop);
  }
  public static int Show(DesktopControl desktop, string caption, string text, string[] buttonText, int defaultButton)
  { return Create(caption, text, buttonText, defaultButton).Show(desktop);
  }

  static string[] ok = new string[] { "Ok" };
  static string[] okCancel = new string[] { "Ok", "Cancel" };
  static string[] yesNo = new string[] { "Yes", "No" };
  static string[] yesNoCancel = new string[] { "Yes", "No", "Cancel" };
}
#endregion

#region ColorPicker
public class ColorPicker : Form
{ public ColorPicker() { throw new NotImplementedException("The ColorPicker form is not yet implemented."); }
}
#endregion
#endregion

} // namespace GameLib.Forms