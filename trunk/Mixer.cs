using System;
using GameLib.Interop.SDL;
using GameLib.Interop.GLMixer;

namespace GameLib.Audio
{

[Flags]
public enum SampleFormat : ushort
{ Eight=GLMixer.Format.U8, Sixteen=GLMixer.Format.U16,
  BitsPart=GLMixer.Format.BitsPart, Signed=GLMixer.Format.Signed, BigEndian=GLMixer.Format.BigEndian,
  Default=GLMixer.Format.S16Sys
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
  public uint SampleSize { get { return (Format&SampleFormat.BitsPart)/8; } }
  public uint ByteRate   { get { return SampleSize*Frequency; } }

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
  public abstract long Position  { get; set; }
  public abstract long Length    { get; }
  public abstract AudioFormat Format { get; }

  public int Volume
  { get { return volume; }
    set { if(value!=-1) Mixer.CheckVolume(value); volume=value; }
  }

  public abstract void Rewind();
 
  public int Play() { return Play(0, Mixer.Infinite, Mixer.FreeChannel); }
  public int Play(int loops) { return Play(loops, Mixer.Infinite, Mixer.FreeChannel); }
  public int Play(int loops, int timeoutMs) { return Play(loops, timeoutMs, Mixer.FreeChannel); }
  public int Play(int loops, int timeoutMs, int channel);

  public int FadeIn(int fadeMs) { return FadeIn(fadeMs, 0, Mixer.FreeChannel, Mixer.Infinite); }
  public int FadeIn(int fadeMs, int loops) { return FadeIn(fadeMs, loops, Mixer.FreeChannel, Mixer.Infinite); }
  public int FadeIn(int fadeMs, int loops, int timeoutMs) { return FadeIn(fadeMs, loops, timeoutMs, Mixer.FreeChannel); }
  public int FadeIn(int fadeMs, int loops, int timeoutMs, int channel);
  
  protected internal abstract unsafe int ReadSamples(int* buffer, int samples);
  
  protected int volume=-1;
}

public abstract class StreamSource : AudioSource
{ public override bool CanRewind { get { return stream.CanSeek; } }
  public override bool CanSeek   { get { return stream.CanSeek; } }
  public override long Position  { get { return curPos; } set { stream.Position=value+startPos; } }
  public override long Length    { get { return length; } }
  
  public override void Rewind() { stream.Position=startPos; }

  protected void Init(System.IO.Stream stream, AudioFormat format, long startPos, long length)
  { this.stream=stream;
    this.format=format;
    this.startPos=startPos;
    this.length=length;
    curPos = stream.CanSeek ? stream.Position-startPos : 0;
  }

  protected internal override unsafe int ReadSamples(int *buffer, int samples)
  { int shift=format.SampleSize-1, toRead=Math.Min((int)(length-curPos), samples<<shift), read;
    if(buf==null || toRead>buf.Length) buf = new byte[toRead];
    read = stream.Read(buf, 0, toRead)>>shift;
    fixed(byte* src = buf) GLMixer.ConvertMix(buffer, src, read, (ushort)format.Format, Mixer.MaxVolume);
    return read;
  }

  protected AudioFormat format;
  protected byte[]      buf;
  protected System.IO.Stream stream;
  protected long curPos, startPos, length;
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

  public AudioStatus Status { get; }
  public Fade Fading { get; }

  public void Pause();
  public void Resume();
  public void Stop();

  public void FadeOut(uint fadeMs);

  internal bool HasFilters { get { return Filters!=null; } }

  unsafe void OnFilter(int* buffer, int samples);
  void OnChannelFinished();

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
    
    chans = new Channel[0];

    uint freq, bytes;
    ushort form;
    byte   chan;
    SDL.Check(GLMixer.GetFormat(out freq, out form, out chan, out bytes));
    format = new AudioFormat(freq, (SampleFormat)form, chan);
    return freq==frequency && form==(short)format && chan==(byte)chans;
  }
  
  public static void Deinitialize()
  { if(init)
    { GLMixer.Quit();
      SDL.Deinitialize(SDL.InitFlag.Audio);
      init=false;
    }
  }
  
  public static void AllocateChannels(int numChannels);

  public static void LockAudio()   { SDL.LockAudio(); }
  public static void UnlockAudio() { SDL.UnlockAudio(); }

  internal static void CheckVolume(int volume)
  { if(volume<0 || volume>Mixer.MaxVolume) throw new ArgumentOutOfRangeException("value");
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