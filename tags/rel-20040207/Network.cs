/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2004 Adam Milazzo

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

 // TODO: implement remote received notification
 
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.Reflection;
using GameLib.IO;

// TODO: stress test the locking, changes were made that may have destabilized it
// TODO: expose low-level networking by cleaning up the NetLink class and making it public

namespace GameLib.Network
{

#region Common types
public interface INetSerializeable
{ int  SizeOf();
  void SerializeTo(byte[] buf, int index);
  void DeserializeFrom(byte[] buf, int index);
}

public struct QueueStats
{ public int SendMessages, SendBytes, ReceiveMessages, ReceiveBytes;
}

[Flags]
public enum SendFlag
{ None=0, Reliable=1, Sequential=2, NotifySent=4, NotifyReceived=8, NoCopy=16,
  LowPriority=32, HighPriority=64,
  
  ReliableSequential = Reliable|Sequential,
  
  // these are not for Send(). they're used for GetQueueStats().
  NormalPriority=128, ReceiveQueue=256, SendStats=LowPriority|NormalPriority|HighPriority,
  AllStats=SendStats|ReceiveQueue
}

public sealed class MessageConverter
{ public int NumTypes { get { return typeIDs.Count; } }

  public void Clear()
  { types.Clear();
    typeIDs.Clear();
  }

  public void RegisterTypes(Type[] types) { foreach(Type type in types) RegisterType(type); }
  public void RegisterType(Type type)
  { TypeInfo info;
    if(type.GetInterface("GameLib.Network.INetSerializeable")!=null)
    { ConstructorInfo cons = type.GetConstructor(Type.EmptyTypes);
      if(cons==null) throw new ArgumentException(String.Format("Type {0} has no parameterless constructor", type));
      info = new TypeInfo(type, cons);
    }
    else info = new TypeInfo(type, null);
    int  i;
    for(i=0; i<types.Count; i++) if(types[i]==null) break;
    if(i==types.Count) types.Add(info); else types[i] = info;
    typeIDs[type] = i;
  }

  public void UnregisterTypes(Type[] types) { foreach(Type type in types) UnregisterType(type); }
  public void UnregisterType(Type type)
  { if(!typeIDs.Contains(type)) throw new ArgumentException(String.Format("{0} is not a registered type", type));
    int index = (int)typeIDs[type];
    typeIDs.Remove(type);
    types[index] = null;
  }
  
  public object ToObject(LinkMessage msg)
  { if(types.Count==0)
    { if(msg.Length==msg.Data.Length) return msg.Data;
      else
      { byte[] buf = new byte[msg.Length];
        Array.Copy(msg.Data, msg.Index, buf, 0, msg.Length);
        return buf;
      }
    }
    else
    { int id = IOH.ReadLE4(msg.Data, msg.Index);
      if(id==0)
      { byte[] buf = new byte[msg.Length-4];
        Array.Copy(msg.Data, msg.Index+4, buf, 0, msg.Length-4);
        return buf;
      }
      else
      { if(id<1 || id>types.Count || types[id-1]==null) return null;
        TypeInfo info = (TypeInfo)types[id-1];
        if(info.ConsInterface!=null)
        { INetSerializeable ns = (INetSerializeable)info.ConsInterface.Invoke(null);
          ns.DeserializeFrom(msg.Data, msg.Index+4);
          return ns;
        }
        else
          unsafe
          { fixed(byte* buf=msg.Data)
              return Marshal.PtrToStructure(new IntPtr(buf+msg.Index+4), info.Type);
          }
      }
    }
  }

  public byte[] FromObject(byte[] data) { return FromObject(data, 0, data.Length); }
  public byte[] FromObject(byte[] data, int index, int length)
  { if(types.Count==0)
      if(index==0 && length==data.Length) return data;
      else
      { byte[] ret = new byte[length];
        Array.Copy(data, index, ret, 0, length);
        return ret;
      }
    else
    { byte[] ret = new byte[length+4];
      Array.Copy(data, index, ret, 4, length);
      return ret;
    }
  }

  public byte[] FromObject(object obj)
  { if(obj is byte[]) return FromObject((byte[])obj);
    if(types.Count==0)
      throw new ArgumentException("If no types are registered, only byte[] can be sent.");
    else
    { Type type = obj.GetType();
      if(!typeIDs.Contains(type)) throw new ArgumentException(String.Format("{0} is not a registered type", type));
      int id = (int)typeIDs[type];
      TypeInfo info = (TypeInfo)types[id];
      byte[]   ret;
      if(info.ConsInterface!=null)
      { INetSerializeable ns = (INetSerializeable)obj;
        ret = new byte[ns.SizeOf()+4];
        ns.SerializeTo(ret, 4);
      }
      else
      { ret = new byte[Marshal.SizeOf(type)+4];
        unsafe { fixed(byte* ptr = ret) Marshal.StructureToPtr(obj, new IntPtr(ptr+4), false); }
      }
      IOH.WriteLE4(ret, 0, id+1);
      return ret;
    }
  }

