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

// TODO: allow the hotkey letter to be underlined
#region MenuItemBase
public abstract class MenuItemBase : Control
{
  protected MenuItemBase()
  {
    BackColor     = SystemColors.Menu;
    ForeColor     = SystemColors.MenuText;
    ControlStyle |= ControlStyle.Clickable;
  }

  public event EventHandler Click;

  public KeyCombo GlobalHotKey
  {
    get; set;
  }
  
  public char HotKey
  {
    get { return hotKey; }
    set { hotKey = char.ToUpper(value); }
  }
  
  public Menu Menu
  {
    get { return (Menu)Parent; }
  }

  public virtual Size MeasureItem() { return Size; }

  protected internal override void OnMouseClick(ClickEventArgs e)
  {
    base.OnMouseClick(e);

    if(e.CE.Button == MouseButton.Left)
    {
      OnClick();
      e.Handled = true;
    }
  }

  protected internal virtual void OnClick()
  {
    if(Click != null) Click(this, EventArgs.Empty);
  }

  char hotKey;
}
#endregion

#region Menu
// TODO: add arrow key support
public class Menu : Control
{
  public Menu()
  {
    BorderStyle   = BorderStyle.FixedThick;
    ControlStyle |= ControlStyle.CanReceiveFocus | ControlStyle.DontLayout;
    BackColor     = SystemColors.Menu;
    ForeColor     = SystemColors.MenuText;
    SelectedForeColor = SelectedBackColor = Color.Transparent;
  }

  public Menu(string text) : this()
  {
    Text = text;
  }
  
  public Menu(string text, KeyCombo globalHotKey) : this(text)
  {
    GlobalHotKey = globalHotKey;
  }

  public KeyCombo GlobalHotKey
  {
    get; set;
  }

  public bool IsOpen
  {
    get { return source != null; }
  }

  public Color SelectedBackColor
  {
    get;
    set;
  }

  public Color SelectedForeColor
  {
    get;
    set;
  }

  public Control SourceControl
  {
    get { return source; }
  }

  public event EventHandler Popup;

  public T Add<T>(T item) where T : MenuItemBase
  {
    Controls.Add(item);
    return item;
  }
  
  public void Clear()
  {
    Controls.Clear();
  }

  public void Close()
  {
    if(source != null)
    {
      MenuBarBase bar = source as MenuBarBase;
      if(bar != null) bar.OnMenuClosed(this);
      Parent = null;
      source = null;
    }
  }

  public void Show(Control source, Point position)
  {
    Show(source, position, false, true);
  }
  
  public void Show(Control source, Point position, bool pullDown)
  {
    Show(source, position, pullDown, true);
  }
  
  public void Show(Control source, Point position, bool pullDown, bool wait)
  {
    if(source == null) throw new ArgumentNullException();
    if(source.Desktop == null) throw new InvalidOperationException("The source control is not part of a desktop.");

    this.source = source;
    this.pullDown = pullDown;
    Visible = false;
    Parent = source.Desktop;
    Bounds = new Rectangle(source.ControlToAncestor(position, source.Desktop), Size);
    OnPopup();
    BringToFront();
    Visible = true;
    SetModal(true);
    Capture = true;

    if(wait)
    {
      while(Events.Events.PumpEvent(true) && this.source != null) ;
      Close();
    }
  }

  internal bool PullDown
  {
    get { return pullDown; }
    set
    {
      if(value != pullDown)
      {
        pullDown = value;
        if(!pullDown) Capture = true;
      }
    }
  }

  protected override void LayOutChildren()
  {
    base.LayOutChildren();

    RectOffset contentOffset = ContentOffset;
    int maxWidth = 0, height = 0;
    foreach(MenuItemBase item in Controls)
    {
      Size size = item.MeasureItem();
      if(size.Width > maxWidth) { maxWidth = size.Width; }
      else size.Width = maxWidth;
      item.SetBounds(new Rectangle(new Point(contentOffset.Left, height + contentOffset.Top), size), true);
      height += item.Height;
    }

    foreach(MenuItemBase item in Controls)
    {
      if(item.Width != maxWidth) item.SetBounds(new Rectangle(item.Location, new Size(maxWidth, item.Height)), true);
    }

    Size = new Size(maxWidth + contentOffset.Horizontal, height + contentOffset.Vertical);
  }

