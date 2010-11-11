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
using System.IO;
using System.Net;
using System.Net.Sockets;
using AdamMil.IO;
using AdamMil.Utilities;

namespace GameLib.Network
{

// TODO: do we need all this locking in NetLink? it's based on polling, so the locking could be pushed into the users. on the other hand, it's relatively fine-grained now.

#region Event handlers
/// <summary>This delegate is used for the <see cref="NetLink.Connected"/> and <see cref="NetLink.Disconnected"/>
/// events.
/// </summary>
public delegate void NetLinkHandler(NetLink link);

/// <summary>This delegate is used for the <see cref="NetLink.MessageSent"/> and <see cref="NetLink.RemoteReceived"/>
/// events.
/// </summary>
public delegate void LinkMessageHandler(NetLink link, LinkMessage msg);
#endregion

#region LinkMessage
/// <summary>This class represents a network message.</summary>
public sealed class LinkMessage : IDisposable
{
  internal LinkMessage() { }

  internal LinkMessage(ref NetLink.IncomingMessage m)
    : this(m.Buffer, 0, m.Buffer.Length, m.Stream, m.Flags, 0, null) { }

  internal LinkMessage(byte[] data, int index, int length, Stream attachedStream, SendFlag flags,
                       int timeout, object tag)
  {
    Data           = data;
    Index          = index;
    Length         = length;
    AttachedStream = attachedStream;
    Flags          = flags;
    Tag            = tag;
    if(timeout != 0) Deadline = Timing.InternalMilliseconds + (uint)timeout;

    if(attachedStream != null)
    {
      StreamPosition = attachedStream.Position;
      StreamLength   = (int)(attachedStream.Length - StreamPosition);
    }
  }

  /// <summary>Gets a stream attached to the message. The stream will be sent along with the message, but is not
  /// subject to the normal message length restraints. The stream should be closed after you're done with the message.
  /// This can be done by calling <see cref="Dispose"/>.
  /// </summary>
  public Stream AttachedStream
  {
    get; private set;
  }

  /// <summary>This property returns the array that contains the message data. Note that there may be additional data
  /// contained within <see cref="AttachedStream"/>.
  /// </summary>
  public byte[] Data
  {
    get; private set;
  }

  /// <summary>This property returns the index into <see cref="Data"/> at which the message data begins.</summary>
  public int Index
  {
    get; private set;
  }

  /// <summary>This property returns the length of the message data, in bytes.</summary>
  public int Length
  {
    get; private set;
  }

  /// <summary>This property returns the <see cref="SendFlag"/> value that was used to send the message.</summary>
  public SendFlag Flags
  {
    get; private set;
  }

  /// <summary>This property returns the context value that was passed to the Send() method.</summary>
  /// <remarks>Note that this value is not transmitted across the network, so it will be null for received messages.</remarks>
  public object Tag
  {
    get; set;
  }

  /// <summary>Disposes the <see cref="LinkMessage"/> by closing the <see cref="AttachedStream"/>, if it exists.</summary>
  public void Dispose()
  {
    Utility.Dispose(AttachedStream);
    AttachedStream = null;
  }

  /// <summary>Returns the message data. Note that calling this method may alter the values of <see cref="Index"/> and
  /// <see cref="Data"/>. Note that there may be additional data stored within the <see cref="AttachedStream"/>.
  /// </summary>
  /// <returns>An array of <see cref="System.Byte"/> containing only the message data. The array's length will
  /// be equal to <see cref="Length"/>. If no copying is necessary, the returned array will be identical to
  /// <see cref="Data"/>.
  /// </returns>
  public byte[] Shrink()
  {
    if(Length != Data.Length)
    {
      byte[] shrunk = new byte[Length];
      Array.Copy(Data, Index, shrunk, 0, Length);
      Index = 0;
      Data  = shrunk;
    }
    return Data;
  }

  internal int StreamLength
  {
    get; private set;
  }

  internal long StreamPosition
  {
    get; private set;
  }

  internal uint Deadline, Lag;
}
#endregion

#region QueueStatus
/// <summary>This represents the status of a send and/or receive queue.</summary>
/// <remarks>An instance of this structure is returned by the <see cref="NetLink.GetQueueStatus"/> method.
/// It provides information about the number and size of messages sitting in the send and receive queues.
/// </remarks>
public struct QueueStatus
{
  /// <summary>The number of messages waiting in the send queue(s).</summary>
  public int SendMessages;
  /// <summary>The total size of the messages waiting in the send queue(s).</summary>
  public int SendBytes;
  /// <summary>The number of messages waiting in the receive queue.</summary>
  public int ReceiveMessages;
  /// <summary>The total size of the messages waiting in the receive queue.</summary>
  public int ReceiveBytes;
}
#endregion

#region QueueStatusFlag
/// <summary>This enumeration is passed to <see cref="NetLink.GetQueueStatus"/> in order to control what data will
/// be returned. The flags can be ORed together to combine their effects.
/// </summary>
[Flags]
public enum QueueStatusFlag
{
  /// <summary>Data about the low-priority send queue will be included.</summary>
  LowPriority=1,
  /// <summary>Data about the normal-priority send queue will be included.</summary>
  NormalPriority=2,
  /// <summary>Data about the high-priority send queue will be included.</summary>
  HighPriority=4,
  /// <summary>Data about the receive queue will be included.</summary>
  ReceiveQueue=8,
  /// <summary>Data about all send queues will be included.</summary>
  SendStats=LowPriority | NormalPriority | HighPriority,
  /// <summary>Data about the receive queue and all send queues will be returned.</summary>
  AllStats=SendStats|ReceiveQueue
}
#endregion

#region SendFlag
/// <summary>This enumeration is used to control how a message will be sent.
/// The flags can be ORed together to combine their effects.
/// </summary>
[Flags]
public enum SendFlag : byte
{
  /// <summary>
  /// The message will be sent at normal priority, unreliably, and it might arrive out of order.
  /// No notification events will be raised, and a copy of the message data well be created if the networking
  /// engine can't prove that it's unnecessary.
  /// </summary>
  None=0,

