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
#region 2D math
namespace TwoD
{

#region Vector
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
  
  public override string ToString() { return string.Format("[{0:f},{1:f}]", X, Y); }

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
#endregion

#region Point
public struct Point
{ public Point(float x, float y) { X=x; Y=y; }

  public bool Valid { get { return !float.IsNaN(X); } }

  public float DistanceTo(Point point)
  { float xd=point.X-X, yd=point.Y-Y;
    return (float)Math.Sqrt(xd*xd+yd*yd);
  }
  public float DistanceSqrTo(Point point)
  { float xd=point.X-X, yd=point.Y-Y;
    return xd*xd+yd*yd;
  }

  public void Offset(float xd, float yd) { X+=xd; Y+=yd; }

  public System.Drawing.Point ToPoint() { return new System.Drawing.Point((int)Math.Round(X), (int)Math.Round(Y)); }
  public override string ToString() { return string.Format("({0:f},{1:f})", X, Y); }

  public static Point Invalid { get { return new Point(float.NaN, float.NaN); } }
  public static Vector operator-(Point lhs, Point rhs)  { return new Vector(lhs.X-rhs.X, lhs.Y-rhs.Y); }
  public static Point  operator-(Point lhs, Vector rhs) { return new Point(lhs.X-rhs.X, lhs.Y-rhs.Y); }
  public static Point  operator+(Point lhs, Vector rhs) { return new Point(lhs.X+rhs.X, lhs.Y+rhs.Y); }
  public static implicit operator Point(System.Drawing.Point point) { return new Point(point.X, point.Y); }

  public float X, Y;
}
#endregion

#region Line
public struct LineIntersectInfo
{ public LineIntersectInfo(Point point, bool onFirst, bool onSecond)
  { Point=point; OnFirst=onFirst; OnSecond=onSecond;
  }
  public Point Point;
  public bool  OnFirst, OnSecond;
}

public struct Line
{ public Line(float x, float y, float xd, float yd) { Start=new Point(x, y); Vector=new Vector(xd, yd); }
  public Line(Point start, Vector vector) { Start=start; Vector=vector; }

  public Point End { get { return Start+Vector; } }
  public float Length { get { return Vector.Length; } }
  public float LengthSqr { get { return Vector.LengthSqr; } }

  public float DistanceTo(Point point) { return Vector.CrossVector.Normal.DotProduct(point-Start); }

  public Point GetPoint(int point)
  { if(point<0 || point>1) throw new ArgumentOutOfRangeException("point", point, "must be 0 or 1");
    return point==0 ? Start : End;
  }

  public LineIntersectInfo GetIntersection(Line line)
  { Point p2 = End, p4 = line.End;
    float d = (p4.Y-line.Start.Y)*(p2.X-Start.X) - (p4.X-line.Start.X)*(p2.Y-Start.Y), ua, ub;
    if(d==0) return new LineIntersectInfo(Point.Invalid, false, false);
    ua = ((p4.X-line.Start.X)*(Start.Y-line.Start.Y) - (p4.Y-line.Start.Y)*(Start.X-line.Start.X)) / d;
    ub = ((p2.X-Start.X)*(Start.Y-line.Start.Y) - (p2.Y-Start.Y)*(Start.X-line.Start.X)) / d;
    return new LineIntersectInfo(new Point(Start.X + Vector.X*ua, Start.Y + Vector.Y*ua), ua>=0&&ua<=1, ub>=0&&ub<=1);
  }

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

  public float WhichSide(Point point) { return Vector.CrossVector.DotProduct(point-Start); }

  public override string ToString() { return string.Format("{0}->{1}", Start, Vector); }

  public static Line FromPoints(Point start, Point end) { return new Line(start, end-start); }
  public static Line FromPoints(float x1, float y1, float x2, float y2) { return new Line(x1, y1, x2-x1, y2-y1); }

  public Point  Start;
  public Vector Vector;
}
#endregion

#region Circle
public struct Circle
{ public Circle(float centerX, float centerY, float radius) { Center=new Point(centerX, centerY); Radius=radius; }
  public Circle(Point center, float radius) { Center=center; Radius=radius; }

