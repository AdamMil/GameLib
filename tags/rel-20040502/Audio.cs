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
using System.IO;
using System.Collections;
using GameLib.IO;
using GameLib.Interop.SDL;
using GameLib.Interop.GLMixer;
using GameLib.Interop.SndFile;
using GameLib.Interop.OggVorbis;

// FIXME: find out why vorbis is unstable

namespace GameLib.Audio
{

#region Structs, Enums, and Delegates
[Flags]
public enum SampleFormat : ushort
{ Eight=GLMixer.Format.Eight, Sixteen=GLMixer.Format.Sixteen, BitsPart=GLMixer.Format.BitsPart,
  Signed=GLMixer.Format.Signed, BigEndian=GLMixer.Format.BigEndian,

  U8=Eight, U16=Sixteen, S8=Eight|Signed, S16=Sixteen|Signed,
  U8BE=U8|BigEndian, U16BE=U16|BigEndian, S8BE=S8|BigEndian, S16BE=S16|BigEndian,
  U8Sys=GLMixer.Format.U8Sys, U16Sys=GLMixer.Format.U16Sys, S8Sys=GLMixer.Format.S8Sys, S16Sys=GLMixer.Format.S16Sys,
  
  Default=S16Sys
}

public enum Speakers : byte { Mono=1, Stereo=2 }
public enum ChannelStatus { Stopped, Playing, Paused }
public enum Fade { None, In, Out }
public enum PlayPolicy { Fail, Oldest, Priority, OldestPriority }
public enum MixPolicy  { DontDivide, Divide }

public struct SizedArray
{ public SizedArray(byte[] array) { Array=array; Length=array.Length; }
  public SizedArray(byte[] array, int length)
  { Array=array; Length=length;
  }
  public byte[] Shrunk
  { get
    { if(Array.Length>Length)
      { byte[] ret = new byte[Length];
        System.Array.Copy(Array, ret, Length);
        Array=ret;
      }
      return Array;
    }
  }
  public byte[] Array;
  public int    Length;
}
  
public struct AudioFormat
{ public AudioFormat(uint frequency, SampleFormat format, byte channels)
  { Frequency=frequency; Format=format; Channels=channels;
  }
  public byte SampleSize  { get { return (byte)((int)(Format&SampleFormat.BitsPart)>>3); } }
  public byte FrameSize   { get { return (byte)(SampleSize*Channels); } }
  public uint ByteRate    { get { return (uint)(FrameSize*Frequency); } }

  public uint         Frequency;
  public SampleFormat Format;
  public byte         Channels;
}

public unsafe delegate void MixFilter(Channel channel, int* buffer, int frames, AudioFormat format);
public delegate void ChannelFinishedHandler(Channel channel);
#endregion

#region AudioSource
public abstract class AudioSource : IDisposable
{ public int Both
  { get { return left; }
    set { Audio.CheckVolume(value); left=right=value; }
  }

  public abstract bool CanRewind { get; }
  public abstract bool CanSeek   { get; }

  public AudioFormat Format { get { return format; } }

  public int Left
  { get { return left; }
    set { Audio.CheckVolume(value); left=value; }
  }

  public int Length { get { return length; } }

  public float PlaybackRate { get { return rate; } set { Audio.CheckRate(rate); lock(this) rate=value; } }

  public abstract int  Position  { get; set; }

  public int Priority { get { return priority; } set { priority=value; } }

  public int Right
  { get { return right; }
    set { Audio.CheckVolume(value); right=value; }
  }
  
  public virtual void Dispose() { buffer=null; }

  public virtual void Rewind() { Position=0; }

  public void GetVolume(out int left, out int right) { left=this.left; right=this.right; }
  public void SetVolume(int left, int right) { Left=left; Right=right; }

  public Channel Play() { return Play(0, Audio.Infinite, 0, Audio.FreeChannel); }
  public Channel Play(int loops) { return Play(loops, Audio.Infinite, 0, Audio.FreeChannel); }
  public Channel Play(int loops, int timeoutMs) { return Play(loops, timeoutMs, 0, Audio.FreeChannel); }
  public Channel Play(int loops, int timeoutMs, int position) { return Play(loops, timeoutMs, position, Audio.FreeChannel); }
  public Channel Play(int loops, int timeoutMs, int position, int channel)
  { if(channel<0) return Audio.StartPlaying(channel, this, loops, position, Fade.None, 0, timeoutMs);
    else
    { Channel c = Audio.Channels[channel];
      lock(c)
      { c.StartPlaying(this, loops, position, Fade.None, 0, timeoutMs);
        return c;
      }
    }
  }

  public Channel FadeIn(uint fadeMs) { return FadeIn(fadeMs, 0, Audio.Infinite, Audio.FreeChannel, 0); }
  public Channel FadeIn(uint fadeMs, int loops) { return FadeIn(fadeMs, loops, Audio.Infinite, Audio.FreeChannel, 0); }
  public Channel FadeIn(uint fadeMs, int loops, int timeoutMs) { return FadeIn(fadeMs, loops, timeoutMs, Audio.FreeChannel, 0); }
  public Channel FadeIn(uint fadeMs, int loops, int timeoutMs, int channel) { return FadeIn(fadeMs, loops, timeoutMs, channel, 0); }
  public Channel FadeIn(uint fadeMs, int loops, int timeoutMs, int channel, int position)
  { if(channel<0) return Audio.StartPlaying(channel, this, loops, position, Fade.In, fadeMs, timeoutMs);
    else
    { Channel c = Audio.Channels[channel];
      lock(Audio.Channels[channel])
      { c.StartPlaying(this, loops, position, Fade.In, fadeMs, timeoutMs);
        return c;
      }
    }
  }
  
  protected int BytesToFrames(int bytes)
  { int frames = bytes/format.FrameSize;
    if(frames*format.FrameSize != bytes) throw new ArgumentException("length must be multiple of the frame size");
    return frames;
  }

  public int ReadBytes(byte[] buf, int length) { return ReadBytes(buf, 0, length); }
  public abstract int ReadBytes(byte[] buf, int index, int length);

