using System;
using System.Collections.Generic;
using System.Drawing;
using GameLib.Events;
using GameLib.Input;
using GameLib.Video;

namespace GameLib.Forms
{

#region AutoFocus
/// <summary>
/// This enum is used with the <see cref="DesktopControl.AutoFocusing"/> property to determine how the
/// desktop will automatically focus controls.
/// </summary>
public enum AutoFocus
{
  /// <summary>The desktop will not autofocus controls. The code will have to call <see cref="Control.Focus"/> and
  /// <see cref="Control.Blur"/> manually in order to alter the focus.
  /// </summary>
  None,
  /// <summary>When a focusable control is clicked with any button, the desktop will focus it and all its ancestors.
  /// </summary>
  Click,
  /// <summary>When the mouse moves over a focusable control, it and its ancestors will be focused. When the
  /// mouse moves off of a control, it will be blurred, even if the mouse did not move onto another focusable
  /// control.
  /// </summary>
  Over,
  /// <summary>When the mouse moves over a focusable control, it and its ancestors will be focused.</summary>
  OverSticky
}
#endregion

public delegate void PaintHandler(PaintEventArgs e);

#region Desktop
public class Desktop : Control, IDisposable
{
  public Desktop()
  {
    Events.Events.Initialize();
    Desktop   = this;
    BackColor = Color.Black;
    ForeColor = Color.White;
    AutoFocusing = AutoFocus.Click;
    SetFlag(Flag.Focused, true);
    Invalidate();
    timer.Start();
  }

  public Desktop(IControlRenderer renderer, IGuiRenderTarget drawTarget) : this()
  {
    DrawTarget = drawTarget;
    Renderer   = renderer;
  }

  ~Desktop()
  {
    Dispose(true);
  }

  /// <summary>Gets or sets the auto focusing mode for this desktop.</summary>
  /// <remarks>See the <see cref="AutoFocus"/> enum for information on the types of auto focusing available.
  /// The default is <see cref="AutoFocus.None"/>.
  /// </remarks>
  public AutoFocus AutoFocusing
  {
    get; set;
  }

  /// <summary>Gets or sets the double click delay in milliseconds for this desktop.</summary>
  /// <remarks>The double click delay is the maximum number of milliseconds allowed between mouse clicks for them
  /// to be recognized as a double click. The default value is 350 milliseconds.
  /// </remarks>
  public int DoubleClickDelay
  {
    get { return dcDelay; }
    set
    {
      if(value < 0) throw new ArgumentOutOfRangeException("DoubleClickDelay", "cannot be negative");
      dcDelay = value;
    }
  }

  /// <summary>Gets or sets the default drag threshold for this desktop.</summary>
  /// <remarks>This property provides a default drag threshold for controls that do not specify one. See
  /// <see cref="GuiControl.DragThreshold"/> for more information about the drag threshold. The default value is 16.
  /// </remarks>
  public int DefaultDragThreshold
  {
    get { return defaultDragThreshold; }
    set
    {
      if(value < 1) throw new ArgumentOutOfRangeException("DragThreshold", "must be >=1");
      defaultDragThreshold = value;
    }
  }

  /// <summary>Gets or sets the render target associated with this desktop.</summary>
  /// <remarks>This property controls the render target into which this desktop will draw. The area of the target into
  /// which the desktop will draw is controlled by <see cref="Control.Bounds"/> and related properties.
  /// </remarks>
  public new IGuiRenderTarget DrawTarget
  {
    get { return base.DrawTarget; }
    set
    {
      if(value != DrawTarget)
      {
        base.DrawTarget = value;
        updatedLen = 0;
        UpdateDrawTargets();
        if(value != null) Invalidate();
      }
    }
  }

  /// <summary>Gets the topmost modal control, or null if there are none.</summary>
  public Control ModalWindow
  {
    get { return modal.Count == 0 ? null : modal[modal.Count - 1]; }
  }

