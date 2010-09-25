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
using GameLib.Interop;
using GameLib.Interop.OpenGL;

// TODO: support 15-bit color
// nice stuff here: http://www.sulaco.co.za/default.htm

namespace GameLib.Video
{

#region OpenGL
/// <summary>This class provides some high-level support for interfacing with the OpenGL API.</summary>
// TODO: add more methods
public static class OpenGL
{ 
  /// <summary>Returns a collection of extensions supported by OpenGL.</summary>
  /// <value>A collection of strings representing the extensions supported by OpenGL.</value>
  public static System.Collections.ICollection Extensions
  { get { if(extensions==null) InitExtensions(); return extensions; }
  }

  /// <summary>Returns true if the OpenGL implementation supports textures with dimensions that are not a power of two
  /// in length.
  /// </summary>
  public static bool HasNonPowerOfTwoExtension
  {
    get { return HasExtension("GL_ARB_texture_non_power_of_two"); }
  }

  /// <summary>Gets the OpenGL major version number.</summary>
  public static int VersionMajor { get { if(version==0) InitVersion(); return version>>16; } }
  /// <summary>Gets the OpenGL minor version number.</summary>
  public static int VersionMinor { get { if(version==0) InitVersion(); return version&0xFFFF; } }

  /// <summary>Duplicates pixels from the edges of the rectangular area to create a border.</summary>
  /// <param name="surface">The surface that contains the source data, and into which the border will be created.</param>
  /// <param name="textureArea">The area of the surface containing the image data. The border will be created in the
  /// pixels adjacent to this rectangle.
  /// </param>
  /// <remarks>The purpose of this function is to allow you to use texture filtering with images that don't take up the
  /// entire texture. Adding an extra border around the image ensures that there are no filter artifacts when the
  /// filter algorithm reads pixels adjacent to the image. You do not need to reserve space for the border in the
  /// surface. If no space is available to create a portion of the border, that portion will not be created.
  /// <paramref name="textureArea"/> does need to reference a valid area of the surface, however.
  /// </remarks>
  public static void AddTextureBorder(Surface surface, Rectangle textureArea)
  {
    if(surface == null)
    {
      throw new ArgumentNullException();
    }

    if(Rectangle.Intersect(textureArea, surface.Bounds) != textureArea)
    {
      throw new ArgumentException("Texture area does not reference a valid area of the surface.");
    }

    surface.Lock();
    unsafe
    {
      byte* data = (byte*)surface.Data;
      int bytesPerPixel = surface.Format.Depth / 8;

      bool leftBorder = textureArea.Left != 0, rightBorder  = textureArea.Right  != surface.Width,
           topBorder  = textureArea.Top  != 0, bottomBorder = textureArea.Bottom != surface.Height;

      // add the edges
      if(topBorder) // if the rectangle isn't already at the top, add a border above the area
      {
        byte* topLeft = data + textureArea.Top*surface.Pitch + textureArea.Left*bytesPerPixel;
        Unsafe.Copy(topLeft, topLeft - surface.Pitch, textureArea.Width * bytesPerPixel);
      }
      if(bottomBorder) // etc
      {
        byte* bottomLeft = data + (textureArea.Bottom-1)*surface.Pitch + textureArea.Left*bytesPerPixel;
        Unsafe.Copy(bottomLeft, bottomLeft + surface.Pitch, textureArea.Width * bytesPerPixel);
      }
      if(leftBorder)
      {
        byte* topLeft = data + textureArea.Top*surface.Pitch + textureArea.Left*bytesPerPixel;
        AddHorizontalBorder(topLeft, topLeft - bytesPerPixel, textureArea.Height, surface.Pitch, bytesPerPixel);
      }
      if(rightBorder)
      {
        byte* topRight = data + textureArea.Top*surface.Pitch + (textureArea.Right-1)*bytesPerPixel;
        AddHorizontalBorder(topRight, topRight + bytesPerPixel, textureArea.Height, surface.Pitch, bytesPerPixel);
      }

      // now add the corners
      if(topBorder && leftBorder) // top-left
      {
        surface.PutPixel(textureArea.X-1, textureArea.Y-1, surface.GetPixelRaw(textureArea.X, textureArea.Y));
      }
      if(topBorder && rightBorder) // top-right
      {
        surface.PutPixel(textureArea.Right, textureArea.Y-1, surface.GetPixelRaw(textureArea.Right-1, textureArea.Y));
      }
      if(bottomBorder && rightBorder) // bottom-right
      {
        surface.PutPixel(textureArea.Right, textureArea.Bottom,
                         surface.GetPixelRaw(textureArea.Right-1, textureArea.Bottom-1));
      }
      if(bottomBorder && leftBorder) // bottom-left
      {
        surface.PutPixel(textureArea.X-1, textureArea.Bottom,
                         surface.GetPixelRaw(textureArea.X, textureArea.Bottom-1));
      }
    }
    surface.Unlock();
  }

