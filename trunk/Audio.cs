using System;
using System.IO;
using GameLib.Interop.SDL;
using GameLib.Interop.SDLMixer;
using System.Runtime.InteropServices;

namespace GameLib.Audio
{

public enum MusicType { None, Command, Wave, Module, Midi, OGG, MP3, Other }
public enum Fade { None, Out, In };
public enum Speakers { Mono=1, Stereo=2 };
public delegate void ChannelHandler(int channel);
public delegate void MusicFinishedHandler();
public unsafe delegate void MixHandler(void *mydata, byte *stream, int bytes);
public unsafe delegate void FilterHandler(Channel channel, void *mydata, byte *stream, int bytes);
public unsafe delegate void FilterCleanupHandler(Channel channel, void *mydata);

#region Sample
[System.Security.SuppressUnmanagedCodeSecurity()]
public class Sample : IDisposable
{ public unsafe Sample(string filename)
  { chunk = Mixer.LoadWAV(filename);
    if(chunk==null) Mixer.RaiseError();
  }
  public unsafe Sample(Stream stream)
  { StreamSource ss = new StreamSource(stream);
    fixed(SDL.RWOps* ops = &ss.ops) chunk=Mixer.LoadWAV_RW(ops, 0);
    if(chunk==null) Mixer.RaiseError();
  }

  ~Sample() { Dispose(true); GC.SuppressFinalize(this); }
  public void Dispose() { Dispose(false); }
  
  public unsafe byte Volume
  { get { return chunk->volume; }
    set
    { if(value>Audio.MaxVolume) throw new ArgumentOutOfRangeException("value");
      Mixer.VolumeChunk(chunk, value);
    }
  }

  public int Play() { return Play(0, Audio.Infinite, Audio.FreeChannel); }
  public int Play(int loops) { return Play(loops, Audio.Infinite, Audio.FreeChannel); }
  public int Play(int loops, int timeoutMs) { return Play(loops, timeoutMs, Audio.FreeChannel); }
  public unsafe int Play(int loops, int timeoutMs, int channel)
  { int ret = Mixer.PlayChannelTimed(channel, chunk, loops, timeoutMs);
    if(ret==Mixer.Error) Mixer.RaiseError();
    Audio.Link(ret, this, timeoutMs);
    return ret;
  }

  public int FadeIn(int fadeMs) { return FadeIn(fadeMs, 0, Audio.FreeChannel, Audio.Infinite); }
  public int FadeIn(int fadeMs, int loops) { return FadeIn(fadeMs, loops, Audio.FreeChannel, Audio.Infinite); }
  public int FadeIn(int fadeMs, int loops, int timeoutMs) { return FadeIn(fadeMs, loops, timeoutMs, Audio.FreeChannel); }
  public unsafe int FadeIn(int fadeMs, int loops, int timeoutMs, int channel)
  { int ret = Mixer.FadeInChannelTimed(channel, chunk, loops, fadeMs, timeoutMs);
    if(ret==Mixer.Error) Mixer.RaiseError();
    Audio.Link(ret, this, timeoutMs);
    return ret;
  }

  protected unsafe void Dispose(bool destructor)
  { if(chunk!=null)
    { Mixer.FreeChunk(chunk);
      chunk=null;
    }
  }

  internal unsafe Mixer.Chunk *chunk;
}
#endregion

#region Song
[System.Security.SuppressUnmanagedCodeSecurity()]
public class Song : IDisposable
{ public Song(string filename)
  { music = Mixer.LoadMUS(filename);
    unsafe { if(music.ToPointer()==null) Mixer.RaiseError(); }
  }
  public Song(Stream stream)
  { string name = Path.GetTempFileName();
    FileStream fs = new FileStream(name, FileMode.Open, FileAccess.Write);
    byte[] buf = new byte[1024];
    int total = (int)(stream.Length-stream.Position), read;
    while(total>0)
    { read = stream.Read(buf, 0, 1024);
      if(read==0) throw new IOException("Unable to read from the stream");
      fs.Write(buf, 0, read);
      total -= read;
    }
    fs.Close();

    unsafe
    { StreamSource ss = new StreamSource(stream);
      fixed(SDL.RWOps* ops = &ss.ops) music = Mixer.LoadMUS(name);
      if(music.ToPointer()==null)
      { new FileInfo(name).Delete();
        Mixer.RaiseError();
      }
      delete = name;
    }
  }
  ~Song() { Dispose(true); }
  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }
  
