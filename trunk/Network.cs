using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using GameLib.IO;

namespace GameLib.Network
{

#region Common types
public struct QueueStats
{ public int SendMessages, SendBytes, ReceiveMessages, ReceiveBytes;
}

[Flags]
public enum SendFlag
{ None=0, Reliable=1, Sequential=2, NotifySent=4, NotifyReceived=8, NoCopy=16,
  LowPriority=32, HighPriority=64,
  
  // these are not for Send(). they're used for GetQueueStats().
  NormalPriority=128, ReceiveQueue=256, AllStats=LowPriority|NormalPriority|HighPriority|ReceiveQueue
}

internal class MessageConverter
{ public int NumTypes { get { return typeIDs.Count; } }

  public void Clear()
  { types.Clear();
    typeIDs.Clear();
  }

  public void RegisterType(Type type)
  { int i;
    for(i=0; i<types.Count; i++) if(types[i]==null) break;
    if(i==types.Count) types.Add(type); else types[i] = type;
    typeIDs[type] = i;
  }

  public void UnregisterType(Type type)
  { int index = (int)typeIDs[type];
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
        unsafe
        { fixed(byte* buf=msg.Data)
            return Marshal.PtrToStructure(new IntPtr(buf+msg.Index+4), (Type)types[id-1]);
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
      int id = (int)typeIDs[type], length = Marshal.SizeOf(type);
      byte[] ret = new byte[length+4];
      IOH.WriteLE4(ret, 0, id+1);
      unsafe { fixed(byte* ptr = ret) Marshal.StructureToPtr(obj, new IntPtr(ptr+4), false); }
      return ret;
    }
  }

  ArrayList types   = new ArrayList();
  Hashtable typeIDs = new Hashtable();
}

internal struct DualTag
{ public DualTag(object tag1, object tag2) { Tag1=tag1; Tag2=tag2; }
  public object Tag1, Tag2;
}
#endregion

#region NetLink class and supporting types
[Flags]
internal enum LinkType
{ Udp=1, Tcp=2, Both=Udp|Tcp, TcpNoDelay=4
}

internal delegate void NetLinkHandler(NetLink link);
internal delegate bool LinkMessageHandler(NetLink link, LinkMessage msg);

internal class LinkMessage
{ public LinkMessage() { }
  public LinkMessage(byte[] data, int index, int length, SendFlag flags, uint timeout, object tag)
  { Data=data; Index=index; Length=length; Flags=flags; Tag=tag;
    if(timeout!=0) Deadline = Timing.Ticks+timeout;
  }
  public int      Index, Length, Sent;
  public uint     Deadline;
  public byte[]   Data;
  public object   Tag;
  public SendFlag Flags;
}

internal class NetLink
{ public NetLink() { }
  public NetLink(IPEndPoint ep, LinkType type) { Open(ep, type); }
  public NetLink(Socket tcp, IPEndPoint ep, LinkType type) { Open(tcp, ep, type); }

  public const uint NoTimeout=0;

  public event LinkMessageHandler MessageReceived, RemoteReceived, MessageSent;
  public event NetLinkHandler     Disconnected;

  public bool Connected        { get { if(tcp!=null) Poll(); return connected; } }
  public int  Available        { get { ReceivePoll(); lock(recv) return recv.Count; } }
  public SendFlag DefaultFlags { get { return defFlags; } set { defFlags=value; } }

  public IPEndPoint RemoteEndPoint
  { get { return udp==null ? tcp==null ? null : (IPEndPoint)tcp.RemoteEndPoint : (IPEndPoint)udp.RemoteEndPoint; }
  }

  public void Open(IPEndPoint ep, LinkType type)
  { Socket sock = null;
    if((type&LinkType.Tcp)!=0)
    { sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      sock.Connect(ep);
    }
    Open(sock, ep, type);
  }
  
