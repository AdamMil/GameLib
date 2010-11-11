/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2010 Adam Milazzo

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
using GameLib.Input;

namespace GameLib.Forms
{

#region LayoutPanel
/// <summary>Provides a base class for controls that lay out their children in particular ways. The base class ensures that
/// layout is triggered at appropriate times, such as when children are added, resized, or removed.
/// </summary>
public abstract class LayoutPanel : Control
{
  protected override void OnControlAdded(ControlEventArgs e)
  {
    base.OnControlAdded(e);
    if(!e.Control.HasStyle(ControlStyle.DontLayout)) TriggerLayout();
    e.Control.Resize += Child_Resized;
  }

  protected override void OnControlRemoved(ControlEventArgs e)
  {
    e.Control.Resize -= Child_Resized;
    base.OnControlRemoved(e);
    if(!e.Control.HasStyle(ControlStyle.DontLayout)) TriggerLayout();
  }

  void Child_Resized(object sender, EventArgs e)
  {
    Control control = (Control)sender;
    if(!control.HasStyle(ControlStyle.DontLayout)) TriggerLayout();
  }
}
#endregion

#region StackPanel
/// <summary>Implements a <see cref="LayoutPanel"/> that stacks its children either horizontally or vertically.</summary>
public class StackPanel : LayoutPanel
{
  /// <summary>Initializes a new vertical <see cref="StackPanel"/>.</summary>
  public StackPanel()
  {
    AutoSize    = true;
    Orientation = Orientation.Vertical;
  }

  /// <summary>Gets or sets whether the size of the <see cref="StackPanel"/> adjusts to match the total size of the contained
  /// controls. Setting this to false increases the performance of control layout, but requries you to size the
  /// <see cref="StackPanel"/> appropriately. The default is true.
  /// </summary>
  public bool AutoSize
  {
    get { return _autoSize; }
    set
    {
      if(value != AutoSize)
      {
        _autoSize = value;
        if(value) TriggerLayout();
      }
    }
  }

  /// <summary>Gets or sets the orientation by which child controls are stacked. The default value is
  /// <see cref="Forms.Orientation.Vertical"/>.
  /// </summary>
  public Orientation Orientation
  {
    get { return _orientation; }
    set
    {
      if(value != Orientation)
      {
        _orientation = value;
        TriggerLayout();
      }
    }
  }

  protected override void LayOutChildren()
  {
    if(AutoSize) // if we need to match our size to the size of the children...
    {
      // calculate the total and maximum sizes of all child controls that respond to layout
      Size totalSize = new Size(), maxSize = new Size();
      foreach(Control child in Controls)
      {
        if(!child.HasStyle(ControlStyle.DontLayout))
        {
          totalSize.Width  += child.Width;
          totalSize.Height += child.Height;
          maxSize.Width  = Math.Max(maxSize.Width, child.Width);
          maxSize.Height = Math.Max(maxSize.Height, child.Height);
        }
      }

      // select the desired size based on the orientation
      Size desiredSize = Orientation == Orientation.Horizontal ?
        new Size(totalSize.Width, maxSize.Height) : new Size(maxSize.Width, totalSize.Height);

      // account for padding, border, etc
      desiredSize = ContentOffset.Grow(desiredSize);

      // then make sure our size equals the desired size
      if(desiredSize != Size)
      {
        Size = desiredSize;
        if(HasFlag(Flag.PendingLayout)) return; // if changing the size means layout will be called again, just wait for that
      }
    }

    // stack up the children
    Rectangle start = ContentRect;
    foreach(Control child in Controls)
    {
      child.SetBounds(new Rectangle(start.X, start.Y, child.Width, child.Height), true);
      if(Orientation == Orientation.Horizontal) start.X += child.Width;
      else start.Y += child.Height;
    }
  }