  public virtual byte[] ReadAll()
  { lock(this)
    { byte[] ret;
      int pos=0;
      if(CanSeek)
      { pos = Position;
        Rewind();
      }
      if(length>=0)
      { ret = new byte[length*format.FrameSize];
        if(ReadBytes(ret, ret.Length)!=ret.Length) throw new EndOfStreamException();
      }
      else ret=ReadAllUL();
      if(CanSeek) Position=pos;
      return ret;
    }
  }

  protected byte[] ReadAllUL()
  { byte[] buf = new byte[16384];
    int toRead=buf.Length, index=0, read = ReadBytes(buf, index, toRead), length;
    while(read!=0 && read==toRead)
    { index = buf.Length;
      byte[] nb = new byte[buf.Length*2];
      Array.Copy(buf, nb, buf.Length);
      buf = nb;
      read = ReadBytes(buf, index, index);
    }
    length = index+read;
    if(length==buf.Length) return buf;
    byte[] ret = new byte[length];
    Array.Copy(buf, ret, length);
    return ret;
  }

  public unsafe int ReadFrames(int* dest, int frames) { return ReadFrames(dest, frames, -1, -1); }
  public unsafe int ReadFrames(int* dest, int frames, int volume) { return ReadFrames(dest, frames, volume, volume); }
  public virtual unsafe int ReadFrames(int* dest, int frames, int left, int right)
  { lock(this)
    { int toRead=length<0 ? frames : Math.Min(length-curPos, frames), bytes=toRead*format.FrameSize, read, samples;
      SizeBuffer(bytes);
      read    = ReadBytes(buffer, bytes);
      samples = read/format.SampleSize;
      fixed(byte* src = buffer)
        GLMixer.Check(GLMixer.ConvertMix(dest, src, (uint)samples, (ushort)format.Format,
                                         (ushort)(left <0 ? Audio.MaxVolume : left),
                                         (ushort)(right<0 ? Audio.MaxVolume : right)));
      read = format.Channels==1 ? samples : samples/2;
      return read;
    }
  }

  protected void SizeBuffer(int size) { if(buffer==null || buffer.Length<size) buffer=new byte[size]; }

  protected byte[] buffer;
  protected AudioFormat format;
  protected float rate=1f;
  protected int   left=Audio.MaxVolume, right=Audio.MaxVolume, curPos=0, length=-1, priority;
  internal  int   playing;
}
#endregion

#region StreamSource
public abstract class StreamSource : AudioSource
{ public override bool CanRewind { get { return stream.CanSeek; } }
  public override bool CanSeek   { get { return stream.CanSeek; } }
  public override int  Position
  { get { return curPos; }
    set { if(value!=curPos) lock(this) { stream.Position=startPos+value*format.FrameSize; curPos=value; } }
  }

  public override void Dispose()
  { if(stream!=null)
    { base.Dispose();
      if(autoClose) stream.Close();
      stream=null;
    }
  }

  public override int ReadBytes(byte[] buf, int index, int length)
  { BytesToFrames(length);
    lock(this)
    { int read = stream.Read(buf, index, length);
      if(read>=0) curPos += read/format.FrameSize;
      return read;
    }
  }

  protected void Init(Stream stream, AudioFormat format, int startPos, int length, bool autoClose)
  { if(stream==null) throw new ArgumentNullException("stream");
    if(this.stream!=null) throw new InvalidOperationException("This object has already been initialized");
    this.stream=stream;
    this.format=format;
    this.startPos=startPos;
    this.length=length<0 ? length : length/format.FrameSize;
    this.autoClose=autoClose;
    curPos = stream.CanSeek ? (int)(stream.Position-startPos/format.FrameSize) : 0;
  }

  protected Stream stream;
  protected int    startPos;
  protected bool   autoClose;
}
#endregion

#region RawSource
public class RawSource : StreamSource
{ public RawSource(string filename, AudioFormat format) : this(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read), format) { }
  public RawSource(string filename, AudioFormat format, int start, int length) : this(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read), format, start, length) { }
  public RawSource(Stream stream, AudioFormat format) : this(stream, format, 0, stream.CanSeek ? (int)stream.Length : -1, true) { }
  public RawSource(Stream stream, AudioFormat format, bool autoClose) : this(stream, format, 0, stream.CanSeek ? (int)stream.Length : -1, autoClose) { }
  public RawSource(Stream stream, AudioFormat format, int start, int length) : this(stream, format, start, length, true) { }
  public RawSource(Stream stream, AudioFormat format, int start, int length, bool autoClose)
  { Init(stream, format, start, length, autoClose);
  }
}
#endregion

#region SoundFileSource
public class SoundFileSource : AudioSource
{ public SoundFileSource(string filename)
  { SF.Info info = new SF.Info();
    info.format = SF.Format.CPUEndian;
    sndfile = SF.Open(filename, SF.OpenMode.Read, ref info);
    Init(ref info);
  }
  public SoundFileSource(string filename, AudioFormat format) // for RAW
  { SF.Info info = new SF.Info();
    InitInfo(ref info, format);
    sndfile = SF.Open(filename, SF.OpenMode.Read, ref info);
    Init(ref info);
  }
  public SoundFileSource(Stream stream) : this(stream, true) { }
  public SoundFileSource(Stream stream, bool autoClose)
  { SF.Info info = new SF.Info();
    calls = new StreamIOCalls(stream, autoClose);
    unsafe
    { fixed(SF.IOCalls* io = &calls.calls)
        sndfile = SF.OpenCalls(io, new IntPtr(null), SF.OpenMode.Read, ref info, 1);
    }
    Init(ref info);
  }
  public SoundFileSource(Stream stream, AudioFormat format) : this(stream, format, true) { } // for RAW
  public SoundFileSource(Stream stream, AudioFormat format, bool autoClose)
  { SF.Info info = new SF.Info();
    InitInfo(ref info, format);
    calls = new StreamIOCalls(stream, autoClose);
    unsafe
    { fixed(SF.IOCalls* io = &calls.calls)
        sndfile = SF.OpenCalls(io, new IntPtr(null), SF.OpenMode.Read, ref info, 1);
    }
    Init(ref info);
  }
  ~SoundFileSource() { Dispose(true); }
  public override void Dispose() { Dispose(false); GC.SuppressFinalize(this); }
  
