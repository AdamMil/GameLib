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
using System.IO;
using System.Runtime.InteropServices;
using GameLib.Interop.SDL;

namespace GameLib.Interop.SDLGFX
{

[System.Security.SuppressUnmanagedCodeSecurity()]
internal static class GFX
{
  #region Pixels
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int pixelRGBA(SDL.SDL.Surface* dst, short x, short y, byte r, byte g, byte b, byte a);
  #endregion

  #region Lines
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int hlineRGBA(SDL.SDL.Surface* dst, short x1, short x2, short y, byte r, byte g, byte b, byte a);
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int vlineRGBA(SDL.SDL.Surface* dst, short x, short y1, short y2, byte r, byte g, byte b, byte a);
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int lineRGBA(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, byte r, byte g, byte b, byte a);
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int aalineRGBA(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, byte r, byte g, byte b, byte a);
  #endregion

  #region Rectangles
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int rectangleRGBA(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, byte r, byte g, byte b, byte a);
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int boxRGBA(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, byte r, byte g, byte b, byte a);
  #endregion

  #region Circles
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int circleRGBA(SDL.SDL.Surface* dst, short x, short y, short rad, byte r, byte g, byte b, byte a);
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int aacircleRGBA(SDL.SDL.Surface* dst, short x, short y, short rad, byte r, byte g, byte b, byte a);
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int filledCircleRGBA(SDL.SDL.Surface* dst, short x, short y, short rad, byte r, byte g, byte b, byte a);
  #endregion

  #region Ellipses
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int ellipseRGBA(SDL.SDL.Surface* dst, short x, short y, short rx, short ry, byte r, byte g, byte b, byte a);
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int aaellipseRGBA(SDL.SDL.Surface* dst, short x, short y, short rx, short ry, byte r, byte g, byte b, byte a);
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int filledEllipseRGBA(SDL.SDL.Surface* dst, short x, short y, short rx, short ry, byte r, byte g, byte b, byte a);
  #endregion

  #region Pies
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int filledpieRGBA(SDL.SDL.Surface* dst, short x, short y, short rad, short start, short end, byte r, byte g, byte b, byte a);
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int pieRGBA(SDL.SDL.Surface* dst, short x, short y, short rad, short start, short end, byte r, byte g, byte b, byte a);
  #endregion

  #region Triangles
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int trigonRGBA(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, short x3, short y3, byte r, byte g, byte b, byte a);
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int aatrigonRGBA(SDL.SDL.Surface* dst,  short x1, short y1, short x2, short y2, short x3, short y3, byte r, byte g, byte b, byte a);
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int filledTrigonRGBA(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, short x3, short y3, byte r, byte g, byte b, byte a);
  #endregion

  #region Polygons
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int polygonRGBA(SDL.SDL.Surface* dst, short* vx, short* vy, int n, byte r, byte g, byte b, byte a);
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int aapolygonRGBA(SDL.SDL.Surface* dst, short* vx, short* vy, int n, byte r, byte g, byte b, byte a);
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int filledPolygonRGBA(SDL.SDL.Surface* dst, short* vx, short* vy, int n, byte r, byte g, byte b, byte a);
  #endregion

  #region Curves
  [DllImport(Config.SdlGfxImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int bezierRGBA(SDL.SDL.Surface* dst, short* vx, short* vy, int n, int steps, byte r, byte g, byte b, byte a);
  #endregion
}

} // namespace GameLib.Interop.SDLGFX