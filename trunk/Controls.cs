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

// TODO: implement more controls (checkbox, listbox, dropdown)
// TODO: have controls display differently when disabled
// TODO: implement a menubar control
using System;
using System.Collections;
using System.Drawing;
using GameLib.Fonts;
using GameLib.Video;
using GameLib.Input;

namespace GameLib.Forms
{

#region ContainerControl
public class ContainerControl : Control
{ protected internal override void OnPaint(PaintEventArgs e)
  { foreach(Control c in Controls)
      if(c.Visible)
      { Rectangle paint = Rectangle.Intersect(c.ParentRect, e.WindowRect);
        if(paint.Width>0)
        { paint.X -= c.Left; paint.Y -= c.Top;
          c.AddInvalidRect(paint);
          c.Update();
        }
      }
    base.OnPaint(e);
  }
  
  protected internal override void OnLayout(EventArgs e)
  { base.OnLayout(e);
    foreach(Control c in Controls)
      if(c.Dock==DockStyle.None && c.Anchor!=AnchorStyle.TopLeft)
      { Rectangle bounds = c.Bounds;
        if((c.Anchor&AnchorStyle.LeftRight)==AnchorStyle.LeftRight) bounds.Width = Right-c.RightAnchorOffset-bounds.X;
        else if((c.Anchor&AnchorStyle.Right)!=0) bounds.X = Right-c.RightAnchorOffset-bounds.Width;
        if((c.Anchor&AnchorStyle.TopBottom)==AnchorStyle.TopBottom)
          bounds.Height = Bottom-c.BottomAnchorOffset-bounds.Y;
        else if((c.Anchor&AnchorStyle.Bottom)!=0) bounds.Y = Bottom-c.BottomAnchorOffset-bounds.Height;
        c.Bounds = bounds;
      }
  }
}
#endregion

#region Line
public class Line : Control
{ public bool TopToBottom
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
      { textAlign = value;
        Invalidate();
      }
    }
  }

  protected override void OnTextChanged(ValueChangedEventArgs e)
  { Invalidate();
    base.OnTextChanged(e);
  }

  IBlittable image;
  ContentAlignment imageAlign=ContentAlignment.TopLeft, textAlign=ContentAlignment.TopLeft;
}
#endregion

#region Label
public class Label : LabelBase
{ public Label() { }
  public Label(string text) { Text=text; }

  public int TextPadding
  { get { return padding; }
    set
    { if(value!=padding)
      { if(value<0) throw new ArgumentOutOfRangeException("TextPadding", value, "must be >=0");
        padding = value;
        Invalidate();
      }
    }
  }

  protected internal override void OnPaint(PaintEventArgs e)
  { base.OnPaint(e);
    if(Image!=null)
    { Point at = Helpers.CalculateAlignment(DisplayRect, new Size(Image.Width, Image.Height), ImageAlign);
      Image.Blit(e.Surface, at.X, at.Y);
    }
    if(Text.Length>0)
    { GameLib.Fonts.Font f = Font;
      if(f != null)
      { Rectangle rect = DisplayRect;
        rect.Inflate(-padding, -padding);
        f.Color     = ForeColor;
        f.BackColor = BackColor;
        f.Render(e.Surface, Text, rect, TextAlign);
      }
    }
  }

  int padding;
}
#endregion

#region ButtonBase
public abstract class ButtonBase : LabelBase
{ public ButtonBase() { Style|=ControlStyle.Clickable|ControlStyle.CanFocus; }

  public event ClickEventHandler Click;
  public event EventHandler PressedChanged;

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
  { Capture = Pressed = true;
    base.OnMouseDown(e);
  }

  protected internal override void OnMouseUp(ClickEventArgs e)
  { Capture = Pressed = false;
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

  bool pressed;
}
#endregion

#region Button
public class Button : ButtonBase
{ public Button() { ImageAlign=TextAlign=ContentAlignment.MiddleCenter; }
  public Button(string text) { ImageAlign=TextAlign=ContentAlignment.MiddleCenter; Text=text; }

