using System;
using System.IO;
using GameLib.IO;
using GameLib.Interop.SDL;
using GameLib.Interop.GLMixer;

namespace GameLib.Audio
{

// TODO: check return values for calls into GLMixer

[Flags]
public enum SampleFormat : ushort
{ Eight=GLMixer.Format.Eight, Sixteen=GLMixer.Format.Sixteen, BitsPart=GLMixer.Format.BitsPart,
  Signed=GLMixer.Format.Signed, BigEndian=GLMixer.Format.BigEndian,

  U8=Eight, U16=Sixteen, S8=Eight|Signed, S16=Sixteen|Signed,
  U8BE=U8|BigEndian, U16BE=U16|BigEndian, S8BE=S8|BigEndian, S16BE=S16|BigEndian,
  U8Sys=GLMixer.Format.U8Sys, U16Sys=GLMixer.Format.U16Sys, S8Sys=GLMixer.Format.S8Sys, S16Sys=GLMixer.Format.S16Sys,
  
  Default=S16Sys, MixerFormat=GLMixer.Format.MixerFormat
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
  public uint ByteRate    { get { return (uint)(SampleSize*Frequency); } }

  public uint         Frequency;
  public SampleFormat Format;
  public byte         Channels;
}

public unsafe delegate void MixFilter(int* buffer, int samples, AudioFormat format);
public delegate void ChannelFinishedHandler(Channel channel);

#region AudioSource
public abstract class AudioSource
{ public abstract bool CanRewind { get; }
  public abstract bool CanSeek   { get; }
  public abstract int  Position  { get; set; }
  public AudioFormat Format { get { return format; } }
  public int Length  { get { return length; } }
  public int Samples { get { return length/format.SampleSize; } }
  public int Priority { get { return priority; } set { priority=value; } }
  
  public int Volume
  { get { return volume; }
    set { if(value!=-1) Mixer.CheckVolume(value); volume=value; }
  }

  public abstract void Rewind();

  public int Play() { return Play(0, Mixer.Infinite, Mixer.FreeChannel); }
  public int Play(int loops) { return Play(loops, Mixer.Infinite, Mixer.FreeChannel); }
  public int Play(int loops, int timeoutMs) { return Play(loops, timeoutMs, Mixer.FreeChannel); }
  public int Play(int loops, int timeoutMs, int channel)
  { if(channel==Mixer.FreeChannel) return Mixer.StartPlaying(this, loops);
    else
      lock(Mixer.Channels[channel])
      { Mixer.Channels[channel].StartPlaying(this, loops);
        return channel;
      }
  }

  public int FadeIn(int fadeMs) { return FadeIn(fadeMs, 0, Mixer.FreeChannel, Mixer.Infinite); }
  public int FadeIn(int fadeMs, int loops) { return FadeIn(fadeMs, loops, Mixer.FreeChannel, Mixer.Infinite); }
  public int FadeIn(int fadeMs, int loops, int timeoutMs) { return FadeIn(fadeMs, loops, timeoutMs, Mixer.FreeChannel); }
  public int FadeIn(int fadeMs, int loops, int timeoutMs, int channel) { throw new NotImplementedException(); }

  protected internal virtual byte[] ReadAll()
  { lock(this)
    { if(Position>0) Rewind();
      if(Length>=0)
      { byte[] buf = new byte[Length];
        if(ReadBytes(buf, Length)!=Length) throw new EndOfStreamException();
        return buf;
      }
      else throw new NotImplementedException("no default implementation for ReadAll()"); // TODO: implement me (and others)
    }
  }

  protected internal abstract int ReadBytes(byte[] buf, int length);
  protected unsafe int ReadSamples(int* buffer, int samples) { return ReadSamples(buffer, samples, -1); }
  protected internal abstract unsafe int ReadSamples(int* buffer, int samples, int volume);
  
  protected AudioFormat format;
  protected int volume=-1, length=-1, priority;
  internal  int playing;
}

public abstract class StreamSource : AudioSource
{ public override bool CanRewind { get { return stream.CanSeek; } }
  public override bool CanSeek   { get { return stream.CanSeek; } }
  public override int  Position
  { get { return curPos; }
    set { lock(this) curPos=(int)(stream.Position=value+startPos); }
  }

