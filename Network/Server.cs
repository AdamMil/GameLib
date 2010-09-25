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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using AdamMil.IO;
using GameLib.Events;

namespace GameLib.Network
{

#region ServerPlayer
/// <summary>This class represents a player connected to the server.</summary>
public sealed class ServerPlayer
{
  internal ServerPlayer(NetLink link, int id)
  {
    Link = link;
    Id   = id;
  }

  /// <summary>Returns true if the player is being dropped.</summary>
  /// <remarks>This property will return true if the player has been dropped, or will be dropped. It is illegal to
  /// send a message to a player that is being dropped.
  /// </remarks>
  public bool Dropping
  {
    get { return DelayedDrop; }
  }

  /// <summary>Gets the player's remote endpoint.</summary>
  public IPEndPoint EndPoint
  {
    get { return Link.RemoteEndPoint; }
  }

  /// <summary>Gets the player's unique Id.</summary>
  public int Id
  {
    get; private set;
  }

  /// <summary>Arbitrary data associated with this player. This field is not examined by the network code, so you can
  /// use it to hold any data you like.
  /// </summary>
  public object Data;

  internal uint DropDelay, DropStart;
  internal NetLink Link;
  internal bool DelayedDrop;
}
#endregion

/// <summary>This delegate is used for the <see cref="Server.PlayerConnecting"/> event. It receives a reference to
/// the <see cref="Server"/> and <see cref="ServerPlayer"/> objects. If it returns false, the player will be
/// immediately disconnected.
/// </summary>
public delegate bool PlayerConnectingHandler(Server server, ServerPlayer player);

/// <summary>This delegate is used for the <see cref="Server.PlayerConnected"/> and
/// <see cref="Server.PlayerDisconnected"/> events. It receives a reference to the <see cref="Server"/> and
/// <see cref="ServerPlayer"/> objects.
/// </summary>
public delegate void PlayerHandler(Server server, ServerPlayer player);

/// <summary>This delegate is used for the <see cref="Server.MessageReceived"/>, <see cref="Server.RemoteReceived"/>,
/// and <see cref="Server.MessageSent"/> events. It receives a reference to the <see cref="Server"/> and
/// <see cref="ServerPlayer"/> objects, and the message that was sent.
/// </summary>
public delegate void ServerMessageHandler(Server sender, ServerPlayer player, object msg);

/// <summary>This class represents the server in a client-server networking setup.</summary>
public class Server
{
  /// <summary>Initializes a new <see cref="Server"/> instance.</summary>
  public Server() { }

  /// <summary>Initializes a new <see cref="Server"/> instance and starts listening for connections on the given
  /// endpoint.
  /// </summary>
  public Server(IPEndPoint local)
  {
    Listen(local);
  }

  /// <summary>This finalizer calls <see cref="Deinitialize"/> to deinitialize the server.</summary>
  ~Server()
  {
    Deinitialize();
  }

  /// <summary>This event is raised when a player is connecting to the server. If any handler returns false, the
  /// player will be disconnected.
  /// </summary>
  public event PlayerConnectingHandler PlayerConnecting;

  /// <summary>This event is raised when a player has connected to the server.</summary>
  public event PlayerHandler PlayerConnected;
  
  /// <summary>This event is raised when a player has disconnected from the server.</summary>
  public event PlayerHandler PlayerDisconnected;
  
  /// <summary>This event is raised when a message has been received.</summary>
  public event ServerMessageHandler MessageReceived;
  
  /// <summary>This event is raised when a message has been received by a remote host.</summary>
  /// <remarks>Note that this event will only be raised for messages sent with the
  /// <see cref="SendFlag.NotifyReceived"/> flag.
  /// </remarks>
  public event ServerMessageHandler RemoteReceived;
  
  /// <summary>This event is raised when a message has been sent over the network.</summary>
  /// <remarks>Note that this event will only be raised for messages sent with the <see cref="SendFlag.NotifySent"/>
  /// flag.
  /// </remarks>
  public event ServerMessageHandler MessageSent;

  /// <summary>Gets the local endpoint upon which the server is listening.</summary>
  public IPEndPoint LocalEndPoint
  {
    get
    {
      if(!listening) throw new InvalidOperationException("The server is not currently listening.");
      return (IPEndPoint)server.LocalEndpoint;
    }
  }