  Orientation _orientation;
  bool _autoSize;
}
#endregion

#region FixedPanel
/// <summary>Determines which panel(s) will be resized a <see cref="SplitContainer"/> is resized, and which panel will preferably
/// kept at a constant size.
/// </summary>
public enum FixedPanel
{
  /// <summary>No panel is fixed, so both panels will be affected by the resize.</summary>
  None=0,
  /// <summary><see cref="SplitContainer.Panel1"/> is fixed, so only <see cref="SplitContainer.Panel2"/> will be affected by the
  /// resize, and various operations will attempt to maintain the size of <see cref="SplitContainer.Panel1"/>.
  /// </summary>
  Panel1,
  /// <summary><see cref="SplitContainer.Panel2"/> is fixed, so only <see cref="SplitContainer.Panel1"/> will be affected by the
  /// resize, and various operations will attempt to maintain the size of <see cref="SplitContainer.Panel2"/>.
  /// </summary>
  Panel2
}
#endregion

#region SplitContainer
/// <summary>Implements a control that divides its content area into two resizable panels, divided by a movable bar.</summary>
public class SplitContainer : Control
{
  /// <summary>Initializes a new vertical <see cref="SplitContainer"/>.</summary>
  public SplitContainer()
  {
    const int DefaultSplitterWidth = 4;
    previousSize = Size;

    ControlStyle     = ControlStyle.CanReceiveFocus | ControlStyle.Draggable;
    Panel1           = new Control() { Height = Height, Width = ContentRect.Width/2 - DefaultSplitterWidth/2 };
    Panel2           = new Control() { Height = Height, Width = ContentRect.Width - Panel1.Width - DefaultSplitterWidth };
    Controls.AddRange(Panel1, Panel2);

    SplitterWidth    = 4;
    Orientation      = Orientation.Vertical;
    SplitterLocation = Panel1.Width;
  }

  /// <summary>Gets or sets whether the splitter bar is fixed in position and cannot be moved by the user. The default is false.</summary>
  public bool IsSplitterFixed
  {
    get { return _isSplitterFixed; }
    set
    {
      if(value != IsSplitterFixed)
      {
        _isSplitterFixed = value;
        if(!value && Focused) Blur();
      }
    }
  }

  /// <summary>Gets or sets the <see cref="Forms.Orientation"/> of the splitter bar. The default is
  /// <see cref="Forms.Orientation.Vertical"/>.
  /// </summary>
  public Orientation Orientation
  {
    get { return _orientation; }
    set
    {
      if(value != Orientation)
      {
        _orientation = value;
        LayoutPanels();
      }
    }
  }

  /// <summary>Gets the top or left panel, depending on the <see cref="Orientation"/>. Do not change the panel's manually.</summary>
  public Control Panel1
  {
    get; private set;
  }

  /// <summary>Gets or sets whether the top or left panel, depending on the <see cref="Orientation"/>, is collapsed and hidden.</summary>
  public bool Panel1Collapsed
  {
    get { return _panel1Collapsed; }
    set
    {
      if(value != Panel1Collapsed)
      {
        _panel1Collapsed = value;
        Panel1.Visible = !value;
        LayoutPanels();
        Invalidate();
      }
    }
  }

  /// <summary>Gets the minimum size that the top or left panel, depending on the <see cref="Orientation"/>, is allowed to be.</summary>
  public int Panel1MinSize
  {
    get { return _panel1MinSize; }
    set
    {
      _panel1MinSize = value;
      if(Panel1Size < value) Panel1Size = value;
    }
  }

  /// <summary>Gets the bottom or right panel, depending on the <see cref="Orientation"/>. Do not change the panel's manually.</summary>
  public Control Panel2
  {
    get; private set;
  }

  /// <summary>Gets or sets whether the bottom or right panel, depending on the <see cref="Orientation"/>, is collapsed and
  /// hidden.
  /// </summary>
  public bool Panel2Collapsed
  {
    get { return _panel2Collapsed; }
    set
    {
      if(value != Panel2Collapsed)
      {
        _panel2Collapsed = value;
        Panel2.Visible = !value;
        LayoutPanels();
        Invalidate();
      }
    }
  }

  /// <summary>Gets the minimum size that the bottom or right panel, depending on the <see cref="Orientation"/>,
  /// is allowed to be.
  /// </summary>
  public int Panel2MinSize
  {
    get { return _panel2MinSize; }
    set
    {
      _panel2MinSize = value;
      if(Panel2Size < value) Panel2Size = value;
    }
  }

  /// <summary>Gets or sets which panel the <see cref="SplitContainer"/> will attempt to maintain at a constant size.</summary>
  public FixedPanel FixedPanel
  {
    get; set;
  }