  public override void Rewind() { Position=0; }

  protected internal override int ReadBytes(byte[] buf, int length)
  { lock(this)
    { int read = stream.Read(buf, 0, length);
      curPos += read;
      return read;
    }
  }

  protected internal override unsafe int ReadSamples(int *dest, int samples, int volume)
  { lock(this)
    { int shift=format.SampleSize>>1, toRead=Math.Min(length-curPos, samples<<shift), read, readSamps;
      if(buffer==null || toRead>buffer.Length) buffer = new byte[toRead];
      read = stream.Read(buffer, 0, toRead); // FIXME: assumes stream is reliable
      readSamps = read>>shift;
      fixed(byte* src = buffer)
        if(format.Format==SampleFormat.MixerFormat)
          if(volume>=0) GLMixer.Mix(dest, (int*)src, (uint)readSamps, (ushort)volume);
          else GLMixer.Copy(dest, (int*)src, (uint)readSamps);
        else
          GLMixer.ConvertMix(dest, src, (uint)readSamps, (ushort)format.Format,
                             (ushort)(volume<0 ? Mixer.MaxVolume : volume));
      curPos += read;
      return readSamps;
    }
  }
  
  protected void Init(Stream stream, AudioFormat format, int startPos, int length)
  { this.stream=stream;
    this.format=format;
    this.startPos=startPos;
    this.length=length;
    curPos = stream.CanSeek ? (int)stream.Position-startPos : 0;
  }

  protected byte[] buffer;
  protected Stream stream;
  protected int    curPos, startPos;
}

public class RawSource : StreamSource
{ public RawSource(string filename, AudioFormat format) : this(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read), format) { }
  public RawSource(string filename, AudioFormat format, int start, int length) : this(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read), format, start, length) { }
  public RawSource(Stream stream, AudioFormat format) : this(stream, format, 0, stream.CanSeek ? (int)stream.Length : -1) { }
  public RawSource(Stream stream, AudioFormat format, int start, int length)
  { Init(stream, format, start, length);
  }
}

public class WaveSource : StreamSource
{ public WaveSource(string filename) : this(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) { }
  public WaveSource(Stream stream)
  { string magic = IOH.ReadString(stream, 4);
    if(magic=="RIFF") LoadWav(stream);
    else if(magic=="FORM") LoadAiff(stream);
    else throw new ArgumentException("Unknown wave format. This class accepts valid, uncompressed RIFF and AIFF formats only.");
  }
  
  protected void LoadWav(Stream stream)
  { IOH.ReadLE4(stream); // skip waveLen
    if(IOH.ReadString(stream, 4)!="WAVE") throw new ArgumentException("Not a WAVE file");
    WaveFormat wfm=null;
    byte[]     data=null;
    while(stream.Position<stream.Length)
    { string chunk = IOH.ReadString(stream, 4);
      int    chunkLen = IOH.ReadLE4(stream);
      if(chunk=="fmt ")
      { wfm = WaveFormat.FromStream(stream);
        if(wfm.Encoding!=1) throw new ArgumentException("This loader does not support compressed WAVE files");
        if(wfm.BitsPerSample!=8 && wfm.BitsPerSample!=16)
          throw new ArgumentException("This loader only supports 8 and 16-bit WAVE files.");
        if(data!=null) break;
      }
      else if(chunk=="data")
      { data = IOH.Read(stream, chunkLen);
        if(wfm!=null) break;
      }
      else stream.Position += chunkLen;
    }
    if(wfm==null || data==null) throw new ArgumentException("Cannot find data within WAVE file!");
    AudioFormat format = new AudioFormat(wfm.Frequency, wfm.BitsPerSample==8 ? SampleFormat.U8 : SampleFormat.S16,
                                         (byte)wfm.Channels);
    Init(new MemoryStream(data, false), format, 0, data.Length);
  }

  protected void LoadAiff(Stream stream)
  { throw new NotImplementedException("AIFF loading not implemented");
  }

