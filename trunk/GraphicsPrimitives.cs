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
using GameLib.Interop.SDLGFX;

namespace GameLib.Video
{

public unsafe class Primitives
{ private Primitives() { }

  #region Pixels
  /// <summary>Writes a pixel on the destination surface, with optional alpha blending.</summary>
  /// <param name="dest">The surface to draw into.</param>
  /// <param name="pt">The coordinate of the pixel to change.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the color will be
  /// alpha blended over the existing pixel.
  /// </param>
  public static void Pixel(Surface dest, Point pt, Color color) { Pixel(dest, pt.X, pt.Y, color); }
  /// <summary>Writes a pixel on the destination surface, with optional alpha blending.</summary>
  /// <param name="dest">The surface to draw into.</param>
  /// <param name="x">The X coordinate of the pixel to change.</param>
  /// <param name="y">The Y coordinate of the pixel to change.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the color will be
  /// alpha blended over the existing pixel.
  /// </param>
  public static void Pixel(Surface dest, int x, int y, Color color)
  { Check(GFX.pixelRGBA(dest.surface, (short)x, (short)y, color.R, color.G, color.B, color.A));
  }
  /// <summary>Writes a pixel on the destination surface, with optional alpha blending.</summary>
  /// <param name="dest">The surface to draw into.</param>
  /// <param name="pt">The coordinate of the pixel to change.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the color
  /// will be alpha blended over the existing pixel.
  /// </param>
  public static void Pixel(Surface dest, Point pt, Color color, byte alpha) { Pixel(dest, pt.X, pt.Y, color, alpha); }
  /// <summary>Writes a pixel on the destination surface, with optional alpha blending.</summary>
  /// <param name="dest">The surface to draw into.</param>
  /// <param name="x">The X coordinate of the pixel to change.</param>
  /// <param name="y">The Y coordinate of the pixel to change.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the color
  /// will be alpha blended over the existing pixel.
  /// </param>
  public static void Pixel(Surface dest, int x, int y, Color color, byte alpha)
  { Check(GFX.pixelRGBA(dest.surface, (short)x, (short)y, color.R, color.G, color.B, alpha));
  }
  #endregion

