/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2004 Adam Milazzo

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
*/

using System;
using System.Drawing;
using System.IO;
using GameLib.IO;
using GameLib.Interop;
using GameLib.Interop.SDL;

namespace GameLib.Video
{

public interface IBlittable
{ int  Width  { get; }
  int  Height { get; }
  bool StaticImage  { get; }
  bool ImageChanged { get; }

  void Blit(Surface dest, int dx, int dy);
  void Blit(Surface dest, Rectangle src, int dx, int dy);
  IBlittable CreateCompatible();
}

public enum ImageType
{ BMP, PNM, XPM, XCF, PCX, GIF, JPG, TIF, PNG, LBM
}

[Flags]
public enum SurfaceFlag : uint
{ None = 0,
  // any surface
  Software  = SDL.VideoFlag.SWSurface, Hardware  = SDL.VideoFlag.HWSurface,
  HWPalette = SDL.VideoFlag.HWPalette, AsyncBlit = SDL.VideoFlag.AsyncBlit,
  // non-display surfaces only
  RLE = SDL.VideoFlag.RLEAccel, SrcAlpha=SDL.VideoFlag.SrcAlpha,
  // display surfaces only
  DoubleBuffer = SDL.VideoFlag.DoubleBuffer, Fullscreen = SDL.VideoFlag.FullScreen,
  OpenGL       = SDL.VideoFlag.OpenGL,       /* DEPRECATED OpenGLBlit = SDL.VideoFlag.OpenGLBlit,*/
  NoFrame      = SDL.VideoFlag.NoFrame,      Resizeable = SDL.VideoFlag.Resizable,
  // display surface creation only
  AnyFormat = SDL.VideoFlag.AnyFormat
}

[System.Security.SuppressUnmanagedCodeSecurity()]
public class Surface : IBlittable, IDisposable
{ public Surface(Bitmap bitmap)
  { System.Drawing.Imaging.PixelFormat format;
    byte depth;
    switch(bitmap.PixelFormat)
    { case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
      case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
      case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
        format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
        depth  = 8;
        break;
      case System.Drawing.Imaging.PixelFormat.Format16bppRgb555:
      case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
        format = System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
        depth  = 16;
        break;
      case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
      default:
        format = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
        depth  = 24;
        break;
      case System.Drawing.Imaging.PixelFormat.Format16bppArgb1555:
      case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
      case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
      case System.Drawing.Imaging.PixelFormat.Format64bppArgb:
      case System.Drawing.Imaging.PixelFormat.Format64bppPArgb:
        format = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
        depth  = 32;
        break;
    }

    InitFromFormat(bitmap.Width, bitmap.Height, new PixelFormat(depth, depth==32),
                   depth==32 ? SurfaceFlag.SrcAlpha : SurfaceFlag.None);
    try
    { System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                                               System.Drawing.Imaging.ImageLockMode.ReadOnly, format);
      unsafe
      { if(depth==8) SetPalette(bitmap.Palette.Entries);
        Lock();
        byte* src=(byte*)data.Scan0, dest=(byte*)Data;
        int len=bitmap.Width*(depth/8);
        for(int i=0; i<bitmap.Height; src+=data.Stride,dest+=Pitch,i++) Unsafe.Copy(dest, src, len); // TODO: big endian safe?
        Unlock();
      }
    }
    catch { Dispose(); }
  }
  public Surface(int width, int height, byte depth) : this(width, height, depth, SurfaceFlag.None) { }
  public Surface(int width, int height, byte depth, SurfaceFlag flags)
  { InitFromFormat(width, height, new PixelFormat(depth, (flags&SurfaceFlag.SrcAlpha)!=0), flags);
  }

  public Surface(int width, int height, byte depth, uint Rmask, uint Gmask, uint Bmask, uint Amask)
    : this(width, height, depth, Rmask, Gmask, Bmask, Amask, SurfaceFlag.None) { }
  public unsafe Surface(int width, int height, byte depth,
                        uint Rmask, uint Gmask, uint Bmask, uint Amask, SurfaceFlag flags)
  { InitFromSurface(SDL.CreateRGBSurface((uint)flags, width, height, depth, Rmask, Gmask, Bmask, Amask));
  }

  public Surface(int width, int height, PixelFormat format) : this(width, height, format, SurfaceFlag.None) { }
  public Surface(int width, int height, PixelFormat format, SurfaceFlag flags)
  { InitFromFormat(width, height, format, flags);
  }
  
