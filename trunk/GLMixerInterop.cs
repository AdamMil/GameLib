using System;
using System.Runtime.InteropServices;

namespace GameLib.Interop.GLMixer
{

[System.Security.SuppressUnmanagedCodeSecurity()]
public class GLMixer
{
  [CallConvCdecl] internal unsafe delegate void MixCallback(int* stream, uint samples, IntPtr context);

  [Flags]
  internal enum Format : ushort
  { Eight=8, Sixteen=16, BitsPart=0xFF, Signed=0x8000, BigEndian=0x1000,
    U8=Eight, U16=Sixteen, S8=Eight|Signed, S16=Sixteen|Signed,
    U8BE=U8|BigEndian, U16BE=U16|BigEndian, S8BE=S8|BigEndian, S16BE=S16|BigEndian,
    #if BIGENDIAN
    U8Sys=U8BE, U16Sys=U16BE, S8Sys=S8BE, S16Sys=S16BE,
    #else
    U8Sys=U8, U16Sys=U16, S8Sys=S8, S16Sys=S16,
    #endif
    
    MixerFormat=32
  }

  [StructLayout(LayoutKind.Sequential, Pack=4)]
  public unsafe struct AudioCVT
  { public void CalcLenCvt() { lenCvt=(int)((long)len*lenMul/lenDiv); }

    public byte*  buf;
    public int    len, srcRate, destRate, lenCvt, lenMul, lenDiv;
    public ushort srcFormat, destFormat;
    public byte   srcChans,  destChans;
  }

  [DllImport(Config.GLMixerImportPath, EntryPoint="GLM_Init", CallingConvention=CallingConvention.Cdecl)]
  internal static extern int Init(uint freq, ushort format, byte channels, uint bufferMs, MixCallback callback, IntPtr context);
  [DllImport(Config.GLMixerImportPath, EntryPoint="GLM_GetFormat", CallingConvention=CallingConvention.Cdecl)]
  internal static extern int GetFormat(out uint freq, out ushort format, out byte channels, out uint bufferBytes);
  [DllImport(Config.GLMixerImportPath, EntryPoint="GLM_Quit", CallingConvention=CallingConvention.Cdecl)]
  internal static extern void Quit();

  [DllImport(Config.GLMixerImportPath, EntryPoint="GLM_GetMixVolume", CallingConvention=CallingConvention.Cdecl)]
  internal static extern ushort GetMixVolume();
  [DllImport(Config.GLMixerImportPath, EntryPoint="GLM_SetMixVolume", CallingConvention=CallingConvention.Cdecl)]
  internal static extern void SetMixVolume(ushort volume);

  [DllImport(Config.GLMixerImportPath, EntryPoint="GLM_ConvertAcc", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int ConvertAcc(void* dest, int* src, uint samples, ushort destFormat);
  [DllImport(Config.GLMixerImportPath, EntryPoint="GLM_SetupCVT", CallingConvention=CallingConvention.Cdecl)]
  public static extern int SetupCVT(ref AudioCVT cvt);
  [DllImport(Config.GLMixerImportPath, EntryPoint="GLM_Convert", CallingConvention=CallingConvention.Cdecl)]
  public static extern int Convert(ref AudioCVT cvt);

  [DllImport(Config.GLMixerImportPath, EntryPoint="GLM_Copy", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int Copy(int* dest, int* src, uint samples);
  [DllImport(Config.GLMixerImportPath, EntryPoint="GLM_VolumeScale", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int VolumeScale(int* stream, uint samples, ushort leftVolume, ushort rightVolume);
  [DllImport(Config.GLMixerImportPath, EntryPoint="GLM_Mix", CallingConvention=CallingConvention.Cdecl)]
  internal unsafe static extern int Mix(int* dest, int* src, uint samples, ushort leftVolume, ushort rightVolume);
  [DllImport(Config.GLMixerImportPath, EntryPoint="GLM_ConvertMix", CallingConvention=CallingConvention.Cdecl)]
  internal unsafe static extern int ConvertMix(int* dest, void* src, uint samples, ushort srcFormat, ushort leftVolume, ushort rightVolume);
  [DllImport(Config.GLMixerImportPath, EntryPoint="GLM_DivideAccumulator", CallingConvention=CallingConvention.Cdecl)]
  internal static extern int DivideAccumulator(int divisor);
  
  public static void Check(int result) { if(result<0) SDL.SDL.RaiseError(); } // TODO: do something more appropriate
}

} // namespace GameLib.Interop.GLMixer