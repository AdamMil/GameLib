using System;
using System.Runtime.InteropServices;

namespace GameLib.Interop.SDLMixer
{

[System.Security.SuppressUnmanagedCodeSecurity()]
internal class Mixer
{ [CallConvCdecl] public delegate void ChannelFinishedHandler(int channel);
	[CallConvCdecl] public delegate void MusicFinishedHandler();
	[CallConvCdecl] public unsafe delegate void MixHandler(void *mydata, byte *stream, int bytes);
  [CallConvCdecl] public unsafe delegate void FilterHandler(int channel, byte *stream, int bytes, void *mydata);
  [CallConvCdecl] public unsafe delegate void FilterCleanupHandler(int channel, void *mydata);

  public const int AllChannels=-1, FreeChannel=-1, Infinite=-1, Error=-1, InvalidChannel=-2, NoGroup=-1;
  
  #region Enums
  [Flags]
  public enum Format : ushort
  { Eight=8, Sixteen=16, Signed=0x8000, BigEndian=0x1000, BitsPart=0xFF,
    U8=Eight, U16=Sixteen, S8=Eight|Signed, S16=Sixteen|Signed,
    U8BE=U8|BigEndian, U16BE=U16|BigEndian, S8BE=S8|BigEndian, S16BE=S16|BigEndian,
    #if BIGENDIAN
    U8Sys=U8BE, U16Sys=U16BE, S8Sys=S8BE, S16Sys=S16BE,
    #else
    U8Sys=U8, U16Sys=U16, S8Sys=S8, S16Sys=S16,
    #endif
  }
  #endregion
  
  #region Structs
  [StructLayout(LayoutKind.Sequential, Pack=4)]
  public unsafe struct Chunk
  { public int allocated;
    public byte *buffer;
    public uint  length; // in bytes
    public byte  volume; // 0-128
  }
  #endregion

  #region General
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_OpenAudio", CallingConvention=CallingConvention.Cdecl)]
	public static extern int OpenAudio(uint frequency, uint format, uint numChannels, uint bufferSize);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_CloseAudio", CallingConvention=CallingConvention.Cdecl)]
	public static extern void CloseAudio();
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_QuerySpec", CallingConvention=CallingConvention.Cdecl)]
	public static extern int QuerySpec(out uint frequency, out uint format, out uint numChannels);
	public static string GetError() { return SDL.SDL.GetError(); }
	#endregion

	#region Samples
  public unsafe static Chunk* LoadWAV(string file)
  { return LoadWAV_RW(Interop.SDL.SDL.RWFromFile(file, "rb"), 1);
  }
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_LoadWAV_RW", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern Chunk* LoadWAV_RW(SDL.SDL.RWOps* ops, int freesrc);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_VolumeChunk", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int VolumeChunk(Chunk* chunk, int volume);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_FreeChunk", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern void FreeChunk(Chunk* chunk);
	#endregion

	#region Channels
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_AllocateChannels", CallingConvention=CallingConvention.Cdecl)]
	public static extern int AllocateChannels(int num);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_Volume", CallingConvention=CallingConvention.Cdecl)]
	public static extern int Volume(int channel, int volume);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_PlayChannelTimed", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int PlayChannelTimed(int channel, Chunk* chunk, int loops, int ticks);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_FadeInChannelTimed", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int FadeInChannelTimed(int channel, Chunk* chunk, int loops, int ms, int ticks);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_Pause", CallingConvention=CallingConvention.Cdecl)]
	public static extern void Pause(int channel);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_Resume", CallingConvention=CallingConvention.Cdecl)]
	public static extern void Resume(int channel);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_HaltChannel", CallingConvention=CallingConvention.Cdecl)]
	public static extern int HaltChannel(int channel);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_ExpireChannel", CallingConvention=CallingConvention.Cdecl)]
	public static extern int ExpireChannel(int channel, int ticks);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_FadeOutChannel", CallingConvention=CallingConvention.Cdecl)]
	public static extern int FadeOutChannel(int which, int ms);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_Playing", CallingConvention=CallingConvention.Cdecl)]
	public static extern int Playing(int channel);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_Paused", CallingConvention=CallingConvention.Cdecl)]
	public static extern int Paused(int channel);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_FadingChannel", CallingConvention=CallingConvention.Cdecl)]
	public static extern int FadingChannel(int which);
  #endregion
  
