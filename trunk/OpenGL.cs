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
using GameLib.Interop;
using GameLib.Interop.OpenGL;

namespace GameLib.Video
{

#region OpenGL class
// TODO: add more methods
public sealed class OpenGL
{ private OpenGL() { }

  public static System.Collections.ICollection Extensions
  { get { if(extensions==null) InitExtensions(); return extensions.Keys; }
  }

  public static int VersionMajor { get { if(major==0) InitVersion(); return major; } }
  public static int VersionMinor { get { if(minor==0) InitVersion(); return minor; } }

  public static bool UseNPOTExtension { get { return useNPOT; } set { useNPOT=value; } }
  
  public static bool HasExtension(string name)
  { if(extensions==null) InitExtensions();
    return extensions.Contains(name);
  }

  #region TexImage2D
  public static bool TexImage2D(Surface surface)
  { Size dummy;
    return TexImage2D(0, 0, 0, surface, out dummy);
  }
  public static bool TexImage2D(Surface surface, out Size texSize)
  { return TexImage2D(0, 0, 0, surface, out texSize);
  }
  public static bool TexImage2D(uint internalFormat, Surface surface, out Size texSize)
  { return TexImage2D(internalFormat, 0, 0, surface, out texSize);
  }
  public static bool TexImage2D(uint internalFormat, int level, Surface surface, out Size texSize)
  { return TexImage2D(internalFormat, level, 0, surface, out texSize);
  }
  public static bool TexImage2D(uint internalFormat, int level, int border, Surface surface, out Size texSize)
  { PixelFormat pf = surface.Format;
    uint format=0;
    int nwidth, nheight, awidth=surface.Width-border*2, aheight=surface.Height-border*2;
    bool usingAlpha = surface.Depth==32 && surface.UsingAlpha || surface.UsingKey;
    if(useNPOT && HasExtension("GL_ARB_texture_non_power_of_two")) { nwidth=awidth; nheight=aheight; }
    else
    { nwidth=nheight=1;
      while(nwidth<awidth) nwidth<<=1;
      while(nheight<aheight) nheight<<=1;
    }

    if(internalFormat==0) internalFormat = usingAlpha ? GL.GL_RGBA : GL.GL_RGB;
    if(!WillTextureFit(internalFormat, level, border, nwidth, nheight))
    { texSize = new Size(0, 0);
      return false;
    }
    texSize = new Size(nwidth, nheight);

    if(awidth==nwidth && aheight==nheight && !surface.UsingKey && surface.Pitch==surface.Width*surface.Depth/8)
    { uint type = GL.GL_UNSIGNED_BYTE;
      bool hasPPE = VersionMinor>=2 && major==1 || HasExtension("GL_EXT_packed_pixels") || major>1;
      bool hasBGR = VersionMinor>=2 && major==1 || HasExtension("EXT_bgra") || major>1;
      if(surface.Depth==16 && hasPPE)
      { // TODO: make this work with big-endian machines
        if(pf.RedMask==0xF800 && pf.GreenMask==0x7E0 && pf.BlueMask==0x1F)
        { format = GL.GL_RGB;
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
          format = hasBGR ? GL.GL_BGR_EXT : 0;
        #else
        if(pf.RedMask==0xFF && pf.GreenMask==0xFF00 && pf.BlueMask==0xFF0000 && pf.AlphaMask==0xFF000000)
          format = GL.GL_RGBA;
        else if(pf.RedMask==0xFF0000 && pf.GreenMask==0xFF00 && pf.BlueMask==0xFF && pf.AlphaMask==0xFF000000)
          format = hasBGR ? GL.GL_BGR_EXT : 0;
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
    
    pf = new PixelFormat(usingAlpha ? 32 : 24);
    #if BIGENDIAN
    pf.RedMask=0xFF000000; pf.GreenMask=0xFF0000; pf.BlueMask=0xFF00; pf.AlphaMask=(pf.Depth==32 ? 0xFF : 0);
    #else
    pf.RedMask=0xFF; pf.GreenMask=0xFF00; pf.BlueMask=0xFF0000; pf.AlphaMask=(pf.Depth==32 ? 0xFF000000 : 0);
    #endif

    Surface temp = new Surface(nwidth+border*2, nheight+border*2, pf, pf.Depth==32 ? SurfaceFlag.SrcAlpha : 0);
    bool oua = surface.UsingAlpha;
    if(surface.Format.AlphaMask!=0 || surface.Alpha==AlphaLevel.Opaque) surface.UsingAlpha=false;
    surface.Blit(temp, border, border);
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
            for(int y=0; y<temp.Height; dest+=line,src+=temp.Pitch,y++) Unsafe.Copy(dest, src, line);
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
  public static bool WillTextureFit(uint internalFormat, int width, int height)
  { return WillTextureFit(internalFormat, 0, 0, width, height);
  }
  public static bool WillTextureFit(uint internalFormat, int level, int width, int height)
  { return WillTextureFit(internalFormat, level, 0, width, height);
  }
  public static bool WillTextureFit(uint internalFormat, int level, int border, int width, int height)
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
    if(str==null) throw new InvalidOperationException("OpenGL not initialized yet!");
    string[] exts = str.Split(' ');
    extensions = new System.Collections.Hashtable(exts.Length);
    foreach(string s in exts) extensions[s] = null;
  }

  static void InitVersion()
  { string str = GL.glGetString(GL.GL_VERSION);
    if(str==null) throw new InvalidOperationException("OpenGL not initialized yet!");
    int pos = str.IndexOf(' ');
    string[] bits = (pos==-1 ? str : str.Substring(0, pos)).Split('.');
    major = int.Parse(bits[0]);
    minor = int.Parse(bits[1]);
  }

  static System.Collections.Hashtable extensions;
  static int major, minor;
  static bool useNPOT=true;
}
#endregion

#region GLTexture2D
// TODO: add support for mipmapping
public class GLTexture2D : IDisposable
{ public GLTexture2D() { }
  public GLTexture2D(string filename) : this(0, 0, 0, new Surface(filename)) { }
  public GLTexture2D(System.IO.Stream stream) : this(0, 0, 0, new Surface(stream)) { }
  public GLTexture2D(Surface surface) : this(0, 0, 0, surface) { }
  public GLTexture2D(uint internalFormat, Surface surface) : this(internalFormat, 0, 0, surface) { }
  public GLTexture2D(uint internalFormat, int level, Surface surface) : this(internalFormat, level, 0, surface) { }
  public GLTexture2D(uint internalFormat, int level, int border, Surface surface)
  { if(!Load(internalFormat, level, border, surface)) throw new OutOfTextureMemoryException();
  }
  ~GLTexture2D() { Dispose(true); }

  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  public int ImgHeight
  { get { return imgSize.Height; }
    set
    { if(value<0) throw new ArgumentOutOfRangeException("ImgHeight", value, "must not be negative");
      imgSize.Height = value;
    }
  }

  public Size ImgSize
  { get { return imgSize; }
    set
    { if(value.Width<0 || value.Height<0)
        throw new ArgumentOutOfRangeException("ImgSize", value, "coordinates must not be negative");
      imgSize = value;
    }
  }

  public int ImgWidth
  { get { return imgSize.Width; }
    set
    { if(value<0) throw new ArgumentOutOfRangeException("ImgWidth", value, "must not be negative");
      imgSize.Width = value;
    }
  }
  
  public bool Initialized { get { return texture!=0; } }

  public uint TextureID { get { return texture; } }

  public int TexHeight { get { return size.Height; } }
  public Size TexSize { get { return size; } }
  public int TexWidth { get { return size.Width; } }
  
  public void Bind() { AssertInit(); GL.glBindTexture(GL.GL_TEXTURE_2D, texture); }
  
  public bool Load(string filename) { return Load(0, 0, 0, new Surface(filename)); }
  public bool Load(System.IO.Stream stream) { return Load(0, 0, 0, new Surface(stream)); }
  public bool Load(Surface surface) { return Load(0, 0, 0, surface); }
  public bool Load(uint internalFormat, Surface surface) { return Load(internalFormat, 0, 0, surface); }
  public bool Load(uint internalFormat, int level, Surface surface) { return Load(internalFormat, level, 0, surface); }
  public bool Load(uint internalFormat, int level, int border, Surface surface)
  { Unload();
    uint tex;
    GL.glGenTexture(out tex);
    if(tex==0) throw new NoMoreTexturesException();

    GL.glBindTexture(GL.GL_TEXTURE_2D, tex);
    if(!OpenGL.TexImage2D(internalFormat, level, border, surface, out size))
    { GL.glDeleteTexture(tex);
      return false;
    }
    imgSize = surface.Size; texture = tex;
    return true;
  }

  public void Unload()
  { if(texture!=0)
    { GL.glDeleteTexture(texture);
      texture = 0;
    }
  }

  void AssertInit() { if(texture==0) throw new InvalidOperationException("Texture has not been initialized yet"); }
  void Dispose(bool destructing)
  { Unload();
  }

  Size imgSize, size;
  uint texture;
}
#endregion

} // namespace GameLib.Video
