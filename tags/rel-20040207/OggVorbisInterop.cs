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
internal sealed class Ogg
{ 
  [CallConvCdecl] public unsafe delegate int  ReadHandler(byte* buf, int size, int maxnum);
  [CallConvCdecl] public unsafe delegate int  SeekHandler(int offset, SeekType type);
  [CallConvCdecl] public unsafe delegate int  TellHandler();
  [CallConvCdecl] public unsafe delegate void CloseHandler();

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

    void* codec_setup;
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
  public unsafe static extern int Open(VorbisFile** vf, Callbacks calls);
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_Close", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern void Close(VorbisFile* vf);
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_PcmLength", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int PcmLength(VorbisFile* vf, int section);
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_PcmTell", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int PcmTell(VorbisFile* vf);
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_PcmSeek", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int PcmSeek(VorbisFile* vf, int frames);
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_TimeLength", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern double TimeLength(VorbisFile* vf, int section);
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_TimeTell", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern double TimeTell(VorbisFile* vf);
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_TimeSeek", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int TimeSeek(VorbisFile* vf, double seconds);
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_Read", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int Read(VorbisFile* vf, byte* buf, int length, int bigEndian, int word, int sgned, out int section);
  [DllImport(Config.VorbisImportPath, EntryPoint="VW_Info", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern VorbisInfo* GetInfo(VorbisFile* vf, int section);
  #endregion
  
  public static void Check(int result)
  { if(result>-128) return;
    switch((OggError)result)
    { case OggError.Read:
        throw new Audio.OggVorbisException((Audio.OggError)result, "Read error while fetching compressed data for decode");
      case OggError.Fault:
        throw new Audio.OggVorbisException((Audio.OggError)result, "Internal inconsistency in decode state. Continuing is likely not possible.");
      case OggError.NotImpl:
        throw new Audio.OggVorbisException((Audio.OggError)result, "Feature not implemented");
      case OggError.Invalid:
        throw new Audio.OggVorbisException((Audio.OggError)result, "Either an invalid argument, or incompletely initialized argument passed to libvorbisfile call");
      case OggError.NotVorbis:
        throw new Audio.OggVorbisException((Audio.OggError)result, "The given file/data was not recognized as Ogg Vorbis data.");
      case OggError.BadHeader:
        throw new Audio.OggVorbisException((Audio.OggError)result, "The file/data is apparently an Ogg Vorbis stream, but contains a corrupted or undecipherable header.");
      case OggError.BadVersion:
        throw new Audio.OggVorbisException((Audio.OggError)result, "The bitstream format revision of the given stream is not supported.");
      case OggError.BadLink:
        throw new Audio.OggVorbisException((Audio.OggError)result, "The given link exists in the Vorbis data stream, but is not decipherable due to garbage or corruption.");
      case OggError.NoSeek:
        throw new Audio.OggVorbisException((Audio.OggError)result, "The given stream is not seekable");
      default: throw new Audio.OggVorbisException((Audio.OggError)result, "Unknown error");
    }
  }
}

#region VorbisCallbacks
internal class VorbisCallbacks : StreamCallbackSource
{ public VorbisCallbacks(System.IO.Stream stream) : this(stream, true) { }
  public unsafe VorbisCallbacks(System.IO.Stream stream, bool autoClose) : base(stream, autoClose)
  { seek  = new DelegateMarshaller(new Ogg.SeekHandler(OnSeek));
    read  = new DelegateMarshaller(new Ogg.ReadHandler(OnRead));
    tell  = new DelegateMarshaller(new Ogg.TellHandler(OnTell));
    close = new DelegateMarshaller(new Ogg.CloseHandler(OnClose));
    calls.Seek  = seek.ToPointer();
    calls.Read  = read.ToPointer();
    calls.Tell  = tell.ToPointer();
    calls.Close = close.ToPointer();
  }

  unsafe int OnRead(byte* buf, int size, int maxnum) { return (int)Read(buf, size, maxnum); }
  int OnSeek(int offset, Ogg.SeekType type) { return Seek(offset, (SeekType)type)<0 ? -1 : 0; }
  int OnTell() { return (int)Tell(); }
  void OnClose() { MaybeClose(); }

  internal Ogg.Callbacks calls;
  DelegateMarshaller seek, read, tell, close;
}
#endregion

} // namespace GameLib.Interop.OggVorbis