  /// <summary>Gets a read-only collection of the current players.</summary>
  public ReadOnlyCollection<ServerPlayer> Players
  {
    get { return players.AsReadOnly(); }
  }

  /// <include file="../documentation.xml" path="//Network/Common/LagAverage/*"/>
  public int LagAverage
  {
    get { return lagAverage; }
    set
    {
      lock(this)
      {
        foreach(ServerPlayer p in players) p.Link.LagAverage = value;
      }
      lagAverage = value;
    }
  }

  /// <include file="../documentation.xml" path="//Network/Common/LagVariance/*"/>
  public int LagVariance
  {
    get { return lagVariance; }
    set
    {
      lock(this)
      {
        foreach(ServerPlayer p in players) p.Link.LagVariance = value;
      }
      lagVariance = value;
    }
  }

  /// <summary>Gets or sets the message converter used to convert messages sent and received by this server. If not
  /// set, a default instance will be used. You can either register types on this default instance or assign a new
  /// instance that already has types registered. This property can only be set when the server is not listening.
  /// </summary>
  public MessageConverter MessageConverter
  {
    get { return cvt; }
    set
    {
      if(value != cvt)
      {
        if(value == null) throw new ArgumentNullException();
        AssertClosed();
        cvt = value;
      }
    }
  }

  /// <summary>Stops listening for connections and deinitializes the server.</summary>
  public void Deinitialize()
  {
    if(thread != null)
    {
      lock(this)
      {
        foreach(ServerPlayer p in players) DropPlayer(p);
      }
      quit = true;
      thread.Join(1500);
      thread.Abort();
      thread = null;
    }
    
    StopListening();
    quit   = false;
    nextID = 1;
    players.Clear();
    links.Clear();
  }

  /// <summary>Starts listening on the given port, using all compatible network interfaces.</summary>
  /// <param name="port">The port on which to listen for connections.</param>
  public void Listen(int port)
  {
    Listen(new IPEndPoint(IPAddress.Any, port));
  }

  /// <summary>Starts listening on the given endpoint.</summary>
  /// <param name="local">The <see cref="IPEndPoint"/> on which to listen for connections.</param>
  public void Listen(IPEndPoint local)
  {
    lock(this)
    {
      StopListening();
      try
      {
        server = new TcpListener(local);
        server.Start();
      }
      catch
      {
        server = null;
        throw;
      }

      if(thread == null)
      {
        thread = new Thread(ThreadFunc);
        thread.Start();
      }

      listening = true;
    }
  }

  /// <summary>Stops listening for connections without deinitializing the server.</summary>
  public void StopListening()
  {
    if(server != null)
    {
      lock(this)
      {
        listening = false;
        server.Stop();
        server = null;
      }
    }
  }

  /// <summary>Gets information about the status of the receive and/or send queues for a given player.</summary>
  /// <param name="p">The <see cref="ServerPlayer"/> about which retrieve queue status information will be retrieved.</param>
  /// <param name="flags">A set of <see cref="QueueStatusFlag"/> flags that determine what data will be returned.</param>
  /// <returns>A <see cref="QueueStatus"/> structure containing the number and total size of messages waiting in
  /// the queues specified by <paramref name="flags"/>.
  /// </returns>
  public QueueStatus GetQueueStatus(ServerPlayer p, QueueStatusFlag flags)
  {
    lock(this) return p.Link.GetQueueStatus(flags);
  }

  /// <summary>Gets information about the status of the receive and/or send queues for all players combined.</summary>
  /// <param name="flags">A set of <see cref="QueueStatusFlag"/> flags that determine what data will be returned.</param>
  /// <returns>A <see cref="QueueStatus"/> structure containing the number and total size of messages waiting in
  /// the queues specified by <paramref name="flags"/>.
  /// </returns>
  public QueueStatus GetQueueStatus(QueueStatusFlag flags)
  {
    QueueStatus qs = new QueueStatus(), pqs;
    lock(this)
    {
      foreach(NetLink link in links)
      {
        pqs = link.GetQueueStatus(flags);
        qs.SendBytes       += pqs.SendBytes;
        qs.SendMessages    += pqs.SendMessages;
        qs.ReceiveBytes    += pqs.ReceiveBytes;
        qs.ReceiveMessages += pqs.ReceiveMessages;
      }
    }
    return qs;
  }