  /// <summary>Gets the splitter's location. This is the distance in pixels from the top or left side of the
  /// <see cref="SplitContainer"/>, depending on the <see cref="Orientation"/>.
  /// </summary>
  public int SplitterLocation
  {
    get { return Panel1Size; } // the splitter location is equivalent to the size of the first panel
    set
    {
      if(value < 0 || value >= TotalSize) throw new ArgumentOutOfRangeException();
      Panel1Size = value;
    }
  }

  /// <summary>Gets the width of the splitter bar in pixels. The default value is 4.</summary>
  public int SplitterWidth
  {
    get { return _splitterWidth; }
    set
    {
      if(value != _splitterWidth)
      {
        if(value < 0) throw new ArgumentOutOfRangeException();
        _splitterWidth = value;
        LayoutPanels();
      }
    }
  }

  /// <summary>Gets the area of the control that is occupied by the splitter. The returned rectangle will have zero size if the
  /// splitter is not currently displayed (e.g. because one of the panels is collapsed).
  /// </summary>
  protected Rectangle SplitterRect
  {
    get
    {
      if(Panel1Collapsed || Panel2Collapsed)
      {
        return new Rectangle();
      }
      else
      {
        Rectangle availableArea = ContentRect;
        if(Orientation == Orientation.Horizontal)
        {
          return new Rectangle(availableArea.Left, availableArea.Top + Panel1Size, availableArea.Width, SplitterWidth);
        }
        else
        {
          return new Rectangle(availableArea.Left + Panel1Size, availableArea.Top, SplitterWidth, availableArea.Height);
        }
      }
    }
  }

  protected override void LayOutChildren()
  {
    LayoutPanels();
  }

  protected internal override void OnDragStart(DragEventArgs e)
  {
    base.OnDragStart(e);
    if(IsSplitterFixed || !e.Pressed(MouseButton.Left) || !SplitterRect.Contains(e.Start)) e.Cancel = true;
    else dragPosition = Orientation == Orientation.Horizontal ? e.Start.Y : e.Start.X;
  }

  protected internal override void OnDragMove(DragEventArgs e)
  {
    base.OnDragMove(e);

    Rectangle contentRect = ContentRect;
    int newDragPosition, minDragPosition, maxDragPosition;
    if(Orientation == Orientation.Horizontal)
    {
      newDragPosition = e.End.Y;
      minDragPosition = ContentRect.Top;
      maxDragPosition = Math.Max(0, ContentRect.Bottom - 1);
    }
    else
    {
      newDragPosition = e.End.X;
      minDragPosition = ContentRect.Left;
      maxDragPosition = Math.Max(0, ContentRect.Right - 1);
    }

    minDragPosition += Panel1MinSize;
    maxDragPosition -= Panel2MinSize;

    if(newDragPosition < minDragPosition) newDragPosition = minDragPosition;
    else if(newDragPosition >= maxDragPosition) newDragPosition = maxDragPosition;

    if(newDragPosition != dragPosition)
    {
      TryMoveSplitter(SplitterLocation + newDragPosition - dragPosition);
      dragPosition = newDragPosition;
    }
  }

  protected internal override void OnDragEnd(DragEventArgs e)
  {
    base.OnDragEnd(e);
    Blur();
  }

  protected override void OnGotFocus()
  {
    base.OnGotFocus();
    Invalidate(SplitterRect);
  }

  protected override void OnLostFocus()
  {
    base.OnLostFocus();
    Invalidate(SplitterRect);
  }

  protected internal override void OnKeyDown(KeyEventArgs e)
  {
    base.OnKeyDown(e);

    if(!IsSplitterFixed && !e.Handled && e.KE.KeyMods == KeyMod.None)
    {
      if(e.KE.Key == Key.Home)
      {
        TryMoveSplitter(0);
      }
      else if(e.KE.Key == Key.End)
      {
        TryMoveSplitter(TotalSize-1);
      }
      else if(e.KE.Key == Key.PageUp)
      {
        TryMoveSplitter(SplitterLocation - (TotalSize+9)/10);
      }
      else if(e.KE.Key == Key.PageDown)
      {
        TryMoveSplitter(SplitterLocation + (TotalSize+9)/10);
      }
      else if(Orientation == Orientation.Horizontal)
      {
        if(e.KE.Key == Key.Up) TryMoveSplitter(SplitterLocation-1);
        else if(e.KE.Key == Key.Down) TryMoveSplitter(SplitterLocation+1);
        else return;
      }
      else
      {
        if(e.KE.Key == Key.Left) TryMoveSplitter(SplitterLocation-1);
        else if(e.KE.Key == Key.Right) TryMoveSplitter(SplitterLocation+1);
        else return;
      }

      e.Handled = true;
    }
  }