  public override bool CanRewind { get { return true; } }
  public override bool CanSeek   { get { return true; } }
  public override int  Position
  { get { return curPos; }
    set
    { if(value==curPos) return;
      lock(this) curPos=(int)SF.Seek(sndfile, value, SF.SeekType.Absolute);
    }
  }

  public override int ReadBytes(byte[] buf, int index, int length)
  { int frames = BytesToFrames(length);
    lock(this)
    { int read;
      unsafe { fixed(byte* pbuf = buf) read=(int)SF.ReadShorts(sndfile, (short*)(pbuf+index), frames); }
      if(read>=0) curPos += read;
      return read*format.FrameSize;
    }
  }

  protected void Dispose(bool finalizing)
  { unsafe
    { if(sndfile.ToPointer()!=null)
      { base.Dispose();
        SF.Close(sndfile);
        sndfile = new IntPtr(null);
      }
    }
  }
  
  void Init(ref SF.Info info)
  { unsafe { if(sndfile.ToPointer()==null) throw new FileNotFoundException(); }
    format.Channels  = (byte)info.channels;
    format.Frequency = (uint)info.samplerate;
    format.Format    = SampleFormat.S16;
    #if BIGENDIAN
    if(info.Has(SF.Format.CPUEndian) || info.Has(SF.Format.BigEndian))
    #else
    if(!info.Has(SF.Format.CPUEndian) && info.Has(SF.Format.BigEndian))
    #endif
    { format.Format |= SampleFormat.BigEndian;
    }
    
    length = (int)info.frames;
  }
  
  void InitInfo(ref SF.Info info, AudioFormat format)
  { info.channels   = format.Channels;
    info.samplerate = (int)format.Frequency;
    info.format     = SF.Format.RAW;
    if((format.Format&SampleFormat.S16)==SampleFormat.S16) info.format |= SF.Format.PCM_S16;
    else if((format.Format&SampleFormat.U8)==SampleFormat.U8) info.format |= SF.Format.PCM_U8;
    else if((format.Format&SampleFormat.S8)==SampleFormat.S8) info.format |= SF.Format.PCM_S8;
    if((format.Format&SampleFormat.BigEndian)!=0) info.format |= SF.Format.BigEndian;
    else info.format |= SF.Format.LittleEndian;
  }

  IntPtr sndfile;
  StreamIOCalls calls;
}
#endregion

#region SampleSource
public class SampleSource : AudioSource
{ public SampleSource(AudioSource stream) : this(stream, true) { }
  public SampleSource(AudioSource stream, bool mixerFormat)
  { data   = stream.ReadAll();
    format = stream.Format;

    if(mixerFormat)
    { data = Audio.Convert(data, format, Audio.Format).Shrunk;
      format = Audio.Format;
    }
    length = data.Length/format.FrameSize;
  }
  public SampleSource(AudioSource stream, AudioFormat convertTo)
  { data = Audio.Convert(stream.ReadAll(), stream.Format, format=convertTo).Shrunk;
    length = data.Length/format.FrameSize;
  }
  public override void Dispose() { base.Dispose(); data=null; }

  public override bool CanRewind { get { return true; } }
  public override bool CanSeek   { get { return true; } }
  public override int  Position
  { get { return curPos; }
    set
    { if(value!=curPos)
      { if(value<0 || value>length) throw new ArgumentOutOfRangeException("Position");
        lock(this) curPos = value;
      }
    }
  }

  public override byte[] ReadAll() { return (byte[])data.Clone(); }

  public override int ReadBytes(byte[] buf, int index, int length)
  { int frames = BytesToFrames(length);
    lock(this)
    { int toRead = Math.Min(frames, this.length-curPos);
      Array.Copy(data, curPos*format.FrameSize, buf, index, toRead*format.FrameSize);
      curPos += toRead;
      return toRead*format.FrameSize;
    }
  }

  public override unsafe int ReadFrames(int *dest, int frames, int left, int right)
  { lock(this)
    { int toRead=Math.Min(length-curPos, frames), samples=toRead*format.Channels;
      fixed(byte* src = data)
        GLMixer.Check(GLMixer.ConvertMix(dest, src+curPos*format.FrameSize, (uint)samples, (ushort)format.Format,
                                         (ushort)(left <0 ? Audio.MaxVolume : left),
                                         (ushort)(right<0 ? Audio.MaxVolume : right)));
      curPos += toRead;
      return toRead;
    }
  }

  protected byte[] data;
}
#endregion

