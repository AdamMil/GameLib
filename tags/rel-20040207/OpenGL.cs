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
using GameLib.Interop;
using GameLib.Interop.OpenGL;

namespace GameLib.Video
{

// TODO: add more methods
public sealed class OpenGL
{ private OpenGL() { }

  public static System.Collections.ICollection Extensions
  { get { if(extensions==null) InitExtensions(); return extensions.Keys; }
  }

  public static bool UseNPOTExtension { get { return useNPOT; } set { useNPOT=value; } }
  
  public static bool HasExtension(string name)
  { if(extensions==null) InitExtensions();
    return extensions.Contains(name);
  }

  #region TexImage2D
  public static void TexImage2D(Surface surface) { TexImage2D(0, GL.GL_TEXTURE_2D, 0, 0, surface); }
  public static void TexImage2D(uint internalFormat, Surface surface)
  { TexImage2D(internalFormat, GL.GL_TEXTURE_2D, 0, 0, surface);
  }
  public static void TexImage2D(uint internalFormat, uint target, Surface surface)
  { TexImage2D(internalFormat, target, 0, 0, surface);
  }
  public static void TexImage2D(uint internalFormat, uint target, int level, Surface surface)
  { TexImage2D(internalFormat, target, level, 0, surface);
  }
  public static void TexImage2D(uint internalFormat, uint target, int level, int border, Surface surface)
  { PixelFormat pf = surface.Format;
    uint format=0;
    int nwidth, nheight, awidth=surface.Width-border*2, aheight=surface.Height-border*2;
    if(useNPOT && HasExtension("GL_ARB_texture_non_power_of_two")) { nwidth=awidth; nheight=aheight; }
    else
    { nwidth=nheight=1;
      while(nwidth<awidth) nwidth<<=1;
      while(nheight<aheight) nheight<<=1;
    }

    if(awidth==nwidth && aheight==nheight && surface.Pitch==surface.Width*surface.Depth/8)
    { if(surface.Depth==24)
      { if(pf.RedMask==0xFF && pf.GreenMask==0xFF00 && pf.BlueMask==0xFF0000)
          #if BIGENDIAN
          format = GL.GL_BGR_EXT;
          #else
          format = GL.GL_RGB;
          #endif
        else if(pf.RedMask==0xFF0000 && pf.GreenMask==0xFF00 && pf.BlueMask==0xFF)
          #if BIGENDIAN
          format = GL.GL_RGB;
          #else
          format = GL.GL_BGR_EXT;
          #endif
      }
      else if(surface.Depth==32 && pf.AlphaMask!=0)
      { 
        #if BIGENDIAN
        if(pf.RedMask==0xFF000000 && pf.GreenMask==0xFF0000 && pf.BlueMask==0xFF00 && pf.AlphaMask==0xFF)
          format = GL.GL_RGBA;
        else if(pf.RedMask==0xFF00 && pf.GreenMask==0xFF0000 && pf.BlueMask==0xFF000000 && pf.AlphaMask==0xFF)
          format = GL.GL_BGRA_EXT;
        #else
        if(pf.RedMask==0xFF && pf.GreenMask==0xFF00 && pf.BlueMask==0xFF0000 && pf.AlphaMask==0xFF000000)
          format = GL.GL_RGBA;
        else if(pf.RedMask==0xFF0000 && pf.GreenMask==0xFF00 && pf.BlueMask==0xFF && pf.AlphaMask==0xFF000000)
          format = GL.GL_BGRA_EXT;
        #endif
      }
      if(format!=0)
      { surface.Lock();
        unsafe
        { if(internalFormat==0)
            internalFormat = format==GL.GL_RGBA || format==GL.GL_BGRA_EXT ? GL.GL_RGBA8 : GL.GL_RGB8;
          GL.glTexImage2D(target, level, internalFormat, surface.Width, surface.Height, border, format,
                          GL.GL_UNSIGNED_BYTE, surface.Data);
        }
        surface.Unlock();
        return;
      }
    }
    
    pf = new PixelFormat(surface.Depth==32 && surface.UsingAlpha || surface.UsingKey ? 32 : 24);
    #if BIGENDIAN
    pf.RedMask=0xFF000000; pf.GreenMask=0xFF0000; pf.BlueMask=0xFF00; pf.AlphaMask=(pf.Depth==32 ? 0xFF : 0);
    #else
    pf.RedMask=0xFF; pf.GreenMask=0xFF00; pf.BlueMask=0xFF0000; pf.AlphaMask=(pf.Depth==32 ? 0xFF000000 : 0);
    #endif

    Surface temp = new Surface(nwidth+border*2, nheight+border*2, pf, pf.Depth==32 ? SurfaceFlag.SrcAlpha : 0);
    bool ua = surface.UsingAlpha;
    if(surface.Format.AlphaMask!=0 || surface.Alpha==AlphaLevel.Opaque) surface.UsingAlpha=false;
    surface.Blit(temp, border, border);
    surface.UsingAlpha = ua;
    temp.Lock();
    try
    { if(internalFormat==0) internalFormat = pf.Depth==32 ? GL.GL_RGBA : GL.GL_RGB;
      if(temp.Pitch==temp.Width*temp.Depth/8)
        unsafe
        { GL.glTexImage2D(target, level, internalFormat, temp.Width, temp.Height, border,
                          pf.Depth==32 ? GL.GL_RGBA : GL.GL_RGB, GL.GL_UNSIGNED_BYTE, temp.Data);
        }
      else
      { int line = temp.Width*temp.Depth/8;
        byte[] arr = new byte[temp.Height*line];
        unsafe
        { fixed(byte* arrp=arr)
          { byte* dest=arrp, src=(byte*)temp.Data;
            for(int y=0; y<temp.Height; dest+=line,src+=temp.Pitch,y++) Unsafe.Copy(dest, src, line);
            GL.glTexImage2D(target, level, internalFormat, temp.Width, temp.Height, border,
                            pf.Depth==32 ? GL.GL_RGBA : GL.GL_RGB, GL.GL_UNSIGNED_BYTE, arrp);
          }
        }
      }
    }
    finally { temp.Unlock(); temp.Dispose(); }
  }
  #endregion
  
  static void InitExtensions()
  { string str = GL.glGetString(GL.GL_EXTENSIONS);
    if(str==null) throw new InvalidOperationException("OpenGL not initialized yet!");
    string[] exts = str.Split(' ');
    extensions = new System.Collections.Hashtable(exts.Length);
    foreach(string s in exts) extensions[s] = null;
  }

  static System.Collections.Hashtable extensions;
  static bool useNPOT=true;
}

} // namespace GameLib.Video
