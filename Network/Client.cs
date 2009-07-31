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
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.IO;

namespace GameLib.Network
{

#region ClientHandler
/// <summary>This delegate is used for the <see cref="Client.Connected"/> and <see cref="Client.Disconnected"/>
/// events.
/// </summary>
public delegate void ClientHandler(Client client);
#endregion

#region ClientMessageHandler
/// <summary>This delegate is used for the <see cref="Client.MessageReceived"/>, <see cref="Client.RemoteReceived"/>,
/// and <see cref="Client.MessageSent"/> events. It receives a reference to the <see cref="Client"/> class and the
/// message that was sent or received.
/// </summary>
public delegate void ClientMessageHandler(Client client, object msg);
#endregion

#region Client
/// <summary>This class represents the client in a client-server networking setup.</summary>
public class Client
{
  /// <summary>Initializes a new instance of the <see cref="Client"/> class.</summary>
  public Client() { }

  /// <summary>Initializes a new instance of the <see cref="Client"/> class and connects to the given host and port.</summary>
  /// <param name="hostname">The name of the host to connect to.</param>
  /// <param name="port">The port to connect to.</param>
  public Client(string hostname, int port)
  {
    Connect(hostname, port);
  }
  
  /// <summary>Initializes a new instance of the <see cref="Client"/> class and connects to the given endpoint.</summary>
  /// <param name="remote">The <see cref="IPEndPoint"/> to connect to.</param>
  public Client(IPEndPoint remote)
  {
    Connect(remote);
  }

  /// <summary>This destructor calls <see cref="Disconnect"/> to close the client connection.</summary>
  ~Client()
  {
    Disconnect();
  }

  /// <summary>This event is raised when the client is connected.</summary>
  public event ClientHandler Connected;

  /// <summary>This event is raised when the client is disconnected.</summary>
  public event ClientHandler Disconnected;
  
  /// <summary>This event is raised when a message is received from the server.</summary>
  public event ClientMessageHandler MessageReceived;
  
  /// <summary>This event is raised when remote host receives a message.</summary>
  /// <remarks>Note that this event will only be raised for messages sent with the
  /// <see cref="SendFlag.NotifyReceived"/> flag.
  /// </remarks>
  public event ClientMessageHandler RemoteReceived;
  
  /// <summary>This event is raised when a message is sent over the network.</summary>
  /// <remarks>Note that this event will only be raised for messages sent with the <see cref="SendFlag.NotifySent"/>
  /// flag.
  /// </remarks>
  public event ClientMessageHandler MessageSent;

  /// <include file="../documentation.xml" path="//Network/Common/IsConnected/*"/>
  public bool IsConnected
  {
    get
    {
      NetLink link = this.link; // cache in a local to prevent it from being pulled out from under us
      return link == null ? false : link.IsConnected;
    }
  }

  /// <include file="../documentation.xml" path="//Network/Common/LagAverage/*"/>
  public int LagAverage
  {
    get { return AssertLink().LagAverage; }
    set { AssertLink().LagAverage = value; }
  }

  /// <include file="../documentation.xml" path="//Network/Common/LagVariance/*"/>
  public int LagVariance
  {
    get { return AssertLink().LagVariance; }
    set { AssertLink().LagVariance = value; }
  }

  /// <summary>Gets or sets the message converter used to convert messages sent and received by this client. If not
  /// set, a default instance will be used. You can either register types on this default instance or assign a new
  /// instance that already has types registered. This property can only be set when the client is not connected.
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

  /// <summary>Returns the remote endpoint to which the client is connected.</summary>
  public IPEndPoint RemoteEndPoint
  {
    get { return AssertLink().RemoteEndPoint; }
  }

  /// <summary>Gets or sets whether a background thread will automatically handle sending and receiving message data
  /// through the underlying socket. If set to false, you must call <see cref="Poll()"/> periodically. The default is
  /// true.
  /// </summary>
  public bool UseThread
  {
    get { return useThread; }
    set
    {
      if(value != UseThread)
      {
        useThread = value;
        if(!value && thread != null) KillThread();
        else if(value && thread == null) StartThread();
      }
    }
  }

  /// <summary>Connects to the given host and port.</summary>
  /// <param name="hostname">The name of the host to connect to.</param>
  /// <param name="port">The name of the port to connect to.</param>
  public void Connect(string hostname, int port)
  {
    Connect(new IPEndPoint(Dns.GetHostEntry(hostname).AddressList[0], port));
  }