  class TypeInfo
  { public TypeInfo(Type type, ConstructorInfo cons) { Type=type; ConsInterface=cons; }
    public Type Type;
    public ConstructorInfo ConsInterface;
  }

  ArrayList types   = new ArrayList();
  Hashtable typeIDs = new Hashtable();
}
#endregion

#region NetLink class and supporting types
public delegate void NetLinkHandler(NetLink link);
public delegate void LinkMessageHandler(NetLink link, LinkMessage msg);
public delegate bool LinkMessageRecvHandler(NetLink link, LinkMessage msg);

public class LinkMessage
{ internal LinkMessage() { }
  internal LinkMessage(byte[] data, int index, int length, SendFlag flags, uint timeout, object tag)
  { this.data=data; this.index=index; this.length=length; this.flags=flags; this.tag=tag;
    if(timeout!=0) this.deadline = Timing.Msecs+timeout;
  }
  public byte[] Data { get { return data; } }
  public object Tag { get { return tag; } }
  public int Index { get { return index; } }
  public int Length { get { return length; } }
  public SendFlag Flags { get { return flags; } }

  internal byte[]   data;
  internal object   tag;
  internal int      index, length, sent;
  internal uint     deadline, lag;
  internal SendFlag flags;
}

public sealed class NetLink
{ public NetLink() { }
  public NetLink(IPEndPoint remote) { Open(remote); }
  public NetLink(Socket tcp) { Open(tcp); }

  public const uint NoTimeout=0;

  // FIXNOW: either make MessageReceived a property instead of an event or iterate through the handlers manually
  public event LinkMessageRecvHandler MessageReceived;
  public event LinkMessageHandler RemoteReceived, MessageSent;
  public event NetLinkHandler     Disconnected;

  public bool Connected        { get { if(tcp!=null) Poll(); return connected; } }
  public int  Available        { get { ReceivePoll(); lock(recv) return recv.Count; } }
  public SendFlag DefaultFlags { get { return defFlags; } set { defFlags=value; } }
  
  public uint LagAverage  { get { return lagAverage;  } set { lagAverage =value; } }
  public uint LagVariance { get { return lagVariance; } set { lagVariance=value; } }

  public IPEndPoint RemoteEndPoint
  { get { return tcp==null ? udp==null ? null : (IPEndPoint)udp.RemoteEndPoint : (IPEndPoint)tcp.RemoteEndPoint; }
  }

  public void Open(IPEndPoint remote)
  { Socket sock = null;
    sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    sock.Connect(remote);
    Open(sock);
  }
  
  public void Open(Socket tcp)
  { if(tcp==null) throw new ArgumentNullException("tcp");
    if(!tcp.Connected) throw new ArgumentException("If TCP is being used, the socket must be connected already!");
    
    try
    { IPEndPoint localTcp = (IPEndPoint)tcp.LocalEndPoint, localUdp;

      Socket udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
      udp.Bind(new IPEndPoint(localTcp.Address, 0));  // bind the udp to a local port on the same interface as the tcp
      localUdp = (IPEndPoint)udp.LocalEndPoint;       // figure out what that is

      byte[] buf = new byte[2];                       // send the udp's endpoint to the other side
      IOH.WriteLE2(buf, 0, (short)localUdp.Port);     // the other side should be doing the same thing
      tcp.Send(buf);                                  // PS. i know it's obsolete, but 'new IPAddress(byte[])' fails

      if(!tcp.Poll(2000000, SelectMode.SelectRead) || tcp.Receive(buf)!=2)
      { tcp.Close();
        udp.Close();
      }

      // connect to it
      udp.Connect(new IPEndPoint(((IPEndPoint)tcp.RemoteEndPoint).Address, IOH.ReadLE2U(buf, 0)));

      tcp.Blocking = false;
      udp.Blocking = false;
      this.tcp = tcp;
      this.udp = udp;
    }
    catch(SocketException) { throw new HandshakeException(); }

    udpMax = 1450;

    low = new Queue(); norm = new Queue(); high = new Queue(); recv = new Queue();
    nextSize  = -1;
    connected = true;
  }

  public void Close()
  { if(tcp!=null)
    { Disconnect();
      tcp.Close();
      tcp=null;
    }
    if(udp!=null)
    { udp.Close();
      udp = null;
    }
    low = norm = high = null;
  }

