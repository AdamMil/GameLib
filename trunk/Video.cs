using System;
using System.Drawing;
using GameLib.Interop.SDL;

namespace GameLib.Video
{

public class AlphaLevel
{ private AlphaLevel() { }
  public const byte Transparent=0, Quarter=64, Middle=128, ThreeQuarters=192, Opaque=255;
}

[Flags]
public enum PaletteType
{ Software=1, Hardware=2, Both=Software|Hardware
};

public class PixelFormat
{ public PixelFormat() { }
  public PixelFormat(byte depth) : this(depth, false) { }
  public PixelFormat(byte depth, bool withAlpha) { Depth=depth; GenerateDefaultMasks(withAlpha); }
  internal unsafe PixelFormat(SDL.PixelFormat* pf) { format = *pf; }

  public byte Depth
  { get { return format.BitsPerPixel; }
    set
    { if(value!=8 && value!=16 && value!=24 && value!=32) throw new VideoException("Unknown depth: "+value);
      format.BitsPerPixel=value; format.BytesPerPixel=(byte)(value/8);
    }
  }

  public uint RedMask   { get { return format.Rmask; } set { format.Rmask=value; } }
  public uint GreenMask { get { return format.Gmask; } set { format.Gmask=value; } }
  public uint BlueMask  { get { return format.Bmask; } set { format.Bmask=value; } }
  public uint AlphaMask { get { return format.Amask; } set { format.Amask=value; } }
  
  public void GenerateDefaultMasks() { GenerateDefaultMasks(false); }
  public void GenerateDefaultMasks(bool withAlpha)
  { switch(Depth) // TODO: make big-endian compatible
    { case 16: RedMask=0x0000F800; GreenMask=0x000007E0; BlueMask=0x0000001F; AlphaMask=0; break;
      case 24: RedMask=0x00FF0000; GreenMask=0x0000FF00; BlueMask=0x000000FF; AlphaMask=0; break;
      case 32:
        RedMask=0x00FF0000; GreenMask=0x0000FF00; BlueMask=0x000000FF;
        AlphaMask=withAlpha ? 0xFF000000 : 0;
        break;
      default: RedMask=GreenMask=BlueMask=AlphaMask=0; break;
    }
  }
  
  internal SDL.PixelFormat format;
}

public class GammaRamp
{ public ushort[] Red   { get { return red; } }
  public ushort[] Green { get { return green; } }
  public ushort[] Blue  { get { return blue; } }
  ushort[] red = new ushort[256], green = new ushort[256], blue = new ushort[256];
}

public delegate void ModeChangedHandler();

[System.Security.SuppressUnmanagedCodeSecurity()]
public sealed class Video
{ private Video() { }
  
  public class VideoInfo
  { internal unsafe VideoInfo(SDL.VideoInfo* info) { flags=info->flags; videoMem=info->videoMem; }
    
    public bool Hardware      { get { return (flags&SDL.InfoFlag.Hardware)!=0; } }
    public bool WindowManager { get { return (flags&SDL.InfoFlag.WindowManager)!=0; } }
    public bool AccelHH       { get { return (flags&SDL.InfoFlag.HH)!=0; } }
    public bool AccelHHKeyed  { get { return (flags&SDL.InfoFlag.HHKeyed)!=0; } }
    public bool AccelHHAlpha  { get { return (flags&SDL.InfoFlag.HHAlpha)!=0; } }
    public bool AccelSH       { get { return (flags&SDL.InfoFlag.SH)!=0; } }
    public bool AccelSHKeyed  { get { return (flags&SDL.InfoFlag.SHKeyed)!=0; } }
    public bool AccelSHAlpha  { get { return (flags&SDL.InfoFlag.SHAlpha)!=0; } }
    public bool AccelFills    { get { return (flags&SDL.InfoFlag.Fills)!=0; } }
    public uint VideoMemory   { get { return videoMem; } }

    SDL.InfoFlag flags;
    uint         videoMem;
  }

  public static event ModeChangedHandler ModeChanged;

  public bool Initialized { get { return initCount>0; } }
  public static VideoInfo   Info { get { CheckInit(); return info; } }
  public static Surface     DisplaySurface { get { return display; } }
  public static PixelFormat DisplayFormat { get { return display.Format; } }
  public static OpenGL      OpenGL { get { throw new NotImplementedException(); } }
  public static GammaRamp   GammaRamp
  { get
    { if(ramp==null)
      { CheckModeSet();
        ramp = new GammaRamp();
        unsafe { Check(SDL.GetGammaRamp(ramp.Red, ramp.Green, ramp.Blue)); }
      }
      return ramp;
    }
    set
    { if(value==null)
      { ramp=null;
        SetGamma(1.0f, 1.0f, 1.0f);
      }
      else
      { CheckModeSet();
        ramp = value;
        SDL.SetGammaRamp(ramp.Red, ramp.Green, ramp.Blue);
      }
    }
  }

