/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2007 Adam Milazzo

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
using GameLib.Interop.SDL;

namespace GameLib.Video
{

#region Supporting Types
/// <summary>This enum determines which palette to set when setting the palette.</summary>
/// <remarks>Paletted surfaces always have a logical palette, which determines how pixel values map to colors.
/// If the display surface was created with the <see cref="SurfaceFlag.PhysicalPalette"/> flag, you'll also have
/// access to the physical palette, which determines how the hardware will map colors to the display. The logical
/// palette is always used when converting image data from one surface to another.
/// </remarks>
[Flags]
public enum PaletteType
{ 
  /// <summary>The logical palette will be set.</summary>
  Logical=SDL.PaletteType.Logical,
  /// <summary>The physical palette will be set.</summary>
  Physical=SDL.PaletteType.Physical,
  /// <summary>The both palettes will be set.</summary>
  Both=Logical|Physical
};

/// <summary>This class holds certain OpenGL options. While most OpenGL attributes can be set normally through
/// the OpenGL API, the options contained in this class must be set when the video mode is set.
/// </summary>
[CLSCompliant(false)]
public class GLOptions
{ 
  /// <summary>The size of the frame buffer's red channel, in bits.</summary>
  /// <remarks>This field defaults to -1, which indicates that this OpenGL property should not be altered.</remarks>
  public sbyte Red=-1;
  /// <summary>The size of the frame buffer's green channel, in bits.</summary>
  /// <remarks>This field defaults to -1, which indicates that this OpenGL property should not be altered.</remarks>
  public sbyte Green=-1;
  /// <summary>The size of the frame buffer's blue channel, in bits.</summary>
  /// <remarks>This field defaults to -1, which indicates that this OpenGL property should not be altered.</remarks>
  public sbyte Blue=-1;
  /// <summary>The size of the frame buffer's alpha channel, in bits.</summary>
  /// <remarks>This field defaults to -1, which indicates that this OpenGL property should not be altered.</remarks>
  public sbyte Alpha=-1;

  /// <summary>The size of the accumulation buffer's red channel, in bits.</summary>
  /// <remarks>This field defaults to -1, which indicates that this OpenGL property should not be altered.</remarks>
  public sbyte AccumRed=-1;
  /// <summary>The size of the accumulation buffer's green channel, in bits.</summary>
  /// <remarks>This field defaults to -1, which indicates that this OpenGL property should not be altered.</remarks>
  public sbyte AccumGreen=-1;
  /// <summary>The size of the accumulation buffer's blue channel, in bits.</summary>
  /// <remarks>This field defaults to -1, which indicates that this OpenGL property should not be altered.</remarks>
  public sbyte AccumBlue=-1;
  /// <summary>The size of the accumulation buffer's alpha channel, in bits.</summary>
  /// <remarks>This field defaults to -1, which indicates that this OpenGL property should not be altered.</remarks>
  public sbyte AccumAlpha=-1;

  /// <summary>A value indicating whether double buffering is enabled.</summary>
  /// <remarks>This field can be set to 0 to disable double buffering, or 1 to enable it. The field defaults to -1,
  /// which indicates that this OpenGL property should not be altered.
  /// </remarks>
  public sbyte DoubleBuffer=-1;

  /// <summary>The depth of the frame buffer, in bits per pixel.</summary>
  /// <remarks>This field defaults to -1, which indicates that this OpenGL property should not be altered.</remarks>
  public sbyte Frame=-1;
  /// <summary>The depth of the depth buffer, in bits per pixel.</summary>
  /// <remarks>This field defaults to -1, which indicates that this OpenGL property should not be altered.</remarks>
  public sbyte Depth=-1;
  /// <summary>The depth of the stencil buffer, in bits per pixel.</summary>
  /// <remarks>This field defaults to -1, which indicates that this OpenGL property should not be altered.</remarks>
  public sbyte Stencil=-1;
}

/// <summary>This delegate is used along with <see cref="Video.ModeChanged"/> to notify the application when the
/// video mode is set or changed.
/// </summary>
public delegate void ModeChangedHandler();
#endregion

#region PixelFormat
/// <summary>This class represents the format of the pixels in memory for a given surface.</summary>
[CLSCompliant(false)]
public sealed class PixelFormat
{
  /// <summary>This constructor creates an empty PixelFormat class. The contents are not valid and must be
  /// filled in manually before it's used.
  /// </summary>
  public PixelFormat() { }

  /// <summary>This constructor initializes the PixelFormat class with default values for the given bit depth.
  /// </summary>
  /// <param name="depth">The color depth of the surface in bits per pixel.</param>
  /// <remarks>Calling this is equivalent to calling <see cref="PixelFormat(int, bool)"/> and passing false,
  /// creating a surface without an alpha channel. See <see cref="GenerateDefaultMasks(bool)"/> for more
  /// information on how the pixel format will be initialized.
  /// </remarks>
  public PixelFormat(int depth) : this(depth, false) { }