  /// <summary>Connects to the given remote endpoint.</summary>
  /// <param name="remote">The <see cref="IPEndPoint"/> to connect to.</param>
  public void Connect(IPEndPoint remote)
  {
    Disconnect();
    quitThread = delayedDrop = false;
    link = new NetLink(remote);

    byte[] msg = link.Receive(5000);
    if(msg == null || msg.Length != 0)
    {
      Disconnect();
      throw new HandshakeException("Timed out while waiting for handshake packet.");
    }

    link.MessageSent    += OnMessageSent;
    link.RemoteReceived += OnRemoteReceived;

    if(UseThread) StartThread();
    OnConnected();
  }

  /// <summary>Disconnects from the server with a delay to allow incoming and outgoing messages to be sent.</summary>
  /// <remarks>The connection will be terminated only after all outgoing messages have been sent and all incoming
  /// messages have been processed.
  /// </remarks>
  public void DelayedDisconnect()
  {
    DelayedDisconnect(int.MaxValue);
  }

  /// <summary>Disconnects from the server with a delay to allow incoming and outgoing messages to be sent.</summary>
  /// <param name="timeoutMs">The amount of time to wait for all remaining messages to be sent, in milliseconds.</param>
  /// <remarks>The connection will be terminated after all outgoing messages have been sent and all incoming
  /// messages have been processed, or the timeout expires.
  /// </remarks>
  public void DelayedDisconnect(int timeoutMs)
  {
    if(timeoutMs < 0) throw new ArgumentOutOfRangeException("timeoutMs", "Cannot be negative");
    dropDelay   = (uint)timeoutMs;
    dropStart   = Timing.InternalMsecs;
    delayedDrop = true;
  }

  /// <summary>Disconnects from the server.</summary>
  /// <remarks>This method immediately terminates the connection with the server.</remarks>
  public void Disconnect()
  {
    if(thread != null) KillThread();

    if(link != null)
    {
      if(link.IsConnected)
      {
        link.Close();
        OnDisconnected();
      }
      link = null;
    }
  }

  /// <include file="../documentation.xml" path="//Network/Common/GetQueueStatus/*"/>
  public QueueStatus GetQueueStatus(QueueStatusFlag flags)
  {
    return AssertLink().GetQueueStatus(flags);
  }

  /// <summary>Handles sending and receiving data through the underlying socket. If <see cref="UseThread"/> is false,
  /// this must be called relatively often to prevent data from being lost. If <see cref="UseThread"/> is true, the
  /// processing will be performed automatically and this method must not be called.
  /// </summary>
  /// <returns>True if a message was received and false if not.</returns>
  public void Poll()
  {
    if(UseThread) throw new InvalidOperationException();
    PollCore();
  }

  /// <include file="../documentation.xml" path="//Network/Client/Send/*[self::Common or self::byteData]/*"/>
  public void Send(byte[] data)
  {
    DoSend(data, 0, data.Length, null, DefaultFlags, 0, data);
  }
  
  /// <include file="../documentation.xml" path="//Network/Client/Send/*[self::Common or self::byteData or self::Length or self::Index]/*"/>
  public void Send(byte[] data, int index, int length)
  {
    DoSend(data, index, length, null, DefaultFlags, 0, data);
  }
  
  /// <include file="../documentation.xml" path="//Network/Client/Send/*[self::Common or self::byteData or self::Flags]/*"/>
  public void Send(byte[] data, SendFlag flags)
  {
    DoSend(data, 0, data.Length, null, flags, 0, data);
  }
  
  /// <include file="../documentation.xml" path="//Network/Client/Send/*[self::Common or self::byteData or self::Index or self::Length or self::Flags]/*"/>
  public void Send(byte[] data, int index, int length, SendFlag flags)
  {
    DoSend(data, index, length, null, flags, 0, data);
  }
  
  /// <include file="../documentation.xml" path="//Network/Client/Send/*[self::Common or self::byteData or self::Flags or self::Timeout]/*"/>
  public void Send(byte[] data, SendFlag flags, int timeoutMs)
  {
    DoSend(data, 0, data.Length, null, flags, timeoutMs, data);
  }
  
  /// <include file="../documentation.xml" path="//Network/Client/Send/*[self::Common or self::byteData or self::Index or self::Length or self::Flags or self::Timeout]/*"/>
  public void Send(byte[] data, int index, int length, SendFlag flags, int timeoutMs)
  {
    DoSend(data, index, length, null, flags, timeoutMs, data);
  }

  /// <include file="../documentation.xml" path="//Network/Client/Send/*[self::Common or self::objData]/*"/>
  public void Send(object data)
  {
    Send(data, DefaultFlags, 0);
  }
  
  /// <include file="../documentation.xml" path="//Network/Client/Send/*[self::Common or self::objData or self::Flags]/*"/>
  public void Send(object data, SendFlag flags)
  {
    Send(data, flags, 0);
  }
  