  private class WaveFormat
  { public uint   Frequency, ByteRate;
    public ushort Encoding, Channels, BlockAlign, BitsPerSample;
    
    public static WaveFormat FromStream(Stream stream)
    { WaveFormat f = new WaveFormat();
      f.Encoding      = IOH.ReadLE2U(stream);
      f.Channels      = IOH.ReadLE2U(stream);
      f.Frequency     = IOH.ReadLE4U(stream);
      f.ByteRate      = IOH.ReadLE4U(stream);
      f.BlockAlign    = IOH.ReadLE2U(stream);
      f.BitsPerSample = IOH.ReadLE2U(stream);
      return f;
    }
  }
}

public class SampleSource : AudioSource
{ public override bool CanRewind { get { return true; } }
  public override bool CanSeek   { get { return true; } }
  public override int  Position
  { get { return curPos; }
    set
    { if(value<0 || value>length) throw new ArgumentOutOfRangeException("Position");
      lock(this) curPos = value;
    }
  }

  public override void Rewind() { Position=0; }

  public SampleSource(AudioSource stream) : this(stream, true) { }
  public SampleSource(AudioSource stream, bool mixerFormat)
  { if(stream.Length>=0)
    { data   = stream.ReadAll();
      format = stream.Format;

      if(mixerFormat)
      { data = Mixer.Convert(data, format, Mixer.Format);
        format = Mixer.Format;
      }
      length = data.Length;
    }
    else throw new NotImplementedException("SampleSource can't handle streams of unknown length");
  }

  public SampleSource(AudioSource stream, AudioFormat convertTo)
  { data = Mixer.Convert(stream.ReadAll(), stream.Format, format=convertTo);
    length = data.Length;
  }

  protected internal override int ReadBytes(byte[] buf, int length)
  { lock(this)
    { int toRead = Math.Min(length, this.length-curPos);
      Array.Copy(data, curPos, buf, 0, toRead);
      curPos += toRead;
      return toRead;
    }
  }

  protected internal override unsafe int ReadSamples(int *dest, int samples, int volume)
  { lock(this)
    { int shift=format.SampleSize>>1, toRead=Math.Min(length-curPos, samples<<shift), readSamps=toRead>>shift;
      fixed(byte* src = data)
        if(format.Format==SampleFormat.MixerFormat)
          if(volume>=0) GLMixer.Mix(dest, (int*)(src+curPos), (uint)readSamps, (ushort)volume);
          else GLMixer.Copy(dest, (int*)(src+curPos), (uint)readSamps);
        else
          GLMixer.ConvertMix(dest, src+curPos, (uint)readSamps, (ushort)format.Format,
                             (ushort)(volume<0 ? Mixer.MaxVolume : volume));
      curPos += toRead;
      return readSamps;
    }
  }

  protected byte[] data;
  protected int curPos;
}

#endregion

#region Channel
public sealed class Channel
{ internal Channel(int channel) { number=channel; volume=Mixer.MaxVolume; }
  
  public event MixFilter Filters;
  public event ChannelFinishedHandler Finished;

  public int Number { get { return number; } }
  public int Volume
  { get { return volume; }
    set { Mixer.CheckVolume(value); volume = value; }
  }
  public AudioSource Source { get { return source; } }
  public int  Priority { get { return priority; } }
  public uint Age { get { return source==null ? 0 : Timing.Ticks-startTime; } }
  public AudioStatus Status
  { get { return source==null ? AudioStatus.Stopped : paused ? AudioStatus.Paused : AudioStatus.Playing; }
  }
  //public Fade Fading { get; }

  public void Pause()  { paused=true; }
  public void Resume() { paused=false; }
  public void Stop()   { lock(this) StopPlaying(); }

  //public void FadeOut(uint fadeMs);

