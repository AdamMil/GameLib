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

public enum Speakers : byte{ Mono=1, Stereo=2 };
public enum AudioStatus
{ Stopped, Playing, Paused
}
public enum Fade
{ None, In, Out
}

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

public unsafe delegate void MixFilter(ref AudioFormat format, int* buffer, int samples);
public delegate void ChannelFinishedHandler(Channel channel);

#region AudioSource
public abstract class AudioSource
{ public abstract bool CanRewind { get; }
  public abstract bool CanSeek   { get; }
  public abstract int  Position  { get; set; }
  public abstract int  Length    { get; }
  public abstract AudioFormat Format { get; }

  public int Volume
  { get { return volume; }
    set { if(value!=-1) Mixer.CheckVolume(value); volume=value; }
  }

  public abstract void Rewind();

  public int Play() { return Play(0, Mixer.Infinite, Mixer.FreeChannel); }
  public int Play(int loops) { return Play(loops, Mixer.Infinite, Mixer.FreeChannel); }
  public int Play(int loops, int timeoutMs) { return Play(loops, timeoutMs, Mixer.FreeChannel); }
  public int Play(int loops, int timeoutMs, int channel) { throw new NotImplementedException(); }

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
  protected internal abstract unsafe int ReadSamples(int* buffer, int samples, bool mix);
  
  protected int volume=-1;
}

public abstract class StreamSource : AudioSource
{ public override bool CanRewind { get { return stream.CanSeek; } }
  public override bool CanSeek   { get { return stream.CanSeek; } }
  public override int  Position  { get { return curPos; } set { lock(this) stream.Position=value+startPos; } }
  public override int  Length    { get { return length; } }
  public override AudioFormat Format { get { return format; } }

  public override void Rewind() { stream.Position=startPos; }

  protected internal override int ReadBytes(byte[] buf, int length)
  { lock(this)
    { int read = stream.Read(buf, 0, length);
      curPos += read;
      return read;
    }
  }
  
  protected internal override unsafe int ReadSamples(int *dest, int samples, bool alwaysMix)
  { lock(this)
    { int shift=format.SampleSize>>1, toRead=Math.Min(length-curPos, samples<<shift), read, readSamps;
      if(buffer==null || toRead>buffer.Length) buffer = new byte[toRead];
      read = stream.Read(buffer, 0, toRead); // FIXME: assumes stream is reliable
      readSamps = read>>shift;
      fixed(byte* src = buffer)
        if(format.SampleSize==32)
          if(alwaysMix) GLMixer.Mix(dest, (int*)src, (uint)readSamps, Mixer.MaxVolume);
          else GLMixer.Copy(dest, (int*)src, (uint)readSamps);
        else
          GLMixer.ConvertMix(dest, src, (uint)readSamps, (ushort)format.Format, Mixer.MaxVolume);
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

  protected AudioFormat format;
  protected byte[]      buffer;
  protected Stream      stream;
  protected int curPos, startPos, length;
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
    { if(value<0 || value>Length) throw new ArgumentOutOfRangeException("Position");
      lock(this) curPos = value;
    }
  }
  public override int Length { get { return data.Length; } }
  public override AudioFormat Format { get { return format; } }

  public override void Rewind() { Position=0; }

  public SampleSource(AudioSource stream, bool mixerFormat)
  { if(stream.Length>=0)
    { data   = stream.ReadAll();
      format = stream.Format;

      if(mixerFormat)
      { data = Mixer.Convert(data, format, SampleFormat.MixerFormat);
        format.Format = SampleFormat.MixerFormat;
      }
    }
    else throw new NotImplementedException("SampleSource can't handle streams of unknown length");
  }

  public SampleSource(AudioSource stream, AudioFormat convertTo)
  { data = Mixer.Convert(stream.ReadAll(), stream.Format, format=convertTo);
  }

  protected internal override int ReadBytes(byte[] buf, int length)
  { lock(this)
    { int toRead = Math.Min(length, Length-curPos);
      Array.Copy(data, curPos, buf, 0, toRead);
      curPos += toRead;
      return toRead;
    }
  }
  
  protected internal override unsafe int ReadSamples(int *dest, int samples, bool alwaysMix)
  { lock(this)
    { int shift=format.SampleSize>>1, toRead=Math.Min(Length-curPos, samples<<shift), readSamps=toRead>>shift;
      fixed(byte* src = data)
        if(format.SampleSize==32)
          if(alwaysMix) GLMixer.Mix(dest, (int*)(src+curPos), (uint)readSamps, Mixer.MaxVolume);
          else GLMixer.Copy(dest, (int*)(src+curPos), (uint)readSamps);
        else
          GLMixer.ConvertMix(dest, src+curPos, (uint)readSamps, (ushort)format.Format, Mixer.MaxVolume);
      curPos += toRead;
      return readSamps;
    }
  }

  protected AudioFormat format;
  protected byte[] data;
  protected int curPos;
}

#endregion

#region Channel
[System.Security.SuppressUnmanagedCodeSecurity()]
public sealed class Channel
{ internal Channel(int channel) { number=channel; }
  
  public event MixFilter Filters;
  public event ChannelFinishedHandler ChannelFinished;

  public int Number { get { return number; } }
  public int Volume
  { get { return volume; }
    set { Mixer.CheckVolume(value); volume = value; }
  }