  public unsafe Surface(string filename) { InitFromSurface(Interop.SDLImage.Image.Load(filename)); }
  public unsafe Surface(string filename, ImageType type)
  { SDL.RWOps* ops = SDL.RWFromFile(filename, "rb");
    if(ops==null) throw new System.IO.FileNotFoundException("The file could not be opened", filename);
    InitFromSurface(Interop.SDLImage.Image.LoadTyped_RW(ops, 1, Interop.SDLImage.Image.Type.Types[(int)type]));
  }
  public unsafe Surface(System.IO.Stream stream) : this(stream, true) { }
  public unsafe Surface(System.IO.Stream stream, bool autoClose)
  { SeekableStreamRWOps ss = new SeekableStreamRWOps(stream, autoClose);
    fixed(SDL.RWOps* ops = &ss.ops) InitFromSurface(Interop.SDLImage.Image.Load_RW(ops, 0));
  }
  public unsafe Surface(System.IO.Stream stream, ImageType type) : this(stream, type, true) { }
  public unsafe Surface(System.IO.Stream stream, ImageType type, bool autoClose)
  { SeekableStreamRWOps ss = new SeekableStreamRWOps(stream, autoClose);
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
  public bool StaticImage  { get { return true; } }
  public bool ImageChanged { get { return false; } }
  public byte Depth  { get { return format.Depth; } }
  public Size Size   { get { return new Size(Width, Height); } }
  public Rectangle Bounds { get { return new Rectangle(0, 0, Width, Height); } }

  public unsafe uint Pitch  { get { return surface->Pitch; } }
  public unsafe void* Data     { get { return surface->Pixels; } }
  public PixelFormat  Format   { get { return format; } }

  public unsafe int PaletteSize
  { get
    { SDL.Palette* palette = surface->Format->Palette;
      return palette==null ? 0 : palette->Entries;
    }
  }
  public bool HasHWPalette { get { return (Flags&SurfaceFlag.HWPalette) != 0; } }

  public unsafe bool        Locked { get { return lockCount!=0; } }
  public unsafe SurfaceFlag  Flags { get { return (SurfaceFlag)surface->Flags; } }

  public unsafe Rectangle ClipRect
  { get { return surface->ClipRect.ToRectangle(); }
    set
    { SDL.Rect rect = new SDL.Rect(value);
      SDL.SetClipRect(surface, ref rect);
    }
  }

  public Color ColorKey
  { get { return key; }
    set { if(UsingKey) SetColorKey(value); else key=value; }
  }

  public byte Alpha
  { get { return alpha; }
    set { if(UsingAlpha) SetSurfaceAlpha(value); else alpha=value; }
  }

  public bool UsingKey
  { get { unsafe { return (surface->Flags&(uint)SDL.VideoFlag.SrcColorKey)!=0; } }
    set { if(value!=UsingKey) if(value) SetColorKey(key); else DisableColorKey(); }
  }

  public bool UsingAlpha
  { get { return (Flags&SurfaceFlag.SrcAlpha) != SurfaceFlag.None; }
    set { if(value!=UsingAlpha) if(value) EnableAlpha(); else DisableAlpha(); }
  }
  
  public bool UsingRLE
  { get { return (Flags&SurfaceFlag.RLE) != SurfaceFlag.None; }
    set { if(value) EnableRLE(); else DisableRLE(); }
  }

  public unsafe void Flip() { SDL.Check(SDL.Flip(surface)); }

  public void Fill()            { Fill(Bounds, MapColor(Color.Black)); }
  public void Fill(Color color) { Fill(Bounds, MapColor(color)); }
  public void Fill(uint color)  { Fill(Bounds, color); }
  public void Fill(Rectangle rect, Color color) { Fill(rect, MapColor(color)); }
  public unsafe void Fill(Rectangle rect, uint color)
  { SDL.Rect drect = new SDL.Rect(rect);
    bool locked = Locked;
    if(locked) Unlock();
    try { SDL.Check(SDL.FillRect(surface, ref drect, color)); }
    finally { if(locked) Lock(); }
  }

  public void Blit(Surface dest) { Blit(dest, 0, 0); }
  public void Blit(Surface dest, Point dpt) { Blit(dest, dpt.X, dpt.Y); }
  public unsafe void Blit(Surface dest, int dx, int dy)
  { SDL.Rect rect = new SDL.Rect(dx, dy);
    bool sl = Locked, dl = dest.Locked;
    try
    { if(sl) Unlock();
      if(dl) dest.Unlock();
      SDL.Check(SDL.BlitSurface(surface, null, dest.surface, &rect));
    }
    finally
    { if(sl) Lock();
      if(dl) dest.Lock();
    }
  }
  public void Blit(Surface dest, Rectangle src, Point dpt) { Blit(dest, src, dpt.X, dpt.Y); }
  public unsafe void Blit(Surface dest, Rectangle src, int dx, int dy)
  { SDL.Rect srect = new SDL.Rect(src), drect = new SDL.Rect(dx, dy);
    bool sl = Locked, dl = dest.Locked;
    try
    { if(sl) Unlock();
      if(dl) dest.Unlock();
      SDL.Check(SDL.BlitSurface(surface, &srect, dest.surface, &drect));
    }
    finally
    { if(sl) Lock();
      if(dl) dest.Lock();
    }
  }

  public void  PutPixel(int x, int y, Color color) { PutPixelRaw(x, y, MapColor(color)); }
  public Color GetPixel(int x, int y) { return MapColor(GetPixelRaw(x, y)); }
  public void  PutPixelRaw(int x, int y, uint color)
  { if(!ClipRect.Contains(x, y)) return;
    Lock();
    try
    { unsafe
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
    }
    finally { Unlock(); }
  }
  public uint GetPixelRaw(int x, int y)
  { if(!Bounds.Contains(x, y)) throw new ArgumentOutOfRangeException();
    Lock();
    try
    { unsafe
      { byte* line = (byte*)Data+y*Pitch;
        switch(Depth)
        { case 8:  return *(line+x);
          case 16: return *((ushort*)line+x);
          case 24:
            byte* ptr = line+x*3;
            #if BIGENDIAN
            return (*(ushort*)ptr<<8) | *(ptr+2); // TODO: make big-endian safe
            #else
            return *(ushort*)ptr | (uint)(*(ptr+2)<<16); // TODO: make big-endian safe
            #endif
          case 32: return *((uint*)line+x);
          default: throw new VideoException("Unhandled depth: "+Depth);
        }
      }
    }
    finally { Unlock(); }
  }
  
  public Color GetColorKey() { return key; }
  public unsafe void SetColorKey(Color color) // automatically enables color key
  { SDL.Check(SDL.SetColorKey(surface, (uint)SDL.VideoFlag.SrcColorKey, rawKey=MapColor(key=color)));
  }
  public unsafe void DisableColorKey() { SDL.Check(SDL.SetColorKey(surface, 0, 0)); }

  public byte GetSurfaceAlpha() { return alpha; }
  public unsafe void SetSurfaceAlpha(byte alpha) // automatically enables alpha if 'alpha' != AlphaLevel.Opaque
  { if(alpha==AlphaLevel.Opaque) DisableAlpha();
    else
    { SDL.Check(SDL.SetAlpha(surface, (uint)(SDL.VideoFlag.SrcAlpha | (UsingRLE ? SDL.VideoFlag.RLEAccel : 0)), alpha));
    }
  }
  public unsafe void EnableAlpha()
  { SDL.Check(SDL.SetAlpha(surface, (uint)((UsingRLE ? SDL.VideoFlag.RLEAccel : 0) | SDL.VideoFlag.SrcAlpha), alpha));
  }
  public unsafe void DisableAlpha()
  { SDL.Check(SDL.SetAlpha(surface, (uint)(UsingRLE ? SDL.VideoFlag.RLEAccel : 0), AlphaLevel.Opaque));
  }
  
  public unsafe void EnableRLE()
  { if(UsingRLE) return;
    SDL.Check(SDL.SetColorKey(surface,
                              (uint)((UsingKey ? SDL.VideoFlag.SrcColorKey : 0) | SDL.VideoFlag.RLEAccel), rawKey));
  }
  public unsafe void DisableRLE()
  { if(!UsingRLE) return;
    SDL.Check(SDL.SetColorKey(surface, UsingKey ? (uint)SDL.VideoFlag.SrcColorKey : 0, rawKey));
  }

  public unsafe void Lock()   { if(lockCount++==0) SDL.Check(SDL.LockSurface(surface)); }
  public unsafe void Unlock()
  { if(lockCount==0) throw new InvalidOperationException("Unlock called too many times");
    if(--lockCount==0) SDL.UnlockSurface(surface);
  }

  public unsafe uint MapColor(Color color)
  { return SDL.MapRGBA(surface->Format, color.R, color.G, color.B, color.A);
  }
  public unsafe uint MapColor(Color color, byte alpha)
  { return SDL.MapRGBA(surface->Format, color.R, color.G, color.B, alpha);
  }
  public unsafe Color MapColor(uint color)
  { byte r, g, b, a;
    SDL.GetRGBA(color, surface->Format, out r, out g, out b, out a);
    return Color.FromArgb(a, r, g, b);
  }

  public void GetPalette(Color[] colors) { GetPalette(colors, 0, 0, colors.Length); }
  public void GetPalette(Color[] colors, int numColors) { GetPalette(colors, 0, 0, numColors); }
  public void GetPalette(Color[] colors, int startIndex, int startColor, int numColors)
  { GetPalette(colors, startIndex, startColor, numColors, PaletteType.Both);
  }
  public unsafe void GetPalette(Color[] colors, int startIndex, int startColor, int numColors, PaletteType type)
  { ValidatePaletteArgs(colors, startColor, numColors);

    SDL.Color* palette = surface->Format->Palette->Colors;
    for(int i=0; i<numColors; i++)
    { SDL.Color c = palette[startColor+i];
      colors[startIndex+i] = Color.FromArgb(c.Red, c.Green, c.Blue);
    }
  }

  public bool SetPalette(Color[] colors) { return SetPalette(colors, 0, 0, colors.Length); }
  public bool SetPalette(Color[] colors, int numColors) { return SetPalette(colors, 0, 0, numColors); }
  public bool SetPalette(Color[] colors, int startIndex, int startColor, int numColors)
  { return SetPalette(colors, startIndex, startColor, numColors, PaletteType.Both);
  }
  public unsafe bool SetPalette(Color[] colors, int startIndex, int startColor, int numColors, PaletteType type)
  { ValidatePaletteArgs(colors, startColor, numColors);

    SDL.Color* array = stackalloc SDL.Color[numColors];
    for(int i=0; i<numColors; i++)
    { Color c = colors[startIndex+i];
      array[i].Red   = c.R;
      array[i].Green = c.G;
      array[i].Blue  = c.B;
    }
    return SDL.SetPalette(surface, (uint)type, array, startColor, numColors)==1;
  }

  public Surface CreateCompatible(int width, int height) { return CreateCompatible(width, height, Flags); }
  public unsafe Surface CreateCompatible(int width, int height, SurfaceFlag flags)
  { SDL.Surface* ret = SDL.CreateRGBSurface((uint)flags, width, height, format.Depth, format.RedMask,
                                            format.GreenMask, format.BlueMask, format.AlphaMask);
    if(ret==null) SDL.RaiseError();
    return new Surface(ret, true);
  }

  public Surface Clone()                   { return Clone(Format, Flags); }
  public Surface Clone(PixelFormat format) { return Clone(format, Flags); }
  public Surface Clone(SurfaceFlag flags)  { return Clone(Format, flags); }
  public unsafe Surface Clone(PixelFormat format, SurfaceFlag flags)
  { SDL.Surface* ret;
    fixed(SDL.PixelFormat* pf = &format.format) ret = SDL.ConvertSurface(surface, pf, (uint)flags);
    if(ret==null) SDL.RaiseError();
    return new Surface(ret, true);
  }

  public IBlittable CreateCompatible() { return CloneDisplay(); }
  public unsafe Surface CloneDisplay() { return CloneDisplay(Format.AlphaMask!=0); }
  public unsafe Surface CloneDisplay(bool alphaChannel)
  { SDL.Surface* ret = alphaChannel ? SDL.DisplayFormatAlpha(surface) : SDL.DisplayFormat(surface);
    if(ret==null) SDL.RaiseError();
    return new Surface(ret, true);
  }

  public Bitmap ToBitmap()
  { System.Drawing.Imaging.PixelFormat format;
    switch(Depth) // FIXME: make this work with nonstandard byte packing (BGR, 555, etc)
    { case 8:  format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed; break;
      case 16: format = System.Drawing.Imaging.PixelFormat.Format16bppRgb565; break;
      case 24: format = System.Drawing.Imaging.PixelFormat.Format24bppRgb; break;
      case 32:
        format = Format.AlphaMask==0 ? System.Drawing.Imaging.PixelFormat.Format32bppRgb :
                                       System.Drawing.Imaging.PixelFormat.Format32bppArgb;
        break;
      default: throw new NotImplementedException("Unhandled depth in ToBitmap()");
    }
    Bitmap bitmap;
    Lock();
    unsafe
    { try { bitmap = new Bitmap(Width, Height, (int)Pitch, format, new IntPtr(Data)); }
      finally { Unlock(); }
    }
    if(Depth==8)
    { System.Drawing.Imaging.ColorPalette pal = bitmap.Palette;
      GetPalette(pal.Entries, PaletteSize);
      bitmap.Palette = pal;
    }
    return bitmap;
  }
  
  public void Save(string filename, ImageType type)
  { Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
    try { Save(stream, type); }
    finally { stream.Close(); }
  }
  public void Save(Stream stream, ImageType type)
  { Bitmap bitmap=null;
    switch(type)
    { case ImageType.PCX: WritePCX(stream); break;
      case ImageType.BMP: (bitmap=ToBitmap()).Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);  break;
      case ImageType.GIF: (bitmap=ToBitmap()).Save(stream, System.Drawing.Imaging.ImageFormat.Gif);  break;
      case ImageType.JPG: (bitmap=ToBitmap()).Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg); break;
      case ImageType.PNG: (bitmap=ToBitmap()).Save(stream, System.Drawing.Imaging.ImageFormat.Png);  break;
      case ImageType.TIF: (bitmap=ToBitmap()).Save(stream, System.Drawing.Imaging.ImageFormat.Tiff); break;
      default: throw new NotImplementedException();
    }
    if(bitmap!=null) bitmap.Dispose();
  }
  public void Save(string filename, System.Drawing.Imaging.ImageCodecInfo encoder,
                   System.Drawing.Imaging.EncoderParameters parms)
  { Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
    try { Save(stream, encoder, parms); }
    finally { stream.Close(); }
  }
  public void Save(Stream stream, System.Drawing.Imaging.ImageCodecInfo encoder,
                   System.Drawing.Imaging.EncoderParameters parms)
  { Bitmap bitmap = ToBitmap();
    bitmap.Save(stream, encoder, parms);
    bitmap.Dispose();
  }

  public void SaveJPEG(string filename, int quality)
  { Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
    try { SaveJPEG(stream, quality); }
    finally { stream.Close(); }
  }
  public void SaveJPEG(Stream stream, int quality)
  { if(quality<0 || quality>100) throw new ArgumentOutOfRangeException("quality", quality, "must be from 0-100");
    foreach(System.Drawing.Imaging.ImageCodecInfo codec in System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders())
      if(codec.MimeType.ToLower()=="image/jpeg")
      { System.Drawing.Imaging.EncoderParameters parms = new System.Drawing.Imaging.EncoderParameters(1);
        parms.Param[0] =
          new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);
        Bitmap bitmap = ToBitmap();
        bitmap.Save(stream, codec, parms);
        bitmap.Dispose();
        return;
      }
    throw new CodecNotFoundException("JPEG");
  }

  internal unsafe void InitFromSurface(SDL.Surface* surface)
  { if(surface==null) SDL.RaiseError();
    this.surface=surface;
    autoFree = true;
    Init();
  }

  protected unsafe void Init()
  { format = new PixelFormat(surface->Format);
  }
  protected unsafe void InitFromFormat(int width, int height, PixelFormat format, SurfaceFlag flags)
  { InitFromSurface(SDL.CreateRGBSurface((uint)flags, width, height, format.Depth, format.RedMask,
                                         format.GreenMask, format.BlueMask, format.AlphaMask));
  }

  protected unsafe void ValidatePaletteArgs(Color[] colors, int startColor, int numColors)
  { SDL.Palette* palette = surface->Format->Palette;
    if(palette==null) throw new VideoException("This surface does not have an associated palette.");
    int max = palette->Entries;
    if(startColor<0 || numColors<0 || numColors>max || startColor+numColors>max)
      throw new ArgumentOutOfRangeException();
    if(colors==null) throw new ArgumentNullException("array");
  }

  protected void WritePCX(Stream stream)
  { byte[] pbuf = new byte[768];
    stream.WriteByte(10);           // manufacturer. 10 = z-soft
    stream.WriteByte(5);            // version
    stream.WriteByte(1);            // encoding. 1 = RLE
    stream.WriteByte(8);            // bits per pixel (where pixel is a single item on a plane)
    IOH.WriteLE2(stream, 0);        // xmin
    IOH.WriteLE2(stream, 0);        // ymin
    IOH.WriteLE2(stream, (short)(Width-1));  // xmax
    IOH.WriteLE2(stream, (short)(Height-1)); // ymax
    IOH.WriteLE2(stream, 72);       // DPI
    IOH.WriteLE2(stream, 72);       // DPI
    stream.Write(pbuf, 0, 48);      // color map
    stream.WriteByte(0);            // reserved field
    int bpp = Width, bpl;
    if((bpp&1)!=0) bpp++;
    if(Depth==8)
    { bpl = bpp;
      stream.WriteByte(1); // planes
    }
    else
    { bpl = bpp*3;
      stream.WriteByte(3); // planes
    }
    IOH.WriteLE2(stream, (short)bpp); // bytes per plane line
    IOH.WriteLE2(stream, 1);          // palette type. 1 = color
    stream.Write(pbuf, 0, 58);        // filler
    
    Lock();
    unsafe
    { try
      { byte* mem = (byte*)Data;
        int   x, y, width=Width;
        if(Depth==8)
        { byte  c, count;
          for(y=0; y<Height; y++)
          { for(x=0; x<width;)
            { count=0; c=mem[x];
              do
              { count++; x++;
                if(count==63 || x==width) break;
              } while(mem[x]==c);
              if(c>=192 || count>1) stream.WriteByte((byte)(192+count)); // encode as a run
              stream.WriteByte(c);
            }
            if((Width&1)!=0) stream.WriteByte(0);
            mem += Pitch;
          }
          stream.WriteByte(12); // byte 12 precedes palette
          Color[] pal = new Color[256];
          GetPalette(pal, PaletteSize);
          for(int i=0,j=0; i<256; i++)
          { pbuf[j++] = pal[i].R;
            pbuf[j++] = pal[i].G;
            pbuf[j++] = pal[i].B;
          }
          stream.Write(pbuf, 0, 768); // palette
        }
        else // Depth > 8
        { byte[] linebuf = new byte[bpl];
          fixed(byte* line = linebuf)
          { byte*  omem = mem;
            Color  c;
            int    i;
            byte   p, count;
            for(y=0; y<Height; y++)
            { if(Depth==16)
                for(x=0; x<width; mem+=2,x++)
                { c = MapColor((uint)*(ushort*)mem);
                  line[x]=c.R; line[x+bpp]=c.G; line[x+bpp*2]=c.B;
                }
              else if(Depth==24)
                for(x=0; x<width; mem+=3,x++)
                { c = MapColor((uint)(mem[0]+(mem[1]<<8)+(mem[2]<<16))); // TODO: big-endian safe?
                  line[x]=c.R; line[x+bpp]=c.G; line[x+bpp*2]=c.B;
                }
              else if(Depth==32)
                for(x=0; x<width; mem+=4,x++)
                { c = MapColor(*(uint*)mem);
                  line[x]=c.R; line[x+bpp]=c.G; line[x+bpp*2]=c.B;
                }
              omem += Pitch; mem = omem;

              for(i=0; i<bpl; )
              { count=0; p=line[i];
                do
                { count++; i++;
                  if(count==63 || i==bpl) break;
                } while(line[i]==p);
                if(p>=192 || count>1) stream.WriteByte((byte)(192+count)); // encode as a run
                stream.WriteByte(p);
              }
            }
          }
        }
      }
      finally { Unlock(); }
    }
  }
  
  protected unsafe void Dispose(bool destructor)
  { if(autoFree && surface!=null)
    { SDL.FreeSurface(surface);
      surface=null;
    }
  }

  protected PixelFormat format;
  protected Color key;
  protected uint  lockCount, rawKey;
  protected byte  alpha=AlphaLevel.Opaque;

  internal unsafe SDL.Surface* surface;
  private  bool   autoFree;
}

} // namespace GameLib.Video