  /// <summary>This constructor initializes the PixelFormat class with default values for the given bit depth.
  /// </summary>
  /// <param name="depth">The color depth of the surface in bits per pixel.</param>
  /// <param name="withAlpha">If true, an alpha channel will be specified. Currently this only has an effect
  /// when <paramref name="depth"/> is 32.
  /// </param>
  /// <remarks>See <see cref="GenerateDefaultMasks(bool)"/> for more information on how the pixel format will be
  /// initialized.
  /// </remarks>
  public PixelFormat(int depth, bool withAlpha) { Depth=depth; GenerateDefaultMasks(withAlpha); }
  internal unsafe PixelFormat(SDL.PixelFormat* pf) { format = *pf; }

  /// <summary>Gets or sets the color depth of the pixel format in bits per pixel.</summary>
  /// <value>The color depth of the pixel format in bits per pixel.</value>
  public int Depth
  { get { return format.BitsPerPixel; }
    set
    { format.BitsPerPixel=(byte)value; format.BytesPerPixel=(byte)((value+7)/8);
    }
  }

  /// <summary>Gets or sets the bit mask for the red channel.</summary>
  /// <value>The bit mask for the red channel.</value>
  /// <remarks>The bit mask for the red channel is the mask which, if ANDed with a pixel value, would zero
  /// out all the bits except the ones corresponding to the red channel.
  /// </remarks>
  public uint RedMask { get { return format.Rmask; } set { format.Rmask=value; } }

  /// <summary>Gets or sets the bit mask for the green channel.</summary>
  /// <value>The bit mask for the green channel.</value>
  /// <remarks>The bit mask for the green channel is the mask which, if ANDed with a pixel value, would zero
  /// out all the bits except the ones corresponding to the green channel.
  /// </remarks>
  public uint GreenMask { get { return format.Gmask; } set { format.Gmask=value; } }

  /// <summary>Gets or sets the bit mask for the blue channel.</summary>
  /// <value>The bit mask for the blue channel.</value>
  /// <remarks>The bit mask for the blue channel is the mask which, if ANDed with a pixel value, would zero
  /// out all the bits except the ones corresponding to the blue channel.
  /// </remarks>
  public uint BlueMask { get { return format.Bmask; } set { format.Bmask=value; } }

  /// <summary>Gets or sets the bit mask for the alpha channel.</summary>
  /// <value>The bit mask for the alpha channel.</value>
  /// <remarks>The bit mask for the alpha channel is the mask which, if ANDed with a pixel value, would zero
  /// out all the bits except the ones corresponding to the alpha channel.
  /// </remarks>
  public uint AlphaMask { get { return format.Amask; } set { format.Amask=value; } }

  public override bool Equals(object obj)
  { PixelFormat other = obj as PixelFormat;
    return other!=null && IsCompatible(other);
  }

  public override int GetHashCode()
  { return (int)(RedMask ^ GreenMask ^ BlueMask ^ AlphaMask) ^ Depth;
  }

  /// <summary>Generates default channel masks for the current bit depth.</summary>
  /// <remarks>Calling this is equivalent to calling <see cref="GenerateDefaultMasks(bool)"/> and passing false.
  /// </remarks>
  public void GenerateDefaultMasks() { GenerateDefaultMasks(false); }

  /// <summary>Generates default channel masks for the current bit depth.</summary>
  /// <param name="withAlpha">If true, an alpha channel will be created. Currently this only has an effect
  /// when <see cref="Depth"/> is 32.
  /// </param>
  /// <remarks>Currently this function doesn't attempt to query the video card to determine the most efficient
  /// defaults for the channel masks. It simply makes BRGA masks for little endian machines and ARGB masks for
  /// big endian machines. This behavior will be changed in the future.
  /// </remarks>
  // FIXNOW: this makes BGRA by default. (it's what my card uses...)
  // is this what we want? perhaps the default should be based upon the current/available video modes?
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

  /// <summary>Checks whether two pixel formats are compatible (whether conversion would be required between them).</summary>
  /// <param name="format">The pixel format to check against.</param>
  /// <returns>True if conversion would not be required between these two formats, and false if it would.</returns>
  /// <remarks>If any of the color depth, channel masks, or palettes of the two formats are different,
  /// they are not considered compatible. Comparing two paletted formats has more overhead than comparing nonpaletted
  /// formats, because the entire palettes need to be compared.
  /// </remarks>
  public bool IsCompatible(PixelFormat format)
  { if(format.Depth!=Depth || format.RedMask!=RedMask || format.GreenMask!=GreenMask ||
       format.BlueMask!=BlueMask || format.AlphaMask!=AlphaMask)
      return false;
    unsafe
    { SDL.Palette* p1=this.format.Palette, p2=format.format.Palette;
      if(p1==null && p2==null) return true;
      if(p1==null || p2==null) return false;
      if(p1->Entries != p2->Entries) return false;
      for(int i=0; i<p1->Entries; i++) if(p1->Colors[i].Value != p2->Colors[i].Value) return false;
    }
    return true;
  }