  #region Groups
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_ReserveChannels", CallingConvention=CallingConvention.Cdecl)]
	public static extern int ReserveChannels(int num);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_GroupChannel", CallingConvention=CallingConvention.Cdecl)]
	public static extern int GroupChannel(int channel, int group);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_GroupChannels", CallingConvention=CallingConvention.Cdecl)]
	public static extern int GroupChannels(int from, int to, int group);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_GroupCount", CallingConvention=CallingConvention.Cdecl)]
	public static extern int GroupCount(int group);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_GroupAvailable", CallingConvention=CallingConvention.Cdecl)]
	public static extern int GroupAvailable(int group);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_GroupOldest", CallingConvention=CallingConvention.Cdecl)]
	public static extern int GroupOldest(int group);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_GroupNewer", CallingConvention=CallingConvention.Cdecl)]
	public static extern int GroupNewer(int group);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_FadeOutGroup", CallingConvention=CallingConvention.Cdecl)]
	public static extern int FadeOutGroup(int group, int fadeMs);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_HaltGroup", CallingConvention=CallingConvention.Cdecl)]
	public static extern int HaltGroup(int group);
  #endregion

	#region Effects
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_RegisterEffect", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int RegisterEffect(int channel, FilterHandler filter, FilterCleanupHandler cleanup, void *mydata);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_UnregisterEffect", CallingConvention=CallingConvention.Cdecl)]
	public static extern int UnregisterEffect(int channel, FilterHandler filter);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_UnregisterAllEffects", CallingConvention=CallingConvention.Cdecl)]
	public static extern int UnregisterAllEffects(int channel);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_SetPostMix", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int SetPostMix(MixHandler mixer, void *mydata);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_SetPanning", CallingConvention=CallingConvention.Cdecl)]
	public static extern int SetPanning(int channel, byte left, byte right);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_SetDistance", CallingConvention=CallingConvention.Cdecl)]
	public static extern int SetDistance(int channel, byte distance);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_SetPosition", CallingConvention=CallingConvention.Cdecl)]
	public static extern int SetPosition(int channel, short angle, byte distance);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_SetReverseStereo", CallingConvention=CallingConvention.Cdecl)]
	public static extern int SetReverseStereo(int channel, int flip);
	#endregion
	
	#region Music
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_LoadMUS", CallingConvention=CallingConvention.Cdecl)]
	public static extern IntPtr LoadMUS(string file);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_FreeMusic", CallingConvention=CallingConvention.Cdecl)]
	public static extern void FreeMusic(IntPtr music);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_PlayMusic", CallingConvention=CallingConvention.Cdecl)]
	public static extern int PlayMusic(IntPtr music, int loops);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_FadeInMusic", CallingConvention=CallingConvention.Cdecl)]
	public static extern int FadeInMusic(IntPtr music, int loops, int ms);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_FadeInMusicPos", CallingConvention=CallingConvention.Cdecl)]
	public static extern int FadeInMusicPos(IntPtr music, int loops, int ms, double pos);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_VolumeMusic", CallingConvention=CallingConvention.Cdecl)]
	public static extern int VolumeMusic(int volume);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_PauseMusic", CallingConvention=CallingConvention.Cdecl)]
	public static extern void PauseMusic();
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_ResumeMusic", CallingConvention=CallingConvention.Cdecl)]
	public static extern void ResumeMusic();
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_RewindMusic", CallingConvention=CallingConvention.Cdecl)]
	public static extern void RewindMusic();
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_SetMusicPosition", CallingConvention=CallingConvention.Cdecl)]
	public static extern int SetMusicPosition(double position);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_GetMusicType", CallingConvention=CallingConvention.Cdecl)]
	public static extern int GetMusicType();
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_HaltMusic", CallingConvention=CallingConvention.Cdecl)]
	public static extern int HaltMusic();
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_FadeOutMusic", CallingConvention=CallingConvention.Cdecl)]
	public static extern int FadeOutMusic(int ms);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_PlayingMusic", CallingConvention=CallingConvention.Cdecl)]
	public static extern int PlayingMusic();
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_PausedMusic", CallingConvention=CallingConvention.Cdecl)]
	public static extern int PausedMusic();
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_FadingMusic", CallingConvention=CallingConvention.Cdecl)]
	public static extern int FadingMusic();
	#endregion
	
	#region Hooks
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_HookMusic", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern void HookMusic(MixHandler func, void *mydata);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_ChannelFinished", CallingConvention=CallingConvention.Cdecl)]
	public static extern void ChannelFinished(ChannelFinishedHandler func);
	[DllImport(Config.SDLMixerImportPath, EntryPoint="Mix_HookMusicFinished", CallingConvention=CallingConvention.Cdecl)]
	public static extern void HookMusicFinished(MusicFinishedHandler func);
  #endregion
  
  #region Non-Mixer helper functions
  public static void RaiseError()
  { string error = GetError();
    if(error==null) throw new AudioException("GameLib sez: Something bad happened, but Mixer disagrees");
    throw new AudioException(error);
  }
  #endregion
}
} // namespace GameLib.Interop.SDLMixer