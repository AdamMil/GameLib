// TODO: implement more controls (checkbox, editbox, listbox, dropdown)
// TODO: implement a form (dialog) control
using System;
using System.Collections;
using System.Drawing;
using GameLib.Fonts;
using GameLib.Video;

namespace GameLib.Forms
{

#region ContainerControl
public class ContainerControl : Control
{ protected internal override void OnPaint(PaintEventArgs e)
  { for(int i=Controls.Count-1; i>=0; i--) // we count backwards because the first element is on top
    { Control c = Controls[i];
      if(c.Visible)
      { Rectangle paint = Rectangle.Intersect(c.ParentRect, e.ClientRect);
        if(paint.Width>0)
        { paint.X -= c.Left; paint.Y -= c.Top;
          c.Refresh(paint);
        }
      }
    }
    base.OnPaint(e);
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
public class LabelBase : Control
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

  protected internal override void OnPaint(PaintEventArgs e)
  { base.OnPaint(e);
    if(Image!=null)
    { Point at = Utility.CalculateAlignment(DisplayRect, new Size(Image.Width, Image.Height), ImageAlign);
      Image.Blit(e.Surface, at.X, at.Y);
    }
    if(Text.Length>0)
    { GameLib.Fonts.Font f = Font;
      if(f != null)
      { f.Color     = ForeColor;
        f.BackColor = BackColor;
        f.Render(e.Surface, Text, DisplayRect, TextAlign);
      }
    }
  }
}
#endregion

#region ButtonBase
public abstract class ButtonBase : LabelBase
{ public ButtonBase() { Style|=ControlStyle.Clickable|ControlStyle.CanFocus; }

  public event ClickEventHandler Click;

  protected internal override void OnMouseDown(ClickEventArgs e)
  { Capture = pressed = true;
    base.OnMouseDown(e);
  }

  protected internal override void OnMouseUp(ClickEventArgs e)
  { Capture = pressed = false;
    base.OnMouseUp(e);
  }

  protected internal override void OnMouseClick(ClickEventArgs e)
  { if(e.CE.Button==0 && !e.Handled)
    { if(ClientRect.Contains(e.CE.Point)) OnClick(e);
      e.Handled = true;
    }
    base.OnMouseClick(e);
  }
  
  protected internal override void OnKeyDown(KeyEventArgs e)
  { if((e.KE.Key==Input.Key.Return || e.KE.Key==Input.Key.Space || e.KE.Key==Input.Key.KpEnter) && !e.Handled)
      OnClick(new ClickEventArgs());
  }

  protected virtual void OnClick(ClickEventArgs e) { if(Click!=null) Click(this, e); }
  
  protected bool Pressed { get { return pressed; } set { pressed=value; } }
  
  bool pressed;
}
#endregion

#region Button
public class Button : ButtonBase
{ public Button() { ImageAlign=TextAlign=ContentAlignment.MiddleCenter; }
  public Button(string text) { ImageAlign=TextAlign=ContentAlignment.MiddleCenter; Text=text; }

  protected internal override void OnMouseDown(ClickEventArgs e) { Invalidate(); base.OnMouseDown(e); }
  protected internal override void OnMouseUp(ClickEventArgs e) { Invalidate(); base.OnMouseUp(e); }