  internal SDL.PixelFormat format;
}
#endregion

#region GammaRamp
/// <summary>This class represents a gamma ramp.</summary>
/// <remarks>This class consists of three arrays of 256 values each, corresponding to the red, green, and blue
/// channels, representing a mapping between the input and output for that channel. The input is the index into
/// the array, and the output is the 16-bit gamma value at that index, scaled to the output color precision.
/// </remarks>
[CLSCompliant(false)]
public sealed class GammaRamp
{ 
  /// <summary>Gets the array for the red component.</summary>
  /// <value>An array of 256 unsigned shorts.</value>
  /// <remarks>See <see cref="GammaRamp"/> for a description of the array's expected contents.</remarks>
  public ushort[] Red   { get { return red; } }
  /// <summary>Gets the array for the green component.</summary>
  /// <value>An array of 256 unsigned shorts.</value>
  /// <remarks>See <see cref="GammaRamp"/> for a description of the array's expected contents.</remarks>
  public ushort[] Green { get { return green; } }
  /// <summary>Gets the array for the blue component.</summary>
  /// <value>An array of 256 unsigned shorts.</value>
  /// <remarks>See <see cref="GammaRamp"/> for a description of the array's expected contents.</remarks>
  public ushort[] Blue  { get { return blue; } }
  ushort[] red = new ushort[256], green = new ushort[256], blue = new ushort[256];
}
#endregion

#region Video class
/// <summary>This class provides methods for enumerating and setting video modes, for querying the capabilities
/// of the video hardware, etc. <see cref="Video.Initialize"/> must be called before other methods or properties
/// can be used.
/// </summary>
public static class Video
{ 
  #region VideoInfo
  /// <summary>This class contains informations about the video hardware.</summary>
  public class VideoInfo
  { 
    internal unsafe VideoInfo(SDL.VideoInfo* info)
    {
      flags     = info->flags;
      videoMem  = (int)info->videoMem;
      format    = info->format==null ? null : new PixelFormat(info->format);
    }

    /// <summary>Returns a value indicating whether hardware surfaces can be created.</summary>
    /// <value>A boolean indicating whether hardware surfaces can be created.</value>
    public bool Hardware { get { return (flags&SDL.InfoFlag.Hardware)!=0; } }
    /// <summary>Returns a value indicating whether there is a window manager available.</summary>
    /// <value>A boolean indicating whether there is a window manager available.</value>
    public bool WindowManager { get { return (flags&SDL.InfoFlag.WindowManager)!=0; } }
    /// <summary>Returns a value indicating whether hardware-to-hardware blits are accelerated.</summary>
    /// <value>A boolean indicating whether hardware-to-hardware blits are accelerated.</value>
    public bool AccelHH { get { return (flags&SDL.InfoFlag.HH)!=0; } }
    /// <summary>Returns a value indicating whether hardware-to-hardware colorkey blits are accelerated.</summary>
    /// <value>A boolean indicating whether hardware-to-hardware colorkey blits are accelerated.</value>
    public bool AccelHHKeyed { get { return (flags&SDL.InfoFlag.HHKeyed)!=0; } }
    /// <summary>Returns a value indicating whether hardware-to-hardware alpha blits are accelerated.</summary>
    /// <value>A boolean indicating whether hardware-to-hardware alpha blits are accelerated.</value>
    public bool AccelHHAlpha { get { return (flags&SDL.InfoFlag.HHAlpha)!=0; } }
    /// <summary>Returns a value indicating whether software-to-hardware blits are accelerated.</summary>
    /// <value>A boolean indicating whether software-to-hardware blits are accelerated.</value>
    public bool AccelSH { get { return (flags&SDL.InfoFlag.SH)!=0; } }
    /// <summary>Returns a value indicating whether software-to-hardware colorkey blits are accelerated.</summary>
    /// <value>A boolean indicating whether software-to-hardware colorkey blits are accelerated.</value>
    public bool AccelSHKeyed { get { return (flags&SDL.InfoFlag.SHKeyed)!=0; } }
    /// <summary>Returns a value indicating whether software-to-hardware alpha blits are accelerated.</summary>
    /// <value>A boolean indicating whether software-to-hardware alpha blits are accelerated.</value>
    public bool AccelSHAlpha { get { return (flags&SDL.InfoFlag.SHAlpha)!=0; } }
    /// <summary>Returns a value indicating whether color fills are accelerated.</summary>
    /// <value>A boolean indicating whether color fills are accelerated.</value>
    public bool AccelFills { get { return (flags&SDL.InfoFlag.Fills)!=0; } }
    /// <summary>Returns the amount of video memory.</summary>
    /// <value>The total amount of video memory in kilobytes.</value>
    public int VideoMemory { get { return videoMem; } }
    /// <summary>Returns the pixel format of the "best" video mode.</summary>
    /// <value>A <see cref="PixelFormat"/> object representing the pixel format of the "best" video mode.</value>
    [CLSCompliant(false)]
    public PixelFormat Format { get { return format; } }

