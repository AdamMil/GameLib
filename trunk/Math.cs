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

namespace GameLib.Mathematics
{

// TODO: once generics become available, implement these in a type-generic fashion
// FIXME: think about how to handle rotation! (degrees, radians, 0-255, ?????)
#region 2D
namespace TwoD
{

public struct Point
{ public Point(float x, float y) { X=x; Y=y; }

  public bool Valid { get { return !float.IsNaN(X) && !float.IsNaN(Y); } }

  public static Point Invalid { get { return new Point(float.NaN, float.NaN); } }
  public static Vector operator-(Point lhs, Point rhs) { return new Vector(lhs.X-rhs.X, lhs.Y-rhs.Y); }

  public float X, Y;
}

public struct Vector
{ public Vector(float x, float y) { X=x; Y=y; }
  
  public Vector CrossVector { get { return new Vector(-Y, X); } }
  public float  Length    { get { return (float)System.Math.Sqrt(X*X+Y*Y); } }
  public float  LengthSqr { get { return X*X+Y*Y; } }
  public Vector Normal { get { return this/Length; } }
  
  public float DotProduct(Vector v) { return X*v.X + Y*v.Y; }
  public void  Normalize() { Assign(Normal); }

  public void Rotate(float angle) { Assign(Rotated(angle)); }
  public Vector Rotated(float angle)
  { float sin = (float)Math.Sin(angle), cos = (float)Math.Cos(angle);
    return new Vector(X*cos-Y*sin, X*sin+Y*cos);
  }
  
  public static Vector operator-(Vector v) { return new Vector(-v.X, -v.Y); }
  public static Vector operator+(Vector a, Vector b) { return new Vector(a.X+b.X, a.Y+b.Y); }
  public static Vector operator-(Vector a, Vector b) { return new Vector(a.X-b.X, a.Y-b.Y); }
  public static Vector operator*(Vector a, Vector b) { return new Vector(a.X*b.X, a.Y*b.Y); }
  public static Vector operator/(Vector a, Vector b) { return new Vector(a.X/b.X, a.Y/b.Y); }
  public static Vector operator*(Vector v, float f)   { return new Vector(v.X*f, v.Y*f); }
  public static Vector operator/(Vector v, float f)   { return new Vector(v.X/f, v.Y/f); }
  
  public float X, Y;
  
  void Assign(Vector v) { X=v.X; Y=v.Y; }
}

#region Line
public struct Line
{ public Line(float x, float y, float xd, float yd) { Start=new Point(x, y); Vector=new Vector(xd, yd); }
  public Line(Point start, Vector vector) { Start=start; Vector=vector; }

  public Point End { get { return new Point(Start.X+Vector.X, Start.Y+Vector.Y); } }
  public float Length { get { return Vector.Length; } }
  public float LengthSqr { get { return Vector.LengthSqr; } }

  public Point Intersection(Line segment)
  { Point p2 = End, p4 = segment.End;
    float d = (p4.Y-segment.Start.Y)*(p2.X-Start.X) - (p4.X-segment.Start.X)*(p2.Y-Start.Y), ua, ub;
    if(d==0) return Point.Invalid;
    ua = ((p4.X-segment.Start.X)*(Start.Y-segment.Start.Y) - (p4.Y-segment.Start.Y)*(Start.X-segment.Start.X)) / d;
    if(ua<0 || ua>1) return Point.Invalid;
    ub = ((p2.X-Start.X)*(Start.Y-segment.Start.Y) - (p2.Y-Start.Y)*(Start.X-segment.Start.X)) / d;
    if(ub<0 || ub>1) return Point.Invalid;
    return new Point(Start.X + Vector.X*ua, Start.Y + Vector.Y*ua);
  }
  public bool Intersects(Line segment) { return Intersection(segment).Valid; }

