using System;
using System.IO;
using System.Collections;
using GameLib.IO;
using GameLib.Interop.SDL;
using GameLib.Interop.GLMixer;
using GameLib.Interop.SndFile;
using GameLib.Interop.OggVorbis;

using System.Runtime.InteropServices; // TODO: remove me

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
public enum AudioStatus { Stopped, Playing, Paused }
public enum Fade { None, In, Out }
public enum PlayPolicy { Fail, Oldest, Priority, OldestPriority }
public enum MixPolicy  { DontDivide, Divide }

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
{ public abstract bool CanRewind { get; }
  public abstract bool CanSeek   { get; }
  public abstract int  Position  { get; set; }
  public AudioFormat Format { get { return format; } }
  public int Length   { get { return length; } }
  public int Priority { get { return priority; } set { priority=value; } }
  
  public virtual void Dispose() { buffer=null; }

  public int Volume
  { get { return volume; }
    set { Audio.CheckVolume(value); volume=value; }
  }
  public float PlaybackRate { get { return rate; } set { Audio.CheckRate(rate); lock(this) rate=value; } }

  public virtual void Rewind() { Position=0; }

  public int Play() { return Play(0, Audio.Infinite, Audio.FreeChannel); }
  public int Play(int loops) { return Play(loops, Audio.Infinite, Audio.FreeChannel); }
  public int Play(int loops, int timeoutMs) { return Play(loops, timeoutMs, Audio.FreeChannel); }
  public int Play(int loops, int timeoutMs, int channel)
  { if(channel<0) return Audio.StartPlaying(channel, this, loops, Fade.None, 0, timeoutMs);
    else
      lock(Audio.Channels[channel])
      { Audio.Channels[channel].StartPlaying(this, loops, Fade.None, 0, timeoutMs);
        return channel;
      }
  }

  public int FadeIn(uint fadeMs) { return FadeIn(fadeMs, 0, Audio.FreeChannel, Audio.Infinite); }
  public int FadeIn(uint fadeMs, int loops) { return FadeIn(fadeMs, loops, Audio.FreeChannel, Audio.Infinite); }
  public int FadeIn(uint fadeMs, int loops, int timeoutMs) { return FadeIn(fadeMs, loops, timeoutMs, Audio.FreeChannel); }
  public int FadeIn(uint fadeMs, int loops, int timeoutMs, int channel)
  { if(channel<0) return Audio.StartPlaying(channel, this, loops, Fade.In, fadeMs, timeoutMs);
    else
      lock(Audio.Channels[channel])
      { Audio.Channels[channel].StartPlaying(this, loops, Fade.In, fadeMs, timeoutMs);
        return channel;
      }
  }
  
  protected void SizeBuffer(int size) { if(buffer==null || buffer.Length<size) buffer=new byte[size]; }

  protected int BytesToFrames(int bytes)
  { int frames = bytes/format.FrameSize;
    if(frames*format.FrameSize != bytes) throw new ArgumentException("length must be multiple of the frame size");
    return frames;
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

  public virtual byte[] ReadAll()
  { lock(this)
    { byte[] ret;
      int pos=0;
      if(CanSeek)
      { pos = Position;
        Rewind();
      }
      if(Length>=0)
      { ret = new byte[Length*format.FrameSize];
        if(ReadBytes(ret, ret.Length)!=ret.Length) throw new EndOfStreamException();
      }
      else ret=ReadAllUL();
      if(CanSeek) Position=pos;
      return ret;
    }
  }

  public int ReadBytes(byte[] buf, int length) { return ReadBytes(buf, 0, length); }
  public abstract int ReadBytes(byte[] buf, int index, int length);

  public unsafe int ReadFrames(int* dest, int frames) { return ReadFrames(dest, frames, -1); }
  public virtual unsafe int ReadFrames(int* dest, int frames, int volume)
  { lock(this)
    { int toRead=Math.Min(length-curPos, frames), read, samples;
      SizeBuffer(toRead);
      read    = ReadBytes(buffer, toRead*format.FrameSize);
      samples = read/format.SampleSize;
      fixed(byte* src = buffer)
        GLMixer.Check(GLMixer.ConvertMix(dest, src, (uint)samples, (ushort)format.Format,
                                          (ushort)(volume<0 ? Audio.MaxVolume : volume)));
      read = format.Channels==1 ? samples : samples/2;
      curPos += read;
      return read;
    }
  }

  protected byte[] buffer;
  protected AudioFormat format;
  protected float rate=1f;
  protected int   volume=Audio.MaxVolume, curPos=0, length=-1, priority;
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
    this.length=length/format.FrameSize;
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

  protected void Dispose(bool destructor)
  { unsafe
    { if(sndfile.ToPointer()!=null)
      { base.Dispose();
        SF.Close(sndfile);
        sndfile = new IntPtr(null);
      }
    }
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
  
  IntPtr sndfile;
  StreamIOCalls calls;
}
#endregion

#region SampleSource
public class SampleSource : AudioSource
{ public override bool CanRewind { get { return true; } }
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

  public override void Dispose() { base.Dispose(); data=null; }

  public SampleSource(AudioSource stream) : this(stream, true) { }
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

  public override byte[] ReadAll() { return (byte[])data.Clone(); }

  public override int ReadBytes(byte[] buf, int index, int length)
  { int frames = BytesToFrames(length);
    lock(this)
    { int toRead = Math.Min(frames, this.length-curPos);
      Array.Copy(data, curPos, buf, index, toRead*format.FrameSize);
      curPos += toRead;
      return toRead;
    }
  }

  public override unsafe int ReadFrames(int *dest, int frames, int volume)
  { lock(this)
    { int toRead=Math.Min(length-curPos, frames), samples=toRead*format.Channels;
      fixed(byte* src = data)
        GLMixer.Check(GLMixer.ConvertMix(dest, src+curPos*format.FrameSize, (uint)samples, (ushort)format.Format,
                                          (ushort)(volume<0 ? Audio.MaxVolume : volume)));
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
  public VorbisSource(Stream stream, bool autoClose)
  { calls = new VorbisCallbacks(stream, autoClose);
    unsafe { fixed(Ogg.Callbacks* io = &calls.calls) Ogg.Check(Ogg.Open(out file, io)); }
    if(file.NumLinks>1) throw new NotImplementedException("Support for chained oggs is not supported yet!");
    length = Ogg.PcmLength(ref file, -1)/2; // FIXME: this isn't correct!!
    format = new AudioFormat(44100, SampleFormat.S16Sys, 2);
    open   = true;
  }
  ~VorbisSource() { Dispose(true); }

  public override void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  public override bool CanRewind { get { return file.Seekable!=0; } }
  public override bool CanSeek   { get { return file.Seekable!=0; } }
  public override int  Position
  { get { return curPos; }
    set
    { if(value==curPos) return;
      if(!CanSeek) Ogg.Check((int)Ogg.OggError.NoSeek);
      lock(this)
      { Ogg.Check(Ogg.PcmSeek(ref file, value*2)); // FIXME: this isn't correct!
        curPos = value*2; // FIXME: neither is this!
      }
    }
  }

  public unsafe override int ReadBytes(byte[] buf, int index, int length)
  { BytesToFrames(length);
    fixed(byte* dest = buf)
    { Ogg.VorbisInfo* info;
      int oldsection, section=-1;
      int read, total=0, be=(format.Format&SampleFormat.BigEndian)==0 ? 0 : 1;
      read = Ogg.Read(ref file, dest+index, length, be, 2, 1, out oldsection);
      info = Ogg.GetInfo(ref file, oldsection);
      while(read>0)
      { if(info->Rate!=44100 || info->Channels!=2)
        { throw new NotImplementedException("ogg resampling not supported");
          read=read; // something else
        }
        total += read; index += read; length -= read;
        if(length==0) break;
        read = Ogg.Read(ref file, dest+index, length, be, 2, 1, out section);
        if(section!=oldsection) info = Ogg.GetInfo(ref file, oldsection=section);
      }
      return total;
    }
  }
  
  protected void Dispose(bool destructor)
  { if(open)
    { base.Dispose();
      Ogg.Close(ref file);
      open=false;
    }
  }
  
  Ogg.VorbisFile  file = new Ogg.VorbisFile();
  VorbisCallbacks calls;
  protected bool  open;
}
#endregion

#region Channel class
public sealed class Channel
{ internal Channel(int channel) { number=channel; volume=Audio.MaxVolume; }
  
  public event MixFilter Filters;
  public event ChannelFinishedHandler Finished;

  public int Number { get { return number; } }
  public int Volume
  { get { return volume; }
    set { Audio.CheckVolume(value); volume = value; }
  }
  public AudioSource Source { get { return source; } }
  public int  Priority { get { return priority; } }
  public uint Age { get { return source==null ? 0 : Timing.Ticks-startTime; } }
  public AudioStatus Status
  { get { return source==null ? AudioStatus.Stopped : paused ? AudioStatus.Paused : AudioStatus.Playing; }
  }
  public Fade Fading { get { return fade; } }
  public int  Position { get { return position; } set { lock(this) position=value; } }
  public float PlaybackRate { get { return rate; } set { Audio.CheckRate(rate); lock(this) rate=value; } }

  public void Pause()  { paused=true; }
  public void Resume() { paused=false; }
  public void Stop()   { lock(this) StopPlaying(); }

  public void FadeOut(uint fadeMs)
  { lock(this)
    { if(source==null) return;
      fade        = Fade.Out;
      fadeTime    = fadeMs;
      fadeStart   = Timing.Ticks;
      fadeVolume  = EffectiveVolume;
    }
  }

  internal void StartPlaying(AudioSource source, int loops, Fade fade, uint fadeMs, int timeoutMs)
  { StopPlaying();
    lock(source)
    { if(!source.CanRewind && loops!=0) throw new ArgumentException("Can't play loop sources that can't be rewound");
      if(!source.CanSeek && source.playing>0)
        throw new ArgumentException("Can't play unseekable streams more than once at a time");
      this.source   = source;
      this.loops    = loops;
      this.fade     = fade;
      this.timeout  = timeoutMs;
      if(source.CanRewind) source.Rewind();
      priority  = source.Priority;
      position  = source.Position;
      paused    = false;
      startTime = Timing.Ticks;
      source.playing++;
      convert = !source.Format.Equals(Audio.Format);
      if(convert)
      { GLMixer.AudioCVT cvt = Audio.SetupCVT(source.Format, Audio.Format);
        sdMul=cvt.lenMul; sdDiv=cvt.lenDiv;
      }
      else convBuf=null;
      if(fade!=Fade.None)
      { fadeTime   = fadeMs;
        fadeVolume = fade==Fade.In ? 0 : EffectiveVolume;
        fadeStart  = Timing.Ticks;
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
      int volume = EffectiveVolume, read, toRead, samples;
      bool convert = this.convert;

      if(timeout!=Audio.Infinite && Age>timeout)
      { StopPlaying();
        return;
      }
      if(fade!=Fade.None)
      { uint fadeSoFar = Timing.Ticks-fadeStart;
        int  target = fade==Fade.In ? volume : 0;
        if(fadeSoFar>fadeTime)
        { volume = target;
          if(fade==Fade.Out) { StopPlaying(); return; }
          fade = Fade.None;
        }
        else volume = fadeVolume + (target-fadeVolume)*(int)fadeSoFar/(int)fadeTime;
      }

      if(source.CanSeek) source.Position = position;
      if(convert || rate!=1f)
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
        Audio.SizedArray sa = Audio.Convert(convBuf, format, Audio.Format, toRead, mustWrite);
        if(sa.Array!=convBuf) convBuf = sa.Array;
        framesRead = sa.Length/Audio.Format.FrameSize;
        samples    = framesRead*Audio.Format.Channels;
        if(Filters==null && filters==null)
          fixed(byte* src = convBuf)
                GLMixer.Check(GLMixer.ConvertMix(stream, src, (uint)samples,
                                                (ushort)Audio.Format.Format, (ushort)volume));
        else
        { int* buffer = stackalloc int[samples];
          fixed(byte* src = convBuf)
            GLMixer.Check(GLMixer.ConvertMix(buffer, src, (uint)samples,
                                            (ushort)Audio.Format.Format, (ushort)Audio.MaxVolume));
          if(Filters!=null) Filters(this, buffer, framesRead, Audio.Format);
          if(filters!=null) filters(this, buffer, framesRead, Audio.Format);
          GLMixer.Check(GLMixer.Mix(stream, buffer, (uint)samples, (ushort)volume));
        }
        
        if(stop) { StopPlaying(); return; }
      }
      else
      { toRead=frames;
        while(true)
        { if(Filters==null && filters==null)
          { read=source.ReadFrames(stream, toRead, volume);
            samples=read*Audio.Format.Channels;
          }
          else
          { int* buffer = stackalloc int[toRead];
            read    = source.ReadFrames(buffer, toRead, -1);
            samples = read*Audio.Format.Channels;
            if(read>0)
            { if(Filters!=null) Filters(this, buffer, read, format);
              if(filters!=null) filters(this, buffer, read, format);
              GLMixer.Check(GLMixer.Mix(stream, buffer, (uint)samples, (ushort)volume));
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
  
  int   EffectiveVolume { get { int v=source.Volume; return v==Audio.MaxVolume ? volume : (volume*v)>>8; } }
  float EffectiveRate   { get { return source.PlaybackRate*rate; } }

  AudioSource source;
  byte[] convBuf;
  float rate=1f;
  uint startTime, fadeStart, fadeTime;
  int  volume, fadeVolume, timeout, number, position, loops, priority, sdMul, sdDiv;
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
  public static AudioFormat Format { get { CheckInit(); return format; } }
  public static object SyncRoot { get { CheckInit(); return callback; } }

  public static int MasterVolume
  { get { CheckInit(); return (int)GLMixer.GetMixVolume(); }
    set { CheckInit(); CheckVolume(value); GLMixer.SetMixVolume((ushort)value); }
  }
  public static int ReservedChannels
  { get { CheckInit(); return reserved; }
    set
    { CheckInit();
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
  
  public static void AllocateChannels(int numChannels)
  { CheckInit();
    if(numChannels<0) throw new ArgumentOutOfRangeException("numChannels");
    lock(callback)
    { for(int i=numChannels; i<chans.Length; i++) chans[i].Stop();
      Channel[] narr = new Channel[numChannels];
      Array.Copy(narr, chans, chans.Length);
      for(int i=chans.Length; i<numChannels; i++) narr[i] = new Channel(i);
      chans = narr;
      if(numChannels<reserved) reserved=numChannels;
    }
  }

  public static int AddGroup()
  { CheckInit();
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
    { CheckInit();
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
  
  internal static void OnFiltersFinished(Channel channel)
  { if(eFilters!=null) lock(callback) unsafe { eFilters(channel, null, 0, Format); }
  }

  internal static void OnChannelFinished(Channel channel)
  { if(ChannelFinished!=null) ChannelFinished(channel);
  }
  
  internal static int StartPlaying(int channel, AudioSource source, int loops, Fade fade, uint fadeMs, int timeoutMs)
  { CheckInit();
    if(reserved==chans.Length) return -1;

    IList group=null;
    bool  tried=false;
    do
    { if(channel==FreeChannel)
      { for(int i=reserved; i<chans.Length; i++)
          if(chans[i].Status==AudioStatus.Stopped) // try to lock as little as possible
          { tried=true;
            lock(chans[i])
              if(chans[i].Status==AudioStatus.Stopped)
              { chans[i].StartPlaying(source, loops, fade, fadeMs, timeoutMs);
                return i;
              }
          }
      }
      else
        lock(groups)
        { group = GetGroup(channel);
          for(int i=0; i<group.Count; i++)
          { int chan = (int)group[i];
            if(chan<reserved) continue;
            if(chans[chan].Status==AudioStatus.Stopped) // try to lock as little as possible
            { tried=true;
              lock(chans[chan])
                if(chans[chan].Status==AudioStatus.Stopped)
                { chans[chan].StartPlaying(source, loops, fade, fadeMs, timeoutMs);
                  return chan;
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
          lock(chans[oi]) chans[oi].StartPlaying(source, loops, fade, fadeMs, timeoutMs);
          return oi;
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
          lock(chans[pi]) chans[pi].StartPlaying(source, loops, fade, fadeMs, timeoutMs);
          return pi;
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
          lock(chans[oi]) chans[oi].StartPlaying(source, loops, fade, fadeMs, timeoutMs);
          return oi;
        }
      default: return -1;
    }
  }

  internal class SizedArray
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
  
  internal static void CheckRate(float rate)
  { if(rate<0f) throw new ArgumentOutOfRangeException("PlaybackRate");
  }
  internal static void CheckChannel(int channel)
  { CheckInit();
    if(channel!=FreeChannel && channel<0) throw new ArgumentOutOfRangeException("channel");
  }
  internal static void CheckVolume(int volume)
  { if(volume<0 || volume>Audio.MaxVolume) throw new ArgumentOutOfRangeException("value");
  }

  internal static GLMixer.AudioCVT SetupCVT(AudioFormat srcFormat, AudioFormat destFormat)
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

  internal static SizedArray Convert(byte[] srcData, AudioFormat srcFormat, SampleFormat destFormat)
  { return Convert(srcData, srcFormat, destFormat, -1);
  }
  internal static SizedArray Convert(byte[] srcData, AudioFormat srcFormat, SampleFormat destFormat, int length)
  { AudioFormat df = srcFormat;
    df.Format = destFormat;
    return Convert(srcData, srcFormat, df, length, -1);
  }
  internal static SizedArray Convert(byte[] srcData, AudioFormat srcFormat, AudioFormat destFormat)
  { return Convert(srcData, srcFormat, destFormat, -1, -1);
  }
  internal static SizedArray Convert(byte[] srcData, AudioFormat srcFormat, AudioFormat destFormat, int length)
  { return Convert(srcData, srcFormat, destFormat, -1, -1);
  }
  internal static SizedArray Convert(byte[] srcData, AudioFormat srcFormat, AudioFormat destFormat,
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

  static int ToGroup(int group) { return -group-2; }
  static ArrayList GetGroup(int group)
  { CheckInit();
    int index = ToGroup(group);
    if(index<0 || index>=groups.Count) throw new ArgumentException("Invalid group ID");
    ArrayList list = (ArrayList)groups[ToGroup(group)];
    if(list==null) throw new ArgumentException("Invalid group ID");
    return list;
  }

  static void CheckInit()
  { if(!init) throw new InvalidOperationException("Audio has not been initialized.");
  }
  static unsafe void FillBuffer(int* stream, uint frames, IntPtr context)
  { lock(callback)
    { for(int i=0; i<chans.Length; i++) lock(chans[i]) chans[i].Mix(stream, (int)frames, eFilters);
      if(ePostFilters!=null) ePostFilters(null, stream, (int)frames, format);
      if(MixPolicy==MixPolicy.Divide) GLMixer.Check(GLMixer.DivideAccumulator(chans.Length));
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