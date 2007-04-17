/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2007 Adam Milazzo

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

// TODO: stress test the locking, changes were made that may have destabilized it
// TODO: implement remote received notification
// TODO: implement IPv6 support (it might be done already!)
// TODO: add a 'Peer' class
// FIXME: allow serialization of arrays
// TODO: consider replacing INetSerializable.Deserialize with a constructor that takes a certain struct/class. this would require using a Stream rather than a byte[], though.

using System;
using ArrayList=System.Collections.ArrayList;
using IEnumerable=System.Collections.ArrayList;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Security.Permissions;
using GameLib.IO;

namespace GameLib.Network
{

#region Common types
/// <summary>This interface allows an object to customize the serialization process.</summary>
/// <remarks>
/// It is only necessary to implement this interface for classes containing reference types, but any class or
/// struct can implement it if it wants to customize the serialization process. This interface is used rather
/// than the standard .NET serialization interface because the .NET serialization interface, while more
/// flexible, has comparatively low performance. 
/// </remarks>
public interface INetSerializable
{ 
  /// <summary>Calculates the size of the serialized data.</summary>
  /// <returns>The amount of space that will be required for the serialized data, in bytes.</returns>
  /// <remarks>This method will be called by the serialization engine so that it can allocate a buffer of an
  /// appropriate size.
  /// </remarks>
  int  SizeOf();

  /// <summary>Serializes an object into a byte array.</summary>
  /// <param name="buf">The byte array into which the object should be serialized.</param>
  /// <param name="index">The index into <paramref name="buf"/> at which data should be written.</param>
  /// <remarks>This method will be called by the serialization engine to serialize an object. The only assumption
  /// that can be made about <paramref name="buf"/> is that there are at least N bytes at <paramref name="index"/>
  /// than can be used, where N is the value obtained by calling <see cref="SizeOf"/>.
  /// </remarks>
  void SerializeTo(byte[] buf, int index);

  /// <summary>Deserializes an object from a byte array.</summary>
  /// <param name="buf">The byte array from which the object should be deserialized.</param>
  /// <param name="index">The index into <paramref name="buf"/> from which data should be read.</param>
  /// <remarks>The engine will create an instance of the object using the default constructor and then call this
  /// method on it. The only assumption that can be made about <paramref name="buf"/> is that there are at least
  /// N bytes at <paramref name="index"/> than can be used, where N was the value obtained by calling <see
  /// cref="SizeOf"/> during the serialization process.
  /// </remarks>
  /// <returns>The number of bytes read from <paramref name="buf"/>. The return value is used when deserializing
  /// arrays of objects that implement <see cref="INetSerializable"/>. Since each object can be a different size,
  /// the return value is used to find the offset of the next item in the array. This should be the same as the
  /// value that <see cref="SizeOf"/> returned during serialization.
  /// </returns>
  int DeserializeFrom(byte[] buf, int index);
}

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

/// <summary>This enumeration is used to control how a message will be sent.
/// The flags can be ORed together to combine their effects.
/// </summary>
[Flags]
public enum SendFlag
{
  /// <summary>
  /// The message will be sent at normal priority, unreliably, and may possibly arrive out of order.
  /// No notification events will be raised, and a copy of the message data well be created if the networking
  /// engine can't prove that it's unnecessary.
  /// </summary>
  None=0,

  /// <summary>The message will be sent reliably, meaning that it's guaranteed to arrive as long as the link
  /// isn't broken.
  /// </summary>
  Reliable=1,
  
  /// <summary>The messages will be received in the order that they were sent. Note that messages may still
  /// arrive out of order if a reliable transport is not used. If this happens, the older messages will be
  /// dropped. To guarantee that all messages will be received, and in the right order, use the
  /// <see cref="ReliableSequential"/> flag.
  /// </summary>
  Sequential=2,
  
  /// <summary>This flag causes a notification event to be raised when the message is actually sent out over the
  /// network.
  /// </summary>
  NotifySent=4,
  
  /// <summary>This flag causes a notification event to be raised when the remote host receives and processes the
  /// message. Note that specifying this flag uses additional network and machine resources, as the remote host must
  /// generate a reply saying that it received the message, and the local host must hold the message in memory
  /// until it receives the reply or it determines that the messages was ignored or not received.
  /// </summary>
  NotifyReceived=8,
  
  /// <summary>This flag tells the networking engine to not make a copy of the message data. If you know that that
  /// the data passed to the Send() function will not be altered after the engine is done with it, then you can
  /// safely use this flag. If this flag is not used, the engine will copy the message data into a separate buffer.
  /// Note that the networking engine can ignore this flag if it wishes.
  /// </summary>
  NoCopy=16,
  
  /// <summary>The message will be sent with low priority. This means it will be sent only if no messages of higher
  /// priority are waiting to be sent.
  /// </summary>
  LowPriority=32,
  
  /// <summary>The message will be sent with high priority. This means it will be sent before all messages of lower
  /// priority.
  /// </summary>
  HighPriority=64,

  /// <summary>This flag is the combination of the <see cref="Reliable"/> and <see cref="Sequential"/> flags.</summary>
  ReliableSequential = Reliable|Sequential,
}

/// <summary>This enumeration is passed to <see cref="NetLink.GetQueueStatus"/> in order to control what data will
/// be returned. The flags can be ORed together to combine their effects.
/// </summary>
[Flags]
public enum QueueStat
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
  SendStats=LowPriority|NormalPriority|HighPriority,
  /// <summary>Data about the receive queue and all send queues will be returned.</summary>
  AllStats=SendStats|ReceiveQueue
}

/// <summary>This class handles the serialization and deserialization of objects for the networking engine.</summary>
/// <remarks>Types to be serialized or deserialized must be registered (with the sole exception of arrays of
/// <see cref="System.Byte"/>), and furthermore, must be registered in the same order in order to be deserialized
/// by an instance of this class. This means that that for an application using Client and Server, the same types
/// must be registered in the Client and Server in the same order, or else they will not be able to communicate
/// successfully. See <see cref="RegisterType"/> for more information. <seealso cref="RegisterType"/>
/// </remarks>
public sealed class MessageConverter
{ 
  /// <summary>Returns true if the converter must alter a raw array of <see cref="System.Byte"/>passed to it.</summary>
  /// <remarks>This will be true if any types are registered. If true, even byte arrays must be serialized using
  /// this class because a header needs to be attached. If false, byte arrays can be sent directly over the network.
  /// Note that it's always safe to serialize a byte array, so this property is provided only to allow certain
  /// optimizations.
  /// </remarks>
  public bool AltersByteArray { get { return typeIDs.Count!=0; } }

  /// <summary>Returns the number of types that are currently registered.</summary>
  public int NumTypes { get { return typeIDs.Count; } }

  /// <summary>Unregisters all registered types.</summary>
  public void ClearTypes()
  { types.Clear();
    typeIDs.Clear();
  }

  /// <summary>Registers a list of types.</summary>
  /// <param name="types">An array of <see cref="System.Type"/> holding types to be registered.</param>
  /// <include file="documentation.xml" path="//Network/MessageConverter/RegisterType/*"/>
  public void RegisterTypes(params Type[] types) { foreach(Type type in types) RegisterType(type); }

  /// <summary>Registers a given type, allowing it to be serialized and deserialized by this class.</summary>
  /// <param name="type">The <see cref="System.Type"/> to be registered.</param>
  /// <include file="documentation.xml" path="//Network/MessageConverter/RegisterType/*"/>
  public void RegisterType(Type type)
  { if(type==null) throw new ArgumentNullException("type");
    if(typeIDs.ContainsKey(type)) throw new ArgumentException(type.ToString()+" has already been registered.", "type");
    if(type.IsArray) throw new ArgumentException("Arrays types cannot be registered. Instead, register the element type of the array. For instance, if you want to send an array of System.Double, register the System.Double type.", "type");
    if(type.HasElementType) throw new ArgumentException("Pointer/reference types cannot be registered.", "type");

    TypeInfo info;
    if(type.GetInterface("GameLib.Network.INetSerializable")!=null)
    { ConstructorInfo cons = type.GetConstructor(Type.EmptyTypes);
      if(cons==null) throw new ArgumentException(type.ToString()+" {0} has no default constructor.");
      info = new TypeInfo(type, cons);
    }
    else
    { info = new TypeInfo(type, null);
      info.Size = Marshal.SizeOf(type);
      MarshalType mt = GetMarshalType(type);
      if(mt==MarshalType.Bad)
        throw new ArgumentException("Non-blittable types (types containing reference fields, or unformatted classes) cannot be serialized unless they implement the INetSerializable interface or use MarshalAsAttribute to convert them to blittable types.", "type");
      info.Unsafe = mt==MarshalType.Unsafe;
    }

    int  i;
    for(i=0; i<types.Count; i++) if(types[i]==null) break;
    if(i==types.Count) types.Add(info); else types[i] = info;
    typeIDs[type] = (uint)i;
  }

  /// <summary>Unregisters a list of types.</summary>
  /// <param name="types">An array of <see cref="System.Type"/> holding types to be unregistered.</param>
  public void UnregisterTypes(params Type[] types) { foreach(Type type in types) UnregisterType(type); }

  /// <summary>Unregisters a given type.</summary>
  /// <param name="type">The <see cref="System.Type"/> to unregister.</param>
  public void UnregisterType(Type type)
  { if(!typeIDs.ContainsKey(type)) throw new ArgumentException(type.ToString()+" is not a registered type");
    uint index = (uint)typeIDs[type];
    typeIDs.Remove(type);
    types[(int)index] = null;
  }

  /// <summary>Deserializes an object contained in a <see cref="LinkMessage"/>.</summary>
  /// <param name="msg">The <see cref="LinkMessage"/> holding the data to be deserialized.</param>
  /// <returns>An object by created by deserializing the data from the message.</returns>
  public object Deserialize(LinkMessage msg) { return Deserialize(msg.data, msg.index, msg.length); }