  /// <summary>Drops a player with as little delay as possible.</summary>
  /// <param name="p">The <see cref="ServerPlayer"/> that will be dropped.</param>
  /// <remarks>This method immediately terminates the connection with the given player. However, the player object
  /// will remain in the <see cref="Players"/> collection until it has been cleaned up by the server maintenance
  /// thread.
  /// </remarks>
  public void DropPlayer(ServerPlayer p)
  {
    p.DropStart = p.DropDelay = 0;
    p.DelayedDrop = true;
    p.Link.Close();
  }

  /// <summary>Drops a player with a delay to allow incoming and outgoing messages to be delivered.</summary>
  /// <param name="p">The <see cref="ServerPlayer"/> to drop.</param>
  /// <remarks>The connection with the player player will be terminated only after all outgoing messages have
  /// been sent and all incoming messages have been processed.
  /// </remarks>
  public void DropPlayerDelayed(ServerPlayer p)
  {
    DropPlayerDelayed(p, int.MaxValue);
  }

  /// <summary>Drops a player with a delay to allow incoming and outgoing messages to be delivered.</summary>
  /// <param name="p">The <see cref="ServerPlayer"/> to drop.</param>
  /// <param name="timeoutMs">The amount of time to wait for all remaining messages to be delivered, in milliseconds.</param>
  /// <remarks>The connection with the player player will be terminated after all outgoing messages have
  /// been sent and all incoming messages have been processed, or the timeout expires.
  /// </remarks>
  public void DropPlayerDelayed(ServerPlayer p, int timeoutMs)
  {
    if(timeoutMs < 0) throw new ArgumentOutOfRangeException("timeoutMs", "cannot be negative");
    p.DropDelay   = (uint)timeoutMs;
    p.DropStart   = Timing.InternalMsecs;
    p.DelayedDrop = true;
  }

  /// <include file="../documentation.xml" path="//Network/Server/Send/*[self::Common or self::ToPlayer or self::byteData]/*"/>
  public void Send(ServerPlayer toWhom, byte[] data)
  {
    Send(toWhom, data, 0, data.Length, DefaultFlags);
  }

  /// <include file="../documentation.xml" path="//Network/Server/Send/*[self::Common or self::ToPlayer or self::byteData or self::Index]/*"/>
  public void Send(ServerPlayer toWhom, byte[] data, int index, int length)
  {
    RawSend(toWhom, cvt.Serialize(data, index, length), null, DefaultFlags, 0, data);
  }

  /// <include file="../documentation.xml" path="//Network/Server/Send/*[self::Common or self::ToPlayer or self::byteData or self::Flags]/*"/>
  public void Send(ServerPlayer toWhom, byte[] data, SendFlag flags)
  {
    Send(toWhom, data, 0, data.Length, flags);
  }

  /// <include file="../documentation.xml" path="//Network/Server/Send/*[self::Common or self::ToPlayer or self::byteData or self::Index or self::Flags]/*"/>
  public void Send(ServerPlayer toWhom, byte[] data, int index, int length, SendFlag flags)
  {
    RawSend(toWhom, cvt.Serialize(data, index, length), null, flags, 0, data);
  }

  /// <include file="../documentation.xml" path="//Network/Server/Send/*[self::Common or self::ToPlayer or self::byteData or self::Flags or self::Timeout]/*"/>
  public void Send(ServerPlayer toWhom, byte[] data, SendFlag flags, int timeoutMs)
  {
    Send(toWhom, data, 0, data.Length, flags, timeoutMs);
  }

  /// <include file="../documentation.xml" path="//Network/Server/Send/*[self::Common or self::ToPlayer or self::byteData or self::Index or self::Flags or self::Timeout]/*"/>
  public void Send(ServerPlayer toWhom, byte[] data, int index, int length, SendFlag flags, int timeoutMs)
  {
    RawSend(toWhom, cvt.Serialize(data, index, length), null, flags, timeoutMs, data);
  }

  /// <include file="../documentation.xml" path="//Network/Server/Send/*[self::Common or self::ToPlayer or self::objData]/*"/>
  public void Send(ServerPlayer toWhom, object data)
  {
    Send(toWhom, data, DefaultFlags, 0);
  }