  /// <summary>Gets or sets whether this desktop will process keyboard events.</summary>
  /// <remarks>If false, <see cref="ProcessEvent"/> will ignore events related to the keyboard.
  /// The default value is true.
  /// </remarks>
  public bool ProcessKeys
  {
    get { return keys; }
    set { keys = value; }
  }

  /// <summary>Gets or sets whether this desktop will process mouse movement events.</summary>
  /// <remarks>If false, <see cref="ProcessEvent"/> will ignore events related to mouse movement.
  /// The default value is true.
  /// </remarks>
  public bool ProcessMouseMove
  {
    get { return moves; }
    set { moves = value; }
  }

  /// <summary>Gets or sets whether this desktop will process mouse click events.</summary>
  /// <remarks>If false, <see cref="ProcessEvent"/> will ignore events related to mouse clicks.
  /// The default value is true.
  /// </remarks>
  public bool ProcessClicks
  {
    get { return clicks; }
    set { clicks = value; }
  }

  /// <summary>Gets or sets the renderer used to paint controls on this desktop. This property must be set before the
  /// desktop is usable.
  /// </summary>
  public new IControlRenderer Renderer
  {
    get { return base.Renderer; }
    set
    {
      if(value != base.Renderer)
      {
        if(Renderer != null) Renderer.VideoModeChanged -= OnVideoModeChanged;
        base.Renderer = value;
        if(value != null) value.VideoModeChanged += OnVideoModeChanged;
        UpdateRenderers(this);
        UpdateDrawTargets();
      }
    }
  }

  /// <summary>Gets or sets the key used to tab between controls.</summary>
  /// <remarks>If this property is set to a value other than <see cref="Input.Key.None"/>, that key will be used
  /// to move input focus between controls. When that key is pressed, the desktop will call
  /// <see cref="Control.TabToNextControl"/> on the control that currently has input focus.
  /// </remarks>
  public Input.Key TabCharacter
  {
    get { return tab; }
    set { tab = value; }
  }

  /// <summary>Gets or sets whether the desktop tracks the specific areas of the desktop that have been changed, as
  /// opposed to simply tracking whether the desktop has changed at all.
  /// </summary>
  /// <remarks>If set to true, the desktop will keep track of what parts of the associated surface have been
  /// updated. This can be used to efficiently update the screen. The default value is false.
  /// <seealso cref="Updated"/> <seealso cref="GetUpdatedAreas"/>
  /// </remarks>
  public bool TrackUpdates
  {
    get { return trackUpdates; }
    set
    {
      trackUpdates = value;
      if(!value) updatedLen = 0;
    }
  }

  /// <summary>Gets or sets whether any area of the desktop has been changed.</summary>
  /// <remarks>If true, then an area of the <see cref="DrawTarget"/> has been updated and should be copied to the
  /// screen. After updating the screen, this property should be set to false so that future updates can be
  /// detected.
  /// <seealso cref="TrackUpdates"/> <seealso cref="GetUpdatedAreas"/>
  /// </remarks>
  public bool Updated
  {
    get { return didPainting; }
    set
    {
      if(value != Updated)
      {
        didPainting = value;
        if(value) Invalidate();
        else updatedLen = 0;
      }
    }
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    Dispose(false);
  }

  /// <summary>Returns a list of rectangles within the <see cref="DrawTarget"/> that have been updated.</summary>
  public Rectangle[] GetUpdatedAreas()
  {
    Rectangle[] ret = new Rectangle[updatedLen];
    Array.Copy(updated, ret, updatedLen);
    return ret;
  }

  /// <summary>Processes the specified event.</summary>
  /// <param name="e">The <see cref="Event"/> to process.</param>
  /// <returns>Returns true if the event was handled by the desktop, and false otherwise. See
  /// <see cref="ProcessEvent(Event,bool)"/> for more information about the return value.
  /// </returns>
  /// <remarks>Calling this method is equivalent to calling <see cref="ProcessEvent(Event,bool)"/> and passing
  /// true to allow it to update the <see cref="Input.Input"/> class. This method should be used with care.
  /// See <see cref="ProcessEvent(Event,bool)"/> for information about proper usage of this method.
  /// </remarks>
  public bool ProcessEvent(Event e) { return ProcessEvent(e, true); }

