using System;
using System.Drawing;
using GameLib.Interop.SDLGFX;

namespace GameLib.Video
{

public unsafe class Primitives
{ private Primitives() { }
  
  #region Pixels
  public static void PutPixel(Surface dest, int x, int y, Color color)
  { Check(GFX.pixelRGBA(dest.surface, (short)x, (short)y, color.R, color.G, color.B, color.A));
  }
  public static void PutPixel(Surface dest, int x, int y, uint color)
  { Check(GFX.pixelColor(dest.surface, (short)x, (short)y, color));
  }
  public static void PutPixel(Surface dest, int x, int y, Color color, byte alpha)
  { Check(GFX.pixelRGBA(dest.surface, (short)x, (short)y, color.R, color.G, color.B, alpha));
  }
  #endregion

  #region Lines
  public static void HLine(Surface dest, int x1, int x2, int y, Color color)
  { Check(GFX.hlineRGBA(dest.surface, (short)x1, (short)x2, (short)y, color.R, color.G, color.B, color.A));
  }
  public static void HLine(Surface dest, int x1, int x2, int y, Color color, byte alpha)
  { Check(GFX.hlineRGBA(dest.surface, (short)x1, (short)x2, (short)y, color.R, color.G, color.B, alpha));
  }
  public static void HLine(Surface dest, int x1, int x2, int y, uint color)
  { Check(GFX.hlineColor(dest.surface, (short)x1, (short)x2, (short)y, color));
  }

  public static void VLine(Surface dest, int x, int y1, int y2, Color color)
  { Check(GFX.vlineRGBA(dest.surface, (short)x, (short)y1, (short)y2, color.R, color.G, color.B, color.A));
  }
  public static void VLine(Surface dest, int x, int y1, int y2, Color color, byte alpha)
  { Check(GFX.vlineRGBA(dest.surface, (short)x, (short)y1, (short)y2, color.R, color.G, color.B, alpha));
  }
  public static void VLine(Surface dest, int x, int y1, int y2, uint color)
  { Check(GFX.vlineColor(dest.surface, (short)x, (short)y1, (short)y2, color));
  }
  