  /// <include file="../documentation.xml" path="//Network/Server/Send/*[self::Common or self::ToPlayer or self::objData or self::Flags]/*"/>
  public void Send(ServerPlayer toWhom, object data, SendFlag flags)
  {
    Send(toWhom, data, flags, 0);
  }

  /// <include file="../documentation.xml" path="//Network/Server/Send/*[self::Common or self::ToPlayer or self::objData or self::Flags or self::Timeout]/*"/>
  public void Send(ServerPlayer toWhom, object data, SendFlag flags, int timeoutMs)
  {
    Stream attachedStream;
    RawSend(toWhom, cvt.Serialize(data, out attachedStream), attachedStream, flags, timeoutMs, data);
  }

  /// <include file="../documentation.xml" path="//Network/Server/Send/*[self::Common or self::ToList or self::byteData]/*"/>
  public void Send(ICollection<ServerPlayer> toWhom, byte[] data)
  {
    Send(toWhom, data, 0, data.Length, DefaultFlags);
  }

  /// <include file="../documentation.xml" path="//Network/Server/Send/*[self::Common or self::ToList or self::byteData or self::Index]/*"/>
  public void Send(ICollection<ServerPlayer> toWhom, byte[] data, int index, int length)
  {
    RawSend(toWhom, cvt.Serialize(data, index, length), null, DefaultFlags, 0, data);
  }

  /// <include file="../documentation.xml" path="//Network/Server/Send/*[self::Common or self::ToList or self::byteData or self::Flags]/*"/>
  public void Send(ICollection<ServerPlayer> toWhom, byte[] data, SendFlag flags)
  {
    Send(toWhom, data, 0, data.Length, flags);
  }

  /// <include file="../documentation.xml" path="//Network/Server/Send/*[self::Common or self::ToList or self::byteData  or self::Index or self::Flags]/*"/>
  public void Send(ICollection<ServerPlayer> toWhom, byte[] data, int index, int length, SendFlag flags)
  {
    RawSend(toWhom, cvt.Serialize(data, index, length), null, flags, 0, data);
  }

  /// <include file="../documentation.xml" path="//Network/Server/Send/*[self::Common or self::ToList or self::byteData or self::Flags or self::Timeout]/*"/>
  public void Send(ICollection<ServerPlayer> toWhom, byte[] data, SendFlag flags, int timeoutMs)
  {
    Send(toWhom, data, 0, data.Length, flags, timeoutMs);
  }

  /// <include file="../documentation.xml" path="//Network/Server/Send/*[self::Common or self::ToList or self::byteData or self::Index or self::Flags or self::Timeout]/*"/>
  public void Send(ICollection<ServerPlayer> toWhom, byte[] data, int index, int length, SendFlag flags, int timeoutMs)
  {
    RawSend(toWhom, cvt.Serialize(data, index, length), null, flags, timeoutMs, data);
  }

  /// <include file="../documentation.xml" path="//Network/Server/Send/*[self::Common or self::ToList or self::objData]/*"/>
  public void Send(ICollection<ServerPlayer> toWhom, object data)
  {
    Send(toWhom, data, DefaultFlags, 0);
  }

  /// <include file="../documentation.xml" path="//Network/Server/Send/*[self::Common or self::ToList or self::objData or self::Flags]/*"/>
  public void Send(ICollection<ServerPlayer> toWhom, object data, SendFlag flags)
  {
    Send(toWhom, data, flags, 0);
  }

  /// <include file="../documentation.xml" path="//Network/Server/Send/*[self::Common or self::ToList or self::objData or self::Flags or self::Timeout]/*"/>
  public void Send(ICollection<ServerPlayer> toWhom, object data, SendFlag flags, int timeoutMs)
  {
    Stream attachedStream;
    RawSend(toWhom, cvt.Serialize(data, out attachedStream), attachedStream, flags, timeoutMs, data);
  }

  /// <include file="../documentation.xml" path="//Network/Server/SendToAllExcept/*[self::CommonToAll or self::byteData]/*"/>
  public void SendToAll(byte[] data)
  {
    SendToAll(data, 0, data.Length, DefaultFlags);
  }

  /// <include file="../documentation.xml" path="//Network/Server/SendToAll/*[self::CommonToAll or self::byteData or self::Index]/*"/>
  public void SendToAll(byte[] data, int index, int length)
  {
    RawSend(players, cvt.Serialize(data, index, length), null, DefaultFlags, 0, data);
  }