  protected internal override void OnMouseDown(ClickEventArgs e)
  {
    base.OnMouseDown(e);

    // don't allow the user to focus the control by clicking anywhere except the splitter
    if(!e.Handled && Focused && (IsSplitterFixed || !SplitterRect.Contains(e.CE.Point))) Blur();
  }

  protected override void OnPaint(PaintEventArgs e)
  {
    base.OnPaint(e);

    if(Focused)
    {
      Rectangle splitterRect = SplitterRect;
      if(splitterRect.Width != 0 && splitterRect.Height != 0)
      {
        e.Renderer.DrawFocusRect(this, e, ControlToDraw(splitterRect), GetEffectiveBorderColor());
      }
    }
  }

  protected override void OnResize()
  {
    base.OnResize();

    int previous, newSize;
    if(Orientation == Orientation.Horizontal)
    {
      previous = previousSize.Height;
      newSize  = Height;
    }
    else
    {
      previous = previousSize.Width;
      newSize  = Width;
    }

    SetNewSizes(previous, newSize, FixedPanel);
    previousSize = Size; // remember the previous size
  }

  /// <summary>Called after the splitter bar has been moved by the user. When overriding this method, be sure to call the base
  /// class to allow default processing to occur.
  /// </summary>
  protected virtual void OnSplitterMoved() { }

  int Panel1Size
  {
    get { return Orientation == Orientation.Horizontal ? Panel1.Height : Panel1.Width; }
    set { SetPanel1Size(value, true); }
  }

  int Panel2Size
  {
    get { return Orientation == Orientation.Horizontal ? Panel2.Height : Panel2.Width; }
    set { SetPanel2Size(value, true); }
  }

  int TotalSize
  {
    get
    {
      Size contentSize = ContentRect.Size;
      return Math.Max(0, (Orientation == Orientation.Horizontal ? contentSize.Height : contentSize.Width) - SplitterWidth);
    }
  }

  void LayoutPanels()
  {
    LayoutPanels(FixedPanel.None);
  }

  void LayoutPanels(FixedPanel panelToPreserve)
  {
    // if panelToPreserve is not set, use the value from FixedPanel
    if(panelToPreserve == FixedPanel.None) panelToPreserve = FixedPanel;

    Size availableArea = ContentRect.Size;
    int availableSize = Orientation == Orientation.Horizontal ? availableArea.Height : availableArea.Width;
    SetNewSizes(Panel1Size + SplitterWidth + Panel2Size, availableSize, panelToPreserve);
  }

  /// <summary>Adjusts the sizes of the panels given a change in the available space, and assuming that the panels are already
  /// sized correctly for the old available space.
  /// </summary>
  void SetNewSizes(int previousSize, int newSize, FixedPanel panelToPreserve)
  {
    // this method assumes that the control is already properly laid out, so we can just adjust things from that starting point
    if(Panel1Collapsed || Panel2Collapsed) // if at least one panel is collapsed...
    {
      Size availableArea = ContentRect.Size;
      int availableSize = Orientation == Orientation.Horizontal ? availableArea.Height : availableArea.Width;
      if(!Panel1Collapsed) SetPanel1Size(availableSize, false);
      if(!Panel2Collapsed) SetPanel2Size(availableSize, false);
    }
    else
    {
      int panel1Adjustment, panel2Adjustment, sizeDifference = newSize - previousSize;
      if(panelToPreserve == FixedPanel.None) // if neither panel is fixed...
      {
        // we'll split the difference in size between the two
        panel1Adjustment = panel2Adjustment = sizeDifference / 2;

        // because we want repeated resizes in odd increments to spread the size delta evenly, we'll add the extra pixels to
        // either the left or right pane depending on whether the old size was even or odd
        if((sizeDifference & 1) != 0) // if the size difference is odd...
        {
          int adjustment = Math.Sign(sizeDifference);
          if((previousSize & 1) == 0) panel1Adjustment += adjustment;
          else panel2Adjustment += adjustment;
        }
      }
      else if(panelToPreserve == FixedPanel.Panel1) // if we want to keep panel1 fixed in size...
      {
        panel1Adjustment = 0;
        panel2Adjustment = sizeDifference; // given the entire adjustment to panel2
      }
      else // we want to keep panel2 fixed in size
      {
        panel1Adjustment = sizeDifference; // so give give the entire adjustment to panel1
        panel2Adjustment = 0;
      }

      int newPanel1Size = Panel1Size + panel1Adjustment, newPanel2Size = Panel2Size + panel2Adjustment;
      if(panelToPreserve == FixedPanel.Panel2) LimitNewSizes(Panel2MinSize, Panel1MinSize, ref newPanel2Size, ref newPanel1Size);
      else LimitNewSizes(Panel1MinSize, Panel2MinSize, ref newPanel1Size, ref newPanel2Size);

      SetPanel1Size(newPanel1Size, false);
      SetPanel2Size(newPanel2Size, false);
    }

    SetPanelBounds();
  }