  /// <include file="../documentation.xml" path="//Network/Client/Send/*[self::Common or self::objData or self::Flags or self::Timeout]/*"/>
  public void Send(object data, SendFlag flags, int timeoutMs)
  {
    AssertLink();
    Stream attachedStream;
    DoSend(cvt.Serialize(data, out attachedStream), attachedStream, flags, timeoutMs, data);
  }

  /// <summary>Raises the <see cref="Connected"/> event.</summary>
  /// <remarks>If you override this method, be sure to call the base class' implementation. The proper place
  /// to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnConnected()
  {
    if(Connected != null) Connected(this);
  }

  /// <summary>Raises the <see cref="Disconnected"/> event.</summary>
  /// <remarks>If you override this method, be sure to call the base class' implementation. The proper place
  /// to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnDisconnected()
  {
    if(Disconnected != null) Disconnected(this);
  }

  /// <summary>Raises the <see cref="MessageReceived"/> event.</summary>
  /// <param name="message">The message that was received.</param>
  /// <remarks>If you override this method, be sure to call the base class' implementation. The proper place
  /// to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnMessageReceived(object message)
  {
    ClientMessageHandler handler = MessageReceived;
    if(handler != null) handler(this, message);
  }

  /// <summary>Raises the <see cref="MessageSent"/> event.</summary>
  /// <param name="message">The message that was sent.</param>
  /// <remarks>This method is only called for messages that have requested notification via the
  /// <see cref="SendFlag.NotifySent"/> flag. If you override this method, be sure to call the base class'
  /// implementation. The proper place to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnMessageSent(object message)
  {
    ClientMessageHandler handler = MessageSent;
    if(handler != null) handler(this, message);
  }

  /// <summary>Raises the <see cref="RemoteReceived"/> event.</summary>
  /// <param name="message">The message that was received.</param>
  /// <remarks>This method is only called for messages that have requested notification via the
  /// <see cref="SendFlag.NotifyReceived"/> flag. If you override this method, be sure to call the base class'
  /// implementation. The proper place to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnRemoteReceived(object message)
  {
    ClientMessageHandler cmh = RemoteReceived;
    if(cmh != null) cmh(this, message);
  }

  const SendFlag DefaultFlags = SendFlag.Reliable;

  void DoSend(byte[] data, Stream attachedStream, SendFlag flags, int timeoutMs, object orig)
  {
    DoSend(data, 0, data.Length, attachedStream, flags, timeoutMs, orig);
  }

  void DoSend(byte[] data, int index, int length, Stream attachedStream, SendFlag flags, int timeoutMs, object orig)
  {
    if(data != orig) flags |= SendFlag.NoCopy;
    link.Send(data, index, length, attachedStream, flags, timeoutMs, orig);
  }

  NetLink AssertLink()
  {
    NetLink link = this.link;
    if(link == null) throw new InvalidOperationException("Client is not connected.");
    return link;
  }

  void AssertClosed()
  {
    if(link != null) throw new InvalidOperationException("Client is already connected.");
  }

  void KillThread()
  {
    quitThread = true;
    thread.Join(1500);
    thread.Abort();
    thread = null;
  }

  bool PollCore()
  {
    if(delayedDrop && Timing.InternalMsecs-dropStart >= dropDelay) link.Close();

    // ReceivePoll() is called by ReceiveMessage as necessary, but we need to call SendPoll ourselves
    link.SendPoll();

    LinkMessage msg = link.ReceiveMessage();
    if(msg != null)
    {
      OnMessageReceived(cvt.Deserialize(msg));
      return true;
    }
    else
    {
      if(delayedDrop && link.SendQueueLength == 0) link.Close();

      if(!link.IsConnected)
      {
        link = null;
        OnDisconnected();
      }
    }

    return false;
  }

  void StartThread()
  {
    thread = new Thread(ThreadFunc);
    thread.Start();
  }

  void ThreadFunc()
  {
    try
    {
      while(!quitThread)
      {
        bool didSomething = false;
        try
        {
          didSomething = PollCore();
          if(!IsConnected) break;
        }
        catch(SocketException) { }

        if(!didSomething) link.WaitForEvent(250);
      }
    }
    catch(Exception e)
    {
      try
      {
        if(Events.Events.Initialized)
        {
          Events.Events.PushEvent(new Events.ExceptionEvent(Events.ExceptionLocation.NetworkThread, e));
        }
      }
      catch { throw; }
    }
  }

  void OnMessageSent(NetLink link, LinkMessage msg)
  {
    OnMessageSent(msg.Tag);
  }
  
  void OnRemoteReceived(NetLink link, LinkMessage msg)
  {
    OnRemoteReceived(msg.Tag);
  }

  MessageConverter cvt = new MessageConverter();
  NetLink link;
  Thread thread;
  uint dropStart, dropDelay;
  bool quitThread, delayedDrop, useThread=true;
}
#endregion

} // namespace GameLib.Network