  /// <summary>The message will be sent reliably, meaning that it's guaranteed to arrive, in order, as long as the link
  /// isn't broken.
  /// </summary>
  Reliable=1,

  /// <summary>This flag causes a notification event to be raised when the message is actually sent out over the
  /// network.
  /// </summary>
  NotifySent=2,

  /// <summary>This flag causes a notification event to be raised when the remote host receives and processes the
  /// message. Note that specifying this flag uses additional network and machine resources, as the remote host must
  /// generate a reply saying that it received the message, and the local host must hold the message in memory
  /// until it receives the reply or it determines that the messages was ignored or not received. (This flag is not
  /// implemented yet.)
  /// </summary>
  NotifyReceived=4,

  /// <summary>This flag tells the networking engine to not make a copy of the message data. If you know that that
  /// the data passed to the Send() function will not be altered after the engine is done with it, then you can
  /// safely use this flag. If this flag is not used, the engine will copy the message data into a separate buffer.
  /// Note that the networking engine can ignore this flag if it wishes.
  /// </summary>
  NoCopy=8,

  /// <summary>The message will be sent with low priority. This means it will be sent only if no messages of higher
  /// priority are waiting to be sent.
  /// </summary>
  LowPriority=16,

  /// <summary>The message will be sent with high priority. This means it will be sent before all messages of lower
  /// priority.
  /// </summary>
  HighPriority=32,
}
#endregion

#region NetLink
/// <summary>This class represents a message-oriented network connection.</summary>
/// <remarks>The low-level implementation of this class is not defined. It may use any IP protocol to perform its data
/// transfer, including TCP, UDP, or something else entirely. It may make multiple network connections. Because of
/// this, it is only suitable for communicating with other NetLink objects from a compatible version of GameLib.
/// However, communications are initiated using TCP, so the following code is valid:
/// <code>
/// TcpListener sv = new TcpListener(endPoint);
/// sv.Start();
/// NetLink client = new NetLink(endPoint);
/// NetLink server = new NetLink(sv.AcceptSocket());
/// sv.Stop();
/// </code>
/// </remarks>
public class NetLink
{
  /// <summary>Initializes the class, but does not open any connection.</summary>
  public NetLink() { }

  /// <summary>Initializes the class and connects to the specified endpoint.</summary>
  /// <param name="host">The name of the host to connect to.</param>
  /// <param name="port">The port to connect to.</param>
  public NetLink(string host, int port) { Open(host, port); }

  /// <summary>Initializes the class and connects to the specified endpoint.</summary>
  /// <param name="remote">The <see cref="IPEndPoint"/> to connect to.</param>
  public NetLink(IPEndPoint remote) { Open(remote); }

  /// <summary>Initializes the class from a TCP connection that's already connected.</summary>
  /// <param name="tcp">A TCP stream <see cref="Socket"/> that's already connected.</param>
  public NetLink(Socket tcp) { Open(tcp); }

  /// <summary>This value can be passed to methods expecting a timeout to indicate that the method should wait
  /// forever if necessary.
  /// </summary>
  public const int NoTimeout = -1;

  /// <summary>This event is raised when the network link is connected.</summary>
  public event NetLinkHandler Connected;

  /// <summary>This event is raised when the network link is disconnected.</summary>
  public event NetLinkHandler Disconnected;

  /// <summary>This event is raised when a message has been received. The message is still within the receive queue,
  /// and can be retrieved using <see cref="Receive"/> or <see cref="ReceiveMessage"/>.
  /// </summary>
  public event NetLinkHandler MessageReceived;

  /// <summary>This event is raised when a message is sent over the network.</summary>
  /// <remarks>Note that the message must have been sent with the <see cref="SendFlag.NotifySent"/> flag
  /// in order for this event to be raised.
  /// </remarks>
  public event LinkMessageHandler MessageSent;

  /// <summary>This event is raised when a message is received by the remote host.</summary>
  /// <remarks>Note that the message must have been sent with the <see cref="SendFlag.NotifyReceived"/> flag
  /// in order for this event to be raised. Also note that this is not currently implemented.
  /// </remarks>
  // TODO: implement this
  public event LinkMessageHandler RemoteReceived;

  /// <include file="../documentation.xml" path="//Network/Common/IsConnected/*"/>
  public bool IsConnected
  {
    get
    {
      if(!connected) return false;
      Socket tcp = this.tcp;
      return tcp != null && tcp.Connected;
    }
  }

  /// <include file="../documentation.xml" path="//Network/Common/LagAverage/*"/>
  public int LagAverage
  {
    get { return lagAverage; }
    set
    {
      if(value < 0) throw new ArgumentOutOfRangeException();
      lagAverage = value;
    }
  }