  public Point LineIntersection(Line line)
  { Point p2 = End, p4 = line.End;
    float d = (p4.Y-line.Start.Y)*(p2.X-Start.X) - (p4.X-line.Start.X)*(p2.Y-Start.Y);
    if(d==0) return Point.Invalid;
    d = ((p4.X-line.Start.X)*(Start.Y-line.Start.Y) - (p4.Y-line.Start.Y)*(Start.X-line.Start.X)) / d;
    return new Point(Start.X + Vector.X*d, Start.Y + Vector.Y*d);
  }
  public bool LineIntersects(Line line) { return Intersection(line).Valid; }

  public static Line FromPoints(Point start, Point end) { return new Line(start, end-start); }
  public static Line FromPoints(float x1, float y1, float x2, float y2) { return new Line(x1, y1, x2-x1, y2-y1); }

  public Point  Start;
  public Vector Vector;
}
#endregion

public struct Circle
{ public Circle(float centerX, float centerY, float radius) { Center=new Point(centerX, centerY); Radius=radius; }
  public Circle(Point center, float radius) { Center=center; Radius=radius; }

  public float Area { get { return (float)(Radius*Radius*Math.PI); } }

  public bool Contains(Point point) { return (point-Center).Length <= Radius; }

  public Point Center;
  public float Radius;
}

public struct Corner
{ public Line Line0 { get { return new Line(Point, Edge0); } }
  public Line Line1 { get { return new Line(Point, Edge1); } }
  public Line this[int edge]
  { get
    { if(edge<0 || edge>1) throw new ArgumentOutOfRangeException("Corner[]", edge, "must be 0 or 1");
      return new Line(Point, edge==0 ? Edge0 : Edge1);
    }
  }

  public float CrossZ { get { return (Point.X-Edge0.X)*(Edge1.Y-Point.Y) - (Point.Y-Edge0.Y)*(Edge1.X-Point.X); } }

  public Point Point;
  public Vector Edge0, Edge1;
}

#region Polygon
public sealed class Polygon
{ public Polygon() { points=new Point[4]; }
  public Polygon(Point[] points) { this.points=new Point[4]; AddPoints(points); }

  public Point this[int index] { get { return points[index]; } }
  public int Length { get { return length; } }
  public Point[] Points { get { return points; } }

  public int AddPoint(Point point)
  { if(length==points.Length) ResizeTo(length+1);
    points[length] = point;
    return length++;
  }

  public void AddPoints(Point[] points)
  { ResizeTo(length+points.Length);
    for(int i=0; i<points.Length; i++) this.points[length++] = points[i];
  }

  public Corner GetCorner(int index)
  { if(length<3) throw new InvalidOperationException("Not a valid polygon [not enough points]!");
    Corner c = new Corner();
    c.Point = points[index];
    c.Edge0 = GetPoint(index-1) - c.Point;
    c.Edge1 = GetPoint(index+1) - c.Point;
    return c;
  }

  public Point GetPoint(int index)
  { return index<0 ? points[length+index] : index>=length ? points[index-length] : points[index];
  }

  public void InsertPoint(Point point, int index)
  { if(length==points.Length) ResizeTo(length+1);
    if(index<length) for(int i=length; i>index; i--) points[i] = points[i-1];
    points[index] = point;
    length++;
  }

  public bool IsClockwise()
  { bool neg=false, pos=false;
    for(int i=0; i<length; i++) 
    { float z = GetCorner(i).CrossZ;
      if(z<0)
      { if(pos) throw new InvalidOperationException("Not a simple, convex polygon!");
        neg=true;
      }
      else if(z>0)
      { if(neg) throw new InvalidOperationException("Not a simple, convex polygon!");
        pos=true;
      }
    }
    return pos;
  }

  public bool IsConvex()
  { bool neg=false, pos=false;
    for(int i=0; i<length; i++) 
    { float z = GetCorner(i).CrossZ;
      if(z<0)
      { if(pos) return false;
        neg=true;
      }
      else if(z>0)
      { if(neg) return false;
        pos=true;
      }
    }
    return true;
  }

