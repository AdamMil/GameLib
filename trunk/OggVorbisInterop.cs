using System;
using System.Runtime.InteropServices;
using GameLib.Interop;

namespace GameLib.Interop.OggVorbis
{

[System.Security.SuppressUnmanagedCodeSecurity()]
internal class Ogg
{ 
  [CallConvCdecl] public unsafe delegate int ReadHandler(byte* buf, int size, int num, void* context); // 'int' is really size_t in the .h
  [CallConvCdecl] public unsafe delegate int SeekHandler(void* context, long offset, SeekType type);
  [CallConvCdecl] public unsafe delegate int CloseHandler(void* context);
  [CallConvCdecl] public unsafe delegate int TellHandler(void* context); // 'int' is actually 'long' in the .h, conflicts with seek, suckage! 

  public enum SeekType : int
  { Absolute, Relative, FromEnd
  }
  
  public enum Status : int
  { NotOpen, PartlyOpen, Opened, StreamSet, InitSet
  };
  
  [StructLayout(LayoutKind.Sequential, Pack=4)]
  public unsafe struct Callbacks
  { public void* Read, Seek, Close, Tell;
  }
  
  [StructLayout(LayoutKind.Sequential, Pack=4)]
  public unsafe struct VorbisInfo
  { public int Version;
    public int Channels;
    public int Rate;

    public int UpperBitrate, NominalBitrate, LowerBitrate, BitrateWindow;

    IntPtr codec_setup;
  }

  [StructLayout(LayoutKind.Sequential, Size=720, Pack=4)]
  public unsafe struct VorbisFile
  { private void* dataSource;
    public  int   Seekable;
    private long  offset, end;
    private int   seekState;
    public  int   NumLinks;
    private void* offsets, dataOffsets, serialNos, pcmLengths;
    public VorbisInfo* Info;
  }

  [DllImport(Config.VorbisImportPath, EntryPoint="ov_clear", CallingConvention=CallingConvention.Cdecl)]
  public static extern int Clear(ref VorbisFile vf);
  [DllImport(Config.VorbisImportPath, EntryPoint="ov_open_callbacks", CallingConvention=CallingConvention.Cdecl)]
  public static extern int OpenCallbacks(IntPtr context, out VorbisFile vf, string initial, int ibytes, Callbacks callbacks);
  [DllImport(Config.VorbisImportPath, EntryPoint="ov_pcm_total", CallingConvention=CallingConvention.Cdecl)]
  public static extern long PcmTotal(ref VorbisFile vf, int i);
  [DllImport(Config.VorbisImportPath, EntryPoint="ov_time_total", CallingConvention=CallingConvention.Cdecl)]
  public static extern double TimeTotal(ref VorbisFile vf, int i);
  [DllImport(Config.VorbisImportPath, EntryPoint="ov_raw_seek", CallingConvention=CallingConvention.Cdecl)]
  public static extern int RawSeek(ref VorbisFile vf, long pos);
  [DllImport(Config.VorbisImportPath, EntryPoint="ov_pcm_seek", CallingConvention=CallingConvention.Cdecl)]
  public static extern int PcmSeek(ref VorbisFile vf, long pos);
  [DllImport(Config.VorbisImportPath, EntryPoint="ov_time_seek", CallingConvention=CallingConvention.Cdecl)]
  public static extern int TimeSeek(ref VorbisFile vf, double pos);
  [DllImport(Config.VorbisImportPath, EntryPoint="ov_pcm_tell", CallingConvention=CallingConvention.Cdecl)]
  public static extern long PcmTell(ref VorbisFile vf);
  [DllImport(Config.VorbisImportPath, EntryPoint="ov_time_tell", CallingConvention=CallingConvention.Cdecl)]
  public static extern double TimeTell(ref VorbisFile vf);
  [DllImport(Config.VorbisImportPath, EntryPoint="ov_info", CallingConvention=CallingConvention.Cdecl)]
  public static extern VorbisInfo* GetInfo(ref VorbisFile vf, int link);
  [DllImport(Config.VorbisImportPath, EntryPoint="ov_read", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int Read(ref VorbisFile vf, byte* buffer, int length, int bigendian, int word, int signed, out int bitstream);
}

#region VorbisCallbacks
internal class VorbisCallbacks
{ public VorbisCallbacks(System.IO.Stream stream) : this(stream, true) { }
  public unsafe VorbisCallbacks(System.IO.Stream stream, bool autoClose)
  { if(stream==null) throw new ArgumentNullException("stream");
    else if(!stream.CanRead) throw new ArgumentException("Stream must be readable", "stream");
    this.stream    = stream;
    this.autoClose = autoClose;
    read  = new Ogg.ReadHandler(OnRead);
    seek  = new Ogg.SeekHandler(OnSeek);
    close = new Ogg.CloseHandler(OnClose);
    tell  = new Ogg.TellHandler(OnTell);
    calls.Read  = new DelegateMarshaller(read).ToPointer();
    calls.Seek  = new DelegateMarshaller(seek).ToPointer();
    calls.Close = new DelegateMarshaller(close).ToPointer();
    calls.Tell  = new DelegateMarshaller(tell).ToPointer();

    if(!autoClose) GC.SuppressFinalize(this);
  }
  ~VorbisCallbacks() { if(stream!=null) stream.Close(); }

  unsafe int OnRead(byte* data, int size, int maxnum, void* context)
  { if(size<=0 || maxnum<=0) return 0;

    byte[] buf = new byte[size];
    int i=0, read;
    try
    { for(; i<maxnum; i++)
      { read = stream.Read(buf, 0, size);
        if(read!=size) { return i==0 ? -1 : i; }
        for(int j=0; j<size; j++) *data++=buf[j];
      }
      return i;
    }
    catch { return i==0 ? -1 : i; }
  }

  unsafe int OnSeek(void* context, long offset, Ogg.SeekType type)
  { if(!stream.CanSeek) return -1;
    long pos=-1;
    switch(type)
    { case Ogg.SeekType.Absolute: pos = stream.Seek(offset, System.IO.SeekOrigin.Begin); break;
      case Ogg.SeekType.Relative: pos = stream.Seek(offset, System.IO.SeekOrigin.Current); break;
      case Ogg.SeekType.FromEnd:  pos = stream.Seek(offset, System.IO.SeekOrigin.End); break;
    }
    return (int)pos;
  }
  
  unsafe int OnClose(void* context)
  { if(autoClose) stream.Close();
    stream=null;
    GC.SuppressFinalize(this);
    return 0;
  }
  
  unsafe int OnTell(void* context)
  { return stream.CanSeek ? (int)stream.Position : -1;
  }
  
  internal Ogg.Callbacks calls;
  System.IO.Stream stream;
  Ogg.ReadHandler  read;
  Ogg.SeekHandler  seek;
  Ogg.CloseHandler close;
  Ogg.TellHandler  tell;
  bool autoClose;
}
#endregion

} // namespace GameLib.Interop.OggVorbis