  /// <include file="../documentation.xml" path="//Network/Common/LagVariance/*"/>
  public int LagVariance
  {
    get { return lagVariance; }
    set
    {
      if(value < 0) throw new ArgumentOutOfRangeException();
      lagVariance = value;
    }
  }

  /// <summary>Returns true if a message is waiting to be retrieved.</summary>
  public bool MessageWaiting
  {
    get
    {
      lock(recv)
      {
        if(recv.Count == 0)
        {
          ReceivePoll();
          return recv.Count != 0;
        }
        else return true;
      }
    }
  }

  /// <summary>Gets or sets the maximum message size that can be sent or received through this link, in bytes. The
  /// default is 64 kilobytes. This property can be increased up to one gigabyte, but increasing it to a high value is
  /// NOT recommended, because messages are stored in memory before being processed on the receiving side, and large
  /// messages will cause the receiving side to allocate too much memory and spend too much time collecting garbage. If
  /// you need to send large messages, it's a better idea for the message to use attached streams, which are not held
  /// in memory on the receiving side.
  /// </summary>
  public int MaxMessageSize
  {
    get { return maxMessageSize; }
    set
    {
      if(value < 0 || value > 1024*1024*1024) throw new ArgumentOutOfRangeException();
      maxMessageSize = value;
    }
  }

  /// <summary>Gets or sets the maximum attached stream size that can be sent or received through this link, in bytes.
  /// The default is one megabyte.
  /// </summary>
  public int MaxStreamSize
  {
    get { return maxStreamSize; }
    set
    {
      if(value < 0) throw new ArgumentOutOfRangeException();
      maxStreamSize = value;
    }
  }

  /// <summary>Returns the remote endpoint to which this link is connected, or null if the link is closed.</summary>
  public IPEndPoint RemoteEndPoint
  {
    get { return tcp == null ? null : (IPEndPoint)tcp.RemoteEndPoint; }
  }

  /// <summary>Connects to a remote host.</summary>
  /// <param name="host">The name of the host to connect to.</param>
  /// <param name="port">The port to connect to.</param>
  public void Open(string host, int port)
  {
    Open(new IPEndPoint(Dns.GetHostEntry(host).AddressList[0], port));
  }

  /// <summary>Connects to a remote host.</summary>
  /// <param name="remote">The <see cref="IPEndPoint"/> specifying the address and port to connect to.</param>
  public void Open(IPEndPoint remote)
  {
    Socket sock = null;
    sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    sock.Connect(remote);
    Open(sock);
  }

  /// <summary>Initializes the connection from a socket that's already connected.</summary>
  /// <param name="tcp">A TCP stream <see cref="Socket"/> that's already connected.</param>
  public void Open(Socket tcp)
  {
    if(tcp == null) throw new ArgumentNullException();
    if(!tcp.Connected) throw new ArgumentException("The socket must be connected already!");

    Disconnect();
    IPEndPoint localTcp = (IPEndPoint)tcp.LocalEndPoint, remote = (IPEndPoint)tcp.RemoteEndPoint;

    Socket udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    udp.Bind(new IPEndPoint(localTcp.Address, localTcp.Port));  // bind the udp to the same port/interface
    udp.Connect(remote);

    tcp.Blocking = false;
    udp.Blocking = false;

    #if WINDOWS
    udpMax   = (int)udp.GetSocketOption(SocketOptionLevel.Socket, (SocketOptionName)0x2003);
    #else
    udpMax   = something;
    #endif
    recv     = new Queue<LinkMessage>();
    recvBuf  = new byte[4];
    sendBuf  = new byte[512];
    this.tcp = tcp;
    this.udp = udp;
    connected = true;
    OnConnected();
  }

  /// <summary>Closes the connection to the remote host.</summary>
  /// <remarks>Calling this method is equivalent to calling <see cref="Close(bool)"/> and passing true. See
  /// <see cref="Close(bool)"/> for more information.
  /// </remarks>
  public void Close()
  {
    Close(true);
  }

  /// <summary>Closes the connection to the remote host.</summary>
  /// <param name="force">If false, the connection is shut down and sending is disabled, but it will not be closed
  /// until all data has been received (you must retrieve all messages with one of the message retrieval functions).
  /// If true, the connection is forcibly closed and any unprocessed data will be lost.
  /// </param>
  public void Close(bool force)
  {
    // clear send queues
    sendQueue = 0;

    ClearSendQueue(ref low);
    ClearSendQueue(ref norm);
    ClearSendQueue(ref high);

    if(tcp != null)
    {
      lock(recv)
      {
        if(force) Disconnect();
        else tcp.Shutdown(SocketShutdown.Send);
      }
    }

    // TODO: can we remove this line?
    sendQueue = 0; // duplicated just in case SendPoll() changes it
  }

  /// <include file="../documentation.xml" path="//Network/Common/GetQueueStatus/*"/>
  public QueueStatus GetQueueStatus(QueueStatusFlag flags)
  {
    QueueStatus status = new QueueStatus();

    if(connected)
    {
      if((flags & QueueStatusFlag.LowPriority) != 0) AddSendQueueStatistics(ref status, low);
      if((flags & QueueStatusFlag.NormalPriority) != 0) AddSendQueueStatistics(ref status, norm);
      if((flags & QueueStatusFlag.HighPriority) != 0) AddSendQueueStatistics(ref status, high);
    }

    if((flags & QueueStatusFlag.ReceiveQueue) != 0 && recv != null && recv.Count != 0)
    {
      lock(recv)
      {
        foreach(LinkMessage msg in recv)
        {
          status.ReceiveMessages++;
          status.ReceiveBytes += msg.Length;
        }
      }
    }

    return status;
  }