  public void Play() { Play(Audio.Infinite); }
  public void Play(int loops) { Audio.Check(Mixer.PlayMusic(music, loops)); }
  
  public void FadeIn(int fadeMs) { FadeIn(fadeMs, Audio.Infinite); }
  public void FadeIn(int fadeMs, int loops) { Audio.Check(Mixer.FadeInMusic(music, loops, fadeMs)); }
  public void FadeIn(int fadeMs, double position) { FadeIn(fadeMs, position, Audio.Infinite); }
  public void FadeIn(int fadeMs, double position, int loops)
  { Audio.Check(Mixer.FadeInMusicPos(music, loops, fadeMs, position));
  }
  
  protected unsafe void Dispose(bool destructor)
  { if(music.ToPointer()!=null)
    { Mixer.FreeMusic(music);
      music=new IntPtr(null);
    }
    if(delete!=null)
    { new FileInfo(delete).Delete();
      delete=null;
    }
  }
  
  internal  IntPtr music;
  protected string delete;
}
#endregion

#region Channel
[System.Security.SuppressUnmanagedCodeSecurity()]
public class Channel
{ internal Channel(int channel)
  { this.channel=channel;
    Mixer.Volume(channel, 128);
  }
  
  public int  Number { get { return channel; } }
  public byte Volume
  { get { return volume; }
    set
    { if(value>Audio.MaxVolume) throw new ArgumentOutOfRangeException("value");
      Mixer.Volume(channel, value);
      volume = value;
    }
  }
  public int Timeout
  { get { return timeout; }
    set
    { Mixer.ExpireChannel(channel, value);
      timeout = value;
    }
  }
  public Sample CurrentSample { get { return sample; } }
  public bool Playing { get { return Mixer.Playing(channel)==1; } }
  public bool Paused  { get { return Mixer.Paused(channel)==1; } } // TODO: possibly keep a flag? possibly resume on halt? does halt unpause? etc...
  public Fade Fading  { get { return (Fade)Mixer.FadingChannel(channel); } }

  public void Pause()  { Mixer.Pause(channel); }
  public void Resume() { Mixer.Resume(channel); }
  public void Stop()   { Mixer.HaltChannel(channel); }

  public void FadeOut(uint fadeMs) { Mixer.FadeOutChannel(channel, (int)fadeMs); }
  
  public unsafe void AddFilter(FilterHandler filter, FilterCleanupHandler cleanup, void *mydata)
  { if(filter==null) throw new ArgumentNullException("filter");
    int i=0;
    if(filters==null)
    { filters = new Effect[4];
      this.cleanup = new Mixer.FilterCleanupHandler(OnCleanup);
    }
    else for(i=0; i<filters.Length; i++) if(filters[i].Filter==null) break;
    if(i==filters.Length)
    { Effect[] array = new Effect[filters.Length*2];
      Array.Copy(filters, array, filters.Length);
      filters = array;
    }
    filters[i].Filter  = filter;
    filters[i].Cleanup = cleanup;
    filters[i].Context = mydata;
    filters[i].MixerFilter = new Mixer.FilterHandler(OnFilter);
    if(Mixer.RegisterEffect(channel, filters[i].MixerFilter, this.cleanup, (void*)i)==0)
    { filters[i].Filter=null;
      Mixer.RaiseError();
    }
  }
  public unsafe void RemoveFilter(FilterHandler filter)
  { if(filter==null) throw new ArgumentNullException("filter");
    int i;
    for(i=0; i<filters.Length; i++) if(filters[i].Filter==filter) break;
    if(i==filters.Length) throw new IndexOutOfRangeException("No such filter");
    Mixer.UnregisterEffect(channel, filters[i].MixerFilter);
    filters[i] = new Effect();
  }
  public void RemoveAllFilters() { Mixer.UnregisterAllEffects(channel); filters=null; cleanup=null; }