  /// <include file="../documentation.xml" path="//Network/Server/SendToAll/*[self::CommonToAll or self::byteData or self::Flags]/*"/>
  public void SendToAll(byte[] data, SendFlag flags)
  {
    SendToAll(data, 0, data.Length, flags);
  }

  /// <include file="../documentation.xml" path="//Network/Server/SendToAll/*[self::CommonToAll or self::byteData or self::Index or self::Flags]/*"/>
  public void SendToAll(byte[] data, int index, int length, SendFlag flags)
  {
    RawSend(players, cvt.Serialize(data, index, length), null, flags, 0, data);
  }

  /// <include file="../documentation.xml" path="//Network/Server/SendToAll/*[self::CommonToAll or self::byteData or self::Flags or self::Timeout]/*"/>
  public void SendToAll(byte[] data, SendFlag flags, int timeoutMs)
  {
    SendToAll(data, 0, data.Length, flags, timeoutMs);
  }

  /// <include file="../documentation.xml" path="//Network/Server/SendToAll/*[self::CommonToAll or self::byteData or self::Index or self::Flags or self::Timeout]/*"/>
  public void SendToAll(byte[] data, int index, int length, SendFlag flags, int timeoutMs)
  {
    RawSend(players, cvt.Serialize(data, index, length), null, flags, timeoutMs, data);
  }

  /// <include file="../documentation.xml" path="//Network/Server/SendToAll/*[self::CommonToAll or self::objData]/*"/>
  public void SendToAll(object data)
  {
    Stream attachedStream;
    RawSend(players, cvt.Serialize(data, out attachedStream), attachedStream, DefaultFlags, 0, data);
  }

  /// <include file="../documentation.xml" path="//Network/Server/SendToAll/*[self::CommonToAll or self::objData or self::Flags]/*"/>
  public void SendToAll(object data, SendFlag flags)
  {
    Stream attachedStream;
    RawSend(players, cvt.Serialize(data, out attachedStream), attachedStream, flags, 0, data);
  }

  /// <include file="../documentation.xml" path="//Network/Server/SendToAll/*[self::CommonToAll or self::objData or self::Flags or self::Timeout]/*"/>
  public void SendToAll(object data, SendFlag flags, int timeoutMs)
  {
    Stream attachedStream;
    RawSend(players, cvt.Serialize(data, out attachedStream), attachedStream, flags, timeoutMs, data);
  }

  /// <include file="../documentation.xml" path="//Network/Server/SendToAllExcept/*[self::CommonToAllExcept or self::byteData]/*"/>
  public void SendToAllExcept(ServerPlayer player, byte[] data)
  {
    SendToAllExcept(player, data, 0, data.Length, DefaultFlags);
  }
  
  /// <include file="../documentation.xml" path="//Network/Server/SendToAllExcept/*[self::CommonToAllExcept or self::byteData or self::Index]/*"/>
  public void SendToAllExcept(ServerPlayer player, byte[] data, int index, int length)
  {
    RawSendToAllExcept(player, cvt.Serialize(data, index, length), null, DefaultFlags, 0, data);
  }

  /// <include file="../documentation.xml" path="//Network/Server/SendToAllExcept/*[self::CommonToAllExcept or self::byteData or self::Flags]/*"/>
  public void SendToAllExcept(ServerPlayer player, byte[] data, SendFlag flags)
  {
    RawSendToAllExcept(player, data, 0, data.Length, null, flags, 0, data);
  }
  
  /// <include file="../documentation.xml" path="//Network/Server/SendToAllExcept/*[self::CommonToAllExcept or self::byteData or self::Index or self::Flags]/*"/>
  public void SendToAllExcept(ServerPlayer player, byte[] data, int index, int length, SendFlag flags)
  {
    RawSendToAllExcept(player, cvt.Serialize(data, index, length), null, flags, 0, data);
  }
  
  /// <include file="../documentation.xml" path="//Network/Server/SendToAllExcept/*[self::CommonToAllExcept or self::byteData or self::Flags or self::Timeout]/*"/>
  public void SendToAllExcept(ServerPlayer player, byte[] data, SendFlag flags, int timeoutMs)
  {
    RawSendToAllExcept(player, data, 0, data.Length, null, flags, timeoutMs, data);
  }
  