  /// <summary>Returns the next message if it exists, without removing it from the receive queue.</summary>
  /// <returns>A <see cref="LinkMessage"/> representing the next message, or null if no message is waiting.</returns>
  public LinkMessage PeekMessage()
  {
    if(recv == null) throw new InvalidOperationException("Link has not been opened");

    lock(recv)
    {
      if(recv.Count == 0) ReceivePoll();
      return recv.Count != 0 ? recv.Peek() : null;
    }
  }

  /// <summary>Returns the next message.</summary>
  /// <returns>An array of <see cref="System.Byte"/> containing the message data.</returns>
  public byte[] Receive()
  {
    return Receive(NoTimeout);
  }

  /// <summary>Returns the next message, or null if no message is received within the given timeout.</summary>
  /// <param name="timeoutMs">The number of milliseconds to wait for a message. If the queue is empty and no message
  /// arrives within the given time frame, null will be returned. If <see cref="NetLink.NoTimeout"/> is passed, the
  /// method will wait forever forever for a message (although it will return early if the connection is broken).
  /// </param>
  /// <returns>An array of <see cref="System.Byte"/> containing the message data, or null if no message was received
  /// in time.
  /// </returns>
  public byte[] Receive(int timeoutMs)
  {
    LinkMessage m = ReceiveMessage();
    if(m == null)
    {
      if(timeoutMs != 0 && WaitForEvent(timeoutMs))
      {
        lock(recv) m = recv.Count==0 ? null : recv.Dequeue();
      }
    }
    return m == null ? null : m.Shrink();
  }

  /// <summary>Returns the next message if it exists.</summary>
  /// <returns>A <see cref="LinkMessage"/> representing the next message, or null if no message is waiting.</returns>
  public LinkMessage ReceiveMessage()
  {
    if(recv == null) throw new InvalidOperationException("Link has not been opened");

    lock(recv)
    {
      if(recv.Count == 0) ReceivePoll();
      return recv.Count != 0 ? recv.Dequeue() : null;
    }
  }

  /// <include file="../documentation.xml" path="//Network/NetLink/Send/Common/*"/>
  public void Send(byte[] data)
  {
    Send(data, 0, data.Length, null, DefaultFlags, 0, null);
  }

  /// <include file="../documentation.xml" path="//Network/NetLink/Send/*[self::Common or self::Index]/*"/>
  public void Send(byte[] data, int index, int length)
  {
    Send(data, index, length, null, DefaultFlags, 0, null);
  }

  /// <include file="../documentation.xml" path="//Network/NetLink/Send/*[self::Common or self::Index or self::Flags]/*"/>
  public void Send(byte[] data, int index, int length, SendFlag flags)
  {
    Send(data, index, length, null, flags, 0, null);
  }

  /// <include file="../documentation.xml" path="//Network/NetLink/Send/*[self::Common or self::Flags]/*"/>
  public void Send(byte[] data, SendFlag flags)
  {
    Send(data, 0, data.Length, null, flags, 0, null);
  }

  /// <include file="../documentation.xml" path="//Network/NetLink/Send/*[self::Common or self::Flags or self::Timeout]/*"/>
  public void Send(byte[] data, SendFlag flags, int timeoutMs)
  {
    Send(data, 0, data.Length, null, flags, timeoutMs, null);
  }

  /// <include file="../documentation.xml" path="//Network/NetLink/Send/*[self::Common or self::Stream or self::Flags]/*"/>
  public void Send(byte[] data, Stream attachedStream, SendFlag flags)
  {
    Send(data, 0, data.Length, attachedStream, flags, 0, null);
  }

  /// <include file="../documentation.xml" path="//Network/NetLink/Send/*[self::Common or self::Index or self::Stream or self::Flags]/*"/>
  public void Send(byte[] data, int index, int length, Stream attachedStream, SendFlag flags)
  {
    Send(data, index, length, attachedStream, flags, 0, null);
  }

  /// <include file="../documentation.xml" path="//Network/NetLink/Send/*[self::Common or self::Index or self::Stream or self::Flags or self::Timeout]/*"/>
  /// <param name="tag">Arbitrary data that will be associated with this message and that can be accessed via
  /// <see cref="LinkMessage.Tag"/>. The data is not examined or modified by the network engine. Note that this means
  /// <see cref="LinkMessage.Tag"/> will always be null on the receiving end.
  /// </param>
  public void Send(byte[] data, int index, int length, Stream attachedStream, SendFlag flags, int timeoutMs,
                   object tag)
  {
    if(!connected) throw new InvalidOperationException("Link is not open");

    if((flags & SendFlag.NotifyReceived) != 0)
    {
      throw new NotImplementedException("SendFlag.NotifyReceived is not implemented yet");
    }

    if(!IsConnected) throw new ConnectionLostException();
    if(length >= MaxMessageSize) throw new DataTooLargeException("The message data is too long.", MaxMessageSize);
    if(timeoutMs < 0) throw new ArgumentOutOfRangeException("timeoutMs", "cannot be negative");
    if(index < 0 || length < 0 || index+length > data.Length) throw new ArgumentOutOfRangeException("index or length");

