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

using System;
using System.Runtime.InteropServices;

namespace GameLib.Interop
{

/// <summary>This class allows you to get an unmanaged function pointer to a managed method.</summary>
/// <remarks>
/// <para>When interfacing with unmanaged code that uses callbacks, it is often necessary to get an unmanaged
/// function pointer to a managed method. This class allows you to do that. You'll want to keep an instance of this
/// class around as long as you need the callback.
/// </para>
/// <para>Take note of the expected calling
/// convention of the callback, however. The default calling convention for delegates may not be the same as is
/// expected by the unmanaged code. This is especially common under Microsoft .NET for Windows. It is possible to
/// alter the calling convention of a delegate, but no .NET languages support that as a standard feature yet.
/// In that case, you'll have to rely on hacking the MSIL code for your assembly as GameLib does in order to
/// interface with some libraries. Using this class improperly can result in stack corruption, application crashes,
/// and possibly system crashes too, so only use it if you know what you're doing!
/// </para>
/// </remarks>
// HACK: get a function pointer for a delegate
[StructLayout(LayoutKind.Explicit, Size=4)] // TODO: make this work on systems where a pointer is not 4 bytes
public sealed class DelegateMarshaller
{ 
  /// <summary>Initializes this class from a delegate.</summary>
  /// <param name="func">A delegate for which you want an unmanaged function pointer.</param>
  public DelegateMarshaller(Delegate func) { this.func=func; }
  /// <summary>Converts the delegate given to the constructor into an <see cref="IntPtr"/>.</summary>
  /// <returns>An <see cref="IntPtr"/> that holds an unmanaged function pointer to the delegate.</returns>
  public unsafe IntPtr ToIntPtr()  { IntPtr ptr; Marshal.StructureToPtr(this, new IntPtr(&ptr), false); return ptr; }
  /// <summary>Converts the delegate given to the constructor into a raw pointer.</summary>
  /// <returns>An unmanaged function pointer to the delegate.</returns>
  [CLSCompliant(false)]
  public unsafe void* ToPointer() { void* ptr; Marshal.StructureToPtr(this, new IntPtr(&ptr), false); return ptr; }

  [MarshalAs(UnmanagedType.FunctionPtr),FieldOffset(0)] Delegate func;
}

/// <summary>This class provides methods to help when working with unsafe code.</summary>
[CLSCompliant(false)]
public static class Unsafe
{ 
  /// <summary>This method copies a block of memory to another location.</summary>
  /// <param name="src">A pointer to the beginning of the source block of memory.</param>
  /// <param name="dest">The destination into which the source data will be copied.</param>
  /// <param name="length">The number of bytes to copy.</param>
  /// <remarks>This method does not handle the case where the regions of memory overlap. For that, use
  /// <see cref="SafeCopy"/>.
  /// </remarks>
  public static unsafe void Copy(void* src, void* dest, int length)
  { if(src==null || dest==null) throw new ArgumentNullException();
    if(length<0) throw new ArgumentOutOfRangeException("length", length, "must not be negative");
    GLUtility.Utility.MemCopy(src, dest, length);
  }
  /// <summary>This method fills a block of memory with a specified byte value.</summary>
  /// <param name="dest">The pointer to the memory region that will be filled.</param>
  /// <param name="value">The byte value with which the memory region will be filled.</param>
  /// <param name="length">The number of bytes to fill.</param>
  public static unsafe void Fill(void* dest, byte value, int length)
  { if(dest==null) throw new ArgumentNullException("dest");
    if(length<0) throw new ArgumentOutOfRangeException("length", length, "must not be negative");
    GLUtility.Utility.MemFill(dest, value, length);
  }
  /// <summary>This method copies a block of memory to another location.</summary>
  /// <param name="src">A pointer to the beginning of the source block of memory.</param>
  /// <param name="dest">The destination into which the source data will be copied.</param>
  /// <param name="length">The number of bytes to copy.</param>
  /// <remarks>This method correctly handles the case where the regions of memory overlap.</remarks>
  public static unsafe void SafeCopy(void* src, void* dest, int length)
  { if(src==null || dest==null) throw new ArgumentNullException();
    if(length<0) throw new ArgumentOutOfRangeException("length", length, "must not be negative");
    GLUtility.Utility.MemMove(src, dest, length);
  }
}

internal abstract class StreamCallbackSource : IDisposable
{ protected StreamCallbackSource(System.IO.Stream stream, bool autoClose)
  { if(stream==null) throw new ArgumentNullException("stream");
    if(!stream.CanRead) throw new ArgumentException("stream must be readable", "stream");
    this.stream=stream; this.autoClose=autoClose;
    if(!autoClose) GC.SuppressFinalize(this);
  }
  ~StreamCallbackSource() { Dispose(); }
  public void Dispose() { if(autoClose) Close(); GC.SuppressFinalize(this); }