  public void PanEffect(byte left, byte right)
  { if(Mixer.SetPanning(channel, left, right)==0) Mixer.RaiseError();
  }
  public void DistanceEffect(byte distance)
  { if(Mixer.SetDistance(channel, distance)==0) Mixer.RaiseError();
  }
  public void PositionEffect(int angle, byte distance)
  { if(Mixer.SetPosition(channel, (short)(angle%360), distance)==0) Mixer.RaiseError();
  }
  public void ReverseStereoEffect(bool reverse)
  { if(Mixer.SetReverseStereo(channel, reverse ? 1 : 0)==0) Mixer.RaiseError();
  }

  internal Sample sample;
  internal int timeout=Audio.Infinite;
  
  protected unsafe void OnFilter(int channel, byte *stream, int bytes, void *mydata)
  { filters[(int)mydata].Call(this, stream, bytes);
  }
  protected unsafe void OnCleanup(int channel, void *mydata) { filters[(int)mydata].CallCleanup(this); }

  unsafe struct Effect
  { public void Call(Channel channel, byte *stream, int bytes)
    { Filter(channel, Context, stream, bytes);
    }
    public void CallCleanup(Channel channel) { Cleanup(channel, Context); }
    public FilterHandler        Filter;
    public FilterCleanupHandler Cleanup;
    public Mixer.FilterHandler  MixerFilter;
    public void* Context;
  }
  Effect[] filters;
  Mixer.FilterCleanupHandler cleanup;
  int channel;
  byte volume;
}
#endregion

#region Audio
[System.Security.SuppressUnmanagedCodeSecurity()]
public sealed class Audio
{ private Audio() { }

  [Flags]
  public enum Format
  { Eight=Mixer.Format.U8, Sixteen=Mixer.Format.U16,
    BitsPart=Mixer.Format.BitsPart, Signed=Mixer.Format.Signed, BigEndian=Mixer.Format.BigEndian,
    Default=Mixer.Format.S16Sys
  }
  public const byte MinVolume=0, MaxVolume=128;
  public const int  FreeChannel=Mixer.FreeChannel, NoChannel=Mixer.InvalidChannel, Infinite=Mixer.Infinite;
  public const int  NoGroup=Mixer.NoGroup;
  
  public class ChannelEventArgs : EventArgs
  { public ChannelEventArgs(int channel) { Channel=channel; }
    public int Channel;
  }

  public bool Initialized { get { return init; } }
  public static Channel[] Channels  { get { return channels; } }
  public static int ChannelsPlaying { get { CheckInit(); return Mixer.Playing(Mixer.AllChannels); } }
  public static int ChannelsPaused  { get { CheckInit(); return Mixer.Paused(Mixer.AllChannels); } }
  
  public static byte MusicVolume
  { get { return musicVolume; }
    set { Mixer.VolumeMusic(musicVolume=value); }
  }
  public static bool MusicPlaying { get { return Mixer.PlayingMusic()!=0; } } 
  public static bool MusicPaused  { get { return Mixer.PausedMusic()!=0; } } // TODO: possibly keep a flag? possibly resume on halt? does halt unpause? etc...
  public static Fade MusicFading  { get { return (Fade)Mixer.FadingMusic(); } }
  public static MusicType MusicType
  { get
    { MusicType type = (MusicType)Mixer.GetMusicType();
      return type<MusicType.None || type>MusicType.MP3 ? MusicType.Other : type;
    }
  }