    PixelFormat  format;
    SDL.InfoFlag flags;
    int          videoMem;
  }
  #endregion

  /// <summary>Occurs when the video mode is set or changed.</summary>
  public static event ModeChangedHandler ModeChanged;

  /// <summary>Returns true if the Video class has been initialized.</summary>
  /// <value>A boolean indicating whether the Video class has been initialized.</value>
  public static bool Initialized { get { return initCount>0; } }

  /// <summary>Returns true if a video mode has been set.</summary>
  /// <value>A boolean indicating whether a video mode has been set.</value>
  public static bool ModeSet { get { return display!=null; } }

  /// <summary>Returns the width of the display surface.</summary>
  /// <value>The width of the display surface, in pixels.</value>
  /// <exception cref="InvalidOperationException">Thrown if a video mode has not been set.</exception>
  public static int Width  { get { AssertModeSet(); return display.Width; } }
  /// <summary>Returns the height of the display surface.</summary>
  /// <value>The height of the display surface, in pixels.</value>
  /// <exception cref="InvalidOperationException">Thrown if a video mode has not been set.</exception>
  public static int Height { get { AssertModeSet(); return display.Height; } }
  /// <summary>Returns the depth of the display surface.</summary>
  /// <value>The depth of the display surface, in bits per pixel.</value>
  /// <exception cref="InvalidOperationException">Thrown if a video mode has not been set.</exception>
  public static int Depth { get { AssertModeSet(); return display.Depth; } }

  /// <summary>Returns information about the video hardware.</summary>
  /// <value>A <see cref="VideoInfo"/> object describing the hardware capabilities.</value>
  /// <exception cref="InvalidOperationException">Thrown if the video subsystem has not been initialized.</exception>
  public static VideoInfo Info { get { AssertInit(); return info; } }

  /// <summary>Returns the primary display surface.</summary>
  /// <value>A <see cref="Surface"/> representing the primary display surface.</value>
  /// <remarks>Drawing to the primary display surface does not necessarily update the display immediately.
  /// <see cref="Flip"/>, <see cref="UpdateRect"/>, or <see cref="UpdateRects"/>
  /// must be called to update the display.
  /// </remarks>
  public static Surface DisplaySurface { get { return display; } }

  /// <summary>Returns the pixel format of the primary display surface.</summary>
  /// <value>A <see cref="PixelFormat"/> object describing the pixel format of the primary display surface.</value>
  /// <exception cref="InvalidOperationException">Thrown if a video mode has not been set.</exception>
  [CLSCompliant(false)]
  public static PixelFormat DisplayFormat { get { AssertModeSet(); return display.Format; } }

  /// <summary>Gets or sets the current gamma ramp.</summary>
  /// <value>A <see cref="GammaRamp"/> object containing the gamma table.</value>
  /// <remarks>This property can be set to null to reset the gamma ramp to the default. Not all hardware supports
  /// changing the gamma.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if a video mode has not been set.</exception>
  [CLSCompliant(false)]
  public static GammaRamp GammaRamp
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

  /// <summary>This method initializes the video subsystem.</summary>
  /// <remarks>This method must be called before the video subsystem can be used. This method can be called
  /// multiple times, and you should call <see cref="Deinitialize"/> a corresponding number of times to
  /// deinitialize the video subsystem.
  /// </remarks>
  public static void Initialize()
  { if(initCount++==0)
    { SDL.Initialize(SDL.InitFlag.Video);
      UpdateInfo();
    }
  }

  /// <summary>This method deinitializes the video subsystem.</summary>
  /// <remarks>When this method is called as many times as <see cref="Initialize"/> has been called, the video
  /// subsystem will be deinitialized.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if this method is called when the video subsystem is not
  /// initialized.
  /// </exception>
  public static void Deinitialize()
  { if(initCount==0) throw new InvalidOperationException("Deinitialize called too many times!");
    if(--initCount==0) SDL.Deinitialize(SDL.InitFlag.Video);
  }

  /// <summary>Flips the primary surface to the display.</summary>
  /// <remarks>This method must be used to update the display when in an OpenGL video mode. When in a normal video
  /// mode while double buffering is in use (see <see cref="SurfaceFlag.DoubleBuffer"/>), this method waits for a
  /// vertical retrace, sets up a page flip, and possibly returns immediately. When double buffering is not used,
  /// calling this is equivalent to using <see cref="UpdateRect"/> to update the entire screen.
  /// This method should not be called when the <see cref="DisplaySurface"/> is locked.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if a video mode has not been set.</exception>
  public static void Flip()
  { AssertModeSet();
    if(usingGL) SDL.SwapBuffers();
    else unsafe { SDL.Check(SDL.Flip(DisplaySurface.surface)); }
  }

