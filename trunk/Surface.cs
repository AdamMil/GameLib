using System;
using System.Drawing;
using GameLib.Interop.SDL;
using GameLib.Interop;

namespace GameLib.Video
{

public enum ImageType
{ BMP, PNM, XPM, XCF, PCX, GIF, JPG, TIF, PNG, LBM
}

[System.Security.SuppressUnmanagedCodeSecurity()]
public class Surface : IDisposable
{ 
  [FlagsAttribute()]
  public enum Flag : uint
  { None = 0,
    // any surface
    Software  = SDL.VideoFlag.SWSurface, Hardware  = SDL.VideoFlag.HWSurface,
    HWPalette = SDL.VideoFlag.HWPalette, AsyncBlit = SDL.VideoFlag.AsyncBlit,
    // non-display surfaces only
    RLE = SDL.VideoFlag.RLEAccel, SrcAlpha=SDL.VideoFlag.SrcAlpha,
    // display surfaces only
    DoubleBuffer = SDL.VideoFlag.DoubleBuffer, Fullscreen = SDL.VideoFlag.FullScreen,
    OpenGL       = SDL.VideoFlag.OpenGL,       OpenGLBlit = SDL.VideoFlag.OpenGLBlit,
    NoFrame      = SDL.VideoFlag.NoFrame,      Resizeable = SDL.VideoFlag.Resizable,
    // display surface creation only
    AnyFormat = SDL.VideoFlag.AnyFormat
  }

  public Surface(Bitmap bitmap) { throw new NotImplementedException(); }
  public Surface(int width, int height, byte depth) : this(width, height, depth, Flag.None) { }
  public Surface(int width, int height, byte depth, Flag flags)
  { InitFromFormat(width, height, new PixelFormat(depth, (flags&Flag.SrcAlpha)!=0), flags);
  }

  public Surface(int width, int height, byte depth, uint Rmask, uint Gmask, uint Bmask, uint Amask)
    : this(width, height, depth, Rmask, Gmask, Bmask, Amask, Flag.None) { }
  public unsafe Surface(int width, int height, byte depth,
                        uint Rmask, uint Gmask, uint Bmask, uint Amask, Flag flags)
  { InitFromSurface(SDL.CreateRGBSurface((uint)flags, width, height, depth, Rmask, Gmask, Bmask, Amask));
  }

  public Surface(int width, int height, PixelFormat format) : this(width, height, format, Flag.None) { }
  public Surface(int width, int height, PixelFormat format, Flag flags)
  { InitFromFormat(width, height, format, flags);
  }
  
  public unsafe Surface(string filename) { InitFromSurface(Interop.SDLImage.Image.Load(filename)); }
  public unsafe Surface(System.IO.Stream stream)
  { StreamSource ss = new StreamSource(stream);
    fixed(SDL.RWOps* ops = &ss.ops) InitFromSurface(Interop.SDLImage.Image.Load_RW(ops, 0));
  }
  public unsafe Surface(System.IO.Stream stream, ImageType type)
  { StreamSource ss = new StreamSource(stream);
    fixed(SDL.RWOps* ops = &ss.ops)
      InitFromSurface(Interop.SDLImage.Image.LoadTyped_RW(ops, 0, Interop.SDLImage.Image.Type.Types[(int)type]));
  }

  internal unsafe Surface(SDL.Surface* surface, bool autoFree)
  { if(surface==null) throw new ArgumentNullException("surface");
    this.surface=surface; this.autoFree=autoFree;
    Init();
  }

  ~Surface() { Dispose(true); }
  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  public unsafe int Width  { get { return surface->Width; } }
  public unsafe int Height { get { return surface->Height; } }
  public Size Size   { get { return new Size(Width, Height); } }
  public byte Depth  { get { return format.Depth; } }
  public Rectangle Bounds { get { return new Rectangle(0, 0, Width, Height); } }

  public unsafe uint Pitch  { get { return surface->Pitch; } }
  public unsafe void* Data     { get { return surface->Pixels; } }
  public PixelFormat  Format   { get { return format; } }

  public unsafe uint PaletteSize
  { get
    { SDL.Palette* palette = surface->Format->Palette;
      return palette==null ? 0 : (uint)palette->Entries;
    }
  }
  public bool HasHWPalette { get { return (Flags&Flag.HWPalette) != 0; } }

  public unsafe bool  Locked   { get { return surface->Locked!=0; } }
  public unsafe Flag  Flags    { get { return (Flag)surface->Flags; } }

  public unsafe Rectangle ClipRect
  { get { return surface->ClipRect.ToRectangle(); }
    set
    { SDL.Rect rect = new SDL.Rect(value);
      SDL.SetClipRect(surface, ref rect);
    }
  }

  public Color ColorKey
  { get { return key; }
    set { if(usingKey) SetColorKey(value); else key=value; }
  }

