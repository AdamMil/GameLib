/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2010 Adam Milazzo

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
using AdamMil.Utilities;
using GameLib.Interop.SDL;
using SysColor = System.Drawing.Color;

namespace GameLib.Video
{

#region Enums and supporting types
/// <summary>An enum of image formats used when saving and loading images.</summary>
public enum ImageType
{
  // NOTE: The initial items must be in the same order as the interop enumeration!
  /// <summary>Windows bitmap (.BMP) format.</summary>
  Bmp,
  /// <summary>GIF (Graphics Interchange Format).</summary>
  Gif,
  /// <summary>JPEG (Joint Photographic Experts Group) format.</summary>
  Jpeg,
  /// <summary>Deluxe Paint .LBM format.</summary>
  Lbm,
  /// <summary>PC Paintbrush format.</summary>
  Pcx,
  /// <summary>PNG (Portable Network Graphics) format.</summary>
  Png,
  /// <summary>Portable PixMap, Portable GreyMap, and Portable PixMap formats.</summary>
  Pnm,
  /// <summary>TARGA (Truevision Advanced Raster Graphics Adapter).</summary>
  Tga,
  /// <summary>TIFF (Tagged Image File Format).</summary>
  Tiff,
  /// <summary>GIMP native format.</summary>
  Xcf,
  /// <summary>XPicMap format.</summary>
  Xpm,

  // Note: The rest don't have to be in any particular order.
  /// <summary>Adobe Photoshop .PSD format.</summary>
  Psd
}

[Flags]
public enum SurfaceFlag
{
  /// <summary>Default flags.</summary>
  None = 0,

  /* surfaces only */
  /// <summary>The surface will be run-length encoded.</summary>
  /// <remarks>Run-length encoded surfaces can be used to speed up blitting in the rare case that hardware
  /// acceleration cannot be used and the surface is stored in system memory and it contains blocks of solid
  /// color and does not change often. This flag is not to be used when setting the video mode.
  /// </remarks>
  RunLengthEncoding = (int)SDL.VideoFlag.RLEAccel,
  /// <summary>When blitted, the surface will use alpha blending.</summary>
  /// <remarks>This flag is not to be used when setting the video mode, and only makes sense for surfaces
  /// that contain an alpha channel or use a surface alpha.
  /// </remarks>
  SourceAlpha = (int)SDL.VideoFlag.SrcAlpha,

  /* surfaces and mode setting */
  /// <summary>Asynchronous blits will be used if possible.</summary>
  /// <remarks>This flag can be used during surface creation and mode setting. It indicates that blits should return
  /// immediately if possible, setting up the blit to happen in the background. This may slow down blitting on
  /// single CPU machines, but may provide a speed increase on SMP systems.
  /// </remarks>
  AsyncronousBlit = (int)SDL.VideoFlag.AsyncBlit,
  /// <summary>The surface will be stored in video memory if possible.</summary>
  /// <remarks>When setting the video mode, this flag indicates that a video mode which allows hardware surfaces
  /// is desired.
  /// </remarks>
  Hardware = (int)SDL.VideoFlag.HWSurface,

  /* mode setting only */
  /// <summary>Enables double buffering for the display surface.</summary>
  /// <remarks>This flag is only used when setting the video mode, and is only valid in conjunction with the
  /// <see cref="Hardware"/> flag. Using double buffering will reduce tearing and will allow the
  /// <see cref="Video.Flip"/> call to return immediately in most cases. Not all systems support double buffering,
  /// though, and if it cannot be enabled, a single hardware surface will be attempted instead, falling back to a
  /// software surface if that, too, fails.
  /// </remarks>
  DoubleBuffer = (int)SDL.VideoFlag.DoubleBuffer,
  /// <summary>An attempt will be made to use a fullscreen video mode.</summary>
  Fullscreen = unchecked((int)SDL.VideoFlag.FullScreen),
  /// <summary>Disable video mode emulation.</summary>
  /// <remarks>Normally, if a mode cannot be set at a specified bit depth, an attempt will be made to emulate
  /// the bit depth with a shadow surface. Specifying this flag causes <see cref="Video.SetMode"/> to not
  /// emulate the color depth but instead just use whichever color depth is closest. The format of the
  /// <see cref="Video.DisplaySurface"/> may not be what is expected if this flag is used.
  /// </remarks>
  NoEmulation = (int)SDL.VideoFlag.AnyFormat,
  /// <summary>For windowed video modes, this flag attempts to remove the title bar and frame decoration from
  /// the window.
  /// </summary>
  NoFrame = (int)SDL.VideoFlag.NoFrame,
  /// <summary>The surface has an associated OpenGL rendering context.</summary>
  /// <remarks>If passed to <see cref="Video.SetMode"/>, an OpenGL video mode will be set.
  /// <see cref="Video.SetGLMode"/> automatically applies this flag.
  /// </remarks>
  OpenGL = (int)SDL.VideoFlag.OpenGL,
  /// <summary>For windowed video modes, this flag attempts to make the window resizeable.</summary>
  /// <remarks>Applications should handle the <see cref="Events.ResizeEvent">resize event</see> if they use
  /// this flag.
  /// </remarks>
  Resizeable = (int)SDL.VideoFlag.Resizable,
  /// <summary>This surface will have the physical paletted associated with it.</summary>
  /// <remarks>This flag can be used when setting the video mode to indicate that the display surface can be used
  /// to control the physical palette.
  /// </remarks>
  PhysicalPalette = (int)SDL.VideoFlag.HWPalette
}
#endregion

[System.Security.SuppressUnmanagedCodeSecurity]
public sealed class Surface : IDisposable, IGuiRenderTarget
{
  /// <include file="../documentation.xml" path="//Video/Surface/Cons/FromBmp/*"/>
  /// <remarks>Using this is equivalent to using <see cref="Surface(Bitmap,SurfaceFlag)"/> and passing
  /// <see cref="SurfaceFlag.None"/>. You may want to use <see cref="CloneDisplay"/> to convert the surface
  /// into something that matches the display surface, for efficiency.
  /// </remarks>
  public Surface(Bitmap bitmap) : this(bitmap, SurfaceFlag.None) { }

  /// <include file="../documentation.xml" path="//Video/Surface/Cons/FromBmp/*"/>
  /// <param name="flags">The <see cref="SurfaceFlag">flags</see> to use when initializing this
  /// surface. The <see cref="SurfaceFlag.SourceAlpha"/> flag is automatically set if the bitmap contains an alpha
  /// channel. You may want to use <see cref="CloneDisplay"/> to convert the surface
  /// into something that matches the display surface, for efficiency.
  /// </param>
  public Surface(Bitmap bitmap, SurfaceFlag flags)
  {
    System.Drawing.Imaging.PixelFormat format;
    int depth;
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
      case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
        format = System.Drawing.Imaging.PixelFormat.Format32bppRgb;
        depth  = 32;
        break;
      case System.Drawing.Imaging.PixelFormat.Format16bppArgb1555:
      case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
      case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
      case System.Drawing.Imaging.PixelFormat.Format64bppArgb:
      case System.Drawing.Imaging.PixelFormat.Format64bppPArgb:
        format = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
        depth  = 32;
        break;
      case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
      default:
        format = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
        depth  = 24;
        break;
    }

    // TODO: big endian safe?
    PixelFormat pf = new PixelFormat(depth, true, false);
    if(depth >= 24)
    {
      pf.AlphaMask = format == System.Drawing.Imaging.PixelFormat.Format32bppArgb ? 0xFF000000 : 0;
      pf.RedMask   = 0xFF0000;
      pf.GreenMask = 0xFF00;
      pf.BlueMask  = 0xFF;
    }