  /// <summary>This method returns a list of available screen dimensions given a pixel format and surface flags.
  /// </summary>
  /// <param name="format">A <see cref="PixelFormat"/> object representing the desired pixel format, or null to
  /// use the pixel format returned by <see cref="VideoInfo.Format"/>.
  /// </param>
  /// <param name="flags">The desired surface flags. This plays a strong role in deciding what modes are returned.
  /// For instance, if you use <see cref="SurfaceFlag.Hardware"/>, only modes that support hardware video surfaces
  /// will be returned.
  /// </param>
  /// <returns>An array of <see cref="Size"/> holding the dimensions of matching video modes,
  /// sorted from largest to smallest. If no modes could be found, an empty array will be returned.
  /// If arbitrary dimensions are allowed for the given format, null will be returned. Usually this
  /// occurs in windowed video modes.
  /// </returns>
  /// <exception cref="InvalidOperationException">Thrown if the video subsystem has not been initialized.</exception>
  [CLSCompliant(false)]
  public unsafe static Size[] GetModes(PixelFormat format, SurfaceFlag flags)
  { AssertInit();
    SDL.Rect** list, p;

    if(format==null) list = SDL.ListModes(null, (uint)flags);
    else fixed(SDL.PixelFormat* pf = &format.format) list = SDL.ListModes(pf, (uint)flags);
    if(list==null) return new Size[0];
    else if((int)list==-1) return null;

    int length=0;
    for(p=list; *p!=null; p++) length++;
    Size[] ret = new Size[length];
    for(int i=0; i<length; i++) ret[i] = list[i]->Size;
    return ret;
  }

  /// <summary>Returns a value indicating how well the given video mode is supported.</summary>
  /// <param name="width">The width of the desired video mode.</param>
  /// <param name="height">The height of the desired video mode.</param>
  /// <param name="depth">The color depth of the desired video mode, in bits per pixel.</param>
  /// <param name="flags">The <see cref="SurfaceFlag">surface flags</see> for the desired video mode.</param>
  /// <returns>Returns zero if the requested mode is not supported at any bit depth, or else returns the closest
  /// bit depth at which the requested mode is supported. If the depth returned is the same as the depth passed,
  /// the video mode is supported directly.
  /// </returns>
  /// <exception cref="InvalidOperationException">Thrown if the video subsystem has not been initialized.</exception>
  public static int IsModeSupported(int width, int height, int depth, SurfaceFlag flags)
  { AssertInit();
    return SDL.VideoModeOK(width, height, depth, (uint)flags);
  }

  /// <summary>Sets the video mode.</summary>
  /// <param name="width">The width of the video mode to set.</param>
  /// <param name="height">The height of the video mode to set.</param>
  /// <param name="depth">The color depth of the video mode to set, in bits per pixel.</param>
  /// <remarks>Calling this is equivalent to calling <see cref="SetMode(int, int, int, SurfaceFlag)"/> with
  /// <see cref="SurfaceFlag.None"/>. See <see cref="SetMode(int, int, int, SurfaceFlag)"/> for more information.
  /// An OpenGL mode will be set if the <see cref="SurfaceFlag.OpenGL"/> flag is used, but <see cref="SetGLMode"/>
  /// can also be used for that.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if the video subsystem has not been initialized.</exception>
  public static void SetMode(int width, int height, int depth)
  { SetMode(width, height, depth, SurfaceFlag.None);
  }

  /// <summary>Sets the video mode.</summary>
  /// <param name="width">The width of the video mode to set.</param>
  /// <param name="height">The height of the video mode to set.</param>
  /// <param name="depth">The color depth of the video mode to set, in bits per pixel.</param>
  /// <param name="flags">The <see cref="SurfaceFlag">flags</see> of the video mode to set.</param>
  /// <remarks>This method attempts to set the video mode. If the video mode is not directly supported, an attempt
  /// will be made to emulate it. If there is no matching video mode at the given resolution, the next higher
  /// resolution will be used and the display will be centered within it. If the color depth is not supported,
  /// the color depth will be emulated with a shadow surface. The <see cref="SurfaceFlag.NoEmulation"/> flag can
  /// be used to disable this emulation, in which case the format of the <see cref="DisplaySurface"/> may not
  /// match what you requested.
  /// An OpenGL mode will be set if the <see cref="SurfaceFlag.OpenGL"/> flag is used, but <see cref="SetGLMode"/>
  /// can also be used for that.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if the video subsystem has not been initialized.</exception>
  public static void SetMode(int width, int height, int depth, SurfaceFlag flags)
  { AssertInit();
    if((flags & SurfaceFlag.OpenGL) != 0) SetGLMode(width, height, depth, flags);
    else SetMode(width, height, depth, (uint)flags);
  }