  /// <summary>Deserializes an object contained in an array.</summary>
  /// <param name="data">The array of <see cref="System.Byte"/> from where the object will be deserialized.</param>
  /// <param name="index">The index into <paramref name="data"/> at which the object data begins.</param>
  /// <param name="length">The length of the object data to be used for deserialization.</param>
  /// <returns>An object by created by deserializing the data from the array.</returns>
  public unsafe object Deserialize(byte[] data, int index, int length)
  { if(!AltersByteArray)
    { if(length==data.Length) return data;
      else
      { byte[] buf = new byte[length];
        Array.Copy(data, index, buf, 0, length);
        return buf;
      }
    }
    else
    { uint id = IOH.ReadBE4U(data, index);
      bool isArray = (id&0x80000000)!=0;
      if(isArray) id &= 0x7FFFFFFF;

      if(id==0)
      { byte[] buf = new byte[length-4];
        Array.Copy(data, index+4, buf, 0, length-4);
        return buf;
      }

      if(id>types.Count || types[(int)id-1]==null) throw new ArgumentException("Unregistered type id: "+id);
      TypeInfo info = (TypeInfo)types[(int)id-1];
      Type type = info.Type;

      if(isArray)
      { if(info.ConsInterface!=null)
        { ArrayList objects = new ArrayList();
          for(int i=4; i<data.Length; )
          { INetSerializable ns = (INetSerializable)info.ConsInterface.Invoke(null);
            i += ns.DeserializeFrom(data, i);
            objects.Add(ns);
          }
          return objects.ToArray(type);
        }

        Array arr = Array.CreateInstance(type, (length-4)/info.Size);
        if(arr.Length!=0)
          fixed(byte* src=data)
          { GCHandle handle = GCHandle.Alloc(arr, GCHandleType.Pinned);
            try
            { IntPtr dest = Marshal.UnsafeAddrOfPinnedArrayElement(arr, 0);
              Interop.Unsafe.Copy(src+index+4, dest.ToPointer(), arr.Length*info.Size);
            }
            finally { handle.Free(); }
          }
        return arr;
      }
      else if(info.ConsInterface!=null)
      { INetSerializable ns = (INetSerializable)info.ConsInterface.Invoke(null);
        ns.DeserializeFrom(data, index+4);
        return ns;
      }
      else fixed(byte* src=data) return Marshal.PtrToStructure(new IntPtr(src+index+4), type);
    }
  }

  /// <summary>Serializes an array of <see cref="System.Byte"/>.</summary>
  /// <param name="data">An array of <see cref="System.Byte"/>.</param>
  /// <returns>An array that can later be passed to <see cref="Deserialize"/> to retrieve the original data.
  /// If no header information needs to be added to the original array, then the original array will be returned
  /// directly.
  /// </returns>
  public byte[] Serialize(byte[] data)
  { if(data==null) throw new ArgumentNullException("data");
    return AltersByteArray ? Serialize(data, 0, data.Length) : data;
  }

  /// <summary>Serializes a segment of an array of <see cref="System.Byte"/>.</summary>
  /// <param name="data">An array of <see cref="System.Byte"/>.</param>
  /// <param name="index">The index into <paramref name="data"/> at which the data to be serialized begins.</param>
  /// <param name="length">The length of the data to be serialized.</param>
  /// <returns>An array that can later be passed to <see cref="Deserialize"/> to retrieve the original data.
  /// If no header information needs to be added to the original array and the array doesn't need to be resized, then
  /// the original array will be returned directly.
  /// </returns>
  public byte[] Serialize(byte[] data, int index, int length)
  { if(data==null) throw new ArgumentNullException("data");
    if(index<0 || length<0 || index+length>data.Length) throw new ArgumentOutOfRangeException();

    if(AltersByteArray)
    { byte[] ret = new byte[length+4];
      Array.Copy(data, index, ret, 4, length);
      return ret;
    }
    else if(length==data.Length) return data;
    else
    { byte[] ret = new byte[length];
      Array.Copy(data, index, ret, 0, length);
      return ret;
    }
  }

  /// <summary>Serializes an object.</summary>
  /// <param name="obj">The object to serialize.</param>
  /// <returns>An array of <see cref="System.Byte"/> that can later be passed to <see cref="Deserialize"/> to
  /// reconstruct the original object.
  /// </returns>
  /// <remarks>If the object is not an array of <see cref="System.Byte"/>, the object's type must be registered
  /// (using <see cref="RegisterType"/>) before you can serialize it.
  /// </remarks>
  public unsafe byte[] Serialize(object obj)
  { if(obj==null) throw new ArgumentNullException("obj");
    if(obj is byte[]) return Serialize((byte[])obj);

    if(typeIDs.Count==0)
      throw new ArgumentException("If no types are registered, only byte[] can be sent.");
    else
    { Type type = obj.GetType();
      bool isArray = type.IsArray;

      if(isArray)
      { if(type.GetArrayRank()!=1) throw new ArgumentException("Can't automatically serialize arrays with more than one dimension. Wrap them in an object that implements INetSerializable.");
        type=type.GetElementType();
      }

      if(!typeIDs.ContainsKey(type)) throw new ArgumentException(String.Format("{0} is not a registered type", type));
      uint id = (uint)typeIDs[type];
      TypeInfo info = (TypeInfo)types[(int)id];
      byte[]   ret;

      if(isArray)
      { Array arr = (Array)obj;

        if(info.ConsInterface!=null)
        { int i=0, j=0;
          unsafe
          { int* sizes = stackalloc int[arr.Length];
            foreach(INetSerializable ns in arr) { sizes[i]=ns.SizeOf(); j += sizes[i++]; }
            ret = new byte[j+4];
            i=0; j=4;
            foreach(INetSerializable ns in arr) { ns.SerializeTo(ret, j); j += sizes[i++]; }
          }
        }
        else
        { int size = info.Size;
          ret = new byte[arr.Length*size+4];

          if(arr.Length!=0)
            fixed(byte* dest=ret)
              if(info.Unsafe)
                for(int i=0,j=4; i<arr.Length; j+=size,i++)
                { IntPtr dp = new IntPtr(dest+j);
                  Marshal.StructureToPtr(arr.GetValue(i), dp, false);
                  Marshal.DestroyStructure(dp, type);
                }
              else
              { GCHandle handle = GCHandle.Alloc(arr, GCHandleType.Pinned);
                try
                { IntPtr src = Marshal.UnsafeAddrOfPinnedArrayElement(arr, 0);
                  Interop.Unsafe.Copy(src.ToPointer(), dest+4, ret.Length-4);
                }
                finally { handle.Free(); }
              }
        }
      }
      else if(info.ConsInterface!=null)
      { INetSerializable ns = (INetSerializable)obj;
        ret = new byte[ns.SizeOf()+4];
        ns.SerializeTo(ret, 4);
      }
      else
      { ret = new byte[info.Size+4];
        fixed(byte* dest=ret)
        { IntPtr dp = new IntPtr(dest+4);
          Marshal.StructureToPtr(obj, dp, false);
          if(info.Unsafe) Marshal.DestroyStructure(dp, type);
        }
      }

      id++;
      if(isArray) id |= 0x80000000;
      IOH.WriteBE4U(ret, 0, id);
      return ret;
    }
  }

  sealed class TypeInfo
  { public TypeInfo(Type type, ConstructorInfo cons) { Type=type; ConsInterface=cons; }
    public Type Type;
    public ConstructorInfo ConsInterface;
    public int  Size;
    public bool Unsafe;
  }

  enum MarshalType { Bad, Unsafe, Safe };

  List<TypeInfo> types = new List<TypeInfo>();
  Dictionary<Type,uint> typeIDs = new Dictionary<Type,uint>();

  static MarshalType GetMarshalType(Type type)
  { if(type.IsPrimitive) return MarshalType.Safe;
    if(!type.IsValueType && !type.IsLayoutSequential && !type.IsExplicitLayout) return MarshalType.Bad;
    try { new ReflectionPermission(ReflectionPermissionFlag.TypeInformation).Demand(); }
    catch(System.Security.SecurityException) { return MarshalType.Unsafe; }
    return GetMarshalType(type, null);
  }

  static MarshalType GetMarshalType(Type type, List<Type> saw)
  { bool safe = true;
    foreach(FieldInfo fi in type.GetFields(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public))
    { Type ft = fi.FieldType;
      if(ft.IsPrimitive) continue;
      if(!ft.IsValueType && !ft.IsLayoutSequential && !ft.IsExplicitLayout)
      { // TODO: now that we have .NET 2.0, test this code. (was FIXME: this only works in .NET 2.0!)
        MarshalAsAttribute ma = (MarshalAsAttribute)Attribute.GetCustomAttribute(fi, typeof(MarshalAsAttribute), false);
        if(ma!=null)
          switch(ma.Value)
          { case UnmanagedType.Bool: case UnmanagedType.ByValArray: case UnmanagedType.ByValTStr:
            case UnmanagedType.I1: case UnmanagedType.I2: case UnmanagedType.I4: case UnmanagedType.I8:
            case UnmanagedType.R4: case UnmanagedType.R8: case UnmanagedType.SysInt: case UnmanagedType.SysUInt:
            case UnmanagedType.U1: case UnmanagedType.U2: case UnmanagedType.U4: case UnmanagedType.U8:
            case UnmanagedType.VariantBool:
              safe=false; continue;
          }
        return MarshalType.Bad;
      }

      if(saw==null) saw = new List<Type>();
      if(!saw.Contains(ft))
      { saw.Add(ft);
        MarshalType mt = GetMarshalType(ft, saw);
        if(mt==MarshalType.Bad) return mt;
        else if(mt==MarshalType.Unsafe) safe=false;
      }
    }
    return safe ? MarshalType.Safe : MarshalType.Unsafe;
  }
}
#endregion

#region NetLink class and supporting types
/// <summary>This delegate is used for the <see cref="NetLink.Connected"/> and <see cref="NetLink.Disconnected"/>
/// events.
/// </summary>
public delegate void NetLinkHandler(NetLink link);
/// <summary>This delegate is used for the <see cref="NetLink.MessageSent"/> and <see cref="NetLink.RemoteReceived"/>
/// events.
/// </summary>
public delegate void LinkMessageHandler(NetLink link, LinkMessage msg);
/// <summary>This delegate is used for the <see cref="NetLink.MessageReceived"/> event.
/// If it returns false, the message will be dropped.
/// </summary>
public delegate bool LinkMessageRecvHandler(NetLink link, LinkMessage msg);

