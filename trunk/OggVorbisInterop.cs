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

using System;
using System.Runtime.InteropServices;
using GameLib.Interop;

namespace GameLib.Interop.OggVorbis
{

[System.Security.SuppressUnmanagedCodeSecurity()]
internal class Ogg
{ 
  [CallConvCdecl] public unsafe delegate int  ReadHandler(void* context, byte* buf, int size, int maxnum);
  [CallConvCdecl] public unsafe delegate int  SeekHandler(void* context, int offset, SeekType type);
  [CallConvCdecl] public unsafe delegate int  TellHandler(void* context);
  [CallConvCdecl] public unsafe delegate void CloseHandler(void* context);

  #region Enums
  public enum SeekType : int
  { Absolute, Relative, FromEnd
  }
  
  public enum Status : int
  { NotOpen, PartlyOpen, Opened, StreamSet, InitSet
  };
  
  public enum OggError
  { False=-1, Eof=-2, Hole=-3, Read=-128, Fault=-129, NotImpl=-130, Invalid=-131, NotVorbis=-132, BadHeader=-133,
    BadVersion=-134, NotAudio=-135, BadPacket=-136, BadLink=-137, NoSeek=-138
  }
  #endregion
  
  #region Structs
  [StructLayout(LayoutKind.Sequential, Pack=4, Size=16)]
  public unsafe struct Callbacks
  { public void* Read, Seek, Tell, Close;
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
    private long  dummy1, dummy2, dummy3;
    private int   dummy4;
    public  int   NumLinks;
    private void* offsets, dataOffsets, serialNos, pcmLengths;
    public VorbisInfo* Info;
  }
  #endregion

  #region Imports
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_Open", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int Open(out VorbisFile vf, Callbacks *calls);
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_Close", CallingConvention=CallingConvention.Cdecl)]
  public static extern void Close(ref VorbisFile vf);
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_PcmLength", CallingConvention=CallingConvention.Cdecl)]
  public static extern int PcmLength(ref VorbisFile vf, int section);
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_PcmTell", CallingConvention=CallingConvention.Cdecl)]
  public static extern int PcmTell(ref VorbisFile vf);
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_PcmSeek", CallingConvention=CallingConvention.Cdecl)]
  public static extern int PcmSeek(ref VorbisFile vf, int frames);
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_TimeLength", CallingConvention=CallingConvention.Cdecl)]
  public static extern double TimeLength(ref VorbisFile vf, int section);
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_TimeTell", CallingConvention=CallingConvention.Cdecl)]
  public static extern double TimeTell(ref VorbisFile vf);
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_TimeSeek", CallingConvention=CallingConvention.Cdecl)]
  public static extern int TimeSeek(ref VorbisFile vf, double seconds);
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_Read", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int Read(ref VorbisFile vf, byte *buf, int length, int bigEndian, int word, int sgned, out int section);
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_Info", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern VorbisInfo * GetInfo(ref VorbisFile vf, int section);
  #endregion
  
  public static void Check(int result)
  { if(result>-128) return;
    switch((OggError)result)
    { case OggError.Read:
        throw new OggVorbisException((GameLib.OggError)result, "Read error while fetching compressed data for decode");
      case OggError.Fault:
        throw new OggVorbisException((GameLib.OggError)result, "Internal inconsistency in decode state. Continuing is likely not possible.");
      case OggError.NotImpl:
        throw new OggVorbisException((GameLib.OggError)result, "Feature not implemented");
      case OggError.Invalid:
        throw new OggVorbisException((GameLib.OggError)result, "Either an invalid argument, or incompletely initialized argument passed to libvorbisfile call");
      case OggError.NotVorbis:
        throw new OggVorbisException((GameLib.OggError)result, "The given file/data was not recognized as Ogg Vorbis data.");
      case OggError.BadHeader:
        throw new OggVorbisException((GameLib.OggError)result, "The file/data is apparently an Ogg Vorbis stream, but contains a corrupted or undecipherable header.");
      case OggError.BadVersion:
        throw new OggVorbisException((GameLib.OggError)result, "The bitstream format revision of the given stream is not supported.");
      case OggError.BadLink:
        throw new OggVorbisException((GameLib.OggError)result, "The given link exists in the Vorbis data stream, but is not decipherable due to garbacge or corruption.");
      case OggError.NoSeek:
        throw new OggVorbisException((GameLib.OggError)result, "The given stream is not seekable");
      default: throw new OggVorbisException((GameLib.OggError)result, "Unknown error");
    }
  }
}

#region VorbisCallbacks
internal class VorbisCallbacks : StreamCallbackSource
{ public VorbisCallbacks(System.IO.Stream stream) : this(stream, true) { }
  public unsafe VorbisCallbacks(System.IO.Stream stream, bool autoClose) : base(stream, autoClose)
  { seek  = new Ogg.SeekHandler(OnSeek);
    read  = new Ogg.ReadHandler(OnRead);
    tell  = new Ogg.TellHandler(OnTell);
    close = new Ogg.CloseHandler(OnClose);
    calls.Seek  = new DelegateMarshaller(seek).ToPointer();
    calls.Read  = new DelegateMarshaller(read).ToPointer();
    calls.Tell  = new DelegateMarshaller(tell).ToPointer();
    calls.Close = new DelegateMarshaller(close).ToPointer();
  }

  unsafe int OnRead(void* context, byte* buf, int size, int maxnum) { return (int)Read(buf, size, maxnum); }
  unsafe int OnSeek(void* context, int offset, Ogg.SeekType type) { return Seek(offset, (SeekType)type)<0 ? -1 : 0; }
  unsafe int OnTell(void* context) { return (int)Tell(); }
  unsafe void OnClose(void* context) { MaybeClose(); }

  internal Ogg.Callbacks calls;
  Ogg.SeekHandler  seek;
  Ogg.ReadHandler  read;
  Ogg.TellHandler  tell;
  Ogg.CloseHandler close;
}
#endregion

} // namespace GameLib.Interop.OggVorbis
