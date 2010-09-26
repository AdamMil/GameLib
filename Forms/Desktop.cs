using System;
using System.Collections.Generic;
using System.Drawing;
using GameLib.Events;
using GameLib.Input;
using GameLib.Video;

namespace GameLib.Forms
{

#region AutoFocus
/// <summary>This enum is used with the <see cref="DesktopControl.AutoFocus"/> property to determine how and whether the
/// desktop will automatically focus controls.
/// </summary>
public enum AutoFocus
{
  /// <summary>The desktop will not automatically focus controls. You will have to call <see cref="Control.Focus"/> manually in
  /// order to set the focus.
  /// </summary>
  None,
  /// <summary>When a focusable control is clicked with any mouse button, the desktop will focus it.</summary>
  Click,
  /// <summary>When the mouse moves over a focusable control, it will be focused. When the mouse moves off of a control, it will
  /// be blurred, even if the mouse did not move onto another focusable control.
  /// </summary>
  Over,
  /// <summary>When the mouse moves over a focusable control, it will be focused.</summary>
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
    AutoFocus    = AutoFocus.Click;
    Desktop      = this;
    BackColor    = Color.Black;
    ForeColor    = Color.White;
    TabKey       = Key.Tab;
    ProcessKeys  = ProcessClicks = ProcessMouseMove = true;
    SetFlags(Flag.Active, true);
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

  /// <summary>Gets or sets the <see cref="AutoFocus"/> mode for this desktop. The default is <see cref="AutoFocus.Click"/>.</summary>
  public AutoFocus AutoFocus
  {
    get; set;
  }

  /// <summary>Gets or sets the double click delay in milliseconds for this desktop.</summary>
  /// <remarks>The double click delay is the maximum number of milliseconds allowed between mouse clicks for them
  /// to be recognized as a double click. The default value is 350 milliseconds.
  /// </remarks>
  public int DoubleClickDelay
  {
    get { return _dcDelay; }
    set
    {
      if(value < 0) throw new ArgumentOutOfRangeException("DoubleClickDelay", "cannot be negative");
      _dcDelay = value;
    }
  }