  /// <summary>Sets an OpenGL video mode.</summary>
  /// <param name="width">The width of the video mode to set.</param>
  /// <param name="height">The height of the video mode to set.</param>
  /// <param name="depth">The color depth of the video mode to set, in bits per pixel.</param>
  /// <remarks>Calling this method is equivalent to calling <see cref="SetGLMode(int,int,int,SurfaceFlag,GLOptions)"/>
  /// and passing <see cref="SurfaceFlag.None"/> for the surface flags and null for the options.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if the video subsystem has not been initialized.</exception>
  public static void SetGLMode(int width, int height, int depth)
  { SetGLMode(width, height, depth, SurfaceFlag.None, null);
  }

  /// <summary>Sets an OpenGL video mode.</summary>
  /// <param name="width">The width of the video mode to set.</param>
  /// <param name="height">The height of the video mode to set.</param>
  /// <param name="depth">The color depth of the video mode to set, in bits per pixel.</param>
  /// <param name="opts">A <see cref="GLOptions"/> class representing the OpenGL options to use, or null
  /// to not change any OpenGL options.
  /// </param>
  /// <remarks>Calling this method is equivalent to calling <see cref="SetGLMode(int,int,int,SurfaceFlag,GLOptions)"/>
  /// and passing <see cref="SurfaceFlag.None"/> for the surface flags.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if the video subsystem has not been initialized.</exception>
  [CLSCompliant(false)]
  public static void SetGLMode(int width, int height, int depth, GLOptions opts)
  { SetGLMode(width, height, depth, SurfaceFlag.None, opts);
  }

  /// <summary>Sets an OpenGL video mode.</summary>
  /// <param name="width">The width of the video mode to set.</param>
  /// <param name="height">The height of the video mode to set.</param>
  /// <param name="depth">The color depth of the video mode to set, in bits per pixel.</param>
  /// <param name="flags">The <see cref="SurfaceFlag">flags</see> of the video mode to set.</param>
  /// <remarks>Calling this method is equivalent to calling <see cref="SetGLMode(int,int,int,SurfaceFlag,GLOptions)"/>
  /// and passing null for the options.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if the video subsystem has not been initialized.</exception>
  public static void SetGLMode(int width, int height, int depth, SurfaceFlag flags)
  { SetGLMode(width, height, depth, flags, null);
  }

  /// <summary>Sets an OpenGL video mode.</summary>
  /// <param name="width">The width of the video mode to set.</param>
  /// <param name="height">The height of the video mode to set.</param>
  /// <param name="depth">The color depth of the video mode to set, in bits per pixel.</param>
  /// <param name="flags">The <see cref="SurfaceFlag">flags</see> of the video mode to set.</param>
  /// <param name="opts">A <see cref="GLOptions"/> class representing the OpenGL options to use, or null
  /// to not change any OpenGL options.
  /// </param>
  /// <remarks>Calling this method will attempt to set the video mode and initialize OpenGL.
  /// While most OpenGL attributes can be set through the OpenGL API, those managed by the <see cref="GLOptions"/>
  /// class must be set at the time the video mode is set by passing them to this function.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if the video subsystem has not been initialized.</exception>
  [CLSCompliant(false)]
  public unsafe static void SetGLMode(int width, int height, int depth, SurfaceFlag flags, GLOptions opts)
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

  /// <summary>This function returns the current gamma settings.</summary>
  /// <param name="red">An output parameter to which the red gamma multiplier will be written.</param>
  /// <param name="green">An output parameter to which the green gamma multiplier will be written.</param>
  /// <param name="blue">An output parameter to which the blue gamma multiplier will be written.</param>
  /// <remarks>This method will of course not return a proper result if a <see cref="GammaRamp"/> is in effect.
  /// The gamma settings control how color values are mapped onto the actual display. Not all hardware supports
  /// changing the gamma.
  /// </remarks>
  public static void GetGamma(out float red, out float green, out float blue)
  { red=redGamma; green=greenGamma; blue=blueGamma;
  }

  /// <summary>This function sets the current gamma.</summary>
  /// <param name="red">A parameter holding the gamma multiplier for the red channel.</param>
  /// <param name="green">A parameter holding the gamma multiplier for the green channel.</param>
  /// <param name="blue">A parameter holding the gamma multiplier for the blue channel.</param>
  /// <remarks>The gamma settings control how color values are mapped onto the actual display.
  /// Not all hardware supports changing the gamma.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if a video mode has not been set.</exception>
  public static void SetGamma(float red, float green, float blue)
  { AssertModeSet();
    Check(SDL.SetGamma(red, green, blue));
    redGamma=red; greenGamma=green; blueGamma=blue;
  }