    if(attachedStream != null)
    {
      if(!attachedStream.CanSeek)
      {
        throw new ArgumentException("Only seekable streams can be sent over the network.");
      }
      else if(attachedStream.Length - attachedStream.Position > MaxStreamSize)
      {
        throw new DataTooLargeException("The stream is too long.", MaxStreamSize);
      }
    }

    if((flags & SendFlag.NoCopy) == 0 && length != 0)
    {
      byte[] copy = new byte[length];
      Array.Copy(data, index, copy, 0, length);
      data  = copy;
      index = 0;
    }

    LinkMessage m = new LinkMessage(data, index, length, attachedStream, flags, timeoutMs, tag);
    if(lagAverage != 0 || lagVariance != 0)
    {
      uint lag;
      lock(random) lag = (uint)(random.Next((int)lagVariance*2) + lagAverage - lagVariance);
      if(lag != 0)
      {
        m.Lag       = Timing.InternalMilliseconds + lag;
        m.Deadline += lag;
      }
    }

    Queue<LinkMessage> queue;
    if((flags & SendFlag.HighPriority) != 0)
    {
      if(high == null) high = new Queue<LinkMessage>();
      queue = high;
    }
    else if((flags & SendFlag.LowPriority) != 0)
    {
      if(low == null) low = new Queue<LinkMessage>();
      queue = low;
    }
    else
    {
      if(norm == null) norm = new Queue<LinkMessage>();
      queue = norm;
    }

    lock(queue)
    {
      queue.Enqueue(m);
      sendQueue++;
    }

    SendPoll();

    if(!connected) throw new ConnectionLostException();
  }

  /// <summary>Checks to see if any messages have arrived on the network, and adds them to the receive queue if so.</summary>
  /// <remarks>It's normally unnecessary to call this method, as it will be called automatically by methods and
  /// properties that examine the receive queue. However, if those are not called often enough, the network buffer
  /// could overflow. In that case, it might be worthwhile to call this method every so often.
  /// </remarks>
  public void ReceivePoll()
  {
    lock(recv)
    {
      if(!connected) return;

      if(tcp != null)
      {
        try
        {
          while(true) // while data might be available
          {
            int avail = tcp.Available;
            if(avail == 0) // if no bytes are available to read, check for disconnection, which is signaled by the
            {              // socket being readable, but having 0 bytes of data
              if(tcp.Poll(0, SelectMode.SelectRead) && tcp.Available == 0) Disconnect();
              break; // in any case, there's nothing to do...
            }

            if(tcpMessage.State == State.Header)
            {
              if(avail >= HeaderSize) // if we're waiting for the header, and it's available now
              {
                tcp.Receive(recvBuf, HeaderSize, SocketFlags.None);
                ParseHeader(ref tcpMessage);
                avail -= HeaderSize;

                if(tcpMessage.Size > MaxMessageSize) // if the other side sent too much data, disconnect
                {
                  Disconnect();
                  break;
                }

                tcpMessage.Buffer = new byte[tcpMessage.Size];
                tcpMessage.State  = tcpMessage.HasStream ? State.StreamSize : State.Message;
              }
              else break; // if there's not enough data available for the header, then don't loop potentially infinitely
            }

            if(tcpMessage.State == State.StreamSize)
            {
              if(avail >= 4) // if we're waiting for the stream length, and it's available
              {
                tcp.Receive(recvBuf, 4, SocketFlags.None);
                tcpMessage.StreamLength = (int)IOH.ReadBE4U(recvBuf, 0);
                avail -= 4;

                if((uint)tcpMessage.StreamLength > (uint)MaxStreamSize) // if the other side sent too much data, disconnect
                {
                  Disconnect();
                  break;
                }

                tcpMessage.State = State.Message;
              }
              else break; // ditto
            }

            if(tcpMessage.State == State.Message && (avail != 0 || tcpMessage.Size == 0)) // now read the message data, if it's available and we're ready to
            {
              if(tcpMessage.Size != 0)
              {
                int read = tcp.Receive(tcpMessage.Buffer, tcpMessage.Index, tcpMessage.Size, SocketFlags.None);
                tcpMessage.Index += read;
                tcpMessage.Size  -= read;
                avail -= read; // this may cause 'avail' to drop below zero if more data arrived since 'avail' was set
              }

              if(tcpMessage.Size == 0) tcpMessage.State = tcpMessage.HasStream ? State.Stream : State.Done; // we've finished reading the message
            }

            if(tcpMessage.State == State.Stream && (avail > 0 || tcpMessage.StreamLength == 0))
            {
              if(tcpMessage.Stream == null) // if we haven't set up the stream yet...
              {
                tcpMessage.Filename = Path.GetTempFileName();
                tcpMessage.Stream   = new FileStream(tcpMessage.Filename, FileMode.Create, FileAccess.ReadWrite);
                tcpMessage.Stream.SetLength(tcpMessage.StreamLength);
                ResizeReceiveBuffer(4096); // enlarge the buffer for stream reading
              }

              while(avail > 0 && tcpMessage.StreamLength != 0)
              {
                int read = tcp.Receive(recvBuf, Math.Min(tcpMessage.StreamLength, recvBuf.Length), SocketFlags.None);
                tcpMessage.Stream.Write(recvBuf, 0, read);
                tcpMessage.StreamLength -= read;
                avail -= read;
              }

              if(tcpMessage.StreamLength == 0)
              {
                tcpMessage.Stream.Position = 0;
                tcpMessage.State = State.Done;
              }
            }

            if(tcpMessage.State == State.Done) // if we've finished reading the message
            {
              LinkMessage m = new LinkMessage(ref tcpMessage);
              tcpMessage.Buffer   = null;
              tcpMessage.Stream   = null;
              tcpMessage.Filename = null;
              tcpMessage.State    = State.Header;
              OnMessageReceived(m);
            }
          }
        }
        catch(SocketException) { Disconnect(); }
      }

      if(udp != null)
      {
        while(udp.Available != 0)
        {
          int avail = udp.Available;
          ResizeReceiveBuffer(avail);

          int read = udp.Receive(recvBuf, 0, avail, SocketFlags.None);
          if(read >= HeaderSize)
          {
            ParseHeader(ref udpMessage);
            if(HeaderSize+udpMessage.Size != read) continue; // ignore truncated messages (they should have been resent via tcp)

            udpMessage.Buffer = new byte[udpMessage.Size];
            Array.Copy(recvBuf, HeaderSize, udpMessage.Buffer, 0, udpMessage.Size);
            OnMessageReceived(new LinkMessage(ref udpMessage));
          }
        }
      }
    }
  }