  public QueueStats GetQueueStats(SendFlag flags)
  { QueueStats stats = new QueueStats();
    if(connected)
    { if((flags&SendFlag.LowPriority)!=0)
        lock(low) foreach(LinkMessage msg in low) { stats.SendMessages++; stats.SendBytes+=msg.Length; }
      if((flags&SendFlag.NormalPriority)!=0)
        lock(norm) foreach(LinkMessage msg in norm) { stats.SendMessages++; stats.SendBytes+=msg.Length; }
      if((flags&SendFlag.HighPriority)!=0)
        lock(high) foreach(LinkMessage msg in high) { stats.SendMessages++; stats.SendBytes+=msg.Length; }
    }
    if((flags&SendFlag.ReceiveQueue)!=0)
      lock(recv) foreach(LinkMessage msg in recv) { stats.ReceiveMessages++; stats.ReceiveBytes+=msg.Length; }
    return stats;
  }

  public void Send(byte[] data) { Send(data, 0, data.Length, defFlags, NoTimeout, null); }
  public void Send(byte[] data, int length) { Send(data, 0, length, defFlags, NoTimeout, null); }
  public void Send(byte[] data, int index, int length) { Send(data, index, length, defFlags, NoTimeout, null); }
  public void Send(byte[] data, int index, int length, SendFlag flags) { Send(data, index, length, flags, NoTimeout, null); }
  public void Send(byte[] data, int index, int length, SendFlag flags, uint timeoutMs) { Send(data, index, length, flags, timeoutMs, null); }
  public void Send(byte[] data, int length, SendFlag flags) { Send(data, 0, length, flags, NoTimeout, null); }
  public void Send(byte[] data, int length, SendFlag flags, uint timeoutMs) { Send(data, 0, length, flags, timeoutMs, null); }
  public void Send(byte[] data, SendFlag flags) { Send(data, 0, data.Length, flags, NoTimeout, null); }
  public void Send(byte[] data, SendFlag flags, uint timeoutMs) { Send(data, 0, data.Length, flags, timeoutMs, null); }
  public void Send(byte[] data, int index, int length, SendFlag flags, uint timeoutMs, object tag)
  { if(udp==null || !connected) throw new InvalidOperationException("Link is not open");
    if(tcp==null && (flags&SendFlag.Reliable)!=0)
      throw new InvalidOperationException("Cannot send reliably unless TCP is being used");
    if((flags&SendFlag.NotifyReceived)!=0)
      throw new NotImplementedException("SendFlag.NotifyReceived is not yet implemented");
    if(!connected) throw new ConnectionLostException();
    if(length>65535) throw new DataTooLargeException(65535);
    if(index<0 || length<0 || index+length>data.Length) throw new ArgumentOutOfRangeException("index or length");
    Queue queue = (flags&SendFlag.HighPriority)!=0 ? high : (flags&SendFlag.LowPriority)!=0 ? low : norm;
    
    byte[] buf = new byte[length+HeaderSize];         // add header (NoCopy is currently unimplemented)
    IOH.WriteLE2(buf, 0, (short)length);              // TODO: implement NoCopy
    buf[2] = (byte)(flags&~(SendFlag)HeadFlag.Mask);
    if((flags&SendFlag.Sequential)!=0)
    { buf[2] |= (byte)((sendSeq&0xFF00)>>2);          // sequence number uses top two bits of sendflags field
      buf[3]  = (byte)sendSeq;
      if(++sendSeq>=SeqMax) sendSeq=0;
    }
    Array.Copy(data, index, buf, HeaderSize, length);

    LinkMessage m = new LinkMessage(buf, 0, buf.Length, flags, timeoutMs, tag);
    if(lagAverage>0 || lagVariance>0)
    { int lag = Global.Rand.Next((int)lagVariance*2)+(int)lagAverage-(int)lagVariance;
      if(lag>0) { m.lag = Timing.Msecs+(uint)lag; m.deadline += m.lag; }
    }

    lock(queue) queue.Enqueue(m);
    sendQueue++;
    SendPoll();
    if(!connected) throw new ConnectionLostException();
  }

  public byte[] Receive()
  { LinkMessage m = ReceiveMessage();
    if(m==null) return null;
    if(m.Length==m.Data.Length) return m.Data;
    byte[] ret = new byte[m.Length];
    Array.Copy(m.Data, m.Index, ret, 0, m.Length);
    return ret;
  }

  public LinkMessage ReceiveMessage()
  { if(recv==null) throw new InvalidOperationException("Link has not been opened");
    ReceivePoll();
    lock(recv) return recv.Count>0 ? (LinkMessage)recv.Dequeue() : null;
  }