  public static void Initialize()
  { if(initCount++==0)
    { SDL.Initialize(SDL.InitFlag.Video);
      UpdateInfo();
    }
  }
  public static void Deinitialize()
  { if(initCount==0) throw new InvalidOperationException("Deinitialize called too many times!");
    if(--initCount==0) SDL.Deinitialize(SDL.InitFlag.Video);
  }

  public static void Flip() { DisplaySurface.Flip(); }

  public unsafe static Rectangle[] GetModes(PixelFormat format, Surface.Flag flags)
  { CheckInit();
    SDL.Rect** list, p;

    fixed(SDL.PixelFormat* pf = &format.format) list = SDL.ListModes(pf, (uint)flags);
    if(list==null) return new Rectangle[0];
    else if((int)list==-1) return null;

    int length=0;
    for(p=list; *p!=null; p++) length++;
    Rectangle[] ret = new Rectangle[length];
    for(int i=0; i<length; i++) ret[i] = list[i]->ToRectangle();
    return ret;
  }

  public static byte IsModeSupported(int width, int height, byte depth, Surface.Flag flags)
  { CheckInit();
    return (byte)SDL.VideoModeOK(width, height, depth, (uint)flags);
  }
  
  public unsafe static void SetMode(int width, int height, byte depth)
  { SetMode(width, height, depth, Surface.Flag.None);
  }
  public unsafe static void SetMode(int width, int height, byte depth, Surface.Flag flags)
  { CheckInit();
    SDL.Surface* surface = SDL.SetVideoMode(width, height, depth, (uint)flags);
    if(surface==null) SDL.RaiseError();
    if(display!=null) display.Dispose();
    display = new Surface(surface, false);
    UpdateInfo();
    if(ModeChanged!=null) ModeChanged();
  }
  
  public static void GetGamma(out float red, out float green, out float blue)
  { red=redGamma; green=greenGamma; blue=blueGamma;
  }
  public static void SetGamma(float red, float green, float blue)
  { CheckModeSet();
    Check(SDL.SetGamma(red, green, blue));
    redGamma=red; greenGamma=green; blueGamma=blue;
  }
  
  public static void UpdateRect(Rectangle rect) { UpdateRect(rect.X, rect.Y, rect.Width, rect.Height); }
  public static void UpdateRect(int x, int y, int width, int height)
  { CheckModeSet();
    unsafe { SDL.UpdateRect(display.surface, x, y, width, height); }
  }
  public unsafe static void UpdateRects(Rectangle[] rects)
  { CheckModeSet();
    if(rects==null) throw new ArgumentNullException("rects");
    unsafe
    { SDL.Rect* array = stackalloc SDL.Rect[rects.Length];
      for(int i=0; i<rects.Length; i++) array[i] = new SDL.Rect(rects[i]);
      SDL.UpdateRects(display.surface, (uint)rects.Length, array);
    }
  }

  public static uint MapColor(Color color) { return DisplaySurface.MapColor(color); }
  public Color MapColor(uint color) { return DisplaySurface.MapColor(color); }

  public static void GetPalette(Color[] colors, uint startIndex, uint startColor, uint numColors)
  { DisplaySurface.GetPalette(colors, startIndex, startColor, numColors);
  }
  public static void GetPalette(Color[] colors, uint startIndex, uint startColor, uint numColors, PaletteType type)
  { DisplaySurface.GetPalette(colors, startIndex, startColor, numColors, type);
  }
  public static bool SetPalette(Color[] colors, uint startIndex, uint startColor, uint numColors)
  { return DisplaySurface.SetPalette(colors, startIndex, startColor, numColors);
  }
  public static bool SetPalette(Color[] colors, uint startIndex, uint startColor, uint numColors, PaletteType type)
  { return DisplaySurface.SetPalette(colors, startIndex, startColor, numColors, type);
  }

  static unsafe void UpdateInfo() { info = new VideoInfo(SDL.GetVideoInfo()); }
  static void Check(int result) { if(result!=0) SDL.RaiseError(); }
  static void CheckInit() { if(initCount==0) throw new InvalidOperationException("Not initialized"); }
  static void CheckModeSet() { if(display==null) throw new InvalidOperationException("Video mode not set"); }

  static GammaRamp ramp;
  static Surface   display;
  static OpenGL    openGL;
  static float     redGamma=1.0f, blueGamma=1.0f, greenGamma=1.0f;
  static uint      initCount;
  static VideoInfo info;
}

} // namespace GameLib.Video