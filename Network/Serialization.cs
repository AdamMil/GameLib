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

using System.IO;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using AdamMil.IO;

namespace GameLib.Network
{

#region INetSerializable
/// <summary>This interface allows an object to customize the serialization process.</summary>
/// <remarks>
/// It is only necessary to implement this interface for classes containing fields that can't be marshaled, but any
/// class or struct can implement it if it wants to customize the serialization process. This interface is used rather
/// than the standard .NET serialization interface because the .NET serialization interface, while more flexible, has
/// comparatively low performance. In addition to implementing the methods in this interface, the object must have a
/// default constructor.
/// </remarks>
public interface INetSerializable
{
  /// <summary>Calculates the size in bytes of the serialized message data, not including the size of the attached
  /// stream, if any.
  /// </summary>
  int SizeOf();

  /// <summary>Serializes an object into a byte array.</summary>
  /// <param name="buf">The byte array into which the object should be serialized.</param>
  /// <param name="index">The index into <paramref name="buf"/> at which data should be written.</param>
  /// <param name="attachedStream">A variable that can be set to an instance of a stream if the stream should be
  /// attached to the network message. This stream will be closed by the system. If you don't want that to happen, wrap
  /// the stream with a <see cref="ReferenceCountedStream"/>, with a reference count greater than one.
  /// </param>
  /// <remarks>This method will be called by the serialization engine to serialize an object. The only assumption
  /// that can be made about <paramref name="buf"/> is that there are at least N bytes at <paramref name="index"/>
  /// than can be written, where N is the value obtained by calling <see cref="SizeOf"/>.
  /// </remarks>
  void SerializeTo(byte[] buf, int index, out Stream attachedStream);

  /// <summary>Deserializes an object from a byte array.</summary>
  /// <param name="buf">The byte array from which the object should be deserialized.</param>
  /// <param name="index">The index into <paramref name="buf"/> from which data should be read.</param>
  /// <param name="attachedStream">A reference to the stream that was attached to the message, or null if no stream was
  /// attached. You are responsible for closing the stream if it's not null.
  /// </param>
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
  int DeserializeFrom(byte[] buf, int index, Stream attachedStream);
}
#endregion

#region MessageConverter
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
  public bool AltersByteArray
  {
    get { return typeIDs.Count != 0; }
  }

  /// <summary>Unregisters all registered types.</summary>
  public void ClearTypes()
  {
    types.Clear();
    typeIDs.Clear();
  }

  /// <summary>Registers a list of types.</summary>
  /// <param name="types">An array of <see cref="System.Type"/> holding types to be registered.</param>
  /// <include file="../documentation.xml" path="//Network/MessageConverter/RegisterType/*"/>
  public void RegisterTypes(params Type[] types)
  {
    RegisterTypes((IEnumerable<Type>)types);
  }

  /// <summary>Registers a list of types.</summary>
  /// <param name="types">An collection of <see cref="System.Type"/> holding types to be registered.</param>
  /// <include file="../documentation.xml" path="//Network/MessageConverter/RegisterType/*"/>
  public void RegisterTypes(IEnumerable<Type> types)
  {
    foreach(Type type in types) RegisterType(type);
  }

  /// <summary>Registers all non-abstract types in the assembly that equal to or derived from the given base type.</summary>
  /// <include file="../documentation.xml" path="//Network/MessageConverter/RegisterType/*"/>
  public void RegisterTypes(Assembly assembly, Type baseType)
  {
    if(assembly == null || baseType == null) throw new ArgumentNullException();

    List<Type> types = new List<Type>();
    foreach(Type type in assembly.GetTypes())
    {
      if(!type.IsAbstract && (type == baseType || type.IsSubclassOf(baseType))) types.Add(type);
    }
    types.Sort(delegate(Type a, Type b) { return string.Compare(a.FullName, b.FullName, StringComparison.Ordinal); });
    RegisterTypes(types);
  }