  /*public AudioStatus Status { get; }
  public Fade Fading { get; }

  public void Pause();
  public void Resume();
  public void Stop();

  public void FadeOut(uint fadeMs);*/

  internal bool HasFilters { get { return Filters!=null; } }

  /*unsafe void OnFilter(int* buffer, int samples);
  void OnChannelFinished();*/

  int volume, number;
}
#endregion

#region Mixer class
public class Mixer
{ private Mixer() { }

  public const int Infinite=-1, FreeChannel=-1, MaxVolume=256;

  public event MixFilter Filters;

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

  public static bool Initialize() { return Initialize(22050, SampleFormat.Default, Speakers.Stereo, 100); }
  public static bool Initialize(uint frequency) { return Initialize(frequency, SampleFormat.Default, Speakers.Stereo, 100); }
  public static bool Initialize(uint frequency, SampleFormat format) { return Initialize(frequency, format, Speakers.Stereo, 100); }
  public static bool Initialize(uint frequency, SampleFormat format, Speakers chans) { return Initialize(frequency, format, chans, 100); }
  public unsafe static bool Initialize(uint frequency, SampleFormat format, Speakers chans, uint bufferMs)
  { if(init) throw new InvalidOperationException("Already initialized. Deinitialize first to change format");
    SDL.Initialize(SDL.InitFlag.Audio);
    init=true;
    callback = new GLMixer.MixCallback(FillBuffer);
    try { SDL.Check(GLMixer.Init(frequency, (ushort)format, (byte)chans, bufferMs, callback, new IntPtr(null))); }
    catch(Exception e) { Deinitialize(); throw e; }
    
    Mixer.chans = new Channel[0];

    uint freq, bytes;
    ushort form;
    byte   chan;
    SDL.Check(GLMixer.GetFormat(out freq, out form, out chan, out bytes));
    Mixer.format = new AudioFormat(freq, (SampleFormat)form, chan);
    return freq==frequency && form==(short)format && chan==(byte)chans;
  }
  
  public static void Deinitialize()
  { if(init)
    { GLMixer.Quit();
      SDL.Deinitialize(SDL.InitFlag.Audio);
      init=false;
    }
  }
  
  //public static void AllocateChannels(int numChannels);

  public static void LockAudio()   { SDL.LockAudio(); }
  public static void UnlockAudio() { SDL.UnlockAudio(); }

  internal static void CheckVolume(int volume)
  { if(volume<0 || volume>Mixer.MaxVolume) throw new ArgumentOutOfRangeException("value");
  }
  
  internal static byte[] Convert(byte[] srcData, AudioFormat srcFormat, SampleFormat destFormat)
  { AudioFormat df = srcFormat;
    df.Format = destFormat;
    return Convert(srcData, srcFormat, df);
  }
  internal static byte[] Convert(byte[] srcData, AudioFormat srcFormat, AudioFormat destFormat)
  { if(srcFormat.Equals(destFormat)) return srcData;
    if(srcFormat.Format==SampleFormat.MixerFormat && destFormat.Format==SampleFormat.MixerFormat)
      throw new NotImplementedException("Can't convert between two different mixer formats");
    if(srcFormat.Format==SampleFormat.MixerFormat)
    { int samples = srcData.Length/4;
      byte[] ret  = new byte[samples*destFormat.SampleSize];
      unsafe
      { fixed(byte* src = srcData)
          fixed(byte* dest = ret)
            GLMixer.Check(GLMixer.ConvertAcc(dest, (int*)src, (uint)samples, (ushort)destFormat.Format));
      }
      return ret;
    }
    else if(destFormat.Format==SampleFormat.MixerFormat)
    { int samples = srcData.Length/srcFormat.SampleSize;
      byte[] ret = new byte[samples*4];
      unsafe
      { fixed(byte* src = srcData)
          fixed(byte* dest = ret)
            GLMixer.Check(GLMixer.ConvertMix((int*)dest, src, (uint)samples, (ushort)srcFormat.Format, MaxVolume));
      }
      return ret;
    }
    else
    { unsafe
      { SDL.AudioCVT cvt;
        SDL.Check(SDL.BuildAudioCVT(out cvt, (short)srcFormat.Format, srcFormat.Channels, srcFormat.Frequency,
                                    (short)destFormat.Format, destFormat.Channels, destFormat.Frequency));
        cvt.len = srcData.Length;
        if(cvt.len_ratio<1.0)
        { byte[] src = (byte[])srcData.Clone();
          fixed(byte* buf = src)
          { cvt.buf = buf;
            SDL.Check(SDL.ConvertAudio(ref cvt));
          }
          byte[] ret = new byte[cvt.len_cvt];
          Array.Copy(src, ret, cvt.len_cvt);
          return ret;
        }
        else
        { byte[] ret = new byte[(int)(cvt.len*cvt.len_ratio)];
          Array.Copy(srcData, ret, 0);
          fixed(byte* buf = ret)
          { cvt.buf = buf;
            SDL.Check(SDL.ConvertAudio(ref cvt));
          }
          return ret;
        }
      }
    }
  }

  static void CheckInit()
  { if(!init) throw new InvalidOperationException("Mixer has not been initialized.");
  }

  static unsafe void FillBuffer(int* stream, uint samples, IntPtr context)
  {
  }

  static AudioFormat format;
  static GLMixer.MixCallback callback;
  static Channel[] chans;
  static int  reserved;
  static bool init;
}
#endregion

} // namespace GameLib.Audio