    InitFromFormat(bitmap.Width, bitmap.Height, pf, pf.AlphaMask != 0 ? flags|SurfaceFlag.SourceAlpha : flags);
    System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                                             System.Drawing.Imaging.ImageLockMode.ReadOnly, format);
    try
    {
      Lock();
      unsafe
      {
        if(depth == 8)
        {
          System.Drawing.Imaging.ColorPalette palette = bitmap.Palette;
          Color[] colors = new Color[Math.Min(PaletteSize, palette.Entries.Length)];
          for(int i=0; i<colors.Length; i++) colors[i] = palette.Entries[i];
          SetPalette(colors);
        }

        byte* src=(byte*)data.Scan0, dest=(byte*)Data;
        if(data.Stride == Pitch)
        {
          Unsafe.Copy(src, dest, data.Stride*Height);
        }
        else
        {
          for(int i=0, len=bitmap.Width*(depth/8); i<bitmap.Height; src+=data.Stride, dest+=Pitch, i++)
          {
            Unsafe.Copy(src, dest, len);
          }
        }
      }
    }
    catch
    {
      Dispose();
      throw;
    }
    finally
    {
      bitmap.UnlockBits(data);
      if(Locked) Unlock();
    }
  }

  /// <include file="../documentation.xml" path="//Video/Surface/Cons/FromDims/*"/>
  /// <remarks>Using this is equivalent to using <see cref="Surface(int,int,int,SurfaceFlag)"/> and
  /// passing <see cref="SurfaceFlag.None"/>. The pixel format is the default format for the given bit depth (see
  /// <see cref="PixelFormat.GenerateDefaultMasks"/> for more information).
  /// </remarks>
  public Surface(int width, int height, int depth) : this(width, height, depth, SurfaceFlag.None) { }

  /// <include file="../documentation.xml" path="//Video/Surface/Cons/FromDims/*"/>
  /// <param name="flags">The <see cref="SurfaceFlag"/> flags to use when initializing this surface.</param>
  /// <remarks>The pixel format is the default format for the given bit depth (see
  /// <see cref="PixelFormat.GenerateDefaultMasks"/> for more information).
  /// </remarks>
  public Surface(int width, int height, int depth, SurfaceFlag flags)
  {
    InitFromFormat(width, height, new PixelFormat(depth, (flags&SurfaceFlag.SourceAlpha)!=0), flags);
  }

  /// <include file="../documentation.xml" path="//Video/Surface/Cons/FromFormat/*"/>
  /// <remarks>Using this is equivalent to using <see cref="Surface(int,int,PixelFormat,SurfaceFlag)"/> and
  /// passing <see cref="SurfaceFlag.None"/>.
  /// </remarks>
  [CLSCompliant(false)]
  public Surface(int width, int height, PixelFormat format) : this(width, height, format, SurfaceFlag.None) { }

  /// <include file="../documentation.xml" path="//Video/Surface/Cons/FromFormat/*"/>
  /// <param name="flags">The <see cref="SurfaceFlag"/> flags to use when initializing this surface.</param>
  [CLSCompliant(false)]
  public Surface(int width, int height, PixelFormat format, SurfaceFlag flags)
  {
    InitFromFormat(width, height, format, flags);
  }

  /// <summary>Initializes this surface by loading an image from a file.</summary>
  /// <param name="filename">The path to the image file to load.</param>
  /// <remarks>This constructor will attempt to detect the type of the image, but it is recommended that you use
  /// <see cref="Surface(string,ImageType)"/> if possible for efficiency and because the detection may not be perfect.
  /// You may also want to use <see cref="CloneDisplay"/> to convert the surface
  /// into something that matches the display surface, for efficiency.
  /// </remarks>
  public unsafe Surface(string filename)
  {
    if(string.IsNullOrEmpty(filename)) throw new ArgumentException();
    if(filename.EndsWith(".psd", StringComparison.OrdinalIgnoreCase))
    {
      InitFromSurface(PSDCodec.ReadComposite(filename));
    }
    else
    {
      InitFromSurface(Interop.SDLImage.Image.Load(filename));
    }
  }

  /// <summary>Initializes this surface by loading an image from a file.</summary>
  /// <param name="filename">The path to the image file to load.</param>
  /// <param name="type">The <see cref="ImageType"/> of the image to load.</param>
  /// <remarks>You may want to use <see cref="CloneDisplay"/> to convert the surface
  /// into something that matches the display surface, for efficiency.
  /// </remarks>
  public unsafe Surface(string filename, ImageType type)
  {
    if(string.IsNullOrEmpty(filename)) throw new ArgumentException();
    if(type==ImageType.Psd)
    {
      InitFromSurface(PSDCodec.ReadComposite(filename));
    }
    else
    {
      SDL.RWOps* ops = SDL.RWFromFile(filename, "rb");
      if(ops==null) throw new System.IO.FileNotFoundException("The file could not be opened", filename);
      InitFromSurface(Interop.SDLImage.Image.LoadTyped_RW(ops, 1, Interop.SDLImage.Image.Type.Types[(int)type]));
    }
  }

  /// <summary>Initializes the surface by loading an image from a stream.</summary>
  /// <param name="stream">The <see cref="System.IO.Stream"/> from which the image will be read.
  /// The given stream should be seekable, with its entire range dedicated to the image data.
  /// </param>
  /// <remarks>Using this is equivalent to using <see cref="Surface(System.IO.Stream,bool)"/> and passing true
  /// to automatically close the stream.
  /// This constructor will attempt to detect the type of the image, but it is recommended that you use
  /// <see cref="Surface(System.IO.Stream,ImageType)"/> if possible for efficiency and because the detection may not
  /// be perfect. You may want to use <see cref="CloneDisplay"/> to convert the surface
  /// into something that matches the display surface, for efficiency.
  /// </remarks>
  public unsafe Surface(System.IO.Stream stream) : this(stream, true) { }

  /// <summary>Initializes the surface by loading an image from a stream.</summary>
  /// <param name="stream">The <see cref="System.IO.Stream"/> from which the image will be read.
  /// The given stream should be seekable, with its entire range dedicated to the image data.
  /// </param>
  /// <param name="autoClose">If true, the stream will be closed after the image is successfully loaded.</param>
  /// <remarks>This constructor will attempt to detect the type of the image, but it is recommended that you use
  /// <see cref="Surface(System.IO.Stream,ImageType,bool)"/> if possible for efficiency and because the detection
  /// may not be perfect. (For instance, it will not detect a PSD in an unseekable stream.) You may want to use
  /// <see cref="CloneDisplay"/> to convert the surface into something that matches the display surface, for improved blitting
  /// performance.
  /// </remarks>
  public unsafe Surface(System.IO.Stream stream, bool autoClose)
  {
    if(stream == null) throw new ArgumentNullException();

    if(stream.CanSeek && PSDCodec.IsPSD(stream))
    {
      InitFromSurface(PSDCodec.ReadComposite(stream, autoClose));
    }
    else
    {
      SeekableStreamRWOps ss = new SeekableStreamRWOps(stream, autoClose);
      fixed(SDL.RWOps* ops = &ss.ops) InitFromSurface(Interop.SDLImage.Image.Load_RW(ops, 0));
    }
  }

  /// <summary>Initializes the surface by loading an image from a stream.</summary>
  /// <param name="stream">The <see cref="System.IO.Stream"/> from which the image will be read.
  /// The given stream should be seekable, with its entire range dedicated to the image data.
  /// </param>
  /// <param name="type">The <see cref="ImageType"/> of the image contained in the stream.</param>
  /// <remarks>Using this is equivalent to using <see cref="Surface(System.IO.Stream,ImageType,bool)"/> and passing
  /// true to automatically close the stream.
  /// You may want to use <see cref="CloneDisplay"/> to convert the
  /// surface into something that matches the display surface, for efficiency.
  /// </remarks>
  public unsafe Surface(System.IO.Stream stream, ImageType type) : this(stream, type, true) { }

  /// <summary>Initializes the surface by loading an image from a stream.</summary>
  /// <param name="stream">The <see cref="System.IO.Stream"/> from which the image will be read.
  /// The given stream should be seekable, with its entire range dedicated to the image data.
  /// </param>
  /// <param name="type">The <see cref="ImageType"/> of the image contained in the stream.</param>
  /// <param name="autoClose">If true, the stream will be closed after the image is successfully loaded.</param>
  /// <remarks>
  /// You may want to use <see cref="CloneDisplay"/> to convert the
  /// surface into something that matches the display surface, for efficiency.
  /// </remarks>
  public unsafe Surface(System.IO.Stream stream, ImageType type, bool autoClose)
  {
    if(type==ImageType.Psd)
    {
      InitFromSurface(PSDCodec.ReadComposite(stream, autoClose));
    }
    else
    {
      SeekableStreamRWOps ss = new SeekableStreamRWOps(stream, autoClose);
      fixed(SDL.RWOps* ops = &ss.ops)
        InitFromSurface(Interop.SDLImage.Image.LoadTyped_RW(ops, 0, Interop.SDLImage.Image.Type.Types[(int)type]));
    }
  }

  internal unsafe Surface(SDL.Surface* surface, bool autoFree)
  {
    if(surface==null) throw new ArgumentNullException("surface");
    this.surface=surface; this.autoFree=autoFree;
    Init();
  }

  /// <summary>The finalizer calls <see cref="Dispose()"/> to free the image.</summary>
  ~Surface() { Dispose(true); }

  /// <summary>Frees the resources held by the surface.</summary>
  public void Dispose()
  {
    Dispose(false);
    GC.SuppressFinalize(this);
  }

  /// <summary>Gets the width of the image.</summary>
  /// <value>The width of the image, in pixels.</value>
  public unsafe int Width
  {
    get { return surface->Width; }
  }

  /// <summary>Gets the height of the image.</summary>
  /// <value>The height of the image, in pixels.</value>
  public unsafe int Height
  {
    get { return surface->Height; }
  }

  /// <summary>Gets the color depth of the image.</summary>
  /// <value>The color depth of the image, in bits per pixel.</value>
  public int Depth
  {
    get { return format.Depth; }
  }

  /// <summary>Gets the size of the image.</summary>
  /// <value>A <see cref="Size"/> structure containing the width and height of the image.</value>
  public Size Size
  {
    get { return new Size(Width, Height); }
  }

  /// <summary>Gets the bounds of the image.</summary>
  /// <value>A <see cref="Rectangle"/> containing the bounds of the image. <see cref="Rectangle.X"/> and
  /// <see cref="Rectangle.Y"/> will be 0, and <see cref="Rectangle.Width"/> and <see cref="Rectangle.Height"/>
  /// will be the width and height of the surface.
  /// </value>
  public Rectangle Bounds
  {
    get { return new Rectangle(0, 0, Width, Height); }
  }

  /// <summary>Returns the number of bytes per row of image data.</summary>
  /// <value>The number of bytes per row of image data.</value>
  /// <remarks>This property is used when writing to the image directly. The length of a row of image data may
  /// be greater than the width times the number of bytes per pixel. This property is only valid while the image
  /// is locked (see <see cref="Lock"/> and <see cref="Locked"/>). You shouldn't alter the data between the end of
  /// the actual pixel data and the end of the row.
  /// </remarks>
  public unsafe int Pitch
  {
    get { return surface->Pitch; }
  }

  /// <summary>Gets a pointer to the raw image data.</summary>
  /// <value>A pointer to the raw image data.</value>
  /// <remarks>This property is only valid while the image is locked (see <see cref="Lock"/> and
  /// <see cref="Locked"/>).
  /// </remarks>
  [CLSCompliant(false)]
  public unsafe void* Data
  {
    get { return surface->Pixels; }
  }

  /// <summary>Gets the pixel format of the surface.</summary>
  /// <value>A <see cref="PixelFormat"/> object describing the pixel format of this surface.</value>
  [CLSCompliant(false)]
  public PixelFormat Format
  {
    get { return format; }
  }

  /// <summary>Gets the number of entries in this surface's logical palette.</summary>
  /// <value>The number of entries in this surface's logical palette.</value>
  public unsafe int PaletteSize
  {
    get
    {
      SDL.Palette* palette = format.format.Palette;
      return palette==null ? 0 : palette->Entries;
    }
  }

  /// <summary>Returns true if this surface has a logical palette.</summary>
  /// <value>A boolean indicating whether this surface has a logical palette.</value>
  public unsafe bool HasPalette
  {
    get { return format.format.Palette != null; }
  }

  /// <summary>Returns true if this surface has a physical palette.</summary>
  /// <value>A boolean indicating whether this surface has a physical palette.</value>
  public bool HasPhysicalPalette
  {
    get { return (Flags&SurfaceFlag.PhysicalPalette) != 0; }
  }

  /// <summary>Returns true if this surface is locked.</summary>
  /// <value>A boolean indicating whether this surface is locked.</value>
  public unsafe bool Locked
  {
    get { return lockCount!=0; }
  }

  /// <summary>Returns the flags for this surface.</summary>
  /// <value>The <see cref="SurfaceFlag"/> flags for this surface.</value>
  /// <remarks>This may not be the same as was passed to the constructor, as it represents the surface's current
  /// set of flags.
  /// </remarks>
  public unsafe SurfaceFlag Flags { get { return (SurfaceFlag)surface->Flags; } }

  /// <summary>Gets or sets the clipping rectangle associated with this surface.</summary>
  /// <value>A rectangle to which blits and other drawing operations will be clipped.</value>
  /// <remarks>If you create any primitive drawing code, you should honor the clipping rectangle.</remarks>
  public unsafe Rectangle ClipRect
  {
    get { return surface->ClipRect.ToRectangle(); }
    set
    {
      SDL.Rect rect = new SDL.Rect(value);
      SDL.SetClipRect(surface, ref rect);
    }
  }

  /// <summary>Gets or sets the alpha value for this surface.</summary>
  /// <value>The alpha level for the surface as a whole, from 0 (transparent) to 255 (opaque).</value>
  /// <remarks>The surface alpha is used to blit a surface that doesn't have an alpha channel using a constant
  /// alpha across the entire surface.
  /// This property will not cause blits to be alpha blended unless the surface alpha is also enabled.
  /// The <see cref="UsingAlpha"/> property can be used to enable and disable the use of alpha blending.
  /// The surface alpha will be ignored if the surface has an alpha channel. The default value is 255 (opaque).
  /// The values 0, 128, and 255 are optimized more than other values.
  /// If this property is set to 255 (opaque), <see cref="UsingAlpha"/> will automatically be set to false.
  /// </remarks>
  public byte Alpha
  {
    get { return alpha; }
    set
    {
      if(UsingAlpha) SetSurfaceAlpha(value);
      else alpha=value;
    }
  }

  /// <summary>Gets or sets the color key for this surface.</summary>
  /// <value>The color considered transparent when blitting this surface.</value>
  /// <remarks>The color will be mapped using <see cref="MapColor"/> to set the <see cref="RawColorKey"/> property.
  /// The raw color key is then used during blitting to mark source pixels as transparent. Source pixels matching
  /// the color key will not be processed.
  /// This property will not cause blits to respect the color key unless the color key is also enabled.
  /// The <see cref="UsingKey"/> property can be used to enable and disable the use of the color key. If this
  /// property is set to <see cref="Color.Empty"/>,
  /// <see cref="UsingKey"/> will automatically be set to false. The color key will be ignored if the surface has
  /// an alpha channel. In that case, use the alpha channel to mark pixels as transparent by setting the alpha value
  /// to zero (transparent).
  /// </remarks>
  public Color ColorKey
  {
    get { return key; }
    set
    {
      if(UsingKey) SetColorKey(value);
      else rawKey=MapColor(key=value);
    }
  }

  /// <summary>Gets or sets the raw color key for this surface.</summary>
  /// <value>The pixel value considered transparent when blitting this surface.</value>
  /// <remarks>The raw color key is used during blitting to mark source pixels as transparent. Source pixels matching
  /// the color key will not be processed.
  /// This property will not cause blits to respect the color key unless the color key is also enabled.
  /// The <see cref="UsingKey"/> property can be used to enable and disable the use of the color key. The color key
  /// will be ignored if the surface has an alpha channel. In that case, use the alpha channel to mark pixels as
  /// transparent by setting the alpha value to zero (transparent).
  /// </remarks>
  [CLSCompliant(false)]
  public uint RawColorKey
  {
    get { return rawKey; }
    set
    {
      if(UsingKey)
      {
        SetColorKey(value);
      }
      else
      {
        rawKey = value;
        key    = MapColor(value);
      }
    }
  }

  /// <summary>Enables/disables the use of alpha blending during blit operations.</summary>
  /// <value>A boolean indicating whether alpha blending will be used during blit operations.</value>
  /// <remarks>See <see cref="Blit(Surface,Rectangle,int,int)"/> to see how this property affects blits.</remarks>
  public unsafe bool UsingAlpha
  {
    get { return (surface->Flags&(uint)SDL.VideoFlag.SrcAlpha) != 0; }
    set
    {
      if(value)
      {
        SDL.Check(SDL.SetAlpha(surface, (uint)(SDL.VideoFlag.SrcAlpha | (UsingRLE ? SDL.VideoFlag.RLEAccel : 0)), alpha));
      }
      else
      {
        SDL.Check(SDL.SetAlpha(surface, (uint)(UsingRLE ? SDL.VideoFlag.RLEAccel : 0), 255));
      }
    }
  }

  /// <summary>Enables/disables use of the color key during blit operations.</summary>
  /// <value>A boolean indicating whether the color key will be used during blit operations.</value>
  /// <remarks>See <see cref="Blit(Surface,Rectangle,int,int)"/> to see how this property affects blits.</remarks>
  public unsafe bool UsingKey
  {
    get { return (surface->Flags&(uint)SDL.VideoFlag.SrcColorKey) != 0; }
    set
    {
      if(value)
      {
        SDL.Check(SDL.SetColorKey(surface, (uint)(SDL.VideoFlag.SrcColorKey | (UsingRLE ? SDL.VideoFlag.RLEAccel : 0)), rawKey));
      }
      else
      {
        SDL.Check(SDL.SetColorKey(surface, (uint)(UsingRLE ? SDL.VideoFlag.RLEAccel : 0), 0));
      }
    }
  }

  /// <summary>Enables/disables use of run length encoding.</summary>
  /// <value>A boolean indicating whether run length encoding is in use.</value>
  /// <remarks>Run-length encoded surfaces can be used to speed up blitting in the rare case that hardware
  /// acceleration cannot be used and the surface is stored in system memory and it contains blocks of solid
  /// color and does not change often.
  /// </remarks>
  public unsafe bool UsingRLE
  {
    get
    {
      return (surface->Flags&(uint)SDL.VideoFlag.RLEAccel) != 0;
    }
    set
    {
      if(value != UsingRLE)
      {
        if(value)
        {
          SDL.Check(SDL.SetColorKey(surface, (uint)((UsingKey ? SDL.VideoFlag.SrcColorKey : 0) | SDL.VideoFlag.RLEAccel),
                                    rawKey));
        }
        else
        {
          SDL.Check(SDL.SetColorKey(surface, UsingKey ? (uint)SDL.VideoFlag.SrcColorKey : 0, rawKey));
        }
      }
    }
  }

  /// <summary>Determins whether this surface was created with a given surface flag.</summary>
  /// <param name="flag">A <see cref="SurfaceFlag"/> to test for.</param>
  /// <returns>True if the surface was created with the given flag and false otherwise.</returns>
  public bool HasFlag(SurfaceFlag flag) { return (Flags&flag)!=0; }

  /// <summary>Determines whether this surface is compatible with the display surface (meaning that no conversion
  /// would be required between them.
  /// </summary>
  /// <param name="ignoreAlphaChannel">If true, this surface will not be considered to be incompatible with the display
  /// surface if the only difference is that this surface has an alpha channel and the display surface doesn't.
  /// If false, any difference will result in this method returning false.
  /// </param>
  /// <returns>True if the surface is compatible with the display surface, and false if not.</returns>
  /// <remarks>This method uses <see cref="PixelFormat.IsCompatible"/> to do the comparison. See that method for more
  /// details about how the check is performed.
  /// </remarks>
  public bool IsCompatible(bool ignoreAlphaChannel)
  {
    return Format.IsCompatible(Video.DisplayFormat, ignoreAlphaChannel);
  }

  /// <summary>Fills the surface with black.</summary>
  /// <remarks>This method should not be called while the surface is locked.
  /// This method respects the <see cref="ClipRect"/> set on the surface.
  /// </remarks>
  public void Fill() { Fill(Bounds, MapColor(Color.Black)); }

  /// <include file="../documentation.xml" path="//Video/Surface/Fill/*[self::Rect]/*"/>
  public void Fill(Rectangle rect) { Fill(rect, MapColor(Color.Black)); }

  /// <include file="../documentation.xml" path="//Video/Surface/Fill/*[self::Whole or self::C]/*"/>
  public void Fill(Color color) { Fill(Bounds, MapColor(color)); }

  /// <include file="../documentation.xml" path="//Video/Surface/Fill/*[self::Whole or self::R]/*"/>
  [CLSCompliant(false)]
  public void Fill(uint color) { Fill(Bounds, color); }

  /// <include file="../documentation.xml" path="//Video/Surface/Fill/*[self::Rect or self::C]/*"/>
  public void Fill(Rectangle rect, Color color) { Fill(rect, MapColor(color)); }

  /// <include file="../documentation.xml" path="//Video/Surface/Fill/*[self::Rect or self::R]/*"/>
  [CLSCompliant(false)]
  public unsafe void Fill(Rectangle rect, uint color)
  {
    SDL.Rect drect = new SDL.Rect(rect);
    SDL.Check(SDL.FillRect(surface, ref drect, color));
  }

  /// <summary>Blends this surface onto a destination surface using the given <see cref="BlendMode"/>. The surfaces must be
  /// the same size.
  /// </summary>
  /// <include file="../documentation.xml" path="//Video/Surface/Blend/*"/>
  public void Blend(Surface dest, BlendMode blendMode)
  {
    Blend(dest, Bounds, Point.Empty, blendMode);
  }

  /// <summary>Blends this surface onto a destination surface, using the given <see cref="BlendMode"/>.</summary>
  /// <include file="../documentation.xml" path="//Video/Surface/Blend/*"/>
  public void Blend(Surface dest, Point destPt, BlendMode blendMode)
  {
    Blend(dest, Bounds, destPt, blendMode);
  }

  /// <summary>Blends this surface onto a destination surface, using the given <see cref="BlendMode"/>.</summary>
  /// <include file="../documentation.xml" path="//Video/Surface/Blend/*"/>
  public void Blend(Surface dest, int destX, int destY, BlendMode blendMode)
  {
    Blend(dest, Bounds, new Point(destX, destY), blendMode);
  }

  /// <summary>Blends a portion of this surface onto a destination surface using the given <see cref="BlendMode"/>.</summary>
  /// <include file="../documentation.xml" path="//Video/Surface/Blend/*"/>
  public void Blend(Surface dest, Rectangle srcRect, int destX, int destY, BlendMode blendMode)
  {
    Blend(dest, srcRect, new Point(destX, destY), blendMode);
  }

  /// <summary>Blends a portion of this surface onto a destination surface using the given <see cref="BlendMode"/>.</summary>
  /// <include file="../documentation.xml" path="//Video/Surface/Blend/*"/>
  public void Blend(Surface dest, Rectangle srcRect, Point destPt, BlendMode blendMode)
  {
    if(dest == null) throw new ArgumentNullException();
    if(!Bounds.Contains(srcRect)) throw new ArgumentOutOfRangeException("The rectangle is out of bounds.");

    if(UsingAlpha && alpha == 0) return;

    Rectangle destRect = Rectangle.Intersect(new Rectangle(destPt, srcRect.Size), dest.ClipRect);
    if(srcRect.Size != destRect.Size) // if the destination rectangle was clipped...
    {
      if(destRect.Width == 0 || destRect.Height == 0) return;
      srcRect.Offset(destRect.X - destPt.X, destRect.Y - destPt.Y);
      srcRect.Size = destRect.Size;
    }

    int srcAdvance = GetPixelSize(), destAdvance = dest.GetPixelSize();

    Lock();
    try
    {
      dest.Lock();
      try
      {
        unsafe
        {
          byte* srcLine  = (byte*)Data + srcRect.Y*Pitch + srcRect.X*srcAdvance;
          byte* destLine = (byte*)dest.Data + destRect.Y*dest.Pitch + destRect.X*destAdvance;
          bool usingKey = UsingKey; // cache this so we don't have to check the surface flags per pixel
          bool scaleAlpha = alpha != 255 && UsingAlpha, saturateAlpha = Format.AlphaMask != 0 && !UsingAlpha;
          for(int y=srcRect.Height; y != 0; srcLine+=Pitch, destLine+=dest.Pitch, y--)
          {
            byte* srcPixel = srcLine, destPixel = destLine;
            for(int x=srcRect.Width; x != 0; srcPixel+=srcAdvance, destPixel+=destAdvance, x--)
            {
              uint rawSrc = GetPixelRaw(srcPixel);
              if(usingKey && rawSrc == rawKey) continue;

              Color srcColor = MapColor(rawSrc);
              if(scaleAlpha) srcColor = new Color(srcColor, (byte)((srcColor.Alpha*alpha+255)/256));
              else if(saturateAlpha) srcColor = new Color(srcColor, 255);

              Color destColor = dest.MapColor(dest.GetPixelRaw(destPixel));
              dest.PutPixelRaw(destPixel, dest.MapColor(Color.Blend(destColor, srcColor, blendMode)));
            }
          }
        }
      }
      finally
      {
        dest.Unlock();
      }
    }
    finally
    {
      Unlock();
    }
  }

  /// <summary>Blits this surface onto a destination surface at the top left corner.</summary>
  /// <param name="dest">The destination surface to blit onto.</param>
  /// <include file="../documentation.xml" path="//Video/Surface/Blit/Remarks/*"/>
  public void Blit(Surface dest) { Blit(dest, 0, 0); }

  /// <include file="../documentation.xml" path="//Video/Surface/Blit/*[self::Whole or self::Pt or self::Remarks]/*"/>
  public void Blit(Surface dest, Point dpt) { Blit(dest, dpt.X, dpt.Y); }

  /// <include file="../documentation.xml" path="//Video/Surface/Blit/*[self::Whole or self::XY or self::Remarks]/*"/>
  /// <remarks>This method blits the entire surface onto the destination surface, with the upper left corner of the
  /// blit beginning at the specified point. For details about how blitting works (including alpha blending), see
  /// <see cref="Blit(Surface,Rectangle,int,int)"/>.
  /// This method should not be called when either surface is locked.
  /// </remarks>
  public unsafe void Blit(Surface dest, int dx, int dy)
  {
    SDL.Rect drect = new SDL.Rect(dx, dy, Width, Height);
    SDL.Check(SDL.BlitSurface(surface, null, dest.surface, &drect));
  }
  /// <include file="../documentation.xml" path="//Video/Surface/Blit/*[self::Part or self::Pt or self::Remarks]/*"/>
  public void Blit(Surface dest, Rectangle src, Point dpt) { Blit(dest, src, dpt.X, dpt.Y); }

  /// <include file="../documentation.xml" path="//Video/Surface/Blit/*[self::Part or self::XY or self::Remarks]/*"/>
  public unsafe void Blit(Surface dest, Rectangle src, int dx, int dy)
  {
    SDL.Rect srect = new SDL.Rect(src), drect = new SDL.Rect(dx, dy, src.Width, src.Height);
    SDL.Check(SDL.BlitSurface(surface, &srect, dest.surface, &drect));
  }

  /// <include file="../documentation.xml" path="//Video/Surface/GetPixel/*[self::C or self::Pt]/*"/>
  public Color GetPixel(Point point) { return MapColor(GetPixelRaw(point.X, point.Y)); }

  /// <include file="../documentation.xml" path="//Video/Surface/GetPixel/*[self::C or self::XY]/*"/>
  public Color GetPixel(int x, int y) { return MapColor(GetPixelRaw(x, y)); }

  /// <include file="../documentation.xml" path="//Video/Surface/GetPixel/*[self::R or self::Pt]/*"/>
  [CLSCompliant(false)]
  public uint GetPixelRaw(Point point) { return GetPixelRaw(point.X, point.Y); }

  /// <include file="../documentation.xml" path="//Video/Surface/GetPixel/*[self::R or self::XY]/*"/>
  [CLSCompliant(false)]
  public uint GetPixelRaw(int x, int y)
  {
    if(!Bounds.Contains(x, y)) throw new ArgumentOutOfRangeException();
    Lock();
    try
    {
      unsafe
      {
        byte* line = (byte*)Data + y*Pitch;
        switch(Depth)
        {
          case 32: return *((uint*)line+x);
          case 24:
            byte* ptr = line+x*3;
            #if BIGENDIAN
            return (*(ushort*)ptr<<8) | *(ptr+2); // TODO: make big-endian safe
            #else
            return *(ushort*)ptr | (uint)(*(ptr+2)<<16); // TODO: make big-endian safe
            #endif
          case 16: return *((ushort*)line+x);
          case 8: return *(line+x);
          default: throw new VideoException("Unhandled depth: "+Depth.ToString());
        }
      }
    }
    finally { Unlock(); }
  }

  /// <include file="../documentation.xml" path="//Video/Surface/PutPixel/*[self::C or self::Pt]/*"/>
  public void PutPixel(Point point, Color color) { PutPixel(point.X, point.Y, MapColor(color)); }

  /// <include file="../documentation.xml" path="//Video/Surface/PutPixel/*[self::C or self::XY]/*"/>
  public void PutPixel(int x, int y, Color color) { PutPixel(x, y, MapColor(color)); }

  /// <include file="../documentation.xml" path="//Video/Surface/PutPixel/*[self::R or self::Pt]/*"/>
  [CLSCompliant(false)]
  public void PutPixel(Point point, uint color) { PutPixel(point.X, point.Y, color); }

  /// <include file="../documentation.xml" path="//Video/Surface/PutPixel/*[self::R or self::XY]/*"/>
  [CLSCompliant(false)]
  public void PutPixel(int x, int y, uint color)
  {
    if(!ClipRect.Contains(x, y)) return;
    Lock();
    try
    {
      unsafe
      {
        byte* line = (byte*)Data+y*Pitch;
        switch(Depth)
        {
          case 32: *((uint*)line+x) = color; break;
          case 24:
            byte* ptr = line+x*3;
            *(ushort*)ptr = (ushort)color; // TODO: make big-endian safe
            *(ptr+2) = (byte)(color>>16);
            break;
          case 16: *((ushort*)line+x) = (ushort)color; break;
          case 8: *(line+x) = (byte)color; break;
          default: throw new VideoException("Unhandled depth: "+Depth.ToString());
        }
      }
    }
    finally { Unlock(); }
  }


  /// <summary>This method sets the color key and enables/disables transparent blitting.</summary>
  /// <param name="color">The color to set the color key to.</param>
  /// <remarks>The color key is used during blitting to mark source pixels as transparent. Source pixels matching
  /// the color key will not be copied. This method sets the color key and then sets <see cref="UsingKey"/> to false
  /// if <paramref name="color"/> is <see cref="Color.Empty"/> and true otherwise.
  /// </remarks>
  public unsafe void SetColorKey(Color color)
  {
    key      = color;
    rawKey   = MapColor(color);
    UsingKey = color != Color.Empty; // relies on UsingKey to set the SDL key value
  }

  /// <summary>This method sets the color key and enables/disables transparent blitting.</summary>
  /// <param name="color">The raw color value to set the color key to.</param>
  /// <remarks>The raw color key is used during blitting to mark source pixels as transparent. Source pixels matching
  /// the color key will not be processed. This method sets the color key and then sets <see cref="UsingKey"/> to true.
  /// </remarks>
  [CLSCompliant(false)]
  public unsafe void SetColorKey(uint color)
  {
    rawKey   = color;
    key      = MapColor(color);
    UsingKey = true; // relies on UsingKey to set the SDL key value
  }

  /// <summary>This method sets the surface alpha and enables/disables alpha blending.</summary>
  /// <param name="alpha">The alpha level of the surface, from 0 (transparent) to 255 (opaque).</param>
  /// <remarks>The surface alpha is used to blit a surface that doesn't have an alpha channel using a constant
  /// alpha across the entire surface. This method sets the surface alpha level and then sets <see cref="UsingAlpha"/>
  /// to false if <paramref name="alpha"/> is 255 (opaque) and true otherwise.
  /// </remarks>
  public unsafe void SetSurfaceAlpha(byte alpha)
  {
    this.alpha = alpha;
    UsingAlpha = alpha!=255;
  }

  /// <summary>This method locks the surface, allowing raw access to the surface memory.</summary>
  /// <remarks>After calling this method, the <see cref="Pitch"/> and <see cref="Data"/> properties are
  /// valid until the surface is unlocked. The lock is recursive, so you can lock the surface multiple times.
  /// <see cref="Unlock"/> must be called the same number of times to finally unlock the surface.
  /// </remarks>
  public unsafe void Lock()
  {
    if(lockCount++ == 0) SDL.Check(SDL.LockSurface(surface));
  }

  /// <summary>This method unlocks the surface.</summary>
  /// <remarks><see cref="Lock"/> is recursive, so the surface can be locked multiple times. This method must be
  /// called the same number of times to finally unlock the surface.
  /// </remarks>
  public unsafe void Unlock()
  {
    if(lockCount == 0) throw new InvalidOperationException("Unlock called too many times");
    if(--lockCount == 0) SDL.UnlockSurface(surface);
  }

  /// <summary>Maps a <see cref="Color"/> to the nearest raw pixel value.</summary>
  /// <param name="color">The <see cref="Color"/> to map.</param>
  /// <returns>The raw pixel value closest to the color given.</returns>
  [CLSCompliant(false)]
  public unsafe uint MapColor(Color color)
  {
    return MapColor(color.R, color.G, color.B, color.Alpha);
  }

  /// <summary>Maps a <see cref="Color"/> to the nearest raw pixel value.</summary>
  /// <param name="color">The <see cref="Color"/> to map.</param>
  /// <param name="alpha">The alpha value to use for the color.</param>
  /// <returns>The raw pixel value closest to the color given.</returns>
  /// <remarks>The alpha value passed overrides the alpha value contained in the <see cref="Color.Alpha"/>
  /// property of <paramref name="color"/>.
  /// </remarks>
  [CLSCompliant(false)]
  public unsafe uint MapColor(Color color, byte alpha)
  {
    return MapColor(color.R, color.G, color.B, alpha);
  }

  /// <summary>Maps a color specified as RGB components to the nearest raw pixel value.</summary>
  /// <returns>The raw pixel value closest to the color given.</returns>
  [CLSCompliant(false)]
  public uint MapColor(byte r, byte g, byte b)
  {
    return MapColor(r, g, b, 255);
  }

  /// <summary>Maps a color specified as RGBA components to the nearest raw pixel value.</summary>
  /// <returns>The raw pixel value closest to the color given.</returns>
  [CLSCompliant(false)]
  public unsafe uint MapColor(byte r, byte g, byte b, byte a)
  {
    if(HasPalette)
    {
      return SDL.MapRGBA(surface->Format, r, g, b, a);
    }
    else
    {
      return ((uint)r >> format.format.Rloss << format.format.Rshift) |
             ((uint)g >> format.format.Gloss << format.format.Gshift) |
             ((uint)b >> format.format.Bloss << format.format.Bshift) |
             (((uint)a >> format.format.Aloss << format.format.Ashift) & format.AlphaMask);
    }
  }

  /// <summary>Maps a raw pixel value to the corresponding <see cref="Color"/>.</summary>
  /// <param name="color">The raw pixel value to map.</param>
  /// <returns>The <see cref="Color"/> corresponding to <paramref name="color"/>.</returns>
  [CLSCompliant(false)]
  public unsafe Color MapColor(uint color)
  {
    byte r, g, b, a;
    if(HasPalette)
    {
      SDL.GetRGBA(color, surface->Format, out r, out g, out b, out a);
    }
    else
    {
      uint v = (color & format.format.Rmask) >> format.format.Rshift;
      r = (byte)((v << format.format.Rloss) + (v >> (8 - (format.format.Rloss << 1))));
      v = (color & format.format.Gmask) >> format.format.Gshift;
      g = (byte)((v << format.format.Gloss) + (v >> (8 - (format.format.Gloss << 1))));
      v = (color & format.format.Bmask) >> format.format.Bshift;
      b = (byte)((v << format.format.Bloss) + (v >> (8 - (format.format.Bloss << 1))));

      if(format.AlphaMask == 0)
      {
        a = 255;
      }
      else
      {
        v = (color & format.AlphaMask) >> format.format.Ashift;
        a = (byte)((v << format.format.Aloss) + (v >> (8 - (format.format.Aloss << 1))));
      }
    }

    return new Color(r, g, b, a);
  }

  /// <summary>Returns the logical color palette.</summary>
  /// <returns>An array of <see cref="Color"/> containing the logical color palette.</returns>
  /// <exception cref="VideoException">Thrown if the surface has no associated palette.</exception>
  public Color[] GetPalette()
  {
    Color[] colors = new Color[PaletteSize];
    GetPalette(colors);
    return colors;
  }
  /// <summary>Gets the logical palette colors.</summary>
  /// <param name="colors">An array of <see cref="Color"/> which will be filled with color data from the palette.
  /// </param>
  /// <remarks>This will not fill the entire array if it's longer than the number of entries in the palette.</remarks>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="colors"/> is null.</exception>
  public void GetPalette(Color[] colors)
  {
    if(colors==null) throw new ArgumentNullException("colors");
    GetPalette(colors, 0, 0, Math.Min(PaletteSize, colors.Length));
  }
  /// <summary>Gets the logical palette colors.</summary>
  /// <param name="colors">An array of <see cref="Color"/> which will be filled with color data from the palette.
  /// </param>
  /// <param name="numColors">The number of colors to copy from the palette.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="numColors"/> is invalid or would
  /// overflow the palette.
  /// </exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="colors"/> is null.</exception>
  public void GetPalette(Color[] colors, int numColors) { GetPalette(colors, 0, 0, numColors); }
  public unsafe void GetPalette(Color[] colors, int startIndex, int startColor, int numColors)
  {
    Utility.ValidateRange(colors, startIndex, numColors);
    ValidatePaletteArgs(startColor, numColors);

    Color* palette = format.format.Palette->Colors + startColor;
    // it seems that SDL returns palette colors with alpha == 0, so change the alpha to 255
    for(int i=0; i<numColors; i++) colors[startIndex+i] = new Color(palette[i], 255);
  }

  /// <include file="../documentation.xml" path="//Video/Surface/SetPalette/*[self::Logical or self::A or self::EN]/*"/>
  public bool SetPalette(Color[] colors)
  {
    if(colors==null) throw new ArgumentNullException("colors");
    return SetPalette(colors, 0, 0, colors.Length, PaletteType.Logical);
  }
  /// <include file="../documentation.xml" path="//Video/Surface/SetPalette/*[self::Logical or self::AN or self::EN]/*"/>
  public bool SetPalette(Color[] colors, int numColors)
  {
    return SetPalette(colors, 0, 0, numColors, PaletteType.Logical);
  }
  /// <include file="../documentation.xml" path="//Video/Surface/SetPalette/*[self::Logical or self::AA]/*"/>
  public bool SetPalette(Color[] colors, int startIndex, int startColor, int numColors)
  {
    return SetPalette(colors, startIndex, startColor, numColors, PaletteType.Logical);
  }
  /// <summary>Sets palette colors.</summary>
  /// <param name="type">The palette to change (<see cref="PaletteType"/>).</param>
  /// <include file="../documentation.xml" path="//Video/Surface/SetPalette/*[self::Common or self::AA]/*"/>
  public unsafe bool SetPalette(Color[] colors, int startIndex, int startColor, int numColors, PaletteType type)
  {
    Utility.ValidateRange(colors, startIndex, numColors);
    ValidatePaletteArgs(startColor, numColors);
    fixed(Color* colorPtr=colors) return SDL.SetPalette(surface, (uint)type, colorPtr+startIndex, startColor, numColors) == 1;
  }

  /// <include file="../documentation.xml" path="//Video/Surface/CreateCompatible/*"/>
  /// <remarks>This is equivalent to calling <see cref="CreateCompatible(int,int,SurfaceFlag)"/> and passing
  /// the value of the <see cref="Flags"/> property.
  /// </remarks>
  public Surface CreateCompatible(int width, int height) { return CreateCompatible(width, height, Flags); }
  /// <include file="../documentation.xml" path="//Video/Surface/CreateCompatible/*"/>
  /// <param name="flags">The surface flags for the new surface.</param>
  public unsafe Surface CreateCompatible(int width, int height, SurfaceFlag flags)
  { SDL.Surface* ret = SDL.CreateRGBSurface((uint)flags, width, height, format.Depth, format.RedMask,
                                            format.GreenMask, format.BlueMask, format.AlphaMask);
    if(ret==null) SDL.RaiseError();
    return new Surface(ret, true);
  }

  /// <summary>Returns a clone of this surface.</summary>
  /// <returns>A new <see cref="Surface"/> containing the same image data.</returns>
  public Surface Clone() { return Clone(Format, Flags); }
  /// <summary>Returns a clone of this surface.</summary>
  /// <param name="format">The pixel format of the new surface.</param>
  /// <returns>A new <see cref="Surface"/> containing the same image data, converted to the given pixel format.
  /// </returns>
  [CLSCompliant(false)]
  public Surface Clone(PixelFormat format) { return Clone(format, Flags); }
  /// <summary>Returns a clone of this surface.</summary>
  /// <param name="flags">The surface flags for the new surface.</param>
  /// <returns>A new <see cref="Surface"/> containing the same image data.</returns>
  public Surface Clone(SurfaceFlag flags) { return Clone(Format, flags); }
  /// <summary>Returns a clone of this surface.</summary>
  /// <param name="format">The pixel format of the new surface.</param>
  /// <param name="flags">The surface flags for the new surface.</param>
  /// <returns>A new <see cref="Surface"/> containing the same image data, converted to the given pixel format.
  /// </returns>
  [CLSCompliant(false)]
  public unsafe Surface Clone(PixelFormat format, SurfaceFlag flags)
  { SDL.Surface* ret;
    fixed(SDL.PixelFormat* pf = &format.format) ret = SDL.ConvertSurface(surface, pf, (uint)flags);
    if(ret==null) SDL.RaiseError();
    return new Surface(ret, true);
  }

  /// <include file="../documentation.xml" path="//Video/Surface/CloneDisplay/*"/>
  public unsafe Surface CloneDisplay() { return CloneDisplay(Format.AlphaMask!=0); }
  /// <include file="../documentation.xml" path="//Video/Surface/CloneDisplay/*"/>
  /// <param name="alphaChannel">A boolean which determines whether the new surface should have an alpha channel.</param>
  public unsafe Surface CloneDisplay(bool alphaChannel)
  { SDL.Surface* ret = alphaChannel ? SDL.DisplayFormatAlpha(surface) : SDL.DisplayFormat(surface);
    if(ret==null) SDL.RaiseError();
    return new Surface(ret, true);
  }

  /// <summary>Converts the surface into a <see cref="Bitmap"/>.</summary>
  /// <returns>A new <see cref="Bitmap"/> containing the image data from this surface.</returns>
  public Bitmap ToBitmap() { return ToBitmap(false); }

  /// <summary>Saves this image to a file using the given image format.</summary>
  /// <param name="filename">The path to the file into which the image will be saved.</param>
  /// <param name="type">The <see cref="ImageType"/> of the destination image.</param>
  public void Save(string filename, ImageType type)
  { Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
    try { Save(stream, type); }
    finally { stream.Close(); }
  }

  /// <summary>Writes this image to a stream using the given image format.</summary>
  /// <param name="stream">The stream into which the image will be written. The given stream should be seekable,
  /// with its entire range dedicated to the image data.
  /// </param>
  /// <param name="type">The <see cref="ImageType"/> of the destination image.</param>
  /// <remarks></remarks>
  public void Save(Stream stream, ImageType type)
  { Bitmap bitmap=null;
    switch(type)
    { case ImageType.Pcx: WritePCX(stream); break;
      case ImageType.Psd: PSDCodec.WritePSD(this, stream); break;
      case ImageType.Bmp: (bitmap=ToBitmap(true)).Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);  break;
      case ImageType.Gif: (bitmap=ToBitmap(true)).Save(stream, System.Drawing.Imaging.ImageFormat.Gif);  break;
      case ImageType.Jpeg: (bitmap=ToBitmap(true)).Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg); break;
      case ImageType.Png: (bitmap=ToBitmap(true)).Save(stream, System.Drawing.Imaging.ImageFormat.Png);  break;
      case ImageType.Tiff: (bitmap=ToBitmap(true)).Save(stream, System.Drawing.Imaging.ImageFormat.Tiff); break;
      default: throw new NotImplementedException();
    }
    Utility.Dispose(bitmap);
  }

  /// <summary>Saves this image to a file using the given encoder.</summary>
  /// <param name="filename">The path to the file into which the image will be saved.</param>
  /// <param name="encoder">The <see cref="System.Drawing.Imaging.ImageCodecInfo">encoder</see> that will encode
  /// the image data.
  /// </param>
  /// <param name="parms"><see cref="System.Drawing.Imaging.EncoderParameters">paramaters</see> for the encoder.
  /// </param>
  public void Save(string filename, System.Drawing.Imaging.ImageCodecInfo encoder,
                   System.Drawing.Imaging.EncoderParameters parms)
  { Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
    try { Save(stream, encoder, parms); }
    finally { stream.Close(); }
  }

  /// <summary>Writes this image to a stream using the given encoder.</summary>
  /// <param name="stream">The stream into which the image will be written. The given stream should be seekable,
  /// with its entire range dedicated to the image data.
  /// </param>
  /// <param name="encoder">The <see cref="System.Drawing.Imaging.ImageCodecInfo">encoder</see> that will encode
  /// the image data.
  /// </param>
  /// <param name="parms"><see cref="System.Drawing.Imaging.EncoderParameters">paramaters</see> for the encoder.
  /// </param>
  public void Save(Stream stream, System.Drawing.Imaging.ImageCodecInfo encoder,
                   System.Drawing.Imaging.EncoderParameters parms)
  { Bitmap bitmap = ToBitmap(true);
    bitmap.Save(stream, encoder, parms);
    bitmap.Dispose();
  }

  /// <summary>Writes the image as a JPEG using the given quality level.</summary>
  /// <param name="filename">The path to the file into which the image will be saved.</param>
  /// <param name="quality">The image quality of the compression, from 0 (worst) to 100 (best).</param>
  public void SaveJpeg(string filename, int quality)
  { Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
    try { SaveJpeg(stream, quality); }
    finally { stream.Close(); }
  }

  /// <summary>Writes this image to a stream as a JPEG using the given quality level.</summary>
  /// <param name="stream">The stream into which the image will be written. The given stream should be seekable,
  /// with its entire range dedicated to the image data.
  /// </param>
  /// <param name="quality">The image quality of the compression, from 0 (worst) to 100 (best).</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if quality is less than 0 or greater than 100.</exception>
  public void SaveJpeg(Stream stream, int quality)
  { if(quality<0 || quality>100) throw new ArgumentOutOfRangeException("quality", quality, "must be from 0-100");
    foreach(System.Drawing.Imaging.ImageCodecInfo codec in System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders())
      if(codec.MimeType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
      { System.Drawing.Imaging.EncoderParameters parms = new System.Drawing.Imaging.EncoderParameters(1);
        parms.Param[0] =
          new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);
        Bitmap bitmap = ToBitmap(true);
        bitmap.Save(stream, codec, parms);
        bitmap.Dispose();
        return;
      }
    throw new CodecNotFoundException("JPEG");
  }

  /// <summary>Given a surface, returns a possibly-new surface that is compatible with the display. If the surface is
  /// already compatible, it is returned unchanged. Otherwise, the original surface is disposed and a new surface is
  /// returned.
  /// </summary>
  /// <param name="surface">The surface to check.</param>
  /// <remarks>If the surface has an alpha channel, the returned surface will also have an alpha channel. This may
  /// cause the surface to be incompatible with the display on the basis of the alpha channel
  /// </remarks>
  public static Surface MakeCompatibleWithDisplay(Surface surface)
  {
    return MakeCompatibleWithDisplay(surface, true, true);
  }

  /// <summary>Given a surface, returns a possibly-new surface that is compatible with the display. If the surface is
  /// already compatible, it is returned unchanged. Otherwise, the original surface is disposed and a new surface is
  /// returned.
  /// </summary>
  /// <param name="surface">The surface to check.</param>
  /// <param name="useAlphaChannel">If true and the surface has an alpha channel, the returned surface will also have
  /// an alpha channel. If false or the surface has no alpha channel, the returned surface will not have an alpha
  /// channel.
  /// </param>
  public static Surface MakeCompatibleWithDisplay(Surface surface, bool useAlphaChannel)
  {
    return MakeCompatibleWithDisplay(surface, useAlphaChannel, true);
  }

  /// <summary>Given a surface, returns a possibly-new surface that is compatible with the display. If the surface is
  /// already compatible, it is returned unchanged.
  /// </summary>
  /// <param name="surface">The surface to check.</param>
  /// <param name="useAlphaChannel">If true and the surface has an alpha channel, the returned surface will also have
  /// an alpha channel. If false or the surface has no alpha channel, the returned surface will not have an alpha
  /// channel.
  /// </param>
  /// <param name="disposeOriginal">If true and the surface needs to be converted, the original surface will be
  /// disposed.
  /// </param>
  public static Surface MakeCompatibleWithDisplay(Surface surface, bool useAlphaChannel, bool disposeOriginal)
  {
    if(surface == null) throw new ArgumentNullException();

    if(!surface.IsCompatible(useAlphaChannel))
    {
      Surface newSurface = surface.CloneDisplay();
      if(disposeOriginal) surface.Dispose();
      surface = newSurface;
    }
    return surface;
  }

  internal unsafe void InitFromSurface(Surface surface)
  {
    InitFromSurface(surface.surface);
    surface.surface=null;
    surface.Dispose();
  }

  internal unsafe void InitFromSurface(SDL.Surface* surface)
  {
    if(surface==null) SDL.RaiseError();
    this.surface=surface;
    autoFree = true;
    Init();
  }

  unsafe uint GetPixelRaw(byte* pixel)
  {
    switch(Depth)
    {
      case 32: return *(uint*)pixel;
      case 24: return *(ushort*)pixel | (uint)(*(pixel+2)<<16); // TODO: make big-endian safe
      case 16: return *(ushort*)pixel;
      case 8: return *pixel;
      default: throw new VideoException("Unhandled depth: "+Depth.ToString());
    }
  }

  unsafe void PutPixelRaw(byte* pixel, uint color)
  {
    switch(Depth)
    {
      case 32: *(uint*)pixel = color; break;
      case 24:
        *(ushort*)pixel = (ushort)color; // TODO: make big-endian safe
        *(pixel+2)      = (byte)(color>>16);
        break;
      case 16: *(ushort*)pixel = (ushort)color; break;
      case 8: *pixel = (byte)color; break;
      default: throw new VideoException("Unhandled depth: "+Depth.ToString());
    }
  }

  int GetPixelSize()
  {
    if((Depth&7) != 0) throw new VideoException("Unhandled depth: "+Depth.ToString());
    return Depth/8;
  }

  unsafe void Init()
  {
    format = new PixelFormat(surface->Format);
    rawKey = format.format.Key;
    key    = MapColor(rawKey);
  }

  unsafe void InitFromFormat(int width, int height, PixelFormat format, SurfaceFlag flags)
  {
    InitFromSurface(SDL.CreateRGBSurface((uint)flags, width, height, format.Depth, format.RedMask,
                                         format.GreenMask, format.BlueMask, format.AlphaMask));
  }

  Bitmap ToBitmap(bool forSaving)
  {
    System.Drawing.Imaging.PixelFormat format;
    switch(Depth) // TODO: support 555 packing (15-bit color)
    {
      case 8: format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed; break;
      case 16: format = System.Drawing.Imaging.PixelFormat.Format16bppRgb565; break;
      case 24: format = System.Drawing.Imaging.PixelFormat.Format24bppRgb; break;
      case 32:
        format = Format.AlphaMask==0 ? System.Drawing.Imaging.PixelFormat.Format32bppRgb :
                                       System.Drawing.Imaging.PixelFormat.Format32bppArgb;
        break;
      default: throw new NotSupportedException("Unhandled depth in ToBitmap()");
    }

    Bitmap bitmap;
    // TODO: big endian safe?
    unsafe
    {
      Lock();
      try
      {
        if(Depth >= 24 && Format.RedMask == 0xFF && Format.GreenMask == 0xFF00 && Format.BlueMask == 0xFF0000 &&
           (Format.AlphaMask == 0 || Format.AlphaMask == 0xFF000000))
        {
          byte* src  = (byte*)Data;
          int xinc = Depth/8, len = Width*Height;
          byte[] arr = new byte[len*xinc];

          fixed(byte* arrp = arr)
          {
            byte* dest=arrp;
            byte v;
            if(Pitch==Width*xinc)
            {
              Unsafe.Copy(src, dest, arr.Length);
              while(len-- != 0) { v=*dest; *dest=dest[2]; dest[2]=v; dest+=xinc; }
            }
            else
            {
              for(int y=0, line=Width*xinc, yinc=Pitch-line; y<Height; src+=Pitch, dest+=line, y++)
              {
                Unsafe.Copy(src, dest, line);
                byte* dp = dest;
                int xlen = Width;
                while(xlen-- != 0) { v=*dp; *dp=dp[2]; dp[2]=v; dp+=xinc; }
              }
            }
            bitmap = BitmapFromData(Width, Height, Width*xinc, format, arrp);
          }
        }
        else if(Depth >= 24 && (Format.AlphaMask != 0 && Format.AlphaMask != 0xFF000000 ||
                                Format.RedMask != 0xFF0000 || Format.GreenMask != 0xFF00 || Format.BlueMask != 0xFF) ||
                Depth == 16 && (Format.RedMask != 0xF800 || Format.GreenMask != 0x07E0 || Format.BlueMask != 0x001F))
        {
          PixelFormat pixelFormat = new PixelFormat();
          pixelFormat.Depth = Depth;
          if(Depth >= 24)
          {
            pixelFormat.AlphaMask = Depth == 32 && pixelFormat.AlphaMask != 0 ? 0xFF000000 : 0;
            pixelFormat.RedMask   = 0xFF0000;
            pixelFormat.GreenMask = 0xFF00;
            pixelFormat.BlueMask  = 0xFF;
          }
          else if(Depth == 16)
          {
            pixelFormat.GenerateDefaultMasks(false, false);
          }

          using(Surface converted = Clone(pixelFormat)) return converted.ToBitmap();
        }
        else
        {
          bitmap = forSaving ? new Bitmap(Width, Height, (int)Pitch, format, new IntPtr(Data))
                             : BitmapFromData(Width, Height, Width*Depth/8, (int)Pitch, format, Data);
        }
      }
      finally { Unlock(); }
    }

    if(Depth==8)
    {
      System.Drawing.Imaging.ColorPalette pal = bitmap.Palette;

      Color[] colors = new Color[Math.Min(PaletteSize, pal.Entries.Length)];
      GetPalette(colors);
      for(int i=0; i<colors.Length; i++) pal.Entries[i] = colors[i];

      bitmap.Palette = pal;
    }
    return bitmap;
  }

  unsafe void ValidatePaletteArgs(int startColor, int numColors)
  {
    SDL.Palette* palette = format.format.Palette;
    if(palette==null) throw new VideoException("This surface does not have an associated palette.");
    int max = palette->Entries;
    if(startColor<0 || numColors>max || startColor+numColors>max) throw new ArgumentOutOfRangeException();
  }

  unsafe void WritePCX(Stream stream)
  {
    byte[] pbuf = new byte[768];

    using(AdamMil.IO.BinaryWriter sw = new AdamMil.IO.BinaryWriter(stream))
    {
      sw.Write((byte)10);           // manufacturer. 10 = z-soft
      sw.Write((byte)5);            // version
      sw.Write((byte)1);            // encoding. 1 = RLE
      sw.Write((byte)8);            // bits per pixel (where pixel is a single item on a plane)
      sw.Write((short)0);           // xmin
      sw.Write((short)0);           // ymin
      sw.Write((short)(Width-1));   // xmax
      sw.Write((short)(Height-1));  // ymax
      sw.Write((short)72);          // DPI
      sw.Write((short)72);          // DPI
      sw.Write(pbuf, 0, 48);        // color map (filler)
      sw.Write((byte)0);            // reserved field

      int bpp = Width, bpl;
      if((bpp&1)!=0) bpp++;
      if(Depth==8)
      { bpl = bpp;
        sw.Write((byte)1);    // planes
      }
      else
      { bpl = bpp*3;
        sw.Write((byte)3);    // planes
      }
      sw.Write((short)bpp);   // bytes per plane line
      sw.Write((short)1);     // palette type. 1 = color
      sw.Write(pbuf, 0, 58);  // filler

      Lock();
      try
      {
        byte* mem = (byte*)Data;
        int x, y, width=Width;
        if(Depth == 8)
        {
          byte c, count;
          for(y=0; y<Height; y++)
          {
            for(x=0; x<width; )
            {
              count=0; c=mem[x];
              do
              {
                count++; x++;
                if(count==63 || x==width) break;
              } while(mem[x]==c);
              if(c>=192 || count>1) sw.Write((byte)(192+count)); // encode as a run
              sw.Write(c);
            }
            if((Width&1)!=0) sw.Write((byte)0);
            mem += Pitch;
          }

          sw.Write((byte)12); // byte 12 precedes palette
          Color[] pal = new Color[256];
          GetPalette(pal, PaletteSize);
          for(int i=0, j=0; i<256; i++)
          {
            pbuf[j++] = pal[i].R;
            pbuf[j++] = pal[i].G;
            pbuf[j++] = pal[i].B;
          }
          sw.Write(pbuf, 0, 768); // palette
        }
        else // Depth > 8
        {
          byte[] linebuf = new byte[bpl];
          fixed(byte* line = linebuf)
          {
            byte* omem = mem;
            Color c;
            int i;
            byte p, count;
            for(y=0; y<Height; y++)
            {
              if(Depth==16)
                for(x=0; x<width; mem+=2, x++)
                {
                  c = MapColor((uint)*(ushort*)mem);
                  line[x]=c.R; line[x+bpp]=c.G; line[x+bpp*2]=c.B;
                }
              else if(Depth==24)
                for(x=0; x<width; mem+=3, x++)
                {
                  c = MapColor((uint)(mem[0]+(mem[1]<<8)+(mem[2]<<16))); // TODO: big-endian safe?
                  line[x]=c.R; line[x+bpp]=c.G; line[x+bpp*2]=c.B;
                }
              else if(Depth==32)
                for(x=0; x<width; mem+=4, x++)
                {
                  c = MapColor(*(uint*)mem);
                  line[x]=c.R; line[x+bpp]=c.G; line[x+bpp*2]=c.B;
                }
              omem += Pitch; mem = omem;

              for(i=0; i<bpl; )
              {
                count=0; p=line[i];
                do
                {
                  count++; i++;
                  if(count==63 || i==bpl) break;
                } while(line[i]==p);
                if(p>=192 || count>1) sw.Write((byte)(192+count)); // encode as a run
                sw.Write(p);
              }
            }
          }
        }
      }
      finally { Unlock(); }
    }
  }

  unsafe void Dispose(bool finalizing)
  {
    if(autoFree && surface != null)
    {
      SDL.FreeSurface(surface);
      surface = null;
    }
  }

  void IGuiImage.Draw(Rectangle srcRect, IGuiRenderTarget target, Rectangle destRect)
  {
    if(target == null) throw new ArgumentNullException();
    if(srcRect.X < 0 || srcRect.Y < 0 || srcRect.Right > Width || srcRect.Bottom > Height)
    {
      throw new ArgumentOutOfRangeException("srcRect");
    }

    Surface targetSurface = target as Surface;
    if(targetSurface == null) throw new NotSupportedException("Surface images only support surface render targets.");

    if(srcRect.Size == destRect.Size)
    {
      Blit(targetSurface, srcRect, destRect.X, destRect.Y);
    }
    else
    {
      // this is fairly complicated...
      throw new NotImplementedException();
    }
  }

  void IGuiRenderTarget.Clear()
  {
    Fill(Depth <= 8 && UsingKey ? RawColorKey : 0);
  }

  int IGuiRenderTarget.DrawText(GameLib.Fonts.Font font, string text, Point pt)
  {
    if(font == null || text == null) throw new ArgumentNullException();

    GameLib.Fonts.SurfaceFont surfaceFont = font as GameLib.Fonts.SurfaceFont;
    if(surfaceFont == null)
    {
      throw new NotSupportedException("Surface render targets only support fonts based on SurfaceFont.");
    }

    return surfaceFont.Render(this, text, pt);
  }

  Point IGuiRenderTarget.DrawText(GameLib.Fonts.Font font, string text, Rectangle rect, ContentAlignment align)
  {
    if(font == null || text == null) throw new ArgumentNullException();

    GameLib.Fonts.SurfaceFont surfaceFont = font as GameLib.Fonts.SurfaceFont;
    if(surfaceFont == null)
    {
      throw new NotSupportedException("Surface render targets only support fonts based on SurfaceFont.");
    }

    return surfaceFont.Render(this, text, rect, align);
  }

  void IGuiRenderTarget.FillArea(Rectangle area, Color color)
  {
    if(color.Alpha == 255) Fill(area, color);
    else if(color.Alpha != 0) Shapes.FilledBox(this, area, color);
  }

  PixelFormat format;
  Color key;
  uint  lockCount, rawKey;
  byte  alpha=255;

  internal unsafe SDL.Surface* surface;
  bool autoFree;

  static unsafe Bitmap BitmapFromData(int width, int height, int stride, System.Drawing.Imaging.PixelFormat format,
                               void* data)
  {
    return BitmapFromData(width, height, stride, stride, format, data);
  }

  static unsafe Bitmap BitmapFromData(int width, int height, int row, int stride,
                                      System.Drawing.Imaging.PixelFormat format, void* data)
  {
    Bitmap bmp = new Bitmap(width, height, format);
    System.Drawing.Imaging.BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height),
                                                        System.Drawing.Imaging.ImageLockMode.WriteOnly, format);
    try
    {
      if(bd.Stride<0) throw new NotImplementedException("Can't handle bottom-up images");
      byte* src=(byte*)data, dest=(byte*)bd.Scan0.ToPointer();
      if(bd.Stride==stride) Unsafe.Copy(src, dest, height*stride);
      else for(; height!=0; src += stride, dest += bd.Stride, height--) Unsafe.Copy(src, dest, row);
    }
    finally { bmp.UnlockBits(bd); }
    return bmp;
  }

}

} // namespace GameLib.Video