  /// <include file="../documentation.xml" path="//Network/Server/SendToAllExcept/*[self::CommonToAllExcept or self::byteData or self::Index or self::Flags or self::Timeout]/*"/>
  public void SendToAllExcept(ServerPlayer player, byte[] data, int index, int length, SendFlag flags, int timeoutMs)
  {
    RawSendToAllExcept(player, cvt.Serialize(data, index, length), null, flags, timeoutMs, data);
  }

  /// <include file="../documentation.xml" path="//Network/Server/SendToAllExcept/*[self::CommonToAllExcept or self::objData]/*"/>
  public void SendToAllExcept(ServerPlayer player, object data)
  {
    SendToAllExcept(player, data, DefaultFlags, 0);
  }
  
  /// <include file="../documentation.xml" path="//Network/Server/SendToAllExcept/*[self::CommonToAllExcept or self::objData or self::Flags]/*"/>
  public void SendToAllExcept(ServerPlayer player, object data, SendFlag flags)
  {
    SendToAllExcept(player, data, flags, 0);
  }
  
  /// <include file="../documentation.xml" path="//Network/Server/SendToAllExcept/*[self::CommonToAllExcept or self::objData or self::Flags or self::Timeout]/*"/>
  public void SendToAllExcept(ServerPlayer player, object data, SendFlag flags, int timeoutMs)
  {
    Stream attachedStream;
    RawSendToAllExcept(player, cvt.Serialize(data, out attachedStream), attachedStream, flags, timeoutMs, data);
  }

  /// <summary>Raises the <see cref="MessageReceived"/> event.</summary>
  /// <param name="player">The <see cref="ServerPlayer"/> that sent the message.</param>
  /// <param name="message">The message that was received.</param>
  /// <remarks>If you override this method, be sure to call the base class' implementation. The proper place
  /// to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnMessageReceived(ServerPlayer player, object message)
  {
    ServerMessageHandler handler = MessageReceived; // store it in a local variable so we don't have to lock
    if(handler != null) handler(this, player, message);
  }

  /// <summary>Raises the <see cref="MessageSent"/> event.</summary>
  /// <param name="player">The <see cref="ServerPlayer "/>to which the message was sent.</param>
  /// <param name="message">The message that was sent.</param>
  /// <remarks>This method is only called for messages that have requested notification via the
  /// <see cref="SendFlag.NotifySent"/> flag. If you override this method, be sure to call the base class'
  /// implementation. The proper place to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnMessageSent(ServerPlayer player, object message)
  {
    ServerMessageHandler handler = MessageSent; // store it in a local variable so we don't have to lock
    if(handler != null) handler(this, player, message);
  }

  /// <summary>Raises the <see cref="PlayerConnecting"/> event.</summary>
  /// <param name="player">The <see cref="ServerPlayer"/> that is connecting.</param>
  /// <returns>True if the player should be allowed to connect, and false if the player should be dropped.</returns>
  /// <remarks>If you override this method, be sure to call the base class' implementation. The proper place
  /// to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual bool OnPlayerConnecting(ServerPlayer player)
  {
    PlayerConnectingHandler pch = PlayerConnecting; // store it in a local variable so we don't have to lock
    if(pch != null)
    {
      foreach(PlayerConnectingHandler handler in pch.GetInvocationList())
      {
        if(!handler(this, player)) return false;
      }
    }
    return true;
  }

  /// <summary>Raises the <see cref="PlayerConnected"/> event.</summary>
  /// <param name="player">The <see cref="ServerPlayer"/> that has connected.</param>
  /// <remarks>If you override this method, be sure to call the base class' implementation. The proper place
  /// to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnPlayerConnected(ServerPlayer player)
  {
    PlayerHandler handler = PlayerConnected; // store it in a local variable so we don't have to lock
    if(handler != null) handler(this, player);
  }

  /// <summary>Raises the <see cref="PlayerDisconnected"/> event.</summary>
  /// <param name="player">The <see cref="ServerPlayer"/> that has disconnected.</param>
  /// <remarks>This method is called for both planned and unplanned disconnections.
  /// If you override this method, be sure to call the base class' implementation. The proper place
  /// to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnPlayerDisconnected(ServerPlayer player)
  {
    PlayerHandler handler = PlayerDisconnected; // store it in a local variable so we don't have to lock
    if(handler != null) handler(this, player);
  }