  protected internal override void OnPaint(PaintEventArgs e)
  { base.OnPaint(e);

    Rectangle rect = DisplayRect;

    if(Image!=null)
    { Point at = Utility.CalculateAlignment(rect, new Size(Image.Width, Image.Height), ImageAlign);
      if(Pressed) at.Offset(1, 1);
      Image.Blit(e.Surface, at.X, at.Y);
    }
    if(Text.Length>0)
    { GameLib.Fonts.Font f = Font;
      if(f!=null)
      { Rectangle box = rect;
        box.Inflate(-1, -1);
        if(Pressed) box.Offset(1, 1);
        f.Color     = ForeColor;
        f.BackColor = BackColor;
        f.Render(e.Surface, Text, box, TextAlign);
      }
    }
    
    Color bright, dark, back=BackColor, fore=ForeColor;
    bright = Color.FromArgb(back.R+(255-back.R)*3/5, back.G+(255-back.G)*3/5, back.B+(255-back.B)*3/5);
    dark   = Color.FromArgb(back.R/2, back.G/2, back.B/2);
    if(Pressed) { Color t=bright; bright=dark; dark=t; }
    else if(Focused) bright=dark=Color.Black;
    Primitives.Line(e.Surface, rect.X, rect.Y, rect.Right-1, rect.Y, bright);
    Primitives.Line(e.Surface, rect.X, rect.Y, rect.X, rect.Bottom-1, bright);
    Primitives.Line(e.Surface, rect.X, rect.Bottom-1, rect.Right-1, rect.Bottom-1, dark);
    Primitives.Line(e.Surface, rect.Right-1, rect.Y, rect.Right-1, rect.Bottom-1, dark);
  }
  
  protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
  protected override void OnGotFocus(EventArgs e)  { Invalidate(); base.OnGotFocus(e); }
}
#endregion

#region CheckBoxBase
public class CheckBoxBase : ButtonBase
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
  { if(e.CE.Button==0 && !e.Handled)
    { Checked   = !value;
      e.Handled = true;
    }
    base.OnClick(e);
  }
  
  bool value;
}
#endregion

#region ScrollBarBase
public class ScrollBarBase : Control
{ public ScrollBarBase()
  { Style = ControlStyle.Clickable|ControlStyle.Draggable|ControlStyle.CanFocus;
    ClickRepeatDelay = 300;
    dragThreshold    = 4;
  }

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
  { if(e.CE.Button==0 && crTimer!=null)
    { Place p = FindPlace(e.CE.Point);
      if(p != Place.Thumb)
      { repeatEvent = new ClickRepeat(this, p);
        crTimer.Change(crDelay, crRate);
      }
    }
  }
  protected internal override void OnMouseUp(ClickEventArgs e)
  { if(e.CE.Button==0 && repeatEvent!=null)
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
  protected internal override void OnMouseClick(ClickEventArgs e)
  { if(!repeated && !e.Handled && e.CE.Button==0)
    { switch(FindPlace(e.CE.Point))
      { case Place.Down: OnDown(eventArgs); break;
        case Place.PageDown: OnPageDown(eventArgs); break;
        case Place.PageUp: OnPageUp(eventArgs); break;
        case Place.Up: OnUp(eventArgs); break;
      }
      e.Handled = true;
    }
    base.OnMouseClick(e);
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
  { switch(((ClickRepeat)e).Place)
    { case Place.Down: OnDown(eventArgs); break;
      case Place.PageDown: OnPageDown(eventArgs); break;
      case Place.PageUp: OnPageUp(eventArgs); break;
      case Place.Up: OnUp(eventArgs); break;
    }
    base.OnCustomEvent(e);
  }
  protected internal override void OnKeyDown(KeyEventArgs e)
  { if(!e.Handled)
      switch(e.KE.Key)
      { case Input.Key.PageDown: OnPageUp(eventArgs); break;
        case Input.Key.PageUp: OnPageDown(eventArgs); break;
        case Input.Key.Down: case Input.Key.Right: OnUp(eventArgs); break;
        case Input.Key.Up: case Input.Key.Left: OnDown(eventArgs); break;
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
  
  protected int ThumbToValue(int position)
  { if(position<0) position=0;
    if(horizontal)
    { if(position>Width) position=Width-1;
    }
    else if(position>Height) position=Height-1;
    return (position-endSize)*(max-min)/Space;
  }
    
  protected int ValueToThumb(int value) { return value*Space/(max-min)+endSize; }

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
  { if(repeatEvent!=null) { Events.Events.PushEvent(repeatEvent); repeated=true; }
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

} // namespace GameLib.Forms