  /// <summary>Returns the size of the smallest texture capable of containing an image of the given size.</summary>
  public static Size GetTextureSize(Size imageSize)
  {
    if(imageSize.Width <= 0 || imageSize.Height <= 0)
    {
      throw new ArgumentOutOfRangeException("imageSize", imageSize, "The image dimensions must be positive.");
    }

    if(HasNonPowerOfTwoExtension) { return imageSize; }
    else
    {
      Size textureSize = new Size(1, 1);
      while(textureSize.Width  < imageSize.Width)  textureSize.Width  *= 2;
      while(textureSize.Height < imageSize.Height) textureSize.Height *= 2;
      return textureSize;
    }
  }

  /// <summary>Returns true if OpenGL supports the given extension.</summary>
  /// <param name="name">The name of the extension to check for. This is case-sensitive.</param>
  /// <returns>True if OpenGL supports the given extension and false otherwise.</returns>
  public static bool HasExtension(string name)
  { if(extensions==null) InitExtensions();
    return Array.BinarySearch(extensions, name) >= 0;
  }

  #region TexImage2D
  /// <include file="../documentation.xml" path="//Video/OpenGL/TexImage2D/*"/>
  public static bool TexImage2D(Surface surface)
  { Size dummy;
    return TexImage2D(0, 0, 0, surface, out dummy);
  }
  /// <param name="texSize">An output parameter that will be filled with the dimensions of the texture that was
  /// created. This may be different from the dimensions of the surface because most OpenGL implementations require
  /// texture dimensions to be powers of two in length.
  /// </param>
  /// <include file="../documentation.xml" path="//Video/OpenGL/TexImage2D/*"/>
  public static bool TexImage2D(Surface surface, out Size texSize)
  { return TexImage2D(0, 0, 0, surface, out texSize);
  }
  /// <param name="internalFormat">The internal format that OpenGL should use to store the texture.</param>
  /// <param name="texSize">An output parameter that will be filled with the dimensions of the texture that was
  /// created. This may be different from the dimensions of the surface because most OpenGL implementations require
  /// texture dimensions to be powers of two in length.
  /// </param>
  /// <include file="../documentation.xml" path="//Video/OpenGL/TexImage2D/*"/>
  public static bool TexImage2D(int internalFormat, Surface surface, out Size texSize)
  { return TexImage2D(internalFormat, 0, 0, surface, out texSize);
  }
  /// <param name="internalFormat">The internal format that OpenGL should use to store the texture.</param>
  /// <param name="level">The mipmap level to upload this image into.</param>
  /// <param name="texSize">An output parameter that will be filled with the dimensions of the texture that was
  /// created. This may be different from the dimensions of the surface because most OpenGL implementations require
  /// texture dimensions to be powers of two in length.
  /// </param>
  /// <include file="../documentation.xml" path="//Video/OpenGL/TexImage2D/*"/>
  public static bool TexImage2D(int internalFormat, int level, Surface surface, out Size texSize)
  { return TexImage2D(internalFormat, level, 0, surface, out texSize);
  }
  /// <param name="internalFormat">The internal format that OpenGL should use to store the texture.</param>
  /// <param name="level">The mipmap level to upload this image into.</param>
  /// <param name="border">The width of the border.</param>
  /// <param name="texSize">An output parameter that will be filled with the dimensions of the texture that was
  /// created. This may be different from the dimensions of the surface because most OpenGL implementations require
  /// texture dimensions to be powers of two in length.
  /// </param>
  /// <include file="../documentation.xml" path="//Video/OpenGL/TexImage2D/*"/>
  public static bool TexImage2D(int internalFormat, int level, int border, Surface surface, out Size texSize)
  { PixelFormat pf = surface.Format;
    int format=0, awidth=surface.Width-border*2, aheight=surface.Height-border*2;
    bool usingAlpha = surface.UsingAlpha || surface.UsingKey;

    texSize = GetTextureSize(new Size(awidth, aheight));

    if(internalFormat==0) internalFormat = usingAlpha ? GL.GL_RGBA : GL.GL_RGB;
    if(!WillTextureFit(internalFormat, level, border, texSize.Width, texSize.Height))
    { texSize = Size.Empty;
      return false;
    }

    // TODO: be careful about alignment. see glPixelStore()

    if(awidth==texSize.Width && aheight==texSize.Height && !surface.UsingKey && surface.Pitch==surface.Width*surface.Depth/8)
    { int type = GL.GL_UNSIGNED_BYTE;
      bool hasPPE = VersionMinor>=2 && VersionMajor==1 || HasExtension("GL_EXT_packed_pixels") || VersionMajor>1;
      bool hasBGR = VersionMinor>=2 && VersionMajor==1 || HasExtension("EXT_bgra") || VersionMajor>1;
      if(surface.Depth==16 && hasPPE)
      { // TODO: make this work with big-endian machines
        if(pf.AlphaMask == 1 && pf.RedMask == 0xF800 && pf.GreenMask == 0x7C0 && pf.BlueMask == 0x3E)
        {
          format = GL.GL_RGBA;
          type   = GL.GL_UNSIGNED_SHORT_5_5_5_1;
        }
        else if(pf.RedMask==0xF800 && pf.GreenMask==0x7E0 && pf.BlueMask==0x1F)
        { 
          format = GL.GL_RGB;
          type = GL.GL_UNSIGNED_SHORT_5_6_5;
        }
      }
      else if(surface.Depth==24)
      { if(pf.RedMask==0xFF && pf.GreenMask==0xFF00 && pf.BlueMask==0xFF0000)
          #if BIGENDIAN
          format = hasBGR ? GL.GL_BGR_EXT : 0;
          #else
          format = GL.GL_RGB;
          #endif
        else if(pf.RedMask==0xFF0000 && pf.GreenMask==0xFF00 && pf.BlueMask==0xFF)
          #if BIGENDIAN
          format = GL.GL_RGB;
          #else
          format = hasBGR ? GL.GL_BGR_EXT : 0;
          #endif
      }
      else if(surface.Depth==32 && pf.AlphaMask!=0)
      { 
        #if BIGENDIAN
        if(pf.RedMask==0xFF000000 && pf.GreenMask==0xFF0000 && pf.BlueMask==0xFF00 && pf.AlphaMask==0xFF)
          format = GL.GL_RGBA;
        else if(pf.RedMask==0xFF00 && pf.GreenMask==0xFF0000 && pf.BlueMask==0xFF000000 && pf.AlphaMask==0xFF)
          format = hasBGR ? GL.GL_BGRA_EXT : 0;
        #else
        if(pf.RedMask==0xFF && pf.GreenMask==0xFF00 && pf.BlueMask==0xFF0000 && pf.AlphaMask==0xFF000000)
          format = GL.GL_RGBA;
        else if(pf.RedMask==0xFF0000 && pf.GreenMask==0xFF00 && pf.BlueMask==0xFF && pf.AlphaMask==0xFF000000)
          format = hasBGR ? GL.GL_BGRA_EXT : 0;
        #endif
      }
      if(format!=0)
      { surface.Lock();
        unsafe
        { if(internalFormat==0)
            internalFormat = format==GL.GL_RGBA || format==GL.GL_BGRA_EXT ? GL.GL_RGBA8 : GL.GL_RGB8;
          GL.glTexImage2D(GL.GL_TEXTURE_2D, level, internalFormat, surface.Width, surface.Height, border, format,
                          type, surface.Data);
        }
        surface.Unlock();
        return true;
      }
    }

    pf = new PixelFormat(usingAlpha ? 32 : 24, usingAlpha, false);
    #if BIGENDIAN
    pf.RedMask=0xFF000000; pf.GreenMask=0xFF0000; pf.BlueMask=0xFF00; pf.AlphaMask=(pf.Depth==32 ? 0xFF : 0);
    #else
    pf.RedMask=0xFF; pf.GreenMask=0xFF00; pf.BlueMask=0xFF0000; pf.AlphaMask=(pf.Depth==32 ? 0xFF000000 : 0);
    #endif

    Surface temp = new Surface(texSize.Width+border*2, texSize.Height+border*2, pf,
                               pf.Depth==32 ? SurfaceFlag.SourceAlpha : 0);
    bool oua = surface.UsingAlpha;
    if(surface.Format.AlphaMask!=0 || surface.Alpha==255) surface.UsingAlpha=false;

    surface.Blit(temp, border, border);
    // TODO: add border (not opengl border, but filter border)
    surface.UsingAlpha = oua;
    temp.Lock();
    try
    { if(temp.Pitch==temp.Width*temp.Depth/8)
        unsafe
        { GL.glTexImage2D(GL.GL_TEXTURE_2D, level, internalFormat, temp.Width, temp.Height, border,
                          pf.Depth==32 ? GL.GL_RGBA : GL.GL_RGB, GL.GL_UNSIGNED_BYTE, temp.Data);
        }
      else
      { int line = temp.Width*temp.Depth/8;
        byte[] arr = new byte[temp.Height*line];
        unsafe
        { fixed(byte* arrp=arr)
          { byte* dest=arrp, src=(byte*)temp.Data;
            for(int y=0; y<temp.Height; dest+=line,src+=temp.Pitch,y++) Unsafe.Copy(src, dest, line);
            GL.glTexImage2D(GL.GL_TEXTURE_2D, level, internalFormat, temp.Width, temp.Height, border,
                            pf.Depth==32 ? GL.GL_RGBA : GL.GL_RGB, GL.GL_UNSIGNED_BYTE, arrp);
          }
        }
      }
    }
    finally { temp.Unlock(); temp.Dispose(); }
    return true;
  }
  #endregion