  public float Area { get { return (float)(Radius*Radius*Math.PI); } }

  public bool Contains(Point point) { return (point-Center).Length <= Radius; }

  public Point Center;
  public float Radius;
}
#endregion

#region Corner
public struct Corner
{ public Line Edge0 { get { return new Line(Point+Vector0, -Vector0); } }
  public Line Edge1 { get { return new Line(Point, Vector1); } }

  public float CrossZ
  { get
    { Point p0 = GetPoint(-1), p2 = GetPoint(1);
      return (Point.X-p0.X)*(p2.Y-Point.Y) - (Point.Y-p0.Y)*(p2.X-Point.X);
    }
  }

  public Line GetEdge(int edge)
  { if(edge<0 || edge>1) throw new ArgumentOutOfRangeException("GetEdge", edge, "must be 0 or 1");
    return edge==0 ? Edge0 : Edge1;
  }

  public Point GetPoint(int point)
  { if(point<-1 || point>1) throw new ArgumentOutOfRangeException("GetPoint", point, "must be from -1 to 1");
    return point==0 ? Point : (point==-1 ? Point+Vector0 : Point+Vector1);
  }

  public Point Point;
  public Vector Vector0, Vector1;
}
#endregion

#region Polygon
public sealed class Polygon
{ public Polygon() { points=new Point[4]; }
  public Polygon(Point[] points) : this(points.Length) { AddPoints(points); }
  public Polygon(Point[] points, int nPoints) : this(nPoints) { AddPoints(points, nPoints); }
  public Polygon(int capacity)
  { if(capacity<0) throw new ArgumentOutOfRangeException("capacity", capacity, "must not be negative");
    int size=4;
    while(size<capacity) size*=2;
    points = new Point[size];
  }

  public Point this[int index]
  { get
    { if(index<0 || index>=length) throw new ArgumentOutOfRangeException();
      return points[index];
    }
    set
    { if(index<0 || index>=length) throw new ArgumentOutOfRangeException();
      points[index]=value;
    }
  }
  public int Length { get { return length; } }

  public int AddPoint(float x, float y) { return AddPoint(new Point(x, y)); }
  public int AddPoint(Point point)
  { if(length==points.Length) ResizeTo(length+1);
    points[length] = point;
    return length++;
  }

  public void AddPoints(Point[] points) { AddPoints(points, points.Length); }
  public void AddPoints(Point[] points, int nPoints)
  { ResizeTo(length+nPoints);
    for(int i=0; i<nPoints; i++) this.points[length++] = points[i];
  }

  public void Clear() { length=0; }

  public bool ConvexContains(Point point)
  { int  sgn;
    bool pos=false, neg=false;
    for(int i=0; i<length; i++)
    { sgn = Math.Sign(GetEdge(i).WhichSide(point));
      if(sgn==-1) { if(pos) return false; neg=true; }
      else if(sgn==1) { if(neg) return false; pos=true; }
      else return false;
    }
    return true;
  }

  public Corner GetCorner(int index)
  { if(length<3) throw new InvalidOperationException("Not a valid polygon [not enough points]!");
    Corner c = new Corner();
    c.Point = this[index];
    c.Vector0 = GetPoint(index-1) - c.Point;
    c.Vector1 = GetPoint(index+1) - c.Point;
    return c;
  }

  public Line GetEdge(int index)
  { if(length<2) throw new InvalidOperationException("Polygon has no edges [not enough points]!");
    return Line.FromPoints(this[index], GetPoint(index+1));
  }

  public Point GetPoint(int index)
  { return index<0 ? this[length+index] : index>=length ? this[index-length] : this[index];
  }