  #region Lines
  /// <summary>Draws a horizontal line, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the line will be drawn.</param>
  /// <param name="x1">The starting X coordinate of the line.</param>
  /// <param name="x2">The ending X coordinate of the line.</param>
  /// <param name="y">The row at which the line will be drawn.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the line will be
  /// alpha blended onto the surface.
  /// </param>
  public static void HLine(Surface dest, int x1, int x2, int y, Color color)
  { Check(GFX.hlineRGBA(dest.surface, (short)x1, (short)x2, (short)y, color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws a horizontal line, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the line will be drawn.</param>
  /// <param name="x1">The starting X coordinate of the line.</param>
  /// <param name="x2">The ending X coordinate of the line.</param>
  /// <param name="y">The row at which the line will be drawn.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the line
  /// will be alpha blended onto the surface.
  /// </param>
  public static void HLine(Surface dest, int x1, int x2, int y, Color color, byte alpha)
  { Check(GFX.hlineRGBA(dest.surface, (short)x1, (short)x2, (short)y, color.R, color.G, color.B, alpha));
  }

  /// <summary>Draws a vertical line, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the line will be drawn.</param>
  /// <param name="x">The column at which the line will be drawn.</param>
  /// <param name="y1">The starting Y coordinate of the line.</param>
  /// <param name="y2">The ending Y coordinate of the line.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the line will be
  /// alpha blended onto the surface.
  /// </param>
  public static void VLine(Surface dest, int x, int y1, int y2, Color color)
  { Check(GFX.vlineRGBA(dest.surface, (short)x, (short)y1, (short)y2, color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws a vertical line, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the line will be drawn.</param>
  /// <param name="x">The column at which the line will be drawn.</param>
  /// <param name="y1">The starting Y coordinate of the line.</param>
  /// <param name="y2">The ending Y coordinate of the line.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the line
  /// will be alpha blended onto the surface.
  /// </param>
  public static void VLine(Surface dest, int x, int y1, int y2, Color color, byte alpha)
  { Check(GFX.vlineRGBA(dest.surface, (short)x, (short)y1, (short)y2, color.R, color.G, color.B, alpha));
  }

  /// <summary>Draws a line, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the line will be drawn.</param>
  /// <param name="p1">The starting point of the line.</param>
  /// <param name="p2">The ending point of the line.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the line will be
  /// alpha blended onto the surface.
  /// </param>
  public static void Line(Surface dest, Point p1, Point p2, Color color)
  { Line(dest, p1.X, p1.Y, p2.X, p2.Y, color);
  }
  /// <summary>Draws a line, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the line will be drawn.</param>
  /// <param name="p1">The starting point of the line.</param>
  /// <param name="p2">The ending point of the line.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the line
  /// will be alpha blended onto the surface.
  /// </param>
  public static void Line(Surface dest, Point p1, Point p2, Color color, byte alpha)
  { Line(dest, p1.X, p1.Y, p2.X, p2.Y, color, alpha);
  }
  /// <summary>Draws a line, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the line will be drawn.</param>
  /// <param name="x1">The X coordinate of the starting point of the line.</param>
  /// <param name="y1">The Y coordinate of the starting point of the line.</param>
  /// <param name="x2">The X coordinate of the ending point of the line.</param>
  /// <param name="y2">The Y coordinate of the ending point of the line.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the line will be
  /// alpha blended onto the surface.
  /// </param>
  public static void Line(Surface dest, int x1, int y1, int x2, int y2, Color color)
  { Check(GFX.lineRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws a line, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the line will be drawn.</param>
  /// <param name="x1">The X coordinate of the starting point of the line.</param>
  /// <param name="y1">The Y coordinate of the starting point of the line.</param>
  /// <param name="x2">The X coordinate of the ending point of the line.</param>
  /// <param name="y2">The Y coordinate of the ending point of the line.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the line
  /// will be alpha blended onto the surface.
  /// </param>
  public static void Line(Surface dest, int x1, int y1, int x2, int y2, Color color, byte alpha)
  { Check(GFX.lineRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, alpha));
  }

  /// <summary>Draws an antialiased line, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the line will be drawn.</param>
  /// <param name="p1">The starting point of the line.</param>
  /// <param name="p2">The ending point of the line.</param>
  /// <param name="color">The base color of the line. Surrounding pixels will be blended with this color to provide
  /// antialiasing. If the color contains a non-opaque alpha level, the line will be alpha blended onto the surface.
  /// </param>
  public static void LineAA(Surface dest, Point p1, Point p2, Color color)
  { LineAA(dest, p1.X, p1.Y, p2.X, p2.Y, color);
  }
  /// <summary>Draws an antialiased line, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the line will be drawn.</param>
  /// <param name="p1">The starting point of the line.</param>
  /// <param name="p2">The ending point of the line.</param>
  /// <param name="color">The base color of the line. Surrounding pixels will be blended with this color to provide
  /// antialiasing. The alpha component of this color is ignored.
  /// </param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the line
  /// will be alpha blended onto the surface.
  /// </param>
  public static void LineAA(Surface dest, Point p1, Point p2, Color color, byte alpha)
  { LineAA(dest, p1.X, p1.Y, p2.X, p2.Y, color, alpha);
  }
  /// <summary>Draws an antialiased line, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the line will be drawn.</param>
  /// <param name="x1">The X coordinate of the starting point of the line.</param>
  /// <param name="y1">The Y coordinate of the starting point of the line.</param>
  /// <param name="x2">The X coordinate of the ending point of the line.</param>
  /// <param name="y2">The Y coordinate of the ending point of the line.</param>
  /// <param name="color">The base color of the line. Surrounding pixels will be blended with this color to provide
  /// antialiasing. If the color contains a non-opaque alpha level, the line will be alpha blended onto the surface.
  /// </param>
  public static void LineAA(Surface dest, int x1, int y1, int x2, int y2, Color color)
  { Check(GFX.aalineRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws an antialiased line, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the line will be drawn.</param>
  /// <param name="x1">The X coordinate of the starting point of the line.</param>
  /// <param name="y1">The Y coordinate of the starting point of the line.</param>
  /// <param name="x2">The X coordinate of the ending point of the line.</param>
  /// <param name="y2">The Y coordinate of the ending point of the line.</param>
  /// <param name="color">The base color of the line. Surrounding pixels will be blended with this color to provide
  /// antialiasing. The alpha component of this color is ignored.
  /// </param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the line
  /// will be alpha blended onto the surface.
  /// </param>
  public static void LineAA(Surface dest, int x1, int y1, int x2, int y2, Color color, byte alpha)
  { Check(GFX.aalineRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, alpha));
  }
  #endregion

  #region Rectangles
  /// <summary>Draws a hollow rectangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the rectangle will be drawn.</param>
  /// <param name="rect">A <see cref="Rectangle"/> describing the rectangle to draw.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the rectangle will be
  /// alpha blended onto the surface.
  /// </param>
  public static void Box(Surface dest, Rectangle rect, Color color)
  { Box(dest, rect.X, rect.Y, rect.Right-1, rect.Bottom-1, color);
  }
  /// <summary>Draws a hollow rectangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the rectangle will be drawn.</param>
  /// <param name="rect">A <see cref="Rectangle"/> describing the rectangle to draw.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the rectangle
  /// will be alpha blended onto the surface.
  /// </param>
  public static void Box(Surface dest, Rectangle rect, Color color, byte alpha)
  { Box(dest, rect.X, rect.Y, rect.Right-1, rect.Bottom-1, color, alpha);
  }
  /// <summary>Draws a hollow rectangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the rectangle will be drawn.</param>
  /// <param name="p1">A corner of the rectangle to draw.</param>
  /// <param name="p2">The opposite corner of the rectangle to draw.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the rectangle will be
  /// alpha blended onto the surface.
  /// </param>
  public static void Box(Surface dest, Point p1, Point p2, Color color) { Box(dest, p1.X, p1.Y, p2.X, p2.Y, color); }
  /// <summary>Draws a hollow rectangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the rectangle will be drawn.</param>
  /// <param name="p1">A corner of the rectangle to draw.</param>
  /// <param name="p2">The opposite corner of the rectangle to draw.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the rectangle
  /// will be alpha blended onto the surface.
  /// </param>
  public static void Box(Surface dest, Point p1, Point p2, Color color, byte alpha)
  { Box(dest, p1.X, p1.Y, p2.X, p2.Y, color, alpha);
  }
  /// <summary>Draws a hollow rectangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the rectangle will be drawn.</param>
  /// <param name="x1">The X coordinate of a corner of the rectangle to draw.</param>
  /// <param name="y1">The Y coordinate of a corner of the rectangle to draw.</param>
  /// <param name="x2">The X coordinate of the opposite corner of the rectangle to draw.</param>
  /// <param name="y2">The Y coordinate of the opposite corner of the rectangle to draw.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the rectangle will be
  /// alpha blended onto the surface.
  /// </param>
  public static void Box(Surface dest, int x1, int y1, int x2, int y2, Color color)
  { Check(GFX.rectangleRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2,
                            color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws a hollow rectangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the rectangle will be drawn.</param>
  /// <param name="x1">The X coordinate of a corner of the rectangle to draw.</param>
  /// <param name="y1">The Y coordinate of a corner of the rectangle to draw.</param>
  /// <param name="x2">The X coordinate of the opposite corner of the rectangle to draw.</param>
  /// <param name="y2">The Y coordinate of the opposite corner of the rectangle to draw.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the rectangle
  /// will be alpha blended onto the surface.
  /// </param>
  public static void Box(Surface dest, int x1, int y1, int x2, int y2, Color color, byte alpha)
  { Check(GFX.rectangleRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2,
                            color.R, color.G, color.B, alpha));
  }

  /// <summary>Draws a filled rectangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the rectangle will be drawn.</param>
  /// <param name="rect">A <see cref="Rectangle"/> describing the rectangle to draw.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the rectangle will be
  /// alpha blended onto the surface.
  /// </param>
  public static void FilledBox(Surface dest, Rectangle rect, Color color)
  { FilledBox(dest, rect.X, rect.Y, rect.Right-1, rect.Bottom-1, color);
  }
  /// <summary>Draws a filled rectangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the rectangle will be drawn.</param>
  /// <param name="rect">A <see cref="Rectangle"/> describing the rectangle to draw.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the rectangle
  /// will be alpha blended onto the surface.
  /// </param>
  public static void FilledBox(Surface dest, Rectangle rect, Color color, byte alpha)
  { FilledBox(dest, rect.X, rect.Y, rect.Right-1, rect.Bottom-1, color, alpha);
  }
  /// <summary>Draws a filled rectangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the rectangle will be drawn.</param>
  /// <param name="p1">A corner of the rectangle to draw.</param>
  /// <param name="p2">The opposite corner of the rectangle to draw.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the rectangle will be
  /// alpha blended onto the surface.
  /// </param>
  public static void FilledBox(Surface dest, Point p1, Point p2, Color color)
  { FilledBox(dest, p1.X, p1.Y, p2.X, p2.Y, color);
  }
  /// <summary>Draws a filled rectangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the rectangle will be drawn.</param>
  /// <param name="p1">A corner of the rectangle to draw.</param>
  /// <param name="p2">The opposite corner of the rectangle to draw.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the rectangle
  /// will be alpha blended onto the surface.
  /// </param>
  public static void FilledBox(Surface dest, Point p1, Point p2, Color color, byte alpha)
  { FilledBox(dest, p1.X, p1.Y, p2.X, p2.Y, color, alpha);
  }
  /// <summary>Draws a filled rectangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the rectangle will be drawn.</param>
  /// <param name="x1">The X coordinate of a corner of the rectangle to draw.</param>
  /// <param name="y1">The Y coordinate of a corner of the rectangle to draw.</param>
  /// <param name="x2">The X coordinate of the opposite corner of the rectangle to draw.</param>
  /// <param name="y2">The Y coordinate of the opposite corner of the rectangle to draw.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the rectangle will be
  /// alpha blended onto the surface.
  /// </param>
  public static void FilledBox(Surface dest, int x1, int y1, int x2, int y2, Color color)
  { Check(GFX.boxRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws a filled rectangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the rectangle will be drawn.</param>
  /// <param name="x1">The X coordinate of a corner of the rectangle to draw.</param>
  /// <param name="y1">The Y coordinate of a corner of the rectangle to draw.</param>
  /// <param name="x2">The X coordinate of the opposite corner of the rectangle to draw.</param>
  /// <param name="y2">The Y coordinate of the opposite corner of the rectangle to draw.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the rectangle
  /// will be alpha blended onto the surface.
  /// </param>
  public static void FilledBox(Surface dest, int x1, int y1, int x2, int y2, Color color, byte alpha)
  { Check(GFX.boxRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, alpha));
  }
  #endregion

  #region Circles
  /// <summary>Draws a hollow circle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the circle will be drawn.</param>
  /// <param name="pt">The circle's center point.</param>
  /// <param name="radius">The radius of the circle, in pixels.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the circle will be
  /// alpha blended onto the surface.
  /// </param>
  public static void Circle(Surface dest, Point pt, int radius, Color color)
  { Circle(dest, pt.X, pt.Y, radius, color);
  }
  /// <summary>Draws a hollow circle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the circle will be drawn.</param>
  /// <param name="pt">The circle's center point.</param>
  /// <param name="radius">The radius of the circle, in pixels.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the circle
  /// will be alpha blended onto the surface.
  /// </param>
  public static void Circle(Surface dest, Point pt, int radius, Color color, byte alpha)
  { Circle(dest, pt.X, pt.Y, radius, color, alpha);
  }
  /// <summary>Draws a hollow circle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the circle will be drawn.</param>
  /// <param name="x">The X coordinate of the circle's center point.</param>
  /// <param name="y">The Y coordinate of the circle's center point.</param>
  /// <param name="radius">The radius of the circle, in pixels.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the circle will be
  /// alpha blended onto the surface.
  /// </param>
  public static void Circle(Surface dest, int x, int y, int radius, Color color)
  { Check(GFX.circleRGBA(dest.surface, (short)x, (short)y, (short)radius, color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws a hollow circle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the circle will be drawn.</param>
  /// <param name="x">The X coordinate of the circle's center point.</param>
  /// <param name="y">The Y coordinate of the circle's center point.</param>
  /// <param name="radius">The radius of the circle, in pixels.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the circle
  /// will be alpha blended onto the surface.
  /// </param>
  public static void Circle(Surface dest, int x, int y, int radius, Color color, byte alpha)
  { Check(GFX.circleRGBA(dest.surface, (short)x, (short)y, (short)radius, color.R, color.G, color.B, alpha));
  }

  /// <summary>Draws an antialiased hollow circle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the circle will be drawn.</param>
  /// <param name="pt">The circle's center point.</param>
  /// <param name="radius">The radius of the circle, in pixels.</param>
  /// <param name="color">The base color of the circle. Surrounding pixels will be blended with this color to provide
  /// antialiasing. If the color contains a non-opaque alpha level, the circle will be alpha blended onto the surface.
  /// </param>
  public static void CircleAA(Surface dest, Point pt, int radius, Color color)
  { CircleAA(dest, pt.X, pt.Y, radius, color);
  }
  /// <summary>Draws an antialiased hollow circle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the circle will be drawn.</param>
  /// <param name="pt">The circle's center point.</param>
  /// <param name="radius">The radius of the circle, in pixels.</param>
  /// <param name="color">The base color of the circle. Surrounding pixels will be blended with this color to provide
  /// antialiasing. The alpha component of this color is ignored.
  /// </param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the circle
  /// will be alpha blended onto the surface.
  /// </param>
  public static void CircleAA(Surface dest, Point pt, int radius, Color color, byte alpha)
  { CircleAA(dest, pt.X, pt.Y, radius, color, alpha);
  }
  /// <summary>Draws an antialiased hollow circle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the circle will be drawn.</param>
  /// <param name="x">The X coordinate of the circle's center point.</param>
  /// <param name="y">The Y coordinate of the circle's center point.</param>
  /// <param name="radius">The radius of the circle, in pixels.</param>
  /// <param name="color">The base color of the circle. Surrounding pixels will be blended with this color to provide
  /// antialiasing. If the color contains a non-opaque alpha level, the circle will be alpha blended onto the surface.
  /// </param>
  public static void CircleAA(Surface dest, int x, int y, int radius, Color color)
  { Check(GFX.aacircleRGBA(dest.surface, (short)x, (short)y, (short)radius, color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws an antialiased hollow circle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the circle will be drawn.</param>
  /// <param name="x">The X coordinate of the circle's center point.</param>
  /// <param name="y">The Y coordinate of the circle's center point.</param>
  /// <param name="radius">The radius of the circle, in pixels.</param>
  /// <param name="color">The base color of the circle. Surrounding pixels will be blended with this color to provide
  /// antialiasing. The alpha component of this color is ignored.
  /// </param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the circle
  /// will be alpha blended onto the surface.
  /// </param>
  public static void CircleAA(Surface dest, int x, int y, int radius, Color color, byte alpha)
  { Check(GFX.aacircleRGBA(dest.surface, (short)x, (short)y, (short)radius, color.R, color.G, color.B, alpha));
  }

  /// <summary>Draws a filled circle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the circle will be drawn.</param>
  /// <param name="pt">The circle's center point.</param>
  /// <param name="radius">The radius of the circle, in pixels.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the circle will be
  /// alpha blended onto the surface.
  /// </param>
  public static void FilledCircle(Surface dest, Point pt, int radius, Color color)
  { FilledCircle(dest, pt.X, pt.Y, radius, color);
  }
  /// <summary>Draws a filled circle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the circle will be drawn.</param>
  /// <param name="pt">The circle's center point.</param>
  /// <param name="radius">The radius of the circle, in pixels.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the circle
  /// will be alpha blended onto the surface.
  /// </param>
  public static void FilledCircle(Surface dest, Point pt, int radius, Color color, byte alpha)
  { FilledCircle(dest, pt.X, pt.Y, radius, color, alpha);
  }
  /// <summary>Draws a filled circle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the circle will be drawn.</param>
  /// <param name="x">The X coordinate of the circle's center point.</param>
  /// <param name="y">The Y coordinate of the circle's center point.</param>
  /// <param name="radius">The radius of the circle, in pixels.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the circle will be
  /// alpha blended onto the surface.
  /// </param>
  public static void FilledCircle(Surface dest, int x, int y, int radius, Color color)
  { Check(GFX.filledCircleRGBA(dest.surface, (short)x, (short)y, (short)radius, color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws a filled circle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the circle will be drawn.</param>
  /// <param name="x">The X coordinate of the circle's center point.</param>
  /// <param name="y">The Y coordinate of the circle's center point.</param>
  /// <param name="radius">The radius of the circle, in pixels.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the circle
  /// will be alpha blended onto the surface.
  /// </param>
  public static void FilledCircle(Surface dest, int x, int y, int radius, Color color, byte alpha)
  { Check(GFX.filledCircleRGBA(dest.surface, (short)x, (short)y, (short)radius, color.R, color.G, color.B, alpha));
  }
  #endregion

  #region Ellipses
  /// <summary>Draws a hollow ellipse, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the ellipse will be drawn.</param>
  /// <param name="pt">The ellipse's center point.</param>
  /// <param name="xRadius">The radius of the ellipse along the X axis, in pixels.</param>
  /// <param name="yRadius">The radius of the ellipse along the Y axis, in pixels.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the ellipse will be
  /// alpha blended onto the surface.
  /// </param>
  public static void Ellipse(Surface dest, Point pt, int xRadius, int yRadius, Color color)
  { Ellipse(dest, pt.X, pt.Y, xRadius, yRadius, color);
  }
  /// <summary>Draws a hollow ellipse, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the ellipse will be drawn.</param>
  /// <param name="pt">The ellipse's center point.</param>
  /// <param name="xRadius">The radius of the ellipse along the X axis, in pixels.</param>
  /// <param name="yRadius">The radius of the ellipse along the Y axis, in pixels.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the ellipse
  /// will be alpha blended onto the surface.
  /// </param>
  public static void Ellipse(Surface dest, Point pt, int xRadius, int yRadius, Color color, byte alpha)
  { Ellipse(dest, pt.X, pt.Y, xRadius, yRadius, color, alpha);
  }
  /// <summary>Draws a hollow ellipse, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the ellipse will be drawn.</param>
  /// <param name="x">The X coordinate of the ellipse's center point.</param>
  /// <param name="y">The Y coordinate of the ellipse's center point.</param>
  /// <param name="xRadius">The radius of the ellipse along the X axis, in pixels.</param>
  /// <param name="yRadius">The radius of the ellipse along the Y axis, in pixels.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the ellipse will be
  /// alpha blended onto the surface.
  /// </param>
  public static void Ellipse(Surface dest, int x, int y, int xRadius, int yRadius, Color color)
  { Check(GFX.ellipseRGBA(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius,
                          color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws a hollow ellipse, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the ellipse will be drawn.</param>
  /// <param name="x">The X coordinate of the ellipse's center point.</param>
  /// <param name="y">The Y coordinate of the ellipse's center point.</param>
  /// <param name="xRadius">The radius of the ellipse along the X axis, in pixels.</param>
  /// <param name="yRadius">The radius of the ellipse along the Y axis, in pixels.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the ellipse
  /// will be alpha blended onto the surface.
  /// </param>
  public static void Ellipse(Surface dest, int x, int y, int xRadius, int yRadius, Color color, byte alpha)
  { Check(GFX.ellipseRGBA(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius,
                          color.R, color.G, color.B, alpha));
  }

  /// <summary>Draws an antialiased hollow ellipse, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the ellipse will be drawn.</param>
  /// <param name="pt">The ellipse's center point.</param>
  /// <param name="xRadius">The radius of the ellipse along the X axis, in pixels.</param>
  /// <param name="yRadius">The radius of the ellipse along the Y axis, in pixels.</param>
  /// <param name="color">The base color of the ellipse. Surrounding pixels will be blended with this color to provide
  /// antialiasing. If the color contains a non-opaque alpha level, the ellipse will be alpha blended onto the surface.
  /// </param>
  public static void EllipseAA(Surface dest, Point pt, int xRadius, int yRadius, Color color)
  { EllipseAA(dest, pt.X, pt.Y, xRadius, yRadius, color);
  }
  /// <summary>Draws an antialiased hollow ellipse, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the ellipse will be drawn.</param>
  /// <param name="pt">The ellipse's center point.</param>
  /// <param name="xRadius">The radius of the ellipse along the X axis, in pixels.</param>
  /// <param name="yRadius">The radius of the ellipse along the Y axis, in pixels.</param>
  /// <param name="color">The base color of the ellipse. Surrounding pixels will be blended with this color to provide
  /// antialiasing. The alpha component of this color is ignored.
  /// </param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the ellipse
  /// will be alpha blended onto the surface.
  /// </param>
  public static void EllipseAA(Surface dest, Point pt, int xRadius, int yRadius, Color color, byte alpha)
  { EllipseAA(dest, pt.X, pt.Y, xRadius, yRadius, color, alpha);
  }
  /// <summary>Draws an antialiased hollow ellipse, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the ellipse will be drawn.</param>
  /// <param name="x">The X coordinate of the ellipse's center point.</param>
  /// <param name="y">The Y coordinate of the ellipse's center point.</param>
  /// <param name="xRadius">The radius of the ellipse along the X axis, in pixels.</param>
  /// <param name="yRadius">The radius of the ellipse along the Y axis, in pixels.</param>
  /// <param name="color">The base color of the ellipse. Surrounding pixels will be blended with this color to provide
  /// antialiasing. If the color contains a non-opaque alpha level, the ellipse will be alpha blended onto the surface.
  /// </param>
  public static void EllipseAA(Surface dest, int x, int y, int xRadius, int yRadius, Color color)
  { Check(GFX.aaellipseRGBA(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius,
                            color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws an antialiased hollow ellipse, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the ellipse will be drawn.</param>
  /// <param name="x">The X coordinate of the ellipse's center point.</param>
  /// <param name="y">The Y coordinate of the ellipse's center point.</param>
  /// <param name="xRadius">The radius of the ellipse along the X axis, in pixels.</param>
  /// <param name="yRadius">The radius of the ellipse along the Y axis, in pixels.</param>
  /// <param name="color">The base color of the ellipse. Surrounding pixels will be blended with this color to provide
  /// antialiasing. The alpha component of this color is ignored.
  /// </param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the ellipse
  /// will be alpha blended onto the surface.
  /// </param>
  public static void EllipseAA(Surface dest, int x, int y, int xRadius, int yRadius, Color color, byte alpha)
  { Check(GFX.aaellipseRGBA(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius,
                            color.R, color.G, color.B, alpha));
  }

  /// <summary>Draws a filled ellipse, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the ellipse will be drawn.</param>
  /// <param name="pt">The ellipse's center point.</param>
  /// <param name="xRadius">The radius of the ellipse along the X axis, in pixels.</param>
  /// <param name="yRadius">The radius of the ellipse along the Y axis, in pixels.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the ellipse will be
  /// alpha blended onto the surface.
  /// </param>
  public static void FilledEllipse(Surface dest, Point pt, int xRadius, int yRadius, Color color)
  { FilledEllipse(dest, pt.X, pt.Y, xRadius, yRadius, color);
  }
  /// <summary>Draws a filled ellipse, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the ellipse will be drawn.</param>
  /// <param name="pt">The ellipse's center point.</param>
  /// <param name="xRadius">The radius of the ellipse along the X axis, in pixels.</param>
  /// <param name="yRadius">The radius of the ellipse along the Y axis, in pixels.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the ellipse
  /// will be alpha blended onto the surface.
  /// </param>
  public static void FilledEllipse(Surface dest, Point pt, int xRadius, int yRadius, Color color, byte alpha)
  { FilledEllipse(dest, pt.X, pt.Y, xRadius, yRadius, color, alpha);
  }
  /// <summary>Draws a filled ellipse, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the ellipse will be drawn.</param>
  /// <param name="x">The X coordinate of the ellipse's center point.</param>
  /// <param name="y">The Y coordinate of the ellipse's center point.</param>
  /// <param name="xRadius">The radius of the ellipse along the X axis, in pixels.</param>
  /// <param name="yRadius">The radius of the ellipse along the Y axis, in pixels.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the ellipse will be
  /// alpha blended onto the surface.
  /// </param>
  public static void FilledEllipse(Surface dest, int x, int y, int xRadius, int yRadius, Color color)
  { Check(GFX.filledEllipseRGBA(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius,
                                color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws a filled ellipse, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the ellipse will be drawn.</param>
  /// <param name="x">The X coordinate of the ellipse's center point.</param>
  /// <param name="y">The Y coordinate of the ellipse's center point.</param>
  /// <param name="xRadius">The radius of the ellipse along the X axis, in pixels.</param>
  /// <param name="yRadius">The radius of the ellipse along the Y axis, in pixels.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the ellipse
  /// will be alpha blended onto the surface.
  /// </param>
  public static void FilledEllipse(Surface dest, int x, int y, int xRadius, int yRadius, Color color, byte alpha)
  { Check(GFX.filledEllipseRGBA(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius,
                                color.R, color.G, color.B, alpha));
  }
  #endregion

  #region Pies
  /// <summary>Draws a filled pie, with optional alpha blending.</summary>
  /// <param name="dest">The surface into which the pie will be drawn.</param>
  /// <param name="pt">The pie's center point.</param>
  /// <param name="radius">The radius of the pie, in pixels.</param>
  /// <param name="startDegs">The starting angle of the pie's arc, in degrees. 0 is to the right.</param>
  /// <param name="endDegs">The ending angle of the pie's arc, in degrees. 0 is to the right.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the pie will be
  /// alpha blended onto the surface.
  /// </param>
  public static void FilledPie(Surface dest, Point pt, int radius, int startDegs, int endDegs, Color color)
  { FilledPie(dest, pt.X, pt.Y, radius, startDegs, endDegs, color);
  }
  /// <summary>Draws a filled pie, with optional alpha blending.</summary>
  /// <param name="dest">The surface into which the pie will be drawn.</param>
  /// <param name="pt">The pie's center point.</param>
  /// <param name="radius">The radius of the pie, in pixels.</param>
  /// <param name="startDegs">The starting angle of the pie's arc, in degrees. 0 is to the right.</param>
  /// <param name="endDegs">The ending angle of the pie's arc, in degrees. 0 is to the right.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the pie
  /// will be alpha blended onto the surface.
  /// </param>
  public static void FilledPie(Surface dest, Point pt, int radius, int startDegs, int endDegs,
                               Color color, byte alpha)
  { FilledPie(dest, pt.X, pt.Y, radius, startDegs, endDegs, color, alpha);
  }
  /// <summary>Draws a filled pie, with optional alpha blending.</summary>
  /// <param name="dest">The surface into which the pie will be drawn.</param>
  /// <param name="x">The X coordinate of the pie's center point.</param>
  /// <param name="y">The Y coordinate of the pie's center point.</param>
  /// <param name="radius">The radius of the pie, in pixels.</param>
  /// <param name="startDegs">The starting angle of the pie's arc, in degrees. 0 is to the right.</param>
  /// <param name="endDegs">The ending angle of the pie's arc, in degrees. 0 is to the right.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the pie will be
  /// alpha blended onto the surface.
  /// </param>
  public static void FilledPie(Surface dest, int x, int y, int radius, int startDegs, int endDegs, Color color)
  { Check(GFX.filledpieRGBA(dest.surface, (short)x, (short)y, (short)radius, (short)(startDegs%360),
                            (short)(endDegs%360), color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws a filled pie, with optional alpha blending.</summary>
  /// <param name="dest">The surface into which the pie will be drawn.</param>
  /// <param name="x">The X coordinate of the pie's center point.</param>
  /// <param name="y">The Y coordinate of the pie's center point.</param>
  /// <param name="radius">The radius of the pie, in pixels.</param>
  /// <param name="startDegs">The starting angle of the pie's arc, in degrees. 0 is to the right.</param>
  /// <param name="endDegs">The ending angle of the pie's arc, in degrees. 0 is to the right.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the pie
  /// will be alpha blended onto the surface.
  /// </param>
  public static void FilledPie(Surface dest, int x, int y, int radius, int startDegs, int endDegs,
                               Color color, byte alpha)
  { Check(GFX.filledpieRGBA(dest.surface, (short)x, (short)y, (short)radius, (short)(startDegs%360),
                            (short)(endDegs%360), color.R, color.G, color.B, alpha));
  }
  #endregion

  #region Triangles
  /// <summary>Draws a hollow triangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the triangle will be drawn.</param>
  /// <param name="p1">The first point of the triangle.</param>
  /// <param name="p2">The second point of the triangle.</param>
  /// <param name="p3">The third point of the triangle.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the triangle will be
  /// alpha blended onto the surface.
  /// </param>
  public static void Triangle(Surface dest, Point p1, Point p2, Point p3, Color color)
  { Triangle(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color);
  }
  /// <summary>Draws a hollow triangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the triangle will be drawn.</param>
  /// <param name="p1">The first point of the triangle.</param>
  /// <param name="p2">The second point of the triangle.</param>
  /// <param name="p3">The third point of the triangle.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the triangle
  /// will be alpha blended onto the surface.
  /// </param>
  public static void Triangle(Surface dest, Point p1, Point p2, Point p3, Color color, byte alpha)
  { Triangle(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color, alpha);
  }
  /// <summary>Draws a hollow triangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the triangle will be drawn.</param>
  /// <param name="x1">The X coordinate of the first point of the triangle.</param>
  /// <param name="y1">The Y coordinate of the first point of the triangle.</param>
  /// <param name="x2">The X coordinate of the second point of the triangle.</param>
  /// <param name="y2">The Y coordinate of the second point of the triangle.</param>
  /// <param name="x3">The X coordinate of the third point of the triangle.</param>
  /// <param name="y3">The Y coordinate of the third point of the triangle.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the triangle will be
  /// alpha blended onto the surface.
  /// </param>
  public static void Triangle(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, Color color)
  { Check(GFX.trigonRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3,
                         color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws a hollow triangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the triangle will be drawn.</param>
  /// <param name="x1">The X coordinate of the first point of the triangle.</param>
  /// <param name="y1">The Y coordinate of the first point of the triangle.</param>
  /// <param name="x2">The X coordinate of the second point of the triangle.</param>
  /// <param name="y2">The Y coordinate of the second point of the triangle.</param>
  /// <param name="x3">The X coordinate of the third point of the triangle.</param>
  /// <param name="y3">The Y coordinate of the third point of the triangle.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the triangle
  /// will be alpha blended onto the surface.
  /// </param>
  public static void Triangle(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, Color color, byte alpha)
  { Check(GFX.trigonRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3,
                         color.R, color.G, color.B, alpha));
  }

  /// <summary>Draws an antialiased hollow triangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the triangle will be drawn.</param>
  /// <param name="p1">The first point of the triangle.</param>
  /// <param name="p2">The second point of the triangle.</param>
  /// <param name="p3">The third point of the triangle.</param>
  /// <param name="color">The base color of the triangle. Surrounding pixels will be blended with this color to
  /// provide antialiasing. If the color contains a non-opaque alpha level, the triangle will be alpha blended onto
  /// the surface.
  /// </param>
  public static void TriangleAA(Surface dest, Point p1, Point p2, Point p3, Color color)
  { TriangleAA(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color);
  }
  /// <summary>Draws an antialiased hollow triangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the triangle will be drawn.</param>
  /// <param name="p1">The first point of the triangle.</param>
  /// <param name="p2">The second point of the triangle.</param>
  /// <param name="p3">The third point of the triangle.</param>
  /// <param name="color">The base color of the triangle. Surrounding pixels will be blended with this color to
  /// provide antialiasing. The alpha component of this color is ignored.
  /// </param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the triangle
  /// will be alpha blended onto the surface.
  /// </param>
  public static void TriangleAA(Surface dest, Point p1, Point p2, Point p3, Color color, byte alpha)
  { TriangleAA(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color, alpha);
  }
  /// <summary>Draws an antialiased hollow triangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the triangle will be drawn.</param>
  /// <param name="x1">The X coordinate of the first point of the triangle.</param>
  /// <param name="y1">The Y coordinate of the first point of the triangle.</param>
  /// <param name="x2">The X coordinate of the second point of the triangle.</param>
  /// <param name="y2">The Y coordinate of the second point of the triangle.</param>
  /// <param name="x3">The X coordinate of the third point of the triangle.</param>
  /// <param name="y3">The Y coordinate of the third point of the triangle.</param>
  /// <param name="color">The base color of the triangle. Surrounding pixels will be blended with this color to
  /// provide antialiasing. If the color contains a non-opaque alpha level, the triangle will be alpha blended onto
  /// the surface.
  /// </param>
  public static void TriangleAA(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, Color color)
  { Check(GFX.aatrigonRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3,
                           color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws an antialiased hollow triangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the triangle will be drawn.</param>
  /// <param name="x1">The X coordinate of the first point of the triangle.</param>
  /// <param name="y1">The Y coordinate of the first point of the triangle.</param>
  /// <param name="x2">The X coordinate of the second point of the triangle.</param>
  /// <param name="y2">The Y coordinate of the second point of the triangle.</param>
  /// <param name="x3">The X coordinate of the third point of the triangle.</param>
  /// <param name="y3">The Y coordinate of the third point of the triangle.</param>
  /// <param name="color">The base color of the triangle. Surrounding pixels will be blended with this color to
  /// provide antialiasing. The alpha component of this color is ignored.
  /// </param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the triangle
  /// will be alpha blended onto the surface.
  /// </param>
  public static void TriangleAA(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, Color color, byte alpha)
  { Check(GFX.aatrigonRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3,
                           color.R, color.G, color.B, alpha));
  }

  /// <summary>Draws a filled triangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the triangle will be drawn.</param>
  /// <param name="p1">The first point of the triangle.</param>
  /// <param name="p2">The second point of the triangle.</param>
  /// <param name="p3">The third point of the triangle.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the triangle will be
  /// alpha blended onto the surface.
  /// </param>
  public static void FilledTriangle(Surface dest, Point p1, Point p2, Point p3, Color color)
  { FilledTriangle(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color);
  }
  /// <summary>Draws a filled triangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the triangle will be drawn.</param>
  /// <param name="p1">The first point of the triangle.</param>
  /// <param name="p2">The second point of the triangle.</param>
  /// <param name="p3">The third point of the triangle.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the triangle
  /// will be alpha blended onto the surface.
  /// </param>
  public static void FilledTriangle(Surface dest, Point p1, Point p2, Point p3, Color color, byte alpha)
  { FilledTriangle(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color, alpha);
  }
  /// <summary>Draws a filled triangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the triangle will be drawn.</param>
  /// <param name="x1">The X coordinate of the first point of the triangle.</param>
  /// <param name="y1">The Y coordinate of the first point of the triangle.</param>
  /// <param name="x2">The X coordinate of the second point of the triangle.</param>
  /// <param name="y2">The Y coordinate of the second point of the triangle.</param>
  /// <param name="x3">The X coordinate of the third point of the triangle.</param>
  /// <param name="y3">The Y coordinate of the third point of the triangle.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the triangle will be
  /// alpha blended onto the surface.
  /// </param>
  public static void FilledTriangle(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, Color color)
  { Check(GFX.filledTrigonRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3,
                               color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws a filled triangle, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the triangle will be drawn.</param>
  /// <param name="x1">The X coordinate of the first point of the triangle.</param>
  /// <param name="y1">The Y coordinate of the first point of the triangle.</param>
  /// <param name="x2">The X coordinate of the second point of the triangle.</param>
  /// <param name="y2">The Y coordinate of the second point of the triangle.</param>
  /// <param name="x3">The X coordinate of the third point of the triangle.</param>
  /// <param name="y3">The Y coordinate of the third point of the triangle.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the triangle
  /// will be alpha blended onto the surface.
  /// </param>
  public static void FilledTriangle(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, Color color, byte alpha)
  { Check(GFX.filledTrigonRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3,
                               color.R, color.G, color.B, alpha));
  }
  #endregion  

  #region Polygons
  /// <summary>Draws a hollow polygon, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the polygon will be drawn.</param>
  /// <param name="points">An array of points defining the polygon.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the polygon will be
  /// alpha blended onto the surface.
  /// </param>
  public static void Polygon(Surface dest, Point[] points, Color color)
  { Polygon(dest, points, 0, points.Length, color);
  }
  /// <summary>Draws a hollow polygon, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the polygon will be drawn.</param>
  /// <param name="points">An array of points defining the polygon.</param>
  /// <param name="index">The starting index into <paramref name="points"/>.</param>
  /// <param name="length">The number of points to read from <paramref nam="points"/>, starting from
  /// <paramref name="index"/>.
  /// </param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the polygon will be
  /// alpha blended onto the surface.
  /// </param>
  public static void Polygon(Surface dest, Point[] points, int index, int length, Color color)
  { if(index<0 || length<0 || index+length>points.Length) throw new ArgumentOutOfRangeException();
    if(length==0) return;
    short *vx = stackalloc short[length];
    short *vy = stackalloc short[length];
    fixed(Point* ptb=points)
    { Point* pt = ptb+index;
      for(int i=0; i<length; i++) { vx[i]=(short)pt[i].X; vy[i]=(short)pt[i].Y; }
    }
    Check(GFX.polygonRGBA(dest.surface, vx, vy, length, color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws a hollow polygon, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the polygon will be drawn.</param>
  /// <param name="points">An array of points defining the polygon.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the polygon
  /// will be alpha blended onto the surface.
  /// </param>
  public static void Polygon(Surface dest, Point[] points, Color color, byte alpha)
  { Polygon(dest, points, 0, points.Length, Color.FromArgb(alpha, color));
  }
  /// <summary>Draws a hollow polygon, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the polygon will be drawn.</param>
  /// <param name="points">An array of points defining the polygon.</param>
  /// <param name="index">The starting index into <paramref name="points"/>.</param>
  /// <param name="length">The number of points to read from <paramref nam="points"/>, starting from
  /// <paramref name="index"/>.
  /// </param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the polygon
  /// will be alpha blended onto the surface.
  /// </param>
  public static void Polygon(Surface dest, Point[] points, int index, int length, Color color, byte alpha)
  { Polygon(dest, points, index, length, Color.FromArgb(alpha, color));
  }

  /// <summary>Draws an antialiased hollow polygon, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the polygon will be drawn.</param>
  /// <param name="points">An array of points defining the polygon.</param>
  /// <param name="color">The base color of the polygon. Surrounding pixels will be blended with this color to
  /// provide antialiasing. If the color contains a non-opaque alpha level, the polygon will be alpha blended onto
  /// the surface.
  /// </param>
  public static void PolygonAA(Surface dest, Point[] points, Color color)
  { PolygonAA(dest, points, 0, points.Length, color);
  }
  /// <summary>Draws an antialiased hollow polygon, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the polygon will be drawn.</param>
  /// <param name="points">An array of points defining the polygon.</param>
  /// <param name="index">The starting index into <paramref name="points"/>.</param>
  /// <param name="length">The number of points to read from <paramref nam="points"/>, starting from
  /// <paramref name="index"/>.
  /// </param>
  /// <param name="color">The base color of the polygon. Surrounding pixels will be blended with this color to
  /// provide antialiasing. If the color contains a non-opaque alpha level, the polygon will be alpha blended onto
  /// the surface.
  /// </param>
  public static void PolygonAA(Surface dest, Point[] points, int index, int length, Color color)
  { if(index<0 || length<0 || index+length>points.Length) throw new ArgumentOutOfRangeException();
    if(length==0) return;
    short *vx = stackalloc short[length];
    short *vy = stackalloc short[length];
    fixed(Point* ptb=points)
    { Point* pt = ptb+index;
      for(int i=0; i<length; i++) { vx[i]=(short)pt[i].X; vy[i]=(short)pt[i].Y; }
    }
    Check(GFX.aapolygonRGBA(dest.surface, vx, vy, length, color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws an antialiased hollow polygon, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the polygon will be drawn.</param>
  /// <param name="points">An array of points defining the polygon.</param>
  /// <param name="color">The base color of the polygon. Surrounding pixels will be blended with this color to
  /// provide antialiasing. The alpha component of this color is ignored.
  /// </param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the polygon
  /// will be alpha blended onto the surface.
  /// </param>
  public static void PolygonAA(Surface dest, Point[] points, Color color, byte alpha)
  { PolygonAA(dest, points, 0, points.Length, Color.FromArgb(alpha, color));
  }
  /// <summary>Draws an antialiased hollow polygon, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the polygon will be drawn.</param>
  /// <param name="points">An array of points defining the polygon.</param>
  /// <param name="index">The starting index into <paramref name="points"/>.</param>
  /// <param name="length">The number of points to read from <paramref nam="points"/>, starting from
  /// <paramref name="index"/>.
  /// </param>
  /// <param name="color">The base color of the polygon. Surrounding pixels will be blended with this color to
  /// provide antialiasing. The alpha component of this color is ignored.
  /// </param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the polygon
  /// will be alpha blended onto the surface.
  /// </param>
  public static void PolygonAA(Surface dest, Point[] points, int index, int length, Color color, byte alpha)
  { PolygonAA(dest, points, index, length, Color.FromArgb(alpha, color));
  }

  /// <summary>Draws a filled polygon, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the polygon will be drawn.</param>
  /// <param name="points">An array of points defining the polygon.</param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the polygon will be
  /// alpha blended onto the surface.
  /// </param>
  public static void FilledPolygon(Surface dest, Point[] points, Color color)
  { FilledPolygon(dest, points, 0, points.Length, color);
  }
  /// <summary>Draws a filled polygon, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the polygon will be drawn.</param>
  /// <param name="points">An array of points defining the polygon.</param>
  /// <param name="index">The starting index into <paramref name="points"/>.</param>
  /// <param name="length">The number of points to read from <paramref nam="points"/>, starting from
  /// <paramref name="index"/>.
  /// </param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the polygon will be
  /// alpha blended onto the surface.
  /// </param>
  public static void FilledPolygon(Surface dest, Point[] points, int index, int length, Color color)
  { if(index<0 || length<0 || index+length>points.Length) throw new ArgumentOutOfRangeException();
    if(length==0) return;
    short *vx = stackalloc short[length];
    short *vy = stackalloc short[length];
    fixed(Point* ptb=points)
    { Point* pt = ptb+index;
      for(int i=0; i<length; i++) { vx[i]=(short)pt[i].X; vy[i]=(short)pt[i].Y; }
    }
    Check(GFX.filledPolygonRGBA(dest.surface, vx, vy, length, color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws a filled polygon, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the polygon will be drawn.</param>
  /// <param name="points">An array of points defining the polygon.</param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the polygon
  /// will be alpha blended onto the surface.
  /// </param>
  public static void FilledPolygon(Surface dest, Point[] points, Color color, byte alpha)
  { FilledPolygon(dest, points, 0, points.Length, Color.FromArgb(alpha, color));
  }
  /// <summary>Draws a filled polygon, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the polygon will be drawn.</param>
  /// <param name="points">An array of points defining the polygon.</param>
  /// <param name="index">The starting index into <paramref name="points"/>.</param>
  /// <param name="length">The number of points to read from <paramref nam="points"/>, starting from
  /// <paramref name="index"/>.
  /// </param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the polygon
  /// will be alpha blended onto the surface.
  /// </param>
  public static void FilledPolygon(Surface dest, Point[] points, int index, int length, Color color, byte alpha)
  { FilledPolygon(dest, points, index, length, Color.FromArgb(alpha, color));
  }
  #endregion

  #region Curves
  /// <summary>Draws a bezier curve, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the curve will be drawn.</param>
  /// <param name="points">An array of points defining the curve.</param>
  /// <param name="steps">The number of interpolation steps. Higher numbers of steps will yield smoother curves,
  /// but with correspondingly reduced performance.
  /// </param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the curve will be
  /// alpha blended onto the surface.
  /// </param>
  public static void Bezier(Surface dest, Point[] points, int steps, Color color)
  { Bezier(dest, points, 0, points.Length, steps, color);
  }
  /// <summary>Draws a bezier curve, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the curve will be drawn.</param>
  /// <param name="points">An array of points defining the curve.</param>
  /// <param name="index">The starting index into <paramref name="points"/>.</param>
  /// <param name="length">The number of points to read from <paramref nam="points"/>, starting from
  /// <paramref name="index"/>.
  /// </param>
  /// <param name="steps">The number of interpolation steps. Higher numbers of steps will yield smoother curves,
  /// but with correspondingly reduced performance.
  /// </param>
  /// <param name="color">The color to use. If the color contains a non-opaque alpha level, the curve will be
  /// alpha blended onto the surface.
  /// </param>
  public static void Bezier(Surface dest, Point[] points, int index, int length, int steps, Color color)
  { if(index<0 || length<0 || index+length>points.Length) throw new ArgumentOutOfRangeException();
    if(length==0) return;
    short *vx = stackalloc short[length];
    short *vy = stackalloc short[length];
    fixed(Point* ptb=points)
    { Point* pt = ptb+index;
      for(int i=0; i<length; i++) { vx[i]=(short)pt[i].X; vy[i]=(short)pt[i].Y; }
    }
    Check(GFX.bezierRGBA(dest.surface, vx, vy, length, steps, color.R, color.G, color.B, color.A));
  }
  /// <summary>Draws a bezier curve, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the curve will be drawn.</param>
  /// <param name="points">An array of points defining the curve.</param>
  /// <param name="steps">The number of interpolation steps. Higher numbers of steps will yield smoother curves,
  /// but with correspondingly reduced performance.
  /// </param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the curve
  /// will be alpha blended onto the surface.
  /// </param>
  public static void Bezier(Surface dest, Point[] points, int steps, Color color, byte alpha)
  { Bezier(dest, points, 0, points.Length, steps, Color.FromArgb(alpha, color));
  }
  /// <summary>Draws a bezier curve, with optional alpha blending.</summary>
  /// <param name="dest">The surface onto which the curve will be drawn.</param>
  /// <param name="points">An array of points defining the curve.</param>
  /// <param name="index">The starting index into <paramref name="points"/>.</param>
  /// <param name="length">The number of points to read from <paramref nam="points"/>, starting from
  /// <paramref name="index"/>.
  /// </param>
  /// <param name="steps">The number of interpolation steps. Higher numbers of steps will yield smoother curves,
  /// but with correspondingly reduced performance.
  /// </param>
  /// <param name="color">The color to use. The alpha component of this color is ignored.</param>
  /// <param name="alpha">The alpha value to use, from 0 (transparent) to 255 (opaque). If not opaque, the curve
  /// will be alpha blended onto the surface.
  /// </param>
  public static void Bezier(Surface dest, Point[] points, int index, int length, int steps, Color color, byte alpha)
  { Bezier(dest, points, index, length, steps, Color.FromArgb(alpha, color));
  }
  #endregion

  static void Check(int result)
  { if(result!=0) throw new VideoException("An error occurred during a graphics primitive call");
  }
}

} // namespace GameLib.Video