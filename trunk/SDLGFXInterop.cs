using System;
using System.IO;
using System.Runtime.InteropServices;
using GameLib.Interop.SDL;

namespace GameLib.Interop.SDLGFX
{

[System.Security.SuppressUnmanagedCodeSecurity()]
internal class GFX
{
  #region Pixels
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int pixelColor(SDL.SDL.Surface* dst, short x, short y, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int pixelRGBA(SDL.SDL.Surface* dst, short x, short y, byte r, byte g, byte b, byte a);
  #endregion
  
  #region Lines
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int hlineColor(SDL.SDL.Surface* dst, short x1, short x2, short y, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int hlineRGBA(SDL.SDL.Surface* dst, short x1, short x2, short y, byte r, byte g, byte b, byte a);

  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int vlineColor(SDL.SDL.Surface* dst, short x, short y1, short y2, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int vlineRGBA(SDL.SDL.Surface* dst, short x, short y1, short y2, byte r, byte g, byte b, byte a);

  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int lineColor(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int lineRGBA(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, byte r, byte g, byte b, byte a);

  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int aalineColor(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int aalineRGBA(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, byte r, byte g, byte b, byte a);
  #endregion

  #region Rectangles
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int rectangleColor(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int rectangleRGBA(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, byte r, byte g, byte b, byte a);

  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int boxColor(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int boxRGBA(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, byte r, byte g, byte b, byte a);
  #endregion
  
  #region Circles
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int circleColor(SDL.SDL.Surface* dst, short x, short y, short r, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int circleRGBA(SDL.SDL.Surface* dst, short x, short y, short rad, byte r, byte g, byte b, byte a);

  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int aacircleColor(SDL.SDL.Surface* dst, short x, short y, short r, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int aacircleRGBA(SDL.SDL.Surface* dst, short x, short y, short rad, byte r, byte g, byte b, byte a);

  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int filledCircleColor(SDL.SDL.Surface* dst, short x, short y, short r, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int filledCircleRGBA(SDL.SDL.Surface* dst, short x, short y, short rad, byte r, byte g, byte b, byte a);
  #endregion
  
  #region Ellipses
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int ellipseColor(SDL.SDL.Surface* dst, short x, short y, short rx, short ry, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int ellipseRGBA(SDL.SDL.Surface* dst, short x, short y, short rx, short ry, byte r, byte g, byte b, byte a);

  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int aaellipseColor(SDL.SDL.Surface* dst, short xc, short yc, short rx, short ry, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int aaellipseRGBA(SDL.SDL.Surface* dst, short x, short y, short rx, short ry, byte r, byte g, byte b, byte a);

  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int filledEllipseColor(SDL.SDL.Surface* dst, short x, short y, short rx, short ry, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int filledEllipseRGBA(SDL.SDL.Surface* dst, short x, short y, short rx, short ry, byte r, byte g, byte b, byte a);
  #endregion

  #region Pies
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int filledpieColor(SDL.SDL.Surface* dst, short x, short y, short rad, short start, short end, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int filledpieRGBA(SDL.SDL.Surface* dst, short x, short y, short rad, short start, short end, byte r, byte g, byte b, byte a);
  #endregion
  
  #region Triangles
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int trigonColor(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, short x3, short y3, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int trigonRGBA(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, short x3, short y3, byte r, byte g, byte b, byte a);

  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int aatrigonColor(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, short x3, short y3, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int aatrigonRGBA(SDL.SDL.Surface* dst,  short x1, short y1, short x2, short y2, short x3, short y3, byte r, byte g, byte b, byte a);

  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int filledTrigonColor(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, short x3, short y3, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int filledTrigonRGBA(SDL.SDL.Surface* dst, short x1, short y1, short x2, short y2, short x3, short y3, byte r, byte g, byte b, byte a);
  #endregion
  
  #region Polygons
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int polygonColor(SDL.SDL.Surface* dst, short* vx, short* vy, int n, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int polygonRGBA(SDL.SDL.Surface* dst, short* vx, short* vy, int n, byte r, byte g, byte b, byte a);

  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int aapolygonColor(SDL.SDL.Surface* dst, short* vx, short* vy, int n, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int aapolygonRGBA(SDL.SDL.Surface* dst, short* vx, short* vy, int n, byte r, byte g, byte b, byte a);

  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int filledPolygonColor(SDL.SDL.Surface* dst, short* vx, short* vy, int n, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int filledPolygonRGBA(SDL.SDL.Surface* dst, short* vx, short* vy, int n, byte r, byte g, byte b, byte a);
  #endregion
  
  #region Curves
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int bezierColor(SDL.SDL.Surface* dst, short* vx, short* vy, int n, int steps, uint color);
  [DllImport(Config.SDLGFXImportPath, CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern int bezierRGBA(SDL.SDL.Surface* dst, short* vx, short* vy, int n, int steps, byte r, byte g, byte b, byte a);
  #endregion
}

} // namespace GameLib.Interop.SDLGFX