#region VorbisSource
public class VorbisSource : AudioSource
{ public VorbisSource(string filename)
    : this(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read), true) { }
  public VorbisSource(Stream stream) : this(stream, true) { }
  public unsafe VorbisSource(Stream stream, bool autoClose)
  { calls = new VorbisCallbacks(stream, autoClose);
    unsafe
    { fixed(Ogg.VorbisFile** fp=&file) Ogg.Check(Ogg.Open(fp, calls.calls));
      Init(autoClose, new AudioFormat((uint)file->Info->Rate,
                                      Audio.Initialized ? Audio.Format.Format : SampleFormat.S16Sys,
                                      (byte)file->Info->Channels));
    }
  }
  public VorbisSource(Stream stream, bool autoClose, AudioFormat format)
  { calls = new VorbisCallbacks(stream, autoClose);
    unsafe { fixed(Ogg.VorbisFile** fp=&file) Ogg.Check(Ogg.Open(fp, calls.calls)); }
    Init(autoClose, format);
  }
  ~VorbisSource() { Dispose(true); }

  public override void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  public unsafe override bool CanRewind { get { return file->Seekable!=0; } }
  public unsafe override bool CanSeek   { get { return file->Seekable!=0; } }
  public unsafe override int Position
  { get { return curPos; }
    set
    { if(value==curPos) return;
      if(!CanSeek) Ogg.Check((int)Ogg.OggError.NoSeek);
      lock(this)
      { if(links==null) Ogg.Check(Ogg.PcmSeek(file, value));
        else
        { int i, pos=value, pbase=0;
          for(i=0; i<links.Length && links[i].CvtLen<=pos; i++)
          { pos   -= links[i].CvtLen;
            pbase += links[i].RealLen;
          }
          if(i==links.Length)
          { Ogg.Check(Ogg.PcmSeek(file, pbase));
            value = length;
            curLink = 0;
          }
          else
          { Ogg.Check(Ogg.PcmSeek(file, pbase+(int)((long)pos*links[i].CVT.lenDiv/links[i].CVT.lenMul)));
            curLink = i;
          }
        }
        curPos = value;
      }
    }
  }

  internal unsafe void CheckVorbisFile(Ogg.VorbisFile* vf)
  { byte* p1 = *(byte**)((byte*)vf+656);
    if(p1==null) return;
    byte* p2 = *(byte**)((byte*)p1+4);
    short chans = *(short*)((byte*)p2+4);
    if(chans!=2) throw new Exception("it's fucked");
  }
  
  public unsafe override int ReadBytes(byte[] buf, int index, int length)
  { BytesToFrames(length);
    fixed(byte* dest = buf)
    { Ogg.VorbisInfo* info;
      int read, toRead, total=0, section, be=(format.Format&SampleFormat.BigEndian)==0 ? 0 : 1;
      int signed = (format.Format&SampleFormat.Signed)==0 ? 0 : 1;
      do
      { info = file->Info+curLink; // TODO: test with nonseekable streams! (this may be illegal!!)
        if(info->Rate!=format.Frequency || info->Channels!=format.Channels)
        { Link link = links[curLink];
          int   len = Math.Min(length, 16384);
          toRead = (int)((long)len*link.CVT.lenDiv/link.CVT.lenMul);
          len = Math.Max(toRead, len);
          if(cvtBuf==null || cvtBuf.Length<len) cvtBuf = new byte[len];
          fixed(byte* cvtbuf = cvtBuf)
          { read = Ogg.Read(file, cvtbuf, toRead, be, format.SampleSize, signed, out section);
            Ogg.Check(read);
            if(read==0) break;
            link.CVT.buf = cvtbuf;
            link.CVT.len = read;
            link.CVT.CalcLenCvt();
            GLMixer.Check(GLMixer.Convert(ref link.CVT));
            read=link.CVT.lenCvt;
            GameLib.Interop.Unsafe.Copy(cvtbuf, dest+index, read);
          }
        }
        else
        { CheckVorbisFile(file);
          read = Ogg.Read(file, dest+index, length, be, format.SampleSize, signed, out section);
          CheckVorbisFile(file);
          Ogg.Check(read);
          if(read==0) break;
        }

        curLink = section;
        total += read; index += read; length -= read;
      } while(length>0);
      curPos += total/format.FrameSize;
      return total;
    }
  }
  
  protected class Link
  { public Link(AudioFormat format, GLMixer.AudioCVT cvt, int realLen, int cvtLen)
    { Format=format; CVT=cvt; RealLen=realLen; CvtLen=cvtLen;
    }
    public AudioFormat Format;
    public GLMixer.AudioCVT CVT;
    public int RealLen, CvtLen;
  }

  protected void Dispose(bool finalizing)
  { if(open)
      lock(this)
      { base.Dispose();
        unsafe { Ogg.Close(file); file=null; }
        cvtBuf = null;
        open = false;
      }
  }

  unsafe void Init(bool autoClose, AudioFormat format)
  { if(CanSeek) // TODO: do more testing with nonseekable streams
    { length = 0;
      links  = new Link[file->NumLinks];
      GLMixer.AudioCVT cvt;
      AudioFormat sf;
      int  len;
      bool grr=false;
      for(int i=0; i<file->NumLinks; i++)
        unsafe
        { Ogg.VorbisInfo* info = file->Info+i;
          sf  = new AudioFormat((uint)info->Rate, format.Format, (byte)info->Channels);
          if(!sf.Equals(format)) grr=true;
          cvt = Audio.SetupCVT(sf, format);
          len = Ogg.PcmLength(file, i);
          Ogg.Check(len);
          links[i] = new Link(sf, cvt, len, (int)((long)len*cvt.lenMul/cvt.lenDiv));
          length += links[i].CvtLen;
        }
      if(!grr) links=null; // yay, it's all in the right format
    }
    this.format = format;
    open = true;
  }
  
  unsafe Ogg.VorbisFile* file;
  VorbisCallbacks  calls;
  protected byte[] cvtBuf;
  protected int    curLink;
  protected Link[] links;
  protected bool   open;
}
#endregion

#region Channel class
public sealed class Channel
{ internal Channel(int channel) { number=channel; Reset(); }
  
  public event MixFilter Filters;
  public event ChannelFinishedHandler Finished;

  public uint Age { get { return source==null ? 0 : Timing.Msecs-startTime; } }

  public int Both
  { get { return left; }
    set { Audio.CheckVolume(value); left=right=value; }
  }

  public Fade Fading { get { return fade; } }

  public int Left
  { get { return left; }
    set { Audio.CheckVolume(value); left=value; }
  }

  public int Number { get { return number; } }
  public bool Paused { get { return paused; } set { paused=value; } }
  public float PlaybackRate { get { return rate; } set { Audio.CheckRate(rate); lock(this) rate=value; } }
  public bool Playing { get { return Status==ChannelStatus.Playing; } }
  public int Position { get { return position; } set { lock(this) position=value; } }
  public int Priority { get { return priority; } }

  public int Right
  { get { return right; }
    set { Audio.CheckVolume(value); right=value; }
  }

  public AudioSource Source { get { return source; } }

  public ChannelStatus Status
  { get { return source==null ? ChannelStatus.Stopped : paused ? ChannelStatus.Paused : ChannelStatus.Playing; }
  }

  public bool Stopped { get { return Status==ChannelStatus.Stopped; } }

  public void FadeOut(uint fadeMs)
  { lock(this)
    { if(source==null) return;
      fade      = Fade.Out;
      fadeTime  = fadeMs;
      fadeStart = Timing.Msecs;
      fadeLeft  = EffectiveLeft;
      fadeRight = EffectiveRight;
    }
  }

