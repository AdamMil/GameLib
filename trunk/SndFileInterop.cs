using System;
using System.IO;
using System.Runtime.InteropServices;

namespace GameLib.Interop.SndFile
{

[System.Security.SuppressUnmanagedCodeSecurity()]
internal class SF
{ 
  [CallConvCdecl] public unsafe delegate long SeekHandler(IntPtr context, long offset, SeekType type);
  [CallConvCdecl] public unsafe delegate long ReadHandler(IntPtr context, byte* data, long size, long maxnum);
  [CallConvCdecl] public unsafe delegate long WriteHandler(IntPtr context, byte* data, long size, long num);
  [CallConvCdecl] public unsafe delegate long TellHandler(IntPtr context);
  [CallConvCdecl] public unsafe delegate long GetsHandler(IntPtr context, byte* buffer, long bufsize);
  [CallConvCdecl] public unsafe delegate long LengthHandler(IntPtr context);
  [CallConvCdecl] public unsafe delegate int  TruncateHandler(IntPtr context, long len);
  [CallConvCdecl] public unsafe delegate int  CloseHandler(IntPtr context);

  #region Structs
  [StructLayout(LayoutKind.Sequential, Pack=4)]
  public unsafe struct IOCalls
  { public void* Seek, Read, Write, Tell, Gets, Length, Truncate, Close;
  }

  [StructLayout(LayoutKind.Sequential, Pack=4)]
  public struct Info
  {	public bool Has(Format f) { return (format&f)==f; }
    public long   frames;
	  public int    samplerate;
	  public int    channels;
	  public Format format;
	  public int    sections;
	  public int    seekable;
  }
  #endregion

  #region Enums
  public enum SeekType : int
  { Absolute, Relative, FromEnd
  }
  
  [Flags]
  public enum OpenMode : int
  { Read=0x10, Write=0x20, ReadWrite=Read|Write
  }

  public enum Format : int
  { /* Major formats. */
    WAV          = 0x010000,     /* Microsoft WAV format (little endian). */
    AIFF         = 0x020000,     /* Apple/SGI AIFF format (big endian). */
    AU           = 0x030000,     /* Sun/NeXT AU format (big endian). */
    RAW          = 0x040000,     /* RAW PCM data. */
    PAF          = 0x050000,     /* Ensoniq PARIS file format. */
    SVX          = 0x060000,     /* Amiga IFF / SVX8 / SV16 format. */
    NIST         = 0x070000,     /* Sphere NIST format. */
    VOC          = 0x080000,     /* VOC files. */
    IRCAM        = 0x0A0000,     /* Berkeley/IRCAM/CARL */
    W64          = 0x0B0000,     /* Sonic Foundry's 64 bit RIFF/WAV */
    MAT4         = 0x0C0000,     /* Matlab (tm) V4.2 / GNU Octave 2.0 */
    MAT5         = 0x0D0000,     /* Matlab (tm) V5.0 / GNU Octave 2.1 */

    /* Subtypes from here on. */

    PCM_S8       = 0x0001,       /* Signed 8 bit data */
    PCM_S16      = 0x0002,       /* Signed 16 bit data */
    PCM_S24      = 0x0003,       /* Signed 24 bit data */
    PCM_S32      = 0x0004,       /* Signed 32 bit data */

    PCM_U8       = 0x0005,       /* Unsigned 8 bit data (WAV and RAW only) */

    Float        = 0x0006,       /* 32 bit float data */
    Double       = 0x0007,       /* 64 bit float data */

    ULAW         = 0x0010,       /* U-Law encoded. */
    ALAW         = 0x0011,       /* A-Law encoded. */
    IMA_ADPCM    = 0x0012,       /* IMA ADPCM. */
    MS_ADPCM     = 0x0013,       /* Microsoft ADPCM. */

    GSM610       = 0x0020,       /* GSM 6.10 encoding. */
    VOX_ADPCM    = 0x0021,       /* Oki Dialogic ADPCM encoding. */

    G721_32      = 0x0030,       /* 32kbs G721 ADPCM encoding. */
    G723_24      = 0x0031,       /* 24kbs G723 ADPCM encoding. */
    G723_40      = 0x0032,       /* 40kbs G723 ADPCM encoding. */

    DWVW_12      = 0x0040,       /* 12 bit Delta Width Variable Word encoding. */
    DWVW_16      = 0x0041,       /* 16 bit Delta Width Variable Word encoding. */
    DWVW_24      = 0x0042,       /* 24 bit Delta Width Variable Word encoding. */
    DWVW_N       = 0x0043,       /* N bit Delta Width Variable Word encoding. */

    /* Endian-ness options. */

    FileEndian   = 0x00000000,   /* Default file endian-ness. */
    LittleEndian = 0x10000000,   /* Force little endian-ness. */
    BigEndian    = 0x20000000,   /* Force big endian-ness. */
    CPUEndian    = 0x30000000,   /* Force CPU endian-ness. */

    SubMask      = 0x0000FFFF,
    TypeMask     = 0x0FFF0000,
    EndMask      = 0x30000000
  }
  #endregion
  