  /// <summary>Attempts to send any messages waiting in the send queues.</summary>
  /// <remarks>Although this method is automatically called by <see cref="Send"/>, it should also be called
  /// occasionally to make sure any queued messages are sent. The call by <see cref="Send"/> may not actually send
  /// the message, for instance if the network send buffer is full.
  /// </remarks>
  public void SendPoll()
  {
    if(sendQueue == 0 || !connected) return;
    lock(this)
    {
      ProcessSendQueue(high);
      ProcessSendQueue(norm);
      ProcessSendQueue(low);
      SendCurrentMessage();
    }
  }

  /// <summary>Calling this method is equivalent to calling both <see cref="SendPoll"/> and <see cref="ReceivePoll"/>.</summary>
  public void Poll()
  {
    SendPoll();
    ReceivePoll();
  }

  /// <summary>This methods waits for a message to be received or the connection to be lost.</summary>
  /// <param name="timeoutMs">The maximum amount of time to wait for a message or disconnection. If
  /// <see cref="NoTimeout"/> is passed, the method will wait forever.
  /// </param>
  /// <returns>Returns true if the connection was lost or a message was received, and false otherwise.</returns>
  /// <remarks>This method will return immediately if the connection is broken.</remarks>
  public bool WaitForEvent(int timeoutMs)
  {
    if(timeoutMs < NoTimeout) throw new ArgumentOutOfRangeException();
    if(!IsConnected) return true;

    List<Socket> read = new List<Socket>(2), write = null;
    uint start = timeoutMs > 0 ? Timing.InternalMilliseconds : 0;
    while(true)
    {
      read.Add(tcp);
      read.Add(udp);

      if(sendQueue != 0)
      {
        if(write == null) write = new List<Socket>(2);
        write.Add(tcp);
        write.Add(udp);
      }

      Socket.Select(read, write, null, timeoutMs == NoTimeout ? -1 : timeoutMs*1000);

      if(read.Count != 0 && MessageWaiting || !IsConnected) return true;

      if(timeoutMs == 0)
      {
        break;
      }
      else if(timeoutMs != NoTimeout)
      {
        uint now = Timing.InternalMilliseconds, elapsed = now - start;
        if(elapsed >= (uint)timeoutMs) break;
        timeoutMs -= (int)elapsed;
        start = now;
      }

      if(write != null && write.Count != 0)
      {
        SendPoll();
        write.Clear();
      }

      read.Clear();
    }

    return false;
  }

  /// <summary>For a set of NetLink objects, waits for a message to be received or a connection to be lost.</summary>
  /// <param name="links">A collection of <see cref="NetLink"/> objects to check.</param>
  /// <param name="timeoutMs">The maximum amount of time to wait for a message or a disconnection. If
  /// <see cref="NoTimeout"/> is passed, the method will wait forever.
  /// </param>
  /// <returns>A list of <see cref="NetLink"/> objects that either have messages waiting or are disconnected.</returns>
  public unsafe static IList<NetLink> WaitForEvent(IList<NetLink> links, int timeoutMs)
  {
    if(links == null) throw new ArgumentNullException();
    if(timeoutMs < NoTimeout) throw new ArgumentOutOfRangeException();
    if(links.Count == 0) throw new ArgumentException("No links were given.");

    Dictionary<Socket, int> dict = new Dictionary<Socket, int>(links.Count*2);
    List<Socket> read = new List<Socket>(links.Count*2), write = null;
    List<NetLink> ret = new List<NetLink>();

    // build the map from Sockets back to NetLinks
    for(int i=0; i<links.Count; i++)
    {
      NetLink link = links[i];
      if(!link.IsConnected || link.MessageWaiting)
      {
        ret.Add(link);
      }
      else if(ret.Count == 0) // there's only a point in building the dictionary if we're not going to return immediately
      {
        dict[link.tcp] = i;
        dict[link.udp] = i;
      }
    }

    if(ret.Count != 0) return ret; // return immediately if any of the links are not connected or have messages

    uint start = timeoutMs > 0 ? Timing.InternalMilliseconds : 0;
    bool* added = stackalloc bool[links.Count]; // whether the given links have been added to the return array
    while(true)
    {
      foreach(NetLink link in links)
      {
        read.Add(link.tcp);
        read.Add(link.udp);

        if(link.sendQueue != 0)
        {
          if(write != null) write = new List<Socket>();
          write.Add(link.tcp);
          write.Add(link.udp);
        }
      }

      Socket.Select(read, write, null, timeoutMs == NoTimeout ? -1 : timeoutMs*1000);
      if(read.Count != 0)
      {
        foreach(Socket socket in read)
        {
          int index = dict[socket];
          if(!added[index])
          {
            NetLink link = links[index];
            if(link.MessageWaiting || !link.IsConnected)
            {
              ret.Add(link);
              added[index] = true;
            }
          }
        }
      }

      if(ret.Count != 0 || timeoutMs == 0)
      {
        break;
      }
      else if(write != null && write.Count != 0)
      {
        foreach(Socket socket in write) links[dict[socket]].SendPoll();
        write.Clear();
      }

      if(timeoutMs != NoTimeout)
      {
        uint now = Timing.InternalMilliseconds, elapsed = now - start;
        if(elapsed >= (uint)timeoutMs) break;
        timeoutMs -= (int)elapsed;
        start = now;
      }

      read.Clear();
    }

    return ret;
  }

