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

public class MathConst
{ MathConst() { }

  public const double PI = 3.1415926535897932384626433832795;
  public const double PI2Div256 = 0.0245436926061702596754894014318711; // 2 * PI / 256
  public const double RadsTo256 = 40.74366543152520595683424342337; // multiply a radian by this to get a number from 0-255
  public const double DegsTo256 = 0.711111111111111111111111111112; // multiply a degree by this to get a number from 0-255
}

public class SineTable
{ SineTable() { }
  static SineTable() { for(int i=0; i<256; i++) Sines[i]=System.Math.Sin(MathConst.PI2Div256*i); }
  
  public static double Sin(int angle) { return Sines[angle&0xFF]; }
  public static double Cos(int angle) { return Sines[(angle+64)&0xFF]; }
  public static readonly double[] Sines = new double[256];
}

// TODO: once generics become available, implement these in a type-generic fashion
// FIXME: think about how to handle rotation! (degrees, radians, 0-255, ?????)
#region 2D
namespace TwoD
{

public struct Point
{ public float X, Y;
}

public struct Vector
{ public Vector(float x, float y) { X=x; Y=y; }
  
  public Vector CrossVector { get { return new Vector(-Y, X); } }
  public float  Length    { get { return (float)System.Math.Sqrt(X*X+Y*Y); } }
  public float  LengthSqr { get { return X*X+Y*Y; } }
  public Vector Normal { get { return this/Length; } }
  
  public float DotProduct(Vector v) { return X*v.X + Y*v.Y; }
  public void  Normalize() { Assign(Normal); }

  public void Rotate(int angle) { Assign(Rotated(angle)); }
  public Vector Rotated(int angle)
  { float sin = (float)SineTable.Sin(angle), cos = (float)SineTable.Cos(angle);
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

public struct Line
{ public Point Start, End;
}

public struct ParaLine
{ public Point  Point;
  public Vector Vector;
}

public struct Circle
{ public Point Center;
  public float Radius;
}

public struct Polygon
{ public Point[] Points;
}

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

  public void Rotate(int xa, int ya, int za) { Assign(Rotated(xa, ya, za)); }
  public void RotateX(int a) { Assign(RotatedX(a)); } 
  public void RotateY(int a) { Assign(RotatedY(a)); } 
  public void RotateZ(int a) { Assign(RotatedZ(a)); } 
  
  public Vector Rotated(int xa, int ya, int za) { return RotatedX(xa).RotatedY(ya).RotatedZ(za); }
  public Vector RotatedX(int a)
  { float sin = (float)SineTable.Sin(a), cos = (float)SineTable.Cos(a);
    return new Vector(X, Y*cos-Z*sin, Y*sin+Z*cos);
  }
  public Vector RotatedY(int a)
  { float sin = (float)SineTable.Sin(a), cos = (float)SineTable.Cos(a);
    return new Vector(Z*sin+X*cos, Y, Z*cos-X*sin);
  }
  public Vector RotatedZ(int a)
  { float sin = (float)SineTable.Sin(a), cos = (float)SineTable.Cos(a);
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