  #region WillTextureFit
  /// <include file="../documentation.xml" path="//Video/OpenGL/WillTextureFit/*"/>
  public static bool WillTextureFit(int internalFormat, int width, int height)
  { return WillTextureFit(internalFormat, 0, 0, width, height);
  }
  /// <param name="level">The mipmap level of the texture.</param>
  /// <include file="../documentation.xml" path="//Video/OpenGL/WillTextureFit/*"/>
  public static bool WillTextureFit(int internalFormat, int level, int width, int height)
  { return WillTextureFit(internalFormat, level, 0, width, height);
  }
  /// <param name="level">The mipmap level of the texture.</param>
  /// <param name="border">The width of the texture's border.</param>
  /// <include file="../documentation.xml" path="//Video/OpenGL/WillTextureFit/*"/>
  public static bool WillTextureFit(int internalFormat, int level, int border, int width, int height)
  { int fits;
    unsafe
    { GL.glTexImage2D(GL.GL_PROXY_TEXTURE_2D, level, internalFormat, width, height, border, GL.GL_RGB,
                      GL.GL_UNSIGNED_BYTE, (void*)null);
    }
    GL.glGetTexLevelParameteriv(GL.GL_PROXY_TEXTURE_2D, level, GL.GL_TEXTURE_WIDTH, out fits);
    return fits!=0;
  }
  #endregion