  public void ReceivePoll()
  { if(tcp!=null && connected)
      lock(tcp)
        try
        { while(connected)
          { int avail = tcp.Available;
            if(avail==0)
            { if(tcp.Poll(0, SelectMode.SelectRead) && tcp.Available==0) Disconnect();
              break;
            }
            if(nextSize==-1)
            { if(avail>=HeaderSize) // check for a message header
              { SizeBuffer(HeaderSize);
                tcp.Receive(recvBuf, HeaderSize, SocketFlags.None);
                nextSize  = IOH.ReadLE2U(recvBuf, 0); // first two bytes are the length
                nextIndex = 0;
                recvFlags = (SendFlag)(recvBuf[2]&~(byte)HeadFlag.Mask); // next byte is the send/header flags
                nextSeq   = (ushort)((((ushort)recvBuf[2]<<2)&0xFF00) | recvBuf[3]);
                avail -= HeaderSize;
              }
            }
            if(nextSize!=-1 && avail>=nextSize)
            { SizeBuffer(nextSize);
              int read = tcp.Receive(recvBuf, nextIndex, nextSize, SocketFlags.None);
              nextSize -= read; nextIndex += read;
              if(nextSize==0)
              { nextSize = -1;
                if((recvFlags&SendFlag.Sequential)!=0)
                  if(nextSeq>=recvSeq || recvSeq-nextSeq>SeqMax*4/10) recvSeq=nextSeq; // handles integer wraparound in a probabilistic way
                  else continue;

                LinkMessage m = new LinkMessage();
                m.index  = 0;
                m.length = nextIndex;
                m.data   = new byte[m.Length];
                m.flags  = recvFlags;
                Array.Copy(recvBuf, m.Data, m.Length);
                if(MessageReceived==null || MessageReceived(this, m)) lock(recv) recv.Enqueue(m);
              }
            }
            else break;
          }
        }
        catch(SocketException) { Disconnect(); }

    if(udp!=null)
      lock(udp)
        while(udp.Available>0)
        { int avail = udp.Available;
          SizeBuffer(avail);
          int read = udp.Receive(recvBuf, 0, avail, SocketFlags.None);
          LinkMessage m = new LinkMessage();
          m.index  = 0;      
          m.length = read-HeaderSize; // first part is the header (same format as tcp header)
          m.data   = new byte[m.Length];
          m.flags  = (SendFlag)(recvBuf[0]&~(byte)HeadFlag.Mask);
          Array.Copy(recvBuf, HeaderSize, m.Data, 0, m.Length);
          if(MessageReceived==null || MessageReceived(this, m)) lock(recv) recv.Enqueue(m);
        }
  }
  
  public void SendPoll()
  { if(tcp!=null) System.Threading.Monitor.Enter(tcp);
    if(udp!=null) System.Threading.Monitor.Enter(udp);
    try { SendMessages(low, SendMessages(norm, SendMessages(high, true))); }
    finally
    { if(udp!=null) System.Threading.Monitor.Exit(udp);
      if(tcp!=null) System.Threading.Monitor.Exit(tcp);
    }
  }

  public void Poll() { SendPoll(); ReceivePoll(); }

  public static ArrayList WaitForEvent(ICollection links, uint timeoutMs)
  { Hashtable hash = new Hashtable(), did = new Hashtable();
    ArrayList read=new ArrayList(links.Count*2), write=new ArrayList(links.Count*2), ret = new ArrayList();
    foreach(NetLink link in links)
    { bool send = link.sendQueue>0;
      if(link.tcp!=null) { hash[link.tcp]=link; read.Add(link.tcp); if(send) write.Add(link.tcp); }
      if(link.udp!=null) { hash[link.udp]=link; read.Add(link.udp); if(send) write.Add(link.udp); }
    }
    if(read.Count==0) { Thread.Sleep((int)timeoutMs); return ret; }

    uint thresh = timeoutMs+Timing.Msecs;
    do
    { ArrayList rl = (ArrayList)read.Clone(), wl = write.Count>0 ? (ArrayList)write.Clone() : null;
      Socket.Select(rl, wl, null, (int)timeoutMs*1000);
      if(rl.Count>0)
        foreach(Socket sock in rl)
        { NetLink link = (NetLink)hash[sock];
          link.ReceivePoll();
          if(!link.Connected || link.Available>0) ret.Add(link);
        }
      if(wl!=null && wl.Count>0)
        foreach(Socket sock in wl)
        { NetLink link = (NetLink)hash[sock];
          if(!did.Contains(link))
          { link.SendPoll();
            did[link]=true;
          }
        }
      if(ret.Count>0) return ret;
      did.Clear();
      timeoutMs = thresh-Timing.Msecs;
    } while(timeoutMs<thresh); // works because timeoutMs is unsigned and becomes >thresh when it becomes "negative"
    return null;
  }