  public void GetVolume(out int left, out int right) { left=this.left; right=this.right; }
  public void SetVolume(int left, int right) { Left=left; Right=right; }

  public void Pause()  { paused=true; }
  public void Resume() { paused=false; }
  public void Stop()   { lock(this) StopPlaying(); }

  internal void Reset() { rate=1f; left=Audio.MaxVolume; right=Audio.MaxVolume; }

  internal void StartPlaying(AudioSource source, int loops, int position, Fade fade, uint fadeMs, int timeoutMs)
  { StopPlaying();
    lock(source)
    { if(!source.CanRewind && loops!=0) throw new ArgumentException("Can't play loop sources that can't be rewound");
      if(!source.CanSeek && source.playing>0)
        throw new ArgumentException("Can't play unseekable streams more than once at a time");
      this.source   = source;
      this.loops    = loops;
      this.fade     = fade;
      this.timeout  = timeoutMs;
      this.position = position;
      if(!source.CanSeek && source.CanRewind && position==0) source.Rewind();
      priority  = source.Priority;
      paused    = false;
      startTime = Timing.Msecs;
      source.playing++;
      convert = !source.Format.Equals(Audio.Format);
      if(convert)
      { GLMixer.AudioCVT cvt = Audio.SetupCVT(source.Format, Audio.Format);
        sdMul=cvt.lenMul; sdDiv=cvt.lenDiv;
      }
      else convBuf=null;
      if(fade!=Fade.None)
      { fadeTime  = fadeMs;
        fadeLeft  = fade==Fade.In ? 0 : EffectiveLeft;
        fadeRight = fade==Fade.In ? 0 : EffectiveRight;
        fadeStart = Timing.Msecs;
      }
    }
  }

  internal void StopPlaying()
  { if(source==null) return;
    lock(source)
    { if(Filters!=null) unsafe { Filters(this, null, 0, Audio.Format); }
      Audio.OnFiltersFinished(this);
      if(Finished!=null) Finished(this);
      Audio.OnChannelFinished(this);
      source = null;
    }
  }

  internal unsafe void Mix(int* stream, int frames, MixFilter filters)
  { if(source==null || paused) return;
    lock(source)
    { if(source.Length==0) return;
      AudioFormat format = source.Format;
      float rate = EffectiveRate;
      int   left = EffectiveLeft, right=EffectiveRight, read, toRead, samples;
      bool  convert = this.convert;

      if(timeout!=Audio.Infinite && Age>timeout)
      { StopPlaying();
        return;
      }
      if(fade!=Fade.None)
      { uint fadeSoFar = Timing.Msecs-fadeStart;
        int  ltarg, rtarg;
        if(fade==Fade.In) { ltarg=left; rtarg=right; }
        else ltarg=rtarg=0;
        if(fadeSoFar>fadeTime)
        { if(fade==Fade.Out) { StopPlaying(); return; }
          left  = ltarg;
          right = rtarg;
          fade  = Fade.None;
        }
        else
        { left  = fadeLeft  + (ltarg-fadeLeft )*(int)fadeSoFar/(int)fadeTime;
          right = fadeRight + (rtarg-fadeRight)*(int)fadeSoFar/(int)fadeTime;
        }
      }

      if(source.CanSeek) source.Position = position;
      if(convert || rate!=1f) // FIXME: this path can result in 'toRead' not being a frame multiple!!!
      { int index=0, mustWrite = frames*Audio.Format.FrameSize, framesRead;
        bool stop=false;
        if(rate==1f) toRead = (int)((long)mustWrite*sdDiv/sdMul);
        else
        { int shift = format.FrameSize>>1;
          format.Frequency = (uint)(format.Frequency*rate);
          if(format.Frequency==0) return;
          convert = true;
          GLMixer.AudioCVT cvt = Audio.SetupCVT(format, Audio.Format);
          toRead = (int)((long)mustWrite*cvt.lenDiv/cvt.lenMul+shift)>>shift<<shift;
        }
        
        // FIXME: HACK: this hacks the 'toRead' not frame multiple problem by duplicating samples
        // this means it's possible that a few samples may be added each buffer fill. this is not correct output!
        if(format.FrameSize>1)
        { int diff = toRead & (format.FrameSize-1);
          if(diff>0) toRead += format.FrameSize-diff;
        }

        int len = Math.Max(toRead, mustWrite);
        if(convBuf==null || convBuf.Length<len) convBuf = new byte[len];

        while(index<toRead)
        { read = source.ReadBytes(convBuf, index, toRead-index);
          if(read==0)
          { if(loops==0)
            { Array.Clear(convBuf, index, toRead-index);
              stop = true;
              break;
            }
            else
            { source.Rewind();
              position = source.Position;
              if(loops!=Audio.Infinite) loops--;
            }
          }
          index += read;
        }
        SizedArray sa = Audio.Convert(convBuf, format, Audio.Format, toRead, mustWrite);
        if(sa.Array!=convBuf) convBuf = sa.Array;
        framesRead = sa.Length/Audio.Format.FrameSize;
        samples    = framesRead*Audio.Format.Channels;
        if(Filters==null && filters==null)
          fixed(byte* src = convBuf)
            GLMixer.Check(GLMixer.ConvertMix(stream, src, (uint)samples, (ushort)Audio.Format.Format,
                                             (ushort)left, (ushort)right));
        else
        { int* buffer = stackalloc int[samples];
          fixed(byte* src = convBuf)
            GLMixer.Check(GLMixer.ConvertMix(buffer, src, (uint)samples,
                                             (ushort)Audio.Format.Format,
                                             (ushort)Audio.MaxVolume, (ushort)Audio.MaxVolume));
          if(Filters!=null) Filters(this, buffer, framesRead, Audio.Format);
          if(filters!=null) filters(this, buffer, framesRead, Audio.Format);
          GLMixer.Check(GLMixer.Mix(stream, buffer, (uint)samples, (ushort)left, (ushort)right));
        }
        
        if(stop) { StopPlaying(); return; }
      }
      else
      { toRead=frames;
        while(true)
        { if(Filters==null && filters==null)
          { read=source.ReadFrames(stream, toRead, left, right);
            samples=read*Audio.Format.Channels;
          }
          else
          { int* buffer = stackalloc int[toRead];
            read    = source.ReadFrames(buffer, toRead, -1);
            samples = read*Audio.Format.Channels;
            if(read>0)
            { if(Filters!=null) Filters(this, buffer, read, format);
              if(filters!=null) filters(this, buffer, read, format);
              GLMixer.Check(GLMixer.Mix(stream, buffer, (uint)samples, (ushort)left, (ushort)right));
            }
          }
          toRead -= read;
          if(toRead<=0) break;
          stream += samples;
          if(read==0)
          { if(loops==0) { StopPlaying(); return; }
            else
            { source.Rewind();
              position = source.Position;
              if(loops!=Audio.Infinite) loops--;
            }
          }
        }
      }
      position = source.Position;
    }
  }