  /// <summary>Registers a given type, allowing it to be serialized and deserialized by this class.</summary>
  /// <param name="type">The <see cref="System.Type"/> to be registered.</param>
  /// <include file="../documentation.xml" path="//Network/MessageConverter/RegisterType/*"/>
  public void RegisterType(Type type)
  {
    if(type == null) throw new ArgumentNullException("type");
    if(typeIDs.ContainsKey(type)) throw new ArgumentException(type.FullName + " has already been registered.");
    if(type.IsArray)
    {
      throw new ArgumentException("Arrays types cannot be registered directly. Instead, register the element type of "+
                                  "the array. For instance, if you want to send an array of System.Double, register "+
                                  "the System.Double type.");
    }
    if(type.HasElementType) throw new ArgumentException("Pointer types cannot be registered.");

    TypeInfo info;
    if(typeof(INetSerializable).IsAssignableFrom(type))
    {
      ConstructorInfo cons = type.GetConstructor(Type.EmptyTypes);
      if(cons == null) throw new ArgumentException(type.FullName + " has no default constructor.");
      info = new TypeInfo(type, cons);
    }
    else
    {
      info = new TypeInfo(type, null);
      info.Size = Marshal.SizeOf(type);

      MarshalType mt = GetMarshalType(type);
      if(mt == MarshalType.Unmarshalable)
      {
        throw new ArgumentException("Types that cannot be marshaled cannot be serialized unless they implement the " +
                                    "INetSerializable interface or use MarshalAsAttribute to make all their fields " +
                                    "safely marshalable.");
      }
      info.Blittable = (mt == MarshalType.Blittable);
    }

    // find a new place 
    typeIDs[type] = types.Count;
    types.Add(info);
  }

  /// <summary>Deserializes an object contained in a <see cref="LinkMessage"/>.</summary>
  /// <param name="msg">The <see cref="LinkMessage"/> holding the data to be deserialized.</param>
  /// <returns>An object by created by deserializing the data from the message.</returns>
  public object Deserialize(LinkMessage msg)
  {
    return Deserialize(msg.Data, msg.Index, msg.Length, msg.AttachedStream);
  }

  /// <summary>Deserializes an object contained in an array.</summary>
  /// <param name="data">The array of <see cref="System.Byte"/> from where the object will be deserialized.</param>
  /// <param name="index">The index into <paramref name="data"/> at which the object data begins.</param>
  /// <param name="length">The length of the object data to be used for deserialization.</param>
  /// <param name="attachedStream">The stream which was attached to the message data.</param>
  /// <returns>An object by created by deserializing the data from the array.</returns>
  public unsafe object Deserialize(byte[] data, int index, int length, Stream attachedStream)
  {
    if(types.Count == 0) // if no types are registered, the message must have been a byte array, since that's all that
    {                    // could be sent on the other end, assuming an identical MessageConverter
      if(length == data.Length)
      {
        return data;
      }
      else
      {
        byte[] shrunk = new byte[length];
        Array.Copy(data, index, shrunk, 0, length);
        return shrunk;
      }
    }
    else
    {
      uint id = IOH.ReadBE4U(data, index);
      index += HeaderSize;

      if(id == 0) // ID 0 is reserved for byte arrays
      {
        byte[] buf = new byte[length-HeaderSize];
        Array.Copy(data, index, buf, 0, length-HeaderSize);
        return buf;
      }

      bool isArray = (id & 0x80000000) != 0;
      if(isArray) id &= 0x7FFFFFFF;

      // 'id' is one greater than the actual index
      if(id > types.Count) throw new ArgumentException("Unregistered type id: " + id);
      TypeInfo info = (TypeInfo)types[(int)id-1];
      Type type = info.Type;

      if(isArray) // it's an array of objects
      {
        if(info.Constructor != null) // if the object implements INetSerializable
        {
          System.Collections.ArrayList objects = new System.Collections.ArrayList();
          while(index < data.Length)
          {
            INetSerializable ns = (INetSerializable)info.Constructor.Invoke(null);
            index += ns.DeserializeFrom(data, index, attachedStream);
            objects.Add(ns);
          }
          return objects.ToArray(type);
        }
        else // it's an array of objects that don't implement INetSerializable
        {
          Array array = Array.CreateInstance(type, (length-HeaderSize) / info.Size);
          if(array.Length != 0)
          {
            fixed(byte* src=data)
            {
              if(info.Blittable) // if it's a blittable type, we can copy the memory directly
              {
                GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                try
                {
                  IntPtr dest = Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
                  Interop.Unsafe.Copy(src+index, dest.ToPointer(), array.Length*info.Size);
                }
                finally { handle.Free(); }
              }
              else // it's not a blittable type, so we have to marshal each item individually
              {
                byte* ptr = src+index;
                for(int i=0; i<array.Length; ptr += info.Size, i++)
                {
                  array.SetValue(Marshal.PtrToStructure(new IntPtr(ptr), type), i);
                }
              }
            }
          }
          return array;
        }
      }
      else if(info.Constructor != null) // is a single object that implements INetSerializable
      {
        INetSerializable ns = (INetSerializable)info.Constructor.Invoke(null);
        ns.DeserializeFrom(data, index, attachedStream);
        return ns;
      }
      else // it's a single object that doesn't implement INetSerializable
      {
        fixed(byte* src=data) return Marshal.PtrToStructure(new IntPtr(src+index), type);
      }
    }
  }