  [Flags]
  enum HeadFlag : byte { Ack=32, Mask=0xE0 }
  const int HeaderSize=4, SeqMax=1024;
  
  void SizeBuffer(int len)
  { if(len<256) len=256;
    if(recvBuf==null || recvBuf.Length<len) recvBuf=new byte[len];
  }

  bool SendMessages(Queue queue, bool trySend)
  { if(!connected) return false;
    
    lock(queue)
    { while(queue.Count>0)
      { LinkMessage msg = (LinkMessage)queue.Peek();
        if(Timing.Msecs<msg.lag) return true;
        if(msg.deadline!=0 && Timing.Msecs>msg.deadline) goto Remove;
        if(!trySend) return false;

        bool useTcp;
        int  sent;

        if((msg.Flags&SendFlag.Reliable)!=0 && msg.Length<=udpMax && udp!=null) useTcp=false;
        else if(tcp==null || !connected) goto Remove;
        else useTcp=true;
        
        Retry:
        try
        { sent = (useTcp?tcp:udp).Send(msg.Data, msg.Index+msg.sent, msg.Length-msg.sent, SocketFlags.None);
        }
        catch(SocketException e)
        { if(e.ErrorCode==Config.EMSGSIZE)
          { if(!useTcp)
            { if(msg.Length<udpMax) udpMax=msg.Length;
              useTcp=true;
              goto Retry;
            }
            else goto Remove;
          }
          else if(e.ErrorCode==Config.EWOULDBLOCK) return false;
          else
          { if(useTcp) Disconnect();
            return false;
          }
        }
        
        msg.sent += sent;
        if(msg.sent!=msg.Length && useTcp) return false;
        if(!useTcp && sent>udpMax) udpMax=sent;
        if((msg.Flags&SendFlag.NotifySent)!=0 && MessageSent!=null) MessageSent(this, msg);

        Remove:
        queue.Dequeue();
        sendQueue--;
      }
      return true;
    }
  }
  
  void Disconnect()
  { connected = false;
    tcp.Shutdown(SocketShutdown.Send);
    if(Disconnected!=null) Disconnected(this);
  }

  SendFlag   defFlags;
  Socket     tcp, udp;
  Queue      low, norm, high, recv;
  byte[]     recvBuf;
  int        nextSize, nextIndex, udpMax;
  uint       lagAverage, lagVariance, sendQueue;
  ushort     sendSeq, recvSeq, nextSeq;
  SendFlag   recvFlags;
  bool       connected;
}
#endregion

#region Server class and supporting types
public class ServerPlayer
{ internal ServerPlayer(NetLink link, uint id) { Link=link; ID=id; }
  
  public object     Data;
  public IPEndPoint EndPoint { get { return Link.RemoteEndPoint; } }

  internal uint     ID, DropTime;
  internal NetLink  Link;
  internal bool     DelayedDrop;
}

public delegate bool PlayerConnectHandler(Server server, ServerPlayer player);
public delegate void PlayerDisconnectHandler(Server server, ServerPlayer player);
public delegate void ServerReceivedHandler(Server sender, ServerPlayer player, object msg);
public delegate void ServerSentHandler(Server sender, ServerPlayer player, object msg);

public class Server
{ public class PlayerCollection : ReadOnlyCollectionBase
  { public ServerPlayer this[int index] { get { return (ServerPlayer)InnerList[index]; } }
    internal ArrayList Array { get { return InnerList; } }
  }

  public Server() { }
  public Server(IPEndPoint local) { Listen(local); }
  ~Server() { Close(); }

  public event PlayerConnectHandler    PlayerConnecting, PlayerConnected;
  public event PlayerDisconnectHandler PlayerDisconnected;
  public event ServerReceivedHandler   MessageReceived, RemoteReceived;
  public event ServerSentHandler       MessageSent
  { add    { lock(this) eMessageSent += value; }
    remove { lock(this) eMessageSent -= value; }
  }

  public SendFlag DefaultFlags { get { return defFlags; } set { defFlags=value; } }
  public IPEndPoint LocalEndPoint { get { return listening ? (IPEndPoint)server.LocalEndpoint : null; } }
  public PlayerCollection Players { get { return players; } }

  public uint LagAverage
  { get { return lagAverage; }
    set
    { lock(this) foreach(ServerPlayer p in players) p.Link.LagAverage=value;
      lagAverage=value;
    }
  }
  public uint LagVariance
  { get { return lagVariance; }
    set
    { lock(this) foreach(ServerPlayer p in players) p.Link.LagVariance=value;
      lagVariance=value;
    }
  }