  protected override void OnControlAdded(ControlEventArgs e)
  {
    base.OnControlAdded(e);
    TriggerLayout();
  }

  protected override void OnControlRemoved(ControlEventArgs e)
  {
    base.OnControlRemoved(e);
    TriggerLayout();
  }

  protected virtual void OnPopup()
  {
    if(Popup != null) Popup(this, EventArgs.Empty);
  }

  protected internal override void OnKeyPress(KeyEventArgs e)
  {
    if(!e.Handled)
    {
      char c = char.ToUpper(e.KE.Char);
      foreach(MenuItemBase item in Controls)
      {
        if(c == item.HotKey && item.Enabled)
        {
          PostClickEvent(item);
          Close();
          e.Handled = true;
          break;
        }
      }
    }

    base.OnKeyPress(e);
  }

  protected internal override void OnKeyDown(KeyEventArgs e)
  {
    if(!e.Handled && e.KE.Down && e.KE.Key == Key.Escape && e.KE.KeyMods == KeyMod.None)
    {
      Close();
      e.Handled = true;
    }
  }

  protected internal override void OnMouseUp(ClickEventArgs e)
  {
    if(!e.Handled && pullDown)
    {
      if(!ControlRect.Contains(e.CE.Point))
      {
        Close();
      }
      else
      {
        pullDown = false;
        foreach(MenuItemBase item in Controls)
        {
          if(item.Bounds.Contains(e.CE.Point))
          {
            if(item.Enabled) PostClickEvent(item);
            Close();
            goto done;
          }
        }
        Capture = true;
      }
      done:
      e.Handled = true;
    }
    base.OnMouseUp(e);
  }

  protected internal override void OnCustomEvent(Events.ControlEvent e)
  {
    ItemClickedEvent ice = e as ItemClickedEvent;
    if(ice != null) Click(ice.Item);
    base.OnCustomEvent(e);
  }

  protected override void OnTextChanged(ValueChangedEventArgs e)
  {
    MenuBarBase bar = Parent as MenuBarBase;
    if(bar != null) bar.Relayout();
    base.OnTextChanged(e);
  }

  protected internal void PostClickEvent(MenuItemBase item)
  {
    if(Events.Events.Initialized) Events.Events.PushEvent(new ItemClickedEvent(this, item));
  }

  sealed class ItemClickedEvent : Events.ControlEvent
  {
    public ItemClickedEvent(Control menu, MenuItemBase item) : base(menu) { Item = item; }
    public MenuItemBase Item;
  }

  void Click(MenuItemBase item)
  {
    item.OnClick();
    Close();
  }

  Control source;
  bool pullDown;
}
#endregion

#region MenuBarBase
public abstract class MenuBarBase : Control
{
  protected MenuBarBase()
  {
    ControlStyle |= ControlStyle.Clickable;
    BackColor = SystemColors.Menu;
    ForeColor = SystemColors.MenuText;
    menus = new MenuCollection(this);
  }

  #region MenuCollection
  /// <summary>This class provides a strongly-typed collection of <see cref="Menu"/> objects.</summary>
  public class MenuCollection : Collection<Menu>
  {
    internal MenuCollection(MenuBarBase parent) { this.parent = parent; }

    public Menu Find(string text)
    {
      foreach(Menu menu in this) if(menu.Text == text) return menu;
      return null;
    }

    protected override void ClearItems()
    {
      base.ClearItems();
      parent.Relayout();
    }

    protected override void InsertItem(int index, Menu menu)
    {
      if(menu == null) throw new ArgumentNullException("menu", "Item cannot be null.");
      base.InsertItem(index, menu);
      parent.Relayout();
    }

    protected override void RemoveItem(int index)
    {
      base.RemoveItem(index);
      parent.Relayout();
    }

    protected override void SetItem(int index, Menu menu)
    {
      if(menu == null) throw new ArgumentNullException("menu", "Item cannot be null.");
      base.SetItem(index, menu);
      parent.Relayout();
    }

    readonly MenuBarBase parent;
  }
  #endregion