  /// <summary>Processes the specified event.</summary>
  /// <param name="e">The <see cref="Event"/> to process.</param>
  /// <param name="passToInput">If true, the event is first passed to <see cref="Input.Input.ProcessEvent"/>.</param>
  /// <returns>Returns true if the event was handled by the desktop, and false otherwise. A return value of true
  /// does not necessarily mean that the event had an effect on this desktop, only that it might have had an effect.
  /// Thus, the event should still be passed to all other desktops.
  /// </returns>
  /// <remarks>The main event handler should pass events to this method in the order they are received. The desktop
  /// will use them to handle
  /// all user interaction with the desktop and its descendants. If <paramrem name="passToInput"/> is true,
  /// the event will first be passed to <see cref="Input.Input.ProcessEvent"/>. This is an important step, but
  /// should not be done more than once to avoid confusing the <see cref="Input.Input"/> class. Thus, if you have
  /// multiple desktops or want to update the <see cref="Input.Input"/> class yourself, you should manually pass
  /// the event to <see cref="Input.Input.ProcessEvent"/> and then call this method for each desktop, passing false
  /// for <paramrem name="passToInput"/>. If you have only a single desktop, you can safely pass true for
  /// <paramref name="passToInput"/>, assuming you don't call <see cref="Input.Input.ProcessEvent"/> yourself.
  /// <seealso cref="Events.Events"/> <seealso cref="Input.Input.ProcessEvent"/>
  /// </remarks>
  public bool ProcessEvent(Event e, bool passToInput)
  {
    if(!passToInput || Input.Input.ProcessEvent(e))
    {
      #region Mouse moves
      if(moves && e.Type == EventType.MouseMove)
      {
        MouseMoveEvent ea = (MouseMoveEvent)e;
        Point at = ea.Point;
        // if the cursor is not within the desktop area, ignore it (unless dragging or capturing)
        if(dragging == null && capturing == null && !Bounds.Contains(at)) return false;

        Control p = this, c;
        // passModal is true if there's no modal window, or this movement is within the modal window
        bool passModal = modal.Count == 0;
        at.X -= Bounds.X; at.Y -= Bounds.Y; // at is the cursor point local to 'p'
        int ei = 0;
        while(p.Enabled && p.Visible)
        {
          c = p.GetChildAtPoint(at);
          // enter/leave algorithm:
          // keep an array of the path down the control tree, from the root down
          // on mouse move, go down the tree, comparing against the stored path
          if(ei < enteredLen && c != entered[ei])
          {
            for(int i = enteredLen - 1; i >= ei; i--)
            {
              entered[i].OnMouseLeave();
              entered[i] = null;
            }
            enteredLen = ei;
          }
          if(c == null) break;
          if(!passModal && c == modal[modal.Count - 1]) passModal = true;
          if(ei == enteredLen && passModal)
          {
            if(enteredLen == entered.Length)
            {
              Control[] na = new Control[entered.Length * 2];
              Array.Copy(entered, na, enteredLen);
              entered = na;
            }
            entered[enteredLen++] = c;
            c.OnMouseEnter();
          }
          at = c.ParentToControl(at);
          if((focus == AutoFocus.OverSticky || focus == AutoFocus.Over) && c.Enabled && passModal)
          {
            c.Focus(false);
          }
          ei++;
          p = c;
        }
        // at this point, 'p' points to the control that doesn't have a child at 'at'
        // normally we'd set its FocusedControl to null to indicate this, but if there's a modal window,
        // we don't unset any focus
        if(focus == AutoFocus.Over && passModal) p.FocusedControl = null;

        if(dragging != null)
        {
          if(dragStarted)
          {
            drag.End = p == dragging ? at : dragging.ScreenToControl(ea.Point);
            dragging.OnDragMove(drag);
            if(drag.Cancel) EndDrag();
          }
          else if(capturing == null || capturing == p)
          {
            int xd = ea.X - drag.Start.X;
            int yd = ea.Y - drag.Start.Y;
            if(xd * xd + yd * yd >= (dragging.DragThreshold == -1 ? DragThreshold : p.DragThreshold))
            {
              drag.Start = dragging.ScreenToControl(drag.Start);
              drag.End = ea.Point;
              drag.Buttons = ea.Buttons;
              drag.Cancel = false;
              dragStarted = true;
              dragging.OnDragStart(drag);
              if(drag.Cancel) EndDrag();
            }
          }
        }

        if(capturing != null)
        {
          ea.Point = capturing.ScreenToControl(ea.Point);
          capturing.OnMouseMove(ea);
        }
        else if(passModal)
        {
          if(p != this)
          {
            ea.Point = at;
            p.OnMouseMove(ea);
          }
        }
        return true;
      }
      #endregion
      #region Keyboard
      else if(keys && e.Type == EventType.Keyboard)
      {
        KeyboardEvent ke = (KeyboardEvent)e;
        KeyEventArgs ea = new KeyEventArgs(ke);
        ea.KE.Mods = Input.Keyboard.Mods;
        DispatchKeyToFocused(ea);
        return true;
      }
      #endregion
      #region Mouse clicks
      else if(clicks && e.Type == EventType.MouseClick)
      {
        ClickEventArgs ea = new ClickEventArgs((MouseClickEvent)e);
        Point at = ea.CE.Point;
        // if the click is not within the desktop area, ignore it (unless dragging or capturing)
        if(capturing == null && !dragStarted && !Bounds.Contains(at)) return false;
        Control p = this, c;
        uint time = Timing.Msecs;
        bool passModal = modal.Count == 0;

        at.X -= Bounds.X; at.Y -= Bounds.Y; // at is the cursor point local to 'p'
        while(p.Enabled && p.Visible)
        {
          c = p.GetChildAtPoint(at);
          if(c == null) break;
          if(!passModal && c == modal[modal.Count - 1]) passModal = true;
          at = c.ParentToControl(at);
          if(focus == AutoFocus.Click && ea.CE.Down && c.Enabled && passModal && !ea.CE.IsMouseWheel)
          {
            c.Focus(false);
          }
          p = c;
        }
        if(p == this) // if p=='this', the desktop was clicked
        {
          if(focus == AutoFocus.Click && ea.CE.Down && FocusedControl != null && passModal) FocusedControl = null; // blur
          if(!dragStarted && capturing == null) goto done; // if we're not dragging or capturing, then we're done
        }

        if(ea.CE.Down)
        { // only consider a drag if the click occurred within the modal window, and we're not already tracking one
          if(passModal && dragging == null && p.HasStyle(ControlStyle.Draggable))
          {
            dragging = p;
            drag.Start = ea.CE.Point;
            drag.SetPressed(ea.CE.Button, true);
          }
        }
        // button released. if we haven't started dragging (only considering one) or the button was one
        // involved in the drag, then end the drag/consideration
        else if(!dragStarted || drag.Pressed(ea.CE.Button))
        {
          bool skipClick = dragStarted;
          if(dragStarted)
          {
            drag.End = dragging.ScreenToControl(ea.CE.Point);
            dragging.OnDragEnd(drag);
          }
          EndDrag();
          // if we were dragging, or the mouse was released over the desktop and we're not capturing,
          // then don't trigger any other mouse events (MouseUp or MouseClick). we're done.
          if(skipClick || (p == this && capturing == null)) goto done;
        }
        else if(p == this && capturing == null) goto done; // the mouse was released over the desktop and we're not capturing

        clickStatus = ClickStatus.All;
        if(capturing != null)
        {
          ea.CE.Point = capturing.ScreenToControl(ea.CE.Point);
          DispatchClickEvent(capturing, ea, time);
        }
        else if(passModal)
        {
          ea.CE.Point = at;
          do
          {
            if(!DispatchClickEvent(p, ea, time)) break;
            ea.CE.Point = p.ControlToParent(ea.CE.Point);
            p = p.Parent;
          } while(p != null && p != this && p.Enabled && p.Visible);
        }
        done:
        // lastClicked is used to track if the button release occurred over the same control it was pressed over
        // this allows you to press the mouse on a control, then drag off and release to avoid the MouseClick event
        if(!ea.CE.Down && (byte)ea.CE.Button < 8) lastClicked[(byte)ea.CE.Button] = null;
        return true;
      }
      #endregion
    }
    #region WindowEvent
    else if(e.Type == EventType.Control)
    {
      ControlEvent we = (ControlEvent)e;
      if(we.Control.Desktop != this && we.Control.Desktop != null)
      {
        return false;
      }
      else if(we.SubType == ControlEvent.MessageType.Custom)
      {
        we.Control.OnCustomEvent(we);
        return true;
      }

      switch(we.SubType)
      {
        case ControlEvent.MessageType.DesktopUpdated:
          if(we.Control == this)
          {
            DoLayout();
            DoPaint();
            sentUpdateEvent = false;
          }
          break;
      }
      return true;
    }
    #endregion
    return false;
  }