  /// <summary>Serializes an array of <see cref="System.Byte"/>.</summary>
  /// <param name="data">An array of <see cref="System.Byte"/>.</param>
  /// <returns>An array that can later be passed to <see cref="Deserialize"/> to retrieve the original data.
  /// If no header information needs to be added to the original array, then the original array will be returned
  /// directly.
  /// </returns>
  public byte[] Serialize(byte[] data)
  {
    if(data == null) throw new ArgumentNullException();
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
  {
    if(data == null) throw new ArgumentNullException();
    if(index < 0 || length < 0 || index+length > data.Length) throw new ArgumentOutOfRangeException();

    byte[] ret;
    if(AltersByteArray)
    {
      ret = new byte[length + HeaderSize];
      Array.Copy(data, index, ret, HeaderSize, length);
    }
    else if(length == data.Length)
    {
      ret = data;
    }
    else
    {
      ret = new byte[length];
      Array.Copy(data, index, ret, 0, length);
    }
    return ret;
  }

  /// <summary>Serializes an object.</summary>
  /// <param name="obj">The object to serialize.</param>
  /// <param name="attachedStream">A stream that the object may require to be attached to the message data.</param>
  /// <returns>An array of <see cref="System.Byte"/> that can later be passed to <see cref="Deserialize"/> to
  /// reconstruct the original object.
  /// </returns>
  /// <remarks>If the object is not an array of <see cref="System.Byte"/>, the object's type must be registered
  /// (using <see cref="RegisterType"/>) before you can serialize it.
  /// </remarks>
  public unsafe byte[] Serialize(object obj, out Stream attachedStream)
  {
    if(obj == null) throw new ArgumentNullException();

    attachedStream = null;
    if(obj is byte[]) return Serialize((byte[])obj);

    if(typeIDs.Count == 0) throw new ArgumentException("If no types are registered, only byte[] can be sent.");

    Type type = obj.GetType();
    bool isArray = type.IsArray;

    if(isArray)
    {
      if(type.GetArrayRank() != 1)
      {
        throw new ArgumentException("Can't automatically serialize arrays with more than one dimension. Wrap them in "+
                                    "an object that implements INetSerializable.");
      }
      type = type.GetElementType();
    }

    int id;
    if(!typeIDs.TryGetValue(type, out id)) throw new ArgumentException(type.FullName + " is not a registered type.");
    TypeInfo info = types[id];
    byte[] ret;

    if(isArray)
    {
      Array array = (Array)obj;
      if(info.Constructor != null) // it's an array of objects that implement INetSerializable
      {
        int i=0, j=0;
        int* sizes = stackalloc int[array.Length]; // create an array to hold the sizes of the individual objects
        System.Collections.IEnumerator e = array.GetEnumerator();
        while(e.MoveNext())
        {
          sizes[i] = ((INetSerializable)e.Current).SizeOf();
          j += sizes[i++];
        }

        ret = new byte[j + HeaderSize]; // allocate the memory for the objects
        i = 0;
        j = HeaderSize;
        e.Reset();
        while(e.MoveNext())
        {
          ((INetSerializable)e.Current).SerializeTo(ret, j, out attachedStream);
          if(attachedStream != null)
          {
            throw new ArgumentException("Can't serialize an array of objects that use attached streams.");
          }
          j += sizes[i++];
        }
      }
      else // it's an array of marshalable objects
      {
        int size = info.Size;
        ret = new byte[array.Length*size + HeaderSize];

        if(array.Length != 0)
        {
          fixed(byte* dest=ret)
          {
            if(info.Blittable) // if it's a blittable type, we can copy the memory directly
            {
              GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
              try
              {
                IntPtr src = Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
                Interop.Unsafe.Copy(src.ToPointer(), dest+HeaderSize, ret.Length-HeaderSize);
              }
              finally { handle.Free(); }
            }
            else // it's a non-blittable type, so we need to marshal each object
            {
              byte* ptr = dest + HeaderSize;
              for(int i=0; i < array.Length; ptr += size, i++)
              {
                IntPtr iptr = new IntPtr(ptr);
                Marshal.StructureToPtr(array.GetValue(i), iptr, false);
                Marshal.DestroyStructure(iptr, type);
              }
            }
          }
        }
      }
    }
    else if(info.Constructor != null) // it's a single object that implements INetSerializable
    {
      INetSerializable ns = (INetSerializable)obj;
      ret = new byte[ns.SizeOf() + HeaderSize];
      ns.SerializeTo(ret, HeaderSize, out attachedStream);
    }
    else
    {
      ret = new byte[info.Size + HeaderSize];
      fixed(byte* dest=ret)
      {
        IntPtr iptr = new IntPtr(dest + HeaderSize);
        Marshal.StructureToPtr(obj, iptr, false);
        if(!info.Blittable) Marshal.DestroyStructure(iptr, type);
      }
    }

    id++; // the actual ID is one greater because ID 0 is reserved for byte arrays
    IOH.WriteBE4U(ret, 0, isArray ? (uint)id | 0x80000000 : (uint)id); // write the header
    return ret;
  }

  const int HeaderSize = 4;

  sealed class TypeInfo
  {
    public TypeInfo(Type type, ConstructorInfo cons) { Type=type; Constructor=cons; }
    public Type Type;
    public ConstructorInfo Constructor;
    public int Size;
    public bool Blittable;
  }

  enum MarshalType
  {
    Unmarshalable, Marshalable, Blittable
  };

  readonly List<TypeInfo> types = new List<TypeInfo>();
  readonly Dictionary<Type,int> typeIDs = new Dictionary<Type,int>();

  static MarshalType GetMarshalType(Type type)
  {
    // reference types without layout attributes are unmarshalable
    if(!type.IsValueType && !type.IsLayoutSequential && !type.IsExplicitLayout)
    {
      return MarshalType.Unmarshalable;
    }

    /*try { new ReflectionPermission(ReflectionPermissionFlag.NoFlags).Demand(); }
    catch(System.Security.SecurityException) { return MarshalType.Marshalable; }*/
    
    return GetMarshalType(type, null);
  }

  static MarshalType GetMarshalType(Type type, List<Type> typesSeen)
  {
    if(type.IsPrimitive || type.IsEnum) return MarshalType.Blittable; // primitive and enum types are all blittable

    MarshalType marshalType = MarshalType.Blittable;

    // the type is marshalable if each field is marshalable, so check each field
    foreach(FieldInfo fi in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
    {
      Type ft = fi.FieldType;
      
      if(ft.IsPrimitive || ft.IsEnum)
      {
        // primitive types are blittable and so don't change the marshal type
      }
      else
      {
        // reference fields without layout attributes are allowed if they have a suitable MarshalAs attribute
        if(!ft.IsValueType && !ft.IsLayoutSequential && !ft.IsExplicitLayout)
        { 
          MarshalAsAttribute ma = (MarshalAsAttribute)Attribute.GetCustomAttribute(fi, typeof(MarshalAsAttribute), false);
          if(ma != null)
          {
            switch(ma.Value)
            {
              case UnmanagedType.Bool: case UnmanagedType.ByValArray: case UnmanagedType.ByValTStr:
              case UnmanagedType.I1: case UnmanagedType.I2: case UnmanagedType.I4: case UnmanagedType.I8:
              case UnmanagedType.R4: case UnmanagedType.R8:
              case UnmanagedType.SysInt: case UnmanagedType.SysUInt:
              case UnmanagedType.U1: case UnmanagedType.U2: case UnmanagedType.U4: case UnmanagedType.U8:
              case UnmanagedType.VariantBool:
                marshalType = MarshalType.Marshalable;
                continue;
            }
          }

          // if a field has no MarshalAs attribute, or it's unsuitable, the type is unmarshalable
          marshalType = MarshalType.Unmarshalable;
          break;
        }

        // the field type is either a value type or a reference type with a layout attribute.
        // recursively examine its fields if we haven't seen it before
        if(typesSeen == null) typesSeen = new List<Type>();
        if(!typesSeen.Contains(ft))
        {
          typesSeen.Add(ft);
          MarshalType mt = GetMarshalType(ft, typesSeen);
          if(mt < marshalType) marshalType = mt;
          if(mt == MarshalType.Unmarshalable) break;
        }
      }
    }

    return marshalType;
  }
}
#endregion

} // namespace GameLib.Network