  public void RegisterTypes(Type[] types)   { AssertClosed(); cvt.RegisterTypes(types); }
  public void RegisterType(Type type)       { AssertClosed(); cvt.RegisterType(type); }
  public void UnregisterTypes(Type[] types) { AssertClosed(); cvt.UnregisterTypes(types); }
  public void UnregisterType(Type type)     { AssertClosed(); cvt.UnregisterType(type); }
  public void ClearTypes() { AssertClosed(); cvt.Clear(); }

  public void Open()
  { Close();
    players = new PlayerCollection();
    links   = new ArrayList();
    nextID  = 1;
    quit    = listening = false;
    server  = new TcpListener(IPAddress.Any, 30000); // dummy port
    thread  = new Thread(new ThreadStart(ThreadFunc));
    thread.Start();
  }

  public void Close()
  { if(thread!=null)
    { lock(this) foreach(ServerPlayer p in players) DropPlayer(p);
      quit = true;
      thread.Join(1500);
      thread.Abort();
      StopListening();
      thread = null;
    }
    players = null;
    links   = null;
  }

  public void Listen(int port) { Listen(new IPEndPoint(IPAddress.Any, port)); }
  public void Listen(IPEndPoint local)
  { if(thread==null) Open();
    lock(this)
    { StopListening();
      server = new TcpListener(local);
      server.Start();
      listening = true;
    }
  }

  public void StopListening()
  { listening = false;
    server.Stop();
  }
  
  public QueueStats GetQueueStats(ServerPlayer p, SendFlag flags) { lock(this) return p.Link.GetQueueStats(flags); }
  public QueueStats GetQueueStats(SendFlag flags)
  { QueueStats qs = new QueueStats(), pqs;
    lock(this)
      foreach(NetLink link in links)
      { pqs = link.GetQueueStats(flags);
        qs.SendBytes       += pqs.SendBytes;
        qs.SendMessages    += pqs.SendMessages;
        qs.ReceiveBytes    += pqs.ReceiveBytes;
        qs.ReceiveMessages += pqs.ReceiveMessages;
      }
    return qs;
  }

  public void DropPlayer(ServerPlayer p)
  { p.DropTime    = Timing.Msecs;
    p.DelayedDrop = true;
  }
  public void DropPlayerDelayed(ServerPlayer p) { DropPlayerDelayed(p, 0); }
  public void DropPlayerDelayed(ServerPlayer p, uint timeoutMs)
  { p.DropTime    = timeoutMs==0 ? 0 : Timing.Msecs+timeoutMs;
    p.DelayedDrop = true;
  }

  public void Send(object toWho, byte[] data) { Send(toWho, data, 0, data.Length, defFlags); }
  public void Send(object toWho, byte[] data, int length) { Send(toWho, data, 0, length, defFlags); }
  public void Send(object toWho, byte[] data, int index, int length)
  { DoSend(toWho, cvt.FromObject(data, index, length), defFlags, 0, data);
  }
  public void Send(object toWho, object data) { DoSend(toWho, cvt.FromObject(data), defFlags, 0, data); }

  public void Send(object toWho, byte[] data, SendFlag flags) { Send(toWho, data, 0, data.Length, flags); }
  public void Send(object toWho, byte[] data, int length, SendFlag flags) { Send(toWho, data, 0, length, flags); }
  public void Send(object toWho, byte[] data, int index, int length, SendFlag flags)
  { DoSend(toWho, cvt.FromObject(data, index, length), flags, 0, data);
  }
  public void Send(object toWho, object data, SendFlag flags) { DoSend(toWho, cvt.FromObject(data), flags, 0, data); }

  public void Send(object toWho, byte[] data, SendFlag flags, uint timeoutMs) { Send(toWho, data, 0, data.Length, flags); }
  public void Send(object toWho, byte[] data, int length, SendFlag flags, uint timeoutMs) { Send(toWho, data, 0, length, flags); }
  public void Send(object toWho, byte[] data, int index, int length, SendFlag flags, uint timeoutMs)
  { DoSend(toWho, cvt.FromObject(data, index, length), flags, timeoutMs, data);
  }
  public void Send(object toWho, object data, SendFlag flags, uint timeoutMs)
  { DoSend(toWho, cvt.FromObject(data), flags, timeoutMs, data);
  }

  struct DualTag
  { public DualTag(object tag1, object tag2) { Tag1=tag1; Tag2=tag2; }
    public object Tag1, Tag2;
  }