  protected virtual void Dispose(bool finalizing)
  {
    if(Renderer != null) Renderer.VideoModeChanged -= OnVideoModeChanged;
    if(!disposed) Events.Events.Deinitialize();
    disposed = true;
  }

  protected internal override void OnKeyDown(KeyEventArgs e)
  {
    if(!e.Handled && e.KE.Down && e.KE.Key == tab &&
       (e.KE.KeyMods == Input.KeyMod.None || e.KE.HasOnly(Input.KeyMod.Shift)))
    {
      TabToNext(e.KE.HasAny(Input.KeyMod.Shift));
      e.Handled = true;
    }

    base.OnKeyDown(e);
  }

  protected sealed override void ValidateNewParent(Control newParent)
  {
 	  throw new ArgumentException("A desktop cannot be placed within another control.");
  }

  internal Control CapturingControl
  {
    get { return capturing; }
  }

  internal void DoPaint(Control control)
  {
    DoPaint(control, control.InvalidRect, control.DoPaint, true);
  }

  internal void DoPaint(Control control, Rectangle paintArea)
  {
    DoPaint(control, paintArea, control.DoPaint, false);
  }

  internal void DoPaint(Control control, Rectangle paintArea, PaintHandler paintMethod)
  {
    DoPaint(control, paintArea, paintMethod, false);
  }