/// <summary>This class represents a network message.</summary>
public sealed class LinkMessage
{ internal LinkMessage() { }
  internal LinkMessage(uint header, byte[] data, int index, int length, SendFlag flags, int timeout, object tag)
  { this.header=header; this.data=data; this.index=index; this.length=length; this.flags=flags; this.tag=tag;
    if(timeout!=0) this.deadline = Timing.Msecs+(uint)timeout;
  }

  /// <summary>This property returns the array that contains the message data.</summary>
  public byte[] Data { get { return data; } }
  /// <summary>This property returns the index into <see cref="Data"/> at which the message data begins.</summary>
  public int Index { get { return index; } }
  /// <summary>This property returns the length of the message data, in bytes.</summary>
  public int Length { get { return length; } }
  /// <summary>This property returns the <see cref="SendFlag"/> value that was used to send the message.</summary>
  public SendFlag Flags { get { return flags; } }
  /// <summary>This property returns the context value that was passed to the Send() method.</summary>
  /// <remarks>Note that this value is not transmitted across the network, so it will be null for received messages.
  /// </remarks>
  public object Tag { get { return tag; } }

  /// <summary>Returns the message data. Note that calling this method may alter the values of <see cref="Index"/> and
  /// <see cref="Data"/>.
  /// </summary>
  /// <returns>An array of <see cref="System.Byte"/> containing only the message data. The array's length will
  /// be equal to <see cref="Length"/>. If no copying is necessary, the returned array will be identical to
  /// <see cref="Data"/>.
  /// </returns>
  public byte[] Shrink()
  {
    if(length != data.Length)
    {
      byte[] newData = new byte[length];
      Array.Copy(data, index, newData, 0, length);
      index = 0;
      data  = newData;
    }
    return data;
  }

  internal byte[]   data;
  internal object   tag;
  internal int      index, length, sent;
  internal uint     deadline, header, lag;
  internal SendFlag flags;
}

