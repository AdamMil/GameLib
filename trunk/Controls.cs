// TODO: implement more controls (checkbox, editbox, listbox, dropdown)
// TODO: implement a form (dialog) control
// TODO: have controls display differently when disabled
using System;
using System.Collections;
using System.Drawing;
using GameLib.Fonts;
using GameLib.Video;
using GameLib.Input;

namespace GameLib.Forms
{

public enum BorderStyle { None, Flat, ThreeDimensional };

#region ContainerControl
public class ContainerControl : Control
{ protected internal override void OnPaint(PaintEventArgs e)
  { for(int i=Controls.Count-1; i>=0; i--) // we count backwards because the first element is on top
    { Control c = Controls[i];
      if(c.Visible)
      { Rectangle paint = Rectangle.Intersect(c.ParentRect, e.WindowRect);
        if(paint.Width>0)
        { paint.X -= c.Left; paint.Y -= c.Top;
          c.AddInvalidRect(paint);
          c.Update();
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
    { if(WindowRect.Contains(e.CE.Point)) OnClick(e);
      e.Handled = true;
    }
    base.OnMouseClick(e);
  }
  
  protected internal override void OnKeyDown(KeyEventArgs e)
  { if((e.KE.Key==Key.Return || e.KE.Key==Key.Space || e.KE.Key==Key.KpEnter) && !e.Handled)
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
  { if(!e.Handled && e.CE.Button==0)
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
public class TextBoxBase : Control
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
        if(oldlen!=0 && (Focused || !hideSelection)) Invalidate();
        OnCaretPositionChanged(e);
      }
    }
  }

  public bool HideSelection
  { get { return hideSelection; }
    set
    { hideSelection=value;
      if(value && !Focused) Invalidate();
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
        if(Focused || !hideSelection) Invalidate();
      }
    }
  }
  
  public int SelectionStart { get { return selectLen<0 ? caret+selectLen : caret; } }

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
  
  protected override void OnGotFocus (EventArgs e) { withCaret=this; base.OnGotFocus(e); }
  protected override void OnLostFocus(EventArgs e) { withCaret=null; base.OnLostFocus(e); }

  protected internal override void OnKeyPress(KeyEventArgs e)
  { if(e.KE.Char>=32)
    { if(maxLength==-1 || Text.Length<maxLength) { Modified=true; InsertText(e.KE.Char.ToString()); }
      e.Handled=true;
    }
    base.OnKeyPress(e);
  }
  
  protected internal override void OnKeyDown(KeyEventArgs e)
  { char c=e.KE.Char;
    // TODO: implement ctrl-arrow
    if(e.KE.Key==Key.Left)
    { if(e.KE.KeyMods==KeyMod.None || e.KE.HasOnlyKeys(KeyMod.Shift))
      { if(caret>0)
        { if(e.KE.KeyMods!=KeyMod.None) Select(caret-1, selectLen+1);
          else Select(caret-1, 0);
        }
        else if(e.KE.KeyMods==KeyMod.None) SelectionLength=0;
        e.Handled=true;
      }
    }
    else if(e.KE.Key==Key.Right)
    { if(e.KE.KeyMods==KeyMod.None || e.KE.HasOnlyKeys(KeyMod.Shift))
      { if(caret<Text.Length)
        { if(e.KE.KeyMods!=KeyMod.None) Select(caret+1, selectLen-1);
          else Select(caret+1, 0);
        }
        else if(e.KE.KeyMods==KeyMod.None) SelectionLength=0;
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
    }
    else if(e.KE.Key==Key.Delete && e.KE.KeyMods==KeyMod.None)
    { if(selectLen!=0) { Modified=true; SelectedText=""; }
      else if(caret<Text.Length)
      { Modified=true;
        Text = Text.Substring(0, caret) + Text.Substring(caret+1, Text.Length-caret-1);
      }
      e.Handled=true;
    }
    else if(e.KE.Key==Key.Home)
    { if(e.KE.KeyMods==KeyMod.None) { Select(0, 0); e.Handled=true; }
      else if(e.KE.HasOnlyKeys(KeyMod.Shift)) { Select(0, caret); e.Handled=true; }
    }
    else if(e.KE.Key==Key.End)
    { if(e.KE.KeyMods==KeyMod.None) { Select(Text.Length, 0); e.Handled=true; }
      else if(e.KE.HasOnlyKeys(KeyMod.Shift)) { Select(caret, Text.Length-caret); e.Handled=true; }
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
  
  protected bool HasCaret { get { return withCaret==this; } }

  int  caret, selectLen, maxLength=-1;
  bool hideSelection, modified, wordWrap;

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
      { if(withCaret!=null && caretOn) { caretOn=false; withCaret.OnCaretFlash(); caretOn=true; }
        withCaret=value;
      }
    }
  }

  static void CaretFlash(object dummy)
  { caretOn = !caretOn;
    TextBoxBase tb = withCaret;
    if(tb!=null) tb.OnCaretFlash();
  }
  
  static System.Threading.Timer caretTimer;
  static TextBoxBase withCaret;
  static string clipboard = string.Empty;
  static int flashRate;
  static bool caretOn;
  #endregion
}
#endregion

#region TextBox
public class TextBox : TextBoxBase
{ 
}
#endregion

#region FormBase
/*
public class FormBase : ContainerControl
{ public bool Modal { get; set; }
  
  public void Close()
  { if(Parent!=null) return;
    System.ComponentModel.CancelEventArgs e;
    OnClosing(e);
    if(!e.Cancel)
    { OnClosed(new EventArgs());
      Parent = null;
    }
  }

  #region Events
  public event System.ComponentModel.CancelEventHandler Closing;
  public event EventHandler Closed;
  
  protected virtual void OnClosing(System.ComponentModel.CancelEventHandler e)
  { if(Closing!=null) Closing(this, e);
  }
  protected virtual void OnClosed(EventArgs e) { if(Closed) Closed(this, e); }

  protected virtual void OnNcMouseDown(ClickEventArgs e) { }
  protected virtual void OnNcMouseUp(ClickEventArgs e) { }
  protected virtual void OnNcMouseClick(ClickEventArgs e) { }
  
  protected virtual void OnNcDragStart(DragEventArgs e) { }
  protected virtual void OnNcDragMove(DragEventArgs e) { }
  protected virtual void OnNcDragEnd(DragEventArgs e) { }
  
  protected virtual void OnNcPaint(PaintEventArgs e) { }
  
  protected internal virtual void OnMouseDown(ClickEventArgs e)
  { if(NonClientRect.Contains(e.CE.Point)) OnNcMouseDown(e);
    e.Handled = true;
    base.OnMouseDown(e);
  }
  
  protected internal virtual void OnMouseUp(ClickEventArgs e)
  { if(NonClientRect.Contains(e.CE.Point)) OnNcMouseUp(e);
    e.Handled = true;
    base.OnMouseUp(e);
  }
  
  protected internal virtual void OnMouseClick(ClickEventArgs e)
  { if(NonClientRect.Contains(e.CE.Point)) OnNcMouseClick(e);
    e.Handled = true;
    base.OnMouseClick(e);
  }
  
  protected internal virtual void OnDragStart(DragEventArgs e)
  { if(NonClientRect.Contains(e.Start)) OnNcDragStart(e);
    base.OnDragStart(e);
  }
  
  protected internal virtual void OnDragMove(DragEventArgs e)
  { if(NonClientRect.Contains(e.End)) OnNcDragMove(e);
    base.OnDragMove(e);
  }

  protected internal virtual void OnDragEnd(DragEventArgs e)
  { if(NonClientRect.Contains(e.End)) OnNcDragEnd(e);
    base.OnDragEnd(e);
  }
  
  protected internal virtual void OnPaint(PaintEventArgs e);
  { base.OnPaint(e);

    //Rectangle displayRect = e.DisplayRect, windowRect = e.WindowRect;
    //e.DisplayRect.Intersect(ncRect);
    //if(e.DisplayRect.Width>0)
    //{ e.WindowRect = RectToWindow(e.DisplayRect);
    //  OnNcPaint(e);
    //}
  }
  #endregion
  
  protected Rectangle NonClientRect { get { return ncRect; } set { ncRect=value; } }
  
  protected Point WindowToNonClient(Point windowPoint);
  protected Point NonClientToWindow(Point ncPoint);

  protected Rectangle WindowToNonClient(Rectangle windowRect);
  protected Rectangle NonClientToWindow(Rectangle ncRect);

  Rectangle ncRect;
}
*/
#endregion

} // namespace GameLib.Forms