  static void InitExtensions()
  { string str = GL.glGetString(GL.GL_EXTENSIONS);
    if(str == null) throw new InvalidOperationException("OpenGL not initialized yet!");
    extensions = str.Split(' ');
    Array.Sort(extensions);
  }

  static void InitVersion()
  { string str = GL.glGetString(GL.GL_VERSION);
    if(str == null) throw new InvalidOperationException("OpenGL not initialized yet!");
    int pos = str.IndexOf(' ');
    string[] bits = (pos==-1 ? str : str.Substring(0, pos)).Split('.');
    version = (int.Parse(bits[0])<<16) + int.Parse(bits[1]);
  }

  static unsafe void AddHorizontalBorder(byte* src, byte* dest, int pixels, int pitch, int bytesPerPixel)
  {
    for(int i=0; i<pixels; i++)
    {
      for(int j=0; j<bytesPerPixel; j++)
      {
        dest[j] = src[j];
      }

      src  += pitch;
      dest += pitch;
    }
  }

  static string[] extensions;
  static int version;
}
#endregion

#region GLTexture2D
/// <summary>This class wraps an OpenGL texture handle.</summary>
// TODO: add support for mipmapping
public class GLTexture2D : IDisposable, IGuiImage
{ 
  /// <summary>Creates an uninitialized instance of this class.</summary>
  /// <remarks>After using this constructor, no texture will have been loaded. <see cref="Load"/> will have to
  /// be called before this class can be used.
  /// </remarks>
  public GLTexture2D() { }
  /// <summary>Initializes this texture from an image file on disk.</summary>
  /// <param name="filename">The path to the image file.</param>
  public GLTexture2D(string filename) : this(0, 0, 0, new Surface(filename)) { }
  /// <summary>Initializes this texture from a stream.</summary>
  /// <param name="stream">The stream from which to load image data. The stream should be seekable, with its entire
  /// set of data devoted to the image. The stream will be closed after the texture is loaded.
  /// </param>
  public GLTexture2D(System.IO.Stream stream) : this(0, 0, 0, new Surface(stream)) { }
  /// <summary>Initializes this this texture from a surface.</summary>
  /// <param name="surface">The surface from which the texture will be loaded.</param>
  public GLTexture2D(Surface surface) : this(0, 0, 0, surface) { }
  /// <summary>Initializes this this texture from a surface.</summary>
  /// <param name="internalFormat">The internal format OpenGL should use for the texture.</param>
  /// <param name="surface">The surface from which the texture will be loaded.</param>
  public GLTexture2D(int internalFormat, Surface surface) : this(internalFormat, 0, 0, surface) { }
  /// <summary>Initializes this this texture from a surface.</summary>
  /// <param name="internalFormat">The internal format OpenGL should use for the texture.</param>
  /// <param name="level">The mipmap level to upload the texture into.</param>
  /// <param name="surface">The surface from which the texture will be loaded.</param>
  public GLTexture2D(int internalFormat, int level, Surface surface) : this(internalFormat, level, 0, surface) { }
  /// <summary>Initializes this this texture from a surface.</summary>
  /// <param name="internalFormat">The internal format OpenGL should use for the texture.</param>
  /// <param name="level">The mipmap level to upload the texture into.</param>
  /// <param name="border">The width of the border.</param>
  /// <param name="surface">The surface from which the texture will be loaded.</param>
  public GLTexture2D(int internalFormat, int level, int border, Surface surface)
  { if(!Load(internalFormat, level, border, surface)) throw new OutOfTextureMemoryException();
  }
  ~GLTexture2D() { Dispose(true); }