  int   EffectiveLeft  { get { int v=source.Left;  return v==Audio.MaxVolume ? left  : (left *v)>>8; } }
  int   EffectiveRight { get { int v=source.Right; return v==Audio.MaxVolume ? right : (right*v)>>8; } }
  float EffectiveRate  { get { return source.PlaybackRate*rate; } }

  AudioSource source;
  byte[] convBuf;
  float rate=1f;
  uint startTime, fadeStart, fadeTime;
  int  left=Audio.MaxVolume, right=Audio.MaxVolume, fadeLeft, fadeRight;
  int  timeout, number, position, loops, priority, sdMul, sdDiv;
  Fade fade;
  bool paused, convert;
}
#endregion

#region Audio class
public class Audio
{ private Audio() { }

  public const int Infinite=-1, FreeChannel=-1, MaxVolume=256;

  public static event MixFilter Filters
  { add    { lock(callback) eFilters += value; }
    remove { lock(callback) eFilters -= value; }
  }
  public static event MixFilter PostFilters
  { add    { lock(callback) ePostFilters += value; }
    remove { lock(callback) ePostFilters -= value; }
  }
  public static event ChannelFinishedHandler ChannelFinished;

  public static bool Initialized { get { return init; } }
  public static AudioFormat Format { get { AssertInit(); return format; } }
  public static object SyncRoot { get { AssertInit(); return callback; } }

  public static int MasterVolume
  { get { AssertInit(); return (int)GLMixer.GetMixVolume(); }
    set { AssertInit(); CheckVolume(value); GLMixer.SetMixVolume((ushort)value); }
  }
  public static int ReservedChannels
  { get { AssertInit(); return reserved; }
    set
    { AssertInit();
      if(reserved<0 || reserved>chans.Length) throw new ArgumentOutOfRangeException("value");
      reserved=value;
    }
  }
  public static Channel[] Channels { get { return chans; } }
  public static PlayPolicy PlayPolicy { get { return playPolicy; } set { playPolicy=value; } }
  public static MixPolicy  MixPolicy  { get { return mixPolicy; } set { mixPolicy=value; } }

  public static bool Initialize() { return Initialize(22050, SampleFormat.Default, Speakers.Stereo, 100); }
  public static bool Initialize(uint frequency) { return Initialize(frequency, SampleFormat.Default, Speakers.Stereo, 100); }
  public static bool Initialize(uint frequency, SampleFormat format) { return Initialize(frequency, format, Speakers.Stereo, 100); }
  public static bool Initialize(uint frequency, SampleFormat format, Speakers chans) { return Initialize(frequency, format, chans, 100); }
  public unsafe static bool Initialize(uint frequency, SampleFormat format, Speakers chans, uint bufferMs)
  { if(init) throw new InvalidOperationException("Already initialized. Deinitialize first to change format");
    callback    = new GLMixer.MixCallback(FillBuffer);
    Audio.chans = new Channel[0];
    groups      = new ArrayList();
    SDL.Initialize(SDL.InitFlag.Audio);
    init        = true;

    try { GLMixer.Check(GLMixer.Init(frequency, (ushort)format, (byte)chans, bufferMs, callback, new IntPtr(null))); }
    catch(Exception e) { Deinitialize(); throw e; }
    
    uint freq, bytes;
    ushort form;
    byte   chan;
    GLMixer.Check(GLMixer.GetFormat(out freq, out form, out chan, out bytes));
    Audio.format = new AudioFormat(freq, (SampleFormat)form, chan);
    
    SDL.PauseAudio(0);
    return freq==frequency && form==(short)format && chan==(byte)chans;
  }
  
  public static void Deinitialize()
  { if(init)
    { lock(callback)
      { Stop();
        if(ePostFilters!=null) unsafe { ePostFilters(null, null, 0, Format); }
        GLMixer.Quit();
        SDL.Deinitialize(SDL.InitFlag.Audio);
        callback = null;
        chans    = null;
        groups   = null;
        init     = false;
      }
    }
  }
  
  public static void AllocateChannels(int numChannels) { AllocateChannels(numChannels, true); }
  public static void AllocateChannels(int numChannels, bool resetChannels)
  { AssertInit();
    if(numChannels<0) throw new ArgumentOutOfRangeException("numChannels");
    lock(callback)
    { for(int i=numChannels; i<chans.Length; i++)
      { chans[i].Stop();
        if(resetChannels) chans[i].Reset();
      }
      Channel[] narr = new Channel[numChannels];
      Array.Copy(narr, chans, chans.Length);
      for(int i=chans.Length; i<numChannels; i++) narr[i] = new Channel(i);
      chans = narr;
      if(numChannels<reserved) reserved=numChannels;
    }
  }

  public static int AddGroup()
  { AssertInit();
    lock(groups)
    { for(int i=0; i<groups.Count; i++)
        if(groups[i]==null)
        { groups[i] = new ArrayList();
          return i;
        }
      return groups.Add(new ArrayList());
    }
  }
  
  public static void RemoveGroup(int group) { lock(groups) { GetGroup(group); groups[ToGroup(group)]=null; } }

  public static void GroupChannel(int channel, int group)
  { CheckChannel(channel);
    lock(groups)
    { ArrayList list = GetGroup(group);
      if(!list.Contains(channel)) list.Add(channel);
    }
  }