  /// <summary>Makes sure the given area is updated on the display.</summary>
  /// <param name="rect">A <see cref="Rectangle"/> holding the bounds of the area to update. The rectangle must be
  /// within the screen boundaries. If it is not, the behavior of this method is undefined.
  /// </param>
  /// <remarks>It is advised to call this method only once per frame because each call may have significant overhead.
  /// If you want to update multiple sections of the screen, use <see cref="UpdateRects"/>.
  /// This method should not be called when the <see cref="DisplaySurface"/> is locked.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if a video mode has not been set.</exception>
  public static void UpdateRect(Rectangle rect) { UpdateRect(rect.X, rect.Y, rect.Width, rect.Height); }

  /// <summary>Makes sure the given area is updated on the display.</summary>
  /// <param name="x">The X coordinate of the left edge of the area to update.</param>
  /// <param name="y">The Y coordinate of the top edge of the area to update.</param>
  /// <param name="width">The width of the area to update.</param>
  /// <param name="height">The height of the area to update.</param>
  /// <remarks>The bounds to update must be within the screen boundaries. If they are not, the behavior of this method
  /// is undefined. It is advised to call this method only once per frame because each call may have significant
  /// overhead. If you want to update multiple sections of the screen, use <see cref="UpdateRects"/>.
  /// This method should not be called when the <see cref="DisplaySurface"/> is locked.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if a video mode has not been set.</exception>
  public static void UpdateRect(int x, int y, int width, int height)
  { AssertModeSet();
    unsafe { SDL.UpdateRect(display.surface, x, y, width, height); }
  }

  /// <summary>Makes sure the given set of rectangles is updated on the display.</summary>
  /// <param name="rects">An array of <see cref="Rectangle"/> holding the areas to update on the screen.
  /// The rectangles must all be within the screen boundaries. If they are not, the behavior of this method is
  /// undefined.
  /// </param>
  /// <remarks>It is advised to call this method only once per frame because each call may have significant overhead.
  /// The rectangles are not automatically merged or checked for overlap,
  /// so you should make an attempt to merge them in an efficient way to prevent overdraw.
  /// This method should not be called when the <see cref="DisplaySurface"/> is locked.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if a video mode has not been set.</exception>
  public static void UpdateRects(Rectangle[] rects) { UpdateRects(rects, 0, rects.Length); }

  /// <summary>Makes sure the given set of rectangles is updated on the display.</summary>
  /// <param name="rects">An array of <see cref="Rectangle"/> holding the areas to update on the screen.
  /// The rectangles must all be within the screen boundaries. If it is not, the behavior of this method is undefined.
  /// </param>
  /// <param name="index">The starting index of the rectangles to use in the update.</param>
  /// <param name="length">The number of rectangles to use in the update.</param>
  /// <remarks>It is advised to call this method only once per frame because each call may have significant overhead.
  /// The rectangles are not automatically merged or checked for overlap,
  /// so you should make an attempt to merge them in an efficient way to prevent overdraw.
  /// This method should not be called when the <see cref="DisplaySurface"/> is locked.
  /// </remarks>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="rects"/> is null.</exception>
  /// <exception cref="InvalidOperationException">Thrown if a video mode has not been set.</exception>
  public unsafe static void UpdateRects(Rectangle[] rects, int index, int length)
  { AssertModeSet();
    if(rects==null) throw new ArgumentNullException("rects");
    SDL.Rect* array = stackalloc SDL.Rect[length];
    for(int i=index; i<length; i++) array[i] = new SDL.Rect(rects[i]);
    SDL.UpdateRects(display.surface, (uint)length, array);
  }

  /// <summary>Maps a color to the corresponding pixel value for the display surface.</summary>
  /// <param name="color">The <see cref="System.Drawing.Color"/> to map.</param>
  /// <returns>The raw pixel value corresponding to the nearest color the display surface can draw.</returns>
  /// <remarks>Calling this is equivalent to calling <see cref="Surface.MapColor(Color)"/> on the
  /// <see cref="DisplaySurface"/>.
  /// </remarks>
  [CLSCompliant(false)]
  public static uint MapColor(Color color) { return DisplaySurface.MapColor(color); }

  /// <summary>Maps a raw pixel value to the corresponding color for the display surface.</summary>
  /// <param name="color">The raw pixel value to map.</param>
  /// <returns>The <see cref="System.Drawing.Color"/> corresponding to the given raw pixel value.</returns>
  /// <remarks>Calling this is equivalent to calling <see cref="Surface.MapColor(uint)"/> on the
  /// <see cref="DisplaySurface"/>.
  /// </remarks>
  [CLSCompliant(false)]
  public static Color MapColor(uint color) { return DisplaySurface.MapColor(color); }