  public MenuCollection Menus
  {
    get { return menus; }
  }

  public int Spacing
  {
    get { return spacing; }
    set
    {
      if(value < 0) throw new ArgumentOutOfRangeException("spacing", value, "must not be negative");
      spacing = value;
      Relayout();
    }
  }

  public T Add<T>(T item) where T : Menu
  {
    Menus.Add(item);
    return item;
  }

  public bool HandleKey(Events.KeyboardEvent e)
  {
    for(int i = 0; i < buttons.Length; i++)
    {
      if(buttons[i].Menu.GlobalHotKey.Matches(e))
      {
        Open(i, false);
        return true;
      }
    }
    
    foreach(Menu menu in menus)
    {
      if(menu.Enabled)
      {
        foreach(MenuItemBase item in menu.Controls)
        {
          if(item.Enabled && item.GlobalHotKey.Matches(e))
          {
            menu.PostClickEvent(item);
            return true;
          }
        }
      }
    }

    return false;
  }

  protected internal override void OnMouseDown(ClickEventArgs e)
  {
    if(!e.Handled && e.CE.Button == MouseButton.Left)
    {
      for(int i = 0; i < buttons.Length; i++)
      {
        if(buttons[i].Area.Contains(e.CE.Point) && buttons[i].Menu.Enabled) Open(i);
        else if(buttons[i].State != ButtonState.Normal) Close(i);
      }
      e.Handled = true;
    }

    base.OnMouseDown(e);
  }

  protected internal override void OnMouseUp(ClickEventArgs e)
  {
    if(!e.Handled && e.CE.Button == MouseButton.Left)
    {
      int open = -1;
      for(int i = 0; i < buttons.Length; i++)
      {
        if(buttons[i].State == ButtonState.Open)
        {
          if(buttons[i].Area.Contains(e.CE.Point))
          {
            buttons[i].Menu.PullDown = false;
            buttons[i].Menu.Capture  = true;
            open = -1;
            break;
          }
          else open = i; // TODO: what is this doing? should there be a break after this?
        }
      }

      if(open != -1)
      {
        e.CE.Point = buttons[open].Menu.ParentToControl(ControlToAncestor(e.CE.Point, Desktop));
        buttons[open].Menu.OnMouseUp(e);
      }
      
      e.Handled = true;
    }

    base.OnMouseDown(e);
  }

  protected internal override void OnMouseMove(Events.MouseMoveEvent e)
  {
    int over = -1, oldOver = -1;
    for(int i = 0; i < buttons.Length; i++)
    {
      if(buttons[i].Over || buttons[i].State != ButtonState.Normal) oldOver = i;
      if(buttons[i].Area.Contains(e.Point))
      {
        if(!buttons[i].Menu.Enabled) goto done;
        buttons[i].Over = true;
        over = i;
      }
      else buttons[i].Over = false;
    }

    if(over != oldOver)
    {
      ButtonState oldState = ButtonState.Over;
      if(oldOver == -1) oldState = ButtonState.Over;
      else if(over != -1 || buttons[oldOver].State == ButtonState.Over)
      {
        oldState = buttons[oldOver].State;
        buttons[oldOver].State = ButtonState.Normal;
        buttons[oldOver].Menu.Close();
        Invalidate(buttons[oldOver].Area);
      }
      if(over != -1)
      {
        buttons[over].State = oldState;
        if(oldState == ButtonState.Open) Open(over);
        Invalidate(buttons[over].Area);
      }
    }
    
    done: base.OnMouseMove(e);
  }

  protected internal override void OnMouseLeave()
  {
    int oldOver = -1;
    for(int i = 0; i < buttons.Length; i++)
    {
      if(buttons[i].Over || buttons[i].State != ButtonState.Normal)
      {
        oldOver = i;
        break;
      }
    }
    
    if(oldOver != -1)
    {
      buttons[oldOver].Over = false;
      if(buttons[oldOver].State == ButtonState.Over)
      {
        buttons[oldOver].State = ButtonState.Normal;
        Invalidate(buttons[oldOver].Area);
      }
    }
    
    base.OnMouseLeave();
  }