  public void Open(Socket tcp, IPEndPoint ep, LinkType type)
  { if(tcp!=null)
    { if(!tcp.Connected) throw new ArgumentException("If TCP is being used, the socket must be connected already!");
      tcp.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, (type&LinkType.TcpNoDelay)==0 ? 0 : 1);
      tcp.Blocking = false;
      this.tcp = tcp;
    }
    if((type&LinkType.Udp)!=0)
    { udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
      udp.Connect(ep);
      udp.Blocking = false;
      udpMax = 1432;
    }
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
  { if(tcp==null || !connected)
    { if(udp==null) throw new InvalidOperationException("Link has not been opened");
      if((flags&SendFlag.Reliable)!=0)
        throw new InvalidOperationException("Cannot send reliably unless TCP is being used");
      if((flags&SendFlag.Sequential)!=0)
        throw new NotImplementedException("Cannot send sequentially unless TCP is being used... yet");
    }
    if((flags&SendFlag.NotifyReceived)!=0)
      throw new NotImplementedException("SendFlag.NotifyReceived is not yet implemented");
    if(!connected) throw new ConnectionLostException();
    if(length>65535) throw new DataTooLargeException(65535);
    if(index<0 || length<0 || index+length>data.Length) throw new ArgumentOutOfRangeException("index or length");
    Queue queue = (flags&SendFlag.HighPriority)!=0 ? high : (flags&SendFlag.LowPriority)!=0 ? low : norm;
    
    byte[] buf = new byte[length+4]; // add header (NoCopy is currently unimplemented)
    IOH.WriteLE2(buf, 0, (short)length);
    buf[2] = (byte)(flags&~(SendFlag)HeadFlag.Mask);
    Array.Copy(data, index, buf, 4, length);

    LinkMessage m = new LinkMessage(buf, 0, buf.Length, flags, timeoutMs, tag);
    lock(queue) queue.Enqueue(m);
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
  { if(tcp!=null)
      lock(tcp)
        try
        { while(true)
          { int avail = tcp.Available;
            if(avail==0) break;
            if(nextSize==-1)
            { if(avail>=4) // check for a message header
              { SizeBuffer(4);
                tcp.Receive(recvBuf, 4, SocketFlags.None);
                nextSize  = IOH.ReadLE2U(recvBuf, 0); // first two bytes are the length
                nextIndex = 0;
                recvFlags = (SendFlag)(recvBuf[2]&~(byte)HeadFlag.Mask); // next byte is the send/header flags
                                                      // final byte is reserved
                avail -= 4;
              }
            }
            if(nextSize!=-1 && avail>=nextSize)
            { SizeBuffer(nextSize);
              int read = tcp.Receive(recvBuf, nextIndex, nextSize, SocketFlags.None);
              nextSize -= read; nextIndex += read;
              if(nextSize==0)
              { LinkMessage m = new LinkMessage();
                m.Index  = 0;
                m.Length = nextIndex;
                m.Data   = new byte[m.Length];
                m.Flags  = recvFlags;
                Array.Copy(recvBuf, m.Data, m.Length);
                nextSize = -1;
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
          m.Index  = 0;      
          m.Length = read-4; // first four bytes are the header (same format as tcp header)
          m.Data   = new byte[m.Length];
          m.Flags  = (SendFlag)(recvBuf[0]&~(byte)HeadFlag.Mask);
          Array.Copy(recvBuf, 4, m.Data, 0, m.Length);
          if(MessageReceived==null || MessageReceived(this, m)) lock(recv) recv.Enqueue(m);
        }
  }
  
  public void SendPoll()
  { if(tcp!=null) System.Threading.Monitor.Enter(tcp);
    if(udp!=null) System.Threading.Monitor.Enter(udp);
    try
    { if(!SendMessages(high)) return;
      if(!SendMessages(norm)) return;
      if(!SendMessages(low))  return;
    }
    finally
    { if(udp!=null) System.Threading.Monitor.Exit(udp);
      if(tcp!=null) System.Threading.Monitor.Exit(tcp);
    }
  }

  public void Poll() { SendPoll(); ReceivePoll(); }

  public static ArrayList WaitForEvent(ICollection links, uint timeoutMs)
  { Hashtable hash = new Hashtable(), did = new Hashtable();
    ArrayList socks = new ArrayList(links.Count*2), read, write, ret = new ArrayList();
    foreach(NetLink link in links)
    { if(link.tcp!=null) { hash[link.tcp]=link; socks.Add(link.tcp); }
      if(link.udp!=null) { hash[link.udp]=link; socks.Add(link.udp); }
    }
    uint thresh = timeoutMs+Timing.Ticks;
    do
    { read = (ArrayList)socks.Clone(); write = (ArrayList)socks.Clone();
      Socket.Select(read, write, null, (int)timeoutMs*1000);
      if(read.Count>0)
        foreach(Socket sock in read)
        { NetLink link = (NetLink)hash[sock];
          link.ReceivePoll();
          if(!link.Connected || link.Available>0) ret.Add(link);
        }
      if(write.Count>0)
        foreach(Socket sock in write)
        { NetLink link = (NetLink)hash[sock];
          if(!did.Contains(link))
          { link.SendPoll();
            did[link]=true;
          }
        }
      if(ret.Count>0) return ret;
      did.Clear();
      timeoutMs = thresh-Timing.Ticks;
    } while(timeoutMs<thresh); // works because timeoutMs is unsigned and becomes >thresh when it becomes "negative"
    return null;
  }
  
  [Flags]
  enum HeadFlag : byte
  { Ack=32, Mask=0xE0
  }
  
  void SizeBuffer(int len)
  { if(len<256) len=256;
    if(recvBuf==null || recvBuf.Length<len) recvBuf=new byte[len];
  }

  bool SendMessages(Queue queue)
  { if(!connected) return false;
    
    lock(queue)
    { while(queue.Count>0)
      { LinkMessage msg = (LinkMessage)queue.Peek();
        if(msg.Deadline!=0 && Timing.Ticks>msg.Deadline) goto Remove;

        bool useTcp;
        int  sent;

        if(((msg.Flags & (SendFlag.Reliable|SendFlag.Sequential))==0 || msg.Length>udpMax) && udp!=null) useTcp=false;
        else if(tcp==null || !connected) goto Remove;
        else useTcp=true;
        
        Retry:
        try
        { sent = (useTcp?tcp:udp).Send(msg.Data, msg.Index+msg.Sent, msg.Length-msg.Sent, SocketFlags.None);
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
        
        msg.Sent += sent;
        if(msg.Sent!=msg.Length && useTcp) return false;
        if(!useTcp && sent>udpMax) udpMax=sent;
        if((msg.Flags&SendFlag.NotifySent)!=0 && MessageSent!=null) MessageSent(this, msg);

        Remove:
        queue.Dequeue();
      }
      return true;
    }
  }
  
  void Disconnect()
  { connected = false;
    tcp.Shutdown(SocketShutdown.Send);
    if(Disconnected!=null) Disconnected(this);
  }

  SendFlag   defFlags = SendFlag.None;
  Socket     tcp, udp;
  byte[]     recvBuf;
  int        nextSize, nextIndex, udpMax;
  SendFlag   recvFlags;
  bool       connected;
  Queue      low, norm, high, recv;
}
#endregion

#region Server class and supporting types
public class Player
{ internal Player(IPEndPoint ep, NetLink link, uint id) { EndPoint=ep; Link=link; ID=id; }
  public object     Data;
  public IPEndPoint EndPoint;
  internal uint     ID;
  internal NetLink  Link;
}

public delegate bool PlayerConnectHandler(Server server, Player player);
public delegate void PlayerDisconnectHandler(Server server, Player player);
public delegate void ServerReceivedHandler(Server sender, Player player, object msg);
public delegate void ServerSentHandler(Server sender, Player player, object msg);

public class Server
{ public class PlayerCollection : ReadOnlyCollectionBase
  { public Player this[int index] { get { return (Player)InnerList[index]; } }
    internal ArrayList Array { get { return InnerList; } }
  }

  public Server() { }
  public Server(IPEndPoint ep) { Listen(ep); }

  public event PlayerConnectHandler    PlayerConnecting;
  public event PlayerDisconnectHandler PlayerDisconnected;
  public event ServerReceivedHandler   MessageReceived, RemoteReceived;
  public event ServerSentHandler       MessageSent
  { add    { lock(this) eMessageSent += value; }
    remove { lock(this) eMessageSent -= value; }
  }

  public IPEndPoint LocalEndPoint { get { return server==null ? null : (IPEndPoint)server.LocalEndpoint; } }
  public PlayerCollection Players { get { return players; } }

  public void RegisterType(Type type) { CheckClosed(); cvt.RegisterType(type); }
  public void UnregisterType(Type type) { CheckClosed(); cvt.UnregisterType(type); }
  public void ClearTypes() { CheckClosed(); cvt.Clear(); }

  public void Open()
  { Close();
    players = new PlayerCollection();
    links   = new ArrayList();
    nextID  = 1;
    quit    = false;
    thread  = new Thread(new ThreadStart(ThreadFunc));
  }

  public void Close()
  { if(thread!=null)
    { quit = true;
      thread.Join(1500);
      thread.Abort();
      StopListening();
      thread = null;
    }
    players = null;
    links   = null;
  }

  public void Listen(IPEndPoint ep)
  { if(thread==null) Open();
    lock(this)
    { StopListening();
      server = new TcpListener(ep);
      server.Start();
    }
  }

  public void StopListening()
  { lock(this)
      if(server!=null)
      { server.Stop();
        server = null;
      }
  }

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

  void DoSend(object toWho, byte[] data, SendFlag flags, uint timeoutMs, object orig)
  { if(toWho is ICollection)
      foreach(Player p in (ICollection)toWho)
        p.Link.Send(data, 0, data.Length, flags, timeoutMs, new DualTag(p, orig));
    else
    { Player p = (Player)toWho;
      p.Link.Send(data, 0, data.Length, flags, timeoutMs, new DualTag(p, orig));
    }
  }

  void ThreadFunc()
  { while(!quit)
    { bool did=false;
      lock(this)
        while(server!=null && server.Pending())
        { Socket sock = server.AcceptSocket();
          try
          { Player p = new Player((IPEndPoint)sock.RemoteEndPoint,
                                  new NetLink(sock, (IPEndPoint)sock.RemoteEndPoint, LinkType.Both), nextID++);
            if(!quit && (PlayerConnecting==null || PlayerConnecting(this, p)))
            { players.Array.Add(p);
              links.Add(p.Link);
              p.Link.MessageSent    += new LinkMessageHandler(OnMessageSent);
              p.Link.RemoteReceived += new LinkMessageHandler(OnRemoteReceived);
            }
            else sock.Close();
          }
          catch { sock.Close(); }
        }

      for(int i=0; i<players.Count; i++)
      { Player p = players[i];
        lock(p.Link)
          try
          { p.Link.Poll();

            LinkMessage msg = p.Link.ReceiveMessage();
            if(msg!=null)
            { did=true;
              if(MessageReceived!=null) MessageReceived(this, p, cvt.ToObject(msg));
            }
            else if(!p.Link.Connected)
            { did=true;
              players.Array.RemoveAt(i--);
              links.Remove(p.Link);
              if(!quit && PlayerDisconnected!=null) PlayerDisconnected(this, p);
            }
          }
          catch { }
      }

      if(!did) NetLink.WaitForEvent(links, 1000);
    }
  }

  bool OnMessageSent(NetLink link, LinkMessage msg)
  { lock(this)
    { if(eMessageSent!=null)
      { DualTag tag = (DualTag)msg.Tag;
        eMessageSent(this, (Player)tag.Tag1, tag.Tag2);
      }
      return true;
    }
  }

  bool OnRemoteReceived(NetLink link, LinkMessage msg)
  { if(RemoteReceived==null)
    { DualTag tag = (DualTag)msg.Tag;
      RemoteReceived(this, (Player)tag.Tag1, tag.Tag2);
    }
    return true;
  }

  void CheckOpen()   { if(thread==null) throw new InvalidOperationException("Server not open yet"); }
  void CheckClosed() { if(thread!=null) throw new InvalidOperationException("Server already open"); }

  PlayerCollection  players;
  ArrayList         links;
  MessageConverter  cvt = new MessageConverter();
  TcpListener       server;
  Thread            thread;
  ServerSentHandler eMessageSent;
  uint              nextID;
  bool              quit;
}
#endregion

#region Client class and associated types
public delegate void ClientReceivedHandler(Client client, object msg);
public delegate void ClientSentHandler(Client client, object msg);
public delegate void DisconnectHandler(object sender);

public class Client
{ public Client() { }
  public Client(IPEndPoint ep) { Connect(ep); }
  
  public event DisconnectHandler       Disconnected;
  public event ClientReceivedHandler   MessageReceived, RemoteReceived;
  public event ClientSentHandler       MessageSent
  { add    { lock(this) eMessageSent += value; }
    remove { lock(this) eMessageSent -= value; }
  }

  public IPEndPoint RemoteEndPoint { get { CheckLink(); return link.RemoteEndPoint; } }
  public bool       Connected      { get { return link==null ? false : link.Connected; } }

  public void Connect(IPEndPoint ep)
  { Disconnect();
    quit   = false;
    link   = new NetLink(ep, LinkType.Both);
    link.MessageSent    += new LinkMessageHandler(OnMessageSent);
    link.RemoteReceived += new LinkMessageHandler(OnRemoteReceived);
    thread = new Thread(new ThreadStart(ThreadFunc));
  }
  public void Disconnect()
  { if(thread!=null)
    { quit = true;
      thread.Join(1500);
      thread.Abort();
    }
    link = null;
  }

  public void RegisterType(Type type) { CheckClosed(); cvt.RegisterType(type); }
  public void UnregisterType(Type type) { CheckClosed(); cvt.UnregisterType(type); }
  public void ClearTypes() { CheckClosed(); cvt.Clear(); }

  public void Send(byte[] data, SendFlag flags) { Send(data, 0, data.Length, flags); }
  public void Send(byte[] data, int length, SendFlag flags) { Send(data, 0, length, flags); }
  public void Send(byte[] data, int index, int length, SendFlag flags)
  { CheckLink();
    DoSend(cvt.FromObject(data, index, length), flags, 0, data);
  }
  public void Send(object data, SendFlag flags)
  { CheckLink();
    DoSend(cvt.FromObject(data), flags, 0, data);
  }

  public void Send(byte[] data, SendFlag flags, uint timeoutMs) { Send(data, 0, data.Length, flags); }
  public void Send(byte[] data, int length, SendFlag flags, uint timeoutMs) { Send(data, 0, length, flags); }
  public void Send(byte[] data, int index, int length, SendFlag flags, uint timeoutMs)
  { CheckLink();
    DoSend(cvt.FromObject(data, index, length), flags, timeoutMs, data);
  }
  public void Send(object data, SendFlag flags, uint timeoutMs)
  { CheckLink();
    DoSend(cvt.FromObject(data), flags, timeoutMs, data);
  }

  void DoSend(byte[] data, SendFlag flags, uint timeoutMs, object orig)
  { link.Send(data, 0, data.Length, flags, timeoutMs, orig);
  }
  void CheckLink()   { if(link==null) throw new InvalidOperationException("Client is not connected"); }
  void CheckClosed() { if(link!=null) throw new InvalidOperationException("Client is already connected"); }

  void ThreadFunc()
  { NetLink[] links = new NetLink[] { link };
    while(!quit)
    { bool did=false;
      lock(this)
        try
        { link.Poll();

          LinkMessage msg = link.ReceiveMessage();
          if(msg!=null)
          { did=true;
            if(MessageReceived!=null) MessageReceived(this, cvt.ToObject(msg));
          }
          else if(!link.Connected)
          { did=true;
            if(!quit && Disconnected!=null) Disconnected(this);
          }
        }
        catch { }

      if(!did) NetLink.WaitForEvent(links, 1000);
    }
  }

  bool OnMessageSent(NetLink link, LinkMessage msg)
  { lock(this)
    { if(eMessageSent!=null) eMessageSent(this, msg.Tag);
      return true;
    }
  }

  bool OnRemoteReceived(NetLink link, LinkMessage msg)
  { if(RemoteReceived==null) RemoteReceived(this, msg.Tag);
    return true;
  }

  MessageConverter  cvt = new MessageConverter();
  ClientSentHandler eMessageSent;
  NetLink link;
  Thread  thread;
  bool    quit;
}
#endregion

} // namespace GameLib.Network