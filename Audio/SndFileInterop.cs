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
using System.IO;
using System.Runtime.InteropServices;

namespace GameLib.Interop.SndFile
{

[System.Security.SuppressUnmanagedCodeSecurity()]
internal static class SF
{ 
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public unsafe delegate long LengthHandler(IntPtr context);
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public unsafe delegate long SeekHandler(long offset, SeekType type, IntPtr context);
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public unsafe delegate long ReadHandler(byte* data, long bytes, IntPtr context);
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public unsafe delegate long WriteHandler(byte* data, long bytes, IntPtr context);
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public unsafe delegate long TellHandler(IntPtr context);

  #region Structs
  [StructLayout(LayoutKind.Sequential, Pack=4)]
  public unsafe struct VirtualIO
  { public void* Length, Seek, Read, Write, Tell;
  }

  // specify size because C compilers pad to 8-byte boundaries if the struct contains any 8-byte members
  [StructLayout(LayoutKind.Sequential, Size=32, Pack=4)]
  public struct Info
  { public bool Has(Format f) { return (format&f)==f; }
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
    PVF          = 0x0E0000,     /* Portable Voice Format */
    XI           = 0x0F0000,     /* Fasttracker 2 Extended Instrument */
    HTK          = 0x100000,     /* HMM Tool Kit format */
    SDS          = 0x110000,     /* Midi Sample Dump Standard */
    AVR          = 0x120000,     /* Audio Visual Research */
    WAVEX        = 0x130000,     /* MS WAVE with WAVEFORMATEX */
    SD2          = 0x160000,     /* Sound Designer 2 */
    FLAC         = 0x170000,     /* FLAC lossless file format */
    CAF          = 0x180000,     /* Core Audio File format */

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

    DPCM_8       = 0x0050,       /* 8 bit differential PCM (XI only) */
    DPCM_16      = 0x0051,       /* 16 bit differential PCM (XI only) */

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
  [DllImport(Config.SndFileImportPath, ExactSpelling=true, EntryPoint="sf_open", CallingConvention=CallingConvention.Cdecl)]
  public static extern IntPtr Open(string path, OpenMode mode, ref Info sfinfo);
  [DllImport(Config.SndFileImportPath, ExactSpelling=true, EntryPoint="sf_open_virtual", CallingConvention=CallingConvention.Cdecl)]
  public static unsafe extern IntPtr OpenVirtual(VirtualIO* vio, OpenMode mode, ref Info sfInfo, IntPtr context);

  [DllImport(Config.SndFileImportPath, ExactSpelling=true, EntryPoint="sf_read_raw", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern long ReadRaw(IntPtr sndfile, void* ptr, long bytes);
  [DllImport(Config.SndFileImportPath, ExactSpelling=true, EntryPoint="sf_readf_short", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern long ReadShorts(IntPtr sndfile, short* ptr, long frames);
  [DllImport(Config.SndFileImportPath, ExactSpelling=true, EntryPoint="sf_readf_int", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern long ReadInts(IntPtr sndfile, int* ptr, long frames);
  [DllImport(Config.SndFileImportPath, ExactSpelling=true, EntryPoint="sf_seek", CallingConvention=CallingConvention.Cdecl)]
  public static extern long Seek(IntPtr sndfile, long frames, SeekType whence);
  [DllImport(Config.SndFileImportPath, ExactSpelling=true, EntryPoint="sf_close", CallingConvention=CallingConvention.Cdecl)]
  public static extern int Close(IntPtr sndfile);

  [DllImport(Config.SndFileImportPath, ExactSpelling=true, EntryPoint="sf_strerror", CallingConvention=CallingConvention.Cdecl)]
  public static extern string GetError(IntPtr sndfile);
  #endregion

  public static void RaiseError(IntPtr sndfile)
  { string error = GetError(sndfile);
    throw new GameLibException(error!=null ? error : "GameLib sez: Something bad happened, but SF disagrees");
  }
}

#region StreamVirtualIO class
internal class StreamVirtualIO : StreamCallbackSource
{ public unsafe StreamVirtualIO(Stream stream) : this(stream, true) { }
  public unsafe StreamVirtualIO(Stream stream, bool autoClose) : base(stream, autoClose)
  { if(!stream.CanSeek) throw new ArgumentException("stream must be seekable", "stream");
    length = new SF.LengthHandler(OnLength);
    seek   = new SF.SeekHandler(OnSeek);
    read   = new SF.ReadHandler(OnRead);
    write  = new SF.WriteHandler(OnWrite);
    tell   = new SF.TellHandler(OnTell);
    virtualIO.Length = new DelegateMarshaller(length).ToPointer();
    virtualIO.Seek   = new DelegateMarshaller(seek).ToPointer();
    virtualIO.Read   = new DelegateMarshaller(read).ToPointer();
    virtualIO.Write  = new DelegateMarshaller(write).ToPointer();
    virtualIO.Tell   = new DelegateMarshaller(tell).ToPointer();
  }

  long OnLength(IntPtr context) { return stream.Length; }
  long OnSeek(long offset, SF.SeekType type, IntPtr context) { return Seek(offset, (SeekType)type); }
  unsafe long OnRead(byte* data, long bytes, IntPtr context) { return Read(data, (int)bytes, (int)bytes); }
  unsafe long OnWrite(byte* data, long bytes, IntPtr context) { return Write(data, bytes, 1); }
  long OnTell(IntPtr context) { return stream.Position; }

  internal SF.VirtualIO virtualIO;
  SF.LengthHandler   length;
  SF.SeekHandler     seek;
  SF.ReadHandler     read;
  SF.WriteHandler    write;
  SF.TellHandler     tell;
}
#endregion

} // namespace GameLib.Interop.SndFile