  public static bool Initialize() { return Initialize(22050, Format.Default, Speakers.Stereo); }
  public static bool Initialize(uint frequency) { return Initialize(frequency, Format.Default, Speakers.Stereo); }
  public static bool Initialize(uint frequency, Format format) { return Initialize(frequency, format, Speakers.Stereo); }
  public static bool Initialize(uint frequency, Format format, Speakers chans)
  { uint mul = (uint)(format&Format.BitsPart)/8;
    return Initialize(frequency, format, chans, frequency*mul*(uint)chans/10);
  }
  public static bool Initialize(uint frequency, Format format, Speakers chans, uint bufferSize)
  { if(init) throw new InvalidOperationException("Already initialized. Deinitialize first to change format");
    SDL.Initialize();
    SDL.Init(SDL.InitFlag.Audio);
    init=true;
    try { Check(Mixer.OpenAudio(frequency, (uint)format, (uint)chans, bufferSize)); }
    catch(Exception e) { Deinitialize(); throw e; }

    uint freq, form, chan;
    if(Mixer.QuerySpec(out freq, out form, out chan)==0) Mixer.RaiseError();
    Mixer.VolumeMusic(MaxVolume);
    unsafe
    { musicMix = new Mixer.MixHandler(OnMusicMix);
      postMix  = new Mixer.MixHandler(OnPostMix);
    }
    channelFin = new Mixer.ChannelFinishedHandler(OnChannelFinished);
    musicFin   = new Mixer.MusicFinishedHandler(OnMusicFinished);
    Mixer.ChannelFinished(channelFin);
    Mixer.HookMusicFinished(musicFin);
    return freq==frequency && form==(uint)format && chan==(uint)chans;
  }
  
  public static void Deinitialize()
  { if(init)
    { Stop(); StopMusic();
      foreach(Channel c in Channels) c.RemoveAllFilters();
      Mixer.CloseAudio();
      channels = new Channel[0];
      SDL.QuitSubSystem(SDL.InitFlag.Audio);
      SDL.Deinitialize();
      init=false;
    }
  }
  
  public static void QuerySettings(out uint frequency, out Format format, out Speakers chans)
  { CheckInit();
    uint form, chan;
    if(Mixer.QuerySpec(out frequency, out form, out chan)==0) Mixer.RaiseError();
    format = (Format)form; chans = (Speakers)chan;
  }

  public static void AllocateChannels(int numChannels)
  { if(numChannels<0) throw new ArgumentOutOfRangeException("numChannels");
    if(numChannels!=channels.Length)
    { Mixer.AllocateChannels(numChannels);
      Channel[] array = new Channel[numChannels];
      Array.Copy(channels, array, Math.Min(channels.Length, numChannels));
      for(int i=channels.Length; i<numChannels; i++) array[i] = new Channel(i);
      channels = array;
    }
  }
  
  public static void ReserveChannels(int numChannels)
  { CheckInit();
    if(numChannels<0 || numChannels>channels.Length) throw new ArgumentOutOfRangeException("numChannels");
    Mixer.ReserveChannels(numChannels);
  }

  public static void GroupChannel(int channel, int group)
  { CheckChannel(channel); CheckGroup(group);
    Check(Mixer.GroupChannel(channel, group));
  }
  
  public static void GroupRange(int first, int last, int group)
  { CheckChannel(last); CheckChannel(last); CheckGroup(group);
    if(first>last) throw new ArgumentException("first must be <= last");
    if(Mixer.GroupChannels(first, last, group)<last-first+1) Mixer.RaiseError();
  }

  public static int SizeOfGroup(int group)
  { CheckGroup(group);
    return Mixer.GroupCount(group);
  }

  public static int AvailableChannel() { return Mixer.GroupAvailable(NoGroup); }
  public static int AvailableChannel(int group) { CheckGroup(group); return Mixer.GroupAvailable(group); }
  
  public static int GroupOldest() { return Mixer.GroupOldest(NoGroup); }
  public static int GroupOldest(int group) { CheckGroup(group); return Mixer.GroupOldest(group); }

  public static int GroupNewest() { return Mixer.GroupNewer(NoGroup); }
  public static int GroupNewest(int group) { CheckGroup(group); return Mixer.GroupNewer(group); }
  