  /// <summary>Raises the <see cref="RemoteReceived"/> event.</summary>
  /// <param name="player">The <see cref="ServerPlayer"/> that has received the message.</param>
  /// <param name="message">The message that was received.</param>
  /// <remarks>This method is only called for messages that have requested notification via the
  /// <see cref="SendFlag.NotifyReceived"/> flag. If you override this method, be sure to call the base class'
  /// implementation. The proper place to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnRemoteReceived(ServerPlayer player, object message)
  {
    ServerMessageHandler handler = RemoteReceived; // store it in a local variable so we don't have to lock
    if(handler != null) handler(this, player, message);
  }

  const SendFlag DefaultFlags = SendFlag.Reliable;

  #region SentMessage
  sealed class SentMessage
  {
    public SentMessage(ServerPlayer player, object data)
    {
      Player = player;
      Data   = data;
    }

    public readonly ServerPlayer Player;
    public readonly object Data;
  }
  #endregion

  void AssertClosed()
  {
    if(thread != null) throw new InvalidOperationException("Server already initialized.");
  }

  void AssertOpen()
  {
    if(thread == null) throw new InvalidOperationException("Server not initialized yet.");
  }
  
  void OnRemoteReceived(NetLink link, LinkMessage msg)
  {
    SentMessage sent = (SentMessage)msg.Tag;
    OnRemoteReceived(sent.Player, sent.Data);
  }

  void OnMessageSent(NetLink link, LinkMessage msg)
  {
    SentMessage sent = (SentMessage)msg.Tag;
    OnMessageSent(sent.Player, sent.Data);
  }

  void RawSend(ServerPlayer toWhom, byte[] data, Stream attachedStream, SendFlag flags, int timeoutMs, object orig)
  {
    RawSend(toWhom, data, 0, data.Length, attachedStream, flags, timeoutMs, orig);
  }

  void RawSend(ICollection<ServerPlayer> toWhom, byte[] data, Stream attachedStream,
               SendFlag flags, int timeoutMs, object orig)
  {
    RawSend(toWhom, data, 0, data.Length, attachedStream, flags, timeoutMs, orig);
  }

  void RawSend(ServerPlayer toWhom, byte[] data, int index, int length, Stream attachedStream,
               SendFlag flags, int timeoutMs, object orig)
  {
    if(toWhom == null) throw new ArgumentNullException();
    if(timeoutMs < 0) throw new ArgumentOutOfRangeException();
    if(data != orig) flags |= SendFlag.NoCopy;

    if(toWhom.DelayedDrop)
    {
      throw new ArgumentException("The player is currently being dropped, so sending data to it is not allowed.");
    }
    toWhom.Link.Send(data, index, length, attachedStream, flags, timeoutMs, new SentMessage(toWhom, orig));
  }

  void RawSend(ICollection<ServerPlayer> players, byte[] data, int index, int length, Stream attachedStream,
               SendFlag flags, int timeoutMs, object orig)
  {
    if(players == null) throw new ArgumentNullException();
    if(timeoutMs < 0) throw new ArgumentOutOfRangeException();
    if(data != orig) flags |= SendFlag.NoCopy;

    if(players == this.players) Monitor.Enter(this); // we have to lock if the collection is our player collection,
    try                                              // since it could be modified by the background thread
    {
      if(attachedStream != null)
      {
        // if there's an attached stream and we're sending to multiple players, we need to wrap it with a reference-
        // counted stream so it'll stay open until all the links finish with it
        int sendingTo = 0;
        foreach(ServerPlayer player in players)
        {
          if(!player.DelayedDrop) sendingTo++;
        }
        if(sendingTo > 1) attachedStream = new ReferenceCountedStream(attachedStream, sendingTo);
      }

      foreach(ServerPlayer player in players)
      {
        if(!player.DelayedDrop)
        {
          try
          {
            player.Link.Send(data, index, length, attachedStream, flags, timeoutMs, new SentMessage(player, orig));
          }
          catch(ConnectionLostException) { }
        }
        else if(players != this.players)
        {
          throw new ArgumentException("A player is currently being dropped, so sending data to it is not allowed.");
        }
      }
    }
    finally
    {
      if(players == this.players) Monitor.Exit(this);
    }
  }