  public void InsertPoint(Point point, int index)
  { if(length==points.Length) ResizeTo(length+1);
    if(index<length) for(int i=length; i>index; i--) points[i] = points[i-1];
    length++;
    this[index] = point;
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
  { if(index<0 || index>=length) throw new ArgumentOutOfRangeException("index");
    if(index != --length) for(int i=index; i<length; i++) points[i]=points[i+1];
  }
  
  public void RemoveRange(int start, int length)
  { if(length==0) return;
    int end = start+length;
    if(start<0 || end<0 || end>this.length || start>=this.length) throw new ArgumentOutOfRangeException();
    for(; end<this.length; end++) points[end-length]=points[end];
    this.length -= length;
  }

  public void Reverse()
  { Polygon newPoly = new Polygon(length);
    for(int i=length-1; i>=0; i--) newPoly.AddPoint(points[i]);
    this.points = newPoly.points;
  }

  public Polygon Reversed()
  { Polygon newPoly = new Polygon(length);
    for(int i=length-1; i>=0; i--) newPoly.AddPoint(points[i]);
    return newPoly;
  }

  public Polygon[] SplitIntoConvexPolygons()
  { Polygon[] test = new Polygon[4], done = new Polygon[4];
    int tlen=1, dlen=0;

    test[0] = new Polygon(points, length);
    do
    { for(int pi=0,len=tlen; pi<len; pi++)
      { Polygon poly = test[pi];

        if(--tlen>0) // remove the current polygon
        { test[pi] = test[tlen];
          if(tlen<len) { pi--; len--; }
        }

        if(poly.length<3) continue;
        // remove corners with coincident/parallel edges.
        for(int ci=poly.length-2; ci>=1; ci--) if(poly.GetCorner(ci).CrossZ==0) poly.RemovePoint(ci);
        if(poly.length<3) continue;

        int sgn = Math.Sign(poly.GetCorner(0).CrossZ);
        for(int ci=1; ci<poly.length; ci++)
        { Corner c = poly.GetCorner(ci);
          // if the sign is different, then the polygon is not convex, and splitting at this corner will result in
          // a simplification
          if(Math.Sign(c.CrossZ) != sgn)
          { float dist = float.MaxValue, d, d2;
            Point splitPoint=new Point();
            int   splitEdge=-1, extPoint=-1, ept;
            for(int ei=0; ei<2; ei++) // try to extend each of the edges that make up this corner
            { Line toExtend = c.GetEdge(ei);
              for(int sei=0; sei<poly.length; sei++) // test the edge with the intersection of every other edge
              { if(sei==ci || sei==ci-1) continue; // except, don't compare against the edges that comprise the corner
                LineIntersectInfo lint = toExtend.GetIntersection(poly.GetEdge(sei));
                // we don't want any points that are on the edge being extended (because it wouldn't be an extension)
                // and we want to make sure the other point is actually on the line segment
                if(!lint.Point.Valid || lint.OnFirst || !lint.OnSecond) continue;
                ept = 0;
                d  = lint.Point.DistanceSqrTo(toExtend.GetPoint(0)); // find the shortest cut
                d2 = lint.Point.DistanceSqrTo(toExtend.GetPoint(1));
                if(d2<d)   { d=d2; ept=1; } // 'ept' references which point gets moved to do the extension
                if(d<dist) { dist=d; splitEdge=sei; extPoint=ept; splitPoint=lint.Point; }
              }
              if(splitEdge!=-1) // if we could split it with this edge, do it. don't bother trying the other edge
              { poly.InsertPoint(splitPoint, ++splitEdge); // insert the split point
                Polygon new1 = new Polygon(), new2 = new Polygon();
                int extended = poly.Clip(ci-1+ei+extPoint), other=poly.Clip(extended+(extPoint==0 ? 1 : -1));
                int npi = splitEdge;
                if(extended>=splitEdge) { extended++; other++; }
                // 'extended' is the point that was extended. 'other' is the other side of the edge being extended
                do // circle around the polygon, starting at the new point, adding points until we hit 'extended'
                { new1.AddPoint(poly.points[npi]);
                  // if this polygon contains the edge being extended, then it must not contain the point being extended
                  if(npi==other) other=-1;
                  if(++npi>=poly.length) npi-=poly.length;
                } while(npi != extended);
                if(other!=-1) new1.AddPoint(poly.points[npi++]); // add the extended point to the appropriate polygon
                do // continue circling, adding points to the other polygon, and end by adding the split point again
                { if(npi>=poly.length) npi-=poly.length;
                  new2.AddPoint(poly.points[npi]);
                } while(npi++ != splitEdge);
                test = AddPoly(new1, test, tlen++); // add the two polygons
                test = AddPoly(new2, test, tlen++);
                goto outer; // and continue the main loop
              }
            }
            if(splitEdge==-1) // if no split points could be found, give up
              throw new NotSupportedException("Unable to split polygon. This must not be a simple polygon.");
          }
        }
        done = AddPoly(poly, done, dlen++); // all the signs are the same, it's convex, so add it to the 'done' list
        outer:;
      }
    } while(tlen>0);
    if(dlen==done.Length) return done; // return an array of the proper size
    else
    { Polygon[] narr = new Polygon[dlen];
      Array.Copy(done, narr, dlen);
      return narr;
    }
  }
  
  int Clip(int index)
  { if(index<0) index += length;
    else if(index>=length) index -= length;
    return index;
  }

  void ResizeTo(int capacity)
  { if(points.Length<capacity)
    { Point[] narr = new Point[Math.Max(capacity, points.Length*2)];
      Array.Copy(points, narr, length);
      points = narr;
    }
  }

  static Polygon[] AddPoly(Polygon poly, Polygon[] array, int index)
  { if(index>=array.Length)
    { Polygon[] narr = new Polygon[array.Length*2];
      Array.Copy(array, narr, array.Length);
      array=narr;
    }
    array[index] = poly;
    return array;
  }

  Point[] points;
  int length;
}
#endregion

} // namespace TwoD
#endregion

#region 3D math
namespace ThreeD
{

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
  