  public static void FadeOutGroup(int group, int fadeMs)
  { CheckGroup(group);
    if(group==-1) FadeOut(fadeMs);
    else Mixer.FadeOutGroup(group, fadeMs);
  }

  public static void HaltGroup(int group)
  { CheckGroup(group);
    if(group==-1) Stop();
    else Mixer.HaltGroup(group);
  }

  public static void PauseMusic()  { Mixer.PauseMusic();  }
  public static void ResumeMusic() { Mixer.ResumeMusic(); }
  public static void StopMusic()   { Mixer.HaltMusic(); }
  public static void FadeOutMusic(int fadeMs) { Mixer.FadeOutMusic(fadeMs); }

  public static void SetMusicPos(double position)
  { if(position==0.0) { Mixer.RewindMusic(); return; }
    if(MusicType==MusicType.MP3) Mixer.RewindMusic();
    Check(Mixer.SetMusicPosition(position));
  }
  public unsafe static void SetCustomMusic(MixHandler handler) { SetCustomMusic(handler, null); }
  public unsafe static void SetCustomMusic(MixHandler handler, void *mydata)
  { if(handler==null)
    { Mixer.HookMusic(null, null);
      musicHandler=null;
    }
    else
    { musicHandler=handler;
      Mixer.HookMusic(musicMix, mydata);
    }
  }
  public unsafe static void SetPostProcess(MixHandler handler) { SetPostProcess(handler, null); }
  public unsafe static void SetPostProcess(MixHandler handler, void *mydata)
  { if(handler==null)
    { Mixer.SetPostMix(null, null);
      postHandler=null;
    }
    else
    { postHandler=handler;
      Mixer.SetPostMix(postMix, mydata);
    }
  }

  public static void SetVolume(byte volume) { CheckInit(); Mixer.Volume(Mixer.AllChannels, volume); }

  public static void Pause()  { CheckInit(); Mixer.Pause(Mixer.AllChannels); }
  public static void Resume() { CheckInit(); Mixer.Resume(Mixer.AllChannels); }
  public static void Stop()   { CheckInit(); Mixer.HaltChannel(Mixer.AllChannels); }

  public static void SetTimeout(int timeoutMs) { CheckInit(); Mixer.ExpireChannel(Mixer.AllChannels, timeoutMs); }

  public static void FadeOut(int fadeMs) { CheckInit(); Mixer.FadeOutChannel(Mixer.AllChannels, fadeMs); }

  public static event ChannelHandler ChannelFinished;
  public static event MusicFinishedHandler MusicFinished;
  
  internal static void Check(int res) { if(res!=0) Mixer.RaiseError(); }
  internal static void Link(int channel, Sample sample, int timeoutMs)
  { channels[channel].sample  = sample;
    channels[channel].timeout = timeoutMs;
  }
  static void Unlink(int channel)
  { channels[channel].sample = null;
  }

  static void CheckInit() { if(!init) throw new InvalidOperationException("Not initialized"); }
  static void CheckChannel(int channel)
  { if(channel<0 || channel>=channels.Length) throw new ArgumentOutOfRangeException("channel");
  }
  static void CheckGroup(int group)
  { if(group<0 && group!=NoGroup) throw new ArgumentOutOfRangeException("group");
  }

  static void OnChannelFinished(int channel)
  { if(ChannelFinished!=null) ChannelFinished(channel);
    Unlink(channel);
  }
  
  static void OnMusicFinished() { if(MusicFinished!=null) MusicFinished(); }
  unsafe static void OnMusicMix(void *mydata, byte *stream, int bytes) { musicHandler(mydata, stream, bytes); }
  unsafe static void OnPostMix(void *mydata, byte *stream, int bytes) { postHandler(mydata, stream, bytes); }

  static Channel[] channels = new Channel[0];
  static Mixer.ChannelFinishedHandler channelFin;
  static Mixer.MusicFinishedHandler   musicFin;
  static Mixer.MixHandler musicMix, postMix;
  static MixHandler musicHandler, postHandler;
  static byte musicVolume;
  static bool init;
}
#endregion

} // namespace GameLib.Audio