  void RawSendToAllExcept(ServerPlayer player, byte[] data, Stream attachedStream,
                          SendFlag flags, int timeoutMs, object orig)
  {
    RawSendToAllExcept(player, data, 0, data.Length, attachedStream, flags, timeoutMs, orig);
  }

  void RawSendToAllExcept(ServerPlayer player, byte[] data, int index, int length, Stream attachedStream,
                          SendFlag flags, int timeoutMs, object orig)
  {
    if(timeoutMs < 0) throw new ArgumentOutOfRangeException();
    if(data != orig) flags |= SendFlag.NoCopy;

    lock(this)
    {
      if(attachedStream != null)
      {
        // if there's an attached stream and we're sending to multiple players, we need to wrap it with a reference-
        // counted stream so it'll stay open until all the links finish with it
        int sendingTo = 0;
        foreach(ServerPlayer p in players)
        {
          if(p != player && !p.DelayedDrop) sendingTo++;
        }
        if(sendingTo > 1) attachedStream = new ReferenceCountedStream(attachedStream, sendingTo);
      }

      foreach(ServerPlayer p in players)
      {
        if(p != player && !p.DelayedDrop)
        {
          try { p.Link.Send(data, index, length, attachedStream, flags, timeoutMs, new SentMessage(p, orig)); }
          catch(ConnectionLostException) { }
        }
      }
    }
  }

  void ThreadFunc()
  {
    try
    {
      List<ServerPlayer> disconnected = new List<ServerPlayer>();
      while(!quit)
      {
        bool didSomething = false;

        lock(this)
        {
          while(listening && server.Pending())
          {
            Socket sock = server.AcceptSocket();
            try
            {
              ServerPlayer player = new ServerPlayer(new NetLink(sock), nextID++);
              if(!quit && OnPlayerConnecting(player))
              {
                player.Link.LagAverage      = lagAverage;
                player.Link.LagVariance     = lagVariance;
                player.Link.MessageSent    += OnMessageSent;
                player.Link.RemoteReceived += OnRemoteReceived;
                players.Add(player);
                links.Add(player.Link);
                // send a handshaking packet just so that the client will wait until we've hooked up event handlers
                // before sending anything
                player.Link.Send(new byte[0], SendFlag.Reliable);
                OnPlayerConnected(player);
              }
              else sock.Close();
            }
            catch(SocketException) { sock.Close(); }
            catch(NetworkException) { sock.Close(); }
          }

          for(int i=0; i<players.Count; i++)
          {
            ServerPlayer player = players[i];
            try
            {
              if(player.DelayedDrop && Timing.InternalMsecs-player.DropStart >= player.DropDelay) player.Link.Close();

              player.Link.SendPoll(); // ReceivePoll() is called by ReceiveMessage(), but we need to call SendPoll() ourselves
              LinkMessage msg = player.Link.ReceiveMessage();
              if(msg != null)
              {
                didSomething = true;
                OnMessageReceived(player, cvt.Deserialize(msg));
              }
              else
              {
                if(player.DelayedDrop && player.Link.SendQueueLength == 0) player.Link.Close();
                if(!player.Link.IsConnected)
                {
                  didSomething = true;
                  players.RemoveAt(i);
                  links.RemoveAt(i);
                  i--;
                  disconnected.Add(player);
                }
              }
            }
            catch(SocketException) { }
          }

          if(disconnected != null && disconnected.Count != 0)
          {
            foreach(ServerPlayer p in disconnected) OnPlayerDisconnected(p);
            disconnected.Clear();
          }
        }

        if(!didSomething)
        {
          if(links.Count != 0) NetLink.WaitForEvent(links, 250);
          else Thread.Sleep(250);
        }
      }
    }
    catch(Exception e)
    {
      try
      {
        if(Events.Events.Initialized) Events.Events.PushEvent(new ExceptionEvent(ExceptionLocation.NetworkThread, e));
      }
      catch { throw; }
    }
  }

  List<ServerPlayer> players = new List<ServerPlayer>();
  List<NetLink> links = new List<NetLink>();
  MessageConverter cvt = new MessageConverter();
  TcpListener server;
  Thread thread;
  int nextID=1, lagAverage, lagVariance;
  bool quit, listening;
}


} // namespace GameLib.Network