  protected internal override void OnPaint(PaintEventArgs e)
  { base.OnPaint(e);

    Rectangle rect = DisplayRect;
    bool pressed = Pressed && over;

    if(Image!=null)
    { Point at = Helpers.CalculateAlignment(rect, new Size(Image.Width, Image.Height), ImageAlign);
      if(pressed) at.Offset(1, 1);
      Image.Blit(e.Surface, at.X, at.Y);
    }
    if(Text.Length>0)
    { GameLib.Fonts.Font f = Font;
      if(f!=null)
      { Rectangle box = rect;
        box.Inflate(-1, -1);
        if(pressed) box.Offset(1, 1);
        f.Color     = ForeColor;
        f.BackColor = BackColor;
        f.Render(e.Surface, Text, box, TextAlign);
      }
    }
    
    Color bright, dark, back=BackColor;
    bright = Color.FromArgb(back.R+(255-back.R)*3/5, back.G+(255-back.G)*3/5, back.B+(255-back.B)*3/5);
    dark   = Color.FromArgb(back.R/2, back.G/2, back.B/2);
    if(pressed) { Color t=bright; bright=dark; dark=t; }
    else if(Selected) bright=dark=Color.Black;
    Primitives.Line(e.Surface, rect.X, rect.Y, rect.Right-1, rect.Y, bright);
    Primitives.Line(e.Surface, rect.X, rect.Y, rect.X, rect.Bottom-1, bright);
    Primitives.Line(e.Surface, rect.X, rect.Bottom-1, rect.Right-1, rect.Bottom-1, dark);
    Primitives.Line(e.Surface, rect.Right-1, rect.Y, rect.Right-1, rect.Bottom-1, dark);
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
  
  bool over;
}
#endregion

#region CheckBoxBase
public abstract class CheckBoxBase : ButtonBase
{ public bool Checked
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
{ public CheckBox() { }
  public CheckBox(bool check) { Checked=check; } 

  public ContentAlignment TextAlignment
  { get { return align; }
    set
    { if(value!=align)
      { if(Helpers.AlignedMiddle(value)) throw new ArgumentException("Middle alignment not allowed for checkbox");
        align=value;
        Invalidate();
      }
    }
  }

  protected internal override void OnPaintBackground(PaintEventArgs e)
  { base.OnPaintBackground(e);
    if(Focused) Helpers.DrawBorder(e.Surface, DisplayRect, BorderStyle.FixedFlat, Color.Black, false);
  }

  protected internal override void OnPaint(PaintEventArgs e)
  { base.OnPaint(e);
    
    Rectangle rect = DisplayRect;
    rect.Inflate(-1, -1);

    bool right = Helpers.AlignedRight(align);
    int boxSize=13, x=right ? rect.X : rect.Right-boxSize, y=rect.Y+(Height-boxSize)/2;
    Rectangle box = new Rectangle(x, y, boxSize, boxSize);

    Helpers.DrawBorder(e.Surface, box, BorderStyle.FixedThick, border, true);
    box.Inflate(-2, -2);
    e.Surface.Fill(box, down ? border : Color.White);

    if(Checked)
      for(int yo=0; yo<3; yo++)
      { Primitives.Line(e.Surface, box.X+1, box.Y+yo+3, box.X+3, box.Y+yo+5, Color.Black);
        Primitives.Line(e.Surface, box.X+4, box.Y+yo+4, box.X+7, box.Y+yo+1, Color.Black);
      }

    GameLib.Fonts.Font font = Font;
    if(font!=null)
    { if(right) rect.X += boxSize+3;
      else rect.Width -= boxSize+3;
      font.BackColor = BackColor;
      font.Color = ForeColor;
      font.Render(e.Surface, Text, rect, align);
    }

    Color.FromArgb(212, 208, 200);
  }
  
  protected internal override void OnMouseDown(ClickEventArgs e) { down=true; Invalidate(); base.OnMouseDown(e); }
  protected internal override void OnMouseUp(ClickEventArgs e) { down=false; Invalidate(); base.OnMouseUp(e); }
  protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
  protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
  protected override void OnCheckedChanged(EventArgs e) { Invalidate(); base.OnCheckedChanged(e); }

  
  Color border = Color.FromArgb(212, 208, 200);
  ContentAlignment align=ContentAlignment.MiddleRight;
  bool down;
}
#endregion

#region ScrollBarBase
public abstract class ScrollBarBase : Control, IDisposable
{ public ScrollBarBase()
  { Style = ControlStyle.Clickable|ControlStyle.Draggable|ControlStyle.CanFocus;
    ClickRepeatDelay = 300;
    dragThreshold    = 4;
  }
  ~ScrollBarBase() { Dispose(true); }
  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  public class ThumbEventArgs : EventArgs
  { public int Start;
    public int End;
  }
  public delegate void ThumbHandler(object sender, ThumbEventArgs e);

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
    set { horizontal=value; Invalidate(); }
  }