/// <summary>This class represents a message-oriented network connection.</summary>
/// <remarks>The low-level implementation of this class is not defined. It may any IP protocol to perform its data
/// transfer, including TCP, UDP, or something else entirely. It may make any number of actual network connections,
/// possibly zero even. Because of this, it is only suitable for communicating with other NetLink objects. However,
/// communications are initialized using TCP, so the following method of code is valid:
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
  public const int NoTimeout=-1;

  /// <summary>This event is raised when the network link is connected.</summary>
  public event NetLinkHandler Connected;
  /// <summary>This event is raised when the network link is disconnected.</summary>
  public event NetLinkHandler Disconnected;
  /// <summary>This event is raised when a message is received.</summary>
  /// <remarks>Each handler attached to this event can return false, indicating that the message should be
  /// dropped. If that occurs, the message will not be passed to any remaining handlers.
  /// </remarks>
  public event LinkMessageRecvHandler MessageReceived;
  /// <summary>This event is raised when a message is sent over the network.</summary>
  /// <remarks>Note that the message must have been sent with the <see cref="SendFlag.NotifySent"/> flag
  /// in order for this event to be raised.
  /// </remarks>
  public event LinkMessageHandler MessageSent;
  /// <summary>This event is raised when a message is received by the remote host.</summary>
  /// <remarks>Note that the message must have been sent with the <see cref="SendFlag.NotifyReceived"/> flag
  /// in order for this event to be raised.
  /// </remarks>
  public event LinkMessageHandler RemoteReceived;

  /// <include file="documentation.xml" path="//Network/Common/DefaultFlags/*"/>
  public SendFlag DefaultFlags { get { return defFlags; } set { defFlags=value; } }

  /// <include file="documentation.xml" path="//Network/Common/IsConnected/*"/>
  public bool IsConnected
  { get
    { if(!connected) return false;
      Socket tcp = this.tcp;
      return tcp!=null && tcp.Connected;
    }
  }

  /// <include file="documentation.xml" path="//Network/Common/LagAverage/*"/>
  public int LagAverage { get { return lagAverage;  } set { lagAverage =value; } }
  /// <include file="documentation.xml" path="//Network/Common/LagVariance/*"/>
  public int LagVariance { get { return lagVariance; } set { lagVariance=value; } }

  /// <summary>Returns true if a message is waiting to be retrieved.</summary>
  public bool MessageWaiting
  { get
    { lock(recv)
      { if(recv.Count==0)
        { ReceivePoll();
          return recv.Count!=0;
        }
        else return true;
      }
    }
  }

  /// <summary>Returns the remote endpoint to which this link is connected, or null if the link is closed.</summary>
  public IPEndPoint RemoteEndPoint { get { return tcp==null ? null : (IPEndPoint)tcp.RemoteEndPoint; } }

  /// <summary>Connects to a remote host.</summary>
  /// <param name="host">The name of the host to connect to.</param>
  /// <param name="port">The port to connect to.</param>
  public void Open(string host, int port) { Open(new IPEndPoint(Dns.GetHostEntry(host).AddressList[0], port)); }

  /// <summary>Connects to a remote host.</summary>
  /// <param name="remote">The <see cref="IPEndPoint"/> specifying the address and port to connect to.</param>
  public void Open(IPEndPoint remote)
  { Socket sock = null;
    sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    sock.Connect(remote);
    Open(sock);
  }

  /// <summary>Initializes the connection from a socket that's already connected.</summary>
  /// <param name="tcp">A TCP stream <see cref="Socket"/> that's already connected.</param>
  public void Open(Socket tcp)
  { if(tcp==null) throw new ArgumentNullException("tcp");
    if(!tcp.Connected) throw new ArgumentException("The socket must be connected already!", "tcp");

    Disconnect();
    IPEndPoint localTcp = (IPEndPoint)tcp.LocalEndPoint, remote = (IPEndPoint)tcp.RemoteEndPoint;

    Socket udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    udp.Bind(new IPEndPoint(localTcp.Address, localTcp.Port));  // bind the udp to the same port/interface
    udp.Connect(remote);

    tcp.Blocking = false;
    udp.Blocking = false;

    udpMax = 1450; // this is common for ethernet, but maybe too high for dialup (???)
    recv = new Queue<LinkMessage>();
    nextSize  = -1;
    this.tcp = tcp;
    this.udp = udp;
    connected = true;
    OnConnected();
  }

  /// <summary>Closes the connection to the remote host.</summary>
  /// <remarks>Calling this method is equivalent to calling <see cref="Close(bool)"/> and passing true. See
  /// <see cref="Close(bool)"/> for more information.
  /// </remarks>
  public void Close() { Close(true); }
  /// <summary>Closes the connection to the remote host.</summary>
  /// <param name="force">If false, the connection is shut down and sending is disabled, but it will not be closed
  /// until all data has been received (you must retrieve all messages with one of the message retrieval functions).
  /// If true, the connection is forcibly closed and any data sent by the remote host will be lost.
  /// </param>
  public void Close(bool force)
  { sendQueue=0;
    if(low!=null)
      lock(low)
      { low.Clear();
        low=null;
      }
    if(norm!=null)
      lock(norm)
      { norm.Clear();
        norm=null;
      }
    if(high!=null)
      lock(high)
      { high.Clear();
        high=null;
      }
    if(tcp!=null)
      lock(recv)
        if(force) Disconnect();
        else tcp.Shutdown(SocketShutdown.Send);
    sendQueue=0; // duplicated just in case SendPoll() changes it
  }

  /// <include file="documentation.xml" path="//Network/Common/GetQueueStatus/*"/>
  public QueueStatus GetQueueStatus(QueueStat flags)
  { QueueStatus status = new QueueStatus();
    if(connected)
    { if((flags&QueueStat.LowPriority)!=0 && low!=null)
        lock(low) foreach(LinkMessage msg in low) { status.SendMessages++; status.SendBytes+=msg.Length; }
      if((flags&QueueStat.NormalPriority)!=0 && norm!=null)
        lock(norm) foreach(LinkMessage msg in norm) { status.SendMessages++; status.SendBytes+=msg.Length; }
      if((flags&QueueStat.HighPriority)!=0 && high!=null)
        lock(high) foreach(LinkMessage msg in high) { status.SendMessages++; status.SendBytes+=msg.Length; }
    }
    if((flags&QueueStat.ReceiveQueue)!=0)
      lock(recv) foreach(LinkMessage msg in recv) { status.ReceiveMessages++; status.ReceiveBytes+=msg.Length; }
    return status;
  }

  /// <summary>Returns the next message if it exists, without removing it from the receive queue.</summary>
  /// <returns>A <see cref="LinkMessage"/> representing the next message, or null if no message is waiting.</returns>
  public LinkMessage PeekMessage()
  { if(recv==null) throw new InvalidOperationException("Link has not been opened");
    lock(recv)
    { if(recv.Count==0) ReceivePoll();
      return recv.Count!=0 ? recv.Peek() : null;
    }
  }

  /// <summary>Returns the next message.</summary>
  /// <returns>An array of <see cref="System.Byte"/> containing the message data.</returns>
  public byte[] Receive() { return Receive(NoTimeout); }

  /// <summary>Returns the next message, or null if no message is received within the given timeout.</summary>
  /// <param name="timeoutMs">The number of milliseconds to wait for a message. If the queue is empty and no message
  /// arrives within the given time frame, null will be returned. If <see cref="NetLink.NoTimeout"/> is passed, the
  /// method will wait forever forever for a message (although it will return early if the connection is broken).
  /// </param>
  /// <returns>An array of <see cref="System.Byte"/> containing the message data, or null if no message was received
  /// in time.
  /// </returns>
  public byte[] Receive(int timeoutMs)
  { LinkMessage m = ReceiveMessage();
    if(m==null)
    { if(timeoutMs!=0 && WaitForEvent(timeoutMs))
        lock(recv) m = recv.Count==0 ? null : recv.Dequeue();
      if(m==null) return null;
    }
    return m.Shrink();
  }

  /// <summary>Returns the next message if it exists.</summary>
  /// <returns>A <see cref="LinkMessage"/> representing the next message, or null if no message is waiting.</returns>
  public LinkMessage ReceiveMessage()
  { if(recv==null) throw new InvalidOperationException("Link has not been opened");
    lock(recv)
    { if(recv.Count==0) ReceivePoll();
      return recv.Count!=0 ? recv.Dequeue() : null;
    }
  }

  /// <include file="documentation.xml" path="//Network/NetLink/Send/Common/*"/>
  public void Send(byte[] data) { Send(data, 0, data.Length, defFlags, 0, null); }
  public void Send(byte[] data, int length) { Send(data, 0, length, defFlags, 0, null); }
  /// <include file="documentation.xml" path="//Network/NetLink/Send/*[self::Common or self::Length or self::Index]/*"/>
  public void Send(byte[] data, int index, int length) { Send(data, index, length, defFlags, 0, null); }
  /// <include file="documentation.xml" path="//Network/NetLink/Send/*[self::Common or self::Length or self::Index or self::Flags]/*"/>
  public void Send(byte[] data, int index, int length, SendFlag flags) { Send(data, index, length, flags, 0, null); }
  /// <include file="documentation.xml" path="//Network/NetLink/Send/*[self::Common or self::Length or self::Index or self::Flags or self::Timeout]/*"/>
  public void Send(byte[] data, int index, int length, SendFlag flags, int timeoutMs) { Send(data, index, length, flags, timeoutMs, null); }
  /// <include file="documentation.xml" path="//Network/NetLink/Send/*[self::Common or self::Length or self::Flags]/*"/>
  public void Send(byte[] data, int length, SendFlag flags) { Send(data, 0, length, flags, 0, null); }
  /// <include file="documentation.xml" path="//Network/NetLink/Send/*[self::Common or self::Length or self::Flags or self::Timeout]/*"/>
  public void Send(byte[] data, int length, SendFlag flags, int timeoutMs) { Send(data, 0, length, flags, timeoutMs, null); }
  /// <include file="documentation.xml" path="//Network/NetLink/Send/*[self::Common or self::Flags]/*"/>
  public void Send(byte[] data, SendFlag flags) { Send(data, 0, data.Length, flags, 0, null); }
  /// <include file="documentation.xml" path="//Network/NetLink/Send/*[self::Common or self::Flags or self::Timeout]/*"/>
  public void Send(byte[] data, SendFlag flags, int timeoutMs) { Send(data, 0, data.Length, flags, timeoutMs, null); }
  /// <include file="documentation.xml" path="//Network/NetLink/Send/*[self::Common or self::Length or self::Index or self::Flags or self::Timeout]/*"/>
  /// <param name="tag">Arbitrary data that will be associated with this message and that can be accessed via
  /// <see cref="LinkMessage.Tag"/>. The data is not examined or modified by the network engine. Note that this means
  /// <see cref="LinkMessage.Tag"/> will always be null on the receiving end.
  /// </param>
  public void Send(byte[] data, int index, int length, SendFlag flags, int timeoutMs, object tag)
  {
    if(!connected) throw new InvalidOperationException("Link is not open");
    if((flags&SendFlag.NotifyReceived)!=0)
      throw new NotImplementedException("SendFlag.NotifyReceived is not yet implemented");
    if(!IsConnected) throw new ConnectionLostException();
    if(length>65535) throw new DataTooLargeException(65535);
    if(timeoutMs < 0 || index<0 || length<0 || index+length>data.Length)
    {
      throw new ArgumentOutOfRangeException("index or length");
    }

    Queue<LinkMessage> queue;
    if((flags&SendFlag.HighPriority)!=0)
    { if(high==null) high = new Queue<LinkMessage>();
      queue = high;
    }
    else if((flags&SendFlag.LowPriority)!=0)
    { if(low==null) low = new Queue<LinkMessage>();
      queue = low;
    }
    else
    { if(norm==null) norm = new Queue<LinkMessage>();
      queue = norm;
    }

    // sequence number uses top two bits of sendflags field to provide a 10-bit sequence number
    uint header = (ushort)length | ((uint)(flags&~(SendFlag)HeadFlag.Mask)<<16);
    if((flags&SendFlag.Sequential)!=0)
    { header |= (uint)sendSeq<<22;
      if(++sendSeq>=SeqMax) sendSeq=0;
    }

    if((flags&SendFlag.NoCopy)==0)
    { byte[] narr = new byte[length];
      Array.Copy(data, index, narr, 0, length);
      data=narr; index=0;
    }

    LinkMessage m = new LinkMessage(header, data, index, length, flags, timeoutMs, tag);
    if(lagAverage!=0 || lagVariance!=0)
    { int lag = Utility.Random.Next((int)lagVariance*2)+(int)lagAverage-(int)lagVariance;
      if(lag!=0) { m.lag = Timing.Msecs+(uint)lag; m.deadline += m.lag; }
    }

    lock(queue)
    { queue.Enqueue(m);
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
  { lock(recv)
    { if(!connected) return;
      if(tcp!=null)
        try
        { while(true)
          { int avail = tcp.Available;
            if(avail==0)
            { if(tcp.Poll(0, SelectMode.SelectRead) && tcp.Available==0) Disconnect();
              break;
            }
            if(nextSize==-1)
            { if(avail>=HeaderSize) // check for a message header
              { SizeBuffer(HeaderSize);
                tcp.Receive(recvBuf, HeaderSize, SocketFlags.None);
                uint header = IOH.ReadBE4U(recvBuf, 0);
                nextSize  = (int)(header&0xFFFF);
                nextIndex = 0;
                recvFlags = (SendFlag)((header>>16)&~(byte)HeadFlag.Mask);
                recvSeq   = (ushort)(header>>22);
                avail -= HeaderSize;
              }
            }
            if(nextSize!=-1 && avail>=nextSize)
            { SizeBuffer(nextSize);
              int read = avail==0 ? 0 : tcp.Receive(recvBuf, nextIndex, nextSize, SocketFlags.None);
              nextSize -= read; nextIndex += read;
              if(nextSize==0)
              { nextSize = -1;
                if((recvFlags&SendFlag.Sequential)!=0)
                { if(IsBadSequence) continue;
                  else nextSeq = recvSeq;
                }

                LinkMessage m = new LinkMessage();
                m.index  = 0;
                m.length = nextIndex;
                m.data   = new byte[nextIndex];
                m.flags  = recvFlags;
                Array.Copy(recvBuf, m.data, m.length);
                OnMessageReceived(m);
              }
            }
            else break;
          }
        }
        catch(SocketException) { Disconnect(); }

      if(udp!=null)
        while(udp.Available!=0)
        { int avail = udp.Available;
          SizeBuffer(avail);
          int read = udp.Receive(recvBuf, 0, avail, SocketFlags.None);
          if(read>=HeaderSize)
          { uint header = IOH.ReadBE4U(recvBuf, 0);
            int  length = (int)(header&0xFFFF);
            if(HeaderSize+length != read) continue; // ignore truncated messages (they should have been resent via tcp)

            recvFlags   = (SendFlag)((header>>16)&~(byte)HeadFlag.Mask);
            recvSeq     = (ushort)(header>>22);

            if((recvFlags&SendFlag.Sequential)!=0)
            { if(IsBadSequence) continue;
              else nextSeq = recvSeq;
            }

            LinkMessage m = new LinkMessage();
            m.index  = 0;
            m.length = read-HeaderSize; // first part is the header (same format as tcp header)
            m.data   = new byte[m.length];
            m.flags  = recvFlags;
            Array.Copy(recvBuf, HeaderSize, m.data, 0, m.length);
            OnMessageReceived(m);
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
  { if(sendQueue==0 || !connected) return;
    SendMessages(low, SendMessages(norm, SendMessages(high, true)));
  }

  /// <summary>Calling this method is equivalent to calling <see cref="SendPoll"/> and <see cref="ReceivePoll"/>.</summary>
  public void Poll() { SendPoll(); ReceivePoll(); }

  /// <summary>This methods waits for a message to be received or the connection to be lost.</summary>
  /// <param name="timeoutMs">The maximum amount of time to wait for a message or disconnection. If
  /// <see cref="NoTimeout"/> is passed, the method will wait forever.
  /// </param>
  /// <returns>Returns true if the connection was lost or a message was received, and false otherwise.</returns>
  /// <remarks>This method will return immediately if the connection is broken.</remarks>
  public bool WaitForEvent(int timeoutMs)
  { if(!IsConnected) return true;

    ArrayList read=new ArrayList(2), write=null;
    uint start = timeoutMs>0 ? Timing.Msecs : 0;
    while(true)
    { read.Add(tcp);
      read.Add(udp);
      if(sendQueue!=0)
      { if(write==null) write = new ArrayList(2);
        write.Add(tcp);
        write.Add(udp);
      }

      Socket.Select(read, write, null, timeoutMs==NoTimeout ? int.MaxValue : (int)timeoutMs*1000);
      if(read.Count!=0)
      { if(MessageWaiting) return true;
      }
      else if(!IsConnected) return true;
      if(write!=null && write.Count!=0) SendPoll();

      if(timeoutMs==0) break;
      else if(timeoutMs!=NoTimeout)
      { uint now=Timing.Msecs;
        int  elapsed=(int)(now-start);
        if(elapsed>=timeoutMs) break;
        timeoutMs -= elapsed;
        start = now;
      }

      read.Clear();
      if(write!=null) write.Clear();
    }
    return false;
  }

  /// <summary>For a set of NetLink objects, waits for a message to be received or a connection to be lost.</summary>
  /// <param name="links">A collection of <see cref="NetLink"/> objects to check.</param>
  /// <param name="timeoutMs">The maximum amount of time to wait for a message or a disconnection. If
  /// <see cref="NoTimeout"/> is passed, the method will wait forever.
  /// </param>
  /// <returns>A list of <see cref="NetLink"/> objects that either have messages waiting or are disconnected.</returns>
  public static IList<NetLink> WaitForEvent(ICollection<NetLink> links, int timeoutMs)
  { if(links.Count==0)
    { if(timeoutMs==NoTimeout) throw new ArgumentException("Infinite timeout specified, but no links were passed.");
      Thread.Sleep(timeoutMs);
      return null;
    }

    System.Collections.Specialized.ListDictionary dict = new System.Collections.Specialized.ListDictionary();
    List<NetLink> ret = null;
    ArrayList read=new ArrayList(links.Count*2), write=null;

    uint start = timeoutMs>0 ? Timing.Msecs : 0;
    while(true)
    { if(dict.Count==0)
      { foreach(NetLink link in links)
        { bool send;
          if(link.sendQueue==0) send=false;
          else
          { if(write==null) write = new ArrayList();
            send = true;
          }

          if(!link.IsConnected || link.MessageWaiting)
          { if(ret==null) ret = new List<NetLink>();
            ret.Add(link);
          }
          else
          { dict[link.tcp]=link; dict[link.udp]=link; 
            read.Add(link.tcp); read.Add(link.udp);
            if(send) { write.Add(link.tcp); write.Add(link.udp); }
          }
        }
        if(ret!=null) return ret; // return immediately if any of the links are not connected
      }
      else
        foreach(NetLink link in links)
        { bool send;
          if(link.sendQueue==0) send=false;
          else
          { if(write!=null) write = new ArrayList();
            send = true;
          }

          
          read.Add(link.tcp); read.Add(link.udp);
          if(send) { write.Add(link.tcp); write.Add(link.udp); }
        }

      if(read.Count==0)
      { if(timeoutMs!=NoTimeout) Thread.Sleep(timeoutMs);
        return null;
      }

      Socket.Select(read, write, null, timeoutMs==NoTimeout ? int.MaxValue : (int)timeoutMs*1000);
      if(read.Count!=0)
        foreach(Socket sock in read)
        { NetLink link = (NetLink)dict[sock];
          if(link.MessageWaiting || !link.IsConnected)
          { if(ret==null) ret = new List<NetLink>();
            else if(!ret.Contains(link)) ret.Add(link);
          }
        }

      if(write!=null && write.Count!=0)
        foreach(Socket sock in write) ((NetLink)dict[sock]).SendPoll();

      if(ret!=null) return ret;

      if(timeoutMs==0) break;
      else if(timeoutMs!=NoTimeout)
      { uint now = Timing.Msecs;
        int  elapsed = (int)(now-start);
        if(elapsed>=timeoutMs) break;
        timeoutMs -= elapsed;
        start = now;
      }

      read.Clear();
      if(write!=null) write.Clear();
    }

    return null;
  }

  /// <summary>Raises the <see cref="Connected"/> event.</summary>
  /// <remarks>If you override this method, be sure to call the base class' implementation.</remarks>
  protected virtual void OnConnected()
  { if(Connected!=null) Connected(this);
  }

  /// <summary>Raises the <see cref="Disconnected"/> event.</summary>
  /// <remarks>If you override this method, be sure to call the base class' implementation.</remarks>
  protected virtual void OnDisconnected()
  { if(Disconnected!=null) Disconnected(this);
  }

  /// <summary>Raises the <see cref="MessageReceived"/> event.</summary>
  /// <param name="message">The <see cref="LinkMessage"/> that was received.</param>
  /// <returns>Returns true if the message was placed in the queue and false otherwise.</returns>
  /// <remarks>If you override this method, be sure to call the base class' implementation. The proper place
  /// to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual bool OnMessageReceived(LinkMessage message)
  { if(MessageReceived==null) lock(recv) recv.Enqueue(message);
    else
    { foreach(LinkMessageRecvHandler eh in MessageReceived.GetInvocationList())
        if(!eh(this, message)) return false;
      lock(recv) recv.Enqueue(message);
    }
    return true;
  }

  /// <summary>Raises the <see cref="MessageSent"/> event.</summary>
  /// <param name="message">The <see cref="LinkMessage"/> that was sent.</param>
  /// <remarks>This method is only called for messages that have requested notification via the
  /// <see cref="SendFlag.NotifySent"/> flag. If you override this method, be sure to call the base class'
  /// implementation. The proper place to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnMessageSent(LinkMessage message)
  { if(MessageSent!=null) MessageSent(this, message);
  }

  [Flags]
  enum HeadFlag : byte { Ack=32, Mask=0xE0 }
  const int HeaderSize=4, SeqMax=1024;

  bool IsBadSequence
  {
    get { return recvSeq<nextSeq && nextSeq-recvSeq<=SeqMax*4/10; }
  }

  void SizeBuffer(int len)
  { if(len<256) len=256;
    if(recvBuf==null || recvBuf.Length<len) recvBuf=new byte[len];
  }

  bool SendMessages(Queue<LinkMessage> queue, bool trySend)
  { if(queue==null) return true;
    if(!connected) return false;

    lock(queue)
    { while(queue.Count!=0)
      { LinkMessage msg = queue.Peek();
        if(Timing.Msecs<msg.lag) return true;
        if(msg.deadline!=0 && Timing.Msecs>msg.deadline) goto Remove;
        if(!trySend) return false;

        int  sent, length=msg.Length+HeaderSize;
        bool useTcp = (msg.flags&SendFlag.Reliable)!=0 || length>udpMax;

        Retry:
        try
        { if(msg.sent==0)
          { if(useTcp)
            { if(sendBuf==null) sendBuf=new byte[HeaderSize];
              IOH.WriteBE4U(sendBuf, 0, msg.header);
              sent = tcp.Send(sendBuf, 0, HeaderSize, SocketFlags.None);

              if(sent<HeaderSize)
              { if(sendBuf.Length < length)
                { byte[] narr = new byte[length];
                  Array.Copy(sendBuf, narr, HeaderSize);
                  sendBuf = narr;
                }
                Array.Copy(msg.data, msg.index, sendBuf, HeaderSize, msg.length);
              }
              
              sent += tcp.Send(msg.data, msg.index, msg.length, SocketFlags.None);
              if(sent<length)
              { if(sendBuf.Length<length) sendBuf = new byte[length];
                Array.Copy(msg.data, 0, sendBuf, HeaderSize, msg.length);
              }
            }
            else
            { if(sendBuf==null || sendBuf.Length<length) sendBuf=new byte[length];
              IOH.WriteBE4U(sendBuf, 0, msg.header);
              Array.Copy(msg.data, msg.index, sendBuf, HeaderSize, msg.length);
              sent = udp.Send(sendBuf, 0, length, SocketFlags.None);
            }
          }
          else sent = tcp.Send(sendBuf, msg.sent, length-msg.sent, SocketFlags.None); // can only get here for TCP
        }
        catch(SocketException e)
        { if(e.ErrorCode==Config.EMSGSIZE)
          { if(!useTcp)
            { if(length<udpMax) udpMax=length;
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
        if(msg.sent!=length)
        { if(useTcp) return false;
          else // this is probably impossible but we'll guard against it anyway
          { if(length<udpMax) udpMax=length;
            msg.sent=0;
            useTcp=true;
            goto Retry;
          }
        }

        if(!useTcp && sent>udpMax) udpMax=sent;
        if((msg.Flags&SendFlag.NotifySent)!=0) OnMessageSent(msg);

        Remove:
        queue.Dequeue();
        sendQueue--;
      }
      return true;
    }
  }

  void Disconnect()
  { if(connected)
    { connected = false;
      tcp.Close();
      udp.Close();
      tcp=udp=null;
      OnDisconnected();
    }
  }

  internal uint sendQueue;

  Socket     tcp, udp;
  Queue<LinkMessage> low, norm, high, recv;
  byte[]     recvBuf, sendBuf;
  int        nextSize, nextIndex, udpMax, lagAverage, lagVariance;
  ushort     sendSeq, recvSeq, nextSeq;
  SendFlag   recvFlags, defFlags=SendFlag.ReliableSequential;
  bool       connected;
}
#endregion

#region Server class and supporting types
/// <summary>This class represents a player connected to the server.</summary>
public sealed class ServerPlayer
{ internal ServerPlayer(NetLink link, int id) { Link=link; this.id=id; }

  /// <summary>Returns true if the player is being dropped.</summary>
  /// <remarks>This property will return true if the player has been dropped, or will be dropped. It is invalid to
  /// send a message to a player that has been dropped.
  /// </remarks>
  public bool Dropping { get { return DelayedDrop; } }

  /// <summary>Gets the player's remote endpoint.</summary>
  public IPEndPoint EndPoint { get { return Link.RemoteEndPoint; } }

  /// <summary>Gets the player's unique Id.</summary>
  public int Id { get { return id; } }

  /// <summary>Arbitrary data associated with this player. This field is not examined by the network code, so you can
  /// use it to hold any data you like.
  /// </summary>
  public object Data;

  internal uint     DropDelay, DropStart;
  internal NetLink  Link;
  internal bool     DelayedDrop;

  int id;
}

/// <summary>This delegate is used for the <see cref="Server.PlayerConnecting"/> event. It receives a reference to
/// the <see cref="Server"/> and <see cref="ServerPlayer"/> objects. If it returns false, the player will be
/// immediately disconnected.
/// </summary>
public delegate bool PlayerConnectingHandler(Server server, ServerPlayer player);
/// <summary>This delegate is used for the <see cref="Server.PlayerConnected"/> event. It receives a reference to
/// the <see cref="Server"/> and <see cref="ServerPlayer"/> objects.
/// </summary>
public delegate void PlayerConnectedHandler(Server server, ServerPlayer player);
/// <summary>This delegate is used for the <see cref="Server.PlayerDisconnected"/> event. It receives a reference
/// to the <see cref="Server"/> and <see cref="ServerPlayer"/> objects.
/// </summary>
public delegate void PlayerDisconnectHandler(Server server, ServerPlayer player);
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
  /// <summary>Initializes a new <see cref="Server"/> instance and starts listening for connections.</summary>
  /// 
  public Server(IPEndPoint local) { Listen(local); }
  ~Server() { Deinitialize(); }

  /// <summary>This event is raised when a player is connecting to the server. If any handler returns false, the
  /// player will be disconnected.
  /// </summary>
  public event PlayerConnectingHandler PlayerConnecting;
  /// <summary>This event is raised when a player has connected to the server.</summary>
  public event PlayerConnectedHandler PlayerConnected;
  /// <summary>This event is raised when a player has disconnected from the server.</summary>
  public event PlayerDisconnectHandler PlayerDisconnected;
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

  /// <include file="documentation.xml" path="//Network/Common/DefaultFlags/*"/>
  public SendFlag DefaultFlags { get { return defFlags; } set { defFlags=value; } }

  /// <summary>Gets the local endpoint upon which the server is listening.</summary>
  public IPEndPoint LocalEndPoint
  { get
    { if(!listening) throw new InvalidOperationException("The server is not currently listening.");
      return (IPEndPoint)server.LocalEndpoint;
    }
  }

  /// <summary>Gets a read-only collection of the current players.</summary>
  public ReadOnlyCollection<ServerPlayer> Players { get { return players.AsReadOnly(); } }

  /// <include file="documentation.xml" path="//Network/Common/LagAverage/*"/>
  public int LagAverage
  { get { return lagAverage; }
    set
    { lock(this) foreach(ServerPlayer p in players) p.Link.LagAverage=value;
      lagAverage=value;
    }
  }

  /// <include file="documentation.xml" path="//Network/Common/LagVariance/*"/>
  public int LagVariance
  { get { return lagVariance; }
    set
    { lock(this) foreach(ServerPlayer p in players) p.Link.LagVariance=value;
      lagVariance=value;
    }
  }

  /// <include file="documentation.xml" path="//Network/Common/RegisterTypes/*"/>
  public void RegisterTypes(params Type[] types) { AssertClosed(); cvt.RegisterTypes(types); }
  /// <include file="documentation.xml" path="//Network/Common/RegisterType/*"/>
  public void RegisterType(Type type) { AssertClosed(); cvt.RegisterType(type); }
  /// <include file="documentation.xml" path="//Network/Common/UnregisterTypes/*"/>
  public void UnregisterTypes(params Type[] types) { AssertClosed(); cvt.UnregisterTypes(types); }
  /// <include file="documentation.xml" path="//Network/Common/UnregisterType/*"/>
  public void UnregisterType(Type type) { AssertClosed(); cvt.UnregisterType(type); }
  /// <include file="documentation.xml" path="//Network/Common/ClearTypes/*"/>
  public void ClearTypes() { AssertClosed(); cvt.ClearTypes(); }

  /// <summary>Stops listening for connections and deinitializes the server.</summary>
  public void Deinitialize()
  { if(thread!=null)
    { lock(this) foreach(ServerPlayer p in players) DropPlayer(p);
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
  public void Listen(int port) { Listen(new IPEndPoint(IPAddress.Any, port)); }

  /// <summary>Starts listening on the given endpoint.</summary>
  /// <param name="local">The <see cref="IPEndPoint"/> on which to listen for connections.</param>
  public void Listen(IPEndPoint local)
  { if(thread==null)
    { thread = new Thread(new ThreadStart(ThreadFunc));
      thread.Start();
    }
    lock(this)
    { StopListening();
      server = new TcpListener(local);
      server.Start();
      listening = true;
    }
  }

  /// <summary>Stops listening for connections without deinitializing the server.</summary>
  public void StopListening()
  { if(server!=null)
      lock(this)
      { listening = false;
        server.Stop();
        server = null;
      }
  }

  /// <summary>Gets information about the status of the receive and/or send queues for a given player.</summary>
  /// <param name="p">The <see cref="ServerPlayer"/> about which retrieve queue status information will be retrieved.</param>
  /// <param name="flags">A set of <see cref="QueueStat"/> flags that determine what data will be returned.</param>
  /// <returns>A <see cref="QueueStatus"/> structure containing the number and total size of messages waiting in
  /// the queues specified by <paramref name="flags"/>.
  /// </returns>
  public QueueStatus GetQueueStatus(ServerPlayer p, QueueStat flags)
  { lock(this) return p.Link.GetQueueStatus(flags);
  }

  /// <summary>Gets information about the status of the receive and/or send queues for all players combined.</summary>
  /// <param name="flags">A set of <see cref="QueueStat"/> flags that determine what data will be returned.</param>
  /// <returns>A <see cref="QueueStatus"/> structure containing the number and total size of messages waiting in
  /// the queues specified by <paramref name="flags"/>.
  /// </returns>
  public QueueStatus GetQueueStatus(QueueStat flags)
  { QueueStatus qs = new QueueStatus(), pqs;
    lock(this)
      foreach(NetLink link in links)
      { pqs = link.GetQueueStatus(flags);
        qs.SendBytes       += pqs.SendBytes;
        qs.SendMessages    += pqs.SendMessages;
        qs.ReceiveBytes    += pqs.ReceiveBytes;
        qs.ReceiveMessages += pqs.ReceiveMessages;
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
  { p.DelayedDrop = true;
    p.Link.Close();
  }

  /// <summary>Drops a player with a delay to allow incoming and outgoing messages to be sent.</summary>
  /// <param name="p">The <see cref="ServerPlayer"/> to drop.</param>
  /// <remarks>The connection with the player player will be terminated only after all outgoing messages have
  /// been sent and all incoming messages have been processed.
  /// </remarks>
  public void DropPlayerDelayed(ServerPlayer p) { DropPlayerDelayed(p, int.MaxValue); }

  /// <summary>Drops a player with a delay to allow incoming and outgoing messages to be sent.</summary>
  /// <param name="p">The <see cref="ServerPlayer"/> to drop.</param>
  /// <param name="timeoutMs">The amount of time to wait for all remaining messages to be sent, in milliseconds.</param>
  /// <remarks>The connection with the player player will be terminated after all outgoing messages have
  /// been sent and all incoming messages have been processed, or the timeout expires.
  /// </remarks>
  public void DropPlayerDelayed(ServerPlayer p, int timeoutMs)
  {
    if(timeoutMs < 0) throw new ArgumentOutOfRangeException("timeoutMs", "cannot be negative");
    p.DropDelay   = (uint)timeoutMs;
    p.DropStart   = Timing.Msecs;
    p.DelayedDrop = true;
  }

  /// <include file="documentation.xml" path="//Network/Server/Send/*[self::Common or self::byteData]/*"/>
  public void Send(object toWho, byte[] data) { Send(toWho, data, 0, data.Length, defFlags); }
  /// <include file="documentation.xml" path="//Network/Server/Send/*[self::Common or self::byteData or self::Length]/*"/>
  public void Send(object toWho, byte[] data, int length) { Send(toWho, data, 0, length, defFlags); }
  /// <include file="documentation.xml" path="//Network/Server/Send/*[self::Common or self::byteData or self::Length or self::Index]/*"/>
  public void Send(object toWho, byte[] data, int index, int length)
  { if(cvt.AltersByteArray) RawSend(toWho, cvt.Serialize(data, index, length), defFlags, 0, data);
    else RawSend(toWho, data, index, length, defFlags, 0, data);
  }
  /// <include file="documentation.xml" path="//Network/Server/Send/*[self::Common or self::byteData or self::Flags]/*"/>
  public void Send(object toWho, byte[] data, SendFlag flags) { Send(toWho, data, 0, data.Length, flags); }
  /// <include file="documentation.xml" path="//Network/Server/Send/*[self::Common or self::byteData or self::Length or self::Flags]/*"/>
  public void Send(object toWho, byte[] data, int length, SendFlag flags) { Send(toWho, data, 0, length, flags); }
  /// <include file="documentation.xml" path="//Network/Server/Send/*[self::Common or self::byteData or self::Length or self::Index or self::Flags]/*"/>
  public void Send(object toWho, byte[] data, int index, int length, SendFlag flags)
  { if(cvt.AltersByteArray) RawSend(toWho, cvt.Serialize(data, index, length), flags, 0, data);
    else RawSend(toWho, data, index, length, flags, 0, data);
  }
  /// <include file="documentation.xml" path="//Network/Server/Send/*[self::Common or self::byteData or self::Flags or self::Timeout]/*"/>
  public void Send(object toWho, byte[] data, SendFlag flags, int timeoutMs) { Send(toWho, data, 0, data.Length, flags, timeoutMs); }
  /// <include file="documentation.xml" path="//Network/Server/Send/*[self::Common or self::byteData or self::Length or self::Flags or self::Timeout]/*"/>
  public void Send(object toWho, byte[] data, int length, SendFlag flags, int timeoutMs) { Send(toWho, data, 0, length, flags, timeoutMs); }
  /// <include file="documentation.xml" path="//Network/Server/Send/*[self::Common or self::byteData or self::Index or self::Length or self::Flags or self::Timeout]/*"/>
  public void Send(object toWho, byte[] data, int index, int length, SendFlag flags, int timeoutMs)
  { if(cvt.AltersByteArray) RawSend(toWho, cvt.Serialize(data, index, length), flags, timeoutMs, data);
    else RawSend(toWho, data, index, length, flags, timeoutMs, data);
  }
  /// <include file="documentation.xml" path="//Network/Server/Send/*[self::Common or self::objData]/*"/>
  public void Send(object toWho, object data) { RawSend(toWho, cvt.Serialize(data), defFlags, 0, data); }
  /// <include file="documentation.xml" path="//Network/Server/Send/*[self::Common or self::objData or self::Flags]/*"/>
  public void Send(object toWho, object data, SendFlag flags) { RawSend(toWho, cvt.Serialize(data), flags, 0, data); }
  /// <include file="documentation.xml" path="//Network/Server/Send/*[self::Common or self::objData or self::Flags or self::Timeout]/*"/>
  public void Send(object toWho, object data, SendFlag flags, int timeoutMs)
  { RawSend(toWho, cvt.Serialize(data), flags, timeoutMs, data);
  }

  /// <summary>Gets the <see cref="MessageConverter"/> used to serialize/deserialize messages.</summary>
  /// <remarks>This is most commonly used in conjunction with <see cref="RawSend"/> to serialize a complex message
  /// once and then send it multiple times.
  /// </remarks>
  protected MessageConverter Converter { get { return cvt; } }

  /// <include file="documentation.xml" path="//Network/Server/Send/*[self::Common or self::RawSend or self::Flags or self::Timeout]/*"/>
  protected void RawSend(object toWho, byte[] data, SendFlag flags, int timeoutMs, object orig)
  { RawSend(toWho, data, 0, data.Length, flags, timeoutMs, orig);
  }
  /// <include file="documentation.xml" path="//Network/Server/Send/*[self::Common or self::RawSend or self::Index or self::Length or self::Flags or self::Timeout]/*"/>
  protected void RawSend(object toWho, byte[] data, int index, int length, SendFlag flags, int timeoutMs, object orig)
  {
    if(timeoutMs < 0) throw new ArgumentOutOfRangeException();
    if((object)data!=orig) flags |= SendFlag.NoCopy;
    if(toWho==null || toWho==Players)
      lock(this)
        foreach(ServerPlayer p in players)
        { if(!p.DelayedDrop)
            try { p.Link.Send(data, index, length, flags, timeoutMs, new DualTag(p, orig)); }
            catch(ConnectionLostException) { }
        }
    else if(toWho is ServerPlayer)
    { ServerPlayer p = (ServerPlayer)toWho;
      if(p.DelayedDrop)
        throw new ArgumentException("The player is currently being dropped, so sending data to it is not allowed.");
      p.Link.Send(data, index, length, flags, timeoutMs, new DualTag(p, orig));
    }
    else if(toWho is IEnumerable)
      foreach(ServerPlayer p in (IEnumerable)toWho)
      { if(p.DelayedDrop)
          throw new ArgumentException("The player is currently being dropped, so sending data to it is not allowed.");
        try { p.Link.Send(data, index, length, flags, timeoutMs, new DualTag(p, orig)); }
        catch(ConnectionLostException) { }
      }
    else throw new ArgumentException("Unknown destination type: "+toWho.GetType(), "toWho");
  }

  /// <summary>Raises the <see cref="MessageReceived"/> event.</summary>
  /// <param name="player">The <see cref="ServerPlayer"/> that sent the message.</param>
  /// <param name="message">The message that was received.</param>
  /// <remarks>If you override this method, be sure to call the base class' implementation. The proper place
  /// to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnMessageReceived(ServerPlayer player, object message)
  { ServerMessageHandler smh = MessageReceived;
    if(smh!=null) smh(this, player, message);
  }

  /// <summary>Raises the <see cref="MessageSent"/> event.</summary>
  /// <param name="player">The <see cref="ServerPlayer "/>to which the message was sent.</param>
  /// <param name="message">The message that was sent.</param>
  /// <remarks>This method is only called for messages that have requested notification via the
  /// <see cref="SendFlag.NotifySent"/> flag. If you override this method, be sure to call the base class'
  /// implementation. The proper place to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnMessageSent(ServerPlayer player, object message)
  { ServerMessageHandler smh = MessageSent;
    if(smh!=null) smh(this, player, message);
  }

  /// <summary>Raises the <see cref="PlayerConnecting"/> event.</summary>
  /// <param name="player">The <see cref="ServerPlayer"/> that is connecting.</param>
  /// <returns>True if the player should be allowed to connect, and false if the player should be dropped.</returns>
  /// <remarks>If you override this method, be sure to call the base class' implementation. The proper place
  /// to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual bool OnPlayerConnecting(ServerPlayer player)
  { PlayerConnectingHandler pch = PlayerConnecting;
    if(pch!=null)
      foreach(PlayerConnectingHandler eh in pch.GetInvocationList())
        if(!eh(this, player)) return false;
    return true;
  }

  /// <summary>Raises the <see cref="PlayerConnected"/> event.</summary>
  /// <param name="player">The <see cref="ServerPlayer"/> that has connected.</param>
  /// <remarks>If you override this method, be sure to call the base class' implementation. The proper place
  /// to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnPlayerConnected(ServerPlayer player)
  { PlayerConnectedHandler pch = PlayerConnected;
    if(pch!=null) pch(this, player);
  }

  /// <summary>Raises the <see cref="PlayerDisconnected"/> event.</summary>
  /// <param name="player">The <see cref="ServerPlayer"/> that has disconnected.</param>
  /// <remarks>This method is called for both planned and unplanned disconnections.
  /// If you override this method, be sure to call the base class' implementation. The proper place
  /// to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnPlayerDisconnected(ServerPlayer player)
  { PlayerDisconnectHandler pdh = PlayerDisconnected;
    if(pdh!=null) pdh(this, player);
  }

  /// <summary>Raises the <see cref="RemoteReceived"/> event.</summary>
  /// <param name="player">The <see cref="ServerPlayer"/> that has received the message.</param>
  /// <param name="message">The message that was received.</param>
  /// <remarks>This method is only called for messages that have requested notification via the
  /// <see cref="SendFlag.NotifyReceived"/> flag. If you override this method, be sure to call the base class'
  /// implementation. The proper place to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnRemoteReceived(ServerPlayer player, object message)
  { ServerMessageHandler smh = RemoteReceived;
    if(smh==null) smh(this, player, message);
  }

  struct DualTag
  { public DualTag(object tag1, object tag2) { Tag1=tag1; Tag2=tag2; }
    public object Tag1, Tag2;
  }

  void AssertOpen()   { if(thread==null) throw new InvalidOperationException("Server not initialized yet."); }
  void AssertClosed() { if(thread!=null) throw new InvalidOperationException("Server already initialized."); }

  void OnRemoteReceived(NetLink link, LinkMessage msg)
  { DualTag tag = (DualTag)msg.Tag;
    OnRemoteReceived((ServerPlayer)tag.Tag1, tag.Tag2);
  }

  void OnMessageSent(NetLink link, LinkMessage msg)
  { DualTag tag = (DualTag)msg.Tag;
    OnMessageSent((ServerPlayer)tag.Tag1, tag.Tag2);
  }

  void ThreadFunc()
  { try
    { List<ServerPlayer> disconnected = new List<ServerPlayer>();
      while(!quit)
      { bool did = false;

        lock(this)
        { while(listening && server.Pending())
          { Socket sock = server.AcceptSocket();
            try
            { ServerPlayer p = new ServerPlayer(new NetLink(sock), nextID++);
              if(!quit && OnPlayerConnecting(p))
              { p.Link.LagAverage      = lagAverage;
                p.Link.LagVariance     = lagVariance;
                p.Link.MessageSent    += new LinkMessageHandler(OnMessageSent);
                p.Link.RemoteReceived += new LinkMessageHandler(OnRemoteReceived);
                players.Add(p);
                links.Add(p.Link);
                p.Link.Send(new byte[0], SendFlag.Reliable);
                OnPlayerConnected(p);
              }
              else sock.Close();
            }
            catch(SocketException) { sock.Close(); }
            catch(HandshakeException) { sock.Close(); }
          }

          for(int i=0; i<players.Count; i++)
          { ServerPlayer p = players[i];
            try
            { if(p.DelayedDrop && Timing.Msecs-p.DropStart>=p.DropDelay) p.Link.Close();

              p.Link.SendPoll(); // ReceivePoll() is called by ReceiveMessage() as necessary
              LinkMessage msg = p.Link.ReceiveMessage();
              if(msg!=null)
              { did=true;
                OnMessageReceived(p, cvt.Deserialize(msg));
              }
              else
              { if(p.DelayedDrop && p.Link.sendQueue==0) p.Link.Close();
                if(!p.Link.IsConnected)
                { did=true;
                  players.RemoveAt(i);
                  links.RemoveAt(i);
                  i--;
                  disconnected.Add(p);
                }
              }
            }
            catch(SocketException) { }
          }

          if(disconnected!=null)
          { foreach(ServerPlayer p in disconnected) OnPlayerDisconnected(p);
            disconnected.Clear();
          }
        }

        if(!did) NetLink.WaitForEvent(links, 250);
      }
    }
    catch(Exception e)
    { try
      { if(Events.Events.Initialized)
          Events.Events.PushEvent(new Events.ExceptionEvent(Events.ExceptionLocation.NetworkThread, e));
      }
      catch { throw; }
    }
  }

  List<ServerPlayer> players = new List<ServerPlayer>();
  List<NetLink>      links   = new List<NetLink>();
  MessageConverter   cvt = new MessageConverter();
  TcpListener        server;
  Thread             thread;
  SendFlag           defFlags = SendFlag.ReliableSequential;
  int                nextID=1, lagAverage, lagVariance;
  bool               quit, listening;
}
#endregion

#region Client class and supporting types
/// <summary>This delegate is used for the <see cref="Client.MessageReceived"/>, <see cref="Client.RemoteReceived"/>,
/// and <see cref="Client.MessageSent"/> events. It receives a reference to the <see cref="Client"/> class and the
/// message that was sent or received.
/// </summary>
public delegate void ClientMessageHandler(Client client, object msg);
/// <summary>This delegate is used for the <see cref="Client.Connected"/> and <see cref="Client.Disconnected"/>
/// events.
/// </summary>
public delegate void ClientHandler(Client client);

public class Client
{ 
  /// <summary>Initializes a new instance of the <see cref="Client"/> class.</summary>
  public Client() { }
  /// <summary>Initializes a new instance of the <see cref="Client"/> class and connects to the given host and port.</summary>
  /// <param name="hostname">The name of the host to connect to.</param>
  /// <param name="port">The port to connect to.</param>
  public Client(string hostname, int port) { Connect(hostname, port); }
  /// <summary>Initializes a new instance of the <see cref="Client"/> class and connects to the given endpoint.</summary>
  /// <param name="remote">The <see cref="IPEndPoint"/> to connect to.</param>
  public Client(IPEndPoint remote) { Connect(remote); }
  ~Client() { Disconnect(); }

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

  /// <include file="documentation.xml" path="//Network/Common/DefaultFlags/*"/>
  public SendFlag DefaultFlags { get { return defFlags; } set { defFlags=value; } }

  /// <include file="documentation.xml" path="//Network/Common/IsConnected/*"/>
  public bool IsConnected
  { get
    { NetLink link = this.link;
      return link==null ? false : link.IsConnected;
    }
  }

  /// <include file="documentation.xml" path="//Network/Common/LagAverage/*"/>
  public int LagAverage
  { get { return AssertLink().LagAverage; }
    set { AssertLink().LagAverage=value; }
  }
  /// <include file="documentation.xml" path="//Network/Common/LagVariance/*"/>
  public int LagVariance
  { get { return AssertLink().LagVariance; }
    set { AssertLink().LagVariance=value; }
  }

  /// <summary>Returns the remote endpoint to which the client is connected.</summary>
  public IPEndPoint RemoteEndPoint { get { return AssertLink().RemoteEndPoint; } }

  /// <summary>Connects to the given host and port.</summary>
  /// <param name="hostname">The name of the host to connect to.</param>
  /// <param name="port">The name of the port to connect to.</param>
  public void Connect(string hostname, int port)
  { Connect(new IPEndPoint(Dns.GetHostEntry(hostname).AddressList[0], port));
  }

  /// <summary>Connects to the given remote endpoint.</summary>
  /// <param name="remote">The <see cref="IPEndPoint"/> to connect to.</param>
  public void Connect(IPEndPoint remote)
  { Disconnect();
    quit = delayedDrop = false;
    link = new NetLink(remote);

    byte[] msg = link.Receive(5000);
    if(msg==null)
    { Disconnect();
      throw new NetworkException("Timed out while waiting for handshake packet.");
    }

    link.MessageSent    += new LinkMessageHandler(OnMessageSent);
    link.RemoteReceived += new LinkMessageHandler(OnRemoteReceived);
    thread = new Thread(new ThreadStart(ThreadFunc));
    thread.Start();
    
    OnConnected();
  }

  /// <summary>Disconnects from the server with a delay to allow incoming and outgoing messages to be sent.</summary>
  /// <remarks>The connection will be terminated only after all outgoing messages have been sent and all incoming
  /// messages have been processed.
  /// </remarks>
  public void DelayedDisconnect() { DelayedDisconnect(int.MaxValue); }

  /// <summary>Disconnects from the server with a delay to allow incoming and outgoing messages to be sent.</summary>
  /// <param name="timeoutMs">The amount of time to wait for all remaining messages to be sent, in milliseconds.</param>
  /// <remarks>The connection will be terminated after all outgoing messages have been sent and all incoming
  /// messages have been processed, or the timeout expires.
  /// </remarks>
  public void DelayedDisconnect(int timeoutMs)
  {
    if(timeoutMs < 0) throw new ArgumentOutOfRangeException("timeoutMs", "Cannot be negative");
    dropDelay   = (uint)timeoutMs;
    dropStart   = Timing.Msecs;
    delayedDrop = true;
  }

  /// <summary>Disconnects from the server.</summary>
  /// <remarks>This method immediately terminates the connection with the server.</remarks>
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

  /// <include file="documentation.xml" path="//Network/Common/RegisterTypes/*"/>
  public void RegisterTypes(params Type[] types) { AssertClosed(); cvt.RegisterTypes(types); }
  /// <include file="documentation.xml" path="//Network/Common/RegisterType/*"/>
  public void RegisterType(Type type) { AssertClosed(); cvt.RegisterType(type); }
  /// <include file="documentation.xml" path="//Network/Common/UnregisterTypes/*"/>
  public void UnregisterTypes(params Type[] types) { AssertClosed(); cvt.UnregisterTypes(types); }
  /// <include file="documentation.xml" path="//Network/Common/UnregisterType/*"/>
  public void UnregisterType(Type type) { AssertClosed(); cvt.UnregisterType(type); }
  /// <include file="documentation.xml" path="//Network/Common/ClearTypes/*"/>
  public void ClearTypes() { AssertClosed(); cvt.ClearTypes(); }

  /// <include file="documentation.xml" path="//Network/Common/GetQueueStatus/*"/>
  public QueueStatus GetQueueStatus(QueueStat flags) { AssertLink(); return link.GetQueueStatus(flags); }

  /// <include file="documentation.xml" path="//Network/Client/Send/*[self::Common or self::byteData]/*"/>
  public void Send(byte[] data) { Send(data, 0, data.Length, defFlags); }
  /// <include file="documentation.xml" path="//Network/Client/Send/*[self::Common or self::byteData or self::Length]/*"/>
  public void Send(byte[] data, int length) { Send(data, 0, length, defFlags); }
  /// <include file="documentation.xml" path="//Network/Client/Send/*[self::Common or self::byteData or self::Length or self::Index]/*"/>
  public void Send(byte[] data, int index, int length)
  { AssertLink();
    if(cvt.AltersByteArray) DoSend(cvt.Serialize(data, index, length), defFlags, 0, data);
    else DoSend(data, index, length, defFlags, 0, data);
  }
  /// <include file="documentation.xml" path="//Network/Client/Send/*[self::Common or self::byteData or self::Flags]/*"/>
  public void Send(byte[] data, SendFlag flags) { Send(data, 0, data.Length, flags); }
  /// <include file="documentation.xml" path="//Network/Client/Send/*[self::Common or self::byteData or self::Length or self::Flags]/*"/>
  public void Send(byte[] data, int length, SendFlag flags) { Send(data, 0, length, flags); }
  /// <include file="documentation.xml" path="//Network/Client/Send/*[self::Common or self::byteData or self::Index or self::Length or self::Flags]/*"/>
  public void Send(byte[] data, int index, int length, SendFlag flags)
  { AssertLink();
    if(cvt.AltersByteArray) DoSend(cvt.Serialize(data, index, length), flags, 0, data);
    else DoSend(data, index, length, flags, 0, data);
  }
  /// <include file="documentation.xml" path="//Network/Client/Send/*[self::Common or self::byteData or self::Flags or self::Timeout]/*"/>
  public void Send(byte[] data, SendFlag flags, int timeoutMs) { Send(data, 0, data.Length, flags, timeoutMs); }
  /// <include file="documentation.xml" path="//Network/Client/Send/*[self::Common or self::byteData or self::Length or self::Flags or self::Timeout]/*"/>
  public void Send(byte[] data, int length, SendFlag flags, int timeoutMs) { Send(data, 0, length, flags, timeoutMs); }
  /// <include file="documentation.xml" path="//Network/Client/Send/*[self::Common or self::byteData or self::Index or self::Length or self::Flags or self::Timeout]/*"/>
  public void Send(byte[] data, int index, int length, SendFlag flags, int timeoutMs)
  { AssertLink();
    if(cvt.AltersByteArray) DoSend(cvt.Serialize(data, index, length), flags, timeoutMs, data);
    else DoSend(data, index, length, flags, timeoutMs, data);
  }
  /// <include file="documentation.xml" path="//Network/Client/Send/*[self::Common or self::objData]/*"/>
  public void Send(object data)
  { AssertLink();
    DoSend(cvt.Serialize(data), defFlags, 0, data);
  }
  /// <include file="documentation.xml" path="//Network/Client/Send/*[self::Common or self::objData or self::Flags]/*"/>
  public void Send(object data, SendFlag flags)
  { AssertLink();
    DoSend(cvt.Serialize(data), flags, 0, data);
  }
  /// <include file="documentation.xml" path="//Network/Client/Send/*[self::Common or self::objData or self::Flags or self::Timeout]/*"/>
  public void Send(object data, SendFlag flags, int timeoutMs)
  { AssertLink();
    DoSend(cvt.Serialize(data), flags, timeoutMs, data);
  }

  /// <summary>Raises the <see cref="Connected"/> event.</summary>
  /// <remarks>If you override this method, be sure to call the base class' implementation. The proper place
  /// to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnConnected()
  { if(Connected!=null) Connected(this);
  }

  /// <summary>Raises the <see cref="Disconnected"/> event.</summary>
  /// <remarks>If you override this method, be sure to call the base class' implementation. The proper place
  /// to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnDisconnected()
  { if(Disconnected!=null) Disconnected(this);
  }

  /// <summary>Raises the <see cref="MessageReceived"/> event.</summary>
  /// <param name="message">The message that was received.</param>
  /// <remarks>If you override this method, be sure to call the base class' implementation. The proper place
  /// to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnMessageReceived(object message)
  { ClientMessageHandler cmh = MessageReceived;
    if(cmh!=null) cmh(this, message);
  }

  /// <summary>Raises the <see cref="MessageSent"/> event.</summary>
  /// <param name="message">The message that was sent.</param>
  /// <remarks>This method is only called for messages that have requested notification via the
  /// <see cref="SendFlag.NotifySent"/> flag. If you override this method, be sure to call the base class'
  /// implementation. The proper place to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnMessageSent(object message)
  { ClientMessageHandler cmh = MessageSent;
    if(cmh!=null) cmh(this, message);
  }

  /// <summary>Raises the <see cref="RemoteReceived"/> event.</summary>
  /// <param name="message">The message that was received.</param>
  /// <remarks>This method is only called for messages that have requested notification via the
  /// <see cref="SendFlag.NotifyReceived"/> flag. If you override this method, be sure to call the base class'
  /// implementation. The proper place to do this is at the beginning of the derived version.
  /// </remarks>
  protected virtual void OnRemoteReceived(object message)
  { ClientMessageHandler cmh = RemoteReceived;
    if(cmh!=null) cmh(this, message);
  }

  void DoSend(byte[] data, SendFlag flags, int timeoutMs, object orig)
  { if((object)data!=orig) flags |= SendFlag.NoCopy;
    link.Send(data, 0, data.Length, flags, timeoutMs, orig);
  }
  void DoSend(byte[] data, int index, int length, SendFlag flags, int timeoutMs, object orig)
  { if((object)data!=orig) flags |= SendFlag.NoCopy;
    link.Send(data, index, length, flags, timeoutMs, orig);
  }

  NetLink AssertLink()
  { NetLink link = this.link;
    if(link==null) throw new InvalidOperationException("Client is not connected");
    return link;
  }

  void AssertClosed() { if(link!=null) throw new InvalidOperationException("Client is already connected"); }

  void ThreadFunc()
  { try
    { NetLink[] links = new NetLink[] { link };
      while(!quit)
      { bool did=false;
        try
        { if(delayedDrop && Timing.Msecs-dropStart>=dropDelay) link.Close();

          link.SendPoll(); // ReceivePoll() is called by ReceiveMessage as necessary
          LinkMessage msg = link.ReceiveMessage();
          if(msg!=null)
          { did=true;
            OnMessageReceived(cvt.Deserialize(msg));
          }
          else
          { if(delayedDrop && link.sendQueue==0) link.Close();
            if(!link.IsConnected)
            { link = null;
              OnDisconnected();
              break;
            }
          }
        }
        catch(SocketException) { }

        if(!did) NetLink.WaitForEvent(links, 250);
      }
    }
    catch(Exception e)
    { try
      { if(Events.Events.Initialized)
          Events.Events.PushEvent(new Events.ExceptionEvent(Events.ExceptionLocation.NetworkThread, e));
      }
      catch { throw; }
    }
  }

  void OnMessageSent(NetLink link, LinkMessage msg) { OnMessageSent(msg.Tag); }
  void OnRemoteReceived(NetLink link, LinkMessage msg) { OnRemoteReceived(msg.Tag); }

  MessageConverter cvt = new MessageConverter();
  NetLink  link;
  Thread   thread;
  uint     dropStart, dropDelay;
  SendFlag defFlags = SendFlag.ReliableSequential;
  bool     quit, delayedDrop;
}
#endregion

} // namespace GameLib.Network