  public byte Alpha
  { get { return alpha; }
    set { if(usingAlpha) SetAlpha(value); else alpha=value; }
  }

  public bool UsingKey
  { get { return usingKey; }
    set { if(value!=usingKey) if(value) SetColorKey(key); else DisableColorKey(); }
  }

  public bool UsingAlpha
  { get { return usingAlpha; }
    set { if(value!=usingAlpha) if(value) SetAlpha(alpha); else DisableAlpha(); }
  }

  public unsafe void Flip() { SDL.Check(SDL.Flip(surface)); }

  public void Fill()            { Fill(Bounds, MapColor(Color.Black)); }
  public void Fill(Color color) { Fill(Bounds, MapColor(color)); }
  public void Fill(uint color)  { Fill(Bounds, color); }
  public void Fill(Rectangle rect, Color color) { Fill(rect, MapColor(color)); }
  public unsafe void Fill(Rectangle rect, uint color)
  { SDL.Rect drect = new SDL.Rect(rect);
    SDL.Check(SDL.FillRect(surface, ref drect, color));
  }

  public void Blit(Surface dest) { Blit(dest, 0, 0); }
  public void Blit(Surface dest, Point dpt) { Blit(dest, dpt.X, dpt.Y); }
  public unsafe void Blit(Surface dest, int dx, int dy)
  { SDL.Rect rect = new SDL.Rect(dx, dy);
    SDL.Check(SDL.BlitSurface(surface, null, dest.surface, &rect));
  }
  public void Blit(Surface dest, Rectangle src, Point dpt) { Blit(dest, src, dpt.X, dpt.Y); }
  public unsafe void Blit(Surface dest, Rectangle src, int dx, int dy)
  { SDL.Rect srect = new SDL.Rect(src), drect = new SDL.Rect(dx, dy);
    SDL.Check(SDL.BlitSurface(surface, &srect, dest.surface, &drect));
  }

  public void  PutPixel(int x, int y, Color color) { PutPixelRaw(x, y, MapColor(color)); }
  public Color GetPixel(int x, int y) { return MapColor(GetPixelRaw(x, y)); }
  public void  PutPixelRaw(int x, int y, uint color)
  { if(!ClipRect.Contains(x, y)) return;
    bool locked=Locked;
    if(!locked) Lock();
    unsafe
    { byte* line = (byte*)Data+y*Pitch;
      switch(Depth)
      { case 8:  *(line+x) = (byte)color; break;
        case 16: *((ushort*)line+x) = (ushort)color; break;
        case 24:
          byte* ptr = line+x*3;
          *(ushort*)ptr = (ushort)color; // TODO: make big-endian safe
          *(ptr+2) = (byte)(color>>16);
          break;
        case 32: *((uint*)line+x) = color; break;
        default: throw new VideoException("Unhandled depth: "+Depth);
      }
    }
    if(!locked) Unlock();
  }
  public uint GetPixelRaw(int x, int y)
  { if(!Bounds.Contains(x, y)) throw new ArgumentOutOfRangeException();
    bool locked=Locked;
    if(!locked) Lock();
    try
    { unsafe
      { byte* line = (byte*)Data+y*Pitch;
        switch(Depth)
        { case 8:  return *(line+x);
          case 16: return *((ushort*)line+x);
          case 24:
            byte* ptr = line+x*3;
            return *(ushort*)ptr | (uint)(*(ptr+2)<<16); // TODO: make big-endian safe
          case 32: return *((uint*)line+x);
          default: throw new VideoException("Unhandled depth: "+Depth);
        }
      }
    }
    finally { if(!locked) Unlock(); }
  }
  
  public Color GetColorKey() { return key; }
  public unsafe void SetColorKey(Color color) // automatically enables color key
  { SDL.Check(SDL.SetColorKey(surface, (uint)SDL.VideoFlag.SrcColorKey, MapColor(key=color)));
  }
  public unsafe void DisableColorKey() { SDL.Check(SDL.SetColorKey(surface, 0, 0)); }

  public byte GetAlpha() { return alpha; }
  public unsafe void SetAlpha(byte alpha) // automatically enables alpha
  { SDL.Check(SDL.SetAlpha(surface, (uint)((alpha==AlphaLevel.Opaque ? SDL.VideoFlag.None : SDL.VideoFlag.SrcAlpha) |
                                    (usingRLE ? SDL.VideoFlag.RLEAccel : 0)), alpha));
  }
  public unsafe void DisableAlpha() { SDL.Check(SDL.SetAlpha(surface, 0, 0)); }

  public unsafe void Lock()   { SDL.Check(SDL.LockSurface(surface)); }
  public unsafe void Unlock() { SDL.UnlockSurface(surface); }