  /// <summary>Sets the physical palette.</summary>
  /// <param name="colors">An array of colors to use for setting the palette.</param>
  /// <param name="startIndex">The starting index within the physical palette where colors will be written.</param>
  /// <param name="startColor">The starting index within the color array from where colors will be read.</param>
  /// <param name="numColors">The number of colors to set.</param>
  /// <returns>Returns true if the palette was set as specified. If the <see cref="DisplaySurface"/> does not
  /// have a physical palette, or the colors could not all be set exactly, false will be returned.
  /// </returns>
  /// <remarks>Calling this is equivalent to calling <see cref="Surface.SetPalette"/> on the
  /// <see cref="DisplaySurface"/> to set the physical palette.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if a video mode has not been set.</exception>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if the number of colors would overflow the palette or
  /// one of the indices are invalid.
  /// </exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="colors"/> is null.</exception>
  public static bool SetPalette(Color[] colors, int startIndex, int startColor, int numColors)
  { AssertModeSet();
    return DisplaySurface.SetPalette(colors, startIndex, startColor, numColors, PaletteType.Physical);
  }

  static unsafe void SetMode(int width, int height, int depth, uint flags)
  { SDL.Surface* surface = SDL.SetVideoMode(width, height, depth, flags);
    if(surface==null) SDL.RaiseError();
    if(display!=null) { display.Dispose(); display=null; } // set to null so ModeSet is false in the next constructor
    display = new Surface(surface, false);
    UpdateInfo();
    if((flags & (uint)SDL.VideoFlag.FullScreen) == 0) WM.WindowTitle = WM.WindowTitle; // assuming SDL doesn't reset it
    if(ModeChanged!=null) ModeChanged();
  }
  static unsafe void UpdateInfo() { info = new VideoInfo(SDL.GetVideoInfo()); }
  static void Check(int result) { if(result!=0) SDL.RaiseError(); }
  static void AssertInit()      { if(initCount==0) throw new InvalidOperationException("Not initialized"); }
  static void AssertModeSet()   { if(display==null) throw new InvalidOperationException("Video mode not set"); }

  static GammaRamp ramp;
  static Surface   display;
  static float     redGamma=1.0f, blueGamma=1.0f, greenGamma=1.0f;
  static uint      initCount;
  static VideoInfo info;
  static bool      usingGL;
}
#endregion

#region WM class
/// <summary>This class provides some support for communicating with the Windowing Manager, if running in a
/// windowed video mode.
/// </summary>
public static class WM
{ 
  /// <summary>Gets whether the application window is active (focused) or not.</summary>
  /// <value>A boolean indicating whether the application window has input focus.</value>
  /// <remarks>This property is updated by the <see cref="Events.Events"/> class when it receives an event from
  /// the underlying system.
  /// </remarks>
  public static bool Active { get { return Events.Events.inputFocus; } }

  /// <summary>Gets whether the application window is minimized (iconified)or not.</summary>
  /// <value>A boolean indicating whether the application window has been minimized (iconified).</value>
  /// <remarks>This property is updated by the <see cref="Events.Events"/> class when it receives an event from
  /// the underlying system, as well as by the <see cref="Minimize"/> method.
  /// </remarks>
  public static bool Minimized { get { return Events.Events.minimized; } }

  /// <summary>Gets whether the application window has mouse focus.</summary>
  /// <value>A boolean indicating whether the application window has mouse focus.</value>
  /// <remarks>In some windowing systems, a window can be active without having mouse focus. This property is
  /// updated by the <see cref="Events.Events"/> class when it receives an event from the underlying system.
  /// </remarks>
  public static bool MouseFocus { get { return Events.Events.mouseFocus; } }

  /// <summary>Gets or sets the title displayed in the application's window.</summary>
  /// <value>A string containing the title displayed in the application's window.</value>
  /// <remarks>This should be set after the video mode is set for maximum compatibility.</remarks>
  public static string WindowTitle
  { get
    { string title, icon;
      SDL.WM_GetCaption(out title, out icon);
      return title;
    }
    set { SDL.WM_SetCaption(value, null); }
  }

  /// <summary>Gets or sets whether the mouse cursor will be bounded to the window when in a windowed video mode.
  /// </summary>
  /// <value>A boolean indicating whether the mouse cursor will be bounded to the window when in a windowed video
  /// mode. If set to true, the mouse will be forced to say within the window boundaries.
  /// </value>
  public static bool MouseBounded // TODO: possibly move this to Input?
  { get
    { SDL.GrabMode mode = SDL.WM_GrabInput(SDL.GrabMode.Query);
      return mode==SDL.GrabMode.On;
    }
    set { SDL.WM_GrabInput(value ? SDL.GrabMode.On : SDL.GrabMode.Off); }
  }

  /// <summary>Sets the icon in use for the window.</summary>
  /// <remarks>The surface used for the icon currently must be 32x32 for compatibility.</remarks>
  public static void SetIcon(Surface icon)
  { if(icon.Width!=32 || icon.Height!=32) throw new ArgumentException("Icon should be 32x32 for compatibility");
    unsafe { SDL.WM_SetIcon(icon.surface, null); }
  }

  /// <summary>Attempts to minimize (iconify) the window.</summary>
  public static void Minimize()
  { if(SDL.WM_IconifyWindow()!=0) SDL.RaiseError();
    Events.Events.minimized = true;
  }
}

#endregion

} // namespace GameLib.Video