  protected override void OnParentChanged(ValueChangedEventArgs e)
  {
    Relayout();
    base.OnParentChanged(e);
  }
  
  protected override void OnEffectiveFontChanged(ValueChangedEventArgs e)
  {
    base.OnEffectiveFontChanged(e);
    Relayout();
  }
  
  protected override void OnResize()
  {
    Relayout();
    base.OnResize();
  }

  protected abstract Size MeasureItem(Menu menu);

  protected enum ButtonState : byte { Normal, Over, Open };

  protected struct MenuButton
  {
    public MenuButton(Menu menu, Rectangle area)
    {
      Menu  = menu;
      Area  = area;
      State = ButtonState.Normal;
      Over  = false;
    }

    public Menu Menu;
    public Rectangle Area;
    public ButtonState State;
    public bool Over;
  }
  
  protected MenuButton[] Buttons
  {
    get { return buttons; }
  }

  protected internal void Relayout()
  {
    if(EffectiveFont == null)
    {
      buttons = new MenuButton[0];
    }
    else
    {
      buttons = new MenuButton[menus.Count];
      for(int i = 0, x = spacing; i < menus.Count; i++)
      {
        Menu menu = menus[i];
        Size size = MeasureItem(menu);
        buttons[i] = new MenuButton(menu, new Rectangle(x, (Height - size.Height) / 2, size.Width, size.Height));
        x += size.Width + spacing;
      }
      Invalidate();
    }
  }

  internal void OnMenuClosed(Menu menu)
  {
    for(int i = 0; i < buttons.Length; i++)
    {
      if(buttons[i].Menu == menu && buttons[i].State != ButtonState.Normal)
      {
        Close(i);
        break;
      }
    }
  }

  void Open(int menu)
  {
    Open(menu, true);
  }
  
  void Open(int menu, bool pullDown)
  {
    buttons[menu].State = ButtonState.Open;
    buttons[menu].Menu.Show(this, new Point(buttons[menu].Area.X, buttons[menu].Area.Bottom), pullDown, false);
    Capture = pullDown;
    Invalidate(buttons[menu].Area);
  }

  void Close(int menu)
  {
    buttons[menu].State = ButtonState.Normal;
    buttons[menu].Menu.Close();
    Invalidate(buttons[menu].Area);
    Capture = false;
  }

  MenuCollection menus;
  MenuButton[] buttons;
  int spacing = 1;
}
#endregion

#region MenuItem
public class MenuItem : MenuItemBase
{
  public MenuItem()
  {
    Padding = new RectOffset(2, 0);
  }
  
  public MenuItem(string text) : this()
  {
    Text = text;
  }
  
  public MenuItem(string text, char hotKey) : this(text)
  {
    HotKey = hotKey;
  }
  
  public MenuItem(string text, char hotKey, KeyCombo globalHotKey) : this(text, hotKey)
  {
    GlobalHotKey = globalHotKey;
  }

  public Color RawSelectedBackColor
  {
    get { return selBack; }
  }
  
  public Color RawSelectedForeColor
  {
    get { return selFore; }
  }

  public Color SelectedBackColor
  {
    get
    {
      if(selBack.A != 0) return selBack;

      Menu menu = (Menu)Parent;
      return menu == null || menu.SelectedBackColor.A == 0 ? GetEffectiveForeColor() : menu.SelectedBackColor;
    }
    set
    {
      selBack = value;
      if(mouseOver) Invalidate();
    }
  }

  public Color SelectedForeColor
  {
    get
    {
      if(selFore.A != 0) return selFore;
      Menu menu = (Menu)Parent;
      return menu == null || menu.SelectedForeColor.A == 0 ? BackColor : menu.SelectedForeColor;
    }
    set
    {
      selFore = value;
      if(mouseOver) Invalidate();
    }
  }

  protected internal override void OnMouseEnter()
  {
    mouseOver = true;
    Invalidate();
    base.OnMouseEnter();
  }
  
  protected internal override void OnMouseLeave()
  {
    mouseOver = false;
    Invalidate();
    base.OnMouseLeave();
  }

  protected override void PaintBackgroundColor(PaintEventArgs e)
  {
    if(mouseOver && EffectivelyEnabled) e.Target.FillArea(e.DrawRect, SelectedBackColor);
    else base.PaintBackgroundColor(e);
  }