  public unsafe uint MapColor(Color color)
  { return SDL.MapRGBA(surface->Format, color.R, color.G, color.B, color.A);
  }
  public unsafe Color MapColor(uint color)
  { byte r, g, b, a;
    SDL.GetRGBA(color, surface->Format, out r, out g, out b, out a);
    return Color.FromArgb(a, r, g, b);
  }

  public void GetPalette(Color[] colors) { GetPalette(colors, 0, 0, 256); }
  public void GetPalette(Color[] colors, uint startIndex, uint startColor, uint numColors)
  { GetPalette(colors, startIndex, startColor, numColors, PaletteType.Both);
  }
  public unsafe void GetPalette(Color[] colors, uint startIndex, uint startColor, uint numColors, PaletteType type)
  { ValidatePaletteArgs(colors, startColor, numColors);

    SDL.Color* palette = surface->Format->Palette->Colors;
    for(uint i=0; i<numColors; i++)
    { SDL.Color c = palette[startColor+i];
      colors[startIndex+i] = Color.FromArgb(c.Red, c.Green, c.Blue);
    }
  }

  public bool SetPalette(Color[] colors) { return SetPalette(colors, 0, 0, 256); }
  public bool SetPalette(Color[] colors, uint startIndex, uint startColor, uint numColors)
  { return SetPalette(colors, startIndex, startColor, numColors, PaletteType.Both);
  }
  public unsafe bool SetPalette(Color[] colors, uint startIndex, uint startColor, uint numColors, PaletteType type)
  { ValidatePaletteArgs(colors, startColor, numColors);

    SDL.Color* array = stackalloc SDL.Color[(int)numColors];
    for(uint i=0; i<numColors; i++)
    { Color c = colors[startIndex+i];
      array[i].Red   = c.R;
      array[i].Green = c.G;
      array[i].Blue  = c.B;
    }
    return SDL.SetPalette(surface, (uint)type, array, startColor, numColors)==1;
  }

  public Surface CreateCompatible(int width, int height) { return CreateCompatible(width, height, Flags); }
  public unsafe Surface CreateCompatible(int width, int height, Flag flags)
  { SDL.Surface* ret = SDL.CreateRGBSurface((uint)flags, width, height, format.Depth, format.RedMask,
                                            format.GreenMask, format.BlueMask, format.AlphaMask);
    if(ret==null) SDL.RaiseError();
    return new Surface(ret, true);
  }

  public Surface Clone()                   { return Clone(Format, Flags); }
  public Surface Clone(PixelFormat format) { return Clone(format, Flags); }
  public Surface Clone(Flag flags)         { return Clone(Format, flags); }
  public unsafe Surface Clone(PixelFormat format, Flag flags)
  { SDL.Surface* ret;
    fixed(SDL.PixelFormat* pf = &format.format) ret = SDL.ConvertSurface(surface, pf, (uint)flags);
    if(ret==null) SDL.RaiseError();
    return new Surface(ret, true);
  }

  public unsafe Surface CloneDisplay() { return CloneDisplay(Format.AlphaMask!=0); }
  public unsafe Surface CloneDisplay(bool alphaChannel)
  { SDL.Surface* ret = alphaChannel ? SDL.DisplayFormatAlpha(surface) : SDL.DisplayFormat(surface);
    if(ret==null) SDL.RaiseError();
    return new Surface(ret, true);
  }

  public Bitmap ToBitmap() { throw new NotImplementedException(); }

  internal unsafe void InitFromSurface(SDL.Surface* surface)
  { if(surface==null) SDL.RaiseError();
    this.surface=surface;
    autoFree = true;
    Init();
  }
  protected unsafe void Init()
  { format   = new PixelFormat(surface->Format);
    usingKey = usingAlpha = false;
    usingRLE = (surface->Flags&(uint)SDL.VideoFlag.RLEAccel)!=0;
  }
  protected unsafe void InitFromFormat(int width, int height, PixelFormat format, Flag flags)
  { InitFromSurface(SDL.CreateRGBSurface((uint)flags, width, height, format.Depth, format.RedMask,
                                         format.GreenMask, format.BlueMask, format.AlphaMask));
  }

  protected unsafe void ValidatePaletteArgs(Color[] colors, uint startColor, uint numColors)
  { SDL.Palette* palette = surface->Format->Palette;
    if(palette==null) throw new VideoException("This surface does not have an associated palette.");
    uint max = (uint)palette->Entries;
    if(numColors==0) return;
    if(numColors>max || startColor+numColors>max) throw new ArgumentOutOfRangeException();
    if(colors==null) throw new ArgumentNullException("array");
  }

  protected unsafe void Dispose(bool destructor)
  { if(autoFree && surface!=null)
    { SDL.FreeSurface(surface);
      surface=null;
    }
  }

  protected PixelFormat format;
  protected Color key;
  protected byte  alpha;
  protected bool  usingKey, usingAlpha;

  internal unsafe SDL.Surface* surface;
  private  bool   autoFree, usingRLE;
}

} // namespace GameLib.Video