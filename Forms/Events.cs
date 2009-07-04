/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2009 Adam Milazzo

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
using GameLib.Forms;
using GameLib.Input;
using GameLib.Interop.SDL;

namespace GameLib.Events
{

#region ControlEvent
/// <summary>This event class serves as a base for all messages specific to the windowing system.</summary>
/// <remarks>These messages serve to implement the internals of the windowing system and do not normally need to be
/// handled by user code. However, implementors of custom windowing controls may want to derive from this class to
/// implement custom events. Since the UI handling code and the event handling code should be on the same thread,
/// controls that use other threads (or use objects such as <see cref="System.Threading.Timer"/>, which use other
/// threads) are good candidates for using events derived from this class. See <see cref="Forms.Control"/> for more
/// information.
/// </remarks>
public abstract class ControlEvent : Event
{
  /// <summary>Initializes this object as a custom windowing event.</summary>
  /// <param name="control">The control associated with this event. The event will be passed to that control's
  /// <see cref="Forms.Control.OnCustomEvent"/> handler.
  /// </param>
  /// <remarks>This constructor sets <see cref="SubType"/> to <see cref="MessageType.Custom"/>.</remarks>
  protected ControlEvent(Control control) : this(control, MessageType.Custom) { }

  internal ControlEvent(Control control, MessageType subType)
    : base(EventType.Control)
  {
    if(control == null) throw new ArgumentNullException();
    Control = control; this.subType = subType;
  }

  /// <summary>This enum contains values denoting the various types of window events.</summary>
  public enum MessageType
  {
    /// <summary>A custom event.</summary>
    /// <remarks>All windowing events that are not used to implement the core windowing logic use this type.</remarks>
    Custom,
    /// <summary>An event which signals that a control needs to arrange its children.</summary>
    Layout,
    /// <summary>An event which serves to wake up the desktop so it'll redraw itself.</summary>
    DesktopUpdated
  };
  /// <summary>Gets the type of windowing event this object represents.</summary>
  public MessageType SubType { get { return subType; } }
  /// <summary>The control associated with this event.</summary>
  public Control Control;

  MessageType subType;
}
#endregion

#region DesktopUpdatedEvent
/// <summary>This event serves to wake up the desktop so it'll redraw itself, in the event that it's been
/// updated manually by a custom control.
/// </summary>
/// <remarks>This event helps serve to implement the internals of the windowing system and does not need to be
/// handled by ordinary user code. The desktop will normally update itself automatically, but if it's waiting for
/// the next event (it may do this, depending on the type of event loop you use in the application), it won't paint
/// itself until the next event wakes it up. A custom control that manually updates the desktop and wants to be sure
/// the change will be immediately visible may post this message to wake up the desktop so it'll see that it's been
/// updated.
/// </remarks>
public class DesktopUpdatedEvent : ControlEvent
{
  public DesktopUpdatedEvent(Desktop desktop)
    : base(desktop, ControlEvent.MessageType.DesktopUpdated) { }
}
#endregion

} // namespace GameLib.Events