  internal void StartPlaying(AudioSource source, int loops)
  { StopPlaying();
    lock(source)
    { if(!source.CanRewind && loops!=0) throw new ArgumentException("Can't loop sources that can't be rewound");
      if(!source.CanSeek && source.playing>0)
        throw new ArgumentException("Can't play unseekable streams more than once at a time");
      this.source   = source;
      this.loops    = loops;
      priority  = source.Priority;
      position  = 0;
      paused    = false;
      startTime = Timing.Ticks;
      source.playing++;
      convert = source.Format.Frequency!=Mixer.Format.Frequency || source.Format.Channels!=Mixer.Format.Channels;
      if(convert)
      { sdRatio = Mixer.ConversionRatio(source.Format, Mixer.Format);
        dsRatio = Mixer.ConversionRatio(Mixer.Format, source.Format);
      }
      else convBuf=null;
    }
  }

  internal void StopPlaying()
  { if(source==null) return;
    lock(source)
    { if(Finished!=null) Finished(this);
      Mixer.OnChannelFinished(this);
      source = null;
    }
  }

  internal unsafe void Mix(int* stream, int samples, MixFilter filters)
  { if(source==null || paused) return;
    lock(source)
    { int volume = source.Volume<0 ? this.volume : source.Volume, read, toRead;
      
      if(convert)
      { toRead = (int)(samples*source.Format.SampleSize*dsRatio);
        int len = Math.Max(toRead, samples*Mixer.Format.SampleSize);
        if(convBuf==null || convBuf.Length<len) convBuf = new byte[len];
        if(source.CanSeek) source.Position = position;
        samples = (int)(source.Format.SampleSize*dsRatio); // 'samples' reused as a divisor
      }
      else
      { toRead  = samples;
        if(source.CanSeek) source.Position = position*source.Format.SampleSize;
      }

      while(true)
      { if(convert)
        { read=source.ReadBytes(convBuf, toRead);
          Mixer.Convert(convBuf, source.Format, Mixer.Format, read);
          if(Filters==null && filters==null) 
            fixed(byte* src = convBuf)
              GLMixer.Check(GLMixer.ConvertMix(stream, src, (uint)(read/samples),
                                              (ushort)Mixer.Format.Format, (ushort)volume));
          else
          { int len = read/samples;
            int* buffer = stackalloc int[len];
            fixed(byte* src = convBuf)
              GLMixer.Check(GLMixer.ConvertMix(buffer, src, (uint)len,
                                              (ushort)Mixer.Format.Format, (ushort)Mixer.MaxVolume));
            if(Filters!=null) Filters(buffer, len, Mixer.Format);
            if(filters!=null) filters(buffer, len, Mixer.Format);
            GLMixer.Check(GLMixer.Mix(stream, buffer, (uint)len, (ushort)volume));
          }
        }
        else
        { if(Filters==null && filters==null) read=source.ReadSamples(stream, toRead, volume);
          else
          { int* buffer = stackalloc int[toRead];
            read = source.ReadSamples(buffer, toRead, -1);
            if(read>0)
            { if(Filters!=null) Filters(buffer, read, source.Format);
              if(filters!=null) filters(buffer, read, source.Format);
              GLMixer.Check(GLMixer.Mix(stream, buffer, (uint)read, (ushort)volume));
            }
          }
        }
        position += read;
        toRead   -= read;
        if(toRead<=0) break;
        if(read==0)
        { if(loops==0) { StopPlaying(); break; }
          else
          { source.Rewind();
            position = 0;
            if(loops>0) loops--;
          }
        }
      }
    }
  }

  AudioSource source;
  byte[] convBuf;
  double sdRatio, dsRatio;
  uint startTime;
  int  volume, number, position, loops, priority;
  bool paused, convert;
}
#endregion

#region Mixer class
public class Mixer
{ private Mixer() { }

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
    SDL.Initialize(SDL.InitFlag.Audio);
    init        = true;
    callback    = new GLMixer.MixCallback(FillBuffer);
    Mixer.chans = new Channel[0];

    try { GLMixer.Check(GLMixer.Init(frequency, (ushort)format, (byte)chans, bufferMs, callback, new IntPtr(null))); }
    catch(Exception e) { Deinitialize(); throw e; }
    
    uint freq, bytes;
    ushort form;
    byte   chan;
    SDL.Check(GLMixer.GetFormat(out freq, out form, out chan, out bytes));
    Mixer.format = new AudioFormat(freq, (SampleFormat)form, chan);
    