  /// <summary>Gets or sets the default drag threshold for this desktop.</summary>
  /// <remarks>This property provides a default drag threshold for controls that do not specify one. See
  /// <see cref="Control.DragThreshold"/> for more information about the drag threshold. The default value is 16.
  /// </remarks>
  public int DefaultDragThreshold
  {
    get { return _defaultDragThreshold; }
    set
    {
      if(value < 1) throw new ArgumentOutOfRangeException("DragThreshold", "must be >=1");
      _defaultDragThreshold = value;
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

  /// <summary>Gets or sets whether this desktop will process keyboard events. The default value is true.</summary>
  /// <remarks>If false, <see cref="ProcessEvent(Event,bool)"/> will ignore events related to the keyboard.</remarks>
  public bool ProcessKeys
  {
    get; set;
  }

  /// <summary>Gets or sets whether this desktop will process mouse movement events. The default value is true.</summary>
  /// <remarks>If false, <see cref="ProcessEvent(Event,bool)"/> will ignore events related to mouse movement.</remarks>
  public bool ProcessMouseMove
  {
    get; set;
  }

  /// <summary>Gets or sets whether this desktop will process mouse click events. The default value is true.</summary>
  /// <remarks>If false, <see cref="ProcessEvent(Event,bool)"/> will ignore events related to mouse clicks.</remarks>
  public bool ProcessClicks
  {
    get; set;
  }

  /// <summary>Gets or sets the renderer used to paint controls on this desktop. This property must be set before the
  /// desktop can be used.
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

  /// <summary>Gets or sets the key used to tab between controls. The default is <see cref="Key.Tab"/>.</summary>
  /// <remarks>If this property is set to a value other than <see cref="Key.None"/>, that key will be used
  /// to move input focus between controls. When that key is pressed, the desktop will call
  /// <see cref="Control.TabToNextControl"/> on the control that currently has input focus.
  /// </remarks>
  public Key TabKey
  {
    get; set;
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
    get { return _trackUpdates; }
    set
    {
      _trackUpdates = value;
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
    get { return _didPainting; }
    set
    {
      if(value != Updated)
      {
        _didPainting = value;
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
  /// <returns>Returns true if the event was handled by the desktop, and false otherwise. See
  /// <see cref="ProcessEvent(Event,bool)"/> for more information about the return value.
  /// </returns>
  /// <remarks>Calling this method is equivalent to calling <see cref="ProcessEvent(Event,bool)"/> and passing
  /// true to allow it to update the <see cref="Input.Input"/> class. This method should be used with care.
  /// See <see cref="ProcessEvent(Event,bool)"/> for more information about proper usage of this method.
  /// </remarks>
  public bool ProcessEvent(Event e)
  {
    return ProcessEvent(e, true);
  }

  /// <summary>Processes the specified event.</summary>
  /// <param name="e">The <see cref="Event"/> to process.</param>
  /// <param name="passToInput">If true, the event is first passed to <see cref="Input.Input.ProcessEvent"/>. It is important
  /// that this happen, but it should not be done more than once to avoid confusing the <see cref="Input.Input"/> class.
  /// Thus, if you have multiple desktops or want to update the <see cref="Input.Input"/> class yourself, you should manually
  /// pass the event to <see cref="Input.Input.ProcessEvent"/> and then call this method for each desktop, passing false for this
  /// parameter. If you have only a single desktop, you can safely pass true for <paramref name="passToInput"/>, assuming you
  /// don't call <see cref="Input.Input.ProcessEvent"/> yourself.
  /// </param>
  /// <returns>Returns true if the event was handled by the desktop, and false otherwise. A return value of true
  /// does not necessarily mean that the event had an effect on this desktop, only that it might have had an effect.
  /// Thus, the event should still be passed to all other desktops, if there are multiple.
  /// </returns>
  /// <remarks>The main application event handler should pass events to this method in the order they are received. The desktop
  /// will use them to handle all user interaction with the desktop and its controls. 
  /// <seealso cref="Events.Events"/> <seealso cref="Input.Input.ProcessEvent"/>
  /// </remarks>
  public bool ProcessEvent(Event e, bool passToInput)
  {
    if(!passToInput || Input.Input.ProcessEvent(e))
    {
      #region Mouse moves
      if(ProcessMouseMove && e.Type == EventType.MouseMove)
      {
        MouseMoveEvent me = (MouseMoveEvent)e;
        Point at = me.Point;
        // if the cursor is not within the desktop area, ignore it (unless dragging (or potentially dragging) or capturing)
        if(dragging == null && capturing == null && !Bounds.Contains(at)) return false;
        at = ScreenToControl(at); // now make 'at' in control coordinates relative to the desktop

        Control control = this, child;
        // passModal will be set to true if there's no modal window, or this movement is within the modal window
        bool passModal = modal.Count == 0, shouldFocusIfSticky = true;
        for(int ei=0; control.HasFlags(Flag.Enabled | Flag.Visible); ei++, control = child)
        {
          child = control.GetChildAtPoint(at);

          // enter/leave algorithm: keep an array of the path down the control tree, from the root down.
          // on mouse move, go down the tree, comparing against the stored path
          if(ei < enteredLen && child != entered[ei]) // if the control is not the one we are previously recorded as being in
          {
            for(int i = enteredLen - 1; i >= ei; i--) // then leave all controls from this point on
            {
              entered[i].OnMouseLeave();
              entered[i] = null;
            }
            enteredLen = ei;
          }

          if(child == null) break;
          if(!passModal && child == modal[modal.Count - 1]) passModal = true;

          if(ei == enteredLen && passModal) // if we haven't entered this child yet, do it now
          {
            AdamMil.Utilities.Utility.EnlargeArray(ref entered, enteredLen, 1);
            entered[enteredLen++] = child;
            child.OnMouseEnter();
          }

          at = child.ParentToControl(at); // convert 'at' to the child's control coordinates
          
          // if we focus on mouse over, try to select the current child
          if(passModal)
          {
            if(AutoFocus == AutoFocus.Over && passModal && child.Selectable) child.Select(false);
            else if(AutoFocus == AutoFocus.OverSticky && !child.Selectable) shouldFocusIfSticky = false;
          }
        }

        // at this point, 'control' points to the control that doesn't have a child at 'at'.
        if(passModal)
        {
          if(AutoFocus == AutoFocus.Over ||
             AutoFocus == AutoFocus.OverSticky && shouldFocusIfSticky && control.HasStyle(ControlStyle.CanReceiveFocus))
          {
            control.SelectedControl = null; // deselect whatever was selected because there's no child under the mouse now
            control.Focus(); // and try to focus the control that it is over
          }
        }

        if(dragging != null) // if we're dragging a control or considering dragging it...
        {
          if(dragStarted) // and the drag already started (i.e. we're currently dragging it)...
          {
            drag.End = control == dragging ? at : dragging.ScreenToControl(me.Point); // compute the current drag point
            dragging.OnDragMove(drag);
            if(drag.Cancel) EndDrag();
          }
          // otherwise, we're only considering dragging it. so if the mouse isn't captured (or is captured by 'control')...
          else if(capturing == null || capturing.IsAncestorOf(control, true))
          {
            // then see if we should start the drag
            int xd = me.X - drag.Start.X, yd = me.Y - drag.Start.Y;
            if(xd*xd + yd*yd >= (dragging.DragThreshold == -1 ? DefaultDragThreshold : control.DragThreshold))
            {
              // we kept track of the start position in screen coordinates, so convert it to control coordinate now
              drag.Start   = dragging.ScreenToControl(drag.Start);
              drag.End     = me.Point; // then keep track of the end position in screen coordinates
              drag.Buttons = me.Buttons;
              drag.Cancel  = false;
              dragStarted = true; // indicate that the drag has started
              dragging.OnDragStart(drag);
              if(drag.Cancel) EndDrag();
            }
          }
        }

        if(capturing != null) // if a control has mouse capture...
        {
          // dispatch events normally if 'control' is a descendant of the capturing control. otherwise send to 'capturing'
          Control dispatchTo = capturing.IsAncestorOf(control, true) ? control : capturing;
          me.Point = dispatchTo == control ? at : dispatchTo.ScreenToControl(me.Point);
          dispatchTo.OnMouseMove(me);
        }
        else if(passModal && control.HasFlags(Flag.EffectivelyEnabled | Flag.EffectivelyVisible))
        {
          me.Point = at;
          control.OnMouseMove(me);
        }
        return true;
      }
      #endregion
      #region Keyboard
      else if(ProcessKeys && e.Type == EventType.Keyboard)
      {
        KeyEventArgs ea = new KeyEventArgs((KeyboardEvent)e);
        ea.KE.Mods = Keyboard.Mods;
        DispatchKeyToFocused(ea);
        return true;
      }
      #endregion
      #region Mouse clicks
      else if(ProcessClicks && e.Type == EventType.MouseClick)
      {
        MouseClickEvent me = (MouseClickEvent)e;
        Point at = me.Point;
        // if the click is not within the desktop area, ignore it (unless dragging or capturing)
        if(!dragStarted && capturing == null && !Bounds.Contains(at)) return false;
        at = ScreenToControl(at);

        Control control = this;
        bool tryToFocus = AutoFocus == AutoFocus.Click && me.Down && !me.IsMouseWheel;
        bool passModal = modal.Count == 0; // passModal is true if there's no modal window, or the click is within it
        for(Control child; control.Enabled && control.Visible; control = child)
        {
          child = control.GetChildAtPoint(at);
          if(child == null) break;
          if(!passModal && child == modal[modal.Count - 1]) passModal = true;
          at = child.ParentToControl(at);

          // if we focus on mouse click, try to select the current child
          if(tryToFocus && passModal && child.Selectable) child.Select(false);
        }

        // at this point, 'control' points to the control that doesn't have a child at 'at'.
        if(tryToFocus && passModal) control.Focus();

        // TODO: we should avoid trying to drag mouse wheel movements
        if(me.Down) // if the mouse button has been depressed...
        { 
          // consider a drag if the click passed the modal check and and we're not already tracking a drag
          if(passModal && dragging == null && control.HasStyle(ControlStyle.Draggable))
          {
            dragging = control;
            drag.Start = me.Point; // keep track of the start position in screen coordinates
            drag.SetPressed(me.Button, true); // pass the button that started the drag to the DragEventArgs
          }
        }
        // the button was released. if we haven't started dragging or we did start and the button was one that started the drag,
        // then end the drag (or drag consideration)
        else if(!dragStarted || drag.Pressed(me.Button))
        {
          bool skipClick = dragStarted; // whether we'll skip the later mouse click events
          if(dragStarted) // if an actual drag was started, signal that it's ending
          {
            drag.End = dragging.ScreenToControl(me.Point);
            dragging.OnDragEnd(drag);
          }
          EndDrag();

          // if we were dragging, don't trigger any other mouse events (MouseUp or MouseClick)
          if(skipClick) goto done;
        }

        ClickEventArgs ea = new ClickEventArgs(me);
        uint time = Timing.InternalMilliseconds;
        mouseUpDownHandled = mouseClickHandled = false; // reset the handler flags in preparation for calling DispatchClickEvent
        if(capturing != null) // if a control has mouse capture...
        {
          // dispatch to it (or a descendant)
          Control dispatchTo = capturing.IsAncestorOf(control, true) ? control : capturing;
          me.Point = dispatchTo == control ? at : dispatchTo.ScreenToControl(me.Point);
          DispatchClickEvent(dispatchTo, ea, time);
        }
        else if(passModal)
        {
          // dispatch the click to 'control', and let it bubble up the control tree until somebody handles it
          me.Point = at;
          do
          {
            if(control.HasFlags(Flag.EffectivelyEnabled | Flag.EffectivelyVisible) && DispatchClickEvent(control, ea, time))
            {
              break;
            }

            me.Point = control.ControlToParent(me.Point);
            control  = control.Parent;
          } while(control != null);
        }

        done:
        // lastClicked is used to track if the button release occurred over the same control it was pressed over.
        // this allows you to press the mouse on a control, then drag off and release to avoid the MouseClick event
        if(!me.Down && (byte)me.Button < 8) lastClicked[(byte)me.Button] = null;
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
    if(!disposed)
    {
      DisposeAll(false);
      if(Renderer != null) Renderer.VideoModeChanged -= OnVideoModeChanged;
      Events.Events.Deinitialize();
      disposed = true;
    }
  }

  protected internal override void OnKeyDown(KeyEventArgs e)
  {
    if(!e.Handled && e.KE.Down && e.KE.Key == TabKey &&
       (e.KE.KeyMods == KeyMod.None || e.KE.HasOnly(KeyMod.Shift)))
    {
      TabToNext(e.KE.HasAny(KeyMod.Shift));
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
    DoPaint(control, control.InvalidRect, null, true);
  }

  internal void DoPaint(Control control, Rectangle paintArea)
  {
    DoPaint(control, paintArea, null, false);
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
    control.SetFlags(Flag.RecursiveLayout, recursive);
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
    control.Focus();
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
      control.Focus();
    }
  }

  /// <summary>Adds a rectangle to the list of updated rectangles.</summary>
  void AddUpdatedArea(Rectangle area)
  { 
    // TODO: combine rectangles more efficiently so there's no overlap
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

      // we treat null as referencing a control's DoPaint method because we don't want to make callers allocate a new
      // delegate every time a control gets painted with its normal paint method
      if(paintMethod != null) paintMethod(pe);
      else control.DoPaint(pe);

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

        if(_trackUpdates) AddUpdatedArea(destRect);
      }
      else if(_trackUpdates) AddUpdatedArea(pe.DrawRect);

      _didPainting = true;
    }
  }

  static bool DispatchKeyEvent(Control target, KeyEventArgs e)
  {
    if(e.KE.Down)
    {
      target.OnKeyDown(e);
      if(!e.Handled && e.KE.Char != 0) target.OnKeyPress(e);
    }
    else
    {
      target.OnKeyUp(e);
    }
    return e.Handled;
  }

  bool DispatchClickEvent(Control target, ClickEventArgs e, uint time)
  {
    // if the MouseUp/MouseDown/MouseWheel event hasn't been handled yet, try to handle it now
    if(!mouseUpDownHandled)
    {
      if(e.CE.IsMouseWheel) target.OnMouseWheel(e);
      else if(e.CE.Down) target.OnMouseDown(e);
      else target.OnMouseUp(e);

      if(e.Handled) // if the event was handled, mark it as such, so we don't try to handle it again
      {
        mouseUpDownHandled = true;
        e.Handled = false; // then mark e.Handled as false for when we later call OnClick, OnDoubleClick, etc.
      }
    }

    // if we haven't handled the click events, and it's a normal button, and the target accepts click/doubleclick events...
    if(!mouseClickHandled && !e.CE.IsMouseWheel && (byte)e.CE.Button < 8 && target.HasStyle(ControlStyle.NormalClick))
    {
      if(e.CE.Down) // if it was pressed...
      {
        lastClicked[(byte)e.CE.Button] = target; // keep track of the control it was pressed on
      }
      else if(lastClicked[(byte)e.CE.Button] == target) // if it was released over the same control it was pressed on...
      {
        // then handle either it as a click or double click
        if(target.HasStyle(ControlStyle.DoubleClickable) && time - target.lastClickTime <= DoubleClickDelay)
        {
          target.OnDoubleClick(e);
        }
        else
        {
          target.OnMouseClick(e);
        }

        target.lastClickTime = time; // keep track of the last time we clicked on the control

        if(e.Handled) // if the event was handled...
        {
          mouseClickHandled = true; // mark it as such, so we don't try to handle it again
          lastClicked[(byte)e.CE.Button] = null; // and forget which control we last clicked on
        }
        else
        {
          // TODO: make sure this is okay with captured/dragged controls
          lastClicked[(byte)e.CE.Button] = target.Parent; // allow the check to be done for the parent, too
        }
      }
    }

    return mouseUpDownHandled && mouseClickHandled;
  }

  void DispatchKeyToFocused(KeyEventArgs e)
  {
    Control c = this;

    // move down the control tree, dispatching the key to controls with key preview until we find a focused control
    while(c.SelectedControl != null && c.HasFlags(Flag.EffectivelyEnabled | Flag.EffectivelyVisible) && !c.Focused)
    {
      if(c.KeyPreview && DispatchKeyEvent(c, e)) return;
      c = c.SelectedControl;
    }

    // if we found a focused control, dispatch it there
    if(c.Focused && DispatchKeyEvent(c, e)) return;

    // otherwise, it we didn't find a focused control to dispatch it to, so dispatch it to the desktop if it's enabled and it
    // hasn't already received it (via key preview)
    if(HasFlags(Flag.EffectivelyEnabled | Flag.EffectivelyVisible) && !KeyPreview) DispatchKeyEvent(this, e);
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
    while(fc.SelectedControl != null) fc = fc.SelectedControl;
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

  readonly List<Control> controlsToRepaint = new List<Control>(), controlsToLayOut = new List<Control>();
  readonly List<Control> modal = new List<Control>();
  readonly DragEventArgs drag = new DragEventArgs();
  readonly System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
  Control capturing;
  Control[] lastClicked = new Control[8], entered = new Control[8];
  Control dragging;
  Rectangle[] updated = new Rectangle[8];
  bool mouseUpDownHandled, mouseClickHandled;
  int enteredLen, updatedLen, _dcDelay = 350, _defaultDragThreshold = 16;
  bool _didPainting, dragStarted, _trackUpdates, sentUpdateEvent, disposed;
}
#endregion

} // namespace GameLib.Forms