  internal bool IsCapturing(Control control)
  {
    return control != null && CapturingControl == control;
  }

  internal void NeedsLayout(Control control, bool recursive)
  {
    control.SetFlag(Flag.RecursiveLayout, recursive);
    controlsToLayOut.Add(control);
    PostUpdateEvent();
  }

  internal void NeedsRepaint(Control control)
  {
    controlsToRepaint.Add(control);
    PostUpdateEvent();
  }

  internal void SetCapture(Control control)
  {
    capturing = control;
  }

  internal void SetModal(Control control)
  {
    if(control.Desktop != this)
    {
      throw new InvalidOperationException("The control is not associated with this desktop.");
    }
    if(modal.Contains(control)) UnsetModal(control);
    modal.Add(control);
    if(capturing != control) capturing = null;
    if(dragging != null && dragging != control) EndDrag();
    control.Focus(true);
  }

  internal void UnsetModal(Control control)
  {
    if(control.Desktop != this)
    {
      throw new InvalidOperationException("The control is not associated with this desktop.");
    }
    modal.Remove(control);
    if(modal.Count > 0)
    {
      control = modal[modal.Count - 1];
      if(capturing != control) capturing = null;
      if(dragging != null && dragging != control) EndDrag();
      control.Focus(true);
    }
  }