    SDL.PauseAudio(0);
    return freq==frequency && form==(short)format && chan==(byte)chans;
  }
  
  public static void Deinitialize()
  { if(init)
    { GLMixer.Quit();
      SDL.Deinitialize(SDL.InitFlag.Audio);
      chans=null;
      init=false;
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

  internal static void OnChannelFinished(Channel chan)
  { if(ChannelFinished!=null) ChannelFinished(chan);
  }
  
  internal static int StartPlaying(AudioSource source, int loops)
  { CheckInit();
    if(reserved==chans.Length) return -1;

    bool tried=false;
    do
      for(int i=reserved; i<chans.Length; i++)
        if(chans[i].Status==AudioStatus.Stopped) // try to lock as little as possible
        { tried=true;
          lock(chans[i])
            if(chans[i].Status==AudioStatus.Stopped)
            { chans[i].StartPlaying(source, loops);
              return i;
            }
        }
    while(!tried);

    switch(playPolicy)
    { case PlayPolicy.Oldest:
        lock(callback)
        { int  oi=reserved;
          uint age=0;
          for(int i=reserved; i<chans.Length; i++) if(chans[i].Age>age) { age=chans[i].Age; oi=i; }
          lock(chans[oi]) chans[oi].StartPlaying(source, loops);
          return oi;
        }
      case PlayPolicy.Priority:
        lock(callback)
        { int  pi=reserved, prio=int.MaxValue;
          for(int i=reserved; i<chans.Length; i++) if(chans[i].Priority<prio) { prio=chans[i].Priority; pi=i; }
          lock(chans[pi]) chans[pi].StartPlaying(source, loops);
          return pi;
        }
      case PlayPolicy.OldestPriority:
        lock(callback)
        { int  pi=reserved, oi, prio=int.MaxValue;
          uint age=0;
          for(int i=reserved; i<chans.Length; i++) if(chans[i].Priority<prio) { prio=chans[i].Priority; pi=i; }
          oi=pi;
          for(int i=reserved; i<chans.Length; i++)
            if(chans[i].Priority==prio && chans[i].Age>age) { oi=i; age=chans[i].Age; }
          lock(chans[oi]) chans[oi].StartPlaying(source, loops);
          return oi;
        }
      default: return -1;
    }
  }

  internal static void CheckVolume(int volume)
  { if(volume<0 || volume>Mixer.MaxVolume) throw new ArgumentOutOfRangeException("value");
  }

  internal static double ConversionRatio(AudioFormat srcFormat, AudioFormat destFormat)
  { if(srcFormat.Equals(destFormat)) return 1.0;
    if(srcFormat.Format==SampleFormat.MixerFormat && destFormat.Format==SampleFormat.MixerFormat)
      throw new NotImplementedException("Can't convert between two different mixer formats");
    if(srcFormat.Format==SampleFormat.MixerFormat) return (double)destFormat.SampleSize/4;
    else if(destFormat.Format==SampleFormat.MixerFormat) return (double)4/destFormat.SampleSize;
    else
    { SDL.AudioCVT cvt;
      SDL.Check(SDL.BuildAudioCVT(out cvt, (short)srcFormat.Format, srcFormat.Channels, srcFormat.Frequency,
                                  (short)destFormat.Format, destFormat.Channels, destFormat.Frequency));
      return cvt.len_ratio;
    }
  }

  internal static byte[] Convert(byte[] srcData, AudioFormat srcFormat, SampleFormat destFormat)
  { return Convert(srcData, srcFormat, destFormat, -1);
  }
  internal static byte[] Convert(byte[] srcData, AudioFormat srcFormat, SampleFormat destFormat, int inline)
  { AudioFormat df = srcFormat;
    df.Format = destFormat;
    return Convert(srcData, srcFormat, df);
  }
  internal static byte[] Convert(byte[] srcData, AudioFormat srcFormat, AudioFormat destFormat)
  { return Convert(srcData, srcFormat, destFormat, -1);
  }
  internal static byte[] Convert(byte[] srcData, AudioFormat srcFormat, AudioFormat destFormat, int inline)
  { if(srcFormat.Equals(destFormat)) return srcData;
    if(inline>=0)
    { if(srcFormat.Format==SampleFormat.MixerFormat || destFormat.Format==SampleFormat.MixerFormat)
        throw new NotSupportedException("Can't convert to or from a mixer format inline");
    }
    else
    { if(srcFormat.Format==SampleFormat.MixerFormat && destFormat.Format==SampleFormat.MixerFormat)
        throw new NotSupportedException("Can't convert between two different mixer formats");
      if(srcFormat.Format==SampleFormat.MixerFormat)
      { int samples = srcData.Length/4;
        byte[] ret  = new byte[samples*destFormat.SampleSize];
        unsafe
        { fixed(byte* src = srcData)
            fixed(byte* dest = ret)
              GLMixer.Check(GLMixer.ConvertAcc(dest, (int*)src, (uint)samples, (ushort)destFormat.Format));
        }
        if(srcFormat.Channels!=destFormat.Channels || srcFormat.Frequency!=destFormat.Frequency)
        { AudioFormat format = srcFormat;
          format.Format = destFormat.Format;
          ret = Convert(ret, format, destFormat);
        }
        return ret;
      }
      else if(destFormat.Format==SampleFormat.MixerFormat)
      { if(srcFormat.Channels!=Mixer.Format.Channels || srcFormat.Frequency!=Mixer.Format.Frequency)
        { srcData = Convert(srcData, srcFormat, Mixer.Format);
          srcFormat = Mixer.Format;
        }
        int samples = srcData.Length/srcFormat.SampleSize;
        byte[] ret = new byte[samples*4];
        unsafe
        { fixed(byte* src = srcData)
            fixed(byte* dest = ret)
              GLMixer.Check(GLMixer.ConvertMix((int*)dest, src, (uint)samples, (ushort)srcFormat.Format, MaxVolume));
        }
        return ret;
      }
    }
    unsafe
    { SDL.AudioCVT cvt;
      SDL.Check(SDL.BuildAudioCVT(out cvt, (short)srcFormat.Format, srcFormat.Channels, srcFormat.Frequency,
                                  (short)destFormat.Format, destFormat.Channels, destFormat.Frequency));
      if(inline>=0)
      { cvt.len = inline;
        fixed(byte* buf = srcData)
        { cvt.buf = buf;
          SDL.Check(SDL.ConvertAudio(ref cvt));
        }
        return srcData;
      }
      else if(cvt.len_ratio<1.0)
      { cvt.len = srcData.Length;
        byte[] src = (byte[])srcData.Clone();
        fixed(byte* buf = src)
        { cvt.buf = buf;
          SDL.Check(SDL.ConvertAudio(ref cvt));
        }
        byte[] ret = new byte[cvt.len_cvt];
        Array.Copy(src, ret, cvt.len_cvt);
        return ret;
      }
      else
      { cvt.len = srcData.Length;
        byte[] ret = new byte[(int)(cvt.len*cvt.len_ratio)];
        Array.Copy(srcData, ret, 0);
        fixed(byte* buf = ret)
        { cvt.buf = buf;
          SDL.Check(SDL.ConvertAudio(ref cvt));
        }
        return ret;
      }
    }
  }

  static void CheckInit()
  { if(!init) throw new InvalidOperationException("Mixer has not been initialized.");
  }

  static unsafe void FillBuffer(int* stream, uint samples, IntPtr context)
  { lock(callback)
    { for(int i=0; i<chans.Length; i++) lock(chans[i]) chans[i].Mix(stream, (int)samples, eFilters);
      if(ePostFilters!=null) ePostFilters(stream, (int)samples, format);
      if(MixPolicy==MixPolicy.Divide) GLMixer.DivideAccumulator(chans.Length);
    }
  }

  static AudioFormat format;
  static GLMixer.MixCallback callback;
  static Channel[] chans;
  static MixFilter eFilters, ePostFilters;
  static int reserved;
  static PlayPolicy playPolicy = PlayPolicy.Fail;
  static MixPolicy  mixPolicy  = MixPolicy.DontDivide;
  static bool init;
}
#endregion

} // namespace GameLib.Audio