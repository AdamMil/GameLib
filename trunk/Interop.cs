using System;
using System.Runtime.InteropServices;

namespace GameLib.Interop
{

// HACK: C# doesn't support changing the calling convention of a delegate
[Serializable, AttributeUsage(AttributeTargets.Delegate)]
internal sealed class CallConvCdeclAttribute : Attribute { }

// HACK: get a function pointer for a delegate
[StructLayout(LayoutKind.Explicit, Size=4)]
public sealed class DelegateMarshaller
{ public DelegateMarshaller(Delegate func) { this.func=func; }
  public unsafe IntPtr ToIntPtr()  { IntPtr ptr; Marshal.StructureToPtr(this, new IntPtr(&ptr), false); return ptr; }
  public unsafe void*  ToPointer() { void* ptr; Marshal.StructureToPtr(this, new IntPtr(&ptr), false); return ptr; }
  [MarshalAs(UnmanagedType.FunctionPtr),FieldOffset(0)] Delegate func;
}

public sealed class Unsafe
{ public static unsafe void Copy(byte* dest, byte* src, int length)
  { if(src==null || dest==null) throw new ArgumentNullException();
    int i, len=length/4;
    for(i=0; i<len; src+=4,dest+=4,i++) *((int*)dest) = *((int*)src);
    for(i=0,len=length&3; i<len; i++) *dest++ = *src++;
  }
}

internal abstract class StreamCallbackSource
{ protected StreamCallbackSource(System.IO.Stream stream, bool autoClose)
  { if(stream==null) throw new ArgumentNullException("stream");
    if(!stream.CanRead) throw new ArgumentException("stream must be readable", "stream");
    this.stream=stream; this.autoClose=autoClose;
    if(!autoClose) GC.SuppressFinalize(this);
  }
  ~StreamCallbackSource() { Close(); }

  public virtual void Close()
  { if(stream!=null) { stream.Close(); stream=null; }
    buffer = null;
  }

  protected enum SeekType { Absolute, Relative, FromEnd }
  protected long Seek(long offset, SeekType type)
  { if(stream==null) return -1;
    long pos=-1;
    switch(type)
    { case SeekType.Absolute: pos = stream.Seek(offset, System.IO.SeekOrigin.Begin); break;
      case SeekType.Relative: pos = stream.Seek(offset, System.IO.SeekOrigin.Current); break;
      case SeekType.FromEnd:  pos = stream.Seek(offset, System.IO.SeekOrigin.End); break;
    }
    return pos;
  }
  
  protected long Tell() { return stream.CanSeek ? stream.Position : -1; }

  protected unsafe long Read(byte* data, long size, long maxnum)
  { if(stream==null) return -1;
    if(size<=0 || maxnum<=0) return 0;
    if(size==1)
    { try
      { int len = (int)maxnum, read;
        if(buffer==null || buffer.Length<len) buffer = new byte[len];
        read = stream.Read(buffer, 0, (int)maxnum);
        fixed(byte* src = buffer) Unsafe.Copy(data, src, read);
        return read;
      }
      catch { return -1; }
    }
    else
    { long i=0, read;
      int  j, jlen=(int)size;
      if(buffer==null || buffer.Length<jlen) buffer = new byte[jlen];

      try
      { fixed(byte* src=buffer)
          for(; i<maxnum; i++)
          { read = stream.Read(buffer, 0, jlen);
            if(read!=size)
            { if(stream.CanSeek) stream.Position-=read;
              return i==0 ? -1 : i;
            }
            if(size>8) Unsafe.Copy(data, src, jlen);
            else for(j=0; j<jlen; j++) *data=src[j];
            data += jlen;
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
          if(len>8) Unsafe.Copy(dest, data, len);
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
    if(autoClose) try { Close(); } catch { }
    GC.SuppressFinalize(this);
  }

  protected byte[] buffer;
  protected System.IO.Stream stream;
  protected bool autoClose;
}

}