  public virtual void Close()
  { if(stream!=null) { stream.Close(); stream=null; }
    buffer = null;
  }

  protected enum SeekType { Absolute, Relative, FromEnd }
  protected long Seek(long offset, SeekType type)
  { if(stream==null || !stream.CanSeek) return -1;
    long pos=-1;
    switch(type)
    { case SeekType.Absolute: pos = stream.Seek(offset, System.IO.SeekOrigin.Begin); break;
      case SeekType.Relative: pos = stream.Seek(offset, System.IO.SeekOrigin.Current); break;
      case SeekType.FromEnd:  pos = stream.Seek(offset, System.IO.SeekOrigin.End); break;
    }
    return pos;
  }

  protected long Tell() { return stream.CanSeek ? stream.Position : -1; }

  protected unsafe int Read(byte* data, int size, int maxnum)
  { if(stream==null) return -1;
    if(size<=0 || maxnum<=0) return 0;
    if(size==1)
    { try
      { int read;
        if(buffer==null || buffer.Length<maxnum) buffer = new byte[maxnum];
        read = stream.Read(buffer, 0, maxnum);
        fixed(byte* src = buffer) Unsafe.Copy(src, data, read);
        return read;
      }
      catch { return -1; }
    }
    else
    { int i=0, j, read;
      if(buffer==null || buffer.Length<size) buffer = new byte[size];

      try
      { fixed(byte* src=buffer)
          for(; i<maxnum; i++)
          { read = stream.Read(buffer, 0, size);
            if(read!=size)
            { if(stream.CanSeek) stream.Position-=read;
              return read==0 ? 0 : i==0 ? -1 : i;
            }
            if(size>8) Unsafe.Copy(src, data, size);
            else for(j=0; j<size; j++) data[j]=src[j];
            data += size;
          }
        return i;
      }
      catch { return i==0 ? -1 : i; }
    }
  }

  protected unsafe long Write(byte* data, long size, long num)
  { if(stream==null || !stream.CanWrite) return -1;
    if(size<=0 || num<=0) return 0;
    long total=size*num;
    int len = (int)Math.Min(total, 1024);
    try
    { byte[] buf = new byte[len];
      fixed(byte* dest = buf)
        do
        { if(total<len) len=(int)total;
          if(len>8) Unsafe.Copy(data, dest, len);
          else for(int i=0; i<len; i++) dest[i]=data[i];
          data += len;
          stream.Write(buf, 0, len);
          total -= len;
        } while(total>0);
      return size;
    }
    catch { return -1; }
  }

  protected int Truncate(long len)
  { if(stream==null) return -1;
    try { stream.SetLength(len); } catch { return -1; }
    return 0;
  }

  protected void MaybeClose()
  { if(stream==null) return;
    Dispose();
  }

  protected byte[] buffer;
  protected System.IO.Stream stream;
  protected bool autoClose;
}

} // namespace GameLib.Interop