  void DoSend(object toWho, byte[] data, SendFlag flags, uint timeoutMs, object orig)
  { if(toWho==null || toWho==Players)
      lock(this)
        foreach(ServerPlayer p in players)
          p.Link.Send(data, 0, data.Length, flags, timeoutMs, new DualTag(p, orig));
    else if(toWho is ICollection)
      foreach(ServerPlayer p in (ICollection)toWho)
        p.Link.Send(data, 0, data.Length, flags, timeoutMs, new DualTag(p, orig));
    else
    { ServerPlayer p = (ServerPlayer)toWho;
      p.Link.Send(data, 0, data.Length, flags, timeoutMs, new DualTag(p, orig));
    }
  }

  void ThreadFunc()
  { while(!quit)
    { bool did=false;
      while(listening && server.Pending())
      { Socket sock = server.AcceptSocket();
        try
        { ServerPlayer p = new ServerPlayer(new NetLink(sock), nextID++);
          if(!quit && (PlayerConnecting==null || PlayerConnecting(this, p)))
          { p.Link.LagAverage      = lagAverage;
            p.Link.LagVariance     = lagVariance;
            p.Link.MessageSent    += new LinkMessageHandler(OnMessageSent);
            p.Link.RemoteReceived += new LinkMessageHandler(OnRemoteReceived);
            lock(this)
            { players.Array.Add(p);
              links.Add(p.Link);
            }
            if(PlayerConnected!=null) PlayerConnected(this, p);
          }
          else sock.Close();
        }
        catch(SocketException) { sock.Close(); }
        catch(HandshakeException) { sock.Close(); }
      }

      lock(this)
      { ArrayList disconnected=null;
        for(int i=0; i<players.Count; i++)
        { ServerPlayer p = players[i];
          try
          { if(p.DelayedDrop && ((p.DropTime!=0 && Timing.Msecs>=p.DropTime) ||
                                p.Link.GetQueueStats(SendFlag.SendStats).SendMessages==0))
              p.Link.Close();

            p.Link.Poll();

            LinkMessage msg = p.Link.ReceiveMessage();
            if(msg!=null)
            { did=true;
              if(MessageReceived!=null) MessageReceived(this, p, cvt.ToObject(msg));
            }
            else if(!p.Link.Connected)
            { did=true;
              players.Array.RemoveAt(i--);
              links.Remove(p.Link);
              if(PlayerDisconnected!=null)
              { if(disconnected==null) disconnected=new ArrayList();
                disconnected.Add(p);
              }
            }
          }
          catch(SocketException) { }
        }
        if(disconnected!=null) foreach(ServerPlayer p in disconnected) PlayerDisconnected(this, p);
      }

      if(!did) NetLink.WaitForEvent(links, 250);
    }
  }

  void OnMessageSent(NetLink link, LinkMessage msg)
  { if(eMessageSent!=null)
    { DualTag tag = (DualTag)msg.Tag;
      eMessageSent(this, (ServerPlayer)tag.Tag1, tag.Tag2);
    }
  }

  void OnRemoteReceived(NetLink link, LinkMessage msg)
  { if(RemoteReceived==null)
    { DualTag tag = (DualTag)msg.Tag;
      RemoteReceived(this, (ServerPlayer)tag.Tag1, tag.Tag2);
    }
  }

  void CheckOpen()    { if(thread==null) throw new InvalidOperationException("Server not open yet"); }
  void AssertClosed() { if(thread!=null) throw new InvalidOperationException("Server already open"); }

  PlayerCollection  players;
  ArrayList         links;
  MessageConverter  cvt = new MessageConverter();
  TcpListener       server;
  Thread            thread;
  ServerSentHandler eMessageSent;
  SendFlag          defFlags;
  uint              nextID, lagAverage, lagVariance;
  bool              quit, listening;
}
#endregion

#region Client class and associated types
public delegate void ClientReceivedHandler(Client client, object msg);
public delegate void ClientSentHandler(Client client, object msg);
public delegate void DisconnectHandler(object sender);

public class Client
{ public Client() { }
  public Client(IPEndPoint remote) { Connect(remote); }
  ~Client() { Disconnect(); }

  public event DisconnectHandler       Disconnected;
  public event ClientReceivedHandler   MessageReceived, RemoteReceived;
  public event ClientSentHandler       MessageSent
  { add    { lock(this) eMessageSent += value; }
    remove { lock(this) eMessageSent -= value; }
  }

  public bool Connected { get { return link==null ? false : link.Connected; } }

  public SendFlag DefaultFlags { get { return defFlags; } set { defFlags=value; } }

  public uint LagAverage { get { AssertLink(); return link.LagAverage; } set { AssertLink(); link.LagAverage=value; } }
  public uint LagVariance { get { AssertLink(); return link.LagVariance; } set { AssertLink(); link.LagVariance=value; } }

  public IPEndPoint RemoteEndPoint { get { AssertLink(); return link.RemoteEndPoint; } }