  public static void Line(Surface dest, Point p1, Point p2, Color color) { Line(dest, p1.X, p1.Y, p2.X, p2.Y, color); }
  public static void Line(Surface dest, Point p1, Point p2, Color color, byte alpha) { Line(dest, p1.X, p1.Y, p2.X, p2.Y, color, alpha); }
  public static void Line(Surface dest, int x1, int y1, int x2, int y2, Color color)
  { Check(GFX.lineRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, color.A));
  }
  public static void Line(Surface dest, int x1, int y1, int x2, int y2, Color color, byte alpha)
  { Check(GFX.lineRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, alpha));
  }
  public static void Line(Surface dest, Point p1, Point p2, uint color) { Line(dest, p1.X, p1.Y, p2.X, p2.Y, color); }
  public static void Line(Surface dest, int x1, int y1, int x2, int y2, uint color)
  { Check(GFX.lineColor(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color));
  }
  
  public static void LineAA(Surface dest, Point p1, Point p2, Color color) { Line(dest, p1.X, p1.Y, p2.X, p2.Y, color); }
  public static void LineAA(Surface dest, Point p1, Point p2, Color color, byte alpha) { Line(dest, p1.X, p1.Y, p2.X, p2.Y, color, alpha); }
  public static void LineAA(Surface dest, int x1, int y1, int x2, int y2, Color color)
  { Check(GFX.aalineRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, color.A));
  }
  public static void LineAA(Surface dest, int x1, int y1, int x2, int y2, Color color, byte alpha)
  { Check(GFX.aalineRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, alpha));
  }
  public static void LineAA(Surface dest, Point p1, Point p2, uint color) { Line(dest, p1.X, p1.Y, p2.X, p2.Y, color); }
  public static void LineAA(Surface dest, int x1, int y1, int x2, int y2, uint color)
  { Check(GFX.aalineColor(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color));
  }
  #endregion

  #region Rectangles
  public static void Box(Surface dest, Rectangle rect, Color color) { Box(dest, rect.X, rect.Y, rect.X+rect.Width-1, rect.Y+rect.Height-1, color); }
  public static void Box(Surface dest, Rectangle rect, Color color, byte alpha) { Box(dest, rect.X, rect.Y, rect.X+rect.Width-1, rect.Y+rect.Height-1, color, alpha); }
  public static void Box(Surface dest, Point p1, Point p2, Color color) { Box(dest, p1.X, p1.Y, p2.X, p2.Y, color); }
  public static void Box(Surface dest, Point p1, Point p2, Color color, byte alpha) { Box(dest, p1.X, p1.Y, p2.X, p2.Y, color, alpha); }
  public static void Box(Surface dest, int x1, int y1, int x2, int y2, Color color)
  { Check(GFX.rectangleRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, color.A));
  }
  public static void Box(Surface dest, int x1, int y1, int x2, int y2, Color color, byte alpha)
  { Check(GFX.rectangleRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, alpha));
  }
  public static void Box(Surface dest, Rectangle rect, uint color) { Box(dest, rect.X, rect.Y, rect.X+rect.Width-1, rect.Y+rect.Height-1, color); }
  public static void Box(Surface dest, Point p1, Point p2, uint color) { Box(dest, p1.X, p1.Y, p2.X, p2.Y, color); }
  public static void Box(Surface dest, int x1, int y1, int x2, int y2, uint color)
  { Check(GFX.rectangleColor(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color));
  }

  public static void FilledBox(Surface dest, Rectangle rect, Color color) { FilledBox(dest, rect.X, rect.Y, rect.X+rect.Width-1, rect.Y+rect.Height-1, color); }
  public static void FilledBox(Surface dest, Rectangle rect, Color color, byte alpha) { FilledBox(dest, rect.X, rect.Y, rect.X+rect.Width-1, rect.Y+rect.Height-1, color, alpha); }
  public static void FilledBox(Surface dest, Point p1, Point p2, Color color) { FilledBox(dest, p1.X, p1.Y, p2.X, p2.Y, color); }
  public static void FilledBox(Surface dest, Point p1, Point p2, Color color, byte alpha) { FilledBox(dest, p1.X, p1.Y, p2.X, p2.Y, color, alpha); }
  public static void FilledBox(Surface dest, int x1, int y1, int x2, int y2, Color color)
  { Check(GFX.boxRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, color.A));
  }
  public static void FilledBox(Surface dest, int x1, int y1, int x2, int y2, Color color, byte alpha)
  { Check(GFX.boxRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color.R, color.G, color.B, alpha));
  }
  public static void FilledBox(Surface dest, Rectangle rect, uint color) { FilledBox(dest, rect.X, rect.Y, rect.X+rect.Width-1, rect.Y+rect.Height-1, color); }
  public static void FilledBox(Surface dest, Point p1, Point p2, uint color) { FilledBox(dest, p1.X, p1.Y, p2.X, p2.Y, color); }
  public static void FilledBox(Surface dest, int x1, int y1, int x2, int y2, uint color)
  { Check(GFX.boxColor(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, color));
  }
  #endregion
  
  #region Circles
  public static void Circle(Surface dest, Point pt, int radius, Color color) { Circle(dest, pt.X, pt.Y, radius, color); }
  public static void Circle(Surface dest, Point pt, int radius, Color color, byte alpha) { Circle(dest, pt.X, pt.Y, radius, color, alpha); }
  public static void Circle(Surface dest, int x, int y, int radius, Color color)
  { Check(GFX.circleRGBA(dest.surface, (short)x, (short)y, (short)radius, color.R, color.G, color.B, color.A));
  }
  public static void Circle(Surface dest, int x, int y, int radius, Color color, byte alpha)
  { Check(GFX.circleRGBA(dest.surface, (short)x, (short)y, (short)radius, color.R, color.G, color.B, alpha));
  }
  public static void Circle(Surface dest, Point pt, int radius, uint color) { Circle(dest, pt.X, pt.Y, radius, color); }
  public static void Circle(Surface dest, int x, int y, int radius, uint color)
  { Check(GFX.circleColor(dest.surface, (short)x, (short)y, (short)radius, color));
  }

  public static void CircleAA(Surface dest, Point pt, int radius, Color color) { CircleAA(dest, pt.X, pt.Y, radius, color); }
  public static void CircleAA(Surface dest, Point pt, int radius, Color color, byte alpha) { CircleAA(dest, pt.X, pt.Y, radius, color, alpha); }
  public static void CircleAA(Surface dest, int x, int y, int radius, Color color)
  { Check(GFX.aacircleRGBA(dest.surface, (short)x, (short)y, (short)radius, color.R, color.G, color.B, color.A));
  }
  public static void CircleAA(Surface dest, int x, int y, int radius, Color color, byte alpha)
  { Check(GFX.aacircleRGBA(dest.surface, (short)x, (short)y, (short)radius, color.R, color.G, color.B, alpha));
  }
  public static void CircleAA(Surface dest, Point pt, int radius, uint color) { CircleAA(dest, pt.X, pt.Y, radius, color); }
  public static void CircleAA(Surface dest, int x, int y, int radius, uint color)
  { Check(GFX.aacircleColor(dest.surface, (short)x, (short)y, (short)radius, color));
  }

  public static void FilledCircle(Surface dest, Point pt, int radius, Color color) { FilledCircle(dest, pt.X, pt.Y, radius, color); }
  public static void FilledCircle(Surface dest, Point pt, int radius, Color color, byte alpha) { FilledCircle(dest, pt.X, pt.Y, radius, color, alpha); }
  public static void FilledCircle(Surface dest, int x, int y, int radius, Color color)
  { Check(GFX.filledCircleRGBA(dest.surface, (short)x, (short)y, (short)radius, color.R, color.G, color.B, color.A));
  }
  public static void FilledCircle(Surface dest, int x, int y, int radius, Color color, byte alpha)
  { Check(GFX.filledCircleRGBA(dest.surface, (short)x, (short)y, (short)radius, color.R, color.G, color.B, alpha));
  }
  public static void FilledCircle(Surface dest, Point pt, int radius, uint color) { FilledCircle(dest, pt.X, pt.Y, radius, color); }
  public static void FilledCircle(Surface dest, int x, int y, int radius, uint color)
  { Check(GFX.filledCircleColor(dest.surface, (short)x, (short)y, (short)radius, color));
  }
  #endregion
  
  #region Ellipses
  public static void Ellipse(Surface dest, Point pt, int xRadius, int yRadius, Color color) { Ellipse(dest, pt.X, pt.Y, xRadius, yRadius, color); }
  public static void Ellipse(Surface dest, Point pt, int xRadius, int yRadius, Color color, byte alpha) { Ellipse(dest, pt.X, pt.Y, xRadius, yRadius, color, alpha); }
  public static void Ellipse(Surface dest, int x, int y, int xRadius, int yRadius, Color color)
  { Check(GFX.ellipseRGBA(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius, color.R, color.G, color.B, color.A));
  }
  public static void Ellipse(Surface dest, int x, int y, int xRadius, int yRadius, Color color, byte alpha)
  { Check(GFX.ellipseRGBA(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius, color.R, color.G, color.B, alpha));
  }
  public static void Ellipse(Surface dest, Point pt, int xRadius, int yRadius, uint color) { Ellipse(dest, pt.X, pt.Y, xRadius, yRadius, color); }
  public static void Ellipse(Surface dest, int x, int y, int xRadius, int yRadius, uint color)
  { Check(GFX.ellipseColor(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius, color));
  }

  public static void EllipseAA(Surface dest, Point pt, int xRadius, int yRadius, Color color) { EllipseAA(dest, pt.X, pt.Y, xRadius, yRadius, color); }
  public static void EllipseAA(Surface dest, Point pt, int xRadius, int yRadius, Color color, byte alpha) { EllipseAA(dest, pt.X, pt.Y, xRadius, yRadius, color, alpha); }
  public static void EllipseAA(Surface dest, int x, int y, int xRadius, int yRadius, Color color)
  { Check(GFX.aaellipseRGBA(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius, color.R, color.G, color.B, color.A));
  }
  public static void EllipseAA(Surface dest, int x, int y, int xRadius, int yRadius, Color color, byte alpha)
  { Check(GFX.aaellipseRGBA(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius, color.R, color.G, color.B, alpha));
  }
  public static void EllipseAA(Surface dest, Point pt, int xRadius, int yRadius, uint color) { EllipseAA(dest, pt.X, pt.Y, xRadius, yRadius, color); }
  public static void EllipseAA(Surface dest, int x, int y, int xRadius, int yRadius, uint color)
  { Check(GFX.aaellipseColor(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius, color));
  }

  public static void FilledEllipse(Surface dest, Point pt, int xRadius, int yRadius, Color color) { FilledEllipse(dest, pt.X, pt.Y, xRadius, yRadius, color); }
  public static void FilledEllipse(Surface dest, Point pt, int xRadius, int yRadius, Color color, byte alpha) { FilledEllipse(dest, pt.X, pt.Y, xRadius, yRadius, color, alpha); }
  public static void FilledEllipse(Surface dest, int x, int y, int xRadius, int yRadius, Color color)
  { Check(GFX.filledEllipseRGBA(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius, color.R, color.G, color.B, color.A));
  }
  public static void FilledEllipse(Surface dest, int x, int y, int xRadius, int yRadius, Color color, byte alpha)
  { Check(GFX.filledEllipseRGBA(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius, color.R, color.G, color.B, alpha));
  }
  public static void FilledEllipse(Surface dest, Point pt, int xRadius, int yRadius, uint color) { FilledEllipse(dest, pt.X, pt.Y, xRadius, yRadius, color); }
  public static void FilledEllipse(Surface dest, int x, int y, int xRadius, int yRadius, uint color)
  { Check(GFX.filledEllipseColor(dest.surface, (short)x, (short)y, (short)xRadius, (short)yRadius, color));
  }
  #endregion
  
  #region Pies
  public static void FilledPie(Surface dest, Point pt, int radius, int startDegs, int endDegs, Color color) { FilledPie(dest, pt.X, pt.Y, radius, startDegs, endDegs, color); }
  public static void FilledPie(Surface dest, Point pt, int radius, int startDegs, int endDegs, Color color, byte alpha) { FilledPie(dest, pt.X, pt.Y, radius, startDegs, endDegs, color, alpha); }
  public static void FilledPie(Surface dest, int x, int y, int radius, int startDegs, int endDegs, Color color)
  { Check(GFX.filledpieRGBA(dest.surface, (short)x, (short)y, (short)radius, (short)((startDegs-90)%360), (short)((endDegs-90)%360), color.R, color.G, color.B, color.A));
  }
  public static void FilledPie(Surface dest, int x, int y, int radius, int startDegs, int endDegs, Color color, byte alpha)
  { Check(GFX.filledpieRGBA(dest.surface, (short)x, (short)y, (short)radius, (short)((startDegs-90)%360), (short)((endDegs-90)%360), color.R, color.G, color.B, alpha));
  }
  public static void FilledPie(Surface dest, Point pt, int radius, int startDegs, int endDegs, uint color) { FilledPie(dest, pt.X, pt.Y, radius, startDegs, endDegs, color); }
  public static void FilledPie(Surface dest, int x, int y, int radius, int startDegs, int endDegs, uint color)
  { Check(GFX.filledpieColor(dest.surface, (short)x, (short)y, (short)radius, (short)((startDegs-90)%360), (short)((endDegs-90)%360), color));
  }
  #endregion

  #region Triangles
  public static void Triangle(Surface dest, Point p1, Point p2, Point p3, Color color) { Triangle(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color); }
  public static void Triangle(Surface dest, Point p1, Point p2, Point p3, Color color, byte alpha) { Triangle(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color, alpha); }
  public static void Triangle(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, Color color)
  { Check(GFX.trigonRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3, color.R, color.G, color.B, color.A));
  }
  public static void Triangle(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, Color color, byte alpha)
  { Check(GFX.trigonRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3, color.R, color.G, color.B, alpha));
  }
  public static void Triangle(Surface dest, Point p1, Point p2, Point p3, uint color) { Triangle(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color); }
  public static void Triangle(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, uint color)
  { Check(GFX.trigonColor(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3, color));
  }

  public static void TriangleAA(Surface dest, Point p1, Point p2, Point p3, Color color) { TriangleAA(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color); }
  public static void TriangleAA(Surface dest, Point p1, Point p2, Point p3, Color color, byte alpha) { TriangleAA(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color, alpha); }
  public static void TriangleAA(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, Color color)
  { Check(GFX.aatrigonRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3, color.R, color.G, color.B, color.A));
  }
  public static void TriangleAA(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, Color color, byte alpha)
  { Check(GFX.aatrigonRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3, color.R, color.G, color.B, alpha));
  }
  public static void TriangleAA(Surface dest, Point p1, Point p2, Point p3, uint color) { TriangleAA(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color); }
  public static void TriangleAA(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, uint color)
  { Check(GFX.aatrigonColor(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3, color));
  }

  public static void FilledTriangle(Surface dest, Point p1, Point p2, Point p3, Color color) { FilledTriangle(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color); }
  public static void FilledTriangle(Surface dest, Point p1, Point p2, Point p3, Color color, byte alpha) { FilledTriangle(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color, alpha); }
  public static void FilledTriangle(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, Color color)
  { Check(GFX.filledTrigonRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3, color.R, color.G, color.B, color.A));
  }
  public static void FilledTriangle(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, Color color, byte alpha)
  { Check(GFX.filledTrigonRGBA(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3, color.R, color.G, color.B, alpha));
  }
  public static void FilledTriangle(Surface dest, Point p1, Point p2, Point p3, uint color) { FilledTriangle(dest, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, color); }
  public static void FilledTriangle(Surface dest, int x1, int y1, int x2, int y2, int x3, int y3, uint color)
  { Check(GFX.filledTrigonColor(dest.surface, (short)x1, (short)y1, (short)x2, (short)y2, (short)x3, (short)y3, color));
  }
  #endregion  
  
  #region Polygons
  public static void Polygon(Surface dest, Point[] points, Color color, byte alpha)
  { Polygon(dest, points, Color.FromArgb(alpha, color));
  }
  public static void Polygon(Surface dest, Point[] points, Color color)
  { int len=points.Length;
    short *vx = stackalloc short[len];
    short *vy = stackalloc short[len];
    fixed(Point* pt = points)
      for(int i=0; i<len; i++) { vx[i]=(short)pt[i].X; vy[i]=(short)pt[i].Y; }
    Check(GFX.polygonRGBA(dest.surface, vx, vy, len, color.R, color.G, color.B, color.A));
  }
  public static void Polygon(Surface dest, Point[] points, uint color)
  { int len=points.Length;
    short *vx = stackalloc short[len];
    short *vy = stackalloc short[len];
    fixed(Point* pt = points)
      for(int i=0; i<len; i++) { vx[i]=(short)pt[i].X; vy[i]=(short)pt[i].Y; }
    Check(GFX.polygonColor(dest.surface, vx, vy, len, color));
  }
  
  public static void PolygonAA(Surface dest, Point[] points, Color color, byte alpha)
  { PolygonAA(dest, points, Color.FromArgb(alpha, color));
  }
  public static void PolygonAA(Surface dest, Point[] points, Color color)
  { int len=points.Length;
    short *vx = stackalloc short[len];
    short *vy = stackalloc short[len];
    fixed(Point* pt = points)
      for(int i=0; i<len; i++) { vx[i]=(short)pt[i].X; vy[i]=(short)pt[i].Y; }
    Check(GFX.aapolygonRGBA(dest.surface, vx, vy, len, color.R, color.G, color.B, color.A));
  }
  public static void PolygonAA(Surface dest, Point[] points, uint color)
  { int len=points.Length;
    short *vx = stackalloc short[len];
    short *vy = stackalloc short[len];
    fixed(Point* pt = points)
      for(int i=0; i<len; i++) { vx[i]=(short)pt[i].X; vy[i]=(short)pt[i].Y; }
    Check(GFX.aapolygonColor(dest.surface, vx, vy, len, color));
  }
  
  public static void FilledPolygon(Surface dest, Point[] points, Color color, byte alpha)
  { FilledPolygon(dest, points, Color.FromArgb(alpha, color));
  }
  public static void FilledPolygon(Surface dest, Point[] points, Color color)
  { int len=points.Length;
    short *vx = stackalloc short[len];
    short *vy = stackalloc short[len];
    fixed(Point* pt = points)
      for(int i=0; i<len; i++) { vx[i]=(short)pt[i].X; vy[i]=(short)pt[i].Y; }
    Check(GFX.filledPolygonRGBA(dest.surface, vx, vy, len, color.R, color.G, color.B, color.A));
  }
  public static void FilledPolygon(Surface dest, Point[] points, uint color)
  { int len=points.Length;
    short *vx = stackalloc short[len];
    short *vy = stackalloc short[len];
    fixed(Point* pt = points)
      for(int i=0; i<len; i++) { vx[i]=(short)pt[i].X; vy[i]=(short)pt[i].Y; }
    Check(GFX.filledPolygonColor(dest.surface, vx, vy, len, color));
  }
  #endregion
  
  #region Curves
  public static void Bezier(Surface dest, Point[] points, int steps, Color color, byte alpha)
  { Bezier(dest, points, steps, Color.FromArgb(alpha, color));
  }
  public static void Bezier(Surface dest, Point[] points, int steps, Color color)
  { int len=points.Length;
    short *vx = stackalloc short[len];
    short *vy = stackalloc short[len];
    fixed(Point* pt = points)
      for(int i=0; i<len; i++) { vx[i]=(short)pt[i].X; vy[i]=(short)pt[i].Y; }
    Check(GFX.bezierRGBA(dest.surface, vx, vy, len, steps, color.R, color.G, color.B, color.A));
  }
  public static void Bezier(Surface dest, Point[] points, int steps, uint color)
  { int len=points.Length;
    short *vx = stackalloc short[len];
    short *vy = stackalloc short[len];
    fixed(Point* pt = points)
      for(int i=0; i<len; i++) { vx[i]=(short)pt[i].X; vy[i]=(short)pt[i].Y; }
    Check(GFX.bezierColor(dest.surface, vx, vy, len, steps, color));
  }
  #endregion

  static void Check(int result)
  { if(result!=0) throw new VideoException("An error occurred during a graphics primitive call");
  }
}

}