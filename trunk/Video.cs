using System;
using System.Drawing;
using GameLib.Interop.SDL;

namespace GameLib.Video
{

#region Supporting Types
public class AlphaLevel
{ private AlphaLevel() { }
  public const byte Transparent=0, Quarter=64, Middle=128, ThreeQuarters=192, Opaque=255;
}

[Flags]
public enum PaletteType
{ Software=1, Hardware=2, Both=Software|Hardware
};

public class GLOptions
{ public sbyte Red=-1, Green=-1, Blue=-1, Alpha=-1, AccumRed=-1, AccumGreen=-1, AccumBlue=-1, AccumAlpha=-1;
  public sbyte DoubleBuffer=-1, Frame=-1, Depth=-1, Stencil=-1;
}

public delegate void ModeChangedHandler();
#endregion

#region PixelFormat
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
  { switch(Depth) // TODO: make big-endian compatible (?)
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
#endregion

#region GammaRamp
public class GammaRamp
{ public ushort[] Red   { get { return red; } }
  public ushort[] Green { get { return green; } }
  public ushort[] Blue  { get { return blue; } }
  ushort[] red = new ushort[256], green = new ushort[256], blue = new ushort[256];
}
#endregion

#region Video class
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

  public static bool Initialized { get { return initCount>0; } }
  public static int        Width  { get { return display.Width; } }
  public static int        Height { get { return display.Height; } }
  public static VideoInfo   Info { get { AssertInit(); return info; } }
  public static Surface     DisplaySurface { get { return display; } }
  public static PixelFormat DisplayFormat { get { return display.Format; } }
  public static OpenGL      OpenGL { get { throw new NotImplementedException(); } }
  public static GammaRamp   GammaRamp
  { get
    { if(ramp==null)
      { AssertModeSet();
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
      { AssertModeSet();
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

  public static void Flip() { if(usingGL) SDL.SwapBuffers(); else DisplaySurface.Flip(); }

  public unsafe static Rectangle[] GetModes(PixelFormat format, SurfaceFlag flags)
  { AssertInit();
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

  public static byte IsModeSupported(int width, int height, byte depth, SurfaceFlag flags)
  { AssertInit();
    return (byte)SDL.VideoModeOK(width, height, depth, (uint)flags);
  }
  
  public static void SetMode(int width, int height, byte depth)
  { SetMode(width, height, depth, SurfaceFlag.None);
  }
  public static void SetMode(int width, int height, byte depth, SurfaceFlag flags)
  { AssertInit();
    flags &= ~SurfaceFlag.OpenGL;
    SetMode(width, height, depth, (uint)flags);
  }

  public static void SetGLMode(int width, int height, byte depth)
  { SetGLMode(width, height, depth, SurfaceFlag.None, null);
  }
  public static void SetGLMode(int width, int height, byte depth, GLOptions opts)
  { SetGLMode(width, height, depth, SurfaceFlag.None, opts);
  }
  public static void SetGLMode(int width, int height, byte depth, SurfaceFlag flags)
  { SetGLMode(width, height, depth, flags, null);
  }
  public unsafe static void SetGLMode(int width, int height, byte depth, SurfaceFlag flags, GLOptions opts)
  { if(opts!=null)
    { if(opts.Red  !=-1) SDL.SetAttribute(SDL.Attribute.RedSize,   opts.Red);
      if(opts.Green!=-1) SDL.SetAttribute(SDL.Attribute.GreenSize, opts.Green);
      if(opts.Blue !=-1) SDL.SetAttribute(SDL.Attribute.BlueSize,  opts.Blue);
      if(opts.Alpha!=-1) SDL.SetAttribute(SDL.Attribute.AlphaSize, opts.Alpha);
      if(opts.DoubleBuffer!=-1) SDL.SetAttribute(SDL.Attribute.DoubleBuffer, opts.DoubleBuffer);
      if(opts.Frame!=-1) SDL.SetAttribute(SDL.Attribute.FrameDepth, opts.Frame);
      if(opts.Depth!=-1) SDL.SetAttribute(SDL.Attribute.DepthDepth, opts.Depth);
      if(opts.AccumRed  !=-1) SDL.SetAttribute(SDL.Attribute.AccumRedSize,   opts.AccumRed);
      if(opts.AccumGreen!=-1) SDL.SetAttribute(SDL.Attribute.AccumGreenSize, opts.AccumGreen);
      if(opts.AccumBlue !=-1) SDL.SetAttribute(SDL.Attribute.AccumBlueSize,  opts.AccumBlue);
      if(opts.AccumAlpha!=-1) SDL.SetAttribute(SDL.Attribute.AccumAlphaSize, opts.AccumAlpha);
    }
    SetMode(width, height, depth, (uint)(flags|SurfaceFlag.OpenGL));
    usingGL = true;
  }
  
  public static void GetGamma(out float red, out float green, out float blue)
  { red=redGamma; green=greenGamma; blue=blueGamma;
  }
  public static void SetGamma(float red, float green, float blue)
  { AssertModeSet();
    Check(SDL.SetGamma(red, green, blue));
    redGamma=red; greenGamma=green; blueGamma=blue;
  }
  
  public static void UpdateRect(Rectangle rect) { UpdateRect(rect.X, rect.Y, rect.Width, rect.Height); }
  public static void UpdateRect(int x, int y, int width, int height)
  { AssertModeSet();
    unsafe { SDL.UpdateRect(display.surface, x, y, width, height); }
  }
  public unsafe static void UpdateRects(Rectangle[] rects)
  { AssertModeSet();
    if(rects==null) throw new ArgumentNullException("rects");
    unsafe
    { SDL.Rect* array = stackalloc SDL.Rect[rects.Length];
      for(int i=0; i<rects.Length; i++) array[i] = new SDL.Rect(rects[i]);
      SDL.UpdateRects(display.surface, (uint)rects.Length, array);
    }
  }

  public static uint MapColor(Color color) { return DisplaySurface.MapColor(color); }
  public Color MapColor(uint color) { return DisplaySurface.MapColor(color); }

  public static void GetPalette(Color[] colors, int startIndex, int startColor, int numColors)
  { DisplaySurface.GetPalette(colors, startIndex, startColor, numColors);
  }
  public static void GetPalette(Color[] colors, int startIndex, int startColor, int numColors, PaletteType type)
  { DisplaySurface.GetPalette(colors, startIndex, startColor, numColors, type);
  }
  public static bool SetPalette(Color[] colors, int startIndex, int startColor, int numColors)
  { return DisplaySurface.SetPalette(colors, startIndex, startColor, numColors);
  }
  public static bool SetPalette(Color[] colors, int startIndex, int startColor, int numColors, PaletteType type)
  { return DisplaySurface.SetPalette(colors, startIndex, startColor, numColors, type);
  }

  static unsafe void SetMode(int width, int height, int depth, uint flags)
  { SDL.Surface* surface = SDL.SetVideoMode(width, height, depth, flags);
    if(surface==null) SDL.RaiseError();
    if(display!=null) display.Dispose();
    display = new Surface(surface, false);
    UpdateInfo();
    if(ModeChanged!=null) ModeChanged();
  }
  static unsafe void UpdateInfo() { info = new VideoInfo(SDL.GetVideoInfo()); }
  static void Check(int result) { if(result!=0) SDL.RaiseError(); }
  static void AssertInit()      { if(initCount==0) throw new InvalidOperationException("Not initialized"); }
  static void AssertModeSet()   { if(display==null) throw new InvalidOperationException("Video mode not set"); }

  static GammaRamp ramp;
  static Surface   display;
  static OpenGL    openGL;
  static float     redGamma=1.0f, blueGamma=1.0f, greenGamma=1.0f;
  static uint      initCount;
  static VideoInfo info;
  static bool      usingGL;
}
#endregion

#region WM class
public class WM
{ private WM() { }

  public static string WindowTitle
  { get
    { string title, icon;
      SDL.WM_GetCaption(out title, out icon);
      return title;
    }
    set { SDL.WM_SetCaption(value, null); }
  }
  
  public static bool GrabInput // TODO: possibly move this to Input?
  { get
    { SDL.GrabMode mode = SDL.WM_GrabInput(SDL.GrabMode.Query);
      return mode==SDL.GrabMode.On;
    }
    set { SDL.WM_GrabInput(value ? SDL.GrabMode.On : SDL.GrabMode.Off); }
  }
  
  public static void SetIcon(Surface icon)
  { if(icon.Width!=32 || icon.Height!=32) throw new ArgumentException("Icon should be 32x32 for compatibility");
    unsafe { SDL.WM_SetIcon(icon.surface, null); }
  }

  public static void Minimize() { if(SDL.WM_IconifyWindow()!=0) SDL.RaiseError(); }
}

#endregion

} // namespace GameLib.Video