  public override string ToString() { return string.Format("[{0:f},{1:f},{2:f}]", X, Y, Z); }

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

#region Point
public struct Point
{ public Point(float x, float y, float z) { X=x; Y=y; Z=z; }

  public float DistanceTo(Point point)
  { float xd=point.X-X, yd=point.Y-Y, zd=point.Z-Z;
    return (float)Math.Sqrt(xd*xd+yd*yd+zd*zd);
  }
  public float DistanceSqrTo(Point point)
  { float xd=point.X-X, yd=point.Y-Y, zd=point.Z-Z;
    return xd*xd+yd*yd+zd*zd;
  }

  public void Offset(float xd, float yd, float zd) { X+=xd; Y+=yd; Z+=zd; }

  public override string ToString() { return string.Format("({0:f},{1:f},{2:f})", X, Y, Z); }

  public static Vector operator-(Point lhs, Point rhs)  { return new Vector(lhs.X-rhs.X, lhs.Y-rhs.Y, lhs.Z-rhs.Z); }
  public static Point  operator-(Point lhs, Vector rhs) { return new Point(lhs.X-rhs.X, lhs.Y-rhs.Y, lhs.Z-rhs.Z); }
  public static Point  operator+(Point lhs, Vector rhs) { return new Point(lhs.X+rhs.X, lhs.Y+rhs.Y, lhs.Z+rhs.Z); }
  
  public float X, Y, Z;
}
#endregion

#region Line
public struct Line
{ public Line(float x, float y, float z, float xd, float yd, float zd) { Start=new Point(x, y, z); Vector=new Vector(xd, yd, zd); }
  public Line(Point start, Vector vector) { Start=start; Vector=vector; }

  public Point End { get { return Start+Vector; } }
  public float Length { get { return Vector.Length; } }
  public float LengthSqr { get { return Vector.LengthSqr; } }

  public Point GetPoint(int point)
  { if(point<0 || point>1) throw new ArgumentOutOfRangeException("point", point, "must be 0 or 1");
    return point==0 ? Start : End;
  }

  public override string ToString() { return string.Format("{0}->{1}", Start, Vector); }

  public static Line FromPoints(Point start, Point end) { return new Line(start, end-start); }
  public static Line FromPoints(float x1, float y1, float z1, float x2, float y2, float z2)
  { return new Line(x1, y1, z1, x2-x1, y2-y1, z2-z1);
  }

  public Point  Start;
  public Vector Vector;
}
#endregion

public struct Plane
{ public Point  Point;
  public Vector Normal;
}

public struct Sphere
{ public Point Center;
  public float Radius;
}

} // namespace ThreeD
#endregion

} // namespace GameLib.Mathematics