  public int Maximum
  { get { return max; }
    set { max=value; Invalidate(); }
  }

  public int Minimum
  { get { return min; }
    set { min=value; Invalidate(); }
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

  protected enum Place { Down, PageDown, Thumb, PageUp, Up };
  protected class ClickRepeat : GameLib.Events.WindowEvent
  { public ClickRepeat(Control control, Place place) : base(control) { Place=place; }
    public Place Place;
  }

  protected internal override void OnMouseDown(ClickEventArgs e)
  { if(!e.Handled && e.CE.Button==MouseButton.Left)
    { if(crTimer!=null)
      { Place p = FindPlace(e.CE.Point);
        if(p != Place.Thumb)
        { repeatEvent = new ClickRepeat(this, p);
          crTimer.Change(crDelay, crRate);
        }
      }
      if(!repeated)
      { switch(FindPlace(e.CE.Point))
        { case Place.Down: OnDown(eventArgs); break;
          case Place.PageDown: OnPageDown(eventArgs); break;
          case Place.PageUp: OnPageUp(eventArgs); break;
          case Place.Up: OnUp(eventArgs); break;
        }
      }
      e.Handled = true;
    }
  }
  protected internal override void OnMouseUp(ClickEventArgs e)
  { if(e.CE.Button==MouseButton.Left && repeatEvent!=null)
    { crTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
      repeatEvent = null;
      repeated    = false;
    }
  }
  protected internal override void OnMouseLeave(EventArgs e)
  { if(repeatEvent!=null)
    { crTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
      repeatEvent = null;
      repeated    = false;
    }
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
  { if(e is ClickRepeat)
    { switch(((ClickRepeat)e).Place)
      { case Place.Down: OnDown(eventArgs); break;
        case Place.PageDown: OnPageDown(eventArgs); break;
        case Place.PageUp: OnPageUp(eventArgs); break;
        case Place.Up: OnUp(eventArgs); break;
      }
    }
    base.OnCustomEvent(e);
  }
  protected internal override void OnKeyDown(KeyEventArgs e)
  { if(!e.Handled)
      switch(e.KE.Key)
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
  
  protected void Dispose(bool destructing)
  { if(crTimer!=null)
    { crTimer.Dispose();
      crTimer=null;
    }
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

  protected ThumbEventArgs thumbArgs = new ThumbEventArgs();
  protected EventArgs eventArgs = new EventArgs();
  protected ValueChangedEventArgs valChange = new ValueChangedEventArgs(0);

  int Space { get { return (horizontal ? Width : Height)-EndSize*2-ThumbSize; } }

  System.Threading.Timer crTimer;
  ClickRepeat repeatEvent;
  int  value, min, max=100, smallInc=1, pageInc=10, endSize=8, thumbSize=10, dragOff=-1;
  uint crDelay, crRate=50;
  bool autoUpdate, horizontal, repeated;
}
#endregion

#region ScrollBar
public class ScrollBar : ScrollBarBase // TODO: replace with image-based scrollbar
{ public ScrollBar() { BackColor=Color.LightGray; ForeColor=Color.Gray; }
  
  protected override void OnValueChanged(ValueChangedEventArgs e)
  { Refresh();
    base.OnValueChanged(e);
  }
  
  protected internal override void OnPaint(PaintEventArgs e)
  { base.OnPaint(e);
    Rectangle rect = DisplayRect;
    int thumb = ValueToThumb(Value);
    if(Horizontal)
    { int x=rect.X, w=rect.Width;
      rect.Width = EndSize; e.Surface.Fill(rect, ForeColor);
      rect.X = x+w-EndSize; e.Surface.Fill(rect, ForeColor);
      rect.X = x+thumb; rect.Width = ThumbSize;
    }
    else
    { int y=rect.Y, h=rect.Height;
      rect.Height = EndSize; e.Surface.Fill(rect, ForeColor);
      rect.Y = y+h-EndSize; e.Surface.Fill(rect, ForeColor);
      rect.Y = y+thumb; rect.Height = ThumbSize;
    }
    Primitives.Box(e.Surface, rect, Color.Black);
    rect.Inflate(-1, -1);
    e.Surface.Fill(rect, ForeColor);
  }
}
#endregion

#region TextBoxBase
// TODO: support multi-line edit controls
public abstract class TextBoxBase : Control
{ public TextBoxBase() { Style=ControlStyle.CanFocus; }
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
        if(oldlen!=0 && (Selected || !hideSelection)) Invalidate();
        OnCaretPositionChanged(e);
      }
    }
  }

  public bool HideSelection
  { get { return hideSelection; }
    set
    { hideSelection=value;
      if(value && !Selected) Invalidate();
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
        if(Selected || !hideSelection) Invalidate();
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
  { if(selectOnFocus) { SelectAll(); Invalidate(); }
    WithCaret=this;
    base.OnGotFocus(e);
  }
  protected override void OnLostFocus(EventArgs e)
  { if(hideSelection) Invalidate();
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

  protected internal override void OnCustomEvent(GameLib.Events.WindowEvent e)
  { if(e is CaretFlashEvent) OnCaretFlash();
    base.OnCustomEvent(e);
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
      { if(withCaret!=null) withCaret.Invalidate();
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
public class TextBox : TextBoxBase
{ public TextBox()
  { BackColor=Color.White; ForeColor=Color.Black; Style|=ControlStyle.Clickable|ControlStyle.Draggable;
  }

  public Color BorderColor { get { return border; } set { border=value; } }

  protected internal override void OnPaintBackground(PaintEventArgs e)
  { base.OnPaintBackground(e);

    GameLib.Fonts.Font font = Font;
    Rectangle rect = DisplayRect;
    if(font!=null && Text.Length>0)
    { int caret = CaretPosition;
      if(caret<start) start = Math.Max(caret-10, 0);
      string text;
      while(true)
      { text = start==0 ? Text : Text.Substring(start);
        end = start+font.HowManyFit(text, rect.Width-padding*2);
        if(end-start<text.Length && caret>=end) start = Math.Min(start+10+caret-end, Text.Length);
        else break;
      }

      if((!HideSelection || Focused) && SelectionLength!=0)
      { text = Text.Substring(start, end-start);
        int x = rect.X+padding, width;
        if(SelectionStart>start) x += font.CalculateSize(text.Substring(0, SelectionStart-start)).Width;
        width = font.CalculateSize(text.Substring(Math.Max(0, SelectionStart-start),
                                                  Math.Min(Math.Abs(SelectionLength), end-start))).Width;
        e.Surface.Fill(new Rectangle(x, rect.Y+padding, width, rect.Height-padding*2), ForeColor);
      }
    }
    else start=end=0;

    Helpers.DrawBorder(e.Surface, rect, BorderStyle.FixedThick, border, true);
  }
  
  protected internal override void OnPaint(PaintEventArgs e)
  { base.OnPaint(e);
    GameLib.Fonts.Font font = Font;
    if(font!=null)
    { Rectangle rect = DisplayRect;
      rect.Inflate(-padding, -padding);
      Point location = rect.Location;
      if(rect.Height>font.Height) location.Y += (rect.Height-font.Height)/2;

      string text = Text.Substring(start, end-start);

      font.Color = ForeColor;
      font.BackColor = BackColor;
      if((HideSelection && !Focused) || SelectionLength==0) font.Render(e.Surface, text, location);
      else
      { if(SelectionStart>start)
          location.X += font.Render(e.Surface, text.Substring(0, SelectionStart-start), location);

        font.Color = BackColor;
        font.BackColor = ForeColor;
        location.X += font.Render(e.Surface, text.Substring(Math.Max(0, SelectionStart-start),
                                                            Math.Min(Math.Abs(SelectionLength), end-start)), location);
        if(SelectionEnd<end)
        { font.Color = ForeColor;
          font.BackColor = BackColor;
          font.Render(e.Surface, text.Substring(SelectionEnd-start, end-SelectionEnd), location);
        }
      }

      int x = rect.X + font.CalculateSize(text.Substring(0, CaretPosition-start)).Width;
      Primitives.VLine(e.Surface, x, rect.Top+2, rect.Bottom-3, CaretOn && HasCaret ? ForeColor : BackColor);
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

  protected override void OnCaretPositionChanged(ValueChangedEventArgs e) { Invalidate(); }
  
  protected override void OnTextChanged(ValueChangedEventArgs e)
  { Invalidate();
    base.OnTextChanged(e);
  }

  protected override void OnCaretFlash()
  { DesktopControl desktop = Desktop;
    GameLib.Fonts.Font font = Font;
    if(desktop!=null && desktop.Surface!=null && font!=null)
    { Rectangle rect = DisplayRect;
      rect.Y += padding+2; rect.Height -= padding*2+4; rect.Width = 1;
      rect.X += font.CalculateSize(Text.Substring(start, CaretPosition-start)).Width + padding;
      Primitives.VLine(desktop.Surface, rect.X, rect.Top, rect.Bottom-1, CaretOn && HasCaret ? ForeColor : BackColor);
      desktop.AddUpdatedArea(rect);
      if(Events.Events.QueueSize==0) Events.Events.PushEvent(new Events.DesktopUpdatedEvent(desktop));
    }
  }

  int PointToPos(int x)
  { Rectangle rect = WindowRect;
    rect.Inflate(-padding, -padding);
    if(x<rect.X) return start;
    else if(x>rect.Width) return end;
    else
    { GameLib.Fonts.Font font = Font;
      return font==null ? 0 : start + font.HowManyFit(Text.Substring(start, end-start), x-rect.X);
    }
  }
  
  const int padding=2;

  Color border = Color.FromArgb(212, 208, 200);
  int start, end;
}
#endregion

#region MenuItemBase
public abstract class MenuItemBase : Control
{ public bool AllowKeyRepeat { get { return keyRepeat; } set { keyRepeat=value; } }
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
{ public MenuBase() { Style |= ControlStyle.Clickable|ControlStyle.CanFocus; }

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
    Location = source.WindowToAncestor(position, desktop);
    Visible = false;
    Parent  = desktop;
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
        if(c==item.HotKey)
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
          if(item.Bounds.Contains(e.CE.Point)) { PostClickEvent(item); Close(); goto done; }
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
        if(item.Bounds.Contains(e.CE.Point)) { PostClickEvent(item); break; }
      Close();
      e.Handled=true;
    }
    base.OnMouseClick(e);
  }

  protected internal override void OnCustomEvent(GameLib.Events.WindowEvent e)
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
{ public MenuBarBase() { Style |= ControlStyle.Clickable; menus=new MenuCollection(this); }

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
      foreach(MenuItemBase item in menu.Controls)
        if(item.GlobalHotKey.Matches(e))
        { if(!item.AllowKeyRepeat) Desktop.StopKeyRepeat();
          menu.PostClickEvent(item);
          return true;
        }
    return false;
  }

  protected internal override void OnMouseDown(ClickEventArgs e)
  { if(e.CE.Button==MouseButton.Left)
    { for(int i=0; i<buttons.Length; i++)
      { if(buttons[i].Area.Contains(e.CE.Point)) Open(i);
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

  protected internal override void OnMouseMove(GameLib.Events.MouseMoveEvent e)
  { int over=-1, oldOver=-1;
    for(int i=0; i<buttons.Length; i++)
    { if(buttons[i].Over || buttons[i].State!=ButtonState.Normal) oldOver=i;
      buttons[i].Over = buttons[i].Area.Contains(e.Point);
      if(buttons[i].Over) over=i;
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
    base.OnMouseMove(e);
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
{ public MenuItem() { }
  public MenuItem(string text) { Text=text; }
  public MenuItem(string text, char hotKey) { Text=text; HotKey=hotKey; }
  public MenuItem(string text, char hotKey, KeyCombo globalHotKey)
  { Text=text; HotKey=hotKey; GlobalHotKey=globalHotKey;
  }

  public int HorizontalPadding
  { get { return horzPadding; }
    set
    { if(value<0) throw new ArgumentOutOfRangeException("Padding", value, "must be >=0");
      horzPadding = value;
      Invalidate();
    }
  }

  public int VerticalPadding
  { get { return vertPadding; }
    set
    { if(value<0) throw new ArgumentOutOfRangeException("Padding", value, "must be >=0");
      vertPadding = value;
      Invalidate();
    }
  }

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

  protected override void OnBackColorChanged(ValueChangedEventArgs e) { Invalidate(); base.OnBackColorChanged(e); }
  protected override void OnForeColorChanged(ValueChangedEventArgs e) { Invalidate(); base.OnForeColorChanged(e); }

  protected internal override void OnPaintBackground(PaintEventArgs e)
  { if(mouseOver)
    { Color c = RawBackColor;
      BackColor = SelectedBackColor;
      base.OnPaintBackground(e);
      BackColor = c;
    }
    else base.OnPaintBackground(e);
  }

  protected internal override void OnPaint(PaintEventArgs e)
  { base.OnPaint(e);
    if(Text.Length>0)
    { GameLib.Fonts.Font f = Font;
      if(f != null)
      { Rectangle rect = DisplayRect;
        rect.Inflate(-horzPadding, -vertPadding);
        f.Color     = mouseOver ? SelectedForeColor : ForeColor;
        f.BackColor = mouseOver ? SelectedBackColor : BackColor;
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
  Color selFore=Color.Transparent, selBack=Color.Transparent;
  bool mouseOver;
}
#endregion

#region Menu
// TODO: make sure this handles font/text changes, etc
public class Menu : MenuBase
{ public Menu() { }
  public Menu(string text) { Text=text; }
  public Menu(string text, KeyCombo globalHotKey) { Text=text; GlobalHotKey=globalHotKey; }

  public Color SelectedBackColor { get { return selBack; } set { selBack=value; } }
  public Color SelectedForeColor { get { return selFore; } set { selFore=value; } }

  protected override void OnPopup(EventArgs e)
  { base.OnPopup(e);

    int width=0, height=0;
    foreach(MenuItemBase item in Controls)
    { item.Size = item.MeasureItem();
      if(item.Width>width) width=item.Width;
      item.Location = new Point(2, height+2);
      height += item.Height;
    }
    foreach(MenuItemBase item in Controls) item.Width=width;
    Size = new Size(width+4, height+4);
  }
  
  protected internal override void OnPaintBackground(PaintEventArgs e)
  { base.OnPaintBackground(e);
    Helpers.DrawBorder(e.Surface, DisplayRect, BorderStyle.Fixed3D, BackColor, false);
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
      { if(button.State!=ButtonState.Normal)
          Helpers.DrawBorder(e.Surface, WindowToDisplay(button.Area), BorderStyle.Fixed3D, BackColor, false);
      }
    }
  }

  protected internal override void OnPaint(PaintEventArgs e)
  { base.OnPaint(e);
    GameLib.Fonts.Font font = Font;
    if(font==null) return;
    foreach(MenuButton button in Buttons)
    { if(button.Area.IntersectsWith(e.WindowRect))
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

#region FormBase
// TODO: implement resizing using the mouse
public abstract class FormBase : ContainerControl
{ public FormBase() { Style |= ControlStyle.CanFocus; }

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
    BringToFront();
    SetModal(true);
    while(Events.Events.PumpEvent() && Parent!=null);
    return returnValue;
  }

  public event System.ComponentModel.CancelEventHandler Closing;
  public event EventHandler Closed;
  
  protected virtual void OnClosing(System.ComponentModel.CancelEventArgs e)
  { if(Closing!=null) Closing(this, e);
  }
  protected virtual void OnClosed(EventArgs e) { if(Closed!=null) Closed(this, e); }

  object returnValue;
}
#endregion

#region Form
// TODO: add caption bar and menu bar
public class Form : FormBase
{ public Form() { ForeColor=Color.Black; BackColor=Color.FromArgb(212, 208, 200); }

  public BorderStyle BorderStyle
  { get { return border; }
    set
    { if(border!=value)
      { border=value;
        Invalidate();
      }
    }
  }

  protected internal override void OnPaintBackground(PaintEventArgs e)
  { base.OnPaintBackground(e);
    Helpers.DrawBorder(e.Surface, DisplayRect, border, BackColor, false);
  }

  BorderStyle border=BorderStyle.FixedThick;
}
#endregion

#region MessageBox
public enum MessageBoxButtons { Ok, OkCancel, YesNo, YesNoCancel }
public sealed class MessageBox : Form
{ internal MessageBox(string text, string[] buttons) { DialogResult=-1; message=text; buttonTexts=buttons; }

  string[] buttonTexts;
  string   message;

  public int Show(DesktopControl desktop)
  { if(Controls.Count==0)
    { GameLib.Fonts.Font font = RawFont==null ? desktop.Font : RawFont;
      if(font!=null)
      { int btnWidth=0, btnHeight=font.LineSkip*3/2, height=font.LineSkip*3+btnHeight, btnSpace=12;
        int[] sizes = new int[buttonTexts.Length];
        for(int i=0; i<buttonTexts.Length; i++)
        { sizes[i] = font.CalculateSize(buttonTexts[i]).Width * 3/2; // padding inside button
          if(sizes[i]<40) sizes[i]=40;
          btnWidth += sizes[i];
        }
        btnWidth += (buttonTexts.Length-1) * btnSpace; // space between buttons
        
        Label label  = new Label(message);
        int textWidth = desktop.Width/2-font.LineSkip*2, textHeight;
        if(btnWidth>textWidth) textWidth = btnWidth;
        Rectangle rect = new Rectangle(0, 0, textWidth, int.MaxValue);
        int lines = font.WordWrap(message, rect).Length;
        if(lines==1) textWidth = font.CalculateSize(message).Width;
        textWidth += label.TextPadding*2;
        textHeight = lines*font.LineSkip + label.TextPadding*2;

        height += textHeight;
        Size = new Size(Math.Max(Math.Min(desktop.Width, btnWidth*3/2), textWidth+font.LineSkip*2), height);
        Location = new Point((desktop.Width-Width)/2, (desktop.Height-Height)/2);
        
        label.Bounds = new Rectangle((Width-textWidth)/2, font.LineSkip, textWidth, textHeight);
        label.TextAlign = ContentAlignment.TopCenter;
        
        int x = (Width-btnWidth)/2, y = Height-font.LineSkip-btnHeight;
        for(int i=0; i<buttonTexts.Length; i++)
        { Button btn = new Button(buttonTexts[i]);
          btn.Bounds = new Rectangle(x, y, sizes[i], btnHeight);
          btn.Click += new ClickEventHandler(btn_OnClick);
          btn.Tag    = i;
          btn.TabIndex = i;
          x += sizes[i]+btnSpace;
          Controls.Add(btn);
        }
        Controls.Add(label);
        Controls[0].Focus();
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

  public static MessageBox Create(string caption, string text) { return Create(caption, text, MessageBoxButtons.Ok); }
  public static MessageBox Create(string caption, string text, MessageBoxButtons buttons)
  { switch(buttons)
    { case MessageBoxButtons.Ok: return Create(caption, text, ok);
      case MessageBoxButtons.OkCancel: return Create(caption, text, okCancel);
      case MessageBoxButtons.YesNo: return Create(caption, text, yesNo);
      case MessageBoxButtons.YesNoCancel: return Create(caption, text, yesNoCancel);
      default: throw new ArgumentException("Unknown MessageBoxButtons value");
    }
  }
  public static MessageBox Create(string caption, string text, string[] buttonText)
  { if(buttonText.Length==0) throw new ArgumentException("Can't create a MessageBox with no buttons!", "buttonText");
    MessageBox box = new MessageBox(text, buttonText);
    box.Text = caption;
    return box;
  }

  public static void Show(DesktopControl desktop, string caption, string text)
  { Create(caption, text).Show(desktop);
  }
  public static int Show(DesktopControl desktop, string caption, string text, MessageBoxButtons buttons)
  { return Create(caption, text, buttons).Show(desktop);
  }
  public static int Show(DesktopControl desktop, string caption, string text, string[] buttonText)
  { return Create(caption, text, buttonText).Show(desktop);
  }

  static string[] ok = new string[] { "Ok" };
  static string[] okCancel = new string[] { "Ok", "Cancel" };
  static string[] yesNo = new string[] { "Yes", "No" };
  static string[] yesNoCancel = new string[] { "Yes", "No", "Cancel" };
}
#endregion

} // namespace GameLib.Forms