  /// <summary>Adds a rectangle to the list of updated rectangles.</summary>
  void AddUpdatedArea(Rectangle area)
  { // TODO: combine rectangles more efficiently so there's no overlap
    int i;
    for(i=0; i < updatedLen; i++)
    {
      if(updated[i].Contains(area)) return;
      while(area.Contains(updated[i]) && --updatedLen != i) updated[i] = updated[updatedLen];
    }

    if(i == updatedLen)
    {
      if(updatedLen == updated.Length)
      {
        Rectangle[] narr = new Rectangle[updated.Length * 2];
        Array.Copy(updated, narr, updated.Length);
        updated = narr;
      }
      updated[updatedLen++] = area;
    }
  }

  void DoPaint(Control control, Rectangle paintArea, PaintHandler paintMethod, bool skipTranslucent)
  {
    if(paintArea.Width != 0 && control.Desktop == this && DrawTarget != null && Renderer != null)
    {
      PaintEventArgs pe = new PaintEventArgs(Renderer, control, paintArea);
      paintMethod(pe);

      // propogate changes up to the desktop's surface if we need to. we skip translucent controls because their
      // Invalidate() call simply invalidates the given area of the parent control
      if(control != this &&
         (control.DrawTarget != null || (skipTranslucent && pe.Target != DrawTarget && !control.IsTranslucent)))
      {
        Rectangle destRect = pe.ControlRect;
        while(control.DrawTarget == null)
        {
          destRect.Location = control.ControlToParent(destRect.Location);
          control = control.Parent;
        }

        IGuiImage srcImage = control.DrawTarget;
        Rectangle srcRect = pe.DrawRect;
        do
        {
          do
          {
            destRect.Location = control.ControlToParent(destRect.Location);
            control = control.Parent;
          } while(control.DrawTarget == null);

          Rectangle oldClipRect = control.DrawTarget.ClipRect;
          try
          {
            control.DrawTarget.ClipRect = new Rectangle(Point.Empty, control.DrawTarget.Size);
            srcImage.Draw(srcRect, control.DrawTarget, destRect);

            srcImage = control.DrawTarget;
            srcRect  = destRect;
          }
          finally { control.DrawTarget.ClipRect = oldClipRect; }

        } while(control != this);

        if(trackUpdates) AddUpdatedArea(destRect);
      }
      else if(trackUpdates) AddUpdatedArea(pe.DrawRect);

      didPainting = true;
    }
  }

  static bool DispatchKeyEvent(Control target, KeyEventArgs e)
  {
    if(e.KE.Down)
    {
      target.OnKeyDown(e);
      if(e.Handled) return false;
      if(e.KE.Char != 0) target.OnKeyPress(e);
    }
    else target.OnKeyUp(e);
    return e.Handled;
  }

  bool DispatchClickEvent(Control target, ClickEventArgs e, uint time)
  {
    if(e.CE.Down && (clickStatus & ClickStatus.UpDown) != 0)
    {
      target.OnMouseDown(e);
      if(e.Handled) { clickStatus ^= ClickStatus.UpDown; e.Handled = false; }
    }
    if(!e.CE.IsMouseWheel && (byte)e.CE.Button < 8)
    {
      if(target.HasStyle(ControlStyle.NormalClick) && (clickStatus & ClickStatus.Click) != 0)
      {
        if(e.CE.Down) lastClicked[(byte)e.CE.Button] = target;
        else
        {
          if(lastClicked[(byte)e.CE.Button] == target)
          {
            if(target.HasStyle(ControlStyle.DoubleClickable) && time - target.lastClickTime <= dcDelay)
            {
              target.OnDoubleClick(e);
            }
            else target.OnMouseClick(e);
            target.lastClickTime = time;
            if(e.Handled) { clickStatus ^= ClickStatus.Click; e.Handled = false; }
            lastClicked[(byte)e.CE.Button] = target.Parent; // allow the check to be done for the parent, too // TODO: make sure this is okay with captured/dragged controls
          }
        }
      }
      else clickStatus ^= ClickStatus.Click;
    }
    if(!e.CE.Down && (clickStatus & ClickStatus.UpDown) != 0)
    {
      if(e.CE.IsMouseWheel) e.Handled = true;
      else target.OnMouseUp(e);
      if(e.Handled) { clickStatus ^= ClickStatus.UpDown; e.Handled = false; }
    }
    return clickStatus != ClickStatus.None;
  }