  public static void GroupRange(int start, int end, int group)
  { CheckChannel(start); CheckChannel(end);
    if(start>end) throw new ArgumentException("start should be <= end");
    lock(groups)
    { ArrayList list = GetGroup(group);
      for(; start<=end; start++) if(!list.Contains(start)) list.Add(start);
    }
  }
  
  public static void UngroupChannel(int channel, int group) { lock(groups) GetGroup(group).Remove(channel); }
  
  public static int GroupSize(int group) { lock(groups) return GetGroup(group).Count; }

  public static int[] GetGroupChannels(int group) { lock(groups) return (int[])GetGroup(group).ToArray(typeof(int)); }

  public static int OldestChannel(bool unreserved) { return OldestChannel(-1, unreserved); }
  public static int OldestChannel(int group, bool unreserved)
  { int  i,oi;
    uint age=0;
    if(group==-1)
    { AssertInit();
      lock(callback)
        for(oi=unreserved ? 0 : reserved,i=oi; i<chans.Length; i++) if(chans[i].Age>age) { age=chans[i].Age; oi=i; }
    }
    else
      lock(groups)
      { ArrayList list = GetGroup(group);
        for(oi=i=0; i<list.Count; i++)
        { int chan = (int)list[i];
          if(!unreserved && chan<reserved) continue;
          if(chans[chan].Age>age) { age=chans[chan].Age; oi=chan; }
        }
      }
    return oi;
  }

  public static void FadeOut(uint fadeMs) { FadeOut(-1, fadeMs); }
  public static void FadeOut(int group, uint fadeMs)
  { if(group==-1) lock(callback) for(int i=0; i<chans.Length; i++) chans[i].FadeOut(fadeMs);
    else
      lock(groups)
      { ArrayList list = GetGroup(group);
        for(int i=0; i<list.Count; i++) chans[(int)list[i]].FadeOut(fadeMs);
      }
  }

  public static void Pause() { Pause(-1); }
  public static void Pause(int group)
  { if(group==-1) lock(callback) for(int i=0; i<chans.Length; i++) chans[i].Pause();
    else
      lock(groups)
      { ArrayList list = GetGroup(group);
        for(int i=0; i<list.Count; i++) chans[(int)list[i]].Pause();
      }
  }

  public static void Resume() { Resume(-1); }
  public static void Resume(int group)
  { if(group==-1) lock(callback) for(int i=0; i<chans.Length; i++) chans[i].Resume();
    else
      lock(groups)
      { ArrayList list = GetGroup(group);
        for(int i=0; i<list.Count; i++) chans[(int)list[i]].Resume();
      }
  }

  public static void Stop() { Stop(-1); }
  public static void Stop(int group)
  { if(group==-1) lock(callback) for(int i=0; i<chans.Length; i++) chans[i].Stop();
    else
      lock(groups)
      { ArrayList list = GetGroup(group);
        for(int i=0; i<list.Count; i++) chans[(int)list[i]].Stop();
      }
  }

  public static GLMixer.AudioCVT SetupCVT(AudioFormat srcFormat, AudioFormat destFormat)
  { GLMixer.AudioCVT cvt = new GLMixer.AudioCVT();
    if(srcFormat.Equals(destFormat)) { cvt.lenMul=cvt.lenDiv=1; }
    cvt.srcRate    = (int)srcFormat.Frequency;
    cvt.srcFormat  = (ushort)srcFormat.Format;
    cvt.srcChans   = srcFormat.Channels;
    cvt.destRate   = (int)destFormat.Frequency;
    cvt.destFormat = (ushort)destFormat.Format;
    cvt.destChans  = destFormat.Channels;
    GLMixer.Check(GLMixer.SetupCVT(ref cvt));
    return cvt;
  }

  public static SizedArray Convert(byte[] srcData, AudioFormat srcFormat, SampleFormat destFormat)
  { return Convert(srcData, srcFormat, destFormat, -1);
  }
  public static SizedArray Convert(byte[] srcData, AudioFormat srcFormat, SampleFormat destFormat, int length)
  { AudioFormat df = srcFormat;
    df.Format = destFormat;
    return Convert(srcData, srcFormat, df, length, -1);
  }
  public static SizedArray Convert(byte[] srcData, AudioFormat srcFormat, AudioFormat destFormat)
  { return Convert(srcData, srcFormat, destFormat, -1, -1);
  }
  public static SizedArray Convert(byte[] srcData, AudioFormat srcFormat, AudioFormat destFormat, int length)
  { return Convert(srcData, srcFormat, destFormat, -1, -1);
  }
  public static SizedArray Convert(byte[] srcData, AudioFormat srcFormat, AudioFormat destFormat,
                                     int length, int mustWrite)
  { if(srcFormat.Equals(destFormat)) return new SizedArray(srcData);
    unsafe
    { GLMixer.AudioCVT cvt = new GLMixer.AudioCVT();
      if(length<0) length = srcData.Length;
      cvt.srcRate    = (int)srcFormat.Frequency;
      cvt.srcFormat  = (ushort)srcFormat.Format;
      cvt.srcChans   = srcFormat.Channels;
      cvt.destRate   = (int)destFormat.Frequency;
      cvt.destFormat = (ushort)destFormat.Format;
      cvt.destChans  = destFormat.Channels;
      cvt.len        = length;

      GLMixer.Check(GLMixer.SetupCVT(ref cvt));
      if(mustWrite!=-1) cvt.lenCvt=mustWrite;

      if(srcData.Length>=cvt.lenCvt)
      { fixed(byte* buf = srcData)
        { cvt.buf = buf;
          GLMixer.Check(GLMixer.Convert(ref cvt));
        }
        return new SizedArray(srcData, cvt.lenCvt);
      }
      else
      { byte[] ret = new byte[cvt.lenCvt];
        Array.Copy(srcData, ret, length);
        fixed(byte* buf = ret)
        { cvt.buf = buf;
          GLMixer.Check(GLMixer.Convert(ref cvt));
        }
        return new SizedArray(ret, cvt.lenCvt);
      }
    }
  }
  
  internal static void OnFiltersFinished(Channel channel)
  { if(eFilters!=null) lock(callback) unsafe { eFilters(channel, null, 0, Format); }
  }