  /// <summary>Raises the <see cref="Connected"/> event.</summary>
  /// <remarks>If you override this method, be sure to call the base class' implementation.</remarks>
  protected virtual void OnConnected()
  {
    if(Connected != null) Connected(this);
  }

  /// <summary>Raises the <see cref="Disconnected"/> event.</summary>
  /// <remarks>If you override this method, be sure to call the base class' implementation.</remarks>
  protected virtual void OnDisconnected()
  {
    if(Disconnected != null) Disconnected(this);
  }

  /// <summary>Raises the <see cref="MessageReceived"/> event.</summary>
  /// <param name="message">The <see cref="LinkMessage"/> that was received.</param>
  /// <remarks>If you override this method, be sure to call the base class' implementation. The proper place
  /// to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnMessageReceived(LinkMessage message)
  {
    lock(recv) recv.Enqueue(message);
    if(MessageReceived != null) MessageReceived(this);
  }

  /// <summary>Raises the <see cref="MessageSent"/> event.</summary>
  /// <param name="message">The <see cref="LinkMessage"/> that was sent.</param>
  /// <remarks>This method is only called for messages that have requested notification via the
  /// <see cref="SendFlag.NotifySent"/> flag. If you override this method, be sure to call the base class'
  /// implementation. The proper place to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnMessageSent(LinkMessage message)
  {
    if(MessageSent != null) MessageSent(this, message);
  }

  internal enum State : byte { Header, StreamSize, Message, Stream, Done }
  const SendFlag DefaultFlags = SendFlag.Reliable;
  const int HeaderSize = 4;

  void Disconnect()
  {
    if(connected)
    {
      connected = false;
      tcp.Close();
      udp.Close();
      tcp = udp = null;
      recvBuf = sendBuf = null;
      toSend = null;
      Utility.Dispose(tcpMessage.Stream);
      if(tcpMessage.Filename != null) File.Delete(tcpMessage.Filename);
      tcpMessage = udpMessage = new IncomingMessage();
      OnDisconnected();
    }
  }

  void ParseHeader(ref IncomingMessage msg)
  {
    uint header   = IOH.ReadBE4U(recvBuf, 0);
    msg.Size      = (int)(header & (1<<30)-1);
    msg.Index     = 0;
    msg.Flags     = (header & 1<<30) != 0 ? SendFlag.NotifyReceived : 0;
    msg.HasStream = (header & 1<<31) != 0;
  }

  void ProcessSendQueue(Queue<LinkMessage> queue)
  {
    if(queue != null && queue.Count != 0)
    {
      lock(queue)
      {
        LinkMessage msg;
        do
        {
          msg = queue.Peek();
          if(msg.Deadline != 0 && Timing.InternalMilliseconds > msg.Deadline)
          {
            queue.Dequeue().Dispose();
            sendQueue--;
            msg = null;
          }
        } while(msg == null && queue.Count != 0);

        if(msg != null && toSend == null && (msg.Lag == 0 || Timing.InternalMilliseconds < msg.Lag))
        {
          toSend    = queue.Dequeue();
          sendState = State.Header;
        }
      }
    }
  }

  void ResizeReceiveBuffer(int length)
  {
    if(length > recvBuf.Length) recvBuf = new byte[length];
  }