  bool DispatchKeyToFocused(KeyEventArgs e)
  {
    if(!e.Handled)
    {
      Control fc = this;
      if(fc.FocusedControl != null)
      {
        do
        {
          if(fc.HasStyle(ControlStyle.CanReceiveFocus) && fc.KeyPreview && DispatchKeyEvent(fc, e)) goto done;
          fc = fc.FocusedControl;
        } while(fc.FocusedControl != null);
      }

      if((fc == this || fc.HasStyle(ControlStyle.CanReceiveFocus)) && DispatchKeyEvent(fc, e)) goto done;

      if(fc != this && !KeyPreview) DispatchKeyEvent(this, e);
    }
    
    done:
    return e.Handled;
  }

  void DoLayout()
  {
    // don't use a foreach loop in case a control calls Invalidate(), causing the enumerator to throw an exception
    LayoutEventArgs e = new LayoutEventArgs(false);
    for(int i = 0; i < controlsToLayOut.Count; i++)
    {
      Control control = controlsToLayOut[i];
      e.Recursive = control.HasFlag(Flag.RecursiveLayout);
      control.OnLayout(e);
    }
    controlsToLayOut.Clear();
  }

  void DoPaint()
  {
    // don't use a foreach loop in case a control calls Invalidate(), causing the enumerator to throw an exception
    for(int i = 0; i < controlsToRepaint.Count; i++) DoPaint(controlsToRepaint[i]);
    controlsToRepaint.Clear();
  }

  void EndDrag()
  {
    dragging = null;
    dragStarted = false;
    drag.Buttons = 0;
  }

  void OnVideoModeChanged()
  {
    UpdateDrawTargets();
  }

  void PostUpdateEvent()
  {
    if(!sentUpdateEvent)
    {
      Events.Events.PushEvent(new DesktopUpdatedEvent(this));
      sentUpdateEvent = true;
    }
  }

  void TabToNext(bool reverse)
  {
    Control fc = this;
    while(fc.FocusedControl != null) fc = fc.FocusedControl;
    (fc == this ? this : fc.Parent).TabToNextControl(reverse);
  }

  void UpdateDrawTargets()
  {
    UpdateDrawTarget(true, true);
  }

  void UpdateRenderers(Control control)
  {
    foreach(Control child in control.Controls)
    {
      child.SetRenderer(Renderer);
      UpdateRenderers(child);
    }
  }

  [Flags]
  enum ClickStatus { None = 0, UpDown = 1, Click = 2, All = UpDown | Click };

  readonly List<Control> controlsToRepaint = new List<Control>(), controlsToLayOut = new List<Control>();
  readonly List<Control> modal = new List<Control>();
  readonly DragEventArgs drag = new DragEventArgs();
  readonly System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
  Control capturing;
  Control[] lastClicked = new Control[8], entered = new Control[8];
  Control dragging;
  Rectangle[] updated = new Rectangle[8];
  Input.Key tab = Input.Key.Tab;
  AutoFocus focus = AutoFocus.Click;
  ClickStatus clickStatus;
  int enteredLen, updatedLen, dcDelay = 350, defaultDragThreshold = 16;
  bool keys = true, clicks = true, moves = true, didPainting, dragStarted, trackUpdates, sentUpdateEvent, disposed;
}
#endregion

} // namespace GameLib.Forms