  /// <summary>Unloads this texture from video memory.</summary>
  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  /// <summary>Gets the height of the image in pixels.</summary>
  /// <remarks>The size of the image may differ from the size of the texture because most OpenGL implementations
  /// require that texture dimensions be powers of two.
  /// </remarks>
  public int ImgHeight
  { get { return imgSize.Height; }
    set
    { if(value<0) throw new ArgumentOutOfRangeException("ImgHeight", value, "must not be negative");
      imgSize.Height = value;
      texCoordHeight = TexHeight == 0 ? 0 : (double)value / TexHeight;
      pixelToTexY = value == 0 ? 0 : texCoordHeight / value;
    }
  }

  /// <summary>Gets the size of the image in pixels.</summary>
  /// <remarks>The size of the image may differ from the size of the texture because most OpenGL implementations
  /// require that texture dimensions be powers of two.
  /// </remarks>
  public Size ImgSize
  { get { return imgSize; }
    set
    {
      ImgWidth  = value.Width;
      ImgHeight = value.Height;
    }
  }

  /// <summary>Gets the width of the image in pixels.</summary>
  /// <remarks>The size of the image may differ from the size of the texture because most OpenGL implementations
  /// require that texture dimensions be powers of two.
  /// </remarks>
  public int ImgWidth
  { get { return imgSize.Width; }
    set
    { 
      if(value < 0) throw new ArgumentOutOfRangeException("ImgWidth", value, "must not be negative");
      imgSize.Width = value;
      texCoordWidth = TexWidth == 0 ? 0 : (double)value / TexWidth;
      pixelToTexX = value == 0 ? 0 : texCoordWidth / value;
    }
  }