  protected override void OnPaint(PaintEventArgs e)
  {
    base.OnPaint(e);

    if(EffectiveFont != null && Text.Length != 0)
    {
      if(EffectivelyEnabled)
      {
        EffectiveFont.Color     = mouseOver ? SelectedForeColor : GetEffectiveForeColor();
        EffectiveFont.BackColor = mouseOver ? SelectedBackColor : GetEffectiveBackColor();
      }
      else
      {
        EffectiveFont.Color     = SystemColors.GrayText;
        EffectiveFont.BackColor = GetEffectiveBackColor();
      }

      Rectangle rect = GetContentDrawRect();
      e.Target.DrawText(EffectiveFont, Text, new Point(rect.X, rect.Y + (rect.Height - EffectiveFont.Height) / 2));
      if(GlobalHotKey.IsValid)
      {
        string hotkeyText = GlobalHotKey.ToString();
        Size size = EffectiveFont.CalculateSize(hotkeyText);
        e.Target.DrawText(EffectiveFont, hotkeyText,
                          UIHelper.CalculateAlignment(rect, size, ContentAlignment.MiddleRight));
      }
    }
  }

  public override Size MeasureItem()
  {
    if(EffectiveFont == null)
    {
      return base.MeasureItem();
    }
    else
    {
      Size size = EffectiveFont.CalculateSize(Text);
      if(GlobalHotKey.IsValid)
      {
        Size hotkey = EffectiveFont.CalculateSize(GlobalHotKey.ToString());
        size.Width += hotkey.Width + hotKeyPadding;
        if(hotkey.Height > size.Height) size.Height = hotkey.Height;
      }
      size.Width  += horzPadding * 2;
      size.Height += vertPadding * 2;
      return size;
    }
  }

  const int hotKeyPadding = 20, horzPadding = 2, vertPadding = 3;
  Color selFore = SystemColors.HighlightText, selBack = SystemColors.Highlight;
  bool mouseOver;
}
#endregion

#region MenuBar
public class MenuBar : MenuBarBase
{
  public int HorizontalPadding
  {
    get { return horzPadding; }
    set
    {
      if(value < 1) throw new ArgumentOutOfRangeException("HorizontalPadding", value, "must be positive");
      horzPadding = value;
      Relayout();
    }
  }

  public int VerticalPadding
  {
    get { return vertPadding; }
    set
    {
      if(value < 1) throw new ArgumentOutOfRangeException("VerticalPadding", value, "must be positive");
      vertPadding = value;
      Relayout();
    }
  }

  protected override void PaintBorder(PaintEventArgs e)
  {
    base.PaintBorder(e);

    foreach(MenuButton button in Buttons)
    {
      if(button.Area.IntersectsWith(e.ControlRect))
      {
        if(button.State != ButtonState.Normal)
        {
          // TODO: make this border style/color configurable
          e.Renderer.DrawBorder(e.Target, ControlToDraw(button.Area), BorderStyle.Fixed3D, BackColor);
        }
      }
    }
  }
  
  protected override void OnPaint(PaintEventArgs e)
  {
    base.OnPaint(e);

    if(EffectiveFont != null)
    {
      EffectiveFont.BackColor = BackColor;
      foreach(MenuButton button in Buttons)
      {
        if(button.Area.IntersectsWith(e.ControlRect))
        {
          EffectiveFont.Color = button.Menu.Enabled ? GetEffectiveForeColor() : SystemColors.GrayText;
          Point renderPt =
            UIHelper.CalculateAlignment(ControlToDraw(button.Area),
                                        EffectiveFont.CalculateSize(button.Menu.Text), ContentAlignment.MiddleCenter);
          e.Target.DrawText(EffectiveFont, button.Menu.Text, renderPt);
        }
      }
    }
  }

  protected override Size MeasureItem(Menu menu)
  {
    Size size = EffectiveFont.CalculateSize(menu.Text);
    size.Width  += horzPadding * 2;
    size.Height += vertPadding * 2;
    return size;
  }

  int horzPadding = 6, vertPadding = 2;
}
#endregion

} // namespace GameLib.Forms