  void SendCurrentMessage()
  {
    if(toSend != null)
    {
      if(sendState == State.Header)
      {
        int dataLength = toSend.Length + HeaderSize;
        // TODO: we should be able to send small streams over UDP, but i that's probably not important in practice...
        bool sendWithTcp = (toSend.Flags & SendFlag.Reliable) != 0 || toSend.AttachedStream != null ||
                           dataLength > udpMax;

        // if we're sending with UDP, we'll need to fit the entire message into the send buffer, so resize it
        if(!sendWithTcp && sendBuf.Length < toSend.Length+HeaderSize) sendBuf = new byte[toSend.Length+HeaderSize];

        // add the header to the send buffer
        uint header = (uint)(toSend.Length | ((toSend.Flags & SendFlag.NotifyReceived) != 0 ? 1<<30 : 0) |
                             (toSend.AttachedStream != null ? 1<<31 : 0));
        IOH.WriteBE4U(sendBuf, 0, header);
        bytesInSendBuffer = 4;

        if(!sendWithTcp) // if we're sending with UDP...
        {
          Array.Copy(sendBuf, HeaderSize, toSend.Data, 0, toSend.Length); // add the message data
          try
          {
            udp.Send(sendBuf, 0, dataLength, SocketFlags.None); // and send
            sendState = State.Done;
            goto sentSuccessfully;
          }
          catch(SocketException ex)
          {
            if(ex.ErrorCode == Config.EMSGSIZE) // the message was too long
            {
              udpMax = dataLength-1; // update the maximum allowed UDP length
              sendWithTcp = true;    // and try again with TCP
            }
            else return; // try again later
          }
        }

        if(sendWithTcp)
        {
          if(toSend.AttachedStream != null)
          {
            IOH.WriteBE4(sendBuf, bytesInSendBuffer, toSend.StreamLength);
            bytesInSendBuffer = 8;
          }
          sentLength = -bytesInSendBuffer; // don't include the header bytes in the message bytes sent

          int dataInBuffer = Math.Min(sendBuf.Length-bytesInSendBuffer, toSend.Length);
          Array.Copy(toSend.Data, toSend.Index, sendBuf, bytesInSendBuffer, dataInBuffer);
          bytesInSendBuffer += dataInBuffer;
          sendBufferIndex    = 0;
        }

        sendState = State.Message;
      }

      try
      {
        if(sendState == State.Message)
        {
          // if we got here, we're sending with TCP
          do
          {
            if(bytesInSendBuffer == 0)
            {
              bytesInSendBuffer = Math.Min(sendBuf.Length, toSend.Length-sentLength);
              Array.Copy(toSend.Data, toSend.Index+sentLength, sendBuf, 0, bytesInSendBuffer);
              sendBufferIndex = 0;
            }

            int sent = tcp.Send(sendBuf, sendBufferIndex, bytesInSendBuffer, SocketFlags.None);
            sendBufferIndex   += sent;
            sentLength        += sent;
            bytesInSendBuffer -= sent;
            if(bytesInSendBuffer != 0) return; // the underlying send buffer is full
          } while(sentLength < toSend.Length);

          sendState  = toSend.AttachedStream != null ? State.Stream : State.Done;
          sentLength = 0;
        }

        if(sendState == State.Stream)
        {
          do
          {
            if(bytesInSendBuffer == 0)
            {
              bytesInSendBuffer = Math.Min(sendBuf.Length, toSend.StreamLength-sentLength);
              lock(toSend.AttachedStream)
              {
                toSend.AttachedStream.Position = toSend.StreamPosition + sentLength;
                toSend.AttachedStream.ReadOrThrow(sendBuf, 0, bytesInSendBuffer);
              }
              sendBufferIndex = 0;
            }

            int sent = tcp.Send(sendBuf, sendBufferIndex, bytesInSendBuffer, SocketFlags.None);
            sendBufferIndex   += sent;
            sentLength        += sent;
            bytesInSendBuffer -= sent;
            if(bytesInSendBuffer != 0) return; // the underlying send buffer is full
          } while(sentLength < toSend.StreamLength);

          sendState = State.Done;
        }
      }
      catch(SocketException ex)
      {
        if(ex.ErrorCode == Config.EWOULDBLOCK) return;
        else Disconnect();
      }

      sentSuccessfully:
      if(sendState == State.Done)
      {
        Utility.Dispose(ref toSend);
        sendState = State.Header;
        sendQueue--;
      }
    }
  }

  internal struct IncomingMessage
  {
    public byte[] Buffer;
    public Stream Stream;
    public string Filename;
    public int Size, Index, StreamLength;
    public SendFlag Flags;
    public State State;
    public bool HasStream;
  }

  internal int SendQueueLength
  {
    get { return sendQueue; }
  }

  Socket tcp, udp;
  Queue<LinkMessage> low, norm, high, recv;
  byte[] recvBuf, sendBuf;
  LinkMessage toSend;
  int sendQueue, udpMax, lagAverage, lagVariance, bytesInSendBuffer, sendBufferIndex, sentLength;
  int maxMessageSize = 64*1024, maxStreamSize = 1024*1024;
  IncomingMessage tcpMessage, udpMessage;
  State sendState;
  bool connected;

  static void AddSendQueueStatistics(ref QueueStatus status, Queue<LinkMessage> queue)
  {
    if(queue != null && queue.Count != 0)
    {
      lock(queue)
      {
        foreach(LinkMessage msg in queue)
        {
          status.SendMessages++;
          status.SendBytes += msg.Length;
        }
      }
    }
  }

  static void ClearSendQueue(ref Queue<LinkMessage> queue)
  {
    if(queue != null)
    {
      lock(queue)
      {
        foreach(LinkMessage m in queue) m.Dispose();
        queue.Clear();
        #pragma warning disable 728 // disable warning about assigning to the lock variable inside the lock.
        queue = null;               // we want it nulled before the lock is released
        #pragma warning restore 728
      }
    }
  }

  static readonly Random random = new Random();
}
#endregion

} // namespace GameLib.Network