/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2009 Adam Milazzo

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

public static unsafe class Primitives
{
  #region Pixels
  /// <include file="../documentation.xml" path="//Video/Primitives/Pixel/*[self::Common or self::Pt or self::C]/*"/>
  public static void Pixel(Surface dest, Point pt, Color color) { Pixel(dest, pt.X, pt.Y, color); }
  /// <include file="../documentation.xml" path="//Video/Primitives/Pixel/*[self::Common or self::XY or self::C]/*"/>
  public static void Pixel(Surface dest, int x, int y, Color color)
  { Check(GFX.pixelRGBA(dest.surface, (short)x, (short)y, color.R, color.G, color.B, color.A));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Pixel/*[self::Common or self::Pt or self::CA]/*"/>
  public static void Pixel(Surface dest, Point pt, Color color, byte alpha) { Pixel(dest, pt.X, pt.Y, color, alpha); }
  /// <include file="../documentation.xml" path="//Video/Primitives/Pixel/*[self::Common or self::XY or self::CA]/*"/>
  public static void Pixel(Surface dest, int x, int y, Color color, byte alpha)
  { Check(GFX.pixelRGBA(dest.surface, (short)x, (short)y, color.R, color.G, color.B, alpha));
  }
  #endregion

  #region Lines
  /// <include file="../documentation.xml" path="//Video/Primitives/Line/*[self::HLine or self::C]/*"/>
  public static void HLine(Surface dest, int x1, int x2, int y, Color color)
  { Check(GFX.hlineRGBA(dest.surface, (short)x1, (short)x2, (short)y, color.R, color.G, color.B, color.A));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Line/*[self::HLine or self::CA]/*"/>
  public static void HLine(Surface dest, int x1, int x2, int y, Color color, byte alpha)
  { Check(GFX.hlineRGBA(dest.surface, (short)x1, (short)x2, (short)y, color.R, color.G, color.B, alpha));
  }

  /// <include file="../documentation.xml" path="//Video/Primitives/Line/*[self::VLine or self::C]/*"/>
  public static void VLine(Surface dest, int x, int y1, int y2, Color color)
  { Check(GFX.vlineRGBA(dest.surface, (short)x, (short)y1, (short)y2, color.R, color.G, color.B, color.A));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Line/*[self::VLine or self::CA]/*"/>
  public static void VLine(Surface dest, int x, int y1, int y2, Color color, byte alpha)
  { Check(GFX.vlineRGBA(dest.surface, (short)x, (short)y1, (short)y2, color.R, color.G, color.B, alpha));
  }

  /// <include file="../documentation.xml" path="//Video/Primitives/Line/*[self::Line or self::Pt or self::C]/*"/>
  public static void Line(Surface dest, Point p1, Point p2, Color color)
  { Line(dest, p1.X, p1.Y, p2.X, p2.Y, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Line/*[self::Line or self::Pt or self::CA]/*"/>
  public static void Line(Surface dest, Point p1, Point p2, Color color, byte alpha)
  { Line(dest, p1.X, p1.Y, p2.X, p2.Y, color, alpha);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Line/*[self::Line or self::XY or self::C]/*"/>
  public static void Line(Surface dest, int x1, int y1, int x2, int y2, Color color)
  { Check(GFX.lineRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, color.A));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Line/*[self::Line or self::XY or self::CA]/*"/>
  public static void Line(Surface dest, int x1, int y1, int x2, int y2, Color color, byte alpha)
  { Check(GFX.lineRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, alpha));
  }

  /// <include file="../documentation.xml" path="//Video/Primitives/Line/*[self::AA or self::Pt or self::C]/*"/>
  public static void LineAA(Surface dest, Point p1, Point p2, Color color)
  { LineAA(dest, p1.X, p1.Y, p2.X, p2.Y, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Line/*[self::AA or self::Pt or self::CA]/*"/>
  public static void LineAA(Surface dest, Point p1, Point p2, Color color, byte alpha)
  { LineAA(dest, p1.X, p1.Y, p2.X, p2.Y, color, alpha);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Line/*[self::AA or self::XY or self::C]/*"/>
  public static void LineAA(Surface dest, int x1, int y1, int x2, int y2, Color color)
  { Check(GFX.aalineRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, color.A));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Line/*[self::AA or self::XY or self::CA]/*"/>
  public static void LineAA(Surface dest, int x1, int y1, int x2, int y2, Color color, byte alpha)
  { Check(GFX.aalineRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, alpha));
  }
  #endregion

  #region Rectangles
  /// <include file="../documentation.xml" path="//Video/Primitives/Rectangle/*[self::Hollow or self::Rect or self::C]/*"/>
  public static void Box(Surface dest, Rectangle rect, Color color)
  { Box(dest, rect.X, rect.Y, rect.Right-1, rect.Bottom-1, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Rectangle/*[self::Hollow or self::Rect or self::CA]/*"/>
  public static void Box(Surface dest, Rectangle rect, Color color, byte alpha)
  { Box(dest, rect.X, rect.Y, rect.Right-1, rect.Bottom-1, color, alpha);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Rectangle/*[self::Hollow or self::Pt or self::C]/*"/>
  public static void Box(Surface dest, Point p1, Point p2, Color color) { Box(dest, p1.X, p1.Y, p2.X, p2.Y, color); }
  /// <include file="../documentation.xml" path="//Video/Primitives/Rectangle/*[Hollow or Pt or CA]/*"/>
  public static void Box(Surface dest, Point p1, Point p2, Color color, byte alpha)
  { Box(dest, p1.X, p1.Y, p2.X, p2.Y, color, alpha);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Rectangle/*[self::Hollow or self::XY or self::C]/*"/>
  public static void Box(Surface dest, int x1, int y1, int x2, int y2, Color color)
  { Check(GFX.rectangleRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2,
                            color.R, color.G, color.B, color.A));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Rectangle/*[self::Hollow or self::XY or self::CA]/*"/>
  public static void Box(Surface dest, int x1, int y1, int x2, int y2, Color color, byte alpha)
  { Check(GFX.rectangleRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2,
                            color.R, color.G, color.B, alpha));
  }

  /// <include file="../documentation.xml" path="//Video/Primitives/Rectangle/*[self::Filled or self::Rect or self::C]/*"/>
  public static void FilledBox(Surface dest, Rectangle rect, Color color)
  { FilledBox(dest, rect.X, rect.Y, rect.Right-1, rect.Bottom-1, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Rectangle/*[self::Filled or self::Rect or self::CA]/*"/>
  public static void FilledBox(Surface dest, Rectangle rect, Color color, byte alpha)
  { FilledBox(dest, rect.X, rect.Y, rect.Right-1, rect.Bottom-1, color, alpha);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Rectangle/*[self::Filled or self::Pt or self::C]/*"/>
  public static void FilledBox(Surface dest, Point p1, Point p2, Color color)
  { FilledBox(dest, p1.X, p1.Y, p2.X, p2.Y, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Rectangle/*[self::Filled or self::Pt or self::CA]/*"/>
  public static void FilledBox(Surface dest, Point p1, Point p2, Color color, byte alpha)
  { FilledBox(dest, p1.X, p1.Y, p2.X, p2.Y, color, alpha);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Rectangle/*[self::Filled or self::XY or self::C]/*"/>
  public static void FilledBox(Surface dest, int x1, int y1, int x2, int y2, Color color)
  { Check(GFX.boxRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, color.A));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Rectangle/*[self::Filled or self::XY or self::CA]/*"/>
  public static void FilledBox(Surface dest, int x1, int y1, int x2, int y2, Color color, byte alpha)
  { Check(GFX.boxRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, alpha));
  }
  #endregion

  #region Circles
  /// <include file="../documentation.xml" path="//Video/Primitives/Circle/*[self::Hollow or self::Pt or self::C]/*"/>
  public static void Circle(Surface dest, Point pt, int radius, Color color)
  { Circle(dest, pt.X, pt.Y, radius, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Circle/*[self::Hollow or self::Pt or self::CA]/*"/>
  public static void Circle(Surface dest, Point pt, int radius, Color color, byte alpha)
  { Circle(dest, pt.X, pt.Y, radius, color, alpha);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Circle/*[self::Hollow or self::XY or self::C]/*"/>
  public static void Circle(Surface dest, int x, int y, int radius, Color color)
  { Check(GFX.circleRGBA(dest.surface, (short)x, (short)y, (short)radius, color.R, color.G, color.B, color.A));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Circle/*[self::Hollow or self::XY or self::CA]/*"/>
  public static void Circle(Surface dest, int x, int y, int radius, Color color, byte alpha)
  { Check(GFX.circleRGBA(dest.surface, (short)x, (short)y, (short)radius, color.R, color.G, color.B, alpha));
  }

  /// <include file="../documentation.xml" path="//Video/Primitives/Circle/*[self::AA or self::Pt or self::AAC]/*"/>
  public static void CircleAA(Surface dest, Point pt, int radius, Color color)
  { CircleAA(dest, pt.X, pt.Y, radius, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Circle/*[self::AA or self::Pt or self::AACA]/*"/>
  public static void CircleAA(Surface dest, Point pt, int radius, Color color, byte alpha)
  { CircleAA(dest, pt.X, pt.Y, radius, color, alpha);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Circle/*[self::AA or self::XY or self::AAC]/*"/>
  public static void CircleAA(Surface dest, int x, int y, int radius, Color color)
  { Check(GFX.aacircleRGBA(dest.surface, (short)x, (short)y, (short)radius, color.R, color.G, color.B, color.A));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Circle/*[self::AA or self::XY or self::AACA]/*"/>
  public static void CircleAA(Surface dest, int x, int y, int radius, Color color, byte alpha)
  { Check(GFX.aacircleRGBA(dest.surface, (short)x, (short)y, (short)radius, color.R, color.G, color.B, alpha));
  }

  /// <include file="../documentation.xml" path="//Video/Primitives/Circle/*[self::Filled or self::Pt or self::C]/*"/>
  public static void FilledCircle(Surface dest, Point pt, int radius, Color color)
  { FilledCircle(dest, pt.X, pt.Y, radius, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Circle/*[self::Filled or self::Pt or self::CA]/*"/>
  public static void FilledCircle(Surface dest, Point pt, int radius, Color color, byte alpha)
  { FilledCircle(dest, pt.X, pt.Y, radius, color, alpha);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Circle/*[self::Filled or self::XY or self::C]/*"/>
  public static void FilledCircle(Surface dest, int x, int y, int radius, Color color)
  { Check(GFX.filledCircleRGBA(dest.surface, (short)x, (short)y, (short)radius, color.R, color.G, color.B, color.A));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Circle/*[self::Filled or self::XY or self::CA]/*"/>
  public static void FilledCircle(Surface dest, int x, int y, int radius, Color color, byte alpha)
  { Check(GFX.filledCircleRGBA(dest.surface, (short)x, (short)y, (short)radius, color.R, color.G, color.B, alpha));
  }
  #endregion

  #region Ellipses
  /// <include file="../documentation.xml" path="//Video/Primitives/Ellipse/*[self::Hollow or self::Pt or self::C]/*"/>
  public static void Ellipse(Surface dest, Point pt, int xRadius, int yRadius, Color color)
  { Ellipse(dest, pt.X, pt.Y, xRadius, yRadius, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Ellipse/*[self::Hollow or self::Pt or self::CA]/*"/>
  public static void Ellipse(Surface dest, Point pt, int xRadius, int yRadius, Color color, byte alpha)
  { Ellipse(dest, pt.X, pt.Y, xRadius, yRadius, color, alpha);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Ellipse/*[self::Hollow or self::XY or self::C]/*"/>
  public static void Ellipse(Surface dest, int x, int y, int xRadius, int yRadius, Color color)
  { Check(GFX.ellipseRGBA(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius,
                          color.R, color.G, color.B, color.A));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Ellipse/*[self::Hollow or self::XY or self::CA]/*"/>
  public static void Ellipse(Surface dest, int x, int y, int xRadius, int yRadius, Color color, byte alpha)
  { Check(GFX.ellipseRGBA(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius,
                          color.R, color.G, color.B, alpha));
  }

  /// <include file="../documentation.xml" path="//Video/Primitives/Ellipse/*[self::AA or self::Pt or self::AAC]/*"/>
  public static void EllipseAA(Surface dest, Point pt, int xRadius, int yRadius, Color color)
  { EllipseAA(dest, pt.X, pt.Y, xRadius, yRadius, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Ellipse/*[self::AA or self::Pt or self::AACA]/*"/>
  public static void EllipseAA(Surface dest, Point pt, int xRadius, int yRadius, Color color, byte alpha)
  { EllipseAA(dest, pt.X, pt.Y, xRadius, yRadius, color, alpha);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Ellipse/*[self::AA or self::XY or self::AAC]/*"/>
  public static void EllipseAA(Surface dest, int x, int y, int xRadius, int yRadius, Color color)
  { Check(GFX.aaellipseRGBA(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius,
                            color.R, color.G, color.B, color.A));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Ellipse/*[self::AA or self::XY or self::AACA]/*"/>
  public static void EllipseAA(Surface dest, int x, int y, int xRadius, int yRadius, Color color, byte alpha)
  { Check(GFX.aaellipseRGBA(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius,
                            color.R, color.G, color.B, alpha));
  }

  /// <include file="../documentation.xml" path="//Video/Primitives/Ellipse/*[self::Filled or self::Pt or self::C]/*"/>
  public static void FilledEllipse(Surface dest, Point pt, int xRadius, int yRadius, Color color)
  { FilledEllipse(dest, pt.X, pt.Y, xRadius, yRadius, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Ellipse/*[self::Filled or self::Pt or self::CA]/*"/>
  public static void FilledEllipse(Surface dest, Point pt, int xRadius, int yRadius, Color color, byte alpha)
  { FilledEllipse(dest, pt.X, pt.Y, xRadius, yRadius, color, alpha);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Ellipse/*[self::Filled or self::XY or self::C]/*"/>
  public static void FilledEllipse(Surface dest, int x, int y, int xRadius, int yRadius, Color color)
  { Check(GFX.filledEllipseRGBA(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius,
                                color.R, color.G, color.B, color.A));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Ellipse/*[self::Filled or self::XY or self::CA]/*"/>
  public static void FilledEllipse(Surface dest, int x, int y, int xRadius, int yRadius, Color color, byte alpha)
  { Check(GFX.filledEllipseRGBA(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius,
                                color.R, color.G, color.B, alpha));
  }
  #endregion

  #region Pies
  /// <include file="../documentation.xml" path="//Video/Primitives/Pie/*[self::Filled or self::Pt or self::C]/*"/>
  public static void FilledPie(Surface dest, Point pt, int radius, int startDegs, int endDegs, Color color)
  { FilledPie(dest, pt.X, pt.Y, radius, startDegs, endDegs, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Pie/*[self::Filled or self::Pt or self::CA]/*"/>
  public static void FilledPie(Surface dest, Point pt, int radius, int startDegs, int endDegs,
                               Color color, byte alpha)
  { FilledPie(dest, pt.X, pt.Y, radius, startDegs, endDegs, color, alpha);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Pie/*[self::Filled or self::XY or self::C]/*"/>
  public static void FilledPie(Surface dest, int x, int y, int radius, int startDegs, int endDegs, Color color)
  { Check(GFX.filledpieRGBA(dest.surface, (short)x, (short)y, (short)radius, (short)(startDegs%360),
                            (short)(endDegs%360), color.R, color.G, color.B, color.A));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Pie/*[self::Filled or self::XY or self::CA]/*"/>
  public static void FilledPie(Surface dest, int x, int y, int radius, int startDegs, int endDegs,
                               Color color, byte alpha)
  { Check(GFX.filledpieRGBA(dest.surface, (short)x, (short)y, (short)radius, (short)(startDegs%360),
                            (short)(endDegs%360), color.R, color.G, color.B, alpha));
  }

  /// <include file="../documentation.xml" path="//Video/Primitives/Pie/*[self::Hollow or self::Pt or self::C]/*"/>
  public static void Pie(Surface dest, Point pt, int radius, int startDegs, int endDegs, Color color)
  { Pie(dest, pt.X, pt.Y, radius, startDegs, endDegs, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Pie/*[self::Hollow or self::Pt or self::CA]/*"/>
  public static void Pie(Surface dest, Point pt, int radius, int startDegs, int endDegs, Color color, byte alpha)
  { Pie(dest, pt.X, pt.Y, radius, startDegs, endDegs, color, alpha);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Pie/*[self::Hollow or self::XY or self::C]/*"/>
  public static void Pie(Surface dest, int x, int y, int radius, int startDegs, int endDegs, Color color)
  { Check(GFX.pieRGBA(dest.surface, (short)x, (short)y, (short)radius, (short)(startDegs%360),
                      (short)(endDegs%360), color.R, color.G, color.B, color.A));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Pie/*[self::Hollow or self::XY or self::CA]/*"/>
  public static void Pie(Surface dest, int x, int y, int radius, int startDegs, int endDegs, Color color, byte alpha)
  { Check(GFX.pieRGBA(dest.surface, (short)x, (short)y, (short)radius, (short)(startDegs%360),
                      (short)(endDegs%360), color.R, color.G, color.B, alpha));
  }
  #endregion

  #region Triangles
  /// <include file="../documentation.xml" path="//Video/Primitives/Triangle/*[self::Hollow or self::Pt or self::C]/*"/>
  public static void Triangle(Surface dest, Point p1, Point p2, Point p3, Color color)
  { Triangle(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Triangle/*[self::Hollow or self::Pt or self::CA]/*"/>
  public static void Triangle(Surface dest, Point p1, Point p2, Point p3, Color color, byte alpha)
  { Triangle(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color, alpha);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Triangle/*[self::Hollow or self::XY or self::C]/*"/>
  public static void Triangle(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, Color color)
  { Check(GFX.trigonRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3,
                         color.R, color.G, color.B, color.A));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Triangle/*[self::Hollow or self::XY or self::CA]/*"/>
  public static void Triangle(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, Color color, byte alpha)
  { Check(GFX.trigonRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3,
                         color.R, color.G, color.B, alpha));
  }

  /// <include file="../documentation.xml" path="//Video/Primitives/Triangle/*[self::AA or self::Pt or self::AAC]/*"/>
  public static void TriangleAA(Surface dest, Point p1, Point p2, Point p3, Color color)
  { TriangleAA(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Triangle/*[self::AA or self::Pt or self::AACA]/*"/>
  public static void TriangleAA(Surface dest, Point p1, Point p2, Point p3, Color color, byte alpha)
  { TriangleAA(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color, alpha);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Triangle/*[self::AA or self::XY or self::AAC]/*"/>
  public static void TriangleAA(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, Color color)
  { Check(GFX.aatrigonRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3,
                           color.R, color.G, color.B, color.A));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Triangle/*[self::AA or self::XY or self::AACA]/*"/>
  public static void TriangleAA(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, Color color, byte alpha)
  { Check(GFX.aatrigonRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3,
                           color.R, color.G, color.B, alpha));
  }

  /// <include file="../documentation.xml" path="//Video/Primitives/Triangle/*[self::Filled or self::Pt or self::C]/*"/>
  public static void FilledTriangle(Surface dest, Point p1, Point p2, Point p3, Color color)
  { FilledTriangle(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Triangle/*[self::Filled or self::Pt or self::CA]/*"/>
  public static void FilledTriangle(Surface dest, Point p1, Point p2, Point p3, Color color, byte alpha)
  { FilledTriangle(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color, alpha);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Triangle/*[self::Filled or self::XY or self::C]/*"/>
  public static void FilledTriangle(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, Color color)
  { Check(GFX.filledTrigonRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3,
                               color.R, color.G, color.B, color.A));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Triangle/*[self::Filled or self::XY or self::CA]/*"/>
  public static void FilledTriangle(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, Color color, byte alpha)
  { Check(GFX.filledTrigonRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3,
                               color.R, color.G, color.B, alpha));
  }
  #endregion  

  #region Polygons
  /// <include file="../documentation.xml" path="//Video/Primitives/Polygon/*[self::Hollow or self::A or self::C]/*"/>
  public static void Polygon(Surface dest, Point[] points, Color color)
  { Polygon(dest, points, 0, points.Length, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Polygon/*[self::Hollow or self::AL or self::C]/*"/>
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
  /// <include file="../documentation.xml" path="//Video/Primitives/Polygon/*[self::Hollow or self::A or self::CA]/*"/>
  public static void Polygon(Surface dest, Point[] points, Color color, byte alpha)
  { Polygon(dest, points, 0, points.Length, Color.FromArgb(alpha, color));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Polygon/*[self::Hollow or self::AL or self::CA]/*"/>
  public static void Polygon(Surface dest, Point[] points, int index, int length, Color color, byte alpha)
  { Polygon(dest, points, index, length, Color.FromArgb(alpha, color));
  }

  /// <include file="../documentation.xml" path="//Video/Primitives/Polygon/*[self::AA or self::A or self::AAC]/*"/>
  public static void PolygonAA(Surface dest, Point[] points, Color color)
  { PolygonAA(dest, points, 0, points.Length, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Polygon/*[self::AA or self::AL or self::AAC]/*"/>
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
  /// <include file="../documentation.xml" path="//Video/Primitives/Polygon/*[self::AA or self::A or self::AACA]/*"/>
  public static void PolygonAA(Surface dest, Point[] points, Color color, byte alpha)
  { PolygonAA(dest, points, 0, points.Length, Color.FromArgb(alpha, color));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Polygon/*[self::AA or self::AL or self::AACA]/*"/>
  public static void PolygonAA(Surface dest, Point[] points, int index, int length, Color color, byte alpha)
  { PolygonAA(dest, points, index, length, Color.FromArgb(alpha, color));
  }

  /// <include file="../documentation.xml" path="//Video/Primitives/Polygon/*[self::Filled or self::A or self::C]/*"/>
  public static void FilledPolygon(Surface dest, Point[] points, Color color)
  { FilledPolygon(dest, points, 0, points.Length, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Polygon/*[self::Filled or self::AL or self::C]/*"/>
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
  /// <include file="../documentation.xml" path="//Video/Primitives/Polygon/*[self::Filled or self::A or self::CA]/*"/>
  public static void FilledPolygon(Surface dest, Point[] points, Color color, byte alpha)
  { FilledPolygon(dest, points, 0, points.Length, Color.FromArgb(alpha, color));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Polygon/*[self::Filled or self::AL or self::CA]/*"/>
  public static void FilledPolygon(Surface dest, Point[] points, int index, int length, Color color, byte alpha)
  { FilledPolygon(dest, points, index, length, Color.FromArgb(alpha, color));
  }
  #endregion

  #region Curves
  /// <include file="../documentation.xml" path="//Video/Primitives/Curve/*[self::Common or self::A or self::C]/*"/>
  public static void Bezier(Surface dest, Point[] points, int steps, Color color)
  { Bezier(dest, points, 0, points.Length, steps, color);
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Curve/*[self::Common or self::AL or self::C]/*"/>
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
  /// <include file="../documentation.xml" path="//Video/Primitives/Curve/*[self::Common or self::A or self::CA]/*"/>
  public static void Bezier(Surface dest, Point[] points, int steps, Color color, byte alpha)
  { Bezier(dest, points, 0, points.Length, steps, Color.FromArgb(alpha, color));
  }
  /// <include file="../documentation.xml" path="//Video/Primitives/Curve/*[self::Common or self::AL or self::CA]/*"/>
  public static void Bezier(Surface dest, Point[] points, int index, int length, int steps, Color color, byte alpha)
  { Bezier(dest, points, index, length, steps, Color.FromArgb(alpha, color));
  }
  #endregion

  static void Check(int result)
  { if(result!=0) throw new VideoException("An error occurred during a graphics primitive call");
  }
}

} // namespace GameLib.Video