  public void RemovePoint(int index)
  { if(index != --length) for(int i=index; i<length; i++) points[i]=points[i+1];
  }
  
  public Polygon[] SplitIntoConvexPolygons()
  {
  }
  
  void ResizeTo(int capacity) { points=ResizeTo(points, capacity); }
  void ResizeTo(Point[] array, int capacity)
  { if(array.Length<capacity)
    { Point[] narr = new Point[Math.Max(capacity, points.Length*2)];
      Array.Copy(points, narr, length);
      return narr;
    }
    return array;
  }

  Point[] points;
  int length;
}
#endregion

} // namespace TwoD
#endregion

#region 3D
namespace ThreeD
{

public struct Point
{ public float X, Y, Z;
}

#region Vector
public struct Vector
{ public Vector(float x, float y, float z) { X=x; Y=y; Z=z; }
  
  public float  Length    { get { return (float)System.Math.Sqrt(X*X+Y*Y+Z*Z); } }
  public float  LengthSqr { get { return X*X+Y*Y+Z*Z; } }
  public Vector Normal { get { return this/Length; } }
  
  public Vector CrossProduct(Vector v) { return new Vector(X*v.Z-Z*v.Y, Z*v.X-X*v.Z, X*v.Y-Y*v.X); }
  public float  DotProduct(Vector v) { return X*v.X + Y*v.Y + Z*v.Z; }
  public void Normalize() { Assign(Normal); }

  public void Rotate(float xangle, float yangle, float zangle) { Assign(Rotated(xangle, yangle, zangle)); }
  public void RotateX(float angle) { Assign(RotatedX(angle)); } 
  public void RotateY(float angle) { Assign(RotatedY(angle)); } 
  public void RotateZ(float angle) { Assign(RotatedZ(angle)); } 
  
  public Vector Rotated(float xangle, float yangle, float zangles)
  { return RotatedX(xangle).RotatedY(xangle).RotatedZ(xangle);
  }
  public Vector RotatedX(float angle)
  { float sin = (float)Math.Sin(angle), cos = (float)Math.Cos(angle);
    return new Vector(X, Y*cos-Z*sin, Y*sin+Z*cos);
  }
  public Vector RotatedY(float angle)
  { float sin = (float)Math.Sin(angle), cos = (float)Math.Cos(angle);
    return new Vector(Z*sin+X*cos, Y, Z*cos-X*sin);
  }
  public Vector RotatedZ(float angle)
  { float sin = (float)Math.Sin(angle), cos = (float)Math.Cos(angle);
    return new Vector(X*cos-Y*sin, X*sin+Y*cos, Z);
  }
  
  public static Vector operator-(Vector v) { return new Vector(-v.X, -v.Y, -v.Z); }
  public static Vector operator+(Vector a, Vector b) { return new Vector(a.X+b.X, a.Y+b.Y, a.Z+b.Z); }
  public static Vector operator-(Vector a, Vector b) { return new Vector(a.X-b.X, a.Y-b.Y, a.Z-b.Z); }
  public static Vector operator*(Vector a, Vector b) { return new Vector(a.X*b.X, a.Y*b.Y, a.Z*b.Z); }
  public static Vector operator/(Vector a, Vector b) { return new Vector(a.X/b.X, a.Y/b.Y, a.Z/b.Z); }
  public static Vector operator*(Vector v, float f)   { return new Vector(v.X*f, v.Y*f, v.Z*f); }
  public static Vector operator/(Vector v, float f)   { return new Vector(v.X/f, v.Y/f, v.Z/f); }
  
  public float X, Y, Z;
  
  void Assign(Vector v) { X=v.X; Y=v.Y; Z=v.Z; }
}
#endregion

public struct Plane
{ public Point  Point;
  public Vector Normal;
}

public struct Line
{ public Point Start, End;
}

public struct ParaLine
{ public Point Point;
  public Vector Vector;
}

public struct Sphere
{ public Point Center;
  public float Radius;
}

} // namespace ThreeD
#endregion

} // namespace GameLib.Mathematics