  /// <summary>Gets a factor that can be multiplied by a pixel's X position within the image to convert it to that
  /// pixel's U texture coordinate. This is equal to <see cref="TexCoordWidth"/> divided by the image width.
  /// </summary>
  public double PixelToTexCoordX
  {
    get { return pixelToTexX; }
  }

  /// <summary>Gets a factor that can be multiplied by a pixel's Y position within the image to convert it to that
  /// pixel's V texture coordinate. This is equal to <see cref="TexCoordHeight"/> divided by the image height.
  /// </summary>
  public double PixelToTexCoordY
  {
    get { return pixelToTexY; }
  }

  /// <summary>Gets the width of the image in texture coordinates. This is equal to the image width divided by the
  /// texture width.
  /// </summary>
  public double TexCoordWidth
  {
    get { return texCoordWidth; }
  }

  /// <summary>Gets the height of the image in texture coordinates. This is equal to the image height divided by the
  /// texture height.
  /// </summary>
  public double TexCoordHeight
  {
    get { return texCoordHeight; }
  }

  /// <summary>Evaluates to true if a texture has been loaded.</summary>
  public bool Initialized { get { return texture!=0; } }

  /// <summary>Gets the OpenGL texture ID of this texture, or zero if no texture is loaded.</summary>
  public int TextureId { get { return texture; } }

  /// <summary>Gets the height of the texture in pixels.</summary>
  /// <remarks>The size of the image may differ from the size of the texture because most OpenGL implementations
  /// require that texture dimensions be powers of two.
  /// </remarks>
  public int TexHeight { get { return size.Height; } }
  /// <summary>Gets the size of the texture in pixels.</summary>
  /// <remarks>The size of the image may differ from the size of the texture because most OpenGL implementations
  /// require that texture dimensions be powers of two.
  /// </remarks>
  public Size TexSize { get { return size; } }
  /// <summary>Gets the width of the texture in pixels.</summary>
  /// <remarks>The size of the image may differ from the size of the texture because most OpenGL implementations
  /// require that texture dimensions be powers of two.
  /// </remarks>
  public int TexWidth { get { return size.Width; } }

  /// <summary>Binds this texture as the current OpenGL texture.</summary>
  /// <remarks>This method calls <see cref="GL.glBindTexture"/> to bind this texture.</remarks>
  public void Bind() { AssertInit(); GL.glBindTexture(GL.GL_TEXTURE_2D, texture); }