  public void Connect(IPEndPoint remote)
  { Disconnect();
    quit = delayedDrop = false;
    link = new NetLink(remote);
    link.MessageSent    += new LinkMessageHandler(OnMessageSent);
    link.RemoteReceived += new LinkMessageHandler(OnRemoteReceived);
    thread = new Thread(new ThreadStart(ThreadFunc));
    thread.Start();
  }
  public void DelayedDisconnect() { DelayedDisconnect(0); }
  public void DelayedDisconnect(uint timeoutMs)
  { dropTime    = timeoutMs==0 ? 0 : Timing.Msecs+timeoutMs;
    delayedDrop = true;
  }
  public void Disconnect()
  { if(thread!=null)
    { if(Disconnected!=null) Disconnected(this);
      quit = true;
      thread.Join(1500);
      thread.Abort();
      thread = null;
    }
    link = null;
  }

  public void RegisterTypes(Type[] types)   { AssertClosed(); cvt.RegisterTypes(types); }
  public void RegisterType(Type type)       { AssertClosed(); cvt.RegisterType(type); }
  public void UnregisterTypes(Type[] types) { AssertClosed(); cvt.UnregisterTypes(types); }
  public void UnregisterType(Type type)     { AssertClosed(); cvt.UnregisterType(type); }
  public void ClearTypes() { AssertClosed(); cvt.Clear(); }

  public QueueStats GetQueueStats(SendFlag flags) { AssertLink(); return link.GetQueueStats(flags); }

  public void Send(byte[] data) { Send(data, 0, data.Length, defFlags); }
  public void Send(byte[] data, int length) { Send(data, 0, length, defFlags); }
  public void Send(byte[] data, int index, int length)
  { AssertLink();
    DoSend(cvt.FromObject(data, index, length), defFlags, 0, data);
  }
  public void Send(object data)
  { AssertLink();
    DoSend(cvt.FromObject(data), defFlags, 0, data);
  }

  public void Send(byte[] data, SendFlag flags) { Send(data, 0, data.Length, flags); }
  public void Send(byte[] data, int length, SendFlag flags) { Send(data, 0, length, flags); }
  public void Send(byte[] data, int index, int length, SendFlag flags)
  { AssertLink();
    DoSend(cvt.FromObject(data, index, length), flags, 0, data);
  }
  public void Send(object data, SendFlag flags)
  { AssertLink();
    DoSend(cvt.FromObject(data), flags, 0, data);
  }

  public void Send(byte[] data, SendFlag flags, uint timeoutMs) { Send(data, 0, data.Length, flags); }
  public void Send(byte[] data, int length, SendFlag flags, uint timeoutMs) { Send(data, 0, length, flags); }
  public void Send(byte[] data, int index, int length, SendFlag flags, uint timeoutMs)
  { AssertLink();
    DoSend(cvt.FromObject(data, index, length), flags, timeoutMs, data);
  }
  public void Send(object data, SendFlag flags, uint timeoutMs)
  { AssertLink();
    DoSend(cvt.FromObject(data), flags, timeoutMs, data);
  }

  void DoSend(byte[] data, SendFlag flags, uint timeoutMs, object orig)
  { link.Send(data, 0, data.Length, flags, timeoutMs, orig);
  }
  void AssertLink()   { if(link==null) throw new InvalidOperationException("Client is not connected"); }
  void AssertClosed() { if(link!=null) throw new InvalidOperationException("Client is already connected"); }

  void ThreadFunc()
  { NetLink[] links = new NetLink[] { link };
    while(!quit)
    { bool did=false;
      try
      { if(delayedDrop && ((dropTime!=0 && Timing.Msecs>=dropTime) ||
                            link.GetQueueStats(SendFlag.SendStats).SendMessages==0))
          link.Close();

        link.Poll();

        LinkMessage msg = link.ReceiveMessage();
        if(msg!=null)
        { did=true;
          if(MessageReceived!=null) MessageReceived(this, cvt.ToObject(msg));
        }
        else if(!link.Connected)
        { if(Disconnected!=null) Disconnected(this);
          link = null;
          return;
        }
      }
      catch(SocketException) { }

      if(!did) NetLink.WaitForEvent(links, 250);
    }
  }

  void OnMessageSent(NetLink link, LinkMessage msg) { lock(this) if(eMessageSent!=null) eMessageSent(this, msg.Tag); }
  void OnRemoteReceived(NetLink link, LinkMessage msg) { if(RemoteReceived==null) RemoteReceived(this, msg.Tag); }
  
  MessageConverter  cvt = new MessageConverter();
  ClientSentHandler eMessageSent;
  NetLink  link;
  Thread   thread;
  uint     dropTime;
  SendFlag defFlags;
  bool     quit, delayedDrop;
}
#endregion

} // namespace GameLib.Network