  internal static void OnChannelFinished(Channel channel)
  { if(ChannelFinished!=null) ChannelFinished(channel);
  }
  
  internal static Channel StartPlaying(int channel, AudioSource source, int loops, int position, Fade fade, uint fadeMs, int timeoutMs)
  { AssertInit();
    if(reserved==chans.Length) return null;

    IList group=null;
    bool  tried=false;
    do
    { if(channel==FreeChannel)
      { for(int i=reserved; i<chans.Length; i++)
          if(chans[i].Status==ChannelStatus.Stopped) // try to lock as little as possible
          { tried=true;
            lock(chans[i])
              if(chans[i].Status==ChannelStatus.Stopped)
              { chans[i].StartPlaying(source, loops, position, fade, fadeMs, timeoutMs);
                return chans[i];
              }
          }
      }
      else
        lock(groups)
        { group = GetGroup(channel);
          for(int i=0; i<group.Count; i++)
          { int chan = (int)group[i];
            if(chan<reserved) continue;
            if(chans[chan].Status==ChannelStatus.Stopped) // try to lock as little as possible
            { tried=true;
              lock(chans[chan])
                if(chans[chan].Status==ChannelStatus.Stopped)
                { chans[chan].StartPlaying(source, loops, position, fade, fadeMs, timeoutMs);
                  return chans[chan];
                }
            }
          }
        }
    } while(!tried);

    switch(playPolicy)
    { case PlayPolicy.Oldest:
        lock(callback)
        { int  oi=reserved;
          uint age=0;
          if(channel==FreeChannel)
            for(int i=reserved; i<chans.Length; i++) { if(chans[i].Age>age) { age=chans[i].Age; oi=i; } }
          else
            lock(groups)
              for(int i=0; i<group.Count; i++)
              { int chan = (int)group[i];
                if(chan<reserved) continue;
                if(chans[chan].Age>age) { age=chans[chan].Age; oi=chan; }
              }
          lock(chans[oi]) chans[oi].StartPlaying(source, loops, position, fade, fadeMs, timeoutMs);
          return chans[oi];
        }
      case PlayPolicy.Priority:
        lock(callback)
        { int pi=reserved, prio=int.MaxValue;
          if(channel==FreeChannel)
            for(int i=reserved; i<chans.Length; i++) { if(chans[i].Priority<prio) { prio=chans[i].Priority; pi=i; } }
          else
            lock(groups)
              for(int i=0; i<group.Count; i++)
              { int chan = (int)group[i];
                if(chan<reserved) continue;
                if(chans[chan].Priority<prio) { prio=chans[chan].Priority; pi=chan; }
              }
          lock(chans[pi]) chans[pi].StartPlaying(source, loops, position, fade, fadeMs, timeoutMs);
          return chans[pi];
        }
      case PlayPolicy.OldestPriority:
        lock(callback)
        { int  pi=reserved, oi, prio=int.MaxValue;
          uint age=0;
          if(channel==FreeChannel)
          { for(int i=reserved; i<chans.Length; i++) if(chans[i].Priority<prio) { prio=chans[i].Priority; pi=i; }
            oi=pi;
            for(int i=reserved; i<chans.Length; i++)
              if(chans[i].Priority==prio && chans[i].Age>age) { oi=i; age=chans[i].Age; }
          }
          else
            lock(groups)
            { for(int i=0; i<group.Count; i++)
              { int chan = (int)group[i];
                if(chan<reserved) continue;
                if(chans[chan].Priority<prio) { prio=chans[chan].Priority; pi=chan; }
              }
              oi=pi;
              for(int i=0; i<group.Count; i++)
              { int chan = (int)group[i];
                if(chan<reserved) continue;
                if(chans[chan].Priority==prio && chans[chan].Age>age) { oi=chan; age=chans[chan].Age; }
              }
            }
          lock(chans[oi]) chans[oi].StartPlaying(source, loops, position, fade, fadeMs, timeoutMs);
          return chans[oi];
        }
      default: return null;
    }
  }

  internal static void CheckRate(float rate)
  { if(rate<0f) throw new ArgumentOutOfRangeException("PlaybackRate");
  }
  internal static void CheckChannel(int channel)
  { AssertInit();
    if(channel!=FreeChannel && (channel<0 || channel>=chans.Length)) throw new ArgumentOutOfRangeException("channel");
  }
  internal static void CheckVolume(int volume)
  { if(volume<0 || volume>Audio.MaxVolume) throw new ArgumentOutOfRangeException("value");
  }

  static int ToGroup(int group) { return -group-2; }
  static ArrayList GetGroup(int group)
  { AssertInit();
    int index = ToGroup(group);
    if(index<0 || index>=groups.Count) throw new ArgumentException("Invalid group ID");
    ArrayList list = (ArrayList)groups[ToGroup(group)];
    if(list==null) throw new ArgumentException("Invalid group ID");
    return list;
  }

  static void AssertInit()
  { if(!init) throw new InvalidOperationException("Audio has not been initialized.");
  }
  static unsafe void FillBuffer(int* stream, uint frames, IntPtr context)
  { try
    { lock(callback)
      { for(int i=0; i<chans.Length; i++) lock(chans[i]) chans[i].Mix(stream, (int)frames, eFilters);
        if(ePostFilters!=null) ePostFilters(null, stream, (int)frames, format);
        if(MixPolicy==MixPolicy.Divide) GLMixer.Check(GLMixer.DivideAccumulator(chans.Length));
      }
    }
    catch(Exception e)
    { if(Events.Events.Initialized)
        try { Events.Events.PushEvent(new Events.ExceptionEvent(Events.ExceptionLocation.AudioThread, e)); }
        catch { }
    }
  }

  static AudioFormat format;
  static GLMixer.MixCallback callback;
  static Channel[] chans;
  static ArrayList groups;
  static MixFilter eFilters, ePostFilters;
  static int reserved;
  static PlayPolicy playPolicy = PlayPolicy.Fail;
  static MixPolicy  mixPolicy  = MixPolicy.DontDivide;
  static bool init;
}
#endregion

} // namespace GameLib.Audio