  #region Functions
  [DllImport(Config.SndFileImportPath, EntryPoint="sf_open", CallingConvention=CallingConvention.Cdecl)]
  public static extern IntPtr Open(string path, OpenMode mode, ref Info sfinfo);
  [DllImport(Config.SndFileImportPath, EntryPoint="sf_open_calls", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern IntPtr OpenCalls(IOCalls* calls, IntPtr ioContext, OpenMode mode, ref Info sfinfo, int close);

  [DllImport(Config.SndFileImportPath, EntryPoint="sf_read_raw", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern long ReadRaw(IntPtr sndfile, void* ptr, long bytes);
  [DllImport(Config.SndFileImportPath, EntryPoint="sf_readf_short", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern long ReadShorts(IntPtr sndfile, short* ptr, long frames);
  [DllImport(Config.SndFileImportPath, EntryPoint="sf_readf_int", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern long ReadInts(IntPtr sndfile, int* ptr, long frames);
  [DllImport(Config.SndFileImportPath, EntryPoint="sf_seek", CallingConvention=CallingConvention.Cdecl)]
  public static extern long Seek(IntPtr sndfile, long frames, SeekType whence);
  [DllImport(Config.SndFileImportPath, EntryPoint="sf_close", CallingConvention=CallingConvention.Cdecl)]
  public static extern int Close(IntPtr sndfile);

  [DllImport(Config.SndFileImportPath, EntryPoint="sf_strerror", CallingConvention=CallingConvention.Cdecl)]
  public static extern string GetError(IntPtr sndfile);
  #endregion
  
  public static void RaiseError(IntPtr sndfile)
  { string error = GetError(sndfile);
    if(error==null) throw new GameLibException("GameLib sez: Something bad happened, but SF disagrees");
    else throw new GameLibException(error);
  }
}

#region StreamIOCalls class
internal class StreamIOCalls
{ public unsafe StreamIOCalls(Stream stream) : this(stream, true) { }
  public unsafe StreamIOCalls(Stream stream, bool autoClose)
  { if(stream==null) throw new ArgumentNullException("stream");
    else if(!stream.CanSeek || !stream.CanRead)
      throw new ArgumentException("Stream must be seekable and readable", "stream");
    this.stream    = stream;
    this.autoClose = autoClose;
    seek   = new SF.SeekHandler(OnSeek);
    read   = new SF.ReadHandler(OnRead);
    write  = new SF.WriteHandler(OnWrite);
    tell   = new SF.TellHandler(OnTell);
    gets   = new SF.GetsHandler(OnGets);
    length = new SF.LengthHandler(OnLength);
    trunc  = new SF.TruncateHandler(OnTruncate);
    close  = new SF.CloseHandler(OnClose);
    calls.Seek     = new DelegateMarshaller(seek).ToPointer();
    calls.Read     = new DelegateMarshaller(read).ToPointer();
    calls.Write    = new DelegateMarshaller(write).ToPointer();
    calls.Tell     = new DelegateMarshaller(tell).ToPointer();
    calls.Gets     = new DelegateMarshaller(gets).ToPointer();
    calls.Length   = new DelegateMarshaller(length).ToPointer();
    calls.Truncate = new DelegateMarshaller(trunc).ToPointer();
    calls.Close    = new DelegateMarshaller(close).ToPointer();
    
    if(!autoClose) GC.SuppressFinalize(this);
  }
  ~StreamIOCalls() { if(stream!=null) stream.Close(); }

  unsafe long OnSeek(IntPtr context, long offset, SF.SeekType type)
  { long pos=-1;
    switch(type)
    { case SF.SeekType.Absolute: pos = stream.Seek(offset, SeekOrigin.Begin); break;
      case SF.SeekType.Relative: pos = stream.Seek(offset, SeekOrigin.Current); break;
      case SF.SeekType.FromEnd:  pos = stream.Seek(offset, SeekOrigin.End); break;
    }
    return pos;
  }
  
  unsafe long OnRead(IntPtr context, byte* data, long size, long maxnum)
  { if(size<=0 || maxnum<=0) return 0;

    byte[] buf = new byte[size];
    long i=0, read;
    try
    { for(; i<maxnum; i++)
      { read = stream.Read(buf, 0, (int)size);
        if(read!=size) { return i==0 ? -1 : i; }
        for(int j=0; j<size; j++) *data++=buf[j];
      }
      return i;
    }
    catch { return i==0 ? -1 : i; }
  }

  unsafe long OnWrite(IntPtr context, byte* data, long size, long num)
  { if(!stream.CanWrite) return -1;
    if(size<=0 || num<=0) return 0;
    long total=size*num;
    int len = (int)Math.Min(total, 1024);
    byte[] buf = new byte[len];
    try
    { do
      { if(total<len) len=(int)total;
        for(int i=0; i<len; i++) buf[i]=*data++;
        stream.Write(buf, 0, len);
        total -= len;
      } while(total>0);
      return size;
    }
    catch { return -1; }
  }
  
  unsafe long OnTell(IntPtr context) { return stream.Position; }

  unsafe long OnGets(IntPtr context, byte* buffer, long bufsize)
  { long i=0;
    int  b;
    while(i<bufsize-1)
	  {	b = stream.ReadByte();
      if(b==-1) break;
      buffer[i++]=(byte)b;
      if(b==0 || buffer[i++] == 0x10) break;
		}
    buffer[i]=0;
    return i;
  }
    
  unsafe long OnLength(IntPtr context) { return stream.Length; }

  unsafe int OnTruncate(IntPtr context, long len)
  { try { stream.SetLength(len); } catch { return -1; }
    return 0;
  }
  
  unsafe int OnClose(IntPtr context)
  { if(autoClose) stream.Close();
    stream=null;
    GC.SuppressFinalize(this);
    return 0;
  }
  
  internal SF.IOCalls calls;
  Stream stream;
  SF.SeekHandler     seek;
  SF.ReadHandler     read;
  SF.WriteHandler    write;
  SF.TellHandler     tell;
  SF.GetsHandler     gets;
  SF.LengthHandler   length;
  SF.TruncateHandler trunc;
  SF.CloseHandler    close;
  bool autoClose;
}
#endregion

} // namespace GameLib.Interop.SndFile