  /// <summary>Loads this texture from an image file on disk.</summary>
  /// <param name="filename">The path to the image file.</param>
  /// <returns>True if the texture could be loaded and false otherwise.</returns>
  public bool Load(string filename) { return Load(0, 0, 0, new Surface(filename)); }
  /// <summary>Loads this texture from a stream.</summary>
  /// <param name="stream">The stream from which to load image data. The stream should be seekable, with its entire
  /// set of data devoted to the image.
  /// </param>
  /// <returns>True if the texture could be loaded and false otherwise.</returns>
  public bool Load(System.IO.Stream stream) { return Load(0, 0, 0, new Surface(stream)); }
  /// <summary>Loads this this texture from a surface.</summary>
  /// <param name="surface">The surface from which the texture will be loaded.</param>
  /// <returns>True if the texture could be loaded and false otherwise.</returns>
  public bool Load(Surface surface) { return Load(0, 0, 0, surface); }
  /// <summary>Loads this this texture from a surface.</summary>
  /// <param name="internalFormat">The internal format OpenGL should use for the texture.</param>
  /// <param name="surface">The surface from which the texture will be loaded.</param>
  /// <returns>True if the texture could be loaded and false otherwise.</returns>
  public bool Load(int internalFormat, Surface surface) { return Load(internalFormat, 0, 0, surface); }
  /// <summary>Loads this this texture from a surface.</summary>
  /// <param name="internalFormat">The internal format OpenGL should use for the texture.</param>
  /// <param name="level">The mipmap level to upload the texture into.</param>
  /// <param name="surface">The surface from which the texture will be loaded.</param>
  /// <returns>True if the texture could be loaded and false otherwise.</returns>
  public bool Load(int internalFormat, int level, Surface surface) { return Load(internalFormat, level, 0, surface); }
  /// <summary>Loads this this texture from a surface.</summary>
  /// <param name="internalFormat">The internal format OpenGL should use for the texture.</param>
  /// <param name="level">The mipmap level to upload the texture into.</param>
  /// <param name="border">The width of the border.</param>
  /// <param name="surface">The surface from which the texture will be loaded.</param>
  /// <returns>True if the texture could be loaded and false otherwise.</returns>
  public bool Load(int internalFormat, int level, int border, Surface surface)
  { Unload();

    int tex;
    GL.glGenTexture(out tex);
    if(tex == 0) throw new NoMoreTexturesException();

    int old = GL.glGetIntegerv(GL.GL_TEXTURE_BINDING_2D);
    GL.glBindTexture(GL.GL_TEXTURE_2D, tex);
    try
    { if(!OpenGL.TexImage2D(internalFormat, level, border, surface, out size))
      { GL.glDeleteTexture(tex);
        return false;
      }
      ImgSize = surface.Size;
      texture = tex;
      return true;
    }
    finally { GL.glBindTexture(GL.GL_TEXTURE_2D, old); }
  }

  /// <summary>Unloads the texture from video memory.</summary>
  /// <remarks>If no texture is loaded, this method will do nothing.</remarks>
  public void Unload()
  { 
    if(texture != 0)
    {
      if(GL.glGetIntegerv(GL.GL_TEXTURE_BINDING_2D) == texture) GL.glBindTexture(GL.GL_TEXTURE_2D, 0);
      GL.glDeleteTexture(texture);
      texture = 0;
    }
  }

  void AssertInit() { if(texture==0) throw new InvalidOperationException("Texture has not been initialized yet"); }
  void Dispose(bool finalizing) { Unload(); }

  double texCoordWidth, texCoordHeight, pixelToTexX, pixelToTexY;
  Size imgSize, size;
  int texture;

  Size IGuiImage.Size
  {
    get { return ImgSize; }
  }

  void IGuiImage.Draw(Rectangle srcRect, IGuiRenderTarget target, Rectangle destRect)
  {
    bool blendEnabled = GL.glIsEnabled(GL.GL_BLEND), textureEnabled = GL.glIsEnabled(GL.GL_TEXTURE_2D);
    if(!blendEnabled) GL.glEnable(GL.GL_BLEND);
    if(!textureEnabled) GL.glEnable(GL.GL_TEXTURE_2D);
    const int DesiredSourceBlend = GL.GL_SRC_ALPHA, DesiredDestBlend = GL.GL_ONE_MINUS_SRC_ALPHA;
    GL.glBlendFunc(DesiredSourceBlend, DesiredDestBlend);

    double tx1 = srcRect.X*PixelToTexCoordX, ty1 = srcRect.Y*PixelToTexCoordY;
    double tx2 = srcRect.Right * PixelToTexCoordX, ty2 = srcRect.Bottom * PixelToTexCoordY;

    Bind();
    GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_NEAREST);
    GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_NEAREST);

    GL.glColor3f(1, 1, 1);
    GL.glBegin(GL.GL_QUADS);
    GL.glTexCoord2d(tx1, ty1);
    GL.glVertex2i(destRect.X, destRect.Y);
    GL.glTexCoord2d(tx2, ty1);
    GL.glVertex2i(destRect.Right, destRect.Y);
    GL.glTexCoord2d(tx2, ty2);
    GL.glVertex2i(destRect.Right, destRect.Bottom);
    GL.glTexCoord2d(tx1, ty2);
    GL.glVertex2i(destRect.X, destRect.Bottom);
    GL.glEnd();

    if(!textureEnabled) GL.glDisable(GL.GL_TEXTURE_2D);
    if(!blendEnabled) GL.glDisable(GL.GL_BLEND);
  }
}
#endregion

} // namespace GameLib.Video