  void SetPanel1Size(int size, bool doLayout)
  {
    if(Orientation == Orientation.Horizontal) Panel1.Height = size;
    else Panel1.Width = size;
    if(doLayout) LayoutPanels(FixedPanel.Panel1);
    // if the size wasn't changed by LayoutPanels(), then OnSplitterMoved() wasn't called, so we need to call it ourselves
    if(Panel1Size == size) OnSplitterMoved();
  }

  void SetPanel2Size(int size, bool doLayout)
  {
    if(Orientation == Orientation.Horizontal) Panel2.Height = size;
    else Panel2.Width = size;
    if(doLayout) LayoutPanels(FixedPanel.Panel2);
  }

  /// <summary>Ensures that the location and the non-split dimension of the panels is correct.</summary>
  void SetPanelBounds()
  {
    Rectangle availableArea = ContentRect;
    if(Orientation == Orientation.Horizontal)
    {
      Panel1.Bounds = new Rectangle(availableArea.Left, availableArea.Top, availableArea.Width, Panel1.Height);
      Rectangle bounds = new Rectangle(availableArea.Left, availableArea.Top, availableArea.Width, Panel2.Height);
      if(!Panel1Collapsed) bounds.Y += Panel1.Height + SplitterWidth;
      Panel2.Bounds = bounds;
    }
    else
    {
      Panel1.Bounds = new Rectangle(availableArea.Left, availableArea.Top, Panel1.Width, availableArea.Height);
      Rectangle bounds = new Rectangle(availableArea.Left, availableArea.Top, Panel2.Width, availableArea.Height);
      if(!Panel1Collapsed) bounds.X += Panel1.Width + SplitterWidth;
      Panel2.Bounds = bounds;
    }
  }

  void TryMoveSplitter(int newSplitterLocation)
  {
    int totalSize = TotalSize;
    if(newSplitterLocation < 0) newSplitterLocation = 0;
    else if(newSplitterLocation >= totalSize) newSplitterLocation = totalSize;

    // it's possible that totalSize == 0, so no splitter locations are valid. in that case, we shouldn't set the location at all
    if(newSplitterLocation < totalSize) SplitterLocation = newSplitterLocation;
  }

  Size previousSize;
  int _panel1MinSize, _panel2MinSize, _splitterWidth, dragPosition;
  Orientation _orientation;
  bool _panel1Collapsed, _panel2Collapsed, _isSplitterFixed;

  static void LimitNewSizes(int preferredMinSize, int otherMinSize, ref int preferredNewSize, ref int otherNewSize)
  {
    // if the preferred panel is too small, enlarge it at the expense of the other panel
    if(preferredNewSize < preferredMinSize)
    {
      otherNewSize    -= preferredMinSize - preferredNewSize;
      preferredNewSize = preferredMinSize;
    }

    // do the same for the other panel, but without taking too much from the preferred panel
    if(otherNewSize < otherMinSize)
    {
      int adjustment = Math.Min(otherMinSize - otherNewSize, preferredNewSize - preferredMinSize);
      otherNewSize     += adjustment;
      preferredNewSize -= adjustment;
      if(otherNewSize < 0) otherNewSize = 0; // if it's still negative, force it to zero to prevent a crash
    }
  }
}
#endregion

} // namespace GameLib.Forms
